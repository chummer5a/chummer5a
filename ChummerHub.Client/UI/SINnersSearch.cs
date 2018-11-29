using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using ChummerHub.Client.Model;
using SINners.Models;
using ChummerHub.Client.Backend;
using System.Xml;
using System.Collections;
using GroupControls;
using System.Reflection;

namespace ChummerHub.Client.UI
{
    public partial class SINnersSearch : UserControl
    {
        public static CharacterExtended MySearchCharacter = null;

        public SearchTag motherTag = null;
        private Action<string> GetSelectedObjectCallback;

        public string SelectedId { get; private set; }

        public SINnersSearch()
        {
            MySearchCharacter = new CharacterExtended(new Character(), null);
            InitializeComponent();
        }

        private void SINnersSearchSearch_Load(object sender, EventArgs e)
        {
            UpdateDialog();
           
            
            
        }

        private bool loading = false;

        private Control GetCbOrOInputontrolFromMembers(SearchTag stag)
        {
            loading = true;
            try
            {


                //input can be here any time, regardless of childs!
                var input = GetUserInputControl(stag);
                if (input != null)
                {
                    flpReflectionMembers.Controls.Add(input);
                    return input;
                }
                var list = SearchTagExtractor.ExtractTagsFromAttributes(stag.MyRuntimePropertyValue, stag);
                if (list.Any())
                {
                    var ordered = (from a in list orderby a.STagName select a).ToList();
                    ComboBox cb = new ComboBox
                    {
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        FlatStyle = FlatStyle.Standard
                    };
                    flpReflectionMembers.Controls.Add(cb);
                    cb.DataSource = ordered;
                    cb.DisplayMember = "STagName";
                    cb.SelectedValueChanged += (sender, e) =>
                    {
                        var tag = cb.SelectedItem as SearchTag;
                        var childcb = GetCbOrOInputontrolFromMembers(tag);
                    };
                    return cb;
                }
                return null;
            }
            finally
            {
                this.loading = false;
            }
        }

        public List<SearchTag> MySetTags = new List<SearchTag>();

        private Control GetUserInputControl(SearchTag stag)
        {
            string switchname = stag.STagName;
            string typename = stag.MyRuntimePropertyValue.GetType().ToString();
            FlowLayoutPanel flp = new FlowLayoutPanel(); ;
            TextBox tb = null;
            Button b = null;
            NumericUpDown nud = null;
            ComboBox cb = null;
            switch (typename)
            {
                case "System.Boolean":
                    {
                        RadioButtonList rdb = new RadioButtonList();
                        RadioButtonListItem itrue = new RadioButtonListItem() { Text = "true" };
                        RadioButtonListItem ifalse = new RadioButtonListItem() { Text = "false" };
                        rdb.Text = stag.STagName;
                        rdb.Items.Add(itrue);
                        rdb.Items.Add(ifalse);
                        rdb.SelectedIndexChanged += (sender, e) =>
                        {
                            PropertyInfo info = stag.MyPropertyInfo as PropertyInfo;
                            info.SetValue(stag.MyParentTag.MyRuntimePropertyValue, itrue.Checked);
                            MySetTags.Add(stag);
                            UpdateDialog();
                        };
                        return rdb;
                        break;
                    }
                case "System.String":
                    {
                        tb = new TextBox();
                        flp.Controls.Add(tb);
                        b = new Button() { Text = "OK" };
                        b.Click += (sender, e) =>
                        {
                            PropertyInfo info = stag.MyPropertyInfo as PropertyInfo;
                            info.SetValue(stag.MyParentTag.MyRuntimePropertyValue, tb.Text);
                            MySetTags.Add(stag);
                            UpdateDialog();
                        };
                        flp.Controls.Add(b);
                        return flp;
                        break;
                    }
                case "System.Int32":
                    {
                        nud = new NumericUpDown() { Minimum = int.MinValue, Maximum = int.MaxValue };

                        flp.Controls.Add(nud);
                        b = new Button() { Text = "OK" };
                        b.Click += (sender, e) =>
                        {
                            PropertyInfo info = stag.MyPropertyInfo as PropertyInfo;
                            info.SetValue(stag.MyParentTag.MyRuntimePropertyValue, (int)nud.Value);
                            MySetTags.Add(stag);
                            UpdateDialog();
                        };
                        flp.Controls.Add(b);
                        return flp;
                        break;
                    }
                case "Chummer.Backend.Uniques.Tradition":
                    {
                        var traditions = Chummer.Backend.Uniques.Tradition.GetTraditions(SINnersSearch.MySearchCharacter.MyCharacter);
                        cb = new ComboBox
                        {
                            DataSource = traditions,
                            DropDownStyle = ComboBoxStyle.DropDownList,
                            FlatStyle = FlatStyle.Standard,
                            DisplayMember = "Name"
                        };
                        cb.SelectedValueChanged += (sender, e) =>
                        {
                            if (loading)
                                return;
                            PropertyInfo info = stag.MyPropertyInfo as PropertyInfo;
                            info.SetValue(stag.MyParentTag.MyRuntimePropertyValue, cb.SelectedValue);
                            stag.STagValue = (cb.SelectedValue as Chummer.Backend.Uniques.Tradition).Name;
                            MySetTags.Add(stag);
                            UpdateDialog();
                        };
                        flp.Controls.Add(cb);
                        return flp;
                        break;
                    }
                default:
                    break;
            }
            Object obj = stag.MyRuntimePropertyValue;
            if (!typeof(String).IsAssignableFrom(obj.GetType()))
            {
                IEnumerable islist = obj as IEnumerable;
                if (islist == null)
                {
                    islist = obj as ICollection;
                }
                if (islist != null)
                {
                    Type listtype = StaticUtils.GetListType(islist);
                    if (listtype != null)
                        switchname = listtype.Name;
                }
            }
            
            switch (switchname)
            {
                case "Spell":
                    {
                        Button button = new Button();
                        button.Text = "select Spell";
                        button.Click += ((sender, e) =>
                        {
                            var frmPickSpell = new frmSelectSpell(MySearchCharacter.MyCharacter);
                            frmPickSpell.ShowDialog();
                        // Open the Spells XML file and locate the selected piece.
                        XmlDocument objXmlDocument = XmlManager.Load("spells.xml");
                            XmlNode objXmlSpell = objXmlDocument.SelectSingleNode("/chummer/spells/spell[id = \"" + frmPickSpell.SelectedSpell + "\"]");
                            Spell objSpell = new Spell(MySearchCharacter.MyCharacter);
                            objSpell.Create(objXmlSpell, string.Empty, frmPickSpell.Limited, frmPickSpell.Extended, frmPickSpell.Alchemical);
                            MySearchCharacter.MyCharacter.Spells.Add(objSpell);
                            SearchTag spellsearch = new SearchTag(stag.MyPropertyInfo, stag.MyRuntimeHubClassTag);
                            spellsearch.MyRuntimePropertyValue = objSpell;
                            spellsearch.MyParentTag = stag;
                            spellsearch.STagName = objSpell.Name;
                            spellsearch.STagValue = "";
                            spellsearch.SSearchOpterator = "exists";
                            MySetTags.Add(spellsearch);
                            UpdateDialog();
                        });
                        return button;
                        break;
                    }
                default:
                    break;
            }
            return null;
            
        }

        private void UpdateDialog()
        {
            flpReflectionMembers.Controls.Clear();
            MySearchCharacter.MySINnerFile.SiNnerMetaData.Tags.Clear();
            MySearchCharacter.PopulateTags();
            MyTagTreeView.Nodes.Clear();
            TreeNode root = null;
            MySearchCharacter.PopulateTree(ref root, null, MySetTags);
            MyTagTreeView.Nodes.Add(root);
            motherTag = new SearchTag()
            {
                STags = new List<SearchTag>(),
                MyPropertyInfo = null,
                MyRuntimePropertyValue = MySearchCharacter.MyCharacter,
                STagName = "Root",
                STagValue = "Search"
            };
            Control cbChar = GetCbOrOInputontrolFromMembers(motherTag);
        }

     
    }
}
