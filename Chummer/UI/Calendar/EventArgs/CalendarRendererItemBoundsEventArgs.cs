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
    /// Event arguments for calendar renter item bounds
    /// </summary>
    public class CalendarRendererItemBoundsEventArgs
        : CalendarRendererItemEventArgs
    {
        #region Fields
        private Rectangle _bounds;
        private bool _isFirst;
        private bool _isLast;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the bounds of the item to be rendered.
        /// </summary>
        /// <remarks>
        /// Items may have more than one bounds due to week segmentation.
        /// </remarks>
        public Rectangle Bounds
        {
            get { return _bounds; }
        }

        /// <summary>
        /// Gets a value indicating if the bounds are the first of the item.
        /// </summary>
        /// <remarks>
        /// Items may have more than one bounds due to week segmentation.
        /// </remarks>
        public bool IsFirst
        {
            get { return _isFirst; }
        }

        /// <summary>
        /// Gets a value indicating if the bounds are the last of the item.
        /// </summary>
        /// <remarks>
        /// Items may have more than one bounds due to week segmentation.
        /// </remarks>
        public bool IsLast
        {
            get { return _isLast; }
            set { _isLast = value; }
        }


        #endregion

        /// <summary>
        /// Creates a new Event
        /// </summary>
        /// <param name="original"></param>
        /// <param name="bounds"></param>
        /// <param name="isFirst"></param>
        /// <param name="isLast"></param>
        internal CalendarRendererItemBoundsEventArgs(CalendarRendererItemEventArgs original, Rectangle bounds, bool isFirst, bool isLast)
            : base(original, original.Item)
        {
            _isFirst = isFirst;
            _isLast = isLast;
            _bounds = bounds;

            this.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            this.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

        }

    }
}
