using System;
using System.CodeDom;
using System.CodeDom.Compiler;
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
using Microsoft.Rest;

namespace ChummerHub.Client.UI
{
    public partial class SINnerGroupSearch : UserControl
    {
        public CharacterExtended MyCE { get; set; }
        public EventHandler<SINnerGroup> OnGroupJoinCallback = null;

        private SINSearchGroupResult _mySINSearchGroupResult = null;
        public SINSearchGroupResult MySINSearchGroupResult
        {
            get
            {
                return _mySINSearchGroupResult;
            }
            set
            {
                try
                {
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                        _mySINSearchGroupResult = value;
                        this.tvGroupSearchResult.Nodes.Clear();
                        if (_mySINSearchGroupResult == null)
                        {
                            this.bCreateGroup.Enabled = !String.IsNullOrEmpty(this.tbSearchGroupname.Text);
                            this.bJoinGroup.Enabled = false;
                        }
                        else
                        {
                            this.bCreateGroup.Enabled = !_mySINSearchGroupResult.SinGroups.Any();
                            this.bJoinGroup.Enabled = _mySINSearchGroupResult.SinGroups.Any();
                        }
                        var rootseq = (from a in MySINSearchGroupResult?.SinGroups select a).ToList();
                        List<TreeNode> nodes = CreateTreeViewNodes(rootseq);
                        this.tvGroupSearchResult.Nodes.AddRange(nodes.ToArray());
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.Message, ex);
                    throw;
                }
            }
        }

        private List<TreeNode> CreateTreeViewNodes(IList<SINnerSearchGroup> sinGroups)
        {
            var res = new List<TreeNode>();
            if (sinGroups == null)
                return res;

            foreach (var ssg in sinGroups)
            {
                TreeNode tn = new TreeNode(ssg.GroupDisplayname);
                if (String.IsNullOrEmpty(ssg.Language))
                    ssg.Language = "en-us";
                int index = frmViewer.LstLanguages.FindIndex(c => c.Value?.ToString() == ssg.Language);
                if (index != -1)
                {
                    tn.ImageIndex = index;
                    tn.SelectedImageIndex = index;
                }
                tn.Tag = ssg;
                var nodes = CreateTreeViewNodes(ssg.MySINSearchGroups);
                tn.Nodes.AddRange(nodes.ToArray());
                res.Add(tn);
            }

            return res;
        }

        public frmSINnerGroupSearch MyParentForm { get; internal set; }

        public SINnerGroupSearch()
        {
            InitializeComponent();
            ImageList myCountryImageList = new ImageList();
            
            foreach (var lang in frmViewer.LstLanguages)
            {
                var img = FlagImageGetter.GetFlagFromCountryCode(lang.Value.ToString().Substring(3, 2));
                myCountryImageList.Images.Add(img);
            }
            tvGroupSearchResult.ImageList = myCountryImageList;
            bCreateGroup.Enabled = false;
            bJoinGroup.Enabled = false;
        }

        private async void bCreateGroup_Click(object sender, EventArgs e)
        {
            try
            {
                var group = new SINnerGroup();
                group.Groupname = this.tbSearchGroupname.Text;
                group.IsPublic = false;
                if ((MyCE.MySINnerFile.MyGroup != null)
                    && ((String.IsNullOrEmpty(tbSearchGroupname.Text))
                    || (tbSearchGroupname.Text == MyCE.MySINnerFile.MyGroup?.Groupname)))
                    group = MyCE.MySINnerFile.MyGroup;
                if (this.tvGroupSearchResult.SelectedNode != null)
                {
                    SINnerSearchGroup sel = tvGroupSearchResult.SelectedNode.Tag as SINnerSearchGroup;
                    if (sel != null)
                    {
                        group = new SINnerGroup(sel);
                    }
                }
                frmSINnerGroupEdit ge = new frmSINnerGroupEdit(group, false);
                var result = ge.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    group = ge.MySINnerGroupCreate.MyGroup;
                    try
                    {
                        using (new CursorWait(false, this))
                        {
                            if (group == MyCE.MySINnerFile.MyGroup)
                            {
                                var client = await StaticUtils.GetClient();
                                var response = await client.PostGroupWithHttpMessagesAsync(group, MyCE.MySINnerFile.Id);
                                if ((response.Response.StatusCode == HttpStatusCode.Accepted)
                                    ||(response.Response.StatusCode == HttpStatusCode.Created)
                                    || (response.Response.StatusCode == HttpStatusCode.OK)) 
                                {
                                    //ok
                                }
                                else if ((response.Response.StatusCode == HttpStatusCode.NotFound))
                                {
                                    var rescontent = await response.Response.Content.ReadAsStringAsync();
                                    string msg = "StatusCode: " + response.Response.StatusCode + Environment.NewLine;
                                    msg += rescontent;
                                    throw new ArgumentNullException(nameof(group), msg);
                                }
                                else
                                {
                                    var rescontent = await response.Response.Content.ReadAsStringAsync();
                                    throw new ArgumentException(rescontent);
                                }
                            }
                            else
                            {
                                //create mode

                                var a = await CreateGroup(ge.MySINnerGroupCreate.MyGroup);
                                if (a != null)
                                {
                                    MySINSearchGroupResult = await SearchForGroups(a.Groupname, null, null);
                                    if (MyParentForm?.MyParentForm != null)
                                        await MyParentForm.MyParentForm.CheckSINnerStatus();
                                }

                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }

                

            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString(), ex);
                MessageBox.Show(ex.ToString());
            }
            
        }

        private async Task<SINnerGroup> CreateGroup(SINnerGroup mygroup)
        {
            try
            {
                if (String.IsNullOrEmpty(this.tbSearchGroupname.Text))
                {
                    MessageBox.Show("Please specify a groupename to create!");
                    this.tbSearchGroupname.Focus();
                    return null;
                }
                if (this.MyCE == null)
                {
                    MessageBox.Show("MySinner not set!");
                    return null;
                }

                var client = await StaticUtils.GetClient();
                var Result = await client.PostGroupWithHttpMessagesAsync(
                    mygroup,
                    MyCE.MySINnerFile.Id);
                var rescontent = await Result.Response.Content.ReadAsStringAsync();
                if ((Result.Response.StatusCode == HttpStatusCode.OK)
                    || (Result.Response.StatusCode == HttpStatusCode.Created))
                {
                    var jsonResultString = Result.Response.Content.ReadAsStringAsync().Result;
                    try
                    {
                        SINnerGroup newgroup = Newtonsoft.Json.JsonConvert.DeserializeObject<SINnerGroup>(jsonResultString);
                        //Guid id;
                        //if (!Guid.TryParse(objIds.ToString(), out id))
                        //{
                        //    string msg = "ChummerHub did not return a valid Id for the group " + this.tbSearchGroupname.Text + ".";
                        //    System.Diagnostics.Trace.TraceError(msg);
                        //    throw new ArgumentException(msg);
                        //}


                        var join = await client.PutSINerInGroupWithHttpMessagesAsync(newgroup.Id,
                            MyCE.MySINnerFile.Id, mygroup.PasswordHash);
                        if (join.Response.StatusCode == HttpStatusCode.OK)
                        {
                            var getgroup = await client.GetGroupByIdWithHttpMessagesAsync(newgroup.Id);
                            MyCE.MySINnerFile.MyGroup = getgroup.Body;
                            if (OnGroupJoinCallback != null)
                                OnGroupJoinCallback(this, getgroup.Body);
                            return getgroup.Body;
                        }
                        else
                        {
                            var joinresp = join.Response.Content.ReadAsStringAsync().Result;
                            System.Diagnostics.Trace.TraceInformation(joinresp);
                            MessageBox.Show(joinresp);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError(ex.ToString());
                        throw;
                    }
                }
                else if (Result.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var jsonResultString = Result.Response.Content.ReadAsStringAsync().Result;
                    if (jsonResultString?.Contains("already exists!") == true)
                    {
                        var searchgroup = await client.GetSearchGroupsWithHttpMessagesAsync(mygroup.Groupname, null, null);
                        var id = searchgroup.Body as Guid?;
                        var getgroup = await client.GetGroupByIdWithHttpMessagesAsync(id);
                        MyCE.MySINnerFile.MyGroup = getgroup.Body;
                        if (OnGroupJoinCallback != null)
                            OnGroupJoinCallback(this, getgroup.Body);
                        return getgroup.Body;
                    }
                }
                else
                {
                    MessageBox.Show(rescontent);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                MessageBox.Show(ex.Message);
                throw;
            }
            return null;
        }

        private async void bSearch_Click(object sender, EventArgs e)
        {
            try
            {
                using (new CursorWait(true, this))
                {
                    lbGroupMembers.DataSource = null;
                    tvGroupSearchResult.SelectedNode = null;
                    this.bSearch.Text = "searching";
                    MySINSearchGroupResult = await SearchForGroups(this.tbSearchGroupname.Text, this.tbSearchByUsername.Text, this.tbSearchByAlias.Text);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.bSearch.Text = "Search";
                TvGroupSearchResult_AfterSelect(sender, new TreeViewEventArgs(new TreeNode()));
            }
            
        }

        private async Task<SINSearchGroupResult> SearchForGroups(string groupname, string user, string alias)
        {
            try
            {
                if (user == "not implemented yet")
                    user = null;
                if (alias == "not implemented yet")
                    alias = null;
                var a = await SearchForGroupsTask(groupname, user, alias);
                
                return a;
            }
            catch(Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.Message, e);
                throw;
            }
            
        }

        private async Task<SINSearchGroupResult> SearchForGroupsTask(string groupname, string userName, string sinnername)
        {
            try
            {
                SINSearchGroupResult ssgr = null;
                var client = await StaticUtils.GetClient();
                var response = await client.GetSearchGroupsWithHttpMessagesAsync(groupname, userName, sinnername);
                if ((response.Response.StatusCode == HttpStatusCode.OK))
                {
                    return (SINSearchGroupResult)response.Body;
                }
                else if ((response.Response.StatusCode == HttpStatusCode.NotFound))
                {
                    var rescontent = await response.Response.Content.ReadAsStringAsync();
                    string msg = "StatusCode: " + response.Response.StatusCode + Environment.NewLine;
                    msg += rescontent;
                    throw new ArgumentNullException(nameof(groupname), msg);
                }
                else
                {
                    var rescontent = await response.Response.Content.ReadAsStringAsync();
                    Exception e = null;
                    try
                    {
                        e = Newtonsoft.Json.JsonConvert.DeserializeObject<Exception>(rescontent);
                    }
                    catch (Exception exception)
                    {
                        throw new ArgumentException(rescontent);
                    }
                    if (e != null)
                        throw e;
                }

                return ssgr;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch(Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.Message, e);
                throw;
            }
            return null;
        }

     
      

        private async void bJoinGroup_Click(object sender, EventArgs e)
        {
            if (tvGroupSearchResult.SelectedNode == null)
                return;

            try
            {
                using (new CursorWait(false, this))
                {
                    SINnerSearchGroup item = tvGroupSearchResult.SelectedNode.Tag as SINnerSearchGroup;
                    if (MyCE.MySINnerFile.MyGroup != null)
                    {
                        await LeaveGroupTask(MyCE.MySINnerFile, MyCE.MySINnerFile.MyGroup, item != null);
                    }

                    if (item == null)
                        return;

                    var uploadtask = MyCE.Upload();
                    await uploadtask.ContinueWith(b =>
                    {
                        using (new CursorWait(false, this))
                        {
                            var task = JoinGroupTask(item, MyCE);
                            task.ContinueWith(a =>
                            {
                                using (new CursorWait(false, this))
                                {
                                    if (a.IsFaulted)
                                    {
                                        string msg = "JoinGroupTask returned faulted!";
                                        if ((a.Exception != null) && (a.Exception is AggregateException))
                                        {
                                            msg = "";
                                            foreach (var exp in (a.Exception as AggregateException).InnerExceptions)
                                            {
                                                msg += exp.Message + Environment.NewLine;
                                            }
                                        }
                                        else
                                        {
                                            if (a.Exception != null)
                                                msg = a.Exception.Message;
                                        }

                                        MessageBox.Show(msg);
                                        return;
                                    }

                                    if (!String.IsNullOrEmpty(a.Result?.ErrorText))
                                    {
                                        System.Diagnostics.Trace.TraceError(a.Result.ErrorText);
                                    }
                                    else
                                    {

                                        MyCE.MySINnerFile.MyGroup = new SINnerGroup(item);
                                        //if (OnGroupJoinCallback != null)
                                        //    OnGroupJoinCallback(this, MyCE.MySINnerFile.MyGroup);
                                        string msg = "Char " + MyCE.MyCharacter.CharacterName + " joined group " +
                                                     item.Groupname +
                                                     ".";
                                        System.Diagnostics.Trace.TraceInformation(msg);
                                        MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        TlpGroupSearch_VisibleChanged(null, new EventArgs());
                                    }
                                }
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message, ex);
                throw;
            }

        }

        private async Task LeaveGroupTask(SINner mySINnerFile, SINnerGroup myGroup, bool noupdate = false)
        {
            try
            {
                var client = await StaticUtils.GetClient();
                var response = await client.DeleteLeaveGroupWithHttpMessagesAsync(myGroup.Id, mySINnerFile.Id);
                if ((response.Response.StatusCode == HttpStatusCode.OK))
                {
                    try
                    {
                        MyCE = MyCE;
                        if (!noupdate)
                            TlpGroupSearch_VisibleChanged(null, new EventArgs());
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.TraceError("Group disbanded: "  + e.Message, e);
                        MessageBox.Show("Group " + myGroup.Groupname + " disbanded because of no members left.");
                    }
                    finally
                    {
                        if ((!noupdate) && (MyParentForm?.MyParentForm != null))
#pragma warning disable 4014
                            MyParentForm.MyParentForm.CheckSINnerStatus();
#pragma warning restore 4014
                    }
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
                MessageBox.Show(e.Message.ToString());
            }
            if (!noupdate)
                OnGroupJoinCallback?.Invoke(this, myGroup);
        }

        private async Task<SINSearchGroupResult> JoinGroupTask(SINnerSearchGroup searchgroup, CharacterExtended myCE)
        {
            bool exceptionlogged = false;
            SINSearchGroupResult ssgr = null;
            try
            {
                SINnerGroup joinGroup = new SINnerGroup(searchgroup);
                DialogResult result = DialogResult.Cancel;
                frmSINnerGroupEdit groupEdit = null;
                PluginHandler.MainForm.DoThreadSafe(() =>
                {
                    groupEdit = new frmSINnerGroupEdit(joinGroup, true);
                    result = groupEdit.ShowDialog(this);
                });
                if (result == DialogResult.OK)
                {
                    try
                    {
                        using (new CursorWait(true, this))
                        {
                            var client = await StaticUtils.GetClient();
                            var response =
                                await client.PutSINerInGroupWithHttpMessagesAsync(searchgroup.Id, myCE.MySINnerFile.Id,
                                    groupEdit.MySINnerGroupCreate.MyGroup.PasswordHash);
                            if ((response.Response.StatusCode != HttpStatusCode.OK))
                            {
                                var rescontent = await response.Response.Content.ReadAsStringAsync();
                                if (response.Response.StatusCode == HttpStatusCode.BadRequest)
                                {
                                    if (rescontent.Contains("PW is wrong!"))
                                    {
                                        throw new ArgumentException("Wrong Password provided!");
                                    }

                                    string searchfor = "NoUserRightException\",\"Message\":\"";
                                    if (rescontent.Contains(searchfor))
                                    {
                                        string msg =
                                            rescontent.Substring(rescontent.IndexOf(searchfor) + searchfor.Length);
                                        msg = msg.Substring(0, msg.IndexOf("\""));
                                        throw new ArgumentException(msg);
                                    }

                                    throw new ArgumentException(rescontent);
                                }
                                else
                                {
                                    string msg = "StatusCode: " + response.Response.StatusCode + Environment.NewLine;
                                    msg += rescontent;
                                    throw new ArgumentException(msg);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.TraceInformation(e.Message, e);
                        exceptionlogged = true;
                        throw;
                    }
                    finally
                    {
                        MyParentForm?.MyParentForm?.CheckSINnerStatus();
                    }
                }
                
                
            }
            catch (Exception e)
            {
                if (!exceptionlogged)
                    System.Diagnostics.Trace.TraceError(e.Message, e);
                throw;
            }
            
            
            return ssgr;
        }

        private async void TlpGroupSearch_VisibleChanged(object sender, EventArgs e)
        {
            if (this.MyCE == null)
                return;
            if (Visible == false)
                return;
            if (MyCE.MySINnerFile.MyGroup != null)
            {
                using (new CursorWait(true, this))
                {
                    
                    try
                    {
                        using (new CursorWait(true, this))
                        {
                            //MySINSearchGroupResult = null;
                            if (String.IsNullOrEmpty(this.tbSearchGroupname.Text))
                            {
                                var temp = new SINSearchGroupResult(MyCE.MySINnerFile.MyGroup);
                                MySINSearchGroupResult = temp;

                            }
                            else
                            {
                                MySINSearchGroupResult =
                                    await SearchForGroups(this.tbSearchGroupname.Text, null, null);
                            }
                        }
                    }
                    catch (ArgumentNullException ex)
                    {
                        System.Diagnostics.Trace.TraceInformation(
                            "No group found with name: " + MyCE.MySINnerFile.MyGroup.Groupname);
                        MyCE.MySINnerFile.MyGroup = null;
                        MyParentForm?.MyParentForm?.CheckSINnerStatus();
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                    finally
                    {
                        TvGroupSearchResult_AfterSelect(sender, new TreeViewEventArgs(new TreeNode()));
                    }
                }
                
            }
        }

        private void TbSearchGroupname_TextChanged(object sender, EventArgs e)
        {
            bCreateGroup.Enabled = true;
            bCreateGroup.Text = "create group";
            bJoinGroup.Enabled = false;
            this.tvGroupSearchResult.SelectedNode = null;

        }

        private void TbSearchGroupname_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                bSearch_Click(this, new EventArgs());
            }
        }

        private void TvGroupSearchResult_AfterSelect(object sender, TreeViewEventArgs e)
        {
            PluginHandler.MainForm.DoThreadSafe(() =>
            {

                var item = tvGroupSearchResult.SelectedNode?.Tag as SINnerSearchGroup;
                if (item != null)
                {
                    if (this.MyCE.MySINnerFile.MyGroup == null)
                    {
                        this.bJoinGroup.Enabled = true;
                        this.bCreateGroup.Text = "view group";
                        this.bJoinGroup.Text = "join group";
                    }
                    else if (((this.MyCE.MySINnerFile.MyGroup?.Id != item.Id)
                              || (this.MyCE.MySINnerFile.MyGroup.Groupname != this.tbSearchGroupname.Text))
                             && (MyCE.MySINnerFile.MyGroup.Groupname != item.Groupname))
                    {
                        this.bCreateGroup.Enabled = true;
                        this.bCreateGroup.Text = "view group";
                        this.bJoinGroup.Enabled = true;
                        this.bJoinGroup.Text = "switch to group";
                    }
                    else
                    {
                        this.bCreateGroup.Enabled = true;
                        this.bCreateGroup.Text = "view group";
                        this.bJoinGroup.Text = "leave group";
                    }
                    var members = item.MyMembers;
                    lbGroupMembers.DataSource = members;
                    lbGroupMembers.DisplayMember = "Display";
                }
                else
                {
                    lbGroupMembers.DataSource = null;
                    this.bJoinGroup.Enabled = false;
                    if (!String.IsNullOrEmpty(this.tbSearchGroupname.Text))
                    {
                        this.bCreateGroup.Enabled = true;
                        this.bCreateGroup.Text = "create group";
                    }
                    else
                    {
                        this.bCreateGroup.Enabled = false;
                        this.bCreateGroup.Text = "create group";
                    }
                }
            });
        }

        private void BGroupFoundLoadInCharacterRoster_Click(object sender, EventArgs e)
        {
            PluginHandler.MainForm.DoThreadSafe(() =>
            {

                var item = tvGroupSearchResult.SelectedNode?.Tag as SINnerSearchGroup;
                if (item != null)
                {
                    var list = new List<SINnerSearchGroup>() {item};
                    var nodelist = ChummerHub.Client.Backend.Utils.CharacterRosterTreeNodifyGroupList(list);
                    foreach (var node in nodelist)
                    {
                        PluginHandler.MyTreeNodes2Add.AddOrUpdate(node.Name, node, (key, oldValue) => node);
                    }
                    PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false, true);
                    PluginHandler.MainForm.CharacterRoster.BringToFront();
                    this.MyParentForm.Close();
                }
            });
        }

        private async void BGroupsFoundDeleteGroup_Click(object sender, EventArgs e)
        {
            var item = tvGroupSearchResult.SelectedNode?.Tag as SINnerSearchGroup;
            if (item != null)
            {
                try
                {
                    var client = await StaticUtils.GetClient();
                    var response = await client.DeleteGroupWithHttpMessagesAsync(item.Id).CancelAfter(1000 * 30);
                    if ((response.Response.StatusCode == HttpStatusCode.OK))
                    {
                        bSearch_Click(sender, e);
                        MessageBox.Show("Group deleted.");
                    }
                    else if ((response.Response.StatusCode == HttpStatusCode.NotFound))
                    {
                        var rescontent = await response.Response.Content.ReadAsStringAsync();
                        string msg = "StatusCode: " + response.Response.StatusCode + Environment.NewLine;
                        msg += rescontent;
                        throw new ArgumentNullException(item.Groupname, msg);
                    }
                    else
                    {
                        var rescontent = await response.Response.Content.ReadAsStringAsync();
                        Exception ex = null;
                        try
                        {
                            ex = Newtonsoft.Json.JsonConvert.DeserializeObject<Exception>(rescontent);
                        }
                        catch (Exception exception)
                        {
                            throw new ArgumentException(rescontent);
                        }
                        if (ex != null)
                            throw ex;
                    }
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Trace.TraceError(exception.ToString());
                    Console.WriteLine(exception);
                    MessageBox.Show(exception.ToString(), "Error deleting Group", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            
               
            }
        }
    }
}
