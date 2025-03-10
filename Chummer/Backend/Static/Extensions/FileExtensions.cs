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
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace Chummer
{
    public static class FileExtensions
    {
        /// <summary>
        /// Wait for an open file to be available for deletion and then delete it.
        /// </summary>
        /// <param name="strPath">File path to delete.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether to show a message if the file cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <param name="token">Cancellation token to use</param>
        /// <returns>True if file does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        public static bool SafeDelete(string strPath, bool blnShowUnauthorizedAccess = false, int intTimeout = Utils.DefaultSleepDuration * 60, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => SafeDeleteCoreAsync(true, strPath, blnShowUnauthorizedAccess, intTimeout, token), token);
        }

        /// <summary>
        /// Wait for an open file to be available for deletion and then delete it.
        /// </summary>
        /// <param name="strPath">File path to delete.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether to show a message if the file cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <param name="token">Cancellation token to use</param>
        /// <returns>True if file does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        public static Task<bool> SafeDeleteAsync(string strPath, bool blnShowUnauthorizedAccess = false, int intTimeout = Utils.DefaultSleepDuration * 60, CancellationToken token = default)
        {
            return SafeDeleteCoreAsync(false, strPath, blnShowUnauthorizedAccess, intTimeout, token);
        }

        /// <summary>
        /// Wait for an open file to be available for deletion and then delete it.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strPath">File path to delete.</param>
        /// <param name="blnShowUnauthorizedAccess">Whether to show a message if the file cannot be accessed because of permissions.</param>
        /// <param name="intTimeout">Amount of time to wait for deletion, in milliseconds</param>
        /// <param name="token">Cancellation token to use</param>
        /// <returns>True if file does not exist or deletion was successful. False if deletion was unsuccessful.</returns>
        private static async Task<bool> SafeDeleteCoreAsync(bool blnSync, string strPath, bool blnShowUnauthorizedAccess, int intTimeout, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strPath))
                return true;
            int intWaitInterval = Math.Max(intTimeout / Utils.DefaultSleepDuration, Utils.DefaultSleepDuration);
            while (File.Exists(strPath))
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    if (!strPath.StartsWith(Utils.GetStartupPath, StringComparison.OrdinalIgnoreCase)
                        && !strPath.StartsWith(Utils.GetTempPath(), StringComparison.OrdinalIgnoreCase))
                    {
                        token.ThrowIfCancellationRequested();
                        // For safety purposes, do not allow unprompted deleting of any files outside of the Chummer folder itself
                        if (blnShowUnauthorizedAccess)
                        {
                            if (blnSync)
                            {
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                if (Program.ShowScrollableMessageBox(
                                        string.Format(GlobalSettings.CultureInfo,
                                            // ReSharper disable once MethodHasAsyncOverload
                                            LanguageManager.GetString("Message_Prompt_Delete_Existing_File",
                                                token: token), strPath),
                                        buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Warning) !=
                                    DialogResult.Yes)
                                    return false;
                            }
                            else if (await Program.ShowScrollableMessageBoxAsync(
                                         string.Format(GlobalSettings.CultureInfo,
                                             await LanguageManager.GetStringAsync(
                                                     "Message_Prompt_Delete_Existing_File", token: token)
                                                 .ConfigureAwait(false), strPath),
                                         buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Warning, token: token).ConfigureAwait(false) !=
                                     DialogResult.Yes)
                                return false;
                        }
                        else
                        {
                            Utils.BreakIfDebug();
                            return false;
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    if (blnSync)
                        File.Delete(strPath);
                    else
                        await Task.Run(() => File.Delete(strPath), token).ConfigureAwait(false);
                }
                catch (PathTooLongException)
                {
                    // File path is somehow too long? File is not deleted, so return false.
                    return false;
                }
                catch (UnauthorizedAccessException)
                {
                    // We do not have sufficient privileges to delete this file.
                    if (blnShowUnauthorizedAccess)
                    {
                        if (blnSync)
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            Program.ShowScrollableMessageBox(
                                // ReSharper disable once MethodHasAsyncOverload
                                LanguageManager.GetString("Message_Insufficient_Permissions_Warning", token: token));
                        else
                            await Program.ShowScrollableMessageBoxAsync(
                                await LanguageManager
                                    .GetStringAsync("Message_Insufficient_Permissions_Warning", token: token)
                                    .ConfigureAwait(false), token: token).ConfigureAwait(false);
                    }

                    return false;
                }
                catch (DirectoryNotFoundException)
                {
                    // File doesn't exist.
                    return true;
                }
                catch (FileNotFoundException)
                {
                    // File doesn't exist.
                    return true;
                }
                catch (IOException)
                {
                    //the file is unavailable because it is:
                    //still being written to
                    //or being processed by another thread
                    //or does not exist (has already been processed)
                    if (blnSync)
                        Utils.SafeSleep(intWaitInterval, token);
                    else
                        await Utils.SafeSleepAsync(intWaitInterval, token).ConfigureAwait(false);
                    intTimeout -= intWaitInterval;
                }
                if (intTimeout < 0)
                {
                    Utils.BreakIfDebug();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// An extended version of File.ReadAllText() that can handle interruptions from a cancellation token.
        /// </summary>
        /// <param name="strPath">Path of the file whose contents should be read.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The complete contents of the file from <paramref name="strPath"/>.</returns>
        public static string ReadAllText(string strPath, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strPath) || !File.Exists(strPath))
                return string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                token.ThrowIfCancellationRequested();
                using (FileStream objFileStream = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader objReader = new StreamReader(objFileStream))
                {
                    token.ThrowIfCancellationRequested();
                    for (string strLine = objReader.ReadLine(); strLine != null; strLine = objReader.ReadLine())
                    {
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrEmpty(strLine))
                            sbdReturn.AppendLine(strLine);
                    }
                }
                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// An extended version of File.ReadAllText() that can handle interruptions from a cancellation token.
        /// </summary>
        /// <param name="strPath">Path of the file whose contents should be read.</param>
        /// <param name="eEncoding">Specific encoding to use when reading the file.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The complete contents of the file from <paramref name="strPath"/>.</returns>
        public static string ReadAllText(string strPath, Encoding eEncoding, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strPath) || !File.Exists(strPath))
                return string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                token.ThrowIfCancellationRequested();
                using (FileStream objFileStream = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader objReader = new StreamReader(objFileStream, eEncoding))
                {
                    token.ThrowIfCancellationRequested();
                    for (string strLine = objReader.ReadLine(); strLine != null; strLine = objReader.ReadLine())
                    {
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrEmpty(strLine))
                            sbdReturn.AppendLine(strLine);
                    }
                }
                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// An extended version of File.ReadAllText() that is asynchronous and can handle interruptions from a cancellation token.
        /// </summary>
        /// <param name="strPath">Path of the file whose contents should be read.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The complete contents of the file from <paramref name="strPath"/>.</returns>
        public static async Task<string> ReadAllTextAsync(string strPath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strPath) || !File.Exists(strPath))
                return string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                token.ThrowIfCancellationRequested();
                await using (FileStream objFileStream
                             = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                {
                    token.ThrowIfCancellationRequested();
                    using (StreamReader objReader = new StreamReader(objFileStream))
                    {
                        token.ThrowIfCancellationRequested();
                        for (string strLine = await objReader.ReadLineAsync(token).ConfigureAwait(false);
                             strLine != null;
                             strLine = await objReader.ReadLineAsync(token).ConfigureAwait(false))
                        {
                            token.ThrowIfCancellationRequested();
                            if (!string.IsNullOrEmpty(strLine))
                                sbdReturn.AppendLine(strLine);
                        }
                    }
                }

                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// An extended version of File.ReadAllText() that is asynchronous and can handle interruptions from a cancellation token.
        /// </summary>
        /// <param name="strPath">Path of the file whose contents should be read.</param>
        /// <param name="eEncoding">Specific encoding to use when reading the file.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The complete contents of the file from <paramref name="strPath"/>.</returns>
        public static async Task<string> ReadAllTextAsync(string strPath, Encoding eEncoding, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strPath) || !File.Exists(strPath))
                return string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                token.ThrowIfCancellationRequested();
                await using (FileStream objFileStream
                             = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                {
                    token.ThrowIfCancellationRequested();
                    using (StreamReader objReader = new StreamReader(objFileStream, eEncoding))
                    {
                        token.ThrowIfCancellationRequested();
                        for (string strLine = await objReader.ReadLineAsync(token).ConfigureAwait(false);
                             strLine != null;
                             strLine = await objReader.ReadLineAsync(token).ConfigureAwait(false))
                        {
                            token.ThrowIfCancellationRequested();
                            if (!string.IsNullOrEmpty(strLine))
                                sbdReturn.AppendLine(strLine);
                        }
                    }
                }

                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// An extended version of File.WriteAllText() that is asynchronous.
        /// </summary>
        /// <param name="strPath">Path of the file where contents should be written.</param>
        /// <param name="strContents">The contents to write to the file.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task WriteAllTextAsync(string strPath, string strContents, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strPath))
                return;
            using (FileStream objFileStream
                   = new FileStream(strPath, FileMode.Append, FileAccess.Write, FileShare.Write, 4096, true))
            using (StreamWriter objWriter = new StreamWriter(objFileStream))
            {
                token.ThrowIfCancellationRequested();
                await objWriter.WriteAsync(strContents).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// An extended version of File.WriteAllText() that is asynchronous.
        /// </summary>
        /// <param name="strPath">Path of the file where contents should be written.</param>
        /// <param name="strContents">The contents to write to the file.</param>
        /// <param name="eEncoding">Specific encoding to use when writing to the file.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task WriteAllTextAsync(string strPath, string strContents, Encoding eEncoding, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strPath))
                return;
            using (FileStream objFileStream
                   = new FileStream(strPath, FileMode.Append, FileAccess.Write, FileShare.Write, 4096, true))
            using (StreamWriter objWriter = new StreamWriter(objFileStream, eEncoding))
            {
                token.ThrowIfCancellationRequested();
                await objWriter.WriteAsync(strContents).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// An extended version of File.ReadAllBytes() that is asynchronous.
        /// </summary>
        /// <param name="strPath">The file to open for reading.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<byte[]> ReadAllBytesAsync(string strPath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strPath) || !File.Exists(strPath))
                return Array.Empty<byte>();
            using (FileStream objFileStream = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                long lngLength = objFileStream.Length;
                if (lngLength == 0)
                    return Array.Empty<byte>();
                int intCount = lngLength <= int.MaxValue ? (int)lngLength : throw new IOException("File too large.");
                byte[] achrReturn = new byte[intCount];
                int intLoop;
                for (int intOffset = 0; intCount > 0; intCount -= intLoop)
                {
                    token.ThrowIfCancellationRequested();
                    intLoop = await objFileStream.ReadAsync(achrReturn, intOffset, intCount, token).ConfigureAwait(false);
                    if (intLoop == 0)
                        throw new EndOfStreamException();
                    intOffset += intLoop;
                }
                return achrReturn;
            }
        }

        /// <summary>
        /// An extended version of File.WriteAllBytes() that is asynchronous.
        /// </summary>
        /// <param name="strPath">The file to write to.</param>
        /// <param name="achrBytes">The bytes to write to the file.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task WriteAllBytesAsync(string strPath, byte[] achrBytes, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strPath))
                throw new ArgumentException("Path is empty.", nameof(strPath));
            if (achrBytes == null)
                throw new ArgumentNullException(nameof(achrBytes));

            using (FileStream objFileStream = new FileStream(strPath, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, true))
            {
                token.ThrowIfCancellationRequested();
                await objFileStream.WriteAsync(achrBytes, 0, achrBytes.Length, token).ConfigureAwait(false);
            }
        }
    }
}
