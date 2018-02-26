using System;
using System.Collections.Concurrent;

namespace Conductor.Core.Services
{
    public class JobRegistory : IJobRegistory
    {
        private ConcurrentQueue<string> _queue;
        public JobRegistory()
        {
            _queue = new ConcurrentQueue<string>();
        }
        public void Enqueue(string jobName)
        {
            _queue.Enqueue(jobName);
        }
        public string Dequeue()
        {
            string jobName = _queue.TryDequeue(out jobName) ? jobName : null;
            return jobName;
        }
    }
}