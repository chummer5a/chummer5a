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
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using Chummer.Annotations;
using Chummer.Backend.Equipment;
using Chummer.Backend.Attributes;
using System.Drawing;

namespace Chummer.Backend.Skills
{
    [DebuggerDisplay("{_strName} {_intBase} {_intKarma} {Rating}")]
    [HubClassTag("SkillId", true, "Name", "Rating;Specialization")]
    public partial class Skill : INotifyMultiplePropertyChanged, IHasName, IHasXmlNode, IHasNotes
    {
        private CharacterAttrib _objAttribute;
        private string _strDefaultAttribute;
        private bool _blnCheckSwapSkillImprovements = true;
        private bool _blnRequiresGroundMovement;
        private bool _blnRequiresSwimMovement;
        private bool _blnRequiresFlyMovement;

        public CharacterAttrib AttributeObject
        {
            get
            {
                if (!_blnCheckSwapSkillImprovements && _objAttribute != null) return _objAttribute;
                if (CharacterObject.Improvements.Any(imp =>
                        imp.ImproveType == Improvement.ImprovementType.SwapSkillAttribute && imp.Target == Name))
                {
                    AttributeObject = CharacterObject.GetAttribute(CharacterObject.Improvements.First(imp =>
                            imp.ImproveType == Improvement.ImprovementType.SwapSkillAttribute &&
                            imp.Target == Name).ImprovedName);
                }
                else if (_strDefaultAttribute != _objAttribute?.Abbrev || _objAttribute == null)
                {
                    AttributeObject = CharacterObject.GetAttribute(_strDefaultAttribute);
                }

                return _objAttribute;
            }
            protected set
            {
                if (_objAttribute == value) return;
                if (_objAttribute != null)
                    _objAttribute.PropertyChanged -= OnLinkedAttributeChanged;
                if (value != null)
                    value.PropertyChanged += OnLinkedAttributeChanged;
                _objAttribute = value;
                _strDefaultAttribute = _objAttribute?.Abbrev;
                OnPropertyChanged();
            }
        } //Attribute this skill primarily depends on

        private string _strName = string.Empty; //English name of this skill
        private string _strNotes = string.Empty; //Text of any notes that were entered by the user
        public List<ListItem> SuggestedSpecializations { get; } = new List<ListItem>(10); //List of suggested specializations for this skill
        private bool _blnDefault;

        public virtual void WriteTo(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("skill");
            objWriter.WriteElementString("guid", Id.ToString("D", GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("suid", SkillId.ToString("D", GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("isknowledge", bool.FalseString);
            objWriter.WriteElementString("skillcategory", SkillCategory);
            objWriter.WriteElementString("requiresgroundmovement", RequiresGroundMovement.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("requiresswimmovement", RequiresSwimMovement.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("requiresflymovement", RequiresFlyMovement.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("karma", _intKarma.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("base", _intBase.ToString(GlobalOptions.InvariantCultureInfo)); //this could actually be saved in karma too during career
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
            objWriter.WriteElementString("name", _strName);
            if (!CharacterObject.Created)
            {
                objWriter.WriteElementString("buywithkarma", BuyWithKarma.ToString(GlobalOptions.InvariantCultureInfo));
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

            objWriter.WriteEndElement();

        }

        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("skill");

            int intPool = Pool;
            int intSpecPool = intPool + GetSpecializationBonus();

            int intRatingModifiers = RatingModifiers(Attribute);
            int intDicePoolModifiers = PoolModifiers(Attribute);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("suid", SkillId.ToString("D", GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("name", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("skillgroup", SkillGroupObject?.DisplayName(strLanguageToPrint) ?? LanguageManager.GetString("String_None", strLanguageToPrint));
            objWriter.WriteElementString("skillgroup_english", SkillGroupObject?.Name ?? LanguageManager.GetString("String_None", strLanguageToPrint));
            objWriter.WriteElementString("skillcategory", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("skillcategory_english", SkillCategory);  //Might exist legacy but not existing atm, will see if stuff breaks
            objWriter.WriteElementString("grouped", (SkillGroupObject != null && SkillGroupObject.CareerIncrease && SkillGroupObject.Rating > 0).ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("default", Default.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("requiresgroundmovement", RequiresGroundMovement.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("requiresswimmovement", RequiresSwimMovement.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("requiresflymovement", RequiresFlyMovement.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("rating", Rating.ToString(objCulture));
            objWriter.WriteElementString("ratingmax", RatingMaximum.ToString(objCulture));
            objWriter.WriteElementString("specializedrating", intSpecPool.ToString(objCulture));
            objWriter.WriteElementString("total", intPool.ToString(objCulture));
            objWriter.WriteElementString("knowledge", IsKnowledgeSkill.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("exotic", IsExoticSkill.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("buywithkarma", BuyWithKarma.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("base", Base.ToString(objCulture));
            objWriter.WriteElementString("karma", Karma.ToString(objCulture));
            objWriter.WriteElementString("spec", DisplaySpecialization(strLanguageToPrint));
            objWriter.WriteElementString("attribute", Attribute);
            objWriter.WriteElementString("displayattribute", DisplayAttributeMethod(strLanguageToPrint));
            if (CharacterObject.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteElementString("attributemod", CharacterObject.GetAttribute(Attribute).TotalValue.ToString(objCulture));
            objWriter.WriteElementString("ratingmod", (intRatingModifiers + intDicePoolModifiers).ToString(objCulture));
            objWriter.WriteElementString("poolmod", intDicePoolModifiers.ToString(objCulture));
            objWriter.WriteElementString("islanguage", IsLanguage.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("isnativelanguage", IsNativeLanguage.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("bp", CurrentKarmaCost.ToString(objCulture));
            objWriter.WriteStartElement("skillspecializations");
            foreach (SkillSpecialization objSpec in Specializations)
            {
                objSpec.Print(objWriter, objCulture, strLanguageToPrint);
            }
            objWriter.WriteEndElement();

            objWriter.WriteEndElement();
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
            XmlDocument xmlSkills = XmlManager.Load("skills.xml");
            Skill objLoadingSkill = null;
            bool blnIsKnowledgeSkill = false;
            if (xmlSkillNode.TryGetBoolFieldQuickly("isknowledge", ref blnIsKnowledgeSkill) && blnIsKnowledgeSkill)
            {
                if (xmlSkillNode["forced"] != null)
                    objLoadingSkill = new KnowledgeSkill(objCharacter, xmlSkillNode["name"]?.InnerText ?? string.Empty, !Convert.ToBoolean(xmlSkillNode["disableupgrades"]?.InnerText, GlobalOptions.InvariantCultureInfo));
                else
                {
                    KnowledgeSkill knoSkill = new KnowledgeSkill(objCharacter);
                    knoSkill.Load(xmlSkillNode);
                    objLoadingSkill = knoSkill;
                }
            }
            else if (suid != Guid.Empty)
            {
                XmlNode xmlSkillDataNode = xmlSkills.SelectSingleNode("/chummer/skills/skill[id = '" + xmlSkillNode["suid"]?.InnerText + "']");

                if (xmlSkillDataNode == null)
                    return null;

                if (xmlSkillDataNode["exotic"]?.InnerText == bool.TrueString)
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
                    objLoadingSkill = new KnowledgeSkill(objCharacter, xmlSkillNode["name"]?.InnerText ?? string.Empty, !Convert.ToBoolean(xmlSkillNode["disableupgrades"]?.InnerText, GlobalOptions.InvariantCultureInfo));
                else
                {
                    KnowledgeSkill knoSkill = new KnowledgeSkill(objCharacter);
                    knoSkill.Load(xmlSkillNode);
                    objLoadingSkill = knoSkill;
                }
            }
            if (xmlSkillNode.TryGetField("guid", Guid.TryParse, out Guid guiTemp))
                objLoadingSkill.Id = guiTemp;

            if (!xmlSkillNode.TryGetMultiLineStringFieldQuickly("altnotes", ref objLoadingSkill._strNotes))
                xmlSkillNode.TryGetMultiLineStringFieldQuickly("notes", ref objLoadingSkill._strNotes);

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
                        objLoadingSkill.Specializations.Add(SkillSpecialization.Load(xmlSpec));
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

            int.TryParse(xmlSkillNode["base"]?.InnerText, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out int intBaseRating);
            int.TryParse(xmlSkillNode["rating"]?.InnerText, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out int intFullRating);
            int intKarmaRating = intFullRating - intBaseRating;  //Not reading karma directly as career only increases rating

            bool blnTemp = false;

            string strName = xmlSkillNode["name"]?.InnerText ?? string.Empty;
            Skill objSkill;
            if (xmlSkillNode.TryGetBoolFieldQuickly("knowledge", ref blnTemp) && blnTemp)
            {
                KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(objCharacter)
                {
                    WriteableName = strName,
                    Base = intBaseRating,
                    Karma = intKarmaRating,

                    Type = xmlSkillNode["skillcategory"]?.InnerText
                };

                objSkill = objKnowledgeSkill;
            }
            else
            {
                XmlDocument xmlSkillsDocument = XmlManager.Load("skills.xml");
                XmlNode xmlSkillDataNode = xmlSkillsDocument
                                               .SelectSingleNode("/chummer/skills/skill[id = '"
                                                                 + suid.ToString("D", GlobalOptions.InvariantCultureInfo)
                                                                 + "']")
                    //Some stuff apparently have a guid of 0000-000... (only exotic?)
                    ?? xmlSkillsDocument.SelectSingleNode("/chummer/skills/skill[name = " + strName.CleanXPath() + ']');


                objSkill = FromData(xmlSkillDataNode, objCharacter);
                objSkill._intBase = intBaseRating;
                objSkill._intKarma = intKarmaRating;

                if (objSkill is ExoticSkill objExoticSkill)
                {
                    //don't need to do more load then.
                    objExoticSkill.Specific = xmlSkillNode.SelectSingleNode("skillspecializations/skillspecialization/name")?.InnerText ?? string.Empty;
                    return objSkill;
                }

                xmlSkillNode.TryGetBoolFieldQuickly("buywithkarma", ref objSkill._blnBuyWithKarma);
            }

            using (XmlNodeList xmlSpecList = xmlSkillNode.SelectNodes("skillspecializations/skillspecialization"))
            {
                if (xmlSpecList != null)
                {
                    foreach (XmlNode xmlSpecializationNode in xmlSpecList)
                    {
                        objSkill.Specializations.Add(SkillSpecialization.Load(xmlSpecializationNode));
                    }
                }
            }

            return objSkill;
        }

        public static Skill LoadFromHeroLab(Character objCharacter, XmlNode xmlSkillNode, bool blnIsKnowledgeSkill, string strSkillType = "")
        {
            if (xmlSkillNode == null)
                throw new ArgumentNullException(nameof(xmlSkillNode));
            string strName = xmlSkillNode.Attributes?["name"]?.InnerText ?? string.Empty;

            XmlNode xmlSkillDataNode = XmlManager.Load("skills.xml")
                .SelectSingleNode((blnIsKnowledgeSkill
                    ? "/chummer/knowledgeskills/skill[name = "
                    : "/chummer/skills/skill[name = ") + strName.CleanXPath() + ']');
            Guid suid = Guid.NewGuid();
            if (xmlSkillDataNode?.TryGetField("id", Guid.TryParse, out suid) != true)
                suid = Guid.NewGuid();

            int intKarmaRating = 0;
            if (xmlSkillNode.Attributes?["text"]?.InnerText != "N")      // Native Languages will have a base + karma rating of 0
                if (!int.TryParse(xmlSkillNode.Attributes?["base"]?.InnerText, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out intKarmaRating)) // Only reading karma rating out for now, any base rating will need modification within SkillsSection
                    intKarmaRating = 0;

            Skill objSkill;
            if (blnIsKnowledgeSkill)
            {
                KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(objCharacter)
                {
                    WriteableName = strName,
                    Karma = intKarmaRating,
                    Type = !string.IsNullOrEmpty(strSkillType) ? strSkillType : (xmlSkillDataNode?["category"]?.InnerText ?? "Academic")
                };

                objSkill = objKnowledgeSkill;
            }
            else
            {
                objSkill = FromData(xmlSkillDataNode, objCharacter);
                if (xmlSkillNode.Attributes?["fromgroup"]?.InnerText == "yes")
                {
                    intKarmaRating -= objSkill.SkillGroupObject.Karma;
                }
                objSkill._intKarma = intKarmaRating;

                if (objSkill is ExoticSkill objExoticSkill)
                {
                    string strSpecializationName = xmlSkillNode.SelectSingleNode("specialization/@bonustext")?.InnerText ?? string.Empty;
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

            List<SkillSpecialization> lstSpecializations;
            using (XmlNodeList xmlSpecList = xmlSkillNode.SelectNodes("specialization"))
            {
                lstSpecializations = new List<SkillSpecialization>(xmlSpecList?.Count ?? 0);
                if (xmlSpecList?.Count > 0)
                {
                    foreach (XmlNode xmlSpecializationNode in xmlSpecList)
                    {
                        string strSpecializationName = xmlSpecializationNode.Attributes?["bonustext"]?.InnerText;
                        if (string.IsNullOrEmpty(strSpecializationName)) continue;
                        int intLastPlus = strSpecializationName.LastIndexOf('+');
                        if (intLastPlus > strSpecializationName.Length)
                            strSpecializationName = strSpecializationName.Substring(0, intLastPlus - 1);
                        lstSpecializations.Add(new SkillSpecialization(strSpecializationName));
                    }
                }
            }
            if (lstSpecializations.Count > 0)
            {
                objSkill.Specializations.AddRange(lstSpecializations);
            }

            return objSkill;
        }

        protected static readonly Dictionary<string, bool> SkillTypeCache = new Dictionary<string, bool>();
        //TODO CACHE INVALIDATE

        /// <summary>
        /// Load a skill from a data file describing said skill
        /// </summary>
        /// <param name="xmlNode">The XML node describing the skill</param>
        /// <param name="character">The character the skill belongs to</param>
        /// <returns></returns>
        public static Skill FromData(XmlNode xmlNode, Character character)
        {
            if (xmlNode == null)
                return null;
            Skill objSkill;
            if (xmlNode["exotic"]?.InnerText == bool.TrueString)
            {
                //load exotic skill
                ExoticSkill objExoticSkill = new ExoticSkill(character, xmlNode);
                objSkill = objExoticSkill;
            }
            else
            {
                string category = xmlNode["category"]?.InnerText;
                if (string.IsNullOrEmpty(category))
                    return null;
                if (SkillTypeCache == null || !SkillTypeCache.TryGetValue(category, out bool blnIsKnowledgeSkill))
                {
                    blnIsKnowledgeSkill = XmlManager.Load("skills.xml")
                        .SelectSingleNode("/chummer/categories/category[. = '" + category + "']/@type")?.InnerText != "active";
                    if (SkillTypeCache != null)
                        SkillTypeCache[category] = blnIsKnowledgeSkill;
                }


                if (blnIsKnowledgeSkill)
                {
                    //TODO INIT SKILL
                    Utils.BreakIfDebug();

                    objSkill = new KnowledgeSkill(character);
                }
                else
                {
                    //TODO INIT SKILL

                    objSkill = new Skill(character, xmlNode);
                }
            }


            return objSkill;
        }

        protected Skill(Character character)
        {
            CharacterObject = character ?? throw new ArgumentNullException(nameof(character));
            CharacterObject.PropertyChanged += OnCharacterChanged;
            CharacterObject.AttributeSection.PropertyChanged += OnAttributeSectionChanged;
            CharacterObject.AttributeSection.Attributes.CollectionChanged += OnAttributesCollectionChanged;
            Specializations.ListChanged += SpecializationsOnListChanged;
            Specializations.BeforeRemove += SpecializationsOnBeforeRemove;
        }

        private void OnAttributesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        if (e.NewItems.Cast<CharacterAttrib>().Any(x => x.Abbrev == Attribute))
                        {
                            AttributeObject.PropertyChanged -= AttributeActiveOnPropertyChanged;
                            AttributeObject = CharacterObject.GetAttribute(Attribute);

                            AttributeObject.PropertyChanged += AttributeActiveOnPropertyChanged;
                            AttributeActiveOnPropertyChanged(sender, null);
                        }
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        if (e.OldItems.Cast<CharacterAttrib>().Any(x => x.Abbrev == Attribute))
                        {
                            AttributeObject.PropertyChanged -= AttributeActiveOnPropertyChanged;
                            AttributeObject = CharacterObject.GetAttribute(Attribute);

                            AttributeObject.PropertyChanged += AttributeActiveOnPropertyChanged;
                            AttributeActiveOnPropertyChanged(sender, null);
                        }
                        break;
                    }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    {
                        AttributeObject.PropertyChanged -= AttributeActiveOnPropertyChanged;
                        AttributeObject = CharacterObject.GetAttribute(Attribute);

                        AttributeObject.PropertyChanged += AttributeActiveOnPropertyChanged;
                        AttributeActiveOnPropertyChanged(sender, null);
                        break;
                    }
            }
        }

        private void OnAttributeSectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AttributeSection.AttributeCategory)) return;
            AttributeObject.PropertyChanged -= AttributeActiveOnPropertyChanged;
            AttributeObject = CharacterObject.GetAttribute(Attribute);

            AttributeObject.PropertyChanged += AttributeActiveOnPropertyChanged;
            AttributeActiveOnPropertyChanged(sender, e);
        }

        private void AttributeActiveOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            OnPropertyChanged(nameof(Rating));
        }

        public void UnbindSkill()
        {
            CharacterObject.PropertyChanged -= OnCharacterChanged;
            CharacterObject.AttributeSection.PropertyChanged -= OnAttributeSectionChanged;
            CharacterObject.AttributeSection.Attributes.CollectionChanged -= OnAttributesCollectionChanged;

            if (SkillGroupObject != null)
                SkillGroupObject.PropertyChanged -= OnSkillGroupChanged;
        }

        //load from data
        protected Skill(Character character, XmlNode xmlNode) : this(character)
        //Ugly hack, needs by then
        {
            if (xmlNode == null)
                return;
            _strName = xmlNode["name"]?.InnerText; //No need to catch errors (for now), if missing we are fsked anyway
            _strDefaultAttribute = xmlNode["attribute"]?.InnerText;
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

            bool blnTemp = false;
            if (xmlNode.TryGetBoolFieldQuickly("requiresgroundmovement", ref blnTemp))
                RequiresGroundMovement = blnTemp;
            else
                RequiresGroundMovement = GetNode()?["requiresgroundmovement"]?.InnerText == bool.TrueString;
            if (xmlNode.TryGetBoolFieldQuickly("requiresswimmovement", ref blnTemp))
                RequiresSwimMovement = blnTemp;
            else
                RequiresSwimMovement = GetNode()?["requiresswimmovement"]?.InnerText == bool.TrueString;
            if (xmlNode.TryGetBoolFieldQuickly("requiresflymovement", ref blnTemp))
                RequiresFlyMovement = blnTemp;
            else
                RequiresFlyMovement = GetNode()?["requiresflymovement"]?.InnerText == bool.TrueString;

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

            XmlNodeList xmlSpecList = xmlNode.SelectNodes("specs/spec");

            if (xmlSpecList != null)
            {
                SuggestedSpecializations.Capacity = xmlSpecList.Count;

                foreach (XmlNode xmlSpecNode in xmlSpecList)
                {
                    string strInnerText = xmlSpecNode.InnerText;
                    SuggestedSpecializations.Add(new ListItem(strInnerText, xmlSpecNode.Attributes?["translate"]?.InnerText ?? strInnerText));
                }

                SuggestedSpecializations.Sort(CompareListItems.CompareNames);
            }
        }

        public void ReloadSuggestedSpecializations()
        {
            SuggestedSpecializations.Clear();

            XmlNodeList xmlSpecList = GetNode()?.SelectNodes("specs/spec");

            if (xmlSpecList != null)
            {
                SuggestedSpecializations.Capacity = xmlSpecList.Count;

                foreach (XmlNode xmlSpecNode in xmlSpecList)
                {
                    string strInnerText = xmlSpecNode.InnerText;
                    SuggestedSpecializations.Add(new ListItem(strInnerText, xmlSpecNode.Attributes?["translate"]?.InnerText ?? strInnerText));
                }

                SuggestedSpecializations.Sort(CompareListItems.CompareNames);
            }
            OnPropertyChanged(nameof(SuggestedSpecializations));
        }
        #endregion

        /// <summary>
        /// The total, general purpose dice pool for this skill
        /// </summary>
        public int Pool => IsNativeLanguage ? int.MaxValue : PoolOtherAttribute(Attribute);

        public bool Leveled => Rating > 0;

        public Color PreferredControlColor => Leveled && Enabled ? ColorManager.Control : ColorManager.ControlLighter;

        private int _intCachedCanHaveSpecs = -1;

        public bool CanHaveSpecs
        {
            get
            {
                if ((this as KnowledgeSkill)?.AllowUpgrade == false)
                    return false;
                if (_intCachedCanHaveSpecs >= 0)
                    return _intCachedCanHaveSpecs > 0;
                if (!Enabled)
                {
                    _intCachedCanHaveSpecs = 0;
                    return _intCachedCanHaveSpecs > 0;
                }
                _intCachedCanHaveSpecs = !IsExoticSkill && TotalBaseRating > 0 && KarmaUnlocked &&
                                         !CharacterObject.Improvements.Any(x => ((x.ImproveType == Improvement.ImprovementType.BlockSkillSpecializations && (string.IsNullOrEmpty(x.ImprovedName) || x.ImprovedName == Name)) ||
                                                                                 (x.ImproveType == Improvement.ImprovementType.BlockSkillCategorySpecializations && x.ImprovedName == SkillCategory)) && x.Enabled) ? 1 : 0;
                if (_intCachedCanHaveSpecs <= 0 && Specializations.Count > 0)
                {
                    Specializations.Clear();
                }

                return _intCachedCanHaveSpecs > 0;
            }
        }

        public Character CharacterObject { get; }

        //TODO change to the actual characterattribute object
        /// <summary>
        /// The Abbreviation of the linked attribute. Not the object due legacy
        /// </summary>
        public string Attribute => AttributeObject.Abbrev;

        /// <summary>
        /// The translated abbreviation of the linked attribute.
        /// </summary>
        public string DisplayAttributeMethod(string strLanguage)
        {
            return LanguageManager.GetString("String_Attribute" + Attribute +  "Short", strLanguage);
        }

        public string DisplayAttribute => DisplayAttributeMethod(GlobalOptions.Language);

        private int _intCachedEnabled = -1;

        private bool _blnForceDisabled;
        //TODO handle aspected/adepts who cannot (always) get magic skills
        public bool Enabled
        {
            get
            {
                if (_intCachedEnabled >= 0)
                    return _intCachedEnabled > 0;

                if (_blnForceDisabled)
                {
                    _intCachedEnabled = 0;
                    return false;
                }
                // SR5 400 : Critters that don't have the Sapience Power are unable to default in skills they don't possess.
                if (CharacterObject.IsCritter && Rating == 0 && !CharacterObject.Improvements.Any(x =>
                        x.ImproveType == Improvement.ImprovementType.AllowSkillDefault &&
                        (x.ImprovedName == Name || string.IsNullOrEmpty(x.ImprovedName)) && x.Enabled))
                {
                    _intCachedEnabled = 0;
                    return false;
                }

                if (CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.SkillDisable && x.ImprovedName == Name && x.Enabled))
                {
                    _intCachedEnabled = 0;
                    return false;
                }
                if (RequiresFlyMovement)
                {
                    string strMovementString = CharacterObject.GetFly(GlobalOptions.InvariantCultureInfo, GlobalOptions.DefaultLanguage);
                    if (string.IsNullOrEmpty(strMovementString)
                        || strMovementString == "0"
                        || strMovementString.Contains(LanguageManager.GetString("String_ModeSpecial", GlobalOptions.DefaultLanguage)))
                    {
                        _intCachedEnabled = 0;
                        return false;
                    }
                }
                if (RequiresSwimMovement)
                {
                    string strMovementString = CharacterObject.GetSwim(GlobalOptions.InvariantCultureInfo, GlobalOptions.DefaultLanguage);
                    if (string.IsNullOrEmpty(strMovementString)
                        || strMovementString == "0"
                        || strMovementString.Contains(LanguageManager.GetString("String_ModeSpecial", GlobalOptions.DefaultLanguage)))
                    {
                        _intCachedEnabled = 0;
                        return false;
                    }
                }
                if (RequiresGroundMovement)
                {
                    string strMovementString = CharacterObject.GetMovement(GlobalOptions.InvariantCultureInfo, GlobalOptions.DefaultLanguage);
                    if (string.IsNullOrEmpty(strMovementString)
                        || strMovementString == "0"
                        || strMovementString.Contains(LanguageManager.GetString("String_ModeSpecial", GlobalOptions.DefaultLanguage)))
                    {
                        _intCachedEnabled = 0;
                        return false;
                    }
                }
                //TODO: This is a temporary workaround until proper support for selectively enabling or disabling skills works, as above.
                if (CharacterObject.IsAI)
                {
                    _intCachedEnabled = Attribute != "MAG" && Attribute != "MAGAdept" && Attribute != "RES" ? 1 : 0;
                }
                else
                {
                    _intCachedEnabled = AttributeObject.Value != 0 ? 1 : 0;
                }

                return _intCachedEnabled > 0;
            }
        }

        public bool ForceDisabled
        {
            get => _blnForceDisabled;
            set
            {
                if (_blnForceDisabled == value) return;
                _blnForceDisabled = value;
                OnPropertyChanged();
            }
        }

        public bool RequiresGroundMovement
        {
            get => _blnRequiresGroundMovement;
            set
            {
                if (_blnRequiresGroundMovement == value) return;
                _blnRequiresGroundMovement = value;
                OnPropertyChanged();
            }
        }

        public bool RequiresSwimMovement
        {
            get => _blnRequiresSwimMovement;
            set
            {
                if (_blnRequiresSwimMovement == value) return;
                _blnRequiresSwimMovement = value;
                OnPropertyChanged();
            }
        }

        public bool RequiresFlyMovement
        {
            get => _blnRequiresFlyMovement;
            set
            {
                if (_blnRequiresFlyMovement == value) return;
                _blnRequiresFlyMovement = value;
                OnPropertyChanged();
            }
        }

        private int _intCachedCanUpgradeCareer = -1;

        public bool CanUpgradeCareer
        {
            get
            {
                if (_intCachedCanUpgradeCareer < 0)
                    _intCachedCanUpgradeCareer = CharacterObject.Karma >= UpgradeKarmaCost && RatingMaximum > TotalBaseRating ? 1 : 0;

                return _intCachedCanUpgradeCareer == 1;
            }
        }

        public virtual bool AllowDelete => false;

        public bool Default
        {
            get
            {
                return _blnDefault && !RelevantImprovements(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.BlockSkillDefault, string.Empty, false, true).Any();
            }
            set
            {
                if (_blnDefault == value) return;
                _blnDefault = value;
                OnPropertyChanged();
            }
        }

        public virtual bool IsExoticSkill => false;

        public virtual bool IsKnowledgeSkill => false;

        public virtual bool AllowNameChange => false;

        public virtual bool AllowTypeChange => false;

        public virtual bool IsLanguage => false;

        public virtual bool IsNativeLanguage
        {
            get => false;
            set
            {
                // Dummy setter that is only set up so that Language skills can have a setter that is functional
            }
        }

        private string _strDictionaryKey;
        public string DictionaryKey => _strDictionaryKey = _strDictionaryKey
                                                           ?? (IsExoticSkill
                                                               ? Name + " (" + DisplaySpecialization(GlobalOptions.DefaultLanguage) + ')'
                                                               : Name);

        public string Name
        {
            get => _strName;
            set
            {
                if (value == _strName) return;
                _strName = value;
                _strDictionaryKey = null;
                _intCachedFreeBase = int.MinValue;
                _intCachedFreeKarma = int.MinValue;
                OnPropertyChanged();
            }
        } //I

        //TODO RENAME DESCRIPTIVE
        private Guid _guidInternalId = Guid.NewGuid();
        /// <summary>
        /// The Unique ID for this skill. This is unique and never repeating
        /// </summary>
        public Guid Id
        {
            get => _guidInternalId;
            private set => _guidInternalId = value;
        }
        public string InternalId => _guidInternalId.ToString("D", GlobalOptions.InvariantCultureInfo);

        private Guid _guidSkillId = Guid.Empty;
        /// <summary>
        /// The ID for this skill. This is persistent for active skills over
        /// multiple characters, ?and predefined knowledge skills,? but not
        /// for skills where the user supplies a name (Exotic and Knowledge)
        /// </summary>
        public Guid SkillId
        {
            get => _guidSkillId;
            set
            {
                if (_guidSkillId == value) return;
                _guidSkillId = value;
                _objCachedMyXmlNode = null;
                ReloadSuggestedSpecializations();
            }
        }

        public string SkillGroup { get; } = string.Empty;

        public virtual string SkillCategory { get; } = string.Empty;

        private List<ListItem> _lstCachedSuggestedSpecializations;

        // ReSharper disable once InconsistentNaming
        public IReadOnlyList<ListItem> CGLSpecializations
        {
            get
            {
                if (_lstCachedSuggestedSpecializations == null)
                {
                    _lstCachedSuggestedSpecializations = new List<ListItem>(SuggestedSpecializations);
                    foreach (Improvement objImprovement in CharacterObject.Improvements.Where(x =>
                        x.ImprovedName == Name && x.ImproveType == Improvement.ImprovementType.SkillSpecializationOption &&
                        _lstCachedSuggestedSpecializations.All(y => y.Value?.ToString() != x.UniqueName) && x.Enabled))
                    {
                        string strSpecializationName = objImprovement.UniqueName;
                        _lstCachedSuggestedSpecializations.Add(new ListItem(strSpecializationName, LanguageManager.TranslateExtra(strSpecializationName)));
                    }
                }

                return _lstCachedSuggestedSpecializations;
            }
        }

        private readonly Dictionary<string, string> _cachedStringSpec = new Dictionary<string, string>();
        public virtual string DisplaySpecialization(string strLanguage)
        {
            if (_cachedStringSpec.TryGetValue(strLanguage, out string strReturn)) return strReturn;
            strReturn = string.Join(", ", Specializations.Select(x => x.DisplayName(strLanguage)));

            _cachedStringSpec.Add(strLanguage, strReturn);

            return strReturn;
        }

        public string CurrentDisplaySpecialization => DisplaySpecialization(GlobalOptions.Language);

        //TODO A unit test here?, I know we don't have them, but this would be improved by some
        //Or just ignore support for multiple specializations even if the rules say it is possible?
        public CachedBindingList<SkillSpecialization> Specializations { get; } = new CachedBindingList<SkillSpecialization>();

        public string Specialization
        {
            get
            {
                if (TotalBaseRating == 0)
                {
                    return string.Empty; //Unlevelled skills cannot have a specialization;
                }

                if (IsExoticSkill)
                {
                    return ((ExoticSkill) this).Specific;
                }

                return Specializations.Count > 0 ? Specializations[0].CurrentDisplayName : string.Empty;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    int index = -1;
                    for (int i = 0; i < Specializations.Count; i++)
                    {
                        if (Specializations[i].Free) continue;
                        index = i;
                        break;
                    }

                    if (index >= 0) Specializations.RemoveAt(index);
                }
                else if (Specializations.Count == 0)
                {
                    Specializations.Add(new SkillSpecialization(value));
                }
                else if (Specializations[0].Free)
                {
                    Specializations.AddWithSort(new SkillSpecialization(value), (x, y) =>
                    {
                        if (x.Free == y.Free)
                        {
                            if (x.Expertise == y.Expertise)
                                return 0;
                            return x.Expertise ? 1 : -1;
                        }
                        return x.Free ? 1 : -1;
                    });
                }
                else
                {
                    Specializations[0] = new SkillSpecialization(value);
                }
            }
        }

        public bool HasSpecialization(string strSpecialization)
        {
            if (IsExoticSkill)
            {
                return ((ExoticSkill)this).Specific == strSpecialization;
            }
            return Specializations.Any(x => x.Name == strSpecialization || x.CurrentDisplayName == strSpecialization)
                   && !CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && x.ImprovedName == Name && string.IsNullOrEmpty(x.Condition) && x.Enabled);
        }

        public SkillSpecialization GetSpecialization(string strSpecialization)
        {

            if (IsExoticSkill && ((ExoticSkill)this).Specific == strSpecialization)
            {
                return Specializations[0];
            }
            return HasSpecialization(strSpecialization)
                ? Specializations.FirstOrDefault(x => x.Name == strSpecialization || x.CurrentDisplayName == strSpecialization)
                : null;
        }

        public string PoolToolTip => IsNativeLanguage ? LanguageManager.GetString("Tip_Skill_NativeLanguage") : CompileDicepoolTooltip(Attribute);

        public string CompileDicepoolTooltip(string abbrev = "")
        {
            CharacterAttrib att = CharacterObject.AttributeSection.GetAttributeByName(abbrev);
            if (!Default && !Leveled)
            {
                return LanguageManager.GetString("Tip_Skill_Cannot_Default");
            }

            string strSpace = LanguageManager.GetString("String_Space");
            List<Improvement> lstRelevantImprovements = RelevantImprovements(null, abbrev, true).ToList();
            StringBuilder s = new StringBuilder();
            if (CyberwareRating > TotalBaseRating)
            {
                s.Append(LanguageManager.GetString("Tip_Skill_SkillsoftRating"))
                    .Append(strSpace).Append('(').Append(CyberwareRating.ToString(GlobalOptions.CultureInfo)).Append(')');
            }
            else
            {
                s.Append(LanguageManager.GetString("Tip_Skill_SkillRating"))
                    .Append(strSpace).Append('(').Append(Rating.ToString(GlobalOptions.CultureInfo));
                bool first = true;
                foreach (Improvement objImprovement in lstRelevantImprovements)
                {
                    if (!objImprovement.AddToRating)
                        continue;
                    if (first)
                    {
                        first = false;
                        s.Append(strSpace).Append("(Base").Append(strSpace).Append('(')
                            .Append(LearnedRating.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }

                    s.Append(strSpace).Append('+').Append(strSpace)
                        .Append(CharacterObject.GetObjectName(objImprovement))
                        .Append(strSpace).Append('(')
                        .Append(objImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                if (!first)
                    s.Append(')');
                s.Append(')');
            }

            s.Append(strSpace).Append('+')
                .Append(strSpace).Append(att.DisplayAbbrev)
                .Append(strSpace).Append('(').Append(att.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');
            if (Default && !Leveled)
            {
                s.Append(strSpace);
                int intDefaultModifier = DefaultModifier;
                if (intDefaultModifier == 0)
                    s.Append(CharacterObject.GetObjectName(
                        CharacterObject.Improvements.FirstOrDefault(x =>
                            x.ImproveType == Improvement.ImprovementType.ReflexRecorderOptimization && x.Enabled)));
                else
                    s.Append((intDefaultModifier > 0 ? '+' : '-'))
                        .Append(strSpace).Append(LanguageManager.GetString("Tip_Skill_Defaulting"))
                        .Append(strSpace).Append('(').Append(Math.Abs(intDefaultModifier).ToString(GlobalOptions.CultureInfo)).Append(')');
            }

            foreach (Improvement source in lstRelevantImprovements)
            {
                if (source.AddToRating
                    || source.ImproveType == Improvement.ImprovementType.SwapSkillAttribute
                    || source.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute)
                    continue;
                s.Append(strSpace).Append('+').Append(strSpace).Append(CharacterObject.GetObjectName(source));
                if (!string.IsNullOrEmpty(source.Condition))
                {
                    s.Append(strSpace).Append('(')
                        .Append(source.Condition.ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                s.Append(strSpace).Append('(').Append(source.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
            }

            int wound = CharacterObject.WoundModifier;
            if (wound != 0)
            {
                s.Append(strSpace).Append('-').Append(strSpace).Append(LanguageManager.GetString("Tip_Skill_Wounds"))
                    .Append(strSpace).Append('(').Append(wound.ToString(GlobalOptions.CultureInfo)).Append(')');
            }

            if (att.Abbrev == "STR" || att.Abbrev == "AGI")
            {
                foreach (Cyberware cyberware in CharacterObject.Cyberware)
                {
                    if (!cyberware.Name.Contains(" Arm") && !cyberware.Name.Contains(" Hand"))
                        continue;
                    s.AppendLine().Append(cyberware.Location)
                        .Append(strSpace).Append(cyberware.DisplayNameShort(GlobalOptions.Language));
                    if (cyberware.Grade.Name != "Standard")
                    {
                        s.Append(strSpace).Append('(').Append(cyberware.Grade.CurrentDisplayName).Append(')');
                    }

                    int pool = PoolOtherAttribute(att.Abbrev, false, att.Abbrev == "STR" ? cyberware.TotalStrength : cyberware.TotalAgility);
                    if (cyberware.Location == CharacterObject.PrimaryArm
                        || CharacterObject.Ambidextrous
                        || cyberware.LimbSlotCount > 1)
                    {
                        s.Append(strSpace).Append(pool.ToString(GlobalOptions.CultureInfo));
                    }
                    else
                    {
                        s.Append(strSpace).AppendFormat(GlobalOptions.CultureInfo, "{0}{1}({2}{1}{3})",
                            pool - 2, strSpace, -2, LanguageManager.GetString("Tip_Skill_OffHand"));
                    }
                }
            }

            if (att.Abbrev != Attribute)
                return s.ToString();
            foreach (Improvement objSwapSkillAttribute in lstRelevantImprovements)
            {
                if (objSwapSkillAttribute.ImproveType != Improvement.ImprovementType.SwapSkillAttribute
                    && objSwapSkillAttribute.ImproveType != Improvement.ImprovementType.SwapSkillSpecAttribute)
                    continue;
                s.AppendLine();
                if (objSwapSkillAttribute.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute)
                    s.Append(objSwapSkillAttribute.Exclude).Append(LanguageManager.GetString("String_Colon")).Append(strSpace);
                s.Append(CharacterObject.GetObjectName(objSwapSkillAttribute)).Append(strSpace);
                int intBasePool = PoolOtherAttribute(objSwapSkillAttribute.ImprovedName, false, CharacterObject.GetAttribute(objSwapSkillAttribute.ImprovedName).Value);
                SkillSpecialization objSpecialization = null;
                if (objSwapSkillAttribute.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute)
                {
                    objSpecialization = Specializations.FirstOrDefault(y =>
                        y.Name == objSwapSkillAttribute.Exclude && !CharacterObject.Improvements.Any(objImprovement =>
                            objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects &&
                            objImprovement.ImprovedName == y.Name && string.IsNullOrEmpty(objImprovement.Condition) &&
                            objImprovement.Enabled));
                    if (objSpecialization != null)
                    {
                        intBasePool += objSpecialization.SpecializationBonus;
                    }
                }

                s.Append(intBasePool.ToString(GlobalOptions.CultureInfo));
                if (objSwapSkillAttribute.ImprovedName != "STR" &&
                    objSwapSkillAttribute.ImprovedName != "AGI") continue;
                foreach (Cyberware cyberware in CharacterObject.Cyberware)
                {
                    if (!cyberware.Name.Contains(" Arm") && !cyberware.Name.Contains(" Hand"))
                        continue;
                    s.AppendLine();
                    if (objSwapSkillAttribute.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute)
                        s.AppendFormat(GlobalOptions.CultureInfo, "{0}{1}{2}", objSwapSkillAttribute.Exclude, LanguageManager.GetString("String_Colon"), strSpace);
                    s.Append(CharacterObject.GetObjectName(objSwapSkillAttribute))
                        .Append(strSpace).Append(cyberware.Location)
                        .Append(strSpace).Append(cyberware.CurrentDisplayNameShort);
                    if (cyberware.Grade.Name != "Standard")
                    {
                        s.Append(strSpace).Append('(').Append(cyberware.Grade.CurrentDisplayName).Append(')');
                    }

                    int intLoopPool =
                        PoolOtherAttribute(objSwapSkillAttribute.ImprovedName, false,
                            objSwapSkillAttribute.ImprovedName == "STR"
                                ? cyberware.TotalStrength
                                : cyberware.TotalAgility);
                    if (objSpecialization != null)
                    {
                        intLoopPool += objSpecialization.SpecializationBonus;
                    }

                    if (cyberware.Location == CharacterObject.PrimaryArm
                        || CharacterObject.Ambidextrous
                        || cyberware.LimbSlotCount > 1)
                    {
                        s.Append(strSpace).Append(intLoopPool.ToString(GlobalOptions.CultureInfo));
                    }
                    else
                    {
                        s.Append(strSpace).AppendFormat(GlobalOptions.CultureInfo, "{0}{1}({2}{1}{3})",
                            intLoopPool - 2, strSpace, -2, LanguageManager.GetString("Tip_Skill_OffHand"));
                    }
                }
            }

            return s.ToString();
        }

        public string UpgradeToolTip => UpgradeKarmaCost < 0
            ? LanguageManager.GetString("Tip_ImproveItemAtMaximum")
            : string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Tip_ImproveItem"), (Rating + 1), UpgradeKarmaCost);

        public string AddSpecToolTip
        {
            get
            {
                int intPrice = IsKnowledgeSkill ? CharacterObject.Options.KarmaKnowledgeSpecialization : CharacterObject.Options.KarmaSpecialization;

                decimal decExtraSpecCost = 0;
                int intTotalBaseRating = TotalBaseRating;
                decimal decSpecCostMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if (objLoopImprovement.Minimum > intTotalBaseRating ||
                        (!string.IsNullOrEmpty(objLoopImprovement.Condition) &&
                         (objLoopImprovement.Condition == "career") != CharacterObject.Created &&
                         (objLoopImprovement.Condition == "create") == CharacterObject.Created) ||
                        !objLoopImprovement.Enabled) continue;
                    if (objLoopImprovement.ImprovedName != SkillCategory) continue;
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
                return string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Tip_Skill_AddSpecialization"), intPrice);
            }
        }

        public string HtmlSkillToolTip => SkillToolTip.CleanForHTML();

        public string SkillToolTip
        {
            get
            {
                //v-- hack i guess
                StringBuilder sbdReturn = new StringBuilder();
                StringBuilder sbdMiddle = new StringBuilder();
                string strSpace = LanguageManager.GetString("String_Space");
                if (!string.IsNullOrWhiteSpace(SkillGroup))
                {
                    sbdMiddle.Append(SkillGroupObject.CurrentDisplayName)
                        .Append(strSpace).AppendLine(LanguageManager.GetString("String_ExpenseSkillGroup"));
                }
                if (!string.IsNullOrEmpty(Notes))
                {
                    sbdReturn.Append(LanguageManager.GetString("Label_Notes"))
                        .Append(strSpace).AppendLine(Notes).AppendLine();
                }

                sbdReturn.AppendLine(DisplayCategory(GlobalOptions.Language))
                    .Append(sbdMiddle).Append(CommonFunctions.LanguageBookLong(Source))
                    .Append(strSpace).Append(LanguageManager.GetString("String_Page"))
                    .Append(strSpace).Append(DisplayPage(GlobalOptions.Language));
                return sbdReturn.ToString();
            }
        }

        public string Notes
        {
            get => _strNotes;
            set
            {
                if (_strNotes == value) return;
                _strNotes = value;
                OnPropertyChanged();
            }
        }

        public Color PreferredColor =>
            !string.IsNullOrEmpty(Notes)
                ? ColorManager.HasNotesColor
                : ColorManager.ControlText;

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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Page;
            string s = GetNode(strLanguage)?["altpage"]?.InnerText ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        public string Source { get; }

        //Stuff that is RO, that is simply calculated from other things
        //Move to extension method?

        #region Calculations

        public int AttributeModifiers => AttributeObject.TotalValue;

        public string DisplayName(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        public string CurrentDisplayName => DisplayName(GlobalOptions.Language);

        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return SkillCategory;

            string strReturn = XmlManager.Load("skills.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + SkillCategory + "\"]/@translate")?.InnerText;

            return strReturn ?? SkillCategory;
        }

        public virtual string DisplayPool => DisplayOtherAttribute(Attribute);

        public string DisplayOtherAttribute(string strAttribute)
        {
            int intPool = PoolOtherAttribute(strAttribute);
            if ((IsExoticSkill || string.IsNullOrWhiteSpace(Specialization) || CharacterObject.Improvements.Any(x =>
                     x.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects &&
                     x.ImprovedName == Name && string.IsNullOrEmpty(x.Condition) && x.Enabled)) &&
                 !CharacterObject.Improvements.Any(i =>
                     i.ImproveType == Improvement.ImprovementType.Skill && !string.IsNullOrEmpty(i.Condition)))
            {
                return intPool.ToString(GlobalOptions.CultureInfo);
            }

            int intConditionalBonus = PoolOtherAttribute(strAttribute, true);
            int intSpecBonus = GetSpecializationBonus();
            if (intSpecBonus == 0 && intPool == intConditionalBonus)
                return intPool.ToString(GlobalOptions.CultureInfo);
            return string.Format(GlobalOptions.CultureInfo, "{0}{1}({2})",
                intPool, LanguageManager.GetString("String_Space"), intSpecBonus + intConditionalBonus);
        }

        public int GetSpecializationBonus(string strSpecialization = "")
        {
            if (IsExoticSkill || TotalBaseRating == 0 || Specializations.Count <= 0)
                return 0;
            SkillSpecialization objTargetSpecialization = string.IsNullOrEmpty(strSpecialization)
                ? Specializations.FirstOrDefault(y => CharacterObject.Improvements.All(x => x.ImproveType != Improvement.ImprovementType.DisableSpecializationEffects
                                                                                            || x.ImprovedName != Name
                                                                                            || !string.IsNullOrEmpty(x.Condition)
                                                                                            || !x.Enabled))
                : GetSpecialization(strSpecialization);
            if (objTargetSpecialization == null)
                return 0;
            return objTargetSpecialization.SpecializationBonus;
        }

        // ReSharper disable once InconsistentNaming
        private protected int _intCachedCyberwareRating = int.MinValue;

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = XmlManager.Load("skills.xml", strLanguage)
                    .SelectSingleNode(string.Format(GlobalOptions.InvariantCultureInfo,
                        IsKnowledgeSkill
                            ? "/chummer/knowledgeskills/skill[id = \"{0}\" or id = \"{1}\"]"
                            : "/chummer/skills/skill[id = \"{0}\" or id = \"{1}\"]",
                        SkillId.ToString("D", GlobalOptions.InvariantCultureInfo),
                        SkillId.ToString("D", GlobalOptions.InvariantCultureInfo).ToUpperInvariant()));
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        /// <summary>
        /// The attributeValue this skill have from Skillwires + Skilljack or Active Hardwires
        /// </summary>
        /// <returns>Artificial skill attributeValue</returns>
        public virtual int CyberwareRating
        {
            get
            {
                if (_intCachedCyberwareRating != int.MinValue)
                    return _intCachedCyberwareRating;

                ExoticSkill objThisAsExoticSkill = this as ExoticSkill;
                //TODO: method is here, but not used in any form, needs testing (worried about child items...)
                //this might do hardwires if i understand how they works correctly
                int intMaxHardwire = -1;
                foreach (Improvement objImprovement in CharacterObject.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.Hardwire && objImprovement.Enabled)
                    {
                        if (objThisAsExoticSkill != null)
                        {
                            if (objImprovement.ImprovedName == Name + " (" + objThisAsExoticSkill.Specific + ')')
                                intMaxHardwire = Math.Max(intMaxHardwire, objImprovement.Value.StandardRound());
                        }
                        else if (objImprovement.ImprovedName == Name)
                            intMaxHardwire = Math.Max(intMaxHardwire, objImprovement.Value.StandardRound());
                    }
                }
                if (intMaxHardwire >= 0)
                {
                    return _intCachedCyberwareRating = intMaxHardwire;
                }

                int intMaxActivesoftRating = Math.Min(ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.Skillwire), ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillsoftAccess)).StandardRound();
                if (intMaxActivesoftRating > 0)
                {
                    int intMax = 0;
                    //TODO this works with translate?
                    foreach (Improvement objSkillsoftImprovement in CharacterObject.Improvements)
                    {
                        if (objSkillsoftImprovement.ImproveType == Improvement.ImprovementType.Activesoft && objSkillsoftImprovement.Enabled)
                        {
                            if (objThisAsExoticSkill != null)
                            {
                                if (objSkillsoftImprovement.ImprovedName == Name + " (" + objThisAsExoticSkill.Specific + ')')
                                    intMaxHardwire = Math.Max(intMaxHardwire, objSkillsoftImprovement.Value.StandardRound());
                            }
                            else if (objSkillsoftImprovement.ImprovedName == Name)
                                intMax = Math.Max(intMax, objSkillsoftImprovement.Value.StandardRound());
                        }
                    }
                    return _intCachedCyberwareRating = Math.Min(intMax, intMaxActivesoftRating);
                }

                return _intCachedCyberwareRating = 0;
            }
        }
        #endregion

        #region Static

        //A tree of dependencies. Once some of the properties are changed,
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly DependencyGraph<string, Skill> s_SkillDependencyGraph =
            new DependencyGraph<string, Skill>(
                new DependencyGraphNode<string, Skill>(nameof(PoolToolTip),
                    new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage)),
                    new DependencyGraphNode<string, Skill>(nameof(AttributeModifiers),
                        new DependencyGraphNode<string, Skill>(nameof(AttributeObject),
                            new DependencyGraphNode<string, Skill>(nameof(RelevantImprovements)))),
                    new DependencyGraphNode<string, Skill>(nameof(DisplayPool),
                        new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage)),
                        new DependencyGraphNode<string, Skill>(nameof(DisplayOtherAttribute),
                            new DependencyGraphNode<string, Skill>(nameof(PoolOtherAttribute),
                                new DependencyGraphNode<string, Skill>(nameof(Enabled)),
                                new DependencyGraphNode<string, Skill>(nameof(Rating)),
                                new DependencyGraphNode<string, Skill>(nameof(GetSpecializationBonus),
                                    new DependencyGraphNode<string, Skill>(nameof(Specializations))
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
                                    new DependencyGraphNode<string, Skill>(nameof(Name))
                                )
                            ),
                            new DependencyGraphNode<string, Skill>(nameof(Name)),
                            new DependencyGraphNode<string, Skill>(nameof(IsExoticSkill)),
                            new DependencyGraphNode<string, Skill>(nameof(Specialization))
                        ),
                        new DependencyGraphNode<string, Skill>(nameof(Pool),
                            new DependencyGraphNode<string, Skill>(nameof(AttributeModifiers),
                                new DependencyGraphNode<string, Skill>(nameof(AttributeObject))
                            ),
                            new DependencyGraphNode<string, Skill>(nameof(PoolOtherAttribute)),
                            new DependencyGraphNode<string, Skill>(nameof(Attribute)),
                            new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage))
                        )
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(CanHaveSpecs),
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.AllowUpgrade), x => x.IsKnowledgeSkill,
                        new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage))
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
                                    new DependencyGraphNode<string, Skill>(nameof(Name))
                                ),
                                new DependencyGraphNode<string, Skill>(nameof(RatingMaximum),
                                    new DependencyGraphNode<string, Skill>(nameof(RelevantImprovements))
                                ),
                                new DependencyGraphNode<string, Skill>(nameof(KarmaPoints))
                            ),
                            new DependencyGraphNode<string, Skill>(nameof(Base),
                                new DependencyGraphNode<string, Skill>(nameof(FreeBase),
                                    new DependencyGraphNode<string, Skill>(nameof(Name))
                                ),
                                new DependencyGraphNode<string, Skill>(nameof(RatingMaximum)),
                                new DependencyGraphNode<string, Skill>(nameof(BasePoints))
                            )
                        )
                    ),
                    new DependencyGraphNode<string, Skill>(nameof(Leveled),
                        new DependencyGraphNode<string, Skill>(nameof(Rating),
                            new DependencyGraphNode<string, Skill>(nameof(CyberwareRating),
                                new DependencyGraphNode<string, Skill>(nameof(Name))
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
                        new DependencyGraphNode<string, Skill>(nameof(Specialization)),
                        new DependencyGraphNode<string, Skill>(nameof(KarmaPoints)),
                        new DependencyGraphNode<string, Skill>(nameof(BasePoints)),
                        new DependencyGraphNode<string, Skill>(nameof(FreeBase))
                    ),
                    new DependencyGraphNode<string, Skill>(nameof(ForcedNotBuyWithKarma),
                        new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating))
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(RelevantImprovements),
                    new DependencyGraphNode<string, Skill>(nameof(Name))
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
                new DependencyGraphNode<string, Skill>(nameof(HtmlSkillToolTip),
                    new DependencyGraphNode<string, Skill>(nameof(SkillToolTip),
                        new DependencyGraphNode<string, Skill>(nameof(Notes)),
                        new DependencyGraphNode<string, Skill>(nameof(DisplayCategory),
                            new DependencyGraphNode<string, Skill>(nameof(SkillCategory),
                                new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.Type), x => x.IsKnowledgeSkill)
                            )
                        )
                    )),
                new DependencyGraphNode<string, Skill>(nameof(PreferredControlColor),
                    new DependencyGraphNode<string, Skill>(nameof(Leveled)),
                    new DependencyGraphNode<string, Skill>(nameof(Enabled))
                ),
                new DependencyGraphNode<string, Skill>(nameof(PreferredColor),
                    new DependencyGraphNode<string, Skill>(nameof(Notes))
                ),
                new DependencyGraphNode<string, Skill>(nameof(CGLSpecializations),
                    new DependencyGraphNode<string, Skill>(nameof(SuggestedSpecializations))
                ),
                new DependencyGraphNode<string, Skill>(nameof(CurrentSpCost),
                    new DependencyGraphNode<string, Skill>(nameof(BasePoints)),
                    new DependencyGraphNode<string, Skill>(nameof(Specialization)),
                    new DependencyGraphNode<string, Skill>(nameof(BuyWithKarma))
                ),
                new DependencyGraphNode<string, Skill>(nameof(CurrentKarmaCost),
                    new DependencyGraphNode<string, Skill>(nameof(RangeCost)),
                    new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating)),
                    new DependencyGraphNode<string, Skill>(nameof(Base)),
                    new DependencyGraphNode<string, Skill>(nameof(FreeKarma)),
                    new DependencyGraphNode<string, Skill>(nameof(Specializations))
                ),
                new DependencyGraphNode<string, Skill>(nameof(CanUpgradeCareer),
                    new DependencyGraphNode<string, Skill>(nameof(UpgradeKarmaCost)),
                    new DependencyGraphNode<string, Skill>(nameof(RatingMaximum)),
                    new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating))
                ),
                new DependencyGraphNode<string, Skill>(nameof(Enabled),
                    new DependencyGraphNode<string, Skill>(nameof(ForceDisabled)),
                    new DependencyGraphNode<string, Skill>(nameof(Attribute)),
                    new DependencyGraphNode<string, Skill>(nameof(Name)),
                    new DependencyGraphNode<string, Skill>(nameof(RequiresGroundMovement)),
                    new DependencyGraphNode<string, Skill>(nameof(RequiresSwimMovement)),
                    new DependencyGraphNode<string, Skill>(nameof(RequiresFlyMovement))
                ),
                new DependencyGraphNode<string, Skill>(nameof(CurrentDisplaySpecialization),
                    new DependencyGraphNode<string, Skill>(nameof(DisplaySpecialization),
                        new DependencyGraphNode<string, Skill>(nameof(Specialization),
                            new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating)),
                            new DependencyGraphNode<string, Skill>(nameof(Specializations))
                        )
                    )
                ),
                new DependencyGraphNode<string, Skill>(nameof(CanAffordSpecialization),
                    new DependencyGraphNode<string, Skill>(nameof(CanHaveSpecs)),
                    new DependencyGraphNode<string, Skill>(nameof(TotalBaseRating))
                ),
                new DependencyGraphNode<string, Skill>(nameof(AllowDelete), x => x.IsKnowledgeSkill,
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.ForcedName)),
                    new DependencyGraphNode<string, Skill>(nameof(FreeBase)),
                    new DependencyGraphNode<string, Skill>(nameof(FreeKarma)),
                    new DependencyGraphNode<string, Skill>(nameof(RatingModifiers)),
                    new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage))
                ),
                new DependencyGraphNode<string, Skill>(nameof(DictionaryKey),
                    new DependencyGraphNode<string, Skill>(nameof(Name)),
                    new DependencyGraphNode<string, Skill>(nameof(IsExoticSkill)),
                    new DependencyGraphNode<string, Skill>(nameof(DisplaySpecialization), x => x.IsExoticSkill)
                ),
                new DependencyGraphNode<string, Skill>(nameof(IsLanguage), x => x.IsKnowledgeSkill,
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.Type))
                ),
                new DependencyGraphNode<string, Skill>(nameof(AllowNameChange), x => x.IsKnowledgeSkill,
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.ForcedName)),
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.AllowUpgrade)),
                    new DependencyGraphNode<string, Skill>(nameof(Karma)),
                    new DependencyGraphNode<string, Skill>(nameof(Base)),
                    new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage))
                ),
                new DependencyGraphNode<string, Skill>(nameof(AllowTypeChange), x => x.IsKnowledgeSkill,
                    new DependencyGraphNode<string, Skill>(nameof(AllowNameChange)),
                    new DependencyGraphNode<string, Skill>(nameof(KnowledgeSkill.Type)),
                    new DependencyGraphNode<string, Skill>(nameof(IsNativeLanguage))
                )
            );
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(params string[] lstPropertyNames)
        {
            ICollection<string> lstNamesOfChangedProperties = null;
            foreach (string strPropertyName in lstPropertyNames)
            {
                if (lstNamesOfChangedProperties == null)
                    lstNamesOfChangedProperties = s_SkillDependencyGraph.GetWithAllDependents(this, strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in s_SkillDependencyGraph.GetWithAllDependents(this, strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if (lstNamesOfChangedProperties == null || lstNamesOfChangedProperties.Count == 0)
                return;

            if (lstNamesOfChangedProperties.Contains(nameof(FreeBase)))
                _intCachedFreeBase = int.MinValue;
            if (lstNamesOfChangedProperties.Contains(nameof(FreeKarma)))
                _intCachedFreeKarma = int.MinValue;
            if (lstNamesOfChangedProperties.Contains(nameof(CanUpgradeCareer)))
                _intCachedCanUpgradeCareer = -1;
            if (lstNamesOfChangedProperties.Contains(nameof(CanAffordSpecialization)))
                _intCachedCanAffordSpecialization = -1;
            if (lstNamesOfChangedProperties.Contains(nameof(Enabled)))
                _intCachedEnabled = -1;
            if (lstNamesOfChangedProperties.Contains(nameof(CanHaveSpecs)))
                _intCachedCanHaveSpecs = -1;
            if (lstNamesOfChangedProperties.Contains(nameof(ForcedBuyWithKarma)))
                _intCachedForcedBuyWithKarma = -1;
            if (lstNamesOfChangedProperties.Contains(nameof(ForcedNotBuyWithKarma)))
                _intCachedForcedNotBuyWithKarma = -1;
            if (lstNamesOfChangedProperties.Contains(nameof(CyberwareRating)))
                _intCachedCyberwareRating = int.MinValue;
            if (lstNamesOfChangedProperties.Contains(nameof(CGLSpecializations)))
                _lstCachedSuggestedSpecializations = null;
            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }
        }

        private void OnSkillGroupChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Skills.SkillGroup.Base))
            {
                if (CharacterObject.BuildMethodHasSkillPoints)
                    OnMultiplePropertyChanged(nameof(Base),
                                              nameof(BaseUnlocked),
                                              nameof(ForcedBuyWithKarma));
                else
                    OnMultiplePropertyChanged(nameof(Base),
                                              nameof(ForcedBuyWithKarma));
            }
            else if (e.PropertyName == nameof(Skills.SkillGroup.Karma))
            {
                OnMultiplePropertyChanged(nameof(Karma),
                                          nameof(CurrentKarmaCost),
                                          nameof(ForcedBuyWithKarma),
                                          nameof(ForcedNotBuyWithKarma));
            }
            else if (e.PropertyName == nameof(Skills.SkillGroup.Rating))
            {
                if (CharacterObject.Options.StrictSkillGroupsInCreateMode && !CharacterObject.Created)
                {
                    OnPropertyChanged(nameof(KarmaUnlocked));
                }
            }
        }

        private void OnCharacterChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Character.Karma):
                    OnMultiplePropertyChanged(nameof(CanUpgradeCareer), nameof(CanAffordSpecialization));
                    break;
                case nameof(Character.WoundModifier):
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
                case nameof(Character.Improvements):
                {
                    //TODO: Dear god outbound improvements please this is is minimal an impact we can have and it's going to be a nightmare.
                    if (CharacterObject.Improvements.Any(i =>
                            i.ImproveType == Improvement.ImprovementType.SwapSkillAttribute && i.Target == "Name")
                        || _strDefaultAttribute != _objAttribute.Abbrev)
                        _blnCheckSwapSkillImprovements = true;
                    break;
                }
            }
        }

        protected void OnLinkedAttributeChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e?.PropertyName)
            {
                case nameof(CharacterAttrib.TotalValue):
                    OnPropertyChanged(nameof(AttributeModifiers));
                    break;
                case nameof(CharacterAttrib.Value):
                case nameof(CharacterAttrib.Abbrev):
                    OnPropertyChanged(nameof(Enabled));
                    break;
            }
        }

        private bool _blnSkipSpecializationRefresh;
        private void SpecializationsOnListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            if (_blnSkipSpecializationRefresh)
                return;
            _cachedStringSpec.Clear();
            if (IsExoticSkill)
                _strDictionaryKey = null;
            _blnSkipSpecializationRefresh = true; // Needed to make sure we don't call this method another time when we set the specialization's Parent
            if (listChangedEventArgs.ListChangedType == ListChangedType.ItemAdded
                || listChangedEventArgs.ListChangedType == ListChangedType.ItemChanged)
                Specializations[listChangedEventArgs.NewIndex].Parent = this;
            _blnSkipSpecializationRefresh = false;
            OnPropertyChanged(nameof(Specializations));
        }
        private void SpecializationsOnBeforeRemove(object sender, RemovingOldEventArgs e)
        {
            if (_blnSkipSpecializationRefresh)
                return;
            _blnSkipSpecializationRefresh = true; // Needed to make sure we don't call this method another time when we set the specialization's Parent
            Specializations[e.OldIndex].Parent = null;
            _blnSkipSpecializationRefresh = false;
        }
    }
}
