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
    /// Represents a range of time that is highlighted as work-time
    /// </summary>
    public class CalendarHighlightRange
    {
        #region Events

        #endregion

        #region Fields
        private Calendar _calendar;
        private DayOfWeek _dayOfWeek;
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the calendar that this range is assigned to. (If any)
        /// </summary>
        public Calendar Calendar
        {
            get { return _calendar; }
        }

        /// <summary>
        /// Gets or sets the day of the week for this range
        /// </summary>
        public DayOfWeek DayOfWeek
        {
            get { return _dayOfWeek; }
            set { _dayOfWeek = value; Update(); }
        }

        /// <summary>
        /// Gets or sets the start time of the range
        /// </summary>
        public TimeSpan StartTime
        {
            get { return _startTime; }
            set { _startTime = value; Update(); }
        }

        /// <summary>
        /// Gets or sets the end time of the range
        /// </summary>
        public TimeSpan EndTime
        {
            get { return _endTime; }
            set { _endTime = value; Update(); }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarHighlightRange"/> class.
        /// </summary>
        public CalendarHighlightRange()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarHighlightRange"/> class.
        /// </summary>
        /// <param name="day">The day.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        public CalendarHighlightRange( DayOfWeek day, TimeSpan startTime, TimeSpan endTime )
            : this()
        {
            _dayOfWeek = day;
            _startTime = startTime;
            _endTime = endTime;
        }

        #region Public Methods

        #endregion

        #region Private Methods

        /// <summary>
        /// Tells the calendar to update the highligts
        /// </summary>
        private void Update()
        {
            if (Calendar != null)
            {
                Calendar.UpdateHighlights();
            }
        }

        /// <summary>
        /// Sets the value of the <see cref="Calendar"/> property
        /// </summary>
        /// <param name="calendar">Calendar that this range belongs to</param>
        internal void SetCalendar(Calendar calendar)
        {
            _calendar = calendar;
        }

        #endregion

    }
}
