using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
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

        public void CustomInitialize(frmChummerMain mainControl)
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

        public void Dispose()
        {
            return;
        }

        public async Task<bool> DoCharacterList_DragDrop(object sender, System.Windows.Forms.DragEventArgs dragEventArgs, System.Windows.Forms.TreeView treCharacterList)
        {
            //if we don't want to use the dragdrop-feature, just return true
            return true;
        }

        public async Task<ICollection<System.Windows.Forms.TreeNode>> GetCharacterRosterTreeNode(frmCharacterRoster frmCharRoster, bool forceUpdate)
        {
            //here you can add nodes to the character roster.
            return null;
        }

        public IEnumerable<System.Windows.Forms.ToolStripMenuItem> GetMenuItems(System.Windows.Forms.ToolStripMenuItem menu)
        {
            //here you could add menu items to the chummer menu
            return null;
        }

        public System.Windows.Forms.UserControl GetOptionsControl()
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
                return this.GetType().Assembly;
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
                var savesetting = "This Char was saved with the SamplePlugin enabled!";
                return JsonConvert.SerializeObject(savesetting, Formatting.Indented);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return null;
        }

        public IEnumerable<System.Windows.Forms.TabPage> GetTabPages(frmCareer input)
        {
            //here you can add (or remove!) tabs from frmCareer
            //as well as manipulate every single tab
            return null;
        }

        public IEnumerable<System.Windows.Forms.TabPage> GetTabPages(frmCreate input)
        {
            //the same goes for the frmCreate
            return null;
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

        public bool SetCharacterRosterNode(System.Windows.Forms.TreeNode objNode)
        {
            //here you can tweak the nodes from the char-roster. Add onclickevents, change texts, reorder them - whatever you want...

            return true;
        }

        public void SetIsUnitTest(bool isUnitTest)
        {
            //In case you want to make some special initialization if you are called in a unit test, this is the place
            if (isUnitTest)
                Console.WriteLine("MySamplePlugin is in a Unit Test!");
            return;
        }

        public Microsoft.ApplicationInsights.Channel.ITelemetry SetTelemetryInitialize(Microsoft.ApplicationInsights.Channel.ITelemetry telemetry)
        {
            //here you can tweak telemetry items before they are logged.
            //if you don't know anything about logging or telemetry, just return the object
            return telemetry;
        }
    }
}
