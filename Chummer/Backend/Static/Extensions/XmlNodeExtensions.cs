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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using NLog;

namespace Chummer
{
    internal static class XmlNodeExtensions
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
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
            catch (Exception ex)
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
                string errorMsg = "Tried to read missing field \"" + field + "\"";
#endif
                ex = ex.Demystify();
                Log.Error(ex, errorMsg);
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
        /// a tryParse style function that is fed the node's <see cref="XmlNode.InnerText"/>
        /// </summary>
        public static bool TryGetField<T>(this XmlNode node, string field, TryParseFunction<T> parser, out T read, T onError = default)
        {
            if (parser != null)
            {
                XmlElement xmlField = node?[field];
                if (xmlField != null)
                {
                    return parser(xmlField.InnerTextViaPool(), out read);
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

            fieldValue = node[field].InnerTextViaPool();
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
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>True if the parent node passes the conditions set in the operation node/nodelist, false otherwise.</returns>
        public static bool ProcessFilterOperationNode(this XmlNode xmlParentNode, XPathNavigator xmlOperationNode,
                                                      bool blnIsOrNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            XPathNavigator xmlParentNavigator = xmlParentNode?.CreateNavigator();
            return xmlParentNavigator.ProcessFilterOperationNode(xmlOperationNode, blnIsOrNode, token);
        }

        /// <summary>
        /// Processes a single operation node with children that are either nodes to check whether the parent has a node that fulfills a condition, or they are nodes that are parents to further operation nodes
        /// </summary>
        /// <param name="blnIsOrNode">Whether this is an OR node (true) or an AND node (false). Default is AND (false).</param>
        /// <param name="xmlOperationNode">The node containing the filter operation or a list of filter operations. Every element here is checked against corresponding elements in the parent node, using an operation specified in the element's attributes.</param>
        /// <param name="xmlParentNode">The parent node against which the filter operations are checked.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>True if the parent node passes the conditions set in the operation node/nodelist, false otherwise.</returns>
        public static bool ProcessFilterOperationNode(this XmlNode xmlParentNode, XmlNode xmlOperationNode, bool blnIsOrNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            XPathNavigator xmlParentNavigator = xmlParentNode?.CreateNavigator();
            return xmlParentNavigator.ProcessFilterOperationNode(xmlOperationNode?.CreateNavigator(), blnIsOrNode, token);
        }

        /// <summary>
        /// Processes a single operation node with children that are either nodes to check whether the parent has a node that fulfills a condition, or they are nodes that are parents to further operation nodes
        /// </summary>
        /// <param name="blnIsOrNode">Whether this is an OR node (true) or an AND node (false). Default is AND (false).</param>
        /// <param name="xmlOperationNode">The node containing the filter operation or a list of filter operations. Every element here is checked against corresponding elements in the parent node, using an operation specified in the element's attributes.</param>
        /// <param name="xmlParentNode">The parent node against which the filter operations are checked.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>True if the parent node passes the conditions set in the operation node/nodelist, false otherwise.</returns>
        public static Task<bool> ProcessFilterOperationNodeAsync(this XmlNode xmlParentNode, XPathNavigator xmlOperationNode, bool blnIsOrNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<bool>(token);
            XPathNavigator xmlParentNavigator = xmlParentNode?.CreateNavigator();
            return token.IsCancellationRequested
                ? Task.FromCanceled<bool>(token)
                : xmlParentNavigator.ProcessFilterOperationNodeAsync(xmlOperationNode, blnIsOrNode, token);
        }

        /// <summary>
        /// Processes a single operation node with children that are either nodes to check whether the parent has a node that fulfills a condition, or they are nodes that are parents to further operation nodes
        /// </summary>
        /// <param name="blnIsOrNode">Whether this is an OR node (true) or an AND node (false). Default is AND (false).</param>
        /// <param name="xmlOperationNode">The node containing the filter operation or a list of filter operations. Every element here is checked against corresponding elements in the parent node, using an operation specified in the element's attributes.</param>
        /// <param name="xmlParentNode">The parent node against which the filter operations are checked.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>True if the parent node passes the conditions set in the operation node/nodelist, false otherwise.</returns>
        public static Task<bool> ProcessFilterOperationNodeAsync(this XmlNode xmlParentNode, XmlNode xmlOperationNode, bool blnIsOrNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<bool>(token);
            XPathNavigator xmlParentNavigator = xmlParentNode?.CreateNavigator();
            if (token.IsCancellationRequested)
                return Task.FromCanceled<bool>(token);
            XPathNavigator xmlOperationNavigator = xmlOperationNode?.CreateNavigator();
            return token.IsCancellationRequested
                ? Task.FromCanceled<bool>(token)
                : xmlParentNavigator.ProcessFilterOperationNodeAsync(xmlOperationNavigator, blnIsOrNode, token);
        }

        /// <summary>
        /// Like <see cref="TryGetField{T}(XmlNode, string, TryParseFunction{T}, out T, T)" /> for strings, only with as little overhead as possible.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetStringFieldQuickly(this XmlNode node, string field, ref string read)
        {
            XmlElement objField = node?[field];
            if (objField != null)
            {
                read = objField.InnerTextViaPool();
                return true;
            }
            XmlAttribute objAttribute = node?.Attributes?[field];
            if (objAttribute == null)
                return false;
            read = objAttribute.InnerTextViaPool();
            return true;
        }

        /// <summary>
        /// Like <see cref="TryGetField{T}(XmlNode, string, TryParseFunction{T}, out T, T)" /> for strings, only with as little overhead as possible.
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
        /// Like <see cref="TryGetField{T}(XmlNode, string, TryParseFunction{T}, out T, T)" /> for ints, but taking advantage of <see cref="int.TryParse(string, out int)"/>... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetInt32FieldQuickly(this XmlNode node, string field, ref int read, IFormatProvider objCulture = null)
        {
            XmlElement objField = node?[field];
            if (objField == null)
                return false;
            if (objCulture == null)
                objCulture = GlobalSettings.InvariantCultureInfo;
            if (!int.TryParse(objField.InnerTextViaPool(), NumberStyles.Any, objCulture, out int intTmp))
                return false;
            read = intTmp;
            return true;
        }

        /// <summary>
        /// Like <see cref="TryGetField{T}(XmlNode, string, TryParseFunction{T}, out T, T)" /> for bools, but taking advantage of <see cref="bool.TryParse(string, out bool)"/>... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetBoolFieldQuickly(this XmlNode node, string field, ref bool read)
        {
            XmlElement objField = node?[field];
            if (objField == null)
                return false;
            if (!bool.TryParse(objField.InnerTextViaPool(), out bool blnTmp))
                return false;
            read = blnTmp;
            return true;
        }

        /// <summary>
        /// Like <see cref="TryGetField{T}(XmlNode, string, TryParseFunction{T}, out T, T)" /> for decimals, but taking advantage of <see cref="decimal.TryParse(string, out decimal)"/>... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetDecFieldQuickly(this XmlNode node, string field, ref decimal read, IFormatProvider objCulture = null)
        {
            XmlElement objField = node?[field];
            if (objField == null)
                return false;
            if (objCulture == null)
                objCulture = GlobalSettings.InvariantCultureInfo;
            if (!decimal.TryParse(objField.InnerTextViaPool(), NumberStyles.Any, objCulture, out decimal decTmp))
                return false;
            read = decTmp;
            return true;
        }

        /// <summary>
        /// Like <see cref="TryGetField{T}(XmlNode, string, TryParseFunction{T}, out T, T)" /> for doubles, but taking advantage of <see cref="double.TryParse(string, out double)"/>... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetDoubleFieldQuickly(this XmlNode node, string field, ref double read, IFormatProvider objCulture = null)
        {
            XmlElement objField = node?[field];
            if (objField == null)
                return false;
            if (objCulture == null)
                objCulture = GlobalSettings.InvariantCultureInfo;
            if (!double.TryParse(objField.InnerTextViaPool(), NumberStyles.Any, objCulture, out double dblTmp))
                return false;
            read = dblTmp;
            return true;
        }

        /// <summary>
        /// Like <see cref="TryGetField{T}(XmlNode, string, TryParseFunction{T}, out T, T)" /> for float, but taking advantage of <see cref="float.TryParse(string, out float)"/>... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetFloatFieldQuickly(this XmlNode node, string field, ref float read, IFormatProvider objCulture = null)
        {
            XmlElement objField = node?[field];
            if (objField == null)
                return false;
            if (objCulture == null)
                objCulture = GlobalSettings.InvariantCultureInfo;
            if (!float.TryParse(objField.InnerTextViaPool(), NumberStyles.Any, objCulture, out float fltTmp))
                return false;
            read = fltTmp;
            return true;
        }

        /// <summary>
        /// Like <see cref="TryGetField{T}(XmlNode, string, TryParseFunction{T}, out T, T)" /> for guids, but taking advantage of <see cref="Guid.TryParse(string, out Guid)"/>. Allows for returning false if the guid is <see cref="Guid.Empty"/>.
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
            if (!Guid.TryParse(objField.InnerTextViaPool(), out Guid guidTmp))
                return false;
            if (guidTmp == Guid.Empty && falseIfEmpty)
                return false;
            read = guidTmp;
            return true;
        }

        /// <summary>
        /// Query the XmlNode for a given node with an id or name element. Includes <see cref="string.ToUpperInvariant"/> processing to handle uppercase ids.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode TryGetNodeByNameOrId(this XmlNode node, string strPath, string strId, string strExtraXPath = "")
        {
            if (node == null || string.IsNullOrEmpty(strPath) || string.IsNullOrEmpty(strId))
                return null;
            XmlNode objReturn;
            if (Guid.TryParse(strId, out Guid guidId))
            {
                objReturn = node.TryGetNodeById(strPath, guidId, strExtraXPath);
                if (objReturn != null)
                    return objReturn;
            }

            string strIdCleaned = strId.CleanXPath();
            objReturn = node.SelectSingleNode(strPath + "[name = " + strIdCleaned
                                         + (string.IsNullOrEmpty(strExtraXPath)
                                             ? "]"
                                             : " and (" + strExtraXPath + ")]"));
            if (objReturn != null)
                return objReturn;
            // There are cases where we use ids that are not Guids (e.g., custom improvements), so we need this part as well.
            return node.SelectSingleNode(strPath + "[id = " + strIdCleaned
                                         + (string.IsNullOrEmpty(strExtraXPath)
                                             ? "]"
                                             : " and (" + strExtraXPath + ")]"));
        }

        /// <summary>
        /// Query the XmlNode for a given node with an id. Includes <see cref="string.ToUpperInvariant"/> processing to handle uppercase ids.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode TryGetNodeById(this XmlNode node, string strPath, Guid guidId, string strExtraXPath = "")
        {
            if (node == null || string.IsNullOrEmpty(strPath))
                return null;
            string strSuffix = string.IsNullOrEmpty(strExtraXPath) ? "]" : " and (" + strExtraXPath + ")]";
            string strId = guidId.ToString("D", GlobalSettings.InvariantCultureInfo);
            return node.SelectSingleNode(strPath + "[id = " + strId.CleanXPath() + strSuffix)
                   // Split into two separate queries because the case-insensitive search here can be expensive if we're doing it a lot
                   ?? node.SelectSingleNode(strPath + "[translate(id, 'abcdef', 'ABCDEF') = " + strId.ToUpperInvariant().CleanXPath() + strSuffix);
        }

        /// <summary>
        /// Determine whether an XmlNode with the specified name exists within an XmlNode.
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

        /// <summary>
        /// Selects a single node using the specified XPath expression, but also caches that expression in case the same expression is used over and over.
        /// Effectively a version of <see cref="XmlNode.SelectSingleNode(string)"/> that is slower on the first run (and consumes some memory), but faster on subsequent runs.
        /// Only use this if there's a particular XPath expression that keeps being used over and over.
        /// </summary>
        public static XPathNavigator SelectSingleNodeAndCacheExpressionAsNavigator(this XmlNode xmlNode, string xpath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            XPathNavigator xmlNavigator = xmlNode.CreateNavigator();
            return xmlNavigator.SelectSingleNodeAndCacheExpression(xpath, token);
        }

        /// <summary>
        /// Selects a node set using the specified XPath expression, but also caches that expression in case the same expression is used over and over.
        /// Effectively a version of <see cref="XmlNode.SelectNodes(string)"/> that is slower on the first run (and consumes some memory), but faster on subsequent runs.
        /// Only use this if there's a particular XPath expression that keeps being used over and over.
        /// </summary>
        public static XPathNodeIterator SelectAndCacheExpressionAsNavigator(this XmlNode xmlNode, string xpath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            XPathNavigator xmlNavigator = xmlNode.CreateNavigator();
            return xmlNavigator.SelectAndCacheExpression(xpath, token);
        }

        /// <summary>
        /// Attempts to get a node from the saved data, falling back to the source XML node if the saved node is null or empty.
        /// This is useful for legacy shims when loading saved data that may have empty elements that should be restored from source definitions.
        /// Like <see cref="TryGetStringFieldQuickly" /> for nodes, with source fallback support.
        /// </summary>
        /// <param name="objSavedNode">The XML node from saved data (e.g., character file)</param>
        /// <param name="strFieldName">The name of the field/node to retrieve</param>
        /// <param name="read">The variable to save the read node to. Will be set to the node from saved data if it exists and is not empty, otherwise the node from source data, or null if neither exists.</param>
        /// <param name="objSourceNode">The XML node from source data (e.g., gear definition). Can be null.</param>
        /// <returns>True if a node was found (either from saved or source data), false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetNodeWithSourceFallback(this XmlNode objSavedNode, string strFieldName, ref XmlNode read, XmlNode objSourceNode)
        {
            XmlNode objNode = objSavedNode?[strFieldName];
            if (!objNode.IsNullOrInnerTextIsEmpty())
            {
                read = objNode;
                return true;
            }
            read = objSourceNode?[strFieldName];
            return read != null;
        }

        /// <summary>
        /// Attempts to get an element from the saved data, falling back to the source XML node if the saved node is null or empty.
        /// Overload for <see cref="TryGetNodeWithSourceFallback(XmlNode, string, ref XmlNode, XmlNode)" /> that works with XmlElement fields.
        /// </summary>
        /// <param name="objSavedNode">The XML node from saved data (e.g., character file)</param>
        /// <param name="strFieldName">The name of the field/node to retrieve</param>
        /// <param name="read">The variable to save the read element to. Will be set to the element from saved data if it exists and is not empty, otherwise the element from source data, or null if neither exists.</param>
        /// <param name="objSourceNode">The XML node from source data (e.g., gear definition). Can be null.</param>
        /// <returns>True if an element was found (either from saved or source data), false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetNodeWithSourceFallback(this XmlNode objSavedNode, string strFieldName, ref XmlElement read, XmlNode objSourceNode)
        {
            XmlElement objElement = objSavedNode?[strFieldName] as XmlElement;
            if (objElement != null && !objElement.IsNullOrInnerTextIsEmpty())
            {
                read = objElement;
                return true;
            }
            read = objSourceNode?[strFieldName] as XmlElement;
            return read != null;
        }

        /// <summary>
        /// Attempts to get a node from the saved data, falling back to the source XPathNavigator if the saved node is null or empty.
        /// This is useful for legacy shims when loading saved data that may have empty elements that should be restored from source definitions.
        /// Like <see cref="TryGetStringFieldQuickly" /> for nodes, with source fallback support.
        /// This overload accepts XPathNavigator for the source to avoid the performance cost of calling GetNode().
        /// </summary>
        /// <param name="objSavedNode">The XML node from saved data (e.g., character file)</param>
        /// <param name="strFieldName">The name of the field/node to retrieve</param>
        /// <param name="read">The variable to save the read node to. Will be set to the node from saved data if it exists and is not empty, otherwise converted from the source XPathNavigator, or null if neither exists.</param>
        /// <param name="objSourceNavigator">The XPathNavigator from source data (e.g., gear definition). Can be null.</param>
        /// <returns>True if a node was found (either from saved or source data), false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetNodeWithSourceFallback(this XmlNode objSavedNode, string strFieldName, ref XmlNode read, XPathNavigator objSourceNavigator)
        {
            XmlNode objNode = objSavedNode?[strFieldName];
            if (!objNode.IsNullOrInnerTextIsEmpty())
            {
                read = objNode;
                return true;
            }
            if (objSourceNavigator != null)
            {
                XPathNavigator objSourceNode = objSourceNavigator.SelectSingleNode(strFieldName);
                if (objSourceNode != null && !objSourceNode.IsNullOrInnerTextIsEmpty())
                {
                    // Convert XPathNavigator to XmlNode using the saved node's document
                    XmlDocument objDocument = objSavedNode?.OwnerDocument ?? new XmlDocument();
                    read = objSourceNode.ToXmlNode(objDocument);
                    return true;
                }
            }
            read = null;
            return false;
        }

        /// <summary>
        /// Attempts to get an element from the saved data, falling back to the source XPathNavigator if the saved element is null or empty.
        /// Overload for <see cref="TryGetNodeWithSourceFallback(XmlNode, string, ref XmlNode, XPathNavigator)" /> that works with XmlElement fields.
        /// This overload accepts XPathNavigator for the source to avoid the performance cost of calling GetNode().
        /// </summary>
        /// <param name="objSavedNode">The XML node from saved data (e.g., character file)</param>
        /// <param name="strFieldName">The name of the field/element to retrieve</param>
        /// <param name="read">The variable to save the read element to. Will be set to the element from saved data if it exists and is not empty, otherwise converted from the source XPathNavigator, or null if neither exists.</param>
        /// <param name="objSourceNavigator">The XPathNavigator from source data (e.g., gear definition). Can be null.</param>
        /// <returns>True if an element was found (either from saved or source data), false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetNodeWithSourceFallback(this XmlNode objSavedNode, string strFieldName, ref XmlElement read, XPathNavigator objSourceNavigator)
        {
            XmlElement objElement = objSavedNode?[strFieldName] as XmlElement;
            if (objElement != null && !objElement.IsNullOrInnerTextIsEmpty())
            {
                read = objElement;
                return true;
            }
            if (objSourceNavigator != null)
            {
                XPathNavigator objSourceNode = objSourceNavigator.SelectSingleNode(strFieldName);
                if (objSourceNode != null && !objSourceNode.IsNullOrInnerTextIsEmpty())
                {
                    // Convert XPathNavigator to XmlNode using the saved node's document
                    XmlDocument objDocument = objSavedNode?.OwnerDocument ?? new XmlDocument();
                    XmlNode objConvertedNode = objSourceNode.ToXmlNode(objDocument);
                    read = objConvertedNode as XmlElement;
                    return read != null;
                }
            }
            read = null;
            return false;
        }

        /// <summary>
        /// Syntactic sugar for an equivalent of calling <see cref="string.IsNullOrEmpty(string)"/> on <see cref="XmlNode.InnerText"/> with a null check, but without needing any sort of heap allocations.
        /// This helps reduce GC pressure and makes the program feel more responsive, especially when saving or loading things.
        /// </summary>
        public static bool IsNullOrInnerTextIsEmpty(this XmlNode xmlNode)
        {
            XmlNode firstChild = xmlNode?.FirstChild;
            if (firstChild == null)
                return true;

            if (firstChild.NextSibling == null)
            {
                XmlNodeType nodeType = firstChild.NodeType;
                if (nodeType == XmlNodeType.Text || nodeType == XmlNodeType.CDATA || nodeType == XmlNodeType.Whitespace || nodeType == XmlNodeType.SignificantWhitespace)
                    return string.IsNullOrWhiteSpace(firstChild.Value);
                // For Element nodes or other types, recursively check if they have content
                return CheckChildren(xmlNode);
            }

            return CheckChildren(xmlNode);

            bool CheckChildren(XmlNode xmlParentNode)
            {
                for (XmlNode xmlNodeInner = xmlParentNode.FirstChild; xmlNodeInner != null; xmlNodeInner = xmlNodeInner.NextSibling)
                {
                    if (xmlNodeInner.FirstChild == null)
                    {
                        XmlNodeType nodeType = xmlNodeInner.NodeType;
                        if ((nodeType == XmlNodeType.Text || nodeType == XmlNodeType.CDATA || nodeType == XmlNodeType.Whitespace || nodeType == XmlNodeType.SignificantWhitespace) && !string.IsNullOrWhiteSpace(xmlNodeInner.Value))
                            return false;
                    }
                    else if (!CheckChildren(xmlNodeInner))
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Syntactic sugar to check if <see cref="XmlNode.InnerText"/> of a node would just be <see cref="bool.TrueString"/> without needing to go through rendering the node's entire contents.
        /// This helps reduce GC pressure and makes the program feel more responsive, especially when saving or loading things.
        /// </summary>
        public static bool InnerTextIsTrueString(this XmlNode xmlNode)
        {
            bool blnFoundTextElement = false;
            for (XmlNode xmlNodeInner = xmlNode.FirstChild; xmlNodeInner != null; xmlNodeInner = xmlNodeInner.NextSibling)
            {
                switch (xmlNodeInner.NodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.EndElement:
                        return false;
                    case XmlNodeType.Text:
                        if (!string.Equals(xmlNodeInner.Value, bool.TrueString, StringComparison.OrdinalIgnoreCase))
                            return false;
                        blnFoundTextElement = true;
                        break;
                }
            }
            return blnFoundTextElement;
        }

        /// <summary>
        /// Syntactic sugar to check if <see cref="XmlNode.InnerText"/> of a node would just be <see cref="bool.FalseString"/> without needing to go through rendering the node's entire contents.
        /// This helps reduce GC pressure and makes the program feel more responsive, especially when saving or loading things.
        /// </summary>
        public static bool InnerTextIsFalseString(this XmlNode xmlNode)
        {
            bool blnFoundTextElement = false;
            for (XmlNode xmlNodeInner = xmlNode.FirstChild; xmlNodeInner != null; xmlNodeInner = xmlNodeInner.NextSibling)
            {
                switch (xmlNodeInner.NodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.EndElement:
                        return false;
                    case XmlNodeType.Text:
                        if (!string.Equals(xmlNodeInner.Value, bool.FalseString, StringComparison.OrdinalIgnoreCase))
                            return false;
                        blnFoundTextElement = true;
                        break;
                }
            }
            return blnFoundTextElement;
        }

        /// <summary>
        /// Syntactic sugar for checking if a compound node's children have any whose name matches a string without needing to go through rendering the node's contents.
        /// This helps reduce GC pressure and makes the program feel more responsive, especially when saving or loading things.
        /// </summary>
        public static bool HasChildWithName(this XmlNode xmlNode, string strName)
        {
            if (string.IsNullOrEmpty(strName))
                return false;

            for (XmlNode xmlNodeInner = xmlNode.FirstChild; xmlNodeInner != null; xmlNodeInner = xmlNodeInner.NextSibling)
            {
                if (string.Equals(xmlNodeInner.Name, strName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Syntactic sugar for checking if a node's contents contain a substring without needing to go through rendering the node's entire contents.
        /// For compound nodes, checks each child of the node individually (and also checks their names), meaning the needle will not fire if it would need to be matched across multiple nodes or across a node's name and contents (use InnerText for that).
        /// This helps reduce GC pressure and makes the program feel more responsive, especially when saving or loading things.
        /// </summary>
        public static bool InnerXmlContentContains(this XmlNode xmlNode, string strNeedle, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strNeedle))
                return false;

            XmlNode firstChild = xmlNode.FirstChild;
            if (firstChild == null)
            {
                return false;
            }

            if (firstChild.NextSibling == null)
            {
                token.ThrowIfCancellationRequested();
                if (firstChild.Name.Contains(strNeedle))
                    return true;
                XmlNodeType nodeType = firstChild.NodeType;
                if (nodeType == XmlNodeType.Text || nodeType == XmlNodeType.CDATA || nodeType == XmlNodeType.Whitespace || nodeType == XmlNodeType.SignificantWhitespace)
                {
                    token.ThrowIfCancellationRequested();
                    return firstChild.Value.Contains(strNeedle);
                }
            }

            return CheckChildren(xmlNode);

            bool CheckChildren(XmlNode xmlParentNode)
            {
                for (XmlNode xmlNodeInner = xmlParentNode.FirstChild; xmlNodeInner != null; xmlNodeInner = xmlNodeInner.NextSibling)
                {
                    token.ThrowIfCancellationRequested();
                    if (xmlNodeInner.Name.Contains(strNeedle))
                        return true;
                    if (xmlNodeInner.FirstChild == null)
                    {
                        if (xmlNodeInner.NodeType == XmlNodeType.Text || xmlNodeInner.NodeType == XmlNodeType.CDATA || xmlNodeInner.NodeType == XmlNodeType.Whitespace || xmlNodeInner.NodeType == XmlNodeType.SignificantWhitespace)
                        {
                            token.ThrowIfCancellationRequested();
                            if (xmlNodeInner.Value.Contains(strNeedle))
                                return true;
                        }
                    }
                    else if (CheckChildren(xmlNodeInner))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Copy of <see cref="XmlNode.InnerText" />, but going through <see cref="Utils.StringBuilderPool"/> instead creating a new one via heap allocation.
        /// This helps reduce GC pressure and makes the program feel more responsive, especially when saving or loading things.
        /// </summary>
        public static string InnerTextViaPool(this XmlNode xmlNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            XmlNode firstChild = xmlNode.FirstChild;
            if (firstChild == null)
            {
                return string.Empty;
            }

            if (firstChild.NextSibling == null)
            {
                XmlNodeType nodeType = firstChild.NodeType;
                if (nodeType == XmlNodeType.Text || nodeType == XmlNodeType.CDATA || nodeType == XmlNodeType.Whitespace || nodeType == XmlNodeType.SignificantWhitespace)
                {
                    return firstChild.Value;
                }
            }

            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                token.ThrowIfCancellationRequested();
                AppendChildText(xmlNode, sbdReturn);
                token.ThrowIfCancellationRequested();
                return sbdReturn.ToString();
            }

            void AppendChildText(XmlNode xmlParentNode, StringBuilder builder)
            {
                for (XmlNode xmlNodeInner = xmlParentNode.FirstChild; xmlNodeInner != null; xmlNodeInner = xmlNodeInner.NextSibling)
                {
                    token.ThrowIfCancellationRequested();
                    if (xmlNodeInner.FirstChild == null)
                    {
                        if (xmlNodeInner.NodeType == XmlNodeType.Text || xmlNodeInner.NodeType == XmlNodeType.CDATA || xmlNodeInner.NodeType == XmlNodeType.Whitespace || xmlNodeInner.NodeType == XmlNodeType.SignificantWhitespace)
                        {
                            builder.Append(xmlNodeInner.InnerTextViaPool(token));
                        }
                    }
                    else
                    {
                        AppendChildText(xmlNodeInner, builder);
                    }
                }
            }
        }

        /// <summary>
        /// Copy of <see cref="XmlNode.InnerText" /> followed by <see cref="string.Trim()"/>, but going through <see cref="Utils.StringBuilderPool"/> and <see cref="StringBuilderExtensions.ToTrimmedString(StringBuilder)"/> instead creating a new one via heap allocation.
        /// This helps reduce GC pressure and makes the program feel more responsive, especially when saving or loading things.
        /// </summary>
        public static string InnerTextViaPoolTrimmed(this XmlNode xmlNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            XmlNode firstChild = xmlNode.FirstChild;
            if (firstChild == null)
            {
                return string.Empty;
            }

            if (firstChild.NextSibling == null)
            {
                XmlNodeType nodeType = firstChild.NodeType;
                if (nodeType == XmlNodeType.Text || nodeType == XmlNodeType.CDATA || nodeType == XmlNodeType.Whitespace || nodeType == XmlNodeType.SignificantWhitespace)
                {
                    return firstChild.Value;
                }
            }

            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                token.ThrowIfCancellationRequested();
                AppendChildText(xmlNode, sbdReturn);
                token.ThrowIfCancellationRequested();
                return sbdReturn.ToTrimmedString();
            }

            void AppendChildText(XmlNode xmlParentNode, StringBuilder builder)
            {
                for (XmlNode xmlNodeInner = xmlParentNode.FirstChild; xmlNodeInner != null; xmlNodeInner = xmlNodeInner.NextSibling)
                {
                    token.ThrowIfCancellationRequested();
                    if (xmlNodeInner.FirstChild == null)
                    {
                        if (xmlNodeInner.NodeType == XmlNodeType.Text || xmlNodeInner.NodeType == XmlNodeType.CDATA || xmlNodeInner.NodeType == XmlNodeType.Whitespace || xmlNodeInner.NodeType == XmlNodeType.SignificantWhitespace)
                        {
                            builder.Append(xmlNodeInner.InnerTextViaPool(token));
                        }
                    }
                    else
                    {
                        AppendChildText(xmlNodeInner, builder);
                    }
                }
            }
        }

        /// <summary>
        /// Copy of <see cref="XmlNode.InnerXml" />, but going through <see cref="Utils.StringBuilderPool"/> instead creating a new one via heap allocation.
        /// This helps reduce GC pressure and makes the program feel more responsive, especially when saving or loading things.
        /// </summary>
        public static string InnerXmlViaPool(this XmlNode xmlNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                token.ThrowIfCancellationRequested();
                using (StringWriter objStringWriter = new StringWriter(sbdReturn, GlobalSettings.InvariantCultureInfo))
                {
                    token.ThrowIfCancellationRequested();
                    using (XmlDOMTextWriter objXmlWriter = new XmlDOMTextWriter(objStringWriter))
                    {
                        token.ThrowIfCancellationRequested();
                        xmlNode.WriteContentTo(objXmlWriter);
                    }
                }
                token.ThrowIfCancellationRequested();
                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// Copy of <see cref="XmlNode.OuterXml" />, but going through <see cref="Utils.StringBuilderPool"/> instead creating a new one via heap allocation.
        /// This helps reduce GC pressure and makes the program feel more responsive, especially when saving or loading things.
        /// </summary>
        public static string OuterXmlViaPool(this XmlNode xmlNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                token.ThrowIfCancellationRequested();
                using (StringWriter objStringWriter = new StringWriter(sbdReturn, GlobalSettings.InvariantCultureInfo))
                {
                    token.ThrowIfCancellationRequested();
                    using (XmlDOMTextWriter objXmlWriter = new XmlDOMTextWriter(objStringWriter))
                    {
                        token.ThrowIfCancellationRequested();
                        xmlNode.WriteTo(objXmlWriter);
                    }
                }
                token.ThrowIfCancellationRequested();
                return sbdReturn.ToString();
            }
        }
    }
}
