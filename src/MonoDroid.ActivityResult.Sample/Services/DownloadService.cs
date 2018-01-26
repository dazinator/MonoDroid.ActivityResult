using System.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using System.Net.Http;

namespace MonoDroid.ActivityResult.Sample
{
    public class DownloadService
    {

        private readonly Func<string, Task<Stream>> _fileStreamFactory;
        private readonly Func<HttpClient> _httpClientFactory;

        public DownloadService(Func<string, Task<Stream>> fileStreamFactory, Func<HttpClient> httpClientFactory)
        {
            _fileStreamFactory = fileStreamFactory;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> DownloadFileAsync(string url, string filePath, CancellationToken cancellationToken, IProgress<HttpClientExtensions.IFileDownloadProgressNotification> progress = null)
        {

            cancellationToken.ThrowIfCancellationRequested();

            using (Stream writeStream = await _fileStreamFactory(filePath))
            {
                using (var client = _httpClientFactory())
                {
                    client.Timeout = TimeSpan.FromMinutes(20);
                    client.BaseAddress = new Uri(url);
                    await client.DownloadAsync(url, writeStream, cancellationToken, progress);
                }
            }

            return filePath;
        }

    }


}

