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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;
using NLog;

namespace Chummer
{
    public partial class frmSelectCyberware : Form
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private readonly Character _objCharacter;
        private readonly List<Grade> _lstGrades;
        private readonly string _strNoneGradeId;

        private decimal _decCostMultiplier = 1.0m;
        private decimal _decESSMultiplier = 1.0m;
        private int _intAvailModifier;

        private Grade _objForcedGrade;
        private string _strSubsystems = string.Empty;
        private string _strDisallowedMounts = string.Empty;
        private string _strHasModularMounts = string.Empty;
        private decimal _decMaximumCapacity = -1;
        private bool _blnLockGrade;
        private bool _blnLoading = true;

        private readonly Mode _objMode = Mode.Cyberware;
        private readonly string _strNodeXPath = "cyberwares/cyberware";
        private static string _sStrSelectCategory = string.Empty;
        private static string _sStrSelectGrade = string.Empty;
        private string _strSelectedCategory = string.Empty;
        private string _strOldSelectedGrade = string.Empty;
        private bool _blnOldGradeEnabled = true;
        private bool _blnIgnoreSecondHand;
        private string _strForceGrade = string.Empty;
        private readonly object _objParentObject;
        private readonly XPathNavigator _objParentNode;
        private readonly HashSet<string> _setBlackMarketMaps;
        private readonly XPathNavigator _xmlBaseCyberwareDataNode;

        private enum Mode
        {
            Cyberware = 0,
            Bioware,
        }

        #region Control Events

        public frmSelectCyberware(Character objCharacter, Improvement.ImprovementSource objWareSource, object objParentNode = null)
        {
            InitializeComponent();

            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objParentObject = objParentNode;
            _objParentNode = (_objParentObject as IHasXmlNode)?.GetNode()?.CreateNavigator();

            switch (objWareSource)
            {
                case Improvement.ImprovementSource.Cyberware:
                    _objMode = Mode.Cyberware;
                    _xmlBaseCyberwareDataNode = objCharacter.LoadDataXPath("cyberware.xml").SelectSingleNodeAndCacheExpression("/chummer");
                    _strNodeXPath = "cyberwares/cyberware";
                    Tag = "Title_SelectCyberware";
                    break;

                case Improvement.ImprovementSource.Bioware:
                    _objMode = Mode.Bioware;
                    _xmlBaseCyberwareDataNode = objCharacter.LoadDataXPath("bioware.xml").SelectSingleNodeAndCacheExpression("/chummer");
                    _strNodeXPath = "biowares/bioware";
                    Tag = "Title_SelectCyberware_Bioware";
                    break;
            }

            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            _lstGrades = _objCharacter.GetGradeList(objWareSource).ToList();
            _strNoneGradeId = _lstGrades.Find(x => x.Name == "None")?.SourceIDString;
            _setBlackMarketMaps = _objCharacter.GenerateBlackMarketMappings(_xmlBaseCyberwareDataNode);
        }

        private void frmSelectCyberware_Load(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                lblMarkupLabel.Visible = true;
                nudMarkup.Visible = true;
                lblMarkupPercentLabel.Visible = true;
                chkHideBannedGrades.Visible = false;
                chkHideOverAvailLimit.Visible = false;
                chkHideOverAvailLimit.Checked = false;
            }
            else
            {
                lblMarkupLabel.Visible = false;
                nudMarkup.Visible = false;
                lblMarkupPercentLabel.Visible = false;
                chkHideBannedGrades.Visible = !_objCharacter.IgnoreRules;
                chkHideOverAvailLimit.Text = string.Format(GlobalSettings.CultureInfo, chkHideOverAvailLimit.Text, _objCharacter.Settings.MaximumAvailability);
                chkHideOverAvailLimit.Checked = GlobalSettings.HideItemsOverAvailLimit;
            }

            if (!string.IsNullOrEmpty(DefaultSearchText))
            {
                txtSearch.Text = DefaultSearchText;
                txtSearch.Enabled = false;
            }

            chkPrototypeTranshuman.Visible = _objCharacter.IsPrototypeTranshuman && _objMode == Mode.Bioware && !_objCharacter.Created;

            PopulateCategories();
            // Select the first Category in the list.
            if (!string.IsNullOrEmpty(_sStrSelectCategory))
                cboCategory.SelectedValue = _sStrSelectCategory;
            if (cboCategory.SelectedIndex == -1 && cboCategory.Items.Count > 0)
                cboCategory.SelectedIndex = 0;
            _strSelectedCategory = cboCategory.SelectedValue?.ToString();

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            // Populate the Grade list. Do not show the Adapsin Grades if Adapsin is not enabled for the character.
            PopulateGrades(false, true, _objForcedGrade?.SourceIDString ?? string.Empty, chkHideBannedGrades.Checked);

            if (_objForcedGrade != null)
                cboGrade.SelectedValue = _objForcedGrade.SourceIDString;
            else if (!string.IsNullOrEmpty(_sStrSelectGrade))
                cboGrade.SelectedValue = _sStrSelectGrade;
            if (cboGrade.SelectedIndex == -1 && cboGrade.Items.Count > 0)
                cboGrade.SelectedIndex = 0;

            // Retrieve the information for the selected Grade.
            string strSelectedGrade = cboGrade.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedGrade))
            {
                XPathNavigator xmlGrade = _xmlBaseCyberwareDataNode.SelectSingleNode("grades/grade[id = " + strSelectedGrade.CleanXPath() + "]");

                // Update the Essence and Cost multipliers based on the Grade that has been selected.
                if (xmlGrade != null)
                {
                    _decCostMultiplier = Convert.ToDecimal(xmlGrade.SelectSingleNode("cost")?.Value, GlobalSettings.InvariantCultureInfo);
                    _decESSMultiplier = Convert.ToDecimal(xmlGrade.SelectSingleNode("ess")?.Value, GlobalSettings.InvariantCultureInfo);
                    _intAvailModifier = xmlGrade.SelectSingleNode("avail")?.ValueAsInt ?? 0;
                }
            }

            lblESSDiscountLabel.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts;
            lblESSDiscountPercentLabel.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts;
            nudESSDiscount.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts;

            _blnLoading = false;
            RefreshList(_strSelectedCategory);
        }

        private void cboGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _blnLoading = true;

            XPathNavigator xmlGrade = null;
            // Retrieve the information for the selected Grade.
            string strSelectedGrade = cboGrade.SelectedValue?.ToString();
            if (cboGrade.Enabled && strSelectedGrade != null)
                _strOldSelectedGrade = strSelectedGrade;
            if (!string.IsNullOrEmpty(strSelectedGrade))
                xmlGrade = _xmlBaseCyberwareDataNode.SelectSingleNode("grades/grade[id = " + strSelectedGrade.CleanXPath() + "]");

            // Update the Essence and Cost multipliers based on the Grade that has been selected.
            if (xmlGrade != null)
            {
                _decCostMultiplier = Convert.ToDecimal(xmlGrade.SelectSingleNode("cost")?.Value, GlobalSettings.InvariantCultureInfo);
                _decESSMultiplier = Convert.ToDecimal(xmlGrade.SelectSingleNode("ess")?.Value, GlobalSettings.InvariantCultureInfo);
                _intAvailModifier = xmlGrade.SelectSingleNode("avail")?.ValueAsInt ?? 0;

                PopulateCategories();
                _blnLoading = false;
                RefreshList(_strSelectedCategory);
                lstCyberware_SelectedIndexChanged(sender, EventArgs.Empty);
            }
            else
            {
                _blnLoading = false;
                UpdateCyberwareInfo();
            }
        }

        private void cboGrade_EnabledChanged(object sender, EventArgs e)
        {
            if (cboGrade.Enabled != _blnOldGradeEnabled)
            {
                _blnOldGradeEnabled = cboGrade.Enabled;
                if (_blnOldGradeEnabled)
                {
                    cboGrade.SelectedValue = _strOldSelectedGrade;
                }
                cboGrade_SelectedIndexChanged(sender, e);
            }
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _blnLoading = true;
            _strSelectedCategory = cboCategory.SelectedValue?.ToString();
            string strForceGrade = string.Empty;
            // Update the list of Cyberware based on the selected Category.
            cboGrade.Enabled = !_blnLockGrade;
            if (_blnLockGrade)
                strForceGrade = cboGrade.SelectedValue?.ToString();
            // We may need to rebuild the Grade list since Cultured Bioware is not allowed to select Standard (Second-Hand) as Grade and ForceGrades can change.
            Grade objForcedGrade = _objForcedGrade ?? (string.IsNullOrEmpty(strForceGrade) ? null : _lstGrades.Find(x => x.SourceIDString == strForceGrade));
            PopulateGrades(!string.IsNullOrEmpty(_strSelectedCategory) && !cboGrade.Enabled && objForcedGrade?.SecondHand != true, false, strForceGrade, chkHideBannedGrades.Checked);
            _blnLoading = false;
            RefreshList(_strSelectedCategory);
        }

        private void lstCyberware_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _blnLoading = true;
            XPathNavigator xmlCyberware = null;
            string strSelectedId = lstCyberware.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected piece of Cyberware.
                xmlCyberware = _xmlBaseCyberwareDataNode.SelectSingleNode(_strNodeXPath + "[id = " + strSelectedId.CleanXPath() + "]");
            }
            string strForceGrade;
            if (xmlCyberware != null)
            {
                strForceGrade = xmlCyberware.SelectSingleNode("forcegrade")?.Value;
                // If the piece has a Rating value, enable the Rating control, otherwise, disable it and set its value to 0.
                XPathNavigator xmlRatingNode = xmlCyberware.SelectSingleNode("rating");
                if (xmlRatingNode != null)
                {
                    string strMinRating = xmlCyberware.SelectSingleNode("minrating")?.Value;
                    int intMinRating = 1;
                    // Not a simple integer, so we need to start mucking around with strings
                    if (!string.IsNullOrEmpty(strMinRating) && !int.TryParse(strMinRating, out intMinRating))
                    {
                        strMinRating = strMinRating.CheapReplace("MaximumSTR", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.TotalBody * 2) : _objCharacter.STR.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplace("MaximumAGI", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.Pilot * 2) : _objCharacter.AGI.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplace("MinimumSTR", () => (ParentVehicle?.TotalBody ?? 3).ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplace("MinimumAGI", () => (ParentVehicle?.Pilot ?? 3).ToString(GlobalSettings.InvariantCultureInfo));

                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strMinRating, out bool blnIsSuccess);
                        intMinRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                    }
                    nudRating.Minimum = intMinRating;

                    string strMaxRating = xmlRatingNode.Value;
                    int intMaxRating = 0;
                    // Not a simple integer, so we need to start mucking around with strings
                    if (!string.IsNullOrEmpty(strMaxRating) && !int.TryParse(strMaxRating, out intMaxRating))
                    {
                        strMaxRating = strMaxRating.CheapReplace("MaximumSTR", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.TotalBody * 2) : _objCharacter.STR.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplace("MaximumAGI", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.Pilot * 2) : _objCharacter.AGI.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplace("MinimumSTR", () => (ParentVehicle?.TotalBody ?? 3).ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplace("MinimumAGI", () => (ParentVehicle?.Pilot ?? 3).ToString(GlobalSettings.InvariantCultureInfo));

                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strMaxRating, out bool blnIsSuccess);
                        intMaxRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                    }
                    nudRating.Maximum = intMaxRating;
                    if (chkHideOverAvailLimit.Checked)
                    {
                        int intAvailModifier = strForceGrade == "None" ? 0 : _intAvailModifier;
                        while (nudRating.Maximum > intMinRating && !xmlCyberware.CheckAvailRestriction(_objCharacter, nudRating.MaximumAsInt, intAvailModifier))
                        {
                            --nudRating.Maximum;
                        }
                    }

                    if (chkShowOnlyAffordItems.Checked && !chkFree.Checked)
                    {
                        decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                        decCostMultiplier *= _decCostMultiplier;
                        if (chkBlackMarketDiscount.Checked)
                            decCostMultiplier *= 0.9m;
                        while (nudRating.Maximum > intMinRating && !xmlCyberware.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier, nudRating.MaximumAsInt))
                        {
                            --nudRating.Maximum;
                        }
                    }
                    nudRating.Value = nudRating.Minimum;
                    nudRating.Enabled = nudRating.Minimum != nudRating.Maximum;
                    nudRating.Visible = true;
                    lblRatingNALabel.Visible = false;
                    lblRatingLabel.Visible = true;
                }
                else
                {
                    lblRatingLabel.Visible = true;
                    lblRatingNALabel.Visible = true;
                    nudRating.Minimum = 0;
                    nudRating.Value = 0;
                    nudRating.Visible = false;
                }

                string strRatingLabel = xmlCyberware.SelectSingleNode("ratinglabel")?.Value;
                lblRatingLabel.Text = !string.IsNullOrEmpty(strRatingLabel)
                    ? string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_RatingFormat"),
                        LanguageManager.GetString(strRatingLabel))
                    : LanguageManager.GetString("Label_Rating");

                string strSource = xmlCyberware.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown");
                string strPage = xmlCyberware.SelectSingleNode("altpage")?.Value ?? xmlCyberware.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown");
                SourceString objSource = new SourceString(strSource, strPage, GlobalSettings.Language,
                    GlobalSettings.CultureInfo, _objCharacter);
                lblSource.Text = objSource.ToString();
                lblSource.SetToolTip(objSource.LanguageBookTooltip);
                lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);

                Grade objForcedGrade = null;
                if (!string.IsNullOrEmpty(strForceGrade))
                {
                    // Force the Cyberware to be a particular Grade.
                    if (cboGrade.Enabled)
                        cboGrade.Enabled = false;
                    objForcedGrade = _lstGrades.Find(x => x.Name == strForceGrade);
                    strForceGrade = objForcedGrade?.SourceIDString;
                }
                else
                {
                    cboGrade.Enabled = !_blnLockGrade;
                    if (_blnLockGrade)
                    {
                        strForceGrade = _objForcedGrade?.SourceIDString ?? cboGrade.SelectedValue?.ToString();
                        objForcedGrade = _objForcedGrade ?? _lstGrades.Find(x => x.SourceIDString == strForceGrade);
                    }
                }

                bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(xmlCyberware.SelectSingleNode("category")?.Value);
                chkBlackMarketDiscount.Enabled = blnCanBlackMarketDiscount;
                if (!chkBlackMarketDiscount.Checked)
                {
                    chkBlackMarketDiscount.Checked = GlobalSettings.AssumeBlackMarket && blnCanBlackMarketDiscount;
                }
                else if (!blnCanBlackMarketDiscount)
                {
                    //Prevent chkBlackMarketDiscount from being checked if the category doesn't match.
                    chkBlackMarketDiscount.Checked = false;
                }

                // We may need to rebuild the Grade list since Cultured Bioware is not allowed to select Standard (Second-Hand) as Grade and ForceGrades can change.
                PopulateGrades(xmlCyberware.SelectSingleNode("nosecondhand") != null || (!cboGrade.Enabled && objForcedGrade?.SecondHand != true), false, strForceGrade, chkHideBannedGrades.Checked);

                string strNotes = xmlCyberware.SelectSingleNode("altnotes")?.Value ?? xmlCyberware.SelectSingleNode("notes")?.Value;
                if (!string.IsNullOrEmpty(strNotes))
                {
                    lblCyberwareNotes.Visible = true;
                    lblCyberwareNotesLabel.Visible = true;
                    lblCyberwareNotes.Text = strNotes;
                }
                else
                {
                    lblCyberwareNotes.Visible = false;
                    lblCyberwareNotesLabel.Visible = false;
                }
                tlpRight.Visible = true;
            }
            else
            {
                tlpRight.Visible = false;
                cboGrade.Enabled = !_blnLockGrade;
                strForceGrade = string.Empty;
                Grade objForcedGrade = null;
                if (_blnLockGrade)
                {
                    strForceGrade = _objForcedGrade?.SourceIDString ?? cboGrade.SelectedValue?.ToString();
                    objForcedGrade = _objForcedGrade ?? _lstGrades.Find(x => x.SourceIDString == strForceGrade);
                }
                PopulateGrades(_blnLockGrade && objForcedGrade?.SecondHand != true, false, strForceGrade, chkHideBannedGrades.Checked);
                chkBlackMarketDiscount.Checked = false;
            }
            _blnLoading = false;
            UpdateCyberwareInfo();
        }

        private void ProcessCyberwareInfoChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            UpdateCyberwareInfo();
        }

        private void RefreshCurrentList(object sender, EventArgs e)
        {
            RefreshList(_strSelectedCategory);
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (chkShowOnlyAffordItems.Checked && !chkFree.Checked)
            {
                RefreshList(_strSelectedCategory);
            }
            UpdateCyberwareInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void chkHideBannedGrades_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _lstGrades.Clear();
            _lstGrades.AddRange(_objCharacter.GetGradeList(
                                    _objMode == Mode.Bioware
                                        ? Improvement.ImprovementSource.Bioware
                                        : Improvement.ImprovementSource.Cyberware, chkHideBannedGrades.Checked));
            PopulateGrades(false, false, string.Empty, chkHideBannedGrades.Checked);
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            AcceptForm();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshList(_strSelectedCategory);
        }

        private void chkFree_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (chkShowOnlyAffordItems.Checked)
            {
                RefreshList(_strSelectedCategory);
            }
            UpdateCyberwareInfo();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down when lstCyberware.SelectedIndex + 1 < lstCyberware.Items.Count:
                    lstCyberware.SelectedIndex++;
                    break;

                case Keys.Down:
                    {
                        if (lstCyberware.Items.Count > 0)
                        {
                            lstCyberware.SelectedIndex = 0;
                        }

                        break;
                    }
                case Keys.Up when lstCyberware.SelectedIndex - 1 >= 0:
                    lstCyberware.SelectedIndex--;
                    break;

                case Keys.Up:
                    {
                        if (lstCyberware.Items.Count > 0)
                        {
                            lstCyberware.SelectedIndex = lstCyberware.Items.Count - 1;
                        }

                        break;
                    }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }
        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain { get; private set; }

        /// <summary>
        /// Essence cost multiplier from the character.
        /// </summary>
        public decimal CharacterESSMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Total Essence cost multiplier from the character (stacks multiplicatively at the very last step.
        /// </summary>
        public decimal CharacterTotalESSMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Cost multiplier for Genetech.
        /// </summary>
        public decimal GenetechCostMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Essence cost multiplier for Genetech.
        /// </summary>
        public decimal GenetechEssMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Essence cost multiplier for Basic Bioware.
        /// </summary>
        public decimal BasicBiowareESSMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Whether or not the item has no cost.
        /// </summary>
        public bool FreeCost => chkFree.Checked;

        /// <summary>
        /// Set the window's Mode to Cyberware or Bioware.
        /// </summary>
        private Mode WindowMode => _objMode;

        /// <summary>
        /// Set the maximum Capacity the piece of Cyberware is allowed to be.
        /// </summary>
        public decimal MaximumCapacity
        {
            get => _decMaximumCapacity;
            set
            {
                _decMaximumCapacity = value;
                lblMaximumCapacity.Text = LanguageManager.GetString("Label_MaximumCapacityAllowed")
                                          + LanguageManager.GetString("String_Space")
                                          + _decMaximumCapacity.ToString("#,0.##", GlobalSettings.CultureInfo);
            }
        }

        /// <summary>
        /// Comma-separate list of Categories to show for Subsystems.
        /// </summary>
        public string Subsystems
        {
            set => _strSubsystems = value;
        }

        /// <summary>
        /// Comma-separate list of mount locations that are disallowed.
        /// </summary>
        public string DisallowedMounts
        {
            set => _strDisallowedMounts = value;
        }

        /// <summary>
        /// Comma-separate list of mount locations that already exist on the parent.
        /// </summary>
        public string HasModularMounts
        {
            set => _strHasModularMounts = value;
        }

        /// <summary>
        /// Manually set the Grade of the piece of Cyberware.
        /// </summary>
        public Grade ForcedGrade
        {
            get => _objForcedGrade;
            set => _objForcedGrade = value;
        }

        /// <summary>
        /// Name of Cyberware that was selected in the dialogue.
        /// </summary>
        public string SelectedCyberware { get; private set; } = string.Empty;

        /// <summary>
        /// Grade of the selected piece of Cyberware.
        /// </summary>
        public Grade SelectedGrade { get; private set; }

        /// <summary>
        /// Rating of the selected piece of Cyberware (0 if not applicable).
        /// </summary>
        public int SelectedRating { get; private set; }

        /// <summary>
        /// Selected Essence cost discount.
        /// </summary>
        public int SelectedESSDiscount { get; private set; }

        /// <summary>
        /// Whether or not the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount { get; private set; }

        /// <summary>
        /// Parent vehicle that the cyberlimb will be attached to.
        /// </summary>
        public Vehicle ParentVehicle { set; get; }

        public decimal Markup { get; set; }

        /// <summary>
        /// Whether the bioware should be discounted by Prototype Transhuman.
        /// </summary>
        public bool PrototypeTranshuman => chkPrototypeTranshuman.Checked && _objMode == Mode.Bioware && !_objCharacter.Created;

        /// <summary>
        /// Parent cyberware that the current selection will be added to.
        /// </summary>
        public Cyberware CyberwareParent { get; set; }

        /// <summary>
        /// Default text string to filter by.
        /// </summary>
        public string DefaultSearchText { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Update the Cyberware's information based on the Cyberware selected and current Rating.
        /// </summary>
        private void UpdateCyberwareInfo()
        {
            XPathNavigator objXmlCyberware = null;
            string strSelectedId = lstCyberware.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected piece of Cyberware.
                objXmlCyberware = _xmlBaseCyberwareDataNode.SelectSingleNode(_strNodeXPath + "[id = " + strSelectedId.CleanXPath() + "]");
            }
            if (objXmlCyberware == null)
            {
                tlpRight.Visible = false;
                return;
            }

            SuspendLayout();
            tlpRight.Visible = true;
            tlpRight.SuspendLayout();

            string strSelectCategory = objXmlCyberware.SelectSingleNode("category")?.Value ?? string.Empty;
            bool blnForceNoESSModifier = objXmlCyberware.SelectSingleNode("forcegrade")?.Value == "None";

            // Place the Genetech cost multiplier in a variable that can be safely modified.
            decimal decGenetechCostModifier = 1;
            // Genetech cost modifier only applies to Genetech.
            if (strSelectCategory.StartsWith("Genetech", StringComparison.Ordinal) || strSelectCategory.StartsWith("Genetic Infusions", StringComparison.Ordinal) || strSelectCategory.StartsWith("Genemods", StringComparison.Ordinal))
                decGenetechCostModifier = GenetechCostMultiplier;

            // Extract the Avail and Cost values from the Cyberware info since these may contain formulas and/or be based off of the Rating.
            // This is done using XPathExpression.

            int intRating = nudRating.ValueAsInt;
            AvailabilityValue objTotalAvail = new AvailabilityValue(nudRating.ValueAsInt, objXmlCyberware.SelectSingleNode("avail")?.Value, _intAvailModifier);
            lblAvailLabel.Visible = true;
            lblAvail.Text = objTotalAvail.ToString();

            // Cost.
            decimal decItemCost = 0;
            if (chkFree.Checked)
            {
                lblCost.Text = (0.0m).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
            }
            else
            {
                string strCost = objXmlCyberware.SelectSingleNode("cost")?.Value;
                if (!string.IsNullOrEmpty(strCost))
                {
                    if (strCost.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        string strSuffix = string.Empty;
                        if (!strCost.EndsWith(')'))
                        {
                            strSuffix = strCost.Substring(strCost.LastIndexOf(')') + 1);
                            strCost = strCost.TrimEndOnce(strSuffix);
                        }
                        string[] strValues = strCost.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                        strCost = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                        strCost += strSuffix;
                    }
                    // Check for a Variable Cost.
                    if (strCost.StartsWith("Variable(", StringComparison.Ordinal))
                    {
                        decimal decMin;
                        decimal decMax = decimal.MaxValue;
                        strCost = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                        if (strCost.Contains('-'))
                        {
                            string[] strValues = strCost.Split('-');
                            decMin = Convert.ToDecimal(strValues[0], GlobalSettings.InvariantCultureInfo);
                            decMax = Convert.ToDecimal(strValues[1], GlobalSettings.InvariantCultureInfo);
                        }
                        else
                            decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

                        lblCost.Text = decMax == decimal.MaxValue ?
                            decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + "¥+" :
                            decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + " - " + decMax.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';

                        decItemCost = decMin;
                    }
                    else
                    {
                        strCost = strCost.CheapReplace("Parent Cost", () => CyberwareParent?.Cost ?? "0")
                            .CheapReplace("Parent Gear Cost", () => CyberwareParent?.GearChildren.Sum(x => x.TotalCost).ToString(GlobalSettings.InvariantCultureInfo) ?? "0")
                            .CheapReplace("MinRating", () => nudRating.Minimum.ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplace("Rating", () => nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo));

                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                        if (blnIsSuccess)
                        {
                            decItemCost = (Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo) * _decCostMultiplier * decGenetechCostModifier);
                            decItemCost *= 1 + (nudMarkup.Value / 100.0m);

                            if (chkBlackMarketDiscount.Checked)
                            {
                                decItemCost *= 0.9m;
                            }

                            lblCost.Text = decItemCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        }
                        else
                        {
                            lblCost.Text = strCost + '¥';
                        }
                    }
                }
                else
                    lblCost.Text = (0.0m).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
            }

            lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);

            // Test required to find the item.
            lblTest.Text = _objCharacter.AvailTest(decItemCost, objTotalAvail);
            lblTestLabel.Visible = !string.IsNullOrEmpty(lblTest.Text);

            // Essence.
            lblESSDiscountLabel.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts;
            lblESSDiscountPercentLabel.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts;
            nudESSDiscount.Visible = _objCharacter.Settings.AllowCyberwareESSDiscounts;

            bool blnAddToParentESS = objXmlCyberware.SelectSingleNode("addtoparentess") != null;
            if (_objParentNode == null || blnAddToParentESS)
            {
                decimal decESS = 0;
                if (!chkPrototypeTranshuman.Checked)
                {
                    // Place the Essence cost multiplier in a variable that can be safely modified.
                    decimal decCharacterESSModifier = 1.0m;

                    if (!blnForceNoESSModifier)
                    {
                        decCharacterESSModifier = CharacterESSMultiplier;
                        // If Basic Bioware is selected, apply the Basic Bioware ESS Multiplier.
                        if (strSelectCategory == "Basic")
                            decCharacterESSModifier -= (1 - BasicBiowareESSMultiplier);
                        else if (strSelectCategory.StartsWith("Genetech", StringComparison.Ordinal) || strSelectCategory.StartsWith("Genetic Infusions", StringComparison.Ordinal) || strSelectCategory.StartsWith("Genemods", StringComparison.Ordinal))
                        {
                            decCharacterESSModifier -= (1 - GenetechEssMultiplier);
                        }

                        if (nudESSDiscount.Visible)
                        {
                            decimal decDiscountModifier = nudESSDiscount.Value / 100.0m;
                            decCharacterESSModifier *= (1.0m - decDiscountModifier);
                        }

                        decCharacterESSModifier -= (1 - _decESSMultiplier);

                        decCharacterESSModifier *= CharacterTotalESSMultiplier;
                    }

                    string strEss = objXmlCyberware.SelectSingleNode("ess")?.Value ?? string.Empty;
                    if (strEss.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        string strSuffix = string.Empty;
                        if (!strEss.EndsWith(')'))
                        {
                            strSuffix = strEss.Substring(strEss.LastIndexOf(')') + 1);
                            strEss = strEss.TrimEndOnce(strSuffix);
                        }
                        string[] strValues = strEss.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                        strEss = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                        strEss += strSuffix;
                    }

                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strEss.Replace("Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                    if (blnIsSuccess)
                    {
                        decESS = decCharacterESSModifier * Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                        if (!_objCharacter.Settings.DontRoundEssenceInternally)
                            decESS = decimal.Round(decESS, _objCharacter.Settings.EssenceDecimals, MidpointRounding.AwayFromZero);
                    }
                }

                lblEssence.Text = decESS.ToString(_objCharacter.Settings.EssenceFormat, GlobalSettings.CultureInfo);
                if (blnAddToParentESS)
                    lblEssence.Text = '+' + lblEssence.Text;
            }
            else
                lblEssence.Text = (0.0m).ToString(_objCharacter.Settings.EssenceFormat, GlobalSettings.CultureInfo);

            lblEssenceLabel.Visible = !string.IsNullOrEmpty(lblEssence.Text);

            // Capacity.
            bool blnAddToParentCapacity = objXmlCyberware.SelectSingleNode("addtoparentcapacity") != null;
            // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
            string strCapacity = objXmlCyberware.SelectSingleNode("capacity")?.Value ?? string.Empty;
            bool blnSquareBrackets = strCapacity.StartsWith('[');
            if (string.IsNullOrEmpty(strCapacity))
            {
                lblCapacity.Text = 0.ToString(GlobalSettings.CultureInfo);
            }
            else
            {
                if (blnSquareBrackets)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                if (strCapacity.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strCapacity = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                }
                if (strCapacity == "[*]" || strCapacity == "*")
                    lblCapacity.Text = "*";
                else
                {
                    int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                    if (intPos != -1)
                    {
                        string strFirstHalf = strCapacity.Substring(0, intPos);
                        string strSecondHalf = strCapacity.Substring(intPos + 1, strCapacity.Length - intPos - 1);

                        blnSquareBrackets = strFirstHalf.StartsWith('[');
                        if (blnSquareBrackets && strFirstHalf.Length > 1)
                            strFirstHalf = strFirstHalf.Substring(1, strCapacity.Length - 2);

                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strFirstHalf.Replace("Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                        lblCapacity.Text = blnIsSuccess ? objProcess.ToString() : strFirstHalf;

                        if (blnSquareBrackets)
                            lblCapacity.Text = '[' + lblCapacity.Text + ']';

                        strSecondHalf = strSecondHalf.Trim('[', ']');
                        objProcess = CommonFunctions.EvaluateInvariantXPath(strSecondHalf.Replace("Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), out blnIsSuccess);
                        strSecondHalf = (blnAddToParentCapacity ? "+[" : "[") + (blnIsSuccess ? objProcess.ToString() : strSecondHalf) + ']';

                        lblCapacity.Text += '/' + strSecondHalf;
                    }
                    else
                    {
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                        lblCapacity.Text = blnIsSuccess ? objProcess.ToString() : strCapacity;
                        if (blnSquareBrackets)
                            lblCapacity.Text = blnAddToParentCapacity ? "+[" + lblCapacity.Text + ']' : '[' + lblCapacity.Text + ']';
                    }
                }
            }

            lblCapacityLabel.Visible = !string.IsNullOrEmpty(lblCapacity.Text);
            tlpRight.ResumeLayout();
            ResumeLayout();
        }

        private bool _blnSkipListRefresh;

        private List<ListItem> RefreshList(string strCategory, bool blnDoUIUpdate = true, bool blnTerminateAfterFirst = false)
        {
            if ((_blnLoading || _blnSkipListRefresh) && blnDoUIUpdate)
                return new List<ListItem>();
            if (string.IsNullOrEmpty(strCategory))
            {
                if (blnDoUIUpdate)
                {
                    lstCyberware.BeginUpdate();
                    lstCyberware.PopulateWithListItems(new List<ListItem>());
                    lstCyberware.EndUpdate();
                }
                return new List<ListItem>();
            }

            StringBuilder sbdFilter = new StringBuilder('(' + _objCharacter.Settings.BookXPath() + ')');
            StringBuilder sbdCategoryFilter;
            if (strCategory != "Show All" && !Upgrading && (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0))
                sbdCategoryFilter = new StringBuilder("category = " + strCategory.CleanXPath() + " or category = \"None\"");
            else
            {
                sbdCategoryFilter = new StringBuilder();
                foreach (ListItem objItem in cboCategory.Items)
                {
                    string strItem = objItem.Value.ToString();
                    if (!string.IsNullOrEmpty(strItem))
                        sbdCategoryFilter.Append("category = ").Append(strItem.CleanXPath()).Append(" or ");
                }
                if (sbdCategoryFilter.Length > 0)
                {
                    sbdCategoryFilter.Append("category = \"None\"");
                }
            }
            if (sbdCategoryFilter.Length > 0)
                sbdFilter.Append(" and (").Append(sbdCategoryFilter).Append(')');

            if (ParentVehicle == null && _objCharacter.IsAI)
                sbdFilter.Append(" and (id = ").Append(Cyberware.EssenceHoleGUID.ToString().CleanXPath())
                         .Append(" or id = ").Append(Cyberware.EssenceAntiHoleGUID.ToString().CleanXPath())
                         .Append(" or mountsto)");
            else if (_objParentNode != null)
                sbdFilter.Append(" and (requireparent or contains(capacity, \"[\")) and not(mountsto)");
            else
                sbdFilter.Append(" and not(requireparent)");
            string strCurrentGradeId = cboGrade.SelectedValue?.ToString();
            Grade objCurrentGrade = string.IsNullOrEmpty(strCurrentGradeId) ? null : _lstGrades.Find(x => x.SourceIDString == strCurrentGradeId);
            if (objCurrentGrade != null)
            {
                sbdFilter.Append(" and (not(forcegrade) or forcegrade = \"None\" or forcegrade = ").Append(objCurrentGrade.Name.CleanXPath()).Append(')');
                if (objCurrentGrade.SecondHand)
                    sbdFilter.Append(" and not(nosecondhand)");
            }
            if (!string.IsNullOrEmpty(txtSearch.Text))
                sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(txtSearch.Text));
            XPathNodeIterator node = null;
            try
            {
                node = _xmlBaseCyberwareDataNode.Select(_strNodeXPath + '[' + sbdFilter + ']');
            }
            catch (XPathException e)
            {
                Log.Warn(e);
            }
            return BuildCyberwareList(node, blnDoUIUpdate, blnTerminateAfterFirst);
        }

        private List<ListItem> BuildCyberwareList(XPathNodeIterator objXmlCyberwareList, bool blnDoUIUpdate = true, bool blnTerminateAfterFirst = false)
        {
            if (_blnLoading && blnDoUIUpdate)
                return new List<ListItem>();

            List<ListItem> lstCyberwares = new List<ListItem>(objXmlCyberwareList?.Count ?? 1);

            bool blnCyberwareDisabled = !_objCharacter.AddCyberwareEnabled;
            bool blnBiowareDisabled = !_objCharacter.AddBiowareEnabled;
            string strCurrentGradeId = cboGrade.SelectedValue?.ToString();
            Grade objCurrentGrade = string.IsNullOrEmpty(strCurrentGradeId) ? null : _lstGrades.Find(x => x.SourceIDString == strCurrentGradeId);
            if (objXmlCyberwareList == null && !blnDoUIUpdate)
                return lstCyberwares;
            int intOverLimit = 0;
            if (objXmlCyberwareList?.Count > 0)
            {
                foreach (XPathNavigator xmlCyberware in objXmlCyberwareList)
                {
                    bool blnIsForceGrade = xmlCyberware.SelectSingleNodeAndCacheExpression("forcegrade") != null;
                    if (objCurrentGrade != null && blnIsForceGrade)
                    {
                        if (WindowMode == Mode.Bioware)
                        {
                            if (ImprovementManager
                                .GetCachedImprovementListForValueOf(_objCharacter,
                                                                    Improvement.ImprovementType.DisableBiowareGrade)
                                .Any(x => objCurrentGrade.Name.Contains(x.ImprovedName)))
                                continue;
                        }
                        else if (ImprovementManager
                                 .GetCachedImprovementListForValueOf(_objCharacter,
                                                                     Improvement.ImprovementType.DisableCyberwareGrade)
                                 .Any(x => objCurrentGrade.Name.Contains(x.ImprovedName)))
                            continue;
                    }

                    if (blnCyberwareDisabled && xmlCyberware.SelectSingleNodeAndCacheExpression("subsystems/cyberware") != null)
                    {
                        continue;
                    }

                    if (blnBiowareDisabled && xmlCyberware.SelectSingleNodeAndCacheExpression("subsystems/bioware") != null)
                    {
                        continue;
                    }

                    XPathNavigator xmlTestNode = xmlCyberware.SelectSingleNodeAndCacheExpression("forbidden/parentdetails");
                    if (xmlTestNode != null && _objParentNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        // Assumes topmost parent is an AND node
                        continue;
                    }

                    xmlTestNode = xmlCyberware.SelectSingleNodeAndCacheExpression("required/parentdetails");
                    if (xmlTestNode != null && !_objParentNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        // Assumes topmost parent is an AND node
                        continue;
                    }

                    if (!string.IsNullOrEmpty(_strHasModularMounts))
                    {
                        string strBlocksMounts = xmlCyberware.SelectSingleNodeAndCacheExpression("blocksmounts")?.Value;
                        if (!string.IsNullOrEmpty(strBlocksMounts))
                        {
                            ObservableCollection<Cyberware> lstWareListToCheck = null;
                            if (CyberwareParent != null)
                                lstWareListToCheck = CyberwareParent.Children;
                            else if (ParentVehicle == null)
                                lstWareListToCheck = _objCharacter.Cyberware;
                            if (xmlCyberware.SelectSingleNodeAndCacheExpression("selectside") == null || !string.IsNullOrEmpty(CyberwareParent?.Location) ||
                                lstWareListToCheck?.Any(x => x.Location == "Left") == true && lstWareListToCheck.Any(x => x.Location == "Right"))
                            {
                                string[] astrBlockedMounts = strBlocksMounts.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                if (_strHasModularMounts.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Any(strLoop => astrBlockedMounts.Contains(strLoop)))
                                    continue;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(_strDisallowedMounts))
                    {
                        string strLoopMount = xmlCyberware.SelectSingleNodeAndCacheExpression("modularmount")?.Value;
                        if (!string.IsNullOrEmpty(strLoopMount) && _strDisallowedMounts.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Contains(strLoopMount))
                            continue;
                    }

                    string strMaxRating = xmlCyberware.SelectSingleNodeAndCacheExpression("rating")?.Value;
                    string strMinRating = xmlCyberware.SelectSingleNodeAndCacheExpression("minrating")?.Value;
                    int intMinRating = 1;
                    // If our rating tag is a complex property, check to make sure our maximum rating is not less than our minimum rating
                    if ((!string.IsNullOrEmpty(strMaxRating) && !int.TryParse(strMaxRating, out int intMaxRating)) || (!string.IsNullOrEmpty(strMinRating) && !int.TryParse(strMinRating, out intMinRating)))
                    {
                        strMinRating = strMinRating.CheapReplace("MaximumSTR", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.TotalBody * 2) : _objCharacter.STR.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplace("MaximumAGI", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.Pilot * 2) : _objCharacter.AGI.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplace("MinimumSTR", () => (ParentVehicle?.TotalBody ?? 3).ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplace("MinimumAGI", () => (ParentVehicle?.Pilot ?? 3).ToString(GlobalSettings.InvariantCultureInfo));

                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strMinRating, out bool blnIsSuccess);
                        intMinRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;

                        strMaxRating = strMaxRating.CheapReplace("MaximumSTR", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.TotalBody * 2) : _objCharacter.STR.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplace("MaximumAGI", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.Pilot * 2) : _objCharacter.AGI.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplace("MinimumSTR", () => (ParentVehicle?.TotalBody ?? 3).ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplace("MinimumAGI", () => (ParentVehicle?.Pilot ?? 3).ToString(GlobalSettings.InvariantCultureInfo));

                        objProcess = CommonFunctions.EvaluateInvariantXPath(strMaxRating, out blnIsSuccess);
                        intMaxRating = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                        if (intMaxRating < intMinRating)
                            continue;
                    }

                    // Ex-Cons cannot have forbidden or restricted 'ware
                    if (_objCharacter.ExCon && ParentVehicle == null && xmlCyberware.SelectSingleNodeAndCacheExpression("mountsto") == null)
                    {
                        Cyberware objParent = CyberwareParent;
                        bool blnAnyParentIsModular = !string.IsNullOrEmpty(objParent?.PlugsIntoModularMount);
                        while (objParent != null && !blnAnyParentIsModular)
                        {
                            objParent = objParent.Parent;
                            blnAnyParentIsModular = !string.IsNullOrEmpty(objParent?.PlugsIntoModularMount);
                        }

                        if (!blnAnyParentIsModular)
                        {
                            string strAvailExpr = xmlCyberware.SelectSingleNodeAndCacheExpression("avail")?.Value ?? string.Empty;
                            if (strAvailExpr.StartsWith("FixedValues(", StringComparison.Ordinal))
                            {
                                string[] strValues = strAvailExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                                strAvailExpr = strValues[Math.Max(Math.Min(intMinRating - 1, strValues.Length - 1), 0)];
                            }

                            if (strAvailExpr.EndsWith('F', 'R'))
                            {
                                continue;
                            }
                        }
                    }

                    if (blnDoUIUpdate)
                    {
                        if (chkHideOverAvailLimit.Checked && !xmlCyberware.CheckAvailRestriction(_objCharacter, intMinRating, blnIsForceGrade ? 0 : _intAvailModifier))
                        {
                            ++intOverLimit;
                            continue;
                        }

                        if (chkShowOnlyAffordItems.Checked && !chkFree.Checked)
                        {
                            decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                            if (_setBlackMarketMaps.Contains(xmlCyberware.SelectSingleNodeAndCacheExpression("category")?.Value))
                                decCostMultiplier *= 0.9m;
                            if (!xmlCyberware.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier))
                            {
                                ++intOverLimit;
                                continue;
                            }
                        }
                    }

                    if (!Upgrading && ParentVehicle == null && !xmlCyberware.RequirementsMet(_objCharacter))
                        continue;

                    lstCyberwares.Add(new ListItem(xmlCyberware.SelectSingleNodeAndCacheExpression("id")?.Value, xmlCyberware.SelectSingleNodeAndCacheExpression("translate")?.Value ?? xmlCyberware.SelectSingleNodeAndCacheExpression("name")?.Value));
                    if (blnTerminateAfterFirst)
                        break;
                }
            }

            if (blnDoUIUpdate)
            {
                lstCyberwares.Sort(CompareListItems.CompareNames);
                if (intOverLimit > 0)
                {
                    // Add after sort so that it's always at the end
                    lstCyberwares.Add(new ListItem(string.Empty,
                        string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_RestrictedItemsHidden"),
                            intOverLimit)));
                }
                string strOldSelected = lstCyberware.SelectedValue?.ToString();
                _blnLoading = true;
                lstCyberware.BeginUpdate();
                lstCyberware.PopulateWithListItems(lstCyberwares);
                _blnLoading = false;
                if (!string.IsNullOrEmpty(strOldSelected))
                    lstCyberware.SelectedValue = strOldSelected;
                else
                    lstCyberware.SelectedIndex = -1;

                lstCyberware.EndUpdate();
            }

            return lstCyberwares;
        }

        /// <summary>
        /// Is a given piece of ware being Upgraded?
        /// </summary>
        public bool Upgrading { get; set; }

        /// <summary>
        /// Lock the Grade so it cannot be changed.
        /// </summary>
        public void LockGrade()
        {
            cboGrade.Enabled = false;
            _blnLockGrade = true;
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstCyberware.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedId))
                return;
            if (cboGrade.Text.StartsWith('*'))
            {
                Program.MainForm.ShowMessageBox(this,
                    LanguageManager.GetString("Message_BannedGrade"),
                    LanguageManager.GetString("MessageTitle_BannedGrade"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            XPathNavigator objCyberwareNode = _xmlBaseCyberwareDataNode.SelectSingleNode(_strNodeXPath + "[id = " + strSelectedId.CleanXPath() + "]");
            if (objCyberwareNode == null)
                return;

            if (_objCharacter.Settings.EnforceCapacity && _objParentObject != null)
            {
                // Capacity.
                bool blnAddToParentCapacity = objCyberwareNode.SelectSingleNodeAndCacheExpression("addtoparentcapacity") != null;
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                string strCapacity = objCyberwareNode.SelectSingleNodeAndCacheExpression("capacity")?.Value;
                if (strCapacity?.Contains('[') == true)
                {
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                    if (strCapacity.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                        strCapacity = strValues[Math.Max(Math.Min(nudRating.ValueAsInt, strValues.Length) - 1, 0)];
                    }

                    decimal decCapacity = 0;

                    if (strCapacity != "*")
                    {
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                        if (blnIsSuccess)
                            decCapacity = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                    }

                    decimal decMaximumCapacityUsed = blnAddToParentCapacity ? (_objParentObject as Cyberware)?.Parent?.CapacityRemaining ?? decimal.MaxValue : MaximumCapacity;

                    if (decMaximumCapacityUsed - decCapacity < 0)
                    {
                        Program.MainForm.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_OverCapacityLimit")
                                , decMaximumCapacityUsed.ToString("#,0.##", GlobalSettings.CultureInfo)
                                , decCapacity.ToString("#,0.##", GlobalSettings.CultureInfo)),
                            LanguageManager.GetString("MessageTitle_OverCapacityLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }
            if (!Upgrading && ParentVehicle == null && !objCyberwareNode.RequirementsMet(_objCharacter, null, LanguageManager.GetString(_objMode == Mode.Cyberware ? "String_SelectPACKSKit_Cyberware" : "String_SelectPACKSKit_Bioware")))
                return;

            string strForceGrade = objCyberwareNode.SelectSingleNodeAndCacheExpression("forcegrade")?.Value;
            if (!string.IsNullOrEmpty(strForceGrade))
            {
                SelectedGrade = _lstGrades.Find(x => x.Name == strForceGrade);
            }
            else
            {
                strForceGrade = cboGrade.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(strForceGrade))
                    SelectedGrade = _lstGrades.Find(x => x.SourceIDString == cboGrade.SelectedValue?.ToString());
                else
                    return;
            }
            _sStrSelectCategory = (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0) ? _strSelectedCategory : objCyberwareNode.SelectSingleNodeAndCacheExpression("category")?.Value;
            _sStrSelectGrade = SelectedGrade?.SourceIDString;
            SelectedCyberware = strSelectedId;
            SelectedRating = nudRating.ValueAsInt;
            BlackMarketDiscount = chkBlackMarketDiscount.Checked;
            Markup = nudMarkup.Value;

            if (nudESSDiscount.Visible)
                SelectedESSDiscount = nudESSDiscount.ValueAsInt;

            DialogResult = DialogResult.OK;
        }

        private bool _blnPopulatingGrades;

        /// <summary>
        /// Populate the list of Cyberware Grades.
        /// </summary>
        /// <param name="blnIgnoreSecondHand">Whether or not Second-Hand Grades should be added to the list.</param>
        /// <param name="blnForce">Force grades to be repopulated.</param>
        /// <param name="strForceGrade">If not empty, force this grade to be selected.</param>
        /// <param name="blnHideBannedGrades">Whether to hide grades banned by the character's gameplay options.</param>
        private void PopulateGrades(bool blnIgnoreSecondHand = false, bool blnForce = false, string strForceGrade = "", bool blnHideBannedGrades = true)
        {
            if (_blnPopulatingGrades)
                return;
            _blnPopulatingGrades = true;
            if (blnForce || blnIgnoreSecondHand != _blnIgnoreSecondHand || _strForceGrade != strForceGrade || cboGrade.Items.Count == 0)
            {
                _blnIgnoreSecondHand = blnIgnoreSecondHand;
                _strForceGrade = strForceGrade;
                List<ListItem> lstGrade = new List<ListItem>(5);
                foreach (Grade objWareGrade in _lstGrades)
                {
                    if (objWareGrade.SourceIDString == _strNoneGradeId && (string.IsNullOrEmpty(strForceGrade) || strForceGrade != _strNoneGradeId))
                        continue;
                    if (string.IsNullOrEmpty(strForceGrade))
                    {
                        if (blnIgnoreSecondHand && objWareGrade.SecondHand)
                            continue;
                        if (WindowMode == Mode.Bioware)
                        {
                            if (ImprovementManager
                                .GetCachedImprovementListForValueOf(_objCharacter,
                                                                    Improvement.ImprovementType.DisableBiowareGrade)
                                .Any(x => objWareGrade.Name.Contains(x.ImprovedName)))
                                continue;
                            if (objWareGrade.Adapsin)
                                continue;
                        }
                        else
                        {
                            if (ImprovementManager
                                .GetCachedImprovementListForValueOf(_objCharacter,
                                                                    Improvement.ImprovementType.DisableCyberwareGrade)
                                .Any(x => objWareGrade.Name.Contains(x.ImprovedName)))
                                continue;
                            if (_objCharacter.AdapsinEnabled && !objWareGrade.Adapsin && _lstGrades.Any(x => objWareGrade.Name.Contains(x.Name)))
                            {
                                continue;
                            }
                        }

                        if (_objCharacter.BurnoutEnabled)
                        {
                            if (!objWareGrade.Burnout && _lstGrades.Any(x =>
                                objWareGrade.Burnout && objWareGrade.Name.Contains(x.Name)))
                            {
                                continue;
                            }
                        }
                        else if (objWareGrade.Burnout)
                            continue;
                        if (blnHideBannedGrades && !_objCharacter.Created && !_objCharacter.IgnoreRules &&
                            _objCharacter.Settings.BannedWareGrades.Any(s => objWareGrade.Name.Contains(s)))
                            continue;
                    }

                    if (!blnHideBannedGrades && !_objCharacter.Created && !_objCharacter.IgnoreRules && _objCharacter.Settings.BannedWareGrades.Any(s => objWareGrade.Name.Contains(s)))
                    {
                        lstGrade.Add(new ListItem(objWareGrade.SourceIDString, '*' + objWareGrade.CurrentDisplayName));
                    }
                    else
                    {
                        lstGrade.Add(new ListItem(objWareGrade.SourceIDString, objWareGrade.CurrentDisplayName));
                    }
                }

                string strOldSelected = cboGrade.SelectedValue?.ToString();
                bool blnOldSkipListRefresh = _blnSkipListRefresh;
                if (strForceGrade == _strNoneGradeId || strOldSelected == _strNoneGradeId || lstGrade.Any(x => x.Value.ToString() == strOldSelected))
                    _blnSkipListRefresh = true;
                bool blnOldLoading = _blnLoading;
                _blnLoading = true;
                cboGrade.BeginUpdate();
                cboGrade.PopulateWithListItems(lstGrade);
                _blnLoading = blnOldLoading;
                if (!string.IsNullOrEmpty(strForceGrade))
                    cboGrade.SelectedValue = strForceGrade;
                else if (cboGrade.SelectedIndex <= 0 && !string.IsNullOrWhiteSpace(strOldSelected))
                    cboGrade.SelectedValue = strOldSelected;
                if (cboGrade.SelectedIndex == -1 && lstGrade.Count > 0)
                    cboGrade.SelectedIndex = 0;

                cboGrade.EndUpdate();

                _blnSkipListRefresh = blnOldSkipListRefresh;
            }
            _blnPopulatingGrades = false;
        }

        private bool _blnPopulatingCategories;

        private void PopulateCategories()
        {
            if (_blnPopulatingCategories)
                return;
            _blnPopulatingCategories = true;
            XPathNodeIterator objXmlCategoryList;
            if (_strSubsystems.Length > 0)
            {
                // Populate the Cyberware Category list.
                string strSubsystem = "categories/category[. = " + _strSubsystems.CleanXPath().Replace(",", "\" or . = \"") + "]";
                objXmlCategoryList = _xmlBaseCyberwareDataNode.Select(strSubsystem);
            }
            else
            {
                objXmlCategoryList = _xmlBaseCyberwareDataNode.SelectAndCacheExpression("categories/category");
            }
            List<ListItem> lstCategory = new List<ListItem>(objXmlCategoryList.Count);
            foreach (XPathNavigator objXmlCategory in objXmlCategoryList)
            {
                // Make sure the category contains items that we can actually display
                if (RefreshList(objXmlCategory.Value, false, true).Count > 0)
                {
                    string strInnerText = objXmlCategory.Value;
                    lstCategory.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strInnerText));
                }
            }

            lstCategory.Sort(CompareListItems.CompareNames);

            if (lstCategory.Count > 0)
            {
                lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll")));
            }

            string strOldSelected = _strSelectedCategory;
            string strOldSelectedCyberware = lstCyberware.SelectedValue?.ToString();
            bool blnOldLoading = _blnLoading;
            _blnLoading = true;
            cboCategory.BeginUpdate();
            cboCategory.PopulateWithListItems(lstCategory);
            _blnLoading = blnOldLoading;
            cboCategory.SelectedValue = strOldSelected;
            if (cboCategory.SelectedIndex == -1 && lstCategory.Count > 0)
                cboCategory.SelectedIndex = 0;

            cboCategory.EndUpdate();

            if (!string.IsNullOrEmpty(strOldSelectedCyberware))
                lstCyberware.SelectedValue = strOldSelectedCyberware;

            _blnPopulatingCategories = false;
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }

        #endregion Methods
    }
}
