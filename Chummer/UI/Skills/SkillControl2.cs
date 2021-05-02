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
using System.Threading;
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
        private readonly Skill _objSkill;
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
            InitializeComponent();
            SkillControl2_DpiChangedAfterParent(null, EventArgs.Empty);
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            foreach (ToolStripItem tssItem in cmsSkillLabel.Items)
            {
                tssItem.UpdateLightDarkMode();
                tssItem.TranslateToolStripItemsRecursively();
            }

            SuspendLayout();

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

            var threadRefreshing = new Thread(SkillControl2_RefreshPoolTooltipAndDisplay) {IsBackground = true};
            threadRefreshing.Start();

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
                cmdDelete.UpdateLightDarkMode();
                cmdDelete.TranslateWinForm();
                tlpRight.Controls.Add(cmdDelete, 4, 0);
            }

            int intMinimumSize;
            using (Graphics g = CreateGraphics())
                intMinimumSize = (int)(25 * g.DpiX / 96.0f);

            if (objSkill.CharacterObject.Created)
            {
                lblCareerRating = new Label
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
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
                    Image = Resources.add,
                    Margin = new Padding(3, 0, 3, 0),
                    Name = "btnCareerIncrease",
                    Padding = new Padding(1),
                    UseVisualStyleBackColor = true
                };
                btnCareerIncrease.Click += btnCareerIncrease_Click;

                lblCareerRating.DoOneWayDataBinding("Text", objSkill, nameof(Skill.Rating));
                btnCareerIncrease.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanUpgradeCareer));
                btnCareerIncrease.DoOneWayDataBinding("ToolTipText", objSkill, nameof(Skill.UpgradeToolTip));

                lblCareerRating.UpdateLightDarkMode();
                lblCareerRating.TranslateWinForm();
                btnCareerIncrease.UpdateLightDarkMode();
                btnCareerIncrease.TranslateWinForm();
                tlpMain.Controls.Add(lblCareerRating, 2, 0);
                tlpMain.Controls.Add(btnCareerIncrease, 3, 0);

                btnAddSpec = new ButtonWithToolTip
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Image = Resources.add,
                    Margin = new Padding(3, 0, 3, 0),
                    Name = "btnAddSpec",
                    Padding = new Padding(1),
                    UseVisualStyleBackColor = true
                };
                btnAddSpec.Click += btnAddSpec_Click;
                lblCareerSpec = new Label
                {
                    Anchor = AnchorStyles.Left,
                    AutoSize = true,
                    Name = "lblCareerSpec",
                    Text = "[Specializations]",
                    TextAlign = ContentAlignment.MiddleLeft
                };
                lblCareerSpec.DoOneWayDataBinding("Text", objSkill, nameof(Skill.CurrentDisplaySpecialization));
                btnAddSpec.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanAffordSpecialization));
                btnAddSpec.DoDatabinding("Visible", objSkill, nameof(Skill.CanHaveSpecs));
                btnAddSpec.DoOneWayDataBinding("ToolTipText", objSkill, nameof(Skill.AddSpecToolTip));

                lblCareerSpec.UpdateLightDarkMode();
                lblCareerSpec.TranslateWinForm();
                btnAddSpec.UpdateLightDarkMode();
                btnAddSpec.TranslateWinForm();
                tlpRight.Controls.Add(lblCareerSpec, 0, 0);
                tlpRight.Controls.Add(btnAddSpec, 1, 0);

                List<ListItem> lstAttributeItems = new List<ListItem>(AttributeSection.AttributeStrings.Count);
                foreach (string strLoopAttribute in AttributeSection.AttributeStrings)
                {
                    if (strLoopAttribute == "MAGAdept")
                    {
                        if (!objSkill.CharacterObject.Options.MysAdeptSecondMAGAttribute)
                            continue;
                        lstAttributeItems.Add(new ListItem(strLoopAttribute, LanguageManager.MAGAdeptString()));
                    }
                    else
                    {
                        string strAttributeShort = LanguageManager.GetString("String_Attribute" + strLoopAttribute + "Short", GlobalOptions.Language, false);
                        lstAttributeItems.Add(new ListItem(strLoopAttribute, !string.IsNullOrEmpty(strAttributeShort) ? strAttributeShort : strLoopAttribute));
                    }
                }

                cboSelectAttribute = new ElasticComboBox
                {
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    FormattingEnabled = true,
                    Margin = new Padding(3, 0, 3, 0),
                    Name = "cboSelectAttribute"
                };
                cboSelectAttribute.DropDownClosed += cboSelectAttribute_Closed;
                cboSelectAttribute.BeginUpdate();
                cboSelectAttribute.DataSource = null;
                cboSelectAttribute.DataSource = lstAttributeItems;
                cboSelectAttribute.DisplayMember = nameof(ListItem.Name);
                cboSelectAttribute.ValueMember = nameof(ListItem.Value);
                cboSelectAttribute.SelectedValue = _objSkill.AttributeObject.Abbrev;
                cboSelectAttribute.EndUpdate();
                cboSelectAttribute.UpdateLightDarkMode();
                cboSelectAttribute.TranslateWinForm();
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
                    Maximum = new decimal(new[] {99, 0, 0, 0}),
                    Name = "nudSkill"
                };
                nudKarma = new NumericUpDownEx
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
                    InterceptMouseWheel = NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver,
                    Margin = new Padding(3, 2, 3, 2),
                    Maximum = new decimal(new[] {99, 0, 0, 0}),
                    Name = "nudKarma"
                };

                // Trick to make it seem like the button is a label (+ onclick method not doing anything in Create mode)
                btnAttribute.FlatAppearance.MouseDownBackColor = Color.Transparent;
                btnAttribute.FlatAppearance.MouseOverBackColor = Color.Transparent;

                nudSkill.DoDatabinding("Visible", objSkill.CharacterObject, nameof(objSkill.CharacterObject.EffectiveBuildMethodUsesPriorityTables));
                nudSkill.DoDatabinding("Value", objSkill, nameof(Skill.Base));
                nudSkill.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.BaseUnlocked));
                nudSkill.InterceptMouseWheel = GlobalOptions.InterceptMode;
                nudKarma.DoDatabinding("Value", objSkill, nameof(Skill.Karma));
                nudKarma.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.KarmaUnlocked));
                nudKarma.InterceptMouseWheel = GlobalOptions.InterceptMode;

                nudSkill.UpdateLightDarkMode();
                nudSkill.TranslateWinForm();
                nudKarma.UpdateLightDarkMode();
                nudKarma.TranslateWinForm();
                tlpMain.Controls.Add(nudSkill, 2, 0);
                tlpMain.Controls.Add(nudKarma, 3, 0);

                if (objSkill.IsExoticSkill)
                {
                    lblCareerSpec = new Label
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        Name = "lblCareerSpec",
                        Text = "[Specializations]",
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    lblCareerSpec.DoOneWayDataBinding("Text", objSkill, nameof(Skill.CurrentDisplaySpecialization));
                    lblCareerSpec.UpdateLightDarkMode();
                    lblCareerSpec.TranslateWinForm();
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
                    cboSpec.DataSource = null;
                    cboSpec.DataSource = objSkill.CGLSpecializations;
                    cboSpec.DisplayMember = nameof(ListItem.Name);
                    cboSpec.ValueMember = nameof(ListItem.Value);
                    cboSpec.SelectedIndex = -1;
                    cboSpec.DoDatabinding("Text", objSkill, nameof(Skill.Specialization));
                    cboSpec.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanHaveSpecs));
                    cboSpec.EndUpdate();
                    cboSpec.UpdateLightDarkMode();
                    cboSpec.TranslateWinForm();
                    chkKarma = new ColorableCheckBox(components)
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        DefaultColorScheme = true,
                        Margin = new Padding(3, 0, 3, 0),
                        Name = "chkKarma",
                        UseVisualStyleBackColor = true
                    };
                    chkKarma.DoDatabinding("Visible", objSkill.CharacterObject, nameof(objSkill.CharacterObject.EffectiveBuildMethodUsesPriorityTables));
                    chkKarma.DoDatabinding("Checked", objSkill, nameof(Skill.BuyWithKarma));
                    chkKarma.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanHaveSpecs));
                    chkKarma.UpdateLightDarkMode();
                    chkKarma.TranslateWinForm();
                    tlpRight.Controls.Add(cboSpec, 0, 0);
                    tlpRight.Controls.Add(chkKarma, 1, 0);
                }
            }

            this.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.Enabled));
            this.DoOneWayDataBinding("BackColor", objSkill, nameof(Skill.PreferredControlColor));

            ResumeLayout(true);

            _blnLoading = false;

            _objSkill.PropertyChanged += Skill_PropertyChanged;
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
                    var threadRefreshing = new Thread(SkillControl2_RefreshPoolTooltipAndDisplay) { IsBackground = true };
                    threadRefreshing.Start();
                    if (blnUpdateAll)
                        goto case nameof(Skill.Default);
                    break;
                case nameof(Skill.Default):
                    lblName.Font = !_objSkill.Default ? _fntItalicName : _fntNormalName;
                    if (blnUpdateAll)
                        goto case nameof(Skill.CGLSpecializations);
                    break;
                case nameof(Skill.CGLSpecializations):
                    if (cboSpec?.Visible == true)
                    {
                        string strOldSpec = cboSpec.Text;
                        cboSpec.BeginUpdate();
                        cboSpec.DataSource = null;
                        cboSpec.DataSource = _objSkill.CGLSpecializations;
                        cboSpec.DisplayMember = nameof(ListItem.Name);
                        cboSpec.ValueMember = nameof(ListItem.Value);
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

                    var threadRefreshing = new Thread(SkillControl2_RefreshPoolTooltipAndDisplay) { IsBackground = true };
                    threadRefreshing.Start();

                    break;
            }
        }
        private void btnCareerIncrease_Click(object sender, EventArgs e)
        {
            string confirmstring = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpense"),
                    _objSkill.CurrentDisplayName, _objSkill.Rating + 1, _objSkill.UpgradeKarmaCost);

            if (!CommonFunctions.ConfirmKarmaExpense(confirmstring))
                return;

            _objSkill.Upgrade();
        }

        private void btnAddSpec_Click(object sender, EventArgs e)
        {
            int price = _objSkill.CharacterObject.Options.KarmaSpecialization;

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
                    if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCost)
                        decExtraSpecCost += objLoopImprovement.Value;
                    else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier)
                        decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                }
            }
            if (decSpecCostMultiplier != 1.0m)
                price = (price * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
            else
                price += decExtraSpecCost.StandardRound(); //Spec

            string confirmstring = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpenseSkillSpecialization"), price);

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
            _objAttributeActive.PropertyChanged -= Attribute_PropertyChanged;
            _objAttributeActive = _objSkill.CharacterObject.GetAttribute((string) cboSelectAttribute.SelectedValue);
            _objAttributeActive.PropertyChanged += Attribute_PropertyChanged;

            btnAttribute.Font = _objAttributeActive == _objSkill.AttributeObject ? _fntNormal : _fntItalic;
            btnAttribute.Text = cboSelectAttribute.Text;
            Attribute_PropertyChanged(sender, new PropertyChangedEventArgs(nameof(CharacterAttrib.Abbrev)));
            CustomAttributeChanged?.Invoke(sender, e);
        }

        public event EventHandler CustomAttributeChanged;

        public bool CustomAttributeSet => _objAttributeActive != _objSkill.AttributeObject;

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
                _objSkill.CharacterObject.SkillsSection.SkillsDictionary.Remove(_objSkill.DictionaryKey);
            }
        }

        private void tsSkillLabelNotes_Click(object sender, EventArgs e)
        {
            using (frmNotes frmItemNotes = new frmNotes(_objSkill.Notes))
            {
                frmItemNotes.ShowDialog(this);
                if (frmItemNotes.DialogResult != DialogResult.OK)
                    return;
                _objSkill.Notes = frmItemNotes.Notes;
            }
        }

        private void lblName_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdf(_objSkill.Source + ' ' + _objSkill.DisplayPage(GlobalOptions.Language), _objSkill.CharacterObject);
        }

        [UsedImplicitly]
        public void MoveControls(int intNewNameWidth)
        {
            lblName.MinimumSize = new Size(intNewNameWidth - lblName.Margin.Right - pnlAttributes.Margin.Left - pnlAttributes.Width, lblName.MinimumSize.Height);
        }

        private void UnbindSkillControl()
        {
            _objSkill.PropertyChanged -= Skill_PropertyChanged;
            _objAttributeActive.PropertyChanged -= Attribute_PropertyChanged;

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

        private void SkillControl2_DpiChangedAfterParent(object sender, EventArgs e)
        {
            using (Graphics g = CreateGraphics())
            {
                pnlAttributes.MinimumSize = new Size((int) (40 * g.DpiX / 96.0f), 0);
                if (lblCareerRating != null)
                    lblCareerRating.MinimumSize = new Size((int) (25 * g.DpiX / 96.0f), 0);
                lblModifiedRating.MinimumSize = new Size((int) (50 * g.DpiX / 96.0f), 0);
            }
        }


        /// <summary>
        /// Refreshes the Tooltip and Displayed Dice Pool. Can be used in another Thread
        /// </summary>
        private void SkillControl2_RefreshPoolTooltipAndDisplay()
        {
            if (_blnLoading || lblModifiedRating.Text != 0.ToString(GlobalOptions.CultureInfo))
            {
                string backgroundCalcPool = _objSkill.DisplayOtherAttribute(_objAttributeActive.Abbrev);
                lblModifiedRating.DoThreadSafe(() => lblModifiedRating.Text = backgroundCalcPool);
            }

            string backgroundCalcTooltip = _objSkill.CompileDicepoolTooltip(_objAttributeActive.Abbrev);
            lblModifiedRating.DoThreadSafe(() => lblModifiedRating.ToolTipText = backgroundCalcTooltip);

        }
    }
}
