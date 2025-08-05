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
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;
using NLog;

namespace Chummer
{
    /// <summary>
    /// A Technomancer Program or Complex Form.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayName(\"en-us\")}")]
    public sealed class ComplexForm : IHasInternalId, IHasName, IHasSourceId, IHasXmlDataNode, IHasNotes, ICanRemove, IHasSource, IHasLockObject, IHasCharacterObject
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guiID;
        private Guid _guiSourceID = Guid.Empty;
        private string _strName = string.Empty;
        private string _strUseSkill = string.Empty;
        private string _strTarget = string.Empty;
        private string _strDuration = string.Empty;
        private string _strFv = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private string _strExtra = string.Empty;
        private int _intGrade;
        private readonly Character _objCharacter;
        private SourceString _objCachedSourceDetail;

        public Character CharacterObject => _objCharacter; // readonly member, no locking needed

        #region Constructor, Create, Save, Load, and Print Methods

        public ComplexForm(Character objCharacter)
        {
            // Create the GUID for the new Complex Form.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
            LockObject = objCharacter.LockObject;
        }

        /// <summary>
        /// Create a Complex Form from an XmlNode.
        /// </summary>
        /// <param name="objXmlComplexFormNode">XmlNode to create the object from.</param>
        /// <param name="strExtra">Value to forcefully select for any ImprovementManager prompts.</param>
        public void Create(XmlNode objXmlComplexFormNode, string strExtra = "")
        {
            using (LockObject.EnterWriteLock())
            {
                if (!objXmlComplexFormNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
                {
                    Log.Warn(new object[] { "Missing id field for complex form xmlnode", objXmlComplexFormNode });
                    Utils.BreakIfDebug();
                }
                objXmlComplexFormNode.TryGetField("id", Guid.TryParse, out _guiSourceID);
                objXmlComplexFormNode.TryGetStringFieldQuickly("name", ref _strName);
                objXmlComplexFormNode.TryGetStringFieldQuickly("useskill", ref _strUseSkill);
                objXmlComplexFormNode.TryGetStringFieldQuickly("target", ref _strTarget);
                objXmlComplexFormNode.TryGetStringFieldQuickly("source", ref _strSource);
                objXmlComplexFormNode.TryGetStringFieldQuickly("page", ref _strPage);
                objXmlComplexFormNode.TryGetStringFieldQuickly("duration", ref _strDuration);
                objXmlComplexFormNode.TryGetStringFieldQuickly("fv", ref _strFv);
                if (!objXmlComplexFormNode.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                    objXmlComplexFormNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objXmlComplexFormNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
                {
                    Notes = CommonFunctions.GetBookNotes(objXmlComplexFormNode, Name, CurrentDisplayName, Source, Page,
                        DisplayPage(GlobalSettings.Language), _objCharacter);
                }

                if (objXmlComplexFormNode["bonus"] != null)
                {
                    ImprovementManager.SetForcedValue(Extra, _objCharacter);
                    if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.ComplexForm, _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), objXmlComplexFormNode["bonus"], 1, CurrentDisplayNameShort))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                    string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                    if (!string.IsNullOrEmpty(strSelectedValue))
                    {
                        _strExtra = strSelectedValue;
                    }
                }
                else
                {
                    _strExtra = strExtra;
                }
            }
        }

        /// <summary>
        /// Create a Complex Form from an XmlNode.
        /// </summary>
        /// <param name="objXmlComplexFormNode">XmlNode to create the object from.</param>
        /// <param name="strExtra">Value to forcefully select for any ImprovementManager prompts.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task CreateAsync(XmlNode objXmlComplexFormNode, string strExtra = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!objXmlComplexFormNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
                {
                    Log.Warn(new object[] { "Missing id field for complex form xmlnode", objXmlComplexFormNode });
                    Utils.BreakIfDebug();
                }
                objXmlComplexFormNode.TryGetField("id", Guid.TryParse, out _guiSourceID);
                objXmlComplexFormNode.TryGetStringFieldQuickly("name", ref _strName);
                objXmlComplexFormNode.TryGetStringFieldQuickly("useskill", ref _strUseSkill);
                objXmlComplexFormNode.TryGetStringFieldQuickly("target", ref _strTarget);
                objXmlComplexFormNode.TryGetStringFieldQuickly("source", ref _strSource);
                objXmlComplexFormNode.TryGetStringFieldQuickly("page", ref _strPage);
                objXmlComplexFormNode.TryGetStringFieldQuickly("duration", ref _strDuration);
                objXmlComplexFormNode.TryGetStringFieldQuickly("fv", ref _strFv);
                if (!objXmlComplexFormNode.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                    objXmlComplexFormNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objXmlComplexFormNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false)))
                {
                    await SetNotesAsync(await CommonFunctions.GetBookNotesAsync(objXmlComplexFormNode, Name, await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), Source, Page,
                        await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false), _objCharacter, token).ConfigureAwait(false), token).ConfigureAwait(false);
                }

                if (objXmlComplexFormNode["bonus"] != null)
                {
                    ImprovementManager.SetForcedValue(Extra, _objCharacter);
                    if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter, Improvement.ImprovementSource.ComplexForm, _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), objXmlComplexFormNode["bonus"], 1, await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                    string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                    if (!string.IsNullOrEmpty(strSelectedValue))
                    {
                        _strExtra = strSelectedValue;
                    }
                }
                else
                {
                    _strExtra = strExtra;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            using (LockObject.EnterReadLock())
            {
                objWriter.WriteStartElement("complexform");
                objWriter.WriteElementString("sourceid", SourceIDString);
                objWriter.WriteElementString("guid", InternalId);
                objWriter.WriteElementString("name", _strName);
                objWriter.WriteElementString("useskill", _strUseSkill);
                objWriter.WriteElementString("target", _strTarget);
                objWriter.WriteElementString("duration", _strDuration);
                objWriter.WriteElementString("fv", _strFv);
                objWriter.WriteElementString("extra", _strExtra);
                objWriter.WriteElementString("source", _strSource);
                objWriter.WriteElementString("page", _strPage);
                objWriter.WriteElementString("notes", _strNotes.CleanOfXmlInvalidUnicodeChars());
                objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
                objWriter.WriteElementString("grade", _intGrade.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Load the Complex Form from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            using (LockObject.EnterWriteLock())
            {
                if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                {
                    _guiID = Guid.NewGuid();
                }
                objNode.TryGetStringFieldQuickly("name", ref _strName);
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
                if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
                {
                    this.GetNodeXPath()?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }

                objNode.TryGetStringFieldQuickly("useskill", ref _strUseSkill);
                objNode.TryGetStringFieldQuickly("target", ref _strTarget);
                objNode.TryGetStringFieldQuickly("source", ref _strSource);
                objNode.TryGetStringFieldQuickly("page", ref _strPage);
                objNode.TryGetStringFieldQuickly("duration", ref _strDuration);
                objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
                objNode.TryGetStringFieldQuickly("fv", ref _strFv);
                objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task Print(XmlWriter objWriter, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // <complexform>
                XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("complexform", token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("fullname", await DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
                    await objWriter
                            .WriteElementStringAsync(
                                "fullname_english", await DisplayNameAsync(GlobalSettings.DefaultLanguage, token).ConfigureAwait(false),
                                token)
                            .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("duration", await DisplayDurationAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("duration_english", await DisplayDurationAsync(GlobalSettings.DefaultLanguage, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("fv", await DisplayFvAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("fv_english", await DisplayFvAsync(GlobalSettings.DefaultLanguage, token), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("target", await DisplayTargetAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("target_english", await DisplayTargetAsync(GlobalSettings.DefaultLanguage, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("source", await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    if (GlobalSettings.PrintNotes)
                        await objWriter.WriteElementStringAsync("notes", await GetNotesAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                }
                finally
                {
                    // </complexform>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiSourceID;
            }
        }

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// Internal identifier which will be used to identify this Complex Form in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// Complex Form's name.
        /// </summary>
        public string Name
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strName;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strName, value) == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _objCachedMyXmlNode = null;
                        _objCachedMyXPathNode = null;
                    }
                }
            }
        }

        public SourceString SourceDetail
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return _objCachedSourceDetail == default
                        ? _objCachedSourceDetail = SourceString.GetSourceString(Source,
                            DisplayPage(GlobalSettings.Language),
                            GlobalSettings.Language,
                            GlobalSettings.CultureInfo,
                            _objCharacter)
                        : _objCachedSourceDetail;
                }
            }
        }

        public async Task<SourceString> GetSourceDetailAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _objCachedSourceDetail == default
                    ? _objCachedSourceDetail = await SourceString.GetSourceStringAsync(Source,
                        await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false),
                        GlobalSettings.Language,
                        GlobalSettings.CultureInfo,
                        _objCharacter, token).ConfigureAwait(false)
                    : _objCachedSourceDetail;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Complex Form's extra info.
        /// </summary>
        public string Extra
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strExtra;
            }
            set
            {
                value = _objCharacter.ReverseTranslateExtra(value);
                using (LockObject.EnterUpgradeableReadLock())
                    _strExtra = value;
            }
        }

        /// <summary>
        /// Complex Form's grade.
        /// </summary>
        public int Grade
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intGrade;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _intGrade = value;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn = Name;
                // Get the translated name if applicable.
                if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    strReturn = this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;

                return strReturn;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            string strReturn = Name;
            // Get the translated name if applicable.
            if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                strReturn = objNode != null ? objNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? Name : Name;
            }

            return strReturn;
        }

        public string DisplayName(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn = DisplayNameShort(strLanguage);

                if (!string.IsNullOrEmpty(Extra))
                {
                    string strExtra = Extra;
                    if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                        strExtra = _objCharacter.TranslateExtra(Extra, strLanguage);
                    strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + strExtra + ')';
                }
                return strReturn;
            }
        }

        public async Task<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(Extra))
            {
                string strExtra = Extra;
                if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    strExtra = await _objCharacter.TranslateExtraAsync(Extra, strLanguage, token: token).ConfigureAwait(false);
                strReturn += await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false) + '(' + strExtra + ')';
            }
            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.Language, token);

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) => DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        /// Translated Duration.
        /// </summary>
        public string DisplayDuration(string strLanguage)
        {
            switch (Duration)
            {
                case "P":
                    return LanguageManager.GetString("String_SpellDurationPermanent", strLanguage);

                case "S":
                    return LanguageManager.GetString("String_SpellDurationSustained", strLanguage);

                case "I":
                    return LanguageManager.GetString("String_SpellDurationInstant", strLanguage);

                case "Special":
                    return LanguageManager.GetString("String_SpellDurationSpecial", strLanguage);

                default:
                    return LanguageManager.GetString("String_None", strLanguage);
            }
        }

        /// <summary>
        /// Translated Duration.
        /// </summary>
        public Task<string> DisplayDurationAsync(string strLanguage, CancellationToken token = default)
        {
            switch (Duration)
            {
                case "P":
                    return LanguageManager.GetStringAsync("String_SpellDurationPermanent", strLanguage, token: token);

                case "S":
                    return LanguageManager.GetStringAsync("String_SpellDurationSustained", strLanguage, token: token);

                case "I":
                    return LanguageManager.GetStringAsync("String_SpellDurationInstant", strLanguage, token: token);

                case "Special":
                    return LanguageManager.GetStringAsync("String_SpellDurationSpecial", strLanguage, token: token);

                default:
                    return LanguageManager.GetStringAsync("String_None", strLanguage, token: token);
            }
        }

        /// <summary>
        /// Complex Form's Duration.
        /// </summary>
        public string Duration
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strDuration;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strDuration = value;
            }
        }

        /// <summary>
        /// Translated Fading Value.
        /// </summary>
        public string DisplayFv(string strLanguage)
        {
            string strReturn = CalculatedFv.Replace('/', '÷').Replace('*', '×');
            if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strReturn = strReturn.CheapReplace("L", () => LanguageManager.GetString("String_ComplexFormLevel", strLanguage))
                    .CheapReplace("Overflow damage", () => LanguageManager.GetString("String_SpellOverflowDamage", strLanguage))
                    .CheapReplace("Damage Value", () => LanguageManager.GetString("String_SpellDamageValue", strLanguage))
                    .CheapReplace("Toxin DV", () => LanguageManager.GetString("String_SpellToxinDV", strLanguage))
                    .CheapReplace("Disease DV", () => LanguageManager.GetString("String_SpellDiseaseDV", strLanguage))
                    .CheapReplace("Radiation Power", () => LanguageManager.GetString("String_SpellRadiationPower", strLanguage))
                    .CheapReplace("Special", () => LanguageManager.GetString("String_Special", strLanguage));
            }
            return strReturn;
        }

        /// <summary>
        /// Translated Fading Value.
        /// </summary>
        public async Task<string> DisplayFvAsync(string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strReturn = (await GetCalculatedFvAsync(token)).Replace('/', '÷').Replace('*', '×');
            if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strReturn = await strReturn
                                  .CheapReplaceAsync(
                                      "L", () => LanguageManager.GetStringAsync("String_ComplexFormLevel", strLanguage, token: token), token: token)
                                  .CheapReplaceAsync("Overflow damage",
                                                     () => LanguageManager.GetStringAsync(
                                                         "String_SpellOverflowDamage", strLanguage, token: token), token: token)
                                  .CheapReplaceAsync("Damage Value",
                                                     () => LanguageManager.GetStringAsync(
                                                         "String_SpellDamageValue", strLanguage, token: token), token: token)
                                  .CheapReplaceAsync(
                                      "Toxin DV", () => LanguageManager.GetStringAsync("String_SpellToxinDV", strLanguage, token: token), token: token)
                                  .CheapReplaceAsync("Disease DV",
                                                     () => LanguageManager.GetStringAsync(
                                                         "String_SpellDiseaseDV", strLanguage, token: token), token: token)
                                  .CheapReplaceAsync("Radiation Power",
                                                     () => LanguageManager.GetStringAsync(
                                                         "String_SpellRadiationPower", strLanguage, token: token), token: token)
                                  .CheapReplaceAsync(
                                      "Special", () => LanguageManager.GetStringAsync("String_Special", strLanguage, token: token), token: token).ConfigureAwait(false);
            }
            return strReturn;
        }

        /// <summary>
        /// Fading Tooltip.
        /// </summary>
        public async Task<string> GetFvTooltipAsync(CancellationToken token = default)
        {
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token);
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intRes = await (await _objCharacter.GetAttributeAsync("RES", token: token).ConfigureAwait(false))
                    .GetTotalValueAsync(token).ConfigureAwait(false);
                int intHighestLevel = intRes * 2;
                string strFv = await GetCalculatedFvAsync(token).ConfigureAwait(false);
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                out StringBuilder sbdTip))
                {
                    sbdTip.Append(await LanguageManager.GetStringAsync("Tip_ComplexFormFading", token: token));
                    for (int i = 1; i <= intHighestLevel; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        // Calculate the Complex Form's Fading for the current Level.
                        (bool blnIsSuccess, object xprResult) = await CommonFunctions.EvaluateInvariantXPathAsync(
                            strFv.Replace("L", i.ToString(GlobalSettings.InvariantCultureInfo)), token: token);

                        if (blnIsSuccess && strFv != "Special")
                        {
                            // Fading is always minimum 2
                            int intFv = Math.Max(((double)xprResult).StandardRound(), 2);
                            sbdTip.AppendLine().Append(await LanguageManager.GetStringAsync("String_Level", token: token).ConfigureAwait(false)).Append(strSpace)
                                    .Append(
                                        i.ToString(GlobalSettings.CultureInfo))
                                    .Append(await LanguageManager.GetStringAsync("String_Colon", token: token).ConfigureAwait(false))
                                    .Append(strSpace).Append(intFv.ToString(GlobalSettings.CultureInfo));
                        }
                        else
                        {
                            sbdTip.Clear();
                            sbdTip.Append(await LanguageManager.GetStringAsync("Tip_ComplexFormFadingSeeDescription", token: token).ConfigureAwait(false));
                            break;
                        }
                    }

                    sbdTip.AppendLine().Append(await LanguageManager.GetStringAsync("Tip_ComplexFormFadingBase", token: token).ConfigureAwait(false)).Append(strSpace).Append('(').Append(FvBase).Append(')');
                    foreach (Improvement objLoopImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                    _objCharacter, Improvement.ImprovementType.FadingValue, Name, true, token))
                    {
                        sbdTip.Append(strSpace).Append('+').Append(strSpace).Append(await _objCharacter.GetObjectNameAsync(objLoopImprovement, token: token).ConfigureAwait(false)).Append(strSpace)
                                .Append('(').Append(objLoopImprovement.Value.ToString("0;-0;0", GlobalSettings.CultureInfo)).Append(')');
                    }
                    // Minimum Fading of 2
                    sbdTip.AppendLine().AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_MinimumAttribute", token: token), 2);
                    return sbdTip.ToString();
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The Complex Form's FV.
        /// </summary>
        public string CalculatedFv
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    string strReturn = FvBase;
                    List<Improvement> lstImprovements
                        = ImprovementManager.GetCachedImprovementListForValueOf(
                            _objCharacter, Improvement.ImprovementType.FadingValue, Name, true);
                    if (lstImprovements.Count > 0)
                    {
                        bool blnForce = strReturn.StartsWith('L');
                        string strFv = blnForce ? strReturn.TrimStartOnce("L", true) : strReturn;
                        //Navigator can't do math on a single value, so inject a mathable value.
                        if (string.IsNullOrEmpty(strFv))
                        {
                            strFv = "0";
                        }
                        else
                        {
                            int intPos = strReturn.IndexOf('-');
                            if (intPos != -1)
                            {
                                strFv = strReturn.Substring(intPos);
                            }
                            else
                            {
                                intPos = strReturn.IndexOf('+');
                                if (intPos != -1)
                                {
                                    strFv = strReturn.Substring(intPos);
                                }
                            }
                        }

                        string strToAppend = string.Empty;
                        int intFadingDv = 0;
                        if (strFv.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                        {
                            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdReturn))
                            {
                                sbdReturn.Append(strFv);
                                foreach (Improvement objImprovement in lstImprovements)
                                {
                                    sbdReturn.AppendFormat(GlobalSettings.InvariantCultureInfo, "{0:+0;-0;+0}",
                                                       objImprovement.Value);
                                }
                                _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdReturn);
                                (bool blnIsSuccess, object xprResult) = CommonFunctions.EvaluateInvariantXPath(sbdReturn.ToString());
                                if (blnIsSuccess)
                                    intFadingDv = ((double)xprResult).StandardRound();
                                else
                                    strToAppend = sbdReturn.ToString();
                            }
                        }
                        else
                        {
                            intFadingDv = decValue.StandardRound();
                        }

                        if (blnForce)
                        {
                            if (!string.IsNullOrEmpty(strToAppend))
                                strReturn += "L" + strToAppend;
                            else
                                strReturn = string.Format(GlobalSettings.InvariantCultureInfo, "L{0:+0;-0;}", intFadingDv);
                        }
                        else if (!string.IsNullOrEmpty(strToAppend))
                            strReturn += strToAppend;
                        else
                            // Fading always minimum 2
                            strReturn = Math.Max(intFadingDv, 2).ToString(GlobalSettings.InvariantCultureInfo);

                        return strReturn;
                    }
                    return strReturn;
                }
            }
        }

        /// <summary>
        /// The Complex Form's FV.
        /// </summary>
        public async Task<string> GetCalculatedFvAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn = FvBase;
                List<Improvement> lstImprovements
                    = await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                        _objCharacter, Improvement.ImprovementType.FadingValue, Name, true, token).ConfigureAwait(false);
                if (lstImprovements.Count > 0)
                {
                    bool blnForce = strReturn.StartsWith('L');
                    string strFv = blnForce ? strReturn.TrimStartOnce("L", true) : strReturn;
                    //Navigator can't do math on a single value, so inject a mathable value.
                    if (string.IsNullOrEmpty(strFv))
                    {
                        strFv = "0";
                    }
                    else
                    {
                        int intPos = strReturn.IndexOf('-');
                        if (intPos != -1)
                        {
                            strFv = strReturn.Substring(intPos);
                        }
                        else
                        {
                            intPos = strReturn.IndexOf('+');
                            if (intPos != -1)
                            {
                                strFv = strReturn.Substring(intPos);
                            }
                        }
                    }

                    string strToAppend = string.Empty;
                    int intFadingDv = 0;
                    if (strFv.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                    {
                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdReturn))
                        {
                            sbdReturn.Append(strFv);
                            foreach (Improvement objImprovement in lstImprovements)
                            {
                                sbdReturn.AppendFormat(GlobalSettings.InvariantCultureInfo, "{0:+0;-0;+0}",
                                                   objImprovement.Value);
                            }
                            await (await _objCharacter.GetAttributeSectionAsync(token).ConfigureAwait(false)).ProcessAttributesInXPathAsync(sbdReturn, token: token).ConfigureAwait(false);
                            (bool blnIsSuccess, object xprResult) = await CommonFunctions.EvaluateInvariantXPathAsync(sbdReturn.ToString(), token).ConfigureAwait(false);
                            if (blnIsSuccess)
                                intFadingDv = ((double)xprResult).StandardRound();
                            else
                                strToAppend = sbdReturn.ToString();
                        }
                    }
                    else
                    {
                        intFadingDv = decValue.StandardRound();
                    }

                    if (blnForce)
                    {
                        if (!string.IsNullOrEmpty(strToAppend))
                            strReturn += "L" + strToAppend;
                        else
                            strReturn = string.Format(GlobalSettings.InvariantCultureInfo, "L{0:+0;-0;}", intFadingDv);
                    }
                    else if (!string.IsNullOrEmpty(strToAppend))
                        strReturn += strToAppend;
                    else
                        // Fading always minimum 2
                        strReturn = Math.Max(intFadingDv, 2).ToString(GlobalSettings.InvariantCultureInfo);

                    return strReturn;
                }
                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string FvBase
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strFv;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strFv = value;
            }
        }

        /// <summary>
        /// Translated Duration.
        /// </summary>
        public string DisplayTarget(string strLanguage)
        {
            switch (Target)
            {
                case "Persona":
                    return LanguageManager.GetString("String_ComplexFormTargetPersona", strLanguage);

                case "Device":
                    return LanguageManager.GetString("String_ComplexFormTargetDevice", strLanguage);

                case "File":
                    return LanguageManager.GetString("String_ComplexFormTargetFile", strLanguage);

                case "Self":
                    return LanguageManager.GetString("String_SpellRangeSelf", strLanguage);

                case "Sprite":
                    return LanguageManager.GetString("String_ComplexFormTargetSprite", strLanguage);

                case "Host":
                    return LanguageManager.GetString("String_ComplexFormTargetHost", strLanguage);

                case "IC":
                    return LanguageManager.GetString("String_ComplexFormTargetIC", strLanguage);

                case "Icon":
                    return LanguageManager.GetString("String_ComplexFormTargetIcon", strLanguage);

                case "Special":
                    return LanguageManager.GetString("String_Special", strLanguage);

                default:
                    return LanguageManager.GetString("String_None", strLanguage);
            }
        }

        /// <summary>
        /// Translated Duration.
        /// </summary>
        public Task<string> DisplayTargetAsync(string strLanguage, CancellationToken token = default)
        {
            switch (Target)
            {
                case "Persona":
                    return LanguageManager.GetStringAsync("String_ComplexFormTargetPersona", strLanguage, token: token);

                case "Device":
                    return LanguageManager.GetStringAsync("String_ComplexFormTargetDevice", strLanguage, token: token);

                case "File":
                    return LanguageManager.GetStringAsync("String_ComplexFormTargetFile", strLanguage, token: token);

                case "Self":
                    return LanguageManager.GetStringAsync("String_SpellRangeSelf", strLanguage, token: token);

                case "Sprite":
                    return LanguageManager.GetStringAsync("String_ComplexFormTargetSprite", strLanguage, token: token);

                case "Host":
                    return LanguageManager.GetStringAsync("String_ComplexFormTargetHost", strLanguage, token: token);

                case "IC":
                    return LanguageManager.GetStringAsync("String_ComplexFormTargetIC", strLanguage, token: token);

                case "Icon":
                    return LanguageManager.GetStringAsync("String_ComplexFormTargetIcon", strLanguage, token: token);

                case "Special":
                    return LanguageManager.GetStringAsync("String_Special", strLanguage, token: token);

                default:
                    return LanguageManager.GetStringAsync("String_None", strLanguage, token: token);
            }
        }

        /// <summary>
        /// The Complex Form's Target.
        /// </summary>
        public string Target
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strTarget;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strTarget = value;
            }
        }

        /// <summary>
        /// Active Skill that should be used with this Complex Form instead of the default one.
        /// </summary>
        public string UseSkill
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strUseSkill;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strUseSkill = value;
            }
        }

        /// <summary>
        /// Complex Form's Source.
        /// </summary>
        public string Source
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strSource;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strSource = value;
            }
        }

        /// <summary>
        /// Sourcebook Page Number.
        /// </summary>
        public string Page
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strPage;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strPage = value;
            }
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <returns></returns>
        public string DisplayPage(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            using (LockObject.EnterReadLock())
            {
                string s = this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? Page;
                return !string.IsNullOrWhiteSpace(s) ? s : Page;
            }
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public async Task<string> DisplayPageAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                string strReturn = objNode?.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value ?? Page;
                return !string.IsNullOrWhiteSpace(strReturn) ? strReturn : Page;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strNotes;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_strNotes == value)
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                    _strNotes = value;
            }
        }

        public async Task<string> GetNotesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strNotes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetNotesAsync(string value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strNotes == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _strNotes = value;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _colNotes;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_colNotes == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_colNotes == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _colNotes = value;
                    }
                }
            }
        }

        public async Task<Color> GetNotesColorAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _colNotes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetNotesColorAsync(Color value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (value == _colNotes)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_colNotes == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _colNotes = value;
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

        public Skill Skill
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    string strSkillKey = UseSkill;
                    if (string.IsNullOrEmpty(strSkillKey))
                        strSkillKey = "Software";
                    return _objCharacter.SkillsSection.GetActiveSkill(strSkillKey);
                }
            }
        }

        public async Task<Skill> GetSkillAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strSkillKey = UseSkill;
                if (string.IsNullOrEmpty(strSkillKey))
                    strSkillKey = "Software";
                return await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetActiveSkillAsync(strSkillKey, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The Dice Pool size for the Active Skill required to thread the Complex Form.
        /// </summary>
        public int DicePool
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    Skill objSkill = Skill;
                    int intReturn = objSkill != null
                        ? objSkill.PoolOtherAttribute("RES") + objSkill.GetSpecializationBonus(CurrentDisplayName)
                        : 0;
                    // Include any Improvements to Threading.
                    intReturn += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ActionDicePool, false, "Threading").StandardRound();
                    return intReturn;
                }
            }
        }

        /// <summary>
        /// The Dice Pool size for the Active Skill required to cast the Spell.
        /// </summary>
        public async Task<int> GetDicePoolAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Skill objSkill = await GetSkillAsync(token).ConfigureAwait(false);
                int intReturn = objSkill != null
                        ? await objSkill.PoolOtherAttributeAsync("RES", token: token).ConfigureAwait(false)
                            + await objSkill.GetSpecializationBonusAsync(await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), token).ConfigureAwait(false)
                        : 0;
                // Include any Improvements to Threading.
                intReturn += (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.ActionDicePool, false, "Threading", token: token).ConfigureAwait(false)).StandardRound();
                return intReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Tooltip information for the Dice Pool.
        /// </summary>
        public string DicePoolTooltip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                using (LockObject.EnterReadLock())
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdReturn))
                    {
                        string strFormat = strSpace + "{0}" + strSpace + "({1})";
                        CharacterAttrib objResonanceAttrib = _objCharacter.GetAttribute("RES");
                        if (objResonanceAttrib != null)
                        {
                            sbdReturn.AppendFormat(GlobalSettings.CultureInfo, strFormat,
                                                   objResonanceAttrib.DisplayNameFormatted,
                                                   objResonanceAttrib.DisplayValue);
                        }

                        Skill objSkill = Skill;
                        if (objSkill != null)
                        {
                            if (sbdReturn.Length > 0)
                                sbdReturn.Append(strSpace).Append('+').Append(strSpace);
                            sbdReturn.Append(objSkill.FormattedDicePool(objSkill.PoolOtherAttribute("RES") -
                                                                        (objResonanceAttrib?.TotalValue ?? 0),
                                                                        CurrentDisplayName));
                        }

                        // Include any Improvements to the Spell Category.
                        foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                                     _objCharacter, Improvement.ImprovementType.ActionDicePool, "Threading"))
                        {
                            if (sbdReturn.Length > 0)
                                sbdReturn.Append(strSpace).Append('+').Append(strSpace);
                            sbdReturn.AppendFormat(GlobalSettings.CultureInfo, strFormat,
                                                   _objCharacter.GetObjectName(objImprovement), objImprovement.Value);
                        }

                        return sbdReturn.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Tooltip information for the Dice Pool.
        /// </summary>
        public async Task<string> GetDicePoolTooltipAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdReturn))
                {
                    string strFormat = strSpace + "{0}" + strSpace + "({1})";
                    CharacterAttrib objResonanceAttrib = await _objCharacter.GetAttributeAsync("RES", token: token).ConfigureAwait(false);
                    if (objResonanceAttrib != null)
                    {
                        sbdReturn.AppendFormat(GlobalSettings.CultureInfo, strFormat,
                                               await objResonanceAttrib.GetDisplayNameFormattedAsync(token).ConfigureAwait(false),
                                               await objResonanceAttrib.GetDisplayValueAsync(token).ConfigureAwait(false));
                    }

                    Skill objSkill = await GetSkillAsync(token).ConfigureAwait(false);
                    if (objSkill != null)
                    {
                        if (sbdReturn.Length > 0)
                            sbdReturn.Append(strSpace).Append('+').Append(strSpace);
                        sbdReturn.Append(await objSkill.FormattedDicePoolAsync(
                            await objSkill.PoolOtherAttributeAsync("RES", token: token).ConfigureAwait(false)
                            - (objResonanceAttrib != null ? await objResonanceAttrib.GetTotalValueAsync(token).ConfigureAwait(false) : 0),
                            await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), token).ConfigureAwait(false));
                    }

                    // Include any Improvements to the Spell Category.
                    foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                 _objCharacter, Improvement.ImprovementType.ActionDicePool, "Threading", token: token).ConfigureAwait(false))
                    {
                        if (sbdReturn.Length > 0)
                            sbdReturn.Append(strSpace).Append('+').Append(strSpace);
                        sbdReturn.AppendFormat(GlobalSettings.CultureInfo, strFormat,
                                               await _objCharacter.GetObjectNameAsync(objImprovement, token: token).ConfigureAwait(false), objImprovement.Value);
                    }

                    return sbdReturn.ToString();
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                objLocker = LockObject.EnterReadLock(token);
            else
                objLockerAsync = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XmlNode objReturn = _objCachedMyXmlNode;
                if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                XmlNode objDoc = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadData("complexforms.xml", strLanguage, token: token)
                    : await _objCharacter.LoadDataAsync("complexforms.xml", strLanguage, token: token).ConfigureAwait(false);
                if (SourceID != Guid.Empty)
                    objReturn = objDoc.TryGetNodeById("/chummer/complexforms/complexform", SourceID);
                if (objReturn == null)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId("/chummer/complexforms/complexform", Name);
                    objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }
                _objCachedMyXmlNode = objReturn;
                _strCachedXmlNodeLanguage = strLanguage;
                return objReturn;
            }
            finally
            {
                objLocker?.Dispose();
                if (objLockerAsync != null)
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                objLocker = LockObject.EnterReadLock(token);
            else
                objLockerAsync = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objReturn = _objCachedMyXPathNode;
                if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                XPathNavigator objDoc = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadDataXPath("complexforms.xml", strLanguage, token: token)
                    : await _objCharacter.LoadDataXPathAsync("complexforms.xml", strLanguage, token: token).ConfigureAwait(false);
                if (SourceID != Guid.Empty)
                    objReturn = objDoc.TryGetNodeById("/chummer/complexforms/complexform", SourceID);
                if (objReturn == null)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId("/chummer/complexforms/complexform", Name);
                    objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }
                _objCachedMyXPathNode = objReturn;
                _strCachedXPathNodeLanguage = strLanguage;
                return objReturn;
            }
            finally
            {
                objLocker?.Dispose();
                if (objLockerAsync != null)
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Properties

        #region UI Methods

        public async Task<TreeNode> CreateTreeNode(ContextMenuStrip cmsComplexForm, bool blnForInitiationsTab = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if ((blnForInitiationsTab ? Grade < 0 : Grade != 0) && !string.IsNullOrEmpty(Source) && !await _objCharacter.Settings.BookEnabledAsync(Source, token).ConfigureAwait(false))
                    return null;

                TreeNode objNode = new TreeNode
                {
                    Name = InternalId,
                    Text = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                    Tag = this,
                    ContextMenuStrip = cmsComplexForm,
                    ForeColor = blnForInitiationsTab
                        ? await GetPreferredColorForInitiationsTabAsync(token).ConfigureAwait(false)
                        : await GetPreferredColorAsync(token).ConfigureAwait(false),
                    ToolTipText = (await GetNotesAsync(token).ConfigureAwait(false)).WordWrap()
                };
                return objNode;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public Color PreferredColor
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (!string.IsNullOrEmpty(Notes))
                    {
                        return Grade != 0
                            ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                            : ColorManager.GenerateCurrentModeColor(NotesColor);
                    }
                    return Grade != 0
                        ? ColorManager.GrayText
                        : ColorManager.WindowText;
                }
            }
        }

        public async Task<Color> GetPreferredColorAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false)))
                {
                    return Grade != 0
                        ? ColorManager.GenerateCurrentModeDimmedColor(await GetNotesColorAsync(token).ConfigureAwait(false))
                        : ColorManager.GenerateCurrentModeColor(await GetNotesColorAsync(token).ConfigureAwait(false));
                }
                return Grade != 0
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public Color PreferredColorForInitiationsTab
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (!string.IsNullOrEmpty(Notes))
                    {
                        return Grade < 0
                            ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                            : ColorManager.GenerateCurrentModeColor(NotesColor);
                    }
                    return Grade < 0
                        ? ColorManager.GrayText
                        : ColorManager.WindowText;
                }
            }
        }

        public async Task<Color> GetPreferredColorForInitiationsTabAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false)))
                {
                    return Grade < 0
                        ? ColorManager.GenerateCurrentModeDimmedColor(await GetNotesColorAsync(token).ConfigureAwait(false))
                        : ColorManager.GenerateCurrentModeColor(await GetNotesColorAsync(token).ConfigureAwait(false));
                }
                return Grade < 0
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion UI Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (Grade < 0)
                    return false;
                if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteComplexForm")))
                {
                    return false;
                }

                using (LockObject.EnterWriteLock())
                {
                    _objCharacter.ComplexForms.Remove(this);
                    ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.ComplexForm,
                        InternalId);
                }
            }
            Dispose();
            return true;
        }

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Grade < 0)
                    return false;
                if (blnConfirmDelete && !await CommonFunctions
                            .ConfirmDeleteAsync(
                                await LanguageManager.GetStringAsync("Message_DeleteComplexForm", token: token)
                                    .ConfigureAwait(false), token).ConfigureAwait(false))
                {
                    return false;
                }

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    await _objCharacter.ComplexForms.RemoveAsync(this, token).ConfigureAwait(false);
                    await ImprovementManager
                        .RemoveImprovementsAsync(_objCharacter, Improvement.ImprovementSource.ComplexForm, InternalId, token)
                        .ConfigureAwait(false);
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

            await DisposeAsync().ConfigureAwait(false);
            return true;
        }

        public void SetSourceDetail(Control sourceControl)
        {
            using (LockObject.EnterReadLock())
            {
                if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                    _objCachedSourceDetail = default;
                SourceDetail.SetControl(sourceControl);
            }
        }

        public async Task SetSourceDetailAsync(Control sourceControl, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                    _objCachedSourceDetail = default;
                await (await GetSourceDetailAsync(token).ConfigureAwait(false)).SetControlAsync(sourceControl, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            IDisposable objDummy = LockObject.EnterWriteLock();
            objDummy.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            await objLocker.DisposeAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }
    }
}
