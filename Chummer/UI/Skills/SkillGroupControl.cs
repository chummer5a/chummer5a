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

        private void btnCareerIncrease_Click(object sender, EventArgs e)
        {
            if (ParentForm is frmCareer objParent)
            {
                string confirmstring = string.Format(LanguageManager.GetString("Message_ConfirmKarmaExpense", GlobalOptions.Language),
                    _skillGroup.DisplayName, _skillGroup.Rating + 1, _skillGroup.UpgradeKarmaCost());

                if (!objParent.ConfirmKarmaExpense(confirmstring))
                    return;
            }

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
    }
}
