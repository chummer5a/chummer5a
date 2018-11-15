using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Backend;
using Newtonsoft.Json;
using SINners.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChummerHub.Client.Model
{
    public class CharacterExtended
    {

        public CharacterExtended(Character character, string fileElement)
        {
            MyCharacter = character;
            MySINnerFile = new SINner();
            if (String.IsNullOrEmpty(fileElement))
            {
                MySINnerFile.SiNnerMetaData = new SINnerMetaData()
                {
                    SiNnerMetaDataId = Guid.NewGuid()
                };
                MySINnerFile.SiNnerMetaData.Tags = new List<Tag>();
            }
            else
            {
                MySINnerFile = JsonConvert.DeserializeObject<SINners.Models.SINner>(fileElement);
            }
        }

        public Character MyCharacter { get; }

        public SINner MySINnerFile { get; }

        public string ZipFilePath { get; set; }

        internal void PopulateTags()
        {
            var tag = new Tag();
            tag.Tags = new List<Tag>();
            tag.MyRuntimeObject = MyCharacter;
            tag.MyParentTag = null;
            tag.ParentTagId = Guid.Empty;
            tag.TagId = Guid.NewGuid();
            tag.TagName = "Reflection";
            //Backend.TagExtractor.MyReflectionCollection = null;

            var tags = Backend.TagExtractor.ExtractTagsFromAttributes(MyCharacter, tag);
            //var tags = Backend.TagExtractor.ExtractTags(MyCharacter, 3, tag);
            var found = from a in MySINnerFile.SiNnerMetaData.Tags.ToList() where a.TagName == "Reflection" select a;
            foreach (var f in found)
            {
                MySINnerFile.SiNnerMetaData.Tags.Remove(f);
            }
            MySINnerFile.SiNnerMetaData.Tags.Add(tag);
            
        }

        private static Dictionary<string, Guid> _SINnerIds = null;

        public static Dictionary<string, Guid> MySINnerIds
        {
            get
            {
                if (_SINnerIds == null)
                {
                    string save = Properties.Settings.Default.SINnerIds.ToString();
                    if (!String.IsNullOrEmpty(save))
                        _SINnerIds = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Guid>>(save);
                    else
                        _SINnerIds = new Dictionary<string, Guid>();
                }
                return _SINnerIds;
            }
            set
            {
                _SINnerIds = value;
                if (_SINnerIds != null)
                {
                    string save = Newtonsoft.Json.JsonConvert.SerializeObject(_SINnerIds);
                    Properties.Settings.Default.SINnerIds = save;
                    Properties.Settings.Default.Save();
                }
            }
        }

        public string PrepareModel()
        {
            MySINnerFile.UploadDateTime = DateTime.Now;

            Guid singuid;
            if (MySINnerIds.TryGetValue(MyCharacter.Alias, out singuid))
                MySINnerFile.SiNnerId = singuid;
            else
            {
                MySINnerFile.SiNnerId = Guid.NewGuid();
                MySINnerIds.Add(MyCharacter.Alias, MySINnerFile.SiNnerId.Value);
                MySINnerIds = MySINnerIds; //Save it!
            }

            var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), MySINnerFile.SiNnerId.Value.ToString());
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);
            foreach(var file in Directory.GetFiles(tempDir))
            {
                System.IO.File.Delete(file);
            }
            var tempfile = System.IO.Path.Combine(tempDir, MyCharacter.FileName);
            MyCharacter.Save(tempfile);
            //Byte[] bytes = File.ReadAllBytes(tempfile);
            
            string zipPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), MySINnerFile.SiNnerId.Value + ".chum5z");
            if (File.Exists(zipPath))
                File.Delete(zipPath);
            ZipFile.CreateFromDirectory(tempDir, zipPath);
            return zipPath;
        }
    }
}
