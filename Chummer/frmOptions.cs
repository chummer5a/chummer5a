using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public partial class frmOptions : Form
	{
		private readonly CharacterOptions _characterOptions = new CharacterOptions();
		private bool _skipRefresh;

        #region Form Events
        public frmOptions()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
        }

        private void frmOptions_Load(object sender, EventArgs e)
        {
            PopulateBuildMethodList();
            PopulateEssenceDecimalsList();
            PopulateLimbCountList();
            SetToolTips();
            MoveControls();
            PopulateSettingsList();
	        SetDefaultValueForSettingsList();
            PopulateGlobalOptions();
            PopulateLanguageList();
            SetDefaultValueForLanguageList();
            PopulateXsltList();
            SetDefaultValueForXsltList();
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
            BuildBooksList();

            _characterOptions.AllowCustomTransgenics = chkAllowCustomTransgenics.Checked;
			_characterOptions.AllowCyberwareESSDiscounts = chkAllowCyberwareESSDiscounts.Checked;
            _characterOptions.AllowInitiationInCreateMode = chkAllowInitiation.Checked;
            _characterOptions.AllowSkillDiceRolling = chkAllowSkillDiceRolling.Checked;
            _characterOptions.AllowSkillRegrouping = chkAllowSkillRegrouping.Checked;
            _characterOptions.AlternateMatrixAttribute = chkAlternateMatrixAttribute.Checked;
            _characterOptions.AlternateComplexFormCost = chkAlternateComplexFormCost.Checked;
            _characterOptions.ArmorDegradation = chkArmorDegradation.Checked;
            _characterOptions.AutomaticCopyProtection = chkAutomaticCopyProtection.Checked;
            _characterOptions.AutomaticRegistration = chkAutomaticRegistration.Checked;
            _characterOptions.BreakSkillGroupsInCreateMode = chkBreakSkillGroupsInCreateMode.Checked;
            _characterOptions.CalculateCommlinkResponse = chkCalculateCommlinkResponse.Checked;
            _characterOptions.CapSkillRating = chkCapSkillRating.Checked;
            _characterOptions.ConfirmDelete = chkConfirmDelete.Checked;
            _characterOptions.ConfirmKarmaExpense = chkConfirmKarmaExpense.Checked;
            _characterOptions.CreateBackupOnCareer = chkCreateBackupOnCareer.Checked;
            _characterOptions.CyberlegMovement = chkCyberlegMovement.Checked;
            _characterOptions.DontDoubleQualities = chkDontDoubleQualities.Checked;
            _characterOptions.EnforceCapacity = chkEnforceCapacity.Checked;
            _characterOptions.EnforceMaximumSkillRatingModifier = chkEnforceSkillMaximumModifiedRating.Checked;
            _characterOptions.ErgonomicProgramLimit = chkErgonomicProgramLimit.Checked;
            _characterOptions.EssenceDecimals = Convert.ToInt32(cboEssenceDecimals.SelectedValue);
            _characterOptions.ExceedNegativeQualities = chkExceedNegativeQualities.Checked;
                if (chkExceedNegativeQualities.Checked)
                    chkExceedNegativeQualitiesLimit.Enabled = true;
            _characterOptions.ExceedNegativeQualitiesLimit = chkExceedNegativeQualitiesLimit.Checked;
            _characterOptions.ExceedPositiveQualities = chkExceedPositiveQualities.Checked;
            _characterOptions.ExtendAnyDetectionSpell = chkExtendAnyDetectionSpell.Checked;
            _characterOptions.FreeContactsMultiplier = Convert.ToInt32(nudContactMultiplier.Value);
            _characterOptions.FreeContactsMultiplierEnabled = chkContactMultiplier.Checked;
                if (chkContactMultiplier.Checked)
                    nudContactMultiplier.Enabled = true;
            _characterOptions.FreeKarmaContacts = chkFreeKarmaContacts.Checked;
            _characterOptions.FreeKarmaKnowledge = chkFreeKarmaKnowledge.Checked;
            _characterOptions.FreeKnowledgeMultiplierEnabled = chkKnowledgeMultiplier.Checked;
                if (chkKnowledgeMultiplier.Checked)
                    chkKnowledgeMultiplier.Enabled = true;
            _characterOptions.FreeKnowledgeMultiplier = Convert.ToInt32(nudKnowledgeMultiplier.Value);
            _characterOptions.IgnoreArt = chkIgnoreArt.Checked;
            _characterOptions.KnucksUseUnarmed = chkKnucks.Checked;
            _characterOptions.LicenseRestricted = chkLicenseEachRestrictedItem.Checked;
            _characterOptions.MaximumArmorModifications = chkMaximumArmorModifications.Checked;
            _characterOptions.MetatypeCostsKarma = chkMetatypeCostsKarma.Checked;
            _characterOptions.MetatypeCostsKarmaMultiplier = Convert.ToInt32(nudMetatypeCostsKarmaMultiplier.Value);
            _characterOptions.MoreLethalGameplay = chkMoreLethalGameplay.Checked;
            _characterOptions.NoSingleArmorEncumbrance = chkNoSingleArmorEncumbrance.Checked;
            _characterOptions.NuyenPerBP = Convert.ToInt32(nudKarmaNuyenPer.Value);
            _characterOptions.PrintExpenses = chkPrintExpenses.Checked;
            _characterOptions.PrintNotes = chkPrintNotes.Checked;
            _characterOptions.PrintSkillsWithZeroRating = chkPrintSkillsWithZeroRating.Checked;
            _characterOptions.RestrictRecoil = chkRestrictRecoil.Checked;
            _characterOptions.StrengthAffectsRecoil = Convert.ToBoolean(chkStrengthAffectsRecoil.Checked);
            _characterOptions.UseCalculatedVehicleSensorRatings = chkUseCalculatedVehicleSensorRatings.Checked;
            _characterOptions.UsePointsOnBrokenGroups = chkUsePointsOnBrokenGroups.Checked;

            switch (cboLimbCount.SelectedValue.ToString())
            {
                case "torso":
                    _characterOptions.LimbCount = 5;
                    _characterOptions.ExcludeLimbSlot = "skull";
                    break;
                case "skull":
                    _characterOptions.LimbCount = 5;
                    _characterOptions.ExcludeLimbSlot = "torso";
                    break;
                default:
                    _characterOptions.LimbCount = 6;
                    _characterOptions.ExcludeLimbSlot = "";
                    break;
            }

            // Karma options.
            _characterOptions.KarmaAttribute = Convert.ToInt32(nudKarmaAttribute.Value);
            _characterOptions.KarmaQuality = Convert.ToInt32(nudKarmaQuality.Value);
            _characterOptions.KarmaSpecialization = Convert.ToInt32(nudKarmaSpecialization.Value);
            _characterOptions.KarmaNewKnowledgeSkill = Convert.ToInt32(nudKarmaNewKnowledgeSkill.Value);
            _characterOptions.KarmaNewActiveSkill = Convert.ToInt32(nudKarmaNewActiveSkill.Value);
            _characterOptions.KarmaNewSkillGroup = Convert.ToInt32(nudKarmaNewSkillGroup.Value);
            _characterOptions.KarmaImproveKnowledgeSkill = Convert.ToInt32(nudKarmaImproveKnowledgeSkill.Value);
            _characterOptions.KarmaImproveActiveSkill = Convert.ToInt32(nudKarmaImproveActiveSkill.Value);
            _characterOptions.KarmaImproveSkillGroup = Convert.ToInt32(nudKarmaImproveSkillGroup.Value);
            _characterOptions.KarmaSpell = Convert.ToInt32(nudKarmaSpell.Value);
            _characterOptions.KarmaNewComplexForm = Convert.ToInt32(nudKarmaNewComplexForm.Value);
            _characterOptions.KarmaImproveComplexForm = Convert.ToInt32(nudKarmaImproveComplexForm.Value);
            _characterOptions.KarmaMetamagic = Convert.ToInt32(nudKarmaMetamagic.Value);
            _characterOptions.KarmaNuyenPer = Convert.ToInt32(nudKarmaNuyenPer.Value);
            _characterOptions.KarmaContact = Convert.ToInt32(nudKarmaContact.Value);
            _characterOptions.KarmaEnemy = Convert.ToInt32(nudKarmaEnemy.Value);
            _characterOptions.KarmaCarryover = Convert.ToInt32(nudKarmaCarryover.Value);
            _characterOptions.KarmaSpirit = Convert.ToInt32(nudKarmaSpirit.Value);
            _characterOptions.KarmaManeuver = Convert.ToInt32(nudKarmaManeuver.Value);
            _characterOptions.KarmaInitiation = Convert.ToInt32(nudKarmaInitiation.Value);
            _characterOptions.KarmaComplexFormOption = Convert.ToInt32(nudKarmaComplexFormOption.Value);
            _characterOptions.KarmaComplexFormSkillsoft = Convert.ToInt32(nudKarmaComplexFormSkillsoft.Value);
            _characterOptions.KarmaJoinGroup = Convert.ToInt32(nudKarmaJoinGroup.Value);
            _characterOptions.KarmaLeaveGroup = Convert.ToInt32(nudKarmaLeaveGroup.Value);

            // Build Priority options.
            _characterOptions.MayBuyQualities = chkMayBuyQualities.Checked;
            _characterOptions.UseContactPoints = chkContactPoints.Checked;

            // Build method options.
            _characterOptions.BuildMethod = cboBuildMethod.SelectedValue.ToString();
            _characterOptions.BuildPoints = Convert.ToInt32(nudBP.Value);
            _characterOptions.Availability = Convert.ToInt32(nudMaxAvail.Value);

            _characterOptions.Name = txtSettingName.Text;
            _characterOptions.Save();

            CloseCreateForm();
            DialogResult = DialogResult.OK;
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

            _characterOptions.Load(objItem.Value);
            PopulateOptions();
        }

        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isEnabled = cboLanguage.SelectedValue.ToString() != "en-us";
            cmdVerify.Enabled = isEnabled;
            cmdVerifyData.Enabled = isEnabled;
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
            bool blnSR5Included = false;

            foreach (TreeNode objNode in treSourcebook.Nodes)
            {
                if (!objNode.Checked)
                    continue;

                lstBooks.Add(objNode.Tag.ToString());

                if (objNode.Tag.ToString() == "SR5")
                    blnSR5Included = true;
            }

            // If the SR5 book was somehow missed, add it back.
            if (!blnSR5Included)
                _characterOptions.Books.Add("SR5");

            XmlManager.Instance.Verify(cboLanguage.SelectedValue.ToString(), lstBooks);

            string strFilePath = Path.Combine(Environment.CurrentDirectory, "lang", "results_" + cboLanguage.SelectedValue + ".xml");
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

        private void cmdRestoreDefaultsKarma_Click(object sender, EventArgs e)
        {
            string text = LanguageManager.Instance.GetString("Message_Options_RestoreDefaults");
            string caption = LanguageManager.Instance.GetString("MessageTitle_Options_RestoreDefaults");

            // Verify that the user wants to reset these values.
            if (MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            RestoreDefaultKarmaValues();
            RestoreDefaultKarmaFociValues();
        }

        private void cmdPDFAppPath_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                    txtPDFAppPath.Text = openFileDialog.FileName;
            }
        }

        private void cmdURLAppPath_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                    txtURLAppPath.Text = openFileDialog.FileName;
            }
        }

	    private void cmdPDFLocation_Click(object sender, EventArgs e)
	    {
            // Prompt the user to select a save file to associate with this Contact.
            using (var openFileDialog = new OpenFileDialog())
	        {
	            openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*";

	            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
	                UpdateSourcebookInfoPath(openFileDialog.FileName);
	        }
	    }

        private void treSourcebook_AfterSelect(object sender, TreeViewEventArgs e)
        {
            cmdPDFLocation.Enabled = true;
            nudPDFOffset.Enabled = true;
            cmdPDFTest.Enabled = true;

            _skipRefresh = true;
            txtPDFLocation.Text = "";
            nudPDFOffset.Value = 0;
            _skipRefresh = false;

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
            if (_skipRefresh)
                return;

            int offset = Convert.ToInt32(nudPDFOffset.Value);
            string tag = treSourcebook.SelectedNode.Tag.ToString();
            SourcebookInfo foundSource = GlobalOptions.Instance.SourcebookInfo.FirstOrDefault(x => x.Code == tag);

            if (foundSource != null)
            {
                foundSource.Offset = offset;
            }
            else
            {
                // If the Sourcebook was not found in the options, add it.
                var newSource = new SourcebookInfo();
                newSource.Code = tag;
                newSource.Offset = offset;
                GlobalOptions.Instance.SourcebookInfo.Add(newSource);
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
            objCommon.OpenPDF(treSourcebook.SelectedNode.Tag + " 5");
        }
        #endregion

        #region Methods
        private void MoveControls()
        {
            int intWidth = 0;

            lblMetatypeCostsKarma.Left = chkMetatypeCostsKarma.Left + chkMetatypeCostsKarma.Width + 6;
            nudMetatypeCostsKarmaMultiplier.Left = lblMetatypeCostsKarma.Left + lblMetatypeCostsKarma.Width + 6;
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
            nudKarmaEnemy.Left = lblKarmaImproveKnowledgeSkill.Left + lblKarmaImproveKnowledgeSkill.Width + 6;
            lblKarmaEnemyExtra.Left = nudKarmaEnemy.Left + nudKarmaEnemy.Width + 6;
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
            Width = intWidth + 29;
            Height = tabControl1.Top + tabControl1.Height + cmdOK.Height + 55;
            // Centre the OK button.
            cmdOK.Left = (Width / 2) - (cmdOK.Width / 2);
        }

	    private void PopulateSourcebookTreeView()
	    {
            // Load the Sourcebook information.
	        XmlDocument objXmlDocument = XmlManager.Instance.Load("books.xml");

            // Put the Sourcebooks into a List so they can first be sorted.
            XmlNodeList objXmlBookList = objXmlDocument.SelectNodes("/chummer/books/book");
            treSourcebook.Nodes.Clear();

            foreach (XmlNode objXmlBook in objXmlBookList)
            {
                bool blnChecked = _characterOptions.Books.Contains(objXmlBook["code"].InnerText);
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
        }

	    private void SetDefaultValueForLimbCount()
	    {
            if (_characterOptions.LimbCount == 6)
                cboLimbCount.SelectedValue = "all";
            else
                cboLimbCount.SelectedValue = _characterOptions.ExcludeLimbSlot == "skull" ? "torso" : "skull";
        }

        /// <summary>
        /// Set the values for all of the controls based on the Options for the selected Setting.
        /// </summary>
        private void PopulateOptions()
	    {
            PopulateSourcebookTreeView();

            chkContactMultiplier.Checked = _characterOptions.FreeContactsMultiplierEnabled;
            nudContactMultiplier.Enabled = _characterOptions.FreeContactsMultiplierEnabled;
            nudContactMultiplier.Value = 3;
            chkKnowledgeMultiplier.Checked = _characterOptions.FreeKnowledgeMultiplierEnabled;
            nudKnowledgeMultiplier.Enabled = _characterOptions.FreeKnowledgeMultiplierEnabled;
            nudKnowledgeMultiplier.Value = 2;
            chkConfirmDelete.Checked = _characterOptions.ConfirmDelete;
	        chkConfirmKarmaExpense.Checked = _characterOptions.ConfirmKarmaExpense;
	        chkPrintSkillsWithZeroRating.Checked = _characterOptions.PrintSkillsWithZeroRating;
	        chkMoreLethalGameplay.Checked = _characterOptions.MoreLethalGameplay;
	        chkEnforceSkillMaximumModifiedRating.Checked = _characterOptions.EnforceMaximumSkillRatingModifier;
	        chkLicenseEachRestrictedItem.Checked = _characterOptions.LicenseRestricted;
	        chkSpecialKarmaCost.Checked = _characterOptions.SpecialKarmaCostBasedOnShownValue;
	        chkCapSkillRating.Checked = _characterOptions.CapSkillRating;
	        chkPrintExpenses.Checked = _characterOptions.PrintExpenses;
	        chkKnucks.Checked = _characterOptions.KnucksUseUnarmed;
	        chkAllowInitiation.Checked = _characterOptions.AllowInitiationInCreateMode;
	        chkFreeKarmaContacts.Checked = _characterOptions.FreeKarmaContacts;
	        chkFreeKarmaKnowledge.Checked = _characterOptions.FreeKarmaKnowledge;
	        chkUsePointsOnBrokenGroups.Checked = _characterOptions.UsePointsOnBrokenGroups;
	        chkDontDoubleQualities.Checked = _characterOptions.DontDoubleQualities;
	        chkIgnoreArt.Checked = _characterOptions.IgnoreArt;
	        chkCyberlegMovement.Checked = _characterOptions.CyberlegMovement;
	        nudContactMultiplier.Value = _characterOptions.FreeContactsMultiplier;
	        nudKnowledgeMultiplier.Value = _characterOptions.FreeKnowledgeMultiplier;
	        nudContactMultiplier.Value = _characterOptions.FreeContactsMultiplier;
	        nudNuyenPerBP.Value = _characterOptions.NuyenPerBP;
	        cboEssenceDecimals.SelectedValue = _characterOptions.EssenceDecimals == 0 ? "2" : _characterOptions.EssenceDecimals.ToString();
	        chkNoSingleArmorEncumbrance.Checked = _characterOptions.NoSingleArmorEncumbrance;
	        chkAllowCyberwareESSDiscounts.Checked = _characterOptions.AllowCyberwareESSDiscounts;
	        chkAllowSkillRegrouping.Checked = _characterOptions.AllowSkillRegrouping;
	        chkMetatypeCostsKarma.Checked = _characterOptions.MetatypeCostsKarma;
	        nudMetatypeCostsKarmaMultiplier.Value = _characterOptions.MetatypeCostsKarmaMultiplier;
	        chkStrengthAffectsRecoil.Checked = _characterOptions.StrengthAffectsRecoil;
	        chkMaximumArmorModifications.Checked = _characterOptions.MaximumArmorModifications;
	        chkArmorSuitCapacity.Checked = _characterOptions.ArmorSuitCapacity;
	        chkArmorDegradation.Checked = _characterOptions.ArmorDegradation;
	        chkAutomaticCopyProtection.Checked = _characterOptions.AutomaticCopyProtection;
	        chkAutomaticRegistration.Checked = _characterOptions.AutomaticRegistration;
	        chkExceedNegativeQualities.Checked = _characterOptions.ExceedNegativeQualities;
	        chkExceedNegativeQualitiesLimit.Enabled = chkExceedNegativeQualities.Checked;
	        chkExceedNegativeQualitiesLimit.Checked = _characterOptions.ExceedNegativeQualitiesLimit;
	        chkExceedPositiveQualities.Checked = _characterOptions.ExceedPositiveQualities;
	        chkUseCalculatedVehicleSensorRatings.Checked = _characterOptions.UseCalculatedVehicleSensorRatings;
	        chkAlternateMatrixAttribute.Checked = _characterOptions.AlternateMatrixAttribute;
	        chkAlternateComplexFormCost.Checked = _characterOptions.AlternateComplexFormCost;
	        chkAllowCustomTransgenics.Checked = _characterOptions.AllowCustomTransgenics;
	        chkBreakSkillGroupsInCreateMode.Checked = _characterOptions.BreakSkillGroupsInCreateMode;
	        chkExtendAnyDetectionSpell.Checked = _characterOptions.ExtendAnyDetectionSpell;
	        chkRestrictRecoil.Checked = _characterOptions.RestrictRecoil;
	        chkEnforceCapacity.Checked = _characterOptions.EnforceCapacity;
	        chkCalculateCommlinkResponse.Checked = _characterOptions.CalculateCommlinkResponse;
	        chkErgonomicProgramLimit.Checked = _characterOptions.ErgonomicProgramLimit;
	        chkAllowSkillDiceRolling.Checked = _characterOptions.AllowSkillDiceRolling;
	        chkCreateBackupOnCareer.Checked = _characterOptions.CreateBackupOnCareer;
	        chkMayBuyQualities.Checked = _characterOptions.MayBuyQualities;
	        chkContactPoints.Checked = _characterOptions.UseContactPoints;
	        chkPrintNotes.Checked = _characterOptions.PrintNotes;
	        chkOpenPDFsAsURLs.Checked = GlobalOptions._blnOpenPDFsAsURLs;
            cboBuildMethod.SelectedValue = _characterOptions.BuildMethod;
            nudBP.Value = _characterOptions.BuildPoints;
            nudMaxAvail.Value = _characterOptions.Availability;
            txtSettingName.Text = _characterOptions.Name;
            txtSettingName.Enabled = cboSetting.SelectedValue.ToString() != "default.xml";

            SetDefaultValueForLimbCount();
            PopulateKarmaFields();
	    }

	    private void PopulateKarmaFields()
	    {
	        nudKarmaAttribute.Value = _characterOptions.KarmaAttribute;
	        nudKarmaQuality.Value = _characterOptions.KarmaQuality;
	        nudKarmaSpecialization.Value = _characterOptions.KarmaSpecialization;
	        nudKarmaNewKnowledgeSkill.Value = _characterOptions.KarmaNewKnowledgeSkill;
	        nudKarmaNewActiveSkill.Value = _characterOptions.KarmaNewActiveSkill;
	        nudKarmaNewSkillGroup.Value = _characterOptions.KarmaNewSkillGroup;
	        nudKarmaImproveKnowledgeSkill.Value = _characterOptions.KarmaImproveKnowledgeSkill;
	        nudKarmaImproveActiveSkill.Value = _characterOptions.KarmaImproveActiveSkill;
	        nudKarmaImproveSkillGroup.Value = _characterOptions.KarmaImproveSkillGroup;
	        nudKarmaSpell.Value = _characterOptions.KarmaSpell;
	        nudKarmaNewComplexForm.Value = _characterOptions.KarmaNewComplexForm;
	        nudKarmaImproveComplexForm.Value = _characterOptions.KarmaImproveComplexForm;
	        nudKarmaComplexFormOption.Value = _characterOptions.KarmaComplexFormOption;
	        nudKarmaComplexFormSkillsoft.Value = _characterOptions.KarmaComplexFormSkillsoft;
	        nudKarmaNuyenPer.Value = _characterOptions.KarmaNuyenPer;
	        nudKarmaContact.Value = _characterOptions.KarmaContact;
	        nudKarmaEnemy.Value = _characterOptions.KarmaEnemy;
	        nudKarmaCarryover.Value = _characterOptions.KarmaCarryover;
	        nudKarmaSpirit.Value = _characterOptions.KarmaSpirit;
	        nudKarmaManeuver.Value = _characterOptions.KarmaManeuver;
	        nudKarmaInitiation.Value = _characterOptions.KarmaInitiation;
	        nudKarmaMetamagic.Value = _characterOptions.KarmaMetamagic;
	        nudKarmaJoinGroup.Value = _characterOptions.KarmaJoinGroup;
	        nudKarmaLeaveGroup.Value = _characterOptions.KarmaLeaveGroup;
	    }

	    private void SaveGlobalOptions()
	    {
            GlobalOptions.Instance.AutomaticUpdate = chkAutomaticUpdate.Checked;
            GlobalOptions.Instance.LocalisedUpdatesOnly = chkLocalisedUpdatesOnly.Checked;
            GlobalOptions.Instance.UseLogging = chkUseLogging.Checked;
            GlobalOptions.Instance.Language = cboLanguage.SelectedValue.ToString();
            GlobalOptions.Instance.StartupFullscreen = chkStartupFullscreen.Checked;
            GlobalOptions.Instance.SingleDiceRoller = chkSingleDiceRoller.Checked;
            if (cboXSLT.SelectedValue.ToString() == "")
            {
                cboXSLT.SelectedValue = "Shadowrun 5";
            }
            GlobalOptions.Instance.DefaultCharacterSheet = cboXSLT.SelectedValue.ToString();
            GlobalOptions.Instance.DatesIncludeTime = chkDatesIncludeTime.Checked;
            GlobalOptions.Instance.PrintToFileFirst = chkPrintToFileFirst.Checked;
            GlobalOptions.Instance.PDFAppPath = txtPDFAppPath.Text;
            GlobalOptions.Instance.URLAppPath = txtURLAppPath.Text;
            GlobalOptions.Instance.OpenPDFsAsURLs = chkOpenPDFsAsURLs.Checked;
            GlobalOptions.Instance.LifeModuleEnabled = chkLifeModule.Checked;
            GlobalOptions.Instance.MissionsOnly = chkMissions.Checked;
        }

	    /// <summary>
        /// Save the global settings to the registry.
        /// </summary>
        private void SaveRegistrySettings()
        {
            SaveGlobalOptions();

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
			objRegistry.SetValue("missionsonly", chkMissions.Checked.ToString());

            // Save the SourcebookInfo.
            Microsoft.Win32.RegistryKey objSourceRegistry = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Chummer5\\Sourcebook");
            foreach (SourcebookInfo objSource in GlobalOptions.Instance.SourcebookInfo)
                objSourceRegistry.SetValue(objSource.Code, objSource.Path + "|" + objSource.Offset);
        }

        private void BuildBooksList()
        {
            _characterOptions.Books.Clear();

            bool blnSR5Included = false;
            foreach (TreeNode objNode in treSourcebook.Nodes)
            {
                if (!objNode.Checked)
                    continue;

                _characterOptions.Books.Add(objNode.Tag.ToString());

                if (objNode.Tag.ToString() == "SR5")
                    blnSR5Included = true;
            }

            // If the SR5 book was somehow missed, add it back.
            if (!blnSR5Included)
                _characterOptions.Books.Add("SR5");
        }

        private void CloseCreateForm()
        {
            Form fc = Application.OpenForms["frmCreate"];

            if (fc == null)
                return;

            string text = LanguageManager.Instance.GetString("Message_Options_CloseForms");
            string caption = LanguageManager.Instance.GetString("MessageTitle_Options_CloseForms");

            if (MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            fc = Application.OpenForms["frmCreate"];

            if (fc != null)
                fc.Close();
        }

        private void RestoreDefaultKarmaValues()
        {
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
            nudKarmaEnemy.Value = 1;
            nudKarmaCarryover.Value = 7;
            nudKarmaInitiation.Value = 3;
            nudKarmaMetamagic.Value = 15;
            nudKarmaJoinGroup.Value = 5;
            nudKarmaLeaveGroup.Value = 1;
        }

        private void RestoreDefaultKarmaFociValues()
        {
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

        private void PopulateBuildMethodList()
        {
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

            cboBuildMethod.ValueMember = "Value";
            cboBuildMethod.DisplayMember = "Name";
            cboBuildMethod.DataSource = lstBuildMethod;
        }

        private void PopulateEssenceDecimalsList()
        {
            List<ListItem> lstDecimals = new List<ListItem>();

            ListItem objTwo = new ListItem();
            objTwo.Value = "2";
            objTwo.Name = "2";

            ListItem objFour = new ListItem();
            objFour.Value = "4";
            objFour.Name = "4";

            lstDecimals.Add(objTwo);
            lstDecimals.Add(objFour);

            cboEssenceDecimals.ValueMember = "Value";
            cboEssenceDecimals.DisplayMember = "Name";
            cboEssenceDecimals.DataSource = lstDecimals;
        }

        private void PopulateLimbCountList()
        {
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

            cboLimbCount.ValueMember = "Value";
            cboLimbCount.DisplayMember = "Name";
            cboLimbCount.DataSource = lstLimbCount;
        }

        private void SetToolTips()
        {
            const int width = 50;
            var functions = new CommonFunctions();

            tipTooltip.SetToolTip(chkKnucks, functions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsKnucks"), width));
            tipTooltip.SetToolTip(chkIgnoreArt, functions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsIgnoreArt"), width));
            tipTooltip.SetToolTip(chkCyberlegMovement, functions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsCyberlegMovement"), width));
            tipTooltip.SetToolTip(chkDontDoubleQualities, functions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsDontDoubleQualities"), width));
            tipTooltip.SetToolTip(chkUsePointsOnBrokenGroups, functions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsUsePointsOnBrokenGroups"), width));
            tipTooltip.SetToolTip(chkAllowInitiation, functions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsAllowInitiation"), width));
        }

        private void PopulateSettingsList()
        {
            List<ListItem> lstSettings = new List<ListItem>();
            string settingsDirectoryPath = Path.Combine(Environment.CurrentDirectory, "settings");
            string[] settingsFilePaths = Directory.GetFiles(settingsDirectoryPath, "*.xml");

            foreach (string filePath in settingsFilePaths)
            {
                XmlDocument xmlDocument = new XmlDocument();

                try
                {
                    xmlDocument.Load(filePath);
                }
                catch (XmlException)
                {
                    continue;
                }

                XmlNode node = xmlDocument.SelectSingleNode("/settings/name");

                if (node == null)
                    continue;

                string settingName = node.InnerText;

                ListItem objItem = new ListItem();
                objItem.Value = Path.GetFileName(filePath);
                objItem.Name = settingName;

                lstSettings.Add(objItem);
            }

            cboSetting.ValueMember = "Value";
            cboSetting.DisplayMember = "Name";
            cboSetting.DataSource = lstSettings;
        }

        private void PopulateLanguageList()
        {
            List<ListItem> lstLanguages = new List<ListItem>();
            string languageDirectoryPath = Path.Combine(Environment.CurrentDirectory, "lang");
            string[] languageFilePaths = Directory.GetFiles(languageDirectoryPath, "*.xml");

            foreach (string filePath in languageFilePaths)
            {
                XmlDocument xmlDocument = new XmlDocument();

                try
                {
                    xmlDocument.Load(filePath);
                }
                catch (XmlException)
                {
                    continue;
                }

                XmlNode node = xmlDocument.SelectSingleNode("/chummer/name");

                if (node == null)
                    continue;

                string languageName = node.InnerText;

                ListItem objItem = new ListItem();
                objItem.Value = Path.GetFileNameWithoutExtension(filePath);
                objItem.Name = languageName;

                lstLanguages.Add(objItem);
            }

            SortListItem objSort = new SortListItem();
            lstLanguages.Sort(objSort.Compare);

            cboLanguage.ValueMember = "Value";
            cboLanguage.DisplayMember = "Name";
            cboLanguage.DataSource = lstLanguages;
        }

        private void PopulateGlobalOptions()
        {
            chkAutomaticUpdate.Checked = GlobalOptions.Instance.AutomaticUpdate;
            chkUseLogging.Checked = GlobalOptions.Instance.UseLogging;
            chkLifeModule.Checked = GlobalOptions.Instance.LifeModuleEnabled;
            chkLocalisedUpdatesOnly.Checked = GlobalOptions.Instance.LocalisedUpdatesOnly;
            chkStartupFullscreen.Checked = GlobalOptions.Instance.StartupFullscreen;
            chkSingleDiceRoller.Checked = GlobalOptions.Instance.SingleDiceRoller;
            chkDatesIncludeTime.Checked = GlobalOptions.Instance.DatesIncludeTime;
            chkMissions.Checked = GlobalOptions.Instance.MissionsOnly;
            chkPrintToFileFirst.Checked = GlobalOptions.Instance.PrintToFileFirst;
            chkOpenPDFsAsURLs.Checked = GlobalOptions.Instance.OpenPDFsAsURLs;
            txtPDFAppPath.Text = GlobalOptions.Instance.PDFAppPath;
            txtURLAppPath.Text = GlobalOptions.Instance.URLAppPath;
        }

        private List<string> ReadXslFileNamesWithoutExtensionFromDirectory(string path)
        {
            var names = new List<string>();

            if (Directory.Exists(path))
            {
                names = Directory.GetFiles(path)
                    .Where(s => s.EndsWith(".xsl"))
                    .Select(Path.GetFileNameWithoutExtension).ToList();
            }

            return names;
        }

        private List<ListItem> GetXslFilesFromSheetsDirectory()
        {
            var items = new List<ListItem>();

            // Populate the XSLT list with all of the XSL files found in the sheets directory.
            string sheetsDirectoryPath = Path.Combine(Environment.CurrentDirectory, "sheets");

            // Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets 
            // (hidden because they are partial templates that cannot be used on their own).
            List<string> fileNames = ReadXslFileNamesWithoutExtensionFromDirectory(sheetsDirectoryPath);

            foreach (string fileName in fileNames)
            {
                ListItem objItem = new ListItem();
                objItem.Value = fileName;
                objItem.Name = fileName;

                items.Add(objItem);
            }

            return items;
        }

        private List<ListItem> GetXslFilesFromLanguageDirectory()
        {
            var items = new List<ListItem>();

            // Populate the XSL list with all of the XSL files found in the sheets\[language] directory.
            if (GlobalOptions.Instance.Language != "en-us")
            {
                XmlDocument objLanguageDocument = LanguageManager.Instance.XmlDoc;
                string strLanguage = objLanguageDocument.SelectSingleNode("/chummer/name").InnerText;
                string languageDirectoryPath = Path.Combine(Environment.CurrentDirectory, "sheets", GlobalOptions.Instance.Language);

                // Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets 
                // (hidden because they are partial templates that cannot be used on their own).
                List<string> fileNames = ReadXslFileNamesWithoutExtensionFromDirectory(languageDirectoryPath);

                foreach (string fileName in fileNames)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = Path.Combine(GlobalOptions.Instance.Language, fileName);
                    objItem.Name = strLanguage + ": " + fileName;

                    items.Add(objItem);
                }
            }

            return items;
        }

        private List<ListItem> GetXslFilesFromOmaeDirectory()
        {
            var items = new List<ListItem>();

            // Populate the XSLT list with all of the XSL files found in the sheets\omae directory.
            string omaeDirectoryPath = Path.Combine(Environment.CurrentDirectory, "sheets", "omae");
            string menuMainOmae = LanguageManager.Instance.GetString("Menu_Main_Omae");

            // Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets 
            // (hidden because they are partial templates that cannot be used on their own).
            List<string> fileNames = ReadXslFileNamesWithoutExtensionFromDirectory(omaeDirectoryPath);

            foreach (string fileName in fileNames)
            {
                ListItem objItem = new ListItem();
                objItem.Value = Path.Combine("omae", fileName);
                objItem.Name = menuMainOmae + ": " + fileName;

                items.Add(objItem);
            }

            return items;
        }

        private void PopulateXsltList()
        {
            List<ListItem> lstFiles = new List<ListItem>();

            lstFiles.AddRange(GetXslFilesFromSheetsDirectory());
            lstFiles.AddRange(GetXslFilesFromLanguageDirectory());
            lstFiles.AddRange(GetXslFilesFromOmaeDirectory());

            cboXSLT.ValueMember = "Value";
            cboXSLT.DisplayMember = "Name";
            cboXSLT.DataSource = lstFiles;
        }

        private void SetDefaultValueForSettingsList()
        {
            // Attempt to make default.xml the default one. If it could not be found in the list, select the first item instead.
            cboSetting.SelectedIndex = cboSetting.FindStringExact("Default Settings");

            if (cboSetting.SelectedIndex == -1)
                cboSetting.SelectedIndex = 0;
        }

        private void SetDefaultValueForLanguageList()
        {
            cboLanguage.SelectedValue = GlobalOptions.Instance.Language;

            if (cboLanguage.SelectedIndex == -1)
                cboLanguage.SelectedValue = "en-us";
        }

        private void SetDefaultValueForXsltList()
        {
            if (string.IsNullOrEmpty(GlobalOptions.Instance.DefaultCharacterSheet))
                GlobalOptions.Instance.DefaultCharacterSheet = "Shadowrun 5";

            cboXSLT.SelectedValue = GlobalOptions.Instance.DefaultCharacterSheet;
        }

        private void UpdateSourcebookInfoPath(string path)
        {
            string tag = treSourcebook.SelectedNode.Tag.ToString();
            SourcebookInfo foundSource = GlobalOptions.Instance.SourcebookInfo.FirstOrDefault(x => x.Code == tag);

            if (foundSource != null)
            {
                foundSource.Path = path;
            }
            else
            {
                // If the Sourcebook was not found in the options, add it.
                var newSource = new SourcebookInfo();
                newSource.Code = tag;
                newSource.Path = path;
                GlobalOptions.Instance.SourcebookInfo.Add(newSource);
            }
        }
        #endregion
    }
}