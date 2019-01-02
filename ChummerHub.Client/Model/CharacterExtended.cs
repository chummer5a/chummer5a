using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Backend;
using ChummerHub.Client.UI;
using Newtonsoft.Json;
using SINners.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Chummer.frmCharacterRoster;

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
                    Id = Guid.NewGuid(),
                };
                MySINnerFile.SiNnerMetaData.Tags = new List<Tag>();
            }
            else
            {
                MySINnerFile = JsonConvert.DeserializeObject<SINners.Models.SINner>(fileElement);
            }
            if (MySINnerFile.SiNnerMetaData.Visibility == null)
            {
                if (!String.IsNullOrEmpty(Properties.Settings.Default.SINnerVisibility))
                    MySINnerFile.SiNnerMetaData.Visibility = JsonConvert.DeserializeObject<SINnerVisibility>(Properties.Settings.Default.SINnerVisibility);
            }
            

        }

        public Character MyCharacter { get; }

        public SINner MySINnerFile { get; }

        public string ZipFilePath { get; set; }

        

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

        internal List<Tag> PopulateTags()
        {
            var tag = new Tag();
            tag.Tags = new List<Tag>();
            tag.MyRuntimeObject = MyCharacter;
            tag.MyParentTag = null;
            tag.ParentTagId = Guid.Empty;
            tag.Id = Guid.NewGuid();
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
            return MySINnerFile.SiNnerMetaData.Tags;
        }

        public void PopulateTree(ref TreeNode root, IList<Tag> tags, IList<SearchTag> filtertags)
        {
            if (tags == null)
                tags = MySINnerFile.SiNnerMetaData.Tags;
            if (root == null)
            {
                root = new TreeNode();
                root.Text = "Tags";
                root.Tag = null;
                // get all tags in the list with parent is null
                var onebranch = tags.Where(t => t.MyParentTag == null);
                foreach (var branch in onebranch)
                {
                    var child = new TreeNode()
                    {
                        Text = branch.TagName,
                        Tag = branch.Id,

                    };
                    if (!String.IsNullOrEmpty(branch.TagValue))
                        child.Text += ": " + branch.TagValue;
                    PopulateTree(ref child, branch.Tags, filtertags);
                    root.Nodes.Add(child);
                }
                root.ExpandAll();
            }
            else
            {
                    foreach (var tag in tags)
                    {
                        if (filtertags == null || (filtertags.Any(x => x.TagName == tag.TagName)
                            || (tag.Tags.Any())))
                        {
                            var child = new TreeNode()
                            {
                                Text = tag.TagName,
                                Tag = tag.Id,
                            };
                            if (!String.IsNullOrEmpty(tag.TagValue))
                                child.Text += ": " + tag.TagValue;
                            PopulateTree(ref child, tag.Tags, filtertags);
                            root.Nodes.Add(child);
                        }
                    }
                
            }
        }


        public string PrepareModel()
        {
            MySINnerFile.UploadDateTime = DateTime.Now;

            Guid singuid;
            if (MySINnerIds.TryGetValue(MyCharacter.Alias, out singuid))
                MySINnerFile.Id = singuid;
            else
            {
                MySINnerFile.Id = Guid.NewGuid();
                MySINnerIds.Add(MyCharacter.Alias, MySINnerFile.Id.Value);
                MySINnerIds = MySINnerIds; //Save it!
            }
            MySINnerFile.LastChange = MyCharacter.FileLastWriteTime;
            if (MySINnerFile.SiNnerMetaData.Visibility?.UserRights == null)
            {
                MySINnerFile.SiNnerMetaData.Visibility = new SINnerVisibility();
                MySINnerFile.SiNnerMetaData.Visibility.Groupname = SINnersOptions.SINnerVisibility.Groupname;
                MySINnerFile.SiNnerMetaData.Visibility.IsGroupVisible = SINnersOptions.SINnerVisibility.IsGroupVisible;
                MySINnerFile.SiNnerMetaData.Visibility.IsPublic = SINnersOptions.SINnerVisibility.IsPublic;
                MySINnerFile.SiNnerMetaData.Visibility.UserRights = SINnersOptions.SINnerVisibility.UserRights;
            }


            var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SINner", MySINnerFile.Id.Value.ToString());
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);
            foreach(var file in Directory.GetFiles(tempDir))
            {
                System.IO.File.Delete(file);
            }
            
            var summary = new CharacterCache(MyCharacter.FileName);
            MySINnerFile.JsonSummary = Newtonsoft.Json.JsonConvert.SerializeObject(summary);
            this.MySINnerFile.LastChange = this.MyCharacter.FileLastWriteTime;
            var tempfile = System.IO.Path.Combine(tempDir, summary.FileName);
            if (!File.Exists(tempfile))
            {
                File.Copy(MyCharacter.FileName, tempfile);
            }
            
            string zipPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SINner",  MySINnerFile.Id.Value + ".chum5z");
            if (File.Exists(zipPath))
                File.Delete(zipPath);
            ZipFile.CreateFromDirectory(tempDir, zipPath);
            this.ZipFilePath = zipPath;
            return zipPath;
        }
    }
}
