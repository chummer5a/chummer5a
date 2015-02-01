using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public partial class frmSelectMartialArtAdvantage : Form
	{
		private string _strSelectedAdvantage = "";

		private bool _blnAddAgain = false;
		private string _strMartialArt = "";

		private XmlDocument _objXmlDocument = new XmlDocument();
		private readonly Character _objCharacter;

		#region Control Events
		public frmSelectMartialArtAdvantage(Character objCharacter)
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			_objCharacter = objCharacter;
		}

		private void frmSelectMartialArtAdvantage_Load(object sender, EventArgs e)
		{
			foreach (Label objLabel in this.Controls.OfType<Label>())
			{
				if (objLabel.Text.StartsWith("["))
					objLabel.Text = "";
			}

			List<ListItem> lstAdvantage = new List<ListItem>();

			// Load the Martial Art information.
			_objXmlDocument = XmlManager.Instance.Load("martialarts.xml");

			// Populate the Martial Art Advantage list.
			XmlNodeList objXmlAdvantageList = _objXmlDocument.SelectNodes("/chummer/martialarts/martialart[(" + _objCharacter.Options.BookXPath() + ") and name = \"" + _strMartialArt + "\"]/techniques/technique");
			foreach (XmlNode objXmlAdvantage in objXmlAdvantageList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlAdvantage["name"].InnerText;
				if (objXmlAdvantage.Attributes["translate"] != null)
					objItem.Name = objXmlAdvantage.Attributes["translate"].InnerText;
				else
					objItem.Name = objXmlAdvantage["name"].InnerText;

                bool blnIsNew = true;
                foreach (MartialArt objMartialArt in _objCharacter.MartialArts)
                {
                    if (objMartialArt.Name == _strMartialArt)
                    {
                        foreach (MartialArtAdvantage objMartialArtAdvantage in objMartialArt.Advantages)
                        {
                            if (objMartialArtAdvantage.Name == objItem.Value)
                            {
                                blnIsNew = false;
                            }
                        }
                    }
                }

                if (blnIsNew)
				    lstAdvantage.Add(objItem);
            }
			SortListItem objSort = new SortListItem();
			lstAdvantage.Sort(objSort.Compare);
			lstAdvantages.DataSource = null;
			lstAdvantages.ValueMember = "Value";
			lstAdvantages.DisplayMember = "Name";
			lstAdvantages.DataSource = lstAdvantage;
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			if (lstAdvantages.Text != "")
				AcceptForm();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void lstAdvantages_DoubleClick(object sender, EventArgs e)
		{
			if (lstAdvantages.Text != "")
				AcceptForm();
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
		/// Martial Art to display Advantages for.
		/// </summary>
		public string MartialArt
		{
			set
			{
				_strMartialArt = value;
			}
		}

		/// <summary>
		/// Martial Art Advantage that was selected in the dialogue.
		/// </summary>
		public string SelectedAdvantage
		{
			get
			{
				return _strSelectedAdvantage;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Accept the selected item and close the form.
		/// </summary>
		private void AcceptForm()
		{
			_strSelectedAdvantage = lstAdvantages.SelectedValue.ToString();
			this.DialogResult = DialogResult.OK;
		}
		#endregion
	}
}