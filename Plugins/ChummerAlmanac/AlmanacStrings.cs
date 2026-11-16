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
using Chummer;

namespace ChummerAlmanac
{
    /// <summary>
    /// The plugin's own UI texts. Texts that Chummer already has (attribute names, "D6") come from Chummer's language files instead.
    /// </summary>
    internal static class AlmanacStrings
    {
        private static readonly Dictionary<string, string> s_dicEnglish = new Dictionary<string, string>
        {
            { "Tab_Spirits", "Spirits" },
            { "Button_Refresh", "Refresh" },
            { "Column_Spirit", "Spirit" },
            { "Column_Force", "Force" },
            { "Column_Services", "Services" },
            { "Column_Bound", "Bound" },
            { "String_Yes", "Yes" },
            { "String_No", "No" },
            { "Group_Attributes", "Attributes" },
            { "Group_Powers", "Powers" },
            { "Group_OptionalPowers", "Optional Powers" },
            { "Group_OptionalPowersCount", "Optional Powers ({0} of {1} chosen)" },
            { "Group_Skills", "Skills" },
            { "Column_Attribute", "Attribute" },
            { "Column_Value", "Value" },
            { "Column_Skill", "Skill" },
            { "Column_LinkedAttribute", "Attribute" },
            { "Column_DicePool", "Dice Pool" },
            { "Label_AstralInitiative", "Astral Initiative" },
            { "Label_Weakness", "Weakness: {0}" },
            { "Message_NoSpirits", "This character has no spirits or sprites." },
            { "Message_NoData", "No data found for this type of spirit." },
            { "Message_Error", "The spirit could not be displayed: {0}" }
        };

        private static readonly Dictionary<string, string> s_dicGerman = new Dictionary<string, string>
        {
            { "Tab_Spirits", "Geister" },
            { "Button_Refresh", "Aktualisieren" },
            { "Column_Spirit", "Geist" },
            { "Column_Force", "Stufe" },
            { "Column_Services", "Dienste" },
            { "Column_Bound", "Gebunden" },
            { "String_Yes", "Ja" },
            { "String_No", "Nein" },
            { "Group_Attributes", "Attribute" },
            { "Group_Powers", "Kr\u00e4fte" },
            { "Group_OptionalPowers", "Zus\u00e4tzliche Kr\u00e4fte" },
            { "Group_OptionalPowersCount", "Zus\u00e4tzliche Kr\u00e4fte ({0} von {1} gew\u00e4hlt)" },
            { "Group_Skills", "Fertigkeiten" },
            { "Column_Attribute", "Attribut" },
            { "Column_Value", "Wert" },
            { "Column_Skill", "Fertigkeit" },
            { "Column_LinkedAttribute", "Attribut" },
            { "Column_DicePool", "W\u00fcrfelpool" },
            { "Label_AstralInitiative", "Astrale Initiative" },
            { "Label_Weakness", "Schw\u00e4che: {0}" },
            { "Message_NoSpirits", "Dieser Charakter hat keine Geister oder Sprites." },
            { "Message_NoData", "F\u00fcr diese Geisterart wurden keine Daten gefunden." },
            { "Message_Error", "Der Geist konnte nicht angezeigt werden: {0}" }
        };

        public static string Get(string strKey)
        {
            string strLanguage = GlobalSettings.Language ?? string.Empty;
            if (strLanguage.StartsWith("de", StringComparison.OrdinalIgnoreCase)
                && s_dicGerman.TryGetValue(strKey, out string strGerman))
                return strGerman;
            return s_dicEnglish.TryGetValue(strKey, out string strEnglish) ? strEnglish : strKey;
        }
    }
}
