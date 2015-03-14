using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public partial class frmSelectLifestyle : Form
	{
		private bool _blnAddAgain = false;
		private Lifestyle _objLifestyle;
		private Lifestyle _objSourceLifestyle;

		private XmlDocument _objXmlDocument = new XmlDocument();
		private readonly Character _objCharacter;

		#region Control Events
		public frmSelectLifestyle(Lifestyle objLifestyle, Character objCharacter)
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			_objCharacter = objCharacter;
			_objLifestyle = objLifestyle;
			MoveControls();
		}

		private void frmSelectLifestyle_Load(object sender, EventArgs e)
		{
			foreach (Label objLabel in this.Controls.OfType<Label>())
			{
				if (objLabel.Text.StartsWith("["))
					objLabel.Text = "";
			}

			List<ListItem> lstLifestyle = new List<ListItem>();

			// Load the Lifestyles information.
			_objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");

			// Populate the Lifestyles list.
			XmlNodeList objXmlLifestyleList = _objXmlDocument.SelectNodes("/chummer/lifestyles/lifestyle[" + _objCharacter.Options.BookXPath() + "]");
			foreach (XmlNode objXmlLifestyle in objXmlLifestyleList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlLifestyle["name"].InnerText;
				if (objXmlLifestyle["translate"] != null)
					objItem.Name = objXmlLifestyle["translate"].InnerText;
				else
					objItem.Name = objXmlLifestyle["name"].InnerText;
				lstLifestyle.Add(objItem);
			}
			lstLifestyles.DataSource = null;
			lstLifestyles.ValueMember = "Value";
			lstLifestyles.DisplayMember = "Name";
			lstLifestyles.DataSource = lstLifestyle;

			if (_objSourceLifestyle != null)
			{
				foreach (ListItem objItem in lstLifestyles.Items)
				{
					if (objItem.Value == _objSourceLifestyle.Name)
					{
						lstLifestyles.SelectedItem = objItem;
						lstLifestyles.Enabled = false;
						break;
					}
				}
				_objLifestyle.LifestyleName = _objSourceLifestyle.LifestyleName;
				nudRoommates.Value = _objSourceLifestyle.Roommates;
				nudPercentage.Value = _objSourceLifestyle.Percentage;
			}
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			if (lstLifestyles.Text != "")
				AcceptForm();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void lstLifestyles_DoubleClick(object sender, EventArgs e)
		{
			if (lstLifestyles.Text != "")
				AcceptForm();
		}

		private void lstLifestyles_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateLifestyle();
		}

		private void cmdOKAdd_Click(object sender, EventArgs e)
		{
			_blnAddAgain = true;
			cmdOK_Click(sender, e);
		}

		private void nudPercentage_ValueChanged(object sender, EventArgs e)
		{
			UpdateLifestyle();
		}

		private void nudRoommates_ValueChanged(object sender, EventArgs e)
		{
			UpdateLifestyle();
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
		#endregion

		#region Methods
		/// <summary>
		/// Accept the selected item and close the form.
		/// </summary>
		private void AcceptForm()
		{
			XmlNode objXmlLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + lstLifestyles.SelectedValue + "\"]");
			_objLifestyle.Name = objXmlLifestyle["name"].InnerText;
			_objLifestyle.Cost = Convert.ToInt32(objXmlLifestyle["cost"].InnerText);
			_objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"].InnerText);
			_objLifestyle.Multiplier = Convert.ToInt32(objXmlLifestyle["multiplier"].InnerText);
			_objLifestyle.Source = objXmlLifestyle["source"].InnerText;
			_objLifestyle.Page = objXmlLifestyle["page"].InnerText;
			_objLifestyle.Roommates = Convert.ToInt32(nudRoommates.Value);
			_objLifestyle.Percentage = Convert.ToInt32(nudPercentage.Value);
			
			this.DialogResult = DialogResult.OK;
		}

		/// <summary>
		/// Update the Lifestyle information.
		/// </summary>
		private int UpdateLifestyle()
		{
			if (lstLifestyles.Text == "")
				return 0;

			// Display the information for the selected Lifestyle.
			XmlNode objXmlLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + lstLifestyles.SelectedValue + "\"]");

			int intCost = Convert.ToInt32(Convert.ToDouble(objXmlLifestyle["cost"].InnerText, GlobalOptions.Instance.CultureInfo) * (1 + Convert.ToDouble(nudRoommates.Value / 10, GlobalOptions.Instance.CultureInfo)) * Convert.ToDouble(nudPercentage.Value / 100, GlobalOptions.Instance.CultureInfo));

			lblCost.Text = String.Format("{0:###,###,##0¥}", intCost);
			lblStartingNuyen.Text = objXmlLifestyle["dice"].InnerText + "D6 x " + String.Format("{0:###,###,##0¥}", Convert.ToInt32(objXmlLifestyle["multiplier"].InnerText));

			string strBook = _objCharacter.Options.LanguageBookShort(objXmlLifestyle["source"].InnerText);
			string strPage = objXmlLifestyle["page"].InnerText;
			if (objXmlLifestyle["altpage"] != null)
				strPage = objXmlLifestyle["altpage"].InnerText;
			lblSource.Text = strBook + " " + strPage;

			tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlLifestyle["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);

			return intCost;
		}

		/// <summary>
		/// Lifestyle to update when editing.
		/// </summary>
		/// <param name="objLifestyle">Lifestyle to edit.</param>
		public void SetLifestyle(Lifestyle objLifestyle)
		{
			_objSourceLifestyle = objLifestyle;
		}

		private void MoveControls()
		{
			int intWidth = Math.Max(lblCostLabel.Width, lblStartingNuyenLabel.Width);
			intWidth = Math.Max(intWidth, lblSourceLabel.Width);
			intWidth = Math.Max(intWidth, lblPercentage.Width);

			lblCost.Left = lblCostLabel.Left + intWidth + 6;
			lblStartingNuyen.Left = lblStartingNuyenLabel.Left + intWidth + 6;
			lblSource.Left = lblSourceLabel.Left + intWidth + 6;
			nudPercentage.Left = lblPercentage.Left + intWidth + 6;
		}
		#endregion
	}
}