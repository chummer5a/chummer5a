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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chummer.Backend.Equipment;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using System.Text;
using System.ComponentModel;
using Chummer.UI.Attributes;
using System.Collections.ObjectModel;
using Microsoft.ApplicationInsights;
using Chummer.Classes;
using Microsoft.ApplicationInsights.DataContracts;
using NLog;

namespace Chummer
{
    /// <summary>
    /// Contains functionality shared between frmCreate and frmCareer
    /// </summary>
    [DesignerCategory("")]
    public class CharacterShared : Form
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly TelemetryClient TelemetryClient = new TelemetryClient();
        private readonly Character _objCharacter;
        private readonly CharacterOptions _objOptions;
        private bool _blnIsDirty;
        private bool _blnIsRefreshing;
        private bool _blnLoading = true;
        private frmViewer _frmPrintView;

        protected CharacterShared(Character objCharacter)
        {
            _objCharacter = objCharacter;
            _objOptions = _objCharacter.Options;
            string name = "Show_Form_" + this.GetType();
            PageViewTelemetry pvt = new PageViewTelemetry(name);
            pvt.Id = Guid.NewGuid().ToString();
            pvt.Name = name;
            pvt.Context.Operation.Name = "Operation CharacterShared.Constructor()";
            pvt.Properties.Add("Name", objCharacter?.Name);
            pvt.Properties.Add("Path", objCharacter?.FileName);
            pvt.Timestamp = DateTimeOffset.UtcNow;
            this.Shown += delegate(object sender, EventArgs args)
            {
                pvt.Duration = DateTimeOffset.UtcNow-pvt.Timestamp;
                if (objCharacter != null)
                {
                    if (Uri.TryCreate(objCharacter.FileName, UriKind.Absolute, out Uri Uriresult))
                        pvt.Url = Uriresult;
                }
                TelemetryClient.TrackPageView(pvt);
            };
        }

        [Obsolete("This constructor is for use by form designers only.", true)]
        protected CharacterShared()
        {
        }

        /// <summary>
        /// Wrapper for relocating contact forms.
        /// </summary>
        protected struct TransportWrapper
        {
            public Control Control { get; }

            public TransportWrapper(Control objControl)
            {
                Control = objControl;
            }

            public override bool Equals(object obj)
            {
                return Control.Equals(obj);
            }

            public static bool operator ==(TransportWrapper objX, object objY)
            {
                return objX.Equals(objY);
            }

            public static bool operator !=(TransportWrapper objX, object objY)
            {
                return !objX.Equals(objY);
            }

            public static bool operator ==(object objX, TransportWrapper objY)
            {
                return objX?.Equals(objY) ?? objY == null;
            }

            public static bool operator !=(object objX, TransportWrapper objY)
            {
                return objX?.Equals(objY) ?? objY == null;
            }

            public override int GetHashCode()
            {
                return Control.GetHashCode();
            }

            public override string ToString()
            {
                return Control.ToString();
            }
        }

        protected Stopwatch AutosaveStopWatch { get; } = Stopwatch.StartNew();
        /// <summary>
        /// Automatically Save the character to a backup folder.
        /// </summary>
        protected void AutoSaveCharacter()
        {
            Cursor objOldCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            string strAutosavePath = string.Empty;
            try
            {
                strAutosavePath = Path.Combine(Utils.GetStartupPath, "saves", "autosave");
            }
            catch (System.ArgumentException e)
            {
                Log.Error(e, "Path: " + Utils.GetStartupPath);
                return;
            }

            if (!Directory.Exists(strAutosavePath))
            {
                try
                {
                    Directory.CreateDirectory(strAutosavePath);
                }
                catch (UnauthorizedAccessException)
                {
                    Cursor = Cursors.Default;
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
                    AutosaveStopWatch.Restart();
                    return;
                }
            }

            string[] strFile = _objCharacter.FileName.Split(Path.DirectorySeparatorChar);
            string strShowFileName = strFile[strFile.Length - 1];

            if (string.IsNullOrEmpty(strShowFileName))
                strShowFileName = _objCharacter.CharacterName;
            var replaceChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (var invalidChar in replaceChars)
            {
                strShowFileName = strShowFileName.Replace(invalidChar, '_');
            }
            string strFilePath = Path.Combine(strAutosavePath, strShowFileName);
            _objCharacter.Save(strFilePath, false, false);
            Cursor = objOldCursor;
            AutosaveStopWatch.Restart();
        }

        /// <summary>
        /// Edit and update a Limit Modifier.
        /// </summary>
        /// <param name="treLimit"></param>
        protected void UpdateLimitModifier(TreeView treLimit)
        {
            if (treLimit.SelectedNode.Level > 0)
            {
                TreeNode objSelectedNode = treLimit.SelectedNode;
                string strGuid = (objSelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                if (string.IsNullOrEmpty(strGuid) || strGuid.IsEmptyGuid())
                    return;
                LimitModifier objLimitModifier = _objCharacter.LimitModifiers.FindById(strGuid);
                //If the LimitModifier couldn't be found (Ie it comes from an Improvement or the user hasn't properly selected a treenode, fail out early.
                if (objLimitModifier == null)
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Warning_NoLimitFound", GlobalOptions.Language));
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

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objNotes"></param>
        /// <param name="treNode"></param>
        protected void WriteNotes(IHasNotes objNotes, TreeNode treNode)
        {
            string strOldValue = objNotes.Notes;
            frmNotes frmItemNotes = new frmNotes
            {
                Notes = strOldValue
            };
            frmItemNotes.ShowDialog(this);

            if (frmItemNotes.DialogResult == DialogResult.OK)
            {
                objNotes.Notes = frmItemNotes.Notes;
                if (objNotes.Notes != strOldValue)
                {
                    IsDirty = true;

                    treNode.ForeColor = objNotes.PreferredColor;
                    treNode.ToolTipText = objNotes.Notes.WordWrap(100);
                }
            }
        }

        protected void RefreshAttributes(FlowLayoutPanel pnlAttributes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (notifyCollectionChangedEventArgs == null)
            {
                pnlAttributes.SuspendLayout();
                pnlAttributes.Controls.Clear();

                foreach (CharacterAttrib objAttrib in CharacterObject.AttributeSection.Attributes)
                {
                    AttributeControl objControl = new AttributeControl(objAttrib);
                    objControl.ValueChanged += MakeDirtyWithCharacterUpdate;
                    pnlAttributes.Controls.Add(objControl);
                    objControl.Width = pnlAttributes.Width;
                    objControl.Anchor |= AnchorStyles.Right;
                }
                pnlAttributes.ResumeLayout();
            }
            else
            {
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (CharacterAttrib objAttrib in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AttributeControl objControl = new AttributeControl(objAttrib);
                                objControl.ValueChanged += MakeDirtyWithCharacterUpdate;
                                pnlAttributes.Controls.Add(objControl);
                                objControl.Width = pnlAttributes.Width;
                                objControl.Anchor |= AnchorStyles.Right;
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (CharacterAttrib objAttrib in notifyCollectionChangedEventArgs.OldItems)
                            {
                                foreach (AttributeControl objControl in pnlAttributes.Controls)
                                {
                                    if (objControl.AttributeName == objAttrib.Abbrev)
                                    {
                                        objControl.ValueChanged -= MakeDirtyWithCharacterUpdate;
                                        pnlAttributes.Controls.Remove(objControl);
                                        objControl.Dispose();
                                    }
                                }
                                if (!_objCharacter.Created)
                                {
                                    objAttrib.Base = 0;
                                    objAttrib.Karma = 0;
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (CharacterAttrib objAttrib in notifyCollectionChangedEventArgs.OldItems)
                            {
                                foreach (AttributeControl objControl in pnlAttributes.Controls)
                                {
                                    if (objControl.AttributeName == objAttrib.Abbrev)
                                    {
                                        objControl.ValueChanged -= MakeDirtyWithCharacterUpdate;
                                        pnlAttributes.Controls.Remove(objControl);
                                        objControl.Dispose();
                                    }
                                }
                                if (!_objCharacter.Created)
                                {
                                    objAttrib.Base = 0;
                                    objAttrib.Karma = 0;
                                }
                            }
                            foreach (CharacterAttrib objAttrib in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AttributeControl objControl = new AttributeControl(objAttrib);
                                objControl.ValueChanged += MakeDirtyWithCharacterUpdate;
                                pnlAttributes.Controls.Add(objControl);
                                objControl.Width = pnlAttributes.Width;
                                objControl.Anchor |= AnchorStyles.Right;
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshAttributes(pnlAttributes);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Clears and updates the treeview for Spells. Typically called as part of AddQuality or UpdateCharacterInfo.
        /// </summary>
        /// <param name="treSpells">Spells tree.</param>
        /// <param name="treMetamagic">Initiations tree.</param>
        /// <param name="cmsSpell">ContextMenuStrip that will be added to spells in the spell tree.</param>
        /// <param name="cmsInitiationNotes">ContextMenuStrip that will be added to spells in the initiations tree.</param>
        /// <param name="notifyCollectionChangedEventArgs">Arguments for the change to the underlying ObservableCollection.</param>
        protected void RefreshSpells(TreeView treSpells, TreeView treMetamagic, ContextMenuStrip cmsSpell, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            TreeNode objCombatNode = null;
            TreeNode objDetectionNode = null;
            TreeNode objHealthNode = null;
            TreeNode objIllusionNode = null;
            TreeNode objManipulationNode = null;
            TreeNode objRitualsNode = null;
            TreeNode objEnchantmentsNode = null;
            if (notifyCollectionChangedEventArgs == null)
            {
                string strSelectedId = (treSpells.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                string strSelectedMetamagicId = (treMetamagic.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                // Clear the default nodes of entries.
                treSpells.Nodes.Clear();

                // Add the Spells that exist.
                foreach (Spell objSpell in _objCharacter.Spells)
                {
                    if (objSpell.Grade > 0)
                    {
                        treMetamagic.FindNodeByTag(objSpell)?.Remove();
                    }
                    AddToTree(objSpell, false);
                }
                treSpells.SortCustomAlphabetically(strSelectedId);
                treMetamagic.SelectedNode = treMetamagic.FindNode(strSelectedMetamagicId);
            }
            else
            {
                objCombatNode = treSpells.FindNode("Node_SelectedCombatSpells", false);
                objDetectionNode = treSpells.FindNode("Node_SelectedDetectionSpells", false);
                objHealthNode = treSpells.FindNode("Node_SelectedHealthSpells", false);
                objIllusionNode = treSpells.FindNode("Node_SelectedIllusionSpells", false);
                objManipulationNode = treSpells.FindNode("Node_SelectedManipulationSpells", false);
                objRitualsNode = treSpells.FindNode("Node_SelectedGeomancyRituals", false);
                objEnchantmentsNode = treSpells.FindNode("Node_SelectedEnchantments", false);
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (Spell objSpell in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objSpell);
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Spell objSpell in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treSpells.FindNodeByTag(objSpell);
                                if (objNode != null)
                                {
                                    TreeNode objParent = objNode.Parent;
                                    objNode.Remove();
                                    if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                        objParent.Remove();
                                }
                                if (objSpell.Grade > 0)
                                {
                                    treMetamagic.FindNode(objSpell.InternalId)?.Remove();
                                }
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshSpells(treSpells, treMetamagic, cmsSpell, cmsInitiationNotes);
                            break;
                        }
                    case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents = new List<TreeNode>();
                            foreach (Spell objSpell in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treSpells.FindNodeByTag(objSpell);
                                if (objNode != null)
                                {
                                    lstOldParents.Add(objNode.Parent);
                                    objNode.Remove();
                                }
                                if (objSpell.Grade > 0)
                                {
                                    treMetamagic.FindNode(objSpell.InternalId)?.Remove();
                                }
                            }
                            foreach (Spell objSpell in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objSpell);
                            }
                            foreach (TreeNode objOldParent in lstOldParents)
                            {
                                if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                    objOldParent.Remove();
                            }
                            break;
                        }
                }
            }

            void AddToTree(Spell objSpell, bool blnSingleAdd = true)
            {
                TreeNode objNode = objSpell.CreateTreeNode(cmsSpell);
                if (objNode == null)
                    return;
                TreeNode objParentNode = null;
                switch (objSpell.Category)
                {
                    case "Combat":
                        if (objCombatNode == null)
                        {
                            objCombatNode = new TreeNode
                            {
                                Tag = "Node_SelectedCombatSpells",
                                Text = LanguageManager.GetString("Node_SelectedCombatSpells", GlobalOptions.Language)
                            };
                            treSpells.Nodes.Insert(0, objCombatNode);
                            objCombatNode.Expand();
                        }
                        objParentNode = objCombatNode;
                        break;
                    case "Detection":
                        if (objDetectionNode == null)
                        {
                            objDetectionNode = new TreeNode
                            {
                                Tag = "Node_SelectedDetectionSpells",
                                Text = LanguageManager.GetString("Node_SelectedDetectionSpells", GlobalOptions.Language)
                            };
                            treSpells.Nodes.Insert(objCombatNode == null ? 0 : 1, objDetectionNode);
                            objDetectionNode.Expand();
                        }
                        objParentNode = objDetectionNode;
                        break;
                    case "Health":
                        if (objHealthNode == null)
                        {
                            objHealthNode = new TreeNode
                            {
                                Tag = "Node_SelectedHealthSpells",
                                Text = LanguageManager.GetString("Node_SelectedHealthSpells", GlobalOptions.Language)
                            };
                            treSpells.Nodes.Insert((objCombatNode == null ? 0 : 1) +
                                (objDetectionNode == null ? 0 : 1), objHealthNode);
                            objHealthNode.Expand();
                        }
                        objParentNode = objHealthNode;
                        break;
                    case "Illusion":
                        if (objIllusionNode == null)
                        {
                            objIllusionNode = new TreeNode
                            {
                                Tag = "Node_SelectedIllusionSpells",
                                Text = LanguageManager.GetString("Node_SelectedIllusionSpells", GlobalOptions.Language)
                            };
                            treSpells.Nodes.Insert((objCombatNode == null ? 0 : 1) +
                                (objDetectionNode == null ? 0 : 1) +
                                (objHealthNode == null ? 0 : 1), objIllusionNode);
                            objIllusionNode.Expand();
                        }
                        objParentNode = objIllusionNode;
                        break;
                    case "Manipulation":
                        if (objManipulationNode == null)
                        {
                            objManipulationNode = new TreeNode
                            {
                                Tag = "Node_SelectedManipulationSpells",
                                Text = LanguageManager.GetString("Node_SelectedManipulationSpells", GlobalOptions.Language)
                            };
                            treSpells.Nodes.Insert((objCombatNode == null ? 0 : 1) +
                                (objDetectionNode == null ? 0 : 1) +
                                (objHealthNode == null ? 0 : 1) +
                                (objIllusionNode == null ? 0 : 1), objManipulationNode);
                            objManipulationNode.Expand();
                        }
                        objParentNode = objManipulationNode;
                        break;
                    case "Rituals":
                        if (objRitualsNode == null)
                        {
                            objRitualsNode = new TreeNode
                            {
                                Tag = "Node_SelectedGeomancyRituals",
                                Text = LanguageManager.GetString("Node_SelectedGeomancyRituals", GlobalOptions.Language)
                            };
                            treSpells.Nodes.Insert((objCombatNode == null ? 0 : 1) +
                                (objDetectionNode == null ? 0 : 1) +
                                (objHealthNode == null ? 0 : 1) +
                                (objIllusionNode == null ? 0 : 1) +
                                (objManipulationNode == null ? 0 : 1), objRitualsNode);
                            objRitualsNode.Expand();
                        }
                        objParentNode = objRitualsNode;
                        break;
                    case "Enchantments":
                        if (objEnchantmentsNode == null)
                        {
                            objEnchantmentsNode = new TreeNode
                            {
                                Tag = "Node_SelectedEnchantments",
                                Text = LanguageManager.GetString("Node_SelectedEnchantments", GlobalOptions.Language)
                            };
                            treSpells.Nodes.Add(objEnchantmentsNode);
                            objEnchantmentsNode.Expand();
                        }
                        objParentNode = objEnchantmentsNode;
                        break;
                }
                if (objSpell.Grade > 0)
                {
                    InitiationGrade objGrade = _objCharacter.InitiationGrades.FirstOrDefault(x => x.Grade == objSpell.Grade);
                    if (objGrade != null)
                    {
                        TreeNode nodMetamagicParent = treMetamagic.FindNodeByTag(objGrade);
                        if (nodMetamagicParent != null)
                        {
                            TreeNodeCollection nodMetamagicParentChildren = nodMetamagicParent.Nodes;
                            TreeNode objMetamagicNode = objSpell.CreateTreeNode(cmsInitiationNotes, true);
                            int intNodesCount = nodMetamagicParentChildren.Count;
                            int intTargetIndex = 0;
                            for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                            {
                                if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex], objMetamagicNode) >= 0)
                                {
                                    break;
                                }
                            }

                            nodMetamagicParentChildren.Insert(intTargetIndex, objMetamagicNode);
                            if (blnSingleAdd)
                                treMetamagic.SelectedNode = objMetamagicNode;
                        }
                    }
                }

                if (objParentNode != null)
                {
                    if (blnSingleAdd)
                    {
                        TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
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
                        treSpells.SelectedNode = objNode;
                    }
                    else
                        objParentNode.Nodes.Add(objNode);
                }
            }
        }

        protected void RefreshAIPrograms(TreeView treAIPrograms, ContextMenuStrip cmsAdvancedProgram, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            TreeNode objParentNode = null;
            if (notifyCollectionChangedEventArgs == null)
            {
                string strSelectedId = (treAIPrograms.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                treAIPrograms.Nodes.Clear();

                // Add AI Programs.
                foreach (AIProgram objAIProgram in CharacterObject.AIPrograms)
                {
                    AddToTree(objAIProgram, false);
                }

                treAIPrograms.SortCustomAlphabetically(strSelectedId);
            }
            else
            {
                objParentNode = treAIPrograms.FindNode("Node_SelectedAIPrograms", false);
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (AIProgram objAIProgram in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objAIProgram);
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (AIProgram objAIProgram in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treAIPrograms.FindNodeByTag(objAIProgram);
                                if (objNode != null)
                                {
                                    TreeNode objParent = objNode.Parent;
                                    objNode.Remove();
                                    if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                        objParent.Remove();
                                }
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshAIPrograms(treAIPrograms, cmsAdvancedProgram);
                            break;
                        }
                    case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents = new List<TreeNode>();
                            foreach (AIProgram objAIProgram in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treAIPrograms.FindNodeByTag(objAIProgram);
                                if (objNode != null)
                                {
                                    lstOldParents.Add(objNode.Parent);
                                    objNode.Remove();
                                }
                            }
                            foreach (AIProgram objAIProgram in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objAIProgram);
                            }
                            foreach (TreeNode objOldParent in lstOldParents)
                            {
                                if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                    objOldParent.Remove();
                            }
                            break;
                        }
                }
            }

            void AddToTree(AIProgram objAIProgram, bool blnSingleAdd = true)
            {
                TreeNode objNode = objAIProgram.CreateTreeNode(cmsAdvancedProgram);
                if (objNode == null)
                    return;

                if (objParentNode == null)
                {
                    objParentNode = new TreeNode()
                    {
                        Tag = "Node_SelectedAIPrograms",
                        Text = LanguageManager.GetString("Node_SelectedAIPrograms", GlobalOptions.Language)
                    };
                    treAIPrograms.Nodes.Add(objParentNode);
                    objParentNode.Expand();
                }

                if (blnSingleAdd)
                {
                    TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
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
                    treAIPrograms.SelectedNode = objNode;
                }
                else
                    objParentNode.Nodes.Add(objNode);
            }
        }

        protected void RefreshComplexForms(TreeView treComplexForms, TreeView treMetamagic, ContextMenuStrip cmsComplexForm, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            TreeNode objParentNode = null;
            if (notifyCollectionChangedEventArgs == null)
            {
                string strSelectedId =
                    (treComplexForms.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                string strSelectedMetamagicId = (treMetamagic.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                treComplexForms.Nodes.Clear();

                // Add Complex Forms.
                foreach (ComplexForm objComplexForm in CharacterObject.ComplexForms)
                {
                    if (objComplexForm.Grade > 0)
                    {
                        treMetamagic.FindNodeByTag(objComplexForm)?.Remove();
                    }
                    AddToTree(objComplexForm, false);
                }

                treComplexForms.SortCustomAlphabetically(strSelectedId);
                treMetamagic.SelectedNode = treMetamagic.FindNode(strSelectedMetamagicId);
            }
            else
            {
                objParentNode = treComplexForms.FindNode("Node_SelectedAdvancedComplexForms", false);
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (ComplexForm objComplexForm in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objComplexForm);
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (ComplexForm objComplexForm in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treComplexForms.FindNodeByTag(objComplexForm);
                                if (objNode != null)
                                {
                                    TreeNode objParent = objNode.Parent;
                                    objNode.Remove();
                                    if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                        objParent.Remove();
                                }
                                if (objComplexForm.Grade > 0)
                                {
                                    treMetamagic.FindNodeByTag(objComplexForm)?.Remove();
                                }
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents = new List<TreeNode>();
                            foreach (ComplexForm objComplexForm in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treComplexForms.FindNodeByTag(objComplexForm);
                                if (objNode != null)
                                {
                                    lstOldParents.Add(objNode.Parent);
                                    objNode.Remove();
                                }
                                if (objComplexForm.Grade > 0)
                                {
                                    treMetamagic.FindNodeByTag(objComplexForm)?.Remove();
                                }
                            }
                            foreach (ComplexForm objComplexForm in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objComplexForm);
                            }
                            foreach (TreeNode objOldParent in lstOldParents)
                            {
                                if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                    objOldParent.Remove();
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshComplexForms(treComplexForms, treMetamagic, cmsComplexForm, cmsInitiationNotes);
                            break;
                        }
                }
            }

            void AddToTree(ComplexForm objComplexForm, bool blnSingleAdd = true)
            {
                TreeNode objNode = objComplexForm.CreateTreeNode(cmsComplexForm);
                if (objNode == null)
                    return;
                if (objParentNode == null)
                {
                    objParentNode = new TreeNode()
                    {
                        Tag = "Node_SelectedAdvancedComplexForms",
                        Text = LanguageManager.GetString("Node_SelectedAdvancedComplexForms", GlobalOptions.Language)
                    };
                    treComplexForms.Nodes.Add(objParentNode);
                    objParentNode.Expand();
                }
                if (objComplexForm.Grade > 0)
                {
                    InitiationGrade objGrade = _objCharacter.InitiationGrades.FirstOrDefault(x => x.Grade == objComplexForm.Grade);
                    if (objGrade != null)
                    {
                        TreeNode nodMetamagicParent = treMetamagic.FindNodeByTag(objGrade);
                        if (nodMetamagicParent != null)
                        {
                            TreeNodeCollection nodMetamagicParentChildren = nodMetamagicParent.Nodes;
                            TreeNode objMetamagicNode = objComplexForm.CreateTreeNode(cmsInitiationNotes);
                            int intNodesCount = nodMetamagicParentChildren.Count;
                            int intTargetIndex = 0;
                            for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                            {
                                if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex], objMetamagicNode) >= 0)
                                {
                                    break;
                                }
                            }

                            nodMetamagicParentChildren.Insert(intTargetIndex, objMetamagicNode);
                            if (blnSingleAdd)
                                treMetamagic.SelectedNode = objMetamagicNode;
                        }
                    }
                }
                if (blnSingleAdd)
                {
                    TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
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
                    treComplexForms.SelectedNode = objNode;
                }
                else
                    objParentNode.Nodes.Add(objNode);
            }
        }

        protected void RefreshInitiationGrades(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (notifyCollectionChangedEventArgs == null)
            {
                string strSelectedId = (treMetamagic.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                TreeNodeCollection lstRootNodes = treMetamagic.Nodes;
                lstRootNodes.Clear();

                foreach (InitiationGrade objGrade in _objCharacter.InitiationGrades)
                {
                    AddToTree(objGrade);
                }

                int intOffset = lstRootNodes.Count;
                foreach (Metamagic objMetamagic in _objCharacter.Metamagics)
                {
                    if (objMetamagic.Grade < 0)
                    {
                        TreeNode objNode = objMetamagic.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode != null)
                        {
                            int intNodesCount = lstRootNodes.Count;
                            int intTargetIndex = intOffset;
                            for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                            {
                                if (CompareTreeNodes.CompareText(lstRootNodes[intTargetIndex], objNode) >= 0)
                                {
                                    break;
                                }
                            }

                            lstRootNodes.Insert(intTargetIndex, objNode);
                            objNode.Expand();
                        }
                    }
                }

                treMetamagic.SelectedNode = treMetamagic.FindNode(strSelectedId);
            }
            else
            {
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (InitiationGrade objGrade in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objGrade, intNewIndex);
                                intNewIndex += 1;
                                if (CharacterObject.RESEnabled)
                                {
                                    CharacterObject.SubmersionGrade++;
                                }
                                else
                                {
                                    CharacterObject.InitiateGrade++;
                                }
                            }
                            //TODO: Annoying boilerplate, but whatever.
                            if (CharacterObject.RESEnabled)
                            {
                                // Remove any existing Initiation Improvements.
                                ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Submersion);

                                // Create the replacement Improvement.
                                ImprovementManager.CreateImprovement(CharacterObject, "RES", Improvement.ImprovementSource.Submersion, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, CharacterObject.SubmersionGrade);
                                ImprovementManager.Commit(CharacterObject);

                                // Update any Echo Improvements the character might have.
                                foreach (Metamagic objMetamagic in CharacterObject.Metamagics.Where(metamagic => metamagic.Bonus != null))
                                {
                                    // If the Bonus contains "Rating", remove the existing Improvement and create new ones.
                                    if (objMetamagic.Bonus.InnerXml.Contains("Rating"))
                                    {
                                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Echo, objMetamagic.InternalId);
                                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Echo, objMetamagic.InternalId, objMetamagic.Bonus, false, CharacterObject.SubmersionGrade, objMetamagic.DisplayNameShort(GlobalOptions.Language));
                                    }
                                }
                            }
                            else if (CharacterObject.MAGEnabled)
                            {
                                // Remove any existing Initiation Improvements.
                                ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Initiation);

                                // Create the replacement Improvement.
                                ImprovementManager.CreateImprovement(CharacterObject, "MAG", Improvement.ImprovementSource.Initiation, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, CharacterObject.InitiateGrade);
                                ImprovementManager.CreateImprovement(CharacterObject, "MAGAdept", Improvement.ImprovementSource.Initiation, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, CharacterObject.InitiateGrade);
                                ImprovementManager.Commit(CharacterObject);

                                // Update any Metamagic Improvements the character might have.
                                foreach (Metamagic objMetamagic in CharacterObject.Metamagics.Where(metamagic => metamagic.Bonus != null))
                                {
                                    // If the Bonus contains "Rating", remove the existing Improvement and create new ones.
                                    if (objMetamagic.Bonus.InnerXml.Contains("Rating"))
                                    {
                                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Metamagic, objMetamagic.InternalId);
                                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Metamagic, objMetamagic.InternalId, objMetamagic.Bonus, false, CharacterObject.InitiateGrade, objMetamagic.DisplayNameShort(GlobalOptions.Language));
                                    }
                                }
                            }
                            else
                            {
                                Utils.BreakIfDebug();
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (InitiationGrade objGrade in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treMetamagic.FindNodeByTag(objGrade)?.Remove();
                                // Remove the child objects (arts, metamagics, enhancements, enchantments, rituals)
                                // Arts
                                List<Art> lstRemoveArts = CharacterObject.Arts.Where(o => o.Grade == objGrade.Grade).ToList();
                                foreach (Art objArt in lstRemoveArts)
                                    CharacterObject.Arts.Remove(objArt);

                                // Metamagics
                                List<Metamagic> lstRemoveMetamagics = CharacterObject.Metamagics.Where(o => o.Grade == objGrade.Grade).ToList();
                                foreach (Metamagic objMetamagic in lstRemoveMetamagics)
                                {
                                    CharacterObject.Metamagics.Remove(objMetamagic);
                                    ImprovementManager.RemoveImprovements(CharacterObject, objMetamagic.SourceType, objMetamagic.InternalId);
                                }

                                // Enhancements
                                List<Enhancement> lstRemoveEnhancements = CharacterObject.Enhancements.Where(objEnhancement => objEnhancement.Grade == objGrade.Grade).ToList();
                                foreach (Enhancement objEnhancement in lstRemoveEnhancements)
                                    CharacterObject.Enhancements.Remove(objEnhancement);

                                // Spells
                                List<Spell> lstRemoveSpells = CharacterObject.Spells.Where(objSpell => objSpell.Grade == objGrade.Grade).ToList();
                                foreach (Spell objSpell in lstRemoveSpells)
                                    CharacterObject.Spells.Remove(objSpell);

                                // Complex Forms
                                List<ComplexForm> lstRemoveComplexForms = CharacterObject.ComplexForms.Where(cf => cf.Grade == objGrade.Grade).ToList();
                                foreach (ComplexForm cf in lstRemoveComplexForms)
                                    CharacterObject.ComplexForms.Remove(cf);

                                // Grade
                                CharacterObject.InitiationGrades.Remove(objGrade);

                                if (CharacterObject.MAGEnabled)
                                {
                                    CharacterObject.InitiateGrade = Math.Max(objGrade.Grade - 1, 0);
                                    // Remove any existing Initiation Improvements.
                                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Initiation);
                                    if (CharacterObject.InitiateGrade == 0) break;
                                    // Create the replacement Improvement.
                                    ImprovementManager.CreateImprovement(CharacterObject, "MAG", Improvement.ImprovementSource.Initiation, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, CharacterObject.InitiateGrade);
                                    ImprovementManager.CreateImprovement(CharacterObject, "MAGAdept", Improvement.ImprovementSource.Initiation, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, CharacterObject.InitiateGrade);
                                    ImprovementManager.Commit(CharacterObject);
                                }
                                else if (CharacterObject.RESEnabled)
                                {
                                    CharacterObject.SubmersionGrade = Math.Max(objGrade.Grade - 1, 0);

                                    // Remove any existing Initiation Improvements.
                                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Submersion);
                                    if (CharacterObject.SubmersionGrade == 0) break;
                                    // Create the replacement Improvement.
                                    ImprovementManager.CreateImprovement(CharacterObject, "RES", Improvement.ImprovementSource.Submersion, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, CharacterObject.SubmersionGrade);
                                    ImprovementManager.Commit(CharacterObject);
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (InitiationGrade objGrade in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treMetamagic.FindNodeByTag(objGrade)?.Remove();
                            }
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (InitiationGrade objGrade in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objGrade, intNewIndex);
                                intNewIndex += 1;
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        {
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (InitiationGrade objGrade in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode nodGrade = treMetamagic.FindNodeByTag(objGrade);
                                if (nodGrade != null)
                                {
                                    nodGrade.Remove();
                                    treMetamagic.Nodes.Insert(intNewIndex, nodGrade);
                                    intNewIndex += 1;
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes);
                        }
                        break;
                }

                Improvement.ImprovementSource objMetamagicSource = CharacterObject.RESEnabled ? Improvement.ImprovementSource.Echo : Improvement.ImprovementSource.Metamagic;
                int grade = CharacterObject.RESEnabled ? CharacterObject.SubmersionGrade : CharacterObject.InitiateGrade;
                // Update any Metamagic Improvements the character might have.
                foreach (Metamagic objMetamagic in CharacterObject.Metamagics.Where(metamagic => metamagic.Bonus != null))
                {
                    // If the Bonus contains "Rating", remove the existing Improvement and create new ones.
                    if (objMetamagic.Bonus.InnerXml.Contains("Rating"))
                    {
                        ImprovementManager.RemoveImprovements(CharacterObject, objMetamagicSource, objMetamagic.InternalId);
                        ImprovementManager.CreateImprovements(CharacterObject, objMetamagicSource, objMetamagic.InternalId, objMetamagic.Bonus, false, grade, objMetamagic.DisplayNameShort(GlobalOptions.Language));
                    }
                }
            }

            void AddToTree(InitiationGrade objInitiationGrade, int intIndex = -1)
            {
                TreeNode nodGrade = objInitiationGrade.CreateTreeNode(cmsMetamagic);
                TreeNodeCollection lstParentNodeChildren = nodGrade.Nodes;
                foreach (Art objArt in _objCharacter.Arts)
                {
                    if (objArt.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objArt.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            continue;
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
                    }
                }
                foreach (Metamagic objMetamagic in _objCharacter.Metamagics)
                {
                    if (objMetamagic.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objMetamagic.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            continue;
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
                    }
                }
                foreach (Spell objSpell in _objCharacter.Spells)
                {
                    if (objSpell.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objSpell.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            continue;
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
                    }
                }
                foreach (ComplexForm objComplexForm in _objCharacter.ComplexForms)
                {
                    if (objComplexForm.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objComplexForm.CreateTreeNode(cmsInitiationNotes);
                        if (objNode == null)
                            continue;
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
                    }
                }
                foreach (Enhancement objEnhancement in _objCharacter.Enhancements)
                {
                    if (objEnhancement.Grade == objInitiationGrade.Grade)
                    {
                        TreeNode objNode = objEnhancement.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            continue;
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
                    }
                }
                foreach (Power objPower in _objCharacter.Powers)
                {
                    foreach (Enhancement objEnhancement in objPower.Enhancements)
                    {
                        if (objEnhancement.Grade == objInitiationGrade.Grade)
                        {
                            TreeNode objNode = objEnhancement.CreateTreeNode(cmsInitiationNotes, true);
                            if (objNode == null)
                                continue;
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
                        }
                    }
                }
                nodGrade.Expand();
                if (intIndex < 0)
                    treMetamagic.Nodes.Add(nodGrade);
                else
                    treMetamagic.Nodes.Insert(intIndex, nodGrade);
            }
        }

        protected void RefreshArtCollection(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (Art objArt in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objArt);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Art objArt in notifyCollectionChangedEventArgs.OldItems)
                        {
                            treMetamagic.FindNodeByTag(objArt)?.Remove();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (Art objArt in notifyCollectionChangedEventArgs.OldItems)
                        {
                            treMetamagic.FindNodeByTag(objArt)?.Remove();
                        }
                        foreach (Art objArt in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objArt);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes);
                    }
                    break;
            }

            void AddToTree(Art objArt, bool blnSingleAdd = true)
            {
                InitiationGrade objGrade = CharacterObject.InitiationGrades.FirstOrDefault(x => x.Grade == objArt.Grade);

                if (objGrade != null)
                {
                    TreeNode nodMetamagicParent = treMetamagic.FindNodeByTag(objGrade);
                    if (nodMetamagicParent != null)
                    {
                        TreeNodeCollection nodMetamagicParentChildren = nodMetamagicParent.Nodes;
                        TreeNode objNode = objArt.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            return;
                        int intNodesCount = nodMetamagicParentChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }
                        nodMetamagicParentChildren.Insert(intTargetIndex, objNode);
                        nodMetamagicParent.Expand();
                        if (blnSingleAdd)
                            treMetamagic.SelectedNode = objNode;
                    }
                }
            }
        }

        protected void RefreshEnhancementCollection(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (Enhancement objEnhancement in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objEnhancement);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Enhancement objEnhancement in notifyCollectionChangedEventArgs.OldItems)
                        {
                            treMetamagic.FindNodeByTag(objEnhancement)?.Remove();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (Enhancement objEnhancement in notifyCollectionChangedEventArgs.OldItems)
                        {
                            treMetamagic.FindNodeByTag(objEnhancement)?.Remove();
                        }
                        foreach (Enhancement objEnhancement in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objEnhancement);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes);
                    }
                    break;
            }

            void AddToTree(Enhancement objEnhancement, bool blnSingleAdd = true)
            {
                InitiationGrade objGrade = CharacterObject.InitiationGrades.FirstOrDefault(x => x.Grade == objEnhancement.Grade);

                if (objGrade != null)
                {
                    TreeNode nodMetamagicParent = treMetamagic.FindNodeByTag(objGrade);
                    if (nodMetamagicParent != null)
                    {
                        TreeNodeCollection nodMetamagicParentChildren = nodMetamagicParent.Nodes;
                        TreeNode objNode = objEnhancement.CreateTreeNode(cmsInitiationNotes, true);
                        if (objNode == null)
                            return;
                        int intNodesCount = nodMetamagicParentChildren.Count;
                        int intTargetIndex = 0;
                        for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                        {
                            if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex], objNode) >= 0)
                            {
                                break;
                            }
                        }
                        nodMetamagicParentChildren.Insert(intTargetIndex, objNode);
                        nodMetamagicParent.Expand();
                        if (blnSingleAdd)
                            treMetamagic.SelectedNode = objNode;
                    }
                }
            }
        }

        protected void RefreshPowerCollectionListChanged(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, ListChangedEventArgs e = null)
        {
            switch (e?.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    {
                        CharacterObject.Powers[e.NewIndex].Enhancements.AddTaggedCollectionChanged(treMetamagic, (x, y) => RefreshEnhancementCollection(treMetamagic, cmsMetamagic, cmsInitiationNotes, y));
                    }
                    break;
                case ListChangedType.Reset:
                    {
                        RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes);
                    }
                    break;
                case null:
                    {
                        foreach (Power objPower in CharacterObject.Powers)
                        {
                            objPower.Enhancements.AddTaggedCollectionChanged(treMetamagic, (x, y) => RefreshEnhancementCollection(treMetamagic, cmsMetamagic, cmsInitiationNotes, y));
                        }
                    }
                    break;
            }
        }

        protected void RefreshPowerCollectionBeforeRemove(TreeView treMetamagic, RemovingOldEventArgs removingOldEventArgs)
        {
            if (removingOldEventArgs.OldObject is Power objPower)
            {
                objPower.Enhancements.RemoveTaggedCollectionChanged(treMetamagic);
            }
        }

        protected void RefreshMetamagicCollection(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (Metamagic objMetamagic in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objMetamagic);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Metamagic objMetamagic in notifyCollectionChangedEventArgs.OldItems)
                        {
                            treMetamagic.FindNodeByTag(objMetamagic)?.Remove();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (Metamagic objMetamagic in notifyCollectionChangedEventArgs.OldItems)
                        {
                            treMetamagic.FindNodeByTag(objMetamagic)?.Remove();
                        }
                        foreach (Metamagic objMetamagic in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objMetamagic);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes);
                    }
                    break;
            }

            void AddToTree(Metamagic objMetamagic, bool blnSingleAdd = true)
            {
                if (objMetamagic.Grade < 0)
                {
                    TreeNodeCollection nodMetamagicParentChildren = treMetamagic.Nodes;
                    TreeNode objNode = objMetamagic.CreateTreeNode(cmsInitiationNotes, true);
                    if (objNode == null)
                        return;
                    int intNodesCount = nodMetamagicParentChildren.Count;
                    int intTargetIndex = CharacterObject.InitiationGrades.Count;
                    for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                    {
                        if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex], objNode) >= 0)
                        {
                            break;
                        }
                    }
                    nodMetamagicParentChildren.Insert(intTargetIndex, objNode);
                    objNode.Expand();
                    if (blnSingleAdd)
                        treMetamagic.SelectedNode = objNode;
                }
                else
                {
                    InitiationGrade objGrade = CharacterObject.InitiationGrades.FirstOrDefault(x => x.Grade == objMetamagic.Grade);

                    if (objGrade != null)
                    {
                        TreeNode nodMetamagicParent = treMetamagic.FindNodeByTag(objGrade);
                        if (nodMetamagicParent != null)
                        {
                            TreeNodeCollection nodMetamagicParentChildren = nodMetamagicParent.Nodes;
                            TreeNode objNode = objMetamagic.CreateTreeNode(cmsInitiationNotes, true);
                            if (objNode == null)
                                return;
                            int intNodesCount = nodMetamagicParentChildren.Count;
                            int intTargetIndex = 0;
                            for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                            {
                                if (CompareTreeNodes.CompareText(nodMetamagicParentChildren[intTargetIndex], objNode) >= 0)
                                {
                                    break;
                                }
                            }
                            nodMetamagicParentChildren.Insert(intTargetIndex, objNode);
                            objNode.Expand();
                            if (blnSingleAdd)
                                treMetamagic.SelectedNode = objNode;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clears and updates the treeview for Critter Powers. Typically called as part of AddQuality or UpdateCharacterInfo.
        /// </summary>
        /// <param name="treCritterPowers">Treenode that will be cleared and populated.</param>
        /// <param name="cmsCritterPowers">ContextMenuStrip that will be added to each power.</param>
        /// <param name="notifyCollectionChangedEventArgs">Arguments for the change to the underlying ObservableCollection.</param>
        protected void RefreshCritterPowers(TreeView treCritterPowers, ContextMenuStrip cmsCritterPowers, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            TreeNode objPowersNode = null;
            TreeNode objWeaknessesNode = null;

            if (notifyCollectionChangedEventArgs == null)
            {
                string strSelectedId = (treCritterPowers.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                treCritterPowers.Nodes.Clear();
                // Add the Critter Powers that exist.
                foreach (CritterPower objPower in _objCharacter.CritterPowers)
                {
                    AddToTree(objPower, false);
                }

                treCritterPowers.SortCustomAlphabetically(strSelectedId);
            }
            else
            {
                objPowersNode = treCritterPowers.FindNode("Node_CritterPowers", false);
                objWeaknessesNode = treCritterPowers.FindNode("Node_CritterWeaknesses", false);
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (CritterPower objPower in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objPower);
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (CritterPower objPower in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treCritterPowers.FindNodeByTag(objPower);
                                if (objNode != null)
                                {
                                    TreeNode objParent = objNode.Parent;
                                    objNode.Remove();
                                    if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                        objParent.Remove();
                                }
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshCritterPowers(treCritterPowers, cmsCritterPowers);
                            break;
                        }
                    case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents = new List<TreeNode>();
                            foreach (CritterPower objPower in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treCritterPowers.FindNode(objPower.InternalId);
                                if (objNode != null)
                                {
                                    lstOldParents.Add(objNode.Parent);
                                    objNode.Remove();
                                }
                            }
                            foreach (CritterPower objPower in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objPower);
                            }
                            foreach (TreeNode objOldParent in lstOldParents)
                            {
                                if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                    objOldParent.Remove();
                            }
                            break;
                        }
                }
            }

            void AddToTree(CritterPower objPower, bool blnSingleAdd = true)
            {
                TreeNode objNode = objPower.CreateTreeNode(cmsCritterPowers);
                if (objNode == null)
                    return;
                TreeNode objParentNode;
                switch (objPower.Category)
                {
                    case "Weakness":
                        if (objWeaknessesNode == null)
                        {
                            objWeaknessesNode = new TreeNode()
                            {
                                Tag = "Node_CritterWeaknesses",
                                Text = LanguageManager.GetString("Node_CritterWeaknesses", GlobalOptions.Language)
                            };
                            treCritterPowers.Nodes.Add(objWeaknessesNode);
                            objWeaknessesNode.Expand();
                        }
                        objParentNode = objWeaknessesNode;
                        break;
                    default:
                        if (objPowersNode == null)
                        {
                            objPowersNode = new TreeNode()
                            {
                                Tag = "Node_CritterPowers",
                                Text = LanguageManager.GetString("Node_CritterPowers", GlobalOptions.Language)
                            };
                            treCritterPowers.Nodes.Insert(0, objPowersNode);
                            objPowersNode.Expand();
                        }
                        objParentNode = objPowersNode;
                        break;
                }
                if (blnSingleAdd)
                {
                    TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
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
                    treCritterPowers.SelectedNode = objNode;
                }
                else
                    objParentNode.Nodes.Add(objNode);
            }
        }

        /// <summary>
        /// Refreshes the list of qualities into the selected TreeNode. If the same number of
        /// </summary>
        /// <param name="treQualities">Treeview to insert the qualities into.</param>
        /// <param name="cmsQuality">ContextMenuStrip to add to each Quality node.</param>
        /// <param name="notifyCollectionChangedEventArgs">Arguments for the change to the underlying ObservableCollection.</param>
        protected void RefreshQualities(TreeView treQualities, ContextMenuStrip cmsQuality, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            TreeNode objPositiveQualityRoot = null;
            TreeNode objNegativeQualityRoot = null;
            TreeNode objLifeModuleRoot = null;

            if (notifyCollectionChangedEventArgs == null)
            {
                string strSelectedNode = (treQualities.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                // Create the root nodes.
                treQualities.Nodes.Clear();

                // Multiple instances of the same quality are combined into just one entry with a number next to it (e.g. 6 discrete entries of "Focused Concentration" become "Focused Concentration 6")
                HashSet<string> strQualitiesToPrint = new HashSet<string>();
                foreach (Quality objQuality in _objCharacter.Qualities)
                {
                    strQualitiesToPrint.Add(objQuality.SourceIDString + '|' + objQuality.GetSourceName(GlobalOptions.Language) + '|' + objQuality.Extra);
                }

                // Add Qualities
                foreach (Quality objQuality in _objCharacter.Qualities)
                {
                    if (!strQualitiesToPrint.Remove(objQuality.SourceIDString + '|' + objQuality.GetSourceName(GlobalOptions.Language) + '|' + objQuality.Extra))
                        continue;

                    AddToTree(objQuality, false);
                }

                treQualities.SortCustomAlphabetically(strSelectedNode);
            }
            else
            {
                objPositiveQualityRoot = treQualities.FindNodeByTag("Node_SelectedPositiveQualities", false);
                objNegativeQualityRoot = treQualities.FindNodeByTag("Node_SelectedNegativeQualities", false);
                objLifeModuleRoot = treQualities.FindNodeByTag("String_LifeModules", false);
                bool blnDoNameRefresh = false;
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (Quality objQuality in notifyCollectionChangedEventArgs.NewItems)
                            {
                                if (objQuality.Levels > 1)
                                    blnDoNameRefresh = true;
                                else
                                    AddToTree(objQuality);
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Quality objQuality in notifyCollectionChangedEventArgs.OldItems)
                            {
                                if (objQuality.Levels > 0)
                                    blnDoNameRefresh = true;
                                else
                                {
                                    TreeNode objNode = treQualities.FindNodeByTag(objQuality);
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
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshQualities(treQualities, cmsQuality);
                            break;
                        }
                    case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents = new List<TreeNode>();
                            foreach (Quality objQuality in notifyCollectionChangedEventArgs.OldItems)
                            {
                                if (objQuality.Levels > 0)
                                    blnDoNameRefresh = true;
                                else
                                {
                                    TreeNode objNode = treQualities.FindNodeByTag(objQuality);
                                    if (objNode != null)
                                    {
                                        if (objNode.Parent != null)
                                            lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }
                                    else
                                    {
                                        RefreshQualityNames(treQualities);
                                    }
                                }
                            }
                            foreach (Quality objQuality in notifyCollectionChangedEventArgs.NewItems)
                            {
                                if (objQuality.Levels > 1)
                                    blnDoNameRefresh = true;
                                else
                                    AddToTree(objQuality);
                            }
                            foreach (TreeNode objOldParent in lstOldParents)
                            {
                                if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                    objOldParent.Remove();
                            }
                            break;
                        }
                }
                if (blnDoNameRefresh)
                    RefreshQualityNames(treQualities);
            }

            void AddToTree(Quality objQuality, bool blnSingleAdd = true)
            {
                TreeNode objNode = objQuality.CreateTreeNode(cmsQuality, treQualities);
                if (objNode == null)
                    return;
                TreeNode objParentNode = null;
                switch (objQuality.Type)
                {
                    case QualityType.Positive:
                        if (objPositiveQualityRoot == null)
                        {
                            objPositiveQualityRoot = new TreeNode
                            {
                                Tag = "Node_SelectedPositiveQualities",
                                Text = LanguageManager.GetString("Node_SelectedPositiveQualities", GlobalOptions.Language)
                            };
                            treQualities.Nodes.Insert(0, objPositiveQualityRoot);
                            objPositiveQualityRoot.Expand();
                        }
                        objParentNode = objPositiveQualityRoot;
                        break;
                    case QualityType.Negative:
                        if (objNegativeQualityRoot == null)
                        {
                            objNegativeQualityRoot = new TreeNode
                            {
                                Tag = "Node_SelectedNegativeQualities",
                                Text = LanguageManager.GetString("Node_SelectedNegativeQualities", GlobalOptions.Language)
                            };
                            treQualities.Nodes.Insert(objLifeModuleRoot != null && objPositiveQualityRoot == null ? 0 : 1, objNegativeQualityRoot);
                            objNegativeQualityRoot.Expand();
                        }
                        objParentNode = objNegativeQualityRoot;
                        break;
                    case QualityType.LifeModule:
                        if (objLifeModuleRoot == null)
                        {
                            objLifeModuleRoot = new TreeNode
                            {
                                Tag = "String_LifeModules",
                                Text = LanguageManager.GetString("String_LifeModules", GlobalOptions.Language)
                            };
                            treQualities.Nodes.Add(objLifeModuleRoot);
                            objLifeModuleRoot.Expand();
                        }
                        objParentNode = objLifeModuleRoot;
                        break;
                }

                if (objParentNode != null)
                {
                    if (blnSingleAdd)
                    {
                        TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
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
                        treQualities.SelectedNode = objNode;
                    }
                    else
                        objParentNode.Nodes.Add(objNode);
                }
            }
        }

        /// <summary>
        /// Refreshes all the names of qualities in the nodes
        /// </summary>
        /// <param name="treQualities">Treeview to insert the qualities into.</param>
        protected void RefreshQualityNames(TreeView treQualities)
        {
            TreeNode objSelectedNode = treQualities.SelectedNode;
            foreach (TreeNode objQualityTypeNode in treQualities.Nodes)
            {
                foreach (TreeNode objQualityNode in objQualityTypeNode.Nodes)
                {
                    objQualityNode.Text = ((Quality)objQualityNode.Tag).DisplayName(GlobalOptions.Instance.CultureInfo, GlobalOptions.Language);
                }
            }
            treQualities.SortCustomAlphabetically(objSelectedNode?.Tag);
        }

        /// <summary>
        /// Method for removing old <addqualities /> nodes from existing characters.
        /// </summary>
        /// <param name="objNodeList">XmlNode to load. Expected to be addqualities/addquality</param>
        protected void RemoveAddedQualities(XmlNodeList objNodeList)
        {
            if (objNodeList == null || objNodeList.Count == 0)
                return;
            foreach (XmlNode objNode in objNodeList)
            {
                Quality objQuality = _objCharacter.Qualities.FirstOrDefault(x => x.Name == objNode.InnerText);
                if (objQuality != null)
                {
                    _objCharacter.Qualities.Remove(objQuality);
                    ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.CritterPower, objQuality.InternalId);
                }
            }
        }
        #region Locations        
        protected void RefreshArmorLocations(TreeView treArmor, ContextMenuStrip cmsArmorLocation, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            string strSelectedId = (treArmor.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
            
            TreeNode nodRoot = treArmor.FindNode("Node_SelectedImprovements", false);
            RefreshLocation(treArmor, nodRoot, cmsArmorLocation, notifyCollectionChangedEventArgs, _objCharacter.ArmorLocations, strSelectedId, "Node_SelectedArmor");
        }

        protected void RefreshGearLocations(TreeView treGear, ContextMenuStrip cmsGearLocation, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            string strSelectedId = (treGear.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode nodRoot = treGear.FindNode("Node_SelectedGear", false);
            RefreshLocation(treGear, nodRoot, cmsGearLocation, notifyCollectionChangedEventArgs, _objCharacter.GearLocations, strSelectedId, "Node_SelectedGear");
        }

        protected void RefreshVehicleLocations(TreeView treVehicles, ContextMenuStrip cmsVehicleLocation, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            TreeNode nodRoot = treVehicles.FindNode("Node_SelectedVehicles", false);
            string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
            RefreshLocation(treVehicles, nodRoot, cmsVehicleLocation, notifyCollectionChangedEventArgs, _objCharacter.VehicleLocations, strSelectedId, "Node_SelectedVehicles");
        }
        protected void RefreshLocationsInVehicle(TreeView treVehicles, Vehicle objVehicle, ContextMenuStrip cmsVehicleLocation, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode nodRoot = treVehicles.FindNodeByTag(objVehicle);
            RefreshLocation(treVehicles, nodRoot, cmsVehicleLocation, funcOffset, notifyCollectionChangedEventArgs,
                objVehicle.Locations, strSelectedId, "Node_SelectedVehicles", false);
        }

        protected void RefreshWeaponLocations(TreeView treWeapons, ContextMenuStrip cmsWeaponLocation, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            string strSelectedId = (treWeapons.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode nodRoot = treWeapons.FindNode("Node_SelectedWeapons", false);
            RefreshLocation(treWeapons, nodRoot, cmsWeaponLocation, notifyCollectionChangedEventArgs, _objCharacter.WeaponLocations, strSelectedId, "Node_SelectedWeapons");
        }

        protected void RefreshCustomImprovementLocations(TreeView treImprovements, ContextMenuStrip cmsImprovementLocation, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            string strSelectedId = (treImprovements.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode nodRoot = treImprovements.FindNode("Node_SelectedImprovements", false);

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (string strLocation in notifyCollectionChangedEventArgs.NewItems)
                        {
                            TreeNode objLocation = new TreeNode
                            {
                                Tag = strLocation,
                                Text = strLocation,
                                ContextMenuStrip = cmsImprovementLocation
                            };
                            treImprovements.Nodes.Insert(intNewIndex, objLocation);
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (string strLocation in notifyCollectionChangedEventArgs.OldItems)
                        {
                            TreeNode objNode = treImprovements.FindNodeByTag(strLocation, false);
                            if (objNode != null)
                            {
                                objNode.Remove();
                                if (objNode.Nodes.Count > 0)
                                {
                                    if (nodRoot == null)
                                    {
                                        nodRoot = new TreeNode
                                        {
                                            Tag = "Node_SelectedImprovements",
                                            Text = LanguageManager.GetString("Node_SelectedImprovements", GlobalOptions.Language)
                                        };
                                        treImprovements.Nodes.Insert(0, nodRoot);
                                    }
                                    for (int i = objNode.Nodes.Count - 1; i >= 0; --i)
                                    {
                                        TreeNode nodImprovement = objNode.Nodes[i];
                                        nodImprovement.Remove();
                                        nodRoot.Nodes.Add(nodImprovement);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        int intNewItemsIndex = 0;
                        foreach (string strLocation in notifyCollectionChangedEventArgs.OldItems)
                        {
                            TreeNode objNode = treImprovements.FindNodeByTag(strLocation, false);
                            if (objNode != null)
                            {
                                if (notifyCollectionChangedEventArgs.NewItems[intNewItemsIndex] is string objNewLocation)
                                {
                                    objNode.Tag = objNewLocation;
                                    objNode.Text = objNewLocation;
                                }
                                intNewItemsIndex += 1;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        List<Tuple<string, TreeNode>> lstMoveNodes = new List<Tuple<string, TreeNode>>();
                        foreach (string strLocation in notifyCollectionChangedEventArgs.OldItems)
                        {
                            TreeNode objLocation = treImprovements.FindNode(strLocation, false);
                            if (objLocation != null)
                            {
                                lstMoveNodes.Add(new Tuple<string, TreeNode>(strLocation, objLocation));
                                objLocation.Remove();
                            }
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (string strLocation in notifyCollectionChangedEventArgs.NewItems)
                        {
                            Tuple<string, TreeNode> objLocationTuple = lstMoveNodes.FirstOrDefault(x => x.Item1 == strLocation);
                            if (objLocationTuple != null)
                            {
                                treImprovements.Nodes.Insert(intNewIndex, objLocationTuple.Item2);
                                intNewIndex += 1;
                                lstMoveNodes.Remove(objLocationTuple);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        foreach (string strLocation in _objCharacter.ImprovementGroups)
                        {
                            TreeNode objLocation = treImprovements.FindNode(strLocation, false);
                            if (objLocation != null)
                            {
                                objLocation.Remove();
                                if (objLocation.Nodes.Count > 0)
                                {
                                    if (nodRoot == null)
                                    {
                                        nodRoot = new TreeNode
                                        {
                                            Tag = "Node_SelectedImprovements",
                                            Text = LanguageManager.GetString("Node_SelectedImprovements", GlobalOptions.Language)
                                        };
                                        treImprovements.Nodes.Insert(0, nodRoot);
                                    }
                                    for (int i = objLocation.Nodes.Count - 1; i >= 0; --i)
                                    {
                                        TreeNode nodImprovement = objLocation.Nodes[i];
                                        nodImprovement.Remove();
                                        nodRoot.Nodes.Add(nodImprovement);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            treImprovements.SelectedNode = treImprovements.FindNode(strSelectedId);
        }

        private void RefreshLocation(TreeView treSelected, TreeNode nodRoot, ContextMenuStrip cmsLocation,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs,
            ObservableCollection<Location> collection, string strSelectedId, string strNodeName)
        {
            RefreshLocation(treSelected, nodRoot, cmsLocation, () => 0, notifyCollectionChangedEventArgs, collection, strSelectedId, strNodeName);
        }

        private void RefreshLocation(TreeView treSelected, TreeNode nodRoot, ContextMenuStrip cmsLocation,
            Func<int> funcOffset,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs,
            ObservableCollection<Location> collection, string strSelectedId, string strNodeName,
            bool rootSibling = true)
        {
            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                    if (funcOffset != null)
                        intNewIndex += funcOffset.Invoke();
                        foreach (Location objLocation in notifyCollectionChangedEventArgs.NewItems)
                    {
                        if (rootSibling)
                        {
                            treSelected.Nodes.Insert(intNewIndex, objLocation.CreateTreeNode(cmsLocation));
                        }
                        else
                        {
                            nodRoot.Nodes.Insert(intNewIndex, objLocation.CreateTreeNode(cmsLocation));
                        }

                        intNewIndex += 1;
                    }
                }
                    break;
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (Location objLocation in notifyCollectionChangedEventArgs.OldItems)
                    {
                        TreeNode nodLocation = treSelected.FindNodeByTag(objLocation, false);
                        if (nodLocation == null) continue;
                        if (nodLocation.Nodes.Count > 0)
                        {
                            if (nodRoot == null)
                            {
                                nodRoot = new TreeNode
                                {
                                    Tag = strNodeName,
                                    Text = LanguageManager.GetString(strNodeName, GlobalOptions.Language)
                                };
                                treSelected.Nodes.Insert(0, nodRoot);
                            }

                            for (int i = nodLocation.Nodes.Count - 1; i >= 0; --i)
                            {
                                TreeNode nodWeapon = nodLocation.Nodes[i];
                                nodWeapon.Remove();
                                nodRoot.Nodes.Add(nodWeapon);
                            }
                        }
                        nodLocation.Remove();
                    }
                }
                    break;
                case NotifyCollectionChangedAction.Replace:
                {
                    int intNewItemsIndex = 0;
                    foreach (Location objLocation in notifyCollectionChangedEventArgs.OldItems)
                    {
                        TreeNode objNode = treSelected.FindNodeByTag(objLocation, false);
                        if (objNode != null)
                        {
                            if (notifyCollectionChangedEventArgs.NewItems[intNewItemsIndex] is Location objNewLocation)
                            {
                                objNode.Tag = objNewLocation;
                                objNode.Text = objNewLocation.DisplayName(GlobalOptions.Language);
                            }

                            intNewItemsIndex += 1;
                        }
                    }
                }
                    break;
                case NotifyCollectionChangedAction.Move:
                {
                    List<Tuple<Location, TreeNode>> lstMoveNodes = new List<Tuple<Location, TreeNode>>();
                    foreach (Location objLocation in notifyCollectionChangedEventArgs.OldItems)
                    {
                        TreeNode objNode = treSelected.FindNodeByTag(objLocation, false);
                        if (objLocation != null)
                        {
                            lstMoveNodes.Add(new Tuple<Location, TreeNode>(objLocation, objNode));
                            objLocation.Remove(_objCharacter, false);
                        }
                    }

                    int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                    foreach (Location objLocation in notifyCollectionChangedEventArgs.NewItems)
                    {
                        Tuple<Location, TreeNode> objLocationTuple = lstMoveNodes.FirstOrDefault(x => x.Item1 == objLocation);
                        if (objLocationTuple != null)
                        {
                            treSelected.Nodes.Insert(intNewIndex, objLocationTuple.Item2);
                            intNewIndex += 1;
                            lstMoveNodes.Remove(objLocationTuple);
                        }
                    }
                }
                    break;
                case NotifyCollectionChangedAction.Reset:
                {
                    foreach (Location objLocation in _objCharacter.WeaponLocations)
                    {
                        TreeNode nodLocation = treSelected.FindNode(objLocation?.InternalId, false);
                        if (nodLocation == null)
                            continue;
                        if (nodLocation.Nodes.Count > 0)
                        {
                            if (nodRoot == null)
                            {
                                nodRoot = new TreeNode
                                {
                                    Tag = strNodeName,
                                    Text = LanguageManager.GetString(strNodeName, GlobalOptions.Language)
                                };
                                treSelected.Nodes.Insert(0, nodRoot);
                            }

                            for (int i = nodLocation.Nodes.Count - 1; i >= 0; --i)
                            {
                                TreeNode nodWeapon = nodLocation.Nodes[i];
                                nodWeapon.Remove();
                                nodRoot.Nodes.Add(nodWeapon);
                            }
                        }

                        objLocation?.Remove(_objCharacter);
                    }
                }
                    break;
            }

            treSelected.SelectedNode = treSelected.FindNode(strSelectedId);
        }
        #endregion

        protected void RefreshWeapons(TreeView treWeapons, ContextMenuStrip cmsWeaponLocation, ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            string strSelectedId = (treWeapons.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode nodRoot = null;

            if (notifyCollectionChangedEventArgs == null)
            {
                treWeapons.Nodes.Clear();

                // Start by populating Locations.
                foreach (Location objLocation in CharacterObject.WeaponLocations)
                {
                    treWeapons.Nodes.Add(objLocation.CreateTreeNode(cmsWeaponLocation));
                }
                foreach (Weapon objWeapon in CharacterObject.Weapons)
                {
                    AddToTree(objWeapon, -1, false);
                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treWeapons, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                }



                treWeapons.SelectedNode = treWeapons.FindNode(strSelectedId);
            }
            else
            {
                nodRoot = treWeapons.FindNode("Node_SelectedWeapons", false);

                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objWeapon, intNewIndex);
                                intNewIndex += 1;
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treWeapons, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treWeapons);
                                treWeapons.FindNode(objWeapon.InternalId)?.Remove();
                            }
                            if (nodRoot != null && nodRoot.Nodes.Count == 0)
                            {
                                nodRoot.Remove();
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treWeapons);
                                treWeapons.FindNode(objWeapon.InternalId)?.Remove();
                            }
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objWeapon, intNewIndex);
                                intNewIndex += 1;
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treWeapons, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                            }
                            if (nodRoot != null && nodRoot.Nodes.Count == 0)
                            {
                                nodRoot.Remove();
                            }
                            treWeapons.SelectedNode = treWeapons.FindNode(strSelectedId);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        {
                            foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treWeapons.FindNode(objWeapon.InternalId)?.Remove();
                            }
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Weapon objWeapon in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objWeapon, intNewIndex);
                                intNewIndex += 1;
                            }
                            if (nodRoot != null && nodRoot.Nodes.Count == 0)
                            {
                                nodRoot.Remove();
                            }
                            treWeapons.SelectedNode = treWeapons.FindNode(strSelectedId);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshWeapons(treWeapons, cmsWeaponLocation, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                        }
                        break;
                }
            }

            void AddToTree(Weapon objWeapon, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objWeapon.CreateTreeNode(cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                if (objNode == null)
                    return;
                TreeNode nodParent = null;
                if (objWeapon.Location != null)
                {
                    nodParent = treWeapons.FindNode(objWeapon.Location.InternalId, false);
                }
                if (nodParent == null)
                {
                    if (nodRoot == null)
                    {
                        nodRoot = new TreeNode
                        {
                            Tag = "Node_SelectedWeapons",
                            Text = LanguageManager.GetString("Node_SelectedWeapons", GlobalOptions.Language)
                        };
                        treWeapons.Nodes.Insert(0, nodRoot);
                    }
                    nodParent = nodRoot;
                }

                if (intIndex >= 0)
                    nodParent.Nodes.Insert(intIndex, objNode);
                else
                    nodParent.Nodes.Add(objNode);
                nodParent.Expand();
                if (blnSingleAdd)
                    treWeapons.SelectedNode = objNode;
            }
        }
        
        protected void RefreshArmor(TreeView treArmor, ContextMenuStrip cmsArmorLocation, ContextMenuStrip cmsArmor, ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            string strSelectedId = (treArmor.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode nodRoot = null;

            if (notifyCollectionChangedEventArgs == null)
            {
                treArmor.Nodes.Clear();

                // Start by adding Locations.
                foreach (Location objLocation in CharacterObject.ArmorLocations)
                {
                    treArmor.Nodes.Add(objLocation.CreateTreeNode(cmsArmorLocation));
                }

                // Add Armor.
                foreach (Armor objArmor in CharacterObject.Armor)
                {
                    AddToTree(objArmor, -1, false);
                    objArmor.ArmorMods.AddTaggedCollectionChanged(treArmor, (x, y) => RefreshArmorMods(treArmor, objArmor, cmsArmorMod, cmsArmorGear, y));
                    objArmor.Gear.AddTaggedCollectionChanged(treArmor, (x, y) => objArmor.RefreshChildrenGears(treArmor, cmsArmorGear, () => objArmor.ArmorMods.Count, y));
                    foreach (Gear objGear in objArmor.Gear)
                        objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                    foreach (ArmorMod objArmorMod in objArmor.ArmorMods)
                    {
                        objArmorMod.Gear.AddTaggedCollectionChanged(treArmor, (x, y) => objArmorMod.RefreshChildrenGears(treArmor, cmsArmorGear, null, y));
                        foreach (Gear objGear in objArmorMod.Gear)
                            objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                    }
                }

                treArmor.SelectedNode = treArmor.FindNode(strSelectedId);
            }
            else
            {
                nodRoot = treArmor.FindNode("Node_SelectedArmor", false);

                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Armor objArmor in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objArmor, intNewIndex);
                                intNewIndex += 1;
                                objArmor.ArmorMods.AddTaggedCollectionChanged(treArmor, (x, y) => RefreshArmorMods(treArmor, objArmor, cmsArmorMod, cmsArmorGear, y));
                                objArmor.Gear.AddTaggedCollectionChanged(treArmor, (x, y) => objArmor.RefreshChildrenGears(treArmor, cmsArmorGear, () => objArmor.ArmorMods.Count, y));
                                foreach (Gear objGear in objArmor.Gear)
                                    objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                                foreach (ArmorMod objArmorMod in objArmor.ArmorMods)
                                {
                                    objArmorMod.Gear.AddTaggedCollectionChanged(treArmor, (x, y) => objArmorMod.RefreshChildrenGears(treArmor, cmsArmorGear, null, y));
                                    foreach (Gear objGear in objArmorMod.Gear)
                                        objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Armor objArmor in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objArmor.ArmorMods.RemoveTaggedCollectionChanged(treArmor);
                                objArmor.Gear.RemoveTaggedCollectionChanged(treArmor);
                                foreach (Gear objGear in objArmor.Gear)
                                    objGear.SetupChildrenGearsCollectionChanged(false, treArmor);
                                foreach (ArmorMod objArmorMod in objArmor.ArmorMods)
                                {
                                    objArmorMod.Gear.RemoveTaggedCollectionChanged(treArmor);
                                    foreach (Gear objGear in objArmorMod.Gear)
                                        objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                                }
                                treArmor.FindNode(objArmor.InternalId)?.Remove();
                            }
                            if (nodRoot != null && nodRoot.Nodes.Count == 0)
                            {
                                nodRoot.Remove();
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Armor objArmor in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objArmor.ArmorMods.RemoveTaggedCollectionChanged(treArmor);
                                objArmor.Gear.RemoveTaggedCollectionChanged(treArmor);
                                foreach (Gear objGear in objArmor.Gear)
                                    objGear.SetupChildrenGearsCollectionChanged(false, treArmor);
                                foreach (ArmorMod objArmorMod in objArmor.ArmorMods)
                                {
                                    objArmorMod.Gear.RemoveTaggedCollectionChanged(treArmor);
                                    foreach (Gear objGear in objArmorMod.Gear)
                                        objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                                }
                                treArmor.FindNode(objArmor.InternalId)?.Remove();
                            }
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Armor objArmor in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objArmor, intNewIndex);
                                intNewIndex += 1;
                                objArmor.ArmorMods.AddTaggedCollectionChanged(treArmor, (x, y) => RefreshArmorMods(treArmor, objArmor, cmsArmorMod, cmsArmorGear, y));
                                objArmor.Gear.AddTaggedCollectionChanged(treArmor, (x, y) => objArmor.RefreshChildrenGears(treArmor, cmsArmorGear, () => objArmor.ArmorMods.Count, y));
                                foreach (Gear objGear in objArmor.Gear)
                                    objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                                foreach (ArmorMod objArmorMod in objArmor.ArmorMods)
                                {
                                    objArmorMod.Gear.AddTaggedCollectionChanged(treArmor, (x, y) => objArmorMod.RefreshChildrenGears(treArmor, cmsArmorGear, null, y));
                                    foreach (Gear objGear in objArmorMod.Gear)
                                        objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                                }
                            }
                            if (nodRoot != null && nodRoot.Nodes.Count == 0)
                            {
                                nodRoot.Remove();
                            }
                            treArmor.SelectedNode = treArmor.FindNode(strSelectedId);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        {
                            foreach (Armor objArmor in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treArmor.FindNode(objArmor.InternalId)?.Remove();
                            }
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Armor objArmor in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objArmor, intNewIndex);
                                intNewIndex += 1;
                            }
                            if (nodRoot != null && nodRoot.Nodes.Count == 0)
                            {
                                nodRoot.Remove();
                            }
                            treArmor.SelectedNode = treArmor.FindNode(strSelectedId);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshArmor(treArmor, cmsArmorLocation, cmsArmor, cmsArmorMod, cmsArmorGear);
                        }
                        break;
                }
            }

            void AddToTree(Armor objArmor, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objArmor.CreateTreeNode(cmsArmor, cmsArmorMod, cmsArmorGear);
                if (objNode == null)
                    return;
                TreeNode nodParent = null;
                if (objArmor.Location != null)
                {
                    nodParent = treArmor.FindNode(objArmor.Location.InternalId, false);
                }
                if (nodParent == null)
                {
                    if (nodRoot == null)
                    {
                        nodRoot = new TreeNode
                        {
                            Tag = "Node_SelectedArmor",
                            Text = LanguageManager.GetString("Node_SelectedArmor", GlobalOptions.Language)
                        };
                        treArmor.Nodes.Insert(0, nodRoot);
                    }
                    nodParent = nodRoot;
                }

                if (intIndex >= 0)
                    nodParent.Nodes.Insert(intIndex, objNode);
                else
                    nodParent.Nodes.Add(objNode);
                nodParent.Expand();
                if (blnSingleAdd)
                    treArmor.SelectedNode = objNode;
            }
        }

        protected void RefreshArmorMods(TreeView treArmor, Armor objArmor, ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;
            TreeNode nodArmor = treArmor.FindNode(objArmor.InternalId);
            if (nodArmor == null)
                return;

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (ArmorMod objArmorMod in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objArmorMod, intNewIndex);
                            objArmorMod.Gear.AddTaggedCollectionChanged(treArmor, (x, y) => objArmorMod.RefreshChildrenGears(treArmor, cmsArmorGear, null, y));
                            foreach (Gear objGear in objArmorMod.Gear)
                                objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (ArmorMod objArmorMod in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objArmorMod.Gear.RemoveTaggedCollectionChanged(treArmor);
                            foreach (Gear objGear in objArmorMod.Gear)
                                objGear.SetupChildrenGearsCollectionChanged(false, treArmor);
                            nodArmor.FindNode(objArmorMod.InternalId)?.Remove();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (treArmor.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (ArmorMod objArmorMod in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objArmorMod.Gear.RemoveTaggedCollectionChanged(treArmor);
                            foreach (Gear objGear in objArmorMod.Gear)
                                objGear.SetupChildrenGearsCollectionChanged(false, treArmor);
                            nodArmor.FindNode(objArmorMod.InternalId)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (ArmorMod objArmorMod in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objArmorMod, intNewIndex);
                            objArmorMod.Gear.AddTaggedCollectionChanged(treArmor, (x, y) => objArmorMod.RefreshChildrenGears(treArmor, cmsArmorGear, null, y));
                            foreach (Gear objGear in objArmorMod.Gear)
                                objGear.SetupChildrenGearsCollectionChanged(true, treArmor, cmsArmorGear);
                            intNewIndex += 1;
                        }
                        treArmor.SelectedNode = treArmor.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId = (treArmor.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (ArmorMod objArmorMod in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodArmor.FindNode(objArmorMod.InternalId)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (ArmorMod objArmorMod in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objArmorMod, intNewIndex);
                            intNewIndex += 1;
                        }
                        treArmor.SelectedNode = treArmor.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        nodArmor.Nodes.Clear();
                    }
                    break;
            }

            void AddToTree(ArmorMod objArmorMod, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objArmorMod.CreateTreeNode(cmsArmorMod, cmsArmorGear);
                if (objNode == null)
                    return;
                if (intIndex >= 0)
                    nodArmor.Nodes.Insert(intIndex, objNode);
                else
                    nodArmor.Nodes.Add(objNode);
                nodArmor.Expand();
                if (blnSingleAdd)
                    treArmor.SelectedNode = objNode;
            }
        }

        protected void RefreshGears(TreeView treGear, ContextMenuStrip cmsGearLocation, ContextMenuStrip cmsGear, bool blnCommlinksOnly, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            string strSelectedId = (treGear.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode nodRoot = null;
            
            if (notifyCollectionChangedEventArgs == null)
            {
                treGear.Nodes.Clear();

                // Start by populating Locations.
                foreach (Location objLocation in CharacterObject.GearLocations)
                {
                    treGear.Nodes.Add(objLocation.CreateTreeNode(cmsGearLocation));
                }

                // Add Gear.
                foreach (Gear objGear in CharacterObject.Gear)
                {
                    AddToTree(objGear, -1, false);
                    objGear.SetupChildrenGearsCollectionChanged(true, treGear, cmsGear);
                }

                treGear.SelectedNode = treGear.FindNode(strSelectedId);
            }
            else
            {
                nodRoot = treGear.FindNode("Node_SelectedGear", false);

                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objGear, intNewIndex);
                                objGear.SetupChildrenGearsCollectionChanged(true, treGear, cmsGear);
                                intNewIndex += 1;
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objGear.SetupChildrenGearsCollectionChanged(false, treGear);
                                treGear.FindNodeByTag(objGear)?.Remove();
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objGear.SetupChildrenGearsCollectionChanged(false, treGear, cmsGear);
                                treGear.FindNodeByTag(objGear)?.Remove();
                            }
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objGear, intNewIndex);
                                objGear.Children.AddTaggedCollectionChanged(treGear, (x, y) => objGear.RefreshChildrenGears(treGear, cmsGear, null, y));
                                objGear.SetupChildrenGearsCollectionChanged(true, treGear, cmsGear);
                                intNewIndex += 1;
                            }
                            treGear.SelectedNode = treGear.FindNode(strSelectedId);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        {
                            foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treGear.FindNodeByTag(objGear)?.Remove();
                            }
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objGear, intNewIndex);
                                intNewIndex += 1;
                            }
                            treGear.SelectedNode = treGear.FindNode(strSelectedId);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshGears(treGear, cmsGearLocation, cmsGear, blnCommlinksOnly);
                        }
                        break;
                }
            }

            void AddToTree(Gear objGear, int intIndex = -1, bool blnSingleAdd = true)
            {
                if (blnCommlinksOnly && !objGear.IsCommlink)
                    return;

                TreeNode objNode = objGear.CreateTreeNode(cmsGear);
                if (objNode == null)
                    return;
                TreeNode nodParent = null;
                if (objGear.Location != null)
                {
                    nodParent = treGear.FindNodeByTag(objGear.Location, false);
                }
                if (nodParent == null)
                {
                    if (nodRoot == null)
                    {
                        nodRoot = new TreeNode
                        {
                            Tag = "Node_SelectedGear",
                            Text = LanguageManager.GetString("Node_SelectedGear", GlobalOptions.Language)
                        };
                        treGear.Nodes.Insert(0, nodRoot);
                    }
                    nodParent = nodRoot;
                }

                if (intIndex >= 0)
                    nodParent.Nodes.Insert(intIndex, objNode);
                else
                    nodParent.Nodes.Add(objNode);
                nodParent.Expand();
                if (blnSingleAdd)
                    treGear.SelectedNode = objNode;
            }
        }
        
        protected void RefreshDrugs(TreeView treGear, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            string strSelectedId = (treGear.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode nodRoot = null;

            if (notifyCollectionChangedEventArgs == null)
            {
                treGear.Nodes.Clear();

                // Add Gear.
                foreach (Drug d in CharacterObject.Drugs)
                {
                    AddToTree(d, -1, false);
                }

                treGear.SelectedNode = treGear.FindNode(strSelectedId);
            }
            else
            {
                nodRoot = treGear.FindNode("Node_SelectedDrugs", false);

                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Drug d in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(d, intNewIndex);
                                intNewIndex += 1;
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Drug d in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treGear.FindNodeByTag(d)?.Remove();
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Drug d in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treGear.FindNodeByTag(d)?.Remove();
                            }
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Drug d in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(d, intNewIndex);
                                intNewIndex += 1;
                            }
                            treGear.SelectedNode = treGear.FindNode(strSelectedId);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        {
                            foreach (Drug d in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treGear.FindNodeByTag(d)?.Remove();
                            }
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Drug d in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(d, intNewIndex);
                                intNewIndex += 1;
                            }
                            treGear.SelectedNode = treGear.FindNode(strSelectedId);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshDrugs(treGear);
                        }
                        break;
                }
            }

            void AddToTree(Drug objGear, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objGear.CreateTreeNode();
                if (objNode == null)
                    return;
                if (nodRoot == null)
                {
                    nodRoot = new TreeNode
                    {
                        Tag = "Node_SelectedDrugs",
                        Text = LanguageManager.GetString("Node_SelectedDrugs", GlobalOptions.Language)
                    };
                    treGear.Nodes.Insert(0, nodRoot);
                }

                if (intIndex >= 0)
                    nodRoot.Nodes.Insert(intIndex, objNode);
                else
                    nodRoot.Nodes.Add(objNode);
                nodRoot.Expand();
                if (blnSingleAdd)
                    treGear.SelectedNode = objNode;
            }
        }
        protected void RefreshCyberware(TreeView treCyberware, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            string strSelectedId = (treCyberware.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode objCyberwareRoot = null;
            TreeNode objBiowareRoot = null;
            TreeNode objModularRoot = null;
            TreeNode objHoleNode = null;
            TreeNode objAntiHoleNode = null;

            if (notifyCollectionChangedEventArgs == null)
            {
                treCyberware.Nodes.Clear();

                foreach (Cyberware objCyberware in CharacterObject.Cyberware)
                {
                    AddToTree(objCyberware, false);
                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treCyberware, cmsCyberware, cmsCyberwareGear);
                }

                treCyberware.SortCustomAlphabetically(strSelectedId);
            }
            else
            {
                objCyberwareRoot = treCyberware.FindNode("Node_SelectedCyberware", false);
                objBiowareRoot = treCyberware.FindNode("Node_SelectedBioware", false);
                objModularRoot = treCyberware.FindNode("Node_UnequippedModularCyberware", false);
                objHoleNode = treCyberware.FindNode(Cyberware.EssenceHoleGUID.ToString("D"), false);
                objAntiHoleNode = treCyberware.FindNode(Cyberware.EssenceAntiHoleGUID.ToString("D"), false);
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objCyberware);
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treCyberware, cmsCyberware, cmsCyberwareGear);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objCyberware.SetupChildrenCyberwareCollectionChanged(false, treCyberware);
                                TreeNode objNode = treCyberware.FindNodeByTag(objCyberware);
                                if (objNode != null)
                                {
                                    TreeNode objParent = objNode.Parent;
                                    objNode.Remove();
                                    if (objParent != null && objParent.Level == 0 && objParent.Nodes.Count == 0)
                                        objParent.Remove();
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParentNodes = new List<TreeNode>();

                            foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objCyberware.SetupChildrenCyberwareCollectionChanged(false, treCyberware);
                                TreeNode objNode = treCyberware.FindNodeByTag(objCyberware);
                                if (objNode != null)
                                {
                                    TreeNode objParent = objNode.Parent;
                                    objNode.Remove();
                                    if (objParent != null && objParent.Level == 0)
                                        lstOldParentNodes.Add(objParent);
                                }
                            }
                            foreach (Cyberware objCyberware in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objCyberware);
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treCyberware, cmsCyberware, cmsCyberwareGear);
                            }
                            foreach (TreeNode objOldParent in lstOldParentNodes)
                            {
                                if (objOldParent.Nodes.Count == 0)
                                    objOldParent.Remove();
                            }
                            treCyberware.SelectedNode = treCyberware.FindNode(strSelectedId);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshCyberware(treCyberware, cmsCyberware, cmsCyberwareGear);
                        }
                        break;
                }
            }

            void AddToTree(Cyberware objCyberware, bool blnSingleAdd = true)
            {
                if (objCyberware.SourceID == Cyberware.EssenceHoleGUID)
                {
                    if (objHoleNode == null)
                    {
                        objHoleNode = objCyberware.CreateTreeNode(null, null);
                        treCyberware.Nodes.Insert(3, objHoleNode);
                    }
                    if (blnSingleAdd)
                        treCyberware.SelectedNode = objHoleNode;
                    return;
                }
                if (objCyberware.SourceID == Cyberware.EssenceAntiHoleGUID)
                {
                    if (objAntiHoleNode == null)
                    {
                        objAntiHoleNode = objCyberware.CreateTreeNode(null, null);
                        treCyberware.Nodes.Insert(3, objAntiHoleNode);
                    }
                    if (blnSingleAdd)
                        treCyberware.SelectedNode = objAntiHoleNode;
                    return;
                }

                TreeNode objNode = objCyberware.CreateTreeNode(cmsCyberware, cmsCyberwareGear);
                if (objNode == null)
                    return;

                TreeNode nodParent = null;
                if (objCyberware.SourceType == Improvement.ImprovementSource.Cyberware)
                {
                    if (objCyberware.IsModularCurrentlyEquipped)
                    {
                        if (objCyberwareRoot == null)
                        {
                            objCyberwareRoot = new TreeNode
                            {
                                Tag = "Node_SelectedCyberware",
                                Text = LanguageManager.GetString("Node_SelectedCyberware", GlobalOptions.Language)
                            };
                            treCyberware.Nodes.Insert(0, objCyberwareRoot);
                            objCyberwareRoot.Expand();
                        }
                        nodParent = objCyberwareRoot;
                    }
                    else
                    {
                        if (objModularRoot == null)
                        {
                            objModularRoot = new TreeNode
                            {
                                Tag = "Node_UnequippedModularCyberware",
                                Text = LanguageManager.GetString("Node_UnequippedModularCyberware", GlobalOptions.Language)
                            };
                            treCyberware.Nodes.Insert(objBiowareRoot == null && objCyberwareRoot == null ? 0 :
                                (objBiowareRoot == null) != (objCyberwareRoot == null) ? 1 : 2, objModularRoot);
                            objModularRoot.Expand();
                        }
                        nodParent = objModularRoot;
                    }
                }
                else if (objCyberware.SourceType == Improvement.ImprovementSource.Bioware)
                {
                    if (objBiowareRoot == null)
                    {
                        objBiowareRoot = new TreeNode
                        {
                            Tag = "Node_SelectedBioware",
                            Text = LanguageManager.GetString("Node_SelectedBioware", GlobalOptions.Language)
                        };
                        treCyberware.Nodes.Insert(objCyberwareRoot == null ? 0 : 1, objBiowareRoot);
                        objBiowareRoot.Expand();
                    }
                    nodParent = objBiowareRoot;
                }

                if (nodParent != null)
                {
                    if (blnSingleAdd)
                    {
                        TreeNodeCollection lstParentNodeChildren = nodParent.Nodes;
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
                        treCyberware.SelectedNode = objNode;
                    }
                    else
                        nodParent.Nodes.Add(objNode);
                }
            }
        }

        protected void RefreshVehicleMods(TreeView treVehicles, IHasInternalId objParent, ContextMenuStrip cmsVehicleMod, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            TreeNode nodParent = treVehicles.FindNodeByTag(objParent);
            if (nodParent == null)
                return;

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objVehicleMod, intNewIndex);
                            objVehicleMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicleMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsCyberwareGear, null, y));
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear);
                            objVehicleMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicleMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objVehicleMod.Cyberware.Count, y));
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objVehicleMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles);
                            objVehicleMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            nodParent.FindNodeByTag(objVehicleMod)?.Remove();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objVehicleMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                            objVehicleMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            nodParent.FindNodeByTag(objVehicleMod)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objVehicleMod, intNewIndex);
                            objVehicleMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicleMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsCyberwareGear, null, y));
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear);
                            objVehicleMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicleMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objVehicleMod.Cyberware.Count, y));
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            intNewIndex += 1;
                        }
                        treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodParent.FindNodeByTag(objVehicleMod)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objVehicleMod, intNewIndex);
                            intNewIndex += 1;
                        }
                        treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        nodParent.Nodes.Clear();
                    }
                    break;
            }

            void AddToTree(VehicleMod objVehicleMod, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objVehicleMod.CreateTreeNode(cmsVehicleMod, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                if (objNode == null)
                    return;

                if (intIndex >= 0)
                    nodParent.Nodes.Insert(intIndex, objNode);
                else
                    nodParent.Nodes.Add(objNode);
                nodParent.Expand();
                if (blnSingleAdd)
                    treVehicles.SelectedNode = objNode;
            }
        }

        protected void RefreshVehicleWeaponMounts(TreeView treVehicles, IHasInternalId objParent, ContextMenuStrip cmsVehicleWeaponMount, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, ContextMenuStrip cmsVehicleMod, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            TreeNode nodVehicleParent = treVehicles.FindNodeByTag(objParent);
            if (nodVehicleParent == null)
                return;
            TreeNode nodParent = nodVehicleParent.FindNode("String_WeaponMounts", false);

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeaponMount, intNewIndex);
                            objWeaponMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                (x, y) => RefreshVehicleMods(treVehicles, objWeaponMount, cmsVehicleMod, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, null, y));
                            objWeaponMount.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objWeaponMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objWeaponMount.Mods.Count, y));
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsCyberwareGear, null, y));
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear);
                                objMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y));
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            }
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objWeaponMount.Mods.RemoveTaggedCollectionChanged(treVehicles);
                            objWeaponMount.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles);
                                objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            }
                            if (nodParent != null)
                            {
                                nodParent.FindNodeByTag(objWeaponMount)?.Remove();
                                if (nodParent.Nodes.Count == 0)
                                    nodParent.Remove();
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objWeaponMount.Mods.RemoveTaggedCollectionChanged(treVehicles);
                            objWeaponMount.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            }
                            nodParent?.FindNodeByTag(objWeaponMount)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeaponMount, intNewIndex);
                            objWeaponMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                (x, y) => RefreshVehicleMods(treVehicles, objWeaponMount, cmsVehicleMod, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, null, y));
                            objWeaponMount.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objWeaponMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objWeaponMount.Mods.Count, y));
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsCyberwareGear, null, y));
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles , cmsCyberware, cmsCyberwareGear);
                                objMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y));
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            }
                            intNewIndex += 1;
                        }
                        treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodParent?.FindNodeByTag(objWeaponMount)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeaponMount, intNewIndex);
                            intNewIndex += 1;
                        }
                        treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        nodParent?.Remove();
                    }
                    break;
            }

            void AddToTree(WeaponMount objWeaponMount, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objWeaponMount.CreateTreeNode(cmsVehicleWeaponMount, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsCyberware, cmsCyberwareGear, cmsVehicleMod);
                if (objNode == null)
                    return;

                if (nodParent == null)
                {
                    nodParent = new TreeNode
                    {
                        Tag = "String_WeaponMounts",
                        Text = LanguageManager.GetString("String_WeaponMounts", GlobalOptions.Language)
                    };
                    nodVehicleParent.Nodes.Insert(funcOffset?.Invoke() ?? 0, nodParent);
                    nodParent.Expand();
                }

                if (intIndex >= 0)
                    nodParent.Nodes.Insert(intIndex, objNode);
                else
                    nodParent.Nodes.Add(objNode);
                nodParent.Expand();
                if (blnSingleAdd)
                    treVehicles.SelectedNode = objNode;
            }
        }

        protected void RefreshVehicles(TreeView treVehicles, ContextMenuStrip cmsVehicleLocation, ContextMenuStrip cmsVehicle, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, ContextMenuStrip cmsVehicleGear, ContextMenuStrip cmsVehicleWeaponMount, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode nodRoot = null;

            if (notifyCollectionChangedEventArgs == null)
            {
                treVehicles.Nodes.Clear();

                // Start by populating Locations.
                foreach (Location objLocation in CharacterObject.VehicleLocations)
                {
                    treVehicles.Nodes.Add(objLocation.CreateTreeNode(cmsVehicleLocation));
                }

                // Add Vehicles.
                foreach (Vehicle objVehicle in CharacterObject.Vehicles)
                {
                    AddToTree(objVehicle, -1, false);
                    objVehicle.Mods.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshVehicleMods(treVehicles, objVehicle, cmsVehicle, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, null, y));
                    objVehicle.WeaponMounts.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshVehicleWeaponMounts(treVehicles, objVehicle, cmsVehicleWeaponMount, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsCyberware, cmsCyberwareGear, cmsVehicle, () => objVehicle.Mods.Count, y));
                    objVehicle.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicle.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objVehicle.Mods.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsCyberwareGear, null, y));
                        foreach (Cyberware objCyberware in objMod.Cyberware)
                            objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear);
                        objMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y));
                        foreach (Weapon objWeapon in objMod.Weapons)
                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                    }
                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                    {
                        objMount.Mods.AddTaggedCollectionChanged(treVehicles,
                            (x, y) => RefreshVehicleMods(treVehicles, objMount, cmsVehicle, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, null, y));
                        objMount.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMount.Mods.Count, y));
                        foreach (Weapon objWeapon in objMount.Weapons)
                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                        foreach (VehicleMod objMod in objMount.Mods)
                        {
                            foreach (Cyberware objCyberware in objMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear);
                            foreach (Weapon objWeapon in objMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                        }
                    }
                    foreach (Weapon objWeapon in objVehicle.Weapons)
                        objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                    objVehicle.Gear.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicle.RefreshChildrenGears(treVehicles, cmsVehicleGear, () => objVehicle.Mods.Count + objVehicle.Weapons.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                    foreach (Gear objGear in objVehicle.Gear)
                        objGear.SetupChildrenGearsCollectionChanged(true, treVehicles, cmsVehicleGear);
                    objVehicle.Locations.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshLocationsInVehicle(treVehicles, objVehicle, cmsVehicleLocation, () => objVehicle.Mods.Count + objVehicle.Weapons.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0) + objVehicle.Gear.Count(z => z.Location == null), y));
                }

                treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
            }
            else
            {
                nodRoot = treVehicles.FindNode("Node_SelectedVehicles", false);

                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objVehicle, intNewIndex);
                                objVehicle.Mods.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshVehicleMods(treVehicles, objVehicle, cmsVehicle, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, null, y));
                                objVehicle.WeaponMounts.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshVehicleWeaponMounts(treVehicles, objVehicle, cmsVehicleWeaponMount, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsCyberware, cmsCyberwareGear, cmsVehicle, () => objVehicle.Mods.Count, y));
                                objVehicle.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicle.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objVehicle.Mods.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                                foreach (VehicleMod objMod in objVehicle.Mods)
                                {
                                    objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsCyberwareGear, null, y));
                                    foreach (Cyberware objCyberware in objMod.Cyberware)
                                        objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear);
                                    objMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y));
                                    foreach (Weapon objWeapon in objMod.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                }
                                foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                                {
                                    objMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                        (x, y) => RefreshVehicleMods(treVehicles, objMount, cmsVehicle, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, null, y));
                                    objMount.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMount.Mods.Count, y));
                                    foreach (Weapon objWeapon in objMount.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                    foreach (VehicleMod objMod in objMount.Mods)
                                    {
                                        foreach (Cyberware objCyberware in objMod.Cyberware)
                                            objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear);
                                        foreach (Weapon objWeapon in objMod.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                    }
                                }
                                foreach (Weapon objWeapon in objVehicle.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                objVehicle.Gear.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicle.RefreshChildrenGears(treVehicles, cmsVehicleGear, () => objVehicle.Mods.Count + objVehicle.Weapons.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                                foreach (Gear objGear in objVehicle.Gear)
                                    objGear.SetupChildrenGearsCollectionChanged(true, treVehicles, cmsVehicleGear);
                                objVehicle.Locations.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshLocationsInVehicle(treVehicles, objVehicle, cmsVehicleLocation, () => objVehicle.Mods.Count + objVehicle.Weapons.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0) + objVehicle.Gear.Count(z => z.Location == null), y));
                                intNewIndex += 1;
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objVehicle.Mods.RemoveTaggedCollectionChanged(treVehicles);
                                objVehicle.WeaponMounts.RemoveTaggedCollectionChanged(treVehicles);
                                objVehicle.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (VehicleMod objMod in objVehicle.Mods)
                                {
                                    objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (Cyberware objCyberware in objMod.Cyberware)
                                        objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                    objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (Weapon objWeapon in objMod.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                }
                                foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                                {
                                    objMount.Mods.RemoveTaggedCollectionChanged(treVehicles);
                                    objMount.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (Weapon objWeapon in objMount.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                    foreach (VehicleMod objMod in objMount.Mods)
                                    {
                                        objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                        foreach (Cyberware objCyberware in objMod.Cyberware)
                                            objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                        objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                        foreach (Weapon objWeapon in objMod.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                    }
                                }
                                foreach (Weapon objWeapon in objVehicle.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                objVehicle.Gear.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Gear objGear in objVehicle.Gear)
                                    objGear.SetupChildrenGearsCollectionChanged(false, treVehicles);
                                objVehicle.Locations.RemoveTaggedCollectionChanged(treVehicles);
                                treVehicles.FindNodeByTag(objVehicle)?.Remove();
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objVehicle.Mods.RemoveTaggedCollectionChanged(treVehicles);
                                objVehicle.WeaponMounts.RemoveTaggedCollectionChanged(treVehicles);
                                objVehicle.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (VehicleMod objMod in objVehicle.Mods)
                                {
                                    objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (Cyberware objCyberware in objMod.Cyberware)
                                        objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                    objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (Weapon objWeapon in objMod.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                }
                                foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                                {
                                    objMount.Mods.RemoveTaggedCollectionChanged(treVehicles);
                                    objMount.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (Weapon objWeapon in objMount.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                    foreach (VehicleMod objMod in objMount.Mods)
                                    {
                                        objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                        foreach (Cyberware objCyberware in objMod.Cyberware)
                                            objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                        objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                        foreach (Weapon objWeapon in objMod.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                    }
                                }
                                foreach (Weapon objWeapon in objVehicle.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                objVehicle.Gear.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Gear objGear in objVehicle.Gear)
                                    objGear.SetupChildrenGearsCollectionChanged(false, treVehicles);
                                objVehicle.Locations.RemoveTaggedCollectionChanged(treVehicles);
                                treVehicles.FindNodeByTag(objVehicle)?.Remove();
                            }
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objVehicle, intNewIndex);
                                objVehicle.Mods.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshVehicleMods(treVehicles, objVehicle, cmsVehicle, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, null, y));
                                objVehicle.WeaponMounts.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshVehicleWeaponMounts(treVehicles, objVehicle, cmsVehicleWeaponMount, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsCyberware, cmsCyberwareGear, cmsVehicle, () => objVehicle.Mods.Count, y));
                                objVehicle.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicle.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objVehicle.Mods.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                                foreach (VehicleMod objMod in objVehicle.Mods)
                                {
                                    objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsCyberwareGear, null, y));
                                    foreach (Cyberware objCyberware in objMod.Cyberware)
                                        objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear);
                                    objMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y));
                                    foreach (Weapon objWeapon in objMod.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                }
                                foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                                {
                                    objMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                        (x, y) => RefreshVehicleMods(treVehicles, objMount, cmsVehicle, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, null, y));
                                    objMount.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMount.Mods.Count, y));
                                    foreach (Weapon objWeapon in objMount.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                    foreach (VehicleMod objMod in objMount.Mods)
                                    {
                                        objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsCyberwareGear, null, y));
                                        foreach (Cyberware objCyberware in objMod.Cyberware)
                                            objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsCyberwareGear);
                                        objMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y));
                                        foreach (Weapon objWeapon in objMod.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                    }
                                }
                                foreach (Weapon objWeapon in objVehicle.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                objVehicle.Gear.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicle.RefreshChildrenGears(treVehicles, cmsVehicleGear, () => objVehicle.Mods.Count + objVehicle.Weapons.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                                foreach (Gear objGear in objVehicle.Gear)
                                    objGear.SetupChildrenGearsCollectionChanged(true, treVehicles, cmsVehicleGear);
                                objVehicle.Locations.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshLocationsInVehicle(treVehicles, objVehicle, cmsVehicleLocation, () => objVehicle.Mods.Count + objVehicle.Weapons.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0) + objVehicle.Gear.Count(z => z.Location != null), y));
                                intNewIndex += 1;
                            }
                            treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        {
                            foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treVehicles.FindNodeByTag(objVehicle)?.Remove();
                            }
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objVehicle, intNewIndex);
                                intNewIndex += 1;
                            }
                            treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshVehicles(treVehicles, cmsVehicleLocation, cmsVehicle, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsVehicleGear, cmsVehicleWeaponMount, cmsCyberware, cmsCyberwareGear);
                        }
                        break;
                }
            }

            void AddToTree(Vehicle objVehicle, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objVehicle.CreateTreeNode(cmsVehicle, cmsVehicleLocation, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsVehicleGear, cmsVehicleWeaponMount, cmsCyberware, cmsCyberwareGear);
                if (objNode == null)
                    return;

                TreeNode nodParent = null;
                if (objVehicle.Location != null)
                {
                    nodParent = treVehicles.FindNodeByTag(objVehicle.Location, false);
                }
                if (nodParent == null)
                {
                    if (nodRoot == null)
                    {
                        nodRoot = new TreeNode
                        {
                            Tag = "Node_SelectedVehicles",
                            Text = LanguageManager.GetString("Node_SelectedVehicles", GlobalOptions.Language)
                        };
                        treVehicles.Nodes.Insert(0, nodRoot);
                    }
                    nodParent = nodRoot;
                }

                if (intIndex >= 0)
                    nodParent.Nodes.Insert(intIndex, objNode);
                else
                    nodParent.Nodes.Add(objNode);
                nodParent.Expand();
                if (blnSingleAdd)
                    treVehicles.SelectedNode = objNode;
            }
        }

        public void RefreshFociFromGear(TreeView treFoci, ContextMenuStrip cmsFocus, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            string strSelectedId = (treFoci.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            if (notifyCollectionChangedEventArgs == null)
            {
                treFoci.Nodes.Clear();

                int intFociTotal = 0;

                int intMaxFocusTotal = _objCharacter.MAG.TotalValue * 5;
                if (_objOptions.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                    intMaxFocusTotal = Math.Min(intMaxFocusTotal, _objCharacter.MAGAdept.TotalValue * 5);

                foreach (Gear objGear in _objCharacter.Gear)
                {
                    switch (objGear.Category)
                    {
                        case "Foci":
                        case "Metamagic Foci":
                            {
                                TreeNode objNode = objGear.CreateTreeNode(cmsFocus);
                                if (objNode == null)
                                    continue;
                                objNode.Text = objNode.Text.CheapReplace(LanguageManager.GetString("String_Rating", GlobalOptions.Language),
                                            () => LanguageManager.GetString(objGear.RatingLabel, GlobalOptions.Language));
                                for (int i = _objCharacter.Foci.Count - 1; i >= 0; --i)
                                {
                                    if (i < _objCharacter.Foci.Count)
                                    {
                                        Focus objFocus = _objCharacter.Foci[i];
                                        if (objFocus.GearObject == objGear)
                                        {
                                            intFociTotal += objFocus.Rating;
                                            // Do not let the number of BP spend on bonded Foci exceed MAG * 5.
                                            if (intFociTotal > intMaxFocusTotal && !_objCharacter.IgnoreRules)
                                            {
                                                objGear.Bonded = false;
                                                _objCharacter.Foci.RemoveAt(i);
                                                objNode.Checked = false;
                                            }
                                            else
                                                objNode.Checked = true;
                                        }
                                    }
                                }
                                AddToTree(objNode, false);
                            }
                            break;
                        case "Stacked Focus":
                            {
                                foreach (StackedFocus objStack in _objCharacter.StackedFoci)
                                {
                                    if (objStack.GearId == objGear.InternalId)
                                    {
                                        ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId);

                                        if (objStack.Bonded)
                                        {
                                            foreach (Gear objFociGear in objStack.Gear)
                                            {
                                                if (!string.IsNullOrEmpty(objFociGear.Extra))
                                                    ImprovementManager.ForcedValue = objFociGear.Extra;
                                                ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.Bonus, false, objFociGear.Rating, objFociGear.DisplayNameShort(GlobalOptions.Language));
                                                if (objFociGear.WirelessOn)
                                                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.WirelessBonus, false, objFociGear.Rating, objFociGear.DisplayNameShort(GlobalOptions.Language));
                                            }
                                        }

                                        AddToTree(objStack.CreateTreeNode(objGear, cmsFocus), false);
                                    }
                                }
                            }
                            break;
                    }
                }
                treFoci.SortCustomAlphabetically(strSelectedId);
            }
            else
            {
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            bool blnWarned = false;
                            int intMaxFocusTotal = _objCharacter.MAG.TotalValue * 5;
                            if (_objOptions.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                                intMaxFocusTotal = Math.Min(intMaxFocusTotal, _objCharacter.MAGAdept.TotalValue * 5);

                            HashSet<Gear> setNewGears = new HashSet<Gear>();
                            foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                                setNewGears.Add(objGear);

                            int intFociTotal = _objCharacter.Foci.Where(x => !setNewGears.Contains(x.GearObject)).Sum(x => x.Rating);

                            foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                            {
                                switch (objGear.Category)
                                {
                                    case "Foci":
                                    case "Metamagic Foci":
                                        {
                                            TreeNode objNode = objGear.CreateTreeNode(cmsFocus);
                                            if (objNode == null)
                                                continue;
                                            objNode.Text = objNode.Text.CheapReplace(LanguageManager.GetString("String_Rating", GlobalOptions.Language),
                                                () => LanguageManager.GetString("String_Force", GlobalOptions.Language));
                                            for (int i = _objCharacter.Foci.Count - 1; i >= 0; --i)
                                            {
                                                if (i < _objCharacter.Foci.Count)
                                                {
                                                    Focus objFocus = _objCharacter.Foci[i];
                                                    if (objFocus.GearObject == objGear)
                                                    {
                                                        intFociTotal += objFocus.Rating;
                                                        // Do not let the number of BP spend on bonded Foci exceed MAG * 5.
                                                        if (intFociTotal > intMaxFocusTotal && !_objCharacter.IgnoreRules)
                                                        {
                                                            // Mark the Gear a Bonded.
                                                            objGear.Bonded = false;
                                                            _objCharacter.Foci.RemoveAt(i);
                                                            objNode.Checked = false;
                                                            if (!blnWarned)
                                                            {
                                                                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_FocusMaximumForce", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_FocusMaximum", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                                blnWarned = true;
                                                                break;
                                                            }
                                                        }
                                                        else
                                                            objNode.Checked = true;
                                                    }
                                                }
                                            }
                                            AddToTree(objNode);
                                        }
                                        break;
                                    case "Stacked Focus":
                                        {
                                            foreach (StackedFocus objStack in _objCharacter.StackedFoci)
                                            {
                                                if (objStack.GearId == objGear.InternalId)
                                                {
                                                    ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId);

                                                    if (objStack.Bonded)
                                                    {
                                                        foreach (Gear objFociGear in objStack.Gear)
                                                        {
                                                            if (!string.IsNullOrEmpty(objFociGear.Extra))
                                                                ImprovementManager.ForcedValue = objFociGear.Extra;
                                                            ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.Bonus, false, objFociGear.Rating, objFociGear.DisplayNameShort(GlobalOptions.Language));
                                                            if (objFociGear.WirelessOn)
                                                                ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.WirelessBonus, false, objFociGear.Rating, objFociGear.DisplayNameShort(GlobalOptions.Language));
                                                        }
                                                    }

                                                    AddToTree(objStack.CreateTreeNode(objGear, cmsFocus));
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                            {
                                switch (objGear.Category)
                                {
                                    case "Foci":
                                    case "Metamagic Foci":
                                        {
                                            for (int i = _objCharacter.Foci.Count - 1; i >= 0; --i)
                                            {
                                                if (i < _objCharacter.Foci.Count)
                                                {
                                                    Focus objFocus = _objCharacter.Foci[i];
                                                    if (objFocus.GearObject == objGear)
                                                    {
                                                        _objCharacter.Foci.RemoveAt(i);
                                                    }
                                                }
                                            }
                                            treFoci.FindNodeByTag(objGear)?.Remove();
                                        }
                                        break;
                                    case "Stacked Focus":
                                        {
                                            for (int i = _objCharacter.StackedFoci.Count - 1; i >= 0; --i)
                                            {
                                                if (i < _objCharacter.StackedFoci.Count)
                                                {
                                                    StackedFocus objStack = _objCharacter.StackedFoci[i];
                                                    if (objStack.GearId == objGear.InternalId)
                                                    {
                                                        _objCharacter.StackedFoci.RemoveAt(i);
                                                        treFoci.FindNodeByTag(objStack)?.Remove();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Gear objGear in notifyCollectionChangedEventArgs.OldItems)
                            {
                                switch (objGear.Category)
                                {
                                    case "Foci":
                                    case "Metamagic Foci":
                                        {
                                            for (int i = _objCharacter.Foci.Count - 1; i >= 0; --i)
                                            {
                                                if (i < _objCharacter.Foci.Count)
                                                {
                                                    Focus objFocus = _objCharacter.Foci[i];
                                                    if (objFocus.GearObject == objGear)
                                                    {
                                                        _objCharacter.Foci.RemoveAt(i);
                                                    }
                                                }
                                            }
                                            treFoci.FindNodeByTag(objGear)?.Remove();
                                        }
                                        break;
                                    case "Stacked Focus":
                                        {
                                            for (int i = _objCharacter.StackedFoci.Count - 1; i >= 0; --i)
                                            {
                                                if (i < _objCharacter.StackedFoci.Count)
                                                {
                                                    StackedFocus objStack = _objCharacter.StackedFoci[i];
                                                    if (objStack.GearId == objGear.InternalId)
                                                    {
                                                        _objCharacter.StackedFoci.RemoveAt(i);
                                                        treFoci.FindNodeByTag(objStack)?.Remove();
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }
                            }

                            bool blnWarned = false;
                            int intMaxFocusTotal = _objCharacter.MAG.TotalValue * 5;
                            if (_objOptions.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                                intMaxFocusTotal = Math.Min(intMaxFocusTotal, _objCharacter.MAGAdept.TotalValue * 5);

                            HashSet<Gear> setNewGears = new HashSet<Gear>();
                            foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                                setNewGears.Add(objGear);

                            int intFociTotal = _objCharacter.Foci.Where(x => !setNewGears.Contains(x.GearObject)).Sum(x => x.Rating);

                            foreach (Gear objGear in notifyCollectionChangedEventArgs.NewItems)
                            {
                                switch (objGear.Category)
                                {
                                    case "Foci":
                                    case "Metamagic Foci":
                                        {
                                            TreeNode objNode = objGear.CreateTreeNode(cmsFocus);
                                            if (objNode == null)
                                                continue;
                                            objNode.Text = objNode.Text.CheapReplace(LanguageManager.GetString("String_Rating", GlobalOptions.Language),
                                                () => LanguageManager.GetString("String_Force", GlobalOptions.Language));
                                            for (int i = _objCharacter.Foci.Count - 1; i >= 0; --i)
                                            {
                                                if (i < _objCharacter.Foci.Count)
                                                {
                                                    Focus objFocus = _objCharacter.Foci[i];
                                                    if (objFocus.GearObject == objGear)
                                                    {
                                                        intFociTotal += objFocus.Rating;
                                                        // Do not let the number of BP spend on bonded Foci exceed MAG * 5.
                                                        if (intFociTotal > intMaxFocusTotal && !_objCharacter.IgnoreRules)
                                                        {
                                                            // Mark the Gear a Bonded.
                                                            objGear.Bonded = false;
                                                            _objCharacter.Foci.RemoveAt(i);
                                                            objNode.Checked = false;
                                                            if (!blnWarned)
                                                            {
                                                                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_FocusMaximumForce", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_FocusMaximum", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                                blnWarned = true;
                                                                break;
                                                            }
                                                        }
                                                        else
                                                            objNode.Checked = true;
                                                    }
                                                }
                                            }
                                            AddToTree(objNode);
                                        }
                                        break;
                                    case "Stacked Focus":
                                        {
                                            foreach (StackedFocus objStack in _objCharacter.StackedFoci)
                                            {
                                                if (objStack.GearId == objGear.InternalId)
                                                {
                                                    ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId);

                                                    if (objStack.Bonded)
                                                    {
                                                        foreach (Gear objFociGear in objStack.Gear)
                                                        {
                                                            if (!string.IsNullOrEmpty(objFociGear.Extra))
                                                                ImprovementManager.ForcedValue = objFociGear.Extra;
                                                            ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.Bonus, false, objFociGear.Rating, objFociGear.DisplayNameShort(GlobalOptions.Language));
                                                            if (objFociGear.WirelessOn)
                                                                ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.WirelessBonus, false, objFociGear.Rating, objFociGear.DisplayNameShort(GlobalOptions.Language));
                                                        }
                                                    }

                                                    AddToTree(objStack.CreateTreeNode(objGear, cmsFocus));
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshFociFromGear(treFoci, cmsFocus);
                        }
                        break;
                }
            }

            void AddToTree(TreeNode objNode, bool blnSingleAdd = true)
            {
                TreeNodeCollection lstParentNodeChildren = treFoci.Nodes;
                if (blnSingleAdd)
                {
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
                    treFoci.SelectedNode = objNode;
                }
                else
                    lstParentNodeChildren.Add(objNode);
            }
        }
        
        protected void RefreshMartialArts(TreeView treMartialArts, ContextMenuStrip cmsMartialArts, ContextMenuStrip cmsTechnique, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            string strSelectedId = (treMartialArts.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode objMartialArtsParentNode = null;
            TreeNode objQualityNode = null;

            if (notifyCollectionChangedEventArgs == null)
            {
                treMartialArts.Nodes.Clear();

                foreach (MartialArt objMartialArt in CharacterObject.MartialArts)
                {
                    AddToTree(objMartialArt, false);
                    objMartialArt.Techniques.AddTaggedCollectionChanged(treMartialArts, (x, y) => RefreshMartialArtTechniques(treMartialArts, objMartialArt, cmsTechnique, y));
                }

                treMartialArts.SortCustomAlphabetically(strSelectedId);
            }
            else
            {
                objMartialArtsParentNode = treMartialArts.FindNode("Node_SelectedMartialArts", false);
                objQualityNode = treMartialArts.FindNode("Node_SelectedQualities", false);
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (MartialArt objMartialArt in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objMartialArt);
                                objMartialArt.Techniques.AddTaggedCollectionChanged(treMartialArts, (x, y) => RefreshMartialArtTechniques(treMartialArts, objMartialArt, cmsTechnique, y));
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (MartialArt objMartialArt in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objMartialArt.Techniques.RemoveTaggedCollectionChanged(treMartialArts);
                                TreeNode objNode = treMartialArts.FindNodeByTag(objMartialArt);
                                if (objNode != null)
                                {
                                    TreeNode objParent = objNode.Parent;
                                    objNode.Remove();
                                    if (objParent.Nodes.Count == 0)
                                        objParent.Remove();
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents = new List<TreeNode>();
                            foreach (MartialArt objMartialArt in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objMartialArt.Techniques.RemoveTaggedCollectionChanged(treMartialArts);

                                TreeNode objNode = treMartialArts.FindNodeByTag(objMartialArt);
                                if (objNode != null)
                                {
                                    lstOldParents.Add(objNode.Parent);
                                    objNode.Remove();
                                }
                            }
                            foreach (MartialArt objMartialArt in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objMartialArt);
                                objMartialArt.Techniques.AddTaggedCollectionChanged(treMartialArts, (x, y) => RefreshMartialArtTechniques(treMartialArts, objMartialArt, cmsTechnique, y));
                            }
                            foreach (TreeNode objOldParent in lstOldParents)
                            {
                                if (objOldParent.Nodes.Count == 0)
                                    objOldParent.Remove();
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshMartialArts(treMartialArts, cmsMartialArts, cmsTechnique);
                        }
                        break;
                }
            }

            void AddToTree(MartialArt objMartialArt, bool blnSingleAdd = true)
            {
                TreeNode objNode = objMartialArt.CreateTreeNode(cmsMartialArts, cmsTechnique);
                if (objNode == null)
                    return;

                TreeNode objParentNode;
                if (objMartialArt.IsQuality)
                {
                    if (objQualityNode == null)
                    {
                        objQualityNode = new TreeNode()
                        {
                            Tag = "Node_SelectedQualities",
                            Text = LanguageManager.GetString("Node_SelectedQualities", GlobalOptions.Language)
                        };
                        treMartialArts.Nodes.Add(objQualityNode);
                        objQualityNode.Expand();
                    }
                    objParentNode = objQualityNode;
                }
                else
                {
                    if (objMartialArtsParentNode == null)
                    {
                        objMartialArtsParentNode = new TreeNode()
                        {
                            Tag = "Node_SelectedMartialArts",
                            Text = LanguageManager.GetString("Node_SelectedMartialArts", GlobalOptions.Language)
                        };
                        treMartialArts.Nodes.Insert(0, objMartialArtsParentNode);
                        objMartialArtsParentNode.Expand();
                    }
                    objParentNode = objMartialArtsParentNode;
                }

                if (blnSingleAdd)
                {
                    TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
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
                    treMartialArts.SelectedNode = objNode;
                }
                else
                    objParentNode.Nodes.Add(objNode);

                objParentNode.Expand();
            }
        }

        protected void RefreshMartialArtTechniques(TreeView treMartialArts, MartialArt objMartialArt, ContextMenuStrip cmsTechnique, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;
            TreeNode nodMartialArt = treMartialArts.FindNodeByTag(objMartialArt);
            if (nodMartialArt == null)
                return;

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (MartialArtTechnique objTechnique in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objTechnique);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (MartialArtTechnique objTechnique in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodMartialArt.FindNodeByTag(objTechnique)?.Remove();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (MartialArtTechnique objTechnique in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodMartialArt.FindNodeByTag(objTechnique)?.Remove();
                        }
                        foreach (MartialArtTechnique objTechnique in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objTechnique);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        string strSelectedId = (treMartialArts.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

                        nodMartialArt.Nodes.Clear();

                        foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                        {
                            AddToTree(objTechnique, false);
                        }

                        treMartialArts.SortCustomAlphabetically(strSelectedId);
                    }
                    break;
            }

            void AddToTree(MartialArtTechnique objTechnique, bool blnSingleAdd = true)
            {
                TreeNode objNode = objTechnique.CreateTreeNode(cmsTechnique);
                if (objNode == null)
                    return;

                if (blnSingleAdd)
                {
                    TreeNodeCollection lstParentNodeChildren = nodMartialArt.Nodes;
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
                    treMartialArts.SelectedNode = objNode;
                }
                else
                    nodMartialArt.Nodes.Add(objNode);

                nodMartialArt.Expand();
            }
        }

        /// <summary>
        /// Refresh the list of Improvements.
        /// </summary>
        protected void RefreshCustomImprovements(TreeView treImprovements, TreeView treLimit, ContextMenuStrip cmsImprovementLocation, ContextMenuStrip cmsImprovement, ContextMenuStrip cmsLimitModifier, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            string strSelectedId = (treImprovements.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode objRoot;

            if (notifyCollectionChangedEventArgs == null)
            {
                treImprovements.Nodes.Clear();

                objRoot = new TreeNode
                {
                    Tag = "Node_SelectedImprovements",
                    Text = LanguageManager.GetString("Node_SelectedImprovements", GlobalOptions.Language)
                };
                treImprovements.Nodes.Add(objRoot);

                // Add the Locations.
                foreach (string strGroup in CharacterObject.ImprovementGroups)
                {
                    TreeNode objGroup = new TreeNode
                    {
                        Tag = strGroup,
                        Text = strGroup,
                        ContextMenuStrip = cmsImprovementLocation
                    };
                    treImprovements.Nodes.Add(objGroup);
                }

                foreach (Improvement objImprovement in CharacterObject.Improvements)
                {
                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom || objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                    {
                        AddToTree(objImprovement, false);
                    }
                }

                // Sort the list of Custom Improvements in alphabetical order based on their Custom Name within each Group.
                treImprovements.SortCustomAlphabetically(strSelectedId);
            }
            else
            {
                objRoot = treImprovements.FindNode("Node_SelectedImprovements", false);
                TreeNode[] aobjLimitNodes =
                {
                    treLimit.FindNode("Node_Physical", false),
                    treLimit.FindNode("Node_Mental", false),
                    treLimit.FindNode("Node_Social", false),
                    treLimit.FindNode("Node_Astral", false)
                };

                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (Improvement objImprovement in notifyCollectionChangedEventArgs.NewItems)
                            {
                                if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom ||
                                    objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                                {
                                    AddToTree(objImprovement);
                                    AddToLimitTree(objImprovement);
                                }
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Improvement objImprovement in notifyCollectionChangedEventArgs.OldItems)
                            {
                                if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom ||
                                    objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                                {
                                    TreeNode objNode = treImprovements.FindNodeByTag(objImprovement);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent.Tag.ToString() == "Node_SelectedImprovements" && objParent.Nodes.Count == 0)
                                            objParent.Remove();
                                    }
                                    objNode = treLimit.FindNodeByTag(objImprovement);
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
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshCustomImprovements(treImprovements, treLimit, cmsImprovementLocation, cmsImprovement, cmsLimitModifier);
                            break;
                        }
                    case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents = new List<TreeNode>();
                            foreach (Improvement objImprovement in notifyCollectionChangedEventArgs.OldItems)
                            {
                                if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom ||
                                    objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                                {
                                    TreeNode objNode = treImprovements.FindNodeByTag(objImprovement);
                                    if (objNode != null)
                                    {
                                        lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }
                                    objNode = treLimit.FindNodeByTag(objImprovement);
                                    if (objNode != null)
                                    {
                                        lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }
                                }
                            }
                            foreach (Improvement objImprovement in notifyCollectionChangedEventArgs.NewItems)
                            {
                                if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom ||
                                    objImprovement.ImproveSource == Improvement.ImprovementSource.Drug)
                                {
                                    AddToTree(objImprovement);
                                    AddToLimitTree(objImprovement);
                                }
                            }
                            foreach (TreeNode objOldParent in lstOldParents)
                            {
                                if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                    objOldParent.Remove();
                            }
                            break;
                        }
                }

                void AddToLimitTree(Improvement objImprovement)
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
                        TreeNode objParentNode = aobjLimitNodes[intTargetLimit];
                        if (objParentNode == null)
                        {
                            switch (intTargetLimit)
                            {
                                case 0:
                                    objParentNode = new TreeNode()
                                    {
                                        Tag = "Node_Physical",
                                        Text = LanguageManager.GetString("Node_Physical", GlobalOptions.Language)
                                    };
                                    treLimit.Nodes.Insert(0, objParentNode);
                                    break;
                                case 1:
                                    objParentNode = new TreeNode()
                                    {
                                        Tag = "Node_Mental",
                                        Text = LanguageManager.GetString("Node_Mental", GlobalOptions.Language)
                                    };
                                    treLimit.Nodes.Insert(aobjLimitNodes[0] == null ? 0 : 1, objParentNode);
                                    break;
                                case 2:
                                    objParentNode = new TreeNode()
                                    {
                                        Tag = "Node_Social",
                                        Text = LanguageManager.GetString("Node_Social", GlobalOptions.Language)
                                    };
                                    treLimit.Nodes.Insert((aobjLimitNodes[0] == null ? 0 : 1) + (aobjLimitNodes[1] == null ? 0 : 1), objParentNode);
                                    break;
                                case 3:
                                    objParentNode = new TreeNode()
                                    {
                                        Tag = "Node_Astral",
                                        Text = LanguageManager.GetString("Node_Astral", GlobalOptions.Language)
                                    };
                                    treLimit.Nodes.Add(objParentNode);
                                    break;
                            }
                            objParentNode?.Expand();
                        }

                        string strName = objImprovement.UniqueName + LanguageManager.GetString("String_Colon", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language);
                        if (objImprovement.Value > 0)
                            strName += '+';
                        strName += objImprovement.Value.ToString();
                        if (!string.IsNullOrEmpty(objImprovement.Condition))
                            strName += ',' + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objImprovement.Condition;
                        if (objParentNode?.Nodes.ContainsKey(strName) == false)
                        {
                            TreeNode objNode = new TreeNode
                            {
                                Name = strName,
                                Text = strName,
                                Tag = objImprovement.SourceName,
                                ContextMenuStrip = cmsLimitModifier,
                                ForeColor = objImprovement.PreferredColor,
                                ToolTipText = objImprovement.Notes.WordWrap(100)
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

                            TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
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
                            treLimit.SelectedNode = objNode;
                        }
                    }
                }
            }

            void AddToTree(Improvement objImprovement, bool blnSingleAdd = true)
            {
                TreeNode objNode = objImprovement.CreateTreeNode(cmsImprovement);

                TreeNode objParentNode = objRoot;
                if (!string.IsNullOrEmpty(objImprovement.CustomGroup))
                {
                    foreach (TreeNode objFind in treImprovements.Nodes)
                    {
                        if (objFind.Text == objImprovement.CustomGroup)
                        {
                            objParentNode = objFind;
                            break;
                        }
                    }
                }
                else
                {
                    if (objParentNode == null)
                    {
                        objParentNode = new TreeNode()
                        {
                            Tag = "Node_SelectedImprovements",
                            Text = LanguageManager.GetString("Node_SelectedImprovements", GlobalOptions.Language)
                        };
                        treImprovements.Nodes.Add(objParentNode);
                    }
                }

                if (blnSingleAdd)
                {
                    TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
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
                    treImprovements.SelectedNode = objNode;
                }
                else
                    objParentNode.Nodes.Add(objNode);

                objParentNode.Expand();
            }
        }

        protected void RefreshLifestyles(TreeView treLifestyles, ContextMenuStrip cmsBasicLifestyle, ContextMenuStrip cmsAdvancedLifestyle, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            string strSelectedId = (treLifestyles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
            TreeNode objParentNode = null;

            if (notifyCollectionChangedEventArgs == null)
            {
                treLifestyles.Nodes.Clear();

                if (CharacterObject.Lifestyles.Count > 0)
                {
                    foreach (Lifestyle objLifestyle in CharacterObject.Lifestyles)
                    {
                        AddToTree(objLifestyle, false);
                    }

                    treLifestyles.SortCustomAlphabetically(strSelectedId);
                }
            }
            else
            {
                objParentNode = treLifestyles.FindNode("Node_SelectedLifestyles", false);
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (Lifestyle objLifestyle in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objLifestyle);
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Lifestyle objLifestyle in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treLifestyles.FindNodeByTag(objLifestyle);
                                if (objNode != null)
                                {
                                    TreeNode objParent = objNode.Parent;
                                    objNode.Remove();
                                    if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                        objParent.Remove();
                                }
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshLifestyles(treLifestyles, cmsBasicLifestyle, cmsAdvancedLifestyle);
                            break;
                        }
                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Lifestyle objLifestyle in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objOldParent = null;
                                TreeNode objNode = treLifestyles.FindNodeByTag(objLifestyle);
                                if (objNode != null)
                                {
                                    objOldParent = objNode.Parent;
                                    objNode.Remove();
                                }
                                AddToTree(objLifestyle);
                                if (objOldParent != null && objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                    objOldParent.Remove();
                            }
                            break;
                        }
                }
            }

            void AddToTree(Lifestyle objLifestyle, bool blnSingleAdd = true)
            {
                TreeNode objNode = objLifestyle.CreateTreeNode(cmsBasicLifestyle, cmsAdvancedLifestyle);
                if (objNode == null)
                    return;

                if (objParentNode == null)
                {
                    objParentNode = new TreeNode()
                    {
                        Tag = "Node_SelectedLifestyles",
                        Text = LanguageManager.GetString("Node_SelectedLifestyles", GlobalOptions.Language)
                    };
                    treLifestyles.Nodes.Add(objParentNode);
                    objParentNode.Expand();
                }

                if (blnSingleAdd)
                {
                    TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
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
                    treLifestyles.SelectedNode = objNode;
                }
                else
                    objParentNode.Nodes.Add(objNode);
            }
        }

        /// <summary>
        /// Refresh the Calendar List.
        /// </summary>
        public void RefreshCalendar(ListView lstCalendar, ListChangedEventArgs listChangedEventArgs = null)
        {
            if (listChangedEventArgs == null)
            {
                lstCalendar.Items.Clear();
                for (int i = 0; i < CharacterObject.Calendar.Count; ++i)
                {
                    CalendarWeek objWeek = CharacterObject.Calendar[i];

                    ListViewItem.ListViewSubItem objNoteItem = new ListViewItem.ListViewSubItem
                    {
                        Text = objWeek.Notes
                    };
                    ListViewItem.ListViewSubItem objInternalIdItem = new ListViewItem.ListViewSubItem
                    {
                        Text = objWeek.InternalId
                    };

                    ListViewItem objItem = new ListViewItem
                    {
                        Text = objWeek.DisplayName(GlobalOptions.Language)
                    };
                    objItem.SubItems.Add(objNoteItem);
                    objItem.SubItems.Add(objInternalIdItem);

                    lstCalendar.Items.Add(objItem);
                }
            }
            else
            {
                switch (listChangedEventArgs.ListChangedType)
                {
                    case ListChangedType.Reset:
                        {
                            RefreshCalendar(lstCalendar);
                        }
                        break;
                    case ListChangedType.ItemAdded:
                        {
                            int intInsertIndex = listChangedEventArgs.NewIndex;
                            CalendarWeek objWeek = CharacterObject.Calendar[intInsertIndex];

                            ListViewItem.ListViewSubItem objNoteItem = new ListViewItem.ListViewSubItem
                            {
                                Text = objWeek.Notes
                            };
                            ListViewItem.ListViewSubItem objInternalIdItem = new ListViewItem.ListViewSubItem
                            {
                                Text = objWeek.InternalId
                            };

                            ListViewItem objItem = new ListViewItem
                            {
                                Text = objWeek.DisplayName(GlobalOptions.Language)
                            };
                            objItem.SubItems.Add(objNoteItem);
                            objItem.SubItems.Add(objInternalIdItem);

                            lstCalendar.Items.Insert(intInsertIndex, objItem);
                        }
                        break;
                    case ListChangedType.ItemDeleted:
                        {
                            lstCalendar.Items.RemoveAt(listChangedEventArgs.NewIndex);
                        }
                        break;
                    case ListChangedType.ItemChanged:
                        {
                            lstCalendar.Items.RemoveAt(listChangedEventArgs.NewIndex);
                            int intInsertIndex = listChangedEventArgs.NewIndex;
                            CalendarWeek objWeek = CharacterObject.Calendar[intInsertIndex];

                            ListViewItem.ListViewSubItem objNoteItem = new ListViewItem.ListViewSubItem
                            {
                                Text = objWeek.Notes
                            };
                            ListViewItem.ListViewSubItem objInternalIdItem = new ListViewItem.ListViewSubItem
                            {
                                Text = objWeek.InternalId
                            };

                            ListViewItem objItem = new ListViewItem
                            {
                                Text = objWeek.DisplayName(GlobalOptions.Language)
                            };
                            objItem.SubItems.Add(objNoteItem);
                            objItem.SubItems.Add(objInternalIdItem);

                            lstCalendar.Items.Insert(intInsertIndex, objItem);
                        }
                        break;
                    case ListChangedType.ItemMoved:
                        {
                            lstCalendar.Items.RemoveAt(listChangedEventArgs.OldIndex);
                            int intInsertIndex = listChangedEventArgs.NewIndex;
                            CalendarWeek objWeek = CharacterObject.Calendar[intInsertIndex];

                            ListViewItem.ListViewSubItem objNoteItem = new ListViewItem.ListViewSubItem
                            {
                                Text = objWeek.Notes
                            };
                            ListViewItem.ListViewSubItem objInternalIdItem = new ListViewItem.ListViewSubItem
                            {
                                Text = objWeek.InternalId
                            };

                            ListViewItem objItem = new ListViewItem
                            {
                                Text = objWeek.DisplayName(GlobalOptions.Language)
                            };
                            objItem.SubItems.Add(objNoteItem);
                            objItem.SubItems.Add(objInternalIdItem);

                            lstCalendar.Items.Insert(intInsertIndex, objItem);
                        }
                        break;
                }
            }
        }

        public void RefreshContacts(FlowLayoutPanel panContacts, FlowLayoutPanel panEnemies, FlowLayoutPanel panPets, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (notifyCollectionChangedEventArgs == null)
            {
                panContacts.Controls.Clear();
                panEnemies.Controls.Clear();
                panPets.Controls.Clear();
                int intContacts = -1;
                int intEnemies = -1;
                foreach (Contact objContact in CharacterObject.Contacts)
                {
                    switch (objContact.EntityType)
                    {
                        case ContactType.Contact:
                            {
                                intContacts += 1;
                                ContactControl objContactControl = new ContactControl(objContact);
                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                objContactControl.DeleteContact += DeleteContact;
                                objContactControl.MouseDown += DragContactControl;

                                objContactControl.Top = intContacts * objContactControl.Height;

                                panContacts.Controls.Add(objContactControl);
                            }
                            break;
                        case ContactType.Enemy:
                            {
                                intEnemies += 1;
                                ContactControl objContactControl = new ContactControl(objContact);
                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                if (_objCharacter.Created)
                                    objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                else
                                    objContactControl.ContactDetailChanged += EnemyChanged;
                                objContactControl.DeleteContact += DeleteEnemy;
                                objContactControl.MouseDown += DragContactControl;

                                objContactControl.Top = intEnemies * objContactControl.Height;

                                panEnemies.Controls.Add(objContactControl);
                            }
                            break;
                        case ContactType.Pet:
                            {
                                PetControl objContactControl = new PetControl(objContact);
                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                objContactControl.DeleteContact += DeletePet;
                                objContactControl.MouseDown += DragContactControl;

                                panPets.Controls.Add(objContactControl);
                            }
                            break;
                    }
                }
            }
            else
            {
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            int intContacts = panContacts.Controls.Count;
                            int intEnemies = panEnemies.Controls.Count;
                            foreach (Contact objLoopContact in notifyCollectionChangedEventArgs.NewItems)
                            {
                                switch (objLoopContact.EntityType)
                                {
                                    case ContactType.Contact:
                                        {
                                            intContacts += 1;
                                            ContactControl objContactControl = new ContactControl(objLoopContact);
                                            // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                            objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                            objContactControl.DeleteContact += DeleteContact;
                                            objContactControl.MouseDown += DragContactControl;

                                            objContactControl.Top = intContacts * objContactControl.Height;

                                            panContacts.Controls.Add(objContactControl);
                                        }
                                        break;
                                    case ContactType.Enemy:
                                        {
                                            intEnemies += 1;
                                            ContactControl objContactControl = new ContactControl(objLoopContact);
                                            // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                            if (_objCharacter.Created)
                                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                            else
                                                objContactControl.ContactDetailChanged += EnemyChanged;
                                            objContactControl.DeleteContact += DeleteEnemy;
                                            //objContactControl.MouseDown += DragContactControl;

                                            objContactControl.Top = intEnemies * objContactControl.Height;

                                            panEnemies.Controls.Add(objContactControl);
                                        }
                                        break;
                                    case ContactType.Pet:
                                        {
                                            PetControl objPetControl = new PetControl(objLoopContact);
                                            // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                            objPetControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                            objPetControl.DeleteContact += DeletePet;
                                            //objPetControl.MouseDown += DragContactControl;

                                            panPets.Controls.Add(objPetControl);
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Contact objLoopContact in notifyCollectionChangedEventArgs.OldItems)
                            {
                                switch (objLoopContact.EntityType)
                                {
                                    case ContactType.Contact:
                                        {
                                            for (int i = panContacts.Controls.Count - 1; i >= 0; i--)
                                            {
                                                if (panContacts.Controls[i] is ContactControl objContactControl && objContactControl.ContactObject == objLoopContact)
                                                {
                                                    panContacts.Controls.RemoveAt(i);
                                                    objContactControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                    objContactControl.DeleteContact -= DeleteContact;
                                                    objContactControl.MouseDown -= DragContactControl;
                                                    objContactControl.Dispose();
                                                }
                                            }
                                        }
                                        break;
                                    case ContactType.Enemy:
                                        {
                                            for (int i = panEnemies.Controls.Count - 1; i >= 0; i--)
                                            {
                                                if (panEnemies.Controls[i] is ContactControl objContactControl && objContactControl.ContactObject == objLoopContact)
                                                {
                                                    panEnemies.Controls.RemoveAt(i);
                                                    if (_objCharacter.Created)
                                                        objContactControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                    else
                                                        objContactControl.ContactDetailChanged -= EnemyChanged;
                                                    objContactControl.DeleteContact -= DeleteEnemy;
                                                    objContactControl.Dispose();
                                                }
                                            }
                                        }
                                        break;
                                    case ContactType.Pet:
                                        {
                                            for (int i = panPets.Controls.Count - 1; i >= 0; i--)
                                            {
                                                if (panPets.Controls[i] is PetControl objPetControl && objPetControl.ContactObject == objLoopContact)
                                                {
                                                    panPets.Controls.RemoveAt(i);
                                                    objPetControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                    objPetControl.DeleteContact -= DeletePet;
                                                    objPetControl.Dispose();
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Contact objLoopContact in notifyCollectionChangedEventArgs.OldItems)
                            {
                                switch (objLoopContact.EntityType)
                                {
                                    case ContactType.Contact:
                                        {
                                            for (int i = panContacts.Controls.Count - 1; i >= 0; i--)
                                            {
                                                if (panContacts.Controls[i] is ContactControl objContactControl && objContactControl.ContactObject == objLoopContact)
                                                {
                                                    panContacts.Controls.RemoveAt(i);
                                                    objContactControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                    objContactControl.DeleteContact -= DeleteContact;
                                                    objContactControl.MouseDown -= DragContactControl;
                                                    objContactControl.Dispose();
                                                }
                                            }
                                        }
                                        break;
                                    case ContactType.Enemy:
                                        {
                                            for (int i = panEnemies.Controls.Count - 1; i >= 0; i--)
                                            {
                                                if (panEnemies.Controls[i] is ContactControl objContactControl && objContactControl.ContactObject == objLoopContact)
                                                {
                                                    panEnemies.Controls.RemoveAt(i);
                                                    if (_objCharacter.Created)
                                                        objContactControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                    else
                                                        objContactControl.ContactDetailChanged -= EnemyChanged;
                                                    objContactControl.DeleteContact -= DeleteEnemy;
                                                    objContactControl.Dispose();
                                                }
                                            }
                                        }
                                        break;
                                    case ContactType.Pet:
                                        {
                                            for (int i = panPets.Controls.Count - 1; i >= 0; i--)
                                            {
                                                if (panPets.Controls[i] is PetControl objPetControl && objPetControl.ContactObject == objLoopContact)
                                                {
                                                    panPets.Controls.RemoveAt(i);
                                                    objPetControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                                    objPetControl.DeleteContact -= DeletePet;
                                                    objPetControl.Dispose();
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                            int intContacts = panContacts.Controls.Count;
                            int intEnemies = panEnemies.Controls.Count;
                            foreach (Contact objLoopContact in notifyCollectionChangedEventArgs.NewItems)
                            {
                                switch (objLoopContact.EntityType)
                                {
                                    case ContactType.Contact:
                                        {
                                            intContacts += 1;
                                            ContactControl objContactControl = new ContactControl(objLoopContact);
                                            // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                            objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                            objContactControl.DeleteContact += DeleteContact;
                                            objContactControl.MouseDown += DragContactControl;

                                            objContactControl.Top = intContacts * objContactControl.Height;

                                            panContacts.Controls.Add(objContactControl);
                                        }
                                        break;
                                    case ContactType.Enemy:
                                        {
                                            intEnemies += 1;
                                            ContactControl objContactControl = new ContactControl(objLoopContact);
                                            // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                            if (_objCharacter.Created)
                                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                            else
                                                objContactControl.ContactDetailChanged += EnemyChanged;
                                            objContactControl.DeleteContact += DeleteEnemy;
                                            //objContactControl.MouseDown += DragContactControl;

                                            objContactControl.Top = intEnemies * objContactControl.Height;

                                            panEnemies.Controls.Add(objContactControl);
                                        }
                                        break;
                                    case ContactType.Pet:
                                        {
                                            PetControl objPetControl = new PetControl(objLoopContact);
                                            // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                            objPetControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                            objPetControl.DeleteContact += DeletePet;
                                            //objPetControl.MouseDown += DragContactControl;

                                            panPets.Controls.Add(objPetControl);
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshContacts(panContacts, panEnemies, panPets);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Moves a tree node to a specified spot in it's parent node collection.
        /// Will persist between loads if the node's object is an ICanSort
        /// </summary>
        /// <param name="objNode">The item to move</param>
        /// <param name="intNewIndex">The new index in the parent array</param>
        public void MoveTreeNode(TreeNode objNode, int intNewIndex)
        {
            if (objNode == null || !(objNode.Tag is ICanSort objSortable))
                return;

            TreeView treOwningTree = objNode.TreeView;
            TreeNode objParent = objNode.Parent;
            TreeNodeCollection lstNodes = objParent?.Nodes ?? treOwningTree.Nodes;

            if (lstNodes != null)
            {
                List<ICanSort> lstSorted = lstNodes.Cast<TreeNode>().Select(n => n.Tag).OfType<ICanSort>().ToList();

                // Anything that can't be sorted gets sent to the front of the list, so subtract that number from our new
                // sorting index and make sure we're still inside the array
                intNewIndex = Math.Min(lstSorted.Count - 1, Math.Max(0, intNewIndex + lstSorted.Count - lstNodes.Count));

                lstSorted.Remove(objSortable);
                lstSorted.Insert(intNewIndex, objSortable);

                // Update the sort field of everything in the array. Doing it this way means we only t
                for (int i = 0; i < lstSorted.Count; ++i)
                {
                    lstSorted[i].SortOrder = i;
                }

                // Sort the actual tree
                treOwningTree.SortCustomOrder();

                IsDirty = true;
            }
        }

        /// <summary>
        /// Adds the selected Object and child items to the clipboard as appropriate.
        /// </summary>
        /// <param name="selectedObject"></param>
        public void CopyObject(object selectedObject)
        {
            switch (selectedObject)
            {
                case Armor objCopyArmor:
                    {
                        MemoryStream objStream = new MemoryStream();
                        XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 1,
                            IndentChar = '\t'
                        };

                        objWriter.WriteStartDocument();

                        // </characters>
                        objWriter.WriteStartElement("character");

                        objCopyArmor.Save(objWriter);
                        ClipboardManager.ClipboardContentType = ClipboardContentType.Armor;

                        if (!objCopyArmor.WeaponID.IsEmptyGuid())
                        {
                            // <weapons>
                            objWriter.WriteStartElement("weapons");
                            // Copy any Weapon that comes with the Gear.
                            foreach (Weapon objCopyWeapon in CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCopyArmor.InternalId))
                            {
                                objCopyWeapon.Save(objWriter);
                            }

                            objWriter.WriteEndElement();
                        }

                        // </characters>
                        objWriter.WriteEndElement();

                        // Finish the document and flush the Writer and Stream.
                        objWriter.WriteEndDocument();
                        objWriter.Flush();

                        // Read the stream.
                        StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                        objStream.Position = 0;
                        XmlDocument objCharacterXML = new XmlDocument();

                        // Put the stream into an XmlDocument.
                        string strXML = objReader.ReadToEnd();
                        objCharacterXML.LoadXml(strXML);

                        objWriter.Close();

                        ClipboardManager.Clipboard = objCharacterXML;
                        break;
                    }
                case ArmorMod objCopyArmorMod:
                    {
                        MemoryStream objStream = new MemoryStream();
                        XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 1,
                            IndentChar = '\t'
                        };

                        objWriter.WriteStartDocument();

                        // </characters>
                        objWriter.WriteStartElement("character");

                        objCopyArmorMod.Save(objWriter);
                        ClipboardManager.ClipboardContentType = ClipboardContentType.Armor;

                        if (!objCopyArmorMod.WeaponID.IsEmptyGuid())
                        {
                            // <weapons>
                            objWriter.WriteStartElement("weapons");
                            // Copy any Weapon that comes with the Gear.
                            foreach (Weapon objCopyWeapon in CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCopyArmorMod.InternalId))
                            {
                                objCopyWeapon.Save(objWriter);
                            }

                            objWriter.WriteEndElement();
                        }

                        // </characters>
                        objWriter.WriteEndElement();

                        // Finish the document and flush the Writer and Stream.
                        objWriter.WriteEndDocument();
                        objWriter.Flush();

                        // Read the stream.
                        StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                        objStream.Position = 0;
                        XmlDocument objCharacterXML = new XmlDocument();

                        // Put the stream into an XmlDocument.
                        string strXML = objReader.ReadToEnd();
                        objCharacterXML.LoadXml(strXML);

                        objWriter.Close();

                        ClipboardManager.Clipboard = objCharacterXML;
                        break;
                    }
                case Cyberware objCopyCyberware:
                    {
                        MemoryStream objStream = new MemoryStream();
                        XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 1,
                            IndentChar = '\t'
                        };

                        objWriter.WriteStartDocument();

                        // </characters>
                        objWriter.WriteStartElement("character");

                        objCopyCyberware.Save(objWriter);
                        ClipboardManager.ClipboardContentType = ClipboardContentType.Cyberware;

                        if (!objCopyCyberware.WeaponID.IsEmptyGuid())
                        {
                            // <weapons>
                            objWriter.WriteStartElement("weapons");
                            // Copy any Weapon that comes with the Gear.
                            foreach (Weapon objCopyWeapon in CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCopyCyberware.InternalId))
                            {
                                objCopyWeapon.Save(objWriter);
                            }

                            objWriter.WriteEndElement();
                        }
                        if (!objCopyCyberware.VehicleID.IsEmptyGuid())
                        {
                            // <vehicles>
                            objWriter.WriteStartElement("vehicles");
                            // Copy any Vehicle that comes with the Gear.
                            foreach (Vehicle objCopyVehicle in CharacterObject.Vehicles.Where(x => x.ParentID == objCopyCyberware.InternalId))
                            {
                                objCopyVehicle.Save(objWriter);
                            }

                            objWriter.WriteEndElement();
                        }

                        // </characters>
                        objWriter.WriteEndElement();

                        // Finish the document and flush the Writer and Stream.
                        objWriter.WriteEndDocument();
                        objWriter.Flush();

                        // Read the stream.
                        StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                        objStream.Position = 0;
                        XmlDocument objCharacterXML = new XmlDocument();

                        // Put the stream into an XmlDocument.
                        string strXML = objReader.ReadToEnd();
                        objCharacterXML.LoadXml(strXML);

                        objWriter.Close();

                        ClipboardManager.Clipboard = objCharacterXML;
                        //Clipboard.SetText(objCharacterXML.OuterXml);
                        break;
                    }
                case Gear objCopyGear:
                    {
                        MemoryStream objStream = new MemoryStream();
                        XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 1,
                            IndentChar = '\t'
                        };

                        objWriter.WriteStartDocument();

                        // </characters>
                        objWriter.WriteStartElement("character");

                        objCopyGear.Save(objWriter);
                        ClipboardManager.ClipboardContentType = ClipboardContentType.Gear;

                        if (!objCopyGear.WeaponID.IsEmptyGuid())
                        {
                            // <weapons>
                            objWriter.WriteStartElement("weapons");
                            // Copy any Weapon that comes with the Gear.
                            foreach (Weapon objCopyWeapon in CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCopyGear.InternalId))
                            {
                                objCopyWeapon.Save(objWriter);
                            }

                            objWriter.WriteEndElement();
                        }

                        // </characters>
                        objWriter.WriteEndElement();

                        // Finish the document and flush the Writer and Stream.
                        objWriter.WriteEndDocument();
                        objWriter.Flush();

                        // Read the stream.
                        StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                        objStream.Position = 0;
                        XmlDocument objCharacterXML = new XmlDocument();

                        // Put the stream into an XmlDocument.
                        string strXML = objReader.ReadToEnd();
                        objCharacterXML.LoadXml(strXML);

                        objWriter.Close();

                        ClipboardManager.Clipboard = objCharacterXML;
                        break;
                    }
                case Lifestyle objCopyLifestyle:
                    {
                    MemoryStream objStream = new MemoryStream();
                    XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 1,
                        IndentChar = '\t'
                    };

                    objWriter.WriteStartDocument();

                    // </characters>
                    objWriter.WriteStartElement("character");

                    objCopyLifestyle.Save(objWriter);

                    // </characters>
                    objWriter.WriteEndElement();

                    // Finish the document and flush the Writer and Stream.
                    objWriter.WriteEndDocument();
                    objWriter.Flush();

                    // Read the stream.
                    StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                    objStream.Position = 0;
                    XmlDocument objCharacterXML = new XmlDocument();

                    // Put the stream into an XmlDocument.
                    string strXML = objReader.ReadToEnd();
                    objCharacterXML.LoadXml(strXML);

                    objWriter.Close();

                    ClipboardManager.Clipboard = objCharacterXML;
                    ClipboardManager.ClipboardContentType = ClipboardContentType.Lifestyle;
                    //Clipboard.SetText(objCharacterXML.OuterXml);
                    break;
                    }
                case Vehicle objCopyVehicle:
                    {
                    MemoryStream objStream = new MemoryStream();
                    XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 1,
                        IndentChar = '\t'
                    };

                    objWriter.WriteStartDocument();

                    // </characters>
                    objWriter.WriteStartElement("character");

                    objCopyVehicle.Save(objWriter);

                    // </characters>
                    objWriter.WriteEndElement();

                    // Finish the document and flush the Writer and Stream.
                    objWriter.WriteEndDocument();
                    objWriter.Flush();

                    // Read the stream.
                    StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                    objStream.Position = 0;
                    XmlDocument objCharacterXML = new XmlDocument();

                    // Put the stream into an XmlDocument.
                    string strXML = objReader.ReadToEnd();
                    objCharacterXML.LoadXml(strXML);

                    objWriter.Close();

                    ClipboardManager.Clipboard = objCharacterXML;
                    ClipboardManager.ClipboardContentType = ClipboardContentType.Vehicle;
                    //Clipboard.SetText(objCharacterXML.OuterXml);
                    break;
                }
                case Weapon objCopyWeapon:
                    {
                        // Do not let the user copy Gear or Cyberware Weapons.
                        if (objCopyWeapon.Category == "Gear" || objCopyWeapon.Cyberware)
                            return;

                        MemoryStream objStream = new MemoryStream();
                        XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 1,
                            IndentChar = '\t'
                        };

                        objWriter.WriteStartDocument();

                        // </characters>
                        objWriter.WriteStartElement("character");

                        objCopyWeapon.Save(objWriter);

                        // </characters>
                        objWriter.WriteEndElement();

                        // Finish the document and flush the Writer and Stream.
                        objWriter.WriteEndDocument();
                        objWriter.Flush();

                        // Read the stream.
                        StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                        objStream.Position = 0;
                        XmlDocument objCharacterXML = new XmlDocument();

                        // Put the stream into an XmlDocument.
                        string strXML = objReader.ReadToEnd();
                        objCharacterXML.LoadXml(strXML);

                        objWriter.Close();

                        ClipboardManager.Clipboard = objCharacterXML;
                        ClipboardManager.ClipboardContentType = ClipboardContentType.Weapon;
                        break;
                    }
                case WeaponAccessory objCopyAccessory:
                    {
                    // Do not let the user copy accessories that are unique to its parent.
                    if (objCopyAccessory.IncludedInWeapon)
                        return;

                    MemoryStream objStream = new MemoryStream();
                    XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 1,
                        IndentChar = '\t'
                    };

                    objWriter.WriteStartDocument();

                    // </characters>
                    objWriter.WriteStartElement("character");

                    objCopyAccessory.Save(objWriter);

                    // </characters>
                    objWriter.WriteEndElement();

                    // Finish the document and flush the Writer and Stream.
                    objWriter.WriteEndDocument();
                    objWriter.Flush();

                    // Read the stream.
                    StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                    objStream.Position = 0;
                    XmlDocument objCharacterXML = new XmlDocument();

                    // Put the stream into an XmlDocument.
                    string strXML = objReader.ReadToEnd();
                    objCharacterXML.LoadXml(strXML);

                    objWriter.Close();

                    ClipboardManager.Clipboard = objCharacterXML;
                    ClipboardManager.ClipboardContentType = ClipboardContentType.WeaponAccessory;
                    break;
                    }
            }
        }
        #region ContactControl Events
        protected void DragContactControl(object sender, MouseEventArgs e)
        {
            Control source = (Control)sender;
            source.DoDragDrop(new TransportWrapper(source), DragDropEffects.Move);
        }

        protected void AddContact()
        {
            Contact objContact = new Contact(CharacterObject)
            {
                EntityType = ContactType.Contact
            };
            CharacterObject.Contacts.Add(objContact);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        protected void DeleteContact(object sender, EventArgs e)
        {
            if (sender is ContactControl objSender)
            {
                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteContact", GlobalOptions.Language)))
                    return;

                CharacterObject.Contacts.Remove(objSender.ContactObject);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }
        #endregion

        #region PetControl Events
        protected void AddPet()
        {
            Contact objContact = new Contact(CharacterObject)
            {
                EntityType = ContactType.Pet
            };

            CharacterObject.Contacts.Add(objContact);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        protected void DeletePet(object sender, EventArgs e)
        {
            if (sender is PetControl objSender)
            {
                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteContact", GlobalOptions.Language)))
                    return;

                CharacterObject.Contacts.Remove(objSender.ContactObject);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }
        #endregion

        #region EnemyControl Events
        protected void AddEnemy()
        {
            // Handle the ConnectionRatingChanged Event for the ContactControl object.
            Contact objContact = new Contact(CharacterObject)
            {
                EntityType = ContactType.Enemy
            };

            CharacterObject.Contacts.Add(objContact);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        protected void EnemyChanged(object sender, TextEventArgs e)
        {
            ContactControl objSenderControl = sender as ContactControl;

            // Handle the ConnectionRatingChanged Event for the ContactControl object.
            int intNegativeQualityBP = 0;
            // Calculate the BP used for Negative Qualities.
            foreach (Quality objQuality in CharacterObject.Qualities)
            {
                if (objQuality.Type == QualityType.Negative && objQuality.ContributeToLimit)
                    intNegativeQualityBP += objQuality.BP;
            }
            // Include the amount of free Negative Qualities from Improvements.
            intNegativeQualityBP -= ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.FreeNegativeQualities);

            // Adjust for Karma cost multiplier.
            intNegativeQualityBP *= CharacterObjectOptions.KarmaQuality;

            // Find current enemy BP total
            int intBPUsed = 0;
            foreach (Contact objLoopEnemy in CharacterObject.Contacts)
            {
                if (objLoopEnemy.EntityType == ContactType.Enemy && !objLoopEnemy.Free)
                {
                    intBPUsed -= (objLoopEnemy.Connection + objLoopEnemy.Loyalty) * CharacterObjectOptions.KarmaEnemy;
                }
            }

            int intEnemyMax = CharacterObject.GameplayOptionQualityLimit;
            int intQualityMax = CharacterObject.GameplayOptionQualityLimit;
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            string strEnemyPoints = intEnemyMax.ToString() + strSpaceCharacter + LanguageManager.GetString("String_Karma", GlobalOptions.Language);
            string strQualityPoints = intQualityMax.ToString() + strSpaceCharacter + LanguageManager.GetString("String_Karma", GlobalOptions.Language);

            if (intBPUsed < (intEnemyMax * -1) && !CharacterObject.IgnoreRules && CharacterObjectOptions.EnemyKarmaQualityLimit)
            {
                Program.MainForm.ShowMessageBox(string.Format(LanguageManager.GetString("Message_EnemyLimit", GlobalOptions.Language), strEnemyPoints),
                    LanguageManager.GetString("MessageTitle_EnemyLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Contact objSenderContact = objSenderControl?.ContactObject;
                if (objSenderContact != null)
                {
                    int intTotal = (intEnemyMax * -1) - intBPUsed;
                    switch (e.Text)
                    {
                        case "Connection":
                            objSenderContact.Connection -= intTotal;
                            break;
                        case "Loyalty":
                            objSenderContact.Loyalty -= intTotal;
                            break;
                    }
                }

                return;
            }

            if (!CharacterObjectOptions.ExceedNegativeQualities && CharacterObjectOptions.EnemyKarmaQualityLimit)
            {
                if (intBPUsed + intNegativeQualityBP < (intQualityMax * -1) && !CharacterObject.IgnoreRules)
                {
                    Program.MainForm.ShowMessageBox(string.Format(LanguageManager.GetString("Message_NegativeQualityLimit", GlobalOptions.Language), strQualityPoints),
                        LanguageManager.GetString("MessageTitle_NegativeQualityLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Contact objSenderContact = objSenderControl?.ContactObject;
                    if (objSenderContact != null)
                    {
                        switch (e.Text)
                        {
                            case "Connection":
                                objSenderContact.Connection -= (((intQualityMax * -1) - (intBPUsed + intNegativeQualityBP)) /
                                                                CharacterObjectOptions.KarmaQuality);
                                break;
                            case "Loyalty":
                                objSenderContact.Loyalty -= (((intQualityMax * -1) - (intBPUsed + intNegativeQualityBP)) /
                                                             CharacterObjectOptions.KarmaQuality);
                                break;
                        }
                    }
                }
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        protected void DeleteEnemy(object sender, EventArgs e)
        {
            if (sender is ContactControl objSender)
            {
                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteEnemy", GlobalOptions.Language)))
                    return;

                CharacterObject.Contacts.Remove(objSender.ContactObject);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }
        #endregion

        #region Additional Relationships Tab Control Events
        protected void AddContactsFromFile()
        {
            // Displays an OpenFileDialog so the user can select the XML to read.
            OpenFileDialog dlgOpenFileDialog = new OpenFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Xml", GlobalOptions.Language) + '|' + LanguageManager.GetString("DialogFilter_All", GlobalOptions.Language)
            };

            // Show the Dialog.
            // If the user cancels out, return early.
            if (dlgOpenFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                using (StreamReader objStreamReader = new StreamReader(dlgOpenFileDialog.FileName, Encoding.UTF8, true))
                {
                    xmlDoc.Load(objStreamReader);
                }
            }
            catch (IOException ex)
            {
                Program.MainForm.ShowMessageBox(ex.ToString());
                return;
            }
            catch (XmlException ex)
            {
                Program.MainForm.ShowMessageBox(ex.ToString());
                return;
            }

            foreach (XPathNavigator xmlContact in xmlDoc.GetFastNavigator().Select("/chummer/contacts/contact"))
            {
                Contact objContact = new Contact(CharacterObject);
                objContact.Load(xmlContact);
                CharacterObject.Contacts.Add(objContact);
            }
        }
        #endregion

        public void RefreshSpirits(Panel panSpirits, Panel panSprites, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (notifyCollectionChangedEventArgs == null)
            {
                panSpirits.Controls.Clear();
                panSprites.Controls.Clear();
                int intSpirits = -1;
                int intSprites = -1;
                foreach (Spirit objSpirit in CharacterObject.Spirits)
                {
                    bool blnIsSpirit = objSpirit.EntityType == SpiritType.Spirit;
                    SpiritControl objSpiritControl = new SpiritControl(objSpirit);

                    // Attach an EventHandler for the ServicesOwedChanged Event.
                    objSpiritControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                    objSpiritControl.DeleteSpirit += DeleteSpirit;

                    objSpiritControl.RebuildSpiritList(CharacterObject.MagicTradition);

                    if (blnIsSpirit)
                    {
                        intSpirits += 1;
                        objSpiritControl.Top = intSpirits * objSpiritControl.Height;
                        panSpirits.Controls.Add(objSpiritControl);
                    }
                    else
                    {
                        intSprites += 1;
                        objSpiritControl.Top = intSprites * objSpiritControl.Height;
                        panSprites.Controls.Add(objSpiritControl);
                    }
                }
            }
            else
            {
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            int intSpirits = panSpirits.Controls.Count;
                            int intSprites = panSprites.Controls.Count;
                            foreach (Spirit objSpirit in notifyCollectionChangedEventArgs.NewItems)
                            {
                                bool blnIsSpirit = objSpirit.EntityType == SpiritType.Spirit;
                                SpiritControl objSpiritControl = new SpiritControl(objSpirit);

                                // Attach an EventHandler for the ServicesOwedChanged Event.
                                objSpiritControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                objSpiritControl.DeleteSpirit += DeleteSpirit;

                                objSpiritControl.RebuildSpiritList(CharacterObject.MagicTradition);

                                if (blnIsSpirit)
                                {
                                    objSpiritControl.Top = intSpirits * objSpiritControl.Height;
                                    panSpirits.Controls.Add(objSpiritControl);
                                    intSpirits += 1;
                                }
                                else
                                {
                                    objSpiritControl.Top = intSprites * objSpiritControl.Height;
                                    panSprites.Controls.Add(objSpiritControl);
                                    intSprites += 1;
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Spirit objSpirit in notifyCollectionChangedEventArgs.OldItems)
                            {
                                int intMoveUpAmount = 0;
                                if (objSpirit.EntityType == SpiritType.Spirit)
                                {
                                    int intSpirits = panSpirits.Controls.Count;
                                    for (int i = 0; i < intSpirits; ++i)
                                    {
                                        Control objLoopControl = panSpirits.Controls[i];
                                        if (objLoopControl is SpiritControl objSpiritControl && objSpiritControl.SpiritObject == objSpirit)
                                        {
                                            intMoveUpAmount = objSpiritControl.Height;
                                            panSpirits.Controls.RemoveAt(i);
                                            objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                            objSpiritControl.DeleteSpirit -= DeleteSpirit;
                                            objSpiritControl.Dispose();
                                            i -= 1;
                                            intSpirits -= 1;
                                        }
                                        else if (intMoveUpAmount != 0)
                                        {
                                            objLoopControl.Top -= intMoveUpAmount;
                                        }
                                    }
                                }
                                else
                                {
                                    int intSprites = panSprites.Controls.Count;
                                    for (int i = 0; i < intSprites; ++i)
                                    {
                                        Control objLoopControl = panSprites.Controls[i];
                                        if (objLoopControl is SpiritControl objSpiritControl && objSpiritControl.SpiritObject == objSpirit)
                                        {
                                            intMoveUpAmount = objSpiritControl.Height;
                                            panSprites.Controls.RemoveAt(i);
                                            objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                            objSpiritControl.DeleteSpirit -= DeleteSpirit;
                                            objSpiritControl.Dispose();
                                            i -= 1;
                                            intSprites -= 1;
                                        }
                                        else if (intMoveUpAmount != 0)
                                        {
                                            objLoopControl.Top -= intMoveUpAmount;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            int intSpirits = panSpirits.Controls.Count;
                            int intSprites = panSprites.Controls.Count;
                            foreach (Spirit objSpirit in notifyCollectionChangedEventArgs.OldItems)
                            {
                                int intMoveUpAmount = 0;
                                if (objSpirit.EntityType == SpiritType.Spirit)
                                {
                                    for (int i = 0; i < intSpirits; ++i)
                                    {
                                        Control objLoopControl = panSpirits.Controls[i];
                                        if (objLoopControl is SpiritControl objSpiritControl && objSpiritControl.SpiritObject == objSpirit)
                                        {
                                            intMoveUpAmount = objSpiritControl.Height;
                                            panSpirits.Controls.RemoveAt(i);
                                            objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                            objSpiritControl.DeleteSpirit -= DeleteSpirit;
                                            objSpiritControl.Dispose();
                                            i -= 1;
                                            intSpirits -= 1;
                                        }
                                        else if (intMoveUpAmount != 0)
                                        {
                                            objLoopControl.Top -= intMoveUpAmount;
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < intSprites; ++i)
                                    {
                                        Control objLoopControl = panSprites.Controls[i];
                                        if (objLoopControl is SpiritControl objSpiritControl && objSpiritControl.SpiritObject == objSpirit)
                                        {
                                            intMoveUpAmount = objSpiritControl.Height;
                                            panSprites.Controls.RemoveAt(i);
                                            objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                            objSpiritControl.DeleteSpirit -= DeleteSpirit;
                                            objSpiritControl.Dispose();
                                            i -= 1;
                                            intSprites -= 1;
                                        }
                                        else if (intMoveUpAmount != 0)
                                        {
                                            objLoopControl.Top -= intMoveUpAmount;
                                        }
                                    }
                                }
                            }
                            foreach (Spirit objSpirit in notifyCollectionChangedEventArgs.NewItems)
                            {
                                bool blnIsSpirit = objSpirit.EntityType == SpiritType.Spirit;
                                SpiritControl objSpiritControl = new SpiritControl(objSpirit);

                                // Attach an EventHandler for the ServicesOwedChanged Event.
                                objSpiritControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                objSpiritControl.DeleteSpirit += DeleteSpirit;

                                objSpiritControl.RebuildSpiritList(CharacterObject.MagicTradition);

                                if (blnIsSpirit)
                                {
                                    objSpiritControl.Top = intSpirits * objSpiritControl.Height;
                                    panSpirits.Controls.Add(objSpiritControl);
                                    intSpirits += 1;
                                }
                                else
                                {
                                    objSpiritControl.Top = intSprites * objSpiritControl.Height;
                                    panSprites.Controls.Add(objSpiritControl);
                                    intSprites += 1;
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshSpirits(panSpirits, panSprites);
                        }
                        break;
                }
            }
        }

        #region SpiritControl Events
        protected void AddSpirit()
        {
            // The number of bound Spirits cannot exeed the character's CHA.
            if (!CharacterObject.IgnoreRules && CharacterObject.Spirits.Count(x => x.EntityType == SpiritType.Spirit) >= CharacterObject.CHA.Value)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_BoundSpiritLimit", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_BoundSpiritLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Spirit objSpirit = new Spirit(CharacterObject)
            {
                EntityType = SpiritType.Spirit,
                Force = CharacterObject.MaxSpiritForce
            };
            CharacterObject.Spirits.Add(objSpirit);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        protected void AddSprite()
        {
            if (CharacterObject.Created && CharacterObject.Spirits.Count(x => x.EntityType == SpiritType.Sprite && !x.Bound && !x.Fettered) > 0)
            {
                // Once created, new sprites are added as Unbound first. We're not permitted to have more than 1 at a time.
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_UnregisteredSpriteLimit", GlobalOptions.Language),
                    LanguageManager.GetString("MessageTitle_UnregisteredSpriteLimit", GlobalOptions.Language),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                // In create, all sprites are added as Bound/Registered. The number of registered Sprites cannot exceed the character's LOG.
                if (!CharacterObject.IgnoreRules &&
                    CharacterObject.Spirits.Count(x => x.EntityType == SpiritType.Sprite && x.Bound && !x.Fettered) >=
                    CharacterObject.LOG.TotalValue)
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_RegisteredSpriteLimit", GlobalOptions.Language),
                        LanguageManager.GetString("MessageTitle_RegisteredSpriteLimit", GlobalOptions.Language),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            Spirit objSprite = new Spirit(CharacterObject)
            {
                EntityType = SpiritType.Sprite,
                Force = CharacterObject.MaxSpriteLevel
            };
            CharacterObject.Spirits.Add(objSprite);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        protected void DeleteSpirit(object sender, EventArgs e)
        {
            if (sender is SpiritControl objSender)
            {
                Spirit objSpirit = objSender.SpiritObject;
                bool blnIsSpirit = objSpirit.EntityType == SpiritType.Spirit;
                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString(blnIsSpirit ? "Message_DeleteSpirit" : "Message_DeleteSprite", GlobalOptions.Language)))
                    return;
                objSpirit.Fettered = false; // Fettered spirits consume MAG.
                CharacterObject.Spirits.Remove(objSpirit);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }
        #endregion

        /// <summary>
        /// Add a mugshot to the character.
        /// </summary>
        protected bool AddMugshot()
        {
            bool blnSuccess = false;
            using (OpenFileDialog dlgOpenFileDialog = new OpenFileDialog())
            {
                if (!string.IsNullOrWhiteSpace(_objOptions.RecentImageFolder) && Directory.Exists(_objOptions.RecentImageFolder))
                {
                    dlgOpenFileDialog.InitialDirectory = _objOptions.RecentImageFolder;
                }
                // Prompt the user to select an image to associate with this character.

                ImageCodecInfo[] lstCodecs = ImageCodecInfo.GetImageEncoders();
                dlgOpenFileDialog.Filter = string.Format("All image files ({1})|{1}|{0}|All files|*",
                    string.Join("|", lstCodecs.Select(codec => string.Format("{0} ({1})|{1}", codec.CodecName, codec.FilenameExtension)).ToArray()),
                    string.Join(";", lstCodecs.Select(codec => codec.FilenameExtension).ToArray()));

                if (dlgOpenFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    blnSuccess = true;
                    // Convert the image to a string usinb Base64.
                    _objOptions.RecentImageFolder = Path.GetDirectoryName(dlgOpenFileDialog.FileName);

                    Bitmap imgMugshot = (new Bitmap(dlgOpenFileDialog.FileName, true)).ConvertPixelFormat(PixelFormat.Format32bppPArgb);

                    _objCharacter.Mugshots.Add(imgMugshot);
                    if (_objCharacter.MainMugshotIndex == -1)
                        _objCharacter.MainMugshotIndex = _objCharacter.Mugshots.Count - 1;
                }
            }
            return blnSuccess;
        }

        /// <summary>
        /// Update the mugshot info of a character.
        /// </summary>
        /// <param name="picMugshot"></param>
        /// <param name="intCurrentMugshotIndexInList"></param>
        protected void UpdateMugshot(PictureBox picMugshot, int intCurrentMugshotIndexInList)
        {
            if (intCurrentMugshotIndexInList < 0 || intCurrentMugshotIndexInList >= _objCharacter.Mugshots.Count || _objCharacter.Mugshots[intCurrentMugshotIndexInList] == null)
            {
                picMugshot.Image = null;
                return;
            }

            Image imgMugshot = _objCharacter.Mugshots[intCurrentMugshotIndexInList];

            if (imgMugshot != null && picMugshot.Height >= imgMugshot.Height && picMugshot.Width >= imgMugshot.Width)
                picMugshot.SizeMode = PictureBoxSizeMode.CenterImage;
            else
                picMugshot.SizeMode = PictureBoxSizeMode.Zoom;
            picMugshot.Image = imgMugshot;
        }

        /// <summary>
        /// Remove a mugshot of a character.
        /// </summary>
        /// <param name="intCurrentMugshotIndexInList"></param>
        protected void RemoveMugshot(int intCurrentMugshotIndexInList)
        {
            if (intCurrentMugshotIndexInList < 0 || intCurrentMugshotIndexInList >= _objCharacter.Mugshots.Count)
            {
                return;
            }

            _objCharacter.Mugshots.RemoveAt(intCurrentMugshotIndexInList);
            if (intCurrentMugshotIndexInList == _objCharacter.MainMugshotIndex)
            {
                _objCharacter.MainMugshotIndex = -1;
            }
            else if (intCurrentMugshotIndexInList < _objCharacter.MainMugshotIndex)
            {
                _objCharacter.MainMugshotIndex -= 1;
            }
        }

        /// <summary>
        /// Whether or not the character has changes that can be saved
        /// </summary>
        public bool IsDirty
        {
            get => _blnIsDirty;
            set
            {
                if (_blnIsDirty != value)
                {
                    _blnIsDirty = value;
                    UpdateWindowTitle(true);
                }
            }
        }

        /// <summary>
        /// Whether or not the form is currently in the middle of refreshing some UI elements
        /// </summary>
        public bool IsRefreshing
        {
            get => _blnIsRefreshing;
            set
            {
                //if (_blnIsRefreshing != value)
                {
                    _blnIsRefreshing = value;
                    /*
                    if (value)
                        SuspendLayout();
                    else if (!IsLoading)
                        ResumeLayout();
                        */
                }
            }
        }

        public bool IsLoading
        {
            get => _blnLoading;
            set
            {
                //if (_blnLoading != value)
                {
                    _blnLoading = value;
                    /*
                    if (value)
                        SuspendLayout();
                    else if (!IsRefreshing)
                        ResumeLayout();
                        */
                }
            }
        }
        
        public void MakeDirtyWithCharacterUpdate(object sender, EventArgs e)
        {
            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        public void MakeDirty(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        public bool IsCharacterUpdateRequested { get; set; }

        public Character CharacterObject => _objCharacter;

        protected CharacterOptions CharacterObjectOptions => _objOptions;

		protected virtual string FormMode => string.Empty;

        protected void ShiftTabsOnMouseScroll(object sender, MouseEventArgs e)
        {
            //TODO: Global option to switch behaviour on/off, method to emulate clicking the scroll buttons instead of changing the selected index,
            //allow wrapping back to first/last tab item based on scroll direction
            if (sender is TabControl tabControl)
            {
                if (e.Location.Y <= tabControl.ItemSize.Height)
                {
                    int intScrollAmount = e.Delta;
                    int intSelectedTabIndex = tabControl.SelectedIndex;

                    if (intScrollAmount < 0)
                    {
                        if (intSelectedTabIndex < tabControl.TabCount - 1)
                            tabControl.SelectedIndex = intSelectedTabIndex + 1;
                    }
                    else if (intSelectedTabIndex > 0)
                        tabControl.SelectedIndex = intSelectedTabIndex - 1;
                }
            }
        }

        /// <summary>
        /// Update the Window title to show the Character's name and unsaved changes status.
        /// </summary>
        public void UpdateWindowTitle(bool blnCanSkip)
        {
            if (Text.EndsWith('*') == _blnIsDirty && blnCanSkip)
                return;

            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            string strTitle = _objCharacter.CharacterName + strSpaceCharacter + '-' + strSpaceCharacter + FormMode + strSpaceCharacter + '(' + _objOptions.Name + ')';
            if (_blnIsDirty)
                strTitle += '*';
            this.DoThreadSafe(() => Text = strTitle);
        }

        /// <summary>
        /// Save the Character.
        /// </summary>
        public virtual bool SaveCharacter(bool blnNeedConfirm = true, bool blnDoCreated = false)
        {
            // If the Character does not have a file name, trigger the Save As menu item instead.
            if (string.IsNullOrEmpty(_objCharacter.FileName))
            {
                return SaveCharacterAs();
            }
            // If the Created is checked, make sure the user wants to actually save this character.
            if (blnDoCreated)
            {
                if (blnNeedConfirm && !ConfirmSaveCreatedCharacter())
                {
                    return false;
                }
            }

            Cursor objOldCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            if (_objCharacter.Save())
            {
                GlobalOptions.MostRecentlyUsedCharacters.Insert(0, _objCharacter.FileName);
                IsDirty = false;
                Cursor = objOldCursor;

                // If this character has just been saved as Created, close this form and re-open the character which will open it in the Career window instead.
                if (blnDoCreated)
                {
                    SaveCharacterAsCreated();
                }

                return true;
            }
            Cursor = objOldCursor;
            return false;
        }

        /// <summary>
        /// Save the Character using the Save As dialogue box.
        /// </summary>
        public virtual bool SaveCharacterAs(bool blnDoCreated = false)
        {
            // If the Created is checked, make sure the user wants to actually save this character.
            if (blnDoCreated)
            {
                if (!ConfirmSaveCreatedCharacter())
                {
                    return false;
                }
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Chum5", GlobalOptions.Language) + '|' + LanguageManager.GetString("DialogFilter_All", GlobalOptions.Language)
            };

            string[] strFile = _objCharacter.FileName.Split(Path.DirectorySeparatorChar);
            string strShowFileName = strFile[strFile.Length - 1];

            if (string.IsNullOrEmpty(strShowFileName))
                strShowFileName = _objCharacter.CharacterName;

            saveFileDialog.FileName = strShowFileName;

            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                _objCharacter.FileName = saveFileDialog.FileName;
                return SaveCharacter(false, blnDoCreated);
            }

            return false;
        }

        /// <summary>
        /// Save the character as Created and re-open it in Career Mode.
        /// </summary>
        public virtual void SaveCharacterAsCreated() { }

        /// <summary>
        /// Verify that the user wants to save this character as Created.
        /// </summary>
        public virtual bool ConfirmSaveCreatedCharacter() { return true; }

        /// <summary>
        /// The frmViewer window being used by the character.
        /// </summary>
        public frmViewer PrintWindow
        {
            get => _frmPrintView;
            set => _frmPrintView = value;
        }

        public void DoPrint()
        {
            // If a reference to the Viewer window does not yet exist for this character, open a new Viewer window and set the reference to it.
            // If a Viewer window already exists for this character, use it instead.
            if (_frmPrintView == null)
            {
                List<Character> lstCharacters = new List<Character>
                {
                    CharacterObject
                };
                _frmPrintView = new frmViewer
                {
                    Characters = lstCharacters
                };
                _frmPrintView.Show();
            }
            else
            {
                _frmPrintView.Activate();
            }
            _frmPrintView.RefreshCharacters();
            if (Program.MainForm.PrintMultipleCharactersForm?.CharacterList?.Contains(CharacterObject) == true)
                Program.MainForm.PrintMultipleCharactersForm.PrintViewForm?.RefreshCharacters();
        }
        
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _frmPrintView?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
