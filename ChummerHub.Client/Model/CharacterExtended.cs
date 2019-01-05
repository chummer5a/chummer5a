using Chummer;
using ChummerHub.Client.Backend;
using ChummerHub.Client.UI;
using Newtonsoft.Json;
using SINners.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace ChummerHub.Client.Model
{
    public class CharacterExtended
    {
        public CharacterExtended(Character character, string fileElement = null)
        {
            MyCharacter = character;
            MySINnerFile = new SINner();
            if (string.IsNullOrEmpty(fileElement))
            {
                MySINnerFile.SiNnerMetaData = new SINnerMetaData
                {
                    Id = Guid.NewGuid(),
                    Tags = new List<Tag>(),
                };
            }
            else
            {
                MySINnerFile = JsonConvert.DeserializeObject<SINner>(fileElement);
            }

            if (MySINnerFile.SiNnerMetaData.Visibility != null) return;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SINnerVisibility))
                MySINnerFile.SiNnerMetaData.Visibility =
                    JsonConvert.DeserializeObject<SINnerVisibility>(Properties.Settings.Default.SINnerVisibility);
        }

        public Character MyCharacter { get; }

        // ReSharper disable once InconsistentNaming
        public SINner MySINnerFile { get; }

        public string ZipFilePath { get; set; }


        // ReSharper disable once InconsistentNaming
        private static Dictionary<string, Guid> _SINnerIds;

        // ReSharper disable once InconsistentNaming
        public static Dictionary<string, Guid> MySINnerIds
        {
            get
            {
                if (_SINnerIds != null) return _SINnerIds;
                string save = Properties.Settings.Default.SINnerIds;
                _SINnerIds = !string.IsNullOrEmpty(save)
                    ? JsonConvert.DeserializeObject<Dictionary<string, Guid>>(save)
                    : new Dictionary<string, Guid>();
                return _SINnerIds;
            }
            set
            {
                _SINnerIds = value;
                if (_SINnerIds == null) return;
                string save = JsonConvert.SerializeObject(_SINnerIds);
                Properties.Settings.Default.SINnerIds = save;
                Properties.Settings.Default.Save();
            }
        }

        internal List<Tag> PopulateTags()
        {
            var tag = new Tag
            {
                Tags = new List<Tag>(),
                MyRuntimeObject = MyCharacter,
                MyParentTag = null,
                ParentTagId = Guid.Empty,
                Id = Guid.NewGuid(),
                TagName = "Reflection"
            };
            //Backend.TagExtractor.MyReflectionCollection = null;

            var tags = TagExtractor.ExtractTagsFromAttributes(MyCharacter, tag);
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
                root = new TreeNode
                {
                    Text = "Tags",
                    Tag = null
                };
                // get all tags in the list with parent is null
                var onebranch = tags.Where(t => t.MyParentTag == null);
                foreach (var branch in onebranch)
                {
                    var child = new TreeNode
                    {
                        Text = branch.TagName,
                        Tag = branch.Id,
                    };
                    if (!string.IsNullOrEmpty(branch.TagValue))
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
                    if (filtertags != null && (filtertags.All(x => x.TagName != tag.TagName) && (!tag.Tags.Any())))
                        continue;
                    var child = new TreeNode()
                    {
                        Text = tag.TagName,
                        Tag = tag.Id,
                    };
                    if (!string.IsNullOrEmpty(tag.TagValue))
                        child.Text += ": " + tag.TagValue;
                    PopulateTree(ref child, tag.Tags, filtertags);
                    root.Nodes.Add(child);
                }
            }
        }


        public string PrepareModel()
        {
            MySINnerFile.UploadDateTime = DateTime.Now;

            if (MySINnerIds.TryGetValue(MyCharacter.Alias, out var singuid))
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
                MySINnerFile.SiNnerMetaData.Visibility =
                    new SINnerVisibility
                    {
                        Groupname = SINnersOptions.SINnerVisibility.Groupname,
                        IsGroupVisible = SINnersOptions.SINnerVisibility.IsGroupVisible,
                        IsPublic = SINnersOptions.SINnerVisibility.IsPublic,
                        UserRights = SINnersOptions.SINnerVisibility.UserRights
                    };
            }


            var tempDir = Path.Combine(Path.GetTempPath(), "SINner", MySINnerFile.Id.Value.ToString());
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);
            foreach (var file in Directory.GetFiles(tempDir))
            {
                File.Delete(file);
            }

            var summary = new frmCharacterRoster.CharacterCache(MyCharacter.FileName);
            MySINnerFile.JsonSummary = JsonConvert.SerializeObject(summary);
            MySINnerFile.LastChange = MyCharacter.FileLastWriteTime;
            var tempfile = Path.Combine(tempDir, summary.FileName);
            if (!File.Exists(tempfile))
            {
                File.Copy(MyCharacter.FileName, tempfile);
            }

            string zipPath = Path.Combine(Path.GetTempPath(), "SINner", MySINnerFile.Id.Value + ".chum5z");
            if (File.Exists(zipPath))
                File.Delete(zipPath);
            ZipFile.CreateFromDirectory(tempDir, zipPath);
            ZipFilePath = zipPath;
            return zipPath;
        }
    }
}
