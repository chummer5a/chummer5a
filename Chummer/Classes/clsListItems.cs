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
using System.Collections;
 using System.Diagnostics;
 using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public class ListItem<T>
    {

        /// <summary>
        /// Value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }

        public ListItem(T value, string name)
        {
            Value = value;
            Name = name;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

	/// <summary>
	/// ListItem class to make populating a DropDownList from a DataSource easier.
	/// </summary>
	public class ListItem : ListItem<string>
	{
        public static readonly ListItem Blank = new ListItem(string.Empty, string.Empty);

        public static ListItem AutoXml(string value, XmlNode node)
        {
            string display = node.Attributes["translate"]?.InnerText ?? node.InnerText;
            return new ListItem(value, display);
        }


        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

		public ListItem(string value, string name) : base(value, name)
		{
			
		}
        
        public static bool operator ==(ListItem x, object y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ListItem x, object y)
        {
            return !x.Equals(y);
        }

        public static bool operator ==(object x, ListItem y)
        {
            return x?.Equals(y) ?? ReferenceEquals(y, null);
        }

        public static bool operator !=(object x, ListItem y)
        {
            return !(x?.Equals(y) ?? ReferenceEquals(y, null));
        }
    }

    #region Sorting Classes
    public static class CompareTreeNodes
    {
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
            return string.Compare(tx.Text.FastEscape('[', ']'), ty.Text.FastEscape('[', ']'), false, GlobalOptions.CultureInfo);
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
            DateTime datY;
            if (lx == null || !DateTime.TryParse(lx.Text, GlobalOptions.CultureInfo, System.Globalization.DateTimeStyles.None, out DateTime datX))
            {
                if (ly == null || !DateTime.TryParse(ly.Text, GlobalOptions.CultureInfo, System.Globalization.DateTimeStyles.None, out datY))
                    return 0;
                else
                    return -1;
            }
            else if (ly == null || !DateTime.TryParse(ly.Text, GlobalOptions.CultureInfo, System.Globalization.DateTimeStyles.None, out datY))
                return 1;

            return DateTime.Compare(datY, datX);
        }
    }

    public static class CompareListItems
    {
        /// <summary>
        /// Sort ListItems by Name in alphabetical order.
        /// </summary>
        public static int CompareNames(ListItem objX, ListItem objY)
        {
            return string.Compare(objX.Name, objY.Name, false, GlobalOptions.CultureInfo);
        }
    }

    /// <summary>
    /// Sort ListViewColumns.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        private int _intColumnToSort;
        private SortOrder _objOrderOfSort;

        public int Compare(object x, object y)
        {
            if (_objOrderOfSort == SortOrder.None)
                return 0;

            int intCompareResult;

            // Cast the objects to be compared to ListViewItem objects
            ListViewItem listviewX = (ListViewItem)x;
            ListViewItem listviewY = (ListViewItem)y;

            // Compare the two items
            if (_intColumnToSort == 0)
            {
                intCompareResult = CompareListViewItems.CompareTextAsDates(listviewX, listviewY);
            }
            else
            {
                string strX = listviewX?.SubItems[_intColumnToSort].Text.FastEscape('¥');
                string strY = listviewY?.SubItems[_intColumnToSort].Text.FastEscape('¥');
                if (decimal.TryParse(strX, System.Globalization.NumberStyles.Any, GlobalOptions.CultureInfo, out decimal decX) &&
                    decimal.TryParse(strY, System.Globalization.NumberStyles.Any, GlobalOptions.CultureInfo, out decimal decY))
                    intCompareResult = decimal.Compare(decX, decY);
                else
                    intCompareResult = string.Compare(strX, strY, true, GlobalOptions.CultureInfo);
            }

            // Calculate correct return value based on object comparison
            if (_objOrderOfSort == SortOrder.Ascending)
                return intCompareResult;
            return (-intCompareResult);
        }

        /// <summary>
        /// Column number to sort on.
        /// </summary>
        public int SortColumn
        {
            get => _intColumnToSort;
            set => _intColumnToSort = value;
        }

        /// <summary>
        /// SortOrder to be used.
        /// </summary>
        public SortOrder Order
        {
            get => _objOrderOfSort;
            set => _objOrderOfSort = value;
        }
    }

    /// <summary>
    /// Sort DataGridView Columns.
    /// </summary>
    public class DataGridViewColumnSorter : IComparer
    {
        private int _intColumnToSort;
        private SortOrder _objOrderOfSort;

        public int Compare(object x, object y)
        {
            if (_objOrderOfSort == SortOrder.None)
                return 0;

            int intCompareResult;

            // Cast the objects to be compared to ListViewItem objects
            DataGridViewRow datagridviewrowX = (DataGridViewRow)x;
            DataGridViewRow datagridviewrowY = (DataGridViewRow)y;

            // Compare the two items
            string strX = datagridviewrowX?.Cells[_intColumnToSort].Value.ToString();
            string strY = datagridviewrowY?.Cells[_intColumnToSort].Value.ToString();
            string strNumberX = datagridviewrowX?.Cells[_intColumnToSort].Value.ToString().TrimEnd('¥', '+')
                .TrimEndOnce(LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language))
                .TrimEndOnce(LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language));
            string strNumberY = datagridviewrowY?.Cells[_intColumnToSort].Value.ToString().TrimEnd('¥', '+')
                .TrimEndOnce(LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language))
                .TrimEndOnce(LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language));
            if (decimal.TryParse(strNumberX, System.Globalization.NumberStyles.Any, GlobalOptions.CultureInfo, out decimal decX))
            {
                if (decimal.TryParse(strNumberY, System.Globalization.NumberStyles.Any, GlobalOptions.CultureInfo, out decimal decY))

                    intCompareResult = decimal.Compare(decX, decY);
                else
                    intCompareResult = -1;
            }
            else if (decimal.TryParse(strNumberY, System.Globalization.NumberStyles.Any, GlobalOptions.CultureInfo, out decimal _))
            {
                intCompareResult = 1;
            }
            else
                intCompareResult = string.Compare(strX, strY, true, GlobalOptions.CultureInfo);

            // Calculate correct return value based on object comparison
            if (_objOrderOfSort == SortOrder.Ascending)
                return intCompareResult;
            return (-intCompareResult);
        }

        /// <summary>
        /// Column number to sort on.
        /// </summary>
        public int SortColumn
        {
            get => _intColumnToSort;
            set => _intColumnToSort = value;
        }

        /// <summary>
        /// SortOrder to be used.
        /// </summary>
        public SortOrder Order
        {
            get => _objOrderOfSort;
            set => _objOrderOfSort = value;
        }
    }
    #endregion
}
