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
using System.Threading;
using System.Threading.Tasks;
using Chummer;

namespace ChummerHub.Client.Sinners
{
    public partial class SINner
    {
        public DateTime DownloadedFromSINnersTime { get; set; }

        public string ZipFilePath => Id == null ? null : Path.Combine(Path.GetTempPath(), "SINner", Id.Value.ToString());

        public string FilePath
        {
            get
            {
                if (Directory.Exists(ZipFilePath))
                {
                    foreach (string file in Directory.EnumerateFiles(ZipFilePath, "*.chum5", SearchOption.TopDirectoryOnly))
                    {
                        DateTime lastwrite = File.GetLastWriteTime(file);
                        if (lastwrite >= LastChange || LastChange == default)
                        {
                            return file;
                        }
                        File.Delete(file);
                    }
                }

                return string.Empty;
            }
        }

        public CharacterCache GetCharacterCache(CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => GetCharacterCacheCoreAsync(true, token), token);
        }

        public Task<CharacterCache> GetCharacterCacheAsync(CancellationToken token = default)
        {
            return GetCharacterCacheCoreAsync(false, token);
        }

        private async Task<CharacterCache> GetCharacterCacheCoreAsync(bool blnSync, CancellationToken token = default)
        {
            string strPath = FilePath;
            if (!string.IsNullOrEmpty(strPath))
            {
                CharacterCache objReturn;
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    objReturn = new CharacterCache(strPath);
                else
                    objReturn = await CharacterCache.CreateFromFileAsync(strPath, token);
                return objReturn;
            }
            return null;
        }
    }
}
