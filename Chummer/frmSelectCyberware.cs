using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectCyberware : Form
    {
		private string _strSelectedCyberware = "";
		private Grade _objSelectedGrade;
		private int _intSelectedRating = 0;
		private readonly Character _objCharacter;
		private int _intSelectedESSDiscount = 0;

        private double _dblCostMultiplier = 1.0;
        private double _dblESSMultiplier = 1.0;
		private int _intAvailModifier = 0;
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
		private static string _strSelectCategory = "";
		private static string _strSelectedGrade = "";

		private XmlDocument _objXmlDocument = new XmlDocument();

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
					if (cboGrade.SelectedIndex == -1)
						cboGrade.SelectedIndex = 0;
				}
			}

            // Update the list of Cyberware based on the selected Category.
            XmlNodeList objXmlCyberwareList;
			List<ListItem> lstCyberwares = new List<ListItem>();

			if (cboCategory.SelectedValue.ToString().StartsWith("Genetech:") || cboCategory.SelectedValue.ToString() == "Symbiont" || cboCategory.SelectedValue.ToString() == "Genetic Infusions" || _blnLockGrade)
				cboGrade.Enabled = false;
			else
				cboGrade.Enabled = true;

			if (cboCategory.SelectedValue.ToString().StartsWith("Genetech:") || cboCategory.SelectedValue.ToString() == "Symbiont" || cboCategory.SelectedValue.ToString() == "Genetic Infusions")
				cboGrade.SelectedValue = "Standard";

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
                nudRating.Maximum = Convert.ToInt32(objXmlCyberware["rating"].InnerText);
				if (objXmlCyberware["minrating"] != null)
					nudRating.Minimum = Convert.ToInt32(objXmlCyberware["minrating"].InnerText);
				else
					nudRating.Minimum = 1;
            }
            else
            {
                nudRating.Minimum = 0;
                nudRating.Value = 0;
                nudRating.Enabled = false;
            }

			if (objXmlCyberware["forcegrade"] != null)
			{
				// Force the Cyberware to be a particular Grade.
				cboGrade.SelectedValue = objXmlCyberware["forcegrade"].InnerText;
				cboGrade.Enabled = false;
			}
			else
				cboGrade.Enabled = true;

			string strBook = _objCharacter.Options.LanguageBookShort(objXmlCyberware["source"].InnerText);
			string strPage = objXmlCyberware["page"].InnerText;
			if (objXmlCyberware["altpage"] != null)
				strPage = objXmlCyberware["altpage"].InnerText;
			lblSource.Text = strBook + " " + strPage;

			tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlCyberware["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);

            UpdateCyberwareInfo();
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
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

				// Genetech and Genetic Infusions are not subject to Bioware cost multipliers, so if we're looking at either, suppress the multiplier.
				if (strSelectCategory.StartsWith("Genetech") || strSelectCategory.StartsWith("Genetic Infusions"))
					dblCharacterESSModifier = 1;

				if (nudESSDiscount.Visible)
				{
					double dblDiscountModifier = Convert.ToDouble(nudESSDiscount.Value, GlobalOptions.Instance.CultureInfo) * 0.01;
					dblCharacterESSModifier -= dblDiscountModifier;
				}

				dblCharacterESSModifier -= (1 - _dblESSMultiplier);

				// Place the Genetech cost multiplier in a varaible that can be safely modified.
				double dblGenetechCostModifier = 1;
				// Genetech cost modifier only applies to Genetech.
				if (strSelectCategory.StartsWith("Genetech") || strSelectCategory.StartsWith("Genetic Infusions"))
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
					xprAvail = nav.Compile(strAvailExpr.Replace("Rating", nudRating.Value.ToString()));
					int intAvail = Convert.ToInt32(nav.Evaluate(xprAvail)) + _intAvailModifier;
					// Avail cannot go below 0.
					if (intAvail < 0)
						intAvail = 0;
					lblAvail.Text = intAvail.ToString() + strAvail;
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
						XPathExpression xprCost = nav.Compile(objXmlCyberware["cost"].InnerText.Replace("Rating", nudRating.Value.ToString()));
						lblCost.Text = String.Format("{0:###,###,##0¥}", Convert.ToInt32((Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.Instance.CultureInfo) * _dblCostMultiplier * dblGenetechCostModifier)));
						intItemCost = Convert.ToInt32((Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.Instance.CultureInfo) * _dblCostMultiplier * dblGenetechCostModifier));
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

				if (chkFree.Checked)
				{
					lblCost.Text = String.Format("{0:###,###,##0¥}", 0);
					intItemCost = 0;
				}

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
				_objSelectedGrade = GlobalOptions.BiowareGrades.GetGrade(cboGrade.SelectedValue.ToString());
			else
				_objSelectedGrade = GlobalOptions.CyberwareGrades.GetGrade(cboGrade.SelectedValue.ToString());

			_strSelectedGrade = cboGrade.SelectedValue.ToString();
			_intSelectedRating = Convert.ToInt32(nudRating.Value);

			if (nudESSDiscount.Visible)
				_intSelectedESSDiscount = Convert.ToInt32(nudESSDiscount.Value);

			this.DialogResult = DialogResult.OK;
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
			nudESSDiscount.Left = lblESSDiscountLabel.Left = lblESSDiscountLabel.Width + 6;
			lblESSDiscountPercentLabel.Left = nudESSDiscount.Left + nudESSDiscount.Width;

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