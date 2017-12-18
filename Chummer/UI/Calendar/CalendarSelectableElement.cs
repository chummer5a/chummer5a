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
    /// Implements a basic <see cref="ICalendarSelectableElement"/>
    /// </summary>
    public abstract class CalendarSelectableElement
        : ICalendarSelectableElement
    {
        #region Fields
        
        private Calendar _calendar;
        
        private Rectangle _bounds;
        
        private DateTime _date;
        
        private bool _selected;
        
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarSelectableElement"/> class.
        /// </summary>
        /// <param name="calendar">The calendar.</param>
        public CalendarSelectableElement(Calendar calendar)
        {
            //if (calendar == null) throw new ArgumentNullException("calendar");

            _calendar = calendar;
        }

        #region ICalendarSelectableElement Members

        /// <summary>
        /// Gets the calendar
        /// </summary>
        public virtual DateTime Date
        {
            get { return _date; }
        }

        /// <summary>
        /// Gets the Calendar this element belongs to
        /// </summary>
        public virtual Calendar Calendar
        {
            get { return _calendar; }
        }

        /// <summary>
        /// Gets the Bounds of the element on the <see cref="Calendar"/> window
        /// </summary>
        public virtual Rectangle Bounds
        {
            get { return _bounds; }
        }

        /// <summary>
        /// Gets a value indicating if the element is currently selected
        /// </summary>
        public virtual bool Selected
        {
            get
            {
                return _selected;
            }
        }

        /// <summary>
        /// Compares this element with other using date as comparer
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public virtual int CompareTo(ICalendarSelectableElement element)
        {
            return this.Date.CompareTo(element.Date);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Sets the value of the <see cref="Bounds"/> property
        /// </summary>
        /// <param name="bounds">Bounds of the element</param>
        internal virtual void SetBounds(Rectangle bounds)
        {
            _bounds = bounds;
        }

        /// <summary>
        /// Sets the value of the <see cref="Selected"/> property
        /// </summary>
        /// <param name="selected">Value indicating if the element is currently selected</param>
        internal virtual void SetSelected(bool selected)
        {
            _selected = selected;

            //Calendar.Invalidate(Bounds);
        }

        #endregion

    }
}
