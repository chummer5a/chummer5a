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
using System.Collections.Generic;

namespace ChummerAlmanac
{
    /// <summary>
    /// A place on Earth with its time zone, used for sunrise and sunset times.
    /// </summary>
    internal sealed class AlmanacLocation
    {
        public const string CustomName = "Custom";

        public AlmanacLocation(string strName, double dblLatitude, double dblLongitude, string strTimeZoneId)
        {
            Name = string.IsNullOrEmpty(strName) ? CustomName : strName;
            Latitude = Math.Max(-90.0, Math.Min(90.0, dblLatitude));
            Longitude = Math.Max(-180.0, Math.Min(180.0, dblLongitude));
            TimeZoneId = strTimeZoneId ?? string.Empty;
        }

        public string Name { get; }

        /// <summary>
        /// Latitude in degrees, north is positive.
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// Longitude in degrees, east is positive.
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// ID of the time zone as Windows knows it, e.g. "Pacific Standard Time".
        /// </summary>
        public string TimeZoneId { get; }

        /// <summary>
        /// The time zone of this place. Falls back to UTC if the computer does not know the time zone.
        /// </summary>
        public TimeZoneInfo GetTimeZone()
        {
            if (!string.IsNullOrEmpty(TimeZoneId))
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
                }
                catch (TimeZoneNotFoundException)
                {
                    // fall through to UTC
                }
                catch (InvalidTimeZoneException)
                {
                    // fall through to UTC
                }
            }
            return TimeZoneInfo.Utc;
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// The place used until the user picks one.
        /// </summary>
        public static AlmanacLocation Default => Presets[0];

        /// <summary>
        /// Places offered for selection. Names are real-world city names.
        /// </summary>
        public static IReadOnlyList<AlmanacLocation> Presets { get; } = new[]
        {
            new AlmanacLocation("Seattle", 47.6062, -122.3321, "Pacific Standard Time"),
            new AlmanacLocation("San Francisco", 37.7749, -122.4194, "Pacific Standard Time"),
            new AlmanacLocation("Los Angeles", 34.0522, -118.2437, "Pacific Standard Time"),
            new AlmanacLocation("Denver", 39.7392, -104.9903, "Mountain Standard Time"),
            new AlmanacLocation("Chicago", 41.8781, -87.6298, "Central Standard Time"),
            new AlmanacLocation("Atlanta", 33.7490, -84.3880, "Eastern Standard Time"),
            new AlmanacLocation("New York", 40.7128, -74.0060, "Eastern Standard Time"),
            new AlmanacLocation("Boston", 42.3601, -71.0589, "Eastern Standard Time"),
            new AlmanacLocation("London", 51.5074, -0.1278, "GMT Standard Time"),
            new AlmanacLocation("Paris", 48.8566, 2.3522, "Romance Standard Time"),
            new AlmanacLocation("Hamburg", 53.5511, 9.9937, "W. Europe Standard Time"),
            new AlmanacLocation("Berlin", 52.5200, 13.4050, "W. Europe Standard Time"),
            new AlmanacLocation("Frankfurt", 50.1109, 8.6821, "W. Europe Standard Time"),
            new AlmanacLocation("Munich", 48.1351, 11.5820, "W. Europe Standard Time"),
            new AlmanacLocation("Zurich", 47.3769, 8.5417, "W. Europe Standard Time"),
            new AlmanacLocation("Vienna", 48.2082, 16.3738, "W. Europe Standard Time"),
            new AlmanacLocation("Cape Town", -33.9249, 18.4241, "South Africa Standard Time"),
            new AlmanacLocation("Hong Kong", 22.3193, 114.1694, "China Standard Time"),
            new AlmanacLocation("Tokyo", 35.6762, 139.6503, "Tokyo Standard Time"),
            new AlmanacLocation("Sydney", -33.8688, 151.2093, "AUS Eastern Standard Time")
        };
    }
}
