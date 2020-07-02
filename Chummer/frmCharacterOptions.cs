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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Text;
using Microsoft.Win32;
using NLog;

namespace Chummer
{
    public partial class frmCharacterOptions : Form
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly CharacterOptions _objCharacterOptions;
        // List of custom data directories on the character, in load order. If the character has a directory name for which we have no info, Item1 will be null
        private readonly List<Tuple<object, bool>> _lstCharacterCustomDataDirectoryInfos = new List<Tuple<object, bool>>();
        private readonly HashSet<CustomDataDirectoryInfo> _setCustomDataDirectoryInfos;
        private bool _blnDirty;
        private bool _blnLoading = true;
        private bool _blnSourcebookToggle = true;

        #region Form Events
        public frmCharacterOptions(CharacterOptions objExistingOptions = null)
        {
            InitializeComponent();
#if !DEBUG
            // tabPage3 only contains cmdUploadPastebin, which is not used if DEBUG is not enabled
            // Remove this line if cmdUploadPastebin_Click has some functionality if DEBUG is not enabled or if tabPage3 gets some other control that can be used if DEBUG is not enabled
            tabOptions.TabPages.Remove(tabGitHubIssues);
#endif
            this.TranslateWinForm();
            _objCharacterOptions = objExistingOptions ?? OptionsManager.;

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

        private void frmCharacterOptions_Load(object sender, EventArgs e)
        {
            PopulateLimbCountList();
            SetToolTips();
            PopulateSettingsList();
            SetDefaultValueForSettingsList();
            _blnLoading = false;
        }
        #endregion

        #region Control Events
        private void cmdOK_Click(object sender, EventArgs e)
        {
            // Make sure the current Setting has a name.
            if(string.IsNullOrWhiteSpace(txtSettingName.Text))
            {
                string text = LanguageManager.GetString("Message_Options_SettingsName");
                string caption = LanguageManager.GetString("MessageTitle_Options_SettingsName");

                Program.MainForm.ShowMessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSettingName.Focus();
                return;
            }

            if(_blnDirty)
            {
                string text = LanguageManager.GetString("Message_Options_SaveForms");
                string caption = LanguageManager.GetString("MessageTitle_Options_CloseForms");

                if(MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }

            DialogResult = DialogResult.OK;

            BuildBooksList();
            BuildCustomDataDirectoryNamesList();
            SaveRegistrySettings();

            _objCharacterOptions.AllowCyberwareESSDiscounts = chkAllowCyberwareESSDiscounts.Checked;
            _objCharacterOptions.AllowInitiationInCreateMode = chkAllowInitiation.Checked;
            _objCharacterOptions.DontUseCyberlimbCalculation = chkDontUseCyberlimbCalculation.Checked;
            _objCharacterOptions.AllowSkillRegrouping = chkAllowSkillRegrouping.Checked;
            _objCharacterOptions.CyberlegMovement = chkCyberlegMovement.Checked;
            _objCharacterOptions.DontDoubleQualityPurchases = chkDontDoubleQualityPurchases.Checked;
            _objCharacterOptions.DontDoubleQualityRefunds = chkDontDoubleQualityRefunds.Checked;
            _objCharacterOptions.EnforceCapacity = chkEnforceCapacity.Checked;
            try
            {
                _objCharacterOptions.EssenceDecimals = decimal.ToInt32(nudEssenceDecimals.Value);
                _objCharacterOptions.DroneArmorMultiplier = decimal.ToInt32(nudDroneArmorMultiplier.Value);
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

            _objCharacterOptions.ContactPointsExpression = txtContactPoints.Text;
            _objCharacterOptions.KnowledgePointsExpression = txtKnowledgePoints.Text;

            _objCharacterOptions.DroneArmorMultiplierEnabled = chkDroneArmorMultiplier.Checked;
            nudDroneArmorMultiplier.Enabled = chkDroneArmorMultiplier.Checked;

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
            _objCharacterOptions.DroneMods = chkDronemods.Checked;
            _objCharacterOptions.DroneModsMaximumPilot = chkDronemodsMaximumPilot.Checked;

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
        }

        private void cboSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedFile = cboSetting.SelectedValue?.ToString();
            if(strSelectedFile?.Contains(".xml") != true)
                return;

            _objCharacterOptions.Load(strSelectedFile);
            PopulateOptions();
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
            string text = LanguageManager.GetString("Message_Options_RestoreDefaults");
            string caption = LanguageManager.GetString("MessageTitle_Options_RestoreDefaults");

            // Verify that the user wants to reset these values.
            if(MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            RestoreDefaultKarmaValues();
            RestoreDefaultKarmaFociValues();
        }
        #endregion

        #region Methods

        private void PopulateSourcebookTreeView()
        {
            // Load the Sourcebook information.
            XmlDocument objXmlDocument = XmlManager.Load("books.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths);

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
            PopulateSourcebookTreeView();
            PopulateCustomDataDirectoryTreeView();

            nudEssenceDecimals.Value = _objCharacterOptions.EssenceDecimals == 0 ? 2 : _objCharacterOptions.EssenceDecimals;
            chkDontRoundEssenceInternally.Checked = _objCharacterOptions.DontRoundEssenceInternally;
            chkAllowCyberwareESSDiscounts.Checked = _objCharacterOptions.AllowCyberwareESSDiscounts;
            chkAllowInitiation.Checked = _objCharacterOptions.AllowInitiationInCreateMode;
            chkDontUseCyberlimbCalculation.Checked = _objCharacterOptions.DontUseCyberlimbCalculation;
            chkAllowSkillRegrouping.Checked = _objCharacterOptions.AllowSkillRegrouping;
            txtContactPoints.Text = _objCharacterOptions.ContactPointsExpression;
            txtKnowledgePoints.Text = _objCharacterOptions.KnowledgePointsExpression;
            chkDroneArmorMultiplier.Checked = _objCharacterOptions.DroneArmorMultiplierEnabled;
            chkCyberlegMovement.Checked = _objCharacterOptions.CyberlegMovement;
            chkMysAdPp.Checked = _objCharacterOptions.MysAdeptAllowPPCareer;
            chkMysAdeptSecondMAGAttribute.Checked = _objCharacterOptions.MysAdeptSecondMAGAttribute;
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
            chkDronemods.Checked = _objCharacterOptions.DroneMods;
            chkDronemodsMaximumPilot.Checked = _objCharacterOptions.DroneModsMaximumPilot;
            chkIncreasedImprovedAbilityModifier.Checked = _objCharacterOptions.IncreasedImprovedAbilityMultiplier;
            chkAllowFreeGrids.Checked = _objCharacterOptions.AllowFreeGrids;
            nudDroneArmorMultiplier.Enabled = _objCharacterOptions.DroneArmorMultiplierEnabled;
            nudDroneArmorMultiplier.Value = _objCharacterOptions.DroneArmorMultiplier;
            nudMetatypeCostsKarmaMultiplier.Value = _objCharacterOptions.MetatypeCostsKarmaMultiplier;
            nudNuyenPerBP.Value = _objCharacterOptions.NuyenPerBP;
            chkUsePointsOnBrokenGroups.Checked = _objCharacterOptions.UsePointsOnBrokenGroups;

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

        /// <summary>
        /// Save the global settings to the registry.
        /// </summary>
        private void SaveRegistrySettings()
        {
            using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Chummer5"))
            {
                if (objRegistry != null)
                {
                    objRegistry.SetValue("dronemods", chkDronemods.Checked);
                    objRegistry.SetValue("dronemodsPilot", chkDronemodsMaximumPilot.Checked);

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

        private void PopulateLimbCountList()
        {
            List<ListItem> lstLimbCount = new List<ListItem>();

            using (XmlNodeList objXmlNodeList = XmlManager.Load("options.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths)
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

        private void SetToolTips()
        {
            const int width = 100;
            chkUnarmedSkillImprovements.SetToolTip(LanguageManager.GetString("Tip_OptionsUnarmedSkillImprovements").WordWrap(width));
            chkIgnoreArt.SetToolTip(LanguageManager.GetString("Tip_OptionsIgnoreArt").WordWrap(width));
            chkIgnoreComplexFormLimit.SetToolTip(LanguageManager.GetString("Tip_OptionsIgnoreComplexFormLimit").WordWrap(width));
            chkCyberlegMovement.SetToolTip(LanguageManager.GetString("Tip_OptionsCyberlegMovement").WordWrap(width));
            chkDontDoubleQualityPurchases.SetToolTip(LanguageManager.GetString("Tip_OptionsDontDoubleQualityPurchases").WordWrap(width));
            chkDontDoubleQualityRefunds.SetToolTip(LanguageManager.GetString("Tip_OptionsDontDoubleQualityRefunds").WordWrap(width));
            chkStrictSkillGroups.SetToolTip(LanguageManager.GetString("Tip_OptionStrictSkillGroups").WordWrap(width));
            chkAllowInitiation.SetToolTip(LanguageManager.GetString("Tip_OptionsAllowInitiation").WordWrap(width));
            chkUseCalculatedPublicAwareness.SetToolTip(LanguageManager.GetString("Tip_PublicAwareness").WordWrap(width));
        }

        private void PopulateSettingsList()
        {
            List<ListItem> lstSettings = new List<ListItem>();
            string settingsDirectoryPath = Path.Combine(Utils.GetStartupPath, "settings");

            foreach (KeyValuePair<string, CharacterOptions> kvpCharacterOptionsEntry in OptionsManager.LoadedCharacterOptions)
            {
                lstSettings.Add(new ListItem(kvpCharacterOptionsEntry.Key, kvpCharacterOptionsEntry.Value.Name));
            }

            if(lstSettings.Count == 0)
            {
                string strFilePath = Path.Combine(settingsDirectoryPath, "standard.xml");
                if(!File.Exists(strFilePath) || !_objCharacterOptions.Load("standard.xml"))
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
            cboSetting.DataSource = null;
            cboSetting.ValueMember = nameof(ListItem.Value);
            cboSetting.DisplayMember = nameof(ListItem.Name);
            cboSetting.DataSource = lstSettings;
            if(!string.IsNullOrEmpty(strOldSelected))
                cboSetting.SelectedValue = strOldSelected;
            if(cboSetting.SelectedIndex == -1 && lstSettings.Count > 0)
                cboSetting.SelectedIndex = 0;
            cboSetting.EndUpdate();
        }

        private void SetDefaultValueForSettingsList()
        {
            // Attempt to make default.xml the default one. If it could not be found in the list, select the first item instead.
            cboSetting.SelectedIndex = cboSetting.FindStringExact("Default Settings");

            if(cboSetting.SelectedIndex == -1 && cboSetting.Items.Count > 0)
                cboSetting.SelectedIndex = 0;
        }
        #endregion

        private void OptionsChanged(object sender, EventArgs e)
        {
            if(!_blnLoading)
            {
                _blnDirty = true;
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

        private void chkCyberlimbAttributeBonusCap_CheckedChanged(object sender, EventArgs e)
        {
            nudCyberlimbAttributeBonusCap.Enabled = chkCyberlimbAttributeBonusCap.Checked;
            if (!chkCyberlimbAttributeBonusCap.Checked)
            {
                nudCyberlimbAttributeBonusCap.Value = 4;
                nudCyberlimbAttributeBonusCap.Enabled = false;
            }
        }
    }
}
