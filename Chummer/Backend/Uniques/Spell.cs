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
using System.Globalization;
using System.Linq;
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
    /// A Magician Spell.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayName(GlobalSettings.DefaultLanguage)}")]
    public sealed class Spell : IHasInternalId, IHasName, IHasSourceId, IHasXmlDataNode, IHasNotes, ICanRemove, IHasSource, IHasLockObject, IHasCharacterObject
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guiID;
        private Guid _guiSourceID = Guid.Empty;
        private string _strName = string.Empty;
        private string _strDescriptors = string.Empty;
        private string _strCategory = string.Empty;
        private string _strType = string.Empty;
        private string _strRange = string.Empty;
        private string _strDamage = string.Empty;
        private string _strDuration = string.Empty;
        private string _strDV = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strExtra = string.Empty;
        private string _strUseSkill = string.Empty;
        private bool _blnLimited;
        private bool _blnExtended;
        private bool _blnCustomExtended;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private readonly Character _objCharacter;
        private bool _blnAlchemical;
        private bool _blnFreeBonus;
        private bool _blnBarehandedAdept;
        private int _intGrade;

        private Improvement.ImprovementSource _eImprovementSource = Improvement.ImprovementSource.Spell;

        public Character CharacterObject => _objCharacter; // readonly member, no locking needed

        #region Constructor, Create, Save, Load, and Print Methods

        public Spell(Character objCharacter)
        {
            // Create the GUID for the new Spell.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = objCharacter.LockObject;
        }

        /// <summary>
        /// Create a Spell from an XmlNode.
        /// </summary>
        /// <param name="objXmlSpellNode">XmlNode to create the object from.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        /// <param name="blnLimited">Whether the Spell should be marked as Limited.</param>
        /// <param name="blnExtended">Whether the Spell should be marked as Extended.</param>
        /// <param name="blnAlchemical">Whether the Spell is one for an alchemical preparation.</param>
        /// <param name="eSource">Enum representing the actual type of spell this object represents. Used for initiation benefits that would grant spells.</param>
        public void Create(XmlNode objXmlSpellNode, string strForcedValue = "", bool blnLimited = false, bool blnExtended = false, bool blnAlchemical = false, Improvement.ImprovementSource eSource = Improvement.ImprovementSource.Spell)
        {
            using (LockObject.EnterWriteLock())
            {
                if (!objXmlSpellNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
                {
                    Log.Warn(new object[] {"Missing id field for xmlnode", objXmlSpellNode});
                    Utils.BreakIfDebug();
                }

                objXmlSpellNode.TryGetStringFieldQuickly("name", ref _strName);

                ImprovementManager.SetForcedValue(strForcedValue, _objCharacter);
                if (objXmlSpellNode["bonus"] != null)
                {
                    if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Spell,
                                                               _guiID.ToString(
                                                                   "D", GlobalSettings.InvariantCultureInfo),
                                                               objXmlSpellNode["bonus"], 1, CurrentDisplayNameShort))
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

                if (!objXmlSpellNode.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                    objXmlSpellNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objXmlSpellNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
                {
                    Notes = CommonFunctions.GetBookNotes(objXmlSpellNode, Name, CurrentDisplayName, Source, Page,
                                                         DisplayPage(GlobalSettings.Language), _objCharacter);
                }

                if (objXmlSpellNode.TryGetStringFieldQuickly("descriptor", ref _strDescriptors))
                    UpdateHashDescriptors();
                objXmlSpellNode.TryGetStringFieldQuickly("category", ref _strCategory);
                objXmlSpellNode.TryGetStringFieldQuickly("type", ref _strType);
                objXmlSpellNode.TryGetStringFieldQuickly("range", ref _strRange);
                objXmlSpellNode.TryGetStringFieldQuickly("damage", ref _strDamage);
                objXmlSpellNode.TryGetStringFieldQuickly("duration", ref _strDuration);
                objXmlSpellNode.TryGetStringFieldQuickly("dv", ref _strDV);
                objXmlSpellNode.TryGetStringFieldQuickly("useskill", ref _strUseSkill);
                _blnLimited = blnLimited;
                _blnAlchemical = blnAlchemical;
                objXmlSpellNode.TryGetStringFieldQuickly("source", ref _strSource);
                objXmlSpellNode.TryGetStringFieldQuickly("page", ref _strPage);
                _eImprovementSource = eSource;

                _blnExtended = blnExtended;
                if (blnExtended)
                {
                    _blnCustomExtended = !HashDescriptors.Any(x =>
                                                                  string.Equals(
                                                                      x.Trim(), "Extended Area",
                                                                      StringComparison.OrdinalIgnoreCase));
                }

                /*
                if (string.IsNullOrEmpty(_strNotes))
                {
                    _strNotes = CommonFunctions.GetText(_strSource + ' ' + _strPage, Name);
                }
                */
            }
        }

        /// <summary>
        /// Create a Spell from an XmlNode.
        /// </summary>
        /// <param name="objXmlSpellNode">XmlNode to create the object from.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        /// <param name="blnLimited">Whether the Spell should be marked as Limited.</param>
        /// <param name="blnExtended">Whether the Spell should be marked as Extended.</param>
        /// <param name="blnAlchemical">Whether the Spell is one for an alchemical preparation.</param>
        /// <param name="eSource">Enum representing the actual type of spell this object represents. Used for initiation benefits that would grant spells.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task CreateAsync(XmlNode objXmlSpellNode, string strForcedValue = "", bool blnLimited = false, bool blnExtended = false, bool blnAlchemical = false, Improvement.ImprovementSource eSource = Improvement.ImprovementSource.Spell, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!objXmlSpellNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
                {
                    Log.Warn(new object[] { "Missing id field for xmlnode", objXmlSpellNode });
                    Utils.BreakIfDebug();
                }

                objXmlSpellNode.TryGetStringFieldQuickly("name", ref _strName);

                ImprovementManager.SetForcedValue(strForcedValue, _objCharacter);
                if (objXmlSpellNode["bonus"] != null)
                {
                    if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter,
                                Improvement.ImprovementSource.Spell,
                                _guiID.ToString(
                                    "D", GlobalSettings.InvariantCultureInfo),
                                objXmlSpellNode["bonus"], 1,
                                await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token: token)
                            .ConfigureAwait(false))
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

                if (!objXmlSpellNode.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                    objXmlSpellNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objXmlSpellNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
                {
                    Notes = await CommonFunctions.GetBookNotesAsync(objXmlSpellNode, Name,
                        await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), Source, Page,
                        await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false), _objCharacter,
                        token).ConfigureAwait(false);
                }

                if (objXmlSpellNode.TryGetStringFieldQuickly("descriptor", ref _strDescriptors))
                    await UpdateHashDescriptorsAsync(token).ConfigureAwait(false);
                objXmlSpellNode.TryGetStringFieldQuickly("category", ref _strCategory);
                objXmlSpellNode.TryGetStringFieldQuickly("type", ref _strType);
                objXmlSpellNode.TryGetStringFieldQuickly("range", ref _strRange);
                objXmlSpellNode.TryGetStringFieldQuickly("damage", ref _strDamage);
                objXmlSpellNode.TryGetStringFieldQuickly("duration", ref _strDuration);
                objXmlSpellNode.TryGetStringFieldQuickly("dv", ref _strDV);
                objXmlSpellNode.TryGetStringFieldQuickly("useskill", ref _strUseSkill);
                _blnLimited = blnLimited;
                _blnAlchemical = blnAlchemical;
                objXmlSpellNode.TryGetStringFieldQuickly("source", ref _strSource);
                objXmlSpellNode.TryGetStringFieldQuickly("page", ref _strPage);
                _eImprovementSource = eSource;

                _blnExtended = blnExtended;
                if (blnExtended)
                {
                    _blnCustomExtended = !HashDescriptors.Any(x =>
                        string.Equals(
                            x.Trim(), "Extended Area",
                            StringComparison.OrdinalIgnoreCase));
                }

                /*
                if (string.IsNullOrEmpty(_strNotes))
                {
                    _strNotes = CommonFunctions.GetText(_strSource + ' ' + _strPage, Name);
                }
                */
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private SourceString _objCachedSourceDetail;

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
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            using (LockObject.EnterReadLock())
            {
                objWriter.WriteStartElement("spell");
                objWriter.WriteElementString("sourceid", SourceIDString);
                objWriter.WriteElementString("guid", InternalId);
                objWriter.WriteElementString("name", _strName);
                objWriter.WriteElementString("descriptors", _strDescriptors);
                objWriter.WriteElementString("category", _strCategory);
                objWriter.WriteElementString("type", _strType);
                objWriter.WriteElementString("range", _strRange);
                objWriter.WriteElementString("damage", _strDamage);
                objWriter.WriteElementString("duration", _strDuration);
                objWriter.WriteElementString("dv", _strDV);
                objWriter.WriteElementString("useskill", _strUseSkill);
                objWriter.WriteElementString("limited", _blnLimited.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("extended", _blnExtended.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("customextended",
                                             _blnCustomExtended.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("alchemical",
                                             _blnAlchemical.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("source", _strSource);
                objWriter.WriteElementString("page", _strPage);
                objWriter.WriteElementString("extra", _strExtra);
                objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
                objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
                objWriter.WriteElementString("freebonus", _blnFreeBonus.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("barehandedadept",
                                             _blnBarehandedAdept.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("improvementsource", _eImprovementSource.ToString());
                objWriter.WriteElementString("grade", _intGrade.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Load the Spell from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            using (LockObject.EnterWriteLock())
            {
                if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                {
                    _guiID = Guid.NewGuid();
                }

                objNode.TryGetStringFieldQuickly("name", ref _strName);
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
                Lazy<XPathNavigator> objMyNode = new Lazy<XPathNavigator>(() => this.GetNodeXPath());
                if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
                {
                    objMyNode.Value?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }

                if (objNode.TryGetStringFieldQuickly("descriptors", ref _strDescriptors))
                    UpdateHashDescriptors();
                objNode.TryGetStringFieldQuickly("category", ref _strCategory);
                objNode.TryGetStringFieldQuickly("type", ref _strType);
                objNode.TryGetStringFieldQuickly("range", ref _strRange);
                objNode.TryGetStringFieldQuickly("damage", ref _strDamage);
                objNode.TryGetStringFieldQuickly("duration", ref _strDuration);
                if (objNode["improvementsource"] != null)
                {
                    _eImprovementSource
                        = Improvement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);
                }

                objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
                objNode.TryGetStringFieldQuickly("dv", ref _strDV);
                objNode.TryGetStringFieldQuickly("useskill", ref _strUseSkill);
                if (objNode.TryGetBoolFieldQuickly("limited", ref _blnLimited) && _blnLimited
                                                                               && _objCharacter.LastSavedVersion
                                                                               <= new ValueVersion(5, 197, 30))
                {
                    objMyNode.Value?.TryGetStringFieldQuickly("dv", ref _strDV);
                }

                objNode.TryGetBoolFieldQuickly("extended", ref _blnExtended);
                if (_blnExtended)
                {
                    if (!objNode.TryGetBoolFieldQuickly("customextended", ref _blnCustomExtended))
                    {
                        _blnCustomExtended = !HashDescriptors.Any(x =>
                                                                      string.Equals(
                                                                          x.Trim(), "Extended Area",
                                                                          StringComparison.OrdinalIgnoreCase));
                    }
                }
                else
                    _blnCustomExtended = false;

                objNode.TryGetBoolFieldQuickly("freebonus", ref _blnFreeBonus);
                if (!objNode.TryGetBoolFieldQuickly("barehandedadept", ref _blnBarehandedAdept))
                    objNode.TryGetBoolFieldQuickly("usesunarmed", ref _blnBarehandedAdept);
                objNode.TryGetBoolFieldQuickly("alchemical", ref _blnAlchemical);
                objNode.TryGetStringFieldQuickly("source", ref _strSource);
                objNode.TryGetStringFieldQuickly("page", ref _strPage);

                objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
                objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print numbers.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // <spell>
                XmlElementWriteHelper objBaseElement
                    = await objWriter.StartElementAsync("spell", token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false),
                            token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "fullname", await DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false),
                            token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "fullname_english", await DisplayNameAsync(GlobalSettings.Language, token).ConfigureAwait(false),
                            token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("descriptors",
                            await DisplayDescriptorsAsync(strLanguageToPrint, token)
                                .ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("descriptors_english", Descriptors, token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "category", await DisplayCategoryAsync(strLanguageToPrint, token).ConfigureAwait(false),
                            token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("category_english", Category, token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "type", await DisplayTypeAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("type_english", Type, token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "range", await DisplayRangeAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("range_english", Range, token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "damage", await DisplayDamageAsync(strLanguageToPrint, objCulture, token).ConfigureAwait(false),
                            token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("damage_english", Damage, token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "duration", await DisplayDurationAsync(strLanguageToPrint, token).ConfigureAwait(false),
                            token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("duration_english", Duration, token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "dv", await DisplayDvAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("dv_english", await GetCalculatedDvAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("alchemy", Alchemical.ToString(GlobalSettings.InvariantCultureInfo),
                            token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("limited", Limited.ToString(GlobalSettings.InvariantCultureInfo),
                            token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("barehandedadept",
                            BarehandedAdept.ToString(GlobalSettings.InvariantCultureInfo), token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("dicepool", (await GetDicePoolAsync(token).ConfigureAwait(false)).ToString(objCulture), token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "source",
                            await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token)
                                .ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "extra",
                            await _objCharacter.TranslateExtraAsync(Extra, strLanguageToPrint, token: token)
                                .ConfigureAwait(false), token).ConfigureAwait(false);
                    if (GlobalSettings.PrintNotes)
                        await objWriter.WriteElementStringAsync("notes", Notes, token).ConfigureAwait(false);
                }
                finally
                {
                    // </spell>
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
        /// Internal identifier which will be used to identify this Spell in the Improvement system.
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
        /// Spell's name.
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

        /// <summary>
        /// Spell's grade.
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
        /// Spell's descriptors.
        /// </summary>
        public string Descriptors
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strDescriptors;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strDescriptors, value) == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        UpdateHashDescriptors();
                        if (Extended)
                        {
                            _blnCustomExtended = !HashDescriptors.Any(x =>
                                                                          string.Equals(
                                                                              x.Trim(), "Extended Area",
                                                                              StringComparison.OrdinalIgnoreCase));
                        }
                    }
                }
            }
        }

        private void UpdateHashDescriptors()
        {
            using (LockObject.EnterWriteLock())
            {
                HashDescriptors.Clear();
                foreach (string strDescriptor in Descriptors.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                    HashDescriptors.Add(strDescriptor);
            }
        }

        private async Task UpdateHashDescriptorsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                HashDescriptors.Clear();
                foreach (string strDescriptor in Descriptors.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                    HashDescriptors.Add(strDescriptor);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public HashSet<string> HashDescriptors
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _setDescriptors;
            }
        }

        /// <summary>
        /// Translated Descriptors.
        /// </summary>
        public string DisplayDescriptors(string strLanguage = "")
        {
            using (LockObject.EnterReadLock())
            {
                if (string.IsNullOrWhiteSpace(Descriptors))
                    return LanguageManager.GetString("String_None", strLanguage);
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdReturn))
                {
                    string strSpace = LanguageManager.GetString("String_Space", strLanguage);
                    if (HashDescriptors.Count > 0)
                    {
                        foreach (string strDescriptor in HashDescriptors)
                        {
                            switch (strDescriptor.Trim())
                            {
                                case "Alchemical Preparation":
                                    sbdReturn.Append(
                                        LanguageManager.GetString("String_DescAlchemicalPreparation", strLanguage));
                                    break;

                                case "Extended Area":
                                    sbdReturn.Append(LanguageManager.GetString("String_DescExtendedArea", strLanguage));
                                    break;

                                case "Material Link":
                                    sbdReturn.Append(LanguageManager.GetString("String_DescMaterialLink", strLanguage));
                                    break;

                                case "Multi-Sense":
                                    sbdReturn.Append(LanguageManager.GetString("String_DescMultiSense", strLanguage));
                                    break;

                                case "Organic Link":
                                    sbdReturn.Append(LanguageManager.GetString("String_DescOrganicLink", strLanguage));
                                    break;

                                case "Single-Sense":
                                    sbdReturn.Append(LanguageManager.GetString("String_DescSingleSense", strLanguage));
                                    break;

                                default:
                                    sbdReturn.Append(LanguageManager.GetString("String_Desc" + strDescriptor.Trim(),
                                                                               strLanguage));
                                    break;
                            }

                            sbdReturn.Append(',').Append(strSpace);
                        }
                    }

                    // If Extended Area was not found and the Extended flag is enabled, add Extended Area to the list of Descriptors.
                    if (Extended && _blnCustomExtended)
                        sbdReturn.Append(LanguageManager.GetString("String_DescExtendedArea", strLanguage)).Append(',')
                                 .Append(strSpace);

                    // Remove the trailing comma.
                    if (sbdReturn.Length >= strSpace.Length + 1)
                        sbdReturn.Length -= strSpace.Length + 1;

                    return sbdReturn.ToString();
                }
            }
        }

        /// <summary>
        /// Translated Descriptors.
        /// </summary>
        public async Task<string> DisplayDescriptorsAsync(string strLanguage = "", CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(Descriptors))
                    return await LanguageManager.GetStringAsync("String_None", strLanguage, token: token).ConfigureAwait(false);
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdReturn))
                {
                    string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                                                           .ConfigureAwait(false);
                    if (HashDescriptors.Count > 0)
                    {
                        foreach (string strDescriptor in HashDescriptors)
                        {
                            switch (strDescriptor.Trim())
                            {
                                case "Alchemical Preparation":
                                    sbdReturn.Append(
                                        await LanguageManager
                                              .GetStringAsync("String_DescAlchemicalPreparation", strLanguage,
                                                              token: token).ConfigureAwait(false));
                                    break;

                                case "Extended Area":
                                    sbdReturn.Append(await LanguageManager
                                                           .GetStringAsync(
                                                               "String_DescExtendedArea", strLanguage, token: token)
                                                           .ConfigureAwait(false));
                                    break;

                                case "Material Link":
                                    sbdReturn.Append(await LanguageManager
                                                           .GetStringAsync(
                                                               "String_DescMaterialLink", strLanguage, token: token)
                                                           .ConfigureAwait(false));
                                    break;

                                case "Multi-Sense":
                                    sbdReturn.Append(await LanguageManager
                                                           .GetStringAsync(
                                                               "String_DescMultiSense", strLanguage, token: token)
                                                           .ConfigureAwait(false));
                                    break;

                                case "Organic Link":
                                    sbdReturn.Append(await LanguageManager
                                                           .GetStringAsync(
                                                               "String_DescOrganicLink", strLanguage, token: token)
                                                           .ConfigureAwait(false));
                                    break;

                                case "Single-Sense":
                                    sbdReturn.Append(await LanguageManager
                                                           .GetStringAsync(
                                                               "String_DescSingleSense", strLanguage, token: token)
                                                           .ConfigureAwait(false));
                                    break;

                                default:
                                    sbdReturn.Append(await LanguageManager.GetStringAsync(
                                                         "String_Desc" + strDescriptor.Trim(),
                                                         strLanguage, token: token).ConfigureAwait(false));
                                    break;
                            }

                            sbdReturn.Append(',').Append(strSpace);
                        }
                    }

                    // If Extended Area was not found and the Extended flag is enabled, add Extended Area to the list of Descriptors.
                    if (Extended && _blnCustomExtended)
                        sbdReturn.Append(await LanguageManager
                                               .GetStringAsync("String_DescExtendedArea", strLanguage, token: token)
                                               .ConfigureAwait(false)).Append(',')
                                 .Append(strSpace);

                    // Remove the trailing comma.
                    if (sbdReturn.Length >= strSpace.Length + 1)
                        sbdReturn.Length -= strSpace.Length + 1;

                    return sbdReturn.ToString();
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            using (LockObject.EnterReadLock())
            {
                return _objCharacter.LoadDataXPath("spells.xml", strLanguage)
                                    .SelectSingleNodeAndCacheExpression("/chummer/categories/category[. = "
                                                                        + Category.CleanXPath()
                                                                        + "]/@translate")?.Value ?? Category;
            }
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public async Task<string> DisplayCategoryAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return (await _objCharacter.LoadDataXPathAsync("spells.xml", strLanguage, token: token)
                           .ConfigureAwait(false))
                       .SelectSingleNodeAndCacheExpression(
                           "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate",
                           token: token)
                       ?.Value
                       ?? Category;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Spell's category.
        /// </summary>
        public string Category
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strCategory;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strCategory = value;
            }
        }

        /// <summary>
        /// Spell's type.
        /// </summary>
        public string Type
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strType;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strType = value;
            }
        }

        /// <summary>
        /// Translated Type.
        /// </summary>
        public string DisplayType(string strLanguage)
        {
            switch (Type)
            {
                case "M":
                    return LanguageManager.GetString("String_SpellTypeMana", strLanguage);

                default:
                    return LanguageManager.GetString("String_SpellTypePhysical", strLanguage);
            }
        }

        /// <summary>
        /// Translated Type.
        /// </summary>
        public Task<string> DisplayTypeAsync(string strLanguage, CancellationToken token = default)
        {
            switch (Type)
            {
                case "M":
                    return LanguageManager.GetStringAsync("String_SpellTypeMana", strLanguage, token: token);

                default:
                    return LanguageManager.GetStringAsync("String_SpellTypePhysical", strLanguage, token: token);
            }
        }

        /// <summary>
        /// Translated Drain Value.
        /// </summary>
        public string DisplayDv(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn = CalculatedDv.Replace('/', '÷').Replace('*', '×');
                if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    strReturn = strReturn
                                .CheapReplace("F", () => LanguageManager.GetString("String_SpellForce", strLanguage))
                                .CheapReplace("Overflow damage",
                                              () => LanguageManager.GetString(
                                                  "String_SpellOverflowDamage", strLanguage))
                                .CheapReplace("Damage Value",
                                              () => LanguageManager.GetString("String_SpellDamageValue", strLanguage))
                                .CheapReplace(
                                    "Toxin DV", () => LanguageManager.GetString("String_SpellToxinDV", strLanguage))
                                .CheapReplace("Disease DV",
                                              () => LanguageManager.GetString("String_SpellDiseaseDV", strLanguage))
                                .CheapReplace("Radiation Power",
                                              () => LanguageManager.GetString(
                                                  "String_SpellRadiationPower", strLanguage))
                                .CheapReplace(
                                    "Special", () => LanguageManager.GetString("String_Special", strLanguage));
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Translated Drain Value.
        /// </summary>
        public async Task<string> DisplayDvAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn = (await GetCalculatedDvAsync(token).ConfigureAwait(false)).Replace('/', '÷').Replace('*', '×');
                if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    strReturn = await strReturn
                                      .CheapReplaceAsync(
                                          "F",
                                          () => LanguageManager.GetStringAsync(
                                              "String_SpellForce", strLanguage, token: token), token: token)
                                      .CheapReplaceAsync("Overflow damage",
                                                         () => LanguageManager.GetStringAsync(
                                                             "String_SpellOverflowDamage", strLanguage, token: token),
                                                         token: token)
                                      .CheapReplaceAsync("Damage Value",
                                                         () => LanguageManager.GetStringAsync(
                                                             "String_SpellDamageValue", strLanguage, token: token),
                                                         token: token)
                                      .CheapReplaceAsync(
                                          "Toxin DV",
                                          () => LanguageManager.GetStringAsync(
                                              "String_SpellToxinDV", strLanguage, token: token), token: token)
                                      .CheapReplaceAsync("Disease DV",
                                                         () => LanguageManager.GetStringAsync(
                                                             "String_SpellDiseaseDV", strLanguage, token: token),
                                                         token: token)
                                      .CheapReplaceAsync("Radiation Power",
                                                         () => LanguageManager.GetStringAsync(
                                                             "String_SpellRadiationPower", strLanguage, token: token),
                                                         token: token)
                                      .CheapReplaceAsync(
                                          "Special",
                                          () => LanguageManager.GetStringAsync(
                                              "String_Special", strLanguage, token: token), token: token)
                                      .ConfigureAwait(false);
                }

                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Drain Tooltip.
        /// </summary>
        public async Task<string> GetDvTooltipAsync(CancellationToken token = default)
        {
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token);
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intMag = await (await _objCharacter.GetAttributeAsync("MAG", token: token).ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false);
                // Barehanded Adept is limited to a Force of MAG/3 rounded up
                int intHighestForce = BarehandedAdept
                    ? (intMag + 2) / 3
                    : intMag * 2;
                string strDV = await GetCalculatedDvAsync(token).ConfigureAwait(false);
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdTip))
                {
                    sbdTip.Append(await LanguageManager.GetStringAsync("Tip_SpellDrain", token: token).ConfigureAwait(false));
                    for (int i = 1; i <= intHighestForce; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        // Calculate the Spell's Drain for the current Force.
                        (bool blnIsSuccess, object xprResult) = await CommonFunctions.EvaluateInvariantXPathAsync(
                            strDV.Replace("F", i.ToString(GlobalSettings.InvariantCultureInfo)), token: token).ConfigureAwait(false);

                        if (blnIsSuccess && strDV != "Special")
                        {
                            // Drain is always minimum 2 (doubled for Barehanded Adept)
                            int intDV = Math.Max(((double)xprResult).StandardRound(), BarehandedAdept ? 4 : 2);
                            sbdTip.AppendLine().Append(await LanguageManager.GetStringAsync("String_Force", token: token).ConfigureAwait(false))
                                  .Append(strSpace).Append(i.ToString(GlobalSettings.CultureInfo))
                                  .Append(await LanguageManager.GetStringAsync("String_Colon", token: token).ConfigureAwait(false)).Append(strSpace)
                                  .Append(intDV.ToString(GlobalSettings.CultureInfo));
                        }
                        else
                        {
                            sbdTip.Clear();
                            sbdTip.Append(await LanguageManager.GetStringAsync("Tip_SpellDrainSeeDescription", token: token).ConfigureAwait(false));
                            break;
                        }
                    }

                    sbdTip.AppendLine();
                    if (BarehandedAdept)
                        sbdTip.Append('(');
                    sbdTip.Append(await LanguageManager.GetStringAsync("Tip_SpellDrainBase", token: token).ConfigureAwait(false)).Append(strSpace).Append('(')
                          .Append(DvBase).Append(')');
                    if (Limited)
                    {
                        sbdTip.Append(strSpace).Append('+').Append(strSpace)
                              .Append(await LanguageManager.GetStringAsync("String_SpellLimited", token: token).ConfigureAwait(false)).Append(strSpace)
                              .Append("(-2)");
                    }

                    if (Extended && _blnCustomExtended)
                    {
                        sbdTip.Append(strSpace).Append('+').Append(strSpace)
                              .Append(await LanguageManager.GetStringAsync("String_SpellExtended", token: token).ConfigureAwait(false)).Append(strSpace)
                              .Append("(+2)");
                    }

                    foreach (Improvement objLoopImprovement in await RelevantImprovementsAsync(o =>
                                 o.ImproveType == Improvement.ImprovementType.DrainValue
                                 || o.ImproveType == Improvement.ImprovementType.SpellCategoryDrain
                                 || o.ImproveType == Improvement.ImprovementType.SpellDescriptorDrain, token: token).ConfigureAwait(false))
                    {
                        sbdTip.Append(strSpace).Append('+').Append(strSpace)
                              .Append(await _objCharacter.GetObjectNameAsync(objLoopImprovement, token: token).ConfigureAwait(false)).Append(strSpace)
                              .Append('(').Append(objLoopImprovement.Value.ToString("0;-0;0", GlobalSettings.CultureInfo)).Append(')');
                    }

                    // Minimum drain of 2
                    sbdTip.AppendLine().AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_MinimumAttribute", token: token).ConfigureAwait(false), 2);

                    if (BarehandedAdept)
                    {
                        Improvement objBarehandedAdeptImprovement = (await ImprovementManager
                                                                    .GetCachedImprovementListForValueOfAsync(
                                                                        _objCharacter,
                                                                        Improvement.ImprovementType.AllowSpellRange, token: token).ConfigureAwait(false))
                                                                    .Find(x => x.ImprovedName == "T"
                                                                              || x.ImprovedName == "T (A)");
                        string strBarehandedAdeptName = objBarehandedAdeptImprovement != null
                            ? await _objCharacter.GetObjectNameAsync(objBarehandedAdeptImprovement, token: token).ConfigureAwait(false)
                            : await _objCharacter.TranslateExtraAsync("Barehanded Adept", GlobalSettings.Language,
                                                           "qualities.xml", token).ConfigureAwait(false);
                        sbdTip.Append(')').Append(strSpace).Append('×').Append(strSpace)
                              .Append(strBarehandedAdeptName).Append(strSpace).Append("(×2)");
                    }

                    return sbdTip.ToString();
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Spell's range.
        /// </summary>
        public string Range
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strRange;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strRange = value;
            }
        }

        /// <summary>
        /// Translated Range.
        /// </summary>
        public string DisplayRange(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn = Range;
                if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    strReturn = strReturn
                                .CheapReplace(
                                    "Self", () => LanguageManager.GetString("String_SpellRangeSelf", strLanguage))
                                .CheapReplace(
                                    "LOS", () => LanguageManager.GetString("String_SpellRangeLineOfSight", strLanguage))
                                .CheapReplace(
                                    "LOI",
                                    () => LanguageManager.GetString("String_SpellRangeLineOfInfluence", strLanguage))
                                .CheapReplace(
                                    "Touch",
                                    () => LanguageManager.GetString("String_SpellRangeTouch",
                                                                    strLanguage)) // Short form to remain export-friendly
                                .CheapReplace(
                                    "T", () => LanguageManager.GetString("String_SpellRangeTouch", strLanguage))
                                .CheapReplace(
                                    "(A)",
                                    () => '(' + LanguageManager.GetString("String_SpellRangeArea", strLanguage) + ')')
                                .CheapReplace(
                                    "MAG", () => LanguageManager.GetString("String_AttributeMAGShort", strLanguage))
                                .CheapReplace(
                                    "Special", () => LanguageManager.GetString("String_Special", strLanguage));
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Translated Range.
        /// </summary>
        public async Task<string> DisplayRangeAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn = Range;
                if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    strReturn = await strReturn
                                      .CheapReplaceAsync(
                                          "Self",
                                          () => LanguageManager.GetStringAsync(
                                              "String_SpellRangeSelf", strLanguage, token: token), token: token)
                                      .CheapReplaceAsync(
                                          "LOS",
                                          () => LanguageManager.GetStringAsync(
                                              "String_SpellRangeLineOfSight", strLanguage, token: token), token: token)
                                      .CheapReplaceAsync(
                                          "LOI",
                                          () => LanguageManager.GetStringAsync(
                                              "String_SpellRangeLineOfInfluence", strLanguage, token: token),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "Touch",
                                          () => LanguageManager.GetStringAsync("String_SpellRangeTouch",
                                                                               strLanguage, token: token),
                                          token: token) // Short form to remain export-friendly
                                      .CheapReplaceAsync(
                                          "T",
                                          () => LanguageManager.GetStringAsync(
                                              "String_SpellRangeTouch", strLanguage, token: token), token: token)
                                      .CheapReplaceAsync(
                                          "(A)",
                                          async () => '(' + await LanguageManager
                                                                  .GetStringAsync(
                                                                      "String_SpellRangeArea", strLanguage,
                                                                      token: token).ConfigureAwait(false) + ')',
                                          token: token)
                                      .CheapReplaceAsync(
                                          "MAG",
                                          () => LanguageManager.GetStringAsync(
                                              "String_AttributeMAGShort", strLanguage, token: token), token: token)
                                      .CheapReplaceAsync(
                                          "Special",
                                          () => LanguageManager.GetStringAsync(
                                              "String_Special", strLanguage, token: token), token: token)
                                      .ConfigureAwait(false);
                }

                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Spell's damage.
        /// </summary>
        public string Damage
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strDamage;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strDamage = value;
            }
        }

        /// <summary>
        /// Translated Damage.
        /// </summary>
        public string DisplayDamage(string strLanguage, CultureInfo objCultureInfo)
        {
            using (LockObject.EnterReadLock())
            {
                if (Damage != "S" && Damage != "P")
                    return LanguageManager.GetString("String_None", strLanguage);
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdReturn))
                {
                    sbdReturn.Append('0');
                    foreach (Improvement improvement in RelevantImprovements(
                                 i => i.ImproveType == Improvement.ImprovementType.SpellDescriptorDamage
                                      || i.ImproveType == Improvement.ImprovementType.SpellCategoryDamage))
                        sbdReturn.AppendFormat(GlobalSettings.InvariantCultureInfo, " + {0:0;-0;0}", improvement.Value);
                    string output = sbdReturn.ToString();
                    if (output.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                    {
                        (bool blnIsSuccess, object xprResult)
                            = CommonFunctions.EvaluateInvariantXPath(output.TrimStart('+'));
                        sbdReturn.Clear();
                        if (blnIsSuccess)
                            sbdReturn.Append(((double)xprResult).ToString("#,0.##", objCultureInfo));
                    }
                    else
                    {
                        sbdReturn.Clear();
                        sbdReturn.Append(decValue.ToString("#,0.##", objCultureInfo));
                    }

                    switch (Damage)
                    {
                        case "P":
                            sbdReturn.Append(LanguageManager.GetString("String_DamagePhysical", strLanguage));
                            break;

                        case "S":
                            sbdReturn.Append(LanguageManager.GetString("String_DamageStun", strLanguage));
                            break;
                    }

                    return sbdReturn.ToString();
                }
            }
        }

        /// <summary>
        /// Translated Damage.
        /// </summary>
        public async Task<string> DisplayDamageAsync(string strLanguage, CultureInfo objCultureInfo, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Damage != "S" && Damage != "P")
                    return await LanguageManager.GetStringAsync("String_None", strLanguage, token: token)
                                                .ConfigureAwait(false);
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdReturn))
                {
                    sbdReturn.Append('0');
                    foreach (Improvement improvement in RelevantImprovements(
                                 i => i.ImproveType == Improvement.ImprovementType.SpellDescriptorDamage
                                      || i.ImproveType == Improvement.ImprovementType.SpellCategoryDamage))
                        sbdReturn.AppendFormat(GlobalSettings.InvariantCultureInfo, " + {0:0;-0;0}", improvement.Value);
                    string output = sbdReturn.ToString();
                    if (output.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                    {
                        (bool blnIsSuccess, object xprResult) = await CommonFunctions
                                                                  .EvaluateInvariantXPathAsync(
                                                                      output.TrimStart('+'), token)
                                                                  .ConfigureAwait(false);
                        sbdReturn.Clear();
                        if (blnIsSuccess)
                            sbdReturn.Append(((double)xprResult).ToString("#,0.##", objCultureInfo));
                    }
                    else
                    {
                        sbdReturn.Clear();
                        sbdReturn.Append(decValue.ToString("#,0.##", objCultureInfo));
                    }
                    switch (Damage)
                    {
                        case "P":
                            sbdReturn.Append(await LanguageManager
                                                   .GetStringAsync("String_DamagePhysical", strLanguage, token: token)
                                                   .ConfigureAwait(false));
                            break;

                        case "S":
                            sbdReturn.Append(await LanguageManager
                                                   .GetStringAsync("String_DamageStun", strLanguage, token: token)
                                                   .ConfigureAwait(false));
                            break;
                    }

                    return sbdReturn.ToString();
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Spell's duration.
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
        /// Spell's drain value.
        /// </summary>
        public string CalculatedDv
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    string strReturn = DvBase;
                    if (!Limited && !(Extended && _blnCustomExtended) && !BarehandedAdept && !RelevantImprovements(o =>
                            o.ImproveType == Improvement.ImprovementType.DrainValue
                            || o.ImproveType == Improvement.ImprovementType.SpellCategoryDrain
                            || o.ImproveType == Improvement.ImprovementType.SpellDescriptorDrain, true).Any())
                        return strReturn;
                    bool blnForce = strReturn.StartsWith('F');
                    string strDv = blnForce ? strReturn.TrimStartOnce("F", true) : strReturn;
                    //Navigator can't do math on a single value, so inject a mathable value.
                    if (string.IsNullOrEmpty(strDv))
                    {
                        strDv = "0";
                    }
                    else
                    {
                        int intPos = strReturn.IndexOf('-');
                        if (intPos != -1)
                        {
                            strDv = strReturn.Substring(intPos);
                        }
                        else
                        {
                            intPos = strReturn.IndexOf('+');
                            if (intPos != -1)
                            {
                                strDv = strReturn.Substring(intPos);
                            }
                        }
                    }

                    string strToAppend = string.Empty;
                    int intDrainDv = 0;
                    if (strDv.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                    {
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdReturn))
                        {
                            sbdReturn.Append(strDv);
                            foreach (Improvement objImprovement in RelevantImprovements(i =>
                                         i.ImproveType == Improvement.ImprovementType.DrainValue
                                         || i.ImproveType == Improvement.ImprovementType.SpellCategoryDrain
                                         || i.ImproveType == Improvement.ImprovementType.SpellDescriptorDrain))
                            {
                                sbdReturn.AppendFormat(GlobalSettings.InvariantCultureInfo, "{0:+0;-0;+0}",
                                                       objImprovement.Value);
                            }

                            if (Limited)
                            {
                                sbdReturn.Append("-2");
                            }

                            if (Extended && _blnCustomExtended)
                            {
                                sbdReturn.Append("+2");
                            }

                            if (BarehandedAdept && !blnForce)
                            {
                                sbdReturn.Insert(0, "2 * (").Append(')');
                            }

                            (bool blnIsSuccess, object xprResult) = CommonFunctions.EvaluateInvariantXPath(sbdReturn.ToString());
                            if (blnIsSuccess)
                                intDrainDv = ((double)xprResult).StandardRound();
                            else
                                strToAppend = sbdReturn.ToString();
                        }
                    }
                    else
                    {
                        foreach (Improvement objImprovement in RelevantImprovements(i =>
                                         i.ImproveType == Improvement.ImprovementType.DrainValue
                                         || i.ImproveType == Improvement.ImprovementType.SpellCategoryDrain
                                         || i.ImproveType == Improvement.ImprovementType.SpellDescriptorDrain))
                        {
                            decValue += objImprovement.Value;
                        }
                        if (Limited)
                            decValue -= 2;
                        if (Extended && _blnCustomExtended)
                            decValue += 2;
                        if (BarehandedAdept && !blnForce)
                            decValue *= 2;
                        intDrainDv = decValue.StandardRound();
                    }

                    if (blnForce)
                    {
                        if (!string.IsNullOrEmpty(strToAppend))
                        {
                            strReturn += 'F' + strToAppend;
                            if (BarehandedAdept)
                                strReturn = "2 * (" + strReturn + ')';
                        }
                        else
                            strReturn = string.Format(GlobalSettings.InvariantCultureInfo,
                                                  BarehandedAdept ? "2 * (F{0:+0;-0;})" : "F{0:+0;-0;}", intDrainDv);
                    }
                    else if (!string.IsNullOrEmpty(strToAppend))
                    {
                        strReturn += strToAppend;
                        if (BarehandedAdept)
                            strReturn = "2 * (" + strReturn + ')';
                    }
                    else
                        // Drain always minimum 2 (doubled for Barehanded Adept)
                        strReturn = Math.Max(intDrainDv, BarehandedAdept ? 4 : 2).ToString(GlobalSettings.InvariantCultureInfo);

                    return strReturn;
                }
            }
        }

        /// <summary>
        /// Spell's drain value.
        /// </summary>
        public async Task<string> GetCalculatedDvAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn = DvBase;
                if (!Limited && !(Extended && _blnCustomExtended) && !BarehandedAdept && (await RelevantImprovementsAsync(o =>
                        o.ImproveType == Improvement.ImprovementType.DrainValue
                        || o.ImproveType == Improvement.ImprovementType.SpellCategoryDrain
                        || o.ImproveType == Improvement.ImprovementType.SpellDescriptorDrain, true, token).ConfigureAwait(false)).Count == 0)
                    return strReturn;
                bool blnForce = strReturn.StartsWith('F');
                string strDv = blnForce ? strReturn.TrimStartOnce("F", true) : strReturn;
                //Navigator can't do math on a single value, so inject a mathable value.
                if (string.IsNullOrEmpty(strDv))
                {
                    strDv = "0";
                }
                else
                {
                    int intPos = strReturn.IndexOf('-');
                    if (intPos != -1)
                    {
                        strDv = strReturn.Substring(intPos);
                    }
                    else
                    {
                        intPos = strReturn.IndexOf('+');
                        if (intPos != -1)
                        {
                            strDv = strReturn.Substring(intPos);
                        }
                    }
                }

                string strToAppend = string.Empty;
                int intDrainDv = 0;
                if (strDv.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdReturn))
                    {
                        sbdReturn.Append(strDv);
                        foreach (Improvement objImprovement in await RelevantImprovementsAsync(i =>
                                     i.ImproveType == Improvement.ImprovementType.DrainValue
                                     || i.ImproveType == Improvement.ImprovementType.SpellCategoryDrain
                                     || i.ImproveType == Improvement.ImprovementType.SpellDescriptorDrain, token: token).ConfigureAwait(false))
                        {
                            sbdReturn.AppendFormat(GlobalSettings.InvariantCultureInfo, "{0:+0;-0;+0}",
                                                   objImprovement.Value);
                        }

                        if (Limited)
                        {
                            sbdReturn.Append("-2");
                        }

                        if (Extended && _blnCustomExtended)
                        {
                            sbdReturn.Append("+2");
                        }

                        if (BarehandedAdept && !blnForce)
                        {
                            sbdReturn.Insert(0, "2 * (").Append(')');
                        }

                        (bool blnIsSuccess, object xprResult) = await CommonFunctions.EvaluateInvariantXPathAsync(sbdReturn.ToString(), token).ConfigureAwait(false);
                        if (blnIsSuccess)
                            intDrainDv = ((double)xprResult).StandardRound();
                        else
                            strToAppend = sbdReturn.ToString();
                    }
                }
                else
                {
                    foreach (Improvement objImprovement in await RelevantImprovementsAsync(i =>
                                     i.ImproveType == Improvement.ImprovementType.DrainValue
                                     || i.ImproveType == Improvement.ImprovementType.SpellCategoryDrain
                                     || i.ImproveType == Improvement.ImprovementType.SpellDescriptorDrain, token: token).ConfigureAwait(false))
                    {
                        decValue += objImprovement.Value;
                    }
                    if (Limited)
                        decValue -= 2;
                    if (Extended && _blnCustomExtended)
                        decValue += 2;
                    if (BarehandedAdept && !blnForce)
                        decValue *= 2;
                    intDrainDv = decValue.StandardRound();
                }

                if (blnForce)
                {
                    if (!string.IsNullOrEmpty(strToAppend))
                    {
                        strReturn += "F" + strToAppend;
                        if (BarehandedAdept)
                            strReturn = "2 * (" + strReturn + ')';
                    }
                    else
                        strReturn = string.Format(GlobalSettings.InvariantCultureInfo,
                                              BarehandedAdept ? "2 * (F{0:+0;-0;})" : "F{0:+0;-0;}", intDrainDv);
                }
                else if (!string.IsNullOrEmpty(strToAppend))
                {
                    strReturn += strToAppend;
                    if (BarehandedAdept)
                        strReturn = "2 * (" + strReturn + ')';
                }
                else
                    // Drain always minimum 2 (doubled for Barehanded Adept)
                    strReturn = Math.Max(intDrainDv, BarehandedAdept ? 4 : 2).ToString(GlobalSettings.InvariantCultureInfo);

                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DvBase
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strDV;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strDV = value;
            }
        }

        /// <summary>
        /// Spell's Source.
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
        /// Extra information from Improvement dialogues.
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
        /// Whether the Spell is Limited.
        /// </summary>
        public bool Limited
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnLimited;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _blnLimited = value;
            }
        }

        /// <summary>
        /// Whether the Spell is Extended.
        /// </summary>
        public bool Extended
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnExtended;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnExtended == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnExtended == value)
                        return;
                    if (value)
                    {
                        bool blnNewCustomExtended = !HashDescriptors.Any(x =>
                            string.Equals(
                                x.Trim(), "Extended Area",
                                StringComparison.OrdinalIgnoreCase));
                        using (LockObject.EnterWriteLock())
                        {
                            _blnExtended = true;
                            _blnCustomExtended = blnNewCustomExtended;
                        }
                    }
                    else
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            _blnExtended = false;
                            _blnCustomExtended = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Whether the Spell is Alchemical.
        /// </summary>
        public bool Alchemical
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAlchemical;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _blnAlchemical = value;
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
                using (LockObject.EnterUpgradeableReadLock())
                    _strNotes = value;
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
                using (LockObject.EnterUpgradeableReadLock())
                    _colNotes = value;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn
                    = !strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                        ? this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name
                        : Name;
                if (Extended && _blnCustomExtended)
                    strReturn += ',' + LanguageManager.GetString("String_Space", strLanguage)
                                     + LanguageManager.GetString("String_SpellExtended", strLanguage);

                return strReturn;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            string strReturn;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    strReturn = Name;
                else
                {
                    XPathNavigator objNode
                        = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                    strReturn = objNode != null
                        ? objNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? Name
                        : Name;
                }

                if (Extended && _blnCustomExtended)
                    strReturn += ','
                                 + await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                                                        .ConfigureAwait(false) + await LanguageManager
                                     .GetStringAsync("String_SpellExtended", strLanguage, token: token)
                                     .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists.
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn = DisplayNameShort(strLanguage);

                if (Limited)
                    strReturn += LanguageManager.GetString("String_Space", strLanguage) + '('
                        + LanguageManager.GetString("String_SpellLimited", strLanguage) + ')';
                if (Alchemical)
                    strReturn += LanguageManager.GetString("String_Space", strLanguage) + '('
                        + LanguageManager.GetString("String_SpellAlchemical", strLanguage) + ')';
                if (!string.IsNullOrEmpty(Extra))
                {
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += LanguageManager.GetString("String_Space", strLanguage) + '('
                        + _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
                }

                return strReturn;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists.
        /// </summary>
        public async Task<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                                                       .ConfigureAwait(false);
                if (Limited)
                    strReturn += strSpace + '(' + await LanguageManager
                                                        .GetStringAsync("String_SpellLimited", strLanguage,
                                                                        token: token).ConfigureAwait(false) + ')';
                if (Alchemical)
                    strReturn += strSpace + '(' + await LanguageManager
                                                        .GetStringAsync("String_SpellAlchemical", strLanguage,
                                                                        token: token).ConfigureAwait(false) + ')';
                if (!string.IsNullOrEmpty(Extra))
                {
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += strSpace + '(' + await _objCharacter
                                                        .TranslateExtraAsync(Extra, strLanguage, token: token)
                                                        .ConfigureAwait(false) + ')';
                }

                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
            DisplayNameAsync(GlobalSettings.Language, token);

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) =>
            DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        /// Does the spell cost Karma? Typically provided by improvements.
        /// </summary>
        public bool FreeBonus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnFreeBonus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _blnFreeBonus = value;
            }
        }

        /// <summary>
        /// Does the spell use Unarmed in place of Spellcasting for its casting test and have its drain doubled?
        /// </summary>
        public bool BarehandedAdept
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnBarehandedAdept;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _blnBarehandedAdept = value;
            }
        }

        /// <summary>
        /// Active Skill that should be used with this Spell instead of the default one.
        /// </summary>
        public string UseSkill
        {
            get => _strUseSkill;
            set => _strUseSkill = value;
        }

        #endregion Properties

        #region ComplexProperties

        /// <summary>
        /// Skill used by this spell
        /// </summary>
        public Skill Skill
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    string strSkillKey = string.Empty;
                    if (string.IsNullOrEmpty(UseSkill))
                    {
                        XPathNavigator objCategoryNode = _objCharacter.LoadDataXPath("spells.xml")
                                              .SelectSingleNode(
                                                  "/chummer/categories/category[. = "
                                                  + Category.CleanXPath() + ']');
                        if (objCategoryNode == null)
                            return null;
                        objCategoryNode.TryGetStringFieldQuickly("@useskill", ref strSkillKey);
                        strSkillKey =
                            RelevantImprovements(o => o.ImproveType == Improvement.ImprovementType.ReplaceSkillSpell)
                                .FirstOrDefault()?.Target ?? strSkillKey;
                        if (Alchemical)
                        {
                            objCategoryNode.TryGetStringFieldQuickly("@alchemicalskill", ref strSkillKey);
                        }
                        else if (BarehandedAdept)
                        {
                            objCategoryNode.TryGetStringFieldQuickly("@barehandedadeptskill", ref strSkillKey);
                        }
                    }
                    else
                    {
                        strSkillKey = UseSkill;
                    }

                    return string.IsNullOrEmpty(strSkillKey)
                        ? null
                        : _objCharacter.SkillsSection.GetActiveSkill(strSkillKey);
                }
            }
        }

        /// <summary>
        /// Skill used by this spell
        /// </summary>
        public async Task<Skill> GetSkillAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strSkillKey = string.Empty;
                if (string.IsNullOrEmpty(UseSkill))
                {
                    // If UseSkill is not set, we need to look it up in the XML file.
                    // This is done asynchronously to avoid blocking the UI thread.
                    XPathNavigator objCategoryNode = (await _objCharacter.LoadDataXPathAsync("spells.xml", token: token).ConfigureAwait(false))
                    .SelectSingleNode(
                        "/chummer/categories/category[. = "
                        + Category.CleanXPath() + ']');
                    if (objCategoryNode == null)
                        return null;
                    objCategoryNode.TryGetStringFieldQuickly("@useskill", ref strSkillKey);
                    strSkillKey =
                        (await RelevantImprovementsAsync(o => o.ImproveType == Improvement.ImprovementType.ReplaceSkillSpell, token: token).ConfigureAwait(false))
                            .FirstOrDefault()?.Target ?? strSkillKey;
                    if (Alchemical)
                    {
                        objCategoryNode.TryGetStringFieldQuickly("@alchemicalskill", ref strSkillKey);
                    }
                    else if (BarehandedAdept)
                    {
                        objCategoryNode.TryGetStringFieldQuickly("@barehandedadeptskill", ref strSkillKey);
                    }
                }
                else
                {
                    // If UseSkill is set, we use that directly.
                    strSkillKey = UseSkill;
                }

                return string.IsNullOrEmpty(strSkillKey)
                    ? null
                    : await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetActiveSkillAsync(strSkillKey, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The Dice Pool size for the Active Skill required to cast the Spell.
        /// </summary>
        public int DicePool
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    Skill objSkill = Skill;
                    int intReturn = objSkill != null
                        ? (BarehandedAdept
                            ? objSkill.PoolOtherAttribute("MAG")
                            : objSkill.Pool) + objSkill.GetSpecializationBonus(Category)
                        : 0;

                    // Include any Improvements to the Spell's dicepool.
                    intReturn += RelevantImprovements(x =>
                                                          x.ImproveType == Improvement.ImprovementType.SpellCategory
                                                          || x.ImproveType == Improvement.ImprovementType.SpellDicePool)
                                 .Sum(x => x.Value).StandardRound();
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
                    ? (BarehandedAdept
                        ? await objSkill.PoolOtherAttributeAsync("MAG", token: token).ConfigureAwait(false)
                        : await objSkill.GetPoolAsync(token).ConfigureAwait(false)) + await objSkill.GetSpecializationBonusAsync(Category, token).ConfigureAwait(false)
                    : 0;

                // Include any Improvements to the Spell's dicepool.
                intReturn += (await RelevantImprovementsAsync(x =>
                        x.ImproveType == Improvement.ImprovementType.SpellCategory
                        || x.ImproveType == Improvement.ImprovementType.SpellDicePool, token: token).ConfigureAwait(false))
                    .Sum(x => x.Value).StandardRound();
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
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdReturn))
                {
                    string strFormat = strSpace + "{0}" + strSpace + "({1})";
                    Skill objSkill = Skill;
                    CharacterAttrib objAttrib
                        = _objCharacter.GetAttribute(BarehandedAdept ? "MAG" : objSkill?.Attribute ?? "MAG");
                    if (objAttrib != null)
                    {
                        sbdReturn.AppendFormat(GlobalSettings.CultureInfo, strFormat,
                                               objAttrib.DisplayNameFormatted, objAttrib.DisplayValue);
                    }

                    if (objSkill != null)
                    {
                        int intPool = BarehandedAdept ? objSkill.PoolOtherAttribute("MAG") : objSkill.Pool;
                        if (objAttrib != null)
                            intPool -= objAttrib.TotalValue;
                        if (sbdReturn.Length > 0)
                            sbdReturn.Append(strSpace).Append('+').Append(strSpace);
                        sbdReturn.Append(objSkill.FormattedDicePool(intPool, Category));
                    }

                    // Include any Improvements to the Spell Category or Spell Name.
                    foreach (Improvement objImprovement in RelevantImprovements(
                                 x => x.ImproveType == Improvement.ImprovementType.SpellCategory
                                      || x.ImproveType == Improvement.ImprovementType.SpellDicePool))
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
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                           out StringBuilder sbdReturn))
                {
                    string strFormat = strSpace + "{0}" + strSpace + "({1})";
                    Skill objSkill = await GetSkillAsync(token).ConfigureAwait(false);
                    CharacterAttrib objAttrib
                        = await _objCharacter.GetAttributeAsync(
                            BarehandedAdept ? "MAG" :
                            objSkill != null ? await objSkill.GetAttributeAsync(token).ConfigureAwait(false) : "MAG", token: token).ConfigureAwait(false);
                    if (objAttrib != null)
                    {
                        sbdReturn.AppendFormat(GlobalSettings.CultureInfo, strFormat,
                            objAttrib.DisplayNameFormatted, objAttrib.DisplayValue);
                    }

                    if (objSkill != null)
                    {
                        int intPool = BarehandedAdept
                            ? await objSkill.PoolOtherAttributeAsync("MAG", token: token).ConfigureAwait(false)
                            : await objSkill.GetPoolAsync(token).ConfigureAwait(false);
                        if (objAttrib != null)
                            intPool -= await objAttrib.GetTotalValueAsync(token).ConfigureAwait(false);
                        if (sbdReturn.Length > 0)
                            sbdReturn.Append(strSpace).Append('+').Append(strSpace);
                        sbdReturn.Append(await objSkill.FormattedDicePoolAsync(intPool, Category, token).ConfigureAwait(false));
                    }

                    // Include any Improvements to the Spell Category or Spell Name.
                    foreach (Improvement objImprovement in await RelevantImprovementsAsync(
                                 x => x.ImproveType == Improvement.ImprovementType.SpellCategory
                                      || x.ImproveType == Improvement.ImprovementType.SpellDicePool, token: token).ConfigureAwait(false))
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
                XmlDocument objDoc = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadData("spells.xml", strLanguage, token: token)
                    : await _objCharacter.LoadDataAsync("spells.xml", strLanguage, token: token)
                                         .ConfigureAwait(false);
                if (SourceID != Guid.Empty)
                    objReturn = objDoc.TryGetNodeById("/chummer/spells/spell", SourceID);
                if (objReturn == null)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId("/chummer/spells/spell", Name);
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
        private HashSet<string> _setDescriptors = Utils.StringHashSetPool.Get();

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
                    ? _objCharacter.LoadDataXPath("spells.xml", strLanguage, token: token)
                    : await _objCharacter.LoadDataXPathAsync("spells.xml", strLanguage, token: token)
                                         .ConfigureAwait(false);
                if (SourceID != Guid.Empty)
                    objReturn = objDoc.TryGetNodeById("/chummer/spells/spell", SourceID);
                if (objReturn == null)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId("/chummer/spells/spell", Name);
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

        private IEnumerable<Improvement> RelevantImprovements(Func<Improvement, bool> funcWherePredicate = null, bool blnExitAfterFirst = false)
        {
            using (LockObject.EnterReadLock())
            {
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (!objImprovement.Enabled || funcWherePredicate?.Invoke(objImprovement) != true)
                        continue;

                    switch (objImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.SpellDicePool:
                            if (objImprovement.ImprovedName == Name
                                || objImprovement.ImprovedName == SourceID.ToString())
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst) yield break;
                            }

                            break;

                        case Improvement.ImprovementType.ReplaceSkillSpell:
                            if (objImprovement.ImprovedName == Name
                                || objImprovement.ImprovedName == SourceID.ToString()
                                || string.IsNullOrEmpty(objImprovement.ImprovedName))
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst) yield break;
                            }

                            break;

                        case Improvement.ImprovementType.SpellCategory:
                            if (objImprovement.ImprovedName == Category)
                            {
                                // SR5 318: Regardless of the number of bonded foci you have,
                                // only one focus may add its Force to a dicepool for any given test.
                                // We need to do some checking to make sure this is the most powerful focus before we add it in
                                if (objImprovement.ImproveSource == Improvement.ImprovementSource.Gear)
                                {
                                    //TODO: THIS IS NOT SAFE. While we can mostly assume that Gear that add to SpellCategory are Foci, it's not reliable.
                                    // we are returning either the original improvement, null or a newly instantiated improvement
                                    Improvement bestFocus = CompareFocusPower(objImprovement);
                                    if (bestFocus != null)
                                    {
                                        yield return bestFocus;
                                        if (blnExitAfterFirst) yield break;
                                    }
                                }
                                else
                                {
                                    yield return objImprovement;
                                    if (blnExitAfterFirst) yield break;
                                }
                            }

                            break;

                        case Improvement.ImprovementType.SpellCategoryDamage:
                        case Improvement.ImprovementType.SpellCategoryDrain:
                            if (objImprovement.ImprovedName == Category)
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst) yield break;
                            }

                            break;

                        case Improvement.ImprovementType.SpellDescriptorDrain:
                        case Improvement.ImprovementType.SpellDescriptorDamage:
                            if (HashDescriptors.Count > 0)
                            {
                                bool blnAllow = false;
                                foreach (string strDescriptor in objImprovement.ImprovedName.SplitNoAlloc(
                                             ',', StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (strDescriptor.StartsWith("NOT", StringComparison.Ordinal))
                                    {
                                        if (HashDescriptors.Contains(
                                                strDescriptor.TrimStartOnce("NOT(").TrimEndOnce(')')))
                                        {
                                            blnAllow = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        blnAllow = HashDescriptors.Contains(strDescriptor);
                                    }
                                }

                                if (blnAllow)
                                {
                                    yield return objImprovement;
                                    if (blnExitAfterFirst) yield break;
                                }
                            }

                            break;

                        case Improvement.ImprovementType.DrainValue:
                        {
                            if (string.IsNullOrEmpty(objImprovement.ImprovedName)
                                || objImprovement.ImprovedName == Name)
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst) yield break;
                            }
                        }
                            break;
                    }
                }
            }
        }

        private async Task<List<Improvement>> RelevantImprovementsAsync(Func<Improvement, bool> funcWherePredicate = null, bool blnExitAfterFirst = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                List<Improvement> lstReturn = new List<Improvement>();
                await _objCharacter.Improvements.ForEachWithBreakAsync(async objImprovement =>
                {
                    if (!objImprovement.Enabled || funcWherePredicate?.Invoke(objImprovement) != true)
                        return true;

                    switch (objImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.SpellDicePool:
                            if (objImprovement.ImprovedName == Name
                                || objImprovement.ImprovedName == SourceID.ToString())
                            {
                                lstReturn.Add(objImprovement);
                                if (blnExitAfterFirst)
                                    return false;
                            }

                            break;

                        case Improvement.ImprovementType.ReplaceSkillSpell:
                            if (objImprovement.ImprovedName == Name
                                || objImprovement.ImprovedName == SourceID.ToString()
                                || string.IsNullOrEmpty(objImprovement.ImprovedName))
                            {
                                lstReturn.Add(objImprovement);
                                if (blnExitAfterFirst)
                                    return false;
                            }

                            break;

                        case Improvement.ImprovementType.SpellCategory:
                            if (objImprovement.ImprovedName == Category)
                            {
                                // SR5 318: Regardless of the number of bonded foci you have,
                                // only one focus may add its Force to a dicepool for any given test.
                                // We need to do some checking to make sure this is the most powerful focus before we add it in
                                if (objImprovement.ImproveSource == Improvement.ImprovementSource.Gear)
                                {
                                    //TODO: THIS IS NOT SAFE. While we can mostly assume that Gear that add to SpellCategory are Foci, it's not reliable.
                                    // we are returning either the original improvement, null or a newly instantiated improvement
                                    Improvement bestFocus = await _objCharacter.GetBestFocusPowerAsync(objImprovement, token).ConfigureAwait(false);
                                    if (bestFocus != null)
                                    {
                                        lstReturn.Add(bestFocus);
                                        if (blnExitAfterFirst)
                                            return false;
                                    }
                                }
                                else
                                {
                                    lstReturn.Add(objImprovement);
                                    if (blnExitAfterFirst)
                                        return false;
                                }
                            }

                            break;

                        case Improvement.ImprovementType.SpellCategoryDamage:
                        case Improvement.ImprovementType.SpellCategoryDrain:
                            if (objImprovement.ImprovedName == Category)
                            {
                                lstReturn.Add(objImprovement);
                                if (blnExitAfterFirst)
                                    return false;
                            }

                            break;

                        case Improvement.ImprovementType.SpellDescriptorDrain:
                        case Improvement.ImprovementType.SpellDescriptorDamage:
                            if (HashDescriptors.Count > 0)
                            {
                                bool blnAllow = false;
                                foreach (string strDescriptor in objImprovement.ImprovedName.SplitNoAlloc(
                                             ',', StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (strDescriptor.StartsWith("NOT", StringComparison.Ordinal))
                                    {
                                        if (HashDescriptors.Contains(
                                                strDescriptor.TrimStartOnce("NOT(").TrimEndOnce(')')))
                                        {
                                            blnAllow = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        blnAllow = HashDescriptors.Contains(strDescriptor);
                                    }
                                }

                                if (blnAllow)
                                {
                                    lstReturn.Add(objImprovement);
                                    if (blnExitAfterFirst)
                                        return false;
                                }
                            }

                            break;

                        case Improvement.ImprovementType.DrainValue:
                        {
                            if (string.IsNullOrEmpty(objImprovement.ImprovedName)
                                || objImprovement.ImprovedName == Name)
                            {
                                lstReturn.Add(objImprovement);
                                if (blnExitAfterFirst)
                                    return false;
                            }
                        }
                            break;
                    }

                    return true;
                }, token: token).ConfigureAwait(false);
                return lstReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Method to check we are only applying the highest focus to the spell dicepool
        /// </summary>
        private Improvement CompareFocusPower(Improvement objImprovement)
        {
            using (LockObject.EnterReadLock())
            {
                List<Focus> list
                    = _objCharacter.Foci.FindAll(
                        x => x.GearObject?.Bonded == true && x.GearObject.Bonus.InnerText == "MAGRating");
                if (list.Count > 0)
                {
                    // get any bonded foci that add to the base magic stat and return the highest rated one's rating
                    int powerFocusRating = list.Max(x => x.Rating);

                    // If our focus is higher, add in a partial bonus
                    if (powerFocusRating > 0)
                    {
                        // This is hackz -- because we don't want to lose the original improvement's value
                        // we instantiate a fake version of the improvement that isn't saved to represent the diff
                        if (powerFocusRating < objImprovement.Value)
                            return new Improvement(_objCharacter)
                            {
                                Value = objImprovement.Value - powerFocusRating,
                                SourceName = objImprovement.SourceName,
                                ImprovedName = objImprovement.ImprovedName,
                                ImproveSource = objImprovement.ImproveSource,
                                ImproveType = objImprovement.ImproveType
                            };
                        return null;
                    }
                }

                return objImprovement;
            }
        }

        #endregion ComplexProperties

        #region UI Methods

        public TreeNode CreateTreeNode(ContextMenuStrip cmsSpell, bool blnAddCategory = false)
        {
            using (LockObject.EnterReadLock())
            {
                if (Grade != 0 && !string.IsNullOrEmpty(Source) && !_objCharacter.Settings.BookEnabled(Source))
                    return null;

                string strText = CurrentDisplayName;
                if (blnAddCategory)
                {
                    switch (Category)
                    {
                        case "Rituals":
                            strText = LanguageManager.GetString("Label_Ritual")
                                      + LanguageManager.GetString("String_Space") + strText;
                            break;

                        case "Enchantments":
                            strText = LanguageManager.GetString("Label_Enchantment")
                                      + LanguageManager.GetString("String_Space") + strText;
                            break;
                    }
                }

                TreeNode objNode = new TreeNode
                {
                    Name = InternalId,
                    Text = strText,
                    Tag = this,
                    ContextMenuStrip = cmsSpell,
                    ForeColor = PreferredColor,
                    ToolTipText = Notes.WordWrap()
                };

                return objNode;
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

        #endregion UI Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (blnConfirmDelete)
                {
                    if (Grade != 0) // If we are prompting, we are not removing this by removing the initiation/submersion that granted it
                        return false;
                    if (!CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteSpell")))
                        return false;
                }

                using (LockObject.EnterWriteLock())
                {
                    _objCharacter.Spells.Remove(this);
                    ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Spell,
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
                if (blnConfirmDelete)
                {
                    if (Grade != 0) // If we are prompting, we are not removing this by removing the initiation/submersion that granted it
                        return false;
                    if (!await CommonFunctions
                            .ConfirmDeleteAsync(
                                await LanguageManager.GetStringAsync("Message_DeleteSpell", token: token)
                                    .ConfigureAwait(false), token).ConfigureAwait(false))
                        return false;
                }

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    await _objCharacter.Spells.RemoveAsync(this, token).ConfigureAwait(false);
                    await ImprovementManager
                        .RemoveImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Spell, InternalId, token)
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

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
                Utils.StringHashSetPool.Return(ref _setDescriptors);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                Utils.StringHashSetPool.Return(ref _setDescriptors);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }
    }
}
