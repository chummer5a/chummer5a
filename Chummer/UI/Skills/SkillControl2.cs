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
using System.Linq;
using System.Windows.Forms;
using Chummer.Annotations;
using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;
using Chummer.Properties;

namespace Chummer.UI.Skills
{
    [DebuggerDisplay("{_objSkill.Name} {Visible} {btnAddSpec.Visible}")]
    public sealed partial class SkillControl2 : UserControl
    {
        private readonly bool _blnLoading = true;
        private bool _blnUpdatingSpec = true;
        private readonly Skill _objSkill;
        private readonly Timer _tmrSpecChangeTimer;
        private readonly Font _fntNormal;
        private readonly Font _fntItalic;
        private readonly Font _fntNormalName;
        private readonly Font _fntItalicName;
        private CharacterAttrib _objAttributeActive;
        private readonly Button cmdDelete;
        private readonly ButtonWithToolTip btnCareerIncrease;
        private readonly Label lblCareerRating;
        private readonly NumericUpDownEx nudKarma;
        private readonly NumericUpDownEx nudSkill;
        private readonly Label lblCareerSpec;
        private readonly ButtonWithToolTip btnAddSpec;
        private readonly ElasticComboBox cboSpec;
        private readonly ColorableCheckBox chkKarma;
        private readonly ElasticComboBox cboSelectAttribute;

        public SkillControl2(Skill objSkill)
        {
            if (objSkill == null)
                return;
            _objSkill = objSkill;
            _objAttributeActive = objSkill.AttributeObject;
            if (_objAttributeActive != null)
                _objAttributeActive.PropertyChanged += Attribute_PropertyChanged;
            InitializeComponent();
            SuspendLayout();
            pnlAttributes.SuspendLayout();
            tlpMain.SuspendLayout();
            tlpRight.SuspendLayout();
            try
            {
                //Display
                _fntNormalName = lblName.Font;
                _fntItalicName = new Font(_fntNormalName, FontStyle.Italic);
                _fntNormal = btnAttribute.Font;
                _fntItalic = new Font(_fntNormal, FontStyle.Italic);

                if (!_objSkill.Default)
                    lblName.Font = _fntItalicName;
                lblName.DoOneWayDataBinding("Text", objSkill, nameof(Skill.CurrentDisplayName));
                lblName.DoOneWayDataBinding("ForeColor", objSkill, nameof(Skill.PreferredColor));
                lblName.DoOneWayDataBinding("ToolTipText", objSkill, nameof(Skill.HtmlSkillToolTip));

                btnAttribute.DoOneWayDataBinding("Text", objSkill, nameof(Skill.DisplayAttribute));

                SkillControl2_RefreshPoolTooltipAndDisplay();

                // Creating controls outside of the designer saves on handles if the controls would be invisible anyway
                if (objSkill.AllowDelete) // For active skills, can only change by going from Create to Career mode, so no databinding necessary
                {
                    cmdDelete = new Button
                    {
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        Dock = DockStyle.Fill,
                        Margin = new Padding(3, 0, 3, 0),
                        Name = "cmdDelete",
                        Tag = "String_Delete",
                        Text = "Delete",
                        UseVisualStyleBackColor = true
                    };
                    cmdDelete.Click += cmdDelete_Click;
                    tlpRight.Controls.Add(cmdDelete, 4, 0);
                }

                if (objSkill.CharacterObject.Created)
                {
                    int intMinimumSize;
                    using (Graphics g = CreateGraphics())
                        intMinimumSize = (int)(25 * g.DpiX / 96.0f);
                    lblCareerRating = new Label
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        MinimumSize = new Size(intMinimumSize, 0),
                        Name = "lblCareerRating",
                        Text = "00",
                        TextAlign = ContentAlignment.MiddleRight
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

                    lblCareerRating.DoOneWayDataBinding("Text", objSkill, nameof(Skill.Rating));
                    btnCareerIncrease.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanUpgradeCareer));
                    btnCareerIncrease.DoOneWayDataBinding("ToolTipText", objSkill, nameof(Skill.UpgradeToolTip));

                    tlpMain.Controls.Add(lblCareerRating, 2, 0);
                    tlpMain.Controls.Add(btnCareerIncrease, 3, 0);

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
                    lblCareerSpec = new Label
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        Name = "lblCareerSpec",
                        Text = "[Specializations]",
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    lblCareerSpec.DoOneWayDataBinding("Text", objSkill, nameof(Skill.CurrentDisplaySpecialization));
                    btnAddSpec.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanAffordSpecialization));
                    btnAddSpec.DoOneWayDataBinding("Visible", objSkill, nameof(Skill.CanHaveSpecs));
                    btnAddSpec.DoOneWayDataBinding("ToolTipText", objSkill, nameof(Skill.AddSpecToolTip));

                    tlpRight.Controls.Add(lblCareerSpec, 0, 0);
                    tlpRight.Controls.Add(btnAddSpec, 1, 0);

                    List<ListItem> lstAttributeItems = new List<ListItem>(AttributeSection.AttributeStrings.Count);
                    foreach (string strLoopAttribute in AttributeSection.AttributeStrings)
                    {
                        if (strLoopAttribute == "MAGAdept")
                        {
                            if (!objSkill.CharacterObject.Settings.MysAdeptSecondMAGAttribute)
                                continue;
                            lstAttributeItems.Add(new ListItem(strLoopAttribute, LanguageManager.MAGAdeptString()));
                        }
                        else
                        {
                            string strAttributeShort = LanguageManager.GetString(
                                "String_Attribute" + strLoopAttribute + "Short", GlobalSettings.Language, false);
                            lstAttributeItems.Add(new ListItem(strLoopAttribute,
                                !string.IsNullOrEmpty(strAttributeShort) ? strAttributeShort : strLoopAttribute));
                        }
                    }

                    cboSelectAttribute = new ElasticComboBox
                    {
                        Dock = DockStyle.Fill,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        FormattingEnabled = true,
                        Margin = new Padding(3, 0, 3, 0),
                        Name = "cboSelectAttribute"
                    };
                    cboSelectAttribute.BeginUpdate();
                    cboSelectAttribute.PopulateWithListItems(lstAttributeItems);
                    cboSelectAttribute.SelectedValue = _objSkill.AttributeObject.Abbrev;
                    cboSelectAttribute.EndUpdate();
                    cboSelectAttribute.DropDownClosed += cboSelectAttribute_Closed;
                    pnlAttributes.Controls.Add(cboSelectAttribute);
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

                    // Trick to make it seem like the button is a label (+ onclick method not doing anything in Create mode)
                    btnAttribute.FlatAppearance.MouseDownBackColor = Color.Transparent;
                    btnAttribute.FlatAppearance.MouseOverBackColor = Color.Transparent;

                    nudSkill.DoOneWayDataBinding("Visible", objSkill.CharacterObject,
                        nameof(objSkill.CharacterObject.EffectiveBuildMethodUsesPriorityTables));
                    nudSkill.DoDataBinding("Value", objSkill, nameof(Skill.Base));
                    nudSkill.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.BaseUnlocked));
                    nudSkill.InterceptMouseWheel = GlobalSettings.InterceptMode;
                    nudKarma.DoDataBinding("Value", objSkill, nameof(Skill.Karma));
                    nudKarma.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.KarmaUnlocked));
                    nudKarma.InterceptMouseWheel = GlobalSettings.InterceptMode;

                    tlpMain.Controls.Add(nudSkill, 2, 0);
                    tlpMain.Controls.Add(nudKarma, 3, 0);

                    if (objSkill.IsExoticSkill)
                    {
                        lblCareerSpec = new Label
                        {
                            Anchor = AnchorStyles.Left,
                            AutoSize = true,
                            Margin = new Padding(3, 6, 3, 6),
                            Name = "lblCareerSpec",
                            Text = "[Specializations]",
                            TextAlign = ContentAlignment.MiddleLeft
                        };
                        lblCareerSpec.DoOneWayDataBinding("Text", objSkill, nameof(Skill.CurrentDisplaySpecialization));
                        tlpRight.Controls.Add(lblCareerSpec, 0, 0);
                    }
                    else
                    {
                        cboSpec = new ElasticComboBox
                        {
                            Anchor = AnchorStyles.Left | AnchorStyles.Right,
                            AutoCompleteMode = AutoCompleteMode.Suggest,
                            FormattingEnabled = true,
                            Margin = new Padding(3, 0, 3, 0),
                            Name = "cboSpec",
                            Sorted = true,
                            TabStop = false
                        };
                        cboSpec.BeginUpdate();
                        cboSpec.PopulateWithListItems(objSkill.CGLSpecializations);
                        cboSpec.EndUpdate();
                        cboSpec.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanHaveSpecs));
                        cboSpec.Text = objSkill.CurrentDisplaySpecialization;
                        cboSpec.TextChanged += cboSpec_TextChanged;
                        _blnUpdatingSpec = false;
                        _tmrSpecChangeTimer = new Timer { Interval = 1000 };
                        _tmrSpecChangeTimer.Tick += SpecChangeTimer_Tick;
                        chkKarma = new ColorableCheckBox(components)
                        {
                            Anchor = AnchorStyles.Left,
                            AutoSize = true,
                            DefaultColorScheme = true,
                            Margin = new Padding(3, 4, 3, 4),
                            Name = "chkKarma",
                            UseVisualStyleBackColor = true
                        };
                        chkKarma.DoOneWayDataBinding("Visible", objSkill.CharacterObject,
                            nameof(objSkill.CharacterObject.EffectiveBuildMethodUsesPriorityTables));
                        chkKarma.DoDataBinding("Checked", objSkill, nameof(Skill.BuyWithKarma));
                        chkKarma.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanHaveSpecs));
                        tlpRight.Controls.Add(cboSpec, 0, 0);
                        tlpRight.Controls.Add(chkKarma, 1, 0);

                        // Hacky way of fixing a weird UI issue caused by items of a combobox only being populated from the DataSource after the combobox is added
                        _blnUpdatingSpec = true;
                        cboSpec.Text = objSkill.CurrentDisplaySpecialization;
                        _blnUpdatingSpec = false;
                    }
                }

                this.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.Enabled));
                this.DoOneWayDataBinding("BackColor", objSkill, nameof(Skill.PreferredControlColor));

                SkillControl2_DpiChangedAfterParent(null, EventArgs.Empty);
                this.UpdateLightDarkMode();
                this.TranslateWinForm(string.Empty, false);

                foreach (ToolStripItem tssItem in cmsSkillLabel.Items)
                {
                    tssItem.UpdateLightDarkMode();
                    tssItem.TranslateToolStripItemsRecursively();
                }
            }
            finally
            {
                _blnLoading = false;
                tlpRight.ResumeLayout();
                tlpMain.ResumeLayout();
                pnlAttributes.ResumeLayout();
                ResumeLayout(true);
                _objSkill.PropertyChanged += Skill_PropertyChanged;
            }
        }

        private void Skill_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (_blnLoading)
                return;

            bool blnUpdateAll = false;
            //I learned something from this but i'm not sure it is a good solution
            //scratch that, i'm sure it is a bad solution. (Tooltip manager from tooltip, properties from reflection?

            //if name of changed is null it does magic to change all, otherwise it only does one.
            switch (propertyChangedEventArgs?.PropertyName)
            {
                case null:
                    blnUpdateAll = true;
                    goto case nameof(Skill.DisplayPool);
                case nameof(Skill.DisplayPool):
                    SkillControl2_RefreshPoolTooltipAndDisplay();
                    if (blnUpdateAll)
                        goto case nameof(Skill.Default);
                    break;

                case nameof(Skill.Default):
                    lblName.Font = !_objSkill.Default ? _fntItalicName : _fntNormalName;
                    if (blnUpdateAll)
                        goto case nameof(Skill.DefaultAttribute);
                    break;

                case nameof(Skill.DefaultAttribute):
                    if (cboSelectAttribute != null)
                    {
                        cboSelectAttribute.SelectedValue = _objSkill.AttributeObject.Abbrev;
                        cboSelectAttribute_Closed(this, EventArgs.Empty);
                    }
                    else
                    {
                        AttributeActive = _objSkill.AttributeObject;
                    }
                    if (blnUpdateAll)
                        goto case nameof(Skill.TopMostDisplaySpecialization);
                    break;

                case nameof(Skill.TopMostDisplaySpecialization):
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
                    if (blnUpdateAll)
                        goto case nameof(Skill.CGLSpecializations);
                    break;

                case nameof(Skill.CGLSpecializations):
                    if (cboSpec?.Visible == true)
                    {
                        string strOldSpec = cboSpec.Text;
                        IReadOnlyList<ListItem> lstSpecializations = _objSkill.CGLSpecializations;
                        cboSpec.QueueThreadSafe(() =>
                        {
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
                        });
                    }
                    break;
            }
        }

        private void Attribute_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (_blnLoading)
                return;

            switch (propertyChangedEventArgs?.PropertyName)
            {
                case null:
                case nameof(CharacterAttrib.Abbrev):
                case nameof(CharacterAttrib.TotalValue):
                    SkillControl2_RefreshPoolTooltipAndDisplay();
                    break;
            }
        }

        private void btnCareerIncrease_Click(object sender, EventArgs e)
        {
            string confirmstring = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpense"),
                    _objSkill.CurrentDisplayName, _objSkill.Rating + 1, _objSkill.UpgradeKarmaCost);

            if (!CommonFunctions.ConfirmKarmaExpense(confirmstring))
                return;

            _objSkill.Upgrade();
        }

        private void btnAddSpec_Click(object sender, EventArgs e)
        {
            int price = _objSkill.CharacterObject.Settings.KarmaSpecialization;

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

            string confirmstring = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpenseSkillSpecialization"), price);

            if (!CommonFunctions.ConfirmKarmaExpense(confirmstring))
                return;

            using (frmSelectSpec selectForm = new frmSelectSpec(_objSkill))
            {
                selectForm.ShowDialog(Program.MainForm);

                if (selectForm.DialogResult != DialogResult.OK)
                    return;

                _objSkill.AddSpecialization(selectForm.SelectedItem);
            }

            if (ParentForm is CharacterShared frmParent)
                frmParent.IsCharacterUpdateRequested = true;
        }

        private void btnAttribute_Click(object sender, EventArgs e)
        {
            if (cboSelectAttribute != null)
            {
                btnAttribute.Visible = false;
                cboSelectAttribute.Visible = true;
                cboSelectAttribute.DroppedDown = true;
            }
        }

        private void cboSelectAttribute_Closed(object sender, EventArgs e)
        {
            btnAttribute.Visible = true;
            cboSelectAttribute.Visible = false;
            AttributeActive = _objSkill.CharacterObject.GetAttribute((string)cboSelectAttribute.SelectedValue);
            btnAttribute.Text = cboSelectAttribute.Text;
        }

        private CharacterAttrib AttributeActive
        {
            get => _objAttributeActive;
            set
            {
                if (_objAttributeActive == value)
                    return;
                if (_objAttributeActive != null)
                    _objAttributeActive.PropertyChanged -= Attribute_PropertyChanged;
                _objAttributeActive = value;
                if (_objAttributeActive != null)
                    _objAttributeActive.PropertyChanged += Attribute_PropertyChanged;
                btnAttribute.QueueThreadSafe(() =>
                    btnAttribute.Font = _objAttributeActive == _objSkill.AttributeObject ? _fntNormal : _fntItalic);
                SkillControl2_RefreshPoolTooltipAndDisplay();
                CustomAttributeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler CustomAttributeChanged;

        public bool CustomAttributeSet => AttributeActive != _objSkill.AttributeObject;

        [UsedImplicitly]
        public int NameWidth => lblName.PreferredWidth + lblName.Margin.Right + pnlAttributes.Margin.Left + pnlAttributes.Width;

        [UsedImplicitly]
        public int NudSkillWidth => nudSkill?.Visible == true ? nudSkill.Width : 0;

        [UsedImplicitly]
        public void ResetSelectAttribute(object sender, EventArgs e)
        {
            if (!CustomAttributeSet)
                return;
            if (cboSelectAttribute != null)
            {
                cboSelectAttribute.SelectedValue = _objSkill.AttributeObject.Abbrev;
                cboSelectAttribute_Closed(sender, e);
            }
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            if (_objSkill.AllowDelete)
            {
                if (!CommonFunctions.ConfirmDelete(LanguageManager.GetString(_objSkill.IsExoticSkill ? "Message_DeleteExoticSkill" : "Message_DeleteSkill")))
                    return;
                _objSkill.UnbindSkill();
                _objSkill.CharacterObject.SkillsSection.Skills.Remove(_objSkill);
            }
        }

        private void tsSkillLabelNotes_Click(object sender, EventArgs e)
        {
            using (frmNotes frmItemNotes = new frmNotes(_objSkill.Notes, _objSkill.NotesColor))
            {
                frmItemNotes.ShowDialog(this);
                if (frmItemNotes.DialogResult != DialogResult.OK)
                    return;
                _objSkill.Notes = frmItemNotes.Notes;
            }
        }

        private void lblName_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdf(_objSkill.Source + ' ' + _objSkill.DisplayPage(GlobalSettings.Language), _objSkill.CharacterObject);
        }

        [UsedImplicitly]
        public void MoveControls(int intNewNameWidth)
        {
            lblName.MinimumSize = new Size(intNewNameWidth - lblName.Margin.Right - pnlAttributes.Margin.Left - pnlAttributes.Width, lblName.MinimumSize.Height);
        }

        private void UnbindSkillControl()
        {
            _tmrSpecChangeTimer?.Dispose();
            _objSkill.PropertyChanged -= Skill_PropertyChanged;
            AttributeActive.PropertyChanged -= Attribute_PropertyChanged;

            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

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

        private void SkillControl2_DpiChangedAfterParent(object sender, EventArgs e)
        {
            using (Graphics g = CreateGraphics())
            {
                pnlAttributes.MinimumSize = new Size((int)(40 * g.DpiX / 96.0f), 0);
                if (lblCareerRating != null)
                    lblCareerRating.MinimumSize = new Size((int)(25 * g.DpiX / 96.0f), 0);
                lblModifiedRating.MinimumSize = new Size((int)(50 * g.DpiX / 96.0f), 0);
            }
        }

        /// <summary>
        /// Refreshes the Tooltip and Displayed Dice Pool. Can be used in another Thread
        /// </summary>
        private void SkillControl2_RefreshPoolTooltipAndDisplay()
        {
            string backgroundCalcPool = _objSkill.DisplayOtherAttribute(AttributeActive.Abbrev);
            lblModifiedRating.QueueThreadSafe(() => lblModifiedRating.Text = backgroundCalcPool);
            string backgroundCalcTooltip = _objSkill.CompileDicepoolTooltip(AttributeActive.Abbrev);
            lblModifiedRating.QueueThreadSafe(() => lblModifiedRating.ToolTipText = backgroundCalcTooltip);
        }

        // Hacky solutions to data binding causing cursor to reset whenever the user is typing something in: have text changes start a timer, and have a 1s delay in the timer update fire the text update
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

        private void SpecChangeTimer_Tick(object sender, EventArgs e)
        {
            _tmrSpecChangeTimer.Stop();
            _blnUpdatingSpec = true;
            _objSkill.TopMostDisplaySpecialization = cboSpec.Text;
            _blnUpdatingSpec = false;
        }
    }
}
