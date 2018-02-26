using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Conductor.Core.Models;
using Conductor.Core.Repositories;

namespace Conductor.Core.Services
{
    public class JobService : IJobService
    {
        private readonly IJobDefinitionRepository _defs;
        private readonly IJobResultRepository _results;
        private readonly IBlobRepository _blobs;
        public JobService(IJobDefinitionRepository jobDefinitionRepository, IJobResultRepository jobResultRepository, IBlobRepository blobRepository)
        {
            _defs = jobDefinitionRepository;
            _results = jobResultRepository;
            _blobs = blobRepository;
        }
        public async Task<IEnumerable<JobDefinition>> GetAsync()
        {
            return await _defs.GetJobsAsync();
        }
        public async Task<TJob> FindAsync<TJob>(string name) where TJob : JobDefinition
        {
            return await _defs.FindAsync<TJob>(name);
        }
        public async Task<JobDefinition> FindAsync(string name)
        {
            var job = await FindAsync<JobDefinition>(name);
            if (job == null)
                return null;
            switch(job.GetJobType())
            {
                case JobType.Cron: 
                    return await FindAsync<CronJobDefinition>(name);
                case JobType.Queue: 
                    return await FindAsync<QueueJobDefinition>(name);
                default: 
                    return null;
            }
        }
        public async Task AddAsync<TJob>(TJob job) where TJob : JobDefinition
        {
            await _defs.AddAsync(job);
        }
        public async Task UpdateAsync<TJob>(TJob job) where TJob : JobDefinition
        {
            await _defs.UpdateAsync(job);
        }

        public async Task<IEnumerable<JobResult>> GetHistoryAsync(string jobName, string skipToken = null, int limit = 1000)
        {
            return await _results.GetHistoryAsync(jobName, skipToken, limit);
        }
        public async Task<JobResult> FindResultAsync(string jobName, string rowKey)
        {
            return await _results.FindAsync(jobName, rowKey);
        }
        public async Task<JobResult> CreateResultAsync(string jobName, string containerName, DateTimeOffset startAt)
        {
            var result = new JobResult(jobName, startAt)
            { 
                Status = ResultStatus.Creating.ToString(), 
                ContainerName = containerName, 
                StartAt =  startAt 
            };
            
            await _results.AddAsync(result);

            return result;
        }
        public async Task WriteLogAsync(string jobName, string rowKey, string log, ResultStatus status, DateTimeOffset? finishAt)
        {
            var result = await FindResultAsync(jobName, rowKey);
            result.LogUri = (await _blobs.UploadAsync(log, result.LogUri)).ToString();
            result.Status = status.ToString();
            result.FinishAt = finishAt;

            await _results.UpdateAsync(result);
        }
        public async Task<string> ReadLogAsync(string uri)
        {
            if (uri == null)
                return "";

            return await _blobs.DownloadAsync(uri);
        }
        public async Task<string> ReadLogAsync(string jobName, string rowKey)
        {
            var result = await FindResultAsync(jobName, rowKey);
            return await ReadLogAsync(result.LogUri);
        }
    }
}