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
using Amazon.Runtime;

namespace ChummerDataViewer.Model
{
    internal static class PersistentState
    {
        public static Database Database { get; private set; }

        public static string BulkFolder => _path.Value;
        public static string DatabaseFolder => _dbFolder.Value;

        public static bool Setup => Database != null;
        public static AWSCredentials AWSCredentials => Setup ? _awsCredentials.Value : null;

        public static void Initialize(string id, string key, string bulkStoragePath)
        {
            if (Database != null) return;

            Directory.CreateDirectory(DatabaseFolder);

            Database = Database.Create();
            Database.SetKey("crashdumps_aws_id", id);
            Database.SetKey("crashdumps_aws_key", key);
            Database.SetKey("crashdumps_zip_folder", bulkStoragePath);

            Directory.CreateDirectory(BulkFolder);
        }

        static PersistentState()
        {
            if (File.Exists(Path.Combine(DatabaseFolder, "persistent.db")))
            {
                Database = new Database();
            }
        }

        private static readonly Lazy<string> _path = new Lazy<string>(() => Database.GetKey("crashdumps_zip_folder"));

        private static readonly Lazy<string> _dbFolder =
            new Lazy<string>(
                () =>
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "ChummerCrashDump"));

        private static readonly Lazy<AWSCredentials> _awsCredentials =
            new Lazy<AWSCredentials>(() => new BasicAWSCredentials(Database.GetKey("crashdumps_aws_id"), Database.GetKey("crashdumps_aws_key")));
    }
}
