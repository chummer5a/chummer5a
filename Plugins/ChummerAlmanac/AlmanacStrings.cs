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
            { "Message_Error", "The spirit could not be displayed: {0}" },
            { "Tab_Calendar", "Calendar" },
            { "Button_AddHour", "+1 hour" },
            { "Button_AddDay", "+1 day" },
            { "Button_ShowCurrentDay", "Show current day" },
            { "Label_CurrentDateTime", "Current in-game date and time:" },
            { "Label_Location", "Location:" },
            { "Label_Latitude", "Latitude:" },
            { "Label_Longitude", "Longitude:" },
            { "Label_TimeZone", "Time zone:" },
            { "Location_Custom", "Custom" },
            { "Label_Sunrise", "Sunrise:" },
            { "Label_Sunset", "Sunset:" },
            { "Label_DayLength", "Length of day:" },
            { "Label_Moon", "Moon:" },
            { "Label_Notes", "Notes for this day:" },
            { "String_HoursMinutes", "{0} h {1:00} min" },
            { "String_PolarDay", "Polar day (the sun does not set)" },
            { "String_PolarNight", "Polar night (the sun does not rise)" },
            { "String_MoonPhase", "{0} ({1} % illuminated)" },
            { "Moon_0", "New moon" },
            { "Moon_1", "Waxing crescent" },
            { "Moon_2", "First quarter" },
            { "Moon_3", "Waxing gibbous" },
            { "Moon_4", "Full moon" },
            { "Moon_5", "Waning gibbous" },
            { "Moon_6", "Last quarter" },
            { "Moon_7", "Waning crescent" },
            { "Group_Summoning", "Summoning" },
            { "Label_Summoned", "Summoned at:" },
            { "Button_SummonedNow", "Now" },
            { "Expiry_NotSet", "Tick the box and enter when the spirit was summoned to see when its services expire." },
            { "Expiry_Bound", "Bound spirit: its services do not expire at sunrise or sunset." },
            { "Expiry_Sunrise", "Services expire at sunrise: {0}" },
            { "Expiry_Sunset", "Services expire at sunset: {0}" },
            { "Expiry_Expired", "{0} (expired)" },
            { "Expiry_None", "No sunrise or sunset found for this location." }
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
            { "Message_Error", "Der Geist konnte nicht angezeigt werden: {0}" },
            { "Tab_Calendar", "Kalender" },
            { "Button_AddHour", "+1 Stunde" },
            { "Button_AddDay", "+1 Tag" },
            { "Button_ShowCurrentDay", "Aktuellen Tag zeigen" },
            { "Label_CurrentDateTime", "Aktuelles Spieldatum und Uhrzeit:" },
            { "Label_Location", "Ort:" },
            { "Label_Latitude", "Breite:" },
            { "Label_Longitude", "L\u00e4nge:" },
            { "Label_TimeZone", "Zeitzone:" },
            { "Location_Custom", "Eigener Ort" },
            { "Label_Sunrise", "Sonnenaufgang:" },
            { "Label_Sunset", "Sonnenuntergang:" },
            { "Label_DayLength", "Tagesl\u00e4nge:" },
            { "Label_Moon", "Mond:" },
            { "Label_Notes", "Notizen zu diesem Tag:" },
            { "String_HoursMinutes", "{0} h {1:00} min" },
            { "String_PolarDay", "Polartag (die Sonne geht nicht unter)" },
            { "String_PolarNight", "Polarnacht (die Sonne geht nicht auf)" },
            { "String_MoonPhase", "{0} ({1} % beleuchtet)" },
            { "Moon_0", "Neumond" },
            { "Moon_1", "Zunehmende Sichel" },
            { "Moon_2", "Erstes Viertel" },
            { "Moon_3", "Zunehmender Mond" },
            { "Moon_4", "Vollmond" },
            { "Moon_5", "Abnehmender Mond" },
            { "Moon_6", "Letztes Viertel" },
            { "Moon_7", "Abnehmende Sichel" },
            { "Group_Summoning", "Beschw\u00f6rung" },
            { "Label_Summoned", "Beschworen am:" },
            { "Button_SummonedNow", "Jetzt" },
            { "Expiry_NotSet", "Haken setzen und eintragen, wann der Geist beschworen wurde, um den Verfall der Dienste zu sehen." },
            { "Expiry_Bound", "Gebundener Geist: Seine Dienste verfallen nicht bei Sonnenauf- oder -untergang." },
            { "Expiry_Sunrise", "Dienste verfallen bei Sonnenaufgang: {0}" },
            { "Expiry_Sunset", "Dienste verfallen bei Sonnenuntergang: {0}" },
            { "Expiry_Expired", "{0} (verfallen)" },
            { "Expiry_None", "F\u00fcr diesen Ort wurde kein Sonnenauf- oder -untergang gefunden." }
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
