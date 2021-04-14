using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;
using Chummer.Backend.Uniques;

namespace Chummer

{
    public partial class SustainedSpellControl : UserControl
    {
        private readonly Spell _objSustainedSpell;
        private bool _blnLoading = true;

        //Events
        public event EventHandler SpellDetailChanged;
        public event EventHandler UnsustainSpell;


        public SustainedSpellControl(Spell objSustainedSpell)
        {
            _objSustainedSpell = objSustainedSpell;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private void SustainedSpellControl_Load(object sender, EventArgs e)
        {
            lblSustainedSpell.Text = _objSustainedSpell.DisplayName(GlobalOptions.Language);
            chkSelfSustained.DoDatabinding("Checked", _objSustainedSpell, nameof(_objSustainedSpell.SelfSustained));
            nudForce.DoDatabinding("Value", _objSustainedSpell, nameof(_objSustainedSpell.Force));

            _blnLoading = false;
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Raise the UnsustainSpell Event when the user has confirmed their desire to Unsustain a Spell
            // The entire SustainedSpellControll is passed as an argument so the handling event can evaluate its contents.
            if (!_blnLoading)
            UnsustainSpell?.Invoke(this, e);
        }

        private void nudForce_ValueChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
            {
                SpellDetailChanged?.Invoke(this, e);
            }
                
        }

        private void chkSelf_CheckedChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
                SpellDetailChanged?.Invoke(this, e);
            
        }
        #region Properties
        public Spell SustainedSpellObject => _objSustainedSpell;
        #endregion
    }
}
