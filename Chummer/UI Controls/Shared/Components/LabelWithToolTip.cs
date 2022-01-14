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

using System.Windows.Forms;

namespace Chummer
{
    public sealed class LabelWithToolTip : Label
    {
        private readonly int _intToolTipWrap;

        private readonly ToolTip _objToolTip;

        // ReSharper disable once MemberCanBePrivate.Global
        // Used by databinding
        public ToolTip ToolTipObject => _objToolTip;

        private string _strToolTipText = string.Empty;

        public string ToolTipText
        {
            // ReSharper disable once UnusedMember.Global
            // Used by databinding
            get => _strToolTipText;
            set
            {
                value = _intToolTipWrap > 0 ? value.WordWrap(_intToolTipWrap) : value.WordWrap();
                if (_strToolTipText == value)
                    return;
                _strToolTipText = value;
                _objToolTip.SetToolTip(this, value.CleanForHtml());
            }
        }

        public LabelWithToolTip() : this(ToolTipFactory.ToolTip)
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public LabelWithToolTip(ToolTip objToolTip, int intToolTipWrap = -1)
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
