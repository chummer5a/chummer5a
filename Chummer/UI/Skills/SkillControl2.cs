/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Chummer.Backend.Skills;
using Chummer.Backend.Attributes;

namespace Chummer.UI.Skills
{
    [DebuggerDisplay("{_skill.Name} {Visible} {btnAddSpec.Visible}")]
    public sealed partial class SkillControl2 : UserControl
    {
        private readonly Skill _skill;
        private readonly Font _normal;
        private readonly Font _italic;
        private readonly Font _italicName;
        private CharacterAttrib _attributeActive;

        public SkillControl2(Skill skill)
        {
            _skill = skill;
            InitializeComponent();
            SuspendLayout();

            foreach (ToolStripItem objItem in cmsSkillLabel.Items)
            {
                LanguageManager.TranslateToolStripItemsRecursively(objItem, GlobalOptions.Language);
            }

            DataBindings.Add("Enabled", skill, nameof(Skill.Enabled), false, DataSourceUpdateMode.OnPropertyChanged);

            //Display
            if (!skill.Default)
            {
                _italicName = new Font(lblName.Font, FontStyle.Italic);
                lblName.Font = _italicName;
            }
            if (!string.IsNullOrWhiteSpace(_skill.Notes))
            {
                lblName.ForeColor = Color.SaddleBrown;
            }

            lblName.DataBindings.Add("Text", skill, nameof(Skill.DisplayName));
            _attributeActive = skill.AttributeObject;
            _skill.PropertyChanged += Skill_PropertyChanged;
            _skill.CharacterObject.AttributeSection.PropertyChanged += AttributeSection_PropertyChanged;
            Skill_PropertyChanged(null, null);  //if null it updates all
            _normal = btnAttribute.Font;
            _italic = new Font(_normal, FontStyle.Italic);
            if (skill.CharacterObject.Created)
            {
                lblModifiedRating.Location = new Point(256 - 13, 4);

                lblCareerRating.DataBindings.Add("Text", skill, nameof(Skill.Rating), false,
                    DataSourceUpdateMode.OnPropertyChanged);
                lblCareerRating.Visible = true;

                btnCareerIncrease.Visible = true;
                btnCareerIncrease.DataBindings.Add("Enabled", skill, nameof(Skill.CanUpgradeCareer), false, DataSourceUpdateMode.OnPropertyChanged);
                nudSkill.Visible = false;
                nudKarma.Visible = false;
                chkKarma.Visible = false;

                cboSpec.Visible = false;
                btnAddSpec.DataBindings.Add("Enabled", skill, nameof(Skill.CanAffordSpecialization), false, DataSourceUpdateMode.OnPropertyChanged);
                
                lblCareerSpec.Visible = true;
                lblCareerSpec.DataBindings.Add("Text", skill, nameof(Skill.DisplaySpecialization), false, DataSourceUpdateMode.OnPropertyChanged);

                lblAttribute.Visible = false;  //Was true, cannot think it should be

                btnAttribute.DataBindings.Add("Text", skill, nameof(Skill.DisplayAttribute));
                btnAttribute.Visible = true;

                SetupDropdown();
            }
            else
            {
                lblAttribute.DataBindings.Add("Text", skill, nameof(Skill.DisplayAttribute));
                //Up down boxes
                nudKarma.DataBindings.Add("Value", skill, nameof(Skill.Karma), false, DataSourceUpdateMode.OnPropertyChanged);
                nudSkill.DataBindings.Add("Value", skill, nameof(Skill.Base), false, DataSourceUpdateMode.OnPropertyChanged);

                nudSkill.DataBindings.Add("Visible", skill.CharacterObject, nameof(skill.CharacterObject.BuildMethodHasSkillPoints), false, DataSourceUpdateMode.OnPropertyChanged);
                nudSkill.DataBindings.Add("Enabled", skill, nameof(Skill.BaseUnlocked), false,
                    DataSourceUpdateMode.OnPropertyChanged);
                nudSkill.DataBindings.Add("InterceptMouseWheel", skill.CharacterObject.Options, nameof(CharacterOptions.InterceptMode), false, 
                    DataSourceUpdateMode.OnPropertyChanged);
                nudKarma.DataBindings.Add("Enabled", skill, nameof(Skill.KarmaUnlocked), false,
                    DataSourceUpdateMode.OnPropertyChanged);
                nudKarma.DataBindings.Add("InterceptMouseWheel", skill.CharacterObject.Options, nameof(CharacterOptions.InterceptMode), false, 
                    DataSourceUpdateMode.OnPropertyChanged);

                chkKarma.DataBindings.Add("Visible", skill.CharacterObject, nameof(skill.CharacterObject.BuildMethodHasSkillPoints), false, DataSourceUpdateMode.OnPropertyChanged);
                chkKarma.DataBindings.Add("Checked", skill, nameof(Skill.BuyWithKarma), false, DataSourceUpdateMode.OnPropertyChanged);
                chkKarma.DataBindings.Add("Enabled", skill, nameof(Skill.CanHaveSpecs), false, DataSourceUpdateMode.OnPropertyChanged);

                cboSpec.BeginUpdate();
                if (skill.IsExoticSkill)
                {
                    cboSpec.Enabled = false;
                    cboSpec.DataBindings.Add("Text", skill, nameof(Skill.DisplaySpecialization), false, DataSourceUpdateMode.OnPropertyChanged);
                }
                else
                {
                    //dropdown/spec
                    cboSpec.DisplayMember = nameof(ListItem.Name);
                    cboSpec.ValueMember = nameof(ListItem.Value);
                    cboSpec.DataBindings.Add("Enabled", skill, nameof(Skill.CanHaveSpecs), false, DataSourceUpdateMode.OnPropertyChanged);
                    cboSpec.SelectedIndex = -1;
                    cboSpec.DataSource = skill.CGLSpecializations;
                    
                    cboSpec.DataBindings.Add("Text", skill, nameof(Skill.Specialization), false, DataSourceUpdateMode.OnPropertyChanged);
                }
                cboSpec.EndUpdate();
            }

            //Delete button
            if (skill.AllowDelete)
            {
                cmdDelete.Visible = true;
                cmdDelete.Click += (sender, args) =>
                {
                    skill.UnbindSkill();
                    skill.CharacterObject.SkillsSection.Skills.Remove(skill);
                    skill.CharacterObject.SkillsSection.SkillsDictionary.Remove(skill.IsExoticSkill ? skill.Name + " (" + skill.DisplaySpecializationMethod(GlobalOptions.Language) + ')' : skill.Name);
                };

                if (skill.CharacterObject.Created)
                {
                    btnAddSpec.Location = new Point(btnAddSpec.Location.X - cmdDelete.Width, btnAddSpec.Location.Y);
                }
            }

            ResumeLayout();
        }

        private void AttributeSection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AttributeSection.AttributeCategory))
            {
                _attributeActive.PropertyChanged -= AttributeActiveOnPropertyChanged;
                _attributeActive = _skill.CharacterObject.GetAttribute((string)cboSelectAttribute.SelectedValue);

                _attributeActive.PropertyChanged += AttributeActiveOnPropertyChanged;
                AttributeActiveOnPropertyChanged(sender, e);
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
                    BackColor = _skill.Leveled ? SystemColors.ButtonHighlight : SystemColors.Control;
                    btnAddSpec.Visible = _skill.CharacterObject.Created &&  !_skill.IsExoticSkill && _skill.CanHaveSpecs;
                    if (all) { goto case nameof(Skill.SkillToolTip); }  break;


                case nameof(Skill.SkillToolTip):
                    tipTooltip.SetToolTip(lblName, _skill.SkillToolTip);  //is this the best way?
                    //tipTooltip.SetToolTip(this, skill.SkillToolTip);
                    //tipTooltip.SetToolTip(lblAttribute, skill.SkillToolTip);
                    //tipTooltip.SetToolTip(lblCareerSpec, skill.SkillToolTip);
                    if (all) { goto case nameof(Skill.AddSpecToolTip); } break;


                case nameof(Skill.AddSpecToolTip):
                    tipTooltip.SetToolTip(btnAddSpec, _skill.AddSpecToolTip);
                    if (all) { goto case nameof(Skill.PoolToolTip); } break;


                case nameof(Skill.PoolToolTip):
                    tipTooltip.SetToolTip(lblModifiedRating, _skill.PoolToolTip);
                    if (all) { goto case nameof(Skill.UpgradeToolTip); } break;


                case nameof(Skill.UpgradeToolTip):
                    tipTooltip.SetToolTip(btnCareerIncrease, _skill.UpgradeToolTip);
                    if (all) { goto case nameof(Skill.Rating); } break;

                case nameof(Skill.Rating):
                case nameof(Skill.Specialization):
                    lblModifiedRating.Text =  _skill.DisplayOtherAttribute(_attributeActive.TotalValue, _attributeActive.Abbrev);
                    break;
            }
        }

        private void btnCareerIncrease_Click(object sender, EventArgs e)
        {
            string confirmstring = string.Format(LanguageManager.GetString("Message_ConfirmKarmaExpense", GlobalOptions.Language),
                    _skill.DisplayName, _skill.Rating + 1, _skill.UpgradeKarmaCost());

            if (!_skill.CharacterObject.ConfirmKarmaExpense(confirmstring))
                return;

            _skill.Upgrade();
        }

        private void btnAddSpec_Click(object sender, EventArgs e)
        {
            int price = _skill.CharacterObject.Options.KarmaSpecialization;

            int intExtraSpecCost = 0;
            int intTotalBaseRating = _skill.TotalBaseRating;
            decimal decSpecCostMultiplier = 1.0m;
            foreach (Improvement objLoopImprovement in _skill.CharacterObject.Improvements)
            {
                if (objLoopImprovement.Minimum <= intTotalBaseRating &&
                    (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == _skill.CharacterObject.Created || (objLoopImprovement.Condition == "create") != _skill.CharacterObject.Created) && objLoopImprovement.Enabled)
                {
                    if (objLoopImprovement.ImprovedName == _skill.SkillCategory)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCost)
                            intExtraSpecCost += objLoopImprovement.Value;
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier)
                            decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
            }
            if (decSpecCostMultiplier != 1.0m)
                price = decimal.ToInt32(decimal.Ceiling(price * decSpecCostMultiplier));
            price += intExtraSpecCost; //Spec

            string confirmstring = string.Format(LanguageManager.GetString("Message_ConfirmKarmaExpenseSkillSpecialization", GlobalOptions.Language), price.ToString());

            if (!_skill.CharacterObject.ConfirmKarmaExpense(confirmstring))
                return;

            frmSelectSpec selectForm = new frmSelectSpec(_skill);
            selectForm.ShowDialog();

            if (selectForm.DialogResult != DialogResult.OK) return;

            _skill.AddSpecialization(selectForm.SelectedItem);

            if (ParentForm is CharacterShared frmParent)
                frmParent.IsCharacterUpdateRequested = true;
        }

        private void SetupDropdown()
        {
            List<ListItem> lstAttributeItems = new List<ListItem>();
		    foreach (string strLoopAttribute in AttributeSection.AttributeStrings)
		    {
                if (strLoopAttribute != "MAGAdept")
                    lstAttributeItems.Add(new ListItem (strLoopAttribute, LanguageManager.GetString($"String_Attribute{strLoopAttribute}Short", GlobalOptions.Language)));
            }

            cboSelectAttribute.BeginUpdate();
            cboSelectAttribute.ValueMember = "Value";
            cboSelectAttribute.DisplayMember = "Name";
            cboSelectAttribute.DataSource = lstAttributeItems;
            cboSelectAttribute.SelectedValue = _skill.AttributeObject.Abbrev;
            cboSelectAttribute.EndUpdate();
        }

        private void btnAttribute_Click(object sender, EventArgs e)
        {
            btnAttribute.Visible = false;
            cboSelectAttribute.Visible = true;
            cboSelectAttribute.DroppedDown = true;
        }

        private void cboSelectAttribute_Closed(object sender, EventArgs e)
        {
            btnAttribute.Visible = true;
            cboSelectAttribute.Visible = false;
            _attributeActive.PropertyChanged -= AttributeActiveOnPropertyChanged;
            _attributeActive = _skill.CharacterObject.GetAttribute((string) cboSelectAttribute.SelectedValue);

            btnAttribute.Font = _attributeActive == _skill.AttributeObject ? _normal : _italic;
            btnAttribute.Text = cboSelectAttribute.Text;

            _attributeActive.PropertyChanged += AttributeActiveOnPropertyChanged;
            AttributeActiveOnPropertyChanged(null, null);

            CustomAttributeChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CustomAttributeChanged;

        public bool CustomAttributeSet => _attributeActive != _skill.AttributeObject;

        public int NameWidth => lblName.PreferredWidth;
        public int NudSkillWidth => nudSkill.Width;

        public void ResetSelectAttribute()
        {
            if (CustomAttributeSet)
            {
                cboSelectAttribute.SelectedValue = _skill.AttributeObject.Abbrev;
                cboSelectAttribute_Closed(null, null);
            }
        }

        private void tsSkillLabelNotes_Click(object sender, EventArgs e)
        {
            frmNotes frmItemNotes = new frmNotes
            {
                Notes = _skill.Notes
            };
            frmItemNotes.ShowDialog(this);

            if (frmItemNotes.DialogResult == DialogResult.OK)
            {
                _skill.Notes = frmItemNotes.Notes;
                _skill.Notes = _skill.Notes.WordWrap(100);
                tipTooltip.SetToolTip(lblName, _skill.SkillToolTip);
            }
            if (!string.IsNullOrEmpty(_skill.Notes))
            {
                lblName.ForeColor = Color.SaddleBrown;
            }
            else
            {
                lblName.ForeColor = Color.Black;
            }
        }

        private void AttributeActiveOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            Skill_PropertyChanged(sender, new PropertyChangedEventArgs(nameof(Skill.Rating)));
        }
        
        private void lblName_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDF(_skill.Source + ' ' + _skill.DisplayPage(GlobalOptions.Language));
        }

        private void cboSpec_TextChanged(object sender, EventArgs e)
        {
            if (!_skill.CharacterObject.Options.AllowPointBuySpecializationsOnKarmaSkills &&
                !string.IsNullOrWhiteSpace(cboSpec.Text) && (nudSkill.Value == 0 || !nudSkill.Enabled))
            {
                chkKarma.Checked = true;
            }
        }

        /* Delnar: TODO Awaiting other authors' approval before activation.
        private void chkKarma_CheckChanged(object sender, EventArgs e)
        {
            cboSpec_TextChanged(sender, e);
        }
        */
        public void MoveControls(int i)
        {
            lblName.Width = i;
            if (_skill.CharacterObject.Created)
            {
                btnAttribute.Left = lblName.Right + 6;
                cboSelectAttribute.Left = lblName.Right + 6;
                lblCareerRating.Left = cboSelectAttribute.Right + 6;
            }
            else
            {
                nudSkill.Left = lblName.Right + 6;
                nudKarma.Left = nudSkill.Right + 6;
                lblAttribute.Left = nudKarma.Right + 6;
            }
        }

        public void UnbindSkillControl()
        {
            _skill.PropertyChanged -= Skill_PropertyChanged;
            _skill.CharacterObject.AttributeSection.PropertyChanged -= AttributeSection_PropertyChanged;

            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }
    }
}
