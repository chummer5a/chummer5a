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
        private readonly int _intToolTipWrap;

        private readonly ToolTip _objToolTip;

        private string _strToolTipText = string.Empty;

        public string TooltipText
        {
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

        public ElasticComboBox() : this(ToolTipFactory.ToolTip)
        {
        }

        public ElasticComboBox(ToolTip objToolTip, int intToolTipWrap = -1)
        {
            _objToolTip = objToolTip;
            _intToolTipWrap = intToolTipWrap;
            DoubleBuffered = true;
            SelectedIndexChanged += ClearUnintendedHighlight;
            Resize += ClearUnintendedHighlight;
        }

        private void ClearUnintendedHighlight(object sender, EventArgs e)
        {
            if (DropDownStyle != ComboBoxStyle.DropDownList && IsHandleCreated)
                this.DoThreadSafe(ClearSelection);
        }

        protected override void OnDataSourceChanged(EventArgs e)
        {
            base.OnDataSourceChanged(e);
            ResizeDropDown();
        }

        protected override void OnDisplayMemberChanged(EventArgs e)
        {
            base.OnDisplayMemberChanged(e);
            ResizeDropDown();
        }

        protected override void OnValueMemberChanged(EventArgs e)
        {
            base.OnValueMemberChanged(e);
            ResizeDropDown();
        }

        private void ResizeDropDown()
        {
            int intMaxItemWidth = Width;
            foreach (var objItem in Items)
            {
                string strItemText = string.Empty;
                if (objItem is ListItem objListItem)
                    strItemText = objListItem.Name;
                if (string.IsNullOrEmpty(strItemText))
                    strItemText = GetItemText(objItem);
                int intLoopItemWidth = TextRenderer.MeasureText(strItemText, Font).Width;
                if (intLoopItemWidth > intMaxItemWidth)
                    intMaxItemWidth = intLoopItemWidth;
            }
            DropDownWidth = intMaxItemWidth;
        }

        private void ClearSelection()
        {
            Control objActiveControl = FindForm()?.ActiveControl;
            if (objActiveControl == null || objActiveControl == this)
                return;
            SelectionLength = 0;
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
