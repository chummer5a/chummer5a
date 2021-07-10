using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;

namespace ChummerDataViewer.Model
{
    public sealed class DownloaderWorker : INotifyThreadStatus, IDisposable
    {
        public event StatusChangedEvent StatusChanged;

        public string Name => "DownloaderWorker";
        private readonly BackgroundWorker _worker = new BackgroundWorker();
        private readonly AutoResetEvent resetEvent = new AutoResetEvent(false);
        private readonly ConcurrentBag<DownloadTask> _queue = new ConcurrentBag<DownloadTask>();

        public DownloaderWorker()
        {
            _worker.WorkerReportsProgress = false;
            _worker.WorkerSupportsCancellation = false;
            _worker.DoWork += WorkerEntryPoint;
            _worker.RunWorkerAsync();
        }

        private async void WorkerEntryPoint(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    while (true)
                    {
                        if (_queue.TryTake(out DownloadTask task))
                        {
                            OnStatusChanged(new StatusChangedEventArgs("Downloading " + task.Url + Queue()));
                            byte[] encrypted = await client.DownloadDataTaskAsync(task.Url);
                            byte[] buffer = Decrypt(task.Key, encrypted);
                            WriteAndForget(buffer, task.DestinationPath, task.ReportGuid);
                        }

                        if (_queue.IsEmpty)
                        {
                            OnStatusChanged(new StatusChangedEventArgs("Idle"));
                            resetEvent.WaitOne(15000);  //in case i fuck something up
                        }
                    }
                }
            }
#if DEBUG
            catch (StackOverflowException ex)
#else
			catch (Exception ex)
#endif
            {
                OnStatusChanged(new StatusChangedEventArgs("Crashed", ex));
            }
        }

        public static byte[] Decrypt(string key, byte[] encrypted)
        {
            byte[] buffer;
            // Create the streams used for encryption.
            AesManaged managed = null;
            try
            {
                managed = new AesManaged
                {
                    IV = GetIv(key),
                    Key = GetKey(key)
                };
                ICryptoTransform encryptor = managed.CreateDecryptor();

                MemoryStream msEncrypt = new MemoryStream();
                // csEncrypt.Dispose() should call msEncrypt.Dispose()
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(encrypted, 0, encrypted.Length);
                    buffer = msEncrypt.ToArray();
                }
            }
            finally
            {
                managed?.Dispose();
            }
            return buffer;
        }

        private void WriteAndForget(byte[] buffer, string destinationPath, Guid guid)
        {
            ThreadPool.QueueUserWorkItem(a =>
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? string.Empty);
                File.WriteAllBytes(destinationPath, buffer);
                OnStatusChanged(new StatusChangedEventArgs("Saving " + destinationPath + Queue(), new { destinationPath, guid }));
            });
        }

        private static byte[] GetKey(string key)
        {
            string keypart = key.Split(':')[1];

            return Enumerable.Range(0, keypart.Length)
                     .Where(x => (x & 1) == 0)
                     .Select(x => Convert.ToByte(keypart.Substring(x, 2), 16))
                     .ToArray();
        }

        private static byte[] GetIv(string iv)
        {
            string ivpart = iv.Split(':')[0];

            return Enumerable.Range(0, ivpart.Length)
                     .Where(x => (x & 1) == 0)
                     .Select(x => Convert.ToByte(ivpart.Substring(x, 2), 16))
                     .ToArray();
        }

        private void OnStatusChanged(StatusChangedEventArgs args)
        {
            StatusChanged?.Invoke(this, args);
        }

        public void Enqueue(Guid guid, Uri url, string key, string destinationPath)
        {
            _queue.Add(new DownloadTask(guid, url, key, destinationPath));
            resetEvent.Set();
        }

        private readonly struct DownloadTask
        {
            public Guid ReportGuid { get; }
            public Uri Url { get; }
            public string Key { get; }
            public string DestinationPath { get; }

            public DownloadTask(Guid reportGuid, Uri url, string key, string destinationPath)
            {
                Url = url;
                Key = key;
                DestinationPath = destinationPath;
                ReportGuid = reportGuid;
            }
        }

        private string Queue() => _queue.Count > 0 ? _queue.Count.ToString() + " in queue" : string.Empty;

        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    resetEvent.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}
