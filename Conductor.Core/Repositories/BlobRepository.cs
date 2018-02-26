using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Conductor.Core.Repositories
{
    public class BlobRepository : IBlobRepository
    {
        private readonly CloudBlobClient _client;
        private readonly CloudBlobContainer _container;
        public BlobRepository(IConfiguration config)
        {
            var connectionsString = config.GetConnectionString("StorageAccount");
            if (string.IsNullOrEmpty(connectionsString))
                throw new ArgumentNullException("'StorageAccount' connection string is not found.");

            _client = CloudStorageAccount.Parse(connectionsString).CreateCloudBlobClient();
            _container = _client.GetContainerReference("conductor");
        }
        public async Task<Uri> UploadAsync(string text, string replaceTo = null)
        {
            await _container.CreateIfNotExistsAsync();

            CloudBlockBlob block;

            if (string.IsNullOrEmpty(replaceTo))
            {
                var prefix = DateTimeOffset.Now.ToString("yyyy-MM-dd/hh/mm");
                var guid = Guid.NewGuid().ToString();

                block = _container.GetBlockBlobReference($"{prefix}/{guid}.log");   
            }
            else
            {
                block = (CloudBlockBlob)await _client.GetBlobReferenceFromServerAsync(new Uri(replaceTo));
            }

            await block.UploadTextAsync(text);

            return block.Uri;
        }

        public async Task<string> DownloadAsync(string uri)
        {
            await _container.CreateIfNotExistsAsync();

            if (string.IsNullOrEmpty(uri))
                return null;
            
            var block = (CloudBlockBlob)await _client.GetBlobReferenceFromServerAsync(new Uri(uri));

            return await block.DownloadTextAsync();
        }
    }
}