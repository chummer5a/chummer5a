using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
//using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace Chummer
{
	public partial class frmOptions : Form
	{
		private readonly CharacterOptions _objOptions = new CharacterOptions();
		private CommonFunctions _objFunctions = new CommonFunctions();
		private bool _blnSkipRefresh = false;

        #region Form Events
        public frmOptions()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

            // Populate the Build Method list.
            List<ListItem> lstBuildMethod = new List<ListItem>();

            ListItem objKarma = new ListItem();
            objKarma.Value = "Karma";
            objKarma.Name = LanguageManager.Instance.GetString("String_Karma");

            ListItem objPriority = new ListItem();
            objPriority.Value = "Priority";
            objPriority.Name = LanguageManager.Instance.GetString("String_Priority");

            ListItem objSumtoTen = new ListItem();
            objSumtoTen.Value = "SumtoTen";
            objSumtoTen.Name = LanguageManager.Instance.GetString("String_SumtoTen");

            lstBuildMethod.Add(objSumtoTen);
            lstBuildMethod.Add(objKarma);
            lstBuildMethod.Add(objPriority);
            cboBuildMethod.DataSource = lstBuildMethod;
            cboBuildMethod.ValueMember = "Value";
            cboBuildMethod.DisplayMember = "Name";

            // Populate the Essence Decimals list.
            List<ListItem> lstDecimals = new List<ListItem>();
            ListItem objTwo = new ListItem();
            objTwo.Value = "2";
            objTwo.Name = "2";

            ListItem objFour = new ListItem();
            objFour.Value = "4";
            objFour.Name = "4";

            lstDecimals.Add(objTwo);
            lstDecimals.Add(objFour);
            cboEssenceDecimals.DataSource = lstDecimals;
            cboEssenceDecimals.ValueMember = "Value";
            cboEssenceDecimals.DisplayMember = "Name";



            // Populate the Limb Count list.
            List<ListItem> lstLimbCount = new List<ListItem>();
            ListItem objLimbCount6 = new ListItem();
            objLimbCount6.Value = "all";
            objLimbCount6.Name = LanguageManager.Instance.GetString("String_LimbCount6");

            ListItem objLimbCount5Torso = new ListItem();
            objLimbCount5Torso.Value = "torso";
            objLimbCount5Torso.Name = LanguageManager.Instance.GetString("String_LimbCount5Torso");

            ListItem objLimbCount5Skull = new ListItem();
            objLimbCount5Skull.Value = "skull";
            objLimbCount5Skull.Name = LanguageManager.Instance.GetString("String_LimbCount5Skull");

            lstLimbCount.Add(objLimbCount6);
            lstLimbCount.Add(objLimbCount5Torso);
            lstLimbCount.Add(objLimbCount5Skull);
            cboLimbCount.DataSource = lstLimbCount;
            cboLimbCount.ValueMember = "Value";
            cboLimbCount.DisplayMember = "Name";

            lblMetatypeCostsKarma.Left = chkMetatypeCostsKarma.Left + chkMetatypeCostsKarma.Width + 6;
            nudMetatypeCostsKarmaMultiplier.Left = lblMetatypeCostsKarma.Left + lblMetatypeCostsKarma.Width + 6;

            tipTooltip.SetToolTip(chkKnucks, _objFunctions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsKnucks"), 50));
            tipTooltip.SetToolTip(chkIgnoreArt, _objFunctions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsIgnoreArt"), 50));
            tipTooltip.SetToolTip(chkCyberlegMovement, _objFunctions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsCyberlegMovement"), 50));
            tipTooltip.SetToolTip(chkDontDoubleQualities, _objFunctions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsDontDoubleQualities"), 50));
            tipTooltip.SetToolTip(chkUsePointsOnBrokenGroups, _objFunctions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsUsePointsOnBrokenGroups"), 50));
            tipTooltip.SetToolTip(chkAllowInitiation, _objFunctions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsAllowInitiation"), 50));

            MoveControls();
        }

        private void frmOptions_Load(object sender, EventArgs e)
        {
            // Populate the list of Settings.
            List<ListItem> lstSettings = new List<ListItem>();
            foreach (string strFileName in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "settings"), "*.xml"))
            {
                // Remove the path from the file name.
                string strSettingsFile = strFileName;
                strSettingsFile = strSettingsFile.Replace(Path.Combine(Environment.CurrentDirectory, "settings"), string.Empty);
                strSettingsFile = strSettingsFile.Replace(Path.DirectorySeparatorChar, ' ').Trim();

                // Load the file so we can get the Setting name.
                XmlDocument objXmlSetting = new XmlDocument();
                objXmlSetting.Load(strFileName);
                string strSettingsName = objXmlSetting.SelectSingleNode("/settings/name").InnerText;

                ListItem objItem = new ListItem();
                objItem.Value = strSettingsFile;
                objItem.Name = strSettingsName;

                lstSettings.Add(objItem);
            }
            cboSetting.DataSource = lstSettings;
            cboSetting.ValueMember = "Value";
            cboSetting.DisplayMember = "Name";
            cboSetting.SelectedIndex = 0;

            //tabControl1.TabPages.RemoveAt(1);
            // tabControl1.TabPages.RemoveAt(1);

            // Attempt to make default.xml the default one. If it could not be found in the list, select the first item instead.
            cboSetting.SelectedIndex = cboSetting.FindStringExact("Default Settings");
            if (cboSetting.SelectedIndex == -1)
                cboSetting.SelectedIndex = 0;

            bool blnAutomaticUpdates = false;
            try
            {
                blnAutomaticUpdates = GlobalOptions.Instance.AutomaticUpdate;
            }
            catch
            {
            }
            chkAutomaticUpdate.Checked = blnAutomaticUpdates;

            bool blnUseLogging = false;
            try
            {
                blnUseLogging = GlobalOptions.Instance.UseLogging;
            }
            catch
            {
            }
            chkUseLogging.Checked = blnUseLogging;
            chkLifeModule.Checked = GlobalOptions.Instance.LifeModuleEnabled;
            bool blnLocalisedUpdatesOnly = false;
            try
            {
                blnLocalisedUpdatesOnly = GlobalOptions.Instance.LocalisedUpdatesOnly;
            }
            catch
            {
            }
            chkLocalisedUpdatesOnly.Checked = blnLocalisedUpdatesOnly;

            bool blnStartupFullscreen = false;
            try
            {
                blnStartupFullscreen = GlobalOptions.Instance.StartupFullscreen;
            }
            catch
            {
            }
            chkStartupFullscreen.Checked = blnStartupFullscreen;

            bool blnSingleDiceRoller = true;
            try
            {
                blnSingleDiceRoller = GlobalOptions.Instance.SingleDiceRoller;
            }
            catch
            {
            }
            chkSingleDiceRoller.Checked = blnSingleDiceRoller;

            bool blnDatesIncludeTime = true;
            try
            {
                blnDatesIncludeTime = GlobalOptions.Instance.DatesIncludeTime;
            }
            catch
            {
            }
            chkDatesIncludeTime.Checked = blnDatesIncludeTime;

            bool blnPrintToFileFirst = false;
            try
            {
                blnPrintToFileFirst = GlobalOptions.Instance.PrintToFileFirst;
            }
            catch
            {
            }
            chkPrintToFileFirst.Checked = blnPrintToFileFirst;

            bool blnOpenPDFsAsURLs = false;
            try
            {
                blnOpenPDFsAsURLs = GlobalOptions.Instance.OpenPDFsAsURLs;
            }
            catch
            {
            }
            chkOpenPDFsAsURLs.Checked = blnOpenPDFsAsURLs;

            chkLicenseEachRestrictedItem.Checked = _objOptions.LicenseRestricted;

            txtPDFAppPath.Text = GlobalOptions.Instance.PDFAppPath;
            txtURLAppPath.Text = GlobalOptions.Instance.URLAppPath;

            // Populate the Language List.
            string strPath = Path.Combine(Environment.CurrentDirectory, "lang");
            List<ListItem> lstLanguages = new List<ListItem>();
            foreach (string strFile in Directory.GetFiles(strPath, "*.xml"))
            {
                XmlDocument objXmlDocument = new XmlDocument();
                try
                {
                    objXmlDocument.Load(strFile);
                    ListItem objItem = new ListItem();
                    string strFileName = strFile.Replace(strPath, string.Empty).Replace(".xml", string.Empty).Replace(Path.DirectorySeparatorChar, ' ').Trim();
                    objItem.Value = strFileName;
                    objItem.Name = objXmlDocument.SelectSingleNode("/chummer/name").InnerText;
                    lstLanguages.Add(objItem);
                }
                catch
                {
                }
            }

            SortListItem objSort = new SortListItem();
            lstLanguages.Sort(objSort.Compare);
            cboLanguage.DataSource = lstLanguages;
            cboLanguage.ValueMember = "Value";
            cboLanguage.DisplayMember = "Name";
            try
            {
                cboLanguage.SelectedValue = GlobalOptions.Instance.Language;
            }
            catch
            {
            }
            if (cboLanguage.SelectedIndex == -1)
                cboLanguage.SelectedValue = "en-us";

            List<ListItem> lstFiles = new List<ListItem>();
            // Populate the XSLT list with all of the XSL files found in the sheets directory.
            foreach (string strFile in Directory.GetFiles(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "sheets"))
            {
                // Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets (hidden because they are partial templates that cannot be used on their own).
                if (!strFile.EndsWith(".xslt") && strFile.EndsWith(".xsl"))
                {
                    string strFileName = strFile.Replace(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "sheets" + Path.DirectorySeparatorChar, string.Empty).Replace(".xsl", string.Empty);
                    ListItem objItem = new ListItem();
                    objItem.Value = strFileName;
                    objItem.Name = strFileName;
                    lstFiles.Add(objItem);

                    //cboXSLT.Items.Add(strFileName);
                }
            }

            try
            {
                // Populate the XSL list with all of the XSL files found in the sheets\[language] directory.
                if (GlobalOptions.Instance.Language != "en-us")
                {
                    XmlDocument objLanguageDocument = LanguageManager.Instance.XmlDoc;
                    string strLanguage = objLanguageDocument.SelectSingleNode("/chummer/name").InnerText;

                    foreach (string strFile in Directory.GetFiles(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "sheets" + Path.DirectorySeparatorChar + GlobalOptions.Instance.Language))
                    {
                        // Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets (hidden because they are partial templates that cannot be used on their own).
                        if (!strFile.EndsWith(".xslt") && strFile.EndsWith(".xsl"))
                        {
                            string strFileName = strFile.Replace(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "sheets" + Path.DirectorySeparatorChar + GlobalOptions.Instance.Language + Path.DirectorySeparatorChar, string.Empty).Replace(".xsl", string.Empty);
                            ListItem objItem = new ListItem();
                            objItem.Value = GlobalOptions.Instance.Language + Path.DirectorySeparatorChar + strFileName;
                            objItem.Name = strLanguage + ": " + strFileName;
                            lstFiles.Add(objItem);
                        }
                    }
                }
            }
            catch
            {
            }

            try
            {
                // Populate the XSLT list with all of the XSL files found in the sheets\omae directory.
                foreach (string strFile in Directory.GetFiles(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "sheets" + Path.DirectorySeparatorChar + "omae"))
                {
                    // Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets (hidden because they are partial templates that cannot be used on their own).
                    if (!strFile.EndsWith(".xslt") && strFile.EndsWith(".xsl"))
                    {
                        string strFileName = strFile.Replace(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "sheets" + Path.DirectorySeparatorChar + "omae" + Path.DirectorySeparatorChar, string.Empty).Replace(".xsl", string.Empty);
                        ListItem objItem = new ListItem();
                        objItem.Value = "omae" + Path.DirectorySeparatorChar + strFileName;
                        objItem.Name = LanguageManager.Instance.GetString("Menu_Main_Omae") + ": " + strFileName;
                        lstFiles.Add(objItem);
                    }
                }
            }
            catch
            {
            }

            cboXSLT.ValueMember = "Value";
            cboXSLT.DisplayMember = "Name";
            cboXSLT.DataSource = lstFiles;

            if (GlobalOptions.Instance.DefaultCharacterSheet == "" || GlobalOptions.Instance.DefaultCharacterSheet == null)
            {
                GlobalOptions.Instance.DefaultCharacterSheet = "Shadowrun 5";
            }
            cboXSLT.SelectedValue = GlobalOptions.Instance.DefaultCharacterSheet;
        }
        #endregion

        #region Control Events
        private void cmdOK_Click(object sender, EventArgs e)
        {
            // Make sure the current Setting has a name.
            if (txtSettingName.Text.Trim() == "")
            {
                MessageBox.Show("You must give your Settings a name.", "Chummer Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSettingName.Focus();
                return;
            }

            SaveRegistrySettings();

            // Set Settings file value.
            // Build the list of Books (SR5 is always included as it's required for everything).
            _objOptions.Books.Clear();

            bool blnSR5Included = false;
            foreach (TreeNode objNode in treSourcebook.Nodes)
            {
                if (objNode.Checked)
                {
                    _objOptions.Books.Add(objNode.Tag.ToString());
                    if (objNode.Tag.ToString() == "SR5")
                        blnSR5Included = true;
                }
            }

            // If the SR5 book was somehow missed, add it back.
            if (!blnSR5Included)
                _objOptions.Books.Add("SR5");

            _objOptions.AllowCustomTransgenics = chkAllowCustomTransgenics.Checked;
            _objOptions.AllowInitiationInCreateMode = chkAllowInitiation.Checked;
            _objOptions.AllowSkillDiceRolling = chkAllowSkillDiceRolling.Checked;
            _objOptions.AllowSkillRegrouping = chkAllowSkillRegrouping.Checked;
            _objOptions.AlternateMatrixAttribute = chkAlternateMatrixAttribute.Checked;
            _objOptions.AlternateComplexFormCost = chkAlternateComplexFormCost.Checked;
            _objOptions.ArmorDegradation = chkArmorDegradation.Checked;
            _objOptions.AutomaticCopyProtection = chkAutomaticCopyProtection.Checked;
            _objOptions.AutomaticRegistration = chkAutomaticRegistration.Checked;
            _objOptions.BreakSkillGroupsInCreateMode = chkBreakSkillGroupsInCreateMode.Checked;
            _objOptions.CalculateCommlinkResponse = chkCalculateCommlinkResponse.Checked;
            _objOptions.CapSkillRating = chkCapSkillRating.Checked;
            _objOptions.ConfirmDelete = chkConfirmDelete.Checked;
            _objOptions.ConfirmKarmaExpense = chkConfirmKarmaExpense.Checked;
            _objOptions.CreateBackupOnCareer = chkCreateBackupOnCareer.Checked;
            _objOptions.CyberlegMovement = chkCyberlegMovement.Checked;
            _objOptions.DontDoubleQualities = chkDontDoubleQualities.Checked;
            _objOptions.EnforceCapacity = chkEnforceCapacity.Checked;
            _objOptions.EnforceMaximumSkillRatingModifier = chkEnforceSkillMaximumModifiedRating.Checked;
            _objOptions.ErgonomicProgramLimit = chkErgonomicProgramLimit.Checked;
            _objOptions.EssenceDecimals = Convert.ToInt32(cboEssenceDecimals.SelectedValue);
            _objOptions.ExceedNegativeQualities = chkExceedNegativeQualities.Checked;
                if (chkExceedNegativeQualities.Checked)
                    chkExceedNegativeQualitiesLimit.Enabled = true;
            _objOptions.ExceedNegativeQualitiesLimit = chkExceedNegativeQualitiesLimit.Checked;
            _objOptions.ExceedPositiveQualities = chkExceedPositiveQualities.Checked;
            _objOptions.ExtendAnyDetectionSpell = chkExtendAnyDetectionSpell.Checked;
            _objOptions.FreeContactsMultiplier = Convert.ToInt32(nudContactMultiplier.Value);
            _objOptions.FreeContactsMultiplierEnabled = chkContactMultiplier.Checked;
                if (chkContactMultiplier.Checked)
                    nudContactMultiplier.Enabled = true;
            _objOptions.FreeKarmaContacts = chkFreeKarmaContacts.Checked;
            _objOptions.FreeKarmaKnowledge = chkFreeKarmaKnowledge.Checked;
            _objOptions.FreeKnowledgeMultiplierEnabled = chkKnowledgeMultiplier.Checked;
                if (chkKnowledgeMultiplier.Checked)
                    chkKnowledgeMultiplier.Enabled = true;
            _objOptions.FreeKnowledgeMultiplier = Convert.ToInt32(nudKnowledgeMultiplier.Value);
            _objOptions.IgnoreArt = chkIgnoreArt.Checked;
            _objOptions.KnucksUseUnarmed = chkKnucks.Checked;
            _objOptions.LicenseRestricted = chkLicenseEachRestrictedItem.Checked;
            _objOptions.MaximumArmorModifications = chkMaximumArmorModifications.Checked;
            _objOptions.MetatypeCostsKarma = chkMetatypeCostsKarma.Checked;
            _objOptions.MetatypeCostsKarmaMultiplier = Convert.ToInt32(nudMetatypeCostsKarmaMultiplier.Value);
            _objOptions.MoreLethalGameplay = chkMoreLethalGameplay.Checked;
            _objOptions.NoSingleArmorEncumbrance = chkNoSingleArmorEncumbrance.Checked;
            _objOptions.NuyenPerBP = Convert.ToInt32(nudKarmaNuyenPer.Value);
            _objOptions.PrintExpenses = chkPrintExpenses.Checked;
            _objOptions.PrintNotes = chkPrintNotes.Checked;
            _objOptions.PrintSkillsWithZeroRating = chkPrintSkillsWithZeroRating.Checked;
            _objOptions.RestrictRecoil = chkRestrictRecoil.Checked;
            _objOptions.StrengthAffectsRecoil = Convert.ToBoolean(chkStrengthAffectsRecoil.Checked);
            _objOptions.UseCalculatedVehicleSensorRatings = chkUseCalculatedVehicleSensorRatings.Checked;
            _objOptions.UsePointsOnBrokenGroups = chkUsePointsOnBrokenGroups.Checked;

            switch (cboLimbCount.SelectedValue.ToString())
            {
                case "torso":
                    _objOptions.LimbCount = 5;
                    _objOptions.ExcludeLimbSlot = "skull";
                    break;
                case "skull":
                    _objOptions.LimbCount = 5;
                    _objOptions.ExcludeLimbSlot = "torso";
                    break;
                default:
                    _objOptions.LimbCount = 6;
                    _objOptions.ExcludeLimbSlot = "";
                    break;
            }

            // Karma options.
            _objOptions.KarmaAttribute = Convert.ToInt32(nudKarmaAttribute.Value);
            _objOptions.KarmaQuality = Convert.ToInt32(nudKarmaQuality.Value);
            _objOptions.KarmaSpecialization = Convert.ToInt32(nudKarmaSpecialization.Value);
            _objOptions.KarmaNewKnowledgeSkill = Convert.ToInt32(nudKarmaNewKnowledgeSkill.Value);
            _objOptions.KarmaNewActiveSkill = Convert.ToInt32(nudKarmaNewActiveSkill.Value);
            _objOptions.KarmaNewSkillGroup = Convert.ToInt32(nudKarmaNewSkillGroup.Value);
            _objOptions.KarmaImproveKnowledgeSkill = Convert.ToInt32(nudKarmaImproveKnowledgeSkill.Value);
            _objOptions.KarmaImproveActiveSkill = Convert.ToInt32(nudKarmaImproveActiveSkill.Value);
            _objOptions.KarmaImproveSkillGroup = Convert.ToInt32(nudKarmaImproveSkillGroup.Value);
            _objOptions.KarmaSpell = Convert.ToInt32(nudKarmaSpell.Value);
            _objOptions.KarmaNewComplexForm = Convert.ToInt32(nudKarmaNewComplexForm.Value);
            _objOptions.KarmaImproveComplexForm = Convert.ToInt32(nudKarmaImproveComplexForm.Value);
            _objOptions.KarmaMetamagic = Convert.ToInt32(nudKarmaMetamagic.Value);
            _objOptions.KarmaNuyenPer = Convert.ToInt32(nudKarmaNuyenPer.Value);
            _objOptions.KarmaContact = Convert.ToInt32(nudKarmaContact.Value);
            _objOptions.KarmaCarryover = Convert.ToInt32(nudKarmaCarryover.Value);
            _objOptions.KarmaSpirit = Convert.ToInt32(nudKarmaSpirit.Value);
            _objOptions.KarmaManeuver = Convert.ToInt32(nudKarmaManeuver.Value);
            _objOptions.KarmaInitiation = Convert.ToInt32(nudKarmaInitiation.Value);
            _objOptions.KarmaComplexFormOption = Convert.ToInt32(nudKarmaComplexFormOption.Value);
            _objOptions.KarmaComplexFormSkillsoft = Convert.ToInt32(nudKarmaComplexFormSkillsoft.Value);
            _objOptions.KarmaJoinGroup = Convert.ToInt32(nudKarmaJoinGroup.Value);
            _objOptions.KarmaLeaveGroup = Convert.ToInt32(nudKarmaLeaveGroup.Value);

            // Build Priority options.
            _objOptions.MayBuyQualities = chkMayBuyQualities.Checked;
            _objOptions.UseContactPoints = chkContactPoints.Checked;

            // Foci options.
            //_objOptions.KarmaAlchemicalFocus = Convert.ToInt32(nudKarmaAnchoringFocus.Value);
            //_objOptions.KarmaBanishingFocus = Convert.ToInt32(nudKarmaBanishingFocus.Value);
            //_objOptions.KarmaBindingFocus = Convert.ToInt32(nudKarmaBindingFocus.Value);
            //_objOptions.KarmaCenteringFocus = Convert.ToInt32(nudKarmaCenteringFocus.Value);
            //_objOptions.KarmaCounterspellingFocus = Convert.ToInt32(nudKarmaCounterspellingFocus.Value);
            //_objOptions.KarmaDiviningFocus = Convert.ToInt32(nudKarmaDiviningFocus.Value);
            //_objOptions.KarmaDowsingFocus = Convert.ToInt32(nudKarmaDowsingFocus.Value);
            //_objOptions.KarmaInfusionFocus = Convert.ToInt32(nudKarmaInfusionFocus.Value);
            //_objOptions.KarmaMaskingFocus = Convert.ToInt32(nudKarmaMaskingFocus.Value);
            //_objOptions.KarmaPowerFocus = Convert.ToInt32(nudKarmaPowerFocus.Value);
            //_objOptions.KarmaShieldingFocus = Convert.ToInt32(nudKarmaShieldingFocus.Value);
            //_objOptions.KarmaSpellcastingFocus = Convert.ToInt32(nudKarmaSpellcastingFocus.Value);
            //_objOptions.KarmaSummoningFocus = Convert.ToInt32(nudKarmaSummoningFocus.Value);
            //_objOptions.KarmaSustainingFocus = Convert.ToInt32(nudKarmaSustainingFocus.Value);
            //_objOptions.KarmaSymbolicLinkFocus = Convert.ToInt32(nudKarmaSymbolicLinkFocus.Value);
            //_objOptions.KarmaWeaponFocus = Convert.ToInt32(nudKarmaWeaponFocus.Value);

            // Build method options.
            _objOptions.BuildMethod = cboBuildMethod.SelectedValue.ToString();
            _objOptions.BuildPoints = Convert.ToInt32(nudBP.Value);
            _objOptions.Availability = Convert.ToInt32(nudMaxAvail.Value);

            _objOptions.Name = txtSettingName.Text;

            _objOptions.Save();

            Form fc = Application.OpenForms["frmCreate"];
            if (fc != null)
            {
                if (MessageBox.Show(LanguageManager.Instance.GetString("Message_Options_CloseForms"), LanguageManager.Instance.GetString("MessageTitle_Options_CloseForms"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    fc = Application.OpenForms["frmCreate"]; if (fc != null) fc.Close();
                }
            }
            this.DialogResult = DialogResult.OK;
        }

        private void cboBuildMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboBuildMethod.SelectedValue.ToString() == "BP")
                nudBP.Value = 400;
            else
                nudBP.Value = 750;
        }

        private void cboSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListItem objItem = (ListItem)cboSetting.SelectedItem;
            if (!objItem.Value.Contains(".xml"))
                return;

            _objOptions.Load(objItem.Value);
            PopulateOptions();
        }

        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboLanguage.SelectedValue.ToString() == "en-us")
            {
                cmdVerify.Enabled = false;
                cmdVerifyData.Enabled = false;
            }
            else
            {
                cmdVerify.Enabled = true;
                cmdVerifyData.Enabled = true;
            }
        }

        private void cmdVerify_Click(object sender, EventArgs e)
        {
            LanguageManager.Instance.VerifyStrings(cboLanguage.SelectedValue.ToString());
        }

        private void cmdVerifyData_Click(object sender, EventArgs e)
        {
            // Build a list of Sourcebooks that will be passed to the Verify method.
            // This is done since not all of the books are available in every language or the user may only wish to verify the content of certain books.
            List<string> lstBooks = new List<string>();
            //lstBooks.Add("SR5");

            bool blnSR5Included = false;
            foreach (TreeNode objNode in treSourcebook.Nodes)
            {
                if (objNode.Checked)
                {
                    lstBooks.Add(objNode.Tag.ToString());
                    if (objNode.Tag.ToString() == "SR5")
                        blnSR5Included = true;
                }
            }

            // If the SR5 book was somehow missed, add it back.
            if (!blnSR5Included)
                _objOptions.Books.Add("SR5");

            XmlManager.Instance.Verify(cboLanguage.SelectedValue.ToString(), lstBooks);

            string strFilePath = Path.Combine(Environment.CurrentDirectory, "lang");
            strFilePath = Path.Combine(strFilePath, "results_" + cboLanguage.SelectedValue + ".xml");
            MessageBox.Show("Results were written to " + strFilePath, "Validation Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void chkExceedNegativeQualities_CheckedChanged(object sender, EventArgs e)
        {
            chkExceedNegativeQualitiesLimit.Enabled = chkExceedNegativeQualities.Checked;
            if (!chkExceedNegativeQualitiesLimit.Enabled)
                chkExceedNegativeQualitiesLimit.Checked = false;
        }

        private void chkContactMultiplier_CheckedChanged(object sender, EventArgs e)
        {
            nudContactMultiplier.Enabled = chkContactMultiplier.Checked;
            if (!chkContactMultiplier.Checked)
            {
                nudContactMultiplier.Value = 3;
                nudContactMultiplier.Enabled = false;
            }
        }

        private void chkKnowledgeMultiplier_CheckedChanged(object sender, EventArgs e)
        {
            nudKnowledgeMultiplier.Enabled = chkKnowledgeMultiplier.Checked;
            if (!chkKnowledgeMultiplier.Checked)
            {
                nudKnowledgeMultiplier.Value = 2;
                nudKnowledgeMultiplier.Enabled = false;
            }
        }
        private void cmdRestoreDefaultsBP_Click(object sender, EventArgs e)
        {
            // Verify that the user wants to reset these values.
            if (MessageBox.Show(LanguageManager.Instance.GetString("Message_Options_RestoreDefaults"), LanguageManager.Instance.GetString("MessageTitle_Options_RestoreDefaults"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
        }

        private void cmdRestoreDefaultsKarma_Click(object sender, EventArgs e)
        {
            // Verify that the user wants to reset these values.
            if (MessageBox.Show(LanguageManager.Instance.GetString("Message_Options_RestoreDefaults"), LanguageManager.Instance.GetString("MessageTitle_Options_RestoreDefaults"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            // Restore the default Karma values.
            nudKarmaSpecialization.Value = 7;
            nudKarmaNewKnowledgeSkill.Value = 1;
            nudKarmaNewActiveSkill.Value = 2;
            nudKarmaNewSkillGroup.Value = 5;
            nudKarmaImproveKnowledgeSkill.Value = 1;
            nudKarmaImproveActiveSkill.Value = 2;
            nudKarmaImproveSkillGroup.Value = 5;
            nudKarmaAttribute.Value = 5;
            nudKarmaQuality.Value = 1;
            nudKarmaSpell.Value = 5;
            nudKarmaNewComplexForm.Value = 4;
            nudKarmaImproveComplexForm.Value = 1;
            nudKarmaComplexFormOption.Value = 2;
            nudKarmaComplexFormSkillsoft.Value = 1;
            nudKarmaSpirit.Value = 2;
            nudKarmaManeuver.Value = 4;
            nudKarmaNuyenPer.Value = 2000;
            nudKarmaContact.Value = 1;
            nudKarmaCarryover.Value = 7;
            nudKarmaInitiation.Value = 3;
            nudKarmaMetamagic.Value = 15;
            nudKarmaJoinGroup.Value = 5;
            nudKarmaLeaveGroup.Value = 1;

            // Restore the default Karma Foci values.
            nudKarmaAnchoringFocus.Value = 6;
            nudKarmaBanishingFocus.Value = 3;
            nudKarmaBindingFocus.Value = 3;
            nudKarmaCenteringFocus.Value = 6;
            nudKarmaCounterspellingFocus.Value = 3;
            nudKarmaDiviningFocus.Value = 6;
            nudKarmaDowsingFocus.Value = 6;
            nudKarmaInfusionFocus.Value = 3;
            nudKarmaMaskingFocus.Value = 6;
            nudKarmaPowerFocus.Value = 6;
            nudKarmaShieldingFocus.Value = 6;
            nudKarmaSpellcastingFocus.Value = 4;
            nudKarmaSummoningFocus.Value = 4;
            nudKarmaSustainingFocus.Value = 2;
            nudKarmaSymbolicLinkFocus.Value = 1;
            nudKarmaWeaponFocus.Value = 3;
        }

        private void cmdPDFAppPath_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                txtPDFAppPath.Text = openFileDialog.FileName;
        }

        private void cmdURLAppPath_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                txtURLAppPath.Text = openFileDialog.FileName;
        }
        private void cmdPDFLocation_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                SourcebookInfo objFoundSource = new SourcebookInfo();
                bool blnFound = false;

                // Find the selected item in the Sourcebook List.
                foreach (SourcebookInfo objSource in GlobalOptions.Instance.SourcebookInfo)
                {
                    if (objSource.Code == treSourcebook.SelectedNode.Tag.ToString())
                    {
                        objFoundSource = objSource;
                        blnFound = true;
                        break;
                    }
                }

                txtPDFLocation.Text = openFileDialog.FileName;
                objFoundSource.Path = openFileDialog.FileName;

                // If the Sourcebook was not found in the options, add it.
                if (!blnFound)
                {
                    objFoundSource.Code = treSourcebook.SelectedNode.Tag.ToString();
                    GlobalOptions.Instance.SourcebookInfo.Add(objFoundSource);
                }
            }
        }

        private void treSourcebook_AfterSelect(object sender, TreeViewEventArgs e)
        {
            cmdPDFLocation.Enabled = true;
            nudPDFOffset.Enabled = true;
            cmdPDFTest.Enabled = true;

            _blnSkipRefresh = true;
            txtPDFLocation.Text = "";
            nudPDFOffset.Value = 0;
            _blnSkipRefresh = false;

            // Find the selected item in the Sourcebook List.
            foreach (SourcebookInfo objSource in GlobalOptions.Instance.SourcebookInfo)
            {
                if (objSource.Code == treSourcebook.SelectedNode.Tag.ToString())
                {
                    txtPDFLocation.Text = objSource.Path;
                    nudPDFOffset.Value = objSource.Offset;
                }
            }
        }

        private void nudPDFOffset_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            SourcebookInfo objFoundSource = new SourcebookInfo();
            bool blnFound = false;

            // Find the selected item in the Sourcebook List.
            foreach (SourcebookInfo objSource in GlobalOptions.Instance.SourcebookInfo)
            {
                if (objSource.Code == treSourcebook.SelectedNode.Tag.ToString())
                {
                    objFoundSource = objSource;
                    blnFound = true;
                    break;
                }
            }

            objFoundSource.Offset = Convert.ToInt32(nudPDFOffset.Value);

            // If the Sourcebook was not found in the options, add it.
            if (!blnFound)
            {
                objFoundSource.Code = treSourcebook.SelectedNode.Tag.ToString();
                GlobalOptions.Instance.SourcebookInfo.Add(objFoundSource);
            }
        }

        private void treSourcebook_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Tag.ToString() == "SR5")
                e.Cancel = true;
        }

        private void cmdPDFTest_Click(object sender, EventArgs e)
        {
            if (txtPDFLocation.Text == string.Empty)
                return;

            SaveRegistrySettings();

            CommonFunctions objCommon = new CommonFunctions(null);
            objCommon.OpenPDF(treSourcebook.SelectedNode.Tag.ToString() + " 5");
        }
        #endregion

        #region Methods
        private void MoveControls()
        {
            int intWidth = 0;

            cboSetting.Left = lblSetting.Left + lblSetting.Width + 6;
            lblSettingName.Left = cboSetting.Left + cboSetting.Width + 6;
            txtSettingName.Left = lblSettingName.Left + lblSettingName.Width + 6;
            cboLimbCount.Left = lblLimbCount.Left + lblLimbCount.Width + 6;
            nudNuyenPerBP.Left = lblNuyenPerBP.Left + lblNuyenPerBP.Width + 6;
            lblMetatypeCostsKarma.Left = chkMetatypeCostsKarma.Left + chkMetatypeCostsKarma.Width;
            nudMetatypeCostsKarmaMultiplier.Left = lblMetatypeCostsKarma.Left + lblMetatypeCostsKarma.Width;
            cboEssenceDecimals.Left = lblEssenceDecimals.Left + lblEssenceDecimals.Width + 6;

            txtPDFAppPath.Left = lblPDFAppPath.Left + lblPDFAppPath.Width + 6;
            cmdPDFAppPath.Left = txtPDFAppPath.Left + txtPDFAppPath.Width + 6;
            cmdPDFTest.Left = nudPDFOffset.Left + nudPDFOffset.Width + 6;

            intWidth = Math.Max(lblPDFLocation.Width, lblPDFOffset.Width);
            txtPDFLocation.Left = lblPDFLocation.Left + intWidth + 6;
            cmdPDFLocation.Left = txtPDFLocation.Left + txtPDFLocation.Width + 6;
            nudPDFOffset.Left = lblPDFOffset.Left + intWidth + 6;

            intWidth = Math.Max(lblLanguage.Width, lblXSLT.Width);
            cboLanguage.Left = lblLanguage.Left + intWidth + 6;
            cmdVerify.Left = cboLanguage.Left + cboLanguage.Width + 6;
            cmdVerifyData.Left = cmdVerify.Left + cmdVerify.Width + 6;
            cboXSLT.Left = lblXSLT.Left + intWidth + 6;

            // Karma fields.
            nudKarmaSpecialization.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            nudKarmaNewKnowledgeSkill.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            nudKarmaNewActiveSkill.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            nudKarmaNewSkillGroup.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            nudKarmaImproveKnowledgeSkill.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            lblKarmaImproveKnowledgeSkillExtra.Left = nudKarmaImproveKnowledgeSkill.Left + nudKarmaImproveKnowledgeSkill.Width + 6;
            nudKarmaImproveActiveSkill.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            lblKarmaImproveActiveSkillExtra.Left = nudKarmaImproveActiveSkill.Left + nudKarmaImproveActiveSkill.Width + 6;
            nudKarmaImproveSkillGroup.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            lblKarmaImproveSkillGroupExtra.Left = nudKarmaImproveSkillGroup.Left + nudKarmaImproveSkillGroup.Width + 6;
            nudKarmaAttribute.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            lblKarmaAttributeExtra.Left = nudKarmaAttribute.Left + nudKarmaAttribute.Width + 6;
            nudKarmaQuality.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            lblKarmaQualityExtra.Left = nudKarmaQuality.Left + nudKarmaQuality.Width + 6;
            nudKarmaSpell.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            nudKarmaNewComplexForm.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            nudKarmaImproveComplexForm.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            lblKarmaImproveComplexFormExtra.Left = nudKarmaImproveComplexForm.Left + nudKarmaImproveComplexForm.Width + 6;
            nudKarmaComplexFormOption.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            lblKarmaComplexFormOptionExtra.Left = nudKarmaComplexFormOption.Left + nudKarmaComplexFormOption.Width + 6;
            nudKarmaComplexFormSkillsoft.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            lblKarmaComplexFormSkillsoftExtra.Left = nudKarmaComplexFormSkillsoft.Left + nudKarmaComplexFormSkillsoft.Width + 6;
            nudKarmaSpirit.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            lblKarmaSpiritExtra.Left = nudKarmaSpirit.Left + nudKarmaSpirit.Width + 6;
            nudKarmaManeuver.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            nudKarmaNuyenPer.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            lblKarmaNuyenPerExtra.Left = nudKarmaNuyenPer.Left + nudKarmaNuyenPer.Width + 6;
            nudKarmaContact.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            lblKarmaContactExtra.Left = nudKarmaContact.Left + nudKarmaContact.Width + 6;
            nudKarmaCarryover.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            lblKarmaCarryoverExtra.Left = nudKarmaCarryover.Left + nudKarmaCarryover.Width + 6;
            nudKarmaInitiation.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            lblKarmaInitiationBracket.Left = nudKarmaInitiation.Left - lblKarmaInitiationBracket.Width;
            lblKarmaInitiationExtra.Left = nudKarmaInitiation.Left + nudKarmaInitiation.Width + 6;

            intWidth = Math.Max(lblKarmaMetamagic.Width, lblKarmaJoinGroup.Width);
            intWidth = Math.Max(intWidth, lblKarmaLeaveGroup.Width);
            intWidth = Math.Max(intWidth, lblKarmaAnchoringFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaBanishingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaBindingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaCenteringFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaCounterspellingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaDiviningFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaDowsingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaInfusionFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaMaskingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaPowerFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaShieldingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaSpellcastingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaSummoningFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaSustainingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaSymbolicLinkFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaWeaponFocus.Width);

            nudKarmaMetamagic.Left = lblKarmaMetamagic.Left + intWidth + 6;
            nudKarmaJoinGroup.Left = nudKarmaMetamagic.Left;
            nudKarmaLeaveGroup.Left = nudKarmaMetamagic.Left;
            nudKarmaAnchoringFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaBanishingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaBindingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaCenteringFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaCounterspellingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaDiviningFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaDowsingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaInfusionFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaMaskingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaPowerFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaShieldingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaSpellcastingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaSummoningFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaSustainingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaSymbolicLinkFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaWeaponFocus.Left = nudKarmaMetamagic.Left;

            lblKarmaAnchoringFocusExtra.Left = nudKarmaAnchoringFocus.Left + nudKarmaAnchoringFocus.Width + 6;
            lblKarmaBanishingFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;
            lblKarmaBindingFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;
            lblKarmaCenteringFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;
            lblKarmaCounterspellingFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;
            lblKarmaDiviningFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;
            lblKarmaDowsingFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;
            lblKarmaInfusionFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;
            lblKarmaMaskingFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;
            lblKarmaPowerFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;
            lblKarmaShieldingFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;
            lblKarmaSpellcastingFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;
            lblKarmaSummoningFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;
            lblKarmaSustainingFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;
            lblKarmaSymbolicLinkFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;
            lblKarmaWeaponFocusExtra.Left = lblKarmaAnchoringFocusExtra.Left;

            // Determine where the widest control ends so we can change the window with to accommodate it.
            foreach (Control objControl in tabGeneral.Controls)
                intWidth = Math.Max(intWidth, objControl.Left + objControl.Width);
            foreach (Control objControl in tabKarmaCosts.Controls)
                intWidth = Math.Max(intWidth, objControl.Left + objControl.Width);
            foreach (Control objControl in tabOptionalRules.Controls)
                intWidth = Math.Max(intWidth, objControl.Left + objControl.Width);
            foreach (Control objControl in tabHouseRules.Controls)
                intWidth = Math.Max(intWidth, objControl.Left + objControl.Width);

            // Change the window size.
            this.Width = intWidth + 29;
            this.Height = tabControl1.Top + tabControl1.Height + cmdOK.Height + 55;
            // Centre the OK button.
            cmdOK.Left = (this.Width / 2) - (cmdOK.Width / 2);
        }

        /// <summary>
        /// Set the values for all of the controls based on the Options for the selected Setting.
        /// </summary>
        private void PopulateOptions()
        {
            // Load the Sourcebook information.
            bool blnChecked = false;

            XmlDocument objXmlDocument = XmlManager.Instance.Load("books.xml");

            // Put the Sourcebooks into a List so they can first be sorted.
            XmlNodeList objXmlBookList = objXmlDocument.SelectNodes("/chummer/books/book");
            treSourcebook.Nodes.Clear();
            foreach (XmlNode objXmlBook in objXmlBookList)
            {
                blnChecked = false;
                if (_objOptions.Books.Contains(objXmlBook["code"].InnerText))
                    blnChecked = true;

                TreeNode objNode = new TreeNode();
                if (objXmlBook["translate"] != null)
                    objNode.Text = objXmlBook["translate"].InnerText;
                else
                    objNode.Text = objXmlBook["name"].InnerText;
                objNode.Tag = objXmlBook["code"].InnerText;
                objNode.Checked = blnChecked;
                treSourcebook.Nodes.Add(objNode);
            }
            treSourcebook.Sort();

            // General Options.
            bool blnConfirmDelete = true;
            try
            {
                blnConfirmDelete = _objOptions.ConfirmDelete;
            }
            catch
            {
            }
            chkConfirmDelete.Checked = blnConfirmDelete;

            bool blnConfirmKarmaExpense = true;
            try
            {
                blnConfirmKarmaExpense = _objOptions.ConfirmKarmaExpense;
            }
            catch
            {
            }
            chkConfirmKarmaExpense.Checked = blnConfirmKarmaExpense;

            bool blnPrintSkillsWithZeroRating = true;
            try
            {
                blnPrintSkillsWithZeroRating = _objOptions.PrintSkillsWithZeroRating;
            }
            catch
            {
            }
            chkPrintSkillsWithZeroRating.Checked = blnPrintSkillsWithZeroRating;

            bool blnMoreLethalGameplay = false;
            try
            {
                blnMoreLethalGameplay = _objOptions.MoreLethalGameplay;
            }
            catch
            {
            }
            chkMoreLethalGameplay.Checked = blnMoreLethalGameplay;

            bool blnSpiritForceBasedOnTotalMAG = false;
            try
            {
                blnSpiritForceBasedOnTotalMAG = _objOptions.SpiritForceBasedOnTotalMAG;
            }
            catch
            {
            }

            bool blnSkillDefaultingIncludesModifiers = false;
            try
            {
                blnSkillDefaultingIncludesModifiers = _objOptions.SkillDefaultingIncludesModifiers;
            }
            catch
            {
            }

            bool blnEnforceSkillMaximumModifiedRating = false;
            try
            {
                blnEnforceSkillMaximumModifiedRating = _objOptions.EnforceMaximumSkillRatingModifier;
            }
            catch
            {
            }
            chkEnforceSkillMaximumModifiedRating.Checked = blnEnforceSkillMaximumModifiedRating;

            bool blnSpecialKarmaCost = false;
            try
            {
                blnSpecialKarmaCost = _objOptions.SpecialKarmaCostBasedOnShownValue;
            }
            catch
            {
            }
            chkSpecialKarmaCost.Checked = blnSpecialKarmaCost;

            bool blnCapSkillRating = false;
            try
            {
                blnCapSkillRating = _objOptions.CapSkillRating;
            }
            catch
            {
            }
            chkCapSkillRating.Checked = blnCapSkillRating;

            bool blnPrintExpenses = false;
            try
            {
                blnPrintExpenses = _objOptions.PrintExpenses;
            }
            catch
            {
            }
            chkPrintExpenses.Checked = blnPrintExpenses;

            bool blnKnucksUseUnarmed = false;
            try
            {
                blnKnucksUseUnarmed = _objOptions.KnucksUseUnarmed;
            }
            catch
            {
            }
            chkKnucks.Checked = blnKnucksUseUnarmed;


            bool blnAllowInitiation = false;
            try
            {
                blnAllowInitiation = _objOptions.AllowInitiationInCreateMode;
            }
            catch
            {
            }
            chkAllowInitiation.Checked = blnAllowInitiation;

            //Free Contacts in Point Buy
            bool blnFreeKarmaContacts = false;
            try
            {
                blnFreeKarmaContacts = _objOptions.FreeKarmaContacts;
            }
            catch
            {
            }
            chkFreeKarmaContacts.Checked = blnFreeKarmaContacts;

            //Free Knowledge Skills in Point Buy
            bool blnFreeKarmaKnowledge = false;
            try
            {
                blnFreeKarmaKnowledge = _objOptions.FreeKarmaKnowledge;
            }
            catch
            {
            }
            chkFreeKarmaKnowledge.Checked = blnFreeKarmaKnowledge;

            //Contact Multiplier Enabled
            bool blnFreeContactMultiplierEnabled = false;
            try
            {
                blnFreeContactMultiplierEnabled = _objOptions.FreeContactsMultiplierEnabled;
            }
            catch
            {
            }
            chkContactMultiplier.Checked = blnFreeContactMultiplierEnabled;
            nudContactMultiplier.Enabled = chkContactMultiplier.Checked;
            nudContactMultiplier.Value = 3;

            //Knowledge Multiplier Enabled
            bool blnFreeKnowledgeMultiplierEnabled = false;
            try
            {
                blnFreeKnowledgeMultiplierEnabled = _objOptions.FreeKnowledgeMultiplierEnabled;
            }
            catch
            {
            }
            chkKnowledgeMultiplier.Checked = blnFreeKnowledgeMultiplierEnabled;
            nudKnowledgeMultiplier.Enabled = chkKnowledgeMultiplier.Checked;
            nudKnowledgeMultiplier.Value = 2;

            bool blnUsePointsOnBrokenGroups = false;
            try
            {
                blnUsePointsOnBrokenGroups = _objOptions.UsePointsOnBrokenGroups;
            }
            catch
            {
            }
            chkUsePointsOnBrokenGroups.Checked = blnUsePointsOnBrokenGroups;

            bool blnDontDoubleQualities = false;
            try
            {
                blnDontDoubleQualities = _objOptions.DontDoubleQualities;
            }
            catch
            {
            }
            chkDontDoubleQualities.Checked = blnDontDoubleQualities;

            bool blnIgnoreArt = false;
            try
            {
                blnIgnoreArt = _objOptions.IgnoreArt;
            }
            catch
            {
            }
            chkIgnoreArt.Checked = blnIgnoreArt;

            bool blnCyberlegsMovement = false;
            try
            {
                blnCyberlegsMovement = _objOptions.CyberlegMovement;
            }
            catch
            {
            }
            chkCyberlegMovement.Checked = blnCyberlegsMovement;

            int intFreeKarmaContactsMultiplier = 1;
            try
            {
                intFreeKarmaContactsMultiplier = _objOptions.FreeContactsMultiplier;
            }
            catch
            {
            }
            nudContactMultiplier.Value = _objOptions.FreeContactsMultiplier;

            int intFreeKarmaKnowledgeMultiplier = 3;
            try
            {
                intFreeKarmaKnowledgeMultiplier = _objOptions.FreeKnowledgeMultiplier;
            }
            catch
            {
            }
            nudKnowledgeMultiplier.Value = intFreeKarmaKnowledgeMultiplier;

            int intFreeContactsMultiplier = 3;
            try
            {
                intFreeContactsMultiplier = _objOptions.FreeContactsMultiplier;
            }
            catch
            {
            }
            nudContactMultiplier.Value = intFreeContactsMultiplier;

            int intNuyenPerBP = 2000;
            try
            {
                intNuyenPerBP = _objOptions.NuyenPerBP;
            }
            catch
            {
            }
            nudNuyenPerBP.Value = intNuyenPerBP;

            int intEssenceDecimals = 2;
            try
            {
                intEssenceDecimals = _objOptions.EssenceDecimals;
            }
            catch
            {
            }
            if (intEssenceDecimals == 0)
                intEssenceDecimals = 2;
            cboEssenceDecimals.SelectedValue = intEssenceDecimals.ToString();

            bool blnNoSingleArmorEncumbrance = false;
            try
            {
                blnNoSingleArmorEncumbrance = _objOptions.NoSingleArmorEncumbrance;
            }
            catch
            {
            }
            chkNoSingleArmorEncumbrance.Checked = blnNoSingleArmorEncumbrance;

            bool blnIgnoreArmorEncumbrance = false;
            try
            {
                blnIgnoreArmorEncumbrance = _objOptions.IgnoreArmorEncumbrance;
            }
            catch
            {
            }

            bool blnAlternateArmorEncumbrance = false;
            try
            {
                blnAlternateArmorEncumbrance = _objOptions.AlternateArmorEncumbrance;
            }
            catch
            {
            }

            bool blnAllowCyberwareESSDiscounts = false;
            try
            {
                blnAllowCyberwareESSDiscounts = _objOptions.AllowCyberwareESSDiscounts;
            }
            catch
            {
            }

            bool blnESSLossReducesMaximumOnly = false;
            try
            {
                blnESSLossReducesMaximumOnly = _objOptions.ESSLossReducesMaximumOnly;
            }
            catch
            {
            }

            bool blnAllowSkillRegrouping = false;
            try
            {
                blnAllowSkillRegrouping = _objOptions.AllowSkillRegrouping;
            }
            catch
            {
            }
            chkAllowSkillRegrouping.Checked = blnAllowSkillRegrouping;

            bool blnMetatypeCostsKarma = true;
            try
            {
                blnMetatypeCostsKarma = _objOptions.MetatypeCostsKarma;
            }
            catch
            {
            }
            chkMetatypeCostsKarma.Checked = blnMetatypeCostsKarma;

            int intMetatypeCostsKarmaMultiplier = 1;
            try
            {
                intMetatypeCostsKarmaMultiplier = _objOptions.MetatypeCostsKarmaMultiplier;
            }
            catch
            {
            }
            nudMetatypeCostsKarmaMultiplier.Value = intMetatypeCostsKarmaMultiplier;

            bool blnStrengthAffectsRecoil = false;
            try
            {
                blnStrengthAffectsRecoil = _objOptions.StrengthAffectsRecoil;
            }
            catch
            {
            }
            chkStrengthAffectsRecoil.Checked = blnStrengthAffectsRecoil;

            bool blnMaximumArmorModifications = false;
            try
            {
                blnMaximumArmorModifications = _objOptions.MaximumArmorModifications;
            }
            catch
            {
            }
            chkMaximumArmorModifications.Checked = blnMaximumArmorModifications;

            bool blnArmorSuitCapacity = false;
            try
            {
                blnArmorSuitCapacity = _objOptions.ArmorSuitCapacity;
            }
            catch
            {
            }
            chkArmorSuitCapacity.Checked = blnArmorSuitCapacity;

            bool blnArmorDegradation = false;
            try
            {
                blnArmorDegradation = _objOptions.ArmorDegradation;
            }
            catch
            {
            }
            chkArmorDegradation.Checked = blnArmorDegradation;

            bool blnAutomaticCopyProtection = true;
            try
            {
                blnAutomaticCopyProtection = _objOptions.AutomaticCopyProtection;
            }
            catch
            {
            }
            chkAutomaticCopyProtection.Checked = blnAutomaticCopyProtection;

            bool blnAutomaticRegistration = true;
            try
            {
                blnAutomaticRegistration = _objOptions.AutomaticRegistration;
            }
            catch
            {
            }
            chkAutomaticRegistration.Checked = blnAutomaticRegistration;

            bool blnExceedNegativeQualities = false;
            try
            {
                blnExceedNegativeQualities = _objOptions.ExceedNegativeQualities;
            }
            catch
            {
            }
            chkExceedNegativeQualities.Checked = blnExceedNegativeQualities;
            chkExceedNegativeQualitiesLimit.Enabled = chkExceedNegativeQualities.Checked;

            bool blnExceedNegativeQualitiesLimit = false;
            try
            {
                blnExceedNegativeQualitiesLimit = _objOptions.ExceedNegativeQualitiesLimit;
            }
            catch
            {
            }
            chkExceedNegativeQualitiesLimit.Checked = blnExceedNegativeQualitiesLimit;

            bool blnExceedPositiveQualities = false;
            try
            {
                blnExceedPositiveQualities = _objOptions.ExceedPositiveQualities;
            }
            catch
            {
            }
            chkExceedPositiveQualities.Checked = _objOptions.ExceedPositiveQualities;

            bool blnUseCalculatedVehicleSensorRatings = false;
            try
            {
                blnUseCalculatedVehicleSensorRatings = _objOptions.UseCalculatedVehicleSensorRatings;
            }
            catch
            {
            }
            chkUseCalculatedVehicleSensorRatings.Checked = blnUseCalculatedVehicleSensorRatings;

            bool blnAlternateMatrixAttribute = false;
            try
            {
                blnAlternateMatrixAttribute = _objOptions.AlternateMatrixAttribute;
            }
            catch
            {
            }
            chkAlternateMatrixAttribute.Checked = blnAlternateMatrixAttribute;

            bool blnAlternateComplexFormCost = false;
            try
            {
                blnAlternateComplexFormCost = _objOptions.AlternateComplexFormCost;
            }
            catch
            {
            }
            chkAlternateComplexFormCost.Checked = blnAlternateComplexFormCost;

            bool blnAllowCustomTransgenics = false;
            try
            {
                blnAllowCustomTransgenics = _objOptions.AllowCustomTransgenics;
            }
            catch
            {
            }
            chkAllowCustomTransgenics.Checked = blnAllowCustomTransgenics;

            bool blnBreakSkillGroupsInCreateMode = false;
            try
            {
                blnBreakSkillGroupsInCreateMode = _objOptions.BreakSkillGroupsInCreateMode;
            }
            catch
            {
            }
            chkBreakSkillGroupsInCreateMode.Checked = blnBreakSkillGroupsInCreateMode;

            bool blnExtendAnyDetectionSpell = false;
            try
            {
                blnExtendAnyDetectionSpell = _objOptions.ExtendAnyDetectionSpell;
            }
            catch
            {
            }
            chkExtendAnyDetectionSpell.Checked = blnExtendAnyDetectionSpell;

            bool blnRestrictRecoil = true;
            try
            {
                blnRestrictRecoil = _objOptions.RestrictRecoil;
            }
            catch
            {
            }
            chkRestrictRecoil.Checked = blnRestrictRecoil;

            bool blnMultiplyRestrictedCost = false;
            try
            {
                blnMultiplyRestrictedCost = _objOptions.MultiplyRestrictedCost;
            }
            catch
            {
            }

            bool blnMultiplyForbiddenCost = false;
            try
            {
                blnMultiplyForbiddenCost = _objOptions.MultiplyForbiddenCost;
            }
            catch
            {
            }

            int intRestrictedCostMultiplier = 1;
            try
            {
                intRestrictedCostMultiplier = _objOptions.RestrictedCostMultiplier;
            }
            catch
            {
            }

            int intForbiddenCostMultiplier = 1;
            try
            {
                intForbiddenCostMultiplier = _objOptions.ForbiddenCostMultiplier;
            }
            catch
            {
            }

            bool blnEnforceCapacity = true;
            try
            {
                blnEnforceCapacity = _objOptions.EnforceCapacity;
            }
            catch
            {
            }
            chkEnforceCapacity.Checked = blnEnforceCapacity;

            bool blnAllowExceedAttributeBP = false;
            try
            {
                blnAllowExceedAttributeBP = _objOptions.AllowExceedAttributeBP;
            }
            catch
            {
            }

            bool blnUnrestrictedNueyn = false;
            try
            {
                blnUnrestrictedNueyn = _objOptions.UnrestrictedNuyen;
            }
            catch
            {
            }

            bool blnCalculateCommlinkResponse = true;
            try
            {
                blnCalculateCommlinkResponse = _objOptions.CalculateCommlinkResponse;
            }
            catch
            {
            }
            chkCalculateCommlinkResponse.Checked = blnCalculateCommlinkResponse;

            bool blnErgonomicProgramLimit = true;
            try
            {
                blnErgonomicProgramLimit = _objOptions.ErgonomicProgramLimit;
            }
            catch
            {
            }
            chkErgonomicProgramLimit.Checked = blnErgonomicProgramLimit;

            bool blnAllowSkillDiceRolling = false;
            try
            {
                blnAllowSkillDiceRolling = _objOptions.AllowSkillDiceRolling;
            }
            catch
            {
            }
            chkAllowSkillDiceRolling.Checked = blnAllowSkillDiceRolling;

            bool blnCreateBackupOnCareer = false;
            try
            {
                blnCreateBackupOnCareer = _objOptions.CreateBackupOnCareer;
            }
            catch
            {
            }
            chkCreateBackupOnCareer.Checked = blnCreateBackupOnCareer;

            bool blnMayBuyQualities = false;
            try
            {
                blnMayBuyQualities = _objOptions.MayBuyQualities;
            }
            catch
            {
            }
            chkMayBuyQualities.Checked = blnMayBuyQualities;

            bool blnUseContactPoints = false;
            try
            {
                blnUseContactPoints = _objOptions.UseContactPoints;
            }
            catch
            {
            }
            chkContactPoints.Checked = blnUseContactPoints;

            bool blnPrintLeadershipAlternates = false;
            try
            {
                blnPrintLeadershipAlternates = _objOptions.PrintLeadershipAlternates;
            }
            catch
            {
            }

            bool blnPrintArcanaAlternates = false;
            try
            {
                blnPrintArcanaAlternates = _objOptions.PrintArcanaAlternates;
            }
            catch
            {
            }

            bool blnPrintNotes = false;
            try
            {
                blnPrintNotes = _objOptions.PrintNotes;
            }
            catch
            {
            }
            chkPrintNotes.Checked = blnPrintNotes;

            bool blnAllowHigherStackedFoci = false;
            try
            {
                blnAllowHigherStackedFoci = _objOptions.AllowHigherStackedFoci;
            }
            catch
            {
            }

            bool blnAllowEditPartOfBaseWeapon = false;
            try
            {
                blnAllowEditPartOfBaseWeapon = _objOptions.AllowEditPartOfBaseWeapon;
            }
            catch
            {
            }

            bool blnAlternateMetatypeAttributeKarma = false;
            try
            {
                blnAlternateMetatypeAttributeKarma = _objOptions.AlternateMetatypeAttributeKarma;
            }
            catch
            {
            }

            bool blnAllowObsolescentUpgrade = false;
            try
            {
                blnAllowObsolescentUpgrade = _objOptions.AllowObsolescentUpgrade;
            }
            catch
            {
            }

            bool blnAllowBiowareSuites = false;
            try
            {
                blnAllowBiowareSuites = _objOptions.AllowBiowareSuites;
            }
            catch
            {
            }

            bool blnFreeSpiritPowerPointsMAG = false;
            try
            {
                blnFreeSpiritPowerPointsMAG = _objOptions.FreeSpiritPowerPointsMAG;
            }
            catch
            {
            }

            bool blnSpecialAttributeKarmaLimit = false;
            try
            {
                blnSpecialAttributeKarmaLimit = _objOptions.SpecialAttributeKarmaLimit;
            }
            catch
            {
            }

            bool blnTechnomancerAllowAutosoft = false;
            try
            {
                blnTechnomancerAllowAutosoft = _objOptions.TechnomancerAllowAutosoft;
            }
            catch
            {
            }

            bool blnOpenPDFsAsURLs = false;
            try
            {
                blnOpenPDFsAsURLs = GlobalOptions._blnOpenPDFsAsURLs;
            }
            catch
            {
            }
            chkOpenPDFsAsURLs.Checked = blnOpenPDFsAsURLs;

            if (_objOptions.LimbCount == 6)
                cboLimbCount.SelectedValue = "all";
            else
            {
                if (_objOptions.ExcludeLimbSlot == "skull")
                    cboLimbCount.SelectedValue = "torso";
                else
                    cboLimbCount.SelectedValue = "skull";
            }

            // Populate the Karma fields.
            nudKarmaAttribute.Value = _objOptions.KarmaAttribute;
            nudKarmaQuality.Value = _objOptions.KarmaQuality;
            nudKarmaSpecialization.Value = _objOptions.KarmaSpecialization;
            nudKarmaNewKnowledgeSkill.Value = _objOptions.KarmaNewKnowledgeSkill;
            nudKarmaNewActiveSkill.Value = _objOptions.KarmaNewActiveSkill;
            nudKarmaNewSkillGroup.Value = _objOptions.KarmaNewSkillGroup;
            nudKarmaImproveKnowledgeSkill.Value = _objOptions.KarmaImproveKnowledgeSkill;
            nudKarmaImproveActiveSkill.Value = _objOptions.KarmaImproveActiveSkill;
            nudKarmaImproveSkillGroup.Value = _objOptions.KarmaImproveSkillGroup;
            nudKarmaSpell.Value = _objOptions.KarmaSpell;
            nudKarmaNewComplexForm.Value = _objOptions.KarmaNewComplexForm;
            nudKarmaImproveComplexForm.Value = _objOptions.KarmaImproveComplexForm;
            nudKarmaComplexFormOption.Value = _objOptions.KarmaComplexFormOption;
            nudKarmaComplexFormSkillsoft.Value = _objOptions.KarmaComplexFormSkillsoft;
            nudKarmaNuyenPer.Value = _objOptions.KarmaNuyenPer;
            nudKarmaContact.Value = _objOptions.KarmaContact;
            nudKarmaCarryover.Value = _objOptions.KarmaCarryover;
            nudKarmaSpirit.Value = _objOptions.KarmaSpirit;
            nudKarmaManeuver.Value = _objOptions.KarmaManeuver;
            nudKarmaInitiation.Value = _objOptions.KarmaInitiation;
            nudKarmaMetamagic.Value = _objOptions.KarmaMetamagic;
            nudKarmaJoinGroup.Value = _objOptions.KarmaJoinGroup;
            nudKarmaLeaveGroup.Value = _objOptions.KarmaLeaveGroup;

            //nudKarmaAnchoringFocus.Value = _objOptions.KarmaAnchoringFocus;
            //nudKarmaBanishingFocus.Value = _objOptions.KarmaBanishingFocus;
            //nudKarmaBindingFocus.Value = _objOptions.KarmaBindingFocus;
            //nudKarmaCenteringFocus.Value = _objOptions.KarmaCenteringFocus;
            //nudKarmaCounterspellingFocus.Value = _objOptions.KarmaCounterspellingFocus;
            //nudKarmaDiviningFocus.Value = _objOptions.KarmaDiviningFocus;
            //nudKarmaDowsingFocus.Value = _objOptions.KarmaDowsingFocus;
            //nudKarmaInfusionFocus.Value = _objOptions.KarmaInfusionFocus;
            //nudKarmaMaskingFocus.Value = _objOptions.KarmaMaskingFocus;
            //nudKarmaPowerFocus.Value = _objOptions.KarmaPowerFocus;
            //nudKarmaShieldingFocus.Value = _objOptions.KarmaShieldingFocus;
            //nudKarmaSpellcastingFocus.Value = _objOptions.KarmaSpellcastingFocus;
            //nudKarmaSummoningFocus.Value = _objOptions.KarmaSummoningFocus;
            //nudKarmaSustainingFocus.Value = _objOptions.KarmaSustainingFocus;
            //nudKarmaSymbolicLinkFocus.Value = _objOptions.KarmaSymbolicLinkFocus;
            //nudKarmaWeaponFocus.Value = _objOptions.KarmaWeaponFocus;

            // Load default build method info.
            cboBuildMethod.SelectedValue = _objOptions.BuildMethod;
            nudBP.Value = _objOptions.BuildPoints;
            nudMaxAvail.Value = _objOptions.Availability;

            txtSettingName.Text = _objOptions.Name;
            if (cboSetting.SelectedValue.ToString() == "default.xml")
                txtSettingName.Enabled = false;
            else
                txtSettingName.Enabled = true;
        }

        /// <summary>
        /// Save the global settings to the registry.
        /// </summary>
        private void SaveRegistrySettings()
        {
            // If we're just now enabling logging, flush the log
            
            // Set Registry values.
            GlobalOptions.Instance.AutomaticUpdate = chkAutomaticUpdate.Checked;
            GlobalOptions.Instance.LocalisedUpdatesOnly = chkLocalisedUpdatesOnly.Checked;
            GlobalOptions.Instance.UseLogging = chkUseLogging.Checked;
            GlobalOptions.Instance.Language = cboLanguage.SelectedValue.ToString();
            GlobalOptions.Instance.StartupFullscreen = chkStartupFullscreen.Checked;
            GlobalOptions.Instance.SingleDiceRoller = chkSingleDiceRoller.Checked;
            GlobalOptions.Instance.DefaultCharacterSheet = cboXSLT.SelectedValue.ToString();
            GlobalOptions.Instance.DatesIncludeTime = chkDatesIncludeTime.Checked;
            GlobalOptions.Instance.PrintToFileFirst = chkPrintToFileFirst.Checked;
            GlobalOptions.Instance.PDFAppPath = txtPDFAppPath.Text;
            GlobalOptions.Instance.URLAppPath = txtURLAppPath.Text;
            GlobalOptions.Instance.OpenPDFsAsURLs = chkOpenPDFsAsURLs.Checked;
            GlobalOptions.Instance.LifeModuleEnabled = chkLifeModule.Checked;
            Microsoft.Win32.RegistryKey objRegistry = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
            objRegistry.SetValue("autoupdate", chkAutomaticUpdate.Checked.ToString());
            objRegistry.SetValue("localisedupdatesonly", chkLocalisedUpdatesOnly.Checked.ToString());
            objRegistry.SetValue("uselogging", chkUseLogging.Checked.ToString());
            objRegistry.SetValue("language", cboLanguage.SelectedValue.ToString());
            objRegistry.SetValue("startupfullscreen", chkStartupFullscreen.Checked.ToString());
            objRegistry.SetValue("singlediceroller", chkSingleDiceRoller.Checked.ToString());
            objRegistry.SetValue("defaultsheet", cboXSLT.SelectedValue.ToString());
            objRegistry.SetValue("datesincludetime", chkDatesIncludeTime.Checked.ToString());
            objRegistry.SetValue("printtofilefirst", chkPrintToFileFirst.Checked.ToString());
            objRegistry.SetValue("openpdfsasurls", chkOpenPDFsAsURLs.Checked.ToString());
            objRegistry.SetValue("urlapppath", txtURLAppPath.Text);
            objRegistry.SetValue("pdfapppath", txtPDFAppPath.Text);
            objRegistry.SetValue("lifemodule", chkLifeModule.Checked.ToString());

            // Save the SourcebookInfo.
            Microsoft.Win32.RegistryKey objSourceRegistry = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Chummer5\\Sourcebook");
            foreach (SourcebookInfo objSource in GlobalOptions.Instance.SourcebookInfo)
                objSourceRegistry.SetValue(objSource.Code, objSource.Path + "|" + objSource.Offset.ToString());
        }
        #endregion
    }
}