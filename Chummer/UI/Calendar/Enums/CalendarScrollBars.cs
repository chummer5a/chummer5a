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
using System.Linq;
using System.Text;

namespace Chummer
{
    /// <summary>
    /// Enumeration for the different type of scroll bars
    /// </summary>
    public enum CalendarScrollBars
    {
        /// <summary>
        /// No scroll bars
        /// </summary>
        None,

        /// <summary>
        /// Vertical and Horizontal scrollbars
        /// </summary>
        Both,

        /// <summary>
        /// Only vertical scrollbars
        /// </summary>
        Vertical,

        /// <summary>
        /// Only horizontal scrollbars
        /// </summary>
        Horizontal
    }
}
