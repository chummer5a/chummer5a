using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
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
		private List<KeyValuePair<string, Predicate<Skill>>> _dropDownList;
		private List<KeyValuePair<string, IComparer<Skill>>>  _sortList;
		private List<SkillControl2> controls = new List<SkillControl2>();
		private bool _searchMode;
		private List<KeyValuePair<string, Predicate<KnowledgeSkill>>> _dropDownKnowledgeList;
		private List<KeyValuePair<string, IComparer<KnowledgeSkill>>> _sortKnowledgeList;

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

		private void RealLoad() //Cannot be called before both Loaded are called and it have a character object
		{
			if (_initialized) return;

			if (!(_character != null && _loadCalled)) return;

			_initialized = true;  //Only do once
			Stopwatch sw = Stopwatch.StartNew();  //Benchmark, should probably remove in release 
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
			_dropDownKnowledgeList = GenerateKnowledgeDropdownFilter();

			parts.TaskEnd("GenerateDropDown()");

			_sortList = GenerateSortList();
			_sortKnowledgeList = GenerateKnowledgeSortList();

			parts.TaskEnd("GenerateSortList()");

            cboDisplayFilter.BeginUpdate();
            cboDisplayFilterKnowledge.BeginUpdate();
            cboSort.BeginUpdate();
            cboSortKnowledge.BeginUpdate();

            cboDisplayFilter.DataSource = _dropDownList;
			cboDisplayFilter.ValueMember = "Value";
			cboDisplayFilter.DisplayMember = "Key";
			cboDisplayFilter.SelectedIndex = 0;
			cboDisplayFilter.MaxDropDownItems = _dropDownList.Count;

			cboDisplayFilterKnowledge.DataSource = _dropDownKnowledgeList;
			cboDisplayFilterKnowledge.ValueMember = "Value";
			cboDisplayFilterKnowledge.DisplayMember = "Key";
			cboDisplayFilterKnowledge.SelectedIndex = 0;
			cboDisplayFilterKnowledge.MaxDropDownItems = _dropDownKnowledgeList.Count;
			parts.TaskEnd("_ddl databind");

			cboSort.DataSource = _sortList;
			cboSort.ValueMember = "Value";
			cboSort.DisplayMember = "Key";
			cboSort.SelectedIndex = 0;
			cboSort.MaxDropDownItems = _sortList.Count;

			cboSortKnowledge.DataSource = _sortKnowledgeList;
			cboSortKnowledge.ValueMember = "Value";
			cboSortKnowledge.DisplayMember = "Key";
			cboSortKnowledge.SelectedIndex = 0;
			cboSortKnowledge.MaxDropDownItems = _sortKnowledgeList.Count;

            cboDisplayFilter.EndUpdate();
            cboDisplayFilterKnowledge.EndUpdate();
            cboSort.EndUpdate();
            cboSortKnowledge.EndUpdate();

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

			if (!_character.Created)
			{
				lblKnoKarma.Visible = true;
				lblActiveKarma.Visible = true;
				lblGroupKarma.Visible = true;

				if (_character.BuildMethod.HaveSkillPoints())
				{
					lblActiveSp.Visible = true;
					lblBuyWithKarma.Visible = true;
				}


				//Because visible is broken in databindings
				_character.SkillsSection.PropertyChanged += (o, e) =>
				{
					if (e.PropertyName ==
					    nameof(SkillsSection.HasKnowledgePoints))
					{
						lblKnoSp.Visible =
							_character.SkillsSection.HasKnowledgePoints;
						lblKnoBwk.Visible = _character.SkillsSection.HasKnowledgePoints;
					}
				};
				//lblKnoSp.Visible = true;
				//lblKnoSp.DataBindings.Add("Visible", _character.SkillsSection, nameof(SkillsSection.HasKnowledgePoints), false, DataSourceUpdateMode.OnPropertyChanged);
				//lblKnoBwk.DataBindings.Add("Visible", _character.SkillsSection, nameof(SkillsSection.HasKnowledgePoints), false, DataSourceUpdateMode.OnPropertyChanged);
			}
		}

		private List<KeyValuePair<string, IComparer<Skill>>> GenerateSortList()
		{
			List<KeyValuePair<string, IComparer<Skill>>> ret = new List<KeyValuePair<string, IComparer<Skill>>>()
			{
				new KeyValuePair<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortAlphabetical"),
					new SkillSorter(SkillsSection.CompareSkills)),
				new KeyValuePair<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortRating"),
					new SkillSorter((x, y) => y.Rating.CompareTo(x.Rating))),
				new KeyValuePair<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortDicepool"),
					new SkillSorter((x, y) => y.Pool.CompareTo(x.Pool))),
				new KeyValuePair<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortLowerDicepool"),
					new SkillSorter((x, y) => x.Pool.CompareTo(y.Pool))),
				new KeyValuePair<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortAttributeValue"),
					new SkillSorter((x, y) => y.AttributeModifiers.CompareTo(x.AttributeModifiers))),
				new KeyValuePair<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortAttributeName"),
					new SkillSorter((x, y) => x.Attribute.CompareTo(y.Attribute))),
				new KeyValuePair<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortGroupName"),
					new SkillSorter((x, y) => y.SkillGroup.CompareTo(x.SkillGroup))),
				new KeyValuePair<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortGroupRating"),
					new SkillSortBySkillGroup()),
				new KeyValuePair<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortCategory"),
					new SkillSorter((x, y) => x.SkillCategory.CompareTo(y.SkillCategory))),
			};



			return ret;
		}

        static string[] strKnoAttributeStrings = { "INT", "LOG" };

        private List<KeyValuePair<string, Predicate<Skill>>> GenerateDropdownFilter()
		{
			List<KeyValuePair<string, Predicate<Skill>>> ret = new List<KeyValuePair<string, Predicate<Skill>>>
			{
				new KeyValuePair<string, Predicate<Skill>>(LanguageManager.Instance.GetString("String_Search"), null),
				new KeyValuePair<string, Predicate<Skill>>(LanguageManager.Instance.GetString("String_SkillFilterAll"), skill => true),
				new KeyValuePair<string, Predicate<Skill>>(LanguageManager.Instance.GetString("String_SkillFilterRatingAboveZero"),
					skill => skill.Rating > 0),
				new KeyValuePair<string, Predicate<Skill>>(LanguageManager.Instance.GetString("String_SkillFilterTotalRatingAboveZero"),
					skill => skill.Pool > 0),
				new KeyValuePair<string, Predicate<Skill>>(LanguageManager.Instance.GetString("String_SkillFilterRatingZero"),
					skill => skill.Rating == 0),
                new KeyValuePair<string, Predicate<Skill>>(LanguageManager.Instance.GetString("String_SkillFilterNoSkillGroup"),
                    skill => skill.SkillGroup.Length == 0),
                new KeyValuePair<string, Predicate<Skill>>(LanguageManager.Instance.GetString("String_SkillFilterBrokenSkillGroup"),
                    skill => skill.Pool > 0 && (skill.SkillGroup.Length == 0 || skill.SkillGroupObject != null && skill.Rating > skill.SkillGroupObject.Rating))
            };
            //TODO: TRANSLATIONS

		    List<KeyValuePair<string, Predicate<Skill>>> lstExtraFilters = new List<KeyValuePair<string, Predicate<Skill>>>();
            XmlDocument objXmlDocument = XmlManager.Instance.Load("skills.xml");
		    XmlNodeList objXmlNodeList = objXmlDocument?.SelectNodes("/chummer/categories/category[@type = \"active\"]");
		    if (objXmlNodeList != null)
		    {
		        foreach (XmlNode objLoopNode in objXmlNodeList)
		        {
		            string strDisplayName = objLoopNode.Attributes?["translate"]?.InnerText ?? objLoopNode.InnerText;
		            lstExtraFilters.Add(new KeyValuePair<string, Predicate<Skill>>(
		                $"{LanguageManager.Instance.GetString("Label_Category")} {strDisplayName}",
		                skill => skill.SkillCategory == objLoopNode.InnerText));
		        }
		    }

		    foreach (string strAttribute in Character.strAttributeStrings)
            {
                lstExtraFilters.Add(new KeyValuePair<string, Predicate<Skill>>(
                    $"{LanguageManager.Instance.GetString("String_ExpenseAttribute")}: {LanguageManager.Instance.GetString($"String_Attribute{strAttribute}Short")}",
                    skill => skill.Attribute == strAttribute));
            }

            foreach (SkillGroup objLoopSkillGroup in _character.SkillsSection.SkillGroups)
            {
                lstExtraFilters.Add(new KeyValuePair<string, Predicate<Skill>>(
                    $"{LanguageManager.Instance.GetString("String_ExpenseSkillGroup")}: {objLoopSkillGroup.DisplayName}",
                    skill => skill.SkillGroupObject.Equals(objLoopSkillGroup)));
            }

		    ret.AddRange(lstExtraFilters);
            return ret;
		}

		private List<KeyValuePair<string, IComparer<KnowledgeSkill>>> GenerateKnowledgeSortList()
		{
			List<KeyValuePair<string, IComparer<KnowledgeSkill>>> ret = new List<KeyValuePair<string, IComparer<KnowledgeSkill>>>()
			{
				new KeyValuePair<string, IComparer<KnowledgeSkill>>(LanguageManager.Instance.GetString("Skill_SortAlphabetical"),
					new KnowledgeSkillSorter(KnowledgeSkill.CompareKnowledgeSkills)),
				new KeyValuePair<string, IComparer<KnowledgeSkill>>(LanguageManager.Instance.GetString("Skill_SortRating"),
					new KnowledgeSkillSorter((x, y) => y.Rating.CompareTo(x.Rating))),
				new KeyValuePair<string, IComparer<KnowledgeSkill>>(LanguageManager.Instance.GetString("Skill_SortDicepool"),
					new KnowledgeSkillSorter((x, y) => y.Pool.CompareTo(x.Pool))),
				new KeyValuePair<string, IComparer<KnowledgeSkill>>(LanguageManager.Instance.GetString("Skill_SortLowerDicepool"),
					new KnowledgeSkillSorter((x, y) => x.Pool.CompareTo(y.Pool))),
				new KeyValuePair<string, IComparer<KnowledgeSkill>>(LanguageManager.Instance.GetString("Skill_SortAttributeValue"),
					new KnowledgeSkillSorter((x, y) => y.AttributeModifiers.CompareTo(x.AttributeModifiers))),
				new KeyValuePair<string, IComparer<KnowledgeSkill>>(LanguageManager.Instance.GetString("Skill_SortAttributeName"),
					new KnowledgeSkillSorter((x, y) => x.Attribute.CompareTo(y.Attribute))),
				new KeyValuePair<string, IComparer<KnowledgeSkill>>(LanguageManager.Instance.GetString("Skill_SortCategory"),
					new KnowledgeSkillSorter((x, y) => x.SkillCategory.CompareTo(y.SkillCategory))),
			};
			return ret;
		}

		private List<KeyValuePair<string, Predicate<KnowledgeSkill>>> GenerateKnowledgeDropdownFilter()
		{
			List<KeyValuePair<string, Predicate<KnowledgeSkill>>> ret = new List<KeyValuePair<string, Predicate<KnowledgeSkill>>>
			{
				//TODO: Search doesn't play nice with writeable name
				//new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.Instance.GetString("String_Search"), null),

				new KeyValuePair<string, Predicate<KnowledgeSkill>>(LanguageManager.Instance.GetString("String_KnowledgeSkillFilterAll"), skill => true),
				new KeyValuePair<string, Predicate<KnowledgeSkill>>(LanguageManager.Instance.GetString("String_KnowledgeSkillFilterRatingAboveZero"),
					skill => skill.Rating > 0),
				new KeyValuePair<string, Predicate<KnowledgeSkill>>(LanguageManager.Instance.GetString("String_KnowledgeSkillFilterTotalRatingAboveZero"),
					skill => skill.Pool > 0),
				new KeyValuePair<string, Predicate<KnowledgeSkill>>(LanguageManager.Instance.GetString("String_KnowledgeSkillFilterRatingZero"),
					skill => skill.Rating == 0)
			};
            //TODO: TRANSLATIONS

            List<KeyValuePair<string, Predicate<KnowledgeSkill>>> lstExtraFilters = new List<KeyValuePair<string, Predicate<KnowledgeSkill>>>();
            XmlDocument objXmlDocument = XmlManager.Instance.Load("skills.xml");
            XmlNodeList objXmlNodeList = objXmlDocument?.SelectNodes("/chummer/categories/category[@type = \"knowledge\"]");
            if (objXmlNodeList != null)
            {
                foreach (XmlNode objLoopNode in objXmlNodeList)
                {
                    string strDisplayName = objLoopNode.Attributes?["translate"]?.InnerText ?? objLoopNode.InnerText;
                    lstExtraFilters.Add(new KeyValuePair<string, Predicate<KnowledgeSkill>>(
                        $"{LanguageManager.Instance.GetString("Label_Category")} {strDisplayName}",
                        skill => skill.SkillCategory == objLoopNode.InnerText));
                }
            }

            foreach (string strAttribute in strKnoAttributeStrings)
            {
                lstExtraFilters.Add(new KeyValuePair<string, Predicate<KnowledgeSkill>>(
                    $"{LanguageManager.Instance.GetString("String_ExpenseAttribute")}: {LanguageManager.Instance.GetString($"String_Attribute{strAttribute}Short")}",
                    skill => skill.Attribute == strAttribute));
            }
            /*
			foreach (SkillGroup objLoopSkillGroup in _character.SkillsSection.SkillGroups)
            {
                lstExtraFilters.Add(new KeyValuePair<string, Predicate<KnowledgeSkill>>(
                    $"{LanguageManager.Instance.GetString("String_ExpenseSkillGroup")}: {objLoopSkillGroup.DisplayName}",
                    skill => skill.SkillGroupObject.Equals(objLoopSkillGroup)));
            }
			*/
            ret.AddRange(lstExtraFilters);
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

			_skills = new BindingListDisplay<Skill>(_character.SkillsSection.Skills, MakeActiveSkill)
			{
				Location = new Point(265, 42),
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

		private Control MakeActiveSkill(Skill arg)
		{
			SkillControl2 control = new SkillControl2(arg);
			controls.Add(control);
			control.CustomAttributeChanged += Control_CustomAttributeChanged;
			return control;
		}

		private void Control_CustomAttributeChanged(object sender, EventArgs e)
		{
		    foreach (SkillControl2 objLoopControl in controls)
		    {
		        if (objLoopControl.CustomAttributeSet)
		        {
                    btnResetCustomDisplayAttribute.Visible = true;
		            return;
		        }
		    }
		    btnResetCustomDisplayAttribute.Visible = false;
		}

		private void Panel1_Resize(object sender, EventArgs e)
		{
			int height = splitSkills.Panel1.Height;
			int intWidth = 255;
			if (_groups != null)
			{
				foreach (SkillGroupControl objControl in _groups.Controls[0].Controls)
				{
					if (objControl.PreferredSize.Width > intWidth)
					{
						intWidth = objControl.PreferredSize.Width;
					}
				}
				_groups.Height = height - _groups.Top;
				_groups.Size = new Size(intWidth, splitSkills.Panel1.Height - 15);
			}
			if (_skills != null)
			{
				_skills.Height = height - _skills.Top;
				_skills.Size = new Size(splitSkills.Panel1.Width - (intWidth+10), splitSkills.Panel1.Height - 39);

			}
		}

		private void Panel2_Resize(object sender, EventArgs e)
		{
			if (_knoSkills != null)
			{
				_knoSkills.Size = new Size(splitSkills.Panel2.Width - 6, splitSkills.Panel2.Height - 53);
				//_knoSkills.Height = splitSkills.Panel2.Height - 53;
			}
		}

		private void cboDisplayFilter_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox csender = (ComboBox) sender;
            KeyValuePair<string, Predicate<Skill>> selectedItem = (KeyValuePair<string, Predicate<Skill>>)csender.SelectedItem;

			if (selectedItem.Value == null)
			{
				csender.DropDownStyle = ComboBoxStyle.DropDown;
				_searchMode = true;
			}
			else
			{
				csender.DropDownStyle = ComboBoxStyle.DropDownList;
				_searchMode = false;
				_skills.Filter(selectedItem.Value);
			}
		}

		private void cboSort_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox csender = (ComboBox)sender;
            KeyValuePair<string, IComparer<Skill>> selectedItem = (KeyValuePair<string, IComparer<Skill>>)csender.SelectedItem;

			_skills.Sort(selectedItem.Value);
		}

		private void btnExotic_Click(object sender, EventArgs e)
		{
			if (_character.Options.KarmaNewActiveSkill > _character.Karma && _character.Created)
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
			skill.Specific = frmPickExoticSkill.SelectedExoticSkillSpecialisation;
			skill.Upgrade();
			ObjCharacter.SkillsSection.Skills.Add(skill);
		}

		private void UpdateKnoSkillRemaining()
		{
			lblKnowledgeSkillPoints.Text = $"{ObjCharacter.SkillsSection.KnowledgeSkillPointsRemain} {LanguageManager.Instance.GetString("String_Of")} {ObjCharacter.SkillsSection.KnowledgeSkillPoints}";
		}

		private void btnKnowledge_Click(object sender, EventArgs e)
		{
		    if (_character.Created)
		    {
		        frmSelectItem form = new frmSelectItem();
		        form.Description = LanguageManager.Instance.GetString("Label_Options_NewKnowledgeSkill");
		        form.DropdownItems = KnowledgeSkill.DefaultKnowledgeSkillCatagories;

		        if (form.ShowDialog() == DialogResult.OK)
		        {
		            KnowledgeSkill skill = new KnowledgeSkill(ObjCharacter);
		            skill.WriteableName = form.SelectedItem;


		            ObjCharacter.SkillsSection.KnowledgeSkills.Add(skill);
		        }
		    }
		    else
		    {
                ObjCharacter.SkillsSection.KnowledgeSkills.Add(new KnowledgeSkill(ObjCharacter));
            }
		}

		private void btnResetCustomDisplayAttribute_Click(object sender, EventArgs e)
		{
            foreach (SkillControl2 objLoopControl in controls)
            {
                if (objLoopControl.CustomAttributeSet)
                {
                    objLoopControl.ResetSelectAttribute();
                }
            }
		}

		private void cboDisplayFilter_TextUpdate(object sender, EventArgs e)
		{
			if (_searchMode)
			{
				_skills.Filter(skill => CultureInfo.InvariantCulture.CompareInfo.IndexOf(skill.DisplayName, cboDisplayFilter.Text, CompareOptions.IgnoreCase) >= 0, true);
			}
		}

		private void cboSortKnowledge_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox csender = (ComboBox)sender;
            KeyValuePair<string, IComparer<KnowledgeSkill>> selectedItem = (KeyValuePair<string, IComparer<KnowledgeSkill>>)csender.SelectedItem;

			_knoSkills.Sort(selectedItem.Value);
		}

		private void cboDisplayFilterKnowledge_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox csender = (ComboBox)sender;
            KeyValuePair<string, Predicate<KnowledgeSkill>> selectedItem = (KeyValuePair<string, Predicate<KnowledgeSkill>>)csender.SelectedItem;

			if (selectedItem.Value == null)
			{
				csender.DropDownStyle = ComboBoxStyle.DropDown;
				_searchMode = true;
			}
			else
			{
				csender.DropDownStyle = ComboBoxStyle.DropDownList;
				_searchMode = false;
				_knoSkills.Filter(selectedItem.Value);
			}
		}

		private void cboDisplayFilterKnowledge_TextUpdate(object sender, EventArgs e)
		{
			if (_searchMode)
			{
				_knoSkills.Filter(skill => CultureInfo.InvariantCulture.CompareInfo.IndexOf(skill.WriteableName, cboDisplayFilter.Text, CompareOptions.IgnoreCase) >= 0, true);
			}
		}
	}
}
