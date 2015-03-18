using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public partial class frmSelectAdvancedLifestyle : Form
	{
		private bool _blnAddAgain = false;
		private Lifestyle _objLifestyle;
		private Lifestyle _objSourceLifestyle;
		private readonly Character _objCharacter;
		private LifestyleType _objType = LifestyleType.Advanced;

		private XmlDocument _objXmlDocument = new XmlDocument();

		private bool _blnSkipRefresh = false;

		#region Control Events
		public frmSelectAdvancedLifestyle(Lifestyle objLifestyle, Character objCharacter)
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			_objCharacter = objCharacter;
			_objLifestyle = objLifestyle;
			MoveControls();
		}

		private void frmSelectAdvancedLifestyle_Load(object sender, EventArgs e)
		{
			_blnSkipRefresh = true;

			foreach (Label objLabel in this.Controls.OfType<Label>())
			{
				if (objLabel.Text.StartsWith("["))
					objLabel.Text = "";
			}

			// Load the Lifestyles information.
			_objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");

			// Populate the Advanced Lifestyle ComboBoxes.
            // Lifestyles.
            List<ListItem> lstLifestyles = new List<ListItem>();
            foreach (XmlNode objXmlLifestyle in _objXmlDocument.SelectNodes("/chummer/lifestyles/lifestyle"))
            {
                bool blnAdd = true;
                if (_objType != LifestyleType.Advanced && objXmlLifestyle["slp"] != null)
                {
                    if (objXmlLifestyle["slp"].InnerText == "remove")
                        blnAdd = false;
                }

                if (blnAdd)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlLifestyle["name"].InnerText;
                    if (objXmlLifestyle["translate"] != null)
                        objItem.Name = objXmlLifestyle["translate"].InnerText;
                    else
                        objItem.Name = objXmlLifestyle["name"].InnerText;
                    lstLifestyles.Add(objItem);
                }
            }
            cboBaseLifestyle.ValueMember = "Value";
            cboBaseLifestyle.DisplayMember = "Name";
            cboBaseLifestyle.DataSource = lstLifestyles;

            cboBaseLifestyle.SelectedValue = _objLifestyle.BaseLifestyle;

            if (cboBaseLifestyle.SelectedIndex == -1)
            { cboBaseLifestyle.SelectedIndex = 0; }
/*
			// Fill the Positive Qualities list.
            foreach (XmlNode objXmlQuality in _objXmlDocument.SelectNodes("/chummer/qualities/quality[category = \"Positive\"]"))
			{
				TreeNode nodQuality = new TreeNode();
				//nodQuality.Tag = objXmlQuality["name"].InnerText + " [" + objXmlQuality["lp"].InnerText + "LP]";
				if (objXmlQuality["translate"] != null)
					nodQuality.Text = objXmlQuality["translate"].InnerText + " [" + objXmlQuality["lp"].InnerText + "LP]";
				else
					nodQuality.Text = objXmlQuality["name"].InnerText + " [" + objXmlQuality["lp"].InnerText + "LP]";
				trePositiveQualities.Nodes.Add(nodQuality);
			}

			// Fill the Negative Qualities list.
			foreach (XmlNode objXmlQuality in _objXmlDocument.SelectNodes("/chummer/qualities/quality[category = \"Negative\")]"))
			{
				TreeNode nodQuality = new TreeNode();
				nodQuality.Tag = objXmlQuality["name"].InnerText + " [" + objXmlQuality["lp"].InnerText + "LP]";
				if (objXmlQuality["translate"] != null)
					nodQuality.Text = objXmlQuality["translate"].InnerText + " [" + objXmlQuality["lp"].InnerText + "LP]";
				else
					nodQuality.Text = objXmlQuality["name"].InnerText + " [" + objXmlQuality["lp"].InnerText + "LP]";
				treNegativeQualities.Nodes.Add(nodQuality);
			}

            // Fill the Entertainments list.
            foreach (XmlNode objXmlQuality in _objXmlDocument.SelectNodes("/chummer/entertainments/entertainment"))
            {
                TreeNode nodQuality = new TreeNode();
                nodQuality.Tag = objXmlQuality["name"].InnerText + " [" + objXmlQuality["lp"].InnerText + "LP]";
                if (objXmlQuality["translate"] != null)
                    nodQuality.Text = objXmlQuality["translate"].InnerText + " [" + objXmlQuality["lp"].InnerText + "LP]";
                else
                    nodQuality.Text = objXmlQuality["name"].InnerText + " [" + objXmlQuality["lp"].InnerText + "LP]";
                treEntertainments.Nodes.Add(nodQuality);
            }

			SortTree(trePositiveQualities);
			SortTree(treNegativeQualities);
            SortTree(treEntertainments);
*/
			if (_objSourceLifestyle != null)
			{
				txtLifestyleName.Text = _objSourceLifestyle.Name;
				nudRoommates.Value = _objSourceLifestyle.Roommates;
				nudPercentage.Value = _objSourceLifestyle.Percentage;
                lblSource.Text = _objSourceLifestyle.Source;
				foreach (string strQuality in _objSourceLifestyle.Qualities)
				{
					foreach (TreeNode objNode in trePositiveQualities.Nodes)
					{
						if (objNode.Tag.ToString() == strQuality)
						{
							objNode.Checked = true;
							break;
						}
					}
					foreach (TreeNode objNode in treNegativeQualities.Nodes)
					{
						if (objNode.Tag.ToString() == strQuality)
						{
							objNode.Checked = true;
							break;
						}
					}
				}
			}

			// Safehouses have a cost per week instead of cost per month.
			if (_objType == LifestyleType.Safehouse)
				lblCostLabel.Text = LanguageManager.Instance.GetString("Label_SelectLifestyle_CostPerWeek");

			_blnSkipRefresh = false;
			CalculateValues();
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			if (txtLifestyleName.Text == "")
			{
				MessageBox.Show(LanguageManager.Instance.GetString("Message_SelectAdvancedLifestyle_LifestyleName"), LanguageManager.Instance.GetString("MessageTitle_SelectAdvancedLifestyle_LifestyleName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			AcceptForm();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void cmdOKAdd_Click(object sender, EventArgs e)
		{
			_blnAddAgain = true;
			cmdOK_Click(sender, e);
		}

		private void trePositiveQualities_AfterCheck(object sender, TreeViewEventArgs e)
		{
			CalculateValues();
		}

		private void treNegativeQualities_AfterCheck(object sender, TreeViewEventArgs e)
		{
			CalculateValues();
		}

		private void cboBaseLifestyle_SelectedIndexChanged(object sender, EventArgs e)
		{
            CalculateValues();      
		}

		private void nudPercentage_ValueChanged(object sender, EventArgs e)
		{
			CalculateValues();
		}

		private void nudRoommates_ValueChanged(object sender, EventArgs e)
		{
			CalculateValues();
		}

		private void trePositiveQualities_AfterSelect(object sender, TreeViewEventArgs e)
		{
			string strQualityName = trePositiveQualities.SelectedNode.Tag.ToString().Substring(0, trePositiveQualities.SelectedNode.Tag.ToString().IndexOf('[') - 1);
			XmlNode objXmlQuality = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + strQualityName + "\"]");
			string strBook = _objCharacter.Options.LanguageBookShort(objXmlQuality["source"].InnerText);
			string strPage = objXmlQuality["page"].InnerText;
			if (objXmlQuality["altpage"] != null)
				strPage = objXmlQuality["altpage"].InnerText;
			lblSource.Text = strBook + " " + strPage;

			tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlQuality["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
		}

		private void treNegativeQualities_AfterSelect(object sender, TreeViewEventArgs e)
		{
			string strQualityName = treNegativeQualities.SelectedNode.Tag.ToString().Substring(0, treNegativeQualities.SelectedNode.Tag.ToString().IndexOf('[') - 1);
			XmlNode objXmlQuality = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + strQualityName + "\"]");
			string strBook = _objCharacter.Options.LanguageBookShort(objXmlQuality["source"].InnerText);
			string strPage = objXmlQuality["page"].InnerText;
			if (objXmlQuality["altpage"] != null)
				strPage = objXmlQuality["altpage"].InnerText;
			lblSource.Text = strBook + " " + strPage;

			tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlQuality["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
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
		/// Lifestyle that was created in the dialogue.
		/// </summary>
		public Lifestyle SelectedLifestyle
		{
			get
			{
				return _objLifestyle;
			}
		}

		/// <summary>
		/// Type of Lifestyle to create.
		/// </summary>
		public LifestyleType StyleType
		{
			get
			{
				return _objType;
			}
			set
			{
				_objType = value;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Accept the selected item and close the form.
		/// </summary>
		private void AcceptForm()
		{
			_objLifestyle.Source = "RC";
			_objLifestyle.Page = "154";
			_objLifestyle.Name = txtLifestyleName.Text;
			_objLifestyle.Cost = CalculateValues(false);
			_objLifestyle.Roommates = Convert.ToInt32(nudRoommates.Value);
			_objLifestyle.Percentage = Convert.ToInt32(nudPercentage.Value);
			_objLifestyle.Qualities.Clear();

			// Calculate the LP used by the Lifestyle (not including Qualities) and determine the appropriate Lifestyle to pull Starting Nuyen amounts from.
			int intEffectiveLP = 0;
			int intTotalLP = 0;

			// Calculate the cost of the 5 aspects. This determines the effective LP. Effective LP determines the effective equivalent Lifestyle (such as Medium) for determining the Starting Nuyen multiplier.
			XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");

			// Find the appropriate Lifestyle.
			XmlNode objXmlEffectiveLifestyle;
			if (intEffectiveLP >= 21)
				objXmlEffectiveLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"Luxury\"]");
			else if (intEffectiveLP >= 16 && intEffectiveLP <= 20)
				objXmlEffectiveLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"High\"]");
			else if (intEffectiveLP >= 11 && intEffectiveLP <= 15)
				objXmlEffectiveLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"Middle\"]");
			else if (intEffectiveLP >= 6 && intEffectiveLP <= 10)
				objXmlEffectiveLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"Low\"]");
			else if (intEffectiveLP >= 1 && intEffectiveLP <= 5)
				objXmlEffectiveLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"Squatter\"]");
			else
				objXmlEffectiveLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"Street\"]");

			// Add the Qualities LP to determine the total Lifestyle LP.
			// Calculate the cost of Positive Qualities.
			foreach (TreeNode objNode in trePositiveQualities.Nodes)
			{
				if (objNode.Checked)
				{
					objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + GetQualityName(objNode.Tag.ToString()) + "\" and category = \"Positive\"]");
					intTotalLP += Convert.ToInt32(objXmlAspect["lp"].InnerText);
				}
			}

			// Calculate the cost of Negative Qualities.
			foreach (TreeNode objNode in treNegativeQualities.Nodes)
			{
				if (objNode.Checked)
				{
					objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + GetQualityName(objNode.Tag.ToString()) + "\" and category = \"Negative\"]");
					intTotalLP += Convert.ToInt32(objXmlAspect["lp"].InnerText);
				}
			}

			// Find the appropriate Lifestyle.
			XmlNode objXmlActualLifestyle;
			if (intTotalLP >= 21)
				objXmlActualLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"Luxury\"]");
			else if (intTotalLP >= 16 && intTotalLP <= 20)
				objXmlActualLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"High\"]");
			else if (intTotalLP >= 11 && intTotalLP <= 15)
				objXmlActualLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"Middle\"]");
			else if (intTotalLP >= 6 && intTotalLP <= 10)
				objXmlActualLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"Low\"]");
			else if (intTotalLP >= 1 && intTotalLP <= 5)
				objXmlActualLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"Squatter\"]");
			else
				objXmlActualLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"Street\"]");

			// Get the starting Nuyen information.
			_objLifestyle.Dice = Convert.ToInt32(objXmlEffectiveLifestyle["dice"].InnerText);
			_objLifestyle.Multiplier = Convert.ToInt32(objXmlEffectiveLifestyle["multiplier"].InnerText);
			_objLifestyle.StyleType = _objType;

			foreach (TreeNode objNode in trePositiveQualities.Nodes)
			{
				if (objNode.Checked)
					_objLifestyle.Qualities.Add(objNode.Tag.ToString());
			}
			foreach (TreeNode objNode in treNegativeQualities.Nodes)
			{
				if (objNode.Checked)
					_objLifestyle.Qualities.Add(objNode.Tag.ToString());
			}

			this.DialogResult = DialogResult.OK;
		}

		/// <summary>
		/// Get the name of a Quality by parsing out its LP cost.
		/// </summary>
		/// <param name="strQuality">String to parse.</param>
		private string GetQualityName(string strQuality)
		{
			string strTemp = strQuality;
			int intPos = strTemp.IndexOf('[');

			strTemp = strTemp.Substring(0, intPos - 1);

			return strTemp;
		}

		/// <summary>
		/// Calculate the LP value for the selected items.
		/// </summary>
		private int CalculateValues(bool blnIncludePercentage = true)
		{
			if (_blnSkipRefresh)
				return 0;

			int intLP = 0;
			int intNuyen = 0;

            // Calculate the limits of the 3 aspects.
            // Comforts LP.
            XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
            Label_SelectAdvancedLifestyle_Base_Comforts.Text = LanguageManager.Instance.GetString("Label_SelectAdvancedLifestyle_Base_Comforts").Replace("{0}", objXmlAspect["minimum"].InnerText).Replace("{1}", objXmlAspect["limit"].InnerText);
            nudComforts.Minimum = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
            nudComforts.Value = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
            nudComforts.Maximum = Convert.ToInt32(objXmlAspect["limit"].InnerText);
            nudComfortsEntertainment.Maximum = Convert.ToInt32(objXmlAspect["limit"].InnerText);

            // Area.
            objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/neighborhoods/neighborhood[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
            Label_SelectAdvancedLifestyle_Base_Area.Text = LanguageManager.Instance.GetString("Label_SelectAdvancedLifestyle_Base_Area").Replace("{0}", objXmlAspect["minimum"].InnerText).Replace("{1}", objXmlAspect["limit"].InnerText);
            nudArea.Minimum = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
            nudArea.Value = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
            nudArea.Maximum = Convert.ToInt32(objXmlAspect["limit"].InnerText);
            nudAreaEntertainment.Maximum = Convert.ToInt32(objXmlAspect["limit"].InnerText);
            
            // Security.
            objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/securities/security[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
            Label_SelectAdvancedLifestyle_Base_Securities.Text = LanguageManager.Instance.GetString("Label_SelectAdvancedLifestyle_Base_Security").Replace("{0}", objXmlAspect["minimum"].InnerText).Replace("{1}", objXmlAspect["limit"].InnerText);
            nudSecurity.Minimum = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
            nudSecurity.Value = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
            nudSecurity.Maximum = Convert.ToInt32(objXmlAspect["limit"].InnerText);
            nudSecurityEntertainment.Maximum = Convert.ToInt32(objXmlAspect["limit"].InnerText);

            intLP = (Convert.ToInt32(nudComforts.Maximum) - Convert.ToInt32(nudComforts.Value));
            intLP += (Convert.ToInt32(nudArea.Maximum) - Convert.ToInt32(nudArea.Value));
            intLP += (Convert.ToInt32(nudSecurity.Maximum) - Convert.ToInt32(nudSecurity.Value));

            // Calculate the cost of Positive Qualities.
			foreach (TreeNode objNode in trePositiveQualities.Nodes)
			{
				if (objNode.Checked)
				{
					objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + GetQualityName(objNode.Tag.ToString()) + "\" and category = \"Positive\"]");
					intLP -= Convert.ToInt32(objXmlAspect["lp"].InnerText);
				}
			}

			// Calculate the cost of Negative Qualities.
			foreach (TreeNode objNode in treNegativeQualities.Nodes)
			{
				if (objNode.Checked)
				{
					objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + GetQualityName(objNode.Tag.ToString()) + "\" and category = \"Negative\"]");
					intLP -= Convert.ToInt32(objXmlAspect["lp"].InnerText);
				}
			}

			// Determine the Nuyen cost.
            XmlNode objXmlLifestyle = _objXmlDocument.SelectSingleNode("chummer/lifestyles/lifestyle[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");

			if (blnIncludePercentage)
			{
				intNuyen = Convert.ToInt32(Convert.ToDouble(intNuyen, GlobalOptions.Instance.CultureInfo) * (1.0 + Convert.ToDouble(nudRoommates.Value / 10, GlobalOptions.Instance.CultureInfo)));
				intNuyen = Convert.ToInt32(Convert.ToDouble(intNuyen, GlobalOptions.Instance.CultureInfo) * Convert.ToDouble(nudPercentage.Value / 100, GlobalOptions.Instance.CultureInfo));
			}
            intNuyen += Convert.ToInt32(objXmlLifestyle["cost"].InnerText);
			lblTotalLP.Text = intLP.ToString();
			lblCost.Text = String.Format("{0:###,###,##0¥}", intNuyen);

			return intNuyen;
		}

		/// <summary>
		/// Lifestyle to update when editing.
		/// </summary>
		/// <param name="objLifestyle">Lifestyle to edit.</param>
		public void SetLifestyle(Lifestyle objLifestyle)
		{
			_objSourceLifestyle = objLifestyle;
			_objType = objLifestyle.StyleType;
		}

		/// <summary>
		/// Sort the contents of a TreeView alphabetically.
		/// </summary>
		/// <param name="treTree">TreeView to sort.</param>
		private void SortTree(TreeView treTree)
		{
			List<TreeNode> lstNodes = new List<TreeNode>();
			foreach (TreeNode objNode in treTree.Nodes)
				lstNodes.Add(objNode);
			treTree.Nodes.Clear();
			try
			{
				SortByName objSort = new SortByName();
				lstNodes.Sort(objSort.Compare);
			}
			catch
			{
			}
			foreach (TreeNode objNode in lstNodes)
			treTree.Nodes.Add(objNode);
		}

		private void MoveControls()
		{
			//int intLeft = 0;
			//intLeft = Math.Max(lblLifestyleNameLabel.Left + lblLifestyleNameLabel.Width, Label_SelectAdvancedLifestyle_Upgrade_Comforts.Left + Label_SelectAdvancedLifestyle_Upgrade_Comforts.Width);
			//intLeft = Math.Max(intLeft, lblNeighborhood.Left + lblNeighborhood.Width);
			//intLeft = Math.Max(intLeft, lblSecurity.Left + lblSecurity.Width);

			//txtLifestyleName.Left = intLeft + 6;
			//cboBaseLifestyle.Left = intLeft + 6;
			//cboBaseLifestyle.Left = intLeft + 6;
			//cboBaseLifestyle.Left = intLeft + 6;
		}
		#endregion

        private void cmdAddQuality_Click(object sender, EventArgs e)
        {
            frmSelectLifestyleQuality frmSelectLifestyleQuality = new frmSelectLifestyleQuality(_objCharacter);
            frmSelectLifestyleQuality.ShowDialog(this);
        }

        private void cmdDeleteQuality_Click(object sender, EventArgs e)
        {

        }
	}
}