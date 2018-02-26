using System.Collections.Generic;
using System.Threading.Tasks;
using Conductor.Core.Models;

namespace Conductor.Core.Services
{
    public interface IACIService
    {
        Task<IEnumerable<string>> GetImagesAsync();
        Task<bool> ExitsRunning(string imageName);
        Task CreateAsync(string containerName, string imageName, bool @private, string os, double cpu, double memory, IDictionary<string, string> env);
        Task<ACIStatus> GetStatusAsync(string containerName);
        Task<string> GetLogAsync(string containerName);
        Task DeleteAsync(string containerName);
    }
}