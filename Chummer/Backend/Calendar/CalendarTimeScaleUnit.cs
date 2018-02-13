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
using System.Drawing;

namespace Chummer
{
    /// <summary>
    /// Represents a selectable timescale unit on a <see cref="CalendarDay"/>
    /// </summary>
    public class CalendarTimeScaleUnit
        : CalendarSelectableElement
    {
        #region Events

        #endregion

        #region Fields
        private DateTime _date;
        private CalendarDay _day;
        private bool _highlighted;
        private int _hours;
        private int _index;
        private int _minutes;
        private List<CalendarItem> _passingItems;
        private bool _visible;
        #endregion

        #region Properties



        /// <summary>
        /// Gets the exact date when the unit starts
        /// </summary>
        public override DateTime Date
        {
            get
            {
                if( _date.Equals( DateTime.MinValue ) )
                {
                    _date = new DateTime( Day.Date.Year, Day.Date.Month, Day.Date.Day, Hours, Minutes, 0 );
                }

                return _date;
            }
        }

        /// <summary>
        /// Gets the <see cref="CalendarDay"/> this unit belongs to
        /// </summary>
        public CalendarDay Day
        {
            get { return _day; }
        }

        /// <summary>
        /// Gets the duration of the unit.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                return new TimeSpan( 0, (int)Calendar.TimeScale, 0 );
            }
        }

        /// <summary>
        /// Gets if the unit is highlighted because it fits in some of the calendar's highlight ranges
        /// </summary>
        public bool Highlighted
        {
            get { return _highlighted; }
        }

        /// <summary>
        /// Gets the hour when this unit starts
        /// </summary>
        public int Hours
        {
            get { return _hours; }
        }

        /// <summary>
        /// Gets the index of the unit relative to the day
        /// </summary>
        public int Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Gets the minute when this unit starts
        /// </summary>
        public int Minutes
        {
            get { return _minutes; }
            set { _minutes = value; }
        }

        /// <summary>
        /// Gets or sets the amount of items that pass over the unit
        /// </summary>
        internal List<CalendarItem> PassingItems
        {
            get { return _passingItems; }
            set { _passingItems = value; }
        }

        /// <summary>
        /// Gets a value indicating if the unit is currently visible on viewport
        /// </summary>
        public bool Visible
        {
            get { return _visible; }
        }

        #endregion

        /// <summary>
        /// Creates a new <see cref="CalendarTimeScaleUnit"/>
        /// </summary>
        /// <param name="day"><see cref="CalendarDay"/> this unit belongs to</param>
        /// <param name="index">Index of the unit relative to the container day</param>
        /// <param name="hours">Hour of the unit</param>
        /// <param name="minutes">Minutes of the unit</param>
        internal CalendarTimeScaleUnit( CalendarDay day, int index, int hours, int minutes )
            : base( day.Calendar )
        {
            _day = day;
            _index = index;
            _hours = hours;
            _minutes = minutes;

            _passingItems = new List<CalendarItem>();
        }

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "[{0}] - {1}", Index, Date.ToShortTimeString() );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets a value indicating if the unit should be higlighted
        /// </summary>
        /// <returns></returns>
        internal bool CheckHighlighted()
        {
            for( int i = 0; i < Day.Calendar.HighlightRanges.Length; i++ )
            {
                CalendarHighlightRange range = Day.Calendar.HighlightRanges[i];

                if( range.DayOfWeek != Date.DayOfWeek )
                    continue;

                if( Date.TimeOfDay.CompareTo( range.StartTime ) >= 0
                    && Date.TimeOfDay.CompareTo( range.EndTime ) < 0 )
                {
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// Sets the value of the <see cref="Highlighted"/> property
        /// </summary>
        /// <param name="highlighted">Value of the property</param>
        internal void SetHighlighted( bool highlighted )
        {
            _highlighted = highlighted;
        }

        /// <summary>
        /// Sets the value of the <see cref="Visible"/> property
        /// </summary>
        /// <param name="visible">Value indicating if the unit is currently visible on viewport</param>
        internal void SetVisible( bool visible )
        {
            _visible = visible;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Adds the passing item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void AddPassingItem( CalendarItem item )
        {
            if( !PassingItems.Contains( item ) )
            {
                PassingItems.Add( item );
                Day.AddContainedItem( item );
            }
        }

        /// <summary>
        /// Clears existance of item from this unit and it's corresponding day.
        /// </summary>
        /// <param name="item"></param>
        internal void ClearItemExistance( CalendarItem item )
        {
            if( PassingItems.Contains( item ) )
            {
                PassingItems.Remove( item );
            }

            if( Day.ContainedItems.Contains( item ) )
            {
                Day.ContainedItems.Remove( item );
            }
        }

        #endregion

    }
}