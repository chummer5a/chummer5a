/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;
using Chummer.Backend.Equipment;
using Application = System.Windows.Forms.Application;
using DataFormats = System.Windows.Forms.DataFormats;
using DragDropEffects = System.Windows.Forms.DragDropEffects;
using DragEventArgs = System.Windows.Forms.DragEventArgs;
using Path = System.IO.Path;
using Size = System.Drawing.Size;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Text;
using Microsoft.ApplicationInsights.DataContracts;
using NLog;

namespace Chummer
{
    public sealed partial class frmChummerMain : Form
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private frmDiceRoller _frmRoller;
        private frmUpdate _frmUpdate;
        private readonly ThreadSafeObservableCollection<Character> _lstCharacters = new ThreadSafeObservableCollection<Character>();
        private readonly ObservableCollection<CharacterShared> _lstOpenCharacterForms = new ObservableCollection<CharacterShared>();
        private readonly BackgroundWorker _workerVersionUpdateChecker = new BackgroundWorker();
        private readonly Version _objCurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        private readonly string _strCurrentVersion;
        private Chummy _mascotChummy;

        public string MainTitle
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdTitle = new StringBuilder(Application.ProductName)
                    .Append(strSpace).Append('-').Append(strSpace).Append(LanguageManager.GetString("String_Version"))
                    .Append(strSpace).Append(_strCurrentVersion);
#if DEBUG
                sbdTitle.Append(" DEBUG BUILD");
#endif
                return sbdTitle.ToString();
            }
        }

        #region Control Events
        public frmChummerMain(bool isUnitTest = false)
        {
            Utils.IsUnitTest = isUnitTest;

            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _strCurrentVersion =
                string.Format(GlobalOptions.InvariantCultureInfo, "{0}.{1}.{2}", _objCurrentVersion.Major, _objCurrentVersion.Minor, _objCurrentVersion.Build);

            //lets write that in separate lines to see where the exception is thrown
            if (!GlobalOptions.HideMasterIndex)
            {
                MasterIndex = new frmMasterIndex
                {
                    MdiParent = this
                };
            }
            if (!GlobalOptions.HideCharacterRoster)
            {
                CharacterRoster = new frmCharacterRoster
                {
                    MdiParent = this
                };
            }
        }

        private static readonly string[] s_astrPreloadFileNames =
        {
            "actions.xml",
            "armor.xml",
            "bioware.xml",
            "books.xml",
            "complexforms.xml",
            "contacts.xml",
            "critters.xml",
            "critterpowers.xml",
            "cyberware.xml",
            "drugcomponents.xml",
            "echoes.xml",
            "options.xml",
            "gear.xml",
            "improvements.xml",
            "licenses.xml",
            "lifemodules.xml",
            "lifestyles.xml",
            "martialarts.xml",
            "mentors.xml",
            "metamagic.xml",
            "metatypes.xml",
            "options.xml",
            "packs.xml",
            "powers.xml",
            "priorities.xml",
            "programs.xml",
            "qualities.xml",
            "ranges.xml",
            "settings.xml",
            "sheets.xml",
            "skills.xml",
            "spells.xml",
            "spiritpowers.xml",
            "streams.xml",
            "traditions.xml",
            "vehicles.xml",
            "weapons.xml"
        };

        //Moved most of the initialization out of the constructor to allow the Mainform to be generated fast
        //in case of a commandline argument not asking for the mainform to be shown.
        private async void frmChummerMain_Load(object sender, EventArgs e)
        {
            using (var op_frmChummerMain = Timekeeper.StartSyncron("frmChummerMain_Load", null, CustomActivity.OperationType.DependencyOperation, _strCurrentVersion))
            {
                try
                {
                    op_frmChummerMain.MyDependencyTelemetry.Type = "loadfrmChummerMain";
                    op_frmChummerMain.MyDependencyTelemetry.Target = _strCurrentVersion;

                    if (MyStartupPVT != null)
                    {
                        MyStartupPVT.Duration = DateTimeOffset.UtcNow - MyStartupPVT.Timestamp;
                        op_frmChummerMain.tc.TrackPageView(MyStartupPVT);
                    }

                    Text = MainTitle;

                    //this.toolsMenu.DropDownItems.Add("GM Dashboard").Click += this.dashboardToolStripMenuItem_Click;

                    // If Automatic Updates are enabled, check for updates immediately.

#if !DEBUG
                    _workerVersionUpdateChecker.WorkerReportsProgress = false;
                    _workerVersionUpdateChecker.WorkerSupportsCancellation = true;
                    _workerVersionUpdateChecker.DoWork += DoCacheGitVersion;
                    _workerVersionUpdateChecker.RunWorkerCompleted += CheckForUpdate;
                    Application.Idle += IdleUpdateCheck;
                    _workerVersionUpdateChecker.RunWorkerAsync();
#endif

                    GlobalOptions.MRUChanged += (senderInner, eInner) => { this.DoThreadSafe(() => { PopulateMRUToolstripMenu(senderInner, eInner); }); };

                    try
                    {
                        // Delete the old executable if it exists (created by the update process).
                        string[] oldfiles =
                            Directory.GetFiles(Utils.GetStartupPath, "*.old", SearchOption.AllDirectories);
                        foreach (string strLoopOldFilePath in oldfiles)
                        {
                            try
                            {
                                if (File.Exists(strLoopOldFilePath))
                                    File.Delete(strLoopOldFilePath);
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                //we will just delete it the next time
                                //its probably the "used by another process"
                                Log.Trace(ex,
                                    "UnauthorizedAccessException can be ignored - probably used by another process.");
                            }
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Log.Trace(ex,
                            "UnauthorizedAccessException in " + Utils.GetStartupPath +
                            "can be ignored - probably a weird path like Recycle.Bin or something...");
                    }
                    catch (IOException ex)
                    {
                        Log.Trace(ex,
                            "IOException in " + Utils.GetStartupPath +
                            "can be ignored - probably another instance blocking it...");
                    }

                    // Populate the MRU list.
                    PopulateMRUToolstripMenu(this, null);

                    Program.MainForm = this;

                    using (frmLoading frmProgressBar = new frmLoading { CharacterFile = Text })
                    {
                        frmProgressBar.Reset((GlobalOptions.AllowEasterEggs ? 4 : 3) + s_astrPreloadFileNames.Length);
                        frmProgressBar.Show();

#if DEBUG
                        if (!Utils.IsUnitTest && GlobalOptions.ShowCharacterCustomDataWarning && CurrentVersion.Minor < 215)
#else
                        if (!Utils.IsUnitTest && GlobalOptions.ShowCharacterCustomDataWarning && CurrentVersion.Build > 0 && CurrentVersion.Minor < 215)
#endif
                        {
                            if (ShowMessageBox(LanguageManager.GetString("Message_CharacterCustomDataWarning"),
                                LanguageManager.GetString("MessageTitle_CharacterCustomDataWarning"),
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                            {
                                Application.Exit();
                                return;
                            }

                            GlobalOptions.ShowCharacterCustomDataWarning = false;
                        }

                        // Attempt to cache all XML files that are used the most.
                        using (_ = Timekeeper.StartSyncron("cache_load", op_frmChummerMain))
                        {
                            // Embedding Parallel.ForEach inside Task.Run is hacky but prevents lock-ups
                            await Task.Run(() =>
                                Parallel.ForEach(s_astrPreloadFileNames, x =>
                                {
                                    // Load default language data first for performance reasons
                                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                                        XmlManager.Load(x, null, GlobalOptions.DefaultLanguage);
                                    XmlManager.Load(x);
                                    frmProgressBar.PerformStep(Application.ProductName);
                                }));
                            //Timekeeper.Finish("cache_load");
                        }

                        frmProgressBar.PerformStep(LanguageManager.GetString("String_UI"));

                        _lstCharacters.CollectionChanged += LstCharactersOnCollectionChanged;
                        _lstOpenCharacterForms.CollectionChanged += LstOpenCharacterFormsOnCollectionChanged;

                        // Retrieve the arguments passed to the application. If more than 1 is passed, we're being given the name of a file to open.
                        string[] strArgs = Environment.GetCommandLineArgs();
                        ConcurrentBag<Character> lstCharactersToLoad = new ConcurrentBag<Character>();
                        bool blnShowTest = false;
                        if (!Utils.IsUnitTest && strArgs.Length > 1)
                        {
                            HashSet<string> setFilesToLoad = new HashSet<string>();
                            try
                            {
                                foreach (string strArg in strArgs)
                                {
                                    if (strArg == "/test")
                                    {
                                        blnShowTest = true;
                                    }
                                    else if ((strArg == "/help")
                                             || (strArg == "?")
                                             || (strArg == "/?"))
                                    {
                                        string msg = "Commandline parameters are either " + Environment.NewLine;
                                        msg += "\t/test" + Environment.NewLine;
                                        msg += "\t/help" + Environment.NewLine;
                                        msg += "\t(filename to open)" + Environment.NewLine;
                                        msg += "\t/plugin:pluginname (like \"SINners\") to trigger (with additional parameters following the symbol \":\")" + Environment.NewLine;
                                        Console.WriteLine(msg);
                                    }
                                    else if (strArg.Contains("/plugin"))
                                    {
                                        Log.Info("Encountered command line argument, that should already have been handled in one of the plugins: " + strArg);
                                    }
                                    else if (!strArg.StartsWith('/'))
                                    {
                                        if (!File.Exists(strArg))
                                        {
                                            throw new ArgumentException("Chummer started with unknown command line arguments: " +
                                                                        strArgs.Aggregate((j, k) => j + " " + k));
                                        }
                                        if (setFilesToLoad.Contains(strArg))
                                            continue;
                                        setFilesToLoad.Add(strArg);
                                    }
                                }

                                // Embedding Parallel.ForEach inside Task.Run is hacky but prevents lock-ups
                                await Task.Run(() =>
                                    Parallel.ForEach(setFilesToLoad, async x =>
                                    {
                                        Character objCharacter = await LoadCharacter(x);
                                        lstCharactersToLoad.Add(objCharacter);
                                    }));
                            }
                            catch (Exception ex)
                            {
                                op_frmChummerMain.SetSuccess(false);
                                ExceptionTelemetry ext = new ExceptionTelemetry(ex)
                                {
                                    SeverityLevel = SeverityLevel.Warning
                                };
                                op_frmChummerMain.tc.TrackException(ext);
                                Log.Warn(ex);
                            }
                        }

                        frmProgressBar.PerformStep(LanguageManager.GetString("Title_MasterIndex"));

                        if (MasterIndex != null)
                        {
                            if (CharacterRoster == null)
                                MasterIndex.WindowState = FormWindowState.Maximized;
                            MasterIndex.Show();
                        }

                        frmProgressBar.PerformStep(LanguageManager.GetString("String_CharacterRoster"));
                        
                        if (CharacterRoster != null)
                        {
                            if (MasterIndex == null)
                                CharacterRoster.WindowState = FormWindowState.Maximized;
                            CharacterRoster.Show();
                        }

                        if (GlobalOptions.AllowEasterEggs)
                        {
                            frmProgressBar.PerformStep(LanguageManager.GetString("String_Chummy"));
                            _mascotChummy = new Chummy(null);
                            _mascotChummy.Show(this);
                        }

                        // This weird ordering of WindowState after Show() is meant to counteract a weird WinForms issue where form handle creation crashes
                        if (MasterIndex != null && CharacterRoster != null)
                        {
                            MasterIndex.WindowState = FormWindowState.Maximized;
                            CharacterRoster.WindowState = FormWindowState.Maximized;
                        }

                        if (blnShowTest)
                        {
                            frmTest frmTestData = new frmTest();
                            frmTestData.Show();
                        }

                        if (lstCharactersToLoad.Count > 0)
                            OpenCharacterList(lstCharactersToLoad);
                    }

                    Program.PluginLoader.CallPlugins(toolsMenu, op_frmChummerMain);

                    // Set the Tag for each ToolStrip item so it can be translated.
                    foreach (ToolStripMenuItem tssItem in menuStrip.Items.OfType<ToolStripMenuItem>())
                    {
                        tssItem.UpdateLightDarkMode();
                        tssItem.TranslateToolStripItemsRecursively();
                    }

                    foreach (ToolStripMenuItem tssItem in mnuProcessFile.Items.OfType<ToolStripMenuItem>())
                    {
                        tssItem.UpdateLightDarkMode();
                        tssItem.TranslateToolStripItemsRecursively();
                    }
                }
                catch (Exception ex)
                {
                    if (op_frmChummerMain != null)
                    {
                        op_frmChummerMain.SetSuccess(false);
                        op_frmChummerMain.tc.TrackException(ex);
                    }

                    Log.Error(ex);
                    throw;
                }

                //sometimes the Configuration gets messed up - make sure it is valid!
                try
                {
                    Size _ = Properties.Settings.Default.Size;
                }
                catch (ArgumentException ex)
                {
                    //the config is invalid - reset it!
                    Properties.Settings.Default.Reset();
                    Properties.Settings.Default.Save();
                    Log.Warn("Configuartion Settings were invalid and had to be reset. Exception: " + ex.Message);
                }
                catch (System.Configuration.ConfigurationErrorsException ex)
                {
                    //the config is invalid - reset it!
                    Properties.Settings.Default.Reset();
                    Properties.Settings.Default.Save();
                    Log.Warn("Configuartion Settings were invalid and had to be reset. Exception: " + ex.Message);
                }

                if (Properties.Settings.Default.Size.Width == 0 || Properties.Settings.Default.Size.Height == 0 || !IsVisibleOnAnyScreen())
                {
                    int intDefaultWidth = 1280;
                    int intDefaultHeight = 720;
                    using (Graphics g = CreateGraphics())
                    {
                        intDefaultWidth = (int)(intDefaultWidth * g.DpiX / 96.0f);
                        intDefaultHeight = (int)(intDefaultHeight * g.DpiY / 96.0f);
                    }
                    Size = new Size(intDefaultWidth, intDefaultHeight);
                    StartPosition = FormStartPosition.CenterScreen;
                }
                else
                {
                    WindowState = Properties.Settings.Default.WindowState;

                    if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;

                    Location = Properties.Settings.Default.Location;
                    Size = Properties.Settings.Default.Size;
                }

                if (GlobalOptions.StartupFullscreen)
                    WindowState = FormWindowState.Maximized;
            }
        }

        public PageViewTelemetry MyStartupPVT { get; set; }

        private void LstOpenCharacterFormsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(CharacterRoster != null)
            {
                switch(e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        CharacterRoster.RefreshNodes();
                        break;
                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Remove:
                        {
                            bool blnRefreshSticky = false;
                            foreach(CharacterShared objClosedForm in e.OldItems)
                            {
                                if(GlobalOptions.FavoritedCharacters.Contains(objClosedForm.CharacterObject.FileName))
                                {
                                    blnRefreshSticky = true;
                                    break;
                                }
                            }

                            // Need a full refresh because the recents list in the character roster also shows open characters that are not in the most recently used list because of it being too full
                            CharacterRoster.PopulateCharacterList(this, new TextEventArgs(blnRefreshSticky ? "stickymru" : "mru"));
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            bool blnRefreshSticky = false;
                            foreach(CharacterShared objClosedForm in e.OldItems)
                            {
                                if(GlobalOptions.FavoritedCharacters.Contains(objClosedForm.CharacterObject.FileName))
                                {
                                    blnRefreshSticky = true;
                                    break;
                                }
                            }

                            if(!blnRefreshSticky)
                            {
                                foreach(CharacterShared objNewForm in e.NewItems)
                                {
                                    if(GlobalOptions.FavoritedCharacters.Contains(objNewForm.CharacterObject.FileName))
                                    {
                                        blnRefreshSticky = true;
                                        break;
                                    }
                                }
                            }

                            // Need a full refresh because the recents list in the character roster also shows open characters that are not in the most recently used list because of it being too full
                            CharacterRoster.PopulateCharacterList(this, new TextEventArgs(blnRefreshSticky ? "stickymru" : "mru"));
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        CharacterRoster.PopulateCharacterList(this, null);
                        break;
                }
            }
        }

        private void LstCharactersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            switch(notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach(Character objCharacter in notifyCollectionChangedEventArgs.NewItems)
                            objCharacter.PropertyChanged += UpdateCharacterTabTitle;
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach(Character objCharacter in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objCharacter.PropertyChanged -= UpdateCharacterTabTitle;
                            objCharacter.Dispose();
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach(Character objCharacter in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objCharacter.PropertyChanged -= UpdateCharacterTabTitle;
                            if (!notifyCollectionChangedEventArgs.NewItems.Contains(objCharacter))
                                objCharacter.Dispose();
                        }
                        foreach(Character objCharacter in notifyCollectionChangedEventArgs.NewItems)
                            objCharacter.PropertyChanged += UpdateCharacterTabTitle;
                        break;
                    }
            }
        }

        public frmCharacterRoster CharacterRoster { get; }

        public frmMasterIndex MasterIndex { get; }

        private Uri UpdateLocation { get; } = new Uri(GlobalOptions.PreferNightlyBuilds
            ? "https://api.github.com/repos/chummer5a/chummer5a/releases"
            : "https://api.github.com/repos/chummer5a/chummer5a/releases/latest");

        private void DoCacheGitVersion(object sender, DoWorkEventArgs e)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest request;
            try
            {
                WebRequest objTemp = WebRequest.Create(UpdateLocation);
                request = objTemp as HttpWebRequest;
            }
            catch(System.Security.SecurityException ex)
            {
                Utils.CachedGitVersion = null;
                Log.Error(ex);
                return;
            }
            if(request == null)
            {
                Utils.CachedGitVersion = null;
                return;
            }

            if(_workerVersionUpdateChecker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            request.Accept = "application/json";

            try
            {
                // Get the response.
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response == null)
                    {
                        Utils.CachedGitVersion = null;
                        return;
                    }

                    if (_workerVersionUpdateChecker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    // Get the stream containing content returned by the server.
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        if (dataStream == null)
                        {
                            Utils.CachedGitVersion = null;
                            return;
                        }

                        if (_workerVersionUpdateChecker.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }

                        // Open the stream using a StreamReader for easy access.
                        using (StreamReader reader = new StreamReader(dataStream, Encoding.UTF8, true))
                        {
                            if (_workerVersionUpdateChecker.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }

                            // Read the content.
                            string responseFromServer = reader.ReadToEnd();

                            if (_workerVersionUpdateChecker.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }

                            string line = responseFromServer.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(x => x.Contains("tag_name"));

                            if (_workerVersionUpdateChecker.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }

                            Version verLatestVersion = null;
                            if (!string.IsNullOrEmpty(line))
                            {
                                string strVersion = line.Substring(line.IndexOf(':') + 1);
                                int intPos = strVersion.IndexOf('}');
                                if (intPos != -1)
                                    strVersion = strVersion.Substring(0, intPos);
                                strVersion = strVersion.FastEscape('\"');

                                // Adds zeroes if minor and/or build version are missing
                                while (strVersion.Count(x => x == '.') < 2)
                                {
                                    strVersion += ".0";
                                }

                                if (_workerVersionUpdateChecker.CancellationPending)
                                {
                                    e.Cancel = true;
                                    return;
                                }

                                if (!Version.TryParse(strVersion.TrimStartOnce("Nightly-v"), out verLatestVersion))
                                    verLatestVersion = null;

                                if (_workerVersionUpdateChecker.CancellationPending)
                                {
                                    e.Cancel = true;
                                    return;
                                }
                            }

                            Utils.CachedGitVersion = verLatestVersion;
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                Utils.CachedGitVersion = null;
                Log.Error(ex);
            }
        }

        private void CheckForUpdate(object sender, RunWorkerCompletedEventArgs e)
        {
            if(!e.Cancelled && Utils.GitUpdateAvailable > 0)
            {
                if(GlobalOptions.AutomaticUpdate)
                {
                    if(_frmUpdate == null)
                    {
                        _frmUpdate = new frmUpdate();
                        _frmUpdate.FormClosed += ResetFrmUpdate;
                        _frmUpdate.SilentMode = true;
                    }
                }
                string strSpace = LanguageManager.GetString("String_Space");
                Text = new StringBuilder(Application.ProductName)
                    .Append(strSpace).Append('-')
                    .Append(strSpace).Append(LanguageManager.GetString("String_Version"))
                    .Append(strSpace).Append(_strCurrentVersion)
                    .Append(strSpace).Append('-')
                    .Append(strSpace).AppendFormat(GlobalOptions.CultureInfo, LanguageManager.GetString("String_Update_Available"),
                        Utils.CachedGitVersion).ToString();
            }
        }

        private readonly Stopwatch _idleUpdateCheckStopWatch = Stopwatch.StartNew();
        private void IdleUpdateCheck(object sender, EventArgs e)
        {
            // Automatically check for updates every hour
            if(_idleUpdateCheckStopWatch.ElapsedMilliseconds >= 3600000 && !_workerVersionUpdateChecker.IsBusy)
            {
                _idleUpdateCheckStopWatch.Restart();
                _workerVersionUpdateChecker.RunWorkerAsync();
            }
        }

        /*
        public sealed override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }
        */

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /*
        private void dashboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmGMDashboard.Instance.Show();
        }
        */

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(Form childForm in MdiChildren)
            {
                if (childForm != CharacterRoster && childForm != MasterIndex)
                    childForm.Close();
            }
        }

        private void mnuOptions_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
                using (frmOptions frmOptions = new frmOptions())
                    frmOptions.ShowDialog(this);
        }

        private void mnuCharacterOptions_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
                using (frmCharacterOptions frmCharacterOptions = new frmCharacterOptions((tabForms.SelectedTab?.Tag as CharacterShared)?.CharacterObject?.Options))
                    frmCharacterOptions.ShowDialog(this);
        }

        private void mnuToolsUpdate_Click(object sender, EventArgs e)
        {
            // Only a single instance of the updater can be open, so either find the current instance and focus on it, or create a new one.
            if(_frmUpdate == null)
            {
                _frmUpdate = new frmUpdate();
                _frmUpdate.FormClosed += ResetFrmUpdate;
                _frmUpdate.Show();
            }
            // Silent updater is running, so make it visible
            else if(_frmUpdate.SilentMode)
            {
                _frmUpdate.SilentMode = false;
                _frmUpdate.Show();
            }
            else
            {
                _frmUpdate.Focus();
            }
        }

        private void ResetFrmUpdate(object sender, EventArgs e)
        {
            _frmUpdate = null;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (frmAbout frmShowAbout = new frmAbout())
                frmShowAbout.ShowDialog(this);
        }

        private void mnuChummerWiki_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/chummer5a/chummer5a/wiki/");
        }

        private void mnuChummerDiscord_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/mJB7st9");
        }

        private void mnuHelpDumpshock_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/chummer5a/chummer5a/issues/");
        }

		public frmPrintMultiple PrintMultipleCharactersForm { get; private set; }

		private void mnuFilePrintMultiple_Click(object sender, EventArgs e)
        {
            if(PrintMultipleCharactersForm == null || PrintMultipleCharactersForm.IsDisposed)
                PrintMultipleCharactersForm = new frmPrintMultiple();
            else
                PrintMultipleCharactersForm.Activate();
            PrintMultipleCharactersForm.Show(this);
        }

        private void mnuHelpRevisionHistory_Click(object sender, EventArgs e)
        {
            using (frmHistory frmShowHistory = new frmHistory())
                frmShowHistory.ShowDialog(this);
        }

        private void mnuNewCritter_Click(object sender, EventArgs e)
        {
            Character objCharacter = new Character();

            using (new CursorWait(this))
            {
                using (frmSelectBuildMethod frmPickSetting = new frmSelectBuildMethod(objCharacter))
	            {
	                frmPickSetting.ShowDialog(this);
                    if (frmPickSetting.DialogResult == DialogResult.Cancel)
	                    return;
	            }

                // Override the defaults for the setting.
                objCharacter.IgnoreRules = true;
                objCharacter.IsCritter = true;
                objCharacter.Created = true;

                // Show the Metatype selection window.
                using (frmKarmaMetatype frmSelectMetatype = new frmKarmaMetatype(objCharacter, "critters.xml"))
                {
                    frmSelectMetatype.ShowDialog(this);

                    if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                        return;
                }

                // Add the Unarmed Attack Weapon to the character.
                XmlNode objXmlWeapon = objCharacter.LoadData("weapons.xml").SelectSingleNode("/chummer/weapons/weapon[name = \"Unarmed Attack\"]");
                if (objXmlWeapon != null)
                {
                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    Weapon objWeapon = new Weapon(objCharacter);
                    objWeapon.Create(objXmlWeapon, lstWeapons);
                    objWeapon.ParentID = Guid.NewGuid().ToString("D", GlobalOptions.InvariantCultureInfo); // Unarmed Attack can never be removed
                    objCharacter.Weapons.Add(objWeapon);
                    foreach (Weapon objLoopWeapon in lstWeapons)
                        objCharacter.Weapons.Add(objLoopWeapon);
                }

                frmCareer frmNewCharacter = new frmCareer(objCharacter)
                {
                    MdiParent = this
                };
                if (MdiChildren.Length <= 1)
                    frmNewCharacter.WindowState = FormWindowState.Maximized;
                frmNewCharacter.Show();
                // This weird ordering of WindowState after Show() is meant to counteract a weird WinForms issue where form handle creation crashes
                if (MdiChildren.Length > 1)
                    frmNewCharacter.WindowState = FormWindowState.Maximized;
            }
        }

        private async void mnuMRU_Click(object sender, EventArgs e)
        {
            string strFileName = ((ToolStripMenuItem)sender).Text;
            strFileName = strFileName.Substring(3, strFileName.Length - 3).Trim();
            using (new CursorWait(this))
            {
                Character objOpenCharacter = await LoadCharacter(strFileName).ConfigureAwait(false);
                Program.MainForm.OpenCharacter(objOpenCharacter);
            }
        }

        private void mnuMRU_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                string strFileName = ((ToolStripMenuItem)sender).Tag as string;
                if (!string.IsNullOrEmpty(strFileName))
                    GlobalOptions.FavoritedCharacters.Add(strFileName);
            }
        }

        private async void mnuStickyMRU_Click(object sender, EventArgs e)
        {
            string strFileName = ((ToolStripMenuItem)sender).Tag as string;
            if (string.IsNullOrEmpty(strFileName))
                return;
            using (new CursorWait(this))
            {
                Character objOpenCharacter = await LoadCharacter(strFileName).ConfigureAwait(false);
                Program.MainForm.OpenCharacter(objOpenCharacter);
            }
        }

        private void mnuStickyMRU_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                string strFileName = ((ToolStripMenuItem)sender).Tag as string;

                if (!string.IsNullOrEmpty(strFileName))
                {
                    GlobalOptions.FavoritedCharacters.Remove(strFileName);
                    GlobalOptions.MostRecentlyUsedCharacters.Insert(0, strFileName);
                }
            }
        }

        private void frmChummerMain_MdiChildActivate(object sender, EventArgs e)
        {
            // If there are no child forms, hide the tab control.
            if(ActiveMdiChild != null)
            {
                if (ActiveMdiChild.WindowState == FormWindowState.Minimized)
                {
                    ActiveMdiChild.WindowState = FormWindowState.Normal;
                }

                // If this is a new child form and does not have a tab page, create one.
                if (!(ActiveMdiChild.Tag is TabPage))
                {
                    TabPage tp = new TabPage
                    {
                        // Add a tab page.
                        Tag = ActiveMdiChild,
                        Parent = tabForms
                    };

                    if(ActiveMdiChild is CharacterShared frmCharacterShared)
                    {
                        tp.Text = frmCharacterShared.CharacterObject.CharacterName;
                        if (GlobalOptions.AllowEasterEggs && _mascotChummy != null)
                        {
                            _mascotChummy.CharacterObject = frmCharacterShared.CharacterObject;
                        }
                    }
                    else
                    {
                        string strTagText = LanguageManager.GetString(ActiveMdiChild.Tag?.ToString(), GlobalOptions.Language, false);
                        if(!string.IsNullOrEmpty(strTagText))
                            tp.Text = strTagText;
                    }

                    tabForms.SelectedTab = tp;

                    ActiveMdiChild.Tag = tp;
                    ActiveMdiChild.FormClosed += ActiveMdiChild_FormClosed;
                }
            }
            // Don't show the tab control if there is only one window open.
            tabForms.Visible = tabForms.TabCount > 1;
        }

        private void ActiveMdiChild_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(sender is Form objForm)
            {
                objForm.FormClosed -= ActiveMdiChild_FormClosed;
                objForm.Dispose();
                (objForm.Tag as TabPage)?.Dispose();
            }

            // Don't show the tab control if there is only one window open.
            if(tabForms.TabCount <= 1)
                tabForms.Visible = false;
        }

        private void tabForms_SelectedIndexChanged(object sender, EventArgs e)
        {
            (tabForms.SelectedTab?.Tag as Form)?.Select();
        }

        public bool SwitchToOpenCharacter(Character objCharacter, bool blnIncludeInMRU)
        {
            if(objCharacter != null)
            {
                Form objCharacterForm = OpenCharacterForms.FirstOrDefault(x => x.CharacterObject == objCharacter);
                if(objCharacterForm != null)
                {
                    foreach(TabPage objTabPage in tabForms.TabPages)
                    {
                        if(objTabPage.Tag == objCharacterForm)
                        {
                            tabForms.SelectTab(objTabPage);
                            if (_mascotChummy != null)
                                _mascotChummy.CharacterObject = objCharacter;
                            return true;
                        }
                    }
                }
                if(OpenCharacters.Contains(objCharacter))
                {
                    using (new CursorWait(this))
                        OpenCharacter(objCharacter, blnIncludeInMRU);
                    return true;
                }
            }
            return false;
        }

        public void UpdateCharacterTabTitle(object sender, PropertyChangedEventArgs e)
        {
            // Change the TabPage's text to match the character's name (or "Unnamed Character" if they are currently unnamed).
            if(tabForms.TabCount > 0 && e?.PropertyName == nameof(Character.CharacterName) && sender is Character objCharacter)
            {
                foreach(TabPage objTabPage in tabForms.TabPages)
                {
                    if(objTabPage.Tag is CharacterShared objCharacterForm && objCharacterForm.CharacterObject == objCharacter)
                    {
                        objTabPage.Text = objCharacter.CharacterName.Trim();
                        return;
                    }
                }
            }
        }

        private void mnuToolsDiceRoller_Click(object sender, EventArgs e)
        {
            if(GlobalOptions.SingleDiceRoller)
            {
                // Only a single instance of the Dice Roller window is allowed, so either find the existing one and focus on it, or create a new one.
                if(_frmRoller == null)
                {
                    _frmRoller = new frmDiceRoller(this);
                    _frmRoller.Show();
                }
                else
                {
                    _frmRoller.Activate();
                }
            }
            else
            {
                // No limit on the number of Dice Roller windows, so just create a new one.
                frmDiceRoller frmRoller = new frmDiceRoller(this);
                frmRoller.Show();
            }
        }

        private void menuStrip_ItemAdded(object sender, ToolStripItemEventArgs e)
        {
            // Translate the items in the menu by finding their Tags in the translation file.
            foreach(ToolStripItem tssItem in menuStrip.Items.OfType<ToolStripItem>())
            {
                tssItem.UpdateLightDarkMode();
                tssItem.TranslateToolStripItemsRecursively();
            }
        }

        private void toolStrip_ItemAdded(object sender, ToolStripItemEventArgs e)
        {
            // ToolStrip Items.
            foreach(ToolStrip objToolStrip in Controls.OfType<ToolStrip>())
            {
                foreach(ToolStripItem tssItem in objToolStrip.Items.OfType<ToolStripItem>())
                {
                    tssItem.UpdateLightDarkMode();
                    tssItem.TranslateToolStripItemsRecursively();
                }
            }
        }

        private void toolStrip_ItemRemoved(object sender, ToolStripItemEventArgs e)
        {
            // ToolStrip Items.
            foreach(ToolStrip objToolStrip in Controls.OfType<ToolStrip>())
            {
                foreach(ToolStripItem tssItem in objToolStrip.Items.OfType<ToolStripItem>())
                {
                    tssItem.UpdateLightDarkMode();
                    tssItem.TranslateToolStripItemsRecursively();
                }
            }
        }

        private static bool IsVisibleOnAnyScreen()
        {
            return Screen.AllScreens.Any(screen => screen.WorkingArea.Contains(Properties.Settings.Default.Location));
        }

        private async void frmChummerMain_DragDrop(object sender, DragEventArgs e)
        {
            using (new CursorWait(this))
            {
                // Open each file that has been dropped into the window.
                string[] s = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
                if (s.Length == 0)
                    return;
                Dictionary<int, string> dicIndexedStrings = new Dictionary<int, string>(s.Length);
                for (int i = 0; i < s.Length; ++i)
                {
                    dicIndexedStrings.Add(i, s[i]);
                }
                // Array with locker instead of concurrent bag because we want to preserve order
                Character[] lstCharacters = new Character[s.Length];
                // Embedding Parallel.ForEach inside Task.Run is hacky but prevents lock-ups
                await Task.Run(() => Parallel.ForEach(dicIndexedStrings, async x => lstCharacters[x.Key] = await LoadCharacter(x.Value)));
                Program.MainForm.OpenCharacterList(lstCharacters);
            }
        }

        private void frmChummerMain_DragEnter(object sender, DragEventArgs e)
        {
            // Only use a drop effect if a file is being dragged into the window.
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void mnuToolsTranslator_Click(object sender, EventArgs e)
        {
            string strTranslator = Path.Combine(Utils.GetStartupPath, "Translator.exe");
            if(File.Exists(strTranslator))
                Process.Start(strTranslator);
        }

        private void frmChummerMain_Closing(object sender, FormClosingEventArgs e)
        {
            if (_workerVersionUpdateChecker.IsBusy)
                _workerVersionUpdateChecker.CancelAsync();
            Properties.Settings.Default.WindowState = WindowState;
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.Location = Location;
                Properties.Settings.Default.Size = Size;
            }
            else
            {
                Properties.Settings.Default.Location = RestoreBounds.Location;
                Properties.Settings.Default.Size = RestoreBounds.Size;
            }

            try
            {
                Properties.Settings.Default.Save();
            }
            catch (IOException ex)
            {
                Log.Warn(ex, ex.Message);
            }
        }

        private void mnuHeroLabImporter_Click(object sender, EventArgs e)
        {
            if (ShowMessageBox(LanguageManager.GetString("Message_HeroLabImporterWarning"),
                    LanguageManager.GetString("Message_HeroLabImporterWarning_Title"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            frmHeroLabImporter frmImporter = new frmHeroLabImporter();
            frmImporter.Show();
        }

        private void tabForms_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < tabForms.TabCount; ++i)
                {
                    if (!tabForms.GetTabRect(i).Contains(e.Location)) continue;
                    if (tabForms.SelectedTab.Tag is CharacterShared)
                    {
                        if (tabForms.SelectedIndex == i)
                        {
                            mnuProcessFile.Show(this, e.Location);
                            break;
                        }
                    }
                }
            }
        }

        private void tsSave_Click(object sender, EventArgs e)
        {
            if (tabForms.SelectedTab.Tag is CharacterShared objShared)
            {
                objShared.SaveCharacter();
            }
        }

        private void tsSaveAs_Click(object sender, EventArgs e)
        {
            if (tabForms.SelectedTab.Tag is CharacterShared objShared)
            {
                objShared.SaveCharacterAs();
            }
        }

        private void tsClose_Click(object sender, EventArgs e)
        {
            if (tabForms.SelectedTab.Tag is CharacterShared objShared)
            {
                objShared.Close();
            }
        }

        private void tsPrint_Click(object sender, EventArgs e)
        {
            if (tabForms.SelectedTab.Tag is CharacterShared objShared)
            {
                objShared.DoPrint();
            }
        }

        private void frmChummerMain_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            tabForms.ItemSize = new Size(
                tabForms.ItemSize.Width * e.DeviceDpiNew / Math.Max(e.DeviceDpiOld, 1),
                tabForms.ItemSize.Height * e.DeviceDpiNew / Math.Max(e.DeviceDpiOld, 1));
        }
#endregion

#region Methods

        private static bool showDevWarningAboutDebuggingOnlyOnce = true;

        /// <summary>
        /// This makes sure, that the MessageBox is shown in the UI Thread.
        /// https://stackoverflow.com/questions/559252/does-messagebox-show-automatically-marshall-to-the-ui-thread
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        /// <param name="icon"></param>
        /// <param name="defaultButton"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public DialogResult ShowMessageBox(string message, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            return ShowMessageBox(null, message, caption, buttons, icon);
        }

        public DialogResult ShowMessageBox(Control owner, string message, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            if (Utils.IsUnitTest)
            {
                string msg = "We don't want to see MessageBoxes in Unit Tests!" + Environment.NewLine;
                msg += "Caption: " + caption + Environment.NewLine;
                msg += "Message: " + message;
                throw new ArgumentException(msg);
            }

            if (owner == null)
                owner = this;

            if (owner.InvokeRequired)
            {
                if ((showDevWarningAboutDebuggingOnlyOnce) && (Debugger.IsAttached))
                {
                    showDevWarningAboutDebuggingOnlyOnce = false;
                    //it works on my installation even in the debugger, so maybe we can ignore that...
                    //WARNING from the link above (you can edit that out if it's not causing problem):
                    //
                    //BUT ALSO KEEP IN MIND: when debugging a multi-threaded GUI app, and you're debugging in a thread
                    //other than the main/application thread, YOU NEED TO TURN OFF
                    //the "Enable property evaluation and other implicit function calls" option, or else VS will
                    //automatically fetch the values of local/global GUI objects FROM THE CURRENT THREAD, which will
                    //cause your application to crash/fail in strange ways. Go to Tools->Options->Debugging to turn
                    //that setting off.
                    Debugger.Break();
                }

                try
                {
                    return (DialogResult)owner.Invoke(new PassStringStringReturnDialogResultDelegate(ShowMessageBox),
                        message, caption, buttons, icon, defaultButton);
                }
                catch (ObjectDisposedException)
                {
                    //if the main form is disposed, we really don't need to bother anymore...
                }
                catch (Exception e)
                {
                    string msg = "Could not show a MessageBox " + caption + ":" + Environment.NewLine;
                    msg += message + Environment.NewLine + Environment.NewLine;
                    msg += "Exception: " + e;
                    Log.Fatal(e, msg);
                }
            }

            return CenterableMessageBox.Show(this as IWin32Window, message, caption, buttons, icon, defaultButton);
        }

        public delegate DialogResult PassStringStringReturnDialogResultDelegate(
            string s1, string s2, MessageBoxButtons buttons,
            MessageBoxIcon icon, MessageBoxDefaultButton defaultButton);

        /// <summary>
        /// Create a new character and show the Create Form.
        /// </summary>
        private void ShowNewForm(object sender, EventArgs e)
        {
            Character objCharacter = new Character();
            using (new CursorWait(this))
            {
                // Show the BP selection window.
	            using (frmSelectBuildMethod frmBP = new frmSelectBuildMethod(objCharacter))
	            {
	                frmBP.ShowDialog(this);
                    if (frmBP.DialogResult == DialogResult.Cancel)
                    {
                        objCharacter.Dispose();
                        return;
                    }
                }
                // Show the Metatype selection window.
	            if (objCharacter.EffectiveBuildMethodUsesPriorityTables)
	            {
	                using (frmPriorityMetatype frmSelectMetatype = new frmPriorityMetatype(objCharacter))
                    {
                        frmSelectMetatype.ShowDialog(this);

                        if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                        {
                            objCharacter.Dispose();
                            return;
                        }
                    }
                }
                else
                {
                    using (frmKarmaMetatype frmSelectMetatype = new frmKarmaMetatype(objCharacter))
                    {
                        frmSelectMetatype.ShowDialog(this);

                        if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                        {
                            objCharacter.Dispose();
                            return;
                        }
                    }
                }

                // Add the Unarmed Attack Weapon to the character.
                XmlNode objXmlWeapon = objCharacter.LoadData("weapons.xml").SelectSingleNode("/chummer/weapons/weapon[name = \"Unarmed Attack\"]");
                if (objXmlWeapon != null)
                {
                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    Weapon objWeapon = new Weapon(objCharacter);
                    objWeapon.Create(objXmlWeapon, lstWeapons);
                    objWeapon.ParentID = Guid.NewGuid().ToString("D", GlobalOptions.InvariantCultureInfo); // Unarmed Attack can never be removed
                    objCharacter.Weapons.Add(objWeapon);
                    foreach (Weapon objLoopWeapon in lstWeapons)
                        objCharacter.Weapons.Add(objLoopWeapon);
                }

                OpenCharacters.Add(objCharacter);
            }

            using (new CursorWait(this))
            {
                frmCreate frmNewCharacter = new frmCreate(objCharacter)
                {
                    MdiParent = this
                };
                if (MdiChildren.Length <= 1)
                    frmNewCharacter.WindowState = FormWindowState.Maximized;
                frmNewCharacter.Show();
                // This weird ordering of WindowState after Show() is meant to counteract a weird WinForms issue where form handle creation crashes
                if (MdiChildren.Length > 1)
                    frmNewCharacter.WindowState = FormWindowState.Maximized;
            }
        }

        /// <summary>
        /// Show the Open File dialogue, then load the selected character.
        /// </summary>
        private async void OpenFile(object sender, EventArgs e)
        {
            using (new CursorWait(this))
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = LanguageManager.GetString("DialogFilter_Chum5") + '|' +
                             LanguageManager.GetString("DialogFilter_All"),
                    Multiselect = true
                })
                {
                    if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                        return;
                    // Array with locker instead of concurrent bag because we want to preserve order
                    Character[] lstCharacters = null;
                    //Timekeeper.Start("load_sum");
                    using (new CursorWait(this))
                    {
                        List<string> lstFilesToOpen = new List<string>(openFileDialog.FileNames.Length);
                        foreach (string strFile in openFileDialog.FileNames)
                        {
                            Character objLoopCharacter = OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
                            if (objLoopCharacter != null)
                                SwitchToOpenCharacter(objLoopCharacter, true);
                            else
                                lstFilesToOpen.Add(strFile);
                        }

                        if (lstFilesToOpen.Count > 0)
                        {
                            using (frmLoading frmProgressBar = new frmLoading
                            {
                                CharacterFile = string.Join(',' + LanguageManager.GetString("String_Space"),
                                    lstFilesToOpen)
                            })
                            {
                                frmProgressBar.Reset(lstFilesToOpen.Count);
                                frmProgressBar.Show();
                                lstCharacters = new Character[lstFilesToOpen.Count];
                                Dictionary<int, string> dicIndexedStrings = new Dictionary<int, string>(lstFilesToOpen.Count);
                                for (int i = 0; i < lstFilesToOpen.Count; ++i)
                                {
                                    dicIndexedStrings.Add(i, lstFilesToOpen[i]);
                                }
                                // Embedding Parallel.ForEach inside Task.Run is hacky but prevents lock-ups
                                await Task.Run(() =>
                                    Parallel.ForEach(dicIndexedStrings, async x =>
                                    {
                                        lstCharacters[x.Key] = await LoadCharacter(x.Value, string.Empty, false, true, false);
                                        frmProgressBar.PerformStep();
                                    }));
                            }
                        }
                    }
                    Program.MainForm.OpenCharacterList(lstCharacters);
                }
            }

            Application.DoEvents();
            //Timekeeper.Finish("load_sum");
            //Timekeeper.Log();
        }

        /// <summary>
        /// Opens the correct window for a single character (not thread-safe).
        /// </summary>
        public void OpenCharacter(Character objCharacter, bool blnIncludeInMRU = true)
        {
            OpenCharacterList(objCharacter.Yield(), blnIncludeInMRU);
        }

        /// <summary>
        /// Open the correct windows for a list of characters (not thread-safe).
        /// </summary>
        /// <param name="lstCharacters">Characters for which windows should be opened.</param>
        /// <param name="blnIncludeInMRU">Added the opened characters to the Most Recently Used list.</param>
        public void OpenCharacterList(IEnumerable<Character> lstCharacters, bool blnIncludeInMRU = true)
        {
            if(lstCharacters == null)
                return;

            FormWindowState wsPreference = MdiChildren.Length == 0
                                           || MdiChildren.Any(x => x.WindowState == FormWindowState.Maximized)
                ? FormWindowState.Maximized
                : FormWindowState.Normal;
            List<CharacterShared> lstNewFormsToProcess = new List<CharacterShared>();
            foreach (Character objCharacter in lstCharacters)
            {
                if (objCharacter == null || OpenCharacterForms.Any(x => x.CharacterObject == objCharacter))
                    continue;
                //Timekeeper.Start("load_event_time");
                // Show the character forms.
                CharacterShared frmNewCharacter = objCharacter.Created
                    ? (CharacterShared)new frmCareer(objCharacter)
                    : new frmCreate(objCharacter);
                frmNewCharacter.MdiParent = this;
                frmNewCharacter.Show();
                lstNewFormsToProcess.Add(frmNewCharacter);

                if (blnIncludeInMRU && !string.IsNullOrEmpty(objCharacter.FileName) && File.Exists(objCharacter.FileName))
                    GlobalOptions.MostRecentlyUsedCharacters.Insert(0, objCharacter.FileName);

                UpdateCharacterTabTitle(objCharacter, new PropertyChangedEventArgs(nameof(Character.CharacterName)));

                //Timekeeper.Finish("load_event_time");
            }
            // This weird ordering of WindowState after Show() is meant to counteract a weird WinForms issue where form handle creation crashes
            foreach (CharacterShared frmNewCharacter in lstNewFormsToProcess)
                frmNewCharacter.WindowState = wsPreference;
        }

        /// <summary>
        /// Load a Character from a file and return it (thread-safe).
        /// </summary>
        /// <param name="strFileName">File to load.</param>
        /// <param name="strNewName">New name for the character.</param>
        /// <param name="blnClearFileName">Whether or not the name of the save file should be cleared.</param>
        /// <param name="blnShowErrors">Show error messages if the character failed to load.</param>
        /// <param name="blnShowProgressBar">Show loading bar for the character.</param>
        public async Task<Character> LoadCharacter(string strFileName, string strNewName = "", bool blnClearFileName = false, bool blnShowErrors = true, bool blnShowProgressBar = true)
        {
            if (string.IsNullOrEmpty(strFileName))
                return null;
            Character objCharacter = null;
            if(File.Exists(strFileName) && strFileName.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
            {
                //Timekeeper.Start("loading");
                bool blnLoadAutosave = false;
                string strAutosavesPath = Path.Combine(Utils.GetStartupPath, "saves", "autosave");
                objCharacter = new Character
                {
                    FileName = strFileName
                };
                if (blnShowErrors) // Only do the autosave prompt if we will show prompts
                {
                    if (!strFileName.StartsWith(strAutosavesPath))
                    {
                        string strNewAutosaveName = Path.GetFileName(strFileName);
                        if (!string.IsNullOrEmpty(strNewAutosaveName))
                        {
                            strNewAutosaveName = Path.Combine(strAutosavesPath, strNewAutosaveName);
                            if (File.Exists(strNewAutosaveName) && File.GetLastWriteTimeUtc(strNewAutosaveName) > File.GetLastWriteTimeUtc(strFileName))
                            {
                                blnLoadAutosave = true;
                                objCharacter.FileName = strNewAutosaveName;
                            }
                        }

                        if (!blnLoadAutosave && !string.IsNullOrEmpty(strNewName))
                        {
                            string strOldAutosaveName = strNewName;
                            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                            {
                                strOldAutosaveName = strOldAutosaveName.Replace(invalidChar, '_');
                            }

                            if (!string.IsNullOrEmpty(strOldAutosaveName))
                            {
                                strOldAutosaveName = Path.Combine(strAutosavesPath, strOldAutosaveName);
                                if (File.Exists(strOldAutosaveName) && File.GetLastWriteTimeUtc(strOldAutosaveName) > File.GetLastWriteTimeUtc(strFileName))
                                {
                                    blnLoadAutosave = true;
                                    objCharacter.FileName = strOldAutosaveName;
                                }
                            }
                        }
                    }
                    if (blnLoadAutosave && Program.MainForm.ShowMessageBox(
                        string.Format(GlobalOptions.CultureInfo,
                            LanguageManager.GetString("Message_AutosaveFound"),
                            Path.GetFileName(strFileName),
                            File.GetLastWriteTimeUtc(objCharacter.FileName).ToLocalTime(),
                            File.GetLastWriteTimeUtc(strFileName).ToLocalTime()),
                        LanguageManager.GetString("MessageTitle_AutosaveFound"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        blnLoadAutosave = false;
                        objCharacter.FileName = strFileName;
                    }
                }
                if (blnShowProgressBar)
                {
                    using (frmLoading frmProgressBar = new frmLoading { CharacterFile = objCharacter.FileName })
                    {
                        frmProgressBar.Reset(35);
                        frmProgressBar.Show();
                        OpenCharacters.Add(objCharacter);
                        //Timekeeper.Start("load_file");
                        bool blnLoaded = await objCharacter.Load(frmProgressBar, blnShowErrors).ConfigureAwait(false);
                        //Timekeeper.Finish("load_file");
                        if (!blnLoaded)
                        {
                            OpenCharacters.Remove(objCharacter);
                            return null;
                        }
                    }
                }
                else
                {
                    OpenCharacters.Add(objCharacter);
                    //Timekeeper.Start("load_file");
                    bool blnLoaded = await objCharacter.Load(null, blnShowErrors).ConfigureAwait(false);
                    //Timekeeper.Finish("load_file");
                    if (!blnLoaded)
                    {
                        OpenCharacters.Remove(objCharacter);
                        return null;
                    }
                }

                // If a new name is given, set the character's name to match (used in cloning).
                if (!string.IsNullOrEmpty(strNewName))
                    objCharacter.Name = strNewName;
                // Clear the File Name field so that this does not accidentally overwrite the original save file (used in cloning).
                if (blnClearFileName)
                    objCharacter.FileName = string.Empty;
                // Restore original filename if we loaded from an autosave
                if (blnLoadAutosave)
                    objCharacter.FileName = strFileName;
            }
            else if(blnShowErrors)
            {
                Program.MainForm.ShowMessageBox(string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_FileNotFound"), strFileName),
                    LanguageManager.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return objCharacter;
        }

        /// <summary>
        /// Populate the MRU items.
        /// </summary>
        public void PopulateMRUToolstripMenu(object sender, TextEventArgs e)
        {
            SuspendLayout();
            mnuFileMRUSeparator.Visible = GlobalOptions.FavoritedCharacters.Count > 0
                                          || GlobalOptions.MostRecentlyUsedCharacters.Count > 0;

            if(e?.Text != "mru")
            {
                for(int i = 0; i < GlobalOptions.MaxMruSize; ++i)
                {
                    ToolStripMenuItem objItem;
                    switch(i)
                    {
                        case 0:
                            objItem = mnuStickyMRU0;
                            break;
                        case 1:
                            objItem = mnuStickyMRU1;
                            break;
                        case 2:
                            objItem = mnuStickyMRU2;
                            break;
                        case 3:
                            objItem = mnuStickyMRU3;
                            break;
                        case 4:
                            objItem = mnuStickyMRU4;
                            break;
                        case 5:
                            objItem = mnuStickyMRU5;
                            break;
                        case 6:
                            objItem = mnuStickyMRU6;
                            break;
                        case 7:
                            objItem = mnuStickyMRU7;
                            break;
                        case 8:
                            objItem = mnuStickyMRU8;
                            break;
                        case 9:
                            objItem = mnuStickyMRU9;
                            break;
                        default:
                            continue;
                    }

                    if(i < GlobalOptions.FavoritedCharacters.Count)
                    {
                        objItem.Text = GlobalOptions.FavoritedCharacters[i];
                        objItem.Tag = GlobalOptions.FavoritedCharacters[i];
                        objItem.Visible = true;
                    }
                    else
                    {
                        objItem.Visible = false;
                    }
                }
            }

            mnuMRU0.Visible = false;
            mnuMRU1.Visible = false;
            mnuMRU2.Visible = false;
            mnuMRU3.Visible = false;
            mnuMRU4.Visible = false;
            mnuMRU5.Visible = false;
            mnuMRU6.Visible = false;
            mnuMRU7.Visible = false;
            mnuMRU8.Visible = false;
            mnuMRU9.Visible = false;

            string strSpace = LanguageManager.GetString("String_Space");
            int i2 = 0;
            for(int i = 0; i < GlobalOptions.MaxMruSize; ++i)
            {
                if(i2 < GlobalOptions.MostRecentlyUsedCharacters.Count && i < GlobalOptions.MostRecentlyUsedCharacters.Count)
                {
                    string strFile = GlobalOptions.MostRecentlyUsedCharacters[i];
                    if(!GlobalOptions.FavoritedCharacters.Contains(strFile))
                    {
                        ToolStripMenuItem objItem;
                        switch(i2)
                        {
                            case 0:
                                objItem = mnuMRU0;
                                break;
                            case 1:
                                objItem = mnuMRU1;
                                break;
                            case 2:
                                objItem = mnuMRU2;
                                break;
                            case 3:
                                objItem = mnuMRU3;
                                break;
                            case 4:
                                objItem = mnuMRU4;
                                break;
                            case 5:
                                objItem = mnuMRU5;
                                break;
                            case 6:
                                objItem = mnuMRU6;
                                break;
                            case 7:
                                objItem = mnuMRU7;
                                break;
                            case 8:
                                objItem = mnuMRU8;
                                break;
                            case 9:
                                objItem = mnuMRU9;
                                break;
                            default:
                                continue;
                        }

                        if (i2 <= 9 && i2 >= 0)
                        {
                            string strNumAsString = (i2 + 1).ToString(GlobalOptions.CultureInfo);
                            objItem.Text = strNumAsString.Insert(strNumAsString.Length - 1, "&") + strSpace + strFile;
                        }
                        else
                            objItem.Text = (i2 + 1).ToString(GlobalOptions.CultureInfo) + strSpace + strFile;
                        objItem.Tag = strFile;
                        objItem.Visible = true;

                        ++i2;
                    }
                }
            }

            ResumeLayout();
        }

        public void OpenDiceRollerWithPool(Character objCharacter = null, int intDice = 0)
        {
            if (GlobalOptions.SingleDiceRoller)
            {
                if (_frmRoller == null)
                {
                    _frmRoller = new frmDiceRoller(this, objCharacter?.Qualities, intDice);
                    _frmRoller.Show();
                }
                else
                {
                    _frmRoller.Dice = intDice;
                    _frmRoller.Qualities = objCharacter?.Qualities;
                    _frmRoller.Activate();
                }
            }
            else
            {
                frmDiceRoller frmRoller = new frmDiceRoller(this, objCharacter?.Qualities, intDice);
                frmRoller.Show();
            }
        }

        private void mnuClearUnpinnedItems_Click(object sender, EventArgs e)
        {
            GlobalOptions.MostRecentlyUsedCharacters.Clear();
        }

        private void mnuRestart_Click(object sender, EventArgs e)
        {
            Utils.RestartApplication(GlobalOptions.Language, "Message_Options_Restart");
        }
#endregion

#region Application Properties
        /// <summary>
        /// The frmDiceRoller window being used by the application.
        /// </summary>
        public frmDiceRoller RollerWindow
        {
            get => _frmRoller;
            set => _frmRoller = value;
        }

        public ThreadSafeObservableCollection<Character> OpenCharacters => _lstCharacters;

        public ObservableCollection<CharacterShared> OpenCharacterForms => _lstOpenCharacterForms;

        public Version CurrentVersion => _objCurrentVersion;

#endregion
    }
}
