using System;
using System.Threading;
using System.Threading.Tasks;
using Conductor.Core.Services;
using Microsoft.Extensions.Logging;

namespace Conductor.Core.Models
{
    public class CronJob : JobBase<CronJobDefinition>
    {
        public CronJob(JobDefinition def, IJobService jobService, ILockFactory lockFactory, IACIService aciService, ILogger logger)
            : base (def, jobService, lockFactory, aciService, logger)
        {
        }

        protected override Task<bool> ShouldStartAsync(CronJobDefinition def, DateTimeOffset now)
        {
            return Task.FromResult(Cron.Parse(def.Cron).Match(now));
        }

        protected override Task WaitNext(CronJobDefinition def)
        {
            var next = Cron.Parse(def.Cron).CalcNext(DateTimeOffset.Now);
            Logger.LogInformation($"{def.Name}: wait {next}");
            return Task.Delay(next);
        }
    }
}