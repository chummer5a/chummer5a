﻿using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;

namespace Chummer
{
	public partial class frmMain : Form
	{
		private frmOmae _frmOmae;
		private frmDiceRoller _frmRoller;
		private frmUpdate _frmUpdate;
		private Character _objCharacter;

        #region Control Events
        public frmMain()
		{
			InitializeComponent();
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			string strCurrentVersion = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);

			this.Text = string.Format("Chummer 5a - Version " + strCurrentVersion);

#if DEBUG
	        Text += " DEBUG BUILD";
#endif

			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

            /** Dashboard **/
            //this.toolsMenu.DropDownItems.Add("GM Dashboard").Click += this.dashboardToolStripMenuItem_Click;
            /** End Dashboard **/

			// If Automatic Updates are enabled, check for updates immediately.
			if (GlobalOptions.Instance.AutomaticUpdate)
			{
				frmUpdate frmAutoUpdate = new frmUpdate();
                frmAutoUpdate.SilentMode = true;
                frmAutoUpdate.Visible = false;
                frmAutoUpdate.ShowDialog(this);
			}
			else
			{
#if RELEASE
				frmUpdate frmAutoUpdate = new frmUpdate();
				frmAutoUpdate.GetChummerVersion();
				Version verCurrentVersion = new Version(strCurrentVersion);
				Version verLatestVersion = new Version(frmAutoUpdate.LatestVersion);

				var result = verCurrentVersion.CompareTo(verLatestVersion);
				if (result != 0)
					this.Text += String.Format(" - Update {0} now available!",verLatestVersion);
#endif
			}

			GlobalOptions.Instance.MRUChanged += PopulateMRU;

			// Delete the old executable if it exists (created by the update process).
			if (File.Exists("Chummer.exe.old"))
			{
				try
				{
					File.Delete("Chummer.exe.old");
				}
				catch
				{
				}
			}

			// Populate the MRU list.
			PopulateMRU();

			// Retrieve the arguments passed to the application. If more than 1 is passed, we're being given the name of a file to open.
			string[] strArgs = Environment.GetCommandLineArgs();
			if (strArgs.GetUpperBound(0) > 0)
			{
				if (strArgs[1] != "/debug")
					LoadCharacter(strArgs[1]);
				if (strArgs.Length > 2)
				{
					if (strArgs[2] == "/test")
					{
						frmTest frmTestData = new frmTest();
						frmTestData.Show();
					}
				}
			}

			GlobalOptions.Instance.MainForm = this;

			// Set the Tag for each ToolStrip item so it can be translated.
			foreach (ToolStripMenuItem objItem in menuStrip.Items.OfType<ToolStripMenuItem>())
			{
				if (objItem.Tag != null)
				{
					objItem.Text = LanguageManager.Instance.GetString(objItem.Tag.ToString());
				}
			}

			// ToolStrip Items.
			foreach (ToolStrip objToolStrip in this.Controls.OfType<ToolStrip>())
			{
				foreach (ToolStripButton objButton in objToolStrip.Items.OfType<ToolStripButton>())
				{
					if (objButton.Tag != null)
						objButton.Text = LanguageManager.Instance.GetString(objButton.Tag.ToString());
				}
			}

			// Attempt to cache the XML files that are used the most.
			try
			{
				Timekeeper.Start("cache_load");
				XmlManager.Instance.Load("armor.xml");
				XmlManager.Instance.Load("bioware.xml");
				XmlManager.Instance.Load("books.xml");
				XmlManager.Instance.Load("cyberware.xml");
				XmlManager.Instance.Load("gear.xml");
				XmlManager.Instance.Load("lifestyles.xml");
				XmlManager.Instance.Load("metatypes.xml");
				XmlManager.Instance.Load("qualities.xml");
				XmlManager.Instance.Load("ranges.xml");
				XmlManager.Instance.Load("skills.xml");
				XmlManager.Instance.Load("vehicles.xml");
				XmlManager.Instance.Load("weapons.xml");
				Timekeeper.Finish("cache_load");
			}
			catch
			{
			}
            mnuPlayerDashboard.Enabled = false;
            mnuPlayerDashboard.ToolTipText = "to be done";
		}

		private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
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
			frmOptions frmOptions = new frmOptions();
			frmOptions.ShowDialog(this);
		}

		private void mnuToolsUpdate_Click(object sender, EventArgs e)
		{
			// Only a single instance of the updater can be open, so either find the current instance and focus on it, or create a new one.
			if (_frmUpdate == null)
			{
				frmUpdate frmUpdate = new frmUpdate();
				_frmUpdate = frmUpdate;
				_frmUpdate.Show();
			}
			else
			{
				_frmUpdate.Focus();
			}
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

		private void mnuFilePrintMultiple_Click(object sender, EventArgs e)
		{
			frmPrintMultiple frmPrintMultipleCharacters = new frmPrintMultiple();
			frmPrintMultipleCharacters.ShowDialog(this);
		}

		private void mnuHelpRevisionHistory_Click(object sender, EventArgs e)
		{
			frmHistory frmShowHistory = new frmHistory();
			frmShowHistory.ShowDialog(this);
		}

		private void mnuNewCritter_Click(object sender, EventArgs e)
		{
			Character objCharacter = new Character();
			string settingsPath = Path.Combine(Environment.CurrentDirectory, "settings");
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

			// Override the defaults for the setting.
			objCharacter.IgnoreRules = true;
			objCharacter.IsCritter = true;
			objCharacter.BuildMethod = CharacterBuildMethod.Karma;
			objCharacter.BuildPoints = 0;

			// Make sure that Running Wild is one of the allowed source books since most of the Critter Powers come from this book.
			bool blnRunningWild = false;
			blnRunningWild = (objCharacter.Options.Books.Contains("RW"));

			if (!blnRunningWild)
			{
			    MessageBox.Show(LanguageManager.Instance.GetString("Message_Main_RunningWild"), LanguageManager.Instance.GetString("MessageTitle_Main_RunningWild"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			    return;
			}

			// Show the Metatype selection window.
            if (objCharacter.BuildMethod == CharacterBuildMethod.Priority)
            {
                frmPriorityMetatype frmSelectMetatype = new frmPriorityMetatype(objCharacter);
                frmSelectMetatype.XmlFile = "critters.xml";
                frmSelectMetatype.ShowDialog();

                if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                    return;
            }
            else
            {
                frmKarmaMetatype frmSelectMetatype = new frmKarmaMetatype(objCharacter);
                frmSelectMetatype.XmlFile = "critters.xml";
                frmSelectMetatype.ShowDialog();

                if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                    return;
            }

			// Add the Unarmed Attack Weapon to the character.
			try
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("weapons.xml");
				XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"Unarmed Attack\"]");
				TreeNode objDummy = new TreeNode();
				Weapon objWeapon = new Weapon(objCharacter);
				objWeapon.Create(objXmlWeapon, objCharacter, objDummy, null, null, null);
				objCharacter.Weapons.Add(objWeapon);
			}
			catch
			{
			}

			frmCreate frmNewCharacter = new frmCreate(objCharacter);
			frmNewCharacter.MdiParent = this;
			frmNewCharacter.WindowState = FormWindowState.Maximized;
			frmNewCharacter.Show();

			objCharacter.CharacterNameChanged += objCharacter_CharacterNameChanged;
		}

		private void mnuMRU_Click(object sender, EventArgs e)
		{
			string strFileName = ((ToolStripMenuItem)sender).Text;
			string strNumber = strFileName.Substring(0, 3);
			strFileName = strFileName.Replace(strNumber, string.Empty).Trim();
			LoadCharacter(strFileName);
		}

		private void mnuMRU_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				string strFileName = ((ToolStripMenuItem)sender).Text;
				string strNumber = strFileName.Substring(0, 3);
				strFileName = strFileName.Replace(strNumber, string.Empty).Trim();

				GlobalOptions.Instance.RemoveFromMRUList(strFileName);
				GlobalOptions.Instance.AddToStickyMRUList(strFileName);
			}
		}

		private void mnuStickyMRU_Click(object sender, EventArgs e)
		{
			string strFileName = ((ToolStripMenuItem)sender).Text;
			LoadCharacter(strFileName);
		}

		private void mnuStickyMRU_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				string strFileName = ((ToolStripMenuItem)sender).Text;

				GlobalOptions.Instance.RemoveFromStickyMRUList(strFileName);
				GlobalOptions.Instance.AddToMRUList(strFileName);
			}
		}

		private void frmMain_MdiChildActivate(object sender, EventArgs e)
		{
			// If there are no child forms, hide the tab control.
			if (this.ActiveMdiChild == null)
				tabForms.Visible = false;
			else
			{
				this.ActiveMdiChild.WindowState = FormWindowState.Maximized;

				// If this is a new child form and does not have a tab page, create one.
				if (this.ActiveMdiChild.Tag == null)
				{
					// Add a tab page.
					TabPage tp = new TabPage(LanguageManager.Instance.GetString("String_UnnamedCharacter"));
					tp.Tag = this.ActiveMdiChild;
					tp.Parent = tabForms;

					if (this.ActiveMdiChild.GetType() == typeof(frmCareer))
					{
						tp.Text = ((frmCareer)this.ActiveMdiChild).CharacterName;
					}

					tabForms.SelectedTab = tp;

					this.ActiveMdiChild.Tag = tp;
					this.ActiveMdiChild.FormClosed += ActiveMdiChild_FormClosed;
				}

				// Don't show the tab control if there is only one window open.
				if (tabForms.TabCount <= 1)
					tabForms.Visible = false;
				else
					tabForms.Visible = true;
			}
		}

		private void ActiveMdiChild_FormClosed(object sender, FormClosedEventArgs e)
		{
			(sender as Form).FormClosed -= ActiveMdiChild_FormClosed;
			(sender as Form).Dispose();
			((sender as Form).Tag as TabPage).Dispose();
			
			// Don't show the tab control if there is only one window open.
			if (tabForms.TabCount <= 1)
				tabForms.Visible = false;
		}

		private void tabForms_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabForms.SelectedTab != null && tabForms.SelectedTab.Tag != null)
				(tabForms.SelectedTab.Tag as Form).Select();
		}

		private void objCharacter_CharacterNameChanged(Object sender)
		{
			// Change the TabPage's text to match the character's name (or "Unnamed Character" if they are currently unnamed).
			Character objCharacter = (Character)sender;
			string strTitle = objCharacter.Name;
			if (objCharacter.Alias.Trim() != string.Empty)
			{
				strTitle = objCharacter.Alias;
			}
			else
			{
				strTitle = LanguageManager.Instance.GetString("String_UnnamedCharacter");
			}

			try
			{
				tabForms.SelectedTab.Text = strTitle;
			}
			catch
			{
			}
		}

		private void mnuToolsDiceRoller_Click(object sender, EventArgs e)
		{
			if (GlobalOptions.Instance.SingleDiceRoller)
			{
				// Only a single instance of the Dice Roller window is allowed, so either find the existing one and focus on it, or create a new one.
				if (_frmRoller == null)
				{
					frmDiceRoller frmRoller = new frmDiceRoller(this);
					_frmRoller = frmRoller;
					frmRoller.Show();
				}
				else
				{
					_frmRoller.Focus();
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
				frmOmae frmOmaeOnline = new frmOmae(this);
				_frmOmae = frmOmaeOnline;
				frmOmaeOnline.Show();
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
					objItem.Text = LanguageManager.Instance.GetString(objItem.Tag.ToString());
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
					objItem.Text = LanguageManager.Instance.GetString(objItem.Tag.ToString());
				}
			}
		}

		private void toolStrip_ItemAdded(object sender, ToolStripItemEventArgs e)
		{
			// ToolStrip Items.
			foreach (ToolStrip objToolStrip in this.Controls.OfType<ToolStrip>())
			{
				foreach (ToolStripButton objButton in objToolStrip.Items.OfType<ToolStripButton>())
				{
					if (objButton.Tag != null)
						objButton.Text = LanguageManager.Instance.GetString(objButton.Tag.ToString());
				}
			}
		}

		private void toolStrip_ItemRemoved(object sender, ToolStripItemEventArgs e)
		{
			// ToolStrip Items.
			foreach (ToolStrip objToolStrip in this.Controls.OfType<ToolStrip>())
			{
				foreach (ToolStripButton objButton in objToolStrip.Items.OfType<ToolStripButton>())
				{
					if (objButton.Tag != null)
						objButton.Text = LanguageManager.Instance.GetString(objButton.Tag.ToString());
				}
			}
		}

		private void frmMain_Load(object sender, EventArgs e)
		{
			if (GlobalOptions.Instance.StartupFullscreen)
				this.WindowState = FormWindowState.Maximized;
            if (GlobalOptions.Instance.UseLogging)
            {
				CommonFunctions objFunctions = new CommonFunctions();
            }
		}

		private void frmMain_DragDrop(object sender, DragEventArgs e)
		{
			// Open each file that has been dropped into the window.
			string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
			foreach (string strFileName in s)
				LoadCharacter(strFileName);
		}

		private void frmMain_DragEnter(object sender, DragEventArgs e)
		{
			// Only use a drop effect if a file is being dragged into the window.
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.All;
			else
				e.Effect = DragDropEffects.None;
		}

        private void trySkillToolStripMenuItem_Click(object sender, EventArgs e, Character objCharacter)
        {
            objCharacter = _objCharacter;
            foreach (Skill objSkill in objCharacter.Skills)
            {
                if (objSkill.Name == "Impersonation")
                {
                    MessageBox.Show(objSkill.Rating.ToString());
                }
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Create a new character and show the Create Form.
        /// </summary>
        private void ShowNewForm(object sender, EventArgs e)
		{
			string strFilePath = Path.Combine(Environment.CurrentDirectory, "settings", "default.xml");
			if (!File.Exists(strFilePath))
			{
				if (MessageBox.Show(LanguageManager.Instance.GetString("Message_CharacterOptions_OpenOptions"), LanguageManager.Instance.GetString("MessageTitle_CharacterOptions_OpenOptions"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					frmOptions frmOptions = new frmOptions();
					frmOptions.ShowDialog();
				}
			}
			Character objCharacter = new Character();
			string settingsPath = Path.Combine(Environment.CurrentDirectory, "settings");
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

			if (frmBP.DialogResult == DialogResult.Cancel)
				return;
            if (objCharacter.BuildMethod == CharacterBuildMethod.Karma || objCharacter.BuildMethod == CharacterBuildMethod.LifeModule)
            {
                frmKarmaMetatype frmSelectMetatype = new frmKarmaMetatype(objCharacter);
                frmSelectMetatype.ShowDialog();

                if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                { return; }
            }
            // Show the Metatype selection window.
            else if (objCharacter.BuildMethod == CharacterBuildMethod.Priority || objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                frmPriorityMetatype frmSelectMetatype = new frmPriorityMetatype(objCharacter);
                frmSelectMetatype.ShowDialog();

                if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                { return; }
            }

			// Add the Unarmed Attack Weapon to the character.
			try
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("weapons.xml");
				XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"Unarmed Attack\"]");
				TreeNode objDummy = new TreeNode();
				Weapon objWeapon = new Weapon(objCharacter);
				objWeapon.Create(objXmlWeapon, objCharacter, objDummy, null, null, null);
				objCharacter.Weapons.Add(objWeapon);
			}
			catch
			{
			}

			frmCreate frmNewCharacter = new frmCreate(objCharacter);
			frmNewCharacter.MdiParent = this;
			frmNewCharacter.WindowState = FormWindowState.Maximized;
			frmNewCharacter.Show();

			objCharacter.CharacterNameChanged += objCharacter_CharacterNameChanged;
		}

		/// <summary>
		/// Show the Open File dialogue, then load the selected character.
		/// </summary>
		private void OpenFile(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Chummer5 Files (*.chum5)|*.chum5|All Files (*.*)|*.*";
			openFileDialog.Multiselect = true;

			if (openFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				Timekeeper.Start("load_sum");
				foreach (string strFileName in openFileDialog.FileNames)
				{
					LoadCharacter(strFileName);
					Timekeeper.Start("load_event_time");
					Application.DoEvents();
					Timekeeper.Finish("load_event_time");
				}
				Timekeeper.Finish("load_sum");
				Timekeeper.Log();
			}
		}

		/// <summary>
		/// Load a Character and open the correct window.
		/// </summary>
		/// <param name="strFileName">File to load.</param>
		/// <param name="blnIncludeInMRU">Whether or not the file should appear in the MRU list.</param>
		/// <param name="strNewName">New name for the character.</param>
		/// <param name="blnClearFileName">Whether or not the name of the save file should be cleared.</param>
		public void LoadCharacter(string strFileName, bool blnIncludeInMRU = true, string strNewName = "", bool blnClearFileName = false)
		{
			if (File.Exists(strFileName) && strFileName.EndsWith("chum5"))
			{
				Timekeeper.Start("loading");
				bool blnLoaded = false;
				Character objCharacter = new Character();
				objCharacter.FileName = strFileName;
				Timekeeper.Start("load_file");
				blnLoaded = objCharacter.Load();
				Timekeeper.Finish("load_file");
				Timekeeper.Start("load_free");
				if (!blnLoaded)
					return;

				// If a new name is given, set the character's name to match (used in cloning).
				if (strNewName != "")
					objCharacter.Name = strNewName;
				// Clear the File Name field so that this does not accidentally overwrite the original save file (used in cloning).
				if (blnClearFileName)
					objCharacter.FileName = "";

				// Show the character form.
				if (!objCharacter.Created)
				{
					frmCreate frmCharacter = new frmCreate(objCharacter);
					frmCharacter.MdiParent = this;
					frmCharacter.WindowState = FormWindowState.Maximized;
					frmCharacter.Loading = true;
					frmCharacter.Show();
				}
				else
				{
					frmCareer frmCharacter = new frmCareer(objCharacter);
					frmCharacter.MdiParent = this;
					frmCharacter.WindowState = FormWindowState.Maximized;
					frmCharacter.Loading = true;
					frmCharacter.DiceRollerOpened += objCareer_DiceRollerOpened;
					frmCharacter.DiceRollerOpenedInt += objCareer_DiceRollerOpenedInt;
					frmCharacter.Show();
				}

				if (blnIncludeInMRU)
					GlobalOptions.Instance.AddToMRUList(strFileName);

				objCharacter.CharacterNameChanged += objCharacter_CharacterNameChanged;
				objCharacter_CharacterNameChanged(objCharacter);
			}
			else
			{
				MessageBox.Show(LanguageManager.Instance.GetString("Message_FileNotFound").Replace("{0}", strFileName), LanguageManager.Instance.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Populate the MRU items.
		/// </summary>
		public void PopulateMRU()
		{
			int i = -1;
			foreach (string strFile in GlobalOptions.Instance.ReadStickyMRUList())
			{
				i++;
				ToolStripMenuItem objItem = new ToolStripMenuItem();
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
				}

				objItem.Visible = true;
				objItem.Text = strFile;
				mnuFileMRUSeparator.Visible = true;
			}

			// Hide any unused items.
			for (int x = i + 1; x <= 10; x++)
			{
				ToolStripMenuItem objItem = new ToolStripMenuItem();
				switch (x)
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
				}

				objItem.Visible = false;
			}

			i = -1;
			foreach (string strFile in GlobalOptions.Instance.ReadMRUList())
			{
				i++;
				ToolStripMenuItem objItem = new ToolStripMenuItem();
				switch (i)
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
				}

				objItem.Visible = true;
				if (i == 9)
					objItem.Text = "1&0 " + strFile;
				else
					objItem.Text = "&" + (i + 1).ToString() + " " + strFile;
				mnuFileMRUSeparator.Visible = true;
			}

			// Hide any unused items.
			for (int x = i + 1; x < 10; x++)
			{
				ToolStripMenuItem objItem = new ToolStripMenuItem();
				switch (x)
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
				}

				objItem.Visible = false;
			}
		}

		private void objCareer_DiceRollerOpened(Object sender)
		{
			SkillControl objControl = (SkillControl)sender;

			if (GlobalOptions.Instance.SingleDiceRoller)
			{
				if (_frmRoller == null)
				{
					frmDiceRoller frmRoller = new frmDiceRoller(this, objControl.SkillObject.CharacterObject.Qualities, objControl.SkillObject.TotalRating);
					_frmRoller = frmRoller;
					frmRoller.Show();
				}
				else
				{
					_frmRoller.Dice = objControl.SkillObject.TotalRating;
					_frmRoller.Qualities = objControl.SkillObject.CharacterObject.Qualities;
					_frmRoller.Focus();
				}
			}
			else
			{
				frmDiceRoller frmRoller = new frmDiceRoller(this, objControl.SkillObject.CharacterObject.Qualities, objControl.SkillObject.TotalRating);
				frmRoller.Show();
			}
		}

		private void objCareer_DiceRollerOpenedInt(Character objCharacter, int intDice)
		{
			if (GlobalOptions.Instance.SingleDiceRoller)
			{
				if (_frmRoller == null)
				{
					frmDiceRoller frmRoller = new frmDiceRoller(this, objCharacter.Qualities, intDice);
					_frmRoller = frmRoller;
					frmRoller.Show();
				}
				else
				{
					_frmRoller.Dice = intDice;
					_frmRoller.Qualities = objCharacter.Qualities;
					_frmRoller.Focus();
				}
			}
			else
			{
				frmDiceRoller frmRoller = new frmDiceRoller(this, objCharacter.Qualities, intDice);
				frmRoller.Show();
			}
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
        #endregion

        private void mnuGMDashboard_Click(object sender, EventArgs e)
        {
            dashboardToolStripMenuItem_Click(sender,e);
        }

        private void mnuPlayerDashboard_Click(object sender, EventArgs e)
        {
            frmPlayerDashboard.Instance.Show();
        }
    }
}