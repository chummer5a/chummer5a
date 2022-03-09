using System;
using System.IO;
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

        public CharacterCache GetCharacterCache()
        {
            return GetCharacterCacheCoreAsync(true).GetAwaiter().GetResult();
        }

        public Task<CharacterCache> GetCharacterCacheAsync()
        {
            return GetCharacterCacheCoreAsync(false);
        }

        private async Task<CharacterCache> GetCharacterCacheCoreAsync(bool blnSync)
        {
            string strPath = FilePath;
            if (!string.IsNullOrEmpty(strPath))
            {
                CharacterCache objReturn;
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    objReturn = new CharacterCache(strPath);
                else
                    objReturn = await CharacterCache.CreateFromFileAsync(strPath);
                return objReturn;
            }
            return null;
        }
    }
}
