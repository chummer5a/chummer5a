using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace ChummerDataViewer.Model
{
	class DynamoDbLoader : INotifyThreadStatus
	{
		const string DataTable = "ChummerDumpsList";
		private AmazonDynamoDBClient _client;
		private readonly Thread _workerThread;
		private readonly WaitDurationProvider _backoff = new WaitDurationProvider();

		public DynamoDbLoader()
		{
			_client = new AmazonDynamoDBClient(PersistentState.AWSCredentials, RegionEndpoint.EUCentral1);
			_workerThread = new Thread(WorkerEntryPrt);
			_workerThread.Start();
		}

		private void WorkerEntryPrt()
		{
			try
			{
				while (true)
				{
					try
					{
						OnStatusChanged(new StatusChangedEventArgs("Connecting"));

						ScanResponse response = ScanData(
							PersistentState.Database.GetKey("crashdumps_last_timestamp"),
							PersistentState.Database.GetKey("crashdumps_last_key")); //Start scanning based on last key in db

						List<Guid> newItems = new List<Guid>();

						//Wait exponential backoff if no items
						if (response.Items.Count == 0)
						{
							int timeout = _backoff.GetSeconds();
							OnStatusChanged(new StatusChangedEventArgs($"No data. Retrying in {TimeSpan.FromSeconds(timeout)}"));
							for (int i = timeout - 1; i >= 0; i--)
							{
								Thread.Sleep(1000);
							}
							continue;
						}

						using (SQLiteTransaction transaction = PersistentState.Database.GetTransaction())
						{
							Dictionary<string, AttributeValue> nextRead;
							if (response.LastEvaluatedKey.Count != 0)
							{
								nextRead= response.LastEvaluatedKey;
								
							}
							else
							{
								nextRead = response.Items.Last();
							}

							PersistentState.Database.SetKey("crashdumps_last_timestamp", nextRead["upload_timestamp"].N);
							PersistentState.Database.SetKey("crashdumps_last_key", nextRead["crash_id"].S);

							foreach (Dictionary<string, AttributeValue> attributeValues in response.Items)
							{
								Guid guid = Guid.Parse(attributeValues["crash_id"].S);
								Version version;
								if (Version.TryParse(attributeValues["version"].S, out version))
								{ }
								else
								{ version = new Version(attributeValues["version"].S + ".0"); }

								PersistentState.Database.CreateCrashReport(
									guid,
									long.Parse(attributeValues["upload_timestamp"].N),
									attributeValues["build_type"].S,
									attributeValues["error_friendly"].S,
									attributeValues["key"].S,
									attributeValues["location"].S,
									version
								);

								_backoff.Sucess();
								newItems.Add(guid);
							}
							transaction.Commit();
						}

						OnStatusChanged(new StatusChangedEventArgs("Working", newItems));
					}
					catch (InternalServerErrorException)
					{
						int timeout = _backoff.GetSeconds();
						for (int i = timeout - 1; i >= 0; i--)
						{
							OnStatusChanged(new StatusChangedEventArgs($"Internal server error, retrying in {i} seconds"));
							Thread.Sleep(1000);
						}
					}
					catch (ProvisionedThroughputExceededException)
					{
						int timeout = _backoff.GetSeconds();
						for (int i = timeout - 1; i >= 0; i--)
						{
							OnStatusChanged(new StatusChangedEventArgs($"Too fast, retrying in {i} seconds"));
							Thread.Sleep(1000);
						}
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
				throw;
			}
		}

		private ScanResponse ScanData(string lastTimeStamp, string lastKey)
		{

			var request = new ScanRequest
			{
				TableName = DataTable,
				Limit = 10,
				ReturnConsumedCapacity = "TOTAL"
			};

			if (lastKey != null && lastTimeStamp != null)
			{
				request.ExclusiveStartKey = new Dictionary<string, AttributeValue>
				{
					{"crash_id", new AttributeValue {S = lastKey}},
					{"upload_timestamp", new AttributeValue {N = lastTimeStamp}}
				};
			} 

			return _client.Scan(request);
		}

		public event StatusChangedEvent StatusChanged;
		public string Name => "DynamoDBConnection";

		protected virtual void OnStatusChanged(StatusChangedEventArgs args)
		{
			StatusChanged?.Invoke(this, args);
		}
	}

	internal class WaitDurationProvider
	{
		private int _time = 1;

		public int GetSeconds()
		{
			int time = _time;
			_time *= 2;
			return time;
		}

		public void Sucess()
		{
			if (_time > 4)
			{
				_time /= 4;
			}
			else
			{
				_time = 1;
			}
		}
	}

	internal interface INotifyThreadStatus
	{
		event StatusChangedEvent StatusChanged;
		string Name { get; }
	}

	internal delegate void StatusChangedEvent(INotifyThreadStatus sender, StatusChangedEventArgs args);

	internal class StatusChangedEventArgs : EventArgs
	{
		public StatusChangedEventArgs(string status, dynamic attachedData = null)
		{
			if(status == null) throw new ArgumentNullException(nameof(status));

			Status = status;
			AttachedData = attachedData;
		}

		public string Status { get; }
		public dynamic AttachedData { get; }
	}
}
