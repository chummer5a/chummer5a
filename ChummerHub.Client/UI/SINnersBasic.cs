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
    public partial class SINnersBasic : UserControl
    {
        public SINnersUserControl myUC { get; private set; }

        public SINnersBasic()
        {
            SINnersBasicConstructor(null);
        }

        public SINnersBasic(SINnersUserControl parent)
        {
            SINnersBasicConstructor(parent);
        }

        private void SINnersBasicConstructor(SINnersUserControl parent)
        {
            InitializeComponent();
            this.AutoSize = true;
            myUC = parent;
            myUC.MyCE = parent.MyCE;
            CheckSINnerStatus();
        }

        private async void CheckSINnerStatus()
        {
            try
            {
                if (myUC.MyCE.MySINnerFile.Id == Guid.Empty)
                {
                    this.bUpload.Text = "SINless Character";
                    return;
                }
                var response = await StaticUtils.Client.GetByIdWithHttpMessagesAsync(myUC.MyCE.MySINnerFile.Id.Value);
                if (response.Response.StatusCode == HttpStatusCode.OK)
                {
                    this.bUpload.Text = "Remove from SINners";
                }
                else
                {
                    this.bUpload.Text = "Upload to SINners";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                this.bUpload.Text = "unknown Status";
            }
        }

        private void cbSRMReady_Click(object sender, EventArgs e)
        {
            
            var tagseq = from a in myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags
                         where a.TagName == "SRM_ready"
                         select a;
            if (cbSRMReady.Checked == true)
            {
                
                if (!tagseq.Any())
                {
                    Tag tag = new Tag(true);
                    tag.TagName = "SRM_ready";
                    tag.TagValue = "True";
                    tag.TagType = "bool";
                    myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.Add(tag);
                }
            }
            else
            {
                if (tagseq.Any())
                {
                    foreach(var tag in tagseq)
                    {
                        myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.Remove(tag);
                    }
                }
            }
            
        }

        private void tbGroupname_TextChanged(object sender, EventArgs e)
        {
            var tagseq = from a in myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags
                         where a.TagName == "GM_Groupname"
                         select a;
            if (!tagseq.Any())
            {
                Tag tag = new Tag(true);
                tag.TagName = "GM_Groupname";
                tag.TagType = "string";
                myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.Add(tag);
            }
            tagseq = from a in myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags
                     where a.TagName == "GM_Groupname"
                     select a;
            foreach (var tag in tagseq)
            {
                tag.TagValue = tbGroupname.Text;
            }
        }

        private async void bUpload_Click(object sender, EventArgs e)
        {
            if (bUpload.Text.Contains("Upload"))
                await Utils.PostSINnerAsync(myUC.MyCE);
            else
                myUC.RemoveSINnerAsync();
            CheckSINnerStatus();
        }

        private void tabLayoutPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        
    }
}
