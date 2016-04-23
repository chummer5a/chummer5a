using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Skills;
using Chummer.Skills;
using Chummer.UI.Shared;

namespace Chummer.UI.Skills
{
	public partial class SkillsTabUserControl : UserControl
	{
		public event PropertyChangedEventHandler ChildPropertyChanged; 

		private BindingListDisplay<Skill> _skills;
		private BindingListDisplay<SkillGroup> _groups;
		private BindingListDisplay<KnowledgeSkill> _knoSkills; 

		public SkillsTabUserControl()
		{
			InitializeComponent();

			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
		}

		public void MissingDatabindingsWorkaround()
		{
			UpdateKnoSkillRemaining();
		}

		private bool _loadCalled = false;
		private bool _initialized = false;
		private Character _character;
		private List<Tuple<string, Func<Skill, bool>>> _dropDownList;
		private List<Tuple<string, IComparer<Skill>>>  _sortList;

		public Character ObjCharacter
		{
			set
			{
				_character = value;
				RealLoad();
			}
			get { return _character; }
		}
		private void SkillsTabUserControl_Load(object sender, EventArgs e)
		{
			_loadCalled = true;
			RealLoad();
		}

		private void RealLoad() //Ensure loaded and got character obj
		{
			if (_initialized) return;

			if (!(_character != null && _loadCalled)) return;

			_initialized = true;
			Stopwatch sw = Stopwatch.StartNew();
			Stopwatch parts = Stopwatch.StartNew();
			//Keep everything visible until ready to display everything. This 
			//seems to prevent redrawing everything each time anything is added
			//Not benched, but should be faster

			//Might also be useless horseshit, 2 lines

			//Visible = false;
			//this.SuspendLayout();
			MakeSkillDisplays();

			parts.TaskEnd("MakeSkillDisplay()");


			if (ObjCharacter.Created)
			{
				lblKnowledgeSkillPoints.Visible = false;
				lblKnowledgeSkillPointsTitle.Visible = false;
			}
			else
			{
				UpdateKnoSkillRemaining();
			}

			_dropDownList = GenerateDropdownFilter();

			parts.TaskEnd("GenerateDropDown()");

			_sortList = GenerateSortList();

			parts.TaskEnd("GenerateSortList()");


			cboDisplayFilter.DataSource = _dropDownList;
			cboDisplayFilter.ValueMember = "Item2";
			cboDisplayFilter.DisplayMember = "Item1";
			cboDisplayFilter.SelectedIndex = 0;
			cboDisplayFilter.MaxDropDownItems = _dropDownList.Count;

			parts.TaskEnd("_ddl databind");

			cboSort.DataSource = _sortList;
			cboSort.ValueMember = "Item2";
			cboSort.DisplayMember = "Item1";
			cboSort.SelectedIndex = 0;
			cboSort.MaxDropDownItems = _sortList.Count;

			parts.TaskEnd("_sort databind");

			_skills.ChildPropertyChanged += ChildPropertyChanged;
			_groups.ChildPropertyChanged += ChildPropertyChanged;
			_knoSkills.ChildPropertyChanged += ChildPropertyChanged;

			//Visible = true;
			//this.ResumeLayout(false);
			//this.PerformLayout();
			parts.TaskEnd("visible");
			Panel1_Resize(null, null);
			Panel2_Resize(null, null);
			parts.TaskEnd("resize");
			sw.Stop();
			Debug.WriteLine("RealLoad() in {0} ms", sw.Elapsed.TotalMilliseconds);
			//this.Update();
			//this.ResumeLayout(true);
			//this.PerformLayout();
		}

		private List<Tuple<string, IComparer<Skill>>> GenerateSortList()
		{
			List<Tuple<string, IComparer<Skill>>> ret = new List<Tuple<string, IComparer<Skill>>>()
			{
				new Tuple<string, IComparer<Skill>>("Alphabetical",
					new SkillSorter((x, y) => x.DisplayName.CompareTo(y.DisplayName))),
				new Tuple<string, IComparer<Skill>>("Rating",
					new SkillSorter((x, y) => y.Rating.CompareTo(x.Rating))),
				new Tuple<string, IComparer<Skill>>("Dicepool",
					new SkillSorter((x, y) => y.Pool.CompareTo(x.Pool))),
				new Tuple<string, IComparer<Skill>>("Smaller Dicepool",
					new SkillSorter((x, y) => x.Pool.CompareTo(y.Pool))),
				new Tuple<string, IComparer<Skill>>("Attribute Value",
					new SkillSorter((x, y) => y.AttributeModifiers.CompareTo(x.AttributeModifiers))),
				new Tuple<string, IComparer<Skill>>("Attribute Name",
					new SkillSorter((x, y) => x.Attribute.CompareTo(y.Attribute))),
				new Tuple<string, IComparer<Skill>>("Group Name",
					new SkillSorter((x, y) => y.SkillGroup.CompareTo(x.SkillGroup))),
				new Tuple<string, IComparer<Skill>>("Group Rating",
					new SkillSortBySkillGroup()),
				new Tuple<string, IComparer<Skill>>("Category",
					new SkillSorter((x, y) => x.SkillCategory.CompareTo(y.SkillCategory))),
			};



			return ret;
		}
		
		private List<Tuple<string, Func<Skill, bool>>> GenerateDropdownFilter()
		{
			List<Tuple<string, Func<Skill, bool>>> ret = new List<Tuple<string, Func<Skill, bool>>>
			{
				new Tuple<string, Func<Skill, bool>>(LanguageManager.Instance.GetString("String_SkillFilterAll"), skill => true),
				new Tuple<string, Func<Skill, bool>>(LanguageManager.Instance.GetString("String_SkillFilterRatingAboveZero"),
					skill => skill.Rating > 0),
				new Tuple<string, Func<Skill, bool>>(LanguageManager.Instance.GetString("String_SkillFilterTotalRatingAboveZero"),
					skill => skill.Pool > 0),
				new Tuple<string, Func<Skill, bool>>(LanguageManager.Instance.GetString("String_SkillFilterRatingZero"),
					skill => skill.Rating == 0)
			};
			//TODO: TRANSLATIONS



			ret.AddRange(
				from XmlNode objNode 
				in XmlManager.Instance.Load("skills.xml").SelectNodes("/chummer/categories/category[@type = \"active\"]")
				let displayName = objNode.Attributes["translate"]?.InnerText ?? objNode.InnerText
				select new Tuple<string, Func<Skill, bool>>(
					$"{LanguageManager.Instance.GetString("Label_Category")} {displayName}", 
					skill => skill.SkillCategory == objNode.InnerText));

			ret.AddRange(
				from string attribute
				in new[]{"BOD", "AGI", "REA", "STR", "CHA", "INT", "LOG", "WIL", "MAG", "RES"} //TODO: This should be somewhere in Character or CharacterAttrib i think
				select new Tuple<string, Func<Skill, bool>>(
					$"{LanguageManager.Instance.GetString("String_ExpenseAttribute")}: {LanguageManager.Instance.GetString($"String_Attribute{attribute}Short")}",
					skill => skill.Attribute == attribute));

			ret.AddRange(
				from SkillGroup @group
				in _character.SkillsSection.SkillGroups
				select new Tuple<string, Func<Skill, bool>>(
					$"{LanguageManager.Instance.GetString("String_ExpenseSkillGroup")}: {@group.DisplayName}", 
					skill => skill.SkillGroupObject == @group));

			return ret;
		}

		private void MakeSkillDisplays()
		{
			Stopwatch sw = Stopwatch.StartNew();
			_groups = new BindingListDisplay<SkillGroup>(_character.SkillsSection.SkillGroups, @group => new SkillGroupControl(@group))
			{
				Location = new Point(0, 15),
			};
			sw.TaskEnd("_groups");

			splitSkills.Panel1.Controls.Add(_groups);

			sw.TaskEnd("_group add");

			_skills = new BindingListDisplay<Skill>(_character.SkillsSection.Skills, skill => new SkillControl2(skill))
			{
				Location = new Point(265, 39),
			};
			
			

			sw.TaskEnd("_skills");

			splitSkills.Panel1.Controls.Add(_skills);

			sw.TaskEnd("_skills add");

			_knoSkills = new BindingListDisplay<KnowledgeSkill>(_character.SkillsSection.KnowledgeSkills,
				knoSkill => new KnowledgeSkillControl(knoSkill))
			{
				Location = new Point(3, 50),
			};

			splitSkills.Panel2.Controls.Add(_knoSkills);
			
		}


		private void Panel1_Resize(object sender, EventArgs e)
		{
			int height = splitSkills.Panel1.Height;

			if (_skills != null)
			{
				_skills.Height = height - _skills.Top;
				_skills.Size = new Size(splitSkills.Panel1.Width - 265, splitSkills.Panel1.Height - 39);

			}
			if (_groups != null)
			{
				_groups.Height = height - _groups.Top;
				_groups.Size = new Size(255, splitSkills.Panel1.Height - 15);
			}
		}

		private void Panel2_Resize(object sender, EventArgs e)
		{
			if (_knoSkills != null)
			{
				_knoSkills.Size = new Size(splitSkills.Panel2.Width - 6, splitSkills.Panel2.Height - 50);
				_knoSkills.Height = splitSkills.Panel2.Height - 53;
			}
		}

		private void cboDisplayFilter_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox csender = (ComboBox) sender;
			Tuple<string, Func<Skill, bool>> selectedItem = (Tuple<string, Func<Skill, bool>>)csender.SelectedItem;

			_skills.Filter(selectedItem.Item2);
		}

		private void cboSort_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox csender = (ComboBox)sender;
			Tuple<string, IComparer<Skill>> selectedItem = (Tuple<string, IComparer<Skill>>)csender.SelectedItem;

			_skills.Sort(selectedItem.Item2);
		}

		private void btnExotic_Click(object sender, EventArgs e)
		{
			if (_character.Options.KarmaNewActiveSkill > _character.Karma)
			{
				MessageBox.Show(LanguageManager.Instance.GetString("Message_NotEnoughKarma"));
				return;
			}


			XmlDocument document = XmlManager.Instance.Load("skills.xml");
			frmSelectExoticSkill frmPickExoticSkill = new frmSelectExoticSkill();
			frmPickExoticSkill.ShowDialog(this);

			if (frmPickExoticSkill.DialogResult == DialogResult.Cancel)
				return;

			XmlNode node =
				document.SelectSingleNode("/chummer/skills/skill[name = \"" + frmPickExoticSkill.SelectedExoticSkill + "\"]");

			ExoticSkill skill = new ExoticSkill(ObjCharacter, node);
			skill.Specializations.Add(new SkillSpecialization(frmPickExoticSkill.SelectedExoticSkillSpecialisation, true));
			skill.Upgrade();
			ObjCharacter.SkillsSection.Skills.Add(skill);
		}

		private void UpdateKnoSkillRemaining()
		{
			lblKnowledgeSkillPoints.Text = $"{ObjCharacter.SkillsSection.KnowledgeSkillPointsRemain} {LanguageManager.Instance.GetString("String_Of")} {ObjCharacter.SkillsSection.KnowledgeSkillPoints}";
		}

		private void btnKnowledge_Click(object sender, EventArgs e)
		{
			ObjCharacter.SkillsSection.KnowledgeSkills.Add(new KnowledgeSkill(ObjCharacter));
		}

		
	}
}
