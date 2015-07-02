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
	    private bool _cboStageInit = false;
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


            BuildTree("[stage = \"" + _strStageName + "\" and (" + _objCharacter.Options.BookXPath() + ")]");
        }

	    private void BuildTree(String stageString)
	    {
		    treModules.Nodes.Clear();
		    treModules.Nodes.AddRange(
			    BuildList(_xmlDocument.SelectNodes("chummer/modules/module" + stageString)));
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
			BuildTree("[stage = \"" + _strStageName + "\" and (" + _objCharacter.Options.BookXPath() +")]");

			lblStage.Visible = chkLimitList.Checked;
			cboStage.Visible = !chkLimitList.Checked;

			if (cboStage.Visible)
			{
				if (cboStage.DataSource == null)
				{
					List<ListItem> Stages = new List<ListItem>()
					{
						new ListItem()
						{
							Name = LanguageManager.Instance.GetString("String_All"),
							Value = "0"
						}
					};

					XmlNodeList xnodes = _xmlDocument.SelectNodes("/chummer/stages/stage");
					foreach (XmlNode xnode in xnodes)
					{
						XmlAttribute attrib = xnode.Attributes["order"];
						if (attrib != null)
						{
							ListItem item = new ListItem();
							item.Name = xnode.InnerText;
							item.Value = xnode.Attributes["order"].Value;
							Stages.Add(item);
						}
					}

					Stages.Sort((x, y) =>
					{
						int xint = 0;
						int yint = 0;
						if (int.TryParse(x.Value, out xint))
						{
							if (int.TryParse(y.Value, out yint))
							{
								return xint - yint;
							}
							else
							{
								return 1;
							}
						}
						else
						{
							if (int.TryParse(y.Value, out yint))
							{
								return -1;
							}
							else
							{
								return 0;
							}
						}
					});

					cboStage.ValueMember = "Value";
					cboStage.DisplayMember = "Name";
					cboStage.DataSource = Stages;
				}

				ListItem selectedItem = ((List<ListItem>) cboStage.DataSource).Find(x => x.Value == _intStage.ToString());
				if (selectedItem != null)
				{
					cboStage.SelectedItem = selectedItem;
				}

			}
			else
			{
				BuildTree("[stage = \"" + _strStageName + "\" and (" + _objCharacter.Options.BookXPath() + ")]");
			}

		}

		private void cboStage_SelectionChangeCommitted(object sender, EventArgs e)
		{
			String strSelected = (String) cboStage.SelectedValue;
			if (strSelected == "0")
			{
				BuildTree("[" + _objCharacter.Options.BookXPath() + "]");
			}
			else
			{
				String Stage = _xmlDocument.SelectSingleNode("chummer/stages/stage[@order = \"" + strSelected + "\" and (" + _objCharacter.Options.BookXPath() +")]").InnerText;
				BuildTree("[stage = \"" + Stage + "\"]");
			}
			
		}
	}
}
