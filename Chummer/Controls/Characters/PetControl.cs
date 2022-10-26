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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class PetControl : UserControl
    {
        private readonly Contact _objContact;
        private int _intLoading = 1;

        // Events.
        public event EventHandler<TextEventArgs> ContactDetailChanged;

        public event EventHandler DeleteContact;

        #region Control Events

        public PetControl(Contact objContact)
        {
            _objContact = objContact;
            InitializeComponent();

            Disposed += (sender, args) => UnbindPetControl();

            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            foreach (ToolStripItem tssItem in cmsContact.Items)
            {
                tssItem.UpdateLightDarkMode();
                tssItem.TranslateToolStripItemsRecursively();
            }
        }

        private async void PetControl_Load(object sender, EventArgs e)
        {
            await LoadContactList().ConfigureAwait(false);

            await DoDataBindings().ConfigureAwait(false);

            Interlocked.Decrement(ref _intLoading);
        }

        public void UnbindPetControl()
        {
            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private void txtContactName_TextChanged(object sender, EventArgs e)
        {
            if (_intLoading == 0)
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Name"));
        }

        private void UpdateMetatype(object sender, EventArgs e)
        {
            if (_intLoading > 0 || _objContact.DisplayMetatype == cboMetatype.Text)
                return;
            _objContact.DisplayMetatype = cboMetatype.Text;
            if (_objContact.DisplayMetatype != cboMetatype.Text)
            {
                if (Interlocked.Increment(ref _intLoading) != 1)
                {
                    Interlocked.Decrement(ref _intLoading);
                    return;
                }
                try
                {
                    cboMetatype.Text = _objContact.DisplayMetatype;
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }
            }
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Metatype"));
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Raise the DeleteContact Event when the user has confirmed their desire to delete the Contact.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            DeleteContact?.Invoke(this, e);
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
            cmsContact.Show(cmdLink, cmdLink.Left - 700, cmdLink.Top);
        }

        private async void tsContactOpen_Click(object sender, EventArgs e)
        {
            if (_objContact.LinkedCharacter != null)
            {
                Character objOpenCharacter = await Program.OpenCharacters.ContainsAsync(_objContact.LinkedCharacter).ConfigureAwait(false)
                    ? _objContact.LinkedCharacter
                    : null;
                CursorWait objCursorWait = await CursorWait.NewAsync(ParentForm).ConfigureAwait(false);
                try
                {
                    if (objOpenCharacter == null)
                    {
                        using (ThreadSafeForm<LoadingBar> frmLoadingBar
                               = await Program.CreateAndShowProgressBarAsync(
                                   _objContact.LinkedCharacter.FileName, Character.NumLoadingSections).ConfigureAwait(false))
                            objOpenCharacter = await Program.LoadCharacterAsync(
                                _objContact.LinkedCharacter.FileName, frmLoadingBar: frmLoadingBar.MyForm).ConfigureAwait(false);
                    }

                    if (!await Program.SwitchToOpenCharacter(objOpenCharacter).ConfigureAwait(false))
                        await Program.OpenCharacter(objOpenCharacter).ConfigureAwait(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
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
                        Program.ShowMessageBox(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_FileNotFound").ConfigureAwait(false), _objContact.FileName), await LanguageManager.GetStringAsync("MessageTitle_FileNotFound").ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                string strFile = blnUseRelative ? Path.GetFullPath(_objContact.RelativeFileName) : _objContact.FileName;
                Process.Start(strFile);
            }
        }

        private async void tsAttachCharacter_Click(object sender, EventArgs e)
        {
            string strFileName = string.Empty;
            string strFilter = await LanguageManager.GetStringAsync("DialogFilter_Chummer").ConfigureAwait(false) + '|'
                +
                await LanguageManager.GetStringAsync("DialogFilter_Chum5").ConfigureAwait(false) + '|' +
                await LanguageManager.GetStringAsync("DialogFilter_Chum5lz").ConfigureAwait(false) + '|' +
                await LanguageManager.GetStringAsync("DialogFilter_All").ConfigureAwait(false);
            // Prompt the user to select a save file to associate with this Contact.
            DialogResult eResult = await this.DoThreadSafeFuncAsync(x =>
            {
                using (OpenFileDialog dlgOpenFile = new OpenFileDialog())
                {
                    dlgOpenFile.Filter = strFilter;
                    if (!string.IsNullOrEmpty(_objContact.FileName) && File.Exists(_objContact.FileName))
                    {
                        dlgOpenFile.InitialDirectory = Path.GetDirectoryName(_objContact.FileName);
                        dlgOpenFile.FileName = Path.GetFileName(_objContact.FileName);
                    }

                    DialogResult eReturn = dlgOpenFile.ShowDialog(x);
                    strFileName = dlgOpenFile.FileName;
                    return eReturn;
                }
            }).ConfigureAwait(false);

            if (eResult != DialogResult.OK)
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(ParentForm).ConfigureAwait(false);
            try
            {
                _objContact.FileName = strFileName;
                string strText = await LanguageManager.GetStringAsync("Tip_Contact_OpenFile").ConfigureAwait(false);
                await cmdLink.SetToolTipTextAsync(strText).ConfigureAwait(false);

                // Set the relative path.
                Uri uriApplication = new Uri(Utils.GetStartupPath);
                Uri uriFile = new Uri(_objContact.FileName);
                Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
                _objContact.RelativeFileName = "../" + uriRelative;

                ContactDetailChanged?.Invoke(this, new TextEventArgs("File"));
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void tsRemoveCharacter_Click(object sender, EventArgs e)
        {
            // Remove the file association from the Contact.
            if (Program.ShowMessageBox(await LanguageManager.GetStringAsync("Message_RemoveCharacterAssociation").ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_RemoveCharacterAssociation").ConfigureAwait(false), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _objContact.FileName = string.Empty;
                _objContact.RelativeFileName = string.Empty;
                string strText = await LanguageManager.GetStringAsync("Tip_Contact_LinkFile").ConfigureAwait(false);
                await cmdLink.SetToolTipTextAsync(strText).ConfigureAwait(false);
                ContactDetailChanged?.Invoke(this, new TextEventArgs("File"));
            }
        }

        private async void cmdNotes_Click(object sender, EventArgs e)
        {
            using (ThreadSafeForm<EditNotes> frmContactNotes = await ThreadSafeForm<EditNotes>.GetAsync(() => new EditNotes(_objContact.Notes, _objContact.NotesColor)).ConfigureAwait(false))
            {
                if (await frmContactNotes.ShowDialogSafeAsync(_objContact.CharacterObject).ConfigureAwait(false) != DialogResult.OK)
                    return;
                _objContact.Notes = frmContactNotes.MyForm.Notes;
            }

            string strTooltip = await LanguageManager.GetStringAsync("Tip_Contact_EditNotes").ConfigureAwait(false);
            if (!string.IsNullOrEmpty(_objContact.Notes))
                strTooltip += Environment.NewLine + Environment.NewLine + _objContact.Notes;
            strTooltip = strTooltip.WordWrap();
            await cmdNotes.SetToolTipTextAsync(strTooltip).ConfigureAwait(false);
            ContactDetailChanged?.Invoke(this, new TextEventArgs("Notes"));
        }

        #endregion Control Events

        #region Methods

        private async ValueTask LoadContactList(CancellationToken token = default)
        {
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetatypes))
            {
                lstMetatypes.Add(ListItem.Blank);
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                foreach (XPathNavigator xmlMetatypeNode in await (await _objContact.CharacterObject.LoadDataXPathAsync("critters.xml", token: token).ConfigureAwait(false))
                                                                 .SelectAndCacheExpressionAsync(
                                                                     "/chummer/metatypes/metatype", token: token).ConfigureAwait(false))
                {
                    string strName = (await xmlMetatypeNode.SelectSingleNodeAndCacheExpressionAsync("name", token: token).ConfigureAwait(false))?.Value;
                    string strMetatypeDisplay = (await xmlMetatypeNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false))?.Value
                                                ?? strName;
                    lstMetatypes.Add(new ListItem(strName, strMetatypeDisplay));
                    XPathNodeIterator xmlMetavariantsList
                        = await xmlMetatypeNode.SelectAndCacheExpressionAsync("metavariants/metavariant", token: token).ConfigureAwait(false);
                    if (xmlMetavariantsList.Count > 0)
                    {
                        string strMetavariantFormat = strMetatypeDisplay + strSpace + "({0})";
                        foreach (XPathNavigator objXmlMetavariantNode in xmlMetavariantsList)
                        {
                            string strMetavariantName
                                = (await objXmlMetavariantNode.SelectSingleNodeAndCacheExpressionAsync("name", token: token).ConfigureAwait(false))?.Value
                                  ?? string.Empty;
                            if (lstMetatypes.All(
                                    x => strMetavariantName.Equals(x.Value.ToString(),
                                                                   StringComparison.OrdinalIgnoreCase)))
                                lstMetatypes.Add(new ListItem(strMetavariantName,
                                                              string.Format(
                                                                  GlobalSettings.CultureInfo, strMetavariantFormat,
                                                                  (await objXmlMetavariantNode
                                                                         .SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false))
                                                                      ?.Value ?? strMetavariantName)));
                        }
                    }
                }

                lstMetatypes.Sort(CompareListItems.CompareNames);
                await cboMetatype.PopulateWithListItemsAsync(lstMetatypes, token: token).ConfigureAwait(false);
            }
        }

        private async ValueTask DoDataBindings(CancellationToken token = default)
        {
            string strMetatype = await _objContact.GetMetatypeAsync(token).ConfigureAwait(false);
            await cboMetatype.DoThreadSafeAsync(x => x.SelectedValue = strMetatype, token: token).ConfigureAwait(false);
            if (await cboMetatype.DoThreadSafeFuncAsync(x => x.SelectedIndex, token: token).ConfigureAwait(false) < 0)
            {
                string strText = await _objContact.GetDisplayMetatypeAsync(token).ConfigureAwait(false);
                await cboMetatype.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
            }

            await txtContactName.DoDataBindingAsync("Text", _objContact, nameof(Contact.Name), token).ConfigureAwait(false);
            await this.DoOneWayDataBindingAsync("BackColor", _objContact, nameof(Contact.PreferredColor), token).ConfigureAwait(false);

            // Properties controllable by the character themselves
            await txtContactName.DoOneWayDataBindingAsync("Enabled", _objContact, nameof(Contact.NoLinkedCharacter), token).ConfigureAwait(false);
            await cboMetatype.DoOneWayDataBindingAsync("Enabled", _objContact, nameof(Contact.NoLinkedCharacter), token).ConfigureAwait(false);
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Contact object this is linked to.
        /// </summary>
        public Contact ContactObject => _objContact;

        #endregion Properties
    }
}
