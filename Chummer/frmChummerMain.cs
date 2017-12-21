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
 using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;
 using System.Text.RegularExpressions;
 using System.Windows;
 using System.Windows.Shapes;
 using Chummer.Backend.Equipment;
 using Chummer.Backend.Skills;
 using Application = System.Windows.Forms.Application;
 using DataFormats = System.Windows.Forms.DataFormats;
 using DragDropEffects = System.Windows.Forms.DragDropEffects;
 using DragEventArgs = System.Windows.Forms.DragEventArgs;
 using MessageBox = System.Windows.Forms.MessageBox;
 using Path = System.IO.Path;
 using Point = System.Drawing.Point;
 using Rectangle = System.Drawing.Rectangle;
 using Size = System.Drawing.Size;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Net;

namespace Chummer
{
    public sealed partial class frmChummerMain : Form
    {
        private frmOmae _frmOmae;
        private frmDiceRoller _frmRoller;
        private frmUpdate _frmUpdate;
        private readonly List<Character> _lstCharacters = new List<Character>();
        private readonly List<CharacterShared> _lstOpenCharacterForms = new List<CharacterShared>();
        private readonly BackgroundWorker _workerVersionUpdateChecker = new BackgroundWorker();
        private readonly Version _objCurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        private readonly string _strCurrentVersion = string.Empty;

        #region Control Events
        public frmChummerMain()
        {
            InitializeComponent();
            _strCurrentVersion = $"{_objCurrentVersion.Major}.{_objCurrentVersion.Minor}.{_objCurrentVersion.Build}";
            this.Text = "Chummer 5a - Version " + _strCurrentVersion;
#if DEBUG
            Text += " DEBUG BUILD";
#endif

            LanguageManager.Load(GlobalOptions.Language, this);

            /** Dashboard **/
            //this.toolsMenu.DropDownItems.Add("GM Dashboard").Click += this.dashboardToolStripMenuItem_Click;
            /** End Dashboard **/

            // If Automatic Updates are enabled, check for updates immediately.

#if !DEBUG
            _workerVersionUpdateChecker.WorkerReportsProgress = false;
            _workerVersionUpdateChecker.WorkerSupportsCancellation = false;
            _workerVersionUpdateChecker.DoWork += DoCacheGitVersion;
            _workerVersionUpdateChecker.RunWorkerCompleted += CheckForUpdate;
            Application.Idle += IdleUpdateCheck;
            _workerVersionUpdateChecker.RunWorkerAsync();
#endif

            GlobalOptions.MRUChanged += PopulateMRUToolstripMenu;

            // Delete the old executable if it exists (created by the update process).
            foreach (string strLoopOldFilePath in Directory.GetFiles(Application.StartupPath, "*.old", SearchOption.AllDirectories))
            {
                if (File.Exists(strLoopOldFilePath))
                    File.Delete(strLoopOldFilePath);
            }

            // Populate the MRU list.
            PopulateMRUToolstripMenu();

            Program.MainForm = this;

            // Set the Tag for each ToolStrip item so it can be translated.
            foreach (ToolStripMenuItem objItem in menuStrip.Items.OfType<ToolStripMenuItem>())
            {
                if (objItem.Tag != null)
                {
                    objItem.Text = LanguageManager.GetString(objItem.Tag.ToString());
                }
            }

            // ToolStrip Items.
            foreach (ToolStrip objToolStrip in Controls.OfType<ToolStrip>())
            {
                foreach (ToolStripButton objButton in objToolStrip.Items.OfType<ToolStripButton>())
                {
                    if (objButton.Tag != null)
                        objButton.Text = LanguageManager.GetString(objButton.Tag.ToString());
                }
            }

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
                //() => XmlManager.Load("drugcomponents.xml"), TODO: Re-enable when Custom Drugs branch is merged
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
                () => XmlManager.Load("traditions.xml"),
                () => XmlManager.Load("vehicles.xml"),
                () => XmlManager.Load("weapons.xml")
            );
            Timekeeper.Finish("cache_load");

            _frmCharacterRoster = new frmCharacterRoster
            {
                MdiParent = this
            };

            // Retrieve the arguments passed to the application. If more than 1 is passed, we're being given the name of a file to open.
            string[] strArgs = Environment.GetCommandLineArgs();
            string strLoop = string.Empty;
            List<Character> lstCharactersToLoad = new List<Character>();
            object lstCharactersToLoadLock = new object();
            bool blnShowTest = false;
            object blnShowTestLock = new object();
            Parallel.For(1, strArgs.Length, i =>
            {
                strLoop = strArgs[i];
                if (strLoop == "/test")
                {
                    lock (blnShowTestLock)
                        blnShowTest = true;
                }
                else if (!strLoop.StartsWith('/'))
                {
                    Character objLoopCharacter = LoadCharacter(strLoop);
                    lock (lstCharactersToLoadLock)
                        lstCharactersToLoad.Add(objLoopCharacter);
                }
            });

            if (blnShowTest)
            {
                frmTest frmTestData = new frmTest();
                frmTestData.Show();
            }
            OpenCharacterList(lstCharactersToLoad);

            _frmCharacterRoster.WindowState = FormWindowState.Maximized;
            _frmCharacterRoster.Show();
        }

        private readonly frmCharacterRoster _frmCharacterRoster;
        public frmCharacterRoster CharacterRoster
        {
            get
            {
                return _frmCharacterRoster;
            }
        }

        private static void DoCacheGitVersion(object sender, EventArgs e)
        {
            string strUpdateLocation = "https://api.github.com/repos/chummer5a/chummer5a/releases/latest";
            if (GlobalOptions.PreferNightlyBuilds)
            {
                strUpdateLocation = "https://api.github.com/repos/chummer5a/chummer5a/releases";
            }
            HttpWebRequest request = null;
            try
            {
                WebRequest objTemp = WebRequest.Create(strUpdateLocation);
                request = objTemp as HttpWebRequest;
            }
            catch (System.Security.SecurityException)
            {
                Utils.CachedGitVersion = null;
                return;
            }
            if (request == null)
            {
                Utils.CachedGitVersion = null;
                return;
            }
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            request.Accept = "application/json";
            // Get the response.

            HttpWebResponse response = null;
            try
            {
                response = request.GetResponse() as HttpWebResponse;
            }
            catch (WebException)
            {
            }

            // Get the stream containing content returned by the server.
            Stream dataStream = response?.GetResponseStream();
            if (dataStream == null)
            {
                Utils.CachedGitVersion = null;
                return;
            }
            Version verLatestVersion = null;
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.

            string responseFromServer = reader.ReadToEnd();
            string[] stringSeparators = new string[] { "," };
            string[] result = responseFromServer.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            string line = result.FirstOrDefault(x => x.Contains("tag_name"));
            if (!string.IsNullOrEmpty(line))
            {
                string strVersion = line.Substring(line.IndexOf(':') + 1);
                if (strVersion.Contains('}'))
                    strVersion = strVersion.Substring(0, strVersion.IndexOf('}'));
                strVersion = strVersion.FastEscape('\"');
                // Adds zeroes if minor and/or build version are missing
                while (strVersion.Count(x => x == '.') < 2)
                {
                    strVersion = strVersion + ".0";
                }
                Version.TryParse(strVersion.TrimStart("Nightly-v"), out verLatestVersion);
            }
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();

            Utils.CachedGitVersion = verLatestVersion;
            return;
        }

        private void CheckForUpdate(object sender, EventArgs e)
        {
            if (Utils.GitUpdateAvailable() > 0)
            {
                if (GlobalOptions.AutomaticUpdate)
                {
                    if (_frmUpdate == null)
                    {
                        _frmUpdate = new frmUpdate();
                        _frmUpdate.FormClosed += ResetFrmUpdate;
                        _frmUpdate.SilentMode = true;
                    }
                }
                this.Text = string.Format("Chummer 5a - Version " + _strCurrentVersion + " - Update {0} now available!", Utils.CachedGitVersion);
            }
        }

        private readonly Stopwatch IdleUpdateCheck_StopWatch = Stopwatch.StartNew();
        private void IdleUpdateCheck(object sender, EventArgs e)
        {
            // Automatically check for updates every hour
            if (IdleUpdateCheck_StopWatch.ElapsedMilliseconds >= 3600000 && !_workerVersionUpdateChecker.IsBusy)
            {
                IdleUpdateCheck_StopWatch.Restart();
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

        private void dashboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmGMDashboard.Instance.Show();
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            frmOptions frmOptions = new frmOptions();
            frmOptions.ShowDialog(this);
            Cursor = Cursors.Default;
        }

        private void mnuToolsUpdate_Click(object sender, EventArgs e)
        {
            // Only a single instance of the updater can be open, so either find the current instance and focus on it, or create a new one.
            if (_frmUpdate == null)
            {
                _frmUpdate = new frmUpdate();
                _frmUpdate.FormClosed += ResetFrmUpdate;
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

        private void ResetFrmUpdate(object sender, EventArgs e)
        {
            _frmUpdate = null;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout frmShowAbout = new frmAbout();
            frmShowAbout.ShowDialog(this);
        }

        private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.chummergen.com/chummer/wiki/");
        }

        private void mnuHelpDumpshock_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/chummer5a/chummer5a/issues/");
        }

        private frmPrintMultiple _frmPrintMultipleCharacters;

        public frmPrintMultiple PrintMultipleCharactersForm
        {
            get
            {
                return _frmPrintMultipleCharacters;
            }
        }

        private void mnuFilePrintMultiple_Click(object sender, EventArgs e)
        {
            if (_frmPrintMultipleCharacters == null)
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
            string settingsPath = Path.Combine(Application.StartupPath, "settings");
            string[] settingsFiles = Directory.GetFiles(settingsPath, "*.xml");

            if (settingsFiles.Length > 1)
            {
                Cursor = Cursors.WaitCursor;
                frmSelectSetting frmPickSetting = new frmSelectSetting();
                frmPickSetting.ShowDialog(this);
                Cursor = Cursors.Default;

                if (frmPickSetting.DialogResult == DialogResult.Cancel)
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
            objCharacter.BuildPoints = 0;

            // Show the Metatype selection window.
            frmKarmaMetatype frmSelectMetatype = new frmKarmaMetatype(objCharacter)
            {
                XmlFile = "critters.xml"
            };
            frmSelectMetatype.ShowDialog();
            Cursor = Cursors.Default;

            if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                return;
            Cursor = Cursors.WaitCursor;

            // Add the Unarmed Attack Weapon to the character.
            XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");
            XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"Unarmed Attack\"]");
            if (objXmlWeapon != null)
            {
                Weapon objWeapon = new Weapon(objCharacter);
                objWeapon.Create(objXmlWeapon, null, null, null, objCharacter.Weapons);
                objCharacter.Weapons.Add(objWeapon);
            }

            frmCareer frmNewCharacter = new frmCareer(objCharacter)
            {
                MdiParent = this,
                WindowState = FormWindowState.Maximized
            };
            frmNewCharacter.Show();

            objCharacter.CharacterNameChanged += objCharacter_CharacterNameChanged;
            Cursor = Cursors.Default;
        }

        private void mnuMRU_Click(object sender, EventArgs e)
        {
            string strFileName = ((ToolStripMenuItem)sender).Text;
            strFileName = strFileName.Substring(3, strFileName.Length - 3).Trim();
            Cursor = Cursors.WaitCursor;
            Character objOpenCharacter = LoadCharacter(strFileName);
            Cursor = Cursors.Default;
            Program.MainForm.OpenCharacter(objOpenCharacter);
        }

        private void mnuMRU_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                string strFileName = ((ToolStripMenuItem)sender).Text;
                strFileName = strFileName.Substring(3, strFileName.Length - 3).Trim();

                GlobalOptions.RemoveFromMRUList(strFileName, "mru", false);
                GlobalOptions.AddToMRUList(strFileName, "stickymru");
            }
        }

        private void mnuStickyMRU_Click(object sender, EventArgs e)
        {
            string strFileName = ((ToolStripMenuItem)sender).Text;
            Cursor = Cursors.WaitCursor;
            Character objOpenCharacter = LoadCharacter(strFileName);
            Cursor = Cursors.Default;
            Program.MainForm.OpenCharacter(objOpenCharacter);
        }

        private void mnuStickyMRU_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                string strFileName = ((ToolStripMenuItem)sender).Text;

                GlobalOptions.RemoveFromMRUList(strFileName, "stickymru", false);
                GlobalOptions.AddToMRUList(strFileName);
            }
        }

        private void frmChummerMain_MdiChildActivate(object sender, EventArgs e)
        {
            // If there are no child forms, hide the tab control.
            if (ActiveMdiChild != null)
            {
                ActiveMdiChild.WindowState = FormWindowState.Maximized;

                // If this is a new child form and does not have a tab page, create one.
                if (ActiveMdiChild.Tag == null)
                {
                    TabPage tp = new TabPage
                    {
                        // Add a tab page.
                        Tag = ActiveMdiChild,
                        Parent = tabForms
                    };

                    if (ActiveMdiChild.GetType() == typeof(frmCareer))
                    {
                        tp.Text = ((frmCareer)ActiveMdiChild).CharacterObject.CharacterName;
                    }
                    else if (ActiveMdiChild.GetType() == typeof(frmCreate))
                    {
                        tp.Text = ((frmCreate)ActiveMdiChild).CharacterObject.CharacterName;
                    }
                    else if (ActiveMdiChild.GetType() == typeof(frmCharacterRoster))
                    {
                        tp.Text = LanguageManager.GetString("String_CharacterRoster");
                    }

                    tabForms.SelectedTab = tp;

                    ActiveMdiChild.Tag = tp;
                    ActiveMdiChild.FormClosed += ActiveMdiChild_FormClosed;
                }
            }
            // Don't show the tab control if there is only one window open.
            if (tabForms.TabCount > 1)
                tabForms.Visible = true;
            else
                tabForms.Visible = false;
        }

        private void ActiveMdiChild_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (sender is Form objForm)
            {
                objForm.FormClosed -= ActiveMdiChild_FormClosed;
                objForm.Dispose();
                (objForm.Tag as TabPage)?.Dispose();
            }

            // Don't show the tab control if there is only one window open.
            if (tabForms.TabCount <= 1)
                tabForms.Visible = false;
        }

        private void tabForms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabForms.SelectedTab != null && tabForms.SelectedTab.Tag != null)
                (tabForms.SelectedTab.Tag as Form)?.Select();
        }

        public bool SwitchToOpenCharacter(Character objCharacter, bool blnIncludeInMRU)
        {
            if (objCharacter != null)
            {
                Form objCharacterForm = OpenCharacterForms.FirstOrDefault(x => x.CharacterObject == objCharacter);
                if (objCharacterForm != null)
                {
                    foreach (TabPage objTabPage in tabForms.TabPages)
                    {
                        if (objTabPage.Tag == objCharacterForm)
                        {
                            tabForms.SelectTab(objTabPage);
                            return true;
                        }
                    }
                }
                if (OpenCharacters.Contains(objCharacter))
                {
                    OpenCharacter(objCharacter, blnIncludeInMRU);
                    return true;
                }
            }
            return false;
        }

        private void objCharacter_CharacterNameChanged(Object sender)
        {
            // Change the TabPage's text to match the character's name (or "Unnamed Character" if they are currently unnamed).
            if (tabForms.TabCount > 0 && tabForms.SelectedTab != null)
            {
                if (sender is Character objCharacter)
                {
                    string strTitle = objCharacter.CharacterName.Trim();

                    tabForms.SelectedTab.Text = strTitle;
                }
            }
        }

        private void mnuToolsDiceRoller_Click(object sender, EventArgs e)
        {
            if (GlobalOptions.SingleDiceRoller)
            {
                // Only a single instance of the Dice Roller window is allowed, so either find the existing one and focus on it, or create a new one.
                if (_frmRoller == null)
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
        }

        private void Menu_DropDownOpening(object sender, EventArgs e)
        {
            // Translate the items in the menu by finding their Tags in the translation file.
            foreach (ToolStripMenuItem objItem in ((ToolStripMenuItem)sender).DropDownItems.OfType<ToolStripMenuItem>())
            {
                if (objItem.Tag != null)
                {
                    objItem.Text = LanguageManager.GetString(objItem.Tag.ToString());
                }
            }
        }

        private void menuStrip_ItemAdded(object sender, ToolStripItemEventArgs e)
        {
            // Translate the items in the menu by finding their Tags in the translation file.
            foreach (ToolStripMenuItem objItem in menuStrip.Items.OfType<ToolStripMenuItem>())
            {
                if (objItem.Tag != null)
                {
                    objItem.Text = LanguageManager.GetString(objItem.Tag.ToString());
                }
            }
        }

        private void toolStrip_ItemAdded(object sender, ToolStripItemEventArgs e)
        {
            // ToolStrip Items.
            foreach (ToolStrip objToolStrip in Controls.OfType<ToolStrip>())
            {
                foreach (ToolStripButton objButton in objToolStrip.Items.OfType<ToolStripButton>())
                {
                    if (objButton.Tag != null)
                        objButton.Text = LanguageManager.GetString(objButton.Tag.ToString());
                }
            }
        }

        private void toolStrip_ItemRemoved(object sender, ToolStripItemEventArgs e)
        {
            // ToolStrip Items.
            foreach (ToolStrip objToolStrip in Controls.OfType<ToolStrip>())
            {
                foreach (ToolStripButton objButton in objToolStrip.Items.OfType<ToolStripButton>())
                {
                    if (objButton.Tag != null)
                        objButton.Text = LanguageManager.GetString(objButton.Tag.ToString());
                }
            }
        }

        private void frmChummerMain_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.Size.Width == 0 || Properties.Settings.Default.Size.Height == 0 || !IsVisibleOnAnyScreen())
            {
                Size = new Size(1191, 752);
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
            Cursor = Cursors.WaitCursor;
            // Open each file that has been dropped into the window.
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            Character[] lstCharacters = new Character[s.Length];
            object lstCharactersLock = new object();
            Parallel.For(0, s.Length, i =>
            {
                Character objLoopCharacter = LoadCharacter(s[i]);
                lock (lstCharactersLock)
                    lstCharacters[i] = objLoopCharacter;
            });
            Cursor = Cursors.Default;
            Program.MainForm.OpenCharacterList(lstCharacters);
        }

        private void frmChummerMain_DragEnter(object sender, DragEventArgs e)
        {
            // Only use a drop effect if a file is being dragged into the window.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void mnuToolsTranslator_Click(object sender, EventArgs e)
        {
            string strTranslator = Path.Combine(Application.StartupPath, "Translator.exe");
            if (File.Exists(strTranslator))
                System.Diagnostics.Process.Start(strTranslator);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Create a new character and show the Create Form.
        /// </summary>
        private void ShowNewForm(object sender, EventArgs e)
        {
            string strFilePath = Path.Combine(Application.StartupPath, "settings", "default.xml");
            if (!File.Exists(strFilePath))
            {
                if (MessageBox.Show(LanguageManager.GetString("Message_CharacterOptions_OpenOptions"), LanguageManager.GetString("MessageTitle_CharacterOptions_OpenOptions"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Cursor = Cursors.WaitCursor;
                    frmOptions frmOptions = new frmOptions();
                    frmOptions.ShowDialog();
                    Cursor = Cursors.Default;
                }
            }
            Cursor = Cursors.WaitCursor;
            Character objCharacter = new Character();
            string settingsPath = Path.Combine(Application.StartupPath, "settings");
            string[] settingsFiles = Directory.GetFiles(settingsPath, "*.xml");

            if (settingsFiles.Length > 1)
            {
                frmSelectSetting frmPickSetting = new frmSelectSetting();
                frmPickSetting.ShowDialog(this);

                if (frmPickSetting.DialogResult == DialogResult.Cancel)
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
            Cursor = Cursors.Default;

            if (frmBP.DialogResult == DialogResult.Cancel)
                return;
            if (objCharacter.BuildMethod == CharacterBuildMethod.Karma || objCharacter.BuildMethod == CharacterBuildMethod.LifeModule)
            {
                Cursor = Cursors.WaitCursor;
                frmKarmaMetatype frmSelectMetatype = new frmKarmaMetatype(objCharacter);
                frmSelectMetatype.ShowDialog();
                Cursor = Cursors.Default;

                if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                { return; }
            }
            // Show the Metatype selection window.
            else if (objCharacter.BuildMethod == CharacterBuildMethod.Priority || objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                Cursor = Cursors.WaitCursor;
                frmPriorityMetatype frmSelectMetatype = new frmPriorityMetatype(objCharacter);
                frmSelectMetatype.ShowDialog();
                Cursor = Cursors.Default;

                if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                { return; }
            }
            Cursor = Cursors.WaitCursor;

            // Add the Unarmed Attack Weapon to the character.
            XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");
            XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"Unarmed Attack\"]");
            if (objXmlWeapon != null)
            {
                Weapon objWeapon = new Weapon(objCharacter);
                objWeapon.Create(objXmlWeapon, null, null, null, objCharacter.Weapons);
                objCharacter.Weapons.Add(objWeapon);
            }

            frmCreate frmNewCharacter = new frmCreate(objCharacter)
            {
                MdiParent = this,
                WindowState = FormWindowState.Maximized
            };
            frmNewCharacter.Show();

            objCharacter.CharacterNameChanged += objCharacter_CharacterNameChanged;
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Show the Open File dialogue, then load the selected character.
        /// </summary>
        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Chummer5 Files (*.chum5)|*.chum5|All Files (*.*)|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                Timekeeper.Start("load_sum");
                Cursor = Cursors.WaitCursor;
                Character[] lstCharacters = new Character[openFileDialog.FileNames.Length];
                object lstCharactersLock = new object();
                Parallel.For(0, lstCharacters.Length, i =>
                {
                    Character objLoopCharacter = LoadCharacter(openFileDialog.FileNames[i]);
                    lock (lstCharactersLock)
                        lstCharacters[i] = objLoopCharacter;
                });
                Cursor = Cursors.Default;
                Program.MainForm.OpenCharacterList(lstCharacters);
                Application.DoEvents();
                Timekeeper.Finish("load_sum");
                Timekeeper.Log();
            }
        }

        /// <summary>
        /// Opens the correct window for a single character (not thread-safe).
        /// </summary>
        /// <param name="lstCharacters">Characters for which windows should be opened.</param>
        public void OpenCharacter(Character objCharacter, bool blnIncludeInMRU = true)
        {
            OpenCharacterList(new List<Character>{ objCharacter }, blnIncludeInMRU);
        }

        /// <summary>
        /// Open the correct windows for a list of characters (not thread-safe).
        /// </summary>
        /// <param name="lstCharacters">Characters for which windows should be opened.</param>
        public void OpenCharacterList(IEnumerable<Character> lstCharacters, bool blnIncludeInMRU = true)
        {
            if (lstCharacters == null)
                return;

            Cursor = Cursors.WaitCursor;

            foreach (Character objCharacter in lstCharacters)
            {
                if (objCharacter == null)
                    continue;
                Timekeeper.Start("load_event_time");
                // Show the character form.
                if (!objCharacter.Created)
                {
                    frmCreate frmCharacter = new frmCreate(objCharacter)
                    {
                        MdiParent = this,
                        WindowState = FormWindowState.Maximized,
                        Loading = true
                    };
                    frmCharacter.Show();
                }
                else
                {
                    frmCareer frmCharacter = new frmCareer(objCharacter)
                    {
                        MdiParent = this,
                        WindowState = FormWindowState.Maximized,
                        Loading = true
                    };
                    frmCharacter.DiceRollerOpened += objCareer_DiceRollerOpened;
                    frmCharacter.DiceRollerOpenedInt += objCareer_DiceRollerOpenedInt;
                    frmCharacter.Show();
                }

                if (blnIncludeInMRU)
                    GlobalOptions.AddToMRUList(objCharacter.FileName);

                objCharacter.CharacterNameChanged += objCharacter_CharacterNameChanged;
                objCharacter_CharacterNameChanged(objCharacter);
                Timekeeper.Finish("load_event_time");
            }

            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Load a Character from a file and return it (thread-safe).
        /// </summary>
        /// <param name="strFileName">File to load.</param>
        /// <param name="blnIncludeInMRU">Whether or not the file should appear in the MRU list.</param>
        /// <param name="strNewName">New name for the character.</param>
        /// <param name="blnClearFileName">Whether or not the name of the save file should be cleared.</param>
        public Character LoadCharacter(string strFileName, string strNewName = "", bool blnClearFileName = false, bool blnShowErrors = true)
        {
            Character objCharacter = null;
            if (File.Exists(strFileName) && strFileName.EndsWith("chum5"))
            {
                Timekeeper.Start("loading");
                bool blnLoaded = false;
                objCharacter = new Character
                {
                    FileName = strFileName
                };

                XmlDocument objXmlDocument = new XmlDocument();
                //StreamReader is used to prevent encoding errors
                using (StreamReader sr = new StreamReader(strFileName, true))
                {
                    try
                    {
                        objXmlDocument.Load(sr);
                    }
                    catch (XmlException ex)
                    {
                        if (blnShowErrors)
                            MessageBox.Show(LanguageManager.GetString("Message_FailedLoad").Replace("{0}", ex.Message), LanguageManager.GetString("MessageTitle_FailedLoad"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
                XmlNode objXmlCharacter = objXmlDocument.SelectSingleNode("/character");
                if (!string.IsNullOrEmpty(objXmlCharacter?["appversion"]?.InnerText))
                {
                    string strVersion = objXmlCharacter["appversion"].InnerText;
                    if (strVersion.StartsWith("0."))
                    {
                        strVersion = strVersion.Substring(2);
                    }
                    Version.TryParse(strVersion, out Version verSavedVersion);
                    Version.TryParse("5.188.34", out Version verCorrectedVersion);
                    if (verCorrectedVersion != null && verSavedVersion != null)
                    {
                        int intResult = verSavedVersion.CompareTo(verCorrectedVersion);
                        //Check for typo in Corrupter quality and correct it
                        if (intResult == -1)
                        {
                            File.WriteAllText(strFileName, Regex.Replace(File.ReadAllText(strFileName), "Corruptor", "Corrupter"));
                        }
                    }
                }

                OpenCharacters.Add(objCharacter);
                Timekeeper.Start("load_file");
                blnLoaded = objCharacter.Load();
                Timekeeper.Finish("load_file");
                if (!blnLoaded)
                {
                    objCharacter.Dispose();
                    OpenCharacters.Remove(objCharacter);
                    return null;
                }

                // If a new name is given, set the character's name to match (used in cloning).
                if (!string.IsNullOrEmpty(strNewName))
                    objCharacter.Name = strNewName;
                // Clear the File Name field so that this does not accidentally overwrite the original save file (used in cloning).
                if (blnClearFileName)
                    objCharacter.FileName = string.Empty;
            }
            else if (blnShowErrors)
            {
                MessageBox.Show(LanguageManager.GetString("Message_FileNotFound").Replace("{0}", strFileName), LanguageManager.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return objCharacter;
        }

        /// <summary>
        /// Populate the MRU items.
        /// </summary>
        public void PopulateMRUToolstripMenu()
        {
            List<string> strStickyMRUList = GlobalOptions.ReadMRUList("stickymru");
            List<string> strMRUList = GlobalOptions.ReadMRUList();

            for (int i = 0; i < 10; i++)
            {
                ToolStripMenuItem objStickyItem;
                ToolStripMenuItem objItem;
                switch (i)
                {
                    case 0:
                        objStickyItem = mnuStickyMRU0;
                        objItem = mnuMRU0;
                        break;
                    case 1:
                        objStickyItem = mnuStickyMRU1;
                        objItem = mnuMRU1;
                        break;
                    case 2:
                        objStickyItem = mnuStickyMRU2;
                        objItem = mnuMRU2;
                        break;
                    case 3:
                        objStickyItem = mnuStickyMRU3;
                        objItem = mnuMRU3;
                        break;
                    case 4:
                        objStickyItem = mnuStickyMRU4;
                        objItem = mnuMRU4;
                        break;
                    case 5:
                        objStickyItem = mnuStickyMRU5;
                        objItem = mnuMRU5;
                        break;
                    case 6:
                        objStickyItem = mnuStickyMRU6;
                        objItem = mnuMRU6;
                        break;
                    case 7:
                        objStickyItem = mnuStickyMRU7;
                        objItem = mnuMRU7;
                        break;
                    case 8:
                        objStickyItem = mnuStickyMRU8;
                        objItem = mnuMRU8;
                        break;
                    case 9:
                        objStickyItem = mnuStickyMRU9;
                        objItem = mnuMRU9;
                        break;
                    default:
                        continue;
                }

                if (i < strStickyMRUList.Count)
                {
                    objStickyItem.Visible = true;
                    objStickyItem.Text = strStickyMRUList[i];
                    mnuFileMRUSeparator.Visible = true;
                }
                else
                {
                    objStickyItem.Visible = false;
                }
                if (i < strMRUList.Count)
                {
                    objItem.Visible = true;
                    if (i == 9)
                        objItem.Text = "1&0 " + strMRUList[i];
                    else
                        objItem.Text = "&" + (i + 1).ToString() + " " + strMRUList[i];
                    mnuFileMRUSeparator.Visible = true;
                }
                else
                {
                    objItem.Visible = false;
                }
            }
        }

        private void objCareer_DiceRollerOpened(Object sender)
        {
            MessageBox.Show("This feature is currently disabled. Please open a ticket if this makes the world burn, otherwise it will get re-enabled when somebody gets around to it");
            //TODO: IMPLEMENT THIS SHIT
        }

        private void objCareer_DiceRollerOpenedInt(Character objCharacter, int intDice)
        {
            if (GlobalOptions.SingleDiceRoller)
            {
                if (_frmRoller == null)
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
            GlobalOptions.RemoveFromMRUList(GlobalOptions.ReadMRUList());
        }

        private void mnuRestart_Click(object sender, EventArgs e)
        {
            Utils.RestartApplication();
        }
        #endregion

        #region Application Properties
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

        /// <summary>
        /// The frmDiceRoller window being used by the application.
        /// </summary>
        public frmDiceRoller RollerWindow
        {
            get
            {
                return _frmRoller;
            }
            set
            {
                _frmRoller = value;
            }
        }

        public IList<Character> OpenCharacters
        {
            get { return _lstCharacters; }
        }

        public IList<CharacterShared> OpenCharacterForms
        {
            get { return _lstOpenCharacterForms; }
        }
        #endregion

        private void frmChummerMain_Closing(object sender, FormClosingEventArgs e)
        {
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

            Properties.Settings.Default.Save();
        }
    }
}
