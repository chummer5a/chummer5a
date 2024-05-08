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
    public partial class SkillsTabUserControl : UserControl, IHasCharacterObject
    {
        public event PropertyChangedAsyncEventHandler MakeDirtyWithCharacterUpdate;

        private BindingListDisplay<Skill> _lstActiveSkills;
        private BindingListDisplay<SkillGroup> _lstSkillGroups;
        private BindingListDisplay<KnowledgeSkill> _lstKnowledgeSkills;

        public CancellationToken MyToken { get; set; }

        public Character CharacterObject => _objCharacter;

        public SkillsTabUserControl() : this(default)
        {
            // Need to set up constructors like this so that the WinForms designer doesn't freak out
        }

        public SkillsTabUserControl(CancellationToken objMyToken)
        {
            InitializeComponent();

            Disposed += (sender, args) => UnbindSkillsTabUserControl(CancellationToken.None);

            lblGroupKarma.Margin = new Padding(
                lblGroupKarma.Margin.Left,
                lblGroupKarma.Margin.Top,
                lblGroupKarma.Margin.Right + SystemInformation.VerticalScrollBarWidth,
                lblGroupKarma.Margin.Bottom);
            this.UpdateLightDarkMode(token: objMyToken);
            this.TranslateWinForm(token: objMyToken);

            MyToken = objMyToken;
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

        private async Task UpdateKnoSkillRemainingAsync(CancellationToken token = default)
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
        private List<Tuple<string, IComparer<Skill>>> _lstSortSkills;
        private bool _blnActiveSkillSearchMode;
        private bool _blnKnowledgeSkillSearchMode;
        private List<Tuple<string, Predicate<KnowledgeSkill>>> _lstDropDownKnowledgeSkills;
        private List<Tuple<string, IComparer<KnowledgeSkill>>> _lstSortKnowledgeSkills;

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
                            RefreshSkillLabels(MyToken);
                            RefreshKnowledgeSkillLabels(MyToken);
                            RefreshSkillGroupLabels(MyToken);
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

        public async Task RealLoad(CancellationToken objMyToken = default, CancellationToken token = default)
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

            _lstSortSkills = await GenerateSortList(token).ConfigureAwait(false);
            _lstSortKnowledgeSkills = await GenerateKnowledgeSortList(token).ConfigureAwait(false);
            _lstDropDownActiveSkills = await GenerateDropdownFilter(_objCharacter, token).ConfigureAwait(false);
            _lstDropDownKnowledgeSkills = await GenerateKnowledgeDropdownFilter(_objCharacter, token).ConfigureAwait(false);

#if DEBUG
            Stopwatch sw = Utils.StopwatchPool.Get();
            try
            {
                sw.Start();
#endif
                //Keep everything visible until ready to display everything. This
                //seems to prevent redrawing everything each time anything is added
                //Not benched, but should be faster

                //Might also be useless horseshit, 2 lines

                //Visible = false;

                bool blnExoticVisible
                    = (await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false))
                    .SelectSingleNode(
                        "/chummer/skills/skill[exotic = "
                        + bool.TrueString.CleanXPath()
                        + ']') != null;
                await this.DoThreadSafeAsync(() =>
                {
                    using (new FetchSafelyFromPool<Stopwatch>(Utils.StopwatchPool, out Stopwatch parts))
                    {
                        parts.Start();
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

                            parts.TaskEnd("_lstActiveSkills");

                            tlpActiveSkills.Controls.Add(_lstActiveSkills, 0, 2);
                            tlpActiveSkills.SetColumnSpan(_lstActiveSkills, 5);

                            parts.TaskEnd("_lstActiveSkills add");

                            _lstKnowledgeSkills = new BindingListDisplay<KnowledgeSkill>(
                                _objCharacter.SkillsSection.KnowledgeSkills,
                                knoSkill => new KnowledgeSkillControl(knoSkill, objMyToken))
                            {
                                Dock = DockStyle.Fill
                            };
                            Disposed += (sender, args) => _lstKnowledgeSkills.Dispose();

                            parts.TaskEnd("_lstKnowledgeSkills");

                            tlpBottomPanel.Controls.Add(_lstKnowledgeSkills, 0, 2);
                            tlpBottomPanel.SetColumnSpan(_lstKnowledgeSkills, 4);

                            parts.TaskEnd("_lstKnowledgeSkills add");

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
                                    z => z.SkillList.Any(y =>
                                        _objCharacter.SkillsSection.HasActiveSkill(y.DictionaryKey)),
                                    (z, t) => z.SkillList.AnyAsync(async y =>
                                        await _objCharacter.SkillsSection.HasActiveSkillAsync(
                                            await y.GetDictionaryKeyAsync(t).ConfigureAwait(false), t).ConfigureAwait(false), t),
                                    true);
                                _lstSkillGroups.Sort(new SkillGroupSorter(SkillsSection.CompareSkillGroups));

                                parts.TaskEnd("_lstSkillGroups");

                                tlpSkillGroups.Controls.Add(_lstSkillGroups, 0, 1);
                                tlpSkillGroups.SetColumnSpan(_lstSkillGroups, 3);

                                parts.TaskEnd("_lstSkillGroups add");
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
                                _lstSkillGroups.ChildPropertyChangedAsync += ChildPropertyChanged;
                            _lstActiveSkills.ChildPropertyChangedAsync += ChildPropertyChanged;
                            _lstKnowledgeSkills.ChildPropertyChangedAsync += ChildPropertyChanged;

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
                    }
                }, token: token).ConfigureAwait(false);

                await Task.WhenAll(_lstActiveSkills.ContentControls.OfType<SkillControl>()
                    .Select(x => x.DoLoad(token))).ConfigureAwait(false);
                await Task.WhenAll(_lstKnowledgeSkills.ContentControls.OfType<KnowledgeSkillControl>()
                    .Select(x => x.DoLoad(token))).ConfigureAwait(false);
                await Task.WhenAll(_lstSkillGroups.ContentControls.OfType<SkillGroupControl>()
                    .Select(x => x.DoLoad(token))).ConfigureAwait(false);

                if (!await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                {
                    await lblGroupsSp.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacter,
                            nameof(Character
                                .EffectiveBuildMethodUsesPriorityTables),
                            x => x
                                .GetEffectiveBuildMethodUsesPriorityTablesAsync(
                                    objMyToken), token)
                        .ConfigureAwait(false);
                    await lblActiveSp.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objCharacter,
                            nameof(Character
                                .EffectiveBuildMethodUsesPriorityTables),
                            x => x
                                .GetEffectiveBuildMethodUsesPriorityTablesAsync(
                                    objMyToken), token)
                        .ConfigureAwait(false);
                    await lblBuyWithKarma.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y,
                            _objCharacter,
                            nameof(Character
                                .EffectiveBuildMethodUsesPriorityTables),
                            x => x
                                .GetEffectiveBuildMethodUsesPriorityTablesAsync(
                                    objMyToken), token)
                        .ConfigureAwait(false);

                    await lblKnoSp.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y,
                        _objCharacter.SkillsSection,
                        nameof(SkillsSection.HasKnowledgePoints),
                        x => x.GetHasKnowledgePointsAsync(objMyToken)
                            ,
                        token).ConfigureAwait(false);
                    await lblKnoBwk.RegisterOneWayAsyncDataBindingAsync(
                        (x, y) => x.Visible = y, _objCharacter.SkillsSection,
                        nameof(SkillsSection.HasKnowledgePoints),
                        x => x.GetHasKnowledgePointsAsync(objMyToken),
                        token).ConfigureAwait(false);
                    await UpdateKnoSkillRemainingAsync(token).ConfigureAwait(false);
                }

                IAsyncDisposable objLocker = await _objCharacter.SkillsSection.LockObject
                    .EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _objCharacter.SkillsSection.Skills.ListChangedAsync += SkillsOnListChanged;
                    _objCharacter.SkillsSection.SkillGroups.ListChanged += SkillGroupsOnListChanged;
                    _objCharacter.SkillsSection.KnowledgeSkills.ListChanged += KnowledgeSkillsOnListChanged;
                    _objCharacter.SkillsSection.MultiplePropertiesChangedAsync += SkillsSectionOnPropertyChanged;
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
#if DEBUG
            }
            finally
            {
                sw.Stop();
                Debug.WriteLine("RealLoad() in {0} ms", sw.Elapsed.TotalMilliseconds);
                Utils.StopwatchPool.Return(ref sw);
            }
#endif
        }

        private Task ChildPropertyChanged(object sender, PropertyChangedEventArgs e, CancellationToken token = default)
        {
            if (MakeDirtyWithCharacterUpdate != null)
                return MakeDirtyWithCharacterUpdate.Invoke(sender, e, token);
            return token.IsCancellationRequested ? Task.FromCanceled(token) : Task.CompletedTask;
        }

        private async Task SkillsSectionOnPropertyChanged(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                if ((e.PropertyNames.Contains(nameof(SkillsSection.KnowledgeSkillPointsRemain))
                     || e.PropertyNames.Contains(nameof(SkillsSection.KnowledgeSkillPoints))
                     || e.PropertyNames.Contains(nameof(SkillsSection.SkillPointsSpentOnKnoskills)))
                    && _objCharacter != null && !await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                {
                    await UpdateKnoSkillRemainingAsync(token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void RefreshSkillGroupLabels(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_lstSkillGroups == null)
                return;
            int intNameLabelWidth = lblSkillGroups.PreferredWidth;
            foreach (SkillGroupControl sg in _lstSkillGroups.DisplayPanel.Controls)
            {
                token.ThrowIfCancellationRequested();
                intNameLabelWidth = Math.Max(sg.NameWidth, intNameLabelWidth);
            }
            token.ThrowIfCancellationRequested();
            lblSkillGroups.MinimumSize = new Size(intNameLabelWidth, lblSkillGroups.MinimumSize.Height);
            token.ThrowIfCancellationRequested();
            foreach (SkillGroupControl s in _lstSkillGroups.DisplayPanel.Controls)
            {
                token.ThrowIfCancellationRequested();
                s.MoveControls(intNameLabelWidth);
            }
        }

        private void RefreshSkillLabels(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_lstActiveSkills == null)
                return;
            int intNameLabelWidth = lblActiveSkills.PreferredWidth;
            int intRatingLabelWidth = lblActiveSp.PreferredWidth;
            foreach (SkillControl objSkillControl in _lstActiveSkills.DisplayPanel.Controls)
            {
                token.ThrowIfCancellationRequested();
                intNameLabelWidth = Math.Max(intNameLabelWidth, objSkillControl.NameWidth);
                intRatingLabelWidth = Math.Max(intRatingLabelWidth, objSkillControl.NudSkillWidth);
            }
            token.ThrowIfCancellationRequested();
            lblActiveSkills.MinimumSize = new Size(intNameLabelWidth - lblActiveSkills.Margin.Right, lblActiveSkills.MinimumSize.Height);
            token.ThrowIfCancellationRequested();
            lblActiveKarma.Margin = new Padding(
                Math.Max(0, lblActiveSp.Margin.Left + intRatingLabelWidth - lblActiveSp.Width),
                lblActiveKarma.Margin.Top,
                lblActiveKarma.Margin.Right,
                lblActiveKarma.Margin.Bottom);
            token.ThrowIfCancellationRequested();
            foreach (SkillControl objSkillControl in _lstActiveSkills.DisplayPanel.Controls)
            {
                token.ThrowIfCancellationRequested();
                objSkillControl.MoveControls(intNameLabelWidth);
            }
        }

        private void RefreshKnowledgeSkillLabels(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_lstKnowledgeSkills == null)
                return;
            int intNameLabelWidth = 0;
            int intRatingLabelWidth = lblKnoSp.PreferredWidth;
            int intRightButtonsWidth = 0;
            token.ThrowIfCancellationRequested();
            foreach (KnowledgeSkillControl objKnowledgeSkillControl in _lstKnowledgeSkills.DisplayPanel.Controls)
            {
                token.ThrowIfCancellationRequested();
                intNameLabelWidth = Math.Max(intNameLabelWidth, objKnowledgeSkillControl.NameWidth);
                intRatingLabelWidth = Math.Max(intRatingLabelWidth, objKnowledgeSkillControl.NudSkillWidth);
                intRightButtonsWidth = Math.Max(intRightButtonsWidth, objKnowledgeSkillControl.RightButtonsWidth);
            }
            token.ThrowIfCancellationRequested();
            lblKnowledgeSkills.MinimumSize = new Size(intNameLabelWidth, lblKnowledgeSkills.MinimumSize.Height);
            token.ThrowIfCancellationRequested();
            lblKnoKarma.Margin = new Padding(
                Math.Max(0, lblKnoSp.Margin.Left + intRatingLabelWidth - lblKnoSp.Width),
                lblKnoKarma.Margin.Top,
                lblKnoKarma.Margin.Right,
                lblKnoKarma.Margin.Bottom);
            token.ThrowIfCancellationRequested();
            lblKnoBwk.Margin = new Padding(
                lblKnoBwk.Margin.Left,
                lblKnoBwk.Margin.Top,
                Math.Max(0, lblKnoBwk.Margin.Left + intRightButtonsWidth + SystemInformation.VerticalScrollBarWidth - lblKnoBwk.PreferredWidth / 2),
                lblKnoBwk.Margin.Bottom);
        }

        private void SkillGroupsOnListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType != ListChangedType.Reset
                && e.ListChangedType != ListChangedType.ItemAdded
                && e.ListChangedType != ListChangedType.ItemDeleted)
                return;

            try
            {
                RefreshSkillGroupLabels(MyToken);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async Task SkillsOnListChanged(object sender, ListChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (e.ListChangedType != ListChangedType.Reset
                && e.ListChangedType != ListChangedType.ItemAdded
                && e.ListChangedType != ListChangedType.ItemDeleted)
                return;

            await this.DoThreadSafeAsync(RefreshSkillLabels, token).ConfigureAwait(false);
            // Special, hacky fix to force skill group displays to refresh when skill lists could change (e.g, because skill groups can end up getting
            // added before their skills do through said skills' constructors and how they are used, making them not show up in the UI initially)
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                Skill objNewSkill =
                    await (await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false))
                            .GetSkillsAsync(token).ConfigureAwait(false)).GetValueAtAsync(e.NewIndex, token)
                        .ConfigureAwait(false);
                SkillGroup objNewSkillGroup = objNewSkill?.SkillGroupObject;
                if (objNewSkillGroup != null &&
                    await objNewSkillGroup.SkillList.CountAsync(async y =>
                        await _objCharacter.SkillsSection
                            .HasActiveSkillAsync(await y.GetDictionaryKeyAsync(token).ConfigureAwait(false),
                                token)
                            .ConfigureAwait(false), token).ConfigureAwait(false) == 1)
                {
                    await _lstSkillGroups
                        .DoThreadSafeAsync(
                            x => x.Filter(
                                z => z.SkillList.Any(y =>
                                    _objCharacter.SkillsSection.HasActiveSkill(y.DictionaryKey)),
                                (z, t) => z.SkillList.AnyAsync(async y =>
                                    await _objCharacter.SkillsSection.HasActiveSkillAsync(
                                            await y.GetDictionaryKeyAsync(t).ConfigureAwait(false), t)
                                        .ConfigureAwait(false), t), true), token)
                        .ConfigureAwait(false);
                }
            }
            else
            {
                await _lstSkillGroups
                    .DoThreadSafeAsync(
                        x => x.Filter(
                            z => z.SkillList.Any(y => _objCharacter.SkillsSection.HasActiveSkill(y.DictionaryKey)),
                            (z, t) => z.SkillList.AnyAsync(async y =>
                                    await _objCharacter.SkillsSection.HasActiveSkillAsync(
                                        await y.GetDictionaryKeyAsync(t).ConfigureAwait(false), t)
                                    .ConfigureAwait(false),
                                t),
                            true), token).ConfigureAwait(false);
            }
        }

        private void KnowledgeSkillsOnListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType != ListChangedType.Reset
                && e.ListChangedType != ListChangedType.ItemAdded
                && e.ListChangedType != ListChangedType.ItemDeleted)
                return;

            try
            {
                RefreshKnowledgeSkillLabels(MyToken);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private void UnbindSkillsTabUserControl(CancellationToken token = default)
        {
            if (_objCharacter?.IsDisposed == false)
            {
                try
                {
                    using (_objCharacter.SkillsSection.LockObject.EnterWriteLock(token))
                    {
                        _objCharacter.SkillsSection.Skills.ListChangedAsync -= SkillsOnListChanged;
                        _objCharacter.SkillsSection.SkillGroups.ListChanged -= SkillGroupsOnListChanged;
                        _objCharacter.SkillsSection.KnowledgeSkills.ListChanged -= KnowledgeSkillsOnListChanged;
                        _objCharacter.SkillsSection.MultiplePropertiesChangedAsync -= SkillsSectionOnPropertyChanged;
                    }
                }
                catch (ObjectDisposedException)
                {
                    //swallow this
                }
            }
        }

        private static async Task<List<Tuple<string, IComparer<Skill>>>> GenerateSortList(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Tuple<string, IComparer<Skill>>> ret = new List<Tuple<string, IComparer<Skill>>>(9)
            {
                new Tuple<string, IComparer<Skill>>(await LanguageManager.GetStringAsync("Skill_SortAlphabetical", token: token).ConfigureAwait(false),
                    new SkillSorter(SkillsSection.CompareSkills)),
                new Tuple<string, IComparer<Skill>>(await LanguageManager.GetStringAsync("Skill_SortRating", token: token).ConfigureAwait(false),
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
                new Tuple<string, IComparer<Skill>>(await LanguageManager.GetStringAsync("Skill_SortDicepool", token: token).ConfigureAwait(false),
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
                new Tuple<string, IComparer<Skill>>(await LanguageManager.GetStringAsync("Skill_SortLowerDicepool", token: token).ConfigureAwait(false),
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
                new Tuple<string, IComparer<Skill>>(await LanguageManager.GetStringAsync("Skill_SortAttributeValue", token: token).ConfigureAwait(false),
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
                new Tuple<string, IComparer<Skill>>(await LanguageManager.GetStringAsync("Skill_SortAttributeName", token: token).ConfigureAwait(false),
                    new SkillSorter((x, y) =>
                    {
                        int intReturn = string.Compare(x.DisplayAttribute, y.DisplayAttribute, false, GlobalSettings.CultureInfo);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<Skill>>(await LanguageManager.GetStringAsync("Skill_SortGroupName", token: token).ConfigureAwait(false),
                    new SkillSorter((x, y) =>
                    {
                        int intReturn = SkillsSection.CompareSkillGroups(x.SkillGroupObject, y.SkillGroupObject);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<Skill>>(await LanguageManager.GetStringAsync("Skill_SortGroupRating", token: token).ConfigureAwait(false),
                    new SkillSortBySkillGroup()),
                new Tuple<string, IComparer<Skill>>(await LanguageManager.GetStringAsync("Skill_SortCategory", token: token).ConfigureAwait(false),
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

        private static async Task<List<Tuple<string, Predicate<Skill>>>> GenerateDropdownFilter(Character objCharacter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Tuple<string, Predicate<Skill>>> ret = new List<Tuple<string, Predicate<Skill>>>(7)
            {
                new Tuple<string, Predicate<Skill>>(await LanguageManager.GetStringAsync("String_Search", token: token).ConfigureAwait(false),
                    null),
                new Tuple<string, Predicate<Skill>>(await LanguageManager.GetStringAsync("String_SkillFilterAll", token: token).ConfigureAwait(false),
                    skill => true),
                new Tuple<string, Predicate<Skill>>(await LanguageManager.GetStringAsync("String_SkillFilterRatingAboveZero", token: token).ConfigureAwait(false),
                    skill => skill.Rating > 0),
                new Tuple<string, Predicate<Skill>>(await LanguageManager.GetStringAsync("String_SkillFilterTotalRatingAboveZero", token: token).ConfigureAwait(false),
                    skill => skill.Pool > 0),
                new Tuple<string, Predicate<Skill>>(await LanguageManager.GetStringAsync("String_SkillFilterRatingZero", token: token).ConfigureAwait(false),
                    skill => skill.Rating == 0),
                new Tuple<string, Predicate<Skill>>(await LanguageManager.GetStringAsync("String_SkillFilterNoSkillGroup", token: token).ConfigureAwait(false),
                    skill => skill.SkillGroup.Length == 0),
                new Tuple<string, Predicate<Skill>>(await LanguageManager.GetStringAsync("String_SkillFilterBrokenSkillGroup", token: token).ConfigureAwait(false),
                    skill => skill.Pool > 0 && (skill.SkillGroup.Length == 0 || (skill.SkillGroupObject != null && skill.Rating > skill.SkillGroupObject.Rating)))
            };
            //TODO: TRANSLATIONS

            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            string strColon = await LanguageManager.GetStringAsync("String_Colon", token: token).ConfigureAwait(false);
            IReadOnlyList<string> lstCustomDataPaths = objCharacter != null
                ? await (await objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetEnabledCustomDataDirectoryPathsAsync(token).ConfigureAwait(false)
                : null;

            string strCategory = await LanguageManager.GetStringAsync("Label_Category", token: token).ConfigureAwait(false);
            foreach (XPathNavigator xmlCategoryNode in (await XmlManager.LoadXPathAsync("skills.xml", lstCustomDataPaths, token: token).ConfigureAwait(false))
                .SelectAndCacheExpression("/chummer/categories/category[@type = \"active\"]", token))
            {
                string strName = xmlCategoryNode.Value;
                if (!string.IsNullOrEmpty(strName))
                    ret.Add(new Tuple<string, Predicate<Skill>>(
                        strCategory + strSpace + (xmlCategoryNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value ?? strName),
                        skill => skill.SkillCategory == strName));
            }

            string strAttributeLabel = await LanguageManager.GetStringAsync("String_ExpenseAttribute", token: token).ConfigureAwait(false);
            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                string strAttributeShort = await LanguageManager.GetStringAsync("String_Attribute" + strAttribute + "Short", GlobalSettings.Language, false, token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strAttributeShort))
                    ret.Add(new Tuple<string, Predicate<Skill>>(strAttributeLabel + strColon + strSpace + strAttributeShort,
                        skill => skill.Attribute == strAttribute));
            }

            string strSkillGroupLabel = await LanguageManager.GetStringAsync("String_ExpenseSkillGroup", token: token).ConfigureAwait(false);
            foreach (XPathNavigator xmlSkillGroupNode in (await XmlManager.LoadXPathAsync("skills.xml", lstCustomDataPaths, token: token).ConfigureAwait(false))
                .SelectAndCacheExpression("/chummer/skillgroups/name", token))
            {
                string strName = xmlSkillGroupNode.Value;
                if (!string.IsNullOrEmpty(strName))
                    ret.Add(new Tuple<string, Predicate<Skill>>(
                        strSkillGroupLabel + strSpace + (xmlSkillGroupNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value ?? strName),
                        skill => skill.SkillGroup == strName));
            }

            return ret;
        }

        private static async Task<List<Tuple<string, IComparer<KnowledgeSkill>>>> GenerateKnowledgeSortList(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Tuple<string, IComparer<KnowledgeSkill>>> ret = new List<Tuple<string, IComparer<KnowledgeSkill>>>(7)
            {
                new Tuple<string, IComparer<KnowledgeSkill>>(await LanguageManager.GetStringAsync("Skill_SortAlphabetical", token: token).ConfigureAwait(false),
                    new KnowledgeSkillSorter(SkillsSection.CompareSkills)),
                new Tuple<string, IComparer<KnowledgeSkill>>(await LanguageManager.GetStringAsync("Skill_SortRating", token: token).ConfigureAwait(false),
                    new KnowledgeSkillSorter((x, y) =>
                    {
                        int intReturn = y.Rating.CompareTo(x.Rating);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<KnowledgeSkill>>(await LanguageManager.GetStringAsync("Skill_SortDicepool", token: token).ConfigureAwait(false),
                    new KnowledgeSkillSorter((x, y) =>
                    {
                        int intReturn = y.Pool.CompareTo(x.Pool);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<KnowledgeSkill>>(await LanguageManager.GetStringAsync("Skill_SortLowerDicepool", token: token).ConfigureAwait(false),
                    new KnowledgeSkillSorter((x, y) =>
                    {
                        int intReturn = x.Pool.CompareTo(y.Pool);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<KnowledgeSkill>>(await LanguageManager.GetStringAsync("Skill_SortAttributeValue", token: token).ConfigureAwait(false),
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
                new Tuple<string, IComparer<KnowledgeSkill>>(await LanguageManager.GetStringAsync("Skill_SortAttributeName", token: token).ConfigureAwait(false),
                    new KnowledgeSkillSorter((x, y) =>
                    {
                        int intReturn = string.Compare(x.DisplayAttribute, y.DisplayAttribute, false, GlobalSettings.CultureInfo);
                        if (intReturn == 0)
                            intReturn = SkillsSection.CompareSkills(x, y);
                        return intReturn;
                    })),
                new Tuple<string, IComparer<KnowledgeSkill>>(await LanguageManager.GetStringAsync("Skill_SortCategory", token: token).ConfigureAwait(false),
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

        private static async Task<List<Tuple<string, Predicate<KnowledgeSkill>>>> GenerateKnowledgeDropdownFilter(Character objCharacter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Tuple<string, Predicate<KnowledgeSkill>>> ret = new List<Tuple<string, Predicate<KnowledgeSkill>>>(5)
            {
                //TODO: Search doesn't play nice with writable name
                new Tuple<string, Predicate<KnowledgeSkill>>(await LanguageManager.GetStringAsync("String_Search", token: token).ConfigureAwait(false),
                    null),
                new Tuple<string, Predicate<KnowledgeSkill>>(await LanguageManager.GetStringAsync("String_KnowledgeSkillFilterAll", token: token).ConfigureAwait(false),
                    skill => true),
                new Tuple<string, Predicate<KnowledgeSkill>>(await LanguageManager.GetStringAsync("String_KnowledgeSkillFilterRatingAboveZero", token: token).ConfigureAwait(false),
                    skill => skill.Rating > 0),
                new Tuple<string, Predicate<KnowledgeSkill>>(await LanguageManager.GetStringAsync("String_KnowledgeSkillFilterTotalRatingAboveZero", token: token).ConfigureAwait(false),
                    skill => skill.Pool > 0),
                new Tuple<string, Predicate<KnowledgeSkill>>(await LanguageManager.GetStringAsync("String_KnowledgeSkillFilterRatingZero", token: token).ConfigureAwait(false),
                    skill => skill.Rating == 0)
            };
            //TODO: TRANSLATIONS

            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            string strColon = await LanguageManager.GetStringAsync("String_Colon", token: token).ConfigureAwait(false);
            IReadOnlyList<string> lstCustomDataPaths = objCharacter != null
                ? await (await objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetEnabledCustomDataDirectoryPathsAsync(token).ConfigureAwait(false)
                : null;

            string strCategory = await LanguageManager.GetStringAsync("Label_Category", token: token).ConfigureAwait(false);
            foreach (XPathNavigator xmlCategoryNode in (await XmlManager.LoadXPathAsync("skills.xml", lstCustomDataPaths, token: token).ConfigureAwait(false))
                .SelectAndCacheExpression("/chummer/categories/category[@type = \"knowledge\"]", token))
            {
                string strName = xmlCategoryNode.Value;
                if (!string.IsNullOrEmpty(strName))
                    ret.Add(new Tuple<string, Predicate<KnowledgeSkill>>(
                        strCategory + strSpace + (xmlCategoryNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value ?? strName),
                        skill => skill.SkillCategory == strName));
            }

            string strAttributeLabel = await LanguageManager.GetStringAsync("String_ExpenseAttribute", token: token).ConfigureAwait(false);
            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                string strAttributeShort = await LanguageManager.GetStringAsync("String_Attribute" + strAttribute + "Short", GlobalSettings.Language, false, token).ConfigureAwait(false);
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
            try
            {
                RefreshSkillGroupLabels(MyToken);
                RefreshSkillLabels(MyToken);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private void Panel2_Resize(object sender, EventArgs e)
        {
            try
            {
                RefreshKnowledgeSkillLabels(MyToken);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void cboDisplayFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (!(await cboDisplayFilter.DoThreadSafeFuncAsync(x => x.SelectedItem, MyToken).ConfigureAwait(false)
                        is Tuple<string, Predicate<Skill>>
                        selectedItem))
                    return;
                if (selectedItem.Item2 == null)
                {
                    await cboDisplayFilter.DoThreadSafeAsync(x =>
                    {
                        x.DropDownStyle = ComboBoxStyle.DropDown;
                        _blnActiveSkillSearchMode = true;
                        x.Text = string.Empty;
                    }, token: MyToken).ConfigureAwait(false);
                }
                else
                {
                    await cboDisplayFilter.DoThreadSafeAsync(x =>
                    {
                        x.DropDownStyle = ComboBoxStyle.DropDownList;
                        _blnActiveSkillSearchMode = false;
                    }, token: MyToken).ConfigureAwait(false);
                    await _lstActiveSkills.DoThreadSafeAsync(x => x.SuspendLayout(), token: MyToken)
                        .ConfigureAwait(false);
                    try
                    {
                        await _lstActiveSkills.FilterAsync(selectedItem.Item2, token: MyToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        await _lstActiveSkills.DoThreadSafeAsync(x => x.ResumeLayout(), token: MyToken)
                            .ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void cboDisplayFilter_TextUpdate(object sender, EventArgs e)
        {
            if (!_blnActiveSkillSearchMode)
                return;
            try
            {
                await _lstActiveSkills.DoThreadSafeAsync(x => x.SuspendLayout(), token: MyToken).ConfigureAwait(false);
                try
                {
                    string strText = await cboDisplayFilter.DoThreadSafeFuncAsync(x => x.Text, token: MyToken).ConfigureAwait(false);
                    await _lstActiveSkills.FilterAsync(
                        skill => GlobalSettings.CultureInfo.CompareInfo.IndexOf(
                            skill.CurrentDisplayName, strText, CompareOptions.IgnoreCase) >= 0,
                        async (skill, token) => GlobalSettings.CultureInfo.CompareInfo.IndexOf(
                            await skill.GetCurrentDisplayNameAsync(token).ConfigureAwait(false), strText, CompareOptions.IgnoreCase) >= 0,
                        true, MyToken).ConfigureAwait(false);
                }
                finally
                {
                    await _lstActiveSkills.DoThreadSafeAsync(x => x.ResumeLayout(), token: MyToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cboSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (!(await cboSort.DoThreadSafeFuncAsync(x => x.SelectedItem, token: MyToken).ConfigureAwait(false) is
                        Tuple<string, IComparer<Skill>> selectedItem))
                    return;
                await _lstActiveSkills.DoThreadSafeAsync(x => x.SuspendLayout(), token: MyToken).ConfigureAwait(false);
                try
                {
                    await _lstActiveSkills.SortAsync(selectedItem.Item2, token: MyToken).ConfigureAwait(false);
                }
                finally
                {
                    await _lstActiveSkills.DoThreadSafeAsync(x => x.ResumeLayout(), token: MyToken)
                        .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
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

                IAsyncDisposable objLocker = await objSkill.LockObject.EnterUpgradeableReadLockAsync(MyToken).ConfigureAwait(false);
                try
                {
                    MyToken.ThrowIfCancellationRequested();
                    // Karma check needs to come after the skill is created to make sure bonus-based modifiers (e.g. JoAT) get applied properly (since they can potentially trigger off of the specific exotic skill target)
                    if (await _objCharacter.GetCreatedAsync(MyToken).ConfigureAwait(false)
                        && await objSkill.GetUpgradeKarmaCostAsync(MyToken).ConfigureAwait(false)
                        > await _objCharacter.GetKarmaAsync(MyToken).ConfigureAwait(false))
                    {
                        await Program.ShowScrollableMessageBoxAsync(await LanguageManager
                            .GetStringAsync("Message_NotEnoughKarma", token: MyToken)
                            .ConfigureAwait(false), token: MyToken).ConfigureAwait(false);
                        await _objCharacter.SkillsSection.Skills.RemoveAsync(objSkill, MyToken)
                            .ConfigureAwait(false);
                        return;
                    }

                    await objSkill.Upgrade(MyToken).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
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
                        form.MyForm.SetDropdownItemsMode(await _objCharacter.SkillsSection.GetMyDefaultKnowledgeSkillsAsync(MyToken).ConfigureAwait(false));
                        if (await form.ShowDialogSafeAsync(_objCharacter, MyToken).ConfigureAwait(false)
                            != DialogResult.OK)
                            return;
                        strSelectedSkill = form.MyForm.SelectedItem;
                    }

                    KnowledgeSkill skill = new KnowledgeSkill(_objCharacter, false);
                    await skill.SetDefaultAttributeAsync("LOG", MyToken).ConfigureAwait(false);
                    await skill.SetWritableNameAsync(strSelectedSkill, MyToken).ConfigureAwait(false);

                    if (await _objCharacter.SkillsSection.GetHasAvailableNativeLanguageSlotsAsync(MyToken)
                                           .ConfigureAwait(false)
                        && (await skill.GetIsLanguageAsync(MyToken).ConfigureAwait(false)
                            || string.IsNullOrEmpty(await skill.GetTypeAsync(MyToken).ConfigureAwait(false))))
                    {
                        DialogResult eDialogResult = await Program.ShowScrollableMessageBoxAsync(this,
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
                            MessageBoxButtons.YesNoCancel, token: MyToken).ConfigureAwait(false);
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
                    KnowledgeSkill skill = new KnowledgeSkill(_objCharacter, false);
                    await skill.SetDefaultAttributeAsync("LOG", MyToken).ConfigureAwait(false);
                    await _objCharacter.SkillsSection.KnowledgeSkills.AddAsync(skill, MyToken).ConfigureAwait(false);
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

        private async void cboSortKnowledge_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (!(await cboSortKnowledge.DoThreadSafeFuncAsync(x => x.SelectedItem, token: MyToken).ConfigureAwait(false) is
                        Tuple<string, IComparer<KnowledgeSkill>> selectedItem))
                    return;
                await _lstKnowledgeSkills.DoThreadSafeAsync(x => x.SuspendLayout(), token: MyToken).ConfigureAwait(false);
                try
                {
                    await _lstKnowledgeSkills.SortAsync(selectedItem.Item2, token: MyToken).ConfigureAwait(false);
                }
                finally
                {
                    await _lstKnowledgeSkills.DoThreadSafeAsync(x => x.ResumeLayout(), token: MyToken)
                        .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cboDisplayFilterKnowledge_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (!(await cboDisplayFilterKnowledge.DoThreadSafeFuncAsync(x => x.SelectedItem, MyToken).ConfigureAwait(false)
                        is Tuple<string, Predicate<KnowledgeSkill>>
                        selectedItem))
                    return;
                if (selectedItem.Item2 == null)
                {
                    await cboDisplayFilterKnowledge.DoThreadSafeAsync(x =>
                    {
                        x.DropDownStyle = ComboBoxStyle.DropDown;
                        _blnKnowledgeSkillSearchMode = true;
                        x.Text = string.Empty;
                    }, token: MyToken).ConfigureAwait(false);
                }
                else
                {
                    await cboDisplayFilterKnowledge.DoThreadSafeAsync(x =>
                    {
                        x.DropDownStyle = ComboBoxStyle.DropDownList;
                        _blnKnowledgeSkillSearchMode = false;
                    }, token: MyToken).ConfigureAwait(false);
                    await _lstKnowledgeSkills.DoThreadSafeAsync(x => x.SuspendLayout(), token: MyToken)
                        .ConfigureAwait(false);
                    try
                    {
                        await _lstKnowledgeSkills.FilterAsync(selectedItem.Item2, token: MyToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        await _lstKnowledgeSkills.DoThreadSafeAsync(x => x.ResumeLayout(), token: MyToken)
                            .ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }

        private async void cboDisplayFilterKnowledge_TextUpdate(object sender, EventArgs e)
        {
            if (!_blnKnowledgeSkillSearchMode)
                return;
            try
            {
                await _lstKnowledgeSkills.DoThreadSafeAsync(x => x.SuspendLayout(), token: MyToken).ConfigureAwait(false);
                try
                {
                    string strText = await cboDisplayFilterKnowledge.DoThreadSafeFuncAsync(x => x.Text, token: MyToken).ConfigureAwait(false);
                    await _lstKnowledgeSkills.FilterAsync(
                        skill => GlobalSettings.CultureInfo.CompareInfo.IndexOf(
                            skill.CurrentDisplayName, strText, CompareOptions.IgnoreCase) >= 0,
                        async (skill, token) => GlobalSettings.CultureInfo.CompareInfo.IndexOf(
                            await skill.GetCurrentDisplayNameAsync(token).ConfigureAwait(false), strText, CompareOptions.IgnoreCase) >= 0,
                        true, MyToken).ConfigureAwait(false);
                }
                finally
                {
                    await _lstKnowledgeSkills.DoThreadSafeAsync(x => x.ResumeLayout(), token: MyToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void splitSkills_Resize(object sender, EventArgs e)
        {
            try
            {
                RefreshSkillGroupLabels(MyToken);
                RefreshSkillLabels(MyToken);
                RefreshKnowledgeSkillLabels(MyToken);
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
        }
    }
}
