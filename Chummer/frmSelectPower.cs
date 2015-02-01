using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectPower : Form
    {
        private string _strSelectedPower = "";
        private bool _blnLevels = false;
        private decimal _decPointsPerLevel = 0;
        private decimal _decAdeptWayDiscount = 0;
		private int _intMaxLevels = 0;

		private bool _blnAddAgain = false;

		private readonly Character _objCharacter;

		private XmlDocument _objXmlDocument = new XmlDocument();

		#region Control Events
		public frmSelectPower(Character objCharacter)
        {
            InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			_objCharacter = objCharacter;
			MoveControls();
        }

        private void frmSelectPower_Load(object sender, EventArgs e)
        {
			foreach (Label objLabel in this.Controls.OfType<Label>())
			{
				if (objLabel.Text.StartsWith("["))
					objLabel.Text = "";
			}

        	List<ListItem> lstPower = new List<ListItem>();

            // Load the Powers information.
			_objXmlDocument = XmlManager.Instance.Load("powers.xml");

            // Populate the Powers list.
			XmlNodeList objXmlPowerList = _objXmlDocument.SelectNodes("/chummer/powers/power[" + _objCharacter.Options.BookXPath() + "]");
			foreach (XmlNode objXmlPower in objXmlPowerList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlPower["name"].InnerText;
				if (objXmlPower["translate"] != null)
					objItem.Name = objXmlPower["translate"].InnerText;
				else
					objItem.Name = objXmlPower["name"].InnerText;
				lstPower.Add(objItem);
			}
			SortListItem objSort = new SortListItem();
			lstPower.Sort(objSort.Compare);
			lstPowers.DataSource = null;
			lstPowers.ValueMember = "Value";
			lstPowers.DisplayMember = "Name";
			lstPowers.DataSource = lstPower;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (lstPowers.Text != "")
                AcceptForm();
        }

        private void lstPowers_DoubleClick(object sender, EventArgs e)
        {
            if (lstPowers.Text != "")
                AcceptForm();
        }

		private void lstPowers_SelectedIndexChanged(object sender, EventArgs e)
        {
			if (lstPowers.SelectedValue == null)
				return;

            // Display the information for the selected Power.
			XmlNode objXmlPower = _objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + lstPowers.SelectedValue + "\"]");

            lblPowerPoints.Text = objXmlPower["points"].InnerText;
			if (objXmlPower["levels"].InnerText == "yes")
			{
				lblPowerPoints.Text += " / ";
				lblPowerPoints.Text += LanguageManager.Instance.GetString("Label_Power_Level");
			}

			string strBook = _objCharacter.Options.LanguageBookShort(objXmlPower["source"].InnerText);
			string strPage = objXmlPower["page"].InnerText;
			if (objXmlPower["altpage"] != null)
				strPage = objXmlPower["altpage"].InnerText;
			lblSource.Text = strBook + " " + strPage;

			tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlPower["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
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

		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
			XmlNodeList objXmlPowerList;
			if (txtSearch.Text == "")
				objXmlPowerList = _objXmlDocument.SelectNodes("/chummer/powers/power[" + _objCharacter.Options.BookXPath() + "]");
			else
				objXmlPowerList = _objXmlDocument.SelectNodes("/chummer/powers/power[(" + _objCharacter.Options.BookXPath() + ") and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))]");

			List<ListItem> lstPower = new List<ListItem>();
			foreach (XmlNode objXmlPower in objXmlPowerList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlPower["name"].InnerText;
				if (objXmlPower["translate"] != null)
					objItem.Name = objXmlPower["translate"].InnerText;
				else
					objItem.Name = objXmlPower["name"].InnerText;
				lstPower.Add(objItem);
			}
			SortListItem objSort = new SortListItem();
			lstPower.Sort(objSort.Compare);
			lstPowers.DataSource = null;
			lstPowers.ValueMember = "Value";
			lstPowers.DisplayMember = "Name";
			lstPowers.DataSource = lstPower;
		}

		private void txtSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
			{
				try
				{
					lstPowers.SelectedIndex++;
				}
				catch
				{
					try
					{
						lstPowers.SelectedIndex = 0;
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
					lstPowers.SelectedIndex--;
					if (lstPowers.SelectedIndex == -1)
						lstPowers.SelectedIndex = lstPowers.Items.Count - 1;
				}
				catch
				{
					try
					{
						lstPowers.SelectedIndex = lstPowers.Items.Count - 1;
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
        /// Power that was selected in the dialogue.
        /// </summary>
        public string SelectedPower
        {
            get
            {
                return _strSelectedPower;
            }
        }

        /// <summary>
        /// Is Power level enabled for this Power?
        /// </summary>
        public bool LevelEnabled
        {
            get
            {
                return _blnLevels;
            }
        }

        /// <summary>
        /// Power Points per level cost for the Power.
        /// </summary>
        public decimal PointsPerLevel
        {
            get
            {
                return _decPointsPerLevel;
            }
        }

        /// <summary>
        /// Power Point discount for an Adept Way.
        /// </summary>
        public decimal AdeptWayDiscount
        {
            get
            {
                return _decAdeptWayDiscount;
            }
        }

        /// <summary>
		/// Maximum number of levels allowed by the Power. Standard rules apply if this is 0.
		/// </summary>
		public int MaxLevels()
		{
			return _intMaxLevels;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Accept the selected item and close the form.
		/// </summary>
        private void AcceptForm()
        {
            // Check to see if the user needs to select anything for the Power.
			XmlNode objXmlPower = _objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + lstPowers.SelectedValue + "\"]");

			// Make sure the character meets the Quality requirements if any.
			if (objXmlPower.InnerXml.Contains("<required>"))
			{
				string strRequirement = "";
				bool blnRequirementMet = true;

				// Quality requirements.
				foreach (XmlNode objXmlQuality in objXmlPower.SelectNodes("required/allof/quality"))
				{
					bool blnFound = false;
					foreach (Quality objQuality in _objCharacter.Qualities)
					{
						if (objQuality.Name == objXmlQuality.InnerText)
						{
							blnFound = true;
							break;
						}
					}

					if (!blnFound)
					{
						blnRequirementMet = false;
						strRequirement += "\n\t" + objXmlQuality.InnerText;
					}
				}

				foreach (XmlNode objXmlQuality in objXmlPower.SelectNodes("required/oneof/quality"))
				{
					blnRequirementMet = false;
					foreach (Quality objQuality in _objCharacter.Qualities)
					{
						if (objQuality.Name == objXmlQuality.InnerText)
						{
							blnRequirementMet = true;
							break;
						}
					}

					if (!blnRequirementMet)
						strRequirement += "\n\t" + objXmlQuality.InnerText;
					else
						break;
				}

				if (!blnRequirementMet)
				{
					string strMessage = LanguageManager.Instance.GetString("Message_SelectPower_PowerRequirement");
					strMessage += strRequirement;

					MessageBox.Show(strMessage, LanguageManager.Instance.GetString("MessageTitle_SelectPower_PowerRequirement"), MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
			}

            // Retrieve the Power Level information.
			if (objXmlPower["levels"].InnerText != "no")
			{
				_blnLevels = true;
                if (objXmlPower["maxlevel"] != null)
                    _intMaxLevels = Convert.ToInt32(objXmlPower["maxlevel"].InnerText);
                else
                    _intMaxLevels = 100;
			}
			else
				_blnLevels = false;

			_decPointsPerLevel = Convert.ToDecimal(objXmlPower["points"].InnerText, GlobalOptions.Instance.CultureInfo);
            _decAdeptWayDiscount = Convert.ToDecimal(objXmlPower["adeptway"].InnerText, GlobalOptions.Instance.CultureInfo);

            _strSelectedPower = lstPowers.SelectedValue.ToString();
            this.DialogResult = DialogResult.OK;
        }

		private void MoveControls()
		{
			lblPowerPoints.Left = lblPowerPointsLabel.Left + lblPowerPointsLabel.Width + 6;
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