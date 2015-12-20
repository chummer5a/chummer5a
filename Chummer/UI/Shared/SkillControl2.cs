using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chummer.Skills;

namespace Chummer.UI.Shared
{
	public partial class SkillControl2 : UserControl
	{
		private Skill skill;
		private bool _lock = true;
		public SkillControl2(Skill skill)
		{
			this.skill = skill;
			InitializeComponent();

			skill.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == nameof(skill.IsUnlocked))
				{
					if(skill.IsUnlocked == false)
						cboSpec.SelectedIndex = -1;

					nudSkill.Enabled = _lock && skill.IsUnlocked;
					nudKarma.Enabled = skill.IsUnlocked;
				}
			};

			//Display
			lblName.DataBindings.Add("Text", skill, "DisplayName");
			lblAttribute.DataBindings.Add("Text", skill, "Attribute");
			lblModifiedRating.DataBindings.Add("Text", skill, "DisplayPool", false, DataSourceUpdateMode.OnPropertyChanged);

			//Up down boxes
			nudKarma.DataBindings.Add("Value", skill, "Karma", false, DataSourceUpdateMode.OnPropertyChanged);
			nudSkill.DataBindings.Add("Value", skill, "Base", false, DataSourceUpdateMode.OnPropertyChanged);

			//nudKarma.DataBindings.Add("Enabled", skill, "IsUnlocked", false, DataSourceUpdateMode.OnPropertyChanged);
			//nudSkill.DataBindings.Add("Enabled", skill, "IsUnlocked", false, DataSourceUpdateMode.OnPropertyChanged);
			
			//Delete button
			cmdDelete.Visible = skill.AllowDelete;

			//dropdown/spec
			cboSpec.DataSource = skill.CGLSpecializations;
			cboSpec.DisplayMember = "Name";
			cboSpec.ValueMember = "Value";

			cboSpec.DataBindings.Add("Enabled", skill, "Leveled", false, DataSourceUpdateMode.OnPropertyChanged);

			cboSpec.DataBindings.Add("Text", skill, "Specialization", false, DataSourceUpdateMode.OnPropertyChanged);
			cboSpec.SelectedIndex = -1;


		}

		private void SkillControl2_Load(object sender, EventArgs e)
		{
			if (skill.CharacterObject.Created)
			{
				//TODO: Change display to play mode
			}
			else
			{
				if (skill.CharacterObject.BuildMethod == CharacterBuildMethod.Karma ||
					skill.CharacterObject.BuildMethod == CharacterBuildMethod.LifeModule)
				{
					_lock = nudSkill.Enabled = false;
				}
			}
		}
	}
}
