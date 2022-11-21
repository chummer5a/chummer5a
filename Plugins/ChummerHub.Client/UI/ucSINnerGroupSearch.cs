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
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChummerHub.Client.Backend;
using System.Text;
using System.Threading;
using Chummer;
using ChummerHub.Client.Sinners;
using Chummer.Plugins;
using Point = System.Drawing.Point;
using NLog;

namespace ChummerHub.Client.UI
{
    public partial class ucSINnerGroupSearch : UserControl
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        public CharacterExtended MyCE { get; set; }

        public EventHandler<SINnerGroup> OnGroupJoinCallback { get; set; }

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
            List<TreeNode> res = new List<TreeNode>();
            if (sinGroups == null)
                return res;

            List<ListItem> lstLanguages = LanguageManager.GetSheetLanguageList(null, true);
            try
            {
                foreach (SINnerSearchGroup ssg in sinGroups)
                {
                    TreeNode tn = new TreeNode(ssg.GroupDisplayname);
                    if (string.IsNullOrEmpty(ssg.Language))
                        ssg.Language = "en-us";
                    int index = lstLanguages.FindIndex(c => c.Value?.ToString() == ssg.Language);
                    if (index != -1)
                    {
                        tn.ImageIndex = index;
                        tn.SelectedImageIndex = index;
                    }

                    tn.Tag = ssg;
                    tn.ToolTipText = ssg.Description;
                    if (cbShowMembers.Checked)
                    {
                        foreach (SINnerSearchGroupMember member in ssg.MyMembers)
                        {
                            TreeNode tnm = new TreeNode(member.MySINner.Alias)
                            {
                                Tag = member,
                                ImageIndex = tvGroupSearchResult.ImageList.Images.Count - 1,
                                SelectedImageIndex = tvGroupSearchResult.ImageList.Images.Count - 1
                            };
                            if (!string.IsNullOrEmpty(member.MySINner?.Language))
                            {
                                int mindex = lstLanguages.FindIndex(
                                    c => c.Value?.ToString() == member.MySINner?.Language);
                                if (mindex != -1)
                                {
                                    tnm.ImageIndex = mindex;
                                    tnm.SelectedImageIndex = mindex;
                                }
                            }

                            tn.Nodes.Add(tnm);
                        }
                    }

                    List<TreeNode> nodes = CreateTreeViewNodes(ssg.MySINSearchGroups);
                    tn.Nodes.AddRange(nodes.ToArray());
                    res.Add(tn);
                }
            }
            finally
            {
                Chummer.Utils.ListItemListPool.Return(ref lstLanguages);
            }

            return res;
        }

        public frmSINnerGroupSearch MyParentForm { get; internal set; }

        public ucSINnerGroupSearch()
        {
            InitializeComponent();
            ImageList myCountryImageList = new ImageList();
            List<ListItem> lstLanguages = LanguageManager.GetSheetLanguageList(null, true);
            try
            {
                foreach (ListItem lang in lstLanguages)
                {
                    Image img = FlagImageGetter.GetFlagFromCountryCode(lang.Value.ToString().Substring(3, 2));
                    myCountryImageList.Images.Add(img);
                }
            }
            finally
            {
                Chummer.Utils.ListItemListPool.Return(ref lstLanguages);
            }
            Image empty = FlagImageGetter.GetFlagFromCountryCode("noimagedots");
            myCountryImageList.Images.Add(empty);
            tvGroupSearchResult.ImageList = myCountryImageList;
            bCreateGroup.Enabled = true;
            bJoinGroup.Enabled = false;
        }



        private async void bSearch_Click(object sender, EventArgs e)
        {
            await DoSearch(sender);
        }

        private async ValueTask DoSearch(object sender = null, CancellationToken token = default)
        {
            try
            {
                using (await CursorWait.NewAsync(this, true, token))
                {
                    await tvGroupSearchResult.DoThreadSafeAsync(x => x.SelectedNode = null, token);
                    await bSearch.DoThreadSafeAsync(x => x.Text = "searching", token);
                    MySINSearchGroupResult = await SearchForGroups(tbSearchGroupname.Text, token);
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex);
                Program.ShowMessageBox(ex.Message);
            }
            finally
            {
                bSearch.Text = "Search";
                TvGroupSearchResult_AfterSelect(sender, new TreeViewEventArgs(new TreeNode()));
            }
        }

        public static async Task<SINSearchGroupResult> SearchForGroups(string groupname, CancellationToken token = default)
        {
            try
            {
                SINSearchGroupResult a = await SearchForGroupsTask(groupname, token);
                return a;
            }
            catch(Exception e)
            {
                Log.Error(e);
                throw;
            }

        }

        private static async Task<SINSearchGroupResult> SearchForGroupsTask(string groupname, CancellationToken token = default)
        {
            try
            {
                SinnersClient client = StaticUtils.GetClient();
                ResultGroupGetSearchGroups res = await client.GetSearchGroupsAsync(groupname, null, null, null, token);
                if (!(await Backend.Utils.ShowErrorResponseFormAsync(res, token: token) is ResultGroupGetSearchGroups result))
                    return null;
                if (result.CallSuccess)
                {
                    return result.MySearchGroupResult;
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
                using (await CursorWait.NewAsync(this))
                {
                    SINnerSearchGroup item = tvGroupSearchResult.SelectedNode.Tag as SINnerSearchGroup;
                    if (MyCE.MySINnerFile.MyGroup != null)
                    {
                        await LeaveGroupTask(MyCE.MySINnerFile, MyCE.MySINnerFile.MyGroup, item != null);
                    }

                    if (item == null)
                        return;

                    //var uploadtask = MyCE.Upload();
                    //await uploadtask.ContinueWith(b =>
                    //{
                    using (await CursorWait.NewAsync(this))
                    {
                        Task<SINSearchGroupResult> task = JoinGroupTask(item, MyCE);
                        await task.ContinueWith(async a =>
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

                                Program.ShowMessageBox(msg.ToString());
                                return;
                            }

                            SINSearchGroupResult objResult = await a;
                            if (!string.IsNullOrEmpty(objResult?.ErrorText))
                            {
                                Log.Error(objResult.ErrorText);
                            }
                            else if (objResult == null)
                            {
                                MyCE.MySINnerFile.MyGroup = null;
                                string msg = "Char " + MyCE.MyCharacter.CharacterName + " did not join group " +
                                             item.Groupname +
                                             ".";
                                Log.Info(msg);
                                Program.ShowMessageBox(msg, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                await ProcessGroupSearchVisible();
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
                                Program.ShowMessageBox(msg, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                await ProcessGroupSearchVisible();
                                await PluginHandler.MainForm.CharacterRoster.RefreshPluginNodesAsync(PluginHandler.MyPluginHandlerInstance);
                            }
                        }).Unwrap();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }

        }

        private async Task LeaveGroupTask(SINner mySINnerFile, SINnerGroup myGroup, bool noupdate = false, CancellationToken token = default)
        {
            try
            {
                SinnersClient client = StaticUtils.GetClient();
                bool response = await client.DeleteLeaveGroupAsync(myGroup.Id, mySINnerFile.Id, token);
                if (response)
                {
                    try
                    {
                        MyCE = MyCE;
                        if (!noupdate)
                            await ProcessGroupSearchVisible(token: token);
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Group disbanded: " + e.Message);
                        Program.ShowMessageBox("Group " + myGroup.Groupname
                                                                 + " disbanded because of no members left.");
                    }
                    finally
                    {
                        if (!noupdate && MyParentForm?.MyParentForm != null)
                            await MyParentForm.MyParentForm.CheckSINnerStatus(token);
                    }
                }
                else
                {
                    string msg = "StatusCode: " + false + " after DeleteLeaveGroupAsync!" + Environment.NewLine;
                    Log.Info(msg);
                    Program.ShowMessageBox(msg);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                Program.ShowMessageBox(e.Message);
            }
            if (!noupdate)
                OnGroupJoinCallback?.Invoke(this, myGroup);
        }

        private async Task<SINSearchGroupResult> JoinGroupTask(SINnerSearchGroup searchgroup, CharacterExtended myCE, CancellationToken token = default)
        {
            bool exceptionlogged = false;
            SINSearchGroupResult ssgr = null;
            try
            {
                DialogResult result = DialogResult.Cancel;
                frmSINnerGroupEdit groupEdit = null;
                if (searchgroup.HasPassword)
                {
                    SINnerGroup joinGroup = new SINnerGroup(searchgroup);
                    await PluginHandler.MainForm.DoThreadSafeAsync(() =>
                    {
                        groupEdit = new frmSINnerGroupEdit(joinGroup, true);
                        result = groupEdit.ShowDialog(this);
                    }, token: token);
                }

                if (result == DialogResult.OK || !searchgroup.HasPassword)
                {
                    try
                    {
                        using (await CursorWait.NewAsync(this, true, token))
                        {
                            SinnersClient client = StaticUtils.GetClient();
                            SINner response =
                                await client.PutSINerInGroupAsync(searchgroup.Id, myCE.MySINnerFile.Id,
                                    groupEdit?.MySINnerGroupCreate?.MyGroup?.PasswordHash, token);
                            if (response == null)
                            {
                                throw new NotImplementedException();
                                //var rescontent = await response.Response.Content.ReadAsStringAsync();
                                //if (response.Response.StatusCode == HttpStatusCode.BadRequest)
                                //{
                                //    if (rescontent.Contains("PW is wrong!"))
                                //    {
                                //        throw new ArgumentException("Wrong Password provided!");
                                //    }

                                //    string searchfor = "NoUserRightException\",\"Message\":\"";
                                //    if (rescontent.Contains(searchfor))
                                //    {
                                //        string msg =
                                //            rescontent.Substring(rescontent.IndexOf(searchfor, StringComparison.Ordinal) + searchfor.Length);
                                //        msg = msg.Substring(0, msg.IndexOf("\"", StringComparison.Ordinal));
                                //        throw new ArgumentException(msg);
                                //    }

                                //    throw new ArgumentException(rescontent);
                                //}
                                //else
                                //{
                                //    string msg = "StatusCode: " + response.Response.StatusCode + Environment.NewLine;
                                //    msg += rescontent;
                                //    throw new ArgumentException(msg);
                                //}
                            }

                            ResultGroupGetGroupById found = await client.GetGroupByIdAsync(searchgroup.Id, token);
                            await Backend.Utils.ShowErrorResponseFormAsync(found, token: token);
                            if (found?.CallSuccess == true)
                            {
                                ssgr = new SINSearchGroupResult(found.MyGroup);
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
                        if (MyParentForm?.MyParentForm != null)
                            await MyParentForm.MyParentForm.CheckSINnerStatus(token);
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
            await ProcessGroupSearchVisible(sender);
        }

        private async ValueTask ProcessGroupSearchVisible(object sender = null, CancellationToken token = default)
        {
            if (!await this.DoThreadSafeFuncAsync(x => x.Visible, token: token))
                return;
            await lSINnerName.DoThreadSafeAsync(x => x.Text = "not set", token: token);
            await cbShowMembers.DoThreadSafeAsync(x => x.Checked = MyCE == null, token: token);
            if (MyCE == null)
                return;
            await lSINnerName.DoThreadSafeAsync(x => x.Text = MyCE.MySINnerFile.Alias, token: token);
            if (MyCE?.MySINnerFile.MyGroup != null)
            {
                using (await CursorWait.NewAsync(this, true, token))
                {
                    try
                    {
                        //MySINSearchGroupResult = null;
                        string strText = await tbSearchGroupname.DoThreadSafeFuncAsync(x => x.Text, token: token);
                        if (string.IsNullOrEmpty(strText))
                        {
                            SINSearchGroupResult temp = await this.DoThreadSafeFuncAsync(() => new SINSearchGroupResult(MyCE?.MySINnerFile.MyGroup), token: token);
                            MySINSearchGroupResult = temp;
                        }
                        else
                        {
                            MySINSearchGroupResult = await SearchForGroups(strText, token);
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        Log.Info(
                            "No group found with name: " + MyCE?.MySINnerFile.MyGroup.Groupname);
                        if (MyCE?.MySINnerFile != null)
                            MyCE.MySINnerFile.MyGroup = null;
                        if (MyParentForm?.MyParentForm != null)
                            await MyParentForm.MyParentForm.CheckSINnerStatus(token);
                    }
                    catch (Exception exception)
                    {
                        Log.Error(exception);
                        Program.ShowMessageBox(exception.Message);
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

        private async void TbSearchGroupname_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                await DoSearch(this);
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
                    else if ((MyCE.MySINnerFile.MyGroup.Id != item.Id
                              || MyCE.MySINnerFile.MyGroup.Groupname != tbSearchGroupname.Text)
                             && MyCE.MySINnerFile.MyGroup.Groupname != item.Groupname)
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
                    ICollection<SINnerSearchGroupMember> members = item.MyMembers;
                }
                else
                {
                    bJoinGroup.Enabled = false;
                }
            });
        }

        private async void BGroupFoundLoadInCharacterRoster_Click(object sender, EventArgs e)
        {
            SINnerSearchGroup item = await PluginHandler.MainForm.DoThreadSafeFuncAsync(() =>
            {
                TreeNode selectedNode = tvGroupSearchResult.SelectedNode;
                SINnerSearchGroup objInnerItem = selectedNode?.Tag as SINnerSearchGroup;
                while (objInnerItem == null && selectedNode?.Parent != null)
                {
                    selectedNode = selectedNode.Parent;
                    objInnerItem = selectedNode?.Tag as SINnerSearchGroup;
                }
                return objInnerItem;
            });
            if (item != null)
            {
                List<SINnerSearchGroup> list = new List<SINnerSearchGroup> {item};
                IEnumerable<TreeNode> nodelist = await PluginHandler.MainForm.DoThreadSafeFuncAsync(() => Backend.Utils.CharacterRosterTreeNodifyGroupList(list));
                foreach (TreeNode node in nodelist)
                {
                    await PluginHandler.MyTreeNodes2Add.AddOrUpdateAsync(node.Name, node, (key, oldValue) => node);
                }
                await PluginHandler.MainForm.CharacterRoster.RefreshPluginNodesAsync(PluginHandler.MyPluginHandlerInstance);
                await PluginHandler.MainForm.CharacterRoster.DoThreadSafeAsync(x => x.BringToFront());
                await MyParentForm.DoThreadSafeAsync(x => x.Close());
            }
        }

        private async void BGroupsFoundDeleteGroup_Click(object sender, EventArgs e)
        {
            if (tvGroupSearchResult.SelectedNode?.Tag is SINnerSearchGroup item)
            {
                try
                {
                    SinnersClient client = StaticUtils.GetClient();
                    bool response = await client.DeleteGroupAsync(item.Id).CancelAfter(1000 * 30);
                    if (response)
                    {
                        await DoSearch(sender);
                        Program.ShowMessageBox("Group deleted.");
                    }
                    else
                    {

                        string msg = "StatusCode: " + false + " DeleteGroupAsync" + Environment.NewLine;

                        throw new ArgumentNullException(item.Groupname, msg);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                    Program.ShowMessageBox(exception.ToString(), "Error deleting Group", MessageBoxButtons.OK,
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

                    SinnersClient client = StaticUtils.GetClient();
                    if (draggedNode.Tag is SINnerSearchGroup draggedGroup)
                    {

                        ResultGroupPutGroupInGroup res = await client.PutGroupInGroupAsync(draggedGroup.Id,
                            draggedGroup.Groupname, targetGroup?.Id,
                            draggedGroup.MyAdminIdentityRole, draggedGroup.IsPublic);
                        if (res != null)
                        {
                            MoveNode(draggedNode, targetNode);
                        }
                    }
                    if (draggedSINner?.MySINner?.MyGroup != null && targetGroup != null)
                    {
                        if (targetGroup.Groupname == "My SINners")
                        {
                            bool res = await client.DeleteLeaveGroupAsync(draggedSINner.MySINner.MyGroup.Id, draggedSINner.MySINner.Id);
                            await Backend.Utils.ShowErrorResponseFormAsync(res);
                            if (res)
                            {
                                await DoSearch(sender);
                            }
                        }
                        if (draggedSINner.MySINner.MyGroup?.Id == targetGroup.Id)
                            return;
                        if (draggedSINner.MySINner.MyGroup == null && targetGroup.Id == null)
                            return;

                        try
                        {
                            SINner res = await client.PutSINerInGroupAsync(targetGroup.Id, draggedSINner.MySINner.Id, null);
                            await Backend.Utils.ShowErrorResponseFormAsync(res);
                            if (res != null)
                            {
                                await DoSearch(sender);
                            }
                        }
                        catch (Exception exception)
                        {
                            Backend.Utils.HandleError(exception);
                        }
                    }

                    if (draggedSINner?.MySINner?.MyGroup?.Id != null && targetNode == null)
                    {
                        bool res = await client.DeleteLeaveGroupAsync(
                            draggedSINner.MySINner.MyGroup.Id, draggedSINner.MySINner.Id);
                        await Backend.Utils.ShowErrorResponseFormAsync(res);
                        MoveNode(draggedNode, targetNode);
                    }
                    else if (draggedSINner?.MySINner != null && targetNode == null)
                    {
                        SINner res = await client.PutSINerInGroupAsync(null, draggedSINner.MySINner.Id, null);
                        await Backend.Utils.ShowErrorResponseFormAsync(res);
                        if (res != null)
                        {
                            MoveNode(draggedNode, targetNode);
                        }
                    }
                    else if (draggedSINner?.MySINner != null && targetGroup != null)
                    {
                        SINner res = await client.PutSINerInGroupAsync(targetGroup.Id, draggedSINner.MySINner.Id, null);
                        await Backend.Utils.ShowErrorResponseFormAsync(res);
                        if (res != null)
                        {
                            MoveNode(draggedNode, targetNode);
                        }
                    }
                }
            }

            // Optional: Select the dropped node and navigate (however you do it)
            tvGroupSearchResult.SelectedNode = draggedNode;
            // NavigateToContent(draggedNode.Tag);

        }

        private static void MoveNode(TreeNode draggedNode, TreeNode targetNode)
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


        private async void SINnerGroupSearch_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                await DoSearch(this);
            }
        }

        private void BViewGroup_Click(object sender, EventArgs e)
        {
            try
            {
                SINnerGroup group = new SINnerGroup (null)
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

                if (tvGroupSearchResult.SelectedNode?.Tag is SINnerSearchGroup sel)
                {
                    @group = new SINnerGroup(sel);
                }

                using (frmSINnerGroupEdit ge = new frmSINnerGroupEdit(group, false))
                {
                    DialogResult result = ge.ShowDialog(this);
                    if (result == DialogResult.OK)
                    {
                        group = ge.MySINnerGroupCreate.MyGroup;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Program.ShowMessageBox(ex.ToString());
            }
        }

        private async void bCreateGroup_Click(object sender, EventArgs e)
        {
            SINnerGroup group = null;
            if (tvGroupSearchResult.SelectedNode?.Tag is SINnerSearchGroup sel)
            {
                group = new SINnerGroup(sel);
            }
            group = await Backend.Utils.CreateGroupOnClickAsync(group, tbSearchGroupname.Text);

            if (group != null)
            {
                MySINSearchGroupResult = await SearchForGroups(group.Groupname);
                if (MyParentForm?.MyParentForm != null)
                    await MyParentForm.MyParentForm.CheckSINnerStatus();
            }
        }

        private void CbShowMembers_CheckedChanged(object sender, EventArgs e)
        {
            SINSearchGroupResult setMeAgain = MySINSearchGroupResult;
            MySINSearchGroupResult = setMeAgain;
        }
    }
}
