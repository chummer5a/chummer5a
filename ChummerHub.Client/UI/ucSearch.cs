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
    public partial class ucSearch : UserControl
    {
        public static Character dummyCharacter = null;

        public SearchTag motherTag = null;

        public ucSearch()
        {
            dummyCharacter = new Character();
            InitializeComponent();
        }

        private void ucSearch_Load(object sender, EventArgs e)
        {
            motherTag = new SearchTag();
        }
    }
}
