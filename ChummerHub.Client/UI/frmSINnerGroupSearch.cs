using ChummerHub.Client.Model;
using SINners.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChummerHub.Client.UI
{
    public partial class frmSINnerGroupSearch : Form
    {
        private CharacterExtended MyCE { get; }
        public ucSINnersBasic MyParentForm { get; }

        public ucSINnerGroupSearch MySINnerGroupSearch
        {
            get { return this.siNnerGroupSearch1; }
        }
        public frmSINnerGroupSearch(CharacterExtended ce, ucSINnersBasic parentBasic)
        {
            MyCE = ce;
            MyParentForm = parentBasic;
            InitializeComponent();
            this.siNnerGroupSearch1.MyCE = ce;
            this.siNnerGroupSearch1.MyParentForm = this;
        }
    }
}
