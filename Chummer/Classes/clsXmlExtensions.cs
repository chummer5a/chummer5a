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
using System.Diagnostics;
 using System.Globalization;
 using System.Linq;
using System.Text;
using System.Xml;
using Chummer;
using System.Runtime.CompilerServices;

namespace Chummer
{
    internal static class XmlExtensions
    {
        //QUESTION: TrySelectField<T> that uses SelectSingleNode instead of this[node]?

        public delegate bool TryParseFunction<T>(string input, out T result);

        /// <summary>
        /// This method is syntaxtic sugar for atempting to read a data field
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
        public static bool TryGetField<T>(this XmlNode node, string field, out T read, T onError = default(T)) where T : IConvertible
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
             * We can do some acctual error checking instead of relying on exceptions
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
             * The compiler will fill out <T> itself, unless you specificaly 
             * tell it to be something else
             * 
             * in case you need to act on weither the read was successfull
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
                read = (T) Convert.ChangeType(fieldValue, typeof (T), GlobalOptions.InvariantCultureInfo);
                return true;
            }
            catch (Exception)
            {
                //If we are debugging, great
                //if (Debugger.IsAttached && false)
                //    Debugger.Break();

                //Otherwise just log it
#if DEBUG
                System.Reflection.MethodBase mth 
                    = new StackTrace().GetFrame(1).GetMethod();
                string errorMsg = string.Format("Tried to read missing field \"{0}\" in {1}.{2}", field, mth.ReflectedType.Name, mth);
#else
                string errorMsg = string.Format("Tried to read missing field \"{0}\"", field);
#endif
                Log.Error(errorMsg);
                //Finaly, we have to assign an out parameter something, so default
                //null or 0 most likeley
                read = onError;
                return false;
            }
        }


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
        public static bool TryGetField<T>(this XmlNode node, string field, TryParseFunction<T> parser, out T read,
            T onError = default(T))
        {
            string fieldValue = node[field]?.InnerText;
            if (parser != null && fieldValue != null)
            {
                return parser(fieldValue, out read);
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
                        "Tried to read missing field \"{0}\" of type \"{1}\" in {1}.{2}",
                        field,
                        typeof (T),
                        mth.ReflectedType.Name
                    );
#else //So if DEBUG flag is missing we don't reflect info
                string errorMsg = $"Tried to read missing field \"{field}\" of type \"{typeof(T)}\"";
#endif
                Log.Error(errorMsg);
                //Assign something
                return false;
            }

            fieldValue = node[field].InnerText;
            return true;
        }

        /// <summary>
        /// This method is syntaxtic sugar for atempting to read a data field
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
            T value;
            if (node.TryGetField(field, out value))
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
            string fieldValue;
            if (node.TryGetField(field, out fieldValue))
            {
                return fieldValue == value;
            }

            return false;
        }

        /// <summary>
        /// Processes a single operation node with children that are either nodes to check whether the parent has a node that fulfills a condition, or they are nodes that are parents to further operation nodes
        /// </summary>
        public static bool ProcessFilterOperationNode(this XmlNode objXmlParentNode, XmlNode objXmlOperationNode, bool boolIsOrNode = false)
        {
            if (objXmlParentNode == null || objXmlOperationNode == null)
                return false;

            XmlNodeList objXmlNodeList = objXmlOperationNode.SelectNodes("*");
            if (objXmlNodeList == null)
                return false;
            foreach (XmlNode objXmlOperationChildNode in objXmlNodeList)
            {
                bool boolInvert = objXmlOperationChildNode.Attributes?["NOT"] != null;

                bool boolOperationChildNodeResult;
                if (objXmlOperationChildNode.Name == "OR")
                {
                    boolOperationChildNodeResult = ProcessFilterOperationNode(objXmlParentNode, objXmlOperationChildNode, true) != boolInvert;
                }
                else if (objXmlOperationChildNode.Name == "AND")
                {
                    boolOperationChildNodeResult = ProcessFilterOperationNode(objXmlParentNode, objXmlOperationChildNode) != boolInvert;
                }
                else
                {
                    string strOperationType = objXmlOperationChildNode.Attributes?["operation"]?.InnerText ?? "==";
                    XmlNodeList objXmlTargetNodeList = objXmlParentNode.SelectNodes(objXmlOperationChildNode.Name);
                    // If we're just checking for existance of a node, no need for more processing
                    if (strOperationType == "exists")
                    {
                        boolOperationChildNodeResult = (objXmlTargetNodeList.Count > 0) != boolInvert;
                    }
                    else
                    {
                        // default is "any", replace with switch() if more check modes are necessary
                        bool blnCheckAll = objXmlOperationChildNode.Attributes?["checktype"]?.InnerText == "all";
                        boolOperationChildNodeResult = blnCheckAll;
                        bool blnOperationChildNodeEmpty =
                                    string.IsNullOrWhiteSpace(objXmlOperationChildNode.InnerText);

                        foreach (XmlNode objXmlTargetNode in objXmlTargetNodeList)
                        {
                            bool boolSubNodeResult = boolInvert;
                            if (objXmlTargetNode.SelectNodes("*").Count > 0)
                            {
                                if (objXmlOperationChildNode.SelectNodes("*").Count > 0)
                                    boolSubNodeResult = ProcessFilterOperationNode(objXmlTargetNode, objXmlOperationChildNode, objXmlOperationChildNode.Attributes?["OR"] != null) != boolInvert;
                            }
                            else
                            {
                                int intTargetNodeValue;
                                int intChildNodeValue;
                                bool blnTargetNodeEmpty =
                                    string.IsNullOrWhiteSpace(objXmlTargetNode.InnerText);
                                if (blnTargetNodeEmpty || blnOperationChildNodeEmpty)
                                {
                                    if (blnTargetNodeEmpty == blnOperationChildNodeEmpty &&
                                        (strOperationType == "==" || strOperationType == "equals"))
                                    {
                                        boolSubNodeResult = !boolInvert;
                                    }
                                    else
                                    {
                                        boolSubNodeResult = boolInvert;
                                    }
                                }
                                // Note when adding more operation cases: XML does not like the "<" symbol as part of an attribute value
                                else switch (strOperationType)
                                {
                                    case "doesnotequal":
                                    case "notequals":
                                    case "!=":
                                        boolInvert = !boolInvert;
                                        goto case "==";
                                    case "lessthan":
                                        boolInvert = !boolInvert;
                                        goto case ">=";
                                    case "lessthanequals":
                                        boolInvert = !boolInvert;
                                        goto case ">";

                                    case "like":
                                    case "contains":
                                        boolSubNodeResult = objXmlTargetNode.InnerText.Contains(objXmlOperationChildNode.InnerText) != boolInvert;
                                        break;
                                    case "greaterthan":
                                    case ">":
                                        boolSubNodeResult = (int.TryParse(objXmlTargetNode.InnerText, out intTargetNodeValue) &&
                                            int.TryParse(objXmlOperationChildNode.InnerText, out intChildNodeValue) &&
                                            intTargetNodeValue > intChildNodeValue) != boolInvert;
                                        break;
                                    case "greaterthanequals":
                                    case ">=":
                                        boolSubNodeResult = (int.TryParse(objXmlTargetNode.InnerText, out intTargetNodeValue) &&
                                            int.TryParse(objXmlOperationChildNode.InnerText, out intChildNodeValue) &&
                                            intTargetNodeValue >= intChildNodeValue) != boolInvert;
                                        break;
                                    case "equals":
                                    case "==":
                                    default:
                                        boolSubNodeResult = (objXmlTargetNode.InnerText.Trim() == objXmlOperationChildNode.InnerText.Trim()) != boolInvert;
                                        break;
                                }
                            }
                            if (blnCheckAll)
                            {
                                if (!boolSubNodeResult)
                                {
                                    boolOperationChildNodeResult = false;
                                    break;
                                }
                            }
                            // default is "any", replace above with a switch() should more than two checktypes be required
                            else if (boolSubNodeResult)
                            {
                                boolOperationChildNodeResult = true;
                                break;
                            }
                        }
                    }
                }
                if (boolIsOrNode && boolOperationChildNodeResult)
                    return true;
                if (!boolIsOrNode && !boolOperationChildNodeResult)
                    return false;
            }

            return !boolIsOrNode;
        }

        /// <summary>
        /// Like TryGetField for strings, only with as little overhead as possible.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetStringFieldQuickly(this XmlNode node, string field, ref string read)
        {

            XmlElement objField = node[field];
            if (objField != null)
            {
                read = objField.InnerText;
                return true;
            }
            XmlAttribute objAttribute = node.Attributes?[field];
            if (objAttribute != null)
            {
                read = objAttribute.InnerText;
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
            XmlElement objField = node[field];
            if (objField != null)
            {
                if (objCulture == null)
                    objCulture = GlobalOptions.InvariantCultureInfo;
                int intTmp;
                if (int.TryParse(objField.InnerText, NumberStyles.Any, objCulture, out intTmp))
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
        public static bool TryGetBoolFieldQuickly(this XmlNode node, string field, ref bool read)
        {
            XmlElement objField = node[field];
            if (objField != null)
            {
                bool blnTmp;
                if (bool.TryParse(objField.InnerText, out blnTmp))
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
        public static bool TryGetDecFieldQuickly(this XmlNode node, string field, ref decimal read, IFormatProvider objCulture = null)
        {
            XmlElement objField = node[field];
            if (objField != null)
            {
                if (objCulture == null)
                    objCulture = GlobalOptions.InvariantCultureInfo;
                decimal decTmp;
                if (decimal.TryParse(objField.InnerText, NumberStyles.Any, objCulture, out decTmp))
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
        public static bool TryGetDoubleFieldQuickly(this XmlNode node, string field, ref double read, IFormatProvider objCulture = null)
        {
            XmlElement objField = node[field];
            if (objField != null)
            {
                if (objCulture == null)
                    objCulture = GlobalOptions.InvariantCultureInfo;
                double dblTmp;
                if (double.TryParse(objField.InnerText, NumberStyles.Any, objCulture, out dblTmp))
                {
                    read = dblTmp;
                    return true;
                }
            }
            return false;
        }
    }
}