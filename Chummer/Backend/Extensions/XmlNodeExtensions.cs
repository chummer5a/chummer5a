using System;
using System.Collections;
using System.Xml;

namespace Chummer.Backend
{
	static class XmlNodeExtensions
	{
        /// <summary>
        /// Processes a single operation node with children that are either nodes to check whether the parent has a node that fulfills a condition, or they are nodes that are parents to further operation nodes
        /// </summary>
        public static bool processFilterOperationNode(XmlNode objXmlParentNode, XmlNode objXmlOperationNode, bool boolIsOrNode = false)
        {
            if (objXmlParentNode == null || objXmlOperationNode == null)
                return false;

            bool boolReturnValue = !boolIsOrNode;
            XmlNodeList objXmlNodeList = objXmlOperationNode.SelectNodes("*");
            bool boolInvert = false;
            string strOperationType = "==";
            bool boolOperationChildNodeResult = false;
            bool boolSubNodeResult = false;
            foreach (XmlNode objXmlOperationChildNode in objXmlNodeList)
            {
                boolInvert = false;
                if (objXmlOperationChildNode.Attributes?["NOT"] != null)
                    boolInvert = true;

                if (objXmlOperationChildNode.Name == "OR")
                {
                    boolOperationChildNodeResult = processFilterOperationNode(objXmlParentNode, objXmlOperationChildNode, true) != boolInvert;
                }
                else if (objXmlOperationChildNode.Name == "AND")
                {
                    boolOperationChildNodeResult = processFilterOperationNode(objXmlParentNode, objXmlOperationChildNode, false) != boolInvert;
                }
                else
                {
                    XmlNodeList objXmlTargetNodeList = objXmlParentNode.SelectNodes(objXmlOperationChildNode.Name);
                    // default is "any", replace with switch() if more check modes are necessary
                    boolOperationChildNodeResult = false;
                    if (objXmlOperationChildNode?.Attributes?["checktype"]?.InnerText == "all")
                        boolOperationChildNodeResult = true;

                    boolOperationChildNodeResult = boolSubNodeResult = boolInvert;
                    foreach (XmlNode objXmlTargetNode in objXmlTargetNodeList)
                    {
                        boolSubNodeResult = boolInvert;
                        if (objXmlTargetNode.SelectNodes("*").Count > 0)
                        {
                            if (objXmlOperationChildNode.SelectNodes("*").Count > 0)
                                boolSubNodeResult = processFilterOperationNode(objXmlTargetNode, objXmlOperationChildNode, objXmlOperationChildNode.Attributes?["OR"] != null) != boolInvert;
                        }
                        else
                        {
                            strOperationType = "==";
                            if (objXmlOperationChildNode.Attributes["operation"] != null)
                                strOperationType = objXmlOperationChildNode.Attributes["operation"].InnerText;
                            // Note when adding more operation cases: XML does not like the "<" symbol as part of an attribute value
                            switch (strOperationType)
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
                                    boolSubNodeResult = (System.Convert.ToInt32(objXmlTargetNode.InnerText) > Convert.ToInt32(objXmlOperationChildNode.InnerText)) != boolInvert;
                                    break;
                                case "greaterthanequals":
                                case ">=":
                                    boolSubNodeResult = (Convert.ToInt32(objXmlTargetNode.InnerText) >= Convert.ToInt32(objXmlOperationChildNode.InnerText)) != boolInvert;
                                    break;
                                case "equals":
                                case "==":
                                default:
                                    boolSubNodeResult = (objXmlTargetNode.InnerText == objXmlOperationChildNode.InnerText) != boolInvert;
                                    break;
                            }
                        }
                        if (objXmlOperationChildNode.Attributes?["checktype"]?.InnerText == "all")
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
                if (boolIsOrNode && boolOperationChildNodeResult)
                    return true;
                else if (!boolIsOrNode && !boolOperationChildNodeResult)
                    return false;
            }

            return boolReturnValue;
        }
    }
}
