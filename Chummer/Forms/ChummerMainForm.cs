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
using System.Threading;
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
        private ChummerUpdater _frmUpdate;
        private readonly ThreadSafeObservableCollection<CharacterShared> _lstOpenCharacterEditorForms
            = new ThreadSafeObservableCollection<CharacterShared>();
        private readonly ThreadSafeObservableCollection<CharacterSheetViewer> _lstOpenCharacterSheetViewers
            = new ThreadSafeObservableCollection<CharacterSheetViewer>();
        private readonly ThreadSafeObservableCollection<ExportCharacter> _lstOpenCharacterExportForms
            = new ThreadSafeObservableCollection<ExportCharacter>();
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

        public ChummerMainForm(bool blnIsUnitTest = false, bool blnIsUnitTestForUI = false)
        {
            Utils.IsUnitTest = blnIsUnitTest;
            Utils.IsUnitTestForUI = blnIsUnitTestForUI;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _strCurrentVersion = Utils.CurrentChummerVersion.ToString(3);

            Disposed += (sender, args) =>
            {
                _lstOpenCharacterEditorForms.Dispose();
                _lstOpenCharacterSheetViewers.Dispose();
                _lstOpenCharacterExportForms.Dispose();
                _lstCharactersToReopen.Dispose();
            };

            _lstOpenCharacterEditorForms.BeforeClearCollectionChanged += OpenCharacterEditorFormsOnBeforeClearCollectionChanged;
            _lstOpenCharacterEditorForms.CollectionChanged += OpenCharacterEditorFormsOnCollectionChanged;
            _lstOpenCharacterSheetViewers.BeforeClearCollectionChanged += OpenCharacterSheetViewersOnBeforeClearCollectionChanged;
            _lstOpenCharacterSheetViewers.CollectionChanged += OpenCharacterSheetViewersOnCollectionChanged;
            _lstOpenCharacterExportForms.BeforeClearCollectionChanged += OpenCharacterExportFormsOnBeforeClearCollectionChanged;
            _lstOpenCharacterExportForms.CollectionChanged += OpenCharacterExportFormsOnCollectionChanged;

            //lets write that in separate lines to see where the exception is thrown
            if (!GlobalSettings.HideMasterIndex || blnIsUnitTest)
            {
                MasterIndex = new MasterIndex
                {
                    MdiParent = this
                };
                MasterIndex.FormClosed += (sender, args) => MasterIndex = null;
            }
            if (!GlobalSettings.HideCharacterRoster || blnIsUnitTest)
            {
                CharacterRoster = new CharacterRoster
                {
                    MdiParent = this
                };
                CharacterRoster.FormClosed += (sender, args) => CharacterRoster = null;
            }
        }

        private async void OpenCharacterExportFormsOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Utils.IsUnitTest)
                return;
            foreach (ExportCharacter objOldForm in e.OldItems)
            {
                foreach (Character objCharacter in objOldForm.CharacterObjects)
                {
                    if (objCharacter == null)
                        continue;
                    if (await Program.OpenCharacters.ContainsAsync(objCharacter))
                    {
                        if (await Program.OpenCharacters.AllAsync(
                                x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter))
                            && Program.MainForm.OpenFormsWithCharacters.All(
                                x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                            await Program.OpenCharacters.RemoveAsync(objCharacter);
                    }
                    else
                        await objCharacter.DisposeAsync();
                }
            }
        }

        private async void OpenCharacterExportFormsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ExportCharacter objNewForm in e.NewItems)
                    {
                        async void OnNewFormOnFormClosed(object o, FormClosedEventArgs args) => await Program.MainForm.OpenCharacterExportForms.RemoveAsync(objNewForm);
                        objNewForm.FormClosed += OnNewFormOnFormClosed;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (Utils.IsUnitTest)
                        return;
                    foreach (ExportCharacter objOldForm in e.OldItems)
                    {
                        foreach (Character objCharacter in objOldForm.CharacterObjects)
                        {
                            if (objCharacter == null)
                                continue;
                            if (await Program.OpenCharacters.ContainsAsync(objCharacter))
                            {
                                if (await Program.OpenCharacters.AllAsync(
                                        x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter))
                                    && Program.MainForm.OpenFormsWithCharacters.All(
                                        x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                                    await Program.OpenCharacters.RemoveAsync(objCharacter);
                            }
                            else
                                await objCharacter.DisposeAsync();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (!Utils.IsUnitTest)
                    {
                        foreach (ExportCharacter objOldForm in e.OldItems)
                        {
                            if (e.NewItems.Contains(objOldForm))
                                continue;
                            foreach (Character objCharacter in objOldForm.CharacterObjects)
                            {
                                if (objCharacter == null)
                                    continue;
                                if (await Program.OpenCharacters.ContainsAsync(objCharacter))
                                {
                                    if (await Program.OpenCharacters.AllAsync(
                                            x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter))
                                        && Program.MainForm.OpenFormsWithCharacters.All(
                                            x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                                        await Program.OpenCharacters.RemoveAsync(objCharacter);
                                }
                                else
                                    await objCharacter.DisposeAsync();
                            }
                        }
                    }

                    foreach (ExportCharacter objNewForm in e.NewItems)
                    {
                        if (e.OldItems.Contains(objNewForm))
                            continue;
                        async void OnNewFormOnFormClosed(object o, FormClosedEventArgs args) => await Program.MainForm.OpenCharacterExportForms.RemoveAsync(objNewForm);
                        objNewForm.FormClosed += OnNewFormOnFormClosed;
                    }
                    break;
            }
        }

        private async void OpenCharacterSheetViewersOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Utils.IsUnitTest)
                return;
            foreach (CharacterSheetViewer objOldForm in e.OldItems)
            {
                foreach (Character objCharacter in objOldForm.CharacterObjects)
                {
                    if (objCharacter == null)
                        continue;
                    if (await Program.OpenCharacters.ContainsAsync(objCharacter))
                    {
                        if (await Program.OpenCharacters.AllAsync(
                                x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter))
                            && Program.MainForm.OpenFormsWithCharacters.All(
                                x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                            await Program.OpenCharacters.RemoveAsync(objCharacter);
                    }
                    else
                        await objCharacter.DisposeAsync();
                }
            }
        }

        private async void OpenCharacterSheetViewersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (CharacterSheetViewer objNewForm in e.NewItems)
                    {
                        async void OnNewFormOnFormClosed(object o, FormClosedEventArgs args) => await Program.MainForm.OpenCharacterSheetViewers.RemoveAsync(objNewForm);
                        objNewForm.FormClosed += OnNewFormOnFormClosed;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (Utils.IsUnitTest)
                        return;
                    foreach (CharacterSheetViewer objOldForm in e.OldItems)
                    {
                        foreach (Character objCharacter in objOldForm.CharacterObjects)
                        {
                            if (objCharacter == null)
                                continue;
                            if (await Program.OpenCharacters.ContainsAsync(objCharacter))
                            {
                                if (await Program.OpenCharacters.AllAsync(
                                        x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter))
                                    && Program.MainForm.OpenFormsWithCharacters.All(
                                        x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                                    await Program.OpenCharacters.RemoveAsync(objCharacter);
                            }
                            else
                                await objCharacter.DisposeAsync();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (!Utils.IsUnitTest)
                    {
                        foreach (CharacterSheetViewer objOldForm in e.OldItems)
                        {
                            if (e.NewItems.Contains(objOldForm))
                                continue;
                            foreach (Character objCharacter in objOldForm.CharacterObjects)
                            {
                                if (objCharacter == null)
                                    continue;
                                if (await Program.OpenCharacters.ContainsAsync(objCharacter))
                                {
                                    if (await Program.OpenCharacters.AllAsync(
                                            x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter))
                                        && Program.MainForm.OpenFormsWithCharacters.All(
                                            x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                                        await Program.OpenCharacters.RemoveAsync(objCharacter);
                                }
                                else
                                    await objCharacter.DisposeAsync();
                            }
                        }
                    }

                    foreach (CharacterSheetViewer objNewForm in e.NewItems)
                    {
                        if (e.OldItems.Contains(objNewForm))
                            continue;
                        async void OnNewFormOnFormClosed(object o, FormClosedEventArgs args) => await Program.MainForm.OpenCharacterSheetViewers.RemoveAsync(objNewForm);
                        objNewForm.FormClosed += OnNewFormOnFormClosed;
                    }
                    break;
            }
        }

        private bool _blnSkipReopenUntilAllClear;
        private readonly ThreadSafeList<Character> _lstCharactersToReopen = new ThreadSafeList<Character>();

        private async void OpenCharacterEditorFormsOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Utils.IsUnitTest)
                return;
            _blnSkipReopenUntilAllClear = true;
            foreach (CharacterShared objOldForm in e.OldItems)
            {
                if (objOldForm is CharacterCreate objOldCreateForm && objOldCreateForm.IsReopenQueued)
                {
                    await _lstCharactersToReopen.AddAsync(objOldCreateForm.CharacterObject);
                    continue;
                }
                Character objCharacter = objOldForm.CharacterObject;
                if (objCharacter == null)
                    continue;
                if (await Program.OpenCharacters.ContainsAsync(objCharacter))
                {
                    if (await Program.OpenCharacters.AllAsync(x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter))
                        && Program.MainForm.OpenFormsWithCharacters.All(x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                        await Program.OpenCharacters.RemoveAsync(objCharacter);
                }
                else
                    await objCharacter.DisposeAsync();
            }
        }

        private async void OpenCharacterEditorFormsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (CharacterShared objNewForm in e.NewItems)
                    {
                        async void OnNewFormOnFormClosed(object o, FormClosedEventArgs args) => await Program.MainForm.OpenCharacterEditorForms.RemoveAsync(objNewForm);
                        objNewForm.FormClosed += OnNewFormOnFormClosed;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (Utils.IsUnitTest)
                        return;
                    foreach (CharacterShared objOldForm in e.OldItems)
                    {
                        if (objOldForm is CharacterCreate objOldCreateForm && objOldCreateForm.IsReopenQueued)
                        {
                            await _lstCharactersToReopen.AddAsync(objOldCreateForm.CharacterObject);
                            continue;
                        }
                        Character objCharacter = objOldForm.CharacterObject;
                        if (objCharacter == null)
                            continue;
                        if (await Program.OpenCharacters.ContainsAsync(objCharacter))
                        {
                            if (await Program.OpenCharacters.AllAsync(x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter))
                                && Program.MainForm.OpenFormsWithCharacters.All(x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                                await Program.OpenCharacters.RemoveAsync(objCharacter);
                        }
                        else
                            await objCharacter.DisposeAsync();
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (!Utils.IsUnitTest)
                    {
                        foreach (CharacterShared objOldForm in e.OldItems)
                        {
                            if (e.NewItems.Contains(objOldForm))
                                continue;
                            if (objOldForm is CharacterCreate objOldCreateForm && objOldCreateForm.IsReopenQueued)
                            {
                                await _lstCharactersToReopen.AddAsync(objOldCreateForm.CharacterObject);
                                continue;
                            }
                            Character objCharacter = objOldForm.CharacterObject;
                            if (objCharacter == null)
                                continue;
                            if (await Program.OpenCharacters.ContainsAsync(objCharacter))
                            {
                                if (await Program.OpenCharacters.AllAsync(
                                        x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter))
                                    && Program.MainForm.OpenFormsWithCharacters.All(
                                        x => x == objOldForm || !x.CharacterObjects.Contains(objCharacter)))
                                    await Program.OpenCharacters.RemoveAsync(objCharacter);
                            }
                            else
                                await objCharacter.DisposeAsync();
                        }
                    }

                    foreach (CharacterShared objNewForm in e.NewItems)
                    {
                        if (e.OldItems.Contains(objNewForm))
                            continue;
                        async void OnNewFormOnFormClosed(object o, FormClosedEventArgs args) => await Program.MainForm.OpenCharacterEditorForms.RemoveAsync(objNewForm);
                        objNewForm.FormClosed += OnNewFormOnFormClosed;
                    }
                    break;
            }
        }

        //Moved most of the initialization out of the constructor to allow the Mainform to be generated fast
        //in case of a commandline argument not asking for the mainform to be shown.
        private async void ChummerMainForm_Load(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                using (CustomActivity opFrmChummerMain = await Timekeeper.StartSyncronAsync(
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
                        if (NativeMethods.ChangeWindowMessageFilterEx(await this.DoThreadSafeFuncAsync(x => x.Handle),
                                                                      NativeMethods.WM_COPYDATA,
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

                        await this.DoThreadSafeAsync(x => x.Text = MainTitle);
                        dlgOpenFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Chummer") + '|' +
                                             await LanguageManager.GetStringAsync("DialogFilter_Chum5") + '|' +
                                             await LanguageManager.GetStringAsync("DialogFilter_Chum5lz") + '|' +
                                             await LanguageManager.GetStringAsync("DialogFilter_All");

                        //this.toolsMenu.DropDownItems.Add("GM Dashboard").Click += this.dashboardToolStripMenuItem_Click;

#if !DEBUG
                        // If Automatic Updates are enabled, check for updates immediately.
                        StartAutoUpdateChecker();
#endif

                        GlobalSettings.MruChanged += PopulateMruToolstripMenu;

                        // Populate the MRU list.
                        await DoPopulateMruToolstripMenu();

                        Program.MainForm = this;

                        ThreadSafeList<Character> lstCharactersToLoad = new ThreadSafeList<Character>(1);
                        try
                        {
                            using (ThreadSafeForm<LoadingBar> frmLoadingBar
                                   = await Program.CreateAndShowProgressBarAsync(
                                       Text,
                                       GlobalSettings.AllowEasterEggs ? 4 : 3))
                            {
                                Task objCharacterLoadingTask = null;
                                await frmLoadingBar.MyForm.PerformStepAsync(
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
                                            foreach (string strAutosave in Directory
                                                                           .EnumerateFiles(
                                                                               Utils.GetAutosavesFolderPath, "*.chum5",
                                                                               SearchOption.AllDirectories)
                                                                           .Concat(Directory.EnumerateFiles(
                                                                               Utils.GetAutosavesFolderPath,
                                                                               "*.chum5lz",
                                                                               SearchOption.AllDirectories)))
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
                                            await frmLoadingBar.MyForm.ResetAsync(
                                                (GlobalSettings.AllowEasterEggs ? 3 : 2)
                                                + setFilesToLoad.Count * Character.NumLoadingSections);
                                            List<Task> tskLoadingTasks = new List<Task>(setFilesToLoad.Count);
                                            foreach (string strFile in setFilesToLoad)
                                            {
                                                tskLoadingTasks.Add(Task.Run(async () =>
                                                {
                                                    Character objCharacter
                                                        = await Program.LoadCharacterAsync(
                                                            // ReSharper disable once AccessToDisposedClosure
                                                            strFile, frmLoadingBar: frmLoadingBar.MyForm);
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

                                await frmLoadingBar.MyForm.PerformStepAsync(
                                    await LanguageManager.GetStringAsync("Title_MasterIndex"));

                                if (MasterIndex != null)
                                {
                                    await MasterIndex.DoThreadSafeAsync(x =>
                                    {
                                        if (CharacterRoster == null)
                                            x.WindowState = FormWindowState.Maximized;
                                        x.Show();
                                    });
                                }

                                await frmLoadingBar.MyForm.PerformStepAsync(
                                    await LanguageManager.GetStringAsync("String_CharacterRoster"));

                                if (CharacterRoster != null)
                                {
                                    await CharacterRoster.DoThreadSafeAsync(x =>
                                    {
                                        if (MasterIndex == null)
                                            x.WindowState = FormWindowState.Maximized;
                                        x.Show();
                                    });
                                }

                                if (GlobalSettings.AllowEasterEggs)
                                {
                                    await frmLoadingBar.MyForm.PerformStepAsync(
                                        await LanguageManager.GetStringAsync("String_Chummy"));
                                    _mascotChummy = await this.DoThreadSafeFuncAsync(x =>
                                    {
                                        Chummy objReturn = new Chummy(null);
                                        x.Disposed += (o, args) => objReturn.Dispose();
                                        return objReturn;
                                    });
                                    await _mascotChummy.DoThreadSafeAsync(x => x.Show(this));
                                }

                                // This weird ordering of WindowState after Show() is meant to counteract a weird WinForms issue where form handle creation crashes
                                if (MasterIndex != null && CharacterRoster != null)
                                {
                                    await MasterIndex.DoThreadSafeAsync(x => x.WindowState = FormWindowState.Maximized);
                                    await CharacterRoster.DoThreadSafeAsync(
                                        x => x.WindowState = FormWindowState.Maximized);
                                }

                                if (blnShowTest)
                                {
                                    TestDataEntries frmTestData
                                        = await this.DoThreadSafeFuncAsync(() => new TestDataEntries());
                                    await frmTestData.DoThreadSafeAsync(x => x.Show());
                                }

                                await Program.PluginLoader.CallPlugins(toolsMenu, opFrmChummerMain);

                                // Set the Tag for each ToolStrip item so it can be translated.
                                await menuStrip.DoThreadSafeAsync(x =>
                                {
                                    foreach (ToolStripMenuItem tssItem in x.Items.OfType<ToolStripMenuItem>())
                                    {
                                        tssItem.UpdateLightDarkMode();
                                        tssItem.TranslateToolStripItemsRecursively();
                                    }
                                });
                                await mnuProcessFile.DoThreadSafeAsync(x =>
                                {
                                    foreach (ToolStripMenuItem tssItem in x.Items.OfType<ToolStripMenuItem>())
                                    {
                                        tssItem.UpdateLightDarkMode();
                                        tssItem.TranslateToolStripItemsRecursively();
                                    }
                                });

                                if (objCharacterLoadingTask?.IsCompleted == false)
                                    await objCharacterLoadingTask;
                            }

                            if (lstCharactersToLoad.Count > 0)
                                await OpenCharacterList(lstCharactersToLoad);
                        }
                        finally
                        {
                            await lstCharactersToLoad.DisposeAsync();
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

                        await this.DoThreadSafeAsync(x =>
                        {
                            x.Size = new Size(intDefaultWidth, intDefaultHeight);
                            x.StartPosition = FormStartPosition.CenterScreen;
                        });
                    }
                    else
                    {
                        await this.DoThreadSafeAsync(x =>
                        {
                            if (!Utils.IsUnitTest)
                            {
                                x.WindowState = Properties.Settings.Default.WindowState;
                                if (x.WindowState == FormWindowState.Minimized)
                                    x.WindowState = FormWindowState.Normal;
                            }

                            x.Location = Properties.Settings.Default.Location;
                            x.Size = Properties.Settings.Default.Size;
                        });
                    }

                    if (!Utils.IsUnitTest && GlobalSettings.StartupFullscreen)
                        await this.DoThreadSafeAsync(x => x.WindowState = FormWindowState.Maximized);
                }

                if (Utils.IsUnitTestForUI)
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

        private void OpenCharactersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
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
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (Character objCharacter in notifyCollectionChangedEventArgs.OldItems)
                            objCharacter.PropertyChanged -= UpdateCharacterTabTitle;
                        foreach (Character objCharacter in notifyCollectionChangedEventArgs.NewItems)
                            objCharacter.PropertyChanged += UpdateCharacterTabTitle;
                        break;
                    }
            }
        }

        public CharacterRoster CharacterRoster { get; private set; }

        public MasterIndex MasterIndex { get; private set; }

#if !DEBUG
        private Uri UpdateLocation { get; } = new Uri(GlobalSettings.PreferNightlyBuilds
            ? "https://api.github.com/repos/chummer5a/chummer5a/releases"
            : "https://api.github.com/repos/chummer5a/chummer5a/releases/latest");

        private Task _tskVersionUpdate;

        private CancellationTokenSource _objVersionUpdaterCancellationTokenSource;

        private async ValueTask DoCacheGitVersion(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
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

            token.ThrowIfCancellationRequested();

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

                    token.ThrowIfCancellationRequested();

                    // Get the stream containing content returned by the server.
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        if (dataStream == null)
                        {
                            Utils.CachedGitVersion = null;
                            return;
                        }

                        token.ThrowIfCancellationRequested();

                        // Open the stream using a StreamReader for easy access.
                        using (StreamReader reader = new StreamReader(dataStream, System.Text.Encoding.UTF8, true))
                        {
                            token.ThrowIfCancellationRequested();

                            // Read the content.
                            string responseFromServer = await reader.ReadToEndAsync();

                            string line = responseFromServer.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(x => x.Contains("tag_name"));

                            token.ThrowIfCancellationRequested();

                            Version verLatestVersion = null;
                            if (!string.IsNullOrEmpty(line))
                            {
                                string strVersion = line.Substring(line.IndexOf(':') + 1);
                                int intPos = strVersion.IndexOf('}');
                                if (intPos != -1)
                                    strVersion = strVersion.Substring(0, intPos);
                                strVersion = strVersion.FastEscape('\"');

                                // Adds zeroes if minor and/or build version are missing
                                if (strVersion.Count(x => x == '.') < 2)
                                {
                                    strVersion += ".0";
                                    if (strVersion.Count(x => x == '.') < 2)
                                    {
                                        strVersion += ".0";
                                    }
                                }

                                token.ThrowIfCancellationRequested();

                                if (!Version.TryParse(strVersion.TrimStartOnce("Nightly-v"), out verLatestVersion))
                                    verLatestVersion = null;

                                token.ThrowIfCancellationRequested();
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

        private void StartAutoUpdateChecker()
        {
            CancellationTokenSource objNewSource = new CancellationTokenSource();
            CancellationTokenSource objTemp = Interlocked.Exchange(ref _objVersionUpdaterCancellationTokenSource, objNewSource);
            if (objTemp?.IsCancellationRequested == false)
            {
                try
                {
                    objTemp.Cancel(false);
                }
                finally
                {
                    objTemp.Dispose();
                }
            }

            try
            {
                if (_tskVersionUpdate != null)
                {
                    while (!_tskVersionUpdate.IsCompleted)
                        Utils.SafeSleep();
                    if (_tskVersionUpdate.Exception != null)
                        throw _tskVersionUpdate.Exception;
                }
            }
            catch (OperationCanceledException)
            {
                // Swallow this
            }
            catch
            {
                Interlocked.CompareExchange(ref _objVersionUpdaterCancellationTokenSource, null, objNewSource);
                objNewSource.Dispose();
                throw;
            }

            CancellationToken token = objNewSource.Token;
            _tskVersionUpdate = Task.Run(async () =>
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    await DoCacheGitVersion(token);
                    token.ThrowIfCancellationRequested();
                    if (Utils.GitUpdateAvailable > 0)
                    {
                        string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token);
                        string strNewText = Application.ProductName + strSpace + '-' + strSpace +
                                            await LanguageManager.GetStringAsync("String_Version", token: token)
                                            + strSpace + _strCurrentVersion + strSpace + '-' + strSpace
                                            + string.Format(GlobalSettings.CultureInfo,
                                                            await LanguageManager.GetStringAsync(
                                                                "String_Update_Available", token: token),
                                                            Utils.CachedGitVersion);
                        await this.DoThreadSafeAsync(x => x.Text = strNewText, token);
                        if (GlobalSettings.AutomaticUpdate && _frmUpdate == null)
                        {
                            _frmUpdate = await this.DoThreadSafeFuncAsync(x =>
                            {
                                ChummerUpdater objReturn = new ChummerUpdater();
                                x.Disposed += (o, args) => objReturn.Dispose();
                                return objReturn;
                            }, token);
                            await _frmUpdate.DoThreadSafeAsync(x =>
                            {
                                x.FormClosed += ResetChummerUpdater;
                                x.SilentMode = true;
                            }, token);
                        }
                    }
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(TimeSpan.FromHours(1), token);
                }
                // ReSharper disable once FunctionNeverReturns
            }, token);
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

        private void closeWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabForms.SelectedTab.Tag is Form frmCurrentForm)
                frmCurrentForm.Close();
        }

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
                using (ThreadSafeForm<EditGlobalSettings> frmOptions = await ThreadSafeForm<EditGlobalSettings>.GetAsync(() => new EditGlobalSettings()))
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
                using (ThreadSafeForm<EditCharacterSettings> frmCharacterOptions =
                       await ThreadSafeForm<EditCharacterSettings>.GetAsync(() =>
                                                                                new EditCharacterSettings(
                                                                                    (tabForms.SelectedTab?.Tag as
                                                                                        CharacterShared)
                                                                                    ?.CharacterObject
                                                                                    ?.Settings)))
                    await frmCharacterOptions.ShowDialogSafeAsync(this);
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async void mnuToolsUpdate_Click(object sender, EventArgs e)
        {
            // Only a single instance of the updater can be open, so either find the current instance and focus on it, or create a new one.
            if (_frmUpdate == null)
            {
                _frmUpdate = await this.DoThreadSafeFuncAsync(x =>
                {
                    ChummerUpdater objReturn = new ChummerUpdater();
                    x.Disposed += (o, args) => objReturn.Dispose();
                    return objReturn;
                });
                await _frmUpdate.DoThreadSafeAsync(x =>
                {
                    x.FormClosed += ResetChummerUpdater;
                    x.Show();
                });
            }
            // Silent updater is running, so make it visible
            else
            {
                await _frmUpdate.DoThreadSafeAsync(x =>
                {
                    if (x.SilentMode)
                    {
                        x.SilentMode = false;
                        x.Show();
                    }
                    else
                    {
                        x.Focus();
                    }
                });
            }
        }

        private async void mnuMasterIndex_Click(object sender, EventArgs e)
        {
            if (MasterIndex.IsNullOrDisposed())
            {
                MasterIndex = await this.DoThreadSafeFuncAsync(() => new MasterIndex());
                await MasterIndex.DoThreadSafeAsync(x =>
                {
                    x.FormClosed += (y, args) => MasterIndex = null;
                    x.MdiParent = this;
                    if (CharacterRoster.IsNullOrDisposed())
                        x.WindowState = FormWindowState.Maximized;
                    x.Show();
                    if (!CharacterRoster.IsNullOrDisposed())
                        x.WindowState = FormWindowState.Maximized;
                });
            }
            else
            {
                await MasterIndex.DoThreadSafeAsync(x =>
                {
                    foreach (TabPage objTabPage in tabForms.TabPages)
                    {
                        if (objTabPage.Tag != x)
                            continue;
                        tabForms.SelectTab(objTabPage);
                        if (_mascotChummy != null)
                            _mascotChummy.CharacterObject = null;
                        return;
                    }
                    x.BringToFront();
                });
            }
        }

        private async void mnuCharacterRoster_Click(object sender, EventArgs e)
        {
            if (CharacterRoster.IsNullOrDisposed())
            {
                CharacterRoster = await this.DoThreadSafeFuncAsync(() => new CharacterRoster());
                await CharacterRoster.DoThreadSafeAsync(x =>
                {
                    x.FormClosed += (y, args) => CharacterRoster = null;
                    x.MdiParent = this;
                    if (MasterIndex.IsNullOrDisposed())
                        x.WindowState = FormWindowState.Maximized;
                    x.Show();
                    if (!MasterIndex.IsNullOrDisposed())
                        x.WindowState = FormWindowState.Maximized;
                });
            }
            else
            {
                await CharacterRoster.DoThreadSafeAsync(x =>
                {
                    foreach (TabPage objTabPage in tabForms.TabPages)
                    {
                        if (objTabPage.Tag != x)
                            continue;
                        tabForms.SelectTab(objTabPage);
                        if (_mascotChummy != null)
                            _mascotChummy.CharacterObject = null;
                        return;
                    }
                    x.BringToFront();
                });
            }
        }

        private void ResetChummerUpdater(object sender, EventArgs e)
        {
            ChummerUpdater frmUpdate = Interlocked.Exchange(ref _frmUpdate, null);
            frmUpdate?.Close();
        }

        private async void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ThreadSafeForm<About> frmAbout = await ThreadSafeForm<About>.GetAsync(() => new About()))
                await frmAbout.ShowDialogSafeAsync(this);
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

        private async void mnuFilePrintMultiple_Click(object sender, EventArgs e)
        {
            if (PrintMultipleCharactersForm.IsNullOrDisposed())
            {
                PrintMultipleCharactersForm = await this.DoThreadSafeFuncAsync(() => new PrintMultipleCharacters());
                await PrintMultipleCharactersForm.DoThreadSafeAsync(x => x.Show(this));
            }
            else
                await PrintMultipleCharactersForm.DoThreadSafeAsync(x => x.Activate());
        }

        private async void mnuHelpRevisionHistory_Click(object sender, EventArgs e)
        {
            using (ThreadSafeForm<VersionHistory> frmShowHistory = await ThreadSafeForm<VersionHistory>.GetAsync(() => new VersionHistory()))
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
                    using (ThreadSafeForm<SelectBuildMethod> frmPickSetting
                           = await ThreadSafeForm<SelectBuildMethod>.GetAsync(
                               () => new SelectBuildMethod(objCharacter)))
                    {
                        if (await frmPickSetting.ShowDialogSafeAsync(this) == DialogResult.Cancel)
                            return;
                    }

                    // Override the defaults for the setting.
                    objCharacter.IgnoreRules = true;
                    objCharacter.IsCritter = true;
                    await objCharacter.SetCreatedAsync(true);

                    // Show the Metatype selection window.
                    using (ThreadSafeForm<SelectMetatypeKarma> frmSelectMetatype =
                           await ThreadSafeForm<SelectMetatypeKarma>.GetAsync(() =>
                                                                                  new SelectMetatypeKarma(
                                                                                      objCharacter, "critters.xml")))
                    {
                        if (await frmSelectMetatype.ShowDialogSafeAsync(this) == DialogResult.Cancel)
                            return;
                    }

                    await Program.OpenCharacters.AddAsync(objCharacter);
                    await OpenCharacter(objCharacter, false);
                }
                finally
                {
                    await objCharacter
                        .DisposeAsync(); // Fine here because Dispose()/DisposeAsync() code is skipped if the character is open in a form
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async void mnuMRU_Click(object sender, EventArgs e)
        {
            string strFileName = await mnuProcessFile.DoThreadSafeFuncAsync(() => ((ToolStripMenuItem)sender).Tag) as string;
            if (string.IsNullOrEmpty(strFileName))
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                Character objCharacter;
                using (ThreadSafeForm<LoadingBar> frmLoadingBar
                       = await Program.CreateAndShowProgressBarAsync(strFileName, Character.NumLoadingSections))
                    objCharacter = await Program.LoadCharacterAsync(strFileName, frmLoadingBar: frmLoadingBar.MyForm);
                await OpenCharacter(objCharacter);
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async void mnuMRU_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            string strFileName = ((ToolStripMenuItem)sender).Tag as string;
            if (!string.IsNullOrEmpty(strFileName))
                await GlobalSettings.FavoriteCharacters.AddWithSortAsync(strFileName);
        }

        private async void mnuStickyMRU_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            string strFileName = ((ToolStripMenuItem)sender).Tag as string;
            if (!string.IsNullOrEmpty(strFileName))
            {
                await GlobalSettings.FavoriteCharacters.RemoveAsync(strFileName);
                await GlobalSettings.MostRecentlyUsedCharacters.InsertAsync(0, strFileName);
            }
        }

        private async void ChummerMainForm_MdiChildActivate(object sender, EventArgs e)
        {
            // If there are no child forms, hide the tab control.
            Form objMdiChild = await this.DoThreadSafeFuncAsync(x => x.ActiveMdiChild);
            if (objMdiChild != null)
            {
                await objMdiChild.DoThreadSafeAsync(x =>
                {
                    if (x.WindowState == FormWindowState.Minimized)
                    {
                        x.WindowState = FormWindowState.Normal;
                    }
                });

                // If this is a new child form and does not have a tab page, create one.
                if (!(await objMdiChild.DoThreadSafeFuncAsync(x => x.Tag) is TabPage))
                {
                    TabPage objTabPage = await this.DoThreadSafeFuncAsync(() => new TabPage
                    {
                        // Add a tab page.
                        Tag = objMdiChild,
                        Parent = tabForms
                    });

                    switch (objMdiChild)
                    {
                        case CharacterShared frmCharacterShared:
                        {
                            await objTabPage.DoThreadSafeAsync(x => x.Text = frmCharacterShared.CharacterObject.CharacterName);
                            if (GlobalSettings.AllowEasterEggs && _mascotChummy != null)
                            {
                                _mascotChummy.CharacterObject = frmCharacterShared.CharacterObject;
                            }

                            break;
                        }
                        case CharacterSheetViewer frmSheetViewer:
                        {
                            string strSpace = await LanguageManager.GetStringAsync("String_Space");
                            string strSheet = await LanguageManager.GetStringAsync("String_Sheet_Blank");
                            await objTabPage.DoThreadSafeAsync(
                                x => x.Text = string.Format(
                                    strSheet,
                                    string.Join(',' + strSpace,
                                                frmSheetViewer.CharacterObjects.Select(y => y.CharacterName.Trim()))));
                            if (GlobalSettings.AllowEasterEggs && _mascotChummy != null)
                            {
                                _mascotChummy.CharacterObject = null;
                            }

                            break;
                        }
                        case ExportCharacter frmExportCharacter:
                        {
                            string strExport = await LanguageManager.GetStringAsync("String_Export_Blank");
                            await objTabPage.DoThreadSafeAsync(
                                x => x.Text = string.Format(
                                    strExport,
                                    frmExportCharacter.CharacterObject.CharacterName.Trim()));
                            if (GlobalSettings.AllowEasterEggs && _mascotChummy != null)
                            {
                                _mascotChummy.CharacterObject = null;
                            }

                            break;
                        }
                        default:
                        {
                            string strKey = await objMdiChild.DoThreadSafeFuncAsync(x => x.Tag?.ToString());
                            if (!string.IsNullOrEmpty(strKey))
                            {
                                string strTagText
                                    = await LanguageManager.GetStringAsync(strKey, GlobalSettings.Language, false);
                                if (!string.IsNullOrEmpty(strTagText))
                                    await objTabPage.DoThreadSafeAsync(x => x.Text = strTagText);
                            }
                            if (GlobalSettings.AllowEasterEggs && _mascotChummy != null)
                            {
                                _mascotChummy.CharacterObject = null;
                            }

                            break;
                        }
                    }

                    await tabForms.DoThreadSafeAsync(x =>
                    {
                        if (objMdiChild == MasterIndex && x.TabPages.IndexOf(objTabPage) != 0)
                        {
                            x.TabPages.Remove(objTabPage);
                            x.TabPages.Insert(0, objTabPage);
                        }
                        else if (objMdiChild == CharacterRoster)
                        {
                            if (MasterIndex.IsNullOrDisposed())
                            {
                                if (x.TabPages.IndexOf(objTabPage) != 0)
                                {
                                    x.TabPages.Remove(objTabPage);
                                    x.TabPages.Insert(0, objTabPage);
                                }
                            }
                            else if (x.TabPages.IndexOf(objTabPage) != 1)
                            {
                                x.TabPages.Remove(objTabPage);
                                x.TabPages.Insert(1, objTabPage);
                            }
                        }
                        x.SelectedTab = objTabPage;
                    });

                    await objMdiChild.DoThreadSafeAsync(x =>
                    {
                        x.Tag = objTabPage;
                        x.FormClosed += ActiveMdiChild_FormClosed;
                    });
                }
            }
            // Don't show the tab control if there is only one window open.
            await tabForms.DoThreadSafeAsync(x => x.Visible = x.TabCount > 1);
        }

        private async void ActiveMdiChild_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (sender is Form objForm)
            {
                objForm.FormClosed -= ActiveMdiChild_FormClosed;
                if (objForm.Tag is TabPage objTabPage)
                {
                    await tabForms.DoThreadSafeAsync(x =>
                    {
                        if (x.TabCount > 1)
                        {
                            int intSelectTab = x.TabPages.IndexOf(objTabPage);
                            if (intSelectTab > 0)
                            {
                                if (intSelectTab + 1 >= x.TabCount)
                                    --intSelectTab;
                                else
                                    ++intSelectTab;
                                x.SelectedIndex = intSelectTab;
                            }
                        }
                    });
                    await objTabPage.DoThreadSafeAsync(x => x.Dispose());
                }
            }

            await tabForms.DoThreadSafeAsync(x =>
            {
                // Don't show the tab control if there is only one window open.
                if (x.TabCount <= 1)
                    x.Visible = false;
            });

            await DoReopenCharacters();
        }

        private void tabForms_SelectedIndexChanged(object sender, EventArgs e)
        {
            (tabForms.SelectedTab?.Tag as Form)?.Select();
        }

        private async Task DoReopenCharacters(CancellationToken token = default)
        {
            if (this.IsNullOrDisposed())
                return;
            if (_blnSkipReopenUntilAllClear)
            {
                if (await OpenCharacterEditorForms.GetCountAsync(token) != 0)
                    return;
                _blnSkipReopenUntilAllClear = false;
            }
            List<Character> lstLocal;
            IAsyncDisposable objLocker = await _lstCharactersToReopen.LockObject.EnterWriteLockAsync(token);
            try
            {
                lstLocal = await _lstCharactersToReopen.ToListAsync(token);
                await _lstCharactersToReopen.ClearAsync(token);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }

            await Program.OpenCharacterList(lstLocal, token: token);
        }

        /// <summary>
        /// Switch to an open character form if one is available.
        /// </summary>
        public async Task<bool> SwitchToOpenCharacter(Character objCharacter, CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token);
            try
            {
                if (objCharacter == null)
                    return false;
                CharacterShared objCharacterForm
                    = await OpenCharacterEditorForms.FirstOrDefaultAsync(x => x.CharacterObject == objCharacter, token);
                if (objCharacterForm == null)
                    return false;
                await objCharacterForm.DoThreadSafeAsync(x =>
                {
                    foreach (TabPage objTabPage in tabForms.TabPages)
                    {
                        if (objTabPage.Tag != x)
                            continue;
                        tabForms.SelectTab(objTabPage);
                        if (_mascotChummy != null)
                            _mascotChummy.CharacterObject = objCharacter;
                        return;
                    }

                    x.BringToFront();
                }, token);
                return true;
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        /// <summary>
        /// Switch to an open character sheet viewer if one is available.
        /// </summary>
        public async Task<bool> SwitchToOpenPrintCharacter(Character objCharacter, CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token);
            try
            {
                if (objCharacter == null)
                    return false;
                CharacterSheetViewer objCharacterForm
                    = await OpenCharacterSheetViewers.FirstOrDefaultAsync(
                        x => x.CharacterObjects.Contains(objCharacter), token);
                if (objCharacterForm == null)
                    return false;
                await objCharacterForm.DoThreadSafeAsync(x =>
                {
                    foreach (TabPage objTabPage in tabForms.TabPages)
                    {
                        if (objTabPage.Tag != x)
                            continue;
                        tabForms.SelectTab(objTabPage);
                        if (_mascotChummy != null)
                            _mascotChummy.CharacterObject = objCharacter;
                        return;
                    }

                    x.BringToFront();
                }, token);
                return true;
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        /// <summary>
        /// Switch to an open character exporter if one is available.
        /// </summary>
        public async Task<bool> SwitchToOpenExportCharacter(Character objCharacter, CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token);
            try
            {
                if (objCharacter == null)
                    return false;
                ExportCharacter objCharacterForm
                    = await OpenCharacterExportForms.FirstOrDefaultAsync(
                        x => ReferenceEquals(x.CharacterObject, objCharacter), token);
                if (objCharacterForm == null)
                    return false;
                await objCharacterForm.DoThreadSafeAsync(x =>
                {
                    foreach (TabPage objTabPage in tabForms.TabPages)
                    {
                        if (objTabPage.Tag != x)
                            continue;
                        tabForms.SelectTab(objTabPage);
                        if (_mascotChummy != null)
                            _mascotChummy.CharacterObject = objCharacter;
                        return;
                    }

                    x.BringToFront();
                }, token);
                return true;
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        public async void UpdateCharacterTabTitle(object sender, PropertyChangedEventArgs e)
        {
            // Change the TabPage's text to match the character's name (or "Unnamed Character" if they are currently unnamed).
            if (e?.PropertyName == nameof(Character.CharacterName) && sender is Character objCharacter && await tabForms.DoThreadSafeFuncAsync(x => x.TabCount) > 0)
            {
                await UpdateCharacterTabTitle(objCharacter, objCharacter.CharacterName.Trim());
            }
        }

        private async Task UpdateCharacterTabTitle(Character objCharacter, string strCharacterName, CancellationToken token = default)
        {
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token);
            string strSheet = await LanguageManager.GetStringAsync("String_Sheet_Blank", token: token);
            string strExport = await LanguageManager.GetStringAsync("String_Export_Blank", token: token);
            await tabForms.DoThreadSafeAsync(x =>
            {
                foreach (TabPage objTabPage in x.TabPages)
                {
                    switch (objTabPage.Tag)
                    {
                        case CharacterShared frmCharacter when frmCharacter.CharacterObject == objCharacter:
                            objTabPage.Text = strCharacterName;
                            break;
                        case CharacterSheetViewer frmCharacterSheetViewer when frmCharacterSheetViewer.CharacterObjects.Contains(objCharacter):
                            objTabPage.Text
                                = string.Format(
                                    strSheet,
                                    string.Join(',' + strSpace,
                                                frmCharacterSheetViewer.CharacterObjects.Select(
                                                    y => y.CharacterName.Trim())));
                            break;
                        case ExportCharacter frmExport when frmExport.CharacterObject == objCharacter:
                            objTabPage.Text = string.Format(strExport, strCharacterName);
                            break;
                    }
                }
            }, token: token);
        }

        public void RefreshAllTabTitles()
        {
            string strSpace = LanguageManager.GetString("String_Space");
            string strSheet = LanguageManager.GetString("String_Sheet_Blank");
            string strExport = LanguageManager.GetString("String_Export_Blank");
            tabForms.DoThreadSafe(x =>
            {
                foreach (TabPage objTabPage in x.TabPages)
                {
                    switch (objTabPage.Tag)
                    {
                        case CharacterShared frmCharacter:
                            objTabPage.Text = frmCharacter.CharacterObject.CharacterName.Trim();
                            break;
                        case CharacterSheetViewer frmCharacterSheetViewer:
                            objTabPage.Text
                                = string.Format(
                                    strSheet,
                                    string.Join(',' + strSpace,
                                                frmCharacterSheetViewer.CharacterObjects.Select(
                                                    y => y.CharacterName.Trim())));
                            break;
                        case ExportCharacter frmExport:
                            objTabPage.Text = string.Format(strExport, frmExport.CharacterObject.CharacterName);
                            break;
                        case Form frmOther:
                            objTabPage.Text = frmOther.Text;
                            break;
                    }
                }
            });
        }

        public async Task RefreshAllTabTitlesAsync(CancellationToken token = default)
        {
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token);
            string strSheet = await LanguageManager.GetStringAsync("String_Sheet_Blank", token: token);
            string strExport = await LanguageManager.GetStringAsync("String_Export_Blank", token: token);
            await tabForms.DoThreadSafeAsync(x =>
            {
                foreach (TabPage objTabPage in x.TabPages)
                {
                    switch (objTabPage.Tag)
                    {
                        case CharacterShared frmCharacter:
                            objTabPage.Text = frmCharacter.CharacterObject.CharacterName.Trim();
                            break;
                        case CharacterSheetViewer frmCharacterSheetViewer:
                            objTabPage.Text
                                = string.Format(
                                    strSheet,
                                    string.Join(',' + strSpace,
                                                frmCharacterSheetViewer.CharacterObjects.Select(
                                                    y => y.CharacterName.Trim())));
                            break;
                        case ExportCharacter frmExport:
                            objTabPage.Text = string.Format(strExport, frmExport.CharacterObject.CharacterName);
                            break;
                        case Form frmOther:
                            objTabPage.Text = frmOther.Text;
                            break;
                    }
                }
            }, token);
        }

        private async void mnuToolsDiceRoller_Click(object sender, EventArgs e)
        {
            if (GlobalSettings.SingleDiceRoller)
            {
                // Only a single instance of the Dice Roller window is allowed, so either find the existing one and focus on it, or create a new one.
                if (RollerWindow == null)
                {
                    RollerWindow = await this.DoThreadSafeFuncAsync(x =>
                    {
                        DiceRoller objReturn = new DiceRoller(this);
                        x.Disposed += (o, args) => objReturn.Dispose();
                        return objReturn;
                    });
                    await RollerWindow.DoThreadSafeAsync(x => x.Show());
                }
                else
                {
                    await RollerWindow.DoThreadSafeAsync(x => x.Activate());
                }
            }
            else
            {
                // No limit on the number of Dice Roller windows, so just create a new one.
                DiceRoller frmRoller = await this.DoThreadSafeFuncAsync(() => new DiceRoller(this));
                await frmRoller.DoThreadSafeAsync(x => x.Show());
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
                string[] s = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
                if (s.Length == 0)
                    return;
                Character[] lstCharacters = new Character[s.Length];
                using (ThreadSafeForm<LoadingBar> frmLoadingBar
                       = await Program.CreateAndShowProgressBarAsync(string.Empty,
                                                                     Character.NumLoadingSections * s.Length))
                {
                    Task<Character>[] tskCharacterLoads = new Task<Character>[s.Length];
                    // Array instead of concurrent bag because we want to preserve order
                    for (int i = 0; i < s.Length; ++i)
                    {
                        string strFile = s[i];
                        // ReSharper disable once AccessToDisposedClosure
                        tskCharacterLoads[i]
                            = Task.Run(() => Program.LoadCharacterAsync(strFile, frmLoadingBar: frmLoadingBar.MyForm));
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

        private async void ChummerMainForm_Closing(object sender, FormClosingEventArgs e)
        {
            Program.OpenCharacters.CollectionChanged -= OpenCharactersOnCollectionChanged;
            foreach (Character objCharacter in Program.OpenCharacters)
                objCharacter.PropertyChanged -= UpdateCharacterTabTitle;
#if !DEBUG
            CancellationTokenSource objTemp = Interlocked.Exchange(ref _objVersionUpdaterCancellationTokenSource, null);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }
#endif
            // ReSharper disable MethodSupportsCancellation
            await Task.WhenAll(_lstOpenCharacterEditorForms.ClearAsync().AsTask(),
                               _lstOpenCharacterExportForms.ClearAsync().AsTask(),
                               _lstOpenCharacterSheetViewers.ClearAsync().AsTask());
            // ReSharper restore MethodSupportsCancellation
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

            HeroLabImporter frmImporter = await this.DoThreadSafeFuncAsync(() => new HeroLabImporter());
            await frmImporter.DoThreadSafeAsync(x => x.Show());
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
            if (await tabForms.DoThreadSafeFuncAsync(x => x.SelectedTab.Tag) is CharacterShared objShared
                && await objShared.SaveCharacter() && objShared is CharacterCreate objCreate
                && objCreate.IsReopenQueued)
            {
                await objCreate.DoThreadSafeAsync(x => x.Close());
            }
        }

        private async void tsSaveAs_Click(object sender, EventArgs e)
        {
            if (await tabForms.DoThreadSafeFuncAsync(x => x.SelectedTab.Tag) is CharacterShared objShared
                && await objShared.SaveCharacterAs() && objShared is CharacterCreate objCreate
                && objCreate.IsReopenQueued)
            {
                await objCreate.DoThreadSafeAsync(x => x.Close());
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
            if (await tabForms.DoThreadSafeFuncAsync(x => x.SelectedTab.Tag) is CharacterShared objShared)
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
                    using (ThreadSafeForm<SelectBuildMethod> frmBP
                           = await ThreadSafeForm<SelectBuildMethod>.GetAsync(
                               () => new SelectBuildMethod(objCharacter)))
                    {
                        if (await frmBP.ShowDialogSafeAsync(this) != DialogResult.OK)
                            return;
                    }

                    // Show the Metatype selection window.
                    if (objCharacter.EffectiveBuildMethodUsesPriorityTables)
                    {
                        using (ThreadSafeForm<SelectMetatypePriority> frmSelectMetatype
                               = await ThreadSafeForm<SelectMetatypePriority>.GetAsync(
                                   () => new SelectMetatypePriority(objCharacter)))
                        {
                            if (await frmSelectMetatype.ShowDialogSafeAsync(this) != DialogResult.OK)
                                return;
                        }
                    }
                    else
                    {
                        using (ThreadSafeForm<SelectMetatypeKarma> frmSelectMetatype
                               = await ThreadSafeForm<SelectMetatypeKarma>.GetAsync(
                                   () => new SelectMetatypeKarma(objCharacter)))
                        {
                            if (await frmSelectMetatype.ShowDialogSafeAsync(this) != DialogResult.OK)
                                return;
                        }
                    }

                    await Program.OpenCharacters.AddAsync(objCharacter);
                }
                finally
                {
                    await objCursorWait.DisposeAsync();
                }

                objCursorWait = await CursorWait.NewAsync(this);
                try
                {
                    await this.DoThreadSafeAsync(x =>
                    {
                        CharacterCreate frmNewCharacter = new CharacterCreate(objCharacter)
                        {
                            MdiParent = x
                        };
                        if (MdiChildren.Length <= 1)
                            frmNewCharacter.WindowState = FormWindowState.Maximized;
                        frmNewCharacter.Show();
                        // This weird ordering of WindowState after Show() is meant to counteract a weird WinForms issue where form handle creation crashes
                        if (MdiChildren.Length > 1)
                            frmNewCharacter.WindowState = FormWindowState.Maximized;
                    });
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
                if (await this.DoThreadSafeFuncAsync(x => dlgOpenFile.ShowDialog(x)) != DialogResult.OK)
                    return;
                List<string> lstFilesToOpen = new List<string>(dlgOpenFile.FileNames.Length);
                foreach (string strFile in dlgOpenFile.FileNames)
                {
                    Character objLoopCharacter
                        = await Program.OpenCharacters.FirstOrDefaultAsync(x => x.FileName == strFile);
                    if (objLoopCharacter != null)
                    {
                        if (!await SwitchToOpenCharacter(objLoopCharacter))
                            await OpenCharacter(objLoopCharacter);
                    }
                    else
                        lstFilesToOpen.Add(strFile);
                }

                if (lstFilesToOpen.Count == 0)
                    return;
                // Array instead of concurrent bag because we want to preserve order
                Character[] lstCharacters = new Character[lstFilesToOpen.Count];
                using (ThreadSafeForm<LoadingBar> frmLoadingBar = await Program.CreateAndShowProgressBarAsync(
                           string.Join(',' + await LanguageManager.GetStringAsync("String_Space"),
                                       lstFilesToOpen.Select(Path.GetFileName)),
                           lstFilesToOpen.Count * Character.NumLoadingSections))
                {
                    Task<Character>[] tskCharacterLoads = new Task<Character>[lstFilesToOpen.Count];
                    for (int i = 0; i < lstFilesToOpen.Count; ++i)
                    {
                        string strFile = lstFilesToOpen[i];
                        // ReSharper disable once AccessToDisposedClosure
                        tskCharacterLoads[i]
                            = Task.Run(() => Program.LoadCharacterAsync(strFile, frmLoadingBar: frmLoadingBar.MyForm));
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
        /// Opens the correct window for a single character.
        /// </summary>
        public Task OpenCharacter(Character objCharacter, bool blnIncludeInMru = true, CancellationToken token = default)
        {
            return OpenCharacterList(objCharacter.Yield(), blnIncludeInMru, token);
        }

        /// <summary>
        /// Open the correct windows for a list of characters.
        /// </summary>
        /// <param name="lstCharacters">Characters for which windows should be opened.</param>
        /// <param name="blnIncludeInMru">Added the opened characters to the Most Recently Used list.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task OpenCharacterList(IEnumerable<Character> lstCharacters, bool blnIncludeInMru = true, CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token);
            try
            {
                if (lstCharacters == null)
                    return;
                List<Character> lstNewCharacters = lstCharacters.ToList();
                if (lstNewCharacters.Count == 0)
                    return;
                FormWindowState wsPreference
                    = await this.DoThreadSafeFuncAsync(x => x.MdiChildren.Length == 0
                                                            || x.MdiChildren.Any(
                                                                y => y.WindowState == FormWindowState.Maximized), token)
                        ? FormWindowState.Maximized
                        : FormWindowState.Normal;
                string strUI = await LanguageManager.GetStringAsync("String_UI", token: token);
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token);
                string strTooManyHandles = await LanguageManager.GetStringAsync("Message_TooManyHandlesWarning", token: token);
                string strTooManyHandlesTitle = await LanguageManager.GetStringAsync("Message_TooManyHandlesWarning", token: token);
                using (ThreadSafeForm<LoadingBar> frmLoadingBar = await Program.CreateAndShowProgressBarAsync(strUI, lstNewCharacters.Count, token))
                {
                    foreach (Character objCharacter in lstNewCharacters)
                    {
                        token.ThrowIfCancellationRequested();
                        await frmLoadingBar.MyForm.PerformStepAsync(objCharacter == null
                                                                        ? strUI
                                                                        : strUI + strSpace + '('
                                                                          + objCharacter.CharacterName
                                                                          + ')', token: token);
                        if (objCharacter == null
                            || await OpenCharacterEditorForms.AnyAsync(x => x.CharacterObject == objCharacter, token))
                            continue;
                        if (Program.MyProcess.HandleCount >= (objCharacter.Created ? 8000 : 7500)
                            && Program.ShowMessageBox(
                                string.Format(strTooManyHandles, objCharacter.CharacterName),
                                strTooManyHandlesTitle,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        {
                            if (Program.OpenCharacters.All(
                                    x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter)))
                                await Program.OpenCharacters.RemoveAsync(objCharacter, token);
                            continue;
                        }

                        //Timekeeper.Start("load_event_time");
                        // Show the character forms.
                        await this.DoThreadSafeAsync(y =>
                        {
                            CharacterShared frmNewCharacter = objCharacter.Created
                                ? (CharacterShared) new CharacterCareer(objCharacter)
                                : new CharacterCreate(objCharacter);
                            frmNewCharacter.MdiParent = y;
                            if (y.MdiChildren.Length <= 1)
                                frmNewCharacter.WindowState = wsPreference;
                            frmNewCharacter.Show();
                            // This weird ordering of WindowState after Show() is meant to counteract a weird WinForms issue where form handle creation crashes
                            if (y.MdiChildren.Length > 1)
                                frmNewCharacter.WindowState = wsPreference;
                        }, token);
                        if (blnIncludeInMru && !string.IsNullOrEmpty(objCharacter.FileName)
                                            && File.Exists(objCharacter.FileName))
                            await GlobalSettings.MostRecentlyUsedCharacters.InsertAsync(0, objCharacter.FileName, token);
                        //Timekeeper.Finish("load_event_time");
                    }
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async void OpenFileForPrinting(object sender, EventArgs e)
        {
            if (Utils.IsUnitTest)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                if (await this.DoThreadSafeFuncAsync(x => dlgOpenFile.ShowDialog(x)) != DialogResult.OK)
                    return;
                List<string> lstFilesToOpen = new List<string>(dlgOpenFile.FileNames.Length);
                foreach (string strFile in dlgOpenFile.FileNames)
                {
                    Character objLoopCharacter
                        = await Program.OpenCharacters.FirstOrDefaultAsync(x => x.FileName == strFile);
                    if (objLoopCharacter != null)
                    {
                        if (!await SwitchToOpenPrintCharacter(objLoopCharacter))
                            await OpenCharacterForPrinting(objLoopCharacter);
                    }
                    else
                        lstFilesToOpen.Add(strFile);
                }

                if (lstFilesToOpen.Count == 0)
                    return;
                // Array instead of concurrent bag because we want to preserve order
                Character[] lstCharacters = new Character[lstFilesToOpen.Count];
                using (ThreadSafeForm<LoadingBar> frmLoadingBar = await Program.CreateAndShowProgressBarAsync(
                           string.Join(',' + await LanguageManager.GetStringAsync("String_Space"),
                                       lstFilesToOpen.Select(Path.GetFileName)),
                           lstFilesToOpen.Count * Character.NumLoadingSections))
                {
                    Task<Character>[] tskCharacterLoads = new Task<Character>[lstFilesToOpen.Count];
                    for (int i = 0; i < lstFilesToOpen.Count; ++i)
                    {
                        string strFile = lstFilesToOpen[i];
                        // ReSharper disable once AccessToDisposedClosure
                        tskCharacterLoads[i]
                            = Task.Run(() => Program.LoadCharacterAsync(strFile, frmLoadingBar: frmLoadingBar.MyForm));
                    }

                    await Task.WhenAll(tskCharacterLoads);
                    for (int i = 0; i < lstCharacters.Length; ++i)
                        lstCharacters[i] = await tskCharacterLoads[i];
                }

                await OpenCharacterListForPrinting(lstCharacters);
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        /// <summary>
        /// Open a character's print form up without necessarily opening them up fully for editing.
        /// </summary>
        public Task OpenCharacterForPrinting(Character objCharacter, bool blnIncludeInMru = false, CancellationToken token = default)
        {
            return OpenCharacterListForPrinting(objCharacter.Yield(), blnIncludeInMru, token);
        }

        /// <summary>
        /// Open print forms for a list of characters.
        /// </summary>
        /// <param name="lstCharacters">Characters for which windows should be opened.</param>
        /// <param name="blnIncludeInMru">Added the opened characters to the Most Recently Used list.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task OpenCharacterListForPrinting(IEnumerable<Character> lstCharacters, bool blnIncludeInMru = false, CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token);
            try
            {
                if (lstCharacters == null)
                    return;
                List<Character> lstNewCharacters = lstCharacters.ToList();
                if (lstNewCharacters.Count == 0)
                    return;
                FormWindowState wsPreference
                    = await this.DoThreadSafeFuncAsync(x => x.MdiChildren.Length == 0
                                                            || x.MdiChildren.Any(
                                                                y => y.WindowState == FormWindowState.Maximized), token)
                        ? FormWindowState.Maximized
                        : FormWindowState.Normal;
                List<Tuple<CharacterSheetViewer, Character>> lstNewFormsToProcess = new List<Tuple<CharacterSheetViewer, Character>>(lstNewCharacters.Count);
                string strUI = await LanguageManager.GetStringAsync("String_UI", token: token);
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token);
                string strTooManyHandles = await LanguageManager.GetStringAsync("Message_TooManyHandlesWarning", token: token);
                string strTooManyHandlesTitle = await LanguageManager.GetStringAsync("Message_TooManyHandlesWarning", token: token);
                using (ThreadSafeForm<LoadingBar> frmLoadingBar
                       = await Program.CreateAndShowProgressBarAsync(strUI, lstNewCharacters.Count, token))
                {
                    foreach (Character objCharacter in lstNewCharacters)
                    {
                        token.ThrowIfCancellationRequested();
                        await frmLoadingBar.MyForm.PerformStepAsync(objCharacter == null
                                                                        ? strUI
                                                                        : strUI + strSpace + '(' + objCharacter.CharacterName
                                                                          + ')', token: token);
                        if (objCharacter == null
                            || await OpenCharacterSheetViewers.AnyAsync(x => x.CharacterObjects.Contains(objCharacter), token))
                            continue;

                        if (Program.MyProcess.HandleCount >= 9500
                            && Program.ShowMessageBox(
                                string.Format(strTooManyHandles, objCharacter.CharacterName),
                                strTooManyHandlesTitle,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        {
                            if (Program.OpenCharacters.All(
                                    x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter)))
                                await Program.OpenCharacters.RemoveAsync(objCharacter, token);
                            continue;
                        }

                        //Timekeeper.Start("load_event_time");
                        // Show the character forms.
                        await this.DoThreadSafeAsync(y =>
                        {
                            CharacterSheetViewer frmViewer = new CharacterSheetViewer
                            {
                                MdiParent = y
                            };
                            lstNewFormsToProcess.Add(
                                new Tuple<CharacterSheetViewer, Character>(frmViewer, objCharacter));
                        }, token: token);

                        if (blnIncludeInMru && !string.IsNullOrEmpty(objCharacter.FileName)
                                            && File.Exists(objCharacter.FileName))
                            await GlobalSettings.MostRecentlyUsedCharacters.InsertAsync(0, objCharacter.FileName, token);
                        //Timekeeper.Finish("load_event_time");
                    }
                }

                // This weird ordering of WindowState after Show() is meant to counteract a weird WinForms issue where form handle creation crashes
                foreach ((CharacterSheetViewer frmViewer, Character objCharacter) in lstNewFormsToProcess)
                {
                    await frmViewer.SetCharacters(token, objCharacter);
                    if (await this.DoThreadSafeFuncAsync(y => y.MdiChildren.Length, token) <= 1)
                    {
                        await frmViewer.DoThreadSafeAsync(x =>
                        {
                            x.WindowState = wsPreference;
                            // This weird ordering of WindowState after Show() is meant to counteract a weird WinForms issue where form handle creation crashes
                            x.Show();
                        }, token);
                    }
                    else
                    {
                        await frmViewer.DoThreadSafeAsync(x =>
                        {
                            x.Show();
                            x.WindowState = wsPreference;
                        }, token);
                    }
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async void OpenFileForExport(object sender, EventArgs e)
        {
            if (Utils.IsUnitTest)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                if (await this.DoThreadSafeFuncAsync(x => dlgOpenFile.ShowDialog(x)) != DialogResult.OK)
                    return;
                List<string> lstFilesToOpen = new List<string>(dlgOpenFile.FileNames.Length);
                foreach (string strFile in dlgOpenFile.FileNames)
                {
                    Character objLoopCharacter
                        = await Program.OpenCharacters.FirstOrDefaultAsync(x => x.FileName == strFile);
                    if (objLoopCharacter != null)
                    {
                        if (!await SwitchToOpenExportCharacter(objLoopCharacter))
                            await OpenCharacterForExport(objLoopCharacter);
                    }
                    else
                        lstFilesToOpen.Add(strFile);
                }

                if (lstFilesToOpen.Count == 0)
                    return;
                // Array instead of concurrent bag because we want to preserve order
                Character[] lstCharacters = new Character[lstFilesToOpen.Count];
                using (ThreadSafeForm<LoadingBar> frmLoadingBar = await Program.CreateAndShowProgressBarAsync(
                           string.Join(',' + await LanguageManager.GetStringAsync("String_Space"),
                                       lstFilesToOpen.Select(Path.GetFileName)),
                           lstFilesToOpen.Count * Character.NumLoadingSections))
                {
                    Task<Character>[] tskCharacterLoads = new Task<Character>[lstFilesToOpen.Count];
                    for (int i = 0; i < lstFilesToOpen.Count; ++i)
                    {
                        string strFile = lstFilesToOpen[i];
                        // ReSharper disable once AccessToDisposedClosure
                        tskCharacterLoads[i]
                            = Task.Run(() => Program.LoadCharacterAsync(strFile, frmLoadingBar: frmLoadingBar.MyForm));
                    }

                    await Task.WhenAll(tskCharacterLoads);
                    for (int i = 0; i < lstCharacters.Length; ++i)
                        lstCharacters[i] = await tskCharacterLoads[i];
                }

                await OpenCharacterListForExport(lstCharacters);
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        /// <summary>
        /// Open a character's export form up without necessarily opening them up fully for editing.
        /// </summary>
        public Task OpenCharacterForExport(Character objCharacter, bool blnIncludeInMru = false, CancellationToken token = default)
        {
            return OpenCharacterListForExport(objCharacter.Yield(), blnIncludeInMru, token);
        }

        /// <summary>
        /// Open export forms for a list of characters.
        /// </summary>
        /// <param name="lstCharacters">Characters for which windows should be opened.</param>
        /// <param name="blnIncludeInMru">Added the opened characters to the Most Recently Used list.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task OpenCharacterListForExport(IEnumerable<Character> lstCharacters, bool blnIncludeInMru = false, CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token);
            try
            {
                if (lstCharacters == null)
                    return;
                List<Character> lstNewCharacters = lstCharacters.ToList();
                if (lstNewCharacters.Count == 0)
                    return;
                FormWindowState wsPreference
                    = await this.DoThreadSafeFuncAsync(x => x.MdiChildren.Length == 0
                                                            || x.MdiChildren.Any(
                                                                y => y.WindowState == FormWindowState.Maximized), token)
                        ? FormWindowState.Maximized
                        : FormWindowState.Normal;
                string strUI = await LanguageManager.GetStringAsync("String_UI", token: token);
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token);
                string strTooManyHandles = await LanguageManager.GetStringAsync("Message_TooManyHandlesWarning", token: token);
                string strTooManyHandlesTitle = await LanguageManager.GetStringAsync("Message_TooManyHandlesWarning", token: token);
                using (ThreadSafeForm<LoadingBar> frmLoadingBar
                       = await Program.CreateAndShowProgressBarAsync(strUI, lstNewCharacters.Count, token))
                {
                    foreach (Character objCharacter in lstNewCharacters)
                    {
                        await frmLoadingBar.MyForm.PerformStepAsync(objCharacter == null
                                                                        ? strUI
                                                                        : strUI + strSpace + '(' + objCharacter.CharacterName
                                                                          + ')', token: token);
                        if (objCharacter == null
                            || await OpenCharacterExportForms.AnyAsync(x => x.CharacterObject == objCharacter, token))
                            continue;
                        if (Program.MyProcess.HandleCount >= 9500
                            && Program.ShowMessageBox(
                                string.Format(strTooManyHandles, objCharacter.CharacterName),
                                strTooManyHandlesTitle,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        {
                            if (Program.OpenCharacters.All(
                                    x => x == objCharacter || !x.LinkedCharacters.Contains(objCharacter)))
                                await Program.OpenCharacters.RemoveAsync(objCharacter, token);
                            continue;
                        }

                        //Timekeeper.Start("load_event_time");
                        // Show the character forms.
                        await this.DoThreadSafeAsync(y =>
                        {
                            ExportCharacter frmViewer = new ExportCharacter(objCharacter)
                            {
                                MdiParent = y
                            };
                            if (y.MdiChildren.Length <= 1)
                                frmViewer.WindowState = wsPreference;
                            frmViewer.Show();
                            // This weird ordering of WindowState after Show() is meant to counteract a weird WinForms issue where form handle creation crashes
                            if (y.MdiChildren.Length > 1)
                                frmViewer.WindowState = wsPreference;
                        }, token);
                        if (blnIncludeInMru && !string.IsNullOrEmpty(objCharacter.FileName)
                                            && File.Exists(objCharacter.FileName))
                            await GlobalSettings.MostRecentlyUsedCharacters.InsertAsync(0, objCharacter.FileName, token);
                        //Timekeeper.Finish("load_event_time");
                    }
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
            await DoPopulateMruToolstripMenu(e?.Text);
        }

        private async ValueTask DoPopulateMruToolstripMenu(string strText = "", CancellationToken token = default)
        {
            await menuStrip.DoThreadSafeAsync(x => x.SuspendLayout(), token);
            try
            {
                await menuStrip.DoThreadSafeAsync(() => mnuFileMRUSeparator.Visible = GlobalSettings.FavoriteCharacters.Count > 0
                                                      || GlobalSettings.MostRecentlyUsedCharacters
                                                                       .Count > 0, token);

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
                            await menuStrip.DoThreadSafeAsync(() =>
                            {
                                objItem.Text = GlobalSettings.FavoriteCharacters[i];
                                objItem.Tag = GlobalSettings.FavoriteCharacters[i];
                                objItem.Visible = true;
                            }, token);
                        }
                        else
                        {
                            await menuStrip.DoThreadSafeAsync(() => objItem.Visible = false, token);
                        }
                    }
                }

                await menuStrip.DoThreadSafeAsync(() =>
                {
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
                }, token);

                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token);
                int i2 = 0;
                for (int i = 0; i < GlobalSettings.MaxMruSize; ++i)
                {
                    if (i2 >= GlobalSettings.MostRecentlyUsedCharacters.Count ||
                        i >= GlobalSettings.MostRecentlyUsedCharacters.Count)
                        continue;
                    string strFile = GlobalSettings.MostRecentlyUsedCharacters[i];
                    if (await GlobalSettings.FavoriteCharacters.ContainsAsync(strFile, token))
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
                        await menuStrip.DoThreadSafeAsync(() => objItem.Text = strNumAsString.Insert(strNumAsString.Length - 1, "&") + strSpace + strFile, token);
                    }
                    else
                        await menuStrip.DoThreadSafeAsync(() => objItem.Text = (i2 + 1).ToString(GlobalSettings.CultureInfo) + strSpace + strFile, token);

                    await menuStrip.DoThreadSafeAsync(() =>
                    {
                        objItem.Tag = strFile;
                        objItem.Visible = true;
                    }, token);
                    ++i2;
                }
            }
            finally
            {
                await menuStrip.DoThreadSafeAsync(x => x.ResumeLayout(), token);
            }
        }

        public async ValueTask OpenDiceRollerWithPool(Character objCharacter = null, int intDice = 0, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (GlobalSettings.SingleDiceRoller)
            {
                if (RollerWindow == null)
                {
                    RollerWindow = await this.DoThreadSafeFuncAsync(x =>
                    {
                        DiceRoller objReturn = new DiceRoller(this, objCharacter?.Qualities, intDice);
                        x.Disposed += (o, args) => objReturn.Dispose();
                        return objReturn;
                    }, token);
                    await RollerWindow.DoThreadSafeAsync(x => x.Show(), token);
                }
                else
                {
                    await RollerWindow.DoThreadSafeAsync(x =>
                    {
                        x.Dice = intDice;
                        x.ProcessGremlins(objCharacter?.Qualities);
                        x.Activate();
                    }, token);
                }
            }
            else
            {
                DiceRoller frmRoller = await this.DoThreadSafeFuncAsync(() => new DiceRoller(this, objCharacter?.Qualities, intDice), token);
                await frmRoller.DoThreadSafeAsync(x => x.Show(), token);
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
                List<Character> lstCharactersToLoad = new List<Character>(1);

                using (CursorWait.New(this, true))
                using (ThreadSafeForm<LoadingBar> frmLoadingBar = Program.CreateAndShowProgressBar())
                {
                    Task objCharacterLoadingTask = null;
                    Task<Character>[] atskLoadingTasks = null;
                    // Extract the file name
                    NativeMethods.CopyDataStruct objReceivedData
                        = (NativeMethods.CopyDataStruct) Marshal.PtrToStructure(
                            m.LParam, typeof(NativeMethods.CopyDataStruct));
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
                                             "*.chum5", SearchOption.AllDirectories).Concat(Directory.EnumerateFiles(
                                             Utils.GetAutosavesFolderPath,
                                             "*.chum5lz", SearchOption.AllDirectories)))
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
                                frmLoadingBar.MyForm.Reset(setFilesToLoad.Count * Character.NumLoadingSections);
                                atskLoadingTasks = new Task<Character>[setFilesToLoad.Count];
                                for (int i = 0; i < setFilesToLoad.Count; ++i)
                                {
                                    string strFile = setFilesToLoad.ElementAt(i);
                                    // ReSharper disable once AccessToDisposedClosure
                                    atskLoadingTasks[i] = Task.Run(() => Program.LoadCharacterAsync(strFile, frmLoadingBar: frmLoadingBar.MyForm));
                                }

                                objCharacterLoadingTask = Task.WhenAll(atskLoadingTasks);
                            }
                        }
                        finally
                        {
                            Utils.StringHashSetPool.Return(setFilesToLoad);
                        }

                        if (blnShowTest)
                        {
                            TestDataEntries frmTestData = new TestDataEntries();
                            frmTestData.Show();
                        }
                    }

                    Utils.RunWithoutThreadLock(async () =>
                    {
                        if (objCharacterLoadingTask?.IsCompleted == false)
                            await objCharacterLoadingTask;
                        if (atskLoadingTasks != null)
                        {
                            foreach (Task<Character> tskLoadingTask in atskLoadingTasks)
                                lstCharactersToLoad.Add(await tskLoadingTask);
                        }
                    });
                }

                if (lstCharactersToLoad.Count > 0)
                    Utils.RunWithoutThreadLock(() => OpenCharacterList(lstCharactersToLoad));
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
                    if (strArg.EndsWith(Path.GetFileName(Application.ExecutablePath), StringComparison.OrdinalIgnoreCase))
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

                                    string strExtension = Path.GetExtension(strArg);
                                    if (!string.Equals(strExtension, ".chum5", StringComparison.OrdinalIgnoreCase)
                                        && !string.Equals(strExtension, ".chum5lz", StringComparison.OrdinalIgnoreCase))
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
        public DiceRoller RollerWindow { get; set; }

        public ThreadSafeObservableCollection<CharacterShared> OpenCharacterEditorForms => _lstOpenCharacterEditorForms;

        public ThreadSafeObservableCollection<CharacterSheetViewer> OpenCharacterSheetViewers =>
            _lstOpenCharacterSheetViewers;

        public ThreadSafeObservableCollection<ExportCharacter> OpenCharacterExportForms =>
            _lstOpenCharacterExportForms;

        public IEnumerable<IHasCharacterObjects> OpenFormsWithCharacters
        {
            get
            {
                foreach (CharacterShared frmLoop in _lstOpenCharacterEditorForms)
                    yield return frmLoop;
                foreach (CharacterSheetViewer frmLoop in _lstOpenCharacterSheetViewers)
                    yield return frmLoop;
                foreach (ExportCharacter frmLoop in _lstOpenCharacterExportForms)
                    yield return frmLoop;
            }
        }

        /// <summary>
        /// Set to True at the end of the OnLoad method. Useful because the load method is executed asynchronously, so form might end up getting closed before it fully loads.
        /// </summary>
        public bool IsFinishedLoading { get; private set; }

#endregion Application Properties
    }
}
