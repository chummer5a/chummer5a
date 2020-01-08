using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chummer;

namespace SINners.Models
{
    public partial class SINner
    {
        public DateTime DownloadedFromSINnersTime { get;
            set; }


        public string ZipFilePath
        {
            get
            {
                if (this.Id == null)
                    return null;
                string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SINner", this.Id.Value.ToString());
                return path;
            }
        }

        public string FilePath
        {
            get
            {
                string loadFilePath = null;
                if (Directory.Exists(this.ZipFilePath))
                {
                    var files = Directory.EnumerateFiles(this.ZipFilePath, "*.chum5", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        DateTime lastwrite = File.GetLastWriteTime(file);
                        if ((lastwrite >= this.LastChange)
                            || this.LastChange == null)
                        {
                            loadFilePath = file;
                            return loadFilePath;
                            break;
                        }
                        File.Delete(file);
                    }
                }
                return loadFilePath;
            }
        }

        public frmCharacterRoster.CharacterCache GetCharacterCache()
        {
            if (this.FilePath != null)
            {
                frmCharacterRoster.CharacterCache ret = new frmCharacterRoster.CharacterCache(this.FilePath);
                return ret;
            }

            return null;
        }

    }
}
