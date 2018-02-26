using System;
using System.Net;
using System.Threading.Tasks;
using Lixar.Azure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Conductor.Core.Services
{
    public class LockFactory : ILockFactory
    {
        private readonly CloudBlobClient _client;
        private readonly CloudBlobContainer _container;
        public LockFactory(IConfiguration config)
        {
            var connectionsString = config.GetConnectionString("StorageAccount");
            if (string.IsNullOrEmpty(connectionsString))
                throw new ArgumentNullException("'StorageAccount' connection string is not found.");

            _client = CloudStorageAccount.Parse(connectionsString).CreateCloudBlobClient();
            _container = _client.GetContainerReference("conductor");
        }

        public async Task<CloudLock> CreateAsync(string key, TimeSpan lockTime)
        {
            await _container.CreateIfNotExistsAsync();

            var blob = _container.GetBlockBlobReference($"{key}.lock");
            await blob.UploadTextAsync("");
            return new CloudLock(blob, lockTime);
        }
    }
}