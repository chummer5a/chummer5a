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
            motherTag = new SearchTag();
            ComboBox cbChar = GetCbFromMembers(dummyCharacter);
            
            flpReflectionMembers.Controls.Add(cbChar);
        }

        private ComboBox GetCbFromMembers(Object data)
        {
            ComboBox cb = new ComboBox();
            cb.SelectedValueChanged += Cb_SelectedValueChanged;
            
            return cb;
        }

        private void Cb_SelectedValueChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
