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
using System.Text.RegularExpressions;
using Chummer.Backend.Equipment;
using Application = System.Windows.Forms.Application;
using DataFormats = System.Windows.Forms.DataFormats;
using DragDropEffects = System.Windows.Forms.DragDropEffects;
using DragEventArgs = System.Windows.Forms.DragEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;
using Path = System.IO.Path;
using Size = System.Drawing.Size;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Net;
using System.Text;
using Chummer.Plugins;
using System.IO.Compression;

namespace Chummer
{
    public sealed partial class frmChummerMain : Form
    {
#if LEGACY
        private frmOmae _frmOmae;
#endif
        private frmDiceRoller _frmRoller;
        private frmUpdate _frmUpdate;
        private readonly ObservableCollection<Character> _lstCharacters = new ObservableCollection<Character>();
        private readonly ObservableCollection<CharacterShared> _lstOpenCharacterForms = new ObservableCollection<CharacterShared>();
        private readonly BackgroundWorker _workerVersionUpdateChecker = new BackgroundWorker();
        private readonly Version _objCurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        private readonly string _strCurrentVersion;
        public readonly PluginControl PluginLoader = new PluginControl();
        private readonly Chummy _mascotChummy;


        #region Control Events
        public frmChummerMain(bool isUnitTest = false)
        {
            Utils.IsUnitTest = isUnitTest;
            InitializeComponent();




            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            _strCurrentVersion = $"{_objCurrentVersion.Major}.{_objCurrentVersion.Minor}.{_objCurrentVersion.Build}";
            Text = Application.ProductName + strSpaceCharacter + '-' + strSpaceCharacter + LanguageManager.GetString("String_Version", GlobalOptions.Language) + strSpaceCharacter + _strCurrentVersion;
#if DEBUG
            Text += " DEBUG BUILD";
#endif

            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            /** Dashboard **/
            //this.toolsMenu.DropDownItems.Add("GM Dashboard").Click += this.dashboardToolStripMenuItem_Click;
            /** End Dashboard **/

            // If Automatic Updates are enabled, check for updates immediately.

#if !DEBUG
            _workerVersionUpdateChecker.WorkerReportsProgress = false;
            _workerVersionUpdateChecker.WorkerSupportsCancellation = true;
            _workerVersionUpdateChecker.DoWork += DoCacheGitVersion;
            _workerVersionUpdateChecker.RunWorkerCompleted += CheckForUpdate;
            Application.Idle += IdleUpdateCheck;
            _workerVersionUpdateChecker.RunWorkerAsync();
#endif

            GlobalOptions.MRUChanged += PopulateMRUToolstripMenu;

            // Delete the old executable if it exists (created by the update process).
            foreach(string strLoopOldFilePath in Directory.GetFiles(Utils.GetStartupPath, "*.old", SearchOption.AllDirectories))
            {
                if(File.Exists(strLoopOldFilePath))
                    File.Delete(strLoopOldFilePath);
            }

            // Populate the MRU list.
            PopulateMRUToolstripMenu(this, null);

            Program.MainForm = this;
            PluginLoader.LoadPlugins();
            if (GlobalOptions.AllowEasterEggs)
            {
                _mascotChummy = new Chummy();
                _mascotChummy.Show(this);
            }

            // Set the Tag for each ToolStrip item so it can be translated.
            foreach(ToolStripMenuItem objItem in menuStrip.Items.OfType<ToolStripMenuItem>())
            {
                LanguageManager.TranslateToolStripItemsRecursively(objItem, GlobalOptions.Language);
            }

            frmLoading frmLoadingForm = new frmLoading { CharacterFile = Text };
            frmLoadingForm.Reset(3);
            frmLoadingForm.Show();

            // Attempt to cache all XML files that are used the most.
            Timekeeper.Start("cache_load");
            Parallel.Invoke(
                () => XmlManager.Load("armor.xml"),
                () => XmlManager.Load("bioware.xml"),
                () => XmlManager.Load("books.xml"),
                () => XmlManager.Load("complexforms.xml"),
                () => XmlManager.Load("contacts.xml"),
                () => XmlManager.Load("critters.xml"),
                () => XmlManager.Load("critterpowers.xml"),
                () => XmlManager.Load("cyberware.xml"),
                () => XmlManager.Load("drugcomponents.xml"),
                () => XmlManager.Load("echoes.xml"),
                () => XmlManager.Load("gameplayoptions.xml"),
                () => XmlManager.Load("gear.xml"),
                () => XmlManager.Load("improvements.xml"),
                () => XmlManager.Load("licenses.xml"),
                () => XmlManager.Load("lifemodules.xml"),
                () => XmlManager.Load("lifestyles.xml"),
                () => XmlManager.Load("martialarts.xml"),
                () => XmlManager.Load("mentors.xml"),
                () => XmlManager.Load("metamagic.xml"),
                () => XmlManager.Load("metatypes.xml"),
                () => XmlManager.Load("options.xml"),
                () => XmlManager.Load("packs.xml"),
                () => XmlManager.Load("powers.xml"),
                () => XmlManager.Load("priorities.xml"),
                () => XmlManager.Load("programs.xml"),
                () => XmlManager.Load("qualities.xml"),
                () => XmlManager.Load("ranges.xml"),
                () => XmlManager.Load("sheets.xml"),
                () => XmlManager.Load("skills.xml"),
                () => XmlManager.Load("spells.xml"),
                () => XmlManager.Load("spiritpowers.xml"),
                () => XmlManager.Load("streams.xml"),
                () => XmlManager.Load("traditions.xml"),
                () => XmlManager.Load("vehicles.xml"),
                () => XmlManager.Load("weapons.xml")
            );
            Timekeeper.Finish("cache_load");
            frmLoadingForm.PerformStep(LanguageManager.GetString("String_UI"));
            CharacterRoster = GlobalOptions.HideCharacterRoster
                ? null
                : new frmCharacterRoster
                {
                    MdiParent = this
                };

            _lstCharacters.CollectionChanged += LstCharactersOnCollectionChanged;
            _lstOpenCharacterForms.CollectionChanged += LstOpenCharacterFormsOnCollectionChanged;

            frmLoadingForm.PerformStep(LanguageManager.GetString("String_UI"));
            // Retrieve the arguments passed to the application. If more than 1 is passed, we're being given the name of a file to open.
            string[] strArgs = Environment.GetCommandLineArgs();
            ConcurrentBag<Character> lstCharactersToLoad = new ConcurrentBag<Character>();
            bool blnShowTest = false;
            object blnShowTestLock = new object();
            if(!Utils.IsUnitTest)
            {
                Parallel.For(1, strArgs.Length, i =>
                {
                    if (strArgs[i] == "/test")
                    {
                        lock(blnShowTestLock)
                            blnShowTest = true;
                    }
                    else if(!strArgs[i].StartsWith('/'))
                    {
                        if(!File.Exists(strArgs[i]))
                        {
                            throw new ArgumentException("Chummer started with unknown command line arguments: " + strArgs.Aggregate((j, k) => j + " " + k));
                        }

                        if (lstCharactersToLoad.Any(x => x.FileName == strArgs[i])) return;
                        Character objLoopCharacter = LoadCharacter(strArgs[i]);
                        lstCharactersToLoad.Add(objLoopCharacter);
                    }
                });
            }
            frmLoadingForm.PerformStep(LanguageManager.GetString("String_UI"));
            if(blnShowTest)
            {
                frmTest frmTestData = new frmTest();
                frmTestData.Show();
            }
            OpenCharacterList(lstCharactersToLoad);
            if(!GlobalOptions.HideCharacterRoster)
            {
                CharacterRoster.WindowState = FormWindowState.Maximized;
                CharacterRoster.Show();
            }
            PluginLoader.CallPlugins(toolsMenu);
            frmLoadingForm.Close();
        }

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
                            objCharacter.PropertyChanged -= UpdateCharacterTabTitle;
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach(Character objCharacter in notifyCollectionChangedEventArgs.OldItems)
                            objCharacter.PropertyChanged -= UpdateCharacterTabTitle;
                        foreach(Character objCharacter in notifyCollectionChangedEventArgs.NewItems)
                            objCharacter.PropertyChanged += UpdateCharacterTabTitle;
                        break;
                    }
            }
        }

        public frmCharacterRoster CharacterRoster { get; }

        private void DoCacheGitVersion(object sender, DoWorkEventArgs e)
        {
            string strUpdateLocation = GlobalOptions.PreferNightlyBuilds
                ? "https://api.github.com/repos/chummer5a/chummer5a/releases"
                : "https://api.github.com/repos/chummer5a/chummer5a/releases/latest";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest request;
            try
            {
                WebRequest objTemp = WebRequest.Create(strUpdateLocation);
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

            // Get the response.
            HttpWebResponse response;
            try
            {
                response = request.GetResponse() as HttpWebResponse;
            }
            catch(WebException ex)
            {
                Utils.CachedGitVersion = null;
                Log.Error(ex);
                return;
            }

            if(response == null)
            {
                Utils.CachedGitVersion = null;
                return;
            }

            if(_workerVersionUpdateChecker.CancellationPending)
            {
                e.Cancel = true;
                response.Close();
                return;
            }

            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            if(dataStream == null)
            {
                response.Close();
                Utils.CachedGitVersion = null;
                return;
            }

            if(_workerVersionUpdateChecker.CancellationPending)
            {
                e.Cancel = true;
                dataStream.Close();
                response.Close();
                return;
            }

            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream, Encoding.UTF8, true);

            if(_workerVersionUpdateChecker.CancellationPending)
            {
                e.Cancel = true;
                reader.Close();
                response.Close();
                return;
            }

            // Read the content.
            string responseFromServer = reader.ReadToEnd();

            if(_workerVersionUpdateChecker.CancellationPending)
            {
                e.Cancel = true;
                reader.Close();
                response.Close();
                return;
            }

            string[] stringSeparators = { "," };
            string[] result = responseFromServer.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            if(_workerVersionUpdateChecker.CancellationPending)
            {
                e.Cancel = true;
                reader.Close();
                response.Close();
                return;
            }

            string line = result.FirstOrDefault(x => x.Contains("tag_name"));

            if(_workerVersionUpdateChecker.CancellationPending)
            {
                e.Cancel = true;
                reader.Close();
                response.Close();
                return;
            }

            Version verLatestVersion = null;
            if(!string.IsNullOrEmpty(line))
            {
                string strVersion = line.Substring(line.IndexOf(':') + 1);
                int intPos = strVersion.IndexOf('}');
                if(intPos != -1)
                    strVersion = strVersion.Substring(0, intPos);
                strVersion = strVersion.FastEscape('\"');

                if(_workerVersionUpdateChecker.CancellationPending)
                {
                    e.Cancel = true;
                    reader.Close();
                    response.Close();
                    return;
                }

                // Adds zeroes if minor and/or build version are missing
                while(strVersion.Count(x => x == '.') < 2)
                {
                    strVersion = strVersion + ".0";
                }
                Version.TryParse(strVersion.TrimStartOnce("Nightly-v"), out verLatestVersion);
            }
            // Cleanup the streams and the response.
            reader.Close();
            response.Close();

            Utils.CachedGitVersion = verLatestVersion;
        }

        private void CheckForUpdate(object sender, RunWorkerCompletedEventArgs e)
        {
            if(!e.Cancelled && Utils.GitUpdateAvailable() > 0)
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
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                Text = Application.ProductName + strSpaceCharacter + '-' + strSpaceCharacter +
                       LanguageManager.GetString("String_Version", GlobalOptions.Language) + strSpaceCharacter + _strCurrentVersion + strSpaceCharacter + '-' + strSpaceCharacter +
                       string.Format(LanguageManager.GetString("String_Update_Available", GlobalOptions.Language), Utils.CachedGitVersion);
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
                if(childForm != CharacterRoster)
                    childForm.Close();
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor objOldCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            frmOptions frmOptions = new frmOptions();
            frmOptions.ShowDialog(this);
            Cursor = objOldCursor;
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
            frmAbout frmShowAbout = new frmAbout();
            frmShowAbout.ShowDialog(this);
        }

        private void mnuChummerWiki_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.chummergen.com/chummer/wiki/");
        }

        private void mnuChummerDiscord_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/mJB7st9");
        }

        private void mnuHelpDumpshock_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/chummer5a/chummer5a/issues/");
        }

        private frmPrintMultiple _frmPrintMultipleCharacters;

        public frmPrintMultiple PrintMultipleCharactersForm => _frmPrintMultipleCharacters;

        private void mnuFilePrintMultiple_Click(object sender, EventArgs e)
        {
            if(_frmPrintMultipleCharacters == null)
                _frmPrintMultipleCharacters = new frmPrintMultiple();
            else
                _frmPrintMultipleCharacters.Activate();
            _frmPrintMultipleCharacters.Show(this);
        }

        private void mnuHelpRevisionHistory_Click(object sender, EventArgs e)
        {
            frmHistory frmShowHistory = new frmHistory();
            frmShowHistory.ShowDialog(this);
        }

        private void mnuNewCritter_Click(object sender, EventArgs e)
        {
            Character objCharacter = new Character();
            string settingsPath = Path.Combine(Utils.GetStartupPath, "settings");
            string[] settingsFiles = Directory.GetFiles(settingsPath, "*.xml");

            Cursor objOldCursor = Cursor;
            if(settingsFiles.Length > 1)
            {
                Cursor = Cursors.WaitCursor;
                frmSelectSetting frmPickSetting = new frmSelectSetting();
                frmPickSetting.ShowDialog(this);
                Cursor = objOldCursor;

                if(frmPickSetting.DialogResult == DialogResult.Cancel)
                    return;

                objCharacter.SettingsFile = frmPickSetting.SettingsFile;
            }
            else
            {
                string strSettingsFile = settingsFiles[0];
                objCharacter.SettingsFile = Path.GetFileName(strSettingsFile);
            }

            Cursor = Cursors.WaitCursor;

            // Override the defaults for the setting.
            objCharacter.IgnoreRules = true;
            objCharacter.IsCritter = true;
            objCharacter.Created = true;
            objCharacter.BuildMethod = CharacterBuildMethod.Karma;

            // Show the Metatype selection window.
            frmKarmaMetatype frmSelectMetatype = new frmKarmaMetatype(objCharacter, "critters.xml");
            frmSelectMetatype.ShowDialog();
            Cursor = objOldCursor;

            if(frmSelectMetatype.DialogResult == DialogResult.Cancel)
                return;
            objOldCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            // Add the Unarmed Attack Weapon to the character.
            XmlNode objXmlWeapon = XmlManager.Load("weapons.xml").SelectSingleNode("/chummer/weapons/weapon[name = \"Unarmed Attack\"]");
            if(objXmlWeapon != null)
            {
                List<Weapon> lstWeapons = new List<Weapon>();
                Weapon objWeapon = new Weapon(objCharacter);
                objWeapon.Create(objXmlWeapon, lstWeapons);
                objWeapon.ParentID = Guid.NewGuid().ToString("D"); // Unarmed Attack can never be removed
                objCharacter.Weapons.Add(objWeapon);
                foreach(Weapon objLoopWeapon in lstWeapons)
                    objCharacter.Weapons.Add(objLoopWeapon);
            }

            frmCareer frmNewCharacter = new frmCareer(objCharacter)
            {
                MdiParent = this,
                WindowState = FormWindowState.Maximized
            };
            frmNewCharacter.Show();

            Cursor = objOldCursor;
        }

        private void mnuMRU_Click(object sender, EventArgs e)
        {
            string strFileName = ((ToolStripMenuItem)sender).Text;
            strFileName = strFileName.Substring(3, strFileName.Length - 3).Trim();
            Cursor objOldCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            Character objOpenCharacter = LoadCharacter(strFileName);
            Cursor = objOldCursor;
            Program.MainForm.OpenCharacter(objOpenCharacter);
        }

        private void mnuMRU_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                string strFileName = ((ToolStripMenuItem)sender).Text;
                strFileName = strFileName.Substring(3, strFileName.Length - 3).Trim();

                GlobalOptions.FavoritedCharacters.Add(strFileName);
            }
        }

        private void mnuStickyMRU_Click(object sender, EventArgs e)
        {
            string strFileName = ((ToolStripMenuItem)sender).Text;
            Cursor objOldCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            Character objOpenCharacter = LoadCharacter(strFileName);
            Cursor = objOldCursor;
            Program.MainForm.OpenCharacter(objOpenCharacter);
        }

        private void mnuStickyMRU_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                string strFileName = ((ToolStripMenuItem)sender).Text;

                GlobalOptions.FavoritedCharacters.Remove(strFileName);
                GlobalOptions.MostRecentlyUsedCharacters.Insert(0, strFileName);
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
                if(!(ActiveMdiChild.Tag is TabPage))
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
                        _mascotChummy.CharacterObject = frmCharacterShared.CharacterObject;
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
                            _mascotChummy.CharacterObject = objCharacter;
                            return true;
                        }
                    }
                }
                if(OpenCharacters.Contains(objCharacter))
                {
                    OpenCharacter(objCharacter, blnIncludeInMRU);
                    return true;
                }
            }
            return false;
        }

        public void UpdateCharacterTabTitle(object sender, PropertyChangedEventArgs e)
        {
            // Change the TabPage's text to match the character's name (or "Unnamed Character" if they are currently unnamed).
            if(tabForms.TabCount > 0 && e.PropertyName == nameof(Character.CharacterName) && sender is Character objCharacter)
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

        private void mnuToolsOmae_Click(object sender, EventArgs e)
        {
#if LEGACY
            // Only a single instance of Omae can be open, so either find the current instance and focus on it, or create a new one.
            if (_frmOmae == null)
            {
                _frmOmae = new frmOmae(this);
                _frmOmae.Show();
            }
            else
            {
                _frmOmae.Focus();
            }
#endif
        }

        private void menuStrip_ItemAdded(object sender, ToolStripItemEventArgs e)
        {
            // Translate the items in the menu by finding their Tags in the translation file.
            foreach(ToolStripItem objItem in menuStrip.Items.OfType<ToolStripItem>())
            {
                LanguageManager.TranslateToolStripItemsRecursively(objItem, GlobalOptions.Language);
            }
        }

        private void toolStrip_ItemAdded(object sender, ToolStripItemEventArgs e)
        {
            // ToolStrip Items.
            foreach(ToolStrip objToolStrip in Controls.OfType<ToolStrip>())
            {
                foreach(ToolStripItem objItem in objToolStrip.Items.OfType<ToolStripItem>())
                {
                    LanguageManager.TranslateToolStripItemsRecursively(objItem, GlobalOptions.Language);
                }
            }
        }

        private void toolStrip_ItemRemoved(object sender, ToolStripItemEventArgs e)
        {
            // ToolStrip Items.
            foreach(ToolStrip objToolStrip in Controls.OfType<ToolStrip>())
            {
                foreach(ToolStripItem objItem in objToolStrip.Items.OfType<ToolStripItem>())
                {
                    LanguageManager.TranslateToolStripItemsRecursively(objItem, GlobalOptions.Language);
                }
            }
        }

        private void frmChummerMain_Load(object sender, EventArgs e)
        {
            if(Properties.Settings.Default.Size.Width == 0 || Properties.Settings.Default.Size.Height == 0 || !IsVisibleOnAnyScreen())
            {
                Size = new Size(1280, 720);
                StartPosition = FormStartPosition.CenterScreen;
            }
            else
            {
                WindowState = Properties.Settings.Default.WindowState;

                if(WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;

                Location = Properties.Settings.Default.Location;
                Size = Properties.Settings.Default.Size;
            }

            if(GlobalOptions.StartupFullscreen)
                WindowState = FormWindowState.Maximized;

            mnuToolsOmae.Visible = GlobalOptions.OmaeEnabled;

            //        if (GlobalOptions.UseLogging)
            //        {
            //CommonFunctions objFunctions = new CommonFunctions();
            //        }
        }

        private static bool IsVisibleOnAnyScreen()
        {
            return Screen.AllScreens.Any(screen => screen.WorkingArea.Contains(Properties.Settings.Default.Location));
        }

        private void frmChummerMain_DragDrop(object sender, DragEventArgs e)
        {
            Cursor objOldCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            // Open each file that has been dropped into the window.
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            Character[] lstCharacters = new Character[s.Length];
            object lstCharactersLock = new object();
            Parallel.For(0, s.Length, i =>
            {
                Character objLoopCharacter = LoadCharacter(s[i]);
                lock(lstCharactersLock)
                    lstCharacters[i] = objLoopCharacter;
            });
            Cursor = objOldCursor;
            Program.MainForm.OpenCharacterList(lstCharacters);
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
        #endregion

        #region Methods
        /// <summary>
        /// Create a new character and show the Create Form.
        /// </summary>
        private void ShowNewForm(object sender, EventArgs e)
        {
            string strFilePath = Path.Combine(Utils.GetStartupPath, "settings", "default.xml");
            Cursor objOldCursor = Cursor;
            if(!File.Exists(strFilePath))
            {
                if(MessageBox.Show(LanguageManager.GetString("Message_CharacterOptions_OpenOptions", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CharacterOptions_OpenOptions", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Cursor = Cursors.WaitCursor;
                    frmOptions frmOptions = new frmOptions();
                    frmOptions.ShowDialog();
                    Cursor = objOldCursor;
                }
            }
            Cursor = Cursors.WaitCursor;
            Character objCharacter = new Character();
            string settingsPath = Path.Combine(Utils.GetStartupPath, "settings");
            string[] settingsFiles = Directory.GetFiles(settingsPath, "*.xml");

            if(settingsFiles.Length > 1)
            {
                frmSelectSetting frmPickSetting = new frmSelectSetting();
                frmPickSetting.ShowDialog(this);

                if(frmPickSetting.DialogResult == DialogResult.Cancel)
                    return;

                objCharacter.SettingsFile = frmPickSetting.SettingsFile;
            }
            else
            {
                string strSettingsFile = settingsFiles[0];
                objCharacter.SettingsFile = Path.GetFileName(strSettingsFile);
            }

            // Show the BP selection window.
            frmSelectBuildMethod frmBP = new frmSelectBuildMethod(objCharacter);
            frmBP.ShowDialog();
            Cursor = objOldCursor;

            if(frmBP.DialogResult == DialogResult.Cancel)
                return;
            if(objCharacter.BuildMethod == CharacterBuildMethod.Karma || objCharacter.BuildMethod == CharacterBuildMethod.LifeModule)
            {
                objOldCursor = Cursor;
                Cursor = Cursors.WaitCursor;
                frmKarmaMetatype frmSelectMetatype = new frmKarmaMetatype(objCharacter);
                frmSelectMetatype.ShowDialog();
                Cursor = objOldCursor;

                if(frmSelectMetatype.DialogResult == DialogResult.Cancel)
                { return; }
            }
            // Show the Metatype selection window.
            else if(objCharacter.BuildMethod == CharacterBuildMethod.Priority || objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                objOldCursor = Cursor;
                Cursor = Cursors.WaitCursor;
                frmPriorityMetatype frmSelectMetatype = new frmPriorityMetatype(objCharacter);
                frmSelectMetatype.ShowDialog();
                Cursor = objOldCursor;

                if(frmSelectMetatype.DialogResult == DialogResult.Cancel)
                { return; }
            }
            objOldCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            // Add the Unarmed Attack Weapon to the character.
            XmlNode objXmlWeapon = XmlManager.Load("weapons.xml").SelectSingleNode("/chummer/weapons/weapon[name = \"Unarmed Attack\"]");
            if(objXmlWeapon != null)
            {
                List<Weapon> lstWeapons = new List<Weapon>();
                Weapon objWeapon = new Weapon(objCharacter);
                objWeapon.Create(objXmlWeapon, lstWeapons);
                objWeapon.ParentID = Guid.NewGuid().ToString("D"); // Unarmed Attack can never be removed
                objCharacter.Weapons.Add(objWeapon);
                foreach(Weapon objLoopWeapon in lstWeapons)
                    objCharacter.Weapons.Add(objLoopWeapon);
            }

            OpenCharacters.Add(objCharacter);
            frmCreate frmNewCharacter = new frmCreate(objCharacter)
            {
                MdiParent = this,
                WindowState = FormWindowState.Maximized
            };
            frmNewCharacter.Show();

            Cursor = objOldCursor;
        }

        /// <summary>
        /// Show the Open File dialogue, then load the selected character.
        /// </summary>
        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Chum5", GlobalOptions.Language) + '|' + LanguageManager.GetString("DialogFilter_All", GlobalOptions.Language),
                Multiselect = true
            };

            if(openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                Timekeeper.Start("load_sum");
                Cursor objOldCursor = Cursor;
                Cursor = Cursors.WaitCursor;
                List<string> lstFilesToOpen = new List<string>(openFileDialog.FileNames.Length);
                foreach(string strFile in openFileDialog.FileNames)
                {
                    Character objLoopCharacter = OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
                    if(objLoopCharacter != null)
                        SwitchToOpenCharacter(objLoopCharacter, true);
                    else
                        lstFilesToOpen.Add(strFile);
                }
                if(lstFilesToOpen.Count != 0)
                {
                    Character[] lstCharacters = new Character[lstFilesToOpen.Count];
                    object lstCharactersLock = new object();
                    Parallel.For(0, lstCharacters.Length, i =>
                    {
                        Character objLoopCharacter = LoadCharacter(lstFilesToOpen[i]);
                        lock(lstCharactersLock)
                            lstCharacters[i] = objLoopCharacter;
                    });
                    Program.MainForm.OpenCharacterList(lstCharacters);
                }

                Cursor = objOldCursor;
                Application.DoEvents();
                Timekeeper.Finish("load_sum");
                Timekeeper.Log();
            }
        }

        /// <summary>
        /// Opens the correct window for a single character (not thread-safe).
        /// </summary>
        public void OpenCharacter(Character objCharacter, bool blnIncludeInMRU = true)
        {
            OpenCharacterList(new List<Character> { objCharacter }, blnIncludeInMRU);
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

            Cursor objOldCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            FormWindowState wsPreference = FormWindowState.Maximized;
            if (OpenCharacterForms.Any(x => x.WindowState != wsPreference))
            {
                wsPreference = FormWindowState.Normal;
            }
            foreach(Character objCharacter in lstCharacters)
            {
                if(objCharacter == null || OpenCharacterForms.Any(x => x.CharacterObject == objCharacter))
                    continue;
                Timekeeper.Start("load_event_time");
                // Show the character form.
                if(!objCharacter.Created)
                {
                    frmCreate frmCharacter = new frmCreate(objCharacter)
                    {
                        MdiParent = this,
                        WindowState = wsPreference
                    };
                    frmCharacter.Show();
                }
                else
                {
                    frmCareer frmCharacter = new frmCareer(objCharacter)
                    {
                        MdiParent = this,
                        WindowState = wsPreference
                    };
                    frmCharacter.DiceRollerOpened += objCareer_DiceRollerOpened;
                    frmCharacter.DiceRollerOpenedInt += objCareer_DiceRollerOpenedInt;
                    frmCharacter.Show();
                }

                if(blnIncludeInMRU && !string.IsNullOrEmpty(objCharacter.FileName) && File.Exists(objCharacter.FileName))
                    GlobalOptions.MostRecentlyUsedCharacters.Insert(0, objCharacter.FileName);

                UpdateCharacterTabTitle(objCharacter, new PropertyChangedEventArgs(nameof(Character.CharacterName)));

                Timekeeper.Finish("load_event_time");
            }

            Cursor = objOldCursor;
        }

        /// <summary>
        /// Load a Character from a file and return it (thread-safe).
        /// </summary>
        /// <param name="strFileName">File to load.</param>
        /// <param name="strNewName">New name for the character.</param>
        /// <param name="blnClearFileName">Whether or not the name of the save file should be cleared.</param>
        /// <param name="blnShowErrors">Show error messages if the character failed to load.</param>
        public Character LoadCharacter(string strFileName, string strNewName = "", bool blnClearFileName = false, bool blnShowErrors = true)
        {
            Character objCharacter = null;
            if(File.Exists(strFileName) && strFileName.EndsWith("chum5"))
            {
                Timekeeper.Start("loading");
                objCharacter = new Character
                {
                    FileName = strFileName
                };
                frmLoading frmLoadingForm = null;
                if(blnShowErrors)
                {
                    frmLoadingForm = new frmLoading { CharacterFile = objCharacter.FileName };
                    frmLoadingForm.Reset(35);
                    frmLoadingForm.Show();
                }

                XmlDocument objXmlDocument = new XmlDocument();
                //StreamReader is used to prevent encoding errors
                using(StreamReader sr = new StreamReader(strFileName, Encoding.UTF8, true))
                {
                    try
                    {
                        objXmlDocument.Load(sr);
                    }
                    catch(XmlException ex)
                    {
                        if(blnShowErrors)
                            MessageBox.Show(string.Format(LanguageManager.GetString("Message_FailedLoad", GlobalOptions.Language), ex.Message),
                                LanguageManager.GetString("MessageTitle_FailedLoad", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        frmLoadingForm?.Close();
                        return null;
                    }
                }
                XmlNode objXmlCharacter = objXmlDocument.SelectSingleNode("/character");
                if(!string.IsNullOrEmpty(objXmlCharacter?["appversion"]?.InnerText))
                {
                    string strVersion = objXmlCharacter["appversion"].InnerText;
                    if(strVersion.StartsWith("0."))
                    {
                        strVersion = strVersion.Substring(2);
                    }
                    Version.TryParse(strVersion, out Version verSavedVersion);
                    Version.TryParse("5.188.34", out Version verCorrectedVersion);
                    if(verCorrectedVersion != null && verSavedVersion != null)
                    {
                        int intResult = verSavedVersion.CompareTo(verCorrectedVersion);
                        //Check for typo in Corrupter quality and correct it
                        if(intResult == -1)
                        {
                            File.WriteAllText(strFileName, Regex.Replace(File.ReadAllText(strFileName), "Corruptor", "Corrupter"));
                        }
                    }
                }

                OpenCharacters.Add(objCharacter);
                Timekeeper.Start("load_file");
                bool blnLoaded = objCharacter.Load(frmLoadingForm);
                Timekeeper.Finish("load_file");
                if(!blnLoaded)
                {
                    OpenCharacters.Remove(objCharacter);
                    objCharacter.DeleteCharacter();
                    frmLoadingForm?.Close();
                    return null;
                }

                // If a new name is given, set the character's name to match (used in cloning).
                if(!string.IsNullOrEmpty(strNewName))
                    objCharacter.Name = strNewName;
                // Clear the File Name field so that this does not accidentally overwrite the original save file (used in cloning).
                if(blnClearFileName)
                    objCharacter.FileName = string.Empty;
                frmLoadingForm?.Close();
            }
            else if(blnShowErrors)
            {
                MessageBox.Show(string.Format(LanguageManager.GetString("Message_FileNotFound", GlobalOptions.Language), strFileName),
                    LanguageManager.GetString("MessageTitle_FileNotFound", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return objCharacter;
        }

        /// <summary>
        /// Populate the MRU items.
        /// </summary>
        public void PopulateMRUToolstripMenu(object sender, TextEventArgs e)
        {
            ReadOnlyObservableCollection<string> strStickyMRUList = new ReadOnlyObservableCollection<string>(GlobalOptions.FavoritedCharacters);
            ReadOnlyObservableCollection<string> strMRUList = new ReadOnlyObservableCollection<string>(GlobalOptions.MostRecentlyUsedCharacters);

            SuspendLayout();
            mnuFileMRUSeparator.Visible = strStickyMRUList.Count > 0 || strMRUList.Count > 0;

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

                    if(i < strStickyMRUList.Count)
                    {
                        objItem.Visible = true;
                        objItem.Text = strStickyMRUList[i];
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

            int i2 = 0;
            for(int i = 0; i < GlobalOptions.MaxMruSize; ++i)
            {
                if(i2 < strMRUList.Count && i < strMRUList.Count)
                {
                    string strFile = strMRUList[i];
                    if(!strStickyMRUList.Contains(strFile))
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

                        objItem.Visible = true;
                        if(i2 == 9)
                            objItem.Text = "1&0 " + strFile;
                        else
                            objItem.Text = '&' + (i + 1).ToString() + ' ' + strFile;

                        ++i2;
                    }
                }
            }

            ResumeLayout();
        }

        private void objCareer_DiceRollerOpened(object sender)
        {
            MessageBox.Show("This feature is currently disabled. Please open a ticket if this makes the world burn, otherwise it will get re-enabled when somebody gets around to it");
            //TODO: IMPLEMENT THIS SHIT
        }

        private void objCareer_DiceRollerOpenedInt(Character objCharacter, int intDice)
        {
            if(GlobalOptions.SingleDiceRoller)
            {
                if(_frmRoller == null)
                {
                    _frmRoller = new frmDiceRoller(this, objCharacter.Qualities, intDice);
                    _frmRoller.Show();
                }
                else
                {
                    _frmRoller.Dice = intDice;
                    _frmRoller.Qualities = objCharacter.Qualities;
                    _frmRoller.Activate();
                }
            }
            else
            {
                frmDiceRoller frmRoller = new frmDiceRoller(this, objCharacter.Qualities, intDice);
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
#if LEGACY
        /// <summary>
        /// The frmOmae window being used by the application.
        /// </summary>
        public frmOmae OmaeWindow
        {
            get
            {
                return _frmOmae;
            }
            set
            {
                _frmOmae = value;
            }
        }
#endif

        /// <summary>
        /// The frmDiceRoller window being used by the application.
        /// </summary>
        public frmDiceRoller RollerWindow
        {
            get => _frmRoller;
            set => _frmRoller = value;
        }

        public ObservableCollection<Character> OpenCharacters => _lstCharacters;

        public ObservableCollection<CharacterShared> OpenCharacterForms => _lstOpenCharacterForms;

        #endregion

        private void frmChummerMain_Closing(object sender, FormClosingEventArgs e)
        {
            if(_workerVersionUpdateChecker.IsBusy)
                _workerVersionUpdateChecker.CancelAsync();
            Properties.Settings.Default.WindowState = WindowState;
            if(WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.Location = Location;
                Properties.Settings.Default.Size = Size;
            }
            else
            {
                Properties.Settings.Default.Location = RestoreBounds.Location;
                Properties.Settings.Default.Size = RestoreBounds.Size;
            }

            Properties.Settings.Default.Save();
        }

        private void mnuHeroLabImporter_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show(LanguageManager.GetString("Message_HeroLabImporterWarning", GlobalOptions.Language),
                    LanguageManager.GetString("Message_HeroLabImporterWarning_Title", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            frmHeroLabImporter frmHeroLabImporter = new frmHeroLabImporter();
            frmHeroLabImporter.Show();
        }
    }
}
