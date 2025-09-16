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
using Chummer.Properties;
using Timer = System.Windows.Forms.Timer;

namespace Chummer
{
    public partial class ContactControl : UserControl
    {
        private readonly Contact _objContact;
        private readonly CancellationToken _objMyToken;
        private int _intLoading = 1;

        private int _intStatBlockIsLoaded;
        //private readonly int _intLowHeight = 25;
        //private readonly int _intFullHeight = 156;

        private int _intUpdatingRole = 1;
        private readonly Timer _tmrRoleChangeTimer;
        private int _intUpdatingMetatype = 1;
        private readonly Timer _tmrMetatypeChangeTimer;
        private int _intUpdatingGender = 1;
        private readonly Timer _tmrGenderChangeTimer;
        private int _intUpdatingAge = 1;
        private readonly Timer _tmrAgeChangeTimer;
        private int _intUpdatingType = 1;
        private readonly Timer _tmrTypeChangeTimer;
        private int _intUpdatingPersonalLife = 1;
        private readonly Timer _tmrPersonalLifeChangeTimer;
        private int _intUpdatingPreferredPayment = 1;
        private readonly Timer _tmrPreferredPaymentChangeTimer;
        private int _intUpdatingHobbiesVice = 1;
        private readonly Timer _tmrHobbiesViceChangeTimer;

        // Events.
        public event EventHandlerExtensions.SafeAsyncEventHandler DeleteContact;

        #region Control Events

        public ContactControl(Contact objContact, CancellationToken objMyToken)
        {
            _objContact = objContact ?? throw new ArgumentNullException(nameof(objContact));
            _objMyToken = objMyToken;

            InitializeComponent();

            _tmrRoleChangeTimer = new Timer { Interval = 1000 };
            _tmrRoleChangeTimer.Tick += UpdateContactRole;
            _tmrMetatypeChangeTimer = new Timer { Interval = 1000 };
            _tmrMetatypeChangeTimer.Tick += UpdateMetatype;
            _tmrGenderChangeTimer = new Timer { Interval = 1000 };
            _tmrGenderChangeTimer.Tick += UpdateGender;
            _tmrAgeChangeTimer = new Timer { Interval = 1000 };
            _tmrAgeChangeTimer.Tick += UpdateAge;
            _tmrTypeChangeTimer = new Timer { Interval = 1000 };
            _tmrTypeChangeTimer.Tick += UpdateType;
            _tmrPersonalLifeChangeTimer = new Timer { Interval = 1000 };
            _tmrPersonalLifeChangeTimer.Tick += UpdatePersonalLife;
            _tmrPreferredPaymentChangeTimer = new Timer { Interval = 1000 };
            _tmrPreferredPaymentChangeTimer.Tick += UpdatePreferredPayment;
            _tmrHobbiesViceChangeTimer = new Timer { Interval = 1000 };
            _tmrHobbiesViceChangeTimer.Tick += UpdateHobbiesVice;

            this.UpdateLightDarkMode(objMyToken);
            this.TranslateWinForm(token: objMyToken);

            foreach (ToolStripItem tssItem in cmsContact.Items)
            {
                tssItem.UpdateLightDarkMode(objMyToken);
                tssItem.TranslateToolStripItemsRecursively(token: objMyToken);
            }
        }

        private async void ContactControl_Load(object sender, EventArgs e)
        {
            try
            {
                if (this.IsNullOrDisposed())
                    return;
                await LoadContactList(_objMyToken).ConfigureAwait(false);

                await DoDataBindings(_objMyToken).ConfigureAwait(false);

                if (await _objContact.GetIsEnemyAsync(_objMyToken).ConfigureAwait(false))
                {
                    if (cmdLink != null)
                    {
                        string strText = !string.IsNullOrEmpty(await _objContact.GetFileNameAsync(_objMyToken).ConfigureAwait(false))
                            ? await LanguageManager.GetStringAsync("Tip_Enemy_OpenLinkedEnemy", token: _objMyToken)
                                .ConfigureAwait(false)
                            : await LanguageManager.GetStringAsync("Tip_Enemy_LinkEnemy", token: _objMyToken)
                                .ConfigureAwait(false);
                        await cmdLink.SetToolTipTextAsync(strText, _objMyToken).ConfigureAwait(false);
                    }

                    string strTooltip = await LanguageManager.GetStringAsync("Tip_Enemy_EditNotes", token: _objMyToken)
                        .ConfigureAwait(false);
                    string strNotes = await _objContact.GetNotesAsync(_objMyToken).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strNotes))
                        strTooltip += Environment.NewLine + Environment.NewLine + strNotes;
                    await cmdNotes.SetToolTipTextAsync(strTooltip.WordWrap(), _objMyToken).ConfigureAwait(false);
                }
                else
                {
                    if (cmdLink != null)
                    {
                        string strText = !string.IsNullOrEmpty(await _objContact.GetFileNameAsync(_objMyToken).ConfigureAwait(false))
                            ? await LanguageManager.GetStringAsync("Tip_Contact_OpenLinkedContact", token: _objMyToken)
                                .ConfigureAwait(false)
                            : await LanguageManager.GetStringAsync("Tip_Contact_LinkContact", token: _objMyToken)
                                .ConfigureAwait(false);
                        await cmdLink.SetToolTipTextAsync(strText, _objMyToken).ConfigureAwait(false);
                    }

                    string strTooltip = await LanguageManager
                        .GetStringAsync("Tip_Contact_EditNotes", token: _objMyToken).ConfigureAwait(false);
                    string strNotes = await _objContact.GetNotesAsync(_objMyToken).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strNotes))
                        strTooltip += Environment.NewLine + Environment.NewLine + strNotes;
                    await cmdNotes.SetToolTipTextAsync(strTooltip.WordWrap(), _objMyToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
            finally
            {
                Interlocked.Decrement(ref _intUpdatingRole);
                Interlocked.Decrement(ref _intLoading);
            }
        }

        public void UnbindContactControl()
        {
            _tmrRoleChangeTimer.Dispose();
            _tmrMetatypeChangeTimer.Dispose();
            _tmrGenderChangeTimer.Dispose();
            _tmrAgeChangeTimer.Dispose();
            _tmrTypeChangeTimer.Dispose();
            _tmrPersonalLifeChangeTimer.Dispose();
            _tmrPreferredPaymentChangeTimer.Dispose();
            _tmrHobbiesViceChangeTimer.Dispose();
            foreach (Control objControl in Controls)
                objControl.ResetBindings();
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

        private async void cmdExpand_Click(object sender, EventArgs e)
        {
            try
            {
                await SetExpandedAsync(!await GetExpandedAsync(_objMyToken).ConfigureAwait(false), _objMyToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void UpdateMetatype(object sender, EventArgs e)
        {
            if (_intLoading > 0 || _intStatBlockIsLoaded < 1)
                return;
            try
            {
                while (_intStatBlockIsLoaded == 1)
                    await Utils.SafeSleepAsync(_objMyToken).ConfigureAwait(false);
                if (_intUpdatingMetatype > 0)
                    return;
                _tmrMetatypeChangeTimer.Stop();
                string strNew = await cboMetatype.DoThreadSafeFuncAsync(x => x.Text, token: _objMyToken)
                    .ConfigureAwait(false);
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
                        await cboMetatype.DoThreadSafeAsync(x => x.Text = strOld, token: _objMyToken)
                            .ConfigureAwait(false);
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

        private async void UpdateGender(object sender, EventArgs e)
        {
            if (_intLoading > 0 || _intStatBlockIsLoaded < 1)
                return;
            try
            {
                while (_intStatBlockIsLoaded == 1)
                    await Utils.SafeSleepAsync(_objMyToken).ConfigureAwait(false);
                if (_intUpdatingGender > 0)
                    return;
                _tmrGenderChangeTimer.Stop();
                string strNew = await cboGender.DoThreadSafeFuncAsync(x => x.Text, token: _objMyToken)
                    .ConfigureAwait(false);
                string strOld = await _objContact.GetDisplayGenderAsync(_objMyToken).ConfigureAwait(false);
                if (strOld == strNew)
                    return;
                await _objContact.SetDisplayGenderAsync(strNew, _objMyToken).ConfigureAwait(false);
                strOld = await _objContact.GetDisplayGenderAsync(_objMyToken).ConfigureAwait(false);
                if (strOld != strNew)
                {
                    Interlocked.Increment(ref _intUpdatingGender);
                    try
                    {
                        await cboGender.DoThreadSafeAsync(x => x.Text = strOld, token: _objMyToken)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intUpdatingGender);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void UpdateAge(object sender, EventArgs e)
        {
            if (_intLoading > 0 || _intStatBlockIsLoaded <= 1)
                return;
            try
            {
                while (_intStatBlockIsLoaded == 1)
                    await Utils.SafeSleepAsync(_objMyToken).ConfigureAwait(false);
                if (_intUpdatingAge > 0)
                    return;
                _tmrAgeChangeTimer.Stop();
                string strNew = await cboAge.DoThreadSafeFuncAsync(x => x.Text, token: _objMyToken)
                    .ConfigureAwait(false);
                string strOld = await _objContact.GetDisplayAgeAsync(_objMyToken).ConfigureAwait(false);
                if (strOld == strNew)
                    return;
                await _objContact.SetDisplayAgeAsync(strNew, _objMyToken).ConfigureAwait(false);
                strOld = await _objContact.GetDisplayAgeAsync(_objMyToken).ConfigureAwait(false);
                if (strOld != strNew)
                {
                    Interlocked.Increment(ref _intUpdatingAge);
                    try
                    {
                        await cboAge.DoThreadSafeAsync(x => x.Text = strOld, token: _objMyToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intUpdatingAge);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void UpdatePersonalLife(object sender, EventArgs e)
        {
            if (_intLoading > 0 || _intStatBlockIsLoaded < 1)
                return;
            try
            {
                while (_intStatBlockIsLoaded == 1)
                    await Utils.SafeSleepAsync(_objMyToken).ConfigureAwait(false);
                if (_intUpdatingPersonalLife > 0)
                    return;
                _tmrPersonalLifeChangeTimer.Stop();
                string strNew = await cboPersonalLife.DoThreadSafeFuncAsync(x => x.Text, token: _objMyToken)
                    .ConfigureAwait(false);
                string strOld = await _objContact.GetDisplayPersonalLifeAsync(_objMyToken).ConfigureAwait(false);
                if (strOld == strNew)
                    return;
                await _objContact.SetDisplayPersonalLifeAsync(strNew, _objMyToken).ConfigureAwait(false);
                strOld = await _objContact.GetDisplayPersonalLifeAsync(_objMyToken).ConfigureAwait(false);
                if (strOld != strNew)
                {
                    Interlocked.Increment(ref _intUpdatingPersonalLife);
                    try
                    {
                        await cboPersonalLife.DoThreadSafeAsync(x => x.Text = strOld, token: _objMyToken)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intUpdatingPersonalLife);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void UpdateType(object sender, EventArgs e)
        {
            if (_intLoading > 0 || _intStatBlockIsLoaded < 1)
                return;
            try
            {
                while (_intStatBlockIsLoaded == 1)
                    await Utils.SafeSleepAsync(_objMyToken).ConfigureAwait(false);
                if (_intUpdatingType > 0)
                    return;
                _tmrTypeChangeTimer.Stop();
                string strNew = await cboType.DoThreadSafeFuncAsync(x => x.Text, token: _objMyToken)
                    .ConfigureAwait(false);
                string strOld = await _objContact.GetDisplayTypeAsync(_objMyToken).ConfigureAwait(false);
                if (strOld == strNew)
                    return;
                await _objContact.SetDisplayTypeAsync(strNew, _objMyToken).ConfigureAwait(false);
                strOld = await _objContact.GetDisplayTypeAsync(_objMyToken).ConfigureAwait(false);
                if (strOld != strNew)
                {
                    Interlocked.Increment(ref _intUpdatingType);
                    try
                    {
                        await cboType.DoThreadSafeAsync(x => x.Text = strOld, token: _objMyToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intUpdatingType);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void UpdatePreferredPayment(object sender, EventArgs e)
        {
            if (_intLoading > 0 || _intStatBlockIsLoaded < 1)
                return;
            try
            {
                while (_intStatBlockIsLoaded == 1)
                    await Utils.SafeSleepAsync(_objMyToken).ConfigureAwait(false);
                if (_intUpdatingPreferredPayment > 0)
                    return;
                _tmrPreferredPaymentChangeTimer.Stop();
                string strNew = await cboPreferredPayment.DoThreadSafeFuncAsync(x => x.Text, token: _objMyToken)
                    .ConfigureAwait(false);
                string strOld = await _objContact.GetDisplayPreferredPaymentAsync(_objMyToken).ConfigureAwait(false);
                if (strOld == strNew)
                    return;
                await _objContact.SetDisplayPreferredPaymentAsync(strNew, _objMyToken).ConfigureAwait(false);
                strOld = await _objContact.GetDisplayPreferredPaymentAsync(_objMyToken).ConfigureAwait(false);
                if (strOld != strNew)
                {
                    Interlocked.Increment(ref _intUpdatingPreferredPayment);
                    try
                    {
                        await cboPreferredPayment.DoThreadSafeAsync(x => x.Text = strOld, token: _objMyToken)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intUpdatingPreferredPayment);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void UpdateHobbiesVice(object sender, EventArgs e)
        {
            if (_intLoading > 0 || _intStatBlockIsLoaded < 1)
                return;
            try
            {
                while (_intStatBlockIsLoaded == 1)
                    await Utils.SafeSleepAsync(_objMyToken).ConfigureAwait(false);
                if (_intUpdatingHobbiesVice > 0)
                    return;
                _tmrHobbiesViceChangeTimer.Stop();
                string strNew = await cboHobbiesVice.DoThreadSafeFuncAsync(x => x.Text, token: _objMyToken)
                    .ConfigureAwait(false);
                string strOld = await _objContact.GetDisplayHobbiesViceAsync(_objMyToken).ConfigureAwait(false);
                if (strOld == strNew)
                    return;
                await _objContact.SetDisplayHobbiesViceAsync(strNew, _objMyToken).ConfigureAwait(false);
                strOld = await _objContact.GetDisplayHobbiesViceAsync(_objMyToken).ConfigureAwait(false);
                if (strOld != strNew)
                {
                    Interlocked.Increment(ref _intUpdatingHobbiesVice);
                    try
                    {
                        await cboHobbiesVice.DoThreadSafeAsync(x => x.Text = strOld, token: _objMyToken)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intUpdatingHobbiesVice);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void UpdateContactRole(object sender, EventArgs e)
        {
            if (_intLoading > 0 || _intUpdatingRole > 0)
                return;
            _tmrRoleChangeTimer.Stop();
            try
            {
                string strNew = await cboContactRole.DoThreadSafeFuncAsync(x => x.Text, token: _objMyToken)
                    .ConfigureAwait(false);
                string strOld = await _objContact.GetDisplayRoleAsync(_objMyToken).ConfigureAwait(false);
                if (strOld == strNew)
                    return;
                await _objContact.SetDisplayRoleAsync(strNew, _objMyToken).ConfigureAwait(false);
                strOld = await _objContact.GetDisplayRoleAsync(_objMyToken).ConfigureAwait(false);
                if (strOld != strNew)
                {
                    Interlocked.Increment(ref _intUpdatingRole);
                    try
                    {
                        await cboContactRole.DoThreadSafeAsync(x => x.Text = strOld, token: _objMyToken)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intUpdatingRole);
                    }
                }
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
                string strFilter =
                    await LanguageManager.GetStringAsync("DialogFilter_Chummer", token: _objMyToken)
                        .ConfigureAwait(false) +
                    '|' +
                    await LanguageManager.GetStringAsync("DialogFilter_Chum5", token: _objMyToken)
                        .ConfigureAwait(false) +
                    '|' + await LanguageManager.GetStringAsync("DialogFilter_Chum5lz", token: _objMyToken)
                        .ConfigureAwait(false) + '|' + await LanguageManager
                        .GetStringAsync("DialogFilter_All", token: _objMyToken).ConfigureAwait(false);
                IAsyncDisposable objLocker = await _objContact.LockObject.EnterUpgradeableReadLockAsync(_objMyToken).ConfigureAwait(false);
                try
                {
                    _objMyToken.ThrowIfCancellationRequested();
                    string strOldFileName = await _objContact.GetFileNameAsync(_objMyToken).ConfigureAwait(false);
                    string strFileName = string.Empty;
                    DialogResult eResult = await this.DoThreadSafeFuncAsync(x =>
                    {
                        // Prompt the user to select a save file to associate with this Contact.
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
                    }, token: _objMyToken).ConfigureAwait(false);
                    if (eResult != DialogResult.OK)
                        return;
                    if (cmdLink != null)
                    {
                        string strText = await _objContact.GetIsEnemyAsync(_objMyToken).ConfigureAwait(false)
                            ? await LanguageManager.GetStringAsync("Tip_Enemy_OpenFile", token: _objMyToken)
                                .ConfigureAwait(false)
                            : await LanguageManager.GetStringAsync("Tip_Contact_OpenFile", token: _objMyToken)
                                .ConfigureAwait(false);
                        await cmdLink.SetToolTipTextAsync(strText, _objMyToken).ConfigureAwait(false);
                    }

                    // Set the relative path.
                    Uri uriApplication = new Uri(Utils.GetStartupPath);
                    Uri uriFile = new Uri(strFileName);
                    Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);

                    IAsyncDisposable objLocker2 = await _objContact.LockObject.EnterWriteLockAsync(_objMyToken).ConfigureAwait(false);
                    try
                    {
                        _objMyToken.ThrowIfCancellationRequested();
                        await _objContact.SetFileNameAsync(strFileName, _objMyToken).ConfigureAwait(false);
                        await _objContact.SetRelativeFileNameAsync("../" + uriRelative, _objMyToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
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
                        await LanguageManager
                            .GetStringAsync("MessageTitle_RemoveCharacterAssociation", token: _objMyToken)
                            .ConfigureAwait(false),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, token: _objMyToken).ConfigureAwait(false) == DialogResult.Yes)
                {
                    if (cmdLink != null)
                    {
                        string strText = await _objContact.GetIsEnemyAsync(_objMyToken).ConfigureAwait(false)
                            ? await LanguageManager.GetStringAsync("Tip_Enemy_LinkFile", token: _objMyToken)
                                .ConfigureAwait(false)
                            : await LanguageManager.GetStringAsync("Tip_Contact_LinkFile", token: _objMyToken)
                                .ConfigureAwait(false);
                        await cmdLink.SetToolTipTextAsync(strText, _objMyToken).ConfigureAwait(false);
                    }
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
                using (ThreadSafeForm<EditNotes> frmContactNotes
                       = await ThreadSafeForm<EditNotes>.GetAsync(
                               () => new EditNotes(strNotes, objColor, _objMyToken), _objMyToken)
                           .ConfigureAwait(false))
                {
                    if (await frmContactNotes.ShowDialogSafeAsync(this, _objMyToken).ConfigureAwait(false) !=
                        DialogResult.OK)
                        return;
                    await _objContact.SetNotesAsync(frmContactNotes.MyForm.Notes, _objMyToken).ConfigureAwait(false);
                    await _objContact.SetNotesColorAsync(frmContactNotes.MyForm.NotesColor, _objMyToken).ConfigureAwait(false);
                }

                string strTooltip
                    = await LanguageManager.GetStringAsync(await _objContact.GetIsEnemyAsync(_objMyToken).ConfigureAwait(false)
                        ? "Tip_Enemy_EditNotes"
                        : "Tip_Contact_EditNotes", token: _objMyToken).ConfigureAwait(false);
                strNotes = await _objContact.GetNotesAsync(_objMyToken).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strNotes))
                    strTooltip += Environment.NewLine + Environment.NewLine + strNotes;
                await cmdNotes.SetToolTipTextAsync(strTooltip.WordWrap(), _objMyToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Contact object this is linked to.
        /// </summary>
        public Contact ContactObject => _objContact;

        public async Task<bool> GetExpandedAsync(CancellationToken token = default)
        {
            if (tlpStatBlock != null)
                return await tlpStatBlock.DoThreadSafeFuncAsync(x => x.Visible, token).ConfigureAwait(false);
            return false;
        }

        public async Task SetExpandedAsync(bool value, CancellationToken token = default)
        {
            await cmdExpand.DoThreadSafeAsync(x =>
            {
                if (value)
                {
                    x.BatchSetImages(Resources.toggle_16, Resources.toggle_20, Resources.toggle_24, Resources.toggle_32,
                        Resources.toggle_48, Resources.toggle_64);
                }
                else
                {
                    x.BatchSetImages(Resources.toggle_expand_16, Resources.toggle_expand_20, Resources.toggle_expand_24, Resources.toggle_expand_32,
                        Resources.toggle_expand_48, Resources.toggle_expand_64);
                }
            }, token).ConfigureAwait(false);
            if (value)
            {
                int intOld = Interlocked.CompareExchange(ref _intStatBlockIsLoaded, 1, 0);
                try
                {
                    while (tlpStatBlock == null || intOld < 2)
                    {
                        while (intOld == 1)
                        {
                            await Utils.SafeSleepAsync(token).ConfigureAwait(false);
                            intOld = Interlocked.CompareExchange(ref _intStatBlockIsLoaded, 1, 0);
                        }

                        if (tlpStatBlock == null || intOld < 2)
                        {
                            // Create second row and statblock only on the first expansion to save on handles and load times
                            await CreateSecondRowAsync(token).ConfigureAwait(false);
                            await CreateStatBlockAsync(token).ConfigureAwait(false);
                            intOld = Interlocked.CompareExchange(ref _intStatBlockIsLoaded, 2, 1);
                        }
                    }
                }
                catch
                {
                    Interlocked.CompareExchange(ref _intStatBlockIsLoaded, intOld, 1);
                }
            }

            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                try
                {
                    await lblConnection.DoThreadSafeAsync(x => x.Visible = value, token).ConfigureAwait(false);
                    await lblLoyalty.DoThreadSafeAsync(x => x.Visible = value, token).ConfigureAwait(false);
                    await nudConnection.DoThreadSafeAsync(x => x.Visible = value, token).ConfigureAwait(false);
                    await nudLoyalty.DoThreadSafeAsync(x => x.Visible = value, token).ConfigureAwait(false);
                    await chkGroup.DoThreadSafeAsync(x => x.Visible = value, token).ConfigureAwait(false);
                    //We don't actually pay for contacts in play so everyone is free
                    //Don't present a useless field
                    if (value && _objContact != null)
                    {
                        bool blnCreated = await _objContact.CharacterObject.GetCreatedAsync(token).ConfigureAwait(false);
                        await chkFree.DoThreadSafeAsync(x => x.Visible = !blnCreated, token: token).ConfigureAwait(false);
                    }
                    else
                        await chkFree.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                    await chkBlackmail.DoThreadSafeAsync(x => x.Visible = value, token).ConfigureAwait(false);
                    await chkFamily.DoThreadSafeAsync(x => x.Visible = value, token).ConfigureAwait(false);
                    await cmdLink.DoThreadSafeAsync(x => x.Visible = value, token).ConfigureAwait(false);
                    await tlpStatBlock.DoThreadSafeAsync(x => x.Visible = value, token).ConfigureAwait(false);
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout(), token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Properties

        #region Methods

        private async Task LoadContactList(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (await _objContact.GetIsEnemyAsync(token).ConfigureAwait(false))
            {
                string strContactRole = await _objContact.GetDisplayRoleAsync(token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strContactRole))
                    await cboContactRole.DoThreadSafeAsync(x => x.Text = strContactRole, token: token)
                                        .ConfigureAwait(false);
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

            await cboContactRole
                  .PopulateWithListItemsAsync(
                      await _objContact.CharacterObject.ContactArchetypesAsync(token: token).ConfigureAwait(false),
                      token: token).ConfigureAwait(false);
            if (await cboContactRole.DoThreadSafeFuncAsync(x =>
                {
                    x.SelectedValue = _objContact.Role;
                    return x.SelectedIndex;
                }, token: token).ConfigureAwait(false) < 0)
            {
                string strDisplayRole = await _objContact.GetDisplayRoleAsync(token: token).ConfigureAwait(false);
                await cboContactRole.DoThreadSafeAsync(x =>
                {
                    if (x.SelectedIndex < 0)
                        x.Text = strDisplayRole;
                }, token).ConfigureAwait(false);
            }
        }

        private async Task DoDataBindings(CancellationToken token = default)
        {
            await lblQuickStats.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objContact,
                                                               nameof(Contact.QuickText),
                                                               // ReSharper disable once MethodSupportsCancellation
                                                               x => x.GetQuickTextAsync(token), token: token).ConfigureAwait(false);
            await txtContactName.RegisterAsyncDataBindingWithDelayAsync(x => x.Text, (x, y) => x.Text = y,
                _objContact,
                nameof(Contact.Name),
                (x, y) => x.TextChanged += y,
                x => x.GetNameAsync(token),
                (x, y) => x.SetNameAsync(y, token),
                1000, token, token).ConfigureAwait(false);
            await txtContactLocation.RegisterAsyncDataBindingWithDelayAsync(x => x.Text, (x, y) => x.Text = y,
                _objContact,
                nameof(Contact.Location),
                (x, y) => x.TextChanged += y,
                x => x.GetLocationAsync(token),
                (x, y) => x.SetLocationAsync(y, token),
                1000, token, token).ConfigureAwait(false);
            await cmdDelete.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = !y, _objContact,
                nameof(Contact.ReadOnly), x => x.GetReadOnlyAsync(_objMyToken), token).ConfigureAwait(false);
            await this.RegisterOneWayAsyncDataBindingAsync((x, y) => x.BackColor = y, _objContact,
                    nameof(Contact.PreferredColor), x => x.GetPreferredColorAsync(_objMyToken), token)
                .ConfigureAwait(false);

            // Properties controllable by the character themselves
            await txtContactName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objContact,
                nameof(Contact.NoLinkedCharacter), x => x.GetNoLinkedCharacterAsync(_objMyToken), token).ConfigureAwait(false);
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

        private async Task CreateSecondRowAsync(CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                await this.DoThreadSafeAsync(x =>
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
                        Maximum = 12m,
                        Minimum = decimal.One,
                        Name = "nudConnection",
                        Value = decimal.One
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
                        Maximum = 6m,
                        Minimum = decimal.One,
                        Name = "nudLoyalty",
                        Value = decimal.One
                    };
                    x.chkFree = new ColorableCheckBox
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        DefaultColorScheme = true,
                        Name = "chkFree",
                        Tag = "Checkbox_Contact_Free",
                        Text = "Free",
                        UseVisualStyleBackColor = true
                    };
                    x.chkGroup = new ColorableCheckBox
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        DefaultColorScheme = true,
                        Name = "chkGroup",
                        Tag = "Checkbox_Contact_Group",
                        Text = "Group",
                        UseVisualStyleBackColor = true
                    };
                    x.chkBlackmail = new ColorableCheckBox
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        DefaultColorScheme = true,
                        Name = "chkBlackmail",
                        Tag = "Checkbox_Contact_Blackmail",
                        Text = "Blackmail",
                        UseVisualStyleBackColor = true
                    };
                    x.chkFamily = new ColorableCheckBox
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        DefaultColorScheme = true,
                        Name = "chkFamily",
                        Tag = "Checkbox_Contact_Family",
                        Text = "Family",
                        UseVisualStyleBackColor = true
                    };
                    x.cmdLink = new ButtonWithToolTip
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        FlatAppearance = {BorderSize = 0},
                        FlatStyle = FlatStyle.Flat,
                        Padding = new Padding(1),
                        MinimumSize = new Size(24, 24),
                        Name = "cmdLink",
                        UseVisualStyleBackColor = true,
                        TabStop = false
                    };
                    x.cmdLink.BatchSetImages(Resources.link_16, Resources.link_20, Resources.link_24, Resources.link_32,
                        Resources.link_48, Resources.link_64);
                    x.cmdLink.Click += cmdLink_Click;
                }, token).ConfigureAwait(false);
                if (_objContact != null)
                {
                    //We don't actually pay for contacts in play so everyone is free
                    //Don't present a useless field
                    if (_objContact.CharacterObject != null)
                    {
                        await chkFree.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = !y, _objContact.CharacterObject,
                            nameof(Character.Created), x => x.GetCreatedAsync(_objMyToken), token).ConfigureAwait(false);
                    }
                    else
                        await chkFree.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                    await chkGroup.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y, _objContact,
                        nameof(Contact.IsGroup),
                        (x, y) => x.CheckedChanged += y,
                        x => x.GetIsGroupAsync(_objMyToken),
                        (x, y) => x.SetIsGroupAsync(y, _objMyToken),
                        _objMyToken,
                        token).ConfigureAwait(false);
                    await chkFree.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y, _objContact,
                        nameof(Contact.IsGroup),
                        (x, y) => x.CheckedChanged += y,
                        x => x.GetFreeAsync(_objMyToken),
                        (x, y) => x.SetFreeAsync(y, _objMyToken),
                        _objMyToken,
                        token).ConfigureAwait(false);
                    await chkFamily.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y, _objContact,
                        nameof(Contact.Family),
                        (x, y) => x.CheckedChanged += y,
                        x => x.GetFamilyAsync(_objMyToken),
                        (x, y) => x.SetFamilyAsync(y, _objMyToken),
                        _objMyToken,
                        token).ConfigureAwait(false);
                    await chkFamily.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = !y, _objContact,
                            nameof(Contact.ReadOnly), x => x.GetIsEnemyAsync(_objMyToken), token)
                        .ConfigureAwait(false);
                    await chkBlackmail.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y,
                        _objContact,
                        nameof(Contact.Blackmail),
                        (x, y) => x.CheckedChanged += y,
                        x => x.GetBlackmailAsync(_objMyToken),
                        (x, y) => x.SetBlackmailAsync(y, _objMyToken),
                        _objMyToken,
                        token).ConfigureAwait(false);
                    await chkBlackmail.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = !y, _objContact,
                            nameof(Contact.ReadOnly), x => x.GetIsEnemyAsync(_objMyToken), token)
                        .ConfigureAwait(false);
                    await nudLoyalty.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y,
                        _objContact,
                        nameof(Contact.Loyalty),
                        (x, y) => x.ValueChanged += y,
                        x => x.GetLoyaltyAsync(_objMyToken),
                        (x, y) => x.SetLoyaltyAsync(y, _objMyToken),
                        250,
                        _objMyToken,
                        token).ConfigureAwait(false);
                    await nudConnection.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y,
                        _objContact,
                        nameof(Contact.Connection),
                        (x, y) => x.ValueChanged += y,
                        x => x.GetConnectionAsync(_objMyToken),
                        (x, y) => x.SetConnectionAsync(y, _objMyToken),
                        250,
                        _objMyToken,
                        token).ConfigureAwait(false);
                    await nudConnection.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = !y, _objContact,
                            nameof(Contact.ReadOnly), x => x.GetReadOnlyAsync(_objMyToken), token)
                        .ConfigureAwait(false);
                    await chkGroup.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objContact,
                                                                       nameof(Contact.GroupEnabled),
                                                                       // ReSharper disable once MethodSupportsCancellation
                                                                       x => x.GetGroupEnabledAsync(token),
                                                                       token: token).ConfigureAwait(false);
                    await chkFree.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objContact,
                                                                      nameof(Contact.FreeEnabled),
                                                                      // ReSharper disable once MethodSupportsCancellation
                                                                      x => x.GetFreeEnabledAsync(token),
                                                                      token: token).ConfigureAwait(false);
                    await nudLoyalty.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objContact,
                                                                         nameof(Contact.LoyaltyEnabled),
                                                                         // ReSharper disable once MethodSupportsCancellation
                                                                         x => x.GetLoyaltyEnabledAsync(token),
                                                                         token: token).ConfigureAwait(false);
                    await nudConnection.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _objContact,
                                                                            nameof(Contact.ConnectionMaximum),
                                                                            // ReSharper disable once MethodSupportsCancellation
                                                                            x => x.GetConnectionMaximumAsync(token),
                                                                            token: token).ConfigureAwait(false);
                    string strToolTipText;
                    if (await _objContact.GetIsEnemyAsync(token).ConfigureAwait(false))
                    {
                        strToolTipText = !string.IsNullOrEmpty(await _objContact.GetFileNameAsync(token).ConfigureAwait(false))
                            ? await LanguageManager.GetStringAsync("Tip_Enemy_OpenLinkedEnemy", token: token)
                                                   .ConfigureAwait(false)
                            : await LanguageManager.GetStringAsync("Tip_Enemy_LinkEnemy", token: token)
                                                   .ConfigureAwait(false);
                    }
                    else
                    {
                        strToolTipText = !string.IsNullOrEmpty(await _objContact.GetFileNameAsync(token).ConfigureAwait(false))
                            ? await LanguageManager.GetStringAsync("Tip_Contact_OpenLinkedContact", token: token)
                                                   .ConfigureAwait(false)
                            : await LanguageManager.GetStringAsync("Tip_Contact_LinkContact", token: token)
                                                   .ConfigureAwait(false);
                    }

                    await cmdLink.DoThreadSafeAsync(x => x.ToolTipText = strToolTipText, token).ConfigureAwait(false);
                }

                await this.DoThreadSafeAsync(x =>
                {
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
                }, token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private TableLayoutPanel tlpStatBlock;
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
        private void CreateStatBlock(CancellationToken token = default)
        {
            using (CursorWait.New(this))
            {
                this.DoThreadSafe(x =>
                {
                    x.cboMetatype = new ElasticComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        FormattingEnabled = true,
                        Name = "cboMetatype"
                    };
                    x.cboGender = new ElasticComboBox
                    { Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboGender" };
                    x.cboAge = new ElasticComboBox
                    { Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboAge" };
                    x.cboType = new ElasticComboBox
                    { Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboType" };
                    x.cboPersonalLife = new ElasticComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        FormattingEnabled = true,
                        Name = "cboPersonalLife"
                    };
                    x.cboPreferredPayment = new ElasticComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        FormattingEnabled = true,
                        Name = "cboPreferredPayment"
                    };
                    x.cboHobbiesVice = new ElasticComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        FormattingEnabled = true,
                        Name = "cboHobbiesVice"
                    };
                }, token: token);

                LoadStatBlockLists(token);

                if (_objContact != null)
                {
                    cboMetatype.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objContact,
                        nameof(Contact.NoLinkedCharacter), x => x.GetNoLinkedCharacterAsync(_objMyToken), token: token);
                    cboGender.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objContact,
                        nameof(Contact.NoLinkedCharacter), x => x.GetNoLinkedCharacterAsync(_objMyToken), token: token);
                    cboAge.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objContact,
                        nameof(Contact.NoLinkedCharacter), x => x.GetNoLinkedCharacterAsync(_objMyToken), token: token);
                }

                this.DoThreadSafe(x =>
                {
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

                    x.tlpStatBlock = new TableLayoutPanel
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

                    x.tlpStatBlock.TranslateWinForm(token: token);
                    x.tlpStatBlock.UpdateLightDarkMode(token: token);

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

                    x.cboMetatype.TextChanged += MetatypeOnTextChanged;
                    x.cboGender.TextChanged += GenderOnTextChanged;
                    x.cboAge.TextChanged += AgeOnTextChanged;
                    x.cboType.TextChanged += TypeOnTextChanged;
                    x.cboPersonalLife.TextChanged += PersonalLifeOnTextChanged;
                    x.cboPreferredPayment.TextChanged += PreferredPaymentOnTextChanged;
                    x.cboHobbiesVice.TextChanged += HobbiesViceOnTextChanged;
                }, token: token);

                Interlocked.Decrement(ref _intUpdatingMetatype);
                Interlocked.Decrement(ref _intUpdatingGender);
                Interlocked.Decrement(ref _intUpdatingAge);
                Interlocked.Decrement(ref _intUpdatingType);
                Interlocked.Decrement(ref _intUpdatingPersonalLife);
                Interlocked.Decrement(ref _intUpdatingPreferredPayment);
                Interlocked.Decrement(ref _intUpdatingHobbiesVice);
            }
        }

        /// <summary>
        /// Method to dynamically create stat block is separated out so that we only create it if the control is expanded
        /// </summary>
        private async Task CreateStatBlockAsync(CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                await this.DoThreadSafeAsync(x =>
                {
                    x.cboMetatype = new ElasticComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        FormattingEnabled = true,
                        Name = "cboMetatype"
                    };
                    x.cboGender = new ElasticComboBox
                        {Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboGender"};
                    x.cboAge = new ElasticComboBox
                        {Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboAge"};
                    x.cboType = new ElasticComboBox
                        {Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true, Name = "cboType"};
                    x.cboPersonalLife = new ElasticComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        FormattingEnabled = true,
                        Name = "cboPersonalLife"
                    };
                    x.cboPreferredPayment = new ElasticComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        FormattingEnabled = true,
                        Name = "cboPreferredPayment"
                    };
                    x.cboHobbiesVice = new ElasticComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        FormattingEnabled = true,
                        Name = "cboHobbiesVice"
                    };
                }, token).ConfigureAwait(false);

                await LoadStatBlockListsAsync(token).ConfigureAwait(false);

                if (_objContact != null)
                {
                    await cboMetatype.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objContact,
                        nameof(Contact.NoLinkedCharacter), x => x.GetNoLinkedCharacterAsync(_objMyToken), token: token).ConfigureAwait(false);
                    await cboGender.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objContact,
                        nameof(Contact.NoLinkedCharacter), x => x.GetNoLinkedCharacterAsync(_objMyToken), token: token).ConfigureAwait(false);
                    await cboAge.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objContact,
                        nameof(Contact.NoLinkedCharacter), x => x.GetNoLinkedCharacterAsync(_objMyToken), token: token).ConfigureAwait(false);
                }

                await this.DoThreadSafeAsync(x =>
                {
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

                    x.tlpStatBlock = new TableLayoutPanel
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
                }, token).ConfigureAwait(false);
                await tlpStatBlock.TranslateWinFormAsync(token: token).ConfigureAwait(false);
                await tlpStatBlock.UpdateLightDarkModeAsync(token: token).ConfigureAwait(false);
                await this.DoThreadSafeAsync(x =>
                {
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
                }, token: token).ConfigureAwait(false);

                // Need these as separate instead of as simple data bindings so that we don't get annoying live partial translations
                if (_objContact != null)
                {
                    string strMetatype = await _objContact.GetMetatypeAsync(token).ConfigureAwait(false);
                    string strGender = await _objContact.GetGenderAsync(token).ConfigureAwait(false);
                    string strAge = await _objContact.GetAgeAsync(token).ConfigureAwait(false);
                    string strPersonalLife = await _objContact.GetPersonalLifeAsync(token).ConfigureAwait(false);
                    string strType = await _objContact.GetTypeAsync(token).ConfigureAwait(false);
                    string strPreferredPayment = await _objContact.GetPreferredPaymentAsync(token).ConfigureAwait(false);
                    string strHobbiesVice = await _objContact.GetHobbiesViceAsync(token).ConfigureAwait(false);
                    await this.DoThreadSafeAsync(x =>
                    {
                        if (!string.IsNullOrEmpty(strMetatype))
                            x.cboMetatype.SelectedValue = strMetatype;
                        if (!string.IsNullOrEmpty(strGender))
                            x.cboGender.SelectedValue = strGender;
                        if (!string.IsNullOrEmpty(strAge))
                            x.cboAge.SelectedValue = strAge;
                        if (!string.IsNullOrEmpty(strPersonalLife))
                            x.cboPersonalLife.SelectedValue = strPersonalLife;
                        if (!string.IsNullOrEmpty(strType))
                            x.cboType.SelectedValue = strType;
                        if (!string.IsNullOrEmpty(strPreferredPayment))
                            x.cboPreferredPayment.SelectedValue = strPreferredPayment;
                        if (!string.IsNullOrEmpty(strHobbiesVice))
                            x.cboHobbiesVice.SelectedValue = strHobbiesVice;
                    }, token: token).ConfigureAwait(false);
                    if (await cboMetatype.DoThreadSafeFuncAsync(x => x.SelectedIndex, token: token)
                                         .ConfigureAwait(false) < 0)
                    {
                        string strTemp = await _objContact.GetDisplayMetatypeAsync(token).ConfigureAwait(false);
                        await cboMetatype.DoThreadSafeAsync(x =>
                        {
                            if (x.SelectedIndex < 0)
                                x.Text = strTemp;
                        }, token: token).ConfigureAwait(false);
                    }

                    if (await cboGender.DoThreadSafeFuncAsync(x => x.SelectedIndex, token: token).ConfigureAwait(false)
                        < 0)
                    {
                        string strTemp = await _objContact.GetDisplayGenderAsync(token).ConfigureAwait(false);
                        await cboGender.DoThreadSafeAsync(x =>
                        {
                            if (x.SelectedIndex < 0)
                                x.Text = strTemp;
                        }, token: token).ConfigureAwait(false);
                    }

                    if (await cboAge.DoThreadSafeFuncAsync(x => x.SelectedIndex, token: token).ConfigureAwait(false)
                        < 0)
                    {
                        string strTemp = await _objContact.GetDisplayAgeAsync(token).ConfigureAwait(false);
                        await cboAge.DoThreadSafeAsync(x =>
                        {
                            if (x.SelectedIndex < 0)
                                x.Text = strTemp;
                        }, token: token).ConfigureAwait(false);
                    }

                    if (await cboPersonalLife.DoThreadSafeFuncAsync(x => x.SelectedIndex, token: token)
                                             .ConfigureAwait(false) < 0)
                    {
                        string strTemp = await _objContact.GetDisplayPersonalLifeAsync(token).ConfigureAwait(false);
                        await cboPersonalLife.DoThreadSafeAsync(x =>
                            {
                                if (x.SelectedIndex < 0)
                                    x.Text = strTemp;
                            }, token: token).ConfigureAwait(false);
                    }

                    if (await cboType.DoThreadSafeFuncAsync(x => x.SelectedIndex, token: token).ConfigureAwait(false)
                        < 0)
                    {
                        string strTemp = await _objContact.GetDisplayTypeAsync(token).ConfigureAwait(false);
                        await cboType.DoThreadSafeAsync(x =>
                        {
                            if (x.SelectedIndex < 0)
                                x.Text = strTemp;
                        }, token: token).ConfigureAwait(false);
                    }

                    if (await cboPreferredPayment.DoThreadSafeFuncAsync(x => x.SelectedIndex, token: token)
                                                 .ConfigureAwait(false) < 0)
                    {
                        string strTemp = await _objContact.GetDisplayTypeAsync(token).ConfigureAwait(false);
                        await cboPreferredPayment.DoThreadSafeAsync(x =>
                            {
                                if (x.SelectedIndex < 0)
                                    x.Text = strTemp;
                            }, token: token).ConfigureAwait(false);
                    }

                    if (await cboHobbiesVice.DoThreadSafeFuncAsync(x => x.SelectedIndex, token: token)
                                            .ConfigureAwait(false) < 0)
                    {
                        string strTemp = await _objContact.GetDisplayTypeAsync(token).ConfigureAwait(false);
                        await cboHobbiesVice.DoThreadSafeAsync(x =>
                            {
                                if (x.SelectedIndex < 0)
                                    x.Text = strTemp;
                            }, token: token).ConfigureAwait(false);
                    }
                }

                // Need these as separate instead of as simple data bindings so that we don't get annoying live partial translations
                await this.DoThreadSafeAsync(x =>
                {
                    x.cboMetatype.TextChanged += MetatypeOnTextChanged;
                    x.cboGender.TextChanged += GenderOnTextChanged;
                    x.cboAge.TextChanged += AgeOnTextChanged;
                    x.cboType.TextChanged += TypeOnTextChanged;
                    x.cboPersonalLife.TextChanged += PersonalLifeOnTextChanged;
                    x.cboPreferredPayment.TextChanged += PreferredPaymentOnTextChanged;
                    x.cboHobbiesVice.TextChanged += HobbiesViceOnTextChanged;
                }, token).ConfigureAwait(false);

                Interlocked.Decrement(ref _intUpdatingMetatype);
                Interlocked.Decrement(ref _intUpdatingGender);
                Interlocked.Decrement(ref _intUpdatingAge);
                Interlocked.Decrement(ref _intUpdatingType);
                Interlocked.Decrement(ref _intUpdatingPersonalLife);
                Interlocked.Decrement(ref _intUpdatingPreferredPayment);
                Interlocked.Decrement(ref _intUpdatingHobbiesVice);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void cboContactRole_TextChanged(object sender, EventArgs e)
        {
            if (_tmrRoleChangeTimer == null)
                return;
            if (_tmrRoleChangeTimer.Enabled)
                _tmrRoleChangeTimer.Stop();
            if (_intUpdatingRole > 0)
                return;
            _tmrRoleChangeTimer.Start();
        }

        private void MetatypeOnTextChanged(object sender, EventArgs e)
        {
            if (_tmrMetatypeChangeTimer == null)
                return;
            if (_tmrMetatypeChangeTimer.Enabled)
                _tmrMetatypeChangeTimer.Stop();
            if (_intUpdatingMetatype > 0)
                return;
            _tmrMetatypeChangeTimer.Start();
        }

        private void GenderOnTextChanged(object sender, EventArgs e)
        {
            if (_tmrGenderChangeTimer == null)
                return;
            if (_tmrGenderChangeTimer.Enabled)
                _tmrGenderChangeTimer.Stop();
            if (_intUpdatingGender > 0)
                return;
            _tmrGenderChangeTimer.Start();
        }

        private void AgeOnTextChanged(object sender, EventArgs e)
        {
            if (_tmrAgeChangeTimer == null)
                return;
            if (_tmrAgeChangeTimer.Enabled)
                _tmrAgeChangeTimer.Stop();
            if (_intUpdatingAge > 0)
                return;
            _tmrAgeChangeTimer.Start();
        }

        private void TypeOnTextChanged(object sender, EventArgs e)
        {
            if (_tmrTypeChangeTimer == null)
                return;
            if (_tmrTypeChangeTimer.Enabled)
                _tmrTypeChangeTimer.Stop();
            if (_intUpdatingType > 0)
                return;
            _tmrTypeChangeTimer.Start();
        }

        private void PersonalLifeOnTextChanged(object sender, EventArgs e)
        {
            if (_tmrPersonalLifeChangeTimer == null)
                return;
            if (_tmrPersonalLifeChangeTimer.Enabled)
                _tmrPersonalLifeChangeTimer.Stop();
            if (_intUpdatingPersonalLife > 0)
                return;
            _tmrPersonalLifeChangeTimer.Start();
        }

        private void PreferredPaymentOnTextChanged(object sender, EventArgs e)
        {
            if (_tmrPreferredPaymentChangeTimer == null)
                return;
            if (_tmrPreferredPaymentChangeTimer.Enabled)
                _tmrPreferredPaymentChangeTimer.Stop();
            if (_intUpdatingPreferredPayment > 0)
                return;
            _tmrPreferredPaymentChangeTimer.Start();
        }

        private void HobbiesViceOnTextChanged(object sender, EventArgs e)
        {
            if (_tmrHobbiesViceChangeTimer == null)
                return;
            if (_tmrHobbiesViceChangeTimer.Enabled)
                _tmrHobbiesViceChangeTimer.Stop();
            if (_intUpdatingHobbiesVice > 0)
                return;
            _tmrHobbiesViceChangeTimer.Start();
        }

        private void LoadStatBlockLists(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Read the list of Categories from the XML file.
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetatypes))
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstGenders))
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstAges))
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstPersonalLives))
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstTypes))
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstPreferredPayments))
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstHobbiesVices))
            {
                token.ThrowIfCancellationRequested();
                lstMetatypes.Add(ListItem.Blank);
                lstGenders.Add(ListItem.Blank);
                lstAges.Add(ListItem.Blank);
                lstPersonalLives.Add(ListItem.Blank);
                lstTypes.Add(ListItem.Blank);
                lstPreferredPayments.Add(ListItem.Blank);
                lstHobbiesVices.Add(ListItem.Blank);

                XPathNavigator xmlContactsBaseNode = _objContact.CharacterObject.LoadDataXPath("contacts.xml", token: token)
                                                                .SelectSingleNodeAndCacheExpression("/chummer", token);
                if (xmlContactsBaseNode != null)
                {
                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression("genders/gender", token))
                    {
                        token.ThrowIfCancellationRequested();
                        string strName = xmlNode.Value;
                        lstGenders.Add(new ListItem(
                                           strName,
                                           xmlNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression("ages/age", token))
                    {
                        token.ThrowIfCancellationRequested();
                        string strName = xmlNode.Value;
                        lstAges.Add(new ListItem(
                                        strName,
                                        xmlNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression(
                                 "personallives/personallife", token))
                    {
                        token.ThrowIfCancellationRequested();
                        string strName = xmlNode.Value;
                        lstPersonalLives.Add(new ListItem(
                                                 strName,
                                                 xmlNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value
                                                 ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression("types/type", token))
                    {
                        token.ThrowIfCancellationRequested();
                        string strName = xmlNode.Value;
                        lstTypes.Add(new ListItem(
                                         strName,
                                         xmlNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression(
                                 "preferredpayments/preferredpayment", token))
                    {
                        token.ThrowIfCancellationRequested();
                        string strName = xmlNode.Value;
                        lstPreferredPayments.Add(new ListItem(
                                                     strName,
                                                     xmlNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value
                                                     ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression(
                                 "hobbiesvices/hobbyvice", token))
                    {
                        token.ThrowIfCancellationRequested();
                        string strName = xmlNode.Value;
                        lstHobbiesVices.Add(new ListItem(
                                                strName,
                                                xmlNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value
                                                ?? strName));
                    }
                }

                string strSpace = LanguageManager.GetString("String_Space", token: token);
                foreach (XPathNavigator xmlMetatypeNode in _objContact.CharacterObject.LoadDataXPath("metatypes.xml", token: token)
                                                                      .SelectAndCacheExpression(
                                                                          "/chummer/metatypes/metatype", token))
                {
                    token.ThrowIfCancellationRequested();
                    string strName = xmlMetatypeNode.SelectSingleNodeAndCacheExpression("name", token)?.Value;
                    string strMetatypeDisplay = xmlMetatypeNode.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                                                ?? strName;
                    lstMetatypes.Add(new ListItem(strName, strMetatypeDisplay));
                    XPathNodeIterator xmlMetavariantsList
                        = xmlMetatypeNode.SelectAndCacheExpression("metavariants/metavariant", token);
                    if (xmlMetavariantsList.Count > 0)
                    {
                        string strMetavariantFormat = strMetatypeDisplay + strSpace + "({0})";
                        foreach (XPathNavigator objXmlMetavariantNode in xmlMetavariantsList)
                        {
                            token.ThrowIfCancellationRequested();
                            string strMetavariantName
                                = objXmlMetavariantNode.SelectSingleNodeAndCacheExpression("name", token)?.Value
                                  ?? string.Empty;
                            if (!string.IsNullOrEmpty(strMetavariantName) &&
                                (lstMetatypes.Count == 0 || lstMetatypes.TrueForAll(
                                    x => !strMetavariantName.Equals(x.Value?.ToString(),
                                                                   StringComparison.OrdinalIgnoreCase))))
                                lstMetatypes.Add(new ListItem(strMetavariantName,
                                                              string.Format(
                                                                  GlobalSettings.CultureInfo, strMetavariantFormat,
                                                                  objXmlMetavariantNode
                                                                      .SelectSingleNodeAndCacheExpression("translate", token)
                                                                      ?.Value ?? strMetavariantName)));
                        }
                    }
                }

                token.ThrowIfCancellationRequested();
                lstMetatypes.Sort(CompareListItems.CompareNames);
                token.ThrowIfCancellationRequested();
                lstGenders.Sort(CompareListItems.CompareNames);
                token.ThrowIfCancellationRequested();
                lstAges.Sort(CompareListItems.CompareNames);
                token.ThrowIfCancellationRequested();
                lstPersonalLives.Sort(CompareListItems.CompareNames);
                token.ThrowIfCancellationRequested();
                lstTypes.Sort(CompareListItems.CompareNames);
                token.ThrowIfCancellationRequested();
                lstHobbiesVices.Sort(CompareListItems.CompareNames);
                token.ThrowIfCancellationRequested();
                lstPreferredPayments.Sort(CompareListItems.CompareNames);

                cboMetatype.PopulateWithListItems(lstMetatypes, token);
                cboGender.PopulateWithListItems(lstGenders, token);
                cboAge.PopulateWithListItems(lstAges, token);
                cboPersonalLife.PopulateWithListItems(lstPersonalLives, token);
                cboType.PopulateWithListItems(lstTypes, token);
                cboPreferredPayment.PopulateWithListItems(lstPreferredPayments, token);
                cboHobbiesVice.PopulateWithListItems(lstHobbiesVices, token);
            }
        }

        private async Task LoadStatBlockListsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Read the list of Categories from the XML file.
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetatypes))
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstGenders))
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstAges))
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstPersonalLives))
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstTypes))
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstPreferredPayments))
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstHobbiesVices))
            {
                token.ThrowIfCancellationRequested();
                lstMetatypes.Add(ListItem.Blank);
                lstGenders.Add(ListItem.Blank);
                lstAges.Add(ListItem.Blank);
                lstPersonalLives.Add(ListItem.Blank);
                lstTypes.Add(ListItem.Blank);
                lstPreferredPayments.Add(ListItem.Blank);
                lstHobbiesVices.Add(ListItem.Blank);

                XPathNavigator xmlContactsBaseNode = (await _objContact.CharacterObject.LoadDataXPathAsync("contacts.xml", token: token).ConfigureAwait(false))
                                                           .SelectSingleNodeAndCacheExpression("/chummer", token);
                if (xmlContactsBaseNode != null)
                {
                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression("genders/gender", token))
                    {
                        token.ThrowIfCancellationRequested();
                        string strName = xmlNode.Value;
                        lstGenders.Add(new ListItem(
                                           strName,
                                           xmlNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression("ages/age", token))
                    {
                        token.ThrowIfCancellationRequested();
                        string strName = xmlNode.Value;
                        lstAges.Add(new ListItem(
                                        strName,
                                        xmlNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression(
                                 "personallives/personallife", token))
                    {
                        token.ThrowIfCancellationRequested();
                        string strName = xmlNode.Value;
                        lstPersonalLives.Add(new ListItem(
                                                 strName,
                                                 xmlNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value
                                                 ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression("types/type", token))
                    {
                        token.ThrowIfCancellationRequested();
                        string strName = xmlNode.Value;
                        lstTypes.Add(new ListItem(
                                         strName,
                                         xmlNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression(
                                 "preferredpayments/preferredpayment", token))
                    {
                        token.ThrowIfCancellationRequested();
                        string strName = xmlNode.Value;
                        lstPreferredPayments.Add(new ListItem(
                                                     strName,
                                                     xmlNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value
                                                     ?? strName));
                    }

                    foreach (XPathNavigator xmlNode in xmlContactsBaseNode.SelectAndCacheExpression(
                                 "hobbiesvices/hobbyvice", token))
                    {
                        token.ThrowIfCancellationRequested();
                        string strName = xmlNode.Value;
                        lstHobbiesVices.Add(new ListItem(
                                                strName,
                                                xmlNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value
                                                ?? strName));
                    }
                }

                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                foreach (XPathNavigator xmlMetatypeNode in (await _objContact.CharacterObject.LoadDataXPathAsync("metatypes.xml", token: token).ConfigureAwait(false))
                                                                 .SelectAndCacheExpression(
                                                                     "/chummer/metatypes/metatype", token))
                {
                    token.ThrowIfCancellationRequested();
                    string strName = xmlMetatypeNode.SelectSingleNodeAndCacheExpression("name", token)?.Value;
                    string strMetatypeDisplay = xmlMetatypeNode.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                                                ?? strName;
                    lstMetatypes.Add(new ListItem(strName, strMetatypeDisplay));
                    XPathNodeIterator xmlMetavariantsList
                        = xmlMetatypeNode.SelectAndCacheExpression("metavariants/metavariant", token);
                    if (xmlMetavariantsList.Count > 0)
                    {
                        string strMetavariantFormat = strMetatypeDisplay + strSpace + "({0})";
                        foreach (XPathNavigator objXmlMetavariantNode in xmlMetavariantsList)
                        {
                            token.ThrowIfCancellationRequested();
                            string strMetavariantName
                                = objXmlMetavariantNode.SelectSingleNodeAndCacheExpression("name", token)?.Value
                                  ?? string.Empty;
                            if (!string.IsNullOrEmpty(strMetavariantName) &&
                                (lstMetatypes.Count == 0 || lstMetatypes.TrueForAll(
                                    x => !strMetavariantName.Equals(x.Value?.ToString(),
                                                                   StringComparison.OrdinalIgnoreCase))))
                                lstMetatypes.Add(new ListItem(strMetavariantName,
                                                              string.Format(
                                                                  GlobalSettings.CultureInfo, strMetavariantFormat,
                                                                  objXmlMetavariantNode
                                                                      .SelectSingleNodeAndCacheExpression("translate", token)
                                                                      ?.Value ?? strMetavariantName)));
                        }
                    }
                }

                token.ThrowIfCancellationRequested();
                lstMetatypes.Sort(CompareListItems.CompareNames);
                token.ThrowIfCancellationRequested();
                lstGenders.Sort(CompareListItems.CompareNames);
                token.ThrowIfCancellationRequested();
                lstAges.Sort(CompareListItems.CompareNames);
                token.ThrowIfCancellationRequested();
                lstPersonalLives.Sort(CompareListItems.CompareNames);
                token.ThrowIfCancellationRequested();
                lstTypes.Sort(CompareListItems.CompareNames);
                token.ThrowIfCancellationRequested();
                lstHobbiesVices.Sort(CompareListItems.CompareNames);
                token.ThrowIfCancellationRequested();
                lstPreferredPayments.Sort(CompareListItems.CompareNames);

                await cboMetatype.PopulateWithListItemsAsync(lstMetatypes, token).ConfigureAwait(false);
                await cboGender.PopulateWithListItemsAsync(lstGenders, token).ConfigureAwait(false);
                await cboAge.PopulateWithListItemsAsync(lstAges, token).ConfigureAwait(false);
                await cboPersonalLife.PopulateWithListItemsAsync(lstPersonalLives, token).ConfigureAwait(false);
                await cboType.PopulateWithListItemsAsync(lstTypes, token).ConfigureAwait(false);
                await cboPreferredPayment.PopulateWithListItemsAsync(lstPreferredPayments, token).ConfigureAwait(false);
                await cboHobbiesVice.PopulateWithListItemsAsync(lstHobbiesVices, token).ConfigureAwait(false);
            }
        }

        #endregion Methods
    }
}
