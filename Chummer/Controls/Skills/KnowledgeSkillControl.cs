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
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer.Annotations;
using Chummer.Backend.Skills;
using Chummer.Properties;
using Timer = System.Windows.Forms.Timer;

namespace Chummer.UI.Skills
{
    public sealed partial class KnowledgeSkillControl : UserControl
    {
        private int _intUpdatingName = 1;
        private int _intUpdatingSpec = 1;
        private readonly KnowledgeSkill _objSkill;
        private readonly Timer _tmrNameChangeTimer;
        private readonly Timer _tmrSpecChangeTimer;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly NumericUpDownEx nudKarma;

        private readonly NumericUpDownEx nudSkill;
        private readonly Label lblRating;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ButtonWithToolTip btnCareerIncrease;

        private readonly ColorableCheckBox chkNativeLanguage;
        private readonly ElasticComboBox cboSpec;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ColorableCheckBox chkKarma;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Label lblSpec;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ButtonWithToolTip btnAddSpec;

        private readonly CancellationToken _objMyToken;

        public KnowledgeSkillControl(KnowledgeSkill objSkill, CancellationToken objMyToken = default)
        {
            if (objSkill == null)
                return;
            _objMyToken = objMyToken;
            _objSkill = objSkill;
            InitializeComponent();
            Disposed += (sender, args) => UnbindKnowledgeSkillControl();
            SuspendLayout();
            tlpMain.SuspendLayout();
            tlpMiddle.SuspendLayout();
            try
            {
                // To make sure that the initial load formats the name column properly, we need to set the attribute name in the constructor and then make it invisible
                lblName.Text = _objSkill.WritableName;
                if (_objSkill.AllowNameChange)
                    lblName.Visible = false;
                else
                    cboName.Visible = false;
                _tmrNameChangeTimer = new Timer { Interval = 1000 };
                _tmrNameChangeTimer.Tick += NameChangeTimer_Tick;

                if (objSkill.CharacterObject.Created)
                {
                    lblRating = new Label
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        Name = "lblRating",
                        Text = "00",
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    btnCareerIncrease = new ButtonWithToolTip
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        MinimumSize = new Size(24, 24),
                        Name = "btnCareerIncrease",
                        Padding = new Padding(1),
                        UseVisualStyleBackColor = true
                    };
                    btnCareerIncrease.BatchSetImages(Resources.add_16, Resources.add_20, Resources.add_24,
                        Resources.add_32, Resources.add_48, Resources.add_64);
                    btnCareerIncrease.Click += btnCareerIncrease_Click;

                    tlpMain.Controls.Add(lblRating, 1, 0);
                    tlpMain.Controls.Add(btnCareerIncrease, 2, 0);

                    lblSpec = new Label
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        Name = "lblSpec",
                        Text = "[SPEC]",
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    btnAddSpec = new ButtonWithToolTip
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        MinimumSize = new Size(24, 24),
                        Name = "btnAddSpec",
                        Padding = new Padding(1),
                        UseVisualStyleBackColor = true
                    };
                    btnAddSpec.BatchSetImages(Resources.add_16, Resources.add_20, Resources.add_24,
                        Resources.add_32, Resources.add_48, Resources.add_64);
                    btnAddSpec.Click += btnAddSpec_Click;

                    tlpMiddle.SetColumnSpan(lblSpec, 2);
                    tlpMiddle.Controls.Add(lblSpec, 1, 0);
                    tlpMiddle.Controls.Add(btnAddSpec, 3, 0);
                }
                else
                {
                    nudSkill = new NumericUpDownEx
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        InterceptMouseWheel = GlobalSettings.InterceptMode,
                        Margin = new Padding(3, 2, 3, 2),
                        Maximum = new decimal(new[] { 99, 0, 0, 0 }),
                        Name = "nudSkill"
                    };
                    nudKarma = new NumericUpDownEx
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        InterceptMouseWheel = GlobalSettings.InterceptMode,
                        Margin = new Padding(3, 2, 3, 2),
                        Maximum = new decimal(new[] { 99, 0, 0, 0 }),
                        Name = "nudKarma"
                    };

                    tlpMain.Controls.Add(nudSkill, 1, 0);
                    tlpMain.Controls.Add(nudKarma, 2, 0);

                    chkNativeLanguage = new ColorableCheckBox
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        DefaultColorScheme = true,
                        Margin = new Padding(3, 4, 3, 4),
                        Name = "chkNativeLanguage",
                        Tag = "Skill_NativeLanguageLong",
                        Text = "Native",
                        UseVisualStyleBackColor = true
                    };
                    cboSpec = new ElasticComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        AutoCompleteMode = AutoCompleteMode.Suggest,
                        FormattingEnabled = true,
                        Margin = new Padding(3, 0, 3, 0),
                        Name = "cboSpec",
                        TabStop = false
                    };
                    cboSpec.TextChanged += cboSpec_TextChanged;
                    chkKarma = new ColorableCheckBox
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        DefaultColorScheme = true,
                        Margin = new Padding(3, 4, 3, 4),
                        Name = "chkKarma",
                        UseVisualStyleBackColor = true
                    };
                    Interlocked.Decrement(ref _intUpdatingSpec);
                    _tmrSpecChangeTimer = new Timer { Interval = 1000 };
                    _tmrSpecChangeTimer.Tick += SpecChangeTimer_Tick;

                    tlpMiddle.Controls.Add(chkNativeLanguage, 1, 0);
                    tlpMiddle.Controls.Add(cboSpec, 2, 0);
                    tlpMiddle.Controls.Add(chkKarma, 3, 0);
                }

                DoDataBindings();

                this.UpdateLightDarkMode(token: objMyToken);
                this.TranslateWinForm(blnDoResumeLayout: false, token: objMyToken);
            }
            finally
            {
                tlpMiddle.ResumeLayout();
                tlpMain.ResumeLayout();
                ResumeLayout(true);
            }
            using (objSkill.LockObject.EnterWriteLock(objMyToken))
                objSkill.PropertyChanged += Skill_PropertyChanged;
            using (objSkill.CharacterObject.SkillsSection.LockObject.EnterWriteLock(objMyToken))
                objSkill.CharacterObject.SkillsSection.PropertyChanged += OnSkillsSectionPropertyChanged;
            Interlocked.Decrement(ref _intUpdatingName);
            Interlocked.Decrement(ref _intUpdatingSpec);
        }

        private void DoDataBindings()
        {
            try
            {
                lblModifiedRating.RegisterOneWayAsyncDataBinding((x, y) => x.Text = y, _objSkill,
                                                                           nameof(KnowledgeSkill.DisplayPool),
                                                                           x => x.GetDisplayPoolAsync(_objMyToken),
                                                                           _objMyToken, _objMyToken);
                lblModifiedRating.RegisterOneWayAsyncDataBinding((x, y) => x.ToolTipText = y, _objSkill,
                                                                       nameof(KnowledgeSkill.PoolToolTip),
                                                                       x => x.GetPoolToolTipAsync(_objMyToken)
                                                                           .AsTask(),
                                                                       _objMyToken, _objMyToken);

                cmdDelete.RegisterOneWayAsyncDataBinding((x, y) => x.Visible = y, _objSkill,
                                                               nameof(KnowledgeSkill.AllowDelete),
                                                               x => x.GetAllowDeleteAsync(_objMyToken).AsTask(),
                                                               _objMyToken, _objMyToken);

                cboType.PopulateWithListItems(_objSkill.CharacterObject.SkillsSection.MyKnowledgeTypes,
                                                   token: _objMyToken);
                cboType.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                             nameof(KnowledgeSkill.AllowTypeChange),
                                                             x => x.GetAllowTypeChangeAsync(_objMyToken).AsTask(),
                                                             _objMyToken, _objMyToken);
                cboType.DoDataBinding("SelectedValue", _objSkill, nameof(KnowledgeSkill.Type), _objMyToken);

                lblName.RegisterOneWayAsyncDataBinding((x, y) => x.Visible = !y, _objSkill,
                                                             nameof(KnowledgeSkill.AllowNameChange),
                                                             x => x.GetAllowNameChangeAsync(_objMyToken).AsTask(),
                                                             _objMyToken, _objMyToken);
                lblName.RegisterOneWayAsyncDataBinding((x, y) => x.Text = y, _objSkill,
                                                             nameof(KnowledgeSkill.WritableName),
                                                             x => x.GetWritableNameAsync(_objMyToken).AsTask(),
                                                             _objMyToken, _objMyToken);
                lblName.RegisterOneWayAsyncDataBinding((x, y) => x.ForeColor = y, _objSkill,
                                                             nameof(KnowledgeSkill.PreferredColor),
                                                             x => x.GetPreferredColorAsync(_objMyToken).AsTask(),
                                                             _objMyToken, _objMyToken);

                string strWritableName = _objSkill.WritableName;
                Interlocked.Increment(ref _intUpdatingName);
                try
                {
                    cboName.PopulateWithListItems(_objSkill.CharacterObject.SkillsSection.MyDefaultKnowledgeSkills, token: _objMyToken);
                    cboName.DoThreadSafe((x, y) =>
                    {
                        x.SelectedIndex = -1;
                        x.Text = strWritableName;
                    }, _objMyToken);
                }
                finally
                {
                    Interlocked.Decrement(ref _intUpdatingName);
                }

                cboName.RegisterOneWayAsyncDataBinding((x, y) => x.Visible = y, _objSkill,
                                                             nameof(KnowledgeSkill.AllowNameChange),
                                                             x => x.GetAllowNameChangeAsync(_objMyToken).AsTask(),
                                                             _objMyToken, _objMyToken);

                if (_objSkill.CharacterObject.Created)
                {
                    lblRating.RegisterOneWayAsyncDataBinding((x, y) => x.Visible = !y, _objSkill,
                                                                   nameof(KnowledgeSkill.IsNativeLanguage),
                                                                   x => x.GetIsNativeLanguageAsync(_objMyToken)
                                                                         .AsTask(),
                                                                   _objMyToken, _objMyToken);
                    lblRating.RegisterOneWayAsyncDataBinding(
                        (x, y) => x.Text = y.ToString(GlobalSettings.CultureInfo), _objSkill,
                        nameof(KnowledgeSkill.Rating),
                        x => x.GetRatingAsync(_objMyToken).AsTask(),
                        _objMyToken, _objMyToken);

                    btnCareerIncrease.RegisterOneWayAsyncDataBinding((x, y) => x.Visible = y, _objSkill,
                                                                           nameof(KnowledgeSkill.AllowUpgrade),
                                                                           x => x.GetAllowUpgradeAsync(_objMyToken)
                                                                               .AsTask(),
                                                                           _objMyToken, _objMyToken);
                    btnCareerIncrease.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                                           nameof(KnowledgeSkill.CanUpgradeCareer),
                                                                           x => x.GetCanUpgradeCareerAsync(
                                                                                   _objMyToken)
                                                                               .AsTask(),
                                                                           _objMyToken, _objMyToken);
                    btnCareerIncrease.RegisterOneWayAsyncDataBinding((x, y) => x.ToolTipText = y, _objSkill,
                                                                           nameof(KnowledgeSkill.UpgradeToolTip),
                                                                           x => x.GetUpgradeToolTipAsync(
                                                                                   _objMyToken)
                                                                               .AsTask(),
                                                                           _objMyToken, _objMyToken);

                    lblSpec.RegisterOneWayAsyncDataBinding((x, y) => x.Text = y, _objSkill,
                                                                 nameof(KnowledgeSkill
                                                                            .CurrentDisplaySpecialization),
                                                                 x => x.GetCurrentDisplaySpecializationAsync(
                                                                           _objMyToken)
                                                                       .AsTask(),
                                                                 _objMyToken, _objMyToken);

                    btnAddSpec.RegisterOneWayAsyncDataBinding((x, y) => x.Visible = y, _objSkill,
                                                                    nameof(KnowledgeSkill.CanHaveSpecs),
                                                                    x => x.GetCanHaveSpecsAsync(_objMyToken)
                                                                          .AsTask(),
                                                                    _objMyToken, _objMyToken);
                    btnAddSpec.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                                    nameof(KnowledgeSkill.CanAffordSpecialization),
                                                                    x => x
                                                                         .GetCanAffordSpecializationAsync(
                                                                             _objMyToken)
                                                                         .AsTask(),
                                                                    _objMyToken, _objMyToken);
                    btnAddSpec.RegisterOneWayAsyncDataBinding((x, y) => x.ToolTipText = y, _objSkill,
                                                                    nameof(KnowledgeSkill.AddSpecToolTip),
                                                                    x => x.GetAddSpecToolTipAsync(_objMyToken)
                                                                          .AsTask(),
                                                                    _objMyToken, _objMyToken);
                }
                else
                {
                    nudSkill.RegisterOneWayAsyncDataBinding((x, y) => x.Visible = y,
                                                                  _objSkill.CharacterObject.SkillsSection,
                                                                  nameof(SkillsSection.HasKnowledgePoints),
                                                                  x => x.GetHasKnowledgePointsAsync(
                                                                      _objMyToken).AsTask(), _objMyToken,
                                                                  _objMyToken);
                    nudSkill.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                                  nameof(KnowledgeSkill.AllowUpgrade),
                                                                  x => x.GetAllowUpgradeAsync(_objMyToken).AsTask(),
                                                                  _objMyToken, _objMyToken);
                    nudKarma.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                                  nameof(KnowledgeSkill.AllowUpgrade),
                                                                  x => x.GetAllowUpgradeAsync(_objMyToken).AsTask(),
                                                                  _objMyToken, _objMyToken);

                    chkNativeLanguage.RegisterOneWayAsyncDataBinding((x, y) => x.Visible = y, _objSkill,
                                                                           nameof(KnowledgeSkill.IsLanguage),
                                                                           x => x.GetIsLanguageAsync(_objMyToken)
                                                                               .AsTask(),
                                                                           _objMyToken, _objMyToken);
                    bool blnEnableNative
                        = _objSkill.IsNativeLanguage
                          || _objSkill.CharacterObject.SkillsSection.HasAvailableNativeLanguageSlots;
                    chkNativeLanguage.DoThreadSafe((x, y) => x.Enabled = blnEnableNative, _objMyToken);

                    chkKarma.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                                  nameof(KnowledgeSkill.CanHaveSpecs),
                                                                  x => x.GetCanHaveSpecsAsync(_objMyToken).AsTask(),
                                                                  _objMyToken, _objMyToken);

                    cboSpec.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                           nameof(KnowledgeSkill.CanHaveSpecs),
                                                           x => x.GetCanHaveSpecsAsync(_objMyToken).AsTask(),
                                                           _objMyToken, _objMyToken);
                    string strDisplaySpec = _objSkill.CurrentDisplaySpecialization;
                    Interlocked.Increment(ref _intUpdatingSpec);
                    try
                    {
                        cboSpec.PopulateWithListItems(_objSkill.CGLSpecializations, token: _objMyToken);
                        cboSpec.DoThreadSafe((x, y) => x.Text = strDisplaySpec, token: _objMyToken);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intUpdatingSpec);
                    }

                    nudSkill.DoDataBinding("Value", _objSkill, nameof(Skill.Base), _objMyToken);
                    nudKarma.DoDataBinding("Value", _objSkill, nameof(Skill.Karma), _objMyToken);
                    chkNativeLanguage.DoDataBinding("Checked", _objSkill, nameof(Skill.IsNativeLanguage), _objMyToken);
                    chkKarma.DoDataBinding("Checked", _objSkill, nameof(Skill.BuyWithKarma), _objMyToken);
                }

                if (_objSkill.ForcedName)
                {
                    this.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                              nameof(KnowledgeSkill.Enabled),
                                                              x => x.GetEnabledAsync(_objMyToken).AsTask(),
                                                              _objMyToken, _objMyToken);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task DoDataBindingsAsync()
        {
            try
            {
                await lblModifiedRating.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objSkill,
                                                                           nameof(KnowledgeSkill.DisplayPool),
                                                                           x => x.GetDisplayPoolAsync(_objMyToken),
                                                                           _objMyToken, _objMyToken)
                                           .ConfigureAwait(false);
                await lblModifiedRating.RegisterOneWayAsyncDataBindingAsync((x, y) => x.ToolTipText = y, _objSkill,
                                                                       nameof(KnowledgeSkill.PoolToolTip),
                                                                       x => x.GetPoolToolTipAsync(_objMyToken)
                                                                           .AsTask(),
                                                                       _objMyToken, _objMyToken)
                                       .ConfigureAwait(false);

                await cmdDelete.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objSkill,
                                                               nameof(KnowledgeSkill.AllowDelete),
                                                               x => x.GetAllowDeleteAsync(_objMyToken).AsTask(),
                                                               _objMyToken, _objMyToken).ConfigureAwait(false);

                await cboType
                      .PopulateWithListItemsAsync(_objSkill.CharacterObject.SkillsSection.MyKnowledgeTypes,
                                                  token: _objMyToken).ConfigureAwait(false);
                await cboType.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                             nameof(KnowledgeSkill.AllowTypeChange),
                                                             x => x.GetAllowTypeChangeAsync(_objMyToken).AsTask(),
                                                             _objMyToken, _objMyToken).ConfigureAwait(false);
                await cboType
                      .DoDataBindingAsync("SelectedValue", _objSkill, nameof(KnowledgeSkill.Type), _objMyToken)
                      .ConfigureAwait(false);

                await lblName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = !y, _objSkill,
                                                             nameof(KnowledgeSkill.AllowNameChange),
                                                             x => x.GetAllowNameChangeAsync(_objMyToken).AsTask(),
                                                             _objMyToken, _objMyToken).ConfigureAwait(false);
                await lblName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objSkill,
                                                             nameof(KnowledgeSkill.WritableName),
                                                             x => x.GetWritableNameAsync(_objMyToken).AsTask(),
                                                             _objMyToken, _objMyToken).ConfigureAwait(false);
                await lblName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.ForeColor = y, _objSkill,
                                                             nameof(KnowledgeSkill.PreferredColor),
                                                             x => x.GetPreferredColorAsync(_objMyToken).AsTask(),
                                                             _objMyToken, _objMyToken).ConfigureAwait(false);

                string strWritableName = await _objSkill.GetWritableNameAsync(_objMyToken).ConfigureAwait(false);
                Interlocked.Increment(ref _intUpdatingName);
                try
                {
                    await cboName
                          .PopulateWithListItemsAsync(_objSkill.CharacterObject.SkillsSection.MyDefaultKnowledgeSkills,
                                                      token: _objMyToken).ConfigureAwait(false);
                    await cboName.DoThreadSafeAsync(x =>
                    {
                        x.SelectedIndex = -1;
                        x.Text = strWritableName;
                    }, _objMyToken).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Decrement(ref _intUpdatingName);
                }

                await cboName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objSkill,
                                                             nameof(KnowledgeSkill.AllowNameChange),
                                                             x => x.GetAllowNameChangeAsync(_objMyToken).AsTask(),
                                                             _objMyToken, _objMyToken).ConfigureAwait(false);

                if (await _objSkill.CharacterObject.GetCreatedAsync(_objMyToken).ConfigureAwait(false))
                {
                    await lblRating.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = !y, _objSkill,
                                                                   nameof(KnowledgeSkill.IsNativeLanguage),
                                                                   x => x.GetIsNativeLanguageAsync(_objMyToken)
                                                                         .AsTask(),
                                                                   _objMyToken, _objMyToken).ConfigureAwait(false);
                    await lblRating.RegisterOneWayAsyncDataBindingAsync(
                        (x, y) => x.Text = y.ToString(GlobalSettings.CultureInfo), _objSkill,
                        nameof(KnowledgeSkill.Rating),
                        x => x.GetRatingAsync(_objMyToken).AsTask(),
                        _objMyToken, _objMyToken).ConfigureAwait(false);

                    await btnCareerIncrease.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objSkill,
                                                                           nameof(KnowledgeSkill.AllowUpgrade),
                                                                           x => x.GetAllowUpgradeAsync(_objMyToken)
                                                                               .AsTask(),
                                                                           _objMyToken, _objMyToken)
                                           .ConfigureAwait(false);
                    await btnCareerIncrease.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                                           nameof(KnowledgeSkill.CanUpgradeCareer),
                                                                           x => x.GetCanUpgradeCareerAsync(
                                                                                   _objMyToken)
                                                                               .AsTask(),
                                                                           _objMyToken, _objMyToken)
                                           .ConfigureAwait(false);
                    await btnCareerIncrease.RegisterOneWayAsyncDataBindingAsync((x, y) => x.ToolTipText = y, _objSkill,
                                                                           nameof(KnowledgeSkill.UpgradeToolTip),
                                                                           x => x.GetUpgradeToolTipAsync(
                                                                                   _objMyToken)
                                                                               .AsTask(),
                                                                           _objMyToken, _objMyToken)
                                           .ConfigureAwait(false);

                    await lblSpec.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objSkill,
                                                                 nameof(KnowledgeSkill
                                                                            .CurrentDisplaySpecialization),
                                                                 x => x.GetCurrentDisplaySpecializationAsync(
                                                                           _objMyToken)
                                                                       .AsTask(),
                                                                 _objMyToken, _objMyToken).ConfigureAwait(false);

                    await btnAddSpec.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objSkill,
                                                                    nameof(KnowledgeSkill.CanHaveSpecs),
                                                                    x => x.GetCanHaveSpecsAsync(_objMyToken)
                                                                          .AsTask(),
                                                                    _objMyToken, _objMyToken).ConfigureAwait(false);
                    await btnAddSpec.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                                    nameof(KnowledgeSkill.CanAffordSpecialization),
                                                                    x => x
                                                                         .GetCanAffordSpecializationAsync(
                                                                             _objMyToken)
                                                                         .AsTask(),
                                                                    _objMyToken, _objMyToken).ConfigureAwait(false);
                    await btnAddSpec.RegisterOneWayAsyncDataBindingAsync((x, y) => x.ToolTipText = y, _objSkill,
                                                                    nameof(KnowledgeSkill.AddSpecToolTip),
                                                                    x => x.GetAddSpecToolTipAsync(_objMyToken)
                                                                          .AsTask(),
                                                                    _objMyToken, _objMyToken).ConfigureAwait(false);
                }
                else
                {
                    await nudSkill.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y,
                                                                  _objSkill.CharacterObject.SkillsSection,
                                                                  nameof(SkillsSection.HasKnowledgePoints),
                                                                  x => x.GetHasKnowledgePointsAsync(
                                                                      _objMyToken).AsTask(), _objMyToken,
                                                                  _objMyToken)
                                  .ConfigureAwait(false);
                    await nudSkill.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                                  nameof(KnowledgeSkill.AllowUpgrade),
                                                                  x => x.GetAllowUpgradeAsync(_objMyToken).AsTask(),
                                                                  _objMyToken, _objMyToken).ConfigureAwait(false);
                    await nudKarma.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                                  nameof(KnowledgeSkill.AllowUpgrade),
                                                                  x => x.GetAllowUpgradeAsync(_objMyToken).AsTask(),
                                                                  _objMyToken, _objMyToken).ConfigureAwait(false);

                    await chkNativeLanguage.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objSkill,
                                                                           nameof(KnowledgeSkill.IsLanguage),
                                                                           x => x.GetIsLanguageAsync(_objMyToken)
                                                                               .AsTask(),
                                                                           _objMyToken, _objMyToken)
                                           .ConfigureAwait(false);
                    bool blnEnableNative
                        = await _objSkill.GetIsNativeLanguageAsync(_objMyToken).ConfigureAwait(false)
                          || await _objSkill.CharacterObject.SkillsSection
                                            .GetHasAvailableNativeLanguageSlotsAsync(_objMyToken)
                                            .ConfigureAwait(false);
                    await chkNativeLanguage.DoThreadSafeAsync(x => x.Enabled = blnEnableNative, _objMyToken)
                                           .ConfigureAwait(false);

                    await chkKarma.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                                  nameof(KnowledgeSkill.CanHaveSpecs),
                                                                  x => x.GetCanHaveSpecsAsync(_objMyToken).AsTask(),
                                                                  _objMyToken, _objMyToken).ConfigureAwait(false);

                    await cboSpec.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                                      nameof(KnowledgeSkill.CanHaveSpecs),
                                                                      x => x.GetCanHaveSpecsAsync(_objMyToken).AsTask(),
                                                                      _objMyToken, _objMyToken).ConfigureAwait(false);
                    string strDisplaySpec = await _objSkill.GetCurrentDisplaySpecializationAsync(_objMyToken)
                                                           .ConfigureAwait(false);
                    Interlocked.Increment(ref _intUpdatingSpec);
                    try
                    {
                        await cboSpec.PopulateWithListItemsAsync(
                                         await _objSkill.GetCGLSpecializationsAsync(_objMyToken).ConfigureAwait(false),
                                         token: _objMyToken)
                                     .ConfigureAwait(false);
                        await cboSpec.DoThreadSafeAsync(x => x.Text = strDisplaySpec, token: _objMyToken)
                                     .ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intUpdatingSpec);
                    }

                    await nudSkill.DoDataBindingAsync("Value", _objSkill, nameof(Skill.Base), _objMyToken)
                                  .ConfigureAwait(false);
                    await nudKarma.DoDataBindingAsync("Value", _objSkill, nameof(Skill.Karma), _objMyToken)
                                  .ConfigureAwait(false);
                    await chkNativeLanguage
                          .DoDataBindingAsync("Checked", _objSkill, nameof(Skill.IsNativeLanguage), _objMyToken)
                          .ConfigureAwait(false);
                    await chkKarma.DoDataBindingAsync("Checked", _objSkill, nameof(Skill.BuyWithKarma), _objMyToken)
                                  .ConfigureAwait(false);
                }

                if (_objSkill.ForcedName)
                {
                    await this.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                              nameof(KnowledgeSkill.Enabled),
                                                              x => x.GetEnabledAsync(_objMyToken).AsTask(),
                                                              _objMyToken, _objMyToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void KnowledgeSkillControl_Load(object sender, EventArgs e)
        {
            AdjustForDpi();
        }

        private async void OnSkillsSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == nameof(SkillsSection.HasAvailableNativeLanguageSlots)
                    && chkNativeLanguage != null)
                {
                    bool blnEnabled = await _objSkill.GetIsNativeLanguageAsync(_objMyToken).ConfigureAwait(false)
                                      || await _objSkill.CharacterObject.SkillsSection
                                                        .GetHasAvailableNativeLanguageSlotsAsync(_objMyToken)
                                                        .ConfigureAwait(false);
                    await chkNativeLanguage.DoThreadSafeAsync(x => x.Enabled = blnEnabled, token: _objMyToken)
                                           .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void Skill_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool blnAll = false;
            try
            {
                switch (e?.PropertyName)
                {
                    case null:
                        blnAll = true;
                        goto case nameof(Skill.CGLSpecializations);
                    case nameof(Skill.CGLSpecializations):
                        if (cboSpec != null)
                        {
                            IReadOnlyList<ListItem> lstSpecializations
                                = await _objSkill.GetCGLSpecializationsAsync(_objMyToken).ConfigureAwait(false);
                            string strOldSpec = lstSpecializations.Count != 0
                                ? await cboSpec
                                        .DoThreadSafeFuncAsync(x => x.SelectedItem?.ToString(), token: _objMyToken)
                                        .ConfigureAwait(false)
                                : await cboSpec.DoThreadSafeFuncAsync(x => x.Text, token: _objMyToken)
                                               .ConfigureAwait(false);
                            Interlocked.Increment(ref _intUpdatingSpec);
                            try
                            {
                                await cboSpec.PopulateWithListItemsAsync(lstSpecializations, token: _objMyToken)
                                             .ConfigureAwait(false);
                                await cboSpec.DoThreadSafeAsync(x =>
                                {
                                    if (string.IsNullOrEmpty(strOldSpec))
                                        x.SelectedIndex = -1;
                                    else
                                    {
                                        x.SelectedValue = strOldSpec;
                                        if (x.SelectedIndex == -1)
                                            x.Text = strOldSpec;
                                    }
                                }, token: _objMyToken).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intUpdatingSpec);
                            }
                        }

                        if (blnAll)
                            goto case nameof(KnowledgeSkill.WritableName);
                        break;

                    case nameof(KnowledgeSkill.WritableName):
                        {
                            int intOldUpdating = Interlocked.Increment(ref _intUpdatingName);
                            try
                            {
                                if (intOldUpdating == 0)
                                {
                                    string strWritableName = await _objSkill.GetWritableNameAsync(_objMyToken)
                                                                            .ConfigureAwait(false);
                                    await cboName.DoThreadSafeAsync(x => x.Text = strWritableName, token: _objMyToken)
                                                 .ConfigureAwait(false);
                                }
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intUpdatingName);
                            }

                            if (blnAll)
                                goto case nameof(Skill.TopMostDisplaySpecialization);
                            break;
                        }
                    case nameof(KnowledgeSkill.TopMostDisplaySpecialization):
                        {
                            if (cboSpec != null)
                            {
                                int intOldUpdating = Interlocked.Increment(ref _intUpdatingSpec);
                                try
                                {
                                    if (intOldUpdating == 0)
                                    {
                                        string strDisplaySpec = await _objSkill
                                                                      .GetTopMostDisplaySpecializationAsync(_objMyToken)
                                                                      .ConfigureAwait(false);
                                        await cboSpec.DoThreadSafeAsync(x => x.Text = strDisplaySpec, token: _objMyToken)
                                                     .ConfigureAwait(false);
                                    }
                                }
                                finally
                                {
                                    Interlocked.Decrement(ref _intUpdatingSpec);
                                }
                            }

                            if (blnAll)
                                goto case nameof(Skill.IsNativeLanguage);
                            break;
                        }
                    case nameof(Skill.IsNativeLanguage):
                        if (chkNativeLanguage != null)
                        {
                            bool blnEnabled
                                = await _objSkill.GetIsNativeLanguageAsync(_objMyToken).ConfigureAwait(false)
                                  || await _objSkill.CharacterObject.SkillsSection
                                                    .GetHasAvailableNativeLanguageSlotsAsync(_objMyToken)
                                                    .ConfigureAwait(false);
                            await chkNativeLanguage.DoThreadSafeAsync(x => x.Enabled = blnEnabled, token: _objMyToken)
                                                   .ConfigureAwait(false);
                        }

                        if (blnAll)
                            goto case nameof(Skill.Specializations);
                        break;

                    case nameof(Skill.Specializations):
                        {
                            if (await Program.GetFormForDialogAsync(_objSkill.CharacterObject, _objMyToken)
                                             .ConfigureAwait(false) is CharacterShared frmParent)
                                await frmParent.RequestCharacterUpdate(_objMyToken).ConfigureAwait(false);
                            break;
                        }
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void UnbindKnowledgeSkillControl()
        {
            _tmrNameChangeTimer?.Dispose();
            _tmrSpecChangeTimer?.Dispose();
            try
            {
                using (_objSkill.LockObject.EnterWriteLock())
                    _objSkill.PropertyChanged -= Skill_PropertyChanged;
                using (_objSkill.CharacterObject.SkillsSection.LockObject.EnterWriteLock())
                    _objSkill.CharacterObject.SkillsSection.PropertyChanged -= OnSkillsSectionPropertyChanged;
            }
            catch (ObjectDisposedException)
            {
                // swallow this
            }

            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private async void btnCareerIncrease_Click(object sender, EventArgs e)
        {
            try
            {
                using (await EnterReadLock.EnterAsync(_objSkill.LockObject, _objMyToken).ConfigureAwait(false))
                {
                    int intKarmaCost = await _objSkill.GetUpgradeKarmaCostAsync(_objMyToken).ConfigureAwait(false);

                    if (intKarmaCost == -1)
                        return; //TODO: more descriptive
                    string strConfirm = string.Format(GlobalSettings.CultureInfo,
                                                      await LanguageManager
                                                            .GetStringAsync(
                                                                "Message_ConfirmKarmaExpense", token: _objMyToken)
                                                            .ConfigureAwait(false),
                                                      await _objSkill.GetCurrentDisplayNameAsync(_objMyToken)
                                                                     .ConfigureAwait(false),
                                                      await _objSkill.GetRatingAsync(_objMyToken).ConfigureAwait(false)
                                                      + 1,
                                                      intKarmaCost,
                                                      await cboType
                                                            .DoThreadSafeFuncAsync(
                                                                x => x.GetItemText(x.SelectedItem), token: _objMyToken)
                                                            .ConfigureAwait(false));

                    if (!await CommonFunctions.ConfirmKarmaExpenseAsync(strConfirm, _objMyToken).ConfigureAwait(false))
                        return;

                    await _objSkill.Upgrade(_objMyToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void btnAddSpec_Click(object sender, EventArgs e)
        {
            try
            {
                using (await EnterReadLock.EnterAsync(_objSkill.LockObject, _objMyToken).ConfigureAwait(false))
                {
                    int price = _objSkill.CharacterObject.Settings.KarmaKnowledgeSpecialization;

                    decimal decExtraSpecCost = 0;
                    int intTotalBaseRating = _objSkill.TotalBaseRating;
                    decimal decSpecCostMultiplier = 1.0m;
                    bool blnCreated = await _objSkill.CharacterObject.GetCreatedAsync(_objMyToken).ConfigureAwait(false);
                    foreach (Improvement objLoopImprovement in _objSkill.CharacterObject.Improvements)
                    {
                        if (objLoopImprovement.Minimum <= intTotalBaseRating
                            && (string.IsNullOrEmpty(objLoopImprovement.Condition)
                                || (objLoopImprovement.Condition == "career") == blnCreated
                                || (objLoopImprovement.Condition == "create") != blnCreated)
                            && objLoopImprovement.Enabled
                            && objLoopImprovement.ImprovedName == _objSkill.SkillCategory)
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.SkillCategorySpecializationKarmaCost:
                                    decExtraSpecCost += objLoopImprovement.Value;
                                    break;

                                case Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier:
                                    decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
                    }

                    if (decSpecCostMultiplier != 1.0m)
                        price = (price * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
                    else
                        price += decExtraSpecCost.StandardRound(); //Spec

                    string strConfirm = string.Format(GlobalSettings.CultureInfo,
                                                         await LanguageManager
                                                               .GetStringAsync(
                                                                   "Message_ConfirmKarmaExpenseSkillSpecialization", token: _objMyToken)
                                                               .ConfigureAwait(false), price);

                    if (!await CommonFunctions.ConfirmKarmaExpenseAsync(strConfirm, _objMyToken).ConfigureAwait(false))
                        return;

                    using (ThreadSafeForm<SelectSpec> selectForm =
                           await ThreadSafeForm<SelectSpec>.GetAsync(() => new SelectSpec(_objSkill)
                                                                         {Mode = "Knowledge"}, _objMyToken).ConfigureAwait(false))
                    {
                        if (await selectForm.ShowDialogSafeAsync(_objSkill.CharacterObject, _objMyToken).ConfigureAwait(false)
                            != DialogResult.OK)
                            return;
                        await _objSkill.AddSpecialization(selectForm.MyForm.SelectedItem, _objMyToken).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cmdDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (!await _objSkill.GetAllowDeleteAsync(_objMyToken).ConfigureAwait(false))
                    return;
                if (!await CommonFunctions
                           .ConfirmDeleteAsync(
                               await LanguageManager.GetStringAsync("Message_DeleteKnowledgeSkill", token: _objMyToken)
                                                    .ConfigureAwait(false), _objMyToken).ConfigureAwait(false))
                    return;
                await _objSkill.CharacterObject.SkillsSection.KnowledgeSkills.RemoveAsync(_objSkill, _objMyToken)
                               .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        [UsedImplicitly]
        public int NameWidth => tlpLeft.Width -
                                (_objSkill.AllowNameChange
                                    ? cboName.Margin.Left + cboName.Margin.Right
                                    : lblName.Margin.Left + lblName.Margin.Right);

        [UsedImplicitly]
        public int NudSkillWidth =>
            !_objSkill.CharacterObject.Created && _objSkill.CharacterObject.SkillsSection.HasKnowledgePoints
                ? nudSkill.Width
                : 0;

        [UsedImplicitly]
        public int RightButtonsWidth => tlpRight.Width;

        /// <summary>
        /// I'm not super pleased with how this works, but it's functional so w/e.
        /// The goal is for controls to retain the ability to display tooltips even while disabled. IT DOES NOT WORK VERY WELL.
        /// </summary>
        #region ButtonWithToolTip Visibility workaround

        private ButtonWithToolTip _activeButton;

        private ButtonWithToolTip ActiveButton
        {
            get => _activeButton;
            set
            {
                ButtonWithToolTip objOldValue = Interlocked.Exchange(ref _activeButton, value);
                if (objOldValue == value)
                    return;
                objOldValue?.ToolTipObject.Hide(this);
                if (value?.Visible == true)
                {
                    value.ToolTipObject.Show(value.ToolTipText, this);
                }
            }
        }

        private ButtonWithToolTip FindToolTipControl(Point pt)
        {
            return Controls.OfType<ButtonWithToolTip>().FirstOrDefault(c => c.Bounds.Contains(pt));
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            ActiveButton = FindToolTipControl(e.Location);
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            ActiveButton = null;
        }

        #endregion ButtonWithToolTip Visibility workaround

        private void KnowledgeSkillControl_DpiChangedAfterParent(object sender, EventArgs e)
        {
            AdjustForDpi();
        }

        private void AdjustForDpi()
        {
            using (Graphics g = CreateGraphics())
            {
                if (lblRating != null)
                    lblRating.MinimumSize = new Size((int)(25 * g.DpiX / 96.0f), 0);
                lblModifiedRating.MinimumSize = new Size((int)(50 * g.DpiX / 96.0f), 0);
            }
        }

        // Hacky solutions to data binding causing cursor to reset whenever the user is typing something in: have text changes start a timer, and have a 1s delay in the timer update fire the text update
        private void cboName_TextChanged(object sender, EventArgs e)
        {
            if (_tmrNameChangeTimer == null)
                return;
            if (_tmrNameChangeTimer.Enabled)
                _tmrNameChangeTimer.Stop();
            if (_intUpdatingName > 0)
                return;
            _tmrNameChangeTimer.Start();
        }

        private void cboSpec_TextChanged(object sender, EventArgs e)
        {
            if (_tmrSpecChangeTimer == null)
                return;
            if (_tmrSpecChangeTimer.Enabled)
                _tmrSpecChangeTimer.Stop();
            if (_intUpdatingSpec > 0)
                return;
            _tmrSpecChangeTimer.Start();
        }

        private async void NameChangeTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                _tmrNameChangeTimer.Stop();
                Interlocked.Increment(ref _intUpdatingName);
                try
                {
                    string strName = await cboName.DoThreadSafeFuncAsync(x => x.Text, token: _objMyToken)
                                                  .ConfigureAwait(false);
                    await _objSkill.SetWritableNameAsync(strName, _objMyToken).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Decrement(ref _intUpdatingName);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void SpecChangeTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                _tmrSpecChangeTimer.Stop();
                Interlocked.Increment(ref _intUpdatingSpec);
                try
                {
                    string strSpec = await cboSpec.DoThreadSafeFuncAsync(x => x.Text, token: _objMyToken)
                                                  .ConfigureAwait(false);
                    await _objSkill.SetTopMostDisplaySpecializationAsync(strSpec, _objMyToken).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Decrement(ref _intUpdatingSpec);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }
    }
}
