using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chummer
{
    /// <summary>
    /// Represents the different kinds of selection in MonthView
    /// </summary>
    public enum MonthViewSelection
    {
        /// <summary>
        /// User can select whatever date available to mouse reach
        /// </summary>
        Manual,

        /// <summary>
        /// Selection is limited to just one day
        /// </summary>
        Day,

        /// <summary>
        /// Selecion is limited to <see cref="DayOfWeek"/> weekly ranges
        /// </summary>
        WorkWeek,

        /// <summary>
        /// Selection is limited to a full week
        /// </summary>
        Week,

        /// <summary>
        /// Selection is limited to a full month
        /// </summary>
        Month
    }
}