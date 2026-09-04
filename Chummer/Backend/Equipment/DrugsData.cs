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

using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Data file names and lookup helpers for the drugs system.
    /// </summary>
    public static class DrugsData
    {
        /// <summary>
        /// Catalog file containing premade drugs and drug grades.
        /// </summary>
        public const string CatalogFileName = "drugs.xml";

        /// <summary>
        /// Design components file for custom drug recipes.
        /// </summary>
        public const string ComponentsFileName = "drugcomponents.xml";

        /// <summary>
        /// XPath to a catalog drug node by id.
        /// </summary>
        public const string CatalogDrugXPath = "/chummer/drugs/drug";

        /// <summary>
        /// XPath to a design component node by id.
        /// </summary>
        public const string ComponentXPath = "/chummer/drugcomponents/drugcomponent";

        /// <summary>
        /// Loads a catalog drug node, falling back to legacy <c>drugcomponents.xml</c> amends.
        /// </summary>
        /// <param name="objCharacter">Character used for data loading.</param>
        /// <param name="guiCatalogId">Catalog drug id.</param>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The catalog node, or null when not found.</returns>
        public static XmlNode GetCatalogDrugNode(Character objCharacter, System.Guid guiCatalogId,
            string strLanguage = "", CancellationToken token = default)
        {
            if (objCharacter == null || guiCatalogId == System.Guid.Empty)
                return null;

            XmlDocument objCatalogDoc = objCharacter.LoadData(CatalogFileName, strLanguage, token: token);
            XmlNode objNode = objCatalogDoc.TryGetNodeById(CatalogDrugXPath, guiCatalogId);
            if (objNode != null)
                return objNode;

            XmlDocument objLegacyDoc =
                objCharacter.LoadData(ComponentsFileName, strLanguage, token: token);
            return objLegacyDoc.TryGetNodeById(CatalogDrugXPath, guiCatalogId);
        }

        /// <summary>
        /// Loads a catalog drug node, falling back to legacy <c>drugcomponents.xml</c> amends.
        /// </summary>
        /// <param name="objCharacter">Character used for data loading.</param>
        /// <param name="guiCatalogId">Catalog drug id.</param>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The catalog node, or null when not found.</returns>
        public static async Task<XmlNode> GetCatalogDrugNodeAsync(Character objCharacter, System.Guid guiCatalogId,
            string strLanguage = "", CancellationToken token = default)
        {
            if (objCharacter == null || guiCatalogId == System.Guid.Empty)
                return null;

            XmlDocument objCatalogDoc =
                await objCharacter.LoadDataAsync(CatalogFileName, strLanguage, token: token)
                    .ConfigureAwait(false);
            XmlNode objNode = objCatalogDoc.TryGetNodeById(CatalogDrugXPath, guiCatalogId);
            if (objNode != null)
                return objNode;

            XmlDocument objLegacyDoc =
                await objCharacter.LoadDataAsync(ComponentsFileName, strLanguage, token: token)
                    .ConfigureAwait(false);
            return objLegacyDoc.TryGetNodeById(CatalogDrugXPath, guiCatalogId);
        }

        /// <summary>
        /// Loads a catalog drug node by name or id.
        /// </summary>
        /// <param name="objCharacter">Character used for data loading.</param>
        /// <param name="strNameOrId">Drug name or id.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The catalog node, or null when not found.</returns>
        public static async Task<XmlNode> GetCatalogDrugNodeByNameOrIdAsync(Character objCharacter, string strNameOrId,
            CancellationToken token = default)
        {
            if (objCharacter == null || string.IsNullOrEmpty(strNameOrId))
                return null;

            XmlDocument objCatalogDoc =
                await objCharacter.LoadDataAsync(CatalogFileName, token: token).ConfigureAwait(false);
            XmlNode objNode = objCatalogDoc.TryGetNodeByNameOrId(CatalogDrugXPath, strNameOrId);
            if (objNode != null)
                return objNode;

            XmlDocument objLegacyDoc =
                await objCharacter.LoadDataAsync(ComponentsFileName, token: token).ConfigureAwait(false);
            return objLegacyDoc.TryGetNodeByNameOrId(CatalogDrugXPath, strNameOrId);
        }
    }
}
