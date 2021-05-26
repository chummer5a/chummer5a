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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.XPath;
using Chummer.Properties;

namespace Chummer
{
    public partial class ContactControl : UserControl
    {
        private readonly Contact _objContact;
        private bool _blnLoading = true;
        private bool _blnStatBlockIsLoaded;
        //private readonly int _intLowHeight = 25;
        //private readonly int _intFullHeight = 156;

        // Events.
        public event TextEventHandler ContactDetailChanged;
        public event EventHandler DeleteContact;

        #region Control Events
        public ContactControl(Contact objContact)
        {
            _objContact = objContact ?? throw new ArgumentNullException(nameof(objContact));

            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            foreach (ToolStripItem tssItem in cmsContact.Items)
            {
                tssItem.UpdateLightDarkMode();
                tssItem.TranslateToolStripItemsRecursively();
            }
        }

        private void ContactControl_Load(object sender, EventArgs e)
        {
            if (this.IsNullOrDisposed())
                return;
            LoadContactList();

            DoDataBindings();

            if (_objContact.EntityType == ContactType.Enemy)
            {
                imgLink?.SetToolTip(!string.IsNullOrEmpty(_objContact.FileName)
                        ? LanguageManager.GetString("Tip_Enemy_OpenLinkedEnemy")
                        : LanguageManager.GetString("Tip_Enemy_LinkEnemy"));

                string strTooltip = LanguageManager.GetString("Tip_Enemy_EditNotes");
                if (!string.IsNullOrEmpty(_objContact.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objContact.Notes;
                imgNotes.SetToolTip(strTooltip.WordWrap());
            }
            else
            {
                imgLink?.SetToolTip(!string.IsNullOrEmpty(_objContact.FileName)
                        ? LanguageManager.GetString("Tip_Contact_OpenLinkedContact")
                        : LanguageManager.GetString("Tip_Contact_LinkContact"));

                string strTooltip = LanguageManager.GetString("Tip_Contact_EditNotes");
                if (!string.IsNullOrEmpty(_objContact.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objContact.Notes;
                imgNotes.SetToolTip(strTooltip.WordWrap());
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
            if (!_blnLoading && _blnStatBlockIsLoaded)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Connection"));
        }

        private void nudLoyalty_ValueChanged(object sender, EventArgs e)
        {
            // Raise the ContactDetailChanged Event when the NumericUpDown's Value changes.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            if (!_blnLoading && _blnStatBlockIsLoaded)
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
            if (!_blnLoading && _blnStatBlockIsLoaded)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Group"));
        }


        private void cmdExpand_Click(object sender, EventArgs e)
        {
            Expanded = !Expanded;
        }

        private void cboContactRole_TextChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Role"));
        }

        private void txtContactName_TextChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Name"));
        }

        private void txtContactLocation_TextChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Location"));
        }

        private void UpdateMetatype(object sender, EventArgs e)
        {
            if (_blnLoading || !_blnStatBlockIsLoaded || _objContact.DisplayMetatype == cboMetatype.Text)
                return;
            _objContact.DisplayMetatype = cboMetatype.Text;
            if (_objContact.DisplayMetatype != cboMetatype.Text)
            {
                _blnLoading = true;
                cboMetatype.Text = _objContact.DisplayMetatype;
                _blnLoading = false;
            }
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Metatype"));
        }

        private void UpdateGender(object sender, EventArgs e)
        {
            if (_blnLoading || !_blnStatBlockIsLoaded || _objContact.DisplayGender == cboGender.Text)
                return;
            _objContact.DisplayGender = cboGender.Text;
            if (_objContact.DisplayGender != cboGender.Text)
            {
                _blnLoading = true;
                cboGender.Text = _objContact.DisplayGender;
                _blnLoading = false;
            }
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Gender"));
        }

        private void UpdateAge(object sender, EventArgs e)
        {
            if (_blnLoading || !_blnStatBlockIsLoaded || _objContact.DisplayAge == cboAge.Text)
                return;
            _objContact.DisplayAge = cboAge.Text;
            if (_objContact.DisplayAge != cboAge.Text)
            {
                _blnLoading = true;
                cboAge.Text = _objContact.DisplayAge;
                _blnLoading = false;
            }
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Age"));
        }

        private void UpdatePersonalLife(object sender, EventArgs e)
        {
            if (_blnLoading || !_blnStatBlockIsLoaded || _objContact.DisplayPersonalLife == cboPersonalLife.Text)
                return;
            _objContact.DisplayPersonalLife = cboPersonalLife.Text;
            if (_objContact.DisplayPersonalLife != cboPersonalLife.Text)
            {
                _blnLoading = true;
                cboPersonalLife.Text = _objContact.DisplayPersonalLife;
                _blnLoading = false;
            }
            ContactDetailChanged?.Invoke(this, new TextEventArgs("PersonalLife"));
        }

        private void UpdateType(object sender, EventArgs e)
        {
            if (_blnLoading || !_blnStatBlockIsLoaded || _objContact.DisplayType == cboType.Text)
                return;
            _objContact.DisplayType = cboType.Text;
            if (_objContact.DisplayType != cboType.Text)
            {
                _blnLoading = true;
                cboType.Text = _objContact.DisplayType;
                _blnLoading = false;
            }
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Type"));
        }

        private void UpdatePreferredPayment(object sender, EventArgs e)
        {
            if (_blnLoading || !_blnStatBlockIsLoaded || _objContact.DisplayPreferredPayment == cboPreferredPayment.Text)
                return;
            _objContact.DisplayPreferredPayment = cboPreferredPayment.Text;
            if (_objContact.DisplayPreferredPayment != cboPreferredPayment.Text)
            {
                _blnLoading = true;
                cboPreferredPayment.Text = _objContact.DisplayPreferredPayment;
                _blnLoading = false;
            }
            ContactDetailChanged?.Invoke(this, new TextEventArgs("PreferredPayment"));
        }

        private void UpdateHobbiesVice(object sender, EventArgs e)
        {
            if (_blnLoading || !_blnStatBlockIsLoaded || _objContact.DisplayHobbiesVice == cboHobbiesVice.Text)
                return;
            _objContact.DisplayHobbiesVice = cboHobbiesVice.Text;
            if (_objContact.DisplayHobbiesVice != cboHobbiesVice.Text)
            {
                _blnLoading = true;
                cboHobbiesVice.Text = _objContact.DisplayHobbiesVice;
                _blnLoading = false;
            }
            ContactDetailChanged?.Invoke(this, new TextEventArgs("HobbiesVice"));
        }

        private void UpdateContactRole(object sender, EventArgs e)
        {
            if (_blnLoading || _objContact.DisplayRole == cboContactRole.Text)
                return;
            _objContact.DisplayRole = cboContactRole.Text;
            if (_objContact.DisplayRole != cboContactRole.Text)
            {
                _blnLoading = true;
                cboContactRole.Text = _objContact.DisplayRole;
                _blnLoading = false;
            }
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Role"));
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
            cmsContact.Show(imgLink, imgLink.Left - cmsContact.PreferredSize.Width, imgLink.Top);
        }

        private void tsContactOpen_Click(object sender, EventArgs e)
        {
            if (_objContact.LinkedCharacter != null)
            {
                Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x == _objContact.LinkedCharacter);
                using (new CursorWait(this))
                {
                    if (objOpenCharacter == null || !Program.MainForm.SwitchToOpenCharacter(objOpenCharacter, true))
                    {
                        objOpenCharacter = Program.MainForm.LoadCharacter(_objContact.LinkedCharacter.FileName);
                        Program.MainForm.OpenCharacter(objOpenCharacter);
                    }
                }
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
                Process.Start(strFile);
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
                imgLink?.SetToolTip(_objContact.EntityType == ContactType.Enemy
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
            if (Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_RemoveCharacterAssociation"), LanguageManager.GetString("MessageTitle_RemoveCharacterAssociation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _objContact.FileName = string.Empty;
                _objContact.RelativeFileName = string.Empty;
                imgLink?.SetToolTip(_objContact.EntityType == ContactType.Enemy
                        ? LanguageManager.GetString("Tip_Enemy_LinkFile")
                        : LanguageManager.GetString("Tip_Contact_LinkFile"));
                ContactDetailChanged?.Invoke(this, new TextEventArgs("File"));
            }
        }

        private void imgNotes_Click(object sender, EventArgs e)
        {
            using (frmNotes frmContactNotes = new frmNotes(_objContact.Notes, _objContact.NotesColor))
            {
                frmContactNotes.ShowDialog(this);
                if (frmContactNotes.DialogResult != DialogResult.OK)
                    return;
                _objContact.Notes = frmContactNotes.Notes;
            }

            string strTooltip = LanguageManager.GetString(_objContact.EntityType == ContactType.Enemy ? "Tip_Enemy_EditNotes" : "Tip_Contact_EditNotes");
            if (!string.IsNullOrEmpty(_objContact.Notes))
                strTooltip += Environment.NewLine + Environment.NewLine + _objContact.Notes;
            imgNotes.SetToolTip(strTooltip.WordWrap());
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Notes"));
        }

        private void chkFree_CheckedChanged(object sender, EventArgs e)
        {
            if (!_blnLoading && _blnStatBlockIsLoaded)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Free"));
        }

        private void chkBlackmail_CheckedChanged(object sender, EventArgs e)
        {
            if (!_blnLoading && _blnStatBlockIsLoaded)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Blackmail"));
        }

        private void chkFamily_CheckedChanged(object sender, EventArgs e)
        {
            if (!_blnLoading && _blnStatBlockIsLoaded)
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
            get => tlpStatBlock?.Visible == true;
            set
            {
                cmdExpand.Image = value ? Resources.Collapse : Resources.Expand;
                if (value && (tlpStatBlock == null || !_blnStatBlockIsLoaded))
                {
                    // Create second row and statblock only on the first expansion to save on handles and load times
                    CreateSecondRow();
                    CreateStatBlock();
                    _blnStatBlockIsLoaded = true;
                }
                using (new CursorWait(this))
                {
                    SuspendLayout();
                    lblConnection.Visible = value;
                    lblLoyalty.Visible = value;
                    nudConnection.Visible = value;
                    nudLoyalty.Visible = value;
                    chkGroup.Visible = value;
                    //We don't actually pay for contacts in play so everyone is free
                    //Don't present a useless field
                    chkFree.Visible = _objContact?.CharacterObject.Created == false && value;
                    chkBlackmail.Visible = value;
                    chkFamily.Visible = value;
                    imgLink.Visible = value;
                    tlpStatBlock.Visible = value;
                    ResumeLayout();
                }
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

            //the values are now loaded direct in the (new) property lstContactArchetypes (see above).
            //I only left this in here for better understanding what happend before (and because of bug #3566)
            //using (XmlNodeList xmlNodeList = xmlContactsBaseNode.SelectNodes("contacts/contact"))
            //    if (xmlNodeList != null)
            //        foreach (XmlNode xmlNode in xmlNodeList)
            //        {
            //            string strName = xmlNode.InnerText;
            //            ContactProfession.Add(new ListItem(strName, xmlNode.Attributes?["translate"]?.InnerText ?? strName));
            //        }

            cboContactRole.BeginUpdate();
            cboContactRole.PopulateWithListItems(Contact.ContactArchetypes(_objContact.CharacterObject));
            cboContactRole.SelectedValue = _objContact.Role;
            if (cboContactRole.SelectedIndex < 0)
                cboContactRole.Text = _objContact.DisplayRole;
            cboContactRole.EndUpdate();
        }

        private void DoDataBindings()
        {
            lblQuickStats.DoOneWayDataBinding("Text", _objContact, nameof(_objContact.QuickText));
            txtContactName.DoDatabinding("Text", _objContact, nameof(_objContact.Name));
            txtContactLocation.DoDatabinding("Text", _objContact, nameof(_objContact.Location));
            cmdDelete.DoDatabinding("Visible", _objContact, nameof(_objContact.NotReadOnly));
            this.DoOneWayDataBinding("BackColor", _objContact, nameof(_objContact.PreferredColor));

            // Properties controllable by the character themselves
            txtContactName.DoOneWayDataBinding("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter));
        }

        private Label lblConnection;
        private Label lblLoyalty;
        private NumericUpDownEx nudConnection;
        private NumericUpDownEx nudLoyalty;
        private ColorableCheckBox chkGroup;
        private ColorableCheckBox chkFree;
        private ColorableCheckBox chkBlackmail;
        private ColorableCheckBox chkFamily;
        private PictureBox imgLink;

        private void CreateSecondRow()
        {
            using (new CursorWait(this))
            {
                lblConnection = new Label
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
                    Name = "lblConnection",
                    Tag = "Label_Contact_Connection",
                    Text = "Connection:",
                    TextAlign = ContentAlignment.MiddleRight
                };
                nudConnection = new NumericUpDownEx
                {
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    AutoSize = true,
                    Maximum = new decimal(new[] { 12, 0, 0, 0 }),
                    Minimum = new decimal(new[] { 1, 0, 0, 0 }),
                    Name = "nudConnection",
                    Value = new decimal(new[] { 1, 0, 0, 0 })
                };
                lblLoyalty = new Label
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
                    Name = "lblLoyalty",
                    Tag = "Label_Contact_Loyalty",
                    Text = "Loyalty:",
                    TextAlign = ContentAlignment.MiddleRight
                };
                nudLoyalty = new NumericUpDownEx
                {
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    AutoSize = true,
                    Maximum = new decimal(new[] { 6, 0, 0, 0 }),
                    Minimum = new decimal(new[] { 1, 0, 0, 0 }),
                    Name = "nudLoyalty",
                    Value = new decimal(new[] { 1, 0, 0, 0 })
                };
                chkFree = new ColorableCheckBox(components)
                {
                    Anchor = AnchorStyles.Left,
                    AutoSize = true,
                    DefaultColorScheme = true,
                    Name = "chkFree",
                    Tag = "Checkbox_Contact_Free",
                    Text = "Free",
                    UseVisualStyleBackColor = true
                };
                chkGroup = new ColorableCheckBox(components)
                {
                    Anchor = AnchorStyles.Left,
                    AutoSize = true,
                    DefaultColorScheme = true,
                    Name = "chkGroup",
                    Tag = "Checkbox_Contact_Group",
                    Text = "Group",
                    UseVisualStyleBackColor = true
                };
                chkBlackmail = new ColorableCheckBox(components)
                {
                    Anchor = AnchorStyles.Left,
                    AutoSize = true,
                    DefaultColorScheme = true,
                    Name = "chkBlackmail",
                    Tag = "Checkbox_Contact_Blackmail",
                    Text = "Blackmail",
                    UseVisualStyleBackColor = true
                };
                chkFamily = new ColorableCheckBox(components)
                {
                    Anchor = AnchorStyles.Left,
                    AutoSize = true,
                    DefaultColorScheme = true,
                    Name = "chkFamily",
                    Tag = "Checkbox_Contact_Family",
                    Text = "Family",
                    UseVisualStyleBackColor = true
                };
                imgLink = new PictureBox
                {
                    Cursor = Cursors.Hand,
                    Dock = DockStyle.Fill,
                    Image = Resources.link,
                    Margin = new Padding(3, 0, 3, 0),
                    Size = new Size(16, 26),
                    Name = "imgLink",
                    SizeMode = PictureBoxSizeMode.Zoom,
                    TabStop = false
                };
                nudConnection.ValueChanged += nudConnection_ValueChanged;
                nudLoyalty.ValueChanged += nudLoyalty_ValueChanged;
                chkFree.CheckedChanged += chkFree_CheckedChanged;
                chkGroup.CheckedChanged += chkGroup_CheckedChanged;
                chkBlackmail.CheckedChanged += chkBlackmail_CheckedChanged;
                chkFamily.CheckedChanged += chkFamily_CheckedChanged;
                imgLink.Click += imgLink_Click;
                if (_objContact != null)
                {
                    chkGroup.DoDatabinding("Checked", _objContact, nameof(_objContact.IsGroup));
                    chkGroup.DoOneWayDataBinding("Enabled", _objContact, nameof(_objContact.GroupEnabled));
                    chkFree.DoDatabinding("Checked", _objContact, nameof(_objContact.Free));
                    chkFree.DoOneWayDataBinding("Enabled", _objContact, nameof(_objContact.FreeEnabled));
                    //We don't actually pay for contacts in play so everyone is free
                    //Don't present a useless field
                    chkFree.Visible = _objContact?.CharacterObject.Created == false;
                    chkFamily.DoDatabinding("Checked", _objContact, nameof(_objContact.Family));
                    chkFamily.DoDatabinding("Visible", _objContact, nameof(_objContact.IsNotEnemy));
                    chkBlackmail.DoDatabinding("Checked", _objContact, nameof(_objContact.Blackmail));
                    chkBlackmail.DoDatabinding("Visible", _objContact, nameof(_objContact.IsNotEnemy));
                    nudLoyalty.DoDatabinding("Value", _objContact, nameof(_objContact.Loyalty));
                    nudLoyalty.DoOneWayDataBinding("Enabled", _objContact, nameof(_objContact.LoyaltyEnabled));
                    nudConnection.DoDatabinding("Value", _objContact, nameof(_objContact.Connection));
                    nudConnection.DoOneWayDataBinding("Enabled", _objContact, nameof(_objContact.NotReadOnly));
                    nudConnection.DoOneWayDataBinding("Maximum", _objContact, nameof(_objContact.ConnectionMaximum));
                    if (_objContact.EntityType == ContactType.Enemy)
                    {
                        imgLink.SetToolTip(!string.IsNullOrEmpty(_objContact.FileName)
                            ? LanguageManager.GetString("Tip_Enemy_OpenLinkedEnemy")
                            : LanguageManager.GetString("Tip_Enemy_LinkEnemy"));
                    }
                    else
                    {
                        imgLink.SetToolTip(!string.IsNullOrEmpty(_objContact.FileName)
                            ? LanguageManager.GetString("Tip_Contact_OpenLinkedContact")
                            : LanguageManager.GetString("Tip_Contact_LinkContact"));
                    }
                }
                tlpMain.SetColumnSpan(lblConnection, 2);
                tlpMain.SetColumnSpan(chkFamily, 3);

                SuspendLayout();
                tlpMain.SuspendLayout();
                tlpMain.Controls.Add(lblConnection, 0, 2);
                tlpMain.Controls.Add(nudConnection, 2, 2);
                tlpMain.Controls.Add(lblLoyalty, 3, 2);
                tlpMain.Controls.Add(nudLoyalty, 4, 2);
                tlpMain.Controls.Add(chkFree, 6, 2);
                tlpMain.Controls.Add(chkGroup, 7, 2);
                tlpMain.Controls.Add(chkBlackmail, 8, 2);
                tlpMain.Controls.Add(chkFamily, 9, 2);
                tlpMain.Controls.Add(imgLink, 12, 2);
                tlpMain.ResumeLayout();
                ResumeLayout();
            }
        }

        private BufferedTableLayoutPanel tlpStatBlock;
        private Label lblHobbiesVice;
        private Label lblPreferredPayment;
        private Label lblPersonalLife;
        private Label lblType;
        private Label lblMetatype;
        private Label lblGender;
        private Label lblAge;
        private ElasticComboBox cboMetatype;
        private ElasticComboBox cboGender;
        private ElasticComboBox cboType;
        private ElasticComboBox cboAge;
        private ElasticComboBox cboPersonalLife;
        private ElasticComboBox cboPreferredPayment;
        private ElasticComboBox cboHobbiesVice;

        /// <summary>
        /// Method to dynamically create stat block is separated out so that we only create it if the control is expanded
        /// </summary>
        private void CreateStatBlock()
        {
            using (new CursorWait(this))
            {
                cboMetatype = new ElasticComboBox {Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboMetatype"};
                cboGender = new ElasticComboBox {Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboGender"};
                cboAge = new ElasticComboBox {Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboAge"};
                cboType = new ElasticComboBox {Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboType"};
                cboPersonalLife = new ElasticComboBox {Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboPersonalLife"};
                cboPreferredPayment = new ElasticComboBox {Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboPreferredPayment"};
                cboHobbiesVice = new ElasticComboBox {Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboHobbiesVice"};

                LoadStatBlockLists();

                if (_objContact != null)
                {
                    // Properties controllable by the character themselves
                    cboMetatype.DoOneWayDataBinding("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter));
                    cboGender.DoOneWayDataBinding("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter));
                    cboAge.DoOneWayDataBinding("Enabled", _objContact, nameof(_objContact.NoLinkedCharacter));
                }

                lblType = new Label
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
                    Margin = new Padding(3, 6, 3, 6),
                    Name = "lblType",
                    Tag = "Label_Type",
                    Text = "Type:",
                    TextAlign = ContentAlignment.MiddleRight
                };
                lblMetatype = new Label
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
                    Margin = new Padding(3, 6, 3, 6),
                    Name = "lblMetatype",
                    Tag = "Label_Metatype",
                    Text = "Metatype:",
                    TextAlign = ContentAlignment.MiddleRight
                };
                lblGender = new Label
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
                    Margin = new Padding(3, 6, 3, 6),
                    Name = "lblGender",
                    Tag = "Label_Gender",
                    Text = "Gender:",
                    TextAlign = ContentAlignment.MiddleRight
                };
                lblAge = new Label
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
                    Margin = new Padding(3, 6, 3, 6),
                    Name = "lblAge",
                    Tag = "Label_Age",
                    Text = "Age:",
                    TextAlign = ContentAlignment.MiddleRight
                };
                lblPersonalLife = new Label
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
                    Margin = new Padding(3, 6, 3, 6),
                    Name = "lblPersonalLife",
                    Tag = "Label_Contact_PersonalLife",
                    Text = "Personal Life:",
                    TextAlign = ContentAlignment.MiddleRight
                };
                lblPreferredPayment = new Label
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
                    Margin = new Padding(3, 6, 3, 6),
                    Name = "lblPreferredPayment",
                    Tag = "Label_Contact_PreferredPayment",
                    Text = "Preferred Payment:",
                    TextAlign = ContentAlignment.MiddleRight
                };
                lblHobbiesVice = new Label
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
                    Margin = new Padding(3, 6, 3, 6),
                    Name = "lblHobbiesVice",
                    Tag = "Label_Contact_HobbiesVice",
                    Text = "Hobbies/Vice:",
                    TextAlign = ContentAlignment.MiddleRight
                };

                tlpStatBlock = new BufferedTableLayoutPanel(components)
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ColumnCount = 4,
                    RowCount = 5,
                    Dock = DockStyle.Fill,
                    Name = "tlpStatBlock"
                };
                tlpStatBlock.ColumnStyles.Add(new ColumnStyle());
                tlpStatBlock.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                tlpStatBlock.ColumnStyles.Add(new ColumnStyle());
                tlpStatBlock.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                tlpStatBlock.RowStyles.Add(new RowStyle());
                tlpStatBlock.RowStyles.Add(new RowStyle());
                tlpStatBlock.RowStyles.Add(new RowStyle());
                tlpStatBlock.RowStyles.Add(new RowStyle());
                tlpStatBlock.Controls.Add(lblMetatype, 0, 0);
                tlpStatBlock.Controls.Add(cboMetatype, 1, 0);
                tlpStatBlock.Controls.Add(lblGender, 0, 1);
                tlpStatBlock.Controls.Add(cboGender, 1, 1);
                tlpStatBlock.Controls.Add(lblAge, 0, 2);
                tlpStatBlock.Controls.Add(cboAge, 1, 2);
                tlpStatBlock.Controls.Add(lblType, 0, 3);
                tlpStatBlock.Controls.Add(cboType, 1, 3);
                tlpStatBlock.Controls.Add(lblPersonalLife, 2, 0);
                tlpStatBlock.Controls.Add(cboPersonalLife, 3, 0);
                tlpStatBlock.Controls.Add(lblPreferredPayment, 2, 1);
                tlpStatBlock.Controls.Add(cboPreferredPayment, 3, 1);
                tlpStatBlock.Controls.Add(lblHobbiesVice, 2, 2);
                tlpStatBlock.Controls.Add(cboHobbiesVice, 3, 2);

                tlpStatBlock.TranslateWinForm();
                tlpStatBlock.UpdateLightDarkMode();

                SuspendLayout();
                tlpMain.SuspendLayout();
                tlpMain.SetColumnSpan(tlpStatBlock, 13);
                tlpMain.Controls.Add(tlpStatBlock, 0, 3);
                tlpMain.ResumeLayout();
                ResumeLayout();

                // Need these as separate instead of as simple data bindings so that we don't get annoying live partial translations

                if (_objContact != null)
                {
                    cboMetatype.SelectedValue = _objContact.Metatype;
                    cboGender.SelectedValue = _objContact.Gender;
                    cboAge.SelectedValue = _objContact.Age;
                    cboPersonalLife.SelectedValue = _objContact.PersonalLife;
                    cboType.SelectedValue = _objContact.Type;
                    cboPreferredPayment.SelectedValue = _objContact.PreferredPayment;
                    cboHobbiesVice.SelectedValue = _objContact.HobbiesVice;
                    if (cboMetatype.SelectedIndex < 0)
                        cboMetatype.Text = _objContact.DisplayMetatype;
                    if (cboGender.SelectedIndex < 0)
                        cboGender.Text = _objContact.DisplayGender;
                    if (cboAge.SelectedIndex < 0)
                        cboAge.Text = _objContact.DisplayAge;
                    if (cboPersonalLife.SelectedIndex < 0)
                        cboPersonalLife.Text = _objContact.DisplayPersonalLife;
                    if (cboType.SelectedIndex < 0)
                        cboType.Text = _objContact.DisplayType;
                    if (cboPreferredPayment.SelectedIndex < 0)
                        cboPreferredPayment.Text = _objContact.DisplayPreferredPayment;
                    if (cboHobbiesVice.SelectedIndex < 0)
                        cboHobbiesVice.Text = _objContact.DisplayHobbiesVice;
                }

                cboMetatype.SelectedIndexChanged += UpdateMetatype;
                cboGender.SelectedIndexChanged += UpdateGender;
                cboAge.SelectedIndexChanged += UpdateAge;
                cboType.SelectedIndexChanged += UpdateType;
                cboPersonalLife.SelectedIndexChanged += UpdatePersonalLife;
                cboPreferredPayment.SelectedIndexChanged += UpdatePreferredPayment;
                cboHobbiesVice.SelectedIndexChanged += UpdateHobbiesVice;
                cboMetatype.Leave += UpdateMetatype;
                cboGender.Leave += UpdateGender;
                cboAge.Leave += UpdateAge;
                cboType.Leave += UpdateType;
                cboPersonalLife.Leave += UpdatePersonalLife;
                cboPreferredPayment.Leave += UpdatePreferredPayment;
                cboHobbiesVice.Leave += UpdateHobbiesVice;
            }
        }

        private void LoadStatBlockLists()
        {
            // Read the list of Categories from the XML file.
            List<ListItem> lstMetatypes = new List<ListItem>(10)
            {
                ListItem.Blank
            };
            List<ListItem> lstGenders = new List<ListItem>(5)
            {
                ListItem.Blank
            };
            List<ListItem> lstAges = new List<ListItem>(5)
            {
                ListItem.Blank
            };
            List<ListItem> lstPersonalLives = new List<ListItem>(10)
            {
                ListItem.Blank
            };
            List<ListItem> lstTypes = new List<ListItem>(10)
            {
                ListItem.Blank
            };
            List<ListItem> lstPreferredPayments = new List<ListItem>(20)
            {
                ListItem.Blank
            };
            List<ListItem> lstHobbiesVices = new List<ListItem>(20)
            {
                ListItem.Blank
            };

            XPathNavigator xmlContactsBaseNode = _objContact.CharacterObject.LoadDataXPath("contacts.xml").SelectSingleNode("/chummer");
            if (xmlContactsBaseNode != null)
            {
                foreach (XPathNavigator xmlNode in xmlContactsBaseNode.Select("genders/gender"))
                {
                    string strName = xmlNode.Value;
                    lstGenders.Add(new ListItem(strName, xmlNode.SelectSingleNode("@translate")?.Value ?? strName));
                }

                foreach (XPathNavigator xmlNode in xmlContactsBaseNode.Select("ages/age"))
                {
                    string strName = xmlNode.Value;
                    lstAges.Add(new ListItem(strName, xmlNode.SelectSingleNode("@translate")?.Value ?? strName));
                }

                foreach (XPathNavigator xmlNode in xmlContactsBaseNode.Select("personallives/personallife"))
                {
                    string strName = xmlNode.Value;
                    lstPersonalLives.Add(new ListItem(strName, xmlNode.SelectSingleNode("@translate")?.Value ?? strName));
                }

                foreach (XPathNavigator xmlNode in xmlContactsBaseNode.Select("types/type"))
                {
                    string strName = xmlNode.Value;
                    lstTypes.Add(new ListItem(strName, xmlNode.SelectSingleNode("@translate")?.Value ?? strName));
                }

                foreach (XPathNavigator xmlNode in xmlContactsBaseNode.Select("preferredpayments/preferredpayment"))
                {
                    string strName = xmlNode.Value;
                    lstPreferredPayments.Add(new ListItem(strName, xmlNode.SelectSingleNode("@translate")?.Value ?? strName));
                }

                foreach (XPathNavigator xmlNode in xmlContactsBaseNode.Select("hobbiesvices/hobbyvice"))
                {
                    string strName = xmlNode.Value;
                    lstHobbiesVices.Add(new ListItem(strName, xmlNode.SelectSingleNode("@translate")?.Value ?? strName));
                }
            }

            string strSpace = LanguageManager.GetString("String_Space");
            foreach (XPathNavigator xmlMetatypeNode in _objContact.CharacterObject.LoadDataXPath("metatypes.xml").Select("/chummer/metatypes/metatype"))
            {
                string strName = xmlMetatypeNode.SelectSingleNode("name")?.Value;
                string strMetatypeDisplay = xmlMetatypeNode.SelectSingleNode("translate")?.Value ?? strName;
                lstMetatypes.Add(new ListItem(strName, strMetatypeDisplay));
                XPathNodeIterator xmlMetavariantsList = xmlMetatypeNode.Select("metavariants/metavariant");
                if (xmlMetavariantsList.Count > 0)
                {
                    string strMetavariantFormat = strMetatypeDisplay + strSpace + "({0})";
                    foreach (XPathNavigator objXmlMetavariantNode in xmlMetavariantsList)
                    {
                        string strMetavariantName = objXmlMetavariantNode.SelectSingleNode("name")?.Value ?? string.Empty;
                        if (lstMetatypes.All(x => strMetavariantName.Equals(x.Value.ToString(), StringComparison.OrdinalIgnoreCase)))
                            lstMetatypes.Add(new ListItem(strMetavariantName, string.Format(GlobalOptions.CultureInfo, strMetavariantFormat, objXmlMetavariantNode.SelectSingleNode("translate")?.Value ?? strMetavariantName)));
                    }
                }
            }

            lstMetatypes.Sort(CompareListItems.CompareNames);
            lstGenders.Sort(CompareListItems.CompareNames);
            lstAges.Sort(CompareListItems.CompareNames);
            lstPersonalLives.Sort(CompareListItems.CompareNames);
            lstTypes.Sort(CompareListItems.CompareNames);
            lstHobbiesVices.Sort(CompareListItems.CompareNames);
            lstPreferredPayments.Sort(CompareListItems.CompareNames);

            cboMetatype.BeginUpdate();
            cboMetatype.PopulateWithListItems(lstMetatypes);
            cboMetatype.EndUpdate();

            cboGender.BeginUpdate();
            cboGender.PopulateWithListItems(lstGenders);
            cboGender.EndUpdate();

            cboAge.BeginUpdate();
            cboAge.PopulateWithListItems(lstAges);
            cboAge.EndUpdate();

            cboPersonalLife.BeginUpdate();
            cboPersonalLife.PopulateWithListItems(lstPersonalLives);
            cboPersonalLife.EndUpdate();

            cboType.BeginUpdate();
            cboType.PopulateWithListItems(lstTypes);
            cboType.EndUpdate();

            cboPreferredPayment.BeginUpdate();
            cboPreferredPayment.PopulateWithListItems(lstPreferredPayments);
            cboPreferredPayment.EndUpdate();

            cboHobbiesVice.BeginUpdate();
            cboHobbiesVice.PopulateWithListItems(lstHobbiesVices);
            cboHobbiesVice.EndUpdate();
        }
        #endregion
    }
}
