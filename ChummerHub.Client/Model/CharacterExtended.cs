using Chummer;
using ChummerHub.Client.Backend;
using ChummerHub.Client.UI;
using Newtonsoft.Json;
using SINners.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChummerHub.Client.Model
{
    public class CharacterExtended
    {
        public CharacterExtended(Character character, string fileElement = null)
        {
            MyCharacter = character;
            _MySINnerFile = new SINner();
            if (string.IsNullOrEmpty(fileElement))
            {
                MySINnerFile.SiNnerMetaData = new SINnerMetaData
                {
                    Id = Guid.NewGuid(),
                    Tags = new List<Tag>(),
                    Visibility = new SINnerVisibility()
                };
            }
            else
            {
                try
                {
                    MySINnerFile.SiNnerMetaData = JsonConvert.DeserializeObject<SINnerMetaData>(fileElement);
                }
                catch(Exception e)
                {
                    System.Diagnostics.Trace.TraceWarning(e.ToString());
                }
            }

            if(MySINnerFile.SiNnerMetaData.Visibility != null)
            {
                if(MySINnerFile.SiNnerMetaData.Visibility.Id == null)
                    MySINnerFile.SiNnerMetaData.Visibility.Id = Guid.NewGuid();
            }
            
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SINnerVisibility))
                MySINnerFile.SiNnerMetaData.Visibility =
                    JsonConvert.DeserializeObject<SINnerVisibility>(Properties.Settings.Default.SINnerVisibility);

            if(MySINnerFile.SiNnerMetaData.Visibility?.Id == null)
                MySINnerFile.SiNnerMetaData.Visibility.Id = Guid.NewGuid();

            var cache = new frmCharacterRoster.CharacterCache(character.FileName);
        
            this.MySINnerFile.LastChange = MyCharacter.FileLastWriteTime;

            if(MySINnerIds.TryGetValue(MyCharacter.Alias, out var singuid))
                MySINnerFile.Id = singuid;
            else
            {
                MySINnerFile.Id = Guid.NewGuid();
                MySINnerIds.Add(MyCharacter.Alias, MySINnerFile.Id.Value);
                MySINnerIds = MySINnerIds; //Save it!
            }
            this.MySINnerFile.JsonSummary = JsonConvert.SerializeObject(cache);
        }

        public Character MyCharacter { get; set; }

        private SINner _MySINnerFile = null;

        // ReSharper disable once InconsistentNaming
        public SINner MySINnerFile
        {
            get { return _MySINnerFile; }
        }

        public void SetSINner(SINner sinner)
        {
            _MySINnerFile = sinner;
        }

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
            var tags = TagExtractor.ExtractTagsFromAttributes(MyCharacter, tag);
            var found = from a in MySINnerFile.SiNnerMetaData.Tags.ToList() where a.TagName == "Reflection" select a;
            foreach (var f in found)
            {
                MySINnerFile.SiNnerMetaData.Tags.Remove(f);
            }
            MySINnerFile.SiNnerMetaData.Tags.Add(tag);
            foreach(var childtag in MySINnerFile.SiNnerMetaData.Tags)
            {
                childtag.SetSinnerIdRecursive(MySINnerFile.Id);
            }
            return MySINnerFile.SiNnerMetaData.Tags.ToList();
        }

        public async Task<bool> Upload()
        {
            try
            {


                var found = await StaticUtils.Client.GetSINByIdWithHttpMessagesAsync(this.MySINnerFile.Id.Value);
                if(found.Response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var sinjson = await found.Response.Content.ReadAsStringAsync();
                    var foundobj = Newtonsoft.Json.JsonConvert.DeserializeObject<SINner>(sinjson);
                    SINner foundsin = foundobj as SINner;
                    if(foundsin.LastChange >= this.MyCharacter.FileLastWriteTime)
                    {
                        //is already up to date!
                        return true;
                    }

                }
                this.MySINnerFile.SiNnerMetaData.Tags = this.PopulateTags();
                this.PrepareModel();
                await ChummerHub.Client.Backend.Utils.PostSINnerAsync(this);
                await ChummerHub.Client.Backend.Utils.UploadChummerFileAsync(this);
                return true;
            }
            catch(Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.Message, e);
                Debug.WriteLine(e.ToString());
                throw;
            }
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
                    if(!string.IsNullOrEmpty(branch.TagComment))
                        child.ToolTipText = branch.TagComment;
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
                        Text = "",
                        Tag = tag.Id
                    };
                    if(!string.IsNullOrEmpty(tag.TagComment))
                        child.Text = "(" + tag.TagComment + ") ";
                    child.Text += tag.TagName;
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

            MySINnerFile.Alias = MyCharacter.CharacterName;
            MySINnerFile.LastChange = MyCharacter.FileLastWriteTime;
            if (MySINnerFile.SiNnerMetaData.Visibility?.UserRights == null)
            {
                MySINnerFile.SiNnerMetaData.Visibility =
                    new SINnerVisibility
                    {
                        Id = Guid.NewGuid(),
                        IsGroupVisible = SINnersOptions.SINnerVisibility.IsGroupVisible,
                        IsPublic = SINnersOptions.SINnerVisibility.IsPublic,
                        UserRights = SINnersOptions.SINnerVisibility.UserRights
                    };
            }
            if (MySINnerFile.SiNnerMetaData.Visibility.Id == SINnersOptions.SINnerVisibility.Id)
            {
                //make the visibility your own and dont reuse the id from the general options!
                MySINnerFile.SiNnerMetaData.Visibility.Id = Guid.NewGuid();
            }
            foreach(var ur in MySINnerFile.SiNnerMetaData.Visibility.UserRights)
            {
                if (SINnersOptions.SINnerVisibility.UserRights.Any(a => a.Id == ur.Id))
                {
                    ur.Id = Guid.NewGuid();
                }
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

   
        public void UploadInBackground()
        {
            this.PopulateTags();
            this.PrepareModel();
            ChummerHub.Client.Backend.Utils.PostSINnerAsync(this).ContinueWith((posttask) =>
            {
                if(posttask.Status != TaskStatus.RanToCompletion)
                {
                    if(posttask.Exception != null)
                        throw posttask.Exception;
                    return;
                }
                var jsonResultString = posttask.Result.Response.Content.ReadAsStringAsync().Result;
                try
                {
                    Object objIds = Newtonsoft.Json.JsonConvert.DeserializeObject<Object>(jsonResultString);
                    System.Collections.ICollection ids = objIds as System.Collections.ICollection;
                    if(ids == null || ids.Count == 0)
                    {
                        string msg = "ChummerHub did not return a valid Id for sinner " + this.MyCharacter.Name + ".";
                        System.Diagnostics.Trace.TraceError(msg);
                        throw new ArgumentException(msg);
                    }
                    var cur = ids.GetEnumerator();
                    cur.MoveNext();
                    if(!Guid.TryParse(cur.Current.ToString(), out var sinGuid))
                    {
                        string msg = "ChummerHub did not return a valid IdArray for sinner " + this.MyCharacter.Alias + ".";
                        System.Diagnostics.Trace.TraceError(msg);
                        throw new ArgumentException(msg);
                    }
                    this.MySINnerFile.Id = sinGuid;
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.ToString());
                    throw;
                }
                System.Diagnostics.Trace.TraceInformation("Character " + this.MyCharacter.Alias + " posted with ID " + this.MySINnerFile.Id);
                ChummerHub.Client.Backend.Utils.UploadChummerFileAsync(this).ContinueWith((uploadtask) =>
                {
                    if(uploadtask.Status != TaskStatus.RanToCompletion)
                    {
                        if(uploadtask.Exception != null)
                            throw uploadtask.Exception;
                        return;
                    }
                    if(uploadtask.Result?.Response?.StatusCode != HttpStatusCode.OK)
                    {
                        System.Diagnostics.Trace.TraceWarning("Upload failed for Character " + this.MyCharacter.Alias + ": " + uploadtask.Result?.Response?.StatusCode);
                    }
                    else
                        System.Diagnostics.Trace.TraceInformation("Character " + this.MyCharacter.Alias + " uploaded.");
                });
            }).ConfigureAwait(false);
        }

    }
}
