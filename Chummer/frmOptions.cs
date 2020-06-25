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
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
#if DEBUG
using System.Net;
#endif
using Application = System.Windows.Forms.Application;
using System.Text;
using Microsoft.Win32;
using NLog;

namespace Chummer
{
    public partial class frmOptions : Form
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly Character _objCharacter;
        private readonly CharacterOptions _objCharacterOptions;
        // List of custom data directories on the character, in load order. If the character has a directory name for which we have no info, Item1 will be null
        private readonly List<Tuple<object, bool>> _lstCharacterCustomDataDirectoryInfos = new List<Tuple<object, bool>>();
        private readonly HashSet<CustomDataDirectoryInfo> _setCustomDataDirectoryInfos;
        private bool _blnSkipRefresh;
        private bool _blnDirty;
        private bool _blnLoading = true;
        private bool _blnSourcebookToggle = true;
        private string _strSelectedLanguage = GlobalOptions.Language;
        private CultureInfo _objSelectedCultureInfo = GlobalOptions.CultureInfo;

        #region Form Events
        public frmOptions(Character objCharacter)
        {
            InitializeComponent();
#if !DEBUG
            // tabPage3 only contains cmdUploadPastebin, which is not used if DEBUG is not enabled
            // Remove this line if cmdUploadPastebin_Click has some functionality if DEBUG is not enabled or if tabPage3 gets some other control that can be used if DEBUG is not enabled
            tabOptions.TabPages.Remove(tabGitHubIssues);
#endif
            this.TranslateWinForm(_strSelectedLanguage);
            _objCharacter = objCharacter;
            _objCharacterOptions = objCharacter?.Options ?? new CharacterOptions(null);

            _setCustomDataDirectoryInfos = new HashSet<CustomDataDirectoryInfo>(GlobalOptions.CustomDataDirectoryInfos);
            foreach (KeyValuePair<string, Tuple<int, bool>> kvpCustomDataDirectory in _objCharacterOptions.CustomDataDirectoryNames.OrderBy(x => x.Value.Item1))
            {
                CustomDataDirectoryInfo objLoopInfo = _setCustomDataDirectoryInfos.FirstOrDefault(x => x.Name == kvpCustomDataDirectory.Key);
                if (objLoopInfo != null)
                {
                    _lstCharacterCustomDataDirectoryInfos.Add(
                        new Tuple<object, bool>(
                            objLoopInfo,
                            kvpCustomDataDirectory.Value.Item2));
                }
                else
                {
                    _lstCharacterCustomDataDirectoryInfos.Add(
                        new Tuple<object, bool>(
                            kvpCustomDataDirectory.Key,
                            kvpCustomDataDirectory.Value.Item2));
                }
            }
        }

        private void frmOptions_Load(object sender, EventArgs e)
        {
            PopulateBuildMethodList();
            PopulateDefaultGameplayOptionList();
            PopulateLimbCountList();
            PopulateMugshotCompressionOptions();
            SetToolTips();
            PopulateSettingsList();
            SetDefaultValueForSettingsList();
            PopulateGlobalOptions();
            PopulateLanguageList();
            SetDefaultValueForLanguageList();
            PopulateSheetLanguageList();
            SetDefaultValueForSheetLanguageList();
            PopulateXsltList();
            SetDefaultValueForXsltList();
            PopulatePDFParameters();
            _blnLoading = false;
        }
        #endregion

        #region Control Events
        private void cmdOK_Click(object sender, EventArgs e)
        {
            // Make sure the current Setting has a name.
            if(string.IsNullOrWhiteSpace(txtSettingName.Text))
            {
                string text = LanguageManager.GetString("Message_Options_SettingsName", _strSelectedLanguage);
                string caption = LanguageManager.GetString("MessageTitle_Options_SettingsName", _strSelectedLanguage);

                Program.MainForm.ShowMessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSettingName.Focus();
                return;
            }

            if(_blnDirty)
            {
                string text = LanguageManager.GetString("Message_Options_SaveForms", _strSelectedLanguage);
                string caption = LanguageManager.GetString("MessageTitle_Options_CloseForms", _strSelectedLanguage);

                if(MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }

            DialogResult = DialogResult.OK;

            BuildBooksList();
            BuildCustomDataDirectoryNamesList();
            SaveRegistrySettings();

            _objCharacterOptions.AllowCyberwareESSDiscounts = chkAllowCyberwareESSDiscounts.Checked;
            _objCharacterOptions.AllowInitiationInCreateMode = chkAllowInitiation.Checked;
            _objCharacterOptions.AllowSkillDiceRolling = chkAllowSkillDiceRolling.Checked;
            _objCharacterOptions.DontUseCyberlimbCalculation = chkDontUseCyberlimbCalculation.Checked;
            _objCharacterOptions.AllowSkillRegrouping = chkAllowSkillRegrouping.Checked;
            _objCharacterOptions.ConfirmDelete = chkConfirmDelete.Checked;
            _objCharacterOptions.ConfirmKarmaExpense = chkConfirmKarmaExpense.Checked;
            _objCharacterOptions.CyberlegMovement = chkCyberlegMovement.Checked;
            _objCharacterOptions.UseTotalValueForFreeContacts = chkUseTotalValueForFreeContacts.Checked;
            _objCharacterOptions.UseTotalValueForFreeKnowledge = chkUseTotalValueForFreeKnowledge.Checked;
            _objCharacterOptions.DontDoubleQualityPurchases = chkDontDoubleQualityPurchases.Checked;
            _objCharacterOptions.DontDoubleQualityRefunds = chkDontDoubleQualityRefunds.Checked;
            _objCharacterOptions.EnforceCapacity = chkEnforceCapacity.Checked;
            try
            {
                _objCharacterOptions.FreeContactsMultiplier = decimal.ToInt32(nudContactMultiplier.Value);
                _objCharacterOptions.EssenceDecimals = decimal.ToInt32(nudEssenceDecimals.Value);
                _objCharacterOptions.DroneArmorMultiplier = decimal.ToInt32(nudDroneArmorMultiplier.Value);
                _objCharacterOptions.FreeKnowledgeMultiplier = decimal.ToInt32(nudKnowledgeMultiplier.Value);
                _objCharacterOptions.MetatypeCostsKarmaMultiplier = decimal.ToInt32(nudMetatypeCostsKarmaMultiplier.Value);
                _objCharacterOptions.NuyenPerBP = decimal.ToInt32(nudKarmaNuyenPer.Value);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Error(ex.Message);
            }

            _objCharacterOptions.DontRoundEssenceInternally = chkDontRoundEssenceInternally.Checked;
            _objCharacterOptions.ESSLossReducesMaximumOnly = chkESSLossReducesMaximumOnly.Checked;
            _objCharacterOptions.ExceedNegativeQualities = chkExceedNegativeQualities.Checked;
            _objCharacterOptions.ExceedNegativeQualitiesLimit = chkExceedNegativeQualitiesLimit.Checked;
            _objCharacterOptions.ExceedPositiveQualities = chkExceedPositiveQualities.Checked;
            _objCharacterOptions.ExceedPositiveQualitiesCostDoubled = chkExceedPositiveQualitiesCostDoubled.Checked;
            _objCharacterOptions.ExtendAnyDetectionSpell = chkExtendAnyDetectionSpell.Checked;

            _objCharacterOptions.FreeContactsMultiplierEnabled = chkContactMultiplier.Checked;
            if(chkContactMultiplier.Checked)
                nudContactMultiplier.Enabled = true;

            _objCharacterOptions.DroneArmorMultiplierEnabled = chkDroneArmorMultiplier.Checked;
            nudDroneArmorMultiplier.Enabled = chkDroneArmorMultiplier.Checked;
            _objCharacterOptions.FreeKnowledgeMultiplierEnabled = chkKnowledgeMultiplier.Checked;
            if(chkKnowledgeMultiplier.Checked)
                chkKnowledgeMultiplier.Enabled = true;

            _objCharacterOptions.HideItemsOverAvailLimit = chkHideItemsOverAvail.Checked;
            _objCharacterOptions.IgnoreArt = chkIgnoreArt.Checked;
            _objCharacterOptions.IgnoreComplexFormLimit = chkIgnoreComplexFormLimit.Checked;
            _objCharacterOptions.UnarmedImprovementsApplyToWeapons = chkUnarmedSkillImprovements.Checked;
            _objCharacterOptions.LicenseRestricted = chkLicenseEachRestrictedItem.Checked;
            _objCharacterOptions.ReverseAttributePriorityOrder = chkReverseAttributePriorityOrder.Checked;

            _objCharacterOptions.MoreLethalGameplay = chkMoreLethalGameplay.Checked;
            _objCharacterOptions.NoArmorEncumbrance = chkNoArmorEncumbrance.Checked;

            _objCharacterOptions.PrintExpenses = chkPrintExpenses.Checked;
            _objCharacterOptions.PrintFreeExpenses = chkPrintFreeExpenses.Checked;
            _objCharacterOptions.PrintNotes = chkPrintNotes.Checked;
            _objCharacterOptions.PrintSkillsWithZeroRating = chkPrintSkillsWithZeroRating.Checked;
            _objCharacterOptions.RestrictRecoil = chkRestrictRecoil.Checked;
            _objCharacterOptions.SpecialKarmaCostBasedOnShownValue = chkSpecialKarmaCost.Checked;
            _objCharacterOptions.UseCalculatedPublicAwareness = chkUseCalculatedPublicAwareness.Checked;
            _objCharacterOptions.StrictSkillGroupsInCreateMode = chkStrictSkillGroups.Checked;
            _objCharacterOptions.AllowPointBuySpecializationsOnKarmaSkills = chkAllowPointBuySpecializationsOnKarmaSkills.Checked;
            _objCharacterOptions.AlternateMetatypeAttributeKarma = chkAlternateMetatypeAttributeKarma.Checked;
            _objCharacterOptions.CompensateSkillGroupKarmaDifference = chkCompensateSkillGroupKarmaDifference.Checked;
            _objCharacterOptions.MysAdeptAllowPPCareer = chkMysAdPp.Checked;
            _objCharacterOptions.MysAdeptSecondMAGAttribute = chkMysAdeptSecondMAGAttribute.Checked;
            _objCharacterOptions.FreeMartialArtSpecialization = chkFreeMartialArtSpecialization.Checked;
            _objCharacterOptions.PrioritySpellsAsAdeptPowers = chkPrioritySpellsAsAdeptPowers.Checked;
            _objCharacterOptions.EnemyKarmaQualityLimit = chkEnemyKarmaQualityLimit.Checked;
            _objCharacterOptions.IncreasedImprovedAbilityMultiplier = chkIncreasedImprovedAbilityModifier.Checked;
            _objCharacterOptions.AllowFreeGrids = chkAllowFreeGrids.Checked;
            _objCharacterOptions.AllowTechnomancerSchooling = chkAllowTechnomancerSchooling.Checked;
            _objCharacterOptions.CyberlimbAttributeBonusCap = decimal.ToInt32(nudCyberlimbAttributeBonusCap.Value);
            _objCharacterOptions.UsePointsOnBrokenGroups = chkUsePointsOnBrokenGroups.Checked;

            string strLimbCount = cboLimbCount.SelectedValue?.ToString();
            if(string.IsNullOrEmpty(strLimbCount))
            {
                _objCharacterOptions.LimbCount = 6;
                _objCharacterOptions.ExcludeLimbSlot = string.Empty;
            }
            else
            {
                int intSeparatorIndex = strLimbCount.IndexOf('<');
                if(intSeparatorIndex == -1)
                {
                    _objCharacterOptions.LimbCount = Convert.ToInt32(strLimbCount, GlobalOptions.InvariantCultureInfo);
                    _objCharacterOptions.ExcludeLimbSlot = string.Empty;
                }
                else
                {
                    _objCharacterOptions.LimbCount = Convert.ToInt32(strLimbCount.Substring(0, intSeparatorIndex), GlobalOptions.InvariantCultureInfo);
                    _objCharacterOptions.ExcludeLimbSlot = intSeparatorIndex + 1 < strLimbCount.Length ? strLimbCount.Substring(intSeparatorIndex + 1) : string.Empty;
                }
            }
            _objCharacterOptions.AllowHoverIncrement = chkAllowHoverIncrement.Checked;
            _objCharacterOptions.SearchInCategoryOnly = chkSearchInCategoryOnly.Checked;

            try
            {
                StringBuilder objNuyenFormat = new StringBuilder("#,0");
                int intNuyenDecimalPlacesMaximum = decimal.ToInt32(nudNuyenDecimalsMaximum.Value);
                int intNuyenDecimalPlacesMinimum = decimal.ToInt32(nudNuyenDecimalsMinimum.Value);
                if (intNuyenDecimalPlacesMaximum > 0)
                {
                    objNuyenFormat.Append(".");
                    for (int i = 0; i < intNuyenDecimalPlacesMaximum; ++i)
                    {
                        objNuyenFormat.Append(i < intNuyenDecimalPlacesMinimum ? "0" : "#");
                    }
                }

                _objCharacterOptions.NuyenFormat = objNuyenFormat.ToString();

                // Karma options.
                _objCharacterOptions.KarmaAttribute = decimal.ToInt32(nudKarmaAttribute.Value);
                _objCharacterOptions.KarmaQuality = decimal.ToInt32(nudKarmaQuality.Value);
                _objCharacterOptions.KarmaSpecialization = decimal.ToInt32(nudKarmaSpecialization.Value);
                _objCharacterOptions.KarmaKnowledgeSpecialization = decimal.ToInt32(nudKarmaKnowledgeSpecialization.Value);
                _objCharacterOptions.KarmaNewKnowledgeSkill = decimal.ToInt32(nudKarmaNewKnowledgeSkill.Value);
                _objCharacterOptions.KarmaNewActiveSkill = decimal.ToInt32(nudKarmaNewActiveSkill.Value);
                _objCharacterOptions.KarmaNewSkillGroup = decimal.ToInt32(nudKarmaNewSkillGroup.Value);
                _objCharacterOptions.KarmaImproveKnowledgeSkill = decimal.ToInt32(nudKarmaImproveKnowledgeSkill.Value);
                _objCharacterOptions.KarmaImproveActiveSkill = decimal.ToInt32(nudKarmaImproveActiveSkill.Value);
                _objCharacterOptions.KarmaImproveSkillGroup = decimal.ToInt32(nudKarmaImproveSkillGroup.Value);
                _objCharacterOptions.KarmaSpell = decimal.ToInt32(nudKarmaSpell.Value);
                _objCharacterOptions.KarmaNewComplexForm = decimal.ToInt32(nudKarmaNewComplexForm.Value);
                _objCharacterOptions.KarmaImproveComplexForm = decimal.ToInt32(nudKarmaImproveComplexForm.Value);
                _objCharacterOptions.KarmaNewAIProgram = decimal.ToInt32(nudKarmaNewAIProgram.Value);
                _objCharacterOptions.KarmaNewAIAdvancedProgram = decimal.ToInt32(nudKarmaNewAIAdvancedProgram.Value);
                _objCharacterOptions.KarmaMetamagic = decimal.ToInt32(nudKarmaMetamagic.Value);
                _objCharacterOptions.KarmaNuyenPer = decimal.ToInt32(nudKarmaNuyenPer.Value);
                _objCharacterOptions.KarmaContact = decimal.ToInt32(nudKarmaContact.Value);
                _objCharacterOptions.KarmaEnemy = decimal.ToInt32(nudKarmaEnemy.Value);
                _objCharacterOptions.KarmaCarryover = decimal.ToInt32(nudKarmaCarryover.Value);
                _objCharacterOptions.KarmaSpirit = decimal.ToInt32(nudKarmaSpirit.Value);
                _objCharacterOptions.KarmaManeuver = decimal.ToInt32(nudKarmaManeuver.Value);
                _objCharacterOptions.KarmaInitiation = decimal.ToInt32(nudKarmaInitiation.Value);
                _objCharacterOptions.KarmaInitiationFlat = decimal.ToInt32(nudKarmaInitiationFlat.Value);
                _objCharacterOptions.KarmaComplexFormOption = decimal.ToInt32(nudKarmaComplexFormOption.Value);
                _objCharacterOptions.KarmaComplexFormSkillsoft = decimal.ToInt32(nudKarmaComplexFormSkillsoft.Value);
                _objCharacterOptions.KarmaJoinGroup = decimal.ToInt32(nudKarmaJoinGroup.Value);
                _objCharacterOptions.KarmaLeaveGroup = decimal.ToInt32(nudKarmaLeaveGroup.Value);
                _objCharacterOptions.KarmaMysticAdeptPowerPoint = (int) nudKarmaMysticAdeptPowerPoint.Value;

                // Focus costs
                _objCharacterOptions.KarmaAlchemicalFocus = decimal.ToInt32(nudKarmaAlchemicalFocus.Value);
                _objCharacterOptions.KarmaBanishingFocus = decimal.ToInt32(nudKarmaBanishingFocus.Value);
                _objCharacterOptions.KarmaBindingFocus = decimal.ToInt32(nudKarmaBindingFocus.Value);
                _objCharacterOptions.KarmaCenteringFocus = decimal.ToInt32(nudKarmaCenteringFocus.Value);
                _objCharacterOptions.KarmaCounterspellingFocus = decimal.ToInt32(nudKarmaCounterspellingFocus.Value);
                _objCharacterOptions.KarmaDisenchantingFocus = decimal.ToInt32(nudKarmaDisenchantingFocus.Value);
                _objCharacterOptions.KarmaFlexibleSignatureFocus = decimal.ToInt32(nudKarmaFlexibleSignatureFocus.Value);
                _objCharacterOptions.KarmaMaskingFocus = decimal.ToInt32(nudKarmaMaskingFocus.Value);
                _objCharacterOptions.KarmaPowerFocus = decimal.ToInt32(nudKarmaPowerFocus.Value);
                _objCharacterOptions.KarmaQiFocus = decimal.ToInt32(nudKarmaQiFocus.Value);
                _objCharacterOptions.KarmaRitualSpellcastingFocus = decimal.ToInt32(nudKarmaRitualSpellcastingFocus.Value);
                _objCharacterOptions.KarmaSpellcastingFocus = decimal.ToInt32(nudKarmaSpellcastingFocus.Value);
                _objCharacterOptions.KarmaSpellShapingFocus = decimal.ToInt32(nudKarmaSpellShapingFocus.Value);
                _objCharacterOptions.KarmaSummoningFocus = decimal.ToInt32(nudKarmaSummoningFocus.Value);
                _objCharacterOptions.KarmaSustainingFocus = decimal.ToInt32(nudKarmaSustainingFocus.Value);
                _objCharacterOptions.KarmaWeaponFocus = decimal.ToInt32(nudKarmaWeaponFocus.Value);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Error(ex.Message);
            }

            _objCharacterOptions.Name = txtSettingName.Text;
            _objCharacterOptions.Save();

            if(_blnDirty)
                Utils.RestartApplication(_strSelectedLanguage, "Message_Options_CloseForms");
        }

        private void cboSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedFile = cboSetting.SelectedValue?.ToString();
            if(strSelectedFile?.Contains(".xml") != true)
                return;

            _objCharacterOptions.Load(strSelectedFile);
            PopulateOptions();
        }

        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            _strSelectedLanguage = cboLanguage.SelectedValue?.ToString() ?? GlobalOptions.DefaultLanguage;
            try
            {
                _objSelectedCultureInfo = CultureInfo.GetCultureInfo(_strSelectedLanguage);
            }
            catch (CultureNotFoundException)
            {
                _objSelectedCultureInfo = GlobalOptions.SystemCultureInfo;
            }

            imgLanguageFlag.Image = FlagImageGetter.GetFlagFromCountryCode(_strSelectedLanguage.Substring(3, 2));

            bool isEnabled = !string.IsNullOrEmpty(_strSelectedLanguage) && _strSelectedLanguage != GlobalOptions.DefaultLanguage;
            cmdVerify.Enabled = isEnabled;
            cmdVerifyData.Enabled = isEnabled;

            if(!_blnLoading)
            {
                Cursor = Cursors.WaitCursor;
                TranslateForm();
                Cursor = Cursors.Default;
            }

            OptionsChanged(sender, e);
        }

        private void cboSheetLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateXsltList();
        }

        private void cmdVerify_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            LanguageManager.VerifyStrings(_strSelectedLanguage);
            Cursor = Cursors.Default;
        }

        private void cmdVerifyData_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            // Build a list of Sourcebooks that will be passed to the Verify method.
            // This is done since not all of the books are available in every language or the user may only wish to verify the content of certain books.
            List<string> lstBooks = new List<string>();
            bool blnSR5Included = false;

            foreach(ListItem objItem in lstGlobalSourcebookInfos.Items)
            {
                string strItemValue = objItem.Value?.ToString();
                lstBooks.Add(strItemValue);
                if(strItemValue == "SR5")
                    blnSR5Included = true;
            }

            // If the SR5 book was somehow missed, add it back.
            if(!blnSR5Included)
                _objCharacterOptions.Books.Add("SR5");
            _objCharacterOptions.RecalculateBookXPath();

            string strSelectedLanguage = _strSelectedLanguage;
            XmlManager.Verify(strSelectedLanguage, lstBooks);

            string strFilePath = Path.Combine(Utils.GetStartupPath, "lang", "results_" + strSelectedLanguage + ".xml");
            Program.MainForm.ShowMessageBox(string.Format(_objSelectedCultureInfo, LanguageManager.GetString("Message_Options_ValidationResults", _strSelectedLanguage), strFilePath),
                LanguageManager.GetString("MessageTitle_Options_ValidationResults", _strSelectedLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
            Cursor = Cursors.Default;
        }

        private void chkExceedNegativeQualities_CheckedChanged(object sender, EventArgs e)
        {
            chkExceedNegativeQualitiesLimit.Enabled = chkExceedNegativeQualities.Checked;
            if(!chkExceedNegativeQualitiesLimit.Enabled)
                chkExceedNegativeQualitiesLimit.Checked = false;
            OptionsChanged(sender, e);
        }

        private void chkExceedPositiveQualities_CheckedChanged(object sender, EventArgs e)
        {
            chkExceedPositiveQualitiesCostDoubled.Enabled = chkExceedPositiveQualities.Checked;
            if(!chkExceedPositiveQualitiesCostDoubled.Enabled)
                chkExceedPositiveQualitiesCostDoubled.Checked = false;
            OptionsChanged(sender, e);
        }

        private void chkContactMultiplier_CheckedChanged(object sender, EventArgs e)
        {
            nudContactMultiplier.Enabled = chkContactMultiplier.Checked;
            if(!chkContactMultiplier.Checked)
            {
                nudContactMultiplier.Value = 3;
                nudContactMultiplier.Enabled = false;
            }
        }

        private void chkKnowledgeMultiplier_CheckedChanged(object sender, EventArgs e)
        {
            nudKnowledgeMultiplier.Enabled = chkKnowledgeMultiplier.Checked;
            if(!chkKnowledgeMultiplier.Checked)
            {
                nudKnowledgeMultiplier.Value = 2;
                nudKnowledgeMultiplier.Enabled = false;
            }
        }

        private void chkDroneArmorMultiplier_CheckedChanged(object sender, EventArgs e)
        {
            nudDroneArmorMultiplier.Enabled = chkDroneArmorMultiplier.Checked;
            if(!chkDroneArmorMultiplier.Checked)
            {
                nudDroneArmorMultiplier.Value = 2;
            }
            OptionsChanged(sender, e);
        }

        private void cmdRestoreDefaultsKarma_Click(object sender, EventArgs e)
        {
            string text = LanguageManager.GetString("Message_Options_RestoreDefaults", _strSelectedLanguage);
            string caption = LanguageManager.GetString("MessageTitle_Options_RestoreDefaults", _strSelectedLanguage);

            // Verify that the user wants to reset these values.
            if(MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            RestoreDefaultKarmaValues();
            RestoreDefaultKarmaFociValues();
        }

        private void cmdPDFAppPath_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Exe") + '|' + LanguageManager.GetString("DialogFilter_All")
            })
            {
                if (!string.IsNullOrEmpty(txtPDFAppPath.Text) && File.Exists(txtPDFAppPath.Text))
                {
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(txtPDFAppPath.Text);
                    openFileDialog.FileName = Path.GetFileName(txtPDFAppPath.Text);
                }

                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                    txtPDFAppPath.Text = openFileDialog.FileName;
            }
        }

        private void cmdPDFLocation_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Pdf") + '|' + LanguageManager.GetString("DialogFilter_All")
            })
            {
                if (!string.IsNullOrEmpty(txtPDFLocation.Text) && File.Exists(txtPDFLocation.Text))
                {
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(txtPDFLocation.Text);
                    openFileDialog.FileName = Path.GetFileName(txtPDFLocation.Text);
                }

                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    UpdateSourcebookInfoPath(openFileDialog.FileName);
                    txtPDFLocation.Text = openFileDialog.FileName;
                }
            }
        }

        private void lstGlobalSourcebookInfos_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedCode = lstGlobalSourcebookInfos.SelectedValue?.ToString();

            // Find the selected item in the Sourcebook List.
            SourcebookInfo objSource = !string.IsNullOrEmpty(strSelectedCode) ? GlobalOptions.SourcebookInfo.FirstOrDefault(x => x.Code == strSelectedCode) : null;

            if (objSource != null)
            {
                grpSelectedSourcebook.Enabled = true;
                txtPDFLocation.Text = objSource.Path;
                nudPDFOffset.Value = objSource.Offset;
            }
            else
            {
                grpSelectedSourcebook.Enabled = false;
            }
        }

        private void nudPDFOffset_ValueChanged(object sender, EventArgs e)
        {
            if(_blnSkipRefresh || _blnLoading)
                return;

            int intOffset = decimal.ToInt32(nudPDFOffset.Value);
            string strTag = lstGlobalSourcebookInfos.SelectedValue?.ToString();
            SourcebookInfo objFoundSource = GlobalOptions.SourcebookInfo.FirstOrDefault(x => x.Code == strTag);

            if(objFoundSource != null)
            {
                objFoundSource.Offset = intOffset;
            }
            else
            {
                // If the Sourcebook was not found in the options, add it.
                GlobalOptions.SourcebookInfo.Add(new SourcebookInfo
                {
                    Code = strTag,
                    Offset = intOffset
                });
            }
        }

        private void cmdPDFTest_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(txtPDFLocation.Text))
                return;

            CommonFunctions.OpenPDF(lstGlobalSourcebookInfos.SelectedValue + " 5", _objCharacter, cboPDFParameters.SelectedValue?.ToString() ?? string.Empty, txtPDFAppPath.Text);
        }
        #endregion

        #region Methods
        private void TranslateForm()
        {
            this.TranslateWinForm(_strSelectedLanguage);
            PopulateBuildMethodList();
            PopulateDefaultGameplayOptionList();

            XmlNode xmlBooksNode = XmlManager.Load("books.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths, _strSelectedLanguage).SelectSingleNode("/chummer/books");
            if(xmlBooksNode != null)
            {
                RefreshGlobalSourcebookInfosListView();

                foreach(TreeNode nodBook in treSourcebook.Nodes)
                {
                    XmlNode xmlBook = xmlBooksNode.SelectSingleNode("book[code = \"" + nodBook.Tag + "\"]");
                    if(xmlBook != null)
                    {
                        nodBook.Text = xmlBook["translate"]?.InnerText ?? xmlBook["name"]?.InnerText ?? string.Empty;
                    }
                }

                treSourcebook.Sort();
            }

            PopulateLimbCountList();
            PopulateMugshotCompressionOptions();
            SetToolTips();

            string strSheetLanguage = cboSheetLanguage.SelectedValue?.ToString();
            if(strSheetLanguage != _strSelectedLanguage)
            {
                if(cboSheetLanguage.Items.Cast<ListItem>().Any(x => x.Value.ToString() == _strSelectedLanguage))
                {
                    cboSheetLanguage.SelectedValue = _strSelectedLanguage;
                }
            }

            PopulatePDFParameters();
            PopulateCustomDataDirectoryTreeView();
            PopulateApplicationInsightsOptions();
        }

        private void RefreshGlobalSourcebookInfosListView()
        {
            // Load the Sourcebook information.
            XmlDocument objXmlDocument = XmlManager.Load("books.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths, _strSelectedLanguage);

            // Put the Sourcebooks into a List so they can first be sorted.
            List<ListItem> lstSourcebookInfos = new List<ListItem>();

            using(XmlNodeList objXmlBookList = objXmlDocument.SelectNodes("/chummer/books/book"))
            {
                if(objXmlBookList != null)
                {
                    foreach(XmlNode objXmlBook in objXmlBookList)
                    {
                        string strCode = objXmlBook["code"]?.InnerText;
                        if(!string.IsNullOrEmpty(strCode))
                        {
                            ListItem objBookInfo = new ListItem(strCode, objXmlBook["translate"]?.InnerText ?? objXmlBook["name"]?.InnerText ?? strCode);
                            lstSourcebookInfos.Add(objBookInfo);
                        }
                    }
                }
            }

            lstSourcebookInfos.Sort(CompareListItems.CompareNames);
            bool blnOldSkipRefresh = _blnSkipRefresh;
            _blnSkipRefresh = true;
            lstGlobalSourcebookInfos.BeginUpdate();
            string strOldSelected = lstGlobalSourcebookInfos.SelectedValue?.ToString();
            lstGlobalSourcebookInfos.DataSource = null;
            lstGlobalSourcebookInfos.DataSource = lstSourcebookInfos;
            lstGlobalSourcebookInfos.ValueMember = nameof(ListItem.Value);
            lstGlobalSourcebookInfos.DisplayMember = nameof(ListItem.Name);
            _blnSkipRefresh = blnOldSkipRefresh;
            if(string.IsNullOrEmpty(strOldSelected))
                lstGlobalSourcebookInfos.SelectedIndex = -1;
            else
                lstGlobalSourcebookInfos.SelectedValue = strOldSelected;
            lstGlobalSourcebookInfos.EndUpdate();
        }

        private void PopulateSourcebookTreeView()
        {
            // Load the Sourcebook information.
            XmlDocument objXmlDocument = XmlManager.Load("books.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths, _strSelectedLanguage);

            // Put the Sourcebooks into a List so they can first be sorted.

            treSourcebook.Nodes.Clear();

            using(XmlNodeList objXmlBookList = objXmlDocument.SelectNodes("/chummer/books/book"))
            {
                if(objXmlBookList != null)
                {
                    foreach(XmlNode objXmlBook in objXmlBookList)
                    {
                        if(objXmlBook["hide"] != null)
                            continue;
                        string strCode = objXmlBook["code"]?.InnerText;
                        bool blnChecked = _objCharacterOptions.Books.Contains(strCode);
                        TreeNode objNode = new TreeNode
                        {
                            Text = objXmlBook["translate"]?.InnerText ?? objXmlBook["name"]?.InnerText ?? string.Empty,
                            Tag = strCode,
                            Checked = blnChecked
                        };
                        treSourcebook.Nodes.Add(objNode);
                    }
                }
            }

            treSourcebook.Sort();
        }

        private void PopulateCustomDataDirectoryTreeView()
        {
            object objOldSelected = treCustomDataDirectories.SelectedNode?.Tag;
            if(_lstCharacterCustomDataDirectoryInfos.Count != treCustomDataDirectories.Nodes.Count)
            {
                treCustomDataDirectories.Nodes.Clear();

                foreach (Tuple<object, bool> objCustomDataDirectory in _lstCharacterCustomDataDirectoryInfos)
                {
                    TreeNode objNode = new TreeNode
                    {
                        Tag = objCustomDataDirectory.Item1,
                        Checked = objCustomDataDirectory.Item2
                    };
                    if (objCustomDataDirectory.Item1 is CustomDataDirectoryInfo objInfo)
                    {
                        objNode.Text = objInfo.Name;
                    }
                    else
                    {
                        objNode.Text = objCustomDataDirectory.Item1.ToString();
                        objNode.ForeColor = SystemColors.GrayText;
                    }
                    treCustomDataDirectories.Nodes.Add(objNode);
                }
            }
            else
            {
                for(int i = 0; i < treCustomDataDirectories.Nodes.Count; ++i)
                {
                    TreeNode objNode = treCustomDataDirectories.Nodes[i];
                    Tuple<object, bool> objCustomDataDirectory = _lstCharacterCustomDataDirectoryInfos[i];
                    objNode.Tag = objCustomDataDirectory.Item1;
                    objNode.Checked = objCustomDataDirectory.Item2;
                    if (objCustomDataDirectory.Item1 is CustomDataDirectoryInfo objInfo)
                    {
                        objNode.Text = objInfo.Name;
                    }
                    else
                    {
                        objNode.Text = objCustomDataDirectory.Item1.ToString();
                        objNode.ForeColor = SystemColors.GrayText;
                    }
                }
            }

            if(objOldSelected != null)
                treCustomDataDirectories.SelectedNode = treCustomDataDirectories.FindNodeByTag(objOldSelected);
        }

        /// <summary>
        /// Set the values for all of the controls based on the Options for the selected Setting.
        /// </summary>
        private void PopulateOptions()
        {
            RefreshGlobalSourcebookInfosListView();
            PopulateSourcebookTreeView();
            PopulateCustomDataDirectoryTreeView();

            nudEssenceDecimals.Value = _objCharacterOptions.EssenceDecimals == 0 ? 2 : _objCharacterOptions.EssenceDecimals;
            chkDontRoundEssenceInternally.Checked = _objCharacterOptions.DontRoundEssenceInternally;
            chkAllowCyberwareESSDiscounts.Checked = _objCharacterOptions.AllowCyberwareESSDiscounts;
            chkAllowInitiation.Checked = _objCharacterOptions.AllowInitiationInCreateMode;
            chkAllowSkillDiceRolling.Checked = _objCharacterOptions.AllowSkillDiceRolling;
            chkDontUseCyberlimbCalculation.Checked = _objCharacterOptions.DontUseCyberlimbCalculation;
            chkAllowSkillRegrouping.Checked = _objCharacterOptions.AllowSkillRegrouping;
            chkConfirmDelete.Checked = _objCharacterOptions.ConfirmDelete;
            chkConfirmKarmaExpense.Checked = _objCharacterOptions.ConfirmKarmaExpense;
            chkUseTotalValueForFreeContacts.Checked = _objCharacterOptions.UseTotalValueForFreeContacts;
            chkUseTotalValueForFreeKnowledge.Checked = _objCharacterOptions.UseTotalValueForFreeKnowledge;
            chkContactMultiplier.Checked = _objCharacterOptions.FreeContactsMultiplierEnabled;
            chkDroneArmorMultiplier.Checked = _objCharacterOptions.DroneArmorMultiplierEnabled;
            chkCyberlegMovement.Checked = _objCharacterOptions.CyberlegMovement;
            chkMysAdPp.Checked = _objCharacterOptions.MysAdeptAllowPPCareer;
            chkMysAdeptSecondMAGAttribute.Checked = _objCharacterOptions.MysAdeptSecondMAGAttribute;
            chkHideItemsOverAvail.Checked = _objCharacterOptions.HideItemsOverAvailLimit;
            chkFreeMartialArtSpecialization.Checked = _objCharacterOptions.FreeMartialArtSpecialization;
            chkPrioritySpellsAsAdeptPowers.Checked = _objCharacterOptions.PrioritySpellsAsAdeptPowers;
            chkDontDoubleQualityPurchases.Checked = _objCharacterOptions.DontDoubleQualityPurchases;
            chkDontDoubleQualityRefunds.Checked = _objCharacterOptions.DontDoubleQualityRefunds;
            chkEnforceCapacity.Checked = _objCharacterOptions.EnforceCapacity;
            chkESSLossReducesMaximumOnly.Checked = _objCharacterOptions.ESSLossReducesMaximumOnly;
            chkExceedNegativeQualities.Checked = _objCharacterOptions.ExceedNegativeQualities;
            chkExceedNegativeQualitiesLimit.Checked = _objCharacterOptions.ExceedNegativeQualitiesLimit;
            chkExceedNegativeQualitiesLimit.Enabled = chkExceedNegativeQualities.Checked;
            chkExceedPositiveQualities.Checked = _objCharacterOptions.ExceedPositiveQualities;
            chkExceedPositiveQualitiesCostDoubled.Checked = _objCharacterOptions.ExceedPositiveQualitiesCostDoubled;
            chkExceedPositiveQualitiesCostDoubled.Enabled = chkExceedPositiveQualities.Checked;
            chkExtendAnyDetectionSpell.Checked = _objCharacterOptions.ExtendAnyDetectionSpell;
            chkIgnoreArt.Checked = _objCharacterOptions.IgnoreArt;
            chkIgnoreComplexFormLimit.Checked = _objCharacterOptions.IgnoreComplexFormLimit;
            chkKnowledgeMultiplier.Checked = _objCharacterOptions.FreeKnowledgeMultiplierEnabled;
            chkUnarmedSkillImprovements.Checked = _objCharacterOptions.UnarmedImprovementsApplyToWeapons;
            chkLicenseEachRestrictedItem.Checked = _objCharacterOptions.LicenseRestricted;
            chkMoreLethalGameplay.Checked = _objCharacterOptions.MoreLethalGameplay;
            chkNoArmorEncumbrance.Checked = _objCharacterOptions.NoArmorEncumbrance;
            chkPrintExpenses.Checked = _objCharacterOptions.PrintExpenses;
            chkPrintFreeExpenses.Checked = _objCharacterOptions.PrintFreeExpenses;
            chkPrintFreeExpenses.Enabled = chkPrintExpenses.Checked;
            chkPrintNotes.Checked = _objCharacterOptions.PrintNotes;
            chkPrintSkillsWithZeroRating.Checked = _objCharacterOptions.PrintSkillsWithZeroRating;
            chkRestrictRecoil.Checked = _objCharacterOptions.RestrictRecoil;
            chkSpecialKarmaCost.Checked = _objCharacterOptions.SpecialKarmaCostBasedOnShownValue;
            chkUseCalculatedPublicAwareness.Checked = _objCharacterOptions.UseCalculatedPublicAwareness;
            chkAllowPointBuySpecializationsOnKarmaSkills.Checked = _objCharacterOptions.AllowPointBuySpecializationsOnKarmaSkills;
            chkStrictSkillGroups.Checked = _objCharacterOptions.StrictSkillGroupsInCreateMode;
            chkAlternateMetatypeAttributeKarma.Checked = _objCharacterOptions.AlternateMetatypeAttributeKarma;
            chkCompensateSkillGroupKarmaDifference.Checked = _objCharacterOptions.CompensateSkillGroupKarmaDifference;
            chkEnemyKarmaQualityLimit.Checked = _objCharacterOptions.EnemyKarmaQualityLimit;
            chkAllowTechnomancerSchooling.Checked = _objCharacterOptions.AllowTechnomancerSchooling;
            chkCyberlimbAttributeBonusCap.Checked = _objCharacterOptions.CyberlimbAttributeBonusCap != 4;
            nudCyberlimbAttributeBonusCap.Enabled = chkCyberlimbAttributeBonusCap.Checked;
            nudCyberlimbAttributeBonusCap.Value = _objCharacterOptions.CyberlimbAttributeBonusCap;
            chkReverseAttributePriorityOrder.Checked = _objCharacterOptions.ReverseAttributePriorityOrder;
            chkAllowHoverIncrement.Checked = _objCharacterOptions.AllowHoverIncrement;
            chkSearchInCategoryOnly.Checked = _objCharacterOptions.SearchInCategoryOnly;
            chkIncreasedImprovedAbilityModifier.Checked = _objCharacterOptions.IncreasedImprovedAbilityMultiplier;
            chkAllowFreeGrids.Checked = _objCharacterOptions.AllowFreeGrids;
            nudContactMultiplier.Enabled = _objCharacterOptions.FreeContactsMultiplierEnabled;
            nudContactMultiplier.Value = _objCharacterOptions.FreeContactsMultiplier;
            nudKnowledgeMultiplier.Enabled = _objCharacterOptions.FreeKnowledgeMultiplierEnabled;
            nudKnowledgeMultiplier.Value = _objCharacterOptions.FreeKnowledgeMultiplier;
            nudDroneArmorMultiplier.Enabled = _objCharacterOptions.DroneArmorMultiplierEnabled;
            nudDroneArmorMultiplier.Value = _objCharacterOptions.DroneArmorMultiplier;
            nudMetatypeCostsKarmaMultiplier.Value = _objCharacterOptions.MetatypeCostsKarmaMultiplier;
            nudNuyenPerBP.Value = _objCharacterOptions.NuyenPerBP;
            chkUsePointsOnBrokenGroups.Checked = _objCharacterOptions.UsePointsOnBrokenGroups;

            txtSettingName.Enabled = cboSetting.SelectedValue?.ToString() != "default.xml";
            txtSettingName.Text = _objCharacterOptions.Name;

            int intNuyenDecimalPlacesMaximum = 0;
            int intNuyenDecimalPlacesAlways = 0;
            string strNuyenFormat = _objCharacterOptions.NuyenFormat;
            int intDecimalIndex = strNuyenFormat.IndexOf('.');
            if(intDecimalIndex != -1)
            {
                strNuyenFormat = strNuyenFormat.Substring(intDecimalIndex);
                intNuyenDecimalPlacesMaximum = strNuyenFormat.Length - 1;
                intNuyenDecimalPlacesAlways = strNuyenFormat.IndexOf('#') - 1;
                if(intNuyenDecimalPlacesAlways < 0)
                    intNuyenDecimalPlacesAlways = intNuyenDecimalPlacesMaximum;
            }
            nudNuyenDecimalsMaximum.Value = intNuyenDecimalPlacesMaximum;
            nudNuyenDecimalsMinimum.Value = intNuyenDecimalPlacesAlways;

            string strLimbSlot = _objCharacterOptions.LimbCount.ToString(GlobalOptions.InvariantCultureInfo);
            if(!string.IsNullOrEmpty(_objCharacterOptions.ExcludeLimbSlot))
                strLimbSlot += '<' + _objCharacterOptions.ExcludeLimbSlot;
            cboLimbCount.SelectedValue = strLimbSlot;

            PopulateKarmaFields();
        }

        private void PopulateKarmaFields()
        {
            nudKarmaAttribute.Value = _objCharacterOptions.KarmaAttribute;
            nudKarmaQuality.Value = _objCharacterOptions.KarmaQuality;
            nudKarmaSpecialization.Value = _objCharacterOptions.KarmaSpecialization;
            nudKarmaKnowledgeSpecialization.Value = _objCharacterOptions.KarmaKnowledgeSpecialization;
            nudKarmaNewKnowledgeSkill.Value = _objCharacterOptions.KarmaNewKnowledgeSkill;
            nudKarmaNewActiveSkill.Value = _objCharacterOptions.KarmaNewActiveSkill;
            nudKarmaNewSkillGroup.Value = _objCharacterOptions.KarmaNewSkillGroup;
            nudKarmaImproveKnowledgeSkill.Value = _objCharacterOptions.KarmaImproveKnowledgeSkill;
            nudKarmaImproveActiveSkill.Value = _objCharacterOptions.KarmaImproveActiveSkill;
            nudKarmaImproveSkillGroup.Value = _objCharacterOptions.KarmaImproveSkillGroup;
            nudKarmaSpell.Value = _objCharacterOptions.KarmaSpell;
            nudKarmaNewComplexForm.Value = _objCharacterOptions.KarmaNewComplexForm;
            nudKarmaImproveComplexForm.Value = _objCharacterOptions.KarmaImproveComplexForm;
            nudKarmaNewAIProgram.Value = _objCharacterOptions.KarmaNewAIProgram;
            nudKarmaNewAIAdvancedProgram.Value = _objCharacterOptions.KarmaNewAIAdvancedProgram;
            nudKarmaComplexFormOption.Value = _objCharacterOptions.KarmaComplexFormOption;
            nudKarmaComplexFormSkillsoft.Value = _objCharacterOptions.KarmaComplexFormSkillsoft;
            nudKarmaNuyenPer.Value = _objCharacterOptions.KarmaNuyenPer;
            nudKarmaContact.Value = _objCharacterOptions.KarmaContact;
            nudKarmaEnemy.Value = _objCharacterOptions.KarmaEnemy;
            nudKarmaCarryover.Value = _objCharacterOptions.KarmaCarryover;
            nudKarmaSpirit.Value = _objCharacterOptions.KarmaSpirit;
            nudKarmaManeuver.Value = _objCharacterOptions.KarmaManeuver;
            nudKarmaInitiation.Value = _objCharacterOptions.KarmaInitiation;
            nudKarmaInitiationFlat.Value = _objCharacterOptions.KarmaInitiationFlat;
            nudKarmaMetamagic.Value = _objCharacterOptions.KarmaMetamagic;
            nudKarmaJoinGroup.Value = _objCharacterOptions.KarmaJoinGroup;
            nudKarmaLeaveGroup.Value = _objCharacterOptions.KarmaLeaveGroup;
            nudKarmaAlchemicalFocus.Value = _objCharacterOptions.KarmaAlchemicalFocus;
            nudKarmaBanishingFocus.Value = _objCharacterOptions.KarmaBanishingFocus;
            nudKarmaBindingFocus.Value = _objCharacterOptions.KarmaBindingFocus;
            nudKarmaCenteringFocus.Value = _objCharacterOptions.KarmaCenteringFocus;
            nudKarmaCounterspellingFocus.Value = _objCharacterOptions.KarmaCounterspellingFocus;
            nudKarmaDisenchantingFocus.Value = _objCharacterOptions.KarmaDisenchantingFocus;
            nudKarmaFlexibleSignatureFocus.Value = _objCharacterOptions.KarmaFlexibleSignatureFocus;
            nudKarmaMaskingFocus.Value = _objCharacterOptions.KarmaMaskingFocus;
            nudKarmaPowerFocus.Value = _objCharacterOptions.KarmaPowerFocus;
            nudKarmaQiFocus.Value = _objCharacterOptions.KarmaQiFocus;
            nudKarmaRitualSpellcastingFocus.Value = _objCharacterOptions.KarmaRitualSpellcastingFocus;
            nudKarmaSpellcastingFocus.Value = _objCharacterOptions.KarmaSpellcastingFocus;
            nudKarmaSpellShapingFocus.Value = _objCharacterOptions.KarmaSpellShapingFocus;
            nudKarmaSummoningFocus.Value = _objCharacterOptions.KarmaSummoningFocus;
            nudKarmaSustainingFocus.Value = _objCharacterOptions.KarmaSustainingFocus;
            nudKarmaWeaponFocus.Value = _objCharacterOptions.KarmaWeaponFocus;
            nudKarmaMysticAdeptPowerPoint.Value = _objCharacterOptions.KarmaMysticAdeptPowerPoint;
        }

        private void SaveGlobalOptions()
        {
            GlobalOptions.AutomaticUpdate = chkAutomaticUpdate.Checked;
            GlobalOptions.LiveCustomData = chkLiveCustomData.Checked;
            GlobalOptions.LiveUpdateCleanCharacterFiles = chkLiveUpdateCleanCharacterFiles.Checked;
            GlobalOptions.UseLogging = chkUseLogging.Checked;
            if (Enum.TryParse(cboUseLoggingApplicationInsights.SelectedValue.ToString(), out UseAILogging useAI))
                GlobalOptions.UseLoggingApplicationInsights = useAI;

            if (string.IsNullOrEmpty(_strSelectedLanguage))
            {
                // We have this set differently because changing the selected language also changes the selected default character sheet
                _strSelectedLanguage = GlobalOptions.DefaultLanguage;
                try
                {
                    _objSelectedCultureInfo = CultureInfo.GetCultureInfo(_strSelectedLanguage);
                }
                catch (CultureNotFoundException)
                {
                    _objSelectedCultureInfo = GlobalOptions.SystemCultureInfo;
                }
            }
            GlobalOptions.Language = _strSelectedLanguage;
            GlobalOptions.StartupFullscreen = chkStartupFullscreen.Checked;
            GlobalOptions.SingleDiceRoller = chkSingleDiceRoller.Checked;
            GlobalOptions.DefaultCharacterSheet = cboXSLT.SelectedValue?.ToString() ?? GlobalOptions.DefaultCharacterSheetDefaultValue;
            GlobalOptions.DatesIncludeTime = chkDatesIncludeTime.Checked;
            GlobalOptions.PrintToFileFirst = chkPrintToFileFirst.Checked;
            GlobalOptions.EmulatedBrowserVersion = decimal.ToInt32(nudBrowserVersion.Value);
            GlobalOptions.PDFAppPath = txtPDFAppPath.Text;
            GlobalOptions.PDFParameters = cboPDFParameters.SelectedValue?.ToString() ?? string.Empty;
            GlobalOptions.LifeModuleEnabled = chkLifeModule.Checked;
            GlobalOptions.OmaeEnabled = chkOmaeEnabled.Checked;
            GlobalOptions.PreferNightlyBuilds = chkPreferNightlyBuilds.Checked;
            GlobalOptions.Dronemods = chkDronemods.Checked;
            GlobalOptions.DronemodsMaximumPilot = chkDronemodsMaximumPilot.Checked;
            GlobalOptions.CharacterRosterPath = txtCharacterRosterPath.Text;
            GlobalOptions.HideCharacterRoster = chkHideCharacterRoster.Checked;
            GlobalOptions.CreateBackupOnCareer = chkCreateBackupOnCareer.Checked;
            GlobalOptions.DefaultBuildMethod = cboBuildMethod.SelectedValue?.ToString() ?? GlobalOptions.DefaultBuildMethodDefaultValue;
            GlobalOptions.DefaultGameplayOption = XmlManager.Load("gameplayoptions.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths, _strSelectedLanguage)
                .SelectSingleNode("/chummer/gameplayoptions/gameplayoption[id = \"" + cboDefaultGameplayOption.SelectedValue + "\"]/name")?.InnerText
                                                  ?? GlobalOptions.DefaultGameplayOptionDefaultValue;
            GlobalOptions.PluginsEnabled = chkEnablePlugins.Enabled;
            GlobalOptions.SavedImageQuality = nudMugshotCompressionQuality.Enabled ? decimal.ToInt32(nudMugshotCompressionQuality.Value) : int.MaxValue;
        }

        /// <summary>
        /// Save the global settings to the registry.
        /// </summary>
        private void SaveRegistrySettings()
        {
            SaveGlobalOptions();

            using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5"))
            {
                if (objRegistry != null)
                {
                    objRegistry.SetValue("autoupdate", chkAutomaticUpdate.Checked);
                    objRegistry.SetValue("livecustomdata", chkLiveCustomData.Checked);
                    objRegistry.SetValue("liveupdatecleancharacterfiles", chkLiveUpdateCleanCharacterFiles.Checked);
                    objRegistry.SetValue("uselogging", chkUseLogging.Checked);
                    var useAI = cboUseLoggingApplicationInsights.SelectedItem.ToString();
                    objRegistry.SetValue("useloggingApplicationInsights", useAI);
                    objRegistry.SetValue("language", _strSelectedLanguage);
                    objRegistry.SetValue("startupfullscreen", chkStartupFullscreen.Checked);
                    objRegistry.SetValue("singlediceroller", chkSingleDiceRoller.Checked);
                    objRegistry.SetValue("defaultsheet", cboXSLT.SelectedValue?.ToString() ?? GlobalOptions.DefaultCharacterSheetDefaultValue);
                    objRegistry.SetValue("defaultbuildmethod", cboBuildMethod.SelectedValue?.ToString() ?? GlobalOptions.DefaultBuildMethodDefaultValue);
                    objRegistry.SetValue("datesincludetime", chkDatesIncludeTime.Checked);
                    objRegistry.SetValue("printtofilefirst", chkPrintToFileFirst.Checked);
                    objRegistry.SetValue("emulatedbrowserversion", nudBrowserVersion.Value.ToString(GlobalOptions.InvariantCultureInfo));
                    objRegistry.SetValue("pdfapppath", txtPDFAppPath.Text);
                    objRegistry.SetValue("pdfparameters", cboPDFParameters.SelectedValue.ToString());
                    objRegistry.SetValue("lifemodule", chkLifeModule.Checked);
                    objRegistry.SetValue("omaeenabled", chkOmaeEnabled.Checked);
                    objRegistry.SetValue("prefernightlybuilds", chkPreferNightlyBuilds.Checked);
                    objRegistry.SetValue("dronemods", chkDronemods.Checked);
                    objRegistry.SetValue("dronemodsPilot", chkDronemodsMaximumPilot.Checked);
                    objRegistry.SetValue("characterrosterpath", txtCharacterRosterPath.Text);
                    objRegistry.SetValue("hidecharacterroster", chkHideCharacterRoster.Checked);
                    objRegistry.SetValue("createbackuponcareer", chkCreateBackupOnCareer.Checked);
                    objRegistry.SetValue("pluginsenabled", chkEnablePlugins.Checked);
                    objRegistry.SetValue("alloweastereggs", chkAllowEasterEggs.Checked);
                    objRegistry.SetValue("hidecharts", chkHideCharts.Checked);
                    objRegistry.SetValue("usecustomdatetime", chkCustomDateTimeFormats.Checked);
                    objRegistry.SetValue("customdateformat", txtDateFormat.Text);
                    objRegistry.SetValue("customtimeformat", txtTimeFormat.Text);
                    objRegistry.SetValue("savedimagequality", nudMugshotCompressionQuality.Enabled
                        ? decimal.ToInt32(nudMugshotCompressionQuality.Value).ToString(GlobalOptions.InvariantCultureInfo)
                        : int.MaxValue.ToString(GlobalOptions.InvariantCultureInfo));

                    //Save the Plugins-Dictionary
                    string jsonstring = Newtonsoft.Json.JsonConvert.SerializeObject(GlobalOptions.PluginsEnabledDic);
                    objRegistry.SetValue("plugins", jsonstring);

                    // Save the SourcebookInfo.
                    using (RegistryKey objSourceRegistry = objRegistry.CreateSubKey("Sourcebook"))
                    {
                        if (objSourceRegistry != null)
                        {
                            foreach (SourcebookInfo objSource in GlobalOptions.SourcebookInfo)
                                objSourceRegistry.SetValue(objSource.Code, objSource.Path + "|" + objSource.Offset);
                        }
                    }

                    // Save the Custom Data Directory Info.
                    if (_setCustomDataDirectoryInfos.Count != GlobalOptions.CustomDataDirectoryInfos.Count
                        || _setCustomDataDirectoryInfos.Any(x => !GlobalOptions.CustomDataDirectoryInfos.Contains(x)))
                    {
                        if (objRegistry.OpenSubKey("CustomDataDirectory") != null)
                            objRegistry.DeleteSubKeyTree("CustomDataDirectory");
                        using (RegistryKey objCustomDataDirectoryRegistry = objRegistry.CreateSubKey("CustomDataDirectory"))
                        {
                            if (objCustomDataDirectoryRegistry != null)
                            {
                                foreach (CustomDataDirectoryInfo objCustomDataDirectory in _setCustomDataDirectoryInfos)
                                {
                                    using (RegistryKey objLoopKey = objCustomDataDirectoryRegistry.CreateSubKey(objCustomDataDirectory.Name))
                                    {
                                        objLoopKey?.SetValue("Path", objCustomDataDirectory.Path.Replace(Utils.GetStartupPath, "$CHUMMER"));
                                    }
                                }
                            }
                        }
                    }

                    GlobalOptions.RebuildCustomDataDirectoryInfoList();
                }
            }
        }

        private void BuildBooksList()
        {
            _objCharacterOptions.Books.Clear();

            bool blnSR5Included = false;
            foreach(TreeNode objNode in treSourcebook.Nodes)
            {
                if(!objNode.Checked)
                    continue;

                _objCharacterOptions.Books.Add(objNode.Tag.ToString());

                if(objNode.Tag.ToString() == "SR5")
                    blnSR5Included = true;
            }

            // If the SR5 book was somehow missed, add it back.
            if(!blnSR5Included)
                _objCharacterOptions.Books.Add("SR5");
            _objCharacterOptions.RecalculateBookXPath();
        }

        private void BuildCustomDataDirectoryNamesList()
        {
            _objCharacterOptions.CustomDataDirectoryNames.Clear();
            for (int i = 0; i < _lstCharacterCustomDataDirectoryInfos.Count; ++i)
            {
                Tuple<object, bool> objTupleInfo = _lstCharacterCustomDataDirectoryInfos[i];
                CustomDataDirectoryInfo objCustomDataDirectory = objTupleInfo.Item1 as CustomDataDirectoryInfo;
                _objCharacterOptions.CustomDataDirectoryNames.Add(objCustomDataDirectory != null ? objCustomDataDirectory.Name : objTupleInfo.Item1.ToString(),
                    new Tuple<int, bool>(i, objTupleInfo.Item2));
            }
            _objCharacterOptions.RecalculateEnabledCustomDataDirectories();
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
            nudKarmaMysticAdeptPowerPoint.Value = 5;
        }

        private void RestoreDefaultKarmaFociValues()
        {
            nudKarmaAlchemicalFocus.Value = 3;
            nudKarmaBanishingFocus.Value = 2;
            nudKarmaBindingFocus.Value = 2;
            nudKarmaCenteringFocus.Value = 3;
            nudKarmaCounterspellingFocus.Value = 2;
            nudKarmaDisenchantingFocus.Value = 3;
            nudKarmaFlexibleSignatureFocus.Value = 3;
            nudKarmaMaskingFocus.Value = 3;
            nudKarmaPowerFocus.Value = 6;
            nudKarmaQiFocus.Value = 2;
            nudKarmaRitualSpellcastingFocus.Value = 2;
            nudKarmaSpellcastingFocus.Value = 2;
            nudKarmaSpellShapingFocus.Value = 3;
            nudKarmaSummoningFocus.Value = 2;
            nudKarmaSustainingFocus.Value = 2;
            nudKarmaWeaponFocus.Value = 3;
        }

        private void PopulateBuildMethodList()
        {
            // Populate the Build Method list.
            List<ListItem> lstBuildMethod = new List<ListItem>
            {
                new ListItem("Karma", LanguageManager.GetString("String_Karma", _strSelectedLanguage)),
                new ListItem("Priority", LanguageManager.GetString("String_Priority", _strSelectedLanguage)),
                new ListItem("SumtoTen", LanguageManager.GetString("String_SumtoTen", _strSelectedLanguage)),
            };

            if(GlobalOptions.LifeModuleEnabled)
            {
                lstBuildMethod.Add(new ListItem("LifeModule", LanguageManager.GetString("String_LifeModule", _strSelectedLanguage)));
            }

            string strOldSelected = cboBuildMethod.SelectedValue?.ToString() ?? GlobalOptions.DefaultBuildMethod;

            cboBuildMethod.BeginUpdate();
            cboBuildMethod.DataSource = null;
            cboBuildMethod.DataSource = lstBuildMethod;
            cboBuildMethod.ValueMember = nameof(ListItem.Value);
            cboBuildMethod.DisplayMember = nameof(ListItem.Name);

            if(!string.IsNullOrEmpty(strOldSelected))
            {
                cboBuildMethod.SelectedValue = strOldSelected;
                if (cboBuildMethod.SelectedIndex == -1 && lstBuildMethod.Count > 0)
                {
                    cboBuildMethod.SelectedIndex = 0;
                }
            }

            cboBuildMethod.EndUpdate();
        }

        private void PopulateDefaultGameplayOptionList()
        {
            List<ListItem> lstGameplayOptions = new List<ListItem>();

            int intIndex = 0;

            using (XmlNodeList objXmlNodeList = XmlManager.Load("gameplayoptions.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths, _strSelectedLanguage)
                .SelectNodes("/chummer/gameplayoptions/gameplayoption"))
            {
                if (objXmlNodeList != null)
                {
                    foreach (XmlNode objXmlNode in objXmlNodeList)
                    {
                        string strId = objXmlNode["id"]?.InnerText;
                        if (!string.IsNullOrEmpty(strId))
                        {
                            string strName = objXmlNode["translate"]?.InnerText ?? objXmlNode["name"]?.InnerText ?? strId;
                            lstGameplayOptions.Add(new ListItem(strId, strName));
                            if (!string.IsNullOrWhiteSpace(GlobalOptions.PDFParameters) && GlobalOptions.PDFParameters == strName)
                            {
                                intIndex = lstGameplayOptions.Count - 1;
                            }
                        }
                    }
                }
            }

            string strOldSelected = cboPDFParameters.SelectedValue?.ToString();

            cboDefaultGameplayOption.BeginUpdate();
            cboDefaultGameplayOption.DataSource = null;
            cboDefaultGameplayOption.DataSource = lstGameplayOptions;
            cboDefaultGameplayOption.ValueMember = nameof(ListItem.Value);
            cboDefaultGameplayOption.DisplayMember = nameof(ListItem.Name);

            cboDefaultGameplayOption.SelectedIndex = intIndex;

            if(!string.IsNullOrEmpty(strOldSelected))
            {
                cboDefaultGameplayOption.SelectedValue = strOldSelected;
                if(cboDefaultGameplayOption.SelectedIndex == -1 && lstGameplayOptions.Count > 0)
                    cboDefaultGameplayOption.SelectedIndex = 0;
            }

            cboDefaultGameplayOption.EndUpdate();
        }

        private void PopulateLimbCountList()
        {
            List<ListItem> lstLimbCount = new List<ListItem>();

            using (XmlNodeList objXmlNodeList = XmlManager.Load("options.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths, _strSelectedLanguage)
                .SelectNodes("/chummer/limbcounts/limb"))
            {
                if (objXmlNodeList != null)
                {
                    foreach (XmlNode objXmlNode in objXmlNodeList)
                    {
                        string strExclude = objXmlNode["exclude"]?.InnerText ?? string.Empty;
                        if (!string.IsNullOrEmpty(strExclude))
                            strExclude = '<' + strExclude;
                        lstLimbCount.Add(new ListItem(objXmlNode["limbcount"]?.InnerText + strExclude, objXmlNode["translate"]?.InnerText ?? objXmlNode["name"]?.InnerText ?? string.Empty));
                    }
                }
            }

            string strOldSelected = cboLimbCount.SelectedValue?.ToString();

            cboLimbCount.BeginUpdate();
            cboLimbCount.ValueMember = nameof(ListItem.Value);
            cboLimbCount.DisplayMember = nameof(ListItem.Name);
            cboLimbCount.DataSource = lstLimbCount;

            if(!string.IsNullOrEmpty(strOldSelected))
            {
                cboLimbCount.SelectedValue = strOldSelected;
                if(cboLimbCount.SelectedIndex == -1 && lstLimbCount.Count > 0)
                    cboLimbCount.SelectedIndex = 0;
            }

            cboLimbCount.EndUpdate();
        }

        private void PopulateMugshotCompressionOptions()
        {
            List<ListItem> lstMugshotCompressionOptions = new List<ListItem>
            {
                new ListItem(ImageFormat.Png.ToString(), LanguageManager.GetString("String_Lossless_Compression_Option")),
                new ListItem(ImageFormat.Jpeg.ToString(), LanguageManager.GetString("String_Lossy_Compression_Option"))
            };

            string strOldSelected = cboMugshotCompression.SelectedValue?.ToString();

            cboMugshotCompression.BeginUpdate();
            cboMugshotCompression.ValueMember = nameof(ListItem.Value);
            cboMugshotCompression.DisplayMember = nameof(ListItem.Name);
            cboMugshotCompression.DataSource = lstMugshotCompressionOptions;

            if (!string.IsNullOrEmpty(strOldSelected))
            {
                cboMugshotCompression.SelectedValue = strOldSelected;
                if (cboMugshotCompression.SelectedIndex == -1 && lstMugshotCompressionOptions.Count > 0)
                    cboMugshotCompression.SelectedIndex = 0;
            }

            cboMugshotCompression.EndUpdate();
        }

        private void PopulatePDFParameters()
        {
            List<ListItem> lstPdfParameters = new List<ListItem>();

            int intIndex = 0;

            using (XmlNodeList objXmlNodeList = XmlManager.Load("options.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths, _strSelectedLanguage)
                .SelectNodes("/chummer/pdfarguments/pdfargument"))
            {
                if (objXmlNodeList != null)
                {
                    foreach (XmlNode objXmlNode in objXmlNodeList)
                    {
                        string strValue = objXmlNode["value"]?.InnerText;
                        lstPdfParameters.Add(new ListItem(strValue, objXmlNode["translate"]?.InnerText ?? objXmlNode["name"]?.InnerText ?? string.Empty));
                        if (!string.IsNullOrWhiteSpace(GlobalOptions.PDFParameters) && GlobalOptions.PDFParameters == strValue)
                        {
                            intIndex = lstPdfParameters.Count - 1;
                        }
                    }
                }
            }

            string strOldSelected = cboPDFParameters.SelectedValue?.ToString();

            cboPDFParameters.BeginUpdate();
            cboPDFParameters.ValueMember = nameof(ListItem.Value);
            cboPDFParameters.DisplayMember = nameof(ListItem.Name);
            cboPDFParameters.DataSource = lstPdfParameters;
            cboPDFParameters.SelectedIndex = intIndex;

            if(!string.IsNullOrEmpty(strOldSelected))
            {
                cboPDFParameters.SelectedValue = strOldSelected;
                if(cboPDFParameters.SelectedIndex == -1 && lstPdfParameters.Count > 0)
                    cboPDFParameters.SelectedIndex = 0;
            }

            cboPDFParameters.EndUpdate();
        }

        private void PopulateApplicationInsightsOptions()
        {
            string strOldSelected = cboUseLoggingApplicationInsights.SelectedValue?.ToString() ?? GlobalOptions.UseLoggingApplicationInsights.ToString();

            List<ListItem> lstUseAIOptions = new List<ListItem>();
            foreach (var myoption in Enum.GetValues(typeof(UseAILogging)))
            {
                lstUseAIOptions.Add(new ListItem(myoption, LanguageManager.GetString("String_ApplicationInsights_" + myoption, _strSelectedLanguage)));
            }

            cboUseLoggingApplicationInsights.BeginUpdate();
            cboUseLoggingApplicationInsights.DataSource = null;
            cboUseLoggingApplicationInsights.DataSource = lstUseAIOptions;
            cboUseLoggingApplicationInsights.ValueMember = nameof(ListItem.Value);
            cboUseLoggingApplicationInsights.DisplayMember = nameof(ListItem.Name);

            if (!string.IsNullOrEmpty(strOldSelected))
                cboUseLoggingApplicationInsights.SelectedValue = Enum.Parse(typeof(UseAILogging), strOldSelected);
            if (cboUseLoggingApplicationInsights.SelectedIndex == -1 && lstUseAIOptions.Count > 0)
                cboUseLoggingApplicationInsights.SelectedIndex = 0;
            cboUseLoggingApplicationInsights.EndUpdate();
        }

        private void SetToolTips()
        {
            const int width = 100;
            chkUnarmedSkillImprovements.SetToolTip(LanguageManager.GetString("Tip_OptionsUnarmedSkillImprovements", _strSelectedLanguage).WordWrap(width));
            chkIgnoreArt.SetToolTip(LanguageManager.GetString("Tip_OptionsIgnoreArt", _strSelectedLanguage).WordWrap(width));
            chkIgnoreComplexFormLimit.SetToolTip(LanguageManager.GetString("Tip_OptionsIgnoreComplexFormLimit", _strSelectedLanguage).WordWrap(width));
            chkCyberlegMovement.SetToolTip(LanguageManager.GetString("Tip_OptionsCyberlegMovement", _strSelectedLanguage).WordWrap(width));
            chkDontDoubleQualityPurchases.SetToolTip(LanguageManager.GetString("Tip_OptionsDontDoubleQualityPurchases", _strSelectedLanguage).WordWrap(width));
            chkDontDoubleQualityRefunds.SetToolTip(LanguageManager.GetString("Tip_OptionsDontDoubleQualityRefunds", _strSelectedLanguage).WordWrap(width));
            chkStrictSkillGroups.SetToolTip(LanguageManager.GetString("Tip_OptionStrictSkillGroups", _strSelectedLanguage).WordWrap(width));
            chkAllowInitiation.SetToolTip(LanguageManager.GetString("Tip_OptionsAllowInitiation", _strSelectedLanguage).WordWrap(width));
            chkUseCalculatedPublicAwareness.SetToolTip(LanguageManager.GetString("Tip_PublicAwareness", _strSelectedLanguage).WordWrap(width));
            cboUseLoggingApplicationInsights.SetToolTip(string.Format(_objSelectedCultureInfo, LanguageManager.GetString("Tip_Options_TelemetryId", _strSelectedLanguage).WordWrap(width), Properties.Settings.Default.UploadClientId));
        }

        private void PopulateSettingsList()
        {
            List<ListItem> lstSettings = new List<ListItem>();
            string settingsDirectoryPath = Path.Combine(Utils.GetStartupPath, "settings");
            string[] settingsFilePaths = Directory.GetFiles(settingsDirectoryPath, "*.xml");

            foreach(string filePath in settingsFilePaths)
            {
                XmlDocument xmlDocument = new XmlDocument
                {
                    XmlResolver = null
                };

                try
                {
                    using (StreamReader objStreamReader = new StreamReader(filePath, Encoding.UTF8, true))
                        using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, new XmlReaderSettings {XmlResolver = null}))
                            xmlDocument.Load(objXmlReader);
                }
                catch(IOException)
                {
                    continue;
                }
                catch(XmlException)
                {
                    continue;
                }

                XmlNode node = xmlDocument.SelectSingleNode("/settings/name");
                if(node != null)
                    lstSettings.Add(new ListItem(Path.GetFileName(filePath), node.InnerText));
            }

            if(lstSettings.Count == 0)
            {
                string strFilePath = Path.Combine(settingsDirectoryPath, "default.xml");
                if(!File.Exists(strFilePath) || !_objCharacterOptions.Load("default.xml"))
                {
                    _blnDirty = true;
                    _objCharacterOptions.LoadFromRegistry();
                    _objCharacterOptions.Save();
                    XmlDocument xmlDocument = new XmlDocument
                    {
                        XmlResolver = null
                    };
                    try
                    {
                        using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                            using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, new XmlReaderSettings {XmlResolver = null}))
                                xmlDocument.Load(objXmlReader);
                    }
                    catch(IOException)
                    {
                    }
                    catch(XmlException)
                    {
                    }
                    XmlNode node = xmlDocument.SelectSingleNode("/settings/name");
                    if(node != null)
                        lstSettings.Add(new ListItem(Path.GetFileName(strFilePath), node.InnerText));
                }
            }

            string strOldSelected = cboSetting.SelectedValue?.ToString();

            cboSetting.BeginUpdate();
            cboSetting.ValueMember = nameof(ListItem.Value);
            cboSetting.DisplayMember = nameof(ListItem.Name);
            cboSetting.DataSource = lstSettings;

            if(!string.IsNullOrEmpty(strOldSelected))
                cboSetting.SelectedValue = strOldSelected;
            if(cboSetting.SelectedIndex == -1 && lstSettings.Count > 0)
                cboSetting.SelectedIndex = 0;

            cboSetting.EndUpdate();
        }

        private void PopulateLanguageList()
        {
            List<ListItem> lstLanguages = new List<ListItem>();
            string languageDirectoryPath = Path.Combine(Utils.GetStartupPath, "lang");
            string[] languageFilePaths = Directory.GetFiles(languageDirectoryPath, "*.xml");

            foreach(string filePath in languageFilePaths)
            {
                XmlDocument xmlDocument = new XmlDocument
                {
                    XmlResolver = null
                };

                try
                {
                    using (StreamReader objStreamReader = new StreamReader(filePath, Encoding.UTF8, true))
                        using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, new XmlReaderSettings {XmlResolver = null}))
                            xmlDocument.Load(objXmlReader);
                }
                catch(IOException)
                {
                    continue;
                }
                catch(XmlException)
                {
                    continue;
                }

                XmlNode node = xmlDocument.SelectSingleNode("/chummer/name");
                if(node == null)
                    continue;

                lstLanguages.Add(new ListItem(Path.GetFileNameWithoutExtension(filePath), node.InnerText));
            }

            lstLanguages.Sort(CompareListItems.CompareNames);

            cboLanguage.BeginUpdate();
            cboLanguage.ValueMember = nameof(ListItem.Value);
            cboLanguage.DisplayMember = nameof(ListItem.Name);
            cboLanguage.DataSource = lstLanguages;
            cboLanguage.EndUpdate();
        }

        private void PopulateSheetLanguageList()
        {
            cboSheetLanguage.BeginUpdate();
            cboSheetLanguage.ValueMember = nameof(ListItem.Value);
            cboSheetLanguage.DisplayMember = nameof(ListItem.Name);
            cboSheetLanguage.DataSource = GetSheetLanguageList();
            cboSheetLanguage.EndUpdate();
        }

        public static List<ListItem> GetSheetLanguageList()
        {
            HashSet<string> setLanguagesWithSheets = new HashSet<string>();

            // Populate the XSL list with all of the manifested XSL files found in the sheets\[language] directory.
            using (XmlNodeList xmlSheetLanguageList = XmlManager.Load("sheets.xml").SelectNodes("/chummer/sheets/@lang"))
            {
                if (xmlSheetLanguageList != null)
                {
                    foreach (XmlNode xmlSheetLanguage in xmlSheetLanguageList)
                    {
                        setLanguagesWithSheets.Add(xmlSheetLanguage.InnerText);
                    }
                }
            }

            List<ListItem> lstSheetLanguages = new List<ListItem>();

            string languageDirectoryPath = Path.Combine(Utils.GetStartupPath, "lang");
            string[] languageFilePaths = Directory.GetFiles(languageDirectoryPath, "*.xml");

            foreach(string filePath in languageFilePaths)
            {
                string strLanguageName = Path.GetFileNameWithoutExtension(filePath);
                if(!setLanguagesWithSheets.Contains(strLanguageName))
                    continue;

                XmlDocument xmlDocument = new XmlDocument
                {
                    XmlResolver = null
                };

                try
                {
                    using (StreamReader objStreamReader = new StreamReader(filePath, Encoding.UTF8, true))
                        using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, new XmlReaderSettings {XmlResolver = null}))
                            xmlDocument.Load(objXmlReader);
                }
                catch(IOException)
                {
                    continue;
                }
                catch(XmlException)
                {
                    continue;
                }

                XmlNode node = xmlDocument.SelectSingleNode("/chummer/name");
                if(node == null)
                    continue;

                lstSheetLanguages.Add(new ListItem(strLanguageName, node.InnerText));
            }

            lstSheetLanguages.Sort(CompareListItems.CompareNames);

            return lstSheetLanguages;
        }

        private void PopulateGlobalOptions()
        {
            chkAutomaticUpdate.Checked = GlobalOptions.AutomaticUpdate;
            chkLiveCustomData.Checked = GlobalOptions.LiveCustomData;
            chkLiveUpdateCleanCharacterFiles.Checked = GlobalOptions.LiveUpdateCleanCharacterFiles;
            chkUseLogging.Checked = GlobalOptions.UseLogging;
            cboUseLoggingApplicationInsights.Enabled = chkUseLogging.Checked;
            PopulateApplicationInsightsOptions();

            chkLifeModule.Checked = GlobalOptions.LifeModuleEnabled;
            chkOmaeEnabled.Checked = GlobalOptions.OmaeEnabled;
            chkPreferNightlyBuilds.Checked = GlobalOptions.PreferNightlyBuilds;
            chkStartupFullscreen.Checked = GlobalOptions.StartupFullscreen;
            chkSingleDiceRoller.Checked = GlobalOptions.SingleDiceRoller;
            chkDatesIncludeTime.Checked = GlobalOptions.DatesIncludeTime;
            chkDronemods.Checked = GlobalOptions.Dronemods;
            chkDronemodsMaximumPilot.Checked = GlobalOptions.DronemodsMaximumPilot;
            chkPrintToFileFirst.Checked = GlobalOptions.PrintToFileFirst;
            nudBrowserVersion.Value = GlobalOptions.EmulatedBrowserVersion;
            txtPDFAppPath.Text = GlobalOptions.PDFAppPath;
            txtCharacterRosterPath.Text = GlobalOptions.CharacterRosterPath;
            chkHideCharacterRoster.Checked = GlobalOptions.HideCharacterRoster;
            chkCreateBackupOnCareer.Checked = GlobalOptions.CreateBackupOnCareer;
            chkAllowEasterEggs.Checked = GlobalOptions.AllowEasterEggs;
            chkEnablePlugins.Checked = GlobalOptions.PluginsEnabled;
            chkCustomDateTimeFormats.Checked = GlobalOptions.CustomDateTimeFormats;
            if (!chkCustomDateTimeFormats.Checked)
            {
                txtDateFormat.Text = GlobalOptions.CultureInfo.DateTimeFormat.ShortDatePattern;
                txtTimeFormat.Text = GlobalOptions.CultureInfo.DateTimeFormat.ShortTimePattern;
            }
            else
            {
                txtDateFormat.Text = GlobalOptions.CustomDateFormat;
                txtTimeFormat.Text = GlobalOptions.CustomTimeFormat;
            }
            PluginsShowOrHide(chkEnablePlugins.Checked);
        }

        private static IList<string> ReadXslFileNamesWithoutExtensionFromDirectory(string path)
        {
            List<string> names = new List<string>();

            if (Directory.Exists(path))
            {
                foreach (string strName in Directory.GetFiles(path, "*.xsl", SearchOption.AllDirectories))
                {
                    names.Add(Path.GetFileNameWithoutExtension(strName));
                }
            }

            return names;
        }

        private IList<ListItem> GetXslFilesFromLocalDirectory(string strLanguage)
        {
            List<ListItem> lstSheets = new List<ListItem>();

            // Populate the XSL list with all of the manifested XSL files found in the sheets\[language] directory.
            using (XmlNodeList xmlSheetList = XmlManager.Load("sheets.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths, strLanguage)
                .SelectNodes($"/chummer/sheets[@lang='{strLanguage}']/sheet[not(hide)]"))
            {
                if (xmlSheetList != null)
                {
                    foreach (XmlNode xmlSheet in xmlSheetList)
                    {
                        string strFile = xmlSheet["filename"]?.InnerText ?? string.Empty;
                        lstSheets.Add(new ListItem(strLanguage != GlobalOptions.DefaultLanguage ? Path.Combine(strLanguage, strFile) : strFile, xmlSheet["name"]?.InnerText ?? string.Empty));
                    }
                }
            }

            return lstSheets;
        }

        private static IList<ListItem> GetXslFilesFromOmaeDirectory(string strLanguage)
        {
            List<ListItem> lstItems = new List<ListItem>();

            // Populate the XSLT list with all of the XSL files found in the sheets\omae directory.
            string omaeDirectoryPath = Path.Combine(Utils.GetStartupPath, "sheets", "omae");
            string menuMainOmae = LanguageManager.GetString("Menu_Main_Omae", strLanguage);

            // Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets
            // (hidden because they are partial templates that cannot be used on their own).
            foreach(string fileName in ReadXslFileNamesWithoutExtensionFromDirectory(omaeDirectoryPath))
            {
                lstItems.Add(new ListItem(Path.Combine("omae", fileName), menuMainOmae + LanguageManager.GetString("String_Colon", strLanguage) + LanguageManager.GetString("String_Space", strLanguage) + fileName));
            }

            return lstItems;
        }

        private void PopulateXsltList()
        {
            string strSelectedSheetLanguage = cboSheetLanguage.SelectedValue?.ToString();
            imgSheetLanguageFlag.Image = FlagImageGetter.GetFlagFromCountryCode(strSelectedSheetLanguage?.Substring(3, 2));

            IList<ListItem> lstFiles = GetXslFilesFromLocalDirectory(strSelectedSheetLanguage);
            if(GlobalOptions.OmaeEnabled)
            {
                foreach(ListItem objFile in GetXslFilesFromOmaeDirectory(strSelectedSheetLanguage))
                    lstFiles.Add(objFile);
            }

            string strOldSelected = cboXSLT.SelectedValue?.ToString() ?? string.Empty;
            // Strip away the language prefix
            int intPos = strOldSelected.LastIndexOf(Path.DirectorySeparatorChar);
            if(intPos != -1)
                strOldSelected = strOldSelected.Substring(intPos + 1);

            cboXSLT.BeginUpdate();
            cboXSLT.ValueMember = nameof(ListItem.Value);
            cboXSLT.DisplayMember = nameof(ListItem.Name);
            cboXSLT.DataSource = lstFiles;

            if(!string.IsNullOrEmpty(strOldSelected))
            {
                cboXSLT.SelectedValue = !string.IsNullOrEmpty(strSelectedSheetLanguage) && strSelectedSheetLanguage != GlobalOptions.DefaultLanguage ? Path.Combine(strSelectedSheetLanguage, strOldSelected) : strOldSelected;
                // If the desired sheet was not found, fall back to the Shadowrun 5 sheet.
                if(cboXSLT.SelectedIndex == -1 && lstFiles.Count > 0)
                {
                    cboXSLT.SelectedValue = !string.IsNullOrEmpty(strSelectedSheetLanguage) && strSelectedSheetLanguage != GlobalOptions.DefaultLanguage ? Path.Combine(strSelectedSheetLanguage, GlobalOptions.DefaultCharacterSheetDefaultValue) : GlobalOptions.DefaultCharacterSheetDefaultValue;
                    if(cboXSLT.SelectedIndex == -1)
                    {
                        cboXSLT.SelectedIndex = 0;
                    }
                }
            }

            cboXSLT.EndUpdate();
        }

        private void SetDefaultValueForSettingsList()
        {
            // Attempt to make default.xml the default one. If it could not be found in the list, select the first item instead.
            cboSetting.SelectedIndex = cboSetting.FindStringExact("Default Settings");

            if(cboSetting.SelectedIndex == -1 && cboSetting.Items.Count > 0)
                cboSetting.SelectedIndex = 0;
        }

        private void SetDefaultValueForLanguageList()
        {
            cboLanguage.SelectedValue = GlobalOptions.Language;

            if(cboLanguage.SelectedIndex == -1)
                cboLanguage.SelectedValue = GlobalOptions.DefaultLanguage;
        }

        private void SetDefaultValueForSheetLanguageList()
        {
            string strDefaultCharacterSheet = GlobalOptions.DefaultCharacterSheet;
            if(string.IsNullOrEmpty(strDefaultCharacterSheet) || strDefaultCharacterSheet == "Shadowrun (Rating greater 0)")
                strDefaultCharacterSheet = GlobalOptions.DefaultCharacterSheetDefaultValue;

            string strDefaultSheetLanguage = GlobalOptions.Language;
            int intLastIndexDirectorySeparator = strDefaultCharacterSheet.LastIndexOf(Path.DirectorySeparatorChar);
            if(intLastIndexDirectorySeparator != -1)
            {
                string strSheetLanguage = strDefaultCharacterSheet.Substring(0, intLastIndexDirectorySeparator);
                if(strSheetLanguage.Length == 5)
                    strDefaultSheetLanguage = strSheetLanguage;
            }

            cboSheetLanguage.SelectedValue = strDefaultSheetLanguage;

            if(cboSheetLanguage.SelectedIndex == -1)
                cboSheetLanguage.SelectedValue = GlobalOptions.DefaultLanguage;
        }

        private void SetDefaultValueForXsltList()
        {
            if(string.IsNullOrEmpty(GlobalOptions.DefaultCharacterSheet))
                GlobalOptions.DefaultCharacterSheet = GlobalOptions.DefaultCharacterSheetDefaultValue;

            cboXSLT.SelectedValue = GlobalOptions.DefaultCharacterSheet;
            if(cboXSLT.SelectedValue == null && cboXSLT.Items.Count > 0)
            {
                int intNameIndex;
                string strLanguage = _strSelectedLanguage;
                if(string.IsNullOrEmpty(strLanguage) || strLanguage == GlobalOptions.DefaultLanguage)
                    intNameIndex = cboXSLT.FindStringExact(GlobalOptions.DefaultCharacterSheet);
                else
                    intNameIndex = cboXSLT.FindStringExact(GlobalOptions.DefaultCharacterSheet.Substring(GlobalOptions.DefaultLanguage.LastIndexOf(Path.DirectorySeparatorChar) + 1));
                cboXSLT.SelectedIndex = Math.Max(0, intNameIndex);
            }
        }

        private void UpdateSourcebookInfoPath(string strPath)
        {
            string strTag = lstGlobalSourcebookInfos.SelectedValue?.ToString();
            SourcebookInfo objFoundSource = GlobalOptions.SourcebookInfo.FirstOrDefault(x => x.Code == strTag);

            if(objFoundSource != null)
            {
                objFoundSource.Path = strPath;
            }
            else
            {
                // If the Sourcebook was not found in the options, add it.
                GlobalOptions.SourcebookInfo.Add(new SourcebookInfo
                {
                    Code = strTag,
                    Path = strPath
                });
            }
        }

        private void cmdUploadPastebin_Click(object sender, EventArgs e)
        {
#if DEBUG
            string strFilePath = "Insert local file here";
            System.Collections.Specialized.NameValueCollection data = new System.Collections.Specialized.NameValueCollection();
            string line;
            using(StreamReader sr = new StreamReader(strFilePath, Encoding.UTF8, true))
            {
                line = sr.ReadToEnd();
            }
            data["api_paste_name"] = "Chummer";
            data["api_paste_expire_date"] = "N";
            data["api_paste_format"] = "xml";
            data["api_paste_code"] = line;
            data["api_dev_key"] = "7845fd372a1050899f522f2d6bab9666";
            data["api_option"] = "paste";

            using (WebClient wb = new WebClient())
            {
                byte[] bytes;
                try
                {
                    bytes = wb.UploadValues("http://pastebin.com/api/api_post.php", data);
                }
                catch (WebException)
                {
                    return;
                }

                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (StreamReader reader = new StreamReader(ms, Encoding.UTF8, true))
                    {
                        string response = reader.ReadToEnd();
                        Clipboard.SetText(response);
                    }
                }
            }
#endif
        }
        #endregion

        private void OptionsChanged(object sender, EventArgs e)
        {
            if(!_blnLoading)
            {
                _blnDirty = true;
            }
        }

        private void chkLifeModules_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkLifeModule.Checked || _blnLoading) return;
            if(MessageBox.Show(LanguageManager.GetString("Tip_LifeModule_Warning", _strSelectedLanguage), Application.ProductName,
                   MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                chkLifeModule.Checked = false;
            else
            {
                OptionsChanged(sender, e);
            }
        }

        private void chkOmaeEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkOmaeEnabled.Checked || _blnLoading) return;
            if(MessageBox.Show(LanguageManager.GetString("Tip_Omae_Warning", _strSelectedLanguage), Application.ProductName,
                   MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                chkOmaeEnabled.Checked = false;
            else
            {
                OptionsChanged(sender, e);
            }
        }

        private void cmdEnableSourcebooks_Click(object sender, EventArgs e)
        {
            foreach(TreeNode objNode in treSourcebook.Nodes)
            {
                if(objNode.Tag.ToString() != "SR5")
                {
                    objNode.Checked = _blnSourcebookToggle;
                }
            }
            _blnSourcebookToggle = !_blnSourcebookToggle;
        }

        private void cmdCharacterRoster_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (FolderBrowserDialog dlgSelectFolder = new FolderBrowserDialog())
                if (dlgSelectFolder.ShowDialog(this) == DialogResult.OK)
                    txtCharacterRosterPath.Text = dlgSelectFolder.SelectedPath;
        }

        private void cmdAddCustomDirectory_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (FolderBrowserDialog dlgSelectFolder = new FolderBrowserDialog {SelectedPath = Utils.GetStartupPath})
            {
                if (dlgSelectFolder.ShowDialog(this) != DialogResult.OK)
                    return;
                using (frmSelectText frmSelectCustomDirectoryName = new frmSelectText
                {
                    Description = LanguageManager.GetString("String_CustomItem_SelectText", _strSelectedLanguage)
                })
                {
                    if (frmSelectCustomDirectoryName.ShowDialog(this) != DialogResult.OK)
                        return;
                    CustomDataDirectoryInfo objNewCustomDataDirectory = new CustomDataDirectoryInfo(frmSelectCustomDirectoryName.SelectedValue, dlgSelectFolder.SelectedPath);

                    if (_setCustomDataDirectoryInfos.Any(x => x.Name == objNewCustomDataDirectory.Name))
                    {
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Duplicate_CustomDataDirectoryName", _strSelectedLanguage),
                            LanguageManager.GetString("Message_Duplicate_CustomDataDirectoryName_Title", _strSelectedLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        _setCustomDataDirectoryInfos.Add(objNewCustomDataDirectory);
                        PopulateCustomDataDirectoryTreeView();
                    }
                }
            }
        }

        private void cmdRemoveCustomDirectory_Click(object sender, EventArgs e)
        {
            TreeNode nodSelectedCustomDataDirectory = treCustomDataDirectories.SelectedNode;
            CustomDataDirectoryInfo objInfoToRemove = nodSelectedCustomDataDirectory?.Tag as CustomDataDirectoryInfo;
            if (objInfoToRemove == null || !_setCustomDataDirectoryInfos.Contains(objInfoToRemove))
                return;
            if(nodSelectedCustomDataDirectory.Checked)
                OptionsChanged(sender, e);
            _setCustomDataDirectoryInfos.Remove(objInfoToRemove);
            PopulateCustomDataDirectoryTreeView();
        }

        private void cmdRenameCustomDataDirectory_Click(object sender, EventArgs e)
        {
            TreeNode nodSelectedInfo = treCustomDataDirectories.SelectedNode;
            if (nodSelectedInfo == null)
                return;
            CustomDataDirectoryInfo objInfoToRename = nodSelectedInfo.Tag as CustomDataDirectoryInfo;
            if (objInfoToRename == null)
                return;
            using (frmSelectText frmSelectCustomDirectoryName = new frmSelectText
            {
                Description = LanguageManager.GetString("String_CustomItem_SelectText", _strSelectedLanguage)
            })
            {
                if (frmSelectCustomDirectoryName.ShowDialog(this) != DialogResult.OK)
                    return;
                if (_setCustomDataDirectoryInfos.Any(x => x.Name == frmSelectCustomDirectoryName.Name))
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Duplicate_CustomDataDirectoryName", _strSelectedLanguage),
                        LanguageManager.GetString("Message_Duplicate_CustomDataDirectoryName_Title", _strSelectedLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    CustomDataDirectoryInfo objNewInfo = new CustomDataDirectoryInfo(frmSelectCustomDirectoryName.SelectedValue, objInfoToRename.Path);
                    _setCustomDataDirectoryInfos.Remove(objInfoToRename);
                    _setCustomDataDirectoryInfos.Add(objNewInfo);
                    int intItemIndex =  _lstCharacterCustomDataDirectoryInfos.FindIndex(x => objInfoToRename.Equals(x.Item1));
                    if (intItemIndex >= 0)
                    {
                        bool blnEnabled = _lstCharacterCustomDataDirectoryInfos[intItemIndex].Item2;
                        _lstCharacterCustomDataDirectoryInfos.RemoveAt(intItemIndex);
                        _lstCharacterCustomDataDirectoryInfos.Insert(intItemIndex, new Tuple<object, bool>(objNewInfo, blnEnabled));
                    }
                    PopulateCustomDataDirectoryTreeView();
                }
            }
        }

        private void cmdIncreaseCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = treCustomDataDirectories.SelectedNode;
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex <= 0)
                return;
            bool blnOptionsChanged = nodSelected.Checked || (nodSelected.Parent?.Nodes[intIndex - 1].Checked ?? treCustomDataDirectories.Nodes[intIndex - 1].Checked);
            _lstCharacterCustomDataDirectoryInfos.Reverse(intIndex - 1, 2);
            PopulateCustomDataDirectoryTreeView();
            if(blnOptionsChanged)
                OptionsChanged(sender, e);
        }

        private void cmdDecreaseCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = treCustomDataDirectories.SelectedNode;
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex >= _lstCharacterCustomDataDirectoryInfos.Count)
                return;
            bool blnOptionsChanged = nodSelected.Checked || (nodSelected.Parent?.Nodes[intIndex + 1].Checked ?? treCustomDataDirectories.Nodes[intIndex + 1].Checked);
            _lstCharacterCustomDataDirectoryInfos.Reverse(intIndex, 2);
            PopulateCustomDataDirectoryTreeView();
            if (blnOptionsChanged)
                OptionsChanged(sender, e);
        }

        private void nudNuyenDecimalsMaximum_ValueChanged(object sender, EventArgs e)
        {
            if(nudNuyenDecimalsMinimum.Value > nudNuyenDecimalsMaximum.Value)
                nudNuyenDecimalsMinimum.Value = nudNuyenDecimalsMaximum.Value;
            OptionsChanged(sender, e);
        }

        private void nudNuyenDecimalsMinimum_ValueChanged(object sender, EventArgs e)
        {
            if(nudNuyenDecimalsMaximum.Value < nudNuyenDecimalsMinimum.Value)
                nudNuyenDecimalsMaximum.Value = nudNuyenDecimalsMinimum.Value;
            OptionsChanged(sender, e);
        }

        private void chkPrintFreeExpenses_CheckedChanged(object sender, EventArgs e)
        {
            chkPrintFreeExpenses.Enabled = chkPrintExpenses.Checked;
            if(!chkPrintFreeExpenses.Enabled)
                chkPrintFreeExpenses.Checked = true;
            OptionsChanged(sender, e);
        }

        private void treCustomDataDirectories_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TreeNode objNode = e.Node;
            if (objNode == null)
                return;
            OptionsChanged(sender, e);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }


        private void chkEnablePlugins_CheckedChanged(object sender, EventArgs e)
        {
            PluginsShowOrHide(chkEnablePlugins.Checked);
            OptionsChanged(sender, e);
        }

        private void PluginsShowOrHide(bool show)
        {
            if (show)
            {
                if (!tabOptions.TabPages.Contains(tabPlugins))
                    tabOptions.TabPages.Add(tabPlugins);
            }
            else
            {
                if (tabOptions.TabPages.Contains(tabPlugins))
                    tabOptions.TabPages.Remove(tabPlugins);
            }
        }

        private void clbPlugins_VisibleChanged(object sender, EventArgs e)
        {
            clbPlugins.Items.Clear();
            if (Program.PluginLoader.MyPlugins.Count == 0) return;
            using (new CursorWait(false, this))
            {
                foreach (var plugin in Program.PluginLoader.MyPlugins)
                {
                    try
                    {
                        plugin.CustomInitialize(Program.MainForm);
                        if (GlobalOptions.PluginsEnabledDic.TryGetValue(plugin.ToString(), out var check))
                        {
                            clbPlugins.Items.Add(plugin, check);
                        }
                        else
                        {
                            clbPlugins.Items.Add(plugin);
                        }
                    }
                    catch (ApplicationException ae)
                    {
                        Log.Debug(ae);
                    }
                }

                if (clbPlugins.Items.Count > 0)
                {
                    clbPlugins.SelectedIndex = 0;
                }
            }
        }

        private void clbPlugins_SelectedValueChanged(object sender, EventArgs e)
        {
            UserControl pluginControl = (clbPlugins.SelectedItem as Plugins.IPlugin)?.GetOptionsControl();
            if (pluginControl != null)
            {
                panelPluginOption.Controls.Clear();
                panelPluginOption.Controls.Add(pluginControl);
            }
        }

        private void clbPlugins_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            using (new CursorWait(false, this))
            {
                var plugin = clbPlugins.Items[e.Index];
                if (GlobalOptions.PluginsEnabledDic.ContainsKey(plugin.ToString()))
                    GlobalOptions.PluginsEnabledDic.Remove(plugin.ToString());
                GlobalOptions.PluginsEnabledDic.Add(plugin.ToString(), e.NewValue == CheckState.Checked);
                OptionsChanged(sender, e);
            }

        }

        private void cboUseLoggingApplicationInsights_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            UseAILogging useAI = (UseAILogging) ((ListItem) cboUseLoggingApplicationInsights.SelectedItem).Value;
            if (useAI > UseAILogging.Info && GlobalOptions.UseLoggingApplicationInsights <= UseAILogging.Info)
            {
                if (DialogResult.Yes != Program.MainForm.ShowMessageBox(
                    LanguageManager.GetString("Message_Options_ConfirmTelemetry", _strSelectedLanguage).WordWrap(256),
                    LanguageManager.GetString("MessageTitle_Options_ConfirmTelemetry", _strSelectedLanguage),
                    MessageBoxButtons.YesNo))
                {
                    _blnLoading = true;
                    cboUseLoggingApplicationInsights.SelectedItem = UseAILogging.Info;
                    _blnLoading = false;
                    return;
                }
            }
            OptionsChanged(sender, e);
        }

        private void chkUseLogging_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (chkUseLogging.Checked && !GlobalOptions.UseLogging)
            {
                if (DialogResult.Yes != Program.MainForm.ShowMessageBox(
                                            LanguageManager.GetString("Message_Options_ConfirmDetailedTelemetry", _strSelectedLanguage).WordWrap(256),
                                            LanguageManager.GetString("MessageTitle_Options_ConfirmDetailedTelemetry", _strSelectedLanguage),
                                            MessageBoxButtons.YesNo))
                {
                    _blnLoading = true;
                    chkUseLogging.Checked = false;
                    _blnLoading = false;
                    return;
                }
            }
            cboUseLoggingApplicationInsights.Enabled = chkUseLogging.Checked;
            OptionsChanged(sender, e);
        }

        private void cboUseLoggingHelp_Click(object sender, EventArgs e)
        {
            //open the telemetry document
            System.Diagnostics.Process.Start("https://docs.google.com/document/d/1LThAg6U5qXzHAfIRrH0Kb7griHrPN0hy7ab8FSJDoFY/edit?usp=sharing");
        }

        private void cmdPluginsHelp_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://docs.google.com/document/d/1WOPB7XJGgcmxg7REWxF6HdP3kQdtHpv6LJOXZtLggxM/edit?usp=sharing");
        }

        private void chkCyberlimbAttributeBonusCap_CheckedChanged(object sender, EventArgs e)
        {
            nudCyberlimbAttributeBonusCap.Enabled = chkCyberlimbAttributeBonusCap.Checked;
            if (!chkCyberlimbAttributeBonusCap.Checked)
            {
                nudCyberlimbAttributeBonusCap.Value = 4;
                nudCyberlimbAttributeBonusCap.Enabled = false;
            }
        }

        private void chkCustomDateTimeFormats_CheckedChanged(object sender, EventArgs e)
        {
            grpDateFormat.Enabled = chkCustomDateTimeFormats.Checked;
            grpTimeFormat.Enabled = chkCustomDateTimeFormats.Checked;
            if (!chkCustomDateTimeFormats.Checked)
            {
                txtDateFormat.Text = GlobalOptions.CultureInfo.DateTimeFormat.ShortDatePattern;
                txtTimeFormat.Text = GlobalOptions.CultureInfo.DateTimeFormat.ShortTimePattern;
            }
            OptionsChanged(sender, e);
        }

        private void txtDateFormat_TextChanged(object sender, EventArgs e)
        {
            try
            {
                txtDateFormatView.Text = DateTime.Now.ToString(txtDateFormat.Text, _objSelectedCultureInfo);
            }
            catch (Exception)
            {
                txtDateFormatView.Text = LanguageManager.GetString("String_Error", _strSelectedLanguage);
            }
            OptionsChanged(sender, e);
        }

        private void txtTimeFormat_TextChanged(object sender, EventArgs e)
        {
            try
            {
                txtTimeFormatView.Text = DateTime.Now.ToString(txtTimeFormat.Text, _objSelectedCultureInfo);
            }
            catch (Exception)
            {
                txtTimeFormatView.Text = LanguageManager.GetString("String_Error", _strSelectedLanguage);
            }
            OptionsChanged(sender, e);
        }

        private void cboMugshotCompression_SelectedIndexChanged(object sender, EventArgs e)
        {
            nudMugshotCompressionQuality.Enabled = string.Equals(cboMugshotCompression.SelectedValue.ToString(), ImageFormat.Jpeg.ToString(), StringComparison.Ordinal);
            OptionsChanged(sender, e);
        }
    }
}
