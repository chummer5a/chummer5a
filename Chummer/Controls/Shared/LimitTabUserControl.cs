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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer.UI.Shared
{
    public partial class LimitTabUserControl : UserControl
    {
        private bool _blnDisposeCharacterOnDispose;
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
            using (await CursorWait.NewAsync(this))
                await RealLoad();
        }

        public Task RealLoad()
        {
            if (ParentForm is CharacterShared frmParent)
                _objCharacter = frmParent.CharacterObject;
            else
            {
                _blnDisposeCharacterOnDispose = true;
                _objCharacter = new Character();
                Utils.BreakIfDebug();
            }

            if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                return Task.CompletedTask;

            lblPhysical.DoOneWayDataBinding("Text", _objCharacter, nameof(Character.LimitPhysical));
            lblPhysical.DoOneWayDataBinding("ToolTipText", _objCharacter, nameof(Character.LimitPhysicalToolTip));
            lblMental.DoOneWayDataBinding("Text", _objCharacter, nameof(Character.LimitMental));
            lblMental.DoOneWayDataBinding("ToolTipText", _objCharacter, nameof(Character.LimitMentalToolTip));
            lblSocial.DoOneWayDataBinding("Text", _objCharacter, nameof(Character.LimitSocial));
            lblSocial.DoOneWayDataBinding("ToolTipText", _objCharacter, nameof(Character.LimitSocialToolTip));
            lblAstral.DoOneWayDataBinding("Text", _objCharacter, nameof(Character.LimitAstral));
            lblAstral.DoOneWayDataBinding("ToolTipText", _objCharacter, nameof(Character.LimitAstralToolTip));

            _objCharacter.LimitModifiers.CollectionChanged += LimitModifierCollectionChanged;
            return RefreshLimitModifiers();
        }

        #region Click Events

        private async void cmdAddLimitModifier_Click(object sender, EventArgs e)
        {
            using (ThreadSafeForm<SelectLimitModifier> frmPickLimitModifier =
                   await ThreadSafeForm<SelectLimitModifier>.GetAsync(() =>
                       new SelectLimitModifier(null, "Physical", "Mental", "Social")))
            {
                if (await frmPickLimitModifier.ShowDialogSafeAsync(_objCharacter) == DialogResult.Cancel)
                    return;

                // Create the new limit modifier.
                LimitModifier objLimitModifier = new LimitModifier(_objCharacter);
                objLimitModifier.Create(frmPickLimitModifier.MyForm.SelectedName,
                    frmPickLimitModifier.MyForm.SelectedBonus, frmPickLimitModifier.MyForm.SelectedLimitType,
                    frmPickLimitModifier.MyForm.SelectedCondition, true);
                if (objLimitModifier.InternalId.IsEmptyGuid())
                    return;

                _objCharacter.LimitModifiers.Add(objLimitModifier);
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
            object objSelectedNodeTag = await treLimit.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag);
            if (objSelectedNodeTag == null)
                return;
            if (objSelectedNodeTag is IHasNotes objNotes)
            {
                await WriteNotes(objNotes, await treLimit.DoThreadSafeFuncAsync(x => x.SelectedNode));
            }
            else
            {
                // the limit modifier has a source
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (objImprovement.ImproveType != Improvement.ImprovementType.LimitModifier ||
                        objImprovement.SourceName != objSelectedNodeTag.ToString())
                        continue;
                    using (ThreadSafeForm<EditNotes> frmItemNotes = await ThreadSafeForm<EditNotes>.GetAsync(() => new EditNotes(objImprovement.Notes, objImprovement.NotesColor)))
                    {
                        if (await frmItemNotes.ShowDialogSafeAsync(_objCharacter) != DialogResult.OK)
                            continue;

                        objImprovement.Notes = frmItemNotes.MyForm.Notes;
                    }
                    await treLimit.DoThreadSafeAsync(x =>
                    {
                        x.SelectedNode.ForeColor = objImprovement.PreferredColor;
                        x.SelectedNode.ToolTipText = objImprovement.Notes.WordWrap();
                    });
                    MakeDirty?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private async void tssLimitModifierEdit_Click(object sender, EventArgs e)
        {
            await UpdateLimitModifier();
        }

        #endregion Click Events

        #region Methods

        /// <summary>
        /// Allows the user to input notes that should be linked to the selected object.
        /// TODO: Should be linked back to CharacterShared in some way or moved into a more generic helper class.
        /// </summary>
        /// <param name="objNotes"></param>
        /// <param name="treNode"></param>
        private async ValueTask WriteNotes(IHasNotes objNotes, TreeNode treNode)
        {
            using (ThreadSafeForm<EditNotes> frmItemNotes = await ThreadSafeForm<EditNotes>.GetAsync(() => new EditNotes(objNotes.Notes, objNotes.NotesColor)))
            {
                if (await frmItemNotes.ShowDialogSafeAsync(_objCharacter) != DialogResult.OK)
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
                });
            else
            {
                treNode.ForeColor = objNotes.PreferredColor;
                treNode.ToolTipText = objNotes.Notes.WordWrap();
            }
            MakeDirty?.Invoke(this, EventArgs.Empty);
        }

        private async Task RefreshLimitModifiers(NotifyCollectionChangedEventArgs e = null)
        {
            string strSelectedId = (await treLimit.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag) as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode[] aobjLimitNodes = new TreeNode[(int)LimitType.NumLimitTypes];

            if (e == null)
            {
                await treLimit.DoThreadSafeAsync(x => x.Nodes.Clear());

                // Add Limit Modifiers.
                foreach (LimitModifier objLimitModifier in _objCharacter.LimitModifiers)
                {
                    int intTargetLimit = (int)Enum.Parse(typeof(LimitType), objLimitModifier.Limit);
                    TreeNode objParentNode = await GetLimitModifierParentNode(intTargetLimit);
                    await treLimit.DoThreadSafeAsync(() =>
                    {
                        if (!objParentNode.Nodes.ContainsKey(objLimitModifier.CurrentDisplayName))
                        {
                            objParentNode.Nodes.Add(objLimitModifier.CreateTreeNode(
                                                        objLimitModifier.CanDelete
                                                            ? cmsLimitModifier
                                                            : cmsLimitModifierNotesOnly));
                        }
                    });
                }

                // Add Limit Modifiers from Improvements
                foreach (Improvement objImprovement in _objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveSource == Improvement.ImprovementSource.Custom))
                {
                    int intTargetLimit = -1;
                    switch (objImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.LimitModifier:
                            intTargetLimit = (int)Enum.Parse(typeof(LimitType), objImprovement.ImprovedName);
                            break;

                        case Improvement.ImprovementType.PhysicalLimit:
                            intTargetLimit = (int)LimitType.Physical;
                            break;

                        case Improvement.ImprovementType.MentalLimit:
                            intTargetLimit = (int)LimitType.Mental;
                            break;

                        case Improvement.ImprovementType.SocialLimit:
                            intTargetLimit = (int)LimitType.Social;
                            break;
                    }

                    if (intTargetLimit != -1)
                    {
                        TreeNode objParentNode = await GetLimitModifierParentNode(intTargetLimit);
                        string strName = objImprovement.UniqueName
                                         + await LanguageManager.GetStringAsync("String_Colon")
                                         + await LanguageManager.GetStringAsync("String_Space");
                        if (objImprovement.Value > 0)
                            strName += '+';
                        strName += objImprovement.Value.ToString(GlobalSettings.CultureInfo);
                        if (!string.IsNullOrEmpty(objImprovement.Condition))
                            strName += ',' + await LanguageManager.GetStringAsync("String_Space")
                                           + objImprovement.Condition;
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
                        });
                    }
                }

                await treLimit.DoThreadSafeAsync(x => x.SortCustomAlphabetically(strSelectedId));
            }
            else
            {
                await treLimit.DoThreadSafeAsync(x =>
                {
                    aobjLimitNodes[0] = x.FindNode("Node_Physical", false);
                    aobjLimitNodes[1] = x.FindNode("Node_Mental", false);
                    aobjLimitNodes[2] = x.FindNode("Node_Social", false);
                    aobjLimitNodes[3] = x.FindNode("Node_Astral", false);
                });

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    {
                        foreach (LimitModifier objLimitModifier in e.NewItems)
                        {
                            int intTargetLimit = (int) Enum.Parse(typeof(LimitType), objLimitModifier.Limit);
                            TreeNode objParentNode = await GetLimitModifierParentNode(intTargetLimit);
                            await treLimit.DoThreadSafeAsync(x =>
                            {
                                TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                                if (!lstParentNodeChildren.ContainsKey(objLimitModifier.CurrentDisplayName))
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
                            });
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
                        });
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
                        });
                        foreach (LimitModifier objLimitModifier in e.NewItems)
                        {
                            int intTargetLimit = (int) Enum.Parse(typeof(LimitType), objLimitModifier.Limit);
                            TreeNode objParentNode = await GetLimitModifierParentNode(intTargetLimit);
                            await treLimit.DoThreadSafeAsync(x =>
                            {
                                TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                                if (!lstParentNodeChildren.ContainsKey(objLimitModifier.CurrentDisplayName))
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
                            });
                        }

                        await treLimit.DoThreadSafeAsync(() =>
                        {
                            foreach (TreeNode objOldParentNode in lstOldParentNodes)
                            {
                                if (objOldParentNode.Level == 0 && objOldParentNode.Nodes.Count == 0)
                                    objOldParentNode.Remove();
                            }
                        });
                        break;
                    }

                    case NotifyCollectionChangedAction.Reset:
                    {
                        await RefreshLimitModifiers();
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
                                Text = await LanguageManager.GetStringAsync("Node_Physical")
                            };
                            await treLimit.DoThreadSafeAsync(x => x.Nodes.Insert(0, objParentNode));
                            break;

                        case 1:
                            objParentNode = new TreeNode
                            {
                                Tag = "Node_Mental",
                                Text = await LanguageManager.GetStringAsync("Node_Mental")
                            };
                            await treLimit.DoThreadSafeAsync(x => x.Nodes.Insert(aobjLimitNodes[0] == null ? 0 : 1, objParentNode));
                            break;

                        case 2:
                            objParentNode = new TreeNode
                            {
                                Tag = "Node_Social",
                                Text = await LanguageManager.GetStringAsync("Node_Social")
                            };
                            await treLimit.DoThreadSafeAsync(x => x.Nodes.Insert((aobjLimitNodes[0] == null ? 0 : 1) + (aobjLimitNodes[1] == null ? 0 : 1), objParentNode));
                            break;

                        case 3:
                            objParentNode = new TreeNode
                            {
                                Tag = "Node_Astral",
                                Text = await LanguageManager.GetStringAsync("Node_Astral")
                            };
                            await treLimit.DoThreadSafeAsync(x => x.Nodes.Add(objParentNode));
                            break;
                    }

                    aobjLimitNodes[intTargetLimit] = objParentNode;
                    if (objParentNode != null)
                        await treLimit.DoThreadSafeAsync(() => objParentNode.Expand());
                }
                return objParentNode;
            }
        }

        /// <summary>
        /// Edit and update a Limit Modifier.
        /// </summary>
        protected async ValueTask UpdateLimitModifier()
        {
            TreeNode objSelectedNode = await treLimit.DoThreadSafeFuncAsync(x => x.SelectedNode);
            if (objSelectedNode == null || objSelectedNode.Level <= 0)
                return;
            string strGuid = (objSelectedNode.Tag as IHasInternalId)?.InternalId ?? string.Empty;
            if (string.IsNullOrEmpty(strGuid) || strGuid.IsEmptyGuid())
                return;
            LimitModifier objLimitModifier = _objCharacter.LimitModifiers.FindById(strGuid);
            //If the LimitModifier couldn't be found (Ie it comes from an Improvement or the user hasn't properly selected a treenode, fail out early.
            if (objLimitModifier == null)
            {
                Program.ShowMessageBox(await LanguageManager.GetStringAsync("Warning_NoLimitFound"));
                return;
            }

            using (ThreadSafeForm<SelectLimitModifier> frmPickLimitModifier =
                   await ThreadSafeForm<SelectLimitModifier>.GetAsync(() =>
                       new SelectLimitModifier(objLimitModifier, "Physical", "Mental", "Social")))
            {
                if (await frmPickLimitModifier.ShowDialogSafeAsync(_objCharacter) == DialogResult.Cancel)
                    return;

                //Remove the old LimitModifier to ensure we don't double up.
                _objCharacter.LimitModifiers.Remove(objLimitModifier);
                // Create the new limit modifier.
                objLimitModifier = new LimitModifier(_objCharacter, strGuid);
                objLimitModifier.Create(frmPickLimitModifier.MyForm.SelectedName,
                    frmPickLimitModifier.MyForm.SelectedBonus, frmPickLimitModifier.MyForm.SelectedLimitType,
                    frmPickLimitModifier.MyForm.SelectedCondition, true);

                _objCharacter.LimitModifiers.Add(objLimitModifier);
            }

            MakeDirtyWithCharacterUpdate?.Invoke(this, EventArgs.Empty);
        }

        private async void LimitModifierCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await RefreshLimitModifiers(e);
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
