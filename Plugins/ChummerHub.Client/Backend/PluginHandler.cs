using System;
using System.Collections.Concurrent;
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
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static UploadClient MyUploadClient;
        public static IPlugin MyPluginHandlerInstance;
        public static frmChummerMain MainForm;

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
            if (objNode == null || objNode.Tag == null)
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
            while (isPluginNode == false && checkNode != null)
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

            foreach (var item in objNode.ContextMenuStrip.Items.Cast<ToolStripItem>())
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
            //We should maybe add an option in the plugin-option dialog to give the user the opportunity to enable this again.
            //if (!String.IsNullOrEmpty(ChummerHub.Client.Properties.Settings.Default.UserEmail))
            //{
            //    if (telemetry?.Context?.User != null)
            //        telemetry.Context.User.AccountId = ChummerHub.Client.Properties.Settings.Default.UserEmail;
            //}
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

        void IPlugin.Dispose()
        {
            if (PipeManager != null)
            {
                //only stop the server if this is the last instance!
                if (!BlnHasDuplicate)
                    PipeManager.StopServer();
                PipeManager.Dispose();
            }
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
                    var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
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
                        MainForm.ShowMessageBox("Error loading SINner: " + e.Message);
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

        public IEnumerable<TabPage> GetTabPages(frmCareer input)
        {
            foreach (TabPage tabPage in GetTabPagesCommon(input))
                yield return tabPage;
        }

        public IEnumerable<TabPage> GetTabPages(frmCreate input)
        {
            foreach (TabPage tabPage in GetTabPagesCommon(input))
                yield return tabPage;
        }

        private IEnumerable<TabPage> GetTabPagesCommon(CharacterShared input)
        {
            if (Settings.Default.UserModeRegistered == false)
                yield break;
            ucSINnersUserControl uc = new ucSINnersUserControl();
            try
            {
                uc.SetCharacterFrom(input);
            }
            catch (Exception e)
            {
                ChummerHub.Client.Backend.Utils.HandleError(e);
                yield break;
            }
            TabPage page = new TabPage("SINners")
            {
                Name = "SINners"
            };
            page.Controls.Add(uc);
            yield return page;
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
                var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    ContractResolver = jsonResolver
                };
                //remove the reflection tag - no need to save it
                var refTag = ce?.MySINnerFile?.SiNnerMetaData?.Tags?.FirstOrDefault(x => x?.TagName == "Reflection");
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

        public static bool MyOnSaveUpload(Character input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (Settings.Default.UserModeRegistered == false)
            {
                string msg = "Public Mode currently does not save to the SINners Plugin by default, even if \"onlinemode\" is enabled!" + Environment.NewLine;
                msg += "If you want to use SINners as online store, please register!";
                Log.Warn(msg);
            }
            else
            {
                if (input.DoOnSaveCompleted.Remove(MyOnSaveUpload)) // Makes we only run this if we haven't already triggered the callback
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            using (new CursorWait(MainForm, true))
                            {
                                using (var ce = await GetMyCeAsync(input))
                                {
                                    //ce = new CharacterExtended(input, null);
                                    if (ce.MySINnerFile.SiNnerMetaData.Tags.Any(a =>
                                            a != null && a.TagName == "Reflection") ==
                                        false)
                                    {
                                        ce.MySINnerFile.SiNnerMetaData.Tags = ce.PopulateTags();
                                    }

                                    await ce.Upload();
                                }

                                TabPage tabPage = null;
                                var found = MainForm.OpenCharacterForms.FirstOrDefault(x => x.CharacterObject == input);
                                if (found is frmCreate frm && frm.TabCharacterTabs.TabPages.ContainsKey("SINners"))
                                {
                                    var index = frm.TabCharacterTabs.TabPages.IndexOfKey("SINners");
                                    tabPage = frm.TabCharacterTabs.TabPages[index];
                                }
                                else if (found is frmCareer frm2 &&
                                         frm2.TabCharacterTabs.TabPages.ContainsKey("SINners"))
                                {
                                    var index = frm2.TabCharacterTabs.TabPages.IndexOfKey("SINners");
                                    tabPage = frm2.TabCharacterTabs.TabPages[index];
                                }

                                if (tabPage == null)
                                    return;
                                var ucseq = tabPage.Controls.Find("SINnersBasic", true);
                                foreach (var uc in ucseq)
                                {
                                    if (uc is ucSINnersBasic sb)
                                        await sb.CheckSINnerStatus();
                                }

                                var ucseq2 = tabPage.Controls.Find("SINnersAdvanced", true);
                            }
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError(e.ToString());
                        }
                        finally
                        {
                            input.DoOnSaveCompleted.TryAdd(MyOnSaveUpload);
                        }
                    });
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
                    case frmCreate foundcreate:
                        myCollection = foundcreate.TabCharacterTabs.TabPages;
                        break;
                    case frmCareer foundcareer:
                        myCollection = foundcareer.TabCharacterTabs.TabPages;
                        break;
                }

                if (myCollection == null)
                    return null;

                sinnertab = myCollection.OfType<TabPage>().FirstOrDefault(x => x.Name == "SINners");
            }
            
            CharacterCache myCharacterCache = new CharacterCache();
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                myCharacterCache.LoadFromFile(input?.FileName);
            else
                await myCharacterCache.LoadFromFileAsync(input?.FileName);
            if (sinnertab == null)
                return new CharacterExtended(input, null, myCharacterCache);
            ucSINnersUserControl myUcSIN = sinnertab.Controls.OfType<ucSINnersUserControl>().FirstOrDefault();
            return myUcSIN == null ? new CharacterExtended(input, null, myCharacterCache) : myUcSIN.MyCE;
        }

        public void LoadFileElement(Character input, string strPluginFileElement)
        {
            try
            {
                CharacterExtended.SaveFromPluginFile(strPluginFileElement, input, MySINnerLoading);
            }
            catch (Exception e)
            {
                Log.Error(e);
#if DEBUG
                throw;
#endif
            }
        }

        public IEnumerable<ToolStripMenuItem> GetMenuItems(ToolStripMenuItem input)
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
                using (new CursorWait(MainForm, true))
                {
                    var client = StaticUtils.GetClient();
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
                            var nodelist = ChummerHub.Client.Backend.Utils
                                .CharacterRosterTreeNodifyGroupList(
                                    ssgr.SinGroups.Where(a => a.Groupname == "Archetypes")).ToList();
                            foreach (var node in nodelist)
                            {
                                MyTreeNodes2Add.AddOrUpdate(node.Name, node,
                                    (key, oldValue) => node);
                            }

                            await MainForm.CharacterRoster.LoadCharacters(false, false, false);
                            MainForm.CharacterRoster.treCharacterList.SelectedNode =
                                nodelist.FirstOrDefault(a => a.Name == "Archetypes");
                            MainForm.BringToFront();
                        });
                    }
                    else
                    {
                        MainForm.ShowMessageBox("No archetypes found!");
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

        public static readonly ConcurrentDictionary<string, TreeNode> MyTreeNodes2Add = new ConcurrentDictionary<string, TreeNode>();

        private void mnuSINners_Click(object sender, EventArgs ea)
        {
            try
            {
                using (new CursorWait(MainForm, true))
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

        public async Task<ICollection<TreeNode>> GetCharacterRosterTreeNode(frmCharacterRoster frmCharRoster, bool forceUpdate)
        {
            try
            {
                using (new CursorWait(frmCharRoster, true))
                {
                    IEnumerable<TreeNode> res = null;
                    if (Settings.Default.UserModeRegistered)
                    {
                        Log.Info("Loading CharacterRoster from SINners...");

                        async Task<ResultAccountGetSinnersByAuthorization> getSINnersFunction()
                        {
                            SinnersClient client = null;
                            try
                            {
                                client = StaticUtils.GetClient();
                                var ret = await client.GetSINnersByAuthorizationAsync();
                                return ret;
                            }
                            catch (Exception e)
                            {
                                if (client != null)
                                    client.ReadResponseAsString = !client.ReadResponseAsString;
                                try
                                {
                                    var ret = client != null ? await client.GetSINnersByAuthorizationAsync() : null;
                                    Log.Error(e);
                                }
                                catch(ApiException e1)
                                {
                                    if (e1.Response?.Contains("<li><a href=\"/Identity/Account/Login\">Login</a></li>") == true)
                                    {
                                        Log.Info(e1, "User is not logged in.");
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
                                throw;
                            }
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
                if (e.StatusCode == 500)
                {
                    node = new TreeNode("SINers seems to be down (Error 500)")
                    {
                        ToolTipText = e.Message,
                        Tag = e

                    };
                }
                else
                {
                    node = new TreeNode("SINers encounterd an error: " + e.StatusCode)
                    {
                        ToolTipText = e.Message,
                        Tag = e
                    };
                }
                return new List<TreeNode>
                    {
                        node
                    };
            }
            catch(Exception e)
            {
                Log.Info(e);
                var node = new TreeNode("SINers: please log in")
                {
                    ToolTipText = e.Message,
                    Tag = e

                };
                //if (node.ContextMenuStrip == null)
                //    node.ContextMenuStrip = new ContextMenuStrip();
                //ToolStripMenuItem login = new ToolStripMenuItem("Login")
                //{
                //    Name = "tsLogin",
                //    Tag = "Menu_LoginChummerHub",
                //    Text = "Log in/register",
                //    Size = new Size(177, 22),
                //    Image = Resources.page_refresh
                //};
                //login.Click += LoginOnClick;
                //login.UpdateLightDarkMode();
                //login.TranslateToolStripItemsRecursively();
                //node.ContextMenuStrip.Items.Add(login);
                return new List<TreeNode>
                {
                    node
                };
            }
        }

        //private void LoginOnClick(object sender, EventArgs e)
        //{
        //    frmOptions options = new frmOptions();
        //    options.ShowDialog();
        //}

        private async void ShowMySINnersOnClick(object sender, EventArgs e)
        {
            //TreeNode t = PluginHandler.MainForm.CharacterRoster.treCharacterList.SelectedNode;
            try
            {
                using (new CursorWait(MainForm.CharacterRoster, true))
                {
                    var MySINSearchGroupResult = await ucSINnerGroupSearch.SearchForGroups(null);
                    var item = MySINSearchGroupResult.SinGroups.FirstOrDefault(x => x.Groupname?.Contains("My Data") == true);
                    if (item != null)
                    {
                        var list = new List<SINnerSearchGroup> { item };
                        var nodelist = ChummerHub.Client.Backend.Utils.CharacterRosterTreeNodifyGroupList(list);
                        foreach (var node in nodelist)
                        {
                            MyTreeNodes2Add.AddOrUpdate(node.Name, node, (key, oldValue) => node);
                        }
                        await MainForm.CharacterRoster.LoadCharacters(false, false, false);
                        MainForm.CharacterRoster.BringToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Program.MainForm.ShowMessageBox(ex.Message);
            }
        }

        private async void AddPinnedOnClick(object sender, EventArgs e)
        {
            TreeNode t = MainForm.CharacterRoster.treCharacterList.SelectedNode;

            if (t?.Tag is CharacterCache objCache)
            {
                try
                {
                    string sinneridstring = null;
                    var client = StaticUtils.GetClient();
                    if (objCache.MyPluginDataDic.TryGetValue("SINnerId", out object sinneridobj))
                    {
                        sinneridstring = sinneridobj?.ToString();
                    }

                    if (Guid.TryParse(sinneridstring, out Guid sinnerid))
                    {
                        SINner res = null;
                        try
                        {
                            res = await client.PutSINerInGroupAsync(Guid.Empty, sinnerid, null);
                            var response = await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                            if (res != null)
                                await MainForm.CharacterRoster.LoadCharacters(false, false, false);
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
                    MainForm.ShowMessageBox("Error sharing SINner: " + exception.Message);
                }
            }
            else if (t?.Tag is SINnerSearchGroup ssg)
            {
                ResultGroupPutGroupInGroup res = null;
                try
                {
                    var client = StaticUtils.GetClient();
                    res = await client.PutGroupInGroupAsync(ssg.Id, null, Guid.Empty, null, null);
                    var response = await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                    if (res != null)
                    {
                        await MainForm.CharacterRoster.LoadCharacters(false, false, false);
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
            }
        }

        private async void RemovePinnedOnClick(object sender, EventArgs e)
        {
            TreeNode t = MainForm.CharacterRoster.treCharacterList.SelectedNode;

            if (t?.Tag is CharacterCache objCache)
            {
                try
                {
                    string sinneridstring = null;
                    var client = StaticUtils.GetClient();
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

                            var response = await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                            if (res != null)
                            {
                                await MainForm.CharacterRoster.LoadCharacters(false, false, false);
                            }
                            
                        }
                        catch(ApiException e1)
                        {
                            if (res != null)
                            {
                                var response = await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                                if (res != null)
                                {
                                    await MainForm.CharacterRoster.LoadCharacters(false, false, false);
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
                    MainForm.ShowMessageBox("Error sharing SINner: " + exception.Message);
                }
            }
            else if (t?.Tag is SINnerSearchGroup ssg)
            {
                try
                {
                    var client = StaticUtils.GetClient();
                    var res = await client.PutGroupInGroupAsync(ssg.Id, null, null, null, null);
                    {
                        var response = await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                        if (res != null)
                        {
                            await MainForm.CharacterRoster.LoadCharacters(false, false, false);
                        }
                    }
                }
                catch (Exception exception)
                {
                    ChummerHub.Client.Backend.Utils.HandleError(exception);
                }
            }
        }


        private async void NewShareOnClick(object sender, EventArgs e)
        {
            TreeNode t = MainForm.CharacterRoster.treCharacterList.SelectedNode;

            if (t?.Tag is CharacterCache objCache)
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
                        MainForm.ShowMessageBox("Error sharing SINner: " + exception.Message);
                    }
                }
            }
            else if (t?.Tag is SINnerSearchGroup ssg)
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
                        MainForm.ShowMessageBox("Error sharing Group: " + exception.Message);
                    }
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

        public void CustomInitialize(frmChummerMain mainControl)
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
                    throw new ApplicationException("More than one instance is running.");
                }

                Log.Info("Only one instance, starting NamedPipe-Server...");
                PipeManager.StartServer();
                PipeManager.ReceiveString += HandleNamedPipe_OpenRequest;
            }
        }

        private static string fileNameToLoad = string.Empty;

        public static async void HandleNamedPipe_OpenRequest(string argument)
        {
            Log.Trace("Pipeserver receiced a request: " + argument);
            if (!string.IsNullOrEmpty(argument))
            {
                //make sure the mainform is visible ...
                var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
                if (uptime < TimeSpan.FromSeconds(5))
                    await Task.Delay(TimeSpan.FromSeconds(4)).ConfigureAwait(false);
                if (MainForm.Visible == false)
                {
                    await MainForm.DoThreadSafeAsync(() =>
                    {
                        if (MainForm.WindowState == FormWindowState.Minimized)
                            MainForm.WindowState = FormWindowState.Normal;
                    });
                }

                await MainForm.DoThreadSafeAsync(() =>
                {
                    MainForm.Activate();
                    MainForm.BringToFront();
                });
                var client = StaticUtils.GetClient();
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

                            var found = await client.GetSINByIdAsync(SINnerId);
                            await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(found);
                            if (found?.CallSuccess == true)
                            {
                                await StaticUtils.WebCall(callback, 40,
                                    "Character found online");
                                fileNameToLoad =
                                    await ChummerHub.Client.Backend.Utils.DownloadFileTask(found.MySINner, null);
                                await StaticUtils.WebCall(callback, 70,
                                    "Character downloaded");
                                MainFormLoadChar(fileNameToLoad);
                                await StaticUtils.WebCall(callback, 100,
                                    "Character opened");
                            }
                            else if (found == null || found.CallSuccess == false)
                            {
                                await StaticUtils.WebCall(callback, 0,
                                    "Character not found");
                                MainForm.ShowMessageBox("Could not find a SINner with Id " + SINnerId + " online!");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        MainForm.ShowMessageBox("Error loading SINner: " + e.Message);
                    }

                }
                else
                {
                    throw new ArgumentException("Unkown command received: " + argument, nameof(argument));
                }
            }
        }

        private static void MainFormLoadChar(string fileToLoad)
        {
            //already open
            Character objCharacter = MainForm.OpenCharacters.FirstOrDefault(a => a.FileName == fileToLoad);
            if (objCharacter == null)
            {
                objCharacter = new Character
                {
                    FileName = fileToLoad
                };
                using (frmLoading frmLoadingForm = frmChummerMain.CreateAndShowProgressBar(Path.GetFileName(fileToLoad), Character.NumLoadingSections))
                {
                    if (objCharacter.Load(frmLoadingForm, Settings.Default.IgnoreWarningsOnOpening))
                        MainForm.OpenCharacters.Add(objCharacter);
                    else
                        return;
                }
            }
            using (new CursorWait(MainForm))
            {
                MainForm.DoThreadSafe(() =>
                {
                    if (MainForm.OpenCharacterForms.Any(a => a.CharacterObject == objCharacter))
                    {
                        MainForm.SwitchToOpenCharacter(objCharacter, false);
                    }
                    else
                    {
                        MainForm.OpenCharacter(objCharacter, false);
                    }

                    MainForm.BringToFront();
                });
            }
        }

        public async Task<bool> DoCharacterList_DragDrop(object sender, DragEventArgs e, TreeView treCharacterList)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (treCharacterList == null)
                throw new ArgumentNullException(nameof(treCharacterList));
            try
            {
                // Do not allow the root element to be moved.
                if (treCharacterList.SelectedNode == null || treCharacterList.SelectedNode.Level == 0 ||
                    treCharacterList.SelectedNode.Parent?.Tag?.ToString() == "Watch")
                    return false;

                if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
                {
                    if (!(sender is TreeView treSenderView))
                        return false;
                    Point pt = treSenderView.PointToClient(new Point(e.X, e.Y));
                    TreeNode nodDestinationNode = treSenderView.GetNodeAt(pt);
                    if (nodDestinationNode.Level > 0)
                        nodDestinationNode = nodDestinationNode.Parent;
                    string strDestinationNode = nodDestinationNode.Tag?.ToString();
                    if (strDestinationNode != "Watch")
                    {
                        if (!(e.Data.GetData("System.Windows.Forms.TreeNode") is TreeNode nodNewNode))
                            return false;
                        var client = StaticUtils.GetClient();
                        Guid? mySiNnerId = null;
                        switch (nodNewNode.Tag)
                        {
                            case SINnerSearchGroup sinGroup when nodDestinationNode.Tag == MyPluginHandlerInstance:
                            {
                                var res = await client.PutGroupInGroupAsync(sinGroup.Id, sinGroup.Groupname, null, null, null);
                                await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                                return true;
                            }
                            case SINnerSearchGroup sinGroup when nodDestinationNode.Tag is SINnerSearchGroup destGroup:
                            {
                                var res = await client.PutGroupInGroupAsync(sinGroup.Id, sinGroup.Groupname, destGroup.Id, sinGroup.MyAdminIdentityRole, sinGroup.IsPublic);
                                await ChummerHub.Client.Backend.Utils.ShowErrorResponseFormAsync(res);
                                return true;
                            }
                            case CharacterCache objCache:
                            {
                                object sinidob = null;
                                if (objCache.MyPluginDataDic?.TryGetValue("SINnerId", out sinidob) == true)
                                {
                                    mySiNnerId = (Guid?) sinidob;
                                }
                                else
                                {
                                    using (var ce = await ChummerHub.Client.Backend.Utils.UploadCharacterFromFile(objCache.FilePath))
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
                                var res = await client.PutSINerInGroupAsync(null, mySiNnerId, null);
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
                                        var pwdquestion = LanguageManager.GetString("String_SINners_EnterGroupPassword", true);
                                        var pwdcaption = LanguageManager.GetString("String_SINners_EnterGroupPasswordTitle", true);
                                        passwd = getPWD.ShowDialog(Program.MainForm, pwdquestion, pwdcaption);
                                    }
                                }
                                var res = await client.PutSINerInGroupAsync(destGroup.Id, mySiNnerId, passwd);
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
                await MainForm.CharacterRoster.LoadCharacters(false, false, false);
            }
            return true;
        }
    }
}
