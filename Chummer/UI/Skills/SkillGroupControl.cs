using System;
using System.Diagnostics;
using System.Windows.Forms;
using Chummer.Skills;

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
            lblName.DataBindings.Add("Text", _skillGroup, "DisplayName");

            tipToolTip.SetToolTip(lblName, _skillGroup.ToolTip);
            
            if (_skillGroup.Character.Created)
            {
                nudKarma.Visible = false;
                nudSkill.Visible = false;

                btnCareerIncrease.Visible = true;
                btnCareerIncrease.DataBindings.Add("Enabled", _skillGroup, nameof(SkillGroup.CareerCanIncrease), false, DataSourceUpdateMode.OnPropertyChanged);
                tipToolTip.SetToolTip(btnCareerIncrease, skillGroup.UpgradeToolTip);

                lblGroupRating.Visible = true;
                lblGroupRating.DataBindings.Add("Text", _skillGroup, nameof(SkillGroup.DisplayRating), false,
                    DataSourceUpdateMode.OnPropertyChanged);
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

            sw.TaskEnd("Create skillgroup");
        }

        private void btnCareerIncrease_Click(object sender, EventArgs e)
        {
            frmCareer parrent = ParentForm as frmCareer;
            if (parrent != null)
            {
                string confirmstring = string.Format(LanguageManager.Instance.GetString("Message_ConfirmKarmaExpense"),
                    _skillGroup.DisplayName, _skillGroup.Rating + 1, _skillGroup.UpgradeKarmaCost());

                if (!parrent.ConfirmKarmaExpense(confirmstring))
                    return;
            }

            _skillGroup.Upgrade();
        }
    }
}
