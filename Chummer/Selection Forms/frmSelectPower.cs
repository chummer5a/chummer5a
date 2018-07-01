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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectPower : Form
    {
        private bool _blnLoading = true;
        private string _strLimitToPowers;
        private decimal _decLimitToRating;

        private readonly Character _objCharacter;

        private readonly XPathNavigator _xmlBasePowerDataNode;

        #region Control Events
        public frmSelectPower(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            MoveControls();
            // Load the Powers information.
            _xmlBasePowerDataNode = XmlManager.Load("powers.xml").GetFastNavigator().SelectSingleNode("/chummer");
        }

        private void frmSelectPower_Load(object sender, EventArgs e)
        {
            _blnLoading = false;

            BuildPowerList();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void lstPowers_DoubleClick(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void lstPowers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelectedId = lstPowers.SelectedValue?.ToString();
            XPathNavigator objXmlPower = null;
            if (!string.IsNullOrEmpty(strSelectedId))
                objXmlPower = _xmlBasePowerDataNode.SelectSingleNode("powers/power[id = \"" + strSelectedId + "\"]");

            if (objXmlPower != null)
            {
                // Display the information for the selected Power.
                string strPowerPointsText = objXmlPower.SelectSingleNode("points")?.Value ?? string.Empty;
                if (objXmlPower.SelectSingleNode("levels")?.Value == bool.TrueString)
                {
                    strPowerPointsText += $" / {LanguageManager.GetString("Label_Power_Level", GlobalOptions.Language)}";
                }
                string strExtrPointCost = objXmlPower.SelectSingleNode("extrapointcost")?.Value;
                if (!string.IsNullOrEmpty(strExtrPointCost))
                {
                    strPowerPointsText = strExtrPointCost + " + " + strPowerPointsText;
                }
                lblPowerPoints.Text = strPowerPointsText;

                string strSource = objXmlPower.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                string strPage = objXmlPower.SelectSingleNode("altpage")?.Value ?? objXmlPower.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + strSpaceCharacter + strPage);
            }
            else
            {
                lblPowerPoints.Text = string.Empty;
                lblSource.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblSource, string.Empty);
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            AcceptForm();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            BuildPowerList();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstPowers.SelectedIndex + 1 < lstPowers.Items.Count)
                {
                    lstPowers.SelectedIndex += 1;
                }
                else if (lstPowers.Items.Count > 0)
                {
                    lstPowers.SelectedIndex = 0;
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (lstPowers.SelectedIndex - 1 >= 0)
                {
                    lstPowers.SelectedIndex -= 1;
                }
                else if (lstPowers.Items.Count > 0)
                {
                    lstPowers.SelectedIndex = lstPowers.Items.Count - 1;
                }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain { get; private set; }

        /// <summary>
        /// Whether or not we should ignore how many of a given power may be taken. Generally used when bonding Qi Foci.
        /// </summary>
        public bool IgnoreLimits { get; set; }

        /// <summary>
        /// Power that was selected in the dialogue.
        /// </summary>
        public string SelectedPower { get; private set; } = string.Empty;


        /// <summary>
        /// Only the provided Powers should be shown in the list.
        /// </summary>
        public string LimitToPowers
        {
            set => _strLimitToPowers = value;
        }

        /// <summary>
        /// Limit the selections based on the Rating of an external source, where 1 Rating = 0.25 PP.
        /// </summary>
        public int LimitToRating
        {
            set => _decLimitToRating = value * PointsPerLevel;
        }

        /// <summary>
        /// Value of the PP per level if using LimitToRating. Defaults to 0.25.
        /// </summary>
        public decimal PointsPerLevel { set; get; } = 0.25m;

        #endregion

        #region Methods
        private void BuildPowerList()
        {
            if (_blnLoading)
                return;

            string strFilter = "(" + _objCharacter.Options.BookXPath() + ')';
            if (!string.IsNullOrEmpty(_strLimitToPowers))
            {
                StringBuilder objFilter = new StringBuilder();
                foreach (string strPower in _strLimitToPowers.Split(','))
                    if (!string.IsNullOrEmpty(strPower))
                        objFilter.Append("name = \"" + strPower.Trim() + "\" or ");
                if (objFilter.Length > 0)
                {
                    strFilter += " and (" + objFilter.ToString().TrimEndOnce(" or ") + ')';
                }
            }

            strFilter += CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            List<ListItem> lstPower = new List<ListItem>();
            foreach (XPathNavigator objXmlPower in _xmlBasePowerDataNode.Select("powers/power[" + strFilter + "]"))
            {
                decimal decPoints = Convert.ToDecimal(objXmlPower.SelectSingleNode("points")?.Value, GlobalOptions.InvariantCultureInfo);
                string strExtraPointCost = objXmlPower.SelectSingleNode("extrapointcost")?.Value;
                string strName = objXmlPower.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                if (!string.IsNullOrEmpty(strExtraPointCost))
                {
                    //If this power has already had its rating paid for with PP, we don't care about the extrapoints cost.
                    if (!_objCharacter.Powers.Any(power => power.Name == strName && power.TotalRating > 0))
                        decPoints += Convert.ToDecimal(strExtraPointCost, GlobalOptions.InvariantCultureInfo);
                }
                if (_decLimitToRating > 0 && decPoints > _decLimitToRating)
                {
                    continue;
                }

                if (!objXmlPower.RequirementsMet(_objCharacter, null, string.Empty, string.Empty, string.Empty, string.Empty, IgnoreLimits))
                    continue;

                lstPower.Add(new ListItem(objXmlPower.SelectSingleNode("id")?.Value ?? string.Empty, objXmlPower.SelectSingleNode("translate")?.Value ?? strName));
            }
            lstPower.Sort(CompareListItems.CompareNames);
            _blnLoading = true;
            string strOldSelected = lstPowers.SelectedValue?.ToString();
            lstPowers.BeginUpdate();
            lstPowers.ValueMember = "Value";
            lstPowers.DisplayMember = "Name";
            lstPowers.DataSource = lstPower;
            _blnLoading = false;
            if (!string.IsNullOrEmpty(strOldSelected))
                lstPowers.SelectedValue = strOldSelected;
            else
                lstPowers.SelectedIndex = -1;
            lstPowers.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstPowers.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Check to see if the user needs to select anything for the Power.
                XPathNavigator objXmlPower = _xmlBasePowerDataNode.SelectSingleNode("powers/power[id = \"" + strSelectedId + "\"]");

                if (objXmlPower.RequirementsMet(_objCharacter, null, LanguageManager.GetString("String_Power", GlobalOptions.Language), string.Empty, string.Empty, string.Empty, IgnoreLimits))
                {
                    SelectedPower = strSelectedId;
                    DialogResult = DialogResult.OK;
                }
            }
        }

        private void MoveControls()
        {
            lblPowerPoints.Left = lblPowerPointsLabel.Left + lblPowerPointsLabel.Width + 6;
            lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;
            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
        }
        #endregion
    }
}
