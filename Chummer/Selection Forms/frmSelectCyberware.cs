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
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;
using Chummer.Backend.Attributes;
using System.Text;

namespace Chummer
{
    public partial class frmSelectCyberware : Form
    {
        private readonly Character _objCharacter;
        private readonly List<Grade> _objGradeList;
        private readonly string _strNoneGradeId;

        private decimal _decCostMultiplier = 1.0m;
        private decimal _decESSMultiplier = 1.0m;
        private int _intAvailModifier;

        private string _strSetGrade = string.Empty;
        private string _strSubsystems = string.Empty;
        private string _strDisallowedMounts = string.Empty;
        private string _strHasModularMounts = string.Empty;
        private decimal _decMaximumCapacity = -1;
        private bool _blnLockGrade;
        private bool _blnLoading = true;

        private Mode _objMode = Mode.Cyberware;
        private string _strNode = "cyberware";
        private static string s_StrSelectCategory = string.Empty;
        private static string s_StrSelectGrade = string.Empty;
        private string _strSelectedCategory = string.Empty;
        private string _strOldSelectedGrade = string.Empty;
        private bool _blnOldGradeEnabled = true;
        private bool _blnIgnoreSecondHand = false;
        private string _strForceGrade = string.Empty;
        private XmlNode _objParentNode = null;

        private readonly XmlDocument _objXmlDocument = null;

        private enum Mode
        {
            Cyberware = 0,
            Bioware,
        }

        #region Control Events
        public frmSelectCyberware(Character objCharacter, Improvement.ImprovementSource objWareSource, XmlNode objParentNode = null)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            lblMarkupLabel.Visible = objCharacter.Created;
            nudMarkup.Visible = objCharacter.Created;
            lblMarkupPercentLabel.Visible = objCharacter.Created;
            _objCharacter = objCharacter;
            _objParentNode = objParentNode;
            MoveControls();

            // Load the Cyberware information.
            if (objWareSource == Improvement.ImprovementSource.Bioware)
                WindowMode = Mode.Bioware;
            else
                WindowMode = Mode.Cyberware;
            switch (WindowMode)
            {
                case Mode.Cyberware:
                    _objXmlDocument = XmlManager.Load("cyberware.xml");
                    break;
                case Mode.Bioware:
                    _objXmlDocument = XmlManager.Load("bioware.xml");
                    break;
            }

            _objGradeList = (List<Grade>)_objCharacter.GetGradeList(_objMode == Mode.Bioware ? Improvement.ImprovementSource.Bioware : Improvement.ImprovementSource.Cyberware);
            _strNoneGradeId = _objGradeList.FirstOrDefault(x => x.Name == "None").SourceId.ToString();
        }

        private void frmSelectCyberware_Load(object sender, EventArgs e)
        {
            // Update the window title if needed.
            if (_strNode == "bioware")
                Text = LanguageManager.GetString("Title_SelectCyberware_Bioware", GlobalOptions.Language);

            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith('['))
                    objLabel.Text = string.Empty;
            }

            if (_objCharacter.Created)
            {
                chkHideOverAvailLimit.Visible = false;
                chkHideOverAvailLimit.Checked = false;
            }
            else
            {
                chkHideOverAvailLimit.Text = chkHideOverAvailLimit.Text.Replace("{0}", _objCharacter.MaximumAvailability.ToString());
                chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;
            }

            chkPrototypeTranshuman.Visible = _objCharacter.PrototypeTranshuman > 0 && _objMode == Mode.Bioware && !_objCharacter.Created;

            PopulateCategories();
            // Select the first Category in the list.
            if (!string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedValue = s_StrSelectCategory;
            if (cboCategory.SelectedIndex == -1 && cboCategory.Items.Count > 0)
                cboCategory.SelectedIndex = 0;

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            // Populate the Grade list. Do not show the Adapsin Grades if Adapsin is not enabled for the character.
            PopulateGrades(false, true);

            if (!string.IsNullOrEmpty(_strSetGrade))
                cboGrade.SelectedValue = _strSetGrade;
            else if (!string.IsNullOrEmpty(s_StrSelectGrade))
                cboGrade.SelectedValue = s_StrSelectGrade;
            if (cboGrade.SelectedIndex == -1 && cboGrade.Items.Count > 0)
                cboGrade.SelectedIndex = 0;

            lblESSDiscountLabel.Visible = _objCharacter.Options.AllowCyberwareESSDiscounts;
            lblESSDiscountPercentLabel.Visible = _objCharacter.Options.AllowCyberwareESSDiscounts;
            nudESSDiscount.Visible = _objCharacter.Options.AllowCyberwareESSDiscounts;

            if (_objMode == Mode.Bioware && _objCharacter.Options.AllowCustomTransgenics)
                chkTransgenic.Visible = true;
            else
                chkTransgenic.Visible = false;

            _blnLoading = false;

            RefreshList(_strSelectedCategory);
        }

        private void cboGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            XmlNode objXmlGrade = null;
            // Retrieve the information for the selected Grade.
            string strSelectedGrade = cboGrade.SelectedValue?.ToString();
            if (cboGrade.Enabled)
                _strOldSelectedGrade = strSelectedGrade;
            if (!string.IsNullOrEmpty(strSelectedGrade))
                objXmlGrade = _objXmlDocument.SelectSingleNode("/chummer/grades/grade[id = \"" + strSelectedGrade + "\"]");

            // Update the Essence and Cost multipliers based on the Grade that has been selected.
            if (objXmlGrade != null)
            {
                _decCostMultiplier = Convert.ToDecimal(objXmlGrade["cost"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                _decESSMultiplier = Convert.ToDecimal(objXmlGrade["ess"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                _intAvailModifier = Convert.ToInt32(objXmlGrade["avail"]?.InnerText);
                
                PopulateCategories();
            }

            UpdateCyberwareInfo();
        }

        private void cboGrade_EnabledChanged(object sender, EventArgs e)
        {
            if (cboGrade.Enabled != _blnOldGradeEnabled)
            {
                _blnOldGradeEnabled = cboGrade.Enabled;
                if (_blnOldGradeEnabled)
                    cboGrade.SelectedValue = _strOldSelectedGrade;
                cboGrade_SelectedIndexChanged(sender, e);
            }
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            _strSelectedCategory = cboCategory.SelectedValue?.ToString();
            string strForceGrade = string.Empty;
            // Update the list of Cyberware based on the selected Category.
            cboGrade.Enabled = !_blnLockGrade;
            if (_blnLockGrade)
                strForceGrade = cboGrade.SelectedValue?.ToString();
            // We may need to rebuild the Grade list since Cultured Bioware is not allowed to select Standard (Second-Hand) as Grade and ForceGrades can change.
            Grade objForcedGrade = string.IsNullOrEmpty(strForceGrade) ? null : _objGradeList.FirstOrDefault(x => x.SourceId.ToString() == strForceGrade);
            PopulateGrades(!string.IsNullOrEmpty(_strSelectedCategory) && !cboGrade.Enabled && objForcedGrade?.SecondHand != true, false, strForceGrade);
            RefreshList(_strSelectedCategory);
        }

        private void lstCyberware_SelectedIndexChanged(object sender, EventArgs e)
        {
            XmlNode objXmlCyberware = null;
            string strSelectedId = lstCyberware.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected piece of Cyberware.
                objXmlCyberware = _objXmlDocument.SelectSingleNode("/chummer/" + _strNode + "s/" + _strNode + "[id = \"" + strSelectedId + "\"]");
            }

            // If the piece has a Rating value, enable the Rating control, otherwise, disable it and set its value to 0.
            if (objXmlCyberware?["rating"] != null)
            {
                nudRating.Enabled = true;

                string strMinRating = objXmlCyberware["minrating"]?.InnerText;
                int intMinRating = 1;
                // Not a simple integer, so we need to start mucking around with strings
                if (!string.IsNullOrEmpty(strMinRating) && !int.TryParse(strMinRating, out intMinRating))
                {
                    strMinRating = strMinRating.CheapReplace("MaximumSTR", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.TotalBody * 2) : _objCharacter.STR.TotalMaximum).ToString());
                    strMinRating = strMinRating.CheapReplace("MaximumAGI", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.Pilot * 2) : _objCharacter.AGI.TotalMaximum).ToString());
                    strMinRating = strMinRating.CheapReplace("MinimumSTR", () => (ParentVehicle != null ? ParentVehicle.TotalBody : 3).ToString());
                    strMinRating = strMinRating.CheapReplace("MinimumAGI", () => (ParentVehicle != null ? ParentVehicle.Pilot : 3).ToString());
                    try
                    {
                        intMinRating = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strMinRating));
                    }
                    catch (XPathException)
                    {
                        intMinRating = 1;
                    }
                }
                nudRating.Minimum = intMinRating;

                string strMaxRating = objXmlCyberware["rating"].InnerText;
                int intMaxRating = 0;
                // Not a simple integer, so we need to start mucking around with strings
                if (!string.IsNullOrEmpty(strMaxRating) && !int.TryParse(strMaxRating, out intMaxRating))
                {
                    strMaxRating = strMaxRating.CheapReplace("MaximumSTR", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.TotalBody * 2) : _objCharacter.STR.TotalMaximum).ToString());
                    strMaxRating = strMaxRating.CheapReplace("MaximumAGI", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.Pilot * 2) : _objCharacter.AGI.TotalMaximum).ToString());
                    strMaxRating = strMaxRating.CheapReplace("MinimumSTR", () => (ParentVehicle != null ? ParentVehicle.TotalBody : 3).ToString());
                    strMaxRating = strMaxRating.CheapReplace("MinimumAGI", () => (ParentVehicle != null ? ParentVehicle.Pilot : 3).ToString());
                    try
                    {
                        intMaxRating = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strMaxRating));
                    }
                    catch (XPathException)
                    {
                    }
                }
                nudRating.Maximum = intMaxRating;
                while (nudRating.Maximum > intMinRating && !Backend.SelectionShared.CheckAvailRestriction(objXmlCyberware, _objCharacter, chkHideOverAvailLimit.Checked, decimal.ToInt32(nudRating.Maximum), objXmlCyberware["forcegrade"]?.InnerText == "None" ? 0 : _intAvailModifier))
                {
                    nudRating.Maximum -= 1;
                }
                nudRating.Value = nudRating.Minimum;

                string strBook = CommonFunctions.LanguageBookShort(objXmlCyberware["source"].InnerText, GlobalOptions.Language);
                string strPage = objXmlCyberware["altpage"]?.InnerText ?? objXmlCyberware["page"].InnerText;
                lblSource.Text = $"{strBook} {strPage}";
                tipTooltip.SetToolTip(lblSource,
                    CommonFunctions.LanguageBookLong(objXmlCyberware["source"].InnerText, GlobalOptions.Language) + " " + LanguageManager.GetString("String_Page", GlobalOptions.Language) + " " + strPage);
            }
            else
            {
                nudRating.Minimum = 0;
                nudRating.Value = 0;
                nudRating.Enabled = false;
                lblSource.Text = string.Empty;
                tipTooltip.SetToolTip(lblSource, string.Empty);
            }
            
            string strForceGrade = objXmlCyberware?["forcegrade"]?.InnerText;
            Grade objForcedGrade = null;
            if (!string.IsNullOrEmpty(strForceGrade))
            {
                // Force the Cyberware to be a particular Grade.
                if (cboGrade.Enabled)
                    cboGrade.Enabled = false;
                objForcedGrade = _objGradeList.FirstOrDefault(x => x.Name == strForceGrade);
            }
            else
            {
                cboGrade.Enabled = !_blnLockGrade;
                if (_blnLockGrade)
                {
                    strForceGrade = cboGrade.SelectedValue?.ToString();
                    objForcedGrade = _objGradeList.FirstOrDefault(x => x.SourceId.ToString() == strForceGrade);
                }
            }

            // We may need to rebuild the Grade list since Cultured Bioware is not allowed to select Standard (Second-Hand) as Grade and ForceGrades can change.
            PopulateGrades(objXmlCyberware?["nosecondhand"] != null || (!cboGrade.Enabled && objForcedGrade?.SecondHand != true), false, strForceGrade);

            if (objXmlCyberware?["notes"] != null)
            {
                lblCyberwareNotes.Visible = true;
                lblCyberwareNotesLabel.Visible = true;
                lblCyberwareNotes.Text = objXmlCyberware["notes"].InnerText;
            }
            else
            {
                lblCyberwareNotes.Visible = false;
                lblCyberwareNotesLabel.Visible = false;
            }

            UpdateCyberwareInfo();
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateCyberwareInfo();
        }

        private void chkHideOverAvailLimit_CheckedChanged(object sender, EventArgs e)
        {
            RefreshList(_strSelectedCategory);
        }

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDF(lblSource.Text);
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            UpdateCyberwareInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstCyberware.Text))
                AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lblCategory_Click(object sender, EventArgs e)
        {

        }

        private void lstCyberware_DoubleClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstCyberware.Text))
                AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            cmdOK_Click(sender, e);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshList(_strSelectedCategory);
        }

        private void chkFree_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCyberwareInfo();
        }

        private void nudESSDiscount_ValueChanged(object sender, EventArgs e)
        {
            UpdateCyberwareInfo();
        }
        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCyberwareInfo();
        }
        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstCyberware.SelectedIndex + 1 < lstCyberware.Items.Count)
                {
                    lstCyberware.SelectedIndex++;
                }
                else if (lstCyberware.Items.Count > 0)
                {
                    lstCyberware.SelectedIndex = 0;
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (lstCyberware.SelectedIndex - 1 >= 0)
                {
                    lstCyberware.SelectedIndex--;
                }
                else if (lstCyberware.Items.Count > 0)
                {
                    lstCyberware.SelectedIndex = lstCyberware.Items.Count - 1;
                }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        private void chkTransgenic_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTransgenic.Checked)
            {
                cboGrade.Enabled = false;
                cboGrade.SelectedValue = "Standard";
            }
            else
                cboGrade.Enabled = true;

            UpdateCyberwareInfo();
        }
        private void chkPrototypeTranshuman_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCyberwareInfo();
        }
        #endregion

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
        /// Essence cost multiplier for Basic Bioware.
        /// </summary>
        public decimal BasicBiowareESSMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Cost multiplier for Transgenics Bioware.
        /// </summary>
        public decimal TransgenicsBiowareCostMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Whether or not the item has no cost.
        /// </summary>
        public bool FreeCost => chkFree.Checked;

        /// <summary>
        /// Set the window's Mode to Cyberware or Bioware.
        /// </summary>
        private Mode WindowMode
        {
            get
            {
                return _objMode;
            }
            set
            {
                _objMode = value;
                switch (_objMode)
                {
                    case Mode.Cyberware:
                        _strNode = "cyberware";
                        break;
                    case Mode.Bioware:
                        _strNode = "bioware";
                        break;
                }
            }
        }

        /// <summary>
        /// Set the maximum Capacity the piece of Cyberware is allowed to be.
        /// </summary>
        public decimal MaximumCapacity
        {
            get { return _decMaximumCapacity; }
            set
            {
                _decMaximumCapacity = value;
                lblMaximumCapacity.Text = $"{LanguageManager.GetString("Label_MaximumCapacityAllowed", GlobalOptions.Language)} {_decMaximumCapacity:#,0.##}";
            }
        }

        /// <summary>
        /// Comma-separate list of Categories to show for Subsystems.
        /// </summary>
        public string Subsystems
        {
            set
            {
                _strSubsystems = value;
            }
        }

        /// <summary>
        /// Comma-separate list of mount locations that are disallowed.
        /// </summary>
        public string DisallowedMounts
        {
            set
            {
                _strDisallowedMounts = value;
            }
        }

        /// <summary>
        /// Comma-separate list of mount locations that already exist on the parent.
        /// </summary>
        public string HasModularMounts
        {
            set
            {
                _strHasModularMounts = value;
            }
        }

        /// <summary>
        /// Manually set the Grade of the piece of Cyberware.
        /// </summary>
        public string SetGrade
        {
            set
            {
                _strSetGrade = value;
            }
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
        /// Whether or not the Bioware should be forced into the Genetech: Transgenics category.
        /// </summary>
        public bool ForceTransgenic
        {
            get
            {
                // If the Transgenics checkbox is checked, force it to the Genetech: Transgenics category.
                return chkTransgenic.Checked;
            }
        }

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
        #endregion

        #region Methods
        /// <summary>
        /// Update the Cyberware's information based on the Cyberware selected and current Rating.
        /// </summary>
        private void UpdateCyberwareInfo()
        {
            XmlNode objXmlCyberware = null;
            string strSelectedId = lstCyberware.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retireve the information for the selected piece of Cyberware.
                objXmlCyberware = _objXmlDocument.SelectSingleNode("/chummer/" + _strNode + "s/" + _strNode + "[id = \"" + strSelectedId + "\"]");
            }
            if (objXmlCyberware == null)
            {
                lblCost.Text = string.Empty;
                lblAvail.Text = string.Empty;
                lblTest.Text = string.Empty;
                lblEssence.Text = string.Empty;
                lblCapacity.Text = string.Empty;
                return;
            }

            string strSelectCategory = objXmlCyberware["category"].InnerText;
            bool blnForceNoESSModifier = objXmlCyberware["forcegrade"]?.InnerText == "None";
            // If the Transgenics checkbox has been checked, force it to the Genetech: Transgenics category instead.
            if (chkTransgenic.Checked)
                strSelectCategory = "Genetech: Transgenics";

            // Place the Genetech cost multiplier in a varaible that can be safely modified.
            decimal decGenetechCostModifier = 1;
            // Genetech cost modifier only applies to Genetech.
            if (strSelectCategory.StartsWith("Genetech") || strSelectCategory.StartsWith("Genetic Infusions") || strSelectCategory.StartsWith("Genemods"))
                decGenetechCostModifier = GenetechCostMultiplier;

            // If Genetech: Transgenics is selected, apply the Transgenetics Bioware ESS Multiplier.
            if (strSelectCategory == "Genetech: Transgenics")
                decGenetechCostModifier -= (1 - TransgenicsBiowareCostMultiplier);

            // Extract the Avil and Cost values from the Cyberware info since these may contain formulas and/or be based off of the Rating.
            // This is done using XPathExpression.

            // Avail.
            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strSuffix = string.Empty;
            string strPrefix = string.Empty;
            if (objXmlCyberware?["avail"] != null)
            {
                string strAvailExpr = objXmlCyberware["avail"].InnerText;
                if (strAvailExpr.StartsWith("FixedValues"))
                {
                    string[] strValues = strAvailExpr.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (decimal.ToInt32(nudRating.Value) > 0)
                        strAvailExpr = strValues[Math.Min(decimal.ToInt32(nudRating.Value), strValues.Length) - 1];
                }
                if (strAvailExpr.StartsWith('+') || strAvailExpr.StartsWith('-'))
                {
                    strPrefix = strAvailExpr.Substring(0, 1);
                    strAvailExpr = strAvailExpr.Substring(1, strAvailExpr.Length - 1);
                }

                if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
                {
                    strSuffix = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
                    // Translate the Avail string.
                    if (strSuffix == "R")
                        strSuffix = LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language);
                    else if (strSuffix == "F")
                        strSuffix = LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language);
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }

                strAvailExpr = strAvailExpr.CheapReplace("MinRating", () => nudRating.Minimum.ToString(GlobalOptions.InvariantCultureInfo));
                strAvailExpr = strAvailExpr.CheapReplace("Rating", () => nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo));

                try
                {
                    int intAvail = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvailExpr)) + _intAvailModifier;
                    // Avail cannot go below 0.
                    if (intAvail < 0)
                        intAvail = 0;
                    lblAvail.Text = strPrefix + intAvail + strSuffix;
                }
                catch (XPathException)
                {
                    lblAvail.Text = objXmlCyberware["avail"].InnerText;
                }
            }

            // Cost.
            decimal decItemCost = 0;
            if (chkFree.Checked)
            {
                lblCost.Text = 0.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            }
            else if (objXmlCyberware["cost"] != null)
            {
                string strCost = objXmlCyberware["cost"].InnerText;
                if (strCost.StartsWith("FixedValues"))
                {
                    string[] strValues = strCost.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (decimal.ToInt32(nudRating.Value) > 0)
                        strCost = strValues[Math.Min(decimal.ToInt32(nudRating.Value), strValues.Length) - 1];
                }
                // Check for a Variable Cost.
                if (strCost.StartsWith("Variable"))
                {
                    decimal decMin = 0;
                    decimal decMax = decimal.MaxValue;
                    strCost = strCost.TrimStart("Variable", true).Trim("()".ToCharArray());
                    if (strCost.Contains('-'))
                    {
                        string[] strValues = strCost.Split('-');
                        decMin = Convert.ToDecimal(strValues[0], GlobalOptions.InvariantCultureInfo);
                        decMax = Convert.ToDecimal(strValues[1], GlobalOptions.InvariantCultureInfo);
                    }
                    else
                        decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                    lblCost.Text = decMax == decimal.MaxValue ?
                        decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥+" :
                        decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + " - " + decMax.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                    decItemCost = decMin;
                }
                else
                {
                    if (strCost.Contains("Parent Cost") || strCost.Contains("Parent Gear Cost"))
                    {
                        if (CyberwareParent != null)
                        {
                            strCost = strCost.CheapReplace("Parent Cost", () => CyberwareParent.Cost);
                            strCost = strCost.CheapReplace("Parent Gear Cost", () => CyberwareParent.Gear.AsParallel().Sum(x => x.TotalCost).ToString(GlobalOptions.InvariantCultureInfo));
                        }
                        else
                        {
                            strCost = strCost.Replace("Parent Cost", "0");
                            strCost = strCost.Replace("Parent Gear Cost", "0");
                        }
                    }
                    strCost = strCost.CheapReplace("MinRating", () => nudRating.Minimum.ToString(GlobalOptions.InvariantCultureInfo));
                    strCost = strCost.CheapReplace("Rating", () => nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo));
                    try
                    {
                        decItemCost = (Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(strCost), GlobalOptions.InvariantCultureInfo) * _decCostMultiplier * decGenetechCostModifier);
                        decItemCost *= 1 + (nudMarkup.Value / 100.0m);

                        if (chkBlackMarketDiscount.Checked)
                        {
                            decItemCost *= 0.9m;
                        }

                        lblCost.Text = decItemCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                    }
                    catch (XPathException)
                    {
                        lblCost.Text = strCost + '¥';
                    }
                }
            }
            else
                lblCost.Text = decItemCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

            // Test required to find the item.
            lblTest.Text = _objCharacter.AvailTest(decItemCost, lblAvail.Text);

            // Essence.

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

                    if (nudESSDiscount.Visible)
                    {
                        decimal decDiscountModifier = nudESSDiscount.Value / 100.0m;
                        decCharacterESSModifier *= (1.0m - decDiscountModifier);
                    }

                    decCharacterESSModifier -= (1 - _decESSMultiplier);

                    decCharacterESSModifier *= CharacterTotalESSMultiplier;
                }
                string strEss = objXmlCyberware["ess"].InnerText;
                if (strEss.StartsWith("FixedValues"))
                {
                    string[] strValues = strEss.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (decimal.ToInt32(nudRating.Value) > 0)
                    strEss = strValues[Math.Min(decimal.ToInt32(nudRating.Value), strValues.Length) - 1];
                }
                decESS = decCharacterESSModifier * Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(strEss.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo))), GlobalOptions.InvariantCultureInfo);
                if (!_objCharacter.Options.DontRoundEssenceInternally)
                    decESS = decimal.Round(decESS, _objCharacter.Options.EssenceDecimals, MidpointRounding.AwayFromZero);
            }
            lblEssence.Text = decESS.ToString(GlobalOptions.CultureInfo);
            if (objXmlCyberware["addtoparentess"] != null)
                lblEssence.Text = "+" + lblEssence.Text;

            // Capacity.
            // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
            bool blnSquareBrackets = objXmlCyberware["capacity"]?.InnerText.Contains('[') ?? false;
            string strCapacity = objXmlCyberware["capacity"]?.InnerText ?? string.Empty;
            if (string.IsNullOrEmpty(strCapacity))
            {
                lblCapacity.Text = "0";
            }
            else
            {
                if (strCapacity.StartsWith("FixedValues"))
                {
                    string[] strValues = strCapacity.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (Convert.ToInt32(nudRating.Value, GlobalOptions.InvariantCultureInfo) > 0)
                        strCapacity = strValues[Math.Min(Convert.ToInt32(nudRating.Value, GlobalOptions.InvariantCultureInfo), strValues.Length) - 1];
                }
                if (strCapacity == "[*]")
                    lblCapacity.Text = "*";
                else
                {
                    if (strCapacity.Contains("/["))
                    {
                        int intPos = strCapacity.IndexOf("/[");
                        string strFirstHalf = strCapacity.Substring(0, intPos);
                        string strSecondHalf = strCapacity.Substring(intPos + 1, strCapacity.Length - intPos - 1);

                        blnSquareBrackets = strFirstHalf.Contains('[');
                        if (blnSquareBrackets && strFirstHalf.Length > 1)
                            strFirstHalf = strFirstHalf.Substring(1, strCapacity.Length - 2);
                        lblCapacity.Text = CommonFunctions.EvaluateInvariantXPath(strFirstHalf.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo))).ToString();
                        if (blnSquareBrackets)
                            lblCapacity.Text = $"[{lblCapacity.Text}]";

                        strSecondHalf = strSecondHalf.Trim("[]".ToCharArray());
                        strSecondHalf = "[" + CommonFunctions.EvaluateInvariantXPath(strSecondHalf.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo))).ToString() + "]";

                        lblCapacity.Text += "/" + strSecondHalf;
                    }
                    else
                    {
                        if (blnSquareBrackets)
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        lblCapacity.Text = CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo))).ToString();
                        if (blnSquareBrackets)
                            lblCapacity.Text = $"[{lblCapacity.Text}]";
                    }
                }
            }
        }

        private bool _blnSkipListRefresh = false;
        private IList<ListItem> RefreshList(string strCategory, bool blnDoUIUpdate = true, bool blnTerminateAfterFirst = false)
        {
            if ((_blnLoading || _blnSkipListRefresh) && blnDoUIUpdate)
                return null;
            if (string.IsNullOrEmpty(strCategory))
            {
                if (blnDoUIUpdate)
                {
                    lstCyberware.BeginUpdate();
                    lstCyberware.DataSource = null;
                    lstCyberware.EndUpdate();
                }
                return null;
            }

            string strFilter = "(" + _objCharacter.Options.BookXPath() + ")";
            string strCategoryFilter = "(";
            if (strCategory != "Show All" && (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0))
                strCategoryFilter += "category = \"" + strCategory + "\"";
            else
            {
                StringBuilder objCategoryFilter = new StringBuilder();
                foreach (string strItem in cboCategory.Items.Cast<ListItem>().Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                        objCategoryFilter.Append("category = \"" + strItem + "\" or ");
                }
                if (objCategoryFilter.Length > 0)
                {
                    strCategoryFilter += objCategoryFilter.ToString().TrimEnd(" or ");
                }
            }
            strFilter += " and " + strCategoryFilter + " or category = \"None\")";
            if (txtSearch.TextLength != 0)
            {
                // Treat everything as being uppercase so the search is case-insensitive.
                string strSearchText = txtSearch.Text.ToUpper();
                strFilter += " and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + strSearchText + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + strSearchText + "\"))";
            }

            if (_objCharacter.DEPEnabled && ParentVehicle == null)
                strFilter += " and (name = \"Essence Hole\" or name = \"Essence Antihole\" or mountsto)";
            else if (_objParentNode != null)
                strFilter += " and (requireparent or contains(capacity, \"[\")) and not(mountsto)";
            else
                strFilter += " and not(requireparent)";
            string strCurrentGradeId = cboGrade.SelectedValue?.ToString();
            Grade objCurrentGrade = string.IsNullOrEmpty(strCurrentGradeId) ? null : _objGradeList.FirstOrDefault(x => x.SourceId.ToString() == strCurrentGradeId);
            if (objCurrentGrade != null)
            {
                strFilter += " and (not(forcegrade) or forcegrade = \"None\" or forcegrade = \"" + objCurrentGrade.Name + "\")";
                if (objCurrentGrade.SecondHand)
                    strFilter += " and not(nosecondhand)";
            }

            return BuildCyberwareList(_objXmlDocument.SelectNodes("/chummer/" + _strNode + "s/" + _strNode + "[" + strFilter + "]"), blnDoUIUpdate, blnTerminateAfterFirst);
        }

        private IList<ListItem> BuildCyberwareList(XmlNodeList objXmlCyberwareList, bool blnDoUIUpdate = true, bool blnTerminateAfterFirst = false)
        {
            if (_blnLoading && blnDoUIUpdate)
                return null;

            List<ListItem> lstCyberwares = new List<ListItem>();

            bool blnCyberwareDisabled = _objCharacter.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.DisableCyberware && x.Enabled);
            bool blnBiowareDisabled = _objCharacter.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.DisableBioware && x.Enabled);
            string strCurrentGradeId = cboGrade.SelectedValue?.ToString();
            Grade objCurrentGrade = string.IsNullOrEmpty(strCurrentGradeId) ? null : _objGradeList.FirstOrDefault(x => x.SourceId.ToString() == strCurrentGradeId);
            foreach (XmlNode objXmlCyberware in objXmlCyberwareList)
            {
                if (objCurrentGrade != null && objXmlCyberware["forcegrade"] == null)
                {
                    if (_objCharacter.Improvements.Any(x => ((_objMode == Mode.Bioware && x.ImproveType == Improvement.ImprovementType.DisableBiowareGrade) || (_objMode != Mode.Bioware && x.ImproveType == Improvement.ImprovementType.DisableCyberwareGrade)) && objCurrentGrade.Name.Contains(x.ImprovedName) && x.Enabled))
                        continue;
                }
                if (blnCyberwareDisabled && objXmlCyberware.SelectSingleNode("subsystems/cyberware") != null)
                {
                    continue;
                }
                if (blnBiowareDisabled && objXmlCyberware.SelectSingleNode("subsystems/bioware") != null)
                {
                    continue;
                }
                if (objXmlCyberware["forbidden"]?["parentdetails"] != null)
                {
                    // Assumes topmost parent is an AND node
                    if (_objParentNode.ProcessFilterOperationNode(objXmlCyberware["forbidden"]["parentdetails"], false))
                    {
                        continue;
                    }
                }
                if (objXmlCyberware["required"]?["parentdetails"] != null)
                {
                    // Assumes topmost parent is an AND node
                    if (!_objParentNode.ProcessFilterOperationNode(objXmlCyberware["required"]["parentdetails"], false))
                    {
                        continue;
                    }
                }
                // TODO: Fix if someone has an amount of limbs different from the default amount
                if (!string.IsNullOrEmpty(_strHasModularMounts) && objXmlCyberware["blocksmounts"] != null)
                {
                    IList<Cyberware> lstWareListToCheck = CyberwareParent == null ? (ParentVehicle == null ? _objCharacter.Cyberware : null) : CyberwareParent.Children;
                    if (objXmlCyberware["selectside"] == null || !string.IsNullOrEmpty(CyberwareParent?.Location) || (lstWareListToCheck != null && lstWareListToCheck.Any(x => x.Location == "Left") && lstWareListToCheck.Any(x => x.Location == "Right")))
                    {
                        string[] astrBlockedMounts = objXmlCyberware["blocksmounts"].InnerText.Split(',');
                        foreach (string strLoop in _strHasModularMounts.Split(','))
                        {
                            if (astrBlockedMounts.Contains(strLoop))
                            {
                                goto NextCyberware;
                            }
                        }
                    }
                }
                // TODO: Fix if someone has an amount of limbs different from the default amount
                if (!string.IsNullOrEmpty(_strDisallowedMounts) && objXmlCyberware["modularmount"] != null)
                {
                    string strLoopMount = objXmlCyberware["modularmount"].InnerText;
                    foreach (string strLoop in _strHasModularMounts.Split(','))
                    {
                        if (strLoopMount == strLoop)
                        {
                            goto NextCyberware;
                        }
                    }
                }
                string strMaxRating = objXmlCyberware["rating"]?.InnerText;
                int intMaxRating = 0;
                string strMinRating = objXmlCyberware["minrating"]?.InnerText;
                int intMinRating = 1;
                // If our rating tag is a complex property, check to make sure our maximum rating is not less than our minimum rating
                if ((!string.IsNullOrEmpty(strMaxRating) && !int.TryParse(strMaxRating, out intMaxRating)) || (!string.IsNullOrEmpty(strMinRating) && !int.TryParse(strMinRating, out intMinRating)))
                {
                    strMinRating = strMinRating.CheapReplace("MaximumSTR", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.TotalBody * 2) : _objCharacter.STR.TotalMaximum).ToString());
                    strMinRating = strMinRating.CheapReplace("MaximumAGI", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.Pilot * 2) : _objCharacter.AGI.TotalMaximum).ToString());
                    strMinRating = strMinRating.CheapReplace("MinimumSTR", () => (ParentVehicle != null ? ParentVehicle.TotalBody : 3).ToString());
                    strMinRating = strMinRating.CheapReplace("MinimumAGI", () => (ParentVehicle != null ? ParentVehicle.Pilot : 3).ToString());
                    try
                    {
                        intMinRating = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strMinRating));
                    }
                    catch (XPathException)
                    {
                        intMinRating = 1;
                    }

                    strMaxRating = strMaxRating.CheapReplace("MaximumSTR", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.TotalBody * 2) : _objCharacter.STR.TotalMaximum).ToString());
                    strMaxRating = strMaxRating.CheapReplace("MaximumAGI", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.Pilot * 2) : _objCharacter.AGI.TotalMaximum).ToString());
                    strMaxRating = strMaxRating.CheapReplace("MinimumSTR", () => (ParentVehicle != null ? ParentVehicle.TotalBody : 3).ToString());
                    strMaxRating = strMaxRating.CheapReplace("MinimumAGI", () => (ParentVehicle != null ? ParentVehicle.Pilot : 3).ToString());
                    try
                    {
                        intMaxRating = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strMaxRating));
                    }
                    catch (XPathException)
                    {
                    }
                    if (intMaxRating < intMinRating)
                        continue;
                }
                if (!Backend.SelectionShared.CheckAvailRestriction(objXmlCyberware, _objCharacter,
                    chkHideOverAvailLimit.Checked, intMinRating, objXmlCyberware["forcegrade"]?.InnerText == "None" ? 0 : _intAvailModifier))
                    continue;
                if (ParentVehicle == null && !Backend.SelectionShared.RequirementsMet(objXmlCyberware, false, _objCharacter))
                    continue;
                
                lstCyberwares.Add(new ListItem(objXmlCyberware["id"]?.InnerText, objXmlCyberware["translate"]?.InnerText ?? objXmlCyberware["name"]?.InnerText));
                if (blnTerminateAfterFirst)
                    break;
                NextCyberware:;
            }
            if (blnDoUIUpdate)
            {
                lstCyberwares.Sort(CompareListItems.CompareNames);

                string strOldSelected = lstCyberware.SelectedValue?.ToString();

                lstCyberware.BeginUpdate();
                lstCyberware.DataSource = null;
                lstCyberware.ValueMember = "Value";
                lstCyberware.DisplayMember = "Name";
                lstCyberware.DataSource = lstCyberwares;

                if (!string.IsNullOrEmpty(strOldSelected))
                    lstCyberware.SelectedValue = strOldSelected;
                else
                    lstCyberware.SelectedIndex = -1;

                lstCyberware.EndUpdate();
            }

            return lstCyberwares;
        }

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
            XmlNode objCyberwareNode = _objXmlDocument.SelectSingleNode("/chummer/" + _strNode + "s/" + _strNode + "[id = \"" + strSelectedId + "\"]");
            if (objCyberwareNode == null)
                return;

            if (_objCharacter.Options.EnforceCapacity && _objParentNode != null && objCyberwareNode["capacity"].InnerText.Contains('['))
            {
                // Capacity.
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                string strCapacity = objCyberwareNode["capacity"].InnerText;
                strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                if (strCapacity.StartsWith("FixedValues"))
                {
                    string[] strValues = strCapacity.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    strCapacity = strValues[Math.Min(decimal.ToInt32(nudRating.Value), strValues.Length) - 1];
                }
                decimal decCapacity = 0;

                if (strCapacity != "*")
                {
                    decCapacity = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo))), GlobalOptions.InvariantCultureInfo);
                }
                if (MaximumCapacity - decCapacity < 0)
                {
                    MessageBox.Show(
                        LanguageManager.GetString("Message_OverCapacityLimit", GlobalOptions.Language)
                            .Replace("{0}", MaximumCapacity.ToString("#,0.##", GlobalOptions.CultureInfo))
                            .Replace("{1}", decCapacity.ToString("#,0.##", GlobalOptions.CultureInfo)),
                        LanguageManager.GetString("MessageTitle_OverCapacityLimit", GlobalOptions.Language),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            if (ParentVehicle == null && !Backend.SelectionShared.RequirementsMet(objCyberwareNode, true, _objCharacter))
                return;

            string strForceGrade = objCyberwareNode["forcegrade"]?.InnerText;
            if (!string.IsNullOrEmpty(strForceGrade))
            {
                SelectedGrade = _objGradeList.FirstOrDefault(x => x.Name == strForceGrade);
            }
            else
            {
                strForceGrade = cboGrade.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(strForceGrade))
                    SelectedGrade = _objGradeList.FirstOrDefault(x => x.SourceId.ToString() == cboGrade.SelectedValue?.ToString());
                else
                    return;
            }
            s_StrSelectCategory = (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0) ? _strSelectedCategory : objCyberwareNode["category"]?.InnerText;
            s_StrSelectGrade = SelectedGrade.SourceId.ToString();
            SelectedCyberware = objCyberwareNode["name"]?.InnerText;
            SelectedRating = decimal.ToInt32(nudRating.Value);
            BlackMarketDiscount = chkBlackMarketDiscount.Checked;

            if (nudESSDiscount.Visible)
                SelectedESSDiscount = decimal.ToInt32(nudESSDiscount.Value);

            DialogResult = DialogResult.OK;
        }

        private bool _blnPopulatingGrades = false;
        /// <summary>
        /// Populate the list of Cyberware Grades.
        /// </summary>
        /// <param name="blnIgnoreSecondHand">Whether or not Secon-Hand Grades should be added to the list.</param>
        private void PopulateGrades(bool blnIgnoreSecondHand = false, bool blnForce = false, string strForceGrade = "")
        {
            if (_blnPopulatingGrades)
                return;
            _blnPopulatingGrades = true;
            if (blnForce || blnIgnoreSecondHand != _blnIgnoreSecondHand || _strForceGrade != strForceGrade)
            {
                _blnIgnoreSecondHand = blnIgnoreSecondHand;
                _strForceGrade = strForceGrade;
                List<ListItem> lstGrade = new List<ListItem>(5);
                foreach (Grade objWareGrade in _objGradeList)
                {
                    if (objWareGrade.SourceId.ToString() == _strNoneGradeId && (string.IsNullOrEmpty(strForceGrade) || strForceGrade != _strNoneGradeId))
                        continue;
                    if (_objCharacter.Improvements.Any(x => (WindowMode == Mode.Bioware && x.ImproveType == Improvement.ImprovementType.DisableBiowareGrade || WindowMode != Mode.Bioware && x.ImproveType == Improvement.ImprovementType.DisableCyberwareGrade)
                                                                   && objWareGrade.Name.Contains(x.ImprovedName) && x.Enabled))
                        continue;
                    if (blnIgnoreSecondHand && objWareGrade.SecondHand)
                        continue;
                    if (!_objCharacter.AdapsinEnabled && objWareGrade.Adapsin)
                        continue;
                    if (!_objCharacter.Created && _objCharacter.BannedGrades.Any(s => objWareGrade.Name.Contains(s)))
                        continue;

                    lstGrade.Add(new ListItem(objWareGrade.SourceId.ToString(), objWareGrade.DisplayName(GlobalOptions.Language)));
                }
                
                string strOldSelected = cboGrade.SelectedValue?.ToString();
                bool blnOldSkipListRefresh = _blnSkipListRefresh;
                if (strForceGrade == _strNoneGradeId || strOldSelected == _strNoneGradeId || lstGrade.Any(x => x.Value == strOldSelected))
                    _blnSkipListRefresh = true;

                cboGrade.BeginUpdate();
                cboGrade.DataSource = null;
                cboGrade.ValueMember = "Value";
                cboGrade.DisplayMember = "Name";
                cboGrade.DataSource = lstGrade;
                
                if (!string.IsNullOrEmpty(strForceGrade))
                    cboGrade.SelectedValue = strForceGrade;
                if (cboGrade.SelectedIndex == -1 && !string.IsNullOrEmpty(strOldSelected))
                    cboGrade.SelectedValue = strOldSelected;
                if (cboGrade.SelectedIndex == -1 && lstGrade.Count > 0)
                    cboGrade.SelectedIndex = 0;

                cboGrade.EndUpdate();

                _blnSkipListRefresh = blnOldSkipListRefresh;
            }
            _blnPopulatingGrades = false;
        }

        private bool _blnPopulatingCategories = false;
        private void PopulateCategories()
        {
            if (_blnPopulatingCategories)
                return;
            _blnPopulatingCategories = true;
            XmlNodeList objXmlCategoryList;
            if (_strSubsystems.Length > 0)
            {
                // Populate the Cyberware Category list.
                string strSubsystem = ". = \"";
                if (_strSubsystems.Contains(','))
                {
                    strSubsystem += _strSubsystems.Replace(",", "\" or . = \"");
                }
                else
                {
                    strSubsystem += _strSubsystems;
                }
                objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category[" + strSubsystem + "\"]");
            }
            else
            {
                objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            }
            List<ListItem> lstCategory = new List<ListItem>();
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                // Make sure the category contains items that we can actually display
                if (RefreshList(objXmlCategory.InnerText, false, true).Count > 0)
                {
                    string strInnerText = objXmlCategory.InnerText;
                    lstCategory.Add(new ListItem(strInnerText, objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText));
                }
            }

            lstCategory.Sort(CompareListItems.CompareNames);

            if (lstCategory.Count > 0)
            {
                lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll", GlobalOptions.Language)));
            }

            string strOldSelected = _strSelectedCategory;
            string strOldSelectedCyberware = lstCyberware.SelectedValue?.ToString();

            cboCategory.BeginUpdate();
            cboCategory.DataSource = null;
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = lstCategory;

            if (!string.IsNullOrEmpty(strOldSelected))
                cboCategory.SelectedValue = strOldSelected;
            if (cboCategory.SelectedIndex == -1 && lstCategory.Count > 0)
                cboCategory.SelectedIndex = 0;

            cboCategory.EndUpdate();

            if (!string.IsNullOrEmpty(strOldSelectedCyberware))
                lstCyberware.SelectedValue = strOldSelectedCyberware;

            _blnPopulatingCategories = false;
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblRatingLabel.Width, lblEssenceLabel.Width);
            intWidth = Math.Max(intWidth, lblCapacityLabel.Width);
            intWidth = Math.Max(intWidth, lblAvailLabel.Width);
            intWidth = Math.Max(intWidth, lblCostLabel.Width);

            nudRating.Left = lblRatingLabel.Left + intWidth + 6;
            lblEssence.Left = lblEssenceLabel.Left + intWidth + 6;
            lblCapacity.Left = lblCapacityLabel.Left + intWidth + 6;
            lblAvail.Left = lblAvailLabel.Left + intWidth + 6;
            lblCost.Left = lblCostLabel.Left + intWidth + 6;

            lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;
            lblTest.Left = lblTestLabel.Left + lblTestLabel.Width + 6;
            nudESSDiscount.Left = lblESSDiscountLabel.Left + lblESSDiscountLabel.Width + 6;
            lblESSDiscountPercentLabel.Left = nudESSDiscount.Left + nudESSDiscount.Width;

            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
        }
        #endregion
    }
}
