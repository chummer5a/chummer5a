using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Chummer.Annotations;
using Chummer.Backend;
using Chummer.Backend.Equipment;
using Chummer.Datastructures;
using Chummer.Backend.Attributes;

namespace Chummer.Skills
{
    [DebuggerDisplay("{_name} {_base} {_karma}")]
    public partial class Skill : INotifyPropertyChanged
    {
        /// <summary>
        /// Called during save to allow derived classes to save additional infomation required to rebuild state
        /// </summary>
        /// <param name="writer"></param>
        protected virtual void SaveExtendedData(XmlTextWriter writer)
        {
        }

        public CharacterAttrib AttributeObject { get; protected set; } //Attribute this skill primarily depends on
        private readonly Character _character; //The Character (parent) to this skill
        protected readonly string Category; //Name of the skill category it belongs to
        private readonly string _group; //Name of the skill group this skill belongs to (remove?)
        private string _name = string.Empty; //English name of this skill
        private string _strNotes; //Text of any notes that were entered by the user
        protected List<ListItem> SuggestedSpecializations; //List of suggested specializations for this skill
        private readonly string _translatedName = null;
        private string _translatedCategory = null;
        private bool _blnDefault;

        public void WriteTo(XmlTextWriter writer)
        {
            writer.WriteStartElement("skill");
            writer.WriteElementString("guid", Id.ToString());
            writer.WriteElementString("suid", SkillId.ToString());
            writer.WriteElementString("isknowledge", IsKnowledgeSkill.ToString());
            writer.WriteElementString("skillcategory", SkillCategory);
            writer.WriteElementString("karma", _karma.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("base", _base.ToString(CultureInfo.InvariantCulture)); //this could acctually be saved in karma too during career
            writer.WriteElementString("notes", _strNotes);
            if (!CharacterObject.Created)
            {
                writer.WriteElementString("buywithkarma", _buyWithKarma.ToString());
            }

            if (Specializations.Count != 0)
            {
                writer.WriteStartElement("specs");
                foreach (SkillSpecialization specialization in Specializations)
                {
                    specialization.Save(writer);
                }
                writer.WriteEndElement();
            }

            SaveExtendedData(writer);

            writer.WriteEndElement();

        }

        public void Print(XmlTextWriter objWriter, CultureInfo objCulture)
        {
            KnowledgeSkill objSkillAsKnowledgeSkill = this as KnowledgeSkill;
            objWriter.WriteStartElement("skill");

            int rating = PoolOtherAttribute(AttributeObject.TotalValue);
            int specRating = Specializations.Count == 0 || CharacterObject.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && objImprovement.UniqueName == Name && string.IsNullOrEmpty(objImprovement.Condition))
                ? rating
                : (!IsKnowledgeSkill && Name == "Artisan" &&
                   CharacterObject.Qualities.Any(objQuality => objQuality.Name == "Inspired")
                    ? rating + 3
                    : rating + 2);

            int ratingModifiers = RatingModifiers, dicePoolModifiers = PoolModifiers;

            if (objSkillAsKnowledgeSkill == null)
                objWriter.WriteElementString("name", DisplayName);
            else
                objWriter.WriteElementString("name", objSkillAsKnowledgeSkill.WriteableName);
            objWriter.WriteElementString("skillgroup", SkillGroupObject?.DisplayName ?? LanguageManager.GetString("String_None"));
            objWriter.WriteElementString("skillgroup_english", SkillGroupObject?.Name ?? LanguageManager.GetString("String_None"));
            objWriter.WriteElementString("skillcategory", DisplayCategory);
            objWriter.WriteElementString("skillcategory_english", SkillCategory);  //Might exist legacy but not existing atm, will see if stuff breaks
            objWriter.WriteElementString("grouped", (SkillGroupObject != null && SkillGroupObject.CareerIncrease && SkillGroupObject.Rating > 0).ToString());
            objWriter.WriteElementString("default", Default.ToString());
            objWriter.WriteElementString("rating", Rating.ToString(objCulture));
            objWriter.WriteElementString("ratingmax", RatingMaximum.ToString(objCulture));
            objWriter.WriteElementString("specializedrating", specRating.ToString(objCulture));
            objWriter.WriteElementString("specbonus", (specRating - rating).ToString(objCulture));
            objWriter.WriteElementString("total", PoolOtherAttribute(AttributeObject.TotalValue).ToString(objCulture));
            objWriter.WriteElementString("knowledge", IsKnowledgeSkill.ToString());
            objWriter.WriteElementString("exotic", IsExoticSkill.ToString());
            objWriter.WriteElementString("buywithkarma", BuyWithKarma.ToString());
            objWriter.WriteElementString("base", Base.ToString(objCulture));
            objWriter.WriteElementString("karma", Karma.ToString(objCulture));
            objWriter.WriteElementString("spec", DisplaySpecialization);
            objWriter.WriteElementString("attribute", Attribute);
            objWriter.WriteElementString("displayattribute", DisplayAttribute);
            if (CharacterObject.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("source", CharacterObject.Options.LanguageBookShort(Source));
            objWriter.WriteElementString("page", Page);
            objWriter.WriteElementString("attributemod", CharacterObject.GetAttribute(Attribute).TotalValue.ToString(objCulture));
            objWriter.WriteElementString("ratingmod", (ratingModifiers + dicePoolModifiers).ToString(objCulture));
            objWriter.WriteElementString("poolmod", dicePoolModifiers.ToString(objCulture));
            objWriter.WriteElementString("islanguage", (SkillCategory == "Language").ToString());
            objWriter.WriteElementString("bp", CurrentKarmaCost().ToString(objCulture));
            objWriter.WriteStartElement("skillspecializations");
            foreach (SkillSpecialization objSpec in Specializations)
            {
                objSpec.Print(objWriter);
            }
            objWriter.WriteEndElement();

            objWriter.WriteEndElement();
        }

        #region Factory
        /// <summary>
        /// Load a skill from a xml node from a saved .chum5 file
        /// </summary>
        /// <param name="n">The XML node describing the skill</param>
        /// <param name="character">The character this skill belongs to</param>
        /// <returns></returns>
        public static Skill Load(Character character, XmlNode n)
        {
            if (n?["suid"] == null) return null;

            Guid suid;
            if (!Guid.TryParse(n["suid"].InnerText, out suid))
            {
                return null;
            }
            XmlDocument skills = XmlManager.Load("skills.xml");
            Skill skill = null;
            bool blnIsKnowledgeSkill = false;
            if (n.TryGetBoolFieldQuickly("isknowledge", ref blnIsKnowledgeSkill) && blnIsKnowledgeSkill)
            {
                if (n["forced"] != null)
                    skill = new KnowledgeSkill(character, n["name"]?.InnerText ?? string.Empty);
                else
                {
                    KnowledgeSkill knoSkill = new KnowledgeSkill(character);
                    knoSkill.Load(n);
                    skill = knoSkill;
                }
            }
            else if (suid != Guid.Empty)
            {
                XmlNode node = skills.SelectSingleNode($"/chummer/skills/skill[id = '{n["suid"].InnerText}']");

                if (node == null) return null;

                if (node["exotic"]?.InnerText == "Yes")
                {
                    ExoticSkill exotic = new ExoticSkill(character, node);
                    exotic.Load(n);
                    skill = exotic;
                }
                else
                {
                    skill = new Skill(character, node);
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
                if (n["forced"] != null)
                    skill = new KnowledgeSkill(character, n["name"]?.InnerText ?? string.Empty);
                else
                {
                    KnowledgeSkill knoSkill = new KnowledgeSkill(character);
                    knoSkill.Load(n);
                    skill = knoSkill;
                }
            }
            XmlElement element = n["guid"];
            if (element != null) skill.Id = Guid.Parse(element.InnerText);

            n.TryGetInt32FieldQuickly("karma", ref skill._karma);
            n.TryGetInt32FieldQuickly("base", ref skill._base);
            n.TryGetBoolFieldQuickly("buywithkarma", ref skill._buyWithKarma);
            n.TryGetStringFieldQuickly("notes", ref skill._strNotes);

            foreach (XmlNode spec in n.SelectNodes("specs/spec"))
            {
                skill.Specializations.Add(SkillSpecialization.Load(spec));
            }
            XmlNode objCategoryNode = skills.SelectSingleNode($"/chummer/categories/category[. = '{skill.SkillCategory}']");

            skill.DisplayCategory = objCategoryNode?.Attributes?["translate"]?.InnerText ?? skill.SkillCategory;

            return skill;
        }

        /// <summary>
        /// Loads skill saved in legacy format
        /// </summary>
        /// <param name="character"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Skill LegacyLoad(Character character, XmlNode n)
        {
            if (n == null)
                return null;
            Guid suid;
            Skill skill;

            n.TryGetField("id", Guid.TryParse, out suid, Guid.NewGuid());

            int baseRating;
            int.TryParse(n["base"]?.InnerText, out baseRating);
            int fullRating;
            int.TryParse(n["rating"]?.InnerText, out fullRating);
            int karmaRating = fullRating - baseRating;  //Not reading karma directly as career only increases rating

            bool blnTemp = false;

            if (n.TryGetBoolFieldQuickly("knowledge", ref blnTemp) && blnTemp)
            {
                KnowledgeSkill kno = new KnowledgeSkill(character);
                kno.WriteableName = n["name"]?.InnerText;
                kno.Base = baseRating;
                kno.Karma = karmaRating;

                kno.Type = n["skillcategory"]?.InnerText;

                skill = kno;
            }
            else
            {
                XmlNode data =
                    XmlManager.Load("skills.xml").SelectSingleNode($"/chummer/skills/skill[id = '{suid}']");

                //Some stuff apparently have a guid of 0000-000... (only exotic?)
                if (data == null)
                {
                    data = XmlManager.Load("skills.xml")
                        .SelectSingleNode($"/chummer/skills/skill[name = '{n["name"]?.InnerText}']");
                }


                skill = FromData(data, character);
                skill._base = baseRating;
                skill._karma = karmaRating;

                ExoticSkill exoticSkill = skill as ExoticSkill;
                if (exoticSkill != null)
                {
                    string name = n.SelectSingleNode("skillspecializations/skillspecialization/name")?.InnerText ?? string.Empty;
                    //don't need to do more load then.
                    exoticSkill.Specific = name;
                    return skill;
                }

                n.TryGetBoolFieldQuickly("buywithkarma", ref skill._buyWithKarma);
            }

            var v = from XmlNode node
                in n.SelectNodes("skillspecializations/skillspecialization")
                    select SkillSpecialization.Load(node);
            var q = v.ToList();
            if (q.Count != 0)
            {
                skill.Specializations.AddRange(q);
            }

            return skill;
        }

        protected static readonly Dictionary<string, bool> SkillTypeCache = new Dictionary<string, bool>();
        //TODO CACHE INVALIDATE

        /// <summary>
        /// Load a skill from a data file describing said skill
        /// </summary>
        /// <param name="n">The XML node describing the skill</param>
        /// <param name="character">The character the skill belongs to</param>
        /// <returns></returns>
        public static Skill FromData(XmlNode n, Character character)
        {
            if (n == null)
                return null;
            Skill s;
            if (n["exotic"]?.InnerText == "Yes")
            {
                //load exotic skill
                ExoticSkill s2 = new ExoticSkill(character, n);
                s = s2;
            }
            else
            {
                XmlDocument document = XmlManager.Load("skills.xml");
                XmlNode knoNode = null;
                string category = n["category"]?.InnerText;
                if (string.IsNullOrEmpty(category))
                    return null;
                bool knoSkill;
                if (SkillTypeCache == null || !SkillTypeCache.TryGetValue(category, out knoSkill))
                {
                    knoNode = document.SelectSingleNode($"/chummer/categories/category[. = '{category}']");
                    knoSkill = knoNode?.Attributes?["type"]?.InnerText != "active";
                    if (SkillTypeCache != null)
                        SkillTypeCache[category] = knoSkill;
                }


                if (knoSkill)
                {
                    //TODO INIT SKILL
                    if (Debugger.IsAttached) Debugger.Break();

                    s = new KnowledgeSkill(character);
                }
                else
                {
                    //TODO INIT SKILL

                    s = new Skill(character, n);
                }
                s.DisplayCategory = knoNode?.Attributes?["translate"]?.InnerText ?? string.Empty;
            }


            return s;
        }

        protected Skill(Character character)
        {
            _character = character;

            _character.PropertyChanged += OnCharacterChanged;

            character.SkillImprovementEvent += OnImprovementEvent;
            Specializations.ListChanged += SpecializationsOnListChanged;
        }


        //load from data
        protected Skill(Character character, XmlNode n) : this(character)
        //Ugly hack, needs by then
        {
            if (n == null)
                return;
            _name = n["name"]?.InnerText; //No need to catch errors (for now), if missing we are fsked anyway
            AttributeObject = CharacterObject.GetAttribute(n["attribute"]?.InnerText);
            Category = n["category"]?.InnerText;
            Default = n["default"]?.InnerText.ToLower() == "yes";
            Source = n["source"]?.InnerText;
            Page = n["page"]?.InnerText;
            if (n["id"] != null)
                SkillId = Guid.Parse(n["id"].InnerText);
            else if (n["suid"] != null)
                SkillId = Guid.Parse(n["suid"].InnerText);
            if (n["guid"] != null)
                Id = Guid.Parse(n["guid"].InnerText);

            _translatedName = n["translate"]?.InnerText;

            AttributeObject.PropertyChanged += OnLinkedAttributeChanged;

            SuggestedSpecializations = new List<ListItem>();
            XmlNodeList lstSuggestedSpecializationsXml = n["specs"]?.ChildNodes;
            if (lstSuggestedSpecializationsXml != null)
            {
                SuggestedSpecializations.Capacity = lstSuggestedSpecializationsXml.Count;
                foreach (XmlNode node in lstSuggestedSpecializationsXml)
                {
                    SuggestedSpecializations.Add(ListItem.AutoXml(node.InnerText, node));
                }
            }

            string strGroup = n["skillgroup"]?.InnerText;

            if (!string.IsNullOrEmpty(strGroup))
            {
                _group = strGroup;
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
        public int Pool
        {
            get { return PoolOtherAttribute(AttributeObject.TotalValue); }
        }

        public bool Leveled
        {
            get { return Rating > 0; }
        }

        public bool CanHaveSpecs
        {
            get
            {
                return TotalBaseRating > 0 && KarmaUnlocked &&
                    !_character.Improvements.Any(x => ((x.ImproveType == Improvement.ImprovementType.BlockSkillSpecializations && (string.IsNullOrEmpty(x.ImprovedName) || x.ImprovedName == Name)) || (x.ImproveType == Improvement.ImprovementType.BlockSkillCategorySpecializations && x.ImprovedName == SkillCategory)) && x.Enabled);
            }
        }

        public Character CharacterObject
        {
            get { return _character; }
        }

        //TODO change to the acctual characterattribute object
        /// <summary>
        /// The Abbreviation of the linke attribute. Not the object due legacy
        /// </summary>
        public string Attribute
        {
            get
            {
                return AttributeObject.Abbrev;
            }
        }
        /// <summary>
        /// The translated abbreviation of the linked attribute.
        /// </summary>
        public string DisplayAttribute
        {
            get
            {
                if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                {
                    return LanguageManager.GetString($"String_Attribute{AttributeObject.Abbrev}Short");
                }
                else
                {
                    return AttributeObject.Abbrev;
                }
            }
        }

        private bool _oldEnable = true; //For OnPropertyChanged

        //TODO handle aspected/adepts who cannot (always) get magic skills
        public bool Enabled
        {
            get
            {
                if (Name.Contains("Flight"))
                {
                    string strFlyString = CharacterObject.GetFly(GlobalOptions.InvariantCultureInfo);
                    if (string.IsNullOrEmpty(strFlyString) || strFlyString == "0" || strFlyString.Contains(LanguageManager.GetString("String_ModeSpecial")))
                        return false;
                }
                if (Name.Contains("Swimming"))
                {
                    string strSwimString = CharacterObject.GetSwim(GlobalOptions.InvariantCultureInfo);
                    if (string.IsNullOrEmpty(strSwimString) || strSwimString == "0" || strSwimString.Contains(LanguageManager.GetString("String_ModeSpecial")))
                        return false;
                }
                if (Name.Contains("Running"))
                {
                    string strMovementString = CharacterObject.GetMovement(GlobalOptions.InvariantCultureInfo);
                    if (string.IsNullOrEmpty(strMovementString) || strMovementString == "0" || strMovementString.Contains(LanguageManager.GetString("String_ModeSpecial")))
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
        }

        private bool _oldUpgrade = false;

        public bool CanUpgradeCareer
        {
            get { return CharacterObject.Karma >= UpgradeKarmaCost() && RatingMaximum > TotalBaseRating; }
        }

        public virtual bool AllowDelete
        {
            get { return false; }
        }

        public bool Default
        {
            get
            {
                return _blnDefault && RelevantImprovements(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.BlockSkillDefault)?.Count == 0;
            }
            set
            {
                _blnDefault = value;
            }
        }

        public virtual bool IsExoticSkill
        {
            get { return false; }
        }

        public virtual bool IsKnowledgeSkill
        {
            get { return false; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        } //I

        //TODO RENAME DESCRIPTIVE
        /// <summary>
        /// The Unique ID for this skill. This is unique and never repeating
        /// </summary>
        public Guid Id { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// The ID for this skill. This is persistent for active skills over
        /// multiple characters, ?and predefined knowledge skills,? but not
        /// for skills where the user supplies a name (Exotic and Knowledge)
        /// </summary>
        public Guid SkillId { get; private set; } = Guid.Empty;

        public string SkillGroup
        {
            get { return _group; }
        }

        public virtual string SkillCategory
        {
            get { return Category; }
        }

        public IReadOnlyList<ListItem> CGLSpecializations
        {
            get { return SuggestedSpecializations; }
        }

        private string _cachedStringSpec = null;
        public virtual string DisplaySpecialization
        {
            get { return _cachedStringSpec = _cachedStringSpec ?? string.Join(", ", Specializations.Select(x => x.DisplayName)); }
        }

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
                    return Specializations[0].DisplayName;
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
                    Specializations.Add(new SkillSpecialization(value, false));
                }
                else
                {
                    if (Specializations[0].Free)
                    {
                        Specializations.MergeInto(new SkillSpecialization(value, false), (x, y) => x.Free == y.Free ? 0 : (x.Free ? 1 : -1));
                    }
                    else
                    {
                        Specializations[0] = new SkillSpecialization(value, false);
                    }
                }
            }
        }

        public string PoolToolTip
        {
            get
            {
                if (!Default && !Leveled)
                {
                    return "You cannot default in this skill";
                    //TODO translate (could not find it in lang file, did not check old source)
                }

                List<Improvement> lstRelevantImprovements = RelevantImprovements() ?? new List<Improvement>();

                StringBuilder s;
                if (CyberwareRating() > TotalBaseRating)
                {
                    s = new StringBuilder($"{LanguageManager.GetString("Tip_Skill_SkillsoftRating")} ({CyberwareRating()})");
                }
                else
                {
                    s = new StringBuilder($"{LanguageManager.GetString("Tip_Skill_SkillRating")} ({Rating}");


                    bool first = true;
                    foreach (Improvement source in lstRelevantImprovements.Where(x => x.AddToRating))
                    {
                        if (first)
                        {
                            first = false;

                            s.Append(" (Base (");
                            s.Append(LearnedRating.ToString());
                            s.Append(")");
                        }

                        s.Append(" + ");
                        s.Append(GetName(source));
                        s.Append(" (");
                        s.Append(source.Value.ToString());
                        s.Append(")");
                    }
                    if (!first) s.Append(")");

                    s.Append(")");
                }

                s.Append($" + {DisplayAttribute} ({AttributeModifiers})");

                if (Default && !Leveled)
                {
                    if (DefaultModifier == 0)
                    {
                        s.Append(" Reflex Recorder Optimization ");
                    }
                    else
                    {
                        s.Append($" - {LanguageManager.GetString("Tip_Skill_Defaulting")} (1)");
                    }

                }

                foreach (Improvement source in lstRelevantImprovements.Where(x => !x.AddToRating && x.ImproveType != Improvement.ImprovementType.SwapSkillAttribute && x.ImproveType != Improvement.ImprovementType.SwapSkillSpecAttribute))
                {
                    s.Append(" + ");
                    s.Append(GetName(source));
                    s.Append(" (");
                    s.Append(source.Value.ToString());
                    s.Append(")");
                }


                int wound = WoundModifier;
                if (wound != 0)
                {
                    s.Append(" - " + LanguageManager.GetString("Tip_Skill_Wounds") + " (" + wound.ToString() + ")");
                }

                if (AttributeObject.Abbrev == "STR" || AttributeObject.Abbrev == "AGI")
                {
                    foreach (Cyberware cyberware in _character.Cyberware.Where(x => x.Name.Contains(" Arm") || x.Name.Contains(" Hand")))
                    {
                        s.Append("\n");
                        s.AppendFormat("{0} {1} ", cyberware.Location, cyberware.DisplayNameShort);
                        if (cyberware.Grade.Name != "Standard")
                        {
                            s.AppendFormat("({0}) ", cyberware.Grade.DisplayName);
                        }

                        int pool = PoolOtherAttribute(Attribute == "STR" ? cyberware.TotalStrength : cyberware.TotalAgility);

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
                    s.Append("\n");
                    if (objSwapSkillAttribute.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute)
                        s.AppendFormat("{0}: ", objSwapSkillAttribute.Exclude);
                    s.AppendFormat("{0} ", GetName(objSwapSkillAttribute));

                    int intLoopAttribute = CharacterObject.GetAttribute(objSwapSkillAttribute.ImprovedName).Value;
                    int intBasePool = PoolOtherAttribute(intLoopAttribute);
                    bool blnHaveSpec = false;

                    if (objSwapSkillAttribute.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute &&
                        Specializations.Any(y => y.Name == objSwapSkillAttribute.Exclude && !CharacterObject.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && objImprovement.UniqueName == y.Name && string.IsNullOrEmpty(objImprovement.Condition))))
                    {
                        intBasePool += 2;
                        blnHaveSpec = true;
                    }
                    s.Append(intBasePool.ToString());

                    if (objSwapSkillAttribute.ImprovedName == "STR" || objSwapSkillAttribute.ImprovedName == "AGI")
                    {
                        foreach (Cyberware cyberware in _character.Cyberware.Where(x => x.Name.Contains(" Arm") || x.Name.Contains(" Hand")))
                        {
                            s.Append("\n");
                            if (objSwapSkillAttribute.ImproveType == Improvement.ImprovementType.SwapSkillSpecAttribute)
                                s.AppendFormat("{0}: ", objSwapSkillAttribute.Exclude);
                            s.AppendFormat("{0} ", GetName(objSwapSkillAttribute));
                            s.AppendFormat("{0} {1} ", cyberware.Location, cyberware.DisplayNameShort);
                            if (cyberware.Grade.Name != "Standard")
                            {
                                s.AppendFormat("({0}) ", cyberware.Grade.DisplayName);
                            }

                            int intLoopPool = PoolOtherAttribute(Attribute == "STR" ? cyberware.TotalStrength : cyberware.TotalAgility);
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

        private string GetName(Improvement source)
        {
            string value = string.Empty;
            switch (source.ImproveSource)
            {
                case Improvement.ImprovementSource.Bioware:
                case Improvement.ImprovementSource.Cyberware:
                    {
                        Cyberware q = _character.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == source.SourceName);
                        value = q?.DisplayNameShort;
                    }
                    break;
                case Improvement.ImprovementSource.MartialArtAdvantage:
                    {
                        MartialArtAdvantage objAdvantage = CommonFunctions.FindMartialArtAdvantage(source.SourceName, _character.MartialArts);
                        value = objAdvantage?.DisplayName;
                    }
                    break;
                case Improvement.ImprovementSource.Armor:
                    {
                        Armor objArmor = _character.Armor.FirstOrDefault(x => x.InternalId == source.SourceName);
                        value = objArmor?.DisplayName;
                    }
                    break;
                case Improvement.ImprovementSource.ArmorMod:
                    {
                        ArmorMod objArmorMod = CommonFunctions.FindArmorMod(source.SourceName, _character.Armor);
                        value = objArmorMod?.DisplayName;
                    }
                    break;
                case Improvement.ImprovementSource.CritterPower:
                    {
                        CritterPower objCritterPower = _character.CritterPowers.FirstOrDefault(x => x.InternalId == source.SourceName);
                        value = objCritterPower?.DisplayName;
                    }
                    break;
                case Improvement.ImprovementSource.Quality:
                    {
                        Quality q = _character.Qualities.FirstOrDefault(x => x.InternalId == source.SourceName);
                        value = q?.DisplayName;
                    }
                    break;
                case Improvement.ImprovementSource.Power:
                    {
                        Power power = _character.Powers.FirstOrDefault(x => x.InternalId == source.SourceName);
                        value = power?.DisplayNameShort;
                    }
                    break;
                case Improvement.ImprovementSource.MentorSpirit:
                    {
                        MentorSpirit mentor = _character.MentorSpirits.FirstOrDefault(x => x.InternalId == source.SourceName);
                        value = mentor?.DisplayName;
                    }
                    break;
                case Improvement.ImprovementSource.Metamagic:
                case Improvement.ImprovementSource.Echo:
                    {
                        Metamagic metamagic = _character.Metamagics.FirstOrDefault(x => x.InternalId == source.SourceName);
                        value = metamagic?.DisplayName;
                    }
                    break;
                case Improvement.ImprovementSource.Art:
                    {
                        Art art = _character.Arts.FirstOrDefault(x => x.InternalId == source.SourceName);
                        value = art?.DisplayName;
                    }
                    break;
                case Improvement.ImprovementSource.Enhancement:
                    {
                        Enhancement enhancement = _character.Enhancements.FirstOrDefault(x => x.InternalId == source.SourceName);
                        value = enhancement?.DisplayName;
                    }
                    break;
                case Improvement.ImprovementSource.Custom:
                    {
                        return source.CustomName;
                    }
                case Improvement.ImprovementSource.Gear:
                    {
                        Gear objGear = _character.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == source.SourceName);
                        if (objGear == null)
                        {
                            objGear = CommonFunctions.FindArmorGear(source.SourceName, _character.Armor);
                            if (objGear == null)
                            {
                                objGear = CommonFunctions.FindWeaponGear(source.SourceName, _character.Weapons);
                                if (objGear == null)
                                {
                                    objGear = CommonFunctions.FindCyberwareGear(source.SourceName, _character.Cyberware);
                                    if (objGear == null)
                                    {
                                        objGear = CommonFunctions.FindVehicleGear(source.SourceName, _character.Vehicles);
                                    }
                                }
                            }
                        }
                        value = objGear?.DisplayName;
                    }
                    break;
                default:
                    return source.SourceName;
            }

            if (string.IsNullOrEmpty(value))
            {
                Log.Warning(new object[] { "Skill Tooltip GetName value = null", source.SourceName, source.ImproveSource, source.ImproveType, source.ImprovedName });
                return source.ImproveSource + " source not found";
            }
            return value;
        }

        public string UpgradeToolTip
        {
            get
            {
                return string.Format(LanguageManager.GetString("Tip_ImproveItem"), (Rating + 1), UpgradeKarmaCost());
            }
        }

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
                return string.Format(LanguageManager.GetString("Tip_Skill_AddSpecialization"), price.ToString());
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
                    middle = $"{SkillGroup} {LanguageManager.GetString("String_ExpenseSkillGroup")}\n";
                }
                if (!String.IsNullOrEmpty(_strNotes))
                {
                    _strNotes = CommonFunctions.WordWrap(_strNotes, 100);
                    strReturn = LanguageManager.GetString("Label_Notes") + " " + _strNotes + "\n\n";
                }

                strReturn += $"{this.GetDisplayCategory()}\n{middle}{CharacterObject.Options.LanguageBookLong(Source)} {LanguageManager.GetString("String_Page")} {Page}";

                return strReturn;
            }
        }

        public string Notes
        {
            get
            {
                return _strNotes;
            }
            set
            {
                _strNotes = value;
            }
        }

        public SkillGroup SkillGroupObject { get; }

        public string Page { get; private set; }

        public string Source { get; private set; }

        //Stuff that is RO, that is simply calculated from other things
        //Move to extension method?

        #region Calculations

        public int AttributeModifiers
        {
            get { return AttributeObject.TotalValue; }
        }

        //TODO: Add translation support
        public string DisplayName
        {
            get { return _translatedName ?? Name; }
        }

        public string DisplayCategory
        {
            get { return _translatedCategory ?? Category; }
            set { _translatedCategory = value; }
        }
        public virtual string DisplayPool
        {
            get {
                return DisplayOtherAttribue(AttributeObject.TotalValue);
            }
        }

        public string DisplayOtherAttribue(int attributeValue)
        {
            int pool = PoolOtherAttribute(attributeValue);

            if (string.IsNullOrWhiteSpace(Specialization) || CharacterObject.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && objImprovement.UniqueName == Name && string.IsNullOrEmpty(objImprovement.Condition)))
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

        private int _cachedWareRating = int.MinValue;
        public int CachedWareRating
        {
            get
            {
                return _cachedWareRating;
            }
            set
            {
                _cachedWareRating = value;
            }
        }
        /// <summary>
        /// The attributeValue this skill have from Skillwires + Skilljack or Active Hardwires
        /// </summary>
        /// <returns>Artificial skill attributeValue</returns>
        public virtual int CyberwareRating()
        {
            if (CachedWareRating != int.MinValue)
                return CachedWareRating;

            //TODO: method is here, but not used in any form, needs testing (worried about child items...)
            //this might do hardwires if i understand how they works correctly
            int intMaxHardwire = -1;
            foreach (Improvement objImprovement in CharacterObject.Improvements)
            {
                if (objImprovement.ImproveType == Improvement.ImprovementType.Hardwire && objImprovement.ImprovedName == Name && objImprovement.Enabled && objImprovement.Value > intMaxHardwire)
                {
                    intMaxHardwire = objImprovement.Value;
                }
            }
            if (intMaxHardwire >= 0)
            {
                return CachedWareRating = intMaxHardwire;
            }

            int skillWireRating = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.Skillwire);
            if ((skillWireRating > 0 || IsKnowledgeSkill) && CharacterObject.SkillsoftAccess)
            {
                int intMax = 0;
                //TODO this works with translate?
                foreach (Gear objSkillsoft in CharacterObject.Gear.DeepWhere(x => x.Children, x => x.Equipped && x.Category == "Skillsofts" &&
                    (x.Extra == Name || x.Extra == Name + ", " + LanguageManager.GetString("Label_SelectGear_Hacked"))))
                {
                    if (objSkillsoft.Rating > intMax)
                        intMax = objSkillsoft.Rating;
                }
                return CachedWareRating = Math.Min(intMax, skillWireRating);
            }

            return CachedWareRating = 0;
        }
        #endregion

        #region Static

        //A tree of dependencies. Once some of the properties are changed,
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly ReverseTree<string> DependencyTree =
            new ReverseTree<string>(nameof(PoolToolTip),
                new ReverseTree<string>(nameof(DisplayPool),
                    new ReverseTree<string>(nameof(Pool),
                        new ReverseTree<string>(nameof(PoolModifiers)),
                        new ReverseTree<string>(nameof(AttributeModifiers)),
                        new ReverseTree<string>(nameof(CanHaveSpecs),
                            new ReverseTree<string>(nameof(Leveled),
                                new ReverseTree<string>(nameof(Rating),
                                    new ReverseTree<string>(nameof(TotalBaseRating),
                                        new ReverseTree<string>(nameof(RatingModifiers)),
                                        new ReverseTree<string>(nameof(LearnedRating),
                                            new ReverseTree<string>(nameof(KarmaUnlocked),
                                                new ReverseTree<string>(nameof(Karma))),
                                            new ReverseTree<string>(nameof(BaseUnlocked),
                                                new ReverseTree<string>(nameof(Base))
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );
        #endregion

        internal void ForceEvent(string property)
        {
            foreach (string s in DependencyTree.Find(property))
            {
                var v = new PropertyChangedEventArgs(s);
                PropertyChanged?.Invoke(this, v);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            foreach (string s in DependencyTree.Find(propertyName))
            {
                var v = new PropertyChangedEventArgs(s);
                PropertyChanged?.Invoke(this, v);
            }
        }

        private void OnSkillGroupChanged(object sender, PropertyChangedEventArgs propertyChangedEventArg)
        {
            if (propertyChangedEventArg.PropertyName == nameof(Skills.SkillGroup.Base))
            {
                OnPropertyChanged(propertyChangedEventArg.PropertyName);
                KarmaSpecForcedMightChange();
            }
            else if (propertyChangedEventArg.PropertyName == nameof(Skills.SkillGroup.Karma))
            {
                OnPropertyChanged(propertyChangedEventArg.PropertyName);
                KarmaSpecForcedMightChange();
            }
        }

        private void OnCharacterChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Character.Karma))
            {
                if (_oldUpgrade != CanUpgradeCareer)
                {
                    _oldUpgrade = CanUpgradeCareer;
                    OnPropertyChanged(nameof(CanUpgradeCareer));
                }
                if (_oldCanAffordSpecialization != CanAffordSpecialization)
                {
                    _oldCanAffordSpecialization = CanAffordSpecialization;
                    OnPropertyChanged(nameof(CanAffordSpecialization));
                }
            }
        }

        protected void OnLinkedAttributeChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            OnPropertyChanged(nameof(AttributeModifiers));
            if (Enabled != _oldEnable)
            {
                OnPropertyChanged(nameof(Enabled));
                _oldEnable = Enabled;
            }
        }

        private void SpecializationsOnListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            _cachedStringSpec = null;
            OnPropertyChanged(nameof(Specialization));
            OnPropertyChanged(nameof(DisplaySpecialization));
        }

        [Obsolete("Refactor this method away once improvementmanager gets outbound events")]
        private void OnImprovementEvent(List<Improvement> improvements)
        {
            _cachedFreeBase = int.MinValue;
            _cachedFreeKarma = int.MinValue;
            _cachedWareRating = int.MinValue;
            if (improvements.Any(imp => imp.ImprovedName == Name &&
                (imp.ImproveType == Improvement.ImprovementType.SkillLevel || imp.ImproveType == Improvement.ImprovementType.SkillBase ||
                imp.ImproveType == Improvement.ImprovementType.Skill || imp.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects)))
            {
                OnPropertyChanged(nameof(Base));
            }
            else if (improvements.Any(imp => imp.ImproveType == Improvement.ImprovementType.ReflexRecorderOptimization))
            {
                OnPropertyChanged(nameof(PoolModifiers));
            }

            if (improvements.Any(imp => imp.ImproveType == Improvement.ImprovementType.Attribute && imp.ImprovedName == Attribute))
            {
                OnPropertyChanged(nameof(AttributeModifiers));
            }
            else if (improvements.Any(imp => imp.ImproveSource == Improvement.ImprovementSource.Cyberware))
            {
                OnPropertyChanged(nameof(AttributeModifiers));
                OnPropertyChanged(nameof(PoolModifiers));
            }
            //TODO: Doesn't work
            else if (improvements.Any(imp => imp.ImproveType == Improvement.ImprovementType.BlockSkillDefault))
            {
                OnPropertyChanged(nameof(PoolToolTip));
            }
            if (improvements.Any(imp => imp.ImprovedName == SkillCategory &&
                (imp.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCost ||
                imp.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier)))
            {
                OnPropertyChanged(nameof(CanAffordSpecialization));
            }
            if (improvements.Any(imp =>
                ((imp.ImprovedName == Name || string.IsNullOrEmpty(imp.ImprovedName)) && 
                (imp.ImproveType == Improvement.ImprovementType.ActiveSkillKarmaCost ||
                imp.ImproveType == Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier ||
                imp.ImproveType == Improvement.ImprovementType.KnowledgeSkillKarmaCost ||
                imp.ImproveType == Improvement.ImprovementType.KnowledgeSkillKarmaCostMultiplier)) ||
                (imp.ImprovedName == SkillCategory) &&
                (imp.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCost ||
                imp.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier)))
            {
                OnPropertyChanged(nameof(CanUpgradeCareer));
            }
        }
    }
}
