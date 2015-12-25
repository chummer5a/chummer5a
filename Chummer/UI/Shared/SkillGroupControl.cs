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
			lblName.DataBindings.Add("Text", _skillGroup, "DisplayName");

			nudKarma.DataBindings.Add("Value", _skillGroup, "Karma", false, DataSourceUpdateMode.OnPropertyChanged);
			nudKarma.DataBindings.Add("Enabled", _skillGroup, "KarmaUnbroken", false, DataSourceUpdateMode.OnPropertyChanged);


			nudSkill.DataBindings.Add("Value", _skillGroup, "Base", false, DataSourceUpdateMode.OnPropertyChanged);
			nudSkill.DataBindings.Add("Enabled", _skillGroup, "BaseUnbroken", false, DataSourceUpdateMode.OnPropertyChanged);
			
			//_skillGroup.PropertyChanged += (o, args) =>
			//{
			//	if (args.PropertyName == nameof(SkillGroup.BaseUnbroken))
			//	{
			//		nudSkill.Enabled = _skillGroup.BaseUnbroken;
			//	}
			//};

			if (_skillGroup.Character.Created)
			{
				//TODO: Change display to play mode
			}
			else
			{
				if (_skillGroup.Character.BuildMethod == CharacterBuildMethod.Karma ||
					_skillGroup.Character.BuildMethod == CharacterBuildMethod.LifeModule)
				{
					nudSkill.Enabled = false;
				}
			}
		}
	}
}
