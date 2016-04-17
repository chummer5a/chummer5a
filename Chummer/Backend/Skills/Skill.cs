using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using Chummer.Annotations;
using Chummer.Datastructures;

namespace Chummer.Skills
{
	[DebuggerDisplay("{_name} {_base} {_karma}")]
	public partial class Skill : INotifyPropertyChanged
	{
		/// <summary>
		/// Called during save to allow derived classes to save additional infomation required to rebuild state
		/// </summary>
		/// <param name="writer"></param>
		protected virtual void SaveExtendedData(XmlTextWriter writer) { }

		protected CharacterAttrib AttributeObject; //Attribute this skill primarily depends on
		private readonly Character _character; //The Character (parent) to this skill
		protected readonly string Category; //Name of the skill category it belongs to
		protected readonly string _group;  //Name of the skill group this skill belongs to (remove?)
		protected string _name;  //English name of this skill
		protected List<ListItem> _spec;  //List of suggested specializations for this skill
		private readonly string _translatedName = null;


		#region REMOVELATERANDPLACEINCHILDORNEVER?

		public bool Absorb(Skill s)
		{
			return false;
		}
		public void Free(Skill s) { }

		public ReadOnlyCollection<Skill> Fold
		{
			get { return null; }
		}

		
		public bool IdImprovement;
		public bool LockKnowledge;

		#endregion

		#region REFACTORAWAY_NOTANYMORE_RENAME_MEANING



		public void Print(XmlWriter xw) { } //Not this one, due grouping, interface?
		#endregion

		#region Factory

		public void WriteTo(XmlTextWriter writer)
		{
			writer.WriteStartElement("skill");
			writer.WriteElementString("guid", Id.ToString());
			writer.WriteElementString("suid", SkillId.ToString());
			writer.WriteElementString("karma", _karma.ToString());
			writer.WriteElementString("base", _base.ToString());  //this could acctually be saved in karma too during career

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


		/// <summary>
		/// Load a skill from a xml node from a saved .chum5 file
		/// </summary>
		/// <param name="n">The XML node describing the skill</param>
		/// <param name="character">The character this skill belongs to</param>
		/// <returns></returns>
		public static Skill Load(Character character, XmlNode n)
		{
			if (n["suid"] == null) return null;


			Guid suid;
			if (!Guid.TryParse(n["suid"].InnerText, out suid))
			{
				return null;
			}
			Skill skill;
			if (suid != Guid.Empty)
			{
				XmlDocument skills = XmlManager.Instance.Load("skills.xml");
				XmlNode node = skills.SelectSingleNode($"/chummer/skills/skill[id = '{n["suid"].InnerText}']");

				if (node == null) return null;

				skill = node["exotic"]?.InnerText == "Yes" ? new ExoticSkill(character, node) : new Skill(character, node);
			}
			else  //This is ugly but i'm not sure how to make it pretty
			{
				KnowledgeSkill knoSkill = new KnowledgeSkill(character);
				knoSkill.Load(n);

				skill = knoSkill;

			}

			XmlElement element = n["guid"];
			if (element != null) skill.Id = Guid.Parse(element.InnerText);

			n.TryGetField("karma", out skill._karma);
			n.TryGetField("base", out skill._base);
			n.TryGetField("buywithkarma", out skill._buyWithKarma);

			foreach (XmlNode spec in n.SelectNodes("specs/spec"))
			{
				skill.Specializations.Add(SkillSpecialization.Load(spec));
			}

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
			Guid suid;
			if (!n.TryGetField("id", Guid.TryParse, out suid))
				return null;

			Skill skill;

			if (n.TryCheckValue("knowledge", "True"))
			{
				Skills.KnowledgeSkill kno = new KnowledgeSkill(character);
				kno.WriteableName = n["name"].InnerText;
				kno.Base = Int32.Parse(n["base"].InnerText);
				kno.Karma = Int32.Parse(n["karma"].InnerText);

				kno.Type = n["skillcategory"].InnerText;

				skill = kno;
			}
			else
			{
				XmlNode data = XmlManager.Instance.Load("skills.xml").SelectSingleNode($"/chummer/skills/skill[id = '{n["id"].InnerText}']");

				//Some stuff apparently have a guid of 0000-000... (only exotic?)
				if (data == null)
				{
					data = XmlManager.Instance.Load("skills.xml")
						.SelectSingleNode($"/chummer/skills/skill[name = '{n["name"].InnerText}']");
				}


				skill = Skill.FromData(data, character);

				n.TryGetField("base", out skill._base);
				n.TryGetField("karma", out skill._karma);

				skill._buyWithKarma = n.TryCheckValue("buywithkarma", "True");

				
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

		protected static readonly Dictionary<string, bool> SkillTypeCache = new Dictionary<string, bool>();  //TODO CACHE INVALIDATE

		/// <summary>
		/// Load a skill from a data file describing said skill
		/// </summary>
		/// <param name="n">The XML node describing the skill</param>
		/// <param name="character">The character the skill belongs to</param>
		/// <returns></returns>
		public static Skill FromData(XmlNode n, Character character)
		{
			Skill s;
			if (n["exotic"] != null && n["exotic"].InnerText == "Yes")
			{
				//load exotic skill
				//TODO FINISH THIS
				if (Debugger.IsAttached)
					Debugger.Break();

				ExoticSkill s2 = new ExoticSkill(character, n);

				s = s2;
			}
			else
			{

				string category = n["category"].InnerText;  //if missing we have bigger problems, and a nullref is probably prefered
				bool knoSkill;

				if (SkillTypeCache != null && SkillTypeCache.ContainsKey(category))
				{
					knoSkill = SkillTypeCache[category];  //Simple cache, no need to be sloppy
				}
				else
				{
					XmlDocument document = XmlManager.Instance.Load("skills.xml");
					XmlNode knoNode = document.SelectSingleNode($"/chummer/categories/category[. = '{category}']");
					knoSkill = knoNode.Attributes["type"].InnerText != "active";
					SkillTypeCache[category] = knoSkill;
				}


				if (knoSkill)
				{
					//TODO INIT SKILL
					if (Debugger.IsAttached) Debugger.Break();

					KnowledgeSkill s2 = new KnowledgeSkill(character);

					s = s2;
				}
				else
				{
					Skill s2 = new Skill(character, n);
					//TODO INIT SKILL

					s = s2;
				}
			}

			
			return s;
		}

		protected Skill(Character character, string group)
		{
			_character = character;
			_group = group;
			
			_character.PropertyChanged += OnCharacterChanged;
			SkillGroupObject = Skills.SkillGroup.Get(this);
			if (SkillGroupObject != null)
			{
				SkillGroupObject.PropertyChanged += OnSkillGroupChanged;
			}

			character.ImprovementEvent += OnImprovementEvent;
		}

		//load from data
		protected Skill(Character character, XmlNode n) : this(character, n["skillgroup"].InnerText) //Ugly hack, needs by then
		{
			_name = n["name"].InnerText; //No need to catch errors (for now), if missing we are fsked anyway
			AttributeObject = CharacterObject.GetAttribute(n["attribute"].InnerText);
			Category = n["category"].InnerText;
			Default = n["default"].InnerText.ToLower() == "yes";
			Source = n["source"].InnerText;
			Page = n["page"].InnerText;
			SkillId = Guid.Parse(n["id"].InnerText);

			_translatedName = n["translate"]?.InnerText;

			AttributeObject.PropertyChanged += OnLinkedAttributeChanged;

			_spec = new List<ListItem>();
			foreach (XmlNode node in n["specs"].ChildNodes)
			{
				_spec.Add(ListItem.AutoXml(node.InnerText, node));
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
				if (GlobalOptions.Instance.Language != "en-us")
				{
					return LanguageManager.Instance.GetString($"String_Attribute{AttributeObject.Abbrev}Short");
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
			get { return AttributeObject.Value != 0; }
		}

		private bool _oldUpgrade = false;
		public bool CanUpgradeCareer
		{
			get { return CharacterObject.Karma >= UpgradeKarmaCost() && RatingMaximum > LearnedRating; }
		}

		public virtual bool AllowDelete
		{
			get
			{
				return false;
			}
		}

		public bool Default { get; private set; }

		public virtual bool ExoticSkill
		{
			get { return false; }
			set { } //TODO REFACTOR AWAY
		}

		public virtual bool KnowledgeSkill
		{
			get { return false; }
			set { } //TODO REFACTOR AWAY
		}
		
		public virtual string Name
		{
			get { return _name; }
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

		public string SkillGroup { get { return _group; } }

		public virtual string SkillCategory
		{
			get { return Category; }
		}

		public IReadOnlyList<ListItem> CGLSpecializations { get { return _spec; } } 

		//TODO A unit test here?, I know we don't have them, but this would be improved by some
		//Or just ignore support for multiple specizalizations even if the rules say it is possible?
		public List<SkillSpecialization> Specializations { get; } = new List<SkillSpecialization>();
		public string Specialization
		{
			get
			{
				if (LearnedRating == 0)
				{
					return ""; //Unleveled skills cannot have a specialization;
				}

				Specializations.Sort((x, y) => x.Free == y.Free ? 0 : (x.Free ? 1 : -1));
				if (Specializations.Count > 0)
				{
					return Specializations[0].Name;
				}

				return "";
			}
			set
			{
				if (Specializations.Count == 0)
				{
					Specializations.Add(new SkillSpecialization(value, false));
				}
				else
				{
					if (Specializations[0].Free)
					{
						Specializations.Add(new SkillSpecialization(value, false));
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
					return "You cannot default in this skill"; //TODO translate (could not find it in lang file, did not check old source)
				}

				StringBuilder s;
				if (WireRating() > LearnedRating)
				{
					s = new StringBuilder($"{LanguageManager.Instance.GetString("Tip_Skill_SkillsoftRating")} ({WireRating()})");
				}
				else
				{
					s = new StringBuilder($"{LanguageManager.Instance.GetString("Tip_Skill_SkillRating")} ({Rating})");
				}

				s.Append($" + {Attribute} ({AttributeModifiers})");

				if (Default && !Leveled)
				{
					s.Append($" - {LanguageManager.Instance.GetString("Tip_Skill_Defaulting")} (1)");
				}
				

				if (Rating > 0)
				{
					s.Append(InsaneOldModifiersMaker());  //TODO: Confirm. Old code hints modifiers are only when not defaulting.
				}

				int poolmod = PoolModifiers;
				if (poolmod != 0)
				{
					s.Append(" + " + LanguageManager.Instance.GetString("Tip_Skill_RatingModifiers") + " (" + poolmod + ")");
				}

				int wound = WoundModifier;
				if (wound != 0)
				{
					s.Append(" - " + LanguageManager.Instance.GetString("Tip_Skill_Wounds") + " (" + wound + ")");
				}

				return s.ToString();
			}
		}

		private string InsaneOldModifiersMaker()
		{
			List<string> lstUniqueName = new List<string>();
			List<string[,,]> lstUniquePair = new List<string[,,]>();

			string strReturn = "";
			int intModifier = 0;
			foreach (Improvement objImprovement in CharacterObject.Improvements)
			{
				if (!objImprovement.AddToRating && objImprovement.Enabled && !objImprovement.Custom)
				{
					// Improvement for an individual Skill.
					if (!ExoticSkill)
					{
						if (objImprovement.ImproveType == Improvement.ImprovementType.Skill && objImprovement.ImprovedName == Name)
						{
							if (objImprovement.UniqueName != "")
							{
								// If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
								bool blnFound = false;
								foreach (string strName in lstUniqueName)
								{
									if (strName == objImprovement.UniqueName)
									{
										blnFound = true;
										break;
									}
								}
								if (!blnFound)
									lstUniqueName.Add(objImprovement.UniqueName);

								// Add the values to the UniquePair List so we can check them later.
								string[,,] strValues = new string[,,]
								{{{objImprovement.UniqueName, objImprovement.Value.ToString(), objImprovement.SourceName}}};
								lstUniquePair.Add(strValues);
							}
							else
							{
								intModifier += objImprovement.Value;
								if (objImprovement.Value != 0)
									strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + objImprovement.Value.ToString() + ")";
							}
						}
					}
					else
					{
						if (objImprovement.ImproveType == Improvement.ImprovementType.Skill &&
						    objImprovement.ImprovedName == Name + " (" + Specialization + ")")
						{
							intModifier += objImprovement.Value;
							if (objImprovement.Value != 0)
								strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + objImprovement.Value.ToString() + ")";
						}
					}

					// Improvement for a Skill Group.
					if (objImprovement.ImproveType == Improvement.ImprovementType.SkillGroup && objImprovement.ImprovedName == SkillGroup)
					{
						if (!objImprovement.Exclude.Contains(Name))
						{
							intModifier += objImprovement.Value;
							if (objImprovement.Value != 0)
								strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + objImprovement.Value.ToString() + ")";
						}
					}

					// Improvement for a Skill Category.
					if (objImprovement.ImproveType == Improvement.ImprovementType.SkillCategory &&
					    objImprovement.ImprovedName == SkillCategory)
					{
						if (!objImprovement.Exclude.Contains(Name))
						{
							if (objImprovement.UniqueName != "")
							{
								// If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
								bool blnFound = false;
								foreach (string strName in lstUniqueName)
								{
									if (strName == objImprovement.UniqueName)
									{
										blnFound = true;
										break;
									}
								}
								if (!blnFound)
									lstUniqueName.Add(objImprovement.UniqueName);

								// Add the values to the UniquePair List so we can check them later.
								string[,,] strValues = new string[,,]
								{{{objImprovement.UniqueName, objImprovement.Value.ToString(), objImprovement.SourceName}}};
								lstUniquePair.Add(strValues);
							}
							else
							{
								intModifier += objImprovement.Value;
								if (objImprovement.Value != 0)
									strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + objImprovement.Value.ToString() + ")";
							}
						}
					}

					// Improvement for a Skill linked to an Attribute.
					if (objImprovement.ImproveType == Improvement.ImprovementType.SkillAttribute &&
					    objImprovement.ImprovedName == Attribute)
					{
						if (!objImprovement.Exclude.Contains(Name))
						{
							intModifier += objImprovement.Value;
							if (objImprovement.Value != 0)
								strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + objImprovement.Value.ToString() + ")";
						}
					}

					// Improvement for Enhanced Articulation
					if (SkillCategory == "Physical Active" &&
					    (AttributeObject.Abbrev == "BOD" || AttributeObject.Abbrev == "AGI" || AttributeObject.Abbrev == "REA" || AttributeObject.Abbrev == "STR"))
					{
						if (objImprovement.ImproveType == Improvement.ImprovementType.EnhancedArticulation)
						{
							intModifier += objImprovement.Value;
							if (objImprovement.Value != 0)
								strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + objImprovement.Value.ToString() + ")";
						}
					}
				}
			}

			// Run through the list of UniqueNames and pick out the highest value for each one.
			foreach (string strName in lstUniqueName)
			{
				string strHighestName = "";
				int intHighest = -999;
				foreach (string[,,] strValues in lstUniquePair)
				{
					if (strValues[0, 0, 0] == "" || (strValues[0, 0, 0] == strName && !strName.StartsWith("precedence")))
					{
						if (Convert.ToInt32(strValues[0, 1, 0]) > intHighest)
						{
							intHighest = Convert.ToInt32(strValues[0, 0, 1]);
							strHighestName = strValues[0, 0, 2];
						}
					}
				}
				if (intHighest == -999)
					intHighest = 0;
				intModifier += intHighest;

				if (intHighest != 0)
				{
					foreach (Improvement objImprovement in CharacterObject.Improvements)
					{
						if (objImprovement.SourceName == strHighestName)
						{
							strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + intHighest.ToString() + ")";
							break;
						}
					}
				}
			}

			if (lstUniqueName.Contains("precedence2"))
			{
				string strHighestName = "";
				int intHighest = -999;
				foreach (string[,,] strValues in lstUniquePair)
				{
					if (strValues[0, 0, 0] == "precedence2")
					{
						if (Convert.ToInt32(strValues[0, 0, 1]) > intHighest)
						{
							intHighest = Convert.ToInt32(strValues[0, 0, 1]);
							strHighestName = strValues[0, 0, 2];
						}
					}
				}
				intModifier += intHighest;

				if (intHighest != 0)
				{
					foreach (Improvement objImprovement in CharacterObject.Improvements)
					{
						if (objImprovement.SourceName == strHighestName)
						{
							strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + intHighest.ToString() + ")";
							break;
						}
					}
				}
			}

			if (lstUniqueName.Contains("precedence1"))
			{
				intModifier = 0;
				// Retrieve all of the items that are precedence1 and nothing else.
				foreach (string[,,] strValues in lstUniquePair)
				{
					if (strValues[0, 0, 0] == "precedence1")
						intModifier += Convert.ToInt32(strValues[0, 0, 1]);
				}
			}

			if (lstUniqueName.Contains("precedence0"))
			{
				// Retrieve only the highest precedence0 value.
				// Run through the list of UniqueNames and pick out the highest value for each one.
				string strHighestName = "";
				int intHighest = -999;
				foreach (string[,,] strValues in lstUniquePair)
				{
					if (strValues[0, 0, 0] == "precedence0")
					{
						if (Convert.ToInt32(strValues[0, 0, 1]) > intHighest)
						{
							intHighest = Convert.ToInt32(strValues[0, 0, 1]);
							strHighestName = strValues[0, 0, 2];
						}
					}
				}
				intModifier = intHighest;

				if (intHighest != 0)
				{
					foreach (Improvement objImprovement in CharacterObject.Improvements)
					{
						if (objImprovement.SourceName == strHighestName)
						{
							strReturn = " + " + CharacterObject.GetObjectName(objImprovement) + " (" + intHighest.ToString() + ")";
							break;
						}
					}
				}
				else
					strReturn = "";
			}

			// Factor in Custom Improvements.
			lstUniqueName = new List<string>();
			lstUniquePair = new List<string[,,]>();

			int intCustomModifier = 0;
			foreach (Improvement objImprovement in CharacterObject.Improvements)
			{
				if (!objImprovement.AddToRating && objImprovement.Enabled && objImprovement.Custom)
				{
					// Improvement for an individual Skill.
					if (!ExoticSkill)
					{
						if (objImprovement.ImproveType == Improvement.ImprovementType.Skill && objImprovement.ImprovedName == Name)
						{
							if (objImprovement.UniqueName != "")
							{
								// If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
								bool blnFound = false;
								foreach (string strName in lstUniqueName)
								{
									if (strName == objImprovement.UniqueName)
									{
										blnFound = true;
										break;
									}
								}
								if (!blnFound)
									lstUniqueName.Add(objImprovement.UniqueName);

								// Add the values to the UniquePair List so we can check them later.
								string[,,] strValues = new string[,,]
								{{{objImprovement.UniqueName, objImprovement.Value.ToString(), objImprovement.SourceName}}};
								lstUniquePair.Add(strValues);
							}
							else
							{
								intCustomModifier += objImprovement.Value;
								if (objImprovement.Value != 0)
									strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + objImprovement.Value.ToString() + ")";
							}
						}
					}
					else
					{
						if (objImprovement.ImproveType == Improvement.ImprovementType.Skill &&
						    objImprovement.ImprovedName == Name + " (" + Specialization + ")")
						{
							intCustomModifier += objImprovement.Value;
							if (objImprovement.Value != 0)
								strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + objImprovement.Value.ToString() + ")";
						}
					}

					// Improvement for a Skill Group.
					if (objImprovement.ImproveType == Improvement.ImprovementType.SkillGroup && objImprovement.ImprovedName == SkillGroup)
					{
						if (!objImprovement.Exclude.Contains(Name))
						{
							intCustomModifier += objImprovement.Value;
							if (objImprovement.Value != 0)
								strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + objImprovement.Value.ToString() + ")";
						}
					}
					// Improvement for a Skill Category.
					if (objImprovement.ImproveType == Improvement.ImprovementType.SkillCategory &&
					    objImprovement.ImprovedName == SkillCategory)
					{
						if (!objImprovement.Exclude.Contains(Name))
						{
							intCustomModifier += objImprovement.Value;
							if (objImprovement.Value != 0)
								strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + objImprovement.Value.ToString() + ")";
						}
					}
					// Improvement for a Skill linked to an Attribute.
					if (objImprovement.ImproveType == Improvement.ImprovementType.SkillAttribute &&
					    objImprovement.ImprovedName == AttributeObject.Abbrev)
					{
						if (!objImprovement.Exclude.Contains(Name))
						{
							intCustomModifier += objImprovement.Value;
							if (objImprovement.Value != 0)
								strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + objImprovement.Value.ToString() + ")";
						}
					}

					// Improvement for Enhanced Articulation
					if (SkillCategory == "Physical Active" &&
					    (AttributeObject.Abbrev == "BOD" || AttributeObject.Abbrev == "AGI" || AttributeObject.Abbrev == "REA" || AttributeObject.Abbrev == "STR"))
					{
						if (objImprovement.ImproveType == Improvement.ImprovementType.EnhancedArticulation)
						{
							intCustomModifier += objImprovement.Value;
							if (objImprovement.Value != 0)
								strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + objImprovement.Value.ToString() + ")";
						}
					}
				}
			}

			// Run through the list of UniqueNames and pick out the highest value for each one.
			foreach (string strName in lstUniqueName)
			{
				string strHighestName = "";
				int intHighest = -999;
				foreach (string[,,] strValues in lstUniquePair)
				{
					if (strValues[0, 0, 0] == "")
					{
						if (Convert.ToInt32(strValues[0, 0, 1]) > intHighest)
						{
							intHighest = Convert.ToInt32(strValues[0, 0, 1]);
							strHighestName = strValues[0, 0, 2];
						}
					}
				}
				if (intHighest == -999)
					intHighest = 0;
				intCustomModifier += intHighest;

				if (intHighest != 0)
				{
					foreach (Improvement objImprovement in CharacterObject.Improvements)
					{
						if (objImprovement.SourceName == strHighestName)
						{
							strReturn += " + " + CharacterObject.GetObjectName(objImprovement) + " (" + intHighest.ToString() + ")";
							break;
						}
					}
				}
			}
			return strReturn;
		}

		public string UpgradeToolTip
		{
			get { return string.Format(LanguageManager.Instance.GetString("Tip_ImproveItem"), (Rating + 1), UpgradeKarmaCost()); ; }
		}

		public string AddSpecToolTip
		{
			get { return string.Format(LanguageManager.Instance.GetString("Tip_Skill_AddSpecialization"), CharacterObject.Options.KarmaSpecialization); }
		}

		public string SkillToolTip
		{
			get
			{
				//v-- hack i guess
				string middle = "";
				if (!string.IsNullOrWhiteSpace(SkillGroup))
				{
					middle = $"{SkillGroup} {LanguageManager.Instance.GetString("String_ExpenseSkillGroup")}\n";
				}

				return $"{this.GetDisplayCategory()}\n{middle}{CharacterObject.Options.LanguageBookLong(Source)} {LanguageManager.Instance.GetString("String_Page")} {Page}";
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

		public string DisplayPool
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Specialization))
				{
					return Pool.ToString();
				}
				else
				{
					//Handler for the Inspired Quality. 
					if (!KnowledgeSkill && Name == "Artisan")
					{
						if (CharacterObject.Qualities.Any(objQuality => objQuality.Name == "Inspired"))
						{
							return $"{Pool} ({Pool + 3})";
						}
					}
					else if (ExoticSkill)
					{
						return $"{Pool}";
					}
					return $"{Pool} ({Pool + 2})";
				}
			}

		}

		/// <summary>
		/// The rating this skill have from Skillwires + Skilljack or Active Hardwires
		/// </summary>
		/// <returns>Artificial skill rating</returns>
		public int WireRating()
		{
			//TODO: method is here, but not used in any form, needs testing (worried about child items...)
			//this might do hardwires if i understand how they works correctly
			var hardwire = CharacterObject.Improvements.Where(
				improvement =>
					improvement.ImproveType == Improvement.ImprovementType.HardWire && improvement.ImprovedName == Name &&
					improvement.Enabled).ToList();

			if (hardwire.Any())
			{
				return hardwire.Max(x => x.Value);
			}
			

			ImprovementManager manager = new ImprovementManager(CharacterObject);

			int skillWireRating = manager.ValueOf(Improvement.ImprovementType.Skillwire);
			if (skillWireRating > 0 || (CharacterObject.SkillsoftAccess && KnowledgeSkill))
			{
				Func<Gear, int> recusivestuff = null; recusivestuff = (gear) =>
				{
					//TODO this works with translate?
					if (gear.Equipped && gear.Category == "Skillsofts" &&
					    (gear.Extra == Name ||
					     gear.Extra == Name + ", " + LanguageManager.Instance.GetString("Label_SelectGear_Hacked")))
					{
						return gear.Name == "Activesoft"
							? Math.Min(gear.Rating, skillWireRating)
							: gear.Rating;

					}
					return gear.Children.Select(child => recusivestuff(child)).FirstOrDefault(returned => returned > 0);
				};

				return CharacterObject.Gear.Select(child => recusivestuff(child)).FirstOrDefault(val => val > 0);

			}

			return 0;
		}

		#endregion

		#region Static

		//A tree of dependencies. Once some of the properties are changed, 
		//anything they depend on, also needs to raise OnChanged
		//This tree keeps track of dependencies
		private static readonly ReverseTree<string> DependencyTree =
		
		new ReverseTree<string>(nameof(DisplayPool),
			new ReverseTree<string>(nameof(Pool),
				new ReverseTree<string>(nameof(PoolModifiers)),
				new ReverseTree<string>(nameof(AttributeModifiers)),
				new ReverseTree<string>(nameof(Leveled),
					new ReverseTree<string>(nameof(Rating),
						new ReverseTree<string>(nameof(Karma)),
						new ReverseTree<string>(nameof(BaseUnlocked),
							new ReverseTree<string>(nameof(Base))
						)
					)
				)
			)
		);
		
		
		#endregion

		public void ForceEvent(string property)
		{
			foreach (string s in DependencyTree.Find(property))
			{
				var v = new PropertyChangedEventArgs(s);
				PropertyChanged?.Invoke(this, v);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
			else if(propertyChangedEventArg.PropertyName == nameof(Skills.SkillGroup.Karma))
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

			}
		}

		private void OnLinkedAttributeChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			OnPropertyChanged(nameof(AttributeModifiers));
			if (Enabled != _oldEnable)
			{
				OnPropertyChanged(nameof(Enabled));
				_oldEnable = Enabled;
			}

		}

		[Obsolete("Refactor this method away once improvementmanager gets outbound events")]
		private void OnImprovementEvent(List<Improvement> improvements, ImprovementManager improvementManager)
		{
			_cachedFreeBase = int.MinValue;
			_cachedFreeKarma = int.MinValue;
			if(improvements.Any(imp => imp.ImproveType == Improvement.ImprovementType.SkillLevel 
			&& imp.ImprovedName == _name))
				OnPropertyChanged(nameof(PoolModifiers));
		}
		
	}
}
