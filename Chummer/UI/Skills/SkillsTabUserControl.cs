using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
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
        private List<Tuple<string, Predicate<Skill>>> _dropDownList;
        private List<Tuple<string, IComparer<Skill>>>  _sortList;
        private readonly List<SkillControl2> _controls = new List<SkillControl2>();
        private bool _searchMode;
        private List<Tuple<string, Predicate<KnowledgeSkill>>> _dropDownKnowledgeList;
        private List<Tuple<string, IComparer<KnowledgeSkill>>> _sortKnowledgeList;

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
            cboDisplayFilter.ValueMember = "Item2";
            cboDisplayFilter.DisplayMember = "Item1";
            cboDisplayFilter.SelectedIndex = 0;
            cboDisplayFilter.MaxDropDownItems = _dropDownList.Count;

            cboDisplayFilterKnowledge.DataSource = _dropDownKnowledgeList;
            cboDisplayFilterKnowledge.ValueMember = "Item2";
            cboDisplayFilterKnowledge.DisplayMember = "Item1";
            cboDisplayFilterKnowledge.SelectedIndex = 0;
            cboDisplayFilterKnowledge.MaxDropDownItems = _dropDownKnowledgeList.Count;
            parts.TaskEnd("_ddl databind");

            cboSort.DataSource = _sortList;
            cboSort.ValueMember = "Item2";
            cboSort.DisplayMember = "Item1";
            cboSort.SelectedIndex = 0;
            cboSort.MaxDropDownItems = _sortList.Count;

            cboSortKnowledge.DataSource = _sortKnowledgeList;
            cboSortKnowledge.ValueMember = "Item2";
            cboSortKnowledge.DisplayMember = "Item1";
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

        private List<Tuple<string, IComparer<Skill>>> GenerateSortList()
        {
            List<Tuple<string, IComparer<Skill>>> ret = new List<Tuple<string, IComparer<Skill>>>()
            {
                new Tuple<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortAlphabetical"),
                    new SkillSorter(SkillsSection.CompareSkills)),
                new Tuple<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortRating"),
                    new SkillSorter((x, y) => y.Rating.CompareTo(x.Rating))),
                new Tuple<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortDicepool"),
                    new SkillSorter((x, y) => y.Pool.CompareTo(x.Pool))),
                new Tuple<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortLowerDicepool"),
                    new SkillSorter((x, y) => x.Pool.CompareTo(y.Pool))),
                new Tuple<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortAttributeValue"),
                    new SkillSorter((x, y) => y.AttributeModifiers.CompareTo(x.AttributeModifiers))),
                new Tuple<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortAttributeName"),
                    new SkillSorter((x, y) => string.Compare(x.Attribute, y.Attribute, StringComparison.Ordinal))),
                new Tuple<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortGroupName"),
                    new SkillSorter((x, y) => string.Compare(y.SkillGroup, x.SkillGroup, StringComparison.Ordinal))),
                new Tuple<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortGroupRating"),
                    new SkillSortBySkillGroup()),
                new Tuple<string, IComparer<Skill>>(LanguageManager.Instance.GetString("Skill_SortCategory"),
                    new SkillSorter((x, y) => string.Compare(x.SkillCategory, y.SkillCategory, StringComparison.Ordinal))),
            };

            return ret;
        }

        private List<Tuple<string, Predicate<Skill>>> GenerateDropdownFilter()
        {
            List<Tuple<string, Predicate<Skill>>> ret = new List<Tuple<string, Predicate<Skill>>>
            {
                new Tuple<string, Predicate<Skill>>(LanguageManager.Instance.GetString("String_Search"), null),
                new Tuple<string, Predicate<Skill>>(LanguageManager.Instance.GetString("String_SkillFilterAll"), skill => true),
                new Tuple<string, Predicate<Skill>>(LanguageManager.Instance.GetString("String_SkillFilterRatingAboveZero"),
                    skill => skill.Rating > 0),
                new Tuple<string, Predicate<Skill>>(LanguageManager.Instance.GetString("String_SkillFilterTotalRatingAboveZero"),
                    skill => skill.Pool > 0),
                new Tuple<string, Predicate<Skill>>(LanguageManager.Instance.GetString("String_SkillFilterRatingZero"),
                    skill => skill.Rating == 0),
                new Tuple<string, Predicate<Skill>>(LanguageManager.Instance.GetString("String_SkillFilterNoSkillGroup"),
                    skill => skill.SkillGroup.Length == 0),
                new Tuple<string, Predicate<Skill>>(LanguageManager.Instance.GetString("String_SkillFilterBrokenSkillGroup"),
                    skill => skill.Pool > 0 && (skill.SkillGroup.Length == 0 || skill.SkillGroupObject != null && skill.Rating > skill.SkillGroupObject.Rating))
            };
            //TODO: TRANSLATIONS



            ret.AddRange(
                from XmlNode objNode 
                in XmlManager.Instance.Load("skills.xml").SelectNodes("/chummer/categories/category[@type = \"active\"]")
                let displayName = objNode.Attributes["translate"]?.InnerText ?? objNode.InnerText
                select new Tuple<string, Predicate<Skill>>(
                    $"{LanguageManager.Instance.GetString("Label_Category")} {displayName}", 
                    skill => skill.SkillCategory == objNode.InnerText));

            ret.AddRange(
                from string attribute
                in new[]{"BOD", "AGI", "REA", "STR", "CHA", "INT", "LOG", "WIL", "MAG", "RES"} //TODO: This should be somewhere in Character or CharacterAttrib i think
                select new Tuple<string, Predicate<Skill>>(
                    $"{LanguageManager.Instance.GetString("String_ExpenseAttribute")}: {LanguageManager.Instance.GetString($"String_Attribute{attribute}Short")}",
                    skill => skill.Attribute == attribute));

            ret.AddRange(
                from SkillGroup @group
                in _character.SkillsSection.SkillGroups
                select new Tuple<string, Predicate<Skill>>(
                    $"{LanguageManager.Instance.GetString("String_ExpenseSkillGroup")}: {@group.DisplayName}", 
                    skill => skill.SkillGroupObject == @group));

            return ret;
        }

        private List<Tuple<string, IComparer<KnowledgeSkill>>> GenerateKnowledgeSortList()
        {
            List<Tuple<string, IComparer<KnowledgeSkill>>> ret = new List<Tuple<string, IComparer<KnowledgeSkill>>>()
            {
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.Instance.GetString("Skill_SortAlphabetical"),
                    new KnowledgeSkillSorter(KnowledgeSkill.CompareKnowledgeSkills)),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.Instance.GetString("Skill_SortRating"),
                    new KnowledgeSkillSorter((x, y) => y.Rating.CompareTo(x.Rating))),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.Instance.GetString("Skill_SortDicepool"),
                    new KnowledgeSkillSorter((x, y) => y.Pool.CompareTo(x.Pool))),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.Instance.GetString("Skill_SortLowerDicepool"),
                    new KnowledgeSkillSorter((x, y) => x.Pool.CompareTo(y.Pool))),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.Instance.GetString("Skill_SortAttributeValue"),
                    new KnowledgeSkillSorter((x, y) => y.AttributeModifiers.CompareTo(x.AttributeModifiers))),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.Instance.GetString("Skill_SortAttributeName"),
                    new KnowledgeSkillSorter((x, y) => string.Compare(x.Attribute, y.Attribute, StringComparison.Ordinal))),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.Instance.GetString("Skill_SortCategory"),
                    new KnowledgeSkillSorter((x, y) => string.Compare(x.SkillCategory, y.SkillCategory, StringComparison.Ordinal))),
            };

            return ret;
        }

        private List<Tuple<string, Predicate<KnowledgeSkill>>> GenerateKnowledgeDropdownFilter()
        {
            List<Tuple<string, Predicate<KnowledgeSkill>>> ret = new List<Tuple<string, Predicate<KnowledgeSkill>>>
            {
                //TODO: Search doesn't play nice with writeable name
                //new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.Instance.GetString("String_Search"), null),

                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.Instance.GetString("String_KnowledgeSkillFilterAll"), skill => true),
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.Instance.GetString("String_KnowledgeSkillFilterRatingAboveZero"),
                    skill => skill.Rating > 0),
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.Instance.GetString("String_KnowledgeSkillFilterTotalRatingAboveZero"),
                    skill => skill.Pool > 0),
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.Instance.GetString("String_KnowledgeSkillFilterRatingZero"),
                    skill => skill.Rating == 0)
            };
            //TODO: TRANSLATIONS



            ret.AddRange(
                from XmlNode objNode
                in XmlManager.Instance.Load("skills.xml").SelectNodes("/chummer/categories/category[@type = \"knowledge\"]")
                let displayName = objNode.Attributes["translate"]?.InnerText ?? objNode.InnerText
                select new Tuple<string, Predicate<KnowledgeSkill>>(
                    $"{LanguageManager.Instance.GetString("Label_Category")} {displayName}",
                    skill => skill.SkillCategory == objNode.InnerText));

            ret.AddRange(
                from string attribute
                in new[] { "INT", "LOG" } //TODO: This should be somewhere in Character or CharacterAttrib i think
                select new Tuple<string, Predicate<KnowledgeSkill>>(
                    $"{LanguageManager.Instance.GetString("String_ExpenseAttribute")}: {LanguageManager.Instance.GetString($"String_Attribute{attribute}Short")}",
                    skill => skill.Attribute == attribute));
            /*
            ret.AddRange(
                from SkillGroup @group
                in _character.SkillsSection.SkillGroups
                select new Tuple<string, Predicate<KnowledgeSkill>>(
                    $"{LanguageManager.Instance.GetString("String_ExpenseSkillGroup")}: {@group.DisplayName}",
                    skill => skill.SkillGroupObject == @group));
            */
            return ret;
        }

        private void MakeSkillDisplays()
        {
            Stopwatch sw = Stopwatch.StartNew();
            _groups = new BindingListDisplay<SkillGroup>(_character.SkillsSection.SkillGroups, group => new SkillGroupControl(group))
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
            _controls.Add(control);
            control.CustomAttributeChanged += Control_CustomAttributeChanged;
            return control;
        }

        private void Control_CustomAttributeChanged(object sender, EventArgs e)
        {
            btnResetCustomDisplayAttribute.Visible = _controls.Any(x => x.CustomAttributeSet);
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
            Tuple<string, Predicate<Skill>> selectedItem = (Tuple<string, Predicate<Skill>>)csender.SelectedItem;

            if (selectedItem.Item2 == null)
            {
                csender.DropDownStyle = ComboBoxStyle.DropDown;
                _searchMode = true;
                
            }
            else
            {
                csender.DropDownStyle = ComboBoxStyle.DropDownList;
                _searchMode = false;
                _skills.Filter(selectedItem.Item2);
            }
        }

        private void cboSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox csender = (ComboBox)sender;
            Tuple<string, IComparer<Skill>> selectedItem = (Tuple<string, IComparer<Skill>>)csender.SelectedItem;

            _skills.Sort(selectedItem.Item2);
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
                    skill.LoadDefaultType(skill.Name);


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
            foreach (SkillControl2 control2 in _controls.Where(x => x.CustomAttributeSet))
            {
                control2.ResetSelectAttribute();
            }
        }

        private void cboDisplayFilter_TextUpdate(object sender, EventArgs e)
        {
            if (_searchMode)
            {
                _skills.Filter(skill => GlobalOptions.CultureInfo.CompareInfo.IndexOf(skill.DisplayName, cboDisplayFilter.Text, CompareOptions.IgnoreCase) >= 0, true);
            }
        }

        private void cboSortKnowledge_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox csender = (ComboBox)sender;
            Tuple<string, IComparer<KnowledgeSkill>> selectedItem = (Tuple<string, IComparer<KnowledgeSkill>>)csender.SelectedItem;

            _knoSkills.Sort(selectedItem.Item2);
        }

        private void cboDisplayFilterKnowledge_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox csender = (ComboBox)sender;
            Tuple<string, Predicate<KnowledgeSkill>> selectedItem = (Tuple<string, Predicate<KnowledgeSkill>>)csender.SelectedItem;

            if (selectedItem.Item2 == null)
            {
                csender.DropDownStyle = ComboBoxStyle.DropDown;
                _searchMode = true;

            }
            else
            {
                csender.DropDownStyle = ComboBoxStyle.DropDownList;
                _searchMode = false;
                _knoSkills.Filter(selectedItem.Item2);
            }
        }

        private void cboDisplayFilterKnowledge_TextUpdate(object sender, EventArgs e)
        {
            if (_searchMode)
            {
                _knoSkills.Filter(skill => GlobalOptions.CultureInfo.CompareInfo.IndexOf(skill.WriteableName, cboDisplayFilter.Text, CompareOptions.IgnoreCase) >= 0, true);
            }
        }
    }
}
