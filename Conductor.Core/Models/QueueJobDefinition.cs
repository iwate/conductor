using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace Conductor.Core.Models
{
    public class QueueJobDefinition : JobDefinition
    {
        public string Queue { get; set; }
        public string ConnectionString { get; set; }
        public QueueJobDefinition()
        {}
        public QueueJobDefinition(string name)
            :base(name)
        {
            Name = name;
            Type = JobType.Queue.ToString();            
        }

        public async Task<CloudQueue> GetQueueAsync()
        {
            CloudStorageAccount account = CloudStorageAccount.TryParse(ConnectionString, out account) ? account : null;
            
            var queue = account.CreateCloudQueueClient().GetQueueReference(Queue);

            await queue.CreateIfNotExistsAsync();

            return queue;
        }

        public override IDictionary<string, string> GetEnvValues()
        {
            var envValues = base.GetEnvValues();
            envValues["ConnectionString"] = ConnectionString;
            envValues["Queue"] = Queue;

            return envValues;
        }
    }
}