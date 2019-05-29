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
    public partial class ucSINnersAdvanced : UserControl
    {
        public ucSINnersUserControl MySINnersUsercontrol { get; private set; }

        public ucSINnersAdvanced()
        {
            SINnersAdvancedConstructor(null);
        }

        public ucSINnersAdvanced(ucSINnersUserControl parent)
        {
            SINnersAdvancedConstructor(parent);
        }

        private void SINnersAdvancedConstructor(ucSINnersUserControl parent)
        {
            InitializeComponent();
            this.Name = "SINnersAdvanced";
            this.AutoSize = true;
            this.cbSINnerUrl.SelectedIndex = 0;
            MySINnersUsercontrol = parent;
            
            //TreeNode root = null;
            //MySINnersUsercontrol.MyCE.PopulateTree(ref root, null, null);
            //MyTagTreeView.Nodes.Add(root);
        }

       

        private void cmdPopulateTags_Click(object sender, EventArgs e)
        {
            PopulateTags();
        }

        private void PopulateTags()
        {
            MyTagTreeView.Nodes.Clear();
            MySINnersUsercontrol.MyCE.MySINnerFile.SiNnerMetaData.Tags = MySINnersUsercontrol.MyCE.PopulateTags();
            TreeNode root = null;
            MySINnersUsercontrol.MyCE.PopulateTree(ref root, null, null);
            MyTagTreeView.Nodes.Add(root);
        }


        private void cmdPrepareModel_Click(object sender, EventArgs e)
        {
            MySINnersUsercontrol.MyCE.PrepareModel();
            
        }

        private async void cmdPostSINnerMetaData_Click(object sender, EventArgs e)
        {
            await ChummerHub.Client.Backend.Utils.PostSINnerAsync(MySINnersUsercontrol.MyCE);
        }

        private void MyTagTreeView_VisibleChanged(object sender, EventArgs e)
        {
            MyTagTreeView.Nodes.Clear();
            TreeNode root = null;
            //if (MySINnersUsercontrol.MyCE?.MySINnerFile?.SiNnerMetaData?.Tags?.Any(a => a.TagName == "Reflection") == false)
            PopulateTags();
            //MySINnersUsercontrol.MyCE.PopulateTree(ref root, null, null);
            //MyTagTreeView.Nodes.Add(root);
        }

        private async void cmdUploadChummerFile_Click(object sender, EventArgs e)
        {
            await ChummerHub.Client.Backend.Utils.UploadChummerFileAsync(MySINnersUsercontrol.MyCE);
        }

       

       
       
    }
}
