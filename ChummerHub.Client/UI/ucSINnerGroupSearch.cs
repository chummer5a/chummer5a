using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChummerHub.Client.Backend;
using System.Net;
using System.Text;
using System.Threading;
using Chummer;
using SINners.Models;
using ChummerHub.Client.Model;
using Chummer.Plugins;
using Point = System.Drawing.Point;
using NLog;

namespace ChummerHub.Client.UI
{
    public partial class ucSINnerGroupSearch : UserControl
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public CharacterExtended MyCE { get; set; }
        public EventHandler<SINnerGroup> OnGroupJoinCallback = null;

        private SINSearchGroupResult _mySINSearchGroupResult;
        public SINSearchGroupResult MySINSearchGroupResult
        {
            get => _mySINSearchGroupResult;
            set
            {
                try
                {
                    this.DoThreadSafe(() =>
                    {
                        _mySINSearchGroupResult = value;
                        tvGroupSearchResult.Nodes.Clear();
                        if (_mySINSearchGroupResult == null)
                        {
                            bJoinGroup.Enabled = false;
                        }
                        else if (_mySINSearchGroupResult.SinGroups != null)
                        {
                            bJoinGroup.Enabled = _mySINSearchGroupResult.SinGroups.Count > 0;
                            TreeNode[] nodes = CreateTreeViewNodes(_mySINSearchGroupResult.SinGroups).ToArray();
                            tvGroupSearchResult.DoThreadSafe(() => tvGroupSearchResult.Nodes.AddRange(nodes));
                        }
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    throw;
                }
            }
        }

        private List<TreeNode> CreateTreeViewNodes(IEnumerable<SINnerSearchGroup> sinGroups)
        {
            var res = new List<TreeNode>();
            if (sinGroups == null)
                return res;

            foreach (var ssg in sinGroups)
            {
                TreeNode tn = new TreeNode(ssg.GroupDisplayname);
                if (string.IsNullOrEmpty(ssg.Language))
                    ssg.Language = "en-us";
                int index = frmViewer.LstLanguages.FindIndex(c => c.Value?.ToString() == ssg.Language);
                if (index != -1)
                {
                    tn.ImageIndex = index;
                    tn.SelectedImageIndex = index;
                }
                tn.Tag = ssg;
                tn.ToolTipText = ssg.Description;
                if (cbShowMembers.Checked)
                {
                    foreach (var member in ssg.MyMembers)
                    {
                        TreeNode tnm = new TreeNode(member.MySINner.Alias)
                        {
                            Tag = member,
                            ImageIndex = tvGroupSearchResult.ImageList.Images.Count - 1,
                            SelectedImageIndex = tvGroupSearchResult.ImageList.Images.Count - 1
                        };
                        if (!string.IsNullOrEmpty(member.MySINner?.Language))
                        {
                            int mindex = frmViewer.LstLanguages.FindIndex(c => c.Value?.ToString() == member.MySINner?.Language);
                            if (mindex != -1)
                            {
                                tnm.ImageIndex = mindex;
                                tnm.SelectedImageIndex = mindex;
                            }
                        }
                        tn.Nodes.Add(tnm);
                    }
                }
                var nodes = CreateTreeViewNodes(ssg.MySINSearchGroups);
                tn.Nodes.AddRange(nodes.ToArray());
                res.Add(tn);
            }

            return res;
        }

        public frmSINnerGroupSearch MyParentForm { get; internal set; }

        public ucSINnerGroupSearch()
        {
            InitializeComponent();
            ImageList myCountryImageList = new ImageList();
            foreach (var lang in frmViewer.LstLanguages)
            {
                var img = FlagImageGetter.GetFlagFromCountryCode(lang.Value.ToString().Substring(3, 2));
                myCountryImageList.Images.Add(img);
            }
            var empty = FlagImageGetter.GetFlagFromCountryCode("noimagedots");
            myCountryImageList.Images.Add(empty);
            tvGroupSearchResult.ImageList = myCountryImageList;
            bCreateGroup.Enabled = true;
            bJoinGroup.Enabled = false;
        }

        private async Task<SINnerGroup> CreateGroup(SINnerGroup mygroup)
        {
            try
            {
                if (string.IsNullOrEmpty(tbSearchGroupname.Text) && mygroup == null)
                {
                    Program.MainForm.ShowMessageBox("Please specify a groupename to create!");
                    tbSearchGroupname.Focus();
                    return null;
                }
                var client = StaticUtils.GetClient();
                using (var res = await client.PostGroupWithHttpMessagesAsync(
                    mygroup,
                    MyCE?.MySINnerFile?.Id).ConfigureAwait(true))
                {
                    ResultGroupPostGroup response = await Backend.Utils.HandleError(res, res.Body).ConfigureAwait(true) as ResultGroupPostGroup;
                    if (response?.CallSuccess == true)
                    {
                        return response.MyGroup;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Program.MainForm.ShowMessageBox(ex.Message);
                throw;
            }
            return null;
        }

        private async void bSearch_Click(object sender, EventArgs e)
        {
            try
            {
                using (new CursorWait(this, true))
                {
                    tvGroupSearchResult.SelectedNode = null;
                    bSearch.Text = "searching";
                    MySINSearchGroupResult = await SearchForGroups(tbSearchGroupname.Text).ConfigureAwait(true);
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex);
                Program.MainForm.ShowMessageBox(ex.Message);
            }
            finally
            {
                bSearch.Text = "Search";
                TvGroupSearchResult_AfterSelect(sender, new TreeViewEventArgs(new TreeNode()));
            }

        }

        public static async Task<SINSearchGroupResult> SearchForGroups(string groupname)
        {
            try
            {
                var a = await SearchForGroupsTask(groupname).ConfigureAwait(true);
                return a;
            }
            catch(Exception e)
            {
                Log.Error(e);
                throw;
            }

        }

        private static async Task<SINSearchGroupResult> SearchForGroupsTask(string groupname)
        {
            try
            {
                var client = StaticUtils.GetClient();
                using (var res = await client.GetSearchGroupsWithHttpMessagesAsync(groupname).ConfigureAwait(true))
                {
                    if (!(await Backend.Utils.HandleError(res, res.Body).ConfigureAwait(true) is ResultGroupGetSearchGroups result))
                        return null;
                    if (result.CallSuccess == true)
                    {
                        return result.MySearchGroupResult;
                    }
                }
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch(Exception e)
            {
                Log.Error(e);
                throw;
            }
            return null;
        }

        private async void bJoinGroup_Click(object sender, EventArgs e)
        {
            if (MyCE == null)
                return;
            if (tvGroupSearchResult.SelectedNode == null)
                return;

            try
            {
                using (new CursorWait(this))
                {
                    SINnerSearchGroup item = tvGroupSearchResult.SelectedNode.Tag as SINnerSearchGroup;
                    if (MyCE.MySINnerFile.MyGroup != null)
                    {
                        await LeaveGroupTask(MyCE.MySINnerFile, MyCE.MySINnerFile.MyGroup, item != null).ConfigureAwait(true);
                    }

                    if (item == null)
                        return;

                    //var uploadtask = MyCE.Upload();
                    //await uploadtask.ContinueWith(b =>
                    //{
                    using (new CursorWait(this))
                    {
                        var task = JoinGroupTask(item, MyCE);
#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
                        await task.ContinueWith(a =>
                        {
                            if (a.IsFaulted)
                            {
                                StringBuilder msg = new StringBuilder("JoinGroupTask returned faulted!");
                                if (a.Exception != null)
                                {
                                    msg.Clear();
                                    if (a.Exception is AggregateException exAggregate)
                                    {
                                        foreach (Exception exp in exAggregate.InnerExceptions)
                                        {
                                            msg.AppendLine(exp.Message);
                                        }
                                    }
                                    else
                                        msg = new StringBuilder(a.Exception.Message);
                                }

                                Program.MainForm.ShowMessageBox(msg.ToString());
                                return;
                            }

                            if (!string.IsNullOrEmpty(a.Result?.ErrorText))
                            {
                                Log.Error(a.Result.ErrorText);
                            }
                            else if (a.Result == null)
                            {
                                MyCE.MySINnerFile.MyGroup = null;
                                string msg = "Char " + MyCE.MyCharacter.CharacterName + " did not join group " +
                                             item.Groupname +
                                             ".";
                                Log.Info(msg);
                                Program.MainForm.ShowMessageBox(msg, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                this.DoThreadSafe(() => TlpGroupSearch_VisibleChanged(null, new EventArgs()));
                            }
                            else
                            {

                                MyCE.MySINnerFile.MyGroup = new SINnerGroup(item);
                                //if (OnGroupJoinCallback != null)
                                //    OnGroupJoinCallback(this, MyCE.MySINnerFile.MyGroup);
                                string msg = "Char " + MyCE.MyCharacter.CharacterName + " joined group " +
                                             item.Groupname +
                                             ".";
                                Log.Info(msg);
                                Program.MainForm.ShowMessageBox(msg, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                this.DoThreadSafe(() => TlpGroupSearch_VisibleChanged(null, new EventArgs()));
                                PluginHandler.MainForm.CharacterRoster.DoThreadSafe(() =>
                                    PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false));
                            }
                        }).ConfigureAwait(true);
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }

        }

        private async Task LeaveGroupTask(SINner mySINnerFile, SINnerGroup myGroup, bool noupdate = false)
        {
            try
            {
                var client = StaticUtils.GetClient();
                using (var response = await client.DeleteLeaveGroupWithHttpMessagesAsync(myGroup.Id, mySINnerFile.Id).ConfigureAwait(true))
                {
                    if (response.Response.StatusCode == HttpStatusCode.OK)
                    {
                        try
                        {
                            MyCE = MyCE;
                            if (!noupdate)
                                TlpGroupSearch_VisibleChanged(null, new EventArgs());
                        }
                        catch (Exception e)
                        {
                            Log.Warn("Group disbanded: " + e.Message);
                            Program.MainForm.ShowMessageBox("Group " + myGroup.Groupname + " disbanded because of no members left.");
                        }
                        finally
                        {
                            if (!noupdate)
#pragma warning disable 4014
                                MyParentForm?.MyParentForm?.CheckSINnerStatus();
#pragma warning restore 4014
                        }
                    }
                    else
                    {
                        var rescontent = await response.Response.Content.ReadAsStringAsync().ConfigureAwait(true);
                        string msg = "StatusCode: " + response.Response.StatusCode + Environment.NewLine;
                        msg += rescontent;
                        Log.Info(msg);
                        Program.MainForm.ShowMessageBox(msg);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                Program.MainForm.ShowMessageBox(e.Message);
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
                DialogResult result = DialogResult.Cancel;
                frmSINnerGroupEdit groupEdit = null;
                if (searchgroup.HasPassword == true)
                {
                    SINnerGroup joinGroup = new SINnerGroup(searchgroup);
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                        groupEdit = new frmSINnerGroupEdit(joinGroup, true);
                        result = groupEdit.ShowDialog(this);
                    });
                }

                if (result == DialogResult.OK || searchgroup.HasPassword == false)
                {
                    try
                    {
                        using (new CursorWait(this, true))
                        {
                            var client = StaticUtils.GetClient();
                            using (var response =
                                await client.PutSINerInGroupWithHttpMessagesAsync(searchgroup.Id, myCE.MySINnerFile.Id,
                                    groupEdit?.MySINnerGroupCreate?.MyGroup?.PasswordHash).ConfigureAwait(true))
                            {
                                if (response.Response.StatusCode != HttpStatusCode.OK)
                                {
                                    var rescontent = await response.Response.Content.ReadAsStringAsync().ConfigureAwait(true);
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
                                                rescontent.Substring(rescontent.IndexOf(searchfor, StringComparison.Ordinal) + searchfor.Length);
                                            msg = msg.Substring(0, msg.IndexOf("\"", StringComparison.Ordinal));
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

                            using (var found = await client.GetGroupByIdWithHttpMessagesAsync(searchgroup.Id, null,
                                CancellationToken.None).ConfigureAwait(true))
                            {
                                var res = Backend.Utils.HandleError(found);
                                if (found?.Response?.IsSuccessStatusCode == true)
                                {
                                    ssgr = new SINSearchGroupResult(found.Body.MyGroup);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
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
                {
                    Log.Error(e);
                }

                throw;
            }


            return ssgr;
        }

        private async void TlpGroupSearch_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible == false)
                return;
            lSINnerName.Text = "not set";
            cbShowMembers.Checked = MyCE == null;
            if (MyCE == null)
                return;
            lSINnerName.Text = MyCE.MySINnerFile.Alias;
            if (MyCE?.MySINnerFile.MyGroup != null)
            {
                using (new CursorWait(this, true))
                {
                    try
                    {
                        //MySINSearchGroupResult = null;
                        if (string.IsNullOrEmpty(tbSearchGroupname.Text))
                        {
                            var temp = new SINSearchGroupResult(MyCE?.MySINnerFile.MyGroup);
                            MySINSearchGroupResult = temp;
                        }
                        else
                        {
                            MySINSearchGroupResult = await SearchForGroups(tbSearchGroupname.Text).ConfigureAwait(true);
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        Log.Info(
                            "No group found with name: " + MyCE?.MySINnerFile.MyGroup.Groupname);
                        if (MyCE?.MySINnerFile != null)
                            MyCE.MySINnerFile.MyGroup = null;
                        MyParentForm?.MyParentForm?.CheckSINnerStatus();
                    }
                    catch (Exception exception)
                    {
                        Log.Error(exception);
                        Program.MainForm.ShowMessageBox(exception.Message);
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
            tvGroupSearchResult.SelectedNode = null;

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
                if (MyCE == null)
                    bJoinGroup.Enabled = false;

                if (tvGroupSearchResult.SelectedNode?.Tag is SINnerSearchGroup item)
                {
                    if (MyCE?.MySINnerFile.MyGroup == null)
                    {
                        bCreateGroup.Enabled = true;
                        bJoinGroup.Enabled = true;
                    }
                    else if ((MyCE?.MySINnerFile.MyGroup?.Id != item.Id
                              || MyCE?.MySINnerFile.MyGroup.Groupname != tbSearchGroupname.Text)
                             && MyCE?.MySINnerFile.MyGroup.Groupname != item.Groupname)
                    {
                        bCreateGroup.Enabled = true;
                        bJoinGroup.Enabled = true;
                        bJoinGroup.Text = "join group";
                    }
                    else
                    {
                        bCreateGroup.Enabled = true;
                        bJoinGroup.Enabled = true;
                        bJoinGroup.Text = "leave group";
                    }
                    var members = item.MyMembers;
                }
                else
                {
                    bJoinGroup.Enabled = false;
                }
            });
        }

        private void BGroupFoundLoadInCharacterRoster_Click(object sender, EventArgs e)
        {
            PluginHandler.MainForm.DoThreadSafe(() =>
            {
                TreeNode selectedNode = tvGroupSearchResult.SelectedNode;
                var item = selectedNode?.Tag as SINnerSearchGroup;
                while (item == null && selectedNode?.Parent != null)
                {
                    selectedNode = selectedNode.Parent;
                    item = selectedNode?.Tag as SINnerSearchGroup;
                }
                if (item != null)
                {
                    var list = new List<SINnerSearchGroup>() {item};
                    var nodelist = Backend.Utils.CharacterRosterTreeNodifyGroupList(list);
                    foreach (var node in nodelist)
                    {
                        PluginHandler.MyTreeNodes2Add.AddOrUpdate(node.Name, node, (key, oldValue) => node);
                    }
                    PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false);
                    PluginHandler.MainForm.CharacterRoster.BringToFront();
                    MyParentForm.Close();
                }
            });
        }

        private async void BGroupsFoundDeleteGroup_Click(object sender, EventArgs e)
        {
            if (tvGroupSearchResult.SelectedNode?.Tag is SINnerSearchGroup item)
            {
                try
                {
                    var client = StaticUtils.GetClient();
                    using (var response = await client.DeleteGroupWithHttpMessagesAsync(item.Id).CancelAfter(1000 * 30).ConfigureAwait(true))
                    {
                        if (response.Response.StatusCode == HttpStatusCode.OK)
                        {
                            bSearch_Click(sender, e);
                            Program.MainForm.ShowMessageBox("Group deleted.");
                        }
                        else if (response.Response.StatusCode == HttpStatusCode.NotFound)
                        {
                            var rescontent = await response.Response.Content.ReadAsStringAsync().ConfigureAwait(true);
                            string msg = "StatusCode: " + response.Response.StatusCode + Environment.NewLine;
                            msg += rescontent;
                            throw new ArgumentNullException(item.Groupname, msg);
                        }
                        else
                        {
                            var rescontent = await response.Response.Content.ReadAsStringAsync().ConfigureAwait(true);
                            Exception ex;
                            try
                            {
                                ex = Newtonsoft.Json.JsonConvert.DeserializeObject<Exception>(rescontent);
                            }
                            catch (Exception)
                            {
                                throw new ArgumentException(rescontent);
                            }

                            if (ex != null)
                                throw ex;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                    Program.MainForm.ShowMessageBox(exception.ToString(), "Error deleting Group", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void TvGroupSearchResult_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void TvGroupSearchResult_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private async void TvGroupSearchResult_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the drop location.
            Point targetPoint = tvGroupSearchResult.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.
            TreeNode targetNode = tvGroupSearchResult.GetNodeAt(targetPoint);

            // Retrieve the node that was dragged.
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            // Sanity check
            if (draggedNode == null)
            {
                return;
            }
            SINnerSearchGroup draggedGroup = draggedNode.Tag as SINnerSearchGroup;
            SINnerSearchGroupMember draggedSINner = draggedNode.Tag as SINnerSearchGroupMember;
            SINnerSearchGroup targetGroup = null;
            TreeNode parentNode = targetNode;
            // Did the user drop on a valid target node?
            if (targetNode != null)
            {
                targetGroup = parentNode.Tag as SINnerSearchGroup;

                while (targetGroup == null && parentNode.Parent != null)
                {
                    parentNode = parentNode.Parent;
                    targetGroup = parentNode.Tag as SINnerSearchGroup;
                }
            }

            // Confirm that the node at the drop location is not
            // the dragged node and that target node isn't null
            // (for example if you drag outside the control)
            if (!draggedNode.Equals(targetNode)/* && targetNode != null*/)
            {
                bool canDrop = true;

                // Crawl our way up from the node we dropped on to find out if
                // if the target node is our parent.
                while (canDrop && parentNode != null && targetGroup == null)
                {
                    canDrop = !ReferenceEquals(draggedNode, parentNode);
                    parentNode = parentNode.Parent;
                    targetGroup = parentNode?.Tag as SINnerSearchGroup;
                    targetNode = parentNode;
                }

                // Is this a valid drop location?
                if (canDrop)
                {
                    // Yes. Move the node, expand it, and select it.

                    var client = StaticUtils.GetClient();
                    if (draggedGroup != null)
                    {

                        using (var res = await client.PutGroupInGroupWithHttpMessagesAsync(draggedGroup.Id,
                            draggedGroup.Groupname, targetGroup?.Id,
                            draggedGroup.MyAdminIdentityRole, draggedGroup.IsPublic).ConfigureAwait(true))
                        {
                            if (res.Response.StatusCode == HttpStatusCode.OK)
                            {
                                MoveNode(draggedNode, targetNode);
                            }
                        }
                    }
                    if (draggedSINner?.MySINner?.MyGroup != null && targetGroup != null)
                    {
                        if (targetGroup.Groupname == "My SINners")
                        {
                            using (var res = await client.DeleteLeaveGroupWithHttpMessagesAsync(
                                draggedSINner.MySINner.MyGroup.Id, draggedSINner.MySINner.Id).ConfigureAwait(true))
                            {
                                var response = Backend.Utils.HandleError(res, res.Body);
                                if (res.Response.StatusCode == HttpStatusCode.OK)
                                {
                                    bSearch_Click(sender, e);
                                }
                            }
                        }
                        if (draggedSINner.MySINner.MyGroup?.Id == targetGroup.Id)
                            return;
                        if (draggedSINner.MySINner.MyGroup == null && targetGroup.Id == null)
                            return;

                        try
                        {
                            using (var res = await client.PutSINerInGroupWithHttpMessagesAsync(targetGroup.Id, draggedSINner.MySINner.Id).ConfigureAwait(true))
                            {
                                var response = Backend.Utils.HandleError(res, res.Body);
                                if (res.Response.StatusCode == HttpStatusCode.OK)
                                {
                                    bSearch_Click(sender, e);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            await Backend.Utils.HandleError(exception).ConfigureAwait(true);
                        }
                    }

                    if (draggedSINner?.MySINner?.MyGroup?.Id != null && targetNode == null)
                    {
                        using (var res = await client.DeleteLeaveGroupWithHttpMessagesAsync(
                            draggedSINner.MySINner.MyGroup.Id, draggedSINner.MySINner.Id).ConfigureAwait(true))
                        {
                            var response = Backend.Utils.HandleError(res, res.Body);
                            if (res.Response.StatusCode == HttpStatusCode.OK)
                            {
                                MoveNode(draggedNode, targetNode);
                            }
                        }
                    }
                    else if (draggedSINner?.MySINner != null && targetNode == null)
                    {
                        using (var res = await client.PutSINerInGroupWithHttpMessagesAsync(null, draggedSINner.MySINner.Id).ConfigureAwait(true))
                        {
                            var response = Backend.Utils.HandleError(res, res.Body);
                            if (res.Response.StatusCode == HttpStatusCode.OK)
                            {
                                MoveNode(draggedNode, targetNode);
                            }
                        }
                    }
                    else if (draggedSINner?.MySINner != null && targetGroup != null)
                    {
                        using (var res = await client.PutSINerInGroupWithHttpMessagesAsync(targetGroup.Id, draggedSINner.MySINner.Id).ConfigureAwait(true))
                        {
                            var response = Backend.Utils.HandleError(res, res.Body);
                            if (res.Response.StatusCode == HttpStatusCode.OK)
                            {
                                MoveNode(draggedNode, targetNode);
                            }
                        }
                    }
                }
            }

            // Optional: Select the dropped node and navigate (however you do it)
            tvGroupSearchResult.SelectedNode = draggedNode;
            // NavigateToContent(draggedNode.Tag);

        }

        private void MoveNode(TreeNode draggedNode, TreeNode targetNode)
        {
            draggedNode.Remove();
            if (targetNode != null)
            {
                TreeNode targetGroup = targetNode;
                if (!(targetGroup.Tag is SINnerSearchGroup) && targetGroup.Parent != null)
                    targetGroup = targetGroup.Parent;

                if (targetGroup == null)
                    return;
                targetGroup.Nodes.Add(draggedNode);
                targetGroup.Expand();
            }
            else
            {
                // The user dropped the node on the treeview control instead
                // of another node so lets place the node at the bottom of the tree.
                //tvGroupSearchResult.Nodes.Add(draggedNode);
                //draggedNode.Expand();
            }
        }


        private void SINnerGroupSearch_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                Task.Run(() => this.DoThreadSafe(() => bSearch_Click(this, e)));
            }
        }

        private void BViewGroup_Click(object sender, EventArgs e)
        {
            try
            {
                var group = new SINnerGroup
                {
                    Groupname = tbSearchGroupname.Text,
                    IsPublic = false
                };
                if (MyCE?.MySINnerFile.MyGroup != null
                    && (string.IsNullOrEmpty(tbSearchGroupname.Text)
                        || tbSearchGroupname.Text == MyCE?.MySINnerFile.MyGroup?.Groupname))
                {
                    group = MyCE?.MySINnerFile.MyGroup;
                }

                if (tvGroupSearchResult.SelectedNode != null)
                {
                    if (tvGroupSearchResult.SelectedNode.Tag is SINnerSearchGroup sel)
                    {
                        group = new SINnerGroup(sel);
                    }
                }

                using (frmSINnerGroupEdit ge = new frmSINnerGroupEdit(group, false))
                {
                    var result = ge.ShowDialog(this);
                    if (result == DialogResult.OK)
                    {
                        group = ge.MySINnerGroupCreate.MyGroup;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Program.MainForm.ShowMessageBox(ex.ToString());
            }
        }

        private async void bCreateGroup_Click(object sender, EventArgs e)
        {
            try
            {
                var group = new SINnerGroup
                {
                    Groupname = tbSearchGroupname.Text,
                    IsPublic = false
                };
                //if ((MyCE?.MySINnerFile.MyGroup != null)
                //    && ((String.IsNullOrEmpty(tbSearchGroupname.Text))
                //        || (tbSearchGroupname.Text == MyCE?.MySINnerFile.MyGroup?.Groupname)))
                //{
                //    group = MyCE?.MySINnerFile.MyGroup;
                //}

                if (tvGroupSearchResult.SelectedNode?.Tag is SINnerSearchGroup sel)
                {
                    group = new SINnerGroup(sel);
                }

                using (frmSINnerGroupEdit ge = new frmSINnerGroupEdit(group, false))
                {
                    var result = ge.ShowDialog(this);
                    if (result == DialogResult.OK)
                    {
                        group = ge.MySINnerGroupCreate.MyGroup;
                        try
                        {
                            using (new CursorWait(this))
                            {
                                var a = await CreateGroup(ge.MySINnerGroupCreate.MyGroup).ConfigureAwait(true);
                                if (a != null)
                                {
                                    MySINSearchGroupResult = await SearchForGroups(a.Groupname).ConfigureAwait(true);
                                    if (MyParentForm?.MyParentForm != null)
                                        await (MyParentForm?.MyParentForm?.CheckSINnerStatus()).ConfigureAwait(true);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            Program.MainForm.ShowMessageBox(exception.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Program.MainForm.ShowMessageBox(ex.ToString());
            }

        }

        private void CbShowMembers_CheckedChanged(object sender, EventArgs e)
        {
            var setMeAgain = MySINSearchGroupResult;
            MySINSearchGroupResult = setMeAgain;
        }
    }
}
