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
using Chummer.Plugins;
using Microsoft.Rest;
using NLog;
using Utils = Chummer.Utils;

namespace ChummerHub.Client.Model
{
    public class CharacterExtended
    {
        private Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public CharacterExtended(Character character, string fileElement = null)
        {
            MyCharacter = character;
            
            if (!string.IsNullOrEmpty(fileElement))
            {
                try
                {
                    _MySINnerFile = JsonConvert.DeserializeObject<SINner>(fileElement);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            if (_MySINnerFile == null)
            {
                _MySINnerFile = new SINner
                {
                    Language = GlobalOptions.Language
                };
                
                //_MySINnerFile.MyExtendedAttributes = new SINnerExtended(_MySINnerFile);
            }
            if (MySINnerFile.SiNnerMetaData == null)
            {
                _MySINnerFile.SiNnerMetaData = new SINnerMetaData
                {
                    Id = Guid.NewGuid(),
                    Tags = new List<Tag>(),
                    Visibility = new SINners.Models.SINnerVisibility()
                    {
                        IsGroupVisible = true,
                        IsPublic = true
                    }
                };
            }
            if(MySINnerFile.SiNnerMetaData.Visibility != null)
            {
                if(MySINnerFile.SiNnerMetaData.Visibility.Id == null)
                    MySINnerFile.SiNnerMetaData.Visibility.Id = Guid.NewGuid();
            }
            
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SINnerVisibility))
                MySINnerFile.SiNnerMetaData.Visibility =
                    JsonConvert.DeserializeObject<SINners.Models.SINnerVisibility>(Properties.Settings.Default.SINnerVisibility);

            if(MySINnerFile.SiNnerMetaData.Visibility?.Id == null)
                if (MySINnerFile?.SiNnerMetaData?.Visibility != null)
                    MySINnerFile.SiNnerMetaData.Visibility.Id = Guid.NewGuid();

            var cache = new frmCharacterRoster.CharacterCache(character.FileName);
        
            this.MySINnerFile.LastChange = MyCharacter.FileLastWriteTime;

            if(MySINnerIds.TryGetValue(MyCharacter.FileName, out var singuid))
                MySINnerFile.Id = singuid;
            else
            {
                MySINnerFile.Id = Guid.NewGuid();
                MySINnerIds.Add(MyCharacter.FileName, MySINnerFile.Id.Value);
                MySINnerIds = MySINnerIds; //Save it!
            }
        }

        public CharacterExtended(Character character, string fileElement = null, SINner mySINnerLoading = null) : this(character, fileElement)
        {
            if (mySINnerLoading != null)
            {
                var backup = this._MySINnerFile;
                this._MySINnerFile = mySINnerLoading;
                if ((this._MySINnerFile?.SiNnerMetaData?.Id == null)
                    || (this._MySINnerFile.SiNnerMetaData?.Id == Guid.Empty))
                {
                    if (this._MySINnerFile?.SiNnerMetaData != null)
                        this._MySINnerFile.SiNnerMetaData.Id = backup.SiNnerMetaData.Id;
                }

                if (this._MySINnerFile?.SiNnerMetaData?.Tags?.Count() < backup?.SiNnerMetaData?.Tags?.Count())
                    this._MySINnerFile.SiNnerMetaData.Tags = backup.SiNnerMetaData.Tags;

            }
                
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
            _MySINnerFile.DownloadedFromSINnersTime = DateTime.Now.ToUniversalTime();
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
            var found = (from a in MySINnerFile.SiNnerMetaData.Tags.ToList() where a != null && a.TagName == "Reflection" select a).ToList();
            foreach (var f in found)
            {
                MySINnerFile.SiNnerMetaData.Tags.Remove(f);
            }
            MySINnerFile.SiNnerMetaData.Tags.Add(tag);
            foreach(var childtag in MySINnerFile.SiNnerMetaData.Tags)
            {
                childtag?.SetSinnerIdRecursive(MySINnerFile.Id);
            }
            return MySINnerFile.SiNnerMetaData.Tags.ToList();
        }

        public async Task<bool> Upload(ucSINnerShare.MyUserState myState = null)
        {
            try
            {
                using (new CursorWait(true, PluginHandler.MainForm))
                {
                    if (myState != null)
                    {
//1 Step
                        myState.CurrentProgress += myState.ProgressSteps;
                        myState.StatusText = "Checking online version of file...";
                        myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                    }

                    if (MySINnerFile.DownloadedFromSINnersTime > this.MyCharacter.FileLastWriteTime)
                    {
                        if (myState != null)
                        {
                            myState.CurrentProgress += 4 * myState.ProgressSteps;
                            myState.StatusText = "File already uploaded.";
                            myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                        }
                        return true;
                    }

                    var client = StaticUtils.GetClient();
                    var found = await client.GetSINByIdWithHttpMessagesAsync(this.MySINnerFile.Id.Value);
                    await Backend.Utils.HandleError(found, found.Body);
                    if (myState != null)
//2 Step
                        myState.CurrentProgress += myState.ProgressSteps;
                    if (found.Response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                       if (found.Body.MySINner.LastChange >= this.MyCharacter.FileLastWriteTime)
                       {
                           if (myState != null)
                           {
                               myState.StatusText = "SINner already uploaded and updated online.";
                               myState.CurrentProgress += 3 * myState.ProgressSteps;
                               myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                           }
                           return true;
                       }
                       if (myState != null)
                       {
                           myState.StatusText = "SINner needs to be uploaded.";
                           myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                       }
                        if (!MySINnerFile.SiNnerMetaData.Visibility.UserRights.Any())
                       {
                           MySINnerFile.SiNnerMetaData.Visibility.UserRights =
                               found.Body.MySINner.SiNnerMetaData.Visibility.UserRights;
                       }
                    }

                    this.MySINnerFile.SiNnerMetaData.Tags = this.PopulateTags();
                    await this.PrepareModel();
                    if (myState != null)
                    {
//3 Step
                        myState.CurrentProgress += myState.ProgressSteps;
                        myState.StatusText = "Chummerfile prepared for uploading...";
                        myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                    }
                    var res = await ChummerHub.Client.Backend.Utils.PostSINnerAsync(this);
                    await Backend.Utils.HandleError(res, res.Body);
                    if (myState != null)
                    {
//4 Step
                        myState.CurrentProgress += myState.ProgressSteps;
                        myState.StatusText = "Chummer Metadata stored in DB...";
                        myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                    }
                    if (res.Response.IsSuccessStatusCode)
                    {
                        var uploadres = await ChummerHub.Client.Backend.Utils.UploadChummerFileAsync(this);
                        if (uploadres.Response.IsSuccessStatusCode)
                        {
                            if (myState != null)
                            {
//5 Step
                                myState.CurrentProgress += myState.ProgressSteps;
                                myState.StatusText = "Chummer uploaded...";
                                myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                            }
                            return true;
                        }
                        if (myState != null)
                        {
//5 Step
                            myState.CurrentProgress += myState.ProgressSteps;
                            myState.StatusText = "Chummer upload failed: " + uploadres.Response.ReasonPhrase;
                            myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                        }
                        return false;
                    }
                    if (myState != null)
                    {
//5 Step
                        myState.CurrentProgress += myState.ProgressSteps;
                        myState.StatusText = "Chummer upload of Metadata failed: " + res.Response.ReasonPhrase;
                        myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                    }
                    return false;
                }
            }
            catch(Exception e)
            {
                await Backend.Utils.HandleError(e);
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
                var onebranch = tags.Where(t => t != null && t.MyParentTag == null);
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


        public async Task<string> PrepareModel()
        {
            string zipPath = Path.Combine(Properties.Settings.Default.TempDownloadPath, "SINner", MySINnerFile.Id + ".chum5z");
            if (PluginHandler.MySINnerLoading != null)
            {
                if (MySINnerIds.ContainsKey(MyCharacter.FileName))
                    MySINnerIds.Remove(MyCharacter.FileName);
            }
            if (MySINnerIds.TryGetValue(MyCharacter.FileName, out var singuid))
                MySINnerFile.Id = singuid;
            else
            {
                if (PluginHandler.MySINnerLoading != null)
                {
                    _MySINnerFile = PluginHandler.MySINnerLoading;
                    MySINnerIds.Add(MyCharacter.FileName, MySINnerFile.Id.Value);
                    if (File.Exists(zipPath))
                        return zipPath;
                }
                else
                {
                    if (String.IsNullOrEmpty(MySINnerFile.Alias))
                    {
                        MySINnerFile.Alias = MyCharacter.Alias;
                        if (String.IsNullOrEmpty(MySINnerFile.Alias))
                            MySINnerFile.Alias = MyCharacter.Name;
                    }
                    var client = StaticUtils.GetClient();
                    HttpOperationResponse<ResultSinnerGetOwnedSINByAlias> res = null;
                    try
                    {
                        res = await client.SinnerGetOwnedSINByAliasWithHttpMessagesAsync(MySINnerFile.Alias);
                    }
                    catch (SerializationException e)
                    {
                        e.Data.Add("Alias", MySINnerFile.Alias);
                        e.Data.Add("MySINnerFile.Id", MySINnerFile.Id);
                        e.Data.Add("User", ChummerHub.Client.Properties.Settings.Default.UserEmail);
                        e.Data.Add("ResponseContent", e.Content);
                        throw;
                    }
                    catch (Exception e)
                    {
                        e.Data.Add("Alias", MySINnerFile.Alias);
                        e.Data.Add("MySINnerFile.Id", MySINnerFile.Id);
                        e.Data.Add("User", ChummerHub.Client.Properties.Settings.Default.UserEmail);
                        throw;
                    }
                    if (res.Response.StatusCode == HttpStatusCode.NotFound)
                    {
                        MySINnerFile.Id = Guid.NewGuid();
                    }
                    else if (res.Response.StatusCode == HttpStatusCode.OK)
                    {
                        string message = MySINnerFile.Alias +
                                         " is already available online, but the current client uses a local version of that file." +
                                         Environment.NewLine + Environment.NewLine;
                        message += "Do you want to use only one version and update it through SINner?" +
                                   Environment.NewLine + Environment.NewLine;
                        message += "YES:\tuse the same (= only one) version and get" +
                                   Environment.NewLine;
                        message += "\tupdates on this client when you save" + Environment.NewLine;
                        message += "\tthe chummer on another client(recommended)" + Environment.NewLine
                                                                            + Environment.NewLine;
                        message +=
                            "NO:\tgenerate a new \"Fork\" of this character," +
                            Environment.NewLine;
                        message += "\tthat is not linked to the online one" + Environment.NewLine;
                        message +=
                            "\tand use this new version from now on," + Environment.NewLine;
                        message += "but only on this client (NOT recommended)."
                                   + Environment.NewLine + Environment.NewLine;
                        var result = MessageBox.Show(message, "SIN already found online", MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Cancel)
                            throw new ArgumentException("User aborted perparation for upload!");
                        else if (result == DialogResult.No)
                        {
                            MySINnerFile.Id = Guid.NewGuid();
                        }
                        else
                        {
                            var list = res.Body.MySINners;
                            foreach (var sin in list)
                            {
                                if (sin.Id != null)
                                {
                                    MySINnerFile.Id = sin.Id;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (MySINnerIds.ContainsKey(MyCharacter.FileName))
                    MySINnerIds.Remove(MyCharacter.FileName);
                MySINnerIds.Add(MyCharacter.FileName, MySINnerFile.Id.Value);
                MySINnerIds = MySINnerIds; //Save it!
            }
            zipPath = Path.Combine(Path.GetTempPath(), "SINner", MySINnerFile.Id.Value + ".chum5z");
            MySINnerFile.UploadDateTime = DateTime.Now;
            


            MySINnerFile.Alias = MyCharacter.CharacterName;
            MySINnerFile.LastChange = MyCharacter.FileLastWriteTime;
            if (MySINnerFile.SiNnerMetaData.Visibility?.UserRights == null)
            {
                MySINnerFile.SiNnerMetaData.Visibility =
                    new SINners.Models.SINnerVisibility
                    {
                        Id = Guid.NewGuid(),
                        IsGroupVisible = ucSINnersOptions.SINnerVisibility.IsGroupVisible,
                        IsPublic = ucSINnersOptions.SINnerVisibility.IsPublic,
                        UserRights = ucSINnersOptions.SINnerVisibility.UserRights
                    };
            }
            if (MySINnerFile.SiNnerMetaData.Visibility.Id == ucSINnersOptions.SINnerVisibility.Id)
            {
                //make the visibility your own and dont reuse the id from the general options!
                MySINnerFile.SiNnerMetaData.Visibility.Id = Guid.NewGuid();
            }
            foreach(var visnow in ucSINnersOptions.SINnerVisibility.UserRights)
            {
                if (!MySINnerFile.SiNnerMetaData.Visibility.UserRights.Any(a => a.EMail.ToLowerInvariant() == visnow.EMail.ToLowerInvariant()))
                {
                    MySINnerFile.SiNnerMetaData.Visibility.UserRights.Add(visnow);
                }
            }
            
            foreach(var ur in MySINnerFile.SiNnerMetaData.Visibility.UserRights)
            {
                if (ucSINnersOptions.SINnerVisibility.UserRights.Any(a => a.Id == ur.Id))
                {
                    ur.Id = Guid.NewGuid();
                }
            }

           
                

            var tempDir = Path.Combine(Path.GetTempPath(), "SINner", MySINnerFile.Id.Value.ToString());
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);
            foreach(var file in Directory.GetFiles(tempDir))
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastWriteTimeUtc < MyCharacter.FileLastWriteTime)
                    File.Delete(file);
            }
            var summary = new frmCharacterRoster.CharacterCache(MyCharacter.FileName);
            //MySINnerFile.MyExtendedAttributes.JsonSummary = JsonConvert.SerializeObject(summary);
            MySINnerFile.LastChange = MyCharacter.FileLastWriteTime;
            if (String.IsNullOrEmpty(summary.FileName))
                return null;
            var tempfile = Path.Combine(tempDir, summary.FileName);
            if (File.Exists(tempfile))
                File.Delete(tempfile);

            bool readCallback = false;
            if (MyCharacter.OnSaveCompleted != null)
            {
                readCallback = true;
                MyCharacter.OnSaveCompleted = null;
            }

            if (!File.Exists(MyCharacter.FileName))
            {
                string path2 = MyCharacter.FileName.Substring(0, MyCharacter.FileName.LastIndexOf('\\'));
                CreateDirectoryRecursively(path2);
                MyCharacter.Save(MyCharacter.FileName, false, false);
            }

            //string path = //Path.Combine(ChummerHub.Client.Properties.Settings.Default.TempDownloadPath, 
            //    MyCharacter.FileName.Substring(0, MyCharacter.FileName.LastIndexOf('\\'));

            //CreateDirectoryRecursively(path);
            MyCharacter.Save(tempfile, false, false);
            if (readCallback)
                MyCharacter.OnSaveCompleted += PluginHandler.MyOnSaveUpload;

            if (File.Exists(zipPath))
            {
                try
                {
                    File.Delete(zipPath);
                }
                catch (IOException e)
                {
                    Log.Warn(e, "Could not delete File " + zipPath + ": " + e.Message);
                }
            }

            if (!File.Exists(zipPath))
            {
                ZipFile.CreateFromDirectory(tempDir, zipPath);
                ZipFilePath = zipPath;
            }

            return zipPath;
        }

        void CreateDirectoryRecursively(string path)
        {
            
                string[] pathParts = path.Split('\\');

                for (int i = 0; i < pathParts.Length; i++)
                {
                    if (i > 0)
                        pathParts[i] = Path.Combine(pathParts[i - 1], pathParts[i]);

                    if (!Directory.Exists(pathParts[i]))
                    {
                        try
                        {
                            Directory.CreateDirectory(pathParts[i]);
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            throw new UnauthorizedAccessException("Path: " + pathParts[i], e);
                        }
                    }
                }
            
         
        }


        public async Task<bool> UploadInBackground(ucSINnerShare.MyUserState myState)
        {
            try
            {
                using (new CursorWait(true))
                {
                    if (myState != null)
                    {
                        myState.StatusText = "Preparing file to upload...";
                        myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                    }
                    this.PopulateTags();
                    await this.PrepareModel();
                    if (myState != null)
                    {
                        myState.CurrentProgress += myState.ProgressSteps;
                        myState.StatusText = "Uploading Metadata...";
                        myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                    }

                    ResultSinnerPostSIN response = ChummerHub.Client.Backend.Utils.PostSINner(this);
                    if (response?.CallSuccess == true)
                    {
                        try
                        {
                            try
                            {
                                this.MySINnerFile.Id = response.MySINners.FirstOrDefault().Id;
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex);
                                throw;
                            }
                            Log.Debug("Character " + this.MyCharacter.Alias + " posted with ID " + this.MySINnerFile.Id);
                            if (myState != null)
                            {
                                myState.CurrentProgress += myState.ProgressSteps;
                                myState.StatusText = "Uploading Filedata...";
                                myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                            }

                            var uploadresult = ChummerHub.Client.Backend.Utils.UploadChummer(this);
                            
                            if (uploadresult.CallSuccess != true)
                            {
                                if (myState != null)
                                {
                                    myState.CurrentProgress += 3 * myState.ProgressSteps;
                                    myState.StatusText = "Failed uploading Filedata: " + uploadresult.ErrorText;
                                    myState.StatusText += Environment.NewLine + uploadresult.MyException;
                                    myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                                }
                                if (uploadresult.MyException is Exception aException)
                                    throw aException;
                                return false;
                            }
                            else
                            {
                                string msg = "Character " + this.MyCharacter.Alias + " uploaded with Id " +
                                             this.MySINnerFile.Id;
                                Log.Trace(msg);
                                if (myState != null)
                                {
                                    myState.CurrentProgress += 3 * myState.ProgressSteps;
                                    myState.StatusText = msg;
                                    myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                                }
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                            throw;
                        }
                    }
                    else
                    {
                        if (myState != null)
                        {
                            myState.CurrentProgress += 4 * myState.ProgressSteps;
                            myState.StatusText = "Could not uploading Metadata: " + response?.ErrorText;
                            myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                        }
                    }
         
                }
            }
            catch(Exception e)
            {
                Log.Error(e);
                MessageBox.Show(e.ToString());
            }
            return true;
        }

    }
}
