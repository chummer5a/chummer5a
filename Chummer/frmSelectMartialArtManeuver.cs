using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public partial class frmSelectMartialArtManeuver : Form
	{
		private string _strSelectedManeuver = "";

		private bool _blnAddAgain = false;

		private XmlDocument _objXmlDocument = new XmlDocument();
		private readonly Character _objCharacter;

		#region Control Events
		public frmSelectMartialArtManeuver(Character objCharacter)
		{
			_objCharacter = objCharacter;
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
		}

		private void frmSelectMartialArtManeuver_Load(object sender, EventArgs e)
		{
			foreach (Label objLabel in this.Controls.OfType<Label>())
			{
				if (objLabel.Text.StartsWith("["))
					objLabel.Text = "";
			}

			List<ListItem> lstManeuver = new List<ListItem>();

			// Load the Martial Art information.
			_objXmlDocument = XmlManager.Instance.Load("martialarts.xml");

			// Populate the Martial Art Maneuver list.
			XmlNodeList objManeuverList = _objXmlDocument.SelectNodes("/chummer/maneuvers/maneuver[" + _objCharacter.Options.BookXPath() + "]");
			foreach (XmlNode objXmlManeuver in objManeuverList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlManeuver["name"].InnerText;
				if (objXmlManeuver["translate"] != null)
					objItem.Name = objXmlManeuver["translate"].InnerText;
				else
					objItem.Name = objXmlManeuver["name"].InnerText;
				lstManeuver.Add(objItem);
			}
			SortListItem objSort = new SortListItem();
			lstManeuver.Sort(objSort.Compare);
			lstManeuvers.DataSource = null;
			lstManeuvers.ValueMember = "Value";
			lstManeuvers.DisplayMember = "Name";
			lstManeuvers.DataSource = lstManeuver;
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			if (lstManeuvers.Text != "")
				AcceptForm();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void lstMartialArts_DoubleClick(object sender, EventArgs e)
		{
			if (lstManeuvers.Text != "")
				AcceptForm();
		}

		private void lstMartialArts_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Populate the Maneuvers list.
			XmlNode objXmlManeuver = _objXmlDocument.SelectSingleNode("/chummer/maneuvers/maneuver[name = \"" + lstManeuvers.SelectedValue + "\"]");

			string strBook = _objCharacter.Options.LanguageBookShort(objXmlManeuver["source"].InnerText);
			string strPage = objXmlManeuver["page"].InnerText;
			if (objXmlManeuver["altpage"] != null)
				strPage = objXmlManeuver["altpage"].InnerText;
			lblSource.Text = strBook + " " + strPage;

			tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlManeuver["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
		}

		private void cmdOKAdd_Click(object sender, EventArgs e)
		{
			_blnAddAgain = true;
			cmdOK_Click(sender, e);
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
		/// Maneuver that was selected in the dialogue.
		/// </summary>
		public string SelectedManeuver
		{
			get
			{
				return _strSelectedManeuver;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Accept the selected item and close the form.
		/// </summary>
		private void AcceptForm()
		{
			_strSelectedManeuver = lstManeuvers.SelectedValue.ToString();
			this.DialogResult = DialogResult.OK;
		}
		#endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions objCommon = new CommonFunctions(_objCharacter);
            objCommon.OpenPDF(lblSource.Text);
        }
	}
}