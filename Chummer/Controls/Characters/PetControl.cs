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
using System.Drawing;
using System.IO;
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
        public event EventHandlerExtensions.SafeAsyncEventHandler DeleteContact;

        #region Control Events

        public PetControl(Contact objContact, CancellationToken objMyToken)
        {
            _objContact = objContact ?? throw new ArgumentNullException(nameof(objContact));
            _objMyToken = objMyToken;
            InitializeComponent();

            _tmrMetatypeChangeTimer = new Timer { Interval = 1000 };
            _tmrMetatypeChangeTimer.Tick += UpdateMetatype;

            this.UpdateLightDarkMode(objMyToken);
            this.TranslateWinForm(token: objMyToken);
            this.UpdateParentForToolTipControls();
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
                objControl.ResetBindings();
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
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void cmdDelete_Click(object sender, EventArgs e)
        {
            // Raise the DeleteContact Event when the user has confirmed their desire to delete the Contact.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            if (DeleteContact == null)
                return;
            try
            {
                await DeleteContact.Invoke(this, e, _objMyToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void cmdLink_Click(object sender, EventArgs e)
        {
            // Determine which options should be shown based on the FileName value.
            try
            {
                if (!string.IsNullOrEmpty(await _objContact.GetFileNameAsync(_objMyToken).ConfigureAwait(false)))
                {
                    await cmsContact.DoThreadSafeAsync(() =>
                    {
                        tsAttachCharacter.Visible = false;
                        tsContactOpen.Visible = true;
                        tsRemoveCharacter.Visible = true;
                    }, token: _objMyToken).ConfigureAwait(false);
                }
                else
                {
                    await cmsContact.DoThreadSafeAsync(() =>
                    {
                        tsAttachCharacter.Visible = true;
                        tsContactOpen.Visible = false;
                        tsRemoveCharacter.Visible = false;
                    }, token: _objMyToken).ConfigureAwait(false);
                }

                await cmsContact.DoThreadSafeAsync(x => x.Show(cmdLink, cmdLink.Left - x.PreferredSize.Width, cmdLink.Top), _objMyToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void tsContactOpen_Click(object sender, EventArgs e)
        {
            try
            {
                string strFileName;
                Character objOpenCharacter = null;
                Character objLinkedCharacter = await _objContact.GetLinkedCharacterAsync(_objMyToken).ConfigureAwait(false);
                if (objLinkedCharacter == null)
                {
                    // Make sure the file still exists before attempting to load it.
                    strFileName = await _objContact.GetFileNameAsync(_objMyToken).ConfigureAwait(false);
                    if (!File.Exists(strFileName))
                    {
                        // If the file doesn't exist, use the relative path if one is available.
                        string strRelativeFileName = await _objContact.GetRelativeFileNameAsync(_objMyToken).ConfigureAwait(false);
                        // If the file doesn't exist, use the relative path if one is available.
                        if (string.IsNullOrEmpty(strRelativeFileName) || !File.Exists(Path.GetFullPath(strRelativeFileName)))
                        {
                            await Program.ShowScrollableMessageBoxAsync(
                                string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Message_FileNotFound", token: _objMyToken)
                                        .ConfigureAwait(false), strFileName),
                                await LanguageManager.GetStringAsync("MessageTitle_FileNotFound", token: _objMyToken).ConfigureAwait(false),
                                MessageBoxButtons.OK, MessageBoxIcon.Error, token: _objMyToken).ConfigureAwait(false);
                            return;
                        }
                        else
                            strFileName = Path.GetFullPath(strRelativeFileName);
                    }
                }
                else
                {
                    strFileName = await objLinkedCharacter.GetFileNameAsync(_objMyToken).ConfigureAwait(false);
                    if (await Program.OpenCharacters.ContainsAsync(objLinkedCharacter, _objMyToken).ConfigureAwait(false))
                        objOpenCharacter = objLinkedCharacter;
                }
                CursorWait objCursorWait = await CursorWait.NewAsync(ParentForm, token: _objMyToken).ConfigureAwait(false);
                try
                {
                    if (objOpenCharacter == null)
                    {
                        using (ThreadSafeForm<LoadingBar> frmLoadingBar
                               = await Program.CreateAndShowProgressBarAsync(
                                       strFileName, Character.NumLoadingSections, _objMyToken)
                                   .ConfigureAwait(false))
                            objOpenCharacter = await Program.LoadCharacterAsync(
                                    strFileName, frmLoadingBar: frmLoadingBar.MyForm, token: _objMyToken)
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
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void tsAttachCharacter_Click(object sender, EventArgs e)
        {
            try
            {
                string strOldFileName = await _objContact.GetFileNameAsync(_objMyToken).ConfigureAwait(false);
                string strFileName = string.Empty;
                string strFilter = await LanguageManager.GetStringAsync("DialogFilter_Chummer", token: _objMyToken).ConfigureAwait(false) +
                                   "|" +
                                   await LanguageManager.GetStringAsync("DialogFilter_Chum5", token: _objMyToken).ConfigureAwait(false) +
                                   "|" +
                                   await LanguageManager.GetStringAsync("DialogFilter_Chum5lz", token: _objMyToken).ConfigureAwait(false) +
                                   "|" +
                                   await LanguageManager.GetStringAsync("DialogFilter_All", token: _objMyToken).ConfigureAwait(false);
                // Prompt the user to select a save file to associate with this Contact.
                DialogResult eResult = await this.DoThreadSafeFuncAsync(x =>
                {
                    using (OpenFileDialog dlgOpenFile = new OpenFileDialog())
                    {
                        dlgOpenFile.Filter = strFilter;
                        if (!string.IsNullOrEmpty(strOldFileName) && File.Exists(strOldFileName))
                        {
                            dlgOpenFile.InitialDirectory = Path.GetDirectoryName(strOldFileName);
                            dlgOpenFile.FileName = Path.GetFileName(strOldFileName);
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
                    string strText = await LanguageManager.GetStringAsync("Tip_Contact_OpenFile", token: _objMyToken).ConfigureAwait(false);
                    await cmdLink.SetToolTipTextAsync(strText, _objMyToken).ConfigureAwait(false);

                    // Set the relative path.
                    Uri uriApplication = new Uri(Utils.GetStartupPath);
                    Uri uriFile = new Uri(strFileName);
                    Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
                    IAsyncDisposable objLocker = await _objContact.LockObject.EnterWriteLockAsync(_objMyToken).ConfigureAwait(false);
                    try
                    {
                        _objMyToken.ThrowIfCancellationRequested();
                        await _objContact.SetFileNameAsync(strFileName, _objMyToken).ConfigureAwait(false);
                        await _objContact.SetRelativeFileNameAsync("../" + uriRelative.ToString(), _objMyToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
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
                if (await Program.ShowScrollableMessageBoxAsync(
                        await LanguageManager.GetStringAsync("Message_RemoveCharacterAssociation", token: _objMyToken)
                            .ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_RemoveCharacterAssociation", token: _objMyToken)
                            .ConfigureAwait(false), MessageBoxButtons.YesNo, MessageBoxIcon.Question, token: _objMyToken).ConfigureAwait(false)
                    == DialogResult.Yes)
                {
                    string strText = await LanguageManager.GetStringAsync("Tip_Contact_LinkFile", token: _objMyToken).ConfigureAwait(false);
                    await cmdLink.SetToolTipTextAsync(strText, _objMyToken).ConfigureAwait(false);
                    IAsyncDisposable objLocker = await _objContact.LockObject.EnterWriteLockAsync(_objMyToken).ConfigureAwait(false);
                    try
                    {
                        _objMyToken.ThrowIfCancellationRequested();
                        await _objContact.SetFileNameAsync(string.Empty, _objMyToken).ConfigureAwait(false);
                        await _objContact.SetRelativeFileNameAsync(string.Empty, _objMyToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
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
                string strNotes = await _objContact.GetNotesAsync(_objMyToken).ConfigureAwait(false);
                Color objColor = await _objContact.GetNotesColorAsync(_objMyToken).ConfigureAwait(false);
                using (ThreadSafeForm<EditNotes> frmContactNotes = await ThreadSafeForm<EditNotes>
                           .GetAsync(() => new EditNotes(strNotes, objColor), _objMyToken)
                           .ConfigureAwait(false))
                {
                    if (await frmContactNotes.ShowDialogSafeAsync(_objContact.CharacterObject, _objMyToken).ConfigureAwait(false) !=
                        DialogResult.OK)
                        return;
                    await _objContact.SetNotesAsync(frmContactNotes.MyForm.Notes, _objMyToken).ConfigureAwait(false);
                    await _objContact.SetNotesColorAsync(frmContactNotes.MyForm.NotesColor, _objMyToken).ConfigureAwait(false);
                }

                string strTooltip = await LanguageManager.GetStringAsync("Tip_Contact_EditNotes", token: _objMyToken).ConfigureAwait(false);
                strNotes = await _objContact.GetNotesAsync(_objMyToken).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strNotes))
                    strTooltip += Environment.NewLine + Environment.NewLine + strNotes;
                strTooltip = strTooltip.WordWrap();
                await cmdNotes.SetToolTipTextAsync(strTooltip, _objMyToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            // Note: because we cannot unsubscribe old parents from events if/when we change parents, we do not want to have this automatically update
            // based on a subscription to our parent's ParentChanged (which we would need to be able to automatically update our parent form for nested controls)
            // We therefore need to use the hacky workaround of calling UpdateParentForToolTipControls() for parent forms/controls as appropriate
            this.UpdateParentForToolTipControls();
        }

        #endregion Control Events

        #region Methods

        private async Task LoadContactList(CancellationToken token = default)
        {
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetatypes))
            {
                lstMetatypes.Add(ListItem.Blank);
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                foreach (XPathNavigator xmlMetatypeNode in (await _objContact.CharacterObject.LoadDataXPathAsync("critters.xml", token: token).ConfigureAwait(false))
                                                                 .SelectAndCacheExpression(
                                                                     "/chummer/metatypes/metatype", token: token))
                {
                    string strName = xmlMetatypeNode.SelectSingleNodeAndCacheExpression("name", token: token)?.Value;
                    // Skip metatype entries that don't have a valid name to prevent null Value in ListItem
                    if (string.IsNullOrEmpty(strName))
                        continue;
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
                            // Skip metavariant entries that don't have a valid name
                            if (!string.IsNullOrEmpty(strMetavariantName) &&
                                (lstMetatypes.Count == 0 || lstMetatypes.TrueForAll(
                                    x => !strMetavariantName.Equals(x.Value?.ToString(),
                                                                   StringComparison.OrdinalIgnoreCase))))
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
            string strMetatypeValue = await _objContact.GetMetatypeValueForComboBoxAsync(token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strMetatypeValue))
                await cboMetatype.DoThreadSafeAsync(x => x.SelectedValue = strMetatypeValue, token: token).ConfigureAwait(false);
            if (await cboMetatype.DoThreadSafeFuncAsync(x => x.SelectedIndex, token: token).ConfigureAwait(false) < 0)
            {
                string strText = await _objContact.GetDisplayMetatypeAsync(token).ConfigureAwait(false);
                await cboMetatype.DoThreadSafeAsync(x =>
                {
                    if (x.SelectedIndex < 0)
                        x.Text = strText;
                }, token).ConfigureAwait(false);
            }

            await txtContactName.RegisterAsyncDataBindingWithDelayAsync(x => x.Text, (x, y) => x.Text = y,
                _objContact,
                nameof(Contact.Name),
                (x, y) => x.TextChanged += y,
                x => x.GetNameAsync(token),
                (x, y) => x.SetNameAsync(y, token),
                1000, token, token).ConfigureAwait(false);
            await this.RegisterOneWayAsyncDataBindingAsync((x, y) => x.BackColor = y, _objContact,
                    nameof(Contact.PreferredColor), x => x.GetPreferredColorAsync(_objMyToken), token)
                .ConfigureAwait(false);

            // Properties controllable by the character themselves
            await txtContactName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objContact,
                nameof(Contact.NoLinkedCharacter), x => x.GetNoLinkedCharacterAsync(_objMyToken), token).ConfigureAwait(false);
            await cboMetatype.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objContact,
                nameof(Contact.NoLinkedCharacter), x => x.GetNoLinkedCharacterAsync(_objMyToken), token).ConfigureAwait(false);
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
