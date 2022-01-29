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
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;
using Chummer.UI.Shared;

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
            lblGroupKarma.Margin = new Padding(
                lblGroupKarma.Margin.Left,
                lblGroupKarma.Margin.Top,
                lblGroupKarma.Margin.Right + SystemInformation.VerticalScrollBarWidth,
                lblGroupKarma.Margin.Bottom);
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            _sortList = GenerateSortList();
            _lstSortKnowledgeList = GenerateKnowledgeSortList();
        }

        private void UpdateKnoSkillRemaining()
        {
            string strText =
                _objCharacter.SkillsSection.KnowledgeSkillPointsRemain.ToString(GlobalSettings.CultureInfo) +
                LanguageManager.GetString("String_Of") +
                _objCharacter.SkillsSection.KnowledgeSkillPoints.ToString(GlobalSettings.CultureInfo);
            int intSkillPointsSpentOnKnoSkills = _objCharacter.SkillsSection.SkillPointsSpentOnKnoskills;
            if (intSkillPointsSpentOnKnoSkills != 0)
                strText += string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_PlusSkillPointsSpent"), intSkillPointsSpentOnKnoSkills);
            lblKnowledgeSkillPoints.Text = strText;
        }

        private Character _objCharacter;
        private List<Tuple<string, Predicate<Skill>>> _lstDropDownActiveSkills;
        private readonly List<Tuple<string, IComparer<Skill>>> _sortList;
        private bool _blnActiveSkillSearchMode;
        private bool _blnKnowledgeSkillSearchMode;
        private List<Tuple<string, Predicate<KnowledgeSkill>>> _lstDropDownKnowledgeSkills;
        private readonly List<Tuple<string, IComparer<KnowledgeSkill>>> _lstSortKnowledgeList;

        private void SkillsTabUserControl_Load(object sender, EventArgs e)
        {
            if (_objCharacter != null)
                return;
            using (new CursorWait(this))
                RealLoad();
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

            if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                return;

            _lstDropDownActiveSkills = GenerateDropdownFilter(_objCharacter);
            _lstDropDownKnowledgeSkills = GenerateKnowledgeDropdownFilter(_objCharacter);

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
                SkillControl objSkillControl = new SkillControl(arg);
                objSkillControl.CustomAttributeChanged += Control_CustomAttributeChanged;
                return objSkillControl;
            }
            RefreshSkillLabels();

            swDisplays.TaskEnd("_lstActiveSkills");

            tlpActiveSkills.Controls.Add(_lstActiveSkills, 0, 2);
            tlpActiveSkills.SetColumnSpan(_lstActiveSkills, 5);

            swDisplays.TaskEnd("_lstActiveSkills add");

            _lstKnowledgeSkills = new BindingListDisplay<KnowledgeSkill>(_objCharacter.SkillsSection.KnowledgeSkills,
                knoSkill => new KnowledgeSkillControl(knoSkill))
            {
                Dock = DockStyle.Fill
            };
            RefreshKnowledgeSkillLabels();

            swDisplays.TaskEnd("_lstKnowledgeSkills");

            tlpBottomPanel.Controls.Add(_lstKnowledgeSkills, 0, 2);
            tlpBottomPanel.SetColumnSpan(_lstKnowledgeSkills, 4);

            swDisplays.TaskEnd("_lstKnowledgeSkills add");

            if (_objCharacter.SkillsSection.SkillGroups.Count > 0)
            {
                _lstSkillGroups = new BindingListDisplay<SkillGroup>(_objCharacter.SkillsSection.SkillGroups,
                    group => new SkillGroupControl(group))
                {
                    Dock = DockStyle.Fill
                };
                _lstSkillGroups.Filter(x => x.SkillList.Any(y => _objCharacter.SkillsSection.HasActiveSkill(y.DictionaryKey)), true);
                _lstSkillGroups.Sort(new SkillGroupSorter(SkillsSection.CompareSkillGroups));
                RefreshSkillGroupLabels();

                swDisplays.TaskEnd("_lstSkillGroups");

                tlpSkillGroups.Controls.Add(_lstSkillGroups, 0, 1);
                tlpSkillGroups.SetColumnSpan(_lstSkillGroups, 3);

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
                lblGroupsSp.DoOneWayDataBinding("Visible", _objCharacter, nameof(Character.EffectiveBuildMethodUsesPriorityTables));
                lblActiveSp.DoOneWayDataBinding("Visible", _objCharacter, nameof(Character.EffectiveBuildMethodUsesPriorityTables));
                lblBuyWithKarma.DoOneWayDataBinding("Visible", _objCharacter, nameof(Character.EffectiveBuildMethodUsesPriorityTables));

                lblKnoSp.DoOneWayDataBinding("Visible", _objCharacter.SkillsSection, nameof(SkillsSection.HasKnowledgePoints));
                lblKnoBwk.DoOneWayDataBinding("Visible", _objCharacter.SkillsSection, nameof(SkillsSection.HasKnowledgePoints));
                UpdateKnoSkillRemaining();
            }
            else
            {
                lblGroupsSp.Visible = false;
                lblGroupKarma.Visible = false;
                lblActiveSp.Visible = false;
                lblActiveKarma.Visible = false;
                lblBuyWithKarma.Visible = false;
                lblKnoSp.Visible = false;
                lblKnoKarma.Visible = false;
                lblKnoBwk.Visible = false;
                lblKnowledgeSkillPoints.Visible = false;
                lblKnowledgeSkillPointsTitle.Visible = false;
            }

            btnExotic.Visible = _objCharacter.LoadDataXPath("skills.xml").SelectSingleNode("/chummer/skills/skill[exotic = " + bool.TrueString.CleanXPath() + ']') != null;

            _objCharacter.SkillsSection.Skills.ListChanged += SkillsOnListChanged;
            _objCharacter.SkillsSection.SkillGroups.ListChanged += SkillGroupsOnListChanged;
            _objCharacter.SkillsSection.KnowledgeSkills.ListChanged += KnowledgeSkillsOnListChanged;
            _objCharacter.SkillsSection.PropertyChanged += SkillsSectionOnPropertyChanged;
            ResumeLayout(true);
            sw.Stop();
            Debug.WriteLine("RealLoad() in {0} ms", sw.Elapsed.TotalMilliseconds);
        }

        private void SkillsSectionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(SkillsSection.KnowledgeSkillPointsRemain)
                 || e.PropertyName == nameof(SkillsSection.KnowledgeSkillPoints)
                 || e.PropertyName == nameof(SkillsSection.SkillPointsSpentOnKnoskills))
                && _objCharacter?.Created == false)
            {
                UpdateKnoSkillRemaining();
            }
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
                foreach (SkillControl objSkillControl in _lstActiveSkills.DisplayPanel.Controls)
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
                foreach (SkillControl objSkillControl in _lstActiveSkills.DisplayPanel.Controls)
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
                _objCharacter.SkillsSection.PropertyChanged -= SkillsSectionOnPropertyChanged;
            }
        }

        private static List<Tuple<string, IComparer<Skill>>> GenerateSortList()
        {
            List<Tuple<string, IComparer<Skill>>> ret = new List<Tuple<string, IComparer<Skill>>>(9)
            {
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortAlphabetical"),
                    new SkillSorter(SkillsSection.CompareSkills)),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortRating"),
                    new SkillSorter((x, y) =>
                    {
                        int intReturn = y.Rating.CompareTo(x.Rating);
                        if (intReturn == 0)
                        {
                            if (y.Specializations.Count > 0 || x.Specializations.Count > 0)
                            {
                                if (x.Specializations.Count == 0)
                                    return 1;
                                if (y.Specializations.Count == 0)
                                    return -1;
                                if (y.Specializations.Count > x.Specializations.Count)
                                    return 1;
                                if (y.Specializations.Count < x.Specializations.Count)
                                    return -1;
                            }
                            intReturn = SkillsSection.CompareSkills(x, y);
                        }
                        return intReturn;
                    })),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortDicepool"),
                    new SkillSorter((x, y) =>
                    {
                        int intReturn = y.Pool.CompareTo(x.Pool);
                        if (intReturn == 0)
                        {
                            if (y.Specializations.Count > 0 || x.Specializations.Count > 0)
                            {
                                if (x.Specializations.Count == 0)
                                    return 1;
                                if (y.Specializations.Count == 0)
                                    return -1;
                                int intLeftMax = x.Specializations.Max(z => z.SpecializationBonus);
                                int intRightMax = y.Specializations.Max(z => z.SpecializationBonus);
                                if (intRightMax > intLeftMax)
                                    return 1;
                                if (intRightMax < intLeftMax)
                                    return -1;
                                if (y.Specializations.Count > x.Specializations.Count)
                                    return 1;
                                if (y.Specializations.Count < x.Specializations.Count)
                                    return -1;
                            }
                            intReturn = SkillsSection.CompareSkills(x, y);
                        }

                        return intReturn;
                    })),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortLowerDicepool"),
                    new SkillSorter((x, y) =>
                    {
                        int intReturn = x.Pool.CompareTo(y.Pool);
                        if (intReturn == 0)
                        {
                            if (y.Specializations.Count > 0 || x.Specializations.Count > 0)
                            {
                                if (x.Specializations.Count == 0)
                                    return -1;
                                if (y.Specializations.Count == 0)
                                    return 1;
                                int intLeftMax = x.Specializations.Max(z => z.SpecializationBonus);
                                int intRightMax = y.Specializations.Max(z => z.SpecializationBonus);
                                if (intRightMax > intLeftMax)
                                    return -1;
                                if (intRightMax < intLeftMax)
                                    return 1;
                                if (y.Specializations.Count > x.Specializations.Count)
                                    return -1;
                                if (y.Specializations.Count < x.Specializations.Count)
                                    return 1;
                            }
                            intReturn = SkillsSection.CompareSkills(x, y);
                        }
                        return intReturn;
                    })),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortAttributeValue"),
                    new SkillSorter((x, y) =>
                    {
                        int intReturn = y.AttributeModifiers.CompareTo(x.AttributeModifiers);
                        if (intReturn == 0)
                        {
                            intReturn = string.Compare(x.DisplayAttribute, y.DisplayAttribute, false, GlobalSettings.CultureInfo);
                            if (intReturn == 0)
                                intReturn = SkillsSection.CompareSkills(x, y);
                        }
                        return intReturn;
                    })),
                new Tuple<string, IComparer<Skill>>(LanguageManager.GetString("Skill_SortAttributeName"),
                    new SkillSorter((x, y) =>
                    {
                        int intReturn = string.Compare(x.DisplayAttribute, y.DisplayAttribute, false, GlobalSettings.CultureInfo);
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
                        int intReturn = string.Compare(x.CurrentDisplayCategory, y.CurrentDisplayCategory, false, GlobalSettings.CultureInfo);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    }))
            };

            return ret;
        }

        private static List<Tuple<string, Predicate<Skill>>> GenerateDropdownFilter(Character objCharacter)
        {
            List<Tuple<string, Predicate<Skill>>> ret = new List<Tuple<string, Predicate<Skill>>>(7)
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

            string strCategory = LanguageManager.GetString("Label_Category");
            foreach (XPathNavigator xmlCategoryNode in XmlManager.LoadXPath("skills.xml", objCharacter?.Settings.EnabledCustomDataDirectoryPaths)
                .SelectAndCacheExpression("/chummer/categories/category[@type = \"active\"]"))
            {
                string strName = xmlCategoryNode.Value;
                if (!string.IsNullOrEmpty(strName))
                    ret.Add(new Tuple<string, Predicate<Skill>>(
                        strCategory + strSpace + (xmlCategoryNode.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strName),
                        skill => skill.SkillCategory == strName));
            }

            string strAttributeLabel = LanguageManager.GetString("String_ExpenseAttribute");
            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                string strAttributeShort = LanguageManager.GetString("String_Attribute" + strAttribute + "Short", GlobalSettings.Language, false);
                if (!string.IsNullOrEmpty(strAttributeShort))
                    ret.Add(new Tuple<string, Predicate<Skill>>(strAttributeLabel + strColon + strSpace + strAttributeShort,
                        skill => skill.Attribute == strAttribute));
            }

            string strSkillGroupLabel = LanguageManager.GetString("String_ExpenseSkillGroup");
            foreach (XPathNavigator xmlSkillGroupNode in XmlManager.LoadXPath("skills.xml", objCharacter?.Settings.EnabledCustomDataDirectoryPaths)
                .SelectAndCacheExpression("/chummer/skillgroups/name"))
            {
                string strName = xmlSkillGroupNode.Value;
                if (!string.IsNullOrEmpty(strName))
                    ret.Add(new Tuple<string, Predicate<Skill>>(
                        strSkillGroupLabel + strSpace + (xmlSkillGroupNode.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strName),
                        skill => skill.SkillGroup == strName));
            }

            return ret;
        }

        private static List<Tuple<string, IComparer<KnowledgeSkill>>> GenerateKnowledgeSortList()
        {
            List<Tuple<string, IComparer<KnowledgeSkill>>> ret = new List<Tuple<string, IComparer<KnowledgeSkill>>>(7)
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
                            intReturn = string.Compare(x.DisplayAttribute, y.DisplayAttribute, false, GlobalSettings.CultureInfo);
                            if (intReturn == 0)
                                intReturn = SkillsSection.CompareSkills(x, y);
                        }
                        return intReturn;
                    })),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortAttributeName"),
                    new KnowledgeSkillSorter((x, y) =>
                    {
                        int intReturn = string.Compare(x.DisplayAttribute, y.DisplayAttribute, false, GlobalSettings.CultureInfo);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<KnowledgeSkill>>(LanguageManager.GetString("Skill_SortCategory"),
                    new KnowledgeSkillSorter((x, y) =>
                    {
                        int intReturn = string.Compare(x.CurrentDisplayCategory, y.CurrentDisplayCategory, false, GlobalSettings.CultureInfo);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    }))
            };

            return ret;
        }

        private static List<Tuple<string, Predicate<KnowledgeSkill>>> GenerateKnowledgeDropdownFilter(Character objCharacter)
        {
            List<Tuple<string, Predicate<KnowledgeSkill>>> ret = new List<Tuple<string, Predicate<KnowledgeSkill>>>(5)
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

            string strCategory = LanguageManager.GetString("Label_Category");
            foreach (XPathNavigator xmlCategoryNode in XmlManager.LoadXPath("skills.xml", objCharacter?.Settings.EnabledCustomDataDirectoryPaths)
                .SelectAndCacheExpression("/chummer/categories/category[@type = \"knowledge\"]"))
            {
                string strName = xmlCategoryNode.Value;
                if (!string.IsNullOrEmpty(strName))
                    ret.Add(new Tuple<string, Predicate<KnowledgeSkill>>(
                        strCategory + strSpace + (xmlCategoryNode.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strName),
                        skill => skill.SkillCategory == strName));
            }

            string strAttributeLabel = LanguageManager.GetString("String_ExpenseAttribute");
            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                string strAttributeShort = LanguageManager.GetString("String_Attribute" + strAttribute + "Short", GlobalSettings.Language, false);
                if (!string.IsNullOrEmpty(strAttributeShort))
                    ret.Add(new Tuple<string, Predicate<KnowledgeSkill>>(strAttributeLabel + strColon + strSpace + strAttributeShort,
                        skill => skill.Attribute == strAttribute));
            }

            return ret;
        }

        private void Control_CustomAttributeChanged(object sender, EventArgs e)
        {
            bool blnVisible = false;
            foreach (SkillControl objSkillControl in _lstActiveSkills.DisplayPanel.Controls)
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
                _lstActiveSkills.Filter(skill => GlobalSettings.CultureInfo.CompareInfo.IndexOf(skill.CurrentDisplayName, cboDisplayFilter.Text, CompareOptions.IgnoreCase) >= 0, true);
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
            using (SelectExoticSkill frmPickExoticSkill = new SelectExoticSkill(_objCharacter))
            {
                frmPickExoticSkill.ShowDialogSafe(this);

                if (frmPickExoticSkill.DialogResult != DialogResult.OK)
                    return;

                objSkill = _objCharacter.SkillsSection.AddExoticSkill(frmPickExoticSkill.SelectedExoticSkill,
                    frmPickExoticSkill.SelectedExoticSkillSpecialisation);
            }

            // Karma check needs to come after the skill is created to make sure bonus-based modifiers (e.g. JoAT) get applied properly (since they can potentially trigger off of the specific exotic skill target)
            if (_objCharacter.Created && objSkill.UpgradeKarmaCost > _objCharacter.Karma)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughKarma"));
                _objCharacter.SkillsSection.Skills.Remove(objSkill);
                return;
            }
            objSkill.Upgrade();
        }

        private void btnKnowledge_Click(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                string strSelectedSkill = string.Empty;

                Form frmToUse = ParentForm ?? Program.MainForm;

                DialogResult eResult = frmToUse.DoThreadSafeFunc(() =>
                {
                    using (SelectItem form = new SelectItem
                           {
                               Description = LanguageManager.GetString("Label_Options_NewKnowledgeSkill")
                           })
                    {
                        form.SetDropdownItemsMode(_objCharacter.SkillsSection.MyDefaultKnowledgeSkills);

                        form.ShowDialogSafe(frmToUse);

                        if (form.DialogResult == DialogResult.OK)
                            strSelectedSkill = form.SelectedItem;
                        return form.DialogResult;
                    }
                });

                if (eResult != DialogResult.OK)
                    return;

                KnowledgeSkill skill = new KnowledgeSkill(_objCharacter)
                {
                    WritableName = strSelectedSkill
                };

                if (_objCharacter.SkillsSection.HasAvailableNativeLanguageSlots
                    && (skill.IsLanguage || string.IsNullOrEmpty(skill.Type)))
                {
                    DialogResult eDialogResult = Program.MainForm.ShowMessageBox(this,
                        string.Format(GlobalSettings.CultureInfo,
                                      LanguageManager.GetString("Message_NewNativeLanguageSkill"),
                                      1 + ImprovementManager.ValueOf(
                                          _objCharacter, Improvement.ImprovementType.NativeLanguageLimit),
                                      skill.WritableName),
                        LanguageManager.GetString("Tip_Skill_NativeLanguage"), MessageBoxButtons.YesNoCancel);
                    switch (eDialogResult)
                    {
                        case DialogResult.Cancel:
                            return;

                        case DialogResult.Yes:
                        {
                            if (!skill.IsLanguage)
                                skill.Type = "Language";
                            skill.IsNativeLanguage = true;
                            break;
                        }
                    }
                }

                _objCharacter.SkillsSection.KnowledgeSkills.Add(skill);
            }
            else
            {
                _objCharacter.SkillsSection.KnowledgeSkills.Add(new KnowledgeSkill(_objCharacter));
            }
        }

        private void btnResetCustomDisplayAttribute_Click(object sender, EventArgs e)
        {
            _lstActiveSkills.SuspendLayout();
            foreach (SkillControl objSkillControl in _lstActiveSkills.DisplayPanel.Controls)
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
                _lstKnowledgeSkills.Filter(skill => GlobalSettings.CultureInfo.CompareInfo.IndexOf(skill.CurrentDisplayName, cboDisplayFilterKnowledge.Text, CompareOptions.IgnoreCase) >= 0, true);
                _lstKnowledgeSkills.ResumeLayout();
            }
        }

        private void splitSkills_Resize(object sender, EventArgs e)
        {
            RefreshSkillGroupLabels();
            RefreshSkillLabels();
            RefreshKnowledgeSkillLabels();
        }
    }
}
