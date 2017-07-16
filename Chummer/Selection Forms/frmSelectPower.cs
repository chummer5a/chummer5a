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
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectPower : Form
    {
        private string _strLimitToPowers;
        private double _dblLimitToRating;

        private readonly Character _objCharacter;

        private XmlDocument _objXmlDocument = new XmlDocument();

        #region Control Events
        public frmSelectPower(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;
            MoveControls();
        }

        private void frmSelectPower_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            List<ListItem> lstPower = new List<ListItem>();

            // Load the Powers information.
            _objXmlDocument = XmlManager.Instance.Load("powers.xml");

            // Populate the Powers list.
            XmlNodeList objXmlPowerList;
            if (!string.IsNullOrEmpty(_strLimitToPowers))
            {
                string strFilter = "(";
                string[] strValue = _strLimitToPowers.Split(',');
                foreach (string strPower in strValue)
                    strFilter += "name = \"" + strPower.Trim() + "\" or ";
                // Remove the trailing " or ".
                strFilter = strFilter.Substring(0, strFilter.Length - 4);
                strFilter += ")";
                objXmlPowerList = _objXmlDocument.SelectNodes("chummer/powers/power[" + strFilter + "]");
            }
            else
            {
                objXmlPowerList = _objXmlDocument.SelectNodes("/chummer/powers/power[" + _objCharacter.Options.BookXPath() + "]");
            }
            foreach (XmlNode objXmlPower in objXmlPowerList)
            {
                bool blnAdd = true;

                double dblPoints = Convert.ToDouble(objXmlPower["points"].InnerText, GlobalOptions.InvariantCultureInfo);
                if (objXmlPower["limit"] != null && !IgnoreLimits)
                {
                    if (_objCharacter.Powers.Count(power => power.Name == objXmlPower["name"].InnerText) >=
                        Convert.ToInt32(objXmlPower["limit"].InnerText))
                    {
                        blnAdd = false;
                    }
                }
                if (objXmlPower["extrapointcost"]?.InnerText != null && blnAdd)
                {
                    //If this power has already had its rating paid for with PP, we don't care about the extrapoints cost. 
                    if (!_objCharacter.Powers.Any(power => power.Name == objXmlPower["name"].InnerText && power.Rating > 0))
                        dblPoints += Convert.ToDouble(objXmlPower["extrapointcost"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                }
                if (_dblLimitToRating > 0 && blnAdd)
                {
                    blnAdd = dblPoints <= _dblLimitToRating;
                }
                if (blnAdd)
                {
                ListItem objItem = new ListItem();
                objItem.Value = objXmlPower["name"].InnerText;
                    objItem.Name = objXmlPower["translate"]?.InnerText ?? objXmlPower["name"].InnerText;
                lstPower.Add(objItem);
                }
            }
            SortListItem objSort = new SortListItem();
            lstPower.Sort(objSort.Compare);
            lstPowers.BeginUpdate();
            lstPowers.DataSource = null;
            lstPowers.ValueMember = "Value";
            lstPowers.DisplayMember = "Name";
            lstPowers.DataSource = lstPower;
            lstPowers.EndUpdate();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstPowers.Text))
                AcceptForm();
        }

        private void lstPowers_DoubleClick(object sender, EventArgs e)
        {
            cmdOK_Click(sender, e);
        }

        private void lstPowers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstPowers.SelectedValue == null)
                return;

            // Display the information for the selected Power.
            XmlNode objXmlPower = _objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + lstPowers.SelectedValue + "\"]");

            lblPowerPoints.Text = objXmlPower["points"].InnerText;
            if (Convert.ToBoolean(objXmlPower["levels"].InnerText))
            {
                lblPowerPoints.Text += $" / {LanguageManager.Instance.GetString("Label_Power_Level")}";
            }
            if (objXmlPower["extrapointcost"] != null)
            {
                lblPowerPoints.Text = objXmlPower["extrapointcost"].InnerText + " + " + lblPowerPoints.Text;
            }

            string strBook = _objCharacter.Options.LanguageBookShort(objXmlPower["source"].InnerText);
            string strPage = objXmlPower["page"].InnerText;
            if (objXmlPower["altpage"] != null)
                strPage = objXmlPower["altpage"].InnerText;
            lblSource.Text = strBook + " " + strPage;

            tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlPower["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            cmdOK_Click(sender, e);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            XmlNodeList objXmlPowerList;
            if (string.IsNullOrEmpty(txtSearch.Text))
                objXmlPowerList = _objXmlDocument.SelectNodes("/chummer/powers/power[" + _objCharacter.Options.BookXPath() + "]");
            else
                objXmlPowerList = _objXmlDocument.SelectNodes("/chummer/powers/power[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))]");

            List<ListItem> lstPower = new List<ListItem>();
            foreach (XmlNode objXmlPower in objXmlPowerList)
            {
                bool blnAdd = true;
                double dblPoints = Convert.ToDouble(objXmlPower["points"].InnerText, GlobalOptions.InvariantCultureInfo);
                dblPoints += Convert.ToDouble(objXmlPower["extrapointcost"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                if (_dblLimitToRating > 0)
                {
                    blnAdd = dblPoints <= _dblLimitToRating;
                }
                if (blnAdd)
                {
                ListItem objItem = new ListItem();
                objItem.Value = objXmlPower["name"].InnerText;
                    objItem.Name = objXmlPower["translate"]?.InnerText ?? objXmlPower["name"].InnerText;
                lstPower.Add(objItem);
            }
            }
            SortListItem objSort = new SortListItem();
            lstPower.Sort(objSort.Compare);
            lstPowers.BeginUpdate();
            lstPowers.DataSource = null;
            lstPowers.ValueMember = "Value";
            lstPowers.DisplayMember = "Name";
            lstPowers.DataSource = lstPower;
            lstPowers.EndUpdate();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstPowers.SelectedIndex + 1 < lstPowers.Items.Count)
                {
                    lstPowers.SelectedIndex++;
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
                    lstPowers.SelectedIndex--;
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
        public string SelectedPower { get; private set; } = "";


        /// <summary>
        /// Only the provided Powers should be shown in the list.
        /// </summary>
        public string LimitToPowers
        {
            set
            {
                _strLimitToPowers = value;
            }
        }

        /// <summary>
        /// Limit the selections based on the Rating of an external source, where 1 Rating = 0.25 PP.
        /// </summary>
        public int LimitToRating 
        {
            set { _dblLimitToRating = value * PointsPerLevel; } 
        }

        /// <summary>
        /// Value of the PP per level if using LimitToRating. Defaults to 0.25.
        /// </summary>
        public double PointsPerLevel { set; get; } = 0.25;

        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            // Check to see if the user needs to select anything for the Power.
            XmlNode objXmlPower = _objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + lstPowers.SelectedValue + "\"]");

            // Make sure the character meets the Quality requirements if any.
            if (objXmlPower.InnerXml.Contains("<required>"))
            {
                string strRequirement = string.Empty;
                bool blnRequirementMet = true;

                // Quality requirements.
                foreach (XmlNode objXmlQuality in objXmlPower.SelectNodes("required/allof/quality"))
                {
                    bool blnFound = _objCharacter.Qualities.Any(objQuality => objQuality.Name == objXmlQuality.InnerText);

                    if (!blnFound)
                    {
                        blnRequirementMet = false;
                        strRequirement += "\n\t" + objXmlQuality.InnerText;
                    }
                }

                foreach (XmlNode objXmlQuality in objXmlPower.SelectNodes("required/oneof/quality"))
                {
                    blnRequirementMet = _objCharacter.Qualities.Any(objQuality => objQuality.Name == objXmlQuality.InnerText);

                    if (!blnRequirementMet)
                        strRequirement += "\n\t" + objXmlQuality.InnerText;
                    else
                        break;
                }

                if (!blnRequirementMet)
                {
                    string strMessage = LanguageManager.Instance.GetString("Message_SelectPower_PowerRequirement");
                    strMessage += strRequirement;

                    MessageBox.Show(strMessage, LanguageManager.Instance.GetString("MessageTitle_SelectPower_PowerRequirement"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            
            SelectedPower = lstPowers.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }

        private void MoveControls()
        {
            lblPowerPoints.Left = lblPowerPointsLabel.Left + lblPowerPointsLabel.Width + 6;
            lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;
            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
        }
        #endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.StaticOpenPDF(lblSource.Text, _objCharacter);
        }
    }
}