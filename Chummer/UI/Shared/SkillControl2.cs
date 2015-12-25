using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chummer.Skills;

namespace Chummer.UI.Shared
{
	public partial class SkillControl2 : UserControl
	{
		private static double total = 0;
		private Skill skill;
		public SkillControl2(Skill skill)
		{
			this.skill = skill;
			InitializeComponent();

			//Display
			lblName.DataBindings.Add("Text", skill, nameof(Skill.DisplayName));
			lblAttribute.DataBindings.Add("Text", skill, nameof(Skill.Attribute));
			lblModifiedRating.DataBindings.Add("Text", skill, nameof(Skill.DisplayPool), false, DataSourceUpdateMode.OnPropertyChanged);

			//Up down boxes
			nudKarma.DataBindings.Add("Value", skill, nameof(Skill.Karma), false, DataSourceUpdateMode.OnPropertyChanged);
			nudSkill.DataBindings.Add("Value", skill, nameof(Skill.Base), false, DataSourceUpdateMode.OnPropertyChanged);

			nudSkill.DataBindings.Add("Enabled", skill, nameof(Skill.BaseUnlocked), false, DataSourceUpdateMode.OnPropertyChanged);
			
			//Delete button
			cmdDelete.Visible = skill.AllowDelete;

			//dropdown/spec
			cboSpec.DataSource = skill.CGLSpecializations;
			cboSpec.DisplayMember = nameof(ListItem.Name);
			cboSpec.ValueMember = nameof(ListItem.Value);

			cboSpec.DataBindings.Add("Enabled", skill, nameof(Skill.Leveled), false, DataSourceUpdateMode.OnPropertyChanged);

			cboSpec.DataBindings.Add("Text", skill, nameof(Skill.Specialization), false, DataSourceUpdateMode.OnPropertyChanged);
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
					nudSkill.Enabled = false;
				}
			}
		}
	}
}
