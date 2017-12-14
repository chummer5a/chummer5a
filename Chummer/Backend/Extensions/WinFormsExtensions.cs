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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Chummer
{
    static class WinFormsExtensions
    {
        #region Controls Extensions
        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DoThreadSafe(this Control objControl, Action funcToRun)
        {
            if (objControl?.InvokeRequired == true)
                objControl.Invoke(funcToRun);
            else
                funcToRun.Invoke();
        }
        #endregion

        #region ComboBox Extensions
        public static bool IsInitalized(this ComboBox cboThis, bool isLoading)
        {
            return (isLoading || string.IsNullOrEmpty(cboThis?.SelectedValue?.ToString()));
        }
        #endregion

        #region TreeNode Extensions
        public static TreeNode GetTopParent(this TreeNode objThis)
        {
            TreeNode objReturn = objThis;
            while (objReturn.Parent != null)
                objReturn = objReturn.Parent;
            return objReturn;
        }
        #endregion

        #region TreeView Extensions
        public static void Add(this TreeView treView, LimitModifier input, ContextMenuStrip strip)
        {
            if (treView == null)
                return;
            TreeNode nodeToAddTo = treView.Nodes[(int)Enum.Parse(typeof(LimitType), input.Limit)];
            if (!nodeToAddTo.Nodes.ContainsKey(input.DisplayName))
            {
                TreeNode newNode = new TreeNode();
                newNode.Text = newNode.Name = input.DisplayName;
                newNode.Tag = input.InternalId;
                if (!string.IsNullOrEmpty(input.Notes))
                    newNode.ForeColor = Color.SaddleBrown;
                newNode.ToolTipText = CommonFunctions.WordWrap(input.Notes, 100);
                newNode.ContextMenuStrip = strip;

                nodeToAddTo.Nodes.Add(newNode);
                nodeToAddTo.Expand();
            }
        }
        public static void Add(this TreeView treView, Improvement input, ContextMenuStrip strip)
        {
            if (treView == null)
                return;
            TreeNode nodeToAddTo = treView.Nodes[(int)Enum.Parse(typeof(LimitType), input.ImprovedName)];
            string strName = input.UniqueName + ": ";
            if (input.Value > 0)
                strName += "+";
            strName += input.Value.ToString();
            if (!string.IsNullOrEmpty(input.Condition))
                strName += ", " + input.Condition;
            if (!nodeToAddTo.Nodes.ContainsKey(strName))
            {
                TreeNode newNode = new TreeNode();
                newNode.Text = newNode.Name = strName;
                newNode.Tag = input.SourceName;
                if (!string.IsNullOrEmpty(input.Notes))
                    newNode.ForeColor = Color.SaddleBrown;
                newNode.ToolTipText = CommonFunctions.WordWrap(input.Notes, 100);
                newNode.ContextMenuStrip = strip;
                if (string.IsNullOrEmpty(input.ImprovedName))
                {
                    if (input.ImproveType == Improvement.ImprovementType.SocialLimit)
                        input.ImprovedName = "Social";
                    else if (input.ImproveType == Improvement.ImprovementType.MentalLimit)
                        input.ImprovedName = "Mental";
                    else
                        input.ImprovedName = "Physical";
                }

                nodeToAddTo.Nodes.Add(newNode);
                nodeToAddTo.Expand();
            }
        }
        public static void Add(this TreeView treView, MartialArt input, ContextMenuStrip strip)
        {
            if (treView == null)
                return;
            TreeNode objTargetNode = treView.Nodes[input.IsQuality ? 1 : 0];
            if (objTargetNode != null)
            {
                TreeNode newNode = new TreeNode
                {
                    Text = input.DisplayName,
                    Tag = input.InternalId,
                    ContextMenuStrip = strip
                };
                if (!string.IsNullOrEmpty(input.Notes))
                    newNode.ForeColor = Color.SaddleBrown;
                newNode.ToolTipText = CommonFunctions.WordWrap(input.Notes, 100);

                foreach (MartialArtAdvantage objAdvantage in input.Advantages)
                {
                    TreeNode objAdvantageNode = new TreeNode
                    {
                        Text = objAdvantage.DisplayName,
                        Tag = objAdvantage.InternalId
                    };
                    newNode.Nodes.Add(objAdvantageNode);
                    newNode.Expand();
                }

                objTargetNode.Nodes.Add(newNode);
                objTargetNode.Expand();
            }
        }
        public static void Add(this TreeView treView, Quality input, ContextMenuStrip strip)
        {
            if (treView == null)
                return;
            TreeNode nodeToAddTo = treView.Nodes[(int)input.Type];
            string strName = input.DisplayName;
            if (!nodeToAddTo.Nodes.ContainsKey(strName))
            {
                TreeNode newNode = new TreeNode
                {
                    Text = strName,
                    Tag = input.InternalId,
                    ContextMenuStrip = strip
                };

                if (!string.IsNullOrEmpty(input.Notes))
                    newNode.ForeColor = Color.SaddleBrown;
                else if (input.OriginSource == QualitySource.Metatype || input.OriginSource == QualitySource.MetatypeRemovable || input.OriginSource == QualitySource.Improvement)
                    newNode.ForeColor = SystemColors.GrayText;
                if (!input.Implemented)
                    newNode.ForeColor = Color.Red;
                newNode.ToolTipText = CommonFunctions.WordWrap(input.Notes, 100);

                nodeToAddTo.Nodes.Add(newNode);
                nodeToAddTo.Expand();
            }
        }
        public static void Add(this TreeView treView, Spell input, ContextMenuStrip strip)
        {
            if (treView == null)
                return;
            TreeNode objNode = new TreeNode
            {
                Text = input.DisplayName,
                Tag = input.InternalId,
                ContextMenuStrip = strip
            };
            if (!string.IsNullOrEmpty(input.Notes))
                objNode.ForeColor = Color.SaddleBrown;
            objNode.ToolTipText = CommonFunctions.WordWrap(input.Notes, 100);

            TreeNode objSpellTypeNode = null;
            switch (input.Category)
            {
                case "Combat":
                    objSpellTypeNode = treView.Nodes[0];
                    break;
                case "Detection":
                    objSpellTypeNode = treView.Nodes[1];
                    break;
                case "Health":
                    objSpellTypeNode = treView.Nodes[2];
                    break;
                case "Illusion":
                    objSpellTypeNode = treView.Nodes[3];
                    break;
                case "Manipulation":
                    objSpellTypeNode = treView.Nodes[4];
                    break;
                case "Rituals":
                    objSpellTypeNode = treView.Nodes[5];
                    break;
                case "Enchantments":
                    objSpellTypeNode = treView.Nodes[6];
                    break;
            }
            objSpellTypeNode.Nodes.Add(objNode);
            objSpellTypeNode.Expand();
        }

        /// <summary>
        /// Sort the contents of a TreeView alphabetically within each group Node.
        /// </summary>
        /// <param name="treView">TreeView to sort.</param>
        public static void SortCustom(this TreeView treView)
        {
            TreeNodeCollection lstTreeViewNodes = treView?.Nodes;
            if (lstTreeViewNodes == null)
                return;
            SortByName objSort = new SortByName();
            for (int i = 0; i < lstTreeViewNodes.Count; ++i)
            {
                TreeNode objLoopNode = lstTreeViewNodes[i];
                TreeNodeCollection objLoopNodeChildren = objLoopNode.Nodes;
                TreeNode[] lstNodes = new TreeNode[objLoopNodeChildren.Count];
                objLoopNodeChildren.CopyTo(lstNodes, 0);
                objLoopNodeChildren.Clear();
                Array.Sort(lstNodes, objSort.Compare);
                objLoopNodeChildren.AddRange(lstNodes);

                objLoopNode.Expand();
            }
        }

        /// <summary>
        /// Clear the background colour for all TreeNodes except the one currently being hovered over during a drag-and-drop operation.
        /// </summary>
        /// <param name="treTree">TreeView to check.</param>
        /// <param name="objHighlighted">TreeNode that is currently being hovered over.</param>
        public static void ClearNodeBackground(this TreeView treView, TreeNode objHighlighted)
        {
            treView?.Nodes.ClearNodeBackground(objHighlighted);
        }
        #endregion

        #region TreeNodeCollection Extensions
        /// <summary>
        /// Recursive method to clear the background colour for all TreeNodes except the one currently being hovered over during a drag-and-drop operation.
        /// </summary>
        /// <param name="objNodes">Parent TreeNodeCollection to check.</param>
        /// <param name="objHighlighted">TreeNode that is currently being hovered over.</param>
        public static void ClearNodeBackground(this TreeNodeCollection objNodes, TreeNode objHighlighted)
        {
            foreach (TreeNode objChild in objNodes)
            {
                if (objChild != objHighlighted)
                    objChild.BackColor = SystemColors.Window;
                objChild.Nodes.ClearNodeBackground(objHighlighted);
            }
        }
        #endregion
    }
}
