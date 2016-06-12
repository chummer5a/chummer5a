using System;
using System.Data.SQLite;
using System.IO;

namespace ChummerDataViewer.Model
{
	internal class Database
	{
		private readonly object _syncRoot = new object();
		private readonly SQLiteConnection _dbConnection;
		private readonly SQLiteCommand _setKey;
		private readonly SQLiteCommand _getKey;
		private readonly SQLiteCommand _insertCrash;

		private static void ExecuteSQL(SQLiteConnection connection, string sql)
		{
			using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public static Database Create()
		{
			SQLiteConnection connection =
				new SQLiteConnection($"Data Source={Path.Combine(PersistentState.FolderPath, "persistent.db")}; Version=3;");
			connection.Open();
			ExecuteSQL(connection, "CREATE TABLE kvstore (key TEXT UNIQUE PRIMARY KEY, value TEXT);");

			ExecuteSQL(connection, "CREATE TABLE crashreports " +
			                       "(" +
			                       "guid TEXT UNIQUE PRIMARY KEY, " +
			                       "timestamp INTEGER, " +
			                       "buildtype TEXT, " +
			                       "errorfriendly TEXT, " +
			                       "encryptionkey TEXT, " +
			                       "url TEXT, " +
			                       "version TEXT, " +
			                       "ziplocation TEXT, " +
			                       "folderlocation TEXT" +
			                       ");");

			ExecuteSQL(connection, "CREATE INDEX errortypeindex ON crashreports (errorfriendly);");
			ExecuteSQL(connection, "CREATE INDEX timeindex ON crashreports (timestamp);");
			ExecuteSQL(connection, "CREATE INDEX versionindex ON crashreports (version);");

			return new Database(connection);
		}

		private Database(SQLiteConnection conn)
		{
			_dbConnection = conn;
			_setKey = new SQLiteCommand("INSERT OR REPLACE INTO kvstore (key, value) VALUES (@key, @value);", _dbConnection);
			_getKey = new SQLiteCommand("SELECT value FROM kvstore WHERE key = @key;", _dbConnection);
			_insertCrash = new SQLiteCommand("INSERT OR IGNORE INTO crashreports " +
			                                 "(guid, timestamp, buildtype, errorfriendly, encryptionkey, url, version) " +
			                                 "VALUES " +
			                                 "(@guid, @timestamp, @buildtype, @errorfriendly, @encryptionkey, @url, @version);", 
				_dbConnection);
		}

		public Database() : this(
			new SQLiteConnection($"Data Source={Path.Combine(PersistentState.FolderPath, "persistent.db")}; Version=3;")
				.OpenAndReturn())
		{
			
		}

		public void SetKey(string key, string value)
		{
			lock (_syncRoot)
			{
				_setKey.Reset(true, false);
				_setKey.Parameters.Add(new SQLiteParameter("@key", key));
				_setKey.Parameters.Add(new SQLiteParameter("@value", value));

				_setKey.ExecuteNonQuery();
			}
		}

		public string GetKey(string key)
		{
			_getKey.Reset(true, false);
			_getKey.Parameters.Add(new SQLiteParameter("@key", key));

			using (SQLiteDataReader reader = _getKey.ExecuteReader())
			{
				while (reader.Read())
				{
					return reader["value"].ToString();
				}
			}

			return null;
		}

		public SQLiteTransaction GetTransaction()
		{
			return _dbConnection.BeginTransaction();
		}

		public void CreateCrashReport(Guid guid, long timestamp, string buildType, string errorFriendly, string key, string webFileLocation, Version version)
		{
			_insertCrash.Reset(true, false);
			_insertCrash.Parameters.Add(new SQLiteParameter("@guid", guid.ToString()));
			_insertCrash.Parameters.Add(new SQLiteParameter("@timestamp", timestamp));
			_insertCrash.Parameters.Add(new SQLiteParameter("@buildtype", buildType));
			_insertCrash.Parameters.Add(new SQLiteParameter("@errorfriendly", errorFriendly));
			_insertCrash.Parameters.Add(new SQLiteParameter("@encryptionkey", key));
			_insertCrash.Parameters.Add(new SQLiteParameter("@url", webFileLocation));
			_insertCrash.Parameters.Add(new SQLiteParameter("@version", version.ToString()));

			_insertCrash.ExecuteNonQuery();
		}
	}
}