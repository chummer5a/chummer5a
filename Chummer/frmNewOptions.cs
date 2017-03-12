using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
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
using Microsoft.Win32;

namespace Chummer
{
	public partial class frmNewOptions : Form
	{
	    private Control _currentVisibleControl;
	    private AbstractOptionTree _winformTree;
	    private OptionCollectionCache _options;
	    private Lazy<OptionRender> _searchControl;

	    public frmNewOptions()
		{
			InitializeComponent();

            Load += OnLoad;
		}

	    private void OnLoad(object sender, EventArgs eventArgs)
	    {
	        List<IOptionWinFromControlFactory> controlFactories = new List<IOptionWinFromControlFactory>()
	        {
	            new CheckBoxOptionFactory(),
	            new NumericUpDownOptionFactory(),
                new BookOptionFactory(),
	            new PathSelectiorFactory()
	        };

	        //TODO: dropdown that allows you to select/add multiple
            //TODO: When doing so, remember to include selection login in btnReset_Click
	        CharacterOptions o = Program.OptionsManager.Default;

	        OptionExtractor extactor = new OptionExtractor(
	            new List<Predicate<OptionItem>>(
	                controlFactories.Select
	                    <IOptionWinFromControlFactory, Predicate<OptionItem>>
	                    (x => x.IsSupported)));

            var temp = extactor.BookOptions(o, GlobalOptions.Instance);
	        SimpleTree<OptionRenderItem> globalTree = extactor.Extract(GlobalOptions.Instance);
	        SimpleTree<OptionRenderItem> rawTree = extactor.Extract(o);
	        rawTree.Children.AddRange(globalTree.Children);
	        _options = new OptionCollectionCache(rawTree, temp, controlFactories);
	        _winformTree = GenerateWinFormTree(rawTree);
	        _winformTree.Children.Add(new BookNode(_options));


	        PopulateTree(treeView1.Nodes, _winformTree);

	        if (treeView1.SelectedNode == null) {treeView1.SelectedNode = treeView1.Nodes[treeView1.Nodes.Count-1];}

	        MaybeSpawnAndMakeVisible(treeView1.SelectedNode);

	        textBox1.TextChanged += SearchBoxChanged;

	        _searchControl = new Lazy<OptionRender>(() =>
	        {
	            OptionRender c = new OptionRender();
	            c.Factories = controlFactories;
	            Controls.Add(c);
	            SetupControl(c);

                return c;
	        });

            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
        }

	    private void SearchBoxChanged(object sender, EventArgs args)
	    {
	        if (_currentVisibleControl != null) _currentVisibleControl.Visible = false;

	        string searchfor = textBox1.Text;
	        /*if (keyPressEventArgs.KeyChar != '\b')
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
	        }*/


	        if (string.IsNullOrWhiteSpace(searchfor))
	        {
	            MaybeSpawnAndMakeVisible(treeView1.SelectedNode);
	            return;
	        }

	        List<OptionRenderItem> hits = _options.SearchList
	            .Where(x => x.SearchStrings().Any(y => CultureInfo.InvariantCulture.CompareInfo.IndexOf(y, searchfor, CompareOptions.IgnoreCase) >= 0))
	            .Take(20)
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

	    private AbstractOptionTree GenerateWinFormTree(SimpleTree<OptionRenderItem> tree)
	    {
	        SimpleOptionTree so = new SimpleOptionTree(tree.Tag.ToString(), new List<OptionRenderItem>(tree.Leafs), _options.ControlFactories);
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
                SetupControl(c);
                Controls.Add(c);
                
	        }

	      
           _currentVisibleControl = tree.ControlLazy();
           _currentVisibleControl.Visible = true;
        }

	    private void SetupControl(Control c)
	    {
	        c.Location = new Point(treeView1.Right,0);
	        c.Height = treeView1.Bottom;
	        c.Width = treeView1.Parent.Width - treeView1.Right - SystemInformation.VerticalScrollBarWidth;
	        c.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
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

        private void btnOK_Click(object sender, EventArgs e)
        {
            foreach (OptionItem item in _options.SearchList)
            {
                item.Save();
            }
            string optionPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "settings",
                Program.OptionsManager.Default.FileName);

            ClassSaver saver = new ClassSaver();

            Directory.CreateDirectory(Path.GetDirectoryName(optionPath));
            using (FileStream fs = new FileStream(optionPath, FileMode.Create))
            {
                XmlTextWriter writer = new XmlTextWriter(fs, Encoding.UTF8);
                writer.WriteStartElement("settings");
                writer.WriteAttributeString("fileversion", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                saver.Save(Program.OptionsManager.Default, writer);
                writer.WriteStartElement("books");
                foreach (var book in Program.OptionsManager.Default.Books.Where(x => x.Value).Select(x => x.Key))
                {
                    writer.WriteStartElement("book");
                    writer.WriteElementString("book", book);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.Flush();
                fs.Flush();
            }

            if (Utils.IsLinux)
            {
                string globalOptionPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".config", "Chummer5a", "globaloptions.xml");

                Directory.CreateDirectory(Path.GetDirectoryName(globalOptionPath));
                using (FileStream fs = new FileStream(globalOptionPath, FileMode.Create))
                {
                    XmlTextWriter writer = new XmlTextWriter(fs, Encoding.UTF8);
                    writer.WriteStartElement("settings");

                    saver.Save(GlobalOptions.Instance, writer);

                    writer.WriteStartElement("books");
                    foreach (SourcebookInfo book in GlobalOptions.Instance.SourcebookInfo)
                    {
                        writer.WriteStartElement("book");
                        saver.Save(book, writer);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.Flush();
                    fs.Flush();
                }
            }
            else
            {
                RegistryKey rootKey = Registry.CurrentUser.CreateSubKey("Software\\Chummer5");
                saver.Save(GlobalOptions.Instance, rootKey);
                int count = 0;
                RegistryKey bookKey = Registry.CurrentConfig.CreateSubKey("Software\\Chummer5\\Books");
                foreach (SourcebookInfo book in GlobalOptions.Instance.SourcebookInfo)
                {
                    RegistryKey k2 = bookKey.OpenSubKey(count.ToString("D2"), true);
                    saver.Save(book, k2);
                }
            }
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            foreach (OptionItem item in _options.SearchList)
            {
                item.Reload();
            }
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            //TODO: Do for more stuff (GlobalOptions won't handle this)
            CharacterOptions def = new CharacterOptions(Program.OptionsManager.Default.FileName);

            foreach (FieldInfo field in typeof(CharacterOptions).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                field.SetValue(Program.OptionsManager.Default, field.GetValue(def));
            }

            Close();
        }
    }
}
