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
using System.Threading.Tasks;
using System.Threading;
using System.Xml.XPath;

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
                string errorMsg = "Tried to read missing field \"" + field + '\"';
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
                XmlElement xmlField = node?[field];
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
        /// Query the XmlNode for a given node with an id or name element. Includes ToUpperInvariant processing to handle uppercase ids.
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
        /// Query the XmlNode for a given node with an id. Includes ToUpperInvariant processing to handle uppercase ids.
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
        /// Effectively a version of SelectSingleNode(string xpath) that is slower on the first run (and consumes some memory), but faster on subsequent runs.
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
        /// Effectively a version of Select(string xpath) that is slower on the first run (and consumes some memory), but faster on subsequent runs.
        /// Only use this if there's a particular XPath expression that keeps being used over and over.
        /// </summary>
        public static XPathNodeIterator SelectAndCacheExpressionAsNavigator(this XmlNode xmlNode, string xpath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            XPathNavigator xmlNavigator = xmlNode.CreateNavigator();
            return xmlNavigator.SelectAndCacheExpression(xpath, token);
        }
    }
}
