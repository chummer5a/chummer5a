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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Enums;
using Chummer.Backend.Uniques;

namespace Chummer
{
    public partial class SpiritControl : UserControl
    {
        private readonly Spirit _objSpirit;
        private readonly CancellationToken _objMyToken;
        private bool _blnLoading = true;

        // Events.

        public event EventHandlerExtensions.SafeAsyncEventHandler DeleteSpirit;

        #region Control Events

        public SpiritControl(Spirit objSpirit, CancellationToken objMyToken)
        {
            _objSpirit = objSpirit ?? throw new ArgumentNullException(nameof(objSpirit));
            _objMyToken = objMyToken;
            InitializeComponent();

            Disposed += (sender, args) => UnbindSpiritControl();

            this.UpdateLightDarkMode(objMyToken);
            this.TranslateWinForm(token: objMyToken);
            foreach (ToolStripItem tssItem in cmsSpirit.Items)
            {
                tssItem.UpdateLightDarkMode(objMyToken);
                tssItem.TranslateToolStripItemsRecursively(token: objMyToken);
            }
        }

        private async void SpiritControl_Load(object sender, EventArgs e)
        {
            try
            {
                bool blnIsSpirit = await _objSpirit.GetEntityTypeAsync(_objMyToken).ConfigureAwait(false) == SpiritType.Spirit;
                await nudForce.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSpirit.CharacterObject,
                        nameof(Character.Created), x => x.GetCreatedAsync(_objMyToken), _objMyToken)
                    .ConfigureAwait(false);
                await chkBound.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y,
                    _objSpirit,
                    nameof(Spirit.Bound),
                    (x, y) => x.CheckedChanged += y,
                    x => x.GetBoundAsync(_objMyToken),
                    (x, y) => x.SetBoundAsync(y, _objMyToken),
                    _objMyToken,
                    _objMyToken).ConfigureAwait(false);
                await chkBound.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSpirit.CharacterObject,
                        nameof(Character.Created), x => x.GetCreatedAsync(_objMyToken), _objMyToken)
                    .ConfigureAwait(false);
                await cboSpiritName.RegisterAsyncDataBindingWithDelayAsync(x => x.Text, (x, y) => x.Text = y,
                    _objSpirit,
                    nameof(Spirit.Name),
                    (x, y) => x.TextChanged += y,
                    x => x.GetNameAsync(_objMyToken),
                    (x, y) => x.SetNameAsync(y, _objMyToken),
                    1000, _objMyToken, _objMyToken).ConfigureAwait(false);
                await txtCritterName.RegisterAsyncDataBindingWithDelayAsync(x => x.Text, (x, y) => x.Text = y,
                    _objSpirit,
                    nameof(Spirit.CritterName),
                    (x, y) => x.TextChanged += y,
                    x => x.GetCritterNameAsync(_objMyToken),
                    (x, y) => x.SetCritterNameAsync(y, _objMyToken),
                    1000, _objMyToken, _objMyToken).ConfigureAwait(false);
                await txtCritterName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSpirit,
                        nameof(Spirit.NoLinkedCharacter), x => x.GetNoLinkedCharacterAsync(_objMyToken), _objMyToken)
                    .ConfigureAwait(false);
                await nudForce.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _objSpirit.CharacterObject,
                        blnIsSpirit ? nameof(Character.MaxSpiritForce) : nameof(Character.MaxSpriteLevel),
                        x => blnIsSpirit
                            ? x.GetMaxSpiritForceAsync(_objMyToken)
                            : x.GetMaxSpriteLevelAsync(_objMyToken), _objMyToken)
                    .ConfigureAwait(false);
                await nudServices.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y,
                    _objSpirit,
                    nameof(Spirit.ServicesOwed),
                    (x, y) => x.ValueChanged += y,
                    x => x.GetServicesOwedAsync(_objMyToken),
                    (x, y) => x.SetServicesOwedAsync(y, _objMyToken),
                    250,
                    _objMyToken,
                    _objMyToken).ConfigureAwait(false);
                await nudForce.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y,
                    _objSpirit,
                    nameof(Spirit.Force),
                    (x, y) => x.ValueChanged += y,
                    x => x.GetForceAsync(_objMyToken),
                    (x, y) => x.SetForceAsync(y, _objMyToken),
                    250,
                    _objMyToken,
                    _objMyToken).ConfigureAwait(false);
                await chkFettered.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSpirit,
                        nameof(Spirit.AllowFettering), x => x.GetAllowFetteringAsync(_objMyToken), _objMyToken)
                    .ConfigureAwait(false);
                await chkFettered.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y,
                    _objSpirit,
                    nameof(Spirit.Fettered),
                    (x, y) => x.CheckedChanged += y,
                    x => x.GetFetteredAsync(_objMyToken),
                    (x, y) => x.SetFetteredAsync(y, _objMyToken),
                    _objMyToken,
                    _objMyToken).ConfigureAwait(false);
                if (blnIsSpirit)
                {
                    string strText = await LanguageManager.GetStringAsync("Label_Spirit_Force", token: _objMyToken).ConfigureAwait(false);
                    await lblForce.DoThreadSafeAsync(x => x.Text = strText, _objMyToken).ConfigureAwait(false);
                    string strText2 =
                        await LanguageManager.GetStringAsync("Checkbox_Spirit_Bound", token: _objMyToken).ConfigureAwait(false);
                    await chkBound.DoThreadSafeAsync(x => x.Text = strText2, _objMyToken).ConfigureAwait(false);
                    await cmdLink
                        .SetToolTipTextAsync(await LanguageManager
                            .GetStringAsync(!string.IsNullOrEmpty(await _objSpirit.GetFileNameAsync(_objMyToken).ConfigureAwait(false))
                                ? "Tip_Spirit_OpenFile"
                                : "Tip_Spirit_LinkSpirit", token: _objMyToken).ConfigureAwait(false), _objMyToken).ConfigureAwait(false);
                    string strTooltip =
                        await LanguageManager.GetStringAsync("Tip_Spirit_EditNotes", token: _objMyToken).ConfigureAwait(false);
                    string strNotes = await _objSpirit.GetNotesAsync(_objMyToken).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strNotes))
                        strTooltip += Environment.NewLine + Environment.NewLine + strNotes;
                    await cmdNotes.SetToolTipTextAsync(strTooltip.WordWrap(), _objMyToken).ConfigureAwait(false);
                }
                else
                {
                    string strText = await LanguageManager.GetStringAsync("Label_Sprite_Rating", token: _objMyToken).ConfigureAwait(false);
                    await lblForce.DoThreadSafeAsync(x => x.Text = strText, _objMyToken).ConfigureAwait(false);
                    string strText2 = await LanguageManager.GetStringAsync("Label_Sprite_TasksOwed", token: _objMyToken)
                        .ConfigureAwait(false);
                    await lblServices.DoThreadSafeAsync(x => x.Text = strText2, _objMyToken).ConfigureAwait(false);
                    string strText3 = await LanguageManager.GetStringAsync("Label_Sprite_Registered", token: _objMyToken)
                        .ConfigureAwait(false);
                    await chkBound.DoThreadSafeAsync(x => x.Text = strText3, _objMyToken).ConfigureAwait(false);
                    string strText4 = await LanguageManager.GetStringAsync("Checkbox_Sprite_Pet", token: _objMyToken).ConfigureAwait(false);
                    await chkFettered.DoThreadSafeAsync(x => x.Text = strText4, _objMyToken).ConfigureAwait(false);
                    await cmdLink
                        .SetToolTipTextAsync(await LanguageManager
                            .GetStringAsync(!string.IsNullOrEmpty(await _objSpirit.GetFileNameAsync(_objMyToken).ConfigureAwait(false))
                                ? "Tip_Sprite_OpenFile"
                                : "Tip_Sprite_LinkSpirit", token: _objMyToken).ConfigureAwait(false), _objMyToken).ConfigureAwait(false);
                    string strTooltip =
                        await LanguageManager.GetStringAsync("Tip_Sprite_EditNotes", token: _objMyToken).ConfigureAwait(false);
                    string strNotes = await _objSpirit.GetNotesAsync(_objMyToken).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strNotes))
                        strTooltip += Environment.NewLine + Environment.NewLine + strNotes;
                    await cmdNotes.SetToolTipTextAsync(strTooltip.WordWrap(), _objMyToken).ConfigureAwait(false);
                }

                _objSpirit.CharacterObject.PropertyChangedAsync += RebuildSpiritListOnTraditionChange;
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
            finally
            {
                _blnLoading = false;
            }
        }

        public void UnbindSpiritControl()
        {
            Character objCharacter = _objSpirit.CharacterObject; // for thread safety
            if (objCharacter?.IsDisposed == false)
                objCharacter.PropertyChangedAsync -= RebuildSpiritListOnTraditionChange;

            foreach (Control objControl in Controls)
                objControl.DataBindings.Clear();
        }

        private async void cmdDelete_Click(object sender, EventArgs e)
        {
            // Raise the DeleteSpirit Event when the user has confirmed their desire to delete the Spirit.
            // The entire SpiritControl is passed as an argument so the handling event can evaluate its contents.
            if (_blnLoading || DeleteSpirit == null)
                return;
            try
            {
                await DeleteSpirit.Invoke(this, e, _objMyToken).ConfigureAwait(false);
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
                Character objLinkedCharacter = await _objSpirit.GetLinkedCharacterAsync(_objMyToken).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                {
                    Character objOpenCharacter = await Program.OpenCharacters.ContainsAsync(objLinkedCharacter, _objMyToken)
                        .ConfigureAwait(false)
                        ? objLinkedCharacter
                        : null;
                    CursorWait objCursorWait = await CursorWait.NewAsync(ParentForm, token: _objMyToken).ConfigureAwait(false);
                    try
                    {
                        if (objOpenCharacter == null)
                        {
                            using (ThreadSafeForm<LoadingBar> frmLoadingBar
                                   = await Program.CreateAndShowProgressBarAsync(
                                           await objLinkedCharacter.GetFileNameAsync(_objMyToken).ConfigureAwait(false), Character.NumLoadingSections, _objMyToken)
                                       .ConfigureAwait(false))
                                objOpenCharacter = await Program.LoadCharacterAsync(
                                        await objLinkedCharacter.GetFileNameAsync(_objMyToken).ConfigureAwait(false), frmLoadingBar: frmLoadingBar.MyForm, token: _objMyToken)
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
                    string strFileName = await _objSpirit.GetFileNameAsync(_objMyToken).ConfigureAwait(false);
                    if (!File.Exists(strFileName))
                    {
                        bool blnError = false;
                        string strRelativeFileName = await _objSpirit.GetRelativeFileNameAsync(_objMyToken).ConfigureAwait(false);
                        // If the file doesn't exist, use the relative path if one is available.
                        if (string.IsNullOrEmpty(strRelativeFileName))
                            blnError = true;
                        else if (!File.Exists(Path.GetFullPath(strRelativeFileName)))
                            blnError = true;
                        else
                            blnUseRelative = true;

                        if (blnError)
                        {
                            await Program.ShowScrollableMessageBoxAsync(
                                string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Message_FileNotFound", token: _objMyToken)
                                        .ConfigureAwait(false), strFileName),
                                await LanguageManager.GetStringAsync("MessageTitle_FileNotFound", token: _objMyToken).ConfigureAwait(false),
                                MessageBoxButtons.OK, MessageBoxIcon.Error, token: _objMyToken).ConfigureAwait(false);
                            return;
                        }
                    }

                    string strFile = blnUseRelative
                        ? Path.GetFullPath(await _objSpirit.GetRelativeFileNameAsync(_objMyToken).ConfigureAwait(false))
                        : strFileName;
                    Process.Start(new ProcessStartInfo(strFile) { UseShellExecute = true });
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
                    await _objSpirit.SetFileNameAsync(string.Empty, _objMyToken).ConfigureAwait(false);
                    await _objSpirit.SetRelativeFileNameAsync(string.Empty, _objMyToken).ConfigureAwait(false);
                    string strText = await LanguageManager.GetStringAsync(
                            await _objSpirit.GetEntityTypeAsync(_objMyToken).ConfigureAwait(false) == SpiritType.Spirit
                                ? "Tip_Spirit_LinkSpirit"
                                : "Tip_Sprite_LinkSprite", token: _objMyToken)
                        .ConfigureAwait(false);
                    await cmdLink.SetToolTipTextAsync(strText, _objMyToken).ConfigureAwait(false);

                    // Set the relative path.
                    Uri uriApplication = new Uri(Utils.GetStartupPath);
                    Uri uriFile = new Uri(await _objSpirit.GetFileNameAsync(_objMyToken).ConfigureAwait(false));
                    Uri uriRelative = uriApplication.MakeRelativeUri(uriFile);
                    await _objSpirit.SetRelativeFileNameAsync("../" + uriRelative, _objMyToken).ConfigureAwait(false);
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
                // Prompt the user to select a save file to associate with this Contact.
                string strOldFileName = await _objSpirit.GetFileNameAsync(_objMyToken).ConfigureAwait(false);
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
                }, token: _objMyToken).ConfigureAwait(false);

                if (eResult != DialogResult.OK)
                    return;
                await _objSpirit.SetFileNameAsync(strFileName, _objMyToken).ConfigureAwait(false);
                string strText = await LanguageManager.GetStringAsync(
                        await _objSpirit.GetEntityTypeAsync(_objMyToken).ConfigureAwait(false) == SpiritType.Spirit
                            ? "Tip_Spirit_OpenFile"
                            : "Tip_Sprite_OpenFile", token: _objMyToken)
                    .ConfigureAwait(false);
                await cmdLink.SetToolTipTextAsync(strText, _objMyToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void tsCreateCharacter_Click(object sender, EventArgs e)
        {
            try
            {
                string strSpiritName = await cboSpiritName.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: _objMyToken)
                    .ConfigureAwait(false);
                if (string.IsNullOrEmpty(strSpiritName))
                {
                    await Program.ShowScrollableMessageBoxAsync(
                        await LanguageManager.GetStringAsync("Message_SelectCritterType", token: _objMyToken).ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_SelectCritterType", token: _objMyToken).ConfigureAwait(false),
                        MessageBoxButtons.OK, MessageBoxIcon.Error, token: _objMyToken).ConfigureAwait(false);
                    return;
                }

                await CreateCritter(strSpiritName, nudForce.ValueAsInt, _objMyToken).ConfigureAwait(false);
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
                if (!string.IsNullOrEmpty(await _objSpirit.GetFileNameAsync(_objMyToken).ConfigureAwait(false)))
                {
                    await cmsSpirit.DoThreadSafeAsync(() =>
                    {
                        tsAttachCharacter.Visible = false;
                        tsCreateCharacter.Visible = false;
                        tsContactOpen.Visible = true;
                        tsRemoveCharacter.Visible = true;
                    }, token: _objMyToken).ConfigureAwait(false);
                }
                else
                {
                    await cmsSpirit.DoThreadSafeAsync(() =>
                    {
                        tsAttachCharacter.Visible = true;
                        tsCreateCharacter.Visible = true;
                        tsContactOpen.Visible = false;
                        tsRemoveCharacter.Visible = false;
                    }, token: _objMyToken).ConfigureAwait(false);
                }

                await cmsSpirit.DoThreadSafeAsync(x => x.Show(cmdLink, cmdLink.Left - x.PreferredSize.Width, cmdLink.Top), _objMyToken).ConfigureAwait(false);
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
                string strNotes = await _objSpirit.GetNotesAsync(_objMyToken).ConfigureAwait(false);
                Color objColor = await _objSpirit.GetNotesColorAsync(_objMyToken).ConfigureAwait(false);
                using (ThreadSafeForm<EditNotes> frmSpiritNotes = await ThreadSafeForm<EditNotes>
                           .GetAsync(() => new EditNotes(strNotes, objColor), _objMyToken)
                           .ConfigureAwait(false))
                {
                    if (await frmSpiritNotes.ShowDialogSafeAsync(_objSpirit.CharacterObject, _objMyToken).ConfigureAwait(false) !=
                        DialogResult.OK)
                        return;
                    await _objSpirit.SetNotesAsync(frmSpiritNotes.MyForm.Notes, _objMyToken).ConfigureAwait(false);
                    await _objSpirit.SetNotesColorAsync(frmSpiritNotes.MyForm.NotesColor, _objMyToken).ConfigureAwait(false);
                }

                string strTooltip = await LanguageManager
                    .GetStringAsync(await _objSpirit.GetEntityTypeAsync(_objMyToken).ConfigureAwait(false) == SpiritType.Spirit
                        ? "Tip_Spirit_EditNotes"
                        : "Tip_Sprite_EditNotes", token: _objMyToken).ConfigureAwait(false);
                strNotes = await _objSpirit.GetNotesAsync(_objMyToken).ConfigureAwait(false);
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
        /// Spirit object this is linked to.
        /// </summary>
        public Spirit SpiritObject => _objSpirit;

        #endregion Properties

        #region Methods

        // Rebuild the list of Spirits/Sprites based on the character's selected Tradition/Stream.
        public async Task RebuildSpiritListOnTraditionChange(object sender, PropertyChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (e?.PropertyName == nameof(Character.MagicTradition))
                await RebuildSpiritList(
                        await _objSpirit.CharacterObject.GetMagicTraditionAsync(token).ConfigureAwait(false), token)
                    .ConfigureAwait(false);
        }

        // Rebuild the list of Spirits/Sprites based on the character's selected Tradition/Stream.
        public async Task RebuildSpiritList(Tradition objTradition, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objTradition == null)
                return;
            string strCurrentValue = await cboSpiritName.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false) ?? _objSpirit.Name;

            XPathNavigator objXmlDocument = await _objSpirit.CharacterObject.LoadDataXPathAsync(
                await _objSpirit.GetEntityTypeAsync(token).ConfigureAwait(false) == SpiritType.Spirit
                    ? "traditions.xml"
                    : "streams.xml", token: token).ConfigureAwait(false);

            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setLimitCategories))
            {
                foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                             _objSpirit.CharacterObject, Improvement.ImprovementType.LimitSpiritCategory, token: token).ConfigureAwait(false))
                {
                    setLimitCategories.Add(objImprovement.ImprovedName);
                }

                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstCritters))
                {
                    if (objTradition.IsCustomTradition)
                    {
                        string strSpiritCombat = objTradition.SpiritCombat;
                        string strSpiritDetection = objTradition.SpiritDetection;
                        string strSpiritHealth = objTradition.SpiritHealth;
                        string strSpiritIllusion = objTradition.SpiritIllusion;
                        string strSpiritManipulation = objTradition.SpiritManipulation;

                        if ((setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritCombat))
                            && !string.IsNullOrWhiteSpace(strSpiritCombat))
                        {
                            XPathNavigator objXmlCritterNode
                                = objXmlDocument.SelectSingleNode(
                                    "/chummer/spirits/spirit[name = " + strSpiritCombat.CleanXPath() + ']');
                            string strTranslatedName = objXmlCritterNode != null
                                ? objXmlCritterNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                  ?? strSpiritCombat
                                : strSpiritCombat;
                            lstCritters.Add(new ListItem(strSpiritCombat, strTranslatedName));
                        }

                        if ((setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritDetection))
                            && !string.IsNullOrWhiteSpace(strSpiritDetection))
                        {
                            XPathNavigator objXmlCritterNode
                                = objXmlDocument.SelectSingleNode(
                                    "/chummer/spirits/spirit[name = " + strSpiritDetection.CleanXPath() + ']');
                            string strTranslatedName = objXmlCritterNode != null
                                ? objXmlCritterNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                  ?? strSpiritDetection
                                : strSpiritDetection;
                            lstCritters.Add(new ListItem(strSpiritDetection, strTranslatedName));
                        }

                        if ((setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritHealth))
                            && !string.IsNullOrWhiteSpace(strSpiritHealth))
                        {
                            XPathNavigator objXmlCritterNode
                                = objXmlDocument.SelectSingleNode(
                                    "/chummer/spirits/spirit[name = " + strSpiritHealth.CleanXPath() + ']');
                            string strTranslatedName = objXmlCritterNode != null
                                ? objXmlCritterNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                  ?? strSpiritHealth
                                : strSpiritHealth;
                            lstCritters.Add(new ListItem(strSpiritHealth, strTranslatedName));
                        }

                        if ((setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritIllusion))
                            && !string.IsNullOrWhiteSpace(strSpiritIllusion))
                        {
                            XPathNavigator objXmlCritterNode
                                = objXmlDocument.SelectSingleNode(
                                    "/chummer/spirits/spirit[name = " + strSpiritIllusion.CleanXPath() + ']');
                            string strTranslatedName = objXmlCritterNode != null
                                ? objXmlCritterNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                  ?? strSpiritIllusion
                                : strSpiritIllusion;
                            lstCritters.Add(new ListItem(strSpiritIllusion, strTranslatedName));
                        }

                        if ((setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritManipulation))
                            && !string.IsNullOrWhiteSpace(strSpiritManipulation))
                        {
                            XPathNavigator objXmlCritterNode
                                = objXmlDocument.SelectSingleNode(
                                    "/chummer/spirits/spirit[name = " + strSpiritManipulation.CleanXPath() + ']');
                            string strTranslatedName = objXmlCritterNode != null
                                ? objXmlCritterNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                  ?? strSpiritManipulation
                                : strSpiritManipulation;
                            lstCritters.Add(new ListItem(strSpiritManipulation, strTranslatedName));
                        }
                    }
                    else
                    {
                        XPathNavigator objDataNode = await objTradition.GetNodeXPathAsync(token: token).ConfigureAwait(false);
                        if (objDataNode?.SelectSingleNodeAndCacheExpression("spirits/spirit[. = \"All\"]", token) != null)
                        {
                            if (setLimitCategories.Count == 0)
                            {
                                foreach (XPathNavigator objXmlCritterNode in objXmlDocument.SelectAndCacheExpression(
                                             "/chummer/spirits/spirit", token: token))
                                {
                                    string strSpiritName = objXmlCritterNode.SelectSingleNodeAndCacheExpression("name", token: token)
                                                                            ?.Value;
                                    lstCritters.Add(new ListItem(strSpiritName,
                                                                 objXmlCritterNode
                                                                     .SelectSingleNodeAndCacheExpression("translate", token: token)
                                                                     ?.Value
                                                                 ?? strSpiritName));
                                }
                            }
                            else
                            {
                                foreach (string strSpiritName in setLimitCategories)
                                {
                                    XPathNavigator objXmlCritterNode
                                        = objXmlDocument.SelectSingleNode(
                                            "/chummer/spirits/spirit[name = " + strSpiritName.CleanXPath() + ']');
                                    string strTranslatedName = objXmlCritterNode != null
                                        ? objXmlCritterNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                          ?? strSpiritName
                                        : strSpiritName;
                                    lstCritters.Add(new ListItem(strSpiritName, strTranslatedName));
                                }
                            }
                        }
                        else
                        {
                            XPathNavigator objTraditionNode = await objTradition.GetNodeXPathAsync(token: token).ConfigureAwait(false);
                            XPathNodeIterator xmlSpiritList = objTraditionNode?.SelectAndCacheExpression("spirits/*", token);
                            if (xmlSpiritList != null)
                            {
                                foreach (XPathNavigator objXmlSpirit in xmlSpiritList)
                                {
                                    string strSpiritName = objXmlSpirit.Value;
                                    if (setLimitCategories.Count == 0 || setLimitCategories.Contains(strSpiritName))
                                    {
                                        XPathNavigator objXmlCritterNode
                                            = objXmlDocument.SelectSingleNode(
                                                "/chummer/spirits/spirit[name = " + strSpiritName.CleanXPath()
                                                + ']');
                                        string strTranslatedName = objXmlCritterNode != null
                                            ? objXmlCritterNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                              ?? strSpiritName
                                            : strSpiritName;
                                        lstCritters.Add(new ListItem(strSpiritName, strTranslatedName));
                                    }
                                }
                            }
                        }
                    }

                    // Add any additional Spirits and Sprites the character has Access to through improvements.

                    if (await _objSpirit.CharacterObject.GetMAGEnabledAsync(token).ConfigureAwait(false))
                    {
                        foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                     _objSpirit.CharacterObject, Improvement.ImprovementType.AddSpirit, token: token).ConfigureAwait(false))
                        {
                            string strImprovedName = objImprovement.ImprovedName;
                            if (!string.IsNullOrEmpty(strImprovedName))
                            {
                                lstCritters.Add(new ListItem(strImprovedName,
                                                             objXmlDocument
                                                                 .SelectSingleNode(
                                                                     "/chummer/spirits/spirit[name = "
                                                                     + strImprovedName.CleanXPath() + "]/translate")
                                                                 ?.Value
                                                             ?? strImprovedName));
                            }
                        }
                    }

                    if (await _objSpirit.CharacterObject.GetRESEnabledAsync(token).ConfigureAwait(false))
                    {
                        foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                     _objSpirit.CharacterObject, Improvement.ImprovementType.AddSprite, token: token).ConfigureAwait(false))
                        {
                            string strImprovedName = objImprovement.ImprovedName;
                            if (!string.IsNullOrEmpty(strImprovedName))
                            {
                                lstCritters.Add(new ListItem(strImprovedName,
                                                             objXmlDocument
                                                                 .SelectSingleNode(
                                                                     "/chummer/spirits/spirit[name = "
                                                                     + strImprovedName.CleanXPath() + "]/translate")
                                                                 ?.Value
                                                             ?? strImprovedName));
                            }
                        }
                    }

                    await cboSpiritName.PopulateWithListItemsAsync(lstCritters, token: token).ConfigureAwait(false);
                    // Set the control back to its original value.
                    if (!string.IsNullOrEmpty(strCurrentValue))
                        await cboSpiritName.DoThreadSafeAsync(x => x.SelectedValue = strCurrentValue, token: token).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Create a Critter, put them into Career Mode, link them, and open the newly-created Critter.
        /// </summary>
        /// <param name="strCritterName">Name of the Critter's Metatype.</param>
        /// <param name="intForce">Critter's Force.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private async Task CreateCritter(string strCritterName, int intForce, CancellationToken token = default)
        {
            // Code from frmMetatype.
            XmlDocument objXmlDocument = await _objSpirit.CharacterObject.LoadDataAsync("critters.xml", token: token)
                .ConfigureAwait(false);

            XmlNode objXmlMetatype = objXmlDocument.TryGetNodeByNameOrId("/chummer/metatypes/metatype", strCritterName);

            // If the Critter could not be found, show an error and get out of here.
            if (objXmlMetatype == null)
            {
                await Program.ShowScrollableMessageBoxAsync(
                    string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("Message_UnknownCritterType", token: token)
                            .ConfigureAwait(false),
                        strCritterName),
                    await LanguageManager.GetStringAsync("MessageTitle_SelectCritterType", token: token)
                        .ConfigureAwait(false),
                    MessageBoxButtons.OK, MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                return;
            }

            CursorWait objCursorWait = await CursorWait.NewAsync(ParentForm, token: token).ConfigureAwait(false);
            try
            {
                // The Critter should use the same settings file as the character.
                Character objCharacter = new Character();
                try
                {
                    IAsyncDisposable objLocker =
                        await objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        await objCharacter.SetSettingsKeyAsync(
                            await _objSpirit.CharacterObject.GetSettingsKeyAsync(token).ConfigureAwait(false),
                            token).ConfigureAwait(false);
                        // Override the defaults for the setting.
                        await objCharacter.SetIgnoreRulesAsync(true, token).ConfigureAwait(false);
                        await objCharacter.SetIsCritterAsync(true, token).ConfigureAwait(false);
                        await objCharacter.SetAliasAsync(strCritterName, token).ConfigureAwait(false);
                        await objCharacter.SetCreatedAsync(true, token: token).ConfigureAwait(false);
                        string strCritterCharacterName = await txtCritterName
                            .DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(strCritterCharacterName))
                            await objCharacter.SetNameAsync(strCritterCharacterName, token).ConfigureAwait(false);

                        string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                            .ConfigureAwait(false);
                        string strFileName = string.Empty;
                        string strFilter = await LanguageManager.GetStringAsync("DialogFilter_Chum5", token: token)
                                               .ConfigureAwait(false) + '|' +
                                           await LanguageManager.GetStringAsync("DialogFilter_Chum5lz", token: token)
                                               .ConfigureAwait(false) + '|' +
                                           await LanguageManager.GetStringAsync("DialogFilter_All", token: token)
                                               .ConfigureAwait(false);
                        string strInputFileName = strCritterName + strSpace + '('
                                                  + string.Format(
                                                      GlobalSettings.CultureInfo,
                                                      await LanguageManager
                                                          .GetStringAsync("Label_RatingFormat", token: token)
                                                          .ConfigureAwait(false), await LanguageManager
                                                          .GetStringAsync(
                                                              await _objSpirit.GetRatingLabelAsync(token)
                                                                  .ConfigureAwait(false), token: token)
                                                          .ConfigureAwait(false)) + strSpace
                                                  + (await _objSpirit.GetForceAsync(token).ConfigureAwait(false))
                                                  .ToString(
                                                      GlobalSettings.InvariantCultureInfo);
                        DialogResult eResult = await this.DoThreadSafeFuncAsync(x =>
                        {
                            using (SaveFileDialog dlgSaveFile = new SaveFileDialog())
                            {
                                dlgSaveFile.Filter = strFilter;
                                dlgSaveFile.FileName = strInputFileName;
                                DialogResult eReturn = dlgSaveFile.ShowDialog(x);
                                strFileName = dlgSaveFile.FileName;
                                return eReturn;
                            }
                        }, token: token).ConfigureAwait(false);

                        if (eResult != DialogResult.OK)
                            return;

                        if (!strFileName.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase)
                            && !strFileName.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                            strFileName += ".chum5";
                        await objCharacter.SetFileNameAsync(strFileName, token).ConfigureAwait(false);

                        await objCharacter.CreateAsync(objXmlMetatype["category"]?.InnerText,
                            objXmlMetatype["id"]?.InnerText,
                            string.Empty, objXmlMetatype, intForce, token: token).ConfigureAwait(false);
                        objCharacter.MetatypeBP = 0;
                        using (ThreadSafeForm<LoadingBar> frmLoadingBar =
                               await Program.CreateAndShowProgressBarAsync(token: token).ConfigureAwait(false))
                        {
                            await frmLoadingBar.MyForm.PerformStepAsync(await objCharacter.GetCharacterNameAsync(token).ConfigureAwait(false),
                                LoadingBar.ProgressBarTextPatterns.Saving, token).ConfigureAwait(false);
                            if (!await objCharacter.SaveAsync(token: token).ConfigureAwait(false))
                                return;
                        }
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }

                    // Link the newly-created Critter to the Spirit.
                    string strText = await LanguageManager.GetStringAsync(
                        await _objSpirit.GetEntityTypeAsync(token).ConfigureAwait(false) == SpiritType.Spirit
                            ? "Tip_Spirit_OpenFile"
                            : "Tip_Sprite_OpenFile", token: token).ConfigureAwait(false);
                    await cmdLink.SetToolTipTextAsync(strText, token).ConfigureAwait(false);

                    await Program.OpenCharacter(objCharacter, token: token).ConfigureAwait(false);
                }
                finally
                {
                    await objCharacter
                        .DisposeAsync()
                        .ConfigureAwait(
                            false); // Fine here because Dispose()/DisposeAsync() code is skipped if the character is open in a form
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Methods
    }
}
