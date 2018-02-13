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
    /// Contains information about a <see cref="CalendarTimeScaleUnit"/> that is about to be painted
    /// </summary>
    public class CalendarRendererTimeUnitEventArgs
        : CalendarRendererEventArgs
    {
        #region Events

        #endregion

        #region Fields
        private CalendarTimeScaleUnit _unit;
        #endregion

        #region Properties 

        /// <summary>
        /// Gets the unit that is about to be painted
        /// </summary>
        public CalendarTimeScaleUnit Unit
        {
            get { return _unit; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarRendererTimeUnitEventArgs"/> class.
        /// </summary>
        /// <param name="original">The <see cref="WindowsFormsCalendar.CalendarRendererEventArgs"/> instance containing the event data.</param>
        /// <param name="unit">The unit.</param>
        public CalendarRendererTimeUnitEventArgs(CalendarRendererEventArgs original, CalendarTimeScaleUnit unit)
            : base(original)
        {
            _unit = unit;

            this.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            this.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

        }

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion
    }
}
