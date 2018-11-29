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
using System.Net.Http;
using Microsoft.Rest;
using SINners;
using System.Net;
using SINners.Models;
using ChummerHub.Client.Backend;

namespace ChummerHub.Client.UI
{
    public partial class SINnersAdvanced : UserControl
    {
        public SINnersUserControl MySINnersUsercontrol { get; private set; }

        public SINnersAdvanced()
        {
            SINnersAdvancedConstructor(null);
        }

        public SINnersAdvanced(SINnersUserControl parent)
        {
            SINnersAdvancedConstructor(parent);
        }

        private void SINnersAdvancedConstructor(SINnersUserControl parent)
        {
            InitializeComponent();
            this.AutoSize = true;
            this.cbSINnerUrl.SelectedIndex = 0;
            MySINnersUsercontrol = parent;
        }

       

        private void cmdPopulateTags_Click(object sender, EventArgs e)
        {
            MyTagTreeView.Nodes.Clear();
            MySINnersUsercontrol.MyCharacterExtended.PopulateTags();
            TreeNode root = null;
            MySINnersUsercontrol.MyCharacterExtended.PopulateTree(ref root, null, null);
            MyTagTreeView.Nodes.Add(root);
        }

       
        private void cmdPrepareModel_Click(object sender, EventArgs e)
        {
            MySINnersUsercontrol.MyCharacterExtended.PrepareModel();
            
        }

        private void cmdPostSINnerMetaData_Click(object sender, EventArgs e)
        {
            MySINnersUsercontrol.PostSINnerAsync();
        }

        private void MyTagTreeView_VisibleChanged(object sender, EventArgs e)
        {
            MyTagTreeView.Nodes.Clear();
            TreeNode root = null;
            MySINnersUsercontrol.MyCharacterExtended.PopulateTree(ref root, null, null);
            MyTagTreeView.Nodes.Add(root);
        }

        private void cmdUploadChummerFile_Click(object sender, EventArgs e)
        {
            MySINnersUsercontrol.UploadChummerFileAsync();
        }

        private void cmdDownloadChummerFile_Click(object sender, EventArgs e)
        {
            MySINnersUsercontrol.DownloadFileAsync();
        }

       
       
    }
}
