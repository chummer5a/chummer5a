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
using System.Globalization;
using System.Windows.Forms;
using Chummer;

namespace ChummerAlmanac
{
    /// <summary>
    /// Small helpers for showing dates and texts.
    /// </summary>
    internal static class AlmanacFormat
    {
        /// <summary>
        /// Format for date and time pickers: the date as usual in Chummer's language, plus hours and minutes.
        /// </summary>
        public static string DateTimePickerFormat
        {
            get
            {
                CultureInfo objCulture = GlobalSettings.CultureInfo ?? CultureInfo.CurrentCulture;
                return objCulture.DateTimeFormat.ShortDatePattern + " HH:mm";
            }
        }

        /// <summary>
        /// A date and time as text, e.g. for the expiry of a spirit's services.
        /// </summary>
        public static string ToDisplayString(DateTime dtValue)
        {
            CultureInfo objCulture = GlobalSettings.CultureInfo ?? CultureInfo.CurrentCulture;
            return dtValue.ToString("d", objCulture) + " " + dtValue.ToString("t", objCulture);
        }

        /// <summary>
        /// Keep a date inside the range a DateTimePicker accepts.
        /// </summary>
        public static DateTime ClampToPicker(DateTime dtValue)
        {
            if (dtValue < DateTimePicker.MinimumDateTime)
                return DateTimePicker.MinimumDateTime;
            if (dtValue > DateTimePicker.MaximumDateTime)
                return DateTimePicker.MaximumDateTime;
            return dtValue;
        }

        /// <summary>
        /// Text boxes need Windows line breaks, but line breaks come back from a save file as single line feeds.
        /// </summary>
        public static string ToTextBoxLines(string strText)
        {
            if (string.IsNullOrEmpty(strText))
                return string.Empty;
            return strText.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\r\n");
        }
    }
}
