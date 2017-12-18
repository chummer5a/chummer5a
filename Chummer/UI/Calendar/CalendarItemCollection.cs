/*
	Copyright 2012 Justin LeCheminant

	This file is part of WindowsFormsCalendar.

    indowsFormsCalendar is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    indowsFormsCalendar is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with indowsFormsCalendar.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Chummer
{
    /// <summary>
    /// A collection of calendar items
    /// </summary>
    public class CalendarItemCollection
            : List<CalendarItem>
    {
        #region Events

        #endregion

        #region Fields
        private Calendar _calendar;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the calendar.
        /// </summary>
        public Calendar Calendar
        {
            get { return _calendar; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarItemCollection"/> class.
        /// </summary>
        /// <param name="c">The c.</param>
        internal CalendarItemCollection( Calendar c )
        {
            _calendar = c;
        }

        #region Public Methods

        /// <summary>
        /// Adds an item to the end of the list
        /// </summary>
        /// <param name="item">The object to be added to the end of the collection. The value can be null for reference types.</param>
        public new void Add( CalendarItem item )
        {
            base.Add( item ); CollectionChanged();
        }

        /// <summary>
        /// Adds the items of the specified collection to the end of the list.
        /// </summary>
        /// <param name="items">The items whose elements should be added to the end of the collection. The collection itself cannont be null, but it can contain elements that are null.</param>
        public new void AddRange( IEnumerable<CalendarItem> items )
        {
            base.AddRange( items ); CollectionChanged();
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public new void Clear()
        {
            base.Clear(); CollectionChanged();
        }

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        public new void Insert( int index, CalendarItem item )
        {
            base.Insert( index, item ); CollectionChanged();
        }

        /// <summary>
        /// Inserts the items of a collection into this collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="items"></param>
        public new void InsertRange( int index, IEnumerable<CalendarItem> items )
        {
            base.InsertRange( index, items ); CollectionChanged();
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the collection.
        /// </summary>
        /// <param name="item">The item to remove from the collection. The value can be null for reference types.</param>
        /// <returns><c>true</c> if item is successfully removed; otherwise, <c>false</c>. This method also returns false if item was not found in the collection.</returns>
        public new bool Remove( CalendarItem item )
        {
            bool result = base.Remove( item );

            CollectionChanged();

            return result;
        }

        /// <summary>
        /// Removes the item at the specified index of the collection
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.
        /// -or-
        ///   <paramref name="index"/> is equal to or greater than <see cref="P:System.Collections.Generic.List`1.Count"/>.
        ///   </exception>
        public new void RemoveAt( int index )
        {
            base.RemoveAt( index ); CollectionChanged();
        }

        /// <summary>
        /// Removes the all the items that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The Predicate delegate that defines the conditions of the items to remove.</param>
        /// <returns>The number of items removed from the collection.</returns>
        public new int RemoveAll( Predicate<CalendarItem> match )
        {
            int result = base.RemoveAll( match );

            CollectionChanged();

            return result;
        }

        /// <summary>
        /// Removes a range of elements from the collection
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of items to remove.</param>
        /// <param name="count">The number of items to remove</param>
        public new void RemoveRange( int index, int count )
        {
            base.RemoveRange( index, count ); CollectionChanged();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles what to do when this collection changes
        /// </summary>
        private void CollectionChanged()
        {
            Calendar.Renderer.PerformItemsLayout();
            Calendar.Invalidate();
        }

        #endregion

    }
}