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
 using Chummer.Backend.Equipment;

namespace Chummer
{
	public partial class frmSelectVehicleMod : Form
	{
		private Vehicle _objVehicle;
		private string _strSelectedMod = "";
		private int _intSelectedRating = 0;
		private int _intWeaponCost = 0;
		private int _intTotalWeaponCost = 0;
		private int _intModMultiplier = 1;
		private string _strInputFile = "vehicles";
		private int _intMarkup = 0;
		private static string _strSelectCategory = "";

		private string _strAllowedCategories = "";
		private bool _blnAddAgain = false;

		private XmlNodeList objXmlCategoryList;
		private XmlDocument _objXmlDocument = new XmlDocument();
		private readonly Character _objCharacter;
		private bool _blnBlackMarketDiscount;
		private string _strLimitToCategories;
		private List<ListItem> _lstCategory = new List<ListItem>();

		#region Control Events
		public frmSelectVehicleMod(Character objCharacter, bool blnCareer = false)
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			lblMarkupLabel.Visible = blnCareer;
			nudMarkup.Visible = blnCareer;
			lblMarkupPercentLabel.Visible = blnCareer;
			_objCharacter = objCharacter;
			MoveControls();
		}

		private void frmSelectVehicleMod_Load(object sender, EventArgs e)
		{
			// Load the Mod information.
			_objXmlDocument = XmlManager.Instance.Load(_strInputFile + ".xml");

			// Populate the Weapon Category list.
			if (!string.IsNullOrEmpty(_strLimitToCategories))
			{
				string[] strValues = _strLimitToCategories.Split(',');

				// Populate the Category list.
				XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/modcategories/category");
				foreach (XmlNode objXmlCategory in objXmlNodeList)
				{
					foreach (string strCategory in strValues)
					{
						if (strCategory == objXmlCategory.InnerText)
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
							{
								objItem.Name = objXmlCategory.InnerXml;
							}
							_lstCategory.Add(objItem);
						}
					}
				}
			}
			else
			{
				objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/modcategories/category");

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
			}
			SortListItem objSort = new SortListItem();
			_lstCategory.Sort(objSort.Compare);
			cboCategory.ValueMember = "Value";
			cboCategory.DisplayMember = "Name";
			cboCategory.DataSource = _lstCategory;

			BuildModList();

			chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

			if (_strInputFile == "weapons")
				this.Text = LanguageManager.Instance.GetString("Title_SelectVehicleMod_Weapon");
		}

		private void lstMod_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateGearInfo();
		}

		private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
		{
			List<ListItem> lstMods = new List<ListItem>();
			XmlNodeList objXmlModList = null;
			// Populate the Mod list.
			if (cboCategory.SelectedValue.ToString() != "All")
			{
				objXmlModList =
					_objXmlDocument.SelectNodes("/chummer/mods/mod[(" + _objCharacter.Options.BookXPath() + ") and category = \"" + cboCategory.SelectedValue + "\"]");
			}
			else
			{
				objXmlModList =
					_objXmlDocument.SelectNodes("/chummer/mods/mod[" + _objCharacter.Options.BookXPath() + "]");
			}
			foreach (XmlNode objXmlMod in objXmlModList)
			{
					ListItem objItem = new ListItem();
					objItem.Value = objXmlMod["name"].InnerText;
					if (objXmlMod["translate"] != null)
						objItem.Name = objXmlMod["translate"].InnerText;
					else
						objItem.Name = objXmlMod["name"].InnerText;
					lstMods.Add(objItem);
			}
			lstMod.DataSource = null;
			lstMod.ValueMember = "Value";
			lstMod.DisplayMember = "Name";
			lstMod.DataSource = lstMods;
		}

		private void nudRating_ValueChanged(object sender, EventArgs e)
		{
			UpdateGearInfo();
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			if (lstMod.Text != "")
				AcceptForm();
		}

		private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
		{
			UpdateGearInfo();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			_strSelectCategory = "";
			this.DialogResult = DialogResult.Cancel;
		}

		private void lstMod_DoubleClick(object sender, EventArgs e)
		{
			if (lstMod.Text != "")
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

		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
			BuildModList();
		}

		private void nudMarkup_ValueChanged(object sender, EventArgs e)
		{
			UpdateGearInfo();
		}

		private void txtSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
			{
				try
				{
					lstMod.SelectedIndex++;
				}
				catch
				{
					try
					{
						lstMod.SelectedIndex = 0;
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
					lstMod.SelectedIndex--;
					if (lstMod.SelectedIndex == -1)
						lstMod.SelectedIndex = lstMod.Items.Count - 1;
				}
				catch
				{
					try
					{
						lstMod.SelectedIndex = lstMod.Items.Count - 1;
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
		/// Vehicle's Cost.
		/// </summary>
		public Vehicle SelectedVehicle
		{
			set
			{
				_objVehicle = value;
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
		/// Weapon's Total Cost.
		/// </summary>
		public int TotalWeaponCost
		{
			set
			{
				_intTotalWeaponCost = value;
			}
		}

		/// <summary>
		/// Weapon's Modification Cost Multiplier.
		/// </summary>
		public int ModMultiplier
		{
			set
			{
				_intModMultiplier = value;
			}
		}

		/// <summary>
		/// Name of the Mod that was selected in the dialogue.
		/// </summary>
		public string SelectedMod
		{
			get
			{
				return _strSelectedMod;
			}
		}

		/// <summary>
		/// Rating that was selected in the dialogue.
		/// </summary>
		public int SelectedRating
		{
			get
			{
				return _intSelectedRating;
			}
		}

		/// <summary>
		/// Categories that the Gear allows to be used.
		/// </summary>
		public string AllowedCategories
		{
			get
			{
				return _strAllowedCategories;
			}
			set
			{
				_strAllowedCategories = value;
			}
		}

		/// <summary>
		/// Which XML file should the window read from (default vehicles).
		/// </summary>
		public string InputFile
		{
			set
			{
				_strInputFile = value;
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
		/// Whether or not the Vehicle has the Modular Electronics Vehicle Mod.
		/// </summary>
		/*
		public bool HasModularElectronics
		{
			set
			{
				_blnModularElectronics = value;

				if (_blnModularElectronics)
				{
					_intMaxResponse = 10;
					_intMaxSystem = 10;
					_intMaxFirewall = 10;
					_intMaxSignal = 10;
				}
			}
		}*/

		/// <summary>
		/// Vehicle's Device Rating.
		/// </summary>
		/*
		public int DeviceRating
		{
			set
			{
				_intMaxResponse = value + 2;
				_intMaxSystem = value;
				_intMaxFirewall = value;
				_intMaxSignal = value + 2;
				_intDeviceRating = value;

				if (_blnModularElectronics)
				{
					_intMaxResponse = 10;
					_intMaxSystem = 10;
					_intMaxFirewall = 10;
					_intMaxSignal = 10;
				}
			}
		}*/

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
		/// Build the list of Mods.
		/// </summary>
		private void BuildModList()
		{

			// Select the first Category in the list.
			if (_strSelectCategory == "")
				cboCategory.SelectedIndex = 0;
			else
				cboCategory.SelectedValue = _strSelectCategory;

			foreach (Label objLabel in this.Controls.OfType<Label>())
			{
				if (objLabel.Text.StartsWith("["))
					objLabel.Text = "";
			}

			// Update the list of Mods based on the selected Category.
			XmlNodeList objXmlModList;

			// Load the Mod information.
			_objXmlDocument = XmlManager.Instance.Load(_strInputFile + ".xml");

			// Retrieve the list of Mods for the selected Category.
			if (txtSearch.Text == "")
				objXmlModList = _objXmlDocument.SelectNodes("/chummer/mods/mod[(" + _objCharacter.Options.BookXPath() + ") and category != \"Special\"]");
			else
				objXmlModList = _objXmlDocument.SelectNodes("/chummer/mods/mod[(" + _objCharacter.Options.BookXPath() + ") and category != \"Special\" and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))]");
			bool blnAdd = true;
			List<ListItem> lstMods = new List<ListItem>();
			if (objXmlModList != null)
				foreach (XmlNode objXmlMod in objXmlModList)
				{
					blnAdd = true;
					XmlNode objXmlRequirements = objXmlMod.SelectSingleNode("requires");
					if (objXmlRequirements != null)
					{
						if (_objVehicle.Seats < Convert.ToInt32(objXmlRequirements["seats"]?.InnerText))
						{
							blnAdd = false;
						}
					}

					if (blnAdd)
					{
						ListItem objItem = new ListItem();
						objItem.Value = objXmlMod["name"].InnerText;
						objItem.Name = objXmlMod["translate"]?.InnerText ?? objXmlMod["name"].InnerText;
						lstMods.Add(objItem);
					}
				}
			SortListItem objSort = new SortListItem();
			lstMods.Sort(objSort.Compare);
			lstMod.DataSource = null;
			lstMod.ValueMember = "Value";
			lstMod.DisplayMember = "Name";
			lstMod.DataSource = lstMods;
		}
		
		/// <summary>
		/// Accept the selected item and close the form.
		/// </summary>
		private void AcceptForm()
		{
			_strSelectedMod = lstMod.SelectedValue.ToString();
			_intSelectedRating = Convert.ToInt32(nudRating.Value);
			_intMarkup = Convert.ToInt32(nudMarkup.Value);
			_blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
			_strSelectCategory = cboCategory.SelectedValue.ToString();
			this.DialogResult = DialogResult.OK;
		}

		/// <summary>
		/// Update the Mod's information based on the Mod selected and current Rating.
		/// </summary>
		private void UpdateGearInfo()
		{
			if (lstMod.Text != "")
			{
				// Retireve the information for the selected Mod.
				// Filtering is also done on the Category in case there are non-unique names across categories.
				XmlNode objXmlMod = _objXmlDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + lstMod.SelectedValue + "\"]");

				// Extract the Avil and Cost values from the Gear info since these may contain formulas and/or be based off of the Rating.
				// This is done using XPathExpression.
				XPathNavigator nav = _objXmlDocument.CreateNavigator();

				// Avail.
				// If avail contains "F" or "R", remove it from the string so we can use the expression.
				string strAvail = "";
				string strAvailExpr = "";
                if (objXmlMod["avail"].InnerText.StartsWith("FixedValues"))
				{
					int intRating = Convert.ToInt32(nudRating.Value - 1);
					string[] strValues = objXmlMod["avail"].InnerText.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
					if (intRating > strValues.Length || intRating < 0)
					{
						intRating = strValues.Length -1;
					}
						strAvailExpr = strValues[intRating];
				}
				else
				{
					strAvailExpr = objXmlMod["avail"].InnerText;
				}
				
				XPathExpression xprAvail;
				if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
				{
					strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
					// Remove the trailing character if it is "F" or "R".
					strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
				}
				try
				{
					xprAvail = nav.Compile(strAvailExpr.Replace("Rating", Math.Max(nudRating.Value,1).ToString()));
					lblAvail.Text = Convert.ToInt32(nav.Evaluate(xprAvail)) + strAvail;
				}
				catch
				{
					lblAvail.Text = objXmlMod["avail"].InnerText;
				}
				lblAvail.Text = lblAvail.Text.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted")).Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));

				// Cost.
				int intItemCost = 0;
				if (objXmlMod["cost"].InnerText.StartsWith("Variable"))
				{
					int intMin = 0;
					int intMax = 0;
					string strCost = objXmlMod["cost"].InnerText.Replace("Variable(", string.Empty).Replace(")", string.Empty);
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
				else
				{
					string strCost = "";
                    if (objXmlMod["cost"].InnerText.StartsWith("FixedValues"))
					{
						int intRating = Convert.ToInt32(nudRating.Value) - 1;
						string[] strValues = objXmlMod["cost"].InnerText.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
						if (intRating < 0 || intRating > strValues.Length)
						{
							intRating = 0;
						}
						strCost = strValues[intRating];
					}
					else
					{
						strCost = objXmlMod["cost"].InnerText;
					}
					strCost = ReplaceStrings(strCost);

					if (chkFreeItem.Checked)
						strCost = "0";

					XPathExpression xprCost = nav.Compile(strCost);
					int intCost = Convert.ToInt32(Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.Instance.CultureInfo));
					intCost *= _intModMultiplier;

					// Apply any markup.
					double dblCost = Convert.ToDouble(intCost, GlobalOptions.Instance.CultureInfo);
					dblCost *= 1 + (Convert.ToDouble(nudMarkup.Value, GlobalOptions.Instance.CultureInfo) / 100.0);
					
					if (chkBlackMarketDiscount.Checked)
					{
						dblCost = dblCost - (dblCost*0.90);
					}

					intCost = Convert.ToInt32(dblCost);
					lblCost.Text = String.Format("{0:###,###,##0¥}", intCost);

					intItemCost = intCost;
				}

				// Update the Avail Test Label.
				lblTest.Text = _objCharacter.AvailTest(intItemCost, lblAvail.Text);

				// If the rating is "qty", we're looking at Tires instead of actual Rating, so update the fields appropriately.
				if (objXmlMod["rating"].InnerText == "qty")
				{
					nudRating.Enabled = true;
					nudRating.Maximum = 20;
					nudRating.Minimum = 1;
					lblRatingLabel.Text = LanguageManager.Instance.GetString("Label_Qty");
				}
				//Used for the Armor modifications.
				else if (objXmlMod["rating"].InnerText.ToLower() == "body")
				{
					nudRating.Maximum = _objVehicle.Body;
					nudRating.Minimum = 1;
					nudRating.Enabled = true;
					lblRatingLabel.Text = LanguageManager.Instance.GetString("Label_Body");
				}
				//Used for Metahuman Adjustments.
				else if (objXmlMod["rating"].InnerText.ToLower() == "seats")
				{
					nudRating.Maximum = _objVehicle.Seats;
					nudRating.Minimum = 1;
					nudRating.Enabled = true;
					lblRatingLabel.Text = LanguageManager.Instance.GetString("Label_Qty");
				}
				else 
				{
					if (Convert.ToInt32(objXmlMod["rating"].InnerText) > 0)
					{
						nudRating.Maximum = Convert.ToInt32(objXmlMod["rating"].InnerText);
						nudRating.Minimum = 1;
						nudRating.Enabled = true;
						lblRatingLabel.Text = LanguageManager.Instance.GetString("Label_Rating");
					}
					else
					{
						nudRating.Minimum = 0;
						nudRating.Maximum = 0;
						nudRating.Enabled = false;
						lblRatingLabel.Text = LanguageManager.Instance.GetString("Label_Rating");
					}
				}

				// Slots.

				string strSlots = "";
				if (objXmlMod["slots"].InnerText.StartsWith("FixedValues"))
				{
					string[] strValues = objXmlMod["slots"].InnerText.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
					strSlots = strValues[Convert.ToInt32(nudRating.Value) - 1];
				}
				else
				{
					strSlots = objXmlMod["slots"].InnerText;
				}
				strSlots = ReplaceStrings(strSlots);
				XPathExpression xprSlots = nav.Compile(strSlots);
				lblSlots.Text = nav.Evaluate(xprSlots).ToString();

				try
				{
					if (objXmlMod["category"].InnerText == "Weapon Mod")
						lblCategory.Text = LanguageManager.Instance.GetString("String_WeaponModification");
					else
					{
						// Translate the Category if possible.
						if (GlobalOptions.Instance.Language != "en-us")
						{
							XmlNode objXmlCategory = _objXmlDocument.SelectSingleNode("/chummer/modcategories/category[. = \"" + objXmlMod["category"].InnerText + "\"]");
							if (objXmlCategory != null)
							{
								if (objXmlCategory.Attributes["translate"] != null)
									lblCategory.Text = objXmlCategory.Attributes["translate"].InnerText;
								else
									lblCategory.Text = objXmlMod["category"].InnerText;
							}
							else
								lblCategory.Text = objXmlMod["category"].InnerText;
						}
						else
							lblCategory.Text = objXmlMod["category"].InnerText;
					}
				}
				catch
				{
					lblCategory.Text = LanguageManager.Instance.GetString("String_WeaponModification");
				}

				if (objXmlMod["limit"] != null)
				{
					// Translate the Limit if possible.
					if (GlobalOptions.Instance.Language != "en-us")
					{
						XmlNode objXmlLimit = _objXmlDocument.SelectSingleNode("/chummer/limits/limit[. = \"" + objXmlMod["limit"].InnerText + "\"]");
						if (objXmlLimit != null)
						{
							if (objXmlLimit.Attributes["translate"] != null)
								lblLimit.Text = " (" + objXmlLimit.Attributes["translate"].InnerText + ")";
							else
								lblLimit.Text = " (" + objXmlMod["limit"].InnerText + ")";
						}
						else
							lblLimit.Text = " (" + objXmlMod["limit"].InnerText + ")";
					}
					else
						lblLimit.Text = " (" + objXmlMod["limit"].InnerText + ")";
				}
				else
					lblLimit.Text = "";

				string strBook = _objCharacter.Options.LanguageBookShort(objXmlMod["source"].InnerText);
				string strPage = objXmlMod["page"].InnerText;
				if (objXmlMod["altpage"] != null)
					strPage = objXmlMod["altpage"].InnerText;
				lblSource.Text = strBook + " " + strPage;

				tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlMod["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
			}
		}

		private void MoveControls()
		{
			int intWidth = Math.Max(lblCategoryLabel.Width, lblAvailLabel.Width);
			intWidth = Math.Max(intWidth, lblCostLabel.Width);
			intWidth = Math.Max(intWidth, lblSlotsLabel.Width);
			intWidth = Math.Max(intWidth, lblRatingLabel.Width);

			lblCategory.Left = lblCategoryLabel.Left + intWidth + 6;
			lblLimit.Left = lblCategoryLabel.Left + intWidth + 6;
			lblAvail.Left = lblAvailLabel.Left + intWidth + 6;
			lblTestLabel.Left = lblAvail.Left + lblAvail.Width + 16;
			lblTest.Left = lblTestLabel.Left + lblTestLabel.Width + 6;
			lblCost.Left = lblCostLabel.Left + intWidth + 6;
			lblSlots.Left = lblSlotsLabel.Left + intWidth + 6;
			nudRating.Left = lblRatingLabel.Left + intWidth + 6;

			nudMarkup.Left = lblMarkupLabel.Left + lblMarkupLabel.Width + 6;
			lblMarkupPercentLabel.Left = nudMarkup.Left + nudMarkup.Width;

			lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;

			lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
		}
		private string ReplaceStrings(string strInput)
		{
			strInput = strInput.Replace("Rating", nudRating.Value.ToString());
			strInput = strInput.Replace("Vehicle Cost", _objVehicle.Cost.ToString());
			strInput = strInput.Replace("Weapon Cost", _intWeaponCost.ToString());
			strInput = strInput.Replace("Total Cost", _intTotalWeaponCost.ToString());
			strInput = strInput.Replace("Body", _objVehicle.Body.ToString());
			strInput = strInput.Replace("Handling", _objVehicle.Handling.ToString());
			strInput = strInput.Replace("Offroad Handling", _objVehicle.OffroadHandling.ToString());
			strInput = strInput.Replace("Speed", _objVehicle.Speed.ToString());
			strInput = strInput.Replace("Acceleration", _objVehicle.Accel.ToString());

			return strInput;
		}
		#endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions objCommon = new CommonFunctions(_objCharacter);
            objCommon.OpenPDF(lblSource.Text);
        }
	}
}