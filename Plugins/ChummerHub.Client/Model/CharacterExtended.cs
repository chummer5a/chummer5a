using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Backend;
using ChummerHub.Client.Properties;
using ChummerHub.Client.UI;
using Microsoft.Rest;
using Newtonsoft.Json;
using NLog;
using SINners.Models;
using Utils = ChummerHub.Client.Backend.Utils;

namespace ChummerHub.Client.Model
{
    public sealed class CharacterExtended : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public CharacterExtended(Character character, string fileElement = null, CharacterCache myCharacterCache = null)
        {
            MyCharacter = character ?? throw new ArgumentNullException(nameof(character));
            MyCharacterCache = myCharacterCache ?? new CharacterCache(MyCharacter.FileName);
            if (!string.IsNullOrEmpty(fileElement))
            {
                try
                {
                    MySINnerFile = JsonConvert.DeserializeObject<SINner>(fileElement);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            if (MySINnerFile == null)
            {
                MySINnerFile = new SINner
                {
                    Language = GlobalOptions.Language
                };

                //MySINnerFile.MyExtendedAttributes = new SINnerExtended(MySINnerFile);
            }
            if (MySINnerFile.SiNnerMetaData == null)
            {
                MySINnerFile.SiNnerMetaData = new SINnerMetaData
                {
                    Id = Guid.NewGuid(),
                    Tags = new List<Tag>(),
                    Visibility = new SINnerVisibility
                    {
                        IsGroupVisible = true,
                        IsPublic = true
                    }
                };
            }

            if (!string.IsNullOrEmpty(Settings.Default.SINnerVisibility))
                MySINnerFile.SiNnerMetaData.Visibility =
                    JsonConvert.DeserializeObject<SINnerVisibility>(Settings.Default.SINnerVisibility);

            if(MySINnerFile.SiNnerMetaData.Visibility?.Id == null)
                if (MySINnerFile.SiNnerMetaData.Visibility != null)
                    MySINnerFile.SiNnerMetaData.Visibility.Id = Guid.NewGuid();

            MySINnerFile.LastChange = MyCharacter.FileLastWriteTime;

            if(MySINnerIds.TryGetValue(MyCharacter.FileName, out var singuid))
                MySINnerFile.Id = singuid;
            else
            {
                MySINnerFile.Id = Guid.NewGuid();
                MySINnerIds.Add(MyCharacter.FileName, MySINnerFile.Id.Value);
                MySINnerIds = MySINnerIds; //Save it!
            }
        }

        public CharacterExtended(Character character, string fileElement = null, SINner mySINnerLoading = null, CharacterCache myCharacterCache = null) : this(character, fileElement, myCharacterCache)
        {
            if (mySINnerLoading != null)
            {
                var backup = MySINnerFile;
                MySINnerFile = mySINnerLoading;
                if (MySINnerFile?.SiNnerMetaData != null)
                {
                    if (MySINnerFile.SiNnerMetaData.Id == null || MySINnerFile.SiNnerMetaData.Id == Guid.Empty)
                        MySINnerFile.SiNnerMetaData.Id = backup.SiNnerMetaData.Id;

                    if (MySINnerFile.SiNnerMetaData.Tags?.Count < backup?.SiNnerMetaData?.Tags?.Count)
                        MySINnerFile.SiNnerMetaData.Tags = new List<Tag>(backup.SiNnerMetaData.Tags);
                }
            }
        }

        public CharacterCache  MyCharacterCache { get; }

        public Character MyCharacter { get; }

        private SINner _MySINnerFile;
        // ReSharper disable once InconsistentNaming
        public SINner MySINnerFile
        {
            get => _MySINnerFile;
            set
            {
                _MySINnerFile = value;
                if (value != null)
                    _MySINnerFile.DownloadedFromSINnersTime = DateTime.Now.ToUniversalTime();
            }
        }

        public string ZipFilePath { get; set; }


        // ReSharper disable once InconsistentNaming
        private static Dictionary<string, Guid> _SINnerIds;

        // ReSharper disable once InconsistentNaming
        public static Dictionary<string, Guid> MySINnerIds
        {
            get
            {
                if (_SINnerIds != null)
                    return _SINnerIds;
                string save = Settings.Default.SINnerIds;
                _SINnerIds = !string.IsNullOrEmpty(save)
                    ? JsonConvert.DeserializeObject<Dictionary<string, Guid>>(save)
                    : new Dictionary<string, Guid>();
                return _SINnerIds;
            }
            set
            {
                _SINnerIds = value;
                if (_SINnerIds == null)
                    return;
                string save = JsonConvert.SerializeObject(_SINnerIds);
                Settings.Default.SINnerIds = save;
                Settings.Default.Save();
            }
        }

        internal IList<Tag> PopulateTags()
        {
            var tag = new Tag
            {
                MyRuntimeObject = MyCharacter,
                SiNnerId = MySINnerFile.Id,
                TagName = "Reflection",
                Tags = new List<Tag>(TagExtractor.ExtractTagsFromAttributes(MyCharacter))
            };
            foreach (var f in MySINnerFile.SiNnerMetaData.Tags.Where(x => x?.TagName == "Reflection").ToList())
            {
                MySINnerFile.SiNnerMetaData.Tags.Remove(f);
            }
            MySINnerFile.SiNnerMetaData.Tags.Add(tag);
            foreach(var childtag in MySINnerFile.SiNnerMetaData.Tags.Where(a => a != null).ToList())
            {
                childtag.SetSinnerIdRecursive(MySINnerFile.Id);
            }
            return MySINnerFile.SiNnerMetaData.Tags.ToList();
        }

        public async Task<bool> Upload(ucSINnerShare.MyUserState myState = null, CustomActivity parentActivity = null)
        {
            using (var op_uploadChummer = Timekeeper.StartSyncron(
                "Uploading Chummer", parentActivity,
                CustomActivity.OperationType.DependencyOperation, MyCharacter.FileName))
            {
                try
                {
                    using (new CursorWait(PluginHandler.MainForm, true))
                    {
                        HttpOperationResponse<ResultSinnerGetSINById> found = null;
                        try
                        {
                            using (_ = Timekeeper.StartSyncron(
                                "Checking if already online Chummer", op_uploadChummer,
                                CustomActivity.OperationType.DependencyOperation, MyCharacter?.FileName))
                            {
                                if (myState != null)
                                {
//1 Step
                                    myState.CurrentProgress += myState.ProgressSteps;
                                    myState.StatusText = "Checking online version of file...";
                                    myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                                }

                                if (MySINnerFile.DownloadedFromSINnersTime > MyCharacter.FileLastWriteTime)
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
                                found = await client.GetSINByIdWithHttpMessagesAsync(MySINnerFile.Id.GetValueOrDefault()).ConfigureAwait(true);
                                await Utils.HandleError(found, found.Body).ConfigureAwait(true);
                            }

                            if (myState != null)
//2 Step
                                myState.CurrentProgress += myState.ProgressSteps;
                            using (_ = Timekeeper.StartSyncron(
                                "Setting Visibility for Chummer", op_uploadChummer,
                                CustomActivity.OperationType.DependencyOperation, MyCharacter?.FileName))
                            {
                                if (found.Response.StatusCode == HttpStatusCode.OK)
                                {
                                    if (found.Body.MySINner.LastChange >= MyCharacter.FileLastWriteTime)
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

                                    if (MySINnerFile.SiNnerMetaData.Visibility.UserRights.Count == 0)
                                    {
                                        MySINnerFile.SiNnerMetaData.Visibility.UserRights =
                                            found.Body.MySINner.SiNnerMetaData.Visibility.UserRights;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            found?.Dispose();
                        }

                        using (_ = Timekeeper.StartSyncron(
                            "Populating Reflection Tags", op_uploadChummer,
                            CustomActivity.OperationType.DependencyOperation, MyCharacter?.FileName))
                        {
                            MySINnerFile.SiNnerMetaData.Tags = PopulateTags();
                        }

                        using (_ = Timekeeper.StartSyncron(
                            "Preparing Model", op_uploadChummer,
                            CustomActivity.OperationType.DependencyOperation, MyCharacter?.FileName))
                        {
                            await PrepareModel().ConfigureAwait(true);
                        }

                        if (myState != null)
                        {
//3 Step
                            myState.CurrentProgress += myState.ProgressSteps;
                            myState.StatusText = "Chummerfile prepared for uploading...";
                            myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                        }

                        HttpOperationResponse<ResultSinnerPostSIN> res = null;
                        try
                        {
                            using (_ = Timekeeper.StartSyncron(
                                "Posting SINner", op_uploadChummer,
                                CustomActivity.OperationType.DependencyOperation, MyCharacter?.FileName))
                            {
                                res = await Utils.PostSINnerAsync(this).ConfigureAwait(true);
                                await Utils.HandleError(res, res.Body).ConfigureAwait(true);
                            }

                            if (myState != null)
                            {
//4 Step
                                myState.CurrentProgress += myState.ProgressSteps;
                                myState.StatusText = "Chummer Metadata stored in DB...";
                                myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                            }

                            if (res.Response.IsSuccessStatusCode)
                            {
                                using (_ = Timekeeper.StartSyncron(
                                    "Uploading File", op_uploadChummer,
                                    CustomActivity.OperationType.DependencyOperation, MyCharacter?.FileName))
                                {
                                    using (var uploadres = await Utils.UploadChummerFileAsync(this).ConfigureAwait(true))
                                    {
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
                                    }

                                    return false;
                                }
                            }
                        }
                        finally
                        {
                            res.Dispose();
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
                catch (Exception e)
                {
                    op_uploadChummer?.tc?.TrackException(e);
                    await Utils.HandleError(e).ConfigureAwait(true);
                    throw;
                }
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
                foreach (var branch in tags.Where(t => t != null && t.MyParentTag == null))
                {
                    var child = new TreeNode
                    {
                        Text = branch.TagName,
                        Tag = branch.Id,
                    };
                    if (!string.IsNullOrEmpty(branch.TagValue))
                        child.Text += ": " + branch.TagValue;
                    if (!string.IsNullOrEmpty(branch.TagComment))
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
                    if (filtertags != null && (filtertags.All(x => x.TagName != tag.TagName) && tag.Tags.Count <= 0))
                        continue;
                    var child = new TreeNode
                    {
                        Text = string.Empty,
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
            string zipPath = Path.Combine(Settings.Default.TempDownloadPath, "SINner", MySINnerFile.Id + ".chum5z");
            if (PluginHandler.MySINnerLoading != null)
            {
                if (MySINnerIds.ContainsKey(MyCharacter.FileName))
                    MySINnerIds.Remove(MyCharacter.FileName);
            }
            object sinidob = null;
            if (MyCharacterCache?.MyPluginDataDic?.TryGetValue("SINnerId", out sinidob) == true)
            {
                MySINnerFile.Id = (Guid)sinidob;
            }
            else if (MySINnerIds.TryGetValue(MyCharacter.FileName, out var singuid))
                MySINnerFile.Id = singuid;
            else
            {
                if (PluginHandler.MySINnerLoading?.Id != null)
                {
                    MySINnerFile = PluginHandler.MySINnerLoading;
                    MySINnerIds.Add(MyCharacter.FileName, PluginHandler.MySINnerLoading.Id.Value);
                    if (File.Exists(zipPath))
                        return zipPath;
                }
                else
                {
                    if (string.IsNullOrEmpty(MySINnerFile.Alias))
                    {
                        MySINnerFile.Alias = MyCharacter.Alias;
                        if (string.IsNullOrEmpty(MySINnerFile.Alias))
                            MySINnerFile.Alias = MyCharacter.Name;
                    }
                    var client = StaticUtils.GetClient();
                    HttpOperationResponse<ResultSinnerGetOwnedSINByAlias> res = null;
                    try
                    {
                        try
                        {
                            res = await client.SinnerGetOwnedSINByAliasWithHttpMessagesAsync(MySINnerFile.Alias).ConfigureAwait(true);
                        }
                        catch (SerializationException e)
                        {
                            e.Data.Add("Alias", MySINnerFile.Alias);
                            e.Data.Add("MySINnerFile.Id", MySINnerFile.Id);
                            e.Data.Add("User", Settings.Default.UserEmail);
                            e.Data.Add("ResponseContent", e.Content);
                            throw;
                        }
                        catch (Exception e)
                        {
                            e.Data.Add("Alias", MySINnerFile.Alias);
                            e.Data.Add("MySINnerFile.Id", MySINnerFile.Id);
                            e.Data.Add("User", Settings.Default.UserEmail);
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
                            var result = PluginHandler.MainForm.ShowMessageBox(message, "SIN already found online", MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Asterisk);
                            if (result == DialogResult.Cancel)
                                throw new ArgumentException("User aborted perparation for upload!");
                            if (result == DialogResult.No)
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
                    finally
                    {
                        res?.Dispose();
                    }
                }

                if (MySINnerIds.ContainsKey(MyCharacter.FileName))
                    MySINnerIds.Remove(MyCharacter.FileName);
                if (MySINnerFile.Id != null)
                {
                    MySINnerIds.Add(MyCharacter.FileName, MySINnerFile.Id.Value);
                    MySINnerIds = MySINnerIds; //Save it!
                }
            }

            if (MySINnerFile.Id != null)
            {
                zipPath = Path.Combine(Path.GetTempPath(), "SINner", MySINnerFile.Id.Value + ".chum5z");
                MySINnerFile.UploadDateTime = DateTime.Now;


                MySINnerFile.Alias = MyCharacter.CharacterName;
                if (MySINnerFile.SiNnerMetaData.Visibility?.UserRights == null)
                {
                    MySINnerFile.SiNnerMetaData.Visibility =
                        new SINnerVisibility
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

                foreach (var visnow in ucSINnersOptions.SINnerVisibility.UserRights)
                {
                    if (MySINnerFile.SiNnerMetaData.Visibility.UserRights.All(a => a.EMail?.Equals(visnow.EMail, StringComparison.OrdinalIgnoreCase) != true))
                    {
                        MySINnerFile.SiNnerMetaData.Visibility.UserRights.Add(visnow);
                    }
                }

                foreach (var ur in MySINnerFile.SiNnerMetaData.Visibility.UserRights)
                {
                    if (ucSINnersOptions.SINnerVisibility.UserRights.Any(a => a.Id == ur.Id))
                    {
                        ur.Id = Guid.NewGuid();
                    }
                }

                var tempDir = Path.Combine(Path.GetTempPath(), "SINner", MySINnerFile.Id.Value.ToString());
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);
                foreach (var file in Directory.GetFiles(tempDir))
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.LastWriteTimeUtc < MyCharacter.FileLastWriteTime)
                        File.Delete(file);
                }

                var summary = new CharacterCache(MyCharacter.FileName);
                if (string.IsNullOrEmpty(summary.FileName))
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

                MyCharacter.Save(tempfile, false, false);
                MySINnerFile.LastChange = MyCharacter.FileLastWriteTime;
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
            }

            return zipPath;
        }

        private static void CreateDirectoryRecursively(string path)
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
                using (new CursorWait(null, true))
                {
                    if (myState != null)
                    {
                        myState.StatusText = "Preparing file to upload...";
                        myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                    }
                    PopulateTags();
                    await PrepareModel().ConfigureAwait(true);
                    if (myState != null)
                    {
                        myState.CurrentProgress += myState.ProgressSteps;
                        myState.StatusText = "Uploading Metadata...";
                        myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                    }

                    HttpOperationResponse<ResultSinnerPostSIN> response = await Utils.PostSINner(this).ConfigureAwait(true);
                    if (response?.Body?.CallSuccess == true)
                    {
                        try
                        {
                            try
                            {
                                MySINnerFile.Id = response.Body.MySINners.First().Id;
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex);
                                throw;
                            }
                            Log.Debug("Character " + MyCharacter.Alias + " posted with ID " + MySINnerFile.Id);
                            if (myState != null)
                            {
                                myState.CurrentProgress += myState.ProgressSteps;
                                myState.StatusText = "Uploading Filedata...";
                                myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                            }

                            var uploadresult = Utils.UploadChummer(this);

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

                            string msg = "Character " + MyCharacter.Alias + " uploaded with Id " +
                                         MySINnerFile.Id;
                            Log.Trace(msg);
                            if (myState != null)
                            {
                                myState.CurrentProgress += 3 * myState.ProgressSteps;
                                myState.StatusText = msg;
                                myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
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
                            myState.StatusText = "Could not upload Metadata: " + response?.Body?.ErrorText;
                            myState.StatusText += Environment.NewLine + response?.Body?.MyException;
                            myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                        }
                    }

                }
            }
            catch(Exception e)
            {
                Log.Error(e);
                Program.MainForm.ShowMessageBox(e.ToString());
            }
            return true;
        }

        public void Dispose()
        {
            // There is a check within Character::Dispose() that makes sure it doesn't actually get disposed
            // if the character is open in any forms or is linked to a character that is open.
            // So this will only dispose characters who are only temporarily opened purely to interact with this class
            MyCharacter?.Dispose();
        }
    }
}
