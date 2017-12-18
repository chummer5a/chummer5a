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
    /// Enumerates possible timescales for <see cref="Calendar"/> control
    /// </summary>
    public enum CalendarTimeScale
    {
        /// <summary>
        /// Makes calendar show intervals of 60 minutes
        /// </summary>
        SixtyMinutes = 60,

        /// <summary>
        /// Makes calendar show intervals of 30 minutes
        /// </summary>
        ThirtyMinutes = 30,

        /// <summary>
        /// Makes calendar show intervals of 15 minutes
        /// </summary>
        FifteenMinutes = 15,

        /// <summary>
        /// Makes calendar show intervals of 10 minutes
        /// </summary>
        TenMinutes = 10,

        /// <summary>
        /// Makes calendar show intervals of 6 minutes
        /// </summary>
        SixMinutes = 6,

        /// <summary>
        /// Makes calendar show intervals of 5 minutes
        /// </summary>
        FiveMinutes = 5
    }
}
