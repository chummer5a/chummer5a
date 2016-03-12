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

			DataBindings.Add("Enabled", skill, nameof(Skill.Enabled), false, DataSourceUpdateMode.OnPropertyChanged);

			//Display
			lblName.DataBindings.Add("Text", skill, nameof(Skill.DisplayName));
			lblAttribute.DataBindings.Add("Text", skill, nameof(Skill.Attribute));
			lblModifiedRating.DataBindings.Add("Text", skill, nameof(Skill.DisplayPool), false,
				DataSourceUpdateMode.OnPropertyChanged);

			skill.PropertyChanged += Skill_PropertyChanged;

			Skill_PropertyChanged(null, null);  //if null it updates all

			if (skill.CharacterObject.Created)
			{
				lblCareerRating.DataBindings.Add("Text", skill, nameof(Skill.Rating), false,
					DataSourceUpdateMode.OnPropertyChanged);
				lblCareerRating.Visible = true;

				btnCareerIncrease.Visible = true;
				btnCareerIncrease.DataBindings.Add("Enabled", skill, nameof(Skill.CanUpgradeCareer), false,
					DataSourceUpdateMode.OnPropertyChanged);
				nudSkill.Visible = false;
				nudKarma.Visible = false;
				chkKarma.Visible = false;

				cboSpec.Visible = false;
				lblCareerSpec.Text = string.Join(", ", 
					(from specialization in skill.Specializations
					 select specialization.Name));
				lblCareerSpec.Visible = true;

				btnAddSpec.Visible = skill.Leveled;
				skill.PropertyChanged += VisibleDatabindingBrokenWorkaround;

				btnAddSpec.DataBindings.Add("Enabled", skill.CharacterObject, nameof(Character.CanAffordSpecialization), false,
					DataSourceUpdateMode.OnPropertyChanged);
			}
			else
			{
				//Up down boxes
				nudKarma.DataBindings.Add("Value", skill, nameof(Skill.Karma), false, DataSourceUpdateMode.OnPropertyChanged);
				nudSkill.DataBindings.Add("Value", skill, nameof(Skill.Base), false, DataSourceUpdateMode.OnPropertyChanged);

				nudSkill.DataBindings.Add("Enabled", skill, nameof(Skill.BaseUnlocked), false,
					DataSourceUpdateMode.OnPropertyChanged);

				if (skill.CharacterObject.BuildMethod.HaveSkillPoints())
				{
					chkKarma.DataBindings.Add("Checked", skill, nameof(Skill.BuyWithKarma), false,
						DataSourceUpdateMode.OnPropertyChanged);
				}
				else
				{
					chkKarma.Visible = false;
				}
				
				//dropdown/spec
				cboSpec.DataSource = skill.CGLSpecializations;
				cboSpec.DisplayMember = nameof(ListItem.Name);
				cboSpec.ValueMember = nameof(ListItem.Value);

				cboSpec.DataBindings.Add("Enabled", skill, nameof(Skill.Leveled), false, DataSourceUpdateMode.OnPropertyChanged);

				cboSpec.DataBindings.Add("Text", skill, nameof(Skill.Specialization), false, DataSourceUpdateMode.OnPropertyChanged);
				cboSpec.SelectedIndex = -1;
			}

			//Delete button
			if (skill.AllowDelete)
			{
				cmdDelete.Visible = true;
				cmdDelete.Click += (sender, args) => { skill.CharacterObject.Skills.Remove(skill); };

				//TODO Align?
			}
			else
			{
				cmdDelete.Visible = false;
			}
		}

		private void Skill_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			//I learned something from this but i'm not sure it is a good solution
			//scratch that, i'm sure it is a bad solution. (Tooltip manager from tooltip, properties from reflection?

			//if name of changed is null it does magic to change all, otherwise it only does one.
			bool all = false;
			switch (propertyChangedEventArgs?.PropertyName)
			{
				case null:
					all = true;
					goto case nameof(Skill.Leveled);


				case nameof(Skill.Leveled):
					BackColor = skill.Leveled ? SystemColors.ButtonHighlight : SystemColors.Control;
					if (all) { goto case nameof(Skill.SkillToolTip); }  break;


				case nameof(Skill.SkillToolTip):
					tipTooltip.SetToolTip(lblName, skill.SkillToolTip);  //is this the best way?
					//tipTooltip.SetToolTip(this, skill.SkillToolTip);
					//tipTooltip.SetToolTip(lblAttribute, skill.SkillToolTip);
					//tipTooltip.SetToolTip(lblCareerSpec, skill.SkillToolTip);
					if (all) { goto case nameof(Skill.AddSpecToolTip); } break;


				case nameof(Skill.AddSpecToolTip):
					tipTooltip.SetToolTip(btnAddSpec, skill.AddSpecToolTip);
					if (all) { goto case nameof(Skill.PoolToolTip); } break;


				case nameof(Skill.PoolToolTip):
					tipTooltip.SetToolTip(lblModifiedRating, skill.PoolToolTip);
					if (all) { goto case nameof(Skill.UpgradeToolTip); } break;


				case nameof(Skill.UpgradeToolTip):
					tipTooltip.SetToolTip(btnCareerIncrease, skill.UpgradeToolTip);
					break;
			}
		}

		private void VisibleDatabindingBrokenWorkaround(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Skill.Leveled))
			{
				btnAddSpec.Visible = skill.Leveled;
			}
		}

		private void SkillControl2_Load(object sender, EventArgs e)
		{
			
		}

		private void btnCareerIncrease_Click(object sender, EventArgs e)
		{
			frmCareer parrent = ParentForm as frmCareer;
			if (parrent != null)
			{
				string confirmstring = string.Format(LanguageManager.Instance.GetString("Message_ConfirmKarmaExpense"),
					skill.DisplayName, skill.Rating + 1, skill.UpgradeKarmaCost());
					
				if (!parrent.ConfirmKarmaExpense(confirmstring))
					return;
			}

			skill.Upgrade();
		}

		private void btnAddSpec_Click(object sender, EventArgs e)
		{
			frmCareer parrent = ParentForm as frmCareer;
			if (parrent != null)
			{
				string confirmstring = string.Format(LanguageManager.Instance.GetString("Message_ConfirmKarmaExpenseSkillSpecialization"),
						skill.CharacterObject.Options.KarmaSpecialization);

				if (!parrent.ConfirmKarmaExpense(confirmstring))
					return;
			}

			frmSelectSpec selectForm = new frmSelectSpec(skill);
			selectForm.ShowDialog();

			if (selectForm.DialogResult != DialogResult.OK) return;

			skill.AddSpecialization(selectForm.SelectedItem);

			//TODO turn this into a databinding, but i don't care enough right now
			lblCareerSpec.Text = string.Join(", ",
					(from specialization in skill.Specializations
					 select specialization.Name));

			parrent?.UpdateCharacterInfo();
		}
	}
}
