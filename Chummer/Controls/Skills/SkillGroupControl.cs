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
        private readonly SkillGroup _skillGroup;

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
            Disposed += (sender, args) => UnbindSkillGroupControl();
            //This is apparently a factor 30 faster than placed in load. NFI why
            Stopwatch sw = Stopwatch.StartNew();
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
                        ImageDpi96 = Resources.add_16,
                        ImageDpi192 = Resources.add_32,
                        MinimumSize = new Size(24, 24),
                        Name = "btnCareerIncrease",
                        Padding = new Padding(1),
                        UseVisualStyleBackColor = true
                    };
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
                        Maximum = new decimal(new[] { 99, 0, 0, 0 }),
                        Name = "nudKarma"
                    };
                    nudSkill = new NumericUpDownEx
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        InterceptMouseWheel = GlobalSettings.InterceptMode,
                        Margin = new Padding(3, 2, 3, 2),
                        Maximum = new decimal(new[] { 99, 0, 0, 0 }),
                        Name = "nudSkill"
                    };
                    tlpMain.Controls.Add(nudSkill, 2, 0);
                    tlpMain.Controls.Add(nudKarma, 3, 0);
                }

                DoDataBindings();

                this.UpdateLightDarkMode(token: objMyToken);
                this.TranslateWinForm(blnDoResumeLayout: false, token: objMyToken);
            }
            finally
            {
                tlpMain.ResumeLayout();
                ResumeLayout(true);
            }
            sw.TaskEnd("Create skillgroup");
        }

        private void DoDataBindings()
        {
            try
            {
                lblName.RegisterOneWayAsyncDataBinding((x, y) => x.Text = y, _skillGroup,
                                                       nameof(SkillGroup.CurrentDisplayName),
                                                       x => x.GetCurrentDisplayNameAsync(_objMyToken)
                                                             .AsTask(),
                                                       _objMyToken, _objMyToken);
                lblName.RegisterOneWayAsyncDataBinding((x, y) => x.ToolTipText = y, _skillGroup,
                                                       nameof(SkillGroup.ToolTip),
                                                       x => x.GetToolTipAsync(_objMyToken).AsTask(),
                                                       _objMyToken,
                                                       _objMyToken);

                // Creating these controls outside of the designer saves on handles
                if (_skillGroup.CharacterObject.Created)
                {
                    btnCareerIncrease.RegisterOneWayAsyncDataBinding(
                        (x, y) => x.Enabled = y, _skillGroup,
                        nameof(SkillGroup.CareerCanIncrease),
                        x => x.GetCareerCanIncreaseAsync(_objMyToken).AsTask(), _objMyToken,
                        _objMyToken);
                    btnCareerIncrease.RegisterOneWayAsyncDataBinding(
                        (x, y) => x.ToolTipText = y, _skillGroup,
                        nameof(SkillGroup.UpgradeToolTip),
                        x => x.GetUpgradeToolTipAsync(_objMyToken).AsTask(), _objMyToken,
                        _objMyToken);
                    lblGroupRating.RegisterOneWayAsyncDataBinding((x, y) => x.Text = y, _skillGroup,
                                                                  nameof(SkillGroup.DisplayRating),
                                                                  x => x.GetDisplayRatingAsync(_objMyToken)
                                                                        .AsTask(), _objMyToken, _objMyToken);
                }
                else
                {
                    nudSkill.RegisterOneWayAsyncDataBinding((x, y) => x.Visible = y,
                                                            _skillGroup.CharacterObject,
                                                            nameof(Character
                                                                       .EffectiveBuildMethodUsesPriorityTables),
                                                            x => x
                                                                 .GetEffectiveBuildMethodUsesPriorityTablesAsync(
                                                                     _objMyToken).AsTask(), _objMyToken,
                                                            _objMyToken);
                    nudSkill.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _skillGroup,
                                                            nameof(SkillGroup.BaseUnbroken),
                                                            x => x.GetBaseUnbrokenAsync(_objMyToken).AsTask(),
                                                            _objMyToken, _objMyToken);
                    nudKarma.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _skillGroup,
                                                            nameof(SkillGroup.KarmaUnbroken),
                                                            x => x.GetKarmaUnbrokenAsync(_objMyToken)
                                                                  .AsTask(),
                                                            _objMyToken, _objMyToken);

                    nudKarma.DoDataBinding("Value", _skillGroup, nameof(SkillGroup.Karma), _objMyToken);
                    nudSkill.DoDataBinding("Value", _skillGroup, nameof(SkillGroup.Base), _objMyToken);
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
                await lblName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _skillGroup,
                                                             nameof(SkillGroup.CurrentDisplayName),
                                                             x => x.GetCurrentDisplayNameAsync(_objMyToken)
                                                                   .AsTask(),
                                                             _objMyToken, _objMyToken).ConfigureAwait(false);
                await lblName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.ToolTipText = y, _skillGroup,
                                                             nameof(SkillGroup.ToolTip),
                                                             x => x.GetToolTipAsync(_objMyToken).AsTask(),
                                                             _objMyToken,
                                                             _objMyToken).ConfigureAwait(false);

                // Creating these controls outside of the designer saves on handles
                if (await _skillGroup.CharacterObject.GetCreatedAsync(_objMyToken).ConfigureAwait(false))
                {
                    await btnCareerIncrease.RegisterOneWayAsyncDataBindingAsync(
                                               (x, y) => x.Enabled = y, _skillGroup,
                                               nameof(SkillGroup.CareerCanIncrease),
                                               x => x.GetCareerCanIncreaseAsync(_objMyToken).AsTask(), _objMyToken,
                                               _objMyToken)
                                           .ConfigureAwait(false);
                    await btnCareerIncrease.RegisterOneWayAsyncDataBindingAsync(
                                               (x, y) => x.ToolTipText = y, _skillGroup,
                                               nameof(SkillGroup.UpgradeToolTip),
                                               x => x.GetUpgradeToolTipAsync(_objMyToken).AsTask(), _objMyToken,
                                               _objMyToken)
                                           .ConfigureAwait(false);
                    await lblGroupRating.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _skillGroup,
                                                                        nameof(SkillGroup.DisplayRating),
                                                                        x => x.GetDisplayRatingAsync(_objMyToken)
                                                                              .AsTask(), _objMyToken, _objMyToken)
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
                                                                           _objMyToken).AsTask(), _objMyToken,
                                                                  _objMyToken)
                                  .ConfigureAwait(false);
                    await nudSkill.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _skillGroup,
                                                                  nameof(SkillGroup.BaseUnbroken),
                                                                  x => x.GetBaseUnbrokenAsync(_objMyToken).AsTask(),
                                                                  _objMyToken, _objMyToken).ConfigureAwait(false);
                    await nudKarma.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _skillGroup,
                                                                  nameof(SkillGroup.KarmaUnbroken),
                                                                  x => x.GetKarmaUnbrokenAsync(_objMyToken)
                                                                        .AsTask(),
                                                                  _objMyToken, _objMyToken).ConfigureAwait(false);

                    await nudKarma.DoDataBindingAsync("Value", _skillGroup, nameof(SkillGroup.Karma), _objMyToken)
                                  .ConfigureAwait(false);
                    await nudSkill.DoDataBindingAsync("Value", _skillGroup, nameof(SkillGroup.Base), _objMyToken)
                                  .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void SkillGroupControl_Load(object sender, EventArgs e)
        {
            AdjustForDpi();
        }

        public void UnbindSkillGroupControl()
        {
            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        #region Control Events

        private async void btnCareerIncrease_Click(object sender, EventArgs e)
        {
            try
            {
                using (await EnterReadLock.EnterAsync(_skillGroup.LockObject, _objMyToken).ConfigureAwait(false))
                {
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
            lblName.DoThreadSafe(x => x.MinimumSize = new Size(intNameWidth, x.MinimumSize.Height));
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
    }
}
