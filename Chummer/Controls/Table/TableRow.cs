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
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer.UI.Table
{
    public partial class TableRow : UserControl
    {
        private int _intIndex;
        private bool _blnSelected;

        public TableRow()
        {
            InitializeComponent();
            Layout += (sender, evt) => DoLayout();
        }

        private async void OnLoad(object sender, EventArgs eventArgs)
        {
            await UpdateAsync(Index, Selected).ConfigureAwait(false);
        }

        protected virtual void DoLayout()
        {
        }

        protected virtual void Update(int intIndex, bool blnSelected)
        {
            if (blnSelected)
            {
                this.DoThreadSafe(x => x.BackColor = ColorManager.Highlight);
            }
            else
            {
                this.DoThreadSafe(x => x.BackColor
                                      = (intIndex & 1) == 0 ? ColorManager.ControlLightest : ColorManager.Control);
            }
        }

        protected virtual async ValueTask UpdateAsync(int intIndex, bool blnSelected, CancellationToken token = default)
        {
            if (blnSelected)
            {
                Color objHighlightColor = await ColorManager.GetHighlightAsync(token).ConfigureAwait(false);
                await this.DoThreadSafeAsync(x => x.BackColor = objHighlightColor, token: token).ConfigureAwait(false);
            }
            else
            {
                Color objColor = (intIndex & 1) == 0
                    ? await ColorManager.GetControlLightestAsync(token).ConfigureAwait(false)
                    : await ColorManager.GetControlAsync(token).ConfigureAwait(false);
                await this.DoThreadSafeAsync(x => x.BackColor = objColor, token: token).ConfigureAwait(false);
            }
        }

        public int Index
        {
            get => _intIndex;
            set
            {
                if (_intIndex == value)
                    return;
                _intIndex = value;
                Update(Index, Selected);
            }
        }

        public bool Selected
        {
            get => _blnSelected;
            set
            {
                if (_blnSelected == value)
                    return;
                _blnSelected = value;
                Update(Index, Selected);
            }
        }
    }
}
