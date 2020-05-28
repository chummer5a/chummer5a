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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chummer.Backend.Skills;

namespace Chummer.UI.Skills
{
    public sealed partial class KnowledgeSkillControl : UserControl
    {
        private readonly KnowledgeSkill _skill;
        public KnowledgeSkillControl(KnowledgeSkill skill)
        {
            if (skill == null)
                return;
            _skill = skill;
            InitializeComponent();

            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            //Display
            lblModifiedRating.DoDatabinding("Text", skill, nameof(KnowledgeSkill.DisplayPool));
            lblModifiedRating.DoDatabinding("ToolTipText", skill, nameof(KnowledgeSkill.PoolToolTip));

            List<ListItem> lstTypes = KnowledgeSkill.KnowledgeTypes(skill.CharacterObject).ToList();
            lstTypes.Sort(CompareListItems.CompareNames);

            cboType.BeginUpdate();
            cboSkill.BeginUpdate();
            cboSpec.BeginUpdate();
            cboType.DataSource = lstTypes;
            cboType.DisplayMember = nameof(ListItem.Name);
            cboType.ValueMember = nameof(ListItem.Value);
            cboType.DataBindings.Add("SelectedValue", skill, nameof(KnowledgeSkill.Type), false, DataSourceUpdateMode.OnPropertyChanged);

            nudSkill.Visible = !skill.CharacterObject.Created && skill.CharacterObject.SkillsSection.HasKnowledgePoints;
            nudKarma.Visible = !skill.CharacterObject.Created;
            chkKarma.Visible = !skill.CharacterObject.Created;
            cboSpec.Visible = !skill.CharacterObject.Created;
            //cboType.Visible = !skill.CharacterObject.Created;

            btnCareerIncrease.Visible = skill.CharacterObject.Created;
            lblSpec.Visible = skill.CharacterObject.Created;
            btnAddSpec.Visible = skill.CharacterObject.Created;
            lblRating.Visible = skill.CharacterObject.Created;

            if (skill.CharacterObject.Created)
            {
                nudKarma.Visible = false;
                nudSkill.Visible = false;

                lblRating.Visible = true;
                lblRating.DataBindings.Add("Text", skill, nameof(Skill.Rating), false, DataSourceUpdateMode.OnPropertyChanged);

                //New knowledge skills start at 0. Leave the Type selector unlocked until they spend Karma on the skill.
                cboType.Enabled = (skill.Karma == 0 && skill.Base == 0 || string.IsNullOrWhiteSpace(_skill.Type));

                lblName.Visible = true;
                lblName.DataBindings.Add("Text", skill, nameof(KnowledgeSkill.WriteableName), false, DataSourceUpdateMode.OnPropertyChanged);
                lblName.DataBindings.Add("ForeColor", skill, nameof(Skill.PreferredColor));
                lblName.DataBindings.Add("ToolTipText", skill, nameof(Skill.HtmlSkillToolTip));

                lblSpec.Visible = true;
                lblSpec.DataBindings.Add("Text", skill, nameof(Skill.CurrentDisplaySpecialization), false, DataSourceUpdateMode.OnPropertyChanged);

                cboSkill.Visible = false;
                chkKarma.Visible = false;
                cboSpec.Visible = false;

                lblModifiedRating.Location = new Point(294 - 30, 4);

                btnAddSpec.DataBindings.Add("Enabled", skill, nameof(Skill.CanAffordSpecialization), false, DataSourceUpdateMode.OnPropertyChanged);
                btnAddSpec.DataBindings.Add("Visible", skill, nameof(Skill.CanHaveSpecs), false, DataSourceUpdateMode.OnPropertyChanged);
                btnAddSpec.DataBindings.Add("ToolTipText", skill, nameof(Skill.AddSpecToolTip), false, DataSourceUpdateMode.OnPropertyChanged);
                btnCareerIncrease.DataBindings.Add("Enabled", skill, nameof(Skill.CanUpgradeCareer), false, DataSourceUpdateMode.OnPropertyChanged);
                btnCareerIncrease.DataBindings.Add("ToolTipText", skill, nameof(Skill.UpgradeToolTip), false, DataSourceUpdateMode.OnPropertyChanged);
            }
            else
            {
                //Up down boxes
                nudKarma.DataBindings.Add("Value", skill, nameof(Skill.Karma), false, DataSourceUpdateMode.OnPropertyChanged);
                nudKarma.DataBindings.Add("InterceptMouseWheel", skill.CharacterObject.Options, nameof(CharacterOptions.InterceptMode), false, DataSourceUpdateMode.OnPropertyChanged);

                nudSkill.DataBindings.Add("Value", skill, nameof(Skill.Base), false, DataSourceUpdateMode.OnPropertyChanged);
                nudSkill.DataBindings.Add("Enabled", skill.CharacterObject.SkillsSection, nameof(SkillsSection.HasKnowledgePoints), false, DataSourceUpdateMode.OnPropertyChanged);
                nudSkill.DataBindings.Add("InterceptMouseWheel", skill.CharacterObject.Options, nameof(CharacterOptions.InterceptMode), false, DataSourceUpdateMode.OnPropertyChanged);

                chkKarma.DataBindings.Add("Checked", skill, nameof(Skill.BuyWithKarma), false,
                        DataSourceUpdateMode.OnPropertyChanged);
                List<ListItem> lstDefaultKnowledgeSkills = KnowledgeSkill.DefaultKnowledgeSkills(skill.CharacterObject).ToList();
                lstDefaultKnowledgeSkills.Sort(CompareListItems.CompareNames);
                cboSkill.DataSource = lstDefaultKnowledgeSkills;
                cboSkill.DisplayMember = nameof(ListItem.Name);
                cboSkill.ValueMember = nameof(ListItem.Value);
                cboSkill.SelectedIndex = -1;
                cboSkill.DataBindings.Add("Text", skill, nameof(KnowledgeSkill.WriteableName), false, DataSourceUpdateMode.OnPropertyChanged);
                cboSkill.DataBindings.Add(new NegatableBinding("Enabled", skill, nameof(KnowledgeSkill.ForcedName), true));

                //dropdown/spec
                cboSpec.DataSource = skill.CGLSpecializations;
                cboSpec.DisplayMember = nameof(ListItem.Name);
                cboSpec.ValueMember = nameof(ListItem.Value);
                cboSpec.SelectedIndex = -1;

                cboSpec.DataBindings.Add("Enabled", skill, nameof(Skill.CanHaveSpecs), false, DataSourceUpdateMode.OnPropertyChanged);
                cboSpec.DataBindings.Add("Text", skill, nameof(Skill.Specialization), false, DataSourceUpdateMode.OnPropertyChanged);

                skill.PropertyChanged += Skill_PropertyChanged;
            }

            cmdDelete.DataBindings.Add("Visible", skill, nameof(Skill.AllowDelete), false, DataSourceUpdateMode.OnPropertyChanged);
            cmdDelete.Click += (sender, args) =>
            {
                if (!skill.CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteKnowledgeSkill",
                    GlobalOptions.Language)))
                    return;
                skill.UnbindSkill();
                skill.CharacterObject.SkillsSection.KnowledgeSkills.Remove(skill);
            };
            if (skill.ForcedName)
            {
                DataBindings.Add("Enabled", skill, nameof(KnowledgeSkill.Enabled), false, DataSourceUpdateMode.OnPropertyChanged);
                if (!skill.CharacterObject.Created)
                    cboType.Enabled = string.IsNullOrEmpty(_skill.Type);
            }
            if (!skill.AllowUpgrade)
            {
                nudKarma.Visible = false;
                nudSkill.Visible = false;
                cboSkill.Enabled = false;
                chkKarma.Visible = false;
                btnAddSpec.Enabled = false;
                btnCareerIncrease.Enabled = false;

                if (!skill.CharacterObject.Created)
                {
                    lblRating.Visible = true;
                    lblRating.DataBindings.Add("Text", skill, nameof(Skill.Rating), false, DataSourceUpdateMode.OnPropertyChanged);
                }
            }
            cboType.EndUpdate();
            cboSkill.EndUpdate();
            cboSpec.EndUpdate();
        }

        public void Skill_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool all = false;
            switch (e?.PropertyName)
            {
                case null:
                    all = true;
                    goto case nameof(Skill.CGLSpecializations);
                case nameof(Skill.CGLSpecializations):
                    if (!_skill.CharacterObject.Created)
                    {
                        string strOldSpec = _skill.CGLSpecializations.Count != 0 ? cboSpec.SelectedItem?.ToString() : cboSpec.Text;
                        cboSpec.SuspendLayout();
                        cboSpec.DataSource = null;
                        cboSpec.DataSource = _skill.CGLSpecializations;
                        cboSpec.DisplayMember = nameof(ListItem.Name);
                        cboSpec.ValueMember = nameof(ListItem.Value);
                        cboSpec.MaxDropDownItems = Math.Max(1, _skill.CGLSpecializations.Count);
                        if (string.IsNullOrEmpty(strOldSpec))
                            cboSpec.SelectedIndex = -1;
                        else
                        {
                            cboSpec.SelectedValue = strOldSpec;
                            if (cboSpec.SelectedIndex == -1)
                                cboSpec.Text = strOldSpec;
                        }

                        cboSpec.ResumeLayout();
                    }

                    if (all)
                        goto case nameof(KnowledgeSkill.Type);
                    break;
                case nameof(KnowledgeSkill.Type):
                    if (!cboSkill.Enabled)
                        cboType.Enabled = string.IsNullOrEmpty(_skill.Type);
                    break;
            }
        }

        public void UnbindKnowledgeSkillControl()
        {
            _skill.PropertyChanged -= Skill_PropertyChanged;
            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private void btnCareerIncrease_Click(object sender, EventArgs e)
        {
            int upgradeKarmaCost = _skill.UpgradeKarmaCost;

            if (upgradeKarmaCost == -1)
                return; //TODO: more descriptive
            string confirmstring = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpense"),
                _skill.CurrentDisplayName, _skill.Rating + 1, upgradeKarmaCost, cboType.GetItemText(cboType.SelectedItem));

            if (!_skill.CharacterObject.ConfirmKarmaExpense(confirmstring))
                return;

            cboType.Enabled = false;

            _skill.Upgrade();
        }

        private void btnAddSpec_Click(object sender, EventArgs e)
        {
            int price = _skill.CharacterObject.Options.KarmaKnowledgeSpecialization;

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

            string confirmstring = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpenseSkillSpecialization"), price);

            if (!_skill.CharacterObject.ConfirmKarmaExpense(confirmstring))
                return;

            using (frmSelectSpec selectForm = new frmSelectSpec(_skill)
            {
                Mode = "Knowledge"
            })
            {
                selectForm.ShowDialog();

                if (selectForm.DialogResult != DialogResult.OK)
                    return;

                _skill.AddSpecialization(selectForm.SelectedItem);
            }

            if (ParentForm is CharacterShared frmParent)
                frmParent.IsCharacterUpdateRequested = true;
        }

        public void MoveControls(int i)
        {
            lblName.Width = i;
        }

        /// <summary>
        /// I'm not super pleased with how this works, but it's functional so w/e.
        /// The goal is for controls to retain the ability to display tooltips even while disabled. IT DOES NOT WORK VERY WELL.
        /// </summary>
        #region ButtonWithToolTip Visibility workaround

        ButtonWithToolTip _activeButton;

        private ButtonWithToolTip ActiveButton
        {
            get => _activeButton;
            set
            {
                if (value == ActiveButton) return;
                ActiveButton?.ToolTipObject.Hide(this);
                _activeButton = value;
                if (_activeButton?.Visible == true)
                {
                    ActiveButton?.ToolTipObject.Show(ActiveButton?.ToolTipText, this);
                }
            }
        }

        private Control FindToolTipControl(Point pt)
        {
            foreach (Control c in Controls)
            {
                if (!(c is ButtonWithToolTip)) continue;
                if (c.Bounds.Contains(pt)) return c;
            }
            return null;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            ActiveButton = FindToolTipControl(e.Location) as ButtonWithToolTip;
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            ActiveButton = null;
        }
        #endregion
    }
}
