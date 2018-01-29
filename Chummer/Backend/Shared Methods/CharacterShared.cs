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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using TheArtOfDev.HtmlRenderer.WinForms;
using System.Text;
using System.ComponentModel;
using Chummer.UI.Attributes;
using System.Collections.ObjectModel;

namespace Chummer
{
    /// <summary>
    /// Contains functionality shared between frmCreate and frmCareer
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public class CharacterShared : Form, IDisposable
    {
        private readonly Character _objCharacter;
        private readonly ObservableCollection<CharacterAttrib> _lstPrimaryAttributes;
        private readonly ObservableCollection<CharacterAttrib> _lstSpecialAttributes;
        private readonly CharacterOptions _objOptions;
        private bool _blnIsDirty = false;
        private bool _blnRequestCharacterUpdate = false;
        private frmViewer _frmPrintView;

        public CharacterShared(Character objCharacter)
        {
            _objCharacter = objCharacter;
            _objOptions = _objCharacter.Options;
            _objCharacter.CharacterNameChanged += ForceUpdateWindowTitle;

            _lstPrimaryAttributes = new ObservableCollection<CharacterAttrib>
            {
                CharacterObject.BOD,
                CharacterObject.AGI,
                CharacterObject.REA,
                CharacterObject.STR,
                CharacterObject.CHA,
                CharacterObject.INT,
                CharacterObject.LOG,
                CharacterObject.WIL
            };

            _lstSpecialAttributes = new ObservableCollection<CharacterAttrib>
            {
                CharacterObject.EDG
            };
            if (CharacterObject.MAGEnabled)
            {
                _lstSpecialAttributes.Add(CharacterObject.MAG);
                if (CharacterObjectOptions.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
                    _lstSpecialAttributes.Add(CharacterObject.MAGAdept);
            }
            if (CharacterObject.RESEnabled)
            {
                _lstSpecialAttributes.Add(CharacterObject.RES);
            }
            if (CharacterObject.DEPEnabled)
            {
                _lstSpecialAttributes.Add(CharacterObject.DEP);
            }
        }

        [Obsolete("This constructor is for use by form designers only.", true)]
        public CharacterShared()
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
                return objX.Equals(objY);
            }

            public static bool operator !=(object objX, TransportWrapper objY)
            {
                return !objX.Equals(objY);
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

        public Stopwatch Autosave_StopWatch { get; } = Stopwatch.StartNew();
        /// <summary>
        /// Automatically Save the character to a backup folder.
        /// </summary>
        public void AutoSaveCharacter()
        {
            Cursor = Cursors.WaitCursor;
            string strAutosavePath = Path.Combine(Application.StartupPath, "saves", "autosave");
            if (!Directory.Exists(strAutosavePath))
            {
                try
                {
                    Directory.CreateDirectory(strAutosavePath);
                }
                catch (UnauthorizedAccessException)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
                    Autosave_StopWatch.Restart();
                    return;
                }
            }
            
            string[] strFile = _objCharacter.FileName.Split(Path.DirectorySeparatorChar);
            string strShowFileName = strFile[strFile.Length - 1];

            if (string.IsNullOrEmpty(strShowFileName))
                strShowFileName = _objCharacter.CharacterName;
            string strFilePath = Path.Combine(strAutosavePath, strShowFileName);
            _objCharacter.Save(strFilePath);
            Cursor = Cursors.Default;
            Autosave_StopWatch.Restart();
        }

        /// <summary>
        /// Update the label and tooltip for the character's Condition Monitors.
        /// </summary>
        /// <param name="lblPhysical"></param>
        /// <param name="lblStun"></param>
        /// <param name="tipTooltip"></param>
        /// <param name="_objImprovementManager"></param>
        protected void UpdateConditionMonitor(Label lblPhysical, Label lblStun, ToolTip tipTooltip)
        {
            // Condition Monitor.
            int intCMPhysical = _objCharacter.PhysicalCM;
            int intCMStun = _objCharacter.StunCM;

            // Update the Condition Monitor labels.
            lblPhysical.Text = intCMPhysical.ToString();
            lblStun.Text = intCMStun.ToString();
            if (tipTooltip != null)
            {
                int intBOD = _objCharacter.BOD.TotalValue;
                int intWIL = _objCharacter.WIL.TotalValue;
                string strCM = $"8 + ({_objCharacter.BOD.DisplayAbbrev}/2)({(intBOD + 1) / 2})";
                if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.PhysicalCM) != 0)
                    strCM += " + " + LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + " (" +
                             ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.PhysicalCM).ToString() + ')';
                tipTooltip.SetToolTip(lblPhysical, strCM);
                strCM = $"8 + ({_objCharacter.WIL.DisplayAbbrev}/2)({(intWIL + 1) / 2})";
                if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.StunCM) != 0)
                    strCM += " + " + LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + " (" +
                             ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.StunCM).ToString() + ')';
                tipTooltip.SetToolTip(lblStun, strCM);
            }
        }

        /// <summary>
        /// Update the label and tooltip for the character's Armor Rating.
        /// </summary>
        /// <param name="lblArmor"></param>
        /// <param name="tipTooltip"></param>
        /// <param name="objImprovementManager"></param>
        /// <param name="lblCMArmor"></param>
        protected void UpdateArmorRating(Label lblArmor, ToolTip tipTooltip, Label lblCMArmor = null)
        {
            // Armor Ratings.
            int intTotalArmorRating = _objCharacter.TotalArmorRating;
            int intArmorRating = _objCharacter.ArmorRating;
            lblArmor.Text = intTotalArmorRating.ToString();
            if (tipTooltip != null)
            {
                string strArmorToolTip = LanguageManager.GetString("Tip_Armor", GlobalOptions.Language) + " (" + intArmorRating.ToString() + ')';
                if (intArmorRating != intTotalArmorRating)
                    strArmorToolTip += " + " + LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + " (" +
                                       (intTotalArmorRating - intArmorRating).ToString() + ')';
                tipTooltip.SetToolTip(lblArmor, strArmorToolTip);
                if (lblCMArmor != null)
                {
                    lblCMArmor.Text = intTotalArmorRating.ToString();
                    tipTooltip.SetToolTip(lblCMArmor, strArmorToolTip);
                }
            }

            // Remove any Improvements from Armor Encumbrance.
            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.ArmorEncumbrance);
            // Create the Armor Encumbrance Improvements.
            int intEncumbrance = _objCharacter.ArmorEncumbrance;
            if (intEncumbrance < 0)
            {
                ImprovementManager.CreateImprovement(_objCharacter, "AGI", Improvement.ImprovementSource.ArmorEncumbrance, string.Empty,
                    Improvement.ImprovementType.Attribute, "precedence-1", 0, 1, 0, 0, intEncumbrance);
                ImprovementManager.CreateImprovement(_objCharacter, "REA", Improvement.ImprovementSource.ArmorEncumbrance, string.Empty,
                    Improvement.ImprovementType.Attribute, "precedence-1", 0, 1, 0, 0, intEncumbrance);
                ImprovementManager.Commit(_objCharacter);
            }
        }

        /// <summary>
        /// Update the labels and tooltips for the character's Limits.
        /// </summary>
        /// <param name="lblPhysical"></param>
        /// <param name="lblMental"></param>
        /// <param name="lblSocial"></param>
        /// <param name="lblAstral"></param>
        /// <param name="tipTooltip"></param>
        protected void RefreshLimits(Label lblPhysical, Label lblMental, Label lblSocial, Label lblAstral, ToolTip tipTooltip)
        {
            lblPhysical.Text = _objCharacter.LimitPhysical.ToString();
            lblMental.Text = _objCharacter.LimitMental.ToString();
            lblSocial.Text = _objCharacter.LimitSocial.ToString();

            if (tipTooltip != null)
            {
                StringBuilder objPhysical = new StringBuilder(
                    $"({_objCharacter.STR.DisplayAbbrev} [{_objCharacter.STR.TotalValue}] * 2) + {_objCharacter.BOD.DisplayAbbrev} [{_objCharacter.BOD.TotalValue}] + {_objCharacter.REA.DisplayAbbrev} [{_objCharacter.REA.TotalValue}] / 3");
                StringBuilder objMental = new StringBuilder(
                    $"({_objCharacter.LOG.DisplayAbbrev} [{_objCharacter.LOG.TotalValue}] * 2) + {_objCharacter.INT.DisplayAbbrev} [{_objCharacter.INT.TotalValue}] + {_objCharacter.WIL.DisplayAbbrev} [{_objCharacter.WIL.TotalValue}] / 3");
                StringBuilder objSocial = new StringBuilder(
                    $"({_objCharacter.CHA.DisplayAbbrev} [{_objCharacter.CHA.TotalValue}] * 2) + {_objCharacter.WIL.DisplayAbbrev} [{_objCharacter.WIL.TotalValue}] + {_objCharacter.ESS.DisplayAbbrev} [{_objCharacter.Essence.ToString(GlobalOptions.CultureInfo)}] / 3");

                foreach (Improvement objLoopImprovement in _objCharacter.Improvements.Where(
                    objLoopImprovment => (objLoopImprovment.ImproveType == Improvement.ImprovementType.PhysicalLimit
                    || objLoopImprovment.ImproveType == Improvement.ImprovementType.SocialLimit
                    || objLoopImprovment.ImproveType == Improvement.ImprovementType.MentalLimit) && objLoopImprovment.Enabled))
                {
                    switch (objLoopImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.PhysicalLimit:
                            objPhysical.Append($" + {_objCharacter.GetObjectName(objLoopImprovement, GlobalOptions.Language)} ({objLoopImprovement.Value})");
                            break;
                        case Improvement.ImprovementType.MentalLimit:
                            objMental.Append($" + {_objCharacter.GetObjectName(objLoopImprovement, GlobalOptions.Language)} ({objLoopImprovement.Value})");
                            break;
                        case Improvement.ImprovementType.SocialLimit:
                            objSocial.Append($" + {_objCharacter.GetObjectName(objLoopImprovement, GlobalOptions.Language)} ({objLoopImprovement.Value})");
                            break;
                    }
                }

                tipTooltip.SetToolTip(lblPhysical, objPhysical.ToString());
                tipTooltip.SetToolTip(lblMental, objMental.ToString());
                tipTooltip.SetToolTip(lblSocial, objSocial.ToString());
            }

            lblAstral.Text = _objCharacter.LimitAstral.ToString();
        }

        /// <summary>
        /// Edit and update a Limit Modifier.
        /// </summary>
        /// <param name="treLimit"></param>
        /// <param name="cmsLimitModifier"></param>
        protected void UpdateLimitModifier(TreeView treLimit, ContextMenuStrip cmsLimitModifier)
        {
            TreeNode objSelectedNode = treLimit.SelectedNode;
            string strGuid = objSelectedNode?.Tag.ToString();
            if (string.IsNullOrEmpty(strGuid) || strGuid.IsEmptyGuid())
                return;
            LimitModifier objLimitModifier = _objCharacter.LimitModifiers.FindById(strGuid);
            //If the LimitModifier couldn't be found (Ie it comes from an Improvement or the user hasn't properly selected a treenode, fail out early.
            if (objLimitModifier == null)
            {
                MessageBox.Show(LanguageManager.GetString("Warning_NoLimitFound", GlobalOptions.Language));
                return;
            }
            using (frmSelectLimitModifier frmPickLimitModifier = new frmSelectLimitModifier(objLimitModifier))
            {
                frmPickLimitModifier.ShowDialog(this);

                if (frmPickLimitModifier.DialogResult == DialogResult.Cancel)
                    return;

                //Remove the old LimitModifier to ensure we don't double up.
                _objCharacter.LimitModifiers.Remove(objLimitModifier);
                // Create the new limit modifier.
                objLimitModifier = new LimitModifier(_objCharacter);
                string strLimit = treLimit.SelectedNode.Parent.Text;
                string strCondition = frmPickLimitModifier.SelectedCondition;
                objLimitModifier.Create(frmPickLimitModifier.SelectedName, frmPickLimitModifier.SelectedBonus, strLimit, strCondition);
                objLimitModifier.Guid = new Guid(strGuid);

                _objCharacter.LimitModifiers.Add(objLimitModifier);

                //Add the new treeview node for the LimitModifier.
                objSelectedNode.Parent.Nodes.Add(objLimitModifier.CreateTreeNode(cmsLimitModifier));
                objSelectedNode.Remove();
            }
        }

        protected void RefreshAttributes(FlowLayoutPanel pnlAttributes, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            if (notifyCollectionChangedEventArgs == null)
            {
                pnlAttributes.Controls.Clear();

                foreach (CharacterAttrib objAttrib in _lstPrimaryAttributes.Concat(_lstSpecialAttributes))
                {
                    AttributeControl objControl = new AttributeControl(objAttrib);
                    objControl.ValueChanged += MakeDirtyWithCharacterUpdate;
                    pnlAttributes.Controls.Add(objControl);
                }
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
        /// <param name="treSpells">Treenode that will be cleared and populated.</param>
        /// <param name="cmsSpell">ContextMenuStrip that will be added to each power.</param>
        protected void RefreshSpells(TreeView treSpells, ContextMenuStrip cmsSpell, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
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
                string strSelectedId = treSpells.SelectedNode?.Tag.ToString();

                // Clear the default nodes of entries.
                treSpells.Nodes.Clear();

                // Add the Spells that exist.
                foreach (Spell objSpell in _objCharacter.Spells)
                {
                    AddToTree(objSpell, false);
                }
                treSpells.SortCustom(strSelectedId);
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
                                TreeNode objNode = treSpells.FindNode(objSpell.InternalId);
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
                            RefreshSpells(treSpells, cmsSpell);
                            break;
                        }
                    case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents = new List<TreeNode>();
                            foreach (Spell objSpell in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treSpells.FindNode(objSpell.InternalId);
                                if (objNode != null)
                                {
                                    lstOldParents.Add(objNode.Parent);
                                    objNode.Remove();
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
                TreeNode objNode = objSpell.CreateTreeNode(cmsSpell);
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

        protected void RefreshAIPrograms(TreeView treAIPrograms, ContextMenuStrip cmsAdvancedProgram, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            TreeNode objParentNode = null;
            if (notifyCollectionChangedEventArgs == null)
            {
                string strSelectedId = treAIPrograms.SelectedNode?.Tag.ToString();

                treAIPrograms.Nodes.Clear();

                // Populate AI Programs.
                foreach (AIProgram objAIProgram in CharacterObject.AIPrograms)
                {
                    AddToTree(objAIProgram, false);
                }

                treAIPrograms.SortCustom(strSelectedId);
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
                                TreeNode objNode = treAIPrograms.FindNode(objAIProgram.InternalId);
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
                                TreeNode objNode = treAIPrograms.FindNode(objAIProgram.InternalId);
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
                TreeNode objNode = objAIProgram.CreateTreeNode(cmsAdvancedProgram);
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

        protected void RefreshComplexForms(TreeView treComplexForms, ContextMenuStrip cmsComplexForm, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            TreeNode objParentNode = null;
            if (notifyCollectionChangedEventArgs == null)
            {
                string strSelectedId = treComplexForms.SelectedNode?.Tag.ToString();

                treComplexForms.Nodes.Clear();

                // Populate Complex Forms.
                foreach (ComplexForm objComplexForm in CharacterObject.ComplexForms)
                {
                    AddToTree(objComplexForm, false);
                }

                treComplexForms.SortCustom(strSelectedId);
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
                                TreeNode objNode = treComplexForms.FindNode(objComplexForm.InternalId);
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
                            RefreshComplexForms(treComplexForms, cmsComplexForm);
                            break;
                        }
                    case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents = new List<TreeNode>();
                            foreach (ComplexForm objComplexForm in notifyCollectionChangedEventArgs.OldItems)
                            {
                                TreeNode objNode = treComplexForms.FindNode(objComplexForm.InternalId);
                                if (objNode != null)
                                {
                                    lstOldParents.Add(objNode.Parent);
                                    objNode.Remove();
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
                }
            }

            void AddToTree(ComplexForm objComplexForm, bool blnSingleAdd = true)
            {
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
                TreeNode objNode = objComplexForm.CreateTreeNode(cmsComplexForm);
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

        protected void RefreshLimitModifiers(TreeView treLimit, ContextMenuStrip cmsLimitModifier)
        {
            treLimit.Nodes.Clear();

            TreeNode[] aobjLimitNodes = new TreeNode[(int)LimitType.NumLimitTypes];

            // Populate Limit Modifiers.
            foreach (LimitModifier objLimitModifier in CharacterObject.LimitModifiers)
            {
                int intTargetLimit = (int)Enum.Parse(typeof(LimitType), objLimitModifier.Limit);
                TreeNode objParentNode = GetLimitModifierParentNode(intTargetLimit);
                if (!objParentNode.Nodes.ContainsKey(objLimitModifier.DisplayName))
                {
                    objParentNode.Nodes.Add(objLimitModifier.CreateTreeNode(cmsLimitModifier));
                }
            }

            // Populate Limit Modifiers from Improvements
            foreach (Improvement objImprovement in CharacterObject.Improvements.Where(objImprovement => objImprovement.ImproveSource == Improvement.ImprovementSource.Custom))
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
                    string strName = objImprovement.UniqueName + ": ";
                    if (objImprovement.Value > 0)
                        strName += '+';
                    strName += objImprovement.Value.ToString();
                    if (!string.IsNullOrEmpty(objImprovement.Condition))
                        strName += ", " + objImprovement.Condition;
                    if (!objParentNode.Nodes.ContainsKey(strName))
                    {
                        TreeNode newNode = new TreeNode
                        {
                            Name = strName,
                            Text = strName,
                            Tag = objImprovement.SourceName,
                            ContextMenuStrip = cmsLimitModifier
                        };
                        if (!string.IsNullOrEmpty(objImprovement.Notes))
                            newNode.ForeColor = Color.SaddleBrown;
                        newNode.ToolTipText = objImprovement.Notes.WordWrap(100);
                        if (string.IsNullOrEmpty(objImprovement.ImprovedName))
                        {
                            if (objImprovement.ImproveType == Improvement.ImprovementType.SocialLimit)
                                objImprovement.ImprovedName = "Social";
                            else if (objImprovement.ImproveType == Improvement.ImprovementType.MentalLimit)
                                objImprovement.ImprovedName = "Mental";
                            else
                                objImprovement.ImprovedName = "Physical";
                        }

                        objParentNode.Nodes.Add(newNode);
                    }
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
                    objParentNode.Expand();
                }
                return objParentNode;
            }
        }

        protected void RefreshInitiationGradesTree(TreeView treMetamagic, ContextMenuStrip cmsMetamagic, ContextMenuStrip cmsInitiationNotes)
        {
            treMetamagic.Nodes.Clear();
            foreach (InitiationGrade objGrade in _objCharacter.InitiationGrades)
            {
                TreeNode nodGrade = objGrade.CreateTreeNode(cmsMetamagic);

                foreach (Art objArt in _objCharacter.Arts)
                {
                    if (objArt.Grade == objGrade.Grade)
                    {
                        nodGrade.Nodes.Add(objArt.CreateTreeNode(cmsInitiationNotes, true));
                    }
                }
                foreach (Metamagic objMetamagic in _objCharacter.Metamagics)
                {
                    if (objMetamagic.Grade == objGrade.Grade)
                    {
                        nodGrade.Nodes.Add(objMetamagic.CreateTreeNode(cmsInitiationNotes, true));
                    }
                }
                foreach (Spell objSpell in _objCharacter.Spells)
                {
                    if (objSpell.Grade == objGrade.Grade)
                    {
                        nodGrade.Nodes.Add(objSpell.CreateTreeNode(cmsInitiationNotes, true));
                    }
                }
                foreach (Enhancement objEnhancement in _objCharacter.Enhancements)
                {
                    if (objEnhancement.Grade == objGrade.Grade)
                    {
                        nodGrade.Nodes.Add(objEnhancement.CreateTreeNode(cmsInitiationNotes, true));
                    }
                }
                foreach (Power objPower in _objCharacter.Powers)
                {
                    foreach (Enhancement objEnhancement in objPower.Enhancements)
                    {
                        if (objEnhancement.Grade == objGrade.Grade)
                        {
                            nodGrade.Nodes.Add(objEnhancement.CreateTreeNode(cmsInitiationNotes, true));
                        }
                    }
                }
                treMetamagic.Nodes.Add(nodGrade);
            }
            foreach (Metamagic objMetamagic in _objCharacter.Metamagics)
            {
                if (objMetamagic.Grade < 0)
                {
                    treMetamagic.Nodes.Add(objMetamagic.CreateTreeNode(cmsInitiationNotes, true));
                }
            }
            treMetamagic.ExpandAll();
        }

        /// <summary>
        /// Clears and updates the treeview for Critter Powers. Typically called as part of AddQuality or UpdateCharacterInfo.
        /// </summary>
        /// <param name="treCritterPowers">Treenode that will be cleared and populated.</param>
        /// <param name="cmsCritterPowers">ContextMenuStrip that will be added to each power.</param>
        protected void RefreshCritterPowers(TreeView treCritterPowers, ContextMenuStrip cmsCritterPowers, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            TreeNode objPowersNode = null;
            TreeNode objWeaknessesNode = null;

            if (notifyCollectionChangedEventArgs == null)
            {
                string strSelectedId = treCritterPowers.SelectedNode?.Tag.ToString();
                treCritterPowers.Nodes.Clear();
                // Add the Critter Powers that exist.
                foreach (CritterPower objPower in _objCharacter.CritterPowers)
                {
                    AddToTree(objPower, false);
                }

                treCritterPowers.SortCustom(strSelectedId);
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
                                TreeNode objNode = treCritterPowers.FindNode(objPower.InternalId);
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
                TreeNode objParentNode = null;
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
                TreeNode objNode = objPower.CreateTreeNode(cmsCritterPowers);
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
        /// <param name="blnForce">Forces a refresh of the TreeNode despite a match.</param>
        protected void RefreshQualities(TreeView treQualities, ContextMenuStrip cmsQuality, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            TreeNode objPositiveQualityRoot = null;
            TreeNode objNegativeQualityRoot = null;
            TreeNode objLifeModuleRoot = null;

            if (notifyCollectionChangedEventArgs == null)
            {
                string strSelectedNode = treQualities.SelectedNode?.Tag.ToString();

                // Create the root nodes.
                treQualities.Nodes.Clear();

                // Multiple instances of the same quality are combined into just one entry with a number next to it (e.g. 6 discrete entries of "Focused Concentration" become "Focused Concentration 6")
                HashSet<string> strQualitiesToPrint = new HashSet<string>();
                foreach (Quality objQuality in _objCharacter.Qualities)
                {
                    strQualitiesToPrint.Add(objQuality.QualityId + ' ' + objQuality.GetSourceName(GlobalOptions.Language) + ' ' + objQuality.Extra);
                }
                // Populate the Qualities list.
                foreach (Quality objQuality in _objCharacter.Qualities)
                {
                    if (!strQualitiesToPrint.Remove(objQuality.QualityId + ' ' + objQuality.GetSourceName(GlobalOptions.Language) + ' ' + objQuality.Extra))
                        continue;

                    AddToTree(objQuality, false);
                }

                treQualities.SortCustom(strSelectedNode);
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
                TreeNode objNode = objQuality.CreateTreeNode(cmsQuality);
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
                    objQualityNode.Text = ((Quality)objQualityNode.Tag).DisplayName(GlobalOptions.Language);
                }
            }
            treQualities.SortCustom(objSelectedNode?.Tag);
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

        /// <summary>
        /// Populate the TreeView that contains all of the character's Gear.
        /// </summary>
        protected void PopulateGearList(TreeView treGear, ContextMenuStrip cmsGearLocation, ContextMenuStrip cmsGear, bool blnCommlinksOnly)
        {
            string strSelectedId = treGear.SelectedNode?.Tag.ToString();

            // Populate Gear.
            // Create the root node.
            treGear.Nodes.Clear();
            TreeNode objRoot = new TreeNode
            {
                Tag = "Node_SelectedGear",
                Text = LanguageManager.GetString("Node_SelectedGear", GlobalOptions.Language)
            };
            treGear.Nodes.Add(objRoot);

            // Start by populating Locations.
            foreach (string strLocation in CharacterObject.GearLocations)
            {
                TreeNode objLocation = new TreeNode
                {
                    Tag = strLocation,
                    Text = strLocation,
                    ContextMenuStrip = cmsGearLocation
                };
                treGear.Nodes.Add(objLocation);
            }

            foreach (Gear objGear in CharacterObject.Gear)
            {
                if (!blnCommlinksOnly || objGear.IsCommlink)
                {
                    TreeNode objParent = objRoot;
                    if (!string.IsNullOrEmpty(objGear.Location))
                    {
                        foreach (TreeNode objFind in treGear.Nodes)
                        {
                            if (objFind.Text == objGear.Location)
                            {
                                objParent = objFind;
                                break;
                            }
                        }
                    }
                    objParent.Nodes.Add(objGear.CreateTreeNode(cmsGear));
                }
            }
            foreach (TreeNode objNode in treGear.Nodes)
                if (objNode.Nodes.Count > 0)
                    objNode.Expand();

            TreeNode objSelectedNode = treGear.FindNode(strSelectedId);
            if (objSelectedNode != null)
                treGear.SelectedNode = objSelectedNode;
        }

        /// <summary>
        /// Populate the TreeView that contains all of the character's Weapons.
        /// </summary>
        protected void PopulateWeaponList(TreeView treWeapons, ContextMenuStrip cmsWeaponLocation, ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear)
        {
            string strSelectedId = treWeapons.SelectedNode?.Tag.ToString();

            // Populate Weapons.
            // Create the root node.
            treWeapons.Nodes.Clear();
            TreeNode objRoot = new TreeNode
            {
                Tag = "Node_SelectedWeapons",
                Text = LanguageManager.GetString("Node_SelectedWeapons", GlobalOptions.Language)
            };
            treWeapons.Nodes.Add(objRoot);

            // Start by populating Locations.
            foreach (string strLocation in CharacterObject.WeaponLocations)
            {
                TreeNode objLocation = new TreeNode
                {
                    Tag = strLocation,
                    Text = strLocation,
                    ContextMenuStrip = cmsWeaponLocation
                };
                treWeapons.Nodes.Add(objLocation);
            }
            foreach (Weapon objWeapon in CharacterObject.Weapons)
            {
                TreeNode objParent = objRoot;
                if (!string.IsNullOrEmpty(objWeapon.Location))
                {
                    foreach (TreeNode objFind in treWeapons.Nodes)
                    {
                        if (objFind.Text == objWeapon.Location)
                        {
                            objParent = objFind;
                            break;
                        }
                    }
                }
                objParent.Nodes.Add(objWeapon.CreateTreeNode(cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear));
            }
            foreach (TreeNode objNode in treWeapons.Nodes)
                if (objNode.Nodes.Count > 0)
                    objNode.Expand();

            TreeNode objSelectedNode = treWeapons.FindNode(strSelectedId);
            if (objSelectedNode != null)
                treWeapons.SelectedNode = objSelectedNode;
        }

        /// <summary>
        /// Populate the TreeView that contains all of the character's Armor.
        /// </summary>
        protected void PopulateArmorList(TreeView treArmor, ContextMenuStrip cmsArmorLocation, ContextMenuStrip cmsArmor, ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear)
        {
            string strSelectedId = treArmor.SelectedNode?.Tag.ToString();

            // Populate Armor.
            // Create the root node.
            treArmor.Nodes.Clear();
            TreeNode objRoot = new TreeNode
            {
                Tag = "Node_SelectedArmor",
                Text = LanguageManager.GetString("Node_SelectedArmor", GlobalOptions.Language)
            };
            treArmor.Nodes.Add(objRoot);

            // Start by populating Locations.
            foreach (string strLocation in CharacterObject.ArmorLocations)
            {
                TreeNode objLocation = new TreeNode
                {
                    Tag = strLocation,
                    Text = strLocation,
                    ContextMenuStrip = cmsArmorLocation
                };
                treArmor.Nodes.Add(objLocation);
            }
            foreach (Armor objArmor in CharacterObject.Armor)
            {
                TreeNode objParent = objRoot;
                if (!string.IsNullOrEmpty(objArmor.Location))
                {
                    foreach (TreeNode objFind in treArmor.Nodes)
                    {
                        if (objFind.Text == objArmor.Location)
                        {
                            objParent = objFind;
                            break;
                        }
                    }
                }
                objParent.Nodes.Add(objArmor.CreateTreeNode(cmsArmor, cmsArmorMod, cmsArmorGear));
            }
            foreach (TreeNode objNode in treArmor.Nodes)
                if (objNode.Nodes.Count > 0)
                    objNode.Expand();

            TreeNode objSelectedNode = treArmor.FindNode(strSelectedId);
            if (objSelectedNode != null)
                treArmor.SelectedNode = objSelectedNode;
        }
        
        /// <summary>
        /// Populate the TreeView that contains all of the character's Cyberware and Bioware.
        /// </summary>
        protected void PopulateCyberwareList(TreeView treCyberware, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear)
        {
            string strSelectedId = treCyberware.SelectedNode?.Tag.ToString();

            // Create the root nodes.
            treCyberware.Nodes.Clear();
            TreeNode objCyberwareRoot = null;
            TreeNode objBiowareRoot = null;
            TreeNode objModularRoot = null;
            TreeNode objHoleNode = null;

            foreach (Cyberware objCyberware in CharacterObject.Cyberware)
            {
                if (objCyberware.SourceID == Cyberware.EssenceHoleGUID && objHoleNode == null)
                {
                    objHoleNode = objCyberware.CreateTreeNode(null, null);
                    treCyberware.Nodes.Insert(3, objHoleNode);
                }
                // Populate Cyberware.
                else if (objCyberware.SourceType == Improvement.ImprovementSource.Cyberware)
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
                        objCyberwareRoot.Nodes.Add(objCyberware.CreateTreeNode(cmsCyberware, cmsCyberwareGear));
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
                        objModularRoot.Nodes.Add(objCyberware.CreateTreeNode(cmsCyberware, cmsCyberwareGear));
                    }
                }
                // Populate Bioware.
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
                    objBiowareRoot.Nodes.Add(objCyberware.CreateTreeNode(cmsCyberware, cmsCyberwareGear));
                }
            }

            treCyberware.SortCustom(strSelectedId);
        }

        /// <summary>
        /// Populate the TreeView that contains all of the character's Vehicles.
        /// </summary>
        protected void PopulateVehicleList(TreeView treVehicles, ContextMenuStrip cmsVehicleLocation, ContextMenuStrip cmsVehicle, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, ContextMenuStrip cmsVehicleGear, ContextMenuStrip cmsVehicleWeaponMount, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear)
        {
            string strSelectedId = treVehicles.SelectedNode?.Tag.ToString();

            // Populate Gear.
            // Create the root node.
            treVehicles.Nodes.Clear();
            TreeNode objRoot = new TreeNode
            {
                Tag = "Node_SelectedVehicles",
                Text = LanguageManager.GetString("Node_SelectedVehicles", GlobalOptions.Language)
            };
            treVehicles.Nodes.Add(objRoot);

            // Start by populating Locations.
            foreach (string strLocation in CharacterObject.VehicleLocations)
            {
                TreeNode objLocation = new TreeNode
                {
                    Tag = strLocation,
                    Text = strLocation,
                    ContextMenuStrip = cmsVehicleLocation
                };
                treVehicles.Nodes.Add(objLocation);
            }

            foreach (Vehicle objVehicle in CharacterObject.Vehicles)
            {
                TreeNode objParent = objRoot;
                if (!string.IsNullOrEmpty(objVehicle.Location))
                {
                    foreach (TreeNode objFind in treVehicles.Nodes)
                    {
                        if (objFind.Text == objVehicle.Location)
                        {
                            objParent = objFind;
                            break;
                        }
                    }
                }
                objParent.Nodes.Add(objVehicle.CreateTreeNode(cmsVehicle, cmsVehicleLocation, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsVehicleGear, cmsVehicleWeaponMount, cmsCyberware, cmsCyberwareGear));
            }
            foreach (TreeNode objNode in treVehicles.Nodes)
                if (objNode.Nodes.Count > 0)
                    objNode.Expand();

            TreeNode objSelectedNode = treVehicles.FindNode(strSelectedId);
            if (objSelectedNode != null)
                treVehicles.SelectedNode = objSelectedNode;
        }

        /// <summary>
        /// Populate the list of Bonded Foci.
        /// </summary>
        public void PopulateFocusList(TreeView treFoci, ContextMenuStrip cmsGear)
        {
            string strSelectedId = treFoci.SelectedNode?.Tag.ToString();

            treFoci.Nodes.Clear();

            int intFociTotal = 0;
            bool blnWarned = false;

            int intMaxFocusTotal = _objCharacter.MAG.TotalValue * 5;
            if (_objOptions.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                intMaxFocusTotal = Math.Min(intMaxFocusTotal, _objCharacter.MAGAdept.TotalValue * 5);
            foreach (Gear objGear in _objCharacter.Gear.Where(objGear => objGear.Category == "Foci" || objGear.Category == "Metamagic Foci"))
            {
                List<Focus> lstRemoveFoci = new List<Focus>();
                TreeNode objNode = objGear.CreateTreeNode(cmsGear);
                objNode.Text = objNode.Text.Replace(LanguageManager.GetString("String_Rating", GlobalOptions.Language), LanguageManager.GetString("String_Force", GlobalOptions.Language));
                foreach (Focus objFocus in _objCharacter.Foci)
                {
                    if (objFocus.GearId == objGear.InternalId)
                    {
                        objNode.Checked = true;
                        objFocus.Rating = objGear.Rating;
                        intFociTotal += objFocus.Rating;
                        // Do not let the number of BP spend on bonded Foci exceed MAG * 5.
                        if (intFociTotal > intMaxFocusTotal && !_objCharacter.IgnoreRules)
                        {
                            // Mark the Gear a Bonded.
                            foreach (Gear objCharacterGear in _objCharacter.Gear)
                            {
                                if (objCharacterGear.InternalId == objFocus.GearId)
                                    objCharacterGear.Bonded = false;
                            }
                            lstRemoveFoci.Add(objFocus);
                            if (!blnWarned)
                            {
                                objNode.Checked = false;
                                MessageBox.Show(LanguageManager.GetString("Message_FocusMaximumForce", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_FocusMaximum", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                blnWarned = true;
                                break;
                            }
                        }
                    }
                }
                foreach (Focus objFocus in lstRemoveFoci)
                {
                    _objCharacter.Foci.Remove(objFocus);
                }
                treFoci.Nodes.Add(objNode);
            }

            // Add Stacked Foci.
            foreach (Gear objGear in _objCharacter.Gear)
            {
                if (objGear.Category == "Stacked Focus")
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

                            treFoci.Nodes.Add(objStack.CreateTreeNode(objGear, null));
                        }
                    }
                }
            }

            treFoci.SortCustom(strSelectedId);
        }

        protected void RefreshMartialArts(TreeView treMartialArts, ContextMenuStrip cmsMartialArts, ContextMenuStrip cmsTechnique, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            string strSelectedId = treMartialArts.SelectedNode?.Tag.ToString();
            
            TreeNode objMartialArtsParentNode = null;
            TreeNode objQualityNode = null;

            if (notifyCollectionChangedEventArgs == null)
            {
                treMartialArts.Nodes.Clear();

                foreach (MartialArt objMartialArt in CharacterObject.MartialArts)
                {
                    AddToTree(objMartialArt, false);
                }

                treMartialArts.SortCustom(strSelectedId);
            }
            else
            {
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (MartialArt objMartialArt in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objMartialArt);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (MartialArt objMartialArt in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objMartialArt.Techniques.CollectionChanged -= (x, y) => RefreshMartialArtTechniques(treMartialArts, objMartialArt, cmsTechnique, y);
                                TreeNode objNode = treMartialArts.FindNode(objMartialArt.InternalId);
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
                                objMartialArt.Techniques.CollectionChanged -= (x, y) => RefreshMartialArtTechniques(treMartialArts, objMartialArt, cmsTechnique, y);
                                TreeNode objNode = treMartialArts.FindNode(objMartialArt.InternalId);
                                if (objNode != null)
                                {
                                    lstOldParents.Add(objNode.Parent);
                                    objNode.Remove();
                                }
                            }
                            foreach (MartialArt objMartialArt in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objMartialArt);
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

                TreeNode objParentNode = null;
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
                objMartialArt.Techniques.CollectionChanged += (x, y) => RefreshMartialArtTechniques(treMartialArts, objMartialArt, cmsTechnique, y);
            }
        }
        
        protected void RefreshMartialArtTechniques(TreeView treMartialArts, MartialArt objMartialArt, ContextMenuStrip cmsTechnique, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;
            TreeNode nodMartialArt = treMartialArts.FindNode(objMartialArt.InternalId);
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
                            nodMartialArt.FindNode(objTechnique.InternalId)?.Remove();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (MartialArtTechnique objTechnique in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodMartialArt.FindNode(objTechnique.InternalId)?.Remove();
                        }
                        foreach (MartialArtTechnique objTechnique in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objTechnique);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        string strSelectedId = treMartialArts.SelectedNode?.Tag.ToString();

                        nodMartialArt.Nodes.Clear();

                        foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                        {
                            AddToTree(objTechnique, false);
                        }

                        treMartialArts.SortCustom(strSelectedId);
                    }
                    break;
            }

            void AddToTree(MartialArtTechnique objTechnique, bool blnSingleAdd = true)
            {
                TreeNode objNode = objTechnique.CreateTreeNode(cmsTechnique);
                
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
        protected void RefreshCustomImprovements(TreeView treImprovements, ContextMenuStrip cmsImprovementLocation, ContextMenuStrip cmsImprovement, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            string strSelectedId = treImprovements.SelectedNode?.Tag.ToString();
            
            TreeNode objRoot = null;

            if (notifyCollectionChangedEventArgs == null)
            {
                treImprovements.Nodes.Clear();

                objRoot = new TreeNode
                {
                    Tag = "Node_SelectedImprovements",
                    Text = LanguageManager.GetString("Node_SelectedImprovements", GlobalOptions.Language)
                };
                treImprovements.Nodes.Add(objRoot);

                // Populate the Locations.
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
                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom)
                    {
                        AddToTree(objImprovement, false);
                    }
                }

                // Sort the list of Custom Improvements in alphabetical order based on their Custom Name within each Group.
                treImprovements.SortCustom(strSelectedId);
            }
            else
            {
                objRoot = treImprovements.FindNode("Node_SelectedImprovements", false);
                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (Improvement objImprovement in notifyCollectionChangedEventArgs.NewItems)
                            {
                                if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom)
                                {
                                    AddToTree(objImprovement);
                                }
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Improvement objImprovement in notifyCollectionChangedEventArgs.OldItems)
                            {
                                if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom)
                                {
                                    TreeNode objNode = treImprovements.FindNode(objImprovement.SourceName);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent.Tag.ToString() == "Node_SelectedImprovements" && objParent.Nodes.Count == 0)
                                            objParent.Remove();
                                    }
                                }
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshCustomImprovements(treImprovements, cmsImprovementLocation, cmsImprovement);
                            break;
                        }
                    case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents = new List<TreeNode>();
                            foreach (Improvement objImprovement in notifyCollectionChangedEventArgs.OldItems)
                            {
                                if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom)
                                {
                                    TreeNode objNode = treImprovements.FindNode(objImprovement.SourceName);
                                    if (objNode != null)
                                    {
                                        lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }
                                }
                            }
                            foreach (Improvement objImprovement in notifyCollectionChangedEventArgs.NewItems)
                            {
                                if (objImprovement.ImproveSource == Improvement.ImprovementSource.Custom)
                                {
                                    AddToTree(objImprovement);
                                }
                            }
                            foreach (TreeNode objOldParent in lstOldParents)
                            {
                                if (objOldParent.Tag.ToString() == "Node_SelectedImprovements" && objOldParent.Nodes.Count == 0)
                                    objOldParent.Remove();
                            }
                            break;
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
            string strSelectedId = treLifestyles.SelectedNode?.Tag.ToString();
            
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

                    treLifestyles.SortCustom(strSelectedId);
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
                                TreeNode objNode = treLifestyles.FindNode(objLifestyle.InternalId);
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
                                TreeNode objNode = treLifestyles.FindNode(objLifestyle.InternalId);
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
                TreeNode objNode = objLifestyle.CreateTreeNode(cmsBasicLifestyle, cmsAdvancedLifestyle);
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
        /// Populate the Calendar List.
        /// </summary>
        public void RefreshCalendar(ListView lstCalendar, ListChangedEventArgs listChangedEventArgs = null)
        {
            if (listChangedEventArgs == null)
            {
                lstCalendar.Items.Clear();
                for (int i = CharacterObject.Calendar.Count - 1; i >= 0; i--)
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
                            lstCalendar.Items.RemoveAt(listChangedEventArgs.OldIndex);
                        }
                        break;
                    case ListChangedType.ItemChanged:
                    case ListChangedType.ItemMoved:
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

                                panContacts.Controls.Add(objContactControl);
                            }
                            break;
                        case ContactType.Pet:
                            {
                                PetControl objContactControl = new PetControl(objContact);
                                // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                objContactControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                objContactControl.DeleteContact += DeletePet;
                                objContactControl.MouseDown += DragContactControl;

                                panContacts.Controls.Add(objContactControl);
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

                                            panContacts.Controls.Add(objContactControl);
                                        }
                                        break;
                                    case ContactType.Pet:
                                        {
                                            PetControl objPetControl = new PetControl(objLoopContact);
                                            // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                            objPetControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                            objPetControl.DeleteContact += DeletePet;
                                            //objPetControl.MouseDown += DragContactControl;

                                            panContacts.Controls.Add(objPetControl);
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

                                            panContacts.Controls.Add(objContactControl);
                                        }
                                        break;
                                    case ContactType.Pet:
                                        {
                                            PetControl objPetControl = new PetControl(objLoopContact);
                                            // Attach an EventHandler for the ConnectionRatingChanged, LoyaltyRatingChanged, DeleteContact, FileNameChanged Events and OtherCostChanged
                                            objPetControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                            objPetControl.DeleteContact += DeletePet;
                                            //objPetControl.MouseDown += DragContactControl;

                                            panContacts.Controls.Add(objPetControl);
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

        #region ContactControl Events
        protected void DragContactControl(object sender, MouseEventArgs e)
        {
            Control source = (Control)sender;
            source.DoDragDrop(new TransportWrapper(source), DragDropEffects.Move);
        }

        protected void AddContact(object sender, EventArgs e)
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
                if (CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteContact", GlobalOptions.Language)))
                    return;

                CharacterObject.Contacts.Remove(objSender.ContactObject);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }
        #endregion

        #region PetControl Events
        protected void AddPet(object sender, EventArgs e)
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
        protected void AddEnemy(object sender, EventArgs e)
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

        protected void EnemyChanged(object sender, EventArgs e)
        {
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

            int intEnemyMax = 0;
            int intQualityMax = 0;
            string strQualityPoints = string.Empty;
            string strEnemyPoints = string.Empty;
            intEnemyMax = CharacterObject.GameplayOptionQualityLimit;
            intQualityMax = CharacterObject.GameplayOptionQualityLimit;
            strEnemyPoints = intEnemyMax.ToString() + ' ' + LanguageManager.GetString("String_Karma", GlobalOptions.Language);
            strQualityPoints = intQualityMax.ToString() + ' ' + LanguageManager.GetString("String_Karma", GlobalOptions.Language);

            if (intBPUsed < (intEnemyMax * -1) && !CharacterObject.IgnoreRules)
            {
                MessageBox.Show(LanguageManager.GetString("Message_EnemyLimit", GlobalOptions.Language).Replace("{0}", strEnemyPoints), LanguageManager.GetString("MessageTitle_EnemyLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Contact objSenderContact = ((ContactControl)sender).ContactObject;
                int intTotal = (intEnemyMax * -1) - intBPUsed;
                if (e is TextEventArgs objTextArgument)
                {
                    switch (objTextArgument.Text)
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

            if (!CharacterObjectOptions.ExceedNegativeQualities)
            {
                if (intBPUsed + intNegativeQualityBP < (intQualityMax * -1) && !CharacterObject.IgnoreRules)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NegativeQualityLimit", GlobalOptions.Language).Replace("{0}", strQualityPoints), LanguageManager.GetString("MessageTitle_NegativeQualityLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Contact objSenderContact = ((ContactControl)sender).ContactObject;
                    if (e is TextEventArgs objTextArgument)
                    {
                        switch (objTextArgument.Text)
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
        protected void AddContactsFromFile(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select the XML to read.  
            OpenFileDialog dlgOpenFileDialog = new OpenFileDialog
            {
                Filter = "XML Files|*.xml"
            };

            // Show the Dialog.  
            // If the user cancels out, return early.
            if (dlgOpenFileDialog.ShowDialog() == DialogResult.Cancel)
                return;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(dlgOpenFileDialog.FileName);

            XmlNodeList xmlContactList = xmlDoc.SelectNodes("/chummer/contacts/contact");
            if (xmlContactList != null)
            {
                foreach (XmlNode xmlContact in xmlContactList)
                {
                    Contact objContact = new Contact(CharacterObject);
                    objContact.Load(xmlContact);
                    CharacterObject.Contacts.Add(objContact);
                }
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

                    objSpiritControl.RebuildSpiritList(blnIsSpirit ? CharacterObject.MagicTradition : CharacterObject.TechnomancerStream);
                    
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

                                objSpiritControl.RebuildSpiritList(blnIsSpirit ? CharacterObject.MagicTradition : CharacterObject.TechnomancerStream);

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
                                if (objSpirit.EntityType == SpiritType.Spirit)
                                {
                                    for (int i = panSpirits.Controls.Count - 1; i >= 0; i--)
                                    {
                                        if (panSpirits.Controls[i] is SpiritControl objSpiritControl && objSpiritControl.SpiritObject == objSpirit)
                                        {
                                            panSpirits.Controls.RemoveAt(i);
                                            objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                            objSpiritControl.DeleteSpirit -= DeleteSpirit;
                                            objSpiritControl.Dispose();
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = panSprites.Controls.Count - 1; i >= 0; i--)
                                    {
                                        if (panSprites.Controls[i] is SpiritControl objSpiritControl && objSpiritControl.SpiritObject == objSpirit)
                                        {
                                            panSpirits.Controls.RemoveAt(i);
                                            objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                            objSpiritControl.DeleteSpirit -= DeleteSpirit;
                                            objSpiritControl.Dispose();
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Spirit objSpirit in notifyCollectionChangedEventArgs.OldItems)
                            {
                                if (objSpirit.EntityType == SpiritType.Spirit)
                                {
                                    for (int i = panSpirits.Controls.Count - 1; i >= 0; i--)
                                    {
                                        if (panSpirits.Controls[i] is SpiritControl objSpiritControl && objSpiritControl.SpiritObject == objSpirit)
                                        {
                                            panSpirits.Controls.RemoveAt(i);
                                            objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                            objSpiritControl.DeleteSpirit -= DeleteSpirit;
                                            objSpiritControl.Dispose();
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = panSprites.Controls.Count - 1; i >= 0; i--)
                                    {
                                        if (panSprites.Controls[i] is SpiritControl objSpiritControl && objSpiritControl.SpiritObject == objSpirit)
                                        {
                                            panSpirits.Controls.RemoveAt(i);
                                            objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                                            objSpiritControl.DeleteSpirit -= DeleteSpirit;
                                            objSpiritControl.Dispose();
                                        }
                                    }
                                }
                            }
                            int intSpirits = panSpirits.Controls.Count;
                            int intSprites = panSprites.Controls.Count;
                            foreach (Spirit objSpirit in notifyCollectionChangedEventArgs.NewItems)
                            {
                                bool blnIsSpirit = objSpirit.EntityType == SpiritType.Spirit;
                                SpiritControl objSpiritControl = new SpiritControl(objSpirit);

                                // Attach an EventHandler for the ServicesOwedChanged Event.
                                objSpiritControl.ContactDetailChanged += MakeDirtyWithCharacterUpdate;
                                objSpiritControl.DeleteSpirit += DeleteSpirit;

                                objSpiritControl.RebuildSpiritList(blnIsSpirit ? CharacterObject.MagicTradition : CharacterObject.TechnomancerStream);

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
        protected void AddSpirit(object sender, EventArgs e)
        {
            // The number of bound Spirits cannot exeed the character's CHA.
            if (!CharacterObject.IgnoreRules && CharacterObject.Spirits.Count(x => x.EntityType == SpiritType.Spirit) >= CharacterObject.CHA.Value)
            {
                MessageBox.Show(LanguageManager.GetString("Message_BoundSpiritLimit", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_BoundSpiritLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        protected void AddSprite(object sender, EventArgs e)
        {
            // The number of registered Sprites cannot exceed the character's LOG.
            if (!CharacterObject.IgnoreRules && CharacterObject.Spirits.Count(x => x.EntityType == SpiritType.Sprite) >= CharacterObject.LOG.Value)
            {
                MessageBox.Show(LanguageManager.GetString("Message_RegisteredSpriteLimit", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_RegisteredSpriteLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
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
        protected bool UpdateMugshot(PictureBox picMugshot, int intCurrentMugshotIndexInList)
        {
            if (intCurrentMugshotIndexInList < 0 || intCurrentMugshotIndexInList >= _objCharacter.Mugshots.Count || _objCharacter.Mugshots[intCurrentMugshotIndexInList] == null)
            {
                picMugshot.Image = null;
                return false;
            }

            Image imgMugshot = _objCharacter.Mugshots[intCurrentMugshotIndexInList];

            if (imgMugshot != null && picMugshot.Height >= imgMugshot.Height && picMugshot.Width >= imgMugshot.Width)
                picMugshot.SizeMode = PictureBoxSizeMode.CenterImage;
            else
                picMugshot.SizeMode = PictureBoxSizeMode.Zoom;
            picMugshot.Image = imgMugshot;

            return true;
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

        public bool IsDirty
        {
            get
            {
                return _blnIsDirty;
            }
            set
            {
                if (_blnIsDirty != value)
                {
                    _blnIsDirty = value;
                    UpdateWindowTitle(true);
                }
            }
        }

        public void MakeDirtyWithCharacterUpdate()
        {
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        public void MakeDirtyWithCharacterUpdate(object sender)
        {
            MakeDirtyWithCharacterUpdate();
        }

        public void MakeDirtyWithCharacterUpdate(object sender, EventArgs e)
        {
            MakeDirtyWithCharacterUpdate();
        }

        public bool IsCharacterUpdateRequested
        {
            get
            {
                return _blnRequestCharacterUpdate;
            }
            set
            {
                _blnRequestCharacterUpdate = value;
            }
        }

        public Character CharacterObject
        {
            get
            {
                return _objCharacter;
            }
        }

        public CharacterOptions CharacterObjectOptions
        {
            get
            {
                return _objOptions;
            }
        }

        public ObservableCollection<CharacterAttrib> PrimaryAttributes
        {
            get
            {
                return _lstPrimaryAttributes;
            }
        }

        public ObservableCollection<CharacterAttrib> SpecialAttributes
        {
            get
            {
                return _lstSpecialAttributes;
            }
        }

        private void ForceUpdateWindowTitle(object sender)
        {
            UpdateWindowTitle(false);
        }

        public virtual string FormMode
        {
            get
            {
                return string.Empty;
            }
        }

        protected void ShiftTabsOnMouseScroll(object sender, MouseEventArgs e)
        {
            //TODO: Global option to switch behaviour on/off, method to emulate clicking the scroll buttons instead of changing the selected index,
            //allow wrapping back to first/last tab item based on scroll direction
            TabControl tabControl = (sender as TabControl);
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

        /// <summary>
        /// Update the Window title to show the Character's name and unsaved changes status.
        /// </summary>
        public void UpdateWindowTitle(bool blnCanSkip)
        {
            if (Text.EndsWith('*') == _blnIsDirty && blnCanSkip)
                return;
            
            string strTitle = _objCharacter.CharacterName + " - " + FormMode + " (" + _objOptions.Name + ')';
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

            Cursor = Cursors.WaitCursor;
            if (_objCharacter.Save())
            {
                GlobalOptions.AddToMRUList(_objCharacter.FileName, "mru", true, true);
                IsDirty = false;
                Cursor = Cursors.Default;

                // If this character has just been saved as Created, close this form and re-open the character which will open it in the Career window instead.
                if (blnDoCreated)
                {
                    SaveCharacterAsCreated();
                }

                return true;
            }
            Cursor = Cursors.Default;
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
                Filter = "Chummer5 Files (*.chum5)|*.chum5|All Files (*.*)|*.*"
            };

            string strShowFileName = string.Empty;
            string[] strFile = _objCharacter.FileName.Split(Path.DirectorySeparatorChar);
            strShowFileName = strFile[strFile.Length - 1];

            if (string.IsNullOrEmpty(strShowFileName))
                strShowFileName = _objCharacter.CharacterName;

            saveFileDialog.FileName = strShowFileName;

            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                _objCharacter.FileName = saveFileDialog.FileName;
                return SaveCharacter(false);
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
            get
            {
                return _frmPrintView;
            }
            set
            {
                _frmPrintView = value;
            }
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
        /// Processes the string strDrain into a calculated Drain dicepool and appropriate display attributes and labels.
        /// </summary>
        /// <param name="strDrain"></param>
        /// <param name="objImprovementManager"></param>
        /// <param name="drain"></param>
        /// <param name="attributeText"></param>
        /// <param name="valueText"></param>
        /// <param name="tooltip"></param>
        public void CalculateTraditionDrain(string strDrain, Improvement.ImprovementType drain, Label attributeText = null, Label valueText = null, ToolTip tooltip = null)
        {
            if (string.IsNullOrWhiteSpace(strDrain) || (attributeText == null && valueText == null && tooltip == null))
                return;
            StringBuilder objDrain = valueText != null ? new StringBuilder(strDrain) : null;
            StringBuilder objDisplayDrain = attributeText != null ? new StringBuilder(strDrain) : null;
            StringBuilder objTip = tooltip != null ? new StringBuilder(strDrain) : null;
            int intDrain = 0;
            // Update the Fading CharacterAttribute Value.
            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                CharacterAttrib objAttrib = _objCharacter.GetAttribute(strAttribute);
                if (strDrain.Contains(objAttrib.Abbrev))
                {
                    string strAttribTotalValue = objAttrib.TotalValue.ToString();
                    objDrain?.Replace(objAttrib.Abbrev, strAttribTotalValue);
                    objDisplayDrain?.Replace(objAttrib.Abbrev, objAttrib.DisplayAbbrev);
                    objTip?.Replace(objAttrib.Abbrev, objAttrib.DisplayAbbrev + " (" + strAttribTotalValue + ')');
                }
            }
            if (objDrain != null)
            {
                try
                {
                    intDrain = Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(objDrain.ToString())));
                }
                catch (XPathException) { }
                catch (OverflowException) { } // Result is text and not a double
                catch (InvalidCastException) { } // Result is text and not a double
            }

            if (valueText != null || tooltip != null)
            {
                int intBonusDrain = ImprovementManager.ValueOf(_objCharacter, drain);
                if (intBonusDrain != 0)
                {
                    intDrain += intBonusDrain;
                    objTip?.Append(" + " + LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + " (" + intBonusDrain.ToString() + ')');
                }
            }

            if (attributeText != null)
                attributeText.Text = objDisplayDrain.ToString();
            if (valueText != null)
                valueText.Text = intDrain.ToString();
            if (tooltip != null)
                tooltip.SetToolTip(valueText, objTip.ToString());
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
