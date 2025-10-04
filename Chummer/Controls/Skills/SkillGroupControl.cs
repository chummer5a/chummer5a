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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer.Backend.Skills;
using Chummer.Properties;

namespace Chummer.UI.Skills
{
    public partial class SkillGroupControl : UserControl
    {
        private SkillGroup _skillGroup;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly NumericUpDownEx nudSkill;

        private readonly NumericUpDownEx nudKarma;
        private readonly Label lblGroupRating;
        private readonly ButtonWithToolTip btnCareerIncrease;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        private readonly CancellationToken _objMyToken;

        public SkillGroupControl(SkillGroup skillGroup, CancellationToken objMyToken = default)
        {
            if (skillGroup == null)
                return;
            _objMyToken = objMyToken;
            _skillGroup = skillGroup;
            InitializeComponent();
            //This is apparently a factor 30 faster than placed in load. NFI why
            using (new FetchSafelyFromSafeObjectPool<Stopwatch>(Utils.StopwatchPool, out Stopwatch sw))
            {
                sw.Start();
                SuspendLayout();
                tlpMain.SuspendLayout();
                try
                {
                    // To make sure that the initial load formats the name column properly, we need to set the attribute name in the constructor
                    lblName.Text = skillGroup.CurrentDisplayName;
                    // Creating these controls outside of the designer saves on handles
                    if (skillGroup.CharacterObject.Created)
                    {
                        lblGroupRating = new Label
                        {
                            Anchor = AnchorStyles.Right,
                            AutoSize = true,
                            Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0),
                            Margin = new Padding(3, 6, 3, 6),
                            Name = "lblGroupRating",
                            TextAlign = ContentAlignment.MiddleRight
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

                        tlpMain.Controls.Add(lblGroupRating, 2, 0);
                        tlpMain.Controls.Add(btnCareerIncrease, 3, 0);
                    }
                    else
                    {
                        nudKarma = new NumericUpDownEx
                        {
                            Anchor = AnchorStyles.Right,
                            AutoSize = true,
                            InterceptMouseWheel = GlobalSettings.InterceptMode,
                            Margin = new Padding(3, 2, 3, 2),
                            Name = "nudKarma"
                        };
                        nudSkill = new NumericUpDownEx
                        {
                            Anchor = AnchorStyles.Right,
                            AutoSize = true,
                            InterceptMouseWheel = GlobalSettings.InterceptMode,
                            Margin = new Padding(3, 2, 3, 2),
                            Name = "nudSkill"
                        };
                        tlpMain.Controls.Add(nudSkill, 2, 0);
                        tlpMain.Controls.Add(nudKarma, 3, 0);
                    }

                    this.UpdateLightDarkMode(token: objMyToken);
                    this.TranslateWinForm(blnDoResumeLayout: false, token: objMyToken);
                    this.UpdateParentForToolTipControls();
                }
                finally
                {
                    tlpMain.ResumeLayout();
                    ResumeLayout(true);
                }

                sw.TaskEnd("Create skillgroup");
            }
        }

        private void DoDataBindings()
        {
            try
            {
                lblName.RegisterOneWayAsyncDataBinding((x, y) => x.Text = y, _skillGroup,
                                                       nameof(SkillGroup.CurrentDisplayName),
                                                       x => x.GetCurrentDisplayNameAsync(_objMyToken)
                                                             ,
                                                       _objMyToken);
                lblName.RegisterOneWayAsyncDataBinding((x, y) => x.ToolTipText = y, _skillGroup,
                                                       nameof(SkillGroup.ToolTip),
                                                       x => x.GetToolTipAsync(_objMyToken),
                                                       _objMyToken);

                // Creating these controls outside of the designer saves on handles
                if (_skillGroup.CharacterObject.Created)
                {
                    btnCareerIncrease.RegisterOneWayAsyncDataBinding(
                        (x, y) => x.Enabled = y, _skillGroup,
                        nameof(SkillGroup.CareerCanIncrease),
                        x => x.GetCareerCanIncreaseAsync(_objMyToken), _objMyToken);
                    btnCareerIncrease.RegisterOneWayAsyncDataBinding(
                        (x, y) => x.ToolTipText = y, _skillGroup,
                        nameof(SkillGroup.UpgradeToolTip),
                        x => x.GetUpgradeToolTipAsync(_objMyToken), _objMyToken);
                    lblGroupRating.RegisterOneWayAsyncDataBinding((x, y) => x.Text = y, _skillGroup,
                                                                  nameof(SkillGroup.DisplayRating),
                                                                  x => x.GetDisplayRatingAsync(_objMyToken)
                                                                        , _objMyToken);
                }
                else
                {
                    nudSkill.RegisterOneWayAsyncDataBinding((x, y) => x.Visible = y,
                                                            _skillGroup.CharacterObject,
                                                            nameof(Character
                                                                       .EffectiveBuildMethodUsesPriorityTables),
                                                            x => x
                                                                 .GetEffectiveBuildMethodUsesPriorityTablesAsync(
                                                                     _objMyToken), _objMyToken);
                    nudSkill.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _skillGroup,
                                                            nameof(SkillGroup.BaseUnbroken),
                                                            x => x.GetBaseUnbrokenAsync(_objMyToken),
                                                            _objMyToken);
                    nudSkill.RegisterOneWayAsyncDataBinding((x, y) => x.Maximum = y, _skillGroup,
                        nameof(SkillGroup.RatingMaximum),
                        x => x.GetRatingMaximumAsync(_objMyToken),
                        _objMyToken);
                    nudKarma.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _skillGroup,
                                                            nameof(SkillGroup.KarmaUnbroken),
                                                            x => x.GetKarmaUnbrokenAsync(_objMyToken)
                                                                  ,
                                                            _objMyToken);
                    nudKarma.RegisterOneWayAsyncDataBinding((x, y) => x.Maximum = y, _skillGroup,
                        nameof(SkillGroup.RatingMaximum),
                        x => x.GetRatingMaximumAsync(_objMyToken),
                        _objMyToken);

                    nudKarma.RegisterAsyncDataBindingWithDelay(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y, _skillGroup,
                        nameof(SkillGroup.Karma),
                        (x, y) => x.ValueChanged += y,
                        x => x.GetKarmaAsync(_objMyToken),
                        (x, y) => x.SetKarmaAsync(y, _objMyToken),
                        250,
                        _objMyToken,
                        _objMyToken);
                    nudSkill.RegisterAsyncDataBindingWithDelay(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y, _skillGroup,
                        nameof(SkillGroup.Base),
                        (x, y) => x.ValueChanged += y,
                        x => x.GetBaseAsync(_objMyToken),
                        (x, y) => x.SetBaseAsync(y, _objMyToken),
                        250,
                        _objMyToken,
                        _objMyToken);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task DoDataBindingsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await lblName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _skillGroup,
                nameof(SkillGroup.CurrentDisplayName),
                x => x.GetCurrentDisplayNameAsync(_objMyToken)
                ,
                token).ConfigureAwait(false);
            await lblName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.ToolTipText = y, _skillGroup,
                nameof(SkillGroup.ToolTip),
                x => x.GetToolTipAsync(_objMyToken),
                token).ConfigureAwait(false);

            // Creating these controls outside of the designer saves on handles
            if (await _skillGroup.CharacterObject.GetCreatedAsync(token).ConfigureAwait(false))
            {
                await btnCareerIncrease.RegisterOneWayAsyncDataBindingAsync(
                        (x, y) => x.Enabled = y, _skillGroup,
                        nameof(SkillGroup.CareerCanIncrease),
                        x => x.GetCareerCanIncreaseAsync(_objMyToken), token)
                    .ConfigureAwait(false);
                await btnCareerIncrease.RegisterOneWayAsyncDataBindingAsync(
                        (x, y) => x.ToolTipText = y, _skillGroup,
                        nameof(SkillGroup.UpgradeToolTip),
                        x => x.GetUpgradeToolTipAsync(_objMyToken), token)
                    .ConfigureAwait(false);
                await lblGroupRating.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _skillGroup,
                        nameof(SkillGroup.DisplayRating),
                        x => x.GetDisplayRatingAsync(_objMyToken)
                        , token)
                    .ConfigureAwait(false);
            }
            else
            {
                await nudSkill.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y,
                        _skillGroup.CharacterObject,
                        nameof(Character
                            .EffectiveBuildMethodUsesPriorityTables),
                        x => x
                            .GetEffectiveBuildMethodUsesPriorityTablesAsync(
                                _objMyToken), token)
                    .ConfigureAwait(false);
                await nudSkill.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _skillGroup,
                    nameof(SkillGroup.BaseUnbroken),
                    x => x.GetBaseUnbrokenAsync(_objMyToken),
                    token).ConfigureAwait(false);
                await nudSkill.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _skillGroup,
                    nameof(SkillGroup.RatingMaximum),
                    x => x.GetRatingMaximumAsync(_objMyToken),
                    _objMyToken).ConfigureAwait(false);
                await nudKarma.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _skillGroup,
                    nameof(SkillGroup.KarmaUnbroken),
                    x => x.GetKarmaUnbrokenAsync(_objMyToken)
                    , token).ConfigureAwait(false);
                await nudKarma.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _skillGroup,
                    nameof(SkillGroup.RatingMaximum),
                    x => x.GetRatingMaximumAsync(_objMyToken),
                    _objMyToken).ConfigureAwait(false);

                await nudKarma.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y, _skillGroup,
                    nameof(SkillGroup.Karma),
                    (x, y) => x.ValueChanged += y,
                    x => x.GetKarmaAsync(_objMyToken),
                    (x, y) => x.SetKarmaAsync(y, _objMyToken),
                    250,
                    _objMyToken,
                    _objMyToken).ConfigureAwait(false);
                await nudSkill.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y, _skillGroup,
                    nameof(SkillGroup.Base),
                    (x, y) => x.ValueChanged += y,
                    x => x.GetBaseAsync(_objMyToken),
                    (x, y) => x.SetBaseAsync(y, _objMyToken),
                    250,
                    _objMyToken,
                    _objMyToken).ConfigureAwait(false);
            }
        }

        private int _intLoaded;

        public async Task DoLoad(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (Interlocked.CompareExchange(ref _intLoaded, 1, 0) > 0)
                return;
            IAsyncDisposable objLocker = await _skillGroup.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await DoDataBindingsAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void SkillGroupControl_Load(object sender, EventArgs e)
        {
            try
            {
                await DoLoad(_objMyToken).ConfigureAwait(false);
                await this.DoThreadSafeAsync(x => x.AdjustForDpi(), token: _objMyToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        public void UnbindSkillGroupControl()
        {
            foreach (Control objControl in Controls)
                objControl.ResetBindings();

            ButtonWithToolTip objOld = Interlocked.Exchange(ref _activeButton, null);
            if (!objOld.IsNullOrDisposed())
                objOld.Dispose();

            _skillGroup = null;
        }

        #region Control Events

        private async void btnCareerIncrease_Click(object sender, EventArgs e)
        {
            try
            {
                IAsyncDisposable objLocker = await _skillGroup.LockObject.EnterUpgradeableReadLockAsync(_objMyToken).ConfigureAwait(false);
                try
                {
                    _objMyToken.ThrowIfCancellationRequested();
                    string strConfirm = string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager
                            .GetStringAsync(
                                "Message_ConfirmKarmaExpense", token: _objMyToken)
                            .ConfigureAwait(false),
                        await _skillGroup.GetCurrentDisplayNameAsync(_objMyToken)
                            .ConfigureAwait(false),
                        await _skillGroup.GetRatingAsync(_objMyToken)
                            .ConfigureAwait(false) + 1,
                        await _skillGroup.GetUpgradeKarmaCostAsync(_objMyToken)
                            .ConfigureAwait(false));

                    if (!await CommonFunctions.ConfirmKarmaExpenseAsync(strConfirm, _objMyToken).ConfigureAwait(false))
                        return;

                    await _skillGroup.Upgrade(_objMyToken).ConfigureAwait(false);
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

        #endregion Control Events

        #region Properties

        public int NameWidth => lblName.PreferredWidth;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Update the position of controls.
        /// </summary>
        /// <param name="intNameWidth">Width of the Name label</param>
        public void MoveControls(int intNameWidth)
        {
            try
            {
                lblName.DoThreadSafe(x => x.MinimumSize = new Size(intNameWidth, x.MinimumSize.Height),
                    token: _objMyToken);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        #endregion Methods

        /// <summary>
        /// I'm not super pleased with how this works, but it's functional so w/e.
        /// The goal is for controls to retain the ability to display tooltips even while disabled. IT DOES NOT WORK VERY WELL.
        /// </summary>

        #region ButtonWithToolTip Visibility workaround

        private ButtonWithToolTip _activeButton;

        protected ButtonWithToolTip ActiveButton
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
            return Controls.OfType<ButtonWithToolTip>().FirstOrDefault(c => c.ClientRectangle.Contains(pt));
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

        private void SkillGroupControl_DpiChangedAfterParent(object sender, EventArgs e)
        {
            AdjustForDpi();
        }

        private void AdjustForDpi()
        {
            if (lblGroupRating == null)
                return;
            using (Graphics g = CreateGraphics())
                lblGroupRating.MinimumSize = new Size((int)(25 * g.DpiX / 96.0f), 0);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            // Note: because we cannot unsubscribe old parents from events if/when we change parents, we do not want to have this automatically update
            // based on a subscription to our parent's ParentChanged (which we would need to be able to automatically update our parent form for nested controls)
            // We therefore need to use the hacky workaround of calling UpdateParentForToolTipControls() for parent forms/controls as appropriate
            this.UpdateParentForToolTipControls();
        }
    }
}
