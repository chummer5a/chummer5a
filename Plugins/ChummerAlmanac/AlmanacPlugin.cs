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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;

namespace ChummerAlmanac
{
    /// <summary>
    /// Plugin entry point. Adds an "Almanac" tab to character windows.
    /// Spirit details and the calendar with sunrise/sunset times will live in this tab.
    /// </summary>
    public class AlmanacPlugin : IPlugin
    {
        // Name shown in Chummer's plugin list and used as the key for enabling/disabling the plugin.
        public override string ToString()
        {
            return "Almanac (Spirits & Calendar)";
        }

        public void CustomInitialize(ChummerMainForm mainControl)
        {
            // Nothing to initialize yet.
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
            if (frmCharacter == null)
                return null;
            TabPage tabAlmanac = new TabPage("Almanac")
            {
                Name = "tabAlmanac"
            };
            tabAlmanac.Controls.Add(new AlmanacTabControl(frmCharacter.CharacterObject)
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
            // Nothing is stored in the save file yet.
            return string.Empty;
        }

        public void LoadFileElement(Character input, string fileElement)
        {
            // Nothing is stored in the save file yet.
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
