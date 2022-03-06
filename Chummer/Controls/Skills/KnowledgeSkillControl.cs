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
using Chummer.Annotations;
using Chummer.Backend.Skills;
using Chummer.Properties;

namespace Chummer.UI.Skills
{
    public sealed partial class KnowledgeSkillControl : UserControl
    {
        private bool _blnUpdatingName = true;
        private bool _blnUpdatingSpec = true;
        private readonly KnowledgeSkill _objSkill;
        private readonly Timer _tmrNameChangeTimer;
        private readonly Timer _tmrSpecChangeTimer;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly NumericUpDownEx nudKarma;
        private readonly NumericUpDownEx nudSkill;
        private readonly Label lblRating;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ButtonWithToolTip btnCareerIncrease;
        private readonly ColorableCheckBox chkNativeLanguage;
        private readonly ElasticComboBox cboSpec;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ColorableCheckBox chkKarma;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Label lblSpec;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ButtonWithToolTip btnAddSpec;

        public KnowledgeSkillControl(KnowledgeSkill objSkill)
        {
            if (objSkill == null)
                return;
            _objSkill = objSkill;
            InitializeComponent();
            SuspendLayout();
            tlpMain.SuspendLayout();
            tlpMiddle.SuspendLayout();
            try
            {
                lblModifiedRating.DoOneWayDataBinding("Text", objSkill, nameof(KnowledgeSkill.DisplayPool));
                lblModifiedRating.DoOneWayDataBinding("ToolTipText", objSkill, nameof(KnowledgeSkill.PoolToolTip));

                cmdDelete.DoOneWayDataBinding("Visible", objSkill, nameof(Skill.AllowDelete));

                cboType.BeginUpdate();
                cboType.PopulateWithListItems(objSkill.CharacterObject.SkillsSection.MyKnowledgeTypes);
                cboType.DoDataBinding("SelectedValue", objSkill, nameof(KnowledgeSkill.Type));
                cboType.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.AllowTypeChange));
                cboType.EndUpdate();

                lblName.DoOneWayNegatableDataBinding("Visible", objSkill, nameof(Skill.AllowNameChange));
                lblName.DoOneWayDataBinding("Text", objSkill, nameof(KnowledgeSkill.WritableName));
                lblName.DoOneWayDataBinding("ForeColor", objSkill, nameof(Skill.PreferredColor));

                cboName.BeginUpdate();
                cboName.PopulateWithListItems(objSkill.CharacterObject.SkillsSection.MyDefaultKnowledgeSkills);
                cboName.SelectedIndex = -1;
                cboName.Text = objSkill.WritableName;
                cboName.DoOneWayDataBinding("Visible", objSkill, nameof(Skill.AllowNameChange));
                cboName.EndUpdate();
                _blnUpdatingName = false;
                _tmrNameChangeTimer = new Timer { Interval = 1000 };
                _tmrNameChangeTimer.Tick += NameChangeTimer_Tick;

                if (objSkill.CharacterObject.Created)
                {
                    lblRating = new Label
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        Name = "lblRating",
                        Text = "00",
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    btnCareerIncrease = new ButtonWithToolTip
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        ImageDpi96 = Resources.add,
                        ImageDpi192 = Resources.add1,
                        MinimumSize = new Size(24, 24),
                        Name = "btnCareerIncrease",
                        Padding = new Padding(1),
                        UseVisualStyleBackColor = true
                    };
                    btnCareerIncrease.Click += btnCareerIncrease_Click;

                    lblRating.DoOneWayNegatableDataBinding("Visible", objSkill, nameof(KnowledgeSkill.IsNativeLanguage));
                    lblRating.DoOneWayDataBinding("Text", objSkill, nameof(Skill.Rating));

                    btnCareerIncrease.DoOneWayDataBinding("Visible", objSkill, nameof(KnowledgeSkill.AllowUpgrade));
                    btnCareerIncrease.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanUpgradeCareer));
                    btnCareerIncrease.DoOneWayDataBinding("ToolTipText", objSkill, nameof(Skill.UpgradeToolTip));

                    tlpMain.Controls.Add(lblRating, 1, 0);
                    tlpMain.Controls.Add(btnCareerIncrease, 2, 0);

                    lblSpec = new Label
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        Name = "lblSpec",
                        Text = "[SPEC]",
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    btnAddSpec = new ButtonWithToolTip
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        ImageDpi96 = Resources.add,
                        ImageDpi192 = Resources.add1,
                        MinimumSize = new Size(24, 24),
                        Name = "btnAddSpec",
                        Padding = new Padding(1),
                        UseVisualStyleBackColor = true
                    };
                    btnAddSpec.Click += btnAddSpec_Click;
                    
                    lblSpec.DoOneWayDataBinding("Text", objSkill, nameof(Skill.CurrentDisplaySpecialization));

                    btnAddSpec.DoOneWayDataBinding("Visible", objSkill, nameof(Skill.CanHaveSpecs));
                    btnAddSpec.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanAffordSpecialization));
                    btnAddSpec.DoOneWayDataBinding("ToolTipText", objSkill, nameof(Skill.AddSpecToolTip));

                    tlpMiddle.SetColumnSpan(lblSpec, 2);
                    tlpMiddle.Controls.Add(lblSpec, 1, 0);
                    tlpMiddle.Controls.Add(btnAddSpec, 3, 0);
                }
                else
                {
                    nudSkill = new NumericUpDownEx
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        InterceptMouseWheel = NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver,
                        Margin = new Padding(3, 2, 3, 2),
                        Maximum = new decimal(new[] { 99, 0, 0, 0 }),
                        Name = "nudSkill"
                    };
                    nudKarma = new NumericUpDownEx
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        InterceptMouseWheel = NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver,
                        Margin = new Padding(3, 2, 3, 2),
                        Maximum = new decimal(new[] { 99, 0, 0, 0 }),
                        Name = "nudKarma"
                    };

                    nudSkill.DoOneWayDataBinding("Visible", objSkill.CharacterObject.SkillsSection,
                        nameof(SkillsSection.HasKnowledgePoints));
                    nudSkill.DoOneWayDataBinding("Enabled", objSkill, nameof(KnowledgeSkill.AllowUpgrade));
                    nudSkill.DoDataBinding("Value", objSkill, nameof(Skill.Base));
                    nudSkill.InterceptMouseWheel = GlobalSettings.InterceptMode;
                    nudKarma.DoOneWayDataBinding("Enabled", objSkill, nameof(KnowledgeSkill.AllowUpgrade));
                    nudKarma.DoDataBinding("Value", objSkill, nameof(Skill.Karma));
                    nudKarma.InterceptMouseWheel = GlobalSettings.InterceptMode;

                    tlpMain.Controls.Add(nudSkill, 1, 0);
                    tlpMain.Controls.Add(nudKarma, 2, 0);

                    chkNativeLanguage = new ColorableCheckBox(components)
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        DefaultColorScheme = true,
                        Margin = new Padding(3, 4, 3, 4),
                        Name = "chkNativeLanguage",
                        Tag = "Skill_NativeLanguageLong",
                        Text = "Native",
                        UseVisualStyleBackColor = true
                    };
                    cboSpec = new ElasticComboBox
                    {
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        AutoCompleteMode = AutoCompleteMode.Suggest,
                        FormattingEnabled = true,
                        Margin = new Padding(3, 0, 3, 0),
                        Name = "cboSpec",
                        TabStop = false
                    };
                    chkKarma = new ColorableCheckBox(components)
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        DefaultColorScheme = true,
                        Margin = new Padding(3, 4, 3, 4),
                        Name = "chkKarma",
                        UseVisualStyleBackColor = true
                    };

                    chkNativeLanguage.DoOneWayDataBinding("Visible", objSkill, nameof(Skill.IsLanguage));
                    chkNativeLanguage.Enabled = objSkill.IsNativeLanguage ||
                                                objSkill.CharacterObject.SkillsSection.HasAvailableNativeLanguageSlots;
                    chkNativeLanguage.DoDataBinding("Checked", objSkill, nameof(Skill.IsNativeLanguage));

                    cboSpec.BeginUpdate();
                    cboSpec.PopulateWithListItems(objSkill.CGLSpecializations);
                    cboSpec.EndUpdate();
                    cboSpec.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanHaveSpecs));
                    cboSpec.Text = objSkill.CurrentDisplaySpecialization;
                    cboSpec.TextChanged += cboSpec_TextChanged;
                    _blnUpdatingSpec = false;
                    _tmrSpecChangeTimer = new Timer { Interval = 1000 };
                    _tmrSpecChangeTimer.Tick += SpecChangeTimer_Tick;

                    chkKarma.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanHaveSpecs));
                    chkKarma.DoDataBinding("Checked", objSkill, nameof(Skill.BuyWithKarma));

                    tlpMiddle.Controls.Add(chkNativeLanguage, 1, 0);
                    tlpMiddle.Controls.Add(cboSpec, 2, 0);
                    tlpMiddle.Controls.Add(chkKarma, 3, 0);

                    // Hacky way of fixing a weird UI issue caused by items of a combobox only being populated from the DataSource after the combobox is added
                    _blnUpdatingSpec = true;
                    cboSpec.Text = objSkill.CurrentDisplaySpecialization;
                    _blnUpdatingSpec = false;
                }

                if (objSkill.ForcedName)
                {
                    this.DoOneWayDataBinding("Enabled", objSkill, nameof(KnowledgeSkill.Enabled));
                }

                AdjustForDpi();
                this.UpdateLightDarkMode();
                this.TranslateWinForm(blnDoResumeLayout: false);
            }
            finally
            {
                tlpMiddle.ResumeLayout();
                tlpMain.ResumeLayout();
                ResumeLayout(true);
                objSkill.PropertyChanged += Skill_PropertyChanged;
                objSkill.CharacterObject.SkillsSection.PropertyChanged += OnSkillsSectionPropertyChanged;
            }
        }

        private void OnSkillsSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SkillsSection.HasAvailableNativeLanguageSlots) && chkNativeLanguage != null)
            {
                chkNativeLanguage.Enabled = _objSkill.IsNativeLanguage || _objSkill.CharacterObject.SkillsSection.HasAvailableNativeLanguageSlots;
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
                    if (cboSpec != null)
                    {
                        string strOldSpec = _objSkill.CGLSpecializations.Count != 0 ? cboSpec.SelectedItem?.ToString() : cboSpec.Text;
                        IReadOnlyList<ListItem> lstSpecializations = _objSkill.CGLSpecializations;
                        cboSpec.QueueThreadSafe(() =>
                        {
                            _blnUpdatingSpec = true;
                            cboSpec.BeginUpdate();
                            cboSpec.PopulateWithListItems(lstSpecializations);
                            if (string.IsNullOrEmpty(strOldSpec))
                                cboSpec.SelectedIndex = -1;
                            else
                            {
                                cboSpec.SelectedValue = strOldSpec;
                                if (cboSpec.SelectedIndex == -1)
                                    cboSpec.Text = strOldSpec;
                            }
                            cboSpec.EndUpdate();
                            _blnUpdatingSpec = false;
                        });
                    }
                    if (blnAll)
                        goto case nameof(KnowledgeSkill.WritableName);
                    break;

                case nameof(KnowledgeSkill.WritableName):
                    if (!_blnUpdatingName)
                    {
                        string strWritableName = _objSkill.WritableName;
                        cboName.QueueThreadSafe(() =>
                        {
                            _blnUpdatingName = true;
                            cboName.Text = strWritableName;
                            _blnUpdatingName = false;
                        });
                    }
                    if (blnAll)
                        goto case nameof(Skill.TopMostDisplaySpecialization);
                    break;

                case nameof(KnowledgeSkill.TopMostDisplaySpecialization):
                    if (!_blnUpdatingSpec)
                    {
                        string strDisplaySpec = _objSkill.TopMostDisplaySpecialization;
                        cboSpec.QueueThreadSafe(() =>
                        {
                            _blnUpdatingSpec = true;
                            cboSpec.Text = strDisplaySpec;
                            _blnUpdatingSpec = false;
                        });
                    }
                    if (blnAll)
                        goto case nameof(Skill.IsNativeLanguage);
                    break;

                case nameof(Skill.IsNativeLanguage):
                    if (chkNativeLanguage != null)
                    {
                        bool blnEnabled = _objSkill.IsNativeLanguage ||
                                          _objSkill.CharacterObject.SkillsSection.HasAvailableNativeLanguageSlots;
                        chkNativeLanguage.QueueThreadSafe(() => chkNativeLanguage.Enabled = blnEnabled);
                    }
                    break;
            }
        }

        private void UnbindKnowledgeSkillControl()
        {
            _tmrNameChangeTimer?.Dispose();
            _tmrSpecChangeTimer?.Dispose();
            _objSkill.PropertyChanged -= Skill_PropertyChanged;
            if (!_objSkill.CharacterObject.IsDisposed)
                _objSkill.CharacterObject.SkillsSection.PropertyChanged -= OnSkillsSectionPropertyChanged;
            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private async void btnCareerIncrease_Click(object sender, EventArgs e)
        {
            int upgradeKarmaCost = _objSkill.UpgradeKarmaCost;

            if (upgradeKarmaCost == -1)
                return; //TODO: more descriptive
            string confirmstring = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpense"),
                _objSkill.CurrentDisplayName, _objSkill.Rating + 1, upgradeKarmaCost, cboType.GetItemText(cboType.SelectedItem));

            if (!CommonFunctions.ConfirmKarmaExpense(confirmstring))
                return;

            _objSkill.Upgrade();
        }

        private async void btnAddSpec_Click(object sender, EventArgs e)
        {
            int price = _objSkill.CharacterObject.Settings.KarmaKnowledgeSpecialization;

            decimal decExtraSpecCost = 0;
            int intTotalBaseRating = _objSkill.TotalBaseRating;
            decimal decSpecCostMultiplier = 1.0m;
            foreach (Improvement objLoopImprovement in _objSkill.CharacterObject.Improvements)
            {
                if (objLoopImprovement.Minimum <= intTotalBaseRating
                    && (string.IsNullOrEmpty(objLoopImprovement.Condition)
                        || (objLoopImprovement.Condition == "career") == _objSkill.CharacterObject.Created
                        || (objLoopImprovement.Condition == "create") != _objSkill.CharacterObject.Created)
                    && objLoopImprovement.Enabled
                    && objLoopImprovement.ImprovedName == _objSkill.SkillCategory)
                {
                    switch (objLoopImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.SkillCategorySpecializationKarmaCost:
                            decExtraSpecCost += objLoopImprovement.Value;
                            break;

                        case Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier:
                            decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                            break;
                    }
                }
            }
            if (decSpecCostMultiplier != 1.0m)
                price = (price * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
            else
                price += decExtraSpecCost.StandardRound(); //Spec

            string confirmstring = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseSkillSpecialization"), price);

            if (!CommonFunctions.ConfirmKarmaExpense(confirmstring))
                return;

            Form frmToUse = ParentForm ?? Program.MainForm;

            DialogResult eResult = await frmToUse.DoThreadSafeFunc(async x =>
            {
                using (SelectSpec selectForm = new SelectSpec(_objSkill) {Mode = "Knowledge"})
                {
                    await selectForm.ShowDialogSafeAsync(x);

                    if (selectForm.DialogResult == DialogResult.OK)
                        _objSkill.AddSpecialization(selectForm.SelectedItem);

                    return selectForm.DialogResult;
                }
            });

            if (eResult != DialogResult.OK)
                return;

            if (ParentForm is CharacterShared frmParent)
                frmParent.IsCharacterUpdateRequested = true;
        }

        private async void cmdDelete_Click(object sender, EventArgs e)
        {
            if (!_objSkill.AllowDelete)
                return;
            if (!CommonFunctions.ConfirmDelete(await LanguageManager.GetStringAsync("Message_DeleteKnowledgeSkill")))
                return;
            _objSkill.CharacterObject.SkillsSection.KnowledgeSkills.Remove(_objSkill);
        }

        [UsedImplicitly]
        public int NameWidth => tlpLeft.Width - (lblName.Visible ? lblName.Margin.Left + lblName.Margin.Right : cboName.Margin.Left + cboName.Margin.Right);

        [UsedImplicitly]
        public int NudSkillWidth => nudSkill?.Visible == true ? nudSkill.Width : 0;

        [UsedImplicitly]
        public int RightButtonsWidth => tlpRight.Width;

        /// <summary>
        /// I'm not super pleased with how this works, but it's functional so w/e.
        /// The goal is for controls to retain the ability to display tooltips even while disabled. IT DOES NOT WORK VERY WELL.
        /// </summary>
        #region ButtonWithToolTip Visibility workaround

        private ButtonWithToolTip _activeButton;

        private ButtonWithToolTip ActiveButton
        {
            get => _activeButton;
            set
            {
                if (value == ActiveButton)
                    return;
                ActiveButton?.ToolTipObject.Hide(this);
                _activeButton = value;
                if (ActiveButton?.Visible == true)
                {
                    ActiveButton.ToolTipObject.Show(ActiveButton.ToolTipText, this);
                }
            }
        }

        private ButtonWithToolTip FindToolTipControl(Point pt)
        {
            return Controls.OfType<ButtonWithToolTip>().FirstOrDefault(c => c.Bounds.Contains(pt));
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            ActiveButton = FindToolTipControl(e.Location);
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            ActiveButton = null;
        }

        #endregion ButtonWithToolTip Visibility workaround

        private void KnowledgeSkillControl_DpiChangedAfterParent(object sender, EventArgs e)
        {
            AdjustForDpi();
        }

        private void AdjustForDpi()
        {
            using (Graphics g = CreateGraphics())
            {
                if (lblRating != null)
                    lblRating.MinimumSize = new Size((int)(25 * g.DpiX / 96.0f), 0);
                lblModifiedRating.MinimumSize = new Size((int)(50 * g.DpiX / 96.0f), 0);
            }
        }

        // Hacky solutions to data binding causing cursor to reset whenever the user is typing something in: have text changes start a timer, and have a 1s delay in the timer update fire the text update
        private void cboName_TextChanged(object sender, EventArgs e)
        {
            if (_tmrNameChangeTimer == null)
                return;
            if (_tmrNameChangeTimer.Enabled)
                _tmrNameChangeTimer.Stop();
            if (_blnUpdatingName)
                return;
            _tmrNameChangeTimer.Start();
        }

        private void cboSpec_TextChanged(object sender, EventArgs e)
        {
            if (_tmrSpecChangeTimer == null)
                return;
            if (_tmrSpecChangeTimer.Enabled)
                _tmrSpecChangeTimer.Stop();
            if (_blnUpdatingSpec)
                return;
            _tmrSpecChangeTimer.Start();
        }

        private void NameChangeTimer_Tick(object sender, EventArgs e)
        {
            _tmrNameChangeTimer.Stop();
            _blnUpdatingName = true;
            _objSkill.WritableName = cboName.Text;
            _blnUpdatingName = false;
        }

        private void SpecChangeTimer_Tick(object sender, EventArgs e)
        {
            _tmrSpecChangeTimer.Stop();
            _blnUpdatingSpec = true;
            _objSkill.TopMostDisplaySpecialization = cboSpec.Text;
            _blnUpdatingSpec = false;
        }
    }
}
