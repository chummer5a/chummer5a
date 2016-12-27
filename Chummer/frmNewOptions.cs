using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend;
using Chummer.Backend.Attributes.OptionAttributes;
using Chummer.Backend.Options;
using Chummer.Classes;
using Chummer.Datastructures;
using Chummer.UI.Options;
using Chummer.UI.Options.ControlGenerators;

namespace Chummer
{
	public partial class frmNewOptions : Form
	{
	    private Control _currentVisibleControl;
	    private AbstractOptionTree _winformTree;
	    private List<OptionItem> _searchList;
	    private List<IOptionWinFromControlFactory> _controlFactories;
	    private Lazy<OptionRender> _searchControl;

	    public frmNewOptions()
		{
			InitializeComponent();

            Load += OnLoad;
		}

	    private void OnLoad(object sender, EventArgs eventArgs)
	    {
	        _controlFactories = new List<IOptionWinFromControlFactory>()
	        {
	            new CheckBoxOptionFactory(),
	            new NumericUpDownOptionFactory()
	        };

	        //TODO: dropdown that allows you to select/add multiple
	        CharacterOptions o = Program.OptionsManager.Default;

	        OptionExtactor extactor = new OptionExtactor(
	            new List<Predicate<OptionItem>>(
	                _controlFactories.Select
	                    <IOptionWinFromControlFactory, Predicate<OptionItem>>
	                    (x => x.IsSupported)));


	        SimpleTree<OptionItem> _rawTree = extactor.Extract(o);
	        _winformTree = GenerateWinFormTree(_rawTree);
	        _winformTree.Children.Add(new BookNode(new HashSet<string>(){"SR5"}));


	        PopulateTree(treeView1.Nodes, _winformTree);

	        if (treeView1.SelectedNode == null) {treeView1.SelectedNode = treeView1.Nodes[0];}

	        MaybeSpawnAndMakeVisible(treeView1.SelectedNode);


	        _searchList = _rawTree.DepthFirstEnumerator().ToList();
	        _searchList.AddRange(extactor.BookOptions(o, GlobalOptions.Instance));
	        textBox1.KeyPress += SearchBoxChanged;

	        _searchControl = new Lazy<OptionRender>(() =>
	        {
	            OptionRender c = new OptionRender();
	            c.Factories = _controlFactories;
	            Controls.Add(c);
	            c.Location = new Point(treeView1.Right+8, 8);
	            c.Height = treeView1.Height;
	            c.Width = treeView1.Parent.Width - treeView1.Width - 36;
	            c.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
	            return c;
	        });
	    }

	    private void SearchBoxChanged(object sender, KeyPressEventArgs keyPressEventArgs)
	    {
	        if (_currentVisibleControl != null) _currentVisibleControl.Visible = false;

	        string searchfor;
	        if (keyPressEventArgs.KeyChar != '\b')
	        {
	            searchfor = textBox1.Text + keyPressEventArgs.KeyChar;
	        }
	        else if (textBox1.TextLength == 0)
	        {
	            MaybeSpawnAndMakeVisible(treeView1.SelectedNode);
	            return;
	        }
	        else
	        {
	            searchfor = textBox1.Text.Substring(0, textBox1.TextLength - 1);
	        }


	        List<OptionRenderItem> hits = _searchList
	            .Where(x => x.SearchStrings().Any(y => y.Contains(searchfor)))
	            .Select<OptionItem, OptionRenderItem>(x => x)
	            .ToList();

	        if (hits.Count > 0)
	        {
	            _searchControl.Value.SetContents(hits);


	            _currentVisibleControl = _searchControl.Value;
	            _currentVisibleControl.Visible = true;
	        }
	        else
	        {
	            _currentVisibleControl = _searchControl.Value;
	        }
	    }

	    private AbstractOptionTree GenerateWinFormTree(SimpleTree<OptionItem> tree)
	    {
	        SimpleOptionTree so = new SimpleOptionTree(tree.Tag.ToString(), new List<OptionRenderItem>(tree.Leafs), _controlFactories);
	        so.Children.AddRange(tree.Children.Select(GenerateWinFormTree));
	        return so;
	    }

	    private void MaybeSpawnAndMakeVisible(TreeNode selectedNode)
	    {
	        AbstractOptionTree tree = (AbstractOptionTree) selectedNode.Tag;
	        if (_currentVisibleControl != null) _currentVisibleControl.Visible = false;

	        if (!tree.Created)
	        {
	            Control c = tree.ControlLazy();
	            Controls.Add(c);
                c.Location = new Point(treeView1.Right+8, 8);
		        c.Height = treeView1.Height;
		        c.Width = treeView1.Parent.Width - treeView1.Width - 36;
	            c.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
	        }

	      
           _currentVisibleControl = tree.ControlLazy();
           _currentVisibleControl.Visible = true;
        }

	    private void PopulateTree(TreeNodeCollection collection, AbstractOptionTree tree)
	    {
	        foreach (AbstractOptionTree child in tree.Children)
	        {
	            TreeNode n = collection.Add(child.Name); //TODO: Should probably hit LanguageManager
	            n.Tag = child;
                PopulateTree(n.Nodes, child);
	        }
	    }

	    private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            MaybeSpawnAndMakeVisible(e.Node);
        }
    }
}
