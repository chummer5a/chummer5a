using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectLifeModule : Form
    {
        public bool AddAgain { get; private set; }
        private Character _objCharacter;
        private int _intStage;
        private String _strStageName;
        private XmlDocument _xmlDocument;
        private String _selectedId;
        
        public frmSelectLifeModule(Character objCharacter, int stage)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;
            _intStage = stage;
            MoveControls();
        }

        private void frmSelectLifeModule_Load(object sender, EventArgs e)
        {
            MoveControls();

            _xmlDocument = XmlManager.Instance.Load("lifemodules.xml");
            String selectString = "chummer/stages/stage[@order = \"" + _intStage + "\"]";

            XmlNode stageNode = _xmlDocument.SelectSingleNode(selectString);
            _strStageName = stageNode.InnerText;


            BuildTree(_strStageName);
        }

	    private void BuildTree(String stage)
	    {
		    treModules.Nodes.Clear();
		    treModules.Nodes.AddRange(
			    BuildList(_xmlDocument.SelectNodes("chummer/modules/module[stage = \"" + stage + "\"]")));
	    }

	    private TreeNode[] BuildList(XmlNodeList xmlNodes)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            for (int i = 0; i < xmlNodes.Count; i++)
            {
                XmlNode xmlNode = xmlNodes[i];

	            if (Quality.IsValid(_objCharacter, xmlNode) || !chkLimitList.Checked)
	            {

		            TreeNode treNode = new TreeNode();

		            treNode.Text = xmlNode["name"].InnerText;
		            if (xmlNode["versions"] != null)
		            {
			            treNode.Nodes.AddRange(
				            BuildList(xmlNode.SelectNodes("versions/version")));
		            }

		            treNode.Tag = xmlNode["id"].InnerText;
		            nodes.Add(treNode);
	            }
            }

            return nodes.ToArray();
        }
        
        private void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            this.DialogResult = DialogResult.OK;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            this.DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblBPLabel.Width, lblSourceLabel.Width);
            lblBP.Left = lblBPLabel.Left + intWidth + 6;
            lblSource.Left = lblSourceLabel.Left + intWidth + 6;

            lblSearch.Left = textBox1.Left - 6 - lblSearch.Width;
        }

        private void treModules_AfterSelect(object sender, TreeViewEventArgs e)
        {

            bool blnSelectAble;
            if (e.Node.Nodes.Count == 0)
            {
                blnSelectAble = true;
            }
            else
            {
                //Select any node that have an id node equal to tag
                String selectString = "//*[id = \"" + e.Node.Tag + "\"]/selectable";
                XmlNode node = _xmlDocument.SelectSingleNode(selectString);
                //if it contains >selectable>true</selectable>, yes or </selectable>
                //set button to selectable, otherwise not
                blnSelectAble = (node != null &&
                                 (node.InnerText == "true" || node.InnerText == "yes" || node.OuterXml.EndsWith("/>")));
            }

            XmlNode selectedNodeInfo = Quality.GetNodeOverrideable((string) e.Node.Tag);
            _selectedId = (string) e.Node.Tag;

            cmdOK.Enabled = blnSelectAble;
            cmdOKAdd.Enabled = blnSelectAble;

            lblBP.Text = selectedNodeInfo["karma"] != null ? selectedNodeInfo["karma"].InnerText : "";
            lblSource.Text = (selectedNodeInfo["source"] != null ? selectedNodeInfo["source"].InnerText : "") +
                             " " + (selectedNodeInfo["page"] != null ? selectedNodeInfo["page"].InnerText : "");

            lblStage.Text = selectedNodeInfo["stage"] != null ? selectedNodeInfo["stage"].InnerText : "";


        }

        public XmlNode SelectedNode
        {
            get { return Quality.GetNodeOverrideable(_selectedId); }
        }

		private void treModules_DoubleClick(object sender, EventArgs e)
		{
			if (cmdOK.Enabled)
			{
				AddAgain = false;
				cmdOK_Click(sender, e);
			}
		}

		private void chkLimitList_Click(object sender, EventArgs e)
		{
			BuildTree(_strStageName);
		}
	}
}
