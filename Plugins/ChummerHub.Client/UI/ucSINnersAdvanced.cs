/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
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


        private async void cmdPrepareModel_Click(object sender, EventArgs e)
        {
            await MySINnersUsercontrol.MyCE.PrepareModelAsync();
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
