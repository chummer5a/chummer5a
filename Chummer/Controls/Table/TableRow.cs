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
        private int _intSelected;

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
                Color objColor = (intIndex & 1) == 0 ? ColorManager.ControlLightest : ColorManager.Control;
                this.DoThreadSafe(x => x.BackColor = objColor);
            }
        }

        protected virtual Task UpdateAsync(int intIndex, bool blnSelected, CancellationToken token = default)
        {
            if (blnSelected)
            {
                Color objHighlightColor = ColorManager.Highlight;
                return this.DoThreadSafeAsync(x => x.BackColor = objHighlightColor, token: token);
            }
            else
            {
                Color objColor = (intIndex & 1) == 0
                    ? ColorManager.ControlLightest
                    : ColorManager.Control;
                return this.DoThreadSafeAsync(x => x.BackColor = objColor, token: token);
            }
        }

        public int Index
        {
            get => _intIndex;
            set
            {
                if (Interlocked.Exchange(ref _intIndex, value) != value)
                    Update(Index, Selected);
            }
        }

        public bool Selected
        {
            get => _intSelected > 0;
            set
            {
                int intNewValue = value.ToInt32();
                if (Interlocked.Exchange(ref _intSelected, intNewValue) == intNewValue)
                    return;
                Update(Index, Selected);
            }
        }
    }
}
