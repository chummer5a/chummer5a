using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chummer.Skills;

namespace Chummer.UI.Skills
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
			if (!skill.Default)
			{
				lblName.Font = new Font(lblName.Font, FontStyle.Italic);
			}

			lblName.DataBindings.Add("Text", skill, nameof(Skill.DisplayName));

			//lblModifiedRating.DataBindings.Add("Text", skill, nameof(Skill.DisplayPool), false,
			//	DataSourceUpdateMode.OnPropertyChanged);

			skill.PropertyChanged += Skill_PropertyChanged;

			Skill_PropertyChanged(null, null);  //if null it updates all
			attributeActive = skill.AttributeObject.Abbrev;
			_normal = btnAttribute.Font;
			_italic = new Font(_normal, FontStyle.Italic);
			if (skill.CharacterObject.Created)
			{
				lblModifiedRating.Location = new Point(256 - 13, 4);

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
				lblCareerSpec.Text = string.Join(", ", skill.Specializations.Select(x => x.Name));
				lblCareerSpec.Visible = true;
				lblAttribute.Visible = true;

				btnAttribute.DataBindings.Add("Text", skill, nameof(Skill.Attribute));

				if (skill.ExoticSkill)
				{
					
				}
				else
				{
					
					btnAddSpec.DataBindings.Add("Enabled", skill.CharacterObject, nameof(Character.CanAffordSpecialization), false,
					DataSourceUpdateMode.OnPropertyChanged);
				}

				SetupDropdown();
			}
			else
			{
				lblAttribute.DataBindings.Add("Text", skill, nameof(Skill.Attribute));
				
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
				cmdDelete.Click += (sender, args) => { skill.CharacterObject.SkillsSection.Skills.Remove(skill); };

				if (skill.CharacterObject.Created)
				{
					btnAddSpec.Location = new Point(btnAddSpec.Location.X - cmdDelete.Width, btnAddSpec.Location.Y); 
				}

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

				case nameof(Skill.DisplayPool):
					all = true;
					goto case nameof(Skill.PoolToolTip);

				case nameof(Skill.Leveled):
					BackColor = skill.Leveled ? SystemColors.ButtonHighlight : SystemColors.Control;
					btnAddSpec.Visible = skill.CharacterObject.Created && !skill.ExoticSkill;
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
					if (all) { goto case nameof(Skill.Rating); } break;

				case nameof(Skill.Rating):
					lblModifiedRating.Text =
						skill.DisplayOhterAttribue(skill.CharacterObject.GetAttribute(attributeActive).TotalValue);
					break;
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

		private void SetupDropdown()
		{
			List<ListItem> list =  new[] {"BOD", "AGI", "REA", "STR", "CHA", "INT", "LOG", "WIL", "MAG", "RES"}.Select(
				x => new ListItem(x, LanguageManager.Instance.GetString($"String_Attribute{x}Short"))).ToList();

			cboSelectAttribute.ValueMember = "Value";
			cboSelectAttribute.DisplayMember = "Name";
			cboSelectAttribute.DataSource = list;
			cboSelectAttribute.SelectedValue = skill.AttributeObject.Abbrev;
		}

		private void btnAttribute_Click(object sender, EventArgs e)
		{
			btnAttribute.Visible = false;
			cboSelectAttribute.Visible = true;
			cboSelectAttribute.DroppedDown = true;
		}

		private Control _parrent = null;
		private void cboSpec_ParentChanged(object sender, EventArgs e)
		{
			if (_parrent != null) _parrent.Resize -= Parent_Resize;
			_parrent = Parent;
			if (_parrent != null) _parrent.Resize += Parent_Resize;
		}

		private void Parent_Resize(object sender, EventArgs e)
		{
			Width = (Parent?.Width - 2) ?? Width;
		}

		private readonly Font _normal;
		private readonly Font _italic;
		private string attributeActive;
		private void cboSelectAttribute_Closed(object sender, EventArgs e)
		{
			btnAttribute.Visible = true;
			cboSelectAttribute.Visible = false;
			attributeActive = (string) cboSelectAttribute.SelectedValue;

			btnAttribute.Font = attributeActive == skill.AttributeObject.Abbrev ? _normal : _italic;
			btnAttribute.Text = cboSelectAttribute.Text;

			Skill_PropertyChanged(null, new PropertyChangedEventArgs(nameof(Skill.Rating)));
		}
	}
}
