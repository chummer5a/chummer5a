using System;
using System.IO;
using Chummer;

namespace SINners.Models
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
            string strPath = FilePath;
            if (!string.IsNullOrEmpty(strPath))
                return new CharacterCache(strPath);

            return null;
        }
    }
}
