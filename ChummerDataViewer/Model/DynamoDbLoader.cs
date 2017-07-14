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
			_workerThread = new Thread(WorkerEntryPrt)
			{
				IsBackground = true,
				Name = "DynamoDB Worker"
			};
			_workerThread.Start();
		}

		private void WorkerEntryPrt()
		{
			try
			{

				OnStatusChanged(new StatusChangedEventArgs("Connecting"));
				while (true)
				{
					try
					{
						//Scan 10 items. If middle of scan, pick up there
						ScanResponse response = ScanData(
							PersistentState.Database.GetKey("crashdumps_last_timestamp"),
							PersistentState.Database.GetKey("crashdumps_last_key")); //Start scanning based on last key in db

						//Into anon type with a little extra info. DB lookup to see if known, parse guid 
						var newItems = response.Items
							.Select(x => new { item = x, guid = Guid.Parse(x["crash_id"].S)})
							.Select(old => new {item = old.item, guid = old.guid, known = PersistentState.Database.GetCrash(old.guid) != null })
							.ToList();

						//If all items are known
						if (newItems.All(item => item.known))
						{
							//reset progress so we start from start (new first on dynamoDB)
							PersistentState.Database.SetKey("crashdumps_last_timestamp", null);
							PersistentState.Database.SetKey("crashdumps_last_key", null);

							//And sleep for exponential backoff
							int timeout = _backoff.GetSeconds();
							OnStatusChanged(new StatusChangedEventArgs($"No data. Retrying in {TimeSpan.FromSeconds(timeout)}"));
							for (int i = timeout - 1; i >= 0; i--)
							{
								Thread.Sleep(1000);
							}
							continue;
						}

						//Otherwise, add _NEW_ items to db
						using (SQLiteTransaction transaction = PersistentState.Database.GetTransaction())
						{
							
							if (response.LastEvaluatedKey.Count == 0)
							{
								//If we reached the last (oldest), reset progress meter
								PersistentState.Database.SetKey("crashdumps_last_timestamp", null);
								PersistentState.Database.SetKey("crashdumps_last_key", null);
							}
							else
							{
								//Otherwise set next to take next block
								Dictionary<string, AttributeValue> nextRead = response.LastEvaluatedKey;

								PersistentState.Database.SetKey("crashdumps_last_timestamp", nextRead["upload_timestamp"].N);
								PersistentState.Database.SetKey("crashdumps_last_key", nextRead["crash_id"].S);
							}

							//Write stuff
							foreach (var item in newItems.Where(x => !x.known))
							{
								WriteCrashToDb(item.item);

								//Don't take so long waiting for the next if we found anything.
								//Theoretically this should keep it checking roughly same frequency as new items gets added
								//in reality it is probably bull
								_backoff.Sucess(); 
							}
							transaction.Commit();
						}

						//Tell the good news that we have new items. Also tell guids so it can be found
						OnStatusChanged(new StatusChangedEventArgs("Working", 
							newItems
							.Where(x => !x.known)
							.Select(x => x.guid)
							.ToList()
						));
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

		private static void WriteCrashToDb(Dictionary<string, AttributeValue> attributeValues)
		{
			Guid guid = Guid.Parse(attributeValues["crash_id"].S);
			Version version;
			if (Version.TryParse(attributeValues["version"].S, out version))
			{
			}
			else
			{
				version = new Version(attributeValues["version"].S + ".0");
			}

			PersistentState.Database.CreateCrashReport(
				guid,
				long.Parse(attributeValues["upload_timestamp"].N),
				attributeValues["build_type"].S,
				attributeValues["error_friendly"].S,
				attributeValues["key"].S,
				attributeValues["location"].S,
				version
				);
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
