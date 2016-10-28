/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
﻿using System.Runtime.CompilerServices;
﻿using System.Xml;

// ConnectionRatingChanged Event Handler.
public delegate void ConnectionRatingChangedHandler(Object sender);
// GroupRatingChanged Event Handler.
public delegate void ConnectionGroupRatingChangedHandler(Object sender);
// FreeRatingChanged Event Handler.
public delegate void FreeRatingChangedHandler(Object sender);
// LoyaltyRatingChanged Event Handler.
public delegate void LoyaltyRatingChangedHandler(Object sender);
// DeleteContact Event Handler.
public delegate void DeleteContactHandler(Object sender);
// FileNameChanged Event Handler.
public delegate void FileNameChangedHandler(Object sender);
// FamilyChanged Event Handler.
public delegate void FamilyChangedHandler(Object sender);
// BlackmailChanged Event Handler.
public delegate void BlackmailChangedHandler(Object sender);
// OtherCostChanged Event Handler.
public delegate void OtherCostChangedHandler(Object sender);

namespace Chummer
{
    public partial class ContactControl : UserControl
    {
        private Contact _objContact;
		private CommonFunctions functions = new CommonFunctions();
        private readonly Character _objCharacter;
        private string _strContactName;
        private string _strContactRole;
        private string _strContactLocation;
        private bool _blnEnemy = false;
	    private bool _loading = true;


        // Events.
        public event ConnectionRatingChangedHandler ConnectionRatingChanged;
        public event ConnectionGroupRatingChangedHandler GroupStatusChanged;
		public event LoyaltyRatingChangedHandler LoyaltyRatingChanged;
        public event FreeRatingChangedHandler FreeRatingChanged;
        public event DeleteContactHandler DeleteContact;
        public event FileNameChangedHandler FileNameChanged;
        public event BlackmailChangedHandler BlackmailChanged;
        public event FamilyChangedHandler FamilyChanged;

        #region Control Events
        public ContactControl(Character objCharacter)
        {
            InitializeComponent();
            _objCharacter = objCharacter;

	        if (!_objCharacter.Created)
            {
                if (_objCharacter.FriendsInHighPlaces)
                {
                    nudConnection.Maximum = 12;
                }
                else
                {
                    nudConnection.Maximum = 6;
                }
            }
            else
            {
                nudConnection.Maximum = 12;
            }

            //We don't actually pay for contacts in play so everyone is free
            //Don't present a useless field
            if (_objCharacter.Created)
            {
                chkFree.Visible = false;
            }
	        MoveControls();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
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
            if (!_objCharacter.Created)
            {
                if (_objCharacter.FriendsInHighPlaces)
                {
                    nudConnection.Maximum = 12;
                }
                else
                {
                    nudConnection.Maximum = 6;
                }
            }
            else
            {
                nudConnection.Maximum = 12;
            }
            _objContact.Connection = Convert.ToInt32(nudConnection.Value);
            ConnectionRatingChanged(this);
            UpdateQuickText();
        }

        private void nudLoyalty_ValueChanged(object sender, EventArgs e)
        {
            // Raise the LoyaltyRatingChanged Event when the NumericUpDown's Value changes.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            _objContact.Loyalty = Convert.ToInt32(nudLoyalty.Value);
            LoyaltyRatingChanged(this);
            UpdateQuickText();
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Raise the DeleteContact Event when the user has confirmed their desire to delete the Contact.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            DeleteContact(this);
        }

		private void chkGroup_CheckedChanged(object sender, EventArgs e)
		{
			if (_loading)
				return;

			_objContact.IsGroup = chkGroup.Checked;
			chkGroup.Enabled = !_objContact.MadeMan;

            if (GroupStatusChanged != null) GroupStatusChanged(this);

            //Loyalty can be changed by event above
            nudLoyalty.Enabled = !_objContact.IsGroup;
			nudLoyalty.Value = _objContact.Loyalty;
			UpdateQuickText();
		}

		
        private void cmdExpand_Click(object sender, EventArgs e)
        {
	        bool blnExpanded = (this.Height > 22);
	        if (blnExpanded)
            {
                this.Height -= 25;
                this.cmdExpand.Image = Properties.Resources.Expand;
            }
            else
            {
                this.Height += 25;
                this.cmdExpand.Image = Properties.Resources.Collapse;
            }
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
	        if (Path.GetExtension(_objContact.FileName) == "chum5")
	        {
		        if (!blnUseRelative)
			        GlobalOptions.Instance.MainForm.LoadCharacter(_objContact.FileName, false);
		        else
		        {
			        string strFile = Path.GetFullPath(_objContact.RelativeFileName);
			        GlobalOptions.Instance.MainForm.LoadCharacter(strFile, false);
		        }
	        }
	        else
	        {
				if (!blnUseRelative)
					System.Diagnostics.Process.Start(_objContact.FileName);
				else
				{
					string strFile = Path.GetFullPath(_objContact.RelativeFileName);
					System.Diagnostics.Process.Start(strFile);
				}
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
            tipTooltip.SetToolTip(imgNotes, CommonFunctions.WordWrap(strTooltip, 100));
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

		private void chkFree_CheckedChanged(object sender, EventArgs e)
		{
			_objContact.Free = chkFree.Checked;
            FreeRatingChanged?.Invoke(this);
        }

		private void chkBlackmail_CheckedChanged(object sender, EventArgs e)
		{
			_objContact.Blackmail = chkBlackmail.Checked;
            BlackmailChanged?.Invoke(this);
        }

		private void chkFamily_CheckedChanged(object sender, EventArgs e)
		{
			_objContact.Family = chkFamily.Checked;
            FamilyChanged?.Invoke(this);
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
                if (value == ContactType.Enemy)
                {
                    if (_objContact.FileName != "")
                        tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Enemy_OpenLinkedEnemy"));
                    else
                        tipTooltip.SetToolTip(imgLink, LanguageManager.Instance.GetString("Tip_Enemy_LinkEnemy"));

                    string strTooltip = LanguageManager.Instance.GetString("Tip_Enemy_EditNotes");
                    if (_objContact.Notes != string.Empty)
                        strTooltip += "\n\n" + _objContact.Notes;
					tipTooltip.SetToolTip(imgNotes, CommonFunctions.WordWrap(strTooltip, 100));
	                chkFamily.Visible = false;
	                chkBlackmail.Visible = false;
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
					tipTooltip.SetToolTip(imgNotes, CommonFunctions.WordWrap(strTooltip, 100));

					nudConnection.Minimum = 1;
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
		/// Whether or not this contact is a Family member.
		/// </summary>
	    public bool Family
	    {
		    get { return _objContact.Family; }
			set { _objContact.Family = value; }
	    }

		/// <summary>
		/// Whether or not this contact is being blackmailed. 
		/// </summary>
	    public bool Blackmail
	    {
		    get { return _objContact.Blackmail; }
			set { _objContact.Blackmail = value; }
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
        /// <summary>
        /// Is the contract a group contract
        /// </summary>
        public bool IsGroup
        {
            get
            {
                return _objContact.IsGroup;
            }
            set
            {
                _objContact.IsGroup = value;
            }
        }
		#endregion

		#region Methods
		private void LoadContactList()
		{
			if (_blnEnemy)
			{
				if (_strContactRole != "")
					cboContactRole.Text = _strContactRole;
				return;
			}

			if (_objContact.ReadOnly)
			{
				chkFree.Enabled = chkGroup.Enabled =
				nudConnection.Enabled = nudLoyalty.Enabled = false;

				cmdDelete.Visible = false;
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

			SortListItem objContactSort = new SortListItem();
			lstCategories.Sort(objContactSort.Compare);
			cboContactRole.DataSource = lstCategories;
			cboContactRole.ValueMember = "Value";
			cboContactRole.DisplayMember = "Name";
			chkGroup.Checked = _objContact.IsGroup;
			chkFree.Checked = _objContact.Free;
			if (_objContact.MadeMan)
			{
				chkGroup.Checked = _objContact.MadeMan;
			}

			if (_strContactRole != "")
				cboContactRole.Text = _strContactRole;

			_loading = false;
		}

	    private void MoveControls()
	    {
		    lblConnection.Left = txtContactName.Left;
		    nudConnection.Left = lblConnection.Right + 2;
		    lblLoyalty.Left = nudConnection.Right + 2;
		    nudLoyalty.Left = lblLoyalty.Right + 2;
		    imgLink.Left = nudLoyalty.Right + 4; 
		    imgNotes.Left = imgLink.Right + 4;
		    chkGroup.Left = imgNotes.Right + 4;
		    chkFree.Left = chkGroup.Right + 2;
		    chkBlackmail.Left = chkFree.Right + 2;
		    chkFamily.Left = chkBlackmail.Right + 2;

	    }
		#endregion
		

        public void UpdateQuickText()
        {
            lblQuickStats.Text = String.Format("({0}/{1})", _objContact.Connection, _objContact.IsGroup ? (_objContact.MadeMan ? "M" : "G") : _objContact.Loyalty.ToString());

        }
	}
}