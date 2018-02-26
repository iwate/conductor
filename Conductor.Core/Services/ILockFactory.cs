using System;
using System.Threading.Tasks;
using Lixar.Azure.Storage;

namespace Conductor.Core.Services
{
    public interface ILockFactory
    {
        Task<CloudLock> CreateAsync(string key, TimeSpan lockTime);
    }
}