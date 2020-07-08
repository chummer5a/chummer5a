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
        public event PropertyChangedEventHandler MakeDirtyWithCharacterUpdate;

        private BindingListDisplay<Skill> _lstActiveSkills;
        private BindingListDisplay<SkillGroup> _lstSkillGroups;
        private BindingListDisplay<KnowledgeSkill> _lstKnowledgeSkills;

        public SkillsTabUserControl()
        {
            InitializeComponent();
            flpSkillGroupLabels.Margin = new Padding(
                flpSkillGroupLabels.Margin.Left,
                flpSkillGroupLabels.Margin.Top,
                flpSkillGroupLabels.Margin.Right + SystemInformation.VerticalScrollBarWidth,
                flpSkillGroupLabels.Margin.Bottom);

            this.TranslateWinForm();

            _lstDropDownActiveSkills = GenerateDropdownFilter();
            _lstDropDownKnowledgeSkills = GenerateKnowledgeDropdownFilter();
            _sortList = GenerateSortList();
            _lstSortKnowledgeList = GenerateKnowledgeSortList();
        }

        public void MissingDatabindingsWorkaround()
        {
            if (!_objCharacter.Created)
                UpdateKnoSkillRemaining();
        }

        private void UpdateKnoSkillRemaining()
        {
            lblKnowledgeSkillPoints.Text = _objCharacter.SkillsSection.KnowledgeSkillPointsRemain.ToString(GlobalOptions.CultureInfo)
                                           + LanguageManager.GetString("String_Of")
                                           + _objCharacter.SkillsSection.KnowledgeSkillPoints.ToString(GlobalOptions.CultureInfo);
        }

        private Character _objCharacter;
        private readonly IList<Tuple<string, Predicate<Skill>>> _lstDropDownActiveSkills;
        private readonly IList<Tuple<string, IComparer<Skill>>>  _sortList;
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

            _lstActiveSkills = new BindingListDisplay<Skill>(_objCharacter.SkillsSection.Skills, MakeActiveSkill)
            {
                Dock = DockStyle.Fill
            };
            Control MakeActiveSkill(Skill arg)
            {
                SkillControl2 objSkillControl = new SkillControl2(arg);
                objSkillControl.CustomAttributeChanged += Control_CustomAttributeChanged;
                return objSkillControl;
            }
            RefreshSkillLabels();

            swDisplays.TaskEnd("_lstActiveSkills");

            tlpActiveSkills.Controls.Add(_lstActiveSkills, 0, 2);
            tlpActiveSkills.SetColumnSpan(_lstActiveSkills, 3);

            swDisplays.TaskEnd("_lstActiveSkills add");

            _lstKnowledgeSkills = new BindingListDisplay<KnowledgeSkill>(_objCharacter.SkillsSection.KnowledgeSkills,
                knoSkill => new KnowledgeSkillControl(knoSkill))
            {
                Dock = DockStyle.Fill
            };
            RefreshKnowledgeSkillLabels();

            swDisplays.TaskEnd("_lstKnowledgeSkills");

            tlpBottomPanel.Controls.Add(_lstKnowledgeSkills, 0, 2);
            tlpBottomPanel.SetColumnSpan(_lstKnowledgeSkills, 3);

            swDisplays.TaskEnd("_lstKnowledgeSkills add");

            if (_objCharacter.SkillsSection.SkillGroups.Count > 0)
            {
                _lstSkillGroups = new BindingListDisplay<SkillGroup>(_objCharacter.SkillsSection.SkillGroups,
                    group => new SkillGroupControl(group))
                {
                    Dock = DockStyle.Fill
                };
                _lstSkillGroups.Filter(x => x.SkillList.Any(y => _objCharacter.SkillsSection.SkillsDictionary.ContainsKey(y.Name)), true);
                _lstSkillGroups.Sort(new SkillGroupSorter(SkillsSection.CompareSkillGroups));
                RefreshSkillGroupLabels();

                swDisplays.TaskEnd("_lstSkillGroups");

                tlpSkillGroups.Controls.Add(_lstSkillGroups, 0, 1);
                tlpSkillGroups.SetColumnSpan(_lstSkillGroups, 2);

                swDisplays.TaskEnd("_lstSkillGroups add");
            }
            else
            {
                tlpSkillGroups.Visible = false;
                tlpActiveSkills.Margin = new Padding(0);
                tlpTopPanel.ColumnStyles[0] = new ColumnStyle(SizeType.AutoSize);
                tlpTopPanel.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 100F);
            }

            parts.TaskEnd("MakeSkillDisplay()");

            cboDisplayFilter.BeginUpdate();
            cboDisplayFilter.DataSource = null;
            cboDisplayFilter.ValueMember = "Item2";
            cboDisplayFilter.DisplayMember = "Item1";
            cboDisplayFilter.DataSource = _lstDropDownActiveSkills;
            cboDisplayFilter.SelectedIndex = 1;
            cboDisplayFilter.MaxDropDownItems = _lstDropDownActiveSkills.Count;
            cboDisplayFilter.EndUpdate();

            cboDisplayFilterKnowledge.BeginUpdate();
            cboDisplayFilterKnowledge.DataSource = null;
            cboDisplayFilterKnowledge.ValueMember = "Item2";
            cboDisplayFilterKnowledge.DisplayMember = "Item1";
            cboDisplayFilterKnowledge.DataSource = _lstDropDownKnowledgeSkills;
            cboDisplayFilterKnowledge.SelectedIndex = 1;
            cboDisplayFilterKnowledge.MaxDropDownItems = _lstDropDownKnowledgeSkills.Count;
            cboDisplayFilterKnowledge.EndUpdate();
            parts.TaskEnd("_ddl databind");

            cboSort.BeginUpdate();
            cboSort.DataSource = null;
            cboSort.ValueMember = "Item2";
            cboSort.DisplayMember = "Item1";
            cboSort.DataSource = _sortList;
            cboSort.SelectedIndex = 0;
            cboSort.MaxDropDownItems = _sortList.Count;
            cboSort.EndUpdate();

            cboSortKnowledge.BeginUpdate();
            cboSortKnowledge.DataSource = null;
            cboSortKnowledge.ValueMember = "Item2";
            cboSortKnowledge.DisplayMember = "Item1";
            cboSortKnowledge.DataSource = _lstSortKnowledgeList;
            cboSortKnowledge.SelectedIndex = 0;
            cboSortKnowledge.MaxDropDownItems = _lstSortKnowledgeList.Count;
            cboSortKnowledge.EndUpdate();

            parts.TaskEnd("_sort databind");

            if (_lstSkillGroups != null)
                _lstSkillGroups.ChildPropertyChanged += MakeDirtyWithCharacterUpdate;
            _lstActiveSkills.ChildPropertyChanged += MakeDirtyWithCharacterUpdate;
            _lstKnowledgeSkills.ChildPropertyChanged += MakeDirtyWithCharacterUpdate;

            if (!_objCharacter.Created)
            {
                lblGroupsSp.DoOneWayDataBinding("Visible", _objCharacter, nameof(Character.BuildMethodHasSkillPoints));
                lblActiveSp.DoOneWayDataBinding("Visible", _objCharacter, nameof(Character.BuildMethodHasSkillPoints));
                lblBuyWithKarma.DoOneWayDataBinding("Visible", _objCharacter, nameof(Character.BuildMethodHasSkillPoints));

                lblKnoSp.DoOneWayDataBinding("Visible", _objCharacter.SkillsSection, nameof(SkillsSection.HasKnowledgePoints));
                lblKnoBwk.DoOneWayDataBinding("Visible", _objCharacter.SkillsSection, nameof(SkillsSection.HasKnowledgePoints));
                UpdateKnoSkillRemaining();
            }
            else
            {
                flpSkillGroupLabels.Visible = false;

                lblActiveSp.Visible = false;
                lblActiveKarma.Visible = false;
                lblBuyWithKarma.Visible = false;

                flpKnowledgeSkillsLabels.Visible = false;
                lblKnoBwk.Visible = false;
                lblKnowledgeSkillPoints.Visible = false;
                lblKnowledgeSkillPointsTitle.Visible = false;
            }

            btnExotic.Visible = XmlManager.Load("skills.xml").SelectSingleNode("/chummer/skills/skill[exotic = \"True\"]") != null;

            _objCharacter.SkillsSection.Skills.ListChanged += SkillsOnListChanged;
            _objCharacter.SkillsSection.SkillGroups.ListChanged += SkillGroupsOnListChanged;
            _objCharacter.SkillsSection.KnowledgeSkills.ListChanged += KnowledgeSkillsOnListChanged;
            ResumeLayout(true);
            sw.Stop();
            Debug.WriteLine("RealLoad() in {0} ms", sw.Elapsed.TotalMilliseconds);
        }

        private void RefreshSkillGroupLabels()
        {
            if (_lstSkillGroups != null)
            {
                int intNameLabelWidth = lblSkillGroups.PreferredWidth;
                foreach (SkillGroupControl sg in _lstSkillGroups.DisplayPanel.Controls)
                {
                    intNameLabelWidth = Math.Max(sg.NameWidth, intNameLabelWidth);
                }
                lblSkillGroups.MinimumSize = new Size(intNameLabelWidth, lblSkillGroups.MinimumSize.Height);
                foreach (SkillGroupControl s in _lstSkillGroups.DisplayPanel.Controls)
                {
                    s.MoveControls(intNameLabelWidth);
                }
            }
        }

        private void RefreshSkillLabels()
        {
            if (_lstActiveSkills != null)
            {
                int intNameLabelWidth = lblActiveSkills.PreferredWidth;
                int intRatingLabelWidth = lblActiveSp.PreferredWidth;
                foreach (SkillControl2 objSkillControl in _lstActiveSkills.DisplayPanel.Controls)
                {
                    intNameLabelWidth = Math.Max(intNameLabelWidth, objSkillControl.NameWidth);
                    intRatingLabelWidth = Math.Max(intRatingLabelWidth, objSkillControl.NudSkillWidth);
                }
                lblActiveSkills.MinimumSize = new Size(intNameLabelWidth - lblActiveSkills.Margin.Right, lblActiveSkills.MinimumSize.Height);
                lblActiveKarma.Margin = new Padding(
                    Math.Max(0, lblActiveSp.Margin.Left + intRatingLabelWidth - lblActiveSp.Width),
                    lblActiveKarma.Margin.Top,
                    lblActiveKarma.Margin.Right,
                    lblActiveKarma.Margin.Bottom);
                foreach (SkillControl2 objSkillControl in _lstActiveSkills.DisplayPanel.Controls)
                {
                    objSkillControl.MoveControls(intNameLabelWidth);
                }
            }
        }

        private void RefreshKnowledgeSkillLabels()
        {
            if (_lstKnowledgeSkills != null)
            {
                int intNameLabelWidth = 0;
                int intRatingLabelWidth = lblKnoSp.PreferredWidth;
                int intRightButtonsWidth = 0;
                foreach (KnowledgeSkillControl objKnowledgeSkillControl in _lstKnowledgeSkills.DisplayPanel.Controls)
                {
                    intNameLabelWidth = Math.Max(intNameLabelWidth, objKnowledgeSkillControl.NameWidth);
                    intRatingLabelWidth = Math.Max(intRatingLabelWidth, objKnowledgeSkillControl.NudSkillWidth);
                    intRightButtonsWidth = Math.Max(intRightButtonsWidth, objKnowledgeSkillControl.RightButtonsWidth);
                }
                lblKnowledgeSkills.MinimumSize = new Size(intNameLabelWidth, lblKnowledgeSkills.MinimumSize.Height);
                lblKnoKarma.Margin = new Padding(
                    Math.Max(0, lblKnoSp.Margin.Left + intRatingLabelWidth - lblKnoSp.Width),
                    lblKnoKarma.Margin.Top,
                    lblKnoKarma.Margin.Right,
                    lblKnoKarma.Margin.Bottom);
                lblKnoBwk.Margin = new Padding(
                    lblKnoBwk.Margin.Left,
                    lblKnoBwk.Margin.Top,
                    Math.Max(0, lblKnoBwk.Margin.Left + intRightButtonsWidth + SystemInformation.VerticalScrollBarWidth - lblKnoBwk.PreferredWidth / 2),
                    lblKnoBwk.Margin.Bottom);
            }
        }

        private void SkillGroupsOnListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.Reset
                || e.ListChangedType == ListChangedType.ItemAdded
                || e.ListChangedType == ListChangedType.ItemDeleted)
                RefreshSkillGroupLabels();
        }

        private void SkillsOnListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.Reset
                || e.ListChangedType == ListChangedType.ItemAdded
                || e.ListChangedType == ListChangedType.ItemDeleted)
                RefreshSkillLabels();
        }

        private void KnowledgeSkillsOnListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.Reset
                || e.ListChangedType == ListChangedType.ItemAdded
                || e.ListChangedType == ListChangedType.ItemDeleted)
                RefreshKnowledgeSkillLabels();
        }

        private void UnbindSkillsTabUserControl()
        {
            if (_objCharacter != null)
            {
                _objCharacter.SkillsSection.Skills.ListChanged -= SkillsOnListChanged;
                _objCharacter.SkillsSection.SkillGroups.ListChanged -= SkillGroupsOnListChanged;
                _objCharacter.SkillsSection.KnowledgeSkills.ListChanged -= KnowledgeSkillsOnListChanged;
            }
        }

        private static IList<Tuple<string, IComparer<Skill>>> GenerateSortList()
        {
            List<Tuple<string, IComparer<Skill>>> ret = new List<Tuple<string, IComparer<Skill>>>()
            {
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortAlphabetical"),
                    new SkillSorter(SkillsSection.CompareSkills)),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortRating"),
                    new SkillSorter((x, y) =>
                    {
                        int intReturn = y.Rating.CompareTo(x.Rating);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortDicepool"),
                    new SkillSorter((x, y) =>
                    {
                        int intReturn = y.Pool.CompareTo(x.Pool);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortLowerDicepool"),
                    new SkillSorter((x, y) =>
                    {
                        int intReturn = x.Pool.CompareTo(y.Pool);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortAttributeValue"),
                    new SkillSorter((x, y) =>
                    {
                        int intReturn = y.AttributeModifiers.CompareTo(x.AttributeModifiers);
                        if (intReturn == 0)
                        {
                            intReturn = string.Compare(x.DisplayAttribute, y.DisplayAttribute, false, GlobalOptions.CultureInfo);
                            if (intReturn == 0)
                                intReturn = SkillsSection.CompareSkills(x, y);
                        }
                        return intReturn;
                    })),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortAttributeName"),
                    new SkillSorter((x, y) =>
                    {
                        int intReturn = string.Compare(x.DisplayAttribute, y.DisplayAttribute, false, GlobalOptions.CultureInfo);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortGroupName"),
                    new SkillSorter((x, y) =>
                    {
                        int intReturn = SkillsSection.CompareSkillGroups(x.SkillGroupObject, y.SkillGroupObject);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortGroupRating"),
                    new SkillSortBySkillGroup()),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortCategory"),
                    new SkillSorter((x, y) =>
                    {
                        int intReturn = string.Compare(x.DisplayCategory(GlobalOptions.Language), y.DisplayCategory(GlobalOptions.Language), false, GlobalOptions.CultureInfo);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
            };

            return ret;
        }

        private static IList<Tuple<string, Predicate<Skill>>> GenerateDropdownFilter()
        {
            List<Tuple<string, Predicate<Skill>>> ret = new List<Tuple<string, Predicate<Skill>>>
            {
                new Tuple<string, Predicate<Skill>>(LanguageManager.GetString("String_Search"),
                    null),
                new Tuple<string, Predicate<Skill>>(LanguageManager.GetString("String_SkillFilterAll"),
                    skill => true),
                new Tuple<string, Predicate<Skill>>(LanguageManager.GetString("String_SkillFilterRatingAboveZero"),
                    skill => skill.Rating > 0),
                new Tuple<string, Predicate<Skill>>(LanguageManager.GetString("String_SkillFilterTotalRatingAboveZero"),
                    skill => skill.Pool > 0),
                new Tuple<string, Predicate<Skill>>(LanguageManager.GetString("String_SkillFilterRatingZero"),
                    skill => skill.Rating == 0),
                new Tuple<string, Predicate<Skill>>(LanguageManager.GetString("String_SkillFilterNoSkillGroup"),
                    skill => skill.SkillGroup.Length == 0),
                new Tuple<string, Predicate<Skill>>(LanguageManager.GetString("String_SkillFilterBrokenSkillGroup"),
                    skill => skill.Pool > 0 && (skill.SkillGroup.Length == 0 || (skill.SkillGroupObject != null && skill.Rating > skill.SkillGroupObject.Rating)))
            };
            //TODO: TRANSLATIONS

            string strSpace = LanguageManager.GetString("String_Space");
            string strColon = LanguageManager.GetString("String_Colon");

            using (XmlNodeList xmlSkillCategoryList = XmlManager.Load("skills.xml").SelectNodes("/chummer/categories/category[@type = \"active\"]"))
            {
                if (xmlSkillCategoryList != null)
                {
                    string strCategory = LanguageManager.GetString("Label_Category");
                    foreach (XmlNode xmlCategoryNode in xmlSkillCategoryList)
                    {
                        string strName = xmlCategoryNode.InnerText;
                        if (!string.IsNullOrEmpty(strName))
                            ret.Add(new Tuple<string, Predicate<Skill>>(
                                strCategory + strSpace + (xmlCategoryNode.Attributes?["translate"]?.InnerText ?? strName),
                                skill => skill.SkillCategory == strName));
                    }
                }
            }

            string strAttributeLabel = LanguageManager.GetString("String_ExpenseAttribute");
            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                string strAttributeShort = LanguageManager.GetString("String_Attribute" + strAttribute + "Short", GlobalOptions.Language, false);
                if (!string.IsNullOrEmpty(strAttributeShort))
                    ret.Add(new Tuple<string, Predicate<Skill>>(strAttributeLabel + strColon + strSpace + strAttributeShort,
                        skill => skill.Attribute == strAttribute));
            }

            using (XmlNodeList xmlSkillGroupList = XmlManager.Load("skills.xml").SelectNodes("/chummer/skillgroups/name"))
            {
                if (xmlSkillGroupList != null)
                {
                    string strSkillGroupLabel = LanguageManager.GetString("String_ExpenseSkillGroup");
                    foreach (XmlNode xmlSkillGroupNode in xmlSkillGroupList)
                    {
                        string strName = xmlSkillGroupNode.InnerText;
                        if (!string.IsNullOrEmpty(strName))
                            ret.Add(new Tuple<string, Predicate<Skill>>(
                                strSkillGroupLabel + strSpace + (xmlSkillGroupNode.Attributes?["translate"]?.InnerText ?? strName),
                                skill => skill.SkillGroup == strName));
                    }
                }
            }

            return ret;
        }

        private static IList<Tuple<string, IComparer<KnowledgeSkill>>> GenerateKnowledgeSortList()
        {
            List<Tuple<string, IComparer<KnowledgeSkill>>> ret = new List<Tuple<string, IComparer<KnowledgeSkill>>>()
            {
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortAlphabetical"),
                    new KnowledgeSkillSorter(SkillsSection.CompareSkills)),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortRating"),
                    new KnowledgeSkillSorter((x, y) =>
                    {
                        int intReturn = y.Rating.CompareTo(x.Rating);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortDicepool"),
                    new KnowledgeSkillSorter((x, y) =>
                    {
                        int intReturn = y.Pool.CompareTo(x.Pool);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortLowerDicepool"),
                    new KnowledgeSkillSorter((x, y) =>
                    {
                        int intReturn = x.Pool.CompareTo(y.Pool);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortAttributeValue"),
                    new KnowledgeSkillSorter((x, y) =>
                    {
                        int intReturn = y.AttributeModifiers.CompareTo(x.AttributeModifiers);
                        if (intReturn == 0)
                        {
                            intReturn = string.Compare(x.DisplayAttribute, y.DisplayAttribute, false, GlobalOptions.CultureInfo);
                            if (intReturn == 0)
                                intReturn = SkillsSection.CompareSkills(x, y);
                        }
                        return intReturn;
                    })),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortAttributeName"),
                    new KnowledgeSkillSorter((x, y) =>
                    {
                        int intReturn = string.Compare(x.DisplayAttribute, y.DisplayAttribute, false, GlobalOptions.CultureInfo);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortCategory"),
                    new KnowledgeSkillSorter((x, y) =>
                    {
                        int intReturn = string.Compare(x.DisplayCategory(GlobalOptions.Language), y.DisplayCategory(GlobalOptions.Language), false, GlobalOptions.CultureInfo);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
            };

            return ret;
        }

        private static IList<Tuple<string, Predicate<KnowledgeSkill>>> GenerateKnowledgeDropdownFilter()
        {
            List<Tuple<string, Predicate<KnowledgeSkill>>> ret = new List<Tuple<string, Predicate<KnowledgeSkill>>>
            {
                //TODO: Search doesn't play nice with writable name
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_Search"),
                    null),
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_KnowledgeSkillFilterAll"),
                    skill => true),
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_KnowledgeSkillFilterRatingAboveZero"),
                    skill => skill.Rating > 0),
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_KnowledgeSkillFilterTotalRatingAboveZero"),
                    skill => skill.Pool > 0),
                new Tuple<string, Predicate<KnowledgeSkill>>(LanguageManager.GetString("String_KnowledgeSkillFilterRatingZero"),
                    skill => skill.Rating == 0)
            };
            //TODO: TRANSLATIONS

            string strSpace = LanguageManager.GetString("String_Space");
            string strColon = LanguageManager.GetString("String_Colon");

            using (XmlNodeList xmlSkillCategoryList = XmlManager.Load("skills.xml").SelectNodes("/chummer/categories/category[@type = \"knowledge\"]"))
            {
                if (xmlSkillCategoryList != null)
                {
                    string strCategory = LanguageManager.GetString("Label_Category");
                    foreach (XmlNode xmlCategoryNode in xmlSkillCategoryList)
                    {
                        string strName = xmlCategoryNode.InnerText;
                        if (!string.IsNullOrEmpty(strName))
                            ret.Add(new Tuple<string, Predicate<KnowledgeSkill>>(
                                strCategory + strSpace + (xmlCategoryNode.Attributes?["translate"]?.InnerText ?? strName),
                                skill => skill.SkillCategory == strName));
                    }
                }
            }

            string strAttributeLabel = LanguageManager.GetString("String_ExpenseAttribute");
            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                string strAttributeShort = LanguageManager.GetString("String_Attribute" + strAttribute + "Short", GlobalOptions.Language, false);
                if (!string.IsNullOrEmpty(strAttributeShort))
                    ret.Add(new Tuple<string, Predicate<KnowledgeSkill>>(strAttributeLabel + strColon + strSpace + strAttributeShort,
                        skill => skill.Attribute == strAttribute));
            }

            return ret;
        }

        private void Control_CustomAttributeChanged(object sender, EventArgs e)
        {
            bool blnVisible = false;
            foreach (SkillControl2 objSkillControl in _lstActiveSkills.DisplayPanel.Controls)
            {
                if (objSkillControl.CustomAttributeSet)
                {
                    blnVisible = true;
                    break;
                }
            }
            btnResetCustomDisplayAttribute.Visible = blnVisible;
        }

        private void Panel1_Resize(object sender, EventArgs e)
        {
            RefreshSkillGroupLabels();
            RefreshSkillLabels();
        }

        private void Panel2_Resize(object sender, EventArgs e)
        {
            RefreshKnowledgeSkillLabels();
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
                    _lstActiveSkills.SuspendLayout();
                    _lstActiveSkills.Filter(selectedItem.Item2);
                    _lstActiveSkills.ResumeLayout();
                }
            }
        }

        private void cboDisplayFilter_TextUpdate(object sender, EventArgs e)
        {
            if (_blnActiveSkillSearchMode)
            {
                _lstActiveSkills.SuspendLayout();
                _lstActiveSkills.Filter(skill => GlobalOptions.CultureInfo.CompareInfo.IndexOf(skill.CurrentDisplayName, cboDisplayFilter.Text, CompareOptions.IgnoreCase) >= 0, true);
                _lstActiveSkills.ResumeLayout();
            }
        }

        private void cboSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSort.SelectedItem is Tuple<string, IComparer<Skill>> selectedItem)
            {
                _lstActiveSkills.SuspendLayout();
                _lstActiveSkills.Sort(selectedItem.Item2);
                _lstActiveSkills.ResumeLayout();
            }
        }

        private void btnExotic_Click(object sender, EventArgs e)
        {
            ExoticSkill objSkill;
            XmlDocument xmlSkillsDocument = XmlManager.Load("skills.xml");
            using (frmSelectExoticSkill frmPickExoticSkill = new frmSelectExoticSkill(_objCharacter))
            {
                frmPickExoticSkill.ShowDialog(this);

                if (frmPickExoticSkill.DialogResult == DialogResult.Cancel)
                    return;

                XmlNode xmlSkillNode = xmlSkillsDocument.SelectSingleNode("/chummer/skills/skill[name = \"" + frmPickExoticSkill.SelectedExoticSkill + "\"]");

                objSkill = new ExoticSkill(_objCharacter, xmlSkillNode)
                {
                    Specific = frmPickExoticSkill.SelectedExoticSkillSpecialisation
                };
            }

            // Karma check needs to come after the skill is created to make sure bonus-based modifiers (e.g. JoAT) get applied properly (since they can potentially trigger off of the specific exotic skill target)
            if (_objCharacter.Created && objSkill.UpgradeKarmaCost > _objCharacter.Karma)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughKarma"));
                return;
            }
            objSkill.Upgrade();
            _objCharacter.SkillsSection.Skills.Add(objSkill);
            string key = objSkill.DictionaryKey;
            if (!_objCharacter.SkillsSection.SkillsDictionary.ContainsKey(key))
                _objCharacter.SkillsSection.SkillsDictionary.Add(key, objSkill);
        }

        private void btnKnowledge_Click(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                using (frmSelectItem form = new frmSelectItem
                {
                    Description = LanguageManager.GetString("Label_Options_NewKnowledgeSkill")
                })
                {
                    form.SetDropdownItemsMode(KnowledgeSkill.DefaultKnowledgeSkills);

                    if (form.ShowDialog(Program.MainForm) != DialogResult.OK)
                        return;
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
            _lstActiveSkills.SuspendLayout();
            foreach (SkillControl2 objSkillControl in _lstActiveSkills.DisplayPanel.Controls)
            {
                if (objSkillControl.CustomAttributeSet)
                    objSkillControl.ResetSelectAttribute(sender, e);
            }
            _lstActiveSkills.ResumeLayout();
        }

        private void cboSortKnowledge_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSortKnowledge.SelectedItem is Tuple<string, IComparer<KnowledgeSkill>> selectedItem)
            {
                _lstKnowledgeSkills.SuspendLayout();
                _lstKnowledgeSkills.Sort(selectedItem.Item2);
                _lstKnowledgeSkills.ResumeLayout();
            }
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
                    _lstKnowledgeSkills.SuspendLayout();
                    _lstKnowledgeSkills.Filter(selectedItem.Item2);
                    _lstKnowledgeSkills.ResumeLayout();
                }
            }
        }

        private void cboDisplayFilterKnowledge_TextUpdate(object sender, EventArgs e)
        {
            if (_blnKnowledgeSkillSearchMode)
            {
                _lstKnowledgeSkills.SuspendLayout();
                _lstKnowledgeSkills.Filter(skill => GlobalOptions.CultureInfo.CompareInfo.IndexOf(skill.CurrentDisplayName, cboDisplayFilterKnowledge.Text, CompareOptions.IgnoreCase) >= 0, true);
                _lstKnowledgeSkills.ResumeLayout();
            }
        }
    }
}
