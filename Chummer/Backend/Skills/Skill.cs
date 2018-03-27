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

namespace Chummer.Backend.Skills
{
    [DebuggerDisplay("{_strName} {_intBase} {_intKarma}")]
    public partial class Skill : INotifyPropertyChanged, IHasName, IHasXmlNode
    {
        /// <summary>
        /// Called during save to allow derived classes to save additional infomation required to rebuild state
        /// </summary>
        /// <param name="writer"></param>
        protected virtual void SaveExtendedData(XmlTextWriter writer)
        {
        }

        private CharacterAttrib _objAttribute;
        public CharacterAttrib AttributeObject
        {
            get => _objAttribute;
            protected set
            {
                if (_objAttribute != value)
                {
                    if (_objAttribute != null)
                        _objAttribute.PropertyChanged -= OnLinkedAttributeChanged;
                    if (value != null)
                        value.PropertyChanged += OnLinkedAttributeChanged;
                    _objAttribute = value;
                }
            }
        } //Attribute this skill primarily depends on
        private readonly Character _objCharacter; //The Character (parent) to this skill
        private readonly string _strCategory = string.Empty; //Name of the skill category it belongs to
        private string _strName = string.Empty; //English name of this skill
        private string _strNotes = string.Empty; //Text of any notes that were entered by the user
        public List<ListItem> SuggestedSpecializations { get; } = new List<ListItem>(); //List of suggested specializations for this skill
        private bool _blnDefault;

        public void WriteTo(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("skill");
            objWriter.WriteElementString("guid", Id.ToString("D"));
            objWriter.WriteElementString("suid", SkillId.ToString("D"));
            objWriter.WriteElementString("isknowledge", IsKnowledgeSkill.ToString());
            objWriter.WriteElementString("skillcategory", SkillCategory);
            objWriter.WriteElementString("karma", _intKarma.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("base", _intBase.ToString(GlobalOptions.InvariantCultureInfo)); //this could acctually be saved in karma too during career
            objWriter.WriteElementString("notes", _strNotes);
            if (!CharacterObject.Created)
            {
                objWriter.WriteElementString("buywithkarma", _blnBuyWithKarma.ToString());
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

            SaveExtendedData(objWriter);

            objWriter.WriteEndElement();

        }

        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("skill");

            int intRating = PoolOtherAttribute(AttributeObject.TotalValue, AttributeObject.Abbrev);
            int intSpecRating = Specializations.Count == 0 || CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && x.UniqueName == Name && string.IsNullOrEmpty(x.Condition) && x.Enabled)
                ? intRating
                : (!IsKnowledgeSkill && Name == "Artisan" &&
                   CharacterObject.Qualities.Any(objQuality => objQuality.Name == "Inspired")
                    ? intRating + 3
                    : intRating + 2);

            int intRatingModifiers = RatingModifiers(AttributeObject.Abbrev);
            int intDicePoolModifiers = PoolModifiers(AttributeObject.Abbrev);

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
            objWriter.WriteElementString("total", PoolOtherAttribute(AttributeObject.TotalValue, AttributeObject.Abbrev).ToString(objCulture));
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
            objWriter.WriteElementString("bp", CurrentKarmaCost().ToString(objCulture));
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
            XmlDocument skills = XmlManager.Load("skills.xml");
            Skill skill = null;
            bool blnIsKnowledgeSkill = false;
            if (xmlSkillNode.TryGetBoolFieldQuickly("isknowledge", ref blnIsKnowledgeSkill) && blnIsKnowledgeSkill)
            {
                if (xmlSkillNode["forced"] != null)
                    skill = new KnowledgeSkill(objCharacter, xmlSkillNode["name"]?.InnerText ?? string.Empty);
                else
                {
                    KnowledgeSkill knoSkill = new KnowledgeSkill(objCharacter);
                    knoSkill.Load(xmlSkillNode);
                    skill = knoSkill;
                }
            }
            else if (suid != Guid.Empty)
            {
                XmlNode node = skills.SelectSingleNode($"/chummer/skills/skill[id = '{xmlSkillNode["suid"]?.InnerText}']");

                if (node == null) return null;

                if (node["exotic"]?.InnerText == bool.TrueString)
                {
                    ExoticSkill exotic = new ExoticSkill(objCharacter, node);
                    exotic.Load(xmlSkillNode);
                    skill = exotic;
                }
                else
                {
                    skill = new Skill(objCharacter, node);
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
            if (skill == null)
            {
                if (xmlSkillNode["forced"] != null)
                    skill = new KnowledgeSkill(objCharacter, xmlSkillNode["name"]?.InnerText ?? string.Empty);
                else
                {
                    KnowledgeSkill knoSkill = new KnowledgeSkill(objCharacter);
                    knoSkill.Load(xmlSkillNode);
                    skill = knoSkill;
                }
            }
            if (xmlSkillNode.TryGetField("guid", Guid.TryParse, out Guid guiTemp))
                skill.Id = guiTemp;

            xmlSkillNode.TryGetInt32FieldQuickly("karma", ref skill._intKarma);
            xmlSkillNode.TryGetInt32FieldQuickly("base", ref skill._intBase);
            xmlSkillNode.TryGetBoolFieldQuickly("buywithkarma", ref skill._blnBuyWithKarma);
            if (!xmlSkillNode.TryGetStringFieldQuickly("altnotes", ref skill._strNotes))
                xmlSkillNode.TryGetStringFieldQuickly("notes", ref skill._strNotes);

            using (XmlNodeList xmlSpecList = xmlSkillNode.SelectNodes("specs/spec"))
                if (xmlSpecList != null)
                    foreach (XmlNode xmlSpec in xmlSpecList)
                    {
                        skill.Specializations.Add(SkillSpecialization.Load(xmlSpec, skill));
                    }

            return skill;
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
            _objCharacter = character;
            _objCharacter.PropertyChanged += OnCharacterChanged;

            Specializations.ListChanged += SpecializationsOnListChanged;
        }

        public void UnbindSkill()
        {
            _objCharacter.PropertyChanged -= OnCharacterChanged;
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
            AttributeObject = CharacterObject.GetAttribute(xmlNode["attribute"]?.InnerText);
            _strCategory = xmlNode["category"]?.InnerText ?? string.Empty;
            Default = xmlNode["default"]?.InnerText == bool.TrueString;
            Source = xmlNode["source"]?.InnerText;
            Page = xmlNode["page"]?.InnerText;
            if (xmlNode.TryGetField("id", Guid.TryParse, out Guid guiTemp))
                SkillId = guiTemp;
            else if (xmlNode.TryGetField("suid", Guid.TryParse, out guiTemp))
                SkillId = guiTemp;
            if (xmlNode.TryGetField("guid", Guid.TryParse, out guiTemp))
                Id = guiTemp;
            
            XmlNodeList lstSuggestedSpecializationsXml = xmlNode["specs"]?.ChildNodes;
            if (lstSuggestedSpecializationsXml != null)
            {
                SuggestedSpecializations.Capacity = lstSuggestedSpecializationsXml.Count;
                foreach (XmlNode node in lstSuggestedSpecializationsXml)
                {
                    string strInnerText = node.InnerText;
                    SuggestedSpecializations.Add(new ListItem(strInnerText, node.Attributes?["translate"]?.InnerText ?? strInnerText));
                }
            }

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
        }

        #endregion

        /// <summary>
        /// The total, general pourpose dice pool for this skill
        /// </summary>
        public int Pool => PoolOtherAttribute(AttributeObject.TotalValue, AttributeObject.Abbrev);

        public bool Leveled => Rating > 0;

        public bool CanHaveSpecs
        {
            get
            {
                ExoticSkill objThisAsExoticSkill = IsExoticSkill ? this as ExoticSkill : null;
                return TotalBaseRating > 0 && KarmaUnlocked &&
                    !_objCharacter.Improvements.Any(x => ((x.ImproveType == Improvement.ImprovementType.BlockSkillSpecializations &&
                                                           (string.IsNullOrEmpty(x.ImprovedName) || x.ImprovedName == Name || (objThisAsExoticSkill != null && x.ImprovedName == Name + " (" + objThisAsExoticSkill.Specific + ')'))) ||
                                                          (x.ImproveType == Improvement.ImprovementType.BlockSkillCategorySpecializations && x.ImprovedName == SkillCategory)) && x.Enabled);
            }
        }

        public Character CharacterObject => _objCharacter;

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
            return LanguageManager.GetString($"String_Attribute{AttributeObject.Abbrev}Short", strLanguage);
        }

        public string DisplayAttribute => DisplayAttributeMethod(GlobalOptions.Language);

        private bool _blnOldEnable = true; //For OnPropertyChanged

        private bool _blnEnabled = true;
        //TODO handle aspected/adepts who cannot (always) get magic skills
        public bool Enabled
        {
            get
            {
                if (!_blnEnabled)
                    return false;
                if (Name.Contains("Flight"))
                {
                    string strFlyString = CharacterObject.GetFly(GlobalOptions.InvariantCultureInfo, GlobalOptions.DefaultLanguage);
                    if (string.IsNullOrEmpty(strFlyString) || strFlyString == "0" || strFlyString.Contains(LanguageManager.GetString("String_ModeSpecial", GlobalOptions.DefaultLanguage)))
                        return false;
                }
                if (Name.Contains("Swimming"))
                {
                    string strSwimString = CharacterObject.GetSwim(GlobalOptions.InvariantCultureInfo, GlobalOptions.DefaultLanguage);
                    if (string.IsNullOrEmpty(strSwimString) || strSwimString == "0" || strSwimString.Contains(LanguageManager.GetString("String_ModeSpecial", GlobalOptions.DefaultLanguage)))
                        return false;
                }
                if (Name.Contains("Running"))
                {
                    string strMovementString = CharacterObject.GetMovement(GlobalOptions.InvariantCultureInfo, GlobalOptions.DefaultLanguage);
                    if (string.IsNullOrEmpty(strMovementString) || strMovementString == "0" || strMovementString.Contains(LanguageManager.GetString("String_ModeSpecial", GlobalOptions.DefaultLanguage)))
                        return false;
                }
                //TODO: This is a temporary workaround until proper support for selectively enabling or disabling skills works, as above.
                if (CharacterObject.Metatype == "A.I.")
                {
                    return !(AttributeObject.Abbrev == "MAG" || AttributeObject.Abbrev == "RES");
                }
                else
                {
                    return AttributeObject.Value != 0;
                }
            }
            set
            {
                if (_blnEnabled != value)
                {
                    _blnEnabled = value;
                    OnPropertyChanged(nameof(Enabled));
                    _blnOldEnable = Enabled;
                }
            }
        }

        private bool _blnOldUpgrade;

        public bool CanUpgradeCareer => CharacterObject.Karma >= UpgradeKarmaCost() && RatingMaximum > TotalBaseRating;

        public virtual bool AllowDelete => false;

        public bool Default
        {
            get
            {
                return _blnDefault && !RelevantImprovements(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.BlockSkillDefault).Any();
            }
            set => _blnDefault = value;
        }

        public virtual bool IsExoticSkill => false;

        public virtual bool IsKnowledgeSkill => false;

        public string Name
        {
            get => _strName;
            set
            {
                if (value != _strName)
                {
                    _strName = value;
                    _intCachedFreeBase = int.MinValue;
                    _intCachedFreeKarma = int.MinValue;
                }
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
                if (_guidSkillId != value)
                {
                    _guidSkillId = value;
                    _objCachedMyXmlNode = null;
                }
            }
        }

        public string SkillGroup { get; } = string.Empty;

        public virtual string SkillCategory => _strCategory;

        public IReadOnlyList<ListItem> CGLSpecializations => SuggestedSpecializations;

        private readonly Dictionary<string, string> _cachedStringSpec = new Dictionary<string, string>();
        public virtual string DisplaySpecializationMethod(string strLanguage)
        {
            if (!_cachedStringSpec.TryGetValue(strLanguage, out string strReturn))
            {
                strReturn = string.Join(", ", Specializations.Select(x => x.DisplayName(strLanguage)));

                _cachedStringSpec.Add(strLanguage, strReturn);
            }

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

                if (Specializations.Count > 0)
                {
                    return Specializations[0].DisplayName(GlobalOptions.Language);
                }

                return string.Empty;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    int index = -1;
                    for (int i = 0; i < Specializations.Count; i++)
                    {
                        if (!Specializations[i].Free)
                        {
                            index = i;
                            break;
                        }
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
            return Specializations.Any(x => (x.Name == strSpecialization || x.DisplayName(GlobalOptions.Language) == strSpecialization)) && !CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && x.UniqueName == Name && string.IsNullOrEmpty(x.Condition) && x.Enabled);
        }

        public string PoolToolTip
        {
            get
            {
                if (!Default && !Leveled)
                {
                    return LanguageManager.GetString("Tip_Skill_Cannot_Default", GlobalOptions.Language);
                }

                IList<Improvement> lstRelevantImprovements = RelevantImprovements().ToList();

                StringBuilder s;
                if (CyberwareRating > TotalBaseRating)
                {
                    s = new StringBuilder($"{LanguageManager.GetString("Tip_Skill_SkillsoftRating", GlobalOptions.Language)} ({CyberwareRating})");
                }
                else
                {
                    s = new StringBuilder($"{LanguageManager.GetString("Tip_Skill_SkillRating", GlobalOptions.Language)} ({Rating}");


                    bool first = true;
                    foreach (Improvement objImprovement in lstRelevantImprovements.Where(x => x.AddToRating))
                    {
                        if (first)
                        {
                            first = false;

                            s.Append(" (Base (");
                            s.Append(LearnedRating.ToString());
                            s.Append(')');
                        }

                        s.Append(" + ");
                        s.Append(CharacterObject.GetObjectName(objImprovement, GlobalOptions.Language));
                        s.Append(" (");
                        s.Append(objImprovement.Value.ToString());
                        s.Append(')');
                    }
                    if (!first) s.Append(')');

                    s.Append(')');
                }

                s.Append($" + {DisplayAttribute} ({AttributeModifiers})");

                if (Default && !Leveled)
                {
                    s.Append(DefaultModifier == 0
                        ? ' ' + CharacterObject.GetObjectName(CharacterObject.Improvements.FirstOrDefault(x => x.ImproveType == Improvement.ImprovementType.ReflexRecorderOptimization && x.Enabled), GlobalOptions.Language) + ' '
                        : $" - {LanguageManager.GetString("Tip_Skill_Defaulting", GlobalOptions.Language)} (1)");
                }

                foreach (Improvement source in lstRelevantImprovements.Where(x => !x.AddToRating && x.ImproveType != Improvement.ImprovementType.SwapSkillAttribute && x.ImproveType != Improvement.ImprovementType.SwapSkillSpecAttribute))
                {
                    s.Append(" + ");
                    s.Append(CharacterObject.GetObjectName(source, GlobalOptions.Language));
                    s.Append(" (");
                    s.Append(source.Value.ToString());
                    s.Append(')');
                }


                int wound = _objCharacter.WoundModifier;
                if (wound != 0)
                {
                    s.Append(" - " + LanguageManager.GetString("Tip_Skill_Wounds", GlobalOptions.Language) + " (" + wound.ToString() + ')');
                }

                if (AttributeObject.Abbrev == "STR" || AttributeObject.Abbrev == "AGI")
                {
                    foreach (Cyberware cyberware in _objCharacter.Cyberware.Where(x => x.Name.Contains(" Arm") || x.Name.Contains(" Hand")))
                    {
                        s.Append(Environment.NewLine);
                        s.AppendFormat("{0} {1} ", cyberware.Location, cyberware.DisplayNameShort(GlobalOptions.Language));
                        if (cyberware.Grade.Name != "Standard")
                        {
                            s.AppendFormat("({0}) ", cyberware.Grade.DisplayName(GlobalOptions.Language));
                        }

                        int pool = PoolOtherAttribute(Attribute == "STR" ? cyberware.TotalStrength : cyberware.TotalAgility, Attribute);

                        if (cyberware.Location == CharacterObject.PrimaryArm || CharacterObject.Ambidextrous || cyberware.LimbSlotCount > 1)
                        {
                            s.Append(pool);
                        }
                        else
                        {
                            s.AppendFormat("{0} (-2 Off Hand)", pool - 2);
                        }
                    }
                }

                foreach (Improvement objSwapSkillAttribute in lstRelevantImprovements.Where(x => x.ImproveType == Improvement.ImprovementType.SwapSkillAttribute || x.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute))
                {
                    s.Append(Environment.NewLine);
                    if (objSwapSkillAttribute.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute)
                        s.AppendFormat("{0}: ", objSwapSkillAttribute.Exclude);
                    s.AppendFormat("{0} ", CharacterObject.GetObjectName(objSwapSkillAttribute, GlobalOptions.Language));

                    int intLoopAttribute = CharacterObject.GetAttribute(objSwapSkillAttribute.ImprovedName).Value;
                    int intBasePool = PoolOtherAttribute(intLoopAttribute, objSwapSkillAttribute.ImprovedName);
                    bool blnHaveSpec = false;

                    if (objSwapSkillAttribute.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute &&
                        Specializations.Any(y => y.Name == objSwapSkillAttribute.Exclude && !CharacterObject.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && objImprovement.UniqueName == y.Name && string.IsNullOrEmpty(objImprovement.Condition) && objImprovement.Enabled)))
                    {
                        intBasePool += 2;
                        blnHaveSpec = true;
                    }
                    s.Append(intBasePool.ToString());

                    if (objSwapSkillAttribute.ImprovedName == "STR" || objSwapSkillAttribute.ImprovedName == "AGI")
                    {
                        foreach (Cyberware cyberware in _objCharacter.Cyberware.Where(x => x.Name.Contains(" Arm") || x.Name.Contains(" Hand")))
                        {
                            s.Append(Environment.NewLine);
                            if (objSwapSkillAttribute.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute)
                                s.AppendFormat("{0}: ", objSwapSkillAttribute.Exclude);
                            s.AppendFormat("{0} ", CharacterObject.GetObjectName(objSwapSkillAttribute, GlobalOptions.Language));
                            s.AppendFormat("{0} {1} ", cyberware.Location, cyberware.DisplayNameShort(GlobalOptions.Language));
                            if (cyberware.Grade.Name != "Standard")
                            {
                                s.AppendFormat("({0}) ", cyberware.Grade.DisplayName(GlobalOptions.Language));
                            }

                            int intLoopPool = PoolOtherAttribute(objSwapSkillAttribute.ImprovedName == "STR" ? cyberware.TotalStrength : cyberware.TotalAgility, objSwapSkillAttribute.ImprovedName);
                            if (blnHaveSpec)
                            {
                                intLoopPool += 2;
                            }

                            if (cyberware.Location == CharacterObject.PrimaryArm || CharacterObject.Ambidextrous || cyberware.LimbSlotCount > 1)
                            {
                                s.Append(intLoopPool);
                            }
                            else
                            {
                                s.AppendFormat("{0} (-2 Off Hand)", intLoopPool - 2);
                            }
                        }
                    }
                }

                return s.ToString();
            }
        }

        public string UpgradeToolTip => string.Format(LanguageManager.GetString("Tip_ImproveItem", GlobalOptions.Language), (Rating + 1), UpgradeKarmaCost());

        public string AddSpecToolTip
        {
            get
            {
                int price = IsKnowledgeSkill ? CharacterObject.Options.KarmaKnowledgeSpecialization : CharacterObject.Options.KarmaSpecialization;

                int intExtraSpecCost = 0;
                int intTotalBaseRating = TotalBaseRating;
                decimal decSpecCostMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if (objLoopImprovement.Minimum <= intTotalBaseRating &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCost)
                                intExtraSpecCost += objLoopImprovement.Value;
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier)
                                decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                    }
                }
                if (decSpecCostMultiplier != 1.0m)
                    price = decimal.ToInt32(decimal.Ceiling(price * decSpecCostMultiplier));
                price += intExtraSpecCost; //Spec
                return string.Format(LanguageManager.GetString("Tip_Skill_AddSpecialization", GlobalOptions.Language), price.ToString());
            }
        }

        public string SkillToolTip
        {
            get
            {
                //v-- hack i guess
                string strReturn = string.Empty;
                string middle = string.Empty;
                if (!string.IsNullOrWhiteSpace(SkillGroup))
                {
                    middle = $"{SkillGroup} {LanguageManager.GetString("String_ExpenseSkillGroup", GlobalOptions.Language)}" + Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(_strNotes))
                {
                    strReturn = LanguageManager.GetString("Label_Notes", GlobalOptions.Language) + ' ' + _strNotes.WordWrap(100) + Environment.NewLine + Environment.NewLine;
                }

                strReturn += $"{DisplayCategory(GlobalOptions.Language)}{Environment.NewLine}{middle}{CommonFunctions.LanguageBookLong(Source, GlobalOptions.Language)} {LanguageManager.GetString("String_Page", GlobalOptions.Language)} {DisplayPage(GlobalOptions.Language)}";

                return strReturn;
            }
        }

        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        public SkillGroup SkillGroupObject { get; }

        public string Page { get; private set; }

        public string DisplayPage(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Page;

            return GetNode()?["altpage"]?.InnerText ?? Page;
        }

        public string Source { get; private set; }

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

        public virtual string DisplayPool => DisplayOtherAttribute(AttributeObject.TotalValue, AttributeObject.Abbrev);

        public string DisplayOtherAttribute(int intAttributeTotalValue, string strAttribute)
        {
            int pool = PoolOtherAttribute(intAttributeTotalValue, strAttribute);

            if (string.IsNullOrWhiteSpace(Specialization) || CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && x.UniqueName == Name && string.IsNullOrEmpty(x.Condition) && x.Enabled))
            {
                return pool.ToString();
            }
            else
            {
                //Handler for the Inspired Quality.
                if (!IsKnowledgeSkill && Name == "Artisan")
                {
                    if (CharacterObject.Qualities.Any(objQuality => objQuality.Name == "Inspired"))
                    {
                        return $"{pool} ({pool + 3})";
                    }
                }
                else if (IsExoticSkill)
                {
                    return $"{pool}";
                }
                return $"{pool} ({pool + 2})";
            }
        }

        public int CachedWareRating { get; set; } = int.MinValue;

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
                _objCachedMyXmlNode = XmlManager.Load("skills.xml", strLanguage).SelectSingleNode("/chummer/" + (IsKnowledgeSkill ? "knowledgeskills" : "skills") + "/skill[id = \"" + SkillId.ToString() + "\"]");
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
                if (CachedWareRating != int.MinValue)
                    return CachedWareRating;

                //TODO: method is here, but not used in any form, needs testing (worried about child items...)
                //this might do hardwires if i understand how they works correctly
                int intMaxHardwire = -1;
                foreach (Improvement objImprovement in CharacterObject.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.Hardwire && objImprovement.Enabled)
                    {
                        if (this is ExoticSkill objExoticSkill)
                        {
                            if (objImprovement.ImprovedName == Name + " (" + objExoticSkill.Specific + ')')
                                intMaxHardwire = Math.Max(intMaxHardwire, objImprovement.Value);
                        }
                        else if (objImprovement.ImprovedName == Name)
                            intMaxHardwire = Math.Max(intMaxHardwire, objImprovement.Value);
                    }
                }
                if (intMaxHardwire >= 0)
                {
                    return CachedWareRating = intMaxHardwire;
                }

                int intMaxActivesoftRating = Math.Min(IsKnowledgeSkill ? int.MaxValue : ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.Skillwire), ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillsoftAccess));
                if (intMaxActivesoftRating > 0)
                {
                    int intMax = 0;
                    //TODO this works with translate?
                    foreach (Improvement objSkillsoftImprovement in CharacterObject.Improvements)
                    {
                        if (objSkillsoftImprovement.ImproveType == Improvement.ImprovementType.Activesoft && objSkillsoftImprovement.Enabled)
                        {
                            if (this is ExoticSkill objExoticSkill)
                            {
                                if (objSkillsoftImprovement.ImprovedName == Name + " (" + objExoticSkill.Specific + ')')
                                    intMaxHardwire = Math.Max(intMaxHardwire, objSkillsoftImprovement.Value);
                            }
                            else if (objSkillsoftImprovement.ImprovedName == Name)
                                intMax = Math.Max(intMax, objSkillsoftImprovement.Value);
                        }
                    }
                    return CachedWareRating = Math.Min(intMax, intMaxActivesoftRating);
                }

                return CachedWareRating = 0;
            }
        }
        #endregion

        #region Static

        //A tree of dependencies. Once some of the properties are changed,
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly DependancyGraph<string> SkillDependencyGraph =
            new DependancyGraph<string>(
                new DependancyGraphNode<string>(nameof(PoolToolTip),
                    new DependancyGraphNode<string>(nameof(DisplayPool),
                        new DependancyGraphNode<string>(nameof(Pool),
                            new DependancyGraphNode<string>(nameof(PoolModifiers)),
                            new DependancyGraphNode<string>(nameof(AttributeModifiers)),
                            new DependancyGraphNode<string>(nameof(CanHaveSpecs),
                                new DependancyGraphNode<string>(nameof(Leveled),
                                    new DependancyGraphNode<string>(nameof(Rating),
                                        new DependancyGraphNode<string>(nameof(CyberwareRating)),
                                        new DependancyGraphNode<string>(nameof(TotalBaseRating),
                                            new DependancyGraphNode<string>(nameof(RatingModifiers)),
                                            new DependancyGraphNode<string>(nameof(LearnedRating),
                                                new DependancyGraphNode<string>(nameof(KarmaUnlocked),
                                                    new DependancyGraphNode<string>(nameof(Karma),
                                                        new DependancyGraphNode<string>(nameof(FreeKarma)),
                                                        new DependancyGraphNode<string>(nameof(RatingMaximum)),
                                                        new DependancyGraphNode<string>(nameof(Base))
                                                    )
                                                ),
                                                new DependancyGraphNode<string>(nameof(BaseUnlocked),
                                                    new DependancyGraphNode<string>(nameof(Base),
                                                        new DependancyGraphNode<string>(nameof(FreeBase)),
                                                        new DependancyGraphNode<string>(nameof(RatingMaximum))
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                ),
                new DependancyGraphNode<string>(nameof(UpgradeToolTip),
                    new DependancyGraphNode<string>(nameof(UpgradeKarmaCost),
                        new DependancyGraphNode<string>(nameof(Rating))
                    )
                ),
                new DependancyGraphNode<string>(nameof(CanUpgradeCareer),
                    new DependancyGraphNode<string>(nameof(UpgradeKarmaCost)),
                    new DependancyGraphNode<string>(nameof(RatingMaximum)),
                    new DependancyGraphNode<string>(nameof(TotalBaseRating))
                ),
                new DependancyGraphNode<string>(nameof(DisplaySpecialization),
                    new DependancyGraphNode<string>(nameof(Specialization))
                )
            );
        #endregion
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            ICollection<string> lstNamesOfChangedProperties = SkillDependencyGraph.GetWithAllDependants(propertyName);
            if (lstNamesOfChangedProperties.Contains(nameof(FreeBase)))
                _intCachedFreeBase = int.MinValue;
            if (lstNamesOfChangedProperties.Contains(nameof(FreeKarma)))
                _intCachedFreeKarma = int.MinValue;
            if (lstNamesOfChangedProperties.Contains(nameof(CyberwareRating)))
                CachedWareRating = int.MinValue;
            if (PropertyChanged != null)
            {
                foreach (string strPropertyToChange in lstNamesOfChangedProperties)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                }
            }
        }

        private void OnSkillGroupChanged(object sender, PropertyChangedEventArgs propertyChangedEventArg)
        {
            if (propertyChangedEventArg.PropertyName == nameof(Skills.SkillGroup.Base))
            {
                OnPropertyChanged(nameof(Base));
                KarmaSpecForcedMightChange();
            }
            else if (propertyChangedEventArg.PropertyName == nameof(Skills.SkillGroup.Karma))
            {
                OnPropertyChanged(nameof(Karma));
                KarmaSpecForcedMightChange();
            }
        }

        private void OnCharacterChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Character.Karma))
            {
                if (_blnOldUpgrade != CanUpgradeCareer)
                {
                    _blnOldUpgrade = CanUpgradeCareer;
                    OnPropertyChanged(nameof(CanUpgradeCareer));
                }
                if (_oldCanAffordSpecialization != CanAffordSpecialization)
                {
                    _oldCanAffordSpecialization = CanAffordSpecialization;
                    OnPropertyChanged(nameof(CanAffordSpecialization));
                }
            }

            if (propertyChangedEventArgs.PropertyName == nameof(Character.WoundModifier))
            {
                OnPropertyChanged(nameof(Pool));
            }
        }

        protected void OnLinkedAttributeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
                OnPropertyChanged(nameof(AttributeModifiers));
            else if (e.PropertyName == nameof(CharacterAttrib.Value) || e.PropertyName == nameof(CharacterAttrib.Abbrev))
            {
                if (Enabled != _blnOldEnable)
                {
                    OnPropertyChanged(nameof(Enabled));
                    _blnOldEnable = Enabled;
                }
            }
        }

        private void SpecializationsOnListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            _cachedStringSpec.Clear();
            OnPropertyChanged(nameof(Specialization));
        }
    }
}
