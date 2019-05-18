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
using Chummer.Plugins;

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
            this.VisibleChanged += (sender, args) =>
            {
                if (this.Visible == true)
                    ReallyCenterToScreen();
            };

        }
        protected void ReallyCenterToScreen()
        {
            Screen screen = Screen.FromControl(PluginHandler.MainForm);

            Rectangle workingArea = screen.WorkingArea;
            this.Location = new Point()
            {
                X = Math.Max(workingArea.X, workingArea.X + (workingArea.Width - this.Width) / 2),
                Y = Math.Max(workingArea.Y, workingArea.Y + (workingArea.Height - this.Height) / 2)
            };
        }
    }
}
