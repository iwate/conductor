using System.Collections.Generic;
using System.Threading.Tasks;
using Conductor.Core.Models;

namespace Conductor.Core.Repositories
{
    public interface IJobResultRepository
    {
        Task<IEnumerable<JobResult>> GetHistoryAsync(string jobName, string skipToken, int limit);
        Task<JobResult> FindAsync(string jobName, string rowKey);
        Task AddAsync(JobResult result);
        Task UpdateAsync(JobResult result);
    }
}