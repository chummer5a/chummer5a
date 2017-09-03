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

        private double _dblCostMultiplier = 1.0;
        private double _dblESSMultiplier = 1.0;
        private int _intAvailModifier;
        private readonly bool _blnCareer;

        private string _strSetGrade = string.Empty;
        private bool _blnShowOnlySubsystems;
        private string _strSubsystems = string.Empty;
        private int _intMaximumCapacity = -1;
        private bool _blnLockGrade;
        private bool _blnLoading = true;

        private Mode _objMode = Mode.Cyberware;
        private string _strNode = "cyberware";
        private bool _blnAllowModularPlugins;
        private bool _blnShowOnlyLimbs;
        private static string _strSelectCategory = string.Empty;
        private static string _strSelectedGrade = string.Empty;

        private XmlDocument _objXmlDocument = new XmlDocument();
        private readonly XPathNavigator _nav;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private readonly List<ListItem> _lstGrade = new List<ListItem>();

        public enum Mode
        {
            Cyberware = 0,
            Bioware = 1,
        }

        #region Control Events
        public frmSelectCyberware(Character objCharacter, bool blnCareer = false)
        {
            _nav = _objXmlDocument.CreateNavigator();
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            chkFree.Visible = blnCareer;
            lblMarkupLabel.Visible = blnCareer;
            nudMarkup.Visible = blnCareer;
            lblMarkupPercentLabel.Visible = blnCareer;
            _blnCareer = blnCareer;
            _objCharacter = objCharacter;
            MoveControls();

        }

        private void frmSelectCyberware_Load(object sender, EventArgs e)
        {
            // Update the window title if needed.
            if (_strNode == "bioware")
                Text = LanguageManager.Instance.GetString("Title_SelectCyberware_Bioware");

            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            chkHideOverAvailLimit.Text = chkHideOverAvailLimit.Text.Replace("{0}",
                    _objCharacter.Options.Availability.ToString());
            chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;

            chkPrototypeTranshuman.Visible =
                _objCharacter.PrototypeTranshuman > 0 && _objMode == Mode.Bioware && !_objCharacter.Created;
            // Load the Cyberware information.
            switch (_objMode)
            {
                case Mode.Cyberware:
                    _objXmlDocument = XmlManager.Instance.Load("cyberware.xml");
                    break;
                case Mode.Bioware:
                    _objXmlDocument = XmlManager.Instance.Load("bioware.xml");
                    break;
            }

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
            if (objXmlCategoryList != null)
                foreach (XmlNode objXmlCategory in objXmlCategoryList)
                {
                    // Make sure the Category isn't in the exclusion list.
                    bool blnAddItem = true;

                    if (objXmlCategory.Attributes["show"] != null)
                        blnAddItem = _blnAllowModularPlugins;

                    if (_blnShowOnlyLimbs)
                        blnAddItem = objXmlCategory.InnerText == "Cyberlimb";

                    if (blnAddItem)
                    {
                        ListItem objItem = new ListItem();
                        objItem.Value = objXmlCategory.InnerText;
                        objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                        _lstCategory.Add(objItem);
                    }
                }
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            // Populate the Grade list. Do not show the Adapsin Grades if Adapsin is not enabled for the character.
            PopulateGrades();

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
                _dblCostMultiplier = Convert.ToDouble(objXmlGrade["cost"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                _dblESSMultiplier = Convert.ToDouble(objXmlGrade["ess"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                _intAvailModifier = Convert.ToInt32(objXmlGrade["avail"]?.InnerText);
            }

            var selected = lstCyberware.SelectedValue?.ToString() ?? "";
            txtSearch_TextChanged(sender, e);

            for (var index = 0; index < lstCyberware.Items.Count; index++)
            {
                var item = (ListItem) lstCyberware.Items[index];
                if (item.Value != selected) continue;
                lstCyberware.SetSelected(index, true);
                break;
            }
            UpdateCyberwareInfo();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_objMode == Mode.Bioware)
            {
                // If the window is currently showing Bioware, we may need to rebuild the Grade list since Cultured Bioware is not allowed to select Standard (Second-Hand) as as Grade.
                if (cboGrade.SelectedValue != null)
                {
                    string strSelectedValue = cboGrade.SelectedValue.ToString();
                    bool blnCultured = cboCategory.SelectedValue.ToString() == "Cultured";
                    PopulateGrades(blnCultured);
                    cboGrade.SelectedValue = strSelectedValue;
                }
                if (cboGrade.SelectedIndex == -1 && cboGrade.Items.Count > 0)
                    cboGrade.SelectedIndex = 0;
            }

            // Update the list of Cyberware based on the selected Category.
            if (cboCategory.SelectedValue.ToString().StartsWith("Genetech:") ||
                cboCategory.SelectedValue.ToString() == "Symbionts" ||
                cboCategory.SelectedValue.ToString() == "Genemods" ||
                _blnLockGrade)
            {
                cboGrade.Enabled = false;

            }
            else
            {
                cboGrade.Enabled = true;
            }

            if (cboCategory.SelectedValue.ToString().StartsWith("Genetech:") ||
                cboCategory.SelectedValue.ToString() == "Symbionts" ||
                cboCategory.SelectedValue.ToString() == "Genemods")
            {
                cboGrade.SelectedValue = "Standard";
            }
            BuildCyberwareList();
        }

        private void lstCyberware_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstCyberware.Text))
                return;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (cboCategory.SelectedValue.ToString().Contains("Genetech:") && TransgenicsBiowareCostMultiplier != 1.0 || _blnCareer)
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
            if (category.StartsWith("Genetech:") ||
                category.StartsWith("Symbionts") ||
                category.StartsWith("Genemods") ||
                _blnLockGrade)
            {
                cboGrade.Enabled = false;

            }
            else
            {
                cboGrade.Enabled = true;
            }

            if (category.StartsWith("Genetech:") ||
                category.StartsWith("Symbionts") ||
                category.StartsWith("Genemods"))
            {
                cboGrade.SelectedValue = "Standard";
            }

            if (objXmlCyberware["forcegrade"] != null)
            {
                // Force the Cyberware to be a particular Grade.
                cboGrade.SelectedValue = objXmlCyberware["forcegrade"].InnerText;
                cboGrade.Enabled = false;
            }

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

            if (objXmlCyberware["notes"] == null)
            {
                lblCyberwareNotes.Visible = false;
                lblCyberwareNotesLabel.Visible = false;
            }

            tipTooltip.SetToolTip(lblSource,
                _objCharacter.Options.LanguageBookLong(objXmlCyberware["source"].InnerText) + " " +
                LanguageManager.Instance.GetString("String_Page") + " " + strPage);

            UpdateCyberwareInfo();
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateCyberwareInfo();
        }

        private void chkHideOverAvailLimit_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading) return;
            var selected = lstCyberware.SelectedValue?.ToString() ?? "";
            txtSearch_TextChanged(sender, e);

            for (var index = 0; index < lstCyberware.Items.Count; index++)
            {
                var item = (ListItem) lstCyberware.Items[index];
                if (item.Value != selected) continue;
                lstCyberware.SetSelected(index, true);
                break;
            }
            ;
        }

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.StaticOpenPDF(lblSource.Text, _objCharacter);
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

            string strCategoryFilter = _lstCategory.Where(objAllowedCategory => !string.IsNullOrEmpty(objAllowedCategory.Value)).Aggregate(string.Empty, (current, objAllowedCategory) => current + ("category = \"" + objAllowedCategory.Value + "\" or "));

            // Treat everything as being uppercase so the search is case-insensitive.
            string strSearch = "/chummer/" + _strNode + "s/" + _strNode + "[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))";
            if (!string.IsNullOrEmpty(strCategoryFilter))
                strSearch += " and (" + strCategoryFilter + ")";
            // Remove the trailing " or ";
            strSearch = strSearch.Substring(0, strSearch.Length - 4) + ")";
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

            if (cboCategory.SelectedValue.ToString().Contains("Genetech:") && TransgenicsBiowareCostMultiplier != 1.0 || _blnCareer)
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
        /// Cost multiplier for Genetech.
        /// </summary>
        public double GenetechCostMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Essence cost multiplier for Basic Bioware.
        /// </summary>
        public double BasicBiowareESSMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Cost multiplier for Transgenics Bioware.
        /// </summary>
        public double TransgenicsBiowareCostMultiplier { get; set; } = 1.0;

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
        public int MaximumCapacity
        {
            get { return _intMaximumCapacity; }
            set
            {
                _intMaximumCapacity = value;
                lblMaximumCapacity.Text = $"{LanguageManager.Instance.GetString("Label_MaximumCapacityAllowed")} {_intMaximumCapacity}";
            }
        }

        /// <summary>
        /// Set whether or not only subsystems (those that consume Capacity) should be shown.
        /// </summary>
        public bool ShowOnlySubsystems
        {
            set
            {
                _blnShowOnlySubsystems = value;
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
        /// Whether or not Modular Plugins are allowed.
        /// </summary>
        public bool AllowModularPlugins
        {
            set
            {
                _blnAllowModularPlugins = value;
            }
        }

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
        /// Whether or not only Cyberlimb should be shown
        /// </summary>
        public bool ShowOnlyLimbs
        {
            set
            {
                _blnShowOnlyLimbs = value;
            }
        }

        /// <summary>
        /// Parent vehicle that the cyberlimb will be attached to.
        /// </summary>
        public Vehicle ParentVehicle { set; get; }

        public int Markup { get; set; }

        /// <summary>
        /// Whether the bioware should be discounted by Prototype Transhuman.
        /// </summary>
        public bool PrototypeTranshuman => chkPrototypeTranshuman.Checked && _objMode == Mode.Bioware && !_objCharacter.Created;

        /// <summary>
        /// Parent cyberware that the current selection will be added to.
        /// </summary>
        public Cyberware Parent;
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
            // If the Transgenics checkbox has been checked, force it to the Genetech: Transgenics category instead.
            if (chkTransgenic.Checked)
                strSelectCategory = "Genetech: Transgenics";

            // Place the Genetech cost multiplier in a varaible that can be safely modified.
            double dblGenetechCostModifier = 1;
            // Genetech cost modifier only applies to Genetech.
            if (strSelectCategory.StartsWith("Genetech") || strSelectCategory.StartsWith("Genetic Infusions") || strSelectCategory.StartsWith("Genemods"))
                dblGenetechCostModifier = GenetechCostMultiplier;

            // If Genetech: Transgenics is selected, apply the Transgenetics Bioware ESS Multiplier.
            if (strSelectCategory == "Genetech: Transgenics")
                dblGenetechCostModifier -= (1 - TransgenicsBiowareCostMultiplier);

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
                    string[] strValues = strAvailExpr.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
                    if (strValues.Length >= Convert.ToInt32(nudRating.Value))
                        strAvailExpr = strValues[Convert.ToInt32(nudRating.Value) - 1];
                }
                if (strAvailExpr.StartsWith("+") || strAvailExpr.StartsWith("-"))
                {
                    strPrefix = strAvailExpr.Substring(0, 1);
                    strAvailExpr = strAvailExpr.Substring(1, strAvailExpr.Length - 1);
                }

                if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
                {
                    strSuffix = strAvailExpr.Substring(strAvailExpr.Length - 1, 1)
                        .Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted"))
                        .Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));
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
            double dblItemCost = 0;
            if (chkFree.Checked)
            {
                lblCost.Text = $"{0:###,###,##0¥}";
                dblItemCost = 0;
            }
            else if (objXmlCyberware["cost"] != null)
            {
                string strCost = objXmlCyberware["cost"].InnerText;
                // Check for a Variable Cost.
                if (objXmlCyberware["cost"].InnerText.StartsWith("Variable"))
                {
                    int intMin;
                    int intMax = 0;
                    strCost = strCost.Replace("Variable", string.Empty).Trim("()".ToCharArray());
                    if (strCost.Contains("-"))
                    {
                        string[] strValues = strCost.Split('-');
                        intMin = Convert.ToInt32(strValues[0]);
                        intMax = Convert.ToInt32(strValues[1]);
                    }
                    else
                        intMin = Convert.ToInt32(strCost.Replace("+", string.Empty));

                    lblCost.Text = intMax == 0 ? $"{intMin:###,###,##0¥+}" : $"{intMin:###,###,##0} - {intMax:###,###,##0¥}";

                    dblItemCost = intMin;
                }
                else if (strCost.StartsWith("FixedValues"))
                {
                    string[] strValues = strCost.Replace("FixedValues", string.Empty).Trim("()".ToCharArray()).Split(',');
                    if (strValues.Length >= Convert.ToInt32(nudRating.Value))
                    {
                        dblItemCost = Convert.ToDouble(strValues[Convert.ToInt32(nudRating.Value) - 1], GlobalOptions.InvariantCultureInfo) * _dblCostMultiplier * dblGenetechCostModifier;
                        if (chkBlackMarketDiscount.Checked)
                        {
                            dblItemCost -= Convert.ToInt32(dblItemCost * 0.10);
                        }
                        double multiplier = 1 + Convert.ToDouble(nudMarkup.Value, GlobalOptions.InvariantCultureInfo) / 100.0;
                        dblItemCost *= multiplier;
                        lblCost.Text = $"{dblItemCost:###,###,##0¥}";
                    }
                }
                else
                {
                    if (strCost.StartsWith("Parent Cost"))
                    {
                        if (Parent != null)
                        {
                            strCost = strCost.Replace("Parent Cost", Parent.Cost);
                            if (strCost.Contains("Rating"))
                            {
                                strCost = strCost.Replace("Rating", Parent.Rating.ToString());
                            }
                        }
                        else
                            strCost = "0";
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
                    XPathExpression xprCost = _nav.Compile(strCost.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)));
                    double dblCost = (Convert.ToDouble(_nav.Evaluate(xprCost), GlobalOptions.InvariantCultureInfo) * _dblCostMultiplier *
                                      dblGenetechCostModifier);
                    dblCost *= 1 + (Convert.ToDouble(nudMarkup.Value, GlobalOptions.InvariantCultureInfo) / 100.0);
                    dblItemCost = dblCost;

                    if (chkBlackMarketDiscount.Checked)
                    {
                        dblItemCost -= Convert.ToInt32(dblItemCost * 0.10);
                    }

                    lblCost.Text = $"{dblItemCost:###,###,##0¥}";
                }
            }
            else
                lblCost.Text = $"{dblItemCost:###,###,##0¥}";

            // Test required to find the item.
            lblTest.Text = _objCharacter.AvailTest(Convert.ToInt32(dblItemCost), lblAvail.Text);

            // Essence.

            double dblESS = 0;
            if (!chkPrototypeTranshuman.Checked)
            {
                // Place the Essence cost multiplier in a variable that can be safely modified.
                double dblCharacterESSModifier = CharacterESSMultiplier;

                // If Basic Bioware is selected, apply the Basic Bioware ESS Multiplier.
                if (strSelectCategory == "Basic")
                    dblCharacterESSModifier -= (1 - BasicBiowareESSMultiplier);

                if (nudESSDiscount.Visible)
                {
                    double dblDiscountModifier = Convert.ToDouble(nudESSDiscount.Value, GlobalOptions.CultureInfo) * 0.01;
                    dblCharacterESSModifier *= (1.0 - dblDiscountModifier);
                }

                dblCharacterESSModifier -= (1 - _dblESSMultiplier);

                // Genetech and Genetic Infusions are not subject to Bioware cost multipliers, so if we're looking at either, suppress the multiplier.
                if (strSelectCategory.StartsWith("Genetech") || strSelectCategory.StartsWith("Genetic Infusions") ||
                    strSelectCategory.StartsWith("Genemods"))
                    dblCharacterESSModifier = 1;
                if (objXmlCyberware["ess"].InnerText.StartsWith("FixedValues"))
                {
                    string[] strValues =
                        objXmlCyberware["ess"].InnerText.Replace("FixedValues", string.Empty)
                            .Trim("()".ToCharArray())
                            .Split(',');
                    decimal decESS = Convert.ToDecimal(strValues[Convert.ToInt32(nudRating.Value) - 1],
                        GlobalOptions.InvariantCultureInfo);
                    dblESS =
                        Math.Round(Convert.ToDouble(decESS, GlobalOptions.InvariantCultureInfo) * dblCharacterESSModifier,
                            _objCharacter.Options.EssenceDecimals, MidpointRounding.AwayFromZero);
                }
                else
                {
                    XPathExpression xprEssence =
                        _nav.Compile(objXmlCyberware["ess"].InnerText.Replace("Rating",
                            nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)));
                    dblESS =
                        Math.Round(
                            Convert.ToDouble(_nav.Evaluate(xprEssence), GlobalOptions.InvariantCultureInfo) *
                            dblCharacterESSModifier, _objCharacter.Options.EssenceDecimals, MidpointRounding.AwayFromZero);
                }
                // Check if the character has Sensitive System.
                if (_objMode == Mode.Cyberware)
                {
                    dblESS =
                        _objCharacter.Improvements.Where(
                                objImprovement =>
                                    objImprovement.ImproveType == Improvement.ImprovementType.SensitiveSystem && objImprovement.Enabled)
                            .Aggregate(dblESS, (current, objImprovement) => current * 2.0);
                }
            }
            lblEssence.Text = dblESS.ToString(GlobalOptions.CultureInfo);

            // Capacity.
            // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
            bool blnSquareBrackets = objXmlCyberware["capacity"].InnerText.Contains('[');
            string strCapacity = objXmlCyberware["capacity"].InnerText;
            XPathExpression xprCapacity;

            if (objXmlCyberware["capacity"].InnerText.Contains("/["))
            {
                int intPos = objXmlCyberware["capacity"].InnerText.IndexOf("/[");
                string strFirstHalf = objXmlCyberware["capacity"].InnerText.Substring(0, intPos);
                string strSecondHalf = objXmlCyberware["capacity"].InnerText.Substring(intPos + 1, objXmlCyberware["capacity"].InnerText.Length - intPos - 1);

                blnSquareBrackets = strFirstHalf.Contains('[');
                strCapacity = strFirstHalf;
                if (blnSquareBrackets && strCapacity.Length > 1)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                xprCapacity = _nav.Compile(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)));

                if (objXmlCyberware["capacity"] != null)
                {
                    if (objXmlCyberware["capacity"].InnerText == "[*]")
                        lblCapacity.Text = "*";
                    else
                    {
                        if (objXmlCyberware["capacity"].InnerText.StartsWith("FixedValues"))
                        {
                            string[] strValues = objXmlCyberware["capacity"].InnerText.Replace("FixedValues", string.Empty).Trim("()".ToCharArray()).Split(',');
                            if (strValues.Length >= Convert.ToInt32(nudRating.Value, GlobalOptions.InvariantCultureInfo))
                                lblCapacity.Text = strValues[Convert.ToInt32(nudRating.Value, GlobalOptions.InvariantCultureInfo) - 1];
                        }
                        else
                            lblCapacity.Text = _nav.Evaluate(xprCapacity).ToString();
                    }
                    if (blnSquareBrackets)
                        lblCapacity.Text = $"[{lblCapacity.Text}]";
                }
                else
                {
                    lblCapacity.Text = "0";
                }

                if (strSecondHalf.Contains("Rating"))
                {
                    strSecondHalf = strSecondHalf.Replace("[", string.Empty).Replace("]", string.Empty);
                    xprCapacity = _nav.Compile(strSecondHalf.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)));
                    strSecondHalf = "[" + _nav.Evaluate(xprCapacity).ToString() + "]";
                }

                lblCapacity.Text += "/" + strSecondHalf;
            }
            else
            {
                if (blnSquareBrackets)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                xprCapacity = _nav.Compile(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)));

                if (objXmlCyberware["capacity"].InnerText == "[*]")
                    lblCapacity.Text = "*";
                else
                {
                    if (objXmlCyberware["capacity"].InnerText.StartsWith("FixedValues"))
                    {
                        string[] strValues = objXmlCyberware["capacity"].InnerText.Replace("FixedValues", string.Empty).Trim("()".ToCharArray()).Split(',');
                        lblCapacity.Text = strValues[Convert.ToInt32(nudRating.Value) - 1];
                    }
                    else
                        lblCapacity.Text = _nav.Evaluate(xprCapacity).ToString();
                }
                if (blnSquareBrackets)
                    lblCapacity.Text = $"[{lblCapacity.Text}]";
            }
        }

        private void BuildCyberwareList(XmlNodeList objXmlCyberwareList = null)
        {
            List<ListItem> lstCyberwares = new List<ListItem>();

            // Retrieve the list of Cyberware for the selected Category.
            if (objXmlCyberwareList == null)
            {
                if (_objCharacter.DEPEnabled && ParentVehicle == null)
                    objXmlCyberwareList =
                        _objXmlDocument.SelectNodes("/chummer/" + _strNode + "s/" + _strNode + "[category = \"" +
                                                    cboCategory.SelectedValue +
                                                    "\" and (name = \"Essence Hole\" or name = \"Essence Antihole\" ) and (" +
                                                    _objCharacter.Options.BookXPath() + ")]");
                else if (_blnShowOnlySubsystems)
                    objXmlCyberwareList =
                        _objXmlDocument.SelectNodes("/chummer/" + _strNode + "s/" + _strNode + "[category = \"" +
                                                    cboCategory.SelectedValue + "\" and (" + _objCharacter.Options.BookXPath() +
                                                    ") and contains(capacity, \"[\")]");
                else
                    objXmlCyberwareList =
                        _objXmlDocument.SelectNodes("/chummer/" + _strNode + "s/" + _strNode + "[category = \"" +
                                                    cboCategory.SelectedValue + "\" and (" + _objCharacter.Options.BookXPath() + ")]");
            }
            if (objXmlCyberwareList != null)
                foreach (XmlNode objXmlCyberware in objXmlCyberwareList)
                {
                    if (objXmlCyberware["hide"] != null)
                        continue;
                    if (!Backend.Shared_Methods.SelectionShared.CheckAvailRestriction(objXmlCyberware, _objCharacter,
                        chkHideOverAvailLimit.Checked, Convert.ToInt32(nudRating.Value), _intAvailModifier)) continue;
                    ListItem objItem = new ListItem
                    {
                        Value = objXmlCyberware["name"]?.InnerText,
                        Name = objXmlCyberware["translate"]?.InnerText ?? objXmlCyberware["name"]?.InnerText
                    };
                    lstCyberwares.Add(objItem);
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
            if (_objMode == Mode.Bioware)
            {
                if (cboCategory.SelectedValue.ToString().StartsWith("Genetech:") ||
                    cboCategory.SelectedValue.ToString() == "Symbionts" ||
                    cboCategory.SelectedValue.ToString() == "Genemods")
                {
                    SelectedGrade = GlobalOptions.BiowareGrades.GetGrade("Standard");
                }
                else
                {
                    SelectedGrade = GlobalOptions.BiowareGrades.GetGrade(cboGrade.SelectedValue.ToString());
                }
            }
            else
            {
                SelectedGrade = GlobalOptions.CyberwareGrades.GetGrade(cboGrade.SelectedValue.ToString());
            }

            _strSelectedGrade = SelectedGrade.ToString();
            SelectedRating = Convert.ToInt32(nudRating.Value);
            BlackMarketDiscount = chkBlackMarketDiscount.Checked;

            if (nudESSDiscount.Visible)
                SelectedESSDiscount = Convert.ToInt32(nudESSDiscount.Value);

            if (objCyberwareNode["capacity"].InnerText.Contains('[') && _blnShowOnlySubsystems && _objCharacter.Options.EnforceCapacity)
            {
                // Capacity.
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                string strCapacity = objCyberwareNode["capacity"].InnerText;
                strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                int intCapacity;

                XPathExpression xprCapacity = _nav.Compile(strCapacity.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)));

                if (strCapacity == "*")
                    intCapacity = 0;
                else
                {
                    if (strCapacity.StartsWith("FixedValues"))
                    {
                        string[] strValues = strCapacity.Replace("FixedValues", string.Empty).Trim("()".ToCharArray()).Split(',');
                        intCapacity = Convert.ToInt32(strValues[Convert.ToInt32(nudRating.Value) - 1]);
                    }
                    else
                        intCapacity = Convert.ToInt32(_nav.Evaluate(xprCapacity));
                }
                if (MaximumCapacity - intCapacity < 0)
                {
                    MessageBox.Show(
                        LanguageManager.Instance.GetString("Message_OverCapacityLimit")
                            .Replace("{0}", MaximumCapacity.ToString())
                            .Replace("{1}", intCapacity.ToString()),
                        LanguageManager.Instance.GetString("MessageTitle_OverCapacityLimit"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            if (!Backend.Shared_Methods.SelectionShared.RequirementsMet(objCyberwareNode, true, _objCharacter))
                return;
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Populate the list of Cyberware Grades.
        /// </summary>
        /// <param name="blnIgnoreSecondHand">Whether or not Secon-Hand Grades should be added to the list.</param>
        private void PopulateGrades(bool blnIgnoreSecondHand = false)
        {
            GradeList objGradeList = _objMode == Mode.Bioware ? GlobalOptions.BiowareGrades : GlobalOptions.CyberwareGrades;

            _lstGrade.Clear();
            foreach (Grade objGrade in objGradeList)
            {
                bool blnAddItem = true;

                ListItem objItem = new ListItem();
                objItem.Value = objGrade.Name;
                objItem.Name = objGrade.DisplayName;

                if (objGrade.Burnout && !_objCharacter.BurnoutEnabled)
                    blnAddItem = false;
                else if (objGrade.DisplayName == "Standard" && _objCharacter.BurnoutEnabled)
                    blnAddItem = false;
                else if (blnIgnoreSecondHand && objGrade.SecondHand)
                    blnAddItem = false;
                else if (!_objCharacter.AdapsinEnabled && objGrade.Adapsin)
                    blnAddItem = false;

                if (blnAddItem)
                    _lstGrade.Add(objItem);
            }
            cboGrade.BeginUpdate();
            cboGrade.DataSource = null;
            cboGrade.DataSource = _lstGrade;
            cboGrade.ValueMember = "Value";
            cboGrade.DisplayMember = "Name";
            cboGrade.EndUpdate();
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
