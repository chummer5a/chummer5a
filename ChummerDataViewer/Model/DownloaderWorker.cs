using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChummerDataViewer.Model
{
	class DownloaderWorker : INotifyThreadStatus
	{
		public event StatusChangedEvent StatusChanged;
		public string Name => "DownloaderWorker";
		private Thread _thread;
		private AutoResetEvent resetEvent = new AutoResetEvent(false);
		private ConcurrentBag<DownloadTask> _queue = new ConcurrentBag<DownloadTask>();

		public DownloaderWorker()
		{
			_thread = new Thread(WorkerEntryPoint)
			{
				IsBackground = true,
				Name = "DownloaderWorker"
			};
			_thread.Start();
		}

		private void WorkerEntryPoint()
		{
			try
			{
				WebClient client = new WebClient();
				while (true)
				{
					DownloadTask task;
					if (_queue.TryTake(out task))
					{
						OnStatusChanged(new StatusChangedEventArgs("Downloading " + task.Url + Queue()));
						byte[] encrypted = client.DownloadData(task.Url);
					    byte[] buffer;
					    buffer = Decrypt(task.Key, encrypted);
					    WriteAndForget(buffer, task.DestinationPath, task.ReportGuid);
					}

					if (_queue.IsEmpty)
					{
						OnStatusChanged(new StatusChangedEventArgs("Idle"));
						resetEvent.WaitOne(15000);  //in case i fuck something up

					}
				}
			}
#if DEBUG
			catch(StackOverflowException ex)
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
	        using (AesManaged managed = new AesManaged())
	        {
	            managed.IV = GetIv(key);
	            managed.Key = GetKey(key);
	            ICryptoTransform encryptor = managed.CreateDecryptor();

	            // Create the streams used for encryption.
	            using (MemoryStream msEncrypt = new MemoryStream())
	            {
	                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
	                {
	                    csEncrypt.Write(encrypted, 0, encrypted.Length);
	                }

	                buffer = msEncrypt.ToArray();
	            }
	        }
	        return buffer;
	    }

	    private void WriteAndForget(byte[] buffer, string destinationPath, Guid guid)
		{
			ThreadPool.QueueUserWorkItem(a =>
			{
				Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
				File.WriteAllBytes(destinationPath, buffer);
				OnStatusChanged(new StatusChangedEventArgs("Saving " + destinationPath + Queue(), new {destinationPath, guid}));
			});
		}

		private static byte[] GetKey(string key)
		{
			string keypart = key.Split(':')[1];

			return Enumerable.Range(0, keypart.Length)
					 .Where(x => x % 2 == 0)
					 .Select(x => Convert.ToByte(keypart.Substring(x, 2), 16))
					 .ToArray();
		}

		private static byte[] GetIv(string iv)
		{
			string ivpart = iv.Split(':')[0];

			return Enumerable.Range(0, ivpart.Length)
					 .Where(x => x % 2 == 0)
					 .Select(x => Convert.ToByte(ivpart.Substring(x, 2), 16))
					 .ToArray();

		}

		protected virtual void OnStatusChanged(StatusChangedEventArgs args)
		{
			StatusChanged?.Invoke(this, args);
		}

		public void Enqueue(Guid guid, string url, string key, string destinationPath)
		{
			_queue.Add(new DownloadTask(guid, url, key, destinationPath));
			resetEvent.Set();
		}

		private struct DownloadTask
		{
			public Guid ReportGuid;
			public string Url;
			public string Key;
			public string DestinationPath;

			public DownloadTask(Guid reportGuid, string url, string key, string destinationPath)
			{
				Url = url;
				Key = key;
				DestinationPath = destinationPath;
				ReportGuid = reportGuid;
			}
		}

		private string Queue() => _queue.Count > 0 ? " " + _queue.Count + " in queue" : string.Empty;
	}

	
}
