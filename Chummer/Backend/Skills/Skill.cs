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
        public List<ListItem> SuggestedSpecializations { get; } = new List<ListItem>(); //List of suggested specializations for this skill
        private bool _blnDefault;

        public virtual void WriteTo(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("skill");
            objWriter.WriteElementString("guid", Id.ToString("D"));
            objWriter.WriteElementString("suid", SkillId.ToString("D"));
            objWriter.WriteElementString("isknowledge", bool.FalseString);
            objWriter.WriteElementString("skillcategory", SkillCategory);
            objWriter.WriteElementString("karma", _intKarma.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("base", _intBase.ToString(GlobalOptions.InvariantCultureInfo)); //this could acctually be saved in karma too during career
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("name", _strName);
            if (!CharacterObject.Created)
            {
                objWriter.WriteElementString("buywithkarma", BuyWithKarma.ToString());
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
            objWriter.WriteStartElement("skill");

            int intRating = PoolOtherAttribute(AttributeModifiers, Attribute);
            int intSpecRating = Specializations.Count == 0 || CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && x.UniqueName == Name && string.IsNullOrEmpty(x.Condition) && x.Enabled)
                ? intRating
                : (!IsKnowledgeSkill && Name == "Artisan" &&
                   CharacterObject.Qualities.Any(objQuality => objQuality.Name == "Inspired")
                    ? intRating + 3
                    : intRating + 2);

            int intRatingModifiers = RatingModifiers(Attribute);
            int intDicePoolModifiers = PoolModifiers(Attribute);

            objWriter.WriteElementString("name", DisplayNameMethod(strLanguageToPrint));
            objWriter.WriteElementString("skillgroup", SkillGroupObject?.DisplayNameMethod(strLanguageToPrint) ?? LanguageManager.GetString("String_None", strLanguageToPrint));
            objWriter.WriteElementString("skillgroup_english", SkillGroupObject?.Name ?? LanguageManager.GetString("String_None", strLanguageToPrint));
            objWriter.WriteElementString("skillcategory", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("skillcategory_english", SkillCategory);  //Might exist legacy but not existing atm, will see if stuff breaks
            objWriter.WriteElementString("grouped", (SkillGroupObject != null && SkillGroupObject.CareerIncrease && SkillGroupObject.Rating > 0).ToString());
            objWriter.WriteElementString("default", Default.ToString());
            objWriter.WriteElementString("rating", Rating.ToString(objCulture));
            objWriter.WriteElementString("ratingmax", RatingMaximum.ToString(objCulture));
            objWriter.WriteElementString("specializedrating", intSpecRating.ToString(objCulture));
            objWriter.WriteElementString("specbonus", (intSpecRating - intRating).ToString(objCulture));
            objWriter.WriteElementString("total", PoolOtherAttribute(AttributeModifiers, Attribute).ToString(objCulture));
            objWriter.WriteElementString("knowledge", IsKnowledgeSkill.ToString());
            objWriter.WriteElementString("exotic", IsExoticSkill.ToString());
            objWriter.WriteElementString("buywithkarma", BuyWithKarma.ToString());
            objWriter.WriteElementString("base", Base.ToString(objCulture));
            objWriter.WriteElementString("karma", Karma.ToString(objCulture));
            objWriter.WriteElementString("spec", DisplaySpecializationMethod(strLanguageToPrint));
            objWriter.WriteElementString("attribute", Attribute);
            objWriter.WriteElementString("displayattribute", DisplayAttributeMethod(strLanguageToPrint));
            if (CharacterObject.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteElementString("attributemod", CharacterObject.GetAttribute(Attribute).TotalValue.ToString(objCulture));
            objWriter.WriteElementString("ratingmod", (intRatingModifiers + intDicePoolModifiers).ToString(objCulture));
            objWriter.WriteElementString("poolmod", intDicePoolModifiers.ToString(objCulture));
            objWriter.WriteElementString("islanguage", (SkillCategory == "Language").ToString());
            objWriter.WriteElementString("bp", CurrentKarmaCost.ToString(objCulture));
            objWriter.WriteStartElement("skillspecializations");
            foreach (SkillSpecialization objSpec in Specializations)
            {
                objSpec.Print(objWriter, strLanguageToPrint);
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
                    objLoadingSkill = new KnowledgeSkill(objCharacter, xmlSkillNode["name"]?.InnerText ?? string.Empty, !Convert.ToBoolean(xmlSkillNode["disableupgrades"]?.InnerText));
                else
                {
                    KnowledgeSkill knoSkill = new KnowledgeSkill(objCharacter);
                    knoSkill.Load(xmlSkillNode);
                    objLoadingSkill = knoSkill;
                }
            }
            else if (suid != Guid.Empty)
            {
                XmlNode xmlSkillDataNode = xmlSkills.SelectSingleNode($"/chummer/skills/skill[id = '{xmlSkillNode["suid"]?.InnerText}']");

                if (xmlSkillDataNode == null) return null;

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
                    objLoadingSkill = new KnowledgeSkill(objCharacter, xmlSkillNode["name"]?.InnerText ?? string.Empty, !Convert.ToBoolean(xmlSkillNode["disableupgrades"]?.InnerText));
                else
                {
                    KnowledgeSkill knoSkill = new KnowledgeSkill(objCharacter);
                    knoSkill.Load(xmlSkillNode);
                    objLoadingSkill = knoSkill;
                }
            }
            if (xmlSkillNode.TryGetField("guid", Guid.TryParse, out Guid guiTemp))
                objLoadingSkill.Id = guiTemp;

            xmlSkillNode.TryGetInt32FieldQuickly("karma", ref objLoadingSkill._intKarma);
            xmlSkillNode.TryGetInt32FieldQuickly("base", ref objLoadingSkill._intBase);
            xmlSkillNode.TryGetBoolFieldQuickly("buywithkarma", ref objLoadingSkill._blnBuyWithKarma);
            if (!xmlSkillNode.TryGetStringFieldQuickly("altnotes", ref objLoadingSkill._strNotes))
                xmlSkillNode.TryGetStringFieldQuickly("notes", ref objLoadingSkill._strNotes);

            using (XmlNodeList xmlSpecList = xmlSkillNode.SelectNodes("specs/spec"))
            {
                if (xmlSpecList == null) return objLoadingSkill;
                foreach (XmlNode xmlSpec in xmlSpecList)
                {
                    objLoadingSkill.Specializations.Add(SkillSpecialization.Load(xmlSpec, objLoadingSkill));
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

            int.TryParse(xmlSkillNode["base"]?.InnerText, out int intBaseRating);
            int.TryParse(xmlSkillNode["rating"]?.InnerText, out int intFullRating);
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
                XmlNode xmlSkillDataNode = xmlSkillsDocument.SelectSingleNode($"/chummer/skills/skill[id = '{suid}']") ??
                    //Some stuff apparently have a guid of 0000-000... (only exotic?)
                    xmlSkillsDocument.SelectSingleNode($"/chummer/skills/skill[name = \"{strName}\"]");


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

            List<SkillSpecialization> lstSpecializations = new List<SkillSpecialization>();
            using (XmlNodeList xmlSpecList = xmlSkillNode.SelectNodes("skillspecializations/skillspecialization"))
                if (xmlSpecList != null)
                    foreach (XmlNode xmlSpecializationNode in xmlSpecList)
                    {
                        lstSpecializations.Add(SkillSpecialization.Load(xmlSpecializationNode, objSkill));
                    }
            if (lstSpecializations.Count != 0)
            {
                objSkill.Specializations.AddRange(lstSpecializations);
            }

            return objSkill;
        }

        public static Skill LoadFromHeroLab(Character objCharacter, XmlNode xmlSkillNode, bool blnIsKnowledgeSkill, string strSkillType = "")
        {
            string strName = xmlSkillNode.Attributes?["name"]?.InnerText ?? string.Empty;

            XmlNode xmlSkillDataNode = XmlManager.Load("skills.xml").SelectSingleNode("/chummer/" + (blnIsKnowledgeSkill ? "knowledgeskills" : "skills") + "/skill[name = \"" + strName + "\"]");
            Guid suid = Guid.NewGuid();
            if (xmlSkillDataNode?.TryGetField("id", Guid.TryParse, out suid) != true)
                suid = Guid.NewGuid();

            int intKarmaRating = 0;
            if (xmlSkillNode.Attributes?["text"]?.InnerText != "N")      // Native Languages will have a base + karma rating of 0
                int.TryParse(xmlSkillNode.Attributes?["base"]?.InnerText, out intKarmaRating); // Only reading karma rating out for now, any base rating will need modification within SkillsSection

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

            List<SkillSpecialization> lstSpecializations = new List<SkillSpecialization>();
            using (XmlNodeList xmlSpecList = xmlSkillNode.SelectNodes("specialization"))
                if (xmlSpecList?.Count > 0)
                    foreach (XmlNode xmlSpecializationNode in xmlSpecList)
                    {
                        string strSpecializationName = xmlSpecializationNode.Attributes?["bonustext"]?.InnerText;
                        if (string.IsNullOrEmpty(strSpecializationName)) continue;
                        int intLastPlus = strSpecializationName.LastIndexOf('+');
                        if (intLastPlus > strSpecializationName.Length)
                            strSpecializationName = strSpecializationName.Substring(0, intLastPlus - 1);
                        lstSpecializations.Add(new SkillSpecialization(strSpecializationName, false, objSkill));
                    }
            if (lstSpecializations.Count != 0)
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
                    blnIsKnowledgeSkill = XmlManager.Load("skills.xml").SelectSingleNode($"/chummer/categories/category[. = '{category}']/@type")?.InnerText != "active";
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
            CharacterObject = character;
            CharacterObject.PropertyChanged += OnCharacterChanged;
            CharacterObject.AttributeSection.PropertyChanged += OnAttributeSectionChanged;
            CharacterObject.AttributeSection.Attributes.CollectionChanged += OnAttributesCollectionChanged;
            Specializations.ListChanged += SpecializationsOnListChanged;

            _skillDependencyGraph = new DependancyGraph<string>(
                new DependancyGraphNode<string>(nameof(PoolToolTip),
                    new DependancyGraphNode<string>(nameof(AttributeModifiers),
                        new DependancyGraphNode<string>(nameof(AttributeObject),
                            new DependancyGraphNode<string>(nameof(RelevantImprovements)))),
                    new DependancyGraphNode<string>(nameof(DisplayPool),
                        new DependancyGraphNode<string>(nameof(KnowledgeSkill.Type), () => IsKnowledgeSkill),
                        new DependancyGraphNode<string>(nameof(DisplayOtherAttribute),
                            new DependancyGraphNode<string>(nameof(PoolOtherAttribute),
                                new DependancyGraphNode<string>(nameof(Enabled)),
                                new DependancyGraphNode<string>(nameof(Rating)),
                                new DependancyGraphNode<string>(nameof(PoolModifiers),
                                    new DependancyGraphNode<string>(nameof(Bonus),
                                        new DependancyGraphNode<string>(nameof(RelevantImprovements))
                                    )
                                ),
                                new DependancyGraphNode<string>(nameof(Default),
                                    new DependancyGraphNode<string>(nameof(RelevantImprovements))
                                ),
                                new DependancyGraphNode<string>(nameof(DefaultModifier),
                                    new DependancyGraphNode<string>(nameof(Name))
                                )
                            ),
                            new DependancyGraphNode<string>(nameof(Name)),
                            new DependancyGraphNode<string>(nameof(IsExoticSkill)),
                            new DependancyGraphNode<string>(nameof(Specialization))
                        ),
                        new DependancyGraphNode<string>(nameof(Pool),
                            new DependancyGraphNode<string>(nameof(AttributeModifiers),
                                new DependancyGraphNode<string>(nameof(AttributeObject))
                            ),
                            new DependancyGraphNode<string>(nameof(PoolOtherAttribute)),
                            new DependancyGraphNode<string>(nameof(Attribute))
                        )
                    )
                ),
                new DependancyGraphNode<string>(nameof(CanHaveSpecs),
                    new DependancyGraphNode<string>(nameof(KnowledgeSkill.AllowUpgrade), () => IsKnowledgeSkill),
                    new DependancyGraphNode<string>(nameof(IsExoticSkill)),
                    new DependancyGraphNode<string>(nameof(KarmaUnlocked)),
                    new DependancyGraphNode<string>(nameof(TotalBaseRating),
                        new DependancyGraphNode<string>(nameof(RatingModifiers),
                            new DependancyGraphNode<string>(nameof(Bonus))
                        ),
                        new DependancyGraphNode<string>(nameof(LearnedRating),
                            new DependancyGraphNode<string>(nameof(Karma),
                                new DependancyGraphNode<string>(nameof(FreeKarma),
                                    new DependancyGraphNode<string>(nameof(Name))
                                ),
                                new DependancyGraphNode<string>(nameof(RatingMaximum),
                                    new DependancyGraphNode<string>(nameof(RelevantImprovements))
                                ),
                                new DependancyGraphNode<string>(nameof(KarmaPoints))
                            ),
                            new DependancyGraphNode<string>(nameof(Base),
                                new DependancyGraphNode<string>(nameof(FreeBase),
                                    new DependancyGraphNode<string>(nameof(Name))
                                ),
                                new DependancyGraphNode<string>(nameof(RatingMaximum)),
                                new DependancyGraphNode<string>(nameof(BasePoints))
                            )
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(Leveled),
                        new DependancyGraphNode<string>(nameof(Rating),
                            new DependancyGraphNode<string>(nameof(CyberwareRating),
                                new DependancyGraphNode<string>(nameof(Name))
                            ),
                            new DependancyGraphNode<string>(nameof(TotalBaseRating))
                        )
                    )
                ),
                new DependancyGraphNode<string>(nameof(UpgradeToolTip),
                    new DependancyGraphNode<string>(nameof(Rating)),
                    new DependancyGraphNode<string>(nameof(UpgradeKarmaCost),
                        new DependancyGraphNode<string>(nameof(TotalBaseRating)),
                        new DependancyGraphNode<string>(nameof(RatingMaximum))
                    )
                ),
                new DependancyGraphNode<string>(nameof(BuyWithKarma),
                    new DependancyGraphNode<string>(nameof(ForcedBuyWithKarma),
                        new DependancyGraphNode<string>(nameof(Specialization)),
                        new DependancyGraphNode<string>(nameof(KarmaPoints)),
                        new DependancyGraphNode<string>(nameof(BasePoints)),
                        new DependancyGraphNode<string>(nameof(FreeBase))
                    ),
                    new DependancyGraphNode<string>(nameof(ForcedNotBuyWithKarma),
                        new DependancyGraphNode<string>(nameof(TotalBaseRating))
                    )
                ),
                new DependancyGraphNode<string>(nameof(RelevantImprovements),
                    new DependancyGraphNode<string>(nameof(Name))
                ),
                new DependancyGraphNode<string>(nameof(DisplayAttribute),
                    new DependancyGraphNode<string>(nameof(Attribute),
                        new DependancyGraphNode<string>(nameof(AttributeObject))
                    )
                ),
                new DependancyGraphNode<string>(nameof(DisplayName),
                    new DependancyGraphNode<string>(nameof(Name))
                ),
                new DependancyGraphNode<string>(nameof(SkillToolTip),
                    new DependancyGraphNode<string>(nameof(Notes)),
                    new DependancyGraphNode<string>(nameof(DisplayCategory),
                        new DependancyGraphNode<string>(nameof(SkillCategory),
                            new DependancyGraphNode<string>(nameof(KnowledgeSkill.Type), () => IsKnowledgeSkill)
                        )
                    )
                ),
                new DependancyGraphNode<string>(nameof(PreferredControlColor),
                    new DependancyGraphNode<string>(nameof(Leveled))
                ),
                new DependancyGraphNode<string>(nameof(PreferredColor),
                    new DependancyGraphNode<string>(nameof(Notes))
                ),
                new DependancyGraphNode<string>(nameof(CGLSpecializations),
                    new DependancyGraphNode<string>(nameof(SuggestedSpecializations))
                ),
                new DependancyGraphNode<string>(nameof(CurrentSpCost),
                    new DependancyGraphNode<string>(nameof(BasePoints)),
                    new DependancyGraphNode<string>(nameof(Specialization)),
                    new DependancyGraphNode<string>(nameof(BuyWithKarma))
                ),
                new DependancyGraphNode<string>(nameof(CurrentKarmaCost),
                    new DependancyGraphNode<string>(nameof(RangeCost)),
                    new DependancyGraphNode<string>(nameof(TotalBaseRating)),
                    new DependancyGraphNode<string>(nameof(Base)),
                    new DependancyGraphNode<string>(nameof(FreeKarma)),
                    new DependancyGraphNode<string>(nameof(Specializations))
                ),
                new DependancyGraphNode<string>(nameof(CanUpgradeCareer),
                    new DependancyGraphNode<string>(nameof(UpgradeKarmaCost)),
                    new DependancyGraphNode<string>(nameof(RatingMaximum)),
                    new DependancyGraphNode<string>(nameof(TotalBaseRating))
                ),
                new DependancyGraphNode<string>(nameof(Enabled),
                    new DependancyGraphNode<string>(nameof(ForceDisabled)),
                    new DependancyGraphNode<string>(nameof(Attribute)),
                    new DependancyGraphNode<string>(nameof(Name))
                ),
                new DependancyGraphNode<string>(nameof(DisplaySpecialization),
                    new DependancyGraphNode<string>(nameof(Specialization),
                        new DependancyGraphNode<string>(nameof(TotalBaseRating)),
                        new DependancyGraphNode<string>(nameof(Specializations))
                    )
                ),
                new DependancyGraphNode<string>(nameof(AllowDelete), () => IsKnowledgeSkill,
                    new DependancyGraphNode<string>(nameof(FreeBase)),
                    new DependancyGraphNode<string>(nameof(FreeKarma)),
                    new DependancyGraphNode<string>(nameof(RatingModifiers))
                )
            );
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
        /// The total, general pourpose dice pool for this skill
        /// </summary>
        public int Pool => PoolOtherAttribute(AttributeModifiers, Attribute);

        public bool Leveled => Rating > 0;

        public Color PreferredControlColor => Leveled ? SystemColors.ButtonHighlight : SystemColors.Control;

        private int _intCachedCanHaveSpecs = -1;

        public bool CanHaveSpecs
        {
            get
            {
                if ((this as KnowledgeSkill)?.AllowUpgrade == false)
                    return false;
                if (_intCachedCanHaveSpecs >= 0) return _intCachedCanHaveSpecs > 0;
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

		//TODO change to the acctual characterattribute object
		/// <summary>
		/// The Abbreviation of the linke attribute. Not the object due legacy
		/// </summary>
		public string Attribute => AttributeObject.Abbrev;

        /// <summary>
        /// The translated abbreviation of the linked attribute.
        /// </summary>
        public string DisplayAttributeMethod(string strLanguage)
        {
            return LanguageManager.GetString($"String_Attribute{Attribute}Short", strLanguage);
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
                if (CharacterObject.IsCritter && !CharacterObject.Improvements.Any(x =>
                        x.ImproveType == Improvement.ImprovementType.AllowSkillDefault &&
                        (x.ImprovedName == Name || x.ImprovedName == string.Empty) && x.Enabled) && Rating == 0)
                {
                    _intCachedEnabled = 0;
                    return false;
                }

                if (CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.SkillDisable && x.ImprovedName == Name && x.Enabled))
                {
                    _intCachedEnabled = 0;
                    return false;
                }
                if (!IsKnowledgeSkill)
                {
                    if (Name.Contains("Flight"))
                    {
                        string strFlyString = CharacterObject.GetFly(GlobalOptions.InvariantCultureInfo, GlobalOptions.DefaultLanguage);
                        if (string.IsNullOrEmpty(strFlyString) || strFlyString == "0" || strFlyString.Contains(LanguageManager.GetString("String_ModeSpecial", GlobalOptions.DefaultLanguage)))
                        {
                            _intCachedEnabled = 0;
                            return false;
                        }
                    }
                    if (Name.Contains("Swimming"))
                    {
                        string strSwimString = CharacterObject.GetSwim(GlobalOptions.InvariantCultureInfo, GlobalOptions.DefaultLanguage);
                        if (string.IsNullOrEmpty(strSwimString) || strSwimString == "0" || strSwimString.Contains(LanguageManager.GetString("String_ModeSpecial", GlobalOptions.DefaultLanguage)))
                        {
                            _intCachedEnabled = 0;
                            return false;
                        }
                    }
                    if (Name.Contains("Running"))
                    {
                        string strMovementString = CharacterObject.GetMovement(GlobalOptions.InvariantCultureInfo, GlobalOptions.DefaultLanguage);
                        if (string.IsNullOrEmpty(strMovementString) || strMovementString == "0" || strMovementString.Contains(LanguageManager.GetString("String_ModeSpecial", GlobalOptions.DefaultLanguage)))
                        {
                            _intCachedEnabled = 0;
                            return false;
                        }
                    }
                }
                //TODO: This is a temporary workaround until proper support for selectively enabling or disabling skills works, as above.
                if (CharacterObject.IsAI)
                {
                    _intCachedEnabled = !(Attribute == "MAG" || Attribute == "RES") ? 1 : 0;
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
                return _blnDefault && !RelevantImprovements(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.BlockSkillDefault).Any();
            }
            set
            {
                if (_blnDefault == value) return;
                _blnDefault = value;
                OnPropertyChanged();
            }
        }

        public bool IsExoticSkill => this is ExoticSkill;

        public bool IsKnowledgeSkill => this is KnowledgeSkill;

        public string Name
        {
            get => _strName;
            set
            {
                if (value == _strName) return;
                _strName = value;
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
        public string InternalId => _guidInternalId.ToString("D");

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

        // ReSharper disable once InconsistentNaming
        public IReadOnlyList<ListItem> CGLSpecializations
        {
            get
            {
                List<ListItem> lstSuggestedSpecializations = new List<ListItem>(SuggestedSpecializations);
                foreach (Improvement objImprovement in CharacterObject.Improvements.Where(x =>
                    x.ImprovedName == Name && x.ImproveType == Improvement.ImprovementType.SkillSpecializationOption &&
                    lstSuggestedSpecializations.All(y => y.Value?.ToString() != x.UniqueName) && x.Enabled))
                {
                    string strSpecializationName = objImprovement.UniqueName;
                    lstSuggestedSpecializations.Add(new ListItem(strSpecializationName, LanguageManager.TranslateExtra(strSpecializationName, GlobalOptions.Language)));
                }
                return lstSuggestedSpecializations;
            }
        }

        private readonly Dictionary<string, string> _cachedStringSpec = new Dictionary<string, string>();
        public virtual string DisplaySpecializationMethod(string strLanguage)
        {
            if (_cachedStringSpec.TryGetValue(strLanguage, out string strReturn)) return strReturn;
            strReturn = string.Join(", ", Specializations.Select(x => x.DisplayName(strLanguage)));

            _cachedStringSpec.Add(strLanguage, strReturn);

            return strReturn;
        }

        public string DisplaySpecialization => DisplaySpecializationMethod(GlobalOptions.Language);

        //TODO A unit test here?, I know we don't have them, but this would be improved by some
        //Or just ignore support for multiple specizalizations even if the rules say it is possible?
        public BindingList<SkillSpecialization> Specializations { get; } = new BindingList<SkillSpecialization>();

        public string Specialization
        {
            get
            {
                if (TotalBaseRating == 0)
                {
                    return string.Empty; //Unleveled skills cannot have a specialization;
                }

                if (IsExoticSkill)
                {
                    return ((ExoticSkill) this).Specific;
                }

                return Specializations.Count > 0 ? Specializations[0].DisplayName(GlobalOptions.Language) : string.Empty;
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
                    Specializations.Add(new SkillSpecialization(value, false, this));
                }
                else
                {
                    if (Specializations[0].Free)
                    {
                        Specializations.MergeInto(new SkillSpecialization(value, false, this), (x, y) => x.Free == y.Free ? 0 : (x.Free ? 1 : -1));
                    }
                    else
                    {
                        Specializations[0] = new SkillSpecialization(value, false, this);
                    }
                }
            }
        }

        public bool HasSpecialization(string strSpecialization)
        {
            if (IsExoticSkill)
            {
                return ((ExoticSkill)this).Specific == strSpecialization;
            }
            return Specializations.Any(x => (x.Name == strSpecialization || x.DisplayName(GlobalOptions.Language) == strSpecialization)) && !CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && x.UniqueName == Name && string.IsNullOrEmpty(x.Condition) && x.Enabled);
        }

        public SkillSpecialization GetSpecialization(string strSpecialization)
        {

            if (IsExoticSkill && ((ExoticSkill)this).Specific == strSpecialization)
            {
                return Specializations[0];
            }
            return HasSpecialization(strSpecialization) ? Specializations.FirstOrDefault(x => x.Name == strSpecialization || x.DisplayName(GlobalOptions.Language) == strSpecialization) : null;
        }

        public string PoolToolTip => CompileDicepoolTooltip(Attribute);

        public string CompileDicepoolTooltip(string abbrev = "")
        {
            CharacterAttrib att = CharacterObject.AttributeSection.GetAttributeByName(abbrev);
            if (!Default && !Leveled)
            {
                return LanguageManager.GetString("Tip_Skill_Cannot_Default", GlobalOptions.Language);
            }

            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            IList<Improvement> lstRelevantImprovements = RelevantImprovements(null,"",true).ToList();
            StringBuilder s;
            if (CyberwareRating > TotalBaseRating)
            {
                s = new StringBuilder(
                    $"{LanguageManager.GetString("Tip_Skill_SkillsoftRating", GlobalOptions.Language)}{strSpaceCharacter}({CyberwareRating})");
            }
            else
            {
                s = new StringBuilder(
                    $"{LanguageManager.GetString("Tip_Skill_SkillRating", GlobalOptions.Language)}{strSpaceCharacter}({Rating}");
                bool first = true;
                foreach (Improvement objImprovement in lstRelevantImprovements.Where(x => x.AddToRating))
                {
                    if (first)
                    {
                        first = false;
                        s.Append(strSpaceCharacter + "(Base" + strSpaceCharacter + '(');
                        s.Append(LearnedRating.ToString());
                        s.Append(')');
                    }

                    s.Append(strSpaceCharacter + '+' + strSpaceCharacter);
                    s.Append(CharacterObject.GetObjectName(objImprovement, GlobalOptions.Language));
                    s.Append(strSpaceCharacter + '(');
                    s.Append(objImprovement.Value.ToString(GlobalOptions.CultureInfo));
                    s.Append(')');
                }

                if (!first) s.Append(')');
                s.Append(')');
            }

            s.Append(strSpaceCharacter + '+' + strSpaceCharacter + att.DisplayAbbrev + strSpaceCharacter + '(' +
                     att.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');
            if (Default && !Leveled)
            {
                s.Append(DefaultModifier == 0
                    ? strSpaceCharacter +
                      CharacterObject.GetObjectName(
                          CharacterObject.Improvements.FirstOrDefault(x =>
                              x.ImproveType == Improvement.ImprovementType.ReflexRecorderOptimization && x.Enabled),
                          GlobalOptions.Language) + strSpaceCharacter
                    : strSpaceCharacter + '-' + strSpaceCharacter +
                      LanguageManager.GetString("Tip_Skill_Defaulting", GlobalOptions.Language) + strSpaceCharacter +
                      '(' + 1.ToString(GlobalOptions.CultureInfo) + ')');
            }

            foreach (Improvement source in lstRelevantImprovements.Where(x =>
                !x.AddToRating && x.ImproveType != Improvement.ImprovementType.SwapSkillAttribute &&
                x.ImproveType != Improvement.ImprovementType.SwapSkillSpecAttribute))
            {
                s.Append(strSpaceCharacter + '+' + strSpaceCharacter);
                s.Append(CharacterObject.GetObjectName(source, GlobalOptions.Language));
                if (source.Condition != string.Empty)
                {
                    s.Append(strSpaceCharacter + '(');
                    s.Append(source.Condition.ToString(GlobalOptions.CultureInfo));
                    s.Append(')');
                }

                s.Append(strSpaceCharacter + '(');
                s.Append(source.Value.ToString(GlobalOptions.CultureInfo));
                s.Append(')');
            }

            int wound = CharacterObject.WoundModifier;
            if (wound != 0)
            {
                s.Append(strSpaceCharacter + '-' + strSpaceCharacter +
                         LanguageManager.GetString("Tip_Skill_Wounds", GlobalOptions.Language) + strSpaceCharacter +
                         '(' + wound.ToString(GlobalOptions.CultureInfo) + ')');
            }

            if (att.Abbrev == "STR" || att.Abbrev == "AGI")
            {
                foreach (Cyberware cyberware in CharacterObject.Cyberware.Where(x =>
                    x.Name.Contains(" Arm") || x.Name.Contains(" Hand")))
                {
                    s.Append(Environment.NewLine);
                    s.AppendFormat("{0}{1}{2} ", cyberware.Location, strSpaceCharacter,
                        cyberware.DisplayNameShort(GlobalOptions.Language));
                    if (cyberware.Grade.Name != "Standard")
                    {
                        s.AppendFormat("({0}){1}", cyberware.Grade.DisplayName(GlobalOptions.Language),
                            strSpaceCharacter);
                    }

                    int pool = PoolOtherAttribute(
                        att.Abbrev == "STR" ? cyberware.TotalStrength : cyberware.TotalAgility, att.Abbrev);
                    if (cyberware.Location == CharacterObject.PrimaryArm || CharacterObject.Ambidextrous ||
                        cyberware.LimbSlotCount > 1)
                    {
                        s.Append(pool);
                    }
                    else
                    {
                        s.AppendFormat("{0}{1}(-2{1}{2})", pool - 2, strSpaceCharacter,
                            LanguageManager.GetString("Tip_Skill_OffHand", GlobalOptions.Language));
                    }
                }
            }

            if (att.Abbrev != Attribute) return s.ToString();
            foreach (Improvement objSwapSkillAttribute in lstRelevantImprovements.Where(x =>
                x.ImproveType == Improvement.ImprovementType.SwapSkillAttribute ||
                x.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute))
            {
                s.Append(Environment.NewLine);
                if (objSwapSkillAttribute.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute)
                    s.AppendFormat("{0}:{1}", objSwapSkillAttribute.Exclude, strSpaceCharacter);
                s.AppendFormat("{0}{1}", CharacterObject.GetObjectName(objSwapSkillAttribute, GlobalOptions.Language),
                    strSpaceCharacter);
                int intLoopAttribute = CharacterObject.GetAttribute(objSwapSkillAttribute.ImprovedName).Value;
                int intBasePool = PoolOtherAttribute(intLoopAttribute, objSwapSkillAttribute.ImprovedName);
                bool blnHaveSpec = false;
                if (objSwapSkillAttribute.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute &&
                    Specializations.Any(y =>
                        y.Name == objSwapSkillAttribute.Exclude && !CharacterObject.Improvements.Any(objImprovement =>
                            objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects &&
                            objImprovement.UniqueName == y.Name && string.IsNullOrEmpty(objImprovement.Condition) &&
                            objImprovement.Enabled)))
                {
                    intBasePool += 2;
                    blnHaveSpec = true;
                }

                s.Append(intBasePool.ToString());
                if (objSwapSkillAttribute.ImprovedName != "STR" &&
                    objSwapSkillAttribute.ImprovedName != "AGI") continue;
                foreach (Cyberware cyberware in CharacterObject.Cyberware.Where(x =>
                    x.Name.Contains(" Arm") || x.Name.Contains(" Hand")))
                {
                    s.Append(Environment.NewLine);
                    if (objSwapSkillAttribute.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute)
                        s.AppendFormat("{0}:{1}", objSwapSkillAttribute.Exclude, strSpaceCharacter);
                    s.AppendFormat("{0}{1}",
                        CharacterObject.GetObjectName(objSwapSkillAttribute, GlobalOptions.Language),
                        strSpaceCharacter);
                    s.AppendFormat("{0}{1}{2} ", cyberware.Location, strSpaceCharacter,
                        cyberware.DisplayNameShort(GlobalOptions.Language));
                    if (cyberware.Grade.Name != "Standard")
                    {
                        s.AppendFormat("({0}){1}", cyberware.Grade.DisplayName(GlobalOptions.Language),
                            strSpaceCharacter);
                    }

                    int intLoopPool =
                        PoolOtherAttribute(
                            objSwapSkillAttribute.ImprovedName == "STR"
                                ? cyberware.TotalStrength
                                : cyberware.TotalAgility, objSwapSkillAttribute.ImprovedName);
                    if (blnHaveSpec)
                    {
                        intLoopPool += 2;
                    }

                    if (cyberware.Location == CharacterObject.PrimaryArm || CharacterObject.Ambidextrous ||
                        cyberware.LimbSlotCount > 1)
                    {
                        s.Append(intLoopPool);
                    }
                    else
                    {
                        s.AppendFormat("{0}{1}(-2{1}{2})", intLoopPool - 2, strSpaceCharacter,
                            LanguageManager.GetString("Tip_Skill_OffHand", GlobalOptions.Language));
                    }
                }
            }

            return s.ToString();
        }

        public string UpgradeToolTip => UpgradeKarmaCost < 0
            ? LanguageManager.GetString("Tip_ImproveItemAtMaximum", GlobalOptions.Language)
            : string.Format(LanguageManager.GetString("Tip_ImproveItem", GlobalOptions.Language), (Rating + 1), UpgradeKarmaCost);

        public string AddSpecToolTip
        {
            get
            {
                int intPrice = IsKnowledgeSkill ? CharacterObject.Options.KarmaKnowledgeSpecialization : CharacterObject.Options.KarmaSpecialization;

                int intExtraSpecCost = 0;
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
                            intExtraSpecCost += objLoopImprovement.Value;
                            break;
                        case Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier:
                            decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                            break;
                    }
                }
                if (decSpecCostMultiplier != 1.0m)
                    intPrice = decimal.ToInt32(decimal.Ceiling(intPrice * decSpecCostMultiplier));
                intPrice += intExtraSpecCost; //Spec
                return string.Format(LanguageManager.GetString("Tip_Skill_AddSpecialization", GlobalOptions.Language), intPrice.ToString());
            }
        }

        public string SkillToolTip
        {
            get
            {
                //v-- hack i guess
                string strReturn = string.Empty;
                string strMiddle = string.Empty;
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                if (!string.IsNullOrWhiteSpace(SkillGroup))
                {
                    strMiddle = $"{SkillGroupObject.DisplayName}{strSpaceCharacter}{LanguageManager.GetString("String_ExpenseSkillGroup", GlobalOptions.Language)}" + Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(Notes))
                {
                    strReturn = LanguageManager.GetString("Label_Notes", GlobalOptions.Language) + strSpaceCharacter + Notes.WordWrap(100) + Environment.NewLine + Environment.NewLine;
                }

                strReturn += $"{DisplayCategory(GlobalOptions.Language)}{Environment.NewLine}{strMiddle}{CommonFunctions.LanguageBookLong(Source, GlobalOptions.Language)}{strSpaceCharacter}{LanguageManager.GetString("String_Page", GlobalOptions.Language)}{strSpaceCharacter}{DisplayPage(GlobalOptions.Language)}";

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

        public Color PreferredColor => !string.IsNullOrEmpty(Notes) ? Color.SaddleBrown : SystemColors.WindowText;

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

        public string DisplayNameMethod(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        public string DisplayName => DisplayNameMethod(GlobalOptions.Language);

        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return SkillCategory;

            string strReturn = XmlManager.Load("skills.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + SkillCategory + "\"]/@translate")?.InnerText;

            return strReturn ?? SkillCategory;
        }

        public virtual string DisplayPool => DisplayOtherAttribute(AttributeModifiers, Attribute);

        public string DisplayOtherAttribute(int intAttributeTotalValue, string strAttribute)
        {
            int intPool = PoolOtherAttribute(intAttributeTotalValue, strAttribute);
            if ((IsExoticSkill || string.IsNullOrWhiteSpace(Specialization) || CharacterObject.Improvements.Any(x =>
                     x.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects &&
                     x.UniqueName == Name && string.IsNullOrEmpty(x.Condition) && x.Enabled)) &&
                 !CharacterObject.Improvements.Any(i =>
                     i.ImproveType == Improvement.ImprovementType.Skill && !string.IsNullOrEmpty(i.Condition)))
            {
                return intPool.ToString();
            }
            else
            {
                int intConditionalBonus = PoolOtherAttribute(intAttributeTotalValue, strAttribute, true);
                if (string.IsNullOrWhiteSpace(Specialization) && intPool == intConditionalBonus) return intPool
                    .ToString();
                if (string.IsNullOrWhiteSpace(Specialization)) return $"{intPool} ({intConditionalBonus})";
                //Handler for the Inspired Quality.
                if (Name == "Artisan" && !IsKnowledgeSkill &&
                    CharacterObject.Qualities.Any(objQuality => objQuality.Name == "Inspired"))
                {
                    return $"{intPool} ({3 + intConditionalBonus})";
                }

                return $"{intPool} ({2 + intConditionalBonus})";
            }
        }

        // ReSharper disable once InconsistentNaming
        protected int _intCachedCyberwareRating = int.MinValue;

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
                _objCachedMyXmlNode = XmlManager.Load("skills.xml", strLanguage).SelectSingleNode("/chummer/" + (IsKnowledgeSkill ? "knowledgeskills" : "skills") + "/skill[id = \"" + SkillId.ToString("D") + "\" or id = \"" + SkillId.ToString("D").ToUpperInvariant() + "\"]");
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
                                intMaxHardwire = Math.Max(intMaxHardwire, objImprovement.Value);
                        }
                        else if (objImprovement.ImprovedName == Name)
                            intMaxHardwire = Math.Max(intMaxHardwire, objImprovement.Value);
                    }
                }
                if (intMaxHardwire >= 0)
                {
                    return _intCachedCyberwareRating = intMaxHardwire;
                }

                int intMaxActivesoftRating = Math.Min(ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.Skillwire), ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillsoftAccess));
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
                                    intMaxHardwire = Math.Max(intMaxHardwire, objSkillsoftImprovement.Value);
                            }
                            else if (objSkillsoftImprovement.ImprovedName == Name)
                                intMax = Math.Max(intMax, objSkillsoftImprovement.Value);
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
        private readonly DependancyGraph<string> _skillDependencyGraph;
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
                    lstNamesOfChangedProperties = _skillDependencyGraph.GetWithAllDependants(strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in _skillDependencyGraph.GetWithAllDependants(strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if ((lstNamesOfChangedProperties?.Count > 0) != true)
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
            if (e.PropertyName == nameof(Character.Karma))
            {
                OnMultiplePropertyChanged(nameof(CanUpgradeCareer), nameof(CanAffordSpecialization));
            }
            else if (e.PropertyName == nameof(Character.WoundModifier))
            {
                OnPropertyChanged(nameof(PoolOtherAttribute));
            }
            else if (e.PropertyName == nameof(Character.PrimaryArm))
            {
                OnPropertyChanged(nameof(PoolToolTip));
            }
            else if (e.PropertyName == nameof(Character.Improvements))
            {
                //TODO: Dear god outbound improvements please this is is minimal an impact we can have and it's going to be a nightmare.
                if (CharacterObject.Improvements.Any(i =>
                        i.ImproveType == Improvement.ImprovementType.SwapSkillAttribute && i.Target == "Name") ||
                    _strDefaultAttribute != _objAttribute.Abbrev)
                    _blnCheckSwapSkillImprovements = true;
            }
        }

        protected void OnLinkedAttributeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
                OnPropertyChanged(nameof(AttributeModifiers));
            else if (e.PropertyName == nameof(CharacterAttrib.Value) || e.PropertyName == nameof(CharacterAttrib.Abbrev))
            {
                OnPropertyChanged(nameof(Enabled));
            }
        }

        private void SpecializationsOnListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            _cachedStringSpec.Clear();
            OnPropertyChanged(nameof(Specializations));
        }
    }
}
