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
            try
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
            }
            catch (Exception e)
            {
                Log.Error(e);
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
            return null;
        }

        public void LoadFileElement(Character input, string fileElement)
        {
            return;
        }

        public bool ProcessCommandLine(string parameter)
        {
            return true;
        }

        public bool SetCharacterRosterNode(System.Windows.Forms.TreeNode objNode)
        {
            return true;
        }

        public void SetIsUnitTest(bool isUnitTest)
        {
            return;
        }

        public ITelemetry SetTelemetryInitialize(ITelemetry telemetry)
        {
            return telemetry;
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
