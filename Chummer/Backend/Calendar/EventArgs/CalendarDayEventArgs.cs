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
    /// Event data with a <see cref="CalendarDay"/> element
    /// </summary>
    public class CalendarDayEventArgs
        :EventArgs
    {
        /// <summary>
        /// Creates a new event with the specified day
        /// </summary>
        /// <param name="day">Day of the event</param>
        public CalendarDayEventArgs(CalendarDay day)
        {
            _calendarDay = day;
        }

        #region Props
        private CalendarDay _calendarDay;

        /// <summary>
        /// Gets the day related to the event
        /// </summary>
        public CalendarDay CalendarDay
        {
            get { return _calendarDay; }
        }

        #endregion
    }
}
