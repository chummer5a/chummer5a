/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Asynchronously download content from a target location to a destination stream and report on its progress.
        /// </summary>
        internal static async Task DownloadWithProgressAsync(this HttpClient objClient, string strRequestUri, Stream objDestination, IProgress<double> objProgress, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (HttpResponseMessage objResponse = await objClient.GetAsync(strRequestUri, token).ConfigureAwait(false))
            {
                HttpContent objContent = objResponse.Content;
                long? lngContentLength = objContent.Headers.ContentLength;
                if (objProgress == null || !lngContentLength.HasValue)
                {
                    using (Stream objDownload = await objContent.ReadAsStreamAsync(token).ConfigureAwait(false))
                    {
                        token.ThrowIfCancellationRequested();
                        await objDownload.CopyToAsync(objDestination, token).ConfigureAwait(false);
                    }
                }
                else
                {
                    long lngTotalBytes = lngContentLength.Value;
                    Progress<long> objInnerProgress = new Progress<long>(lngProcessedBytes => objProgress.Report(((double)lngProcessedBytes) / lngTotalBytes));
                    token.ThrowIfCancellationRequested();
                    using (Stream objDownload = await objContent.ReadAsStreamAsync(token).ConfigureAwait(false))
                    {
                        token.ThrowIfCancellationRequested();
                        await objDownload.CopyToWithProgressAsync(objDestination, objInnerProgress, token).ConfigureAwait(false);
                    }
                    // Set progress to 100% if we're done.
                    objProgress.Report(1.0);
                }
            }
        }

        /// <summary>
        /// Asynchronously download content from a target location to a destination stream and report on its progress.
        /// </summary>
        internal static async Task DownloadWithProgressAsync(this HttpClient objClient, Uri uriRequest, Stream objDestination, IProgress<double> objProgress, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (HttpResponseMessage objResponse = await objClient.GetAsync(uriRequest, token).ConfigureAwait(false))
            {
                HttpContent objContent = objResponse.Content;
                long? lngContentLength = objContent.Headers.ContentLength;
                if (objProgress == null || !lngContentLength.HasValue)
                {
                    using (Stream objDownload = await objContent.ReadAsStreamAsync(token).ConfigureAwait(false))
                    {
                        token.ThrowIfCancellationRequested();
                        await objDownload.CopyToAsync(objDestination, token).ConfigureAwait(false);
                    }
                }
                else
                {
                    long lngTotalBytes = lngContentLength.Value;
                    Progress<long> objInnerProgress = new Progress<long>(lngProcessedBytes => objProgress.Report(((double)lngProcessedBytes) / lngTotalBytes));
                    token.ThrowIfCancellationRequested();
                    using (Stream objDownload = await objContent.ReadAsStreamAsync(token).ConfigureAwait(false))
                    {
                        token.ThrowIfCancellationRequested();
                        await objDownload.CopyToWithProgressAsync(objDestination, objInnerProgress, token).ConfigureAwait(false);
                    }
                    // Set progress to 100% if we're done.
                    objProgress.Report(1.0);
                }
            }
        }
    }
}
