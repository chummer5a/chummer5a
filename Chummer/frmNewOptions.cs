using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer;
using Chummer.Classes;
using Chummer.UI.Options;

namespace Chummer
{
	public partial class frmNewOptions : Form
	{
		public frmNewOptions()
		{
			InitializeComponent();
			List<Option> _lstOptions = new List<Option>();
			XmlDocument objXmlDocument = new XmlDocument();
			string strFilePath = Path.Combine(Application.StartupPath, "data", "options.xml");
			objXmlDocument.Load(strFilePath);
			XmlNodeList objNodeList = objXmlDocument.SelectNodes("/chummer/options/chummeroptions/chummeroption");
			foreach (XmlNode objNode in objNodeList)
			{
				Option testOption = new Option();
				
				_lstOptions.Add(testOption);
			}
			if (treeView1.SelectedNode == null)
			{
				treeView1.SelectedNode = treeView1.Nodes[0];
			}
			foreach (OptionsNumberControl objControl in _lstOptions.Where(Option => Option.Category == treeView1.SelectedNode.Tag.ToString()).Select(Option => new OptionsNumberControl(Option)))
			{
				flowLayoutPanel1.Controls.Add(objControl);
			}
		}

		private void button3_Click(object sender, EventArgs e)
		{
			Option testOption = new Option();
			testOption.DescriptionTag = "Tip_CombineItems";
			testOption.ModifierTag = "Tip_SplitItems";
			OptionsNumberControl objControl = new OptionsNumberControl(testOption);
			flowLayoutPanel1.Controls.Add(objControl);
		}
	}
}
