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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    public class ElasticComboBox : ComboBox
    {
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            if (DropDownStyle != ComboBoxStyle.DropDownList && IsHandleCreated)
                ClearSelection();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (DropDownStyle != ComboBoxStyle.DropDownList && IsHandleCreated)
                ClearSelection();
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
            foreach (object objItem in Items)
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
    }
}
