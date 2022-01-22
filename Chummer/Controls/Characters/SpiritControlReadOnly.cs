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
    public partial class SpiritControlReadOnly : UserControl
    {
        private readonly Spirit _objSpirit;

        public SpiritControlReadOnly(Spirit objSpirit)
        {
            _objSpirit = objSpirit;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private void SpiritControlReadOnly_Load(object sender, EventArgs e)
        {
            if (this.IsNullOrDisposed())
                return;

            DoDataBindings();

            OnSpiritPropertyChanged(this, null);
        }

        private void DoDataBindings()
        {
            lblSpiritName.DoOneWayDataBinding("Text", _objSpirit, nameof(Spirit.Name));
            lblForce.DoOneWayDataBinding("Text", _objSpirit, nameof(Spirit.Force));
            lblServices.DoOneWayDataBinding("Text", _objSpirit, nameof(Spirit.ServicesOwed));
            chkBound.DoOneWayDataBinding("Checked", _objSpirit, nameof(Spirit.Bound));
            chkFettered.DoOneWayDataBinding("Checked", _objSpirit, nameof(Spirit.Fettered));

            _objSpirit.PropertyChanged += OnSpiritPropertyChanged;
        }

        public void UnbindSpiritControl()
        {
            _objSpirit.PropertyChanged -= OnSpiritPropertyChanged;
            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private void OnSpiritPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool blnDoAllProperties = false;
            switch (e?.PropertyName)
            {
                case null:
                    blnDoAllProperties = true;
                    goto case nameof(Spirit.CritterName);
                case nameof(Spirit.CritterName):
                    txtCritterName.Text = _objSpirit.CritterName.WordWrap();
                    txtCritterName.ScrollBars = txtCritterName.Text.Contains(Environment.NewLine)
                        ? ScrollBars.Vertical
                        : ScrollBars.None;
                    if (blnDoAllProperties)
                        goto case nameof(Spirit.FileName);
                    break;
                case nameof(Spirit.FileName):
                    if (string.IsNullOrEmpty(_objSpirit.FileName))
                        cmdLink.Enabled = false;
                    else
                    {
                        cmdLink.Enabled = true;
                        cmdLink.ToolTipText = LanguageManager.GetString("Tip_Contact_OpenLinkedContact");
                    }
                    if (blnDoAllProperties)
                        goto case nameof(Spirit.Notes);
                    break;
                case nameof(Spirit.Notes):
                    if (string.IsNullOrEmpty(_objSpirit.Notes))
                        cmdNotes.Enabled = false;
                    else
                    {
                        cmdNotes.Enabled = true;
                        string strTooltip = LanguageManager.GetString("Label_Notes") + Environment.NewLine
                            + Environment.NewLine + _objSpirit.Notes;
                        cmdNotes.ToolTipText = strTooltip.WordWrap();
                    }
                    break;
            }
        }

        private void cmdLink_Click(object sender, EventArgs e)
        {
            if (_objSpirit.LinkedCharacter != null)
            {
                Character objOpenCharacter = Program.MainForm.OpenCharacters.Contains(_objSpirit.LinkedCharacter)
                    ? _objSpirit.LinkedCharacter
                    : null;
                using (new CursorWait(this))
                {
                    if (objOpenCharacter == null || !Program.MainForm.SwitchToOpenCharacter(objOpenCharacter, true))
                    {
                        objOpenCharacter = Program.MainForm.LoadCharacter(_objSpirit.LinkedCharacter.FileName);
                        Program.MainForm.OpenCharacter(objOpenCharacter);
                    }
                }
            }
            else
            {
                bool blnUseRelative = false;

                // Make sure the file still exists before attempting to load it.
                if (!File.Exists(_objSpirit.FileName))
                {
                    bool blnError = false;
                    // If the file doesn't exist, use the relative path if one is available.
                    if (string.IsNullOrEmpty(_objSpirit.RelativeFileName))
                        blnError = true;
                    else if (!File.Exists(Path.GetFullPath(_objSpirit.RelativeFileName)))
                        blnError = true;
                    else
                        blnUseRelative = true;

                    if (blnError)
                    {
                        Program.MainForm.ShowMessageBox(
                            string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_FileNotFound"),
                                          _objSpirit.FileName), LanguageManager.GetString("MessageTitle_FileNotFound"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                string strFile = blnUseRelative ? Path.GetFullPath(_objSpirit.RelativeFileName) : _objSpirit.FileName;
                Process.Start(strFile);
            }
        }

        /// <summary>
        /// Spirit object this is linked to.
        /// </summary>
        public Spirit SpiritObject => _objSpirit;
    }
}
