using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conductor.Core.Models;
using Microsoft.Extensions.Logging;

namespace Conductor.Core.Services
{
    public abstract class BackgroundWorkerBase
    {
        private readonly IJobService _jobService;
        private readonly ILockFactory _lockFactory;
        private readonly IJobRegistry _jobRegistry;
        private readonly CancellationTokenSource _globalCancellationTokenSource;
        private readonly List<IJob> _jobs;
        private readonly JobType _targetType;
        private readonly ILogger _logger;
        public BackgroundWorkerBase(IJobService jobService, IJobRegistry jobRegistry, ILockFactory lockFactory, JobType type, ILogger logger)
        {
            _jobService = jobService;
            _jobRegistry = jobRegistry;
            _lockFactory = lockFactory;
            _globalCancellationTokenSource = new CancellationTokenSource();
            _jobs = new List<IJob>();
            _targetType = type;
            _logger = logger;
        }
        protected abstract IEnumerable<IJob> CreateJobs(IEnumerable<JobDefinition> defs);
        
        private async Task<IEnumerable<JobDefinition>> GetTargetJobsAsync()
        {
            return (await _jobService.GetAsync()).Where(def => def.GetJobType() == _targetType && !_jobs.Any(job => job.Name == def.Name));
        }
        protected async Task ExecuteAsync()
        {
            var defs = await GetTargetJobsAsync();
                    
            var jobs = CreateJobs(defs).ToArray();
            
            _jobs.AddRange(jobs);

            foreach(var job in jobs)
            {
                var recoveries = (await _jobService.GetHistoryAsync(job.Name))
                    .Where(_ => !_.FinishAt.HasValue)
                    .OrderBy(_ => _.StartAt);
                job.Start(recoveries, _globalCancellationTokenSource.Token);
            }

            while(string.IsNullOrEmpty(_jobRegistry.Dequeue()))
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        public void Start()
        {
            Task.Factory.StartNew(() => {
                while(true) {
                    ExecuteAsync().Wait(_globalCancellationTokenSource.Token);
                }
            }, _globalCancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        public void Stop()
        {
            _globalCancellationTokenSource.Cancel();
        }
    }
}