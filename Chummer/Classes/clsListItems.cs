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
    public class ListItem
    {
        public static ListItem AutoXml(string value, XmlNode node)
        {
            string display = node.Attributes?["translate"]?.InnerText ?? node.InnerText;

            return new ListItem(value, display);
        }

        public static ListItem Auto(string value, string languageString)
        {
            return new ListItem(value, LanguageManager.Instance.GetString(languageString));
        }

        public ListItem(string value, string name)
        {
            _strValue = value;
            _strName = name;
        }

        public ListItem()
        {

        }

        private string _strValue = string.Empty;
        private string _strName = string.Empty;

        /// <summary>
        /// Value.
        /// </summary>
        public string Value
        {
            get
            {
                return _strValue;
            }
            set
            {
                _strValue = value;
            }
        }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get
            {
                return _strName;
            }
            set
            {
                _strName = value;
            }
        }
    }

    #region Sorting Classes
    /// <summary>
    /// Sort TreeNodes in alphabetical order, ignoring [].
    /// </summary>
    public class SortByName : IComparer
    {
        public int Compare(object objX, object objY)
        {
            TreeNode tx = objX as TreeNode;
            TreeNode ty = objY as TreeNode;

            return string.Compare(tx.Text.Replace("[", string.Empty).Replace("]", string.Empty), ty.Text.Replace("[", string.Empty).Replace("]", string.Empty));
        }
    }

    /// <summary>
    /// Sort ListViewItems in reverse chronological order.
    /// </summary>
    public class SortByDate : IComparer
    {
        public int Compare(object objX, object objY)
        {
            ListViewItem lx = objX as ListViewItem;
            ListViewItem ly = objY as ListViewItem;

            return DateTime.Compare(DateTime.Parse(ly.Text), DateTime.Parse(lx.Text));
        }
    }

    /// <summary>
    /// Sort ListItems by Name in alphabetical order.
    /// </summary>
    public class SortListItem : IComparer
    {
        public int Compare(object objX, object objY)
        {
            ListItem lx = objX as ListItem;
            ListItem ly = objY as ListItem;

            return string.Compare(lx.Name, ly.Name);
        }
    }

    /// <summary>
    /// Sort ListViewColumns.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        private int _intColumnToSort;
        private SortOrder _objOrderOfSort;
        private readonly CaseInsensitiveComparer _objObjectCompare;

        public ListViewColumnSorter()
        {
            _objObjectCompare = new CaseInsensitiveComparer();
        }

        public int Compare(object x, object y)
        {
            int compareResult;

            // Cast the objects to be compared to ListViewItem objects
            ListViewItem listviewX = (ListViewItem)x;
            ListViewItem listviewY = (ListViewItem)y;

            // Compare the two items
            if (_intColumnToSort == 0)
                compareResult = DateTime.Compare(DateTime.Parse(listviewX.SubItems[_intColumnToSort].Text), DateTime.Parse(listviewY.SubItems[_intColumnToSort].Text));
            else if (_intColumnToSort == 1)
                compareResult = _objObjectCompare.Compare(Convert.ToInt32(listviewX.SubItems[_intColumnToSort].Text.Replace("¥", string.Empty).Replace(",", string.Empty).Replace(" ", string.Empty)), Convert.ToInt32(listviewY.SubItems[_intColumnToSort].Text.Replace("¥", string.Empty).Replace(",", string.Empty).Replace(" ", string.Empty)));
            else
                compareResult = _objObjectCompare.Compare(listviewX.SubItems[_intColumnToSort].Text, listviewY.SubItems[_intColumnToSort].Text);

            // Calculate correct return value based on object comparison
            if (_objOrderOfSort == SortOrder.Ascending)
                return compareResult;
            else if (_objOrderOfSort == SortOrder.Descending)
                return (-compareResult);
            else
                return 0;
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
