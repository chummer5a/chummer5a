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

#if DEBUG

using System.Diagnostics;

#endif

using System.Globalization;
using System.Xml;
using System.Runtime.CompilerServices;
using NLog;

namespace Chummer
{
    internal static class XmlNodeExtensions
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        //QUESTION: TrySelectField<T> that uses SelectSingleNode instead of this[node]?

        public delegate bool TryParseFunction<T>(string input, out T result);

        /// <summary>
        /// This method is syntactic sugar for attempting to read a data field
        /// from an XmlNode. This version sets the output variable to its
        /// default value in case of a failed read and can be used for
        /// initializing variables
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="node">The XmlNode to read from</param>
        /// <param name="field">The field to try and extract from the XmlNode</param>
        /// <param name="read">The variable to save the read to</param>
        /// <param name="onError">The value to return in case of failure. This parameter is optional</param>
        /// <returns>true if successful read</returns>
        public static bool TryGetField<T>(this XmlNode node, string field, out T read, T onError = default) where T : IConvertible
        {
            /*
             * This extension method allows easier access of xml, instead of
             * the old TryCatch blocks, not even logging the error
             *
             * It works because most of the types we read from the XmlNode is
             * IConvertible that can be converted to or from string with just
             * a type argument, first known at runtime (not true, but generics)
             *
             * because it is now a generic method, instead of
             * try{convert();}
             * catch{noop();}
             *
             * We can do some actual error checking instead of relying on exceptions
             * in case anything happens. We could do that before, but typing 10
             * lines to read a single variable 100 times would be insane
             *
             * That means this should be an order of magnitude faster in case of
             * missing fields and a little bit slower in case of fields being there
             *
             * To use this method, call it like this
             *
             * aXmlNode.TryGetField("fieldname", out myVariable);
             *
             * The compiler will fill out <T> itself, unless you specifically
             * tell it to be something else
             *
             * in case you need to act on whether the read was successful
             * do it like this
             * if(aXmlNode.TryGetField("fieldname", out myVariable))
             * {
             *     success();
             * }
             * else
             * {
             *     failure();
             * }
             */
            string fieldValue = null;
            if (!CheckGetField<T>(node, field, ref fieldValue))
            {
                read = onError;
                return false;
            }

            try
            {
                read = (T)Convert.ChangeType(fieldValue, typeof(T), GlobalSettings.InvariantCultureInfo);
                return true;
            }
            catch (Exception)
            {
                //If we are debugging, great
                //Utils.BreakIfDebug();

                //Otherwise just log it
#if DEBUG
                System.Reflection.MethodBase mth = new StackTrace().GetFrame(1).GetMethod();
                string errorMsg = string.Format
                    (
                        GlobalSettings.InvariantCultureInfo,
                        "Tried to read missing field \"{0}\" in {1}.{2}",
                        field,
                        mth.ReflectedType?.Name,
                        mth
                    );
#else
                string errorMsg = "Tried to read missing field \"" + field + '\"';
#endif
                Log.Error(errorMsg);
                //Finally, we have to assign an out parameter something, so default
                //null or 0 most likely
                read = onError;
                return false;
            }
        }

        /// <summary>
        /// This method is syntactic sugar for attempting to read a data field
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
        public static bool TryGetField<T>(this XmlNode node, string field, TryParseFunction<T> parser, out T read, T onError = default)
        {
            if (parser != null)
            {
                XmlNode xmlField = node?[field];
                if (xmlField != null)
                {
                    return parser(xmlField.InnerText, out read);
                }
            }

            read = onError;
            return false;
        }

        //T needed for debug info (so not)
        private static bool CheckGetField<T>(XmlNode node, string field, ref string fieldValue)
        {
            if (node[field] == null)
            {
#if DEBUG
                //Extra magic in debug builds, but can provide errors in release
                //builds due to inlining
                System.Reflection.MethodBase mth
                    = new StackTrace().GetFrame(2).GetMethod();
                string errorMsg = string.Format
                    (
                        GlobalSettings.InvariantCultureInfo,
                        "Tried to read missing field \"{0}\" of type \"{1}\" in {1}.{2}",
                        field,
                        typeof(T),
                        mth.ReflectedType?.Name
                    );
#else //So if DEBUG flag is missing we don't reflect info
                string errorMsg = string.Format
                    (
                        GlobalSettings.InvariantCultureInfo,
                        "Tried to read missing field \"{0}\" of type \"{1}\"",
                        field,
                        typeof(T)
                    );
#endif
                Log.Error(errorMsg);
                //Assign something
                return false;
            }

            fieldValue = node[field].InnerText;
            return true;
        }

        /// <summary>
        /// This method is syntactic sugar for attempting to read a data field
        /// from an XmlNode. This version preserves the output variable in case
        /// of a failed read
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="node">The XmlNode to read from</param>
        /// <param name="field">The field to try and extract from the XmlNode</param>
        /// <param name="read">The variable to save the read to, if successful</param>
        /// <returns>true if successful read</returns>
        public static bool TryPreserveField<T>(this XmlNode node, string field, ref T read) where T : IConvertible
        {
            if (node.TryGetField(field, out T value))
            {
                read = value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the specific field exists and is equal to value
        /// </summary>
        /// <param name="node">The XmlNode to read from</param>
        /// <param name="field">The field to check on the XmlNode</param>
        /// <param name="value">The value to compare to</param>
        /// <returns>true if the field exists and is equal to value</returns>
        public static bool TryCheckValue(this XmlNode node, string field, string value)
        {
            //QUESTION: Create regex version?
            if (node.TryGetField(field, out string fieldValue))
            {
                return fieldValue == value;
            }

            return false;
        }

        /// <summary>
        /// Processes a single operation node with children that are either nodes to check whether the parent has a node that fulfills a condition, or they are nodes that are parents to further operation nodes
        /// </summary>
        /// <param name="blnIsOrNode">Whether this is an OR node (true) or an AND node (false). Default is AND (false).</param>
        /// <param name="xmlOperationNode">The node containing the filter operation or a list of filter operations. Every element here is checked against corresponding elements in the parent node, using an operation specified in the element's attributes.</param>
        /// <param name="xmlParentNode">The parent node against which the filter operations are checked.</param>
        /// <returns>True if the parent node passes the conditions set in the operation node/nodelist, false otherwise.</returns>
        public static bool ProcessFilterOperationNode(this XmlNode xmlParentNode, XmlNode xmlOperationNode, bool blnIsOrNode)
        {
            if (xmlOperationNode == null)
                return false;

            using (XmlNodeList xmlOperationChildNodeList = xmlOperationNode.SelectNodes("*"))
            {
                if (xmlOperationChildNodeList != null)
                {
                    foreach (XmlNode xmlOperationChildNode in xmlOperationChildNodeList)
                    {
                        XmlAttributeCollection xmlOperationChildNodeAttributes = xmlOperationChildNode.Attributes;
                        bool blnInvert = xmlOperationChildNodeAttributes?["NOT"] != null;

                        bool blnOperationChildNodeResult = blnInvert;
                        string strNodeName = xmlOperationChildNode.Name;
                        switch (strNodeName)
                        {
                            case "OR":
                                blnOperationChildNodeResult =
                                    ProcessFilterOperationNode(xmlParentNode, xmlOperationChildNode, true) != blnInvert;
                                break;

                            case "AND":
                                blnOperationChildNodeResult =
                                    ProcessFilterOperationNode(xmlParentNode, xmlOperationChildNode, false) != blnInvert;
                                break;

                            case "NONE":
                                blnOperationChildNodeResult = (xmlParentNode == null) != blnInvert;
                                break;

                            default:
                                {
                                    if (xmlParentNode != null)
                                    {
                                        string strOperationType = xmlOperationChildNodeAttributes?["operation"]?.InnerText ?? "==";
                                        XmlNodeList objXmlTargetNodeList = xmlParentNode.SelectNodes(strNodeName);
                                        // If we're just checking for existence of a node, no need for more processing
                                        if (strOperationType == "exists")
                                        {
                                            blnOperationChildNodeResult = (objXmlTargetNodeList.Count > 0) != blnInvert;
                                        }
                                        else
                                        {
                                            // default is "any", replace with switch() if more check modes are necessary
                                            bool blnCheckAll = xmlOperationChildNodeAttributes?["checktype"]?.InnerText == "all";
                                            blnOperationChildNodeResult = blnCheckAll;
                                            string strOperationChildNodeText = xmlOperationChildNode.InnerText;
                                            bool blnOperationChildNodeEmpty = string.IsNullOrWhiteSpace(strOperationChildNodeText);

                                            foreach (XmlNode objXmlTargetNode in objXmlTargetNodeList)
                                            {
                                                bool boolSubNodeResult = blnInvert;
                                                if (objXmlTargetNode.SelectSingleNode("*") != null)
                                                {
                                                    if (xmlOperationChildNode.SelectSingleNode("*") != null)
                                                        boolSubNodeResult = ProcessFilterOperationNode(objXmlTargetNode,
                                                                                xmlOperationChildNode,
                                                                                xmlOperationChildNodeAttributes?["OR"] != null) !=
                                                                            blnInvert;
                                                }
                                                else
                                                {
                                                    string strTargetNodeText = objXmlTargetNode.InnerText;
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
                                                                goto default;
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
                                                // default is "any", replace above with a switch() should more than two check types be required
                                                else if (boolSubNodeResult)
                                                {
                                                    blnOperationChildNodeResult = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    break;
                                }
                        }

                        switch (blnIsOrNode)
                        {
                            case true when blnOperationChildNodeResult:
                                return true;

                            case false when !blnOperationChildNodeResult:
                                return false;
                        }
                    }
                }
            }

            return !blnIsOrNode;
        }

        /// <summary>
        /// Like TryGetField for strings, only with as little overhead as possible.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetStringFieldQuickly(this XmlNode node, string field, ref string read)
        {
            XmlElement objField = node?[field];
            if (objField != null)
            {
                read = objField.InnerText;
                return true;
            }
            XmlAttribute objAttribute = node?.Attributes?[field];
            if (objAttribute == null)
                return false;
            read = objAttribute.InnerText;
            return true;
        }

        /// <summary>
        /// Like TryGetField for strings, only with as little overhead as possible.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetMultiLineStringFieldQuickly(this XmlNode node, string field, ref string read)
        {
            string strReturn = string.Empty;
            if (node.TryGetStringFieldQuickly(field, ref strReturn))
            {
                read = strReturn.NormalizeLineEndings();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Like TryGetField for ints, but taking advantage of int.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetInt32FieldQuickly(this XmlNode node, string field, ref int read, IFormatProvider objCulture = null)
        {
            XmlElement objField = node?[field];
            if (objField == null)
                return false;
            if (objCulture == null)
                objCulture = GlobalSettings.InvariantCultureInfo;
            if (!int.TryParse(objField.InnerText, NumberStyles.Any, objCulture, out int intTmp))
                return false;
            read = intTmp;
            return true;
        }

        /// <summary>
        /// Like TryGetField for bools, but taking advantage of bool.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetBoolFieldQuickly(this XmlNode node, string field, ref bool read)
        {
            XmlElement objField = node?[field];
            if (objField == null)
                return false;
            if (!bool.TryParse(objField.InnerText, out bool blnTmp))
                return false;
            read = blnTmp;
            return true;
        }

        /// <summary>
        /// Like TryGetField for decimals, but taking advantage of decimal.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetDecFieldQuickly(this XmlNode node, string field, ref decimal read, IFormatProvider objCulture = null)
        {
            XmlElement objField = node?[field];
            if (objField == null)
                return false;
            if (objCulture == null)
                objCulture = GlobalSettings.InvariantCultureInfo;
            if (!decimal.TryParse(objField.InnerText, NumberStyles.Any, objCulture, out decimal decTmp))
                return false;
            read = decTmp;
            return true;
        }

        /// <summary>
        /// Like TryGetField for doubles, but taking advantage of double.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetDoubleFieldQuickly(this XmlNode node, string field, ref double read, IFormatProvider objCulture = null)
        {
            XmlElement objField = node?[field];
            if (objField == null)
                return false;
            if (objCulture == null)
                objCulture = GlobalSettings.InvariantCultureInfo;
            if (!double.TryParse(objField.InnerText, NumberStyles.Any, objCulture, out double dblTmp))
                return false;
            read = dblTmp;
            return true;
        }

        /// <summary>
        /// Like TryGetField for float, but taking advantage of float.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetFloatFieldQuickly(this XmlNode node, string field, ref float read, IFormatProvider objCulture = null)
        {
            XmlElement objField = node?[field];
            if (objField == null)
                return false;
            if (objCulture == null)
                objCulture = GlobalSettings.InvariantCultureInfo;
            if (!float.TryParse(objField.InnerText, NumberStyles.Any, objCulture, out float fltTmp))
                return false;
            read = fltTmp;
            return true;
        }

        /// <summary>
        /// Like TryGetField for guids, but taking advantage of guid.TryParse. Allows for returning false if the guid is Empty.
        /// </summary>
        /// <param name="node">XPathNavigator node of the object.</param>
        /// <param name="field">Field name of the InnerXML element we're looking for.</param>
        /// <param name="read">Guid that will be returned.</param>
        /// <param name="falseIfEmpty">Defaults to true. If false, will return an empty Guid if the returned Guid field is empty.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetGuidFieldQuickly(this XmlNode node, string field, ref Guid read, bool falseIfEmpty = true)
        {
            XmlNode objField = node.SelectSingleNode(field);
            if (objField == null)
                return false;
            if (!Guid.TryParse(objField.InnerText, out Guid guidTmp))
                return false;
            if (guidTmp == Guid.Empty && falseIfEmpty)
                return false;
            read = guidTmp;
            return true;
        }

        /// <summary>
        /// Determine whether or not an XmlNode with the specified name exists within an XmlNode.
        /// </summary>
        /// <param name="xmlNode">XmlNode to examine.</param>
        /// <param name="strName">Name of the XmlNode to look for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NodeExists(this XmlNode xmlNode, string strName)
        {
            if (string.IsNullOrEmpty(strName))
                return false;
            return xmlNode?.SelectSingleNode(strName) != null;
        }
    }
}
