using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Y_Track.Network
{
    public static class HttpClientEx
    {
        public static async Task<Stream> GetStreamAsync(this HttpClient client, string requestUri,
      long? from = null, long? to = null, bool ensureSuccess = true)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Range = new RangeHeaderValue(from, to);

            using (request)
            {
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false);

                if (ensureSuccess)
                    response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            }
        }

        public static async Task CopyToStreamAsync(this Stream source, Stream destination,
           IProgress<double> progress = null, CancellationToken cancellationToken = default(CancellationToken),
           int bufferSize = 81920)
        {
            var buffer = new byte[bufferSize];

            var totalBytesCopied = 0L;
            int bytesCopied;

            do
            {
                // Read
                bytesCopied = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

                // Write
                await destination.WriteAsync(buffer, 0, bytesCopied, cancellationToken).ConfigureAwait(false);

                // Report progress
                totalBytesCopied += bytesCopied;
                progress?.Report(1.0 * totalBytesCopied / source.Length);
            } while (bytesCopied > 0);
        }
    }
}
