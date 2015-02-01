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

			// Populate the Lifestyle ComboBoxes.
			List<ListItem> lstLifestyle = new List<ListItem>();
			foreach (XmlNode objXmlComfort in _objXmlDocument.SelectNodes("/chummer/lifestyles/lifestyle"))
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlComfort["name"].InnerText;
				if (objXmlComfort["translate"] != null)
					objItem.Name = objXmlComfort["translate"].InnerText;
				else
					objItem.Name = objXmlComfort["name"].InnerText;
                lstLifestyle.Add(objItem);
			}
			cboLifestyle.ValueMember = "Name";
			cboLifestyle.DisplayMember = "Name";
            cboLifestyle.DataSource = lstLifestyle;

			cboLifestyle.SelectedValue = _objLifestyle.BaseLifestyle;
			if (cboLifestyle.SelectedIndex == -1)
				cboLifestyle.SelectedIndex = 0;

			// Fill the Options list.
			foreach (XmlNode objXmlOption in _objXmlDocument.SelectNodes("/chummer/qualities/quality"))
			{
				TreeNode nodOption = new TreeNode();

                XmlNode nodCost = objXmlOption["lifestylecost"];
                if (nodCost != null)
                {
                    string strCost = nodCost.InnerText;
                    int intCost = Convert.ToInt32(strCost);
                    if (intCost > 0)
                    {
                        nodOption.Tag = objXmlOption["name"].InnerText + " [+" + intCost.ToString() + "%]";
                        if (objXmlOption["translate"] != null)
                            nodOption.Text = objXmlOption["translate"].InnerText + " [+" + intCost.ToString() + "%]";
                        else
                            nodOption.Text = objXmlOption["name"].InnerText + " [+" + intCost.ToString() + "%]";
                        treQualities.Nodes.Add(nodOption);
                    }
                    else
                    {
                        nodOption.Tag = objXmlOption["name"].InnerText + " [" + intCost.ToString() + "%]";
                        if (objXmlOption["translate"] != null)
                            nodOption.Text = objXmlOption["translate"].InnerText + " [" + intCost.ToString() + "%]";
                        else
                            nodOption.Text = objXmlOption["name"].InnerText + " [" + intCost.ToString() + "%]";
                        treQualities.Nodes.Add(nodOption);
                    }
                }
                else
                {
                    string strCost = objXmlOption["cost"].InnerText;
                    nodOption.Tag = objXmlOption["name"].InnerText + " [" + strCost + "¥]";
                    if (objXmlOption["translate"] != null)
                        nodOption.Text = objXmlOption["translate"].InnerText + " [" + strCost + "¥]";
                    else
                        nodOption.Text = objXmlOption["name"].InnerText + " [" + strCost + "¥]";
                    treQualities.Nodes.Add(nodOption);
                }
			}

            SortTree(treQualities);

			if (_objSourceLifestyle != null)
			{
				txtLifestyleName.Text = _objSourceLifestyle.Name;
				cboLifestyle.SelectedValue = _objSourceLifestyle.BaseLifestyle;
				nudRoommates.Value = _objSourceLifestyle.Roommates;
				nudPercentage.Value = _objSourceLifestyle.Percentage;
				foreach (string strQuality in _objSourceLifestyle.Qualities)
				{
                    foreach (TreeNode objNode in treQualities.Nodes)
					{
						if (objNode.Tag.ToString() == strQuality)
						{
							objNode.Checked = true;
							break;
						}
					}
				}
			}

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

		private void treQualities_AfterCheck(object sender, TreeViewEventArgs e)
		{
			CalculateValues();
		}

		private void cboLifestyle_SelectedIndexChanged(object sender, EventArgs e)
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

		private void treQualities_AfterSelect(object sender, TreeViewEventArgs e)
		{
            string strQualityName = treQualities.SelectedNode.Tag.ToString().Substring(0, treQualities.SelectedNode.Tag.ToString().IndexOf('[') - 1);
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
			_objLifestyle.Source = "SR5";
			_objLifestyle.Page = "373";
			_objLifestyle.Name = txtLifestyleName.Text;
			_objLifestyle.BaseLifestyle = cboLifestyle.SelectedValue.ToString();
			_objLifestyle.Cost = CalculateValues(false);
			_objLifestyle.Roommates = Convert.ToInt32(nudRoommates.Value);
			_objLifestyle.Percentage = Convert.ToInt32(nudPercentage.Value);
			_objLifestyle.Qualities.Clear();
			_objLifestyle.StyleType = _objType;

            foreach (TreeNode objNode in treQualities.Nodes)
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

			int intNuyen = 0;

            decimal decCost = 0;
            // Get the base cost of the lifestyle
            XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + cboLifestyle.SelectedValue + "\"]");
            decCost += Convert.ToDecimal(objXmlAspect["cost"].InnerText);
            _objLifestyle.Dice = Convert.ToInt32(objXmlAspect["dice"].InnerText);
            _objLifestyle.Multiplier = Convert.ToInt32(objXmlAspect["multiplier"].InnerText);

            // Add the flat costs from qualities
            foreach (TreeNode objNode in treQualities.Nodes)
            {
                if (objNode.Checked)
                {
                    XmlNode objXmlQuality = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + GetQualityName(objNode.Tag.ToString()) + "\"]");
                    if (objXmlQuality["cost"] != null)
                        decCost += Convert.ToDecimal(objXmlQuality["cost"].InnerText);
                }
            }

            decimal decMod = 0;
            if (blnIncludePercentage)
            {
                // Add the modifiers from qualities
                foreach (TreeNode objNode in treQualities.Nodes)
                {
                    if (objNode.Checked)
                    {
                        objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + GetQualityName(objNode.Tag.ToString()) + "\"]");
                        if (objXmlAspect["lifestylecost"] != null)
                            decMod += (Convert.ToDecimal(objXmlAspect["lifestylecost"].InnerText) / 100);
                    }
                }

                // Check for modifiers in the improvements
                ImprovementManager objImprovementManager = new ImprovementManager(_objCharacter);
                decimal decModifier = Convert.ToDecimal(objImprovementManager.ValueOf(Improvement.ImprovementType.LifestyleCost), GlobalOptions.Instance.CultureInfo);
                decMod += Convert.ToDecimal(decModifier / 100, GlobalOptions.Instance.CultureInfo);
            }

            intNuyen = Convert.ToInt32(decCost + (decCost * decMod));
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
			int intLeft = 0;
			intLeft = Math.Max(lblLifestyleNameLabel.Left + lblLifestyleNameLabel.Width, lblLifestyles.Left + lblLifestyles.Width);

			txtLifestyleName.Left = intLeft + 6;
			cboLifestyle.Left = intLeft + 6;
		}
		#endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions objCommon = new CommonFunctions(_objCharacter);
            objCommon.OpenPDF(lblSource.Text);
        }
	}
}