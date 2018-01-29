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
using System.Windows.Forms;
using Chummer.Backend.Skills;
using System.ComponentModel;

namespace Chummer.UI.Skills
{
    public partial class SkillGroupControl : UserControl
    {
        private readonly SkillGroup _skillGroup;
        public SkillGroupControl(SkillGroup skillGroup)
        {
            _skillGroup = skillGroup;
            InitializeComponent();

            //This is apparently a factor 30 faster than placed in load. NFI why
            Stopwatch sw = Stopwatch.StartNew();
            SuspendLayout();
            lblName.DataBindings.Add("Text", _skillGroup, "DisplayName");

            _skillGroup.PropertyChanged += SkillGroup_PropertyChanged;
            tipToolTip.SetToolTip(lblName, _skillGroup.ToolTip);

            if (_skillGroup.Character.Created)
            {
                nudKarma.Visible = false;
                nudSkill.Visible = false;

                btnCareerIncrease.Visible = true;
                btnCareerIncrease.DataBindings.Add("Enabled", _skillGroup, nameof(SkillGroup.CareerCanIncrease), false, DataSourceUpdateMode.OnPropertyChanged);
                tipToolTip.SetToolTip(btnCareerIncrease, _skillGroup.UpgradeToolTip);

                lblGroupRating.Visible = true;
                lblGroupRating.DataBindings.Add("Text", _skillGroup, nameof(SkillGroup.DisplayRating), false, DataSourceUpdateMode.OnPropertyChanged);
            }
            else
            {
                nudKarma.DataBindings.Add("Value", _skillGroup, "Karma", false, DataSourceUpdateMode.OnPropertyChanged);
                nudKarma.DataBindings.Add("Enabled", _skillGroup, "KarmaUnbroken", false, DataSourceUpdateMode.OnPropertyChanged);
                nudKarma.DataBindings.Add("InterceptMouseWheel", _skillGroup.Character.Options, nameof(CharacterOptions.InterceptMode), false, DataSourceUpdateMode.OnPropertyChanged);

                nudSkill.DataBindings.Add("Value", _skillGroup, "Base", false, DataSourceUpdateMode.OnPropertyChanged);
                nudSkill.DataBindings.Add("Enabled", _skillGroup, "BaseUnbroken", false, DataSourceUpdateMode.OnPropertyChanged);
                nudSkill.DataBindings.Add("InterceptMouseWheel", _skillGroup.Character.Options, nameof(CharacterOptions.InterceptMode), false, DataSourceUpdateMode.OnPropertyChanged);

                if (_skillGroup.Character.BuildMethod == CharacterBuildMethod.Karma ||
                    _skillGroup.Character.BuildMethod == CharacterBuildMethod.LifeModule)
                {
                    nudSkill.Enabled = false;
                }
            }
            ResumeLayout();
            sw.TaskEnd("Create skillgroup");
        }

        #region Control Events
        private void btnCareerIncrease_Click(object sender, EventArgs e)
        {
            string confirmstring = string.Format(LanguageManager.GetString("Message_ConfirmKarmaExpense", GlobalOptions.Language),
                    _skillGroup.DisplayName, _skillGroup.Rating + 1, _skillGroup.UpgradeKarmaCost());

            if (!_skillGroup.Character.ConfirmKarmaExpense(confirmstring))
                return;

            _skillGroup.Upgrade();
        }

        private void SkillGroup_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            //I learned something from this but i'm not sure it is a good solution
            //scratch that, i'm sure it is a bad solution. (Tooltip manager from tooltip, properties from reflection?

            //if name of changed is null it does magic to change all, otherwise it only does one.
            bool all = false;
            switch (propertyChangedEventArgs?.PropertyName)
            {
                case null:
                    all = true;
                    goto case nameof(SkillGroup.ToolTip);
                case nameof(SkillGroup.ToolTip):
                    tipToolTip.SetToolTip(lblName, _skillGroup.ToolTip);
                    if (all) { goto case nameof(Skill.UpgradeToolTip); }
                    break;
                case nameof(SkillGroup.UpgradeToolTip):
                    tipToolTip.SetToolTip(btnCareerIncrease, _skillGroup.UpgradeToolTip);
                    break;
            }
        }
        #endregion

        #region Properties
        public int NameWidth => lblName.PreferredWidth;
        public int RatingWidth => lblGroupRating.PreferredWidth;
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
            lblGroupRating.Left = lblName.Right + 2;
            if (_skillGroup.Character.Created)
            {
                btnCareerIncrease.Left = lblGroupRating.Left + ratingWidth + 4;
            }
            else
            {
                nudSkill.Left = lblGroupRating.Right + 2;
                nudKarma.Left = nudSkill.Right + 2;
            }
        }
        #endregion
    }
}
