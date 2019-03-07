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
            this.bGroupSearch.Enabled = false;
            this.AutoSize = true;
            myUC = parent;
            myUC.MyCE = parent.MyCE;
            CheckSINnerStatus().ContinueWith(a =>
            {
                if (!a.Result)
                {
                    System.Diagnostics.Trace.TraceError("somehow I couldn't check the onlinestatus of " +
                                                        myUC.MyCE.MySINnerFile.Id);
                }
            });
        }

        private async Task<bool> CheckSINnerStatus()
        {
            try
            {
                if ((myUC?.MyCE?.MySINnerFile?.Id == null) || (myUC.MyCE.MySINnerFile.Id == Guid.Empty))
                {
                    this.bUpload.Text = "SINless Character/Error";
                    return false;
                }
                var response = await StaticUtils.Client.GetSINByIdWithHttpMessagesAsync(myUC.MyCE.MySINnerFile.Id.Value);
                if (response.Response.StatusCode == HttpStatusCode.OK)
                {
                    myUC.MyCE.SetSINner(response.Body);
                    this.bUpload.Text = "Remove from SINners";
                    this.bGroupSearch.Enabled = true;
                    this.lUploadStatus.Text = "online";
                    this.bUpload.Enabled = true;
                }
                else if(response.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    this.lUploadStatus.Text = "not online";
                    this.bGroupSearch.Enabled = false;
                    this.bGroupSearch.SetToolTip("SINner needs to be uploaded first, before he/she can join a group.");
                    this.bUpload.Enabled = true;
                    this.bUpload.Text = "Upload";
                }
                else
                {
                    this.lUploadStatus.Text = "Statuscode: " + response.Response.StatusCode;
                    this.bGroupSearch.Enabled = false;
                    this.bGroupSearch.SetToolTip("SINner needs to be uploaded first, before he/she can join a group.");
                    this.bUpload.Text = "Upload";
                    this.bUpload.Enabled = true;
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

                if(myUC?.MyCE?.MySINnerFile?.MyGroup != null)
                    this.lGourpForSinner.Text = myUC.MyCE.MySINnerFile.MyGroup.Groupname;


            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                this.bUpload.Text = "unknown Status";
                return false;
            }

            return true;
        }

        private void cbSRMReady_Click(object sender, EventArgs e)
        {
            
            var tagseq = (from a in myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags
                         where a.TagName == "SRM_ready"
                         select a).ToList();
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
            {
                await myUC.MyCE.Upload();
            }

            else
                await myUC.RemoveSINnerAsync();
            CheckSINnerStatus();
        }

        private void bGroupSearch_Click(object sender, EventArgs e)
        {
            frmSINnerGroupSearch gs = new frmSINnerGroupSearch(myUC.MyCE);
            var res = gs.ShowDialog();
            
        }

        private void cbTagArchetype_Click(object sender, EventArgs e)
        {
            var tagseq = (from a in myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags
                         where a.TagName == "Archetype"
                         select a).ToList();
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
