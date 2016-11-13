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
		public delegate void ValueChangedHandler(Object sender);
		public event ValueChangedHandler ValueChanged;
		private readonly CharacterAttrib attribute;
		private object sender;

		public AttributeControl(CharacterAttrib attribute)
        {
            this.attribute = attribute;
            InitializeComponent();
			
			//Display
			lblName.DataBindings.Add("Text", attribute, nameof(CharacterAttrib.DisplayNameFormatted), false, DataSourceUpdateMode.OnPropertyChanged);
            lblValue.DataBindings.Add("Text", attribute, nameof(CharacterAttrib.Value), false, DataSourceUpdateMode.OnPropertyChanged);
            lblAugmented.DataBindings.Add("Text", attribute, nameof(CharacterAttrib.TotalValue), false, DataSourceUpdateMode.OnPropertyChanged);
            lblLimits.DataBindings.Add("Text", attribute, nameof(CharacterAttrib.AugmentedMetatypeLimits), false, DataSourceUpdateMode.OnPropertyChanged);
            
            if (attribute._objCharacter.Created)
            {
                nudBase.Visible = false;
                nudKarma.Visible = false;
                cmdImproveATT.Visible = true;
	            cmdBurnEdge.Visible = attribute.Abbrev == "EDG";
            }
            else
            {
                nudBase.DataBindings.Add("Minimum", attribute, nameof(CharacterAttrib.TotalMinimum), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("Maximum", attribute, nameof(CharacterAttrib.TotalMaximum), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("Value", attribute, nameof(CharacterAttrib.Base), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("Enabled", attribute, nameof(CharacterAttrib.BaseUnlocked), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.Visible = true;

				nudKarma.Minimum = 0;
				nudKarma.DataBindings.Add("Maximum", attribute, nameof(CharacterAttrib.TotalMaximum), false, DataSourceUpdateMode.OnPropertyChanged);
                nudKarma.DataBindings.Add("Value", attribute, nameof(CharacterAttrib.Karma), false, DataSourceUpdateMode.OnPropertyChanged);
                nudKarma.Visible = true;
                cmdImproveATT.Visible = false;
	            cmdBurnEdge.Visible = false;
            }
        }

		public AttributeControl(object sender)
		{
			this.sender = sender;
		}

		private void cmdImproveATT_Click(object sender, EventArgs e)
        {
            frmCareer parent = ParentForm as frmCareer;
            if (parent != null)
            {
                int upgradeKarmaCost = attribute.UpgradeKarmaCost();

                if (upgradeKarmaCost == -1) return; //TODO: more descriptive
                string confirmstring = "";
                if (upgradeKarmaCost > attribute._objCharacter.Karma)
                {
                    MessageBox.Show(LanguageManager.Instance.GetString("Message_NotEnoughKarma"), LanguageManager.Instance.GetString("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (parent.ConfirmKarmaExpense(attribute.UpgradeKarmaCostString))
                    return;

                if (!parent.ConfirmKarmaExpense(confirmstring))
                    return;
            }
            attribute.Upgrade();
	        ValueChanged?.Invoke(this);
        }

		private void nudBase_ValueChanged(object sender, EventArgs e)
		{// Verify that the CharacterAttribute can be improved within the rules.
			if (!attribute.CanImproveAttribute() && (nudBase.Value + nudKarma.Value) >= nudBase.Maximum && !attribute._objCharacter.IgnoreRules)
			{
				try
				{
					attribute.Value = attribute.TotalMaximum - attribute.Value - 1;
					ShowAttributeRule();
				}
				catch
				{
					nudKarma.Value = 0;
				}
			}
			else if ((nudBase.Value + nudKarma.Value) > nudBase.Maximum)
			{
				try
				{
					nudKarma.Value = nudBase.Maximum - nudBase.Value;
				}
				catch
				{
					nudKarma.Value = 0;
				}
			}

			attribute.Base = Convert.ToInt32(nudBase.Value);
			attribute.Karma = Convert.ToInt32(nudKarma.Value);
			attribute.Value = Convert.ToInt32(nudBase.Value) + Convert.ToInt32(nudKarma.Value);
			ValueChanged?.Invoke(this);
		}

		private void nudKarma_ValueChanged(object sender, EventArgs e)
		{
			// Verify that the CharacterAttribute can be improved within the rules.
			if (!attribute.CanImproveAttribute() && (nudBase.Value + nudKarma.Value) >= nudBase.Maximum && !attribute._objCharacter.IgnoreRules)
			{
				try
				{
					nudKarma.Value = nudBase.Maximum - nudBase.Value - 1;
					ShowAttributeRule();
				}
				catch
				{
					nudKarma.Value = 0;
				}
			}
			else if ((nudBase.Value + nudKarma.Value) > nudBase.Maximum)
			{
				try
				{
					nudKarma.Value = nudBase.Maximum - nudBase.Value;
				}
				catch
				{
					nudKarma.Value = 0;
				}
			}

			attribute.Karma = Convert.ToInt32(nudKarma.Value);
			attribute.Base = Convert.ToInt32(nudBase.Value);
			attribute.Value = Convert.ToInt32(nudBase.Value) + Convert.ToInt32(nudKarma.Value);
			ValueChanged?.Invoke(this);
		}

		/// <summary>
		/// Show the dialogue that notifies the user that characters cannot have more than 1 Attribute at its maximum value during character creation.
		/// </summary>
		public void ShowAttributeRule()
		{
			MessageBox.Show(LanguageManager.Instance.GetString("Message_AttributeMaximum"), LanguageManager.Instance.GetString("MessageTitle_Attribute"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
				MessageBox.Show(LanguageManager.Instance.GetString("Message_CannotBurnEdge"), LanguageManager.Instance.GetString("MessageTitle_CannotBurnEdge"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			// Verify that the user wants to Burn a point of Edge.
			if (MessageBox.Show(LanguageManager.Instance.GetString("Message_BurnEdge"), LanguageManager.Instance.GetString("MessageTitle_BurnEdge"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				return;

			attribute.Value -= 1;
			ValueChanged?.Invoke(this);
		}
	}
}
