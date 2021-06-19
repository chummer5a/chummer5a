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
using System.Xml.XPath;

namespace Chummer.Backend.Skills
{
    [DebuggerDisplay("{_strName} {_intBase} {_intKarma} {Rating}")]
    [HubClassTag("SkillId", true, "Name", "Rating;Specialization")]
    public class Skill : INotifyMultiplePropertyChanged, IHasName, IHasXmlNode, IHasNotes
    {
        private CharacterAttrib _objAttribute;
        private string _strDefaultAttribute;
        private bool _blnCheckSwapSkillImprovements = true;
        private bool _blnRequiresGroundMovement;
        private bool _blnRequiresSwimMovement;
        private bool _blnRequiresFlyMovement;
        private int _intBase;
        private int _intKarma;
        private bool _blnBuyWithKarma;

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
                            imp.Target == DictionaryKey).ImprovedName);
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
        private Color _colNotes = ColorManager.HasNotesColor;
        public List<ListItem> SuggestedSpecializations { get; } = new List<ListItem>(10); //List of suggested specializations for this skill
        private bool _blnDefault;

        public void WriteTo(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("skill");
            objWriter.WriteElementString("guid", Id.ToString("D", GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("suid", SkillId.ToString("D", GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("isknowledge", IsKnowledgeSkill.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("skillcategory", SkillCategory);
            objWriter.WriteElementString("requiresgroundmovement", RequiresGroundMovement.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("requiresswimmovement", RequiresSwimMovement.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("requiresflymovement", RequiresFlyMovement.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("karma", KarmaPoints.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("base", BasePoints.ToString(GlobalOptions.InvariantCultureInfo)); //this could actually be saved in karma too during career
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(Notes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("name", Name);
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

            WriteToDerived(objWriter);

            objWriter.WriteEndElement();
        }

        public virtual void WriteToDerived(XmlTextWriter objWriter)
        {
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
            if (GlobalOptions.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteElementString("source", CharacterObject.LanguageBookShort(Source, strLanguageToPrint));
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
            XmlDocument xmlSkills = objCharacter.LoadData("skills.xml");
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

            String sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
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
                        objLoadingSkill.Specializations.Add(SkillSpecialization.Load(objCharacter, xmlSpec));
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
                        objSkill.Specializations.Add(SkillSpecialization.Load(objCharacter, xmlSpecializationNode));
                    }
                }
            }

            return objSkill;
        }

        public static Skill LoadFromHeroLab(Character objCharacter, XPathNavigator xmlSkillNode, bool blnIsKnowledgeSkill, string strSkillType = "")
        {
            if (xmlSkillNode == null)
                throw new ArgumentNullException(nameof(xmlSkillNode));
            string strName = xmlSkillNode.SelectSingleNode("@name")?.Value ?? string.Empty;

            XmlNode xmlSkillDataNode = objCharacter.LoadData("skills.xml")
                .SelectSingleNode((blnIsKnowledgeSkill
                    ? "/chummer/knowledgeskills/skill[name = "
                    : "/chummer/skills/skill[name = ") + strName.CleanXPath() + ']');
            Guid suid = Guid.Empty;
            if (xmlSkillDataNode?.TryGetField("id", Guid.TryParse, out suid) != true)
                suid = Guid.NewGuid();

            bool blnIsNativeLanguage = false;
            int intKarmaRating = 0;
            if (xmlSkillNode.SelectSingleNode("@text")?.Value == "N")
                blnIsNativeLanguage = true;
            else if (!int.TryParse(xmlSkillNode.SelectSingleNode("@base")?.Value, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out intKarmaRating)) // Only reading karma rating out for now, any base rating will need modification within SkillsSection
                intKarmaRating = 0;

            Skill objSkill;
            if (blnIsKnowledgeSkill)
            {
                KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(objCharacter)
                {
                    WritableName = strName,
                    Karma = intKarmaRating,
                    Type = !string.IsNullOrEmpty(strSkillType) ? strSkillType : (xmlSkillDataNode?["category"]?.InnerText ?? "Academic"),
                    IsNativeLanguage = blnIsNativeLanguage
                };

                objSkill = objKnowledgeSkill;
            }
            else
            {
                objSkill = FromData(xmlSkillDataNode, objCharacter);
                if (xmlSkillNode.SelectSingleNode("@fromgroup")?.Value == "yes")
                {
                    intKarmaRating -= objSkill.SkillGroupObject.Karma;
                }
                objSkill._intKarma = intKarmaRating;

                if (objSkill is ExoticSkill objExoticSkill)
                {
                    string strSpecializationName = xmlSkillNode.SelectSingleNode("specialization/@bonustext")?.Value ?? string.Empty;
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

            List<SkillSpecialization> lstSpecializations = new List<SkillSpecialization>();
            foreach (XPathNavigator xmlSpecializationNode in xmlSkillNode.Select("specialization"))
            {
                string strSpecializationName = xmlSpecializationNode.SelectSingleNode("@bonustext")?.Value;
                if (string.IsNullOrEmpty(strSpecializationName))
                    continue;
                int intLastPlus = strSpecializationName.LastIndexOf('+');
                if (intLastPlus > strSpecializationName.Length)
                    strSpecializationName = strSpecializationName.Substring(0, intLastPlus - 1);
                lstSpecializations.Add(new SkillSpecialization(objCharacter, strSpecializationName));
            }
            if (lstSpecializations.Count > 0)
            {
                objSkill.Specializations.AddRange(lstSpecializations);
            }

            return objSkill;
        }

        protected static Dictionary<string, bool> SkillTypeCache { get; } = new Dictionary<string, bool>();
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
                    blnIsKnowledgeSkill =
                        character.LoadDataXPath("skills.xml")
                            .SelectSingleNode("/chummer/categories/category[. = " + category.CleanXPath() + "]/@type")
                            ?.Value != "active";
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
            CharacterObject.Options.PropertyChanged += OnCharacterOptionsPropertyChanged;
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
            if (e.PropertyName != nameof(AttributeSection.AttributeCategory))
                return;
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
            CharacterObject.Options.PropertyChanged -= OnCharacterOptionsPropertyChanged;
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
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public int BasePoints
        {
            get => _intBase;
            set
            {
                if (_intBase != value)
                {
                    _intBase = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// How many points REALLY are in _karma Better than subclasses calculating Karma - FreeKarma()
        /// </summary>
        public int KarmaPoints
        {
            get => _intKarma;
            set
            {
                if (_intKarma != value)
                {
                    _intKarma = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Is it possible to place points in Base or is it prevented? (Build method or skill group)
        /// </summary>
        public bool BaseUnlocked => CharacterObject.EffectiveBuildMethodUsesPriorityTables
                                    && (SkillGroupObject == null
                                        || SkillGroupObject.Base <= 0
                                        || (CharacterObject.Options.UsePointsOnBrokenGroups
                                            && (!CharacterObject.Options.StrictSkillGroupsInCreateMode
                                                || CharacterObject.Created)));

        /// <summary>
        /// Is it possible to place points in Karma or is it prevented a stricter interpretation of the rules
        /// </summary>
        public bool KarmaUnlocked
        {
            get
            {
                if (CharacterObject.Options.StrictSkillGroupsInCreateMode && !CharacterObject.Created)
                {
                    return (SkillGroupObject == null || SkillGroupObject.Rating <= 0);
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
                if (SkillGroupObject?.Base > 0)
                {
                    if ((CharacterObject.Options.StrictSkillGroupsInCreateMode && !CharacterObject.Created)
                        || !CharacterObject.Options.UsePointsOnBrokenGroups)
                        BasePoints = 0;
                    return Math.Min(SkillGroupObject.Base + BasePoints + FreeBase, RatingMaximum);
                }

                return Math.Min(BasePoints + FreeBase, RatingMaximum);
            }
            set
            {
                if (SkillGroupObject?.Base > 0
                    && ((CharacterObject.Options.StrictSkillGroupsInCreateMode && !CharacterObject.Created)
                        || !CharacterObject.Options.UsePointsOnBrokenGroups))
                    return;

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


        /// <summary>
        /// Amount of skill points bought with karma and bonuses to the skills rating
        /// </summary>
        public int Karma
        {
            get
            {
                if (CharacterObject.Options.StrictSkillGroupsInCreateMode && !CharacterObject.Created && SkillGroupObject?.Karma > 0)
                {
                    _intKarma = 0;
                    Specializations.RemoveAll(x => !x.Free);
                    return SkillGroupObject.Karma;
                }
                return Math.Min(KarmaPoints + FreeKarma + (SkillGroupObject?.Karma ?? 0), RatingMaximum);
            }
            set
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

        /// <summary>
        /// Levels in this skill. Read only. You probably want to increase
        /// Karma instead
        /// </summary>
        public int Rating => Math.Max(CyberwareRating, TotalBaseRating);

        /// <summary>
        /// The rating the character has paid for, plus any improvement-based bonuses to skill rating.
        /// </summary>
        public int TotalBaseRating => LearnedRating + RatingModifiers(Attribute);

        /// <summary>
        /// The rating the character have actually paid for, not including skillwires
        /// or other overrides for skill Rating. Read only, you probably want to
        /// increase Karma instead.
        /// </summary>
        public int LearnedRating => Karma + Base;

        /// <summary>
        /// Is the specialization bought with karma. During career mode this is undefined
        /// </summary>
        public bool BuyWithKarma
        {
            get => (_blnBuyWithKarma || ForcedBuyWithKarma) && !ForcedNotBuyWithKarma && !string.IsNullOrEmpty(Specialization);
            set
            {
                bool blnNewValue = (value || ForcedBuyWithKarma) && !ForcedNotBuyWithKarma && !string.IsNullOrEmpty(Specialization);
                if (_blnBuyWithKarma != blnNewValue)
                {
                    _blnBuyWithKarma = blnNewValue;
                    OnPropertyChanged();
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
                int intOtherBonus = RelevantImprovements(x => x.ImproveType == Improvement.ImprovementType.Skill && x.Enabled).Sum(x => x.Maximum);
                if (CharacterObject.Created || CharacterObject.IgnoreRules)
                    return 12 + intOtherBonus;
                return (IsKnowledgeSkill && CharacterObject.EffectiveBuildMethodIsLifeModule ? 9 : 6) + intOtherBonus;
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
            if (!Enabled)
                return 0;
            int intRating = Rating;
            int intValue = intAttributeOverrideValue > int.MinValue
                ? intAttributeOverrideValue
                : CharacterObject.AttributeSection.GetAttributeByName(strAttribute).TotalValue;
            if (intValue <= 0)
                return 0;
            if (intRating > 0)
                return Math.Max(0, intRating + intValue + PoolModifiers(strAttribute, blnIncludeConditionals) + CharacterObject.WoundModifier + CharacterObject.SustainingPenalty);
            return Default
                ? Math.Max(0,
                    intValue + PoolModifiers(strAttribute, blnIncludeConditionals) + DefaultModifier +
                    CharacterObject.WoundModifier + CharacterObject.SustainingPenalty)
                : 0;
        }

        private static readonly Guid s_GuiReflexRecorderId = new Guid("17a6ba49-c21c-461b-9830-3beae8a237fc");

        public int DefaultModifier
        {
            get
            {
                if (SkillGroupObject != null && CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.ReflexRecorderOptimization && x.Enabled))
                {
                    HashSet<string> setSkillNames = SkillGroupObject.SkillList.Select(x => x.DictionaryKey).ToHashSet();
                    if (CharacterObject.Cyberware.Where(x => x.SourceID == s_GuiReflexRecorderId)
                        .Any(x => setSkillNames.Contains(x.Extra)))
                    {
                        return 0;
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// Things that modify the dicepool of the skill
        /// </summary>
        public int PoolModifiers(string strUseAttribute, bool blnIncludeConditionals = false) => Bonus(false, strUseAttribute, blnIncludeConditionals);

        /// <summary>
        /// Things that modify the dicepool of the skill
        /// </summary>
        public int RatingModifiers(string strUseAttribute, bool blnIncludeConditionals = false) => Bonus(true, strUseAttribute, blnIncludeConditionals);

        protected int Bonus(bool blnAddToRating, string strUseAttribute, bool blnIncludeConditionals = false)
        {
            //Some of this is not future proof. Rating that don't stack is not supported but i'm not aware of any cases where that will happen (for skills)
            return (RelevantImprovements(x => x.AddToRating == blnAddToRating, strUseAttribute, blnIncludeConditionals).Sum(x => x.Value)).StandardRound();
        }

        private IEnumerable<Improvement> RelevantImprovements(Func<Improvement, bool> funcWherePredicate = null, string strUseAttribute = "", bool blnIncludeConditionals = false, bool blnExistAfterFirst = false)
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
                    case Improvement.ImprovementType.Skill:
                    case Improvement.ImprovementType.SwapSkillAttribute:
                    case Improvement.ImprovementType.SwapSkillSpecAttribute:
                    case Improvement.ImprovementType.SkillDisable:
                        if (objImprovement.ImprovedName == strNameToUse)
                        {
                            yield return objImprovement;
                            if (blnExistAfterFirst)
                                yield break;
                        }
                        break;
                    case Improvement.ImprovementType.SkillGroup:
                    case Improvement.ImprovementType.SkillGroupDisable:
                        if (objImprovement.ImprovedName == SkillGroup && !objImprovement.Exclude.Contains(strNameToUse) && !objImprovement.Exclude.Contains(SkillCategory))
                        {
                            yield return objImprovement;
                            if (blnExistAfterFirst)
                                yield break;
                        }
                        break;
                    case Improvement.ImprovementType.SkillCategory:
                        if (objImprovement.ImprovedName == SkillCategory && !objImprovement.Exclude.Contains(strNameToUse))
                        {
                            yield return objImprovement;
                            if (blnExistAfterFirst)
                                yield break;
                        }
                        break;
                    case Improvement.ImprovementType.SkillAttribute:
                        if (objImprovement.ImprovedName == strUseAttribute && !objImprovement.Exclude.Contains(strNameToUse))
                        {
                            yield return objImprovement;
                            if (blnExistAfterFirst)
                                yield break;
                        }
                        break;
                    case Improvement.ImprovementType.SkillLinkedAttribute:
                        if (objImprovement.ImprovedName == Attribute && !objImprovement.Exclude.Contains(strNameToUse))
                        {
                            yield return objImprovement;
                            if (blnExistAfterFirst)
                                yield break;
                        }
                        break;
                    case Improvement.ImprovementType.BlockSkillDefault:
                        if (objImprovement.ImprovedName == SkillGroup)
                        {
                            yield return objImprovement;
                            if (blnExistAfterFirst)
                                yield break;
                        }
                        break;
                    case Improvement.ImprovementType.EnhancedArticulation:
                        if (SkillCategory == "Physical Active" && AttributeSection.PhysicalAttributes.Contains(Attribute))
                        {
                            yield return objImprovement;
                            if (blnExistAfterFirst)
                                yield break;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// How much Sp this costs. Price during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public virtual int CurrentSpCost
        {
            get
            {
                int cost = BasePoints + ((string.IsNullOrWhiteSpace(Specialization) || BuyWithKarma) ? 0 : 1);

                decimal decExtra = 0;
                decimal decMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if (objLoopImprovement.Minimum <= BasePoints &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == DictionaryKey || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillPointCost)
                                decExtra += objLoopImprovement.Value * (Math.Min(BasePoints, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillPointCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                        else if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryPointCost)
                                decExtra += objLoopImprovement.Value * (Math.Min(BasePoints, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryPointCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
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

        /// <summary>
        /// How much karma this costs. Return value during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public virtual int CurrentKarmaCost
        {
            get
            {
                //No rating can obv not cost anything
                //Makes debugging easier as we often only care about value calculation
                int intTotalBaseRating = TotalBaseRating;
                if (intTotalBaseRating == 0) return 0;

                int intCost;
                int intLower;
                if (SkillGroupObject?.Karma > 0)
                {
                    int intGroupUpper = SkillGroupObject.SkillList.Min(x => x.Base + x.Karma + x.RatingModifiers(x.Attribute));
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

                int intSpecCount = 0;
                foreach (SkillSpecialization objSpec in Specializations)
                {
                    if (!objSpec.Free && (BuyWithKarma || !CharacterObject.EffectiveBuildMethodUsesPriorityTables))
                        intSpecCount += 1;
                }
                int intSpecCost = intSpecCount * CharacterObject.Options.KarmaSpecialization;
                decimal decExtraSpecCost = 0;
                decimal decSpecCostMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if (objLoopImprovement.Minimum <= intTotalBaseRating &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName != SkillCategory)
                            continue;
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCost)
                            decExtraSpecCost += objLoopImprovement.Value * intSpecCount;
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier)
                            decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
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
                                         !CharacterObject.Improvements.Any(x =>
                                             ((x.ImproveType == Improvement.ImprovementType.BlockSkillSpecializations &&
                                               (string.IsNullOrEmpty(x.ImprovedName) || x.ImprovedName == DictionaryKey)) ||
                                              (x.ImproveType == Improvement.ImprovementType
                                                   .BlockSkillCategorySpecializations &&
                                               x.ImprovedName == SkillCategory)) && x.Enabled)
                    ? 1
                    : 0;
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
                        (x.ImprovedName == DictionaryKey || string.IsNullOrEmpty(x.ImprovedName)) && x.Enabled))
                {
                    _intCachedEnabled = 0;
                    return false;
                }

                if (CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.SkillDisable && x.ImprovedName == DictionaryKey && x.Enabled))
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
                switch (Attribute)
                {
                    case "MAG":
                    case "MAGAdept":
                        _intCachedEnabled = CharacterObject.MAGEnabled ? 1 : 0;
                        break;
                    case "RES":
                        _intCachedEnabled = CharacterObject.RESEnabled ? 1 : 0;
                        break;
                    case "DEP":
                        _intCachedEnabled = CharacterObject.DEPEnabled ? 1 : 0;
                        break;
                    default:
                        _intCachedEnabled = 1;
                        break;
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
            // ReSharper disable once ValueParameterNotUsed
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
                        x.ImprovedName == DictionaryKey && x.ImproveType == Improvement.ImprovementType.SkillSpecializationOption &&
                        _lstCachedSuggestedSpecializations.All(y => y.Value?.ToString() != x.UniqueName) && x.Enabled))
                    {
                        string strSpecializationName = objImprovement.UniqueName;
                        _lstCachedSuggestedSpecializations.Add(new ListItem(strSpecializationName, CharacterObject.TranslateExtra(strSpecializationName)));
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
                    Specializations.Add(new SkillSpecialization(CharacterObject, value));
                }
                else if (Specializations[0].Free)
                {
                    Specializations.AddWithSort(new SkillSpecialization(CharacterObject, value), (x, y) =>
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
                    Specializations[0] = new SkillSpecialization(CharacterObject, value);
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
                   && !CharacterObject.Improvements.Any(x =>
                       x.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects &&
                       x.ImprovedName == DictionaryKey && string.IsNullOrEmpty(x.Condition) && x.Enabled);
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
            if (!Default && !Leveled)
            {
                return LanguageManager.GetString("Tip_Skill_Cannot_Default");
            }

            CharacterAttrib att = CharacterObject.AttributeSection.GetAttributeByName(abbrev);

            if (att.TotalValue <= 0)
            {
                return string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Tip_Skill_Zero_Attribute"), att.DisplayNameShort(GlobalOptions.Language));
            }

            string strSpace = LanguageManager.GetString("String_Space");
            List<Improvement> lstRelevantImprovements = RelevantImprovements(null, abbrev, true).ToList();
            StringBuilder s = new StringBuilder();
            if (CyberwareRating > TotalBaseRating)
            {
                s.Append(LanguageManager.GetString("Tip_Skill_SkillsoftRating") + strSpace + '(' + CyberwareRating.ToString(GlobalOptions.CultureInfo) + ')');
            }
            else
            {
                s.Append(LanguageManager.GetString("Tip_Skill_SkillRating") + strSpace + '(' + Rating.ToString(GlobalOptions.CultureInfo));
                bool first = true;
                foreach (Improvement objImprovement in lstRelevantImprovements)
                {
                    if (!objImprovement.AddToRating)
                        continue;
                    if (first)
                    {
                        first = false;
                        s.Append(strSpace + "(Base" + strSpace + '(' + LearnedRating.ToString(GlobalOptions.CultureInfo) + ')');
                    }

                    s.Append(strSpace + '+' + strSpace + CharacterObject.GetObjectName(objImprovement) + strSpace + '(' + objImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                }

                if (!first)
                    s.Append(')');
                s.Append(')');
            }

            s.Append(strSpace + '+' + strSpace + att.DisplayAbbrev + strSpace + '(' + att.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

            if (Default && !Leveled)
            {
                s.Append(strSpace);
                int intDefaultModifier = DefaultModifier;
                if (intDefaultModifier == 0)
                    s.Append(CharacterObject.GetObjectName(
                        CharacterObject.Improvements.FirstOrDefault(x =>
                            x.ImproveType == Improvement.ImprovementType.ReflexRecorderOptimization && x.Enabled)));
                else
                    s.Append((intDefaultModifier > 0 ? '+' : '-') + strSpace + LanguageManager.GetString("Tip_Skill_Defaulting")
                             + strSpace + '(' + Math.Abs(intDefaultModifier).ToString(GlobalOptions.CultureInfo) + ')');
            }

            foreach (Improvement source in lstRelevantImprovements)
            {
                if (source.AddToRating
                    || source.ImproveType == Improvement.ImprovementType.SwapSkillAttribute
                    || source.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute)
                    continue;
                s.Append(strSpace + '+' + strSpace + CharacterObject.GetObjectName(source));
                if (!string.IsNullOrEmpty(source.Condition))
                {
                    s.Append(strSpace + '(' + source.Condition.ToString(GlobalOptions.CultureInfo) + ')');
                }
                s.Append(strSpace + '(' + source.Value.ToString(GlobalOptions.CultureInfo) + ')');
            }

            int wound = CharacterObject.WoundModifier;
            if (wound != 0)
            {
                s.Append(strSpace + '-' + strSpace + LanguageManager.GetString("Tip_Skill_Wounds") + strSpace + '(' + wound.ToString(GlobalOptions.CultureInfo) + ')');
            }

            int sustains = CharacterObject.SustainingPenalty;
            if (sustains != 0)
            {
                s.Append(strSpace).Append('-').Append(strSpace).Append(LanguageManager.GetString("Tip_Skill_Sustain"))
                    .Append(strSpace).Append('(').Append(sustains.ToString(GlobalOptions.CultureInfo)).Append(')');
            }

            if (att.Abbrev == "STR" || att.Abbrev == "AGI")
            {
                foreach (Cyberware cyberware in CharacterObject.Cyberware)
                {
                    if (!cyberware.Name.Contains(" Arm") && !cyberware.Name.Contains(" Hand"))
                        continue;
                    s.Append(Environment.NewLine + cyberware.Location + strSpace + cyberware.DisplayNameShort(GlobalOptions.Language));
                    if (cyberware.Grade.Name != "Standard")
                    {
                        s.Append(strSpace + '(' + cyberware.Grade.CurrentDisplayName + ')');
                    }

                    int pool = PoolOtherAttribute(att.Abbrev, false, att.Abbrev == "STR" ? cyberware.TotalStrength : cyberware.TotalAgility);
                    if (cyberware.Location == CharacterObject.PrimaryArm
                        || CharacterObject.Ambidextrous
                        || cyberware.LimbSlotCount > 1)
                    {
                        s.Append(strSpace + pool.ToString(GlobalOptions.CultureInfo));
                    }
                    else
                    {
                        s.AppendFormat(GlobalOptions.CultureInfo, "{1}{0}{1}({2}{1}{3})", pool - 2, strSpace, -2, LanguageManager.GetString("Tip_Skill_OffHand"));
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
                    s.Append(objSwapSkillAttribute.Exclude + LanguageManager.GetString("String_Colon") + strSpace);
                s.Append(CharacterObject.GetObjectName(objSwapSkillAttribute) + strSpace);
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
                        s.Append(objSwapSkillAttribute.Exclude + LanguageManager.GetString("String_Colon") + strSpace);
                    s.Append(CharacterObject.GetObjectName(objSwapSkillAttribute) + strSpace + cyberware.Location + strSpace + cyberware.CurrentDisplayNameShort);
                    if (cyberware.Grade.Name != "Standard")
                    {
                        s.Append(strSpace + '(' + cyberware.Grade.CurrentDisplayName + ')');
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
                        s.Append(strSpace + intLoopPool.ToString(GlobalOptions.CultureInfo));
                    }
                    else
                    {
                        s.AppendFormat(GlobalOptions.CultureInfo, "{1}{0}{1}({2}{1}{3})",
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
                string strSpace = LanguageManager.GetString("String_Space");
                string strReturn = !string.IsNullOrEmpty(Notes)
                    ? LanguageManager.GetString("Label_Notes") + strSpace + Notes + Environment.NewLine +
                      Environment.NewLine
                    : string.Empty;
                string strMiddle = !string.IsNullOrWhiteSpace(SkillGroup)
                    ? SkillGroupObject.CurrentDisplayName + strSpace +
                      LanguageManager.GetString("String_ExpenseSkillGroup") + Environment.NewLine
                    : string.Empty;
                strReturn += DisplayCategory(GlobalOptions.Language) + Environment.NewLine + strMiddle +
                             new SourceString(Source, Page, GlobalOptions.Language, GlobalOptions.CultureInfo,
                                 CharacterObject).LanguageBookTooltip;
                return strReturn;
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

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get => _colNotes;
            set => _colNotes = value;
        }

        public Color PreferredColor =>
            !string.IsNullOrEmpty(Notes)
                ? ColorManager.GenerateCurrentModeColor(NotesColor)
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
            if (strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
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
            if (strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        public string CurrentDisplayName => DisplayName(GlobalOptions.Language);

        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return SkillCategory;

            string strReturn = CharacterObject.LoadDataXPath("skills.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = " + SkillCategory.CleanXPath() + "]/@translate")?.Value;

            return strReturn ?? SkillCategory;
        }

        public virtual string DisplayPool => DisplayOtherAttribute(Attribute);

        public string DisplayOtherAttribute(string strAttribute)
        {

            int intPool = PoolOtherAttribute(strAttribute);
            if ((IsExoticSkill || string.IsNullOrWhiteSpace(Specialization) || CharacterObject.Improvements.Any(x =>
                     x.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects &&
                     x.ImprovedName == DictionaryKey && string.IsNullOrEmpty(x.Condition) && x.Enabled)) &&
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
                ? Specializations.FirstOrDefault(y => CharacterObject.Improvements.All(x =>
                    x.ImproveType != Improvement.ImprovementType.DisableSpecializationEffects
                    || x.ImprovedName != DictionaryKey
                    || !string.IsNullOrEmpty(x.Condition)
                    || !x.Enabled))
                : GetSpecialization(strSpecialization);
            return objTargetSpecialization?.SpecializationBonus ?? 0;
        }

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
                _objCachedMyXmlNode = CharacterObject.LoadData("skills.xml", strLanguage)
                    .SelectSingleNode(string.Format(GlobalOptions.InvariantCultureInfo,
                        IsKnowledgeSkill
                            ? "/chummer/knowledgeskills/skill[id = {0} or id = {1}]"
                            : "/chummer/skills/skill[id = {0} or id = {1}]",
                        SkillId.ToString("D", GlobalOptions.InvariantCultureInfo).CleanXPath(),
                        SkillId.ToString("D", GlobalOptions.InvariantCultureInfo).ToUpperInvariant().CleanXPath()));
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        // ReSharper disable once InconsistentNaming
        private int _intCachedCyberwareRating = int.MinValue;

        protected virtual void ResetCachedCyberwareRating()
        {
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
                if (_intCachedCyberwareRating != int.MinValue)
                    return _intCachedCyberwareRating;
                
                //TODO: method is here, but not used in any form, needs testing (worried about child items...)
                //this might do hardwires if i understand how they works correctly
                int intMaxHardwire = -1;
                foreach (Improvement objImprovement in CharacterObject.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.Hardwire && objImprovement.Enabled && objImprovement.ImprovedName == DictionaryKey)
                    {
                        intMaxHardwire = Math.Max(intMaxHardwire, objImprovement.Value.StandardRound());
                    }
                }
                if (intMaxHardwire >= 0)
                {
                    return _intCachedCyberwareRating = intMaxHardwire;
                }

                int intMaxActivesoftRating =
                    Math.Min(ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.Skillwire),
                            ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillsoftAccess))
                        .StandardRound();
                if (intMaxActivesoftRating > 0)
                {
                    int intMax = 0;
                    //TODO this works with translate?
                    foreach (Improvement objSkillsoftImprovement in CharacterObject.Improvements)
                    {
                        if (objSkillsoftImprovement.ImproveType == Improvement.ImprovementType.Activesoft && objSkillsoftImprovement.Enabled && objSkillsoftImprovement.ImprovedName == DictionaryKey)
                        {
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
                ResetCachedCyberwareRating();
            if (lstNamesOfChangedProperties.Contains(nameof(CGLSpecializations)))
                _lstCachedSuggestedSpecializations = null;
            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }
        }

        private void OnSkillGroupChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Skills.SkillGroup.Base) when CharacterObject.EffectiveBuildMethodUsesPriorityTables:
                    OnMultiplePropertyChanged(nameof(Base),
                        nameof(BaseUnlocked),
                        nameof(ForcedBuyWithKarma));
                    break;
                case nameof(Skills.SkillGroup.Base):
                    OnMultiplePropertyChanged(nameof(Base),
                        nameof(ForcedBuyWithKarma));
                    break;
                case nameof(Skills.SkillGroup.Karma):
                    OnMultiplePropertyChanged(nameof(Karma),
                        nameof(CurrentKarmaCost),
                        nameof(ForcedBuyWithKarma),
                        nameof(ForcedNotBuyWithKarma));
                    break;
                case nameof(Skills.SkillGroup.Rating):
                {
                    if (CharacterObject.Options.StrictSkillGroupsInCreateMode && !CharacterObject.Created)
                    {
                        OnPropertyChanged(nameof(KarmaUnlocked));
                    }

                    break;
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
                    OnMultiplePropertyChanged(nameof(Base),
                        nameof(BaseUnlocked),
                        nameof(ForcedBuyWithKarma));
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

        private void OnCharacterOptionsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CharacterOptions.StrictSkillGroupsInCreateMode):
                {
                    if (SkillGroupObject != null)
                    {
                        if (!CharacterObject.Created)
                        {
                            OnPropertyChanged(nameof(KarmaUnlocked));
                        }

                        OnMultiplePropertyChanged(nameof(BaseUnlocked), nameof(ForcedNotBuyWithKarma));
                    }
                    break;
                }
                case nameof(CharacterOptions.UsePointsOnBrokenGroups):
                {
                    if (SkillGroupObject != null)
                    {
                        OnPropertyChanged(nameof(BaseUnlocked));
                    }
                    break;
                }
                case nameof(CharacterOptions.KarmaNewKnowledgeSkill):
                case nameof(CharacterOptions.KarmaImproveKnowledgeSkill):
                {
                    if (IsKnowledgeSkill)
                    {
                        OnPropertyChanged(nameof(CurrentKarmaCost));
                    }
                    break;
                }
                case nameof(CharacterOptions.KarmaKnowledgeSpecialization):
                {
                    if (IsKnowledgeSkill)
                    {
                        OnMultiplePropertyChanged(nameof(CurrentKarmaCost), nameof(CanAffordSpecialization), nameof(AddSpecToolTip));
                    }
                    break;
                }
                case nameof(CharacterOptions.KarmaNewActiveSkill):
                case nameof(CharacterOptions.KarmaImproveActiveSkill):
                {
                    if (!IsKnowledgeSkill)
                    {
                        OnPropertyChanged(nameof(CurrentKarmaCost));
                    }
                    break;
                }
                case nameof(CharacterOptions.KarmaSpecialization):
                {
                    if (!IsKnowledgeSkill)
                    {
                        OnMultiplePropertyChanged(nameof(CurrentKarmaCost), nameof(CanAffordSpecialization), nameof(AddSpecToolTip));
                    }
                    break;
                }
                case nameof(CharacterOptions.CompensateSkillGroupKarmaDifference):
                {
                    if (SkillGroupObject != null)
                    {
                        OnPropertyChanged(nameof(CurrentKarmaCost));
                    }
                    break;
                }
                case nameof(CharacterOptions.KarmaNewSkillGroup):
                case nameof(CharacterOptions.KarmaImproveSkillGroup):
                {
                    if (SkillGroupObject != null && CharacterObject.Options.CompensateSkillGroupKarmaDifference)
                    {
                        OnPropertyChanged(nameof(CurrentKarmaCost));
                    }
                    break;
                }
                case nameof(CharacterOptions.SpecializationBonus):
                {
                    if (Specializations.Count > 0)
                    {
                        OnPropertyChanged(nameof(PoolOtherAttribute));
                    }
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

        /// <summary>
        /// Calculate the karma cost of increasing a skill from lower to upper
        /// </summary>
        /// <param name="lower">Staring rating of skill</param>
        /// <param name="upper">End rating of the skill</param>
        /// <returns></returns>
        protected int RangeCost(int lower, int upper)
        {
            if (lower >= upper)
                return 0;

            int intLevelsModded = upper * (upper + 1); //cost if nothing else was there
            intLevelsModded -= lower * (lower + 1); //remove "karma" costs from base + free

            intLevelsModded /= 2; //we get square, we need triangle

            int cost;
            if (lower == 0)
                cost = (intLevelsModded - 1) * CharacterObject.Options.KarmaImproveActiveSkill + CharacterObject.Options.KarmaNewActiveSkill;
            else
                cost = intLevelsModded * CharacterObject.Options.KarmaImproveActiveSkill;

            if (CharacterObject.Options.CompensateSkillGroupKarmaDifference)
            {
                SkillGroup objMySkillGroup = SkillGroupObject;
                if (objMySkillGroup != null)
                {
                    int intSkillGroupUpper = int.MaxValue;
                    foreach (Skill objSkillGroupMember in objMySkillGroup.SkillList)
                    {
                        if (objSkillGroupMember != this)
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
                        int intGroupLevelsModded = intSkillGroupUpper * (intSkillGroupUpper + 1); //cost if nothing else was there
                        intGroupLevelsModded -= lower * (lower + 1); //remove "karma" costs from base + free

                        intGroupLevelsModded /= 2; //we get square, we need triangle

                        int intGroupCost;
                        int intNakedSkillCost = objMySkillGroup.SkillList.Count;
                        if (lower == 0)
                        {
                            intGroupCost = (intGroupLevelsModded - 1) * CharacterObject.Options.KarmaImproveSkillGroup + CharacterObject.Options.KarmaNewSkillGroup;
                            intNakedSkillCost *= (intGroupLevelsModded - 1) * CharacterObject.Options.KarmaImproveActiveSkill + CharacterObject.Options.KarmaNewActiveSkill;
                        }
                        else
                        {
                            intGroupCost = intGroupLevelsModded * CharacterObject.Options.KarmaImproveSkillGroup;
                            intNakedSkillCost *= intGroupLevelsModded * CharacterObject.Options.KarmaImproveActiveSkill;
                        }

                        cost += (intGroupCost - intNakedSkillCost);
                    }
                }
            }

            decimal decMultiplier = 1.0m;
            decimal decExtra = 0;
            foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
            {
                if (objLoopImprovement.Minimum <= lower &&
                    (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                {
                    if (objLoopImprovement.ImprovedName == DictionaryKey || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillKarmaCost)
                            decExtra += objLoopImprovement.Value * (Math.Min(upper, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(lower, objLoopImprovement.Minimum - 1));
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                    else if (objLoopImprovement.ImprovedName == SkillCategory)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCost)
                            decExtra += objLoopImprovement.Value * (Math.Min(upper, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(lower, objLoopImprovement.Minimum - 1));
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
            }
            if (decMultiplier != 1.0m)
                cost = (cost * decMultiplier + decExtra).StandardRound();
            else
                cost += decExtra.StandardRound();

            if (cost < 0 && !CharacterObject.Options.CompensateSkillGroupKarmaDifference)
                cost = 0;
            return cost;
        }

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible
        /// </summary>
        /// <returns>Price in karma</returns>
        public virtual int UpgradeKarmaCost
        {
            get
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
                    intOptionsCost = CharacterObject.Options.KarmaNewActiveSkill;
                    upgrade += intOptionsCost;
                }
                else
                {
                    intOptionsCost = CharacterObject.Options.KarmaImproveActiveSkill;
                    upgrade += (intTotalBaseRating + 1) * intOptionsCost;
                }

                if (CharacterObject.Options.CompensateSkillGroupKarmaDifference)
                {
                    SkillGroup objMySkillGroup = SkillGroupObject;
                    if (objMySkillGroup != null)
                    {
                        int intSkillGroupUpper = int.MaxValue;
                        foreach (Skill objSkillGroupMember in objMySkillGroup.SkillList)
                        {
                            if (objSkillGroupMember != this)
                            {
                                int intLoopTotalBaseRating = objSkillGroupMember.TotalBaseRating;
                                if (intLoopTotalBaseRating < intSkillGroupUpper)
                                    intSkillGroupUpper = intLoopTotalBaseRating;
                            }
                        }
                        if (intSkillGroupUpper != int.MaxValue && intSkillGroupUpper > intTotalBaseRating)
                        {
                            int intGroupCost;
                            int intNakedSkillCost = objMySkillGroup.SkillList.Count;
                            if (intTotalBaseRating == 0)
                            {
                                intGroupCost = CharacterObject.Options.KarmaNewSkillGroup;
                                intNakedSkillCost *= CharacterObject.Options.KarmaNewActiveSkill;
                            }
                            else
                            {
                                intGroupCost = (intTotalBaseRating + 1) * CharacterObject.Options.KarmaImproveSkillGroup;
                                intNakedSkillCost *= (intTotalBaseRating + 1) * CharacterObject.Options.KarmaImproveActiveSkill;
                            }

                            upgrade += (intGroupCost - intNakedSkillCost);
                        }
                    }
                }

                decimal decMultiplier = 1.0m;
                decimal decExtra = 0;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if ((objLoopImprovement.Maximum == 0 || intTotalBaseRating + 1 <= objLoopImprovement.Maximum) && objLoopImprovement.Minimum <= intTotalBaseRating + 1 &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == DictionaryKey || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillKarmaCost)
                                decExtra += objLoopImprovement.Value;
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                        else if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCost)
                                decExtra += objLoopImprovement.Value;
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                    }
                }
                if (decMultiplier != 1.0m)
                    upgrade = (upgrade * decMultiplier + decExtra).StandardRound();
                else
                    upgrade += decExtra.StandardRound();

                int intMinCost = Math.Min(1, intOptionsCost);
                if (upgrade < intMinCost && !CharacterObject.Options.CompensateSkillGroupKarmaDifference)
                    upgrade = intMinCost;
                return upgrade;
            }
        }

        public void Upgrade()
        {
            if (CharacterObject.Created)
            {
                if (!CanUpgradeCareer)
                    return;

                int price = UpgradeKarmaCost;
                int intTotalBaseRating = TotalBaseRating;
                //If data file contains {4} this crashes but...
                string upgradetext =
                    string.Format(GlobalOptions.CultureInfo, "{0}{4}{1}{4}{2}{4}->{4}{3}",
                        LanguageManager.GetString(IsKnowledgeSkill ? "String_ExpenseKnowledgeSkill" : "String_ExpenseActiveSkill"),
                        CurrentDisplayName,
                        intTotalBaseRating,
                        intTotalBaseRating + 1,
                        LanguageManager.GetString("String_Space"));

                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(price * -1, upgradetext, ExpenseType.Karma, DateTime.Now);
                objExpense.Undo = new ExpenseUndo().CreateKarma(intTotalBaseRating == 0 ? KarmaExpenseType.AddSkill : KarmaExpenseType.ImproveSkill, InternalId);

                CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                CharacterObject.Karma -= price;
            }

            Karma += 1;
        }

        private int _intCachedCanAffordSpecialization = -1;

        public bool CanAffordSpecialization
        {
            get
            {
                if (_intCachedCanAffordSpecialization < 0)
                {
                    if (!CanHaveSpecs)
                        _intCachedCanAffordSpecialization = 0;
                    else
                    {
                        int intPrice = IsKnowledgeSkill ? CharacterObject.Options.KarmaKnowledgeSpecialization : CharacterObject.Options.KarmaSpecialization;

                        decimal decExtraSpecCost = 0;
                        int intTotalBaseRating = TotalBaseRating;
                        decimal decSpecCostMultiplier = 1.0m;
                        foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                        {
                            if (objLoopImprovement.Minimum <= intTotalBaseRating
                                && (string.IsNullOrEmpty(objLoopImprovement.Condition)
                                    || (objLoopImprovement.Condition == "career") == CharacterObject.Created
                                    || (objLoopImprovement.Condition == "create") != CharacterObject.Created)
                                && objLoopImprovement.Enabled
                                && objLoopImprovement.ImprovedName == SkillCategory)
                            {
                                if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCost)
                                    decExtraSpecCost += objLoopImprovement.Value;
                                else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier)
                                    decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                            }
                        }
                        if (decSpecCostMultiplier != 1.0m)
                            intPrice = (intPrice * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
                        else
                            intPrice += decExtraSpecCost.StandardRound(); //Spec

                        _intCachedCanAffordSpecialization = intPrice <= CharacterObject.Karma ? 1 : 0;
                    }
                }

                return _intCachedCanAffordSpecialization > 0;
            }
        }

        public void AddSpecialization(string strName)
        {
            SkillSpecialization nspec = new SkillSpecialization(CharacterObject, strName);
            if (CharacterObject.Created)
            {
                int intPrice = IsKnowledgeSkill ? CharacterObject.Options.KarmaKnowledgeSpecialization : CharacterObject.Options.KarmaSpecialization;

                decimal decExtraSpecCost = 0;
                int intTotalBaseRating = TotalBaseRating;
                decimal decSpecCostMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if (objLoopImprovement.Minimum <= intTotalBaseRating
                        && (string.IsNullOrEmpty(objLoopImprovement.Condition)
                            || (objLoopImprovement.Condition == "career") == CharacterObject.Created
                            || (objLoopImprovement.Condition == "create") != CharacterObject.Created)
                        && objLoopImprovement.Enabled && objLoopImprovement.ImprovedName == SkillCategory)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCost)
                            decExtraSpecCost += objLoopImprovement.Value;
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier)
                            decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
                if (decSpecCostMultiplier != 1.0m)
                    intPrice = (intPrice * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
                else
                    intPrice += decExtraSpecCost.StandardRound(); //Spec

                if (intPrice > CharacterObject.Karma)
                    return;

                //If data file contains {4} this crashes but...
                string upgradetext = //TODO WRONG
                    string.Format(GlobalOptions.CultureInfo, "{0}{3}{1}{3}({2})",
                        LanguageManager.GetString("String_ExpenseLearnSpecialization"),
                        CurrentDisplayName,
                        strName,
                        LanguageManager.GetString("String_Space"));

                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(intPrice * -1, upgradetext, ExpenseType.Karma, DateTime.Now);
                objExpense.Undo = new ExpenseUndo().CreateKarma(KarmaExpenseType.AddSpecialization, nspec.InternalId);

                CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                CharacterObject.Karma -= intPrice;
            }

            Specializations.Add(nspec);
        }

        /// <summary>
        /// How many free points of this skill have gotten, with the exception of some things during character creation
        /// </summary>
        /// <returns></returns>
        private int _intCachedFreeKarma = int.MinValue;
        public int FreeKarma
        {
            get
            {
                if (_intCachedFreeKarma != int.MinValue)
                    return _intCachedFreeKarma;

                return _intCachedFreeKarma = string.IsNullOrEmpty(Name)
                    ? 0
                    : ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillLevel, false, DictionaryKey).StandardRound();
            }
        }

        /// <summary>
        /// How many free points this skill have gotten during some parts of character creation
        /// </summary>
        /// <returns></returns>
        private int _intCachedFreeBase = int.MinValue;

        public int FreeBase
        {
            get
            {
                if (_intCachedFreeBase != int.MinValue)
                    return _intCachedFreeBase;

                return _intCachedFreeBase = string.IsNullOrEmpty(Name)
                    ? 0
                    : ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillBase, false, DictionaryKey).StandardRound();
            }
        }

        private int _intCachedForcedBuyWithKarma = -1;

        /// <summary>
        /// Do circumstances force the Specialization to be bought with karma?
        /// </summary>
        /// <returns></returns>
        private bool ForcedBuyWithKarma
        {
            get
            {
                if (_intCachedForcedBuyWithKarma < 0)
                {
                    _intCachedForcedBuyWithKarma = !string.IsNullOrWhiteSpace(Specialization)
                                                   && ((KarmaPoints > 0
                                                        && BasePoints + FreeBase == 0
                                                        && !CharacterObject.Options
                                                            .AllowPointBuySpecializationsOnKarmaSkills)
                                                       || SkillGroupObject?.Karma > 0
                                                       || SkillGroupObject?.Base > 0)
                        ? 1
                        : 0;
                }

                return _intCachedForcedBuyWithKarma > 0;
            }
        }

        private int _intCachedForcedNotBuyWithKarma = -1;

        /// <summary>
        /// Do circumstances force the Specialization to not be bought with karma?
        /// </summary>
        /// <returns></returns>
        private bool ForcedNotBuyWithKarma
        {
            get
            {
                if (_intCachedForcedNotBuyWithKarma < 0)
                {
                    _intCachedForcedNotBuyWithKarma = TotalBaseRating == 0
                                                      || (CharacterObject.Options.StrictSkillGroupsInCreateMode
                                                          && !CharacterObject.Created
                                                          && SkillGroupObject?.Karma > 0)
                        ? 1
                        : 0;
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
            string strSpace = LanguageManager.GetString("String_Space");
            string strReturn = string.Format(GlobalOptions.CultureInfo, "{0}{1}({2})", CurrentDisplayName, strSpace, intPool);
            // Add any Specialization bonus if applicable.
            if (!string.IsNullOrWhiteSpace(strValidSpec))
            {
                int intSpecBonus = GetSpecializationBonus(strValidSpec);
                if (intSpecBonus != 0)
                    strReturn +=
                        string.Format(GlobalOptions.CultureInfo, "{0}{1}{0}{2}{3}{0}{4}{0}({5})", strSpace, '+', LanguageManager.GetString("String_ExpenseSpecialization"),
                            LanguageManager.GetString("String_Colon"), strValidSpec, intSpecBonus);
            }

            return strReturn;
        }
    }
}
