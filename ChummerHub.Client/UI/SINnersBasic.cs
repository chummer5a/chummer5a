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
                if ((myUC?.MyCE?.MySINnerFile?.Id == null) || (myUC.MyCE.MySINnerFile.Id == Guid.Empty))
                {
                    this.bUpload.Text = "SINless Character/Error";
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
                this.cbTagArchetype.Enabled = false;
                this.tbArchetypeName.Enabled = false;
                var resroles = await StaticUtils.Client.GetRolesWithHttpMessagesAsync();
                if(response.Response.StatusCode == HttpStatusCode.OK)
                {
                    var archetypeseq = from a in resroles.Body where a.ToLowerInvariant() == "ArchetypeAdmin".ToLowerInvariant() select a;
                    if (archetypeseq.Any())
                    {
                        this.cbTagArchetype.Enabled = true;
                        this.tbArchetypeName.Enabled = true;
                    }
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
                    tag.SiNnerId = myUC.MyCE.MySINnerFile.Id;
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

        

        private async void bUpload_Click(object sender, EventArgs e)
        {
            if (bUpload.Text.Contains("Upload"))
                await ChummerHub.Client.Backend.Utils.PostSINnerAsync(myUC.MyCE);
            else
                await myUC.RemoveSINnerAsync();
            CheckSINnerStatus();
        }

        private void bGroupSearch_Click(object sender, EventArgs e)
        {
            frmSINnerGroupSearch gs = new frmSINnerGroupSearch(myUC.MyCE.MySINnerFile);
            var res = gs.ShowDialog();
            
        }

        private void cbTagArchetype_Click(object sender, EventArgs e)
        {
            var tagseq = from a in myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags
                         where a.TagName == "Archetype"
                         select a;
            if(cbSRMReady.Checked == true)
            {
                if(!tagseq.Any())
                {
                    Tag tag = new Tag(true);
                    tag.SiNnerId = myUC.MyCE.MySINnerFile.Id;
                    tag.TagName = "Archetype";
                    tag.TagValue = tbArchetypeName.Text;
                    tag.TagType = "string";
                    myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.Add(tag);
                }
            }
            else
            {
                if(tagseq.Any())
                {
                    foreach(var tag in tagseq)
                    {
                        myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.Remove(tag);
                    }
                }
            }
        }
    }
}
