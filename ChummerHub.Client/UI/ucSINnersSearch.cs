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
using Chummer.Backend.Equipment;
using NLog;

namespace ChummerHub.Client.UI
{
    public partial class ucSINnersSearch : UserControl
    {
        public static CharacterExtended MySearchCharacter = null;
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public SearchTag motherTag = null;
        private Action<string> GetSelectedObjectCallback;

        public string SelectedId { get; private set; }

        public ucSINnersSearch()
        {
            MySearchCharacter = new CharacterExtended(new Character(), null, null, new frmCharacterRoster.CharacterCache());
            InitializeComponent();
        }

        private void SINnersSearchSearch_Load(object sender, EventArgs e)
        {
            UpdateDialog();
        }

        private bool _loading = false;

        private Control GetCbOrOInputontrolFromMembers(SearchTag stag)
        {
            _loading = true;
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
                    var ordered = (from a in list orderby a.TagName select a).ToList();
                    ComboBox cb = new ComboBox
                    {
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        FlatStyle = FlatStyle.Standard
                    };
                    flpReflectionMembers.Controls.Add(cb);
                    cb.DataSource = ordered;
                    cb.DisplayMember = "TagName";
                    cb.SelectedValueChanged += (sender, e) =>
                    {
                        var tag = cb.SelectedItem as SearchTag;
                        var childcb = GetCbOrOInputontrolFromMembers(tag);
                    };
                    return cb;
                }

                return null;
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }
            finally
            {
                _loading = false;
            }

            return null;
        }

        public List<SearchTag> MySetTags = new List<SearchTag>();

        private Control GetUserInputControl(SearchTag stag)
        {
            string switchname = stag.TagName;
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
                        RadioButtonListItem itrue = new RadioButtonListItem
                        {
                            Text = "true"
                        };
                        RadioButtonListItem ifalse = new RadioButtonListItem
                        {
                            Text = "false"
                        };
                        RadioButtonList rdb = new RadioButtonList
                        {
                            Text = stag.TagName
                        };
                        rdb.Items.Add(itrue);
                        rdb.Items.Add(ifalse);
                        rdb.SelectedIndexChanged += (sender, e) =>
                        {
                            PropertyInfo info = stag.MyPropertyInfo;
                            info.SetValue(((SearchTag)stag.MyParentTag).MyRuntimePropertyValue, itrue.Checked);
                            MySetTags.Add(stag);
                            UpdateDialog();
                        };
                        return rdb;
                    }
                case "System.String":
                    {
                        tb = new TextBox();
                        flp.Controls.Add(tb);
                        b = new Button
                        {
                            Text = "OK"
                        };
                        b.Click += (sender, e) =>
                        {
                            PropertyInfo info = stag.MyPropertyInfo as PropertyInfo;
                            info.SetValue(((SearchTag)stag.MyParentTag).MyRuntimePropertyValue, tb.Text);
                            MySetTags.Add(stag);
                            UpdateDialog();
                        };
                        flp.Controls.Add(b);
                        return flp;
                    }
                case "System.Int32":
                    {
                        nud = new NumericUpDown
                        {
                            Minimum = int.MinValue,
                            Maximum = int.MaxValue
                        };
                        flp.Controls.Add(nud);
                        b = new Button
                        {
                            Text = "OK"
                        };
                        b.Click += (sender, e) =>
                        {
                            PropertyInfo info = stag.MyPropertyInfo;
                            info.SetValue(((SearchTag)stag.MyParentTag).MyRuntimePropertyValue, (int)nud.Value);
                            MySetTags.Add(stag);
                            UpdateDialog();
                        };
                        flp.Controls.Add(b);
                        return flp;
                    }
                case "Chummer.Backend.Uniques.Tradition":
                    {
                        var traditions = Chummer.Backend.Uniques.Tradition.GetTraditions(ucSINnersSearch.MySearchCharacter.MyCharacter);
                        cb = new ComboBox
                        {
                            DataSource = traditions,
                            DropDownStyle = ComboBoxStyle.DropDownList,
                            FlatStyle = FlatStyle.Standard,
                            DisplayMember = "Name"
                        };
                        cb.SelectedValueChanged += (sender, e) =>
                        {
                            if (_loading)
                                return;
                            PropertyInfo info = stag.MyPropertyInfo;
                            info.SetValue(((SearchTag)stag.MyParentTag).MyRuntimePropertyValue, cb.SelectedValue);
                            stag.TagValue = (cb.SelectedValue as Chummer.Backend.Uniques.Tradition)?.Name ?? string.Empty;
                            MySetTags.Add(stag);
                            UpdateDialog();
                        };
                        flp.Controls.Add(cb);
                        return flp;
                    }
            }
            object obj = stag.MyRuntimePropertyValue;
            if (!(obj is string))
            {
                if (obj is IList)
                {
                    Type listtype = StaticUtils.GetListType(obj);
                    if (listtype != null)
                        switchname = listtype.Name;
                }
            }

            switch (switchname)
            {
                ///these are sample implementations to get added one by one...
                case "Spell":
                    {
                        Button button = new Button
                        {
                            Text = "select Spell"
                        };
                        button.Click += (sender, e) =>
                        {
                            var frmPickSpell = new frmSelectSpell(MySearchCharacter.MyCharacter);
                            frmPickSpell.ShowDialog();
                            // Open the Spells XML file and locate the selected piece.
                            XmlDocument objXmlDocument = XmlManager.Load("spells.xml");
                            XmlNode objXmlSpell = objXmlDocument.SelectSingleNode("/chummer/spells/spell[id = \"" + frmPickSpell.SelectedSpell + "\"]");
                            Spell objSpell = new Spell(MySearchCharacter.MyCharacter);
                            if(string.IsNullOrEmpty(objSpell.Name))
                                return;
                            objSpell.Create(objXmlSpell, string.Empty, frmPickSpell.Limited, frmPickSpell.Extended, frmPickSpell.Alchemical);
                            MySearchCharacter.MyCharacter.Spells.Add(objSpell);
                            SearchTag spellsearch = new SearchTag(stag.MyPropertyInfo, stag.MyRuntimeHubClassTag)
                            {
                                MyRuntimePropertyValue = objSpell,
                                MyParentTag = stag,
                                TagName = objSpell.Name,
                                TagValue = string.Empty,
                                SearchOperator = "exists"
                            };
                            MySetTags.Add(spellsearch);
                            UpdateDialog();
                        };
                        return button;
                    }
                case "Quality":
                    {
                        Button button = new Button
                        {
                            Text = "select Quality"
                        };
                        button.Click += ((sender, e) =>
                        {
                            var frmPick = new frmSelectQuality(MySearchCharacter.MyCharacter);
                            frmPick.ShowDialog();
                            // Open the Spells XML file and locate the selected piece.
                            XmlDocument objXmlDocument = XmlManager.Load("qualities.xml");
                            XmlNode objXmlNode = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + frmPick.SelectedQuality + "\"]");
                            Quality objQuality = new Quality(MySearchCharacter.MyCharacter);
                            List<Weapon> lstWeapons = new List<Weapon>();
                            objQuality.Create(objXmlNode, QualitySource.Selected, lstWeapons);
                            MySearchCharacter.MyCharacter.Qualities.Add(objQuality);
                            SearchTag newtag = new SearchTag(stag.MyPropertyInfo, stag.MyRuntimeHubClassTag)
                            {
                                MyRuntimePropertyValue = objQuality,
                                MyParentTag = stag,
                                TagName = objQuality.Name,
                                TagValue = string.Empty,
                                SearchOperator = "exists"
                            };
                            MySetTags.Add(newtag);
                            UpdateDialog();
                        });
                        return button;
                    }
            }
            return null;
        }

        private void UpdateDialog()
        {
            flpReflectionMembers.Controls.Clear();
            MySearchCharacter.MySINnerFile.SiNnerMetaData.Tags.Clear();
            MyTagTreeView.Nodes.Clear();
            TreeNode root = null;
            MySearchCharacter.PopulateTree(ref root, null, MySetTags);
            MyTagTreeView.Nodes.Add(root);
            motherTag = new SearchTag
            {
                Tags = new List<Tag>(),
                MyPropertyInfo = null,
                MyRuntimePropertyValue = MySearchCharacter.MyCharacter,
                TagName = "Root",
                TagValue = "Search"
            };
            Control cbChar = GetCbOrOInputontrolFromMembers(motherTag);
        }
    }
}
