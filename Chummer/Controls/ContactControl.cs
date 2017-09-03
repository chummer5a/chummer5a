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
 using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
 using System.Xml;

namespace Chummer
{
    public partial class ContactControl : UserControl
    {
        private Contact _objContact;
        private string _strContactRole;
        private bool _blnEnemy = false;
        private bool _loading = true;


        // Events.
        public Action<object> ConnectionRatingChanged;
        public Action<object> GroupStatusChanged;
        public Action<object> LoyaltyRatingChanged;
        public Action<object> FreeRatingChanged;
        public Action<object> DeleteContact;
        public Action<object> FileNameChanged;
        public Action<object> BlackmailChanged;
        public Action<object> FamilyChanged;

        #region Control Events
        public ContactControl(Character objCharacter)
        {
            InitializeComponent();

            //We don't actually pay for contacts in play so everyone is free
            //Don't present a useless field
            if (objCharacter.Created)
            {
                chkFree.Visible = false;
            }
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            MoveControls();
        }

        private void ContactControl_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            Width = cmdDelete.Left + cmdDelete.Width;
            LoadContactList();
        }

        private void nudConnection_ValueChanged(object sender, EventArgs e)
        {
            // Raise the ConnectionGroupRatingChanged Event when the NumericUpDown's Value changes.
            ConnectionRatingChanged(this);
        }

        private void nudLoyalty_ValueChanged(object sender, EventArgs e)
        {
            // Raise the LoyaltyRatingChanged Event when the NumericUpDown's Value changes.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            LoyaltyRatingChanged(this);
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

            chkGroup.Enabled = !_objContact.MadeMan;

            GroupStatusChanged?.Invoke(this);
        }

        
        private void cmdExpand_Click(object sender, EventArgs e)
        {
            bool blnExpanded = (Height > 22);
            if (blnExpanded)
            {
                Height -= 25;
                cmdExpand.Image = Properties.Resources.Expand;
            }
            else
            {
                Height += 25;
                cmdExpand.Image = Properties.Resources.Collapse;
            }
        }

        private void cboContactRole_TextChanged(object sender, EventArgs e)
        {
            ConnectionRatingChanged(this);
        }

        private void txtContactName_TextChanged(object sender, EventArgs e)
        {
            ConnectionRatingChanged(this);
        }

        private void txtContactLocation_TextChanged(object sender, EventArgs e)
        {
            ConnectionRatingChanged(this);
        }

        private void imgLink_Click(object sender, EventArgs e)
        {
            // Determine which options should be shown based on the FileName value.
            if (!string.IsNullOrEmpty(_objContact.FileName))
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
                if (string.IsNullOrEmpty(_objContact.RelativeFileName))
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

            if (openFileDialog.ShowDialog(this) != DialogResult.OK) return;
            _objContact.FileName = openFileDialog.FileName;
            tipTooltip.SetToolTip(imgLink,
                _objContact.EntityType == ContactType.Enemy
                    ? LanguageManager.Instance.GetString("Tip_Enemy_OpenFile")
                    : LanguageManager.Instance.GetString("Tip_Contact_OpenFile"));

            // Set the relative path.
            Uri uriApplication = new Uri(@Application.StartupPath);
            Uri uriFile = new Uri(@_objContact.FileName);
            Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
            _objContact.RelativeFileName = "../" + uriRelative;

            FileNameChanged(this);
        }

        private void tsRemoveCharacter_Click(object sender, EventArgs e)
        {
            // Remove the file association from the Contact.
            if (MessageBox.Show(LanguageManager.Instance.GetString("Message_RemoveCharacterAssociation"), LanguageManager.Instance.GetString("MessageTitle_RemoveCharacterAssociation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _objContact.FileName = string.Empty;
                _objContact.RelativeFileName = string.Empty;
                tipTooltip.SetToolTip(imgLink,
                    _objContact.EntityType == ContactType.Enemy
                        ? LanguageManager.Instance.GetString("Tip_Enemy_LinkFile")
                        : LanguageManager.Instance.GetString("Tip_Contact_LinkFile"));
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

            string strTooltip = LanguageManager.Instance.GetString(_objContact.EntityType == ContactType.Enemy ? "Tip_Enemy_EditNotes" : "Tip_Contact_EditNotes");
            if (!string.IsNullOrEmpty(_objContact.Notes))
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
                    tipTooltip.SetToolTip(imgLink,
                        !string.IsNullOrEmpty(_objContact.FileName)
                            ? LanguageManager.Instance.GetString("Tip_Enemy_OpenLinkedEnemy")
                            : LanguageManager.Instance.GetString("Tip_Enemy_LinkEnemy"));

                    string strTooltip = LanguageManager.Instance.GetString("Tip_Enemy_EditNotes");
                    if (!string.IsNullOrEmpty(_objContact.Notes))
                        strTooltip += "\n\n" + _objContact.Notes;
                    tipTooltip.SetToolTip(imgNotes, CommonFunctions.WordWrap(strTooltip, 100));
                    chkFamily.Visible = false;
                    chkBlackmail.Visible = false;
                    nudConnection.Minimum = 1;
                }
                else
                {
                    tipTooltip.SetToolTip(imgLink,
                        !string.IsNullOrEmpty(_objContact.FileName)
                            ? LanguageManager.Instance.GetString("Tip_Contact_OpenLinkedContact")
                            : LanguageManager.Instance.GetString("Tip_Contact_LinkContact"));

                    string strTooltip = LanguageManager.Instance.GetString("Tip_Contact_EditNotes");
                    if (!string.IsNullOrEmpty(_objContact.Notes))
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
                if (!string.IsNullOrEmpty(_strContactRole))
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
            objBlank.Value = string.Empty;
            objBlank.Name = string.Empty;
            lstCategories.Add(objBlank);

            XmlDocument objXmlDocument = XmlManager.Instance.Load("contacts.xml");
            XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/contacts/contact");
            if (objXmlSkillList != null)
                foreach (XmlNode objXmlCategory in objXmlSkillList)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlCategory.InnerText;
                    objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                    lstCategories.Add(objItem);
                }

            SortListItem objContactSort = new SortListItem();
            lstCategories.Sort(objContactSort.Compare);
            cboContactRole.BeginUpdate();
            cboContactRole.ValueMember = "Value";
            cboContactRole.DisplayMember = "Name";
            cboContactRole.DataSource = lstCategories;
            chkGroup.DataBindings.Add("Checked", _objContact, nameof(_objContact.IsGroup), false, 
                DataSourceUpdateMode.OnPropertyChanged);
            chkFree.DataBindings.Add("Checked", _objContact, nameof(_objContact.Free), false, 
                DataSourceUpdateMode.OnPropertyChanged);
            chkFamily.DataBindings.Add("Checked", _objContact, nameof(_objContact.Family), false, 
                DataSourceUpdateMode.OnPropertyChanged);
            chkBlackmail.DataBindings.Add("Checked", _objContact, nameof(_objContact.Blackmail), false, 
                DataSourceUpdateMode.OnPropertyChanged);
            lblQuickStats.DataBindings.Add("Text", _objContact, nameof(_objContact.QuickText), false, 
                DataSourceUpdateMode.OnPropertyChanged);
            nudLoyalty.DataBindings.Add("Value", _objContact, nameof(_objContact.Loyalty), false,
                DataSourceUpdateMode.OnPropertyChanged);
            nudLoyalty.DataBindings.Add("Enabled", _objContact, nameof(_objContact.LoyaltyEnabled), false,
                DataSourceUpdateMode.OnPropertyChanged);
            nudConnection.DataBindings.Add("Value", _objContact, nameof(_objContact.Connection), false,
                DataSourceUpdateMode.OnPropertyChanged);
            nudConnection.DataBindings.Add("Maximum", _objContact, nameof(_objContact.ConnectionMaximum), false,
                DataSourceUpdateMode.OnPropertyChanged);
            txtContactName.DataBindings.Add("Text", _objContact, nameof(_objContact.Name),false,
                DataSourceUpdateMode.OnPropertyChanged);
            txtContactLocation.DataBindings.Add("Text", _objContact, nameof(_objContact.Location), false, 
                DataSourceUpdateMode.OnPropertyChanged);
            cboContactRole.DataBindings.Add("Text", _objContact, nameof(_objContact.Role), false,
                DataSourceUpdateMode.OnPropertyChanged);
            cboContactRole.EndUpdate();

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
    }
}