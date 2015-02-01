using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectWeapon : Form
    {
		private string _strSelectedWeapon = "";
		private int _intMarkup = 0;

		private bool _blnAddAgain = false;
		private static string _strSelectCategory = "";
		private readonly Character _objCharacter;

		private XmlDocument _objXmlDocument = new XmlDocument();

		private List<ListItem> _lstCategory = new List<ListItem>();

		#region Control Events
		public frmSelectWeapon(Character objCharacter, bool blnCareer = false)
        {
            InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			chkFreeItem.Visible = blnCareer;
			lblMarkupLabel.Visible = blnCareer;
			nudMarkup.Visible = blnCareer;
			lblMarkupPercentLabel.Visible = blnCareer;
			_objCharacter = objCharacter;
			MoveControls();
        }

        private void frmSelectWeapon_Load(object sender, EventArgs e)
        {
			foreach (Label objLabel in this.Controls.OfType<Label>())
			{
				if (objLabel.Text.StartsWith("["))
					objLabel.Text = "";
			}

        	// Load the Weapon information.
			_objXmlDocument = XmlManager.Instance.Load("weapons.xml");

            // Populate the Weapon Category list.
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

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
        	List<ListItem> lstWeapons = new List<ListItem>();

			// Populate the Weapon list.
			XmlNodeList objXmlWeaponList = _objXmlDocument.SelectNodes("/chummer/weapons/weapon[category = \"" + cboCategory.SelectedValue + "\" and (" + _objCharacter.Options.BookXPath() + ")]");
			foreach (XmlNode objXmlWeapon in objXmlWeaponList)
			{
                bool blnCyberware = false;
                try 
                {
                    if (objXmlWeapon["cyberware"].InnerText == "yes")
                        blnCyberware = true;
                }
                catch
                { }

                if (!blnCyberware)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlWeapon["name"].InnerText;
                    if (objXmlWeapon["translate"] != null)
                        objItem.Name = objXmlWeapon["translate"].InnerText;
                    else
                        objItem.Name = objXmlWeapon["name"].InnerText;
                    lstWeapons.Add(objItem);
                }
			}
			SortListItem objSort = new SortListItem();
			lstWeapons.Sort(objSort.Compare);
			lstWeapon.DataSource = null;
			lstWeapon.ValueMember = "Value";
			lstWeapon.DisplayMember = "Name";
			lstWeapon.DataSource = lstWeapons;

            if (chkBrowse.Checked)
                LoadGrid();
        }

        private void lstWeapon_SelectedIndexChanged(object sender, EventArgs e)
        {
			if (lstWeapon.Text == "")
				return;

            // Retireve the information for the selected Weapon.
        	XmlNode objXmlWeapon = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + lstWeapon.SelectedValue + "\"]");

			Weapon objWeapon = new Weapon(_objCharacter);
			TreeNode objNode = new TreeNode();
			objWeapon.Create(objXmlWeapon, _objCharacter, objNode, null, null, null);

            lblWeaponReach.Text = objWeapon.TotalReach.ToString();
			lblWeaponDamage.Text = objWeapon.CalculatedDamage();
			lblWeaponAP.Text = objWeapon.TotalAP;
			lblWeaponMode.Text = objWeapon.CalculatedMode;
			lblWeaponRC.Text = objWeapon.TotalRC;
			lblWeaponAmmo.Text = objWeapon.CalculatedAmmo();
            lblWeaponAccuracy.Text = objWeapon.TotalAccuracy.ToString();
            lblWeaponAvail.Text = objWeapon.TotalAvail;

			int intItemCost = 0;
            double dblCost = 0;
            try
            {
                dblCost = Convert.ToDouble(objXmlWeapon["cost"].InnerText, GlobalOptions.Instance.CultureInfo);
            }
            catch { }
			dblCost *= 1 + (Convert.ToDouble(nudMarkup.Value, GlobalOptions.Instance.CultureInfo) / 100.0);
			lblWeaponCost.Text = String.Format("{0:###,###,##0¥}", dblCost);
			try
			{
				intItemCost = Convert.ToInt32(dblCost);
			}
			catch
			{
			}

			if (chkFreeItem.Checked)
			{
				lblWeaponCost.Text = String.Format("{0:###,###,##0¥}", 0);
				intItemCost = 0;
			}

			lblTest.Text = _objCharacter.AvailTest(intItemCost, lblWeaponAvail.Text);

			string strBook = _objCharacter.Options.LanguageBookShort(objXmlWeapon["source"].InnerText);
			string strPage = objXmlWeapon["page"].InnerText;
			if (objXmlWeapon["altpage"] != null)
				strPage = objXmlWeapon["altpage"].InnerText;
			lblSource.Text = strBook + " " + strPage;

			// Build a list of included Accessories and Modifications that come with the weapon.
            string strAccessories = "";
			XmlNodeList objXmlNodeList = objXmlWeapon.SelectNodes("accessories/accessory");
			foreach (XmlNode objXmlAccessory in objXmlNodeList)
			{
				XmlNode objXmlItem = _objXmlDocument.SelectSingleNode("/chummer/accessories/accessory[name = \"" + objXmlAccessory.InnerText + "\"]");
				if (objXmlItem["translate"] != null)
					strAccessories += objXmlItem["translate"].InnerText + "\n";
				else
					strAccessories += objXmlItem["name"].InnerText + "\n";
			}
            objXmlNodeList = objXmlWeapon.SelectNodes("mods/mod");
			foreach (XmlNode objXmlMod in objXmlNodeList)
			{
				XmlNode objXmlItem = _objXmlDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + objXmlMod.InnerText + "\"]");
				if (objXmlItem["translate"] != null)
					strAccessories += objXmlItem["translate"].InnerText + "\n";
				else
					strAccessories += objXmlItem["name"].InnerText + "\n";
			}
			if (strAccessories == "")
				lblIncludedAccessories.Text = LanguageManager.Instance.GetString("String_None");
			else
				lblIncludedAccessories.Text = strAccessories;

            tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlWeapon["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
        }

		private void cmdOK_Click(object sender, EventArgs e)
		{
			if (lstWeapon.Text != "" || dgvWeapons.Visible)
				AcceptForm();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
            tmrSearch.Stop();
            tmrSearch.Enabled = true;
            tmrSearch.Start();
        }

		private void lstWeapon_DoubleClick(object sender, EventArgs e)
		{
			if (lstWeapon.Text != "")
				AcceptForm();
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
				try
				{
					lstWeapon.SelectedIndex++;
				} 
				catch
				{
					try
					{
						lstWeapon.SelectedIndex = 0;
					}
					catch
					{
					}
				}
                try
                {
                    dgvWeapons.Rows[dgvWeapons.SelectedRows[0].Index + 1].Selected = true;
                }
                catch
                {
                    try 
                    {
                        dgvWeapons.Rows[0].Selected = true;
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
					lstWeapon.SelectedIndex--;
					if (lstWeapon.SelectedIndex == -1)
						lstWeapon.SelectedIndex = lstWeapon.Items.Count - 1;
				}
				catch
				{
					try
					{
						lstWeapon.SelectedIndex = lstWeapon.Items.Count - 1;
					}
					catch
					{
					}
				}
                try
                {
                    dgvWeapons.Rows[dgvWeapons.SelectedRows[0].Index - 1].Selected = true;
                }
                catch
                {
                    try
                    {
                        dgvWeapons.Rows[0].Selected = true;
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
		#endregion

		#region Methods
		/// <summary>
		/// Accept the selected item and close the form.
		/// </summary>
		private void AcceptForm()
		{
            if (dgvWeapons.Visible)
            {
                if (dgvWeapons.SelectedRows.Count == 1)
                {

                    XmlNode objNode;
                    if (txtSearch.Text.Length > 1)
                    {
                        string strWeapon = dgvWeapons.SelectedRows[0].Cells[0].Value.ToString();
                        strWeapon = strWeapon.Substring(0, strWeapon.LastIndexOf("(") - 1);
                        objNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + strWeapon + "\"]");
                    }
                    else
                    {
                        objNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + dgvWeapons.SelectedRows[0].Cells[0].Value.ToString() + "\"]");
                    }
                    _strSelectCategory = objNode["category"].InnerText;
                    _strSelectedWeapon = objNode["name"].InnerText;
                    _intMarkup = Convert.ToInt32(nudMarkup.Value);

                    this.DialogResult = DialogResult.OK;
                }
            }
			else if (lstWeapon.Text != "")
			{
				XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + lstWeapon.SelectedValue + "\"]");
				_strSelectCategory = objNode["category"].InnerText;
				_strSelectedWeapon = objNode["name"].InnerText;
				_intMarkup = Convert.ToInt32(nudMarkup.Value);

				this.DialogResult = DialogResult.OK;
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
		#endregion

        private void chkBrowse_CheckedChanged(object sender, EventArgs e)
        {
            dgvWeapons.Visible = chkBrowse.Checked;

            lstWeapon.Visible = !chkBrowse.Checked;
            lblWeaponDamage.Visible = !chkBrowse.Checked;
            lblWeaponDamageLabel.Visible = !chkBrowse.Checked;
            lblWeaponRC.Visible = !chkBrowse.Checked;
            lblWeaponRCLabel.Visible = !chkBrowse.Checked;
            lblWeaponAP.Visible = !chkBrowse.Checked;
            lblWeaponAPLabel.Visible = !chkBrowse.Checked;
            lblWeaponAmmo.Visible = !chkBrowse.Checked;
            lblWeaponAmmoLabel.Visible = !chkBrowse.Checked;
            lblWeaponReach.Visible = !chkBrowse.Checked;
            lblWeaponReachLabel.Visible = !chkBrowse.Checked;
            lblWeaponMode.Visible = !chkBrowse.Checked;
            lblWeaponModeLabel.Visible = !chkBrowse.Checked;
            lblWeaponAvail.Visible = !chkBrowse.Checked;
            lblWeaponAvailLabel.Visible = !chkBrowse.Checked;
            lblTest.Visible = !chkBrowse.Checked;
            lblTestLabel.Visible = !chkBrowse.Checked;
            lblWeaponCost.Visible = !chkBrowse.Checked;
            lblWeaponCostLabel.Visible = !chkBrowse.Checked;
            lblWeaponAccuracy.Visible = !chkBrowse.Checked;
            lblWeaponAccuracyLabel.Visible = !chkBrowse.Checked;
            chkFreeItem.Visible = !chkBrowse.Checked;
            lblMarkupLabel.Visible = !chkBrowse.Checked;
            lblMarkupPercentLabel.Visible = !chkBrowse.Checked;
            nudMarkup.Visible = !chkBrowse.Checked;
            label2.Visible = !chkBrowse.Checked;
            lblIncludedAccessories.Visible = !chkBrowse.Checked;
            lblSource.Visible = !chkBrowse.Checked;
            lblSourceLabel.Visible = !chkBrowse.Checked;

            if (txtSearch.Text.Length > 0)
            {
                tmrSearch_Tick(this, null);
            }

            if (chkBrowse.Checked)
                LoadGrid();
        }

        private void LoadGrid()
        {
            DataTable tabWeapons = new DataTable("weapons");
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

            // Populate the Weapon list.
            XmlNodeList objXmlWeaponList;

            if (txtSearch.Text.Length > 1)
            {
                string strSearch = "/chummer/weapons/weapon[(" + _objCharacter.Options.BookXPath() + ") and category != \"Cyberware\" and category != \"Gear\" and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))]";
                objXmlWeaponList = _objXmlDocument.SelectNodes(strSearch);
            }
            else
            {
                objXmlWeaponList = _objXmlDocument.SelectNodes("/chummer/weapons/weapon[category = \"" + cboCategory.SelectedValue + "\" and (" + _objCharacter.Options.BookXPath() + ")]");
            }

            foreach (XmlNode objXmlWeapon in objXmlWeaponList)
            {
                bool blnCyberware = false;
                try 
                {
                    if (objXmlWeapon["cyberware"].InnerText == "yes")
                        blnCyberware = true;
                }
                catch
                { }

                if (!blnCyberware)
                {
                    TreeNode objNode = new TreeNode();
                    Weapon objWeapon = new Weapon(_objCharacter);
                    objWeapon.Create(objXmlWeapon, _objCharacter, objNode, null, null, null);

                    string strWeaponName = objWeapon.Name;
                    string strDice = objWeapon.DicePool;
                    int intAccuracy = Convert.ToInt32(objWeapon.TotalAccuracy);
                    string strDamage = objWeapon.CalculatedDamage(_objCharacter.STR.Augmented);
                    string strAP = objWeapon.TotalAP;
                    if (strAP == "-")
                        strAP = "0";
                    int intRC = Convert.ToInt32(objWeapon.TotalRC);
                    string strAmmo = objWeapon.Ammo;
                    string strMode = objWeapon.Mode;
                    string strReach = objWeapon.TotalReach.ToString();
                    string strAccessories = "";
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        if (strAccessories.Length > 0)
                            strAccessories += "\n";
                        strAccessories += objAccessory.Name;
                    }
                    string strAvail = objWeapon.Avail.ToString();
                    string strSource = objWeapon.Source + " " + objWeapon.Page;
                    int intCost = objWeapon.Cost;

                    tabWeapons.Rows.Add(strWeaponName, strDice, intAccuracy, strDamage, strAP, intRC, strAmmo, strMode, strReach, strAccessories, strAvail, strSource, intCost);
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

            dgvWeapons.Columns[12].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            dgvWeapons.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvWeapons.DataSource = set;
            dgvWeapons.DataMember = "weapons";
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

            // Treat everything as being uppercase so the search is case-insensitive.
            string strSearch = "/chummer/weapons/weapon[(" + _objCharacter.Options.BookXPath() + ") and category != \"Cyberware\" and category != \"Gear\" and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))]";

            XmlNodeList objXmlWeaponList = _objXmlDocument.SelectNodes(strSearch);

            if (dgvWeapons.Visible)
            {
                DataTable tabWeapons = new DataTable("weapons");
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

                // Populate the Weapon list.
                foreach (XmlNode objXmlWeapon in objXmlWeaponList)
                {
                    TreeNode objNode = new TreeNode();
                    Weapon objWeapon = new Weapon(_objCharacter);
                    objWeapon.Create(objXmlWeapon, _objCharacter, objNode, null, null, null);

                    string strWeaponName = objWeapon.Name;
                    string strDice = objWeapon.DicePool;
                    int intAccuracy = Convert.ToInt32(objWeapon.TotalAccuracy);
                    string strDamage = objWeapon.CalculatedDamage();
                    string strAP = objWeapon.TotalAP;
                    if (strAP == "-")
                        strAP = "0";
                    int intRC = Convert.ToInt32(objWeapon.TotalRC);
                    string strAmmo = objWeapon.Ammo;
                    string strMode = objWeapon.Mode;
                    string strReach = objWeapon.TotalReach.ToString();
                    string strAccessories = "";
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        if (strAccessories.Length > 0)
                            strAccessories += "\n";
                        strAccessories += objAccessory.Name;
                    }
                    string strAvail = objWeapon.Avail.ToString();
                    string strSource = objWeapon.Source + " " + objWeapon.Page;
                    int intCost = objWeapon.Cost;

                    if (objWeapon.DisplayCategory == "Blades" || objWeapon.DisplayCategory == "Clubs" || objWeapon.DisplayCategory == "Improvised Weapons" || objWeapon.DisplayCategory == "Exotic Melee Weapons" || objWeapon.DisplayCategory == "Unarmed")
                    {
                        strAmmo = "";
                        strMode = "";
                    }
                    else
                    {
                        strReach = "";
                    }

                    tabWeapons.Rows.Add(strWeaponName, strDice, intAccuracy, strDamage, strAP, intRC, strAmmo, strMode, strReach, strAccessories, strAvail, strSource, intCost);
                }

                DataSet set = new DataSet("weapons");
                set.Tables.Add(tabWeapons);

                dgvWeapons.Columns[5].Visible = true;
                dgvWeapons.Columns[6].Visible = true;
                dgvWeapons.Columns[7].Visible = true;
                dgvWeapons.Columns[8].Visible = true;
                dgvWeapons.Columns[12].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;

                dgvWeapons.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                dgvWeapons.DataSource = set;
                dgvWeapons.DataMember = "weapons";
            }
            else
            {
                List<ListItem> lstWeapons = new List<ListItem>();
                foreach (XmlNode objXmlWeapon in objXmlWeaponList)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlWeapon["name"].InnerText;
                    if (objXmlWeapon["translate"] != null)
                        objItem.Name = objXmlWeapon["translate"].InnerText;
                    else
                        objItem.Name = objXmlWeapon["name"].InnerText;

                    try
                    {
                        objItem.Name += " [" + _lstCategory.Find(objFind => objFind.Value == objXmlWeapon["category"].InnerText).Name + "]";
                        lstWeapons.Add(objItem);
                    }
                    catch
                    {
                    }
                }
                SortListItem objSort = new SortListItem();
                lstWeapons.Sort(objSort.Compare);
                lstWeapon.DataSource = null;
                lstWeapon.ValueMember = "Value";
                lstWeapon.DisplayMember = "Name";
                lstWeapon.DataSource = lstWeapons;
            }
        }

        private void dgvWeapons_DoubleClick(object sender, EventArgs e)
        {
            if (lstWeapon.Text != "" || dgvWeapons.Visible)
                AcceptForm();
        }

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions objCommon = new CommonFunctions(_objCharacter);
            objCommon.OpenPDF(lblSource.Text);
        }
    }
}