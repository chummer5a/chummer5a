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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;

namespace ChummerAlmanac
{
    /// <summary>
    /// Plugin entry point. Adds an "Almanac" tab to character windows.
    /// The tab shows details of the character's spirits; a calendar with sunrise/sunset times will follow.
    /// </summary>
    public class AlmanacPlugin : IPlugin
    {
        // Chummer creates one plugin object for all characters, so the data of each character is kept here.
        // Entries disappear on their own once a character is closed.
        private static readonly ConditionalWeakTable<Character, AlmanacData> s_tblCharacterData
            = new ConditionalWeakTable<Character, AlmanacData>();

        /// <summary>
        /// The plugin's data for a character. Created empty if the character has none yet.
        /// </summary>
        internal static AlmanacData GetData(Character objCharacter)
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            return s_tblCharacterData.GetValue(objCharacter, x => new AlmanacData());
        }

        // Name shown in Chummer's plugin list and used as the key for enabling/disabling the plugin.
        public override string ToString()
        {
            return "Almanac (Spirits & Calendar)";
        }

        public void CustomInitialize(ChummerMainForm mainControl)
        {
            // Nothing to initialize.
        }

        public Task<ICollection<TabPage>> GetTabPages(CharacterCareer input, CancellationToken token = default)
        {
            return Task.FromResult(CreateTabPages(input));
        }

        public Task<ICollection<TabPage>> GetTabPages(CharacterCreate input, CancellationToken token = default)
        {
            return Task.FromResult(CreateTabPages(input));
        }

        private static ICollection<TabPage> CreateTabPages(CharacterShared frmCharacter)
        {
            if (frmCharacter?.CharacterObject == null)
                return null;
            TabPage tabAlmanac = new TabPage("Almanac")
            {
                Name = "tabAlmanac"
            };
            tabAlmanac.Controls.Add(new AlmanacTabControl(frmCharacter)
            {
                Dock = DockStyle.Fill
            });
            return new[] { tabAlmanac };
        }

        public Task<ICollection<ToolStripMenuItem>> GetMenuItems(ToolStripMenuItem menu, CancellationToken token = default)
        {
            return Task.FromResult<ICollection<ToolStripMenuItem>>(null);
        }

        public Microsoft.ApplicationInsights.Channel.ITelemetry SetTelemetryInitialize(Microsoft.ApplicationInsights.Channel.ITelemetry telemetry)
        {
            return telemetry;
        }

        public bool ProcessCommandLine(string parameter)
        {
            return true;
        }

        public Task<ICollection<TreeNode>> GetCharacterRosterTreeNode(CharacterRoster frmCharRoster, bool forceUpdate, CancellationToken token = default)
        {
            return Task.FromResult<ICollection<TreeNode>>(null);
        }

        public UserControl GetOptionsControl()
        {
            // No options yet. The calendar location setting will go here.
            return null;
        }

        public string GetSaveToFileElement(Character input)
        {
            // Chummer writes this text into the plugin's own element of the save file.
            if (input == null || !s_tblCharacterData.TryGetValue(input, out AlmanacData objData))
                return string.Empty;
            return objData.ToSaveString();
        }

        public void LoadFileElement(Character input, string fileElement)
        {
            if (input == null)
                return;
            GetData(input).LoadFromSaveString(fileElement);
        }

        public void SetIsUnitTest(bool isUnitTest)
        {
        }

        public Assembly GetPluginAssembly()
        {
            return typeof(AlmanacPlugin).Assembly;
        }

        public bool SetCharacterRosterNode(TreeNode objNode)
        {
            return true;
        }

        public Task<bool> DoCharacterList_DragDrop(object sender, DragEventArgs dragEventArgs, TreeView treCharacterList, CancellationToken token = default)
        {
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
