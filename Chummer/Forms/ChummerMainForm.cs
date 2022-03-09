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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ApplicationInsights.DataContracts;
using NLog;
using Application = System.Windows.Forms.Application;
using DataFormats = System.Windows.Forms.DataFormats;
using DragDropEffects = System.Windows.Forms.DragDropEffects;
using DragEventArgs = System.Windows.Forms.DragEventArgs;
using Path = System.IO.Path;
using Size = System.Drawing.Size;

namespace Chummer
{
    public sealed partial class ChummerMainForm : Form
    {
        private bool _blnAbleToReceiveData;
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private DiceRoller _frmRoller;
        private ChummerUpdater _frmUpdate;
        private readonly ThreadSafeObservableCollection<CharacterShared> _lstOpenCharacterForms = new ThreadSafeObservableCollection<CharacterShared>();
        private readonly string _strCurrentVersion;
        private Chummy _mascotChummy;

        public string MainTitle
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                string strTitle = Application.ProductName + strSpace + '-' + strSpace + LanguageManager.GetString("String_Version") + strSpace + _strCurrentVersion;
#if DEBUG
                strTitle += " DEBUG BUILD";
#endif
                return strTitle;
            }
        }

        #region Control Events

        public ChummerMainForm(bool isUnitTest = false)
        {
            Utils.IsUnitTest = isUnitTest;

            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _strCurrentVersion = Utils.CurrentChummerVersion.ToString(3);

            //lets write that in separate lines to see where the exception is thrown
            if (!GlobalSettings.HideMasterIndex || isUnitTest)
            {
                MasterIndex = new MasterIndex
                {
                    MdiParent = this
                };
            }
            if (!GlobalSettings.HideCharacterRoster || isUnitTest)
            {
                CharacterRoster = new CharacterRoster
                {
                    MdiParent = this
                };
            }
        }

        //Moved most of the initialization out of the constructor to allow the Mainform to be generated fast
        //in case of a commandline argument not asking for the mainform to be shown.
        private async void ChummerMainForm_Load(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                using (CustomActivity opFrmChummerMain = Timekeeper.StartSyncron(
                           "frmChummerMain_Load", null, CustomActivity.OperationType.DependencyOperation,
                           _strCurrentVersion))
                {
                    try
                    {
                        opFrmChummerMain.MyDependencyTelemetry.Type = "loadfrmChummerMain";
                        opFrmChummerMain.MyDependencyTelemetry.Target = _strCurrentVersion;

                        if (MyStartupPvt != null)
                        {
                            MyStartupPvt.Duration = DateTimeOffset.UtcNow - MyStartupPvt.Timestamp;
                            opFrmChummerMain.MyTelemetryClient.TrackPageView(MyStartupPvt);
                        }

                        NativeMethods.ChangeFilterStruct changeFilter = new NativeMethods.ChangeFilterStruct();
                        changeFilter.size = (uint) Marshal.SizeOf(changeFilter);
                        changeFilter.info = 0;
                        if (NativeMethods.ChangeWindowMessageFilterEx(Handle, NativeMethods.WM_COPYDATA,
                                                                      NativeMethods.ChangeWindowMessageFilterExAction
                                                                          .Allow, ref changeFilter))
                            _blnAbleToReceiveData = true;
                        else
                        {
                            int intErrorCode = Marshal.GetLastWin32Error();
                            Utils.BreakIfDebug();
                            Log.Error(
                                "The error " + intErrorCode + " occurred while attempting to unblock WM_COPYDATA.");
                        }

                        Text = MainTitle;

                        //this.toolsMenu.DropDownItems.Add("GM Dashboard").Click += this.dashboardToolStripMenuItem_Click;

                        // If Automatic Updates are enabled, check for updates immediately.

#if !DEBUG
                    Application.Idle += IdleUpdateCheck;
                    CheckForUpdate();
#endif

                        GlobalSettings.MruChanged += PopulateMruToolstripMenu;

                        // Populate the MRU list.
                        await this.DoThreadSafeFunc(() => DoPopulateMruToolstripMenu());

                        Program.MainForm = this;

                        CursorWait objCursorWait2 = await CursorWait.NewAsync(this);
                        try
                        {
                            ThreadSafeList<Character> lstCharactersToLoad = new ThreadSafeList<Character>(1);
                            try
                            {
                                Task objCharacterLoadingTask = null;
                                using (Program.MainProgressBar
                                           = await Program.CreateAndShowProgressBarAsync(
                                               Text,
                                               GlobalSettings.AllowEasterEggs ? 4 : 3))
                                {
                                    Program.MainProgressBar.PerformStep(
                                        await LanguageManager.GetStringAsync("String_UI"));

                                    Program.OpenCharacters.CollectionChanged += OpenCharactersOnCollectionChanged;

                                    // Retrieve the arguments passed to the application. If more than 1 is passed, we're being given the name of a file to open.
                                    bool blnShowTest = false;
                                    if (!Utils.IsUnitTest)
                                    {
                                        string[] strArgs = Environment.GetCommandLineArgs();
                                        ProcessCommandLineArguments(strArgs, out blnShowTest,
                                                                    out HashSet<string> setFilesToLoad,
                                                                    opFrmChummerMain);
                                        try
                                        {
                                            if (Directory.Exists(Utils.GetAutosavesFolderPath))
                                            {
                                                // Always process newest autosave if all MRUs are empty
                                                bool blnAnyAutosaveInMru
                                                    = GlobalSettings.MostRecentlyUsedCharacters.Count == 0 &&
                                                      GlobalSettings.FavoriteCharacters.Count == 0;
                                                FileInfo objMostRecentAutosave = null;
                                                List<string> lstOldAutosaves = new List<string>(10);
                                                DateTime objOldAutosaveTimeThreshold =
                                                    DateTime.UtcNow.Subtract(TimeSpan.FromDays(90));
                                                foreach (string strAutosave in Directory.EnumerateFiles(
                                                             Utils.GetAutosavesFolderPath,
                                                             "*.chum5", SearchOption.AllDirectories))
                                                {
                                                    FileInfo objAutosave;
                                                    try
                                                    {
                                                        objAutosave = new FileInfo(strAutosave);
                                                    }
                                                    catch (System.Security.SecurityException)
                                                    {
                                                        continue;
                                                    }
                                                    catch (UnauthorizedAccessException)
                                                    {
                                                        continue;
                                                    }

                                                    if (objMostRecentAutosave == null || objAutosave.LastWriteTimeUtc >
                                                        objMostRecentAutosave.LastWriteTimeUtc)
                                                        objMostRecentAutosave = objAutosave;
                                                    if (GlobalSettings.MostRecentlyUsedCharacters.Any(x =>
                                                            Path.GetFileName(x) == objAutosave.Name) ||
                                                        GlobalSettings.FavoriteCharacters.Any(x =>
                                                            Path.GetFileName(x) == objAutosave.Name))
                                                        blnAnyAutosaveInMru = true;
                                                    else if (objAutosave != objMostRecentAutosave &&
                                                             objAutosave.LastWriteTimeUtc < objOldAutosaveTimeThreshold
                                                             &&
                                                             !setFilesToLoad.Contains(strAutosave))
                                                        lstOldAutosaves.Add(strAutosave);
                                                }

                                                if (objMostRecentAutosave != null)
                                                {
                                                    // Might have had a crash for an unsaved character, so prompt if we want to load them
                                                    if (blnAnyAutosaveInMru &&
                                                        !setFilesToLoad.Contains(objMostRecentAutosave.FullName) &&
                                                        GlobalSettings.MostRecentlyUsedCharacters.All(x =>
                                                            Path.GetFileName(x) != objMostRecentAutosave.Name) &&
                                                        GlobalSettings.FavoriteCharacters.All(x =>
                                                            Path.GetFileName(x) != objMostRecentAutosave.Name))
                                                    {
                                                        if (Program.ShowMessageBox(
                                                                string.Format(GlobalSettings.CultureInfo,
                                                                              await LanguageManager.GetStringAsync(
                                                                                  "Message_PossibleCrashAutosaveFound"),
                                                                              objMostRecentAutosave.Name,
                                                                              objMostRecentAutosave.LastWriteTimeUtc
                                                                                  .ToLocalTime()),
                                                                await LanguageManager.GetStringAsync(
                                                                    "MessageTitle_AutosaveFound"),
                                                                MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                                                            == DialogResult.Yes)
                                                            setFilesToLoad.Add(objMostRecentAutosave.FullName);
                                                        else
                                                            lstOldAutosaves.Add(objMostRecentAutosave.FullName);
                                                    }
                                                    else if (objMostRecentAutosave.LastWriteTimeUtc
                                                             < objOldAutosaveTimeThreshold)
                                                        lstOldAutosaves.Add(objMostRecentAutosave.FullName);
                                                }

                                                // Delete all old autosaves
                                                List<Task> lstTasks = new List<Task>(lstOldAutosaves.Count);
                                                foreach (string strOldAutosave in lstOldAutosaves)
                                                {
                                                    lstTasks.Add(Utils.SafeDeleteFileAsync(strOldAutosave));
                                                }

                                                await Task.WhenAll(lstTasks);
                                            }

                                            if (setFilesToLoad.Count > 0)
                                            {
                                                List<Task> tskLoadingTasks = new List<Task>(setFilesToLoad.Count);
                                                foreach (string strFile in setFilesToLoad)
                                                {
                                                    tskLoadingTasks.Add(Task.Run(async () =>
                                                    {
                                                        Character objCharacter
                                                            = await Program.LoadCharacterAsync(strFile);
                                                        // ReSharper disable once AccessToDisposedClosure
                                                        await lstCharactersToLoad
                                                            .AddAsync(objCharacter);
                                                    }));
                                                }
                                                objCharacterLoadingTask = Task.WhenAll(tskLoadingTasks);
                                            }
                                        }
                                        finally
                                        {
                                            Utils.StringHashSetPool.Return(setFilesToLoad);
                                        }
                                    }

                                    Program.MainProgressBar.PerformStep(
                                        await LanguageManager.GetStringAsync("Title_MasterIndex"));

                                    if (MasterIndex != null)
                                    {
                                        if (CharacterRoster == null)
                                            MasterIndex.WindowState = FormWindowState.Maximized;
                                        MasterIndex.Show();
                                    }

                                    Program.MainProgressBar.PerformStep(
                                        await LanguageManager.GetStringAsync("String_CharacterRoster"));

                                    if (CharacterRoster != null)
                                    {
                                        if (MasterIndex == null)
                                            CharacterRoster.WindowState = FormWindowState.Maximized;
                                        CharacterRoster.Show();
                                    }

                                    if (GlobalSettings.AllowEasterEggs)
                                    {
                                        Program.MainProgressBar.PerformStep(
                                            await LanguageManager.GetStringAsync("String_Chummy"));
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
                                        TestDataEntries frmTestData = new TestDataEntries();
                                        frmTestData.Show();
                                    }
                                }

                                Program.PluginLoader.CallPlugins(toolsMenu, opFrmChummerMain);

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

                                if (objCharacterLoadingTask?.IsCompleted == false)
                                    await objCharacterLoadingTask;
                                if (lstCharactersToLoad.Count > 0)
                                    await OpenCharacterList(lstCharactersToLoad);
                            }
                            finally
                            {
                                await lstCharactersToLoad.DisposeAsync();
                            }
                        }
                        finally
                        {
                            await objCursorWait2.DisposeAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (opFrmChummerMain != null)
                        {
                            opFrmChummerMain.SetSuccess(false);
                            opFrmChummerMain.MyTelemetryClient.TrackException(ex);
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

                    if (Properties.Settings.Default.Size.Width == 0 || Properties.Settings.Default.Size.Height == 0
                                                                    || !IsVisibleOnAnyScreen())
                    {
                        int intDefaultWidth = 1280;
                        int intDefaultHeight = 720;
                        using (Graphics g = CreateGraphics())
                        {
                            intDefaultWidth = (int) (intDefaultWidth * g.DpiX / 96.0f);
                            intDefaultHeight = (int) (intDefaultHeight * g.DpiY / 96.0f);
                        }

                        Size = new Size(intDefaultWidth, intDefaultHeight);
                        StartPosition = FormStartPosition.CenterScreen;
                    }
                    else
                    {
                        if (!Utils.IsUnitTest)
                        {
                            WindowState = Properties.Settings.Default.WindowState;
                            if (WindowState == FormWindowState.Minimized)
                                WindowState = FormWindowState.Normal;
                        }

                        Location = Properties.Settings.Default.Location;
                        Size = Properties.Settings.Default.Size;
                    }

                    if (!Utils.IsUnitTest && GlobalSettings.StartupFullscreen)
                        WindowState = FormWindowState.Maximized;
                }

                if (Utils.IsUnitTest)
                {
                    if (CharacterRoster != null)
                    {
                        if (MasterIndex != null)
                        {
                            while (!CharacterRoster.IsFinishedLoading || !MasterIndex.IsFinishedLoading)
                                await Utils.SafeSleepAsync();
                        }
                        else
                        {
                            while (!CharacterRoster.IsFinishedLoading)
                                await Utils.SafeSleepAsync();
                        }
                    }
                    else if (MasterIndex != null)
                    {
                        while (!MasterIndex.IsFinishedLoading)
                            await Utils.SafeSleepAsync();
                    }
                }

                IsFinishedLoading = true;
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        [CLSCompliant(false)]
        public PageViewTelemetry MyStartupPvt { get; set; }

        private async void OpenCharactersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (Character objCharacter in notifyCollectionChangedEventArgs.NewItems)
                            objCharacter.PropertyChanged += UpdateCharacterTabTitle;
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Character objCharacter in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objCharacter.PropertyChanged -= UpdateCharacterTabTitle;
                            await objCharacter.DisposeAsync();
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (Character objCharacter in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objCharacter.PropertyChanged -= UpdateCharacterTabTitle;
                            if (!notifyCollectionChangedEventArgs.NewItems.Contains(objCharacter))
                                await objCharacter.DisposeAsync();
                        }
                        foreach (Character objCharacter in notifyCollectionChangedEventArgs.NewItems)
                            objCharacter.PropertyChanged += UpdateCharacterTabTitle;
                        break;
                    }
            }
        }

        public CharacterRoster CharacterRoster { get; }

        public MasterIndex MasterIndex { get; }

#if !DEBUG
        private Uri UpdateLocation { get; } = new Uri(GlobalSettings.PreferNightlyBuilds
            ? "https://api.github.com/repos/chummer5a/chummer5a/releases"
            : "https://api.github.com/repos/chummer5a/chummer5a/releases/latest");

        private readonly Stopwatch _idleUpdateCheckStopWatch = Stopwatch.StartNew();

        private Task _tskVersionUpdate;

        private System.Threading.CancellationTokenSource _objVersionUpdaterCancellationTokenSource;

        private async ValueTask DoCacheGitVersion()
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            System.Net.HttpWebRequest request;
            try
            {
                System.Net.WebRequest objTemp = System.Net.WebRequest.Create(UpdateLocation);
                request = objTemp as System.Net.HttpWebRequest;
            }
            catch (System.Security.SecurityException ex)
            {
                Utils.CachedGitVersion = null;
                Log.Error(ex);
                return;
            }
            if (request == null)
            {
                Utils.CachedGitVersion = null;
                return;
            }

            if (_objVersionUpdaterCancellationTokenSource.IsCancellationRequested)
                return;

            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            request.Accept = "application/json";

            try
            {
                // Get the response.
                using (System.Net.HttpWebResponse response = await request.GetResponseAsync() as System.Net.HttpWebResponse)
                {
                    if (response == null)
                    {
                        Utils.CachedGitVersion = null;
                        return;
                    }

                    if (_objVersionUpdaterCancellationTokenSource.IsCancellationRequested)
                        return;

                    // Get the stream containing content returned by the server.
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        if (dataStream == null)
                        {
                            Utils.CachedGitVersion = null;
                            return;
                        }

                        if (_objVersionUpdaterCancellationTokenSource.IsCancellationRequested)
                            return;

                        // Open the stream using a StreamReader for easy access.
                        using (StreamReader reader = new StreamReader(dataStream, System.Text.Encoding.UTF8, true))
                        {
                            if (_objVersionUpdaterCancellationTokenSource.IsCancellationRequested)
                                return;

                            // Read the content.
                            string responseFromServer = await reader.ReadToEndAsync();

                            string line = responseFromServer.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(x => x.Contains("tag_name"));

                            if (_objVersionUpdaterCancellationTokenSource.IsCancellationRequested)
                                return;

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

                                if (_objVersionUpdaterCancellationTokenSource.IsCancellationRequested)
                                    return;

                                if (!Version.TryParse(strVersion.TrimStartOnce("Nightly-v"), out verLatestVersion))
                                    verLatestVersion = null;

                                if (_objVersionUpdaterCancellationTokenSource.IsCancellationRequested)
                                    return;
                            }

                            Utils.CachedGitVersion = verLatestVersion;
                        }
                    }
                }
            }
            catch (System.Net.WebException ex)
            {
                Utils.CachedGitVersion = null;
                Log.Error(ex);
            }
        }

        private void CheckForUpdate()
        {
            _objVersionUpdaterCancellationTokenSource = new System.Threading.CancellationTokenSource();
            _tskVersionUpdate = Task.Run(async () =>
            {
                await DoCacheGitVersion();
                if (_objVersionUpdaterCancellationTokenSource.IsCancellationRequested ||
                    Utils.GitUpdateAvailable <= 0)
                    return;
                string strSpace = await LanguageManager.GetStringAsync("String_Space");
                string strNewText = Application.ProductName + strSpace + '-' + strSpace +
                                    await LanguageManager.GetStringAsync("String_Version")
                                    + strSpace + _strCurrentVersion + strSpace + '-' + strSpace
                                    + string.Format(GlobalSettings.CultureInfo,
                                        await LanguageManager.GetStringAsync("String_Update_Available"), Utils.CachedGitVersion);
                await this.DoThreadSafeAsync(() =>
                {
                    if (GlobalSettings.AutomaticUpdate && _frmUpdate == null)
                    {
                        _frmUpdate = new ChummerUpdater();
                        _frmUpdate.FormClosed += ResetChummerUpdater;
                        _frmUpdate.SilentMode = true;
                    }
                    Text = strNewText;
                });
            }, _objVersionUpdaterCancellationTokenSource.Token);
        }

        private void IdleUpdateCheck(object sender, EventArgs e)
        {
            // Automatically check for updates every hour
            if (_idleUpdateCheckStopWatch.Elapsed < TimeSpan.FromHours(1) || _tskVersionUpdate?.IsCompleted == false)
                return;
            _idleUpdateCheckStopWatch.Restart();
            CheckForUpdate();
        }
#endif

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
            foreach (Form childForm in MdiChildren)
            {
                if (childForm != CharacterRoster && childForm != MasterIndex)
                    childForm.Close();
            }
        }

        private async void mnuGlobalSettings_Click(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                using (EditGlobalSettings frmOptions = new EditGlobalSettings())
                    await frmOptions.ShowDialogSafeAsync(this);
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async void mnuCharacterSettings_Click(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                using (EditCharacterSettings frmCharacterOptions = new EditCharacterSettings((tabForms.SelectedTab?.Tag as CharacterShared)?.CharacterObject?.Settings))
                    await frmCharacterOptions.ShowDialogSafeAsync(this);
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private void mnuToolsUpdate_Click(object sender, EventArgs e)
        {
            // Only a single instance of the updater can be open, so either find the current instance and focus on it, or create a new one.
            if (_frmUpdate == null)
            {
                _frmUpdate = new ChummerUpdater();
                _frmUpdate.FormClosed += ResetChummerUpdater;
                _frmUpdate.Show();
            }
            // Silent updater is running, so make it visible
            else if (_frmUpdate.SilentMode)
            {
                _frmUpdate.SilentMode = false;
                _frmUpdate.Show();
            }
            else
            {
                _frmUpdate.Focus();
            }
        }

        private void ResetChummerUpdater(object sender, EventArgs e)
        {
            _frmUpdate = null;
        }

        private async void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (About showAbout = new About())
                await showAbout.ShowDialogSafeAsync(this);
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

        public PrintMultipleCharacters PrintMultipleCharactersForm { get; private set; }

        private void mnuFilePrintMultiple_Click(object sender, EventArgs e)
        {
            if (PrintMultipleCharactersForm.IsNullOrDisposed())
                PrintMultipleCharactersForm = new PrintMultipleCharacters();
            else
                PrintMultipleCharactersForm.Activate();
            PrintMultipleCharactersForm.Show(this);
        }

        private async void mnuHelpRevisionHistory_Click(object sender, EventArgs e)
        {
            using (VersionHistory frmShowHistory = new VersionHistory())
                await frmShowHistory.ShowDialogSafeAsync(this);
        }

        private async void mnuNewCritter_Click(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                Character objCharacter = new Character();
                try
                {
                    using (SelectBuildMethod frmPickSetting = new SelectBuildMethod(objCharacter))
                    {
                        await frmPickSetting.ShowDialogSafeAsync(this);
                        if (frmPickSetting.DialogResult == DialogResult.Cancel)
                            return;
                    }

                    // Override the defaults for the setting.
                    objCharacter.IgnoreRules = true;
                    objCharacter.IsCritter = true;
                    objCharacter.Created = true;

                    // Show the Metatype selection window.
                    using (SelectMetatypeKarma frmSelectMetatype
                           = new SelectMetatypeKarma(objCharacter, "critters.xml"))
                    {
                        await frmSelectMetatype.ShowDialogSafeAsync(this);

                        if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                            return;
                    }

                    Program.OpenCharacters.Add(objCharacter);
                    await OpenCharacter(objCharacter, false);
                }
                finally
                {
                    await objCharacter.DisposeAsync(); // Fine here because Dispose()/DisposeAsync() code is skipped if the character is open in a form
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async void mnuMRU_Click(object sender, EventArgs e)
        {
            string strFileName = ((ToolStripMenuItem)sender).Tag as string;
            if (string.IsNullOrEmpty(strFileName))
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                await OpenCharacter(await Program.LoadCharacterAsync(strFileName));
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private void mnuMRU_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            string strFileName = ((ToolStripMenuItem)sender).Tag as string;
            if (!string.IsNullOrEmpty(strFileName))
                GlobalSettings.FavoriteCharacters.AddWithSort(strFileName);
        }

        private void mnuStickyMRU_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            string strFileName = ((ToolStripMenuItem)sender).Tag as string;
            if (!string.IsNullOrEmpty(strFileName))
            {
                GlobalSettings.FavoriteCharacters.Remove(strFileName);
                GlobalSettings.MostRecentlyUsedCharacters.Insert(0, strFileName);
            }
        }

        private async void ChummerMainForm_MdiChildActivate(object sender, EventArgs e)
        {
            // If there are no child forms, hide the tab control.
            if (ActiveMdiChild != null)
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

                    if (ActiveMdiChild is CharacterShared frmCharacterShared)
                    {
                        tp.Text = frmCharacterShared.CharacterObject.CharacterName;
                        if (GlobalSettings.AllowEasterEggs && _mascotChummy != null)
                        {
                            _mascotChummy.CharacterObject = frmCharacterShared.CharacterObject;
                        }
                    }
                    else
                    {
                        string strTagText = await LanguageManager.GetStringAsync(ActiveMdiChild.Tag?.ToString(), GlobalSettings.Language, false);
                        if (!string.IsNullOrEmpty(strTagText))
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
            if (sender is Form objForm)
            {
                objForm.FormClosed -= ActiveMdiChild_FormClosed;
                if (objForm.Tag is TabPage objTabPage)
                {
                    if (tabForms.TabCount > 1)
                    {
                        int intSelectTab = tabForms.TabPages.IndexOf(objTabPage);
                        if (intSelectTab > 0)
                        {
                            if (intSelectTab + 1 >= tabForms.TabCount)
                                --intSelectTab;
                            else
                                ++intSelectTab;
                            tabForms.SelectedIndex = intSelectTab;
                        }
                    }
                    objTabPage.Dispose();
                }
                if (!objForm.IsDisposed)
                    objForm.Dispose();
            }

            // Don't show the tab control if there is only one window open.
            if (tabForms.TabCount <= 1)
                tabForms.Visible = false;
        }

        private void tabForms_SelectedIndexChanged(object sender, EventArgs e)
        {
            (tabForms.SelectedTab?.Tag as Form)?.Select();
        }

        /// <summary>
        /// Switch to an open character form if one is available.
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <returns></returns>
        public bool SwitchToOpenCharacter(Character objCharacter)
        {
            using (CursorWait.New(this))
            {
                if (objCharacter == null)
                    return false;
                CharacterShared objCharacterForm = OpenCharacterForms.FirstOrDefault(x => x.CharacterObject == objCharacter);
                if (objCharacterForm == null)
                    return false;
                foreach (TabPage objTabPage in tabForms.TabPages)
                {
                    if (objTabPage.Tag != objCharacterForm)
                        continue;
                    tabForms.SelectTab(objTabPage);
                    if (_mascotChummy != null)
                        _mascotChummy.CharacterObject = objCharacter;
                    return true;
                }
                objCharacterForm.BringToFront();
                return true;
            }
        }

        public void UpdateCharacterTabTitle(object sender, PropertyChangedEventArgs e)
        {
            // Change the TabPage's text to match the character's name (or "Unnamed Character" if they are currently unnamed).
            if (tabForms.TabCount > 0 && e?.PropertyName == nameof(Character.CharacterName) && sender is Character objCharacter)
            {
                foreach (TabPage objTabPage in tabForms.TabPages)
                {
                    if (objTabPage.Tag is CharacterShared objCharacterForm && objCharacterForm.CharacterObject == objCharacter)
                    {
                        objTabPage.QueueThreadSafe(() => objTabPage.Text = objCharacter.CharacterName.Trim());
                        return;
                    }
                }
            }
        }

        private void mnuToolsDiceRoller_Click(object sender, EventArgs e)
        {
            if (GlobalSettings.SingleDiceRoller)
            {
                // Only a single instance of the Dice Roller window is allowed, so either find the existing one and focus on it, or create a new one.
                if (_frmRoller == null)
                {
                    _frmRoller = new DiceRoller(this);
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
                DiceRoller frmRoller = new DiceRoller(this);
                frmRoller.Show();
            }
        }

        private void menuStrip_ItemAdded(object sender, ToolStripItemEventArgs e)
        {
            // Translate the items in the menu by finding their Tags in the translation file.
            foreach (ToolStripItem tssItem in menuStrip.Items.OfType<ToolStripItem>())
            {
                tssItem.UpdateLightDarkMode();
                tssItem.TranslateToolStripItemsRecursively();
            }
        }

        private void toolStrip_ItemAdded(object sender, ToolStripItemEventArgs e)
        {
            // ToolStrip Items.
            foreach (ToolStrip objToolStrip in Controls.OfType<ToolStrip>())
            {
                foreach (ToolStripItem tssItem in objToolStrip.Items.OfType<ToolStripItem>())
                {
                    tssItem.UpdateLightDarkMode();
                    tssItem.TranslateToolStripItemsRecursively();
                }
            }
        }

        private void toolStrip_ItemRemoved(object sender, ToolStripItemEventArgs e)
        {
            // ToolStrip Items.
            foreach (ToolStrip objToolStrip in Controls.OfType<ToolStrip>())
            {
                foreach (ToolStripItem tssItem in objToolStrip.Items.OfType<ToolStripItem>())
                {
                    tssItem.UpdateLightDarkMode();
                    tssItem.TranslateToolStripItemsRecursively();
                }
            }
        }

        private bool IsVisibleOnAnyScreen()
        {
            Rectangle objMyRectangle = ClientRectangle;
            return Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(objMyRectangle));
        }

        private async void ChummerMainForm_DragDrop(object sender, DragEventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                // Open each file that has been dropped into the window.
                string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (s.Length == 0)
                    return;
                Character[] lstCharacters = new Character[s.Length];
                Task<Character>[] tskCharacterLoads = new Task<Character>[s.Length];
                // Array instead of concurrent bag because we want to preserve order
                for (int i = 0; i < s.Length; ++i)
                {
                    string strFile = s[i];
                    tskCharacterLoads[i] = Task.Run(() => Program.LoadCharacterAsync(strFile));
                }
                await Task.WhenAll(tskCharacterLoads);
                for (int i = 0; i < lstCharacters.Length; ++i)
                    lstCharacters[i] = await tskCharacterLoads[i];
                await OpenCharacterList(lstCharacters);
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private void ChummerMainForm_DragEnter(object sender, DragEventArgs e)
        {
            // Only use a drop effect if a file is being dragged into the window.
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void mnuToolsTranslator_Click(object sender, EventArgs e)
        {
            string strTranslator = Path.Combine(Utils.GetStartupPath, "Translator.exe");
            if (File.Exists(strTranslator))
                Process.Start(strTranslator);
        }

        private void ChummerMainForm_Closing(object sender, FormClosingEventArgs e)
        {
            Program.OpenCharacters.CollectionChanged -= OpenCharactersOnCollectionChanged;
            foreach (Character objCharacter in Program.OpenCharacters)
                objCharacter.PropertyChanged -= UpdateCharacterTabTitle;
#if !DEBUG
            _objVersionUpdaterCancellationTokenSource?.Cancel(false);
#endif
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

        private async void mnuHeroLabImporter_Click(object sender, EventArgs e)
        {
            if (Program.ShowMessageBox(await LanguageManager.GetStringAsync("Message_HeroLabImporterWarning"),
                    await LanguageManager.GetStringAsync("Message_HeroLabImporterWarning_Title"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            HeroLabImporter frmImporter = new HeroLabImporter();
            frmImporter.Show();
        }

        private void tabForms_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < tabForms.TabCount; ++i)
                {
                    if (!tabForms.GetTabRect(i).Contains(e.Location))
                        continue;
                    if (tabForms.SelectedTab.Tag is CharacterShared && tabForms.SelectedIndex == i)
                    {
                        mnuProcessFile.Show(this, e.Location);
                        break;
                    }
                }
            }
        }

        private async void tsSave_Click(object sender, EventArgs e)
        {
            if (tabForms.SelectedTab.Tag is CharacterShared objShared)
            {
                await objShared.SaveCharacter();
            }
        }

        private async void tsSaveAs_Click(object sender, EventArgs e)
        {
            if (tabForms.SelectedTab.Tag is CharacterShared objShared)
            {
                await objShared.SaveCharacterAs();
            }
        }

        private void tsClose_Click(object sender, EventArgs e)
        {
            if (tabForms.SelectedTab.Tag is CharacterShared objShared)
            {
                objShared.Close();
            }
        }

        private async void tsPrint_Click(object sender, EventArgs e)
        {
            if (tabForms.SelectedTab.Tag is CharacterShared objShared)
            {
                await objShared.DoPrint();
            }
        }

        private void ChummerMainForm_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            tabForms.ItemSize = new Size(
                tabForms.ItemSize.Width * e.DeviceDpiNew / Math.Max(e.DeviceDpiOld, 1),
                tabForms.ItemSize.Height * e.DeviceDpiNew / Math.Max(e.DeviceDpiOld, 1));
        }

        #endregion Control Events

        #region Methods
        /// <summary>
        /// Create a new character and show the Create Form.
        /// </summary>
        private async void ShowNewForm(object sender, EventArgs e)
        {
            Character objCharacter = new Character();
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(this);
                try
                {
                    // Show the BP selection window.
                    using (SelectBuildMethod frmBP = new SelectBuildMethod(objCharacter))
                    {
                        await frmBP.ShowDialogSafeAsync(this);
                        if (frmBP.DialogResult != DialogResult.OK)
                            return;
                    }

                    // Show the Metatype selection window.
                    if (objCharacter.EffectiveBuildMethodUsesPriorityTables)
                    {
                        using (SelectMetatypePriority frmSelectMetatype = new SelectMetatypePriority(objCharacter))
                        {
                            await frmSelectMetatype.ShowDialogSafeAsync(this);

                            if (frmSelectMetatype.DialogResult != DialogResult.OK)
                                return;
                        }
                    }
                    else
                    {
                        using (SelectMetatypeKarma frmSelectMetatype = new SelectMetatypeKarma(objCharacter))
                        {
                            await frmSelectMetatype.ShowDialogSafeAsync(this);

                            if (frmSelectMetatype.DialogResult != DialogResult.OK)
                                return;
                        }
                    }

                    Program.OpenCharacters.Add(objCharacter);
                }
                finally
                {
                    await objCursorWait.DisposeAsync();
                }

                objCursorWait = await CursorWait.NewAsync(this);
                try
                {
                    CharacterCreate frmNewCharacter = new CharacterCreate(objCharacter)
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
                finally
                {
                    await objCursorWait.DisposeAsync();
                }
            }
            finally
            {
                await objCharacter.DisposeAsync(); // Fine here because Dispose()/DisposeAsync() code is skipped if the character is open in a form
            }
        }

        /// <summary>
        /// Show the Open File dialogue, then load the selected character.
        /// </summary>
        private async void OpenFile(object sender, EventArgs e)
        {
            if (Utils.IsUnitTest)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                List<string> lstFilesToOpen;
                using (OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = await LanguageManager.GetStringAsync("DialogFilter_Chum5") + '|' +
                             await LanguageManager.GetStringAsync("DialogFilter_All"),
                    Multiselect = true
                })
                {
                    if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                        return;
                    //Timekeeper.Start("load_sum");
                    lstFilesToOpen = new List<string>(openFileDialog.FileNames.Length);
                    foreach (string strFile in openFileDialog.FileNames)
                    {
                        Character objLoopCharacter = Program.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
                        if (objLoopCharacter != null)
                        {
                            if (!SwitchToOpenCharacter(objLoopCharacter))
                                await OpenCharacter(objLoopCharacter);
                        }
                        else
                            lstFilesToOpen.Add(strFile);
                    }
                }

                if (lstFilesToOpen.Count == 0)
                    return;
                // Array instead of concurrent bag because we want to preserve order
                Character[] lstCharacters = new Character[lstFilesToOpen.Count];
                using (Program.MainProgressBar = await Program.CreateAndShowProgressBarAsync(
                           string.Join(',' + await LanguageManager.GetStringAsync("String_Space"), lstFilesToOpen.Select(Path.GetFileName)),
                           lstFilesToOpen.Count * Character.NumLoadingSections))
                {
                    Task<Character>[] tskCharacterLoads = new Task<Character>[lstFilesToOpen.Count];
                    for (int i = 0; i < lstFilesToOpen.Count; ++i)
                    {
                        string strFile = lstFilesToOpen[i];
                        tskCharacterLoads[i] = Task.Run(() => Program.LoadCharacterAsync(strFile));
                    }
                    await Task.WhenAll(tskCharacterLoads);
                    for (int i = 0; i < lstCharacters.Length; ++i)
                        lstCharacters[i] = await tskCharacterLoads[i];
                }
                await OpenCharacterList(lstCharacters);
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }

            //Timekeeper.Finish("load_sum");
            //Timekeeper.Log();
        }

        /// <summary>
        /// Opens the correct window for a single character (not thread-safe).
        /// </summary>
        public Task OpenCharacter(Character objCharacter, bool blnIncludeInMru = true)
        {
            return OpenCharacterList(objCharacter.Yield(), blnIncludeInMru);
        }

        /// <summary>
        /// Open the correct windows for a list of characters (not thread-safe).
        /// </summary>
        /// <param name="lstCharacters">Characters for which windows should be opened.</param>
        /// <param name="blnIncludeInMru">Added the opened characters to the Most Recently Used list.</param>
        public async Task OpenCharacterList(IEnumerable<Character> lstCharacters, bool blnIncludeInMru = true)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                if (lstCharacters == null)
                    return;
                List<Character> lstNewCharacters = lstCharacters.ToList();
                if (lstNewCharacters.Count == 0)
                    return;
                FormWindowState wsPreference = MdiChildren.Length == 0
                                               || MdiChildren.Any(x => x.WindowState == FormWindowState.Maximized)
                    ? FormWindowState.Maximized
                    : FormWindowState.Normal;
                List<CharacterShared> lstNewFormsToProcess = new List<CharacterShared>(lstNewCharacters.Count);
                string strUI = await LanguageManager.GetStringAsync("String_UI");
                string strSpace = await LanguageManager.GetStringAsync("String_Space");
                using (Program.MainProgressBar
                           = await Program.CreateAndShowProgressBarAsync(strUI, lstNewCharacters.Count))
                {
                    foreach (Character objCharacter in lstNewCharacters)
                    {
                        Program.MainProgressBar.PerformStep(objCharacter == null
                                                                ? strUI
                                                                : strUI + strSpace + '(' + objCharacter.CharacterName
                                                                  + ')');
                        if (objCharacter == null || OpenCharacterForms.Any(x => x.CharacterObject == objCharacter))
                            continue;
                        if (Program.MyProcess.HandleCount >= (objCharacter.Created ? 8000 : 7500)
                            && Program.ShowMessageBox(
                                string.Format(await LanguageManager.GetStringAsync("Message_TooManyHandlesWarning"),
                                              objCharacter.CharacterName),
                                await LanguageManager.GetStringAsync("MessageTitle_TooManyHandlesWarning"),
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        {
                            if (Program.OpenCharacters.All(
                                    x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter)))
                                Program.OpenCharacters.Remove(objCharacter);
                            continue;
                        }

                        //Timekeeper.Start("load_event_time");
                        // Show the character forms.
                        await this.DoThreadSafeAsync(() =>
                        {
                            CharacterShared frmNewCharacter = objCharacter.Created
                                ? (CharacterShared) new CharacterCareer(objCharacter)
                                : new CharacterCreate(objCharacter);
                            frmNewCharacter.MdiParent = this;
                            frmNewCharacter.Show();
                            lstNewFormsToProcess.Add(frmNewCharacter);
                        });
                        if (blnIncludeInMru && !string.IsNullOrEmpty(objCharacter.FileName)
                                            && File.Exists(objCharacter.FileName))
                            GlobalSettings.MostRecentlyUsedCharacters.Insert(0, objCharacter.FileName);

                        UpdateCharacterTabTitle(objCharacter,
                                                new PropertyChangedEventArgs(nameof(Character.CharacterName)));
                        //Timekeeper.Finish("load_event_time");
                    }
                }

                // This weird ordering of WindowState after Show() is meant to counteract a weird WinForms issue where form handle creation crashes
                foreach (CharacterShared frmNewCharacter in lstNewFormsToProcess)
                    frmNewCharacter.QueueThreadSafe(() => frmNewCharacter.WindowState = wsPreference);
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async void mnuOpenForPrinting_Click(object sender, EventArgs e)
        {
            if (Utils.IsUnitTest)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                string strFile;
                using (OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = await LanguageManager.GetStringAsync("DialogFilter_Chum5") + '|' +
                             await LanguageManager.GetStringAsync("DialogFilter_All")
                })
                {
                    if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                        return;
                    strFile = openFileDialog.FileName;
                }

                if (!File.Exists(strFile))
                    return;

                using (Character objCharacter = Program.OpenCharacters.FirstOrDefault(x => x.FileName == strFile)
                                                ?? await Task.Run(() => Program.LoadCharacterAsync(strFile)))
                {
                    await OpenCharacterForPrinting(objCharacter);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        /// <summary>
        /// Open a character's print form up without necessarily opening them up fully for editing.
        /// </summary>
        public async Task OpenCharacterForPrinting(Character objCharacter)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                // Character is already open in an existing form, so switch to it and make it open up its print viewer
                if (SwitchToOpenCharacter(objCharacter))
                {
                    CharacterShared objOpenForm = OpenCharacterForms.Find(x => x.CharacterObject == objCharacter);
                    if (objOpenForm != null)
                        await objOpenForm.DoPrint();
                }
                else
                {
                    using (CharacterSheetViewer frmViewer = this.DoThreadSafeFunc(() => new CharacterSheetViewer()))
                    {
                        await frmViewer.SetCharacters(objCharacter);
                        await frmViewer.ShowDialogSafeAsync(this);
                    }
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async void mnuOpenForExport_Click(object sender, EventArgs e)
        {
            if (Utils.IsUnitTest)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                string strFile;
                using (OpenFileDialog openFileDialog = new OpenFileDialog
                       {
                           Filter = await LanguageManager.GetStringAsync("DialogFilter_Chum5") + '|' +
                                    await LanguageManager.GetStringAsync("DialogFilter_All")
                       })
                {
                    if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                        return;
                    strFile = openFileDialog.FileName;
                }

                if (!File.Exists(strFile))
                    return;

                using (Character objCharacter = Program.OpenCharacters.FirstOrDefault(x => x.FileName == strFile)
                                                ?? await Task.Run(() => Program.LoadCharacterAsync(strFile)))
                {
                    await OpenCharacterForExport(objCharacter);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        /// <summary>
        /// Open a character for exporting without necessarily opening them up fully for editing.
        /// </summary>
        public async Task OpenCharacterForExport(Character objCharacter)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                // Character is already open in an existing form, so switch to it and make it open up its exporter
                if (SwitchToOpenCharacter(objCharacter))
                {
                    CharacterShared objOpenForm = OpenCharacterForms.Find(x => x.CharacterObject == objCharacter);
                    if (objOpenForm != null)
                        await objOpenForm.DoExport();
                }
                else
                {
                    using (ExportCharacter frmExportCharacter
                           = this.DoThreadSafeFunc(() => new ExportCharacter(objCharacter)))
                        await frmExportCharacter.ShowDialogSafeAsync(this);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        /// <summary>
        /// Populate the MRU items.
        /// </summary>
        private async void PopulateMruToolstripMenu(object sender, TextEventArgs e)
        {
            await this.DoThreadSafeFunc(() => DoPopulateMruToolstripMenu(e?.Text));
        }

        private async ValueTask DoPopulateMruToolstripMenu(string strText = "")
        {
            menuStrip.SuspendLayout();
            try
            {
                mnuFileMRUSeparator.Visible = GlobalSettings.FavoriteCharacters.Count > 0
                                              || GlobalSettings.MostRecentlyUsedCharacters.Count > 0;

                if (strText != "mru")
                {
                    for (int i = 0; i < GlobalSettings.MaxMruSize; ++i)
                    {
                        DpiFriendlyToolStripMenuItem objItem;
                        switch (i)
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

                        if (i < GlobalSettings.FavoriteCharacters.Count)
                        {
                            objItem.Text = GlobalSettings.FavoriteCharacters[i];
                            objItem.Tag = GlobalSettings.FavoriteCharacters[i];
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

                string strSpace = await LanguageManager.GetStringAsync("String_Space");
                int i2 = 0;
                for (int i = 0; i < GlobalSettings.MaxMruSize; ++i)
                {
                    if (i2 >= GlobalSettings.MostRecentlyUsedCharacters.Count ||
                        i >= GlobalSettings.MostRecentlyUsedCharacters.Count)
                        continue;
                    string strFile = GlobalSettings.MostRecentlyUsedCharacters[i];
                    if (await GlobalSettings.FavoriteCharacters.ContainsAsync(strFile))
                        continue;
                    DpiFriendlyToolStripMenuItem objItem;
                    switch (i2)
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

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (i2 <= 9
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        && i2 >= 0)
                    {
                        string strNumAsString = (i2 + 1).ToString(GlobalSettings.CultureInfo);
                        objItem.Text = strNumAsString.Insert(strNumAsString.Length - 1, "&") + strSpace + strFile;
                    }
                    else
                        objItem.Text = (i2 + 1).ToString(GlobalSettings.CultureInfo) + strSpace + strFile;

                    objItem.Tag = strFile;
                    objItem.Visible = true;

                    ++i2;
                }
            }
            finally
            {
                menuStrip.ResumeLayout();
            }
        }

        public void OpenDiceRollerWithPool(Character objCharacter = null, int intDice = 0)
        {
            if (GlobalSettings.SingleDiceRoller)
            {
                if (_frmRoller == null)
                {
                    _frmRoller = new DiceRoller(this, objCharacter?.Qualities, intDice);
                    _frmRoller.Show();
                }
                else
                {
                    _frmRoller.Dice = intDice;
                    _frmRoller.ProcessGremlins(objCharacter?.Qualities);
                    _frmRoller.Activate();
                }
            }
            else
            {
                DiceRoller frmRoller = new DiceRoller(this, objCharacter?.Qualities, intDice);
                frmRoller.Show();
            }
        }

        private void mnuClearUnpinnedItems_Click(object sender, EventArgs e)
        {
            GlobalSettings.MostRecentlyUsedCharacters.Clear();
        }

        private async void mnuRestart_Click(object sender, EventArgs e)
        {
            await Utils.RestartApplication(string.Empty, "Message_Options_Restart");
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_SHOWME)
                ShowMe();
            else if (m.Msg == NativeMethods.WM_COPYDATA && _blnAbleToReceiveData)
            {
                ThreadSafeList<Character> lstCharactersToLoad = new ThreadSafeList<Character>();
                Task objCharacterLoadingTask = null;

                using (Program.MainProgressBar = Program.CreateAndShowProgressBar())
                using (CursorWait.New(this))
                {
                    // Extract the file name
                    NativeMethods.CopyDataStruct objReceivedData = (NativeMethods.CopyDataStruct)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.CopyDataStruct));
                    if (objReceivedData.dwData == Program.CommandLineArgsDataTypeId)
                    {
                        string strParam = Marshal.PtrToStringUni(objReceivedData.lpData);
                        string[] strArgs = strParam.Split("<>", StringSplitOptions.RemoveEmptyEntries);

                        ProcessCommandLineArguments(strArgs, out bool blnShowTest, out HashSet<string> setFilesToLoad);
                        try
                        {
                            if (Directory.Exists(Utils.GetAutosavesFolderPath))
                            {
                                // Always process newest autosave if all MRUs are empty
                                bool blnAnyAutosaveInMru = GlobalSettings.MostRecentlyUsedCharacters.Count == 0 &&
                                                           GlobalSettings.FavoriteCharacters.Count == 0;
                                FileInfo objMostRecentAutosave = null;
                                foreach (string strAutosave in Directory.EnumerateFiles(
                                             Utils.GetAutosavesFolderPath,
                                             "*.chum5", SearchOption.AllDirectories))
                                {
                                    FileInfo objAutosave;
                                    try
                                    {
                                        objAutosave = new FileInfo(strAutosave);
                                    }
                                    catch (System.Security.SecurityException)
                                    {
                                        continue;
                                    }
                                    catch (UnauthorizedAccessException)
                                    {
                                        continue;
                                    }

                                    if (objMostRecentAutosave == null || objAutosave.LastWriteTimeUtc >
                                        objMostRecentAutosave.LastWriteTimeUtc)
                                        objMostRecentAutosave = objAutosave;
                                    if (GlobalSettings.MostRecentlyUsedCharacters.Any(x =>
                                            Path.GetFileName(x) == objAutosave.Name) ||
                                        GlobalSettings.FavoriteCharacters.Any(x =>
                                                                                  Path.GetFileName(x)
                                                                                  == objAutosave.Name))
                                        blnAnyAutosaveInMru = true;
                                }

                                // Might have had a crash for an unsaved character, so prompt if we want to load them
                                if (objMostRecentAutosave != null
                                    && blnAnyAutosaveInMru
                                    && !setFilesToLoad.Contains(objMostRecentAutosave.FullName)
                                    && GlobalSettings.MostRecentlyUsedCharacters.All(
                                        x => Path.GetFileName(x) != objMostRecentAutosave.Name)
                                    && GlobalSettings.FavoriteCharacters.All(
                                        x => Path.GetFileName(x) != objMostRecentAutosave.Name)
                                    && Program.ShowMessageBox(string.Format(GlobalSettings.CultureInfo,
                                                                    LanguageManager.GetString(
                                                                        "Message_PossibleCrashAutosaveFound"),
                                                                    objMostRecentAutosave.Name,
                                                                    objMostRecentAutosave.LastWriteTimeUtc
                                                                        .ToLocalTime()),
                                                      LanguageManager.GetString("MessageTitle_AutosaveFound"),
                                                      MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                                    == DialogResult.Yes)
                                {
                                    setFilesToLoad.Add(objMostRecentAutosave.FullName);
                                }
                            }

                            if (setFilesToLoad.Count > 0)
                            {
                                List<Task> tskLoadingTasks = new List<Task>(setFilesToLoad.Count);
                                foreach (string strFile in setFilesToLoad)
                                {
                                    tskLoadingTasks.Add(Task.Run(async () =>
                                    {
                                        Character objCharacter
                                            = await Program.LoadCharacterAsync(strFile);
                                        // ReSharper disable once AccessToDisposedClosure
                                        await lstCharactersToLoad
                                            .AddAsync(objCharacter);
                                    }));
                                }
                                objCharacterLoadingTask = Task.WhenAll(tskLoadingTasks);
                            }
                        }
                        finally
                        {
                            Utils.StringHashSetPool.Return(setFilesToLoad);
                        }

                        Program.MainProgressBar.PerformStep();

                        if (blnShowTest)
                        {
                            TestDataEntries frmTestData = new TestDataEntries();
                            frmTestData.Show();
                        }
                    }
                }
                Task.Run(async () =>
                {
                    if (objCharacterLoadingTask?.IsCompleted == false)
                        await objCharacterLoadingTask;
                    if (lstCharactersToLoad.Count > 0)
                        // ReSharper disable once AccessToDisposedClosure
                        await this.DoThreadSafeFunc(() => OpenCharacterList(lstCharactersToLoad));
                    await lstCharactersToLoad.DisposeAsync();
                });
            }
            base.WndProc(ref m);
        }

        private void ShowMe()
        {
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            // get our current "TopMost" value (ours will always be false though)
            bool blnOldTopMost = TopMost;
            // make our form jump to the top of everything
            TopMost = true;
            // set it back to whatever it was
            TopMost = blnOldTopMost;
        }

        private static void ProcessCommandLineArguments(IReadOnlyCollection<string> strArgs, out bool blnShowTest, out HashSet<string> setFilesToLoad, CustomActivity opLoadActivity = null)
        {
            blnShowTest = false;
            setFilesToLoad = Utils.StringHashSetPool.Get();
            if (strArgs.Count == 0)
                return;
            try
            {
                foreach (string strArg in strArgs)
                {
                    if (strArg.EndsWith(Path.GetFileName(Application.ExecutablePath)))
                        continue;
                    switch (strArg)
                    {
                        case "/test":
                            blnShowTest = true;
                            break;

                        case "/help":
                        case "?":
                        case "/?":
                            {
                                string msg = "Commandline parameters are either " +
                                             Environment.NewLine + "\t/test" + Environment.NewLine +
                                             "\t/help" + Environment.NewLine +
                                             "\t(filename to open)" +
                                             Environment.NewLine +
                                             "\t/plugin:pluginname (like \"SINners\") to trigger (with additional parameters following the symbol \":\")" +
                                             Environment.NewLine;
                                Console.WriteLine(msg);
                                break;
                            }
                        default:
                            {
                                if (strArg.Contains("/plugin"))
                                {
                                    Log.Info(
                                        "Encountered command line argument, that should already have been handled in one of the plugins: " +
                                        strArg);
                                }
                                else if (!strArg.StartsWith('/'))
                                {
                                    if (!File.Exists(strArg))
                                    {
                                        throw new ArgumentException(
                                            "Chummer started with unknown command line arguments: " +
                                            strArgs.Aggregate((j, k) => j + ' ' + k));
                                    }

                                    if (Path.GetExtension(strArg) != ".chum5")
                                        Utils.BreakIfDebug();
                                    if (setFilesToLoad.Contains(strArg))
                                        continue;
                                    setFilesToLoad.Add(strArg);
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                if (opLoadActivity != null)
                {
                    opLoadActivity.SetSuccess(false);
                    ExceptionTelemetry ext = new ExceptionTelemetry(ex)
                    {
                        SeverityLevel = SeverityLevel.Warning
                    };
                    opLoadActivity.MyTelemetryClient.TrackException(ext);
                }
                Log.Warn(ex);
            }
        }

        #endregion Methods

        #region Application Properties

        /// <summary>
        /// The frmDiceRoller window being used by the application.
        /// </summary>
        public DiceRoller RollerWindow
        {
            get => _frmRoller;
            set => _frmRoller = value;
        }

        public ThreadSafeObservableCollection<CharacterShared> OpenCharacterForms => _lstOpenCharacterForms;

        /// <summary>
        /// Set to True at the end of the OnLoad method. Useful because the load method is executed asynchronously, so form might end up getting closed before it fully loads.
        /// </summary>
        public bool IsFinishedLoading { get; private set; }

        #endregion Application Properties
    }
}
