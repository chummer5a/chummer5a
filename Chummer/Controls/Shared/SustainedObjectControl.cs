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
using System.Threading;
using System.Windows.Forms;

namespace Chummer
{
    public partial class SustainedObjectControl : UserControl
    {
        private readonly SustainedObject _objLinkedSustainedObject;
        private readonly CancellationToken _objMyToken;
        private bool _blnLoading = true;

        //Events
        public event EventHandlerExtensions.SafeAsyncEventHandler SustainedObjectDetailChanged;

        public event EventHandlerExtensions.SafeAsyncEventHandler UnsustainObject;

        public SustainedObjectControl(SustainedObject objLinkedSustainedObject, CancellationToken objMyToken = default)
        {
            _objLinkedSustainedObject = objLinkedSustainedObject;
            _objMyToken = objMyToken;
            InitializeComponent();
            this.UpdateLightDarkMode(token: objMyToken);
            this.TranslateWinForm(token: objMyToken);

            if (objLinkedSustainedObject.LinkedObjectType != Improvement.ImprovementSource.CritterPower)
            {
                chkSelfSustained.Visible = true;
                lblSelfSustained.Visible = true;
            }
        }

        private async void SustainedObjectControl_Load(object sender, EventArgs e)
        {
            try
            {
                await lblSustainedSpell.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y,
                        _objLinkedSustainedObject,
                        nameof(SustainedObject.CurrentDisplayName),
                        x => x.GetCurrentDisplayNameAsync(_objMyToken), token: _objMyToken)
                    .ConfigureAwait(false);
                await nudForce.DoDataBindingAsync("Value", _objLinkedSustainedObject, nameof(SustainedObject.Force), token: _objMyToken)
                    .ConfigureAwait(false);
                await nudNetHits.DoDataBindingAsync("Value", _objLinkedSustainedObject,
                    nameof(SustainedObject.NetHits), token: _objMyToken).ConfigureAwait(false);

                //Only do  the binding if it's actually needed
                if (_objLinkedSustainedObject.LinkedObjectType != Improvement.ImprovementSource.CritterPower)
                {
                    await chkSelfSustained.DoDataBindingAsync("Checked", _objLinkedSustainedObject,
                            nameof(SustainedObject.SelfSustained), token: _objMyToken)
                        .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // swallow this
            }
            finally
            {
                _blnLoading = false;
            }
        }

        private async void cmdDelete_Click(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            // Raise the UnsustainSpell Event when the user has confirmed their desire to Unsustain a Spell
            // The entire SustainedSpellControl is passed as an argument so the handling event can evaluate its contents.
            if (UnsustainObject != null)
            {
                try
                {
                    await UnsustainObject.Invoke(this, e, _objMyToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // swallow this
                }
            }
        }

        private async void SustainedObject_ControlStateChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (SustainedObjectDetailChanged != null)
            {
                try
                {
                    await SustainedObjectDetailChanged.Invoke(this, e, _objMyToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // swallow this
                }
            }
        }

        #region Properties

        public SustainedObject LinkedSustainedObject => _objLinkedSustainedObject;

        #endregion Properties
    }
}
