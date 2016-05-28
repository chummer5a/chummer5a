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
    class TreeView : System.Windows.Forms.TreeView
    {
        public void Add(LimitModifier input, ContextMenuStrip strip)
        {
            TreeNode newNode = new TreeNode();
            newNode.Text = newNode.Name = input.DisplayName;
            newNode.Tag = input.InternalId;
            if (input.Notes != string.Empty)
                newNode.ForeColor = Color.SaddleBrown;
            newNode.ToolTipText = CommonFunctions.WordWrap(input.Notes, 100);
            newNode.ContextMenuStrip = strip;

            TreeNode nodeToAddTo = this.Nodes[(int)Enum.Parse(typeof(LimitType),input.Limit)];
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
            if (input.Exclude != "")
                strName += " (" + input.Exclude + ")";
            newNode.Text = newNode.Name = strName;
            newNode.Tag = input.SourceName;
            if (input.Notes != string.Empty)
                newNode.ForeColor = Color.SaddleBrown;
            newNode.ToolTipText = CommonFunctions.WordWrap(input.Notes, 100);
            newNode.ContextMenuStrip = strip;
            if (input.ImprovedName == "")
            {
                if (input.ImproveType == Improvement.ImprovementType.SocialLimit)
                    input.ImprovedName = "Social";
                else if (input.ImproveType == Improvement.ImprovementType.MentalLimit)
                    input.ImprovedName = "Mental";
                else
                    input.ImprovedName = "Physical";
            }

            TreeNode nodeToAddTo = this.Nodes[(int)Enum.Parse(typeof(LimitType), input.ImprovedName)];
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
            newNode.Tag = input.Name;
            newNode.ContextMenuStrip = strip;
            if (input.Notes != string.Empty)
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
                this.Nodes[1].Nodes.Add(newNode);
                this.Nodes[1].Expand();
            }
            else
            {
                this.Nodes[0].Nodes.Add(newNode);
                this.Nodes[0].Expand();
            }
        }
        public void Add(Quality input, ContextMenuStrip strip)
        {
            TreeNode newNode = new TreeNode();
            newNode.Text = input.DisplayName;
            newNode.Tag = input.InternalId;
            newNode.ContextMenuStrip = strip;

            if (input.Notes != string.Empty)
                newNode.ForeColor = Color.SaddleBrown;
            else
            {
                if (input.OriginSource == QualitySource.Metatype || input.OriginSource == QualitySource.MetatypeRemovable)
                    newNode.ForeColor = SystemColors.GrayText;
                if (!input.Implemented)
                    newNode.ForeColor = Color.Red;
            }
            newNode.ToolTipText = CommonFunctions.WordWrap(input.Notes, 100);

            TreeNode nodeToAddTo = this.Nodes[(int)input.Type];
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
            if (input.Notes != string.Empty)
                objNode.ForeColor = Color.SaddleBrown;
            objNode.ToolTipText = CommonFunctions.WordWrap(input.Notes, 100);

            switch (input.Category)
            {
                case "Combat":
                    this.Nodes[0].Nodes.Add(objNode);
                    this.Nodes[0].Expand();
                    break;
                case "Detection":
                    this.Nodes[1].Nodes.Add(objNode);
                    this.Nodes[1].Expand();
                    break;
                case "Health":
                    this.Nodes[2].Nodes.Add(objNode);
                    this.Nodes[2].Expand();
                    break;
                case "Illusion":
                    this.Nodes[3].Nodes.Add(objNode);
                    this.Nodes[3].Expand();
                    break;
                case "Manipulation":
                    this.Nodes[4].Nodes.Add(objNode);
                    this.Nodes[4].Expand();
                    break;
                case "Rituals":
                    int intNode = 5;
                    if (_objCharacter.AdeptEnabled && !_objCharacter.MagicianEnabled)
                        intNode = 0;
                    this.Nodes[intNode].Nodes.Add(objNode);
                    this.Nodes[intNode].Expand();
                    break;
            }
        }

        /// <summary>
        /// Sort the contents of a TreeView alphabetically within each group Node.
        /// </summary>
        /// <param name="treTree">TreeView to sort.</param>
        public void SortCustom()
        {
            for (int i = 0; i <= this.Nodes.Count - 1; i++)
            {
                List<TreeNode> lstNodes = new List<TreeNode>();
                foreach (TreeNode objNode in this.Nodes[i].Nodes)
                    lstNodes.Add(objNode);
                this.Nodes[i].Nodes.Clear();
                try
                {
                    SortByName objSort = new SortByName();
                    lstNodes.Sort(objSort.Compare);
                }
                catch
                {
                }

                foreach (TreeNode objNode in lstNodes)
                    this.Nodes[i].Nodes.Add(objNode);

                this.Nodes[i].Expand();
            }
        }

        /// <summary>
        /// Clear the background colour for all TreeNodes except the one currently being hovered over during a drag-and-drop operation.
        /// </summary>
        /// <param name="treTree">TreeView to check.</param>
        /// <param name="objHighlighted">TreeNode that is currently being hovered over.</param>
        public void ClearNodeBackground(TreeNode objHighlighted)
        {
            foreach (TreeNode objNode in this.Nodes)
            {
                if (objHighlighted != null)
                {
                    if (objNode != (TreeNode)objHighlighted)
                        objNode.BackColor = SystemColors.Window;
                    ClearNodeBackground(objNode, (TreeNode)objHighlighted);
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
