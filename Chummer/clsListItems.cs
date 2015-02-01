using System;
using System.Collections;
using System.Windows.Forms;

namespace Chummer
{
	/// <summary>
	/// ListItem class to make populating a DropDownList from a DataSource easier.
	/// </summary>
	public class ListItem
	{
		private string _strValue = "";
		private string _strName = "";

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
			_intColumnToSort = 0;
			_objOrderOfSort = SortOrder.None;
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