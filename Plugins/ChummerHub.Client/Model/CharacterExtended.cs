using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
using Utils = ChummerHub.Client.Backend.Utils;

namespace ChummerHub.Client.Sinners
{
    public sealed class CharacterExtended : IDisposable
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        public CharacterExtended(Character character, CharacterCache myCharacterCache = null, bool blnDoSave = true)
        {
            MyCharacter = character ?? throw new ArgumentNullException(nameof(character));
            MyCharacterCache = myCharacterCache ?? new CharacterCache(MyCharacter.FileName);
            MySINnerFile = new SINner
            {
                Language = GlobalSettings.Language,
                SiNnerMetaData = new SINnerMetaData
                {
                    Id = Guid.NewGuid(),
                    Tags = new List<Tag>(),
                    Visibility = new SINnerVisibility
                    {
                        IsGroupVisible = true,
                        IsPublic = true
                    }
                }
            };
            
            MySINnerFile.SiNnerMetaData.Visibility = Utils.DefaultSINnerVisibility;
            if (MySINnerFile.SiNnerMetaData.Visibility != null && MySINnerFile.SiNnerMetaData.Visibility.Id == null)
                MySINnerFile.SiNnerMetaData.Visibility.Id = Guid.NewGuid();
            MySINnerFile.LastChange = MyCharacter.FileLastWriteTime;
            MySINnerFile.Id = MySINnerIds.TryGetValue(MyCharacter.FileName, out Guid singuid) ? singuid : Guid.NewGuid();
            if (!blnDoSave)
                return;
            MySINnerIds.AddOrUpdate(MyCharacter.FileName, MySINnerFile.Id.Value, (x, y) => MySINnerFile.Id.Value);
            SaveSINnerIds(); //Save it!
        }

        public CharacterExtended(Character character, SINner mySINnerLoading = null, CharacterCache myCharacterCache = null, bool blnDoSave = true) : this(character, myCharacterCache, false)
        {
            if (mySINnerLoading == null)
                return;
            SINner backup = MySINnerFile;
            MySINnerFile = mySINnerLoading;
            if (MySINnerFile?.SiNnerMetaData == null)
                return;
            if (MySINnerFile.SiNnerMetaData.Id == Guid.Empty)
                MySINnerFile.SiNnerMetaData.Id = backup.SiNnerMetaData.Id;
            if (MySINnerFile.SiNnerMetaData.Tags?.Count < backup?.SiNnerMetaData?.Tags?.Count)
                MySINnerFile.SiNnerMetaData.Tags = new List<Tag>(backup.SiNnerMetaData.Tags);
            if (!blnDoSave || MySINnerFile.Id == null)
                return;
            if (MySINnerFile.Id == null)
                MySINnerFile.Id = Guid.NewGuid();
            MySINnerIds.AddOrUpdate(MyCharacter.FileName, MySINnerFile.Id.Value, (x, y) => MySINnerFile.Id.Value);
            SaveSINnerIds(); //Save it!
        }

        private static readonly LockingDictionary<string, SINner> s_dicCachedPluginFileSINners =
            new LockingDictionary<string, SINner>();

        public static void SaveFromPluginFile(string strPluginFileElement, Character character, SINner mySINnerLoading = null)
        {
            if (character == null)
                throw new ArgumentNullException(nameof(character));
            if (string.IsNullOrEmpty(strPluginFileElement))
                return;
            CharacterExtended objReturn = new CharacterExtended(character, mySINnerLoading, new CharacterCache(), false);
            objReturn.MyCharacterCache.LoadFromFile(character.FileName);
            try
            {
                if (!s_dicCachedPluginFileSINners.TryGetValue(strPluginFileElement, out SINner objSiNner))
                {
                    objSiNner = JsonConvert.DeserializeObject<SINner>(strPluginFileElement);
                    s_dicCachedPluginFileSINners.AddOrUpdate(strPluginFileElement, objSiNner, (x, y) => objSiNner);
                }

                objReturn.MySINnerFile = objSiNner;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            SaveSINnerIds();
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
        private static LockingDictionary<string, Guid> _SINnerIds;

        // ReSharper disable once InconsistentNaming
        public static LockingDictionary<string, Guid> MySINnerIds
        {
            get
            {
                if (_SINnerIds != null)
                    return _SINnerIds;
                string save = Settings.Default.SINnerIds;
                _SINnerIds = !string.IsNullOrEmpty(save)
                    ? JsonConvert.DeserializeObject<LockingDictionary<string, Guid>>(save)
                    : new LockingDictionary<string, Guid>();
                return _SINnerIds;
            }
            set
            {
                _SINnerIds = value;
                if (_SINnerIds == null)
                    return;
                SaveSINnerIds();
            }
        }

        public static void SaveSINnerIds()
        {
            if (_SINnerIds == null)
                return;
            string strSave = JsonConvert.SerializeObject(_SINnerIds);
            Settings.Default.SINnerIds = strSave;
            Settings.Default.Save();
        }

        internal IList<Tag> PopulateTags()
        {
            Tag tag = new Tag ()
            {
                MyRuntimeObject = MyCharacter,
                SiNnerId = MySINnerFile.Id,
                TagName = "Reflection",
                Tags = new List<Tag>(TagExtractor.ExtractTagsFromAttributes(MyCharacter))
            };
            foreach (Tag f in MySINnerFile.SiNnerMetaData.Tags.Where(x => x?.TagName == "Reflection").ToList())
            {
                MySINnerFile.SiNnerMetaData.Tags.Remove(f);
            }
            MySINnerFile.SiNnerMetaData.Tags.Add(tag);
            foreach(Tag childtag in MySINnerFile.SiNnerMetaData.Tags.Where(a => a != null).ToList())
            {
                childtag.SetSinnerIdRecursive(MySINnerFile.Id);
            }
            return MySINnerFile.SiNnerMetaData.Tags.ToList();
        }

        public async Task<bool> Upload(ucSINnerShare.MyUserState myState = null, CustomActivity parentActivity = null)
        {
            using (CustomActivity op_uploadChummer = Timekeeper.StartSyncron(
                "Uploading Chummer", parentActivity,
                CustomActivity.OperationType.DependencyOperation, MyCharacter.FileName))
            {
                try
                {
                    using (new CursorWait(PluginHandler.MainForm, true))
                    {
                        try
                        {
                            ResultSinnerGetSINById found = null;
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

                                SinnersClient client = StaticUtils.GetClient();
                                try
                                {


                                    found = await client.GetSINByIdAsync(MySINnerFile.Id.GetValueOrDefault());
                                    await Utils.ShowErrorResponseFormAsync(found);
                                }
                                catch(ApiException ae)
                                {
                                    if (ae.StatusCode == 404)
                                        found = new ResultSinnerGetSINById()
                                        {
                                            CallSuccess = false,
                                            MyException = ae,
                                            MySINner = null,
                                            ErrorText = "SINner not found online"
                                        };
                                }
                            }

                            if (myState != null)
//2 Step
                                myState.CurrentProgress += myState.ProgressSteps;
                            using (_ = Timekeeper.StartSyncron(
                                "Setting Visibility for Chummer", op_uploadChummer,
                                CustomActivity.OperationType.DependencyOperation, MyCharacter?.FileName))
                            {
                                if (found?.CallSuccess == true)
                                {
                                    if (found.MySINner != null && found.MySINner.LastChange >= MyCharacter.FileLastWriteTime)
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
                                            found.MySINner?.SiNnerMetaData.Visibility.UserRights;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            //found?.Dispose();
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
                            await PrepareModelAsync();
                        }

                        if (myState != null)
                        {
//3 Step
                            myState.CurrentProgress += myState.ProgressSteps;
                            myState.StatusText = "Chummerfile prepared for uploading...";
                            myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                        }

                        ResultSinnerPostSIN res;
                        try
                        {
                            using (_ = Timekeeper.StartSyncron(
                                "Posting SINner", op_uploadChummer,
                                CustomActivity.OperationType.DependencyOperation, MyCharacter?.FileName))
                            {
                                res = null;
                                try
                                {
                                    res = await Utils.PostSINnerAsync(this);
                                    await Utils.ShowErrorResponseFormAsync(res);
                                }
                                catch(ApiException ae)
                                {
                                    //202 is kind of a special case (and probably wrongly handled in swagger.cs)
                                    if (ae.StatusCode == 202)
                                    {
                                        Log.Trace("SINner posted, next step: uploading the file!");
                                        res = new ResultSinnerPostSIN() { CallSuccess = true };
                                    }
                                    else
                                    {
                                        throw;    
                                    }
                                    
                                }

                            }

                            if (myState != null)
                            {
//4 Step
                                myState.CurrentProgress += myState.ProgressSteps;
                                myState.StatusText = "Chummer Metadata stored in DB...";
                                myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                            }

                            if (res?.CallSuccess == true)
                            {
                                using (_ = Timekeeper.StartSyncron(
                                    "Uploading File", op_uploadChummer,
                                    CustomActivity.OperationType.DependencyOperation, MyCharacter?.FileName))
                                {
                                    ResultSINnerPut uploadres = await Utils.UploadChummerFileAsync(this);
                                    if (uploadres.CallSuccess)
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
                                        myState.StatusText = "Chummer upload failed: " + uploadres.ErrorText;
                                        myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                                    }

                                    return false;
                                }
                            }
                        }
                        finally
                        {
                            //res.Dispose();
                        }

                        if (myState != null)
                        {
//5 Step
                            myState.CurrentProgress += myState.ProgressSteps;
                            myState.StatusText = "Chummer upload of Metadata failed: " + res?.ErrorText;
                            myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                        }

                        return false;
                    }
                }
                catch (Exception e)
                {
                    op_uploadChummer?.MyTelemetryClient?.TrackException(e);
                    Utils.HandleError(e);
                    throw;
                }
            }
        }

        public void PopulateTree(ref TreeNode root, IList<Tag> tags, IList<SearchTag> filtertags)
        {
            if (tags == null)
                tags = MySINnerFile.SiNnerMetaData.Tags.ToArray();
            if (root == null)
            {
                root = new TreeNode
                {
                    Text = "Tags",
                    Tag = null
                };
                // get all tags in the list with parent is null
                foreach (Tag branch in tags.Where(t => t != null && t.MyParentTag == null))
                {
                    TreeNode child = new TreeNode
                    {
                        Text = branch.TagName,
                        Tag = branch.Id,
                    };
                    if (!string.IsNullOrEmpty(branch.TagValue))
                        child.Text += ": " + branch.TagValue;
                    if (!string.IsNullOrEmpty(branch.TagComment))
                        child.ToolTipText = branch.TagComment;
                    PopulateTree(ref child, branch.Tags.ToArray(), filtertags);
                    root.Nodes.Add(child);
                }

                root.ExpandAll();
            }
            else
            {
                foreach (Tag tag in tags)
                {
                    if (filtertags != null && (filtertags.All(x => x.TagName != tag.TagName) && tag.Tags.Count <= 0))
                        continue;
                    TreeNode child = new TreeNode
                    {
                        Text = string.Empty,
                        Tag = tag.Id
                    };
                    if(!string.IsNullOrEmpty(tag.TagComment))
                        child.Text = "(" + tag.TagComment + ") ";
                    child.Text += tag.TagName;
                    if (!string.IsNullOrEmpty(tag.TagValue))
                        child.Text += ": " + tag.TagValue;
                    PopulateTree(ref child, tag.Tags?.ToArray(), filtertags);
                    root.Nodes.Add(child);
                }
            }
        }

        public string PrepareModel()
        {
            return PrepareModelCoreAsync(true).GetAwaiter().GetResult();
        }

        public Task<string> PrepareModelAsync()
        {
            return PrepareModelCoreAsync(false);
        }

        private async Task<string> PrepareModelCoreAsync(bool blnSync)
        {
            string zipPath = Path.Combine(Settings.Default.TempDownloadPath, "SINner", MySINnerFile.Id + ".chum5z");
            if (PluginHandler.MySINnerLoading != null && MySINnerIds.ContainsKey(MyCharacter.FileName))
            {
                MySINnerIds.TryRemove(MyCharacter.FileName, out Guid _);
            }
            object sinidob = null;
            if (MyCharacterCache?.MyPluginDataDic.TryGetValue("SINnerId", out sinidob) == true)
            {
                MySINnerFile.Id = (Guid)sinidob;
            }
            else if (MySINnerIds.TryGetValue(MyCharacter.FileName, out Guid singuid))
                MySINnerFile.Id = singuid;
            else
            {
                if (PluginHandler.MySINnerLoading?.Id != null)
                {
                    MySINnerFile = PluginHandler.MySINnerLoading;
                    MySINnerIds.TryAdd(MyCharacter.FileName, PluginHandler.MySINnerLoading.Id.Value);
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
                    SinnersClient client = StaticUtils.GetClient();
                    try
                    {
                        ResultSinnerGetOwnedSINByAlias res;
                        try
                        {
                            if (blnSync)
                            {
                                Task<ResultSinnerGetOwnedSINByAlias> objSearchTask = client.SinnerGetOwnedSINByAliasAsync(MySINnerFile.Alias);
                                if (objSearchTask.Status == TaskStatus.Created)
                                    objSearchTask.RunSynchronously();
                                res = objSearchTask.Result;
                            }
                            else
                                res =  await client.SinnerGetOwnedSINByAliasAsync(MySINnerFile.Alias);
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

                        if (res == null)
                        {
                            MySINnerFile.Id = Guid.NewGuid();
                        }
                        else if (res.CallSuccess)
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
                            DialogResult result = PluginHandler.MainForm.ShowMessageBox(message, "SIN already found online", MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Asterisk);
                            switch (result)
                            {
                                case DialogResult.Cancel:
                                    throw new ArgumentException("User aborted perparation for upload!");
                                case DialogResult.No:
                                    MySINnerFile.Id = Guid.NewGuid();
                                    break;
                                default:
                                {
                                    ICollection<SINner> list = res.MySINners;
                                    foreach (SINner sin in list)
                                    {
                                        if (sin.Id != null)
                                        {
                                            MySINnerFile.Id = sin.Id;
                                            break;
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    finally
                    {
                        //res?.Dispose();
                    }
                }

                while (MySINnerIds.ContainsKey(MyCharacter.FileName))
                    MySINnerIds.TryRemove(MyCharacter.FileName, out Guid _);
                if (MySINnerFile.Id != null)
                    MySINnerIds.TryAdd(MyCharacter.FileName, MySINnerFile.Id.Value);
                SaveSINnerIds(); //Save it!
            }

            if (MySINnerFile.Id == null)
                return zipPath;
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
            if (!string.IsNullOrEmpty(Settings.Default.UserEmail) && MySINnerFile.SiNnerMetaData.Visibility.UserRights.Any(a =>
                    string.Equals(a.EMail, "delete.this.and.add@your.mail", StringComparison.OrdinalIgnoreCase)))
            {
                List<SINnerUserRight> found = MySINnerFile.SiNnerMetaData.Visibility.UserRights.Where(a =>
                                                              string.Equals(
                                                                  a.EMail, "delete.this.and.add@your.mail",
                                                                  StringComparison.OrdinalIgnoreCase))
                                                          .ToList();
                foreach (SINnerUserRight one in found)
                    MySINnerFile.SiNnerMetaData.Visibility.UserRights.Remove(one);
            }
            if (MySINnerFile.SiNnerMetaData.Visibility.UserRights.Count == 0)
            {
                MySINnerFile.SiNnerMetaData.Visibility.UserRights.Add(new SINnerUserRight
                {
                    Id = Guid.NewGuid(),
                    CanEdit = true,
                    EMail = Settings.Default.UserEmail
                });
            }


            if (MySINnerFile.SiNnerMetaData.Visibility.Id == ucSINnersOptions.SINnerVisibility.Id)
            {
                //make the visibility your own and dont reuse the id from the general options!
                MySINnerFile.SiNnerMetaData.Visibility.Id = Guid.NewGuid();
            }

            foreach (SINnerUserRight visnow in ucSINnersOptions.SINnerVisibility.UserRights)
            {
                if (MySINnerFile.SiNnerMetaData.Visibility.UserRights.All(a =>
                    a.EMail?.Equals(visnow.EMail, StringComparison.OrdinalIgnoreCase) != true))
                {
                    MySINnerFile.SiNnerMetaData.Visibility.UserRights.Add(visnow);
                }
            }

            foreach (SINnerUserRight ur in MySINnerFile.SiNnerMetaData.Visibility.UserRights)
            {
                if (ucSINnersOptions.SINnerVisibility.UserRights.Any(a => a.Id == ur.Id))
                {
                    ur.Id = Guid.NewGuid();
                }
            }

            if (MySINnerFile.MyGroup == null)
                MySINnerFile.MyGroup = new SINnerGroup(null)
                {
                    Id = Guid.Empty
                };

            string tempDir = Path.Combine(Path.GetTempPath(), "SINner", MySINnerFile.Id.Value.ToString());
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);
            foreach (string file in Directory.GetFiles(tempDir))
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastWriteTimeUtc < MyCharacter.FileLastWriteTime)
                    File.Delete(file);
            }
            
            if (string.IsNullOrEmpty(MyCharacter.FileName))
                return null;
            string tempfile = Path.Combine(tempDir, MyCharacter.FileName);
            if (File.Exists(tempfile))
                File.Delete(tempfile);

            bool readCallback = MyCharacter.DoOnSaveCompleted.Remove(PluginHandler.MyOnSaveUpload);

            if (!File.Exists(MyCharacter.FileName))
            {
                string path2 = MyCharacter.FileName.Substring(0, MyCharacter.FileName.LastIndexOf('\\'));
                CreateDirectoryRecursively(path2);
                MyCharacter.Save(MyCharacter.FileName, false, false);
            }

            MyCharacter.Save(tempfile, false, false);
            MySINnerFile.LastChange = MyCharacter.FileLastWriteTime;
            if (readCallback)
                MyCharacter.DoOnSaveCompleted.Add(PluginHandler.MyOnSaveUpload);

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

        private static void CreateDirectoryRecursively(string path)
        {
            string[] pathParts = path.Split('\\');

            string strPrevious = string.Empty;
            for (int i = 0; i < pathParts.Length; i++)
            {
                string strCurrent = pathParts[i];
                if (i > 0)
                    strCurrent = Path.Combine(strPrevious, strCurrent);

                if (!Directory.Exists(strCurrent))
                {
                    try
                    {
                        Directory.CreateDirectory(strCurrent);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        throw new UnauthorizedAccessException("Path: " + strCurrent, e);
                    }
                }

                strPrevious = strCurrent;
            }
        }


        public async Task<bool> UploadInBackground(ucSINnerShare.MyUserState myState)
        {
            try
            {
                using (new CursorWait(PluginHandler.MainForm, true))
                {
                    if (myState != null)
                    {
                        myState.StatusText = "Preparing file to upload...";
                        myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                    }
                    PopulateTags();
                    await PrepareModelAsync();
                    if (myState != null)
                    {
                        myState.CurrentProgress += myState.ProgressSteps;
                        myState.StatusText = "Uploading Metadata...";
                        myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                    }

                    ResultSinnerPostSIN response = await Utils.PostSINnerAsync(this);
                    if (response?.CallSuccess == true)
                    {
                        try
                        {
                            try
                            {
                                MySINnerFile.Id = response.MySINners.First().Id;
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

                            ResultSINnerPut uploadresult = await Utils.UploadChummerFileAsync(this);

                            if (!uploadresult.CallSuccess)
                            {
                                if (myState != null)
                                {
                                    myState.CurrentProgress += 3 * myState.ProgressSteps;
                                    myState.StatusText = "Failed uploading Filedata: " + uploadresult.ErrorText;
                                    myState.StatusText += Environment.NewLine + uploadresult.MyException;
                                    myState.myWorker?.ReportProgress(myState.CurrentProgress, myState);
                                }
                                if (uploadresult.MyException != null)
                                    throw new ArgumentException(uploadresult.MyException.ToString());
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
                            myState.StatusText = "Could not upload Metadata: " + response?.ErrorText;
                            myState.StatusText += Environment.NewLine + response?.MyException;
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
            MyCharacterCache?.Dispose();
        }
    }
}
