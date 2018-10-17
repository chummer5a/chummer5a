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
using System.Windows.Forms;
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
            _objAttribute = attribute;
            _objCharacter = attribute.CharacterObject;
            InitializeComponent();
            _dataSource = _objCharacter.AttributeSection.GetAttributeBindingByName(AttributeName);
            _objCharacter.AttributeSection.PropertyChanged += AttributePropertyChanged;
            //Display
            lblName.DataBindings.Add("Text", _dataSource, nameof(CharacterAttrib.DisplayNameFormatted), false, DataSourceUpdateMode.OnPropertyChanged);
            lblValue.DataBindings.Add("Text", _dataSource, nameof(CharacterAttrib.DisplayValue), false, DataSourceUpdateMode.OnPropertyChanged);
            lblLimits.DataBindings.Add("Text", _dataSource, nameof(CharacterAttrib.AugmentedMetatypeLimits), false, DataSourceUpdateMode.OnPropertyChanged);
            lblValue.DataBindings.Add("ToolTipText", _dataSource, nameof(CharacterAttrib.ToolTip), false, DataSourceUpdateMode.OnPropertyChanged);
            if (_objCharacter.Created)
            {
                nudBase.Visible = false;
                nudKarma.Visible = false;
                cmdImproveATT.DataBindings.Add("ToolTipText", _dataSource, nameof(CharacterAttrib.UpgradeToolTip), false, DataSourceUpdateMode.OnPropertyChanged);
                cmdImproveATT.Visible = true;
                cmdImproveATT.DataBindings.Add("Enabled", _dataSource, nameof(CharacterAttrib.CanUpgradeCareer), false, DataSourceUpdateMode.OnPropertyChanged);
                cmdBurnEdge.Visible = AttributeName == "EDG";
                cmdBurnEdge.ToolTipText = LanguageManager.GetString("Tip_CommonBurnEdge", GlobalOptions.Language);
            }
            else
            {
                while (_objAttribute.KarmaMaximum < 0 && _objAttribute.Base > 0)
                    _objAttribute.Base -= 1;
                // Very rough fix for when Karma values somehow exceed KarmaMaximum after loading in. This shouldn't happen in the first place, but this ad-hoc patch will help fix crashes.
                if (_objAttribute.Karma > _objAttribute.KarmaMaximum)
                    _objAttribute.Karma = _objAttribute.KarmaMaximum;

                nudBase.DataBindings.Add("Visible", _objCharacter, nameof(Character.BuildMethodHasSkillPoints), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("Maximum", _dataSource, nameof(CharacterAttrib.PriorityMaximum), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("Value", _dataSource, nameof(CharacterAttrib.Base), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("Enabled", _dataSource, nameof(CharacterAttrib.BaseUnlocked), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("InterceptMouseWheel", _objCharacter.Options, nameof(CharacterOptions.InterceptMode), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.Visible = true;

                nudKarma.Minimum = 0;
                nudKarma.DataBindings.Add("Maximum", _dataSource, nameof(CharacterAttrib.KarmaMaximum), false, DataSourceUpdateMode.OnPropertyChanged);
                nudKarma.DataBindings.Add("Value", _dataSource, nameof(CharacterAttrib.Karma), false, DataSourceUpdateMode.OnPropertyChanged);
                nudKarma.DataBindings.Add("InterceptMouseWheel", _objCharacter.Options, nameof(CharacterOptions.InterceptMode), false,
                    DataSourceUpdateMode.OnPropertyChanged);
                nudKarma.Visible = true;
                cmdImproveATT.Visible = false;
                cmdBurnEdge.Visible = false;
            }
        }

        private void AttributePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AttributeSection.AttributeCategory))
            {
                _dataSource.DataSource = _objCharacter.AttributeSection.GetAttributeByName(_objAttribute.Abbrev);
                _dataSource.ResetBindings(false);
            }
        }

        public void UnbindAttributeControl()
        {
            _objCharacter.AttributeSection.PropertyChanged -= AttributePropertyChanged;

            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

		private void cmdImproveATT_Click(object sender, EventArgs e)
        {
            int intUpgradeKarmaCost = _objAttribute.UpgradeKarmaCost;

            if (intUpgradeKarmaCost == -1) return; //TODO: more descriptive
            if (intUpgradeKarmaCost > _objCharacter.Karma)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string confirmstring = string.Format(LanguageManager.GetString("Message_ConfirmKarmaExpense", GlobalOptions.Language), _objAttribute.DisplayNameFormatted, _objAttribute.Value + 1, intUpgradeKarmaCost);
            if (!_objAttribute.CharacterObject.ConfirmKarmaExpense(confirmstring))
                return;

            _objAttribute.Upgrade();
	        ValueChanged?.Invoke(this, e);
        }

        private void nudBase_ValueChanged(object sender, EventArgs e)
        {
            decimal d = ((NumericUpDownEx)sender).Value;
            if (d == _oldBase) return;
            if (!ShowAttributeRule(Math.Max(decimal.ToInt32(d + nudKarma.Value) + _objAttribute.FreeBase + _objAttribute.RawMinimum + _objAttribute.AttributeValueModifiers, _objAttribute.TotalMinimum) + decimal.ToInt32(nudKarma.Value)))
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
            decimal d = ((NumericUpDownEx)sender).Value;
            if (d == _oldKarma) return;
            if (!ShowAttributeRule(Math.Max(decimal.ToInt32(nudBase.Value) + _objAttribute.FreeBase + _objAttribute.RawMinimum + _objAttribute.AttributeValueModifiers, _objAttribute.TotalMinimum) + decimal.ToInt32(d)))
            {
                // It's possible that the attribute maximum was reduced by an improvement, so confirm the appropriate value to bounce up/down to. 
                if (_oldKarma > _objAttribute.KarmaMaximum)
                {
                    _oldKarma = _objAttribute.KarmaMaximum - 1;
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
        /// Show the dialogue that notifies the user that characters cannot have more than 1 Attribute at its maximum value during character creation.
        /// </summary>
        private bool ShowAttributeRule(int intValue)
        {
            if (!_objCharacter.IgnoreRules)
            {
                int intTotalMaximum = _objAttribute.TotalMaximum;
                if (intValue >= intTotalMaximum && intTotalMaximum != 0)
                {
                    bool blnAttributeListContainsThisAbbrev = false;
                    int intNumOtherAttributeAtMax = 0;
                    int intMaxOtherAttributesAtMax = _objCharacter.Options.Allow2ndMaxAttribute ? 1 : 0;
                    foreach (CharacterAttrib objLoopAttrib in _objCharacter.AttributeSection.AttributeList)
                    {
                        if (objLoopAttrib.Abbrev == AttributeName)
                            blnAttributeListContainsThisAbbrev = true;
                        else if (objLoopAttrib.AtMetatypeMaximum)
                            intNumOtherAttributeAtMax += 1;
                        if (intNumOtherAttributeAtMax > intMaxOtherAttributesAtMax && blnAttributeListContainsThisAbbrev)
                            break;
                    }
                    if (intNumOtherAttributeAtMax > intMaxOtherAttributesAtMax && blnAttributeListContainsThisAbbrev)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_AttributeMaximum", GlobalOptions.Language),
                            LanguageManager.GetString("MessageTitle_Attribute", GlobalOptions.Language), MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return false;
                    }
                }
            }
            return true;
        }

        public string AttributeName => _objAttribute.Abbrev;

        private void cmdBurnEdge_Click(object sender, EventArgs e)
        {
            // Edge cannot go below 1.
            if (_objAttribute.Value == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CannotBurnEdge", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotBurnEdge", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Verify that the user wants to Burn a point of Edge.
            if (MessageBox.Show(LanguageManager.GetString("Message_BurnEdge", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_BurnEdge", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

			_objAttribute.Degrade(1);
			ValueChanged?.Invoke(this, e);
		}

        private void nudBase_BeforeValueIncrement(object sender, CancelEventArgs e)
        {
            if (nudBase.Value + Math.Max(nudKarma.Value, 0) != _objAttribute.TotalMaximum || nudKarma.Value == nudKarma.Minimum)
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
            if (nudBase.Value + nudKarma.Value != _objAttribute.TotalMaximum || nudBase.Value == nudBase.Minimum)
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
    }
}
