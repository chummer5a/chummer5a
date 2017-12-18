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
    /// Possible states of the calendar
    /// </summary>
    public enum CalendarState
    {
        /// <summary>
        /// Nothing happening
        /// </summary>
        Idle,

        /// <summary>
        /// User is currently dragging on view to select a time range
        /// </summary>
        DraggingTimeSelection,

        /// <summary>
        /// User is currently dragging an item among the view
        /// </summary>
        DraggingItem,

        /// <summary>
        /// User is editing an item's Text
        /// </summary>
        EditingItemText,

        /// <summary>
        /// User is currently resizing an item
        /// </summary>
        ResizingItem
    }
}
