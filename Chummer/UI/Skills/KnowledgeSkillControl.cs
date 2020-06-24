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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Chummer.Annotations;
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

            this.TranslateWinForm();

            lblModifiedRating.DoOneWayDataBinding("Text", _skill, nameof(KnowledgeSkill.DisplayPool));
            lblModifiedRating.DoOneWayDataBinding("ToolTipText", _skill, nameof(KnowledgeSkill.PoolToolTip));

            cmdDelete.DoOneWayDataBinding("Visible", _skill, nameof(Skill.AllowDelete));

            cboType.BeginUpdate();
            cboType.DataSource = null;
            cboType.DisplayMember = nameof(ListItem.Name);
            cboType.ValueMember = nameof(ListItem.Value);
            cboType.DataSource = KnowledgeSkill.KnowledgeTypes;
            cboType.DoDatabinding("SelectedValue", _skill, nameof(KnowledgeSkill.Type));

            if (_skill.CharacterObject.Created)
            {
                //New knowledge_skills start at 0. Leave the Type selector unlocked until they spend Karma on the_skill.
                cboType.Enabled = (_skill.Karma == 0 && _skill.Base == 0 && !_skill.ForcedName) || string.IsNullOrWhiteSpace(_skill.Type);
            }
            if (_skill.ForcedName)
            {
                this.DoOneWayDataBinding("Enabled", _skill, nameof(KnowledgeSkill.Enabled));
                if (!_skill.CharacterObject.Created)
                    cboType.Enabled = string.IsNullOrEmpty(_skill.Type);
            }
            cboType.EndUpdate();

            if (_skill.CharacterObject.Created || !_skill.AllowUpgrade)
            {
                flpButtonsCreate.Visible = false;
                tlpSpecsCreate.Visible = false;
                flpButtonsCareer.Dock = DockStyle.Fill;
                tlpSpecsCareer.Dock = DockStyle.Fill;

                lblRating.DoOneWayDataBinding("Text", _skill, nameof(Skill.Rating));
                lblSpec.DoOneWayDataBinding("Text", _skill, nameof(Skill.CurrentDisplaySpecialization));

                if (_skill.AllowUpgrade)
                {
                    if (_skill.Karma == 0 && _skill.Base == 0 && !_skill.ForcedName)
                    {
                        lblName.Visible = false;
                        cboName.BeginUpdate();
                        cboName.DataSource = null;
                        cboName.DisplayMember = nameof(ListItem.Name);
                        cboName.ValueMember = nameof(ListItem.Value);
                        cboName.DataSource = KnowledgeSkill.DefaultKnowledgeSkills;
                        cboName.SelectedIndex = -1;
                        cboName.DoDatabinding("Text", _skill, nameof(KnowledgeSkill.WriteableName));
                        cboName.DoOneWayDataBinding("ForeColor", _skill, nameof(Skill.PreferredColor));
                        cboName.EndUpdate();
                    }
                    else
                    {
                        cboName.Visible = false;
                        lblName.DoOneWayDataBinding("Text", _skill, nameof(KnowledgeSkill.WriteableName));
                        lblName.DoOneWayDataBinding("ForeColor", _skill, nameof(Skill.PreferredColor));
                    }
                    btnCareerIncrease.DoOneWayDataBinding("Enabled", _skill, nameof(Skill.CanUpgradeCareer));
                    btnCareerIncrease.DoOneWayDataBinding("ToolTipText", _skill, nameof(Skill.UpgradeToolTip));

                    btnAddSpec.DoOneWayDataBinding("Enabled", _skill, nameof(Skill.CanAffordSpecialization));
                    btnAddSpec.DoOneWayDataBinding("Visible", _skill, nameof(Skill.CanHaveSpecs));
                    btnAddSpec.DoOneWayDataBinding("ToolTipText", _skill, nameof(Skill.AddSpecToolTip));
                }
                else
                {
                    btnCareerIncrease.Visible = false;
                    btnAddSpec.Visible = false;
                }
            }
            else
            {
                flpButtonsCareer.Visible = false;
                tlpSpecsCareer.Visible = false;
                flpButtonsCreate.Dock = DockStyle.Fill;
                tlpSpecsCreate.Dock = DockStyle.Fill;

                if (!_skill.ForcedName)
                {
                    lblName.Visible = false;
                    cboName.BeginUpdate();
                    cboName.DataSource = null;
                    cboName.DisplayMember = nameof(ListItem.Name);
                    cboName.ValueMember = nameof(ListItem.Value);
                    cboName.DataSource = KnowledgeSkill.DefaultKnowledgeSkills;
                    cboName.SelectedIndex = -1;
                    cboName.DoDatabinding("Text", _skill, nameof(KnowledgeSkill.WriteableName));
                    cboName.DoOneWayDataBinding("ForeColor", _skill, nameof(Skill.PreferredColor));
                    cboName.EndUpdate();
                }
                else
                {
                    cboName.Visible = false;
                    lblName.DoOneWayDataBinding("Text", _skill, nameof(KnowledgeSkill.WriteableName));
                    lblName.DoOneWayDataBinding("ForeColor", _skill, nameof(Skill.PreferredColor));
                }

                nudSkill.DoOneWayDataBinding("Visible", _skill.CharacterObject.SkillsSection, nameof(SkillsSection.HasKnowledgePoints));
                nudSkill.DoDatabinding("Value", _skill, nameof(Skill.Base));
                nudSkill.DoOneWayDataBinding("InterceptMouseWheel", _skill.CharacterObject.Options, nameof(CharacterOptions.InterceptMode));
                nudKarma.DoDatabinding("Value", _skill, nameof(Skill.Karma));
                nudKarma.DoOneWayDataBinding("InterceptMouseWheel", _skill.CharacterObject.Options, nameof(CharacterOptions.InterceptMode));

                cboSpec.BeginUpdate();
                cboSpec.DataSource = null;
                cboSpec.DisplayMember = nameof(ListItem.Name);
                cboSpec.ValueMember = nameof(ListItem.Value);
                cboSpec.DataSource = _skill.CGLSpecializations;
                cboSpec.SelectedIndex = -1;
                cboSpec.DoOneWayDataBinding("Enabled", _skill, nameof(Skill.CanHaveSpecs));
                cboSpec.DoDatabinding("Text", _skill, nameof(Skill.Specialization));
                cboSpec.EndUpdate();
                chkKarma.DoDatabinding("Checked", _skill, nameof(Skill.BuyWithKarma));
            }

            _skill.PropertyChanged += Skill_PropertyChanged;
        }

        private void Skill_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
                        cboSpec.BeginUpdate();
                        cboSpec.DataSource = null;
                        cboSpec.DisplayMember = nameof(ListItem.Name);
                        cboSpec.ValueMember = nameof(ListItem.Value);
                        cboSpec.DataSource = _skill.CGLSpecializations;
                        cboSpec.MaxDropDownItems = Math.Max(1, _skill.CGLSpecializations.Count);
                        if (string.IsNullOrEmpty(strOldSpec))
                            cboSpec.SelectedIndex = -1;
                        else
                        {
                            cboSpec.SelectedValue = strOldSpec;
                            if (cboSpec.SelectedIndex == -1)
                                cboSpec.Text = strOldSpec;
                        }
                        cboSpec.EndUpdate();
                    }
                    if (all)
                        goto case nameof(KnowledgeSkill.Type);
                    break;
                case nameof(KnowledgeSkill.Type):
                    if (!cboName.Visible || lblName.Visible)
                        cboType.Enabled = string.IsNullOrEmpty(_skill.Type);
                    break;
            }
        }

        private void UnbindKnowledgeSkillControl()
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

            SuspendLayout();
            if (cboName.Visible || !lblName.Visible)
            {
                lblName.Visible = true;
                cboName.Visible = false;
                cboName.DataBindings.Clear();
                lblName.DoOneWayDataBinding("Text", _skill, nameof(KnowledgeSkill.WriteableName));
                lblName.DoOneWayDataBinding("ForeColor", _skill, nameof(Skill.PreferredColor));
            }
            cboType.Enabled = false;
            ResumeLayout();

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
                    (string.IsNullOrEmpty(objLoopImprovement.Condition)
                     || (objLoopImprovement.Condition == "career") == _skill.CharacterObject.Created
                     || (objLoopImprovement.Condition == "create") != _skill.CharacterObject.Created)
                    && objLoopImprovement.Enabled)
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

            using (frmSelectSpec selectForm = new frmSelectSpec(_skill) { Mode = "Knowledge" })
            {
                selectForm.ShowDialog();

                if (selectForm.DialogResult != DialogResult.OK)
                    return;

                _skill.AddSpecialization(selectForm.SelectedItem);
            }

            if (ParentForm is CharacterShared frmParent)
                frmParent.IsCharacterUpdateRequested = true;
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            if (!_skill.CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteKnowledgeSkill")))
                return;
            _skill.UnbindSkill();
            _skill.CharacterObject.SkillsSection.KnowledgeSkills.Remove(_skill);
        }

        public void MoveControls(int i)
        {
            if (cboName.Visible)
                cboName.Width = i;
        }

        [UsedImplicitly]
        public int NameWidth => tlpName.Width;

        [UsedImplicitly]
        public int NudSkillWidth => !_skill.CharacterObject.Created && _skill.AllowUpgrade && _skill.CharacterObject.SkillsSection.HasKnowledgePoints
            ? nudSkill.Width
            : 0;

        [UsedImplicitly]
        public int RightButtonsWidth => tlpRight.Width;

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
