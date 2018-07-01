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
    public sealed class ElasticComboBox : ComboBox
    {
        private readonly ToolTip _tt;

        private string _strToolTipText = string.Empty;
        public string TooltipText
        {
            get => _strToolTipText;
            set
            {
                if (_strToolTipText != value)
                {
                    _strToolTipText = value;
                    if (!string.IsNullOrEmpty(value))
                        _tt.SetToolTip(this, value);
                }
            }
        }

        public ElasticComboBox() : this(null) { }

        public ElasticComboBox(ToolTip objToolTip)
        {
            _tt = objToolTip ?? new ToolTip
            {
                AutoPopDelay = 1500,
                InitialDelay = 400,
                UseAnimation = true,
                UseFading = true,
                Active = true
            };

            MouseEnter += Label_MouseEnter;
            MouseLeave += Label_MouseLeave;
        }

        private void Label_MouseEnter(object sender, EventArgs ea)
        {
            if (!string.IsNullOrEmpty(TooltipText))
            {
                _tt.Show(TooltipText, Parent);
            }
        }
        private void Label_MouseLeave(object sender, EventArgs ea)
        {
            _tt.Hide(this);
        }

        public new object DataSource
        {
            get => base.DataSource;
            set
            {
                if (base.DataSource != value)
                {
                    base.DataSource = value;
                    ResizeDropDown();
                }
            }
        }
        public new string DisplayMember
        {
            get => base.DisplayMember;
            set
            {
                if (base.DisplayMember != value)
                {
                    base.DisplayMember = value;
                    ResizeDropDown();
                }
            }
        }
        public new string ValueMember
        {
            get => base.ValueMember;
            set
            {
                if (base.ValueMember != value)
                {
                    base.ValueMember = value;
                    ResizeDropDown();
                }
            }
        }

        private void ResizeDropDown()
        {
            float fltMaxItemWidth = Width;
            foreach (var objItem in Items)
            {
                string strItemText = string.Empty;
                if (objItem is ListItem objListItem)
                    strItemText = objListItem.Name;
                if (string.IsNullOrEmpty(strItemText))
                    strItemText = GetItemText(objItem);
                float fltLoopItemWidth = TextRenderer.MeasureText(strItemText, Font).Width;
                if (fltLoopItemWidth > fltMaxItemWidth)
                    fltMaxItemWidth = fltLoopItemWidth;
            }
            DropDownWidth = Convert.ToInt32(Math.Ceiling(fltMaxItemWidth));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tt?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
