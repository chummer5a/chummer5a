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
﻿using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// ListItem class to make populating a DropDownList from a DataSource easier.
    /// </summary>
    public struct ListItem
    {
        public static readonly ListItem Blank = new ListItem(string.Empty, string.Empty);

        public ListItem(string strValue, string strName)
        {
            Value = strValue;
            Name = strName;
        }

        /// <summary>
        /// Value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj.ToString());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }
    }

    #region Sorting Classes
    public static class CompareTreeNodes
    {
        private static readonly char[] s_LstBrackets = { '[', ']' };

        /// <summary>
        /// Sort TreeNodes in alphabetical order, ignoring [].
        /// </summary>
        public static int CompareText(TreeNode tx, TreeNode ty)
        {
            if (tx == null)
            {
                if (ty == null)
                    return 0;
                else
                    return -1;
            }
            else if (ty == null)
                return 1;
            return string.Compare(tx.Text.FastEscape(s_LstBrackets), ty.Text.FastEscape(s_LstBrackets));
        }

        public class TextComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return CompareText(x as TreeNode, y as TreeNode);
            }
        }
    }
    
    public static class CompareListViewItems
    {
        /// <summary>
        /// Sort ListViewItems in reverse chronological order.
        /// </summary>
        public static int CompareTextAsDates(ListViewItem lx, ListViewItem ly)
        {
            if (lx == null)
            {
                if (ly == null)
                    return 0;
                else
                    return -1;
            }
            else if (ly == null)
                return 1;
            return DateTime.Compare(DateTime.Parse(ly.Text, GlobalOptions.CultureInfo), DateTime.Parse(lx.Text, GlobalOptions.CultureInfo));
        }
    }

    public static class CompareListItems
    {
        /// <summary>
        /// Sort ListItems by Name in alphabetical order.
        /// </summary>
        public static int CompareNames(ListItem objX, ListItem objY)
        {
            return string.Compare(objX.Name, objY.Name);
        }
    }

    /// <summary>
    /// Sort ListViewColumns.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        private int _intColumnToSort;
        private SortOrder _objOrderOfSort;
        private readonly CaseInsensitiveComparer _objObjectCompare = new CaseInsensitiveComparer();

        private static readonly char[] s_LstCurrencyTrim = { '¥', ',', ' ' };
        public int Compare(object x, object y)
        {
            if (_objOrderOfSort == SortOrder.None)
                return 0;

            int compareResult;

            // Cast the objects to be compared to ListViewItem objects
            ListViewItem listviewX = (ListViewItem)x;
            ListViewItem listviewY = (ListViewItem)y;

            // Compare the two items
            string strX = listviewX.SubItems[_intColumnToSort].Text;
            string strY = listviewY.SubItems[_intColumnToSort].Text;
            if (_intColumnToSort == 0)
                compareResult = DateTime.Compare(DateTime.Parse(strX, GlobalOptions.CultureInfo), DateTime.Parse(strY, GlobalOptions.CultureInfo));
            else if (_intColumnToSort == 1)
                compareResult = _objObjectCompare.Compare(Convert.ToInt32(strX.FastEscape(s_LstCurrencyTrim)), Convert.ToInt32(strY.FastEscape(s_LstCurrencyTrim)));
            else
                compareResult = _objObjectCompare.Compare(strX, strY);

            // Calculate correct return value based on object comparison
            if (_objOrderOfSort == SortOrder.Ascending)
                return compareResult;
            return (-compareResult);
        }

        /// <summary>
        /// Column number to sort on.
        /// </summary>
        public int SortColumn
        {
            get
            {
                return _intColumnToSort;
            }
            set
            {
                _intColumnToSort = value;
            }
        }

        /// <summary>
        /// SortOrder to be used.
        /// </summary>
        public SortOrder Order
        {
            get
            {
                return _objOrderOfSort;
            }
            set
            {
                _objOrderOfSort = value;
            }
        }
    }
    #endregion
}
