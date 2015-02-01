using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Xml;

// ConnectionRatingChanged Event Handler.
public delegate void ConnectionRatingChangedHandler(Object sender);
// GroupRatingChanged Event Handler.
public delegate void ConnectionGroupRatingChangedHandler(Object sender);
// LoyaltyRatingChanged Event Handler.
public delegate void LoyaltyRatingChangedHandler(Object sender);
// DeleteContact Event Handler.
public delegate void DeleteContactHandler(Object sender);
// FileNameChanged Event Handler.
public delegate void FileNameChangedHandler(Object sender);

namespace Chummer
{
    public partial class ContactControl : UserControl
    {
		private Contact _objContact;
        private string _strContactName;
        private string _strContactRole;
        private string _strContactLocation;
        private bool _blnEnemy = false;

        // Events.
		public event ConnectionRatingChangedHandler ConnectionRatingChanged;
		public event ConnectionGroupRatingChangedHandler ConnectionGroupRatingChanged;
        public event LoyaltyRatingChangedHandler LoyaltyRatingChanged;
        public event DeleteContactHandler DeleteContact;
		public event FileNameChangedHandler FileNameChanged;

		#region Control Events
        public ContactControl()
        {
            InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
        }

        private void LoadContactList()
        {
            if (_blnEnemy)
            {
                if (_strContactRole != "")
                    cboContactRole.Text = _strContactRole;
                return;
            }

            // Read the list of Categories from the XML file.
            List<ListItem> lstCategories = new List<ListItem>();

            ListItem objBlank = new ListItem();
            objBlank.Value = "";
            objBlank.Name = "";
            lstCategories.Add(objBlank);

            XmlDocument objXmlDocument = new XmlDocument();
            objXmlDocument = XmlManager.Instance.Load("contacts.xml");
            XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/contacts/contact");
            foreach (XmlNode objXmlCategory in objXmlSkillList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objXmlCategory.InnerText;
                if (objXmlCategory.Attributes["translate"] != null)
                    objItem.Name = objXmlCategory.Attributes["translate"].InnerText;
                else
                    objItem.Name = objXmlCategory.InnerText;
                lstCategories.Add(objItem);
            }
            cboContactRole.ValueMember = "Value";
            cboContactRole.DisplayMember = "Name";
            cboContactRole.DataSource = lstCategories;

            if (_strContactRole != "")
                cboContactRole.Text = _strContactRole;
        }

		private void ContactControl_Load(object sender, EventArgs e)
		{
			this.Width = cmdDelete.Left + cmdDelete.Width;
            LoadContactList();
		}

		private void nudConnection_ValueChanged(object sender, EventArgs e)
        {
            // Raise the ConnectionGroupRatingChanged Event when the NumericUpDown's Value changes.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
			_objContact.Connection = Convert.ToInt32(nudConnection.Value);
            ConnectionRatingChanged(this);
        }

        private void nudLoyalty_ValueChanged(object sender, EventArgs e)
        {
            // Raise the LoyaltyRatingChanged Event when the NumericUpDown's Value changes.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
			_objContact.Loyalty = Convert.ToInt32(nudLoyalty.Value);
            LoyaltyRatingChanged(this);
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Raise the DeleteContact Event when the user has confirmed their desire to delete the Contact.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
			DeleteContact(this);
        }

        private void cboContactRole_TextChanged(object sender, EventArgs e)
		{
			_objContact.Role = cboContactRole.Text;
            ConnectionRatingChanged(this);
		}

        private void txtContactName_TextChanged(object sender, EventArgs e)
        {
            _objContact.Name = txtContactName.Text;
            ConnectionRatingChanged(this);
        }

        private void txtContactLocation_TextChanged(object sender, EventArgs e)
        {
            _objContact.Location = txtContactLocation.Text;
            ConnectionRatingChanged(this);
        }
        
        private void imgLink_Click(object sender, EventArgs e)
		{
			// Determine which options should be shown based on the FileName value.
			if (_objContact.FileName != "")
			{
				tsAttachCharacter.Visible = false;
				tsContactOpen.Visible = true;
				tsRemoveCharacter.Visible = true;
			}
			else
			{
				tsAttachCharacter.Visible = true;
				tsContactOpen.Visible = false;
				tsRemoveCharacter.Visible = false;
			}
			cmsContact.Show(imgLink, imgLink.Left - 490, imgLink.Top);
		}

		private void tsContactOpen_Click(object sender, EventArgs e)
		{
			bool blnError = false;
			bool blnUseRelative = false;

			// Make sure the file still exists before attempting to load it.
			if (!File.Exists(_objContact.FileName))
			{
				// If the file doesn't exist, use the relative path if one is available.
				if (_objContact.RelativeFileName == "")
					blnError = true;
				else
				{
					MessageBox.Show(Path.GetFullPath(_objContact.RelativeFileName));
					if (!File.Exists(Path.GetFullPath(_objContact.RelativeFileName)))
						blnError = true;
					else
						blnUseRelative = true;
				}

				if (blnError)
				{
					MessageBox.Show(LanguageManager.Instance.GetString("Message_FileNotFound").Replace("{0}", _objContact.FileName), LanguageManager.Instance.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}

			if (!blnUseRelative)
				GlobalOptions.Instance.MainForm.LoadCharacter(_objContact.FileName, false);
			else
			{
				string strFile = Path.GetFullPath(_objContact.RelativeFileName);
				GlobalOptions.Instance.MainForm.LoadCharacter(strFile, false);
			}
		}

		private void tsAttachCharacter_Click(object sender, EventArgs e)
		{
			// Prompt the user to select a save file to associate with this Contact.
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Chummer5 Files (*.chum5)|*.chum5|All Files (*.*)|*.*";

			if (openFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				_objContact.FileName = openFileDialog.FileName;
				if (_objContact.EntityType == ContactType.Enemy)
					tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Enemy_OpenFile"));
				else
					tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Contact_OpenFile"));

				// Set the relative path.
				Uri uriApplication = new Uri(@Application.StartupPath);
				Uri uriFile = new Uri(@_objContact.FileName);
				Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
				_objContact.RelativeFileName = "../" + uriRelative.ToString();

				FileNameChanged(this);
			}
		}

		private void tsRemoveCharacter_Click(object sender, EventArgs e)
		{
			// Remove the file association from the Contact.
			if (MessageBox.Show(LanguageManager.Instance.GetString("Message_RemoveCharacterAssociation"), LanguageManager.Instance.GetString("MessageTitle_RemoveCharacterAssociation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				_objContact.FileName = "";
				_objContact.RelativeFileName = "";
				if (_objContact.EntityType == ContactType.Enemy)
					tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Enemy_LinkFile"));
				else
					tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Contact_LinkFile"));
				FileNameChanged(this);
			}
		}

		private void cmdGroup_Click(object sender, EventArgs e)
		{
			frmSelectContactConnection frmPickContactConnection = new frmSelectContactConnection();
			frmPickContactConnection.Membership = _objContact.Membership;
			frmPickContactConnection.AreaOfInfluence = _objContact.AreaOfInfluence;
			frmPickContactConnection.MagicalResources = _objContact.MagicalResources;
			frmPickContactConnection.MatrixResources = _objContact.MatrixResources;
			frmPickContactConnection.GroupName = _objContact.GroupName;
			frmPickContactConnection.Colour = _objContact.Colour;
			frmPickContactConnection.Free = _objContact.Free;
			frmPickContactConnection.ShowDialog(this);

			if (frmPickContactConnection.DialogResult == DialogResult.Cancel)
				return;

			// Update the Connection Modifier values.
			_objContact.Membership = frmPickContactConnection.Membership;
			_objContact.AreaOfInfluence = frmPickContactConnection.AreaOfInfluence;
			_objContact.MagicalResources = frmPickContactConnection.MagicalResources;
			_objContact.MatrixResources = frmPickContactConnection.MatrixResources;
			_objContact.GroupName = frmPickContactConnection.GroupName;
			_objContact.Colour = frmPickContactConnection.Colour;
			_objContact.Free = frmPickContactConnection.Free;

			if (_objContact.Colour.Name != "White" && _objContact.Colour.Name != "Black")
				this.BackColor = _objContact.Colour;
			else
				this.BackColor = SystemColors.Control;

			ConnectionGroupRatingChanged(this);
		}

		private void imgNotes_Click(object sender, EventArgs e)
		{
			frmNotes frmContactNotes = new frmNotes();
			frmContactNotes.Notes = _objContact.Notes;
			frmContactNotes.ShowDialog(this);

			if (frmContactNotes.DialogResult == DialogResult.OK)
				_objContact.Notes = frmContactNotes.Notes;

			string strTooltip = "";
			if (_objContact.EntityType == ContactType.Enemy)
				strTooltip = LanguageManager.Instance.GetString("Tip_Enemy_EditNotes");
			else
				strTooltip = LanguageManager.Instance.GetString("Tip_Contact_EditNotes");
			if (_objContact.Notes != string.Empty)
				strTooltip += "\n\n" + _objContact.Notes;
			tipTooltip.SetToolTip(imgNotes, strTooltip);
		}

		private void cmsContact_Opening(object sender, CancelEventArgs e)
		{
			foreach (ToolStripItem objItem in ((ContextMenuStrip)sender).Items)
			{
				if (objItem.Tag != null)
				{
					objItem.Text = LanguageManager.Instance.GetString(objItem.Tag.ToString());
				}
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Contact object this is linked to.
		/// </summary>
		public Contact ContactObject
		{
			get
			{
				return _objContact;
			}
			set
			{
				_objContact = value;
			}
		}

        public bool IsEnemy
        {
            get
            {
                return _blnEnemy;
            }
            set
            {
                _blnEnemy = value;
                cboContactRole.Items.Clear();
            }
        }

        /// <summary>
        /// Contact name.
        /// </summary>
        public string ContactName
        {
            get
            {
				return _objContact.Name;
            }
            set
            {
                txtContactName.Text = value;
                _strContactName = value;
				_objContact.Name = value;
            }
        }

        /// <summary>
        /// Contact role.
        /// </summary>
        public string ContactRole
        {
            get
            {
                return _objContact.Role;
            }
            set
            {
                cboContactRole.Text = value;
                _strContactRole = value;
                _objContact.Role = value;
            }
        }

        /// <summary>
        /// Contact location.
        /// </summary>
        public string ContactLocation
        {
            get
            {
                return _objContact.Location;
            }
            set
            {
                txtContactLocation.Text = value;
                _strContactLocation = value;
                _objContact.Location = value;
            }
        }

        /// <summary>
        /// Contact name.
        /// </summary>
        public bool Fixed
        {
            get
            {
                return _objContact.Fixed;
            }
            set
            {
                cmdDelete.Enabled = value;
                _objContact.Fixed = value;
            }
        }

        /// <summary>
		/// Indicates if this is a Contact or Enemy.
		/// </summary>
		public ContactType EntityType
		{
			get
			{
				return _objContact.EntityType;
			}
			set
			{
				_objContact.EntityType = value;
				if (value ==  ContactType.Enemy)
				{
					if (_objContact.FileName != "")
						tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Enemy_OpenLinkedEnemy"));
					else
						tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Enemy_LinkEnemy"));

					string strTooltip = LanguageManager.Instance.GetString("Tip_Enemy_EditNotes");
					if (_objContact.Notes != string.Empty)
						strTooltip += "\n\n" + _objContact.Notes;
					tipTooltip.SetToolTip(imgNotes, strTooltip);

					nudConnection.Minimum = 1;
				}
				else
				{
					if (_objContact.FileName != "")
						tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Contact_OpenLinkedContact"));
					else
						tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Contact_LinkContact"));

					string strTooltip = LanguageManager.Instance.GetString("Tip_Contact_EditNotes");
					if (_objContact.Notes != string.Empty)
						strTooltip += "\n\n" + _objContact.Notes;
					tipTooltip.SetToolTip(imgNotes, strTooltip);
                    if (_objContact.Fixed)
                    {
                        nudConnection.Enabled = false;
                        nudLoyalty.Enabled = false;
                        cmdDelete.Visible = false;
                    }

					nudConnection.Minimum = 0;
				}
			}
		}

        /// <summary>
        /// Connection Rating.
        /// </summary>
        public int ConnectionRating
        {
            get
            {
				return _objContact.Connection;
            }
            set
            {
                nudConnection.Value = value;
				_objContact.Connection = value;
            }
        }

		/// <summary>
		/// Connection Modifier: Membership.
		/// </summary>
		public int MembershipRating
		{
			get
			{
				return _objContact.Membership;
			}
			set
			{
				_objContact.Membership = value;
			}
		}

		/// <summary>
		/// Connection Modifier: Area of Influence.
		/// </summary>
		public int AreaOfInfluenceRating
		{
			get
			{
				return _objContact.AreaOfInfluence;
			}
			set
			{
				_objContact.AreaOfInfluence = value;
			}
		}

		/// <summary>
		/// Connection Modifier: Magical Resources.
		/// </summary>
		public int MagicalResourcesRating
		{
			get
			{
				return _objContact.MagicalResources;
			}
			set
			{
				_objContact.MagicalResources = value;
			}
		}

		/// <summary>
		/// Connection Modifier: Matrix Resources.
		/// </summary>
		public int MatrixResourcesRating
		{
			get
			{
				return _objContact.MatrixResources;
			}
			set
			{
				_objContact.MatrixResources = value;
			}
		}

        /// <summary>
        /// Loyalty Rating.
        /// </summary>
        public int LoyaltyRating
        {
            get
            {
				return _objContact.Loyalty;
            }
            set
            {
                nudLoyalty.Value = value;
				_objContact.Loyalty = value;
            }
        }

		/// <summary>
		/// Whether or not this is a free Contact.
		/// </summary>
		public bool Free
		{
			get
			{
				return _objContact.Free;
			}
			set
			{
				_objContact.Free = value;
			}
		}
		#endregion
	}
}