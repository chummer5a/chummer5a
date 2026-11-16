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
using System.Xml;
using System.Xml.Linq;

namespace ChummerAlmanac
{
    /// <summary>
    /// Data the plugin stores per character inside the character's save file.
    /// Currently: the optional powers chosen for each spirit.
    /// </summary>
    internal sealed class AlmanacData
    {
        private const string RootElementName = "almanac";
        private const int CurrentFormatVersion = 1;

        private readonly object _objLock = new object();

        // Key: internal ID of the spirit, value: English names of the chosen optional powers.
        private readonly Dictionary<string, List<string>> _dicOptionalPowers
            = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        public List<string> GetOptionalPowers(string strSpiritId)
        {
            lock (_objLock)
            {
                if (!string.IsNullOrEmpty(strSpiritId)
                    && _dicOptionalPowers.TryGetValue(strSpiritId, out List<string> lstPowers))
                    return new List<string>(lstPowers);
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
                if (lstNewPowers.Count == 0)
                    _dicOptionalPowers.Remove(strSpiritId);
                else
                    _dicOptionalPowers[strSpiritId] = lstNewPowers;
            }
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
                foreach (string strSpiritId in _dicOptionalPowers.Keys)
                {
                    if (!setExistingSpiritIds.Contains(strSpiritId))
                        lstToRemove.Add(strSpiritId);
                }
                foreach (string strSpiritId in lstToRemove)
                    _dicOptionalPowers.Remove(strSpiritId);
                return lstToRemove.Count > 0;
            }
        }

        /// <summary>
        /// Text that goes into the plugin's element of the save file. Empty if there is nothing to store.
        /// </summary>
        public string ToSaveString()
        {
            lock (_objLock)
            {
                if (_dicOptionalPowers.Count == 0)
                    return string.Empty;
                XElement xmlRoot = new XElement(RootElementName, new XAttribute("version", CurrentFormatVersion));
                foreach (KeyValuePair<string, List<string>> kvpSpirit in _dicOptionalPowers)
                {
                    XElement xmlSpirit = new XElement("spirit", new XAttribute("id", kvpSpirit.Key));
                    foreach (string strPower in kvpSpirit.Value)
                        xmlSpirit.Add(new XElement("optionalpower", strPower));
                    xmlRoot.Add(xmlSpirit);
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
            Dictionary<string, List<string>> dicLoaded
                = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(strSaved))
            {
                try
                {
                    XElement xmlRoot = XElement.Parse(strSaved);
                    if (xmlRoot.Name.LocalName == RootElementName)
                    {
                        foreach (XElement xmlSpirit in xmlRoot.Elements("spirit"))
                        {
                            string strSpiritId = (string)xmlSpirit.Attribute("id");
                            if (string.IsNullOrEmpty(strSpiritId))
                                continue;
                            List<string> lstPowers = new List<string>();
                            foreach (XElement xmlPower in xmlSpirit.Elements("optionalpower"))
                            {
                                if (!string.IsNullOrEmpty(xmlPower.Value))
                                    lstPowers.Add(xmlPower.Value);
                            }
                            if (lstPowers.Count > 0)
                                dicLoaded[strSpiritId] = lstPowers;
                        }
                    }
                }
                catch (XmlException)
                {
                    dicLoaded.Clear();
                }
            }
            lock (_objLock)
            {
                _dicOptionalPowers.Clear();
                foreach (KeyValuePair<string, List<string>> kvpSpirit in dicLoaded)
                    _dicOptionalPowers[kvpSpirit.Key] = kvpSpirit.Value;
            }
        }
    }
}
