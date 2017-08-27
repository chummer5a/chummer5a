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
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;
// ReSharper disable LocalizableElement

namespace Chummer
{
    public partial class frmSelectWeapon : Form
    {
        private string _strSelectedWeapon = string.Empty;
        private int _intMarkup;

        private bool _blnAddAgain;
        private bool _blnBlackMarketDiscount;
        private string[] _strLimitToCategories = new string[0];
        private static string _strSelectCategory = string.Empty;
        private readonly Character _objCharacter;
        private XmlNodeList _objXmlCategoryList;
        private XmlDocument _objXmlDocument = new XmlDocument();

        private List<ListItem> _lstCategory = new List<ListItem>();

        #region Control Events
        public frmSelectWeapon(Character objCharacter, bool blnCareer = false)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            lblMarkupLabel.Visible = blnCareer;
            nudMarkup.Visible = blnCareer;
            lblMarkupPercentLabel.Visible = blnCareer;
            _objCharacter = objCharacter;
            MoveControls();
        }

        private void frmSelectWeapon_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>().Where(objLabel => objLabel.Text.StartsWith("[")))
            {
                objLabel.Text = string.Empty;
            }
            chkHideOverAvailLimit.Text = chkHideOverAvailLimit.Text.Replace("{0}",
                    _objCharacter.Options.Availability.ToString());
            chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;

            // Load the Weapon information.
            _objXmlDocument = XmlManager.Instance.Load("weapons.xml");

            // Populate the Weapon Category list.
            if (_strLimitToCategories.Length > 0)
            {
                // Populate the Category list.
                XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/categories/category");
                if (objXmlNodeList != null)
                    foreach (XmlNode objXmlCategory in objXmlNodeList)
                    {
                        foreach (ListItem objItem in from strCategory in _strLimitToCategories where strCategory == objXmlCategory.InnerText select new ListItem())
                        {
                            objItem.Value = objXmlCategory.InnerText;
                            objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                            _lstCategory.Add(objItem);
                        }
                    }
            }
            else
            {
                _objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");

                if (_objXmlCategoryList != null)
                    foreach (XmlNode objXmlCategory in _objXmlCategoryList)
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

            // Select the first Category in the list.
            if (string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = _strSelectCategory;

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            XmlNodeList objXmlWeaponList = _objXmlDocument.SelectNodes("/chummer/weapons/weapon[category = \"" + cboCategory.SelectedValue + "\" and (" + _objCharacter.Options.BookXPath() + ")]");
            BuildWeaponList(objXmlWeaponList);
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Populate the Weapon list.
            XmlNodeList objXmlWeaponList = _objXmlDocument.SelectNodes("/chummer/weapons/weapon[category = \"" + cboCategory.SelectedValue + "\" and (" + _objCharacter.Options.BookXPath() + ")]");

            BuildWeaponList(objXmlWeaponList);
        }

        private void lstWeapon_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lstWeapon.Text))
                return;

            // Retireve the information for the selected Weapon.
            XmlNode objXmlWeapon = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + lstWeapon.SelectedValue + "\"]");
            if (objXmlWeapon == null)
                return;

            Weapon objWeapon = new Weapon(_objCharacter);
            TreeNode objNode = new TreeNode();
            objWeapon.Create(objXmlWeapon, _objCharacter, objNode, null, null);

            lblWeaponReach.Text = objWeapon.TotalReach.ToString();
            lblWeaponDamage.Text = objWeapon.CalculatedDamage();
            lblWeaponAP.Text = objWeapon.TotalAP;
            lblWeaponMode.Text = objWeapon.CalculatedMode;
            lblWeaponRC.Text = objWeapon.TotalRC;
            lblWeaponAmmo.Text = objWeapon.CalculatedAmmo();
            lblWeaponAccuracy.Text = objWeapon.TotalAccuracy;
            lblWeaponAvail.Text = objWeapon.TotalAvail;

            int intItemCost = 0;
            double dblCost = 0;
            if (objXmlWeapon["cost"] != null)
            {
                if (objXmlWeapon["cost"].InnerText.StartsWith("Variable"))
                {
                    int intMin;
                    int intMax = int.MaxValue;
                    string strCost = objXmlWeapon["cost"].InnerText.Replace("Variable", string.Empty).Trim("()".ToCharArray());
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
                        lblWeaponCost.Text = $"{intMin:###,###,##0¥+}";
                    }
                    else
                        lblWeaponCost.Text = $"{intMin:###,###,##0} - {intMax:###,###,##0¥}";

                    intItemCost = intMin;
                }
                else
                {
                    objXmlWeapon.TryGetDoubleFieldQuickly("cost", ref dblCost);
                    dblCost *= 1 + (Convert.ToDouble(nudMarkup.Value, GlobalOptions.CultureInfo) / 100.0);
                    if (chkBlackMarketDiscount.Checked)
                    {
                        dblCost = dblCost * 0.90;
                    }
                    lblWeaponCost.Text = $"{dblCost:###,###,##0¥}";
                    intItemCost = Convert.ToInt32(Math.Ceiling(dblCost));

                    if (chkFreeItem.Checked)
                    {
                        lblWeaponCost.Text = $"{0:###,###,##0¥}";
                        intItemCost = 0;
                    }
                }
            }

            lblTest.Text = _objCharacter.AvailTest(intItemCost, lblWeaponAvail.Text);

            string strBook = _objCharacter.Options.LanguageBookShort(objXmlWeapon["source"]?.InnerText);
            string strPage = objXmlWeapon["page"]?.InnerText;
            if (objXmlWeapon["altpage"] != null)
                strPage = objXmlWeapon["altpage"].InnerText;
            lblSource.Text = strBook + " " + strPage;

            // Build a list of included Accessories and Modifications that come with the weapon.
            string strAccessories = string.Empty;
            XmlNodeList objXmlNodeList = objXmlWeapon.SelectNodes("accessories/accessory");
            if (objXmlNodeList != null)
                foreach (XmlNode objXmlAccessory in objXmlNodeList)
                {
                    if (objXmlAccessory["name"] != null)
                    {
                        XmlNode objXmlItem =
                            _objXmlDocument.SelectSingleNode("/chummer/accessories/accessory[name = \"" +
                                                             objXmlAccessory["name"].InnerText + "\"]");
                        if (objXmlItem?["name"] != null)
                            strAccessories += objXmlItem["translate"] != null
                                ? objXmlItem["translate"].InnerText + "\n"
                                : objXmlItem["name"].InnerText + "\n";
                    }
                }
            lblIncludedAccessories.Text = string.IsNullOrEmpty(strAccessories) ? LanguageManager.Instance.GetString("String_None") : strAccessories;

            tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlWeapon["source"]?.InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
        }

        private void BuildWeaponList(XmlNodeList objNodeList)
        {
            if (tabControl.SelectedIndex == 1)
            {
                DataTable tabWeapons = new DataTable("weapons");
                tabWeapons.Columns.Add("WeaponGuid");
                tabWeapons.Columns.Add("WeaponName");
                tabWeapons.Columns.Add("Dice");
                tabWeapons.Columns.Add("Accuracy");
                tabWeapons.Columns["Accuracy"].DataType = typeof(Int32);
                tabWeapons.Columns.Add("Damage");
                tabWeapons.Columns.Add("AP");
                tabWeapons.Columns.Add("RC");
                tabWeapons.Columns["RC"].DataType = typeof(Int32);
                tabWeapons.Columns.Add("Ammo");
                tabWeapons.Columns.Add("Mode");
                tabWeapons.Columns.Add("Reach");
                tabWeapons.Columns.Add("Accessories");
                tabWeapons.Columns.Add("Avail");
                tabWeapons.Columns.Add("Source");
                tabWeapons.Columns.Add("Cost");
                tabWeapons.Columns["Cost"].DataType = typeof(Int32);

                foreach (XmlNode objXmlWeapon in objNodeList)
                {
                    bool blnHide = objXmlWeapon["cyberware"]?.InnerText == "yes" || objXmlWeapon["hide"]?.InnerText == "yes";

                    if (objXmlWeapon["mount"] != null && !blnHide)
                    {
                        blnHide = !Mounts.Contains(objXmlWeapon["mount"].InnerText);
                    }
                    if (objXmlWeapon["extramount"] != null && !blnHide)
                    {
                        blnHide = !Mounts.Contains(objXmlWeapon["extramount"].InnerText);
                    }
                    if (!blnHide && !Backend.Shared_Methods.SelectionShared.CheckAvailRestriction(objXmlWeapon, _objCharacter, chkHideOverAvailLimit.Checked))
                    {
                        continue;
                    }
                    if (!blnHide)
                    {
                        TreeNode objNode = new TreeNode();
                        Weapon objWeapon = new Weapon(_objCharacter);
                        objWeapon.Create(objXmlWeapon, _objCharacter, objNode, null, null);

                        string strID = objWeapon.SourceID.ToString();
                        string strWeaponName = objWeapon.DisplayName;
                        string strDice = objWeapon.DicePool;
                        int intAccuracy = Convert.ToInt32(objWeapon.TotalAccuracy);
                        string strDamage = objWeapon.CalculatedDamage(_objCharacter.STR.Augmented);
                        string strAP = objWeapon.TotalAP;
                        if (strAP == "-")
                            strAP = "0";
                        int intRC;
                        int.TryParse(objWeapon.TotalRC, out intRC);
                        string strAmmo = objWeapon.Ammo;
                        string strMode = objWeapon.Mode;
                        string strReach = objWeapon.TotalReach.ToString();
                        string strAccessories = string.Empty;
                        foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                        {
                            if (strAccessories.Length > 0)
                                strAccessories += "\n";
                            strAccessories += objAccessory.DisplayName;
                        }
                        string strAvail = objWeapon.TotalAvail;
                        string strSource = objWeapon.Source + " " + objWeapon.Page;
                        int intCost = objWeapon.Cost;

                        tabWeapons.Rows.Add(strID,strWeaponName, strDice, intAccuracy, strDamage, strAP, intRC, strAmmo, strMode, strReach, strAccessories, strAvail, strSource, intCost);
                    }
                }

                DataSet set = new DataSet("weapons");
                set.Tables.Add(tabWeapons);

                if (cboCategory.SelectedValue.ToString() == "Blades" || cboCategory.SelectedValue.ToString() == "Clubs" || cboCategory.SelectedValue.ToString() == "Improvised Weapons" || cboCategory.SelectedValue.ToString() == "Exotic Melee Weapons" || cboCategory.SelectedValue.ToString() == "Unarmed")
                {
                    dgvWeapons.Columns[5].Visible = false;
                    dgvWeapons.Columns[6].Visible = false;
                    dgvWeapons.Columns[7].Visible = false;
                    dgvWeapons.Columns[8].Visible = true;
                }
                else
                {
                    dgvWeapons.Columns[5].Visible = true;
                    dgvWeapons.Columns[6].Visible = true;
                    dgvWeapons.Columns[7].Visible = true;
                    dgvWeapons.Columns[8].Visible = false;
                }
                dgvWeapons.Columns[0].Visible = false;
                dgvWeapons.Columns[12].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
                dgvWeapons.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                dgvWeapons.DataSource = set;
                dgvWeapons.DataMember = "weapons";
            }
            else
            {
                List<ListItem> lstWeapons = new List<ListItem>();
                foreach (XmlNode objXmlWeapon in objNodeList)
                {
                    bool blnHide = objXmlWeapon["cyberware"]?.InnerText == "yes" || objXmlWeapon["hide"]?.InnerText == "yes";

                    if (objXmlWeapon["mount"] != null && !blnHide)
                    {
                        blnHide = !Mounts.Contains(objXmlWeapon["mount"].InnerText);
                    }
                    if (objXmlWeapon["extramount"] != null && !blnHide)
                    {
                        blnHide = !Mounts.Contains(objXmlWeapon["extramount"].InnerText);
                    }
                    if (!blnHide && !Backend.Shared_Methods.SelectionShared.CheckAvailRestriction(objXmlWeapon, _objCharacter, chkHideOverAvailLimit.Checked))
                    {
                        continue;
                    }
                    if (!Backend.Shared_Methods.SelectionShared.CheckAvailRestriction(objXmlWeapon, _objCharacter,chkHideOverAvailLimit.Checked))
                    {
                        continue;
                    }
                    if (blnHide) continue;
                    ListItem objItem = new ListItem
                    {
                        Value = objXmlWeapon["id"]?.InnerText,
                        Name = objXmlWeapon["translate"]?.InnerText ?? objXmlWeapon["name"]?.InnerText
                    };
                    lstWeapons.Add(objItem);
                }
                SortListItem objSort = new SortListItem();
                lstWeapons.Sort(objSort.Compare);
                lstWeapon.BeginUpdate();
                lstWeapon.DataSource = null;
                lstWeapon.ValueMember = "Value";
                lstWeapon.DisplayMember = "Name";
                lstWeapon.DataSource = lstWeapons;
                lstWeapon.EndUpdate();
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                cboCategory_SelectedIndexChanged(sender, e);
                return;
            }

            string strCategoryFilter = string.Empty;

            foreach (object objListItem in cboCategory.Items)
            {
                ListItem objItem = (ListItem)objListItem;
                if (!string.IsNullOrEmpty(objItem.Value))
                    strCategoryFilter += "category = \"" + objItem.Value + "\" or ";
            }

            // Treat everything as being uppercase so the search is case-insensitive.
            string strSearch = "/chummer/weapons/weapon[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))";
            if (!string.IsNullOrEmpty(strCategoryFilter))
                strSearch += " and (" + strCategoryFilter + ")";
            // Remove the trailing " or )";
            if (strSearch.EndsWith(" or )"))
            {
                strSearch = strSearch.Substring(0, strSearch.Length - 4) + ")";
            }
            strSearch += "]";

            XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes(strSearch);
            BuildWeaponList(objXmlNodeList);
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            lstWeapon_SelectedIndexChanged(sender, e);
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            lstWeapon_SelectedIndexChanged(sender, e);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstWeapon.SelectedIndex + 1 < lstWeapon.Items.Count)
                {
                    lstWeapon.SelectedIndex++;
                }
                else if (lstWeapon.Items.Count > 0)
                {
                    lstWeapon.SelectedIndex = 0;
                }
                if (dgvWeapons.SelectedRows.Count > 0 && dgvWeapons.Rows.Count > dgvWeapons.SelectedRows[0].Index + 1)
                {
                    dgvWeapons.Rows[dgvWeapons.SelectedRows[0].Index + 1].Selected = true;
                }
                else if (dgvWeapons.Rows.Count > 0)
                {
                    dgvWeapons.Rows[0].Selected = true;
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (lstWeapon.SelectedIndex - 1 >= 0)
                {
                    lstWeapon.SelectedIndex--;
                }
                else if (lstWeapon.Items.Count > 0)
                {
                    lstWeapon.SelectedIndex = lstWeapon.Items.Count - 1;
                }
                if (dgvWeapons.SelectedRows.Count > 0 && dgvWeapons.Rows.Count > dgvWeapons.SelectedRows[0].Index - 1)
                {
                    dgvWeapons.Rows[dgvWeapons.SelectedRows[0].Index - 1].Selected = true;
                }
                else if (dgvWeapons.Rows.Count > 0)
                {
                    dgvWeapons.Rows[0].Selected = true;
                }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            lstWeapon_SelectedIndexChanged(sender, e);
        }
        #endregion

        #region Properties
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
        /// Whether or not the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount
        {
            get
            {
                return _blnBlackMarketDiscount;
            }
        }

        /// <summary>
        /// Name of Weapon that was selected in the dialogue.
        /// </summary>
        public string SelectedWeapon
        {
            get
            {
                return _strSelectedWeapon;
            }
        }

        /// <summary>
        /// Whether or not the item should be added for free.
        /// </summary>
        public bool FreeCost
        {
            get
            {
                return chkFreeItem.Checked;
            }
        }

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public int Markup
        {
            get
            {
                return _intMarkup;
            }
        }

        /// <summary>
        /// Only the provided Weapon Categories should be shown in the list.
        /// </summary>
        public string LimitToCategories
        {
            set
            {
                _strLimitToCategories = value.Split(',');
            }
        }

        public bool Underbarrel { get; set; }
        public string Mounts { get; set; } = string.Empty;
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            XmlNode objNode;
            switch (tabControl.SelectedIndex)
            {
                case 0:
                    objNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + lstWeapon.SelectedValue + "\"]");
                    if (objNode != null)
                    {
                        _strSelectCategory = objNode["category"]?.InnerText;
                        _strSelectedWeapon = objNode["id"]?.InnerText;
                        _intMarkup = Convert.ToInt32(nudMarkup.Value);
                        _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;

                        DialogResult = DialogResult.OK;
                    }
                    break;
                case 1:
                    if (dgvWeapons.SelectedRows.Count == 1)
                    {
                        if (txtSearch.Text.Length > 1)
                        {
                            string strWeapon = dgvWeapons.SelectedRows[0].Cells[0].Value.ToString();
                            if (!string.IsNullOrEmpty(strWeapon))
                                strWeapon = strWeapon.Substring(0, strWeapon.LastIndexOf("(", StringComparison.Ordinal) - 1);
                            objNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + strWeapon + "\"]");
                        }
                        else
                        {
                            objNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + dgvWeapons.SelectedRows[0].Cells[0].Value + "\"]");
                        }
                        if (objNode != null)
                        {
                            _strSelectCategory = objNode["category"]?.InnerText;
                            _strSelectedWeapon = objNode["id"]?.InnerText;
                        }
                        _intMarkup = Convert.ToInt32(nudMarkup.Value);

                        DialogResult = DialogResult.OK;
                    }
                    break;
            }
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblWeaponDamageLabel.Width, lblWeaponAPLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponReachLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponAvailLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponCostLabel.Width);

            lblWeaponDamage.Left = lblWeaponDamageLabel.Left + intWidth + 6;
            lblWeaponAP.Left = lblWeaponAPLabel.Left + intWidth + 6;
            lblWeaponReach.Left = lblWeaponReachLabel.Left + intWidth + 6;
            lblWeaponAvail.Left = lblWeaponAvailLabel.Left + intWidth + 6;
            lblWeaponCost.Left = lblWeaponCostLabel.Left + intWidth + 6;

            lblWeaponRCLabel.Left = lblWeaponAP.Left + 74;
            lblWeaponRC.Left = lblWeaponRCLabel.Left + lblWeaponRCLabel.Width + 6;

            intWidth = Math.Max(lblWeaponAmmoLabel.Width, lblWeaponModeLabel.Width);
            intWidth = Math.Max(intWidth, lblTestLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponAccuracy.Width);
            lblWeaponAmmoLabel.Left = lblWeaponAP.Left + 74;
            lblWeaponAmmo.Left = lblWeaponAmmoLabel.Left + intWidth + 6;
            lblWeaponModeLabel.Left = lblWeaponAP.Left + 74;
            lblWeaponMode.Left = lblWeaponModeLabel.Left + intWidth + 6;
            lblTestLabel.Left = lblWeaponAP.Left + 74;
            lblTest.Left = lblTestLabel.Left + intWidth + 6;
            lblWeaponAccuracyLabel.Left = lblWeaponAP.Left + 74;
            lblWeaponAccuracy.Left = lblWeaponAccuracyLabel.Left + intWidth + 6;

            nudMarkup.Left = lblMarkupLabel.Left + lblMarkupLabel.Width + 6;
            lblMarkupPercentLabel.Left = nudMarkup.Left + nudMarkup.Width;

            lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;

            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
        }

        private void tmrSearch_Tick(object sender, EventArgs e)
        {
            tmrSearch.Stop();
            tmrSearch.Enabled = false;

            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                cboCategory_SelectedIndexChanged(sender, e);
                return;
            }
            string strCategoryLimit = string.Empty;
            if (_strLimitToCategories.Length > 0)
            {
                strCategoryLimit += "category = \"" + _strLimitToCategories[0] + "\"";
                for (int i = 1; i < _strLimitToCategories.Length; i++)
                {
                    strCategoryLimit += " or category = \"" + _strLimitToCategories[i] + "\"";
                }
            }
            else
            {
                strCategoryLimit = "category != \"Cyberware\" and category != \"Gear\"";
            }
            // Treat everything as being uppercase so the search is case-insensitive.
            string strSearch = "/chummer/weapons/weapon[(" + _objCharacter.Options.BookXPath() + ") and (" + strCategoryLimit + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))]";

            XmlNodeList objXmlWeaponList = _objXmlDocument.SelectNodes(strSearch);
            BuildWeaponList(objXmlWeaponList);
        }

        private void dgvWeapons_DoubleClick(object sender, EventArgs e)
        {
            cmdOK_Click(sender, e);
        }

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.StaticOpenPDF(lblSource.Text, _objCharacter);
        }
        #endregion
    }
}
