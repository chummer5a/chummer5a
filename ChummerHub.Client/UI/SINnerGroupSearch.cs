using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChummerHub.Client.Backend;
using System.Net;
using Chummer;
using SINners.Models;
using ChummerHub.Client.Model;
using Chummer.Plugins;

namespace ChummerHub.Client.UI
{
    public partial class SINnerGroupSearch : UserControl
    {
        public CharacterExtended MyCE { get; set; }
        public SINnerGroupSearch()
        {
            InitializeComponent();
        }

        private void bCreateGroup_Click(object sender, EventArgs e)
        {
            var task = CreateGroup(this.tbSearchGroupname.Text);
            task.ContinueWith(a =>
            {
                PluginHandler.MainForm.DoThreadSafe(() =>
                {
                    var test = a.Result;
                    SearchForGroups(test.Groupname, null, null);
                });


            });
        }

        private async Task<SINnerGroup> CreateGroup(string groupname)
        {
            try
            {
                if(String.IsNullOrEmpty(this.tbSearchGroupname.Text))
                {
                    MessageBox.Show("Please specify a groupename to create!");
                    this.tbSearchGroupname.Focus();
                    return null;
                }
                if(this.MyCE == null)
                {
                    MessageBox.Show("MySinner not set!");
                    return null;
                }

                bool uploaded = await MyCE.Upload();
                if (!uploaded)
                {
                    MessageBox.Show("SINner not successully uploaded.");
                    return null;
                }
                
                var Result = await StaticUtils.Client.PostGroupWithHttpMessagesAsync(this.tbSearchGroupname.Text, MyCE.MySINnerFile.Id);
                var rescontent = await Result.Response.Content.ReadAsStringAsync();
                if((Result.Response.StatusCode == HttpStatusCode.OK)
                    || (Result.Response.StatusCode == HttpStatusCode.Created))
                {
                    var jsonResultString = Result.Response.Content.ReadAsStringAsync().Result;
                    try
                    {
                        Object objIds = Newtonsoft.Json.JsonConvert.DeserializeObject<Object>(jsonResultString);
                        Guid? id = objIds as Guid?;
                        if(id == null)
                        {
                            string msg = "ChummerHub did not return a valid Id for the group " + this.tbSearchGroupname.Text + ".";
                            System.Diagnostics.Trace.TraceError(msg);
                            throw new ArgumentException(msg);
                        }

                        var join = await StaticUtils.Client.PutSINerInGroupWithHttpMessagesAsync(id,
                            MyCE.MySINnerFile.Id);
                        if (join.Response.StatusCode == HttpStatusCode.OK)
                        {
                            var getgroup = await StaticUtils.Client.GetGroupByIdWithHttpMessagesAsync(id);
                            MyCE.MySINnerFile.MyGroup = getgroup.Body;
                            MessageBox.Show("Group " + getgroup.Body.Groupname + "joined!");
                        }
                        else
                        {
                            var joinresp = join.Response.Content.ReadAsStringAsync().Result;
                            MessageBox.Show(joinresp);
                        }
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError(ex.ToString());
                        throw;
                    }
                }
                else
                {
                    MessageBox.Show(rescontent);
                }

            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                MessageBox.Show(ex.Message);
                throw;
            }
            return null;
        }

        private void bSearch_Click(object sender, EventArgs e)
        {
            SearchForGroups(this.tbSearchGroupname.Text, this.tbSearchByUsername.Text, this.tbSearchByAlias.Text);
        }

        private void SearchForGroups(string groupname, string user, string alias)
        {
            var task = SearchForGroupsTask(groupname, user, alias);
            task.ContinueWith(a =>
            {
                PluginHandler.MainForm.DoThreadSafe(() =>
                {
                    var test = a.Result.SinGroups;
                    this.lbGroupSearchResult.DataSource = test;
                    this.lbGroupSearchResult.DisplayMember = "Groupname";
                    if (this.lbGroupSearchResult.Items.Count > 0)
                    {
                        this.lbGroupSearchResult.SelectedItem = this.lbGroupSearchResult.Items[0];
                    }
                });
            });
        }

        private async Task<SINSearchGroupResult> SearchForGroupsTask(string groupname, string userName, string sinnername)
        {
            SINSearchGroupResult ssgr = null;
            var response = await StaticUtils.Client.GetSearchGroupsWithHttpMessagesAsync(groupname, userName, sinnername);
            if((response.Response.StatusCode == HttpStatusCode.OK))
            {
                return response.Body;
            }
            else
            {
                var rescontent = await response.Response.Content.ReadAsStringAsync();
                MessageBox.Show(rescontent);
            }
            return ssgr;
        }

     
        private void lbGroupSearchResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = lbGroupSearchResult.SelectedItem as SINnerSearchGroup;
            if (item != null)
            {
                if (this.MyCE.MySINnerFile.MyGroup == null)
                {
                    this.bJoinGroup.Enabled = true;
                    this.bJoinGroup.Text = "join group";
                }
                else if (this.MyCE.MySINnerFile.MyGroup?.Id != item.Id)

                {
                    this.bJoinGroup.Enabled = true;
                    this.bJoinGroup.Text = "switch to group";
                }
                else
                {
                    this.bJoinGroup.Text = "leave group";
                }
                var members = item.MyMembers;
                lbGroupMembers.DataSource = members;
                lbGroupMembers.DisplayMember = "Display";
            }
            else
            {
                this.bJoinGroup.Enabled = false;
            }
        }

        private void bJoinGroup_Click(object sender, EventArgs e)
        {
            if (lbGroupSearchResult.SelectedItem == null)
                return;

            try
            {


                SINnerSearchGroup item = lbGroupSearchResult.SelectedItem as SINnerSearchGroup;
                if (item == null)
                    return;
                var uploadtask = MyCE.Upload();
                uploadtask.ContinueWith(b =>
                {
                    var task = JoinGroupTask(item, MyCE);
                    //task.Wait(TimeSpan.FromMinutes(1));
                    task.ContinueWith(a =>
                    {
                        if(!String.IsNullOrEmpty(a.Result?.ErrorText))
                        {
                            System.Diagnostics.Trace.TraceError(a.Result.ErrorText);
                        }
                        else
                        {
                            System.Diagnostics.Trace.TraceInformation(
                                "Char " + MyCE.MyCharacter.CharacterName + " joined group " + item.Groupname + ".");
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message, ex);
                throw;
            }

        }

        private async Task<SINSearchGroupResult> JoinGroupTask(SINnerSearchGroup searchgroup, CharacterExtended myCE)
        {
            SINSearchGroupResult ssgr = null;
            try
            {
                var response =
                    await StaticUtils.Client.PutSINerInGroupWithHttpMessagesAsync(searchgroup.Id, myCE.MySINnerFile.Id);
                if ((response.Response.StatusCode == HttpStatusCode.OK))
                {
                    SearchForGroups(searchgroup.Groupname, null, myCE.MyCharacter.CharacterName);
                }
                else
                {
                    var rescontent = await response.Response.Content.ReadAsStringAsync();
                    string msg = "StatusCode: " + response.Response.StatusCode + Environment.NewLine;
                    msg += rescontent;
                    MessageBox.Show(msg);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.Message, e);
                throw;
            }

            return ssgr;
        }
    }
}
