using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Conductor.Core.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Conductor.Core.Models
{
    public abstract class JobBase<TDef> : IJob where TDef : JobDefinition
    {
        private readonly JobDefinition _def;
        private readonly IJobService _jobs;
        private readonly ILockFactory _lockFactory;
        private readonly IACIService _aciService;
        protected readonly ILogger Logger;
        public string Name { get { return _def.Name; } }
        public JobBase(JobDefinition def, IJobService jobService, ILockFactory lockFactory, IACIService aciService, ILogger logger)
        {
            _def = def;
            _jobs = jobService;
            _lockFactory = lockFactory;
            _aciService = aciService;
            Logger = logger;
        }

        protected virtual async Task ExecuteJobAsync(JobDefinition def)
        {
            var @lock = _lockFactory.CreateAsync(def.Name, TimeSpan.FromMinutes(1)).Result;
            var guid = Guid.NewGuid().ToString();

            if (!await @lock.TryAcquireAsync()) 
                    return;
            
            var result = await _jobs.CreateResultAsync(def.Name, guid, DateTimeOffset.Now);

            try
            {   
                await _aciService.CreateAsync(guid, def.Image, def.Private, def.OS, def.CPU, def.Memory, def.GetEnvValues());                
                
                await WaitAndWriteLog(result);
            }
            catch(Exception ex)
            {
                await _jobs.WriteLogAsync(def.Name, result.RowKey, ex.ToString(), ResultStatus.Failed, DateTimeOffset.Now);
            }
            finally
            {
                await @lock.ReleaseLeaseAsync();
            }
        }
        protected async Task WaitAndWriteLog(JobResult result)
        {
            while (true)
            {
                var state = await _aciService.GetStatusAsync(result.ContainerName);
                if (state == ACIStatus.None)
                {
                    await _jobs.WriteLogAsync(_def.Name, result.RowKey, "", ResultStatus.Cancel, DateTimeOffset.Now);
                    break;                  
                }
                if (state == ACIStatus.ProvisioningFailed)
                {
                    await _jobs.WriteLogAsync(_def.Name, result.RowKey, "ProvisioningFailed", ResultStatus.Failed, DateTimeOffset.Now);
                    break;
                }
                if (state == ACIStatus.Succeeded)
                {
                    var log = await _aciService.GetLogAsync(result.ContainerName);
                    await _jobs.WriteLogAsync(_def.Name, result.RowKey, log, ResultStatus.Success, DateTimeOffset.Now);
                    break;
                }
                if (state == ACIStatus.Succeeded)
                {
                    var log = await _aciService.GetLogAsync(result.ContainerName);
                    await _jobs.WriteLogAsync(_def.Name, result.RowKey, log, ResultStatus.Success, DateTimeOffset.Now);
                    break;
                }
                if (state == ACIStatus.Failed)
                {
                    var log = await _aciService.GetLogAsync(result.ContainerName);
                    await _jobs.WriteLogAsync(_def.Name, result.RowKey, log, ResultStatus.Failed, DateTimeOffset.Now);
                    break;
                }
                if (state == ACIStatus.Running)
                {
                    var log = await _aciService.GetLogAsync(result.ContainerName);
                    await _jobs.WriteLogAsync(_def.Name, result.RowKey, log, ResultStatus.Running, null);
                }

                await Task.Delay(TimeSpan.FromSeconds(3));
            }

            await _aciService.DeleteAsync(result.ContainerName);
        }
        protected abstract Task<bool> ShouldStartAsync(TDef def, DateTimeOffset now);
        protected abstract Task WaitNext(TDef def);

        protected async Task ExecuteForever()
        {
            while(true)
            {
                try
                {
                    var current =  await _jobs.FindAsync<TDef>(_def.Name);
                    if (current == null)
                        throw new ApplicationException($"{_def.Name} is not found");
                    
                    if (current.Enabled && await ShouldStartAsync(current, DateTimeOffset.Now))
                    {
                        await ExecuteJobAsync(current);
                    }

                    await WaitNext(current);
                }
                catch(OperationCanceledException cancel)
                {
                    ExceptionDispatchInfo.Capture(cancel).Throw();
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex, "An error has occurred");
                }
            }
        }
        public void Start(IEnumerable<JobResult> recoveries, CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(() => 
            {
                foreach(var recovery in recoveries)
                {
                    WaitAndWriteLog(recovery).Wait(cancellationToken);
                }
                ExecuteForever().Wait(cancellationToken);
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}