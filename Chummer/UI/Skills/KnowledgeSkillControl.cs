using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chummer.Skills;

namespace Chummer.UI.Skills
{
	public partial class KnowledgeSkillControl : UserControl
	{
		private readonly KnowledgeSkill skill;
		public KnowledgeSkillControl(KnowledgeSkill skill)
		{
			this.skill = skill;
			InitializeComponent();

			//Display
			lblModifiedRating.DataBindings.Add("Text", skill, nameof(KnowledgeSkill.DisplayPool), false, DataSourceUpdateMode.OnPropertyChanged);

			cboType.DataSource = KnowledgeSkill.KnowledgeTypes;
			cboType.DisplayMember = nameof(ListItem.Name);
			cboType.ValueMember = nameof(ListItem.Value);
			cboType.DataBindings.Add("SelectedValue", skill, nameof(KnowledgeSkill.Type), false, DataSourceUpdateMode.OnPropertyChanged);


			if (skill.CharacterObject.Created)
			{
				nudKarma.Visible = false;
				nudSkill.Visible = false;

				lblRating.Visible = true;
				lblRating.DataBindings.Add("Text", skill, nameof(Skill.Rating), false, DataSourceUpdateMode.OnPropertyChanged);
				
				//New knowledge skills start at 0. Leave the Type selector unlocked until they spend Karma on the skill.
				cboType.Enabled = skill.Karma == 0;

				lblName.Visible = true;
				lblName.DataBindings.Add("Text", skill, nameof(KnowledgeSkill.WriteableName), false, DataSourceUpdateMode.OnPropertyChanged);

				lblSpec.Visible = true;
				lblSpec.Text = string.Join(", ", skill.Specializations.Select(x => x.Name));
				cboSkill.Visible = false;
				chkKarma.Visible = false;
				cboSpec.Visible = false;

				lblModifiedRating.Location = new Point(294 - 30, 4);

				btnAddSpec.Visible = true;
				btnCareerIncrease.Visible = true;

				lblSpec.DataBindings.Add("Text", skill, nameof(Skill.DisplaySpecialization), false, DataSourceUpdateMode.OnPropertyChanged);
			}
			else
			{
				//Up down boxes
				nudKarma.DataBindings.Add("Value", skill, nameof(Skill.Karma), false, DataSourceUpdateMode.OnPropertyChanged);
				nudSkill.DataBindings.Add("Value", skill, nameof(Skill.Base), false, DataSourceUpdateMode.OnPropertyChanged);

				nudSkill.DataBindings.Add("Enabled", skill.CharacterObject.SkillsSection, nameof(SkillsSection.HasKnowledgePoints), false, DataSourceUpdateMode.OnPropertyChanged);

				if (skill.CharacterObject.BuildMethod.HaveSkillPoints() || skill.CharacterObject.Options.FreeKarmaKnowledge)
				{
					chkKarma.DataBindings.Add("Checked", skill, nameof(Skill.BuyWithKarma), false,
						DataSourceUpdateMode.OnPropertyChanged);
				}
				else
				{
					chkKarma.Visible = false;
				}
				
				cboSkill.DataSource = skill.KnowledgeSkillCatagories;
				cboSkill.DisplayMember = nameof(ListItem.Name);
				cboSkill.ValueMember = nameof(ListItem.Value);
				cboSkill.SelectedIndex = -1;
				cboSkill.DataBindings.Add("Text", skill, nameof(KnowledgeSkill.WriteableName), false, DataSourceUpdateMode.OnPropertyChanged);
				
				//dropdown/spec
				cboSpec.DataSource = skill.CGLSpecializations;
				cboSpec.DisplayMember = nameof(ListItem.Name);
				cboSpec.ValueMember = nameof(ListItem.Value);
				cboSpec.SelectedIndex = -1;
				
				cboSpec.DataBindings.Add("Enabled", skill, nameof(Skill.Leveled), false, DataSourceUpdateMode.OnPropertyChanged);
				cboSpec.DataBindings.Add("Text", skill, nameof(Skill.Specialization), false, DataSourceUpdateMode.OnPropertyChanged);

				skill.PropertyChanged += (sender, args) =>
				{
					if (args.PropertyName == nameof(Skill.CGLSpecializations))
					{
						cboSpec.DataSource = null;
						cboSpec.DataSource = skill.CGLSpecializations;
						cboSpec.DisplayMember = nameof(ListItem.Name);
						cboSpec.ValueMember = nameof(ListItem.Value);
						cboSpec.MaxDropDownItems = Math.Max(1, skill.CGLSpecializations.Count);
					}
				};
			}

			if (skill.ForcedName)
			{
				nudKarma.Visible = false;
				nudSkill.Visible = false;
				cboSkill.Enabled = false;
				cboSpec.DataBindings.Clear();
				cboSpec.Enabled = false;
				btnAddSpec.Enabled = false;
				btnCareerIncrease.Enabled = false;

			    lblRating.Visible = true;
			    lblRating.Text = skill.CyberwareRating().ToString();

				cmdDelete.Visible = false;
			}
			else
			{
				cmdDelete.Click += (sender, args) => { skill.CharacterObject.SkillsSection.KnowledgeSkills.Remove(skill); };
			}
		}

		private void btnCareerIncrease_Click(object sender, EventArgs e)
		{
			frmCareer parent = ParentForm as frmCareer;
			if (parent != null)
			{
				int upgradeKarmaCost = skill.UpgradeKarmaCost();

				if (upgradeKarmaCost == -1) return; //TODO: more descriptive
                string confirmstring = "";
                if (skill.Karma == 0)
                {
                    confirmstring = string.Format(LanguageManager.Instance.GetString("Message_ConfirmKarmaExpenseKnowledgeSkill"), 
                        skill.DisplayName, skill.Rating + 1, skill.CharacterObject.Options.KarmaNewKnowledgeSkill, this.cboType.GetItemText(this.cboType.SelectedItem));
                }
                else
                {
                    confirmstring = string.Format(LanguageManager.Instance.GetString("Message_ConfirmKarmaExpense"),
                       skill.DisplayName, skill.Rating + 1, upgradeKarmaCost, this.cboType.GetItemText(this.cboType.SelectedItem));
                }

				if (!parent.ConfirmKarmaExpense(confirmstring))
					return;
			}
			cboType.Enabled = false;

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
			selectForm.Mode = "Knowledge";
			selectForm.ShowDialog();

			if (selectForm.DialogResult != DialogResult.OK) return;

			skill.AddSpecialization(selectForm.SelectedItem);

			//TODO turn this into a databinding, but i don't care enough right now
			lblSpec.Text = string.Join(", ",
					(from specialization in skill.Specializations
					 select specialization.Name));

			parrent?.UpdateCharacterInfo();
		}
	}
}
