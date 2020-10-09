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
        private bool _blnUpdatingName = true;
        private readonly KnowledgeSkill _skill;
        private readonly Timer _tmrNameChangeTimer;
        public KnowledgeSkillControl(KnowledgeSkill skill)
        {
            if (skill == null)
                return;
            _skill = skill;
            _tmrNameChangeTimer = new Timer { Interval = 1000 };
            _tmrNameChangeTimer.Tick += NameChangeTimer_Tick;
            InitializeComponent();
            KnowledgeSkillControl_DpiChangedAfterParent(null, EventArgs.Empty);
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            lblModifiedRating.DoOneWayDataBinding("Text", _skill, nameof(KnowledgeSkill.DisplayPool));
            lblModifiedRating.DoOneWayDataBinding("ToolTipText", _skill, nameof(KnowledgeSkill.PoolToolTip));

            cmdDelete.DoOneWayDataBinding("Visible", _skill, nameof(Skill.AllowDelete));

            cboType.BeginUpdate();
            cboType.DataSource = null;
            cboType.DisplayMember = nameof(ListItem.Name);
            cboType.ValueMember = nameof(ListItem.Value);
            cboType.DataSource = _skill.MyKnowledgeTypes;
            cboType.DoDatabinding("SelectedValue", _skill, nameof(KnowledgeSkill.Type));
            cboType.DoOneWayDataBinding("Enabled", _skill, nameof(Skill.AllowTypeChange));
            cboType.EndUpdate();

            lblName.DoOneWayNegatableDatabinding("Visible", _skill, nameof(Skill.AllowNameChange));
            lblName.DoOneWayDataBinding("Text", _skill, nameof(KnowledgeSkill.WriteableName));
            lblName.DoOneWayDataBinding("ForeColor", _skill, nameof(Skill.PreferredColor));

            cboName.BeginUpdate();
            cboName.DoOneWayDataBinding("Visible", _skill, nameof(Skill.AllowNameChange));
            cboName.DataSource = null;
            cboName.DisplayMember = nameof(ListItem.Name);
            cboName.ValueMember = nameof(ListItem.Value);
            cboName.DataSource = KnowledgeSkill.DefaultKnowledgeSkills(_skill.CharacterObject);
            cboName.SelectedIndex = -1;
            cboName.Text = _skill.WriteableName;
            cboName.EndUpdate();
            _blnUpdatingName = false;

            if (_skill.CharacterObject.Created)
            {
                flpButtonsCreate.Visible = false;
                tlpSpecsCreate.Visible = false;
                flpButtonsCareer.Dock = DockStyle.Fill;
                tlpSpecsCareer.Dock = DockStyle.Fill;

                lblRating.DoOneWayNegatableDatabinding("Visible", _skill, nameof(KnowledgeSkill.IsNativeLanguage));
                lblRating.DoOneWayDataBinding("Text", _skill, nameof(Skill.Rating));
                lblSpec.DoOneWayNegatableDatabinding("Visible", _skill, nameof(KnowledgeSkill.IsNativeLanguage));
                lblSpec.DoOneWayDataBinding("Text", _skill, nameof(Skill.CurrentDisplaySpecialization));

                btnCareerIncrease.DoOneWayDataBinding("Visible", _skill, nameof(KnowledgeSkill.AllowUpgrade));
                btnCareerIncrease.DoOneWayDataBinding("Enabled", _skill, nameof(Skill.CanUpgradeCareer));
                btnCareerIncrease.DoOneWayDataBinding("ToolTipText", _skill, nameof(Skill.UpgradeToolTip));

                btnAddSpec.DoOneWayDataBinding("Visible", _skill, nameof(Skill.CanHaveSpecs));
                btnAddSpec.DoOneWayDataBinding("Enabled", _skill, nameof(Skill.CanAffordSpecialization));
                btnAddSpec.DoOneWayDataBinding("ToolTipText", _skill, nameof(Skill.AddSpecToolTip));
            }
            else
            {
                flpButtonsCareer.Visible = false;
                tlpSpecsCareer.Visible = false;
                flpButtonsCreate.Dock = DockStyle.Fill;
                tlpSpecsCreate.Dock = DockStyle.Fill;

                nudSkill.DoOneWayDataBinding("Visible", _skill.CharacterObject.SkillsSection, nameof(SkillsSection.HasKnowledgePoints));
                nudSkill.DoOneWayDataBinding("Enabled", _skill, nameof(KnowledgeSkill.AllowUpgrade));
                nudSkill.DoDatabinding("Value", _skill, nameof(Skill.Base));
                nudSkill.InterceptMouseWheel = GlobalOptions.InterceptMode;
                nudKarma.DoOneWayDataBinding("Enabled", _skill, nameof(KnowledgeSkill.AllowUpgrade));
                nudKarma.DoDatabinding("Value", _skill, nameof(Skill.Karma));
                nudKarma.InterceptMouseWheel = GlobalOptions.InterceptMode;

                chkNativeLanguage.DoOneWayDataBinding("Visible", _skill, nameof(Skill.IsLanguage));
                chkNativeLanguage.Enabled = _skill.IsNativeLanguage || _skill.CharacterObject.SkillsSection.HasAvailableNativeLanguageSlots;
                chkNativeLanguage.DoDatabinding("Checked", _skill, nameof(Skill.IsNativeLanguage));

                cboSpec.BeginUpdate();
                cboSpec.DataSource = null;
                cboSpec.DisplayMember = nameof(ListItem.Name);
                cboSpec.ValueMember = nameof(ListItem.Value);
                cboSpec.DataSource = _skill.CGLSpecializations;
                cboSpec.SelectedIndex = -1;
                cboSpec.DoOneWayDataBinding("Enabled", _skill, nameof(Skill.CanHaveSpecs));
                cboSpec.DoDatabinding("Text", _skill, nameof(Skill.Specialization));
                cboSpec.EndUpdate();
                chkKarma.DoOneWayDataBinding("Enabled", _skill, nameof(Skill.CanHaveSpecs));
                chkKarma.DoDatabinding("Checked", _skill, nameof(Skill.BuyWithKarma));
            }

            if (_skill.ForcedName)
            {
                this.DoOneWayDataBinding("Enabled", _skill, nameof(KnowledgeSkill.Enabled));
            }

            _skill.PropertyChanged += Skill_PropertyChanged;
            _skill.CharacterObject.SkillsSection.PropertyChanged += OnSkillsSectionPropertyChanged;
        }

        private void OnSkillsSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SkillsSection.HasAvailableNativeLanguageSlots) && !_skill.CharacterObject.Created)
            {
                chkNativeLanguage.Enabled = _skill.IsNativeLanguage || _skill.CharacterObject.SkillsSection.HasAvailableNativeLanguageSlots;
            }
        }

        private void Skill_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool blnAll = false;
            switch (e?.PropertyName)
            {
                case null:
                    blnAll = true;
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
                    if (blnAll)
                        goto case nameof(KnowledgeSkill.WriteableName);
                    break;
                case nameof(KnowledgeSkill.WriteableName):
                    if (!_blnUpdatingName)
                        cboName.Text = _skill.WriteableName;
                    if (blnAll)
                        goto case nameof(Skill.IsNativeLanguage);
                    break;
                case nameof(Skill.IsNativeLanguage):
                    if (!_skill.CharacterObject.Created)
                        chkNativeLanguage.Enabled = _skill.IsNativeLanguage || _skill.CharacterObject.SkillsSection.HasAvailableNativeLanguageSlots;
                    break;
            }
        }

        private void UnbindKnowledgeSkillControl()
        {
            _tmrNameChangeTimer?.Dispose();
            _skill.PropertyChanged -= Skill_PropertyChanged;
            _skill.CharacterObject.SkillsSection.PropertyChanged -= OnSkillsSectionPropertyChanged;
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

            if (!CommonFunctions.ConfirmKarmaExpense(confirmstring))
                return;

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

            if (!CommonFunctions.ConfirmKarmaExpense(confirmstring))
                return;

            using (frmSelectSpec selectForm = new frmSelectSpec(_skill) { Mode = "Knowledge" })
            {
                selectForm.ShowDialog(Program.MainForm);

                if (selectForm.DialogResult != DialogResult.OK)
                    return;

                _skill.AddSpecialization(selectForm.SelectedItem);
            }

            if (ParentForm is CharacterShared frmParent)
                frmParent.IsCharacterUpdateRequested = true;
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            if (!CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteKnowledgeSkill")))
                return;
            _skill.UnbindSkill();
            _skill.CharacterObject.SkillsSection.KnowledgeSkills.Remove(_skill);
        }

        [UsedImplicitly]
        public int NameWidth => tlpName.Width - lblName.Margin.Left - lblName.Margin.Right;

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

        private void KnowledgeSkillControl_DpiChangedAfterParent(object sender, EventArgs e)
        {
            lblRating.MinimumSize = new Size(LogicalToDeviceUnits(25), 0);
            lblModifiedRating.MinimumSize = new Size(LogicalToDeviceUnits(50), 0);
        }

        // Hacky solution to data binding causing cursor to reset whenever the user is typing something in: have text changes start a timer, and have a 1s delay in the timer update fire the text update
        private void cboName_TextChanged(object sender, EventArgs e)
        {
            if (_tmrNameChangeTimer.Enabled)
                _tmrNameChangeTimer.Stop();
            if (_blnUpdatingName)
                return;
            _tmrNameChangeTimer.Start();
        }

        private void NameChangeTimer_Tick(object sender, EventArgs e)
        {
            _tmrNameChangeTimer.Stop();
            _blnUpdatingName = true;
            _skill.WriteableName = cboName.Text;
            _blnUpdatingName = false;
        }
    }
}
