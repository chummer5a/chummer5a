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

namespace Chummer
{
    public partial class frmSelectCyberware : Form
    {
        private readonly Character _objCharacter;

        private decimal _decCostMultiplier = 1.0m;
        private double _dblESSMultiplier = 1.0;
        private int _intAvailModifier;
        private readonly bool _blnCareer;

        private string _strSetGrade = string.Empty;
        private string _strSubsystems = string.Empty;
        private string _strDisallowedMounts = string.Empty;
        private string _strHasModularMounts = string.Empty;
        private decimal _decMaximumCapacity = -1;
        private bool _blnLockGrade;
        private bool _blnLoading = true;

        private Mode _objMode = Mode.Cyberware;
        private string _strNode = "cyberware";
        private static string _strSelectCategory = string.Empty;
        private static string _strSelectedGrade = string.Empty;
        private bool _blnIgnoreSecondHand = false;
        private string _strForceGrade = string.Empty;
        private XmlNode _objParentNode = null;

        private readonly XmlDocument _objXmlDocument = null;
        private readonly XPathNavigator _nav = null;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private readonly List<ListItem> _lstGrade = new List<ListItem>();

        public enum Mode
        {
            Cyberware = 0,
            Bioware = 1,
        }

        #region Control Events
        public frmSelectCyberware(Character objCharacter, Improvement.ImprovementSource objWareSource, bool blnCareer = false, XmlNode objParentNode = null)
        {
            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);
            chkFree.Visible = blnCareer;
            lblMarkupLabel.Visible = blnCareer;
            nudMarkup.Visible = blnCareer;
            lblMarkupPercentLabel.Visible = blnCareer;
            _blnCareer = blnCareer;
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
            _nav = _objXmlDocument.CreateNavigator();
        }

        private void frmSelectCyberware_Load(object sender, EventArgs e)
        {
            // Update the window title if needed.
            if (_strNode == "bioware")
                Text = LanguageManager.GetString("Title_SelectCyberware_Bioware");

            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            chkHideOverAvailLimit.Text = chkHideOverAvailLimit.Text.Replace("{0}",
                    _objCharacter.MaximumAvailability.ToString());
            chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;

            chkPrototypeTranshuman.Visible =
                _objCharacter.PrototypeTranshuman > 0 && _objMode == Mode.Bioware && !_objCharacter.Created;

            PopulateCategories();
            // Select the first Category in the list.
            if (string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedIndex = 0;
            else if (cboCategory.Items.Contains(_strSelectCategory))
            {
                cboCategory.SelectedValue = _strSelectCategory;
            }

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            // Populate the Grade list. Do not show the Adapsin Grades if Adapsin is not enabled for the character.
            PopulateGrades(false, true);

            if (string.IsNullOrEmpty(_strSetGrade))
            {
                if (string.IsNullOrEmpty(_strSelectedGrade))
                    cboGrade.SelectedIndex = 0;
                else
                    cboGrade.SelectedValue = _strSelectedGrade;
            }
            else
                cboGrade.SelectedValue = _strSetGrade;

            if (cboGrade.SelectedIndex == -1)
                cboGrade.SelectedIndex = 0;

            lblESSDiscountLabel.Visible = _objCharacter.Options.AllowCyberwareESSDiscounts;
            lblESSDiscountPercentLabel.Visible = _objCharacter.Options.AllowCyberwareESSDiscounts;
            nudESSDiscount.Visible = _objCharacter.Options.AllowCyberwareESSDiscounts;

            if (_objMode == Mode.Bioware && _objCharacter.Options.AllowCustomTransgenics)
                chkTransgenic.Visible = true;
            else
                chkTransgenic.Visible = false;
            _blnLoading = false;
        }

        private void cboGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboGrade.SelectedValue == null)
                return;

            // Update the Essence and Cost multipliers based on the Grade that has been selected.
            // Retrieve the information for the selected Grade.
            XmlNode objXmlGrade = _objXmlDocument.SelectSingleNode("/chummer/grades/grade[name = \"" + cboGrade.SelectedValue + "\"]");
            if (objXmlGrade != null)
            {
                _decCostMultiplier = Convert.ToDecimal(objXmlGrade["cost"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                _dblESSMultiplier = Convert.ToDouble(objXmlGrade["ess"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                _intAvailModifier = Convert.ToInt32(objXmlGrade["avail"]?.InnerText);
            }

            _strSelectedGrade = cboGrade.SelectedValue.ToString();
            if (cboGrade.Enabled)
            {
                lstCyberware.SelectedIndexChanged -= lstCyberware_SelectedIndexChanged;
                string strSelected = lstCyberware.SelectedValue?.ToString() ?? string.Empty;
                PopulateCategories();
                // Select the first Category in the list.
                if (string.IsNullOrEmpty(_strSelectCategory))
                    cboCategory.SelectedIndex = 0;
                else if (cboCategory.Items.Contains(_strSelectCategory))
                {
                    cboCategory.SelectedValue = _strSelectCategory;
                }

                if (cboCategory.SelectedIndex == -1)
                    cboCategory.SelectedIndex = 0;
                cboCategory.EndUpdate();
                txtSearch_TextChanged(sender, e);
                lstCyberware.SelectedValue = strSelected;
                lstCyberware.SelectedIndexChanged += lstCyberware_SelectedIndexChanged;
            }

            UpdateCyberwareInfo();
        }

        private void cboGrade_EnabledChanged(object sender, EventArgs e)
        {
            if (cboGrade.Enabled)
                cboGrade.SelectedValue = _strSelectedGrade;
            cboGrade_SelectedIndexChanged(sender, e);
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            _strSelectCategory = cboCategory.SelectedValue.ToString();
            string strForceGrade = string.Empty;
            // Update the list of Cyberware based on the selected Category.
            cboGrade.Enabled = !_blnLockGrade;
            if (_blnLockGrade && cboGrade.SelectedValue != null)
                strForceGrade = cboGrade.SelectedValue.ToString();
            // We may need to rebuild the Grade list since Cultured Bioware is not allowed to select Standard (Second-Hand) as Grade and ForceGrades can change.
            if (cboGrade.SelectedValue != null)
            {
                if (PopulateGrades(cboCategory.SelectedValue != null && !cboGrade.Enabled && !strForceGrade.Contains("Used"), false, strForceGrade))
                {
                    if (cboGrade.Enabled)
                        cboGrade.SelectedValue = _strSelectedGrade;
                }
            }
            if (!string.IsNullOrEmpty(strForceGrade))
                cboGrade.SelectedValue = strForceGrade;
            if (cboGrade.SelectedIndex == -1 && cboGrade.Items.Count > 0)
                cboGrade.SelectedIndex = 0;
            BuildCyberwareList();
        }

        private void lstCyberware_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstCyberware.Text))
                return;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (cboCategory.SelectedValue.ToString().Contains("Genetech:") && TransgenicsBiowareCostMultiplier != 1.0m || _blnCareer)
                chkFree.Visible = true;
            else
            {
                chkFree.Visible = false;
                chkFree.Checked = false;
            }
            if (chkTransgenic.Checked)
                chkFree.Visible = true;

            // Retireve the information for the selected piece of Cyberware.
            XmlNode objXmlCyberware = _objXmlDocument.SelectSingleNode("/chummer/" + _strNode + "s/" + _strNode + "[name = \"" + lstCyberware.SelectedValue + "\"]");
            if (objXmlCyberware == null) return;
            // If the piece has a Rating value, enable the Rating control, otherwise, disable it and set its value to 0.
            if (objXmlCyberware.InnerXml.Contains("<rating>"))
            {
                nudRating.Enabled = true;
                switch (objXmlCyberware["rating"]?.InnerText)
                {
                    case "MaximumSTR":
                        if (ParentVehicle != null)
                        {
                            nudRating.Maximum = ParentVehicle.TotalBody * 2;
                            nudRating.Minimum = ParentVehicle.TotalBody;
                        }
                        else
                        {
                            nudRating.Maximum = _objCharacter.STR.TotalMaximum;
                        }
                        break;
                    case "MaximumAGI":
                        if (ParentVehicle != null)
                        {
                            nudRating.Maximum = ParentVehicle.Pilot * 2;
                        }
                        else
                        {
                            nudRating.Maximum = _objCharacter.AGI.TotalMaximum;
                        }
                        break;
                    default:
                        nudRating.Maximum = Convert.ToInt32(objXmlCyberware["rating"]?.InnerText);
                        break;
                }
                if (objXmlCyberware["minrating"] != null)
                {
                    switch (objXmlCyberware["minrating"].InnerText)
                    {
                        case "MinimumAGI":
                            nudRating.Minimum = ParentVehicle?.Pilot ?? 4;
                            break;
                        case "MinimumSTR":
                            nudRating.Minimum = ParentVehicle?.TotalBody ?? 4;
                            break;
                        default:
                            nudRating.Minimum = Convert.ToInt32(objXmlCyberware["minrating"].InnerText);
                            break;
                    }
                }
                else
                {
                    nudRating.Minimum = 1;
                }
            }
            else
            {
                nudRating.Minimum = 0;
                nudRating.Value = 0;
                nudRating.Enabled = false;
            }

            string category = objXmlCyberware["category"]?.InnerText ?? string.Empty;
            string strForceGrade = string.Empty;
            if (objXmlCyberware["forcegrade"] != null)
            {
                // Force the Cyberware to be a particular Grade.
                cboGrade.Enabled = false;
                strForceGrade = objXmlCyberware["forcegrade"].InnerText;
            }
            else
            {
                cboGrade.Enabled = !_blnLockGrade;
                if (_blnLockGrade && cboGrade.SelectedValue != null)
                    strForceGrade = cboGrade.SelectedValue.ToString();
            }

            // We may need to rebuild the Grade list since Cultured Bioware is not allowed to select Standard (Second-Hand) as Grade and ForceGrades can change.
            if (cboGrade.SelectedValue != null)
            {
                if (PopulateGrades(objXmlCyberware["nosecondhand"] != null || (!cboGrade.Enabled && !strForceGrade.Contains("Used")), false, strForceGrade))
                {
                    if (cboGrade.Enabled)
                        cboGrade.SelectedValue = _strSelectedGrade;
                }
            }
            if (!string.IsNullOrEmpty(strForceGrade))
                cboGrade.SelectedValue = strForceGrade;
            if (cboGrade.SelectedIndex == -1 && cboGrade.Items.Count > 0)
                cboGrade.SelectedIndex = 0;
            string strBook = _objCharacter.Options.LanguageBookShort(objXmlCyberware["source"].InnerText);
            string strPage = objXmlCyberware["page"].InnerText;
            if (objXmlCyberware["altpage"] != null)
            {
                strPage = objXmlCyberware["altpage"].InnerText;
            }
            lblSource.Text = $"{strBook} {strPage}";
            if (objXmlCyberware["notes"] != null)
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

            tipTooltip.SetToolTip(lblSource,
                _objCharacter.Options.LanguageBookLong(objXmlCyberware["source"].InnerText) + " " +
                LanguageManager.GetString("String_Page") + " " + strPage);

            UpdateCyberwareInfo();
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateCyberwareInfo();
        }

        private void chkHideOverAvailLimit_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading) return;
            var selected = lstCyberware.SelectedValue?.ToString() ?? string.Empty;
            txtSearch_TextChanged(sender, e);

            for (var index = 0; index < lstCyberware.Items.Count; index++)
            {
                var item = (ListItem) lstCyberware.Items[index];
                if (item.Value != selected) continue;
                lstCyberware.SetSelected(index, true);
                break;
            }
        }

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDF(lblSource.Text, _objCharacter);
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
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                BuildCyberwareList();
                return;
            }

            string strCategoryFilter = "(";
            if (cboCategory.SelectedValue == null || cboCategory.SelectedValue.ToString() == "Show All")
                strCategoryFilter += _lstCategory.Where(objAllowedCategory => !string.IsNullOrEmpty(objAllowedCategory.Value)).Aggregate(string.Empty, (current, objAllowedCategory) => current + ("category = \"" + objAllowedCategory.Value + "\" or "));
            else
                strCategoryFilter += "category = \"" + cboCategory.SelectedValue + "\" or ";
            strCategoryFilter += "category = \"None\")";

            // Treat everything as being uppercase so the search is case-insensitive.
            string strSearch = "/chummer/" + _strNode + "s/" + _strNode + "[(" + _objCharacter.Options.BookXPath() + ") and " + strCategoryFilter + " and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))";
            if (_objCharacter.DEPEnabled && ParentVehicle == null)
                strSearch += " and (name = \"Essence Hole\" or name = \"Essence Antihole\" or mountsto)";
            else if (_objParentNode != null)
                strSearch += " and (requireparent or contains(capacity, \"[\")) and not(mountsto)";
            else
                strSearch += " and not(requireparent)";
            if (!string.IsNullOrEmpty(_strSelectedGrade))
            {
                strSearch += " and (not(forcegrade) or forcegrade = \"None\" or forcegrade = \"" + _strSelectedGrade + "\")";
            }
            strSearch += "]";

            BuildCyberwareList(_objXmlDocument.SelectNodes(strSearch));
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

            if (cboCategory.SelectedValue.ToString().Contains("Genetech:") && TransgenicsBiowareCostMultiplier != 1.0m || _blnCareer)
                chkFree.Visible = true;
            else
            {
                chkFree.Visible = false;
                chkFree.Checked = false;
            }
            if (chkTransgenic.Checked)
                chkFree.Visible = true;

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
        public double CharacterESSMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Total Essence cost multiplier from the character (stacks multiplicatively at the very last step.
        /// </summary>
        public double CharacterTotalESSMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Cost multiplier for Genetech.
        /// </summary>
        public decimal GenetechCostMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Essence cost multiplier for Basic Bioware.
        /// </summary>
        public double BasicBiowareESSMultiplier { get; set; } = 1.0;

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
        public Mode WindowMode
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
                lblMaximumCapacity.Text = $"{LanguageManager.GetString("Label_MaximumCapacityAllowed")} {_decMaximumCapacity:###,###,##0.##}";
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
        public Cyberware CyberwareParent;
        #endregion

        #region Methods
        /// <summary>
        /// Update the Cyberware's information based on the Cyberware selected and current Rating.
        /// </summary>
        private void UpdateCyberwareInfo()
        {
            if (string.IsNullOrEmpty(lstCyberware.Text)) return;
            XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/" + _strNode + "s/" + _strNode + "[name = \"" + lstCyberware.SelectedValue + "\"]");
            string strSelectCategory = objNode["category"].InnerText;
            bool blnForceNoESSModifier = objNode["forcegrade"]?.InnerText == "None";
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

            // Retireve the information for the selected piece of Cyberware.
            XmlNode objXmlCyberware = _objXmlDocument.SelectSingleNode("/chummer/" + _strNode + "s/" + _strNode + "[name = \"" + lstCyberware.SelectedValue + "\"]");

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
                    if (Convert.ToInt32(nudRating.Value) > 0)
                        strAvailExpr = strValues[Math.Min(Convert.ToInt32(nudRating.Value), strValues.Length) - 1];
                }
                if (strAvailExpr.StartsWith("+") || strAvailExpr.StartsWith("-"))
                {
                    strPrefix = strAvailExpr.Substring(0, 1);
                    strAvailExpr = strAvailExpr.Substring(1, strAvailExpr.Length - 1);
                }

                if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
                {
                    strSuffix = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
                    // Translate the Avail string.
                    if (strSuffix == "R")
                        strSuffix = LanguageManager.GetString("String_AvailRestricted");
                    else if (strSuffix == "F")
                        strSuffix = LanguageManager.GetString("String_AvailForbidden");
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }

                if (strAvailExpr.Contains("MinRating"))
                {
                    XmlNode xmlMinRatingNode = objXmlCyberware["minrating"];
                    if (xmlMinRatingNode != null)
                    {
                        switch (xmlMinRatingNode.InnerText)
                        {
                            case "MinimumAGI":
                                strAvailExpr = strAvailExpr.Replace("MinRating", ParentVehicle?.Pilot.ToString() ?? 3.ToString());
                                break;
                            case "MinimumSTR":
                                strAvailExpr = strAvailExpr.Replace("MinRating", ParentVehicle?.TotalBody.ToString() ?? 3.ToString());
                                break;
                            default:
                                strAvailExpr = strAvailExpr.Replace("MinRating", 3.ToString());
                                break;
                        }
                    }
                }
                strAvailExpr = strAvailExpr.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo));

                try
                {
                    XPathExpression xprAvail = _nav.Compile(strAvailExpr);
                    int intAvail = Convert.ToInt32(_nav.Evaluate(xprAvail)) + _intAvailModifier;
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
                lblCost.Text = $"{0:###,###,##0.##¥}";
            }
            else if (objXmlCyberware["cost"] != null)
            {
                string strCost = objXmlCyberware["cost"].InnerText;
                if (strCost.StartsWith("FixedValues"))
                {
                    string[] strValues = strCost.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (Convert.ToInt32(nudRating.Value) > 0)
                        strCost = strValues[Math.Min(Convert.ToInt32(nudRating.Value), strValues.Length) - 1];
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

                    lblCost.Text = decMax == decimal.MaxValue ? $"{decMin:###,###,##0.##¥+}" : $"{decMin:###,###,##0.##} - {decMax:###,###,##0.##¥}";

                    decItemCost = decMin;
                }
                else
                {
                    if (strCost.Contains("Parent Cost") || strCost.Contains("Parent Gear Cost"))
                    {
                        if (CyberwareParent != null)
                        {
                            strCost = strCost.Replace("Parent Cost", CyberwareParent.Cost);
                            strCost = strCost.Replace("Parent Gear Cost", CyberwareParent.Gear.Sum(x => x.TotalCost).ToString(GlobalOptions.InvariantCultureInfo));
                        }
                        else
                        {
                            strCost = strCost.Replace("Parent Cost", "0");
                            strCost = strCost.Replace("Parent Gear Cost", "0");
                        }
                    }
                    if (strCost.Contains("MinRating"))
                    {
                        XmlNode xmlMinRatingNode = objXmlCyberware["minrating"];
                        if (xmlMinRatingNode != null)
                        {
                            switch (xmlMinRatingNode.InnerText)
                            {
                                case "MinimumAGI":
                                    strCost = strCost.Replace("MinRating", ParentVehicle?.Pilot.ToString() ?? 3.ToString());
                                    break;
                                case "MinimumSTR":
                                    strCost = strCost.Replace("MinRating", ParentVehicle?.TotalBody.ToString() ?? 3.ToString());
                                    break;
                            }
                        }
                    }
                    strCost = strCost.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo));
                    try
                    {
                        XPathExpression xprCost = _nav.Compile(strCost);
                        decItemCost = (Convert.ToDecimal(_nav.Evaluate(xprCost), GlobalOptions.InvariantCultureInfo) * _decCostMultiplier *
                                          decGenetechCostModifier);
                        decItemCost *= 1 + (nudMarkup.Value / 100.0m);

                        if (chkBlackMarketDiscount.Checked)
                        {
                            decItemCost *= 0.9m;
                        }

                        lblCost.Text = $"{decItemCost:###,###,##0.##¥}";
                    }
                    catch (XPathException)
                    {
                        lblCost.Text = $"{strCost:###,###,##0.##¥}";
                    }
                }
            }
            else
                lblCost.Text = $"{decItemCost:###,###,##0.##¥}";

            // Test required to find the item.
            lblTest.Text = _objCharacter.AvailTest(decItemCost, lblAvail.Text);

            // Essence.

            double dblESS = 0;
            if (!chkPrototypeTranshuman.Checked)
            {
                // Place the Essence cost multiplier in a variable that can be safely modified.
                double dblCharacterESSModifier = 1;

                if (!blnForceNoESSModifier)
                {
                    dblCharacterESSModifier = CharacterESSMultiplier;
                    // If Basic Bioware is selected, apply the Basic Bioware ESS Multiplier.
                    if (strSelectCategory == "Basic")
                        dblCharacterESSModifier -= (1 - BasicBiowareESSMultiplier);

                    if (nudESSDiscount.Visible)
                    {
                        double dblDiscountModifier = Convert.ToDouble(nudESSDiscount.Value, GlobalOptions.CultureInfo) * 0.01;
                        dblCharacterESSModifier *= (1.0 - dblDiscountModifier);
                    }

                    dblCharacterESSModifier -= (1 - _dblESSMultiplier);

                    dblCharacterESSModifier *= CharacterTotalESSMultiplier;
                }
                string strEss = objXmlCyberware["ess"].InnerText;
                if (strEss.StartsWith("FixedValues"))
                {
                    string[] strValues = strEss.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (Convert.ToInt32(nudRating.Value) > 0)
                    strEss = strValues[Math.Min(Convert.ToInt32(nudRating.Value), strValues.Length) - 1];
                }
                XPathExpression xprEssence =
                        _nav.Compile(strEss.Replace("Rating",
                            nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)));
                dblESS =
                    Math.Round(
                        Convert.ToDouble(_nav.Evaluate(xprEssence), GlobalOptions.InvariantCultureInfo) *
                        dblCharacterESSModifier, _objCharacter.Options.EssenceDecimals, MidpointRounding.AwayFromZero);
            }
            lblEssence.Text = dblESS.ToString(GlobalOptions.CultureInfo);
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
                    XPathExpression xprCapacity;

                    if (strCapacity.Contains("/["))
                    {
                        int intPos = strCapacity.IndexOf("/[");
                        string strFirstHalf = strCapacity.Substring(0, intPos);
                        string strSecondHalf = strCapacity.Substring(intPos + 1, strCapacity.Length - intPos - 1);

                        blnSquareBrackets = strFirstHalf.Contains('[');
                        if (blnSquareBrackets && strFirstHalf.Length > 1)
                            strFirstHalf = strFirstHalf.Substring(1, strCapacity.Length - 2);
                        xprCapacity = _nav.Compile(strFirstHalf.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)));

                        lblCapacity.Text = _nav.Evaluate(xprCapacity).ToString();
                        if (blnSquareBrackets)
                            lblCapacity.Text = $"[{lblCapacity.Text}]";

                        strSecondHalf = strSecondHalf.Trim("[]".ToCharArray());
                        xprCapacity = _nav.Compile(strSecondHalf.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)));
                        strSecondHalf = "[" + _nav.Evaluate(xprCapacity).ToString() + "]";

                        lblCapacity.Text += "/" + strSecondHalf;
                    }
                    else
                    {
                        if (blnSquareBrackets)
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        xprCapacity = _nav.Compile(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)));
                        lblCapacity.Text = _nav.Evaluate(xprCapacity).ToString();
                        if (blnSquareBrackets)
                            lblCapacity.Text = $"[{lblCapacity.Text}]";
                    }
                }
            }
        }

        private void BuildCyberwareList(XmlNodeList objXmlCyberwareList = null)
        {
            if (!string.IsNullOrEmpty(txtSearch.Text) && objXmlCyberwareList == null)
            {
                txtSearch_TextChanged(null, EventArgs.Empty);
                return;
            }
            List<ListItem> lstCyberwares = new List<ListItem>();

            // Retrieve the list of Cyberware for the selected Category.
            if (objXmlCyberwareList == null)
            {
                string strCategoryFilter = "(";
                if (cboCategory.SelectedValue == null || cboCategory.SelectedValue.ToString() == "Show All")
                    strCategoryFilter += _lstCategory.Where(objAllowedCategory => !string.IsNullOrEmpty(objAllowedCategory.Value)).Aggregate(string.Empty, (current, objAllowedCategory) => current + ("category = \"" + objAllowedCategory.Value + "\" or "));
                else
                    strCategoryFilter += "category = \"" + cboCategory.SelectedValue + "\" or ";
                strCategoryFilter += "category = \"None\")";
                if (!string.IsNullOrEmpty(_strSelectedGrade))
                {
                    strCategoryFilter += " and (not(forcegrade) or forcegrade = \"None\" or forcegrade = \"" + _strSelectedGrade + "\")";
                }
                if (_objCharacter.DEPEnabled && ParentVehicle == null)
                    strCategoryFilter += " and (name = \"Essence Hole\" or name = \"Essence Antihole\" or mountsto)";
                else if (_objParentNode != null)
                    strCategoryFilter += " and (requireparent or contains(capacity, \"[\")) and not(mountsto)";
                else
                    strCategoryFilter += " and not(requireparent)";
                objXmlCyberwareList =
                        _objXmlDocument.SelectNodes("/chummer/" + _strNode + "s/" + _strNode + "[" + strCategoryFilter + " and (" + _objCharacter.Options.BookXPath() + ")]");
            }
            if (objXmlCyberwareList != null)
            {
                bool blnCyberwareDisabled = _objCharacter.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.DisableCyberware && x.Enabled);
                bool blnBiowareDisabled = _objCharacter.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.DisableBioware && x.Enabled);
                foreach (XmlNode objXmlCyberware in objXmlCyberwareList)
                {
                    if (!string.IsNullOrEmpty(_strSelectedGrade) && objXmlCyberware["forcegrade"] == null)
                    {
                        if (_objCharacter.Improvements.Any(x => ((_objMode == Mode.Bioware && x.ImproveType == Improvement.ImprovementType.DisableBiowareGrade) || (_objMode != Mode.Bioware && x.ImproveType == Improvement.ImprovementType.DisableCyberwareGrade)) && _strSelectedGrade.Contains(x.ImprovedName) && x.Enabled))
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
                        List<Cyberware> lstWareListToCheck = CyberwareParent == null ? (ParentVehicle == null ? _objCharacter.Cyberware : null) : CyberwareParent.Children;
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
                    if (!Backend.Shared_Methods.SelectionShared.CheckAvailRestriction(objXmlCyberware, _objCharacter,
                        chkHideOverAvailLimit.Checked, Convert.ToInt32(nudRating.Value), objXmlCyberware["forcegrade"]?.InnerText == "None" ? 0 : _intAvailModifier))
                        continue;
                    if (ParentVehicle == null && !Backend.Shared_Methods.SelectionShared.RequirementsMet(objXmlCyberware, false, _objCharacter))
                        continue;
                    ListItem objItem = new ListItem
                    {
                        Value = objXmlCyberware["name"]?.InnerText,
                        Name = objXmlCyberware["translate"]?.InnerText ?? objXmlCyberware["name"]?.InnerText
                    };
                    lstCyberwares.Add(objItem);
                    NextCyberware:;
                }
            }
            SortListItem objSort = new SortListItem();
            lstCyberwares.Sort(objSort.Compare);
            lstCyberware.BeginUpdate();
            lstCyberware.DataSource = null;
            lstCyberware.ValueMember = "Value";
            lstCyberware.DisplayMember = "Name";
            lstCyberware.DataSource = lstCyberwares;
            lstCyberware.EndUpdate();
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
            XmlNode objCyberwareNode = _objXmlDocument.SelectSingleNode("/chummer/" + _strNode + "s/" + _strNode + "[name = \"" + lstCyberware.SelectedValue + "\"]");
            if (objCyberwareNode == null) return;

            _strSelectCategory = objCyberwareNode["category"]?.InnerText;
            SelectedCyberware = objCyberwareNode["name"]?.InnerText;
            if (!string.IsNullOrEmpty(objCyberwareNode["forcegrade"]?.InnerText))
            {
                SelectedGrade = GlobalOptions.CyberwareGrades.GetGrade(objCyberwareNode["forcegrade"].InnerText);
            }
            else if (_objMode == Mode.Bioware)
            {
                SelectedGrade = GlobalOptions.BiowareGrades.GetGrade(_strSelectedGrade);
            }
            else
            {
                SelectedGrade = GlobalOptions.CyberwareGrades.GetGrade(_strSelectedGrade);
            }

            _strSelectedGrade = SelectedGrade.ToString();
            SelectedRating = Convert.ToInt32(nudRating.Value);
            BlackMarketDiscount = chkBlackMarketDiscount.Checked;

            if (nudESSDiscount.Visible)
                SelectedESSDiscount = Convert.ToInt32(nudESSDiscount.Value);

            if (objCyberwareNode["capacity"].InnerText.Contains('[') && _objParentNode != null && _objCharacter.Options.EnforceCapacity)
            {
                // Capacity.
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                string strCapacity = objCyberwareNode["capacity"].InnerText;
                strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                if (strCapacity.StartsWith("FixedValues"))
                {
                    string[] strValues = strCapacity.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    strCapacity = strValues[Math.Min(Convert.ToInt32(nudRating.Value), strValues.Length) - 1];
                }
                decimal decCapacity = 0;

                XPathExpression xprCapacity = _nav.Compile(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)));

                if (strCapacity != "*")
                {
                    decCapacity = Convert.ToDecimal(_nav.Evaluate(xprCapacity), GlobalOptions.InvariantCultureInfo);
                }
                if (MaximumCapacity - decCapacity < 0)
                {
                    MessageBox.Show(
                        LanguageManager.GetString("Message_OverCapacityLimit")
                            .Replace("{0}", MaximumCapacity.ToString("N2", GlobalOptions.CultureInfo))
                            .Replace("{1}", decCapacity.ToString("N2", GlobalOptions.CultureInfo)),
                        LanguageManager.GetString("MessageTitle_OverCapacityLimit"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            if (ParentVehicle == null && !Backend.Shared_Methods.SelectionShared.RequirementsMet(objCyberwareNode, true, _objCharacter))
                return;
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Populate the list of Cyberware Grades, returns true if anything changed.
        /// </summary>
        /// <param name="blnIgnoreSecondHand">Whether or not Secon-Hand Grades should be added to the list.</param>
        private bool PopulateGrades(bool blnIgnoreSecondHand = false, bool blnForce = false, string strForceGrade = "")
        {
            if (blnForce || blnIgnoreSecondHand != _blnIgnoreSecondHand || _strForceGrade != strForceGrade)
            {
                _blnIgnoreSecondHand = blnIgnoreSecondHand;
                _strForceGrade = strForceGrade;
                GradeList objGradeList = null;
                if (_objMode == Mode.Bioware)
                {
                    GlobalOptions.BiowareGrades.LoadList(Improvement.ImprovementSource.Bioware, _objCharacter.Options);
                    objGradeList = GlobalOptions.BiowareGrades;
                }
                else
                {
                    GlobalOptions.CyberwareGrades.LoadList(Improvement.ImprovementSource.Cyberware, _objCharacter.Options);
                    objGradeList = GlobalOptions.CyberwareGrades;
                }

                _lstGrade.Clear();
                foreach (Grade objGrade in objGradeList)
                {
                    if (objGrade.Name == "None" && (string.IsNullOrEmpty(_strForceGrade) || _strForceGrade != "None"))
                        continue;
                    if (_objCharacter.Improvements.Any(x => ((_objMode == Mode.Bioware && x.ImproveType == Improvement.ImprovementType.DisableBiowareGrade) || (_objMode == Mode.Cyberware && x.ImproveType == Improvement.ImprovementType.DisableCyberwareGrade))
                            && objGrade.Name.Contains(x.ImprovedName) && x.Enabled))
                        continue;
                    if ((!_blnIgnoreSecondHand || !objGrade.SecondHand) &&
                        (_objCharacter.BurnoutEnabled || !objGrade.Burnout) &&
                        (!_objCharacter.BurnoutEnabled || objGrade.Name != "Standard") &&
                        (_objCharacter.AdapsinEnabled || !objGrade.Adapsin))
                    {
                        ListItem objItem = new ListItem();
                        objItem.Value = objGrade.Name;
                        objItem.Name = objGrade.DisplayName;
                        _lstGrade.Add(objItem);
                    }
                }
                cboGrade.BeginUpdate();
                bool blnOldEnabled = cboGrade.Enabled;
                cboGrade.Enabled = false;
                cboGrade.DataSource = null;
                cboGrade.DataSource = _lstGrade;
                cboGrade.ValueMember = "Value";
                cboGrade.DisplayMember = "Name";
                cboGrade.Enabled = blnOldEnabled;
                cboGrade.EndUpdate();
                return true;
            }
            return false;
        }

        private void PopulateCategories()
        {
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
            _lstCategory.Clear();
            if (objXmlCategoryList != null)
                foreach (XmlNode objXmlCategory in objXmlCategoryList)
                {
                    // Make sure the category contains items that we can actually display
                    string strItemFilter = "[category = \"" + objXmlCategory.InnerText + "\" and (" + _objCharacter.Options.BookXPath() + ")";
                    if (!string.IsNullOrEmpty(_strSelectedGrade) && _strSelectedGrade.Contains("Used"))
                        strItemFilter += " and not(nosecondhand)";
                    if (_objCharacter.DEPEnabled && ParentVehicle == null)
                        strItemFilter += " and (name = \"Essence Hole\" or name = \"Essence Antihole\" or mountsto)";
                    else if (_objParentNode != null)
                        strItemFilter += " and (requireparent or contains(capacity, \"[\")) and not(mountsto)";
                    else
                        strItemFilter += " and not(requireparent)";
                    if (!string.IsNullOrEmpty(_strSelectedGrade))
                    {
                        strItemFilter += " and (not(forcegrade) or forcegrade = \"None\" or forcegrade = \"" + _strSelectedGrade + "\")";
                    }
                    strItemFilter += "]";
                    if (_objXmlDocument.SelectSingleNode("/chummer/" + _strNode + "s/" + _strNode + strItemFilter) == null)
                        continue;

                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlCategory.InnerText;
                    objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                    _lstCategory.Add(objItem);
                }

            SortListItem objSort = new SortListItem();
            _lstCategory.Sort(objSort.Compare);

            if (_lstCategory.Count > 0)
            {
                ListItem objItem = new ListItem();
                objItem.Value = "Show All";
                objItem.Name = LanguageManager.GetString("String_ShowAll");
                _lstCategory.Insert(0, objItem);
            }

            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;
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
