using System;
using System.Windows.Forms;
using ChummerHub.Client.Backend;
using ChummerHub.Client.Sinners;

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
            Name = "SINnersAdvanced";
            AutoSize = true;
            cbSINnerUrl.SelectedIndex = 0;
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
            await Utils.PostSINnerAsync(MySINnersUsercontrol.MyCE);
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
            ResultSINnerPut res = await Utils.UploadChummerFileAsync(MySINnersUsercontrol.MyCE);
            if (!res.CallSuccess) 
            {
                throw new NotImplementedException(res.ErrorText);
            }
        }
    }
}
