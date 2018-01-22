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
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chummer.Backend.Attributes;

namespace Chummer.UI.Attributes
{
    public partial class AttributeControl : UserControl
    {
        // ConnectionRatingChanged Event Handler.
        public delegate void ValueChangedHandler(Object sender, EventArgs e);
        public event ValueChangedHandler ValueChanged;
        private readonly CharacterAttrib _objAttribute;
        private readonly object sender;
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

            //Display
            lblName.DataBindings.Add("Text", _dataSource, nameof(CharacterAttrib.DisplayNameFormatted), false, DataSourceUpdateMode.OnPropertyChanged);
            lblValue.DataBindings.Add("Text", _dataSource, nameof(CharacterAttrib.DisplayValue), false, DataSourceUpdateMode.OnPropertyChanged);
            lblLimits.DataBindings.Add("Text", _dataSource, nameof(CharacterAttrib.AugmentedMetatypeLimits), false, DataSourceUpdateMode.OnPropertyChanged);
            lblValue.DataBindings.Add("TooltipText", _dataSource, nameof(CharacterAttrib.ToolTip), false, DataSourceUpdateMode.OnPropertyChanged);
            if (_objCharacter.Created)
            {
                nudBase.Visible = false;
                nudKarma.Visible = false;
                cmdImproveATT.DataBindings.Add("TooltipText", _dataSource, nameof(CharacterAttrib.UpgradeToolTip), false, DataSourceUpdateMode.OnPropertyChanged);
                cmdImproveATT.Visible = true;
                cmdImproveATT.DataBindings.Add("Enabled", _dataSource, nameof(CharacterAttrib.CanUpgradeCareer), false, DataSourceUpdateMode.OnPropertyChanged);
                cmdBurnEdge.Visible = AttributeName == "EDG";
                cmdBurnEdge.TooltipText = LanguageManager.GetString("Tip_CommonBurnEdge", GlobalOptions.Language);
            }
            else
            {
                nudBase.DataBindings.Add("Maximum", _dataSource, nameof(CharacterAttrib.PriorityMaximum), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("Value", _dataSource, nameof(CharacterAttrib.Base), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("Enabled", _dataSource, nameof(CharacterAttrib.BaseUnlocked), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("InterceptMouseWheel", _objCharacter.Options, nameof(CharacterOptions.InterceptMode), false,
                    DataSourceUpdateMode.OnPropertyChanged);
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

        public AttributeControl(object sender)
        {
            this.sender = sender;
        }

		public void ResetBinding(CharacterAttrib attrib)
		{
			_dataSource.DataSource = attrib;
		}

		private void cmdImproveATT_Click(object sender, EventArgs e)
        {
            int upgradeKarmaCost = _objAttribute.UpgradeKarmaCost();

            if (upgradeKarmaCost == -1) return; //TODO: more descriptive
            if (upgradeKarmaCost > _objCharacter.Karma)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string confirmstring = string.Format(LanguageManager.GetString("Message_ConfirmKarmaExpense", GlobalOptions.Language), _objAttribute.DisplayNameFormatted, _objAttribute.Value + 1, upgradeKarmaCost);
            if (!_objAttribute.CharacterObject.ConfirmKarmaExpense(confirmstring))
                return;

            _objAttribute.Upgrade();
	        ValueChanged?.Invoke(this, e);
        }

        private void nudBase_ValueChanged(object sender, EventArgs e)
        {
            decimal d = ((NumericUpDownEx) sender).Value;
            if (d != _oldBase)
            {
                if (!ShowAttributeRule(Math.Max(decimal.ToInt32(d) + _objAttribute.FreeBase + _objAttribute.RawMinimum + _objAttribute.AttributeValueModifiers, _objAttribute.TotalMinimum) + decimal.ToInt32(nudKarma.Value)))
                {
                    nudBase.Value = _oldBase;
                    return;
                }
                ValueChanged?.Invoke(this, e);
                _oldBase = d;
            }
        }

        private void nudKarma_ValueChanged(object sender, EventArgs e)
        {
            decimal d = ((NumericUpDownEx)sender).Value;
            if (d != _oldKarma)
            {
                if (!ShowAttributeRule(Math.Max(decimal.ToInt32(nudBase.Value) + _objAttribute.FreeBase + _objAttribute.RawMinimum + _objAttribute.AttributeValueModifiers, _objAttribute.TotalMinimum) + decimal.ToInt32(d)))
                {
                    nudKarma.Value = _oldKarma;
                    return;
                }
                ValueChanged?.Invoke(this, e);
                _oldKarma = d;
            }
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
                    bool blnAnyOtherAttributeAtMax = false;
                    foreach (CharacterAttrib objLoopAttrib in _objCharacter.AttributeSection.AttributeList)
                    {
                        if (objLoopAttrib.Abbrev == AttributeName)
                            blnAttributeListContainsThisAbbrev = true;
                        else if (objLoopAttrib.AtMetatypeMaximum)
                            blnAnyOtherAttributeAtMax = true;
                        if (blnAnyOtherAttributeAtMax && blnAttributeListContainsThisAbbrev)
                            break;
                    }
                    if (blnAnyOtherAttributeAtMax && blnAttributeListContainsThisAbbrev)
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

        public string AttributeName
	    {
		    get { return _objAttribute.Abbrev; }
	    }

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
