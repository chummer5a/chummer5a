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
using System.Threading.Tasks;
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
        public event EventHandler<TextEventArgs> ContactDetailChanged;

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

        private async void ContactControl_Load(object sender, EventArgs e)
        {
            if (this.IsNullOrDisposed())
                return;
            await LoadContactList();

            DoDataBindings();

            if (_objContact.IsEnemy)
            {
                if (cmdLink != null)
                {
                    string strText = !string.IsNullOrEmpty(_objContact.FileName)
                        ? await LanguageManager.GetStringAsync("Tip_Enemy_OpenLinkedEnemy")
                        : await LanguageManager.GetStringAsync("Tip_Enemy_LinkEnemy");
                    await cmdLink.SetToolTipTextAsync(strText);
                }

                string strTooltip = await LanguageManager.GetStringAsync("Tip_Enemy_EditNotes");
                if (!string.IsNullOrEmpty(_objContact.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objContact.Notes;
                await cmdNotes.SetToolTipTextAsync(strTooltip.WordWrap());
            }
            else
            {
                if (cmdLink != null)
                {
                    string strText = !string.IsNullOrEmpty(_objContact.FileName)
                        ? await LanguageManager.GetStringAsync("Tip_Contact_OpenLinkedContact")
                        : await LanguageManager.GetStringAsync("Tip_Contact_LinkContact");
                    await cmdLink.SetToolTipTextAsync(strText);
                }

                string strTooltip = await LanguageManager.GetStringAsync("Tip_Contact_EditNotes");
                if (!string.IsNullOrEmpty(_objContact.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objContact.Notes;
                await cmdNotes.SetToolTipTextAsync(strTooltip.WordWrap());
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

        private void cmdLink_Click(object sender, EventArgs e)
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
            cmsContact.Show(cmdLink, cmdLink.Left - cmsContact.PreferredSize.Width, cmdLink.Top);
        }

        private async void tsContactOpen_Click(object sender, EventArgs e)
        {
            if (_objContact.LinkedCharacter != null)
            {
                Character objOpenCharacter = await Program.OpenCharacters.ContainsAsync(_objContact.LinkedCharacter)
                    ? _objContact.LinkedCharacter
                    : null;
                using (await CursorWait.NewAsync(ParentForm))
                {
                    if (objOpenCharacter == null)
                    {
                        using (ThreadSafeForm<LoadingBar> frmLoadingBar = await Program.CreateAndShowProgressBarAsync(_objContact.LinkedCharacter.FileName, Character.NumLoadingSections))
                            objOpenCharacter = await Program.LoadCharacterAsync(_objContact.LinkedCharacter.FileName, frmLoadingBar: frmLoadingBar.MyForm);
                    }
                    if (!await Program.SwitchToOpenCharacter(objOpenCharacter))
                        await Program.OpenCharacter(objOpenCharacter);
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
                        Program.ShowMessageBox(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_FileNotFound"), _objContact.FileName), await LanguageManager.GetStringAsync("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                string strFile = blnUseRelative ? Path.GetFullPath(_objContact.RelativeFileName) : _objContact.FileName;
                Process.Start(strFile);
            }
        }

        private async void tsAttachCharacter_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (OpenFileDialog dlgOpenFile = await this.DoThreadSafeFuncAsync(() => new OpenFileDialog()))
            {
                dlgOpenFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Chum5") + '|'
                    + await LanguageManager.GetStringAsync("DialogFilter_All");
                if (!string.IsNullOrEmpty(_objContact.FileName) && File.Exists(_objContact.FileName))
                {
                    dlgOpenFile.InitialDirectory = Path.GetDirectoryName(_objContact.FileName);
                    dlgOpenFile.FileName = Path.GetFileName(_objContact.FileName);
                }

                if (await this.DoThreadSafeFuncAsync(x => dlgOpenFile.ShowDialog(x)) != DialogResult.OK)
                    return;
                _objContact.FileName = dlgOpenFile.FileName;
                if (cmdLink != null)
                {
                    string strText = _objContact.IsEnemy
                        ? await LanguageManager.GetStringAsync("Tip_Enemy_OpenFile")
                        : await LanguageManager.GetStringAsync("Tip_Contact_OpenFile");
                    await cmdLink.SetToolTipTextAsync(strText);
                }
            }

            // Set the relative path.
            Uri uriApplication = new Uri(Utils.GetStartupPath);
            Uri uriFile = new Uri(_objContact.FileName);
            Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
            _objContact.RelativeFileName = "../" + uriRelative;

            ContactDetailChanged?.Invoke(this, new TextEventArgs("File"));
        }

        private async void tsRemoveCharacter_Click(object sender, EventArgs e)
        {
            // Remove the file association from the Contact.
            if (Program.ShowMessageBox(await LanguageManager.GetStringAsync("Message_RemoveCharacterAssociation"), await LanguageManager.GetStringAsync("MessageTitle_RemoveCharacterAssociation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _objContact.FileName = string.Empty;
                _objContact.RelativeFileName = string.Empty;
                if (cmdLink != null)
                {
                    string strText = _objContact.IsEnemy
                        ? await LanguageManager.GetStringAsync("Tip_Enemy_LinkFile")
                        : await LanguageManager.GetStringAsync("Tip_Contact_LinkFile");
                    await cmdLink.SetToolTipTextAsync(strText);
                }
                ContactDetailChanged?.Invoke(this, new TextEventArgs("File"));
            }
        }

        private async void cmdNotes_Click(object sender, EventArgs e)
        {
            using (ThreadSafeForm<EditNotes> frmContactNotes = await ThreadSafeForm<EditNotes>.GetAsync(() => new EditNotes(_objContact.Notes, _objContact.NotesColor)))
            {
                if (await frmContactNotes.ShowDialogSafeAsync(this) != DialogResult.OK)
                    return;
                _objContact.Notes = frmContactNotes.MyForm.Notes;
            }

            string strTooltip = await LanguageManager.GetStringAsync(_objContact.IsEnemy ? "Tip_Enemy_EditNotes" : "Tip_Contact_EditNotes");
            if (!string.IsNullOrEmpty(_objContact.Notes))
                strTooltip += Environment.NewLine + Environment.NewLine + _objContact.Notes;
            await cmdNotes.SetToolTipTextAsync(strTooltip.WordWrap());
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

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Contact object this is linked to.
        /// </summary>
        public Contact ContactObject => _objContact;

        public bool Expanded
        {
            get => tlpStatBlock?.DoThreadSafeFunc(x => x.Visible) == true;
            set
            {
                cmdExpand.DoThreadSafe(x =>
                {
                    x.ImageDpi96 = value ? Resources.toggle : Resources.toggle_expand;
                    x.ImageDpi192 = value ? Resources.toggle1 : Resources.toggle_expand1;
                });
                if (value && (tlpStatBlock == null || !_blnStatBlockIsLoaded))
                {
                    // Create second row and statblock only on the first expansion to save on handles and load times
                    CreateSecondRow();
                    CreateStatBlock();
                    _blnStatBlockIsLoaded = true;
                }
                using (CursorWait.New(this))
                {
                    this.DoThreadSafe(x => x.SuspendLayout());
                    try
                    {
                        lblConnection.DoThreadSafe(x => x.Visible = value);
                        lblLoyalty.DoThreadSafe(x => x.Visible = value);
                        nudConnection.DoThreadSafe(x => x.Visible = value);
                        nudLoyalty.DoThreadSafe(x => x.Visible = value);
                        chkGroup.DoThreadSafe(x => x.Visible = value);
                        //We don't actually pay for contacts in play so everyone is free
                        //Don't present a useless field
                        chkFree.DoThreadSafe(x => x.Visible = _objContact?.CharacterObject.Created == false && value);
                        chkBlackmail.DoThreadSafe(x => x.Visible = value);
                        chkFamily.DoThreadSafe(x => x.Visible = value);
                        cmdLink.DoThreadSafe(x => x.Visible = value);
                        tlpStatBlock.DoThreadSafe(x => x.Visible = value);
                    }
                    finally
                    {
                        this.DoThreadSafe(x => x.ResumeLayout());
                    }
                }
            }
        }

        #endregion Properties

        #region Methods

        private async ValueTask LoadContactList()
        {
            if (_objContact.IsEnemy)
            {
                string strContactRole = _objContact.DisplayRole;
                if (!string.IsNullOrEmpty(strContactRole))
                    await cboContactRole.DoThreadSafeAsync(x => x.Text = strContactRole);
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
            
            await cboContactRole.PopulateWithListItemsAsync(Contact.ContactArchetypes(_objContact.CharacterObject));
            await cboContactRole.DoThreadSafeAsync(x =>
            {
                x.SelectedValue = _objContact.Role;
                if (x.SelectedIndex < 0)
                    x.Text = _objContact.DisplayRole;
            });
        }

        private void DoDataBindings()
        {
            lblQuickStats.DoOneWayDataBinding("Text", _objContact, nameof(_objContact.QuickText));
            txtContactName.DoDataBinding("Text", _objContact, nameof(_objContact.Name));
            txtContactLocation.DoDataBinding("Text", _objContact, nameof(_objContact.Location));
            cmdDelete.DoOneWayDataBinding("Visible", _objContact, nameof(_objContact.NotReadOnly));
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
        private ButtonWithToolTip cmdLink;

        private void CreateSecondRow()
        {
            using (CursorWait.New(this))
            {
                this.DoThreadSafe(x =>
                {
                    x.lblConnection = new Label
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        Name = "lblConnection",
                        Tag = "Label_Contact_Connection",
                        Text = "Connection:",
                        TextAlign = ContentAlignment.MiddleRight
                    };
                    x.nudConnection = new NumericUpDownEx
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        AutoSize = true,
                        Maximum = new decimal(new[] {12, 0, 0, 0}),
                        Minimum = new decimal(new[] {1, 0, 0, 0}),
                        Name = "nudConnection",
                        Value = new decimal(new[] {1, 0, 0, 0})
                    };
                    x.lblLoyalty = new Label
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        Name = "lblLoyalty",
                        Tag = "Label_Contact_Loyalty",
                        Text = "Loyalty:",
                        TextAlign = ContentAlignment.MiddleRight
                    };
                    x.nudLoyalty = new NumericUpDownEx
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        AutoSize = true,
                        Maximum = new decimal(new[] {6, 0, 0, 0}),
                        Minimum = new decimal(new[] {1, 0, 0, 0}),
                        Name = "nudLoyalty",
                        Value = new decimal(new[] {1, 0, 0, 0})
                    };
                    x.chkFree = new ColorableCheckBox(x.components)
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        DefaultColorScheme = true,
                        Name = "chkFree",
                        Tag = "Checkbox_Contact_Free",
                        Text = "Free",
                        UseVisualStyleBackColor = true
                    };
                    x.chkGroup = new ColorableCheckBox(x.components)
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        DefaultColorScheme = true,
                        Name = "chkGroup",
                        Tag = "Checkbox_Contact_Group",
                        Text = "Group",
                        UseVisualStyleBackColor = true
                    };
                    x.chkBlackmail = new ColorableCheckBox(x.components)
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        DefaultColorScheme = true,
                        Name = "chkBlackmail",
                        Tag = "Checkbox_Contact_Blackmail",
                        Text = "Blackmail",
                        UseVisualStyleBackColor = true
                    };
                    x.chkFamily = new ColorableCheckBox(x.components)
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        DefaultColorScheme = true,
                        Name = "chkFamily",
                        Tag = "Checkbox_Contact_Family",
                        Text = "Family",
                        UseVisualStyleBackColor = true
                    };
                    x.cmdLink = new ButtonWithToolTip(x.components)
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        FlatAppearance = {BorderSize = 0},
                        FlatStyle = FlatStyle.Flat,
                        Padding = new Padding(1),
                        MinimumSize = new Size(24, 24),
                        ImageDpi96 = Resources.link,
                        ImageDpi192 = Resources.link1,
                        Name = "cmdLink",
                        UseVisualStyleBackColor = true,
                        TabStop = false
                    };
                    x.nudConnection.ValueChanged += nudConnection_ValueChanged;
                    x.nudLoyalty.ValueChanged += nudLoyalty_ValueChanged;
                    x.chkFree.CheckedChanged += chkFree_CheckedChanged;
                    x.chkGroup.CheckedChanged += chkGroup_CheckedChanged;
                    x.chkBlackmail.CheckedChanged += chkBlackmail_CheckedChanged;
                    x.chkFamily.CheckedChanged += chkFamily_CheckedChanged;
                    x.cmdLink.Click += cmdLink_Click;
                    if (x._objContact != null)
                    {
                        x.chkGroup.DoDataBinding("Checked", x._objContact, nameof(Contact.IsGroup));
                        x.chkGroup.DoOneWayDataBinding("Enabled", x._objContact, nameof(Contact.GroupEnabled));
                        x.chkFree.DoDataBinding("Checked", x._objContact, nameof(Contact.Free));
                        x.chkFree.DoOneWayDataBinding("Enabled", x._objContact, nameof(Contact.FreeEnabled));
                        //We don't actually pay for contacts in play so everyone is free
                        //Don't present a useless field
                        x.chkFree.Visible = x._objContact.CharacterObject?.Created == false;
                        x.chkFamily.DoDataBinding("Checked", x._objContact, nameof(Contact.Family));
                        x.chkFamily.DoOneWayNegatableDataBinding("Visible", _objContact, nameof(Contact.IsEnemy));
                        x.chkBlackmail.DoDataBinding("Checked", x._objContact, nameof(Contact.Blackmail));
                        x.chkBlackmail.DoOneWayNegatableDataBinding("Visible", _objContact, nameof(Contact.IsEnemy));
                        x.nudLoyalty.DoDataBinding("Value", x._objContact, nameof(Contact.Loyalty));
                        x.nudLoyalty.DoOneWayDataBinding("Enabled", x._objContact, nameof(Contact.LoyaltyEnabled));
                        x.nudConnection.DoDataBinding("Value", x._objContact, nameof(Contact.Connection));
                        x.nudConnection.DoOneWayDataBinding("Enabled", x._objContact, nameof(Contact.NotReadOnly));
                        x.nudConnection.DoOneWayDataBinding("Maximum", x._objContact,
                                                            nameof(Contact.ConnectionMaximum));
                        if (x._objContact.IsEnemy)
                        {
                            x.cmdLink.ToolTipText = !string.IsNullOrEmpty(x._objContact.FileName)
                                ? LanguageManager.GetString("Tip_Enemy_OpenLinkedEnemy")
                                : LanguageManager.GetString("Tip_Enemy_LinkEnemy");
                        }
                        else
                        {
                            x.cmdLink.ToolTipText = !string.IsNullOrEmpty(x._objContact.FileName)
                                ? LanguageManager.GetString("Tip_Contact_OpenLinkedContact")
                                : LanguageManager.GetString("Tip_Contact_LinkContact");
                        }
                    }

                    x.tlpMain.SetColumnSpan(x.lblConnection, 2);
                    x.tlpMain.SetColumnSpan(x.chkFamily, 3);

                    x.SuspendLayout();
                    try
                    {
                        x.tlpMain.SuspendLayout();
                        try
                        {
                            x.tlpMain.Controls.Add(x.lblConnection, 0, 2);
                            x.tlpMain.Controls.Add(x.nudConnection, 2, 2);
                            x.tlpMain.Controls.Add(x.lblLoyalty, 3, 2);
                            x.tlpMain.Controls.Add(x.nudLoyalty, 4, 2);
                            x.tlpMain.Controls.Add(x.chkFree, 6, 2);
                            x.tlpMain.Controls.Add(x.chkGroup, 7, 2);
                            x.tlpMain.Controls.Add(x.chkBlackmail, 8, 2);
                            x.tlpMain.Controls.Add(x.chkFamily, 9, 2);
                            x.tlpMain.Controls.Add(x.cmdLink, 12, 2);
                        }
                        finally
                        {
                            x.tlpMain.ResumeLayout();
                        }
                    }
                    finally
                    {
                        x.ResumeLayout(true);
                    }
                });
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
            using (CursorWait.New(this))
            {
                this.DoThreadSafe(x =>
                {
                    x.cboMetatype = new ElasticComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboMetatype"
                    };
                    x.cboGender = new ElasticComboBox
                        {Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboGender"};
                    x.cboAge = new ElasticComboBox
                        {Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboAge"};
                    x.cboType = new ElasticComboBox
                        {Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboType"};
                    x.cboPersonalLife = new ElasticComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true,
                        Name = "cboPersonalLife"
                    };
                    x.cboPreferredPayment = new ElasticComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true,
                        Name = "cboPreferredPayment"
                    };
                    x.cboHobbiesVice = new ElasticComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true,
                        Name = "cboHobbiesVice"
                    };
                });

                LoadStatBlockLists();

                this.DoThreadSafe(x =>
                {
                    if (x._objContact != null)
                    {
                        // Properties controllable by the character themselves
                        x.cboMetatype.DoOneWayDataBinding("Enabled", x._objContact, nameof(Contact.NoLinkedCharacter));
                        x.cboGender.DoOneWayDataBinding("Enabled", x._objContact, nameof(Contact.NoLinkedCharacter));
                        x.cboAge.DoOneWayDataBinding("Enabled", x._objContact, nameof(Contact.NoLinkedCharacter));
                    }

                    x.lblType = new Label
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        Name = "lblType",
                        Tag = "Label_Type",
                        Text = "Type:",
                        TextAlign = ContentAlignment.MiddleRight
                    };
                    x.lblMetatype = new Label
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        Name = "lblMetatype",
                        Tag = "Label_Metatype",
                        Text = "Metatype:",
                        TextAlign = ContentAlignment.MiddleRight
                    };
                    x.lblGender = new Label
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        Name = "lblGender",
                        Tag = "Label_Gender",
                        Text = "Gender:",
                        TextAlign = ContentAlignment.MiddleRight
                    };
                    x.lblAge = new Label
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        Name = "lblAge",
                        Tag = "Label_Age",
                        Text = "Age:",
                        TextAlign = ContentAlignment.MiddleRight
                    };
                    x.lblPersonalLife = new Label
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        Name = "lblPersonalLife",
                        Tag = "Label_Contact_PersonalLife",
                        Text = "Personal Life:",
                        TextAlign = ContentAlignment.MiddleRight
                    };
                    x.lblPreferredPayment = new Label
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        Name = "lblPreferredPayment",
                        Tag = "Label_Contact_PreferredPayment",
                        Text = "Preferred Payment:",
                        TextAlign = ContentAlignment.MiddleRight
                    };
                    x.lblHobbiesVice = new Label
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        Name = "lblHobbiesVice",
                        Tag = "Label_Contact_HobbiesVice",
                        Text = "Hobbies/Vice:",
                        TextAlign = ContentAlignment.MiddleRight
                    };

                    x.tlpStatBlock = new BufferedTableLayoutPanel(components)
                    {
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        ColumnCount = 4,
                        RowCount = 5,
                        Dock = DockStyle.Fill,
                        Name = "tlpStatBlock"
                    };
                    x.tlpStatBlock.ColumnStyles.Add(new ColumnStyle());
                    x.tlpStatBlock.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                    x.tlpStatBlock.ColumnStyles.Add(new ColumnStyle());
                    x.tlpStatBlock.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                    x.tlpStatBlock.RowStyles.Add(new RowStyle());
                    x.tlpStatBlock.RowStyles.Add(new RowStyle());
                    x.tlpStatBlock.RowStyles.Add(new RowStyle());
                    x.tlpStatBlock.RowStyles.Add(new RowStyle());
                    x.tlpStatBlock.Controls.Add(x.lblMetatype, 0, 0);
                    x.tlpStatBlock.Controls.Add(x.cboMetatype, 1, 0);
                    x.tlpStatBlock.Controls.Add(x.lblGender, 0, 1);
                    x.tlpStatBlock.Controls.Add(x.cboGender, 1, 1);
                    x.tlpStatBlock.Controls.Add(x.lblAge, 0, 2);
                    x.tlpStatBlock.Controls.Add(x.cboAge, 1, 2);
                    x.tlpStatBlock.Controls.Add(x.lblType, 0, 3);
                    x.tlpStatBlock.Controls.Add(x.cboType, 1, 3);
                    x.tlpStatBlock.Controls.Add(x.lblPersonalLife, 2, 0);
                    x.tlpStatBlock.Controls.Add(x.cboPersonalLife, 3, 0);
                    x.tlpStatBlock.Controls.Add(x.lblPreferredPayment, 2, 1);
                    x.tlpStatBlock.Controls.Add(x.cboPreferredPayment, 3, 1);
                    x.tlpStatBlock.Controls.Add(x.lblHobbiesVice, 2, 2);
                    x.tlpStatBlock.Controls.Add(x.cboHobbiesVice, 3, 2);

                    x.tlpStatBlock.TranslateWinForm();
                    x.tlpStatBlock.UpdateLightDarkMode();

                    x.SuspendLayout();
                    try
                    {
                        x.tlpMain.SuspendLayout();
                        try
                        {
                            x.tlpMain.SetColumnSpan(x.tlpStatBlock, 13);
                            x.tlpMain.Controls.Add(x.tlpStatBlock, 0, 3);
                        }
                        finally
                        {
                            x.tlpMain.ResumeLayout();
                        }
                    }
                    finally
                    {
                        x.ResumeLayout();
                    }

                    // Need these as separate instead of as simple data bindings so that we don't get annoying live partial translations

                    if (x._objContact != null)
                    {
                        x.cboMetatype.SelectedValue = x._objContact.Metatype;
                        x.cboGender.SelectedValue = x._objContact.Gender;
                        x.cboAge.SelectedValue = x._objContact.Age;
                        x.cboPersonalLife.SelectedValue = x._objContact.PersonalLife;
                        x.cboType.SelectedValue = x._objContact.Type;
                        x.cboPreferredPayment.SelectedValue = x._objContact.PreferredPayment;
                        x.cboHobbiesVice.SelectedValue = x._objContact.HobbiesVice;
                        if (x.cboMetatype.SelectedIndex < 0)
                            x.cboMetatype.Text = x._objContact.DisplayMetatype;
                        if (x.cboGender.SelectedIndex < 0)
                            x.cboGender.Text = x._objContact.DisplayGender;
                        if (x.cboAge.SelectedIndex < 0)
                            x.cboAge.Text = x._objContact.DisplayAge;
                        if (x.cboPersonalLife.SelectedIndex < 0)
                            x.cboPersonalLife.Text = x._objContact.DisplayPersonalLife;
                        if (x.cboType.SelectedIndex < 0)
                            x.cboType.Text = x._objContact.DisplayType;
                        if (x.cboPreferredPayment.SelectedIndex < 0)
                            x.cboPreferredPayment.Text = x._objContact.DisplayPreferredPayment;
                        if (x.cboHobbiesVice.SelectedIndex < 0)
                            x.cboHobbiesVice.Text = x._objContact.DisplayHobbiesVice;
                    }

                    x.cboMetatype.SelectedIndexChanged += UpdateMetatype;
                    x.cboGender.SelectedIndexChanged += UpdateGender;
                    x.cboAge.SelectedIndexChanged += UpdateAge;
                    x.cboType.SelectedIndexChanged += UpdateType;
                    x.cboPersonalLife.SelectedIndexChanged += UpdatePersonalLife;
                    x.cboPreferredPayment.SelectedIndexChanged += UpdatePreferredPayment;
                    x.cboHobbiesVice.SelectedIndexChanged += UpdateHobbiesVice;
                    x.cboMetatype.Leave += UpdateMetatype;
                    x.cboGender.Leave += UpdateGender;
                    x.cboAge.Leave += UpdateAge;
                    x.cboType.Leave += UpdateType;
                    x.cboPersonalLife.Leave += UpdatePersonalLife;
                    x.cboPreferredPayment.Leave += UpdatePreferredPayment;
                    x.cboHobbiesVice.Leave += UpdateHobbiesVice;
                });
            }
        }

        private void LoadStatBlockLists()
        {
            // Read the list of Categories from the XML file.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetatypes))
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstGenders))
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstAges))
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstPersonalLives))
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstTypes))
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstPreferredPayments))
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstHobbiesVices))
            {
                lstMetatypes.Add(ListItem.Blank);
                lstGenders.Add(ListItem.Blank);
                lstAges.Add(ListItem.Blank);
                lstPersonalLives.Add(ListItem.Blank);
                lstTypes.Add(ListItem.Blank);
                lstPreferredPayments.Add(ListItem.Blank);
                lstHobbiesVices.Add(ListItem.Blank);

                XPathNavigator xmlContactsBaseNode = _objContact.CharacterObject.LoadDataXPath("contacts.xml")
                                                                .SelectSingleNodeAndCacheExpression("/chummer");
                if (xmlContactsBaseNode != null)
                {
                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression("genders/gender"))
                    {
                        string strName = xmlNode.Value;
                        lstGenders.Add(new ListItem(
                                           strName,
                                           xmlNode.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression("ages/age"))
                    {
                        string strName = xmlNode.Value;
                        lstAges.Add(new ListItem(
                                        strName,
                                        xmlNode.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression(
                                 "personallives/personallife"))
                    {
                        string strName = xmlNode.Value;
                        lstPersonalLives.Add(new ListItem(
                                                 strName,
                                                 xmlNode.SelectSingleNodeAndCacheExpression("@translate")?.Value
                                                 ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression("types/type"))
                    {
                        string strName = xmlNode.Value;
                        lstTypes.Add(new ListItem(
                                         strName,
                                         xmlNode.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression(
                                 "preferredpayments/preferredpayment"))
                    {
                        string strName = xmlNode.Value;
                        lstPreferredPayments.Add(new ListItem(
                                                     strName,
                                                     xmlNode.SelectSingleNodeAndCacheExpression("@translate")?.Value
                                                     ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression(
                                 "hobbiesvices/hobbyvice"))
                    {
                        string strName = xmlNode.Value;
                        lstHobbiesVices.Add(new ListItem(
                                                strName,
                                                xmlNode.SelectSingleNodeAndCacheExpression("@translate")?.Value
                                                ?? strName));
                    }
                }

                string strSpace = LanguageManager.GetString("String_Space");
                foreach (XPathNavigator xmlMetatypeNode in _objContact.CharacterObject.LoadDataXPath("metatypes.xml")
                                                                      .SelectAndCacheExpression(
                                                                          "/chummer/metatypes/metatype"))
                {
                    string strName = xmlMetatypeNode.SelectSingleNodeAndCacheExpression("name")?.Value;
                    string strMetatypeDisplay = xmlMetatypeNode.SelectSingleNodeAndCacheExpression("translate")?.Value
                                                ?? strName;
                    lstMetatypes.Add(new ListItem(strName, strMetatypeDisplay));
                    XPathNodeIterator xmlMetavariantsList
                        = xmlMetatypeNode.SelectAndCacheExpression("metavariants/metavariant");
                    if (xmlMetavariantsList.Count > 0)
                    {
                        string strMetavariantFormat = strMetatypeDisplay + strSpace + "({0})";
                        foreach (XPathNavigator objXmlMetavariantNode in xmlMetavariantsList)
                        {
                            string strMetavariantName
                                = objXmlMetavariantNode.SelectSingleNodeAndCacheExpression("name")?.Value
                                  ?? string.Empty;
                            if (lstMetatypes.All(
                                    x => strMetavariantName.Equals(x.Value.ToString(),
                                                                   StringComparison.OrdinalIgnoreCase)))
                                lstMetatypes.Add(new ListItem(strMetavariantName,
                                                              string.Format(
                                                                  GlobalSettings.CultureInfo, strMetavariantFormat,
                                                                  objXmlMetavariantNode
                                                                      .SelectSingleNodeAndCacheExpression("translate")
                                                                      ?.Value ?? strMetavariantName)));
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
                
                cboMetatype.PopulateWithListItems(lstMetatypes);
                cboGender.PopulateWithListItems(lstGenders);
                cboAge.PopulateWithListItems(lstAges);
                cboPersonalLife.PopulateWithListItems(lstPersonalLives);
                cboType.PopulateWithListItems(lstTypes);
                cboPreferredPayment.PopulateWithListItems(lstPreferredPayments);
                cboHobbiesVice.PopulateWithListItems(lstHobbiesVices);
            }
        }

        #endregion Methods
    }
}
