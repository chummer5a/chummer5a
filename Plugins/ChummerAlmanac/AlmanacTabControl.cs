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

using System.Globalization;
using System.Windows.Forms;
using Chummer;

namespace ChummerAlmanac
{
    /// <summary>
    /// Content of the "Almanac" tab. For now it only proves that the plugin is loaded
    /// and can read the character.
    /// </summary>
    public sealed class AlmanacTabControl : UserControl
    {
        public AlmanacTabControl(Character objCharacter)
        {
            string strVersion = typeof(AlmanacTabControl).Assembly.GetName().Version.ToString(3);
            string strCharacterName = objCharacter?.CharacterName ?? string.Empty;
            int intSpirits = objCharacter?.Spirits.Count ?? 0;

            Label lblInfo = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(6),
                Text = "Chummer Almanac " + strVersion + " loaded." + System.Environment.NewLine
                       + "Character: " + strCharacterName + System.Environment.NewLine
                       + "Spirits and sprites: " + intSpirits.ToString(CultureInfo.InvariantCulture)
            };
            Controls.Add(lblInfo);
        }
    }
}
