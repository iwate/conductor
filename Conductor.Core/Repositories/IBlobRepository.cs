using System;
using System.Threading.Tasks;

namespace Conductor.Core.Repositories
{
    public interface IBlobRepository
    {
        Task<Uri> UploadAsync(string text, string replaceTo = null);
        Task<string> DownloadAsync(string uri);
    }
}