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
            string strName = input.UniqueName + ": ";
            if (input.Value > 0)
                strName += "+";
            strName += input.Value.ToString();
            if (!string.IsNullOrEmpty(input.Condition))
                strName += ", " + input.Condition;
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
            else if (input.OriginSource == QualitySource.Metatype || input.OriginSource == QualitySource.MetatypeRemovable || input.OriginSource == QualitySource.Improvement)
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

            TreeNode objSpellTypeNode = null;
            switch (input.Category)
            {
                case "Combat":
                    objSpellTypeNode = Nodes[0];
                    break;
                case "Detection":
                    objSpellTypeNode = Nodes[1];
                    break;
                case "Health":
                    objSpellTypeNode = Nodes[2];
                    break;
                case "Illusion":
                    objSpellTypeNode = Nodes[3];
                    break;
                case "Manipulation":
                    objSpellTypeNode = Nodes[4];
                    break;
                case "Rituals":
                    objSpellTypeNode = Nodes[5];
                    break;
            }
            objSpellTypeNode.Nodes.Add(objNode);
            objSpellTypeNode.Expand();
        }

        /// <summary>
        /// Sort the contents of a TreeView alphabetically within each group Node.
        /// </summary>
        /// <param name="treTree">TreeView to sort.</param>
        public void SortCustom()
        {
            SortByName objSort = new SortByName();
            for (int i = 0; i < Nodes.Count; ++i)
            {
                TreeNode objLoopNode = Nodes[i];
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
        public void ClearNodeBackground(TreeNode objHighlighted)
        {
            Nodes.ClearNodeBackground(objHighlighted);
        }
    }
}
