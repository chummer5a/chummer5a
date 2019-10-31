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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;

namespace Chummer
{
    public static class WinFormsExtensions
    {
        private static Logger Log = NLog.LogManager.GetCurrentClassLogger();
        #region Controls Extensions
        /// <summary>
        /// Runs code on a WinForms control in a thread-safe manner.
        /// </summary>
        /// <param name="objControl">Parent control from which Invoke would need to be called.</param>
        /// <param name="funcToRun">Code to run in the form of a delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DoThreadSafe(this Control objControl, Action funcToRun)
        {
            try
            {
                Control myControlCopy = objControl; //to have the Object for sure, regardless of other threads
                if (myControlCopy?.InvokeRequired == true)
                    myControlCopy.Invoke(funcToRun);
                else
                    funcToRun.Invoke();
            }
            catch (ObjectDisposedException e)
            {
                //we really don't need to care about that.
                //Log.Trace(e);
            }
            catch (InvalidAsynchronousStateException e)
            {
                //we really don't need to care about that.
                Log.Trace(e);
            }
            catch(Exception e)
            {
                Log.Error(e);
#if DEBUG
                Program.MainForm.ShowMessageBox(e.ToString());
#endif
            }
        }

        /// <summary>
        /// Bind a control's property to a property via OnPropertyChanged
        /// </summary>
        /// <param name="objControl">Control to bind</param>
        /// <param name="strPropertyName">Control's property to which <paramref name="strDataMember"/> is being bound</param>
        /// <param name="objDataSource">Instance owner of <paramref name="strDataMember"/></param>
        /// <param name="strDataMember">Name of the property of <paramref name="objDataSource"/> that is being bound to <paramref name="objControl"/>'s <paramref name="strPropertyName"/> property</param>
        public static void DoDatabinding(this Control objControl, string strPropertyName, object objDataSource, string strDataMember)
        {
            if (!objControl.IsHandleCreated)
            {
                objControl.CreateControl();
            }
            objControl.DataBindings.Add(strPropertyName, objDataSource, strDataMember, false, DataSourceUpdateMode.OnPropertyChanged);
        }

        /// <summary>
        /// Bind a control's property to the OPPOSITE of property via OnPropertyChanged. Expected to be used exclusively by boolean bindings, other attributes have not been tested. 
        /// </summary>
        /// <param name="objControl">Control to bind</param>
        /// <param name="strPropertyName">Control's property to which <paramref name="strDataMember"/> is being bound</param>
        /// <param name="objDataSource">Instance owner of <paramref name="strDataMember"/></param>
        /// <param name="strDataMember">Name of the property of <paramref name="objDataSource"/> that is being bound to <paramref name="objControl"/>'s <paramref name="strPropertyName"/> property</param>
        public static void DoNegatableDatabinding(this Control objControl, string strPropertyName, object objDataSource, string strDataMember)
        {
            if (!objControl.IsHandleCreated)
            {
                objControl.CreateControl();
            }
            objControl.DataBindings.Add(new NegatableBinding(strPropertyName, objDataSource, strDataMember, true));
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

        /// <summary>
        /// Find a TreeNode in a TreeNode based on its Tag.
        /// </summary>
        /// <param name="strGuid">InternalId of the Node to find.</param>
        /// <param name="objNode">TreeNode to search.</param>
        /// <param name="blnDeep">Whether to look at grandchildren and greater descendents of this node.</param>
        public static TreeNode FindNode(this TreeNode objNode, string strGuid, bool blnDeep = true)
        {
            if (objNode == null || string.IsNullOrEmpty(strGuid) || strGuid.IsEmptyGuid()) return null;
            foreach (TreeNode objChild in objNode.Nodes)
            {
                if (objChild.Tag is IHasInternalId idNode && idNode.InternalId == strGuid || objChild.Tag is string s && s == strGuid)
                    return objChild;

                if (!blnDeep) continue;
                var objFound = objChild.FindNode(strGuid);
                if (objFound != null)
                    return objFound;
            }
            return null;
        }

        /// <summary>
        /// Find a TreeNode in a TreeNode based on its Tag.
        /// </summary>
        /// <param name="objNode">TreeNode to search.</param>
        /// <param name="objTag">Tag to look for.</param>
        /// <param name="blnDeep">Whether to look at grandchildren and greater descendents of this node.</param>
        public static TreeNode FindNodeByTag(this TreeNode objNode, object objTag, bool blnDeep = true)
        {
            if (objNode != null && objTag != null)
            {
                foreach (TreeNode objChild in objNode.Nodes)
                {
                    if (objChild.Tag == objTag)
                        return objChild;

                    if (blnDeep)
                    {
                        TreeNode objFound = objChild.FindNodeByTag(objTag);
                        if (objFound != null)
                            return objFound;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the rightmost edge of the node or any of its descendents.
        /// </summary>
        /// <returns></returns>
        public static int GetRightMostEdge(this TreeNode objNode)
        {
            if (objNode.Nodes.Count == 0)
            {
                return objNode.Bounds.Right;
            }
            int intReturn = 0;
            foreach (TreeNode objChild in objNode.Nodes)
            {
                int intLoopEdge = objChild.GetRightMostEdge();
                if (intLoopEdge > intReturn)
                    intReturn = intLoopEdge;
            }
            return intReturn;
        }
        #endregion

        #region TreeView Extensions

        /// <summary>
        /// Sort the contents of a TreeView alphabetically within each group Node.
        /// </summary>
        /// <param name="treView">TreeView to sort.</param>
        /// <param name="strSelectedNodeTag">String of the tag to select after sorting.</param>
        public static void SortCustomAlphabetically(this TreeView treView, string strSelectedNodeTag = "")
        {
            TreeNodeCollection lstTreeViewNodes = treView?.Nodes;
            if (lstTreeViewNodes == null)
                return;
            if (string.IsNullOrEmpty(strSelectedNodeTag))
                strSelectedNodeTag = (treView.SelectedNode?.Tag as IHasInternalId)?.InternalId;
            for (int i = 0; i < lstTreeViewNodes.Count; ++i)
            {
                TreeNode objLoopNode = lstTreeViewNodes[i];
                TreeNodeCollection objLoopNodeChildren = objLoopNode.Nodes;
                int intChildrenCount = objLoopNodeChildren.Count;
                if (intChildrenCount > 0)
                {
                    TreeNode[] lstNodes = new TreeNode[intChildrenCount];
                    objLoopNodeChildren.CopyTo(lstNodes, 0);
                    objLoopNodeChildren.Clear();
                    Array.Sort(lstNodes, CompareTreeNodes.CompareText);
                    objLoopNodeChildren.AddRange(lstNodes);

                    objLoopNode.Expand();
                }
            }

            TreeNode objSelectedNode = treView.FindNode(strSelectedNodeTag);
            if (objSelectedNode != null)
                treView.SelectedNode = objSelectedNode;
        }

        /// <summary>
        /// Sort the contents of a TreeView alphabetically within each group Node.
        /// </summary>
        /// <param name="treView">TreeView to sort.</param>
        /// <param name="objSelectedNodeTag">String of the tag to select after sorting.</param>
        public static void SortCustomAlphabetically(this TreeView treView, object objSelectedNodeTag = null)
        {
            TreeNodeCollection lstTreeViewNodes = treView?.Nodes;
            if (lstTreeViewNodes == null)
                return;
            if (objSelectedNodeTag == null)
                objSelectedNodeTag = treView.SelectedNode?.Tag;
            for (int i = 0; i < lstTreeViewNodes.Count; ++i)
            {
                TreeNode objLoopNode = lstTreeViewNodes[i];
                TreeNodeCollection objLoopNodeChildren = objLoopNode.Nodes;
                int intChildrenCount = objLoopNodeChildren.Count;
                if (intChildrenCount > 0)
                {
                    TreeNode[] lstNodes = new TreeNode[intChildrenCount];
                    objLoopNodeChildren.CopyTo(lstNodes, 0);
                    objLoopNodeChildren.Clear();
                    Array.Sort(lstNodes, CompareTreeNodes.CompareText);
                    objLoopNodeChildren.AddRange(lstNodes);

                    objLoopNode.Expand();
                }
            }

            TreeNode objSelectedNode = treView.FindNodeByTag(objSelectedNodeTag);
            if (objSelectedNode != null)
                treView.SelectedNode = objSelectedNode;
        }

        /// <summary>
        /// Sort the contents of a TreeView based on the sorting property of any
        /// ICanSorts in the tree
        /// </summary>
        /// <param name="treView">The tree to sort</param>
        public static void SortCustomOrder(this TreeView treView)
        {
            string strSelectedNodeTag = (treView.SelectedNode?.Tag as IHasInternalId)?.InternalId;

            var currentSorter = treView.TreeViewNodeSorter;
            treView.TreeViewNodeSorter = new CustomNodeSorter();
            treView.Sort();
            treView.TreeViewNodeSorter = currentSorter;

            // Reselect whatever was selected before
            TreeNode objSelectedNode = treView.FindNode(strSelectedNodeTag);
            if (objSelectedNode != null)
                treView.SelectedNode = objSelectedNode;
        }

        /// <summary>
        /// Custom comparer used by SortCustomOrder
        /// </summary>
        private class CustomNodeSorter : System.Collections.IComparer
        {
            public CustomNodeSorter() { }

            public int Compare(object x, object y)
            {
                ICanSort lhs = (x as TreeNode)?.Tag as ICanSort;
                ICanSort rhs = (y as TreeNode)?.Tag as ICanSort;

                // Sort any non-sortables first
                if (lhs == null)
                    return -1;
                if (rhs == null)
                    return 1;

                return lhs.SortOrder.CompareTo(rhs.SortOrder);
            }
        }

        /// <summary>
        /// Iterates through a TreeView and stores the sorting order on any
        /// ICanSort objects, allowing them to retain the order after a load
        /// </summary>
        /// <param name="treView"></param>
        public static void CacheSortOrder(this TreeView treView)
        {
            CacheSortOrderRecursive(treView?.Nodes);
        }

        /// <summary>
        /// Does a breadth-first recursion to set the sorting property of any ICanSorts in the tree
        /// </summary>
        /// <param name="lstNodes">The list if TreeNodes to iterate over</param>
        private static void CacheSortOrderRecursive(TreeNodeCollection lstNodes)
        {
            List<TreeNode> lstEnumerable = lstNodes.Cast<TreeNode>().ToList();
            // Do this as two steps because non-sortables can own sortables
            lstEnumerable.Where(n => n?.Tag is ICanSort).ToList().ForEach(n =>
                {
                    if (n.Tag is ICanSort objSortable)
                        objSortable.SortOrder = n.Index;
                });
            lstEnumerable.ForEach(n => CacheSortOrderRecursive(n.Nodes));
        }

        /// <summary>
        /// Clear the background colour for all TreeNodes except the one currently being hovered over during a drag-and-drop operation.
        /// </summary>
        /// <param name="treView">Base TreeView whose nodes should get their background color cleared.</param>
        /// <param name="objHighlighted">TreeNode that is currently being hovered over.</param>
        public static void ClearNodeBackground(this TreeView treView, TreeNode objHighlighted)
        {
            treView?.Nodes.ClearNodeBackground(objHighlighted);
        }

        /// <summary>
        /// Find a TreeNode in a TreeView based on its Tag.
        /// </summary>
        /// <param name="strGuid">InternalId of the Node to find.</param>
        /// <param name="treTree">TreeView to search.</param>
        /// <param name="blnDeep">Whether to look at grandchildren and greater descendents of this node.</param>
        public static TreeNode FindNode(this TreeView treTree, string strGuid, bool blnDeep = true)
        {
            if (treTree == null || string.IsNullOrEmpty(strGuid) || strGuid.IsEmptyGuid()) return null;
            foreach (TreeNode objNode in treTree.Nodes)
            {
                if (objNode?.Tag != null &&  objNode.Tag is IHasInternalId node && node.InternalId == strGuid || objNode?.Tag?.ToString() == strGuid)
                    return objNode;

                if (!blnDeep) continue;
                var objFound = objNode.FindNode(strGuid);
                if (objFound != null)
                    return objFound;
            }
            return null;
        }

        /// <summary>
        /// Find a TreeNode in a TreeView based on its Tag.
        /// </summary>
        /// <param name="treTree">TreeView to search.</param>
        /// <param name="objTag">Tag to look for.</param>
        /// <param name="blnDeep">Whether to look at grandchildren and greater descendents of this node.</param>
        public static TreeNode FindNodeByTag(this TreeView treTree, object objTag, bool blnDeep = true)
        {
            if (treTree != null && objTag != null)
            {
                foreach (TreeNode objNode in treTree.Nodes)
                {
                    if (objNode.Tag == objTag)
                        return objNode;

                    if (blnDeep)
                    {
                        TreeNode objFound = objNode.FindNodeByTag(objTag);
                        if (objFound != null)
                            return objFound;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the rightmost edge of the tree or any of its descendents.
        /// </summary>
        /// <param name="treTree"></param>
        /// <returns></returns>
        public static int GetRightMostEdge(this TreeView treTree)
        {
            if (treTree.Nodes.Count == 0)
            {
                return treTree.Bounds.Right;
            }
            int intReturn = 0;
            foreach (TreeNode objChild in treTree.Nodes)
            {
                int intLoopEdge = objChild.GetRightMostEdge();
                if (intLoopEdge > intReturn)
                    intReturn = intLoopEdge;
            }
            return intReturn;
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
