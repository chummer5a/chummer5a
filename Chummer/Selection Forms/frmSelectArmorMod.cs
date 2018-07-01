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
using System.Windows.Forms;
using System.Xml;
 using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmSelectArmorMod : Form
    {
        private string _strSelectedArmorMod = string.Empty;

        private string _strAllowedCategories = string.Empty;
        private bool _blnAddAgain;
        private decimal _decArmorCapacity;
        private decimal _decArmorCost;
        private decimal _decMarkup;
        private CapacityStyle _eCapacityStyle = CapacityStyle.Standard;

        private readonly XmlDocument _objXmlDocument;
        private readonly Character _objCharacter;
        private bool _blnBlackMarketDiscount;
        private bool _blnExcludeGeneralCategory;
        private readonly HashSet<string> _setBlackMarketMaps;

        #region Control Events
        public frmSelectArmorMod(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            MoveControls();
            // Load the Armor information.
            _objXmlDocument = XmlManager.Load("armor.xml");
            _setBlackMarketMaps = _objCharacter.GenerateBlackMarketMappings(_objXmlDocument);
        }

        private void frmSelectArmorMod_Load(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                chkHideOverAvailLimit.Visible = false;
                chkHideOverAvailLimit.Checked = false;
                lblMarkupLabel.Visible = true;
                nudMarkup.Visible = true;
                lblMarkupPercentLabel.Visible = true;
            }
            else
            {
                chkHideOverAvailLimit.Text = chkHideOverAvailLimit.Text.Replace("{0}", _objCharacter.MaximumAvailability.ToString());
                chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;
                lblMarkupLabel.Visible = false;
                nudMarkup.Visible = false;
                lblMarkupPercentLabel.Visible = false;
            }
            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;
            BuildModList();
        }

        private void lstMod_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedArmor();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateSelectedArmor();
        }

        private void lstMod_DoubleClick(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSelectedArmor();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            UpdateSelectedArmor();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Armor's Cost.
        /// </summary>
        public decimal ArmorCost
        {
            set => _decArmorCost = value;
        }

        /// <summary>
        /// Armor's Cost.
        /// </summary>
        public decimal ArmorCapacity
        {
            set => _decArmorCapacity = value;
        }

        /// <summary>
        /// Whether or not the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount => _blnBlackMarketDiscount;

        /// <summary>
        /// Name of Accessory that was selected in the dialogue.
        /// </summary>
        public string SelectedArmorMod => _strSelectedArmorMod;

        /// <summary>
        /// Rating that was selected in the dialogue.
        /// </summary>
        public int SelectedRating => decimal.ToInt32(nudRating.Value);

        /// <summary>
        /// Categories that the Armor allows to be used.
        /// </summary>
        public string AllowedCategories
        {
            get => _strAllowedCategories;
            set => _strAllowedCategories = value;
        }

        /// <summary>
        /// Whether or not the General category should be included.
        /// </summary>
        public bool ExcludeGeneralCategory {
            get => _blnExcludeGeneralCategory;
            set => _blnExcludeGeneralCategory = value;
        }

        /// <summary>
        /// Whether or not the item should be added for free.
        /// </summary>
        public bool FreeCost => chkFreeItem.Checked;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup => _decMarkup;

        /// <summary>
        /// Capacity display style.
        /// </summary>
        public CapacityStyle CapacityDisplayStyle
        {
            set => _eCapacityStyle = value;
        }
        #endregion

        #region Methods
        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSelectedArmor();
        }
        /// <summary>
        /// Update the information for the selected Armor Mod.
        /// </summary>
        private void UpdateSelectedArmor()
        {
            string strSelectedId = lstMod.SelectedValue?.ToString();
            XmlNode objXmlMod = null;
            if (!string.IsNullOrEmpty(strSelectedId))
                objXmlMod = _objXmlDocument.SelectSingleNode("/chummer/mods/mod[id = \"" + strSelectedId + "\"]");
            if (objXmlMod == null)
            {
                lblA.Text = string.Empty;
                nudRating.Enabled = false;
                lblAvail.Text = string.Empty;
                lblCost.Text = string.Empty;
                chkBlackMarketDiscount.Checked = false;
                lblCapacity.Text = string.Empty;
                lblSource.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblSource, string.Empty);
                return;
            }
            // Extract the Avil and Cost values from the Cyberware info since these may contain formulas and/or be based off of the Rating.
            // This is done using XPathExpression.

            lblA.Text = objXmlMod["armor"]?.InnerText;

            nudRating.Maximum = Convert.ToDecimal(objXmlMod["maxrating"]?.InnerText, GlobalOptions.InvariantCultureInfo);
            if (chkHideOverAvailLimit.Checked)
            {
                while (nudRating.Maximum > 1 && !SelectionShared.CheckAvailRestriction(objXmlMod, _objCharacter, decimal.ToInt32(nudRating.Maximum)))
                {
                    nudRating.Maximum -= 1;
                }
            }
            if (nudRating.Maximum <= 1)
                nudRating.Enabled = false;
            else
            {
                nudRating.Enabled = true;
                if (nudRating.Minimum == 0)
                {
                    nudRating.Value = 1;
                    nudRating.Minimum = 1;
                }
            }

            string strAvail = string.Empty;
            string strAvailExpr = objXmlMod["avail"]?.InnerText ?? string.Empty;
            if (strAvailExpr.Length > 0)
            {
                char chrLastAvailChar = strAvailExpr[strAvailExpr.Length - 1];
                if (chrLastAvailChar == 'F')
                {
                    strAvail = LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language);
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }
                else if (chrLastAvailChar == 'R')
                {
                    strAvail = LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language);
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }
            }

            object objProcess = CommonFunctions.EvaluateInvariantXPath(strAvailExpr.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo)), out bool blnIsSuccess);
            lblAvail.Text = (blnIsSuccess ? Convert.ToInt32(objProcess).ToString() : strAvailExpr) + strAvail;

            // Cost.
            chkBlackMarketDiscount.Checked = _setBlackMarketMaps.Contains(objXmlMod["category"]?.InnerText);
            if (chkFreeItem.Checked)
            {
                lblCost.Text = (0.0m).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblTest.Text = _objCharacter.AvailTest(0, lblAvail.Text);
            }
            else
            {
                string strCostElement = objXmlMod["cost"]?.InnerText ?? string.Empty;
                if (strCostElement.StartsWith("Variable("))
                {
                    decimal decMin;
                    decimal decMax = decimal.MaxValue;
                    string strCost = strCostElement.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                    if (strCost.Contains('-'))
                    {
                        string[] strValues = strCost.Split('-');
                        decMin = Convert.ToDecimal(strValues[0], GlobalOptions.InvariantCultureInfo);
                        decMax = Convert.ToDecimal(strValues[1], GlobalOptions.InvariantCultureInfo);
                    }
                    else
                        decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                    if (decMax == decimal.MaxValue)
                        lblCost.Text = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥+";
                    else
                        lblCost.Text = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + " - " + decMax.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                    lblTest.Text = _objCharacter.AvailTest(decMin, lblAvail.Text);
                }
                else
                {
                    string strCost = strCostElement.CheapReplace("Rating", () => nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo))
                        .CheapReplace("Armor Cost", () => _decArmorCost.ToString(GlobalOptions.InvariantCultureInfo));

                    // Apply any markup.
                    objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out blnIsSuccess);
                    decimal decCost = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : 0;
                    decCost *= 1 + (nudMarkup.Value / 100.0m);

                    lblCost.Text = decCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                    lblTest.Text = _objCharacter.AvailTest(decCost, lblAvail.Text);
                }
            }

            // Capacity.
            // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
            string strCapacity = objXmlMod["armorcapacity"]?.InnerText;

            // Handle YNT Softweave
            if (_eCapacityStyle == CapacityStyle.Zero || string.IsNullOrEmpty(strCapacity))
                lblCapacity.Text = "[0]";
            else
            {
                if (strCapacity.StartsWith("FixedValues("))
                {
                    string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strCapacity = strValues[decimal.ToInt32(nudRating.Value) - 1];
                }

                strCapacity = strCapacity.CheapReplace("Capacity", () => _decArmorCapacity.ToString(GlobalOptions.InvariantCultureInfo))
                    .CheapReplace("Rating", () => nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo));
                bool blnSquareBrackets = strCapacity.StartsWith('[');
                if (blnSquareBrackets)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                //Rounding is always 'up'. For items that generate capacity, this means making it a larger negative number.
                objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity, out blnIsSuccess);
                string strReturn = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalOptions.CultureInfo) : strCapacity;
                if (blnSquareBrackets)
                    strReturn = '[' + strReturn + ']';

                lblCapacity.Text = strReturn;
            }

            string strSource = objXmlMod["source"]?.InnerText ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strPage = objXmlMod["altpage"]?.InnerText ?? objXmlMod["page"]?.InnerText ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
            GlobalOptions.ToolTipProcessor.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
        }

        /// <summary>
        /// 
        /// </summary>
        private void BuildModList()
        {
            List<ListItem> lstMods = new List<ListItem>();

            // Populate the Mods list.
            string[] strAllowed = _strAllowedCategories.Split(',');
            string strMount = string.Empty;
            for (int i = 0; i < strAllowed.Length; i++)
            {
                if (!string.IsNullOrEmpty(strAllowed[i]))
                    strMount += "category = \"" + strAllowed[i] + '\"';
                if (i < strAllowed.Length - 1 || !_blnExcludeGeneralCategory)
                {
                    strMount += " or ";
                }
            }
            if (!_blnExcludeGeneralCategory)
            {
                strMount += "category = \"General\"";
            }

            using (XmlNodeList objXmlModList = _objXmlDocument.SelectNodes("/chummer/mods/mod[" + strMount + " and (" + _objCharacter.Options.BookXPath() + ")]"))
                if (objXmlModList?.Count > 0)
                    foreach (XmlNode objXmlMod in objXmlModList)
                    {
                        string strId = objXmlMod["id"]?.InnerText;
                        if (!string.IsNullOrEmpty(strId))
                        {
                            if (!chkHideOverAvailLimit.Checked || SelectionShared.CheckAvailRestriction(objXmlMod, _objCharacter))
                            {
                                lstMods.Add(new ListItem(strId, objXmlMod["translate"]?.InnerText ?? objXmlMod["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language)));
                            }
                        }
                    }
            lstMods.Sort(CompareListItems.CompareNames);
            lstMod.BeginUpdate();
            lstMod.ValueMember = "Value";
            lstMod.DisplayMember = "Name";
            lstMod.DataSource = lstMods;
            lstMod.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstMod.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                _strSelectedArmorMod = strSelectedId;
                _decMarkup = nudMarkup.Value;
                _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
                DialogResult = DialogResult.OK;
            }
        }

        private void MoveControls()
        {
            int intWidth = lblALabel.Width;
            intWidth = Math.Max(intWidth, lblRatingLabel.Width);
            intWidth = Math.Max(intWidth, lblCapacityLabel.Width);
            intWidth = Math.Max(intWidth, lblAvailLabel.Width);
            intWidth = Math.Max(intWidth, lblCostLabel.Width);

            lblA.Left = lblALabel.Left + intWidth + 6;
            nudRating.Left = lblRatingLabel.Left + intWidth + 6;
            lblCapacity.Left = lblCapacityLabel.Left + intWidth + 6;
            lblAvail.Left = lblAvailLabel.Left + intWidth + 6;
            lblTestLabel.Left = lblAvail.Left + lblAvail.Width + 16;
            lblTest.Left = lblTestLabel.Left + lblTestLabel.Width + 6;
            lblCost.Left = lblCostLabel.Left + intWidth + 6;

            nudMarkup.Left = lblMarkupLabel.Left + lblMarkupLabel.Width + 6;
            lblMarkupPercentLabel.Left = nudMarkup.Left + nudMarkup.Width;

            lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;
        }
        #endregion

        private void chkHideOverAvailLimit_CheckedChanged(object sender, EventArgs e)
        {
            BuildModList();
        }
    }
}
