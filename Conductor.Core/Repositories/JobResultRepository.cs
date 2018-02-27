using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Conductor.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Conductor.Core.Repositories
{
    public class JobResultRepository : IJobResultRepository
    {
        private readonly CloudTable _table;
        public JobResultRepository(IConfiguration config)
        {
            var connectionsString = config.GetConnectionString("StorageAccount");
            if (string.IsNullOrEmpty(connectionsString))
                throw new ArgumentNullException("'StorageAccount' connection string is not found.");

            _table = CloudStorageAccount.Parse(connectionsString).CreateCloudTableClient().GetTableReference("conductor");
            _table.CreateIfNotExistsAsync().Wait();
        }

        public async Task<IEnumerable<JobResult>> GetHistoryAsync(string jobName, string skipToken, int limit)
        {
            await _table.CreateIfNotExistsAsync();


            var query = new TableQuery<JobResult>();
            if (string.IsNullOrEmpty(skipToken))
            {
                query = query.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, jobName));
            }
            else
            {
                query = query.Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, jobName),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, skipToken)
                ));
            }
            query = query.Take(limit);

            var seg = await _table.ExecuteQuerySegmentedAsync(query, null);

            return seg.Results;
        }
        
        public async Task<JobResult> FindAsync(string name, string rowKey)
        {
            await _table.CreateIfNotExistsAsync();
            
            var op = TableOperation.Retrieve<JobResult>(name, rowKey);

            var res = await _table.ExecuteAsync(op);

            return (JobResult)res.Result;
        }

        public async Task AddAsync(JobResult result)
        {
            await _table.CreateIfNotExistsAsync();
            
            var op = TableOperation.Insert(result);

            await _table.ExecuteAsync(op);
        }

        public async Task UpdateAsync(JobResult result)
        {
            await _table.CreateIfNotExistsAsync();
            
            var op = TableOperation.InsertOrReplace(result);

            await _table.ExecuteAsync(op);
        }
    }
}