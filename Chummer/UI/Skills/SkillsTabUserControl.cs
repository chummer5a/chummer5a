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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Skills;
using Chummer.UI.Shared;
using Chummer.Backend.Attributes;

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

            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
        }

        public void MissingDatabindingsWorkaround()
        {
            UpdateKnoSkillRemaining();
        }

        private bool _initialized = false;
        private Character _character = null;
        private List<Tuple<string, Predicate<Skill>>> _dropDownList;
        private List<Tuple<string, IComparer<Skill>>>  _sortList;
        private readonly List<SkillControl2> _controls = new List<SkillControl2>();
        private bool _searchMode = false;
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
            RealLoad();
        }

        private void RealLoad() //Cannot be called before both Loaded are called and it have a character object
        {
            if (_initialized || _character == null)
                return;
            _initialized = true;  //Only do once
            Stopwatch sw = Stopwatch.StartNew();  //Benchmark, should probably remove in release 
            Stopwatch parts = Stopwatch.StartNew();
            //Keep everything visible until ready to display everything. This 
            //seems to prevent redrawing everything each time anything is added
            //Not benched, but should be faster

            //Might also be useless horseshit, 2 lines

            //Visible = false;
            SuspendLayout();
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

            _dropDownList = (List<Tuple<string, Predicate<Skill>>>)GenerateDropdownFilter();
            _dropDownKnowledgeList = (List<Tuple<string, Predicate<KnowledgeSkill>>>)GenerateKnowledgeDropdownFilter();

            parts.TaskEnd("GenerateDropDown()");

            _sortList = (List<Tuple<string, IComparer<Skill>>>)GenerateSortList();
            _sortKnowledgeList = (List<Tuple<string, IComparer<KnowledgeSkill>>>)GenerateKnowledgeSortList();

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
            ResumeLayout(true);
            sw.Stop();
            Debug.WriteLine("RealLoad() in {0} ms", sw.Elapsed.TotalMilliseconds);
        }

        private static IList<Tuple<string, IComparer<Skill>>> GenerateSortList()
        {
            List<Tuple<string, IComparer<Skill>>> ret = new List<Tuple<string, IComparer<Skill>>>()
            {
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortAlphabetical", GlobalOptions.Language),
                    new SkillSorter(SkillsSection.CompareSkills)),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortRating", GlobalOptions.Language),
                    new SkillSorter((x, y) => y.Rating.CompareTo(x.Rating))),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortDicepool", GlobalOptions.Language),
                    new SkillSorter((x, y) => y.Pool.CompareTo(x.Pool))),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortLowerDicepool", GlobalOptions.Language),
                    new SkillSorter((x, y) => x.Pool.CompareTo(y.Pool))),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortAttributeValue", GlobalOptions.Language),
                    new SkillSorter((x, y) => y.AttributeModifiers.CompareTo(x.AttributeModifiers))),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortAttributeName", GlobalOptions.Language),
                    new SkillSorter((x, y) => string.Compare(x.Attribute, y.Attribute, StringComparison.Ordinal))),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortGroupName", GlobalOptions.Language),
                    new SkillSorter((x, y) =>
                    {
                        string strXGroup = x.SkillGroup;
                        string strYGroup = y.SkillGroup;
                        if (string.IsNullOrEmpty(strXGroup))
                        {
                            if (string.IsNullOrEmpty(strYGroup))
                                return 0;
                            else
                                return -1;
                        }
                        else if (string.IsNullOrEmpty(strYGroup))
                            return 1;
                        return string.Compare(x.SkillGroup, y.SkillGroup, StringComparison.Ordinal);
                    })),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortGroupRating", GlobalOptions.Language),
                    new SkillSortBySkillGroup()),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortCategory", GlobalOptions.Language),
                    new SkillSorter((x, y) => string.Compare(x.SkillCategory, y.SkillCategory, StringComparison.Ordinal))),
            };

            return ret;
        }

        private IList<Tuple<string, Predicate<Skill>>> GenerateDropdownFilter()
        {
            List<Tuple<string, Predicate<Skill>>> ret = new List<Tuple<string, Predicate<Skill>>>
            {
                new Tuple<string, Predicate<Skill>>(LanguageManager.GetString("String_Search", GlobalOptions.Language), null),
                new Tuple<string, Predicate<Skill>>(LanguageManager.GetString("String_SkillFilterAll", GlobalOptions.Language), skill => true),
                new Tuple<string, Predicate<Skill>>(LanguageManager.GetString("String_SkillFilterRatingAboveZero", GlobalOptions.Language),
                    skill => skill.Rating > 0),
                new Tuple<string, Predicate<Skill>>(LanguageManager.GetString("String_SkillFilterTotalRatingAboveZero", GlobalOptions.Language),
                    skill => skill.Pool > 0),
                new Tuple<string, Predicate<Skill>>(LanguageManager.GetString("String_SkillFilterRatingZero", GlobalOptions.Language),
                    skill => skill.Rating == 0),
                new Tuple<string, Predicate<Skill>>(LanguageManager.GetString("String_SkillFilterNoSkillGroup", GlobalOptions.Language),
                    skill => skill.SkillGroup.Length == 0),
                new Tuple<string, Predicate<Skill>>(LanguageManager.GetString("String_SkillFilterBrokenSkillGroup", GlobalOptions.Language),
                    skill => skill.Pool > 0 && (skill.SkillGroup.Length == 0 || skill.SkillGroupObject != null && skill.Rating > skill.SkillGroupObject.Rating))
            };
            //TODO: TRANSLATIONS

            ret.AddRange(
                from XmlNode objNode 
                in XmlManager.Load("skills.xml").SelectNodes("/chummer/categories/category[@type = \"active\"]")
                let displayName = objNode.Attributes["translate"]?.InnerText ?? objNode.InnerText
                select new Tuple<string, Predicate<Skill>>(
                    $"{LanguageManager.GetString("Label_Category", GlobalOptions.Language)} {displayName}", 
                    skill => skill.SkillCategory == objNode.InnerText));

            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                string strAttributeShort = LanguageManager.GetString($"String_Attribute{strAttribute}Short", GlobalOptions.Language, false);
                if (!string.IsNullOrEmpty(strAttributeShort))
                {
                    ret.Add(new Tuple<string, Predicate<Skill>>($"{LanguageManager.GetString("String_ExpenseAttribute", GlobalOptions.Language)}: {strAttributeShort}",
                        skill => skill.Attribute == strAttribute));
                }
            }

            ret.AddRange(
                from SkillGroup @group
                in _character.SkillsSection.SkillGroups
                select new Tuple<string, Predicate<Skill>>(
                    $"{LanguageManager.GetString("String_ExpenseSkillGroup", GlobalOptions.Language)}: {@group.DisplayName}", 
                    skill => skill.SkillGroupObject == @group));

            return ret;
        }

        private static IList<Tuple<string, IComparer<KnowledgeSkill>>> GenerateKnowledgeSortList()
        {
            List<Tuple<string, IComparer<KnowledgeSkill>>> ret = new List<Tuple<string, IComparer<KnowledgeSkill>>>()
            {
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortAlphabetical", GlobalOptions.Language),
                    new KnowledgeSkillSorter(SkillsSection.CompareSkills)),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortRating", GlobalOptions.Language),
                    new KnowledgeSkillSorter((x, y) => y.Rating.CompareTo(x.Rating))),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortDicepool", GlobalOptions.Language),
                    new KnowledgeSkillSorter((x, y) => y.Pool.CompareTo(x.Pool))),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortLowerDicepool", GlobalOptions.Language),
                    new KnowledgeSkillSorter((x, y) => x.Pool.CompareTo(y.Pool))),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortAttributeValue", GlobalOptions.Language),
                    new KnowledgeSkillSorter((x, y) => y.AttributeModifiers.CompareTo(x.AttributeModifiers))),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortAttributeName", GlobalOptions.Language),
                    new KnowledgeSkillSorter((x, y) => string.Compare(x.Attribute, y.Attribute, StringComparison.Ordinal))),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortCategory", GlobalOptions.Language),
                    new KnowledgeSkillSorter((x, y) => string.Compare(x.SkillCategory, y.SkillCategory, StringComparison.Ordinal))),
            };

            return ret;
        }

        private static IList<Tuple<string, Predicate<KnowledgeSkill>>> GenerateKnowledgeDropdownFilter()
        {
            List<Tuple<string, Predicate<KnowledgeSkill>>> ret = new List<Tuple<string, Predicate<KnowledgeSkill>>>
            {
                //TODO: Search doesn't play nice with writeable name
                //new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_Search"), null),

                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_KnowledgeSkillFilterAll", GlobalOptions.Language), skill => true),
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_KnowledgeSkillFilterRatingAboveZero", GlobalOptions.Language),
                    skill => skill.Rating > 0),
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_KnowledgeSkillFilterTotalRatingAboveZero", GlobalOptions.Language),
                    skill => skill.Pool > 0),
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_KnowledgeSkillFilterRatingZero", GlobalOptions.Language),
                    skill => skill.Rating == 0)
            };
            //TODO: TRANSLATIONS

            ret.AddRange(
                from XmlNode objNode
                in XmlManager.Load("skills.xml").SelectNodes("/chummer/categories/category[@type = \"knowledge\"]")
                let displayName = objNode.Attributes["translate"]?.InnerText ?? objNode.InnerText
                select new Tuple<string, Predicate<KnowledgeSkill>>(
                    $"{LanguageManager.GetString("Label_Category", GlobalOptions.Language)} {displayName}",
                    skill => skill.SkillCategory == objNode.InnerText));

            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                string strAttributeShort = LanguageManager.GetString($"String_Attribute{strAttribute}Short", GlobalOptions.Language, false);
                if (!string.IsNullOrEmpty(strAttributeShort))
                {
                    ret.Add(new Tuple<string, Predicate<KnowledgeSkill>>($"{LanguageManager.GetString("String_ExpenseAttribute", GlobalOptions.Language)}: {strAttributeShort}",
                        skill => skill.Attribute == strAttribute));
                }
            }
            /*
            ret.AddRange(
                from SkillGroup @group
                in _character.SkillsSection.SkillGroups
                select new Tuple<string, Predicate<KnowledgeSkill>>(
                    $"{LanguageManager.GetString("String_ExpenseSkillGroup")}: {@group.DisplayName}",
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
            _groups.Filter(x => x.SkillList.Any(y => _character.SkillsSection.SkillsDictionary.ContainsKey(y.Name)), true);

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
                _groups.Size = new Size(intWidth, height - _groups.Top);
            }
            if (_skills != null)
            {
                _skills.Size = new Size(splitSkills.Panel1.Width - (intWidth + 10), height - _skills.Top);
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
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language));
                return;
            }


            XmlDocument document = XmlManager.Load("skills.xml");
            frmSelectExoticSkill frmPickExoticSkill = new frmSelectExoticSkill(_character);
            frmPickExoticSkill.ShowDialog(this);

            if (frmPickExoticSkill.DialogResult == DialogResult.Cancel)
                return;

            XmlNode node = document.SelectSingleNode("/chummer/skills/skill[name = \"" + frmPickExoticSkill.SelectedExoticSkill + "\"]");

            ExoticSkill skill = new ExoticSkill(ObjCharacter, node)
            {
                Specific = frmPickExoticSkill.SelectedExoticSkillSpecialisation
            };
            skill.Upgrade();
            ObjCharacter.SkillsSection.Skills.Add(skill);
            ObjCharacter.SkillsSection.SkillsDictionary.Add(skill.Name + " (" + skill.DisplaySpecializationMethod(GlobalOptions.DefaultLanguage) + ')', skill);
        }

        private void UpdateKnoSkillRemaining()
        {
            lblKnowledgeSkillPoints.Text = $"{ObjCharacter.SkillsSection.KnowledgeSkillPointsRemain} {LanguageManager.GetString("String_Of", GlobalOptions.Language)} {ObjCharacter.SkillsSection.KnowledgeSkillPoints}";
        }

        private void btnKnowledge_Click(object sender, EventArgs e)
        {
            if (_character.Created)
            {
                List<ListItem> lstDefaultKnowledgeSkills = KnowledgeSkill.DefaultKnowledgeSkills(GlobalOptions.Language).ToList();
                lstDefaultKnowledgeSkills.Sort(CompareListItems.CompareNames);
                frmSelectItem form = new frmSelectItem
                {
                    Description = LanguageManager.GetString("Label_Options_NewKnowledgeSkill", GlobalOptions.Language),
                    DropdownItems = lstDefaultKnowledgeSkills
                };

                if (form.ShowDialog() == DialogResult.OK)
                {
                    KnowledgeSkill skill = new KnowledgeSkill(ObjCharacter)
                    {
                        WriteableName = form.SelectedItem
                    };

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
                _skills.Filter(skill => GlobalOptions.CultureInfo.CompareInfo.IndexOf(skill.DisplayNameMethod(GlobalOptions.Language), cboDisplayFilter.Text, CompareOptions.IgnoreCase) >= 0, true);
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
