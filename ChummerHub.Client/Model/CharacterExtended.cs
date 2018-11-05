using Chummer;
using ChummerHub.Client.Backend;
using Newtonsoft.Json;
using SINners.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
            var found = from a in MySINnerFile.SiNnerMetaData.Tags where a.TagName == "Reflection" select a;
            foreach (var f in found)
                MySINnerFile.SiNnerMetaData.Tags.Remove(f);
            MySINnerFile.SiNnerMetaData.Tags.Add(tag);
            
        }

        private static Dictionary<string, Guid> _SINnerIds = null;

        public static Dictionary<string, Guid> MySINnerIds
        {
            get
            {
                if (_SINnerIds == null)
                {
                    string save = ChummerHub.Client.Properties.Settings.Default.SINnerIds;
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
                    ChummerHub.Client.Properties.Settings.Default.SINnerIds = save;
                    ChummerHub.Client.Properties.Settings.Default.Save();
                }
            }
        }

        public void PrepareModel()
        {
            var tempfile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), MyCharacter.FileName);
            MyCharacter.Save(tempfile);
            Byte[] bytes = File.ReadAllBytes(tempfile);
            MySINnerFile.Base64EncodedXmlFile = Convert.ToBase64String(bytes);
            MySINnerFile.ChummerUploadClient = new ChummerUploadClient();
            MySINnerFile.UploadDateTime = DateTime.Now;
           
            if (!Utils.IsInUnitTest)
                MySINnerFile.ChummerUploadClient.ChummerVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            else
                MySINnerFile.ChummerUploadClient.ChummerVersion = System.Reflection.Assembly.GetCallingAssembly().GetName().Version.ToString();
            Guid singuid;
            if (MySINnerIds.TryGetValue(MyCharacter.Alias, out singuid))
                MySINnerFile.SiNnerId = singuid;
            else
            {
                MySINnerFile.SiNnerId = Guid.NewGuid();
                MySINnerIds.Add(MyCharacter.Alias, MySINnerFile.SiNnerId.Value);
                MySINnerIds = MySINnerIds; //Save it!
            }
        }
    }
}
