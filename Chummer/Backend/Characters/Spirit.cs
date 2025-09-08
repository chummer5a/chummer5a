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
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;
using Chummer.Backend.Enums;
using Chummer.Backend.Skills;

namespace Chummer
{
    /// <summary>
    /// A Magician's Spirit or Technomancer's Sprite.
    /// </summary>
    [DebuggerDisplay("{Name}, \"{CritterName}\"")]
    public sealed class Spirit : IHasInternalId, IHasName, IHasXmlDataNode, IHasMugshots, INotifyMultiplePropertiesChangedAsync, IHasNotes, IHasLockObject, IHasCharacterObject
    {
        private Guid _guiId;
        private string _strName = string.Empty;
        private string _strCritterName = string.Empty;
        private int _intServicesOwed;
        private SpiritType _eEntityType = SpiritType.Spirit;
        private bool _blnBound = true;
        private int _intForce = 1;
        private string _strFileName = string.Empty;
        private string _strRelativeName = string.Empty;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private Character _objLinkedCharacter;

        private readonly ThreadSafeList<Image> _lstMugshots;
        private int _intMainMugshotIndex = -1;

        #region Helper Methods

        /// <summary>
        /// Convert a string to a SpiritType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static SpiritType ConvertToSpiritType(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return default;
            switch (strValue)
            {
                case "Spirit":
                    return SpiritType.Spirit;

                default:
                    return SpiritType.Sprite;
            }
        }

        #endregion Helper Methods

        #region Constructor, Save, Load, and Print Methods

        public Spirit(Character objCharacter)
        {
            // Create the GUID for the new Spirit.
            _guiId = Guid.NewGuid();
            CharacterObject = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = objCharacter.LockObject;
            _lstMugshots = new ThreadSafeList<Image>(3, LockObject);
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Save(XmlWriter objWriter, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => SaveCoreAsync(true, objWriter, token), token);
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task SaveAsync(XmlWriter objWriter, CancellationToken token = default)
        {
            return SaveCoreAsync(false, objWriter, token);
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="blnSync"></param>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private async Task SaveCoreAsync(bool blnSync, XmlWriter objWriter, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            if (blnSync)
            {
                // ReSharper disable MethodHasAsyncOverload
                // ReSharper disable MethodHasAsyncOverloadWithCancellation
                using (LockObject.EnterReadLock(token))
                using (objWriter.StartElement("spirit"))
                {
                    objWriter.WriteElementString(
                        "guid", _guiId.ToString("D", GlobalSettings.InvariantCultureInfo));
                    objWriter.WriteElementString("name", _strName);
                    objWriter.WriteElementString("crittername", _strCritterName);
                    objWriter.WriteElementString(
                        "services", _intServicesOwed.ToString(GlobalSettings.InvariantCultureInfo));
                    objWriter.WriteElementString(
                        "force", _intForce.ToString(GlobalSettings.InvariantCultureInfo));
                    objWriter.WriteElementString(
                        "bound", _blnBound.ToString(GlobalSettings.InvariantCultureInfo));
                    objWriter.WriteElementString(
                        "fettered", _blnFettered.ToString(GlobalSettings.InvariantCultureInfo));
                    objWriter.WriteElementString("type", _eEntityType.ToString());
                    objWriter.WriteElementString("file", _strFileName);
                    objWriter.WriteElementString("relative", _strRelativeName);
                    objWriter.WriteElementString("notes", _strNotes.CleanOfXmlInvalidUnicodeChars());
                    objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));

                    SaveMugshots(objWriter, token);
                }
                // ReSharper enable MethodHasAsyncOverload
                // ReSharper enable MethodHasAsyncOverloadWithCancellation
            }
            else
            {
                IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    // <spirit>
                    XmlElementWriteHelper objBaseElement
                        = await objWriter.StartElementAsync("spirit", token: token).ConfigureAwait(false);
                    try
                    {
                        await objWriter.WriteElementStringAsync(
                                "guid", _guiId.ToString("D", GlobalSettings.InvariantCultureInfo),
                                token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("name", _strName, token: token).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("crittername", _strCritterName, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync(
                                "services", _intServicesOwed.ToString(GlobalSettings.InvariantCultureInfo),
                                token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync(
                                "force", _intForce.ToString(GlobalSettings.InvariantCultureInfo),
                                token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync(
                                "bound", _blnBound.ToString(GlobalSettings.InvariantCultureInfo),
                                token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync(
                                "fettered", _blnFettered.ToString(GlobalSettings.InvariantCultureInfo),
                                token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("type", _eEntityType.ToString(), token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("file", _strFileName, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("relative", _strRelativeName, token: token)
                            .ConfigureAwait(false);
                        await objWriter
                            .WriteElementStringAsync("notes", _strNotes.CleanOfXmlInvalidUnicodeChars(), token: token)
                            .ConfigureAwait(false);
                        await objWriter
                            .WriteElementStringAsync("notesColor", ColorTranslator.ToHtml(_colNotes), token: token)
                            .ConfigureAwait(false);
                        await SaveMugshotsAsync(objWriter, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        // </spirit>
                        await objBaseElement.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Load the Spirit from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Load(XPathNavigator objNode, CancellationToken token = default)
        {
            if (objNode == null)
                return;
            using (LockObject.EnterWriteLock(token))
            {
                objNode.TryGetField("guid", Guid.TryParse, out _guiId);
                if (_guiId == Guid.Empty)
                    _guiId = Guid.NewGuid();
                if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                {
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                }

                string strTemp = string.Empty;
                if (objNode.TryGetStringFieldQuickly("type", ref strTemp))
                    _eEntityType = ConvertToSpiritType(strTemp);
                objNode.TryGetStringFieldQuickly("crittername", ref _strCritterName);
                objNode.TryGetInt32FieldQuickly("services", ref _intServicesOwed);
                objNode.TryGetInt32FieldQuickly("force", ref _intForce);
                Force = _intForce;
                objNode.TryGetBoolFieldQuickly("bound", ref _blnBound);
                objNode.TryGetBoolFieldQuickly("fettered", ref _blnFettered);
                objNode.TryGetStringFieldQuickly("file", ref _strFileName);
                objNode.TryGetStringFieldQuickly("relative", ref _strRelativeName);
                objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                RefreshLinkedCharacter(token: token);

                LoadMugshots(objNode, token);
            }
        }

        /// <summary>
        /// Load the Spirit from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task LoadAsync(XPathNavigator objNode, CancellationToken token = default)
        {
            if (objNode == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                objNode.TryGetField("guid", Guid.TryParse, out _guiId);
                if (_guiId == Guid.Empty)
                    _guiId = Guid.NewGuid();
                if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                {
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                }

                string strTemp = string.Empty;
                if (objNode.TryGetStringFieldQuickly("type", ref strTemp))
                    _eEntityType = ConvertToSpiritType(strTemp);
                objNode.TryGetStringFieldQuickly("crittername", ref _strCritterName);
                objNode.TryGetInt32FieldQuickly("services", ref _intServicesOwed);
                objNode.TryGetInt32FieldQuickly("force", ref _intForce);
                await SetForceAsync(_intForce, token).ConfigureAwait(false);
                objNode.TryGetBoolFieldQuickly("bound", ref _blnBound);
                objNode.TryGetBoolFieldQuickly("fettered", ref _blnFettered);
                objNode.TryGetStringFieldQuickly("file", ref _strFileName);
                objNode.TryGetStringFieldQuickly("relative", ref _strRelativeName);
                objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

                string strNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objNode.TryGetStringFieldQuickly("notesColor", ref strNotesColor);
                _colNotes = ColorTranslator.FromHtml(strNotesColor);

                await RefreshLinkedCharacterAsync(token: token).ConfigureAwait(false);

                await LoadMugshotsAsync(objNode, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static readonly ReadOnlyCollection<string> s_PrintAttributeLabels = Array.AsReadOnly(new[]
            {"bod", "agi", "rea", "str", "cha", "int", "wil", "log", "ini"});

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
                // Translate the Critter name if applicable.
                string strName = await GetNameAsync(token).ConfigureAwait(false);
                string strDisplayName = strName;
                XmlNode objXmlCritterNode
                    = await this.GetNodeAsync(strLanguageToPrint, token: token).ConfigureAwait(false);
                if (!strLanguageToPrint.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    strDisplayName = objXmlCritterNode?["translate"]?.InnerText ?? strName;
                }

                // <spirit>
                XmlElementWriteHelper objBaseElement
                    = await objWriter.StartElementAsync("spirit", token: token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("guid", InternalId, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name", strDisplayName, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name_english", strName, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("crittername", await GetCritterNameAsync(token).ConfigureAwait(false), token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("fettered", (await GetFetteredAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("bound", (await GetBoundAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("services", (await GetServicesOwedAsync(token).ConfigureAwait(false)).ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("force", (await GetForceAsync(token).ConfigureAwait(false)).ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("ratinglabel",
                            await LanguageManager
                                .GetStringAsync(await GetRatingLabelAsync(token).ConfigureAwait(false), strLanguageToPrint, token: token)
                                .ConfigureAwait(false), token: token).ConfigureAwait(false);

                    if (objXmlCritterNode != null)
                    {
                        //Attributes for spirits, named differently as to not confuse <attribute>

                        Dictionary<string, int> dicAttributes
                            = new Dictionary<string, int>(s_PrintAttributeLabels.Count);
                        // <spiritattributes>
                        XmlElementWriteHelper objSpiritAttributesElement = await objWriter
                            .StartElementAsync("spiritattributes", token: token).ConfigureAwait(false);
                        try
                        {
                            foreach (string strAttribute in s_PrintAttributeLabels)
                            {
                                string strInner = string.Empty;
                                if (objXmlCritterNode.TryGetStringFieldQuickly(strAttribute, ref strInner))
                                {
                                    (bool blnIsSuccess, object objProcess)
                                        = await CommonFunctions.EvaluateInvariantXPathAsync(
                                            strInner.Replace(
                                                "F", _intForce.ToString(GlobalSettings.InvariantCultureInfo)), token).ConfigureAwait(false);
                                    int intValue
                                        = Math.Max(blnIsSuccess ? ((double)objProcess).StandardRound() : _intForce, 1);
                                    await objWriter
                                        .WriteElementStringAsync(strAttribute, intValue.ToString(objCulture),
                                            token: token).ConfigureAwait(false);

                                    dicAttributes[strAttribute] = intValue;
                                }
                            }
                        }
                        finally
                        {
                            // </spiritattributes>
                            await objSpiritAttributesElement.DisposeAsync().ConfigureAwait(false);
                        }

                        if (_objLinkedCharacter != null)
                        {
                            //Dump skills, (optional)powers if present to output

                            XPathNavigator xmlSpiritPowersBaseChummerNode
                                = (await _objLinkedCharacter
                                        .LoadDataXPathAsync("spiritpowers.xml", strLanguageToPrint, token: token)
                                        .ConfigureAwait(false))
                                    .SelectSingleNodeAndCacheExpression("/chummer", token: token);
                            XPathNavigator xmlCritterPowersBaseChummerNode
                                = (await _objLinkedCharacter
                                        .LoadDataXPathAsync("critterpowers.xml", strLanguageToPrint,
                                            token: token).ConfigureAwait(false))
                                    .SelectSingleNodeAndCacheExpression("/chummer", token: token);

                            XmlElement xmlPowersNode = objXmlCritterNode["powers"];
                            if (xmlPowersNode != null)
                            {
                                // <powers>
                                XmlElementWriteHelper objPowersElement = await objWriter
                                    .StartElementAsync(
                                        "powers", token: token)
                                    .ConfigureAwait(false);
                                try
                                {
                                    foreach (XmlNode objXmlPowerNode in xmlPowersNode.ChildNodes)
                                    {
                                        await PrintPowerInfo(objWriter, xmlSpiritPowersBaseChummerNode,
                                            xmlCritterPowersBaseChummerNode, objXmlPowerNode,
                                            strLanguageToPrint, token).ConfigureAwait(false);
                                    }
                                }
                                finally
                                {
                                    // </powers>
                                    await objPowersElement.DisposeAsync().ConfigureAwait(false);
                                }
                            }

                            xmlPowersNode = objXmlCritterNode["optionalpowers"];
                            if (xmlPowersNode != null)
                            {
                                // <optionalpowers>
                                XmlElementWriteHelper objOptionalPowersElement = await objWriter
                                    .StartElementAsync("optionalpowers", token: token).ConfigureAwait(false);
                                try
                                {
                                    foreach (XmlNode objXmlPowerNode in xmlPowersNode.ChildNodes)
                                    {
                                        await PrintPowerInfo(objWriter, xmlSpiritPowersBaseChummerNode,
                                            xmlCritterPowersBaseChummerNode, objXmlPowerNode,
                                            strLanguageToPrint, token).ConfigureAwait(false);
                                    }
                                }
                                finally
                                {
                                    // </optionalpowers>
                                    await objOptionalPowersElement.DisposeAsync().ConfigureAwait(false);
                                }
                            }

                            xmlPowersNode = objXmlCritterNode["skills"];
                            if (xmlPowersNode != null)
                            {
                                XPathNavigator xmlSkillsDocument = await CharacterObject
                                    .LoadDataXPathAsync(
                                        "skills.xml", strLanguageToPrint,
                                        token: token).ConfigureAwait(false);
                                // <skills>
                                XmlElementWriteHelper objSkillsElement = await objWriter
                                    .StartElementAsync(
                                        "skills", token: token)
                                    .ConfigureAwait(false);
                                try
                                {
                                    foreach (XmlNode xmlSkillNode in xmlPowersNode.ChildNodes)
                                    {
                                        string strAttrName = xmlSkillNode.Attributes?["attr"]?.Value ?? string.Empty;
                                        if (!dicAttributes.TryGetValue(strAttrName, out int intAttrValue))
                                            intAttrValue = _intForce;
                                        int intDicepool = intAttrValue + _intForce;

                                        string strEnglishName = xmlSkillNode.InnerText;
                                        string strTranslatedName
                                            = xmlSkillsDocument
                                                  .SelectSingleNode("/chummer/skills/skill[name = "
                                                                    + strEnglishName.CleanXPath() + "]/translate")
                                                  ?.Value ??
                                              xmlSkillsDocument
                                                  .SelectSingleNode(
                                                      "/chummer/knowledgeskills/skill[name = "
                                                      + strEnglishName.CleanXPath()
                                                      + "]/translate")?.Value ?? strEnglishName;
                                        // <skill>
                                        XmlElementWriteHelper objSkillElement = await objWriter
                                            .StartElementAsync("skill", token: token).ConfigureAwait(false);
                                        try
                                        {
                                            await objWriter
                                                .WriteElementStringAsync("name", strTranslatedName, token: token)
                                                .ConfigureAwait(false);
                                            await objWriter
                                                .WriteElementStringAsync("name_english", strEnglishName, token: token)
                                                .ConfigureAwait(false);
                                            await objWriter.WriteElementStringAsync("attr", strAttrName, token: token)
                                                .ConfigureAwait(false);
                                            await objWriter.WriteElementStringAsync(
                                                    "pool", intDicepool.ToString(objCulture), token: token)
                                                .ConfigureAwait(false);
                                        }
                                        finally
                                        {
                                            // </skill>
                                            await objSkillElement.DisposeAsync().ConfigureAwait(false);
                                        }
                                    }
                                }
                                finally
                                {
                                    // </skills>
                                    await objSkillsElement.DisposeAsync().ConfigureAwait(false);
                                }
                            }

                            xmlPowersNode = objXmlCritterNode["weaknesses"];
                            if (xmlPowersNode != null)
                            {
                                // <weaknesses>
                                XmlElementWriteHelper objWeaknessesElement = await objWriter
                                    .StartElementAsync("weaknesses", token: token).ConfigureAwait(false);
                                try
                                {
                                    foreach (XmlNode objXmlPowerNode in xmlPowersNode.ChildNodes)
                                    {
                                        await PrintPowerInfo(objWriter, xmlSpiritPowersBaseChummerNode,
                                            xmlCritterPowersBaseChummerNode, objXmlPowerNode,
                                            strLanguageToPrint, token).ConfigureAwait(false);
                                    }
                                }
                                finally
                                {
                                    // </weaknesses>
                                    await objWeaknessesElement.DisposeAsync().ConfigureAwait(false);
                                }
                            }
                        }

                        //Page in book for reference
                        string strSource = string.Empty;
                        string strPage = string.Empty;

                        if (objXmlCritterNode.TryGetStringFieldQuickly("source", ref strSource))
                            await objWriter
                                .WriteElementStringAsync(
                                    "source",
                                    await CharacterObject.LanguageBookShortAsync(strSource, strLanguageToPrint, token)
                                        .ConfigureAwait(false), token: token).ConfigureAwait(false);
                        if (objXmlCritterNode.TryGetStringFieldQuickly("altpage", ref strPage)
                            || objXmlCritterNode.TryGetStringFieldQuickly("page", ref strPage))
                            await objWriter.WriteElementStringAsync("page", strPage, token: token)
                                .ConfigureAwait(false);
                    }

                    await objWriter
                        .WriteElementStringAsync("bound", (await GetBoundAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("type", (await GetEntityTypeAsync(token).ConfigureAwait(false)).ToString(), token: token)
                        .ConfigureAwait(false);

                    if (GlobalSettings.PrintNotes)
                        await objWriter.WriteElementStringAsync("notes", await GetNotesAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                    await PrintMugshots(objWriter, token).ConfigureAwait(false);
                }
                finally
                {
                    // </spirit>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task PrintPowerInfo(XmlWriter objWriter, XPathNavigator xmlSpiritPowersBaseChummerNode, XPathNavigator xmlCritterPowersBaseChummerNode, XmlNode xmlPowerEntryNode, string strLanguageToPrint = "", CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                           out StringBuilder sbdExtra))
                {
                    string strSelect = xmlPowerEntryNode.SelectSingleNodeAndCacheExpressionAsNavigator("@select", token)?.Value;
                    if (!string.IsNullOrEmpty(strSelect))
                        sbdExtra.Append(await CharacterObject
                            .TranslateExtraAsync(strSelect, strLanguageToPrint, token: token)
                            .ConfigureAwait(false));
                    string strSource = string.Empty;
                    string strPage = string.Empty;
                    string strPowerName = xmlPowerEntryNode.InnerText;
                    string strEnglishName = strPowerName;
                    string strEnglishCategory = string.Empty;
                    string strCategory = string.Empty;
                    string strDisplayType = string.Empty;
                    string strDisplayAction = string.Empty;
                    string strDisplayRange = string.Empty;
                    string strDisplayDuration = string.Empty;
                    XPathNavigator objXmlPowerNode
                        = xmlSpiritPowersBaseChummerNode.SelectSingleNode(
                              "powers/power[name = " + strPowerName.CleanXPath() + ']') ??
                          xmlSpiritPowersBaseChummerNode.SelectSingleNode(
                              "powers/power[starts-with(" + strPowerName.CleanXPath() + ", name)]") ??
                          xmlCritterPowersBaseChummerNode.SelectSingleNode(
                              "powers/power[name = " + strPowerName.CleanXPath() + ']') ??
                          xmlCritterPowersBaseChummerNode.SelectSingleNode(
                              "powers/power[starts-with(" + strPowerName.CleanXPath() + ", name)]");
                    if (objXmlPowerNode != null)
                    {
                        objXmlPowerNode.TryGetStringFieldQuickly("source", ref strSource);
                        if (!objXmlPowerNode.TryGetStringFieldQuickly("altpage", ref strPage))
                            objXmlPowerNode.TryGetStringFieldQuickly("page", ref strPage);

                        objXmlPowerNode.TryGetStringFieldQuickly("name", ref strEnglishName);
                        string strSpace = await LanguageManager
                            .GetStringAsync("String_Space", strLanguageToPrint, token: token)
                            .ConfigureAwait(false);
                        bool blnExtrasAdded = false;
                        foreach (string strLoopExtra in strPowerName
                                     .TrimStartOnce(strEnglishName).Trim().TrimStartOnce('(')
                                     .TrimEndOnce(')')
                                     .SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                        {
                            blnExtrasAdded = true;
                            sbdExtra.Append(await CharacterObject
                                    .TranslateExtraAsync(strLoopExtra, strLanguageToPrint, token: token)
                                    .ConfigureAwait(false)).Append(',')
                                .Append(strSpace);
                        }

                        if (blnExtrasAdded)
                            sbdExtra.Length -= 2;

                        if (!objXmlPowerNode.TryGetStringFieldQuickly("translate", ref strPowerName))
                            strPowerName = strEnglishName;

                        objXmlPowerNode.TryGetStringFieldQuickly("category", ref strEnglishCategory);

                        strCategory = xmlSpiritPowersBaseChummerNode
                                          .SelectSingleNode("categories/category[. = " + strEnglishCategory.CleanXPath()
                                              + "]/@translate")?.Value
                                      ?? strEnglishCategory;

                        switch (objXmlPowerNode.SelectSingleNodeAndCacheExpression("type", token: token)?.Value)
                        {
                            case "M":
                                strDisplayType = await LanguageManager
                                    .GetStringAsync("String_SpellTypeMana", strLanguageToPrint,
                                        token: token).ConfigureAwait(false);
                                break;

                            case "P":
                                strDisplayType = await LanguageManager
                                    .GetStringAsync("String_SpellTypePhysical", strLanguageToPrint,
                                        token: token).ConfigureAwait(false);
                                break;
                        }

                        switch (objXmlPowerNode.SelectSingleNodeAndCacheExpression("action", token: token)?.Value)
                        {
                            case "Auto":
                                strDisplayAction = await LanguageManager
                                    .GetStringAsync(
                                        "String_ActionAutomatic", strLanguageToPrint, token: token)
                                    .ConfigureAwait(false);
                                break;

                            case "Free":
                                strDisplayAction = await LanguageManager
                                    .GetStringAsync(
                                        "String_ActionFree", strLanguageToPrint, token: token)
                                    .ConfigureAwait(false);
                                break;

                            case "Simple":
                                strDisplayAction = await LanguageManager
                                    .GetStringAsync(
                                        "String_ActionSimple", strLanguageToPrint, token: token)
                                    .ConfigureAwait(false);
                                break;

                            case "Complex":
                                strDisplayAction = await LanguageManager
                                    .GetStringAsync(
                                        "String_ActionComplex", strLanguageToPrint, token: token)
                                    .ConfigureAwait(false);
                                break;

                            case "Special":
                                strDisplayAction
                                    = await LanguageManager
                                        .GetStringAsync("String_SpellDurationSpecial", strLanguageToPrint,
                                            token: token).ConfigureAwait(false);
                                break;
                        }

                        switch (objXmlPowerNode.SelectSingleNodeAndCacheExpression("duration", token: token)?.Value)
                        {
                            case "Instant":
                                strDisplayDuration
                                    = await LanguageManager
                                        .GetStringAsync("String_SpellDurationInstantLong", strLanguageToPrint,
                                            token: token).ConfigureAwait(false);
                                break;

                            case "Sustained":
                                strDisplayDuration
                                    = await LanguageManager
                                        .GetStringAsync("String_SpellDurationSustained", strLanguageToPrint,
                                            token: token).ConfigureAwait(false);
                                break;

                            case "Always":
                                strDisplayDuration
                                    = await LanguageManager
                                        .GetStringAsync("String_SpellDurationAlways", strLanguageToPrint,
                                            token: token).ConfigureAwait(false);
                                break;

                            case "Special":
                                strDisplayDuration
                                    = await LanguageManager
                                        .GetStringAsync("String_SpellDurationSpecial", strLanguageToPrint,
                                            token: token).ConfigureAwait(false);
                                break;
                        }

                        if (objXmlPowerNode.TryGetStringFieldQuickly("range", ref strDisplayRange)
                            && !strLanguageToPrint.Equals(GlobalSettings.DefaultLanguage,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            strDisplayRange = await strDisplayRange
                                .CheapReplaceAsync(
                                    "Self",
                                    () => LanguageManager.GetStringAsync(
                                        "String_SpellRangeSelf", strLanguageToPrint, token: token),
                                    token: token)
                                .CheapReplaceAsync(
                                    "Special",
                                    () => LanguageManager.GetStringAsync(
                                        "String_SpellDurationSpecial", strLanguageToPrint,
                                        token: token), token: token)
                                .CheapReplaceAsync(
                                    "LOS",
                                    () => LanguageManager.GetStringAsync(
                                        "String_SpellRangeLineOfSight", strLanguageToPrint,
                                        token: token), token: token)
                                .CheapReplaceAsync(
                                    "LOI",
                                    () => LanguageManager.GetStringAsync(
                                        "String_SpellRangeLineOfInfluence", strLanguageToPrint,
                                        token: token), token: token)
                                .CheapReplaceAsync(
                                    "Touch",
                                    () => LanguageManager.GetStringAsync(
                                        "String_SpellRangeTouch",
                                        strLanguageToPrint, token: token),
                                    token: token) // Short form to remain export-friendly
                                .CheapReplaceAsync(
                                    "T",
                                    () => LanguageManager.GetStringAsync(
                                        "String_SpellRangeTouch", strLanguageToPrint, token: token),
                                    token: token)
                                .CheapReplaceAsync(
                                    "(A)",
                                    async () => '(' + await LanguageManager.GetStringAsync(
                                            "String_SpellRangeArea", strLanguageToPrint,
                                            token: token)
                                        .ConfigureAwait(false) + ')', token: token)
                                .CheapReplaceAsync(
                                    "MAG",
                                    () => LanguageManager.GetStringAsync(
                                        "String_AttributeMAGShort", strLanguageToPrint,
                                        token: token), token: token).ConfigureAwait(false);
                        }
                    }

                    if (string.IsNullOrEmpty(strDisplayType))
                        strDisplayType = await LanguageManager
                            .GetStringAsync("String_None", strLanguageToPrint, token: token)
                            .ConfigureAwait(false);
                    if (string.IsNullOrEmpty(strDisplayAction))
                        strDisplayAction = await LanguageManager
                            .GetStringAsync("String_None", strLanguageToPrint, token: token)
                            .ConfigureAwait(false);

                    // <critterpower>
                    XmlElementWriteHelper objBaseElement
                        = await objWriter.StartElementAsync("critterpower", token: token).ConfigureAwait(false);
                    try
                    {
                        await objWriter.WriteElementStringAsync("name", strPowerName, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("name_english", strEnglishName, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("extra", sbdExtra.ToString(), token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("category", strCategory, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("category_english", strEnglishCategory, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("type", strDisplayType, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("action", strDisplayAction, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("range", strDisplayRange, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("duration", strDisplayDuration, token: token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync(
                            "source",
                            await CharacterObject.LanguageBookShortAsync(strSource, strLanguageToPrint, token)
                                .ConfigureAwait(false), token: token).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("page", strPage, token: token).ConfigureAwait(false);
                    }
                    finally
                    {
                        // </critterpower>
                        await objBaseElement.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// The Character object being used by the Spirit.
        /// </summary>
        public Character CharacterObject { get; }

        /// <summary>
        /// Name of the Spirit's Metatype.
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
                    if (_strName == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _strName, value) == value)
                            return;
                        _objCachedMyXmlNode = null;
                        _objCachedMyXPathNode = null;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Name of the Spirit's Metatype.
        /// </summary>
        public async Task<string> GetNameAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strName;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Name of the Spirit's Metatype.
        /// </summary>
        public async Task SetNameAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strName == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (Interlocked.Exchange(ref _strName, value) == value)
                        return;
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                    await OnPropertyChangedAsync(nameof(Name), token).ConfigureAwait(false);
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

        /// <summary>
        /// Name of the Spirit.
        /// </summary>
        public string CritterName
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return LinkedCharacter != null ? LinkedCharacter.CharacterName : _strCritterName;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strCritterName, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Name of the Spirit.
        /// </summary>
        public async Task<string> GetCritterNameAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                return objLinkedCharacter != null
                    ? await objLinkedCharacter.GetCharacterNameAsync(token).ConfigureAwait(false)
                    : _strCritterName;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Name of the Spirit.
        /// </summary>
        public async Task SetCritterNameAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strCritterName, value) != value)
                    await OnPropertyChangedAsync(nameof(CritterName), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentDisplayName
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    string strReturn = CritterName;
                    if (string.IsNullOrEmpty(strReturn))
                        strReturn = LanguageManager.TranslateExtra(Name, strPreferFile: "critters.xml");
                    return strReturn;
                }
            }
        }

        public async Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn = await GetCritterNameAsync(token).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strReturn))
                    strReturn = await LanguageManager.TranslateExtraAsync(
                        await GetNameAsync(token).ConfigureAwait(false), strPreferFile: "critters.xml", token: token).ConfigureAwait(false);
                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string RatingLabel
        {
            get
            {
                switch (EntityType)
                {
                    case SpiritType.Spirit:
                        return "String_Force";

                    case SpiritType.Sprite:
                        return "String_Level";

                    default:
                        return "String_Rating";
                }
            }
        }

        public async Task<string> GetRatingLabelAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            switch (await GetEntityTypeAsync(token).ConfigureAwait(false))
            {
                case SpiritType.Spirit:
                    return "String_Force";

                case SpiritType.Sprite:
                    return "String_Level";

                default:
                    return "String_Rating";
            }
        }

        /// <summary>
        /// Number of Services the Spirit owes.
        /// </summary>
        public int ServicesOwed
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intServicesOwed;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_intServicesOwed == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_intServicesOwed == value)
                        return;
                    if (CharacterObject.Created)
                    {
                        if (value > 0 && _intServicesOwed <= 0 && !Bound && !Fettered && CharacterObject.Spirits.Any(
                                x =>
                                    !ReferenceEquals(x, this) && x.EntityType == EntityType && x.ServicesOwed > 0
                                    && !x.Bound &&
                                    !x.Fettered))
                        {
                            // Once created, new sprites/spirits are added as Unbound first. We're not permitted to have more than 1 at a time, but we only count ones that have services.
                            Program.ShowScrollableMessageBox(
                                LanguageManager.GetString(EntityType == SpiritType.Sprite
                                                              ? "Message_UnregisteredSpriteLimit"
                                                              : "Message_UnboundSpiritLimit"),
                                LanguageManager.GetString(EntityType == SpiritType.Sprite
                                                              ? "MessageTitle_UnregisteredSpriteLimit"
                                                              : "MessageTitle_UnboundSpiritLimit"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    else if (!CharacterObject.IgnoreRules)
                    {
                        // Retrieve the character's Summoning Skill Rating.
                        int intSkillValue = CharacterObject.SkillsSection
                                                           .GetActiveSkill(
                                                               EntityType == SpiritType.Spirit
                                                                   ? "Summoning"
                                                                   : "Compiling")?.Rating ?? 0;

                        if (value > intSkillValue)
                        {
                            Program.ShowScrollableMessageBox(
                                LanguageManager.GetString(EntityType == SpiritType.Spirit
                                                              ? "Message_SpiritServices"
                                                              : "Message_SpriteServices"),
                                LanguageManager.GetString(EntityType == SpiritType.Spirit
                                                              ? "MessageTitle_SpiritServices"
                                                              : "MessageTitle_SpriteServices"), MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            value = intSkillValue;
                        }
                    }

                    if (Interlocked.Exchange(ref _intServicesOwed, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of Services the Spirit owes.
        /// </summary>
        public async Task<int> GetServicesOwedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intServicesOwed;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number of Services the Spirit owes.
        /// </summary>
        public async Task SetServicesOwedAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intServicesOwed == value)
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
                if (_intServicesOwed == value)
                    return;
                SpiritType eType = await GetEntityTypeAsync(token).ConfigureAwait(false);
                if (await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false))
                {
                    if (value > 0 && _intServicesOwed <= 0 && !await GetBoundAsync(token).ConfigureAwait(false) &&
                        !await GetFetteredAsync(token).ConfigureAwait(false) && await CharacterObject.Spirits.AnyAsync(
                            async x =>
                                !ReferenceEquals(x, this)
                                && await x.GetEntityTypeAsync(token).ConfigureAwait(false) == eType
                                && await x.GetServicesOwedAsync(token).ConfigureAwait(false) > 0
                                && !await x.GetBoundAsync(token).ConfigureAwait(false)
                                && !await x.GetFetteredAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                    {
                        // Once created, new sprites/spirits are added as Unbound first. We're not permitted to have more than 1 at a time, but we only count ones that have services.
                        await Program.ShowScrollableMessageBoxAsync(
                            await LanguageManager.GetStringAsync(eType == SpiritType.Sprite
                                ? "Message_UnregisteredSpriteLimit"
                                : "Message_UnboundSpiritLimit", token: token).ConfigureAwait(false),
                            await LanguageManager.GetStringAsync(eType == SpiritType.Sprite
                                ? "MessageTitle_UnregisteredSpriteLimit"
                                : "MessageTitle_UnboundSpiritLimit", token: token).ConfigureAwait(false),
                            MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                        return;
                    }
                }
                else if (!await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                {
                    // Retrieve the character's Summoning Skill Rating.
                    Skill objSkill = await (await CharacterObject.GetSkillsSectionAsync(token).ConfigureAwait(false))
                        .GetActiveSkillAsync(
                            eType == SpiritType.Spirit
                                ? "Summoning"
                                : "Compiling", token).ConfigureAwait(false);
                    int intSkillValue =
                        objSkill != null ? await objSkill.GetRatingAsync(token).ConfigureAwait(false) : 0;

                    if (value > intSkillValue)
                    {
                        await Program.ShowScrollableMessageBoxAsync(
                            await LanguageManager.GetStringAsync(eType == SpiritType.Spirit
                                ? "Message_SpiritServices"
                                : "Message_SpriteServices", token: token).ConfigureAwait(false),
                            await LanguageManager.GetStringAsync(eType == SpiritType.Spirit
                                ? "MessageTitle_SpiritServices"
                                : "MessageTitle_SpriteServices", token: token).ConfigureAwait(false), MessageBoxButtons.OK,
                            MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                        value = intSkillValue;
                    }
                }

                if (Interlocked.Exchange(ref _intServicesOwed, value) != value)
                    await OnPropertyChangedAsync(nameof(ServicesOwed), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The Spirit's Force.
        /// </summary>
        public int Force
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intForce;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    switch (EntityType)
                    {
                        case SpiritType.Spirit when value > CharacterObject.MaxSpiritForce:
                            value = CharacterObject.MaxSpiritForce;
                            break;

                        case SpiritType.Sprite when value > CharacterObject.MaxSpriteLevel:
                            value = CharacterObject.MaxSpriteLevel;
                            break;
                    }

                    if (Interlocked.Exchange(ref _intForce, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The Spirit's Force.
        /// </summary>
        public async Task<int> GetForceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intForce;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The Spirit's Force.
        /// </summary>
        public async Task SetForceAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                switch (await GetEntityTypeAsync(token).ConfigureAwait(false))
                {
                    case SpiritType.Spirit:
                        int intMaxForce = await CharacterObject.GetMaxSpiritForceAsync(token).ConfigureAwait(false);
                        if (value > intMaxForce)
                            value = intMaxForce;
                        break;

                    case SpiritType.Sprite:
                        int intMaxLevel = await CharacterObject.GetMaxSpriteLevelAsync(token).ConfigureAwait(false);
                        if (value > intMaxLevel)
                            value = intMaxLevel;
                        break;
                }

                if (Interlocked.Exchange(ref _intForce, value) != value)
                    await OnPropertyChangedAsync(nameof(Force), token: token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the Spirit is Bound.
        /// </summary>
        public bool Bound
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnBound;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnBound == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnBound == value)
                        return;
                    if (CharacterObject.Created && !value && ServicesOwed > 0 && !Fettered
                        && CharacterObject.Spirits.Any(x =>
                                                           !ReferenceEquals(x, this) && x.EntityType == EntityType
                                                           && x.ServicesOwed > 0 && !x.Bound &&
                                                           !x.Fettered))
                    {
                        // Once created, new sprites/spirits are added as Unbound first. We're not permitted to have more than 1 at a time, but we only count ones that have services.
                        Program.ShowScrollableMessageBox(
                            LanguageManager.GetString(EntityType == SpiritType.Sprite
                                                          ? "Message_UnregisteredSpriteLimit"
                                                          : "Message_UnboundSpiritLimit"),
                            LanguageManager.GetString(EntityType == SpiritType.Sprite
                                                          ? "MessageTitle_UnregisteredSpriteLimit"
                                                          : "MessageTitle_UnboundSpiritLimit"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    using (LockObject.EnterWriteLock())
                    {
                        _blnBound = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether the Spirit is Bound.
        /// </summary>
        public async Task<bool> GetBoundAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnBound;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the Spirit is Bound.
        /// </summary>
        public async Task SetBoundAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                if (_blnBound == value)
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
                if (_blnBound == value)
                    return;
                SpiritType eType = await GetEntityTypeAsync(token).ConfigureAwait(false);
                if (await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false) && !value && await GetServicesOwedAsync(token).ConfigureAwait(false) > 0 && !await GetFetteredAsync(token).ConfigureAwait(false)
                    && await CharacterObject.Spirits.AnyAsync(async x =>
                        !ReferenceEquals(x, this)
                        && await x.GetEntityTypeAsync(token).ConfigureAwait(false) == eType
                        && await x.GetServicesOwedAsync(token).ConfigureAwait(false) > 0
                        && !await x.GetBoundAsync(token).ConfigureAwait(false)
                        && !await x.GetFetteredAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                {
                    // Once created, new sprites/spirits are added as Unbound first. We're not permitted to have more than 1 at a time, but we only count ones that have services.
                    await Program.ShowScrollableMessageBoxAsync(
                        await LanguageManager.GetStringAsync(eType == SpiritType.Sprite
                            ? "Message_UnregisteredSpriteLimit"
                            : "Message_UnboundSpiritLimit", token: token).ConfigureAwait(false),
                        await LanguageManager.GetStringAsync(eType == SpiritType.Sprite
                            ? "MessageTitle_UnregisteredSpriteLimit"
                            : "MessageTitle_UnboundSpiritLimit", token: token).ConfigureAwait(false),
                        MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                    return;
                }

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnBound = value;
                    await OnPropertyChangedAsync(nameof(Bound), token: token).ConfigureAwait(false);
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

        /// <summary>
        /// The Spirit's type, either Spirit or Sprite.
        /// </summary>
        public SpiritType EntityType
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _eEntityType;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_eEntityType == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        if (InterlockedExtensions.Exchange(ref _eEntityType, value) == value)
                            return;
                        _objCachedMyXmlNode = null;
                        _objCachedMyXPathNode = null;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// The Spirit's type, either Spirit or Sprite.
        /// </summary>
        public async Task<SpiritType> GetEntityTypeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _eEntityType;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The Spirit's type, either Spirit or Sprite.
        /// </summary>
        public async Task SetEntityTypeAsync(SpiritType value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_eEntityType == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (InterlockedExtensions.Exchange(ref _eEntityType, value) == value)
                        return;
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                    await OnPropertyChangedAsync(nameof(EntityType), token).ConfigureAwait(false);
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

        /// <summary>
        /// Name of the save file for this Spirit/Sprite.
        /// </summary>
        public string FileName
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strFileName;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_strFileName == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _strFileName, value) == value)
                            return;
                        RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Name of the save file for this Spirit/Sprite.
        /// </summary>
        public async Task<string> GetFileNameAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strFileName;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Name of the save file for this Spirit/Sprite.
        /// </summary>
        public async Task SetFileNameAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strFileName == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (Interlocked.Exchange(ref _strFileName, value) == value)
                        return;
                    await RefreshLinkedCharacterAsync(!string.IsNullOrEmpty(value), token).ConfigureAwait(false);
                    await OnPropertyChangedAsync(nameof(FileName), token).ConfigureAwait(false);
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

        /// <summary>
        /// Relative path to the save file.
        /// </summary>
        public string RelativeFileName
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strRelativeName;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_strRelativeName == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _strRelativeName, value) == value)
                            return;
                        RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Relative path to the save file.
        /// </summary>
        public async Task<string> GetRelativeFileNameAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strRelativeName;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Relative path to the save file.
        /// </summary>
        public async Task SetRelativeFileNameAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strRelativeName == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (Interlocked.Exchange(ref _strRelativeName, value) == value)
                        return;
                    await RefreshLinkedCharacterAsync(!string.IsNullOrEmpty(value), token).ConfigureAwait(false);
                    await OnPropertyChangedAsync(nameof(FileName), token).ConfigureAwait(false);
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
                {
                    if (Interlocked.Exchange(ref _strNotes, value) != value)
                        OnPropertyChanged();
                }
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
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                if (Interlocked.Exchange(ref _strNotes, value) == value)
                    return;
                await OnPropertyChangedAsync(nameof(Notes), token).ConfigureAwait(false);
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
                        OnPropertyChanged();
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
                    await OnPropertyChangedAsync(nameof(NotesColor), token).ConfigureAwait(false);
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

        private bool _blnFettered;
        private int _intCachedAllowFettering = int.MinValue;

        public bool AllowFettering
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (_intCachedAllowFettering < 0)
                    {
                        SpiritType eMyType = EntityType;
                        _intCachedAllowFettering = (eMyType == SpiritType.Spirit
                                                    || eMyType == SpiritType.Sprite
                                                    && CharacterObject.AllowSpriteFettering).ToInt32();
                    }

                    return _intCachedAllowFettering > 0;
                }
            }
        }

        public async Task<bool> GetAllowFetteringAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedAllowFettering < 0)
                {
                    SpiritType eMyType = await GetEntityTypeAsync(token).ConfigureAwait(false);
                    _intCachedAllowFettering = (eMyType == SpiritType.Spirit
                                                || eMyType == SpiritType.Sprite
                                                && await CharacterObject.GetAllowSpriteFetteringAsync(token).ConfigureAwait(false)).ToInt32();
                }

                return _intCachedAllowFettering > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the sprite/spirit has unlimited services due to Fettering.
        /// See KC 91 and SG 192 for sprites and spirits, respectively.
        /// </summary>
        public bool Fettered
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (_intCachedAllowFettering < 0)
                        _intCachedAllowFettering = CharacterObject.AllowSpriteFettering.ToInt32();
                    return _blnFettered && _intCachedAllowFettering > 0;
                }
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnFettered == value)
                        return;
                    if (value)
                    {
                        //Technomancers require the Sprite Pet Complex Form to Fetter sprites.
                        if (!CharacterObject.AllowSpriteFettering && EntityType == SpiritType.Sprite)
                            return;

                        //Only one Fettered spirit is permitted.
                        if (CharacterObject.Spirits.Any(objSpirit => objSpirit.Fettered))
                            return;
                    }
                    else if (CharacterObject.Created && !Bound && ServicesOwed > 0 && CharacterObject.Spirits.Any(
                                 x =>
                                     !ReferenceEquals(x, this) && x.EntityType == EntityType && x.ServicesOwed > 0
                                     && !x.Bound &&
                                     !x.Fettered))
                    {
                        // Once created, new sprites/spirits are added as Unbound first. We're not permitted to have more than 1 at a time, but we only count ones that have services.
                        Program.ShowScrollableMessageBox(
                            LanguageManager.GetString(EntityType == SpiritType.Sprite
                                ? "Message_UnregisteredSpriteLimit"
                                : "Message_UnboundSpiritLimit"),
                            LanguageManager.GetString(EntityType == SpiritType.Sprite
                                ? "MessageTitle_UnregisteredSpriteLimit"
                                : "MessageTitle_UnboundSpiritLimit"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnFettered == value)
                        return;
                    int intFetteringCost = 0;
                    if (value)
                    {
                        //Technomancers require the Sprite Pet Complex Form to Fetter sprites.
                        if (!CharacterObject.AllowSpriteFettering && EntityType == SpiritType.Sprite)
                            return;

                        //Only one Fettered spirit is permitted.
                        if (CharacterObject.Spirits.Any(objSpirit => objSpirit.Fettered))
                            return;
                        if (CharacterObject.Created)
                        {
                            // Sprites only cost Force in Karma to become Fettered. Spirits cost Force * 3.
                            intFetteringCost = EntityType == SpiritType.Spirit
                                ? Force * CharacterObject.Settings.KarmaSpiritFettering
                                : Force;
                            if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo,
                                    LanguageManager.GetString(
                                        "Message_ConfirmKarmaExpenseSpend"),
                                    Name,
                                    intFetteringCost.ToString(GlobalSettings.CultureInfo))))
                            {
                                return;
                            }
                        }
                    }
                    else if (CharacterObject.Created && !Bound && ServicesOwed > 0 && CharacterObject.Spirits.Any(
                                 x =>
                                     !ReferenceEquals(x, this) && x.EntityType == EntityType && x.ServicesOwed > 0
                                     && !x.Bound &&
                                     !x.Fettered))
                    {
                        // Once created, new sprites/spirits are added as Unbound first. We're not permitted to have more than 1 at a time, but we only count ones that have services.
                        Program.ShowScrollableMessageBox(
                            LanguageManager.GetString(EntityType == SpiritType.Sprite
                                ? "Message_UnregisteredSpriteLimit"
                                : "Message_UnboundSpiritLimit"),
                            LanguageManager.GetString(EntityType == SpiritType.Sprite
                                ? "MessageTitle_UnregisteredSpriteLimit"
                                : "MessageTitle_UnboundSpiritLimit"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    using (LockObject.EnterWriteLock())
                    {
                        _blnFettered = value;
                        if (value)
                        {
                            if (EntityType == SpiritType.Spirit)
                            {
                                try
                                {
                                    ImprovementManager.CreateImprovement(CharacterObject, "MAG",
                                        Improvement.ImprovementSource.SpiritFettering,
                                        string.Empty,
                                        Improvement.ImprovementType.Attribute,
                                        string.Empty, 0,
                                        1, 0, 0, -1);
                                }
                                catch
                                {
                                    ImprovementManager.Rollback(CharacterObject, CancellationToken.None);
                                    throw;
                                }

                                ImprovementManager.Commit(CharacterObject);
                            }

                            if (CharacterObject.Created)
                            {
                                // Create the Expense Log Entry.
                                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                                objExpense.Create(intFetteringCost * -1,
                                    LanguageManager.GetString("String_ExpenseFetteredSpirit")
                                    + LanguageManager.GetString("String_Space") + Name,
                                    ExpenseType.Karma, DateTime.Now);
                                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                                CharacterObject.Karma -= intFetteringCost;

                                ExpenseUndo objUndo = new ExpenseUndo();
                                objUndo.CreateKarma(KarmaExpenseType.SpiritFettering, InternalId);
                                objExpense.Undo = objUndo;
                            }
                        }
                        else
                        {
                            ImprovementManager.RemoveImprovements(CharacterObject,
                                Improvement.ImprovementSource.SpiritFettering);
                        }
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether the sprite/spirit has unlimited services due to Fettering.
        /// See KC 91 and SG 192 for sprites and spirits, respectively.
        /// </summary>
        public async Task<bool> GetFetteredAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedAllowFettering < 0)
                    _intCachedAllowFettering = (await CharacterObject.GetAllowSpriteFetteringAsync(token).ConfigureAwait(false)).ToInt32();
                return _blnFettered && _intCachedAllowFettering > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the sprite/spirit has unlimited services due to Fettering.
        /// See KC 91 and SG 192 for sprites and spirits, respectively.
        /// </summary>
        public async Task SetFetteredAsync(bool value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                if (_blnFettered == value)
                    return;
                SpiritType eEntityType = await GetEntityTypeAsync(token).ConfigureAwait(false);
                if (value)
                {
                    //Technomancers require the Sprite Pet Complex Form to Fetter sprites.
                    if (!await CharacterObject.GetAllowSpriteFetteringAsync(token).ConfigureAwait(false) &&
                        eEntityType == SpiritType.Sprite)
                        return;

                    //Only one Fettered spirit is permitted.
                    if (await CharacterObject.Spirits
                            .AnyAsync(objSpirit => objSpirit.GetFetteredAsync(token), token: token)
                            .ConfigureAwait(false))
                        return;
                }
                else if (await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false) &&
                         !await GetBoundAsync(token).ConfigureAwait(false) &&
                         await GetServicesOwedAsync(token).ConfigureAwait(false) > 0 && await CharacterObject.Spirits
                             .AnyAsync(
                                 async x =>
                                     !ReferenceEquals(x, this)
                                     && await x.GetEntityTypeAsync(token).ConfigureAwait(false) == eEntityType
                                     && await x.GetServicesOwedAsync(token).ConfigureAwait(false) > 0
                                     && !await x.GetBoundAsync(token).ConfigureAwait(false)
                                     && !await x.GetFetteredAsync(token).ConfigureAwait(false), token: token)
                             .ConfigureAwait(false))
                {
                    // Once created, new sprites/spirits are added as Unbound first. We're not permitted to have more than 1 at a time, but we only count ones that have services.
                    await Program.ShowScrollableMessageBoxAsync(
                        await LanguageManager.GetStringAsync(eEntityType == SpiritType.Sprite
                            ? "Message_UnregisteredSpriteLimit"
                            : "Message_UnboundSpiritLimit", token: token).ConfigureAwait(false),
                        await LanguageManager.GetStringAsync(eEntityType == SpiritType.Sprite
                            ? "MessageTitle_UnregisteredSpriteLimit"
                            : "MessageTitle_UnboundSpiritLimit", token: token).ConfigureAwait(false),
                        MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                    return;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnFettered == value)
                    return;
                int intFetteringCost = 0;
                SpiritType eEntityType = await GetEntityTypeAsync(token).ConfigureAwait(false);
                if (value)
                {
                    //Technomancers require the Sprite Pet Complex Form to Fetter sprites.
                    if (!await CharacterObject.GetAllowSpriteFetteringAsync(token).ConfigureAwait(false) &&
                        eEntityType == SpiritType.Sprite)
                        return;

                    //Only one Fettered spirit is permitted.
                    if (await CharacterObject.Spirits.AnyAsync(objSpirit => objSpirit.GetFetteredAsync(token), token: token)
                            .ConfigureAwait(false))
                        return;
                    if (await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false))
                    {
                        // Sprites only cost Force in Karma to become Fettered. Spirits cost Force * 3.
                        intFetteringCost = eEntityType == SpiritType.Spirit
                            ? await GetForceAsync(token).ConfigureAwait(false) *
                              await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false))
                                  .GetKarmaSpiritFetteringAsync(token).ConfigureAwait(false)
                            : await GetForceAsync(token).ConfigureAwait(false);
                        if (!await CommonFunctions.ConfirmKarmaExpenseAsync(string.Format(GlobalSettings.CultureInfo,
                                await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseSpend", token: token).ConfigureAwait(false),
                                await GetNameAsync(token).ConfigureAwait(false),
                                intFetteringCost.ToString(GlobalSettings.CultureInfo)), token).ConfigureAwait(false))
                        {
                            return;
                        }
                    }
                }
                else if (await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false) && !await GetBoundAsync(token).ConfigureAwait(false) &&
                         await GetServicesOwedAsync(token).ConfigureAwait(false) > 0 && await CharacterObject.Spirits.AnyAsync(
                             async x =>
                                 !ReferenceEquals(x, this)
                                 && await x.GetEntityTypeAsync(token).ConfigureAwait(false) == eEntityType
                                 && await x.GetServicesOwedAsync(token).ConfigureAwait(false) > 0
                                 && !await x.GetBoundAsync(token).ConfigureAwait(false)
                                 && !await x.GetFetteredAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                {
                    // Once created, new sprites/spirits are added as Unbound first. We're not permitted to have more than 1 at a time, but we only count ones that have services.
                    await Program.ShowScrollableMessageBoxAsync(
                        await LanguageManager.GetStringAsync(eEntityType == SpiritType.Sprite
                            ? "Message_UnregisteredSpriteLimit"
                            : "Message_UnboundSpiritLimit", token: token).ConfigureAwait(false),
                        await LanguageManager.GetStringAsync(eEntityType == SpiritType.Sprite
                            ? "MessageTitle_UnregisteredSpriteLimit"
                            : "MessageTitle_UnboundSpiritLimit", token: token).ConfigureAwait(false),
                        MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                    return;
                }

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    _blnFettered = value;
                    if (value)
                    {
                        if (await GetEntityTypeAsync(token).ConfigureAwait(false) == SpiritType.Spirit)
                        {
                            try
                            {
                                await ImprovementManager.CreateImprovementAsync(CharacterObject, "MAG",
                                    Improvement.ImprovementSource.SpiritFettering,
                                    string.Empty,
                                    Improvement.ImprovementType.Attribute,
                                    string.Empty, 0,
                                    1, 0, 0, -1, token: token).ConfigureAwait(false);
                            }
                            catch
                            {
                                await ImprovementManager.RollbackAsync(CharacterObject, CancellationToken.None)
                                    .ConfigureAwait(false);
                                throw;
                            }

                            await ImprovementManager.CommitAsync(CharacterObject, token).ConfigureAwait(false);
                        }

                        if (await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false))
                        {
                            // Create the Expense Log Entry.
                            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                            objExpense.Create(intFetteringCost * -1,
                                await LanguageManager.GetStringAsync("String_ExpenseFetteredSpirit", token: token).ConfigureAwait(false)
                                + await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + await GetNameAsync(token).ConfigureAwait(false),
                                ExpenseType.Karma, DateTime.Now);
                            await CharacterObject.ExpenseEntries.AddWithSortAsync(objExpense, token: token)
                                .ConfigureAwait(false);
                            await CharacterObject.ModifyKarmaAsync(-intFetteringCost, token).ConfigureAwait(false);

                            ExpenseUndo objUndo = new ExpenseUndo();
                            objUndo.CreateKarma(KarmaExpenseType.SpiritFettering, InternalId);
                            objExpense.Undo = objUndo;
                        }
                    }
                    else
                    {
                        await ImprovementManager.RemoveImprovementsAsync(CharacterObject,
                            Improvement.ImprovementSource.SpiritFettering, token: token).ConfigureAwait(false);
                    }
                    await OnPropertyChangedAsync(nameof(Fettered), token).ConfigureAwait(false);
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

        /// <summary>
        /// Color used by the Spirit's control in UI.
        /// Placeholder to prevent me having to deal with multiple interfaces.
        /// </summary>
        public Color PreferredColor
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objColor;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_objColor == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_objColor == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _objColor = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public async Task<Color> GetPreferredColorAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _objColor;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string InternalId => _guiId.ToString("D", GlobalSettings.InvariantCultureInfo);

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ConcurrentHashSet<PropertyChangedAsyncEventHandler> _setPropertyChangedAsync =
            new ConcurrentHashSet<PropertyChangedAsyncEventHandler>();

        public event PropertyChangedAsyncEventHandler PropertyChangedAsync
        {
            add => _setPropertyChangedAsync.TryAdd(value);
            remove => _setPropertyChangedAsync.Remove(value);
        }

        public event MultiplePropertiesChangedEventHandler MultiplePropertiesChanged;

        private readonly ConcurrentHashSet<MultiplePropertiesChangedAsyncEventHandler> _setMultiplePropertiesChangedAsync =
            new ConcurrentHashSet<MultiplePropertiesChangedAsyncEventHandler>();

        public event MultiplePropertiesChangedAsyncEventHandler MultiplePropertiesChangedAsync
        {
            add => _setMultiplePropertiesChangedAsync.TryAdd(value);
            remove => _setMultiplePropertiesChangedAsync.Remove(value);
        }

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        public Task OnPropertyChangedAsync(string strPropertyName, CancellationToken token = default)
        {
            return this.OnMultiplePropertyChangedAsync(token, strPropertyName);
        }

        public void OnMultiplePropertiesChanged(IReadOnlyCollection<string> lstPropertyNames)
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                HashSet<string> setNamesOfChangedProperties = null;
                try
                {
                    foreach (string strPropertyName in lstPropertyNames)
                    {
                        if (setNamesOfChangedProperties == null)
                            setNamesOfChangedProperties
                                = s_SpiritDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                        else
                        {
                            foreach (string strLoopChangedProperty in
                                     s_SpiritDependencyGraph.GetWithAllDependentsEnumerable(this, strPropertyName))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    if (_setMultiplePropertiesChangedAsync.Count > 0)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        List<Func<Task>> lstFuncs = new List<Func<Task>>(_setMultiplePropertiesChangedAsync.Count);
                        foreach (MultiplePropertiesChangedAsyncEventHandler objEvent in _setMultiplePropertiesChangedAsync)
                        {
                            lstFuncs.Add(() => objEvent.Invoke(this, objArgs));
                        }

                        Utils.RunWithoutThreadLock(lstFuncs);
                        if (MultiplePropertiesChanged != null)
                        {
                            Utils.RunOnMainThread(() =>
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                MultiplePropertiesChanged?.Invoke(this, objArgs);
                            });
                        }
                    }
                    else if (MultiplePropertiesChanged != null)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        Utils.RunOnMainThread(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            MultiplePropertiesChanged?.Invoke(this, objArgs);
                        });
                    }

                    if (_setPropertyChangedAsync.Count > 0)
                    {
                        List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties.Select(x => new PropertyChangedEventArgs(x)).ToList();
                        List<Func<Task>> lstFuncs = new List<Func<Task>>(lstArgsList.Count * _setPropertyChangedAsync.Count);
                        foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                        {
                            foreach (PropertyChangedEventArgs objArg in lstArgsList)
                                lstFuncs.Add(() => objEvent.Invoke(this, objArg));
                        }

                        Utils.RunWithoutThreadLock(lstFuncs);
                        if (PropertyChanged != null)
                        {
                            Utils.RunOnMainThread(() =>
                            {
                                if (PropertyChanged != null)
                                {
                                    // ReSharper disable once AccessToModifiedClosure
                                    foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                                    {
                                        PropertyChanged.Invoke(this, objArgs);
                                    }
                                }
                            });
                        }
                    }
                    else if (PropertyChanged != null)
                    {
                        Utils.RunOnMainThread(() =>
                        {
                            if (PropertyChanged != null)
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                foreach (string strPropertyToChange in setNamesOfChangedProperties)
                                {
                                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                                }
                            }
                        });
                    }
                }
                finally
                {
                    if (setNamesOfChangedProperties != null)
                        Utils.StringHashSetPool.Return(ref setNamesOfChangedProperties);
                }
            }
        }

        public async Task OnMultiplePropertiesChangedAsync(IReadOnlyCollection<string> lstPropertyNames,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                HashSet<string> setNamesOfChangedProperties = null;
                try
                {
                    foreach (string strPropertyName in lstPropertyNames)
                    {
                        if (setNamesOfChangedProperties == null)
                            setNamesOfChangedProperties
                                = await s_SpiritDependencyGraph.GetWithAllDependentsAsync(this, strPropertyName, true, token).ConfigureAwait(false);
                        else
                        {
                            foreach (string strLoopChangedProperty in
                                     await s_SpiritDependencyGraph.GetWithAllDependentsEnumerableAsync(this, strPropertyName, token).ConfigureAwait(false))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    if (_setMultiplePropertiesChangedAsync.Count > 0)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (MultiplePropertiesChangedAsyncEventHandler objEvent in _setMultiplePropertiesChangedAsync)
                        {
                            lstTasks.Add(objEvent.Invoke(this, objArgs, token));
                            if (++i < Utils.MaxParallelBatchSize)
                                continue;
                            await Task.WhenAll(lstTasks).ConfigureAwait(false);
                            lstTasks.Clear();
                            i = 0;
                        }

                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        if (MultiplePropertiesChanged != null)
                        {
                            await Utils.RunOnMainThreadAsync(() =>
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                MultiplePropertiesChanged?.Invoke(this, objArgs);
                            }, token: token).ConfigureAwait(false);
                        }
                    }
                    else if (MultiplePropertiesChanged != null)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        await Utils.RunOnMainThreadAsync(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            MultiplePropertiesChanged?.Invoke(this, objArgs);
                        }, token: token).ConfigureAwait(false);
                    }

                    if (_setPropertyChangedAsync.Count > 0)
                    {
                        List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties
                            .Select(x => new PropertyChangedEventArgs(x)).ToList();
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                        {
                            foreach (PropertyChangedEventArgs objArg in lstArgsList)
                            {
                                lstTasks.Add(objEvent.Invoke(this, objArg, token));
                                if (++i < Utils.MaxParallelBatchSize)
                                    continue;
                                await Task.WhenAll(lstTasks).ConfigureAwait(false);
                                lstTasks.Clear();
                                i = 0;
                            }
                        }

                        await Task.WhenAll(lstTasks).ConfigureAwait(false);

                        if (PropertyChanged != null)
                        {
                            await Utils.RunOnMainThreadAsync(() =>
                            {
                                if (PropertyChanged != null)
                                {
                                    // ReSharper disable once AccessToModifiedClosure
                                    foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                                    {
                                        PropertyChanged.Invoke(this, objArgs);
                                    }
                                }
                            }, token).ConfigureAwait(false);
                        }
                    }
                    else if (PropertyChanged != null)
                    {
                        await Utils.RunOnMainThreadAsync(() =>
                        {
                            if (PropertyChanged != null)
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                foreach (string strPropertyToChange in setNamesOfChangedProperties)
                                {
                                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                                }
                            }
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    if (setNamesOfChangedProperties != null)
                        Utils.StringHashSetPool.Return(ref setNamesOfChangedProperties);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static readonly PropertyDependencyGraph<Spirit> s_SpiritDependencyGraph =
            new PropertyDependencyGraph<Spirit>(
                new DependencyGraphNode<string, Spirit>(nameof(NoLinkedCharacter),
                    new DependencyGraphNode<string, Spirit>(nameof(LinkedCharacter))
                ),
                new DependencyGraphNode<string, Spirit>(nameof(CurrentDisplayName),
                    new DependencyGraphNode<string, Spirit>(nameof(CritterName),
                        new DependencyGraphNode<string, Spirit>(nameof(LinkedCharacter))
                    ),
                    new DependencyGraphNode<string, Spirit>(nameof(Name), x => string.IsNullOrEmpty(x.CritterName), async (x, t) => string.IsNullOrEmpty(await x.GetCritterNameAsync(t).ConfigureAwait(false)))
                ),
                new DependencyGraphNode<string, Spirit>(nameof(MainMugshot),
                    new DependencyGraphNode<string, Spirit>(nameof(LinkedCharacter)),
                    new DependencyGraphNode<string, Spirit>(nameof(Mugshots),
                        new DependencyGraphNode<string, Spirit>(nameof(LinkedCharacter))
                    ),
                    new DependencyGraphNode<string, Spirit>(nameof(MainMugshotIndex))
                )
            );

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;
        private Color _objColor;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
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
                objReturn = (blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? CharacterObject.LoadData(EntityType == SpiritType.Spirit ? "traditions.xml" : "streams.xml",
                            strLanguage, token: token)
                        : await CharacterObject.LoadDataAsync(
                            await GetEntityTypeAsync(token).ConfigureAwait(false) == SpiritType.Spirit
                                ? "traditions.xml"
                                : "streams.xml", strLanguage,
                            token: token).ConfigureAwait(false))
                    .TryGetNodeByNameOrId("/chummer/spirits/spirit", blnSync ? Name : await GetNameAsync(token).ConfigureAwait(false));
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
            token.ThrowIfCancellationRequested();
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
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
                objReturn = (blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? CharacterObject.LoadDataXPath(
                            EntityType == SpiritType.Spirit ? "traditions.xml" : "streams.xml",
                            strLanguage, token: token)
                        : await CharacterObject.LoadDataXPathAsync(
                            await GetEntityTypeAsync(token).ConfigureAwait(false) == SpiritType.Spirit ? "traditions.xml" : "streams.xml", strLanguage,
                            token: token).ConfigureAwait(false))
                    .TryGetNodeByNameOrId("/chummer/spirits/spirit", blnSync ? Name : await GetNameAsync(token).ConfigureAwait(false));
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

        public Character LinkedCharacter
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objLinkedCharacter;
            }
        }

        public async Task<Character> GetLinkedCharacterAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _objLinkedCharacter;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool NoLinkedCharacter
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objLinkedCharacter == null;
            }
        }

        public async Task<bool> GetNoLinkedCharacterAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _objLinkedCharacter == null;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void RefreshLinkedCharacter(bool blnShowError = false, CancellationToken token = default)
        {
            using (LockObject.EnterUpgradeableReadLock(token))
            {
                Character objOldLinkedCharacter = _objLinkedCharacter;
                bool blnDoPropertyChanged = false;
                using (LockObject.EnterWriteLock(token))
                {
                    CharacterObject.LinkedCharacters.Remove(_objLinkedCharacter);
                    bool blnError = false;
                    bool blnUseRelative = false;

                    // Make sure the file still exists before attempting to load it.
                    if (!File.Exists(FileName))
                    {
                        // If the file doesn't exist, use the relative path if one is available.
                        if (string.IsNullOrEmpty(RelativeFileName))
                            blnError = true;
                        else if (!File.Exists(Path.GetFullPath(RelativeFileName)))
                            blnError = true;
                        else
                            blnUseRelative = true;

                        if (blnError && blnShowError)
                        {
                            Program.ShowScrollableMessageBox(
                                string.Format(GlobalSettings.CultureInfo,
                                              LanguageManager.GetString("Message_FileNotFound", token: token),
                                              FileName),
                                LanguageManager.GetString("MessageTitle_FileNotFound", token: token), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }

                    if (!blnError)
                    {
                        string strFile = blnUseRelative ? Path.GetFullPath(RelativeFileName) : FileName;
                        if (strFile.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase)
                            || strFile.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                        {
                            if ((_objLinkedCharacter = Program.OpenCharacters.Find(x => x.FileName == strFile)) == null)
                            {
                                using (ThreadSafeForm<LoadingBar> frmLoadingBar
                                       = Program.CreateAndShowProgressBar(strFile, Character.NumLoadingSections))
                                    _objLinkedCharacter
                                        = Program.LoadCharacter(strFile, string.Empty, false, false,
                                                                frmLoadingBar.MyForm, token);
                            }

                            if (_objLinkedCharacter != null)
                                CharacterObject.LinkedCharacters.TryAdd(_objLinkedCharacter);
                        }
                    }

                    if (_objLinkedCharacter != objOldLinkedCharacter)
                    {
                        blnDoPropertyChanged = true;
                        if (objOldLinkedCharacter != null)
                        {
                            if (!objOldLinkedCharacter.IsDisposed)
                            {
                                try
                                {
                                    objOldLinkedCharacter.MultiplePropertiesChangedAsync -= LinkedCharacterOnPropertyChanged;
                                }
                                catch (ObjectDisposedException)
                                {
                                    //swallow this
                                }
                            }

                            if (Program.OpenCharacters.Contains(objOldLinkedCharacter))
                            {
                                if (Program.OpenCharacters.All(x => !x.LinkedCharacters.Contains(objOldLinkedCharacter), token)
                                    && Program.MainForm.OpenFormsWithCharacters.All(
                                        x => !x.CharacterObjects.Contains(objOldLinkedCharacter)))
                                    Program.OpenCharacters.Remove(objOldLinkedCharacter);
                            }
                            else
                                objOldLinkedCharacter.Dispose();
                        }

                        if (_objLinkedCharacter != null)
                        {
                            using (_objLinkedCharacter.LockObject.EnterReadLock(token))
                            {
                                if (string.IsNullOrEmpty(_strCritterName))
                                {
                                    string strCritterName = _objLinkedCharacter.CharacterName;
                                    if (strCritterName !=
                                        LanguageManager.GetString("String_UnnamedCharacter", token: token))
                                        _strCritterName = strCritterName;
                                }

                                _objLinkedCharacter.MultiplePropertiesChangedAsync += LinkedCharacterOnPropertyChanged;
                            }
                        }
                    }
                }

                if (blnDoPropertyChanged)
                    OnPropertyChanged(nameof(LinkedCharacter));
            }
        }

        public async Task RefreshLinkedCharacterAsync(bool blnShowError = false, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objOldLinkedCharacter = _objLinkedCharacter;
                bool blnDoPropertyChanged = false;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    ConcurrentHashSet<Character> lstLinkedCharacters =
                        await CharacterObject.GetLinkedCharactersAsync(token).ConfigureAwait(false);
                    lstLinkedCharacters.Remove(_objLinkedCharacter);
                    bool blnError = false;
                    bool blnUseRelative = false;

                    // Make sure the file still exists before attempting to load it.
                    string strFileName = await GetFileNameAsync(token).ConfigureAwait(false);
                    if (!File.Exists(strFileName))
                    {
                        string strRelativeFileName = await GetRelativeFileNameAsync(token).ConfigureAwait(false);
                        // If the file doesn't exist, use the relative path if one is available.
                        if (string.IsNullOrEmpty(strRelativeFileName))
                            blnError = true;
                        else if (!File.Exists(Path.GetFullPath(strRelativeFileName)))
                            blnError = true;
                        else
                            blnUseRelative = true;

                        if (blnError && blnShowError)
                        {
                            await Program.ShowScrollableMessageBoxAsync(
                                string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Message_FileNotFound", token: token)
                                        .ConfigureAwait(false),
                                    strFileName),
                                await LanguageManager.GetStringAsync("MessageTitle_FileNotFound", token: token)
                                    .ConfigureAwait(false), MessageBoxButtons.OK,
                                MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                        }
                    }

                    if (!blnError)
                    {
                        string strFile = blnUseRelative ? Path.GetFullPath(await GetRelativeFileNameAsync(token).ConfigureAwait(false)) : strFileName;
                        if (strFile.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase)
                            || strFile.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                        {
                            if ((_objLinkedCharacter = await Program.OpenCharacters
                                    .FirstOrDefaultAsync(x => x.FileName == strFile, token: token)
                                    .ConfigureAwait(false)) == null)
                            {
                                using (ThreadSafeForm<LoadingBar> frmLoadingBar
                                       = await Program
                                           .CreateAndShowProgressBarAsync(strFile, Character.NumLoadingSections, token)
                                           .ConfigureAwait(false))
                                    _objLinkedCharacter
                                        = await Program.LoadCharacterAsync(strFile, string.Empty, false, false,
                                            frmLoadingBar.MyForm, token).ConfigureAwait(false);
                            }

                            if (_objLinkedCharacter != null)
                                lstLinkedCharacters.TryAdd(_objLinkedCharacter);
                        }
                    }

                    if (_objLinkedCharacter != objOldLinkedCharacter)
                    {
                        blnDoPropertyChanged = true;
                        if (objOldLinkedCharacter != null)
                        {
                            objOldLinkedCharacter.MultiplePropertiesChangedAsync -= LinkedCharacterOnPropertyChanged;

                            if (await Program.OpenCharacters.ContainsAsync(objOldLinkedCharacter, token)
                                    .ConfigureAwait(false))
                            {
                                if (await Program.OpenCharacters.AllAsync(async x => x == _objLinkedCharacter
                                                                               || !(await x.GetLinkedCharactersAsync(token).ConfigureAwait(false)).Contains(
                                                                                   objOldLinkedCharacter), token: token)
                                        .ConfigureAwait(false)
                                    && Program.MainForm.OpenFormsWithCharacters.All(
                                        x => !x.CharacterObjects.Contains(objOldLinkedCharacter)))
                                    await Program.OpenCharacters.RemoveAsync(objOldLinkedCharacter, token)
                                        .ConfigureAwait(false);
                            }
                            else
                                await objOldLinkedCharacter.DisposeAsync().ConfigureAwait(false);
                        }

                        if (_objLinkedCharacter != null)
                        {
                            IAsyncDisposable objLocker3 = await _objLinkedCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                if (string.IsNullOrEmpty(_strCritterName))
                                {
                                    string strCritterName = await _objLinkedCharacter.GetCharacterNameAsync(token)
                                        .ConfigureAwait(false);
                                    if (strCritterName !=
                                        await LanguageManager.GetStringAsync("String_UnnamedCharacter", token: token)
                                            .ConfigureAwait(false))
                                        _strCritterName = strCritterName;
                                }

                                IAsyncDisposable objLocker4 = await _objLinkedCharacter.LockObject
                                    .EnterWriteLockAsync(token).ConfigureAwait(false);
                                try
                                {
                                    token.ThrowIfCancellationRequested();
                                    _objLinkedCharacter.MultiplePropertiesChangedAsync += LinkedCharacterOnPropertyChanged;
                                }
                                finally
                                {
                                    await objLocker4.DisposeAsync().ConfigureAwait(false);
                                }
                            }
                            finally
                            {
                                await objLocker3.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                if (blnDoPropertyChanged)
                    await OnPropertyChangedAsync(nameof(LinkedCharacter), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private Task LinkedCharacterOnPropertyChanged(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<string> lstProperties = new List<string>();
            if (e.PropertyNames.Contains(nameof(Character.CharacterName)))
                lstProperties.Add(nameof(CritterName));
            if (e.PropertyNames.Contains(nameof(Character.Mugshots)))
                lstProperties.Add(nameof(Mugshots));
            if (e.PropertyNames.Contains(nameof(Character.MainMugshot)))
                lstProperties.Add(nameof(MainMugshot));
            if (e.PropertyNames.Contains(nameof(Character.MainMugshotIndex)))
                lstProperties.Add(nameof(MainMugshotIndex));
            if (e.PropertyNames.Contains(nameof(Character.AllowSpriteFettering)))
            {
                _intCachedAllowFettering = int.MinValue;
                lstProperties.Add(nameof(AllowFettering));
                lstProperties.Add(nameof(Fettered));
            }

            return lstProperties.Count > 0
                ? OnMultiplePropertiesChangedAsync(lstProperties, token)
                : Task.CompletedTask;
        }

        #endregion Properties

        #region IHasMugshots

        /// <summary>
        /// Character's portraits encoded using Base64.
        /// </summary>
        public ThreadSafeList<Image> Mugshots
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return LinkedCharacter != null ? LinkedCharacter.Mugshots : _lstMugshots;
            }
        }

        /// <summary>
        /// Character's portraits encoded using Base64.
        /// </summary>
        public async Task<ThreadSafeList<Image>> GetMugshotsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                    return await objLinkedCharacter.GetMugshotsAsync(token).ConfigureAwait(false);
                return _lstMugshots;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Character's main portrait encoded using Base64.
        /// </summary>
        public Image MainMugshot
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (LinkedCharacter != null)
                        return LinkedCharacter.MainMugshot;
                    if (MainMugshotIndex >= Mugshots.Count || MainMugshotIndex < 0)
                        return null;
                    return Mugshots[MainMugshotIndex];
                }
            }
            set
            {
                if (value == null)
                {
                    MainMugshotIndex = -1;
                    return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (LinkedCharacter != null)
                        LinkedCharacter.MainMugshot = value;
                    else
                    {
                        int intNewMainMugshotIndex = Mugshots.IndexOf(value);
                        if (intNewMainMugshotIndex != -1)
                        {
                            MainMugshotIndex = intNewMainMugshotIndex;
                        }
                        else
                        {
                            using (Mugshots.LockObject.EnterWriteLock())
                            {
                                Mugshots.Add(value);
                                MainMugshotIndex = Mugshots.IndexOf(value);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Character's main portrait encoded using Base64.
        /// </summary>
        public async Task<Image> GetMainMugshotAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                    return await objLinkedCharacter.GetMainMugshotAsync(token).ConfigureAwait(false);
                int intIndex = await GetMainMugshotIndexAsync(token).ConfigureAwait(false);
                ThreadSafeList<Image> lstMugshots = await GetMugshotsAsync(token).ConfigureAwait(false);
                if (intIndex >= await lstMugshots.GetCountAsync(token).ConfigureAwait(false) || intIndex < 0)
                    return null;

                return await lstMugshots.GetValueAtAsync(intIndex, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Character's main portrait encoded using Base64.
        /// </summary>
        public async Task SetMainMugshotAsync(Image value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (value == null)
            {
                await SetMainMugshotIndexAsync(-1, token).ConfigureAwait(false);
                return;
            }
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                {
                    await objLinkedCharacter.SetMainMugshotAsync(value, token).ConfigureAwait(false);
                }
                else
                {
                    ThreadSafeList<Image> lstMugshots = await GetMugshotsAsync(token).ConfigureAwait(false);
                    int intNewMainMugshotIndex = await lstMugshots.IndexOfAsync(value, token).ConfigureAwait(false);
                    if (intNewMainMugshotIndex != -1)
                    {
                        await SetMainMugshotIndexAsync(intNewMainMugshotIndex, token).ConfigureAwait(false);
                    }
                    else
                    {
                        IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            IAsyncDisposable objLocker3 =
                                await lstMugshots.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                await lstMugshots.AddAsync(value, token).ConfigureAwait(false);
                                await SetMainMugshotIndexAsync(await lstMugshots.IndexOfAsync(value, token).ConfigureAwait(false), token).ConfigureAwait(false);
                            }
                            finally
                            {
                                await objLocker3.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Index of Character's main portrait. -1 if set to none.
        /// </summary>
        public int MainMugshotIndex
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return LinkedCharacter?.MainMugshotIndex ?? _intMainMugshotIndex;
            }
            set
            {
                if (value < -1)
                    value = -1;
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (LinkedCharacter != null)
                        LinkedCharacter.MainMugshotIndex = value;
                    else
                    {
                        if (value >= Mugshots.Count)
                            value = -1;

                        if (Interlocked.Exchange(ref _intMainMugshotIndex, value) != value)
                            OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Index of Character's main portrait. -1 if set to none.
        /// </summary>
        public async Task<int> GetMainMugshotIndexAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                    return await objLinkedCharacter.GetMainMugshotIndexAsync(token).ConfigureAwait(false);
                return _intMainMugshotIndex;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Index of Character's main portrait. -1 if set to none.
        /// </summary>
        public async Task SetMainMugshotIndexAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (value < -1)
                value = -1;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                    await objLinkedCharacter.SetMainMugshotIndexAsync(value, token).ConfigureAwait(false);
                else
                {
                    if (value >= await (await GetMugshotsAsync(token).ConfigureAwait(false)).GetCountAsync(token).ConfigureAwait(false))
                        value = -1;
                    if (Interlocked.Exchange(ref _intMainMugshotIndex, value) != value)
                        await OnPropertyChangedAsync(nameof(MainMugshotIndex), token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Index of Character's main portrait. -1 if set to none.
        /// </summary>
        public async Task ModifyMainMugshotIndexAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (value == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                    await objLinkedCharacter.ModifyMainMugshotIndexAsync(value, token).ConfigureAwait(false);
                else
                {
                    int intOldValue = _intMainMugshotIndex;
                    int intNewValue = Interlocked.Add(ref _intMainMugshotIndex, value);
                    if (intNewValue < -1 || intNewValue >= await (await GetMugshotsAsync(token).ConfigureAwait(false)).GetCountAsync(token).ConfigureAwait(false))
                        intNewValue = -1;
                    if (intOldValue != intNewValue)
                        await OnPropertyChangedAsync(nameof(MainMugshotIndex), token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void SaveMugshots(XmlWriter objWriter, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => SaveMugshotsCore(true, objWriter, token), token);
        }

        public Task SaveMugshotsAsync(XmlWriter objWriter, CancellationToken token = default)
        {
            return SaveMugshotsCore(false, objWriter, token);
        }

        public async Task SaveMugshotsCore(bool blnSync, XmlWriter objWriter, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            if (blnSync)
            {
                // ReSharper disable MethodHasAsyncOverload
                // ReSharper disable MethodHasAsyncOverloadWithCancellation
                using (LockObject.EnterReadLock(token))
                {
                    objWriter.WriteElementString("mainmugshotindex",
                                                 MainMugshotIndex.ToString(GlobalSettings.InvariantCultureInfo));
                    // <mugshot>
                    using (objWriter.StartElement("mugshots"))
                    {
                        foreach (Image imgMugshot in Mugshots)
                        {
                            objWriter.WriteElementString(
                                "mugshot", GlobalSettings.ImageToBase64StringForStorage(imgMugshot, token));
                        }

                        // </mugshot>
                    }
                }
                // ReSharper enable MethodHasAsyncOverloadWithCancellation
                // ReSharper enable MethodHasAsyncOverload
            }
            else
            {
                IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    await objWriter.WriteElementStringAsync("mainmugshotindex",
                            (await GetMainMugshotIndexAsync(token).ConfigureAwait(false)).ToString(
                                GlobalSettings.InvariantCultureInfo), token: token)
                        .ConfigureAwait(false);
                    // <mugshots>
                    XmlElementWriteHelper objBaseElement
                        = await objWriter.StartElementAsync("mugshots", token: token).ConfigureAwait(false);
                    try
                    {
                        await (await GetMugshotsAsync(token).ConfigureAwait(false)).ForEachAsync(async imgMugshot =>
                        {
                            await objWriter.WriteElementStringAsync(
                                "mugshot",
                                await GlobalSettings.ImageToBase64StringForStorageAsync(imgMugshot, token)
                                    .ConfigureAwait(false), token: token).ConfigureAwait(false);
                        }, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        // </mugshots>
                        await objBaseElement.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        public void LoadMugshots(XPathNavigator xmlSavedNode, CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                xmlSavedNode.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
                XPathNodeIterator xmlMugshotsList = xmlSavedNode.SelectAndCacheExpression("mugshots/mugshot", token);
                if (xmlMugshotsList.Count > 0)
                {
                    string[] astrMugshotsBase64 = ArrayPool<string>.Shared.Rent(xmlMugshotsList.Count);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        int j = 0;
                        foreach (XPathNavigator objXmlMugshot in xmlMugshotsList)
                        {
                            string strMugshot = objXmlMugshot.Value;
                            if (!string.IsNullOrWhiteSpace(strMugshot))
                                astrMugshotsBase64[j++] = strMugshot;
                            else
                                astrMugshotsBase64[j++] = string.Empty;
                        }

                        if (xmlMugshotsList.Count > 1)
                        {
                            Image[] objMugshotImages = ArrayPool<Image>.Shared.Rent(xmlMugshotsList.Count);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                Parallel.For(0, xmlMugshotsList.Count,
                                             i =>
                                             {
                                                 string strLoop = astrMugshotsBase64[i];
                                                 if (!string.IsNullOrEmpty(strLoop))
                                                     objMugshotImages[i] = strLoop.ToImage(PixelFormat.Format32bppPArgb, token);
                                                 else
                                                     objMugshotImages[i] = null;
                                             });
                                for (int i = 0; i < xmlMugshotsList.Count; ++i)
                                {
                                    Image objLoop = objMugshotImages[i];
                                    if (objLoop != null)
                                        _lstMugshots.Add(objLoop);
                                }
                            }
                            finally
                            {
                                ArrayPool<Image>.Shared.Return(objMugshotImages);
                            }
                        }
                        else
                        {
                            string strLoop = astrMugshotsBase64[0];
                            if (!string.IsNullOrEmpty(strLoop))
                                _lstMugshots.Add(strLoop.ToImage(PixelFormat.Format32bppPArgb, token));
                        }
                    }
                    finally
                    {
                        ArrayPool<string>.Shared.Return(astrMugshotsBase64);
                    }
                }
            }
        }

        public async Task LoadMugshotsAsync(XPathNavigator xmlSavedNode, CancellationToken token = default)
        {
            // Mugshots
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                xmlSavedNode.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
                XPathNodeIterator xmlMugshotsList = xmlSavedNode.SelectAndCacheExpression("mugshots/mugshot", token);
                if (xmlMugshotsList.Count > 0)
                {
                    string[] astrMugshotsBase64 = ArrayPool<string>.Shared.Rent(xmlMugshotsList.Count);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        int j = 0;
                        foreach (XPathNavigator objXmlMugshot in xmlMugshotsList)
                        {
                            string strMugshot = objXmlMugshot.Value;
                            if (!string.IsNullOrWhiteSpace(strMugshot))
                                astrMugshotsBase64[j++] = strMugshot;
                            else
                                astrMugshotsBase64[j++] = string.Empty;
                        }

                        if (xmlMugshotsList.Count > 1)
                        {
                            Task<Bitmap>[] atskMugshotImages = new Task<Bitmap>[xmlMugshotsList.Count];
                            for (int i = 0; i < xmlMugshotsList.Count; ++i)
                            {
                                int iLocal = i;
                                atskMugshotImages[i]
                                    = Task.Run(async () =>
                                    {
                                        string strLoop = astrMugshotsBase64[iLocal];
                                        if (!string.IsNullOrEmpty(strLoop))
                                            return await strLoop.ToImageAsync(PixelFormat.Format32bppPArgb, token).ConfigureAwait(false);
                                        return null;
                                    }, token);
                            }
                            foreach (Bitmap objImage in await Task.WhenAll(atskMugshotImages).ConfigureAwait(false))
                            {
                                if (objImage != null)
                                    await _lstMugshots.AddAsync(objImage, token).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            string strLoop = astrMugshotsBase64[0];
                            if (!string.IsNullOrEmpty(strLoop))
                                await _lstMugshots.AddAsync(await strLoop.ToImageAsync(PixelFormat.Format32bppPArgb, token).ConfigureAwait(false), token).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        ArrayPool<string>.Shared.Return(astrMugshotsBase64);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task PrintMugshots(XmlWriter objWriter, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Character objLinkedCharacter = await GetLinkedCharacterAsync(token).ConfigureAwait(false);
                if (objLinkedCharacter != null)
                    await objLinkedCharacter.PrintMugshots(objWriter, token).ConfigureAwait(false);
                else
                {
                    ThreadSafeList<Image> lstMugshots = await GetMugshotsAsync(token).ConfigureAwait(false);
                    if (await lstMugshots.GetCountAsync(token).ConfigureAwait(false) > 0)
                    {
                        // Since IE is retarded and can't handle base64 images before IE9, we need to dump the image to a temporary directory and re-write the information.
                        // If you give it an extension of jpg, gif, or png, it expects the file to be in that format and won't render the image unless it was originally that type.
                        // But if you give it the extension img, it will render whatever you give it (which doesn't make any damn sense, but that's IE for you).
                        string strMugshotsDirectoryPath = Path.Combine(Utils.GetStartupPath, "mugshots");
                        if (!Directory.Exists(strMugshotsDirectoryPath))
                        {
                            try
                            {
                                Directory.CreateDirectory(strMugshotsDirectoryPath);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                await Program.ShowScrollableMessageBoxAsync(await LanguageManager
                                                                       .GetStringAsync(
                                                                           "Message_Insufficient_Permissions_Warning", token: token)
                                                                       .ConfigureAwait(false), token: token).ConfigureAwait(false);
                            }
                        }

                        Image imgMainMugshot = await GetMainMugshotAsync(token).ConfigureAwait(false);
                        if (imgMainMugshot != null)
                        {
                            // <mainmugshotbase64 />
                            await objWriter
                                  .WriteElementStringAsync("mainmugshotbase64",
                                                           await imgMainMugshot.ToBase64StringAsJpegAsync(token: token)
                                                                               .ConfigureAwait(false), token: token)
                                  .ConfigureAwait(false);
                        }

                        // <hasothermugshots>
                        await objWriter.WriteElementStringAsync("hasothermugshots",
                                                                (imgMainMugshot == null || await lstMugshots.GetCountAsync(token).ConfigureAwait(false) > 1).ToString(
                                                                    GlobalSettings.InvariantCultureInfo), token: token)
                                       .ConfigureAwait(false);
                        // <othermugshots>
                        XmlElementWriteHelper objOtherMugshotsElement
                            = await objWriter.StartElementAsync("othermugshots", token: token).ConfigureAwait(false);
                        try
                        {
                            for (int i = 0; i < await lstMugshots.GetCountAsync(token).ConfigureAwait(false); ++i)
                            {
                                if (i == await GetMainMugshotIndexAsync(token).ConfigureAwait(false))
                                    continue;
                                Image imgMugshot = await lstMugshots.GetValueAtAsync(i, token).ConfigureAwait(false);
                                // <mugshot>
                                XmlElementWriteHelper objMugshotElement
                                    = await objWriter.StartElementAsync("mugshot", token: token).ConfigureAwait(false);
                                try
                                {
                                    await objWriter
                                          .WriteElementStringAsync("stringbase64",
                                                                   await imgMugshot.ToBase64StringAsJpegAsync(token: token)
                                                                                   .ConfigureAwait(false), token: token)
                                          .ConfigureAwait(false);
                                }
                                finally
                                {
                                    // </mugshot>
                                    await objMugshotElement.DisposeAsync().ConfigureAwait(false);
                                }
                            }
                        }
                        finally
                        {
                            // </othermugshots>
                            await objOtherMugshotsElement.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
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
            {
                if (_objLinkedCharacter != null && !Utils.IsUnitTest
                                                && Program.OpenCharacters.All(
                                                    x => x == _objLinkedCharacter
                                                         || !x.LinkedCharacters.Contains(_objLinkedCharacter))
                                                && Program.MainForm.OpenFormsWithCharacters.All(
                                                    x => !x.CharacterObjects.Contains(_objLinkedCharacter)))
                    Program.OpenCharacters.Remove(_objLinkedCharacter);
                foreach (Image imgMugshot in _lstMugshots)
                    imgMugshot.Dispose();
                _lstMugshots.Dispose();
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                if (_objLinkedCharacter != null && !Utils.IsUnitTest
                                                && await Program.OpenCharacters.AllAsync(
                                                                    async x => x == _objLinkedCharacter
                                                                         || !(await x.GetLinkedCharactersAsync().ConfigureAwait(false)).Contains(
                                                                             _objLinkedCharacter))
                                                                .ConfigureAwait(false)
                                                && Program.MainForm.OpenFormsWithCharacters.All(
                                                    x => !x.CharacterObjects.Contains(_objLinkedCharacter)))
                    await Program.OpenCharacters.RemoveAsync(_objLinkedCharacter).ConfigureAwait(false);
                await _lstMugshots.ForEachAsync(x => x.Dispose()).ConfigureAwait(false);
                await _lstMugshots.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion IHasMugshots

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }
    }
}
