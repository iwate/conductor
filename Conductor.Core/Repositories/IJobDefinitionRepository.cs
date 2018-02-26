using System.Collections.Generic;
using System.Threading.Tasks;
using Conductor.Core.Models;

namespace Conductor.Core.Repositories
{
    public interface IJobDefinitionRepository
    {
        Task<IEnumerable<JobDefinition>> GetJobsAsync();
        Task<TJob> FindAsync<TJob>(string name) where TJob : JobDefinition;
        Task AddAsync<TJob>(TJob job) where TJob : JobDefinition;
        Task UpdateAsync<TJob>(TJob job) where TJob : JobDefinition;
    }
}