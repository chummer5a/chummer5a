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
using Timer = System.Windows.Forms.Timer;

namespace Chummer
{
    public partial class PetControl : UserControl
    {
        private readonly Contact _objContact;
        private readonly CancellationToken _objMyToken;
        private int _intLoading = 1;

        private int _intUpdatingMetatype = 1;
        private readonly Timer _tmrMetatypeChangeTimer;

        // Events.
        public event EventHandler<TextEventArgs> ContactDetailChanged;

        public event EventHandler DeleteContact;

        #region Control Events

        public PetControl(Contact objContact, CancellationToken objMyToken)
        {
            _objContact = objContact ?? throw new ArgumentNullException(nameof(objContact));
            _objMyToken = objMyToken;
            InitializeComponent();

            _tmrMetatypeChangeTimer = new Timer { Interval = 1000 };
            _tmrMetatypeChangeTimer.Tick += UpdateMetatype;

            Disposed += (sender, args) => UnbindPetControl();

            this.UpdateLightDarkMode(objMyToken);
            this.TranslateWinForm(token: objMyToken);
            foreach (ToolStripItem tssItem in cmsContact.Items)
            {
                tssItem.UpdateLightDarkMode(objMyToken);
                tssItem.TranslateToolStripItemsRecursively(token: objMyToken);
            }
        }

        private async void PetControl_Load(object sender, EventArgs e)
        {
            try
            {
                await LoadContactList(_objMyToken).ConfigureAwait(false);

                await DoDataBindings(_objMyToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
            finally
            {
                Interlocked.Decrement(ref _intUpdatingMetatype);
                Interlocked.Decrement(ref _intLoading);
            }
        }

        public void UnbindPetControl()
        {
            _tmrMetatypeChangeTimer.Dispose();
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

        private void cboMetatype_TextChanged(object sender, EventArgs e)
        {
            if (_tmrMetatypeChangeTimer == null)
                return;
            if (_tmrMetatypeChangeTimer.Enabled)
                _tmrMetatypeChangeTimer.Stop();
            if (_intUpdatingMetatype > 0)
                return;
            _tmrMetatypeChangeTimer.Start();
        }

        private async void UpdateMetatype(object sender, EventArgs e)
        {
            if (_intLoading > 0 || _intUpdatingMetatype > 0)
                return;
            _tmrMetatypeChangeTimer.Stop();
            try
            {
                string strNew = await cboMetatype.DoThreadSafeFuncAsync(x => x.Text, _objMyToken).ConfigureAwait(false);
                string strOld = await _objContact.GetDisplayMetatypeAsync(_objMyToken).ConfigureAwait(false);
                if (strOld == strNew)
                    return;
                await _objContact.SetDisplayMetatypeAsync(strNew, _objMyToken).ConfigureAwait(false);
                strOld = await _objContact.GetDisplayMetatypeAsync(_objMyToken).ConfigureAwait(false);
                if (strOld != strNew)
                {
                    Interlocked.Increment(ref _intUpdatingMetatype);
                    try
                    {
                        await cboMetatype.DoThreadSafeAsync(x => x.Text = strOld, _objMyToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intUpdatingMetatype);
                    }
                }

                ContactDetailChanged?.Invoke(this, new TextEventArgs("Metatype"));
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
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
            try
            {
                if (_objContact.LinkedCharacter != null)
                {
                    Character objOpenCharacter = await Program.OpenCharacters.ContainsAsync(_objContact.LinkedCharacter, _objMyToken)
                        .ConfigureAwait(false)
                        ? _objContact.LinkedCharacter
                        : null;
                    CursorWait objCursorWait = await CursorWait.NewAsync(ParentForm, token: _objMyToken).ConfigureAwait(false);
                    try
                    {
                        if (objOpenCharacter == null)
                        {
                            using (ThreadSafeForm<LoadingBar> frmLoadingBar
                                   = await Program.CreateAndShowProgressBarAsync(
                                           _objContact.LinkedCharacter.FileName, Character.NumLoadingSections, _objMyToken)
                                       .ConfigureAwait(false))
                                objOpenCharacter = await Program.LoadCharacterAsync(
                                        _objContact.LinkedCharacter.FileName, frmLoadingBar: frmLoadingBar.MyForm, token: _objMyToken)
                                    .ConfigureAwait(false);
                        }

                        if (!await Program.SwitchToOpenCharacter(objOpenCharacter, _objMyToken).ConfigureAwait(false))
                            await Program.OpenCharacter(objOpenCharacter, token: _objMyToken).ConfigureAwait(false);
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
                            Program.ShowScrollableMessageBox(
                                string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Message_FileNotFound", token: _objMyToken)
                                        .ConfigureAwait(false), _objContact.FileName),
                                await LanguageManager.GetStringAsync("MessageTitle_FileNotFound", token: _objMyToken).ConfigureAwait(false),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    string strFile = blnUseRelative
                        ? Path.GetFullPath(_objContact.RelativeFileName)
                        : _objContact.FileName;
                    Process.Start(new ProcessStartInfo(strFile) { UseShellExecute = true });
                }
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void tsAttachCharacter_Click(object sender, EventArgs e)
        {
            try
            {
                string strFileName = string.Empty;
                string strFilter = await LanguageManager.GetStringAsync("DialogFilter_Chummer", token: _objMyToken).ConfigureAwait(false) +
                                   '|'
                                   +
                                   await LanguageManager.GetStringAsync("DialogFilter_Chum5", token: _objMyToken).ConfigureAwait(false) +
                                   '|' +
                                   await LanguageManager.GetStringAsync("DialogFilter_Chum5lz", token: _objMyToken).ConfigureAwait(false) +
                                   '|' +
                                   await LanguageManager.GetStringAsync("DialogFilter_All", token: _objMyToken).ConfigureAwait(false);
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
                }, _objMyToken).ConfigureAwait(false);

                if (eResult != DialogResult.OK)
                    return;

                CursorWait objCursorWait = await CursorWait.NewAsync(ParentForm, token: _objMyToken).ConfigureAwait(false);
                try
                {
                    _objContact.FileName = strFileName;
                    string strText = await LanguageManager.GetStringAsync("Tip_Contact_OpenFile", token: _objMyToken).ConfigureAwait(false);
                    await cmdLink.SetToolTipTextAsync(strText, _objMyToken).ConfigureAwait(false);

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
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void tsRemoveCharacter_Click(object sender, EventArgs e)
        {
            try
            {
                // Remove the file association from the Contact.
                if (Program.ShowScrollableMessageBox(
                        await LanguageManager.GetStringAsync("Message_RemoveCharacterAssociation", token: _objMyToken)
                            .ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_RemoveCharacterAssociation", token: _objMyToken)
                            .ConfigureAwait(false), MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    == DialogResult.Yes)
                {
                    _objContact.FileName = string.Empty;
                    _objContact.RelativeFileName = string.Empty;
                    string strText = await LanguageManager.GetStringAsync("Tip_Contact_LinkFile", token: _objMyToken).ConfigureAwait(false);
                    await cmdLink.SetToolTipTextAsync(strText, _objMyToken).ConfigureAwait(false);
                    ContactDetailChanged?.Invoke(this, new TextEventArgs("File"));
                }
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void cmdNotes_Click(object sender, EventArgs e)
        {
            try
            {
                using (ThreadSafeForm<EditNotes> frmContactNotes = await ThreadSafeForm<EditNotes>
                           .GetAsync(() => new EditNotes(_objContact.Notes, _objContact.NotesColor), _objMyToken)
                           .ConfigureAwait(false))
                {
                    if (await frmContactNotes.ShowDialogSafeAsync(_objContact.CharacterObject, _objMyToken).ConfigureAwait(false) !=
                        DialogResult.OK)
                        return;
                    _objContact.Notes = frmContactNotes.MyForm.Notes;
                }

                string strTooltip = await LanguageManager.GetStringAsync("Tip_Contact_EditNotes", token: _objMyToken).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(_objContact.Notes))
                    strTooltip += Environment.NewLine + Environment.NewLine + _objContact.Notes;
                strTooltip = strTooltip.WordWrap();
                await cmdNotes.SetToolTipTextAsync(strTooltip, _objMyToken).ConfigureAwait(false);
                ContactDetailChanged?.Invoke(this, new TextEventArgs("Notes"));
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        #endregion Control Events

        #region Methods

        private async Task LoadContactList(CancellationToken token = default)
        {
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetatypes))
            {
                lstMetatypes.Add(ListItem.Blank);
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                foreach (XPathNavigator xmlMetatypeNode in (await _objContact.CharacterObject.LoadDataXPathAsync("critters.xml", token: token).ConfigureAwait(false))
                                                                 .SelectAndCacheExpression(
                                                                     "/chummer/metatypes/metatype", token: token))
                {
                    string strName = xmlMetatypeNode.SelectSingleNodeAndCacheExpression("name", token: token)?.Value;
                    string strMetatypeDisplay = xmlMetatypeNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                                ?? strName;
                    lstMetatypes.Add(new ListItem(strName, strMetatypeDisplay));
                    XPathNodeIterator xmlMetavariantsList
                        = xmlMetatypeNode.SelectAndCacheExpression("metavariants/metavariant", token: token);
                    if (xmlMetavariantsList.Count > 0)
                    {
                        string strMetavariantFormat = strMetatypeDisplay + strSpace + "({0})";
                        foreach (XPathNavigator objXmlMetavariantNode in xmlMetavariantsList)
                        {
                            string strMetavariantName
                                = objXmlMetavariantNode.SelectSingleNodeAndCacheExpression("name", token: token)?.Value
                                  ?? string.Empty;
                            if (lstMetatypes.All(
                                    x => strMetavariantName.Equals(x.Value.ToString(),
                                                                   StringComparison.OrdinalIgnoreCase)))
                                lstMetatypes.Add(new ListItem(strMetavariantName,
                                                              string.Format(
                                                                  GlobalSettings.CultureInfo, strMetavariantFormat,
                                                                  objXmlMetavariantNode
                                                                      .SelectSingleNodeAndCacheExpression("translate", token: token)
                                                                      ?.Value ?? strMetavariantName)));
                        }
                    }
                }

                lstMetatypes.Sort(CompareListItems.CompareNames);
                await cboMetatype.PopulateWithListItemsAsync(lstMetatypes, token: token).ConfigureAwait(false);
            }
        }

        private async Task DoDataBindings(CancellationToken token = default)
        {
            string strMetatype = await _objContact.GetMetatypeAsync(token).ConfigureAwait(false);
            await cboMetatype.DoThreadSafeAsync(x => x.SelectedValue = strMetatype, token: token).ConfigureAwait(false);
            if (await cboMetatype.DoThreadSafeFuncAsync(x => x.SelectedIndex, token: token).ConfigureAwait(false) < 0)
            {
                string strText = await _objContact.GetDisplayMetatypeAsync(token).ConfigureAwait(false);
                await cboMetatype.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
            }

            await txtContactName.DoDataBindingAsync("Text", _objContact, nameof(Contact.Name), token).ConfigureAwait(false);
            await this.RegisterOneWayAsyncDataBindingAsync((x, y) => x.BackColor = y, _objContact,
                    nameof(Contact.PreferredColor), x => x.GetPreferredColorAsync(_objMyToken), token)
                .ConfigureAwait(false);

            // Properties controllable by the character themselves
            await txtContactName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objContact,
                nameof(_objContact.NoLinkedCharacter), x => x.GetNoLinkedCharacterAsync(_objMyToken), token).ConfigureAwait(false);
            await cboMetatype.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objContact,
                nameof(_objContact.NoLinkedCharacter), x => x.GetNoLinkedCharacterAsync(_objMyToken), token).ConfigureAwait(false);
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
