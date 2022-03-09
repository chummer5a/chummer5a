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
using System.Windows.Forms;
using Chummer.Backend.Skills;
using Chummer.Properties;

namespace Chummer.UI.Skills
{
    public partial class SkillGroupControl : UserControl
    {
        private readonly SkillGroup _skillGroup;

        private readonly NumericUpDownEx nudSkill;
        private readonly NumericUpDownEx nudKarma;
        private readonly Label lblGroupRating;
        private readonly ButtonWithToolTip btnCareerIncrease;

        public SkillGroupControl(SkillGroup skillGroup)
        {
            if (skillGroup == null)
                return;
            _skillGroup = skillGroup;
            InitializeComponent();
            //This is apparently a factor 30 faster than placed in load. NFI why
            Stopwatch sw = Stopwatch.StartNew();
            SuspendLayout();
            tlpMain.SuspendLayout();
            try
            {
                lblName.DoOneWayDataBinding("Text", _skillGroup, nameof(SkillGroup.CurrentDisplayName));
                lblName.DoOneWayDataBinding("ToolTipText", _skillGroup, nameof(SkillGroup.ToolTip));

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
                        ImageDpi96 = Resources.add,
                        ImageDpi192 = Resources.add1,
                        MinimumSize = new Size(24, 24),
                        Name = "btnCareerIncrease",
                        Padding = new Padding(1),
                        UseVisualStyleBackColor = true
                    };
                    btnCareerIncrease.Click += btnCareerIncrease_Click;

                    btnCareerIncrease.DoOneWayDataBinding("Enabled", _skillGroup, nameof(SkillGroup.CareerCanIncrease));
                    btnCareerIncrease.DoOneWayDataBinding("ToolTipText", _skillGroup,
                        nameof(SkillGroup.UpgradeToolTip));

                    lblGroupRating.DoOneWayDataBinding("Text", _skillGroup, nameof(SkillGroup.DisplayRating));

                    tlpMain.Controls.Add(lblGroupRating, 2, 0);
                    tlpMain.Controls.Add(btnCareerIncrease, 3, 0);
                }
                else
                {
                    nudKarma = new NumericUpDownEx
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        InterceptMouseWheel = NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver,
                        Margin = new Padding(3, 2, 3, 2),
                        Maximum = new decimal(new[] { 99, 0, 0, 0 }),
                        Name = "nudKarma"
                    };
                    nudSkill = new NumericUpDownEx
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        InterceptMouseWheel = NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver,
                        Margin = new Padding(3, 2, 3, 2),
                        Maximum = new decimal(new[] { 99, 0, 0, 0 }),
                        Name = "nudSkill"
                    };

                    nudKarma.DoDataBinding("Value", _skillGroup, nameof(SkillGroup.Karma));
                    nudKarma.DoOneWayDataBinding("Enabled", _skillGroup, nameof(SkillGroup.KarmaUnbroken));
                    nudKarma.InterceptMouseWheel = GlobalSettings.InterceptMode;

                    nudSkill.DoOneWayDataBinding("Visible", _skillGroup.CharacterObject,
                        nameof(Character.EffectiveBuildMethodUsesPriorityTables));
                    nudSkill.DoDataBinding("Value", _skillGroup, nameof(SkillGroup.Base));
                    nudSkill.DoOneWayDataBinding("Enabled", _skillGroup, nameof(SkillGroup.BaseUnbroken));
                    nudSkill.InterceptMouseWheel = GlobalSettings.InterceptMode;

                    tlpMain.Controls.Add(nudSkill, 2, 0);
                    tlpMain.Controls.Add(nudKarma, 3, 0);
                }

                AdjustForDpi();
                this.UpdateLightDarkMode();
                this.TranslateWinForm(blnDoResumeLayout: false);
            }
            finally
            {
                tlpMain.ResumeLayout();
                ResumeLayout(true);
            }
            sw.TaskEnd("Create skillgroup");
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
            string confirmstring = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpense"),
                    _skillGroup.CurrentDisplayName, _skillGroup.Rating + 1, _skillGroup.UpgradeKarmaCost);

            if (!CommonFunctions.ConfirmKarmaExpense(confirmstring))
                return;

            _skillGroup.Upgrade();
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
            lblName.MinimumSize = new Size(intNameWidth, lblName.MinimumSize.Height);
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
                if (value == ActiveButton)
                    return;
                ActiveButton?.ToolTipObject.Hide(this);
                _activeButton = value;
                if (ActiveButton?.Visible == true)
                {
                    ActiveButton.ToolTipObject.Show(ActiveButton.ToolTipText, this);
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
