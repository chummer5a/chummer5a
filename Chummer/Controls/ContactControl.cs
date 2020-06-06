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
using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

namespace Chummer
{
    public partial class ContactControl : UserControl
    {
        private readonly Contact _objContact;
        private bool _blnLoading = true;
        //private readonly int _intLowHeight = 25;
        //private readonly int _intFullHeight = 156;

        // Events.
        public event TextEventHandler ContactDetailChanged;
        public event EventHandler DeleteContact;

        #region Control Events
        public ContactControl(Contact objContact)
        {
            if (objContact == null)
                throw new ArgumentNullException(nameof(objContact));
            InitializeComponent();

            //We don't actually pay for contacts in play so everyone is free
            //Don't present a useless field
            if (objContact.CharacterObject.Created)
            {
                chkFree.Visible = false;
            }
            this.TranslateWinForm();
            MoveControls();

            _objContact = objContact;

            foreach (ToolStripItem tssItem in cmsContact.Items)
            {
                tssItem.TranslateToolStripItemsRecursively();
            }
        }

        private void ContactControl_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            Width = cmdDelete.Left + cmdDelete.Width;

            LoadContactList();

            DoDataBindings();

            if (_objContact.EntityType == ContactType.Enemy)
            {
                imgLink.SetToolTip(!string.IsNullOrEmpty(_objContact.FileName)
                        ? LanguageManager.GetString("Tip_Enemy_OpenLinkedEnemy")
                        : LanguageManager.GetString("Tip_Enemy_LinkEnemy"));

                string strTooltip = LanguageManager.GetString("Tip_Enemy_EditNotes");
                if (!string.IsNullOrEmpty(_objContact.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objContact.Notes;
                imgNotes.SetToolTip(strTooltip.WordWrap(100));
            }
            else
            {
                imgLink.SetToolTip(!string.IsNullOrEmpty(_objContact.FileName)
                        ? LanguageManager.GetString("Tip_Contact_OpenLinkedContact")
                        : LanguageManager.GetString("Tip_Contact_LinkContact"));

                string strTooltip = LanguageManager.GetString("Tip_Contact_EditNotes");
                if (!string.IsNullOrEmpty(_objContact.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objContact.Notes;
                imgNotes.SetToolTip(strTooltip.WordWrap(100));
            }

            _blnLoading = false;
        }

        public void UnbindContactControl()
        {
            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private void nudConnection_ValueChanged(object sender, EventArgs e)
        {
            // Raise the ContactDetailChanged Event when the NumericUpDown's Value changes.
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Connection"));
        }

        private void nudLoyalty_ValueChanged(object sender, EventArgs e)
        {
            // Raise the ContactDetailChanged Event when the NumericUpDown's Value changes.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Loyalty"));
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Raise the DeleteContact Event when the user has confirmed their desire to delete the Contact.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            DeleteContact?.Invoke(this, e);
        }

        private void chkGroup_CheckedChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Group"));
        }


        private void cmdExpand_Click(object sender, EventArgs e)
        {
            Expanded = !Expanded;
        }

        private void cboContactRole_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Role"));
        }

        private void txtContactName_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Name"));
        }

        private void txtContactLocation_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Location"));
        }

        private void cboMetatype_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Metatype"));
        }

        private void cboSex_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Sex"));
        }

        private void cboAge_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Age"));
        }

        private void cboPersonalLife_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("PersonalLife"));
        }

        private void cboType_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Type"));
        }

        private void cboPreferredPayment_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("PreferredPayment"));
        }

        private void cboHobbiesVice_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("HobbiesVice"));
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

        private async void tsContactOpen_Click(object sender, EventArgs e)
        {
            if (_objContact.LinkedCharacter != null)
            {
                Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x == _objContact.LinkedCharacter);
                Cursor = Cursors.WaitCursor;
                if (objOpenCharacter == null || !Program.MainForm.SwitchToOpenCharacter(objOpenCharacter, true))
                {
                    objOpenCharacter = await Program.MainForm.LoadCharacter(_objContact.LinkedCharacter.FileName).ConfigureAwait(true);
                    Program.MainForm.OpenCharacter(objOpenCharacter);
                }
                Cursor = Cursors.Default;
            }
            else
            {
                bool blnUseRelative = false;

                // Make sure the file still exists before attempting to load it.
                if (!File.Exists(_objContact.FileName))
                {
                    bool blnError = false;
                    // If the file doesn't exist, use the relative path if one is available.
                    if (string.IsNullOrEmpty(_objContact.RelativeFileName))
                        blnError = true;
                    else if (!File.Exists(Path.GetFullPath(_objContact.RelativeFileName)))
                        blnError = true;
                    else
                        blnUseRelative = true;

                    if (blnError)
                    {
                        Program.MainForm.ShowMessageBox(string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_FileNotFound"), _objContact.FileName), LanguageManager.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                string strFile = blnUseRelative ? Path.GetFullPath(_objContact.RelativeFileName) : _objContact.FileName;
                System.Diagnostics.Process.Start(strFile);
            }
        }

        private void tsAttachCharacter_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Chum5") + '|' + LanguageManager.GetString("DialogFilter_All")
            })
            {
                if (!string.IsNullOrEmpty(_objContact.FileName) && File.Exists(_objContact.FileName))
                {
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(_objContact.FileName);
                    openFileDialog.FileName = Path.GetFileName(_objContact.FileName);
                }

                if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                    return;
                _objContact.FileName = openFileDialog.FileName;
                imgLink.SetToolTip(_objContact.EntityType == ContactType.Enemy
                    ? LanguageManager.GetString("Tip_Enemy_OpenFile")
                    : LanguageManager.GetString("Tip_Contact_OpenFile"));
            }

            // Set the relative path.
            Uri uriApplication = new Uri(Utils.GetStartupPath);
            Uri uriFile = new Uri(_objContact.FileName);
            Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
            _objContact.RelativeFileName = "../" + uriRelative;

            ContactDetailChanged?.Invoke(this, new TextEventArgs("File"));
        }

        private void tsRemoveCharacter_Click(object sender, EventArgs e)
        {
            // Remove the file association from the Contact.
            if (MessageBox.Show(LanguageManager.GetString("Message_RemoveCharacterAssociation"), LanguageManager.GetString("MessageTitle_RemoveCharacterAssociation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _objContact.FileName = string.Empty;
                _objContact.RelativeFileName = string.Empty;
                imgLink.SetToolTip(_objContact.EntityType == ContactType.Enemy
                        ? LanguageManager.GetString("Tip_Enemy_LinkFile")
                        : LanguageManager.GetString("Tip_Contact_LinkFile"));
                ContactDetailChanged?.Invoke(this, new TextEventArgs("File"));
            }
        }

        private void imgNotes_Click(object sender, EventArgs e)
        {
            string strOldValue = _objContact.Notes;
            using (frmNotes frmContactNotes = new frmNotes())
            {
                if (strOldValue.ContainsHtmlTags())
                    frmContactNotes.HtmlNotes = strOldValue;
                else
                    frmContactNotes.Notes = strOldValue;
                frmContactNotes.ShowDialog(this);
                if (frmContactNotes.DialogResult != DialogResult.OK)
                    return;
                frmContactNotes.ShowDialog(this);

                _objContact.Notes = frmContactNotes.HtmlNotes;
                if (strOldValue == _objContact.Notes)
                    return;
            }

            string strTooltip = LanguageManager.GetString(_objContact.EntityType == ContactType.Enemy ? "Tip_Enemy_EditNotes" : "Tip_Contact_EditNotes");
            if (!string.IsNullOrEmpty(_objContact.Notes))
                strTooltip += Environment.NewLine + Environment.NewLine + _objContact.Notes;
            imgNotes.SetToolTip(strTooltip.WordWrap(100));
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Notes"));
        }

        private void chkFree_CheckedChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Free"));
        }

        private void chkBlackmail_CheckedChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Blackmail"));
        }

        private void chkFamily_CheckedChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Family"));
        }
        #endregion

        #region Properties
        /// <summary>
        /// Contact object this is linked to.
        /// </summary>
        public Contact ContactObject => _objContact;

        public bool Expanded
        {
            get => tlpStatBlock.Visible;
            set
            {
                tlpStatHeader.Visible = tlpStatBlock.Visible = value;
                cmdExpand.Image = value ? Properties.Resources.Collapse : cmdExpand.Image = Properties.Resources.Expand;
            }
        }

        private static List<ListItem> _lstContactArchetypes;

        public static List<ListItem> ContactArchetypes
        {
            get
            {
                if (_lstContactArchetypes != null)
                    return _lstContactArchetypes;
                _lstContactArchetypes = new List<ListItem>{ListItem.Blank};
                XmlNode xmlContactsBaseNode = XmlManager.Load("contacts.xml").SelectSingleNode("/chummer");
                if (xmlContactsBaseNode == null)
                    return _lstContactArchetypes;
                using (XmlNodeList xmlNodeList = xmlContactsBaseNode.SelectNodes("contacts/contact"))
                {
                    if (xmlNodeList == null)
                        return _lstContactArchetypes;
                    foreach (XmlNode xmlNode in xmlNodeList)
                    {
                        string strName = xmlNode.InnerText;
                        _lstContactArchetypes.Add(new ListItem(strName,
                            xmlNode.Attributes?["translate"]?.InnerText ?? strName));
                    }
                }

                return _lstContactArchetypes;
            }
        }
        #endregion

        #region Methods
        private void LoadContactList()
        {
            if (_objContact.EntityType == ContactType.Enemy)
            {
                string strContactRole = _objContact.DisplayRole;
                if (!string.IsNullOrEmpty(strContactRole))
                    cboContactRole.Text = strContactRole;
                return;
            }

            // Read the list of Categories from the XML file.
            List<ListItem> lstMetatypes = new List<ListItem>
            {
                ListItem.Blank
            };
            List<ListItem> lstSexes = new List<ListItem>
            {
                ListItem.Blank
            };
            List<ListItem> lstAges = new List<ListItem>
            {
                ListItem.Blank
            };
            List<ListItem> lstPersonalLives = new List<ListItem>
            {
                ListItem.Blank
            };
            List<ListItem> lstTypes = new List<ListItem>
            {
                ListItem.Blank
            };
            List<ListItem> lstPreferredPayments = new List<ListItem>
            {
                ListItem.Blank
            };
            List<ListItem> lstHobbiesVices = new List<ListItem>
            {
                ListItem.Blank
            };

            XmlNode xmlContactsBaseNode = XmlManager.Load("contacts.xml").SelectSingleNode("/chummer");
            if (xmlContactsBaseNode != null)
            {
                //the values are now loaded direct in the (new) property lstContactArchetypes (see above).
                //I only left this in here for better understanding what happend before (and because of bug #3566)
                //using (XmlNodeList xmlNodeList = xmlContactsBaseNode.SelectNodes("contacts/contact"))
                //    if (xmlNodeList != null)
                //        foreach (XmlNode xmlNode in xmlNodeList)
                //        {
                //            string strName = xmlNode.InnerText;
                //            ContactProfession.Add(new ListItem(strName, xmlNode.Attributes?["translate"]?.InnerText ?? strName));
                //        }

                using (XmlNodeList xmlNodeList = xmlContactsBaseNode.SelectNodes("sexes/sex"))
                    if (xmlNodeList != null)
                        foreach (XmlNode xmlNode in xmlNodeList)
                        {
                            string strName = xmlNode.InnerText;
                            lstSexes.Add(new ListItem(strName, xmlNode.Attributes?["translate"]?.InnerText ?? strName));
                        }

                using (XmlNodeList xmlNodeList = xmlContactsBaseNode.SelectNodes("ages/age"))
                    if (xmlNodeList != null)
                        foreach (XmlNode xmlNode in xmlNodeList)
                        {
                            string strName = xmlNode.InnerText;
                            lstAges.Add(new ListItem(strName, xmlNode.Attributes?["translate"]?.InnerText ?? strName));
                        }

                using (XmlNodeList xmlNodeList = xmlContactsBaseNode.SelectNodes("personallives/personallife"))
                    if (xmlNodeList != null)
                        foreach (XmlNode xmlNode in xmlNodeList)
                        {
                            string strName = xmlNode.InnerText;
                            lstPersonalLives.Add(new ListItem(strName, xmlNode.Attributes?["translate"]?.InnerText ?? strName));
                        }

                using (XmlNodeList xmlNodeList = xmlContactsBaseNode.SelectNodes("types/type"))
                    if (xmlNodeList != null)
                        foreach (XmlNode xmlNode in xmlNodeList)
                        {
                            string strName = xmlNode.InnerText;
                            lstTypes.Add(new ListItem(strName, xmlNode.Attributes?["translate"]?.InnerText ?? strName));
                        }

                using (XmlNodeList xmlNodeList = xmlContactsBaseNode.SelectNodes("preferredpayments/preferredpayment"))
                    if (xmlNodeList != null)
                        foreach (XmlNode xmlNode in xmlNodeList)
                        {
                            string strName = xmlNode.InnerText;
                            lstPreferredPayments.Add(new ListItem(strName, xmlNode.Attributes?["translate"]?.InnerText ?? strName));
                        }

                using (XmlNodeList xmlNodeList = xmlContactsBaseNode.SelectNodes("hobbiesvices/hobbyvice"))
                    if (xmlNodeList != null)
                        foreach (XmlNode xmlNode in xmlNodeList)
                        {
                            string strName = xmlNode.InnerText;
                            lstHobbiesVices.Add(new ListItem(strName, xmlNode.Attributes?["translate"]?.InnerText ?? strName));
                        }
            }

            string strSpace = LanguageManager.GetString("String_Space");
            using (XmlNodeList xmlMetatypeList = XmlManager.Load("metatypes.xml").SelectNodes("/chummer/metatypes/metatype"))
                if (xmlMetatypeList != null)
                    foreach (XmlNode xmlMetatypeNode in xmlMetatypeList)
                    {
                        string strName = xmlMetatypeNode["name"]?.InnerText;
                        string strMetatypeDisplay = xmlMetatypeNode["translate"]?.InnerText ?? strName;
                        lstMetatypes.Add(new ListItem(strName, strMetatypeDisplay));
                        XmlNodeList xmlMetavariantList = xmlMetatypeNode.SelectNodes("metavariants/metavariant");
                        if (xmlMetavariantList != null)
                        {
                            foreach (XmlNode objXmlMetavariantNode in xmlMetavariantList)
                            {
                                string strMetavariantName = objXmlMetavariantNode["name"]?.InnerText;
                                if (lstMetatypes.All(x => x.Value != null && x.Value.ToString() != strMetavariantName))
                                    lstMetatypes.Add(new ListItem(strMetavariantName, strMetatypeDisplay + strSpace + '(' + (objXmlMetavariantNode["translate"]?.InnerText ?? strMetavariantName) + ')'));
                            }
                        }
                    }

            ContactArchetypes.Sort(CompareListItems.CompareNames);
            lstMetatypes.Sort(CompareListItems.CompareNames);
            lstSexes.Sort(CompareListItems.CompareNames);
            lstAges.Sort(CompareListItems.CompareNames);
            lstPersonalLives.Sort(CompareListItems.CompareNames);
            lstTypes.Sort(CompareListItems.CompareNames);
            lstHobbiesVices.Sort(CompareListItems.CompareNames);
            lstPreferredPayments.Sort(CompareListItems.CompareNames);

            cboContactRole.BeginUpdate();
            cboContactRole.ValueMember = "Value";
            cboContactRole.DisplayMember = "Name";
            cboContactRole.DataSource = new BindingSource { DataSource = ContactArchetypes };
            cboContactRole.EndUpdate();

            cboMetatype.BeginUpdate();
            cboMetatype.ValueMember = "Value";
            cboMetatype.DisplayMember = "Name";
            cboMetatype.DataSource = lstMetatypes;
            cboMetatype.EndUpdate();

            cboSex.BeginUpdate();
            cboSex.ValueMember = "Value";
            cboSex.DisplayMember = "Name";
            cboSex.DataSource = lstSexes;
            cboSex.EndUpdate();

            cboAge.BeginUpdate();
            cboAge.ValueMember = "Value";
            cboAge.DisplayMember = "Name";
            cboAge.DataSource = lstAges;
            cboAge.EndUpdate();

            cboPersonalLife.BeginUpdate();
            cboPersonalLife.ValueMember = "Value";
            cboPersonalLife.DisplayMember = "Name";
            cboPersonalLife.DataSource = lstPersonalLives;
            cboPersonalLife.EndUpdate();

            cboType.BeginUpdate();
            cboType.ValueMember = "Value";
            cboType.DisplayMember = "Name";
            cboType.DataSource = lstTypes;
            cboType.EndUpdate();

            cboPreferredPayment.BeginUpdate();
            cboPreferredPayment.ValueMember = "Value";
            cboPreferredPayment.DisplayMember = "Name";
            cboPreferredPayment.DataSource = lstPreferredPayments;
            cboPreferredPayment.EndUpdate();

            cboHobbiesVice.BeginUpdate();
            cboHobbiesVice.ValueMember = "Value";
            cboHobbiesVice.DisplayMember = "Name";
            cboHobbiesVice.DataSource = lstHobbiesVices;
            cboHobbiesVice.EndUpdate();
        }

        private void DoDataBindings()
        {
            chkGroup.DoDatabinding("Checked", _objContact, nameof(_objContact.IsGroup));
            chkGroup.DoDatabinding("Enabled", _objContact, nameof(_objContact.GroupEnabled));
            chkFree.DoDatabinding("Checked", _objContact, nameof(_objContact.Free));
            chkFree.DoDatabinding("Enabled", _objContact, nameof(_objContact.FreeEnabled));
            chkFamily.DoDatabinding("Checked", _objContact, nameof(_objContact.Family));
            chkFamily.DoDatabinding("Visible", _objContact, nameof(_objContact.IsNotEnemy));
            chkBlackmail.DoDatabinding("Checked", _objContact, nameof(_objContact.Blackmail));
            chkBlackmail.DoDatabinding("Visible", _objContact, nameof(_objContact.IsNotEnemy));
            lblQuickStats.DoDatabinding("Text", _objContact, nameof(_objContact.QuickText));
            nudLoyalty.DoDatabinding("Value", _objContact, nameof(_objContact.Loyalty));
            nudLoyalty.DoDatabinding("Enabled", _objContact, nameof(_objContact.LoyaltyEnabled));
            nudConnection.DoDatabinding("Value", _objContact, nameof(_objContact.Connection));
            nudConnection.DoDatabinding("Enabled", _objContact, nameof(_objContact.NotReadOnly));
            nudConnection.DoDatabinding("Maximum", _objContact, nameof(_objContact.ConnectionMaximum));
            txtContactName.DoDatabinding("Text", _objContact, nameof(_objContact.Name));
            txtContactLocation.DoDatabinding("Text", _objContact, nameof(_objContact.Location));
            cboContactRole.DoDatabinding("Text", _objContact, nameof(_objContact.DisplayRole));
            cboMetatype.DoDatabinding("Text", _objContact, nameof(_objContact.DisplayMetatype));
            cboSex.DoDatabinding("Text", _objContact, nameof(_objContact.DisplaySex));
            cboAge.DoDatabinding("Text", _objContact, nameof(_objContact.DisplayAge));
            cboPersonalLife.DoDatabinding("Text", _objContact, nameof(_objContact.DisplayPersonalLife));
            cboType.DoDatabinding("Text", _objContact, nameof(_objContact.DisplayType));
            cboPreferredPayment.DoDatabinding("Text", _objContact, nameof(_objContact.DisplayPreferredPayment));
            cboHobbiesVice.DoDatabinding("Text", _objContact, nameof(_objContact.DisplayHobbiesVice));
            cmdDelete.DoDatabinding("Visible", _objContact, nameof(_objContact.NotReadOnly));
            this.DoDatabinding("BackColor", _objContact, nameof(_objContact.PreferredColor));

            // Properties controllable by the character themselves
            txtContactName.DoDatabinding("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter));
            cboMetatype.DoDatabinding("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter));
            cboSex.DoDatabinding("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter));
            cboAge.DoDatabinding("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter));
        }

        private void MoveControls()
        {
            // no need to do anything here using TableLayoutPanels
            //lblConnection.Left = txtContactName.Left;
            //nudConnection.Left = lblConnection.Right + 2;
            //lblLoyalty.Left = nudConnection.Right + 2;
            //nudLoyalty.Left = lblLoyalty.Right + 2;
            //imgLink.Left = nudLoyalty.Right + 4;
            //imgNotes.Left = imgLink.Right + 4;
            //chkGroup.Left = imgNotes.Right + 4;
            //chkFree.Left = chkGroup.Right + 2;
            //chkBlackmail.Left = chkFree.Right + 2;
            //chkFamily.Left = chkBlackmail.Right + 2;

            //lblmetatype.left = cbometatype.left - 7 - lblmetatype.width;
            //lblage.left = cboage.left - 7 - lblage.width;
            //lblsex.left = cbosex.left - 7 - lblsex.width;
            //lbltype.left = cbotype.left - 7 - lbltype.width;

            //lblpersonallife.left = cbopersonallife.left - 7 - lblpersonallife.width;
            //lblpreferredpayment.left = cbopreferredpayment.left - 7 - lblpreferredpayment.width;
            //lblhobbiesvice.left = cbohobbiesvice.left - 7 - lblhobbiesvice.width;
        }
        #endregion

    }
}
