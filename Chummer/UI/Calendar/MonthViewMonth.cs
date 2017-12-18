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
    /// Represents a month displayed"/>
    /// </summary>
    public class MonthViewMonth
    {
        #region Fields

        private DateTime _date;
        private Rectangle monthNameBounds;
        private Rectangle[] dayNamesBounds;
        private MonthViewDay[] days;
        private string[] _dayHeaders;
        private Size _size = new Size();
        private Point _location;
        private MonthView _monthview;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the bounds.
        /// </summary>
        public Rectangle Bounds
        {
            get { return new Rectangle( Location, Size ); }
        }

        /// <summary>
        /// Gets the month view.
        /// </summary>
        public MonthView MonthView
        {
            get { return _monthview; }
        }

        /// <summary>
        /// Gets the location.
        /// </summary>
        public Point Location
        {
            get { return _location; }
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        public Size Size
        {
            get { return MonthView.MonthSize; }
        }

        /// <summary>
        /// Gets or sets the days.
        /// </summary>
        /// <value>
        /// The days.
        /// </value>
        public MonthViewDay[] Days
        {
            get { return days; }
            set { days = value; }
        }

        /// <summary>
        /// Gets or sets the day names bounds.
        /// </summary>
        /// <value>
        /// The day names bounds.
        /// </value>
        public Rectangle[] DayNamesBounds
        {
            get { return dayNamesBounds; }
            set { dayNamesBounds = value; }
        }

        /// <summary>
        /// Gets or sets the day headers.
        /// </summary>
        /// <value>
        /// The day headers.
        /// </value>
        public string[] DayHeaders
        {
            get { return _dayHeaders; }
            set { _dayHeaders = value; }
        }

        /// <summary>
        /// Gets or sets the month name bounds.
        /// </summary>
        /// <value>
        /// The month name bounds.
        /// </value>
        public Rectangle MonthNameBounds
        {
            get { return monthNameBounds; }
            set { monthNameBounds = value; }
        }

        /// <summary>
        /// Gets or sets the date of the first day of the month
        /// </summary>
        public DateTime Date
        {
            get { return _date; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthViewMonth"/> class.
        /// </summary>
        /// <param name="monthView">The month view.</param>
        /// <param name="date">The date.</param>
        internal MonthViewMonth( MonthView monthView, DateTime date )
        {
            if( date.Day != 1 )
            {
                date = new DateTime( date.Year, date.Month, 1 );
            }


            _monthview = monthView;
            _date = date;

            int preDays = ( new int[] { 0, 1, 2, 3, 4, 5, 6 } )[(int)date.DayOfWeek] - (int)MonthView.FirstDayOfWeek;
            days = new MonthViewDay[6 * 7];
            DateTime curDate = date.AddDays( -preDays );
            DayHeaders = new string[7];

            for( int i = 0; i < days.Length; i++ )
            {
                days[i] = new MonthViewDay( this, curDate );

                if( i < 7 )
                {
                    DayHeaders[i] = curDate.ToString( MonthView.DayNamesFormat ).Substring( 0, MonthView.DayNamesLength );
                }

                curDate = curDate.AddDays( 1 );
            }
        }

        #region Public Methods

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the value of the <see cref="Location"/> property
        /// </summary>
        /// <param name="location"></param>
        internal void SetLocation( Point location )
        {

            int startX = location.X;
            int startY = location.Y;
            int curX = startX;
            int curY = startY;

            _location = location;

            monthNameBounds = new Rectangle( location, new Size( Size.Width, MonthView.DaySize.Height ) );

            if( MonthView.DayNamesVisible )
            {
                dayNamesBounds = new Rectangle[7];
                curY = location.Y + MonthView.DaySize.Height;
                for( int i = 0; i < dayNamesBounds.Length; i++ )
                {
                    DayNamesBounds[i] = new Rectangle( curX, curY, MonthView.DaySize.Width, MonthView.DaySize.Height );

                    curX += MonthView.DaySize.Width;
                }

            }
            else
            {
                dayNamesBounds = new Rectangle[] { };
            }

            curX = startX;
            curY = startY + MonthView.DaySize.Height * 2;

            for( int i = 0; i < Days.Length; i++ )
            {
                Days[i].SetBounds( new Rectangle( new Point( curX, curY ), MonthView.DaySize ) );

                curX += MonthView.DaySize.Width;

                if( ( i + 1 ) % 7 == 0 )
                {
                    curX = startX;
                    curY += MonthView.DaySize.Height;
                }
            }

        }

        #endregion

    }
}