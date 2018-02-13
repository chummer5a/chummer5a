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
using System.Windows.Forms;

namespace Chummer
{
    /// <summary>
    /// Represents a day present on the <see cref="Calendar"/> control's view.
    /// </summary>
    public class CalendarDay
        : CalendarSelectableElement
    {
        #region Static

        private Size overflowSize = new Size(16, 16);
        private Padding overflowPadding = new Padding(5);

        #endregion

        #region Events

        #endregion

        #region Fields
        private List<CalendarItem> _containedItems;
        private Calendar _calendar;
        private DateTime _date;
        private CalendarDayTop _dayTop;
        private int _index;
        private bool _overflowStart;
        private bool _overflowEnd;
        private bool _overflowStartSelected;
        private bool _overlowEndSelected;
        private CalendarTimeScaleUnit[] _timeUnits;
        #endregion

        #region Properties

        /// <summary>
        /// Gets a list of items contained on the day
        /// </summary>
        internal List<CalendarItem> ContainedItems
        {
            get { return _containedItems; }
        }

        /// <summary>
        /// Gets the DayTop of the day, the place where multi-day and all-day items are placed
        /// </summary>
        public CalendarDayTop DayTop
        {
            get { return _dayTop; }
        }

        /// <summary>
        /// Gets the bounds of the body of the day (where time-based CalendarItems are placed)
        /// </summary>
        public Rectangle BodyBounds
        {
            get 
            {
                return Rectangle.FromLTRB(Bounds.Left, DayTop.Bounds.Bottom, Bounds.Right, Bounds.Bottom);
            }
        }

        /// <summary>
        /// Gets the date this day represents
        /// </summary>
        public override DateTime Date
        {
            get { return _date; }
        }

        /// <summary>
        /// Gets the bounds of the header of the day
        /// </summary>
        public Rectangle HeaderBounds
        {
            get 
            {
                return new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, Calendar.Renderer.DayHeaderHeight);
            }
        }

        /// <summary>
        /// Gets the index of this day on the calendar
        /// </summary>
        public int Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Gets a value indicating if the day is specified on the view (See remarks).
        /// </summary>
        /// <remarks>
        /// A day may not be specified on the view, but still present to make up a square calendar.
        /// This days should be drawn in a way that indicates it's necessary but unrequested presence.
        /// </remarks>
        public bool SpecifiedOnView
        {
            get 
            {
                return Date.CompareTo(Calendar.ViewStart) >= 0 && Date.CompareTo(Calendar.ViewEnd) <= 0;
            }
        }

        /// <summary>
        /// Gets the time units contained on the day
        /// </summary>
        public CalendarTimeScaleUnit[] TimeUnits
        {
            get { return _timeUnits; }
        }

        /// <summary>
        /// /// <summary>
        /// Gets a value indicating if the day contains items not shown through the start of the day
        /// </summary>
        /// </summary>
        public bool OverflowStart
        {
            get { return _overflowStart; }
        }

        /// <summary>
        /// Gets the bounds of the <see cref="OverflowStart"/> indicator
        /// </summary>
        public virtual Rectangle OverflowStartBounds
        {
            get { return new Rectangle(new Point(Bounds.Right - overflowPadding.Right - overflowSize.Width, Bounds.Top + overflowPadding.Top), overflowSize); }
        }

        /// <summary>
        /// Gets a value indicating if the <see cref="OverflowStart"/> indicator is currently selected
        /// </summary>
        /// <remarks>
        /// This value set to <c>true</c> when user hovers the mouse on the <see cref="OverflowStartBounds"/> area
        /// </remarks>
        public bool OverflowStartSelected
        {
            get { return _overflowStartSelected; }
        }

        /// <summary>
        /// Gets a value indicating if the day contains items not shown through the end of the day
        /// </summary>
        public bool OverflowEnd
        {
            get { return _overflowEnd; }
        }

        /// <summary>
        /// Gets the bounds of the <see cref="OverflowEnd"/> indicator
        /// </summary>
        public virtual Rectangle OverflowEndBounds
        {
            get { return new Rectangle(new Point(Bounds.Right - overflowPadding.Right - overflowSize.Width, Bounds.Bottom - overflowPadding.Bottom - overflowSize.Height), overflowSize); }
        }

        /// <summary>
        /// Gets a value indicating if the <see cref="OverflowEnd"/> indicator is currently selected
        /// </summary>
        /// <remarks>
        /// This value set to <c>true</c> when user hovers the mouse on the <see cref="OverflowStartBounds"/> area
        /// </remarks>
        public bool OverflowEndSelected
        {
            get { return _overlowEndSelected; }
        }


        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarDay"/> class.
        /// </summary>
        /// <param name="calendar">The calendar.</param>
        /// <param name="date">The date.</param>
        /// <param name="index">The index.</param>
        internal CalendarDay(Calendar calendar, DateTime date, int index)
            : base(calendar)
        {
            _containedItems = new List<CalendarItem>();
            _calendar = calendar;
            _dayTop = new CalendarDayTop(this);
            _date = date;
            _index = index;

            UpdateUnits();
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
            return Date.ToShortDateString();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds an item to the <see cref="ContainedItems"/> list if not in yet
        /// </summary>
        /// <param name="item"></param>
        internal void AddContainedItem(CalendarItem item)
        {
            if (!ContainedItems.Contains(item))
            {
                ContainedItems.Add(item);
            }
        }

        /// <summary>
        /// Sets the value of he <see cref="OverflowEnd"/> property
        /// </summary>
        /// <param name="overflow">Value of the property</param>
        internal void SetOverflowEnd(bool overflow)
        {
            _overflowEnd = overflow;
        }

        /// <summary>
        /// Sets the value of the <see cref="OverflowEndSelected"/> property
        /// </summary>
        /// <param name="selected">Value to pass to the property</param>
        internal void SetOverflowEndSelected(bool selected)
        {
            _overlowEndSelected= selected;
        }

        /// <summary>
        /// Sets the value of he <see cref="OverflowStart"/> property
        /// </summary>
        /// <param name="overflow">Value of the property</param>
        internal void SetOverflowStart(bool overflow)
        {
            _overflowStart = overflow;
        }

        /// <summary>
        /// Sets the value of the <see cref="OverflowStartSelected"/> property
        /// </summary>
        /// <param name="selected">Value to pass to the property</param>
        internal void SetOverflowStartSelected(bool selected)
        {
            _overflowStartSelected = selected;
        }

        /// <summary>
        /// Updates the value of <see cref="TimeUnits"/> property
        /// </summary>
        internal void UpdateUnits()
        {
            int factor = 0;

            switch (Calendar.TimeScale)
            {
                case CalendarTimeScale.SixtyMinutes:    factor = 1;     break;
                case CalendarTimeScale.ThirtyMinutes:   factor = 2;     break;
                case CalendarTimeScale.FifteenMinutes:  factor = 4;     break;
                case CalendarTimeScale.TenMinutes:      factor = 6;     break;
                case CalendarTimeScale.SixMinutes:      factor = 10;    break;
                case CalendarTimeScale.FiveMinutes:     factor = 12;    break;
                default: throw new NotImplementedException("TimeScale not supported");
            }

            _timeUnits = new CalendarTimeScaleUnit[24 * factor];
            
            int hourSum = 0;
            int minSum = 0;

            for (int i = 0; i < _timeUnits.Length; i++)
            {
                _timeUnits[i] = new CalendarTimeScaleUnit(this, i, hourSum, minSum);

                minSum += 60 / factor;

                if (minSum >= 60)
                {
                    minSum = 0;
                    hourSum++;
                }
            }

            UpdateHighlights();
        }

        /// <summary>
        /// Updates the highlights of the units
        /// </summary>
        internal void UpdateHighlights()
        {
            if (TimeUnits == null) 
                return;

            for (int i = 0; i < TimeUnits.Length; i++)
            {
                TimeUnits[i].SetHighlighted(TimeUnits[i].CheckHighlighted());
            }
        }

        #endregion

    }
}
