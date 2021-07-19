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

using System.Drawing;
using System.Windows.Forms;

namespace Chummer
{
    public class ListViewItemWithValue : ListViewItem
    {
        public ListViewItemWithValue()
        {
        }

        public ListViewItemWithValue(object objValue) : base(objValue?.ToString() ?? string.Empty)
        {
            Value = objValue;
        }

        public ListViewItemWithValue(object objValue, ListViewGroup objGroup) : base(objValue?.ToString() ?? string.Empty, objGroup)
        {
            Value = objValue;
        }

        public ListViewItemWithValue(object objValue, int intImageIndex) : base(objValue?.ToString() ?? string.Empty, intImageIndex)
        {
            Value = objValue;
        }

        public ListViewItemWithValue(object objValue, int intImageIndex, ListViewGroup objGroup) : base(objValue?.ToString() ?? string.Empty, intImageIndex, objGroup)
        {
            Value = objValue;
        }

        public ListViewItemWithValue(object objValue, string strText) : base(strText)
        {
            Value = objValue;
        }

        public ListViewItemWithValue(object objValue, string strText, ListViewGroup objGroup) : base(strText, objGroup)
        {
            Value = objValue;
        }

        public ListViewItemWithValue(object objValue, string strText, int intImageIndex) : base(strText, intImageIndex)
        {
            Value = objValue;
        }

        public ListViewItemWithValue(object objValue, string strText, int intImageIndex, ListViewGroup objGroup) : base(strText, intImageIndex, objGroup)
        {
            Value = objValue;
        }

        public ListViewItemWithValue(object objValue, string strText, string strImageKey) : base(strText, strImageKey)
        {
            Value = objValue;
        }

        public ListViewItemWithValue(object objValue, string strText, string strImageKey, ListViewGroup objGroup) : base(strText, strImageKey, objGroup)
        {
            Value = objValue;
        }

        public object Value { get; set; }

        public class ListViewSubItemWithValue : ListViewSubItem
        {
            public ListViewSubItemWithValue()
            {
            }

            public ListViewSubItemWithValue(object objValue)
            {
                Value = objValue;
            }

            public ListViewSubItemWithValue(ListViewItem objOwner, object objValue) : base(objOwner, objValue?.ToString() ?? string.Empty)
            {
                Value = objValue;
            }

            public ListViewSubItemWithValue(ListViewItem objOwner, object objValue, Color objForeColor, Color objBackColor, Font objFont) : base(objOwner, objValue?.ToString() ?? string.Empty, objForeColor, objBackColor, objFont)
            {
                Value = objValue;
            }

            public ListViewSubItemWithValue(ListViewItem objOwner, object objValue, string strText) : base(objOwner, strText)
            {
                Value = objValue;
            }

            public ListViewSubItemWithValue(ListViewItem objOwner, object objValue, string strText, Color objForeColor, Color objBackColor, Font objFont) : base(objOwner, strText, objForeColor, objBackColor, objFont)
            {
                Value = objValue;
            }

            public object Value { get; set; }
        }
    }
}