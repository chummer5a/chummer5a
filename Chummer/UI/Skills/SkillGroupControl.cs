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
using System.Windows.Forms;
using Chummer.Backend.Skills;

namespace Chummer.UI.Skills
{
    public partial class SkillGroupControl : UserControl
    {
        private readonly SkillGroup _skillGroup;
        private readonly Graphics _objGraphics;

        public SkillGroupControl(SkillGroup skillGroup)
        {
            if (skillGroup == null)
                return;
            _objGraphics = CreateGraphics();
            _skillGroup = skillGroup;
            InitializeComponent();
            SkillGroupControl_DpiChangedAfterParent(null, EventArgs.Empty);
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            //This is apparently a factor 30 faster than placed in load. NFI why
            Stopwatch sw = Stopwatch.StartNew();
            SuspendLayout();
            lblName.DoOneWayDataBinding("Text", _skillGroup, nameof(SkillGroup.CurrentDisplayName));
            lblName.DoOneWayDataBinding("ToolTipText", _skillGroup, nameof(SkillGroup.ToolTip));

            if (skillGroup.CharacterObject.Created)
            {
                flpRightCreate.Visible = false;

                btnCareerIncrease.DoOneWayDataBinding("Enabled", _skillGroup, nameof(SkillGroup.CareerCanIncrease));
                btnCareerIncrease.DoOneWayDataBinding("ToolTipText", _skillGroup, nameof(SkillGroup.UpgradeToolTip));

                lblGroupRating.DoOneWayDataBinding("Text", _skillGroup, nameof(SkillGroup.DisplayRating));
            }
            else
            {
                flpRightCareer.Visible = false;

                nudKarma.DoDatabinding("Value", _skillGroup, nameof(SkillGroup.Karma));
                nudKarma.DoOneWayDataBinding("Enabled", _skillGroup, nameof(SkillGroup.KarmaUnbroken));
                nudKarma.DoOneWayDataBinding("InterceptMouseWheel", _skillGroup.CharacterObject.Options, nameof(CharacterOptions.InterceptMode));

                nudSkill.DoOneWayDataBinding("Visible", _skillGroup.CharacterObject, nameof(Character.BuildMethodHasSkillPoints));
                nudSkill.DoDatabinding("Value", _skillGroup, nameof(SkillGroup.Base));
                nudSkill.DoOneWayDataBinding("Enabled", _skillGroup, nameof(SkillGroup.BaseUnbroken));
                nudSkill.DoOneWayDataBinding("InterceptMouseWheel", _skillGroup.CharacterObject.Options, nameof(CharacterOptions.InterceptMode));
            }

            ResumeLayout();
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
        private void btnCareerIncrease_Click(object sender, EventArgs e)
        {
            string confirmstring = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpense"),
                    _skillGroup.CurrentDisplayName, _skillGroup.Rating + 1, _skillGroup.UpgradeKarmaCost);

            if (!_skillGroup.CharacterObject.ConfirmKarmaExpense(confirmstring))
                return;

            _skillGroup.Upgrade();
        }
        #endregion

        #region Properties
        public int NameWidth => lblName.PreferredWidth;
        #endregion

        #region Methods
        /// <summary>
        /// Update the position of controls.
        /// </summary>
        /// <param name="intNameWidth">Width of the Name label</param>
        public void MoveControls(int intNameWidth)
        {
            lblName.MinimumSize = new Size(intNameWidth, lblName.MinimumSize.Height);
        }
        #endregion

        /// <summary>
        /// I'm not super pleased with how this works, but it's functional so w/e.
        /// The goal is for controls to retain the ability to display tooltips even while disabled. IT DOES NOT WORK VERY WELL.
        /// </summary>
        #region ButtonWithToolTip Visibility workaround

        ButtonWithToolTip _activeButton;
        protected ButtonWithToolTip ActiveButton
        {
            get => _activeButton;
            set
            {
                if (value == ActiveButton) return;
                ActiveButton?.ToolTipObject.Hide(this);
                _activeButton = value;
                if (_activeButton?.Visible == true)
                {
                    ActiveButton?.ToolTipObject.Show(ActiveButton?.ToolTipText, this);
                }
            }
        }

        protected Control FindToolTipControl(Point pt)
        {
            foreach (Control c in Controls)
            {
                if (!(c is ButtonWithToolTip)) continue;
                if (c.Bounds.Contains(pt)) return c;
            }
            return null;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            ActiveButton = FindToolTipControl(e.Location) as ButtonWithToolTip;
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            ActiveButton = null;
        }
        #endregion

        private void SkillGroupControl_DpiChangedAfterParent(object sender, EventArgs e)
        {
            lblGroupRating.MinimumSize = new Size((int)(25 * _objGraphics.DpiX / 96.0f), 0);
        }
    }
}
