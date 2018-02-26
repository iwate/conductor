using System;
using System.Threading;
using System.Threading.Tasks;
using Conductor.Core.Services;
using Microsoft.Extensions.Logging;

namespace Conductor.Core.Models
{
    public class QueueJob : JobBase<QueueJobDefinition>
    {
        public QueueJob(JobDefinition def, IJobService jobService, ILockFactory lockFactory, IACIService aciService, ILogger logger)
            : base (def, jobService, lockFactory, aciService, logger)
        {
        }

        protected override async Task<bool> ShouldStartAsync(QueueJobDefinition def, DateTimeOffset now)
        {
            var queue = await def.GetQueueAsync();
            
            await queue.FetchAttributesAsync();

            return queue.ApproximateMessageCount.HasValue ? queue.ApproximateMessageCount.Value > 0 : false;
        }

        protected override Task WaitNext(QueueJobDefinition def)
        {
            return Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}