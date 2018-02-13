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
    /// Contains basic information about a drawing event for <see cref="CalendarRenderer"/>
    /// </summary>
    public class CalendarRendererEventArgs
        : EventArgs
    {
        #region Events

        #endregion

        #region Fields

        private Calendar _calendar;
        
        private Rectangle _clip;
        
        private Graphics _graphics;
        
        private object _tag;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the calendar where painting
        /// </summary>
        public Calendar Calendar
        {
            get { return _calendar; }
        }

        /// <summary>
        /// Gets the clip of the paint event
        /// </summary>
        public Rectangle ClipRectangle
        {
            get { return _clip; }
        }

        /// <summary>
        /// Gets the device where to paint
        /// </summary>
        public Graphics Graphics
        {
            get { return _graphics; }
        }

        /// <summary>
        /// Gets or sets a tag for the event
        /// </summary>
        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }


        #endregion

        /// <summary>
        /// Use it wisely just to initialize some stuff
        /// </summary>
        protected CalendarRendererEventArgs()
        {
            this.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            this.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        }

        /// <summary>
        /// Creates a new <see cref="CalendarRendererEventArgs"/>
        /// </summary>
        /// <param name="calendar">Calendar where painting</param>
        /// <param name="graphics">The graphics.</param>
        /// <param name="clipRectangle">The clip rectangle.</param>
        public CalendarRendererEventArgs( Calendar calendar, Graphics graphics, Rectangle clipRectangle )
        {
            _calendar = calendar;
            _graphics = graphics;
            _clip = clipRectangle;

            if( _graphics != null )
            {
                _graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                _graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                _graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                _graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            }
        }

        /// <summary>
        /// Creates a new <see cref="CalendarRendererEventArgs"/>
        /// </summary>
        /// <param name="calendar">Calendar where painting</param>
        /// <param name="graphics">The graphics.</param>
        /// <param name="clipRectangle">The clip rectangle.</param>
        /// <param name="tag">The tag.</param>
        public CalendarRendererEventArgs( Calendar calendar, Graphics graphics, Rectangle clipRectangle, object tag )
        {
            _calendar = calendar;
            _graphics = graphics;
            _clip = clipRectangle;
            _tag = tag;
            
            if( _graphics != null )
            {
                _graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                _graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                _graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                _graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            }
        }

        /// <summary>
        /// Copies the parameters from the specified <see cref="CalendarRendererEventArgs"/>
        /// </summary>
        /// <param name="original">The <see cref="WindowsFormsCalendar.CalendarRendererEventArgs"/> instance containing the event data.</param>
        public CalendarRendererEventArgs( CalendarRendererEventArgs original )
        {
            _calendar = original.Calendar;
            _graphics = original.Graphics;
            _clip = original.ClipRectangle;
            _tag = original.Tag;

            if( _graphics != null )
            {
                _graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                _graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                _graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                _graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            }
        }

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}