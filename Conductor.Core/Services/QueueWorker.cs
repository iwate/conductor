using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conductor.Core.Models;
using Microsoft.Extensions.Logging;

namespace Conductor.Core.Services
{
    public class QueueWorker : BackgroundWorkerBase
    {
        private readonly Func<JobDefinition, QueueJob> _jobFactory;
        public QueueWorker(IJobService jobService, IJobRegistory jobRegistory, ILockFactory lockFactory, IACIService aciService, ILogger logger)
            : base (jobService, jobRegistory, lockFactory, JobType.Queue, logger)
        {
            _jobFactory = def => new QueueJob(def, jobService, lockFactory, aciService, logger);
        }

        protected override IEnumerable<IJob> CreateJobs(IEnumerable<JobDefinition> defs)
        {
            return defs.Select(_jobFactory);
        }
    }
}