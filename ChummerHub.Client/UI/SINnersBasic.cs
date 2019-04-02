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
using Chummer.Plugins;
using Utils = Chummer.Utils;

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
            this.Name = "SINnersBasic";
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

        public async Task<bool> CheckSINnerStatus()
        {
        
            try
            {
                if ((myUC?.MyCE?.MySINnerFile?.Id == null) || (myUC.MyCE.MySINnerFile.Id == Guid.Empty))
                {
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {

                        this.bUpload.Text = "SINless Character/Error";

                    });
                    return false;
                }

                using (new CursorWait(true, this))
                {
                    var client = await StaticUtils.GetClient();
                    var response = await client.GetSINByIdWithHttpMessagesAsync(myUC.MyCE.MySINnerFile.Id.Value);
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                        if (response.Response.StatusCode == HttpStatusCode.OK)
                        {
                            myUC.MyCE.SetSINner(response.Body);
                            this.bUpload.Text = "Remove from SINners";
                            this.bGroupSearch.Enabled = true;
                            this.lUploadStatus.Text = "online";
                            this.bUpload.Enabled = true;
                        }
                        else if (response.Response.StatusCode == HttpStatusCode.NotFound)
                        {
                            this.lUploadStatus.Text = "not online";
                            this.bGroupSearch.Enabled = false;
                            this.bGroupSearch.SetToolTip(
                                "SINner needs to be uploaded first, before he/she can join a group.");
                            this.bUpload.Enabled = true;
                            this.bUpload.Text = "Upload";
                        }
                        else
                        {
                            this.lUploadStatus.Text = "Statuscode: " + response.Response.StatusCode;
                            this.bGroupSearch.Enabled = false;
                            this.bGroupSearch.SetToolTip(
                                "SINner needs to be uploaded first, before he/she can join a group.");
                            this.bUpload.Text = "Upload";
                            this.bUpload.Enabled = true;
                        }

                        this.cbTagArchetype.Enabled = false;
                        this.tbArchetypeName.Enabled = false;
                    });
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                        var archetypeseq = from a in StaticUtils.UserRoles
                            where a.ToLowerInvariant() == "ArchetypeAdmin".ToLowerInvariant()
                            select a;
                        if (archetypeseq.Any())
                        {
                            this.cbTagArchetype.Enabled = true;
                            this.tbArchetypeName.Enabled = true;
                        }
                        if (myUC?.MyCE?.MySINnerFile?.MyGroup != null)
                            this.lGourpForSinner.Text = myUC.MyCE.MySINnerFile.MyGroup.Groupname;
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                PluginHandler.MainForm.DoThreadSafe(() =>
                {
                    this.bUpload.Text = "unknown Status";
                });
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
            using (new CursorWait(true, this))
            {
                try
                {
                    if (bUpload.Text.Contains("Upload"))
                    {
                        this.lUploadStatus.Text = "uploading";
                        await myUC.MyCE.Upload();
                    }
                    else
                    {
                        this.lUploadStatus.Text = "removing";
                        await myUC.RemoveSINnerAsync();
                    }

                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
            await CheckSINnerStatus();


        }

        private void bGroupSearch_Click(object sender, EventArgs e)
        {
            frmSINnerGroupSearch gs = new frmSINnerGroupSearch(myUC.MyCE, this);
            gs.MySINnerGroupSearch.OnGroupJoinCallback += (o, group) =>
            {
                PluginHandler.MainForm.DoThreadSafe(() =>
                {
                    PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false, true);
                });
            };
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
