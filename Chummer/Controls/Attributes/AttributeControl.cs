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
using System.Linq;
using System.Windows.Forms;
using Chummer.Annotations;
using Chummer.Backend.Attributes;
using Chummer.Properties;

namespace Chummer.UI.Attributes
{
    public partial class AttributeControl : UserControl
    {
        public event EventHandler ValueChanged;
        
        private readonly string _strAttributeName;
        private int _oldBase;
        private int _oldKarma;
        private readonly Character _objCharacter;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly BindingSource _dataSource;

        private readonly NumericUpDownEx nudKarma;
        private readonly NumericUpDownEx nudBase;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ButtonWithToolTip cmdBurnEdge;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ButtonWithToolTip cmdImproveATT;

        public AttributeControl(CharacterAttrib attribute)
        {
            if (attribute == null)
                return;
            _strAttributeName = attribute.Abbrev;
            _objCharacter = attribute.CharacterObject;
            _dataSource = _objCharacter.AttributeSection.GetAttributeBindingByName(AttributeName);

            InitializeComponent();

            SuspendLayout();
            //Display
            lblName.DoOneWayDataBinding("Text", _dataSource, nameof(CharacterAttrib.DisplayNameFormatted));
            lblValue.DoOneWayDataBinding("Text", _dataSource, nameof(CharacterAttrib.DisplayValue));
            lblLimits.DoOneWayDataBinding("Text", _dataSource, nameof(CharacterAttrib.AugmentedMetatypeLimits));
            lblValue.DoOneWayDataBinding("ToolTipText", _dataSource, nameof(CharacterAttrib.ToolTip));
            if (_objCharacter.Created)
            {
                cmdImproveATT = new ButtonWithToolTip
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(1),
                    MinimumSize = new Size(24, 24),
                    ImageDpi96 = Resources.add,
                    ImageDpi192 = Resources.add1,
                    Name = "cmdImproveATT",
                    UseVisualStyleBackColor = true
                };
                cmdImproveATT.Click += cmdImproveATT_Click;
                cmdImproveATT.DoOneWayDataBinding("ToolTipText", _dataSource, nameof(CharacterAttrib.UpgradeToolTip));
                cmdImproveATT.DoOneWayDataBinding("Enabled", _dataSource, nameof(CharacterAttrib.CanUpgradeCareer));
                flpRight.Controls.Add(cmdImproveATT);
                if (AttributeName == "EDG")
                {
                    cmdBurnEdge = new ButtonWithToolTip
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        Padding = new Padding(1),
                        MinimumSize = new Size(24, 24),
                        ImageDpi96 = Resources.fire,
                        ImageDpi192 = Resources.fire1,
                        Name = "cmdBurnEdge",
                        ToolTipText = LanguageManager.GetString("Tip_CommonBurnEdge"),
                        UseVisualStyleBackColor = true
                    };
                    cmdBurnEdge.Click += cmdBurnEdge_Click;
                    flpRight.Controls.Add(cmdBurnEdge);
                }
            }
            else
            {
                while (AttributeObject.KarmaMaximum < 0 && AttributeObject.Base > 0)
                    --AttributeObject.Base;
                // Very rough fix for when Karma values somehow exceed KarmaMaximum after loading in. This shouldn't happen in the first place, but this ad-hoc patch will help fix crashes.
                if (AttributeObject.Karma > AttributeObject.KarmaMaximum)
                    AttributeObject.Karma = AttributeObject.KarmaMaximum;

                nudKarma = new NumericUpDownEx
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
                    InterceptMouseWheel = NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver,
                    Margin = new Padding(3, 0, 3, 0),
                    Maximum = new decimal(new[] { 99, 0, 0, 0 }),
                    MinimumSize = new Size(35, 0),
                    Name = "nudKarma"
                };
                nudKarma.BeforeValueIncrement += nudKarma_BeforeValueIncrement;
                nudKarma.ValueChanged += nudKarma_ValueChanged;
                nudBase = new NumericUpDownEx
                {
                    Anchor = AnchorStyles.Right,
                    AutoSize = true,
                    InterceptMouseWheel = NumericUpDownEx.InterceptMouseWheelMode.WhenMouseOver,
                    Margin = new Padding(3, 0, 3, 0),
                    Maximum = new decimal(new[] { 99, 0, 0, 0 }),
                    MinimumSize = new Size(35, 0),
                    Name = "nudBase"
                };
                nudBase.BeforeValueIncrement += nudBase_BeforeValueIncrement;
                nudBase.ValueChanged += nudBase_ValueChanged;

                nudBase.DoOneWayDataBinding("Visible", _objCharacter, nameof(Character.EffectiveBuildMethodUsesPriorityTables));
                nudBase.DoOneWayDataBinding("Maximum", _dataSource, nameof(CharacterAttrib.PriorityMaximum));
                nudBase.DoDataBinding("Value", _dataSource, nameof(CharacterAttrib.Base));
                nudBase.DoOneWayDataBinding("Enabled", _dataSource, nameof(CharacterAttrib.BaseUnlocked));
                nudBase.InterceptMouseWheel = GlobalSettings.InterceptMode;

                nudKarma.DoOneWayDataBinding("Maximum", _dataSource, nameof(CharacterAttrib.KarmaMaximum));
                nudKarma.DoDataBinding("Value", _dataSource, nameof(CharacterAttrib.Karma));
                nudKarma.InterceptMouseWheel = GlobalSettings.InterceptMode;

                flpRight.Controls.Add(nudKarma);
                flpRight.Controls.Add(nudBase);
            }

            ResumeLayout();

            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        public void UpdateWidths(int intNameWidth, int intNudKarmaWidth, int intValueWidth, int intLimitsWidth)
        {
            tlpMain.SuspendLayout();

            if (intNameWidth >= 0)
            {
                if (lblName.MinimumSize.Width > intNameWidth)
                    lblName.MinimumSize = new Size(intNameWidth, lblName.MinimumSize.Height);
                if (lblName.MaximumSize.Width != intNameWidth)
                    lblName.MaximumSize = new Size(intNameWidth, lblName.MinimumSize.Height);
                if (lblName.MinimumSize.Width < intNameWidth)
                    lblName.MinimumSize = new Size(intNameWidth, lblName.MinimumSize.Height);
            }

            if (nudKarma?.Visible == true && nudBase?.Visible == true && intNudKarmaWidth >= 0)
            {
                nudKarma.Margin = new Padding(
                    nudKarma.Margin.Right + Math.Max(intNudKarmaWidth - nudKarma.Width, 0),
                    nudKarma.Margin.Top,
                    nudKarma.Margin.Right,
                    nudKarma.Margin.Bottom);
            }

            if (intValueWidth >= 0)
            {
                if (lblValue.MinimumSize.Width > intValueWidth)
                    lblValue.MinimumSize = new Size(intValueWidth, lblValue.MinimumSize.Height);
                if (lblValue.MaximumSize.Width != intValueWidth)
                    lblValue.MaximumSize = new Size(intValueWidth, lblValue.MinimumSize.Height);
                if (lblValue.MinimumSize.Width < intValueWidth)
                    lblValue.MinimumSize = new Size(intValueWidth, lblValue.MinimumSize.Height);
            }

            if (intLimitsWidth >= 0)
            {
                if (lblLimits.MinimumSize.Width > intLimitsWidth)
                    lblLimits.MinimumSize = new Size(intLimitsWidth, lblLimits.MinimumSize.Height);
                if (lblLimits.MaximumSize.Width != intLimitsWidth)
                    lblLimits.MaximumSize = new Size(intLimitsWidth, lblLimits.MinimumSize.Height);
                if (lblLimits.MinimumSize.Width < intLimitsWidth)
                    lblLimits.MinimumSize = new Size(intLimitsWidth, lblLimits.MinimumSize.Height);
            }

            tlpMain.ResumeLayout();
        }

        private void UnbindAttributeControl()
        {
            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private void cmdImproveATT_Click(object sender, EventArgs e)
        {
            int intUpgradeKarmaCost = AttributeObject.UpgradeKarmaCost;

            if (intUpgradeKarmaCost == -1) return; //TODO: more descriptive
            if (intUpgradeKarmaCost > _objCharacter.Karma)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughKarma"), LanguageManager.GetString("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string confirmstring = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpense"), AttributeObject.DisplayNameFormatted, AttributeObject.Value + 1, intUpgradeKarmaCost);
            if (!CommonFunctions.ConfirmKarmaExpense(confirmstring))
                return;

            AttributeObject.Upgrade();
            ValueChanged?.Invoke(this, e);
        }

        private void nudBase_ValueChanged(object sender, EventArgs e)
        {
            int intValue = ((NumericUpDownEx)sender).ValueAsInt;
            if (intValue == _oldBase)
                return;
            if (!CanBeMetatypeMax(
                Math.Max(
                    nudKarma.ValueAsInt + AttributeObject.FreeBase + AttributeObject.RawMinimum +
                    AttributeObject.AttributeValueModifiers, AttributeObject.TotalMinimum) + intValue))
            {
                decimal newValue = Math.Max(nudBase.Value - 1, 0);
                if (newValue > nudBase.Maximum)
                {
                    newValue = nudBase.Maximum;
                }
                if (newValue < nudBase.Minimum)
                {
                    newValue = nudBase.Minimum;
                }
                nudBase.Value = newValue;
                return;
            }
            ValueChanged?.Invoke(this, e);
            _oldBase = intValue;
        }

        private void nudKarma_ValueChanged(object sender, EventArgs e)
        {
            int intValue = ((NumericUpDownEx)sender).ValueAsInt;
            if (intValue == _oldKarma)
                return;
            if (!CanBeMetatypeMax(
                Math.Max(
                    nudBase.ValueAsInt + AttributeObject.FreeBase + AttributeObject.RawMinimum +
                    AttributeObject.AttributeValueModifiers, AttributeObject.TotalMinimum) + intValue))
            {
                // It's possible that the attribute maximum was reduced by an improvement, so confirm the appropriate value to bounce up/down to.
                if (_oldKarma > AttributeObject.KarmaMaximum)
                {
                    _oldKarma = AttributeObject.KarmaMaximum - 1;
                }
                if (_oldKarma < 0)
                {
                    decimal newValue = Math.Max(nudBase.Value - _oldKarma, 0);
                    if (newValue > nudBase.Maximum)
                    {
                        newValue = nudBase.Maximum;
                    }
                    if (newValue < nudBase.Minimum)
                    {
                        newValue = nudBase.Minimum;
                    }
                    nudBase.Value = newValue;
                    _oldKarma = 0;
                }
                nudKarma.Value = _oldKarma;
                return;
            }
            ValueChanged?.Invoke(this, e);
            _oldKarma = intValue;
        }

        /// <summary>
        /// Is the attribute allowed to reach the Metatype Maximum?
        /// SR5 66: Characters at character creation may only have 1 Mental or Physical
        /// attribute at their natural maximum limit; the special attributes of Magic, Edge,
        /// and Resonance are not included in this limitation.
        /// </summary>
        private bool CanBeMetatypeMax(int intValue)
        {
            if (_objCharacter.IgnoreRules || AttributeObject.MetatypeCategory == CharacterAttrib.AttributeCategory.Special)
                return true;
            int intTotalMaximum = AttributeObject.TotalMaximum;
            if (intValue < intTotalMaximum || intTotalMaximum == 0)
                return true;
            //TODO: This should be in AttributeSection, but I can't be bothered finagling the option into working.
            //Ideally return 2 or 1, allow for an improvement type to increase or decrease the value.
            int intMaxOtherAttributesAtMax = _objCharacter.Settings.Allow2ndMaxAttribute ? 1 : 0;
            int intNumOtherAttributeAtMax = _objCharacter.AttributeSection.AttributeList.Count(att =>
                att.AtMetatypeMaximum && att.Abbrev != AttributeName && att.MetatypeCategory == CharacterAttrib.AttributeCategory.Standard);

            if (intNumOtherAttributeAtMax <= intMaxOtherAttributesAtMax) return true;
            Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_AttributeMaximum"),
                LanguageManager.GetString("MessageTitle_Attribute"), MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return false;
        }

        public string AttributeName => _strAttributeName;

        private CharacterAttrib _objCachedCharacterAttrib;

        public CharacterAttrib AttributeObject =>
            _objCachedCharacterAttrib ?? (_objCachedCharacterAttrib =
                _objCharacter.AttributeSection.GetAttributeByName(AttributeName));

        [UsedImplicitly]
        public int NameWidth => lblName.PreferredWidth;

        private void cmdBurnEdge_Click(object sender, EventArgs e)
        {
            // Edge cannot go below 1.
            if (AttributeObject.Value <= 0)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CannotBurnEdge"), LanguageManager.GetString("MessageTitle_CannotBurnEdge"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Verify that the user wants to Burn a point of Edge.
            if (Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_BurnEdge"), LanguageManager.GetString("MessageTitle_BurnEdge"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            AttributeObject.Degrade(1);
            ValueChanged?.Invoke(this, e);
        }

        private void nudBase_BeforeValueIncrement(object sender, CancelEventArgs e)
        {
            if (nudBase.Value + Math.Max(nudKarma.Value, 0) != AttributeObject.TotalMaximum || nudKarma.Value == nudKarma.Minimum)
                return;
            if (nudKarma.Value - nudBase.Increment >= 0)
            {
                nudKarma.Value -= nudBase.Increment;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void nudKarma_BeforeValueIncrement(object sender, CancelEventArgs e)
        {
            if (nudBase.Value + nudKarma.Value != AttributeObject.TotalMaximum || nudBase.Value == nudBase.Minimum)
                return;
            if (nudBase.Value - nudKarma.Increment >= 0)
            {
                nudBase.Value -= nudKarma.Increment;
            }
            else
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// I'm not super pleased with how this works, but it's functional so w/e.
        /// The goal is for controls to retain the ability to display tooltips even while disabled. IT DOES NOT WORK VERY WELL.
        /// </summary>
        #region ButtonWithToolTip Visibility workaround

        private ButtonWithToolTip _activeButton;

        protected ButtonWithToolTip ActiveButton
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
    }
}
