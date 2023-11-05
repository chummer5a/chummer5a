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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private readonly ConcurrentDictionary<Tuple<string, string>, Tuple<string, string>> _dicCache = new ConcurrentDictionary<Tuple<string, string>, Tuple<string, string>>();
        private CancellationTokenSource _objCharacterXmlGeneratorCancellationTokenSource;
        private CancellationTokenSource _objXmlGeneratorCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericFormClosingCancellationTokenSource = new CancellationTokenSource();
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
            _objGenericToken = _objGenericFormClosingCancellationTokenSource.Token;
            Disposed += (sender, args) =>
            {
                _objGenericFormClosingCancellationTokenSource.Dispose();
                _objXmlGeneratorCancellationTokenSource?.Dispose();
                _objCharacterXmlGeneratorCancellationTokenSource?.Dispose();
            };
            _objCharacter = objCharacter;
            Program.MainForm.OpenCharacterExportForms?.Add(this);
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
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
                    _objCharacter.PropertyChanged += ObjCharacterOnPropertyChanged;
                    _objCharacter.SettingsPropertyChanged += ObjCharacterOnSettingsPropertyChanged;
                    // TODO: Make these also work for any children collection changes
                    _objCharacter.Cyberware.CollectionChanged += OnCharacterCollectionChanged;
                    _objCharacter.Armor.CollectionChanged += OnCharacterCollectionChanged;
                    _objCharacter.Weapons.CollectionChanged += OnCharacterCollectionChanged;
                    _objCharacter.Gear.CollectionChanged += OnCharacterCollectionChanged;
                    _objCharacter.Contacts.CollectionChanged += OnCharacterCollectionChanged;
                    _objCharacter.ExpenseEntries.CollectionChanged += OnCharacterCollectionChanged;
                    _objCharacter.MentorSpirits.CollectionChanged += OnCharacterCollectionChanged;
                    _objCharacter.Powers.ListChanged += OnCharacterListChanged;
                    _objCharacter.Qualities.CollectionChanged += OnCharacterCollectionChanged;
                    _objCharacter.MartialArts.CollectionChanged += OnCharacterCollectionChanged;
                    _objCharacter.Metamagics.CollectionChanged += OnCharacterCollectionChanged;
                    _objCharacter.Spells.CollectionChanged += OnCharacterCollectionChanged;
                    _objCharacter.ComplexForms.CollectionChanged += OnCharacterCollectionChanged;
                    _objCharacter.CritterPowers.CollectionChanged += OnCharacterCollectionChanged;
                    _objCharacter.SustainedCollection.CollectionChanged += OnCharacterCollectionChanged;
                    _objCharacter.InitiationGrades.CollectionChanged += OnCharacterCollectionChanged;
                }
                finally
                {
                    await objInnerLocker.DisposeAsync().ConfigureAwait(false);
                }

                await LanguageManager
                      .PopulateSheetLanguageListAsync(cboLanguage, GlobalSettings.DefaultCharacterSheet,
                                                      _objCharacter.Yield(), _objExportCulture, token: _objGenericToken)
                      .ConfigureAwait(false);
                using (new FetchSafelyFromPool<List<ListItem>>(
                           Utils.ListItemListPool, out List<ListItem> lstExportMethods))
                {
                    // Populate the XSLT list with all of the XSL files found in the sheets directory.
                    foreach (string strFile in Directory.EnumerateFiles(Path.Combine(Utils.GetStartupPath, "export")))
                    {
                        // Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets (hidden because they are partial templates that cannot be used on their own).
                        if (!strFile.EndsWith(".xslt", StringComparison.OrdinalIgnoreCase)
                            && strFile.EndsWith(".xsl", StringComparison.OrdinalIgnoreCase))
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

        private async void ObjCharacterOnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == nameof(CharacterSettings.Name))
                    await UpdateWindowTitleAsync(_objGenericToken).ConfigureAwait(false);
                await DoXsltUpdate(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void ObjCharacterOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == nameof(Character.CharacterName) || e.PropertyName == nameof(Character.Created))
                    await UpdateWindowTitleAsync(_objGenericToken).ConfigureAwait(false);
                await DoXsltUpdate(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void OnCharacterListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemMoved
                || e.ListChangedType == ListChangedType.PropertyDescriptorAdded
                || e.ListChangedType == ListChangedType.PropertyDescriptorChanged
                || e.ListChangedType == ListChangedType.PropertyDescriptorDeleted)
                return;
            try
            {
                await DoXsltUpdate(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void OnCharacterCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;
            try
            {
                await DoXsltUpdate(_objGenericToken).ConfigureAwait(false);
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
            IAsyncDisposable objInnerLocker = await _objCharacter.LockObject.EnterWriteLockAsync()
                                                                 .ConfigureAwait(false);
            try
            {
                _objCharacter.PropertyChanged -= ObjCharacterOnPropertyChanged;
                _objCharacter.SettingsPropertyChanged -= ObjCharacterOnSettingsPropertyChanged;
                _objCharacter.Cyberware.CollectionChanged -= OnCharacterCollectionChanged;
                _objCharacter.Armor.CollectionChanged -= OnCharacterCollectionChanged;
                _objCharacter.Weapons.CollectionChanged -= OnCharacterCollectionChanged;
                _objCharacter.Gear.CollectionChanged -= OnCharacterCollectionChanged;
                _objCharacter.Contacts.CollectionChanged -= OnCharacterCollectionChanged;
                _objCharacter.ExpenseEntries.CollectionChanged -= OnCharacterCollectionChanged;
                _objCharacter.MentorSpirits.CollectionChanged -= OnCharacterCollectionChanged;
                _objCharacter.Powers.ListChanged -= OnCharacterListChanged;
                _objCharacter.Qualities.CollectionChanged -= OnCharacterCollectionChanged;
                _objCharacter.MartialArts.CollectionChanged -= OnCharacterCollectionChanged;
                _objCharacter.Metamagics.CollectionChanged -= OnCharacterCollectionChanged;
                _objCharacter.Spells.CollectionChanged -= OnCharacterCollectionChanged;
                _objCharacter.ComplexForms.CollectionChanged -= OnCharacterCollectionChanged;
                _objCharacter.CritterPowers.CollectionChanged -= OnCharacterCollectionChanged;
                _objCharacter.SustainedCollection.CollectionChanged -= OnCharacterCollectionChanged;
                _objCharacter.InitiationGrades.CollectionChanged -= OnCharacterCollectionChanged;
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

        private async ValueTask DoExport(CancellationToken token = default)
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
                                    new Tuple<string, string>(_strExportLanguage, _strXslt), out Tuple<string, string> strBoxText))
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
                                    ? Task.Run(() => GenerateJson(objToken), objToken)
                                    : Task.Run(() => GenerateXml(objToken), objToken);
                                if (Interlocked.CompareExchange(ref _tskXmlGenerator, tskNew, null) != null)
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

                    Task tskNew = Task.Run(() => GenerateCharacterXml(objToken), objToken);
                    if (Interlocked.CompareExchange(ref _tskCharacterXmlGenerator, tskNew, null) != null)
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
                using (token.Register(() => _objCharacterXmlGeneratorCancellationTokenSource.Cancel(false)))
                {
                    objNewDocument = await _objCharacter.GenerateExportXml(_objExportCulture, _strExportLanguage,
                                                                           _objCharacterXmlGeneratorCancellationTokenSource
                                                                               .Token).ConfigureAwait(false);
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
            string strTitle = await LanguageManager.GetStringAsync("Title_ExportCharacter", token: token).ConfigureAwait(false) + ':' + strSpace
                              + CharacterObject.CharacterName + strSpace + '-' + strSpace
                              + await LanguageManager.GetStringAsync(
                                  CharacterObject.Created ? "Title_CareerMode" : "Title_CreateNewCharacter", token: token).ConfigureAwait(false) + strSpace
                              + '(' + CharacterObject.Settings.Name + ')';
            await this.DoThreadSafeAsync(x => x.Text = strTitle, token).ConfigureAwait(false);
        }

        #region XML

        private async ValueTask ExportNormal(string destination = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSaveFile = destination;
            if (string.IsNullOrEmpty(destination))
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
                dlgSaveFile.Title = await LanguageManager.GetStringAsync("Button_Viewer_SaveAsHtml", token: token).ConfigureAwait(false);
                if (await this.DoThreadSafeFuncAsync(x => dlgSaveFile.ShowDialog(x), token).ConfigureAwait(false) != DialogResult.OK)
                    return;
                strSaveFile = dlgSaveFile.FileName;
                if (string.IsNullOrEmpty(strSaveFile))
                    return;
                if (!strSaveFile.EndsWith('.' + strExtensionLower, StringComparison.OrdinalIgnoreCase)
                    && (!strExtensionLower.TrimEndOnce('l').Equals("htm", StringComparison.OrdinalIgnoreCase)
                        || !strSaveFile.TrimEndOnce('l', 'L').EndsWith(".htm", StringComparison.OrdinalIgnoreCase)))
                    strSaveFile += '.' + strExtensionLower;
            }
            if (string.IsNullOrEmpty(strSaveFile))
                return;

            File.WriteAllText(strSaveFile, // Change this to a proper path.
                _dicCache.TryGetValue(new Tuple<string, string>(_strExportLanguage, _strXslt), out Tuple<string, string> strBoxText)
                                  ? strBoxText.Item1
                                  : txtText.Text,
                              Encoding.UTF8);
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
                    catch (ArgumentException)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(strReturn);
                        await SetTextToWorkerResult(strReturn, token).ConfigureAwait(false);
                        return;
                    }
                    catch (PathTooLongException)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(strReturn);
                        await SetTextToWorkerResult(strReturn, token).ConfigureAwait(false);
                        return;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(strReturn);
                        await SetTextToWorkerResult(strReturn, token).ConfigureAwait(false);
                        return;
                    }
                    catch (XsltException ex)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Error attempting to load " + _strXslt + Environment.NewLine;
                        Log.Debug(strReturn);
                        Log.Error("ERROR Message = " + ex.Message);
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

                    string strText = await Task.Run(async () =>
                    {
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
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
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

                                return sbdReturn.ToString();
                            }
                        }
                    }, token).ConfigureAwait(false);

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

        private async ValueTask SetTextToWorkerResult(string strText, CancellationToken token = default)
        {
            string strDisplayText = strText;
            // Displayed text has all mugshots data removed because it's unreadable as Base64 strings, but massive enough to slow down the program
            strDisplayText = s_RgxMainMugshotReplaceExpression.Replace(strDisplayText, "<mainmugshotbase64>[...]</mainmugshotbase64>");
            strDisplayText = s_RgxStringBase64ReplaceExpression.Replace(strDisplayText, "<stringbase64>[...]</stringbase64>");
            strDisplayText = s_RgxBase64ReplaceExpression.Replace(strDisplayText, "base64\": \"[...]\",");
            _dicCache.AddOrUpdate(new Tuple<string, string>(_strExportLanguage, _strXslt),
                new Tuple<string, string>(strText, strDisplayText),
                (a, b) => new Tuple<string, string>(strText, strDisplayText));
            await txtText.DoThreadSafeAsync(x => x.Text = strDisplayText, token).ConfigureAwait(false);
        }

        private static readonly Regex s_RgxMainMugshotReplaceExpression = new Regex(
            "<mainmugshotbase64>[^\\s\\S]*</mainmugshotbase64>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex s_RgxStringBase64ReplaceExpression = new Regex(
            "<stringbase64>[^\\s\\S]*</stringbase64>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex s_RgxBase64ReplaceExpression = new Regex(
            "base64\": \"[^\\\"]*\",",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        #endregion XML

        #region JSON

        private async ValueTask ExportJson(string destination = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSaveFile = destination;
            if (string.IsNullOrEmpty(strSaveFile))
            {
                dlgSaveFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Json", token: token).ConfigureAwait(false) + '|' + await LanguageManager.GetStringAsync("DialogFilter_All", token: token).ConfigureAwait(false);
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

            // Change this to a proper path.
            await FileExtensions.WriteAllTextAsync(strSaveFile,
                _dicCache.TryGetValue(new Tuple<string, string>(_strExportLanguage, _strXslt),
                    out Tuple<string, string> strBoxText)
                    ? strBoxText.Item1
                    : await txtText.DoThreadSafeFuncAsync(x => x.Text, token: token)
                        .ConfigureAwait(false), Encoding.UTF8).ConfigureAwait(false);
        }

        #endregion JSON

        #endregion Methods
    }
}
