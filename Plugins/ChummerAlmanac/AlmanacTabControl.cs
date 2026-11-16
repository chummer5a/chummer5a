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
using System.Windows.Forms;
using Chummer;

namespace ChummerAlmanac
{
    /// <summary>
    /// Content of the "Almanac" tab: one sub-tab per feature.
    /// </summary>
    public sealed class AlmanacTabControl : UserControl
    {
        public AlmanacTabControl(CharacterShared frmCharacter)
        {
            if (frmCharacter == null)
                throw new ArgumentNullException(nameof(frmCharacter));

            TabControl tabAlmanacTabs = new TabControl
            {
                Dock = DockStyle.Fill
            };

            TabPage tabSpirits = new TabPage(AlmanacStrings.Get("Tab_Spirits"))
            {
                Name = "tabAlmanacSpirits"
            };
            tabSpirits.Controls.Add(new SpiritsControl(frmCharacter)
            {
                Dock = DockStyle.Fill
            });
            tabAlmanacTabs.TabPages.Add(tabSpirits);

            TabPage tabCalendar = new TabPage(AlmanacStrings.Get("Tab_Calendar"))
            {
                Name = "tabAlmanacCalendar"
            };
            tabCalendar.Controls.Add(new CalendarControl(frmCharacter)
            {
                Dock = DockStyle.Fill
            });
            tabAlmanacTabs.TabPages.Add(tabCalendar);

            Controls.Add(tabAlmanacTabs);
        }
    }
}
