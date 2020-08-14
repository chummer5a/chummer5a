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
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace Chummer.UI.Shared
{
    public partial class LimitTabUserControl : UserControl
    {
        private Character _objCharacter;
        public event PropertyChangedEventHandler MakeDirty;
        public event PropertyChangedEventHandler MakeDirtyWithCharacterUpdate;

        public LimitTabUserControl()
        {
            InitializeComponent();

            this.TranslateWinForm();

            foreach (ToolStripMenuItem tssItem in cmsLimitModifier.Items.OfType<ToolStripMenuItem>())
            {
                tssItem.TranslateToolStripItemsRecursively();
            }
            foreach (ToolStripMenuItem tssItem in cmsLimitModifierNotesOnly.Items.OfType<ToolStripMenuItem>())
            {
                tssItem.TranslateToolStripItemsRecursively();
            }
        }

        private void LimitTabUserControl_Load(object sender, EventArgs e)
        {
            if (_objCharacter != null)
                return;
            using (new CursorWait(this))
                RealLoad();
        }

        public void RealLoad()
        {
            if (ParentForm is CharacterShared frmParent)
                _objCharacter = frmParent.CharacterObject;
            else
            {
                Utils.BreakIfDebug();
                _objCharacter = new Character();
            }

            lblPhysical.DoOneWayDataBinding("Text", _objCharacter, nameof(Character.LimitPhysical));
            lblPhysical.DoOneWayDataBinding("ToolTipText", _objCharacter, nameof(Character.LimitPhysicalToolTip));
            lblMental.DoOneWayDataBinding("Text", _objCharacter, nameof(Character.LimitMental));
            lblMental.DoOneWayDataBinding("ToolTipText", _objCharacter, nameof(Character.LimitMentalToolTip));
            lblSocial.DoOneWayDataBinding("Text", _objCharacter, nameof(Character.LimitSocial));
            lblSocial.DoOneWayDataBinding("ToolTipText", _objCharacter, nameof(Character.LimitSocialToolTip));
            lblAstral.DoOneWayDataBinding("Text", _objCharacter, nameof(Character.LimitAstral));
            lblAstral.DoOneWayDataBinding("ToolTipText", _objCharacter, nameof(Character.LimitAstralToolTip));

            _objCharacter.LimitModifiers.CollectionChanged += LimitModifierCollectionChanged;
            RefreshLimitModifiers();
        }
        #region Click Events
        private void cmdAddLimitModifier_Click(object sender, EventArgs e)
        {
            using (frmSelectLimitModifier frmPickLimitModifier = new frmSelectLimitModifier(null, "Physical", "Mental", "Social"))
            {
                frmPickLimitModifier.ShowDialog(this);

                if (frmPickLimitModifier.DialogResult == DialogResult.Cancel)
                    return;

                // Create the new limit modifier.
                LimitModifier objLimitModifier = new LimitModifier(_objCharacter);
                objLimitModifier.Create(frmPickLimitModifier.SelectedName, frmPickLimitModifier.SelectedBonus, frmPickLimitModifier.SelectedLimitType, frmPickLimitModifier.SelectedCondition, true);
                if (objLimitModifier.InternalId.IsEmptyGuid())
                    return;

                _objCharacter.LimitModifiers.Add(objLimitModifier);
            }

            MakeDirtyWithCharacterUpdate?.Invoke(null, null);
        }

        private void cmdDeleteLimitModifier_Click(object sender, EventArgs e)
        {
            if (!(treLimit.SelectedNode?.Tag is ICanRemove selectedObject)) return;
            if (!selectedObject.Remove(GlobalOptions.ConfirmDelete)) return;
            MakeDirtyWithCharacterUpdate?.Invoke(null, null);
        }
        private void treLimit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteLimitModifier_Click(sender, e);
            }
        }

        private void tssLimitModifierNotes_Click(object sender, EventArgs e)
        {
            if (treLimit.SelectedNode == null) return;
            if (treLimit.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treLimit.SelectedNode);
            }
            else
            {
                // the limit modifier has a source
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (objImprovement.ImproveType != Improvement.ImprovementType.LimitModifier ||
                        objImprovement.SourceName != treLimit.SelectedNode?.Tag.ToString())
                        continue;
                    string strOldValue = objImprovement.Notes;
                    using (frmNotes frmItemNotes = new frmNotes { Notes = strOldValue })
                    {
                        frmItemNotes.ShowDialog(this);
                        if (frmItemNotes.DialogResult != DialogResult.OK)
                            continue;

                        objImprovement.Notes = frmItemNotes.Notes;
                    }

                    if (objImprovement.Notes == strOldValue)
                        continue;
                    MakeDirty?.Invoke(null, null);

                    treLimit.SelectedNode.ForeColor = objImprovement.PreferredColor;
                    treLimit.SelectedNode.ToolTipText = objImprovement.Notes.WordWrap();
                }
            }
        }

        private void tssLimitModifierEdit_Click(object sender, EventArgs e)
        {
            UpdateLimitModifier();
        }
        #endregion
        #region Methods

        /// <summary>
        /// Allows the user to input notes that should be linked to the selected object.
        /// TODO: Should be linked back to CharacterShared in some way or moved into a more generic helper class.
        /// </summary>
        /// <param name="objNotes"></param>
        /// <param name="treNode"></param>
        private void WriteNotes(IHasNotes objNotes, TreeNode treNode)
        {
            string strOldValue = objNotes.Notes;
            using (frmNotes frmItemNotes = new frmNotes { Notes = strOldValue })
            {
                frmItemNotes.ShowDialog(this);
                if (frmItemNotes.DialogResult != DialogResult.OK)
                    return;

                objNotes.Notes = frmItemNotes.Notes;
            }

            if (objNotes.Notes == strOldValue)
                return;
            treNode.ForeColor = objNotes.PreferredColor;
            treNode.ToolTipText = objNotes.Notes.WordWrap();
            MakeDirty?.Invoke(null,null);
        }

        private void RefreshLimitModifiers(NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            string strSelectedId = (treLimit.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode[] aobjLimitNodes = new TreeNode[(int)LimitType.NumLimitTypes];

            if (notifyCollectionChangedEventArgs == null)
            {
                treLimit.Nodes.Clear();

                // Add Limit Modifiers.
                foreach (LimitModifier objLimitModifier in _objCharacter.LimitModifiers)
                {
                    int intTargetLimit = (int)Enum.Parse(typeof(LimitType), objLimitModifier.Limit);
                    TreeNode objParentNode = GetLimitModifierParentNode(intTargetLimit);
                    if (!objParentNode.Nodes.ContainsKey(objLimitModifier.CurrentDisplayName))
                    {
                        objParentNode.Nodes.Add(objLimitModifier.CreateTreeNode(objLimitModifier.CanDelete ? cmsLimitModifier : cmsLimitModifierNotesOnly));
                    }
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
                        TreeNode objParentNode = GetLimitModifierParentNode(intTargetLimit);
                        string strName = objImprovement.UniqueName + LanguageManager.GetString("String_Colon") + LanguageManager.GetString("String_Space");
                        if (objImprovement.Value > 0)
                            strName += '+';
                        strName += objImprovement.Value.ToString(GlobalOptions.CultureInfo);
                        if (!string.IsNullOrEmpty(objImprovement.Condition))
                            strName += ',' + LanguageManager.GetString("String_Space") + objImprovement.Condition;
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
                                if (objImprovement.ImproveType == Improvement.ImprovementType.SocialLimit)
                                    objImprovement.ImprovedName = "Social";
                                else if (objImprovement.ImproveType == Improvement.ImprovementType.MentalLimit)
                                    objImprovement.ImprovedName = "Mental";
                                else
                                    objImprovement.ImprovedName = "Physical";
                            }

                            objParentNode.Nodes.Add(objNode);
                        }
                    }
                }

                treLimit.SortCustomAlphabetically(strSelectedId);
            }
            else
            {
                aobjLimitNodes[0] = treLimit.FindNode("Node_Physical", false);
                aobjLimitNodes[1] = treLimit.FindNode("Node_Mental", false);
                aobjLimitNodes[2] = treLimit.FindNode("Node_Social", false);
                aobjLimitNodes[3] = treLimit.FindNode("Node_Astral", false);

                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (LimitModifier objLimitModifier in notifyCollectionChangedEventArgs.NewItems)
                            {
                                int intTargetLimit = (int)Enum.Parse(typeof(LimitType), objLimitModifier.Limit);
                                TreeNode objParentNode = GetLimitModifierParentNode(intTargetLimit);
                                TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                                if (!lstParentNodeChildren.ContainsKey(objLimitModifier.CurrentDisplayName))
                                {
                                    TreeNode objNode = objLimitModifier.CreateTreeNode(objLimitModifier.CanDelete ? cmsLimitModifier : cmsLimitModifierNotesOnly);
                                    int intNodesCount = lstParentNodeChildren.Count;
                                    int intTargetIndex = 0;
                                    for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                                    {
                                        if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                                        {
                                            break;
                                        }
                                    }
                                    lstParentNodeChildren.Insert(intTargetIndex, objNode);
                                    objParentNode.Expand();
                                    treLimit.SelectedNode = objNode;
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (LimitModifier objLimitModifier in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treLimit.FindNodeByTag(objLimitModifier);
                                if (objNode != null)
                                {
                                    TreeNode objParent = objNode.Parent;
                                    objNode.Remove();
                                    if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                        objParent.Remove();
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParentNodes = new List<TreeNode>(notifyCollectionChangedEventArgs.OldItems.Count);
                            foreach (LimitModifier objLimitModifier in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treLimit.FindNodeByTag(objLimitModifier);
                                if (objNode != null)
                                {
                                    lstOldParentNodes.Add(objNode.Parent);
                                    objNode.Remove();
                                }
                            }
                            foreach (LimitModifier objLimitModifier in notifyCollectionChangedEventArgs.NewItems)
                            {
                                int intTargetLimit = (int)Enum.Parse(typeof(LimitType), objLimitModifier.Limit);
                                TreeNode objParentNode = GetLimitModifierParentNode(intTargetLimit);
                                TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                                if (!lstParentNodeChildren.ContainsKey(objLimitModifier.CurrentDisplayName))
                                {
                                    TreeNode objNode = objLimitModifier.CreateTreeNode(objLimitModifier.CanDelete ? cmsLimitModifier : cmsLimitModifierNotesOnly);
                                    int intNodesCount = lstParentNodeChildren.Count;
                                    int intTargetIndex = 0;
                                    for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                                    {
                                        if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                                        {
                                            break;
                                        }
                                    }
                                    lstParentNodeChildren.Insert(intTargetIndex, objNode);
                                    objParentNode.Expand();
                                    treLimit.SelectedNode = objNode;
                                }
                            }
                            foreach (TreeNode objOldParentNode in lstOldParentNodes)
                            {
                                if (objOldParentNode.Level == 0 && objOldParentNode.Nodes.Count == 0)
                                    objOldParentNode.Remove();
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshLimitModifiers();
                        }
                        break;
                }
            }

            TreeNode GetLimitModifierParentNode(int intTargetLimit)
            {
                TreeNode objParentNode = aobjLimitNodes[intTargetLimit];
                if (objParentNode == null)
                {
                    switch (intTargetLimit)
                    {
                        case 0:
                            objParentNode = new TreeNode()
                            {
                                Tag = "Node_Physical",
                                Text = LanguageManager.GetString("Node_Physical")
                            };
                            treLimit.Nodes.Insert(0, objParentNode);
                            break;
                        case 1:
                            objParentNode = new TreeNode()
                            {
                                Tag = "Node_Mental",
                                Text = LanguageManager.GetString("Node_Mental")
                            };
                            treLimit.Nodes.Insert(aobjLimitNodes[0] == null ? 0 : 1, objParentNode);
                            break;
                        case 2:
                            objParentNode = new TreeNode()
                            {
                                Tag = "Node_Social",
                                Text = LanguageManager.GetString("Node_Social")
                            };
                            treLimit.Nodes.Insert((aobjLimitNodes[0] == null ? 0 : 1) + (aobjLimitNodes[1] == null ? 0 : 1), objParentNode);
                            break;
                        case 3:
                            objParentNode = new TreeNode()
                            {
                                Tag = "Node_Astral",
                                Text = LanguageManager.GetString("Node_Astral")
                            };
                            treLimit.Nodes.Add(objParentNode);
                            break;
                    }

                    aobjLimitNodes[intTargetLimit] = objParentNode;
                    objParentNode?.Expand();
                }
                return objParentNode;
            }
        }

        /// <summary>
        /// Edit and update a Limit Modifier.
        /// </summary>
        protected void UpdateLimitModifier()
        {
            if (treLimit.SelectedNode.Level <= 0) return;
            TreeNode objSelectedNode = treLimit.SelectedNode;
            string strGuid = (objSelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
            if (string.IsNullOrEmpty(strGuid) || strGuid.IsEmptyGuid())
                return;
            LimitModifier objLimitModifier = _objCharacter.LimitModifiers.FindById(strGuid);
            //If the LimitModifier couldn't be found (Ie it comes from an Improvement or the user hasn't properly selected a treenode, fail out early.
            if (objLimitModifier == null)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Warning_NoLimitFound"));
                return;
            }

            using (frmSelectLimitModifier frmPickLimitModifier = new frmSelectLimitModifier(objLimitModifier, "Physical", "Mental", "Social"))
            {
                frmPickLimitModifier.ShowDialog(this);

                if (frmPickLimitModifier.DialogResult == DialogResult.Cancel)
                    return;

                //Remove the old LimitModifier to ensure we don't double up.
                _objCharacter.LimitModifiers.Remove(objLimitModifier);
                // Create the new limit modifier.
                objLimitModifier = new LimitModifier(_objCharacter, strGuid);
                objLimitModifier.Create(frmPickLimitModifier.SelectedName, frmPickLimitModifier.SelectedBonus, frmPickLimitModifier.SelectedLimitType, frmPickLimitModifier.SelectedCondition, true);

                _objCharacter.LimitModifiers.Add(objLimitModifier);
            }

            MakeDirtyWithCharacterUpdate?.Invoke(null, null);
        }

        private void LimitModifierCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshLimitModifiers(notifyCollectionChangedEventArgs);
        }
        #endregion
        #region Properties

        public ContextMenuStrip LimitContextMenuStrip => cmsLimitModifierNotesOnly;
        public TreeView LimitTreeView => treLimit;

        #endregion

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
