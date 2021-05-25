using System;
using System.IO;
using System.Threading.Tasks;
using Chummer;

namespace ChummerHub.Client.Sinners
{
    public partial class SINner
    {
        public DateTime DownloadedFromSINnersTime { get; set; }

        public string ZipFilePath
        {
            get
            {
                if (Id == null)
                    return null;
                return Path.Combine(Path.GetTempPath(), "SINner", Id.Value.ToString());
            }
        }

        public string FilePath
        {
            get
            {
                if (Directory.Exists(ZipFilePath))
                {
                    foreach (var file in Directory.EnumerateFiles(ZipFilePath, "*.chum5", SearchOption.TopDirectoryOnly))
                    {
                        DateTime lastwrite = File.GetLastWriteTime(file);
                        if (lastwrite >= LastChange || LastChange == null)
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
                CharacterCache objReturn = new CharacterCache();
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    objReturn.LoadFromFile(strPath);
                else
                    await objReturn.LoadFromFileAsync(strPath);
                return objReturn;
            }
            return null;
        }
    }
}
