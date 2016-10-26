using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Chummer.Backend;

namespace Chummer.Skills
{
	public class SkillsSection : INotifyPropertyChanged
	{
		public event CollegeEducationChangedHandler CollegeEducationChanged;
		public event JackOfAllTradesChangedHandler JackOfAllTradesChanged;
		public event LinguistChangedHandler LinguistChanged;
		public event SchoolOfHardKnocksChangedHandler SchoolOfHardKnocksChanged;
		public event TechSchoolChangedHandler TechSchoolChanged;
		public event UncouthChangedHandler UncouthChanged;
		public event UneducatedChangedHandler UneducatedChanged;
		
		private readonly Character _character;
		private bool _blnUneducated;
		private bool _blnJackOfAllTrades;
		private bool _blnCollegeEducation;
		private bool _blnUncouth;
		private bool _blnSchoolOfHardKnocks;
		private bool _blnTechSchool;
		private bool _blnLinguist;
		private Dictionary<Guid, Skill> _skillValueBackup = new Dictionary<Guid, Skill>(); 

		public SkillsSection(Character character)
		{
			_character = character;
			_character.LOG.PropertyChanged += (sender, args) => KnoChanged();
			_character.INT.PropertyChanged += (sender, args) => KnoChanged();

			_character.ImprovementEvent += CharacterOnImprovementEvent;

		}

		private void CharacterOnImprovementEvent(List<Improvement> improvements, ImprovementManager improvementManager)
		{
			if (improvements.Any(x => x.ImproveType == Improvement.ImprovementType.FreeKnowledgeSkills))
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasKnowledgePoints)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KnowledgeSkillPoints)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KnowledgeSkillPointsRemain)));
			}
		}

		internal void AddSkills(FilterOptions skills)
		{
			var list = GetSkillList(_character, skills);

			//TODO: Handle adept? Only unlocks assencing if s/he has astral perception power
			Skills.MergeInto(list, CompareSkills);
		}

		internal void RemoveSkills(FilterOptions skills)
		{
			string category;
			switch (skills)
			{
				case FilterOptions.Magician:
				case FilterOptions.Sorcery:
				case FilterOptions.Conjuring:
				case FilterOptions.Enchanting:
				case FilterOptions.Adept:
					category = "Magical Active";
					break;
				case FilterOptions.Technomancer:
					category = "Resonance Active";
					break;
				default:
					return;
			}

			for (int i = Skills.Count - 1; i >= 0; i--)
			{
				if (Skills[i].SkillCategory == category)
				{
					Skill skill = Skills[i];
					_skillValueBackup[skill.SkillId] = skill;
					Skills.RemoveAt(i);

					if (_character.Created && skill.LearnedRating > 0)
					{
						KnowledgeSkill kno = new KnowledgeSkill(_character)
						{
							Type = skill.Name == "Arcana" ? "Academic" : "Professional",
							WriteableName = skill.Name,
							Base = skill.Base,
							Karma = skill.Karma
						};
						kno.Specializations.AddRange(skill.Specializations);
						KnowledgeSkills.Add(kno);
					}
				}
			}
		}

		internal void Load(XmlNode skillNode, bool legacy = false)
		{
			Timekeeper.Start("load_char_skills");

			if (!legacy)
			{
				Timekeeper.Start("load_char_skills_groups");
				(from XmlNode node in skillNode.SelectNodes("groups/group") let @group = SkillGroup.Load(_character, node) where @group != null orderby @group.DisplayName descending select @group).ForEach(x => SkillGroups.Add(x));

				Timekeeper.Finish("load_char_skills_groups");

				Timekeeper.Start("load_char_skills_normal");
				//Load skills. Because sorting a BindingList is complicated we use a temporery normal list
				List<Skill> loadingSkills = (from XmlNode node in skillNode.SelectNodes("skills/skill") let skill = Skill.Load(_character, node) where skill != null select skill).ToList();

				loadingSkills.Sort(CompareSkills);


				foreach (Skill skill in loadingSkills)
				{
					_skills.Add(skill);
				}
				Timekeeper.Finish("load_char_skills_normal");

				Timekeeper.Start("load_char_skills_kno");
				List<KnowledgeSkill> knoSkills = (from XmlNode node in skillNode.SelectNodes("knoskills/skill") let skill = (KnowledgeSkill) Skill.Load(_character, node) where skill != null select skill).ToList();


				foreach (KnowledgeSkill skill in knoSkills)
				{
					KnowledgeSkills.Add(skill);
				}
				Timekeeper.Finish("load_char_skills_kno");

				Timekeeper.Start("load_char_knowsoft_buffer");
				// Knowsoft Buffer.
				XmlNodeList objXmlKnowsoftBuffer = skillNode.SelectNodes("skilljackknowledgeskills/skill");
				foreach (XmlNode objXmlSkill in objXmlKnowsoftBuffer)
				{
					string strName = objXmlSkill["name"].InnerText;
					KnowsoftSkills.Add(new KnowledgeSkill(_character, strName));
				}
				Timekeeper.Finish("load_char_knowsoft_buffer");
			}
			else
			{
				XmlNodeList oldskills = skillNode.SelectNodes("skills/skill");

				List<Skill> tempoerySkillList = (from XmlNode node in oldskills let skill = Skill.LegacyLoad(_character, node) where skill != null select skill).ToList();

				List<Skill> unsoredSkills = new List<Skill>();

				//Variable/Anon method as to not clutter anywhere else. Not sure if clever or stupid
				Predicate<Skill> oldSkillFilter = skill =>
				{
					if (skill.Rating > 0) return true;

					if (skill.SkillCategory == "Resonance Active" && !_character.RESEnabled)
					{
						return false;
					}

					//This could be more fine grained, but frankly i don't care
					if (skill.SkillCategory == "Magical Active" && !_character.MAGEnabled)
					{
						return false;
					}


					return true;
				};

				foreach (Skill skill in tempoerySkillList)
				{
					KnowledgeSkill knoSkill = skill as KnowledgeSkill;
					if (knoSkill != null)
					{
						KnowledgeSkills.Add(knoSkill);
					}
					else if(oldSkillFilter(skill))
					{
						unsoredSkills.Add(skill);
					}
				}

				unsoredSkills.Sort(CompareSkills);

				unsoredSkills.ForEach(x => _skills.Add(x));

				UpdateUndoList(skillNode);
			}

			//This might give subtle bugs in the future, 
			//but right now it needs to be run once when upgrading or it might crash. 
			//As some didn't they crashed on loading skills. 
			//After this have run, it won't (for the crash i'm aware)
			//TODO: Move it to the other side of the if someday?

			//remove skillgroups whose skills did not make the final cut
			for (var i = SkillGroups.Count - 1; i >= 0; i--)
			{
				if (SkillGroups[i].GetEnumerable().Any(x => Skills.Contains(x)))
					continue;

				SkillGroups.RemoveAt(i);
			}

			//Workaround for probably breaking compability between earlier beta builds
			if (skillNode["skillptsmax"] == null)
			{
				
				skillNode = skillNode.OwnerDocument["character"];
			}

			SkillPointsMaximum = Convert.ToInt32(skillNode["skillptsmax"].InnerText);
			SkillGroupPointsMaximum = Convert.ToInt32(skillNode["skillgrpsmax"].InnerText);
			skillNode.TryGetField("uneducated", out _blnUneducated);
			skillNode.TryGetField("uncouth", out _blnUncouth);
			skillNode.TryGetField("schoolofhardknocks", out _blnSchoolOfHardKnocks);
			skillNode.TryGetField("collegeeducation", out _blnCollegeEducation);
			skillNode.TryGetField("jackofalltrades", out _blnJackOfAllTrades);
			skillNode.TryGetField("techschool", out _blnTechSchool);
			skillNode.TryGetField("linguist", out _blnLinguist);

			Timekeeper.Finish("load_char_skills");
		}

		private void UpdateUndoList(XmlNode skillNode)
		{
			//Hacky way of converting Expense entries to guid based skill identification
			//specs allready did?
			//First create dictionary mapping name=>guid

			Dictionary<string, Guid> groups =
				SkillGroups.Where(group => group.Rating > 0)
					.GroupBy(arg => arg.Name)
					.Select(group => group.First())
					.ToDictionary(x => x.Name, x => x.Id);

			Dictionary<string, Guid> skills =
				Skills.Where(skill => skill.LearnedRating > 0)
					.Concat(KnowledgeSkills)
					//Next 2 lines prevent dictionary throwing exception in the unlikly case that player have both
					.GroupBy(skill => skill.Name)
					.Select(group => group.First())
					.ToDictionary(x => x.Name, x => x.Id);
			
			UpdateUndoSpecific(skillNode.OwnerDocument, skills, new[] { KarmaExpenseType.AddSkill, KarmaExpenseType.ImproveSkill});
			UpdateUndoSpecific(skillNode.OwnerDocument, groups, new[] { KarmaExpenseType.ImproveSkillGroup });
		}

		private static void UpdateUndoSpecific(XmlDocument doc, Dictionary<string, Guid> map, KarmaExpenseType[] typesRequreingConverting)
		{
			//Build a crazy xpath to get everything we want to convert

			string xpath =
				$"/character/expenses/expense[type = \'Karma\']/undo[{string.Join(" or ", typesRequreingConverting.Select(x => $"karmatype = '{x}'"))}]/objectid";

			//Find everything
			XmlNodeList nodesToChange = doc.SelectNodes(xpath);

			for (var i = 0; i < nodesToChange.Count; i++)
			{
				try
				{
					nodesToChange[i].InnerText = map[nodesToChange[i].InnerText].ToString();
				}
				catch (Exception ex)
				{
					Log.Error(new object[] { "Failed to convert", ex, nodesToChange[i].ParentNode.OuterXml});

					nodesToChange[i].InnerText = new Guid().ToString();  //This creates 00.. guid in default formatting
				}
			}
		}

		internal void Save(XmlTextWriter writer)
		{
			writer.WriteStartElement("newskills");

			writer.WriteElementString("skillptsmax", SkillPointsMaximum.ToString());
			writer.WriteElementString("skillgrpsmax", SkillGroupPointsMaximum.ToString());
			writer.WriteElementString("uneducated", Uneducated.ToString());
			writer.WriteElementString("uncouth", Uncouth.ToString());
			writer.WriteElementString("schoolofhardknocks", SchoolOfHardKnocks.ToString());
			writer.WriteElementString("collegeeducation", CollegeEducation.ToString());
			writer.WriteElementString("jackofalltrades", JackOfAllTrades.ToString());
			writer.WriteElementString("techschool", TechSchool.ToString());
			writer.WriteElementString("linguist", Linguist.ToString());

			writer.WriteStartElement("skills");
			foreach (Skill skill in Skills)
			{
				skill.WriteTo(writer);
			}
			writer.WriteEndElement();
			writer.WriteStartElement("knoskills");
			foreach (KnowledgeSkill knowledgeSkill in KnowledgeSkills)
			{
				knowledgeSkill.WriteTo(writer);
			}
			writer.WriteEndElement();

			writer.WriteStartElement("skilljackknowledgeskills");
			foreach (KnowledgeSkill objSkill in KnowsoftSkills)
			{
				objSkill.WriteTo(writer);
			}
			writer.WriteEndElement();

			writer.WriteStartElement("groups");
			foreach (SkillGroup skillGroup in SkillGroups)
			{
				skillGroup.WriteTo(writer);
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		internal void Reset()
		{
			_skills.Clear();
			KnowledgeSkills.Clear();
			SkillGroups.Clear();
			SkillPointsMaximum = 0;
			SkillGroupPointsMaximum = 0;
			KnowsoftSkills.Clear();
		}

		/// <summary>
		/// Maximum Skill Rating.
		/// </summary>
		public int MaxSkillRating { get; set; } = 0;

		private readonly BindingList<Skill> _skills = new BindingList<Skill>();

		/// <summary>
		/// Active Skills
		/// </summary>
		public BindingList<Skill> Skills
		{
			get
			{
				if (_skills.Count == 0)
				{
					GetSkillList(_character, FilterOptions.NonSpecial).ForEach(x => _skills.Add(x));
				}
				return _skills;
			}
		}

		public BindingList<KnowledgeSkill> KnowledgeSkills { get; } = new BindingList<KnowledgeSkill>();


		/// <summary>
		/// KnowsoftSkills.
		/// </summary>
		public List<KnowledgeSkill> KnowsoftSkills { get; } = new List<KnowledgeSkill>();

		/// <summary>
		/// Skill Groups.
		/// </summary>
		public BindingList<SkillGroup> SkillGroups { get; } = new BindingList<SkillGroup>();

		public bool HasKnowledgePoints => KnowledgeSkillPoints > 0;

		/// <summary>
		/// Number of free Knowledge Skill Points the character has.
		/// </summary>
		public int KnowledgeSkillPoints
		{
			get
			{
				// Calculate Free Knowledge Skill Points. Free points = (INT + LOG) * 2.
				var fromAttributes = _character.BuildMethod == CharacterBuildMethod.Priority ||
				                     (_character.BuildMethod == CharacterBuildMethod.Karma && _character.Options.FreeKarmaKnowledge) ||
				                     _character.BuildMethod == CharacterBuildMethod.SumtoTen
					? (_character.INT.Value + _character.LOG.Value)*_character.Options.FreeKnowledgeMultiplier
					: 0;


				int val = _character.ObjImprovementManager.ValueOf(Improvement.ImprovementType.FreeKnowledgeSkills);
				return fromAttributes + val;
			}
		}

		/// <summary>
		/// Number of free Knowledge skill points the character have remaining
		/// </summary>
		public int KnowledgeSkillPointsRemain
		{
			get { return KnowledgeSkillPoints - KnowledgeSkillPointsUsed; }
		}

		/// <summary>
		/// Number of knowledge skill points the character have used.
		/// </summary>
		public int KnowledgeSkillPointsUsed
		{
			get { return KnowledgeSkills.Sum(x => x.CurrentSpCost()); }
		}

		/// <summary>
		/// Number of free Skill Points the character has left.
		/// </summary>
		public int SkillPoints
		{
			get
			{
				//Even if it is stupid, you can spend real skill points on knoskills...
				if (SkillPointsMaximum == 0)
				{
					return 0;
				}
				int work = 0;
				if (KnowledgeSkillPointsUsed > KnowledgeSkillPoints)
					work -= KnowledgeSkillPoints - KnowledgeSkillPointsUsed;

				return SkillPointsMaximum - Skills.TotalCostSp() - work;
			}
		}

		/// <summary>
		/// Number of maximum Skill Points the character has.
		/// </summary>
		public int SkillPointsMaximum { get; set; }

		/// <summary>
		/// Number of free Skill Points the character has.
		/// </summary>
		public int SkillGroupPoints
		{
			get { return SkillGroupPointsMaximum - SkillGroups.Sum(x => x.Base - x.FreeBase()); }
		}

		/// <summary>
		/// Number of maximum Skill Groups the character has.
		/// </summary>
		public int SkillGroupPointsMaximum { get; set; }

		/// <summary>
		/// Whether or not Uneducated is enabled.
		/// </summary>
		public bool Uneducated
		{
			get { return _blnUneducated; }
			set
			{
				bool blnOldValue = _blnUneducated;
				_blnUneducated = value;

				if (blnOldValue != value)
					UneducatedChanged?.Invoke(_character);
			}
		}

		/// <summary>
		/// Whether or not Jack of All Trades is enabled.
		/// </summary>
		public bool JackOfAllTrades
		{
			get { return _blnJackOfAllTrades; }
			set
			{
				bool blnOldValue = _blnJackOfAllTrades;
				_blnJackOfAllTrades = value;

				if (blnOldValue != value)
					JackOfAllTradesChanged?.Invoke(_character);
			}
		}

		/// <summary>
		/// Whether or not College Education is enabled.
		/// </summary>
		public bool CollegeEducation
		{
			get { return _blnCollegeEducation; }
			set
			{
				bool blnOldValue = _blnCollegeEducation;
				_blnCollegeEducation = value;

				if (blnOldValue != value)
					CollegeEducationChanged?.Invoke(_character);
			}
		}

		/// <summary>
		/// Whether or not Uncouth is enabled.
		/// </summary>
		public bool Uncouth
		{
			get { return _blnUncouth; }
			set
			{
				bool blnOldValue = _blnUncouth;
				_blnUncouth = value;

				if (blnOldValue != value)
					UncouthChanged?.Invoke(_character);
			}
		}

		/// <summary>
		/// Whether or not School of Hard Knocks is enabled.
		/// </summary>
		public bool SchoolOfHardKnocks
		{
			get { return _blnSchoolOfHardKnocks; }
			set
			{
				bool blnOldValue = _blnSchoolOfHardKnocks;
				_blnSchoolOfHardKnocks = value;

				if (blnOldValue != value)
					SchoolOfHardKnocksChanged?.Invoke(_character);
			}
		}

		/// <summary>
		/// Whether or not TechSchool is enabled.
		/// </summary>
		public bool TechSchool
		{
			get { return _blnTechSchool; }
			set
			{
				bool blnOldValue = _blnTechSchool;
				_blnTechSchool = value;

				if (blnOldValue != value)
					TechSchoolChanged?.Invoke(_character);
			}
		}

		/// <summary>
		/// Whether or not Linguist is enabled.
		/// </summary>
		public bool Linguist
		{
			get { return _blnLinguist; }
			set
			{
				bool blnOldValue = _blnLinguist;
				_blnLinguist = value;

				if (blnOldValue != value)
					LinguistChanged?.Invoke(_character);
			}
		}
		public static int CompareSkills(Skill rhs, Skill lhs)
		{
			if (rhs is ExoticSkill)
			{
				if (lhs is ExoticSkill)
				{
					return ((rhs as ExoticSkill).Specific ?? "").CompareTo((lhs as ExoticSkill).Specific ?? "");
				}
				else
				{
					return 1;
				}
			}
			else if (lhs is ExoticSkill)
			{
				return -1;
			}
			else
			{
				return rhs.DisplayName.CompareTo(lhs.DisplayName);
			}
		}

		private static IEnumerable<Skill> GetSkillList(Character c, FilterOptions filter)
		{
			//TODO less retarded way please
			List<Skill> b = new List<Skill>();
			// Load the Skills information.
			XmlDocument objXmlDocument = XmlManager.Instance.Load("skills.xml");

			// Populate the Skills list.
			XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/skills/skill[not(exotic) and (" + c.Options.BookXPath() + ")" + SkillFilter(filter) + "]");

			// First pass, build up a list of all of the Skills so we can sort them in alphabetical order for the current language.
			List<ListItem> lstSkillOrder = new List<ListItem>();
			foreach (XmlNode objXmlSkill in objXmlSkillList)
			{
				ListItem objSkill = new ListItem();
				objSkill.Value = objXmlSkill["name"].InnerText;
				objSkill.Name = objXmlSkill["translate"]?.InnerText ?? objXmlSkill["name"].InnerText;
				lstSkillOrder.Add(objSkill);
			}
			SortListItem objSort = new SortListItem();
			lstSkillOrder.Sort(objSort.Compare);

			// Second pass, retrieve the Skills in the order they're presented in the list.
			foreach (ListItem objItem in lstSkillOrder)
			{
				XmlNode objXmlSkill = objXmlDocument.SelectSingleNode("/chummer/skills/skill[name = \"" + objItem.Value + "\"]");

				//TODO: read from backup
				Skill objSkill = Skill.FromData(objXmlSkill, c);
				b.Add(objSkill);
			}


			return b;
		}

		private static string SkillFilter(FilterOptions filter)
		{
			switch (filter)
			{
				case FilterOptions.All:
					return "";
				case FilterOptions.NonSpecial:
					return " and not(category = 'Magical Active') and not(category = 'Resonance Active')";
				case FilterOptions.Magician:
					return " and category = 'Magical Active'";
				case FilterOptions.Sorcery:
					return " and category = 'Magical Active' and (skillgroup = 'Sorcery' or skillgroup = '' or not(skillgroup))";
				case FilterOptions.Conjuring:
					return " and category = 'Magical Active' and (skillgroup = 'Conjuring' or skillgroup = '' or not(skillgroup))";
				case FilterOptions.Enchanting:
					return " and category = 'Magical Active' and (skillgroup = 'Enchanting' or skillgroup = '' or not(skillgroup))";
				case FilterOptions.Adept:
					return " and category = 'Magical Active' and (skillgroup = '' or not(skillgroup))";
				case FilterOptions.Spellcasting:
					return " and category = 'Magical Active' and name = 'Spellcasting'";
				case FilterOptions.Technomancer:
					return " and category = 'Resonance Active'";
				default:
					throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
			}
		}

		internal enum FilterOptions
		{
			All,
			NonSpecial,
			Magician,
			Sorcery,
			Conjuring,
			Enchanting,
			Adept,
			Technomancer,
			Spellcasting
		}

		internal void ForceProperyChangedNotificationAll(string name)
		{
			foreach (Skill skill in Skills)
			{
				skill.ForceEvent(name);
			}

			foreach (KnowledgeSkill skill in KnowledgeSkills)
			{
				skill.ForceEvent(name);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[Obsolete("Should be private and stuff. Play a little once improvementManager gets events")]
		internal void KnoChanged()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KnowledgeSkillPoints)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasKnowledgePoints)));
		}


		public void Print(XmlTextWriter objWriter)
		{
			foreach (Skill skill in Skills)
			{
				if (_character.Options.PrintSkillsWithZeroRating || skill.Rating > 0)
				{
					skill.Print(objWriter);
				}
			}

			foreach (KnowledgeSkill skill in KnowledgeSkills)
			{
				skill.Print(objWriter);
			}
		}
	}
}