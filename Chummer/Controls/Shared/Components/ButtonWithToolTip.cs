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

using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    public sealed class ButtonWithToolTip : DpiFriendlyImagedButton
    {
        private readonly int _intToolTipWrap;

        private readonly ToolTip _objToolTip;

        public ToolTip ToolTipObject => _objToolTip;

        private string _strToolTipText = string.Empty;

        public string ToolTipText
        {
            get => _strToolTipText;
            set
            {
                value = _intToolTipWrap > 0 ? value.WordWrap(_intToolTipWrap) : value.WordWrap();
                if (_strToolTipText == value)
                    return;
                _strToolTipText = value;
                this.DoThreadSafe(x => _objToolTip.SetToolTip(x, value.CleanForHtml()));
            }
        }

        public Task SetToolTipTextAsync(string value)
        {
            value = _intToolTipWrap > 0 ? value.WordWrap(_intToolTipWrap) : value.WordWrap();
            if (_strToolTipText == value)
                return Task.CompletedTask;
            _strToolTipText = value;
            return this.DoThreadSafeAsync(x => _objToolTip.SetToolTip(x, value.CleanForHtml()));
        }

        public ButtonWithToolTip() : this(ToolTipFactory.ToolTip)
        {
        }

        public ButtonWithToolTip(ToolTip objToolTip, int intToolTipWrap = -1)
        {
            _objToolTip = objToolTip;
            _intToolTipWrap = intToolTipWrap;
            DoubleBuffered = true;
        }

        public ButtonWithToolTip(IContainer container) : this(container, ToolTipFactory.ToolTip)
        {
        }

        public ButtonWithToolTip(IContainer container, ToolTip objToolTip, int intToolTipWrap = -1) : base(container)
        {
            _objToolTip = objToolTip;
            _intToolTipWrap = intToolTipWrap;
            DoubleBuffered = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _objToolTip != null && _objToolTip != ToolTipFactory.ToolTip)
            {
                _objToolTip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
