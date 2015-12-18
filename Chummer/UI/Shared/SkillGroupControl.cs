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
	public partial class SkillGroupControl : UserControl
	{
		private SkillGroup _skillGroup;
		public SkillGroupControl(SkillGroup skillGroup)
		{
			_skillGroup = skillGroup;
			InitializeComponent();

			
		}

		private void SkillGroupControl_Load(object sender, EventArgs e)
		{
			lblName.DataBindings.Add("Text", _skillGroup, "Name");
			nudKarma.DataBindings.Add("Value", _skillGroup, "Karma", false, DataSourceUpdateMode.OnPropertyChanged);
			nudSkill.DataBindings.Add("Value", _skillGroup, "Base", false, DataSourceUpdateMode.OnPropertyChanged);

			if (_skillGroup.Character.Created)
			{
				//TODO: Change display to play mode
			}
			else
			{
				if (_skillGroup.Character.BuildMethod == CharacterBuildMethod.Karma ||
					_skillGroup.Character.BuildMethod == CharacterBuildMethod.LifeModule)
				{
					nudSkill.Visible = false;
				}
			}
		}
	}
}
