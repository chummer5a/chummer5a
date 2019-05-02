using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
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
    public partial class ucSINnersBasic : UserControl
    {
        public ucSINnersUserControl myUC { get; private set; }

        public ucSINnersBasic()
        {
            SINnersBasicConstructor(null);
        }

        public ucSINnersBasic(ucSINnersUserControl parent)
        {
            SINnersBasicConstructor(parent);
        }

        private void SINnersBasicConstructor(ucSINnersUserControl parent)
        {
            InitializeComponent();
           

            this.TagValueArchetype.DataSource = ContactControl.ContactArchetypes;
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
            foreach (var cb in gpTags.Controls)
            {
                if ((cb is Control cont))
                    cont.Click += OnGroupBoxTagsClick;
            }
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
                    var client = StaticUtils.GetClient();
                    var response = await client.GetSINnerGroupFromSINerByIdWithHttpMessagesAsync(myUC.MyCE.MySINnerFile.Id.Value);
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                        if (response.Response.StatusCode == HttpStatusCode.OK)
                        {
                            myUC.MyCE.MySINnerFile.MyGroup = response.Body.MySINnerGroup;
                            this.bUpload.Text = "Remove from SINners";
                            this.bGroupSearch.Enabled = true;
                            this.lUploadStatus.Text = "online";
                            this.bUpload.Enabled = true;
                        }
                        else if (response.Response.StatusCode == HttpStatusCode.NotFound)
                        {
                            myUC.MyCE.MySINnerFile.MyGroup = null;
                            this.lUploadStatus.Text = "not online";
                            this.bGroupSearch.Enabled = false;
                            this.bGroupSearch.SetToolTip(
                                "SINner needs to be uploaded first, before he/she can join a group.");
                            this.bUpload.Enabled = true;
                            this.bUpload.Text = "Upload";
                        }
                        else if (response.Response.StatusCode == HttpStatusCode.NoContent)
                        {
                            myUC.MyCE.MySINnerFile.MyGroup = null;
                            this.lUploadStatus.Text = "Statuscode: " + response.Response.StatusCode;
                            this.bGroupSearch.Enabled = true;
                            this.bGroupSearch.SetToolTip(
                                "SINner does not belong to a group.");
                            this.bUpload.Text = "Remove from SINners";
                            this.lUploadStatus.Text = "online";
                            this.bUpload.Enabled = true;
                        }
                        this.cbTagCustom.Enabled = false;
                        this.TagValueCustomName.Enabled = false;
                    });
                    PluginHandler.MainForm.DoThreadSafe(() => { UpdateTags(); });
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

        private void UpdateTags()
        {
            var archetypeseq = from a in StaticUtils.UserRoles
                where a.ToLowerInvariant() == "ArchetypeAdmin".ToLowerInvariant()
                select a;
            if (archetypeseq.Any())
            {
                this.cbTagCustom.Enabled = true;
                this.TagValueCustomName.Enabled = true;
            }
            if (myUC?.MyCE?.MySINnerFile?.MyGroup != null)
                this.lGourpForSinner.Text = myUC.MyCE.MySINnerFile.MyGroup.Groupname;

            var gpControlSeq = (from a in GetAllControls(gpTags, new List<Control>())
                where a.Name.Contains("cbTag")
                select a).ToList();

            var gpControlValueSeq = (from a in GetAllControls(gpTags, new List<Control>())
                where a.Name.Contains("TagValue")
                select a).ToList();

            foreach (var cb in gpControlSeq)
            {
                if (cb is CheckBox cbTag)
                    cbTag.CheckState = CheckState.Unchecked;
            }

            foreach (var tag in myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.ToList())
            {
                //search for a CheckBox that is named like the tag
                string checkBoxKey = "cbTag" + tag.TagName;
                var tagsCB = (from a in gpControlSeq where a.Name == checkBoxKey select a).ToList();
                if (!tagsCB.Any())
                    continue;
                if (!(tagsCB.FirstOrDefault() is CheckBox cbTag))
                    throw new ArgumentNullException("Control " + checkBoxKey + " is NOT a Checkbox!");
                
                if (Boolean.TryParse(tag.TagValue, out var value))
                {
                    cbTag.Checked = value;
                }
                else
                {
                    cbTag.Checked = false;
                }
                //search for the value-control (whatever that may be)
                var tagValueControlKey = "TagValue" + tag.TagName;
                var tagValueControlSeq = (from a in gpControlValueSeq where a.Name == tagValueControlKey select a).ToList();
                if (!tagValueControlSeq.Any())
                {
                    continue;
                }

                cbTag.Checked = true;
                if (tagValueControlSeq.FirstOrDefault() is TextBox tbTagValue)
                {
                    tbTagValue.Text = tag.TagValue;
                }
                else if (tagValueControlSeq.FirstOrDefault() is ComboBox comboTagValue)
                {
                    if (!comboTagValue.Items.Contains(tag.TagValue))
                        comboTagValue.Items.Add(tag.TagValue);
                    comboTagValue.SelectedItem = tag.TagValue;
                }
                else if (tagValueControlSeq.FirstOrDefault() is NumericUpDown upDownTagValue)
                {
                    if (Decimal.TryParse(tag.TagValue, out var val))
                        upDownTagValue.Value = val;
                }
            }
        }

        private void OnGroupBoxTagsClick(object sender, EventArgs e)
        {
            // Call the base class
            base.OnClick(e);
            SaveTagsToSinner();
        }

        private List<Control> GetAllControls(Control container, List<Control> list)
        {
            foreach (Control c in container.Controls)
            {
                list.Add(c);
                if (c.Controls.Count > 0)
                    list = GetAllControls(c, list);
            }

            return list;
        }

        private void SaveTagsToSinner()
        {
            if (myUC == null)
                return;
            var gpControlSeq = (from a in GetAllControls(gpTags, new List<Control>())
                where a.Name.Contains("cbTag") select a).ToList();

            var gpControlValueSeq = (from a in GetAllControls(gpTags, new List<Control>())
                where a.Name.Contains("TagValue")
                select a).ToList();

            foreach (var cb in gpControlSeq)
            {
                if (!(cb is CheckBox cbTag))
                    continue;
                
                string tagName = cbTag.Name.Substring("cbTag".Length);
                Tag tag = null;
                if (myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.All(a => a.TagName != tagName))
                {
                    tag = new Tag(true)
                    {
                        SiNnerId = myUC.MyCE.MySINnerFile.Id,
                        TagName = tagName
                    };
                    
                }
                else
                {
                    tag = myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.FirstOrDefault(a => a.TagName == tagName);
                }
                if (tag == null) continue;
                if (myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.Contains(tag))
                    myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.Remove(tag);
                if (cbTag.Checked == false)
                    continue;
                else if (cbTag.CheckState == CheckState.Checked)
                {
                    tag.TagValue = (true).ToString();
                }
                myUC.MyCE.MySINnerFile.SiNnerMetaData.Tags.Add(tag);
                //search for the value
                var tagValueControlKey = "TagValue" + tag.TagName;
                var tagValueControlSeq =
                    (from a in gpControlValueSeq where a.Name == tagValueControlKey select a).ToList();
                if (!tagValueControlSeq.Any())
                    continue;
                if (tagValueControlSeq.FirstOrDefault() is TextBox tbTagValue)
                {
                    tag.TagValue = tbTagValue.Text;
                }
                else if (tagValueControlSeq.FirstOrDefault() is ComboBox comboTagValue)
                {
                    tag.TagValue = comboTagValue.SelectedItem?.ToString();
                }
                else if (tagValueControlSeq.FirstOrDefault() is NumericUpDown upDownTagValue)
                {
                    tag.TagValue = upDownTagValue.Value.ToString(CultureInfo.InvariantCulture);
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

        private void BVisibility_Click(object sender, EventArgs e)
        {
            var visfrm = new frmSINnerVisibility();
            visfrm.MyVisibility = this.myUC.MyCE.MySINnerFile.SiNnerMetaData.Visibility;
            var result = visfrm.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                this.myUC.MyCE.MySINnerFile.SiNnerMetaData.Visibility = visfrm.MyVisibility;
            }

        }
    }
}
