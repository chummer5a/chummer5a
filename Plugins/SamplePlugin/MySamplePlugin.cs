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
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;
using Newtonsoft.Json;
using NLog;

namespace SamplePlugin
{
    public class MySamplePlugin : IPlugin
    {
        //Just use NLog, like we all do... or don'T and implement your own logging...
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        //If you want this plugin NOT to be visible with the default "SamplePlugin.MySamplePlugin"
        public override string ToString()
        {
            return "Sample Plugin";
        }

        public void CustomInitialize(ChummerMainForm mainControl)
        {
            try
            {
                // This function is only for "you"
                // Feel free to initialize yourself and set/change anything in the ChummerMain-Form you want.

                // If you uncomment the following line as an example, the main title of the entire program gets changed!
                //mainControl.Text = "SamplePlugin changed the title!";
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of any members in need of disposal here
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task<bool> DoCharacterList_DragDrop(object sender, DragEventArgs dragEventArgs, TreeView treCharacterList)
        {
            //if we don't want to use the dragdrop-feature, just return true
            // If you do this all synchronously, just make sure you return the final result wrapped in a task
            return Task.FromResult(true);
        }

        public Task<ICollection<TreeNode>> GetCharacterRosterTreeNode(CharacterRoster frmCharRoster, bool forceUpdate)
        {
            //here you can add nodes to the character roster.
            // If you do this all synchronously, just make sure you return the final result wrapped in a task
            return Task.FromResult<ICollection<TreeNode>>(null);
        }

        public Task<ICollection<ToolStripMenuItem>> GetMenuItems(ToolStripMenuItem menu)
        {
            //here you could add menu items to the chummer menu
            // If you do this all synchronously, just make sure you return the final result wrapped in a task
            return Task.FromResult<ICollection<ToolStripMenuItem>>(null);
        }

        public UserControl GetOptionsControl()
        {
            try
            {
                //return the UserControl for you options
                return new ucOptions();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return null;
        }

        public Assembly GetPluginAssembly()
        {
            try
            {
                //this is the first thing needed for reflection in Chummer Main. Please don't return null, but your assembly
                //that is probably bad coding AND we should change it, but for now, just stick with it...
                return GetType().Assembly;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return null;
        }

        public string GetSaveToFileElement(Character input)
        {
            try
            {
                //here you can inject your own string in every chummer save file. Preferable as json, so you can
                //a) distingish it from the main chummer elements in xml
                //b) actually use it ;)
                string savesetting = "This Char was saved with the SamplePlugin enabled!";
                return JsonConvert.SerializeObject(savesetting, Formatting.Indented);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return null;
        }

        public Task<ICollection<TabPage>> GetTabPages(CharacterCareer input)
        {
            //here you can add (or remove!) tabs from frmCareer
            //as well as manipulate every single tab
            // If you do this all synchronously, just make sure you return the final result wrapped in a task
            return Task.FromResult<ICollection<TabPage>>(null);
        }

        public Task<ICollection<TabPage>> GetTabPages(CharacterCreate input)
        {
            //the same goes for the frmCreate
            return Task.FromResult<ICollection<TabPage>>(null);
        }

        public void LoadFileElement(Character input, string fileElement)
        {
            //here you get the string "back" on load, that you (maybe) have saved before with the GetSaveToFileElement function.
            //do whatever you want with that string
            return;
        }

        public bool ProcessCommandLine(string parameter)
        {
            //here you have the chance to react to command line parameters that reference your plugin
            // the syntax is: chummer5.exe /plugin:MySamplePlugin:myparameter
            return true;
        }

        public bool SetCharacterRosterNode(TreeNode objNode)
        {
            //here you can tweak the nodes from the char-roster. Add onclickevents, change texts, reorder them - whatever you want...

            return true;
        }

        public void SetIsUnitTest(bool isUnitTest)
        {
            //In case you want to make some special initialization if you are called in a unit test, this is the place
            if (isUnitTest)
                Console.WriteLine("MySamplePlugin is in a Unit Test!");
        }

        public Microsoft.ApplicationInsights.Channel.ITelemetry SetTelemetryInitialize(Microsoft.ApplicationInsights.Channel.ITelemetry telemetry)
        {
            //here you can tweak telemetry items before they are logged.
            //if you don't know anything about logging or telemetry, just return the object
            return telemetry;
        }
    }
}
