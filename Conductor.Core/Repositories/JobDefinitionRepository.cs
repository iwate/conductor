using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Conductor.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Conductor.Core.Repositories
{
    public class JobDefinitionRepository : IJobDefinitionRepository
    {
        private readonly CloudTable _table;
        public JobDefinitionRepository(IConfiguration config)
        {
            var connectionsString = config.GetConnectionString("StorageAccount");
            if (string.IsNullOrEmpty(connectionsString))
                throw new ArgumentNullException("'StorageAccount' connection string is not found.");

            _table = CloudStorageAccount.Parse(connectionsString).CreateCloudTableClient().GetTableReference("conductor");
            _table.CreateIfNotExistsAsync().Wait();
        }

        public async Task<TJob> FindAsync<TJob>(string name) where TJob : JobDefinition
        {
            await _table.CreateIfNotExistsAsync();
            
            var op = TableOperation.Retrieve<TJob>("D", name);
            
            var result = await _table.ExecuteAsync(op);

            return (TJob)result.Result;
        }

        public async Task<IEnumerable<JobDefinition>> GetJobsAsync()
        {
            await _table.CreateIfNotExistsAsync();

            var query = new TableQuery<JobDefinition>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "D"));

            var defs = new List<JobDefinition>();

            TableContinuationToken continuationToken = null;

            do
            {
                var seg = await _table.ExecuteQuerySegmentedAsync(query, continuationToken);

                continuationToken = seg.ContinuationToken;

                defs.AddRange(seg.Results);

            } while(continuationToken != null);

            return defs;
        }

        public async Task AddAsync<TJob>(TJob job) where TJob : JobDefinition
        {
            await _table.CreateIfNotExistsAsync();
            
            var op = TableOperation.Insert(job);

            await _table.ExecuteAsync(op);
        }

        public async Task UpdateAsync<TJob>(TJob job) where TJob : JobDefinition
        {
            await _table.CreateIfNotExistsAsync();
            
            var op = TableOperation.InsertOrReplace(job);

            await _table.ExecuteAsync(op);
        }
    }
}