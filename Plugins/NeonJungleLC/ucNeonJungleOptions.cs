using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace NeonJungleLC
{
    public partial class ucNeonJungleOptions : UserControl
    {
        public ucNeonJungleOptions()
        {
            InitializeComponent();

            LoadSettings();
        }

        private void LoadSettings()
        {
            this.tbUserName.Text = Properties.Settings.Default.NeonJungleLCUserName;
        }

        private void tbUserName_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.NeonJungleLCUserName = this.tbUserName.Text;
            Properties.Settings.Default.Save();
        }
    }
}
