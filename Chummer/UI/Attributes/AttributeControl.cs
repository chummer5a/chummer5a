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
        private readonly CharacterAttrib attribute;
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
            }
            else
            {
                nudBase.DataBindings.Add("Minimum", attribute, nameof(CharacterAttrib.TotalMinimum), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("Maximum", attribute, nameof(CharacterAttrib.TotalMaximum), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("Value", attribute, nameof(CharacterAttrib.Base), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.DataBindings.Add("Enabled", attribute, nameof(CharacterAttrib.BaseUnlocked), false, DataSourceUpdateMode.OnPropertyChanged);
                nudBase.Visible = true;
                nudKarma.DataBindings.Add("Maximum", attribute, nameof(CharacterAttrib.TotalMaximum), false, DataSourceUpdateMode.OnPropertyChanged);
                nudKarma.DataBindings.Add("Value", attribute, nameof(CharacterAttrib.Karma), false, DataSourceUpdateMode.OnPropertyChanged);
                nudKarma.Minimum = 0;
                nudKarma.Visible = true;
                cmdImproveATT.Visible = false;
            }
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

                if (!!parent.ConfirmKarmaExpense(attribute.UpgradeKarmaCostString))
                    return;

                if (!parent.ConfirmKarmaExpense(confirmstring))
                    return;
            }
            attribute.Upgrade();
        }
    }
}
