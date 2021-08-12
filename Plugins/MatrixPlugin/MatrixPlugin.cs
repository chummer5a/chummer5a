using Chummer;
using Chummer.Plugins;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using NLog;
using Chummer.Backend.Equipment;
using Microsoft.ApplicationInsights.Channel;
using System.Diagnostics;
using System.Xml;

namespace MatrixPlugin
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class MatrixPlugin : IPlugin
    {
        
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        List<MatrixAction> Actions;


        public override string ToString()
        {
            return "Matrix Actions Plugin";
        }

        public void CustomInitialize(frmChummerMain mainControl)
        {
            Actions = new List<MatrixAction>();
            XmlDocument xmlComplexFormDocument = XmlManager.Load("actions.xml", null, "", false);
            foreach (XmlNode xmlAction in xmlComplexFormDocument.SelectNodes("/chummer/actions/action"))
                if (xmlAction.SelectSingleNode("test/limit") != null)
                {
                    MatrixAction newAction = new MatrixAction(xmlAction);
                    if (newAction.Attribute != "" && newAction.Skill != "")
                        Actions.Add(newAction);
                }
            return;
        }

        public void Dispose()
        {
            return;
        }

        public async Task<bool> DoCharacterList_DragDrop(object sender, System.Windows.Forms.DragEventArgs dragEventArgs, System.Windows.Forms.TreeView treCharacterList)
        {
            return true;
        }

        public async Task<ICollection<System.Windows.Forms.TreeNode>> GetCharacterRosterTreeNode(frmCharacterRoster frmCharRoster, bool forceUpdate)
        {
            return null;
        }

        public IEnumerable<System.Windows.Forms.ToolStripMenuItem> GetMenuItems(System.Windows.Forms.ToolStripMenuItem menu)
        {
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
            return null;
        }

        public IEnumerable<System.Windows.Forms.TabPage> GetTabPages(frmCareer input)
        {
            MatrixForm FormFrom = new MatrixForm(input, Actions);
            return new System.Windows.Forms.TabPage[] { FormFrom.MatrixTabPage };
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
            // the syntax is: chummer5.exe /plugin:MyMatrixPlugin:myparameter
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
                Console.WriteLine("MyMatrixPlugin is in a Unit Test!");
            return;
        }

        public ITelemetry SetTelemetryInitialize(ITelemetry telemetry)
        {
            //here you can tweak telemetry items before they are logged.
            //if you don't know anything about logging or telemetry, just return the object
            return telemetry;
            
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
