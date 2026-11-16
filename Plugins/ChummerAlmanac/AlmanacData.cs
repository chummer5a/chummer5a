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
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace ChummerAlmanac
{
    /// <summary>
    /// Data the plugin stores per character inside the character's save file:
    /// choices made for spirits and the in-game calendar.
    /// </summary>
    internal sealed class AlmanacData
    {
        private const string RootElementName = "almanac";
        private const int CurrentFormatVersion = 2;
        private const string DateTimeFormat = "yyyy-MM-dd'T'HH:mm";
        private const string DateFormat = "yyyy-MM-dd";

        private sealed class SpiritEntry
        {
            // English names of the chosen optional powers.
            public List<string> OptionalPowers { get; } = new List<string>();

            // In-game time at which the spirit was summoned, if known.
            public DateTime? Summoned { get; set; }

            public bool IsEmpty => OptionalPowers.Count == 0 && !Summoned.HasValue;
        }

        private readonly object _objLock = new object();

        // Key: internal ID of the spirit.
        private readonly Dictionary<string, SpiritEntry> _dicSpirits
            = new Dictionary<string, SpiritEntry>(StringComparer.OrdinalIgnoreCase);

        // Key: calendar day, value: the user's note for that day.
        private readonly Dictionary<DateTime, string> _dicNotes = new Dictionary<DateTime, string>();

        private DateTime? _dtCurrentDateTime;
        private AlmanacLocation _objLocation;

        #region Spirits

        public List<string> GetOptionalPowers(string strSpiritId)
        {
            lock (_objLock)
            {
                if (!string.IsNullOrEmpty(strSpiritId)
                    && _dicSpirits.TryGetValue(strSpiritId, out SpiritEntry objEntry))
                    return new List<string>(objEntry.OptionalPowers);
                return new List<string>();
            }
        }

        public void SetOptionalPowers(string strSpiritId, IEnumerable<string> lstPowers)
        {
            if (string.IsNullOrEmpty(strSpiritId))
                return;
            List<string> lstNewPowers = lstPowers != null ? new List<string>(lstPowers) : new List<string>();
            lock (_objLock)
            {
                SpiritEntry objEntry = GetOrAddSpirit(strSpiritId);
                objEntry.OptionalPowers.Clear();
                objEntry.OptionalPowers.AddRange(lstNewPowers);
                if (objEntry.IsEmpty)
                    _dicSpirits.Remove(strSpiritId);
            }
        }

        /// <summary>
        /// In-game time at which a spirit was summoned. Null if the user has not entered it.
        /// </summary>
        public DateTime? GetSummoned(string strSpiritId)
        {
            lock (_objLock)
            {
                if (!string.IsNullOrEmpty(strSpiritId)
                    && _dicSpirits.TryGetValue(strSpiritId, out SpiritEntry objEntry))
                    return objEntry.Summoned;
                return null;
            }
        }

        public void SetSummoned(string strSpiritId, DateTime? dtSummoned)
        {
            if (string.IsNullOrEmpty(strSpiritId))
                return;
            lock (_objLock)
            {
                SpiritEntry objEntry = GetOrAddSpirit(strSpiritId);
                objEntry.Summoned = dtSummoned.HasValue ? (DateTime?)TruncateToMinute(dtSummoned.Value) : null;
                if (objEntry.IsEmpty)
                    _dicSpirits.Remove(strSpiritId);
            }
        }

        // Only call while holding the lock.
        private SpiritEntry GetOrAddSpirit(string strSpiritId)
        {
            if (!_dicSpirits.TryGetValue(strSpiritId, out SpiritEntry objEntry))
            {
                objEntry = new SpiritEntry();
                _dicSpirits[strSpiritId] = objEntry;
            }
            return objEntry;
        }

        /// <summary>
        /// Forget everything stored for spirits that no longer exist on the character.
        /// </summary>
        /// <returns>True if anything was removed.</returns>
        public bool RemoveAllExcept(ICollection<string> setExistingSpiritIds)
        {
            if (setExistingSpiritIds == null)
                return false;
            lock (_objLock)
            {
                List<string> lstToRemove = new List<string>();
                foreach (string strSpiritId in _dicSpirits.Keys)
                {
                    if (!setExistingSpiritIds.Contains(strSpiritId))
                        lstToRemove.Add(strSpiritId);
                }
                foreach (string strSpiritId in lstToRemove)
                    _dicSpirits.Remove(strSpiritId);
                return lstToRemove.Count > 0;
            }
        }

        #endregion Spirits

        #region Calendar

        /// <summary>
        /// Current in-game date and time. Null until the user has set it.
        /// </summary>
        public DateTime? CurrentDateTime
        {
            get
            {
                lock (_objLock)
                    return _dtCurrentDateTime;
            }
            set
            {
                lock (_objLock)
                    _dtCurrentDateTime = value.HasValue ? (DateTime?)TruncateToMinute(value.Value) : null;
            }
        }

        /// <summary>
        /// Place used for sunrise and sunset times. Null until the user has chosen one.
        /// </summary>
        public AlmanacLocation Location
        {
            get
            {
                lock (_objLock)
                    return _objLocation;
            }
            set
            {
                lock (_objLock)
                    _objLocation = value;
            }
        }

        public string GetNote(DateTime dtDay)
        {
            lock (_objLock)
                return _dicNotes.TryGetValue(dtDay.Date, out string strNote) ? strNote : string.Empty;
        }

        public void SetNote(DateTime dtDay, string strNote)
        {
            lock (_objLock)
            {
                if (string.IsNullOrWhiteSpace(strNote))
                    _dicNotes.Remove(dtDay.Date);
                else
                    _dicNotes[dtDay.Date] = strNote;
            }
        }

        /// <summary>
        /// All days that have a note.
        /// </summary>
        public DateTime[] GetNoteDays()
        {
            lock (_objLock)
            {
                DateTime[] adtReturn = new DateTime[_dicNotes.Count];
                _dicNotes.Keys.CopyTo(adtReturn, 0);
                return adtReturn;
            }
        }

        private static DateTime TruncateToMinute(DateTime dtValue)
        {
            return new DateTime(dtValue.Year, dtValue.Month, dtValue.Day, dtValue.Hour, dtValue.Minute, 0,
                DateTimeKind.Unspecified);
        }

        #endregion Calendar

        #region Save and Load

        /// <summary>
        /// Text that goes into the plugin's element of the save file. Empty if there is nothing to store.
        /// </summary>
        public string ToSaveString()
        {
            lock (_objLock)
            {
                bool blnHasCalendar = _dtCurrentDateTime.HasValue || _objLocation != null || _dicNotes.Count > 0;
                if (_dicSpirits.Count == 0 && !blnHasCalendar)
                    return string.Empty;
                CultureInfo objInvariant = CultureInfo.InvariantCulture;
                XElement xmlRoot = new XElement(RootElementName, new XAttribute("version", CurrentFormatVersion));
                foreach (KeyValuePair<string, SpiritEntry> kvpSpirit in _dicSpirits)
                {
                    XElement xmlSpirit = new XElement("spirit", new XAttribute("id", kvpSpirit.Key));
                    if (kvpSpirit.Value.Summoned.HasValue)
                    {
                        xmlSpirit.Add(new XAttribute("summoned",
                            kvpSpirit.Value.Summoned.Value.ToString(DateTimeFormat, objInvariant)));
                    }
                    foreach (string strPower in kvpSpirit.Value.OptionalPowers)
                        xmlSpirit.Add(new XElement("optionalpower", strPower));
                    xmlRoot.Add(xmlSpirit);
                }
                if (blnHasCalendar)
                {
                    XElement xmlCalendar = new XElement("calendar");
                    if (_dtCurrentDateTime.HasValue)
                    {
                        xmlCalendar.Add(new XAttribute("now",
                            _dtCurrentDateTime.Value.ToString(DateTimeFormat, objInvariant)));
                    }
                    if (_objLocation != null)
                    {
                        xmlCalendar.Add(new XElement("location",
                            new XAttribute("name", _objLocation.Name),
                            new XAttribute("latitude", _objLocation.Latitude.ToString("R", objInvariant)),
                            new XAttribute("longitude", _objLocation.Longitude.ToString("R", objInvariant)),
                            new XAttribute("timezone", _objLocation.TimeZoneId)));
                    }
                    foreach (KeyValuePair<DateTime, string> kvpNote in _dicNotes)
                    {
                        xmlCalendar.Add(new XElement("note",
                            new XAttribute("date", kvpNote.Key.ToString(DateFormat, objInvariant)),
                            kvpNote.Value));
                    }
                    xmlRoot.Add(xmlCalendar);
                }
                return xmlRoot.ToString(SaveOptions.DisableFormatting);
            }
        }

        /// <summary>
        /// Replace the current content with what was stored in a save file.
        /// Text that cannot be read is ignored, so a damaged entry never prevents a character from loading.
        /// </summary>
        public void LoadFromSaveString(string strSaved)
        {
            Dictionary<string, SpiritEntry> dicSpirits
                = new Dictionary<string, SpiritEntry>(StringComparer.OrdinalIgnoreCase);
            Dictionary<DateTime, string> dicNotes = new Dictionary<DateTime, string>();
            DateTime? dtCurrentDateTime = null;
            AlmanacLocation objLocation = null;
            CultureInfo objInvariant = CultureInfo.InvariantCulture;

            if (!string.IsNullOrWhiteSpace(strSaved))
            {
                try
                {
                    XElement xmlRoot = XElement.Parse(strSaved, LoadOptions.PreserveWhitespace);
                    if (xmlRoot.Name.LocalName == RootElementName)
                    {
                        foreach (XElement xmlSpirit in xmlRoot.Elements("spirit"))
                        {
                            string strSpiritId = (string)xmlSpirit.Attribute("id");
                            if (string.IsNullOrEmpty(strSpiritId))
                                continue;
                            SpiritEntry objEntry = new SpiritEntry();
                            foreach (XElement xmlPower in xmlSpirit.Elements("optionalpower"))
                            {
                                if (!string.IsNullOrEmpty(xmlPower.Value))
                                    objEntry.OptionalPowers.Add(xmlPower.Value);
                            }
                            if (DateTime.TryParseExact((string)xmlSpirit.Attribute("summoned") ?? string.Empty,
                                    DateTimeFormat, objInvariant, DateTimeStyles.None, out DateTime dtSummoned))
                                objEntry.Summoned = dtSummoned;
                            if (!objEntry.IsEmpty)
                                dicSpirits[strSpiritId] = objEntry;
                        }

                        XElement xmlCalendar = xmlRoot.Element("calendar");
                        if (xmlCalendar != null)
                        {
                            if (DateTime.TryParseExact((string)xmlCalendar.Attribute("now") ?? string.Empty,
                                    DateTimeFormat, objInvariant, DateTimeStyles.None, out DateTime dtNow))
                                dtCurrentDateTime = dtNow;

                            XElement xmlLocation = xmlCalendar.Element("location");
                            if (xmlLocation != null
                                && double.TryParse((string)xmlLocation.Attribute("latitude") ?? string.Empty,
                                    NumberStyles.Float, objInvariant, out double dblLatitude)
                                && double.TryParse((string)xmlLocation.Attribute("longitude") ?? string.Empty,
                                    NumberStyles.Float, objInvariant, out double dblLongitude))
                            {
                                objLocation = new AlmanacLocation((string)xmlLocation.Attribute("name"),
                                    dblLatitude, dblLongitude, (string)xmlLocation.Attribute("timezone"));
                            }

                            foreach (XElement xmlNote in xmlCalendar.Elements("note"))
                            {
                                if (DateTime.TryParseExact((string)xmlNote.Attribute("date") ?? string.Empty,
                                        DateFormat, objInvariant, DateTimeStyles.None, out DateTime dtDay)
                                    && !string.IsNullOrWhiteSpace(xmlNote.Value))
                                    dicNotes[dtDay.Date] = xmlNote.Value;
                            }
                        }
                    }
                }
                catch (XmlException)
                {
                    dicSpirits.Clear();
                    dicNotes.Clear();
                    dtCurrentDateTime = null;
                    objLocation = null;
                }
            }

            lock (_objLock)
            {
                _dicSpirits.Clear();
                foreach (KeyValuePair<string, SpiritEntry> kvpSpirit in dicSpirits)
                    _dicSpirits[kvpSpirit.Key] = kvpSpirit.Value;
                _dicNotes.Clear();
                foreach (KeyValuePair<DateTime, string> kvpNote in dicNotes)
                    _dicNotes[kvpNote.Key] = kvpNote.Value;
                _dtCurrentDateTime = dtCurrentDateTime;
                _objLocation = objLocation;
            }
        }

        #endregion Save and Load
    }
}
