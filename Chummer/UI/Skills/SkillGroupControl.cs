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
        public SkillGroupControl(SkillGroup skillGroup)
        {
            if (skillGroup == null)
                return;
            _skillGroup = skillGroup;
            InitializeComponent();

            this.TranslateWinForm();

            //This is apparently a factor 30 faster than placed in load. NFI why
            Stopwatch sw = Stopwatch.StartNew();
            SuspendLayout();
            lblName.DoDatabinding("Text", _skillGroup, nameof(SkillGroup.CurrentDisplayName));
            lblName.DoDatabinding("ToolTipText", _skillGroup, nameof(SkillGroup.ToolTip));

            nudSkill.Visible = !skillGroup.CharacterObject.Created && skillGroup.CharacterObject.BuildMethodHasSkillPoints;
            nudKarma.Visible = !skillGroup.CharacterObject.Created;

            btnCareerIncrease.Visible = skillGroup.CharacterObject.Created;
            lblGroupRating.Visible = skillGroup.CharacterObject.Created;

            if (_skillGroup.CharacterObject.Created)
            {
                btnCareerIncrease.DoDatabinding("Enabled", _skillGroup, nameof(SkillGroup.CareerCanIncrease));
                btnCareerIncrease.DoDatabinding("ToolTipText", _skillGroup, nameof(SkillGroup.UpgradeToolTip));

                lblGroupRating.DoDatabinding("Text", _skillGroup, nameof(SkillGroup.DisplayRating));
            }
            else
            {
                nudKarma.DoDatabinding("Value", _skillGroup, nameof(SkillGroup.Karma));
                nudKarma.DoDatabinding("Enabled", _skillGroup, nameof(SkillGroup.KarmaUnbroken));
                nudKarma.DoDatabinding("InterceptMouseWheel", _skillGroup.CharacterObject.Options, nameof(CharacterOptions.InterceptMode));

                nudSkill.DoDatabinding("Visible", _skillGroup.CharacterObject, nameof(Character.BuildMethodHasSkillPoints));
                nudSkill.DoDatabinding("Value", _skillGroup, nameof(SkillGroup.Base));
                nudSkill.DoDatabinding("Enabled", _skillGroup, nameof(SkillGroup.BaseUnbroken));
                nudSkill.DoDatabinding("InterceptMouseWheel", _skillGroup.CharacterObject.Options, nameof(CharacterOptions.InterceptMode));
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
        public int RatingWidth => _skillGroup.CharacterObject.Created ? lblGroupRating.PreferredWidth : nudSkill.Width;
        #endregion

        #region Methods
        /// <summary>
        /// Update the position of controls.
        /// </summary>
        /// <param name="nameWidth">Width of the Name label</param>
        /// <param name="ratingWidth">Width of the Rating label. Expected to be the width of the localised Label_SkillGroup_Broken string.</param>
        public void MoveControls(int nameWidth, int ratingWidth)
        {
            lblName.Width = nameWidth;
            if (_skillGroup.CharacterObject.Created)
            {
                lblGroupRating.Left = lblName.Left + nameWidth + 6;
                btnCareerIncrease.Left = lblGroupRating.Left + ratingWidth + 6;
            }
            else
            {
                nudSkill.Left = lblName.Left + nameWidth + 6;
                nudKarma.Left = nudSkill.Left + ratingWidth + 6;
            }
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
    }
}
