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

namespace ChummerHub.Client.UI
{
    public partial class SINnersSearch : UserControl
    {
        public static Character dummyCharacter = null;

        public SearchTag motherTag = null;

        public SINnersSearch()
        {
            dummyCharacter = new Character();
            InitializeComponent();
        }

        private void SINnersSearchSearch_Load(object sender, EventArgs e)
        {
            motherTag = new SearchTag()
            {
                STags = new List<SearchTag>(),
                MyRuntimeObject = dummyCharacter,
                STagName = "Root",
                STagValue = "Search"
            };
            ComboBox cbChar = GetCbFromMembers(motherTag);
            
            flpReflectionMembers.Controls.Add(cbChar);
        }

        private ComboBox GetCbFromMembers(SearchTag stag)
        {
            ComboBox cb = new ComboBox();
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.FlatStyle = FlatStyle.Standard;
            cb.SelectedValueChanged += Cb_SelectedValueChanged;
            
            var list = SearchTagExtractor.ExtractTagsFromAttributes(stag.MyRuntimeObject, stag);
            cb.DataSource = list;
            cb.DisplayMember = "STagName";
            return cb;
        }

        private void Cb_SelectedValueChanged(object sender, EventArgs e)
        {
            return;
        }
    }
}
