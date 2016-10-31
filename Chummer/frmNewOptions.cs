using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend;
using Chummer.Backend.Attributes.OptionDisplayAttributes;
using Chummer.Classes;
using Chummer.Datastructures;
using Chummer.UI.Options;

namespace Chummer
{
	public partial class frmNewOptions : Form
	{
	    private Control currentVisibleControl;
	    private AbstractOptionTree optionTree;
        public frmNewOptions()
		{
			InitializeComponent();
            //List<Option> _lstOptions = new List<Option>();
            //XmlDocument objXmlDocument = new XmlDocument();
            //string strFilePath = Path.Combine(Application.StartupPath, "data", "options.xml");
            //objXmlDocument.Load(strFilePath);
            //XmlNodeList objNodeList = objXmlDocument.SelectNodes("/chummer/options/chummeroptions/chummeroption");
            //foreach (XmlNode objNode in objNodeList)
            //{
            //	Option testOption = new Option();
            //	_lstOptions.Add(testOption);
            //}
            //if (treeView1.SelectedNode == null)
            //{
            //	treeView1.SelectedNode = treeView1.Nodes[0];
            //}
            //foreach (OptionsNumberControl objControl in _lstOptions.Where(Option => Option.Category == treeView1.SelectedNode.Tag.ToString()).Select(Option => new OptionsNumberControl(Option)))
            //{
            //	flowLayoutPanel1.Controls.Add(objControl);
            //}

            //TODO: get existing characteroptions?
            CharacterOptions o = new CharacterOptions(null);
            optionTree = GetInitialTree(o);


            PopulateTree(treeView1.Nodes, optionTree);

            if (treeView1.SelectedNode == null) treeView1.SelectedNode = treeView1.Nodes[0];
            MaybeSpawnAndMakeVisible(treeView1.SelectedNode);

            //OptionItem item = new OptionItem();
            //Controls.Add(item);
            //Controls.SetChildIndex(item, 0);
            //item.Setoptions(o.GetType().GetProperties().ToList(), o);
		}

	    private void MaybeSpawnAndMakeVisible(TreeNode selectedNode)
	    {
	        AbstractOptionTree tree = (AbstractOptionTree) selectedNode.Tag;
	        if (currentVisibleControl != null) currentVisibleControl.Visible = false;

	        if (!tree.Created)
	        {
	            Control c = tree.ControlLazy();
                Controls.Add(c);
                c.Location = new Point(treeView1.Right+8, 8);
		        c.Height = treeView1.Height;
		        c.Width = treeView1.Parent.Width - treeView1.Width - 36;
	            c.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
	        }

	      
           currentVisibleControl = tree.ControlLazy();
           currentVisibleControl.Visible = true;
        }

	    private void PopulateTree(TreeNodeCollection collection, AbstractOptionTree abstractOptionTree)
	    {
	        foreach (AbstractOptionTree child in abstractOptionTree.Children)
	        {
	            TreeNode n = collection.Add(child.Name);
	            n.Tag = child;
                PopulateTree(n.Nodes, child);
	        }
	    }

	    private static AbstractOptionTree GetInitialTree(CharacterOptions o)
	    {
	        DummyOptionTree root = new DummyOptionTree("root");

            DictionaryList<string,PropertyInfo> properties = new DictionaryList<string, PropertyInfo>();

	        string currentName = "";
	        AbstractOptionTree parentTree;
	        string[] npath;

            //BAD JOHANNES: what did we say about logic in forms?
	        //to be fair, rest of code is winform specific too

	        //Collect all properties in groups based on their option path
	        foreach (PropertyInfo info in o.GetType().GetProperties())
	        {
	            if (info.GetCustomAttribute<OptionAttributes>() != null)
	            {
	                currentName = info.GetCustomAttribute<OptionAttributes>().Path;
	            }

	            properties.Add(currentName, info);
	        }


	        foreach (KeyValuePair<string, List<PropertyInfo>> group in properties
	                    .Where(x => !string.IsNullOrWhiteSpace(x.Key))
	                    .OrderByDescending(x => x.Key))
	        {
	            string[] path = group.Key.Split('/');
	            AbstractOptionTree parrent = root;

	            //find path in option tree, skip last as thats new
	            //Breaks if trying to "jump" a path element
	            for (int i = 0; i < path.Length - 1; i++)
	            {
	                parrent = parrent.Children.First(x => x.Name == path[i]);
	            }

	            parrent.Children.Add(new SimpleOptionTree(path.Last(), o, group.Value));
	        }

            return root;
	    }

	    private void button3_Click(object sender, EventArgs e)
		{
		    /*
			Option testOption = new Option();
			testOption.DescriptionTag = "Tip_CombineItems";
			testOption.ModifierTag = "Tip_SplitItems";
			OptionsNumberControl objControl = new OptionsNumberControl(testOption);
			flowLayoutPanel1.Controls.Add(objControl);
            */

		    using (XmlTextWriter writer = new XmlTextWriter("xmltest.xml", Encoding.UTF8){Formatting = Formatting.Indented})
		    {
		        ClassSaver saver = new ClassSaver();
		        writer.WriteStartElement("settings");
		        saver.Save(new CharacterOptions(null), writer);
		        writer.WriteEndElement();
		        writer.Flush();
		    }
		}

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            MaybeSpawnAndMakeVisible(e.Node);
        }
    }
}
