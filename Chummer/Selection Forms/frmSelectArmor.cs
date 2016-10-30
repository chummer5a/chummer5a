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
using System.Xml.XPath;
 using Chummer.Backend.Equipment;

namespace Chummer
{
	public partial class frmSelectArmor : Form
	{
		private string _strSelectedArmor = "";

		private bool _blnAddAgain = false;
		private static string _strSelectCategory = "";
		private int _intMarkup = 0;

		private XmlDocument _objXmlDocument = new XmlDocument();
		private readonly Character _objCharacter;

		private List<ListItem> _lstCategory = new List<ListItem>();
		private int _intRating;
		private bool _blnBlackMarketDiscount;

		#region Control Events
		public frmSelectArmor(Character objCharacter, bool blnCareer = false)
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			lblMarkupLabel.Visible = blnCareer;
			nudMarkup.Visible = blnCareer;
			lblMarkupPercentLabel.Visible = blnCareer;
			_objCharacter = objCharacter;
			MoveControls();
		}

		private void frmSelectArmor_Load(object sender, EventArgs e)
		{
			foreach (Label objLabel in this.Controls.OfType<Label>())
			{
				if (objLabel.Text.StartsWith("["))
					objLabel.Text = "";
			}

			// Load the Armor information.
			_objXmlDocument = XmlManager.Instance.Load("armor.xml");

			// Populate the Armor Category list.
			XmlNodeList objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
			foreach (XmlNode objXmlCategory in objXmlCategoryList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlCategory.InnerText;
				if (objXmlCategory.Attributes != null)
				{
					if (objXmlCategory.Attributes["translate"] != null)
						objItem.Name = objXmlCategory.Attributes["translate"].InnerText;
					else
						objItem.Name = objXmlCategory.InnerText;
				}
				else
					objItem.Name = objXmlCategory.InnerXml;
				_lstCategory.Add(objItem);
			}
			cboCategory.ValueMember = "Value";
			cboCategory.DisplayMember = "Name";
			cboCategory.DataSource = _lstCategory;
			chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;
			// Select the first Category in the list.
			if (_strSelectCategory == "")
				cboCategory.SelectedIndex = 0;
			else
				cboCategory.SelectedValue = _strSelectCategory;

			if (cboCategory.SelectedIndex == -1)
				cboCategory.SelectedIndex = 0;

            if (chkBrowse.Checked)
                LoadGrid();
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			AcceptForm();
		}

		private void lstArmor_DoubleClick(object sender, EventArgs e)
		{
			AcceptForm();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void lstArmor_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lstArmor.Text == "")
				return;

			// Get the information for the selected piece of Armor.
			XmlNode objXmlArmor = _objXmlDocument.SelectSingleNode("/chummer/armors/armor[name = \"" + lstArmor.SelectedValue + "\"]");
			// Create the Armor so we can show its Total Avail (some Armor includes Chemical Seal which adds +6 which wouldn't be factored in properly otherwise).
			Armor objArmor = new Armor(_objCharacter);
			TreeNode objNode = new TreeNode();
			List<Weapon> objWeapons = new List<Weapon>();
			objArmor.Create(objXmlArmor, objNode, null, 0, objWeapons, true);

			lblArmor.Text = objXmlArmor["name"].InnerText;
            lblArmorValue.Text = objXmlArmor["armor"].InnerText;
			lblAvail.Text = objArmor.TotalAvail;

			if (objXmlArmor["rating"] != null)
			{
				lblRatingLabel.Visible = true;
				nudRating.Visible = true;
				nudRating.Enabled = true;
				nudRating.Maximum = Convert.ToInt32(objXmlArmor["rating"].InnerText);
				nudRating.Minimum = 1;
				nudRating.Value = 1;
			}
			else
			{
				lblRatingLabel.Visible = false;
				nudRating.Visible = false;
				nudRating.Enabled = false;
				nudRating.Minimum = 0;
				nudRating.Value = 0;
			}
			UpdateArmorInfo();
		}

		private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
		{
			List<ListItem> lstArmors = new List<ListItem>();

			// Populate the Armor list.
			XmlNodeList objXmlArmorList = _objXmlDocument.SelectNodes("/chummer/armors/armor[category = \"" + cboCategory.SelectedValue + "\" and (" + _objCharacter.Options.BookXPath() + ")]");
			foreach (XmlNode objXmlArmor in objXmlArmorList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlArmor["name"].InnerText;
				if (objXmlArmor["translate"] != null)
					objItem.Name = objXmlArmor["translate"].InnerText;
				else
					objItem.Name = objXmlArmor["name"].InnerText;
				lstArmors.Add(objItem);
			}
			SortListItem objSort = new SortListItem();
			lstArmors.Sort(objSort.Compare);
			lstArmor.DataSource = null;
			lstArmor.ValueMember = "Value";
			lstArmor.DisplayMember = "Name";
			lstArmor.DataSource = lstArmors;

            if (chkBrowse.Checked)
                LoadGrid();
		}

		private void cmdOKAdd_Click(object sender, EventArgs e)
		{
			_blnAddAgain = true;
			cmdOK_Click(sender, e);
		}

		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
			if (txtSearch.Text == "")
			{
				cboCategory_SelectedIndexChanged(sender, e);
				return;
			}

			string strCategoryFilter = "";

			foreach (object objListItem in cboCategory.Items)
			{
				ListItem objItem = (ListItem)objListItem;
				if (objItem.Value != "")
					strCategoryFilter += "category = \"" + objItem.Value + "\" or ";
			}

			// Treat everything as being uppercase so the search is case-insensitive.
			string strSearch = "/chummer/armors/armor[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))";
			if (strCategoryFilter != "")
				strSearch += " and (" + strCategoryFilter + ")";
			// Remove the trailing " or )";
			if (strSearch.EndsWith(" or )"))
			{
				strSearch = strSearch.Substring(0, strSearch.Length - 4) + ")";
			}
			strSearch += "]";

			XmlNodeList objXmlArmorList = _objXmlDocument.SelectNodes(strSearch);
			List<ListItem> lstArmors = new List<ListItem>();
			foreach (XmlNode objXmlArmor in objXmlArmorList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlArmor["name"].InnerText;
				if (objXmlArmor["translate"] != null)
					objItem.Name = objXmlArmor["translate"].InnerText;
				else
					objItem.Name = objXmlArmor["name"].InnerText;

				try
				{
					objItem.Name += " [" + _lstCategory.Find(objFind => objFind.Value == objXmlArmor["category"].InnerText).Name + "]";
					lstArmors.Add(objItem);
				}
				catch
				{
				}
			}
			SortListItem objSort = new SortListItem();
			lstArmors.Sort(objSort.Compare);
			lstArmor.DataSource = null;
			lstArmor.ValueMember = "Value";
			lstArmor.DisplayMember = "Name";
			lstArmor.DataSource = lstArmors;
		}

		private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
		{
			UpdateArmorInfo();
		}

		private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
		{
			UpdateArmorInfo();
		}

		private void nudRating_ValueChanged(object sender, EventArgs e)
		{
			UpdateArmorInfo();
		}

		private void nudMarkup_ValueChanged(object sender, EventArgs e)
		{
			UpdateArmorInfo();
		}

		private void txtSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
			{
				try
				{
					lstArmor.SelectedIndex++;
				}
				catch
				{
					try
					{
						lstArmor.SelectedIndex = 0;
					}
					catch
					{
					}
				}
			}
			if (e.KeyCode == Keys.Up)
			{
				try
				{
					lstArmor.SelectedIndex--;
					if (lstArmor.SelectedIndex == -1)
						lstArmor.SelectedIndex = lstArmor.Items.Count - 1;
				}
				catch
				{
					try
					{
						lstArmor.SelectedIndex = lstArmor.Items.Count - 1;
					}
					catch
					{
					}
				}
			}
		}

		private void txtSearch_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Up)
				txtSearch.Select(txtSearch.Text.Length, 0);
		}

        private void tmrSearch_Tick(object sender, EventArgs e)
        {
            tmrSearch.Stop();
            tmrSearch.Enabled = false;

            if (txtSearch.Text == "")
            {
                cboCategory_SelectedIndexChanged(sender, e);
                return;
            }

            string strCategoryFilter = "";

            foreach (object objListItem in cboCategory.Items)
            {
                ListItem objItem = (ListItem)objListItem;
                if (objItem.Value != "")
                    strCategoryFilter += "category = \"" + objItem.Value + "\" or ";
            }

            // Treat everything as being uppercase so the search is case-insensitive.
            string strSearch = "/chummer/armors/armor[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))";
            if (strCategoryFilter != "")
                strSearch += " and (" + strCategoryFilter + ")";
            // Remove the trailing " or )";
            if (strSearch.EndsWith(" or )"))
            {
                strSearch = strSearch.Substring(0, strSearch.Length - 4) + ")";
            }
            strSearch += "]";

            XmlNodeList objXmlArmorList = _objXmlDocument.SelectNodes(strSearch);

            if (dgvArmor.Visible)
            {
                DataTable tabArmor = new DataTable("armor");
                tabArmor.Columns.Add("ArmorName");
                tabArmor.Columns.Add("Armor");
                tabArmor.Columns["Armor"].DataType = typeof(Int32);
                tabArmor.Columns.Add("Capacity");
                tabArmor.Columns["Capacity"].DataType = typeof(Int32);
                tabArmor.Columns.Add("Avail");
                tabArmor.Columns.Add("Special");
                tabArmor.Columns.Add("Source");
                tabArmor.Columns.Add("Cost");
                tabArmor.Columns["Cost"].DataType = typeof(Int32);

                // Populate the Weapon list.
                foreach (XmlNode objXmlArmor in objXmlArmorList)
                {
                    TreeNode objNode = new TreeNode();
                    Armor objArmor = new Armor(_objCharacter);
					List<Weapon> objWeapons = new List<Weapon>();
					objArmor.Create(objXmlArmor, objNode, null, 0, objWeapons, true, true);

                    string strWeaponName = objArmor.Name;
                    int intArmor = objArmor.TotalArmor;
                    int intCapacity = Convert.ToInt32(objArmor.CalculatedCapacity);
                    string strAvail = objArmor.Avail;
                    string strAccessories = "";
                    foreach (ArmorMod objMod in objArmor.ArmorMods)
                    {
                        if (strAccessories.Length > 0)
                            strAccessories += "\n";
                        strAccessories += objMod.Name;
                    }
                    foreach (Gear objGear in objArmor.Gear)
                    {
                        if (strAccessories.Length > 0)
                            strAccessories += "\n";
                        strAccessories += objGear.Name;
                    }
                    string strSource = objArmor.Source + " " + objArmor.Page;
                    int intCost = objArmor.Cost;

                    tabArmor.Rows.Add(strWeaponName, intArmor, intCapacity, strAvail, strAccessories, strSource, intCost);
                }

                DataSet set = new DataSet("armor");
                set.Tables.Add(tabArmor);

                dgvArmor.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                dgvArmor.DataSource = set;
                dgvArmor.DataMember = "armor";
            }
            else
            {
                List<ListItem> lstArmors = new List<ListItem>();
                foreach (XmlNode objXmlArmor in objXmlArmorList)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlArmor["name"].InnerText;
                    if (objXmlArmor["translate"] != null)
                        objItem.Name = objXmlArmor["translate"].InnerText;
                    else
                        objItem.Name = objXmlArmor["name"].InnerText;

                    try
                    {
                        objItem.Name += " [" + _lstCategory.Find(objFind => objFind.Value == objXmlArmor["category"].InnerText).Name + "]";
                        lstArmors.Add(objItem);
                    }
                    catch
                    {
                    }
                }
                SortListItem objSort = new SortListItem();
                lstArmors.Sort(objSort.Compare);
                lstArmor.DataSource = null;
                lstArmor.ValueMember = "Value";
                lstArmor.DisplayMember = "Name";
                lstArmor.DataSource = lstArmors;
            }
        }

        private void chkBrowse_CheckedChanged(object sender, EventArgs e)
        {
            dgvArmor.Visible = chkBrowse.Checked;

            lstArmor.Visible = !chkBrowse.Checked;
            lblArmorLabel.Visible = !chkBrowse.Checked;
            lblArmor.Visible = !chkBrowse.Checked;
            lblCost.Visible = !chkBrowse.Checked;
            lblCostLabel.Visible = !chkBrowse.Checked;
            lblAvail.Visible = !chkBrowse.Checked;
            lblAvailLabel.Visible = !chkBrowse.Checked;
            lblSourceLabel.Visible = !chkBrowse.Checked;
            lblSource.Visible = !chkBrowse.Checked;
            chkFreeItem.Visible = !chkBrowse.Checked;
            lblCapacity.Visible = !chkBrowse.Checked;
            lblCapacityLabel.Visible = !chkBrowse.Checked;
            nudMarkup.Visible = !chkBrowse.Checked;
            lblMarkupLabel.Visible = !chkBrowse.Checked;
            lblMarkupPercentLabel.Visible = !chkBrowse.Checked;
            lblTest.Visible = !chkBrowse.Checked;
            lblTestLabel.Visible = !chkBrowse.Checked;
            lblArmorValue.Visible = !chkBrowse.Checked;
            lblArmorValueLabel.Visible = !chkBrowse.Checked;

            if (txtSearch.Text.Length > 0)
            {
                tmrSearch_Tick(this, null);
            }

            if (chkBrowse.Checked)
                LoadGrid();
        }

        private void dgvArmor_DoubleClick(object sender, EventArgs e)
        {
            if (lstArmor.Text != "" || dgvArmor.Visible)
                AcceptForm();
        }

        private void dgvArmor_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column.Index != 1)
                return;
            else
            {
                try
                {
                    int intResult;
                    if (Convert.ToInt32(e.CellValue1) < Convert.ToInt32(e.CellValue2))
                        intResult = -1;
                    else
                        intResult = 1;
                    e.SortResult = intResult;
                    e.Handled = true;
                }
                catch { }
                return;
            }
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
		/// Armor that was selected in the dialogue.
		/// </summary>
		public string SelectedArmor
		{
			get
			{
				return _strSelectedArmor;
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
		/// Markup percentage.
		/// </summary>
		public int Rating
		{
			get
			{
				return _intRating;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Accept the selected item and close the form.
		/// </summary>
		private void AcceptForm()
		{
			if (lstArmor.Text != "")
			{
				XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/armors/armor[name = \"" + lstArmor.SelectedValue + "\"]");
				_strSelectCategory = objNode["category"].InnerText;
				_strSelectedArmor = objNode["name"].InnerText;
				_intMarkup = Convert.ToInt32(nudMarkup.Value);
				_intRating = Convert.ToInt32(nudRating.Value);
				_blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;

				this.DialogResult = DialogResult.OK;
			}
		}

		private void MoveControls()
		{
			int intWidth = lblArmorLabel.Width;
			intWidth = Math.Max(intWidth, lblCapacityLabel.Width);
			intWidth = Math.Max(intWidth, lblAvailLabel.Width);
			intWidth = Math.Max(intWidth, lblCostLabel.Width);

			lblArmor.Left = lblArmorLabel.Left + intWidth + 6;
			lblCapacity.Left = lblCapacityLabel.Left + intWidth + 6;
			lblAvail.Left = lblAvailLabel.Left + intWidth + 6;
			lblTestLabel.Left = lblAvail.Left + lblAvail.Width + 16;
			lblCost.Left = lblCostLabel.Left + intWidth + 6;

			nudMarkup.Left = lblMarkupLabel.Left + lblMarkupLabel.Width + 6;
			lblMarkupPercentLabel.Left = nudMarkup.Left + nudMarkup.Width;

			lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;

			lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
		}

        private void LoadGrid()
        {
            DataTable tabArmor = new DataTable("armor");
            tabArmor.Columns.Add("ArmorName");
            tabArmor.Columns.Add("Armor");
            tabArmor.Columns["Armor"].DataType = typeof(Int32);
            tabArmor.Columns.Add("Capacity");
            tabArmor.Columns["Capacity"].DataType = typeof(Int32);
            tabArmor.Columns.Add("Avail");
            tabArmor.Columns.Add("Special");
            tabArmor.Columns.Add("Source");
            tabArmor.Columns.Add("Cost");
            tabArmor.Columns["Cost"].DataType = typeof(Int32);

            // Populate the Weapon list.
            XmlNodeList objXmlArmorList;

            if (txtSearch.Text.Length > 1)
            {
                string strSearch = "/chummer/weapons/weapon[(" + _objCharacter.Options.BookXPath() + ") and category != \"Cyberware\" and category != \"Gear\" and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))]";
                objXmlArmorList = _objXmlDocument.SelectNodes(strSearch);
            }
            else
            {
                // Populate the Armor list.
                objXmlArmorList = _objXmlDocument.SelectNodes("/chummer/armors/armor[category = \"" + cboCategory.SelectedValue + "\" and (" + _objCharacter.Options.BookXPath() + ")]");
            }

            foreach (XmlNode objXmlArmor in objXmlArmorList)
            {
                TreeNode objNode = new TreeNode();
                Armor objArmor = new Armor(_objCharacter);
				List<Weapon> objWeapons = new List<Weapon>();
				objArmor.Create(objXmlArmor, objNode, null, 0, objWeapons, true, true);

                string strWeaponName = objArmor.Name;
                int intArmor = objArmor.TotalArmor;
                int intCapacity = Convert.ToInt32(objArmor.CalculatedCapacity);
                string strAvail = objArmor.Avail;
                string strAccessories = "";
                foreach (ArmorMod objMod in objArmor.ArmorMods)
                {
                    if (strAccessories.Length > 0)
                        strAccessories += "\n";
                    strAccessories += objMod.Name;
                }
                foreach (Gear objGear in objArmor.Gear)
                {
                    if (strAccessories.Length > 0)
                        strAccessories += "\n";
                    strAccessories += objGear.Name;
                }
                string strSource = objArmor.Source + " " + objArmor.Page;
                int intCost = objArmor.Cost;

                tabArmor.Rows.Add(strWeaponName, intArmor, intCapacity, strAvail, strAccessories, strSource, intCost);
            }

            DataSet set = new DataSet("armor");
            set.Tables.Add(tabArmor);

            dgvArmor.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvArmor.DataSource = set;
            dgvArmor.DataMember = "armor";
        }
		private void UpdateArmorInfo()
		{
			// Get the information for the selected piece of Armor.
			XmlNode objXmlArmor = _objXmlDocument.SelectSingleNode("/chummer/armors/armor[name = \"" + lstArmor.SelectedValue + "\"]");
			// Create the Armor so we can show its Total Avail (some Armor includes Chemical Seal which adds +6 which wouldn't be factored in properly otherwise).
			Armor objArmor = new Armor(_objCharacter);
			TreeNode objNode = new TreeNode();
			List<Weapon> objWeapons = new List<Weapon>();
			objArmor.Create(objXmlArmor, objNode, null, 0, objWeapons, true);

			// Check for a Variable Cost.
			int intItemCost = 0;
			if (objXmlArmor["cost"].InnerText.StartsWith("Variable"))
			{
				int intMin = 0;
				int intMax = 0;
				string strCost = objXmlArmor["cost"].InnerText.Replace("Variable(", string.Empty).Replace(")", string.Empty);
				if (strCost.Contains("-"))
				{
					string[] strValues = strCost.Split('-');
					intMin = Convert.ToInt32(strValues[0]);
					intMax = Convert.ToInt32(strValues[1]);
				}
				else
					intMin = Convert.ToInt32(strCost.Replace("+", string.Empty));

				if (intMax == 0)
				{
					intMax = 1000000;
					lblCost.Text = String.Format("{0:###,###,##0¥+}", intMin);
				}
				else
					lblCost.Text = String.Format("{0:###,###,##0}", intMin) + "-" + String.Format("{0:###,###,##0¥}", intMax);

				intItemCost = intMin;
			}
			else if (objXmlArmor["cost"].InnerText.Contains("Rating"))
			{
				XPathNavigator nav = _objXmlDocument.CreateNavigator();
				XPathExpression xprCost = nav.Compile(objXmlArmor["cost"].InnerText.Replace("Rating", nudRating.Value.ToString()));
				double dblCost = (Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.Instance.CultureInfo));
				if (chkBlackMarketDiscount.Checked)
				{
					dblCost = dblCost - (dblCost*0.90);
				}
				intItemCost = Convert.ToInt32(dblCost);
				lblCost.Text = String.Format("{0:###,###,##0¥}", intItemCost);
			}
			else
			{
				double dblCost = Convert.ToDouble(objXmlArmor["cost"].InnerText, GlobalOptions.Instance.CultureInfo);
				dblCost *= 1 + (Convert.ToDouble(nudMarkup.Value, GlobalOptions.Instance.CultureInfo) / 100.0);
				if (chkBlackMarketDiscount.Checked)
				{
					dblCost = dblCost * 0.90;
				}
				lblCost.Text = String.Format("{0:###,###,##0¥}", dblCost);
				intItemCost = Convert.ToInt32(dblCost);
			}

			lblCapacity.Text = objXmlArmor["armorcapacity"].InnerText;

			if (chkFreeItem.Checked)
			{
				lblCost.Text = String.Format("{0:###,###,##0¥}", 0);
				intItemCost = 0;
			}

			lblTest.Text = _objCharacter.AvailTest(intItemCost, lblAvail.Text);

			string strBook = _objCharacter.Options.LanguageBookShort(objXmlArmor["source"].InnerText);
			string strPage = objXmlArmor["page"].InnerText;
			if (objXmlArmor["altpage"] != null)
				strPage = objXmlArmor["altpage"].InnerText;
			lblSource.Text = strBook + " " + strPage;


			tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlArmor["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
		}
        #endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions objCommon = new CommonFunctions(_objCharacter);
            objCommon.OpenPDF(lblSource.Text);
        }
	}
}