using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conductor.Core.Models;
using Microsoft.Extensions.Logging;

namespace Conductor.Core.Services
{
    public class CronWorker : BackgroundWorkerBase
    {
        private readonly Func<JobDefinition, CronJob> _jobFactory;
        public CronWorker(IJobService jobService, IJobRegistry jobRegistry, ILockFactory lockFactory, IACIService aciService, ILogger logger)
            : base (jobService, jobRegistry, lockFactory, JobType.Cron, logger)
        {
            _jobFactory = def => new CronJob(def, jobService, lockFactory, aciService, logger);
        }

        protected override IEnumerable<IJob> CreateJobs(IEnumerable<JobDefinition> defs)
        {
            return defs.Select(_jobFactory).ToArray();
        }
    }
}