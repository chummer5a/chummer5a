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
using Chummer.Annotations;
using Chummer.Backend.Skills;
using Chummer.Backend.Attributes;

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

        public SkillControl2(Skill objSkill)
        {
            if (objSkill == null)
                return;
            _objSkill = objSkill;
            _objAttributeActive = objSkill.AttributeObject;
            InitializeComponent();
            SuspendLayout();

            this.TranslateWinForm();

            foreach (ToolStripItem tssItem in cmsSkillLabel.Items)
            {
                tssItem.TranslateToolStripItemsRecursively();
            }

            this.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.Enabled));

            //Display
            _fntNormalName = lblName.Font;
            _fntItalicName = new Font(_fntNormalName, FontStyle.Italic);
            _fntNormal = btnAttribute.Font;
            _fntItalic = new Font(_fntNormal, FontStyle.Italic);

            this.DoOneWayDataBinding("BackColor", objSkill, nameof(Skill.PreferredControlColor));

            if (!_objSkill.Default)
                lblName.Font = _fntItalicName;
            lblName.DoOneWayDataBinding("Text", objSkill, nameof(Skill.CurrentDisplayName));
            lblName.DoOneWayDataBinding("ForeColor", objSkill, nameof(Skill.PreferredColor));
            lblName.DoOneWayDataBinding("ToolTipText", objSkill, nameof(Skill.HtmlSkillToolTip));

            btnAttribute.DoOneWayDataBinding("Text", objSkill, nameof(Skill.DisplayAttribute));

            lblModifiedRating.Text = objSkill.DisplayOtherAttribute(_objAttributeActive.TotalValue, _objAttributeActive.Abbrev);
            lblModifiedRating.ToolTipText = objSkill.CompileDicepoolTooltip(_objAttributeActive.Abbrev);

            if (objSkill.AllowDelete) // For active skills, can only change by going from Create to Career mode, so no databinding necessary
                cmdDelete.Visible = true;

            if (objSkill.CharacterObject.Created)
            {
                flpButtonsCreate.Visible = false;
                tlpSpecsCreate.Visible = false;

                lblCareerRating.DoOneWayDataBinding("Text", objSkill, nameof(Skill.Rating));
                btnCareerIncrease.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanUpgradeCareer));
                btnCareerIncrease.DoOneWayDataBinding("ToolTipText", objSkill, nameof(Skill.UpgradeToolTip));

                lblCareerSpec.DoOneWayDataBinding("Text", objSkill, nameof(Skill.CurrentDisplaySpecialization));
                btnAddSpec.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanAffordSpecialization));
                btnAddSpec.DoOneWayDataBinding("Visible", objSkill, nameof(Skill.CanHaveSpecs));
                btnAddSpec.DoOneWayDataBinding("ToolTipText", objSkill, nameof(Skill.AddSpecToolTip));

                List<ListItem> lstAttributeItems = new List<ListItem>();
                foreach (string strLoopAttribute in AttributeSection.AttributeStrings)
                {
                    string strAttributeShort = LanguageManager.GetString("String_Attribute" + strLoopAttribute + "Short", GlobalOptions.Language, false);
                    lstAttributeItems.Add(new ListItem(strLoopAttribute, !string.IsNullOrEmpty(strAttributeShort) ? strAttributeShort : strLoopAttribute));
                }
                cboSelectAttribute.BeginUpdate();
                cboSelectAttribute.DataSource = null;
                cboSelectAttribute.DisplayMember = nameof(ListItem.Name);
                cboSelectAttribute.ValueMember = nameof(ListItem.Value);
                cboSelectAttribute.DataSource = lstAttributeItems;
                cboSelectAttribute.SelectedValue = _objSkill.AttributeObject.Abbrev;
                cboSelectAttribute.EndUpdate();
            }
            else
            {
                flpButtonsCareer.Visible = false;

                // Trick to make it seem like the button is a label (+ onclick method not doing anything in Create mode)
                btnAttribute.FlatAppearance.MouseDownBackColor = btnAttribute.BackColor;
                btnAttribute.FlatAppearance.MouseOverBackColor = btnAttribute.BackColor;

                nudSkill.DoDatabinding("Value", objSkill, nameof(Skill.Base));
                nudSkill.DoOneWayDataBinding("Visible", objSkill.CharacterObject, nameof(objSkill.CharacterObject.BuildMethodHasSkillPoints));
                nudSkill.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.BaseUnlocked));
                nudSkill.DoOneWayDataBinding("InterceptMouseWheel", objSkill.CharacterObject.Options, nameof(CharacterOptions.InterceptMode));
                nudKarma.DoOneWayDataBinding("Value", objSkill, nameof(Skill.Karma));
                nudKarma.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.KarmaUnlocked));
                nudKarma.DoOneWayDataBinding("InterceptMouseWheel", objSkill.CharacterObject.Options, nameof(CharacterOptions.InterceptMode));

                if (objSkill.IsExoticSkill)
                {
                    tlpSpecsCreate.Visible = false;
                    btnAddSpec.Visible = false;
                    lblCareerSpec.DoOneWayDataBinding("Text", objSkill, nameof(Skill.CurrentDisplaySpecialization));
                }
                else
                {
                    tlpSpecsCareer.Visible = false;
                    cboSpec.BeginUpdate();
                    cboSpec.DataSource = null;
                    cboSpec.DisplayMember = nameof(ListItem.Name);
                    cboSpec.ValueMember = nameof(ListItem.Value);
                    cboSpec.DataSource = objSkill.CGLSpecializations;
                    cboSpec.SelectedIndex = -1;
                    cboSpec.DoDatabinding("Text", objSkill, nameof(Skill.Specialization));
                    cboSpec.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanHaveSpecs));
                    cboSpec.EndUpdate();
                }

                chkKarma.DoOneWayDataBinding("Visible", objSkill.CharacterObject, nameof(objSkill.CharacterObject.BuildMethodHasSkillPoints));
                chkKarma.DoDatabinding("Checked", objSkill, nameof(Skill.BuyWithKarma));
                chkKarma.DoOneWayDataBinding("Enabled", objSkill, nameof(Skill.CanHaveSpecs));
            }

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
                    lblModifiedRating.Text = _objSkill.DisplayOtherAttribute(_objAttributeActive.TotalValue, _objAttributeActive.Abbrev);
                    lblModifiedRating.ToolTipText = _objSkill.CompileDicepoolTooltip(_objAttributeActive.Abbrev);
                    if (blnUpdateAll)
                        goto case nameof(Skill.Default);
                    break;
                case nameof(Skill.Default):
                    lblName.Font = !_objSkill.Default ? _fntItalicName : _fntNormalName;
                    if (blnUpdateAll)
                        goto case nameof(Skill.CGLSpecializations);
                    break;
                case nameof(Skill.CGLSpecializations):
                    if (cboSpec.Visible)
                    {
                        string strOldSpec = cboSpec.Text;
                        cboSpec.BeginUpdate();
                        cboSpec.DataSource = null;
                        cboSpec.DisplayMember = nameof(ListItem.Name);
                        cboSpec.ValueMember = nameof(ListItem.Value);
                        cboSpec.DataSource = _objSkill.CGLSpecializations;
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
                    lblModifiedRating.Text = _objSkill.DisplayOtherAttribute(_objAttributeActive.TotalValue, _objAttributeActive.Abbrev);
                    lblModifiedRating.ToolTipText = _objSkill.CompileDicepoolTooltip(_objAttributeActive.Abbrev);
                    break;
            }
        }
        private void btnCareerIncrease_Click(object sender, EventArgs e)
        {
            string confirmstring = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpense"),
                    _objSkill.CurrentDisplayName, _objSkill.Rating + 1, _objSkill.UpgradeKarmaCost);

            if (!_objSkill.CharacterObject.ConfirmKarmaExpense(confirmstring))
                return;

            _objSkill.Upgrade();
        }

        private void btnAddSpec_Click(object sender, EventArgs e)
        {
            int price = _objSkill.CharacterObject.Options.KarmaSpecialization;

            int intExtraSpecCost = 0;
            int intTotalBaseRating = _objSkill.TotalBaseRating;
            decimal decSpecCostMultiplier = 1.0m;
            foreach (Improvement objLoopImprovement in _objSkill.CharacterObject.Improvements)
            {
                if (objLoopImprovement.Minimum <= intTotalBaseRating &&
                    (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == _objSkill.CharacterObject.Created || (objLoopImprovement.Condition == "create") != _objSkill.CharacterObject.Created) && objLoopImprovement.Enabled)
                {
                    if (objLoopImprovement.ImprovedName == _objSkill.SkillCategory)
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

            if (!_objSkill.CharacterObject.ConfirmKarmaExpense(confirmstring))
                return;

            using (frmSelectSpec selectForm = new frmSelectSpec(_objSkill))
            {
                selectForm.ShowDialog();

                if (selectForm.DialogResult != DialogResult.OK)
                    return;

                _objSkill.AddSpecialization(selectForm.SelectedItem);
            }

            if (ParentForm is CharacterShared frmParent)
                frmParent.IsCharacterUpdateRequested = true;
        }

        private void btnAttribute_Click(object sender, EventArgs e)
        {
            if (_objSkill.CharacterObject.Created)
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
        public int NameWidth => lblName.PreferredWidth + lblName.Margin.Right + btnAttribute.Margin.Left + btnAttribute.Width;

        [UsedImplicitly]
        public int NudSkillWidth => nudSkill.Visible ? nudSkill.Width : 0;

        [UsedImplicitly]
        public void ResetSelectAttribute(object sender, EventArgs e)
        {
            if (!CustomAttributeSet)
                return;
            cboSelectAttribute.SelectedValue = _objSkill.AttributeObject.Abbrev;
            cboSelectAttribute_Closed(sender, e);
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            if (_objSkill.AllowDelete)
            {
                if (!_objSkill.CharacterObject.ConfirmDelete(LanguageManager.GetString(_objSkill.IsExoticSkill ? "Message_DeleteExoticSkill" : "Message_DeleteSkill")))
                    return;
                _objSkill.UnbindSkill();
                _objSkill.CharacterObject.SkillsSection.Skills.Remove(_objSkill);
                _objSkill.CharacterObject.SkillsSection.SkillsDictionary.Remove(_objSkill.DictionaryKey);
            }
        }

        private void tsSkillLabelNotes_Click(object sender, EventArgs e)
        {
            using (frmNotes frmItemNotes = new frmNotes { Notes = _objSkill.Notes })
            {
                frmItemNotes.ShowDialog(this);
                if (frmItemNotes.DialogResult != DialogResult.OK)
                    return;

                _objSkill.Notes = frmItemNotes.Notes;
            }
        }

        private void lblName_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDF(_objSkill.Source + ' ' + _objSkill.DisplayPage(GlobalOptions.Language));
        }

        [UsedImplicitly]
        public void MoveControls(int intNewNameWidth)
        {
            lblName.MinimumSize = new Size(intNewNameWidth - lblName.Margin.Right - btnAttribute.Margin.Left - btnAttribute.Width, lblName.MinimumSize.Height);
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
    }
}
