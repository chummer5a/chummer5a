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
    /// Contains information to render an item
    /// </summary>
    public class CalendarRendererItemEventArgs
        : CalendarRendererEventArgs
    {
        #region Fields
        private CalendarItem _item;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the Item being rendered
        /// </summary>
        public CalendarItem Item
        {
            get { return _item; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarRendererItemEventArgs"/> class.
        /// </summary>
        /// <param name="original">The <see cref="WindowsFormsCalendar.CalendarRendererEventArgs"/> instance containing the event data.</param>
        /// <param name="item">The item.</param>
        public CalendarRendererItemEventArgs( CalendarRendererEventArgs original, CalendarItem item )
            : base( original )
        {
            _item = item;

            this.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            this.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

        }

    }
}