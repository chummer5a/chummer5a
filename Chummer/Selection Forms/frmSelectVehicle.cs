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
	public partial class frmSelectVehicle : Form
	{
		private string _strSelectedVehicle = "";
		private bool _blnUsedVehicle = false;
		private string _strUsedAvail = "";
		private int _intUsedCost = 0;
		private int _intMarkup = 0;

		private bool _blnAddAgain = false;
		private static string _strSelectCategory = "";

		private XmlDocument _objXmlDocument = new XmlDocument();
		private readonly Character _objCharacter;

		private List<ListItem> _lstCategory = new List<ListItem>();
		private bool _blnBlackMarketDiscount;

		#region Control Events
		public frmSelectVehicle(Character objCharacter, bool blnCareer = false)
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			lblMarkupLabel.Visible = blnCareer;
			nudMarkup.Visible = blnCareer;
			lblMarkupPercentLabel.Visible = blnCareer;
			_objCharacter = objCharacter;
			MoveControls();
		}

		private void frmSelectVehicle_Load(object sender, EventArgs e)
		{
			foreach (Label objLabel in this.Controls.OfType<Label>())
			{
				if (objLabel.Text.StartsWith("["))
					objLabel.Text = "";
			}

			// Load the Vehicle information.
			_objXmlDocument = XmlManager.Instance.Load("vehicles.xml");

			// Populate the Vehicle Category list.
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

			chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;
		}

		private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Update the list of Vehicles based on the selected Category.
			List<ListItem> lstVehicles = new List<ListItem>();

			// Retrieve the list of Vehicles for the selected Category.
			XmlNodeList objXmlVehicleList = _objXmlDocument.SelectNodes("/chummer/vehicles/vehicle[category = \"" + cboCategory.SelectedValue + "\" and (" + _objCharacter.Options.BookXPath() + ")]");
			foreach (XmlNode objXmlVehicle in objXmlVehicleList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlVehicle["name"].InnerText;
				if (objXmlVehicle["translate"] != null)
					objItem.Name = objXmlVehicle["translate"].InnerText;
				else
					objItem.Name = objXmlVehicle["name"].InnerText;
				lstVehicles.Add(objItem);
			}
			SortListItem objSort = new SortListItem();
			lstVehicles.Sort(objSort.Compare);
			lstVehicle.DataSource = null;
			lstVehicle.ValueMember = "Value";
			lstVehicle.DisplayMember = "Name";
			lstVehicle.DataSource = lstVehicles;
		}

		private void lstVehicle_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateSelectedVehicle();
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			if (lstVehicle.Text != "")
				AcceptForm();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
			if (txtSearch.Text == "")
			{
				cboCategory_SelectedIndexChanged(sender, e);
				return;
			}

			// Treat everything as being uppercase so the search is case-insensitive.
			string strSearch = "/chummer/vehicles/vehicle[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))]";

			XmlNodeList objXmlVehicleList = _objXmlDocument.SelectNodes(strSearch);
			List<ListItem> lstVehicles = new List<ListItem>();
			foreach (XmlNode objXmlVehicle in objXmlVehicleList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlVehicle["name"].InnerText;
				if (objXmlVehicle["translate"] != null)
					objItem.Name = objXmlVehicle["translate"].InnerText;
				else
					objItem.Name = objXmlVehicle["name"].InnerText;

				try
				{
					objItem.Name += " [" + _lstCategory.Find(objFind => objFind.Value == objXmlVehicle["category"].InnerText).Name + "]";
					lstVehicles.Add(objItem);
				}
				catch
				{
				}
			}
			SortListItem objSort = new SortListItem();
			lstVehicles.Sort(objSort.Compare);
			lstVehicle.DataSource = null;
			lstVehicle.ValueMember = "Value";
			lstVehicle.DisplayMember = "Name";
			lstVehicle.DataSource = lstVehicles;
		}

		private void lstVehicle_DoubleClick(object sender, EventArgs e)
		{
			if (lstVehicle.Text != "")
				AcceptForm();
		}

		private void cmdOKAdd_Click(object sender, EventArgs e)
		{
			_blnAddAgain = true;
			cmdOK_Click(sender, e);
		}

		private void chkUsedVehicle_CheckedChanged(object sender, EventArgs e)
		{
			UpdateSelectedVehicle();
		}

		private void nudUsedVehicleDiscount_ValueChanged(object sender, EventArgs e)
		{
			UpdateSelectedVehicle();
		}

		private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
		{
			UpdateSelectedVehicle();
		}

		private void nudMarkup_ValueChanged(object sender, EventArgs e)
		{
			UpdateSelectedVehicle();
		}

		private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
		{
			UpdateSelectedVehicle();
		}

		private void txtSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
			{
				try
				{
					lstVehicle.SelectedIndex++;
				}
				catch
				{
					try
					{
						lstVehicle.SelectedIndex = 0;
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
					lstVehicle.SelectedIndex--;
					if (lstVehicle.SelectedIndex == -1)
						lstVehicle.SelectedIndex = lstVehicle.Items.Count - 1;
				}
				catch
				{
					try
					{
						lstVehicle.SelectedIndex = lstVehicle.Items.Count - 1;
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
		/// Name of Vehicle that was selected in the dialogue.
		/// </summary>
		public string SelectedVehicle
		{
			get
			{
				return _strSelectedVehicle;
			}
		}

		/// <summary>
		/// Whether or not the selected Vehicle is used.
		/// </summary>
		public bool UsedVehicle
		{
			get
			{
				return _blnUsedVehicle;
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
		/// Cost of the Vehicle's cost by when it is used.
		/// </summary>
		public int UsedCost
		{
			get
			{
				return _intUsedCost;
			}
		}

		/// <summary>
		/// Vehicle's Aavailability when it is used.
		/// </summary>
		public string UsedAvail
		{
			get
			{
				return _strUsedAvail;
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
		/// Refresh the information for the selected Vehicle.
		/// </summary>
		private void UpdateSelectedVehicle()
		{
			if (lstVehicle.Text == "")
				return;

			double dblCostModifier = 1.0;

			// Retireve the information for the selected Vehicle.
			XmlNode objXmlVehicle = _objXmlDocument.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + lstVehicle.SelectedValue + "\"]");

			if (chkUsedVehicle.Checked)
				dblCostModifier = Convert.ToDouble(1 - (nudUsedVehicleDiscount.Value / 100), GlobalOptions.Instance.CultureInfo);

			lblVehicleHandling.Text = objXmlVehicle["handling"].InnerText;
			lblVehicleAccel.Text = objXmlVehicle["accel"].InnerText;
			lblVehicleSpeed.Text = objXmlVehicle["speed"].InnerText;
			lblVehiclePilot.Text = objXmlVehicle["pilot"].InnerText;
			lblVehicleBody.Text = objXmlVehicle["body"].InnerText;
			lblVehicleArmor.Text = objXmlVehicle["armor"].InnerText;
			lblVehicleSensor.Text = objXmlVehicle["sensor"].InnerText;

			if (chkUsedVehicle.Checked)
			{
				string strSuffix = "";
				string strAvail = objXmlVehicle["avail"].InnerText;
				if (strAvail.Contains("R") || strAvail.Contains("F"))
				{
					strSuffix = strAvail.Substring(strAvail.Length - 1, 1);
					strAvail = strAvail.Replace(strSuffix, string.Empty);
					strAvail = (Convert.ToInt32(strAvail) + 4).ToString() + strSuffix;
					lblVehicleAvail.Text = strAvail;
				}
				else
					lblVehicleAvail.Text = (Convert.ToInt32(objXmlVehicle["avail"].InnerText) + 4).ToString();
			}
			else
				lblVehicleAvail.Text = objXmlVehicle["avail"].InnerText;

			lblVehicleAvail.Text = lblVehicleAvail.Text.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted")).Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));

			// Apply the cost multiplier to the Vehicle (will be 1 unless Used Vehicle is selected)
            if (objXmlVehicle["cost"].InnerText.StartsWith("Variable"))
            {
                lblVehicleCost.Text = objXmlVehicle["cost"].InnerText;
                lblTest.Text = "";
            }
            else
            {
                int intCost = Convert.ToInt32(objXmlVehicle["cost"].InnerText);

				intCost = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(intCost, GlobalOptions.Instance.CultureInfo) * dblCostModifier));

                // Apply the markup if applicable.
                double dblCost = Convert.ToDouble(intCost, GlobalOptions.Instance.CultureInfo);
                dblCost *= 1 + (Convert.ToDouble(nudMarkup.Value, GlobalOptions.Instance.CultureInfo) / 100.0);

				if (chkBlackMarketDiscount.Checked)
				{
					dblCost *= 0.90;
				}

				intCost = Convert.ToInt32(dblCost);

                if (chkFreeItem.Checked)
                    intCost = 0;

                lblVehicleCost.Text = String.Format("{0:###,###,##0¥}", intCost);
                lblTest.Text = _objCharacter.AvailTest(intCost, lblVehicleAvail.Text);
            }


			string strBook = _objCharacter.Options.LanguageBookShort(objXmlVehicle["source"].InnerText);
			string strPage = objXmlVehicle["page"].InnerText;
			if (objXmlVehicle["altpage"] != null)
				strPage = objXmlVehicle["altpage"].InnerText;
			lblSource.Text = strBook + " " + strPage;

			tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlVehicle["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
		}

		/// <summary>
		/// Accept the selected item and close the form.
		/// </summary>
		private void AcceptForm()
		{
			XmlNode objXmlVehicle = _objXmlDocument.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + lstVehicle.SelectedValue + "\"]");

			if (chkUsedVehicle.Checked)
			{
				double dblCostModifier = Convert.ToDouble(1 - (nudUsedVehicleDiscount.Value / 100), GlobalOptions.Instance.CultureInfo);
				int intCost = Convert.ToInt32(objXmlVehicle["cost"].InnerText);
				intCost = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(intCost, GlobalOptions.Instance.CultureInfo) * dblCostModifier));

				_blnUsedVehicle = true;
				_strUsedAvail = lblVehicleAvail.Text.Replace(LanguageManager.Instance.GetString("String_AvailRestricted"), "R").Replace(LanguageManager.Instance.GetString("String_AvailForbidden"), "F");
				_intUsedCost = intCost;
			}

			_blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
			_strSelectCategory = objXmlVehicle["category"].InnerText;
			_strSelectedVehicle = objXmlVehicle["name"].InnerText;
			_intMarkup = Convert.ToInt32(nudMarkup.Value);

			this.DialogResult = DialogResult.OK;
		}

		private void MoveControls()
		{
			int intWidth = Math.Max(lblVehicleHandlingLabel.Width, lblVehicleSpeedLabel.Width);
			intWidth = Math.Max(intWidth, lblVehicleBodyLabel.Width);
			intWidth = Math.Max(intWidth, lblVehicleSensorLabel.Width);
			intWidth = Math.Max(intWidth, lblVehicleAvailLabel.Width);
			intWidth = Math.Max(intWidth, lblVehicleCostLabel.Width);

			lblVehicleHandling.Left = lblVehicleHandlingLabel.Left + intWidth + 6;
			lblVehicleSpeed.Left = lblVehicleSpeedLabel.Left + intWidth + 6;
			lblVehicleBody.Left = lblVehicleBodyLabel.Left + intWidth + 6;
			lblVehicleSensor.Left = lblVehicleSensorLabel.Left + intWidth + 6;
			lblVehicleAvail.Left = lblVehicleAvailLabel.Left + intWidth + 6;
			lblTestLabel.Left = lblVehicleAvail.Left + lblVehicleAvail.Width + 16;
			lblTest.Left = lblTestLabel.Left + lblTestLabel.Width + 6;
			lblVehicleCost.Left = lblVehicleCostLabel.Left + intWidth + 6;

			intWidth = Math.Max(lblVehicleAccelLabel.Width, lblVehiclePilotLabel.Width);
			intWidth = Math.Max(intWidth, lblVehicleArmorLabel.Width);

			lblVehicleAccelLabel.Left = lblVehicleHandling.Left + 60;
			lblVehicleAccel.Left = lblVehicleAccelLabel.Left + intWidth + 6;
			lblVehiclePilotLabel.Left = lblVehicleHandling.Left + 60;
			lblVehiclePilot.Left = lblVehiclePilotLabel.Left + intWidth + 6;
			lblVehicleArmorLabel.Left = lblVehicleHandling.Left + 60;
			lblVehicleArmor.Left = lblVehicleArmorLabel.Left + intWidth + 6;

			lblUsedVehicleDiscountLabel.Left = chkUsedVehicle.Left + chkUsedVehicle.Width + 6;
			nudUsedVehicleDiscount.Left = lblUsedVehicleDiscountLabel.Left + lblUsedVehicleDiscountLabel.Width + 6;
			lblUsedVehicleDiscountPercentLabel.Left = nudUsedVehicleDiscount.Left + nudUsedVehicleDiscount.Width;

			nudMarkup.Left = lblMarkupLabel.Left + lblMarkupLabel.Width + 6;
			lblMarkupPercentLabel.Left = nudMarkup.Left + nudMarkup.Width + 6;

			lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;

			lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
		}
		#endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions objCommon = new CommonFunctions(_objCharacter);
            objCommon.OpenPDF(lblSource.Text);
        }
	}
}