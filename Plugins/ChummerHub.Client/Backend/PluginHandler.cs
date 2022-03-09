using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChummerHub.Client.Sinners;
using ChummerHub.Client.Backend;
using ChummerHub.Client.Properties;
using ChummerHub.Client.UI;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Rest;
using Newtonsoft.Json;
using NLog;
using Resources = Chummer.Properties.Resources;

namespace Chummer.Plugins
{
    [Export(typeof(IPlugin))]
    //[ExportMetadata("Name", "SINners")]
    //[ExportMetadata("frmCareer", "true")]
    public class PluginHandler : IPlugin
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        public static UploadClient MyUploadClient { get; private set; }
        public static IPlugin MyPluginHandlerInstance { get; private set; }
        public static ChummerMainForm MainForm { get; private set; }

        [ImportingConstructor]
        public PluginHandler()
        {
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }
            Trace.TraceInformation("Plugin ChummerHub.Client importing (Constructor).");
            MyUploadClient = new UploadClient();
            if (Properties.Settings.Default.UploadClientId == Guid.Empty)
            {
                Properties.Settings.Default.UploadClientId = Guid.NewGuid();
                Properties.Settings.Default.Save();
            }
            MyUploadClient.Id = Properties.Settings.Default.UploadClientId;
            MyPluginHandlerInstance = this;
        }

        public override string ToString()
        {
            return "SINners";
        }

        public bool SetCharacterRosterNode(TreeNode objNode)
        {
            if (objNode?.Tag == null)
                return false;
            if (objNode.ContextMenuStrip == null)
            {
                string strTag = objNode.Tag?.ToString();
                objNode.ContextMenuStrip = MainForm.CharacterRoster.CreateContextMenuStrip(strTag.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase)
                                                                                           && MainForm.OpenCharacterForms.Any(x => x.CharacterObject?.FileName == strTag));
            }

            ContextMenuStrip cmsRoster = new ContextMenuStrip();
            DpiFriendlyToolStripMenuItem tsShowMySINners = new DpiFriendlyToolStripMenuItem
            {
                Name = "tsShowMySINners",
                Tag = "Menu_ShowMySINners",
                Text = "Show all my SINners",
                Size = new Size(177, 22),
                Image = Resources.group,
                ImageDpi192 = Resources.group1,
            };
            tsShowMySINners.Click += ShowMySINnersOnClick;
            tsShowMySINners.UpdateLightDarkMode();
            tsShowMySINners.TranslateToolStripItemsRecursively();
            cmsRoster.Items.Add(tsShowMySINners);
            
            DpiFriendlyToolStripMenuItem tsSINnersCreateGroup = new DpiFriendlyToolStripMenuItem
            {
                Name = "tsSINnersCreateGroup",
                Tag = "Menu_SINnersCreateGroup",
                Text = "Create Group",
                Size = new Size(177, 22),
                Image = Resources.group,
                ImageDpi192 = Resources.group1,
            };
            tsSINnersCreateGroup.Click += SINnersCreateGroupOnClick;
            tsSINnersCreateGroup.UpdateLightDarkMode();
            tsSINnersCreateGroup.TranslateToolStripItemsRecursively();
            cmsRoster.Items.Add(tsSINnersCreateGroup);
            cmsRoster.UpdateLightDarkMode();
            cmsRoster.TranslateWinForm();

            objNode.ContextMenuStrip = cmsRoster;
            if (objNode.Tag is CharacterCache member)
            {
                ToolStripMenuItem newShare = new ToolStripMenuItem("Share")
                {
                    Name = "tsShareChummer",
                    Tag = "Menu_ShareChummer",
                    Text = "Share chummer",
                    Size = new Size(177, 22),
                    Image = Resources.link_add
                };
                newShare.Click += NewShareOnClick;
                newShare.UpdateLightDarkMode();
                newShare.TranslateToolStripItemsRecursively();
                objNode.ContextMenuStrip.Items.Add(newShare);

                //is it a favorite sinner?
                if (member.MyPluginDataDic.TryGetValue("IsSINnerFavorite", out object objFavorite))
                {
                    ToolStripMenuItem newFavorite;
                    if (objFavorite is bool isFavorite && isFavorite)
                    {
                        newFavorite = new ToolStripMenuItem("RemovePinned")
                        {
                            Name = "tsRemovePinnedChummer",
                            Tag = "Menu_RemovePinnedChummer",
                            Text = "remove from pinned Chummers",
                            Size = new Size(177, 22),
                            Image = Resources.user_delete
                        };
                        newFavorite.Click += RemovePinnedOnClick;
                    }
                    else
                    {
                        newFavorite = new ToolStripMenuItem("AddPinned")
                        {
                            Name = "tsAddPinnedChummer",
                            Tag = "Menu_AddPinnedChummer",
                            Text = "add to pinned Chummers",
                            Size = new Size(177, 22),
                            Image = Resources.user_add
                        };
                        newFavorite.Click += AddPinnedOnClick;
                    }
                    newFavorite.UpdateLightDarkMode();
                    newFavorite.TranslateToolStripItemsRecursively();
                    objNode.ContextMenuStrip.Items.Add(newFavorite);
                }
                ToolStripMenuItem newDelete = new ToolStripMenuItem("DeleteFromSINners")
                {
                    Name = "tsDeleteFromSINners",
                    Tag = "Menu_DeleteFromSINners",
                    Text = "delete chummer from SINners registry",
                    Size = new Size(177, 22),
                    Image = Resources.delete
                };
                newDelete.Click += MainForm.CharacterRoster.tsDelete_Click;
                newDelete.UpdateLightDarkMode();
                newDelete.TranslateToolStripItemsRecursively();
                objNode.ContextMenuStrip.Items.Add(newDelete);
            }


            bool isPluginNode = false;
            TreeNode checkNode = objNode;
            while (!isPluginNode && checkNode != null)
            {
                if (checkNode.Tag is PluginHandler)
                    isPluginNode = true;
                checkNode = checkNode.Parent;
            }
            if (!isPluginNode)
                return true;

            if (objNode.Tag is SINnerSearchGroup)
            {
                MainForm.DoThreadSafe(() =>
                {
                    ToolStripMenuItem newShare = new ToolStripMenuItem("Share")
                    {
                        Name = "tsShareChummerGroup",
                        Tag = "Menu_ShareChummerGroup",
                        Text = "Share chummer group",
                        Size = new Size(177, 22),
                        Image = Resources.link_add
                    };
                    newShare.Click += NewShareOnClick;
                    newShare.UpdateLightDarkMode();
                    newShare.TranslateToolStripItemsRecursively();
                    objNode.ContextMenuStrip.Items.Add(newShare);

                    //is it a favorite sinner?
                    ToolStripMenuItem newFavorite;
                    //if (group.IsFavorite == true)
                    //{
                    //    newFavorite = new ToolStripMenuItem("RemovePinned")
                    //    {
                    //        Name = "tsRemovePinnedGroup",
                    //        Tag = "Menu_RemovePinnedGroup",
                    //        Text = "remove from pinned",
                    //        Size = new Size(177, 22),
                    //        Image = Resources.user_delete
                    //    };
                    //    newFavorite.Click += RemovePinnedOnClick;
                    //}
                    //else
                    {
                        newFavorite = new ToolStripMenuItem("AddPinned")
                        {
                            Name = "tsAddPinnedGroup",
                            Tag = "Menu_AddPinnedGroup",
                            Text = "Pin Chummer",
                            Size = new Size(177, 22),
                            Image = Resources.user_add
                        };
                        newFavorite.Click += AddPinnedOnClick;
                    }
                    newFavorite.UpdateLightDarkMode();
                    newFavorite.TranslateToolStripItemsRecursively();
                    objNode.ContextMenuStrip.Items.Add(newFavorite);
                });
            }

            foreach (ToolStripItem item in objNode.ContextMenuStrip.Items.Cast<ToolStripItem>())
            {
                switch (item.Name)
                {
                    case "tsToggleFav":
                    case "tsCloseOpenCharacter":
                    case "tsSort":
                        objNode.ContextMenuStrip.Items.Remove(item);
                        break;
                    case "tsDelete":
                        objNode.ContextMenuStrip.Items.Remove(item);
                        ToolStripMenuItem newDelete = new ToolStripMenuItem(item.Text, item.Image);
                        newDelete.Click += MainForm.CharacterRoster.tsDelete_Click;
                        newDelete.UpdateLightDarkMode();
                        newDelete.TranslateToolStripItemsRecursively();
                        objNode.ContextMenuStrip.Items.Add(newDelete);
                        break;
                }
            }

            return true;
        }

        public ITelemetry SetTelemetryInitialize(ITelemetry telemetry)
        {
            if (!String.IsNullOrEmpty(ChummerHub.Client.Properties.Settings.Default.UserEmail))
            {
                if (telemetry?.Context?.User != null)
                {
                    telemetry.Context.User.AccountId = ChummerHub.Client.Properties.Settings.Default.UserEmail;
                }
            }
            return telemetry;
        }

        bool IPlugin.ProcessCommandLine(string parameter)
        {
            Log.Debug("ChummerHub.Client.PluginHandler ProcessCommandLine: " + parameter);
            string argument = string.Empty;
            string onlyparameter = parameter;
            if (parameter.Contains(':'))
            {
                argument = parameter.Substring(parameter.IndexOf(':'));
                argument = argument.TrimStart(':');
                onlyparameter = parameter.Substring(0, parameter.IndexOf(':'));
            }
            switch (onlyparameter)
            {
                case "Load":
                    return HandleLoadCommand(argument).GetAwaiter().GetResult();
            }
            Log.Warn("Unknown command line parameter: " + parameter);
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || PipeManager == null)
                return;
            //only stop the server if this is the last instance!
            if (!BlnHasDuplicate)
                PipeManager.StopServer();
            PipeManager.Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private async Task<bool> HandleLoadCommand(string argument)
        {
            //check global mutex
            bool blnHasDuplicate;
            try
            {
                blnHasDuplicate = !Program.GlobalChummerMutex.WaitOne(0, false);
            }
            catch (AbandonedMutexException ex)
            {
                Log.Error(ex);
                Utils.BreakIfDebug();
                blnHasDuplicate = true;
            }
            await Task.Run(async () =>
            {
                if (!blnHasDuplicate)
                {
                    TimeSpan uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
                    if (uptime < TimeSpan.FromSeconds(2))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                    }
                }
                if (PipeManager != null)
                {
                    try
                    {
                        string SINnerIdvalue = argument.Substring(5).Trim('/');
                        int transactionInt = SINnerIdvalue.IndexOf(':');
                        if (transactionInt != -1)
                        {
                            string transaction = SINnerIdvalue.Substring(transactionInt).TrimStart(':');
                            SINnerIdvalue = SINnerIdvalue.Substring(0, transactionInt).TrimEnd(':');
                            string callback = string.Empty;
                            int callbackInt = transaction.IndexOf(':');
                            if (callbackInt != -1)
                            {
                                callback = transaction.Substring(callbackInt).TrimStart(':');
                                transaction = transaction.Substring(0, callbackInt).TrimEnd(':');
                                callback = WebUtility.UrlDecode(callback);
                            }
                            await StaticUtils.WebCall(callback, 10, "Sending Open Character Request");
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        Program.ShowMessageBox("Error loading SINner: " + e.Message);
                    }
                    string msg = "Load:" + argument;
                    Log.Trace("Sending argument to Pipeserver: " + msg);
                    PipeManager.Write(msg);
                }
            });
            if (blnHasDuplicate)
            {
                Environment.ExitCode = -1;
                return false;
            }

            return true;
        }

        public IEnumerable<TabPage> GetTabPages(CharacterCareer input)
        {
            if (!Settings.Default.UserModeRegistered)
                yield break;
            TabPage objReturn = Utils.RunWithoutThreadLock(() => GetTabPagesCommon(input));
            if (objReturn == null)
                yield break;
            yield return objReturn;
        }

        public IEnumerable<TabPage> GetTabPages(CharacterCreate input)
        {
            if (!Settings.Default.UserModeRegistered)
                yield break;
            TabPage objReturn = Utils.RunWithoutThreadLock(() => GetTabPagesCommon(input));
            if (objReturn == null)
                yield break;
            yield return objReturn;
        }

        private static async Task<TabPage> GetTabPagesCommon(CharacterShared input)
        {
            ucSINnersUserControl uc = new ucSINnersUserControl();
            try
            {
                await uc.SetCharacterFrom(input);
            }
            catch (Exception e)
            {
                ChummerHub.Client.Backend.Utils.HandleError(e);
                return null;
            }
            TabPage page = new TabPage("SINners")
            {
                Name = "SINners"
            };
            page.Controls.Add(uc);
            return page;
        }

        public static SINner MySINnerLoading { get; internal set; }
        public NamedPipeManager PipeManager { get; private set; }

        public string GetSaveToFileElement(Character input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            string returnme = string.Empty;
            using (CharacterExtended ce = GetMyCe(input))
            {
                PropertyRenameAndIgnoreSerializerContractResolver jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    ContractResolver = jsonResolver
                };
                //remove the reflection tag - no need to save it
                Tag refTag = ce?.MySINnerFile?.SiNnerMetaData?.Tags?.FirstOrDefault(x => x?.TagName == "Reflection");
                if (refTag != null)
                {
                    ce.MySINnerFile.SiNnerMetaData.Tags.Remove(refTag);
                    returnme = JsonConvert.SerializeObject(ce.MySINnerFile, Formatting.Indented, settings);
                    ce.MySINnerFile.SiNnerMetaData.Tags.Add(refTag);
                }
                else if (ce != null)
                    returnme = JsonConvert.SerializeObject(ce.MySINnerFile, Formatting.Indented, settings);
            }

            return returnme;
        }

        public static async Task<bool> MyOnSaveUpload(Character input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (!Settings.Default.UserModeRegistered)
            {
                string msg = "Public Mode currently does not save to the SINners Plugin by default, even if \"onlinemode\" is enabled!" + Environment.NewLine;
                msg += "If you want to use SINners as online store, please register!";
                Log.Warn(msg);
            }
            else if (input.DoOnSaveCompletedAsync.Remove(MyOnSaveUpload)) // Makes we only run this if we haven't already triggered the callback
            {
                try
                {
                    using (CursorWait.New(MainForm, true))
                    {
                        using (CharacterExtended ce = await GetMyCeAsync(input))
                        {
                            //ce = new CharacterExtended(input, null);
                            if (ce.MySINnerFile.SiNnerMetaData.Tags.All(a => a?.TagName != "Reflection"))
                            {
                                ce.MySINnerFile.SiNnerMetaData.Tags = ce.PopulateTags();
                            }

                            await ce.Upload();
                        }

                        TabPage tabPage = null;
                        CharacterShared found = MainForm.OpenCharacterForms.FirstOrDefault(x => x.CharacterObject == input);
                        switch (found)
                        {
                            case CharacterCreate frm when frm.TabCharacterTabs.TabPages.ContainsKey("SINners"):
                                {
                                    int index = frm.TabCharacterTabs.TabPages.IndexOfKey("SINners");
                                    tabPage = frm.TabCharacterTabs.TabPages[index];
                                    break;
                                }
                            case CharacterCareer frm2 when frm2.TabCharacterTabs.TabPages.ContainsKey("SINners"):
                                {
                                    int index = frm2.TabCharacterTabs.TabPages.IndexOfKey("SINners");
                                    tabPage = frm2.TabCharacterTabs.TabPages[index];
                                    break;
                                }
                        }

                        if (tabPage == null)
                            return true;
                        Control[] ucseq = tabPage.Controls.Find("SINnersBasic", true);
                        foreach (Control uc in ucseq)
                        {
                            if (uc is ucSINnersBasic sb)
                                await sb.CheckSINnerStatus();
                        }

                        Control[] ucseq2 = tabPage.Controls.Find("SINnersAdvanced", true);
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                }
                finally
                {
                    input.DoOnSaveCompletedAsync.Add(MyOnSaveUpload);
                }
            }
            return true;
        }

        private static CharacterExtended GetMyCe(Character input)
        {
            return GetMyCeCoreAsync(true, input).GetAwaiter().GetResult();
        }

        private static Task<CharacterExtended> GetMyCeAsync(Character input)
        {
            return GetMyCeCoreAsync(false, input);
        }

        private static async Task<CharacterExtended> GetMyCeCoreAsync(bool blnSync, Character input)
        {
            CharacterShared found = null;
            if (MainForm?.OpenCharacterForms != null)
                foreach (CharacterShared a in (MainForm?.OpenCharacterForms))
                {
                    if (a?.CharacterObject != input)
                        continue;
                    found = a;
                    break;
                }
            TabPage sinnertab = null;
            if (found != null)
            {
                TabControl.TabPageCollection myCollection = null;
                switch (found)
                {
                    case CharacterCreate foundcreate:
                        myCollection = foundcreate.TabCharacterTabs.TabPages;
                        break;
                    case CharacterCareer foundcareer:
                        myCollection = foundcareer.TabCharacterTabs.TabPages;
                        break;
                }

                if (myCollection == null)
                    return null;

                sinnertab = myCollection.OfType<TabPage>().FirstOrDefault(x => x.Name == "SINners");
            }
            
            CharacterCache myCharacterCache;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                myCharacterCache = new CharacterCache(input?.FileName);
            else
                myCharacterCache = await CharacterCache.CreateFromFileAsync(input?.FileName);
            if (sinnertab == null)
                return new CharacterExtended(input, null, myCharacterCache);
            ucSINnersUserControl myUcSIN = sinnertab.Controls.OfType<ucSINnersUserControl>().FirstOrDefault();
            return myUcSIN == null ? new CharacterExtended(input, null, myCharacterCache) : myUcSIN.MyCE;
        }

        public void LoadFileElement(Character input, string fileElement)
        {
            try
            {
                CharacterExtended.SaveFromPluginFile(fileElement, input, MySINnerLoading);
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                throw;
#endif
            }
        }

        public IEnumerable<ToolStripMenuItem> GetMenuItems(ToolStripMenuItem menu)
        {
#if DEBUG
            if (Settings.Default.UserModeRegistered)
            {
                DpiFriendlyToolStripMenuItem mnuSINnerSearchs = new DpiFriendlyToolStripMenuItem
                {
                    Name = "mnuSINSearch",
                    Text = "&SINner Search",
                    Image = Resources.group,
                    ImageTransparentColor = Color.Black,
                    Size = new Size(148, 22),
                    Tag = "Menu_Tools_SINnerSearch",
                    ImageDpi192 = Resources.group1,
                };
                mnuSINnerSearchs.Click += mnuSINnerSearchs_Click;
                mnuSINnerSearchs.UpdateLightDarkMode();
                mnuSINnerSearchs.TranslateToolStripItemsRecursively();
                yield return mnuSINnerSearchs;
            }

            DpiFriendlyToolStripMenuItem mnuSINnersArchetypes = new DpiFriendlyToolStripMenuItem
            {
                Name = "mnuSINnersArchetypes",
                Text = "&Archetypes",
                Image = Resources.group,
                ImageTransparentColor = Color.Black,
                Size = new Size(148, 22),
                Tag = "Menu_Tools_SINnersArchetypes",
                ImageDpi192 = Resources.group1,
            };
            mnuSINnersArchetypes.Click += mnuSINnersArchetypes_Click;
            mnuSINnersArchetypes.UpdateLightDarkMode();
            mnuSINnersArchetypes.TranslateToolStripItemsRecursively();
            yield return mnuSINnersArchetypes;
#endif
            if (Settings.Default.UserModeRegistered)
            {
                DpiFriendlyToolStripMenuItem mnuSINners = new DpiFriendlyToolStripMenuItem
                {
                    Name = "mnuSINners",
                    Text = "&SINners",
                    Image = Resources.group,
                    ImageTransparentColor = Color.Black,
                    Size = new Size(148, 22),
                    Tag = "Menu_Tools_SINners",
                    ImageDpi192 = Resources.group1,
                };
                mnuSINners.Click += mnuSINners_Click;
                mnuSINners.UpdateLightDarkMode();
                mnuSINners.TranslateToolStripItemsRecursively();
                yield return mnuSINners;
            }
        }

        private void mnuSINnerSearchs_Click(object sender, EventArgs e)
        {
            frmSINnerSearch search = new frmSINnerSearch();
            search.Show();
        }

        private async void mnuSINnersArchetypes_Click(object sender, EventArgs e)
        {
            ResultGroupGetSearchGroups res = null;
            try
            {
                using (CursorWait.New(MainForm, true))
                {
                    SinnersClient client = StaticUtils.GetClient();
                    res = await client.GetPublicGroupAsync("Archetypes", string.Empty);
                    if (!(await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res) is ResultGroupGetSearchGroups result))
                        return;
                    if (!result.CallSuccess)
                        return;
                    SINSearchGroupResult ssgr = result.MySearchGroupResult;
                    if (ssgr != null && ssgr.SinGroups?.Count > 0)
                    {
                        await MainForm.CharacterRoster.DoThreadSafeAsync(async () =>
                        {
                            List<TreeNode> nodelist = ChummerHub.Client.Backend.Utils
                                                                .CharacterRosterTreeNodifyGroupList(
                                                                    ssgr.SinGroups.Where(a => a.Groupname == "Archetypes")).ToList();
                            foreach (TreeNode node in nodelist)
                            {
                                MyTreeNodes2Add.AddOrUpdate(node.Name, node,
                                    (key, oldValue) => node);
                            }

                            await MainForm.CharacterRoster.RefreshPluginNodes(this);
                            MainForm.CharacterRoster.treCharacterList.SelectedNode =
                                nodelist.Find(a => a.Name == "Archetypes");
                            MainForm.BringToFront();
                        });
                    }
                    else
                    {
                        Program.ShowMessageBox("No archetypes found!");
                    }
                }
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (!(await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res, ex) is ResultGroupGetSearchGroups))
                    return;
            }
            finally
            {
                //res?.Dispose();
            }
        }

        public static readonly LockingDictionary<string, TreeNode> MyTreeNodes2Add = new LockingDictionary<string, TreeNode>();

        private void mnuSINners_Click(object sender, EventArgs ea)
        {
            try
            {
                using (CursorWait.New(MainForm, true))
                {
                    using (frmSINnerGroupSearch frmSearch = new frmSINnerGroupSearch(null, null)
                    {
                        TopMost = true
                    })
                    {
                        frmSearch.ShowDialog();
                    }
                }

            }
            catch (SerializationException e)
            {
                if (e.Content.Contains("Log in - ChummerHub"))
                {
                    TreeNode node = new TreeNode("Online, but not logged in!")
                    {
                        ToolTipText = "Please log in (Options -> Plugins -> Sinners (Cloud) -> Login",
                        Tag = e
                    };
                    Log.Warn("Online, but not logged in!");
                }
                else
                {
                    Log.Warn(e);
                    TreeNode node = new TreeNode("Error: " + e.Message)
                    {
                        ToolTipText = e.ToString(),
                        Tag = e
                    };
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
                TreeNode node = new TreeNode("SINners Error: please log in")
                {
                    ToolTipText = e.ToString(),
                    Tag = e
                };
            }
        }


        public System.Reflection.Assembly GetPluginAssembly()
        {
            return typeof(ucSINnersUserControl).Assembly;
        }

        public void SetIsUnitTest(bool isUnitTest)
        {
            StaticUtils.MyUtils.IsUnitTest = isUnitTest;
            MyUploadClient.ChummerVersion = !StaticUtils.MyUtils.IsUnitTest
                ? System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version.ToString() ?? string.Empty
                : System.Reflection.Assembly.GetCallingAssembly().GetName().Version.ToString();
        }

        public UserControl GetOptionsControl()
        {
            return new ucSINnersOptions();
        }

        public async Task<ICollection<TreeNode>> GetCharacterRosterTreeNode(CharacterRoster frmCharRoster, bool forceUpdate)
        {
            try
            {
                using (CursorWait.New(frmCharRoster, true))
                {
                    IEnumerable<TreeNode> res = null;
                    if (Settings.Default.UserModeRegistered)
                    {
                        Log.Info("Loading CharacterRoster from SINners...");

                        async Task<ResultAccountGetSinnersByAuthorization> getSINnersFunction()
                        {
                            SinnersClient client = null;
                            ResultAccountGetSinnersByAuthorization ret = null;
                            try
                            {
                                client = StaticUtils.GetClient();
                                ret = await client.GetSINnersByAuthorizationAsync();
                                return ret;
                            }
                            catch (Exception)
                            {
                                if (client != null)
                                    client.ReadResponseAsString = !client.ReadResponseAsString;
                                try
                                {
                                    ret = client != null ? await client.GetSINnersByAuthorizationAsync() : null;
                                }
                                catch(ApiException e1)
                                {
                                    if (e1.Response?.Contains("<li><a href=\"/Identity/Account/Login\">Login</a></li>") == true)
                                    {
                                        Log.Info(e1, "User is not logged in.");
                                        throw new ArgumentException("User not logged in.");
                                    }
                                    else {
                                        Log.Error(e1);
                                        throw;
                                    }
                                }
                                finally
                                {
                                    if (client != null)
                                        client.ReadResponseAsString = !client.ReadResponseAsString;
                                }
                                if (ret == null)
                                    throw;
                            }
                            return ret;
                        }

                        res = await ChummerHub.Client.Backend.Utils.GetCharacterRosterTreeNode(forceUpdate, getSINnersFunction);
                        if (res == null)
                        {
                            throw new ArgumentException("Could not load owned SINners from WebService.");
                        }
                    }
                    //AddContextMenuStripRecursive(list, myContextMenuStrip);
                    return res?.Concat(MyTreeNodes2Add.Select(x => x.Value).OrderBy(x => x.Text)).ToList()
                           ?? MyTreeNodes2Add.Select(x => x.Value).OrderBy(x => x.Text).ToList();
                }
            }
            catch(SerializationException e)
            {
                if (e.Content.Contains("Log in - ChummerHub"))
                {
                    Log.Warn(e, "Online, but not logged in!");
                    return new List<TreeNode>
                    {
                        new TreeNode("Online, but not logged in!")
                        {
                            ToolTipText = "Please log in (Options -> Plugins -> Sinners (Cloud) -> Login",
                            Tag = e
                        }
                    };
                }

                Log.Error(e);
                return new List<TreeNode>
                {
                    new TreeNode("Error: " + e.Message)
                    {
                        ToolTipText = e.ToString(),
                        Tag = e
                    }
                };
            }
            catch(ApiException e)
            {
                TreeNode node = null;
                Log.Error(e);
                switch (e.StatusCode)
                {
                    case 500:
                        node = new TreeNode("SINers seems to be down (Error 500)")
                        {
                            ToolTipText = e.Message,
                            Tag = e

                        };
                        break;
                    case 200:
                        node = new TreeNode("SINersplugin encounterd an error: " + e.StatusCode)
                        {
                            ToolTipText = e.Message,
                            Tag = e
                        };
                        break;
                    default:
                        node = new TreeNode("SINers encounterd an error: " + e.StatusCode)
                        {
                            ToolTipText = e.Message,
                            Tag = e
                        };
                        break;
                }
                return new List<TreeNode>
                    {
                        node
                    };
            }
            catch(Exception e)
            {
                Log.Info(e);
                TreeNode node = new TreeNode("SINers: please log in")
                {
                    ToolTipText = e.Message,
                    Tag = e

                };
                
                return new List<TreeNode>
                {
                    node
                };
            }
        }


        

        private async void SINnersCreateGroupOnClick(object sender, EventArgs e)
        {
            try
            {
                SINnerGroup g = await ChummerHub.Client.Backend.Utils.CreateGroupOnClickAsync();
                await ShowMySINners();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Program.ShowMessageBox(ex.Message);
            }
        }

        private async void ShowMySINnersOnClick(object sender, EventArgs e)
        {
            await ShowMySINners();
        }

        private async ValueTask ShowMySINners()
        {
            try
            {
                using (CursorWait.New(MainForm.CharacterRoster, true))
                {
                    SINSearchGroupResult MySINSearchGroupResult = await ucSINnerGroupSearch.SearchForGroups(null);
                    SINnerSearchGroup item = MySINSearchGroupResult.SinGroups.FirstOrDefault(x => x.Groupname?.Contains("My Data") == true);
                    if (item != null)
                    {
                        List<SINnerSearchGroup> list = new List<SINnerSearchGroup> { item };
                        IEnumerable<TreeNode> nodelist = ChummerHub.Client.Backend.Utils.CharacterRosterTreeNodifyGroupList(list);
                        foreach (TreeNode node in nodelist)
                        {
                            MyTreeNodes2Add.AddOrUpdate(node.Name, node, (key, oldValue) => node);
                        }
                        await MainForm.CharacterRoster.RefreshPluginNodes(this);
                        MainForm.CharacterRoster.BringToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Program.ShowMessageBox(ex.Message);
            }
        }

        private async void AddPinnedOnClick(object sender, EventArgs e)
        {
            TreeNode t = MainForm.CharacterRoster.treCharacterList.SelectedNode;

            switch (t?.Tag)
            {
                case CharacterCache objCache:
                    try
                    {
                        string sinneridstring = null;
                        SinnersClient client = StaticUtils.GetClient();
                        if (objCache.MyPluginDataDic.TryGetValue("SINnerId", out object sinneridobj))
                        {
                            sinneridstring = sinneridobj?.ToString();
                        }

                        if (Guid.TryParse(sinneridstring, out Guid sinnerid))
                        {
                            try
                            {
                                SINner res = await client.PutSINerInGroupAsync(Guid.Empty, sinnerid, null);
                                object response = await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                                if (response != null)
                                    await MainForm.CharacterRoster.RefreshPluginNodes(this);
                            }
                            catch (Exception exception)
                            {
                                ChummerHub.Client.Backend.Utils.HandleError(exception);
                            }
                            finally
                            {
                                //res?.Dispose();
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Error(exception);
                        Program.ShowMessageBox("Error sharing SINner: " + exception.Message);
                    }

                    break;
                case SINnerSearchGroup ssg:
                    try
                    {
                        SinnersClient client = StaticUtils.GetClient();
                        ResultGroupPutGroupInGroup res = await client.PutGroupInGroupAsync(ssg.Id, null, Guid.Empty, null, null);
                        object response = await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                        if (response != null)
                        {
                            await MainForm.CharacterRoster.RefreshPluginNodes(this);
                        }
                    }
                    catch (Exception exception)
                    {
                        ChummerHub.Client.Backend.Utils.HandleError(exception);
                    }
                    finally
                    {
                        //res?.Dispose();
                    }

                    break;
            }
        }

        private async void RemovePinnedOnClick(object sender, EventArgs e)
        {
            TreeNode t = MainForm.CharacterRoster.treCharacterList.SelectedNode;

            switch (t?.Tag)
            {
                case CharacterCache objCache:
                    try
                    {
                        string sinneridstring = null;
                        SinnersClient client = StaticUtils.GetClient();
                        if (objCache.MyPluginDataDic.TryGetValue("SINnerId", out object sinneridobj))
                        {
                            sinneridstring = sinneridobj?.ToString();
                        }

                        if (Guid.TryParse(sinneridstring, out Guid sinnerid))
                        {
                            SINner res = null;
                            try
                            {
                                res = await client.PutSINerInGroupAsync(null, sinnerid, null);

                                object response = await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                                if (response != null)
                                {
                                    await MainForm.CharacterRoster.RefreshPluginNodes(this);
                                }
                            
                            }
                            catch(ApiException e1)
                            {
                                if (res != null)
                                {
                                    object response = await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                                    if (response != null)
                                    {
                                        await MainForm.CharacterRoster.RefreshPluginNodes(this);
                                    }
                                }
                                else
                                {
                                    ChummerHub.Client.Backend.Utils.HandleError(e1);
                                }

                            }
                            catch (Exception exception)
                            {
                                ChummerHub.Client.Backend.Utils.HandleError(exception);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Error(exception);
                        Program.ShowMessageBox("Error sharing SINner: " + exception.Message);
                    }

                    break;
                case SINnerSearchGroup ssg:
                    try
                    {
                        SinnersClient client = StaticUtils.GetClient();
                        ResultGroupPutGroupInGroup res = await client.PutGroupInGroupAsync(ssg.Id, null, null, null, null);
                        object response = await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                        if (response != null)
                        {
                            await MainForm.CharacterRoster.RefreshPluginNodes(this);
                        }
                    }
                    catch (Exception exception)
                    {
                        ChummerHub.Client.Backend.Utils.HandleError(exception);
                    }

                    break;
            }
        }


        private async void NewShareOnClick(object sender, EventArgs e)
        {
            TreeNode t = MainForm.CharacterRoster.treCharacterList.SelectedNode;

            switch (t?.Tag)
            {
                case CharacterCache objCache:
                {
                    using (frmSINnerShare share = new frmSINnerShare
                    {
                        TopMost = true
                    })
                    {
                        share.MyUcSINnerShare.MyCharacterCache = objCache;
                        share.Show();
                        try
                        {
                            await share.MyUcSINnerShare.DoWork();
                        }
                        catch (Exception exception)
                        {
                            Log.Error(exception);
                            Program.ShowMessageBox("Error sharing SINner: " + exception.Message);
                        }
                    }

                    break;
                }
                case SINnerSearchGroup ssg:
                {
                    using (frmSINnerShare share = new frmSINnerShare
                    {
                        TopMost = true
                    })
                    {
                        share.MyUcSINnerShare.MySINnerSearchGroup = ssg;
                        share.Show();
                        try
                        {
                            await share.MyUcSINnerShare.DoWork();
                        }
                        catch (Exception exception)
                        {
                            Log.Error(exception);
                            Program.ShowMessageBox("Error sharing Group: " + exception.Message);
                        }
                    }

                    break;
                }
            }
        }

        //private void AddContextMenuStripRecursive(List<TreeNode> list, ContextMenuStrip myCmsRoster)
        //{
        //    foreach (var node in list)
        //    {
        //        if (node.Parent != null)
        //        {
        //            if (node.Tag is SINnerSearchGroup group)
        //            {

        //            }
        //            else if (node.Tag is frmCharacterRoster.CharacterCache member)
        //            {
        //                PluginHandler.MainForm.DoThreadSafe(() => { node.ContextMenuStrip = myCmsRoster; });
        //            }
        //        }

        //        if (node.Nodes.Count > 0)
        //        {
        //            var myList = node.Nodes.Cast<TreeNode>().ToList();
        //            AddContextMenuStripRecursive(myList, myCmsRoster);
        //        }
        //    }
        //}

        public bool BlnHasDuplicate { get; set; }

        public void CustomInitialize(ChummerMainForm mainControl)
        {
            Log.Info("CustomInitialize for Plugin ChummerHub.Client entered.");
            MainForm = mainControl;
            if (string.IsNullOrEmpty(Settings.Default.TempDownloadPath))
            {
                Settings.Default.TempDownloadPath = Path.GetTempPath();
            }

            //check global mutex
            BlnHasDuplicate = false;
            try
            {
                BlnHasDuplicate = !Program.GlobalChummerMutex.WaitOne(0, false);
            }
            catch (AbandonedMutexException ex)
            {
                Log.Error(ex);
                Utils.BreakIfDebug();
                BlnHasDuplicate = true;
            }
            if (PipeManager == null)
            {
                PipeManager = new NamedPipeManager();
                Log.Info("blnHasDuplicate = " + BlnHasDuplicate.ToString(CultureInfo.InvariantCulture));
                // If there is more than 1 instance running, do not let the application start a receiving server.
                if (BlnHasDuplicate)
                {
                    Log.Info("More than one instance, not starting NamedPipe-Server...");
                    throw new InvalidOperationException("More than one instance is running.");
                }

                Log.Info("Only one instance, starting NamedPipe-Server...");
                Task.Run(() => PipeManager.StartServer().ContinueWith(x => PipeManager.ReceiveString += y => HandleNamedPipe_OpenRequest(y).GetAwaiter().GetResult()));
            }
        }

        private static string fileNameToLoad = string.Empty;

        public static async Task HandleNamedPipe_OpenRequest(string argument)
        {
            Log.Trace("Pipeserver receiced a request: " + argument);
            if (!string.IsNullOrEmpty(argument))
            {
                //make sure the mainform is visible ...
                TimeSpan uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
                if (uptime < TimeSpan.FromSeconds(5))
                    await Task.Delay(TimeSpan.FromSeconds(4)).ConfigureAwait(false);
                if (!MainForm.Visible)
                {
                    await MainForm.DoThreadSafeAsync(x =>
                    {
                        ChummerMainForm objMainForm = (ChummerMainForm) x;
                        if (objMainForm.WindowState == FormWindowState.Minimized)
                            objMainForm.WindowState = FormWindowState.Normal;
                    });
                }

                await MainForm.DoThreadSafeAsync(x =>
                {
                    ChummerMainForm objMainForm = (ChummerMainForm)x;
                    objMainForm.Activate();
                    objMainForm.BringToFront();
                });
                SinnersClient client = StaticUtils.GetClient();
                while (!MainForm.Visible)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                }
                if (argument.StartsWith("Load:", StringComparison.Ordinal))
                {
                    try
                    {
                        string SINnerIdvalue = argument.Substring(5);
                        SINnerIdvalue = SINnerIdvalue.Trim('/');
                        int transactionInt = SINnerIdvalue.IndexOf(':');
                        string callback = null;
                        if (transactionInt != -1)
                        {
                            string transaction = SINnerIdvalue.Substring(transactionInt);
                            SINnerIdvalue = SINnerIdvalue.Substring(0, transactionInt);
                            SINnerIdvalue = SINnerIdvalue.TrimEnd(':');
                            transaction = transaction.TrimStart(':');
                            int callbackInt = transaction.IndexOf(':');
                            if (callbackInt != -1)
                            {
                                callback = transaction.Substring(callbackInt);
                                transaction = transaction.Substring(0, callbackInt);
                                transaction = transaction.TrimEnd(':');
                                callback = callback.TrimStart(':');
                                callback = WebUtility.UrlDecode(callback);
                            }

                            await StaticUtils.WebCall(callback, 30,
                                "Open Character Request received!");
                        }

                        if (Guid.TryParse(SINnerIdvalue, out Guid SINnerId))
                        {

                            ResultSinnerGetSINById found = await client.GetSINByIdAsync(SINnerId);
                            await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(found);
                            if (found?.CallSuccess == true)
                            {
                                await StaticUtils.WebCall(callback, 40,
                                    "Character found online");
                                fileNameToLoad =
                                    await ChummerHub.Client.Backend.Utils.DownloadFileTask(found.MySINner, null);
                                await StaticUtils.WebCall(callback, 70,
                                    "Character downloaded");
                                await MainFormLoadChar(fileNameToLoad);
                                await StaticUtils.WebCall(callback, 100,
                                    "Character opened");
                            }
                            else if (found == null || !found.CallSuccess)
                            {
                                await StaticUtils.WebCall(callback, 0,
                                    "Character not found");
                                Program.ShowMessageBox("Could not find a SINner with Id " + SINnerId + " online!");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        Program.ShowMessageBox("Error loading SINner: " + e.Message);
                    }

                }
                else
                {
                    throw new ArgumentException("Unkown command received: " + argument, nameof(argument));
                }
            }
        }

        private static async ValueTask MainFormLoadChar(string fileToLoad)
        {
            //already open
            Character objCharacter = Program.OpenCharacters.FirstOrDefault(a => a.FileName == fileToLoad);
            if (objCharacter == null)
            {
                objCharacter = new Character
                {
                    FileName = fileToLoad
                };
                using (LoadingBar frmLoadingForm = await Program.CreateAndShowProgressBarAsync(Path.GetFileName(fileToLoad), Character.NumLoadingSections))
                {
                    if (await objCharacter.LoadAsync(frmLoadingForm, Settings.Default.IgnoreWarningsOnOpening))
                        Program.OpenCharacters.Add(objCharacter);
                    else
                        return;
                }
            }
            using (CursorWait.New(MainForm))
            {
                await MainForm.DoThreadSafeFunc(async x =>
                {
                    ChummerMainForm frmMain = (ChummerMainForm) x;
                    if (!frmMain.SwitchToOpenCharacter(objCharacter))
                        await frmMain.OpenCharacter(objCharacter, false);
                    frmMain.BringToFront();
                });
            }
        }

        public async Task<bool> DoCharacterList_DragDrop(object sender, DragEventArgs dragEventArgs, TreeView treCharacterList)
        {
            if (dragEventArgs == null)
                throw new ArgumentNullException(nameof(dragEventArgs));
            if (treCharacterList == null)
                throw new ArgumentNullException(nameof(treCharacterList));
            try
            {
                // Do not allow the root element to be moved.
                if (treCharacterList.SelectedNode == null || treCharacterList.SelectedNode.Level == 0 ||
                    treCharacterList.SelectedNode.Parent?.Tag?.ToString() == "Watch")
                    return false;

                if (dragEventArgs.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
                {
                    if (!(sender is TreeView treSenderView))
                        return false;
                    Point pt = treSenderView.PointToClient(new Point(dragEventArgs.X, dragEventArgs.Y));
                    TreeNode nodDestinationNode = treSenderView.GetNodeAt(pt);
                    if (nodDestinationNode.Level > 0)
                        nodDestinationNode = nodDestinationNode.Parent;
                    string strDestinationNode = nodDestinationNode.Tag?.ToString();
                    if (strDestinationNode != "Watch")
                    {
                        if (!(dragEventArgs.Data.GetData("System.Windows.Forms.TreeNode") is TreeNode nodNewNode))
                            return false;
                        SinnersClient client = StaticUtils.GetClient();
                        Guid? mySiNnerId = null;
                        switch (nodNewNode.Tag)
                        {
                            case SINnerSearchGroup sinGroup when nodDestinationNode.Tag == MyPluginHandlerInstance:
                            {
                                ResultGroupPutGroupInGroup res = await client.PutGroupInGroupAsync(sinGroup.Id, sinGroup.Groupname, null, null, null);
                                await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                                return true;
                            }
                            case SINnerSearchGroup sinGroup when nodDestinationNode.Tag is SINnerSearchGroup destGroup:
                            {
                                ResultGroupPutGroupInGroup res = await client.PutGroupInGroupAsync(sinGroup.Id, sinGroup.Groupname, destGroup.Id, sinGroup.MyAdminIdentityRole, sinGroup.IsPublic);
                                await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                                return true;
                            }
                            case CharacterCache objCache:
                            {
                                if (objCache.MyPluginDataDic.TryGetValue("SINnerId", out object sinidob))
                                {
                                    mySiNnerId = (Guid?) sinidob;
                                }
                                else
                                {
                                    using (CharacterExtended ce = await ChummerHub.Client.Backend.Utils.UploadCharacterFromFile(objCache.FilePath))
                                        mySiNnerId = ce?.MySINnerFile?.Id;
                                }

                                break;
                            }
                            case SINner sinner:
                                mySiNnerId = sinner.Id;
                                break;
                        }

                        if (mySiNnerId != null)
                        {
                            if (nodDestinationNode.Tag == MyPluginHandlerInstance)
                            {
                                SINner res = await client.PutSINerInGroupAsync(null, mySiNnerId, null);
                                await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                                return true;
                            }
                            else if (nodDestinationNode.Tag is SINnerSearchGroup destGroup)
                            {
                                string passwd = null;
                                if (destGroup.HasPassword)
                                {
                                    using (frmSINnerPassword getPWD = new frmSINnerPassword())
                                    {
                                        string pwdquestion = await LanguageManager.GetStringAsync("String_SINners_EnterGroupPassword", true);
                                        string pwdcaption = await LanguageManager.GetStringAsync("String_SINners_EnterGroupPasswordTitle", true);
                                        passwd = getPWD.ShowDialog(Program.MainForm, pwdquestion, pwdcaption);
                                    }
                                }
                                SINner res = await client.PutSINerInGroupAsync(destGroup.Id, mySiNnerId, passwd);
                                await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception);
            }
            finally
            {
                await MainForm.CharacterRoster.RefreshPluginNodes(this);
            }
            return true;
        }
    }
}
