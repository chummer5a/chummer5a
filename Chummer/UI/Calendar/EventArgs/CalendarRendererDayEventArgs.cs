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
    /// Contains information about a day to draw on the calendar
    /// </summary>
    public class CalendarRendererDayEventArgs
        : CalendarRendererEventArgs
    {
        #region Fields
        private CalendarDay _day;
        #endregion

        /// <summary>
        /// Creates a new <see cref="CalendarRendererDayEventArgs"/> object
        /// </summary>
        /// <param name="original">Orignal object to copy basic paramters</param>
        /// <param name="day">Day to render</param>
        public CalendarRendererDayEventArgs(CalendarRendererEventArgs original, CalendarDay day)
            : base(original)
        {
            _day = day;
        }

        #region Props

        /// <summary>
        /// Gets the day to paint
        /// </summary>
        public CalendarDay Day
        {
            get { return _day; }
        }


        #endregion
    }
}
