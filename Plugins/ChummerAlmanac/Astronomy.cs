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

namespace ChummerAlmanac
{
    internal enum SunEventKind
    {
        Sunrise,
        Sunset
    }

    internal enum SunDayType
    {
        Normal,
        PolarDay,
        PolarNight
    }

    /// <summary>
    /// Sunrise and sunset of one calendar day at one place, as points in time in UTC.
    /// </summary>
    internal sealed class SunTimes
    {
        public SunTimes(DateTime? dtSunriseUtc, DateTime? dtSunsetUtc, SunDayType eDayType)
        {
            SunriseUtc = dtSunriseUtc;
            SunsetUtc = dtSunsetUtc;
            DayType = eDayType;
        }

        public DateTime? SunriseUtc { get; }
        public DateTime? SunsetUtc { get; }
        public SunDayType DayType { get; }
    }

    /// <summary>
    /// A sunrise or sunset in the local time of a place.
    /// </summary>
    internal sealed class SunEvent
    {
        public SunEvent(SunEventKind eKind, DateTime dtLocalTime)
        {
            Kind = eKind;
            LocalTime = dtLocalTime;
        }

        public SunEventKind Kind { get; }
        public DateTime LocalTime { get; }
    }

    /// <summary>
    /// Sun and moon calculations. No data files or network access are needed.
    /// Sunrise and sunset follow the NOAA solar calculator (based on Jean Meeus, "Astronomical Algorithms")
    /// and are accurate to about a minute outside the polar regions.
    /// </summary>
    internal static class Astronomy
    {
        private const double SynodicMonth = 29.530588853;

        // Julian day of a new moon (6 January 2000, 18:14 UTC) used as the starting point for moon phases.
        private const double ReferenceNewMoon = 2451550.1;

        // Zenith of the sun's center at sunrise/sunset, including refraction and the sun's radius.
        private const double SunriseZenith = 90.833;

        private static double ToRadians(double dblDegrees)
        {
            return dblDegrees * Math.PI / 180.0;
        }

        private static double ToDegrees(double dblRadians)
        {
            return dblRadians * 180.0 / Math.PI;
        }

        /// <summary>
        /// Julian day at 0:00 UTC of a date of the Gregorian calendar.
        /// </summary>
        internal static double JulianDay(int intYear, int intMonth, int intDay)
        {
            if (intMonth <= 2)
            {
                --intYear;
                intMonth += 12;
            }
            int intCentury = intYear / 100;
            int intCorrection = 2 - intCentury + intCentury / 4;
            return Math.Floor(365.25 * (intYear + 4716)) + Math.Floor(30.6001 * (intMonth + 1)) + intDay
                   + intCorrection - 1524.5;
        }

        /// <summary>
        /// Sunrise and sunset of a calendar day at a place.
        /// </summary>
        /// <param name="dtDate">The local calendar day. Only year, month and day are used.</param>
        /// <param name="dblLatitude">Latitude in degrees, north is positive.</param>
        /// <param name="dblLongitude">Longitude in degrees, east is positive.</param>
        public static SunTimes GetSunTimes(DateTime dtDate, double dblLatitude, double dblLongitude)
        {
            double? dblSunrise = GetSunEventMinutes(dtDate, dblLatitude, dblLongitude, true, out SunDayType eDayType);
            double? dblSunset = GetSunEventMinutes(dtDate, dblLatitude, dblLongitude, false, out SunDayType eSunsetDayType);
            if (eDayType == SunDayType.Normal)
                eDayType = eSunsetDayType;
            if (!dblSunrise.HasValue || !dblSunset.HasValue)
                return new SunTimes(null, null, eDayType == SunDayType.Normal ? SunDayType.PolarDay : eDayType);
            DateTime dtMidnightUtc = new DateTime(dtDate.Year, dtDate.Month, dtDate.Day, 0, 0, 0, DateTimeKind.Utc);
            return new SunTimes(dtMidnightUtc.AddMinutes(Math.Round(dblSunrise.Value)),
                dtMidnightUtc.AddMinutes(Math.Round(dblSunset.Value)), SunDayType.Normal);
        }

        /// <summary>
        /// Minutes from 0:00 UTC of the given date to the sunrise or sunset that belongs to this day at this place.
        /// The result can be negative or larger than a day for places far away from the prime meridian.
        /// </summary>
        private static double? GetSunEventMinutes(DateTime dtDate, double dblLatitude, double dblLongitude,
            bool blnSunrise, out SunDayType eDayType)
        {
            eDayType = SunDayType.Normal;
            double dblJulianDay = JulianDay(dtDate.Year, dtDate.Month, dtDate.Day);
            // Start at local solar noon and refine: the sun's position is recalculated for the time just found.
            double dblMinutes = 720.0 - 4.0 * dblLongitude;
            for (int i = 0; i < 3; ++i)
            {
                double dblCentury = (dblJulianDay + dblMinutes / 1440.0 - 2451545.0) / 36525.0;
                double dblMeanLongitude = Modulo(
                    280.46646 + dblCentury * (36000.76983 + dblCentury * 0.0003032), 360.0);
                double dblMeanAnomaly = 357.52911 + dblCentury * (35999.05029 - 0.0001537 * dblCentury);
                double dblEccentricity = 0.016708634 - dblCentury * (0.000042037 + 0.0000001267 * dblCentury);
                double dblAnomalyRad = ToRadians(dblMeanAnomaly);
                double dblCenter = Math.Sin(dblAnomalyRad) * (1.914602 - dblCentury * (0.004817 + 0.000014 * dblCentury))
                                   + Math.Sin(2.0 * dblAnomalyRad) * (0.019993 - 0.000101 * dblCentury)
                                   + Math.Sin(3.0 * dblAnomalyRad) * 0.000289;
                double dblOmegaRad = ToRadians(125.04 - 1934.136 * dblCentury);
                double dblApparentLongitude = dblMeanLongitude + dblCenter - 0.00569 - 0.00478 * Math.Sin(dblOmegaRad);
                double dblMeanObliquity = 23.0 + (26.0 + (21.448 - dblCentury
                    * (46.815 + dblCentury * (0.00059 - dblCentury * 0.001813))) / 60.0) / 60.0;
                double dblObliquityRad = ToRadians(dblMeanObliquity + 0.00256 * Math.Cos(dblOmegaRad));
                double dblDeclinationRad = Math.Asin(Math.Sin(dblObliquityRad) * Math.Sin(ToRadians(dblApparentLongitude)));

                double dblY = Math.Tan(dblObliquityRad / 2.0);
                dblY *= dblY;
                double dblMeanLongitudeRad = ToRadians(dblMeanLongitude);
                double dblEquationOfTime = 4.0 * ToDegrees(
                    dblY * Math.Sin(2.0 * dblMeanLongitudeRad)
                    - 2.0 * dblEccentricity * Math.Sin(dblAnomalyRad)
                    + 4.0 * dblEccentricity * dblY * Math.Sin(dblAnomalyRad) * Math.Cos(2.0 * dblMeanLongitudeRad)
                    - 0.5 * dblY * dblY * Math.Sin(4.0 * dblMeanLongitudeRad)
                    - 1.25 * dblEccentricity * dblEccentricity * Math.Sin(2.0 * dblAnomalyRad));

                double dblLatitudeRad = ToRadians(dblLatitude);
                double dblCosHourAngle = Math.Cos(ToRadians(SunriseZenith))
                                         / (Math.Cos(dblLatitudeRad) * Math.Cos(dblDeclinationRad))
                                         - Math.Tan(dblLatitudeRad) * Math.Tan(dblDeclinationRad);
                if (double.IsNaN(dblCosHourAngle) || dblCosHourAngle > 1.0)
                {
                    eDayType = SunDayType.PolarNight;
                    return null;
                }
                if (dblCosHourAngle < -1.0)
                {
                    eDayType = SunDayType.PolarDay;
                    return null;
                }
                double dblHourAngle = ToDegrees(Math.Acos(dblCosHourAngle));
                dblMinutes = 720.0 - 4.0 * (dblLongitude + (blnSunrise ? dblHourAngle : -dblHourAngle))
                             - dblEquationOfTime;
            }
            return dblMinutes;
        }

        private static double Modulo(double dblValue, double dblDivisor)
        {
            double dblResult = dblValue % dblDivisor;
            return dblResult < 0 ? dblResult + dblDivisor : dblResult;
        }

        /// <summary>
        /// The first sunrise or sunset after a point in time, in the local time of the place.
        /// Returns null if there is none within about a year (which does not happen on Earth).
        /// </summary>
        public static SunEvent GetNextSunEvent(DateTime dtLocalTime, AlmanacLocation objLocation)
        {
            if (objLocation == null)
                return null;
            TimeZoneInfo objTimeZone = objLocation.GetTimeZone();
            // The day before is included because of time zones that are far off the local solar time.
            for (int intOffset = -1; intOffset <= 400; ++intOffset)
            {
                DateTime dtDay;
                try
                {
                    dtDay = dtLocalTime.Date.AddDays(intOffset);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }
                SunTimes objTimes = GetSunTimes(dtDay, objLocation.Latitude, objLocation.Longitude);
                if (objTimes.DayType != SunDayType.Normal || !objTimes.SunriseUtc.HasValue || !objTimes.SunsetUtc.HasValue)
                    continue;
                DateTime dtSunrise = TimeZoneInfo.ConvertTimeFromUtc(objTimes.SunriseUtc.Value, objTimeZone);
                DateTime dtSunset = TimeZoneInfo.ConvertTimeFromUtc(objTimes.SunsetUtc.Value, objTimeZone);
                SunEvent objReturn = null;
                if (dtSunrise > dtLocalTime)
                    objReturn = new SunEvent(SunEventKind.Sunrise, dtSunrise);
                if (dtSunset > dtLocalTime && (objReturn == null || dtSunset < objReturn.LocalTime))
                    objReturn = new SunEvent(SunEventKind.Sunset, dtSunset);
                if (objReturn != null)
                    return objReturn;
            }
            return null;
        }

        /// <summary>
        /// Age of the moon in days since the last new moon (0 to 29.53). Accurate to about half a day.
        /// </summary>
        public static double GetMoonAge(DateTime dtUtc)
        {
            double dblJulianDay = JulianDay(dtUtc.Year, dtUtc.Month, dtUtc.Day) + dtUtc.TimeOfDay.TotalDays;
            return Modulo(dblJulianDay - ReferenceNewMoon, SynodicMonth);
        }

        /// <summary>
        /// Illuminated part of the moon's disc, from 0 (new moon) to 1 (full moon).
        /// </summary>
        public static double GetMoonIllumination(DateTime dtUtc)
        {
            return (1.0 - Math.Cos(2.0 * Math.PI * GetMoonAge(dtUtc) / SynodicMonth)) / 2.0;
        }

        /// <summary>
        /// Phase of the moon as a number from 0 to 7: new moon, waxing crescent, first quarter, waxing gibbous,
        /// full moon, waning gibbous, last quarter, waning crescent.
        /// </summary>
        public static int GetMoonPhaseIndex(DateTime dtUtc)
        {
            return (int)Math.Floor(GetMoonAge(dtUtc) / SynodicMonth * 8.0 + 0.5) % 8;
        }
    }
}
