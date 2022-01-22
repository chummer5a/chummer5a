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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Chummer
{
    public partial class ContactControlReadOnly : UserControl
    {
        private readonly Contact _objContact;

        public ContactControlReadOnly(Contact objContact)
        {
            _objContact = objContact ?? throw new ArgumentNullException(nameof(objContact));

            InitializeComponent();
            if (_objContact?.CharacterObject.Created != false)
            {
                chkFree.Parent.Controls.Remove(chkFree);
                chkFree.Dispose();
            }
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private void ContactControlReadOnly_Load(object sender, EventArgs e)
        {
            if (this.IsNullOrDisposed())
                return;

            DoDataBindings();

            OnContactPropertyChanged(this, null);
        }

        private void DoDataBindings()
        {
            lblConnection.DoOneWayDataBinding("Text", _objContact, nameof(Contact.Connection));
            lblLoyalty.DoOneWayDataBinding("Text", _objContact, nameof(Contact.Loyalty));
            if (!_objContact.CharacterObject.Created)
                chkFree.DoOneWayDataBinding("Checked", _objContact, nameof(Contact.Free));
            chkGroup.DoOneWayDataBinding("Checked", _objContact, nameof(Contact.IsGroup));
            chkFamily.DoOneWayDataBinding("Checked", _objContact, nameof(Contact.Family));
            chkFamily.DoOneWayNegatableDataBinding("Visible", _objContact, nameof(Contact.IsEnemy));
            chkBlackmail.DoOneWayDataBinding("Checked", _objContact, nameof(Contact.Blackmail));
            chkBlackmail.DoOneWayNegatableDataBinding("Visible", _objContact, nameof(Contact.IsEnemy));
            this.DoOneWayDataBinding("BackColor", _objContact, nameof(Contact.PreferredColor));

            _objContact.PropertyChanged += OnContactPropertyChanged;
        }

        public void UnbindContactControl()
        {
            _objContact.PropertyChanged -= OnContactPropertyChanged;
            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private void OnContactPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool blnDoAllProperties = false;
            switch (e?.PropertyName)
            {
                case null:
                    blnDoAllProperties = true;
                    goto case nameof(Contact.Name);
                case nameof(Contact.Name):
                    txtName.Text = _objContact.Name.WordWrap();
                    txtName.ScrollBars = txtName.Text.Contains(Environment.NewLine)
                        ? ScrollBars.Vertical
                        : ScrollBars.None;
                    if (blnDoAllProperties)
                        goto case nameof(Contact.Location);
                    break;
                case nameof(Contact.Location):
                    txtLocation.Text = _objContact.Location.WordWrap();
                    txtLocation.ScrollBars = txtLocation.Text.Contains(Environment.NewLine)
                        ? ScrollBars.Vertical
                        : ScrollBars.None;
                    if (blnDoAllProperties)
                        goto case nameof(Contact.DisplayRole);
                    break;
                case nameof(Contact.DisplayRole):
                    txtArchetype.Text = _objContact.DisplayRole.WordWrap();
                    txtArchetype.ScrollBars = txtArchetype.Text.Contains(Environment.NewLine)
                        ? ScrollBars.Vertical
                        : ScrollBars.None;
                    if (blnDoAllProperties)
                        goto case nameof(Contact.DisplayType);
                    break;
                case nameof(Contact.DisplayType):
                    if (string.IsNullOrEmpty(_objContact.DisplayType))
                    {
                        lblTypeLabel.Visible = false;
                        txtType.Visible = false;
                    }
                    else
                    {
                        lblTypeLabel.Visible = true;
                        txtType.Visible = true;
                        txtType.Text = _objContact.DisplayType.WordWrap();
                        txtType.ScrollBars = txtType.Text.Contains(Environment.NewLine)
                            ? ScrollBars.Vertical
                            : ScrollBars.None;
                    }

                    if (blnDoAllProperties)
                        goto case nameof(Contact.DisplayMetatype);
                    break;
                case nameof(Contact.DisplayMetatype):
                    if (string.IsNullOrEmpty(_objContact.DisplayType))
                    {
                        lblMetatypeLabel.Visible = false;
                        txtMetatype.Visible = false;
                    }
                    else
                    {
                        lblMetatypeLabel.Visible = true;
                        txtMetatype.Visible = true;
                        txtMetatype.Text = _objContact.DisplayMetatype.WordWrap();
                        txtMetatype.ScrollBars = txtMetatype.Text.Contains(Environment.NewLine)
                            ? ScrollBars.Vertical
                            : ScrollBars.None;
                    }

                    if (blnDoAllProperties)
                        goto case nameof(Contact.DisplayGender);
                    break;
                case nameof(Contact.DisplayGender):
                    if (string.IsNullOrEmpty(_objContact.DisplayType))
                    {
                        lblGenderLabel.Visible = false;
                        txtGender.Visible = false;
                    }
                    else
                    {
                        lblGenderLabel.Visible = true;
                        txtGender.Visible = true;
                        txtGender.Text = _objContact.DisplayGender.WordWrap();
                        txtGender.ScrollBars = txtGender.Text.Contains(Environment.NewLine)
                            ? ScrollBars.Vertical
                            : ScrollBars.None;
                    }

                    if (blnDoAllProperties)
                        goto case nameof(Contact.DisplayAge);
                    break;
                case nameof(Contact.DisplayAge):
                    if (string.IsNullOrEmpty(_objContact.DisplayType))
                    {
                        lblAgeLabel.Visible = false;
                        txtAge.Visible = false;
                    }
                    else
                    {
                        lblAgeLabel.Visible = true;
                        txtAge.Visible = true;
                        txtAge.Text = _objContact.DisplayAge.WordWrap();
                        txtAge.ScrollBars = txtAge.Text.Contains(Environment.NewLine)
                            ? ScrollBars.Vertical
                            : ScrollBars.None;
                    }

                    if (blnDoAllProperties)
                        goto case nameof(Contact.DisplayPersonalLife);
                    break;
                case nameof(Contact.DisplayPersonalLife):
                    if (string.IsNullOrEmpty(_objContact.DisplayType))
                    {
                        lblPersonalLifeLabel.Visible = false;
                        txtPersonalLife.Visible = false;
                    }
                    else
                    {
                        lblPersonalLifeLabel.Visible = true;
                        txtPersonalLife.Visible = true;
                        txtPersonalLife.Text = _objContact.DisplayPersonalLife.WordWrap();
                        txtPersonalLife.ScrollBars = txtPersonalLife.Text.Contains(Environment.NewLine)
                            ? ScrollBars.Vertical
                            : ScrollBars.None;
                    }

                    if (blnDoAllProperties)
                        goto case nameof(Contact.DisplayPreferredPayment);
                    break;
                case nameof(Contact.DisplayPreferredPayment):
                    if (string.IsNullOrEmpty(_objContact.DisplayType))
                    {
                        lblPreferredPaymentLabel.Visible = false;
                        txtPreferredPayment.Visible = false;
                    }
                    else
                    {
                        lblPreferredPaymentLabel.Visible = true;
                        txtPreferredPayment.Visible = true;
                        txtPreferredPayment.Text = _objContact.DisplayPreferredPayment.WordWrap();
                        txtPreferredPayment.ScrollBars = txtPreferredPayment.Text.Contains(Environment.NewLine)
                            ? ScrollBars.Vertical
                            : ScrollBars.None;
                    }

                    if (blnDoAllProperties)
                        goto case nameof(Contact.DisplayHobbiesVice);
                    break;
                case nameof(Contact.DisplayHobbiesVice):
                    if (string.IsNullOrEmpty(_objContact.DisplayType))
                    {
                        lblHobbiesViceLabel.Visible = false;
                        txtHobbiesVice.Visible = false;
                    }
                    else
                    {
                        lblHobbiesViceLabel.Visible = true;
                        txtHobbiesVice.Visible = true;
                        txtHobbiesVice.Text = _objContact.DisplayHobbiesVice.WordWrap();
                        txtHobbiesVice.ScrollBars = txtHobbiesVice.Text.Contains(Environment.NewLine)
                            ? ScrollBars.Vertical
                            : ScrollBars.None;
                    }

                    if (blnDoAllProperties)
                        goto case nameof(Contact.IsEnemy);
                    break;
                case nameof(Contact.IsEnemy):
                case nameof(Contact.FileName):
                    if (string.IsNullOrEmpty(_objContact.FileName))
                        cmdLink.Enabled = false;
                    else
                    {
                        cmdLink.Enabled = true;
                        cmdLink.ToolTipText
                            = LanguageManager.GetString(_objContact.IsEnemy
                                                            ? "Tip_Enemy_OpenLinkedEnemy"
                                                            : "Tip_Contact_OpenLinkedContact");
                    }
                    if (blnDoAllProperties)
                        goto case nameof(Contact.Notes);
                    break;
                case nameof(Contact.Notes):
                    if (string.IsNullOrEmpty(_objContact.Notes))
                        cmdNotes.Enabled = false;
                    else
                    {
                        cmdNotes.Enabled = true;
                        string strTooltip = LanguageManager.GetString("Label_Notes") + Environment.NewLine
                            + Environment.NewLine + _objContact.Notes;
                        cmdNotes.ToolTipText = strTooltip.WordWrap();
                    }
                    break;
            }
        }

        private void cmdLink_Click(object sender, EventArgs e)
        {
            if (_objContact.LinkedCharacter != null)
            {
                Character objOpenCharacter = Program.MainForm.OpenCharacters.Contains(_objContact.LinkedCharacter)
                    ? _objContact.LinkedCharacter
                    : null;
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
                        Program.MainForm.ShowMessageBox(
                            string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_FileNotFound"),
                                          _objContact.FileName), LanguageManager.GetString("MessageTitle_FileNotFound"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                string strFile = blnUseRelative ? Path.GetFullPath(_objContact.RelativeFileName) : _objContact.FileName;
                Process.Start(strFile);
            }
        }

        /// <summary>
        /// Contact object this is linked to.
        /// </summary>
        public Contact ContactObject => _objContact;
    }
}
