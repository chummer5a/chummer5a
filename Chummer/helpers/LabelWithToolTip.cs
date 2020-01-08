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

        private ToolTip _tt;
        // ReSharper disable once MemberCanBePrivate.Global
        // Used by databinding
        public ToolTip ToolTipObject
        {
            // ReSharper disable once UnusedMember.Global
            // Used by databinding
            get => _tt;
            private set
            {
                if (_tt != value)
                {
                    _tt?.Hide(this);
                    _tt = value;
                }
            }
        }

        private string _strToolTipText = string.Empty;
        public string ToolTipText
        {
            // ReSharper disable once UnusedMember.Global
            // Used by databinding
            get => _strToolTipText;
            set
            {
                value = value.WordWrap(_intToolTipWrap);
                if (_strToolTipText != value)
                {
                    _strToolTipText = value;
                    _tt.SetToolTip(this, value);
                }
            }
        }

        public LabelWithToolTip() : this(ToolTipFactory.ToolTip) { }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public LabelWithToolTip(ToolTip objToolTip, int intToolTipWrap = 100)
        {
            ToolTipObject = objToolTip;
            _intToolTipWrap = intToolTipWrap;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_tt != null && _tt != ToolTipFactory.ToolTip)
                    _tt.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
