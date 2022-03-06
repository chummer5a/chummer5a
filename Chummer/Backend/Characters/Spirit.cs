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
    public sealed class Spirit : IHasInternalId, IHasName, IHasXmlDataNode, IHasMugshots, INotifyMultiplePropertyChanged, IHasNotes
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

        private readonly ThreadSafeList<Image> _lstMugshots = new ThreadSafeList<Image>(1);
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
        public void Save(XmlWriter objWriter)
        {
            SaveCoreAsync(true, objWriter).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public Task SaveAsync(XmlWriter objWriter)
        {
            return SaveCoreAsync(false, objWriter);
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="blnSync"></param>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        private async Task SaveCoreAsync(bool blnSync, XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            if (blnSync)
            {
                // ReSharper disable MethodHasAsyncOverload
                objWriter.WriteStartElement("spirit");
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
                objWriter.WriteElementString(
                    "notes",
                    System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]",
                                                                 string.Empty));
                objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));

                SaveMugshots(objWriter);

                objWriter.WriteEndElement();
                // ReSharper restore MethodHasAsyncOverload
            }
            else
            {
                await objWriter.WriteStartElementAsync("spirit");
                await objWriter.WriteElementStringAsync(
                    "guid", _guiId.ToString("D", GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync("name", _strName);
                await objWriter.WriteElementStringAsync("crittername", _strCritterName);
                await objWriter.WriteElementStringAsync(
                    "services", _intServicesOwed.ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync(
                    "force", _intForce.ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync(
                    "bound", _blnBound.ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync(
                    "fettered", _blnFettered.ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync("type", _eEntityType.ToString());
                await objWriter.WriteElementStringAsync("file", _strFileName);
                await objWriter.WriteElementStringAsync("relative", _strRelativeName);
                await objWriter.WriteElementStringAsync(
                    "notes",
                    System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]",
                                                                 string.Empty));
                await objWriter.WriteElementStringAsync("notesColor", ColorTranslator.ToHtml(_colNotes));
                await SaveMugshotsAsync(objWriter);
                await objWriter.WriteEndElementAsync();
            }
        }

        /// <summary>
        /// Load the Spirit from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XPathNavigator objNode)
        {
            if (objNode == null)
                return;
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

            RefreshLinkedCharacter(false);

            LoadMugshots(objNode);
        }

        private static readonly ReadOnlyCollection<string> s_PrintAttributeLabels = Array.AsReadOnly(new[]
            {"bod", "agi", "rea", "str", "cha", "int", "wil", "log", "ini"});

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print numbers.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        public async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            // Translate the Critter name if applicable.
            string strName = Name;
            XmlNode objXmlCritterNode = await this.GetNodeAsync(strLanguageToPrint);
            if (!strLanguageToPrint.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strName = objXmlCritterNode?["translate"]?.InnerText ?? Name;
            }

            await objWriter.WriteStartElementAsync("spirit");
            await objWriter.WriteElementStringAsync("guid", InternalId);
            await objWriter.WriteElementStringAsync("name", strName);
            await objWriter.WriteElementStringAsync("name_english", Name);
            await objWriter.WriteElementStringAsync("crittername", CritterName);
            await objWriter.WriteElementStringAsync("fettered", Fettered.ToString(GlobalSettings.InvariantCultureInfo));
            await objWriter.WriteElementStringAsync("bound", Bound.ToString(GlobalSettings.InvariantCultureInfo));
            await objWriter.WriteElementStringAsync("services", ServicesOwed.ToString(objCulture));
            await objWriter.WriteElementStringAsync("force", Force.ToString(objCulture));
            await objWriter.WriteElementStringAsync("ratinglabel", await LanguageManager.GetStringAsync(RatingLabel, strLanguageToPrint));

            if (objXmlCritterNode != null)
            {
                //Attributes for spirits, named differently as to not confuse <attribute>

                Dictionary<string, int> dicAttributes = new Dictionary<string, int>(s_PrintAttributeLabels.Count);
                await objWriter.WriteStartElementAsync("spiritattributes");
                foreach (string strAttribute in s_PrintAttributeLabels)
                {
                    string strInner = string.Empty;
                    if (objXmlCritterNode.TryGetStringFieldQuickly(strAttribute, ref strInner))
                    {
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strInner.Replace("F", _intForce.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                        int intValue = Math.Max(blnIsSuccess ? ((double)objProcess).StandardRound() : _intForce, 1);
                        await objWriter.WriteElementStringAsync(strAttribute, intValue.ToString(objCulture));

                        dicAttributes[strAttribute] = intValue;
                    }
                }

                await objWriter.WriteEndElementAsync();

                if (_objLinkedCharacter != null)
                {
                    //Dump skills, (optional)powers if present to output

                    XPathNavigator xmlSpiritPowersBaseChummerNode = await (await _objLinkedCharacter.LoadDataXPathAsync("spiritpowers.xml", strLanguageToPrint)).SelectSingleNodeAndCacheExpressionAsync("/chummer");
                    XPathNavigator xmlCritterPowersBaseChummerNode = await (await _objLinkedCharacter.LoadDataXPathAsync("critterpowers.xml", strLanguageToPrint)).SelectSingleNodeAndCacheExpressionAsync("/chummer");

                    XmlNode xmlPowersNode = objXmlCritterNode["powers"];
                    if (xmlPowersNode != null)
                    {
                        await objWriter.WriteStartElementAsync("powers");
                        foreach (XmlNode objXmlPowerNode in xmlPowersNode.ChildNodes)
                        {
                            await PrintPowerInfo(objWriter, xmlSpiritPowersBaseChummerNode, xmlCritterPowersBaseChummerNode, objXmlPowerNode, strLanguageToPrint);
                        }
                        await objWriter.WriteEndElementAsync();
                    }

                    xmlPowersNode = objXmlCritterNode["optionalpowers"];
                    if (xmlPowersNode != null)
                    {
                        await objWriter.WriteStartElementAsync("optionalpowers");
                        foreach (XmlNode objXmlPowerNode in xmlPowersNode.ChildNodes)
                        {
                            await PrintPowerInfo(objWriter, xmlSpiritPowersBaseChummerNode, xmlCritterPowersBaseChummerNode, objXmlPowerNode, strLanguageToPrint);
                        }
                        await objWriter.WriteEndElementAsync();
                    }

                    xmlPowersNode = objXmlCritterNode["skills"];
                    if (xmlPowersNode != null)
                    {
                        XPathNavigator xmlSkillsDocument = await CharacterObject.LoadDataXPathAsync("skills.xml", strLanguageToPrint);
                        await objWriter.WriteStartElementAsync("skills");
                        foreach (XmlNode xmlSkillNode in xmlPowersNode.ChildNodes)
                        {
                            string strAttrName = xmlSkillNode.Attributes?["attr"]?.Value ?? string.Empty;
                            if (!dicAttributes.TryGetValue(strAttrName, out int intAttrValue))
                                intAttrValue = _intForce;
                            int intDicepool = intAttrValue + _intForce;

                            string strEnglishName = xmlSkillNode.InnerText;
                            string strTranslatedName = xmlSkillsDocument.SelectSingleNode("/chummer/skills/skill[name = " + strEnglishName.CleanXPath() + "]/translate")?.Value ??
                                                       xmlSkillsDocument.SelectSingleNode("/chummer/knowledgeskills/skill[name = " + strEnglishName.CleanXPath() + "]/translate")?.Value ?? strEnglishName;
                            await objWriter.WriteStartElementAsync("skill");
                            await objWriter.WriteElementStringAsync("name", strTranslatedName);
                            await objWriter.WriteElementStringAsync("name_english", strEnglishName);
                            await objWriter.WriteElementStringAsync("attr", strAttrName);
                            await objWriter.WriteElementStringAsync("pool", intDicepool.ToString(objCulture));
                            await objWriter.WriteEndElementAsync();
                        }
                        await objWriter.WriteEndElementAsync();
                    }

                    xmlPowersNode = objXmlCritterNode["weaknesses"];
                    if (xmlPowersNode != null)
                    {
                        await objWriter.WriteStartElementAsync("weaknesses");
                        foreach (XmlNode objXmlPowerNode in xmlPowersNode.ChildNodes)
                        {
                            await PrintPowerInfo(objWriter, xmlSpiritPowersBaseChummerNode, xmlCritterPowersBaseChummerNode, objXmlPowerNode, strLanguageToPrint);
                        }
                        await objWriter.WriteEndElementAsync();
                    }
                }
                //Page in book for reference
                string strSource = string.Empty;
                string strPage = string.Empty;

                if (objXmlCritterNode.TryGetStringFieldQuickly("source", ref strSource))
                    await objWriter.WriteElementStringAsync("source", await CharacterObject.LanguageBookShortAsync(strSource, strLanguageToPrint));
                if (objXmlCritterNode.TryGetStringFieldQuickly("altpage", ref strPage) || objXmlCritterNode.TryGetStringFieldQuickly("page", ref strPage))
                    await objWriter.WriteElementStringAsync("page", strPage);
            }

            await objWriter.WriteElementStringAsync("bound", Bound.ToString(GlobalSettings.InvariantCultureInfo));
            await objWriter.WriteElementStringAsync("type", EntityType.ToString());

            if (GlobalSettings.PrintNotes)
                await objWriter.WriteElementStringAsync("notes", Notes);
            await PrintMugshots(objWriter);
            await objWriter.WriteEndElementAsync();
        }

        private async ValueTask PrintPowerInfo(XmlWriter objWriter, XPathNavigator xmlSpiritPowersBaseChummerNode, XPathNavigator xmlCritterPowersBaseChummerNode, XmlNode xmlPowerEntryNode, string strLanguageToPrint = "")
        {
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdExtra))
            {
                string strSelect = xmlPowerEntryNode.SelectSingleNode("@select")?.Value;
                if (!string.IsNullOrEmpty(strSelect))
                    sbdExtra.Append(await CharacterObject.TranslateExtraAsync(strSelect, strLanguageToPrint));
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
                    string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguageToPrint);
                    bool blnExtrasAdded = false;
                    foreach (string strLoopExtra in strPowerName.TrimStartOnce(strEnglishName).Trim().TrimStartOnce('(')
                                                                .TrimEndOnce(')')
                                                                .SplitNoAlloc(
                                                                    ',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        blnExtrasAdded = true;
                        sbdExtra.Append(await CharacterObject.TranslateExtraAsync(strLoopExtra, strLanguageToPrint)).Append(',')
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

                    switch ((await objXmlPowerNode.SelectSingleNodeAndCacheExpressionAsync("type"))?.Value)
                    {
                        case "M":
                            strDisplayType = await LanguageManager.GetStringAsync("String_SpellTypeMana", strLanguageToPrint);
                            break;

                        case "P":
                            strDisplayType = await LanguageManager.GetStringAsync("String_SpellTypePhysical", strLanguageToPrint);
                            break;
                    }

                    switch ((await objXmlPowerNode.SelectSingleNodeAndCacheExpressionAsync("action"))?.Value)
                    {
                        case "Auto":
                            strDisplayAction = await LanguageManager.GetStringAsync("String_ActionAutomatic", strLanguageToPrint);
                            break;

                        case "Free":
                            strDisplayAction = await LanguageManager.GetStringAsync("String_ActionFree", strLanguageToPrint);
                            break;

                        case "Simple":
                            strDisplayAction = await LanguageManager.GetStringAsync("String_ActionSimple", strLanguageToPrint);
                            break;

                        case "Complex":
                            strDisplayAction = await LanguageManager.GetStringAsync("String_ActionComplex", strLanguageToPrint);
                            break;

                        case "Special":
                            strDisplayAction
                                = await LanguageManager.GetStringAsync("String_SpellDurationSpecial", strLanguageToPrint);
                            break;
                    }

                    switch ((await objXmlPowerNode.SelectSingleNodeAndCacheExpressionAsync("duration"))?.Value)
                    {
                        case "Instant":
                            strDisplayDuration
                                = await LanguageManager.GetStringAsync("String_SpellDurationInstantLong", strLanguageToPrint);
                            break;

                        case "Sustained":
                            strDisplayDuration
                                = await LanguageManager.GetStringAsync("String_SpellDurationSustained", strLanguageToPrint);
                            break;

                        case "Always":
                            strDisplayDuration
                                = await LanguageManager.GetStringAsync("String_SpellDurationAlways", strLanguageToPrint);
                            break;

                        case "Special":
                            strDisplayDuration
                                = await LanguageManager.GetStringAsync("String_SpellDurationSpecial", strLanguageToPrint);
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
                                    "String_SpellRangeSelf", strLanguageToPrint))
                            .CheapReplaceAsync(
                                "Special",
                                () => LanguageManager.GetStringAsync(
                                    "String_SpellDurationSpecial", strLanguageToPrint))
                            .CheapReplaceAsync(
                                "LOS",
                                () => LanguageManager.GetStringAsync(
                                    "String_SpellRangeLineOfSight", strLanguageToPrint))
                            .CheapReplaceAsync(
                                "LOI",
                                () => LanguageManager.GetStringAsync(
                                    "String_SpellRangeLineOfInfluence", strLanguageToPrint))
                            .CheapReplaceAsync(
                                "Touch",
                                () => LanguageManager.GetStringAsync(
                                    "String_SpellRangeTouch",
                                    strLanguageToPrint)) // Short form to remain export-friendly
                            .CheapReplaceAsync(
                                "T",
                                () => LanguageManager.GetStringAsync(
                                    "String_SpellRangeTouch", strLanguageToPrint))
                            .CheapReplaceAsync(
                                "(A)",
                                async () => '(' + await LanguageManager.GetStringAsync(
                                    "String_SpellRangeArea", strLanguageToPrint) + ')')
                            .CheapReplaceAsync(
                                "MAG",
                                () => LanguageManager.GetStringAsync(
                                    "String_AttributeMAGShort", strLanguageToPrint));
                    }
                }

                if (string.IsNullOrEmpty(strDisplayType))
                    strDisplayType = await LanguageManager.GetStringAsync("String_None", strLanguageToPrint);
                if (string.IsNullOrEmpty(strDisplayAction))
                    strDisplayAction = await LanguageManager.GetStringAsync("String_None", strLanguageToPrint);

                await objWriter.WriteStartElementAsync("critterpower");
                await objWriter.WriteElementStringAsync("name", strPowerName);
                await objWriter.WriteElementStringAsync("name_english", strEnglishName);
                await objWriter.WriteElementStringAsync("extra", sbdExtra.ToString());
                await objWriter.WriteElementStringAsync("category", strCategory);
                await objWriter.WriteElementStringAsync("category_english", strEnglishCategory);
                await objWriter.WriteElementStringAsync("type", strDisplayType);
                await objWriter.WriteElementStringAsync("action", strDisplayAction);
                await objWriter.WriteElementStringAsync("range", strDisplayRange);
                await objWriter.WriteElementStringAsync("duration", strDisplayDuration);
                await objWriter.WriteElementStringAsync(
                    "source", await CharacterObject.LanguageBookShortAsync(strSource, strLanguageToPrint));
                await objWriter.WriteElementStringAsync("page", strPage);
                await objWriter.WriteEndElementAsync();
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
            get => _strName;
            set
            {
                if (_strName != value)
                {
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                    _strName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Name of the Spirit.
        /// </summary>
        public string CritterName
        {
            get => LinkedCharacter != null ? LinkedCharacter.CharacterName : _strCritterName;
            set
            {
                if (_strCritterName != value)
                {
                    _strCritterName = value;
                    OnPropertyChanged();
                }
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
            get => _intServicesOwed;
            set
            {
                if (_intServicesOwed == value)
                    return;
                if (CharacterObject.Created)
                {
                    if (value > 0 && _intServicesOwed <= 0 && !Bound && !Fettered && CharacterObject.Spirits.Any(x =>
                        !ReferenceEquals(x, this) && x.EntityType == EntityType && x.ServicesOwed > 0 && !x.Bound &&
                        !x.Fettered))
                    {
                        // Once created, new sprites/spirits are added as Unbound first. We're not permitted to have more than 1 at a time, but we only count ones that have services.
                        Program.ShowMessageBox(
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
                    int intSkillValue = CharacterObject.SkillsSection.GetActiveSkill(EntityType == SpiritType.Spirit ? "Summoning" : "Compiling")?.Rating ?? 0;

                    if (value > intSkillValue)
                    {
                        Program.ShowMessageBox(
                            LanguageManager.GetString(EntityType == SpiritType.Spirit
                                ? "Message_SpiritServices"
                                : "Message_SpriteServices"),
                            LanguageManager.GetString(EntityType == SpiritType.Spirit
                                ? "MessageTitle_SpiritServices"
                                : "MessageTitle_SpriteServices"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        value = intSkillValue;
                    }
                }
                if (_intServicesOwed != value)
                {
                    _intServicesOwed = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The Spirit's Force.
        /// </summary>
        public int Force
        {
            get => _intForce;
            set
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

                if (_intForce == value) return;
                _intForce = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether or not the Spirit is Bound.
        /// </summary>
        public bool Bound
        {
            get => _blnBound;
            set
            {
                if (_blnBound == value)
                    return;
                if (CharacterObject.Created && !value && ServicesOwed > 0 && !Fettered && CharacterObject.Spirits.Any(x =>
                    !ReferenceEquals(x, this) && x.EntityType == EntityType && x.ServicesOwed > 0 && !x.Bound &&
                    !x.Fettered))
                {
                    // Once created, new sprites/spirits are added as Unbound first. We're not permitted to have more than 1 at a time, but we only count ones that have services.
                    Program.ShowMessageBox(
                        LanguageManager.GetString(EntityType == SpiritType.Sprite
                            ? "Message_UnregisteredSpriteLimit"
                            : "Message_UnboundSpiritLimit"),
                        LanguageManager.GetString(EntityType == SpiritType.Sprite
                            ? "MessageTitle_UnregisteredSpriteLimit"
                            : "MessageTitle_UnboundSpiritLimit"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                _blnBound = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The Spirit's type, either Spirit or Sprite.
        /// </summary>
        public SpiritType EntityType
        {
            get => _eEntityType;
            set
            {
                if (_eEntityType != value)
                {
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                    _eEntityType = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Name of the save file for this Spirit/Sprite.
        /// </summary>
        public string FileName
        {
            get => _strFileName;
            set
            {
                if (_strFileName != value)
                {
                    _strFileName = value;
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
            get => _strRelativeName;
            set
            {
                if (_strRelativeName != value)
                {
                    _strRelativeName = value;
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
            get => _strNotes;
            set
            {
                if (_strNotes != value)
                {
                    _strNotes = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get => _colNotes;
            set => _colNotes = value;
        }

        private bool _blnFettered;
        private int _intCachedAllowFettering = int.MinValue;

        public bool AllowFettering
        {
            get
            {
                if (_intCachedAllowFettering < 0)
                    _intCachedAllowFettering = EntityType == SpiritType.Spirit
                                               || EntityType == SpiritType.Sprite
                                               && CharacterObject.AllowSpriteFettering
                        ? 1
                        : 0;
                return _intCachedAllowFettering > 0;
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
                if (_intCachedAllowFettering < 0)
                    _intCachedAllowFettering = CharacterObject.AllowSpriteFettering
                        ? 1
                        : 0;
                return _blnFettered && _intCachedAllowFettering > 0;
            }

            set
            {
                if (_blnFettered == value)
                    return;
                if (value)
                {
                    //Technomancers require the Sprite Pet Complex Form to Fetter sprites.
                    if (!CharacterObject.AllowSpriteFettering && EntityType == SpiritType.Sprite) return;

                    //Only one Fettered spirit is permitted.
                    if (CharacterObject.Spirits.Any(objSpirit => objSpirit.Fettered)) return;

                    if (CharacterObject.Created)
                    {
                        // Sprites only cost Force in Karma to become Fettered. Spirits cost Force * 3.
                        int fetteringCost = EntityType == SpiritType.Spirit ? Force * CharacterObject.Settings.KarmaSpiritFettering : Force;
                        if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend")
                            , Name
                            , fetteringCost.ToString(GlobalSettings.CultureInfo))))
                        {
                            return;
                        }

                        // Create the Expense Log Entry.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        objExpense.Create(fetteringCost * -1,
                            LanguageManager.GetString("String_ExpenseFetteredSpirit") + LanguageManager.GetString("String_Space") + Name,
                            ExpenseType.Karma, DateTime.Now);
                        CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                        CharacterObject.Karma -= fetteringCost;

                        ExpenseUndo objUndo = new ExpenseUndo();
                        objUndo.CreateKarma(KarmaExpenseType.SpiritFettering, InternalId);
                        objExpense.Undo = objUndo;
                    }

                    if (EntityType == SpiritType.Spirit)
                    {
                        ImprovementManager.CreateImprovement(CharacterObject, "MAG",
                            Improvement.ImprovementSource.SpiritFettering, string.Empty,
                            Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -1);
                        ImprovementManager.Commit(CharacterObject);
                    }
                }
                else
                {
                    if (CharacterObject.Created && !Bound && ServicesOwed > 0 && CharacterObject.Spirits.Any(x =>
                        !ReferenceEquals(x, this) && x.EntityType == EntityType && x.ServicesOwed > 0 && !x.Bound &&
                        !x.Fettered))
                    {
                        // Once created, new sprites/spirits are added as Unbound first. We're not permitted to have more than 1 at a time, but we only count ones that have services.
                        Program.ShowMessageBox(
                            LanguageManager.GetString(EntityType == SpiritType.Sprite
                                ? "Message_UnregisteredSpriteLimit"
                                : "Message_UnboundSpiritLimit"),
                            LanguageManager.GetString(EntityType == SpiritType.Sprite
                                ? "MessageTitle_UnregisteredSpriteLimit"
                                : "MessageTitle_UnboundSpiritLimit"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.SpiritFettering);
                }
                _blnFettered = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Color used by the Spirit's control in UI.
        /// Placeholder to prevent me having to deal with multiple interfaces.
        /// </summary>
        public Color PreferredColor
        {
            get => _objColour;
            set
            {
                if (_objColour != value)
                {
                    _objColour = value;
                    OnPropertyChanged();
                }
            }
        }

        public string InternalId => _guiId.ToString("D", GlobalSettings.InvariantCultureInfo);

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(IReadOnlyCollection<string> lstPropertyNames)
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

                foreach (string strPropertyToChange in setNamesOfChangedProperties)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                }
            }
            finally
            {
                if (setNamesOfChangedProperties != null)
                    Utils.StringHashSetPool.Return(setNamesOfChangedProperties);
            }
        }

        private static readonly PropertyDependencyGraph<Spirit> s_SpiritDependencyGraph =
            new PropertyDependencyGraph<Spirit>(
                new DependencyGraphNode<string, Spirit>(nameof(NoLinkedCharacter),
                    new DependencyGraphNode<string, Spirit>(nameof(LinkedCharacter))
                ),
                new DependencyGraphNode<string, Spirit>(nameof(CritterName),
                    new DependencyGraphNode<string, Spirit>(nameof(LinkedCharacter))
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
        private Color _objColour;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage)
        {
            if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage
                                            && !GlobalSettings.LiveCustomData)
                return _objCachedMyXmlNode;
            _objCachedMyXmlNode = (blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? CharacterObject.LoadData(_eEntityType == SpiritType.Spirit ? "traditions.xml" : "streams.xml",
                                               strLanguage)
                    : await CharacterObject.LoadDataAsync(
                        _eEntityType == SpiritType.Spirit ? "traditions.xml" : "streams.xml", strLanguage))
                .SelectSingleNode("/chummer/spirits/spirit[name = " + Name.CleanXPath() + ']');
            _strCachedXmlNodeLanguage = strLanguage;
            return _objCachedMyXmlNode;
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage)
        {
            if (_objCachedMyXPathNode != null && strLanguage == _strCachedXPathNodeLanguage
                                              && !GlobalSettings.LiveCustomData)
                return _objCachedMyXPathNode;
            _objCachedMyXPathNode = (blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? CharacterObject.LoadDataXPath(
                        _eEntityType == SpiritType.Spirit ? "traditions.xml" : "streams.xml",
                        strLanguage)
                    : await CharacterObject.LoadDataXPathAsync(
                        _eEntityType == SpiritType.Spirit ? "traditions.xml" : "streams.xml", strLanguage))
                .SelectSingleNode("/chummer/spirits/spirit[name = " + Name.CleanXPath() + ']');
            _strCachedXPathNodeLanguage = strLanguage;
            return _objCachedMyXPathNode;
        }

        public Character LinkedCharacter => _objLinkedCharacter;

        public bool NoLinkedCharacter => _objLinkedCharacter == null;

        public void RefreshLinkedCharacter(bool blnShowError)
        {
            Character objOldLinkedCharacter = _objLinkedCharacter;
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
                    Program.ShowMessageBox(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_FileNotFound"), FileName),
                        LanguageManager.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (!blnError)
            {
                string strFile = blnUseRelative ? Path.GetFullPath(RelativeFileName) : FileName;
                if (strFile.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
                {
                    Character objOpenCharacter = Program.OpenCharacters.FirstOrDefault(x => x.FileName == strFile);
                    _objLinkedCharacter = objOpenCharacter ?? Program.LoadCharacter(strFile, string.Empty, false, false);
                    if (_objLinkedCharacter != null)
                        CharacterObject.LinkedCharacters.Add(_objLinkedCharacter);
                }
            }
            if (_objLinkedCharacter != objOldLinkedCharacter)
            {
                if (objOldLinkedCharacter != null)
                {
                    objOldLinkedCharacter.PropertyChanged -= LinkedCharacterOnPropertyChanged;
                    if (Program.OpenCharacters.Contains(objOldLinkedCharacter))
                    {
                        if (Program.OpenCharacters.All(x => !x.LinkedCharacters.Contains(objOldLinkedCharacter))
                            && Program.MainForm.OpenCharacterForms.All(x => x.CharacterObject != objOldLinkedCharacter))
                            Program.OpenCharacters.Remove(objOldLinkedCharacter);
                    }
                    else
                        objOldLinkedCharacter.Dispose();
                }
                if (_objLinkedCharacter != null)
                {
                    if (string.IsNullOrEmpty(_strCritterName) && CritterName != LanguageManager.GetString("String_UnnamedCharacter"))
                        _strCritterName = CritterName;

                    _objLinkedCharacter.PropertyChanged += LinkedCharacterOnPropertyChanged;
                }
                OnPropertyChanged(nameof(LinkedCharacter));
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
                    OnPropertyChanged(nameof(AllowFettering));
                    OnPropertyChanged(nameof(Fettered));
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
                using (EnterReadLock.Enter(CharacterObject.LockObject))
                {
                    return LinkedCharacter != null ? LinkedCharacter.Mugshots : _lstMugshots;
                }
            }
        }

        /// <summary>
        /// Character's main portrait encoded using Base64.
        /// </summary>
        public Image MainMugshot
        {
            get
            {
                if (LinkedCharacter != null)
                    return LinkedCharacter.MainMugshot;
                if (MainMugshotIndex >= Mugshots.Count || MainMugshotIndex < 0)
                    return null;
                return Mugshots[MainMugshotIndex];
            }
            set
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
                        Mugshots.Add(value);
                        MainMugshotIndex = Mugshots.Count - 1;
                    }
                }
            }
        }

        /// <summary>
        /// Index of Character's main portrait. -1 if set to none.
        /// </summary>
        public int MainMugshotIndex
        {
            get => LinkedCharacter?.MainMugshotIndex ?? _intMainMugshotIndex;
            set
            {
                if (LinkedCharacter != null)
                    LinkedCharacter.MainMugshotIndex = value;
                else
                {
                    if (value < -1)
                        value = -1;
                    else if (value >= 0)
                    {
                        using (EnterReadLock.Enter(CharacterObject.LockObject))
                        {
                            if (value >= Mugshots.Count)
                                value = -1;
                        }
                    }

                    using (EnterReadLock.Enter(CharacterObject.LockObject))
                    {
                        if (_intMainMugshotIndex == value)
                            return;
                        using (CharacterObject.LockObject.EnterWriteLock())
                        {
                            _intMainMugshotIndex = value;
                            OnPropertyChanged();
                        }
                    }
                }
            }
        }

        public void SaveMugshots(XmlWriter objWriter)
        {
            SaveMugshotsCore(true, objWriter).GetAwaiter().GetResult();
        }

        public Task SaveMugshotsAsync(XmlWriter objWriter)
        {
            return SaveMugshotsCore(false, objWriter);
        }

        public async Task SaveMugshotsCore(bool blnSync, XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            if (blnSync)
            {
                objWriter.WriteElementString("mainmugshotindex", MainMugshotIndex.ToString(GlobalSettings.InvariantCultureInfo));
                // <mugshot>
                objWriter.WriteStartElement("mugshots");
                foreach (Image imgMugshot in Mugshots)
                {
                    // ReSharper disable once MethodHasAsyncOverload
                    objWriter.WriteElementString("mugshot", GlobalSettings.ImageToBase64StringForStorage(imgMugshot));
                }

                // </mugshot>
                // ReSharper disable once MethodHasAsyncOverload
                objWriter.WriteEndElement();
            }
            else
            {
                await objWriter.WriteElementStringAsync("mainmugshotindex",
                                                        MainMugshotIndex.ToString(GlobalSettings.InvariantCultureInfo));
                // <mugshot>
                await objWriter.WriteStartElementAsync("mugshots");
                foreach (Image imgMugshot in Mugshots)
                {
                    await objWriter.WriteElementStringAsync(
                        "mugshot", await GlobalSettings.ImageToBase64StringForStorageAsync(imgMugshot));
                }

                // </mugshot>
                await objWriter.WriteEndElementAsync();
            }
        }

        public void LoadMugshots(XPathNavigator xmlSavedNode)
        {
            xmlSavedNode.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
            XPathNodeIterator xmlMugshotsList = xmlSavedNode.SelectAndCacheExpression("mugshots/mugshot");
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

        public async ValueTask PrintMugshots(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            if (LinkedCharacter != null)
                await LinkedCharacter.PrintMugshots(objWriter);
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
                        Program.ShowMessageBox(await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning"));
                    }
                }
                Guid guiImage = Guid.NewGuid();
                Image imgMainMugshot = MainMugshot;
                if (imgMainMugshot != null)
                {
                    string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath,
                        guiImage.ToString("N", GlobalSettings.InvariantCultureInfo) + ".jpg");
                    imgMainMugshot.Save(imgMugshotPath);
                    // <mainmugshotpath />
                    await objWriter.WriteElementStringAsync("mainmugshotpath",
                        "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));
                    // <mainmugshotbase64 />
                    await objWriter.WriteElementStringAsync("mainmugshotbase64", await imgMainMugshot.ToBase64StringAsJpegAsync());
                }
                // <othermugshots>
                await objWriter.WriteElementStringAsync("hasothermugshots",
                    (imgMainMugshot == null || Mugshots.Count > 1).ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteStartElementAsync("othermugshots");
                for (int i = 0; i < Mugshots.Count; ++i)
                {
                    if (i == MainMugshotIndex)
                        continue;
                    Image imgMugshot = Mugshots[i];
                    await objWriter.WriteStartElementAsync("mugshot");

                    await objWriter.WriteElementStringAsync("stringbase64", await imgMugshot.ToBase64StringAsJpegAsync());

                    string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath,
                        guiImage.ToString("N", GlobalSettings.InvariantCultureInfo) +
                        i.ToString(GlobalSettings.InvariantCultureInfo) + ".jpg");
                    imgMugshot.Save(imgMugshotPath);
                    await objWriter.WriteElementStringAsync("temppath",
                        "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));

                    await objWriter.WriteEndElementAsync();
                }
                // </mugshots>
                await objWriter.WriteEndElementAsync();
            }
        }

        public void Dispose()
        {
            if (_objLinkedCharacter != null && !Utils.IsUnitTest
                                            && Program.OpenCharacters.Contains(_objLinkedCharacter)
                                            && Program.OpenCharacters.All(x => !x.LinkedCharacters.Contains(_objLinkedCharacter))
                                            && Program.MainForm.OpenCharacterForms.All(x => x.CharacterObject != _objLinkedCharacter))
                Program.OpenCharacters.Remove(_objLinkedCharacter);
            foreach (Image imgMugshot in _lstMugshots)
                imgMugshot.Dispose();
            _lstMugshots.Dispose();
        }

        #endregion IHasMugshots
    }
}
