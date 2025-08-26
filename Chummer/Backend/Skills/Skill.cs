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
    public class Skill : INotifyMultiplePropertiesChangedAsync, IHasName, IHasSourceId, IHasXmlDataNode, IHasNotes, IHasLockObject, IHasCharacterObject
    {
        private CharacterAttrib _objAttribute;
        private string _strDefaultAttribute = "LOG";
        private bool _blnRequiresGroundMovement;
        private bool _blnRequiresSwimMovement;
        private bool _blnRequiresFlyMovement;
        private int _intBase;
        private int _intKarma;
        private bool _blnBuyWithKarma;
        private int _intIsDisposed;

        private int _intIsLoading = 1;
        public bool IsLoading
        {
            get => _intIsLoading > 0;
            set
            {
                if (value)
                {
                    LockObject.SetParent();
                    Interlocked.Increment(ref _intIsLoading);
                }
                else
                {
                    int intNewValue = Interlocked.Decrement(ref _intIsLoading);
                    if (intNewValue < 0)
                        Interlocked.Increment(ref _intIsLoading);
                    LockObject.SetParent(CharacterObject.LockObject);
                }
            }
        }

        public async Task SetIsLoadingAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (value)
            {
                await LockObject.SetParentAsync(token: token).ConfigureAwait(false);
                Interlocked.Increment(ref _intIsLoading);
            }
            else
            {
                int intNewValue = Interlocked.Decrement(ref _intIsLoading);
                if (intNewValue < 0)
                    Interlocked.Increment(ref _intIsLoading);
                await LockObject.SetParentAsync(CharacterObject.LockObject, token: token).ConfigureAwait(false);
            }
        }

        public AsyncFriendlyReaderWriterLock LockObject { get; }

        public CharacterAttrib AttributeObject
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objAttribute;
            }
            private set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _objAttribute, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        public async Task<CharacterAttrib> GetAttributeObjectAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _objAttribute;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected void RecacheAttribute()
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                string strAttributeString = DefaultAttribute;
                if (!IsLoading)
                {
                    Improvement objImprovementOverride = ImprovementManager
                        .GetCachedImprovementListForValueOf(
                            CharacterObject, Improvement.ImprovementType.SwapSkillAttribute)
                        .Find(x => x.Target == DictionaryKey);
                    if (objImprovementOverride != null)
                        strAttributeString = objImprovementOverride.ImprovedName;
                }

                CharacterAttrib objNewAttribute = CharacterObject.GetAttribute(strAttributeString);
                CharacterAttrib objOldAttribute = Interlocked.Exchange(ref _objAttribute, objNewAttribute);
                if (objOldAttribute == objNewAttribute)
                    return;
                if (objOldAttribute != null)
                {
                    try
                    {
                        objOldAttribute.MultiplePropertiesChangedAsync -= OnLinkedAttributeChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                }
                if (objNewAttribute != null)
                    objNewAttribute.MultiplePropertiesChangedAsync += OnLinkedAttributeChanged;
                this.OnMultiplePropertyChanged(nameof(AttributeModifiers), nameof(Enabled));
            }
        }

        protected async Task RecacheAttributeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strAttributeString = await GetDefaultAttributeAsync(token).ConfigureAwait(false);
                if (!IsLoading)
                {
                    string strDictionaryKey = await GetDictionaryKeyAsync(token).ConfigureAwait(false);
                    Improvement objImprovementOverride = (await ImprovementManager
                            .GetCachedImprovementListForValueOfAsync(
                                CharacterObject,
                                Improvement.ImprovementType.SwapSkillAttribute,
                                token: token).ConfigureAwait(false))
                        .Find(x => x.Target == strDictionaryKey);
                    if (objImprovementOverride != null)
                        strAttributeString = objImprovementOverride.ImprovedName;
                }

                CharacterAttrib objNewAttribute = await CharacterObject.GetAttributeAsync(strAttributeString, token: token).ConfigureAwait(false);
                CharacterAttrib objOldAttribute = Interlocked.Exchange(ref _objAttribute, objNewAttribute);
                if (objOldAttribute == objNewAttribute)
                    return;
                if (objOldAttribute != null)
                {
                    try
                    {
                        objOldAttribute.MultiplePropertiesChangedAsync -= OnLinkedAttributeChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                }
                if (objNewAttribute != null)
                    objNewAttribute.MultiplePropertiesChangedAsync += OnLinkedAttributeChanged;

                await this.OnMultiplePropertyChangedAsync(token, nameof(AttributeModifiers), nameof(Enabled))
                    .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private string _strName = string.Empty; //English name of this skill
        private string _strNotes = string.Empty; //Text of any notes that were entered by the user
        private Color _colNotes = ColorManager.HasNotesColor;
        private bool _blnDefault;

        public void WriteTo(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            using (LockObject.EnterReadLock())
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
                objWriter.WriteElementString("notes", _strNotes.CleanOfXmlInvalidUnicodeChars());
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

        public async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;

            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // <skill>
                XmlElementWriteHelper objBaseElement =
                    await objWriter.StartElementAsync("skill", token: token).ConfigureAwait(false);
                try
                {
                    int intPool = await GetPoolAsync(token).ConfigureAwait(false);
                    int intSpecPool = intPool + await GetSpecializationBonusAsync(token: token).ConfigureAwait(false);
                    string strAttribute = await GetAttributeAsync(token).ConfigureAwait(false);
                    int intRatingModifiers = await RatingModifiersAsync(strAttribute, token: token).ConfigureAwait(false);
                    int intDicePoolModifiers = await PoolModifiersAsync(strAttribute, token: token).ConfigureAwait(false);
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
                            ? (SkillGroupObject != null &&
                               !await SkillGroupObject.GetIsBrokenAsync(token).ConfigureAwait(false) &&
                               await SkillGroupObject.GetRatingAsync(token).ConfigureAwait(false) > 0)
                            .ToString(GlobalSettings.InvariantCultureInfo)
                            : (SkillGroupObject != null && !await SkillGroupObject.GetHasAnyBreakingSkillsAsync(token)
                                   .ConfigureAwait(false) &&
                               await SkillGroupObject.GetRatingAsync(token).ConfigureAwait(false) > 0)
                            .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("default", (await GetDefaultAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
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
                    await objWriter.WriteElementStringAsync("attribute", strAttribute, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("displayattribute",
                        await DisplayAttributeMethodAsync(strLanguageToPrint, token).ConfigureAwait(false),
                        token: token).ConfigureAwait(false);
                    if (GlobalSettings.PrintNotes)
                        await objWriter.WriteElementStringAsync("notes", await GetNotesAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("source",
                        await CharacterObject.LanguageBookShortAsync(Source, strLanguageToPrint, token)
                            .ConfigureAwait(false), token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("page",
                            await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("attributemod",
                        (await (await CharacterObject.GetAttributeAsync(strAttribute,
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
                            (await GetIsNativeLanguageAsync(token).ConfigureAwait(false)).ToString(GlobalSettings
                                .InvariantCultureInfo), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("bp",
                        (await GetCurrentKarmaCostAsync(token).ConfigureAwait(false)).ToString(objCulture),
                        token: token).ConfigureAwait(false);
                    // <skillspecializations>
                    XmlElementWriteHelper objSkillSpecializationsElement = await objWriter
                        .StartElementAsync("skillspecializations", token: token).ConfigureAwait(false);
                    try
                    {
                        await (await GetSpecializationsAsync(token).ConfigureAwait(false))
                            .ForEachAsync(
                                objSpec => objSpec.Print(objWriter, objCulture, strLanguageToPrint, token: token),
                                token).ConfigureAwait(false);
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
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #region Factory

        /// <summary>
        /// Load a skill from a xml node from a saved .chum5 file
        /// </summary>
        /// <param name="objCharacter">The character this skill belongs to</param>
        /// <param name="xmlSkillNode">The XML node describing the skill</param>
        /// <param name="objLoadingSkill">Pre-existing skill object into which to load (if it exists)</param>
        /// <returns></returns>
        public static Skill Load(Character objCharacter, XmlNode xmlSkillNode, Skill objLoadingSkill = null)
        {
            return Load(objCharacter, xmlSkillNode, out bool _, objLoadingSkill);
        }

        /// <summary>
        /// Load a skill from a xml node from a saved .chum5 file
        /// </summary>
        /// <param name="objCharacter">The character this skill belongs to</param>
        /// <param name="xmlSkillNode">The XML node describing the skill</param>
        /// <param name="objLoadingSkill">Pre-existing skill object into which to load (if it exists)</param>
        /// <returns></returns>
        public static Skill Load(Character objCharacter, XmlNode xmlSkillNode, out bool blnNewSkill, Skill objLoadingSkill = null)
        {
            if (!xmlSkillNode.TryGetField("suid", Guid.TryParse, out Guid suid))
            {
                blnNewSkill = false;
                return null;
            }

            blnNewSkill = true;
            Guid guidSkillId = xmlSkillNode.TryGetField("id", Guid.TryParse, out Guid guiTemp) ? guiTemp : suid;

            bool blnIsKnowledgeSkill = false;
            if (xmlSkillNode.TryGetBoolFieldQuickly("isknowledge", ref blnIsKnowledgeSkill) && blnIsKnowledgeSkill)
            {
                if (!(objLoadingSkill is KnowledgeSkill objKnowledgeSkill))
                {
                    objKnowledgeSkill = null;
                    if (guidSkillId != Guid.Empty)
                        objKnowledgeSkill =
                            objCharacter.SkillsSection.KnowledgeSkills.Find(x => x.SkillId == guidSkillId);
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

                        try
                        {
                            objKnowledgeSkill.IsLoading = true;
                            objKnowledgeSkill.Load(xmlSkillNode);
                            objLoadingSkill = objKnowledgeSkill;
                        }
                        catch
                        {
                            objKnowledgeSkill.Remove();
                            throw;
                        }
                    }
                    else
                    {
                        blnNewSkill = false;
                        objKnowledgeSkill.Load(xmlSkillNode);
                        objLoadingSkill = objKnowledgeSkill;
                    }
                }
            }
            else if (objLoadingSkill == null && suid != Guid.Empty)
            {
                if (guidSkillId != Guid.Empty)
                {
                    objLoadingSkill
                        = objCharacter.SkillsSection.Skills.Find(x => x.SkillId == guidSkillId);
                    if (objLoadingSkill != null)
                    {
                        if (objLoadingSkill.IsExoticSkill)
                        {
                            objLoadingSkill = null;
                            string strSpecific = string.Empty;
                            if (xmlSkillNode.TryGetStringFieldQuickly("specific", ref strSpecific))
                            {
                                objLoadingSkill
                                    = objCharacter.SkillsSection.Skills.Find(x =>
                                        x.SkillId == guidSkillId && x is ExoticSkill y && y.Specific == strSpecific);
                                if (objLoadingSkill is ExoticSkill objLoadingExoticSkill)
                                {
                                    blnNewSkill = false;
                                    objLoadingExoticSkill.Load(xmlSkillNode);
                                }
                            }
                        }
                        else
                            blnNewSkill = false;
                    }
                }

                if (objLoadingSkill == null)
                {
                    XmlNode xmlSkillDataNode = objCharacter.LoadData("skills.xml")
                        .TryGetNodeById("/chummer/skills/skill", suid);

                    if (xmlSkillDataNode == null)
                        return null;

                    bool blnExotic = false;
                    xmlSkillDataNode.TryGetBoolFieldQuickly("exotic", ref blnExotic);
                    if (blnExotic)
                    {
                        ExoticSkill exotic = FromData(xmlSkillDataNode, objCharacter, false) as ExoticSkill
                                             ?? throw new ArgumentException(
                                                 "Attempted to load non-exotic skill as exotic skill");
                        try
                        {
                            exotic.IsLoading = true;
                            exotic.Load(xmlSkillNode);
                            objLoadingSkill = exotic;
                        }
                        catch
                        {
                            exotic.Remove();
                            throw;
                        }
                    }
                    else
                    {
                        objLoadingSkill = new Skill(objCharacter, xmlSkillDataNode);
                        try
                        {
                            objLoadingSkill.IsLoading = true;
                        }
                        catch
                        {
                            objLoadingSkill.Remove();
                            throw;
                        }
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
                {
                    objLoadingSkill = new KnowledgeSkill(objCharacter,
                        xmlSkillNode["name"]?.InnerText ?? string.Empty,
                        !Convert.ToBoolean(
                            xmlSkillNode["disableupgrades"]?.InnerText,
                            GlobalSettings.InvariantCultureInfo));
                    try
                    {
                        objLoadingSkill.IsLoading = true;
                    }
                    catch
                    {
                        objLoadingSkill.Remove();
                        throw;
                    }
                }
                else
                {
                    KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(objCharacter);
                    try
                    {
                        objKnowledgeSkill.IsLoading = true;
                        objKnowledgeSkill.Load(xmlSkillNode);
                        objLoadingSkill = objKnowledgeSkill;
                    }
                    catch
                    {
                        objKnowledgeSkill.Remove();
                        throw;
                    }
                }
            }

            try
            {
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
                        if (xmlSpecList != null)
                        {
                            foreach (XmlNode xmlSpec in xmlSpecList)
                            {
                                SkillSpecialization objSpec = SkillSpecialization.Load(objCharacter, xmlSpec);
                                if (objSpec != null)
                                {
                                    try
                                    {
                                        objLoadingSkill._lstSpecializations.Add(objSpec);
                                    }
                                    catch
                                    {
                                        objSpec.Dispose();
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                }

                return objLoadingSkill;
            }
            catch
            {
                if (blnNewSkill)
                    objLoadingSkill.Remove();
                throw;
            }
            finally
            {
                objLoadingSkill.IsLoading = false;
            }
        }

        /// <summary>
        /// Load a skill from a xml node from a saved .chum5 file
        /// </summary>
        /// <param name="objCharacter">The character this skill belongs to</param>
        /// <param name="xmlSkillNode">The XML node describing the skill</param>
        /// <param name="objLoadingSkill">Pre-existing skill object into which to load (if it exists)</param>
        public static async Task<Tuple<Skill, bool>> LoadAsync(Character objCharacter, XmlNode xmlSkillNode, Skill objLoadingSkill = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!xmlSkillNode.TryGetField("suid", Guid.TryParse, out Guid suid))
            {
                return new Tuple<Skill, bool>(null, false);
            }

            bool blnNewSkill = true;
            Guid guidSkillId = xmlSkillNode.TryGetField("id", Guid.TryParse, out Guid guiTemp) ? guiTemp : suid;

            SkillsSection objSkillsSection = await objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false);
            bool blnIsKnowledgeSkill = false;
            if (xmlSkillNode.TryGetBoolFieldQuickly("isknowledge", ref blnIsKnowledgeSkill) && blnIsKnowledgeSkill)
            {
                if (!(objLoadingSkill is KnowledgeSkill objKnowledgeSkill))
                {
                    objKnowledgeSkill = null;
                    if (guidSkillId != Guid.Empty)
                        objKnowledgeSkill =
                            await (await objSkillsSection.GetKnowledgeSkillsAsync(token).ConfigureAwait(false))
                                .FirstOrDefaultAsync(async x => await x.GetSkillIdAsync(token).ConfigureAwait(false) == guidSkillId, token).ConfigureAwait(false);
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

                        try
                        {
                            objKnowledgeSkill.IsLoading = true;
                            await objKnowledgeSkill.LoadAsync(xmlSkillNode, token).ConfigureAwait(false);
                            objLoadingSkill = objKnowledgeSkill;
                        }
                        catch
                        {
                            await objKnowledgeSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                            throw;
                        }
                    }
                    else
                    {
                        blnNewSkill = false;
                        await objKnowledgeSkill.LoadAsync(xmlSkillNode, token).ConfigureAwait(false);
                        objLoadingSkill = objKnowledgeSkill;
                    }
                }
            }
            else if (objLoadingSkill == null && suid != Guid.Empty)
            {
                if (guidSkillId != Guid.Empty)
                {
                    ThreadSafeBindingList<Skill> lstSkills = await objSkillsSection.GetSkillsAsync(token).ConfigureAwait(false);
                    objLoadingSkill
                        = await lstSkills.FirstOrDefaultAsync(async x => await x.GetSkillIdAsync(token).ConfigureAwait(false) == guidSkillId, token).ConfigureAwait(false);
                    if (objLoadingSkill != null)
                    {
                        if (objLoadingSkill.IsExoticSkill)
                        {
                            objLoadingSkill = null;
                            string strSpecific = string.Empty;
                            if (xmlSkillNode.TryGetStringFieldQuickly("specific", ref strSpecific))
                            {
                                objLoadingSkill
                                    = await lstSkills.FirstOrDefaultAsync(async x => await x.GetSkillIdAsync(token).ConfigureAwait(false) == guidSkillId
                                        && x is ExoticSkill y
                                        && await y.GetSpecificAsync(token).ConfigureAwait(false) == strSpecific, token).ConfigureAwait(false);
                                if (objLoadingSkill is ExoticSkill objLoadingExoticSkill)
                                {
                                    blnNewSkill = false;
                                    await objLoadingExoticSkill.LoadAsync(xmlSkillNode, token).ConfigureAwait(false);
                                }
                            }
                        }
                        else
                            blnNewSkill = false;
                    }
                }

                if (objLoadingSkill == null)
                {
                    XmlNode xmlSkillDataNode = (await objCharacter.LoadDataAsync("skills.xml", token: token).ConfigureAwait(false))
                        .TryGetNodeById("/chummer/skills/skill", suid);

                    if (xmlSkillDataNode == null)
                        return null;

                    bool blnExotic = false;
                    xmlSkillDataNode.TryGetBoolFieldQuickly("exotic", ref blnExotic);
                    if (blnExotic)
                    {
                        ExoticSkill exotic = await FromDataAsync(xmlSkillDataNode, objCharacter, false, token).ConfigureAwait(false) as ExoticSkill
                                             ?? throw new ArgumentException(
                                                 "Attempted to load non-exotic skill as exotic skill");
                        try
                        {
                            exotic.IsLoading = true;
                            await exotic.LoadAsync(xmlSkillNode, token).ConfigureAwait(false);
                            objLoadingSkill = exotic;
                        }
                        catch
                        {
                            await exotic.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                            throw;
                        }
                    }
                    else
                    {
                        objLoadingSkill = new Skill(objCharacter, xmlSkillDataNode);
                        try
                        {
                            objLoadingSkill.IsLoading = true;
                        }
                        catch
                        {
                            await objLoadingSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                            throw;
                        }
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
                {
                    objLoadingSkill = new KnowledgeSkill(objCharacter,
                        xmlSkillNode["name"]?.InnerText ?? string.Empty,
                        !Convert.ToBoolean(
                            xmlSkillNode["disableupgrades"]?.InnerText,
                            GlobalSettings.InvariantCultureInfo));
                    try
                    {
                        objLoadingSkill.IsLoading = true;
                    }
                    catch
                    {
                        await objLoadingSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                        throw;
                    }
                }
                else
                {
                    KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(objCharacter);
                    try
                    {
                        objKnowledgeSkill.IsLoading = true;
                        await objKnowledgeSkill.LoadAsync(xmlSkillNode, token).ConfigureAwait(false);
                        objLoadingSkill = objKnowledgeSkill;
                    }
                    catch
                    {
                        await objKnowledgeSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                        throw;
                    }
                }
            }

            try
            {
                if (xmlSkillNode.TryGetField("guid", Guid.TryParse, out guiTemp))
                    objLoadingSkill.Id = guiTemp;

                if (!xmlSkillNode.TryGetMultiLineStringFieldQuickly("altnotes", ref objLoadingSkill._strNotes))
                    xmlSkillNode.TryGetMultiLineStringFieldQuickly("notes", ref objLoadingSkill._strNotes);

                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                xmlSkillNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                objLoadingSkill._colNotes = ColorTranslator.FromHtml(sNotesColor);

                if (!await objLoadingSkill.GetIsNativeLanguageAsync(token).ConfigureAwait(false))
                {
                    xmlSkillNode.TryGetInt32FieldQuickly("karma", ref objLoadingSkill._intKarma);
                    xmlSkillNode.TryGetInt32FieldQuickly("base", ref objLoadingSkill._intBase);
                    xmlSkillNode.TryGetBoolFieldQuickly("buywithkarma", ref objLoadingSkill._blnBuyWithKarma);
                    using (XmlNodeList xmlSpecList = xmlSkillNode.SelectNodes("specs/spec"))
                    {
                        if (xmlSpecList != null)
                        {
                            foreach (XmlNode xmlSpec in xmlSpecList)
                            {
                                SkillSpecialization objSpec = SkillSpecialization.Load(objCharacter, xmlSpec);
                                if (objSpec != null)
                                {
                                    try
                                    {
                                        await objLoadingSkill._lstSpecializations.AddAsync(objSpec, token).ConfigureAwait(false);
                                    }
                                    catch
                                    {
                                        await objSpec.DisposeAsync().ConfigureAwait(false);
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                }

                return new Tuple<Skill, bool>(objLoadingSkill, blnNewSkill);
            }
            catch
            {
                if (blnNewSkill)
                    await objLoadingSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                throw;
            }
            finally
            {
                objLoadingSkill.IsLoading = false;
            }
        }

        /// <summary>
        /// Loads skill saved in legacy format
        /// </summary>
        public static Skill LegacyLoad(Character objCharacter, XmlNode xmlSkillNode)
        {
            return Utils.SafelyRunSynchronously(() => LegacyLoadCoreAsync(true, objCharacter, xmlSkillNode));
        }

        /// <summary>
        /// Loads skill saved in legacy format
        /// </summary>
        public static Task<Skill> LegacyLoadAsync(Character objCharacter, XmlNode xmlSkillNode, CancellationToken token = default)
        {
            return LegacyLoadCoreAsync(false, objCharacter, xmlSkillNode, token);
        }

        private static async Task<Skill> LegacyLoadCoreAsync(bool blnSync, Character objCharacter, XmlNode xmlSkillNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
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
                KnowledgeSkill objKnoSkill = new KnowledgeSkill(objCharacter);
                try
                {
                    if (blnSync)
                    {
                        objKnoSkill.WritableName = strName;
                        objKnoSkill.Base = intBaseRating;
                        objKnoSkill.Karma = intKarmaRating;
                        objKnoSkill.Type = xmlSkillNode["skillcategory"]?.InnerText;
                    }
                    else
                    {
                        await objKnoSkill.SetWritableNameAsync(strName, token).ConfigureAwait(false);
                        await objKnoSkill.SetBaseAsync(intBaseRating, token).ConfigureAwait(false);
                        await objKnoSkill.SetKarmaAsync(intKarmaRating, token).ConfigureAwait(false);
                        await objKnoSkill.SetTypeAsync(xmlSkillNode["skillcategory"]?.InnerText, token).ConfigureAwait(false);
                    }
                    objSkill = objKnoSkill;
                }
                catch
                {
                    if (blnSync)
                        objKnoSkill.Remove();
                    else
                        await objKnoSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                    throw;
                }
            }
            else
            {
                XmlDocument xmlSkillsDocument = blnSync ? objCharacter.LoadData("skills.xml", token: token) : await objCharacter.LoadDataAsync("skills.xml", token: token).ConfigureAwait(false);
                XmlNode xmlSkillDataNode = xmlSkillsDocument.TryGetNodeById("/chummer/skills/skill", suid)
                    //Some stuff apparently have a guid of 0000-000... (only exotic?)
                    ?? xmlSkillsDocument.TryGetNodeByNameOrId("/chummer/skills/skill", strName);

                bool blnIsKnowledgeSkill = xmlSkillDataNode != null
                                           && xmlSkillsDocument
                                              .SelectSingleNodeAndCacheExpressionAsNavigator(
                                                  "/chummer/categories/category[. = "
                                                  + xmlSkillDataNode["category"]?.InnerText.CleanXPath() + "]/@type", token)
                                              ?.Value != "active";

                objSkill = blnSync ? FromData(xmlSkillDataNode, objCharacter, blnIsKnowledgeSkill) : await FromDataAsync(xmlSkillDataNode, objCharacter, blnIsKnowledgeSkill, token).ConfigureAwait(false);
                try
                {
                    objSkill._intBase = intBaseRating;
                    objSkill._intKarma = intKarmaRating;

                    if (objSkill is ExoticSkill objExoticSkill)
                    {
                        //don't need to do more load then.
                        if (blnSync)
                            objExoticSkill.Specific = xmlSkillNode.SelectSingleNodeAndCacheExpressionAsNavigator("skillspecializations/skillspecialization/name", token)?.Value ?? string.Empty;
                        else
                            await objExoticSkill.SetSpecificAsync(xmlSkillNode.SelectSingleNodeAndCacheExpressionAsNavigator("skillspecializations/skillspecialization/name", token)?.Value ?? string.Empty, token).ConfigureAwait(false);
                        return objSkill;
                    }

                    xmlSkillNode.TryGetBoolFieldQuickly("buywithkarma", ref objSkill._blnBuyWithKarma);
                }
                catch
                {
                    if (blnSync)
                        objSkill?.Remove();
                    else if (objSkill != null)
                        await objSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                    throw;
                }
            }

            try
            {
                using (XmlNodeList xmlSpecList = xmlSkillNode.SelectNodes("skillspecializations/skillspecialization"))
                {
                    if (xmlSpecList?.Count > 0)
                    {
                        foreach (XmlNode xmlSpec in xmlSpecList)
                        {
                            SkillSpecialization objSpec = SkillSpecialization.Load(objCharacter, xmlSpec);
                            if (objSpec != null)
                            {
                                try
                                {
                                    if (blnSync)
                                        objSkill.Specializations.Add(objSpec);
                                    else
                                        await (await objSkill.GetSpecializationsAsync(token).ConfigureAwait(false)).AddAsync(objSpec, token).ConfigureAwait(false);
                                }
                                catch
                                {
                                    try
                                    {
                                        if (blnSync)
                                            objSkill.Specializations.Remove(objSpec);
                                        else
                                            await (await objSkill.GetSpecializationsAsync(token).ConfigureAwait(false)).RemoveAsync(objSpec, CancellationToken.None).ConfigureAwait(false);
                                    }
                                    catch
                                    {
                                        //swallow this
                                    }
                                    if (blnSync)
                                        objSpec.Dispose();
                                    else
                                        await objSpec.DisposeAsync().ConfigureAwait(false);
                                    throw;
                                }
                            }
                        }
                    }
                }

                return objSkill;
            }
            catch
            {
                if (blnSync)
                    objSkill?.Remove();
                else if (objSkill != null)
                    await objSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                throw;
            }
        }

        public static Skill LoadFromHeroLab(Character objCharacter, XPathNavigator xmlSkillNode, bool blnIsKnowledgeSkill, string strSkillType = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlSkillNode == null)
                throw new ArgumentNullException(nameof(xmlSkillNode));
            string strName = xmlSkillNode.SelectSingleNodeAndCacheExpression("@name", token)?.Value ?? string.Empty;

            XmlNode xmlSkillDataNode = objCharacter.LoadData("skills.xml", token: token)
                                                   .TryGetNodeByNameOrId(blnIsKnowledgeSkill
                                                                             ? "/chummer/knowledgeskills/skill"
                                                                             : "/chummer/skills/skill", strName);
            if (xmlSkillDataNode == null || !xmlSkillDataNode.TryGetField("id", Guid.TryParse, out Guid suid))
                suid = Guid.NewGuid();

            bool blnIsNativeLanguage = false;
            int intKarmaRating = 0;
            if (xmlSkillNode.SelectSingleNodeAndCacheExpression("@text", token)?.Value == "N")
                blnIsNativeLanguage = true;
            else if (!int.TryParse(xmlSkillNode.SelectSingleNodeAndCacheExpression("@base", token)?.Value, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out intKarmaRating)) // Only reading karma rating out for now, any base rating will need modification within SkillsSection
                intKarmaRating = 0;

            Skill objSkill;
            if (blnIsKnowledgeSkill)
            {
                KnowledgeSkill objKnoSkill = new KnowledgeSkill(objCharacter);
                try
                {
                    objKnoSkill.WritableName = strName;
                    objKnoSkill.Karma = intKarmaRating;
                    objKnoSkill.Type = !string.IsNullOrEmpty(strSkillType) ? strSkillType : xmlSkillDataNode?["category"]?.InnerText ?? "Academic";
                    objKnoSkill.IsNativeLanguage = blnIsNativeLanguage;
                }
                catch
                {
                    objKnoSkill.Remove();
                    throw;
                }
                objSkill = objKnoSkill;
            }
            else
            {
                objSkill = FromData(xmlSkillDataNode, objCharacter, false);
                try
                {
                    if (xmlSkillNode.SelectSingleNodeAndCacheExpression("@fromgroup", token)?.Value == "yes")
                    {
                        intKarmaRating -= objSkill.SkillGroupObject.Karma;
                    }
                    objSkill._intKarma = intKarmaRating;

                    if (objSkill is ExoticSkill objExoticSkill)
                    {
                        string strSpecializationName = xmlSkillNode.SelectSingleNodeAndCacheExpression("specialization/@bonustext", token)?.Value ?? string.Empty;
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
                catch
                {
                    objSkill?.Remove();
                    throw;
                }
            }

            try
            {
                objSkill.SkillId = suid;

                foreach (XPathNavigator xmlSpecializationNode in xmlSkillNode.SelectAndCacheExpression("specialization",
                             token))
                {
                    string strSpecializationName = xmlSpecializationNode
                        .SelectSingleNodeAndCacheExpression("@bonustext", token)?.Value;
                    if (string.IsNullOrEmpty(strSpecializationName))
                        continue;
                    int intLastPlus = strSpecializationName.LastIndexOf('+');
                    if (intLastPlus > strSpecializationName.Length)
                        strSpecializationName = strSpecializationName.Substring(0, intLastPlus - 1);
                    SkillSpecialization objSpec = new SkillSpecialization(objCharacter, strSpecializationName);
                    try
                    {
                        objSkill.Specializations.Add(objSpec);
                    }
                    catch
                    {
                        try
                        {
                            objSkill.Specializations.Remove(objSpec);
                        }
                        catch
                        {
                            //swallow this
                        }
                        objSpec.Dispose();
                        throw;
                    }
                }

                return objSkill;
            }
            catch
            {
                objSkill.Remove();
                throw;
            }
        }

        //TODO CACHE INVALIDATE

        /// <summary>
        /// Load a skill from a data file describing said skill
        /// </summary>
        /// <param name="xmlNode">The XML node describing the skill</param>
        /// <param name="objCharacter">The character the skill belongs to</param>
        /// <param name="blnIsKnowledgeSkill">Whether this skill is a knowledge skill.</param>
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

        /// <summary>
        /// Load a skill from a data file describing said skill
        /// </summary>
        /// <param name="xmlNode">The XML node describing the skill</param>
        /// <param name="objCharacter">The character the skill belongs to</param>
        /// <param name="blnIsKnowledgeSkill">Whether this skill is a knowledge skill.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public static async Task<Skill> FromDataAsync(XmlNode xmlNode, Character objCharacter, bool blnIsKnowledgeSkill,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlNode == null)
                return null;
            if (xmlNode["exotic"]?.InnerText == bool.TrueString)
            {
                //load exotic skill
                ExoticSkill objExoticReturn = new ExoticSkill(objCharacter, xmlNode, false);
                try
                {
                    await objExoticReturn.SetIsLoadingAsync(true, token).ConfigureAwait(false);
                    string strExoticGroup = xmlNode["skillgroup"]?.InnerText;

                    if (!string.IsNullOrEmpty(strExoticGroup))
                    {
                        objExoticReturn.SkillGroup = strExoticGroup;
                        SkillGroup objGroup =
                            await Skills.SkillGroup.GetAsync(objExoticReturn, token).ConfigureAwait(false);
                        objExoticReturn.SkillGroupObject = objGroup;
                        if (objGroup != null)
                        {
                            objGroup.MultiplePropertiesChangedAsync += objExoticReturn.OnSkillGroupChanged;
                        }
                    }
                }
                catch
                {
                    await objExoticReturn.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                    throw;
                }
                finally
                {
                    await objExoticReturn.SetIsLoadingAsync(false, token).ConfigureAwait(false);
                }

                return objExoticReturn;
            }

            if (blnIsKnowledgeSkill)
            {
                //TODO INIT SKILL
                Utils.BreakIfDebug();

                KnowledgeSkill objKnoSkillReturn = new KnowledgeSkill(objCharacter, false);
                try
                {
                    await objKnoSkillReturn.SetIsLoadingAsync(true, token).ConfigureAwait(false);
                    await objKnoSkillReturn.SetDefaultAttributeAsync("LOG", token).ConfigureAwait(false);
                }
                catch
                {
                    await objKnoSkillReturn.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                    throw;
                }
                finally
                {
                    await objKnoSkillReturn.SetIsLoadingAsync(false, token).ConfigureAwait(false);
                }

                return objKnoSkillReturn;
            }

            //TODO INIT SKILL

            Skill objReturn = new Skill(objCharacter, xmlNode, false);
            try
            {
                await objReturn.SetIsLoadingAsync(true, token).ConfigureAwait(false);
                string strGroup = xmlNode["skillgroup"]?.InnerText;

                if (!string.IsNullOrEmpty(strGroup))
                {
                    objReturn.SkillGroup = strGroup;
                    SkillGroup objGroup = await Skills.SkillGroup.GetAsync(objReturn, token).ConfigureAwait(false);
                    objReturn.SkillGroupObject = objGroup;
                    if (objGroup != null)
                    {
                        objGroup.MultiplePropertiesChangedAsync += objReturn.OnSkillGroupChanged;
                    }
                }
            }
            catch
            {
                await objReturn.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                throw;
            }
            finally
            {
                await objReturn.SetIsLoadingAsync(false, token).ConfigureAwait(false);
            }

            return objReturn;
        }

        protected Skill(Character objCharacter)
        {
            CharacterObject = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = new AsyncFriendlyReaderWriterLock(); // We need a separate lock so that we can properly disconnect ourselves from the character lock while we are loading data
            _objCachedCyberwareRatingLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _objCachedTotalBaseRatingLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _objCachedLearnedRatingLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _objCachedPoolModifiersLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _objCachedRatingModifiersLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _objCachedSuggestedSpecializationsLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _lstSpecializations = new ThreadSafeObservableCollection<SkillSpecialization>(LockObject);
            _objAttribute = CharacterObject.GetAttribute(DefaultAttribute);
            _objAttribute.MultiplePropertiesChangedAsync += OnLinkedAttributeChanged;
            objCharacter.MultiplePropertiesChangedAsync += OnCharacterChanged;
            CharacterSettings objSettings = objCharacter.Settings;
            if (objSettings?.IsDisposed == false)
                objSettings.MultiplePropertiesChangedAsync += OnCharacterSettingsPropertyChanged;
            AttributeSection objSection = objCharacter.AttributeSection;
            if (objSection != null)
            {
                objSection.PropertyChangedAsync += OnAttributeSectionChanged;
                ThreadSafeObservableCollection<CharacterAttrib> lstAttributes = objSection.Attributes;
                if (lstAttributes?.IsDisposed == false)
                    lstAttributes.CollectionChangedAsync += OnAttributesCollectionChanged;
            }

            Specializations.CollectionChangedAsync += SpecializationsOnCollectionChanged;
            Specializations.BeforeClearCollectionChangedAsync += SpecializationsOnBeforeClearCollectionChanged;
        }

        private async Task OnAttributesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    {
                        string strAttribute = await GetAttributeAsync(token).ConfigureAwait(false);
                        if (e.NewItems.OfType<CharacterAttrib>().Any(x => x.Abbrev == strAttribute))
                        {
                            await RecacheAttributeAsync(token).ConfigureAwait(false);
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Remove:
                    {
                        string strAttribute = await GetAttributeAsync(token).ConfigureAwait(false);
                        if (e.OldItems.OfType<CharacterAttrib>().Any(x => x.Abbrev == strAttribute))
                        {
                            await RecacheAttributeAsync(token).ConfigureAwait(false);
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Replace:
                    {
                        string strAttribute = await GetAttributeAsync(token).ConfigureAwait(false);
                        if (e.OldItems.OfType<CharacterAttrib>().Any(x => x.Abbrev == strAttribute)
                            || e.NewItems.OfType<CharacterAttrib>().Any(x => x.Abbrev == strAttribute))
                        {
                            await RecacheAttributeAsync(token).ConfigureAwait(false);
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Reset:
                    {
                        await RecacheAttributeAsync(token).ConfigureAwait(false);
                        break;
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task OnAttributeSectionChanged(object sender, PropertyChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (e.PropertyName == nameof(AttributeSection.AttributeCategory))
            {
                IAsyncDisposable objLocker =
                    await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    await RecacheAttributeAsync(token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        //load from data
        protected Skill(Character character, XmlNode xmlNode, bool blnDoSkillGroup = true) : this(character)
        //Ugly hack, needs by then
        {
            if (xmlNode == null)
                return;
            _strName = xmlNode["name"]
                    ?.InnerText; //No need to catch errors (for now), if missing we are fsked anyway
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
            if (xmlNode.TryGetBoolFieldQuickly("requiresgroundmovement", ref blnTemp) ||
                objMyNode.Value?.TryGetBoolFieldQuickly("requiresgroundmovement", ref blnTemp) == true)
                RequiresGroundMovement = blnTemp;
            if (xmlNode.TryGetBoolFieldQuickly("requiresswimmovement", ref blnTemp) ||
                objMyNode.Value?.TryGetBoolFieldQuickly("requiresswimmovement", ref blnTemp) == true)
                RequiresSwimMovement = blnTemp;
            if (xmlNode.TryGetBoolFieldQuickly("requiresflymovement", ref blnTemp) ||
                objMyNode.Value?.TryGetBoolFieldQuickly("requiresflymovement", ref blnTemp) == true)
                RequiresFlyMovement = blnTemp;

            if (blnDoSkillGroup)
            {
                string strGroup = xmlNode["skillgroup"]?.InnerText;

                if (!string.IsNullOrEmpty(strGroup))
                {
                    SkillGroup = strGroup;
                    SkillGroupObject = Skills.SkillGroup.Get(this);
                    if (SkillGroupObject != null)
                    {
                        SkillGroupObject.MultiplePropertiesChangedAsync += OnSkillGroupChanged;
                    }
                }
            }

            _blnRecalculateCachedSuggestedSpecializations = true;
            IsLoading = false;
        }

        #endregion Factory

        /// <summary>
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public int BasePoints
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intBase;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _intBase, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public async Task<int> GetBasePointsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intBase;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public async Task SetBasePointsAsync(int value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                if (Interlocked.Exchange(ref _intBase, value) == value)
                    return;
                await OnPropertyChangedAsync(nameof(BasePoints), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public async Task ModifyBasePointsAsync(int value, CancellationToken token = default)
        {
            if (value == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Interlocked.Add(ref _intBase, value);
                await OnPropertyChangedAsync(nameof(BasePoints), token).ConfigureAwait(false);
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
                using (LockObject.EnterReadLock())
                    return _intKarma;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _intKarma, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// How many points REALLY are in _karma Better than subclasses calculating Karma - FreeKarma()
        /// </summary>
        public async Task<int> GetKarmaPointsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarma;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How many points REALLY are in _karma Better than subclasses calculating Karma - FreeKarma()
        /// </summary>
        public async Task SetKarmaPointsAsync(int value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                if (Interlocked.Exchange(ref _intKarma, value) == value)
                    return;
                await OnPropertyChangedAsync(nameof(KarmaPoints), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How many points REALLY are in _karma Better than subclasses calculating Karma - FreeKarma()
        /// </summary>
        public async Task ModifyKarmaPointsAsync(int value, CancellationToken token = default)
        {
            if (value == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Interlocked.Add(ref _intKarma, value);
                await OnPropertyChangedAsync(nameof(KarmaPoints), token).ConfigureAwait(false);
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
                using (LockObject.EnterReadLock())
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
        public async Task<bool> GetBaseUnlockedAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await CharacterObject.GetEffectiveBuildMethodUsesPriorityTablesAsync(token).ConfigureAwait(false))
                {
                    if (SkillGroupObject == null || await SkillGroupObject.GetBaseAsync(token).ConfigureAwait(false) <= 0)
                        return true;
                    CharacterSettings objSettings = await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                    return (await objSettings.GetUsePointsOnBrokenGroupsAsync(token).ConfigureAwait(false)
                               && (!await objSettings.GetStrictSkillGroupsInCreateModeAsync(token)
                                       .ConfigureAwait(false)
                                   || await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                                   || await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false)));
                }
                return false;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Is it possible to place points in Karma or is it prevented a stricter interpretation of the rules
        /// </summary>
        public bool KarmaUnlocked
        {
            get
            {
                using (LockObject.EnterReadLock())
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
        public async Task<bool> GetKarmaUnlockedAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false)).GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false)
                    && !await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                    && !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                {
                    return SkillGroupObject == null
                           || await SkillGroupObject.GetRatingAsync(token).ConfigureAwait(false) <= 0;
                }

                return true;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
                using (LockObject.EnterReadLock())
                {
                    if (SkillGroupObject?.Base > 0)
                    {
                        return (CharacterObject.Settings.StrictSkillGroupsInCreateMode && !CharacterObject.Created &&
                                !CharacterObject.IgnoreRules)
                               || !CharacterObject.Settings.UsePointsOnBrokenGroups
                            ? Math.Min(SkillGroupObject.Base + FreeBase, RatingMaximum)
                            : Math.Min(SkillGroupObject.Base + BasePoints + FreeBase, RatingMaximum);
                    }

                    return Math.Min(BasePoints + FreeBase, RatingMaximum);
                }
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (SkillGroupObject?.Base > 0
                        && ((CharacterObject.Settings.StrictSkillGroupsInCreateMode && !CharacterObject.Created &&
                             !CharacterObject.IgnoreRules)
                            || !CharacterObject.Settings.UsePointsOnBrokenGroups))
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (SkillGroupObject?.Base > 0
                        && ((CharacterObject.Settings.StrictSkillGroupsInCreateMode && !CharacterObject.Created &&
                             !CharacterObject.IgnoreRules)
                            || !CharacterObject.Settings.UsePointsOnBrokenGroups))
                        return;

                    //Calculate how far above maximum we are.
                    int intOverMax = value + Karma - RatingMaximum + RatingModifiers(Attribute);
                    using (LockObject.EnterWriteLock())
                    {
                        if (intOverMax > 0) //Too much
                        {
                            //Get the smaller value, how far above, how much we can reduce
                            int intMax = Math.Min(intOverMax, KarmaPoints);
                            intOverMax -= intMax;
                            KarmaPoints -= intMax; //reduce both by that amount
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
        public async Task<int> GetBaseAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (SkillGroupObject != null)
                {
                    int intGroupBase = await SkillGroupObject.GetBaseAsync(token).ConfigureAwait(false);
                    if (intGroupBase > 0)
                    {
                        CharacterSettings objSettings =
                            await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                        return (await objSettings.GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false)
                                && !await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                                && !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                               || !await objSettings.GetUsePointsOnBrokenGroupsAsync(token).ConfigureAwait(false)
                            ? Math.Min(intGroupBase + await GetFreeBaseAsync(token).ConfigureAwait(false),
                                await GetRatingMaximumAsync(token).ConfigureAwait(false))
                            : Math.Min(
                                intGroupBase + await GetBasePointsAsync(token).ConfigureAwait(false) +
                                await GetFreeBaseAsync(token).ConfigureAwait(false),
                                await GetRatingMaximumAsync(token).ConfigureAwait(false));
                    }
                }

                return Math.Min(await GetBasePointsAsync(token).ConfigureAwait(false) + await GetFreeBaseAsync(token).ConfigureAwait(false),
                                await GetRatingMaximumAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The amount of points this skill have from skill points and bonuses
        /// to the skill rating that would be obtained in some points of character creation
        /// </summary>
        public async Task SetBaseAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intSkillGroupBase = SkillGroupObject != null
                    ? await SkillGroupObject.GetBaseAsync(token).ConfigureAwait(false)
                    : 0;
                if (intSkillGroupBase > 0)
                {
                    CharacterSettings objSettings = await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                    if ((await objSettings.GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false) &&
                         !await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false) &&
                         !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                        || !await objSettings.GetUsePointsOnBrokenGroupsAsync(token).ConfigureAwait(false))
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
                int intSkillGroupBase = SkillGroupObject != null
                    ? await SkillGroupObject.GetBaseAsync(token).ConfigureAwait(false)
                    : 0;
                if (intSkillGroupBase > 0)
                {
                    CharacterSettings objSettings = await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                    if ((await objSettings.GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false) &&
                         !await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false) &&
                         !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                        || !await objSettings.GetUsePointsOnBrokenGroupsAsync(token).ConfigureAwait(false))
                        return;
                }

                //Calculate how far above maximum we are.
                int intOverMax = value + await GetKarmaAsync(token).ConfigureAwait(false) -
                                 await GetRatingMaximumAsync(token).ConfigureAwait(false)
                                 + await RatingModifiersAsync(await GetAttributeAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
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

                    await SetBasePointsAsync(
                        Math.Max(0, value - (await GetFreeBaseAsync(token).ConfigureAwait(false) + intSkillGroupBase)),
                        token).ConfigureAwait(false);
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
        /// Amount of skill points bought with karma and bonuses to the skills rating
        /// </summary>
        public int Karma
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (CharacterObject.Settings.StrictSkillGroupsInCreateMode && !CharacterObject.Created &&
                        !CharacterObject.IgnoreRules && SkillGroupObject?.Karma > 0)
                    {
                        return SkillGroupObject.Karma;
                    }

                    return Math.Min(KarmaPoints + FreeKarma + (SkillGroupObject?.Karma ?? 0), RatingMaximum);
                }
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    //Calculate how far above maximum we are.
                    int intOverMax = value + Base - RatingMaximum + RatingModifiers(Attribute);
                    using (LockObject.EnterWriteLock())
                    {
                        if (intOverMax > 0) //Too much
                        {
                            //Get the smaller value, how far above, how much we can reduce
                            int intMax = Math.Min(intOverMax, BasePoints);
                            intOverMax -= intMax;
                            BasePoints -= intMax; //reduce both by that amount
                        }

                        value -= Math.Max(0, intOverMax); //reduce by 0 or points over.

                        //Handle free levels, don,t go below 0
                        KarmaPoints = Math.Max(0, value - (FreeKarma + (SkillGroupObject?.Karma ?? 0)));
                    }
                }
            }
        }

        /// <summary>
        /// Amount of skill points bought with karma and bonuses to the skills rating
        /// </summary>
        public async Task<int> GetKarmaAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
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
                        return intGroupKarma;
                    }
                }

                return Math.Min(await GetKarmaPointsAsync(token).ConfigureAwait(false) + await GetFreeKarmaAsync(token).ConfigureAwait(false) + intGroupKarma,
                    await GetRatingMaximumAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Amount of skill points bought with karma and bonuses to the skills rating
        /// </summary>
        public async Task SetKarmaAsync(int value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                //Calculate how far above maximum we are.
                int intOverMax = value + await GetBaseAsync(token).ConfigureAwait(false)
                                 - await GetRatingMaximumAsync(token).ConfigureAwait(false)
                                 + await RatingModifiersAsync(await GetAttributeAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
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
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
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
                using (LockObject.EnterReadLock())
                    return Math.Max(CyberwareRating, TotalBaseRating);
            }
        }

        /// <summary>
        /// Levels in this skill. Read only. You probably want to increase
        /// Karma instead
        /// </summary>
        public async Task<int> GetRatingAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return Math.Max(await GetCyberwareRatingAsync(token).ConfigureAwait(false), await GetTotalBaseRatingAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        // ReSharper disable once InconsistentNaming
        private int _intCachedTotalBaseRating = int.MinValue;

        [CLSCompliant(false)]
        protected readonly AsyncFriendlyReaderWriterLock _objCachedTotalBaseRatingLock;

        protected virtual void ResetCachedTotalBaseRating()
        {
            using (_objCachedTotalBaseRatingLock.EnterWriteLock())
                _intCachedTotalBaseRating = int.MinValue;
        }

        protected virtual async Task ResetCachedTotalBaseRatingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker =
                await _objCachedTotalBaseRatingLock.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _intCachedTotalBaseRating = int.MinValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The rating the character has paid for, plus any improvement-based bonuses to skill rating.
        /// </summary>
        public int TotalBaseRating
        {
            get
            {
                using (_objCachedTotalBaseRatingLock.EnterReadLock())
                {
                    if (_intCachedTotalBaseRating != int.MinValue)
                        return _intCachedTotalBaseRating;
                }

                using (_objCachedTotalBaseRatingLock.EnterUpgradeableReadLock())
                {
                    if (_intCachedTotalBaseRating != int.MinValue)
                        return _intCachedTotalBaseRating;
                    using (_objCachedTotalBaseRatingLock.EnterWriteLock())
                    {
                        return _intCachedTotalBaseRating = LearnedRating + RatingModifiers(Attribute);
                    }
                }
            }
        }

        /// <summary>
        /// The rating the character has paid for, plus any improvement-based bonuses to skill rating.
        /// </summary>
        public async Task<int> GetTotalBaseRatingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCachedTotalBaseRatingLock.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedTotalBaseRating != int.MinValue)
                    return _intCachedTotalBaseRating;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await _objCachedTotalBaseRatingLock.EnterUpgradeableReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedTotalBaseRating != int.MinValue)
                    return _intCachedTotalBaseRating;

                IAsyncDisposable objLocker2 =
                    await _objCachedTotalBaseRatingLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    return _intCachedTotalBaseRating = await GetLearnedRatingAsync(token).ConfigureAwait(false)
                        + await RatingModifiersAsync(await GetAttributeAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
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

        // ReSharper disable once InconsistentNaming
        private int _intCachedLearnedRating = int.MinValue;

        [CLSCompliant(false)]
        protected readonly AsyncFriendlyReaderWriterLock _objCachedLearnedRatingLock;

        protected virtual void ResetCachedLearnedRating()
        {
            using (_objCachedLearnedRatingLock.EnterWriteLock())
                _intCachedLearnedRating = int.MinValue;
        }

        protected virtual async Task ResetCachedLearnedRatingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker =
                await _objCachedLearnedRatingLock.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _intCachedLearnedRating = int.MinValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
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
                using (_objCachedLearnedRatingLock.EnterReadLock())
                {
                    if (_intCachedLearnedRating != int.MinValue)
                        return _intCachedLearnedRating;
                }

                using (_objCachedLearnedRatingLock.EnterUpgradeableReadLock())
                {
                    if (_intCachedLearnedRating != int.MinValue)
                        return _intCachedLearnedRating;
                    using (_objCachedLearnedRatingLock.EnterWriteLock())
                    {
                        return _intCachedLearnedRating = Karma + Base;
                    }
                }
            }
        }

        /// <summary>
        /// The rating the character have actually paid for, not including skillwires
        /// or other overrides for skill Rating. Read only, you probably want to
        /// increase Karma instead.
        /// </summary>
        public async Task<int> GetLearnedRatingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCachedLearnedRatingLock.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedLearnedRating != int.MinValue)
                    return _intCachedLearnedRating;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await _objCachedLearnedRatingLock.EnterUpgradeableReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedLearnedRating != int.MinValue)
                    return _intCachedLearnedRating;

                IAsyncDisposable objLocker2 =
                    await _objCachedLearnedRatingLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    return _intCachedLearnedRating = await GetKarmaAsync(token).ConfigureAwait(false) + await GetBaseAsync(token).ConfigureAwait(false);
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
        /// Is the specialization bought with karma. During career mode this is undefined
        /// </summary>
        public virtual bool BuyWithKarma
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return (_blnBuyWithKarma || ForcedBuyWithKarma) && !ForcedNotBuyWithKarma &&
                           Specializations.Any(x => !x.Free) && CanHaveSpecs;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    value = (value || ForcedBuyWithKarma) && !ForcedNotBuyWithKarma &&
                            Specializations.Any(x => !x.Free) && CanHaveSpecs;
                    if (_blnBuyWithKarma == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnBuyWithKarma = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public virtual async Task<bool> GetBuyWithKarmaAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return (_blnBuyWithKarma || await GetForcedBuyWithKarmaAsync(token).ConfigureAwait(false))
                       && !await GetForcedNotBuyWithKarmaAsync(token).ConfigureAwait(false)
                       && await (await GetSpecializationsAsync(token).ConfigureAwait(false)).AnyAsync(async x => !await x.GetFreeAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false)
                       && await GetCanHaveSpecsAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public virtual async Task SetBuyWithKarmaAsync(bool value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                value = (value || await GetForcedBuyWithKarmaAsync(token).ConfigureAwait(false))
                        && !await GetForcedNotBuyWithKarmaAsync(token).ConfigureAwait(false)
                        && await (await GetSpecializationsAsync(token).ConfigureAwait(false))
                            .AnyAsync(async x => !await x.GetFreeAsync(token).ConfigureAwait(false), token: token)
                            .ConfigureAwait(false)
                        && await GetCanHaveSpecsAsync(token).ConfigureAwait(false);
                if (_blnBuyWithKarma == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnBuyWithKarma = value;
                    await OnPropertyChangedAsync(nameof(BuyWithKarma), token).ConfigureAwait(false);
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
        /// Maximum possible rating
        /// </summary>
        public int RatingMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int intOtherBonus =
                        RelevantImprovements(x => x.ImproveType == Improvement.ImprovementType.Skill)
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
        public async Task<int> GetRatingMaximumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intOtherBonus =
                    (await RelevantImprovementsAsync(
                            x => x.ImproveType == Improvement.ImprovementType.Skill, token: token)
                        .ConfigureAwait(false)).Sum(x => x.Maximum);
                CharacterSettings objSettings = await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                int intBaseMax = IsKnowledgeSkill
                    ? await objSettings.GetMaxKnowledgeSkillRatingAsync(token).ConfigureAwait(false)
                    : await objSettings.GetMaxSkillRatingAsync(token).ConfigureAwait(false);
                if (!await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false) &&
                    !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                {
                    intBaseMax = IsKnowledgeSkill
                        ? await objSettings.GetMaxKnowledgeSkillRatingCreateAsync(token).ConfigureAwait(false)
                        : await objSettings.GetMaxSkillRatingCreateAsync(token).ConfigureAwait(false);
                }

                return intBaseMax + intOtherBonus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Does this skill's pool have a non-trivial value.
        /// </summary>
        public bool NonTrivialPool
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return !IsNativeLanguage && Enabled && AttributeModifiers > 0;
            }
        }

        /// <summary>
        /// Does this skill's pool have a non-trivial value.
        /// </summary>
        public async Task<bool> GetNonTrivialPoolAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return !await GetIsLanguageAsync(token).ConfigureAwait(false) && await GetEnabledAsync(token).ConfigureAwait(false) && await GetAttributeModifiersAsync(token).ConfigureAwait(false) > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The total, general purpose dice pool for this skill, using another
        /// value for the attribute part of the test. This allows calculation of dice pools
        /// while using cyberlimbs or while rigging
        /// </summary>
        /// <param name="strAttribute">The English abbreviation of the used attribute.</param>
        /// <param name="blnIncludeConditionals">Whether to include improvements that don't apply under all circumstances.</param>
        /// <param name="intAttributeOverrideValue">The value to be used for the attribute if it's not the default value. int.MinValue is equivalent to not overriding.</param>
        /// <returns></returns>
        public int PoolOtherAttribute(string strAttribute, bool blnIncludeConditionals = false, int intAttributeOverrideValue = int.MinValue)
        {
            using (LockObject.EnterReadLock())
            {
                if (IsNativeLanguage)
                    return int.MaxValue;
                if (!Enabled)
                    return 0;
                int intValue = intAttributeOverrideValue > int.MinValue
                    ? intAttributeOverrideValue
                    : CharacterObject.AttributeSection.GetAttributeByName(strAttribute).TotalValue;
                if (intValue <= 0)
                    return 0;
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
        /// <param name="strAttribute">The English abbreviation of the used attribute.</param>
        /// <param name="blnIncludeConditionals">Whether to include improvements that don't apply under all circumstances.</param>
        /// <param name="intAttributeOverrideValue">The value to be used for the attribute if it's not the default value. int.MinValue is equivalent to not overriding.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public async Task<int> PoolOtherAttributeAsync(string strAttribute, bool blnIncludeConditionals = false,
            int intAttributeOverrideValue = int.MinValue, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await GetIsNativeLanguageAsync(token).ConfigureAwait(false))
                    return int.MaxValue;
                if (!await GetEnabledAsync(token).ConfigureAwait(false))
                    return 0;
                int intValue = intAttributeOverrideValue > int.MinValue
                    ? intAttributeOverrideValue
                    : await (await CharacterObject.AttributeSection.GetAttributeByNameAsync(strAttribute, token)
                        .ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false);
                if (intValue <= 0)
                    return 0;
                int intRating = await GetRatingAsync(token).ConfigureAwait(false);
                if (intRating > 0)
                    return Math.Max(0,
                        intRating + intValue +
                        await PoolModifiersAsync(strAttribute, blnIncludeConditionals, token).ConfigureAwait(false) +
                        await CharacterObject.GetWoundModifierAsync(token).ConfigureAwait(false) +
                        await CharacterObject.GetSustainingPenaltyAsync(token).ConfigureAwait(false));
                return await GetDefaultAsync(token).ConfigureAwait(false)
                    ? Math.Max(0,
                        intValue + await PoolModifiersAsync(strAttribute, blnIncludeConditionals, token)
                            .ConfigureAwait(false) + await GetDefaultModifierAsync(token).ConfigureAwait(false) +
                        await CharacterObject.GetWoundModifierAsync(token).ConfigureAwait(false) +
                        await CharacterObject.GetSustainingPenaltyAsync(token).ConfigureAwait(false))
                    : 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static Guid ReflexRecorderGUID { get; } = new Guid("17a6ba49-c21c-461b-9830-3beae8a237fc");

        public int DefaultModifier
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (ImprovementManager.GetCachedImprovementListForValueOf(CharacterObject,
                            Improvement.ImprovementType.RemoveSkillCategoryDefaultPenalty, SkillCategory, true).Count > 0)
                    {
                        return 0;
                    }

                    if (ImprovementManager.GetCachedImprovementListForValueOf(CharacterObject,
                            Improvement.ImprovementType.RemoveSkillCategoryDefaultPenalty, SkillGroup, true).Count > 0)
                    {
                        return 0;
                    }

                    if (ImprovementManager.GetCachedImprovementListForValueOf(CharacterObject,
                            Improvement.ImprovementType.RemoveSkillDefaultPenalty, DictionaryKey, true).Count > 0)
                    {
                        return 0;
                    }

                    if (ImprovementManager.GetCachedImprovementListForValueOf(CharacterObject,
                            Improvement.ImprovementType.ReflexRecorderOptimization).Count > 0)
                    {
                        List<Cyberware> lstReflexRecorders = CharacterObject.Cyberware
                            .Where(x => x.SourceID == ReflexRecorderGUID)
                            .ToList();
                        if (lstReflexRecorders.Count > 0)
                        {
                            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(
                                       Utils.StringHashSetPool, out HashSet<string> setSkillNames))
                            {
                                if (SkillGroupObject != null)
                                {
                                    setSkillNames.AddRange(SkillGroupObject.SkillList.Select(x => x.DictionaryKey));
                                    if (lstReflexRecorders.Exists(x => setSkillNames.Contains(x.Extra)))
                                    {
                                        return 0;
                                    }
                                }
                                else if (lstReflexRecorders.Exists(x => x.Extra == DictionaryKey))
                                    return 0;
                            }
                        }
                    }

                    return -1;
                }
            }
        }

        public async Task<int> GetDefaultModifierAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if ((await ImprovementManager.GetCachedImprovementListForValueOfAsync(CharacterObject,
                        Improvement.ImprovementType.RemoveSkillDefaultPenalty,
                        SkillCategory, true, token).ConfigureAwait(false)).Count > 0)
                {
                    return 0;
                }

                if ((await ImprovementManager.GetCachedImprovementListForValueOfAsync(CharacterObject,
                        Improvement.ImprovementType.RemoveSkillDefaultPenalty,
                        SkillGroup, true, token).ConfigureAwait(false)).Count > 0)
                {
                    return 0;
                }

                if ((await ImprovementManager.GetCachedImprovementListForValueOfAsync(CharacterObject,
                        Improvement.ImprovementType.RemoveSkillDefaultPenalty,
                        await GetDictionaryKeyAsync(token).ConfigureAwait(false), true, token).ConfigureAwait(false)).Count > 0)
                {
                    return 0;
                }

                if ((await ImprovementManager
                        .GetCachedImprovementListForValueOfAsync(CharacterObject,
                            Improvement.ImprovementType.ReflexRecorderOptimization, token: token).ConfigureAwait(false))
                    .Count > 0)
                {
                    List<Cyberware> lstReflexRecorders = await CharacterObject.Cyberware
                        .ToListAsync(async x => await x.GetSourceIDAsync(token).ConfigureAwait(false) == ReflexRecorderGUID, token: token).ConfigureAwait(false);
                    if (lstReflexRecorders.Count > 0)
                    {
                        using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(
                                   Utils.StringHashSetPool, out HashSet<string> setSkillNames))
                        {
                            if (SkillGroupObject != null)
                            {
                                foreach (Skill objSkill in SkillGroupObject.SkillList)
                                    setSkillNames.Add(await objSkill.GetDictionaryKeyAsync(token)
                                        .ConfigureAwait(false));
                                if (lstReflexRecorders.Exists(x => setSkillNames.Contains(x.Extra)))
                                {
                                    return 0;
                                }
                            }
                            else
                            {
                                string strKey = await GetDictionaryKeyAsync(token).ConfigureAwait(false);
                                if (lstReflexRecorders.Exists(x => x.Extra == strKey))
                                    return 0;
                            }
                        }
                    }
                }

                return -1;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        // ReSharper disable once InconsistentNaming
        private int _intCachedPoolModifiers = int.MinValue;

        [CLSCompliant(false)]
        protected readonly AsyncFriendlyReaderWriterLock _objCachedPoolModifiersLock;

        protected virtual void ResetCachedPoolModifiers()
        {
            using (_objCachedPoolModifiersLock.EnterWriteLock())
                _intCachedPoolModifiers = int.MinValue;
        }

        protected virtual async Task ResetCachedPoolModifiersAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker =
                await _objCachedPoolModifiersLock.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _intCachedPoolModifiers = int.MinValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        // ReSharper disable once InconsistentNaming
        private int _intCachedRatingModifiers = int.MinValue;

        [CLSCompliant(false)]
        protected readonly AsyncFriendlyReaderWriterLock _objCachedRatingModifiersLock;

        protected virtual void ResetCachedRatingModifiers()
        {
            using (_objCachedRatingModifiersLock.EnterWriteLock())
                _intCachedRatingModifiers = int.MinValue;
        }

        protected virtual async Task ResetCachedRatingModifiersAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker =
                await _objCachedRatingModifiersLock.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _intCachedRatingModifiers = int.MinValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Things that modify the dicepool of the skill
        /// </summary>
        public int PoolModifiers(string strUseAttribute, bool blnIncludeConditionals = false)
        {
            if (!blnIncludeConditionals)
            {
                using (LockObject.EnterReadLock())
                {
                    if (strUseAttribute == Attribute)
                    {
                        using (_objCachedPoolModifiersLock.EnterReadLock())
                        {
                            if (_intCachedPoolModifiers != int.MinValue)
                                return _intCachedPoolModifiers;
                        }

                        using (_objCachedPoolModifiersLock.EnterUpgradeableReadLock())
                        {
                            if (_intCachedPoolModifiers != int.MinValue)
                                return _intCachedPoolModifiers;
                            using (_objCachedPoolModifiersLock.EnterWriteLock())
                            {
                                return _intCachedPoolModifiers = Bonus(false, strUseAttribute, blnIncludeConditionals);
                            }
                        }
                    }
                }
            }
            return Bonus(false, strUseAttribute, blnIncludeConditionals);
        }

        /// <summary>
        /// Things that modify the dicepool of the skill
        /// </summary>
        public async Task<int> PoolModifiersAsync(string strUseAttribute, bool blnIncludeConditionals = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!blnIncludeConditionals)
            {
                IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (strUseAttribute == await GetAttributeAsync(token).ConfigureAwait(false))
                    {
                        IAsyncDisposable objLocker2 = await _objCachedPoolModifiersLock.EnterReadLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            if (_intCachedPoolModifiers != int.MinValue)
                                return _intCachedPoolModifiers;
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }

                        objLocker2 = await _objCachedPoolModifiersLock.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            if (_intCachedPoolModifiers != int.MinValue)
                                return _intCachedPoolModifiers;
                            IAsyncDisposable objLocker3 = await _objCachedPoolModifiersLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                return _intCachedPoolModifiers = await BonusAsync(false, strUseAttribute, blnIncludeConditionals, token).ConfigureAwait(false);
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
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
            return await BonusAsync(false, strUseAttribute, blnIncludeConditionals, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Things that modify the rating of the skill
        /// </summary>
        public int RatingModifiers(string strUseAttribute, bool blnIncludeConditionals = false)
        {
            if (!blnIncludeConditionals)
            {
                using (LockObject.EnterReadLock())
                {
                    if (strUseAttribute == Attribute)
                    {
                        using (_objCachedRatingModifiersLock.EnterReadLock())
                        {
                            if (_intCachedRatingModifiers != int.MinValue)
                                return _intCachedRatingModifiers;
                        }

                        using (_objCachedRatingModifiersLock.EnterUpgradeableReadLock())
                        {
                            if (_intCachedRatingModifiers != int.MinValue)
                                return _intCachedRatingModifiers;
                            using (_objCachedRatingModifiersLock.EnterWriteLock())
                            {
                                return _intCachedRatingModifiers = Bonus(true, strUseAttribute, blnIncludeConditionals);
                            }
                        }
                    }
                }
            }
            return Bonus(true, strUseAttribute, blnIncludeConditionals);
        }

        /// <summary>
        /// Things that modify the rating of the skill
        /// </summary>
        public async Task<int> RatingModifiersAsync(string strUseAttribute, bool blnIncludeConditionals = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!blnIncludeConditionals)
            {
                IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (strUseAttribute == await GetAttributeAsync(token).ConfigureAwait(false))
                    {
                        IAsyncDisposable objLocker2 = await _objCachedRatingModifiersLock.EnterReadLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            if (_intCachedRatingModifiers != int.MinValue)
                                return _intCachedRatingModifiers;
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }

                        objLocker2 = await _objCachedRatingModifiersLock.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            if (_intCachedRatingModifiers != int.MinValue)
                                return _intCachedRatingModifiers;
                            IAsyncDisposable objLocker3 = await _objCachedRatingModifiersLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                return _intCachedRatingModifiers = await BonusAsync(true, strUseAttribute, blnIncludeConditionals, token).ConfigureAwait(false);
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
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
            return await BonusAsync(true, strUseAttribute, blnIncludeConditionals, token).ConfigureAwait(false);
        }

        protected int Bonus(bool blnAddToRating, string strUseAttribute, bool blnIncludeConditionals = false)
        {
            using (LockObject.EnterReadLock())
            {
                //Some of this is not future proof. Rating that don't stack is not supported but i'm not aware of any cases where that will happen (for skills)
                if (blnIncludeConditionals)
                {
                    decimal decReturn = 0;
                    decimal decMaxConditionalValue = 0;
                    SkillSpecialization objMaxConditionalSpecialization = null;
                    foreach (Improvement objImprovement in RelevantImprovements(
                                 x => x.AddToRating == blnAddToRating, strUseAttribute, true))
                    {
                        if (string.IsNullOrEmpty(objImprovement.Condition))
                            decReturn += objImprovement.Value;
                        else
                        {
                            decimal decLoopMaxValue = decMaxConditionalValue;
                            if (objMaxConditionalSpecialization != null)
                                decLoopMaxValue += objMaxConditionalSpecialization.SpecializationBonus;
                            decimal decLoop = objImprovement.Value;
                            SkillSpecialization objLoopSpec = GetSpecialization(objImprovement.Condition);
                            if (objLoopSpec != null)
                                decLoop += objLoopSpec.SpecializationBonus;
                            if (decLoop > decLoopMaxValue)
                            {
                                decMaxConditionalValue = objImprovement.Value;
                                objMaxConditionalSpecialization = objLoopSpec;
                            }
                        }
                    }

                    return (decReturn + decMaxConditionalValue).StandardRound();
                }
                return RelevantImprovements(x => x.AddToRating == blnAddToRating, strUseAttribute).Sum(x => x.Value).StandardRound();
            }
        }

        protected async Task<int> BonusAsync(bool blnAddToRating, string strUseAttribute, bool blnIncludeConditionals = false, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                //Some of this is not future proof. Rating that don't stack is not supported but i'm not aware of any cases where that will happen (for skills)
                List<Improvement> lstImprovements = await RelevantImprovementsAsync(
                    x => x.AddToRating == blnAddToRating,
                    strUseAttribute, blnIncludeConditionals,
                    token: token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (blnIncludeConditionals)
                {
                    decimal decReturn = 0;
                    decimal decMaxConditionalValue = 0;
                    SkillSpecialization objMaxConditionalSpecialization = null;
                    foreach (Improvement objImprovement in lstImprovements)
                    {
                        token.ThrowIfCancellationRequested();
                        if (string.IsNullOrEmpty(objImprovement.Condition))
                            decReturn += objImprovement.Value;
                        else
                        {
                            decimal decLoopMaxValue = decMaxConditionalValue;
                            if (objMaxConditionalSpecialization != null)
                                decLoopMaxValue += await objMaxConditionalSpecialization.GetSpecializationBonusAsync(token).ConfigureAwait(false);
                            decimal decLoop = objImprovement.Value;
                            SkillSpecialization objLoopSpec = await GetSpecializationAsync(objImprovement.Condition, token).ConfigureAwait(false);
                            if (objLoopSpec != null)
                                decLoop += await objLoopSpec.GetSpecializationBonusAsync(token).ConfigureAwait(false);
                            if (decLoop > decLoopMaxValue)
                            {
                                decMaxConditionalValue = objImprovement.Value;
                                objMaxConditionalSpecialization = objLoopSpec;
                            }
                        }
                    }
                    token.ThrowIfCancellationRequested();
                    return (decReturn + decMaxConditionalValue).StandardRound();
                }
                return lstImprovements.Sum(x => x.Value).StandardRound();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public IEnumerable<Improvement> RelevantImprovements(Func<Improvement, bool> funcWherePredicate = null, string strUseAttribute = "", bool blnIncludeConditionals = false, bool blnExitAfterFirst = false)
        {
            using (LockObject.EnterReadLock())
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

                        case Improvement.ImprovementType.BlockSkillDefault:
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

                        case Improvement.ImprovementType.BlockSkillCategoryDefault:
                            if (objImprovement.ImprovedName == SkillCategory)
                            {
                                yield return objImprovement;
                                if (blnExitAfterFirst)
                                    yield break;
                            }

                            break;

                        case Improvement.ImprovementType.BlockSkillGroupDefault:
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
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strNameToUse = await GetDictionaryKeyAsync(token).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strUseAttribute))
                    strUseAttribute = await GetAttributeAsync(token).ConfigureAwait(false);
                ThreadSafeObservableCollection<Improvement> lstImprovements = await CharacterObject.GetImprovementsAsync(token).ConfigureAwait(false);
                List<Improvement> lstReturn = new List<Improvement>(await lstImprovements.GetCountAsync(token).ConfigureAwait(false));
                await lstImprovements.ForEachWithBreakAsync(
                    async objImprovement =>
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

                            case Improvement.ImprovementType.BlockSkillDefault:
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
                                if (objImprovement.ImprovedName == await GetAttributeAsync(token).ConfigureAwait(false) &&
                                    !objImprovement.Exclude.Contains(strNameToUse))
                                {
                                    lstReturn.Add(objImprovement);
                                    if (blnExitAfterFirst)
                                        return false;
                                }

                                break;

                            case Improvement.ImprovementType.BlockSkillCategoryDefault:
                                if (objImprovement.ImprovedName == SkillCategory)
                                {
                                    lstReturn.Add(objImprovement);
                                    if (blnExitAfterFirst)
                                        return false;
                                }

                                break;

                            case Improvement.ImprovementType.BlockSkillGroupDefault:
                                if (objImprovement.ImprovedName == SkillGroup)
                                {
                                    lstReturn.Add(objImprovement);
                                    if (blnExitAfterFirst)
                                        return false;
                                }

                                break;

                            case Improvement.ImprovementType.EnhancedArticulation:
                                if (SkillCategory == "Physical Active" &&
                                    AttributeSection.PhysicalAttributes.Contains(await GetAttributeAsync(token).ConfigureAwait(false)))
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
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How much Sp this costs. Price during career mode is undefined
        /// </summary>
        public virtual int CurrentSpCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int cost = BasePoints;
                    if (!IsExoticSkill && !BuyWithKarma && CanHaveSpecs)
                        cost += Specializations.Count(x => !x.Free);

                    decimal decExtra = 0;
                    decimal decMultiplier = 1.0m;
                    CharacterObject.Improvements.ForEach(objLoopImprovement =>
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
                                                                  : objLoopImprovement.Maximum)
                                                     - objLoopImprovement.Minimum);
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
                                                                  : objLoopImprovement.Maximum)
                                                     - objLoopImprovement.Minimum);
                                        break;

                                    case Improvement.ImprovementType.SkillCategoryPointCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                        }
                    });

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
        public virtual async Task<int> GetCurrentSpCostAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intBasePoints = await GetBasePointsAsync(token).ConfigureAwait(false);
                int cost = intBasePoints;
                if (!IsExoticSkill && !await GetBuyWithKarmaAsync(token).ConfigureAwait(false) && await GetCanHaveSpecsAsync(token).ConfigureAwait(false))
                    cost += await (await GetSpecializationsAsync(token).ConfigureAwait(false)).CountAsync(async x => !await x.GetFreeAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);

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
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How much karma this costs. Return value during career mode is undefined
        /// </summary>
        public virtual int CurrentKarmaCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    //No rating can obv not cost anything
                    //Makes debugging easier as we often only care about value calculation
                    int intTotalBaseRating = TotalBaseRating;
                    if (intTotalBaseRating == 0) return 0;

                    int intCost;
                    int intLower;
                    if (SkillGroupObject != null)
                    {
                        bool blnForceOffSkillGroupKarmaCompensation = !CharacterObject.Settings.CompensateSkillGroupKarmaDifference
                                                                      // Only count our discount if we are the first skill in the list
                                                                      || !ReferenceEquals(SkillGroupObject.SkillList.FirstOrDefault(x => x.Enabled), this);
                        if (SkillGroupObject.Karma > 0)
                        {
                            int intGroupUpper =
                                SkillGroupObject.SkillList.Min(x => x.Base + x.Karma + x.RatingModifiers(x.Attribute));
                            int intGroupLower = intGroupUpper - SkillGroupObject.Karma;

                            intLower = Base + FreeKarma + RatingModifiers(Attribute); //Might be an error here

                            intCost = RangeCost(intLower, intGroupLower, blnForceOffSkillGroupKarmaCompensation) + RangeCost(intGroupUpper, intTotalBaseRating, blnForceOffSkillGroupKarmaCompensation);
                        }
                        else
                        {
                            intLower = Base + FreeKarma + RatingModifiers(Attribute);

                            intCost = RangeCost(intLower, intTotalBaseRating, blnForceOffSkillGroupKarmaCompensation);
                        }
                    }
                    else
                    {
                        intLower = Base + FreeKarma + RatingModifiers(Attribute);

                        intCost = RangeCost(intLower, intTotalBaseRating);
                    }

                    //Don't think this is going to happen, but if it happens i want to know
                    if (intCost < 0)
                        Utils.BreakIfDebug();

                    // Exotic skills don't charge for specializations
                    if (IsExoticSkill)
                        return Math.Max(0, intCost);

                    int intSpecCount = (BuyWithKarma || !CharacterObject.EffectiveBuildMethodUsesPriorityTables) && CanHaveSpecs
                        ? Specializations.Count(objSpec => !objSpec.Free)
                        : 0;
                    int intSpecCost = intSpecCount * CharacterObject.Settings.KarmaSpecialization;
                    decimal decExtraSpecCost = 0;
                    decimal decSpecCostMultiplier = 1.0m;
                    CharacterObject.Improvements.ForEach(objLoopImprovement =>
                    {
                        if (objLoopImprovement.Minimum <= intTotalBaseRating &&
                            (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                             (objLoopImprovement.Condition == "career") == CharacterObject.Created ||
                             (objLoopImprovement.Condition == "create") != CharacterObject.Created) &&
                            objLoopImprovement.Enabled)
                        {
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
                        }
                    });

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
        public virtual async Task<int> GetCurrentKarmaCostAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                //No rating can obv not cost anything
                //Makes debugging easier as we often only care about value calculation
                int intTotalBaseRating = await GetTotalBaseRatingAsync(token).ConfigureAwait(false);
                if (intTotalBaseRating == 0)
                    return 0;

                int intCost;
                int intLower;
                if (SkillGroupObject != null)
                {
                    bool blnForceOffSkillGroupKarmaCompensation
                        = !await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false))
                                 .GetCompensateSkillGroupKarmaDifferenceAsync(token).ConfigureAwait(false)
                          // Only count our discount if we are the first skill in the list
                          || !ReferenceEquals(
                              await SkillGroupObject.SkillList
                                                    .FirstOrDefaultAsync(x => x.GetEnabledAsync(token), token)
                                                    .ConfigureAwait(false), this);
                    if (await SkillGroupObject.GetKarmaAsync(token).ConfigureAwait(false) > 0)
                    {
                        int intGroupUpper
                            = await SkillGroupObject.SkillList.MinAsync(
                                                        async x => await x.GetBaseAsync(token).ConfigureAwait(false) +
                                                                   await x.GetKarmaAsync(token).ConfigureAwait(false)
                                                                   + await x.RatingModifiersAsync(
                                                                       await x.GetAttributeAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false),
                                                        token: token)
                                                    .ConfigureAwait(false);
                        int intGroupLower =
                            intGroupUpper - await SkillGroupObject.GetKarmaAsync(token).ConfigureAwait(false);

                        intLower = await GetBaseAsync(token).ConfigureAwait(false) +
                                   await GetFreeKarmaAsync(token).ConfigureAwait(false) +
                                   await RatingModifiersAsync(await GetAttributeAsync(token).ConfigureAwait(false), token: token)
                                       .ConfigureAwait(false); //Might be an error here

                        intCost = await RangeCostAsync(intLower, intGroupLower, blnForceOffSkillGroupKarmaCompensation, token).ConfigureAwait(false) +
                                  await RangeCostAsync(intGroupUpper, intTotalBaseRating, blnForceOffSkillGroupKarmaCompensation, token).ConfigureAwait(false);
                    }
                    else
                    {
                        intLower = await GetBaseAsync(token).ConfigureAwait(false) +
                                   await GetFreeKarmaAsync(token).ConfigureAwait(false) +
                                   await RatingModifiersAsync(await GetAttributeAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);

                        intCost = await RangeCostAsync(intLower, intTotalBaseRating, blnForceOffSkillGroupKarmaCompensation, token).ConfigureAwait(false);
                    }
                }
                else
                {
                    intLower = await GetBaseAsync(token).ConfigureAwait(false) +
                               await GetFreeKarmaAsync(token).ConfigureAwait(false) +
                               await RatingModifiersAsync(await GetAttributeAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);

                    intCost = await RangeCostAsync(intLower, intTotalBaseRating, token: token).ConfigureAwait(false);
                }

                //Don't think this is going to happen, but if it happens i want to know
                if (intCost < 0)
                    Utils.BreakIfDebug();

                // Exotic skills don't charge for specializations
                if (IsExoticSkill)
                    return Math.Max(0, intCost);

                int intSpecCount = (await GetBuyWithKarmaAsync(token).ConfigureAwait(false)
                                    || !await CharacterObject.GetEffectiveBuildMethodUsesPriorityTablesAsync(token)
                                        .ConfigureAwait(false))
                                   && await GetCanHaveSpecsAsync(token).ConfigureAwait(false)
                    ? await (await GetSpecializationsAsync(token).ConfigureAwait(false))
                        .CountAsync(async objSpec => !await objSpec.GetFreeAsync(token).ConfigureAwait(false),
                            token: token).ConfigureAwait(false)
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
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The total, general purpose dice pool for this skill
        /// </summary>
        public int Pool
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return PoolOtherAttribute(Attribute);
            }
        }

        public async Task<int> GetPoolAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await PoolOtherAttributeAsync(await GetAttributeAsync(token).ConfigureAwait(false), token: token);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool Leveled
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return Rating > 0;
            }
        }

        public async Task<bool> GetLeveledAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetRatingAsync(token).ConfigureAwait(false) > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public Color PreferredControlColor
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return Leveled && Enabled ? ColorManager.Control : ColorManager.ControlLighter;
            }
        }

        public async Task<Color> GetPreferredControlColorAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetLeveledAsync(token).ConfigureAwait(false)
                       && await GetEnabledAsync(token).ConfigureAwait(false)
                    ? ColorManager.Control
                    : ColorManager.ControlLighter;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedCanHaveSpecs = -1;

        public virtual bool CanHaveSpecs
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // intReturn for thread safety
                    int intReturn = _intCachedCanHaveSpecs;
                    if (intReturn >= 0)
                        return intReturn > 0;
                    if (!Enabled)
                    {
                        _intCachedCanHaveSpecs = 0;
                        return false;
                    }

                    _intCachedCanHaveSpecs = intReturn = (!IsExoticSkill && KarmaUnlocked && TotalBaseRating > 0 &&
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
                    return intReturn > 0;
                }
            }
        }

        public virtual async Task<bool> GetCanHaveSpecsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // intReturn for thread safety
                int intReturn = _intCachedCanHaveSpecs;
                if (intReturn >= 0)
                    return intReturn > 0;
                if (!await GetEnabledAsync(token).ConfigureAwait(false))
                {
                    _intCachedCanHaveSpecs = 0;
                    return false;
                }

                _intCachedCanHaveSpecs = intReturn = (!IsExoticSkill
                             && await GetKarmaUnlockedAsync(token).ConfigureAwait(false)
                             && await GetTotalBaseRatingAsync(token).ConfigureAwait(false) > 0 &&
                             !((await ImprovementManager
                                      .GetCachedImprovementListForValueOfAsync(
                                          CharacterObject,
                                          Improvement.ImprovementType.BlockSkillSpecializations,
                                          await GetDictionaryKeyAsync(token).ConfigureAwait(false), true, token).ConfigureAwait(false)).Count > 0
                               || (await ImprovementManager
                                         .GetCachedImprovementListForValueOfAsync(
                                             CharacterObject,
                                             Improvement.ImprovementType
                                                        .BlockSkillCategorySpecializations,
                                             SkillCategory, token: token).ConfigureAwait(false)).Count
                               > 0)).ToInt32();

                return intReturn > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
                using (LockObject.EnterReadLock())
                    return AttributeObject.Abbrev;
            }
        }

        /// <summary>
        /// The Abbreviation of the linked attribute. Not the object due legacy
        /// </summary>
        public async Task<string> GetAttributeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return AttributeObject.Abbrev;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DefaultAttribute
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strDefaultAttribute;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _strDefaultAttribute, value) == value)
                        return;
                    if (IsLoading)
                        RecacheAttribute();
                    else
                        OnPropertyChanged();
                }
            }
        }

        public async Task<string> GetDefaultAttributeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strDefaultAttribute;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetDefaultAttributeAsync(string value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                if (Interlocked.Exchange(ref _strDefaultAttribute, value) == value)
                    return;
                if (IsLoading)
                    await RecacheAttributeAsync(token).ConfigureAwait(false);
                else
                    await OnPropertyChangedAsync(nameof(DefaultAttribute), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected string InterlockExchangeDefaultAttribute(string value)
        {
            return Interlocked.Exchange(ref _strDefaultAttribute, value);
        }

        /// <summary>
        /// The translated abbreviation of the linked attribute.
        /// </summary>
        public string DisplayAttributeMethod(string strLanguage)
        {
            using (LockObject.EnterReadLock())
                return LanguageManager.GetString("String_Attribute" + Attribute + "Short", strLanguage);
        }

        /// <summary>
        /// The translated abbreviation of the linked attribute.
        /// </summary>
        public async Task<string> DisplayAttributeMethodAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await LanguageManager.GetStringAsync("String_Attribute" + await GetAttributeAsync(token).ConfigureAwait(false) + "Short", strLanguage,
                    token: token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DisplayAttribute => DisplayAttributeMethod(GlobalSettings.Language);

        public Task<string> GetDisplayAttributeAsync(CancellationToken token = default) =>
            DisplayAttributeMethodAsync(GlobalSettings.Language, token);

        private int _intCachedEnabled = -1;

        private bool _blnForceDisabled;

        //TODO handle aspected/adepts who cannot (always) get magic skills
        public bool Enabled
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // intReturn for thread safety
                    int intReturn = _intCachedEnabled;
                    if (intReturn >= 0)
                        return intReturn > 0;

                    if (ForceDisabled)
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
                            intReturn = CharacterObject.MAGEnabled.ToInt32();
                            _intCachedEnabled = intReturn;
                            return intReturn > 0;

                        case "RES":
                            intReturn = CharacterObject.RESEnabled.ToInt32();
                            _intCachedEnabled = intReturn;
                            return intReturn > 0;

                        case "DEP":
                            intReturn = CharacterObject.DEPEnabled.ToInt32();
                            _intCachedEnabled = intReturn;
                            return intReturn > 0;

                        default:
                            _intCachedEnabled = 1;
                            return true;
                    }
                }
            }
        }

        //TODO handle aspected/adepts who cannot (always) get magic skills
        public async Task<bool> GetEnabledAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // intReturn for thread safety
                int intReturn = _intCachedEnabled;
                if (intReturn >= 0)
                    return intReturn > 0;

                if (await GetForceDisabledAsync(token).ConfigureAwait(false))
                {
                    _intCachedEnabled = 0;
                    return false;
                }

                if ((await ImprovementManager
                           .GetCachedImprovementListForValueOfAsync(CharacterObject,
                                                                    Improvement.ImprovementType.SkillDisable,
                                                                    await GetDictionaryKeyAsync(token)
                                                                        .ConfigureAwait(false), token: token)
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
                switch (await GetAttributeAsync(token).ConfigureAwait(false))
                {
                    case "MAG":
                    case "MAGAdept":
                        intReturn = (await CharacterObject.GetMAGEnabledAsync(token).ConfigureAwait(false))
                            .ToInt32();
                        _intCachedEnabled = intReturn;
                        return intReturn > 0;

                    case "RES":
                        intReturn = (await CharacterObject.GetRESEnabledAsync(token).ConfigureAwait(false))
                            .ToInt32();
                        _intCachedEnabled = intReturn;
                        return intReturn > 0;

                    case "DEP":
                        intReturn = (await CharacterObject.GetDEPEnabledAsync(token).ConfigureAwait(false))
                            .ToInt32();
                        _intCachedEnabled = intReturn;
                        return intReturn > 0;

                    default:
                        _intCachedEnabled = 1;
                        return true;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool ForceDisabled
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnForceDisabled;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnForceDisabled == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
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

        public async Task<bool> GetForceDisabledAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnForceDisabled;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetForceDisabledAsync(bool value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnForceDisabled == value)
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
                if (_blnForceDisabled == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnForceDisabled = value;
                    await OnPropertyChangedAsync(nameof(ForceDisabled), token).ConfigureAwait(false);
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

        public bool RequiresGroundMovement
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnRequiresGroundMovement;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnRequiresGroundMovement == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
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

        public async Task<bool> GetRequiresGroundMovementAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnRequiresGroundMovement;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool RequiresSwimMovement
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnRequiresSwimMovement;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnRequiresSwimMovement == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
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

        public async Task<bool> GetRequiresSwimMovementAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnRequiresSwimMovement;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool RequiresFlyMovement
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnRequiresFlyMovement;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnRequiresFlyMovement == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
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

        public async Task<bool> GetRequiresFlyMovementAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnRequiresFlyMovement;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedCanUpgradeCareer = -1;

        public bool CanUpgradeCareer
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // intReturn for thread safety
                    int intReturn = _intCachedCanUpgradeCareer;
                    if (intReturn < 0)
                    {
                        intReturn = (CharacterObject.Karma >= UpgradeKarmaCost &&
                                     RatingMaximum > TotalBaseRating).ToInt32();
                        _intCachedCanUpgradeCareer = intReturn;
                    }

                    return intReturn > 0;
                }
            }
        }

        public async Task<bool> GetCanUpgradeCareerAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // intReturn for thread safety
                int intReturn = _intCachedCanUpgradeCareer;
                if (intReturn < 0)
                {
                    intReturn = (await CharacterObject.GetKarmaAsync(token)
                                                      .ConfigureAwait(false)
                                 >= await GetUpgradeKarmaCostAsync(token).ConfigureAwait(false)
                                 &&
                                 await GetRatingMaximumAsync(token).ConfigureAwait(false)
                                 > await GetTotalBaseRatingAsync(token).ConfigureAwait(false))
                        .ToInt32();
                    _intCachedCanUpgradeCareer = intReturn;
                }

                return intReturn > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public virtual bool AllowDelete => false;

#pragma warning disable CS1998
        public virtual Task<bool> GetAllowDeleteAsync(CancellationToken token = default)
#pragma warning restore CS1998
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled<bool>(token)
                : Task.FromResult(false);
        }

        public bool Default
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (RelevantImprovements(
                            x => x.ImproveType == Improvement.ImprovementType.BlockSkillDefault
                                 || x.ImproveType == Improvement.ImprovementType.BlockSkillCategoryDefault
                                 || x.ImproveType == Improvement.ImprovementType.BlockSkillGroupDefault,
                            blnExitAfterFirst: true).Any())
                        return false;
                    if (!RelevantImprovements(x => x.ImproveType == Improvement.ImprovementType.AllowSkillDefault, blnExitAfterFirst: true).Any())
                    {
                        if (!_blnDefault)
                            return false;
                        // SR5 400 : Critters that don't have the Sapience Power are unable to default in skills they don't possess.
                        if (CharacterObject.IsCritter && Rating == 0)
                            return false;
                    }

                    return true;
                }
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnDefault == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
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

        public async Task<bool> GetDefaultAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                List<Improvement> lstRelevantImprovements = await RelevantImprovementsAsync(x =>
                        x.ImproveType == Improvement.ImprovementType.BlockSkillDefault
                        || x.ImproveType == Improvement.ImprovementType.BlockSkillCategoryDefault
                        || x.ImproveType == Improvement.ImprovementType.BlockSkillGroupDefault
                        || x.ImproveType == Improvement.ImprovementType.AllowSkillDefault,
                    blnExitAfterFirst: true, token: token).ConfigureAwait(false);
                if (lstRelevantImprovements.Exists(x =>
                        x.ImproveType == Improvement.ImprovementType.BlockSkillDefault
                        || x.ImproveType == Improvement.ImprovementType.BlockSkillCategoryDefault
                        || x.ImproveType == Improvement.ImprovementType.BlockSkillGroupDefault))
                    return false;
                if (lstRelevantImprovements.TrueForAll(x => x.ImproveType != Improvement.ImprovementType.AllowSkillDefault))
                {
                    if (!_blnDefault)
                        return false;
                    // SR5 400 : Critters that don't have the Sapience Power are unable to default in skills they don't possess.
                    if (await CharacterObject.GetIsCritterAsync(token).ConfigureAwait(false)
                        && await GetRatingAsync(token).ConfigureAwait(false) == 0)
                        return false;
                }

                return true;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public virtual bool IsExoticSkill => false;

        public virtual bool IsKnowledgeSkill => false;

        public virtual bool AllowNameChange => false;

#pragma warning disable CS1998
        public virtual Task<bool> GetAllowNameChangeAsync(CancellationToken token = default)
#pragma warning restore CS1998
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled<bool>(token)
                : Task.FromResult(false);
        }

        public virtual bool AllowTypeChange => false;

#pragma warning disable CS1998
        public virtual Task<bool> GetAllowTypeChangeAsync(CancellationToken token = default)
#pragma warning restore CS1998
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled<bool>(token)
                : Task.FromResult(false);
        }

        public virtual bool IsLanguage => false;

#pragma warning disable CS1998
        public virtual Task<bool> GetIsLanguageAsync(CancellationToken token = default)
#pragma warning restore CS1998
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled<bool>(token)
                : Task.FromResult(false);
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
        public virtual Task<bool> GetIsNativeLanguageAsync(CancellationToken token = default)
#pragma warning restore CS1998
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled<bool>(token)
                : Task.FromResult(false);
        }

#pragma warning disable CS1998
        public virtual Task SetIsNativeLanguageAsync(bool value, CancellationToken token = default)
#pragma warning restore CS1998
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled(token)
                : Task.CompletedTask;
        }

        protected string _strDictionaryKey;

        public virtual string DictionaryKey
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return _strDictionaryKey = _strDictionaryKey ?? Name;
                }
            }
        }

        public virtual async Task<string> GetDictionaryKeyAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strDictionaryKey = _strDictionaryKey ?? await GetNameAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

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
                    // Interlocked guarantees thread safety here without write lock
                    if (Interlocked.Exchange(ref _strName, value) == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _strDictionaryKey = null;
                        _intCachedFreeBase = int.MinValue;
                        _intCachedFreeKarma = int.MinValue;
                        OnPropertyChanged();
                    }
                }
            }
        }

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

        public async Task SetNameAsync(string value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Interlocked guarantees thread safety here without write lock
                if (Interlocked.Exchange(ref _strName, value) == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _strDictionaryKey = null;
                    _intCachedFreeBase = int.MinValue;
                    _intCachedFreeKarma = int.MinValue;
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

        //TODO RENAME DESCRIPTIVE
        private Guid _guidInternalId = Guid.NewGuid();

        /// <summary>
        /// The Unique ID for this skill. This is unique and never repeating
        /// </summary>
        public Guid Id
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guidInternalId;
            }
            private set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_guidInternalId == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
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
                using (LockObject.EnterReadLock())
                    return _guidInternalId.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        public void CopyInternalId(Skill objOtherSkill)
        {
            Id = objOtherSkill.Id;
        }

        public Guid SourceID => SkillId;

        public string SourceIDString
        {
            get
            {
                Guid guidReturn = SourceID;
                return guidReturn == Guid.Empty
                    ? string.Empty
                    : guidReturn.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
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
                using (LockObject.EnterReadLock())
                    return _guidSkillId;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_guidSkillId == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
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
        public async Task<Guid> GetSkillIdAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _guidSkillId;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The ID for this skill. This is persistent for active skills over
        /// multiple characters, ?and predefined knowledge skills,? but not
        /// for skills where the user supplies a name (Exotic and Knowledge)
        /// </summary>
        public async Task SetSkillIdAsync(Guid value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_guidSkillId == value)
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
                if (_guidSkillId == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _guidSkillId = value;
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                    _blnRecalculateCachedSuggestedSpecializations = true;
                    await OnPropertyChangedAsync(nameof(SkillId), token).ConfigureAwait(false);
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

        public string SkillGroup { get; private set; } = string.Empty;

        public virtual string SkillCategory { get; } = string.Empty;

        private bool _blnRecalculateCachedSuggestedSpecializations = true;
        private readonly AsyncFriendlyReaderWriterLock _objCachedSuggestedSpecializationsLock;
        private List<ListItem> _lstCachedSuggestedSpecializations;

        // ReSharper disable once InconsistentNaming
        public IReadOnlyList<ListItem> CGLSpecializations
        {
            get
            {
                using (_objCachedSuggestedSpecializationsLock.EnterReadLock())
                {
                    if (!_blnRecalculateCachedSuggestedSpecializations)
                        return _lstCachedSuggestedSpecializations;
                }

                using (_objCachedSuggestedSpecializationsLock.EnterUpgradeableReadLock())
                {
                    if (!_blnRecalculateCachedSuggestedSpecializations)
                        return _lstCachedSuggestedSpecializations;

                    using (_objCachedSuggestedSpecializationsLock.EnterWriteLock())
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
                                        xmlSpecNode.SelectSingleNodeAndCacheExpression("@translate")?.Value ??
                                        strInnerText));
                            }
                        }

                        foreach (string strSpecializationName in ImprovementManager
                                     .GetCachedImprovementListForValueOf(
                                         CharacterObject, Improvement.ImprovementType.SkillSpecializationOption,
                                         DictionaryKey).Select(x => x.UniqueName))
                        {
                            if (_lstCachedSuggestedSpecializations.Exists(
                                    y => y.Value?.ToString() == strSpecializationName))
                                continue;
                            _lstCachedSuggestedSpecializations.Add(
                                new ListItem(strSpecializationName,
                                    CharacterObject.TranslateExtra(strSpecializationName)));
                        }

                        _lstCachedSuggestedSpecializations.Sort(CompareListItems.CompareNames);
                    }

                    return _lstCachedSuggestedSpecializations;
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        public async Task<IReadOnlyList<ListItem>> GetCGLSpecializationsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCachedSuggestedSpecializationsLock.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_blnRecalculateCachedSuggestedSpecializations)
                    return _lstCachedSuggestedSpecializations;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await _objCachedSuggestedSpecializationsLock
                .EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_blnRecalculateCachedSuggestedSpecializations)
                    return _lstCachedSuggestedSpecializations;

                IAsyncDisposable objLocker2 = await _objCachedSuggestedSpecializationsLock
                    .EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnRecalculateCachedSuggestedSpecializations = false;
                    if (_lstCachedSuggestedSpecializations == null)
                        _lstCachedSuggestedSpecializations = Utils.ListItemListPool.Get();
                    else
                        _lstCachedSuggestedSpecializations.Clear();
                    XPathNavigator objBaseNode = await this
                        .GetNodeXPathAsync(GlobalSettings.Language, token: token)
                        .ConfigureAwait(false);
                    XPathNodeIterator xmlSpecList =
                        objBaseNode?.SelectAndCacheExpression("specs/spec", token);
                    if (xmlSpecList?.Count > 0)
                    {
                        foreach (XPathNavigator xmlSpecNode in xmlSpecList)
                        {
                            string strInnerText = xmlSpecNode.Value;
                            if (string.IsNullOrEmpty(strInnerText))
                                continue;
                            _lstCachedSuggestedSpecializations.Add(
                                new ListItem(strInnerText,
                                    xmlSpecNode
                                        .SelectSingleNodeAndCacheExpression("@translate", token)?.Value
                                    ?? strInnerText));
                        }
                    }

                    foreach (string strSpecializationName in (await ImprovementManager
                                 .GetCachedImprovementListForValueOfAsync(
                                     CharacterObject,
                                     Improvement.ImprovementType
                                         .SkillSpecializationOption,
                                     await GetDictionaryKeyAsync(token).ConfigureAwait(false), token: token)
                                 .ConfigureAwait(false)).Select(x => x.UniqueName))
                    {
                        if (_lstCachedSuggestedSpecializations.Exists(
                                y => y.Value?.ToString() == strSpecializationName))
                            continue;
                        _lstCachedSuggestedSpecializations.Add(
                            new ListItem(strSpecializationName,
                                await CharacterObject
                                    .TranslateExtraAsync(strSpecializationName, token: token)
                                    .ConfigureAwait(false)));
                    }

                    _lstCachedSuggestedSpecializations.Sort(CompareListItems.CompareNames);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                return _lstCachedSuggestedSpecializations;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private readonly Dictionary<string, string> _dicCachedStringSpec = new Dictionary<string, string>();

        public virtual string DisplaySpecialization(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                if (_dicCachedStringSpec.TryGetValue(strLanguage, out string strReturn))
                    return strReturn;
                strReturn = string.Join(", ", Specializations.Select(x => x.DisplayName(strLanguage)));

                _dicCachedStringSpec.Add(strLanguage, strReturn);

                return strReturn;
            }
        }

        public virtual async Task<string> DisplaySpecializationAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicCachedStringSpec.TryGetValue(strLanguage, out string strReturn))
                    return strReturn;
                strReturn = await StringExtensions
                    .JoinAsync(", ", (await GetSpecializationsAsync(token).ConfigureAwait(false)).Select(x => x.DisplayNameAsync(strLanguage, token)), token)
                    .ConfigureAwait(false);

                _dicCachedStringSpec.Add(strLanguage, strReturn);

                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentDisplaySpecialization => DisplaySpecialization(GlobalSettings.Language);

        public Task<string> GetCurrentDisplaySpecializationAsync(CancellationToken token = default) => DisplaySpecializationAsync(GlobalSettings.Language, token);

        private readonly ThreadSafeObservableCollection<SkillSpecialization> _lstSpecializations;

        //TODO A unit test here?, I know we don't have them, but this would be improved by some
        //Or just ignore support for multiple specializations even if the rules say it is possible?
        public ThreadSafeObservableCollection<SkillSpecialization> Specializations
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstSpecializations;
            }
        }

        public async Task<ThreadSafeObservableCollection<SkillSpecialization>> GetSpecializationsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstSpecializations;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string TopMostDisplaySpecialization
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (IsExoticSkill)
                    {
                        return ((ExoticSkill)this).CurrentDisplaySpecific;
                    }

                    if (!CanHaveSpecs)
                        return string.Empty;

                    return Specializations.FirstOrDefault(x => !x.Free)?.CurrentDisplayName ?? string.Empty;
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Specializations.RemoveAll(x => !x.Free);
                    return;
                }
                using (LockObject.EnterWriteLock())
                using (Specializations.LockObject.EnterWriteLock())
                {
                    int intIndexToReplace = Specializations.FindIndex(x => !x.Free);
                    SkillSpecialization objNewSpec = new SkillSpecialization(CharacterObject, value);
                    try
                    {
                        if (intIndexToReplace < 0)
                        {
                            Specializations.AddWithSort(objNewSpec, (x, y) =>
                            {
                                bool blnLhsFree = x.Free;
                                if (blnLhsFree != y.Free)
                                    return blnLhsFree ? 1 : -1;
                                bool blnLhsExpertise = x.Expertise;
                                if (blnLhsExpertise != y.Expertise)
                                    return blnLhsExpertise ? 1 : -1;
                                return 0;
                            });
                            return;
                        }

                        Specializations[intIndexToReplace] = objNewSpec;
                    }
                    catch
                    {
                        objNewSpec.Dispose();
                        throw;
                    }
                    // For safety's, remove all non-free specializations after the one we are replacing.
                    intIndexToReplace = Specializations.FindIndex(intIndexToReplace + 1, x => !x.Free);
                    if (intIndexToReplace > 0)
                        Utils.BreakIfDebug(); // This shouldn't happen under normal operations because chargen can only ever have one player-picked specialization at a time
                    while (intIndexToReplace > 0)
                    {
                        SkillSpecialization objToRemove = Specializations[intIndexToReplace];
                        Specializations.RemoveAt(intIndexToReplace);
                        objToRemove.Dispose();
                        intIndexToReplace = Specializations.FindIndex(intIndexToReplace + 1, x => !x.Free);
                    }
                }
            }
        }

        public async Task<string> GetTopMostDisplaySpecializationAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (IsExoticSkill)
                {
                    return await ((ExoticSkill) this).GetCurrentDisplaySpecificAsync(token).ConfigureAwait(false);
                }

                if (!await GetCanHaveSpecsAsync(token).ConfigureAwait(false))
                    return string.Empty;

                SkillSpecialization objSpec = await (await GetSpecializationsAsync(token).ConfigureAwait(false)).FirstOrDefaultAsync(async x => !await x.GetFreeAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                return objSpec != null ? await objSpec.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) : string.Empty;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetTopMostDisplaySpecializationAsync(string value, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                await (await GetSpecializationsAsync(token).ConfigureAwait(false)).RemoveAllAsync(async x => !await x.GetFreeAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                return;
            }
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                ThreadSafeObservableCollection<SkillSpecialization> lstSpecs
                    = await GetSpecializationsAsync(token).ConfigureAwait(false);
                IAsyncDisposable objLocker2 = await lstSpecs.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    int intIndexToReplace = await lstSpecs.FindIndexAsync(async x => !await x.GetFreeAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                    SkillSpecialization objNewSpec = new SkillSpecialization(CharacterObject, value);
                    try
                    {
                        if (intIndexToReplace < 0)
                        {
                            await lstSpecs.AddWithSortAsync(objNewSpec,
                                                                   async (x, y) =>
                                                                   {
                                                                       bool blnLhsFree = await x.GetFreeAsync(token).ConfigureAwait(false);
                                                                       if (blnLhsFree != await y.GetFreeAsync(token).ConfigureAwait(false))
                                                                           return blnLhsFree ? 1 : -1;
                                                                       bool blnLhsExpertise =
                                                                           await x.GetExpertiseAsync(token).ConfigureAwait(false);
                                                                       if (blnLhsExpertise != await y.GetExpertiseAsync(token).ConfigureAwait(false))
                                                                           return blnLhsExpertise ? 1 : -1;
                                                                       return 0;
                                                                   }, token: token).ConfigureAwait(false);
                            return;
                        }

                        await lstSpecs.SetValueAtAsync(intIndexToReplace, objNewSpec, token).ConfigureAwait(false);
                    }
                    catch
                    {
                        await objNewSpec.DisposeAsync().ConfigureAwait(false);
                        throw;
                    }
                    // For safety's, remove all non-free specializations after the one we are replacing.
                    intIndexToReplace
                        = await lstSpecs.FindIndexAsync(intIndexToReplace + 1, async x => !await x.GetFreeAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                    if (intIndexToReplace > 0)
                        Utils.BreakIfDebug(); // This shouldn't happen under normal operations because chargen can only ever have one player-picked specialization at a time
                    while (intIndexToReplace > 0)
                    {
                        SkillSpecialization objToRemove = await lstSpecs.GetValueAtAsync(intIndexToReplace, token).ConfigureAwait(false);
                        await lstSpecs.RemoveAtAsync(intIndexToReplace, token).ConfigureAwait(false);
                        await objToRemove.DisposeAsync().ConfigureAwait(false);
                        intIndexToReplace
                            = await lstSpecs.FindIndexAsync(intIndexToReplace + 1, async x => !await x.GetFreeAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
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

        public bool HasSpecialization(string strSpecialization, CancellationToken token = default)
        {
            using (LockObject.EnterReadLock(token))
            {
                if (IsExoticSkill)
                {
                    return ((ExoticSkill)this).Specific == strSpecialization;
                }

                if (!CanHaveSpecs)
                    return false;

                return Specializations.Any(
                           x => x.Name == strSpecialization || x.CurrentDisplayName == strSpecialization, token)
                       && ImprovementManager.GetCachedImprovementListForValueOf(CharacterObject,
                           Improvement.ImprovementType.DisableSpecializationEffects, DictionaryKey, token: token).Count == 0;
            }
        }

        public async Task<bool> HasSpecializationAsync(string strSpecialization, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (IsExoticSkill)
                {
                    return ((ExoticSkill)this).Specific == strSpecialization;
                }

                if (!await GetCanHaveSpecsAsync(token).ConfigureAwait(false))
                    return false;

                return await (await GetSpecializationsAsync(token).ConfigureAwait(false)).AnyAsync(
                               async x => await x.GetNameAsync(token).ConfigureAwait(false) == strSpecialization ||
                                          await x.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) ==
                                          strSpecialization,
                               token: token)
                           .ConfigureAwait(false)
                       && (await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                               CharacterObject, Improvement.ImprovementType.DisableSpecializationEffects,
                               await GetDictionaryKeyAsync(token).ConfigureAwait(false), token: token)
                           .ConfigureAwait(false)).Count == 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public SkillSpecialization GetSpecialization(string strSpecialization)
        {
            using (LockObject.EnterReadLock())
            {
                if (IsExoticSkill && ((ExoticSkill)this).Specific == strSpecialization)
                {
                    return Specializations[0];
                }

                if (!CanHaveSpecs)
                    return null;

                return HasSpecialization(strSpecialization)
                    ? Specializations.FirstOrDefault(x =>
                        x.Name == strSpecialization || x.CurrentDisplayName == strSpecialization)
                    : null;
            }
        }

        public async Task<SkillSpecialization> GetSpecializationAsync(string strSpecialization, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (IsExoticSkill && ((ExoticSkill)this).Specific == strSpecialization)
                {
                    return await (await GetSpecializationsAsync(token).ConfigureAwait(false)).GetValueAtAsync(0, token).ConfigureAwait(false);
                }

                if (!await GetCanHaveSpecsAsync(token).ConfigureAwait(false))
                    return null;

                return await HasSpecializationAsync(strSpecialization, token).ConfigureAwait(false)
                    ? await (await GetSpecializationsAsync(token).ConfigureAwait(false))
                        .FirstOrDefaultAsync(
                            async x => await x.GetNameAsync(token).ConfigureAwait(false) == strSpecialization ||
                                       await x.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) ==
                                       strSpecialization, token: token)
                        .ConfigureAwait(false)
                    : null;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string PoolToolTip => CompileDicepoolTooltip();

        public Task<string> GetPoolToolTipAsync(CancellationToken token = default) =>
            CompileDicepoolTooltipAsync(token: token);

        public string CompileDicepoolTooltip(string abbrev = "", string strExtraStart = "", string strExtra = "", bool blnListAllLimbs = true, Cyberware objShowOnlyCyberware = null)
        {
            using (LockObject.EnterReadLock())
            {
                if (IsNativeLanguage)
                {
                    return strExtraStart + LanguageManager.GetString("Tip_Skill_NativeLanguage");
                }

                if (!Enabled)
                {
                    return strExtraStart + LanguageManager.GetString("Label_SkillGroup_Disabled");
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

                if (!Default && !Leveled)
                {
                    return strExtraStart + LanguageManager.GetString("Tip_Skill_Cannot_Default");
                }

                string strSpace = LanguageManager.GetString("String_Space");
                List<Improvement> lstRelevantImprovements = RelevantImprovements(null, abbrev, true).ToList();
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                           out StringBuilder sbdReturn))
                {
                    int intCyberwareRating = CyberwareRating;
                    if (intCyberwareRating > TotalBaseRating)
                    {
                        sbdReturn.Append(strExtraStart).Append(LanguageManager.GetString("Tip_Skill_SkillsoftRating"))
                            .Append(strSpace).Append('(').Append(intCyberwareRating.ToString(GlobalSettings.CultureInfo))
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
                        sbdReturn.Append(strSpace).Append('+').Append(strSpace).Append(att.CurrentDisplayAbbrev)
                            .Append(strSpace)
                            .Append('(')
                            .Append(att.TotalValue.ToString(GlobalSettings.CultureInfo)).Append(')');
                    else
                    {
                        sbdReturn.Append(strSpace).Append('+').Append(strSpace)
                            .Append(objShowOnlyCyberware.CurrentDisplayName)
                            .Append(strSpace).Append(att.CurrentDisplayAbbrev).Append(strSpace).Append('(')
                            .Append(objShowOnlyCyberware.GetAttributeTotalValue(att.Abbrev)
                                .ToString(GlobalSettings.CultureInfo)).Append(')');
                        if (!CharacterObject.Ambidextrous
                            && (objShowOnlyCyberware.LimbSlot == "arm"
                                || objShowOnlyCyberware.Name.ContainsAny(" Arm", " Hand"))
                            && objShowOnlyCyberware.Location != CharacterObject.PrimaryArm
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
                            Improvement objImprovement
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                          CharacterObject, Improvement.ImprovementType.RemoveSkillDefaultPenalty,
                                          SkillCategory, true)
                                      .FirstOrDefault()
                                  ?? ImprovementManager.GetCachedImprovementListForValueOf(
                                          CharacterObject, Improvement.ImprovementType.RemoveSkillDefaultPenalty,
                                          SkillGroup, true)
                                      .FirstOrDefault()
                                  ?? ImprovementManager.GetCachedImprovementListForValueOf(
                                          CharacterObject, Improvement.ImprovementType.RemoveSkillDefaultPenalty,
                                          DictionaryKey, true)
                                      .FirstOrDefault()
                                  ?? ImprovementManager.GetCachedImprovementListForValueOf(
                                          CharacterObject, Improvement.ImprovementType.ReflexRecorderOptimization)
                                      .FirstOrDefault();
                            sbdReturn.Append(strSpace).Append(CharacterObject.GetObjectName(objImprovement));
                        }
                        else
                            sbdReturn.Append(strSpace).Append(intDefaultModifier > 0 ? '+' : '-').Append(strSpace)
                                .Append(LanguageManager.GetString("Tip_Skill_Defaulting")).Append(strSpace).Append('(')
                                .Append(Math.Abs(intDefaultModifier).ToString(GlobalSettings.CultureInfo)).Append(')');
                    }

                    List<Improvement> lstConditionalImprovements = new List<Improvement>(lstRelevantImprovements.Count);
                    foreach (Improvement source in lstRelevantImprovements)
                    {
                        if (source.AddToRating
                            || source.ImproveType == Improvement.ImprovementType.SwapSkillAttribute
                            || source.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute)
                            continue;
                        if (!string.IsNullOrEmpty(source.Condition))
                        {
                            lstConditionalImprovements.Add(source);
                            continue;
                        }
                        sbdReturn.Append(strSpace).Append('+').Append(strSpace)
                                 .Append(CharacterObject.GetObjectName(source)).Append(strSpace).Append('(')
                                 .Append(source.Value.ToString(GlobalSettings.CultureInfo)).Append(')');
                    }

                    if (lstConditionalImprovements.Count > 0)
                    {
                        sbdReturn.Append(strSpace).Append('+').Append(strSpace).Append('(').AppendJoin(
                            strSpace + LanguageManager.GetString("String_Or") + strSpace,
                            lstConditionalImprovements.Select(
                                x => CharacterObject.GetObjectName(x) + strSpace + '('
                                     + x.Value.ToString(GlobalSettings.CultureInfo) + ',' + strSpace
                                     + x.Condition + ')')).Append(')');
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
                        CharacterObject.Cyberware.ForEach(x => BuildTooltipString(sbdReturn, x));

                        void BuildTooltipString(StringBuilder sb, Cyberware objCyberware)
                        {
                            if (!objCyberware.IsLimb || !objCyberware.IsModularCurrentlyEquipped)
                            {
                                return;
                            }

                            if (objCyberware.InheritAttributes)
                            {
                                objCyberware.Children.ForEach(x => BuildTooltipString(sbdReturn, x));

                                return;
                            }

                            sb.AppendLine().AppendLine().Append(strExtraStart)
                                .Append(objCyberware.CurrentDisplayName);
                            Grade objGrade = objCyberware.Grade;
                            if (objGrade.Name != "Standard" && objGrade.Name != "None")
                            {
                                sb.Append(strSpace).Append('(').Append(objGrade.CurrentDisplayName)
                                    .Append(')');
                            }

                            int pool = PoolOtherAttribute(att.Abbrev, false,
                                objCyberware.GetAttributeTotalValue(att.Abbrev));
                            if (CharacterObject.Ambidextrous
                                || (objCyberware.LimbSlot != "arm"
                                    && !objCyberware.Name.ContainsAny(" Arm", " Hand"))
                                || objCyberware.Location == CharacterObject.PrimaryArm
                                || objCyberware.LimbSlotCount > 1)
                            {
                                sb.Append(strSpace).Append(pool.ToString(GlobalSettings.CultureInfo));
                            }
                            else
                            {
                                sb.AppendFormat(GlobalSettings.CultureInfo, "{1}{0}{1}({2}{1}{3})", pool - 2,
                                    strSpace, -2,
                                    LanguageManager.GetString("Tip_Skill_OffHand"));
                            }

                            if (!string.IsNullOrEmpty(strExtra))
                                sb.Append(strExtra);
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
                        if (Specializations.Count > 0 && CanHaveSpecs && ImprovementManager
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
                        CharacterObject.Cyberware.ForEach(objChild => BuildTooltip(sbdReturn, objChild));

                        void BuildTooltip(StringBuilder sbdLoop, Cyberware objCyberware)
                        {
                            if (!objCyberware.IsLimb || !objCyberware.IsModularCurrentlyEquipped)
                            {
                                return;
                            }

                            if (objCyberware.InheritAttributes)
                            {
                                objCyberware.Children.ForEach(objChild => BuildTooltip(sbdLoop, objChild));
                                return;
                            }

                            sbdLoop.AppendLine().AppendLine().Append(strExtraStart).Append(strExclude)
                                .Append(LanguageManager.GetString("String_Colon")).Append(strSpace)
                                .Append(CharacterObject.GetObjectName(objSwapSkillAttribute)).Append(strSpace)
                                .Append(objCyberware.CurrentDisplayName);
                            Grade objGrade = objCyberware.Grade;
                            if (objGrade.Name != "Standard" && objGrade.Name != "None")
                            {
                                sbdLoop.Append(strSpace).Append('(').Append(objGrade.CurrentDisplayName)
                                    .Append(')');
                            }

                            int intLoopPool =
                                PoolOtherAttribute(objSwapSkillAttribute.ImprovedName, false,
                                    objCyberware.GetAttributeTotalValue(
                                        objSwapSkillAttribute.ImprovedName));
                            if (objSpecialization != null)
                            {
                                intLoopPool += objSpecialization.SpecializationBonus;
                            }

                            if (CharacterObject.Ambidextrous
                                || (objCyberware.LimbSlot != "arm"
                                    && !objCyberware.Name.ContainsAny(" Arm", " Hand"))
                                || objCyberware.Location == CharacterObject.PrimaryArm
                                || objCyberware.LimbSlotCount > 1)
                            {
                                sbdLoop.Append(strSpace).Append(intLoopPool.ToString(GlobalSettings.CultureInfo));
                            }
                            else
                            {
                                sbdLoop.AppendFormat(GlobalSettings.CultureInfo, "{1}{0}{1}({2}{1}{3})",
                                    intLoopPool - 2, strSpace, -2,
                                    LanguageManager.GetString("Tip_Skill_OffHand"));
                            }

                            if (!string.IsNullOrEmpty(strExtra))
                                sbdLoop.Append(strExtra);
                        }
                    }

                    return sbdReturn.ToString();
                }
            }
        }

        public async Task<string> CompileDicepoolTooltipAsync(string abbrev = "", string strExtraStart = "",
            string strExtra = "", bool blnListAllLimbs = true, Cyberware objShowOnlyCyberware = null,
            CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await GetIsNativeLanguageAsync(token).ConfigureAwait(false))
                {
                    return strExtraStart + await LanguageManager
                        .GetStringAsync("Tip_Skill_NativeLanguage", token: token).ConfigureAwait(false);
                }

                if (!await GetEnabledAsync(token).ConfigureAwait(false))
                {
                    return strExtraStart + await LanguageManager
                        .GetStringAsync("Label_SkillGroup_Disabled", token: token).ConfigureAwait(false);
                }

                bool blnShowSwapSkillAttribute = false;
                if (string.IsNullOrEmpty(abbrev))
                {
                    abbrev = await GetAttributeAsync(token).ConfigureAwait(false);
                    blnShowSwapSkillAttribute = abbrev == await GetDefaultAttributeAsync(token).ConfigureAwait(false);
                }

                CharacterAttrib att = await (await CharacterObject.GetAttributeSectionAsync(token).ConfigureAwait(false)).GetAttributeByNameAsync(abbrev, token)
                    .ConfigureAwait(false);
                int intAttTotalValue = await att.GetTotalValueAsync(token).ConfigureAwait(false);
                if (intAttTotalValue <= 0)
                {
                    return strExtraStart + string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("Tip_Skill_Zero_Attribute", token: token)
                            .ConfigureAwait(false),
                        await att.DisplayNameShortAsync(GlobalSettings.Language, token).ConfigureAwait(false));
                }

                if (!await GetDefaultAsync(token).ConfigureAwait(false)
                    && !await GetLeveledAsync(token).ConfigureAwait(false))
                {
                    return strExtraStart + await LanguageManager
                        .GetStringAsync("Tip_Skill_Cannot_Default", token: token).ConfigureAwait(false);
                }

                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                    .ConfigureAwait(false);
                List<Improvement> lstRelevantImprovements =
                    await RelevantImprovementsAsync(null, abbrev, true, token: token).ConfigureAwait(false);
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
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

                    bool blnAmbidextrous = await CharacterObject.GetAmbidextrousAsync(token).ConfigureAwait(false);

                    if (blnListAllLimbs || !Cyberware.CyberlimbAttributeAbbrevs.Contains(att.Abbrev) ||
                        objShowOnlyCyberware == null)
                        sbdReturn.Append(strSpace).Append('+').Append(strSpace)
                            .Append(await att.GetCurrentDisplayAbbrevAsync(token)
                                .ConfigureAwait(false)).Append(strSpace)
                            .Append('(')
                            .Append(intAttTotalValue.ToString(GlobalSettings.CultureInfo)).Append(')');
                    else
                    {
                        sbdReturn.Append(strSpace).Append('+').Append(strSpace)
                            .Append(await objShowOnlyCyberware.GetCurrentDisplayNameAsync(token).ConfigureAwait(false))
                            .Append(strSpace)
                            .Append(await att.GetCurrentDisplayAbbrevAsync(token)
                                .ConfigureAwait(false)).Append(strSpace).Append('(')
                            .Append((await objShowOnlyCyberware.GetAttributeTotalValueAsync(att.Abbrev, token)
                                    .ConfigureAwait(false))
                                .ToString(GlobalSettings.CultureInfo)).Append(')');
                        if (!blnAmbidextrous
                            && (await objShowOnlyCyberware.GetLimbSlotAsync(token).ConfigureAwait(false) == "arm"
                                || objShowOnlyCyberware.Name.ContainsAny(" Arm", " Hand"))
                            && objShowOnlyCyberware.Location !=
                            await CharacterObject.GetPrimaryArmAsync(token).ConfigureAwait(false)
                            && await objShowOnlyCyberware.GetLimbSlotCountAsync(token).ConfigureAwait(false) <= 1)
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
                            Improvement objImprovement
                                = (await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                      CharacterObject, Improvement.ImprovementType.RemoveSkillDefaultPenalty,
                                      SkillCategory, true, token).ConfigureAwait(false))
                                  .FirstOrDefault()
                                  ?? (await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                      CharacterObject, Improvement.ImprovementType.RemoveSkillDefaultPenalty,
                                      SkillGroup, true, token).ConfigureAwait(false))
                                  .FirstOrDefault()
                                  ?? (await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                      CharacterObject, Improvement.ImprovementType.RemoveSkillDefaultPenalty,
                                      await GetDictionaryKeyAsync(token).ConfigureAwait(false), true, token).ConfigureAwait(false))
                                  .FirstOrDefault()
                                  ?? (await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                      CharacterObject, Improvement.ImprovementType.ReflexRecorderOptimization, token: token).ConfigureAwait(false))
                                  .FirstOrDefault();
                            sbdReturn.Append(strSpace).Append(await CharacterObject
                                .GetObjectNameAsync(objImprovement, token: token).ConfigureAwait(false));
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

                    int wound = await CharacterObject.GetWoundModifierAsync(token).ConfigureAwait(false);
                    if (wound != 0)
                    {
                        sbdReturn.Append(strSpace).Append('-').Append(strSpace)
                            .Append(await LanguageManager.GetStringAsync("Tip_Skill_Wounds", token: token)
                                .ConfigureAwait(false))
                            .Append(strSpace).Append('(').Append(wound.ToString(GlobalSettings.CultureInfo))
                            .Append(')');
                    }

                    int sustains = await CharacterObject.GetSustainingPenaltyAsync(token).ConfigureAwait(false);
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
                        await (await CharacterObject.GetCyberwareAsync(token).ConfigureAwait(false))
                            .ForEachAsync(objCyberware => BuildTooltipAsync(sbdReturn, objCyberware), token)
                            .ConfigureAwait(false);

                        async Task BuildTooltipAsync(StringBuilder sb, Cyberware objCyberware)
                        {
                            if (!await objCyberware.GetIsLimbAsync(token).ConfigureAwait(false) ||
                                !await objCyberware.GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false))
                            {
                                return;
                            }

                            if (await objCyberware.GetInheritAttributesAsync(token).ConfigureAwait(false))
                            {
                                await objCyberware.Children
                                    .ForEachAsync(objChild => BuildTooltipAsync(sbdReturn, objChild), token)
                                    .ConfigureAwait(false);
                                return;
                            }

                            sb.AppendLine().AppendLine().Append(strExtraStart)
                                .Append(await objCyberware.GetCurrentDisplayNameAsync(token).ConfigureAwait(false));
                            Grade objGrade = await objCyberware.GetGradeAsync(token).ConfigureAwait(false);
                            string strGradeName = objGrade.Name;
                            if (strGradeName != "Standard" && strGradeName != "None")
                            {
                                sb.Append(strSpace).Append('(')
                                    .Append(await objGrade.GetCurrentDisplayNameAsync(token).ConfigureAwait(false))
                                    .Append(')');
                            }

                            int intPool = await PoolOtherAttributeAsync(att.Abbrev, false,
                                await objCyberware.GetAttributeTotalValueAsync(att.Abbrev, token).ConfigureAwait(false),
                                token).ConfigureAwait(false);
                            if (blnAmbidextrous || (await objCyberware.GetLimbSlotAsync(token).ConfigureAwait(false) !=
                                                    "arm" && !objCyberware.Name.ContainsAny(" Arm", " Hand"))
                                                || await objCyberware.GetLocationAsync(token).ConfigureAwait(false) ==
                                                await CharacterObject.GetPrimaryArmAsync(token).ConfigureAwait(false)
                                                || await objCyberware.GetLimbSlotCountAsync(token)
                                                    .ConfigureAwait(false) > 1)
                            {
                                sb.Append(strSpace).Append(intPool.ToString(GlobalSettings.CultureInfo));
                            }
                            else
                            {
                                sb.AppendFormat(GlobalSettings.CultureInfo, "{1}{0}{1}({2}{1}{3})", intPool - 2,
                                    strSpace, -2,
                                    await LanguageManager.GetStringAsync("Tip_Skill_OffHand", token: token)
                                        .ConfigureAwait(false));
                            }

                            if (!string.IsNullOrEmpty(strExtra))
                                sb.Append(strExtra);
                        }
                    }

                    if (att.Abbrev != await GetAttributeAsync(token).ConfigureAwait(false))
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
                        ThreadSafeObservableCollection<SkillSpecialization> lstSpecs
                            = await GetSpecializationsAsync(token).ConfigureAwait(false);
                        if (await lstSpecs.GetCountAsync(token).ConfigureAwait(false) > 0 &&
                            await GetCanHaveSpecsAsync(token).ConfigureAwait(false) &&
                            (await ImprovementManager
                                .GetCachedImprovementListForValueOfAsync(
                                    CharacterObject,
                                    Improvement.ImprovementType.DisableSpecializationEffects,
                                    await GetDictionaryKeyAsync(token).ConfigureAwait(false), token: token)
                                .ConfigureAwait(false)).Count == 0)
                        {
                            int intMaxBonus = 0;
                            await lstSpecs.ForEachAsync(async objLoopSpecialization =>
                            {
                                if (await objLoopSpecialization.GetNameAsync(token).ConfigureAwait(false) == strExclude)
                                {
                                    int intLoopBonus = await objLoopSpecialization.GetSpecializationBonusAsync(token)
                                        .ConfigureAwait(false);
                                    if (intLoopBonus > intMaxBonus)
                                    {
                                        objSpecialization = objLoopSpecialization;
                                        intMaxBonus = intLoopBonus;
                                    }
                                }
                            }, token).ConfigureAwait(false);
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
                        await (await CharacterObject.GetCyberwareAsync(token).ConfigureAwait(false))
                            .ForEachAsync(objCyberware => BuildTooltipAsync(sbdReturn, objCyberware), token)
                            .ConfigureAwait(false);

                        async Task BuildTooltipAsync(StringBuilder sb, Cyberware objCyberware)
                        {
                            if (!await objCyberware.GetIsLimbAsync(token).ConfigureAwait(false) ||
                                !await objCyberware.GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false))
                            {
                                return;
                            }

                            if (await objCyberware.GetInheritAttributesAsync(token).ConfigureAwait(false))
                            {
                                await objCyberware.Children
                                    .ForEachAsync(objChild => BuildTooltipAsync(sbdReturn, objChild), token)
                                    .ConfigureAwait(false);
                                return;
                            }

                            sb.AppendLine().AppendLine().Append(strExtraStart).Append(await LanguageManager
                                    .GetStringAsync("String_Colon", token: token)
                                    .ConfigureAwait(false)).Append(strSpace)
                                .Append(await CharacterObject
                                    .GetObjectNameAsync(objSwapSkillAttribute, token: token)
                                    .ConfigureAwait(false)).Append(strSpace)
                                .Append(await objCyberware.GetCurrentDisplayNameAsync(token).ConfigureAwait(false));
                            Grade objGrade = await objCyberware.GetGradeAsync(token).ConfigureAwait(false);
                            string strGradeName = objGrade.Name;
                            if (strGradeName != "Standard" && strGradeName != "None")
                            {
                                sb.Append(strSpace).Append('(')
                                    .Append(await objGrade.GetCurrentDisplayNameAsync(token).ConfigureAwait(false))
                                    .Append(')');
                            }

                            int intPool =
                                await PoolOtherAttributeAsync(objSwapSkillAttribute.ImprovedName, false,
                                        await objCyberware
                                            .GetAttributeTotalValueAsync(
                                                objSwapSkillAttribute.ImprovedName, token)
                                            .ConfigureAwait(false), token)
                                    .ConfigureAwait(false);
                            if (objSpecialization != null)
                            {
                                intPool += await objSpecialization.GetSpecializationBonusAsync(token)
                                    .ConfigureAwait(false);
                            }

                            if (blnAmbidextrous || (await objCyberware.GetLimbSlotAsync(token).ConfigureAwait(false) !=
                                                    "arm" && !objCyberware.Name.ContainsAny(" Arm", " Hand"))
                                                || await objCyberware.GetLocationAsync(token).ConfigureAwait(false) ==
                                                await CharacterObject.GetPrimaryArmAsync(token).ConfigureAwait(false)
                                                || await objCyberware.GetLimbSlotCountAsync(token)
                                                    .ConfigureAwait(false) > 1)
                            {
                                sb.Append(strSpace).Append(intPool.ToString(GlobalSettings.CultureInfo));
                            }
                            else
                            {
                                sb.AppendFormat(GlobalSettings.CultureInfo, "{1}{0}{1}({2}{1}{3})", intPool - 2,
                                    strSpace, -2,
                                    await LanguageManager.GetStringAsync("Tip_Skill_OffHand", token: token)
                                        .ConfigureAwait(false));
                            }

                            if (!string.IsNullOrEmpty(strExtra))
                                sb.Append(strExtra);
                        }
                    }

                    return sbdReturn.ToString();
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string UpgradeToolTip
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int intCost = UpgradeKarmaCost;
                    return intCost < 0
                        ? LanguageManager.GetString("Tip_ImproveItemAtMaximum")
                        : string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_ImproveItem"),
                                        Rating + 1, intCost);
                }
            }
        }

        public async Task<string> GetUpgradeToolTipAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intCost = await GetUpgradeKarmaCostAsync(token).ConfigureAwait(false);
                return intCost < 0
                    ? await LanguageManager.GetStringAsync("Tip_ImproveItemAtMaximum", token: token)
                                           .ConfigureAwait(false)
                    : string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Tip_ImproveItem", token: token)
                                                         .ConfigureAwait(false),
                                    await GetRatingAsync(token).ConfigureAwait(false) + 1, intCost);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string AddSpecToolTip
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int intPrice = IsKnowledgeSkill
                        ? CharacterObject.Settings.KarmaKnowledgeSpecialization
                        : CharacterObject.Settings.KarmaSpecialization;

                    decimal decExtraSpecCost = 0;
                    int intTotalBaseRating = TotalBaseRating;
                    decimal decSpecCostMultiplier = 1.0m;
                    CharacterObject.Improvements.ForEach(objLoopImprovement =>
                    {
                        if (objLoopImprovement.Minimum > intTotalBaseRating
                            || (!string.IsNullOrEmpty(objLoopImprovement.Condition)
                                && (objLoopImprovement.Condition == "career") != CharacterObject.Created
                                && (objLoopImprovement.Condition == "create") == CharacterObject.Created)
                            || !objLoopImprovement.Enabled)
                            return;
                        if (objLoopImprovement.ImprovedName != SkillCategory)
                            return;
                        switch (objLoopImprovement.ImproveType)
                        {
                            case Improvement.ImprovementType.SkillCategorySpecializationKarmaCost:
                                decExtraSpecCost += objLoopImprovement.Value;
                                break;

                            case Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier:
                                decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                                break;
                        }
                    });

                    if (decSpecCostMultiplier != 1.0m)
                        intPrice = (intPrice * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
                    else
                        intPrice += decExtraSpecCost.StandardRound(); //Spec
                    return string.Format(GlobalSettings.CultureInfo,
                        LanguageManager.GetString("Tip_Skill_AddSpecialization"), intPrice);
                }
            }
        }

        public async Task<string> GetAddSpecToolTipAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intPrice = IsKnowledgeSkill
                    ? await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false)).GetKarmaKnowledgeSpecializationAsync(token).ConfigureAwait(false)
                    : await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false)).GetKarmaSpecializationAsync(token).ConfigureAwait(false);

                int intTotalBaseRating = await GetTotalBaseRatingAsync(token).ConfigureAwait(false);
                decimal decSpecCostMultiplier = 1.0m;
                bool blnCreated = await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false);
                decimal decExtraSpecCost = await CharacterObject.Improvements.SumAsync(objLoopImprovement =>
                {
                    if (objLoopImprovement.Minimum > intTotalBaseRating
                        || (!string.IsNullOrEmpty(objLoopImprovement.Condition)
                            && (objLoopImprovement.Condition == "career") != blnCreated
                            && (objLoopImprovement.Condition == "create") == blnCreated)
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
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string SkillToolTip
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    string strSpace = LanguageManager.GetString("String_Space");
                    string strNotes = Notes;
                    string strReturn = !string.IsNullOrEmpty(strNotes)
                        ? LanguageManager.GetString("Label_Notes") + strSpace + strNotes + Environment.NewLine +
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

        public async Task<string> GetSkillToolTipAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
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
                strReturn += await GetCurrentDisplayCategoryAsync(token).ConfigureAwait(false) + Environment.NewLine + strMiddle
                             + await (await SourceString.GetSourceStringAsync(Source, Page, GlobalSettings.Language,
                                                                      GlobalSettings.CultureInfo, CharacterObject, token).ConfigureAwait(false))
                                .GetLanguageBookTooltipAsync(token).ConfigureAwait(false);
                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

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
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _strNotes, value) == value)
                        return;
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

        public Color PreferredColor
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return !string.IsNullOrEmpty(Notes)
                        ? ColorManager.GenerateCurrentModeColor(NotesColor)
                        : ColorManager.ControlText;
            }
        }

        public async Task<Color> GetPreferredColorAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return !string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false))
                    ? ColorManager.GenerateCurrentModeColor(await GetNotesColorAsync(token).ConfigureAwait(false))
                    : ColorManager.ControlText;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public SkillGroup SkillGroupObject { get; private set; }

        public string Page { get; }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <returns></returns>
        public string DisplayPage(string strLanguage)
        {
            using (LockObject.EnterReadLock())
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
        public async Task<string> DisplayPageAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return Page;
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                string strReturn = objNode?.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value ?? Page;
                return !string.IsNullOrWhiteSpace(strReturn) ? strReturn : Page;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
                using (LockObject.EnterReadLock())
                    return AttributeObject.TotalValue;
            }
        }

        public async Task<int> GetAttributeModifiersAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await (await GetAttributeObjectAsync(token).ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DisplayName(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return Name;

                return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
            }
        }

        public async Task<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return Name;

                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                return objNode != null
                    ? objNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? Name
                    : Name;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
            DisplayNameAsync(GlobalSettings.Language, token);

        public string CurrentDisplayCategory => DisplayCategory(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayCategoryAsync(CancellationToken token = default) =>
            DisplayCategoryAsync(GlobalSettings.Language, token);

        public string DisplayCategory(string strLanguage)
        {
            using (LockObject.EnterReadLock())
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

        public async Task<string> DisplayCategoryAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return SkillCategory;

                string strReturn = (await CharacterObject.LoadDataXPathAsync("skills.xml", strLanguage, token: token)
                        .ConfigureAwait(false))
                    .SelectSingleNode(
                        "/chummer/categories/category[. = " + SkillCategory.CleanXPath()
                                                            + "]/@translate")?.Value;

                return strReturn ?? SkillCategory;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DisplayPool
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return IsNativeLanguage ? LanguageManager.GetString("Skill_NativeLanguageShort") : DisplayOtherAttribute(Attribute);
            }
        }

        public async Task<string> GetDisplayPoolAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetIsNativeLanguageAsync(token).ConfigureAwait(false)
                    ? await LanguageManager.GetStringAsync("Skill_NativeLanguageShort", token: token).ConfigureAwait(false)
                    : await DisplayOtherAttributeAsync(await GetAttributeAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DisplayOtherAttribute(string strAttribute)
        {
            using (LockObject.EnterReadLock())
            {
                int intPool = PoolOtherAttribute(strAttribute);
                int intConditionalBonus = PoolOtherAttribute(strAttribute, true);
                int intSpecBonus;
                if (intPool == intConditionalBonus)
                {
                    if (IsExoticSkill
                        || Specializations.Count == 0
                        || !CanHaveSpecs
                        || ImprovementManager.GetCachedImprovementListForValueOf(
                                                 CharacterObject,
                                                 Improvement.ImprovementType.DisableSpecializationEffects,
                                                 DictionaryKey)
                                             .Count > 0)
                    {
                        return intPool.ToString(GlobalSettings.CultureInfo);
                    }

                    intSpecBonus = GetSpecializationBonus();
                    if (intSpecBonus == 0)
                        return intPool.ToString(GlobalSettings.CultureInfo);
                }
                else
                    intSpecBonus = GetSpecializationBonus();

                return string.Format(GlobalSettings.CultureInfo, "{0}{1}({2})",
                                     intPool, LanguageManager.GetString("String_Space"),
                                     Math.Max(intPool + intSpecBonus,
                                              intConditionalBonus)); // Have to do it this way because some conditional bonuses apply specifically to specializations
            }
        }

        public async Task<string> DisplayOtherAttributeAsync(string strAttribute, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intPool = await PoolOtherAttributeAsync(strAttribute, token: token).ConfigureAwait(false);
                int intConditionalBonus =
                    await PoolOtherAttributeAsync(strAttribute, true, token: token).ConfigureAwait(false);
                int intSpecBonus;
                if (intPool == intConditionalBonus)
                {
                    if (IsExoticSkill
                        || await Specializations.GetCountAsync(token).ConfigureAwait(false) == 0
                        || !await GetCanHaveSpecsAsync(token).ConfigureAwait(false)
                        || (await ImprovementManager
                                  .GetCachedImprovementListForValueOfAsync(
                                      CharacterObject, Improvement.ImprovementType.DisableSpecializationEffects,
                                      await GetDictionaryKeyAsync(token).ConfigureAwait(false), token: token)
                                  .ConfigureAwait(false)).Count > 0)
                    {
                        return intPool.ToString(GlobalSettings.CultureInfo);
                    }

                    intSpecBonus = await GetSpecializationBonusAsync(token: token).ConfigureAwait(false);
                    if (intSpecBonus == 0)
                        return intPool.ToString(GlobalSettings.CultureInfo);
                }
                else
                    intSpecBonus = await GetSpecializationBonusAsync(token: token).ConfigureAwait(false);

                return string.Format(GlobalSettings.CultureInfo, "{0}{1}({2})",
                    intPool, await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false),
                    Math.Max(intPool + intSpecBonus,
                        intConditionalBonus)); // Have to do it this way because some conditional bonuses apply specifically to specializations
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int GetSpecializationBonus(string strSpecialization = "")
        {
            using (LockObject.EnterReadLock())
            {
                if (IsExoticSkill || Specializations.Count == 0 || !CanHaveSpecs || TotalBaseRating == 0)
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
                        Specializations.ForEach(objSpec =>
                        {
                            int intLoopSpecBonus = objSpec.SpecializationBonus;
                            if (intHighestSpecBonus < intLoopSpecBonus)
                            {
                                intHighestSpecBonus = intLoopSpecBonus;
                                objTargetSpecialization = objSpec;
                            }
                        });
                    }
                }
                else
                    objTargetSpecialization = GetSpecialization(strSpecialization);

                return objTargetSpecialization?.SpecializationBonus ?? 0;
            }
        }

        public async Task<int> GetSpecializationBonusAsync(string strSpecialization = "", CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (IsExoticSkill
                    || await Specializations.GetCountAsync(token).ConfigureAwait(false) == 0
                    || !await GetCanHaveSpecsAsync(token).ConfigureAwait(false)
                    || await GetTotalBaseRatingAsync(token).ConfigureAwait(false) == 0)
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
                        await Specializations.ForEachAsync(async objSpec =>
                        {
                            int intLoopSpecBonus =
                                await objSpec.GetSpecializationBonusAsync(token).ConfigureAwait(false);
                            if (intHighestSpecBonus < intLoopSpecBonus)
                            {
                                intHighestSpecBonus = intLoopSpecBonus;
                                objTargetSpecialization = objSpec;
                            }
                        }, token).ConfigureAwait(false);
                    }
                }
                else
                    objTargetSpecialization =
                        await GetSpecializationAsync(strSpecialization, token).ConfigureAwait(false);

                return objTargetSpecialization != null
                    ? await objTargetSpecialization.GetSpecializationBonusAsync(token).ConfigureAwait(false)
                    : 0;
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
                        ? CharacterObject.LoadData("skills.xml", strLanguage, token: token)
                        : await CharacterObject.LoadDataAsync("skills.xml", strLanguage, token: token)
                                               .ConfigureAwait(false))
                    .TryGetNodeById(IsKnowledgeSkill ? "/chummer/knowledgeskills/skill" : "/chummer/skills/skill",
                                    SkillId);
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
                        ? CharacterObject.LoadDataXPath("skills.xml", strLanguage, token: token)
                        : await CharacterObject.LoadDataXPathAsync("skills.xml", strLanguage, token: token)
                                               .ConfigureAwait(false))
                    .TryGetNodeById(IsKnowledgeSkill ? "/chummer/knowledgeskills/skill" : "/chummer/skills/skill",
                                    SkillId);
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

        // ReSharper disable once InconsistentNaming
        private int _intCachedCyberwareRating = int.MinValue;

        [CLSCompliant(false)]
        protected readonly AsyncFriendlyReaderWriterLock _objCachedCyberwareRatingLock;

        protected virtual void ResetCachedCyberwareRating()
        {
            using (_objCachedCyberwareRatingLock.EnterWriteLock())
                _intCachedCyberwareRating = int.MinValue;
        }

        protected virtual async Task ResetCachedCyberwareRatingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker =
                await _objCachedCyberwareRatingLock.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _intCachedCyberwareRating = int.MinValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The attributeValue this skill have from Skillwires + Skilljack or Active Hardwires
        /// </summary>
        /// <returns>Artificial skill attributeValue</returns>
        public virtual int CyberwareRating
        {
            get
            {
                using (_objCachedCyberwareRatingLock.EnterReadLock())
                {
                    if (_intCachedCyberwareRating != int.MinValue)
                        return _intCachedCyberwareRating;
                }

                using (_objCachedCyberwareRatingLock.EnterUpgradeableReadLock())
                {
                    if (_intCachedCyberwareRating != int.MinValue)
                        return _intCachedCyberwareRating;
                    using (_objCachedCyberwareRatingLock.EnterWriteLock())
                    {
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
        public virtual async Task<int> GetCyberwareRatingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCachedCyberwareRatingLock.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedCyberwareRating != int.MinValue)
                    return _intCachedCyberwareRating;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await _objCachedCyberwareRatingLock.EnterUpgradeableReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedCyberwareRating != int.MinValue)
                    return _intCachedCyberwareRating;

                IAsyncDisposable objLocker2 =
                    await _objCachedCyberwareRatingLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    //TODO: method is here, but not used in any form, needs testing (worried about child items...)
                    //this might do hardwires if i understand how they works correctly
                    int intMaxHardwire = -1;
                    foreach (Improvement objImprovement in await ImprovementManager
                                 .GetCachedImprovementListForValueOfAsync(
                                     CharacterObject, Improvement.ImprovementType.Hardwire,
                                     await GetDictionaryKeyAsync(token)
                                         .ConfigureAwait(false), token: token)
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
                                    .ValueOfAsync(CharacterObject, Improvement.ImprovementType.Skillwire,
                                        token: token)
                                    .ConfigureAwait(false),
                                await ImprovementManager.ValueOfAsync(CharacterObject,
                                    Improvement.ImprovementType.SkillsoftAccess,
                                    token: token).ConfigureAwait(false))
                            .StandardRound();
                    if (intMaxActivesoftRating > 0)
                    {
                        int intMax = 0;
                        //TODO this works with translate?
                        foreach (Improvement objSkillsoftImprovement in await ImprovementManager
                                     .GetCachedImprovementListForValueOfAsync(
                                         CharacterObject,
                                         Improvement.ImprovementType.Activesoft,
                                         await GetDictionaryKeyAsync(token)
                                             .ConfigureAwait(false), token: token)
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
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Calculations

        #region Static

        //A tree of dependencies. Once some of the properties are changed,
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly PropertyDependencyGraph<Skill> s_SkillDependencyGraph =
            new PropertyDependencyGraph<Skill>(
                new DependencyGraphNode<string, Skill>(nameof(Pool),
                    new DependencyGraphNode<string, Skill>(nameof(NonTrivialPool),
                        new DependencyGraphNode<string, Skill>(nameof(AttributeModifiers),
                            new DependencyGraphNode<string, Skill>(nameof(AttributeObject),
                                new DependencyGraphNode<string, Skill>(nameof(Attribute)),
                                new DependencyGraphNode<string, Skill>(nameof(RelevantImprovements))
                            )
                        ),
                        new DependencyGraphNode<string, Skill>(nameof(Enabled)),
                        new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage))
                    ),
                    new DependencyGraphNode<string, Skill>(nameof(PoolOtherAttribute), x => x.NonTrivialPool, (x, t) => x.GetNonTrivialPoolAsync(t),
                        new DependencyGraphNode<string, Skill>(nameof(NonTrivialPool)),
                        new DependencyGraphNode<string, Skill>(nameof(Rating),
                            new DependencyGraphNode<string, Skill>(nameof(CyberwareRating),
                                new DependencyGraphNode<string, Skill>(nameof(DictionaryKey))
                            ),
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
                            )
                        ),
                        new DependencyGraphNode<string, Skill>(nameof(GetSpecializationBonus),
                            new DependencyGraphNode<string, Skill>(nameof(IsExoticSkill)),
                            new DependencyGraphNode<string, Skill>(nameof(Specializations),
                                new DependencyGraphNode<string, Skill>(nameof(CanHaveSpecs), x => !x.IsExoticSkill) // Not strictly dependent like this, but fetched every time specializations are
                            ),
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
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(PoolToolTip),
                    new DependencyGraphNode<string, Skill>(nameof(DisplayPool),
                        new DependencyGraphNode<string, Skill>(nameof(NonTrivialPool)),
                        new DependencyGraphNode<string, Skill>(nameof(DisplayOtherAttribute), x => x.NonTrivialPool, (x, t) => x.GetNonTrivialPoolAsync(t),
                            new DependencyGraphNode<string, Skill>(nameof(NonTrivialPool)),
                            new DependencyGraphNode<string, Skill>(nameof(PoolOtherAttribute), x => x.NonTrivialPool, (x, t) => x.GetNonTrivialPoolAsync(t)),
                            new DependencyGraphNode<string, Skill>(nameof(DictionaryKey)),
                            new DependencyGraphNode<string, Skill>(nameof(IsExoticSkill)),
                            new DependencyGraphNode<string, Skill>(nameof(Specializations)),
                            new DependencyGraphNode<string, Skill>(nameof(GetSpecializationBonus))
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
                    new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating)),
                    new DependencyGraphNode<string, Skill>(nameof(Leveled),
                        new DependencyGraphNode<string, Skill>(nameof(Rating))
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
                        new DependencyGraphNode<string, Skill>(nameof(SkillGroupObject), x => x.CharacterObject.Settings.CompensateSkillGroupKarmaDifference && x.Enabled, async (x, t) => await
                            (await x.CharacterObject.GetSettingsAsync(t).ConfigureAwait(false)).GetCompensateSkillGroupKarmaDifferenceAsync(t).ConfigureAwait(false) && await x.GetEnabledAsync(t).ConfigureAwait(false)),
                        new DependencyGraphNode<string, Skill>(nameof(Enabled), x => x.CharacterObject.Settings.CompensateSkillGroupKarmaDifference && x.SkillGroupObject != null, async (x, t) => await
                            (await x.CharacterObject.GetSettingsAsync(t).ConfigureAwait(false)).GetCompensateSkillGroupKarmaDifferenceAsync(t).ConfigureAwait(false) && x.SkillGroupObject != null)
                    ),
                    new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating)),
                    new DependencyGraphNode<string, Skill>(nameof(Base)),
                    new DependencyGraphNode<string, Skill>(nameof(FreeKarma)),
                    new DependencyGraphNode<string, Skill>(nameof(Specializations))
                ),
                new DependencyGraphNode<string, Skill>(nameof(CanUpgradeCareer),
                    new DependencyGraphNode<string, Skill>(nameof(UpgradeKarmaCost),
                        new DependencyGraphNode<string, Skill>(nameof(SkillGroupObject), x => x.CharacterObject.Settings.CompensateSkillGroupKarmaDifference && x.Enabled, async (x, t) => await
                            (await x.CharacterObject.GetSettingsAsync(t).ConfigureAwait(false)).GetCompensateSkillGroupKarmaDifferenceAsync(t).ConfigureAwait(false) && await x.GetEnabledAsync(t).ConfigureAwait(false)),
                        new DependencyGraphNode<string, Skill>(nameof(Enabled), x => x.CharacterObject.Settings.CompensateSkillGroupKarmaDifference && x.SkillGroupObject != null, async (x, t) => await
                            (await x.CharacterObject.GetSettingsAsync(t).ConfigureAwait(false)).GetCompensateSkillGroupKarmaDifferenceAsync(t).ConfigureAwait(false) && x.SkillGroupObject != null)
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
                new DependencyGraphNode<string, Skill>(nameof(AllowDelete),
                    new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill)),
                    new DependencyGraphNode<string, Skill>(nameof(IsExoticSkill)),
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.ForcedName), x => x.IsKnowledgeSkill,
                        new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill))
                    ),
                    new DependencyGraphNode<string, Skill>(nameof(FreeBase), x => x.IsKnowledgeSkill || x.IsExoticSkill),
                    new DependencyGraphNode<string, Skill>(nameof(FreeKarma), x => x.IsKnowledgeSkill || x.IsExoticSkill),
                    new DependencyGraphNode<string, Skill>(nameof(RatingModifiers), x => x.IsKnowledgeSkill || x.IsExoticSkill),
                    new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage), x => x.IsKnowledgeSkill,
                        new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill))
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(DictionaryKey),
                    new DependencyGraphNode<string, Skill>(nameof(Name)),
                    new DependencyGraphNode<string, Skill>(nameof(IsExoticSkill)),
                    new DependencyGraphNode<string, Skill>(nameof(ExoticSkill.Specific), x => x.IsExoticSkill)
                ),
                new DependencyGraphNode<string, Skill>(nameof(IsLanguage),
                    new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill)),
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.Type), x => x.IsKnowledgeSkill)
                ),
                new DependencyGraphNode<string, Skill>(nameof(AllowNameChange),
                    new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill)),
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.ForcedName), x => x.IsKnowledgeSkill),
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.AllowUpgrade), x => x.IsKnowledgeSkill),
                    new DependencyGraphNode<string, Skill>(nameof(Karma), x => x.IsKnowledgeSkill),
                    new DependencyGraphNode<string, Skill>(nameof(Base), x => x.IsKnowledgeSkill),
                    new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage), x => x.IsKnowledgeSkill)
                ),
                new DependencyGraphNode<string, Skill>(nameof(AllowTypeChange),
                    new DependencyGraphNode<string, Skill>(nameof(IsKnowledgeSkill)),
                    new DependencyGraphNode<string, Skill>(nameof(AllowNameChange), x => x.IsKnowledgeSkill),
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.Type), x => x.IsKnowledgeSkill),
                    new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage), x => x.IsKnowledgeSkill)
                )
            );

        #endregion Static

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

        private static readonly HashSet<string> s_SetPropertyNamesWithCachedValues = new HashSet<string>
        {
            nameof(FreeBase),
            nameof(FreeKarma),
            nameof(CanUpgradeCareer),
            nameof(CanAffordSpecialization),
            nameof(Enabled),
            nameof(CanHaveSpecs),
            nameof(ForcedBuyWithKarma),
            nameof(ForcedNotBuyWithKarma),
            nameof(CyberwareRating),
            nameof(CGLSpecializations)
        };

        public void OnMultiplePropertiesChanged(IReadOnlyCollection<string> lstPropertyNames)
        {
            if (IsLoading)
                return;
            using (LockObject.EnterUpgradeableReadLock())
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

                    if (setNamesOfChangedProperties.Overlaps(s_SetPropertyNamesWithCachedValues))
                    {
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
                            if (setNamesOfChangedProperties.Contains(nameof(ForcedBuyWithKarma)))
                                _intCachedForcedBuyWithKarma = -1;
                            if (setNamesOfChangedProperties.Contains(nameof(ForcedNotBuyWithKarma)))
                                _intCachedForcedNotBuyWithKarma = -1;
                            if (setNamesOfChangedProperties.Contains(nameof(CyberwareRating)))
                                ResetCachedCyberwareRating();
                            if (setNamesOfChangedProperties.Contains(nameof(TotalBaseRating)))
                                ResetCachedTotalBaseRating();
                            if (setNamesOfChangedProperties.Contains(nameof(LearnedRating)))
                                ResetCachedLearnedRating();
                            if (setNamesOfChangedProperties.Contains(nameof(Attribute)))
                            {
                                ResetCachedPoolModifiers();
                                ResetCachedRatingModifiers();
                            }
                            else
                            {
                                if (setNamesOfChangedProperties.Contains(nameof(PoolModifiers)))
                                    ResetCachedPoolModifiers();
                                if (setNamesOfChangedProperties.Contains(nameof(RatingModifiers)))
                                    ResetCachedRatingModifiers();
                            }
                            if (setNamesOfChangedProperties.Contains(nameof(CGLSpecializations)))
                                _blnRecalculateCachedSuggestedSpecializations = true;
                            if (setNamesOfChangedProperties.Contains(nameof(CanHaveSpecs)))
                            {
                                _intCachedCanHaveSpecs = -1;
                                if (!CharacterObject.IsLoading && !CharacterObject.Created && Specializations.Count > 0 && !CanHaveSpecs)
                                    Specializations.Clear();
                            }
                        }
                    }

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
            if (IsLoading)
                return;
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
                                = await s_SkillDependencyGraph.GetWithAllDependentsAsync(this, strPropertyName, true, token).ConfigureAwait(false);
                        else
                        {
                            foreach (string strLoopChangedProperty in await s_SkillDependencyGraph
                                         .GetWithAllDependentsEnumerableAsync(this, strPropertyName, token).ConfigureAwait(false))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    if (setNamesOfChangedProperties.Overlaps(s_SetPropertyNamesWithCachedValues))
                    {
                        IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
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
                            if (setNamesOfChangedProperties.Contains(nameof(ForcedBuyWithKarma)))
                                _intCachedForcedBuyWithKarma = -1;
                            if (setNamesOfChangedProperties.Contains(nameof(ForcedNotBuyWithKarma)))
                                _intCachedForcedNotBuyWithKarma = -1;
                            if (setNamesOfChangedProperties.Contains(nameof(CyberwareRating)))
                                await ResetCachedCyberwareRatingAsync(token).ConfigureAwait(false);
                            if (setNamesOfChangedProperties.Contains(nameof(TotalBaseRating)))
                                await ResetCachedTotalBaseRatingAsync(token).ConfigureAwait(false);
                            if (setNamesOfChangedProperties.Contains(nameof(LearnedRating)))
                                await ResetCachedLearnedRatingAsync(token).ConfigureAwait(false);
                            if (setNamesOfChangedProperties.Contains(nameof(Attribute)))
                            {
                                await ResetCachedPoolModifiersAsync(token).ConfigureAwait(false);
                                await ResetCachedRatingModifiersAsync(token).ConfigureAwait(false);
                            }
                            else
                            {
                                if (setNamesOfChangedProperties.Contains(nameof(PoolModifiers)))
                                    await ResetCachedPoolModifiersAsync(token).ConfigureAwait(false);
                                if (setNamesOfChangedProperties.Contains(nameof(RatingModifiers)))
                                    await ResetCachedRatingModifiersAsync(token).ConfigureAwait(false);
                            }
                            if (setNamesOfChangedProperties.Contains(nameof(CGLSpecializations)))
                                _blnRecalculateCachedSuggestedSpecializations = true;
                            if (setNamesOfChangedProperties.Contains(nameof(CanHaveSpecs)))
                            {
                                _intCachedCanHaveSpecs = -1;
                                if (!CharacterObject.IsLoading && !await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false))
                                {
                                    ThreadSafeObservableCollection<SkillSpecialization> lstSpecs = await GetSpecializationsAsync(token).ConfigureAwait(false);
                                    if (await lstSpecs.GetCountAsync(token).ConfigureAwait(false) > 0 && !await GetCanHaveSpecsAsync(token).ConfigureAwait(false))
                                        await lstSpecs.ClearAsync(token).ConfigureAwait(false);
                                }
                            }
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }
                    }

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

                    // Do this after firing all property changers. Not part of the dependency graph because dependency is very complicated
                    if (setNamesOfChangedProperties.Contains(nameof(DefaultAttribute)))
                        await RecacheAttributeAsync(token).ConfigureAwait(false);

                    if (setNamesOfChangedProperties.Contains(nameof(Enabled)) && CharacterObject != null &&
                        SkillGroupObject != null)
                    {
                        CharacterSettings objSettings =
                            await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                        if (objSettings != null && await objSettings.GetCompensateSkillGroupKarmaDifferenceAsync(token)
                                .ConfigureAwait(false))
                        {
                            List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                            int i = 0;
                            foreach (Skill objSkill in SkillGroupObject.SkillList)
                            {
                                if (objSkill == this)
                                    continue;
                                lstTasks.Add(objSkill.OnMultiplePropertyChangedAsync(token, nameof(UpgradeKarmaCost),
                                    nameof(RangeCost)));
                                if (++i < Utils.MaxParallelBatchSize)
                                    continue;
                                await Task.WhenAll(lstTasks).ConfigureAwait(false);
                                lstTasks.Clear();
                                i = 0;
                            }

                            await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        }
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

        private async Task OnSkillGroupChanged(object sender, MultiplePropertiesChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (IsLoading || CharacterObject?.IsLoading != false)
                return;
            List<string> lstProperties = new List<string>();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!await GetEnabledAsync(token).ConfigureAwait(false)) // Skip updates for disabled skills because it's unnecessary
                    return;
                CharacterSettings objSettings =
                    await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                if (e.PropertyNames.Contains(nameof(Skills.SkillGroup.Base)))
                {
                    lstProperties.Add(nameof(Base));
                    if (!await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false)
                        && await (await GetSpecializationsAsync(token).ConfigureAwait(false))
                            .AnyAsync(async x => !await x.GetFreeAsync(token).ConfigureAwait(false), token: token)
                            .ConfigureAwait(false)
                        && !(await GetKarmaPointsAsync(token).ConfigureAwait(false) > 0
                             && await GetBasePointsAsync(token).ConfigureAwait(false)
                             + await GetFreeBaseAsync(token).ConfigureAwait(false) == 0
                             && !await objSettings.GetAllowPointBuySpecializationsOnKarmaSkillsAsync(token)
                                 .ConfigureAwait(false))
                        && await objSettings.GetSpecializationsBreakSkillGroupsAsync(token)
                            .ConfigureAwait(false))
                    {
                        lstProperties.Add(nameof(ForcedBuyWithKarma));
                    }

                    if (await CharacterObject.GetEffectiveBuildMethodUsesPriorityTablesAsync(token)
                            .ConfigureAwait(false))
                    {
                        lstProperties.Add(nameof(BaseUnlocked));
                    }
                }

                if (e.PropertyNames.Contains(nameof(Skills.SkillGroup.Karma)))
                {
                    lstProperties.Add(nameof(Karma));
                    if (!await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                    {
                        if (!lstProperties.Contains(nameof(ForcedBuyWithKarma))
                            && await (await GetSpecializationsAsync(token).ConfigureAwait(false))
                                .AnyAsync(async x => !await x.GetFreeAsync(token).ConfigureAwait(false), token: token)
                                .ConfigureAwait(false)
                            && !(await GetKarmaPointsAsync(token).ConfigureAwait(false) > 0
                                 && await GetBasePointsAsync(token).ConfigureAwait(false)
                                 + await GetFreeBaseAsync(token).ConfigureAwait(false) == 0
                                 && !await objSettings.GetAllowPointBuySpecializationsOnKarmaSkillsAsync(token)
                                     .ConfigureAwait(false))
                            && await objSettings.GetSpecializationsBreakSkillGroupsAsync(token)
                                .ConfigureAwait(false))
                        {
                            lstProperties.Add(nameof(ForcedBuyWithKarma));
                        }

                        if (!await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false))
                        {
                            lstProperties.Add(nameof(CurrentKarmaCost));
                            if (await objSettings.GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false)
                                && await GetTotalBaseRatingAsync(token).ConfigureAwait(false) != 0)
                            {
                                lstProperties.Add(nameof(ForcedNotBuyWithKarma));
                            }
                        }
                    }
                }

                if (e.PropertyNames.Contains(nameof(Skills.SkillGroup.Rating)) &&
                    await objSettings.GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false) &&
                    !await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false) &&
                    !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                {
                    lstProperties.Add(nameof(KarmaUnlocked));
                }

                if (e.PropertyNames.Contains(nameof(Skills.SkillGroup.SkillList)) &&
                    await objSettings.GetCompensateSkillGroupKarmaDifferenceAsync(token).ConfigureAwait(false) &&
                    await GetEnabledAsync(token).ConfigureAwait(false))
                {
                    lstProperties.Add(nameof(RangeCost));
                    lstProperties.Add(nameof(UpgradeKarmaCost));
                }

                if (lstProperties.Count > 0)
                    await OnMultiplePropertiesChangedAsync(lstProperties, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task OnCharacterChanged(object sender, MultiplePropertiesChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (IsLoading || CharacterObject?.IsLoading != false)
                return;
            List<string> lstProperties = new List<string>();
            if (e.PropertyNames.Contains(nameof(Character.Karma)))
            {
                lstProperties.Add(nameof(CanUpgradeCareer));
                lstProperties.Add(nameof(CanAffordSpecialization));
            }
            if (e.PropertyNames.Contains(nameof(Character.WoundModifier))
                || e.PropertyNames.Contains(nameof(Character.SustainingPenalty)))
                lstProperties.Add(nameof(PoolOtherAttribute));
            if (e.PropertyNames.Contains(nameof(Character.PrimaryArm)))
                lstProperties.Add(nameof(PoolToolTip));
            if (e.PropertyNames.Contains(nameof(Character.EffectiveBuildMethodUsesPriorityTables)))
            {
                lstProperties.Add(nameof(Base));
                lstProperties.Add(nameof(BaseUnlocked));
                lstProperties.Add(nameof(ForcedBuyWithKarma));
            }
            if (e.PropertyNames.Contains(nameof(Character.IsCritter)))
                lstProperties.Add(nameof(Default));

            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (e.PropertyNames.Contains(nameof(Character.GetMovement)) &&
                    await GetRequiresGroundMovementAsync(token).ConfigureAwait(false))
                    lstProperties.Add(nameof(Enabled));
                else if (e.PropertyNames.Contains(nameof(Character.GetSwim)) &&
                         await GetRequiresSwimMovementAsync(token).ConfigureAwait(false))
                    lstProperties.Add(nameof(Enabled));
                else if (e.PropertyNames.Contains(nameof(Character.GetFly)) &&
                         await GetRequiresFlyMovementAsync(token).ConfigureAwait(false))
                    lstProperties.Add(nameof(Enabled));
                else
                {
                    switch (await GetAttributeAsync(token).ConfigureAwait(false))
                    {
                        case "MAG":
                        case "MAGAdept":
                            if (e.PropertyNames.Contains(nameof(Character.MAGEnabled)))
                                lstProperties.Add(nameof(Enabled));
                            break;
                        case "RES":
                            if (e.PropertyNames.Contains(nameof(Character.RESEnabled)))
                                lstProperties.Add(nameof(Enabled));
                            break;
                        case "DEP":
                            if (e.PropertyNames.Contains(nameof(Character.DEPEnabled)))
                                lstProperties.Add(nameof(Enabled));
                            break;
                    }
                }

                if (lstProperties.Count > 0)
                    await OnMultiplePropertiesChangedAsync(lstProperties, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task OnCharacterSettingsPropertyChanged(object sender, MultiplePropertiesChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (IsLoading || CharacterObject?.IsLoading != false)
                return;
            List<string> lstProperties = new List<string>();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (SkillGroupObject != null)
                {
                    if (e.PropertyNames.Contains(nameof(CharacterSettings.StrictSkillGroupsInCreateMode)))
                    {
                        if (!await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false))
                            lstProperties.Add(nameof(KarmaUnlocked));
                        lstProperties.Add(nameof(BaseUnlocked));
                        lstProperties.Add(nameof(ForcedNotBuyWithKarma));
                    }
                    else if (e.PropertyNames.Contains(nameof(CharacterSettings.UsePointsOnBrokenGroups)))
                    {
                        lstProperties.Add(nameof(BaseUnlocked));
                    }

                    if (await GetEnabledAsync(token).ConfigureAwait(false) &&
                        (e.PropertyNames.Contains(nameof(CharacterSettings.CompensateSkillGroupKarmaDifference))
                         || ((e.PropertyNames.Contains(nameof(CharacterSettings.KarmaNewSkillGroup))
                              || e.PropertyNames.Contains(nameof(CharacterSettings.KarmaImproveSkillGroup)))
                             && await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false)).GetCompensateSkillGroupKarmaDifferenceAsync(token)
                                 .ConfigureAwait(false))))
                    {
                        lstProperties.Add(nameof(RangeCost));
                        lstProperties.Add(nameof(UpgradeKarmaCost));
                    }
                }

                if (IsKnowledgeSkill)
                {
                    if (e.PropertyNames.Contains(nameof(CharacterSettings.KarmaKnowledgeSpecialization)))
                    {
                        lstProperties.Add(nameof(CurrentKarmaCost));
                        lstProperties.Add(nameof(CanAffordSpecialization));
                        lstProperties.Add(nameof(AddSpecToolTip));
                    }
                    else if (e.PropertyNames.Contains(nameof(CharacterSettings.KarmaNewKnowledgeSkill))
                             || e.PropertyNames.Contains(nameof(CharacterSettings.KarmaImproveKnowledgeSkill)))
                    {
                        lstProperties.Add(nameof(CurrentKarmaCost));
                    }
                }
                else if (e.PropertyNames.Contains(nameof(CharacterSettings.KarmaSpecialization)))
                {
                    lstProperties.Add(nameof(CurrentKarmaCost));
                    lstProperties.Add(nameof(CanAffordSpecialization));
                    lstProperties.Add(nameof(AddSpecToolTip));
                }
                else if (e.PropertyNames.Contains(nameof(CharacterSettings.KarmaNewActiveSkill))
                         || e.PropertyNames.Contains(nameof(CharacterSettings.KarmaImproveActiveSkill)))
                {
                    lstProperties.Add(nameof(CurrentKarmaCost));
                }

                if (await Specializations.GetCountAsync(token).ConfigureAwait(false) > 0 &&
                    await GetCanHaveSpecsAsync(token).ConfigureAwait(false) &&
                    (e.PropertyNames.Contains(nameof(CharacterSettings.SpecializationBonus))
                     || (e.PropertyNames.Contains(nameof(CharacterSettings.ExpertiseBonus))
                         && await Specializations.AnyAsync(x => x.GetExpertiseAsync(token), token: token)
                             .ConfigureAwait(false))))
                {
                    lstProperties.Add(nameof(PoolOtherAttribute));
                }

                if (lstProperties.Count > 0)
                    await OnMultiplePropertiesChangedAsync(lstProperties, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected Task OnLinkedAttributeChanged(object sender, MultiplePropertiesChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (IsLoading || CharacterObject?.IsLoading != false)
                return Task.CompletedTask;
            List<string> lstProperties = new List<string>(2);
            if (e.PropertyNames.Contains(nameof(CharacterAttrib.TotalValue)))
                lstProperties.Add(nameof(AttributeModifiers));
            if (e.PropertyNames.Contains(nameof(CharacterAttrib.Abbrev)))
                lstProperties.Add(nameof(Enabled));

            return lstProperties.Count > 0
                ? OnMultiplePropertiesChangedAsync(lstProperties, token)
                : Task.CompletedTask;
        }

        private int _intSkipSpecializationRefresh;

        private async Task SpecializationsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intSkipSpecializationRefresh > 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
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
                                await objSkillSpecialization.DisposeAsync().ConfigureAwait(false);
                            }

                            break;

                        case NotifyCollectionChangedAction.Replace:
                            foreach (SkillSpecialization objSkillSpecialization in e.OldItems)
                            {
                                if (objSkillSpecialization.Parent == this)
                                    objSkillSpecialization.Parent = null;
                                await objSkillSpecialization.DisposeAsync().ConfigureAwait(false);
                            }

                            foreach (SkillSpecialization objSkillSpecialization in e.NewItems)
                                objSkillSpecialization.Parent = this;
                            break;

                        case NotifyCollectionChangedAction.Reset:
                            await Specializations
                                .ForEachAsync(objSkillSpecialization => objSkillSpecialization.Parent = this,
                                    token: token).ConfigureAwait(false);
                            break;
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intSkipSpecializationRefresh);
                }

                await OnPropertyChangedAsync(nameof(Specializations), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task SpecializationsOnBeforeClearCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intSkipSpecializationRefresh > 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
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
                        await objSkillSpecialization.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intSkipSpecializationRefresh);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Calculate the karma cost of increasing a skill from lower to upper
        /// </summary>
        /// <param name="lower">Staring rating of skill</param>
        /// <param name="upper">End rating of the skill</param>
        /// <param name="blnForceOffCompensateSkillGroupKarmaDifference">Whether to force skill group karma compensation off. Needed to work around an issue in create mode.</param>
        protected int RangeCost(int lower, int upper, bool blnForceOffCompensateSkillGroupKarmaDifference = false)
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

            using (LockObject.EnterReadLock())
            {
                int intSkillGroupCostAdjustment = 0;
                if (!blnForceOffCompensateSkillGroupKarmaDifference && CharacterObject.Settings.CompensateSkillGroupKarmaDifference && SkillGroupObject != null)
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
                CharacterObject.Improvements.ForEach(objLoopImprovement =>
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
                });

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
        /// <param name="blnForceOffCompensateSkillGroupKarmaDifference">Whether to force skill group karma compensation off. Needed to work around an issue in create mode.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        protected async Task<int> RangeCostAsync(int lower, int upper, bool blnForceOffCompensateSkillGroupKarmaDifference = false, CancellationToken token = default)
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

            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intSkillGroupCostAdjustment = 0;
                if (!blnForceOffCompensateSkillGroupKarmaDifference
                    && await objSettings.GetCompensateSkillGroupKarmaDifferenceAsync(token).ConfigureAwait(false)
                    && SkillGroupObject != null)
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
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
                using (LockObject.EnterReadLock())
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
                    CharacterObject.Improvements.ForEach(objLoopImprovement =>
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
                    });

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
        public virtual async Task<int> GetUpgradeKarmaCostAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
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
                    intOptionsCost = await objSettings.GetKarmaNewActiveSkillAsync(token).ConfigureAwait(false);
                    upgrade += intOptionsCost;
                }
                else
                {
                    intOptionsCost = await objSettings.GetKarmaImproveActiveSkillAsync(token).ConfigureAwait(false);
                    upgrade += (intTotalBaseRating + 1) * intOptionsCost;
                }

                int intSkillGroupCostAdjustment = 0;
                if (await objSettings.GetCompensateSkillGroupKarmaDifferenceAsync(token).ConfigureAwait(false) && SkillGroupObject != null)
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
                            intGroupCost = await objSettings.GetKarmaNewSkillGroupAsync(token).ConfigureAwait(false);
                            intNakedSkillCost *= await objSettings.GetKarmaNewActiveSkillAsync(token).ConfigureAwait(false);
                        }
                        else
                        {
                            intGroupCost = (intTotalBaseRating + 1) *
                                           await objSettings.GetKarmaImproveSkillGroupAsync(token).ConfigureAwait(false);
                            intNakedSkillCost *= (intTotalBaseRating + 1) *
                                                 await objSettings.GetKarmaImproveActiveSkillAsync(token).ConfigureAwait(false);
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
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task Upgrade(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
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
                                    : "String_ExpenseActiveSkill",
                                token: token).ConfigureAwait(false),
                            await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                            intTotalBaseRating,
                            intTotalBaseRating + 1,
                            await LanguageManager.GetStringAsync("String_Space", token: token)
                                .ConfigureAwait(false));

                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(price * -1, upgradetext, ExpenseType.Karma, DateTime.Now);
                    objExpense.Undo = new ExpenseUndo().CreateKarma(
                        intTotalBaseRating == 0 ? KarmaExpenseType.AddSkill : KarmaExpenseType.ImproveSkill,
                        InternalId);

                    await CharacterObject.ExpenseEntries.AddWithSortAsync(objExpense, token: token)
                        .ConfigureAwait(false);

                    await CharacterObject.ModifyKarmaAsync(-price, token).ConfigureAwait(false);
                }

                await SetKarmaAsync(await GetKarmaAsync(token).ConfigureAwait(false) + 1, token)
                    .ConfigureAwait(false);
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
                using (LockObject.EnterReadLock())
                {
                    // intReturn for thread safety
                    int intReturn = _intCachedCanAffordSpecialization;
                    if (intReturn < 0)
                    {
                        if (!CanHaveSpecs)
                            intReturn = 0;
                        else
                        {
                            int intPrice = IsKnowledgeSkill
                                ? CharacterObject.Settings.KarmaKnowledgeSpecialization
                                : CharacterObject.Settings.KarmaSpecialization;

                            decimal decExtraSpecCost = 0;
                            int intTotalBaseRating = TotalBaseRating;
                            decimal decSpecCostMultiplier = 1.0m;
                            CharacterObject.Improvements.ForEach(objLoopImprovement =>
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
                            });

                            if (decSpecCostMultiplier != 1.0m)
                                intPrice = (intPrice * decSpecCostMultiplier + decExtraSpecCost)
                                    .StandardRound();
                            else
                                intPrice += decExtraSpecCost.StandardRound(); //Spec

                            intReturn = (intPrice <= CharacterObject.Karma).ToInt32();
                        }
                        _intCachedCanAffordSpecialization = intReturn;
                    }

                    return intReturn > 0;
                }
            }
        }

        public async Task<bool> GetCanAffordSpecializationAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // intReturn for thread safety
                int intReturn = _intCachedCanAffordSpecialization;
                if (intReturn < 0)
                {
                    if (!CanHaveSpecs)
                        intReturn = 0;
                    else
                    {
                        int intPrice = IsKnowledgeSkill
                            ? await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false)).GetKarmaKnowledgeSpecializationAsync(token).ConfigureAwait(false)
                            : await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false)).GetKarmaSpecializationAsync(token).ConfigureAwait(false);

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

                        intReturn
                            = (intPrice <= await CharacterObject.GetKarmaAsync(token).ConfigureAwait(false))
                            .ToInt32();
                    }
                    _intCachedCanAffordSpecialization = intReturn;
                }

                return intReturn > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task AddSpecialization(string strName, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                SkillSpecialization nspec = new SkillSpecialization(CharacterObject, strName);
                try
                {
                    bool blnCreated = await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false);
                    if (blnCreated)
                    {
                        int intPrice = IsKnowledgeSkill
                            ? await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false)).GetKarmaKnowledgeSpecializationAsync(token).ConfigureAwait(false)
                            : await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false)).GetKarmaSpecializationAsync(token).ConfigureAwait(false);

                        decimal decExtraSpecCost = 0;
                        int intTotalBaseRating = await GetTotalBaseRatingAsync(token).ConfigureAwait(false);
                        decimal decSpecCostMultiplier = 1.0m;
                        await CharacterObject.Improvements.ForEachAsync(objLoopImprovement =>
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
                        }, token).ConfigureAwait(false);

                        if (decSpecCostMultiplier != 1.0m)
                            intPrice = (intPrice * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
                        else
                            intPrice += decExtraSpecCost.StandardRound(); //Spec

                        if (intPrice > await CharacterObject.GetKarmaAsync(token).ConfigureAwait(false))
                            return;

                        string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                            .ConfigureAwait(false);
                        string strUpgradeText
                            = await LanguageManager.GetStringAsync("String_ExpenseLearnSpecialization", token: token)
                                .ConfigureAwait(false) + strSpace
                                                       + await GetCurrentDisplayNameAsync(token)
                                                           .ConfigureAwait(false) + strSpace + '('
                                                       + strName + ')';
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        objExpense.Create(intPrice * -1, strUpgradeText, ExpenseType.Karma, DateTime.Now);
                        objExpense.Undo =
                            new ExpenseUndo().CreateKarma(KarmaExpenseType.AddSpecialization, nspec.InternalId);

                        await CharacterObject.ExpenseEntries.AddWithSortAsync(objExpense, token: token)
                            .ConfigureAwait(false);

                        await CharacterObject.ModifyKarmaAsync(-intPrice, token).ConfigureAwait(false);
                    }

                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        await (await GetSpecializationsAsync(token).ConfigureAwait(false)).AddAsync(nspec, token)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                catch
                {
                    await nspec.DisposeAsync().ConfigureAwait(false);
                    throw;
                }
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
                using (LockObject.EnterReadLock())
                {
                    // intReturn for thread safety
                    int intReturn = _intCachedFreeKarma;
                    if (intReturn != int.MinValue)
                        return intReturn;

                    return _intCachedFreeKarma = string.IsNullOrEmpty(Name)
                        ? 0
                        : ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillLevel, false,
                                                     DictionaryKey).StandardRound();
                }
            }
        }

        /// <summary>
        /// How many free points of this skill have gotten, with the exception of some things during character creation
        /// </summary>
        public async Task<int> GetFreeKarmaAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // intReturn for thread safety
                int intReturn = _intCachedFreeKarma;
                if (intReturn != int.MinValue)
                    return intReturn;

                return _intCachedFreeKarma = string.IsNullOrEmpty(Name)
                    ? 0
                    : (await ImprovementManager.ValueOfAsync(CharacterObject,
                                                             Improvement.ImprovementType.SkillLevel,
                                                             false,
                                                             await GetDictionaryKeyAsync(token).ConfigureAwait(false),
                                                             token: token).ConfigureAwait(false))
                    .StandardRound();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
                using (LockObject.EnterReadLock())
                {
                    // intReturn for thread safety
                    int intReturn = _intCachedFreeBase;
                    if (intReturn != int.MinValue)
                        return intReturn;

                    return _intCachedFreeBase = string.IsNullOrEmpty(Name)
                        ? 0
                        : ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillBase, false,
                                                     DictionaryKey).StandardRound();
                }
            }
        }

        /// <summary>
        /// How many free points this skill have gotten during some parts of character creation
        /// </summary>
        public async Task<int> GetFreeBaseAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // intReturn for thread safety
                int intReturn = _intCachedFreeBase;
                if (intReturn != int.MinValue)
                    return intReturn;

                return _intCachedFreeBase = string.IsNullOrEmpty(Name)
                    ? 0
                    : (await ImprovementManager.ValueOfAsync(CharacterObject,
                                                             Improvement.ImprovementType.SkillBase,
                                                             false,
                                                             await GetDictionaryKeyAsync(token).ConfigureAwait(false),
                                                             token: token).ConfigureAwait(false)).StandardRound();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
                using (LockObject.EnterReadLock())
                {
                    // intReturn for thread safety
                    int intReturn = _intCachedForcedBuyWithKarma;
                    if (intReturn < 0)
                    {
                        intReturn = (!CharacterObject.IgnoreRules
                                     && Specializations.Any(x => !x.Free)
                                     && CanHaveSpecs
                                     && ((KarmaPoints > 0
                                          && BasePoints + FreeBase == 0
                                          && !CharacterObject.Settings
                                                             .AllowPointBuySpecializationsOnKarmaSkills)
                                         || (CharacterObject.Settings
                                                            .SpecializationsBreakSkillGroups
                                             && (SkillGroupObject?.Karma > 0
                                                 || SkillGroupObject?.Base > 0)))).ToInt32();
                        _intCachedForcedBuyWithKarma = intReturn;
                    }

                    return intReturn > 0;
                }
            }
        }

        /// <summary>
        /// Do circumstances force the skill to be bought with karma?
        /// </summary>
        private async Task<bool> GetForcedBuyWithKarmaAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // intReturn for thread safety
                int intReturn = _intCachedForcedBuyWithKarma;
                if (intReturn < 0)
                {
                    CharacterSettings objSettings =
                        await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                    intReturn =
                        (!await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false)
                         && await (await GetSpecializationsAsync(token).ConfigureAwait(false))
                                  .AnyAsync(async x => !await x.GetFreeAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false)
                         && await GetCanHaveSpecsAsync(token).ConfigureAwait(false)
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
                    _intCachedForcedBuyWithKarma = intReturn;
                }

                return intReturn > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
                using (LockObject.EnterReadLock())
                {
                    // intReturn for thread safety
                    int intReturn = _intCachedForcedNotBuyWithKarma;
                    if (intReturn < 0)
                    {
                        intReturn = (TotalBaseRating == 0
                                     || (CharacterObject.Settings
                                                        .StrictSkillGroupsInCreateMode
                                         && !CharacterObject.Created
                                         && !CharacterObject.IgnoreRules
                                         && SkillGroupObject?.Karma > 0)).ToInt32();
                        _intCachedForcedNotBuyWithKarma = intReturn;
                    }

                    return intReturn > 0;
                }
            }
        }

        /// <summary>
        /// Do circumstances force the skill to not be bought with karma?
        /// </summary>
        private async Task<bool> GetForcedNotBuyWithKarmaAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // intReturn for thread safety
                int intReturn = _intCachedForcedNotBuyWithKarma;
                if (intReturn < 0)
                {
                    intReturn =
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
                    _intCachedForcedNotBuyWithKarma = intReturn;
                }

                return intReturn > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
            using (LockObject.EnterReadLock())
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
        public async Task<string> FormattedDicePoolAsync(int intPool, string strValidSpec = "", CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
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
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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

        public async Task RemoveAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
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

        public bool IsDisposed => _intIsDisposed > 0;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) == 0)
            {
                Character objCharacter = CharacterObject; // for thread safety
                if (objCharacter != null)
                {
                    if (!objCharacter.IsDisposed)
                    {
                        try
                        {
                            objCharacter.MultiplePropertiesChangedAsync -= OnCharacterChanged;
                            AttributeSection objSection = objCharacter.AttributeSection;
                            if (objSection != null)
                            {
                                objSection.PropertyChangedAsync -= OnAttributeSectionChanged;
                                ThreadSafeObservableCollection<CharacterAttrib> lstAttributes = objSection.Attributes;
                                if (lstAttributes?.IsDisposed == false)
                                    lstAttributes.CollectionChangedAsync -= OnAttributesCollectionChanged;
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            //swallow this
                        }
                    }
                    CharacterSettings objSettings = objCharacter.Settings;
                    if (objSettings?.IsDisposed == false)
                    {
                        try
                        {
                            objSettings.MultiplePropertiesChangedAsync -= OnCharacterSettingsPropertyChanged;
                        }
                        catch (ObjectDisposedException)
                        {
                            //swallow this
                        }
                    }
                }

                CharacterAttrib objAttribute = AttributeObject; // for thread safety
                if (objAttribute?.IsDisposed == false)
                {
                    try
                    {
                        objAttribute.MultiplePropertiesChangedAsync -= OnLinkedAttributeChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                }

                SkillGroup objGroup = SkillGroupObject;
                if (objGroup?.IsDisposed == false)
                {
                    try
                    {
                        objGroup.MultiplePropertiesChangedAsync -= OnSkillGroupChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                }

                using (_lstSpecializations.LockObject.EnterWriteLock())
                {
                    _lstSpecializations.CollectionChangedAsync -= SpecializationsOnCollectionChanged;
                    _lstSpecializations.BeforeClearCollectionChangedAsync -= SpecializationsOnBeforeClearCollectionChanged;
                    _lstSpecializations.ForEach(x => x.Dispose());
                }

                _lstSpecializations.Dispose();
                _objCachedCyberwareRatingLock.Dispose();
                _objCachedTotalBaseRatingLock.Dispose();
                _objCachedLearnedRatingLock.Dispose();
                _objCachedPoolModifiersLock.Dispose();
                _objCachedRatingModifiersLock.Dispose();
                _objCachedSuggestedSpecializationsLock.Dispose();
                if (_lstCachedSuggestedSpecializations != null)
                    Utils.ListItemListPool.Return(ref _lstCachedSuggestedSpecializations);
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
            if (disposing && Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) == 0)
            {
                Character objCharacter = CharacterObject; // for thread safety
                if (objCharacter != null)
                {
                    if (!objCharacter.IsDisposed)
                    {
                        try
                        {
                            objCharacter.MultiplePropertiesChangedAsync -= OnCharacterChanged;
                            AttributeSection objSection = await objCharacter.GetAttributeSectionAsync().ConfigureAwait(false);
                            if (objSection != null)
                            {
                                objSection.PropertyChangedAsync -= OnAttributeSectionChanged;
                                ThreadSafeObservableCollection<CharacterAttrib> lstAttributes = await objSection.GetAttributesAsync().ConfigureAwait(false);
                                if (lstAttributes?.IsDisposed == false)
                                    lstAttributes.CollectionChangedAsync -= OnAttributesCollectionChanged;
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            //swallow this
                        }
                    }
                    CharacterSettings objSettings = await objCharacter.GetSettingsAsync().ConfigureAwait(false);
                    if (objSettings?.IsDisposed == false)
                    {
                        try
                        {
                            objSettings.MultiplePropertiesChangedAsync -= OnCharacterSettingsPropertyChanged;
                        }
                        catch (ObjectDisposedException)
                        {
                            //swallow this
                        }
                    }
                }

                CharacterAttrib objAttribute = AttributeObject; // for thread safety
                if (objAttribute?.IsDisposed == false)
                {
                    try
                    {
                        objAttribute.MultiplePropertiesChangedAsync -= OnLinkedAttributeChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                }

                SkillGroup objGroup = SkillGroupObject;
                if (objGroup?.IsDisposed == false)
                {
                    try
                    {
                        objGroup.MultiplePropertiesChangedAsync -= OnSkillGroupChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                }

                IAsyncDisposable objLocker = await _lstSpecializations.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    _lstSpecializations.CollectionChangedAsync -= SpecializationsOnCollectionChanged;
                    _lstSpecializations.BeforeClearCollectionChangedAsync -= SpecializationsOnBeforeClearCollectionChanged;
                    await _lstSpecializations.ForEachWithSideEffectsAsync(async x => await x.DisposeAsync().ConfigureAwait(false)).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
                await _lstSpecializations.DisposeAsync().ConfigureAwait(false);
                await _objCachedCyberwareRatingLock.DisposeAsync().ConfigureAwait(false);
                await _objCachedTotalBaseRatingLock.DisposeAsync().ConfigureAwait(false);
                await _objCachedLearnedRatingLock.DisposeAsync().ConfigureAwait(false);
                await _objCachedPoolModifiersLock.DisposeAsync().ConfigureAwait(false);
                await _objCachedRatingModifiersLock.DisposeAsync().ConfigureAwait(false);
                await _objCachedSuggestedSpecializationsLock.DisposeAsync().ConfigureAwait(false);
                if (_lstCachedSuggestedSpecializations != null)
                    Utils.ListItemListPool.Return(ref _lstCachedSuggestedSpecializations);
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                await DisposeAsync(true).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            GC.SuppressFinalize(this);
        }
    }
}
