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

namespace Chummer.UI.Attributes
{
    public partial class AttributeControl : UserControl
    {
        // ConnectionRatingChanged Event Handler.
        public delegate void ValueChangedHandler(object sender, EventArgs e);
        public event ValueChangedHandler ValueChanged;
        private readonly CharacterAttrib _objAttribute;
        private decimal _oldBase;
        private decimal _oldKarma;
        private readonly Character _objCharacter;
        private readonly BindingSource _dataSource;

        public AttributeControl(CharacterAttrib attribute)
        {
            if (attribute == null)
                return;
            _objAttribute = attribute;
            _objCharacter = attribute.CharacterObject;

            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            _dataSource = _objCharacter.AttributeSection.GetAttributeBindingByName(AttributeName);
            _objCharacter.AttributeSection.PropertyChanged += AttributePropertyChanged;
            //Display
            lblName.DoOneWayDataBinding("Text", _dataSource, nameof(CharacterAttrib.DisplayNameFormatted));
            lblValue.DoOneWayDataBinding("Text", _dataSource, nameof(CharacterAttrib.DisplayValue));
            lblLimits.DoOneWayDataBinding("Text", _dataSource, nameof(CharacterAttrib.AugmentedMetatypeLimits));
            lblValue.DoOneWayDataBinding("ToolTipText", _dataSource, nameof(CharacterAttrib.ToolTip));
            if (_objCharacter.Created)
            {
                flpButtonsCreate.Visible = false;

                cmdImproveATT.DoOneWayDataBinding("ToolTipText", _dataSource, nameof(CharacterAttrib.UpgradeToolTip));
                cmdImproveATT.DoOneWayDataBinding("Enabled", _dataSource, nameof(CharacterAttrib.CanUpgradeCareer));
                cmdBurnEdge.Visible = AttributeName == "EDG";
                cmdBurnEdge.ToolTipText = LanguageManager.GetString("Tip_CommonBurnEdge");
            }
            else
            {
                flpButtonsCareer.Visible = false;

                while (_objAttribute.KarmaMaximum < 0 && _objAttribute.Base > 0)
                    _objAttribute.Base -= 1;
                // Very rough fix for when Karma values somehow exceed KarmaMaximum after loading in. This shouldn't happen in the first place, but this ad-hoc patch will help fix crashes.
                if (_objAttribute.Karma > _objAttribute.KarmaMaximum)
                    _objAttribute.Karma = _objAttribute.KarmaMaximum;

                nudBase.DoOneWayDataBinding("Visible", _objCharacter, nameof(Character.EffectiveBuildMethodUsesPriorityTables));
                nudBase.DoOneWayDataBinding("Maximum", _dataSource, nameof(CharacterAttrib.PriorityMaximum));
                nudBase.DoDatabinding("Value", _dataSource, nameof(CharacterAttrib.Base));
                nudBase.DoOneWayDataBinding("Enabled", _dataSource, nameof(CharacterAttrib.BaseUnlocked));
                nudBase.InterceptMouseWheel = GlobalOptions.InterceptMode;

                nudKarma.Minimum = 0;
                nudKarma.DoOneWayDataBinding("Maximum", _dataSource, nameof(CharacterAttrib.KarmaMaximum));
                nudKarma.DoDatabinding("Value", _dataSource, nameof(CharacterAttrib.Karma));
                nudKarma.InterceptMouseWheel = GlobalOptions.InterceptMode;
            }
        }

        private void AttributePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AttributeSection.AttributeCategory)) return;
            _dataSource.DataSource = _objCharacter.AttributeSection.GetAttributeByName(AttributeName);
            _dataSource.ResetBindings(false);
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

            if (flpButtonsCreate.Visible && nudBase.Visible && intNudKarmaWidth >= 0)
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
            _objCharacter.AttributeSection.PropertyChanged -= AttributePropertyChanged;

            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

		private void cmdImproveATT_Click(object sender, EventArgs e)
		{
		    CharacterAttrib attrib = _objCharacter.AttributeSection.GetAttributeByName(AttributeName);
            int intUpgradeKarmaCost = attrib.UpgradeKarmaCost;

            if (intUpgradeKarmaCost == -1) return; //TODO: more descriptive
            if (intUpgradeKarmaCost > _objCharacter.Karma)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughKarma"), LanguageManager.GetString("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string confirmstring = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpense"), attrib.DisplayNameFormatted, attrib.Value + 1, intUpgradeKarmaCost);
            if (!CommonFunctions.ConfirmKarmaExpense(confirmstring))
                return;

		    attrib.Upgrade();
	        ValueChanged?.Invoke(this, e);
        }

        private void nudBase_ValueChanged(object sender, EventArgs e)
        {
            CharacterAttrib attrib = _objCharacter.AttributeSection.GetAttributeByName(AttributeName);
            decimal d = ((NumericUpDownEx)sender).Value;
            if (d == _oldBase) return;
            if (!CanBeMetatypeMax(
                Math.Max(
                    decimal.ToInt32(nudKarma.Value) + attrib.FreeBase + attrib.RawMinimum +
                    attrib.AttributeValueModifiers, attrib.TotalMinimum) + decimal.ToInt32(d)))
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
            _oldBase = d;
        }

        private void nudKarma_ValueChanged(object sender, EventArgs e)
        {
            CharacterAttrib attrib = _objCharacter.AttributeSection.GetAttributeByName(AttributeName);
            decimal d = ((NumericUpDownEx)sender).Value;
            if (d == _oldKarma) return;
            if (!CanBeMetatypeMax(
                Math.Max(
                    decimal.ToInt32(nudBase.Value) + attrib.FreeBase + attrib.RawMinimum +
                    attrib.AttributeValueModifiers, attrib.TotalMinimum) + decimal.ToInt32(d)))
            {
                // It's possible that the attribute maximum was reduced by an improvement, so confirm the appropriate value to bounce up/down to.
                if (_oldKarma > attrib.KarmaMaximum)
                {
                    _oldKarma = attrib.KarmaMaximum - 1;
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
            _oldKarma = d;
        }

        /// <summary>
        /// Is the attribute allowed to reach the Metatype Maximum?
        /// SR5 66: Characters at character creation may only have 1 Mental or Physical
        /// attribute at their natural maximum limit; the special attributes of Magic, Edge,
        /// and Resonance are not included in this limitation.
        /// </summary>
        private bool CanBeMetatypeMax(int intValue)
        {
            CharacterAttrib attrib = _objCharacter.AttributeSection.GetAttributeByName(AttributeName);
            if (_objCharacter.IgnoreRules || attrib.MetatypeCategory == CharacterAttrib.AttributeCategory.Special) return true;
            int intTotalMaximum = attrib.TotalMaximum;
            if (intValue < intTotalMaximum || intTotalMaximum == 0) return true;
            //TODO: This should be in AttributeSection, but I can't be bothered finagling the option into working.
            //Ideally return 2 or 1, allow for an improvement type to increase or decrease the value.
            int intMaxOtherAttributesAtMax = _objCharacter.Options.Allow2ndMaxAttribute ? 1 : 0;
            int intNumOtherAttributeAtMax = _objCharacter.AttributeSection.AttributeList.Count(att =>
                att.AtMetatypeMaximum && att.Abbrev != AttributeName && att.MetatypeCategory == CharacterAttrib.AttributeCategory.Standard);

            if (intNumOtherAttributeAtMax <= intMaxOtherAttributesAtMax) return true;
            Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_AttributeMaximum"),
                LanguageManager.GetString("MessageTitle_Attribute"), MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return false;
        }

        public string AttributeName => _objAttribute.Abbrev;

        [UsedImplicitly]
        public int NameWidth => lblName.PreferredWidth;

        private void cmdBurnEdge_Click(object sender, EventArgs e)
        {
            // Edge cannot go below 1.
            if (_objAttribute.Value == 0)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CannotBurnEdge"), LanguageManager.GetString("MessageTitle_CannotBurnEdge"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Verify that the user wants to Burn a point of Edge.
            if (Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_BurnEdge"), LanguageManager.GetString("MessageTitle_BurnEdge"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

			_objAttribute.Degrade(1);
			ValueChanged?.Invoke(this, e);
		}

        private void nudBase_BeforeValueIncrement(object sender, CancelEventArgs e)
        {
            CharacterAttrib attrib = _objCharacter.AttributeSection.GetAttributeByName(AttributeName);
            if (nudBase.Value + Math.Max(nudKarma.Value, 0) != attrib.TotalMaximum || nudKarma.Value == nudKarma.Minimum)
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
            CharacterAttrib attrib = _objCharacter.AttributeSection.GetAttributeByName(AttributeName);
            if (nudBase.Value + nudKarma.Value != attrib.TotalMaximum || nudBase.Value == nudBase.Minimum)
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

        ButtonWithToolTip _activeButton;
        protected ButtonWithToolTip ActiveButton
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

        protected Control FindToolTipControl(Point pt)
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
