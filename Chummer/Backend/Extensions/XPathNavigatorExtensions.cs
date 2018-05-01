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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml.XPath;

namespace Chummer
{
    public static class XPathNavigatorExtensions
    {
        public delegate bool TryParseFunction<T>(string input, out T result);

        /// <summary>
        /// This method is syntaxtic sugar for atempting to read a data field
        /// from an XmlNode. This version sets the output variable to its
        /// default value in case of a failed read and can be used for
        /// initializing variables. It can work on any type, but it requires
        /// a tryParse style function that is fed the nodes InnerText
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="field"></param>
        /// <param name="parser"></param>
        /// <param name="read"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static bool TryGetField<T>(this XPathNavigator node, string field, TryParseFunction<T> parser, out T read, T onError = default(T))
        {
            if (parser != null)
            {
                XPathNavigator objField = node?.SelectSingleNode(field);
                if (objField != null)
                {
                    return parser(objField.Value, out read);
                }
            }

            read = onError;
            return false;
        }

        /// <summary>
        /// Processes a single operation node with children that are either nodes to check whether the parent has a node that fulfills a condition, or they are nodes that are parents to further operation nodes
        /// </summary>
        /// <param name="blnIsOrNode">Whether this is an OR node (true) or an AND node (false). Default is AND (false).</param>
        /// <param name="xmlOperationNode">The node containing the filter operation or a list of filter operations. Every element here is checked against corresponding elements in the parent node, using an operation specified in the element's attributes.</param>
        /// <param name="xmlParentNode">The parent node against which the filter operations are checked.</param>
        /// <returns>True if the parent node passes the conditions set in the operation node/nodelist, false otherwise.</returns>
        public static bool ProcessFilterOperationNode(this XPathNavigator xmlParentNode, XPathNavigator xmlOperationNode, bool blnIsOrNode)
        {
            if (xmlOperationNode == null)
                return false;

            foreach (XPathNavigator xmlOperationChildNode in xmlOperationNode.SelectChildren(XPathNodeType.Element))
            {
                bool blnInvert = xmlOperationChildNode.SelectSingleNode("@NOT") != null;

                bool blnOperationChildNodeResult = blnInvert;
                string strNodeName = xmlOperationChildNode.Name;
                if (strNodeName == "OR")
                {
                    blnOperationChildNodeResult =
                        ProcessFilterOperationNode(xmlParentNode, xmlOperationChildNode, true) != blnInvert;
                }
                else if (strNodeName == "AND")
                {
                    blnOperationChildNodeResult =
                        ProcessFilterOperationNode(xmlParentNode, xmlOperationChildNode, false) != blnInvert;
                }
                else if (strNodeName == "NONE")
                {
                    blnOperationChildNodeResult = (xmlParentNode == null) != blnInvert;
                }
                else if (xmlParentNode != null)
                {
                    string strOperationType = xmlOperationChildNode.SelectSingleNode("@operation")?.Value ?? "==";
                    XPathNodeIterator objXmlTargetNodeList = xmlParentNode.Select(strNodeName);
                    // If we're just checking for existance of a node, no need for more processing
                    if (strOperationType == "exists")
                    {
                        blnOperationChildNodeResult = (objXmlTargetNodeList.Count > 0) != blnInvert;
                    }
                    else
                    {
                        bool blnOperationChildNodeAttributeOr = xmlOperationChildNode.SelectSingleNode("@OR") != null;
                        // default is "any", replace with switch() if more check modes are necessary
                        bool blnCheckAll = xmlOperationChildNode.SelectSingleNode("@checktype")?.Value == "all";
                        blnOperationChildNodeResult = blnCheckAll;
                        string strOperationChildNodeText = xmlOperationChildNode.Value;
                        bool blnOperationChildNodeEmpty = string.IsNullOrWhiteSpace(strOperationChildNodeText);

                        foreach (XPathNavigator xmlTargetNode in objXmlTargetNodeList)
                        {
                            bool boolSubNodeResult = blnInvert;
                            if (xmlTargetNode.SelectChildren(XPathNodeType.Element).Count > 0)
                            {
                                if (xmlOperationChildNode.SelectChildren(XPathNodeType.Element).Count > 0)
                                    boolSubNodeResult = ProcessFilterOperationNode(xmlTargetNode,
                                                            xmlOperationChildNode,
                                                            blnOperationChildNodeAttributeOr) !=
                                                        blnInvert;
                            }
                            else
                            {
                                string strTargetNodeText = xmlTargetNode.Value;
                                bool blnTargetNodeEmpty = string.IsNullOrWhiteSpace(strTargetNodeText);
                                if (blnTargetNodeEmpty || blnOperationChildNodeEmpty)
                                {
                                    if (blnTargetNodeEmpty == blnOperationChildNodeEmpty &&
                                        (strOperationType == "==" || strOperationType == "equals"))
                                    {
                                        boolSubNodeResult = !blnInvert;
                                    }
                                    else
                                    {
                                        boolSubNodeResult = blnInvert;
                                    }
                                }
                                // Note when adding more operation cases: XML does not like the "<" symbol as part of an attribute value
                                else
                                    switch (strOperationType)
                                    {
                                        case "doesnotequal":
                                        case "notequals":
                                        case "!=":
                                            blnInvert = !blnInvert;
                                            goto case "==";
                                        case "lessthan":
                                            blnInvert = !blnInvert;
                                            goto case ">=";
                                        case "lessthanequals":
                                            blnInvert = !blnInvert;
                                            goto case ">";

                                        case "like":
                                        case "contains":
                                            {
                                                boolSubNodeResult =
                                                    strTargetNodeText.Contains(strOperationChildNodeText) !=
                                                    blnInvert;
                                                break;
                                            }
                                        case "greaterthan":
                                        case ">":
                                            {
                                                boolSubNodeResult =
                                                    (int.TryParse(strTargetNodeText, out int intTargetNodeValue) &&
                                                     int.TryParse(strOperationChildNodeText,
                                                         out int intChildNodeValue) &&
                                                     intTargetNodeValue > intChildNodeValue) != blnInvert;
                                                break;
                                            }
                                        case "greaterthanequals":
                                        case ">=":
                                            {
                                                boolSubNodeResult =
                                                    (int.TryParse(strTargetNodeText, out int intTargetNodeValue) &&
                                                     int.TryParse(strOperationChildNodeText,
                                                         out int intChildNodeValue) &&
                                                     intTargetNodeValue >= intChildNodeValue) != blnInvert;
                                                break;
                                            }
                                        case "==":
                                        default:
                                            boolSubNodeResult =
                                                (strTargetNodeText.Trim() ==
                                                 strOperationChildNodeText.Trim()) !=
                                                blnInvert;
                                            break;
                                    }
                            }

                            if (blnCheckAll)
                            {
                                if (!boolSubNodeResult)
                                {
                                    blnOperationChildNodeResult = false;
                                    break;
                                }
                            }
                            // default is "any", replace above with a switch() should more than two checktypes be required
                            else if (boolSubNodeResult)
                            {
                                blnOperationChildNodeResult = true;
                                break;
                            }
                        }
                    }
                }

                if (blnIsOrNode && blnOperationChildNodeResult)
                    return true;
                if (!blnIsOrNode && !blnOperationChildNodeResult)
                    return false;
            }

            return !blnIsOrNode;
        }

        /// <summary>
        /// Like TryGetField for strings, only with as little overhead as possible.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetStringFieldQuickly(this XPathNavigator node, string field, ref string read)
        {
            XPathNavigator objField = node.SelectSingleNode(field) ?? node.SelectSingleNode("@" + field);
            if (objField != null)
            {
                read = objField.Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Like TryGetField for ints, but taking advantage of int.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetInt32FieldQuickly(this XPathNavigator node, string field, ref int read, IFormatProvider objCulture = null)
        {
            XPathNavigator objField = node.SelectSingleNode(field);
            if (objField != null)
            {
                if (objCulture == null)
                    objCulture = GlobalOptions.InvariantCultureInfo;
                if (int.TryParse(objField.Value, NumberStyles.Any, objCulture, out int intTmp))
                {
                    read = intTmp;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Like TryGetField for bools, but taking advantage of bool.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetBoolFieldQuickly(this XPathNavigator node, string field, ref bool read)
        {
            XPathNavigator objField = node.SelectSingleNode(field);
            if (objField != null)
            {
                if (bool.TryParse(objField.Value, out bool blnTmp))
                {
                    read = blnTmp;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Like TryGetField for decimals, but taking advantage of decimal.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetDecFieldQuickly(this XPathNavigator node, string field, ref decimal read, IFormatProvider objCulture = null)
        {
            XPathNavigator objField = node.SelectSingleNode(field);
            if (objField != null)
            {
                if (objCulture == null)
                    objCulture = GlobalOptions.InvariantCultureInfo;
                if (decimal.TryParse(objField.Value, NumberStyles.Any, objCulture, out decimal decTmp))
                {
                    read = decTmp;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Like TryGetField for doubles, but taking advantage of double.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetDoubleFieldQuickly(this XPathNavigator node, string field, ref double read, IFormatProvider objCulture = null)
        {
            XPathNavigator objField = node.SelectSingleNode(field);
            if (objField != null)
            {
                if (objCulture == null)
                    objCulture = GlobalOptions.InvariantCultureInfo;
                if (double.TryParse(objField.Value, NumberStyles.Any, objCulture, out double dblTmp))
                {
                    read = dblTmp;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Like TryGetField for float, but taking advantage of float.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetFloatFieldQuickly(this XPathNavigator node, string field, ref float read, IFormatProvider objCulture = null)
        {
            XPathNavigator objField = node.SelectSingleNode(field);
            if (objField != null)
            {
                if (objCulture == null)
                    objCulture = GlobalOptions.InvariantCultureInfo;
                if (float.TryParse(objField.Value, NumberStyles.Any, objCulture, out float fltTmp))
                {
                    read = fltTmp;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determine whether or not an XPathNavigator with the specified name exists within an XPathNavigator.
        /// </summary>
        /// <param name="xmlNode">XPathNavigator to examine.</param>
        /// <param name="strName">Name of the XPathNavigator to look for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NodeExists(this XPathNavigator xmlNode, string strName)
        {
            if (string.IsNullOrEmpty(strName))
                return false;
            return xmlNode?.SelectSingleNode(strName) != null;
        }
    }
}
