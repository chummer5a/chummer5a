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

namespace MatrixPlugin
{
    public partial class ucOptions : UserControl
    {
        public ucOptions()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string msg = "Since the plugininterface functions may be called from threads other than the UI-Thread, please use the\r\n\r\n";
            msg += "Program.MainForm.ShowMessageBox()-function to show messages.";
            Program.MainForm.ShowMessageBox(msg, "take note!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
