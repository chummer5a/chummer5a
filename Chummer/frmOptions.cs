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
using System.Text;

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
            LanguageManager.Load(GlobalOptions.Language, this);
        }

        private void frmOptions_Load(object sender, EventArgs e)
        {
            PopulateBuildMethodList();
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
            if (string.IsNullOrWhiteSpace(txtSettingName.Text))
            {
                MessageBox.Show("You must give your Settings a name.", "Chummer Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSettingName.Focus();
                return;
            }

            if (blnDirty)
            {
                string text = LanguageManager.GetString("Message_Options_SaveForms");
                string caption = LanguageManager.GetString("MessageTitle_Options_CloseForms");

                if (MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }

            DialogResult = DialogResult.OK;

            BuildBooksList();
            BuildCustomDataDirectoryNamesList();
            SaveRegistrySettings();

            _characterOptions.AllowCyberwareESSDiscounts = chkAllowCyberwareESSDiscounts.Checked;
            _characterOptions.AllowInitiationInCreateMode = chkAllowInitiation.Checked;
            _characterOptions.AllowSkillDiceRolling = chkAllowSkillDiceRolling.Checked;
            _characterOptions.DontUseCyberlimbCalculation = chkDontUseCyberlimbCalculation.Checked;
            _characterOptions.AllowSkillRegrouping = chkAllowSkillRegrouping.Checked;
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
            _characterOptions.EssenceDecimals = decimal.ToInt32(nudEssenceDecimals.Value);
            _characterOptions.DontRoundEssenceInternally = chkDontRoundEssenceInternally.Checked;
            _characterOptions.ESSLossReducesMaximumOnly = chkESSLossReducesMaximumOnly.Checked;
            _characterOptions.ExceedNegativeQualities = chkExceedNegativeQualities.Checked;
            _characterOptions.ExceedNegativeQualitiesLimit = chkExceedNegativeQualitiesLimit.Checked;
            _characterOptions.ExceedPositiveQualities = chkExceedPositiveQualities.Checked;
            _characterOptions.ExceedPositiveQualitiesCostDoubled = chkExceedPositiveQualitiesCostDoubled.Checked;
            _characterOptions.ExtendAnyDetectionSpell = chkExtendAnyDetectionSpell.Checked;
            _characterOptions.FreeContactsMultiplier = decimal.ToInt32(nudContactMultiplier.Value);
            _characterOptions.FreeContactsMultiplierEnabled = chkContactMultiplier.Checked;
                if (chkContactMultiplier.Checked)
                    nudContactMultiplier.Enabled = true;
            _characterOptions.DroneArmorMultiplier = decimal.ToInt32(nudDroneArmorMultiplier.Value);
            _characterOptions.DroneArmorMultiplierEnabled = chkDroneArmorMultiplier.Checked;
            nudDroneArmorMultiplier.Enabled = chkDroneArmorMultiplier.Checked;
            _characterOptions.FreeKnowledgeMultiplierEnabled = chkKnowledgeMultiplier.Checked;
                if (chkKnowledgeMultiplier.Checked)
                    chkKnowledgeMultiplier.Enabled = true;
            _characterOptions.FreeKnowledgeMultiplier = decimal.ToInt32(nudKnowledgeMultiplier.Value);
            _characterOptions.HideItemsOverAvailLimit = chkHideItemsOverAvail.Checked;
            _characterOptions.IgnoreArt = chkIgnoreArt.Checked;
            _characterOptions.UnarmedImprovementsApplyToWeapons = chkUnarmedSkillImprovements.Checked;
            _characterOptions.LicenseRestricted = chkLicenseEachRestrictedItem.Checked;
            _characterOptions.MaximumArmorModifications = chkMaximumArmorModifications.Checked;
            _characterOptions.MetatypeCostsKarma = chkMetatypeCostsKarma.Checked;
            _characterOptions.ReverseAttributePriorityOrder = chkReverseAttributePriorityOrder.Checked;
            _characterOptions.MetatypeCostsKarmaMultiplier = decimal.ToInt32(nudMetatypeCostsKarmaMultiplier.Value);
            _characterOptions.MoreLethalGameplay = chkMoreLethalGameplay.Checked;
            _characterOptions.NoSingleArmorEncumbrance = chkNoSingleArmorEncumbrance.Checked;
            _characterOptions.NuyenPerBP = decimal.ToInt32(nudKarmaNuyenPer.Value);
            _characterOptions.PrintExpenses = chkPrintExpenses.Checked;
            _characterOptions.PrintNotes = chkPrintNotes.Checked;
            _characterOptions.PrintSkillsWithZeroRating = chkPrintSkillsWithZeroRating.Checked;
            _characterOptions.RestrictRecoil = chkRestrictRecoil.Checked;
            _characterOptions.SpecialKarmaCostBasedOnShownValue = chkSpecialKarmaCost.Checked;
            _characterOptions.UseCalculatedPublicAwareness = chkUseCalculatedPublicAwareness.Checked;
            _characterOptions.StrictSkillGroupsInCreateMode = chkStrictSkillGroups.Checked;
            _characterOptions.AllowPointBuySpecializationsOnKarmaSkills = chkAllowPointBuySpecializationsOnKarmaSkills.Checked;
            _characterOptions.AlternateMetatypeAttributeKarma = chkAlternateMetatypeAttributeKarma.Checked;
            _characterOptions.MysaddPPCareer = chkMysAdPp.Checked;
            _characterOptions.MysAdeptSecondMAGAttribute = chkMysAdeptSecondMAGAttribute.Checked;
            _characterOptions.FreeMartialArtSpecialization = chkFreeMartialArtSpecialization.Checked;
            _characterOptions.PrioritySpellsAsAdeptPowers = chkPrioritySpellsAsAdeptPowers.Checked;
            _characterOptions.LimbCount = Convert.ToInt32(cboLimbCount.SelectedValue.ToString().Split('/')[0]);
            _characterOptions.ExcludeLimbSlot = cboLimbCount.SelectedValue.ToString().Split('/')[1];
            _characterOptions.AllowHoverIncrement = chkAllowHoverIncrement.Checked;
            _characterOptions.SearchInCategoryOnly = chkSearchInCategoryOnly.Checked;

            StringBuilder objNuyenFormat = new StringBuilder("#,0");
            int intNuyenDecimalPlacesMaximum = decimal.ToInt32(nudNuyenDecimalsMaximum.Value);
            int intNuyenDecimalPlacesAlways = decimal.ToInt32(nudNuyenDecimalsAlways.Value);
            if (intNuyenDecimalPlacesMaximum > 0)
            {
                objNuyenFormat.Append(".");
                for (int i = 0; i < intNuyenDecimalPlacesMaximum; ++i)
                {
                    if (i <= intNuyenDecimalPlacesAlways)
                        objNuyenFormat.Append("0");
                    else
                        objNuyenFormat.Append("#");
                }
            }
            _characterOptions.NuyenFormat = objNuyenFormat.ToString();

            // Karma options.
            _characterOptions.KarmaAttribute = decimal.ToInt32(nudKarmaAttribute.Value);
            _characterOptions.KarmaQuality = decimal.ToInt32(nudKarmaQuality.Value);
            _characterOptions.KarmaSpecialization = decimal.ToInt32(nudKarmaSpecialization.Value);
            _characterOptions.KarmaKnowledgeSpecialization = decimal.ToInt32(nudKarmaKnowledgeSpecialization.Value);
            _characterOptions.KarmaNewKnowledgeSkill = decimal.ToInt32(nudKarmaNewKnowledgeSkill.Value);
            _characterOptions.KarmaNewActiveSkill = decimal.ToInt32(nudKarmaNewActiveSkill.Value);
            _characterOptions.KarmaNewSkillGroup = decimal.ToInt32(nudKarmaNewSkillGroup.Value);
            _characterOptions.KarmaImproveKnowledgeSkill = decimal.ToInt32(nudKarmaImproveKnowledgeSkill.Value);
            _characterOptions.KarmaImproveActiveSkill = decimal.ToInt32(nudKarmaImproveActiveSkill.Value);
            _characterOptions.KarmaImproveSkillGroup = decimal.ToInt32(nudKarmaImproveSkillGroup.Value);
            _characterOptions.KarmaSpell = decimal.ToInt32(nudKarmaSpell.Value);
            _characterOptions.KarmaNewComplexForm = decimal.ToInt32(nudKarmaNewComplexForm.Value);
            _characterOptions.KarmaImproveComplexForm = decimal.ToInt32(nudKarmaImproveComplexForm.Value);
            _characterOptions.KarmaNewAIProgram = decimal.ToInt32(nudKarmaNewAIProgram.Value);
            _characterOptions.KarmaNewAIAdvancedProgram = decimal.ToInt32(nudKarmaNewAIAdvancedProgram.Value);
            _characterOptions.KarmaMetamagic = decimal.ToInt32(nudKarmaMetamagic.Value);
            _characterOptions.KarmaNuyenPer = decimal.ToInt32(nudKarmaNuyenPer.Value);
            _characterOptions.KarmaContact = decimal.ToInt32(nudKarmaContact.Value);
            _characterOptions.KarmaEnemy = decimal.ToInt32(nudKarmaEnemy.Value);
            _characterOptions.KarmaCarryover = decimal.ToInt32(nudKarmaCarryover.Value);
            _characterOptions.KarmaSpirit = decimal.ToInt32(nudKarmaSpirit.Value);
            _characterOptions.KarmaManeuver = decimal.ToInt32(nudKarmaManeuver.Value);
            _characterOptions.KarmaInitiation = decimal.ToInt32(nudKarmaInitiation.Value);
            _characterOptions.KarmaInititationFlat = decimal.ToInt32(nudKarmaInitiationFlat.Value);
            _characterOptions.KarmaComplexFormOption = decimal.ToInt32(nudKarmaComplexFormOption.Value);
            _characterOptions.KarmaComplexFormSkillsoft = decimal.ToInt32(nudKarmaComplexFormSkillsoft.Value);
            _characterOptions.KarmaJoinGroup = decimal.ToInt32(nudKarmaJoinGroup.Value);
            _characterOptions.KarmaLeaveGroup = decimal.ToInt32(nudKarmaLeaveGroup.Value);

            // Focus costs
            _characterOptions.KarmaAlchemicalFocus = decimal.ToInt32(nudKarmaAlchemicalFocus.Value);
            _characterOptions.KarmaBanishingFocus = decimal.ToInt32(nudKarmaBanishingFocus.Value);
            _characterOptions.KarmaBindingFocus = decimal.ToInt32(nudKarmaBindingFocus.Value);
            _characterOptions.KarmaCenteringFocus = decimal.ToInt32(nudKarmaCenteringFocus.Value);
            _characterOptions.KarmaCounterspellingFocus = decimal.ToInt32(nudKarmaCounterspellingFocus.Value);
            _characterOptions.KarmaDisenchantingFocus = decimal.ToInt32(nudKarmaDisenchantingFocus.Value);
            _characterOptions.KarmaFlexibleSignatureFocus = decimal.ToInt32(nudKarmaFlexibleSignatureFocus.Value);
            _characterOptions.KarmaMaskingFocus = decimal.ToInt32(nudKarmaMaskingFocus.Value);
            _characterOptions.KarmaPowerFocus = decimal.ToInt32(nudKarmaPowerFocus.Value);
            _characterOptions.KarmaQiFocus = decimal.ToInt32(nudKarmaQiFocus.Value);
            _characterOptions.KarmaRitualSpellcastingFocus = decimal.ToInt32(nudKarmaRitualSpellcastingFocus.Value);
            _characterOptions.KarmaSpellcastingFocus = decimal.ToInt32(nudKarmaSpellcastingFocus.Value);
            _characterOptions.KarmaSpellShapingFocus = decimal.ToInt32(nudKarmaSpellShapingFocus.Value);
            _characterOptions.KarmaSummoningFocus = decimal.ToInt32(nudKarmaSummoningFocus.Value);
            _characterOptions.KarmaSustainingFocus = decimal.ToInt32(nudKarmaSustainingFocus.Value);
            _characterOptions.KarmaWeaponFocus = decimal.ToInt32(nudKarmaWeaponFocus.Value);

            // Build method options.
            _characterOptions.BuildMethod = cboBuildMethod.SelectedValue.ToString();
            _characterOptions.BuildPoints = decimal.ToInt32(nudBP.Value);
            _characterOptions.Availability = decimal.ToInt32(nudMaxAvail.Value);

            _characterOptions.Name = txtSettingName.Text;
            _characterOptions.Save();

            if (blnDirty)
                Utils.RestartApplication("Message_Options_CloseForms");
        }

        private void cboBuildMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboBuildMethod.SelectedValue != null)
            {
            if (cboBuildMethod.SelectedValue.ToString() == LanguageManager.GetString("String_Karma"))
                nudBP.Value = 800;
            else if (cboBuildMethod.SelectedValue.ToString() == LanguageManager.GetString("String_LifeModule"))
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
            bool isEnabled = cboLanguage.SelectedValue.ToString() != GlobalOptions.DefaultLanguage;
            cmdVerify.Enabled = isEnabled;
            cmdVerifyData.Enabled = isEnabled;

            if (!blnLoading)
            {
                string strOldSelected = cboXSLT.SelectedValue?.ToString() ?? string.Empty;
                // Strip away the language prefix
                if (strOldSelected.Contains('\\'))
                    strOldSelected = strOldSelected.Substring(strOldSelected.LastIndexOf('\\') + 1, strOldSelected.Length - 1 - strOldSelected.LastIndexOf('\\'));
                PopulateXsltList();
                string strNewLanguage = cboLanguage.SelectedValue.ToString();
                if (strNewLanguage == GlobalOptions.DefaultLanguage)
                    cboXSLT.SelectedValue = strOldSelected;
                else
                    cboXSLT.SelectedValue = Path.Combine(strNewLanguage, strOldSelected);
                // If the desired sheet was not found, fall back to the Shadowrun 5 sheet.
                if (cboXSLT.SelectedIndex == -1)
                {
                    if (strNewLanguage == GlobalOptions.DefaultLanguage)
                        cboXSLT.SelectedValue = GlobalOptions.DefaultCharacterSheetDefaultValue;
                    else
                        cboXSLT.SelectedValue = Path.Combine(strNewLanguage, GlobalOptions.DefaultCharacterSheetDefaultValue);
                    if (cboXSLT.SelectedIndex == -1)
                    {
                        cboXSLT.SelectedIndex = 0;
                    }
                }
            }

            OptionsChanged(sender,e);
        }

        private void cmdVerify_Click(object sender, EventArgs e)
        {
            LanguageManager.VerifyStrings(cboLanguage.SelectedValue.ToString());
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

            XmlManager.Verify(cboLanguage.SelectedValue.ToString(), lstBooks);

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

        private void chkExceedPositiveQualities_CheckedChanged(object sender, EventArgs e)
        {
            chkExceedPositiveQualitiesCostDoubled.Enabled = chkExceedNegativeQualities.Checked;
            if (!chkExceedPositiveQualitiesCostDoubled.Enabled)
                chkExceedPositiveQualitiesCostDoubled.Checked = false;
            OptionsChanged(sender, e);
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
            string text = LanguageManager.GetString("Message_Options_RestoreDefaults");
            string caption = LanguageManager.GetString("MessageTitle_Options_RestoreDefaults");

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
            foreach (SourcebookInfo objSource in GlobalOptions.SourcebookInfo.Where(objSource => objSource.Code == treSourcebook.SelectedNode.Tag.ToString()))
            {
                txtPDFLocation.Text = objSource.Path;
                nudPDFOffset.Value = objSource.Offset;
            }
        }

        private void nudPDFOffset_ValueChanged(object sender, EventArgs e)
        {
            if (_skipRefresh)
                return;

            int offset = decimal.ToInt32(nudPDFOffset.Value);
            string tag = treSourcebook.SelectedNode.Tag.ToString();
            SourcebookInfo foundSource = GlobalOptions.SourcebookInfo.FirstOrDefault(x => x.Code == tag);

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
                GlobalOptions.SourcebookInfo.Add(newSource);
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

            CommonFunctions.OpenPDF(treSourcebook.SelectedNode.Tag + " 5");
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
            nudEssenceDecimals.Left = lblEssenceDecimals.Left + lblEssenceDecimals.Width + 6;

            intWidth = Math.Max(lblNuyenDecimalsAlwaysLabel.Width, lblNuyenDecimalsMaximumLabel.Width);
            nudNuyenDecimalsAlways.Left = lblNuyenDecimalsAlwaysLabel.Left + intWidth + 6;
            nudNuyenDecimalsMaximum.Left = lblNuyenDecimalsMaximumLabel.Left + intWidth + 6;

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
            intWidth = Math.Max(lblKarmaSpecialization.Width, lblKarmaKnowledgeSpecialization.Width);
            intWidth = Math.Max(intWidth, lblKarmaNewKnowledgeSkill.Width);
            intWidth = Math.Max(intWidth, lblKarmaNewActiveSkill.Width);
            intWidth = Math.Max(intWidth, lblKarmaNewSkillGroup.Width);
            intWidth = Math.Max(intWidth, lblKarmaImproveKnowledgeSkill.Width);
            intWidth = Math.Max(intWidth, lblKarmaImproveActiveSkill.Width);
            intWidth = Math.Max(intWidth, lblKarmaImproveSkillGroup.Width);
            intWidth = Math.Max(intWidth, lblKarmaAttribute.Width);
            intWidth = Math.Max(intWidth, lblKarmaQuality.Width);
            intWidth = Math.Max(intWidth, lblKarmaSpell.Width);
            intWidth = Math.Max(intWidth, lblKarmaNewComplexForm.Width);
            intWidth = Math.Max(intWidth, lblKarmaImproveComplexForm.Width);
            intWidth = Math.Max(intWidth, lblKarmaComplexFormOption.Width);
            intWidth = Math.Max(intWidth, lblKarmaComplexFormSkillsoft.Width);
            intWidth = Math.Max(intWidth, lblKarmaSpirit.Width);
            intWidth = Math.Max(intWidth, lblKarmaManeuver.Width);
            intWidth = Math.Max(intWidth, lblKarmaNuyenPer.Width);
            intWidth = Math.Max(intWidth, lblKarmaContact.Width);
            intWidth = Math.Max(intWidth, lblKarmaEnemy.Width);
            intWidth = Math.Max(intWidth, lblKarmaCarryover.Width);
            intWidth = Math.Max(intWidth, lblKarmaInitiation.Width);

            nudKarmaSpecialization.Left = lblKarmaSpecialization.Left + intWidth + 6;
            nudKarmaKnowledgeSpecialization.Left = lblKarmaKnowledgeSpecialization.Left + intWidth + 6;
            nudKarmaNewKnowledgeSkill.Left = lblKarmaNewKnowledgeSkill.Left + intWidth + 6;
            nudKarmaNewActiveSkill.Left = lblKarmaNewActiveSkill.Left + intWidth + 6;
            nudKarmaNewSkillGroup.Left = lblKarmaNewSkillGroup.Left + intWidth + 6;
            nudKarmaImproveKnowledgeSkill.Left = lblKarmaImproveKnowledgeSkill.Left + intWidth + 6;
            lblKarmaImproveKnowledgeSkillExtra.Left = nudKarmaImproveKnowledgeSkill.Left + nudKarmaImproveKnowledgeSkill.Width + 6;
            nudKarmaImproveActiveSkill.Left = lblKarmaImproveActiveSkill.Left + intWidth + 6;
            lblKarmaImproveActiveSkillExtra.Left = nudKarmaImproveActiveSkill.Left + nudKarmaImproveActiveSkill.Width + 6;
            nudKarmaImproveSkillGroup.Left = lblKarmaImproveSkillGroup.Left + intWidth + 6;
            lblKarmaImproveSkillGroupExtra.Left = nudKarmaImproveSkillGroup.Left + nudKarmaImproveSkillGroup.Width + 6;
            nudKarmaAttribute.Left = lblKarmaAttribute.Left + intWidth + 6;
            lblKarmaAttributeExtra.Left = nudKarmaAttribute.Left + nudKarmaAttribute.Width + 6;
            nudKarmaQuality.Left = lblKarmaQuality.Left + intWidth + 6;
            lblKarmaQualityExtra.Left = nudKarmaQuality.Left + nudKarmaQuality.Width + 6;
            nudKarmaSpell.Left = lblKarmaSpell.Left + intWidth + 6;
            nudKarmaNewComplexForm.Left = lblKarmaNewComplexForm.Left + intWidth + 6;
            nudKarmaImproveComplexForm.Left = lblKarmaImproveComplexForm.Left + intWidth + 6;
            lblKarmaImproveComplexFormExtra.Left = nudKarmaImproveComplexForm.Left + nudKarmaImproveComplexForm.Width + 6;
            nudKarmaComplexFormOption.Left = lblKarmaComplexFormOption.Left + intWidth + 6;
            lblKarmaComplexFormOptionExtra.Left = nudKarmaComplexFormOption.Left + nudKarmaComplexFormOption.Width + 6;
            nudKarmaComplexFormSkillsoft.Left = lblKarmaComplexFormSkillsoft.Left + intWidth + 6;
            lblKarmaComplexFormSkillsoftExtra.Left = nudKarmaComplexFormSkillsoft.Left + nudKarmaComplexFormSkillsoft.Width + 6;
            nudKarmaSpirit.Left = lblKarmaSpirit.Left + intWidth + 6;
            lblKarmaSpiritExtra.Left = nudKarmaSpirit.Left + nudKarmaSpirit.Width + 6;
            nudKarmaManeuver.Left = lblKarmaManeuver.Left + intWidth + 6;
            nudKarmaNuyenPer.Left = lblKarmaNuyenPer.Left + intWidth + 6;
            lblKarmaNuyenPerExtra.Left = nudKarmaNuyenPer.Left + nudKarmaNuyenPer.Width + 6;
            nudKarmaContact.Left = lblKarmaContact.Left + intWidth + 6;
            lblKarmaContactExtra.Left = nudKarmaContact.Left + nudKarmaContact.Width + 6;
            nudKarmaEnemy.Left = lblKarmaEnemy.Left + intWidth + 6;
            lblKarmaEnemyExtra.Left = nudKarmaEnemy.Left + nudKarmaEnemy.Width + 6;
            nudKarmaCarryover.Left = lblKarmaCarryover.Left + intWidth + 6;
            lblKarmaCarryoverExtra.Left = nudKarmaCarryover.Left + nudKarmaCarryover.Width + 6;
            nudKarmaInitiation.Left = lblKarmaInitiation.Left + intWidth + 6;
            lblKarmaInitiationBracket.Left = nudKarmaInitiation.Left - lblKarmaInitiationBracket.Width;
            lblKarmaInitiationExtra.Left = nudKarmaInitiation.Left + nudKarmaInitiation.Width + 6;
            nudKarmaInitiationFlat.Left = lblKarmaInitiationExtra.Left + lblKarmaInitiationExtra.Width + 6;

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
            XmlDocument objXmlDocument = XmlManager.Load("books.xml");

            // Put the Sourcebooks into a List so they can first be sorted.
            XmlNodeList objXmlBookList = objXmlDocument.SelectNodes("/chummer/books/book");
            treSourcebook.Nodes.Clear();

            foreach (XmlNode objXmlBook in objXmlBookList)
            {
                if (objXmlBook["hide"] != null)
                    continue;
                bool blnChecked = _characterOptions.Books.Contains(objXmlBook["code"].InnerText);
                TreeNode objNode = new TreeNode();

                objNode.Text = objXmlBook["translate"]?.InnerText ?? objXmlBook["name"].InnerText;

                objNode.Tag = objXmlBook["code"].InnerText;
                objNode.Checked = blnChecked;
                treSourcebook.Nodes.Add(objNode);
            }

            treSourcebook.Sort();
        }

        private void PopulateCustomDataDirectoryTreeView()
        {
            if (GlobalOptions.CustomDataDirectoryInfo.Count != treCustomDataDirectories.Nodes.Count)
            {
                treCustomDataDirectories.Nodes.Clear();

                foreach (CustomDataDirectoryInfo objCustomDataDirectory in GlobalOptions.CustomDataDirectoryInfo)
                {
                    TreeNode objNode = new TreeNode();

                    objNode.Text = objCustomDataDirectory.Name + " (" + objCustomDataDirectory.Path + ")";
                    objNode.Tag = objCustomDataDirectory.Name;
                    objNode.Checked = objCustomDataDirectory.Enabled;
                    treCustomDataDirectories.Nodes.Add(objNode);
                }
            }
            else
            {
                for(int i = 0; i < treCustomDataDirectories.Nodes.Count; ++i)
                {
                    TreeNode objLoopNode = treCustomDataDirectories.Nodes[i];
                    CustomDataDirectoryInfo objLoopInfo = GlobalOptions.CustomDataDirectoryInfo[i];
                    objLoopNode.Text = objLoopInfo.Name + " (" + objLoopInfo.Path + ")";
                    objLoopNode.Tag = objLoopInfo.Name;
                    objLoopNode.Checked = objLoopInfo.Enabled;
                }
            }
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
            PopulateCustomDataDirectoryTreeView();

            cboBuildMethod.SelectedValue = _characterOptions.BuildMethod;
            nudEssenceDecimals.Value = _characterOptions.EssenceDecimals == 0 ? 2 : _characterOptions.EssenceDecimals;
            chkDontRoundEssenceInternally.Checked = _characterOptions.DontRoundEssenceInternally;
            chkAllowCyberwareESSDiscounts.Checked = _characterOptions.AllowCyberwareESSDiscounts;
            chkAllowInitiation.Checked = _characterOptions.AllowInitiationInCreateMode;
            chkAllowSkillDiceRolling.Checked = _characterOptions.AllowSkillDiceRolling;
            chkDontUseCyberlimbCalculation.Checked = _characterOptions.DontUseCyberlimbCalculation;
            chkAllowSkillRegrouping.Checked = _characterOptions.AllowSkillRegrouping;
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
            chkCreateBackupOnCareer.Checked = _characterOptions.CreateBackupOnCareer;
            chkCyberlegMovement.Checked = _characterOptions.CyberlegMovement;
            chkMysAdPp.Checked = _characterOptions.MysaddPPCareer;
            chkMysAdeptSecondMAGAttribute.Checked = _characterOptions.MysAdeptSecondMAGAttribute;
            chkHideItemsOverAvail.Checked = _characterOptions.HideItemsOverAvailLimit;
            chkFreeMartialArtSpecialization.Checked = _characterOptions.FreeMartialArtSpecialization;
            chkPrioritySpellsAsAdeptPowers.Checked = _characterOptions.PrioritySpellsAsAdeptPowers;
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
            chkExceedPositiveQualitiesCostDoubled.Checked = _characterOptions.ExceedPositiveQualitiesCostDoubled;
            chkExceedPositiveQualitiesCostDoubled.Enabled = chkExceedPositiveQualities.Checked;
            chkExtendAnyDetectionSpell.Checked = _characterOptions.ExtendAnyDetectionSpell;
            chkIgnoreArt.Checked = _characterOptions.IgnoreArt;
            chkKnowledgeMultiplier.Checked = _characterOptions.FreeKnowledgeMultiplierEnabled;
            chkUnarmedSkillImprovements.Checked = _characterOptions.UnarmedImprovementsApplyToWeapons;
            chkLicenseEachRestrictedItem.Checked = _characterOptions.LicenseRestricted;
            chkMaximumArmorModifications.Checked = _characterOptions.MaximumArmorModifications;
            chkMetatypeCostsKarma.Checked = _characterOptions.MetatypeCostsKarma;
            chkMoreLethalGameplay.Checked = _characterOptions.MoreLethalGameplay;
            chkNoSingleArmorEncumbrance.Checked = _characterOptions.NoSingleArmorEncumbrance;
            chkPrintExpenses.Checked = _characterOptions.PrintExpenses;
            chkPrintNotes.Checked = _characterOptions.PrintNotes;
            chkPrintSkillsWithZeroRating.Checked = _characterOptions.PrintSkillsWithZeroRating;
            chkRestrictRecoil.Checked = _characterOptions.RestrictRecoil;
            chkSpecialKarmaCost.Checked = _characterOptions.SpecialKarmaCostBasedOnShownValue;
            chkUseCalculatedPublicAwareness.Checked = _characterOptions.UseCalculatedPublicAwareness;
            chkAllowPointBuySpecializationsOnKarmaSkills.Checked = _characterOptions.AllowPointBuySpecializationsOnKarmaSkills;
            chkStrictSkillGroups.Checked = _characterOptions.StrictSkillGroupsInCreateMode;
            chkAlternateMetatypeAttributeKarma.Checked = _characterOptions.AlternateMetatypeAttributeKarma;
            chkReverseAttributePriorityOrder.Checked = _characterOptions.ReverseAttributePriorityOrder;
            chkAllowHoverIncrement.Checked = _characterOptions.AllowHoverIncrement;
            chkSearchInCategoryOnly.Checked = _characterOptions.SearchInCategoryOnly;
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

            int intNuyenDecimalPlacesMaximum = 0;
            int intNuyenDecimalPlacesAlways = 0;
            string strNuyenFormat = _characterOptions.NuyenFormat;
            int intDecimalIndex = strNuyenFormat.IndexOf('.');
            if (intDecimalIndex != -1)
            {
                strNuyenFormat = strNuyenFormat.Substring(intDecimalIndex);
                intNuyenDecimalPlacesMaximum = strNuyenFormat.Length - 1;
                intNuyenDecimalPlacesAlways = strNuyenFormat.IndexOf('#') - 1;
                if (intNuyenDecimalPlacesAlways < 0)
                    intNuyenDecimalPlacesAlways = intNuyenDecimalPlacesMaximum;
            }
            nudNuyenDecimalsMaximum.Value = intNuyenDecimalPlacesMaximum;
            nudNuyenDecimalsAlways.Value = intNuyenDecimalPlacesAlways;

            SetDefaultValueForLimbCount();
            PopulateKarmaFields();
        }

        private void PopulateKarmaFields()
        {
            nudKarmaAttribute.Value = _characterOptions.KarmaAttribute;
            nudKarmaQuality.Value = _characterOptions.KarmaQuality;
            nudKarmaSpecialization.Value = _characterOptions.KarmaSpecialization;
            nudKarmaKnowledgeSpecialization.Value = _characterOptions.KarmaKnowledgeSpecialization;
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
            nudKarmaInitiationFlat.Value = _characterOptions.KarmaInititationFlat;
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
            GlobalOptions.AutomaticUpdate = chkAutomaticUpdate.Checked;
            GlobalOptions.LiveCustomData = chkLiveCustomData.Checked;
            GlobalOptions.UseLogging = chkUseLogging.Checked;
            GlobalOptions.Language = cboLanguage.SelectedValue.ToString();
            GlobalOptions.StartupFullscreen = chkStartupFullscreen.Checked;
            GlobalOptions.SingleDiceRoller = chkSingleDiceRoller.Checked;
            if (cboXSLT.SelectedValue == null || string.IsNullOrEmpty(cboXSLT.SelectedValue.ToString()))
            {
                cboXSLT.SelectedValue = GlobalOptions.DefaultCharacterSheetDefaultValue;
            }
            GlobalOptions.DefaultCharacterSheet = cboXSLT.SelectedValue.ToString();
            GlobalOptions.DatesIncludeTime = chkDatesIncludeTime.Checked;
            GlobalOptions.PrintToFileFirst = chkPrintToFileFirst.Checked;
            GlobalOptions.PDFAppPath = txtPDFAppPath.Text;
            GlobalOptions.PDFParameters = cboPDFParameters.SelectedValue.ToString();
            GlobalOptions.LifeModuleEnabled = chkLifeModule.Checked;
            GlobalOptions.OmaeEnabled = chkOmaeEnabled.Checked;
            GlobalOptions.PreferNightlyBuilds = chkPreferNightlyBuilds.Checked;
            GlobalOptions.MissionsOnly = chkMissions.Checked;
            GlobalOptions.Dronemods = chkDronemods.Checked;
            GlobalOptions.DronemodsMaximumPilot = chkDronemodsMaximumPilot.Checked;
            GlobalOptions.CharacterRosterPath = txtCharacterRosterPath.Text;
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
            Microsoft.Win32.RegistryKey objSourceRegistry = objRegistry.CreateSubKey("Sourcebook");
            foreach (SourcebookInfo objSource in GlobalOptions.SourcebookInfo)
                objSourceRegistry.SetValue(objSource.Code, objSource.Path + "|" + objSource.Offset);
            objSourceRegistry.Close();

            // Save the Custom Data Directory Info.
            if (objRegistry.OpenSubKey("CustomDataDirectory") != null)
                objRegistry.DeleteSubKeyTree("CustomDataDirectory");
            Microsoft.Win32.RegistryKey objCustomDataDirectoryRegistry = objRegistry.CreateSubKey("CustomDataDirectory");
            for (int i = 0; i < GlobalOptions.CustomDataDirectoryInfo.Count; ++i)
            {
                CustomDataDirectoryInfo objCustomDataDirectory = GlobalOptions.CustomDataDirectoryInfo[i];
                Microsoft.Win32.RegistryKey objLoopKey = objCustomDataDirectoryRegistry.CreateSubKey(objCustomDataDirectory.Name);
                objLoopKey.SetValue("Path", objCustomDataDirectory.Path);
                objLoopKey.SetValue("Enabled", objCustomDataDirectory.Enabled);
                objLoopKey.SetValue("LoadOrder", i);
                objLoopKey.Close();
            }
            objCustomDataDirectoryRegistry.Close();
            objRegistry.Close();
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

        private void BuildCustomDataDirectoryNamesList()
        {
            _characterOptions.CustomDataDirectoryNames.Clear();

            foreach (TreeNode objNode in treCustomDataDirectories.Nodes)
            {
                CustomDataDirectoryInfo objCustomDataDirectory = GlobalOptions.CustomDataDirectoryInfo.FirstOrDefault(x => x.Name == objNode.Tag.ToString());
                if (objCustomDataDirectory != null)
                {
                    if (objNode.Checked)
                    {
                        _characterOptions.CustomDataDirectoryNames.Add(objNode.Tag.ToString());
                        objCustomDataDirectory.Enabled = true;
                    }
                    else
                        objCustomDataDirectory.Enabled = false;
                }
            }
        }

        private void RestoreDefaultKarmaValues()
        {
            nudKarmaSpecialization.Value = 7;
            nudKarmaKnowledgeSpecialization.Value = 7;
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
            nudKarmaInitiationFlat.Value = 10;
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
            objKarma.Name = LanguageManager.GetString("String_Karma");

            ListItem objPriority = new ListItem();
            objPriority.Value = "Priority";
            objPriority.Name = LanguageManager.GetString("String_Priority");

            ListItem objSumtoTen = new ListItem();
            objSumtoTen.Value = "SumtoTen";
            objSumtoTen.Name = LanguageManager.GetString("String_SumtoTen");

            if (GlobalOptions.LifeModuleEnabled)
            {
                ListItem objLifeModule = new ListItem();
                objLifeModule.Value = "LifeModule";
                objLifeModule.Name = LanguageManager.GetString("String_LifeModule");
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

        private void PopulateLimbCountList()
        {
            List<ListItem> lstLimbCount = new List<ListItem>();

            XmlDocument objXmlDocument = XmlManager.Load("options.xml");

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
                objLimbCount.Name = LanguageManager.GetString(objXmlNode["name"].InnerText);
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

            XmlDocument objXmlDocument = XmlManager.Load("options.xml");

            XmlNodeList objXmlNodeList = objXmlDocument.SelectNodes("/chummer/options/pdfarguments/pdfargument");

            int intIndex = 0;
            foreach (XmlNode objXmlNode in objXmlNodeList)
            {
                ListItem objPDFArgument = new ListItem();
                objPDFArgument.Name = objXmlNode["name"].InnerText;
                objPDFArgument.Value = objXmlNode["value"].InnerText;
                lstPdfParameters.Add(objPDFArgument);
                if (!String.IsNullOrWhiteSpace(GlobalOptions.PDFParameters) && GlobalOptions.PDFParameters == objPDFArgument.Value)
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
            tipTooltip.SetToolTip(chkUnarmedSkillImprovements, CommonFunctions.WordWrap(LanguageManager.GetString("Tip_OptionsUnarmedSkillImprovements"), width));
            tipTooltip.SetToolTip(chkIgnoreArt, CommonFunctions.WordWrap(LanguageManager.GetString("Tip_OptionsIgnoreArt"), width));
            tipTooltip.SetToolTip(chkCyberlegMovement, CommonFunctions.WordWrap(LanguageManager.GetString("Tip_OptionsCyberlegMovement"), width));
            tipTooltip.SetToolTip(chkDontDoubleQualityPurchases, CommonFunctions.WordWrap(LanguageManager.GetString("Tip_OptionsDontDoubleQualityPurchases"), width));
            tipTooltip.SetToolTip(chkDontDoubleQualityRefunds, CommonFunctions.WordWrap(LanguageManager.GetString("Tip_OptionsDontDoubleQualityRefunds"), width));
            tipTooltip.SetToolTip(chkStrictSkillGroups, CommonFunctions.WordWrap(LanguageManager.GetString("Tip_OptionStrictSkillGroups"), width));
            tipTooltip.SetToolTip(chkAllowInitiation, CommonFunctions.WordWrap(LanguageManager.GetString("Tip_OptionsAllowInitiation"), width));
            tipTooltip.SetToolTip(chkUseCalculatedPublicAwareness, CommonFunctions.WordWrap(LanguageManager.GetString("Tip_PublicAwareness"), width));
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
            chkAutomaticUpdate.Checked = GlobalOptions.AutomaticUpdate;
            chkLiveCustomData.Checked = GlobalOptions.LiveCustomData;
            chkUseLogging.Checked = GlobalOptions.UseLogging;
            chkLifeModule.Checked = GlobalOptions.LifeModuleEnabled;
            chkOmaeEnabled.Checked = GlobalOptions.OmaeEnabled;
            chkPreferNightlyBuilds.Checked = GlobalOptions.PreferNightlyBuilds;
            chkStartupFullscreen.Checked = GlobalOptions.StartupFullscreen;
            chkSingleDiceRoller.Checked = GlobalOptions.SingleDiceRoller;
            chkDatesIncludeTime.Checked = GlobalOptions.DatesIncludeTime;
            chkMissions.Checked = GlobalOptions.MissionsOnly;
            chkDronemods.Checked = GlobalOptions.Dronemods;
            chkDronemodsMaximumPilot.Checked = GlobalOptions.DronemodsMaximumPilot;
            chkPrintToFileFirst.Checked = GlobalOptions.PrintToFileFirst;
            txtPDFAppPath.Text = GlobalOptions.PDFAppPath;
            txtCharacterRosterPath.Text = GlobalOptions.CharacterRosterPath;
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

        private List<ListItem> GetXslFilesFromLocalDirectory(string strLanguage)
        {
            List<ListItem> lstSheets = new List<ListItem>();

            // Populate the XSL list with all of the manifested XSL files found in the sheets\[language] directory.
            XmlDocument objLanguageDocument = LanguageManager.XmlDoc;
            XmlDocument manifest = XmlManager.Load("sheets.xml");
            XmlNodeList sheets = manifest.SelectNodes($"/chummer/sheets[@lang='{strLanguage}']/sheet[not(hide)]");
            foreach (XmlNode sheet in sheets)
            {
                ListItem objItem = new ListItem();
                objItem.Value = strLanguage != GlobalOptions.DefaultLanguage ? Path.Combine(strLanguage, sheet["filename"].InnerText) : sheet["filename"].InnerText;
                objItem.Name = sheet["name"].InnerText;

                lstSheets.Add(objItem);
            }

            return lstSheets;
        }

        private List<ListItem> GetXslFilesFromOmaeDirectory()
        {
            var items = new List<ListItem>();

            // Populate the XSLT list with all of the XSL files found in the sheets\omae directory.
            string omaeDirectoryPath = Path.Combine(Application.StartupPath, "sheets", "omae");
            string menuMainOmae = LanguageManager.GetString("Menu_Main_Omae");

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

            lstFiles.AddRange(GetXslFilesFromLocalDirectory(cboLanguage.SelectedValue.ToString()));
            if (GlobalOptions.OmaeEnabled)
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
            cboLanguage.SelectedValue = GlobalOptions.Language;

            if (cboLanguage.SelectedIndex == -1)
                cboLanguage.SelectedValue = GlobalOptions.DefaultLanguage;
        }

        private void SetDefaultValueForXsltList()
        {
            if (string.IsNullOrEmpty(GlobalOptions.DefaultCharacterSheet))
                GlobalOptions.DefaultCharacterSheet = GlobalOptions.DefaultCharacterSheetDefaultValue;

            cboXSLT.SelectedValue = GlobalOptions.DefaultCharacterSheet;
        }

        private void UpdateSourcebookInfoPath(string path)
        {
            string tag = treSourcebook.SelectedNode.Tag.ToString();
            SourcebookInfo foundSource = GlobalOptions.SourcebookInfo.FirstOrDefault(x => x.Code == tag);

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
                GlobalOptions.SourcebookInfo.Add(newSource);
            }
        }

        private void cmdUploadPastebin_Click(object sender, EventArgs e)
        {
            #if DEBUG
            string strFilePath = "Insert local file here";
            System.Collections.Specialized.NameValueCollection Data    = new System.Collections.Specialized.NameValueCollection();
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
            byte[] bytes = null;
            try
            {
                bytes = wb.UploadValues("http://pastebin.com/api/api_post.php", Data);
            }
            catch (WebException)
            {
                return;
            }

            string response;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (StreamReader reader = new StreamReader(ms))
                {
                    response = reader.ReadToEnd();
                }
            }
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
                DialogResult result = MessageBox.Show(LanguageManager.GetString("Tip_Omae_Warning"), "Warning!", MessageBoxButtons.OKCancel);

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

        private void cmdAddCustomDirectory_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (var selectFolderDialog = new FolderBrowserDialog())
            {
                selectFolderDialog.SelectedPath = Application.StartupPath;

                if (selectFolderDialog.ShowDialog(this) == DialogResult.OK)
                {
                    frmSelectText frmSelectCustomDirectoryName = new frmSelectText();
                    frmSelectCustomDirectoryName.Description = LanguageManager.GetString("String_CustomItem_SelectText");
                    if (frmSelectCustomDirectoryName.ShowDialog(this) == DialogResult.OK)
                    {
                        CustomDataDirectoryInfo objNewCustomDataDirectory = new CustomDataDirectoryInfo();
                        objNewCustomDataDirectory.Name = frmSelectCustomDirectoryName.SelectedValue;
                        objNewCustomDataDirectory.Path = selectFolderDialog.SelectedPath;

                        if (GlobalOptions.CustomDataDirectoryInfo.Any(x => x.Name == objNewCustomDataDirectory.Name))
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_Duplicate_CustomDataDirectoryName"), LanguageManager.GetString("Message_Duplicate_CustomDataDirectoryName_Title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            GlobalOptions.CustomDataDirectoryInfo.Add(objNewCustomDataDirectory);
                            PopulateCustomDataDirectoryTreeView();
                        }
                    }
                }
            }
        }

        private void cmdRemoveCustomDirectory_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedCustomDataDirectory = treCustomDataDirectories.SelectedNode;
            if (objSelectedCustomDataDirectory != null)
            {
                CustomDataDirectoryInfo objInfoToRemove = GlobalOptions.CustomDataDirectoryInfo.FirstOrDefault(x => x.Name == objSelectedCustomDataDirectory.Tag.ToString());
                if (objInfoToRemove != null)
                {
                    if (objInfoToRemove.Enabled)
                        OptionsChanged(sender, e);
                    GlobalOptions.CustomDataDirectoryInfo.Remove(objInfoToRemove);
                    PopulateCustomDataDirectoryTreeView();
                }
            }
        }

        private void cmdRenameCustomDataDirectory_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedCustomDataDirectory = treCustomDataDirectories.SelectedNode;
            if (objSelectedCustomDataDirectory != null)
            {
                CustomDataDirectoryInfo objInfoToRename = GlobalOptions.CustomDataDirectoryInfo.FirstOrDefault(x => x.Name == objSelectedCustomDataDirectory.Tag.ToString());
                if (objInfoToRename != null)
                {
                    frmSelectText frmSelectCustomDirectoryName = new frmSelectText();
                    frmSelectCustomDirectoryName.Description = LanguageManager.GetString("String_CustomItem_SelectText");
                    if (frmSelectCustomDirectoryName.ShowDialog(this) == DialogResult.OK)
                    {
                        if (GlobalOptions.CustomDataDirectoryInfo.Any(x => x.Name == frmSelectCustomDirectoryName.Name))
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_Duplicate_CustomDataDirectoryName"), LanguageManager.GetString("Message_Duplicate_CustomDataDirectoryName_Title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            objInfoToRename.Name = frmSelectCustomDirectoryName.SelectedValue;
                            PopulateCustomDataDirectoryTreeView();
                        }
                    }
                }
            }
        }

        private void cmdIncreaseCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedCustomDataDirectory = treCustomDataDirectories.SelectedNode;
            if (objSelectedCustomDataDirectory != null)
            {
                CustomDataDirectoryInfo objInfoToRaise = null;
                List<CustomDataDirectoryInfo> lstCustomDataDirectoryInfos = GlobalOptions.CustomDataDirectoryInfo;
                int intIndex = 0;
                for(;intIndex < lstCustomDataDirectoryInfos.Count; ++intIndex)
                {
                    if (lstCustomDataDirectoryInfos.ElementAt(intIndex).Name == objSelectedCustomDataDirectory.Tag.ToString())
                    {
                        objInfoToRaise = lstCustomDataDirectoryInfos.ElementAt(intIndex);
                        break;
                    }
                }
                if (objInfoToRaise != null && intIndex > 0)
                {
                    CustomDataDirectoryInfo objTempInfo = GlobalOptions.CustomDataDirectoryInfo.ElementAt(intIndex - 1);
                    bool blnOptionsChanged = objInfoToRaise.Enabled || objTempInfo.Enabled;
                    GlobalOptions.CustomDataDirectoryInfo[intIndex - 1] = objInfoToRaise;
                    GlobalOptions.CustomDataDirectoryInfo[intIndex] = objTempInfo;

                    PopulateCustomDataDirectoryTreeView();
                    if (blnOptionsChanged)
                        OptionsChanged(sender, e);
                }
            }
        }

        private void cmdDecreaseCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedCustomDataDirectory = treCustomDataDirectories.SelectedNode;
            if (objSelectedCustomDataDirectory != null)
            {
                CustomDataDirectoryInfo objInfoToLower = null;
                List<CustomDataDirectoryInfo> lstCustomDataDirectoryInfos = GlobalOptions.CustomDataDirectoryInfo;
                int intIndex = 0;
                for (; intIndex < lstCustomDataDirectoryInfos.Count; ++intIndex)
                {
                    if (lstCustomDataDirectoryInfos.ElementAt(intIndex).Name == objSelectedCustomDataDirectory.Tag.ToString())
                    {
                        objInfoToLower = lstCustomDataDirectoryInfos.ElementAt(intIndex);
                        break;
                    }
                }
                if (objInfoToLower != null && intIndex < lstCustomDataDirectoryInfos.Count - 1)
                {
                    CustomDataDirectoryInfo objTempInfo = GlobalOptions.CustomDataDirectoryInfo.ElementAt(intIndex + 1);
                    bool blnOptionsChanged = objInfoToLower.Enabled || objTempInfo.Enabled;
                    GlobalOptions.CustomDataDirectoryInfo[intIndex + 1] = objInfoToLower;
                    GlobalOptions.CustomDataDirectoryInfo[intIndex] = objTempInfo;

                    PopulateCustomDataDirectoryTreeView();
                    if (blnOptionsChanged)
                        OptionsChanged(sender, e);
                }
            }
        }

        private void nudNuyenDecimalsMaximum_ValueChanged(object sender, EventArgs e)
        {
            nudNuyenDecimalsAlways.Maximum = nudNuyenDecimalsMaximum.Value;
            OptionsChanged(sender, e);
        }
    }
}
