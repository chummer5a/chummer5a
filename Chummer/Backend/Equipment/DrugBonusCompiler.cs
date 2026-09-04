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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Compiles drug effects into a &lt;bonus&gt; node for <see cref="ImprovementManager.CreateImprovementsAsync"/>.
    /// </summary>
    public static class DrugBonusCompiler
    {
        /// <summary>
        /// Result of compiling a drug's effects.
        /// </summary>
        public sealed class CompileResult
        {
            /// <summary>
            /// Bonus node suitable for CreateImprovements (quality nodes removed).
            /// </summary>
            public XmlNode BonusNode { get; set; }

            /// <summary>
            /// Quality nodes stripped from the bonus; handled as disabled SpecificQuality stubs.
            /// </summary>
            public IReadOnlyList<XmlNode> QualityNodes { get; set; } = Array.Empty<XmlNode>();
        }

        /// <summary>
        /// Produces the bonus XML used to create drug improvements.
        /// </summary>
        /// <param name="objCharacter">Character owning the drug.</param>
        /// <param name="objDrug">Drug being activated.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Compiled bonus and extracted quality nodes.</returns>
        public static async Task<CompileResult> CompileAsync(Character objCharacter, Drug objDrug,
            CancellationToken token = default)
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            if (objDrug == null)
                throw new ArgumentNullException(nameof(objDrug));

            token.ThrowIfCancellationRequested();

            XmlNode nodSourceBonus = objDrug.Components.Count > 0
                ? await BuildCustomBonusNodeAsync(objCharacter, objDrug, token).ConfigureAwait(false)
                : await GetCatalogBonusNodeAsync(objCharacter, objDrug, token).ConfigureAwait(false);

            if (nodSourceBonus == null)
                return new CompileResult();

            return SplitQualityNodes(NormalizeBonusNode(nodSourceBonus));
        }

        /// <summary>
        /// Returns the catalog &lt;bonus&gt; child for a premade drug, if present.
        /// </summary>
        /// <param name="objCharacter">Character owning the drug.</param>
        /// <param name="objDrug">Drug with a catalog <see cref="Drug.SourceID"/>.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The bonus node, or null when not found.</returns>
        public static async Task<XmlNode> GetCatalogBonusNodeAsync(Character objCharacter, Drug objDrug,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objDrug.SourceID == Guid.Empty)
                return null;

            XmlDocument objDoc =
                await objCharacter.LoadDataAsync(DrugsData.CatalogFileName, token: token).ConfigureAwait(false);
            XmlNode objDrugNode = objDoc.TryGetNodeById(DrugsData.CatalogDrugXPath, objDrug.SourceID);
            if (objDrugNode == null)
            {
                objDoc = await objCharacter.LoadDataAsync(DrugsData.ComponentsFileName, token: token)
                    .ConfigureAwait(false);
                objDrugNode = objDoc.TryGetNodeById(DrugsData.CatalogDrugXPath, objDrug.SourceID);
            }
            return objDrugNode?["bonus"];
        }

        /// <summary>
        /// Merges custom drug component effects into a synthetic &lt;bonus&gt; node.
        /// Narco is not applied here; use bioware <c>drugpositiveattributemodifier</c> instead.
        /// </summary>
        /// <param name="objCharacter">Character owning the drug.</param>
        /// <param name="objDrug">Custom drug with components.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A bonus node, or null when there are no effects.</returns>
        public static async Task<XmlNode> BuildCustomBonusNodeAsync(Character objCharacter, Drug objDrug,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            XmlDocument objDoc = new XmlDocument();
            XmlNode nodBonus = objDoc.CreateElement("bonus");

            foreach (KeyValuePair<string, decimal> kvpAttribute in await objDrug.GetAttributesAsync(token)
                         .ConfigureAwait(false))
            {
                if (kvpAttribute.Value == 0)
                    continue;
                XmlElement nodAttr = objDoc.CreateElement("specificattribute");
                nodAttr.AppendChild(objDoc.CreateElement("name")).InnerText = kvpAttribute.Key;
                nodAttr.AppendChild(objDoc.CreateElement("val")).InnerText =
                    kvpAttribute.Value.ToString(GlobalSettings.InvariantCultureInfo);
                nodBonus.AppendChild(nodAttr);
            }

            foreach (KeyValuePair<string, int> kvpLimit in await objDrug.GetLimitsAsync(token).ConfigureAwait(false))
            {
                if (kvpLimit.Value == 0)
                    continue;
                string strLimitElement;
                switch (kvpLimit.Key.ToUpperInvariant())
                {
                    case "PHYSICAL":
                        strLimitElement = "physicallimit";
                        break;
                    case "MENTAL":
                        strLimitElement = "mentallimit";
                        break;
                    case "SOCIAL":
                        strLimitElement = "sociallimit";
                        break;
                    default:
                        continue;
                }
                XmlElement nodLimit = objDoc.CreateElement(strLimitElement);
                nodLimit.InnerText = kvpLimit.Value.ToString(GlobalSettings.InvariantCultureInfo);
                nodBonus.AppendChild(nodLimit);
            }

            int intInitiative = await objDrug.GetInitiativeAsync(token).ConfigureAwait(false);
            if (intInitiative != 0)
            {
                XmlNode nodIni = objDoc.CreateElement("initiative");
                nodIni.InnerText = intInitiative.ToString(GlobalSettings.InvariantCultureInfo);
                nodBonus.AppendChild(nodIni);
            }

            int intInitiativeDice = await objDrug.GetInitiativeDiceAsync(token).ConfigureAwait(false);
            if (intInitiativeDice != 0)
            {
                XmlNode nodDice = objDoc.CreateElement("initiativedice");
                nodDice.InnerText = intInitiativeDice.ToString(GlobalSettings.InvariantCultureInfo);
                nodBonus.AppendChild(nodDice);
            }

            foreach (XmlNode objXmlQuality in await objDrug.GetQualitiesAsync(token).ConfigureAwait(false))
            {
                string strQualityName = objXmlQuality?.InnerText?.Trim();
                if (string.IsNullOrEmpty(strQualityName))
                    continue;
                XmlElement nodQuality = objDoc.CreateElement("quality");
                nodQuality.InnerText = strQualityName;
                string strRatingAttr = objXmlQuality.Attributes?["rating"]?.InnerText?.Trim();
                nodQuality.SetAttribute("rating", !string.IsNullOrEmpty(strRatingAttr) ? strRatingAttr : "1");
                string strSelect = objXmlQuality.Attributes?["select"]?.InnerText?.Trim();
                if (!string.IsNullOrEmpty(strSelect))
                    nodQuality.SetAttribute("select", strSelect);
                if (objXmlQuality.Attributes?["forced"]?.InnerTextIsTrueString() == true)
                    nodQuality.SetAttribute("forced", bool.TrueString);
                nodBonus.AppendChild(nodQuality);
            }

            await objDrug.Components.ForEachAsync(objComponent =>
            {
                DrugEffect objEffect = objComponent.ActiveDrugEffect;
                if (objEffect == null)
                    return;
                foreach (XmlNode nodSkill in objEffect.SpecificSkills)
                {
                    if (nodSkill == null)
                        continue;
                    nodBonus.AppendChild(objDoc.ImportNode(nodSkill, true));
                }
            }, token).ConfigureAwait(false);

            return nodBonus.ChildNodes.Count > 0 ? nodBonus : null;
        }

        /// <summary>
        /// Converts drug-catalog bonus nodes to the vocabulary expected by <see cref="ImprovementManager"/>.
        /// </summary>
        /// <param name="nodBonus">Source bonus node (may use &lt;attribute&gt; / &lt;limit&gt;).</param>
        /// <returns>Normalized bonus node in a standalone document.</returns>
        public static XmlNode NormalizeBonusNode(XmlNode nodBonus)
        {
            if (nodBonus == null)
                return null;

            XmlDocument objDoc = new XmlDocument();
            XmlElement nodRoot = objDoc.CreateElement("bonus");

            foreach (XmlNode nodChild in nodBonus.ChildNodes)
            {
                if (nodChild.NodeType != XmlNodeType.Element)
                    continue;

                switch (nodChild.Name.ToUpperInvariant())
                {
                    case "ATTRIBUTE":
                    {
                        string strName = nodChild["name"]?.InnerText ?? string.Empty;
                        string strValue = nodChild["value"]?.InnerText ?? nodChild["val"]?.InnerText ?? string.Empty;
                        if (string.IsNullOrEmpty(strName))
                            break;
                        XmlElement nodAttr = objDoc.CreateElement("specificattribute");
                        nodAttr.AppendChild(objDoc.CreateElement("name")).InnerText = strName;
                        nodAttr.AppendChild(objDoc.CreateElement("val")).InnerText = strValue;
                        nodRoot.AppendChild(nodAttr);
                        break;
                    }
                    case "SPECIFICATTRIBUTE":
                        nodRoot.AppendChild(objDoc.ImportNode(nodChild, true));
                        break;
                    case "LIMIT":
                    {
                        string strLimitName = nodChild["name"]?.InnerText ?? string.Empty;
                        string strValue = nodChild["value"]?.InnerText ?? nodChild.InnerText;
                        string strLimitElement = null;
                        switch (strLimitName.ToUpperInvariant())
                        {
                            case "PHYSICAL":
                                strLimitElement = "physicallimit";
                                break;
                            case "MENTAL":
                                strLimitElement = "mentallimit";
                                break;
                            case "SOCIAL":
                                strLimitElement = "sociallimit";
                                break;
                        }
                        if (strLimitElement == null)
                            break;
                        XmlElement nodLimit = objDoc.CreateElement(strLimitElement);
                        nodLimit.InnerText = strValue;
                        nodRoot.AppendChild(nodLimit);
                        break;
                    }
                    case "PHYSICALLIMIT":
                    case "MENTALLIMIT":
                    case "SOCIALLIMIT":
                    case "INITIATIVE":
                    case "INITIATIVEDICE":
                    case "QUALITY":
                        nodRoot.AppendChild(objDoc.ImportNode(nodChild, true));
                        break;
                    default:
                        nodRoot.AppendChild(objDoc.ImportNode(nodChild, true));
                        break;
                }
            }

            return nodRoot.ChildNodes.Count > 0 ? nodRoot : null;
        }

        private static CompileResult SplitQualityNodes(XmlNode nodBonus)
        {
            List<XmlNode> lstQualities = new List<XmlNode>(2);
            XmlDocument objDoc = new XmlDocument();
            XmlNode nodCopy = objDoc.ImportNode(nodBonus, true);
            XmlNode nodBonusRoot = nodCopy.Name.Equals("bonus", StringComparison.OrdinalIgnoreCase)
                ? nodCopy
                : nodCopy.AppendChild(objDoc.CreateElement("bonus"));

            List<XmlNode> lstToRemove = new List<XmlNode>(2);
            foreach (XmlNode nodChild in nodBonusRoot.ChildNodes)
            {
                if (nodChild.NodeType != XmlNodeType.Element)
                    continue;
                if (!nodChild.Name.Equals("quality", StringComparison.OrdinalIgnoreCase))
                    continue;
                lstQualities.Add(nodChild);
                lstToRemove.Add(nodChild);
            }

            foreach (XmlNode nodRemove in lstToRemove)
                nodBonusRoot.RemoveChild(nodRemove);

            return new CompileResult
            {
                BonusNode = nodBonusRoot.ChildNodes.Count > 0 ? nodBonusRoot : null,
                QualityNodes = lstQualities
            };
        }
    }
}
