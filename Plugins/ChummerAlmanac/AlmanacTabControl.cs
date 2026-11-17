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
using Chummer.Backend.Skills;

namespace ChummerAlmanac
{
    /// <summary>
    /// Content of the "Almanac" tab: one sub-tab per feature.
    /// </summary>
    public sealed class AlmanacTabControl : UserControl
    {
        private readonly CharacterShared _frmCharacter;
        private readonly TabControl _tabAlmanacTabs;
        private readonly TabPage _tabSpirits;

        public AlmanacTabControl(CharacterShared frmCharacter)
        {
            _frmCharacter = frmCharacter ?? throw new ArgumentNullException(nameof(frmCharacter));

            TabControl tabAlmanacTabs = new TabControl
            {
                Dock = DockStyle.Fill
            };
            _tabAlmanacTabs = tabAlmanacTabs;

            TabPage tabSpirits = new TabPage(AlmanacStrings.Get("Tab_Spirits"))
            {
                Name = "tabAlmanacSpirits"
            };
            tabSpirits.Controls.Add(new SpiritsControl(frmCharacter)
            {
                Dock = DockStyle.Fill
            });
            _tabSpirits = tabSpirits;

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

            UpdateSpiritsTab();
            // The skills can change while the character is open, so check again whenever the tab is shown.
            VisibleChanged += (sender, e) =>
            {
                if (Visible)
                    UpdateSpiritsTab();
            };
        }

        /// <summary>
        /// Show the spirits sub-tab only for characters who can summon spirits or compile sprites.
        /// </summary>
        private void UpdateSpiritsTab()
        {
            bool blnShow = CanSummon(_frmCharacter.CharacterObject);
            bool blnShown = _tabAlmanacTabs.TabPages.Contains(_tabSpirits);
            if (blnShow == blnShown)
                return;
            TabPage tabSelected = _tabAlmanacTabs.SelectedTab;
            if (blnShow)
                _tabAlmanacTabs.TabPages.Insert(0, _tabSpirits);
            else
                _tabAlmanacTabs.TabPages.Remove(_tabSpirits);
            if (tabSelected != null && _tabAlmanacTabs.TabPages.Contains(tabSelected))
                _tabAlmanacTabs.SelectedTab = tabSelected;
        }

        /// <summary>
        /// True if the character has at least one rating in the skill for summoning spirits ("Summoning",
        /// "Herbeirufen" in the German data) or for compiling sprites ("Compiling").
        /// </summary>
        private static bool CanSummon(Character objCharacter)
        {
            SkillsSection objSkills = objCharacter?.SkillsSection;
            if (objSkills == null)
                return true;
            try
            {
                return (objSkills.GetActiveSkill("Summoning")?.Rating ?? 0) > 0
                       || (objSkills.GetActiveSkill("Compiling")?.Rating ?? 0) > 0;
            }
            catch (Exception)
            {
                // If the skills cannot be read, rather show the tab than hide it.
                return true;
            }
        }
    }
}
