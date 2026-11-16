/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using Chummer;

namespace ChummerAlmanac
{
    /// <summary>
    /// The in-game date, time and place a character's calendar currently shows.
    /// </summary>
    internal static class GameClock
    {
        // Used when neither the plugin nor Chummer's own calendar has a date yet.
        private static readonly DateTime s_dtFallback = new DateTime(2075, 1, 1, 8, 0, 0, DateTimeKind.Unspecified);

        /// <summary>
        /// Current in-game date and time: what the user set in the Almanac, otherwise the start of the latest week
        /// of Chummer's own calendar, otherwise 1 January 2075.
        /// </summary>
        public static DateTime GetCurrentDateTime(Character objCharacter, AlmanacData objData)
        {
            DateTime? dtStored = objData?.CurrentDateTime;
            if (dtStored.HasValue)
                return dtStored.Value;
            return GetStartOfLatestCalendarWeek(objCharacter) ?? s_dtFallback;
        }

        /// <summary>
        /// Place used for sunrise and sunset: what the user chose, otherwise the default place.
        /// </summary>
        public static AlmanacLocation GetLocation(AlmanacData objData)
        {
            return objData?.Location ?? AlmanacLocation.Default;
        }

        private static DateTime? GetStartOfLatestCalendarWeek(Character objCharacter)
        {
            if (objCharacter == null)
                return null;
            int intLatestYear = 0;
            int intLatestWeek = 0;
            foreach (CalendarWeek objWeek in objCharacter.Calendar)
            {
                if (objWeek == null)
                    continue;
                int intYear = objWeek.Year;
                int intWeek = objWeek.Week;
                if (intYear > intLatestYear || (intYear == intLatestYear && intWeek > intLatestWeek))
                {
                    intLatestYear = intYear;
                    intLatestWeek = intWeek;
                }
            }
            if (intLatestYear < 1800 || intLatestYear > 9000 || intLatestWeek < 1 || intLatestWeek > 53)
                return null;
            // ISO 8601: week 1 is the week that contains 4 January, weeks start on Monday.
            DateTime dtJanuaryFourth = new DateTime(intLatestYear, 1, 4, 8, 0, 0, DateTimeKind.Unspecified);
            int intDaysSinceMonday = ((int)dtJanuaryFourth.DayOfWeek + 6) % 7;
            return dtJanuaryFourth.AddDays(-intDaysSinceMonday).AddDays((intLatestWeek - 1) * 7);
        }
    }
}
