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
    /// Holds data of a Calendar Loading Items of certain date range
    /// </summary>
    public class CalendarLoadEventArgs
        : EventArgs
    {
        #region Fields
        private Calendar _calendar;
        private DateTime _dateStart;
        private DateTime _dateEnd;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarLoadEventArgs"/> class.
        /// </summary>
        /// <param name="calendar">The calendar.</param>
        /// <param name="dateStart">The date start.</param>
        /// <param name="dateEnd">The date end.</param>
        public CalendarLoadEventArgs(Calendar calendar, DateTime dateStart, DateTime dateEnd)
        {
            _calendar = calendar;
            _dateEnd = dateEnd;
            _dateStart = dateStart;
        }

        #region Props

        /// <summary>
        /// Gets the calendar that originated the event
        /// </summary>
        public Calendar Calendar
        {
            get { return _calendar; }
        }

        /// <summary>
        /// Gets the start date of the load
        /// </summary>
        public DateTime DateStart
        {
            get { return _dateStart; }
            set { _dateStart = value; }
        }

        /// <summary>
        /// Gets the end date of the load
        /// </summary>
        public DateTime DateEnd
        {
            get { return _dateEnd; }
        }


        #endregion
    }
}
