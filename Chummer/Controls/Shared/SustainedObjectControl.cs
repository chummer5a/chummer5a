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
using System.Windows.Forms;

namespace Chummer
{
    public partial class SustainedObjectControl : UserControl
    {
        private readonly SustainedObject _objLinkedSustainedObject;
        private bool _blnLoading = true;

        //Events
        public event EventHandler SustainedObjectDetailChanged;

        public event EventHandler UnsustainObject;

        public SustainedObjectControl(SustainedObject objLinkedSustainedObject)
        {
            _objLinkedSustainedObject = objLinkedSustainedObject;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            if (objLinkedSustainedObject.LinkedObjectType != Improvement.ImprovementSource.CritterPower)
            {
                chkSelfSustained.Visible = true;
                lblSelfSustained.Visible = true;
            }
        }

        private void SustainedObjectControl_Load(object sender, EventArgs e)
        {
            try
            {
                lblSustainedSpell.DoOneWayDataBinding("Text", _objLinkedSustainedObject, nameof(SustainedObject.CurrentDisplayName));
                nudForce.DoDataBinding("Value", _objLinkedSustainedObject, nameof(SustainedObject.Force));
                nudNetHits.DoDataBinding("Value", _objLinkedSustainedObject, nameof(SustainedObject.NetHits));

                //Only do  the binding if it's actually needed
                if (_objLinkedSustainedObject.LinkedObjectType != Improvement.ImprovementSource.CritterPower)
                    chkSelfSustained.DoDataBinding("Checked", _objLinkedSustainedObject, nameof(SustainedObject.SelfSustained));
            }
            finally
            {
                _blnLoading = false;
            }
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            // Raise the UnsustainSpell Event when the user has confirmed their desire to Unsustain a Spell
            // The entire SustainedSpellControll is passed as an argument so the handling event can evaluate its contents.
            UnsustainObject?.Invoke(this, e);
        }

        private void SustainedObject_ControlStateChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            SustainedObjectDetailChanged?.Invoke(this, e);
        }

        #region Properties

        public SustainedObject LinkedSustainedObject => _objLinkedSustainedObject;

        #endregion Properties
    }
}
