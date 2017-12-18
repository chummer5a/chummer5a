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
    /// Represents a week displayed on the <see cref="Calendar"/>
    /// </summary>
    public class CalendarWeek
    {
        #region Events

        #endregion

        #region Fields
        private Rectangle _bounds;
        private Calendar _calendar;
        private DateTime _firstDay;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the bounds of the week
        /// </summary>
        public Rectangle Bounds
        {
            get { return _bounds; }
        }

        /// <summary>
        /// Gets the calendar this week belongs to
        /// </summary>
        public Calendar Calendar
        {
            get { return _calendar; }
        }

        /// <summary>
        /// Gets the bounds of the week header
        /// </summary>
        public Rectangle HeaderBounds
        {
            get
            {
                return new Rectangle(
                    Bounds.Left,
                    Bounds.Top + Calendar.Renderer.DayHeaderHeight,
                    Calendar.Renderer.WeekHeaderWidth,
                    Bounds.Height - Calendar.Renderer.DayHeaderHeight );
            }
        }

        /// <summary>
        /// Gets the sunday that starts the week
        /// </summary>
        public DateTime StartDate
        {
            get { return _firstDay; }
        }


        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarWeek"/> class.
        /// </summary>
        /// <param name="calendar">The calendar.</param>
        /// <param name="firstDay">The first day.</param>
        internal CalendarWeek(Calendar calendar, DateTime firstDay)
        {
            _calendar = calendar;
            _firstDay = firstDay;
        }

        #region Public Methods

        /// <summary>
        /// Gets the short version of week's string representation
        /// </summary>
        /// <returns></returns>
        public string ToStringShort()
        {
            DateTime saturday = StartDate.AddDays(6);

            if (saturday.Month != StartDate.Month)
            {
                return string.Format("{0} - {1}",
                    StartDate.ToString("d/M"),
                    saturday.ToString("d/M")
                    );
            }
            else
            {
                return string.Format("{0} - {1}",
                    StartDate.Day,
                    saturday.ToString("d/M")
                    );
            }
        }

        /// <summary>
        /// Gets the large version of string representation
        /// </summary>
        /// <returns>The week in a string format</returns>
        public string ToStringLarge()
        {
            DateTime saturday = StartDate.AddDays(6);

            if (saturday.Month != StartDate.Month)
            {
                return string.Format("{0} - {1}",
                    StartDate.ToString("d MMM"),
                    saturday.ToString("d MMM")
                    );
            }
            else
            {
                return string.Format("{0} - {1}",
                    StartDate.Day,
                    saturday.ToString("d MMM")
                    );
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToStringLarge();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the value of the <see cref="Bounds"/> property
        /// </summary>
        /// <param name="r"></param>
        internal void SetBounds(Rectangle r)
        {
            _bounds = r;
        }

        #endregion

    }
}
