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
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Shared_Methods;
using Chummer.Backend.Equipment;
using Chummer.Skills;
using Chummer.Backend.Attributes;

namespace Chummer
{
    public partial class frmSelectQuality : Form
    {
        public int buildPos = 0;
        public int buildNeg = 0;
        private string _strSelectedQuality = string.Empty;
        private bool _blnAddAgain = false;
        private bool _blnLoading = true;
        private readonly Character _objCharacter;

        private XmlDocument _objXmlDocument = new XmlDocument();

        private List<ListItem> _lstCategory = new List<ListItem>();

        private static string _strSelectCategory = string.Empty;

        #region Control Events
        public frmSelectQuality(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;

            MoveControls();
        }

        private void frmSelectQuality_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            // Load the Quality information.
            _objXmlDocument = XmlManager.Instance.Load("qualities.xml");

            // Populate the Quality Category list.
            XmlNodeList objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            foreach (XmlNode objXmlCategory in objXmlCategoryList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objXmlCategory.InnerText;
                objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                _lstCategory.Add(objItem);
            }
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;

            // Select the first Category in the list.
            if (string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = _strSelectCategory;

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            lblBPLabel.Text = LanguageManager.Instance.GetString("Label_Karma");
            _blnLoading = false;
            BuildQualityList();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildQualityList();
        }

        private void lstQualities_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstQualities.Text))
                return;

            XmlNode objXmlQuality = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + lstQualities.SelectedValue + "\"]");
            int intBP = 0;
            if (objXmlQuality["karma"]?.InnerText.StartsWith("Variable") == true)
            {
                int intMin = 0;
                int intMax = int.MaxValue;
                string strCost = objXmlQuality["karma"].InnerText.Replace("Variable(", string.Empty).Replace(")", string.Empty);
                if (strCost.Contains("-"))
                {
                    string[] strValues = strCost.Split('-');
                    int.TryParse(strValues[0], out intMin);
                    int.TryParse(strValues[1], out intMax);
                }
                else
                    int.TryParse(strCost.Replace("+", string.Empty), out intMin);

                if (intMax == int.MaxValue)
                {
                    lblBP.Text = $"{intMin:###,###,##0¥+}";
                }
                else
                    lblBP.Text = $"{intMin:###,###,##0} - {intMax:###,###,##0¥}";

                intBP = intMin;
            }
            else
            {
                int.TryParse(objXmlQuality["karma"]?.InnerText, out intBP);
            }
            bool doubleCostCareer = true;
            if (objXmlQuality["doublecareer"] != null)
            {
                doubleCostCareer = bool.Parse(objXmlQuality["doublecareer"].InnerText);
            }


            if (_objCharacter.Created && !_objCharacter.Options.DontDoubleQualityPurchases && doubleCostCareer)
            {
                intBP *= 2;
            }
            lblBP.Text = (intBP * _objCharacter.Options.KarmaQuality).ToString();
            if (chkFree.Checked)
                lblBP.Text = "0";

            string strBook = _objCharacter.Options.LanguageBookShort(objXmlQuality["source"]?.InnerText);
            string strPage = objXmlQuality["page"]?.InnerText;
            if (objXmlQuality["altpage"] != null)
                strPage = objXmlQuality["altpage"].InnerText;
            lblSource.Text = strBook + " " + strPage;

            tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlQuality["source"]?.InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lstQualities_DoubleClick(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            BuildQualityList();
        }

        private void chkFree_CheckedChanged(object sender, EventArgs e)
        {
            lstQualities_SelectedIndexChanged(sender, e);
        }

        private void chkMetagenetic_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMetagenetic.Checked)
                chkNotMetagenetic.Checked = false;
            BuildQualityList();
        }

        private void chkNotMetagenetic_CheckedChanged(object sender, EventArgs e)
        {
            if (chkNotMetagenetic.Checked)
                chkMetagenetic.Checked = false;
            BuildQualityList();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            BuildQualityList();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstQualities.SelectedIndex + 1 < lstQualities.Items.Count)
                {
                    lstQualities.SelectedIndex++;
                }
                else if (lstQualities.Items.Count > 0)
                {
                    lstQualities.SelectedIndex = 0;
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (lstQualities.SelectedIndex - 1 >= 0)
                {
                    lstQualities.SelectedIndex--;
                }
                else if (lstQualities.Items.Count > 0)
                {
                    lstQualities.SelectedIndex = lstQualities.Items.Count - 1;
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
        /// Quality that was selected in the dialogue.
        /// </summary>
        public string SelectedQuality
        {
            get
            {
                return _strSelectedQuality;
            }
        }

        /// <summary>
        /// Forcefully add a Category to the list.
        /// </summary>
        public string ForceCategory
        {
            set
            {
                cboCategory.BeginUpdate();
                cboCategory.DataSource = null;
                cboCategory.Items.Add(value);
                cboCategory.EndUpdate();
            }
        }

        /// <summary>
        /// A Quality the character has that should be ignored for checking Fobidden requirements (which would prevent upgrading/downgrading a Quality).
        /// </summary>
        public string IgnoreQuality { get; set; } = string.Empty;

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain
        {
            get
            {
                return _blnAddAgain;
            }
        }

        /// <summary>
        /// Whether or not the item has no cost.
        /// </summary>
        public bool FreeCost
        {
            get
            {
                return chkFree.Checked;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Build the list of Qualities.
        /// </summary>
        private void BuildQualityList()
        {
            if (_blnLoading) return;
            List<ListItem> lstQuality = new List<ListItem>();
            XmlDocument objXmlMetatypeDocument = XmlManager.Instance.Load("metatypes.xml");
            XmlDocument objXmlCrittersDocument = XmlManager.Instance.Load("critters.xml");
            if (!string.IsNullOrEmpty(txtSearch.Text.Trim()))
            {
                // Treat everything as being uppercase so the search is case-insensitive.
                string strSearch = "/chummer/qualities/quality[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))";
                if (chkMetagenetic.Checked)
                {
                    strSearch += " and (required/oneof[contains(., 'Changeling (Class I SURGE)')] or metagenetic = 'yes')";
                }
                else if (chkNotMetagenetic.Checked)
                {
                    strSearch += " and not (metagenetic = 'yes')";
                }
                if (nudMinimumBP.Value != 0)
                {
                    strSearch += "and karma => " + nudMinimumBP.Value;
                }
                strSearch += "]";

                bool blnNeedQualityWhitelist = false;
                XmlNode objXmlMetatype = objXmlMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
                if (objXmlMetatype?.SelectNodes("qualityrestriction")?.Count > 0)
                    blnNeedQualityWhitelist = true;
                else
                {
                    objXmlMetatype = objXmlCrittersDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
                    if (objXmlMetatype?.SelectNodes("qualityrestriction")?.Count > 0)
                        blnNeedQualityWhitelist = true;
                }

                XmlNodeList objXmlQualityList;
                try
                {
                    objXmlQualityList = _objXmlDocument.SelectNodes(strSearch);
                }
                catch (XPathException)
                {
                    return;
                }

                foreach (XmlNode objXmlQuality in objXmlQualityList)
                {
                    bool blnQualityAllowed = !blnNeedQualityWhitelist;
                    if (blnNeedQualityWhitelist)
                    {
                        if (objXmlQuality["name"] != null && (
                            objXmlMetatype.SelectSingleNode("qualityrestriction/positive/quality[. = \"" +
                                                            objXmlQuality["name"].InnerText + "\"]") != null ||
                            objXmlMetatype.SelectSingleNode("qualityrestriction/negative/quality[. = \"" +
                                                            objXmlQuality["name"].InnerText + "\"]") != null))
                            blnQualityAllowed = true;
                    }

                    if (objXmlQuality["hide"] == null && blnQualityAllowed)
                    {
                        if (!chkLimitList.Checked || chkLimitList.Checked && SelectionShared.RequirementsMet(objXmlQuality, false, _objCharacter, objXmlMetatypeDocument, objXmlCrittersDocument, _objXmlDocument, IgnoreQuality, LanguageManager.Instance.GetString("String_Quality")))
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objXmlQuality["name"]?.InnerText;
                            objItem.Name = objXmlQuality["translate"]?.InnerText ?? objItem.Value;

                            if (objXmlQuality["category"] != null)
                            {
                                ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value == objXmlQuality["category"].InnerText);
                                if (objFoundItem != null)
                                {
                                    objItem.Name += " [" + objFoundItem.Name + "]";
                                }
                            }

                            lstQuality.Add(objItem);
                        }
                    }
                }
            }
            else
            {

                string strXPath = "category = \"" + cboCategory.SelectedValue + "\" and (" + _objCharacter.Options.BookXPath() + ")";
                if (chkMetagenetic.Checked)
                {
                    strXPath += " and (required/oneof[contains(., 'Changeling (Class I SURGE)')] or metagenetic = 'yes')";
                }
                else if (!chkNotMetagenetic.Checked && (cboCategory.SelectedValue.ToString() == "Negative" || _objCharacter.MetageneticLimit > 0))
                {
                    //Load everything, including metagenetic qualities.
                }
                else
                {
                    strXPath += " and not (required/oneof[contains(., 'Changeling (Class I SURGE)')] or metagenetic = 'yes')";
                }
                if (nudValueBP.Value != 0)
                {
                    strXPath += "and karma = " + nudValueBP.Value;
                }
                else
                {
                    if (nudMinimumBP.Value != 0)
                    {
                        strXPath += "and karma >= " + nudMinimumBP.Value;
                    }

                    if (nudMaximumBP.Value != 0)
                    {
                        strXPath += "and karma <= " + nudMaximumBP.Value;
                    }
                }

                bool blnNeedQualityWhitelist = false;
                XmlNode objXmlMetatype = objXmlMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
                if (objXmlMetatype?.SelectNodes("qualityrestriction")?.Count > 0)
                    blnNeedQualityWhitelist = true;
                else
                {
                    objXmlMetatype = objXmlCrittersDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _objCharacter.Metatype + "\"]");
                    if (objXmlMetatype?.SelectNodes("qualityrestriction")?.Count > 0)
                        blnNeedQualityWhitelist = true;
                }

                foreach (XmlNode objXmlQuality in _objXmlDocument.SelectNodes("/chummer/qualities/quality[" + strXPath + "]"))
                {
                    if (objXmlQuality["name"] == null)
                        continue;
                    if (objXmlQuality["name"].InnerText.StartsWith("Infected"))
                    {
                        //There was something I was going to do with this, but I can't remember what it was.
                    }
                    bool blnQualityAllowed = !blnNeedQualityWhitelist;
                    if (blnNeedQualityWhitelist)
                    {
                        if (objXmlMetatype.SelectSingleNode("qualityrestriction/" + cboCategory.SelectedValue.ToString().ToLower() + "/quality[. = \"" + objXmlQuality["name"].InnerText + "\"]") != null)
                            blnQualityAllowed = true;
                    }
                    if (blnQualityAllowed)
                    {
                        if (!chkLimitList.Checked || chkLimitList.Checked && SelectionShared.RequirementsMet(objXmlQuality, false, _objCharacter, objXmlMetatypeDocument, objXmlCrittersDocument, _objXmlDocument, IgnoreQuality, LanguageManager.Instance.GetString("String_Quality")))
                        {
                            if (objXmlQuality["hide"] == null)
                            {
                                ListItem objItem = new ListItem();
                                objItem.Value = objXmlQuality["name"].InnerText;
                                objItem.Name = objXmlQuality["translate"]?.InnerText ?? objXmlQuality["name"].InnerText;

                                lstQuality.Add(objItem);
                            }
                        }
                    }
                }
            }
            SortListItem objSort = new SortListItem();
            lstQuality.Sort(objSort.Compare);
            lstQualities.BeginUpdate();
            lstQualities.DataSource = null;
            lstQualities.ValueMember = "Value";
            lstQualities.DisplayMember = "Name";
            lstQualities.DataSource = lstQuality;
            lstQualities.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            if (string.IsNullOrEmpty(lstQualities.Text))
                return;
            //Test for whether we're adding a "Special" quality. This should probably be a separate function at some point.
            switch (lstQualities.SelectedValue.ToString())
            {
                case "Changeling (Class I SURGE)":
                    _objCharacter.MetageneticLimit = 30;
                    break;
                case "Changeling (Class II SURGE)":
                    _objCharacter.MetageneticLimit = 30;
                    break;
                case "Changeling (Class III SURGE)":
                    _objCharacter.MetageneticLimit = 30;
                    break;
                default:
                    break;
            }

            XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + lstQualities.SelectedValue + "\"]");
            if (objNode == null)
                return;
            _strSelectedQuality = objNode["name"]?.InnerText;
            _strSelectCategory = objNode["category"]?.InnerText;

            if (!SelectionShared.RequirementsMet(objNode, true, _objCharacter, null, null, _objXmlDocument, IgnoreQuality, LanguageManager.Instance.GetString("String_Quality")))
                return;
            DialogResult = DialogResult.OK;
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblBPLabel.Width, lblSourceLabel.Width);
            lblBP.Left = lblBPLabel.Left + intWidth + 6;
            lblSource.Left = lblSourceLabel.Left + intWidth + 6;

            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
        }
        #endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.StaticOpenPDF(lblSource.Text, _objCharacter);
        }

        private void KarmaFilter(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nudMinimumBP.Text))
            {
                nudMinimumBP.Value = 0;
            }
            if (string.IsNullOrWhiteSpace(nudValueBP.Text))
            {
                nudValueBP.Value = 0;
            }
            if (string.IsNullOrWhiteSpace(nudMaximumBP.Text))
            {
                nudMaximumBP.Value = 0;
            }
            BuildQualityList();
        }
    }
}
