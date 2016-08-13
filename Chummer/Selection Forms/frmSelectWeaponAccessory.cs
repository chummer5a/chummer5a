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
using System.Xml.XPath;

namespace Chummer
{
	public partial class frmSelectWeaponAccessory : Form
	{
		private string _strSelectedAccessory = "";
		private int _intMarkup = 0;

		private string _strAllowedMounts = "";
		private int _intWeaponCost = 0;
		private int _intRating = 0;
		private bool _blnAddAgain = false;

		private XmlDocument _objXmlDocument = new XmlDocument();
		private readonly Character _objCharacter;
		private int _intAccessoryMultiplier = 1;
		private bool _blnBlackMarketDiscount;

		#region Control Events
		public frmSelectWeaponAccessory(Character objCharacter, bool blnCareer = false)
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			lblMarkupLabel.Visible = blnCareer;
			nudMarkup.Visible = blnCareer;
			lblMarkupPercentLabel.Visible = blnCareer;
			_objCharacter = objCharacter;
			MoveControls();
		}

		private void frmSelectWeaponAccessory_Load(object sender, EventArgs e)
		{
            foreach (Label objLabel in this.Controls.OfType<Label>())
			{
				if (objLabel.Text.StartsWith("["))
					objLabel.Text = "";
			}

			List<ListItem> lstAccessories = new List<ListItem>();

			// Load the Weapon information.
			_objXmlDocument = XmlManager.Instance.Load("weapons.xml");

			// Populate the Accessory list.
			string[] strAllowed = _strAllowedMounts.Split('/');
			string strMount = "";
			foreach (string strAllowedMount in strAllowed)
			{
				if (strAllowedMount != "")
					strMount += "contains(mount, \"" + strAllowedMount + "\") or ";
			}
			strMount += "contains(mount, \"Internal\") or contains(mount, \"None\") or ";
			strMount += "mount = \"\"";
			XmlNodeList objXmlAccessoryList = _objXmlDocument.SelectNodes("/chummer/accessories/accessory[(" + strMount + ") and (" + _objCharacter.Options.BookXPath() + ")]");
			foreach (XmlNode objXmlAccessory in objXmlAccessoryList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlAccessory["name"].InnerText;
				if (objXmlAccessory["translate"] != null)
					objItem.Name = objXmlAccessory["translate"].InnerText;
				else
					objItem.Name = objXmlAccessory["name"].InnerText;
				lstAccessories.Add(objItem);
			}

			chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

			SortListItem objSort = new SortListItem();
			lstAccessories.Sort(objSort.Compare);
			lstAccessory.ValueMember = "Value";
			lstAccessory.DisplayMember = "Name";
			lstAccessory.DataSource = lstAccessories;
		}

		private void lstAccessory_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateGearInfo();
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			if (lstAccessory.Text != "")
				AcceptForm();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void lstAccessory_DoubleClick(object sender, EventArgs e)
		{
			if (lstAccessory.Text != "")
				AcceptForm();
		}

		private void cmdOKAdd_Click(object sender, EventArgs e)
		{
			_blnAddAgain = true;
			cmdOK_Click(sender, e);
		}

		private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
		{
			UpdateGearInfo();
		}

		private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
		{
			UpdateGearInfo();
		}

		private void nudMarkup_ValueChanged(object sender, EventArgs e)
		{
			UpdateGearInfo();
		}

		private void nudRating_ValueChanged(object sender, EventArgs e)
		{
			UpdateGearInfo();
		}
		private void UpdateGearInfo()
		{
			// Retrieve the information for the selected Accessory.
			XmlNode objXmlAccessory = _objXmlDocument.SelectSingleNode("/chummer/accessories/accessory[name = \"" + lstAccessory.SelectedValue + "\"]");

			if (objXmlAccessory.InnerXml.Contains("<rc>"))
			{
				lblRC.Visible = true;
				lblRCLabel.Visible = true;
				lblRC.Text = objXmlAccessory["rc"].InnerText;
			}
			else
			{
				lblRC.Visible = false;
				lblRCLabel.Visible = false;
			}
			if (Convert.ToInt32(objXmlAccessory["rating"].InnerText) > 0)
			{
				nudRating.Enabled = true;
				nudRating.Visible = true;
				lblRatingLabel.Visible = true;
				nudRating.Maximum = Convert.ToInt32(objXmlAccessory["rating"].InnerText);
            }
			else
			{
				nudRating.Value = Convert.ToInt32(objXmlAccessory["rating"].InnerText);
				nudRating.Visible = false;
				lblRatingLabel.Visible = false;
			}
			List<string> strMounts = new List<string>();
			foreach (string strItem in (objXmlAccessory["mount"].InnerText.Split('/')))
			{
				strMounts.Add(strItem);
			}
			strMounts.Add("None");

			List<string> strAllowed = new List<string>();
			foreach (string strItem in (_strAllowedMounts.Split('/')))
			{
				strAllowed.Add(strItem);
			}
			strAllowed.Add("None");
			cboMount.Items.Clear();
			foreach (string strCurrentMount in strMounts)
			{
				if (strCurrentMount != "")
				{
					foreach (string strAllowedMount in strAllowed)
					{
						if (strCurrentMount == strAllowedMount)
						{
							cboMount.Items.Add(strCurrentMount);
						}
					}
				}
			}
			if (cboMount.Items.Count <= 1)
			{
				cboMount.Enabled = false;
			}
			else
			{
				cboMount.Enabled = true;
			}
			cboMount.SelectedIndex = 0;
			// Avail.
			// If avail contains "F" or "R", remove it from the string so we can use the expression.
			string strAvail = "";
			string strAvailExpr = objXmlAccessory["avail"].InnerText;
			XPathExpression xprAvail;
			XPathNavigator nav = _objXmlDocument.CreateNavigator();
			if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
			{
				strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
				// Remove the trailing character if it is "F" or "R".
				strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
			}
			try
			{
				xprAvail = nav.Compile(strAvailExpr.Replace("Rating", nudRating.Value.ToString()));
				lblAvail.Text = (Convert.ToInt32(nav.Evaluate(xprAvail))).ToString() + strAvail;
			}
			catch
			{
				lblAvail.Text = objXmlAccessory["avail"].InnerText;
			}
			lblAvail.Text = lblAvail.Text.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted")).Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));
			string strCost = objXmlAccessory["cost"].InnerText;
			strCost = strCost.Replace("Weapon Cost", _intWeaponCost.ToString()).Replace("Rating", nudRating.Value.ToString());
			if (chkFreeItem.Checked)
				strCost = "0";

			if (strCost.Contains("Variable"))
			{
				lblCost.Text = strCost;
				lblTest.Text = "";
			}
			else
			{
				XPathExpression xprCost = nav.Compile(strCost);
				int intCost = Convert.ToInt32(Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.Instance.CultureInfo));

				// Apply any markup.
				double dblCost = Convert.ToDouble(intCost, GlobalOptions.Instance.CultureInfo);
				dblCost *= 1 + (Convert.ToDouble(nudMarkup.Value, GlobalOptions.Instance.CultureInfo) / 100.0);
				intCost = Convert.ToInt32(dblCost);

				lblCost.Text = String.Format("{0:###,###,##0¥}", intCost);
				lblTest.Text = _objCharacter.AvailTest(intCost, lblAvail.Text);
			}

			string strBook = _objCharacter.Options.LanguageBookShort(objXmlAccessory["source"].InnerText);
			string strPage = objXmlAccessory["page"].InnerText;
			if (objXmlAccessory["altpage"] != null)
				strPage = objXmlAccessory["altpage"].InnerText;
			lblSource.Text = strBook + " " + strPage;

			tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlAccessory["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
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
		/// Name of Accessory that was selected in the dialogue.
		/// </summary>
		public string SelectedAccessory
		{
			get
			{
				return _strSelectedAccessory;
			}
		}

		/// <summary>
		/// Mount that was selected in the dialogue.
		/// </summary>
		public string SelectedMount
		{
			get
			{
				return cboMount.SelectedItem.ToString();
			}
		}

		/// <summary>
		/// Rating of the Accessory.
		/// </summary>
		public string SelectedRating
		{
			get
			{
				return nudRating.Value.ToString();
			}
		}

		/// <summary>
		/// Mounts that the Weapon allows to be used.
		/// </summary>
		public string AllowedMounts
		{
			set
			{
				_strAllowedMounts = value;
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
		/// Weapon's Accessory Cost Multiplier.
		/// </summary>
		public int AccessoryMultiplier
		{
			set
			{
				_intAccessoryMultiplier = value;
			}
		}

		/// <summary>
		/// Weapon's Cost.
		/// </summary>
		public int WeaponCost
		{
			set
			{
				_intWeaponCost = value;
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
			_strSelectedAccessory = lstAccessory.SelectedValue.ToString();
			_intRating = Convert.ToInt32(nudRating.Value.ToString());
			_intMarkup = Convert.ToInt32(nudMarkup.Value);
			_blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
			this.DialogResult = DialogResult.OK;
		}

		private void MoveControls()
		{
			int intWidth = Math.Max(lblRCLabel.Width, lblMountLabel.Width);
			intWidth = Math.Max(intWidth, lblAvailLabel.Width);
			intWidth = Math.Max(intWidth, lblCostLabel.Width);

			lblRC.Left = lblRCLabel.Left + intWidth + 6;
			//lblMount.Left = lblMountLabel.Left + intWidth + 6;
			lblAvail.Left = lblAvailLabel.Left + intWidth + 6;
			lblTestLabel.Left = lblAvail.Left + lblAvail.Width + 16;
			lblTest.Left = lblTestLabel.Left + lblTestLabel.Width + 6;
			lblCost.Left = lblCostLabel.Left + intWidth + 6;

			nudMarkup.Left = lblMarkupLabel.Left + lblMarkupLabel.Width + 6;
			lblMarkupPercentLabel.Left = nudMarkup.Left + nudMarkup.Width;

			lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;
		}
		#endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions objCommon = new CommonFunctions(_objCharacter);
            objCommon.OpenPDF(lblSource.Text);
        }
	}
}