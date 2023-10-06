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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;
using Chummer.Controls.Shared;

namespace Chummer.UI.Skills
{
    public partial class SkillsTabUserControl : UserControl
    {
        public event PropertyChangedEventHandler MakeDirtyWithCharacterUpdate;

        private BindingListDisplay<Skill> _lstActiveSkills;
        private BindingListDisplay<SkillGroup> _lstSkillGroups;
        private BindingListDisplay<KnowledgeSkill> _lstKnowledgeSkills;

        public CancellationToken MyToken { get; set; }

        public SkillsTabUserControl() : this(default)
        {
            // Need to set up constructors like this so that the WinForms designer doesn't freak out
        }

        public SkillsTabUserControl(CancellationToken objMyToken)
        {
            InitializeComponent();

            Disposed += (sender, args) => UnbindSkillsTabUserControl();

            lblGroupKarma.Margin = new Padding(
                lblGroupKarma.Margin.Left,
                lblGroupKarma.Margin.Top,
                lblGroupKarma.Margin.Right + SystemInformation.VerticalScrollBarWidth,
                lblGroupKarma.Margin.Bottom);
            this.UpdateLightDarkMode(token: objMyToken);
            this.TranslateWinForm(token: objMyToken);

            MyToken = objMyToken;
            _lstSortSkills = GenerateSortList();
            _lstSortKnowledgeSkills = GenerateKnowledgeSortList();
        }

        private void UpdateKnoSkillRemaining(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strText =
                _objCharacter.SkillsSection.KnowledgeSkillPointsRemain.ToString(GlobalSettings.CultureInfo) +
                LanguageManager.GetString("String_Of", token: token) +
                _objCharacter.SkillsSection.KnowledgeSkillPoints.ToString(GlobalSettings.CultureInfo);
            int intSkillPointsSpentOnKnoSkills = _objCharacter.SkillsSection.SkillPointsSpentOnKnoskills;
            if (intSkillPointsSpentOnKnoSkills != 0)
                strText += string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_PlusSkillPointsSpent", token: token), intSkillPointsSpentOnKnoSkills);
            lblKnowledgeSkillPoints.Text = strText;
        }

        private async ValueTask UpdateKnoSkillRemainingAsync(CancellationToken token = default)
        {
            SkillsSection objSkillSection = await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false);
            string strText =
                (await objSkillSection.GetKnowledgeSkillPointsRemainAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.CultureInfo) +
                await LanguageManager.GetStringAsync("String_Of", token: token).ConfigureAwait(false) +
                (await objSkillSection.GetKnowledgeSkillPointsAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.CultureInfo);
            int intSkillPointsSpentOnKnoSkills = await objSkillSection.GetSkillPointsSpentOnKnoskillsAsync(token).ConfigureAwait(false);
            if (intSkillPointsSpentOnKnoSkills != 0)
                strText += string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_PlusSkillPointsSpent", token: token).ConfigureAwait(false), intSkillPointsSpentOnKnoSkills);
            await lblKnowledgeSkillPoints.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
        }

        private Character _objCharacter;
        private List<Tuple<string, Predicate<Skill>>> _lstDropDownActiveSkills;
        private readonly List<Tuple<string, IComparer<Skill>>> _lstSortSkills;
        private bool _blnActiveSkillSearchMode;
        private bool _blnKnowledgeSkillSearchMode;
        private List<Tuple<string, Predicate<KnowledgeSkill>>> _lstDropDownKnowledgeSkills;
        private readonly List<Tuple<string, IComparer<KnowledgeSkill>>> _lstSortKnowledgeSkills;

        private async void SkillsTabUserControl_Load(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(token: MyToken).ConfigureAwait(false);
                try
                {
                    if (_objCharacter == null)
                        await RealLoad(MyToken, MyToken).ConfigureAwait(false);
                    await this.DoThreadSafeAsync(() =>
                    {
                        SuspendLayout();
                        try
                        {
                            RefreshSkillLabels();
                            RefreshKnowledgeSkillLabels();
                            RefreshSkillGroupLabels();
                        }
                        finally
                        {
                            ResumeLayout(true);
                        }
                    }, MyToken).ConfigureAwait(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        public Character CachedCharacter { get; set; }

        public async ValueTask RealLoad(CancellationToken objMyToken = default, CancellationToken token = default)
        {
            if (CachedCharacter != null)
            {
                if (Interlocked.CompareExchange(ref _objCharacter, CachedCharacter, null) != null)
                    return;
            }
            else if (ParentForm is CharacterShared frmParent && frmParent.CharacterObject != null)
            {
                if (Interlocked.CompareExchange(ref _objCharacter, frmParent.CharacterObject, null) != null)
                    return;
            }
            else
            {
                Character objCharacter = new Character();
                if (Interlocked.CompareExchange(ref _objCharacter, objCharacter, null) != null)
                {
                    await objCharacter.DisposeAsync().ConfigureAwait(false);
                    return;
                }
                await this.DoThreadSafeAsync(x => x.Disposed += (sender, args) => objCharacter.Dispose(), token).ConfigureAwait(false);
                Utils.BreakIfDebug();
            }
            MyToken = objMyToken;

            if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                return;

            _lstDropDownActiveSkills = GenerateDropdownFilter(_objCharacter);
            _lstDropDownKnowledgeSkills = GenerateKnowledgeDropdownFilter(_objCharacter);

            Stopwatch sw = Stopwatch.StartNew();  //Benchmark, should probably remove in release
            //Keep everything visible until ready to display everything. This
            //seems to prevent redrawing everything each time anything is added
            //Not benched, but should be faster

            //Might also be useless horseshit, 2 lines

            //Visible = false;

            try
            {
                bool blnExoticVisible
                    = (await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false))
                    .SelectSingleNode(
                        "/chummer/skills/skill[exotic = "
                        + bool.TrueString.CleanXPath()
                        + ']') != null;
                await this.DoThreadSafeAsync(() =>
                {
                    Stopwatch swDisplays = Stopwatch.StartNew();
                    Stopwatch parts = Stopwatch.StartNew();
                    SuspendLayout();
                    tlpActiveSkills.SuspendLayout();
                    tlpSkillGroups.SuspendLayout();
                    tlpBottomPanel.SuspendLayout();
                    try
                    {
                        _lstActiveSkills
                            = new BindingListDisplay<Skill>(_objCharacter.SkillsSection.Skills, MakeActiveSkill)
                            {
                                Dock = DockStyle.Fill
                            };
                        Disposed += (sender, args) => _lstActiveSkills.Dispose();

                        Control MakeActiveSkill(Skill arg)
                        {
                            SkillControl objSkillControl = new SkillControl(arg, objMyToken);
                            objSkillControl.CustomAttributeChanged += Control_CustomAttributeChanged;
                            return objSkillControl;
                        }

                        swDisplays.TaskEnd("_lstActiveSkills");

                        tlpActiveSkills.Controls.Add(_lstActiveSkills, 0, 2);
                        tlpActiveSkills.SetColumnSpan(_lstActiveSkills, 5);

                        swDisplays.TaskEnd("_lstActiveSkills add");

                        _lstKnowledgeSkills = new BindingListDisplay<KnowledgeSkill>(
                            _objCharacter.SkillsSection.KnowledgeSkills,
                            knoSkill => new KnowledgeSkillControl(knoSkill, objMyToken))
                        {
                            Dock = DockStyle.Fill
                        };
                        Disposed += (sender, args) => _lstKnowledgeSkills.Dispose();

                        swDisplays.TaskEnd("_lstKnowledgeSkills");

                        tlpBottomPanel.Controls.Add(_lstKnowledgeSkills, 0, 2);
                        tlpBottomPanel.SetColumnSpan(_lstKnowledgeSkills, 4);

                        swDisplays.TaskEnd("_lstKnowledgeSkills add");

                        if (_objCharacter.SkillsSection.SkillGroups.Count > 0)
                        {
                            _lstSkillGroups = new BindingListDisplay<SkillGroup>(
                                _objCharacter.SkillsSection.SkillGroups,
                                group => new SkillGroupControl(group, objMyToken))
                            {
                                Dock = DockStyle.Fill
                            };
                            Disposed += (sender, args) => _lstSkillGroups.Dispose();
                            _lstSkillGroups.Filter(
                                z => z.SkillList.Any(y => _objCharacter.SkillsSection.HasActiveSkill(y.DictionaryKey)),
                                true);
                            _lstSkillGroups.Sort(new SkillGroupSorter(SkillsSection.CompareSkillGroups));

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
                        try
                        {
                            cboDisplayFilter.DataSource = null;
                            cboDisplayFilter.ValueMember = "Item2";
                            cboDisplayFilter.DisplayMember = "Item1";
                            cboDisplayFilter.DataSource = _lstDropDownActiveSkills;
                            cboDisplayFilter.SelectedIndex = 1;
                            cboDisplayFilter.MaxDropDownItems = _lstDropDownActiveSkills.Count;
                        }
                        finally
                        {
                            cboDisplayFilter.EndUpdate();
                        }

                        cboDisplayFilterKnowledge.BeginUpdate();
                        try
                        {
                            cboDisplayFilterKnowledge.DataSource = null;
                            cboDisplayFilterKnowledge.ValueMember = "Item2";
                            cboDisplayFilterKnowledge.DisplayMember = "Item1";
                            cboDisplayFilterKnowledge.DataSource = _lstDropDownKnowledgeSkills;
                            cboDisplayFilterKnowledge.SelectedIndex = 1;
                            cboDisplayFilterKnowledge.MaxDropDownItems = _lstDropDownKnowledgeSkills.Count;
                        }
                        finally
                        {
                            cboDisplayFilterKnowledge.EndUpdate();
                        }

                        parts.TaskEnd("_ddl databind");

                        cboSort.BeginUpdate();
                        try
                        {
                            cboSort.DataSource = null;
                            cboSort.ValueMember = "Item2";
                            cboSort.DisplayMember = "Item1";
                            cboSort.DataSource = _lstSortSkills;
                            cboSort.SelectedIndex = 0;
                            cboSort.MaxDropDownItems = _lstSortSkills.Count;
                        }
                        finally
                        {
                            cboSort.EndUpdate();
                        }

                        cboSortKnowledge.BeginUpdate();
                        try
                        {
                            cboSortKnowledge.DataSource = null;
                            cboSortKnowledge.ValueMember = "Item2";
                            cboSortKnowledge.DisplayMember = "Item1";
                            cboSortKnowledge.DataSource = _lstSortKnowledgeSkills;
                            cboSortKnowledge.SelectedIndex = 0;
                            cboSortKnowledge.MaxDropDownItems = _lstSortKnowledgeSkills.Count;
                        }
                        finally
                        {
                            cboSortKnowledge.EndUpdate();
                        }

                        parts.TaskEnd("_sort databind");

                        if (_lstSkillGroups != null)
                            _lstSkillGroups.ChildPropertyChanged += ChildPropertyChanged;
                        _lstActiveSkills.ChildPropertyChanged += ChildPropertyChanged;
                        _lstKnowledgeSkills.ChildPropertyChanged += ChildPropertyChanged;

                        if (_objCharacter.Created)
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

                        btnExotic.Visible = blnExoticVisible;
                    }
                    finally
                    {
                        tlpBottomPanel.ResumeLayout(false);
                        tlpSkillGroups.ResumeLayout(false);
                        tlpActiveSkills.ResumeLayout(false);
                        ResumeLayout(true);
                    }
                }, token: token).ConfigureAwait(false);
                if (!await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                {
                    await lblGroupsSp.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacter,
                                                                          nameof(Character
                                                                              .EffectiveBuildMethodUsesPriorityTables),
                                                                          x => x
                                                                               .GetEffectiveBuildMethodUsesPriorityTablesAsync(
                                                                                   objMyToken).AsTask(), objMyToken,
                                                                          objMyToken)
                                     .ConfigureAwait(false);
                    await lblActiveSp.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacter,
                                                                          nameof(Character
                                                                              .EffectiveBuildMethodUsesPriorityTables),
                                                                          x => x
                                                                               .GetEffectiveBuildMethodUsesPriorityTablesAsync(
                                                                                   objMyToken).AsTask(), objMyToken,
                                                                          objMyToken)
                                     .ConfigureAwait(false);
                    await lblBuyWithKarma.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacter,
                                                                              nameof(Character
                                                                                  .EffectiveBuildMethodUsesPriorityTables),
                                                                              x => x
                                                                                  .GetEffectiveBuildMethodUsesPriorityTablesAsync(
                                                                                      objMyToken).AsTask(), objMyToken,
                                                                              objMyToken)
                                         .ConfigureAwait(false);

                    await lblKnoSp.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y,
                                                                       _objCharacter.SkillsSection,
                                                                       nameof(SkillsSection.HasKnowledgePoints),
                                                                       x => x.GetHasKnowledgePointsAsync(objMyToken)
                                                                             .AsTask(),
                                                                       objMyToken, objMyToken).ConfigureAwait(false);
                    await lblKnoBwk.RegisterOneWayAsyncDataBindingAsync(
                        (x, y) => x.Visible = y, _objCharacter.SkillsSection,
                        nameof(SkillsSection.HasKnowledgePoints),
                        x => x.GetHasKnowledgePointsAsync(objMyToken).AsTask(),
                        objMyToken, objMyToken).ConfigureAwait(false);
                    await UpdateKnoSkillRemainingAsync(objMyToken).ConfigureAwait(false);
                }

                IAsyncDisposable objLocker = await _objCharacter.SkillsSection.LockObject
                                                                .EnterWriteLockAsync(objMyToken).ConfigureAwait(false);
                try
                {
                    _objCharacter.SkillsSection.Skills.ListChanged += SkillsOnListChanged;
                    _objCharacter.SkillsSection.SkillGroups.ListChanged += SkillGroupsOnListChanged;
                    _objCharacter.SkillsSection.KnowledgeSkills.ListChanged += KnowledgeSkillsOnListChanged;
                    _objCharacter.SkillsSection.PropertyChanged += SkillsSectionOnPropertyChanged;
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                sw.Stop();
                Debug.WriteLine("RealLoad() in {0} ms", sw.Elapsed.TotalMilliseconds);
            }
        }

        private void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MakeDirtyWithCharacterUpdate?.Invoke(sender, e);
        }

        private async void SkillsSectionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if ((e.PropertyName == nameof(SkillsSection.KnowledgeSkillPointsRemain)
                     || e.PropertyName == nameof(SkillsSection.KnowledgeSkillPoints)
                     || e.PropertyName == nameof(SkillsSection.SkillPointsSpentOnKnoskills))
                    && _objCharacter != null && !await _objCharacter.GetCreatedAsync(MyToken).ConfigureAwait(false))
                {
                    await UpdateKnoSkillRemainingAsync(MyToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
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
            {
                RefreshSkillLabels();
                // Special, hacky fix to force skill group displays to refresh when skill lists could change (e.g., because skill groups can end up getting
                // added before their skills do through said skills' constructors and how they are used, making them not show up in the UI initially)
                if (e.ListChangedType == ListChangedType.Reset || e.ListChangedType == ListChangedType.ItemDeleted)
                {
                    _lstSkillGroups.Filter(
                        z => z.SkillList.Any(y => _objCharacter.SkillsSection.HasActiveSkill(y.DictionaryKey)),
                        true);
                }
                else if (e.ListChangedType == ListChangedType.ItemAdded)
                {
                    SkillGroup objNewSkillGroup = _objCharacter.SkillsSection.Skills[e.NewIndex]?.SkillGroupObject;
                    if (objNewSkillGroup != null &&
                        objNewSkillGroup.SkillList.Count(y =>
                            _objCharacter.SkillsSection.HasActiveSkill(y.DictionaryKey)) == 1)
                    {
                        _lstSkillGroups.Filter(
                            z => z.SkillList.Any(y => _objCharacter.SkillsSection.HasActiveSkill(y.DictionaryKey)),
                            true);
                    }
                }
            }
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
            if (_objCharacter?.IsDisposed == false)
            {
                try
                {
                    using (_objCharacter.SkillsSection.LockObject.EnterWriteLock())
                    {
                        _objCharacter.SkillsSection.Skills.ListChanged -= SkillsOnListChanged;
                        _objCharacter.SkillsSection.SkillGroups.ListChanged -= SkillGroupsOnListChanged;
                        _objCharacter.SkillsSection.KnowledgeSkills.ListChanged -= KnowledgeSkillsOnListChanged;
                        _objCharacter.SkillsSection.PropertyChanged -= SkillsSectionOnPropertyChanged;
                    }
                }
                catch (ObjectDisposedException)
                {
                    //swallow this
                }
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
                    try
                    {
                        _lstActiveSkills.Filter(selectedItem.Item2);
                    }
                    finally
                    {
                        _lstActiveSkills.ResumeLayout();
                    }
                }
            }
        }

        private void cboDisplayFilter_TextUpdate(object sender, EventArgs e)
        {
            if (_blnActiveSkillSearchMode)
            {
                _lstActiveSkills.SuspendLayout();
                try
                {
                    _lstActiveSkills.Filter(
                        skill => GlobalSettings.CultureInfo.CompareInfo.IndexOf(
                            skill.CurrentDisplayName, cboDisplayFilter.Text, CompareOptions.IgnoreCase) >= 0, true);
                }
                finally
                {
                    _lstActiveSkills.ResumeLayout();
                }
            }
        }

        private void cboSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSort.SelectedItem is Tuple<string, IComparer<Skill>> selectedItem)
            {
                _lstActiveSkills.SuspendLayout();
                try
                {
                    _lstActiveSkills.Sort(selectedItem.Item2);
                }
                finally
                {
                    _lstActiveSkills.ResumeLayout();
                }
            }
        }

        private async void btnExotic_Click(object sender, EventArgs e)
        {
            try
            {
                ExoticSkill objSkill;
                using (ThreadSafeForm<SelectExoticSkill> frmPickExoticSkill
                       = await ThreadSafeForm<SelectExoticSkill>
                               .GetAsync(() => new SelectExoticSkill(_objCharacter), MyToken).ConfigureAwait(false))
                {
                    if (await frmPickExoticSkill.ShowDialogSafeAsync(_objCharacter, MyToken).ConfigureAwait(false)
                        != DialogResult.OK)
                        return;

                    objSkill = await _objCharacter.SkillsSection.AddExoticSkillAsync(
                        frmPickExoticSkill.MyForm.SelectedExoticSkill,
                        frmPickExoticSkill.MyForm.SelectedExoticSkillSpecialisation, MyToken).ConfigureAwait(false);
                }

                using (await EnterReadLock.EnterAsync(objSkill.LockObject, MyToken).ConfigureAwait(false))
                {
                    // Karma check needs to come after the skill is created to make sure bonus-based modifiers (e.g. JoAT) get applied properly (since they can potentially trigger off of the specific exotic skill target)
                    if (await _objCharacter.GetCreatedAsync(MyToken).ConfigureAwait(false)
                        && await objSkill.GetUpgradeKarmaCostAsync(MyToken).ConfigureAwait(false)
                        > await _objCharacter.GetKarmaAsync(MyToken).ConfigureAwait(false))
                    {
                        Program.ShowScrollableMessageBox(await LanguageManager
                                                               .GetStringAsync("Message_NotEnoughKarma", token: MyToken)
                                                               .ConfigureAwait(false));
                        await _objCharacter.SkillsSection.Skills.RemoveAsync(objSkill, MyToken)
                                           .ConfigureAwait(false);
                        return;
                    }

                    await objSkill.Upgrade(MyToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void btnKnowledge_Click(object sender, EventArgs e)
        {
            try
            {
                if (await _objCharacter.GetCreatedAsync(MyToken).ConfigureAwait(false))
                {
                    string strSelectedSkill;

                    string strDescription = await LanguageManager
                                                  .GetStringAsync("Label_Options_NewKnowledgeSkill", token: MyToken)
                                                  .ConfigureAwait(false);
                    using (ThreadSafeForm<SelectItem> form = await ThreadSafeForm<SelectItem>.GetAsync(
                               () => new SelectItem
                               {
                                   Description = strDescription
                               }, MyToken).ConfigureAwait(false))
                    {
                        form.MyForm.SetDropdownItemsMode(_objCharacter.SkillsSection.MyDefaultKnowledgeSkills);
                        if (await form.ShowDialogSafeAsync(_objCharacter, MyToken).ConfigureAwait(false)
                            != DialogResult.OK)
                            return;
                        strSelectedSkill = form.MyForm.SelectedItem;
                    }

                    KnowledgeSkill skill = new KnowledgeSkill(_objCharacter);
                    await skill.SetWritableNameAsync(strSelectedSkill, MyToken).ConfigureAwait(false);

                    if (await _objCharacter.SkillsSection.GetHasAvailableNativeLanguageSlotsAsync(MyToken)
                                           .ConfigureAwait(false)
                        && (await skill.GetIsLanguageAsync(MyToken).ConfigureAwait(false)
                            || string.IsNullOrEmpty(await skill.GetTypeAsync(MyToken).ConfigureAwait(false))))
                    {
                        DialogResult eDialogResult = Program.ShowScrollableMessageBox(this,
                                                                            string.Format(GlobalSettings.CultureInfo,
                                                                                await LanguageManager
                                                                                    .GetStringAsync(
                                                                                        "Message_NewNativeLanguageSkill",
                                                                                        token: MyToken)
                                                                                    .ConfigureAwait(false),
                                                                                1 + await ImprovementManager
                                                                                    .ValueOfAsync(
                                                                                        _objCharacter,
                                                                                        Improvement.ImprovementType
                                                                                            .NativeLanguageLimit,
                                                                                        token: MyToken)
                                                                                    .ConfigureAwait(false),
                                                                                await skill
                                                                                    .GetWritableNameAsync(MyToken)
                                                                                    .ConfigureAwait(false)),
                                                                            await LanguageManager
                                                                                .GetStringAsync(
                                                                                    "Tip_Skill_NativeLanguage",
                                                                                    token: MyToken)
                                                                                .ConfigureAwait(false),
                                                                            MessageBoxButtons.YesNoCancel);
                        switch (eDialogResult)
                        {
                            case DialogResult.Cancel:
                                return;

                            case DialogResult.Yes:
                            {
                                if (!await skill.GetIsLanguageAsync(MyToken).ConfigureAwait(false))
                                    await skill.SetTypeAsync("Language", MyToken).ConfigureAwait(false);
                                await skill.SetIsNativeLanguageAsync(true, MyToken).ConfigureAwait(false);
                                break;
                            }
                        }
                    }

                    await _objCharacter.SkillsSection.KnowledgeSkills.AddAsync(skill, MyToken)
                                       .ConfigureAwait(false);
                }
                else
                {
                    await _objCharacter.SkillsSection.KnowledgeSkills
                                       .AddAsync(new KnowledgeSkill(_objCharacter), MyToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void btnResetCustomDisplayAttribute_Click(object sender, EventArgs e)
        {
            try
            {
                await _lstActiveSkills.DoThreadSafeAsync(x => x.SuspendLayout(), token: MyToken)
                                      .ConfigureAwait(false);
                try
                {
                    foreach (SkillControl objSkillControl in await _lstActiveSkills.DisplayPanel
                                 .DoThreadSafeFuncAsync(x => x.Controls, token: MyToken).ConfigureAwait(false))
                    {
                        if (objSkillControl.CustomAttributeSet)
                            await objSkillControl.ResetSelectAttribute(MyToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await _lstActiveSkills.DoThreadSafeAsync(x => x.ResumeLayout(), MyToken)
                                          .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void cboSortKnowledge_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSortKnowledge.SelectedItem is Tuple<string, IComparer<KnowledgeSkill>> selectedItem)
            {
                _lstKnowledgeSkills.SuspendLayout();
                try
                {
                    _lstKnowledgeSkills.Sort(selectedItem.Item2);
                }
                finally
                {
                    _lstKnowledgeSkills.ResumeLayout();
                }
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
                    try
                    {
                        _lstKnowledgeSkills.Filter(selectedItem.Item2);
                    }
                    finally
                    {
                        _lstKnowledgeSkills.ResumeLayout();
                    }
                }
            }
        }

        private void cboDisplayFilterKnowledge_TextUpdate(object sender, EventArgs e)
        {
            if (_blnKnowledgeSkillSearchMode)
            {
                _lstKnowledgeSkills.SuspendLayout();
                try
                {
                    _lstKnowledgeSkills.Filter(
                        skill => GlobalSettings.CultureInfo.CompareInfo.IndexOf(
                            skill.CurrentDisplayName, cboDisplayFilterKnowledge.Text, CompareOptions.IgnoreCase) >= 0,
                        true);
                }
                finally
                {
                    _lstKnowledgeSkills.ResumeLayout();
                }
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
