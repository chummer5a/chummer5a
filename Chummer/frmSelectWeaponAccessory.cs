using System;
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
		private bool _blnAddAgain = false;

		private XmlDocument _objXmlDocument = new XmlDocument();
		private readonly Character _objCharacter;
		private int _intAccessoryMultiplier = 1;

		#region Control Events
		public frmSelectWeaponAccessory(Character objCharacter, bool blnCareer = false)
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
			SortListItem objSort = new SortListItem();
			lstAccessories.Sort(objSort.Compare);
			lstAccessory.ValueMember = "Value";
			lstAccessory.DisplayMember = "Name";
			lstAccessory.DataSource = lstAccessories;
		}

		private void lstAccessory_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Retireve the information for the selected Accessory.
			XmlNode objXmlAccessory = _objXmlDocument.SelectSingleNode("/chummer/accessories/accessory[name = \"" + lstAccessory.SelectedValue + "\"]");

			if (objXmlAccessory.InnerXml.Contains("<rc>"))
				lblRC.Text = objXmlAccessory["rc"].InnerText;
			else
				lblRC.Text = "";

			string[] strMounts = objXmlAccessory["mount"].InnerText.Split('/');
			string strMount = "";
			foreach (string strCurrentMount in strMounts)
			{
				if (strCurrentMount != "")
					strMount += LanguageManager.Instance.GetString("String_Mount" + strCurrentMount) + "/";
			}
			// Remove the trailing /
			if (strMount != "" && strMount.Contains('/'))
				strMount = strMount.Substring(0, strMount.Length - 1);

			lblMount.Tag = objXmlAccessory["mount"].InnerText;
			lblMount.Text = strMount;
			lblAvail.Text = objXmlAccessory["avail"].InnerText.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted")).Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));

			string strCost = objXmlAccessory["cost"].InnerText;
			strCost = strCost.Replace("Weapon Cost", _intWeaponCost.ToString());
			if (chkFreeItem.Checked)
				strCost = "0";

            if (strCost.Contains("Variable"))
            {
                lblCost.Text = strCost;
                lblTest.Text = "";
            }
            else
            {
                XPathNavigator nav = _objXmlDocument.CreateNavigator();
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
			lstAccessory_SelectedIndexChanged(sender, e);
		}

		private void nudMarkup_ValueChanged(object sender, EventArgs e)
		{
			lstAccessory_SelectedIndexChanged(sender, e);
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
				return lblMount.Tag.ToString();
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
			_intMarkup = Convert.ToInt32(nudMarkup.Value);
			this.DialogResult = DialogResult.OK;
		}

		private void MoveControls()
		{
			int intWidth = Math.Max(lblRCLabel.Width, lblMountLabel.Width);
			intWidth = Math.Max(intWidth, lblAvailLabel.Width);
			intWidth = Math.Max(intWidth, lblCostLabel.Width);

			lblRC.Left = lblRCLabel.Left + intWidth + 6;
			lblMount.Left = lblMountLabel.Left + intWidth + 6;
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