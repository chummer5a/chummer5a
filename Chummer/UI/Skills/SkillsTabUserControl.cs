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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        public event PropertyChangedEventHandler MakeDirtyWithCharacterUpdate;

        private BindingListDisplay<Skill> _lstActiveSkills;
        private BindingListDisplay<SkillGroup> _lstSkillGroups;
        private BindingListDisplay<KnowledgeSkill> _lstKnowledgeSkills; 

        public SkillsTabUserControl()
        {
            InitializeComponent();

            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            _lstDropDownActiveSkills = GenerateDropdownFilter();
            _lstDropDownKnowledgeSkills = GenerateKnowledgeDropdownFilter();
            _sortList = GenerateSortList();
            _lstSortKnowledgeList = GenerateKnowledgeSortList();

            _lstSkillControls.CollectionChanged += LstSkillControlsOnCollectionChanged;
        }

        private void LstSkillControlsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!HasLoaded) return;
            int intNameLabelWidth = _lstSkillControls.Max(i => i.NameWidth);

            foreach (SkillControl2 objSkillControl in _lstSkillControls)
            {
                objSkillControl.MoveControls(intNameLabelWidth);
            }

            if (_objCharacter.Created) return;
            int intRatingLabelWidth = _lstSkillControls.Max(i => i.NudSkillWidth);
            lblActiveSp.Left = lblActiveSkills.Left + intNameLabelWidth + 6;
            lblActiveKarma.Left = lblActiveSp.Left + intRatingLabelWidth + 6;
            //When in karma mode, we will occasionally fail to load the proper size; break if this is the case during debug and try a failback. 
            if (lblActiveKarma.Left >= intNameLabelWidth) return;
            Utils.BreakIfDebug();
            lblActiveKarma.Left = intNameLabelWidth + 6;
        }

        public void MissingDatabindingsWorkaround()
        {
            if (!_objCharacter.Created)
                UpdateKnoSkillRemaining();
        }

        private void UpdateKnoSkillRemaining()
        {
            lblKnowledgeSkillPoints.Text = $"{_objCharacter.SkillsSection.KnowledgeSkillPointsRemain}{LanguageManager.GetString("String_Of", GlobalOptions.Language)}{_objCharacter.SkillsSection.KnowledgeSkillPoints}";
        }

        public bool HasLoaded => _objCharacter != null;
        private Character _objCharacter;
        private readonly IList<Tuple<string, Predicate<Skill>>> _lstDropDownActiveSkills;
        private readonly IList<Tuple<string, IComparer<Skill>>>  _sortList;
        private readonly ObservableCollection<SkillControl2> _lstSkillControls = new ObservableCollection<SkillControl2>();
        private bool _blnActiveSkillSearchMode;
        private bool _blnKnowledgeSkillSearchMode;
        private readonly IList<Tuple<string, Predicate<KnowledgeSkill>>> _lstDropDownKnowledgeSkills;
        private readonly IList<Tuple<string, IComparer<KnowledgeSkill>>> _lstSortKnowledgeList;
        
        private void SkillsTabUserControl_Load(object sender, EventArgs e)
        {
            if (_objCharacter == null)
            {
                if (ParentForm != null)
                    ParentForm.Cursor = Cursors.WaitCursor;
                RealLoad();
                if (ParentForm != null)
                    ParentForm.Cursor = Cursors.Default;
            }
        }
        
        public void RealLoad()
        {
            if (ParentForm is CharacterShared frmParent)
                _objCharacter = frmParent.CharacterObject;
            else
            {
                _objCharacter = new Character();
                Utils.BreakIfDebug();
            }

            Stopwatch sw = Stopwatch.StartNew();  //Benchmark, should probably remove in release
            Stopwatch parts = Stopwatch.StartNew();
            //Keep everything visible until ready to display everything. This
            //seems to prevent redrawing everything each time anything is added
            //Not benched, but should be faster

            //Might also be useless horseshit, 2 lines

            //Visible = false;
            SuspendLayout();

            Stopwatch swDisplays = Stopwatch.StartNew();
            _lstSkillGroups = new BindingListDisplay<SkillGroup>(_objCharacter.SkillsSection.SkillGroups, group => new SkillGroupControl(group))
            {
                Location = new Point(0, 15),
            };
            _lstSkillGroups.Filter(x => x.SkillList.Any(y => _objCharacter.SkillsSection.SkillsDictionary.ContainsKey(y.Name)), true);
            _lstSkillGroups.Sort(new SkillGroupSorter(SkillsSection.CompareSkillGroups));
            int intNameLabelWidth = 0;
            int intRatingLabelWidth = 0;
            foreach (SkillGroupControl sg in _lstSkillGroups.Controls[0].Controls)
            {
                intNameLabelWidth = Math.Max(sg.NameWidth, intNameLabelWidth);
                intRatingLabelWidth = Math.Max(sg.RatingWidth, intRatingLabelWidth);
            }

            foreach (SkillGroupControl s in _lstSkillGroups.Controls[0].Controls)
            {
                s.MoveControls(intNameLabelWidth, intRatingLabelWidth);
            }
            lblGroupsSp.Left = _lstSkillGroups.Controls[0].Left + intNameLabelWidth + 6;
            lblGroupKarma.Left = lblGroupsSp.Left + intRatingLabelWidth + 6;

            swDisplays.TaskEnd("_lstSkillGroups");

            splitSkills.Panel1.Controls.Add(_lstSkillGroups);

            swDisplays.TaskEnd("_lstSkillGroups add");

            _lstActiveSkills = new BindingListDisplay<Skill>(_objCharacter.SkillsSection.Skills, MakeActiveSkill)
            {
                Location = new Point(265, 42),
            };
            intNameLabelWidth = 0;
            foreach (SkillControl2 objSkillControl in _lstSkillControls)
            {
                intNameLabelWidth = Math.Max(intNameLabelWidth, objSkillControl.NameWidth);
            }
            foreach (SkillControl2 objSkillControl in _lstSkillControls)
            {
                objSkillControl.MoveControls(intNameLabelWidth);
            }

            lblActiveSkills.Left = _lstActiveSkills.Left;
            if (!_objCharacter.Created)
            {
                intRatingLabelWidth = 0;
                foreach (SkillControl2 objSkillControl in _lstSkillControls)
                {
                    intRatingLabelWidth = Math.Max(intRatingLabelWidth, objSkillControl.NudSkillWidth);
                }
                lblActiveSp.Left = lblActiveSkills.Left + intNameLabelWidth + 6;
                lblActiveKarma.Left = lblActiveSp.Left + intRatingLabelWidth + 6;
                lblBuyWithKarma.Left = splitSkills.Panel1.Width - lblBuyWithKarma.Width;
            }

            swDisplays.TaskEnd("_lstActiveSkills");

            splitSkills.Panel1.Controls.Add(_lstActiveSkills);

            swDisplays.TaskEnd("_lstActiveSkills add");

            _lstKnowledgeSkills = new BindingListDisplay<KnowledgeSkill>(_objCharacter.SkillsSection.KnowledgeSkills,
                knoSkill => new KnowledgeSkillControl(knoSkill) {Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top})
            {
                Location = new Point(3, 50),
            };
            if (_objCharacter.SkillsSection.KnowledgeSkills.Count > 0)
            {
                intNameLabelWidth = 0;
                foreach (KnowledgeSkill objLoopSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                {
                    intNameLabelWidth = Math.Max(intNameLabelWidth, objLoopSkill.DisplayName.Length);
                }
                foreach (KnowledgeSkillControl objKnowledgeSkillControl in _lstKnowledgeSkills.Controls[0].Controls)
                {
                    objKnowledgeSkillControl.MoveControls(intNameLabelWidth);
                }
            }

            swDisplays.TaskEnd("_lstKnowledgeSkills");

            splitSkills.Panel2.Controls.Add(_lstKnowledgeSkills);

            swDisplays.TaskEnd("_lstKnowledgeSkills add");

            parts.TaskEnd("MakeSkillDisplay()");

            cboDisplayFilter.BeginUpdate();
            cboDisplayFilterKnowledge.BeginUpdate();
            cboSort.BeginUpdate();
            cboSortKnowledge.BeginUpdate();

            cboDisplayFilter.DataSource = _lstDropDownActiveSkills;
            cboDisplayFilter.ValueMember = "Item2";
            cboDisplayFilter.DisplayMember = "Item1";
            cboDisplayFilter.SelectedIndex = 1;
            cboDisplayFilter.MaxDropDownItems = _lstDropDownActiveSkills.Count;

            cboDisplayFilterKnowledge.DataSource = _lstDropDownKnowledgeSkills;
            cboDisplayFilterKnowledge.ValueMember = "Item2";
            cboDisplayFilterKnowledge.DisplayMember = "Item1";
            cboDisplayFilterKnowledge.SelectedIndex = 1;
            cboDisplayFilterKnowledge.MaxDropDownItems = _lstDropDownKnowledgeSkills.Count;
            parts.TaskEnd("_ddl databind");

            cboSort.DataSource = _sortList;
            cboSort.ValueMember = "Item2";
            cboSort.DisplayMember = "Item1";
            cboSort.SelectedIndex = 0;
            cboSort.MaxDropDownItems = _sortList.Count;

            cboSortKnowledge.DataSource = _lstSortKnowledgeList;
            cboSortKnowledge.ValueMember = "Item2";
            cboSortKnowledge.DisplayMember = "Item1";
            cboSortKnowledge.SelectedIndex = 0;
            cboSortKnowledge.MaxDropDownItems = _lstSortKnowledgeList.Count;

            cboDisplayFilter.EndUpdate();
            cboDisplayFilterKnowledge.EndUpdate();
            cboSort.EndUpdate();
            cboSortKnowledge.EndUpdate();

            parts.TaskEnd("_sort databind");

            _lstActiveSkills.ChildPropertyChanged += MakeDirtyWithCharacterUpdate;
            _lstSkillGroups.ChildPropertyChanged += MakeDirtyWithCharacterUpdate;
            _lstKnowledgeSkills.ChildPropertyChanged += MakeDirtyWithCharacterUpdate;

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

            if (!_objCharacter.Created)
            {
                lblKnoKarma.Visible = true;
                lblActiveKarma.Visible = true;
                lblGroupKarma.Visible = true;

                lblGroupsSp.Visible = _objCharacter.BuildMethodHasSkillPoints;
                lblGroupsSp.DataBindings.Add("Visible", _objCharacter, nameof(Character.BuildMethodHasSkillPoints), false, DataSourceUpdateMode.OnPropertyChanged);
                lblActiveSp.Visible = _objCharacter.BuildMethodHasSkillPoints;
                lblActiveSp.DataBindings.Add("Visible", _objCharacter, nameof(Character.BuildMethodHasSkillPoints), false, DataSourceUpdateMode.OnPropertyChanged);
                lblBuyWithKarma.Visible = _objCharacter.BuildMethodHasSkillPoints;
                lblBuyWithKarma.DataBindings.Add("Visible", _objCharacter, nameof(Character.BuildMethodHasSkillPoints), false, DataSourceUpdateMode.OnPropertyChanged);

                //Because visible is broken in databindings
                _objCharacter.SkillsSection.PropertyChanged += RefreshKnowledgePointsLabels;
                lblKnoSp.Visible = true;
                lblKnoSp.DataBindings.Add("Visible", _objCharacter.SkillsSection, nameof(SkillsSection.HasKnowledgePoints), false, DataSourceUpdateMode.OnPropertyChanged);
                lblKnoBwk.Visible = true;
                lblKnoBwk.DataBindings.Add("Visible", _objCharacter.SkillsSection, nameof(SkillsSection.HasKnowledgePoints), false, DataSourceUpdateMode.OnPropertyChanged);
                UpdateKnoSkillRemaining();
            }
            else
            {
                lblKnowledgeSkillPoints.Visible = false;
                lblKnowledgeSkillPointsTitle.Visible = false;
            }
            ResumeLayout(true);
            sw.Stop();
            Debug.WriteLine("RealLoad() in {0} ms", sw.Elapsed.TotalMilliseconds);
        }

        public void RefreshKnowledgePointsLabels(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SkillsSection.HasKnowledgePoints))
            {
                lblKnoSp.Visible = _objCharacter.SkillsSection.HasKnowledgePoints;
                lblKnoBwk.Visible = _objCharacter.SkillsSection.HasKnowledgePoints;
            }
        }

        public void UnbindSkillsTabUserControl()
        {
            if (_objCharacter != null)
            {
                _objCharacter.SkillsSection.PropertyChanged -= RefreshKnowledgePointsLabels;
            }
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
                    new SkillSorter((x, y) => string.Compare(x.DisplayAttribute, y.DisplayAttribute, StringComparison.Ordinal))),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortGroupName", GlobalOptions.Language),
                    new SkillSorter((x, y) => SkillsSection.CompareSkillGroups(x.SkillGroupObject, y.SkillGroupObject))),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortGroupRating", GlobalOptions.Language),
                    new SkillSortBySkillGroup()),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortCategory", GlobalOptions.Language),
                    new SkillSorter((x, y) => string.Compare(x.DisplayCategory(GlobalOptions.Language), y.DisplayCategory(GlobalOptions.Language), StringComparison.Ordinal))),
            };

            return ret;
        }

        private static IList<Tuple<string, Predicate<Skill>>> GenerateDropdownFilter()
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

            using (XmlNodeList xmlSkillCategoryList = XmlManager.Load("skills.xml").SelectNodes("/chummer/categories/category[@type = \"active\"]"))
                if (xmlSkillCategoryList != null)
                    foreach (XmlNode xmlCategoryNode in xmlSkillCategoryList)
                    {
                        string strName = xmlCategoryNode.InnerText;
                        ret.Add(new Tuple<string, Predicate<Skill>>(
                            $"{LanguageManager.GetString("Label_Category", GlobalOptions.Language)} {xmlCategoryNode.Attributes?["translate"]?.InnerText ?? strName}",
                            skill => skill.SkillCategory == strName));
                    }

            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                string strAttributeShort = LanguageManager.GetString($"String_Attribute{strAttribute}Short", GlobalOptions.Language, false);
                if (!string.IsNullOrEmpty(strAttributeShort))
                {
                    ret.Add(new Tuple<string, Predicate<Skill>>($"{LanguageManager.GetString("String_ExpenseAttribute", GlobalOptions.Language)}: {strAttributeShort}",
                        skill => skill.Attribute == strAttribute));
                }
            }

            using (XmlNodeList xmlSkillGroupList = XmlManager.Load("skills.xml").SelectNodes("/chummer/skillgroups/name"))
                if (xmlSkillGroupList != null)
                    foreach (XmlNode xmlSkillGroupNode in xmlSkillGroupList)
                    {
                        string strName = xmlSkillGroupNode.InnerText;
                        ret.Add(new Tuple<string, Predicate<Skill>>(
                            $"{LanguageManager.GetString("String_ExpenseSkillGroup", GlobalOptions.Language)} {xmlSkillGroupNode.Attributes?["translate"]?.InnerText ?? strName}",
                            skill => skill.SkillGroup == strName));
                    }

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
                    new KnowledgeSkillSorter((x, y) => string.Compare(x.DisplayAttribute, y.DisplayAttribute, StringComparison.Ordinal))),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortCategory", GlobalOptions.Language),
                    new KnowledgeSkillSorter((x, y) => string.Compare(x.DisplayCategory(GlobalOptions.Language), y.DisplayCategory(GlobalOptions.Language), StringComparison.Ordinal))),
            };

            return ret;
        }

        private static IList<Tuple<string, Predicate<KnowledgeSkill>>> GenerateKnowledgeDropdownFilter()
        {
            List<Tuple<string, Predicate<KnowledgeSkill>>> ret = new List<Tuple<string, Predicate<KnowledgeSkill>>>
            {
                //TODO: Search doesn't play nice with writeable name
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_Search", GlobalOptions.Language), null),
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_KnowledgeSkillFilterAll", GlobalOptions.Language), skill => true),
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_KnowledgeSkillFilterRatingAboveZero", GlobalOptions.Language),
                    skill => skill.Rating > 0),
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_KnowledgeSkillFilterTotalRatingAboveZero", GlobalOptions.Language),
                    skill => skill.Pool > 0),
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_KnowledgeSkillFilterRatingZero", GlobalOptions.Language),
                    skill => skill.Rating == 0)
            };
            //TODO: TRANSLATIONS
            using (XmlNodeList xmlSkillCategoryList = XmlManager.Load("skills.xml").SelectNodes("/chummer/categories/category[@type = \"knowledge\"]"))
                if (xmlSkillCategoryList != null)
                    foreach (XmlNode xmlCategoryNode in xmlSkillCategoryList)
                    {
                        string strName = xmlCategoryNode.InnerText;
                        ret.Add(new Tuple<string, Predicate<KnowledgeSkill>>(
                            $"{LanguageManager.GetString("Label_Category", GlobalOptions.Language)} {xmlCategoryNode.Attributes?["translate"]?.InnerText ?? strName}",
                            skill => skill.SkillCategory == strName));
                    }

            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                string strAttributeShort = LanguageManager.GetString($"String_Attribute{strAttribute}Short", GlobalOptions.Language, false);
                if (!string.IsNullOrEmpty(strAttributeShort))
                {
                    ret.Add(new Tuple<string, Predicate<KnowledgeSkill>>($"{LanguageManager.GetString("String_ExpenseAttribute", GlobalOptions.Language)}: {strAttributeShort}",
                        skill => skill.Attribute == strAttribute));
                }
            }

            return ret;
        }
        
        private Control MakeActiveSkill(Skill arg)
        {
            SkillControl2 objSkillControl = new SkillControl2(arg) {Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top};
            _lstSkillControls.Add(objSkillControl);
            objSkillControl.CustomAttributeChanged += Control_CustomAttributeChanged;
            return objSkillControl;
        }

        private void Control_CustomAttributeChanged(object sender, EventArgs e)
        {
            btnResetCustomDisplayAttribute.Visible = _lstSkillControls.Any(x => x.CustomAttributeSet);
        }

        private void Panel1_Resize(object sender, EventArgs e)
        {
            int intPanelHeight = splitSkills.Panel1.Height;
            int intSkillGroupsWidth = 255;
            if (_lstSkillGroups != null)
            {
                foreach (SkillGroupControl objSkillGroupControl in _lstSkillGroups.Controls[0].Controls)
                {
                    intSkillGroupsWidth = Math.Max(intSkillGroupsWidth, objSkillGroupControl.PreferredSize.Width);
                }
                _lstSkillGroups.Size = new Size(intSkillGroupsWidth, intPanelHeight - _lstSkillGroups.Top);
            }

            if (_lstActiveSkills != null)
            {
                _lstActiveSkills.Size = new Size(splitSkills.Panel1.Width - (intSkillGroupsWidth + 10), intPanelHeight - _lstActiveSkills.Top);
                if (_objCharacter.SkillsSection.Skills.Count > 0)
                {
                    int intNameLabelWidth = 0;
                    foreach (SkillControl2 objSkillControl in _lstSkillControls)
                    {
                        intNameLabelWidth = Math.Max(intNameLabelWidth, objSkillControl.NameWidth);
                    }

                    foreach (SkillControl2 objSkillControl in _lstSkillControls)
                    {
                        objSkillControl.MoveControls(intNameLabelWidth);
                    }

                    if (!_objCharacter.Created)
                    {
                        int intRatingLabelWidth = 0;
                        foreach (SkillControl2 objSkillControl in _lstSkillControls)
                        {
                            intRatingLabelWidth = Math.Max(intRatingLabelWidth, objSkillControl.NudSkillWidth);
                        }
                        lblActiveSp.Left = lblActiveSkills.Left + intNameLabelWidth + 6;
                        lblActiveKarma.Left = lblActiveSp.Left + intRatingLabelWidth + 6;
                        lblBuyWithKarma.Left = splitSkills.Panel1.Width - lblBuyWithKarma.Width;
                    }
                }
            }
        }

        private void Panel2_Resize(object sender, EventArgs e)
        {
            if (_lstKnowledgeSkills != null)
            {
                //_knoSkills.Height = splitSkills.Panel2.Height - 53;
                _lstKnowledgeSkills.Size = new Size(splitSkills.Panel2.Width - 6, splitSkills.Panel2.Height - 53);

                if (_objCharacter.SkillsSection.KnowledgeSkills.Count > 0)
                {
                    int intNameLabelWidth = 0;
                    foreach (KnowledgeSkill objLoopSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                    {
                        intNameLabelWidth = Math.Max(intNameLabelWidth, objLoopSkill.DisplayName.Length);
                    }

                    foreach (KnowledgeSkillControl objKnowledgeSkillControl in _lstKnowledgeSkills.Controls[0].Controls)
                    {
                        objKnowledgeSkillControl.MoveControls(intNameLabelWidth);
                    }
                }
            }
        }

        private void cboDisplayFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboDisplayFilter.SelectedItem is Tuple<string, Predicate<Skill>> selectedItem)
            {
                if (selectedItem.Item2 == null)
                {
                    cboDisplayFilter.DropDownStyle = ComboBoxStyle.DropDown;
                    _blnActiveSkillSearchMode = true;
                    cboDisplayFilter.Text = string.Empty;
                }
                else
                {
                    cboDisplayFilter.DropDownStyle = ComboBoxStyle.DropDownList;
                    _blnActiveSkillSearchMode = false;
                    _lstActiveSkills.Filter(selectedItem.Item2);
                }
            }
        }

        private void cboDisplayFilter_TextUpdate(object sender, EventArgs e)
        {
            if (_blnActiveSkillSearchMode)
            {
                _lstActiveSkills.Filter(skill => GlobalOptions.CultureInfo.CompareInfo.IndexOf(skill.DisplayNameMethod(GlobalOptions.Language), cboDisplayFilter.Text, CompareOptions.IgnoreCase) >= 0, true);
            }
        }

        private void cboSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSort.SelectedItem is Tuple<string, IComparer<Skill>> selectedItem)
                _lstActiveSkills.Sort(selectedItem.Item2);
        }

        private void btnExotic_Click(object sender, EventArgs e)
        {
            XmlDocument xmlSkillsDocument = XmlManager.Load("skills.xml");
            frmSelectExoticSkill frmPickExoticSkill = new frmSelectExoticSkill(_objCharacter);
            frmPickExoticSkill.ShowDialog(this);

            if (frmPickExoticSkill.DialogResult == DialogResult.Cancel)
                return;

            XmlNode xmlSkillNode = xmlSkillsDocument.SelectSingleNode("/chummer/skills/skill[name = \"" + frmPickExoticSkill.SelectedExoticSkill + "\"]");

            ExoticSkill objSkill = new ExoticSkill(_objCharacter, xmlSkillNode)
            {
                Specific = frmPickExoticSkill.SelectedExoticSkillSpecialisation
            };
            // Karma check needs to come after the skill is created to make sure bonus-based modifiers (e.g. JoAT) get applied properly (since they can potentially trigger off of the specific exotic skill target)
            if (_objCharacter.Created && objSkill.UpgradeKarmaCost > _objCharacter.Karma)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language));
                return;
            }
            objSkill.Upgrade();
            _objCharacter.SkillsSection.Skills.Add(objSkill);
            _objCharacter.SkillsSection.SkillsDictionary.Add(objSkill.Name + " (" + objSkill.DisplaySpecializationMethod(GlobalOptions.DefaultLanguage) + ')', objSkill);
        }
        
        private void btnKnowledge_Click(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
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
                    KnowledgeSkill skill = new KnowledgeSkill(_objCharacter)
                    {
                        WriteableName = form.SelectedItem
                    };

                    _objCharacter.SkillsSection.KnowledgeSkills.Add(skill);
                }
            }
            else
            {
                _objCharacter.SkillsSection.KnowledgeSkills.Add(new KnowledgeSkill(_objCharacter));
            }
        }

        private void btnResetCustomDisplayAttribute_Click(object sender, EventArgs e)
        {
            foreach (SkillControl2 control2 in _lstSkillControls.Where(x => x.CustomAttributeSet))
            {
                control2.ResetSelectAttribute();
            }
        }
        
        private void cboSortKnowledge_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSortKnowledge.SelectedItem is Tuple<string, IComparer<KnowledgeSkill>> selectedItem)
                _lstKnowledgeSkills.Sort(selectedItem.Item2);
        }

        private void cboDisplayFilterKnowledge_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboDisplayFilterKnowledge.SelectedItem is Tuple<string, Predicate<KnowledgeSkill>> selectedItem)
            {
                if (selectedItem.Item2 == null)
                {
                    cboDisplayFilterKnowledge.DropDownStyle = ComboBoxStyle.DropDown;
                    _blnKnowledgeSkillSearchMode = true;
                    cboDisplayFilterKnowledge.Text = string.Empty;
                }
                else
                {
                    cboDisplayFilterKnowledge.DropDownStyle = ComboBoxStyle.DropDownList;
                    _blnKnowledgeSkillSearchMode = false;
                    _lstKnowledgeSkills.Filter(selectedItem.Item2);
                }
            }
        }

        private void cboDisplayFilterKnowledge_TextUpdate(object sender, EventArgs e)
        {
            if (_blnKnowledgeSkillSearchMode)
            {
                _lstKnowledgeSkills.Filter(skill => GlobalOptions.CultureInfo.CompareInfo.IndexOf(skill.DisplayNameMethod(GlobalOptions.Language), cboDisplayFilterKnowledge.Text, CompareOptions.IgnoreCase) >= 0, true);
            }
        }
    }
}
