using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
	public partial class frmSelectLifestyleQuality : Form
	{
        public int buildPos = 0;
        public int buildNeg = 0;
		private string _strSelectedQuality = "";
		private bool _blnAddAgain = false;
		private readonly Character _objCharacter;
		private string _strIgnoreQuality = "";

		private XmlDocument _objXmlDocument = new XmlDocument();

		private List<ListItem> _lstCategory = new List<ListItem>();

		private static string _strSelectCategory = "";

		private readonly XmlDocument _objMetatypeDocument = new XmlDocument();
		private readonly XmlDocument _objCritterDocument = new XmlDocument();

		#region Control Events
		public frmSelectLifestyleQuality(Character objCharacter)
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			_objCharacter = objCharacter;

			_objMetatypeDocument = XmlManager.Instance.Load("metatypes.xml");
			_objCritterDocument = XmlManager.Instance.Load("critters.xml");

			MoveControls();
		}

		private void frmSelectLifestyleQuality_Load(object sender, EventArgs e)
		{
			_objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");

			foreach (Label objLabel in this.Controls.OfType<Label>())
			{
				if (objLabel.Text.StartsWith("["))
					objLabel.Text = "";
			}

			// Load the Quality information.
            _objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");

			// Populate the Quality Category list.
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

			// Change the BP Label to Karma if the character is being built with Karma instead (or is in Career Mode).
            if (_objCharacter.BuildMethod == CharacterBuildMethod.Karma || _objCharacter.BuildMethod == CharacterBuildMethod.Priority || _objCharacter.Created)
				lblBPLabel.Text = LanguageManager.Instance.GetString("Label_Karma");

            BuildLifestyleQualityList();
        }

		private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
		{
			BuildLifestyleQualityList();
		}

		private void lstLifestyleQualities_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lstLifestyleQualities.Text == "")
				return;

            XmlNode objXmlQuality = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + lstLifestyleQualities.SelectedValue + "\"]");
			int intBP = Convert.ToInt32(objXmlQuality["lp"].InnerText);
            if (_objCharacter.Created && !_objCharacter.Options.DontDoubleQualities)
            {
                intBP *= 2;
            }
			lblBP.Text = (intBP * _objCharacter.Options.KarmaQuality).ToString();
			if (chkFree.Checked)
				lblBP.Text = "0";

			string strBook = _objCharacter.Options.LanguageBookShort(objXmlQuality["source"].InnerText);
			string strPage = objXmlQuality["page"].InnerText;
			if (objXmlQuality["altpage"] != null)
				strPage = objXmlQuality["altpage"].InnerText;
			lblSource.Text = strBook + " " + strPage;

			tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlQuality["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			_blnAddAgain = false;
			AcceptForm();
		}

		private void cmdOKAdd_Click(object sender, EventArgs e)
		{
			_blnAddAgain = true;
			AcceptForm();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void lstLifestyleQualities_DoubleClick(object sender, EventArgs e)
		{
			_blnAddAgain = false;
			AcceptForm();
		}

		private void chkLimitList_CheckedChanged(object sender, EventArgs e)
		{
			BuildLifestyleQualityList();
		}

		private void chkFree_CheckedChanged(object sender, EventArgs e)
		{
			lstLifestyleQualities_SelectedIndexChanged(sender, e);
		}

		private void chkMetagenetic_CheckedChanged(object sender, EventArgs e)
		{
			BuildLifestyleQualityList();
		}

		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
			BuildLifestyleQualityList();
		}

		private void txtSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
			{
				try
				{
					lstLifestyleQualities.SelectedIndex++;
				}
				catch
				{
					try
					{
						lstLifestyleQualities.SelectedIndex = 0;
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
					lstLifestyleQualities.SelectedIndex--;
					if (lstLifestyleQualities.SelectedIndex == -1)
						lstLifestyleQualities.SelectedIndex = lstLifestyleQualities.Items.Count - 1;
				}
				catch
				{
					try
					{
						lstLifestyleQualities.SelectedIndex = lstLifestyleQualities.Items.Count - 1;
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
		/// Quality that was selected in the dialogue.
		/// </summary>
		public string SelectedQuality
		{
			get
			{
				return _strSelectedQuality;
			}
		}

		/// <summary>
		/// Forcefully add a Category to the list.
		/// </summary>
		public string ForceCategory
		{
			set
			{
				cboCategory.DataSource = null;
				cboCategory.Items.Add(value);
			}
		}

		/// <summary>
		/// A Quality the character has that should be ignored for checking Fobidden requirements (which would prevent upgrading/downgrading a Quality).
		/// </summary>
		public string IgnoreQuality
		{
			set
			{
				_strIgnoreQuality = value;
			}
		}

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
		/// Whether or not the item has no cost.
		/// </summary>
		public bool FreeCost
		{
			get
			{
				return chkFree.Checked;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Build the list of Qualities.
		/// </summary>
		private void BuildLifestyleQualityList()
		{
			List<ListItem> lstLifestyleQuality = new List<ListItem>();
			if (txtSearch.Text.Trim() != "")
			{
				// Treat everything as being uppercase so the search is case-insensitive.
                string strSearch = "/chummer/qualities/quality[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))";
				
				XmlNodeList objXmlQualityList = _objXmlDocument.SelectNodes(strSearch);
				foreach (XmlNode objXmlQuality in objXmlQualityList)
				{
                    if (objXmlQuality["hide"] == null)
                    {
                        if (!chkLimitList.Checked || (chkLimitList.Checked && RequirementMet(objXmlQuality, false)))
                        {
                            ListItem objItem = new ListItem();
                            objItem.Value = objXmlQuality["name"].InnerText;
                            if (objXmlQuality["translate"] != null)
                                objItem.Name = objXmlQuality["translate"].InnerText;
                            else
                                objItem.Name = objXmlQuality["name"].InnerText;

                             try
                             {
                                 objItem.Name += " [" + _lstCategory.Find(objFind => objFind.Value == objXmlQuality["category"].InnerText).Name + "]";
                            
                                 lstLifestyleQuality.Add(objItem);
                            
                             }
                             catch
                             {
                             }
                        }
                    }
				}
			}
			
			SortListItem objSort = new SortListItem();
			lstLifestyleQuality.Sort(objSort.Compare);
            lstLifestyleQualities.DataSource = null;
            lstLifestyleQualities.ValueMember = "Value";
            lstLifestyleQualities.DisplayMember = "Name";
            lstLifestyleQualities.DataSource = lstLifestyleQuality;
		}

		/// <summary>
		/// Accept the selected item and close the form.
		/// </summary>
		private void AcceptForm()
		{
			if (lstLifestyleQualities.Text == "")
				return;

            XmlNode objNode = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + lstLifestyleQualities.SelectedValue + "\"]");
			_strSelectedQuality = objNode["name"].InnerText;
			_strSelectCategory = objNode["category"].InnerText;

			if (!RequirementMet(objNode, true))
				return;
			this.DialogResult = DialogResult.OK;
		}

		/// <summary>
		/// Check if the Quality's requirements/restrictions are being met.
		/// </summary>
		/// <param name="objXmlQuality">XmlNode of the Quality.</param>
		/// <param name="blnShowMessage">Whether or not a message should be shown if the requirements are not met.</param>
		private bool RequirementMet(XmlNode objXmlQuality, bool blnShowMessage)
		{
			// Ignore the rules.
			if (_objCharacter.IgnoreRules)
				return true;

			// See if the character already has this Quality and whether or not multiple copies are allowed.
			bool blnAllowMultiple = false;
			if (objXmlQuality["limit"] != null)
			{
				if (objXmlQuality["limit"].InnerText == "no")
					blnAllowMultiple = true;
			}
			if (!blnAllowMultiple)
			{
				// Multiples aren't allowed, so make sure the character does not already have it.
				foreach (Quality objQuality in _objCharacter.Qualities)
				{
					if (objQuality.Name == objXmlQuality["name"].InnerText)
					{
						if (blnShowMessage)
							MessageBox.Show(LanguageManager.Instance.GetString("Message_SelectQuality_QualityLimit"), LanguageManager.Instance.GetString("MessageTitle_SelectQuality_QualityLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
						return false;
					}
				}
			}     
			return true;
		}

		private void MoveControls()
		{
			int intWidth = Math.Max(lblBPLabel.Width, lblSourceLabel.Width);
			lblBP.Left = lblBPLabel.Left + intWidth + 6;
			lblSource.Left = lblSourceLabel.Left + intWidth + 6;

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