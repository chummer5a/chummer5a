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
 using Chummer.Skills;

namespace Chummer
{
    public partial class frmSelectCyberware : Form
    {
		private string _strSelectedCyberware = "";
		private Grade _objSelectedGrade;
		private int _intSelectedRating = 0;
		private readonly Character _objCharacter;
        private Vehicle _objVehicle;
		private int _intSelectedESSDiscount = 0;

        private double _dblCostMultiplier = 1.0;
        private double _dblESSMultiplier = 1.0;
		private int _intAvailModifier = 0;
		private int _intMarkup = 0;
		private double _dblCharacterESSModifier = 1.0;
		private double _dblGenetechCostModifier = 1.0;
		private double _dblBasicBiowareESSModifier = 1.0;
		private double _dblTransgenicsBiowareCostModifier = 1.0;
		private bool _blnCareer = false;

		private string _strSetGrade = "";
		private bool _blnShowOnlySubsystems = false;
		private string _strSubsystems = "";
		private int _intMaximumCapacity = -1;
		private bool _blnLockGrade = false;

		private Mode _objMode = Mode.Cyberware;
		private string _strNode = "cyberware";
		private bool _blnAddAgain = false;
		private bool _blnAllowModularPlugins = false;
        private bool _blnShowOnlyLimbs = false;
	    private bool _blnBlackMarketDiscount = false;
		private static string _strSelectCategory = "";
		private static string _strSelectedGrade = "";

		private XmlDocument _objXmlDocument = new XmlDocument();
		private XmlDocument _objMetatypeDocument = new XmlDocument();

		private List<ListItem> _lstCategory = new List<ListItem>();
		private List<ListItem> _lstGrade = new List<ListItem>();
		

		public enum Mode
		{
			Cyberware = 0,
			Bioware = 1,
		}

		#region Control Events
		public frmSelectCyberware(Character objCharacter, bool blnCareer = false)
        {
            InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			chkFree.Visible = blnCareer;
			lblMarkupLabel.Visible = blnCareer;
			nudMarkup.Visible = blnCareer;
			lblMarkupPercentLabel.Visible = blnCareer;
			_blnCareer = blnCareer;
			_objCharacter = objCharacter;
			MoveControls();
			
		}

        private void frmSelectCyberware_Load(object sender, EventArgs e)
        {
			// Update the window title if needed.
			if (_strNode == "bioware")
				this.Text = LanguageManager.Instance.GetString("Title_SelectCyberware_Bioware");

			foreach (Label objLabel in this.Controls.OfType<Label>())
			{
				if (objLabel.Text.StartsWith("["))
					objLabel.Text = "";
			}

        	// Load the Cyberware information.
			switch (_objMode)
			{
				case Mode.Cyberware:
					_objXmlDocument = XmlManager.Instance.Load("cyberware.xml");
					break;
				case Mode.Bioware:
					_objXmlDocument = XmlManager.Instance.Load("bioware.xml");
					break;
			}

			// Populate the Cyberware Category list.
			XmlNodeList objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
			foreach (XmlNode objXmlCategory in objXmlCategoryList)
			{
				// Make sure the Category isn't in the exclusion list.
				bool blnAddItem = true;

				if (objXmlCategory.Attributes["show"] != null)
					blnAddItem = _blnAllowModularPlugins;

			    if (_blnShowOnlyLimbs)
			        blnAddItem = objXmlCategory.InnerText == "Cyberlimb";

				if (blnAddItem)
				{
					ListItem objItem = new ListItem();
					objItem.Value = objXmlCategory.InnerText;
					if (objXmlCategory.Attributes["translate"] != null)
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
			cboCategory.ValueMember = "Value";
			cboCategory.DisplayMember = "Name";
			cboCategory.DataSource = _lstCategory;

			chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

			// Populate the Grade list. Do not show the Adapsin Grades if Adapsin is not enabled for the character.
			PopulateGrades();

			if (_strSetGrade == "")
			{
				if (_strSelectedGrade == "")
					cboGrade.SelectedIndex = 0;
				else
					cboGrade.SelectedValue = _strSelectedGrade;
			}
			else
				cboGrade.SelectedValue = _strSetGrade;

			if (cboGrade.SelectedIndex == -1)
				cboGrade.SelectedIndex = 0;

			// Select the first Category in the list.
			if (_strSelectCategory == "")
				cboCategory.SelectedIndex = 0;
			else
			{
				try
				{
					cboCategory.SelectedValue = _strSelectCategory;
				}
				catch
				{
				}
			}

			if (cboCategory.SelectedIndex == -1)
				cboCategory.SelectedIndex = 0;

			lblESSDiscountLabel.Visible = _objCharacter.Options.AllowCyberwareESSDiscounts;
			lblESSDiscountPercentLabel.Visible = _objCharacter.Options.AllowCyberwareESSDiscounts;
			nudESSDiscount.Visible = _objCharacter.Options.AllowCyberwareESSDiscounts;

			if (_objMode == Mode.Bioware && _objCharacter.Options.AllowCustomTransgenics)
				chkTransgenic.Visible = true;
			else
				chkTransgenic.Visible = false;
        }

        private void cboGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
			if (cboGrade.SelectedValue == null)
				return;

            // Update the Essence and Cost multipliers based on the Grade that has been selected.
        	// Retrieve the information for the selected Grade.
            XmlNode objXmlGrade = _objXmlDocument.SelectSingleNode("/chummer/grades/grade[name = \"" + cboGrade.SelectedValue + "\"]");
			_dblCostMultiplier = Convert.ToDouble(objXmlGrade["cost"].InnerText, GlobalOptions.Instance.CultureInfo);
			_dblESSMultiplier = Convert.ToDouble(objXmlGrade["ess"].InnerText, GlobalOptions.Instance.CultureInfo);
			_intAvailModifier = Convert.ToInt32(objXmlGrade["avail"].InnerText);

            UpdateCyberwareInfo();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
			if (_objMode == Mode.Bioware)
			{
				// If the window is currently showing Bioware, we may need to rebuild the Grade list since Cultured Bioware is not allowed to select Standard (Second-Hand) as as Grade.
				if (cboGrade.SelectedValue != null)
				{
					string strSelectedValue = cboGrade.SelectedValue.ToString();
					bool blnCultured = cboCategory.SelectedValue.ToString() == "Cultured";
					PopulateGrades(blnCultured);
					cboGrade.SelectedValue = strSelectedValue;
				}
				if (cboGrade.SelectedIndex == -1 && cboGrade.Items.Count > 0)
					cboGrade.SelectedIndex = 0;
			}

            // Update the list of Cyberware based on the selected Category.
            XmlNodeList objXmlCyberwareList;
			List<ListItem> lstCyberwares = new List<ListItem>();

	        if (cboCategory.SelectedValue.ToString().StartsWith("Genetech:") ||
	            cboCategory.SelectedValue.ToString() == "Symbionts" || 
				cboCategory.SelectedValue.ToString() == "Genemods" ||
	            _blnLockGrade)
	        {
		        cboGrade.Enabled = false;

	        }
	        else
	        {
		        cboGrade.Enabled = true;
	        }

	        if (cboCategory.SelectedValue.ToString().StartsWith("Genetech:") ||
	            cboCategory.SelectedValue.ToString() == "Symbionts" || 
				cboCategory.SelectedValue.ToString() == "Genemods")
	        {
		        cboGrade.SelectedValue = "Standard";
	        }

            // Retrieve the list of Cyberware for the selected Category.
			if (_blnShowOnlySubsystems)
				objXmlCyberwareList = _objXmlDocument.SelectNodes("/chummer/" + _strNode + "s/" + _strNode + "[category = \"" + cboCategory.SelectedValue + "\" and (" + _objCharacter.Options.BookXPath() + ") and contains(capacity, \"[\")]");
			else
				objXmlCyberwareList = _objXmlDocument.SelectNodes("/chummer/" + _strNode + "s/" + _strNode + "[category = \"" + cboCategory.SelectedValue + "\" and (" + _objCharacter.Options.BookXPath() + ")]");
			foreach (XmlNode objXmlCyberware in objXmlCyberwareList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlCyberware["name"].InnerText;
				if (objXmlCyberware["translate"] != null)
					objItem.Name = objXmlCyberware["translate"].InnerText;
				else
					objItem.Name = objXmlCyberware["name"].InnerText;
				lstCyberwares.Add(objItem);
			}
			SortListItem objSort = new SortListItem();
			lstCyberwares.Sort(objSort.Compare);
			lstCyberware.DataSource = null;
			lstCyberware.ValueMember = "Value";
			lstCyberware.DisplayMember = "Name";
			lstCyberware.DataSource = lstCyberwares;
        }

		private void lstCyberware_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lstCyberware.Text == "")
				return;

			if ((cboCategory.SelectedValue.ToString().Contains("Genetech:") && _dblTransgenicsBiowareCostModifier != 1.0) || _blnCareer)
				chkFree.Visible = true;
			else
			{
				chkFree.Visible = false;
				chkFree.Checked = false;
			}
			if (chkTransgenic.Checked)
				chkFree.Visible = true;

			// Retireve the information for the selected piece of Cyberware.
			XmlNode objXmlCyberware = _objXmlDocument.SelectSingleNode("/chummer/" + _strNode + "s/" + _strNode + "[name = \"" + lstCyberware.SelectedValue + "\"]");

			// If the piece has a Rating value, enable the Rating control, otherwise, disable it and set its value to 0.
			if (objXmlCyberware.InnerXml.Contains("<rating>"))
			{
				nudRating.Enabled = true;
			    if (objXmlCyberware["rating"].InnerText == "MaximumSTR")
			    {
                    if (_objVehicle != null)
                    {
                        nudRating.Maximum = _objVehicle.TotalBody*2;
	                    nudRating.Minimum = _objVehicle.TotalBody;
                    }
                    else
                    {
                        nudRating.Maximum = _objCharacter.STR.TotalMaximum;
                    }
                }
                else if (objXmlCyberware["rating"].InnerText == "MaximumAGI")
			    {
                    if (_objVehicle != null)
                    {
                        nudRating.Maximum = _objVehicle.Pilot*2;
                    }
                    else
                    {
                        nudRating.Maximum = _objCharacter.AGI.TotalMaximum;
                    }
                }
			    else
			    {
			        nudRating.Maximum = Convert.ToInt32(objXmlCyberware["rating"].InnerText);
			    }
				if (objXmlCyberware["minrating"] != null)
				{
					switch (objXmlCyberware["minrating"].InnerText)
					{
						case "MinimumAGI":
							if (_objVehicle != null)
							{
								nudRating.Minimum = _objVehicle.Pilot;
							}
							else
							{
								nudRating.Minimum = 4;
							}
							break;
						case "MinimumSTR":
							if (_objVehicle != null)
							{
								nudRating.Minimum = _objVehicle.TotalBody;
							}
							else
							{
								nudRating.Minimum = 4;
							}
							break;
						default:
							nudRating.Minimum = Convert.ToInt32(objXmlCyberware["minrating"].InnerText);
							break;
					}
				}
				else
				{
					nudRating.Minimum = 1;
				}
			}
			else
			{
				nudRating.Minimum = 0;
				nudRating.Value = 0;
				nudRating.Enabled = false;
			}


			if (objXmlCyberware["category"].InnerText.StartsWith("Genetech:") ||
				objXmlCyberware["category"].InnerText.StartsWith("Symbionts") ||
				objXmlCyberware["category"].InnerText.StartsWith("Genemods") ||
				_blnLockGrade)
			{
				cboGrade.Enabled = false;

			}
			else
			{
				cboGrade.Enabled = true;
			}

			if (objXmlCyberware["category"].InnerText.StartsWith("Genetech:") ||
				objXmlCyberware["category"].InnerText.StartsWith("Symbionts") ||
				objXmlCyberware["category"].InnerText.StartsWith("Genemods"))
            {
				cboGrade.SelectedValue = "Standard";
			}

			if (objXmlCyberware["forcegrade"] != null)
			{
				// Force the Cyberware to be a particular Grade.
				cboGrade.SelectedValue = objXmlCyberware["forcegrade"].InnerText;
				cboGrade.Enabled = false;
			}

			string strBook = _objCharacter.Options.LanguageBookShort(objXmlCyberware["source"].InnerText);
			string strPage = objXmlCyberware["page"].InnerText;
			if (objXmlCyberware["altpage"] != null)
				{
				strPage = objXmlCyberware["altpage"].InnerText;
				}
			lblSource.Text = strBook + " " + strPage;
			if (objXmlCyberware["notes"] != null)
				{
				lblCyberwareNotes.Visible = true;
				lblCyberwareNotesLabel.Visible = true;
                lblCyberwareNotes.Text = objXmlCyberware["notes"].InnerText;
				}

			if (objXmlCyberware["notes"] == null)
			{
				lblCyberwareNotes.Visible = false;
				lblCyberwareNotesLabel.Visible = false;
			}

			tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlCyberware["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);

            UpdateCyberwareInfo();
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateCyberwareInfo();
		}

		private void lblSource_Click(object sender, EventArgs e)
		{
			CommonFunctions objCommon = new CommonFunctions(_objCharacter);
			objCommon.OpenPDF(lblSource.Text);
		}

		private void nudMarkup_ValueChanged(object sender, EventArgs e)
		{
			UpdateCyberwareInfo();
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			if (lstCyberware.Text != "")
				AcceptForm();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void lblCategory_Click(object sender, EventArgs e)
		{

		}

		private void lstCyberware_DoubleClick(object sender, EventArgs e)
		{
			if (lstCyberware.Text != "")
				AcceptForm();
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

			List<ListItem> lstCyberwares = new List<ListItem>();
			string strCategoryFilter = "";

			foreach (ListItem objAllowedCategory in _lstCategory)
			{
				if (objAllowedCategory.Value != "")
					strCategoryFilter += "category = \"" + objAllowedCategory.Value + "\" or ";
			}
			
			// Treat everything as being uppercase so the search is case-insensitive.
			string strSearch = "/chummer/" + _strNode + "s/" + _strNode + "[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))";
			if (strCategoryFilter != "")
				strSearch += " and (" + strCategoryFilter + ")";
			// Remove the trailing " or ";
			strSearch = strSearch.Substring(0, strSearch.Length - 4) + ")";
			strSearch += "]";

			XmlNodeList objXmlCyberwareList = _objXmlDocument.SelectNodes(strSearch);
			foreach (XmlNode objXmlCyberware in objXmlCyberwareList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlCyberware["name"].InnerText;
				if (objXmlCyberware["translate"] != null)
					objItem.Name = objXmlCyberware["translate"].InnerText;
				else
					objItem.Name = objXmlCyberware["name"].InnerText;

				try
				{
					objItem.Name += " [" + _lstCategory.Find(objFind => objFind.Value == objXmlCyberware["category"].InnerText).Name + "]";
					lstCyberwares.Add(objItem);
				}
				catch
				{
				}
			}
			SortListItem objSort = new SortListItem();
			lstCyberwares.Sort(objSort.Compare);
			lstCyberware.DataSource = null;
			lstCyberware.ValueMember = "Value";
			lstCyberware.DisplayMember = "Name";
			lstCyberware.DataSource = lstCyberwares;
		}

		private void chkFree_CheckedChanged(object sender, EventArgs e)
		{
			UpdateCyberwareInfo();
		}

		private void nudESSDiscount_ValueChanged(object sender, EventArgs e)
		{
			UpdateCyberwareInfo();
		}
		private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
		{
			UpdateCyberwareInfo();
		}
		private void txtSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
			{
				try
				{
					lstCyberware.SelectedIndex++;
				}
				catch
				{
					try
					{
						lstCyberware.SelectedIndex = 0;
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
					lstCyberware.SelectedIndex--;
					if (lstCyberware.SelectedIndex == -1)
						lstCyberware.SelectedIndex = lstCyberware.Items.Count - 1;
				}
				catch
				{
					try
					{
						lstCyberware.SelectedIndex = lstCyberware.Items.Count - 1;
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

		private void chkTransgenic_CheckedChanged(object sender, EventArgs e)
		{
			if (chkTransgenic.Checked)
			{
				cboGrade.Enabled = false;
				cboGrade.SelectedValue = "Standard";
			}
			else
				cboGrade.Enabled = true;

			if ((cboCategory.SelectedValue.ToString().Contains("Genetech:") && _dblTransgenicsBiowareCostModifier != 1.0) || _blnCareer)
				chkFree.Visible = true;
			else
			{
				chkFree.Visible = false;
				chkFree.Checked = false;
			}
			if (chkTransgenic.Checked)
				chkFree.Visible = true;

			UpdateCyberwareInfo();
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
		/// Essence cost multiplier from the character.
		/// </summary>
		public double CharacterESSMultiplier
		{
			set
			{
				_dblCharacterESSModifier = value;
			}
		}

		/// <summary>
		/// Cost multiplier for Genetech.
		/// </summary>
		public double GenetechCostMultiplier
		{
			set
			{
				_dblGenetechCostModifier = value;
			}
		}

		/// <summary>
		/// Essence cost multiplier for Basic Bioware.
		/// </summary>
		public double BasicBiowareESSMultiplier
		{
			set
			{
				_dblBasicBiowareESSModifier = value;
			}
		}

		/// <summary>
		/// Cost multiplier for Transgenics Bioware.
		/// </summary>
		public double TransgenicsBiowareCostMultiplier
		{
			set
			{
				_dblTransgenicsBiowareCostModifier = value;
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

		/// <summary>
		/// Set the window's Mode to Cyberware or Bioware.
		/// </summary>
		public Mode WindowMode
		{
			get
			{
				return _objMode;
			}
			set
			{
				_objMode = value;
				switch (_objMode)
				{
					case Mode.Cyberware:
						_strNode = "cyberware";
						break;
					case Mode.Bioware:
						_strNode = "bioware";
						break;
				}
			}
		}

		/// <summary>
		/// Set the maximum Capacity the piece of Cyberware is allowed to be.
		/// </summary>
		public int MaximumCapacity
		{
			set
			{
				_intMaximumCapacity = value;
				lblMaximumCapacity.Text = LanguageManager.Instance.GetString("Label_MaximumCapacityAllowed") + " " + _intMaximumCapacity.ToString();
			}
		}

		/// <summary>
		/// Set whether or not only subsystems (those that consume Capacity) should be shown.
		/// </summary>
		public bool ShowOnlySubsystems
		{
			set
			{
				_blnShowOnlySubsystems = value;
			}
		}

		/// <summary>
		/// Comma-separate list of Categories to show for Subsystems.
		/// </summary>
		public string Subsystems
		{
			set
			{
				_strSubsystems = value;
			}
		}

    	/// <summary>
    	/// Manually set the Grade of the piece of Cyberware.
    	/// </summary>
    	public string SetGrade
		{
			set
			{
				_strSetGrade = value;
			}
		}

		/// <summary>
		/// Name of Cyberware that was selected in the dialogue.
		/// </summary>
		public string SelectedCyberware
		{
			get
			{
				return _strSelectedCyberware;
			}
		}

		/// <summary>
		/// Grade of the selected piece of Cyberware.
		/// </summary>
		public Grade SelectedGrade
		{
			get
			{
				return _objSelectedGrade;
			}
		}

		/// <summary>
		/// Rating of the selected piece of Cyberware (0 if not applicable).
		/// </summary>
		public int SelectedRating
		{
			get
			{
				return _intSelectedRating;
			}
		}

		/// <summary>
		/// Selected Essence cost discount.
		/// </summary>
		public int SelectedESSDiscount
		{
			get
			{
				return _intSelectedESSDiscount;
			}
		}

		/// <summary>
		/// Whether or not Modular Plugins are allowed.
		/// </summary>
		public bool AllowModularPlugins
		{
			set
			{
				_blnAllowModularPlugins = value;
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
		/// Whether or not the Bioware should be forced into the Genetech: Transgenics category.
		/// </summary>
		public bool ForceTransgenic
		{
			get
			{
				// If the Transgenics checkbox is checked, force it to the Genetech: Transgenics category.
				if (chkTransgenic.Checked)
					return true;
				else
					return false;
			}
		}

        /// <summary>
		/// Whether or not only Cyberlimb should be shown
		/// </summary>
		public bool ShowOnlyLimbs
        {
            set
            {
                _blnShowOnlyLimbs = value;
            }
        }

        /// <summary>
        /// Parent vehicle that the cyberlimb will be attached to.
        /// </summary>
        public Vehicle ParentVehicle
        {
            set
            {
                _objVehicle = value;
            }
            get
            {
                return _objVehicle;
            }
        }

	    public int Markup
	    {
		    get
		    {
			    return _intMarkup;
		    }
		    set
		    {
			    _intMarkup = value;
		    }
	    }

        #endregion

        #region Methods
        /// <summary>
        /// Update the Cyberware's information based on the Cyberware selected and current Rating.
        /// </summary>
        private void UpdateCyberwareInfo()
        {
			if (lstCyberware.Text != "")
			{
				XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/" + _strNode + "s/" + _strNode + "[name = \"" + lstCyberware.SelectedValue + "\"]");
				string strSelectCategory = objNode["category"].InnerText;
				// If the Transgenics checkbox has been checked, force it to the Genetech: Transgenics category instead.
				if (chkTransgenic.Checked)
					strSelectCategory = "Genetech: Transgenics";

				// Place the Essence cost multiplier in a variable that can be safely modified.
				double dblCharacterESSModifier = _dblCharacterESSModifier;

				// If Basic Bioware is selected, apply the Basic Bioware ESS Multiplier.
				if (strSelectCategory == "Basic")
					dblCharacterESSModifier -= (1 - _dblBasicBiowareESSModifier);

				if (nudESSDiscount.Visible)
				{
					double dblDiscountModifier = Convert.ToDouble(nudESSDiscount.Value, GlobalOptions.Instance.CultureInfo) * 0.01;
					dblCharacterESSModifier *= (1.0 - dblDiscountModifier);
				}

				dblCharacterESSModifier -= (1 - _dblESSMultiplier);

				// Genetech and Genetic Infusions are not subject to Bioware cost multipliers, so if we're looking at either, suppress the multiplier.
				if (strSelectCategory.StartsWith("Genetech") || strSelectCategory.StartsWith("Genetic Infusions") || strSelectCategory.StartsWith("Genemods"))
					dblCharacterESSModifier = 1;

				// Place the Genetech cost multiplier in a varaible that can be safely modified.
				double dblGenetechCostModifier = 1;
				// Genetech cost modifier only applies to Genetech.
				if (strSelectCategory.StartsWith("Genetech") || strSelectCategory.StartsWith("Genetic Infusions") || strSelectCategory.StartsWith("Genemods"))
					dblGenetechCostModifier = _dblGenetechCostModifier;

				// If Genetech: Transgenics is selected, apply the Transgenetics Bioware ESS Multiplier.
				if (strSelectCategory == "Genetech: Transgenics")
					dblGenetechCostModifier -= (1 - _dblTransgenicsBiowareCostModifier);

				// Retireve the information for the selected piece of Cyberware.
				XmlNode objXmlCyberware = _objXmlDocument.SelectSingleNode("/chummer/" + _strNode + "s/" + _strNode + "[name = \"" + lstCyberware.SelectedValue + "\"]");

				// Extract the Avil and Cost values from the Cyberware info since these may contain formulas and/or be based off of the Rating.
				// This is done using XPathExpression.
				XPathNavigator nav = _objXmlDocument.CreateNavigator();

				// Avail.
				// If avail contains "F" or "R", remove it from the string so we can use the expression.
				string strAvail = "";
				string strAvailExpr = objXmlCyberware["avail"].InnerText;
				XPathExpression xprAvail;
				if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
				{
					strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
					// Remove the trailing character if it is "F" or "R".
					strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
				}
				try
				{
					if (strAvailExpr.Contains("MinRating"))
					{
						XmlNode xmlMinRatingNode = objXmlCyberware["minrating"];
						if (xmlMinRatingNode != null)
						{
							switch (xmlMinRatingNode.InnerText)
							{
								case "MinimumAGI":
									if (_objVehicle != null)
									{
										strAvailExpr = strAvailExpr.Replace("MinRating", 
											_objVehicle.Pilot.ToString());
									}
									else
									{
										strAvailExpr = strAvailExpr.Replace("MinRating", 3.ToString());
									}
									break;
								case "MinimumSTR":
									if (_objVehicle != null)
									{
										strAvailExpr = strAvailExpr.Replace("MinRating",
											_objVehicle.TotalBody.ToString());
									}
									else
									{
										strAvailExpr = strAvailExpr.Replace("MinRating", 3.ToString());
									}
									break;
								default:
									strAvailExpr = strAvailExpr.Replace("MinRating", 3.ToString());
									break;
							}
						}
					}
					strAvailExpr = strAvailExpr.Replace("Rating", nudRating.Value.ToString());

					string strPrefix = "";
					if (strAvailExpr.StartsWith("+") || strAvailExpr.StartsWith("-"))
					{
						strPrefix = strAvailExpr.Substring(0, 1);
						strAvailExpr = strAvailExpr.Substring(1, strAvailExpr.Length-1);
					}
					xprAvail = nav.Compile(strAvailExpr);
					int intAvail = Convert.ToInt32(nav.Evaluate(xprAvail)) + _intAvailModifier;
					// Avail cannot go below 0.
					if (intAvail < 0)
						intAvail = 0;
					lblAvail.Text = strPrefix + intAvail.ToString() + strAvail;
				}
				catch
				{
					if (objXmlCyberware["avail"].InnerText.StartsWith("FixedValues"))
					{
						string[] strValues = objXmlCyberware["avail"].InnerText.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
                        strAvail = strValues[Convert.ToInt32(nudRating.Value) - 1];
                        if (strAvail.EndsWith("F") || strAvail.EndsWith("R"))
                        {
                            string strAvailSuffix = strAvail.Substring(strAvail.Length - 1, 1);
                            strAvail = strAvail.Substring(0, strAvail.Length - 1);
                            int intAvail = Convert.ToInt32(strAvail) + _intAvailModifier;
                            lblAvail.Text = intAvail.ToString() + strAvailSuffix;
                        }
                        else
                        {
                            int intAvail = Convert.ToInt32(strAvail) + _intAvailModifier;
                            lblAvail.Text = intAvail.ToString();
                        }
					}
					else
					{
						if (strAvailExpr.StartsWith("+") && strAvailExpr.Contains("Rating"))
						{
							strAvailExpr = objXmlCyberware["avail"].InnerText.Substring(1, objXmlCyberware["avail"].InnerText.Length - 1);
							
							if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
							{
								strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
								// Remove the trailing character if it is "F" or "R".
								strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
							}

							xprAvail = nav.Compile(strAvailExpr.Replace("Rating", nudRating.Value.ToString()));
							int intAvail = Convert.ToInt32(nav.Evaluate(xprAvail)) + _intAvailModifier;
							// Avail cannot go below 0;
							if (intAvail < 0)
								intAvail = 0;
							lblAvail.Text = "+" + intAvail.ToString() + strAvail;
						}
						else
							lblAvail.Text = objXmlCyberware["avail"].InnerText;
					}
				}
				lblAvail.Text = lblAvail.Text.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted")).Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));

				// Cost.
				// Check for a Variable Cost.
				int intItemCost = 0;
				if (objXmlCyberware["cost"].InnerText.StartsWith("Variable"))
				{
					int intMin = 0;
					int intMax = 0;
					string strCost = objXmlCyberware["cost"].InnerText.Replace("Variable(", string.Empty).Replace(")", string.Empty);
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
					try
					{
						if (objXmlCyberware["cost"].InnerText.Contains("MinRating"))
						{
							XmlNode xmlMinRatingNode = objXmlCyberware["minrating"];
							if (xmlMinRatingNode != null)
							{
								switch (xmlMinRatingNode.InnerText)
								{
									case "MinimumAGI":
										if (_objVehicle != null)
										{
											objXmlCyberware["cost"].InnerText = objXmlCyberware["cost"].InnerText.Replace("MinRating",
												_objVehicle.Pilot.ToString());
										}
										else
										{
											objXmlCyberware["cost"].InnerText = objXmlCyberware["cost"].InnerText.Replace("MinRating", 3.ToString());
										}
										break;
									case "MinimumSTR":
										if (_objVehicle != null)
										{
											objXmlCyberware["cost"].InnerText = objXmlCyberware["cost"].InnerText.Replace("MinRating",
												_objVehicle.TotalBody.ToString());
										}
										else
										{
											objXmlCyberware["cost"].InnerText = objXmlCyberware["cost"].InnerText.Replace("MinRating", 3.ToString());
										}
										break;
								}
							}
						}
						XPathExpression xprCost = nav.Compile(objXmlCyberware["cost"].InnerText.Replace("Rating", nudRating.Value.ToString()));
						double dblCost = (Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.Instance.CultureInfo)*_dblCostMultiplier*
						           dblGenetechCostModifier);
						dblCost *= 1 + (Convert.ToDouble(nudMarkup.Value, GlobalOptions.Instance.CultureInfo) / 100.0);
						intItemCost = Convert.ToInt32(dblCost);
						lblCost.Text = String.Format("{0:###,###,##0¥}", intItemCost);
						
					}
					catch
					{
						if (objXmlCyberware["cost"].InnerText.StartsWith("FixedValues"))
						{
							string[] strValues = objXmlCyberware["cost"].InnerText.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
							int intCost = Convert.ToInt32((Convert.ToDouble(strValues[Convert.ToInt32(nudRating.Value) - 1], GlobalOptions.Instance.CultureInfo) * _dblCostMultiplier * dblGenetechCostModifier));
							lblCost.Text = String.Format("{0:###,###,##0¥}", intCost);
							intItemCost = intCost;
						}
						else
						{
							try
							{
								lblCost.Text = String.Format("{0:###,###,##0¥}", Convert.ToInt32(Convert.ToDouble(objXmlCyberware["cost"].InnerText, GlobalOptions.Instance.CultureInfo) * dblGenetechCostModifier));
								intItemCost = Convert.ToInt32(Convert.ToDouble(objXmlCyberware["cost"].InnerText, GlobalOptions.Instance.CultureInfo) * dblGenetechCostModifier);
							}
							catch
							{
								lblCost.Text = String.Format("{0:###,###,##0¥}", objXmlCyberware["cost"].InnerText);
								try
								{
									intItemCost = Convert.ToInt32(objXmlCyberware["cost"].InnerText);
								}
								catch
								{
								}
							}
						}
					}
				}

				if (chkBlackMarketDiscount.Checked)
				{
					double dblDiscount = 0;
					dblDiscount = intItemCost - (intItemCost * 0.90);
					intItemCost -= Convert.ToInt32(dblDiscount);
					lblCost.Text = String.Format("{0:###,###,##0¥}", intItemCost);
				}

				if (chkFree.Checked)
				{
					lblCost.Text = String.Format("{0:###,###,##0¥}", 0);
					intItemCost = 0;
				}

				// Test required to find the item.
				lblTest.Text = _objCharacter.AvailTest(intItemCost, lblAvail.Text);

				// Essence.
				double dblESS = 0.0;
				if (objXmlCyberware["ess"].InnerText.StartsWith("FixedValues"))
				{
					string[] strValues = objXmlCyberware["ess"].InnerText.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
					decimal decESS = Convert.ToDecimal(strValues[Convert.ToInt32(nudRating.Value) - 1], GlobalOptions.Instance.CultureInfo);
					dblESS = Math.Round(Convert.ToDouble(decESS, GlobalOptions.Instance.CultureInfo) * dblCharacterESSModifier, _objCharacter.Options.EssenceDecimals, MidpointRounding.AwayFromZero);
				}
				else
				{
					XPathExpression xprEssence = nav.Compile(objXmlCyberware["ess"].InnerText.Replace("Rating", nudRating.Value.ToString()));
					dblESS = Math.Round(Convert.ToDouble(nav.Evaluate(xprEssence), GlobalOptions.Instance.CultureInfo) * dblCharacterESSModifier, _objCharacter.Options.EssenceDecimals, MidpointRounding.AwayFromZero);
				}
				// Check if the character has Sensitive System.
				if (_objMode == Mode.Cyberware)
				{
					foreach (Improvement objImprovement in _objCharacter.Improvements)
					{
						if (objImprovement.ImproveType == Improvement.ImprovementType.SensitiveSystem && objImprovement.Enabled)
							dblESS *= 2.0;
					}
				}
				lblEssence.Text = dblESS.ToString();

				// Capacity.
				// XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
				bool blnSquareBrackets = objXmlCyberware["capacity"].InnerText.Contains('[');
				string strCapacity = objXmlCyberware["capacity"].InnerText;
				XPathExpression xprCapacity;

				if (objXmlCyberware["capacity"].InnerText.Contains("/["))
				{
					int intPos = objXmlCyberware["capacity"].InnerText.IndexOf("/[");
					string strFirstHalf = objXmlCyberware["capacity"].InnerText.Substring(0, intPos);
					string strSecondHalf = objXmlCyberware["capacity"].InnerText.Substring(intPos + 1, objXmlCyberware["capacity"].InnerText.Length - intPos - 1);

					try
					{
						blnSquareBrackets = strFirstHalf.Contains('[');
						strCapacity = strFirstHalf;
						if (blnSquareBrackets)
							strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
					}
					catch
					{
					}
					xprCapacity = nav.Compile(strCapacity.Replace("Rating", nudRating.Value.ToString()));

					try
					{
						if (objXmlCyberware["capacity"].InnerText == "[*]")
							lblCapacity.Text = "*";
						else
						{
							if (objXmlCyberware["capacity"].InnerText.StartsWith("FixedValues"))
							{
								string[] strValues = objXmlCyberware["capacity"].InnerText.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
								lblCapacity.Text = strValues[Convert.ToInt32(nudRating.Value) - 1];
							}
							else
								lblCapacity.Text = nav.Evaluate(xprCapacity).ToString();
						}
						if (blnSquareBrackets)
							lblCapacity.Text = "[" + lblCapacity.Text + "]";
					}
					catch
					{
						lblCapacity.Text = "0";
					}

					if (strSecondHalf.Contains("Rating"))
					{
						strSecondHalf = strSecondHalf.Replace("[", string.Empty).Replace("]", string.Empty);
						xprCapacity = nav.Compile(strSecondHalf.Replace("Rating", nudRating.Value.ToString()));
						strSecondHalf = "[" + nav.Evaluate(xprCapacity).ToString() + "]";
					}

					lblCapacity.Text += "/" + strSecondHalf;
				}
				else
				{
					if (blnSquareBrackets)
						strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
					xprCapacity = nav.Compile(strCapacity.Replace("Rating", nudRating.Value.ToString()));

					if (objXmlCyberware["capacity"].InnerText == "[*]")
						lblCapacity.Text = "*";
					else
					{
						if (objXmlCyberware["capacity"].InnerText.StartsWith("FixedValues"))
						{
							string[] strValues = objXmlCyberware["capacity"].InnerText.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
							lblCapacity.Text = strValues[Convert.ToInt32(nudRating.Value) - 1];
						}
						else
							lblCapacity.Text = nav.Evaluate(xprCapacity).ToString();
					}
					if (blnSquareBrackets)
						lblCapacity.Text = "[" + lblCapacity.Text + "]";
				}
			}
        }
		
		/// <summary>
		/// Lock the Grade so it cannot be changed.
		/// </summary>
		public void LockGrade()
		{
			cboGrade.Enabled = false;
			_blnLockGrade = true;
		}

		/// <summary>
		/// Accept the selected item and close the form.
		/// </summary>
		private void AcceptForm()
		{
			if (lstCyberware.Text != "")
			{
				XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/" + _strNode + "s/" + _strNode + "[name = \"" + lstCyberware.SelectedValue + "\"]");
				_strSelectCategory = objNode["category"].InnerText;
				_strSelectedCyberware = objNode["name"].InnerText;
			}

			if (_objMode == Mode.Bioware)
			{
				if (cboCategory.SelectedValue.ToString().StartsWith("Genetech:") ||
				    cboCategory.SelectedValue.ToString() == "Symbionts" ||
				    cboCategory.SelectedValue.ToString() == "Genemods")
				{
					_objSelectedGrade = GlobalOptions.BiowareGrades.GetGrade("Standard");
				}
				else
				{
					_objSelectedGrade = GlobalOptions.BiowareGrades.GetGrade(cboGrade.SelectedValue.ToString());
				}
			}
			else
			{
				_objSelectedGrade = GlobalOptions.CyberwareGrades.GetGrade(cboGrade.SelectedValue.ToString());
			}

			_strSelectedGrade = _objSelectedGrade.ToString();
			_intSelectedRating = Convert.ToInt32(nudRating.Value);
			_blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;

			if (nudESSDiscount.Visible)
				_intSelectedESSDiscount = Convert.ToInt32(nudESSDiscount.Value);

			XmlNode objCyberwareNode = _objXmlDocument.SelectSingleNode("/chummer/" + _strNode + "s/" + _strNode + "[name = \"" + lstCyberware.SelectedValue + "\"]");
			if (!RequirementMet(objCyberwareNode, true))
				return;
			this.DialogResult = DialogResult.OK;
		}

		/// <summary>
		/// Check if the Cyberware's requirements/restrictions are being met.
		/// </summary>
		/// <param name="objXmlCyberware">XmlNode of the Cyberware.</param>
		/// <param name="blnShowMessage">Whether or not a message should be shown if the requirements are not met.</param>
		private bool RequirementMet(XmlNode objXmlCyberware, bool blnShowMessage)
		{
			// Ignore the rules.
			if (_objCharacter.IgnoreRules)
				return true;

			if (objXmlCyberware.InnerXml.Contains("forbidden"))
			{
				bool blnRequirementForbidden = false;
				string strForbidden = "";

				// Loop through the oneof requirements.
				XmlNodeList objXmlForbiddenList = objXmlCyberware.SelectNodes("forbidden/oneof");
				foreach (XmlNode objXmlOneOf in objXmlForbiddenList)
				{
					XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes;

					foreach (XmlNode objXmlForbidden in objXmlOneOfList)
					{
						switch (objXmlForbidden.Name)
						{
							case "cyberware":
								// Run through all of the Cyberwares the character has and see if the current forbidden item exists.
								// If so, turn on the RequirementForbidden flag so it cannot be selected.
								foreach (Cyberware objCharacterCyberware in _objCharacter.Cyberware.Where(objCharacterCyberware => objCharacterCyberware.Name == objXmlForbidden.InnerText))
								{
									blnRequirementForbidden = true;
									strForbidden += "\n\t" + objCharacterCyberware.DisplayNameShort;
								}
								break;
						}
					}
				}

				// The character is not allowed to take the Cyberware, so display a message and uncheck the item.
				if (blnRequirementForbidden)
				{
					if (blnShowMessage)
						MessageBox.Show(LanguageManager.Instance.GetString("Message_SelectCyberware_CyberwareRestriction") + strForbidden, LanguageManager.Instance.GetString("MessageTitle_SelectQuality_QualityRestriction"), MessageBoxButtons.OK, MessageBoxIcon.Information);
					return false;
				}
			}

			if (objXmlCyberware.InnerXml.Contains("required"))
			{
				string strRequirement = "";
				bool blnRequirementMet = true;

				// Loop through the oneof requirements.
				XmlNodeList objXmlRequiredList = objXmlCyberware.SelectNodes("required/oneof");
				foreach (XmlNode objXmlOneOf in objXmlRequiredList)
				{
					bool blnOneOfMet = false;
					string strThisRequirement = "\n" + LanguageManager.Instance.GetString("Message_SelectQuality_OneOf");
					XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes;
					foreach (XmlNode objXmlRequired in objXmlOneOfList)
					{
						switch (objXmlRequired.Name)
						{
							case "quality":
								XmlDocument _objQualityDocument = XmlManager.Instance.Load("qualities.xml");
								// Run through all of the Qualities the character has and see if the current required item exists.
								// If so, turn on the RequirementMet flag so it can be selected.
								foreach (Quality objCharacterQuality in _objCharacter.Qualities)
								{
									if (objCharacterQuality.Name == objXmlRequired.InnerText)
										blnOneOfMet = true;
								}

								if (!blnOneOfMet)
								{
									XmlNode objNode = _objQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlRequired.InnerText + "\"]");
									if (objNode["translate"] != null)
										strThisRequirement += "\n\t" + objNode["translate"].InnerText;
									else
										strThisRequirement += "\n\t" + objXmlRequired.InnerText;
								}
								break;
							case "metatype":
								XmlDocument _objMetatypeDocument = XmlManager.Instance.Load("metatypes.xml");
								// Check the Metatype requirement.
								if (objXmlRequired.InnerText == _objCharacter.Metatype)
									blnOneOfMet = true;
								else
								{
									XmlNode objNode = _objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlRequired.InnerText + "\"]");
									if (objNode["translate"] != null)
										strThisRequirement += "\n\t" + objNode["translate"].InnerText;
									else
										strThisRequirement += "\n\t" + objXmlRequired.InnerText;
								}
								break;
							case "metatypecategory":
								// Check the Metatype Category requirement.
								if (objXmlRequired.InnerText == _objCharacter.MetatypeCategory)
									blnOneOfMet = true;
								else
								{
									XmlNode objNode = this._objMetatypeDocument.SelectSingleNode("/chummer/categories/category[. = \"" + objXmlRequired.InnerText + "\"]");
									if (objNode.Attributes["translate"] != null)
										strThisRequirement += "\n\t" + objNode.Attributes["translate"].InnerText;
									else
										strThisRequirement = "\n\t" + objXmlRequired.InnerText;
								}
								break;
							case "metavariant":
								// Check the Metavariant requirement.
								if (objXmlRequired.InnerText == _objCharacter.Metavariant)
									blnOneOfMet = true;
								else
								{
									XmlNode objNode = this._objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype/metavariants/metavariant[name = \"" + objXmlRequired.InnerText + "\"]");
									if (objNode["translate"] != null)
										strThisRequirement += "\n\t" + objNode["translate"].InnerText;
									else
										strThisRequirement += "\n\t" + objXmlRequired.InnerText;
								}
								break;
							case "power":
								// Run through all of the Powers the character has and see if the current required item exists.
								// If so, turn on the RequirementMet flag so it can be selected.
								if (_objCharacter.AdeptEnabled && _objCharacter.Powers != null)
								{
									foreach (Power objCharacterPower in _objCharacter.Powers)
									{
										//Check that the power matches the name and doesn't come from a bonus source like a focus. There's probably an edge case that this will break.
										if (objXmlRequired.InnerText == objCharacterPower.Name && objCharacterPower.BonusSource.Length == 0)
										{
											blnOneOfMet = true;
										}
									}

									if (!blnOneOfMet)
									{
										strThisRequirement += "\n\t" + objXmlRequired.InnerText;
									}
								}
								break;
							case "inherited":
								strThisRequirement += "\n\t" + LanguageManager.Instance.GetString("Message_SelectQuality_Inherit");
								break;
							case "careerkarma":
								// Check Career Karma requirement.
								if (_objCharacter.CareerKarma >= Convert.ToInt32(objXmlRequired.InnerText))
									blnOneOfMet = true;
								else
									strThisRequirement = "\n\t" + LanguageManager.Instance.GetString("Message_SelectQuality_RequireKarma").Replace("{0}", objXmlRequired.InnerText);
								break;
							case "ess":
								// Check Essence requirement.
								if (objXmlRequired.InnerText.StartsWith("-"))
								{
									// Essence must be less than the value.
									if (_objCharacter.Essence < Convert.ToDecimal(objXmlRequired.InnerText.Replace("-", string.Empty), GlobalOptions.Instance.CultureInfo))
										blnOneOfMet = true;
								}
								else
								{
									// Essence must be equal to or greater than the value.
									if (_objCharacter.Essence >= Convert.ToDecimal(objXmlRequired.InnerText, GlobalOptions.Instance.CultureInfo))
										blnOneOfMet = true;
								}
								break;
							case "skill":
								// Check if the character has the required Skill.
								foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
								{
									if (objSkill.Name == objXmlRequired["name"].InnerText)
									{
										if (objSkill.Rating >= Convert.ToInt32(objXmlRequired["val"].InnerText))
										{
											blnOneOfMet = true;
											break;
										}
									}
								}
								break;
							case "attribute":
								// Check to see if an Attribute meets a requirement.
								CharacterAttrib objAttribute = _objCharacter.GetAttribute(objXmlRequired["name"].InnerText);

								if (objXmlRequired["total"] != null)
								{
									// Make sure the Attribute's total value meets the requirement.
									if (objAttribute.TotalValue >= Convert.ToInt32(objXmlRequired["total"].InnerText))
										blnOneOfMet = true;
								}
								break;
							case "attributetotal":
								// Check if the character's Attributes add up to a particular total.
								string strAttributes = objXmlRequired["attributes"].InnerText;
								strAttributes = strAttributes.Replace("BOD", _objCharacter.GetAttribute("BOD").Value.ToString());
								strAttributes = strAttributes.Replace("AGI", _objCharacter.GetAttribute("AGI").Value.ToString());
								strAttributes = strAttributes.Replace("REA", _objCharacter.GetAttribute("REA").Value.ToString());
								strAttributes = strAttributes.Replace("STR", _objCharacter.GetAttribute("STR").Value.ToString());
								strAttributes = strAttributes.Replace("CHA", _objCharacter.GetAttribute("CHA").Value.ToString());
								strAttributes = strAttributes.Replace("INT", _objCharacter.GetAttribute("INT").Value.ToString());
								strAttributes = strAttributes.Replace("INT", _objCharacter.GetAttribute("LOG").Value.ToString());
								strAttributes = strAttributes.Replace("WIL", _objCharacter.GetAttribute("WIL").Value.ToString());
								strAttributes = strAttributes.Replace("MAG", _objCharacter.GetAttribute("MAG").Value.ToString());
								strAttributes = strAttributes.Replace("RES", _objCharacter.GetAttribute("RES").Value.ToString());
								strAttributes = strAttributes.Replace("EDG", _objCharacter.GetAttribute("EDG").Value.ToString());

								XmlDocument objXmlDocument = new XmlDocument();
								XPathNavigator nav = objXmlDocument.CreateNavigator();
								XPathExpression xprAttributes = nav.Compile(strAttributes);
								if (Convert.ToInt32(nav.Evaluate(xprAttributes)) >= Convert.ToInt32(objXmlRequired["val"].InnerText))
									blnOneOfMet = true;
								break;
							case "skillgrouptotal":
							{
								// Check if the total combined Ratings of Skill Groups adds up to a particular total.
								int intTotal = 0;
								string[] strGroups = objXmlRequired["skillgroups"].InnerText.Split('+');
								for (int i = 0; i <= strGroups.Length - 1; i++)
								{
									foreach (SkillGroup objGroup in _objCharacter.SkillsSection.SkillGroups)
									{
										if (objGroup.Name == strGroups[i])
										{
											intTotal += objGroup.Rating;
											break;
										}
									}
								}

								if (intTotal >= Convert.ToInt32(objXmlRequired["val"].InnerText))
									blnOneOfMet = true;
							}
								break;
							case "cyberwares":
							{
								// Check to see if the character has a number of the required Cyberware/Bioware items.
								int intTotal = 0;

								// Check Cyberware.
								foreach (XmlNode objXmlNode in objXmlRequired.SelectNodes("cyberware"))
								{
									foreach (Cyberware objCyberware in _objCharacter.Cyberware)
									{
										bool blnFound = false;
										if (objCyberware.Name == objXmlNode.InnerText)
										{
											if (objXmlNode.Attributes["select"] == null)
											{
												intTotal++;
												blnFound = true;
												break;
											}
											else if (objXmlNode.Attributes["select"].InnerText == objCyberware.Location)
											{
												intTotal++;
												blnFound = true;
												break;
											}
										}
										if (!blnFound)
										{
											strThisRequirement += "\n\t" + objXmlNode.InnerText;
										}
									}
								}

								// Check Bioware.
								foreach (XmlNode objXmlNode in objXmlRequired.SelectNodes("bioware"))
								{
									bool blnFound = false;
									foreach (Cyberware objCyberware in _objCharacter.Cyberware)
									{
										if (objCyberware.Name == objXmlNode.InnerText)
										{
											intTotal++;
											blnFound = true;
											break;
										}
									}
									if (!blnFound)
									{
										strThisRequirement += "\n\t" + objXmlNode.InnerText;
									}
								}

								// Check Cyberware name that contain a straing.
								foreach (XmlNode objXmlNode in objXmlRequired.SelectNodes("cyberwarecontains"))
								{
									foreach (Cyberware objCyberware in _objCharacter.Cyberware)
									{
										if (objCyberware.Name.Contains(objXmlNode.InnerText))
										{
											if (objXmlCyberware.Attributes["select"] == null)
											{
												intTotal++;
												break;
											}
											else if (objXmlCyberware.Attributes["select"].InnerText == objCyberware.Location)
											{
												intTotal++;
												break;
											}
										}
									}
								}

								// Check Bioware name that contain a straing.
								foreach (XmlNode objXmlNode in objXmlRequired.SelectNodes("biowarecontains"))
								{
									foreach (Cyberware objCyberware in _objCharacter.Cyberware)
									{
										if (objCyberware.Name.Contains(objXmlNode.InnerText))
										{
											if (objXmlNode.Attributes["select"] == null)
											{
												intTotal++;
												break;
											}
											else if (objXmlNode.Attributes["select"].InnerText == objCyberware.Location)
											{
												intTotal++;
												break;
											}
										}
									}
								}

								// Check for Cyberware Plugins.
								foreach (XmlNode objXmlNode in objXmlRequired.SelectNodes("cyberwareplugin"))
								{
									foreach (Cyberware objCyberware in _objCharacter.Cyberware)
									{
										foreach (Cyberware objPlugin in objCyberware.Children)
										{
											if (objPlugin.Name == objXmlNode.InnerText)
											{
												intTotal++;
												break;
											}
										}
									}
								}

								// Check for Cyberware Categories.
								foreach (XmlNode objXmlNode in objXmlRequired.SelectNodes("cyberwarecategory"))
								{
									foreach (Cyberware objCyberware in _objCharacter.Cyberware)
									{
										if (objCyberware.Category == objXmlNode.InnerText)
											intTotal++;
									}
								}

								if (intTotal >= Convert.ToInt32(objXmlRequired["count"].InnerText))
									blnOneOfMet = true;
							}
								break;
							case "streetcredvsnotoriety":
								// Street Cred must be higher than Notoriety.
								if (_objCharacter.StreetCred >= _objCharacter.Notoriety)
									blnOneOfMet = true;
								break;
							case "damageresistance":
								// Damage Resistance must be a particular value.
								ImprovementManager _objImprovementManager = new ImprovementManager(_objCharacter);
								if (_objCharacter.BOD.TotalValue + _objImprovementManager.ValueOf(Improvement.ImprovementType.DamageResistance) >= Convert.ToInt32(objXmlRequired.InnerText))
									blnOneOfMet = true;
								break;
						}
					}

					// Update the flag for requirements met.
					blnRequirementMet = blnRequirementMet && blnOneOfMet;
					strRequirement += strThisRequirement;
				}

				// The character has not met the requirements, so display a message and uncheck the item.
				if (!blnRequirementMet)
				{
					string strMessage = LanguageManager.Instance.GetString("Message_SelectQuality_QualityRequirement");
					strMessage += strRequirement;

					if (blnShowMessage)
						MessageBox.Show(strMessage, LanguageManager.Instance.GetString("MessageTitle_SelectQuality_QualityRequirement"), MessageBoxButtons.OK, MessageBoxIcon.Information);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Populate the list of Cyberware Grades.
		/// </summary>
		/// <param name="blnIgnoreSecondHand">Whether or not Secon-Hand Grades should be added to the list.</param>
		private void PopulateGrades(bool blnIgnoreSecondHand = false)
		{
			GradeList objGradeList;
			if (_objMode == Mode.Bioware)
				objGradeList = GlobalOptions.BiowareGrades;
			else
				objGradeList = GlobalOptions.CyberwareGrades;

			_lstGrade.Clear();
			foreach (Grade objGrade in objGradeList)
			{
				bool blnAddItem = true;

				ListItem objItem = new ListItem();
				objItem.Value = objGrade.Name;
				objItem.Name = objGrade.DisplayName;

                if (objGrade.Burnout && _objCharacter.BurnoutEnabled)
                    blnAddItem = true;
                else if (objGrade.Burnout)
                    blnAddItem = false;

                if (objGrade.DisplayName == "Standard" && _objCharacter.BurnoutEnabled)
                    blnAddItem = false;
				if (blnIgnoreSecondHand && objGrade.SecondHand)
					blnAddItem = false;
				if (!_objCharacter.AdapsinEnabled && objGrade.Adapsin)
					blnAddItem = false;

				if (blnAddItem)
					_lstGrade.Add(objItem);
			}
			cboGrade.DataSource = null;
			cboGrade.ValueMember = "Value";
			cboGrade.DisplayMember = "Name";
			cboGrade.DataSource = _lstGrade;
		}

		private void MoveControls()
		{
			int intWidth = Math.Max(lblRatingLabel.Width, lblEssenceLabel.Width);
			intWidth = Math.Max(intWidth, lblCapacityLabel.Width);
			intWidth = Math.Max(intWidth, lblAvailLabel.Width);
			intWidth = Math.Max(intWidth, lblCostLabel.Width);

			nudRating.Left = lblRatingLabel.Left + intWidth + 6;
			lblEssence.Left = lblEssenceLabel.Left + intWidth + 6;
			lblCapacity.Left = lblCapacityLabel.Left + intWidth + 6;
			lblAvail.Left = lblAvailLabel.Left + intWidth + 6;
			lblCost.Left = lblCostLabel.Left + intWidth + 6;

			lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;
			lblTest.Left = lblTestLabel.Left + lblTestLabel.Width + 6;
            nudESSDiscount.Left = lblESSDiscountLabel.Left + lblESSDiscountLabel.Width + 6;
            lblESSDiscountPercentLabel.Left = nudESSDiscount.Left + nudESSDiscount.Width;

			lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
		}
		#endregion

	}
}