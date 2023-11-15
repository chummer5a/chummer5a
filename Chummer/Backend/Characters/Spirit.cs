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

namespace Chummer
{
    /// <summary>
    /// Type of Spirit.
    /// </summary>
    public enum SpiritType
    {
        Spirit = 0,
        Sprite = 1
    }

    /// <summary>
    /// A Magician's Spirit or Technomancer's Sprite.
    /// </summary>
    [DebuggerDisplay("{Name}, \"{CritterName}\"")]
    public sealed class Spirit : IHasInternalId, IHasName, IHasXmlDataNode, IHasMugshots, INotifyMultiplePropertyChangedAsync, IHasNotes, IHasLockObject
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

        private readonly ThreadSafeList<Image> _lstMugshots = new ThreadSafeList<Image>(3);
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
            CharacterObject = objCharacter;
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
                using (LockObject.EnterHiPrioReadLock(token))
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
                    objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
                    objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));

                    SaveMugshots(objWriter, token);
                }
                // ReSharper enable MethodHasAsyncOverload
                // ReSharper enable MethodHasAsyncOverloadWithCancellation
            }
            else
            {
                IAsyncDisposable objLocker = await LockObject.EnterHiPrioReadLockAsync(token).ConfigureAwait(false);
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
                            .WriteElementStringAsync("notes", _strNotes.CleanOfInvalidUnicodeChars(), token: token)
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
                Force = _intForce;
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
        public async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterHiPrioReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Translate the Critter name if applicable.
                string strName = Name;
                XmlNode objXmlCritterNode
                    = await this.GetNodeAsync(strLanguageToPrint, token: token).ConfigureAwait(false);
                if (!strLanguageToPrint.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    strName = objXmlCritterNode?["translate"]?.InnerText ?? Name;
                }

                // <spirit>
                XmlElementWriteHelper objBaseElement
                    = await objWriter.StartElementAsync("spirit", token: token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("guid", InternalId, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name", strName, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name_english", Name, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("crittername", CritterName, token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("fettered", Fettered.ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("bound", Bound.ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("services", ServicesOwed.ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("force", Force.ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("ratinglabel",
                            await LanguageManager
                                .GetStringAsync(RatingLabel, strLanguageToPrint, token: token)
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
                                        = CommonFunctions.EvaluateInvariantXPath(
                                            strInner.Replace(
                                                "F", _intForce.ToString(GlobalSettings.InvariantCultureInfo)), token);
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
                                = await (await _objLinkedCharacter
                                        .LoadDataXPathAsync("spiritpowers.xml", strLanguageToPrint, token: token)
                                        .ConfigureAwait(false))
                                    .SelectSingleNodeAndCacheExpressionAsync("/chummer", token: token)
                                    .ConfigureAwait(false);
                            XPathNavigator xmlCritterPowersBaseChummerNode
                                = await (await _objLinkedCharacter
                                        .LoadDataXPathAsync("critterpowers.xml", strLanguageToPrint,
                                            token: token).ConfigureAwait(false))
                                    .SelectSingleNodeAndCacheExpressionAsync("/chummer", token: token)
                                    .ConfigureAwait(false);

                            XmlNode xmlPowersNode = objXmlCritterNode["powers"];
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
                        .WriteElementStringAsync("bound", Bound.ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("type", EntityType.ToString(), token: token)
                        .ConfigureAwait(false);

                    if (GlobalSettings.PrintNotes)
                        await objWriter.WriteElementStringAsync("notes", Notes, token: token).ConfigureAwait(false);
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

        private async ValueTask PrintPowerInfo(XmlWriter objWriter, XPathNavigator xmlSpiritPowersBaseChummerNode, XPathNavigator xmlCritterPowersBaseChummerNode, XmlNode xmlPowerEntryNode, string strLanguageToPrint = "", CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
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

                        switch ((await objXmlPowerNode.SelectSingleNodeAndCacheExpressionAsync("type", token: token)
                                    .ConfigureAwait(false))?.Value)
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

                        switch ((await objXmlPowerNode.SelectSingleNodeAndCacheExpressionAsync("action", token: token)
                                    .ConfigureAwait(false))?.Value)
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

                        switch ((await objXmlPowerNode.SelectSingleNodeAndCacheExpressionAsync("duration", token: token)
                                    .ConfigureAwait(false))?.Value)
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
                    if (Interlocked.Exchange(ref _strName, value) == value)
                        return;
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                    OnPropertyChanged();
                }
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
        public async ValueTask<string> GetCritterNameAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return LinkedCharacter != null
                    ? await LinkedCharacter.GetCharacterNameAsync(token).ConfigureAwait(false)
                    : _strCritterName;
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

        public async ValueTask<string> GetCurrentDisplayNameAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                string strReturn = await GetCritterNameAsync(token).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strReturn))
                    strReturn = await LanguageManager.TranslateExtraAsync(
                        Name, strPreferFile: "critters.xml", token: token).ConfigureAwait(false);
                return strReturn;
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
        public async ValueTask<int> GetForceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _intForce;
            }
        }

        /// <summary>
        /// The Spirit's Force.
        /// </summary>
        public async ValueTask SetForceAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                switch (EntityType)
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
        }

        /// <summary>
        /// Whether or not the Spirit is Bound.
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
                    if (InterlockedExtensions.Exchange(ref _eEntityType, value) == value)
                        return;
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The Spirit's type, either Spirit or Sprite.
        /// </summary>
        public async ValueTask<SpiritType> GetEntityTypeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _eEntityType;
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
                    if (Interlocked.Exchange(ref _strFileName, value) == value)
                        return;
                    RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                    OnPropertyChanged();
                }
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
                    if (Interlocked.Exchange(ref _strRelativeName, value) == value)
                        return;
                    RefreshLinkedCharacter(!string.IsNullOrEmpty(value));
                    OnPropertyChanged();
                }
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

        private bool _blnFettered;
        private int _intCachedAllowFettering = int.MinValue;

        public bool AllowFettering
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (_intCachedAllowFettering < 0)
                        _intCachedAllowFettering = (EntityType == SpiritType.Spirit
                                                    || EntityType == SpiritType.Sprite
                                                    && CharacterObject.AllowSpriteFettering).ToInt32();
                    return _intCachedAllowFettering > 0;
                }
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

        public string InternalId => _guiId.ToString("D", GlobalSettings.InvariantCultureInfo);

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly List<PropertyChangedAsyncEventHandler> _lstPropertyChangedAsync =
            new List<PropertyChangedAsyncEventHandler>();

        public event PropertyChangedAsyncEventHandler PropertyChangedAsync
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    _lstPropertyChangedAsync.Add(value);
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    _lstPropertyChangedAsync.Remove(value);
            }
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

        public void OnMultiplePropertyChanged(IReadOnlyCollection<string> lstPropertyNames)
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

                    if (_lstPropertyChangedAsync.Count > 0)
                    {
                        List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties
                            .Select(x => new PropertyChangedEventArgs(x)).ToList();
                        Func<Task>[] aFuncs = new Func<Task>[lstArgsList.Count * _lstPropertyChangedAsync.Count];
                        int i = 0;
                        foreach (PropertyChangedAsyncEventHandler objEvent in _lstPropertyChangedAsync)
                        {
                            foreach (PropertyChangedEventArgs objArg in lstArgsList)
                                aFuncs[i++] = () => objEvent.Invoke(this, objArg);
                        }

                        Utils.RunWithoutThreadLock(aFuncs, CancellationToken.None);
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

        public async Task OnMultiplePropertyChangedAsync(IReadOnlyCollection<string> lstPropertyNames,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
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

                    if (_lstPropertyChangedAsync.Count > 0)
                    {
                        List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties
                            .Select(x => new PropertyChangedEventArgs(x)).ToList();
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (PropertyChangedAsyncEventHandler objEvent in _lstPropertyChangedAsync)
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
                    new DependencyGraphNode<string, Spirit>(nameof(Name), x => string.IsNullOrEmpty(x.CritterName))
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
            // ReSharper disable once MethodHasAsyncOverload
            using (blnSync
                       ? LockObject.EnterReadLock(token)
                       : await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
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
                    .TryGetNodeByNameOrId("/chummer/spirits/spirit", Name);
                _objCachedMyXmlNode = objReturn;
                _strCachedXmlNodeLanguage = strLanguage;
                return objReturn;
            }
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            // ReSharper disable once MethodHasAsyncOverload
            using (blnSync ? LockObject.EnterReadLock(token) : await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
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
                    .TryGetNodeByNameOrId("/chummer/spirits/spirit", Name);
                _objCachedMyXPathNode = objReturn;
                _strCachedXPathNodeLanguage = strLanguage;
                return objReturn;
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

        public bool NoLinkedCharacter
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objLinkedCharacter == null;
            }
        }

        public void RefreshLinkedCharacter(bool blnShowError = false, CancellationToken token = default)
        {
            using (LockObject.EnterUpgradeableReadLock(token))
            {
                Character objOldLinkedCharacter = _objLinkedCharacter;
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
                        if (objOldLinkedCharacter != null)
                        {
                            if (!objOldLinkedCharacter.IsDisposed)
                            {
                                try
                                {
                                    using (objOldLinkedCharacter.LockObject.EnterWriteLock(token))
                                        objOldLinkedCharacter.PropertyChanged -= LinkedCharacterOnPropertyChanged;
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
                            using (_objLinkedCharacter.LockObject.EnterUpgradeableReadLock(token))
                            {
                                if (string.IsNullOrEmpty(_strCritterName))
                                {
                                    string strCritterName = _objLinkedCharacter.CharacterName;
                                    if (strCritterName !=
                                        LanguageManager.GetString("String_UnnamedCharacter", token: token))
                                        _strCritterName = strCritterName;
                                }

                                using (_objLinkedCharacter.LockObject.EnterWriteLock(token))
                                    _objLinkedCharacter.PropertyChanged += LinkedCharacterOnPropertyChanged;
                            }
                        }

                        OnPropertyChanged(nameof(LinkedCharacter));
                    }
                }
            }
        }

        public async Task RefreshLinkedCharacterAsync(bool blnShowError = false, CancellationToken token = default)
        {
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                Character objOldLinkedCharacter = _objLinkedCharacter;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
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
                                              await LanguageManager.GetStringAsync("Message_FileNotFound", token: token).ConfigureAwait(false),
                                              FileName),
                                await LanguageManager.GetStringAsync("MessageTitle_FileNotFound", token: token).ConfigureAwait(false), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }

                    if (!blnError)
                    {
                        string strFile = blnUseRelative ? Path.GetFullPath(RelativeFileName) : FileName;
                        if (strFile.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase)
                            || strFile.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                        {
                            if ((_objLinkedCharacter = await Program.OpenCharacters.FirstOrDefaultAsync(x => x.FileName == strFile, token: token).ConfigureAwait(false)) == null)
                            {
                                using (ThreadSafeForm<LoadingBar> frmLoadingBar
                                       = await Program.CreateAndShowProgressBarAsync(strFile, Character.NumLoadingSections, token).ConfigureAwait(false))
                                    _objLinkedCharacter
                                        = await Program.LoadCharacterAsync(strFile, string.Empty, false, false,
                                                                           frmLoadingBar.MyForm, token).ConfigureAwait(false);
                            }

                            if (_objLinkedCharacter != null)
                                CharacterObject.LinkedCharacters.TryAdd(_objLinkedCharacter);
                        }
                    }

                    if (_objLinkedCharacter != objOldLinkedCharacter)
                    {
                        if (objOldLinkedCharacter != null)
                        {
                            IAsyncDisposable objLocker2 = await objOldLinkedCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                objOldLinkedCharacter.PropertyChanged -= LinkedCharacterOnPropertyChanged;
                            }
                            finally
                            {
                                await objLocker2.DisposeAsync().ConfigureAwait(false);
                            }

                            if (await Program.OpenCharacters.ContainsAsync(objOldLinkedCharacter, token).ConfigureAwait(false))
                            {
                                if (await Program.OpenCharacters.AllAsync(x => x == _objLinkedCharacter
                                                                              || !x.LinkedCharacters.Contains(
                                                                                  objOldLinkedCharacter), token: token).ConfigureAwait(false)
                                    && Program.MainForm.OpenFormsWithCharacters.All(
                                        x => !x.CharacterObjects.Contains(objOldLinkedCharacter)))
                                    await Program.OpenCharacters.RemoveAsync(objOldLinkedCharacter, token).ConfigureAwait(false);
                            }
                            else
                                await objOldLinkedCharacter.DisposeAsync().ConfigureAwait(false);
                        }

                        if (_objLinkedCharacter != null)
                        {
                            using (await _objLinkedCharacter.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
                            {
                                token.ThrowIfCancellationRequested();
                                if (string.IsNullOrEmpty(_strCritterName))
                                {
                                    string strCritterName = await _objLinkedCharacter.GetCharacterNameAsync(token).ConfigureAwait(false);
                                    if (strCritterName !=
                                        await LanguageManager.GetStringAsync("String_UnnamedCharacter", token: token).ConfigureAwait(false))
                                        _strCritterName = strCritterName;
                                }

                                IAsyncDisposable objLocker2 = await _objLinkedCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                                try
                                {
                                    token.ThrowIfCancellationRequested();
                                    _objLinkedCharacter.PropertyChanged += LinkedCharacterOnPropertyChanged;
                                }
                                finally
                                {
                                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                                }
                            }
                        }

                        OnPropertyChanged(nameof(LinkedCharacter));
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private void LinkedCharacterOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Character.Name):
                    OnPropertyChanged(nameof(CritterName));
                    break;

                case nameof(Character.Mugshots):
                    OnPropertyChanged(nameof(Mugshots));
                    break;

                case nameof(Character.MainMugshot):
                    OnPropertyChanged(nameof(MainMugshot));
                    break;

                case nameof(Character.MainMugshotIndex):
                    OnPropertyChanged(nameof(MainMugshotIndex));
                    break;

                case nameof(Character.AllowSpriteFettering):
                    _intCachedAllowFettering = int.MinValue;
                    this.OnMultiplePropertyChanged(nameof(AllowFettering), nameof(Fettered));
                    break;
            }
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
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (LinkedCharacter != null)
                        LinkedCharacter.MainMugshot = value;
                    else
                    {
                        if (value == null)
                        {
                            MainMugshotIndex = -1;
                            return;
                        }

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
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (LinkedCharacter != null)
                        LinkedCharacter.MainMugshotIndex = value;
                    else
                    {
                        if (value < -1 || value >= Mugshots.Count)
                            value = -1;

                        if (Interlocked.Exchange(ref _intMainMugshotIndex, value) != value)
                            OnPropertyChanged();
                    }
                }
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
                                "mugshot", GlobalSettings.ImageToBase64StringForStorage(imgMugshot));
                        }

                        // </mugshot>
                    }
                }
                // ReSharper enable MethodHasAsyncOverloadWithCancellation
                // ReSharper enable MethodHasAsyncOverload
            }
            else
            {
                using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    await objWriter.WriteElementStringAsync("mainmugshotindex",
                                                            MainMugshotIndex.ToString(
                                                                GlobalSettings.InvariantCultureInfo), token: token)
                                   .ConfigureAwait(false);
                    // <mugshots>
                    XmlElementWriteHelper objBaseElement
                        = await objWriter.StartElementAsync("mugshots", token: token).ConfigureAwait(false);
                    try
                    {
                        foreach (Image imgMugshot in Mugshots)
                        {
                            await objWriter.WriteElementStringAsync(
                                "mugshot",
                                await GlobalSettings.ImageToBase64StringForStorageAsync(imgMugshot, token)
                                                    .ConfigureAwait(false), token: token).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        // </mugshots>
                        await objBaseElement.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        public void LoadMugshots(XPathNavigator xmlSavedNode, CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                xmlSavedNode.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
                XPathNodeIterator xmlMugshotsList = xmlSavedNode.SelectAndCacheExpression("mugshots/mugshot", token);
                List<string> lstMugshotsBase64 = new List<string>(xmlMugshotsList.Count);
                foreach (XPathNavigator objXmlMugshot in xmlMugshotsList)
                {
                    string strMugshot = objXmlMugshot.Value;
                    if (!string.IsNullOrWhiteSpace(strMugshot))
                    {
                        lstMugshotsBase64.Add(strMugshot);
                    }
                }

                if (lstMugshotsBase64.Count > 1)
                {
                    Image[] objMugshotImages = new Image[lstMugshotsBase64.Count];
                    Parallel.For(0, lstMugshotsBase64.Count,
                                 i => objMugshotImages[i] = lstMugshotsBase64[i].ToImage(PixelFormat.Format32bppPArgb));
                    _lstMugshots.AddRange(objMugshotImages);
                }
                else if (lstMugshotsBase64.Count == 1)
                {
                    _lstMugshots.Add(lstMugshotsBase64[0].ToImage(PixelFormat.Format32bppPArgb));
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
                List<string> lstMugshotsBase64 = new List<string>(xmlMugshotsList.Count);
                foreach (XPathNavigator objXmlMugshot in xmlMugshotsList)
                {
                    string strMugshot = objXmlMugshot.Value;
                    if (!string.IsNullOrWhiteSpace(strMugshot))
                    {
                        lstMugshotsBase64.Add(strMugshot);
                    }
                }

                if (lstMugshotsBase64.Count > 1)
                {
                    Task<Bitmap>[] atskMugshotImages = new Task<Bitmap>[lstMugshotsBase64.Count];
                    for (int i = 0; i < lstMugshotsBase64.Count; ++i)
                    {
                        int iLocal = i;
                        atskMugshotImages[i]
                            = Task.Run(() => lstMugshotsBase64[iLocal].ToImageAsync(PixelFormat.Format32bppPArgb, token).AsTask(), token);
                    }
                    await _lstMugshots.AddRangeAsync(await Task.WhenAll(atskMugshotImages).ConfigureAwait(false), token).ConfigureAwait(false);
                }
                else if (lstMugshotsBase64.Count == 1)
                {
                    await _lstMugshots.AddAsync(await lstMugshotsBase64[0].ToImageAsync(PixelFormat.Format32bppPArgb, token).ConfigureAwait(false), token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async ValueTask PrintMugshots(XmlWriter objWriter, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (LinkedCharacter != null)
                    await LinkedCharacter.PrintMugshots(objWriter, token).ConfigureAwait(false);
                else if (Mugshots.Count > 0)
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
                            Program.ShowScrollableMessageBox(await LanguageManager
                                                                   .GetStringAsync(
                                                                       "Message_Insufficient_Permissions_Warning", token: token)
                                                                   .ConfigureAwait(false));
                        }
                    }

                    Image imgMainMugshot = MainMugshot;
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
                                                            (imgMainMugshot == null || Mugshots.Count > 1).ToString(
                                                                GlobalSettings.InvariantCultureInfo), token: token)
                                   .ConfigureAwait(false);
                    // <othermugshots>
                    XmlElementWriteHelper objOtherMugshotsElement
                        = await objWriter.StartElementAsync("othermugshots", token: token).ConfigureAwait(false);
                    try
                    {
                        for (int i = 0; i < Mugshots.Count; ++i)
                        {
                            if (i == MainMugshotIndex)
                                continue;
                            Image imgMugshot = Mugshots[i];
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
            LockObject.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                if (_objLinkedCharacter != null && !Utils.IsUnitTest
                                                && await Program.OpenCharacters.AllAsync(
                                                                    x => x == _objLinkedCharacter
                                                                         || !x.LinkedCharacters.Contains(
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

            await LockObject.DisposeAsync().ConfigureAwait(false);
        }

        #endregion IHasMugshots

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();
    }
}
