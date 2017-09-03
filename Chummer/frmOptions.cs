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
 using System.Net;
 using Application = System.Windows.Forms.Application;

namespace Chummer
{
    public partial class frmOptions : Form
    {
        private readonly CharacterOptions _characterOptions = new CharacterOptions(null);
        private bool _skipRefresh;
        private bool blnDirty = false;
        private bool blnLoading = true;
        private bool blnSourcebookToggle = true;
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
            PopulateSettingsList();
            SetDefaultValueForSettingsList();
            PopulateGlobalOptions();
            PopulateLanguageList();
            SetDefaultValueForLanguageList();
            PopulateXsltList();
            SetDefaultValueForXsltList();
            PopulatePDFParameters();
            MoveControls();
            blnLoading = false;
        }
        #endregion

        #region Control Events
        private void cmdOK_Click(object sender, EventArgs e)
        {
            // Make sure the current Setting has a name.
            if (string.IsNullOrEmpty(txtSettingName.Text.Trim()))
            {
                MessageBox.Show("You must give your Settings a name.", "Chummer Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSettingName.Focus();
                return;
            }

            if (blnDirty)
            {
                string text = LanguageManager.Instance.GetString("Message_Options_SaveForms");
                string caption = LanguageManager.Instance.GetString("MessageTitle_Options_CloseForms");

                switch (MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        break;
                    default:
                        return;
                }
                Utils.RestartApplication("Message_Options_CloseForms");
            }

            DialogResult = DialogResult.OK;

            SaveRegistrySettings();
            BuildBooksList();

            _characterOptions.AllowCustomTransgenics = chkAllowCustomTransgenics.Checked;
            _characterOptions.AllowCyberwareESSDiscounts = chkAllowCyberwareESSDiscounts.Checked;
            _characterOptions.AllowInitiationInCreateMode = chkAllowInitiation.Checked;
            _characterOptions.AllowSkillDiceRolling = chkAllowSkillDiceRolling.Checked;
            _characterOptions.DontUseCyberlimbCalculation = chkDontUseCyberlimbCalculation.Checked;
            _characterOptions.AllowSkillRegrouping = chkAllowSkillRegrouping.Checked;
            _characterOptions.AlternateMatrixAttribute = chkAlternateMatrixAttribute.Checked;
            _characterOptions.AlternateComplexFormCost = chkAlternateComplexFormCost.Checked;
            _characterOptions.ArmorDegradation = chkArmorDegradation.Checked;
            _characterOptions.AutomaticCopyProtection = chkAutomaticCopyProtection.Checked;
            _characterOptions.AutomaticRegistration = chkAutomaticRegistration.Checked;
            _characterOptions.CalculateCommlinkResponse = chkCalculateCommlinkResponse.Checked;
            _characterOptions.CapSkillRating = chkCapSkillRating.Checked;
            _characterOptions.ConfirmDelete = chkConfirmDelete.Checked;
            _characterOptions.ConfirmKarmaExpense = chkConfirmKarmaExpense.Checked;
            _characterOptions.CreateBackupOnCareer = chkCreateBackupOnCareer.Checked;
            _characterOptions.CyberlegMovement = chkCyberlegMovement.Checked;
            _characterOptions.UseTotalValueForFreeContacts = chkUseTotalValueForFreeContacts.Checked;
            _characterOptions.UseTotalValueForFreeKnowledge = chkUseTotalValueForFreeKnowledge.Checked;
            _characterOptions.DontDoubleQualityPurchases = chkDontDoubleQualityPurchases.Checked;
            _characterOptions.DontDoubleQualityRefunds = chkDontDoubleQualityRefunds.Checked;
            _characterOptions.EnforceCapacity = chkEnforceCapacity.Checked;
            _characterOptions.EnforceMaximumSkillRatingModifier = chkEnforceSkillMaximumModifiedRating.Checked;
            _characterOptions.ErgonomicProgramLimit = chkErgonomicProgramLimit.Checked;
            _characterOptions.EssenceDecimals = Convert.ToInt32(cboEssenceDecimals.SelectedValue);
            _characterOptions.ESSLossReducesMaximumOnly = chkESSLossReducesMaximumOnly.Checked;
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
            _characterOptions.DroneArmorMultiplier = Convert.ToInt32(nudDroneArmorMultiplier.Value);
            _characterOptions.DroneArmorMultiplierEnabled = chkDroneArmorMultiplier.Checked;
            nudDroneArmorMultiplier.Enabled = chkDroneArmorMultiplier.Checked;
            _characterOptions.FreeKnowledgeMultiplierEnabled = chkKnowledgeMultiplier.Checked;
                if (chkKnowledgeMultiplier.Checked)
                    chkKnowledgeMultiplier.Enabled = true;
            _characterOptions.FreeKnowledgeMultiplier = Convert.ToInt32(nudKnowledgeMultiplier.Value);
            _characterOptions.HideItemsOverAvailLimit = chkHideItemsOverAvail.Checked;
            _characterOptions.IgnoreArt = chkIgnoreArt.Checked;
            _characterOptions.UnarmedImprovementsApplyToWeapons = chkUnarmedSkillImprovements.Checked;
            _characterOptions.LicenseRestricted = chkLicenseEachRestrictedItem.Checked;
            _characterOptions.MaximumArmorModifications = chkMaximumArmorModifications.Checked;
            _characterOptions.MetatypeCostsKarma = chkMetatypeCostsKarma.Checked;
            _characterOptions.ReverseAttributePriorityOrder = chkReverseAttributePriorityOrder.Checked;
            _characterOptions.MetatypeCostsKarmaMultiplier = Convert.ToInt32(nudMetatypeCostsKarmaMultiplier.Value);
            _characterOptions.MoreLethalGameplay = chkMoreLethalGameplay.Checked;
            _characterOptions.NoSingleArmorEncumbrance = chkNoSingleArmorEncumbrance.Checked;
            _characterOptions.NuyenPerBP = Convert.ToInt32(nudKarmaNuyenPer.Value);
            _characterOptions.PrintExpenses = chkPrintExpenses.Checked;
            _characterOptions.PrintNotes = chkPrintNotes.Checked;
            _characterOptions.PrintSkillsWithZeroRating = chkPrintSkillsWithZeroRating.Checked;
            _characterOptions.RestrictRecoil = chkRestrictRecoil.Checked;
            _characterOptions.StrengthAffectsRecoil = Convert.ToBoolean(chkStrengthAffectsRecoil.Checked);
            _characterOptions.UseCalculatedPublicAwareness = chkUseCalculatedPublicAwareness.Checked;
            _characterOptions.StrictSkillGroupsInCreateMode = chkStrictSkillGroups.Checked;
            _characterOptions.AlternateMetatypeAttributeKarma = chkAlternateMetatypeAttributeKarma.Checked;
            _characterOptions.MysaddPPCareer = chkMysAdPp.Checked;
            _characterOptions.FreeMartialArtSpecialization = chkFreeMartialArtSpecialization.Checked;
            _characterOptions.PrioritySpellsAsAdeptPowers = chkPrioritySpellsAsAdeptPowers.Checked;
            _characterOptions.EducationQualitiesApplyOnChargenKarma = chkEducationQualitiesApplyOnChargenKarma.Checked;
            _characterOptions.LimbCount = Convert.ToInt32(cboLimbCount.SelectedValue.ToString().Split('/')[0]);
            _characterOptions.ExcludeLimbSlot = cboLimbCount.SelectedValue.ToString().Split('/')[1];

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
            _characterOptions.KarmaNewAIProgram = Convert.ToInt32(nudKarmaNewAIProgram.Value);
            _characterOptions.KarmaNewAIAdvancedProgram = Convert.ToInt32(nudKarmaNewAIAdvancedProgram.Value);
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
            _characterOptions.KarmaNewAIProgram = Convert.ToInt32(nudKarmaNewAIProgram.Value);
            _characterOptions.KarmaNewAIAdvancedProgram = Convert.ToInt32(nudKarmaNewAIAdvancedProgram.Value);
            _characterOptions.AllowHoverIncrement = chkAllowHoverIncrement.Checked;

            // Focus costs
            _characterOptions.KarmaAlchemicalFocus = Convert.ToInt32(nudKarmaAlchemicalFocus.Value);
            _characterOptions.KarmaBanishingFocus = Convert.ToInt32(nudKarmaBanishingFocus.Value);
            _characterOptions.KarmaBindingFocus = Convert.ToInt32(nudKarmaBindingFocus.Value);
            _characterOptions.KarmaCenteringFocus = Convert.ToInt32(nudKarmaCenteringFocus.Value);
            _characterOptions.KarmaCounterspellingFocus = Convert.ToInt32(nudKarmaCounterspellingFocus.Value);
            _characterOptions.KarmaDisenchantingFocus = Convert.ToInt32(nudKarmaDisenchantingFocus.Value);
            _characterOptions.KarmaFlexibleSignatureFocus = Convert.ToInt32(nudKarmaFlexibleSignatureFocus.Value);
            _characterOptions.KarmaMaskingFocus = Convert.ToInt32(nudKarmaMaskingFocus.Value);
            _characterOptions.KarmaPowerFocus = Convert.ToInt32(nudKarmaPowerFocus.Value);
            _characterOptions.KarmaQiFocus = Convert.ToInt32(nudKarmaQiFocus.Value);
            _characterOptions.KarmaRitualSpellcastingFocus = Convert.ToInt32(nudKarmaRitualSpellcastingFocus.Value);
            _characterOptions.KarmaSpellcastingFocus = Convert.ToInt32(nudKarmaSpellcastingFocus.Value);
            _characterOptions.KarmaSpellShapingFocus = Convert.ToInt32(nudKarmaSpellShapingFocus.Value);
            _characterOptions.KarmaSummoningFocus = Convert.ToInt32(nudKarmaSummoningFocus.Value);
            _characterOptions.KarmaSustainingFocus = Convert.ToInt32(nudKarmaSustainingFocus.Value);
            _characterOptions.KarmaWeaponFocus = Convert.ToInt32(nudKarmaWeaponFocus.Value);

            // Build Priority options.
            _characterOptions.MayBuyQualities = chkMayBuyQualities.Checked;
            _characterOptions.UseContactPoints = chkContactPoints.Checked;

            // Build method options.
            _characterOptions.BuildMethod = cboBuildMethod.SelectedValue.ToString();
            _characterOptions.BuildPoints = Convert.ToInt32(nudBP.Value);
            _characterOptions.Availability = Convert.ToInt32(nudMaxAvail.Value);

            _characterOptions.Name = txtSettingName.Text;
            _characterOptions.Save();
        }

        private void cboBuildMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboBuildMethod.SelectedValue != null)
            {
            if (cboBuildMethod.SelectedValue.ToString() == LanguageManager.Instance.GetString("String_Karma"))
                nudBP.Value = 800;
            else if (cboBuildMethod.SelectedValue.ToString() == LanguageManager.Instance.GetString("String_LifeModule"))
                nudBP.Value = 750;
        }
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
            OptionsChanged(sender,e);
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
            _characterOptions.RecalculateBookXPath();

            XmlManager.Instance.Verify(cboLanguage.SelectedValue.ToString(), lstBooks);

            string strFilePath = Path.Combine(Application.StartupPath, "lang", "results_" + cboLanguage.SelectedValue + ".xml");
            MessageBox.Show("Results were written to " + strFilePath, "Validation Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void chkExceedNegativeQualities_CheckedChanged(object sender, EventArgs e)
        {
            chkExceedNegativeQualitiesLimit.Enabled = chkExceedNegativeQualities.Checked;
            if (!chkExceedNegativeQualitiesLimit.Enabled)
                chkExceedNegativeQualitiesLimit.Checked = false;
            OptionsChanged(sender,e);
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

        private void chkDroneArmorMultiplier_CheckedChanged(object sender, EventArgs e)
        {
            nudDroneArmorMultiplier.Enabled = chkDroneArmorMultiplier.Checked;
            if (!chkDroneArmorMultiplier.Checked)
            {
                nudDroneArmorMultiplier.Value = 2;
            }
            OptionsChanged(sender, e);
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

        private void cmdPDFLocation_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*";

                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    UpdateSourcebookInfoPath(openFileDialog.FileName);
                    txtPDFLocation.Text = openFileDialog.FileName;
                }
            }
        }

        private void treSourcebook_AfterSelect(object sender, TreeViewEventArgs e)
        {
            cmdPDFLocation.Enabled = true;
            nudPDFOffset.Enabled = true;
            cmdPDFTest.Enabled = true;

            _skipRefresh = true;
            txtPDFLocation.Text = string.Empty;
            nudPDFOffset.Value = 0;
            _skipRefresh = false;

            // Find the selected item in the Sourcebook List.
            foreach (SourcebookInfo objSource in GlobalOptions.Instance.SourcebookInfo.Where(objSource => objSource.Code == treSourcebook.SelectedNode.Tag.ToString()))
            {
                txtPDFLocation.Text = objSource.Path;
                nudPDFOffset.Value = objSource.Offset;
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
            //if (e.Node.Tag.ToString() == "SR5")
                //e. = true;
        }

        private void cmdPDFTest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPDFLocation.Text))
                return;

            SaveRegistrySettings();

            CommonFunctions.StaticOpenPDF(treSourcebook.SelectedNode.Tag + " 5");
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
            intWidth = Math.Max(intWidth, lblKarmaAlchemicalFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaBanishingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaBindingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaCenteringFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaCounterspellingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaDisenchantingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaFlexibleSignatureFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaMaskingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaPowerFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaQiFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaRitualSpellcastingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaSpellcastingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaSummoningFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaSustainingFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaWeaponFocus.Width);
            intWidth = Math.Max(intWidth, lblKarmaNewAIProgram.Width);
            intWidth = Math.Max(intWidth, lblKarmaNewAIAdvancedProgram.Width);

            nudKarmaMetamagic.Left = lblKarmaMetamagic.Left + intWidth + 6;
            nudKarmaJoinGroup.Left = nudKarmaMetamagic.Left;
            nudKarmaLeaveGroup.Left = nudKarmaMetamagic.Left;
            nudKarmaAlchemicalFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaBanishingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaBindingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaCenteringFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaCounterspellingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaDisenchantingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaFlexibleSignatureFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaMaskingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaPowerFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaRitualSpellcastingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaSpellcastingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaSpellShapingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaSummoningFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaSustainingFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaQiFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaWeaponFocus.Left = nudKarmaMetamagic.Left;
            nudKarmaNewAIProgram.Left = nudKarmaMetamagic.Left;
            nudKarmaNewAIAdvancedProgram.Left = nudKarmaMetamagic.Left;

            lblKarmaAlchemicalFocusExtra.Left = nudKarmaAlchemicalFocus.Left + nudKarmaAlchemicalFocus.Width + 6;
            lblKarmaBanishingFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;
            lblKarmaBindingFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;
            lblKarmaCenteringFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;
            lblKarmaCounterspellingFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;
            lblKarmaDisenchantingFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;
            lblFlexibleSignatureFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;
            lblKarmaMaskingFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;
            lblKarmaPowerFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;
            lblKarmaQiFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;
            lblKarmaRitualSpellcastingFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;
            lblKarmaSpellcastingFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;
            lblKarmaSpellShapingFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;
            lblKarmaSummoningFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;
            lblKarmaSustainingFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;
            lblKarmaWeaponFocusExtra.Left = lblKarmaAlchemicalFocusExtra.Left;

            // Determine where the widest control ends so we can change the window with to accommodate it.
            intWidth = (from Control objControl in tabGeneral.Controls select objControl.Left + objControl.Width).Concat(new[] {intWidth}).Max();
            intWidth = (from Control objControl in tabKarmaCosts.Controls select objControl.Left + objControl.Width).Concat(new[] {intWidth}).Max();
            intWidth = (from Control objControl in tabOptionalRules.Controls select objControl.Left + objControl.Width).Concat(new[] {intWidth}).Max();
            intWidth = (from Control objControl in tabHouseRules.Controls select objControl.Left + objControl.Width).Concat(new[] {intWidth}).Max();

            // Change the window size.
            Width = intWidth + 29;
            Height = tabControl1.Top + tabControl1.Height + cmdOK.Height + 55;

            intWidth = (from TreeNode objNode in treSourcebook.Nodes select objNode.Bounds.Left * 2 + objNode.Bounds.Width).Concat(new[] { treSourcebook.Width }).Max();
            treSourcebook.Width = intWidth + 12;
            cmdEnableSourcebooks.Left = treSourcebook.Left;
            cmdEnableSourcebooks.Width = treSourcebook.Width;
            tabControl2.Left = treSourcebook.Right + 6;
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

                objNode.Text = objXmlBook["translate"]?.InnerText ?? objXmlBook["name"].InnerText;

                objNode.Tag = objXmlBook["code"].InnerText;
                objNode.Checked = blnChecked;
                treSourcebook.Nodes.Add(objNode);
            }

            treSourcebook.Sort();
        }

        private void SetDefaultValueForLimbCount()
        {
            string strDefaultValue = _characterOptions.LimbCount+"/"+_characterOptions.ExcludeLimbSlot;
                cboLimbCount.SelectedValue = strDefaultValue;
        }

        /// <summary>
        /// Set the values for all of the controls based on the Options for the selected Setting.
        /// </summary>
        private void PopulateOptions()
        {
            PopulateSourcebookTreeView();

            cboBuildMethod.SelectedValue = _characterOptions.BuildMethod;
            cboEssenceDecimals.SelectedValue = _characterOptions.EssenceDecimals == 0 ? "2" : _characterOptions.EssenceDecimals.ToString();
            chkAllowCustomTransgenics.Checked = _characterOptions.AllowCustomTransgenics;
            chkAllowCyberwareESSDiscounts.Checked = _characterOptions.AllowCyberwareESSDiscounts;
            chkAllowInitiation.Checked = _characterOptions.AllowInitiationInCreateMode;
            chkAllowSkillDiceRolling.Checked = _characterOptions.AllowSkillDiceRolling;
            chkDontUseCyberlimbCalculation.Checked = _characterOptions.DontUseCyberlimbCalculation;
            chkAllowSkillRegrouping.Checked = _characterOptions.AllowSkillRegrouping;
            chkAlternateComplexFormCost.Checked = _characterOptions.AlternateComplexFormCost;
            chkAlternateMatrixAttribute.Checked = _characterOptions.AlternateMatrixAttribute;
            chkArmorDegradation.Checked = _characterOptions.ArmorDegradation;
            chkArmorSuitCapacity.Checked = _characterOptions.ArmorSuitCapacity;
            chkAutomaticCopyProtection.Checked = _characterOptions.AutomaticCopyProtection;
            chkAutomaticRegistration.Checked = _characterOptions.AutomaticRegistration;
            chkCalculateCommlinkResponse.Checked = _characterOptions.CalculateCommlinkResponse;
            chkCapSkillRating.Checked = _characterOptions.CapSkillRating;
            chkConfirmDelete.Checked = _characterOptions.ConfirmDelete;
            chkConfirmKarmaExpense.Checked = _characterOptions.ConfirmKarmaExpense;
            chkUseTotalValueForFreeContacts.Checked = _characterOptions.UseTotalValueForFreeContacts;
            chkUseTotalValueForFreeKnowledge.Checked = _characterOptions.UseTotalValueForFreeKnowledge;
            chkContactMultiplier.Checked = _characterOptions.FreeContactsMultiplierEnabled;
            chkDroneArmorMultiplier.Checked = _characterOptions.DroneArmorMultiplierEnabled;
            chkContactPoints.Checked = _characterOptions.UseContactPoints;
            chkCreateBackupOnCareer.Checked = _characterOptions.CreateBackupOnCareer;
            chkCyberlegMovement.Checked = _characterOptions.CyberlegMovement;
            chkMysAdPp.Checked = _characterOptions.MysaddPPCareer;
            chkHideItemsOverAvail.Checked = _characterOptions.HideItemsOverAvailLimit;
            chkFreeMartialArtSpecialization.Checked = _characterOptions.FreeMartialArtSpecialization;
            chkPrioritySpellsAsAdeptPowers.Checked = _characterOptions.PrioritySpellsAsAdeptPowers;
            chkEducationQualitiesApplyOnChargenKarma.Checked = _characterOptions.EducationQualitiesApplyOnChargenKarma;
            chkDontDoubleQualityPurchases.Checked = _characterOptions.DontDoubleQualityPurchases;
            chkDontDoubleQualityRefunds.Checked = _characterOptions.DontDoubleQualityRefunds;
            chkEnforceCapacity.Checked = _characterOptions.EnforceCapacity;
            chkEnforceSkillMaximumModifiedRating.Checked = _characterOptions.EnforceMaximumSkillRatingModifier;
            chkErgonomicProgramLimit.Checked = _characterOptions.ErgonomicProgramLimit;
            chkESSLossReducesMaximumOnly.Checked = _characterOptions.ESSLossReducesMaximumOnly;
            chkExceedNegativeQualities.Checked = _characterOptions.ExceedNegativeQualities;
            chkExceedNegativeQualitiesLimit.Checked = _characterOptions.ExceedNegativeQualitiesLimit;
            chkExceedNegativeQualitiesLimit.Enabled = chkExceedNegativeQualities.Checked;
            chkExceedPositiveQualities.Checked = _characterOptions.ExceedPositiveQualities;
            chkExtendAnyDetectionSpell.Checked = _characterOptions.ExtendAnyDetectionSpell;
            chkIgnoreArt.Checked = _characterOptions.IgnoreArt;
            chkKnowledgeMultiplier.Checked = _characterOptions.FreeKnowledgeMultiplierEnabled;
            chkUnarmedSkillImprovements.Checked = _characterOptions.UnarmedImprovementsApplyToWeapons;
            chkLicenseEachRestrictedItem.Checked = _characterOptions.LicenseRestricted;
            chkMaximumArmorModifications.Checked = _characterOptions.MaximumArmorModifications;
            chkMayBuyQualities.Checked = _characterOptions.MayBuyQualities;
            chkMetatypeCostsKarma.Checked = _characterOptions.MetatypeCostsKarma;
            chkMoreLethalGameplay.Checked = _characterOptions.MoreLethalGameplay;
            chkNoSingleArmorEncumbrance.Checked = _characterOptions.NoSingleArmorEncumbrance;
            chkPrintExpenses.Checked = _characterOptions.PrintExpenses;
            chkPrintNotes.Checked = _characterOptions.PrintNotes;
            chkPrintSkillsWithZeroRating.Checked = _characterOptions.PrintSkillsWithZeroRating;
            chkRestrictRecoil.Checked = _characterOptions.RestrictRecoil;
            chkSpecialKarmaCost.Checked = _characterOptions.SpecialKarmaCostBasedOnShownValue;
            chkStrengthAffectsRecoil.Checked = _characterOptions.StrengthAffectsRecoil;
            chkUseCalculatedPublicAwareness.Checked = _characterOptions.UseCalculatedPublicAwareness;
            chkStrictSkillGroups.Checked = _characterOptions.StrictSkillGroupsInCreateMode;
            chkAlternateMetatypeAttributeKarma.Checked = _characterOptions.AlternateMetatypeAttributeKarma;
            chkReverseAttributePriorityOrder.Checked = _characterOptions.ReverseAttributePriorityOrder;
            chkAllowHoverIncrement.Checked = _characterOptions.AllowHoverIncrement;
            nudBP.Value = _characterOptions.BuildPoints;
            nudContactMultiplier.Enabled = _characterOptions.FreeContactsMultiplierEnabled;
            nudContactMultiplier.Value = _characterOptions.FreeContactsMultiplier;
            nudKnowledgeMultiplier.Enabled = _characterOptions.FreeKnowledgeMultiplierEnabled;
            nudKnowledgeMultiplier.Value = _characterOptions.FreeKnowledgeMultiplier;
            nudDroneArmorMultiplier.Enabled = _characterOptions.DroneArmorMultiplierEnabled;
            nudDroneArmorMultiplier.Value = _characterOptions.DroneArmorMultiplier;
            nudMaxAvail.Value = _characterOptions.Availability;
            nudMetatypeCostsKarmaMultiplier.Value = _characterOptions.MetatypeCostsKarmaMultiplier;
            nudNuyenPerBP.Value = _characterOptions.NuyenPerBP;
            txtSettingName.Enabled = cboSetting.SelectedValue.ToString() != "default.xml";
            txtSettingName.Text = _characterOptions.Name;

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
            nudKarmaNewAIProgram.Value = _characterOptions.KarmaNewAIProgram;
            nudKarmaNewAIAdvancedProgram.Value = _characterOptions.KarmaNewAIAdvancedProgram;
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
            nudKarmaAlchemicalFocus.Value = _characterOptions.KarmaAlchemicalFocus;
            nudKarmaBanishingFocus.Value = _characterOptions.KarmaBanishingFocus;
            nudKarmaBindingFocus.Value = _characterOptions.KarmaBindingFocus;
            nudKarmaCenteringFocus.Value = _characterOptions.KarmaCenteringFocus;
            nudKarmaCounterspellingFocus.Value = _characterOptions.KarmaCounterspellingFocus;
            nudKarmaDisenchantingFocus.Value = _characterOptions.KarmaDisenchantingFocus;
            nudKarmaFlexibleSignatureFocus.Value = _characterOptions.KarmaFlexibleSignatureFocus;
            nudKarmaMaskingFocus.Value = _characterOptions.KarmaMaskingFocus;
            nudKarmaPowerFocus.Value = _characterOptions.KarmaPowerFocus;
            nudKarmaQiFocus.Value = _characterOptions.KarmaQiFocus;
            nudKarmaRitualSpellcastingFocus.Value = _characterOptions.KarmaRitualSpellcastingFocus;
            nudKarmaSpellcastingFocus.Value = _characterOptions.KarmaSpellcastingFocus;
            nudKarmaSpellShapingFocus.Value = _characterOptions.KarmaSpellShapingFocus;
            nudKarmaSummoningFocus.Value = _characterOptions.KarmaSummoningFocus;
            nudKarmaSustainingFocus.Value = _characterOptions.KarmaSustainingFocus;
            nudKarmaWeaponFocus.Value = _characterOptions.KarmaWeaponFocus;
        }

        private void SaveGlobalOptions()
        {
            GlobalOptions.Instance.AutomaticUpdate = chkAutomaticUpdate.Checked;
            GlobalOptions.Instance.LiveCustomData = chkLiveCustomData.Checked;
            GlobalOptions.Instance.UseLogging = chkUseLogging.Checked;
            GlobalOptions.Instance.Language = cboLanguage.SelectedValue.ToString();
            GlobalOptions.Instance.StartupFullscreen = chkStartupFullscreen.Checked;
            GlobalOptions.Instance.SingleDiceRoller = chkSingleDiceRoller.Checked;
            if (cboXSLT.SelectedValue == null || string.IsNullOrEmpty(cboXSLT.SelectedValue.ToString()))
            {
                cboXSLT.SelectedValue = "Shadowrun 5";
            }
            GlobalOptions.Instance.DefaultCharacterSheet = cboXSLT.SelectedValue.ToString();
            GlobalOptions.Instance.DatesIncludeTime = chkDatesIncludeTime.Checked;
            GlobalOptions.Instance.PrintToFileFirst = chkPrintToFileFirst.Checked;
            GlobalOptions.Instance.PDFAppPath = txtPDFAppPath.Text;
            GlobalOptions.Instance.PDFParameters = cboPDFParameters.SelectedValue.ToString();
            GlobalOptions.Instance.LifeModuleEnabled = chkLifeModule.Checked;
            GlobalOptions.Instance.OmaeEnabled = chkOmaeEnabled.Checked;
            GlobalOptions.Instance.PreferNightlyBuilds = chkPreferNightlyBuilds.Checked;
            GlobalOptions.Instance.MissionsOnly = chkMissions.Checked;
            GlobalOptions.Instance.Dronemods = chkDronemods.Checked;
            GlobalOptions.Instance.DronemodsMaximumPilot = chkDronemodsMaximumPilot.Checked;
            GlobalOptions.Instance.CharacterRosterPath = txtCharacterRosterPath.Text;
        }

        /// <summary>
        /// Save the global settings to the registry.
        /// </summary>
        private void SaveRegistrySettings()
        {
            SaveGlobalOptions();

            Microsoft.Win32.RegistryKey objRegistry = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
            objRegistry.SetValue("autoupdate", chkAutomaticUpdate.Checked.ToString());
            objRegistry.SetValue("livecustomdata", chkLiveCustomData.Checked.ToString());
            objRegistry.SetValue("uselogging", chkUseLogging.Checked.ToString());
            objRegistry.SetValue("language", cboLanguage.SelectedValue.ToString());
            objRegistry.SetValue("startupfullscreen", chkStartupFullscreen.Checked.ToString());
            objRegistry.SetValue("singlediceroller", chkSingleDiceRoller.Checked.ToString());
            objRegistry.SetValue("defaultsheet", cboXSLT.SelectedValue.ToString());
            objRegistry.SetValue("datesincludetime", chkDatesIncludeTime.Checked.ToString());
            objRegistry.SetValue("printtofilefirst", chkPrintToFileFirst.Checked.ToString());
            objRegistry.SetValue("pdfapppath", txtPDFAppPath.Text);
            objRegistry.SetValue("pdfparameters", cboPDFParameters.SelectedValue.ToString());
            objRegistry.SetValue("lifemodule", chkLifeModule.Checked.ToString());
            objRegistry.SetValue("omaeenabled", chkOmaeEnabled.Checked.ToString());
            objRegistry.SetValue("prefernightlybuilds", chkPreferNightlyBuilds.Checked.ToString());
            objRegistry.SetValue("missionsonly", chkMissions.Checked.ToString());
            objRegistry.SetValue("dronemods", chkDronemods.Checked.ToString());
            objRegistry.SetValue("dronemodsPilot", chkDronemodsMaximumPilot.Checked.ToString());
            objRegistry.SetValue("characterrosterpath", txtCharacterRosterPath.Text);

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
            _characterOptions.RecalculateBookXPath();
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
            nudKarmaNewAIProgram.Value = 5;
            nudKarmaNewAIAdvancedProgram.Value = 8;
            nudKarmaComplexFormOption.Value = 2;
            nudKarmaComplexFormSkillsoft.Value = 1;
            nudKarmaSpirit.Value = 1;
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
            nudKarmaAlchemicalFocus.Value        = 3;
            nudKarmaBanishingFocus.Value         = 2;
            nudKarmaBindingFocus.Value           = 2;
            nudKarmaCenteringFocus.Value         = 3;
            nudKarmaCounterspellingFocus.Value   = 2;
            nudKarmaDisenchantingFocus.Value     = 3;
            nudKarmaFlexibleSignatureFocus.Value = 3;
            nudKarmaMaskingFocus.Value           = 3;
            nudKarmaPowerFocus.Value             = 6;
            nudKarmaQiFocus.Value                = 2;
            nudKarmaRitualSpellcastingFocus.Value     = 2;
            nudKarmaSpellcastingFocus.Value      = 2;
            nudKarmaSpellShapingFocus.Value      = 3;
            nudKarmaSummoningFocus.Value         = 2;
            nudKarmaSustainingFocus.Value        = 2;
            nudKarmaWeaponFocus.Value            = 3;
        }

        private void PopulateBuildMethodList()
        {
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

            if (GlobalOptions.Instance.LifeModuleEnabled)
            {
                ListItem objLifeModule = new ListItem();
                objLifeModule.Value = "LifeModule";
                objLifeModule.Name = LanguageManager.Instance.GetString("String_LifeModule");
                lstBuildMethod.Add(objLifeModule);
            }

            lstBuildMethod.Add(objPriority);
            lstBuildMethod.Add(objKarma);
            lstBuildMethod.Add(objSumtoTen);
            cboBuildMethod.BeginUpdate();
            cboBuildMethod.ValueMember = "Value";
            cboBuildMethod.DisplayMember = "Name";
            cboBuildMethod.DataSource = lstBuildMethod;
            cboBuildMethod.EndUpdate();
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

            cboEssenceDecimals.BeginUpdate();
            cboEssenceDecimals.ValueMember = "Value";
            cboEssenceDecimals.DisplayMember = "Name";
            cboEssenceDecimals.DataSource = lstDecimals;
            cboEssenceDecimals.EndUpdate();
        }

        private void PopulateLimbCountList()
        {
            List<ListItem> lstLimbCount = new List<ListItem>();

            XmlDocument objXmlDocument = XmlManager.Instance.Load("options.xml");

            XmlNodeList objXmlNodeList = objXmlDocument.SelectNodes("/chummer/options/limbcounts/limb");

            foreach (XmlNode objXmlNode in objXmlNodeList)
            {
                ListItem objLimbCount = new ListItem();
                string strExclude = string.Empty;
                if (objXmlNode["exclude"] != null)
                {
                    strExclude = objXmlNode["exclude"].InnerText;
                }
                objLimbCount.Value = string.Format("{0}/{1}", objXmlNode["limbcount"].InnerText,
                    objXmlNode["exclude"].InnerText);
                objLimbCount.Name = LanguageManager.Instance.GetString(objXmlNode["name"].InnerText);
                lstLimbCount.Add(objLimbCount);
            }

            cboLimbCount.BeginUpdate();
            cboLimbCount.ValueMember = "Value";
            cboLimbCount.DisplayMember = "Name";
            cboLimbCount.DataSource = lstLimbCount;
            cboLimbCount.EndUpdate();
        }

        private void PopulatePDFParameters()
        {
            List<ListItem> lstPdfParameters = new List<ListItem>();

            XmlDocument objXmlDocument = XmlManager.Instance.Load("options.xml");

            XmlNodeList objXmlNodeList = objXmlDocument.SelectNodes("/chummer/options/pdfarguments/pdfargument");

            int intIndex = 0;
            foreach (XmlNode objXmlNode in objXmlNodeList)
            {
                ListItem objPDFArgument = new ListItem();
                objPDFArgument.Name = objXmlNode["name"].InnerText;
                objPDFArgument.Value = objXmlNode["value"].InnerText;
                lstPdfParameters.Add(objPDFArgument);
                if (!String.IsNullOrWhiteSpace(GlobalOptions.Instance.PDFParameters) && GlobalOptions.Instance.PDFParameters == objPDFArgument.Value)
                {
                    intIndex = lstPdfParameters.IndexOf(objPDFArgument);
                }
            }

            cboPDFParameters.BeginUpdate();
            cboPDFParameters.ValueMember = "Value";
            cboPDFParameters.DisplayMember = "Name";
            cboPDFParameters.DataSource = lstPdfParameters;
            cboPDFParameters.SelectedIndex = intIndex;
            cboPDFParameters.EndUpdate();
        }

        private void SetToolTips()
        {
            const int width = 50;
            tipTooltip.SetToolTip(chkUnarmedSkillImprovements, CommonFunctions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsUnarmedSkillImprovements"), width));
            tipTooltip.SetToolTip(chkIgnoreArt, CommonFunctions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsIgnoreArt"), width));
            tipTooltip.SetToolTip(chkCyberlegMovement, CommonFunctions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsCyberlegMovement"), width));
            tipTooltip.SetToolTip(chkDontDoubleQualityPurchases, CommonFunctions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsDontDoubleQualityPurchases"), width));
            tipTooltip.SetToolTip(chkDontDoubleQualityRefunds, CommonFunctions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsDontDoubleQualityRefunds"), width));
            tipTooltip.SetToolTip(chkStrictSkillGroups, CommonFunctions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionStrictSkillGroups"), width));
            tipTooltip.SetToolTip(chkAllowInitiation, CommonFunctions.WordWrap(LanguageManager.Instance.GetString("Tip_OptionsAllowInitiation"), width));
            tipTooltip.SetToolTip(chkUseCalculatedPublicAwareness, CommonFunctions.WordWrap(LanguageManager.Instance.GetString("Tip_PublicAwareness"), width));
        }

        private void PopulateSettingsList()
        {
            List<ListItem> lstSettings = new List<ListItem>();
            string settingsDirectoryPath = Path.Combine(Application.StartupPath, "settings");
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

            cboSetting.BeginUpdate();
            cboSetting.ValueMember = "Value";
            cboSetting.DisplayMember = "Name";
            cboSetting.DataSource = lstSettings;
            cboSetting.EndUpdate();
        }

        private void PopulateLanguageList()
        {
            List<ListItem> lstLanguages = new List<ListItem>();
            string languageDirectoryPath = Path.Combine(Application.StartupPath, "lang");
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

            cboLanguage.BeginUpdate();
            cboLanguage.ValueMember = "Value";
            cboLanguage.DisplayMember = "Name";
            cboLanguage.DataSource = lstLanguages;
            cboLanguage.EndUpdate();
        }

        private void PopulateGlobalOptions()
        {
            chkAutomaticUpdate.Checked = GlobalOptions.Instance.AutomaticUpdate;
            chkLiveCustomData.Checked = GlobalOptions.Instance.LiveCustomData;
            chkUseLogging.Checked = GlobalOptions.Instance.UseLogging;
            chkLifeModule.Checked = GlobalOptions.Instance.LifeModuleEnabled;
            chkOmaeEnabled.Checked = GlobalOptions.Instance.OmaeEnabled;
            chkPreferNightlyBuilds.Checked = GlobalOptions.Instance.PreferNightlyBuilds;
            chkStartupFullscreen.Checked = GlobalOptions.Instance.StartupFullscreen;
            chkSingleDiceRoller.Checked = GlobalOptions.Instance.SingleDiceRoller;
            chkDatesIncludeTime.Checked = GlobalOptions.Instance.DatesIncludeTime;
            chkMissions.Checked = GlobalOptions.Instance.MissionsOnly;
            chkDronemods.Checked = GlobalOptions.Instance.Dronemods;
            chkDronemodsMaximumPilot.Checked = GlobalOptions.Instance.DronemodsMaximumPilot;
            chkPrintToFileFirst.Checked = GlobalOptions.Instance.PrintToFileFirst;
            txtPDFAppPath.Text = GlobalOptions.Instance.PDFAppPath;
            txtCharacterRosterPath.Text = GlobalOptions.Instance.CharacterRosterPath;
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
            
            XmlDocument manifest = XmlManager.Instance.Load("sheets.xml");
            XmlNodeList sheets = manifest.SelectNodes($"/chummer/sheets[@lang='en-us']/sheet");

            foreach (XmlNode sheet in sheets)
            {
                ListItem objItem = new ListItem();
                objItem.Value = sheet["filename"].InnerText;
                objItem.Name = sheet["name"].InnerText;

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
                XmlDocument manifest = XmlManager.Instance.Load("sheets.xml");
                XmlNodeList sheets = manifest.SelectNodes($"/chummer/sheets[@lang='{GlobalOptions.Instance.Language}']/sheet");
                string strLanguage = objLanguageDocument.SelectSingleNode("/chummer/name").InnerText;

                foreach (XmlNode sheet in sheets)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = Path.Combine(GlobalOptions.Instance.Language, sheet["filename"].InnerText);
                    objItem.Name = strLanguage + ": " + sheet["name"].InnerText;

                    items.Add(objItem);
                }
            }

            return items;
        }

        private List<ListItem> GetXslFilesFromOmaeDirectory()
        {
            var items = new List<ListItem>();

            // Populate the XSLT list with all of the XSL files found in the sheets\omae directory.
            string omaeDirectoryPath = Path.Combine(Application.StartupPath, "sheets", "omae");
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
            if (GlobalOptions.Instance.OmaeEnabled)
            {
                lstFiles.AddRange(GetXslFilesFromOmaeDirectory());
            }

            cboXSLT.BeginUpdate();
            cboXSLT.ValueMember = "Value";
            cboXSLT.DisplayMember = "Name";
            cboXSLT.DataSource = lstFiles;
            cboXSLT.EndUpdate();
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

        private void cmdUploadPastebin_Click(object sender, EventArgs e)
        {
            #if DEBUG
            string strFilePath = "Insert local file here";
            System.Collections.Specialized.NameValueCollection Data    = new System.Collections.Specialized.NameValueCollection();
            XmlDocument objDoc = new XmlDocument();
            String line = string.Empty;
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                line = sr.ReadToEnd();
            }
            Data["api_paste_name"] = "Chummer";
            Data["api_paste_expire_date"] = "N";
            Data["api_paste_format"] = "xml";
            Data["api_paste_code"] = line;
            Data["api_dev_key"] = "7845fd372a1050899f522f2d6bab9666";
            Data["api_option"] = "paste";

            WebClient wb = new WebClient();
                byte[] bytes = wb.UploadValues("http://pastebin.com/api/api_post.php", Data);

                string response;
                using (MemoryStream ms = new MemoryStream(bytes))
                using (StreamReader reader = new StreamReader(ms))
                    response = reader.ReadToEnd();
            Clipboard.SetText(response);
            #endif
        }
        #endregion

        private void OptionsChanged(object sender, EventArgs e)
        {
            if (!blnLoading)
            {
                blnDirty = true;
            }
        }

        private void chkOmaeEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (chkOmaeEnabled.Checked && !blnLoading)
            {
                DialogResult result = MessageBox.Show(LanguageManager.Instance.GetString("Tip_Omae_Warning"), "Warning!", MessageBoxButtons.OKCancel);

                if (result != DialogResult.OK) chkOmaeEnabled.Checked = false;
            }
        }

        private void cmdEnableSourcebooks_Click(object sender, EventArgs e)
        {
            foreach (TreeNode objNode in treSourcebook.Nodes)
            {
                if (objNode.Tag.ToString() != "SR5")
                {
                    objNode.Checked = blnSourcebookToggle;
                }
            }
            blnSourcebookToggle = !blnSourcebookToggle;
        }

        private void cmdCharacterRoster_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (var selectFolderDialog = new FolderBrowserDialog())
            {
                if (selectFolderDialog.ShowDialog(this) == DialogResult.OK)
                    txtCharacterRosterPath.Text = selectFolderDialog.SelectedPath;
            }
        }
    }
}