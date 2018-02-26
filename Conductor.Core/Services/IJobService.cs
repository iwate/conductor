using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Conductor.Core.Models;

namespace Conductor.Core.Services
{
    public interface IJobService
    {
        Task<IEnumerable<JobDefinition>> GetAsync();
        Task<JobDefinition> FindAsync(string name);
        Task<TJob> FindAsync<TJob>(string name) where TJob : JobDefinition;
        Task AddAsync<TJob>(TJob job) where TJob : JobDefinition;
        Task UpdateAsync<TJob>(TJob job) where TJob : JobDefinition;

        Task<IEnumerable<JobResult>> GetHistoryAsync(string jobName, string skipToken = null, int limit = 1000);
        Task<JobResult> FindResultAsync(string jobName, string rowKey);
        Task<JobResult> CreateResultAsync(string jobName, string containerName, DateTimeOffset startAt);
        Task WriteLogAsync(string jobName, string rowKey, string log, ResultStatus status, DateTimeOffset? finishAt);
        Task<string> ReadLogAsync(string uri);
        Task<string> ReadLogAsync(string jobName, string rowKey);
    }
}