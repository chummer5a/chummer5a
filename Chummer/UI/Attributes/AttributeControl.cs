using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chummer.Backend.Attributes;
using Chummer.helpers;

namespace Chummer.UI.Attributes
{
    public partial class AttributeControl : UserControl
    {
        // ConnectionRatingChanged Event Handler.
        public delegate void ValueChangedHandler(Object sender);
        public event ValueChangedHandler ValueChanged;
        private readonly CharacterAttrib attribute;
        private object sender;
        private decimal _oldBase;
        private decimal _oldKarma;
        private Character _objCharacter;
        private BindingSource _dataSource;

        public AttributeControl(CharacterAttrib attribute)
        {
            this.attribute = attribute;
            _objCharacter = attribute.CharacteObject;
            InitializeComponent();
            _dataSource = attribute._objCharacter.AttributeSection.GetAttributeBindingByName(AttributeName);

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
                cmdBurnEdge.TooltipText = LanguageManager.GetString("Tip_CommonBurnEdge");
            }
            else
            {
                nudBase.DataBindings.Add("Minimum", _dataSource, nameof(CharacterAttrib.TotalMinimum), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("Maximum", _dataSource, nameof(CharacterAttrib.PriorityMaximum), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("Value", _dataSource, nameof(CharacterAttrib.TotalBase), false, DataSourceUpdateMode.OnPropertyChanged);
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
            frmCareer parent = ParentForm as frmCareer;
            if (parent != null)
            {
                int upgradeKarmaCost = attribute.UpgradeKarmaCost();

                if (upgradeKarmaCost == -1) return; //TODO: more descriptive
                string confirmstring = string.Format(LanguageManager.GetString("Message_ConfirmKarmaExpense"),
                    attribute.DisplayNameFormatted, attribute.Value + 1, upgradeKarmaCost);
                if (upgradeKarmaCost > _objCharacter.Karma)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma"), LanguageManager.GetString("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!parent.ConfirmKarmaExpense(confirmstring))
                    return;
            }
            attribute.Upgrade();
	        ValueChanged?.Invoke(this);
        }

        private void nudBase_ValueChanged(object sender, EventArgs e)
        {
            decimal d = ((NumericUpDownEx) sender).Value;
            if (!ShowAttributeRule(d + nudKarma.Value))
            {
                nudBase.Value = _oldBase;
                return;
            }
            ValueChanged?.Invoke(this);
            _oldBase = d;
        }

        private void nudKarma_ValueChanged(object sender, EventArgs e)
        {
            decimal d = ((NumericUpDownEx)sender).Value;
            if (!ShowAttributeRule(d + nudBase.Value))
            {
                nudKarma.Value = _oldKarma;
                return;
            }
            ValueChanged?.Invoke(this);
            _oldKarma = d;
        }

        /// <summary>
        /// Show the dialogue that notifies the user that characters cannot have more than 1 Attribute at its maximum value during character creation.
        /// </summary>
        private bool ShowAttributeRule(decimal value)
        {
            if (_objCharacter.IgnoreRules || value < attribute.TotalMaximum || attribute.TotalMaximum == 0) return true;
            bool any = _objCharacter.AttributeSection.AttributeList.Any(att => att.AtMetatypeMaximum && att.Abbrev != AttributeName);
            if (!any || attribute.AtMetatypeMaximum || _objCharacter.AttributeSection.AttributeList.All(att => att.Abbrev != AttributeName)) return true;
            MessageBox.Show(LanguageManager.GetString("Message_AttributeMaximum"),
                LanguageManager.GetString("MessageTitle_Attribute"), MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return false;
        }

        public string AttributeName
	    {
		    get { return attribute.Abbrev; }
	    }

        private void cmdBurnEdge_Click(object sender, EventArgs e)
        {
            // Edge cannot go below 1.
            if (attribute.Value == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CannotBurnEdge"), LanguageManager.GetString("MessageTitle_CannotBurnEdge"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Verify that the user wants to Burn a point of Edge.
            if (MessageBox.Show(LanguageManager.GetString("Message_BurnEdge"), LanguageManager.GetString("MessageTitle_BurnEdge"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

			attribute.Degrade(1);
			ValueChanged?.Invoke(this);
		}

        private void nudBase_BeforeValueIncrement(object sender, CancelEventArgs e)
        {
            if (nudBase.Value + Math.Max(nudKarma.Value, 0) != attribute.TotalMaximum ||
                nudKarma.Value == nudKarma.Minimum) return;
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
            if (nudBase.Value + nudKarma.Value != attribute.TotalMaximum || nudBase.Value == nudBase.Minimum) return;
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
