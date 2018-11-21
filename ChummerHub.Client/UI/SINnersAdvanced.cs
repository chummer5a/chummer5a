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
            PopulateTree(ref root, MySINnersUsercontrol.MyCharacterExtended.MySINnerFile.SiNnerMetaData.Tags);
            MyTagTreeView.Nodes.Add(root);
        }

        public void PopulateTree(ref TreeNode root, IList<Tag> tags)
        {
            if (root == null)
            {
                root = new TreeNode();
                root.Text = "Tags";
                root.Tag = null;
                // get all tags in the list with parent is null
                var onebranch = tags.Where(t => t.MyParentTag == null);
                foreach (var branch in onebranch)
                {
                    var child = new TreeNode()
                    {
                        Text = branch.TagName,
                        Tag = branch.TagId,
                    };
                    PopulateTree(ref child, branch.Tags);
                    root.Nodes.Add(child);
                }
            }
            else
            {
                foreach (var tag in tags)
                {
                    var child = new TreeNode()
                    {
                        Text = tag.TagName,
                        Tag = tag.TagId,
                    };
                    if (!String.IsNullOrEmpty(tag.TagValue))
                        child.Text += ": " + tag.TagValue;
                    PopulateTree(ref child, tag.Tags);
                    root.Nodes.Add(child);
                }
            }
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
            PopulateTree(ref root, MySINnersUsercontrol.MyCharacterExtended.MySINnerFile.SiNnerMetaData.Tags);
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
