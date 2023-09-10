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
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer.UI.Shared
{
    public partial class LimitTabUserControl : UserControl
    {
        private Character _objCharacter;

        public event EventHandler MakeDirty;

        public event EventHandler MakeDirtyWithCharacterUpdate;

        public LimitTabUserControl()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            foreach (ToolStripMenuItem tssItem in cmsLimitModifier.Items.OfType<ToolStripMenuItem>())
            {
                tssItem.UpdateLightDarkMode();
                tssItem.TranslateToolStripItemsRecursively();
            }
            foreach (ToolStripMenuItem tssItem in cmsLimitModifierNotesOnly.Items.OfType<ToolStripMenuItem>())
            {
                tssItem.UpdateLightDarkMode();
                tssItem.TranslateToolStripItemsRecursively();
            }
        }

        private async void LimitTabUserControl_Load(object sender, EventArgs e)
        {
            if (_objCharacter != null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                await RealLoad().ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task RealLoad(CancellationToken token = default)
        {
            if (ParentForm is CharacterShared frmParent && frmParent.CharacterObject != null)
            {
                if (Interlocked.CompareExchange(ref _objCharacter, frmParent.CharacterObject, null) != null)
                    return;
            }
            else
            {
                Character objCharacter = new Character();
                if (Interlocked.CompareExchange(ref _objCharacter, objCharacter, null) != null)
                {
                    await objCharacter.DisposeAsync().ConfigureAwait(false);
                    return;
                }
                await this.DoThreadSafeAsync(x => x.Disposed += (sender, args) => objCharacter.Dispose(), token).ConfigureAwait(false);
                Utils.BreakIfDebug();
            }

            if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                return;

            await lblPhysical.DoOneWayDataBindingAsync("Text", _objCharacter, nameof(Character.LimitPhysical), token)
                             .ConfigureAwait(false);
            await lblPhysical
                  .DoOneWayDataBindingAsync("ToolTipText", _objCharacter, nameof(Character.LimitPhysicalToolTip), token)
                  .ConfigureAwait(false);
            await lblMental.DoOneWayDataBindingAsync("Text", _objCharacter, nameof(Character.LimitMental), token)
                           .ConfigureAwait(false);
            await lblMental
                  .DoOneWayDataBindingAsync("ToolTipText", _objCharacter, nameof(Character.LimitMentalToolTip), token)
                  .ConfigureAwait(false);
            await lblSocial.DoOneWayDataBindingAsync("Text", _objCharacter, nameof(Character.LimitSocial), token)
                           .ConfigureAwait(false);
            await lblSocial
                  .DoOneWayDataBindingAsync("ToolTipText", _objCharacter, nameof(Character.LimitSocialToolTip), token)
                  .ConfigureAwait(false);
            await lblAstral.DoOneWayDataBindingAsync("Text", _objCharacter, nameof(Character.LimitAstral), token)
                           .ConfigureAwait(false);
            await lblAstral
                  .DoOneWayDataBindingAsync("ToolTipText", _objCharacter, nameof(Character.LimitAstralToolTip), token)
                  .ConfigureAwait(false);

            _objCharacter.LimitModifiers.CollectionChanged += LimitModifierCollectionChanged;
            await RefreshLimitModifiers(token: token).ConfigureAwait(false);
        }

        #region Click Events

        private async void cmdAddLimitModifier_Click(object sender, EventArgs e)
        {
            using (ThreadSafeForm<SelectLimitModifier> frmPickLimitModifier =
                   await ThreadSafeForm<SelectLimitModifier>.GetAsync(() =>
                                                                          new SelectLimitModifier(null, "Physical", "Mental", "Social")).ConfigureAwait(false))
            {
                if (await frmPickLimitModifier.ShowDialogSafeAsync(_objCharacter).ConfigureAwait(false) == DialogResult.Cancel)
                    return;

                // Create the new limit modifier.
                LimitModifier objLimitModifier = new LimitModifier(_objCharacter);
                objLimitModifier.Create(frmPickLimitModifier.MyForm.SelectedName,
                    frmPickLimitModifier.MyForm.SelectedBonus, frmPickLimitModifier.MyForm.SelectedLimitType,
                    frmPickLimitModifier.MyForm.SelectedCondition, true);
                if (objLimitModifier.InternalId.IsEmptyGuid())
                    return;

                await _objCharacter.LimitModifiers.AddAsync(objLimitModifier).ConfigureAwait(false);
            }

            MakeDirtyWithCharacterUpdate?.Invoke(this, EventArgs.Empty);
        }

        private void cmdDeleteLimitModifier_Click(object sender, EventArgs e)
        {
            if (!(treLimit.SelectedNode?.Tag is ICanRemove selectedObject))
                return;
            if (!selectedObject.Remove())
                return;
            MakeDirtyWithCharacterUpdate?.Invoke(this, EventArgs.Empty);
        }

        private void treLimit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteLimitModifier_Click(sender, e);
            }
        }

        private async void tssLimitModifierNotes_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = await treLimit.DoThreadSafeFuncAsync(x => x.SelectedNode).ConfigureAwait(false);
            object objSelectedNodeTag = objSelectedNode?.Tag;
            switch (objSelectedNodeTag)
            {
                case null:
                    return;
                case IHasNotes objNotes:
                    await WriteNotes(objNotes, objSelectedNode).ConfigureAwait(false);
                    break;
                default:
                {
                        // the limit modifier has a source
                        await _objCharacter.Improvements.ForEachAsync(async objImprovement =>
                        {
                            if (objImprovement.ImproveType != Improvement.ImprovementType.LimitModifier ||
                                objImprovement.SourceName != objSelectedNodeTag.ToString())
                                return;
                            using (ThreadSafeForm<EditNotes> frmItemNotes = await ThreadSafeForm<EditNotes>
                                       .GetAsync(() => new EditNotes(
                                                     objImprovement.Notes,
                                                     objImprovement.NotesColor))
                                       .ConfigureAwait(false))
                            {
                                if (await frmItemNotes.ShowDialogSafeAsync(_objCharacter).ConfigureAwait(false)
                                    != DialogResult.OK)
                                    return;

                                objImprovement.Notes = frmItemNotes.MyForm.Notes;
                            }

                            string strTooltip = objImprovement.Notes.WordWrap();
                            await treLimit.DoThreadSafeAsync(() =>
                            {
                                objSelectedNode.ForeColor = objImprovement.PreferredColor;
                                objSelectedNode.ToolTipText = strTooltip;
                            }).ConfigureAwait(false);
                            MakeDirty?.Invoke(this, EventArgs.Empty);
                        }).ConfigureAwait(false);

                    break;
                }
            }
        }

        private async void tssLimitModifierEdit_Click(object sender, EventArgs e)
        {
            await UpdateLimitModifier().ConfigureAwait(false);
        }

        #endregion Click Events

        #region Methods

        /// <summary>
        /// Allows the user to input notes that should be linked to the selected object.
        /// TODO: Should be linked back to CharacterShared in some way or moved into a more generic helper class.
        /// </summary>
        /// <param name="objNotes"></param>
        /// <param name="treNode"></param>
        /// <param name="token"></param>
        private async ValueTask WriteNotes(IHasNotes objNotes, TreeNode treNode, CancellationToken token = default)
        {
            using (ThreadSafeForm<EditNotes> frmItemNotes = await ThreadSafeForm<EditNotes>.GetAsync(() => new EditNotes(objNotes.Notes, objNotes.NotesColor), token).ConfigureAwait(false))
            {
                if (await frmItemNotes.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) != DialogResult.OK)
                    return;

                objNotes.Notes = frmItemNotes.MyForm.Notes;
                objNotes.NotesColor = frmItemNotes.MyForm.NotesColor;
            }
            TreeView objTreeView = treNode.TreeView;
            if (objTreeView != null)
                await objTreeView.DoThreadSafeAsync(() =>
                {
                    treNode.ForeColor = objNotes.PreferredColor;
                    treNode.ToolTipText = objNotes.Notes.WordWrap();
                }, token: token).ConfigureAwait(false);
            else
            {
                treNode.ForeColor = objNotes.PreferredColor;
                treNode.ToolTipText = objNotes.Notes.WordWrap();
            }
            MakeDirty?.Invoke(this, EventArgs.Empty);
        }

        private async Task RefreshLimitModifiers(NotifyCollectionChangedEventArgs e = null, CancellationToken token = default)
        {
            string strSelectedId = (await treLimit.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag, token: token).ConfigureAwait(false) as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode[] aobjLimitNodes = new TreeNode[(int)LimitType.NumLimitTypes];

            if (e == null)
            {
                await treLimit.DoThreadSafeAsync(x => x.Nodes.Clear(), token: token).ConfigureAwait(false);

                // Add Limit Modifiers.
                await _objCharacter.LimitModifiers.ForEachAsync(async objLimitModifier =>
                {
                    int intTargetLimit = (int)Enum.Parse(typeof(LimitType), objLimitModifier.Limit);
                    TreeNode objParentNode = await GetLimitModifierParentNode(intTargetLimit).ConfigureAwait(false);
                    string strKey = await objLimitModifier.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                    await treLimit.DoThreadSafeAsync(() =>
                    {
                        if (!objParentNode.Nodes.ContainsKey(strKey))
                        {
                            objParentNode.Nodes.Add(objLimitModifier.CreateTreeNode(
                                                        objLimitModifier.CanDelete
                                                            ? cmsLimitModifier
                                                            : cmsLimitModifierNotesOnly));
                        }
                    }, token: token).ConfigureAwait(false);
                }, token).ConfigureAwait(false);

                // Add Limit Modifiers from Improvements
                await _objCharacter.Improvements.ForEachAsync(async objImprovement =>
                {
                    if (objImprovement.ImproveSource != Improvement.ImprovementSource.Custom)
                        return;
                    int intTargetLimit = -1;
                    switch (objImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.LimitModifier:
                            intTargetLimit = (int) Enum.Parse(typeof(LimitType), objImprovement.ImprovedName);
                            break;

                        case Improvement.ImprovementType.PhysicalLimit:
                            intTargetLimit = (int) LimitType.Physical;
                            break;

                        case Improvement.ImprovementType.MentalLimit:
                            intTargetLimit = (int) LimitType.Mental;
                            break;

                        case Improvement.ImprovementType.SocialLimit:
                            intTargetLimit = (int) LimitType.Social;
                            break;
                    }

                    if (intTargetLimit != -1)
                    {
                        TreeNode objParentNode = await GetLimitModifierParentNode(intTargetLimit).ConfigureAwait(false);
                        string strName = objImprovement.UniqueName
                                         + await LanguageManager.GetStringAsync("String_Colon", token: token)
                                                                .ConfigureAwait(false)
                                         + await LanguageManager.GetStringAsync("String_Space", token: token)
                                                                .ConfigureAwait(false);
                        if (objImprovement.Value > 0) strName += '+';
                        strName += objImprovement.Value.ToString(GlobalSettings.CultureInfo);
                        if (!string.IsNullOrEmpty(objImprovement.Condition))
                            strName += ','
                                       + await LanguageManager.GetStringAsync("String_Space", token: token)
                                                              .ConfigureAwait(false) + objImprovement.Condition;
                        await treLimit.DoThreadSafeAsync(() =>
                                      {
                                          if (!objParentNode.Nodes.ContainsKey(strName))
                                          {
                                              TreeNode objNode = new TreeNode
                                              {
                                                  Name = strName,
                                                  Text = strName,
                                                  Tag = objImprovement.SourceName,
                                                  ContextMenuStrip = cmsLimitModifierNotesOnly,
                                                  ForeColor = objImprovement.PreferredColor,
                                                  ToolTipText = objImprovement.Notes.WordWrap()
                                              };
                                              if (string.IsNullOrEmpty(objImprovement.ImprovedName))
                                              {
                                                  switch (objImprovement.ImproveType)
                                                  {
                                                      case Improvement.ImprovementType.SocialLimit:
                                                          objImprovement.ImprovedName = "Social";
                                                          break;

                                                      case Improvement.ImprovementType.MentalLimit:
                                                          objImprovement.ImprovedName = "Mental";
                                                          break;

                                                      default:
                                                          objImprovement.ImprovedName = "Physical";
                                                          break;
                                                  }
                                              }

                                              objParentNode.Nodes.Add(objNode);
                                          }
                                      }, token: token)
                                      .ConfigureAwait(false);
                    }
                }, token).ConfigureAwait(false);

                await treLimit.DoThreadSafeAsync(x => x.SortCustomAlphabetically(strSelectedId), token: token).ConfigureAwait(false);
            }
            else
            {
                await treLimit.DoThreadSafeAsync(x =>
                {
                    aobjLimitNodes[0] = x.FindNode("Node_Physical", false);
                    aobjLimitNodes[1] = x.FindNode("Node_Mental", false);
                    aobjLimitNodes[2] = x.FindNode("Node_Social", false);
                    aobjLimitNodes[3] = x.FindNode("Node_Astral", false);
                }, token: token).ConfigureAwait(false);

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (LimitModifier objLimitModifier in e.NewItems)
                            {
                                int intTargetLimit = (int)Enum.Parse(typeof(LimitType), objLimitModifier.Limit);
                                TreeNode objParentNode = await GetLimitModifierParentNode(intTargetLimit).ConfigureAwait(false);
                                string strKey = await objLimitModifier.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                                await treLimit.DoThreadSafeAsync(x =>
                                {
                                    TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                                    if (!lstParentNodeChildren.ContainsKey(strKey))
                                    {
                                        TreeNode objNode = objLimitModifier.CreateTreeNode(
                                            objLimitModifier.CanDelete ? cmsLimitModifier : cmsLimitModifierNotesOnly);
                                        int intNodesCount = lstParentNodeChildren.Count;
                                        int intTargetIndex = 0;
                                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                                        {
                                            if (CompareTreeNodes.CompareText(
                                                    lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                                            {
                                                break;
                                            }
                                        }

                                        lstParentNodeChildren.Insert(intTargetIndex, objNode);
                                        objParentNode.Expand();
                                        x.SelectedNode = objNode;
                                    }
                                }, token: token).ConfigureAwait(false);
                            }

                            break;
                        }

                    case NotifyCollectionChangedAction.Remove:
                        {
                            await treLimit.DoThreadSafeAsync(x =>
                            {
                                foreach (LimitModifier objLimitModifier in e.OldItems)
                                {
                                    TreeNode objNode = x.FindNodeByTag(objLimitModifier);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                            objParent.Remove();
                                    }
                                }
                            }, token: token).ConfigureAwait(false);
                            break;
                        }

                    case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParentNodes = new List<TreeNode>(e.OldItems.Count);
                            await treLimit.DoThreadSafeAsync(x =>
                            {
                                foreach (LimitModifier objLimitModifier in e.OldItems)
                                {
                                    TreeNode objNode = x.FindNodeByTag(objLimitModifier);
                                    if (objNode != null)
                                    {
                                        lstOldParentNodes.Add(objNode.Parent);
                                        objNode.Remove();
                                    }
                                }
                            }, token: token).ConfigureAwait(false);
                            foreach (LimitModifier objLimitModifier in e.NewItems)
                            {
                                int intTargetLimit = (int)Enum.Parse(typeof(LimitType), objLimitModifier.Limit);
                                TreeNode objParentNode = await GetLimitModifierParentNode(intTargetLimit).ConfigureAwait(false);
                                string strKey = await objLimitModifier.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                                await treLimit.DoThreadSafeAsync(x =>
                                {
                                    TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                                    if (!lstParentNodeChildren.ContainsKey(strKey))
                                    {
                                        TreeNode objNode = objLimitModifier.CreateTreeNode(
                                            objLimitModifier.CanDelete ? cmsLimitModifier : cmsLimitModifierNotesOnly);
                                        int intNodesCount = lstParentNodeChildren.Count;
                                        int intTargetIndex = 0;
                                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                                        {
                                            if (CompareTreeNodes.CompareText(
                                                    lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                                            {
                                                break;
                                            }
                                        }

                                        lstParentNodeChildren.Insert(intTargetIndex, objNode);
                                        objParentNode.Expand();
                                        x.SelectedNode = objNode;
                                    }
                                }, token: token).ConfigureAwait(false);
                            }

                            await treLimit.DoThreadSafeAsync(() =>
                            {
                                foreach (TreeNode objOldParentNode in lstOldParentNodes)
                                {
                                    if (objOldParentNode.Level == 0 && objOldParentNode.Nodes.Count == 0)
                                        objOldParentNode.Remove();
                                }
                            }, token: token).ConfigureAwait(false);
                            break;
                        }

                    case NotifyCollectionChangedAction.Reset:
                        {
                            await RefreshLimitModifiers(token: token).ConfigureAwait(false);
                            break;
                        }
                }
            }

            async ValueTask<TreeNode> GetLimitModifierParentNode(int intTargetLimit)
            {
                TreeNode objParentNode = aobjLimitNodes[intTargetLimit];
                if (objParentNode == null)
                {
                    switch (intTargetLimit)
                    {
                        case 0:
                            objParentNode = new TreeNode
                            {
                                Tag = "Node_Physical",
                                Text = await LanguageManager.GetStringAsync("Node_Physical", token: token).ConfigureAwait(false)
                            };
                            await treLimit.DoThreadSafeAsync(x => x.Nodes.Insert(0, objParentNode), token: token).ConfigureAwait(false);
                            break;

                        case 1:
                            objParentNode = new TreeNode
                            {
                                Tag = "Node_Mental",
                                Text = await LanguageManager.GetStringAsync("Node_Mental", token: token).ConfigureAwait(false)
                            };
                            await treLimit.DoThreadSafeAsync(x => x.Nodes.Insert(aobjLimitNodes[0] == null ? 0 : 1, objParentNode), token: token).ConfigureAwait(false);
                            break;

                        case 2:
                            objParentNode = new TreeNode
                            {
                                Tag = "Node_Social",
                                Text = await LanguageManager.GetStringAsync("Node_Social", token: token).ConfigureAwait(false)
                            };
                            await treLimit.DoThreadSafeAsync(x => x.Nodes.Insert((aobjLimitNodes[0] == null ? 0 : 1) + (aobjLimitNodes[1] == null ? 0 : 1), objParentNode), token: token).ConfigureAwait(false);
                            break;

                        case 3:
                            objParentNode = new TreeNode
                            {
                                Tag = "Node_Astral",
                                Text = await LanguageManager.GetStringAsync("Node_Astral", token: token).ConfigureAwait(false)
                            };
                            await treLimit.DoThreadSafeAsync(x => x.Nodes.Add(objParentNode), token: token).ConfigureAwait(false);
                            break;
                    }

                    aobjLimitNodes[intTargetLimit] = objParentNode;
                    if (objParentNode != null)
                        await treLimit.DoThreadSafeAsync(() => objParentNode.Expand(), token: token).ConfigureAwait(false);
                }
                return objParentNode;
            }
        }

        /// <summary>
        /// Edit and update a Limit Modifier.
        /// </summary>
        protected async ValueTask UpdateLimitModifier(CancellationToken token = default)
        {
            TreeNode objSelectedNode = await treLimit.DoThreadSafeFuncAsync(x => x.SelectedNode, token: token).ConfigureAwait(false);
            if (objSelectedNode == null || objSelectedNode.Level <= 0)
                return;
            string strGuid = (objSelectedNode.Tag as IHasInternalId)?.InternalId ?? string.Empty;
            if (string.IsNullOrEmpty(strGuid) || strGuid.IsEmptyGuid())
                return;
            LimitModifier objLimitModifier = _objCharacter.LimitModifiers.FindById(strGuid);
            //If the LimitModifier couldn't be found (Ie it comes from an Improvement or the user hasn't properly selected a treenode, fail out early.
            if (objLimitModifier == null)
            {
                Program.ShowScrollableMessageBox(await LanguageManager.GetStringAsync("Warning_NoLimitFound", token: token).ConfigureAwait(false));
                return;
            }

            using (ThreadSafeForm<SelectLimitModifier> frmPickLimitModifier =
                   await ThreadSafeForm<SelectLimitModifier>.GetAsync(() =>
                                                                          new SelectLimitModifier(objLimitModifier, "Physical", "Mental", "Social"), token).ConfigureAwait(false))
            {
                if (await frmPickLimitModifier.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    return;

                //Remove the old LimitModifier to ensure we don't double up.
                await _objCharacter.LimitModifiers.RemoveAsync(objLimitModifier, token).ConfigureAwait(false);
                // Create the new limit modifier.
                LimitModifier objNewLimitModifier = new LimitModifier(_objCharacter, strGuid);
                objNewLimitModifier.Create(frmPickLimitModifier.MyForm.SelectedName,
                                           frmPickLimitModifier.MyForm.SelectedBonus, frmPickLimitModifier.MyForm.SelectedLimitType,
                                           frmPickLimitModifier.MyForm.SelectedCondition, true);

                await _objCharacter.LimitModifiers.AddAsync(objNewLimitModifier, token).ConfigureAwait(false);
            }

            MakeDirtyWithCharacterUpdate?.Invoke(this, EventArgs.Empty);
        }

        private async void LimitModifierCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await RefreshLimitModifiers(e).ConfigureAwait(false);
        }

        #endregion Methods

        #region Properties

        public ContextMenuStrip LimitContextMenuStrip => cmsLimitModifierNotesOnly;
        public TreeView LimitTreeView => treLimit;

        #endregion Properties

        private void treLimit_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treLimit.SelectedNode?.Tag is LimitModifier objLimitModifier)
            {
                cmdDeleteLimitModifier.Enabled = objLimitModifier.CanDelete;
                tssLimitModifierEdit.Enabled = objLimitModifier.CanDelete;
            }
            else
            {
                cmdDeleteLimitModifier.Enabled = false;
            }
        }
    }
}
