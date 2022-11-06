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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;

namespace Chummer.Backend.Skills
{
    [DebuggerDisplay("{_strName} {_intBase} {_intKarma} {Rating}")]
    [HubClassTag("SkillId", true, "Name", "Rating;Specialization")]
    public class Skill : INotifyMultiplePropertyChanged, IHasName, IHasXmlDataNode, IHasNotes, IHasLockObject
    {
        private CharacterAttrib _objAttribute;
        private string _strDefaultAttribute;
        private bool _blnRequiresGroundMovement;
        private bool _blnRequiresSwimMovement;
        private bool _blnRequiresFlyMovement;
        private int _intBase;
        private int _intKarma;
        private bool _blnBuyWithKarma;

        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        public CharacterAttrib AttributeObject
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _objAttribute;
            }
            private set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_objAttribute == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _objAttribute, value) != value)
                            OnPropertyChanged();
                    }
                }
            }
        }

        private void RecacheAttribute()
        {
            using (LockObject.EnterWriteLock())
            {
                string strAttributeString = DefaultAttribute;
                Improvement objImprovementOverride = ImprovementManager
                    .GetCachedImprovementListForValueOf(
                        CharacterObject, Improvement.ImprovementType.SwapSkillAttribute)
                    .Find(x => x.Target == DictionaryKey);
                if (objImprovementOverride != null)
                    strAttributeString = objImprovementOverride.ImprovedName;
                CharacterAttrib objNewAttribute = CharacterObject.GetAttribute(strAttributeString);
                if (_objAttribute == objNewAttribute)
                    return;
                if (_objAttribute != null)
                    _objAttribute.PropertyChanged -= OnLinkedAttributeChanged;
                if (objNewAttribute != null)
                    objNewAttribute.PropertyChanged += OnLinkedAttributeChanged;
                _objAttribute = objNewAttribute;
            }
            if (CharacterObject?.SkillsSection?.IsLoading != true)
                this.OnMultiplePropertyChanged(nameof(AttributeModifiers), nameof(Enabled));
        }

        private async ValueTask RecacheAttributeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                string strAttributeString = DefaultAttribute;
                string strDictionaryKey = await GetDictionaryKeyAsync(token).ConfigureAwait(false);
                Improvement objImprovementOverride = (await ImprovementManager
                                                            .GetCachedImprovementListForValueOfAsync(
                                                                CharacterObject,
                                                                Improvement.ImprovementType.SwapSkillAttribute,
                                                                token: token).ConfigureAwait(false))
                    .Find(x => x.Target == strDictionaryKey);
                if (objImprovementOverride != null)
                    strAttributeString = objImprovementOverride.ImprovedName;
                CharacterAttrib objNewAttribute
                    = await CharacterObject.GetAttributeAsync(strAttributeString, token: token).ConfigureAwait(false);
                if (_objAttribute == objNewAttribute)
                    return;
                if (_objAttribute != null)
                    _objAttribute.PropertyChanged -= OnLinkedAttributeChanged;
                if (objNewAttribute != null)
                    objNewAttribute.PropertyChanged += OnLinkedAttributeChanged;
                _objAttribute = objNewAttribute;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            if (CharacterObject?.SkillsSection?.IsLoading != true)
                this.OnMultiplePropertyChanged(nameof(AttributeModifiers), nameof(Enabled));
        }

        private string _strName = string.Empty; //English name of this skill
        private string _strNotes = string.Empty; //Text of any notes that were entered by the user
        private Color _colNotes = ColorManager.HasNotesColor;
        private bool _blnDefault;

        public void WriteTo(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            using (EnterReadLock.Enter(LockObject))
            {
                objWriter.WriteStartElement("skill");
                objWriter.WriteElementString("guid", Id.ToString("D", GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("suid", SkillId.ToString("D", GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("isknowledge",
                    IsKnowledgeSkill.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("skillcategory", SkillCategory);
                objWriter.WriteElementString("requiresgroundmovement",
                    RequiresGroundMovement.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("requiresswimmovement",
                    RequiresSwimMovement.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("requiresflymovement",
                    RequiresFlyMovement.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("karma", KarmaPoints.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("base",
                    BasePoints.ToString(GlobalSettings
                        .InvariantCultureInfo)); //this could actually be saved in karma too during career
                objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
                objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
                objWriter.WriteElementString("name", Name);
                if (!CharacterObject.Created)
                {
                    objWriter.WriteElementString("buywithkarma",
                        BuyWithKarma.ToString(GlobalSettings.InvariantCultureInfo));
                }

                if (Specializations.Count != 0)
                {
                    objWriter.WriteStartElement("specs");
                    foreach (SkillSpecialization objSpecialization in Specializations)
                    {
                        objSpecialization.Save(objWriter);
                    }

                    objWriter.WriteEndElement();
                }

                WriteToDerived(objWriter);

                objWriter.WriteEndElement();
            }
        }

        public virtual void WriteToDerived(XmlWriter objWriter)
        {
        }

        public async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;

            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                // <skill>
                XmlElementWriteHelper objBaseElement =
                    await objWriter.StartElementAsync("skill", token: token).ConfigureAwait(false);
                try
                {
                    int intPool = Pool;
                    int intSpecPool = intPool + await GetSpecializationBonusAsync(token: token).ConfigureAwait(false);

                    int intRatingModifiers = await RatingModifiersAsync(Attribute, token: token).ConfigureAwait(false);
                    int intDicePoolModifiers = await PoolModifiersAsync(Attribute, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("guid", InternalId, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("suid",
                        SkillId.ToString("D", GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name",
                            await DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name_english", Name, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("skillgroup",
                            SkillGroupObject != null
                                ? await SkillGroupObject.DisplayNameAsync(strLanguageToPrint, token)
                                    .ConfigureAwait(false)
                                : await LanguageManager.GetStringAsync(
                                    "String_None", strLanguageToPrint, token: token).ConfigureAwait(false),
                            token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("skillgroup_english",
                        SkillGroupObject?.Name ?? await LanguageManager
                            .GetStringAsync("String_None", strLanguageToPrint, token: token).ConfigureAwait(false),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("skillcategory",
                            await DisplayCategoryAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("skillcategory_english", SkillCategory, token: token)
                        .ConfigureAwait(false); //Might exist legacy but not existing atm, will see if stuff breaks
                    await objWriter.WriteElementStringAsync(
                        "grouped",
                        await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                            ? (SkillGroupObject != null && !await SkillGroupObject.GetIsBrokenAsync(token).ConfigureAwait(false) &&
                               await SkillGroupObject.GetRatingAsync(token).ConfigureAwait(false) > 0)
                            .ToString(GlobalSettings.InvariantCultureInfo)
                            : (SkillGroupObject != null && !await SkillGroupObject.GetHasAnyBreakingSkillsAsync(token).ConfigureAwait(false) &&
                               await SkillGroupObject.GetRatingAsync(token).ConfigureAwait(false) > 0)
                            .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("default", Default.ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("requiresgroundmovement",
                            RequiresGroundMovement.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("requiresswimmovement",
                            RequiresSwimMovement.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("requiresflymovement",
                            RequiresFlyMovement.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("rating",
                            (await GetRatingAsync(token).ConfigureAwait(false)).ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("ratingmax",
                            (await GetRatingMaximumAsync(token).ConfigureAwait(false)).ToString(objCulture),
                            token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("specializedrating", intSpecPool.ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("total", intPool.ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("knowledge",
                            IsKnowledgeSkill.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("exotic",
                            IsExoticSkill.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("buywithkarma",
                        (await GetBuyWithKarmaAsync(token).ConfigureAwait(false)).ToString(GlobalSettings
                            .InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("base",
                            (await GetBaseAsync(token).ConfigureAwait(false)).ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("karma",
                            (await GetKarmaAsync(token).ConfigureAwait(false)).ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("spec",
                            await DisplaySpecializationAsync(strLanguageToPrint, token).ConfigureAwait(false),
                            token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("attribute", Attribute, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("displayattribute",
                        await DisplayAttributeMethodAsync(strLanguageToPrint, token).ConfigureAwait(false),
                        token: token).ConfigureAwait(false);
                    if (GlobalSettings.PrintNotes)
                        await objWriter.WriteElementStringAsync("notes", Notes, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("source",
                        await CharacterObject.LanguageBookShortAsync(Source, strLanguageToPrint, token)
                            .ConfigureAwait(false), token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("page",
                            await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("attributemod",
                        (await (await CharacterObject.GetAttributeAsync(Attribute,
                            token: token).ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false))
                        .ToString(objCulture), token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("ratingmod",
                            (intRatingModifiers + intDicePoolModifiers).ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("poolmod", intDicePoolModifiers.ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("islanguage",
                                                            (await GetIsLanguageAsync(token).ConfigureAwait(false)).ToString(
                                                                GlobalSettings.InvariantCultureInfo), token: token)
                                   .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("isnativelanguage",
                                                            (await GetIsNativeLanguageAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("bp",
                        (await GetCurrentKarmaCostAsync(token).ConfigureAwait(false)).ToString(objCulture),
                        token: token).ConfigureAwait(false);
                    // <skillspecializations>
                    XmlElementWriteHelper objSkillSpecializationsElement = await objWriter
                        .StartElementAsync("skillspecializations", token: token).ConfigureAwait(false);
                    try
                    {
                        foreach (SkillSpecialization objSpec in Specializations)
                        {
                            await objSpec.Print(objWriter, objCulture, strLanguageToPrint, token: token)
                                .ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        // </skillspecializations>
                        await objSkillSpecializationsElement.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    // </skill>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        #region Factory

        /// <summary>
        /// Load a skill from a xml node from a saved .chum5 file
        /// </summary>
        /// <param name="xmlSkillNode">The XML node describing the skill</param>
        /// <param name="objCharacter">The character this skill belongs to</param>
        /// <returns></returns>
        public static Skill Load(Character objCharacter, XmlNode xmlSkillNode)
        {
            if (!xmlSkillNode.TryGetField("suid", Guid.TryParse, out Guid suid))
            {
                return null;
            }

            Guid guidSkillId = xmlSkillNode.TryGetField("id", Guid.TryParse, out Guid guiTemp) ? guiTemp : suid;
            Skill objLoadingSkill = null;
            bool blnIsKnowledgeSkill = false;
            if (xmlSkillNode.TryGetBoolFieldQuickly("isknowledge", ref blnIsKnowledgeSkill) && blnIsKnowledgeSkill)
            {
                KnowledgeSkill objKnowledgeSkill = null;
                if (guidSkillId != Guid.Empty)
                    objKnowledgeSkill = objCharacter.SkillsSection.KnowledgeSkills.Find(x => x.SkillId == guidSkillId);
                if (objKnowledgeSkill == null)
                {
                    if (xmlSkillNode["forced"] != null)
                        objKnowledgeSkill = new KnowledgeSkill(objCharacter,
                                                               xmlSkillNode["name"]?.InnerText ?? string.Empty,
                                                               !Convert.ToBoolean(
                                                                   xmlSkillNode["disableupgrades"]?.InnerText,
                                                                   GlobalSettings.InvariantCultureInfo));
                    else
                    {
                        objKnowledgeSkill = new KnowledgeSkill(objCharacter);
                    }
                }
                objKnowledgeSkill.Load(xmlSkillNode);
                objLoadingSkill = objKnowledgeSkill;
            }
            else if (suid != Guid.Empty)
            {
                if (guidSkillId != Guid.Empty)
                {
                    objLoadingSkill
                        = objCharacter.SkillsSection.Skills.Find(x => x.SkillId == guidSkillId);
                    if (objLoadingSkill?.IsExoticSkill == true)
                    {
                        objLoadingSkill = null;
                        string strSpecific = string.Empty;
                        if (xmlSkillNode.TryGetStringFieldQuickly("specific", ref strSpecific))
                        {
                            objLoadingSkill
                                = objCharacter.SkillsSection.Skills.Find(x => x.SkillId == guidSkillId && x is ExoticSkill y && y.Specific == strSpecific);
                            if (objLoadingSkill is ExoticSkill objLoadingExoticSkill)
                            {
                                objLoadingExoticSkill.Load(xmlSkillNode);
                            }
                        }
                    }
                }

                if (objLoadingSkill == null)
                {
                    XmlNode xmlSkillDataNode
                        = objCharacter.LoadData("skills.xml").SelectSingleNode("/chummer/skills/skill[id = " + suid.ToString("D").CleanXPath() + ']');

                    if (xmlSkillDataNode == null)
                        return null;

                    bool blnExotic = false;
                    xmlSkillDataNode.TryGetBoolFieldQuickly("exotic", ref blnExotic);
                    if (blnExotic)
                    {
                        ExoticSkill exotic = new ExoticSkill(objCharacter, xmlSkillDataNode);
                        exotic.Load(xmlSkillNode);
                        objLoadingSkill = exotic;
                    }
                    else
                    {
                        objLoadingSkill = new Skill(objCharacter, xmlSkillDataNode);
                    }
                }
            }
            /*
            else //This is ugly but i'm not sure how to make it pretty
            {
                if (n["forced"] != null && n["name"] != null)
                {
                    skill = new KnowledgeSkill(character, n["name"].InnerText);
                }
                else
                {
                    KnowledgeSkill knoSkill = new KnowledgeSkill(character);
                    knoSkill.Load(n);
                    skill = knoSkill;
                }
            }
            */
            // Legacy shim
            if (objLoadingSkill == null)
            {
                if (xmlSkillNode["forced"] != null)
                    objLoadingSkill = new KnowledgeSkill(objCharacter, xmlSkillNode["name"]?.InnerText ?? string.Empty,
                                                         !Convert.ToBoolean(
                                                             xmlSkillNode["disableupgrades"]?.InnerText,
                                                             GlobalSettings.InvariantCultureInfo));
                else
                {
                    KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(objCharacter);
                    objKnowledgeSkill.Load(xmlSkillNode);
                    objLoadingSkill = objKnowledgeSkill;
                }
            }

            if (xmlSkillNode.TryGetField("guid", Guid.TryParse, out guiTemp))
                objLoadingSkill.Id = guiTemp;

            if (!xmlSkillNode.TryGetMultiLineStringFieldQuickly("altnotes", ref objLoadingSkill._strNotes))
                xmlSkillNode.TryGetMultiLineStringFieldQuickly("notes", ref objLoadingSkill._strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            xmlSkillNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            objLoadingSkill._colNotes = ColorTranslator.FromHtml(sNotesColor);

            if (!objLoadingSkill.IsNativeLanguage)
            {
                xmlSkillNode.TryGetInt32FieldQuickly("karma", ref objLoadingSkill._intKarma);
                xmlSkillNode.TryGetInt32FieldQuickly("base", ref objLoadingSkill._intBase);
                xmlSkillNode.TryGetBoolFieldQuickly("buywithkarma", ref objLoadingSkill._blnBuyWithKarma);
                using (XmlNodeList xmlSpecList = xmlSkillNode.SelectNodes("specs/spec"))
                {
                    if (xmlSpecList == null)
                        return objLoadingSkill;
                    foreach (XmlNode xmlSpec in xmlSpecList)
                    {
                        SkillSpecialization objSpec = SkillSpecialization.Load(objCharacter, xmlSpec);
                        if (objSpec != null)
                            objLoadingSkill._lstSpecializations.Add(objSpec);
                    }
                }
            }

            return objLoadingSkill;
        }

        /// <summary>
        /// Loads skill saved in legacy format
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <param name="xmlSkillNode"></param>
        /// <returns></returns>
        public static Skill LegacyLoad(Character objCharacter, XmlNode xmlSkillNode)
        {
            if (xmlSkillNode == null)
                return null;
            xmlSkillNode.TryGetField("id", Guid.TryParse, out Guid suid, Guid.NewGuid());

            int.TryParse(xmlSkillNode["base"]?.InnerText, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intBaseRating);
            int.TryParse(xmlSkillNode["rating"]?.InnerText, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intFullRating);
            int intKarmaRating = intFullRating - intBaseRating;  //Not reading karma directly as career only increases rating

            bool blnTemp = false;

            string strName = xmlSkillNode["name"]?.InnerText ?? string.Empty;
            Skill objSkill;
            if (xmlSkillNode.TryGetBoolFieldQuickly("knowledge", ref blnTemp) && blnTemp)
            {
                KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(objCharacter)
                {
                    WritableName = strName,
                    Base = intBaseRating,
                    Karma = intKarmaRating,

                    Type = xmlSkillNode["skillcategory"]?.InnerText
                };

                objSkill = objKnowledgeSkill;
            }
            else
            {
                XmlDocument xmlSkillsDocument = objCharacter.LoadData("skills.xml");
                XmlNode xmlSkillDataNode = xmlSkillsDocument
                                               .SelectSingleNode("/chummer/skills/skill[id = '"
                                                                 + suid.ToString("D", GlobalSettings.InvariantCultureInfo)
                                                                 + "']")
                    //Some stuff apparently have a guid of 0000-000... (only exotic?)
                    ?? xmlSkillsDocument.SelectSingleNode("/chummer/skills/skill[name = " + strName.CleanXPath() + ']');

                bool blnIsKnowledgeSkill = xmlSkillDataNode != null
                                           && xmlSkillsDocument
                                              .SelectSingleNode(
                                                  "/chummer/categories/category[. = "
                                                  + xmlSkillDataNode["category"]?.InnerText.CleanXPath() + "]/@type")
                                              ?.Value != "active";

                objSkill = FromData(xmlSkillDataNode, objCharacter, blnIsKnowledgeSkill);
                objSkill._intBase = intBaseRating;
                objSkill._intKarma = intKarmaRating;

                if (objSkill is ExoticSkill objExoticSkill)
                {
                    //don't need to do more load then.
                    objExoticSkill.Specific = xmlSkillNode.SelectSingleNode("skillspecializations/skillspecialization/name")?.Value ?? string.Empty;
                    return objSkill;
                }

                xmlSkillNode.TryGetBoolFieldQuickly("buywithkarma", ref objSkill._blnBuyWithKarma);
            }

            using (XmlNodeList xmlSpecList = xmlSkillNode.SelectNodes("skillspecializations/skillspecialization"))
            {
                if (xmlSpecList != null && xmlSpecList.Count > 0)
                {
                    foreach (XmlNode xmlSpec in xmlSpecList)
                    {
                        SkillSpecialization objSpec = SkillSpecialization.Load(objCharacter, xmlSpec);
                        if (objSpec != null)
                            objSkill.Specializations.Add(objSpec);
                    }
                }
            }

            return objSkill;
        }

        public static Skill LoadFromHeroLab(Character objCharacter, XPathNavigator xmlSkillNode, bool blnIsKnowledgeSkill, string strSkillType = "")
        {
            if (xmlSkillNode == null)
                throw new ArgumentNullException(nameof(xmlSkillNode));
            string strName = xmlSkillNode.SelectSingleNodeAndCacheExpression("@name")?.Value ?? string.Empty;

            XmlNode xmlSkillDataNode = objCharacter.LoadData("skills.xml")
                .SelectSingleNode((blnIsKnowledgeSkill
                    ? "/chummer/knowledgeskills/skill[name = "
                    : "/chummer/skills/skill[name = ") + strName.CleanXPath() + ']');
            if (xmlSkillDataNode == null || !xmlSkillDataNode.TryGetField("id", Guid.TryParse, out Guid suid))
                suid = Guid.NewGuid();

            bool blnIsNativeLanguage = false;
            int intKarmaRating = 0;
            if (xmlSkillNode.SelectSingleNodeAndCacheExpression("@text")?.Value == "N")
                blnIsNativeLanguage = true;
            else if (!int.TryParse(xmlSkillNode.SelectSingleNodeAndCacheExpression("@base")?.Value, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out intKarmaRating)) // Only reading karma rating out for now, any base rating will need modification within SkillsSection
                intKarmaRating = 0;

            Skill objSkill;
            if (blnIsKnowledgeSkill)
            {
                objSkill = new KnowledgeSkill(objCharacter)
                {
                    WritableName = strName,
                    Karma = intKarmaRating,
                    Type = !string.IsNullOrEmpty(strSkillType) ? strSkillType : (xmlSkillDataNode?["category"]?.InnerText ?? "Academic"),
                    IsNativeLanguage = blnIsNativeLanguage
                };
            }
            else
            {
                objSkill = FromData(xmlSkillDataNode, objCharacter, false);
                if (xmlSkillNode.SelectSingleNodeAndCacheExpression("@fromgroup")?.Value == "yes")
                {
                    intKarmaRating -= objSkill.SkillGroupObject.Karma;
                }
                objSkill._intKarma = intKarmaRating;

                if (objSkill is ExoticSkill objExoticSkill)
                {
                    string strSpecializationName = xmlSkillNode.SelectSingleNodeAndCacheExpression("specialization/@bonustext")?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strSpecializationName))
                    {
                        int intLastPlus = strSpecializationName.LastIndexOf('+');
                        if (intLastPlus > strSpecializationName.Length)
                            strSpecializationName = strSpecializationName.Substring(0, intLastPlus - 1);
                    }
                    //don't need to do more load then.
                    objExoticSkill.Specific = strSpecializationName;
                    return objSkill;
                }
            }

            objSkill.SkillId = suid;

            foreach (XPathNavigator xmlSpecializationNode in xmlSkillNode.SelectAndCacheExpression("specialization"))
            {
                string strSpecializationName = xmlSpecializationNode.SelectSingleNodeAndCacheExpression("@bonustext")?.Value;
                if (string.IsNullOrEmpty(strSpecializationName))
                    continue;
                int intLastPlus = strSpecializationName.LastIndexOf('+');
                if (intLastPlus > strSpecializationName.Length)
                    strSpecializationName = strSpecializationName.Substring(0, intLastPlus - 1);
                objSkill.Specializations.Add(new SkillSpecialization(objCharacter, strSpecializationName));
            }

            return objSkill;
        }

        //TODO CACHE INVALIDATE

        /// <summary>
        /// Load a skill from a data file describing said skill
        /// </summary>
        /// <param name="xmlNode">The XML node describing the skill</param>
        /// <param name="objCharacter">The character the skill belongs to</param>
        /// <param name="blnIsKnowledgeSkill">Whether or not this skill is a knowledge skill.</param>
        /// <returns></returns>
        public static Skill FromData(XmlNode xmlNode, Character objCharacter, bool blnIsKnowledgeSkill)
        {
            if (xmlNode == null)
                return null;
            if (xmlNode["exotic"]?.InnerText == bool.TrueString)
            {
                //load exotic skill
                return new ExoticSkill(objCharacter, xmlNode);
            }

            if (blnIsKnowledgeSkill)
            {
                //TODO INIT SKILL
                Utils.BreakIfDebug();

                return new KnowledgeSkill(objCharacter);
            }

            //TODO INIT SKILL

            return new Skill(objCharacter, xmlNode);
        }

        protected Skill(Character character)
        {
            CharacterObject = character ?? throw new ArgumentNullException(nameof(character));
            CharacterObject.PropertyChanged += OnCharacterChanged;
            CharacterObject.Settings.PropertyChanged += OnCharacterSettingsPropertyChanged;
            CharacterObject.AttributeSection.PropertyChanged += OnAttributeSectionChanged;
            CharacterObject.AttributeSection.Attributes.CollectionChanged += OnAttributesCollectionChanged;
            Specializations.CollectionChanged += SpecializationsOnCollectionChanged;
            Specializations.BeforeClearCollectionChanged += SpecializationsOnBeforeClearCollectionChanged;
        }

        private async void OnAttributesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            using (await EnterReadLock.EnterAsync(LockObject).ConfigureAwait(false))
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    {
                        if (e.NewItems.OfType<CharacterAttrib>().Any(x => x.Abbrev == Attribute))
                        {
                            await RecacheAttributeAsync().ConfigureAwait(false);
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Remove:
                    {
                        if (e.OldItems.OfType<CharacterAttrib>().Any(x => x.Abbrev == Attribute))
                        {
                            await RecacheAttributeAsync().ConfigureAwait(false);
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Replace:
                    {
                        if (e.OldItems.OfType<CharacterAttrib>().Any(x => x.Abbrev == Attribute)
                            || e.NewItems.OfType<CharacterAttrib>().Any(x => x.Abbrev == Attribute))
                        {
                            await RecacheAttributeAsync().ConfigureAwait(false);
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Reset:
                    {
                        await RecacheAttributeAsync().ConfigureAwait(false);
                        break;
                    }
                }
            }
        }

        private async void OnAttributeSectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AttributeSection.AttributeCategory))
                return;
            using (await EnterReadLock.EnterAsync(LockObject).ConfigureAwait(false))
                await RecacheAttributeAsync().ConfigureAwait(false);
        }

        //load from data
        protected Skill(Character character, XmlNode xmlNode) : this(character)
        //Ugly hack, needs by then
        {
            if (xmlNode == null)
                return;
            _strName = xmlNode["name"]?.InnerText; //No need to catch errors (for now), if missing we are fsked anyway
            DefaultAttribute = xmlNode["attribute"]?.InnerText;
            SkillCategory = xmlNode["category"]?.InnerText ?? string.Empty;
            Default = xmlNode["default"]?.InnerText == bool.TrueString;
            Source = xmlNode["source"]?.InnerText;
            Page = xmlNode["page"]?.InnerText;
            if (xmlNode.TryGetField("id", Guid.TryParse, out Guid guiTemp))
                _guidSkillId = guiTemp;
            else if (xmlNode.TryGetField("suid", Guid.TryParse, out guiTemp))
                _guidSkillId = guiTemp;
            if (xmlNode.TryGetField("guid", Guid.TryParse, out guiTemp))
                _guidInternalId = guiTemp;

            Lazy<XPathNavigator> objMyNode = new Lazy<XPathNavigator>(() => this.GetNodeXPath());
            bool blnTemp = false;
            if (xmlNode.TryGetBoolFieldQuickly("requiresgroundmovement", ref blnTemp) || objMyNode.Value?.TryGetBoolFieldQuickly("requiresgroundmovement", ref blnTemp) == true)
                RequiresGroundMovement = blnTemp;
            if (xmlNode.TryGetBoolFieldQuickly("requiresswimmovement", ref blnTemp) || objMyNode.Value?.TryGetBoolFieldQuickly("requiresswimmovement", ref blnTemp) == true)
                RequiresSwimMovement = blnTemp;
            if (xmlNode.TryGetBoolFieldQuickly("requiresflymovement", ref blnTemp) || objMyNode.Value?.TryGetBoolFieldQuickly("requiresflymovement", ref blnTemp) == true)
                RequiresFlyMovement = blnTemp;

            string strGroup = xmlNode["skillgroup"]?.InnerText;

            if (!string.IsNullOrEmpty(strGroup))
            {
                SkillGroup = strGroup;
                SkillGroupObject = Skills.SkillGroup.Get(this);
                if (SkillGroupObject != null)
                {
                    SkillGroupObject.PropertyChanged += OnSkillGroupChanged;
                }
            }

            _blnRecalculateCachedSuggestedSpecializations = true;
        }

        #endregion Factory

        /// <summary>
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public int BasePoints
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _intBase;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intBase == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _intBase, value) != value)
                            OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public async ValueTask<int> GetBasePointsAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _intBase;
        }

        /// <summary>
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public async ValueTask SetBasePointsAsync(int value, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_intBase == value)
                    return;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    if (Interlocked.Exchange(ref _intBase, value) != value)
                        OnPropertyChanged(nameof(BasePoints));
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public async ValueTask ModifyBasePointsAsync(int value, CancellationToken token = default)
        {
            if (value == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                Interlocked.Add(ref _intBase, value);
                OnPropertyChanged(nameof(BasePoints));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How many points REALLY are in _karma Better than subclasses calculating Karma - FreeKarma()
        /// </summary>
        public int KarmaPoints
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _intKarma;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intKarma == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _intKarma, value) != value)
                            OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// How many points REALLY are in _karma Better than subclasses calculating Karma - FreeKarma()
        /// </summary>
        public async ValueTask<int> GetKarmaPointsAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _intKarma;
        }

        /// <summary>
        /// How many points REALLY are in _karma Better than subclasses calculating Karma - FreeKarma()
        /// </summary>
        public async ValueTask SetKarmaPointsAsync(int value, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_intKarma == value)
                    return;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    if (Interlocked.Exchange(ref _intKarma, value) != value)
                        OnPropertyChanged(nameof(KarmaPoints));
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// How many points REALLY are in _karma Better than subclasses calculating Karma - FreeKarma()
        /// </summary>
        public async ValueTask ModifyKarmaPointsAsync(int value, CancellationToken token = default)
        {
            if (value == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                Interlocked.Add(ref _intKarma, value);
                OnPropertyChanged(nameof(KarmaPoints));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Is it possible to place points in Base or is it prevented? (Build method or skill group)
        /// </summary>
        public bool BaseUnlocked
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return CharacterObject.EffectiveBuildMethodUsesPriorityTables
                           && (SkillGroupObject == null
                               || SkillGroupObject.Base <= 0
                               || (CharacterObject.Settings.UsePointsOnBrokenGroups
                                   && (!CharacterObject.Settings.StrictSkillGroupsInCreateMode
                                       || CharacterObject.Created || CharacterObject.IgnoreRules)));
            }
        }

        /// <summary>
        /// Is it possible to place points in Base or is it prevented? (Build method or skill group)
        /// </summary>
        public async ValueTask<bool> GetBaseUnlockedAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return await CharacterObject.GetEffectiveBuildMethodUsesPriorityTablesAsync(token).ConfigureAwait(false)
                       && (SkillGroupObject == null
                           || await SkillGroupObject.GetBaseAsync(token).ConfigureAwait(false) <= 0
                           || (CharacterObject.Settings.UsePointsOnBrokenGroups
                               && (!await CharacterObject.Settings.GetStrictSkillGroupsInCreateModeAsync(token)
                                                         .ConfigureAwait(false)
                                   || await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                                   || await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))));
        }

        /// <summary>
        /// Is it possible to place points in Karma or is it prevented a stricter interpretation of the rules
        /// </summary>
        public bool KarmaUnlocked
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (CharacterObject.Settings.StrictSkillGroupsInCreateMode && !CharacterObject.Created &&
                        !CharacterObject.IgnoreRules)
                    {
                        return SkillGroupObject == null || SkillGroupObject.Rating <= 0;
                    }

                    return true;
                }
            }
        }

        /// <summary>
        /// Is it possible to place points in Karma or is it prevented a stricter interpretation of the rules
        /// </summary>
        public async ValueTask<bool> GetKarmaUnlockedAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (await CharacterObject.Settings.GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false)
                    && !await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                    && !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                {
                    return SkillGroupObject == null
                           || await SkillGroupObject.GetRatingAsync(token).ConfigureAwait(false) <= 0;
                }

                return true;
            }
        }

        /// <summary>
        /// The amount of points this skill have from skill points and bonuses
        /// to the skill rating that would be obtained in some points of character creation
        /// </summary>
        public int Base
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (SkillGroupObject?.Base > 0)
                    {
                        if ((CharacterObject.Settings.StrictSkillGroupsInCreateMode && !CharacterObject.Created &&
                             !CharacterObject.IgnoreRules)
                            || !CharacterObject.Settings.UsePointsOnBrokenGroups)
                            BasePoints = 0;
                        return Math.Min(SkillGroupObject.Base + BasePoints + FreeBase, RatingMaximum);
                    }

                    return Math.Min(BasePoints + FreeBase, RatingMaximum);
                }
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (SkillGroupObject?.Base > 0
                        && ((CharacterObject.Settings.StrictSkillGroupsInCreateMode && !CharacterObject.Created &&
                             !CharacterObject.IgnoreRules)
                            || !CharacterObject.Settings.UsePointsOnBrokenGroups))
                        return;

                    using (LockObject.EnterWriteLock())
                    {
                        //Calculate how far above maximum we are.
                        int intOverMax = value + Karma - RatingMaximum + RatingModifiers(Attribute);

                        if (intOverMax > 0) //Too much
                        {
                            //Get the smaller value, how far above, how much we can reduce
                            int intMax = Math.Min(intOverMax, KarmaPoints);

                            KarmaPoints -= intMax; //reduce both by that amount
                            intOverMax -= intMax;
                        }

                        value -= Math.Max(0, intOverMax); //reduce by 0 or points over.

                        BasePoints = Math.Max(0, value - (FreeBase + (SkillGroupObject?.Base ?? 0)));
                    }
                }
            }
        }

        /// <summary>
        /// The amount of points this skill have from skill points and bonuses
        /// to the skill rating that would be obtained in some points of character creation
        /// </summary>
        public async ValueTask<int> GetBaseAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (SkillGroupObject != null)
                {
                    int intGroupBase = await SkillGroupObject.GetBaseAsync(token).ConfigureAwait(false);
                    if (intGroupBase > 0)
                    {
                        CharacterSettings objSettings =
                            await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                        if ((await objSettings.GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false)
                             && !await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                             && !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                            || !await objSettings.GetUsePointsOnBrokenGroupsAsync(token).ConfigureAwait(false))
                            await SetBasePointsAsync(0, token).ConfigureAwait(false);
                        return Math.Min(intGroupBase + await GetBasePointsAsync(token).ConfigureAwait(false) + await GetFreeBaseAsync(token).ConfigureAwait(false),
                            await GetRatingMaximumAsync(token).ConfigureAwait(false));
                    }
                }

                return Math.Min(await GetBasePointsAsync(token).ConfigureAwait(false) + await GetFreeBaseAsync(token).ConfigureAwait(false),
                                await GetRatingMaximumAsync(token).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// The amount of points this skill have from skill points and bonuses
        /// to the skill rating that would be obtained in some points of character creation
        /// </summary>
        public async ValueTask SetBaseAsync(int value, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                int intSkillGroupBase = SkillGroupObject != null ? await SkillGroupObject.GetBaseAsync(token).ConfigureAwait(false) : 0;
                if (intSkillGroupBase > 0
                    && ((CharacterObject.Settings.StrictSkillGroupsInCreateMode && !await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false) &&
                         !CharacterObject.IgnoreRules)
                        || !CharacterObject.Settings.UsePointsOnBrokenGroups))
                    return;

                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    //Calculate how far above maximum we are.
                    int intOverMax = value + await GetKarmaAsync(token).ConfigureAwait(false) - await GetRatingMaximumAsync(token).ConfigureAwait(false)
                                     + await RatingModifiersAsync(Attribute, token: token).ConfigureAwait(false);

                    if (intOverMax > 0) //Too much
                    {
                        int intKarmaPoints = await GetKarmaPointsAsync(token).ConfigureAwait(false);
                        //Get the smaller value, how far above, how much we can reduce
                        int intMax = Math.Min(intOverMax, intKarmaPoints);

                        //reduce both by that amount
                        await ModifyKarmaPointsAsync(-intMax, token).ConfigureAwait(false);
                        intOverMax -= intMax;
                    }

                    value -= Math.Max(0, intOverMax); //reduce by 0 or points over.

                    await SetBasePointsAsync(Math.Max(0, value - (await GetFreeBaseAsync(token).ConfigureAwait(false) + intSkillGroupBase)),
                                             token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Amount of skill points bought with karma and bonuses to the skills rating
        /// </summary>
        public int Karma
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (CharacterObject.Settings.StrictSkillGroupsInCreateMode && !CharacterObject.Created &&
                        !CharacterObject.IgnoreRules && SkillGroupObject?.Karma > 0)
                    {
                        KarmaPoints = 0;
                        Specializations.RemoveAll(x => !x.Free);
                        return SkillGroupObject.Karma;
                    }

                    return Math.Min(KarmaPoints + FreeKarma + (SkillGroupObject?.Karma ?? 0), RatingMaximum);
                }
            }
            set
            {
                using (LockObject.EnterWriteLock())
                {
                    //Calculate how far above maximum we are.
                    int intOverMax = value + Base - RatingMaximum + RatingModifiers(Attribute);

                    if (intOverMax > 0) //Too much
                    {
                        //Get the smaller value, how far above, how much we can reduce
                        int intMax = Math.Min(intOverMax, BasePoints);

                        BasePoints -= intMax; //reduce both by that amount
                        intOverMax -= intMax;
                    }

                    value -= Math.Max(0, intOverMax); //reduce by 0 or points over.

                    //Handle free levels, don,t go below 0
                    KarmaPoints = Math.Max(0, value - (FreeKarma + (SkillGroupObject?.Karma ?? 0)));
                }
            }
        }

        /// <summary>
        /// Amount of skill points bought with karma and bonuses to the skills rating
        /// </summary>
        public async ValueTask<int> GetKarmaAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                int intGroupKarma = 0;
                if (SkillGroupObject != null)
                {
                    intGroupKarma = await SkillGroupObject.GetKarmaAsync(token).ConfigureAwait(false);
                    if (intGroupKarma > 0
                        && await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false))
                            .GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false)
                        && !await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                        && !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                    {
                        await SetKarmaPointsAsync(0, token).ConfigureAwait(false);
                        foreach (SkillSpecialization objSpecialization in await Specializations.ToListAsync(
                                     x => !x.Free, token).ConfigureAwait(false))
                        {
                            await Specializations.RemoveAsync(objSpecialization, token).ConfigureAwait(false);
                        }

                        return intGroupKarma;
                    }
                }

                return Math.Min(await GetKarmaPointsAsync(token).ConfigureAwait(false) + await GetFreeKarmaAsync(token).ConfigureAwait(false) + intGroupKarma,
                    await GetRatingMaximumAsync(token).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// Amount of skill points bought with karma and bonuses to the skills rating
        /// </summary>
        public async ValueTask SetKarmaAsync(int value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                //Calculate how far above maximum we are.
                int intOverMax = value + await GetBaseAsync(token).ConfigureAwait(false) - await GetRatingMaximumAsync(token).ConfigureAwait(false) + await RatingModifiersAsync(Attribute, token: token).ConfigureAwait(false);

                if (intOverMax > 0) //Too much
                {
                    int intBasePoints = await GetBasePointsAsync(token).ConfigureAwait(false);
                    //Get the smaller value, how far above, how much we can reduce
                    int intMax = Math.Min(intOverMax, intBasePoints);

                    //reduce both by that amount
                    await ModifyBasePointsAsync(-intMax, token).ConfigureAwait(false);
                    intOverMax -= intMax;
                }

                value -= Math.Max(0, intOverMax); //reduce by 0 or points over.

                //Handle free levels, don,t go below 0
                await SetKarmaPointsAsync(
                    Math.Max(
                        0,
                        value - (await GetFreeKarmaAsync(token).ConfigureAwait(false) + (SkillGroupObject != null
                            ? await SkillGroupObject.GetKarmaAsync(token).ConfigureAwait(false)
                            : 0))), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Levels in this skill. Read only. You probably want to increase
        /// Karma instead
        /// </summary>
        public int Rating
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Math.Max(CyberwareRating, TotalBaseRating);
            }
        }

        /// <summary>
        /// Levels in this skill. Read only. You probably want to increase
        /// Karma instead
        /// </summary>
        public async ValueTask<int> GetRatingAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return Math.Max(await GetCyberwareRatingAsync(token).ConfigureAwait(false), await GetTotalBaseRatingAsync(token).ConfigureAwait(false));
        }

        /// <summary>
        /// The rating the character has paid for, plus any improvement-based bonuses to skill rating.
        /// </summary>
        public int TotalBaseRating
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return LearnedRating + RatingModifiers(Attribute);
            }
        }

        /// <summary>
        /// The rating the character has paid for, plus any improvement-based bonuses to skill rating.
        /// </summary>
        public async ValueTask<int> GetTotalBaseRatingAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return await GetLearnedRatingAsync(token).ConfigureAwait(false) + await RatingModifiersAsync(Attribute, token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// The rating the character have actually paid for, not including skillwires
        /// or other overrides for skill Rating. Read only, you probably want to
        /// increase Karma instead.
        /// </summary>
        public int LearnedRating
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Karma + Base;
            }
        }

        /// <summary>
        /// The rating the character have actually paid for, not including skillwires
        /// or other overrides for skill Rating. Read only, you probably want to
        /// increase Karma instead.
        /// </summary>
        public async ValueTask<int> GetLearnedRatingAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return await GetKarmaAsync(token).ConfigureAwait(false) + await GetBaseAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Is the specialization bought with karma. During career mode this is undefined
        /// </summary>
        public virtual bool BuyWithKarma
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return (_blnBuyWithKarma || ForcedBuyWithKarma) && !ForcedNotBuyWithKarma &&
                           Specializations.Any(x => !x.Free);
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    bool blnNewValue = (value || ForcedBuyWithKarma) && !ForcedNotBuyWithKarma &&
                                       Specializations.Any(x => !x.Free);
                    if (_blnBuyWithKarma == blnNewValue)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnBuyWithKarma = blnNewValue;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public virtual async ValueTask<bool> GetBuyWithKarmaAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return (_blnBuyWithKarma || await GetForcedBuyWithKarmaAsync(token).ConfigureAwait(false))
                       && !await GetForcedNotBuyWithKarmaAsync(token).ConfigureAwait(false)
                       && await Specializations.AnyAsync(x => !x.Free, token: token).ConfigureAwait(false);
        }

        public virtual async ValueTask SetBuyWithKarmaAsync(bool value, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                bool blnNewValue = (value || await GetForcedBuyWithKarmaAsync(token).ConfigureAwait(false))
                                   && !await GetForcedNotBuyWithKarmaAsync(token).ConfigureAwait(false)
                                   && await Specializations.AnyAsync(x => !x.Free, token: token).ConfigureAwait(false);
                if (_blnBuyWithKarma == blnNewValue)
                    return;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    _blnBuyWithKarma = blnNewValue;
                    OnPropertyChanged(nameof(BuyWithKarma));
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Maximum possible rating
        /// </summary>
        public int RatingMaximum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    int intOtherBonus =
                        RelevantImprovements(x => x.ImproveType == Improvement.ImprovementType.Skill && x.Enabled)
                            .Sum(x => x.Maximum);
                    int intBaseMax = IsKnowledgeSkill
                        ? CharacterObject.Settings.MaxKnowledgeSkillRating
                        : CharacterObject.Settings.MaxSkillRating;
                    if (!CharacterObject.Created && !CharacterObject.IgnoreRules)
                    {
                        intBaseMax = IsKnowledgeSkill
                            ? CharacterObject.Settings.MaxKnowledgeSkillRatingCreate
                            : CharacterObject.Settings.MaxSkillRatingCreate;
                    }

                    return intBaseMax + intOtherBonus;
                }
            }
        }

        /// <summary>
        /// Maximum possible rating
        /// </summary>
        public async ValueTask<int> GetRatingMaximumAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                int intOtherBonus =
                    (await RelevantImprovementsAsync(
                            x => x.ImproveType == Improvement.ImprovementType.Skill && x.Enabled, token: token)
                        .ConfigureAwait(false)).Sum(x => x.Maximum);
                CharacterSettings objSettings = await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                int intBaseMax = await (IsKnowledgeSkill
                    ? objSettings.GetMaxKnowledgeSkillRatingAsync(token)
                    : objSettings.GetMaxSkillRatingAsync(token)).ConfigureAwait(false);
                if (!await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false) &&
                    !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                {
                    intBaseMax = await (IsKnowledgeSkill
                        ? objSettings.GetMaxKnowledgeSkillRatingCreateAsync(token)
                        : objSettings.GetMaxSkillRatingCreateAsync(token)).ConfigureAwait(false);
                }

                return intBaseMax + intOtherBonus;
            }
        }

        /// <summary>
        /// The total, general purpose dice pool for this skill, using another
        /// value for the attribute part of the test. This allows calculation of dice pools
        /// while using cyberlimbs or while rigging
        /// </summary>
        /// <param name="intAttributeOverrideValue">The value to be used for the attribute if it's not the default value. int.MinValue is equivalent to not overriding.</param>
        /// <param name="strAttribute">The English abbreviation of the used attribute.</param>
        /// <param name="blnIncludeConditionals">Whether to include improvements that don't apply under all circumstances.</param>
        /// <returns></returns>
        public int PoolOtherAttribute(string strAttribute, bool blnIncludeConditionals = false, int intAttributeOverrideValue = int.MinValue)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                bool blnIsNativeLanguage = IsNativeLanguage;
                if (!Enabled && !blnIsNativeLanguage)
                    return 0;
                int intValue = intAttributeOverrideValue > int.MinValue
                    ? intAttributeOverrideValue
                    : CharacterObject.AttributeSection.GetAttributeByName(strAttribute).TotalValue;
                if (intValue <= 0)
                    return 0;
                if (blnIsNativeLanguage)
                    return int.MaxValue;
                int intRating = Rating;
                if (intRating > 0)
                    return Math.Max(0,
                        intRating + intValue + PoolModifiers(strAttribute, blnIncludeConditionals) +
                        CharacterObject.WoundModifier + CharacterObject.SustainingPenalty);
                return Default
                    ? Math.Max(0,
                        intValue + PoolModifiers(strAttribute, blnIncludeConditionals) + DefaultModifier +
                        CharacterObject.WoundModifier + CharacterObject.SustainingPenalty)
                    : 0;
            }
        }

        /// <summary>
        /// The total, general purpose dice pool for this skill, using another
        /// value for the attribute part of the test. This allows calculation of dice pools
        /// while using cyberlimbs or while rigging
        /// </summary>
        /// <param name="intAttributeOverrideValue">The value to be used for the attribute if it's not the default value. int.MinValue is equivalent to not overriding.</param>
        /// <param name="strAttribute">The English abbreviation of the used attribute.</param>
        /// <param name="blnIncludeConditionals">Whether to include improvements that don't apply under all circumstances.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public async ValueTask<int> PoolOtherAttributeAsync(string strAttribute, bool blnIncludeConditionals = false, int intAttributeOverrideValue = int.MinValue, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                bool blnIsNativeLanguage = await GetIsNativeLanguageAsync(token).ConfigureAwait(false);
                if (!await GetEnabledAsync(token).ConfigureAwait(false) && !blnIsNativeLanguage)
                    return 0;
                int intValue = intAttributeOverrideValue > int.MinValue
                    ? intAttributeOverrideValue
                    : await (await CharacterObject.AttributeSection.GetAttributeByNameAsync(strAttribute, token)
                        .ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false);
                if (intValue <= 0)
                    return 0;
                if (blnIsNativeLanguage)
                    return int.MaxValue;
                int intRating = await GetRatingAsync(token).ConfigureAwait(false);
                if (intRating > 0)
                    return Math.Max(0,
                        intRating + intValue +
                        await PoolModifiersAsync(strAttribute, blnIncludeConditionals, token).ConfigureAwait(false) +
                        CharacterObject.WoundModifier + CharacterObject.SustainingPenalty);
                return Default
                    ? Math.Max(0,
                        intValue + await PoolModifiersAsync(strAttribute, blnIncludeConditionals, token)
                            .ConfigureAwait(false) + DefaultModifier +
                        CharacterObject.WoundModifier + CharacterObject.SustainingPenalty)
                    : 0;
            }
        }

        private static readonly Guid s_GuiReflexRecorderId = new Guid("17a6ba49-c21c-461b-9830-3beae8a237fc");

        public int DefaultModifier
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (ImprovementManager.GetCachedImprovementListForValueOf(CharacterObject,
                            Improvement.ImprovementType.ReflexRecorderOptimization).Count > 0)
                    {
                        List<Cyberware> lstReflexRecorders = CharacterObject.Cyberware
                            .Where(x => x.SourceID == s_GuiReflexRecorderId)
                            .ToList();
                        if (lstReflexRecorders.Count > 0)
                        {
                            using (new FetchSafelyFromPool<HashSet<string>>(
                                       Utils.StringHashSetPool, out HashSet<string> setSkillNames))
                            {
                                if (SkillGroupObject != null)
                                {
                                    setSkillNames.AddRange(SkillGroupObject.SkillList.Select(x => x.DictionaryKey));
                                    if (lstReflexRecorders.Any(x => setSkillNames.Contains(x.Extra)))
                                    {
                                        return 0;
                                    }
                                }
                                else if (lstReflexRecorders.Any(x => x.Extra == DictionaryKey))
                                    return 0;
                            }
                        }
                    }

                    return -1;
                }
            }
        }

        public async ValueTask<int> GetDefaultModifierAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if ((await ImprovementManager
                        .GetCachedImprovementListForValueOfAsync(CharacterObject,
                            Improvement.ImprovementType.ReflexRecorderOptimization, token: token).ConfigureAwait(false))
                    .Count > 0)
                {
                    List<Cyberware> lstReflexRecorders = await CharacterObject.Cyberware
                        .ToListAsync(x => x.SourceID == s_GuiReflexRecorderId, token: token).ConfigureAwait(false);
                    if (lstReflexRecorders.Count > 0)
                    {
                        using (new FetchSafelyFromPool<HashSet<string>>(
                                   Utils.StringHashSetPool, out HashSet<string> setSkillNames))
                        {
                            if (SkillGroupObject != null)
                            {
                                foreach (Skill objSkill in SkillGroupObject.SkillList)
                                    setSkillNames.Add(await objSkill.GetDictionaryKeyAsync(token)
                                        .ConfigureAwait(false));
                                if (lstReflexRecorders.Any(x => setSkillNames.Contains(x.Extra)))
                                {
                                    return 0;
                                }
                            }
                            else
                            {
                                string strKey = await GetDictionaryKeyAsync(token).ConfigureAwait(false);
                                if (lstReflexRecorders.Any(x => x.Extra == strKey))
                                    return 0;
                            }
                        }
                    }
                }

                return -1;
            }
        }

        /// <summary>
        /// Things that modify the dicepool of the skill
        /// </summary>
        public int PoolModifiers(string strUseAttribute, bool blnIncludeConditionals = false)
        {
            return Bonus(false, strUseAttribute, blnIncludeConditionals);
        }

        /// <summary>
        /// Things that modify the dicepool of the skill
        /// </summary>
        public ValueTask<int> PoolModifiersAsync(string strUseAttribute, bool blnIncludeConditionals = false, CancellationToken token = default)
        {
            return BonusAsync(false, strUseAttribute, blnIncludeConditionals, token);
        }

        /// <summary>
        /// Things that modify the dicepool of the skill
        /// </summary>
        public int RatingModifiers(string strUseAttribute, bool blnIncludeConditionals = false)
        {
            return Bonus(true, strUseAttribute, blnIncludeConditionals);
        }

        /// <summary>
        /// Things that modify the dicepool of the skill
        /// </summary>
        public ValueTask<int> RatingModifiersAsync(string strUseAttribute, bool blnIncludeConditionals = false, CancellationToken token = default)
        {
            return BonusAsync(true, strUseAttribute, blnIncludeConditionals, token);
        }

        protected int Bonus(bool blnAddToRating, string strUseAttribute, bool blnIncludeConditionals = false)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                //Some of this is not future proof. Rating that don't stack is not supported but i'm not aware of any cases where that will happen (for skills)
                return (RelevantImprovements(x => x.AddToRating == blnAddToRating, strUseAttribute,
                    blnIncludeConditionals).Sum(x => x.Value)).StandardRound();
            }
        }

        protected async ValueTask<int> BonusAsync(bool blnAddToRating, string strUseAttribute, bool blnIncludeConditionals = false, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                //Some of this is not future proof. Rating that don't stack is not supported but i'm not aware of any cases where that will happen (for skills)
                List<Improvement> lstImprovements = await RelevantImprovementsAsync(
                    x => x.AddToRating == blnAddToRating,
                    strUseAttribute, blnIncludeConditionals,
                    token: token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                return lstImprovements.Sum(x => x.Value).StandardRound();
            }
        }

        public IEnumerable<Improvement> RelevantImprovements(Func<Improvement, bool> funcWherePredicate = null, string strUseAttribute = "", bool blnIncludeConditionals = false, bool blnExitAfterFirst = false)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                string strNameToUse = DictionaryKey;
                if (string.IsNullOrEmpty(strUseAttribute))
                    strUseAttribute = Attribute;
                foreach (Improvement objImprovement in CharacterObject.Improvements)
                {
                    if (!objImprovement.Enabled || funcWherePredicate?.Invoke(objImprovement) == false) continue;
                    if (!blnIncludeConditionals && !string.IsNullOrWhiteSpace(objImprovement.Condition)) continue;
                    switch (objImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.SwapSkillAttribute:
                        case Improvement.ImprovementType.SwapSkillSpecAttribute:
                            if (objImprovement.Target == strNameToUse)
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst)
                                    yield break;
                            }

                            break;

                        case Improvement.ImprovementType.Skill:
                        case Improvement.ImprovementType.SkillDisable:
                            if (objImprovement.ImprovedName == strNameToUse)
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst)
                                    yield break;
                            }

                            break;

                        case Improvement.ImprovementType.AllowSkillDefault:
                            if (string.IsNullOrEmpty(objImprovement.ImprovedName) ||
                                objImprovement.ImprovedName == strNameToUse)
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst)
                                    yield break;
                            }

                            break;

                        case Improvement.ImprovementType.SkillGroup:
                        case Improvement.ImprovementType.SkillGroupDisable:
                            if (objImprovement.ImprovedName == SkillGroup &&
                                !objImprovement.Exclude.Contains(strNameToUse) &&
                                !objImprovement.Exclude.Contains(SkillCategory))
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst)
                                    yield break;
                            }

                            break;

                        case Improvement.ImprovementType.SkillCategory:
                            if (objImprovement.ImprovedName == SkillCategory &&
                                !objImprovement.Exclude.Contains(strNameToUse))
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst)
                                    yield break;
                            }

                            break;

                        case Improvement.ImprovementType.SkillAttribute:
                            if (objImprovement.ImprovedName == strUseAttribute &&
                                !objImprovement.Exclude.Contains(strNameToUse))
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst)
                                    yield break;
                            }

                            break;

                        case Improvement.ImprovementType.SkillLinkedAttribute:
                            if (objImprovement.ImprovedName == Attribute &&
                                !objImprovement.Exclude.Contains(strNameToUse))
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst)
                                    yield break;
                            }

                            break;

                        case Improvement.ImprovementType.BlockSkillDefault:
                            if (objImprovement.ImprovedName == SkillGroup)
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst)
                                    yield break;
                            }

                            break;

                        case Improvement.ImprovementType.EnhancedArticulation:
                            if (SkillCategory == "Physical Active" &&
                                AttributeSection.PhysicalAttributes.Contains(Attribute))
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst)
                                    yield break;
                            }

                            break;
                    }
                }
            }
        }

        public async Task<List<Improvement>> RelevantImprovementsAsync(Func<Improvement, bool> funcWherePredicate = null, string strUseAttribute = "", bool blnIncludeConditionals = false, bool blnExitAfterFirst = false, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                string strNameToUse = DictionaryKey;
                if (string.IsNullOrEmpty(strUseAttribute))
                    strUseAttribute = Attribute;
                List<Improvement> lstReturn = new List<Improvement>();
                await (await CharacterObject.GetImprovementsAsync(token).ConfigureAwait(false)).ForEachWithBreakAsync(
                    objImprovement =>
                    {
                        if (!objImprovement.Enabled || funcWherePredicate?.Invoke(objImprovement) == false)
                            return true;
                        if (!blnIncludeConditionals && !string.IsNullOrWhiteSpace(objImprovement.Condition))
                            return true;
                        switch (objImprovement.ImproveType)
                        {
                            case Improvement.ImprovementType.SwapSkillAttribute:
                            case Improvement.ImprovementType.SwapSkillSpecAttribute:
                                if (objImprovement.Target == strNameToUse)
                                {
                                    lstReturn.Add(objImprovement);
                                    if (blnExitAfterFirst)
                                        return false;
                                }

                                break;

                            case Improvement.ImprovementType.Skill:
                            case Improvement.ImprovementType.SkillDisable:
                                if (objImprovement.ImprovedName == strNameToUse)
                                {
                                    lstReturn.Add(objImprovement);
                                    if (blnExitAfterFirst)
                                        return false;
                                }

                                break;

                            case Improvement.ImprovementType.AllowSkillDefault:
                                if (string.IsNullOrEmpty(objImprovement.ImprovedName) ||
                                    objImprovement.ImprovedName == strNameToUse)
                                {
                                    lstReturn.Add(objImprovement);
                                    if (blnExitAfterFirst)
                                        return false;
                                }

                                break;

                            case Improvement.ImprovementType.SkillGroup:
                            case Improvement.ImprovementType.SkillGroupDisable:
                                if (objImprovement.ImprovedName == SkillGroup &&
                                    !objImprovement.Exclude.Contains(strNameToUse) &&
                                    !objImprovement.Exclude.Contains(SkillCategory))
                                {
                                    lstReturn.Add(objImprovement);
                                    if (blnExitAfterFirst)
                                        return false;
                                }

                                break;

                            case Improvement.ImprovementType.SkillCategory:
                                if (objImprovement.ImprovedName == SkillCategory &&
                                    !objImprovement.Exclude.Contains(strNameToUse))
                                {
                                    lstReturn.Add(objImprovement);
                                    if (blnExitAfterFirst)
                                        return false;
                                }

                                break;

                            case Improvement.ImprovementType.SkillAttribute:
                                if (objImprovement.ImprovedName == strUseAttribute &&
                                    !objImprovement.Exclude.Contains(strNameToUse))
                                {
                                    lstReturn.Add(objImprovement);
                                    if (blnExitAfterFirst)
                                        return false;
                                }

                                break;

                            case Improvement.ImprovementType.SkillLinkedAttribute:
                                if (objImprovement.ImprovedName == Attribute &&
                                    !objImprovement.Exclude.Contains(strNameToUse))
                                {
                                    lstReturn.Add(objImprovement);
                                    if (blnExitAfterFirst)
                                        return false;
                                }

                                break;

                            case Improvement.ImprovementType.BlockSkillDefault:
                                if (objImprovement.ImprovedName == SkillGroup)
                                {
                                    lstReturn.Add(objImprovement);
                                    if (blnExitAfterFirst)
                                        return false;
                                }

                                break;

                            case Improvement.ImprovementType.EnhancedArticulation:
                                if (SkillCategory == "Physical Active" &&
                                    AttributeSection.PhysicalAttributes.Contains(Attribute))
                                {
                                    lstReturn.Add(objImprovement);
                                    if (blnExitAfterFirst)
                                        return false;
                                }

                                break;
                        }

                        return true;
                    }, token: token).ConfigureAwait(false);

                return lstReturn;
            }
        }

        /// <summary>
        /// How much Sp this costs. Price during career mode is undefined
        /// </summary>
        public virtual int CurrentSpCost
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    int cost = BasePoints;
                    if (!IsExoticSkill && !BuyWithKarma)
                        cost += Specializations.Count(x => !x.Free);

                    decimal decExtra = 0;
                    decimal decMultiplier = 1.0m;
                    foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                    {
                        if (objLoopImprovement.Minimum <= BasePoints &&
                            (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                             (objLoopImprovement.Condition == "career") == CharacterObject.Created ||
                             (objLoopImprovement.Condition == "create") != CharacterObject.Created) &&
                            objLoopImprovement.Enabled)
                        {
                            if (objLoopImprovement.ImprovedName == DictionaryKey ||
                                string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.ActiveSkillPointCost:
                                        decExtra += objLoopImprovement.Value *
                                                    (Math.Min(BasePoints,
                                                        objLoopImprovement.Maximum == 0
                                                            ? int.MaxValue
                                                            : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                                        break;

                                    case Improvement.ImprovementType.ActiveSkillPointCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                            else if (objLoopImprovement.ImprovedName == SkillCategory)
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillCategoryPointCost:
                                        decExtra += objLoopImprovement.Value *
                                                    (Math.Min(BasePoints,
                                                        objLoopImprovement.Maximum == 0
                                                            ? int.MaxValue
                                                            : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                                        break;

                                    case Improvement.ImprovementType.SkillCategoryPointCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                        }
                    }

                    if (decMultiplier != 1.0m)
                        cost = (cost * decMultiplier + decExtra).StandardRound();
                    else
                        cost += decExtra.StandardRound();

                    return Math.Max(cost, 0);
                }
            }
        }

        /// <summary>
        /// How much Sp this costs. Price during career mode is undefined
        /// </summary>
        public virtual async ValueTask<int> GetCurrentSpCostAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                int intBasePoints = await GetBasePointsAsync(token).ConfigureAwait(false);
                int cost = intBasePoints;
                if (!IsExoticSkill && !await GetBuyWithKarmaAsync(token).ConfigureAwait(false))
                    cost += await Specializations.CountAsync(x => !x.Free, token: token).ConfigureAwait(false);

                decimal decExtra = 0;
                decimal decMultiplier = 1.0m;
                bool blnCreated = await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false);
                await (await CharacterObject.GetImprovementsAsync(token).ConfigureAwait(false)).ForEachAsync(
                    async objLoopImprovement =>
                    {
                        if (objLoopImprovement.Minimum > intBasePoints ||
                            (!string.IsNullOrEmpty(objLoopImprovement.Condition)
                             && (objLoopImprovement.Condition == "career") != blnCreated
                             && (objLoopImprovement.Condition == "create") == blnCreated)
                            || !objLoopImprovement.Enabled)
                            return;
                        if (objLoopImprovement.ImprovedName == await GetDictionaryKeyAsync(token).ConfigureAwait(false)
                            || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.ActiveSkillPointCost:
                                    decExtra += objLoopImprovement.Value
                                                * (Math.Min(
                                                    intBasePoints,
                                                    objLoopImprovement.Maximum == 0
                                                        ? int.MaxValue
                                                        : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                                    break;

                                case Improvement.ImprovementType.ActiveSkillPointCostMultiplier:
                                    decMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
                        else if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.SkillCategoryPointCost:
                                    decExtra += objLoopImprovement.Value
                                                * (Math.Min(
                                                    intBasePoints,
                                                    objLoopImprovement.Maximum == 0
                                                        ? int.MaxValue
                                                        : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                                    break;

                                case Improvement.ImprovementType.SkillCategoryPointCostMultiplier:
                                    decMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
                    }, token: token).ConfigureAwait(false);

                if (decMultiplier != 1.0m)
                    cost = (cost * decMultiplier + decExtra).StandardRound();
                else
                    cost += decExtra.StandardRound();

                return Math.Max(cost, 0);
            }
        }

        /// <summary>
        /// How much karma this costs. Return value during career mode is undefined
        /// </summary>
        public virtual int CurrentKarmaCost
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    //No rating can obv not cost anything
                    //Makes debugging easier as we often only care about value calculation
                    int intTotalBaseRating = TotalBaseRating;
                    if (intTotalBaseRating == 0) return 0;

                    int intCost;
                    int intLower;
                    if (SkillGroupObject?.Karma > 0)
                    {
                        int intGroupUpper =
                            SkillGroupObject.SkillList.Min(x => x.Base + x.Karma + x.RatingModifiers(x.Attribute));
                        int intGroupLower = intGroupUpper - SkillGroupObject.Karma;

                        intLower = Base + FreeKarma + RatingModifiers(Attribute); //Might be an error here

                        intCost = RangeCost(intLower, intGroupLower) + RangeCost(intGroupUpper, intTotalBaseRating);
                    }
                    else
                    {
                        intLower = Base + FreeKarma + RatingModifiers(Attribute);

                        intCost = RangeCost(intLower, intTotalBaseRating);
                    }

                    //Don't think this is going to happen, but if it happens i want to know
                    if (intCost < 0)
                        Utils.BreakIfDebug();

                    int intSpecCount = BuyWithKarma || !CharacterObject.EffectiveBuildMethodUsesPriorityTables
                        ? Specializations.Count(objSpec => !objSpec.Free)
                        : 0;
                    int intSpecCost = intSpecCount * CharacterObject.Settings.KarmaSpecialization;
                    decimal decExtraSpecCost = 0;
                    decimal decSpecCostMultiplier = 1.0m;
                    foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                    {
                        if (objLoopImprovement.Minimum <= intTotalBaseRating &&
                            (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                             (objLoopImprovement.Condition == "career") == CharacterObject.Created ||
                             (objLoopImprovement.Condition == "create") != CharacterObject.Created) &&
                            objLoopImprovement.Enabled)
                        {
                            if (objLoopImprovement.ImprovedName != SkillCategory)
                                continue;
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.SkillCategorySpecializationKarmaCost:
                                    decExtraSpecCost += objLoopImprovement.Value * intSpecCount;
                                    break;

                                case Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier:
                                    decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
                    }

                    if (decSpecCostMultiplier != 1.0m)
                        intSpecCost = (intSpecCost * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
                    else
                        intSpecCost += decExtraSpecCost.StandardRound(); //Spec
                    intCost += intSpecCost;

                    return Math.Max(0, intCost);
                }
            }
        }

        /// <summary>
        /// How much karma this costs. Return value during career mode is undefined
        /// </summary>
        public virtual async ValueTask<int> GetCurrentKarmaCostAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                //No rating can obv not cost anything
                //Makes debugging easier as we often only care about value calculation
                int intTotalBaseRating = await GetTotalBaseRatingAsync(token).ConfigureAwait(false);
                if (intTotalBaseRating == 0)
                    return 0;

                int intCost;
                int intLower;
                if (SkillGroupObject != null && await SkillGroupObject.GetKarmaAsync(token).ConfigureAwait(false) > 0)
                {
                    int intGroupUpper
                        = await SkillGroupObject.SkillList.MinAsync(
                                async x => await x.GetBaseAsync(token).ConfigureAwait(false) +
                                           await x.GetKarmaAsync(token).ConfigureAwait(false)
                                           + await x.RatingModifiersAsync(
                                               x.Attribute, token: token).ConfigureAwait(false), token: token)
                            .ConfigureAwait(false);
                    int intGroupLower =
                        intGroupUpper - await SkillGroupObject.GetKarmaAsync(token).ConfigureAwait(false);

                    intLower = await GetBaseAsync(token).ConfigureAwait(false) +
                               await GetFreeKarmaAsync(token).ConfigureAwait(false) +
                               await RatingModifiersAsync(Attribute, token: token)
                                   .ConfigureAwait(false); //Might be an error here

                    intCost = await RangeCostAysnc(intLower, intGroupLower, token).ConfigureAwait(false) +
                              await RangeCostAysnc(intGroupUpper, intTotalBaseRating, token).ConfigureAwait(false);
                }
                else
                {
                    intLower = await GetBaseAsync(token).ConfigureAwait(false) +
                               await GetFreeKarmaAsync(token).ConfigureAwait(false) +
                               await RatingModifiersAsync(Attribute, token: token).ConfigureAwait(false);

                    intCost = await RangeCostAysnc(intLower, intTotalBaseRating, token).ConfigureAwait(false);
                }

                //Don't think this is going to happen, but if it happens i want to know
                if (intCost < 0)
                    Utils.BreakIfDebug();

                int intSpecCount = await GetBuyWithKarmaAsync(token).ConfigureAwait(false)
                                   || !await CharacterObject.GetEffectiveBuildMethodUsesPriorityTablesAsync(token).ConfigureAwait(false)
                    ? await Specializations.CountAsync(objSpec => !objSpec.Free, token: token).ConfigureAwait(false)
                    : 0;
                int intSpecCost = intSpecCount *
                                  await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false))
                                      .GetKarmaSpecializationAsync(token).ConfigureAwait(false);
                decimal decExtraSpecCost = 0;
                decimal decSpecCostMultiplier = 1.0m;
                bool blnCreated = await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false);
                await (await CharacterObject.GetImprovementsAsync(token).ConfigureAwait(false)).ForEachAsync(
                    objLoopImprovement =>
                    {
                        if (objLoopImprovement.Minimum > intTotalBaseRating ||
                            (!string.IsNullOrEmpty(objLoopImprovement.Condition)
                             && (objLoopImprovement.Condition == "career") != blnCreated
                             && (objLoopImprovement.Condition == "create") == blnCreated)
                            || !objLoopImprovement.Enabled)
                            return;
                        if (objLoopImprovement.ImprovedName != SkillCategory)
                            return;
                        switch (objLoopImprovement.ImproveType)
                        {
                            case Improvement.ImprovementType.SkillCategorySpecializationKarmaCost:
                                decExtraSpecCost += objLoopImprovement.Value * intSpecCount;
                                break;

                            case Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier:
                                decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                                break;
                        }
                    }, token: token).ConfigureAwait(false);

                if (decSpecCostMultiplier != 1.0m)
                    intSpecCost = (intSpecCost * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
                else
                    intSpecCost += decExtraSpecCost.StandardRound(); //Spec
                intCost += intSpecCost;

                return Math.Max(0, intCost);
            }
        }

        /// <summary>
        /// The total, general purpose dice pool for this skill
        /// </summary>
        public int Pool
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return PoolOtherAttribute(Attribute);
            }
        }

        public ValueTask<int> GetPoolAsync(CancellationToken token = default)
        {
            return PoolOtherAttributeAsync(Attribute, token: token);
        }

        public bool Leveled
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Rating > 0;
            }
        }

        public async ValueTask<bool> GetLeveledAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return await GetRatingAsync(token).ConfigureAwait(false) > 0;
        }

        public Color PreferredControlColor
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Leveled && Enabled ? ColorManager.Control : ColorManager.ControlLighter;
            }
        }

        public async ValueTask<Color> GetPreferredControlColorAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return await GetLeveledAsync(token).ConfigureAwait(false)
                       && await GetEnabledAsync(token).ConfigureAwait(false)
                    ? await ColorManager.GetControlAsync(token).ConfigureAwait(false)
                    : await ColorManager.GetControlLighterAsync(token).ConfigureAwait(false);
        }

        private int _intCachedCanHaveSpecs = -1;

        public bool CanHaveSpecs
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if ((this as KnowledgeSkill)?.AllowUpgrade == false)
                        return false;
                    if (_intCachedCanHaveSpecs >= 0)
                        return _intCachedCanHaveSpecs > 0;
                    using (LockObject.EnterWriteLock())
                    {
                        if (_intCachedCanHaveSpecs >= 0) // Just in case
                            return _intCachedCanHaveSpecs > 0;
                        if (!Enabled)
                        {
                            _intCachedCanHaveSpecs = 0;
                            return _intCachedCanHaveSpecs > 0;
                        }

                        _intCachedCanHaveSpecs = (!IsExoticSkill && TotalBaseRating > 0 && KarmaUnlocked &&
                                                  !(ImprovementManager
                                                    .GetCachedImprovementListForValueOf(
                                                        CharacterObject,
                                                        Improvement.ImprovementType.BlockSkillSpecializations,
                                                        DictionaryKey, true).Count > 0
                                                    || ImprovementManager
                                                       .GetCachedImprovementListForValueOf(
                                                           CharacterObject,
                                                           Improvement.ImprovementType
                                                                      .BlockSkillCategorySpecializations,
                                                           SkillCategory).Count > 0)).ToInt32();
                        if (_intCachedCanHaveSpecs <= 0 && Specializations.Count > 0)
                        {
                            Specializations.Clear();
                        }

                        return _intCachedCanHaveSpecs > 0;
                    }
                }
            }
        }

        public async ValueTask<bool> GetCanHaveSpecsAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if ((this as KnowledgeSkill)?.AllowUpgrade == false)
                    return false;
                if (_intCachedCanHaveSpecs >= 0)
                    return _intCachedCanHaveSpecs > 0;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    if (_intCachedCanHaveSpecs >= 0) // Just in case
                        return _intCachedCanHaveSpecs > 0;
                    if (!Enabled)
                    {
                        _intCachedCanHaveSpecs = 0;
                        return _intCachedCanHaveSpecs > 0;
                    }

                    _intCachedCanHaveSpecs = (!IsExoticSkill
                                              && await GetTotalBaseRatingAsync(token).ConfigureAwait(false) > 0
                                              && await GetKarmaUnlockedAsync(token).ConfigureAwait(false) &&
                                              !((await ImprovementManager
                                                       .GetCachedImprovementListForValueOfAsync(
                                                           CharacterObject,
                                                           Improvement.ImprovementType.BlockSkillSpecializations,
                                                           DictionaryKey, true, token).ConfigureAwait(false)).Count > 0
                                                || (await ImprovementManager
                                                          .GetCachedImprovementListForValueOfAsync(
                                                              CharacterObject,
                                                              Improvement.ImprovementType
                                                                         .BlockSkillCategorySpecializations,
                                                              SkillCategory, token: token).ConfigureAwait(false)).Count
                                                > 0)).ToInt32();
                    if (_intCachedCanHaveSpecs <= 0 && Specializations.Count > 0)
                    {
                        await Specializations.ClearAsync(token).ConfigureAwait(false);
                    }

                    return _intCachedCanHaveSpecs > 0;
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        public Character CharacterObject { get; }

        //TODO change to the actual characterattribute object
        /// <summary>
        /// The Abbreviation of the linked attribute. Not the object due legacy
        /// </summary>
        public string Attribute
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return AttributeObject.Abbrev;
            }
        }

        public string DefaultAttribute
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strDefaultAttribute;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_strDefaultAttribute == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _strDefaultAttribute, value) == value)
                            return;
                        if (CharacterObject?.SkillsSection?.IsLoading != true)
                            OnPropertyChanged();
                        else
                            RecacheAttribute();
                    }
                }
            }
        }

        public async ValueTask<string> GetDefaultAttributeAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _strDefaultAttribute;
        }

        public async ValueTask SetDefaultAttributeAsync(string value, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_strDefaultAttribute == value)
                    return;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    if (Interlocked.Exchange(ref _strDefaultAttribute, value) == value)
                        return;
                    if (CharacterObject?.SkillsSection?.IsLoading != true)
                        OnPropertyChanged(nameof(DefaultAttribute));
                    else
                        await RecacheAttributeAsync(token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// The translated abbreviation of the linked attribute.
        /// </summary>
        public string DisplayAttributeMethod(string strLanguage)
        {
            using (EnterReadLock.Enter(LockObject))
                return LanguageManager.GetString("String_Attribute" + Attribute + "Short", strLanguage);
        }

        /// <summary>
        /// The translated abbreviation of the linked attribute.
        /// </summary>
        public async ValueTask<string> DisplayAttributeMethodAsync(string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return await LanguageManager.GetStringAsync("String_Attribute" + Attribute + "Short", strLanguage,
                    token: token).ConfigureAwait(false);
        }

        public string DisplayAttribute => DisplayAttributeMethod(GlobalSettings.Language);

        public ValueTask<string> GetDisplayAttributeAsync(CancellationToken token = default) =>
            DisplayAttributeMethodAsync(GlobalSettings.Language, token);

        private int _intCachedEnabled = -1;

        private bool _blnForceDisabled;

        //TODO handle aspected/adepts who cannot (always) get magic skills
        public bool Enabled
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intCachedEnabled >= 0)
                        return _intCachedEnabled > 0;

                    using (LockObject.EnterWriteLock())
                    {
                        if (_intCachedEnabled >= 0) // Just in case
                            return _intCachedEnabled > 0;
                        if (_blnForceDisabled)
                        {
                            _intCachedEnabled = 0;
                            return false;
                        }

                        if (ImprovementManager
                                .GetCachedImprovementListForValueOf(CharacterObject,
                                    Improvement.ImprovementType.SkillDisable,
                                    DictionaryKey).Count > 0)
                        {
                            _intCachedEnabled = 0;
                            return false;
                        }

                        if (RequiresFlyMovement)
                        {
                            string strMovementString = CharacterObject.GetFly(GlobalSettings.InvariantCultureInfo,
                                GlobalSettings.DefaultLanguage);
                            if (string.IsNullOrEmpty(strMovementString)
                                || strMovementString == "0"
                                || strMovementString.Contains(LanguageManager.GetString("String_ModeSpecial",
                                    GlobalSettings.DefaultLanguage)))
                            {
                                _intCachedEnabled = 0;
                                return false;
                            }
                        }

                        if (RequiresSwimMovement)
                        {
                            string strMovementString = CharacterObject.GetSwim(GlobalSettings.InvariantCultureInfo,
                                GlobalSettings.DefaultLanguage);
                            if (string.IsNullOrEmpty(strMovementString)
                                || strMovementString == "0"
                                || strMovementString.Contains(LanguageManager.GetString("String_ModeSpecial",
                                    GlobalSettings.DefaultLanguage)))
                            {
                                _intCachedEnabled = 0;
                                return false;
                            }
                        }

                        if (RequiresGroundMovement)
                        {
                            string strMovementString = CharacterObject.GetMovement(GlobalSettings.InvariantCultureInfo,
                                GlobalSettings.DefaultLanguage);
                            if (string.IsNullOrEmpty(strMovementString)
                                || strMovementString == "0"
                                || strMovementString.Contains(LanguageManager.GetString("String_ModeSpecial",
                                    GlobalSettings.DefaultLanguage)))
                            {
                                _intCachedEnabled = 0;
                                return false;
                            }
                        }

                        //TODO: This is a temporary workaround until proper support for selectively enabling or disabling skills works, as above.
                        switch (Attribute)
                        {
                            case "MAG":
                            case "MAGAdept":
                                _intCachedEnabled = CharacterObject.MAGEnabled.ToInt32();
                                break;

                            case "RES":
                                _intCachedEnabled = CharacterObject.RESEnabled.ToInt32();
                                break;

                            case "DEP":
                                _intCachedEnabled = CharacterObject.DEPEnabled.ToInt32();
                                break;

                            default:
                                _intCachedEnabled = 1;
                                break;
                        }

                        return _intCachedEnabled > 0;
                    }
                }
            }
        }

        //TODO handle aspected/adepts who cannot (always) get magic skills
        public async ValueTask<bool> GetEnabledAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_intCachedEnabled >= 0)
                    return _intCachedEnabled > 0;

                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    if (_intCachedEnabled >= 0) // Just in case
                        return _intCachedEnabled > 0;
                    if (_blnForceDisabled)
                    {
                        _intCachedEnabled = 0;
                        return false;
                    }

                    if ((await ImprovementManager
                            .GetCachedImprovementListForValueOfAsync(CharacterObject,
                                Improvement.ImprovementType.SkillDisable,
                                await GetDictionaryKeyAsync(token).ConfigureAwait(false), token: token)
                            .ConfigureAwait(false)).Count > 0)
                    {
                        _intCachedEnabled = 0;
                        return false;
                    }

                    if (RequiresFlyMovement)
                    {
                        string strMovementString
                            = await CharacterObject
                                .GetFlyAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage, token)
                                .ConfigureAwait(false);
                        if (string.IsNullOrEmpty(strMovementString)
                            || strMovementString == "0"
                            || strMovementString.Contains(
                                await LanguageManager
                                    .GetStringAsync("String_ModeSpecial", GlobalSettings.DefaultLanguage, token: token)
                                    .ConfigureAwait(false)))
                        {
                            _intCachedEnabled = 0;
                            return false;
                        }
                    }

                    if (RequiresSwimMovement)
                    {
                        string strMovementString
                            = await CharacterObject
                                .GetSwimAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage,
                                    token)
                                .ConfigureAwait(false);
                        if (string.IsNullOrEmpty(strMovementString)
                            || strMovementString == "0"
                            || strMovementString.Contains(
                                await LanguageManager
                                    .GetStringAsync("String_ModeSpecial", GlobalSettings.DefaultLanguage, token: token)
                                    .ConfigureAwait(false)))
                        {
                            _intCachedEnabled = 0;
                            return false;
                        }
                    }

                    if (RequiresGroundMovement)
                    {
                        string strMovementString
                            = await CharacterObject
                                .GetMovementAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage,
                                    token).ConfigureAwait(false);
                        if (string.IsNullOrEmpty(strMovementString)
                            || strMovementString == "0"
                            || strMovementString.Contains(
                                await LanguageManager
                                    .GetStringAsync("String_ModeSpecial", GlobalSettings.DefaultLanguage, token: token)
                                    .ConfigureAwait(false)))
                        {
                            _intCachedEnabled = 0;
                            return false;
                        }
                    }

                    //TODO: This is a temporary workaround until proper support for selectively enabling or disabling skills works, as above.
                    switch (Attribute)
                    {
                        case "MAG":
                        case "MAGAdept":
                            _intCachedEnabled = (await CharacterObject.GetMAGEnabledAsync(token).ConfigureAwait(false))
                                .ToInt32();
                            break;

                        case "RES":
                            _intCachedEnabled = (await CharacterObject.GetRESEnabledAsync(token).ConfigureAwait(false))
                                .ToInt32();
                            break;

                        case "DEP":
                            _intCachedEnabled = (await CharacterObject.GetDEPEnabledAsync(token).ConfigureAwait(false))
                                .ToInt32();
                            break;

                        default:
                            _intCachedEnabled = 1;
                            break;
                    }

                    return _intCachedEnabled > 0;
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        public bool ForceDisabled
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _blnForceDisabled;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_blnForceDisabled == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnForceDisabled = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public bool RequiresGroundMovement
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _blnRequiresGroundMovement;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_blnRequiresGroundMovement == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnRequiresGroundMovement = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public bool RequiresSwimMovement
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _blnRequiresSwimMovement;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_blnRequiresSwimMovement == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnRequiresSwimMovement = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public bool RequiresFlyMovement
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _blnRequiresFlyMovement;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_blnRequiresFlyMovement == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnRequiresFlyMovement = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        private int _intCachedCanUpgradeCareer = -1;

        public bool CanUpgradeCareer
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intCachedCanUpgradeCareer < 0)
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            if (_intCachedCanUpgradeCareer < 0)
                                _intCachedCanUpgradeCareer = (CharacterObject.Karma >= UpgradeKarmaCost &&
                                                              RatingMaximum > TotalBaseRating).ToInt32();
                        }
                    }

                    return _intCachedCanUpgradeCareer > 0;
                }
            }
        }

        public async ValueTask<bool> GetCanUpgradeCareerAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_intCachedCanUpgradeCareer < 0)
                {
                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        if (_intCachedCanUpgradeCareer < 0)
                            _intCachedCanUpgradeCareer = (await CharacterObject.GetKarmaAsync(token)
                                                                               .ConfigureAwait(false)
                                                          >= await GetUpgradeKarmaCostAsync(token).ConfigureAwait(false)
                                                          &&
                                                          await GetRatingMaximumAsync(token).ConfigureAwait(false)
                                                          > await GetTotalBaseRatingAsync(token).ConfigureAwait(false))
                                .ToInt32();
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }

                return _intCachedCanUpgradeCareer == 1;
            }
        }

        public virtual bool AllowDelete => false;

#pragma warning disable CS1998
        public virtual async ValueTask<bool> GetAllowDeleteAsync(CancellationToken token = default)
#pragma warning restore CS1998
        {
            token.ThrowIfCancellationRequested();
            return false;
        }

        public bool Default
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    // SR5 400 : Critters that don't have the Sapience Power are unable to default in skills they don't possess.
                    List<Improvement> lstAllowSkillDefaultImprovements = ImprovementManager
                        .GetCachedImprovementListForValueOf(
                            CharacterObject,
                            Improvement.ImprovementType.AllowSkillDefault,
                            DictionaryKey, true);
                    if (CharacterObject.IsCritter && Rating == 0 && lstAllowSkillDefaultImprovements.Count == 0)
                    {
                        return false;
                    }

                    if (!_blnDefault && lstAllowSkillDefaultImprovements.Count == 0)
                        return false;

                    return ImprovementManager
                        .GetCachedImprovementListForValueOf(
                            CharacterObject,
                            Improvement.ImprovementType.BlockSkillDefault,
                            DictionaryKey, true).All(x => SkillGroup != x.ImprovedName);
                }
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_blnDefault == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnDefault = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public async ValueTask<bool> GetDefaultAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                string strKey = await GetDictionaryKeyAsync(token).ConfigureAwait(false);
                // SR5 400 : Critters that don't have the Sapience Power are unable to default in skills they don't possess.
                List<Improvement> lstAllowSkillDefaultImprovements = await ImprovementManager
                    .GetCachedImprovementListForValueOfAsync(
                        CharacterObject,
                        Improvement.ImprovementType.AllowSkillDefault,
                        strKey, true, token).ConfigureAwait(false);
                if (CharacterObject.IsCritter && lstAllowSkillDefaultImprovements.Count == 0 &&
                    await GetRatingAsync(token).ConfigureAwait(false) == 0)
                {
                    return false;
                }

                if (!_blnDefault && lstAllowSkillDefaultImprovements.Count == 0)
                    return false;

                return (await ImprovementManager
                    .GetCachedImprovementListForValueOfAsync(
                        CharacterObject,
                        Improvement.ImprovementType.BlockSkillDefault,
                        strKey, true, token).ConfigureAwait(false)).All(x => SkillGroup != x.ImprovedName);
            }
        }

        public virtual bool IsExoticSkill => false;

        public virtual bool IsKnowledgeSkill => false;

        public virtual bool AllowNameChange => false;

#pragma warning disable CS1998
        public virtual async ValueTask<bool> GetAllowNameChangeAsync(CancellationToken token = default)
#pragma warning restore CS1998
        {
            token.ThrowIfCancellationRequested();
            return false;
        }

        public virtual bool AllowTypeChange => false;

#pragma warning disable CS1998
        public virtual async ValueTask<bool> GetAllowTypeChangeAsync(CancellationToken token = default)
#pragma warning restore CS1998
        {
            token.ThrowIfCancellationRequested();
            return false;
        }

        public virtual bool IsLanguage => false;

#pragma warning disable CS1998
        public virtual async ValueTask<bool> GetIsLanguageAsync(CancellationToken token = default)
#pragma warning restore CS1998
        {
            token.ThrowIfCancellationRequested();
            return false;
        }

        public virtual bool IsNativeLanguage
        {
            get => false;
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                // Dummy setter that is only set up so that Language skills can have a setter that is functional
            }
        }

#pragma warning disable CS1998
        public virtual async ValueTask<bool> GetIsNativeLanguageAsync(CancellationToken token = default)
#pragma warning restore CS1998
        {
            token.ThrowIfCancellationRequested();
            return false;
        }

#pragma warning disable CS1998
        public virtual async ValueTask SetIsNativeLanguageAsync(bool value, CancellationToken token = default)
#pragma warning restore CS1998
        {
            token.ThrowIfCancellationRequested();
        }

        private string _strDictionaryKey;

        public string DictionaryKey
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    return _strDictionaryKey = _strDictionaryKey
                                               ?? (IsExoticSkill
                                                   ? Name + " (" +
                                                     DisplaySpecialization(GlobalSettings.DefaultLanguage) +
                                                     ')'
                                                   : Name);
                }
            }
        }

        public async ValueTask<string> GetDictionaryKeyAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                return _strDictionaryKey = _strDictionaryKey
                                           ?? (IsExoticSkill
                                               ? await GetNameAsync(token).ConfigureAwait(false) + " (" +
                                                 await DisplaySpecializationAsync(GlobalSettings.DefaultLanguage, token)
                                                     .ConfigureAwait(false) + ')'
                                               : await GetNameAsync(token).ConfigureAwait(false));
            }
        }

        public string Name
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strName;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_strName == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _strName, value) == value)
                            return;
                        _strDictionaryKey = null;
                        _intCachedFreeBase = int.MinValue;
                        _intCachedFreeKarma = int.MinValue;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public async ValueTask<string> GetNameAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _strName;
        }

        public async ValueTask SetNameAsync(string value, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_strName == value)
                    return;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    if (Interlocked.Exchange(ref _strName, value) == value)
                        return;
                    _strDictionaryKey = null;
                    _intCachedFreeBase = int.MinValue;
                    _intCachedFreeKarma = int.MinValue;
                    OnPropertyChanged(nameof(Name));
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        //TODO RENAME DESCRIPTIVE
        private Guid _guidInternalId = Guid.NewGuid();

        /// <summary>
        /// The Unique ID for this skill. This is unique and never repeating
        /// </summary>
        public Guid Id
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _guidInternalId;
            }
            private set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_guidInternalId == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _guidInternalId = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public string InternalId
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _guidInternalId.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        public void CopyInternalId(Skill objOtherSkill)
        {
            using (EnterReadLock.Enter(objOtherSkill.LockObject))
                Id = objOtherSkill.Id;
        }

        private Guid _guidSkillId = Guid.Empty;

        /// <summary>
        /// The ID for this skill. This is persistent for active skills over
        /// multiple characters, ?and predefined knowledge skills,? but not
        /// for skills where the user supplies a name (Exotic and Knowledge)
        /// </summary>
        public Guid SkillId
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _guidSkillId;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_guidSkillId == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _guidSkillId = value;
                        _objCachedMyXmlNode = null;
                        _objCachedMyXPathNode = null;
                        _blnRecalculateCachedSuggestedSpecializations = true;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// The ID for this skill. This is persistent for active skills over
        /// multiple characters, ?and predefined knowledge skills,? but not
        /// for skills where the user supplies a name (Exotic and Knowledge)
        /// </summary>
        public async ValueTask<Guid> GetSkillIdAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _guidSkillId;
        }

        /// <summary>
        /// The ID for this skill. This is persistent for active skills over
        /// multiple characters, ?and predefined knowledge skills,? but not
        /// for skills where the user supplies a name (Exotic and Knowledge)
        /// </summary>
        public async ValueTask SetSkillIdAsync(Guid value, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_guidSkillId == value)
                    return;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    _guidSkillId = value;
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                    _blnRecalculateCachedSuggestedSpecializations = true;
                    OnPropertyChanged(nameof(SkillId));
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        public string SkillGroup { get; } = string.Empty;

        public virtual string SkillCategory { get; } = string.Empty;

        private bool _blnRecalculateCachedSuggestedSpecializations = true;

        private List<ListItem> _lstCachedSuggestedSpecializations;

        // ReSharper disable once InconsistentNaming
        public IReadOnlyList<ListItem> CGLSpecializations
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_blnRecalculateCachedSuggestedSpecializations)
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            if (_blnRecalculateCachedSuggestedSpecializations) // Just in case
                            {
                                _blnRecalculateCachedSuggestedSpecializations = false;
                                if (_lstCachedSuggestedSpecializations == null)
                                    _lstCachedSuggestedSpecializations = Utils.ListItemListPool.Get();
                                else
                                    _lstCachedSuggestedSpecializations.Clear();
                                XPathNodeIterator xmlSpecList =
                                    this.GetNodeXPath(GlobalSettings.Language)?.SelectAndCacheExpression("specs/spec");
                                if (xmlSpecList?.Count > 0)
                                {
                                    foreach (XPathNavigator xmlSpecNode in xmlSpecList)
                                    {
                                        string strInnerText = xmlSpecNode.Value;
                                        if (string.IsNullOrEmpty(strInnerText))
                                            continue;
                                        _lstCachedSuggestedSpecializations.Add(
                                            new ListItem(strInnerText,
                                                xmlSpecNode.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strInnerText));
                                    }
                                }

                                foreach (string strSpecializationName in ImprovementManager
                                             .GetCachedImprovementListForValueOf(
                                                 CharacterObject, Improvement.ImprovementType.SkillSpecializationOption,
                                                 DictionaryKey).Select(x => x.UniqueName))
                                {
                                    if (_lstCachedSuggestedSpecializations.Any(
                                            y => y.Value?.ToString() == strSpecializationName))
                                        continue;
                                    _lstCachedSuggestedSpecializations.Add(
                                        new ListItem(strSpecializationName,
                                            CharacterObject.TranslateExtra(strSpecializationName)));
                                }

                                _lstCachedSuggestedSpecializations.Sort(CompareListItems.CompareNames);
                            }
                        }
                    }

                    return _lstCachedSuggestedSpecializations;
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        public async ValueTask<IReadOnlyList<ListItem>> GetCGLSpecializationsAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_blnRecalculateCachedSuggestedSpecializations)
                {
                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        if (_blnRecalculateCachedSuggestedSpecializations) // Just in case
                        {
                            _blnRecalculateCachedSuggestedSpecializations = false;
                            if (_lstCachedSuggestedSpecializations == null)
                                _lstCachedSuggestedSpecializations = Utils.ListItemListPool.Get();
                            else
                                _lstCachedSuggestedSpecializations.Clear();
                            XPathNavigator objBaseNode = await this
                                                               .GetNodeXPathAsync(GlobalSettings.Language, token: token)
                                                               .ConfigureAwait(false);
                            XPathNodeIterator xmlSpecList = objBaseNode != null
                                ? await objBaseNode.SelectAndCacheExpressionAsync("specs/spec", token).ConfigureAwait(false)
                                : null;
                            if (xmlSpecList?.Count > 0)
                            {
                                foreach (XPathNavigator xmlSpecNode in xmlSpecList)
                                {
                                    string strInnerText = xmlSpecNode.Value;
                                    if (string.IsNullOrEmpty(strInnerText))
                                        continue;
                                    _lstCachedSuggestedSpecializations.Add(
                                        new ListItem(strInnerText,
                                                     (await xmlSpecNode.SelectSingleNodeAndCacheExpressionAsync("@translate", token).ConfigureAwait(false))?.Value
                                                     ?? strInnerText));
                                }
                            }

                            foreach (string strSpecializationName in (await ImprovementManager
                                                                            .GetCachedImprovementListForValueOfAsync(
                                                                                CharacterObject,
                                                                                Improvement.ImprovementType
                                                                                    .SkillSpecializationOption,
                                                                                await GetDictionaryKeyAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false)).Select(x => x.UniqueName))
                            {
                                if (_lstCachedSuggestedSpecializations.Any(
                                        y => y.Value?.ToString() == strSpecializationName))
                                    continue;
                                _lstCachedSuggestedSpecializations.Add(
                                    new ListItem(strSpecializationName,
                                                 await CharacterObject.TranslateExtraAsync(strSpecializationName, token: token).ConfigureAwait(false)));
                            }

                            _lstCachedSuggestedSpecializations.Sort(CompareListItems.CompareNames);
                        }
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }

                return _lstCachedSuggestedSpecializations;
            }
        }

        private readonly Dictionary<string, string> _dicCachedStringSpec = new Dictionary<string, string>();

        public virtual string DisplaySpecialization(string strLanguage)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (_dicCachedStringSpec.TryGetValue(strLanguage, out string strReturn))
                    return strReturn;
                strReturn = string.Join(", ", Specializations.Select(x => x.DisplayName(strLanguage)));

                _dicCachedStringSpec.Add(strLanguage, strReturn);

                return strReturn;
            }
        }

        public virtual async ValueTask<string> DisplaySpecializationAsync(string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_dicCachedStringSpec.TryGetValue(strLanguage, out string strReturn))
                    return strReturn;
                strReturn = await StringExtensions
                    .JoinAsync(", ", Specializations.Select(x => x.DisplayNameAsync(strLanguage, token).AsTask()), token)
                    .ConfigureAwait(false);

                _dicCachedStringSpec.Add(strLanguage, strReturn);

                return strReturn;
            }
        }

        public string CurrentDisplaySpecialization => DisplaySpecialization(GlobalSettings.Language);

        public ValueTask<string> GetCurrentDisplaySpecializationAsync(CancellationToken token = default) => DisplaySpecializationAsync(GlobalSettings.Language, token);

        private readonly ThreadSafeObservableCollection<SkillSpecialization> _lstSpecializations = new ThreadSafeObservableCollection<SkillSpecialization>();

        //TODO A unit test here?, I know we don't have them, but this would be improved by some
        //Or just ignore support for multiple specializations even if the rules say it is possible?
        public ThreadSafeObservableCollection<SkillSpecialization> Specializations
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _lstSpecializations;
            }
        }

        public string TopMostDisplaySpecialization
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (IsExoticSkill)
                    {
                        return ((ExoticSkill)this).DisplaySpecific(GlobalSettings.Language);
                    }

                    return Specializations.FirstOrDefault(x => !x.Free)?.CurrentDisplayName ?? string.Empty;
                }
            }
            set
            {
                using (LockObject.EnterWriteLock())
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        Specializations.RemoveAll(x => !x.Free);
                        return;
                    }

                    int intIndexToReplace = Specializations.FindIndex(x => !x.Free);
                    if (intIndexToReplace < 0)
                    {
                        Specializations.AddWithSort(new SkillSpecialization(CharacterObject, value), (x, y) =>
                        {
                            if (x.Free != y.Free)
                                return x.Free ? 1 : -1;
                            if (x.Expertise != y.Expertise)
                                return x.Expertise ? 1 : -1;
                            return 0;
                        });
                        return;
                    }

                    Specializations[intIndexToReplace] = new SkillSpecialization(CharacterObject, value);
                    // For safety's, remove all non-free specializations after the one we are replacing.
                    intIndexToReplace = Specializations.FindIndex(intIndexToReplace + 1, x => !x.Free);
                    if (intIndexToReplace > 0)
                        Utils.BreakIfDebug(); // This shouldn't happen under normal operations because chargen can only ever have one player-picked specialization at a time
                    while (intIndexToReplace > 0)
                    {
                        Specializations.RemoveAt(intIndexToReplace);
                        intIndexToReplace = Specializations.FindIndex(intIndexToReplace + 1, x => !x.Free);
                    }
                }
            }
        }

        public async ValueTask<string> GetTopMostDisplaySpecializationAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (IsExoticSkill)
                {
                    return await ((ExoticSkill) this).DisplaySpecificAsync(GlobalSettings.Language, token).ConfigureAwait(false);
                }

                SkillSpecialization objSpec = await Specializations.FirstOrDefaultAsync(x => !x.Free, token: token).ConfigureAwait(false);
                return objSpec != null ? await objSpec.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) : string.Empty;
            }
        }

        public async ValueTask SetTopMostDisplaySpecializationAsync(string value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    await Specializations.RemoveAllAsync(x => !x.Free, token: token).ConfigureAwait(false);
                    return;
                }

                IAsyncDisposable objLocker2 = await Specializations.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    int intIndexToReplace = await Specializations.FindIndexAsync(x => !x.Free, token).ConfigureAwait(false);
                    if (intIndexToReplace < 0)
                    {
                        await Specializations.AddWithSortAsync(new SkillSpecialization(CharacterObject, value),
                                                               (x, y) =>
                                                               {
                                                                   if (x.Free != y.Free)
                                                                       return x.Free ? 1 : -1;
                                                                   if (x.Expertise != y.Expertise)
                                                                       return x.Expertise ? 1 : -1;
                                                                   return 0;
                                                               }, token: token).ConfigureAwait(false);
                        return;
                    }

                    await Specializations.SetValueAtAsync(intIndexToReplace,
                                                          new SkillSpecialization(CharacterObject, value), token).ConfigureAwait(false);
                    // For safety's, remove all non-free specializations after the one we are replacing.
                    intIndexToReplace
                        = await Specializations.FindIndexAsync(intIndexToReplace + 1, x => !x.Free, token: token).ConfigureAwait(false);
                    if (intIndexToReplace > 0)
                        Utils.BreakIfDebug(); // This shouldn't happen under normal operations because chargen can only ever have one player-picked specialization at a time
                    while (intIndexToReplace > 0)
                    {
                        await Specializations.RemoveAtAsync(intIndexToReplace, token).ConfigureAwait(false);
                        intIndexToReplace
                            = await Specializations.FindIndexAsync(intIndexToReplace + 1, x => !x.Free, token).ConfigureAwait(false);
                    }
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

        public bool HasSpecialization(string strSpecialization)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (IsExoticSkill)
                {
                    return ((ExoticSkill)this).Specific == strSpecialization;
                }

                return Specializations.Any(
                           x => x.Name == strSpecialization || x.CurrentDisplayName == strSpecialization)
                       && ImprovementManager.GetCachedImprovementListForValueOf(CharacterObject,
                           Improvement.ImprovementType.DisableSpecializationEffects, DictionaryKey).Count == 0;
            }
        }

        public async ValueTask<bool> HasSpecializationAsync(string strSpecialization, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (IsExoticSkill)
                {
                    return ((ExoticSkill)this).Specific == strSpecialization;
                }

                return await Specializations.AnyAsync(
                               async x => x.Name == strSpecialization || await x.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) == strSpecialization,
                               token: token)
                           .ConfigureAwait(false)
                       && (await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                               CharacterObject, Improvement.ImprovementType.DisableSpecializationEffects,
                               await GetDictionaryKeyAsync(token).ConfigureAwait(false), token: token)
                           .ConfigureAwait(false)).Count == 0;
            }
        }

        public SkillSpecialization GetSpecialization(string strSpecialization)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (IsExoticSkill && ((ExoticSkill)this).Specific == strSpecialization)
                {
                    return Specializations[0];
                }

                return HasSpecialization(strSpecialization)
                    ? Specializations.FirstOrDefault(x =>
                        x.Name == strSpecialization || x.CurrentDisplayName == strSpecialization)
                    : null;
            }
        }

        public async ValueTask<SkillSpecialization> GetSpecializationAsync(string strSpecialization, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (IsExoticSkill && ((ExoticSkill)this).Specific == strSpecialization)
                {
                    return await Specializations.GetValueAtAsync(0, token).ConfigureAwait(false);
                }

                return await HasSpecializationAsync(strSpecialization, token).ConfigureAwait(false)
                    ? await Specializations
                        .FirstOrDefaultAsync(
                            async x => x.Name == strSpecialization || await x.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) == strSpecialization, token: token)
                        .ConfigureAwait(false)
                    : null;
            }
        }

        public string PoolToolTip => CompileDicepoolTooltip();

        public ValueTask<string> GetPoolToolTipAsync(CancellationToken token = default) =>
            CompileDicepoolTooltipAsync(token: token);

        public string CompileDicepoolTooltip(string abbrev = "", string strExtraStart = "", string strExtra = "", bool blnListAllLimbs = true, Cyberware objShowOnlyCyberware = null)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                bool blnIsNativeLanguage = IsNativeLanguage;
                if (!Default && !Leveled && !blnIsNativeLanguage)
                {
                    return strExtraStart + LanguageManager.GetString("Tip_Skill_Cannot_Default");
                }

                bool blnShowSwapSkillAttribute = false;
                if (string.IsNullOrEmpty(abbrev))
                {
                    abbrev = Attribute;
                    blnShowSwapSkillAttribute = Attribute == DefaultAttribute;
                }

                CharacterAttrib att = CharacterObject.AttributeSection.GetAttributeByName(abbrev);

                if (att.TotalValue <= 0)
                {
                    return strExtraStart + string.Format(GlobalSettings.CultureInfo,
                        LanguageManager.GetString("Tip_Skill_Zero_Attribute"),
                        att.DisplayNameShort(GlobalSettings.Language));
                }

                if (blnIsNativeLanguage)
                {
                    return strExtraStart + LanguageManager.GetString("Tip_Skill_NativeLanguage");
                }

                string strSpace = LanguageManager.GetString("String_Space");
                List<Improvement> lstRelevantImprovements = RelevantImprovements(null, abbrev, true).ToList();
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                           out StringBuilder sbdReturn))
                {
                    if (CyberwareRating > TotalBaseRating)
                    {
                        sbdReturn.Append(strExtraStart).Append(LanguageManager.GetString("Tip_Skill_SkillsoftRating"))
                            .Append(strSpace).Append('(').Append(CyberwareRating.ToString(GlobalSettings.CultureInfo))
                            .Append(')');
                    }
                    else
                    {
                        sbdReturn.Append(strExtraStart).Append(LanguageManager.GetString("Tip_Skill_SkillRating"))
                            .Append(strSpace).Append('(').Append(Rating.ToString(GlobalSettings.CultureInfo));
                        bool first = true;
                        foreach (Improvement objImprovement in lstRelevantImprovements)
                        {
                            if (!objImprovement.AddToRating)
                                continue;
                            if (first)
                            {
                                first = false;
                                sbdReturn.Append(strSpace).Append("(Base").Append(strSpace).Append('(')
                                    .Append(LearnedRating.ToString(GlobalSettings.CultureInfo)).Append(')');
                            }

                            sbdReturn.Append(strSpace).Append('+').Append(strSpace)
                                .Append(CharacterObject.GetObjectName(objImprovement)).Append(strSpace).Append('(')
                                .Append(objImprovement.Value.ToString(GlobalSettings.CultureInfo)).Append(')');
                        }

                        if (first)
                            sbdReturn.Append(')');
                        else
                            sbdReturn.Append("))");
                    }

                    if (blnListAllLimbs || !Cyberware.CyberlimbAttributeAbbrevs.Contains(att.Abbrev) ||
                        objShowOnlyCyberware == null)
                        sbdReturn.Append(strSpace).Append('+').Append(strSpace).Append(att.DisplayAbbrev)
                            .Append(strSpace)
                            .Append('(')
                            .Append(att.TotalValue.ToString(GlobalSettings.CultureInfo)).Append(')');
                    else
                    {
                        sbdReturn.Append(strSpace).Append('+').Append(strSpace)
                            .Append(objShowOnlyCyberware.CurrentDisplayName)
                            .Append(strSpace).Append(att.DisplayAbbrev).Append(strSpace).Append('(')
                            .Append(objShowOnlyCyberware.GetAttributeTotalValue(att.Abbrev)
                                .ToString(GlobalSettings.CultureInfo)).Append(')');
                        if ((objShowOnlyCyberware.LimbSlot == "arm"
                             || objShowOnlyCyberware.Name.Contains(" Arm")
                             || objShowOnlyCyberware.Name.Contains(" Hand"))
                            && objShowOnlyCyberware.Location != CharacterObject.PrimaryArm
                            && !CharacterObject.Ambidextrous
                            && objShowOnlyCyberware.LimbSlotCount <= 1)
                        {
                            sbdReturn.Append(strSpace).Append('-').Append(strSpace)
                                .Append(2.ToString(GlobalSettings.CultureInfo))
                                .Append(strSpace).Append('(').Append(LanguageManager.GetString("Tip_Skill_OffHand"))
                                .Append(')');
                        }
                    }

                    if (blnShowSwapSkillAttribute)
                    {
                        Improvement objAttributeSwapImprovement =
                            lstRelevantImprovements.Find(
                                x => x.ImproveType == Improvement.ImprovementType.SwapSkillAttribute);
                        if (objAttributeSwapImprovement != null)
                            sbdReturn.Append(strSpace)
                                .Append(CharacterObject.GetObjectName(objAttributeSwapImprovement));
                    }

                    if (Default && !Leveled)
                    {
                        int intDefaultModifier = DefaultModifier;
                        if (intDefaultModifier == 0)
                        {
                            Improvement objReflexRecorder
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                        CharacterObject, Improvement.ImprovementType.ReflexRecorderOptimization)
                                    .FirstOrDefault();
                            sbdReturn.Append(strSpace).Append(CharacterObject.GetObjectName(objReflexRecorder));
                        }
                        else
                            sbdReturn.Append(strSpace).Append(intDefaultModifier > 0 ? '+' : '-').Append(strSpace)
                                .Append(LanguageManager.GetString("Tip_Skill_Defaulting")).Append(strSpace).Append('(')
                                .Append(Math.Abs(intDefaultModifier).ToString(GlobalSettings.CultureInfo)).Append(')');
                    }

                    foreach (Improvement source in lstRelevantImprovements)
                    {
                        if (source.AddToRating
                            || source.ImproveType == Improvement.ImprovementType.SwapSkillAttribute
                            || source.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute)
                            continue;
                        sbdReturn.Append(strSpace).Append('+').Append(strSpace)
                            .Append(CharacterObject.GetObjectName(source));
                        if (!string.IsNullOrEmpty(source.Condition))
                        {
                            sbdReturn.Append(strSpace).Append('(').Append(source.Condition).Append(')');
                        }

                        sbdReturn.Append(strSpace).Append('(').Append(source.Value.ToString(GlobalSettings.CultureInfo))
                            .Append(')');
                    }

                    int wound = CharacterObject.WoundModifier;
                    if (wound != 0)
                    {
                        sbdReturn.Append(strSpace).Append('-').Append(strSpace)
                            .Append(LanguageManager.GetString("Tip_Skill_Wounds"))
                            .Append(strSpace).Append('(').Append(wound.ToString(GlobalSettings.CultureInfo))
                            .Append(')');
                    }

                    int sustains = CharacterObject.SustainingPenalty;
                    if (sustains != 0)
                    {
                        sbdReturn.Append(strSpace).Append('-').Append(strSpace)
                            .Append(LanguageManager.GetString("Tip_Skill_Sustain"))
                            .Append(strSpace).Append('(').Append(sustains.ToString(GlobalSettings.CultureInfo))
                            .Append(')');
                    }

                    if (!string.IsNullOrEmpty(strExtra))
                        sbdReturn.Append(strExtra);

                    if (blnListAllLimbs && Cyberware.CyberlimbAttributeAbbrevs.Contains(att.Abbrev))
                    {
                        foreach (Cyberware cyberware in CharacterObject.Cyberware)
                        {
                            if (cyberware.Category != "Cyberlimb" || !cyberware.IsModularCurrentlyEquipped)
                                continue;
                            sbdReturn.AppendLine().AppendLine().Append(strExtraStart)
                                .Append(cyberware.CurrentDisplayName);
                            if (cyberware.Grade.Name != "Standard" && cyberware.Grade.Name != "None")
                            {
                                sbdReturn.Append(strSpace).Append('(').Append(cyberware.Grade.CurrentDisplayName)
                                    .Append(')');
                            }

                            int pool = PoolOtherAttribute(att.Abbrev, false,
                                cyberware.GetAttributeTotalValue(att.Abbrev));
                            if ((cyberware.LimbSlot != "arm"
                                 && !cyberware.Name.Contains(" Arm")
                                 && !cyberware.Name.Contains(" Hand"))
                                || cyberware.Location == CharacterObject.PrimaryArm
                                || CharacterObject.Ambidextrous
                                || cyberware.LimbSlotCount > 1)
                            {
                                sbdReturn.Append(strSpace).Append(pool.ToString(GlobalSettings.CultureInfo));
                            }
                            else
                            {
                                sbdReturn.AppendFormat(GlobalSettings.CultureInfo, "{1}{0}{1}({2}{1}{3})", pool - 2,
                                    strSpace, -2,
                                    LanguageManager.GetString("Tip_Skill_OffHand"));
                            }

                            if (!string.IsNullOrEmpty(strExtra))
                                sbdReturn.Append(strExtra);
                        }
                    }

                    if (att.Abbrev != Attribute)
                        return sbdReturn.ToString();

                    foreach (Improvement objSwapSkillAttribute in lstRelevantImprovements)
                    {
                        if (objSwapSkillAttribute.ImproveType != Improvement.ImprovementType.SwapSkillSpecAttribute)
                            continue;
                        string strExclude = objSwapSkillAttribute.Exclude;
                        sbdReturn.AppendLine().AppendLine().Append(strExtraStart).Append(strExclude).Append(
                            LanguageManager.GetString("String_Colon")).Append(strSpace).Append(
                            CharacterObject.GetObjectName(objSwapSkillAttribute)).Append(strSpace);
                        int intBasePool = PoolOtherAttribute(objSwapSkillAttribute.ImprovedName, false,
                            CharacterObject
                                .GetAttribute(objSwapSkillAttribute.ImprovedName).Value);
                        SkillSpecialization objSpecialization = null;
                        if (Specializations.Count > 0 && ImprovementManager
                                .GetCachedImprovementListForValueOf(
                                    CharacterObject,
                                    Improvement.ImprovementType.DisableSpecializationEffects,
                                    DictionaryKey).Count == 0)
                        {
                            int intMaxBonus = 0;
                            foreach (SkillSpecialization objLoopSpecialization in Specializations)
                            {
                                if (objLoopSpecialization.Name == strExclude)
                                {
                                    int intLoopBonus = objLoopSpecialization.SpecializationBonus;
                                    if (intLoopBonus > intMaxBonus)
                                    {
                                        objSpecialization = objLoopSpecialization;
                                        intMaxBonus = intLoopBonus;
                                    }
                                }
                            }
                        }

                        if (objSpecialization != null)
                        {
                            intBasePool += objSpecialization.SpecializationBonus;
                        }

                        sbdReturn.Append(intBasePool.ToString(GlobalSettings.CultureInfo));
                        if (!string.IsNullOrEmpty(strExtra))
                            sbdReturn.Append(strExtra);
                        if (!blnListAllLimbs ||
                            !Cyberware.CyberlimbAttributeAbbrevs.Contains(objSwapSkillAttribute.ImprovedName))
                            continue;
                        foreach (Cyberware cyberware in CharacterObject.Cyberware)
                        {
                            if (cyberware.Category != "Cyberlimb" || !cyberware.IsModularCurrentlyEquipped)
                                continue;
                            sbdReturn.AppendLine().AppendLine().Append(strExtraStart).Append(strExclude)
                                .Append(LanguageManager.GetString("String_Colon")).Append(strSpace)
                                .Append(CharacterObject.GetObjectName(objSwapSkillAttribute)).Append(strSpace)
                                .Append(cyberware.CurrentDisplayName);
                            if (cyberware.Grade.Name != "Standard" && cyberware.Grade.Name != "None")
                            {
                                sbdReturn.Append(strSpace).Append('(').Append(cyberware.Grade.CurrentDisplayName)
                                    .Append(')');
                            }

                            int intLoopPool =
                                PoolOtherAttribute(objSwapSkillAttribute.ImprovedName, false,
                                    cyberware.GetAttributeTotalValue(objSwapSkillAttribute.ImprovedName));
                            if (objSpecialization != null)
                            {
                                intLoopPool += objSpecialization.SpecializationBonus;
                            }

                            if ((cyberware.LimbSlot != "arm"
                                 && !cyberware.Name.Contains(" Arm")
                                 && !cyberware.Name.Contains(" Hand"))
                                || cyberware.Location == CharacterObject.PrimaryArm
                                || CharacterObject.Ambidextrous
                                || cyberware.LimbSlotCount > 1)
                            {
                                sbdReturn.Append(strSpace).Append(intLoopPool.ToString(GlobalSettings.CultureInfo));
                            }
                            else
                            {
                                sbdReturn.AppendFormat(GlobalSettings.CultureInfo, "{1}{0}{1}({2}{1}{3})",
                                    intLoopPool - 2, strSpace, -2,
                                    LanguageManager.GetString("Tip_Skill_OffHand"));
                            }

                            if (!string.IsNullOrEmpty(strExtra))
                                sbdReturn.Append(strExtra);
                        }
                    }

                    return sbdReturn.ToString();
                }
            }
        }

        public async ValueTask<string> CompileDicepoolTooltipAsync(string abbrev = "", string strExtraStart = "", string strExtra = "", bool blnListAllLimbs = true, Cyberware objShowOnlyCyberware = null, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                bool blnIsNativeLanguage = await GetIsNativeLanguageAsync(token).ConfigureAwait(false);
                if (!await GetDefaultAsync(token).ConfigureAwait(false)
                    && !await GetLeveledAsync(token).ConfigureAwait(false)
                    && !blnIsNativeLanguage)
                {
                    return strExtraStart + await LanguageManager
                        .GetStringAsync("Tip_Skill_Cannot_Default", token: token).ConfigureAwait(false);
                }

                bool blnShowSwapSkillAttribute = false;
                if (string.IsNullOrEmpty(abbrev))
                {
                    abbrev = Attribute;
                    blnShowSwapSkillAttribute = Attribute == DefaultAttribute;
                }

                CharacterAttrib att = await CharacterObject.AttributeSection.GetAttributeByNameAsync(abbrev, token)
                    .ConfigureAwait(false);
                int intAttTotalValue = await att.GetTotalValueAsync(token).ConfigureAwait(false);
                if (intAttTotalValue <= 0)
                {
                    return strExtraStart + string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("Tip_Skill_Zero_Attribute", token: token)
                            .ConfigureAwait(false),
                        await att.DisplayNameShortAsync(GlobalSettings.Language, token).ConfigureAwait(false));
                }

                if (blnIsNativeLanguage)
                {
                    return strExtraStart + await LanguageManager
                        .GetStringAsync("Tip_Skill_NativeLanguage", token: token).ConfigureAwait(false);
                }

                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                    .ConfigureAwait(false);
                List<Improvement> lstRelevantImprovements =
                    await RelevantImprovementsAsync(null, abbrev, true, token: token).ConfigureAwait(false);
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                           out StringBuilder sbdReturn))
                {
                    int intCyberwareRating = await GetCyberwareRatingAsync(token).ConfigureAwait(false);
                    if (intCyberwareRating > await GetTotalBaseRatingAsync(token).ConfigureAwait(false))
                    {
                        sbdReturn.Append(strExtraStart).Append(await LanguageManager
                                .GetStringAsync("Tip_Skill_SkillsoftRating", token: token).ConfigureAwait(false))
                            .Append(strSpace).Append('(')
                            .Append(intCyberwareRating.ToString(GlobalSettings.CultureInfo))
                            .Append(')');
                    }
                    else
                    {
                        sbdReturn.Append(strExtraStart).Append(await LanguageManager
                                .GetStringAsync("Tip_Skill_SkillRating", token: token).ConfigureAwait(false))
                            .Append(strSpace).Append('(')
                            .Append((await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings
                                .CultureInfo));
                        bool first = true;
                        foreach (Improvement objImprovement in lstRelevantImprovements)
                        {
                            if (!objImprovement.AddToRating)
                                continue;
                            if (first)
                            {
                                first = false;
                                sbdReturn.Append(strSpace).Append("(Base").Append(strSpace).Append('(')
                                    .Append((await GetLearnedRatingAsync(token).ConfigureAwait(false)).ToString(
                                        GlobalSettings.CultureInfo)).Append(')');
                            }

                            sbdReturn.Append(strSpace).Append('+').Append(strSpace)
                                .Append(await CharacterObject.GetObjectNameAsync(objImprovement, token: token)
                                    .ConfigureAwait(false)).Append(strSpace).Append('(')
                                .Append(objImprovement.Value.ToString(GlobalSettings.CultureInfo)).Append(')');
                        }

                        if (first)
                            sbdReturn.Append(')');
                        else
                            sbdReturn.Append("))");
                    }

                    if (blnListAllLimbs || !Cyberware.CyberlimbAttributeAbbrevs.Contains(att.Abbrev) ||
                        objShowOnlyCyberware == null)
                        sbdReturn.Append(strSpace).Append('+').Append(strSpace)
                            .Append(await att.GetDisplayAbbrevAsync(GlobalSettings.Language, token)
                                .ConfigureAwait(false)).Append(strSpace)
                            .Append('(')
                            .Append(intAttTotalValue.ToString(GlobalSettings.CultureInfo)).Append(')');
                    else
                    {
                        sbdReturn.Append(strSpace).Append('+').Append(strSpace)
                            .Append(await objShowOnlyCyberware.GetCurrentDisplayNameAsync(token).ConfigureAwait(false))
                            .Append(strSpace)
                            .Append(await att.GetDisplayAbbrevAsync(GlobalSettings.Language, token)
                                .ConfigureAwait(false)).Append(strSpace).Append('(')
                            .Append((await objShowOnlyCyberware.GetAttributeTotalValueAsync(att.Abbrev, token)
                                    .ConfigureAwait(false))
                                .ToString(GlobalSettings.CultureInfo)).Append(')');
                        if ((objShowOnlyCyberware.LimbSlot == "arm"
                             || objShowOnlyCyberware.Name.Contains(" Arm")
                             || objShowOnlyCyberware.Name.Contains(" Hand"))
                            && objShowOnlyCyberware.Location != CharacterObject.PrimaryArm
                            && !CharacterObject.Ambidextrous
                            && objShowOnlyCyberware.LimbSlotCount <= 1)
                        {
                            sbdReturn.Append(strSpace).Append('-').Append(strSpace)
                                .Append(2.ToString(GlobalSettings.CultureInfo))
                                .Append(strSpace).Append('(').Append(await LanguageManager
                                    .GetStringAsync("Tip_Skill_OffHand", token: token).ConfigureAwait(false))
                                .Append(')');
                        }
                    }

                    if (blnShowSwapSkillAttribute)
                    {
                        Improvement objAttributeSwapImprovement =
                            lstRelevantImprovements.Find(
                                x => x.ImproveType == Improvement.ImprovementType.SwapSkillAttribute);
                        if (objAttributeSwapImprovement != null)
                            sbdReturn.Append(strSpace)
                                .Append(await CharacterObject.GetObjectNameAsync(
                                    objAttributeSwapImprovement, token: token).ConfigureAwait(false));
                    }

                    if (await GetDefaultAsync(token).ConfigureAwait(false) &&
                        !await GetLeveledAsync(token).ConfigureAwait(false))
                    {
                        int intDefaultModifier = await GetDefaultModifierAsync(token).ConfigureAwait(false);
                        if (intDefaultModifier == 0)
                        {
                            Improvement objReflexRecorder
                                = (await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                    CharacterObject, Improvement.ImprovementType.ReflexRecorderOptimization,
                                    token: token).ConfigureAwait(false)).FirstOrDefault();
                            sbdReturn.Append(strSpace).Append(await CharacterObject
                                .GetObjectNameAsync(objReflexRecorder, token: token).ConfigureAwait(false));
                        }
                        else
                            sbdReturn.Append(strSpace).Append(intDefaultModifier > 0 ? '+' : '-').Append(strSpace)
                                .Append(await LanguageManager.GetStringAsync("Tip_Skill_Defaulting", token: token)
                                    .ConfigureAwait(false)).Append(strSpace).Append('(')
                                .Append(Math.Abs(intDefaultModifier).ToString(GlobalSettings.CultureInfo)).Append(')');
                    }

                    foreach (Improvement source in lstRelevantImprovements)
                    {
                        if (source.AddToRating
                            || source.ImproveType == Improvement.ImprovementType.SwapSkillAttribute
                            || source.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute)
                            continue;
                        sbdReturn.Append(strSpace).Append('+').Append(strSpace).Append(
                            await CharacterObject.GetObjectNameAsync(source, token: token).ConfigureAwait(false));
                        if (!string.IsNullOrEmpty(source.Condition))
                        {
                            sbdReturn.Append(strSpace).Append('(').Append(source.Condition).Append(')');
                        }

                        sbdReturn.Append(strSpace).Append('(').Append(source.Value.ToString(GlobalSettings.CultureInfo))
                            .Append(')');
                    }

                    int wound = CharacterObject.WoundModifier;
                    if (wound != 0)
                    {
                        sbdReturn.Append(strSpace).Append('-').Append(strSpace)
                            .Append(await LanguageManager.GetStringAsync("Tip_Skill_Wounds", token: token)
                                .ConfigureAwait(false))
                            .Append(strSpace).Append('(').Append(wound.ToString(GlobalSettings.CultureInfo))
                            .Append(')');
                    }

                    int sustains = CharacterObject.SustainingPenalty;
                    if (sustains != 0)
                    {
                        sbdReturn.Append(strSpace).Append('-').Append(strSpace)
                            .Append(await LanguageManager.GetStringAsync("Tip_Skill_Sustain", token: token)
                                .ConfigureAwait(false))
                            .Append(strSpace).Append('(').Append(sustains.ToString(GlobalSettings.CultureInfo))
                            .Append(')');
                    }

                    if (!string.IsNullOrEmpty(strExtra))
                        sbdReturn.Append(strExtra);

                    if (blnListAllLimbs && Cyberware.CyberlimbAttributeAbbrevs.Contains(att.Abbrev))
                    {
                        bool blnAmbi = await CharacterObject.GetAmbidextrousAsync(token).ConfigureAwait(false);
                        foreach (Cyberware cyberware in CharacterObject.Cyberware)
                        {
                            if (cyberware.Category != "Cyberlimb" || !cyberware.IsModularCurrentlyEquipped)
                                continue;
                            sbdReturn.AppendLine().AppendLine().Append(strExtraStart)
                                .Append(await cyberware.GetCurrentDisplayNameAsync(token).ConfigureAwait(false));
                            if (cyberware.Grade.Name != "Standard" && cyberware.Grade.Name != "None")
                            {
                                sbdReturn.Append(strSpace).Append('(').Append(await cyberware.Grade.GetCurrentDisplayNameAsync(token).ConfigureAwait(false))
                                    .Append(')');
                            }

                            int pool = await PoolOtherAttributeAsync(att.Abbrev, false,
                                await cyberware.GetAttributeTotalValueAsync(att.Abbrev, token).ConfigureAwait(false),
                                token).ConfigureAwait(false);
                            if ((cyberware.LimbSlot != "arm"
                                 && !cyberware.Name.Contains(" Arm")
                                 && !cyberware.Name.Contains(" Hand"))
                                || cyberware.Location == CharacterObject.PrimaryArm
                                || blnAmbi
                                || cyberware.LimbSlotCount > 1)
                            {
                                sbdReturn.Append(strSpace).Append(pool.ToString(GlobalSettings.CultureInfo));
                            }
                            else
                            {
                                sbdReturn.AppendFormat(GlobalSettings.CultureInfo, "{1}{0}{1}({2}{1}{3})", pool - 2,
                                    strSpace, -2,
                                    await LanguageManager.GetStringAsync("Tip_Skill_OffHand", token: token)
                                        .ConfigureAwait(false));
                            }

                            if (!string.IsNullOrEmpty(strExtra))
                                sbdReturn.Append(strExtra);
                        }
                    }

                    if (att.Abbrev != Attribute)
                        return sbdReturn.ToString();

                    foreach (Improvement objSwapSkillAttribute in lstRelevantImprovements)
                    {
                        if (objSwapSkillAttribute.ImproveType != Improvement.ImprovementType.SwapSkillSpecAttribute)
                            continue;
                        string strExclude = objSwapSkillAttribute.Exclude;
                        sbdReturn.AppendLine().AppendLine().Append(strExtraStart).Append(strExclude).Append(
                                await LanguageManager.GetStringAsync("String_Colon", token: token)
                                    .ConfigureAwait(false))
                            .Append(strSpace).Append(
                                await CharacterObject.GetObjectNameAsync(objSwapSkillAttribute, token: token)
                                    .ConfigureAwait(false)).Append(strSpace);
                        int intBasePool = await PoolOtherAttributeAsync(objSwapSkillAttribute.ImprovedName, false,
                            (await CharacterObject
                                .GetAttributeAsync(objSwapSkillAttribute.ImprovedName, token: token)
                                .ConfigureAwait(false)).Value, token).ConfigureAwait(false);
                        SkillSpecialization objSpecialization = null;
                        if (await Specializations.GetCountAsync(token).ConfigureAwait(false) > 0 &&
                            (await ImprovementManager
                                .GetCachedImprovementListForValueOfAsync(
                                    CharacterObject,
                                    Improvement.ImprovementType.DisableSpecializationEffects,
                                    await GetDictionaryKeyAsync(token).ConfigureAwait(false), token: token)
                                .ConfigureAwait(false)).Count == 0)
                        {
                            int intMaxBonus = 0;
                            foreach (SkillSpecialization objLoopSpecialization in Specializations)
                            {
                                if (objLoopSpecialization.Name == strExclude)
                                {
                                    int intLoopBonus = await objLoopSpecialization.GetSpecializationBonusAsync(token)
                                        .ConfigureAwait(false);
                                    if (intLoopBonus > intMaxBonus)
                                    {
                                        objSpecialization = objLoopSpecialization;
                                        intMaxBonus = intLoopBonus;
                                    }
                                }
                            }
                        }

                        if (objSpecialization != null)
                        {
                            intBasePool += await objSpecialization.GetSpecializationBonusAsync(token)
                                .ConfigureAwait(false);
                        }

                        sbdReturn.Append(intBasePool.ToString(GlobalSettings.CultureInfo));
                        if (!string.IsNullOrEmpty(strExtra))
                            sbdReturn.Append(strExtra);
                        if (!blnListAllLimbs ||
                            !Cyberware.CyberlimbAttributeAbbrevs.Contains(objSwapSkillAttribute.ImprovedName))
                            continue;
                        bool blnAmbi = await CharacterObject.GetAmbidextrousAsync(token).ConfigureAwait(false);
                        foreach (Cyberware cyberware in CharacterObject.Cyberware)
                        {
                            if (cyberware.Category != "Cyberlimb" || !cyberware.IsModularCurrentlyEquipped)
                                continue;
                            sbdReturn.AppendLine().AppendLine().Append(strExtraStart).Append(strExclude)
                                .Append(await LanguageManager.GetStringAsync("String_Colon", token: token)
                                    .ConfigureAwait(false)).Append(strSpace)
                                .Append(await CharacterObject.GetObjectNameAsync(objSwapSkillAttribute, token: token)
                                    .ConfigureAwait(false)).Append(strSpace)
                                .Append(await cyberware.GetCurrentDisplayNameAsync(token).ConfigureAwait(false));
                            if (cyberware.Grade.Name != "Standard" && cyberware.Grade.Name != "None")
                            {
                                sbdReturn.Append(strSpace).Append('(').Append(await cyberware.Grade.GetCurrentDisplayNameAsync(token).ConfigureAwait(false))
                                    .Append(')');
                            }

                            int intLoopPool =
                                await PoolOtherAttributeAsync(objSwapSkillAttribute.ImprovedName, false,
                                    await cyberware
                                        .GetAttributeTotalValueAsync(objSwapSkillAttribute.ImprovedName, token)
                                        .ConfigureAwait(false), token).ConfigureAwait(false);
                            if (objSpecialization != null)
                            {
                                intLoopPool += await objSpecialization.GetSpecializationBonusAsync(token)
                                    .ConfigureAwait(false);
                            }

                            if ((cyberware.LimbSlot != "arm"
                                 && !cyberware.Name.Contains(" Arm")
                                 && !cyberware.Name.Contains(" Hand"))
                                || cyberware.Location == CharacterObject.PrimaryArm
                                || blnAmbi
                                || cyberware.LimbSlotCount > 1)
                            {
                                sbdReturn.Append(strSpace).Append(intLoopPool.ToString(GlobalSettings.CultureInfo));
                            }
                            else
                            {
                                sbdReturn.AppendFormat(GlobalSettings.CultureInfo, "{1}{0}{1}({2}{1}{3})",
                                    intLoopPool - 2, strSpace, -2,
                                    await LanguageManager.GetStringAsync("Tip_Skill_OffHand", token: token)
                                        .ConfigureAwait(false));
                            }

                            if (!string.IsNullOrEmpty(strExtra))
                                sbdReturn.Append(strExtra);
                        }
                    }

                    return sbdReturn.ToString();
                }
            }
        }

        public string UpgradeToolTip
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    int intCost = UpgradeKarmaCost;
                    return intCost < 0
                        ? LanguageManager.GetString("Tip_ImproveItemAtMaximum")
                        : string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_ImproveItem"),
                                        Rating + 1, intCost);
                }
            }
        }

        public async ValueTask<string> GetUpgradeToolTipAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                int intCost = await GetUpgradeKarmaCostAsync(token).ConfigureAwait(false);
                return intCost < 0
                    ? await LanguageManager.GetStringAsync("Tip_ImproveItemAtMaximum", token: token)
                                           .ConfigureAwait(false)
                    : string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Tip_ImproveItem", token: token)
                                                         .ConfigureAwait(false),
                                    await GetRatingAsync(token).ConfigureAwait(false) + 1, intCost);
            }
        }

        public string AddSpecToolTip
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    int intPrice = IsKnowledgeSkill
                        ? CharacterObject.Settings.KarmaKnowledgeSpecialization
                        : CharacterObject.Settings.KarmaSpecialization;

                    decimal decExtraSpecCost = 0;
                    int intTotalBaseRating = TotalBaseRating;
                    decimal decSpecCostMultiplier = 1.0m;
                    foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                    {
                        if (objLoopImprovement.Minimum > intTotalBaseRating
                            || (!string.IsNullOrEmpty(objLoopImprovement.Condition)
                                && (objLoopImprovement.Condition == "career") != CharacterObject.Created
                                && (objLoopImprovement.Condition == "create") == CharacterObject.Created)
                            || !objLoopImprovement.Enabled)
                            continue;
                        if (objLoopImprovement.ImprovedName != SkillCategory)
                            continue;
                        switch (objLoopImprovement.ImproveType)
                        {
                            case Improvement.ImprovementType.SkillCategorySpecializationKarmaCost:
                                decExtraSpecCost += objLoopImprovement.Value;
                                break;

                            case Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier:
                                decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                                break;
                        }
                    }

                    if (decSpecCostMultiplier != 1.0m)
                        intPrice = (intPrice * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
                    else
                        intPrice += decExtraSpecCost.StandardRound(); //Spec
                    return string.Format(GlobalSettings.CultureInfo,
                        LanguageManager.GetString("Tip_Skill_AddSpecialization"), intPrice);
                }
            }
        }

        public async ValueTask<string> GetAddSpecToolTipAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                int intPrice = IsKnowledgeSkill
                    ? CharacterObject.Settings.KarmaKnowledgeSpecialization
                    : CharacterObject.Settings.KarmaSpecialization;

                int intTotalBaseRating = await GetTotalBaseRatingAsync(token).ConfigureAwait(false);
                decimal decSpecCostMultiplier = 1.0m;
                decimal decExtraSpecCost = await CharacterObject.Improvements.SumAsync(objLoopImprovement =>
                {
                    if (objLoopImprovement.Minimum > intTotalBaseRating
                        || (!string.IsNullOrEmpty(objLoopImprovement.Condition)
                            && (objLoopImprovement.Condition == "career") != CharacterObject.Created
                            && (objLoopImprovement.Condition == "create") == CharacterObject.Created)
                        || !objLoopImprovement.Enabled)
                        return 0;
                    if (objLoopImprovement.ImprovedName != SkillCategory)
                        return 0;
                    switch (objLoopImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.SkillCategorySpecializationKarmaCost:
                            return objLoopImprovement.Value;

                        case Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier:
                            decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                            break;
                    }

                    return 0;
                }, token: token).ConfigureAwait(false);

                if (decSpecCostMultiplier != 1.0m)
                    intPrice = (intPrice * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
                else
                    intPrice += decExtraSpecCost.StandardRound(); //Spec
                return string.Format(GlobalSettings.CultureInfo,
                                     await LanguageManager.GetStringAsync("Tip_Skill_AddSpecialization", token: token)
                                                          .ConfigureAwait(false), intPrice);
            }
        }

        public string SkillToolTip
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    string strSpace = LanguageManager.GetString("String_Space");
                    string strReturn = !string.IsNullOrEmpty(Notes)
                        ? LanguageManager.GetString("Label_Notes") + strSpace + Notes + Environment.NewLine +
                          Environment.NewLine
                        : string.Empty;
                    string strMiddle = !string.IsNullOrWhiteSpace(SkillGroup)
                        ? SkillGroupObject.CurrentDisplayName + strSpace +
                          LanguageManager.GetString("String_ExpenseSkillGroup") + Environment.NewLine
                        : string.Empty;
                    strReturn += CurrentDisplayCategory + Environment.NewLine + strMiddle +
                                 SourceString.GetSourceString(Source, Page, GlobalSettings.Language,
                                     GlobalSettings.CultureInfo,
                                     CharacterObject).LanguageBookTooltip;
                    return strReturn;
                }
            }
        }

        public async ValueTask<string> GetSkillToolTipAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                string strNotes = await GetNotesAsync(token).ConfigureAwait(false);
                string strReturn = !string.IsNullOrEmpty(strNotes)
                    ? await LanguageManager.GetStringAsync("Label_Notes", token: token).ConfigureAwait(false) + strSpace + strNotes + Environment.NewLine +
                      Environment.NewLine
                    : string.Empty;
                string strMiddle = !string.IsNullOrWhiteSpace(SkillGroup)
                    ? await SkillGroupObject.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) + strSpace +
                      await LanguageManager.GetStringAsync("String_ExpenseSkillGroup", token: token).ConfigureAwait(false) + Environment.NewLine
                    : string.Empty;
                strReturn += await GetCurrentDisplayCategoryAsync(token).ConfigureAwait(false) + Environment.NewLine + strMiddle +
                             (await SourceString.GetSourceStringAsync(Source, Page, GlobalSettings.Language,
                                                                      GlobalSettings.CultureInfo,
                                                                      CharacterObject, token).ConfigureAwait(false)).LanguageBookTooltip;
                return strReturn;
            }
        }

        public string Notes
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _strNotes;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_strNotes == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        if (Interlocked.Exchange(ref _strNotes, value) != value)
                            OnPropertyChanged();
                    }
                }
            }
        }

        public async ValueTask<string> GetNotesAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _strNotes;
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _colNotes;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
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

        public Color PreferredColor
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return !string.IsNullOrEmpty(Notes)
                        ? ColorManager.GenerateCurrentModeColor(NotesColor)
                        : ColorManager.ControlText;
            }
        }

        public async ValueTask<Color> GetPreferredColorAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return !string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false))
                    ? await ColorManager.GenerateCurrentModeColorAsync(NotesColor, token).ConfigureAwait(false)
                    : await ColorManager.GetControlTextAsync(token).ConfigureAwait(false);
        }

        public SkillGroup SkillGroupObject { get; }

        public string Page { get; }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <returns></returns>
        public string DisplayPage(string strLanguage)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return Page;
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
        public async ValueTask<string> DisplayPageAsync(string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return Page;
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                string s = objNode != null
                    ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("altpage", token).ConfigureAwait(false))
                    ?.Value ?? Page
                    : Page;
                return !string.IsNullOrWhiteSpace(s) ? s : Page;
            }
        }

        public string Source { get; }

        //Stuff that is RO, that is simply calculated from other things
        //Move to extension method?

        #region Calculations

        public int AttributeModifiers
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return AttributeObject.TotalValue;
            }
        }

        public string DisplayName(string strLanguage)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return Name;

                return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
            }
        }

        public async ValueTask<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return Name;

                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                return objNode != null
                    ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token)
                        .ConfigureAwait(false))?.Value ?? Name
                    : Name;
            }
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public ValueTask<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
            DisplayNameAsync(GlobalSettings.Language, token);

        public string CurrentDisplayCategory => DisplayCategory(GlobalSettings.Language);

        public ValueTask<string> GetCurrentDisplayCategoryAsync(CancellationToken token = default) =>
            DisplayCategoryAsync(GlobalSettings.Language, token);

        public string DisplayCategory(string strLanguage)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return SkillCategory;

                string strReturn = CharacterObject.LoadDataXPath("skills.xml", strLanguage)
                    .SelectSingleNode(
                        "/chummer/categories/category[. = " + SkillCategory.CleanXPath()
                                                            + "]/@translate")?.Value;

                return strReturn ?? SkillCategory;
            }
        }

        public async ValueTask<string> DisplayCategoryAsync(string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return SkillCategory;

                string strReturn = (await CharacterObject.LoadDataXPathAsync("skills.xml", strLanguage, token: token)
                        .ConfigureAwait(false))
                    .SelectSingleNode(
                        "/chummer/categories/category[. = " + SkillCategory.CleanXPath()
                                                            + "]/@translate")?.Value;

                return strReturn ?? SkillCategory;
            }
        }

        public string DisplayPool
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return IsNativeLanguage ? LanguageManager.GetString("Skill_NativeLanguageShort") : DisplayOtherAttribute(Attribute);
            }
        }

        public async Task<string> GetDisplayPoolAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return await GetIsNativeLanguageAsync(token).ConfigureAwait(false)
                    ? await LanguageManager.GetStringAsync("Skill_NativeLanguageShort", token: token).ConfigureAwait(false)
                    : await DisplayOtherAttributeAsync(Attribute, token).ConfigureAwait(false);
        }

        public string DisplayOtherAttribute(string strAttribute)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                int intPool = PoolOtherAttribute(strAttribute);
                if ((IsExoticSkill || Specializations.Count == 0 || ImprovementManager
                        .GetCachedImprovementListForValueOf(
                            CharacterObject,
                            Improvement.ImprovementType
                                .DisableSpecializationEffects,
                            DictionaryKey).Count > 0)
                    && !CharacterObject.Improvements.Any(i => i.ImproveType == Improvement.ImprovementType.Skill
                                                              && !string.IsNullOrEmpty(i.Condition)))
                {
                    return intPool.ToString(GlobalSettings.CultureInfo);
                }

                int intConditionalBonus = PoolOtherAttribute(strAttribute, true);
                int intSpecBonus = GetSpecializationBonus();
                if (intSpecBonus == 0 && intPool == intConditionalBonus)
                    return intPool.ToString(GlobalSettings.CultureInfo);
                return string.Format(GlobalSettings.CultureInfo, "{0}{1}({2})",
                    intPool, LanguageManager.GetString("String_Space"),
                    Math.Max(intPool + intSpecBonus,
                        intConditionalBonus)); // Have to do it this way because some conditional bonuses apply specifically to specializations
            }
        }

        public async ValueTask<string> DisplayOtherAttributeAsync(string strAttribute, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                int intPool = await PoolOtherAttributeAsync(strAttribute, token: token).ConfigureAwait(false);
                if ((IsExoticSkill || await Specializations.GetCountAsync(token).ConfigureAwait(false) == 0 ||
                     (await ImprovementManager
                         .GetCachedImprovementListForValueOfAsync(
                             CharacterObject,
                             Improvement.ImprovementType
                                 .DisableSpecializationEffects,
                             await GetDictionaryKeyAsync(token).ConfigureAwait(false), token: token)
                         .ConfigureAwait(false)).Count > 0)
                    && !await CharacterObject.Improvements.AnyAsync(i =>
                        i.ImproveType == Improvement.ImprovementType.Skill
                        && !string.IsNullOrEmpty(i.Condition), token: token).ConfigureAwait(false))
                {
                    return intPool.ToString(GlobalSettings.CultureInfo);
                }

                int intConditionalBonus =
                    await PoolOtherAttributeAsync(strAttribute, true, token: token).ConfigureAwait(false);
                int intSpecBonus = await GetSpecializationBonusAsync(token: token).ConfigureAwait(false);
                if (intSpecBonus == 0 && intPool == intConditionalBonus)
                    return intPool.ToString(GlobalSettings.CultureInfo);
                return string.Format(GlobalSettings.CultureInfo, "{0}{1}({2})",
                    intPool, await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false),
                    Math.Max(intPool + intSpecBonus,
                        intConditionalBonus)); // Have to do it this way because some conditional bonuses apply specifically to specializations
            }
        }

        public int GetSpecializationBonus(string strSpecialization = "")
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (IsExoticSkill || TotalBaseRating == 0 || Specializations.Count == 0)
                    return 0;
                SkillSpecialization objTargetSpecialization = default;
                if (string.IsNullOrEmpty(strSpecialization))
                {
                    if (ImprovementManager
                            .GetCachedImprovementListForValueOf(CharacterObject,
                                Improvement.ImprovementType.DisableSpecializationEffects,
                                DictionaryKey).Count == 0)
                    {
                        int intHighestSpecBonus = 0;
                        foreach (SkillSpecialization objSpec in Specializations)
                        {
                            int intLoopSpecBonus = objSpec.SpecializationBonus;
                            if (intHighestSpecBonus < intLoopSpecBonus)
                            {
                                intHighestSpecBonus = intLoopSpecBonus;
                                objTargetSpecialization = objSpec;
                            }
                        }
                    }
                }
                else
                    objTargetSpecialization = GetSpecialization(strSpecialization);

                return objTargetSpecialization?.SpecializationBonus ?? 0;
            }
        }

        public async ValueTask<int> GetSpecializationBonusAsync(string strSpecialization = "", CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (IsExoticSkill || await GetTotalBaseRatingAsync(token).ConfigureAwait(false) == 0 ||
                    await Specializations.GetCountAsync(token).ConfigureAwait(false) == 0)
                    return 0;
                SkillSpecialization objTargetSpecialization = default;
                if (string.IsNullOrEmpty(strSpecialization))
                {
                    if ((await ImprovementManager
                            .GetCachedImprovementListForValueOfAsync(CharacterObject,
                                Improvement.ImprovementType.DisableSpecializationEffects,
                                await GetDictionaryKeyAsync(token).ConfigureAwait(false), token: token)
                            .ConfigureAwait(false)).Count == 0)
                    {
                        int intHighestSpecBonus = 0;
                        foreach (SkillSpecialization objSpec in Specializations)
                        {
                            int intLoopSpecBonus =
                                await objSpec.GetSpecializationBonusAsync(token).ConfigureAwait(false);
                            if (intHighestSpecBonus < intLoopSpecBonus)
                            {
                                intHighestSpecBonus = intLoopSpecBonus;
                                objTargetSpecialization = objSpec;
                            }
                        }
                    }
                }
                else
                    objTargetSpecialization =
                        await GetSpecializationAsync(strSpecialization, token).ConfigureAwait(false);

                return objTargetSpecialization != null
                    ? await objTargetSpecialization.GetSpecializationBonusAsync(token).ConfigureAwait(false)
                    : 0;
            }
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            // ReSharper disable once MethodHasAsyncOverload
            using (blnSync ? EnterReadLock.Enter(LockObject, token) : await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage
                                                && !GlobalSettings.LiveCustomData)
                    return _objCachedMyXmlNode;
                IDisposable objLockerSync = blnSync ? LockObject.EnterWriteLock(token) : null;
                IAsyncDisposable objLocker = blnSync ? null : await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage
                                                    && !GlobalSettings.LiveCustomData)
                        return _objCachedMyXmlNode;
                    _objCachedMyXmlNode = (blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? CharacterObject.LoadData("skills.xml", strLanguage, token: token)
                            : await CharacterObject.LoadDataAsync("skills.xml", strLanguage, token: token)
                                .ConfigureAwait(false))
                        .SelectSingleNode(string.Format(GlobalSettings.InvariantCultureInfo,
                            IsKnowledgeSkill
                                ? "/chummer/knowledgeskills/skill[id = {0} or id = {1}]"
                                : "/chummer/skills/skill[id = {0} or id = {1}]",
                            SkillId.ToString("D", GlobalSettings.InvariantCultureInfo).CleanXPath(),
                            SkillId.ToString("D", GlobalSettings.InvariantCultureInfo)
                                .ToUpperInvariant().CleanXPath()));
                    _strCachedXmlNodeLanguage = strLanguage;
                    return _objCachedMyXmlNode;
                }
                finally
                {
                    if (objLocker != null)
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    objLockerSync?.Dispose();
                }
            }
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            // ReSharper disable once MethodHasAsyncOverload
            using (blnSync ? EnterReadLock.Enter(LockObject, token) : await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_objCachedMyXPathNode != null && strLanguage == _strCachedXPathNodeLanguage
                                                  && !GlobalSettings.LiveCustomData)
                    return _objCachedMyXPathNode;
                IDisposable objLocker = null;
                IAsyncDisposable objLockerAsync = null;
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    objLocker = LockObject.EnterWriteLock(token);
                else
                    objLockerAsync = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    if (_objCachedMyXPathNode != null && strLanguage == _strCachedXPathNodeLanguage
                                                      && !GlobalSettings.LiveCustomData)
                        return _objCachedMyXPathNode;
                    _objCachedMyXPathNode = (blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? CharacterObject.LoadDataXPath("skills.xml", strLanguage, token: token)
                            : await CharacterObject.LoadDataXPathAsync("skills.xml", strLanguage, token: token)
                                .ConfigureAwait(false))
                        .SelectSingleNode(string.Format(GlobalSettings.InvariantCultureInfo,
                            IsKnowledgeSkill
                                ? "/chummer/knowledgeskills/skill[id = {0} or id = {1}]"
                                : "/chummer/skills/skill[id = {0} or id = {1}]",
                            SkillId.ToString(
                                    "D",
                                    GlobalSettings.InvariantCultureInfo)
                                .CleanXPath(),
                            SkillId.ToString(
                                    "D",
                                    GlobalSettings.InvariantCultureInfo)
                                .ToUpperInvariant().CleanXPath()));
                    _strCachedXPathNodeLanguage = strLanguage;
                    return _objCachedMyXPathNode;
                }
                finally
                {
                    if (blnSync)
                        // ReSharper disable once MethodHasAsyncOverload
                        objLocker.Dispose();
                    else
                        await objLockerAsync.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        private int _intCachedCyberwareRating = int.MinValue;

        protected virtual void ResetCachedCyberwareRating()
        {
            using (LockObject.EnterWriteLock())
                _intCachedCyberwareRating = int.MinValue;
        }

        /// <summary>
        /// The attributeValue this skill have from Skillwires + Skilljack or Active Hardwires
        /// </summary>
        /// <returns>Artificial skill attributeValue</returns>
        public virtual int CyberwareRating
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intCachedCyberwareRating != int.MinValue)
                        return _intCachedCyberwareRating;

                    using (LockObject.EnterWriteLock())
                    {
                        if (_intCachedCyberwareRating != int.MinValue)
                            return _intCachedCyberwareRating;
                        //TODO: method is here, but not used in any form, needs testing (worried about child items...)
                        //this might do hardwires if i understand how they works correctly
                        int intMaxHardwire = -1;
                        foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                                     CharacterObject, Improvement.ImprovementType.Hardwire, DictionaryKey))
                        {
                            intMaxHardwire = Math.Max(intMaxHardwire, objImprovement.Value.StandardRound());
                        }

                        if (intMaxHardwire >= 0)
                        {
                            return _intCachedCyberwareRating = intMaxHardwire;
                        }

                        int intMaxActivesoftRating =
                            Math.Min(ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.Skillwire),
                                    ImprovementManager.ValueOf(CharacterObject,
                                        Improvement.ImprovementType.SkillsoftAccess))
                                .StandardRound();
                        if (intMaxActivesoftRating > 0)
                        {
                            int intMax = 0;
                            //TODO this works with translate?
                            foreach (Improvement objSkillsoftImprovement in ImprovementManager
                                         .GetCachedImprovementListForValueOf(CharacterObject,
                                             Improvement.ImprovementType.Activesoft,
                                             DictionaryKey))
                            {
                                intMax = Math.Max(intMax, objSkillsoftImprovement.Value.StandardRound());
                            }

                            return _intCachedCyberwareRating = Math.Min(intMax, intMaxActivesoftRating);
                        }

                        return _intCachedCyberwareRating = 0;
                    }
                }
            }
        }

        /// <summary>
        /// The attributeValue this skill have from Skillwires + Skilljack or Active Hardwires
        /// </summary>
        /// <returns>Artificial skill attributeValue</returns>
        public virtual async ValueTask<int> GetCyberwareRatingAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_intCachedCyberwareRating != int.MinValue)
                    return _intCachedCyberwareRating;

                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    if (_intCachedCyberwareRating != int.MinValue)
                        return _intCachedCyberwareRating;
                    //TODO: method is here, but not used in any form, needs testing (worried about child items...)
                    //this might do hardwires if i understand how they works correctly
                    int intMaxHardwire = -1;
                    foreach (Improvement objImprovement in await ImprovementManager
                                 .GetCachedImprovementListForValueOfAsync(
                                     CharacterObject, Improvement.ImprovementType.Hardwire,
                                     await GetDictionaryKeyAsync(token).ConfigureAwait(false), token: token)
                                 .ConfigureAwait(false))
                    {
                        intMaxHardwire = Math.Max(intMaxHardwire, objImprovement.Value.StandardRound());
                    }

                    if (intMaxHardwire >= 0)
                    {
                        return _intCachedCyberwareRating = intMaxHardwire;
                    }

                    int intMaxActivesoftRating =
                        Math.Min(
                                await ImprovementManager
                                    .ValueOfAsync(CharacterObject, Improvement.ImprovementType.Skillwire, token: token)
                                    .ConfigureAwait(false),
                                await ImprovementManager.ValueOfAsync(CharacterObject,
                                    Improvement.ImprovementType.SkillsoftAccess, token: token).ConfigureAwait(false))
                            .StandardRound();
                    if (intMaxActivesoftRating > 0)
                    {
                        int intMax = 0;
                        //TODO this works with translate?
                        foreach (Improvement objSkillsoftImprovement in await ImprovementManager
                                     .GetCachedImprovementListForValueOfAsync(CharacterObject,
                                         Improvement.ImprovementType.Activesoft,
                                         await GetDictionaryKeyAsync(token).ConfigureAwait(false), token: token)
                                     .ConfigureAwait(false))
                        {
                            intMax = Math.Max(intMax, objSkillsoftImprovement.Value.StandardRound());
                        }

                        return _intCachedCyberwareRating = Math.Min(intMax, intMaxActivesoftRating);
                    }

                    return _intCachedCyberwareRating = 0;
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion Calculations

        #region Static

        //A tree of dependencies. Once some of the properties are changed,
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly PropertyDependencyGraph<Skill> s_SkillDependencyGraph =
            new PropertyDependencyGraph<Skill>(
                new DependencyGraphNode<string, Skill>(nameof(PoolToolTip),
                    new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage)),
                    new DependencyGraphNode<string, Skill>(nameof(AttributeModifiers),
                        new DependencyGraphNode<string, Skill>(nameof(AttributeObject),
                            new DependencyGraphNode<string, Skill>(nameof(RelevantImprovements)))),
                    new DependencyGraphNode<string, Skill>(nameof(DisplayPool),
                        new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage)),
                        new DependencyGraphNode<string, Skill>(nameof(Attribute), x => !x.IsNativeLanguage),
                        new DependencyGraphNode<string, Skill>(nameof(DisplayOtherAttribute), x => !x.IsNativeLanguage,
                            new DependencyGraphNode<string, Skill>(nameof(PoolOtherAttribute),
                                new DependencyGraphNode<string, Skill>(nameof(Enabled)),
                                new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage)),
                                new DependencyGraphNode<string, Skill>(nameof(Rating)),
                                new DependencyGraphNode<string, Skill>(nameof(GetSpecializationBonus),
                                    new DependencyGraphNode<string, Skill>(nameof(IsExoticSkill)),
                                    new DependencyGraphNode<string, Skill>(nameof(Specializations)),
                                    new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating)),
                                    new DependencyGraphNode<string, Skill>(nameof(GetSpecialization),
                                        new DependencyGraphNode<string, Skill>(nameof(IsExoticSkill)),
                                        new DependencyGraphNode<string, Skill>(nameof(Specializations)),
                                        new DependencyGraphNode<string, Skill>(nameof(HasSpecialization),
                                            new DependencyGraphNode<string, Skill>(nameof(IsExoticSkill)),
                                            new DependencyGraphNode<string, Skill>(nameof(Specializations))
                                        )
                                    )
                                ),
                                new DependencyGraphNode<string, Skill>(nameof(PoolModifiers),
                                    new DependencyGraphNode<string, Skill>(nameof(Bonus),
                                        new DependencyGraphNode<string, Skill>(nameof(RelevantImprovements))
                                    )
                                ),
                                new DependencyGraphNode<string, Skill>(nameof(Default),
                                    new DependencyGraphNode<string, Skill>(nameof(RelevantImprovements))
                                ),
                                new DependencyGraphNode<string, Skill>(nameof(DefaultModifier),
                                    new DependencyGraphNode<string, Skill>(nameof(DictionaryKey))
                                )
                            ),
                            new DependencyGraphNode<string, Skill>(nameof(DictionaryKey)),
                            new DependencyGraphNode<string, Skill>(nameof(IsExoticSkill)),
                            new DependencyGraphNode<string, Skill>(nameof(Specializations)),
                            new DependencyGraphNode<string, Skill>(nameof(GetSpecializationBonus))
                        ),
                        new DependencyGraphNode<string, Skill>(nameof(Pool),
                            new DependencyGraphNode<string, Skill>(nameof(AttributeModifiers),
                                new DependencyGraphNode<string, Skill>(nameof(AttributeObject))
                            ),
                            new DependencyGraphNode<string, Skill>(nameof(PoolOtherAttribute)),
                            new DependencyGraphNode<string, Skill>(nameof(Attribute))
                        )
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(CanHaveSpecs),
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.AllowUpgrade), x => x.IsKnowledgeSkill,
                        new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage)),
                        new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill))
                    ),
                    new DependencyGraphNode<string, Skill>(nameof(Enabled)),
                    new DependencyGraphNode<string, Skill>(nameof(IsExoticSkill)),
                    new DependencyGraphNode<string, Skill>(nameof(KarmaUnlocked)),
                    new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating),
                        new DependencyGraphNode<string, Skill>(nameof(RatingModifiers),
                            new DependencyGraphNode<string, Skill>(nameof(Bonus))
                        ),
                        new DependencyGraphNode<string, Skill>(nameof(LearnedRating),
                            new DependencyGraphNode<string, Skill>(nameof(Karma),
                                new DependencyGraphNode<string, Skill>(nameof(FreeKarma),
                                    new DependencyGraphNode<string, Skill>(nameof(DictionaryKey))
                                ),
                                new DependencyGraphNode<string, Skill>(nameof(RatingMaximum),
                                    new DependencyGraphNode<string, Skill>(nameof(RelevantImprovements))
                                ),
                                new DependencyGraphNode<string, Skill>(nameof(KarmaPoints))
                            ),
                            new DependencyGraphNode<string, Skill>(nameof(Base),
                                new DependencyGraphNode<string, Skill>(nameof(FreeBase),
                                    new DependencyGraphNode<string, Skill>(nameof(DictionaryKey))
                                ),
                                new DependencyGraphNode<string, Skill>(nameof(RatingMaximum)),
                                new DependencyGraphNode<string, Skill>(nameof(BasePoints))
                            )
                        )
                    ),
                    new DependencyGraphNode<string, Skill>(nameof(Leveled),
                        new DependencyGraphNode<string, Skill>(nameof(Rating),
                            new DependencyGraphNode<string, Skill>(nameof(CyberwareRating),
                                new DependencyGraphNode<string, Skill>(nameof(DictionaryKey))
                            ),
                            new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating))
                        )
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(UpgradeToolTip),
                    new DependencyGraphNode<string, Skill>(nameof(Rating)),
                    new DependencyGraphNode<string, Skill>(nameof(UpgradeKarmaCost),
                        new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating)),
                        new DependencyGraphNode<string, Skill>(nameof(RatingMaximum))
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(BuyWithKarma),
                    new DependencyGraphNode<string, Skill>(nameof(ForcedBuyWithKarma),
                        new DependencyGraphNode<string, Skill>(nameof(Specializations)),
                        new DependencyGraphNode<string, Skill>(nameof(KarmaPoints)),
                        new DependencyGraphNode<string, Skill>(nameof(BasePoints)),
                        new DependencyGraphNode<string, Skill>(nameof(FreeBase))
                    ),
                    new DependencyGraphNode<string, Skill>(nameof(ForcedNotBuyWithKarma),
                        new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating))
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(RelevantImprovements),
                    new DependencyGraphNode<string, Skill>(nameof(DictionaryKey))
                ),
                new DependencyGraphNode<string, Skill>(nameof(DisplayAttribute),
                    new DependencyGraphNode<string, Skill>(nameof(Attribute),
                        new DependencyGraphNode<string, Skill>(nameof(AttributeObject))
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(CurrentDisplayName),
                    new DependencyGraphNode<string, Skill>(nameof(DisplayName),
                        new DependencyGraphNode<string, Skill>(nameof(Name))
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(SkillToolTip),
                    new DependencyGraphNode<string, Skill>(nameof(Notes)),
                    new DependencyGraphNode<string, Skill>(nameof(CurrentDisplayCategory),
                        new DependencyGraphNode<string, Skill>(nameof(DisplayCategory),
                            new DependencyGraphNode<string, Skill>(nameof(SkillCategory),
                                new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.Type), x => x.IsKnowledgeSkill,
                                    new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill))
                                )
                            )
                        )
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(PreferredControlColor),
                    new DependencyGraphNode<string, Skill>(nameof(Leveled)),
                    new DependencyGraphNode<string, Skill>(nameof(Enabled))
                ),
                new DependencyGraphNode<string, Skill>(nameof(PreferredColor),
                    new DependencyGraphNode<string, Skill>(nameof(Notes))
                ),
                new DependencyGraphNode<string, Skill>(nameof(CGLSpecializations),
                    new DependencyGraphNode<string, Skill>(nameof(SkillId)),
                    new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill))
                ),
                new DependencyGraphNode<string, Skill>(nameof(CurrentSpCost),
                    new DependencyGraphNode<string, Skill>(nameof(BasePoints)),
                    new DependencyGraphNode<string, Skill>(nameof(Specializations)),
                    new DependencyGraphNode<string, Skill>(nameof(BuyWithKarma))
                ),
                new DependencyGraphNode<string, Skill>(nameof(CurrentKarmaCost),
                    new DependencyGraphNode<string, Skill>(nameof(RangeCost),
                        new DependencyGraphNode<string, Skill>(nameof(SkillGroupObject), x => x.CharacterObject.Settings.CompensateSkillGroupKarmaDifference && x.Enabled),
                        new DependencyGraphNode<string, Skill>(nameof(Enabled), x => x.CharacterObject.Settings.CompensateSkillGroupKarmaDifference && x.SkillGroupObject != null)
                    ),
                    new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating)),
                    new DependencyGraphNode<string, Skill>(nameof(Base)),
                    new DependencyGraphNode<string, Skill>(nameof(FreeKarma)),
                    new DependencyGraphNode<string, Skill>(nameof(Specializations))
                ),
                new DependencyGraphNode<string, Skill>(nameof(CanUpgradeCareer),
                    new DependencyGraphNode<string, Skill>(nameof(UpgradeKarmaCost),
                        new DependencyGraphNode<string, Skill>(nameof(SkillGroupObject), x => x.CharacterObject.Settings.CompensateSkillGroupKarmaDifference && x.Enabled),
                        new DependencyGraphNode<string, Skill>(nameof(Enabled), x => x.CharacterObject.Settings.CompensateSkillGroupKarmaDifference && x.SkillGroupObject != null)
                    ),
                    new DependencyGraphNode<string, Skill>(nameof(RatingMaximum)),
                    new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating))
                ),
                new DependencyGraphNode<string, Skill>(nameof(Enabled),
                    new DependencyGraphNode<string, Skill>(nameof(ForceDisabled)),
                    new DependencyGraphNode<string, Skill>(nameof(Attribute)),
                    new DependencyGraphNode<string, Skill>(nameof(DictionaryKey)),
                    new DependencyGraphNode<string, Skill>(nameof(RequiresGroundMovement)),
                    new DependencyGraphNode<string, Skill>(nameof(RequiresSwimMovement)),
                    new DependencyGraphNode<string, Skill>(nameof(RequiresFlyMovement))
                ),
                new DependencyGraphNode<string, Skill>(nameof(CurrentDisplaySpecialization),
                    new DependencyGraphNode<string, Skill>(nameof(DisplaySpecialization),
                        new DependencyGraphNode<string, Skill>(nameof(TopMostDisplaySpecialization),
                            new DependencyGraphNode<string, Skill>(nameof(Specializations))
                        ),
                        new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage), x => x.IsKnowledgeSkill,
                            new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill))
                        )
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(CanAffordSpecialization),
                    new DependencyGraphNode<string, Skill>(nameof(CanHaveSpecs)),
                    new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating))
                ),
                new DependencyGraphNode<string, Skill>(nameof(AllowDelete), x => x.IsKnowledgeSkill || x.IsExoticSkill,
                    new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill)),
                    new DependencyGraphNode<string, Skill>(nameof(IsExoticSkill)),
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.ForcedName), x => x.IsKnowledgeSkill,
                        new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill))
                    ),
                    new DependencyGraphNode<string, Skill>(nameof(FreeBase)),
                    new DependencyGraphNode<string, Skill>(nameof(FreeKarma)),
                    new DependencyGraphNode<string, Skill>(nameof(RatingModifiers)),
                    new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage), x => x.IsKnowledgeSkill,
                        new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill))
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(DictionaryKey),
                    new DependencyGraphNode<string, Skill>(nameof(Name)),
                    new DependencyGraphNode<string, Skill>(nameof(IsExoticSkill)),
                    new DependencyGraphNode<string, Skill>(nameof(DisplaySpecialization), x => x.IsExoticSkill,
                        new DependencyGraphNode<string, Skill>(nameof(IsExoticSkill))
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(IsLanguage), x => x.IsKnowledgeSkill,
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.Type)),
                    new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill))
                ),
                new DependencyGraphNode<string, Skill>(nameof(AllowNameChange), x => x.IsKnowledgeSkill,
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.ForcedName)),
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.AllowUpgrade)),
                    new DependencyGraphNode<string, Skill>(nameof(Karma)),
                    new DependencyGraphNode<string, Skill>(nameof(Base)),
                    new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage)),
                    new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill))
                ),
                new DependencyGraphNode<string, Skill>(nameof(AllowTypeChange), x => x.IsKnowledgeSkill,
                    new DependencyGraphNode<string, Skill>(nameof(AllowNameChange)),
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.Type)),
                    new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage)),
                    new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill))
                )
            );

        #endregion Static

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(IReadOnlyCollection<string> lstPropertyNames)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                HashSet<string> setNamesOfChangedProperties = null;
                try
                {
                    foreach (string strPropertyName in lstPropertyNames)
                    {
                        if (setNamesOfChangedProperties == null)
                            setNamesOfChangedProperties
                                = s_SkillDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                        else
                        {
                            foreach (string strLoopChangedProperty in s_SkillDependencyGraph
                                         .GetWithAllDependentsEnumerable(
                                             this, strPropertyName))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    using (LockObject.EnterWriteLock())
                    {
                        if (setNamesOfChangedProperties.Contains(nameof(FreeBase)))
                            _intCachedFreeBase = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(FreeKarma)))
                            _intCachedFreeKarma = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(CanUpgradeCareer)))
                            _intCachedCanUpgradeCareer = -1;
                        if (setNamesOfChangedProperties.Contains(nameof(CanAffordSpecialization)))
                            _intCachedCanAffordSpecialization = -1;
                        if (setNamesOfChangedProperties.Contains(nameof(Enabled)))
                            _intCachedEnabled = -1;
                        if (setNamesOfChangedProperties.Contains(nameof(CanHaveSpecs)))
                            _intCachedCanHaveSpecs = -1;
                        if (setNamesOfChangedProperties.Contains(nameof(ForcedBuyWithKarma)))
                            _intCachedForcedBuyWithKarma = -1;
                        if (setNamesOfChangedProperties.Contains(nameof(ForcedNotBuyWithKarma)))
                            _intCachedForcedNotBuyWithKarma = -1;
                        if (setNamesOfChangedProperties.Contains(nameof(CyberwareRating)))
                            ResetCachedCyberwareRating();
                        if (setNamesOfChangedProperties.Contains(nameof(CGLSpecializations)))
                            _blnRecalculateCachedSuggestedSpecializations = true;
                    }

                    Utils.RunOnMainThread(() =>
                    {
                        if (PropertyChanged != null)
                        {
                            foreach (string strPropertyToChange in setNamesOfChangedProperties)
                            {
                                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                            }
                        }
                    });

                    using (LockObject.EnterWriteLock())
                    {
                        // Do this after firing all property changers. Not part of the dependency graph because dependency is very complicated
                        if (setNamesOfChangedProperties.Contains(nameof(DefaultAttribute)))
                            RecacheAttribute();
                        if (setNamesOfChangedProperties.Contains(nameof(Enabled))
                            && CharacterObject.Settings.CompensateSkillGroupKarmaDifference && SkillGroupObject != null)
                        {
                            foreach (Skill objSkill in SkillGroupObject.SkillList)
                            {
                                if (objSkill == this)
                                    continue;
                                objSkill.OnMultiplePropertyChanged(nameof(UpgradeKarmaCost), nameof(RangeCost));
                            }
                        }
                    }
                }
                finally
                {
                    if (setNamesOfChangedProperties != null)
                        Utils.StringHashSetPool.Return(setNamesOfChangedProperties);
                }
            }
        }

        private void OnSkillGroupChanged(object sender, PropertyChangedEventArgs e)
        {
            if (CharacterObject?.IsLoading != false)
                return;
            switch (e.PropertyName)
            {
                case nameof(Skills.SkillGroup.Base) when CharacterObject.EffectiveBuildMethodUsesPriorityTables:
                    this.OnMultiplePropertyChanged(nameof(Base),
                                                   nameof(BaseUnlocked),
                                                   nameof(ForcedBuyWithKarma));
                    break;

                case nameof(Skills.SkillGroup.Base):
                    this.OnMultiplePropertyChanged(nameof(Base),
                                                   nameof(ForcedBuyWithKarma));
                    break;

                case nameof(Skills.SkillGroup.Karma):
                    this.OnMultiplePropertyChanged(nameof(Karma),
                                                   nameof(CurrentKarmaCost),
                                                   nameof(ForcedBuyWithKarma),
                                                   nameof(ForcedNotBuyWithKarma));
                    break;

                case nameof(Skills.SkillGroup.Rating):
                    {
                        if (CharacterObject.Settings.StrictSkillGroupsInCreateMode && !CharacterObject.Created && !CharacterObject.IgnoreRules)
                        {
                            OnPropertyChanged(nameof(KarmaUnlocked));
                        }

                        break;
                    }
                case nameof(Skills.SkillGroup.SkillList) when CharacterObject.Settings.CompensateSkillGroupKarmaDifference:
                    {
                        if (Enabled)
                        {
                            this.OnMultiplePropertyChanged(nameof(RangeCost), nameof(UpgradeKarmaCost));
                        }

                        break;
                    }
            }
        }

        private void OnCharacterChanged(object sender, PropertyChangedEventArgs e)
        {
            if (CharacterObject?.IsLoading != false)
                return;
            switch (e.PropertyName)
            {
                case nameof(Character.Karma):
                    this.OnMultiplePropertyChanged(nameof(CanUpgradeCareer), nameof(CanAffordSpecialization));
                    break;

                case nameof(Character.WoundModifier):
                    OnPropertyChanged(nameof(PoolOtherAttribute));
                    break;

                case nameof(Character.SustainingPenalty):
                    OnPropertyChanged(nameof(PoolOtherAttribute));
                    break;

                case nameof(Character.PrimaryArm):
                    OnPropertyChanged(nameof(PoolToolTip));
                    break;

                case nameof(Character.GetMovement):
                    if (RequiresGroundMovement)
                        OnPropertyChanged(nameof(Enabled));
                    break;

                case nameof(Character.GetSwim):
                    if (RequiresSwimMovement)
                        OnPropertyChanged(nameof(Enabled));
                    break;

                case nameof(Character.GetFly):
                    if (RequiresFlyMovement)
                        OnPropertyChanged(nameof(Enabled));
                    break;

                case nameof(Character.MAGEnabled):
                    if (Attribute == "MAG" || Attribute == "MAGAdept")
                        OnPropertyChanged(nameof(Enabled));
                    break;

                case nameof(Character.RESEnabled):
                    if (Attribute == "RES")
                        OnPropertyChanged(nameof(Enabled));
                    break;

                case nameof(Character.DEPEnabled):
                    if (Attribute == "DEP")
                        OnPropertyChanged(nameof(Enabled));
                    break;

                case nameof(Character.EffectiveBuildMethodUsesPriorityTables):
                    this.OnMultiplePropertyChanged(nameof(Base),
                                                   nameof(BaseUnlocked),
                                                   nameof(ForcedBuyWithKarma));
                    break;

                case nameof(Character.IsCritter):
                    OnPropertyChanged(nameof(Default));
                    break;
            }
        }

        private void OnCharacterSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (CharacterObject?.IsLoading != false)
                return;
            switch (e.PropertyName)
            {
                case nameof(CharacterSettings.StrictSkillGroupsInCreateMode):
                    {
                        if (SkillGroupObject != null)
                        {
                            if (!CharacterObject.Created)
                            {
                                OnPropertyChanged(nameof(KarmaUnlocked));
                            }

                            this.OnMultiplePropertyChanged(nameof(BaseUnlocked), nameof(ForcedNotBuyWithKarma));
                        }
                        break;
                    }
                case nameof(CharacterSettings.UsePointsOnBrokenGroups):
                    {
                        if (SkillGroupObject != null)
                        {
                            OnPropertyChanged(nameof(BaseUnlocked));
                        }
                        break;
                    }
                case nameof(CharacterSettings.KarmaNewKnowledgeSkill):
                case nameof(CharacterSettings.KarmaImproveKnowledgeSkill):
                    {
                        if (IsKnowledgeSkill)
                        {
                            OnPropertyChanged(nameof(CurrentKarmaCost));
                        }
                        break;
                    }
                case nameof(CharacterSettings.KarmaKnowledgeSpecialization):
                    {
                        if (IsKnowledgeSkill)
                        {
                            this.OnMultiplePropertyChanged(nameof(CurrentKarmaCost), nameof(CanAffordSpecialization), nameof(AddSpecToolTip));
                        }
                        break;
                    }
                case nameof(CharacterSettings.KarmaNewActiveSkill):
                case nameof(CharacterSettings.KarmaImproveActiveSkill):
                    {
                        if (!IsKnowledgeSkill)
                        {
                            OnPropertyChanged(nameof(CurrentKarmaCost));
                        }
                        break;
                    }
                case nameof(CharacterSettings.KarmaSpecialization):
                    {
                        if (!IsKnowledgeSkill)
                        {
                            this.OnMultiplePropertyChanged(nameof(CurrentKarmaCost), nameof(CanAffordSpecialization), nameof(AddSpecToolTip));
                        }
                        break;
                    }
                case nameof(CharacterSettings.CompensateSkillGroupKarmaDifference):
                    {
                        if (SkillGroupObject != null && Enabled)
                        {
                            this.OnMultiplePropertyChanged(nameof(RangeCost), nameof(UpgradeKarmaCost));
                        }
                        break;
                    }
                case nameof(CharacterSettings.KarmaNewSkillGroup):
                case nameof(CharacterSettings.KarmaImproveSkillGroup):
                    {
                        if (SkillGroupObject != null && CharacterObject.Settings.CompensateSkillGroupKarmaDifference && Enabled)
                        {
                            this.OnMultiplePropertyChanged(nameof(RangeCost), nameof(UpgradeKarmaCost));
                        }
                        break;
                    }
                case nameof(CharacterSettings.SpecializationBonus):
                    {
                        if (Specializations.Count > 0)
                        {
                            OnPropertyChanged(nameof(PoolOtherAttribute));
                        }
                        break;
                    }
                case nameof(CharacterSettings.ExpertiseBonus):
                    {
                        if (Specializations.Any(x => x.Expertise))
                        {
                            OnPropertyChanged(nameof(PoolOtherAttribute));
                        }
                        break;
                    }
            }
        }

        protected void OnLinkedAttributeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (CharacterObject?.IsLoading != false)
                return;
            switch (e?.PropertyName)
            {
                case nameof(CharacterAttrib.TotalValue):
                    OnPropertyChanged(nameof(AttributeModifiers));
                    break;

                case nameof(CharacterAttrib.Abbrev):
                    OnPropertyChanged(nameof(Enabled));
                    break;
            }
        }

        private int _intSkipSpecializationRefresh;

        private void SpecializationsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_intSkipSpecializationRefresh > 0)
                return;
            using (LockObject.EnterWriteLock())
            {
                // Needed to make sure we don't call this method another time when we set the specialization's Parent
                if (Interlocked.Increment(ref _intSkipSpecializationRefresh) != 1)
                {
                    Interlocked.Decrement(ref _intSkipSpecializationRefresh);
                    return;
                }
                try
                {
                    _dicCachedStringSpec.Clear();
                    if (IsExoticSkill)
                        _strDictionaryKey = null;
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            foreach (SkillSpecialization objSkillSpecialization in e.NewItems)
                                objSkillSpecialization.Parent = this;
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            foreach (SkillSpecialization objSkillSpecialization in e.OldItems)
                            {
                                if (objSkillSpecialization.Parent == this)
                                    objSkillSpecialization.Parent = null;
                                objSkillSpecialization.Dispose();
                            }

                            break;

                        case NotifyCollectionChangedAction.Replace:
                            foreach (SkillSpecialization objSkillSpecialization in e.OldItems)
                            {
                                if (objSkillSpecialization.Parent == this)
                                    objSkillSpecialization.Parent = null;
                                objSkillSpecialization.Dispose();
                            }

                            foreach (SkillSpecialization objSkillSpecialization in e.NewItems)
                                objSkillSpecialization.Parent = this;
                            break;

                        case NotifyCollectionChangedAction.Reset:
                            foreach (SkillSpecialization objSkillSpecialization in Specializations)
                                objSkillSpecialization.Parent = this;
                            break;
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intSkipSpecializationRefresh);
                }

                if (CharacterObject?.IsLoading == false)
                    OnPropertyChanged(nameof(Specializations));
            }
        }

        private void SpecializationsOnBeforeClearCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_intSkipSpecializationRefresh > 0)
                return;
            using (LockObject.EnterWriteLock())
            {
                // Needed to make sure we don't call this method another time when we set the specialization's Parent
                if (Interlocked.Increment(ref _intSkipSpecializationRefresh) != 1)
                {
                    Interlocked.Decrement(ref _intSkipSpecializationRefresh);
                    return;
                }
                try
                {
                    foreach (SkillSpecialization objSkillSpecialization in e.OldItems)
                    {
                        if (objSkillSpecialization.Parent == this)
                            objSkillSpecialization.Parent = null;
                        objSkillSpecialization.Dispose();
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intSkipSpecializationRefresh);
                }
            }
        }

        /// <summary>
        /// Calculate the karma cost of increasing a skill from lower to upper
        /// </summary>
        /// <param name="lower">Staring rating of skill</param>
        /// <param name="upper">End rating of the skill</param>
        protected int RangeCost(int lower, int upper)
        {
            if (lower >= upper)
                return 0;

            int intLevelsModded = upper * (upper + 1); //cost if nothing else was there
            intLevelsModded -= lower * (lower + 1); //remove "karma" costs from base + free

            intLevelsModded /= 2; //we get square, we need triangle

            int cost;
            if (lower == 0)
                cost = (intLevelsModded - 1) * CharacterObject.Settings.KarmaImproveActiveSkill + CharacterObject.Settings.KarmaNewActiveSkill;
            else
                cost = intLevelsModded * CharacterObject.Settings.KarmaImproveActiveSkill;

            using (EnterReadLock.Enter(LockObject))
            {
                int intSkillGroupCostAdjustment = 0;
                if (CharacterObject.Settings.CompensateSkillGroupKarmaDifference && SkillGroupObject != null)
                {
                    int intSkillGroupUpper = int.MaxValue;
                    foreach (Skill objSkillGroupMember in SkillGroupObject.SkillList)
                    {
                        if (objSkillGroupMember != this && objSkillGroupMember.Enabled)
                        {
                            int intLoopTotalBaseRating = objSkillGroupMember.TotalBaseRating;
                            if (intLoopTotalBaseRating < intSkillGroupUpper)
                                intSkillGroupUpper = intLoopTotalBaseRating;
                        }
                    }

                    if (intSkillGroupUpper != int.MaxValue && intSkillGroupUpper > lower)
                    {
                        if (intSkillGroupUpper > upper)
                            intSkillGroupUpper = upper;
                        int intGroupCost;
                        int intNakedSkillCost = SkillGroupObject.SkillList.Count(x => x == this || x.Enabled);
                        if (lower == 0)
                        {
                            int intExtraLevels = (intSkillGroupUpper - 1) * intSkillGroupUpper;
                            intGroupCost = intExtraLevels * CharacterObject.Settings.KarmaImproveSkillGroup / 2 +
                                           CharacterObject.Settings.KarmaNewSkillGroup;
                            intNakedSkillCost *= intExtraLevels * CharacterObject.Settings.KarmaImproveActiveSkill / 2 +
                                                 CharacterObject.Settings.KarmaNewActiveSkill;
                        }
                        else
                        {
                            int intExtraLevels = intSkillGroupUpper * (intSkillGroupUpper + 1) - lower * (lower + 1);
                            intGroupCost = intExtraLevels * CharacterObject.Settings.KarmaImproveSkillGroup / 2;
                            intNakedSkillCost *= intExtraLevels * CharacterObject.Settings.KarmaImproveActiveSkill / 2;
                        }

                        intSkillGroupCostAdjustment = intGroupCost - intNakedSkillCost;
                        cost += intSkillGroupCostAdjustment;
                    }
                }

                decimal decMultiplier = 1.0m;
                decimal decExtra = 0;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if (objLoopImprovement.Minimum <= lower &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                         (objLoopImprovement.Condition == "career") == CharacterObject.Created ||
                         (objLoopImprovement.Condition == "create") != CharacterObject.Created) &&
                        objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == DictionaryKey ||
                            string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.ActiveSkillKarmaCost:
                                    decExtra += objLoopImprovement.Value *
                                                (Math.Min(upper,
                                                    objLoopImprovement.Maximum == 0
                                                        ? int.MaxValue
                                                        : objLoopImprovement.Maximum) - Math.Max(lower,
                                                    objLoopImprovement.Minimum - 1));
                                    break;

                                case Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier:
                                    decMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
                        else if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.SkillCategoryKarmaCost:
                                    decExtra += objLoopImprovement.Value *
                                                (Math.Min(upper,
                                                    objLoopImprovement.Maximum == 0
                                                        ? int.MaxValue
                                                        : objLoopImprovement.Maximum) - Math.Max(lower,
                                                    objLoopImprovement.Minimum - 1));
                                    break;

                                case Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier:
                                    decMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
                    }
                }

                if (decMultiplier != 1.0m)
                    cost = (cost * decMultiplier + decExtra).StandardRound();
                else
                    cost += decExtra.StandardRound();

                if (cost < intSkillGroupCostAdjustment)
                    cost = intSkillGroupCostAdjustment;
                return cost;
            }
        }

        /// <summary>
        /// Calculate the karma cost of increasing a skill from lower to upper
        /// </summary>
        /// <param name="lower">Staring rating of skill</param>
        /// <param name="upper">End rating of the skill</param>
        /// <param name="token">Cancellation token to listen to.</param>
        protected async ValueTask<int> RangeCostAysnc(int lower, int upper, CancellationToken token = default)
        {
            if (lower >= upper)
                return 0;

            int intLevelsModded = upper * (upper + 1); //cost if nothing else was there
            intLevelsModded -= lower * (lower + 1); //remove "karma" costs from base + free

            intLevelsModded /= 2; //we get square, we need triangle

            CharacterSettings objSettings = await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
            int cost;
            if (lower == 0)
                cost = (intLevelsModded - 1) * await objSettings.GetKarmaImproveActiveSkillAsync(token).ConfigureAwait(false) + await objSettings.GetKarmaNewActiveSkillAsync(token).ConfigureAwait(false);
            else
                cost = intLevelsModded * await objSettings.GetKarmaImproveActiveSkillAsync(token).ConfigureAwait(false);

            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                int intSkillGroupCostAdjustment = 0;
                if (await objSettings.GetCompensateSkillGroupKarmaDifferenceAsync(token).ConfigureAwait(false) &&
                    SkillGroupObject != null)
                {
                    int intSkillGroupUpper = int.MaxValue;
                    foreach (Skill objSkillGroupMember in SkillGroupObject.SkillList)
                    {
                        if (objSkillGroupMember != this &&
                            await objSkillGroupMember.GetEnabledAsync(token).ConfigureAwait(false))
                        {
                            int intLoopTotalBaseRating = await objSkillGroupMember.GetTotalBaseRatingAsync(token)
                                .ConfigureAwait(false);
                            if (intLoopTotalBaseRating < intSkillGroupUpper)
                                intSkillGroupUpper = intLoopTotalBaseRating;
                        }
                    }

                    if (intSkillGroupUpper != int.MaxValue && intSkillGroupUpper > lower)
                    {
                        if (intSkillGroupUpper > upper)
                            intSkillGroupUpper = upper;
                        int intGroupCost;
                        int intNakedSkillCost
                            = await SkillGroupObject.SkillList.CountAsync(
                                    async x => x == this || await x.GetEnabledAsync(token).ConfigureAwait(false), token)
                                .ConfigureAwait(false);
                        if (lower == 0)
                        {
                            int intExtraLevels = (intSkillGroupUpper - 1) * intSkillGroupUpper;
                            intGroupCost = intExtraLevels *
                                           await objSettings.GetKarmaImproveSkillGroupAsync(token)
                                               .ConfigureAwait(false) / 2
                                           + await objSettings.GetKarmaNewSkillGroupAsync(token).ConfigureAwait(false);
                            intNakedSkillCost
                                *= intExtraLevels * await objSettings.GetKarmaImproveActiveSkillAsync(token)
                                       .ConfigureAwait(false) / 2
                                   + await objSettings.GetKarmaNewActiveSkillAsync(token).ConfigureAwait(false);
                        }
                        else
                        {
                            int intExtraLevels = intSkillGroupUpper * (intSkillGroupUpper + 1) - lower * (lower + 1);
                            intGroupCost = intExtraLevels *
                                await objSettings.GetKarmaImproveSkillGroupAsync(token).ConfigureAwait(false) / 2;
                            intNakedSkillCost *= intExtraLevels *
                                                 await objSettings.GetKarmaImproveActiveSkillAsync(token)
                                                     .ConfigureAwait(false)
                                                 / 2;
                        }

                        intSkillGroupCostAdjustment = intGroupCost - intNakedSkillCost;
                        cost += intSkillGroupCostAdjustment;
                    }
                }

                decimal decMultiplier = 1.0m;
                decimal decExtra = 0;
                bool blnCreated = await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false);
                await (await CharacterObject.GetImprovementsAsync(token).ConfigureAwait(false)).ForEachAsync(
                    async objLoopImprovement =>
                    {
                        if (objLoopImprovement.Minimum > lower ||
                            (!string.IsNullOrEmpty(objLoopImprovement.Condition)
                             && (objLoopImprovement.Condition == "career") != blnCreated
                             && (objLoopImprovement.Condition == "create") == blnCreated)
                            || !objLoopImprovement.Enabled)
                            return;
                        if (objLoopImprovement.ImprovedName == await GetDictionaryKeyAsync(token).ConfigureAwait(false)
                            || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.ActiveSkillKarmaCost:
                                    decExtra += objLoopImprovement.Value
                                                * (Math.Min(
                                                       upper,
                                                       objLoopImprovement.Maximum == 0
                                                           ? int.MaxValue
                                                           : objLoopImprovement.Maximum)
                                                   - Math.Max(lower, objLoopImprovement.Minimum - 1));
                                    break;

                                case Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier:
                                    decMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
                        else if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.SkillCategoryKarmaCost:
                                    decExtra += objLoopImprovement.Value
                                                * (Math.Min(
                                                       upper,
                                                       objLoopImprovement.Maximum == 0
                                                           ? int.MaxValue
                                                           : objLoopImprovement.Maximum)
                                                   - Math.Max(lower, objLoopImprovement.Minimum - 1));
                                    break;

                                case Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier:
                                    decMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
                    }, token: token).ConfigureAwait(false);
                if (decMultiplier != 1.0m)
                    cost = (cost * decMultiplier + decExtra).StandardRound();
                else
                    cost += decExtra.StandardRound();

                if (cost < intSkillGroupCostAdjustment)
                    cost = intSkillGroupCostAdjustment;
                return cost;
            }
        }

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible
        /// </summary>
        /// <returns>Price in karma</returns>
        public virtual int UpgradeKarmaCost
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    int intTotalBaseRating = TotalBaseRating;
                    if (intTotalBaseRating >= RatingMaximum)
                    {
                        return -1;
                    }

                    int upgrade = 0;
                    int intOptionsCost;
                    if (intTotalBaseRating == 0)
                    {
                        intOptionsCost = CharacterObject.Settings.KarmaNewActiveSkill;
                        upgrade += intOptionsCost;
                    }
                    else
                    {
                        intOptionsCost = CharacterObject.Settings.KarmaImproveActiveSkill;
                        upgrade += (intTotalBaseRating + 1) * intOptionsCost;
                    }

                    int intSkillGroupCostAdjustment = 0;
                    if (CharacterObject.Settings.CompensateSkillGroupKarmaDifference && SkillGroupObject != null)
                    {
                        int intSkillGroupUpper = int.MaxValue;
                        foreach (Skill objSkillGroupMember in SkillGroupObject.SkillList)
                        {
                            if (objSkillGroupMember != this && objSkillGroupMember.Enabled)
                            {
                                int intLoopTotalBaseRating = objSkillGroupMember.TotalBaseRating;
                                if (intLoopTotalBaseRating < intSkillGroupUpper)
                                    intSkillGroupUpper = intLoopTotalBaseRating;
                            }
                        }

                        if (intSkillGroupUpper != int.MaxValue && intSkillGroupUpper > intTotalBaseRating)
                        {
                            int intGroupCost;
                            int intNakedSkillCost = SkillGroupObject.SkillList.Count(x => x == this || x.Enabled);
                            if (intTotalBaseRating == 0)
                            {
                                intGroupCost = CharacterObject.Settings.KarmaNewSkillGroup;
                                intNakedSkillCost *= CharacterObject.Settings.KarmaNewActiveSkill;
                            }
                            else
                            {
                                intGroupCost = (intTotalBaseRating + 1) *
                                               CharacterObject.Settings.KarmaImproveSkillGroup;
                                intNakedSkillCost *= (intTotalBaseRating + 1) *
                                                     CharacterObject.Settings.KarmaImproveActiveSkill;
                            }

                            intSkillGroupCostAdjustment = intGroupCost - intNakedSkillCost;
                            upgrade += intSkillGroupCostAdjustment;
                        }
                    }

                    decimal decMultiplier = 1.0m;
                    decimal decExtra = 0;
                    foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                    {
                        if ((objLoopImprovement.Maximum == 0 || intTotalBaseRating + 1 <= objLoopImprovement.Maximum) &&
                            objLoopImprovement.Minimum <= intTotalBaseRating + 1 &&
                            (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                             (objLoopImprovement.Condition == "career") == CharacterObject.Created ||
                             (objLoopImprovement.Condition == "create") != CharacterObject.Created) &&
                            objLoopImprovement.Enabled)
                        {
                            if (objLoopImprovement.ImprovedName == DictionaryKey ||
                                string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.ActiveSkillKarmaCost:
                                        decExtra += objLoopImprovement.Value;
                                        break;

                                    case Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                            else if (objLoopImprovement.ImprovedName == SkillCategory)
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillCategoryKarmaCost:
                                        decExtra += objLoopImprovement.Value;
                                        break;

                                    case Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                        }
                    }

                    if (decMultiplier != 1.0m)
                        upgrade = (upgrade * decMultiplier + decExtra).StandardRound();
                    else
                        upgrade += decExtra.StandardRound();

                    int intMinCost = Math.Min(1, intOptionsCost);
                    if (upgrade < intMinCost + intSkillGroupCostAdjustment)
                        upgrade = intMinCost + intSkillGroupCostAdjustment;
                    return upgrade;
                }
            }
        }

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible
        /// </summary>
        /// <returns>Price in karma</returns>
        public virtual async ValueTask<int> GetUpgradeKarmaCostAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                int intTotalBaseRating = await GetTotalBaseRatingAsync(token).ConfigureAwait(false);
                if (intTotalBaseRating >= await GetRatingMaximumAsync(token).ConfigureAwait(false))
                {
                    return -1;
                }

                int upgrade = 0;
                int intOptionsCost;
                CharacterSettings objSettings = await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                if (intTotalBaseRating == 0)
                {
                    intOptionsCost = objSettings.KarmaNewActiveSkill;
                    upgrade += intOptionsCost;
                }
                else
                {
                    intOptionsCost = objSettings.KarmaImproveActiveSkill;
                    upgrade += (intTotalBaseRating + 1) * intOptionsCost;
                }

                int intSkillGroupCostAdjustment = 0;
                if (objSettings.CompensateSkillGroupKarmaDifference && SkillGroupObject != null)
                {
                    int intSkillGroupUpper = int.MaxValue;
                    foreach (Skill objSkillGroupMember in SkillGroupObject.SkillList)
                    {
                        if (objSkillGroupMember != this && await objSkillGroupMember.GetEnabledAsync(token).ConfigureAwait(false))
                        {
                            int intLoopTotalBaseRating = await objSkillGroupMember.GetTotalBaseRatingAsync(token).ConfigureAwait(false);
                            if (intLoopTotalBaseRating < intSkillGroupUpper)
                                intSkillGroupUpper = intLoopTotalBaseRating;
                        }
                    }

                    if (intSkillGroupUpper != int.MaxValue && intSkillGroupUpper > intTotalBaseRating)
                    {
                        int intGroupCost;
                        int intNakedSkillCost = await SkillGroupObject.SkillList.CountAsync(async x => x == this || await x.GetEnabledAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                        if (intTotalBaseRating == 0)
                        {
                            intGroupCost = objSettings.KarmaNewSkillGroup;
                            intNakedSkillCost *= objSettings.KarmaNewActiveSkill;
                        }
                        else
                        {
                            intGroupCost = (intTotalBaseRating + 1) *
                                           objSettings.KarmaImproveSkillGroup;
                            intNakedSkillCost *= (intTotalBaseRating + 1) *
                                                 objSettings.KarmaImproveActiveSkill;
                        }

                        intSkillGroupCostAdjustment = intGroupCost - intNakedSkillCost;
                        upgrade += intSkillGroupCostAdjustment;
                    }
                }

                decimal decMultiplier = 1.0m;
                decimal decExtra = 0;
                bool blnCreated = await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false);
                string strDictionaryKey = await GetDictionaryKeyAsync(token).ConfigureAwait(false);
                await (await CharacterObject.GetImprovementsAsync(token).ConfigureAwait(false)).ForEachAsync(
                    objLoopImprovement =>
                    {
                        if ((objLoopImprovement.Maximum == 0 || intTotalBaseRating + 1 <= objLoopImprovement.Maximum) &&
                            objLoopImprovement.Minimum <= intTotalBaseRating + 1 &&
                            (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                             (objLoopImprovement.Condition == "career") == blnCreated ||
                             (objLoopImprovement.Condition == "create") != blnCreated) &&
                            objLoopImprovement.Enabled)
                        {
                            if (objLoopImprovement.ImprovedName == strDictionaryKey ||
                                string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.ActiveSkillKarmaCost:
                                        decExtra += objLoopImprovement.Value;
                                        break;

                                    case Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                            else if (objLoopImprovement.ImprovedName == SkillCategory)
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillCategoryKarmaCost:
                                        decExtra += objLoopImprovement.Value;
                                        break;

                                    case Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                        }
                    }, token: token).ConfigureAwait(false);

                if (decMultiplier != 1.0m)
                    upgrade = (upgrade * decMultiplier + decExtra).StandardRound();
                else
                    upgrade += decExtra.StandardRound();

                int intMinCost = Math.Min(1, intOptionsCost);
                if (upgrade < intMinCost + intSkillGroupCostAdjustment)
                    upgrade = intMinCost + intSkillGroupCostAdjustment;
                return upgrade;
            }
        }

        public async ValueTask Upgrade(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                if (await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false))
                {
                    if (!await GetCanUpgradeCareerAsync(token).ConfigureAwait(false))
                        return;

                    int price = await GetUpgradeKarmaCostAsync(token).ConfigureAwait(false);
                    int intTotalBaseRating = await GetTotalBaseRatingAsync(token).ConfigureAwait(false);
                    //If data file contains {4} this crashes but...
                    string upgradetext =
                        string.Format(GlobalSettings.CultureInfo, "{0}{4}{1}{4}{2}{4}->{4}{3}",
                            await LanguageManager.GetStringAsync(IsKnowledgeSkill
                                ? "String_ExpenseKnowledgeSkill"
                                : "String_ExpenseActiveSkill", token: token).ConfigureAwait(false),
                            await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                            intTotalBaseRating,
                            intTotalBaseRating + 1,
                            await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false));

                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(price * -1, upgradetext, ExpenseType.Karma, DateTime.Now);
                    objExpense.Undo = new ExpenseUndo().CreateKarma(
                        intTotalBaseRating == 0 ? KarmaExpenseType.AddSkill : KarmaExpenseType.ImproveSkill,
                        InternalId);

                    await CharacterObject.ExpenseEntries.AddWithSortAsync(objExpense, token: token).ConfigureAwait(false);

                    await CharacterObject.ModifyKarmaAsync(-price, token).ConfigureAwait(false);
                }

                await SetKarmaAsync(await GetKarmaAsync(token).ConfigureAwait(false) + 1, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedCanAffordSpecialization = -1;

        public bool CanAffordSpecialization
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intCachedCanAffordSpecialization < 0)
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            if (_intCachedCanAffordSpecialization < 0) // Just in case
                            {
                                if (!CanHaveSpecs)
                                    _intCachedCanAffordSpecialization = 0;
                                else
                                {
                                    int intPrice = IsKnowledgeSkill
                                        ? CharacterObject.Settings.KarmaKnowledgeSpecialization
                                        : CharacterObject.Settings.KarmaSpecialization;

                                    decimal decExtraSpecCost = 0;
                                    int intTotalBaseRating = TotalBaseRating;
                                    decimal decSpecCostMultiplier = 1.0m;
                                    foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                                    {
                                        if (objLoopImprovement.Minimum <= intTotalBaseRating
                                            && (string.IsNullOrEmpty(objLoopImprovement.Condition)
                                                || (objLoopImprovement.Condition == "career") == CharacterObject.Created
                                                || (objLoopImprovement.Condition == "create") !=
                                                CharacterObject.Created)
                                            && objLoopImprovement.Enabled
                                            && objLoopImprovement.ImprovedName == SkillCategory)
                                        {
                                            switch (objLoopImprovement.ImproveType)
                                            {
                                                case Improvement.ImprovementType.SkillCategorySpecializationKarmaCost:
                                                    decExtraSpecCost += objLoopImprovement.Value;
                                                    break;

                                                case Improvement.ImprovementType
                                                    .SkillCategorySpecializationKarmaCostMultiplier:
                                                    decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                                                    break;
                                            }
                                        }
                                    }

                                    if (decSpecCostMultiplier != 1.0m)
                                        intPrice = (intPrice * decSpecCostMultiplier + decExtraSpecCost)
                                            .StandardRound();
                                    else
                                        intPrice += decExtraSpecCost.StandardRound(); //Spec

                                    _intCachedCanAffordSpecialization = (intPrice <= CharacterObject.Karma).ToInt32();
                                }
                            }
                        }
                    }

                    return _intCachedCanAffordSpecialization > 0;
                }
            }
        }

        public async ValueTask<bool> GetCanAffordSpecializationAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_intCachedCanAffordSpecialization < 0)
                {
                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        if (_intCachedCanAffordSpecialization < 0) // Just in case
                        {
                            if (!CanHaveSpecs)
                                _intCachedCanAffordSpecialization = 0;
                            else
                            {
                                int intPrice = IsKnowledgeSkill
                                    ? CharacterObject.Settings.KarmaKnowledgeSpecialization
                                    : CharacterObject.Settings.KarmaSpecialization;

                                int intTotalBaseRating = await GetTotalBaseRatingAsync(token).ConfigureAwait(false);
                                decimal decSpecCostMultiplier = 1.0m;
                                decimal decExtraSpecCost = await CharacterObject.Improvements.SumAsync(
                                    objLoopImprovement =>
                                    {
                                        if (objLoopImprovement.Minimum <= intTotalBaseRating
                                            && (string.IsNullOrEmpty(objLoopImprovement.Condition)
                                                || (objLoopImprovement.Condition == "career") == CharacterObject.Created
                                                || (objLoopImprovement.Condition == "create") !=
                                                CharacterObject.Created)
                                            && objLoopImprovement.Enabled
                                            && objLoopImprovement.ImprovedName == SkillCategory)
                                        {
                                            switch (objLoopImprovement.ImproveType)
                                            {
                                                case Improvement.ImprovementType.SkillCategorySpecializationKarmaCost:
                                                    return objLoopImprovement.Value;

                                                case Improvement.ImprovementType
                                                                .SkillCategorySpecializationKarmaCostMultiplier:
                                                    decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                                                    break;
                                            }
                                        }

                                        return 0;
                                    }, token: token).ConfigureAwait(false);

                                if (decSpecCostMultiplier != 1.0m)
                                    intPrice = (intPrice * decSpecCostMultiplier + decExtraSpecCost)
                                        .StandardRound();
                                else
                                    intPrice += decExtraSpecCost.StandardRound(); //Spec

                                _intCachedCanAffordSpecialization
                                    = (intPrice <= await CharacterObject.GetKarmaAsync(token).ConfigureAwait(false)).ToInt32();
                            }
                        }
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }

                return _intCachedCanAffordSpecialization > 0;
            }
        }

        public async ValueTask AddSpecialization(string strName, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                SkillSpecialization nspec = new SkillSpecialization(CharacterObject, strName);
                bool blnCreated = await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false);
                if (blnCreated)
                {
                    int intPrice = IsKnowledgeSkill
                        ? CharacterObject.Settings.KarmaKnowledgeSpecialization
                        : CharacterObject.Settings.KarmaSpecialization;

                    decimal decExtraSpecCost = 0;
                    int intTotalBaseRating = await GetTotalBaseRatingAsync(token).ConfigureAwait(false);
                    decimal decSpecCostMultiplier = 1.0m;
                    foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                    {
                        if (objLoopImprovement.Minimum <= intTotalBaseRating
                            && (string.IsNullOrEmpty(objLoopImprovement.Condition)
                                || objLoopImprovement.Condition == "career")
                            && objLoopImprovement.Enabled && objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.SkillCategorySpecializationKarmaCost:
                                    decExtraSpecCost += objLoopImprovement.Value;
                                    break;

                                case Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier:
                                    decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
                    }

                    if (decSpecCostMultiplier != 1.0m)
                        intPrice = (intPrice * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
                    else
                        intPrice += decExtraSpecCost.StandardRound(); //Spec

                    if (intPrice > await CharacterObject.GetKarmaAsync(token).ConfigureAwait(false))
                        return;

                    string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                    string strUpgradeText = await LanguageManager.GetStringAsync("String_ExpenseLearnSpecialization", token: token).ConfigureAwait(false) + strSpace
                        + await GetCurrentDisplayNameAsync(token).ConfigureAwait(false) + strSpace + '(' + strName + ')';
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(intPrice * -1, strUpgradeText, ExpenseType.Karma, DateTime.Now);
                    objExpense.Undo =
                        new ExpenseUndo().CreateKarma(KarmaExpenseType.AddSpecialization, nspec.InternalId);

                    await CharacterObject.ExpenseEntries.AddWithSortAsync(objExpense, token: token).ConfigureAwait(false);

                    await CharacterObject.ModifyKarmaAsync(-intPrice, token).ConfigureAwait(false);
                }

                await Specializations.AddAsync(nspec, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedFreeKarma = int.MinValue;

        /// <summary>
        /// How many free points of this skill have gotten, with the exception of some things during character creation
        /// </summary>
        public int FreeKarma
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intCachedFreeKarma != int.MinValue)
                        return _intCachedFreeKarma;

                    using (LockObject.EnterWriteLock())
                    {
                        if (_intCachedFreeKarma != int.MinValue) // Just in case
                            return _intCachedFreeKarma;
                        return _intCachedFreeKarma = string.IsNullOrEmpty(Name)
                            ? 0
                            : ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillLevel, false,
                                DictionaryKey).StandardRound();
                    }
                }
            }
        }

        /// <summary>
        /// How many free points of this skill have gotten, with the exception of some things during character creation
        /// </summary>
        public async ValueTask<int> GetFreeKarmaAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_intCachedFreeKarma != int.MinValue)
                    return _intCachedFreeKarma;

                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    if (_intCachedFreeKarma != int.MinValue) // Just in case
                        return _intCachedFreeKarma;
                    return _intCachedFreeKarma = string.IsNullOrEmpty(Name)
                        ? 0
                        : (await ImprovementManager.ValueOfAsync(CharacterObject,
                            Improvement.ImprovementType.SkillLevel,
                            false, DictionaryKey, token: token).ConfigureAwait(false)).StandardRound();
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private int _intCachedFreeBase = int.MinValue;

        /// <summary>
        /// How many free points this skill have gotten during some parts of character creation
        /// </summary>
        public int FreeBase
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intCachedFreeBase != int.MinValue)
                        return _intCachedFreeBase;

                    using (LockObject.EnterWriteLock())
                    {
                        if (_intCachedFreeBase != int.MinValue) // Just in case
                            return _intCachedFreeBase;
                        return _intCachedFreeBase = string.IsNullOrEmpty(Name)
                            ? 0
                            : ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillBase, false,
                                DictionaryKey).StandardRound();
                    }
                }
            }
        }

        /// <summary>
        /// How many free points this skill have gotten during some parts of character creation
        /// </summary>
        public async ValueTask<int> GetFreeBaseAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_intCachedFreeBase != int.MinValue)
                    return _intCachedFreeBase;

                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    if (_intCachedFreeBase != int.MinValue) // Just in case
                        return _intCachedFreeBase;
                    return _intCachedFreeBase = string.IsNullOrEmpty(Name)
                        ? 0
                        : (await ImprovementManager.ValueOfAsync(CharacterObject,
                            Improvement.ImprovementType.SkillBase,
                            false, DictionaryKey, token: token).ConfigureAwait(false)).StandardRound();
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private int _intCachedForcedBuyWithKarma = -1;

        /// <summary>
        /// Do circumstances force the skill to be bought with karma?
        /// </summary>
        private bool ForcedBuyWithKarma
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intCachedForcedBuyWithKarma < 0)
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            if (_intCachedForcedBuyWithKarma < 0) // Just in case
                            {
                                _intCachedForcedBuyWithKarma = (!CharacterObject.IgnoreRules
                                                                && Specializations.Any(x => !x.Free)
                                                                && ((KarmaPoints > 0
                                                                     && BasePoints + FreeBase == 0
                                                                     && !CharacterObject.Settings
                                                                         .AllowPointBuySpecializationsOnKarmaSkills)
                                                                    || (CharacterObject.Settings
                                                                            .SpecializationsBreakSkillGroups
                                                                        && (SkillGroupObject?.Karma > 0
                                                                            || SkillGroupObject?.Base > 0)))).ToInt32();
                            }
                        }
                    }

                    return _intCachedForcedBuyWithKarma > 0;
                }
            }
        }

        /// <summary>
        /// Do circumstances force the skill to be bought with karma?
        /// </summary>
        private async ValueTask<bool> GetForcedBuyWithKarmaAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_intCachedForcedNotBuyWithKarma < 0)
                {
                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        if (_intCachedForcedNotBuyWithKarma < 0) // Just in case
                        {
                            CharacterSettings objSettings =
                                await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                            _intCachedForcedNotBuyWithKarma =
                                (!await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false)
                                 && await Specializations.AnyAsync(x => !x.Free, token: token).ConfigureAwait(false)
                                 && ((await GetKarmaPointsAsync(token).ConfigureAwait(false) > 0
                                      && await GetBasePointsAsync(token).ConfigureAwait(false)
                                      + await GetFreeBaseAsync(token).ConfigureAwait(false) == 0
                                      && !await objSettings.GetAllowPointBuySpecializationsOnKarmaSkillsAsync(token)
                                                           .ConfigureAwait(false))
                                     || (await objSettings.GetSpecializationsBreakSkillGroupsAsync(token)
                                                          .ConfigureAwait(false)
                                         && SkillGroupObject != null
                                         && (await SkillGroupObject.GetKarmaAsync(token).ConfigureAwait(false) > 0
                                             || await SkillGroupObject.GetBaseAsync(token).ConfigureAwait(false) > 0))))
                                .ToInt32();
                        }
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }

                return _intCachedForcedNotBuyWithKarma > 0;
            }
        }

        private int _intCachedForcedNotBuyWithKarma = -1;

        /// <summary>
        /// Do circumstances force the skill to not be bought with karma?
        /// </summary>
        private bool ForcedNotBuyWithKarma
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intCachedForcedNotBuyWithKarma < 0)
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            if (_intCachedForcedNotBuyWithKarma < 0) // Just in case
                            {
                                _intCachedForcedNotBuyWithKarma = (TotalBaseRating == 0
                                                                   || (CharacterObject.Settings
                                                                           .StrictSkillGroupsInCreateMode
                                                                       && !CharacterObject.Created
                                                                       && !CharacterObject.IgnoreRules
                                                                       && SkillGroupObject?.Karma > 0)).ToInt32();
                            }
                        }
                    }

                    return _intCachedForcedNotBuyWithKarma > 0;
                }
            }
        }

        /// <summary>
        /// Do circumstances force the skill to not be bought with karma?
        /// </summary>
        private async ValueTask<bool> GetForcedNotBuyWithKarmaAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_intCachedForcedNotBuyWithKarma < 0)
                {
                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        if (_intCachedForcedNotBuyWithKarma < 0)
                        {
                            _intCachedForcedNotBuyWithKarma =
                                (await GetTotalBaseRatingAsync(token).ConfigureAwait(false) == 0
                                 || (await (await CharacterObject.GetSettingsAsync(token)
                                                                 .ConfigureAwait(false))
                                           .GetStrictSkillGroupsInCreateModeAsync(token)
                                           .ConfigureAwait(false)
                                     && !await CharacterObject.GetCreatedAsync(token)
                                                              .ConfigureAwait(false)
                                     && !await CharacterObject.GetIgnoreRulesAsync(token)
                                                              .ConfigureAwait(false)
                                     && SkillGroupObject != null
                                     && await SkillGroupObject.GetKarmaAsync(token)
                                                              .ConfigureAwait(false) > 0)).ToInt32();
                        }
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }

                return _intCachedForcedNotBuyWithKarma > 0;
            }
        }

        /// <summary>
        /// Dicepool of the skill, formatted for use in tooltips by other objects.
        /// </summary>
        /// <param name="intPool">Dicepool to use. In most </param>
        /// <param name="strValidSpec">A specialization to check for. If not empty, will be checked for and added to the string.</param>
        /// <returns></returns>
        public string FormattedDicePool(int intPool, string strValidSpec = "")
        {
            using (EnterReadLock.Enter(LockObject))
            {
                string strSpace = LanguageManager.GetString("String_Space");
                string strReturn = string.Format(GlobalSettings.CultureInfo, "{0}{1}({2})", CurrentDisplayName,
                    strSpace, intPool);
                // Add any Specialization bonus if applicable.
                if (!string.IsNullOrWhiteSpace(strValidSpec))
                {
                    int intSpecBonus = GetSpecializationBonus(strValidSpec);
                    if (intSpecBonus != 0)
                        strReturn +=
                            string.Format(GlobalSettings.CultureInfo, "{0}{1}{0}{2}{3}{0}{4}{0}({5})", strSpace, '+',
                                LanguageManager.GetString("String_ExpenseSpecialization"),
                                LanguageManager.GetString("String_Colon"), strValidSpec, intSpecBonus);
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Dicepool of the skill, formatted for use in tooltips by other objects.
        /// </summary>
        /// <param name="intPool">Dicepool to use. In most </param>
        /// <param name="strValidSpec">A specialization to check for. If not empty, will be checked for and added to the string.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async ValueTask<string> FormattedDicePoolAsync(int intPool, string strValidSpec = "", CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                string strReturn = string.Format(GlobalSettings.CultureInfo, "{0}{1}({2})", await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                                                 strSpace, intPool);
                // Add any Specialization bonus if applicable.
                if (!string.IsNullOrWhiteSpace(strValidSpec))
                {
                    int intSpecBonus = await GetSpecializationBonusAsync(strValidSpec, token).ConfigureAwait(false);
                    if (intSpecBonus != 0)
                        strReturn +=
                            string.Format(GlobalSettings.CultureInfo, "{0}{1}{0}{2}{3}{0}{4}{0}({5})", strSpace, '+',
                                          await LanguageManager.GetStringAsync("String_ExpenseSpecialization", token: token).ConfigureAwait(false),
                                          await LanguageManager.GetStringAsync("String_Colon", token: token).ConfigureAwait(false), strValidSpec, intSpecBonus);
                }

                return strReturn;
            }
        }

        public void Remove()
        {
            using (LockObject.EnterWriteLock())
            {
                SkillGroupObject?.Remove(this);
            }
            Dispose();
        }

        public async ValueTask RemoveAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                SkillGroup objGroup = SkillGroupObject;
                if (objGroup != null)
                    await objGroup.RemoveAsync(this, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            await DisposeAsync().ConfigureAwait(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CharacterObject.PropertyChanged -= OnCharacterChanged;
                CharacterObject.Settings.PropertyChanged -= OnCharacterSettingsPropertyChanged;
                CharacterObject.AttributeSection.PropertyChanged -= OnAttributeSectionChanged;
                CharacterObject.AttributeSection.Attributes.CollectionChanged -= OnAttributesCollectionChanged;

                if (AttributeObject != null)
                    AttributeObject.PropertyChanged -= OnLinkedAttributeChanged;
                if (SkillGroupObject != null)
                    SkillGroupObject.PropertyChanged -= OnSkillGroupChanged;
                foreach (SkillSpecialization objSpec in _lstSpecializations)
                    objSpec.Dispose();
                _lstSpecializations.Dispose();
                if (_lstCachedSuggestedSpecializations != null)
                    Utils.ListItemListPool.Return(_lstCachedSuggestedSpecializations);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                Dispose(true);
            }
            LockObject.Dispose();

            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                CharacterObject.PropertyChanged -= OnCharacterChanged;
                CharacterObject.Settings.PropertyChanged -= OnCharacterSettingsPropertyChanged;
                CharacterObject.AttributeSection.PropertyChanged -= OnAttributeSectionChanged;
                CharacterObject.AttributeSection.Attributes.CollectionChanged -= OnAttributesCollectionChanged;

                if (AttributeObject != null)
                    AttributeObject.PropertyChanged -= OnLinkedAttributeChanged;
                if (SkillGroupObject != null)
                    SkillGroupObject.PropertyChanged -= OnSkillGroupChanged;
                await _lstSpecializations.ForEachAsync(x => x.Dispose()).ConfigureAwait(false);
                await _lstSpecializations.DisposeAsync().ConfigureAwait(false);
                if (_lstCachedSuggestedSpecializations != null)
                    Utils.ListItemListPool.Return(_lstCachedSuggestedSpecializations);
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                Dispose(true);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            await LockObject.DisposeAsync().ConfigureAwait(false);

            GC.SuppressFinalize(this);
        }
    }
}
