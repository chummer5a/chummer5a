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
﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer.helpers
{
    public class TreeView : System.Windows.Forms.TreeView
    {
        public void Add(LimitModifier input, ContextMenuStrip strip)
        {
            TreeNode newNode = new TreeNode();
            newNode.Text = newNode.Name = input.DisplayName;
            newNode.Tag = input.InternalId;
            if (!string.IsNullOrEmpty(input.Notes))
                newNode.ForeColor = Color.SaddleBrown;
            newNode.ToolTipText = CommonFunctions.WordWrap(input.Notes, 100);
            newNode.ContextMenuStrip = strip;

            TreeNode nodeToAddTo = Nodes[(int)Enum.Parse(typeof(LimitType),input.Limit)];
            if (!nodeToAddTo.Nodes.ContainsKey(newNode.Text))
            {
                nodeToAddTo.Nodes.Add(newNode);
                nodeToAddTo.Expand();
            }
        }
        public void Add(Improvement input, ContextMenuStrip strip)
        {
            TreeNode newNode = new TreeNode();
            string strName = input.UniqueName;
            if (input.Value > 0)
                strName += " [+" + input.Value.ToString() + "]";
            else
                strName += " [" + input.Value.ToString() + "]";
            if (!string.IsNullOrEmpty(input.Exclude))
                strName += " (" + input.Exclude + ")";
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

            TreeNode nodeToAddTo = Nodes[(int)Enum.Parse(typeof(LimitType), input.ImprovedName)];
            if (!nodeToAddTo.Nodes.ContainsKey(newNode.Text))
            {
                nodeToAddTo.Nodes.Add(newNode);
                nodeToAddTo.Expand();
            }
        }
        public void Add(MartialArt input, ContextMenuStrip strip)
        {
            TreeNode newNode = new TreeNode();
            newNode.Text = input.DisplayName;
            newNode.Tag = input.InternalId;
            newNode.ContextMenuStrip = strip;
            if (!string.IsNullOrEmpty(input.Notes))
                newNode.ForeColor = Color.SaddleBrown;
            newNode.ToolTipText = CommonFunctions.WordWrap(input.Notes, 100);

            foreach (MartialArtAdvantage objAdvantage in input.Advantages)
            {
                TreeNode objAdvantageNode = new TreeNode();
                objAdvantageNode.Text = objAdvantage.DisplayName;
                objAdvantageNode.Tag = objAdvantage.InternalId;
                newNode.Nodes.Add(objAdvantageNode);
                newNode.Expand();
            }

            if (input.IsQuality)
            {
                Nodes[1].Nodes.Add(newNode);
                Nodes[1].Expand();
            }
            else
            {
                Nodes[0].Nodes.Add(newNode);
                Nodes[0].Expand();
            }
        }
        public void Add(Quality input, ContextMenuStrip strip)
        {
            TreeNode newNode = new TreeNode();
            newNode.Text = input.DisplayName;
            newNode.Tag = input.InternalId;
            newNode.ContextMenuStrip = strip;

            if (!string.IsNullOrEmpty(input.Notes))
                newNode.ForeColor = Color.SaddleBrown;
            else if (input.OriginSource == QualitySource.Metatype || input.OriginSource == QualitySource.MetatypeRemovable)
                newNode.ForeColor = SystemColors.GrayText;
            if (!input.Implemented)
                newNode.ForeColor = Color.Red;
            newNode.ToolTipText = CommonFunctions.WordWrap(input.Notes, 100);

            TreeNode nodeToAddTo = Nodes[(int)input.Type];
            if (!nodeToAddTo.Nodes.ContainsKey(newNode.Text))
            {
                nodeToAddTo.Nodes.Add(newNode);
                nodeToAddTo.Expand();
            }
        }
        public void Add(Spell input, ContextMenuStrip strip, Character _objCharacter)
        {
            TreeNode objNode = new TreeNode();
            objNode.Text = input.DisplayName;
            objNode.Tag = input.InternalId;
            objNode.ContextMenuStrip = strip;
            if (!string.IsNullOrEmpty(input.Notes))
                objNode.ForeColor = Color.SaddleBrown;
            objNode.ToolTipText = CommonFunctions.WordWrap(input.Notes, 100);

            switch (input.Category)
            {
                case "Combat":
                    Nodes[0].Nodes.Add(objNode);
                    Nodes[0].Expand();
                    break;
                case "Detection":
                    Nodes[1].Nodes.Add(objNode);
                    Nodes[1].Expand();
                    break;
                case "Health":
                    Nodes[2].Nodes.Add(objNode);
                    Nodes[2].Expand();
                    break;
                case "Illusion":
                    Nodes[3].Nodes.Add(objNode);
                    Nodes[3].Expand();
                    break;
                case "Manipulation":
                    Nodes[4].Nodes.Add(objNode);
                    Nodes[4].Expand();
                    break;
                case "Rituals":
                    int intNode = 5;
                    if (_objCharacter.AdeptEnabled && !_objCharacter.MagicianEnabled)
                        intNode = 0;
                    Nodes[intNode].Nodes.Add(objNode);
                    Nodes[intNode].Expand();
                    break;
            }
        }

        /// <summary>
        /// Sort the contents of a TreeView alphabetically within each group Node.
        /// </summary>
        /// <param name="treTree">TreeView to sort.</param>
        public void SortCustom()
        {
            for (int i = 0; i <= Nodes.Count - 1; i++)
            {
                List<TreeNode> lstNodes = new List<TreeNode>();
                foreach (TreeNode objNode in Nodes[i].Nodes)
                    lstNodes.Add(objNode);
                Nodes[i].Nodes.Clear();
                SortByName objSort = new SortByName();
                lstNodes.Sort(objSort.Compare);

                foreach (TreeNode objNode in lstNodes)
                    Nodes[i].Nodes.Add(objNode);

                Nodes[i].Expand();
            }
        }

        /// <summary>
        /// Clear the background colour for all TreeNodes except the one currently being hovered over during a drag-and-drop operation.
        /// </summary>
        /// <param name="treTree">TreeView to check.</param>
        /// <param name="objHighlighted">TreeNode that is currently being hovered over.</param>
        public void ClearNodeBackground(TreeNode objHighlighted)
        {
            foreach (TreeNode objNode in Nodes)
            {
                if (objHighlighted != null)
                {
                    if (objNode != objHighlighted)
                        objNode.BackColor = SystemColors.Window;
                    ClearNodeBackground(objNode, objHighlighted);
                }
            }
        }

        /// <summary>
        /// Recursive method to clear the background colour for all TreeNodes except the one currently being hovered over during a drag-and-drop operation.
        /// </summary>
        /// <param name="objNode">Parent TreeNode to check.</param>
        /// <param name="objHighlighted">TreeNode that is currently being hovered over.</param>
        private void ClearNodeBackground(TreeNode objNode, TreeNode objHighlighted)
        {
            foreach (TreeNode objChild in objNode.Nodes)
            {
                if (objChild != objHighlighted)
                    objChild.BackColor = SystemColors.Window;
                if (objChild.Nodes.Count > 0)
                    ClearNodeBackground(objChild, objHighlighted);
            }
        }
    }
}
