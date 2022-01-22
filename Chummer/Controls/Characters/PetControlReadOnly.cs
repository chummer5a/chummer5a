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
    public partial class PetControlReadOnly : UserControl
    {
        private readonly Contact _objContact;

        public PetControlReadOnly(Contact objContact)
        {
            _objContact = objContact;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private void PetControlReadOnly_Load(object sender, EventArgs e)
        {
            if (this.IsNullOrDisposed())
                return;

            DoDataBindings();

            OnContactPropertyChanged(this, null);
        }

        private void DoDataBindings()
        {
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
                        goto case nameof(Contact.FileName);
                    break;
                case nameof(Contact.FileName):
                    if (string.IsNullOrEmpty(_objContact.FileName))
                        cmdLink.Enabled = false;
                    else
                    {
                        cmdLink.Enabled = true;
                        cmdLink.ToolTipText = LanguageManager.GetString("Tip_Contact_OpenLinkedContact");
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
