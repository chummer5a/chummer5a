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
    /// Represents the top area of a day, where multiday and all day items are stored
    /// </summary>
    public class CalendarDayTop
        : CalendarSelectableElement
    {
        #region Events

        #endregion

        #region Fields
        private CalendarDay _day;
        private List<CalendarItem> _passingItems;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the date.
        /// </summary>
        public override DateTime Date
        {
            get
            {
                return new DateTime(Day.Date.Year, Day.Date.Month, Day.Date.Day);
            }
        }

        /// <summary>
        /// Gets the Day of this DayTop
        /// </summary>
        public CalendarDay Day
        {
            get { return _day; }
        }


        /// <summary>
        /// Gets the list of items passing on this daytop
        /// </summary>
        public List<CalendarItem> PassingItems
        {
            get { return _passingItems; }
        }

        #endregion

        /// <summary>
        /// Creates a new DayTop for the specified day
        /// </summary>
        /// <param name="day"></param>
        public CalendarDayTop(CalendarDay day)
            : base(day.Calendar)
        {
            _day = day;
            _passingItems = new List<CalendarItem>();
        }

        #region Public Methods

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds the passing item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void AddPassingItem(CalendarItem item)
        {
            if (!PassingItems.Contains(item))
            {
                PassingItems.Add(item);
            }
        }

        #endregion

    }
}
