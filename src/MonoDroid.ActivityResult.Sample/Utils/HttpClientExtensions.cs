using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult.Sample
{
    public static class HttpClientExtensions
    {
        public class FileDownloadProgressNotification : IFileDownloadProgressNotification
        {
            public long Total
            {
                get; set;
            }

            public long Current
            {
                get; set;
            }
        }

        public interface IFileDownloadProgressNotification
        {
            long Total
            {
                get; set;
            }
            long Current
            {
                get; set;
            }
        }

        public static async Task DownloadAsync(this HttpClient client
            , string requestUri
            , Stream destination
            , CancellationToken cancellationToken
            , IProgress<IFileDownloadProgressNotification> progress = null
            , Action<HttpResponseMessage> validateResponseCallback = null)
        {
            var notification = new FileDownloadProgressNotification();
            cancellationToken.ThrowIfCancellationRequested();

            using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                validateResponseCallback?.Invoke(response);

                response.EnsureSuccessStatusCode();
                cancellationToken.ThrowIfCancellationRequested();

                var contentLength = response.Content.Headers.ContentLength;
                notification.Total = contentLength.GetValueOrDefault();

                cancellationToken.ThrowIfCancellationRequested();
                using (var download = await response.Content.ReadAsStreamAsync())
                {
                    if (progress == null || !contentLength.HasValue)
                    {
                        await download.CopyToAsync(destination, 81920, cancellationToken, null);
                        notification.Total = destination.Length;
                        notification.Current = notification.Total;
                        progress?.Report(notification);
                        return;
                    }

                    Progress<long> bytesProgress = null;
                    if (progress != null)
                    {
                        bytesProgress = new Progress<long>(currentBytes =>
                        {
                            notification.Current = currentBytes; //(int)Math.Round((float)totalBytes / contentLength.Value);
                            progress.Report(notification);
                        });
                    }

                    await download.CopyToAsync(destination, 81920, cancellationToken, bytesProgress);

                    notification.Current = notification.Total;
                    progress?.Report(notification);
                }
            }
        }
    }
}