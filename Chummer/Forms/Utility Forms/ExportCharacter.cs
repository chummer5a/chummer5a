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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;
using Microsoft.IO;
using Newtonsoft.Json;
using NLog;
using Formatting = Newtonsoft.Json.Formatting;

namespace Chummer
{
    public partial class ExportCharacter : Form, IHasCharacterObjects
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private readonly Character _objCharacter;
        private readonly ConcurrentDictionary<ValueTuple<string, string>, ValueTuple<string, string>> _dicCache = new ConcurrentDictionary<ValueTuple<string, string>, ValueTuple<string, string>>();
        private CancellationTokenSource _objCharacterXmlGeneratorCancellationTokenSource;
        private CancellationTokenSource _objXmlGeneratorCancellationTokenSource;
        private CancellationTokenSource _objGenericFormClosingCancellationTokenSource;
        private readonly CancellationToken _objGenericToken;
        private Task _tskCharacterXmlGenerator;
        private Task _tskXmlGenerator;
        private XmlDocument _objCharacterXml;
        private int _intSelected;
        private string _strXslt;
        private string _strExportLanguage;
        private CultureInfo _objExportCulture;
        private int _intLoading = 1;

        public Character CharacterObject => _objCharacter;

        public IEnumerable<Character> CharacterObjects => _objCharacter?.Yield() ?? Enumerable.Empty<Character>();

        #region Control Events

        public ExportCharacter(Character objCharacter)
        {
            _objCharacter = objCharacter;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            _objGenericFormClosingCancellationTokenSource = new CancellationTokenSource();
            _objGenericToken = _objGenericFormClosingCancellationTokenSource.Token;
            Program.MainForm.OpenCharacterExportForms?.Add(this);
        }

        private async void ExportCharacter_Load(object sender, EventArgs e)
        {
            try
            {
                IAsyncDisposable objInnerLocker = await _objCharacter.LockObject.EnterWriteLockAsync(_objGenericToken)
                                                                     .ConfigureAwait(false);
                try
                {
                    _objGenericToken.ThrowIfCancellationRequested();
                    _objCharacter.MultiplePropertiesChangedAsync += ObjCharacterOnPropertyChanged;
                    _objCharacter.SettingsPropertyChangedAsync += ObjCharacterOnSettingsPropertyChanged;
                    // TODO: Make these also work for any children collection changes
                    _objCharacter.Cyberware.CollectionChangedAsync += OnCharacterCollectionChanged;
                    _objCharacter.Armor.CollectionChangedAsync += OnCharacterCollectionChanged;
                    _objCharacter.Weapons.CollectionChangedAsync += OnCharacterCollectionChanged;
                    _objCharacter.Gear.CollectionChangedAsync += OnCharacterCollectionChanged;
                    _objCharacter.Contacts.CollectionChangedAsync += OnCharacterCollectionChanged;
                    _objCharacter.ExpenseEntries.CollectionChangedAsync += OnCharacterCollectionChanged;
                    _objCharacter.MentorSpirits.CollectionChangedAsync += OnCharacterCollectionChanged;
                    _objCharacter.Powers.ListChangedAsync += OnCharacterListChanged;
                    _objCharacter.Qualities.CollectionChangedAsync += OnCharacterCollectionChanged;
                    _objCharacter.MartialArts.CollectionChangedAsync += OnCharacterCollectionChanged;
                    _objCharacter.Metamagics.CollectionChangedAsync += OnCharacterCollectionChanged;
                    _objCharacter.Spells.CollectionChangedAsync += OnCharacterCollectionChanged;
                    _objCharacter.ComplexForms.CollectionChangedAsync += OnCharacterCollectionChanged;
                    _objCharacter.CritterPowers.CollectionChangedAsync += OnCharacterCollectionChanged;
                    _objCharacter.SustainedCollection.CollectionChangedAsync += OnCharacterCollectionChanged;
                    _objCharacter.InitiationGrades.CollectionChangedAsync += OnCharacterCollectionChanged;
                }
                finally
                {
                    await objInnerLocker.DisposeAsync().ConfigureAwait(false);
                }

                await LanguageManager
                    .PopulateSheetLanguageListAsync(cboLanguage, GlobalSettings.DefaultCharacterSheet,
                                                    _objCharacter.Yield(), _objExportCulture, token: _objGenericToken)
                    .ConfigureAwait(false);
                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(
                           Utils.ListItemListPool, out List<ListItem> lstExportMethods))
                {
                    // Populate the XSLT list with all of the XSL files found in the sheets directory.
                    // Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets (hidden because they are partial templates that cannot be used on their own).
                    foreach (string strFile in Directory.EnumerateFiles(Path.Combine(Utils.GetStartupPath, "export"), "*.xsl"))
                    {
                        // We need to test this explicitly because .NET Framework has weird behavior with search patterns for asterisk and then a three-letter extension
                        // (It allows any files whose extension begins with those three letters through, so e.g. it would allow xslt files here)
                        if (strFile.EndsWith(".xsl", StringComparison.OrdinalIgnoreCase))
                        {
                            string strFileName = Path.GetFileNameWithoutExtension(strFile);
                            lstExportMethods.Add(new ListItem(strFileName, strFileName));
                        }
                    }

                    lstExportMethods.Sort();
                    lstExportMethods.Insert(
                        0,
                        new ListItem(
                            "JSON",
                            string.Format(GlobalSettings.CultureInfo,
                                          await LanguageManager
                                                .GetStringAsync("String_Export_Blank", token: _objGenericToken)
                                                .ConfigureAwait(false), "JSON")));

                    await cboXSLT.PopulateWithListItemsAsync(lstExportMethods, _objGenericToken).ConfigureAwait(false);
                    await cboXSLT.DoThreadSafeAsync(x =>
                    {
                        if (x.Items.Count > 0)
                            x.SelectedIndex = 0;
                    }, _objGenericToken).ConfigureAwait(false);
                }

                Interlocked.Decrement(ref _intLoading);
                await UpdateWindowTitleAsync(_objGenericToken).ConfigureAwait(false);
                await DoLanguageUpdate(_objGenericToken).ConfigureAwait(false);
                // Stupid hack to get the MDI icon to show up properly.
                await this.DoThreadSafeFuncAsync(x => x.Icon = x.Icon.Clone() as Icon,
                                                 _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task ObjCharacterOnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e, CancellationToken token = default)
        {
            try
            {
                if (e.PropertyName == nameof(CharacterSettings.Name))
                    await UpdateWindowTitleAsync(token).ConfigureAwait(false);
                await DoXsltUpdate(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task ObjCharacterOnPropertyChanged(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                if (e.PropertyNames.Contains(nameof(Character.CharacterName)) || e.PropertyNames.Contains(nameof(Character.Created)))
                    await UpdateWindowTitleAsync(token).ConfigureAwait(false);
                await DoXsltUpdate(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task OnCharacterListChanged(object sender, ListChangedEventArgs e, CancellationToken token = default)
        {
            if (e.ListChangedType == ListChangedType.ItemMoved
                || e.ListChangedType == ListChangedType.PropertyDescriptorAdded
                || e.ListChangedType == ListChangedType.PropertyDescriptorChanged
                || e.ListChangedType == ListChangedType.PropertyDescriptorDeleted)
                return;
            try
            {
                await DoXsltUpdate(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task OnCharacterCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;
            try
            {
                await DoXsltUpdate(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void ExportCharacter_FormClosing(object sender, FormClosingEventArgs e)
        {
            CancellationTokenSource objTemp = Interlocked.Exchange(ref _objXmlGeneratorCancellationTokenSource, null);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }

            objTemp = Interlocked.Exchange(ref _objCharacterXmlGeneratorCancellationTokenSource, null);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }

            // ReSharper disable once MethodSupportsCancellation
            IAsyncDisposable objInnerLocker = await _objCharacter.LockObject.EnterWriteLockAsync(CancellationToken.None)
                                                                 .ConfigureAwait(false);
            try
            {
                _objCharacter.MultiplePropertiesChangedAsync -= ObjCharacterOnPropertyChanged;
                _objCharacter.SettingsPropertyChangedAsync -= ObjCharacterOnSettingsPropertyChanged;
                _objCharacter.Cyberware.CollectionChangedAsync -= OnCharacterCollectionChanged;
                _objCharacter.Armor.CollectionChangedAsync -= OnCharacterCollectionChanged;
                _objCharacter.Weapons.CollectionChangedAsync -= OnCharacterCollectionChanged;
                _objCharacter.Gear.CollectionChangedAsync -= OnCharacterCollectionChanged;
                _objCharacter.Contacts.CollectionChangedAsync -= OnCharacterCollectionChanged;
                _objCharacter.ExpenseEntries.CollectionChangedAsync -= OnCharacterCollectionChanged;
                _objCharacter.MentorSpirits.CollectionChangedAsync -= OnCharacterCollectionChanged;
                _objCharacter.Powers.ListChangedAsync -= OnCharacterListChanged;
                _objCharacter.Qualities.CollectionChangedAsync -= OnCharacterCollectionChanged;
                _objCharacter.MartialArts.CollectionChangedAsync -= OnCharacterCollectionChanged;
                _objCharacter.Metamagics.CollectionChangedAsync -= OnCharacterCollectionChanged;
                _objCharacter.Spells.CollectionChangedAsync -= OnCharacterCollectionChanged;
                _objCharacter.ComplexForms.CollectionChangedAsync -= OnCharacterCollectionChanged;
                _objCharacter.CritterPowers.CollectionChangedAsync -= OnCharacterCollectionChanged;
                _objCharacter.SustainedCollection.CollectionChangedAsync -= OnCharacterCollectionChanged;
                _objCharacter.InitiationGrades.CollectionChangedAsync -= OnCharacterCollectionChanged;
            }
            finally
            {
                await objInnerLocker.DisposeAsync().ConfigureAwait(false);
            }

            Task tskOld = Interlocked.Exchange(ref _tskXmlGenerator, null);
            if (tskOld != null)
            {
                try
                {
                    await tskOld.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Swallow this
                }
            }

            tskOld = Interlocked.Exchange(ref _tskCharacterXmlGenerator, null);
            if (tskOld != null)
            {
                try
                {
                    await tskOld.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Swallow this
                }
            }

            _objGenericFormClosingCancellationTokenSource?.Cancel(false);
        }

        private async void cmdExport_Click(object sender, EventArgs e)
        {
            try
            {
                await DoExport(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cmdExportClose_Click(object sender, EventArgs e)
        {
            try
            {
                await DoExport(_objGenericToken).ConfigureAwait(false);
                await this.DoThreadSafeAsync(x =>
                {
                    x.DialogResult = DialogResult.OK;
                    x.Close();
                }, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task DoExport(CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(_strXslt))
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                if (_strXslt == "JSON")
                {
                    await ExportJson(token: token).ConfigureAwait(false);
                }
                else
                {
                    await ExportNormal(token: token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await DoLanguageUpdate(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cboXSLT_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await DoXsltUpdate(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task DoLanguageUpdate(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intLoading == 0)
            {
                _strExportLanguage = await cboLanguage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false) ?? GlobalSettings.Language;
                try
                {
                    _objExportCulture = CultureInfo.GetCultureInfo(_strExportLanguage);
                }
                catch (CultureNotFoundException)
                {
                    // Swallow this
                }

                _objCharacterXml = null;
                token.ThrowIfCancellationRequested();
                await imgSheetLanguageFlag
                    .DoThreadSafeAsync(
                        x => x.Image = FlagImageGetter.GetFlagFromCountryCode(_strExportLanguage.Substring(3, 2),
                            Math.Min(x.Width, x.Height)), token).ConfigureAwait(false);
                await DoXsltUpdate(token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
            }
        }

        private async Task DoXsltUpdate(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intLoading == 0)
            {
                if (_objCharacterXml != null)
                {
                    _strXslt = await cboXSLT.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(_strXslt))
                    {
                        CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            string strText = await LanguageManager.GetStringAsync("String_Generating_Data", token: token).ConfigureAwait(false);
                            await txtText.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                            token.ThrowIfCancellationRequested();
                            if (_dicCache.TryGetValue(
                                    new ValueTuple<string, string>(_strExportLanguage, _strXslt), out ValueTuple<string, string> strBoxText))
                            {
                                await txtText.DoThreadSafeAsync(x => x.Text = strBoxText.Item2, token).ConfigureAwait(false);
                            }
                            else
                            {
                                CancellationTokenSource objNewSource = new CancellationTokenSource();
                                CancellationToken objToken = objNewSource.Token;
                                CancellationTokenSource objTemp
                                    = Interlocked.Exchange(ref _objXmlGeneratorCancellationTokenSource, objNewSource);
                                if (objTemp?.IsCancellationRequested == false)
                                {
                                    objTemp.Cancel(false);
                                    objTemp.Dispose();
                                }

                                try
                                {
                                    token.ThrowIfCancellationRequested();
                                }
                                catch (OperationCanceledException)
                                {
                                    Interlocked.CompareExchange(ref _objXmlGeneratorCancellationTokenSource, null,
                                                                objNewSource);
                                    objNewSource.Dispose();
                                    throw;
                                }

                                Task tskOld = Interlocked.Exchange(ref _tskXmlGenerator, null);
                                try
                                {
                                    if (tskOld?.IsCompleted == false)
                                        await tskOld.ConfigureAwait(false);
                                }
                                catch (OperationCanceledException)
                                {
                                    // Swallow this
                                }
                                catch
                                {
                                    Interlocked.CompareExchange(ref _objXmlGeneratorCancellationTokenSource, null, objNewSource);
                                    objNewSource.Dispose();
                                    throw;
                                }

                                Task tskNew = _strXslt == "JSON"
                                    ? GenerateJson(objToken)
                                    : GenerateXml(objToken);
                                tskOld = Interlocked.CompareExchange(ref _tskXmlGenerator, tskNew, null);
                                if (tskOld != null && tskOld != Task.CompletedTask)
                                {
                                    Interlocked.CompareExchange(ref _objXmlGeneratorCancellationTokenSource, null, objNewSource);
                                    try
                                    {
                                        objNewSource.Cancel(false);
                                    }
                                    finally
                                    {
                                        objNewSource.Dispose();
                                    }
                                    try
                                    {
                                        await tskNew.ConfigureAwait(false);
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        //swallow this
                                    }
                                }
                                else
                                    await tskNew.ConfigureAwait(false);
                            }
                        }
                        finally
                        {
                            await objCursorWait.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    token.ThrowIfCancellationRequested();
                    CancellationTokenSource objNewSource = new CancellationTokenSource();
                    CancellationToken objToken = objNewSource.Token;
                    CancellationTokenSource objTemp
                        = Interlocked.Exchange(ref _objCharacterXmlGeneratorCancellationTokenSource, objNewSource);
                    if (objTemp?.IsCancellationRequested == false)
                    {
                        objTemp.Cancel(false);
                        objTemp.Dispose();
                    }

                    try
                    {
                        token.ThrowIfCancellationRequested();
                    }
                    catch (OperationCanceledException)
                    {
                        Interlocked.CompareExchange(ref _objCharacterXmlGeneratorCancellationTokenSource, null,
                                                    objNewSource);
                        objNewSource.Dispose();
                        throw;
                    }

                    Task tskOld = Interlocked.Exchange(ref _tskCharacterXmlGenerator, null);
                    try
                    {
                        if (tskOld?.IsCompleted == false)
                            await tskOld.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // Swallow this
                    }
                    catch
                    {
                        Interlocked.CompareExchange(ref _objCharacterXmlGeneratorCancellationTokenSource, null, objNewSource);
                        objNewSource.Dispose();
                        throw;
                    }

                    Task tskNew = GenerateCharacterXml(objToken);
                    tskOld = Interlocked.CompareExchange(ref _tskCharacterXmlGenerator, tskNew, null);
                    if (tskOld != null && tskOld != Task.CompletedTask)
                    {
                        Interlocked.CompareExchange(ref _objCharacterXmlGeneratorCancellationTokenSource, null, objNewSource);
                        try
                        {
                            objNewSource.Cancel(false);
                        }
                        finally
                        {
                            objNewSource.Dispose();
                        }
                        try
                        {
                            await tskNew.ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow this
                        }
                    }
                    else
                        await tskNew.ConfigureAwait(false);
                }
            }
        }

        private void txtText_Leave(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            _intSelected = 0;
        }

        private void txtText_MouseUp(object sender, MouseEventArgs e)
        {
            if (_intLoading > 0 || _intSelected > 0 || txtText.SelectionLength != 0 || Interlocked.Exchange(ref _intSelected, 1) > 0)
                return;
            txtText.SelectAll();
        }

        #endregion Control Events

        #region Methods

        private async Task GenerateCharacterXml(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                string strGeneratingData = await LanguageManager.GetStringAsync("String_Generating_Data", token: token).ConfigureAwait(false);
                await cmdExport.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                await cmdExportClose.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                await txtText.DoThreadSafeAsync(x => x.Text = strGeneratingData, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                XmlDocument objNewDocument;
                using (CancellationTokenSource objJoinedSource = CancellationTokenSource.CreateLinkedTokenSource(_objCharacterXmlGeneratorCancellationTokenSource.Token, token))
                {
                    objNewDocument = await _objCharacter.GenerateExportXml(_objExportCulture, _strExportLanguage,
                                                                           objJoinedSource.Token).ConfigureAwait(false);
                }

                token.ThrowIfCancellationRequested();
                if ((_objCharacterXml = objNewDocument) != null)
                    await DoXsltUpdate(token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Update the Window title to show the Character's name and unsaved changes status.
        /// </summary>
        protected async Task UpdateWindowTitleAsync(CancellationToken token = default)
        {
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            string strTitle = await LanguageManager.GetStringAsync("Title_ExportCharacter", token: token).ConfigureAwait(false) + ":" + strSpace
                              + await CharacterObject.GetCharacterNameAsync(token).ConfigureAwait(false) + strSpace + "-" + strSpace
                              + await LanguageManager.GetStringAsync(
                                  await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false) ? "Title_CareerMode" : "Title_CreateNewCharacter", token: token).ConfigureAwait(false) + strSpace
                              + "(" + await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false)).GetNameAsync(token).ConfigureAwait(false) + ")";
            await this.DoThreadSafeAsync(x => x.Text = strTitle, token).ConfigureAwait(false);
        }

        #region XML

        private async Task ExportNormal(string strDestination = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSaveFile = strDestination;
            if (string.IsNullOrEmpty(strDestination))
            {
                // Look for the file extension information.
                string strExtension = "xml";
                string exportSheetPath = Path.Combine(Utils.GetStartupPath, "export", _strXslt + ".xsl");
                using (FileStream objFileStream
                       = new FileStream(exportSheetPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    token.ThrowIfCancellationRequested();
                    using (StreamReader objFile = new StreamReader(objFileStream, Encoding.UTF8, true))
                    {
                        token.ThrowIfCancellationRequested();
                        for (string strLine = await objFile.ReadLineAsync().ConfigureAwait(false);
                             strLine != null;
                             strLine = await objFile.ReadLineAsync().ConfigureAwait(false))
                        {
                            token.ThrowIfCancellationRequested();
                            if (strLine.StartsWith("<!-- ext:", StringComparison.Ordinal))
                            {
                                strExtension = strLine.TrimStartOnce("<!-- ext:", true).FastEscapeOnceFromEnd("-->")
                                                      .Trim();
                            }
                        }
                    }
                }

                string strExtensionLower = strExtension.ToLowerInvariant();
                if (strExtension.Equals("XML", StringComparison.OrdinalIgnoreCase))
                    dlgSaveFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Xml", token: token).ConfigureAwait(false);
                else if (strExtension.Equals("JSON", StringComparison.OrdinalIgnoreCase))
                    dlgSaveFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Json", token: token).ConfigureAwait(false);
                else if (strExtension.Equals("HTM", StringComparison.OrdinalIgnoreCase) || strExtension.Equals("HTML", StringComparison.OrdinalIgnoreCase))
                    dlgSaveFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Html", token: token).ConfigureAwait(false);
                else
                    dlgSaveFile.Filter = strExtension.ToUpper(GlobalSettings.CultureInfo) + "|*." + strExtensionLower;
                dlgSaveFile.DefaultExt = strExtensionLower;
                dlgSaveFile.Title = await LanguageManager.GetStringAsync("Button_Viewer_SaveAsHtml", token: token).ConfigureAwait(false);
                if (await this.DoThreadSafeFuncAsync(x => dlgSaveFile.ShowDialog(x), token).ConfigureAwait(false) != DialogResult.OK)
                    return;
                strSaveFile = dlgSaveFile.FileName;
                if (string.IsNullOrEmpty(strSaveFile))
                    return;
                if (!strSaveFile.EndsWith("." + strExtensionLower, StringComparison.OrdinalIgnoreCase)
                    && (!strExtensionLower.TrimEndOnce('l').Equals("htm", StringComparison.OrdinalIgnoreCase)
                        || !strSaveFile.TrimEndOnce('l', 'L').EndsWith(".htm", StringComparison.OrdinalIgnoreCase)))
                    strSaveFile += "." + strExtensionLower;
            }
            if (string.IsNullOrEmpty(strSaveFile))
                return;
            if (File.Exists(strSaveFile) && !await FileExtensions.SafeDeleteAsync(strSaveFile, true, token: token).ConfigureAwait(false))
                return;

            await FileExtensions.WriteAllTextAsync(strSaveFile, // Change this to a proper path.
                _dicCache.TryGetValue(new ValueTuple<string, string>(_strExportLanguage, _strXslt), out ValueTuple<string, string> strBoxText)
                                  ? strBoxText.Item1
                                  : await txtText.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false),
                              Encoding.UTF8, token).ConfigureAwait(false);
        }

        private async Task GenerateXml(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                await cmdExport.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                await cmdExportClose.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    string exportSheetPath = Path.Combine(Utils.GetStartupPath, "export", _strXslt + ".xsl");

                    XslCompiledTransform objXslTransform;
                    try
                    {
                        objXslTransform
                            = await XslManager
                                    .GetTransformForFileAsync(exportSheetPath, token).ConfigureAwait(false); // Use the path for the export sheet.
                    }
                    catch (ArgumentException ex)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(ex, strReturn);
                        await SetTextToWorkerResult(strReturn, token).ConfigureAwait(false);
                        return;
                    }
                    catch (PathTooLongException ex)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(ex, strReturn);
                        await SetTextToWorkerResult(strReturn, token).ConfigureAwait(false);
                        return;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(ex, strReturn);
                        await SetTextToWorkerResult(strReturn, token).ConfigureAwait(false);
                        return;
                    }
                    catch (XsltException ex)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Error attempting to load " + _strXslt + Environment.NewLine;
                        ex = ex.Demystify();
                        Log.Debug(ex, strReturn);
                        strReturn += ex.Message;
                        await SetTextToWorkerResult(strReturn, token).ConfigureAwait(false);
                        return;
                    }

                    token.ThrowIfCancellationRequested();

                    XmlWriterSettings objSettings = objXslTransform.OutputSettings?.Clone();
                    if (objSettings != null)
                    {
                        objSettings.CheckCharacters = false;
                        objSettings.ConformanceLevel = ConformanceLevel.Fragment;
                    }

                    string strText;
                    using (RecyclableMemoryStream objStream = new RecyclableMemoryStream(Utils.MemoryStreamManager))
                    {
                        using (XmlWriter objWriter = objSettings != null
                                   ? XmlWriter.Create(objStream, objSettings)
                                   : Utils.GetXslTransformXmlWriter(objStream))
                        {
                            token.ThrowIfCancellationRequested();
                            objXslTransform.Transform(_objCharacterXml, objWriter);
                        }

                        token.ThrowIfCancellationRequested();
                        objStream.Position = 0;

                        // Read in the resulting code and pass it to the browser.
                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
                        {
                            token.ThrowIfCancellationRequested();
                            using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                            {
                                token.ThrowIfCancellationRequested();
                                for (string strLine = await objReader.ReadLineAsync().ConfigureAwait(false);
                                     strLine != null;
                                     strLine = await objReader.ReadLineAsync().ConfigureAwait(false))
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (!string.IsNullOrEmpty(strLine))
                                        sbdReturn.AppendLine(strLine);
                                }
                            }

                            strText = sbdReturn.ToString();
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    await SetTextToWorkerResult(strText, token).ConfigureAwait(false);
                }
                finally
                {
                    await cmdExport.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                    await cmdExportClose.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task GenerateJson(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                await cmdExport.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                await cmdExportClose.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    string strText = JsonConvert.SerializeXmlNode(_objCharacterXml, Formatting.Indented);
                    token.ThrowIfCancellationRequested();
                    await SetTextToWorkerResult(strText, token).ConfigureAwait(false);
                }
                finally
                {
                    await cmdExport.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                    await cmdExportClose.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private Task SetTextToWorkerResult(string strText, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            string strDisplayText = strText;
            try
            {
                // Displayed text has all mugshots data removed because it's unreadable as Base64 strings, but massive enough to slow down the program
                int intSnipStartIndex = strDisplayText.IndexOf("<mainmugshotbase64>", StringComparison.Ordinal);
                while (intSnipStartIndex >= 0)
                {
                    if (token.IsCancellationRequested)
                        return Task.FromCanceled(token);
                    int intSnipEndIndex = strDisplayText.IndexOf("</mainmugshotbase64>", intSnipStartIndex, StringComparison.Ordinal);
                    if (intSnipEndIndex > intSnipStartIndex)
                    {
                        string strFirstHalf = strDisplayText.Substring(0, intSnipStartIndex + 19);
                        string strSecondHalf = strDisplayText.Substring(intSnipEndIndex);
                        strDisplayText = strFirstHalf + "[...]" + strSecondHalf;
                        intSnipStartIndex = strDisplayText.IndexOf("<mainmugshotbase64>", StringComparison.Ordinal);
                    }
                    else
                        intSnipStartIndex = -1;
                }
                intSnipStartIndex = strDisplayText.IndexOf("<stringbase64>", StringComparison.Ordinal);
                while (intSnipStartIndex >= 0)
                {
                    if (token.IsCancellationRequested)
                        return Task.FromCanceled(token);
                    int intSnipEndIndex = strDisplayText.IndexOf("</stringbase64>", intSnipStartIndex, StringComparison.Ordinal);
                    if (intSnipEndIndex > intSnipStartIndex)
                    {
                        string strFirstHalf = strDisplayText.Substring(0, intSnipStartIndex + 14);
                        string strSecondHalf = strDisplayText.Substring(intSnipEndIndex);
                        strDisplayText = strFirstHalf + "[...]" + strSecondHalf;
                        intSnipStartIndex = strDisplayText.IndexOf("<stringbase64>", StringComparison.Ordinal);
                    }
                    else
                        intSnipStartIndex = -1;
                }
                intSnipStartIndex = strDisplayText.IndexOf("base64\": \"", StringComparison.Ordinal);
                while (intSnipStartIndex >= 0)
                {
                    if (token.IsCancellationRequested)
                        return Task.FromCanceled(token);
                    // Special case here, we do not want to get caught up on escaped quotation marks inside of the text
                    int intSnipEndIndex = strDisplayText.IndexOfAny(intSnipStartIndex + 10, "\\\"", "\"");
                    if (intSnipEndIndex >= 0 && strDisplayText[intSnipEndIndex] != '\"')
                        ++intSnipEndIndex;
                    if (intSnipEndIndex > intSnipStartIndex)
                    {
                        string strFirstHalf = strDisplayText.Substring(0, intSnipStartIndex + 10);
                        string strSecondHalf = strDisplayText.Substring(intSnipEndIndex);
                        strDisplayText = strFirstHalf + "[...]" + strSecondHalf;
                        intSnipStartIndex = strDisplayText.IndexOf("base64\": \"", intSnipStartIndex + 16, StringComparison.Ordinal);
                    }
                    else
                        intSnipStartIndex = -1;
                }
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                _dicCache.AddOrUpdate(new ValueTuple<string, string>(_strExportLanguage, _strXslt),
                    new ValueTuple<string, string>(strText, strDisplayText),
                    (a, b) => new ValueTuple<string, string>(strText, strDisplayText));
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
            return txtText.DoThreadSafeAsync(x => x.Text = strDisplayText, token);
        }

        #endregion XML

        #region JSON

        private async Task ExportJson(string strDestination = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSaveFile = strDestination;
            if (string.IsNullOrEmpty(strSaveFile))
            {
                dlgSaveFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Json", token: token).ConfigureAwait(false) + "|" + await LanguageManager.GetStringAsync("DialogFilter_All", token: token).ConfigureAwait(false);
                dlgSaveFile.Title = await LanguageManager.GetStringAsync("Button_Export_SaveJsonAs", token: token).ConfigureAwait(false);
                if (await this.DoThreadSafeFuncAsync(x => dlgSaveFile.ShowDialog(x), token).ConfigureAwait(false) != DialogResult.OK)
                    return;
                strSaveFile = dlgSaveFile.FileName;
                if (string.IsNullOrWhiteSpace(strSaveFile))
                    return;
                if (!strSaveFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    strSaveFile += ".json";
            }
            if (string.IsNullOrWhiteSpace(strSaveFile))
                return;
            if (File.Exists(strSaveFile) && !await FileExtensions.SafeDeleteAsync(strSaveFile, true, token: token).ConfigureAwait(false))
                return;

            // Change this to a proper path.
            await FileExtensions.WriteAllTextAsync(strSaveFile,
                _dicCache.TryGetValue(new ValueTuple<string, string>(_strExportLanguage, _strXslt),
                    out ValueTuple<string, string> strBoxText)
                    ? strBoxText.Item1
                    : await txtText.DoThreadSafeFuncAsync(x => x.Text, token: token)
                        .ConfigureAwait(false), Encoding.UTF8, token).ConfigureAwait(false);
        }

        #endregion JSON

        #endregion Methods
    }
}
