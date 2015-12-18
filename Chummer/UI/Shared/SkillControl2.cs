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
		public SkillControl2(Skill skill)
		{
			this.skill = skill;
			InitializeComponent();

			lblName.DataBindings.Add("Text", skill, "DisplayName");
			lblAttribute.DataBindings.Add("Text", skill, "Attribute");
			nudKarma.DataBindings.Add("Value", skill, "Karma", false, DataSourceUpdateMode.OnPropertyChanged);
			nudSkill.DataBindings.Add("Value", skill, "Base", false, DataSourceUpdateMode.OnPropertyChanged);

			nudKarma.DataBindings.Add("Enabled", skill, "IsUnlocked", false, DataSourceUpdateMode.OnPropertyChanged);
			nudSkill.DataBindings.Add("Enabled", skill, "IsUnlocked", false, DataSourceUpdateMode.OnPropertyChanged);
			
			lblModifiedRating.DataBindings.Add("Text", skill, "DisplayPool",false, DataSourceUpdateMode.OnPropertyChanged);

			cmdDelete.Visible = skill.AllowDelete;
			cboSpec.DataSource = skill.Specializations;
			cboSpec.DisplayMember = "Name";
			cboSpec.DataBindings.Add("SelectedText", skill, "Specialization", false, DataSourceUpdateMode.OnPropertyChanged);
		}

		private void SkillControl2_Load(object sender, EventArgs e)
		{

		}
	}
}
