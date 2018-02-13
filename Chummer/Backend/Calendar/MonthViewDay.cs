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
    /// A class that represents a month view day
    /// </summary>
    public class MonthViewDay
    {
        #region Fields

        Rectangle _bounds;
        private DateTime _date;
        private MonthViewMonth _month;
        private MonthView _monthView;

        #endregion

        #region Propserties

        /// <summary>
        /// Gets the parent MonthView
        /// </summary>
        public MonthView MonthView
        {
            get { return _monthView; }
            set { _monthView = value; }
        }

        /// <summary>
        /// Gets the parent MonthViewMonth
        /// </summary>
        public MonthViewMonth Month
        {
            get { return _month; }
        }

        /// <summary>
        /// Gets the bounds of the day
        /// </summary>
        public Rectangle Bounds
        {
            get { return _bounds; }
        }

        /// <summary>
        /// Gets the date this day represents
        /// </summary>
        public DateTime Date
        {
            get { return _date; }
        }

        /// <summary>
        /// Gets or sets if the day is currently selected
        /// </summary>
        public bool Selected
        {
            get { return Date >= MonthView.SelectionStart && Date <= MonthView.SelectionEnd; }
        }

        /// <summary>
        /// Gets if the day is grayed
        /// </summary>
        public bool Grayed
        {
            get { return Month.Date.Month != Date.Month; }
        }

        /// <summary>
        /// Gets a value indicating if the day instance is visible on the calendar
        /// </summary>
        public bool Visible
        {
            get
            {
                return !( Grayed && ( Date > MonthView.ViewStart && Date < MonthView.ViewEnd ) );
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthViewDay"/> class.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="date">The date.</param>
        internal MonthViewDay(MonthViewMonth month, DateTime date)
        {
            _month = month;
            _monthView = month.MonthView;
            _date = date;

            
        }

        #region Methods

        /// <summary>
        /// Sets the value of the <see cref="Bounds"/> property
        /// </summary>
        /// <param name="bounds"></param>
        internal void SetBounds(Rectangle bounds)
        {
            _bounds = bounds;
        }

        #endregion

    }
}
