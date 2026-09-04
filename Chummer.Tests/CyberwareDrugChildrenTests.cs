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

using System.Xml;
using Chummer.Backend.Equipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class CyberwareDrugChildrenTests
    {
        [TestMethod]
        public void ProcessCostExpression_GearCostIncludesNestedDrugTotalCost()
        {
            using (Character objCharacter = new Character())
            {
                Cyberware objGland = new Cyberware(objCharacter)
                {
                    Cost = "20000 + (99 * Gear Cost)"
                };

                Drug objDrug = new Drug(objCharacter);
                XmlDocument xmlDoc = new XmlDocument { XmlResolver = null };
                xmlDoc.LoadXml(
                    "<drug><name>Jazz</name><category>Drugs</category><cost>150</cost><quantity>1</quantity><availability>2</availability></drug>");
                objDrug.Load(xmlDoc.DocumentElement);
                objGland.DrugChildren.Add(objDrug);

                string strCost = objGland.ProcessCostExpression(objGland.Cost, () => 1, () => objGland.Grade);
                Assert.AreEqual("34850", strCost);
            }
        }

        [TestMethod]
        public void ProcessCostExpression_ParentGearCostIncludesParentNestedDrugs()
        {
            using (Character objCharacter = new Character())
            {
                Cyberware objParent = new Cyberware(objCharacter)
                {
                    Cost = "0"
                };
                Cyberware objExpanded = new Cyberware(objCharacter)
                {
                    Cost = "2000 + (4 * Parent Gear Cost)",
                    Parent = objParent
                };

                Drug objDrug = new Drug(objCharacter);
                XmlDocument xmlDoc = new XmlDocument { XmlResolver = null };
                xmlDoc.LoadXml(
                    "<drug><name>Jazz</name><category>Drugs</category><cost>100</cost><quantity>1</quantity><availability>2</availability></drug>");
                objDrug.Load(xmlDoc.DocumentElement);
                objParent.DrugChildren.Add(objDrug);

                string strCost = objExpanded.ProcessCostExpression(objExpanded.Cost, () => 1, () => objExpanded.Grade);
                Assert.AreEqual("2400", strCost);
            }
        }

        [TestMethod]
        public void AllowDrug_CategoryFilter_RejectsDisallowedCategories()
        {
            // Mirrors SelectDrug filtering: an empty allow list means all; a set must contain the drug category.
            Assert.IsTrue(Drug.PositiveAttributeModifierAppliesToCategory("Drugs", "Drugs"));
            Assert.IsFalse(Drug.PositiveAttributeModifierAppliesToCategory("BTLs", "Drugs"));
            Assert.IsTrue(Drug.IsCustomDrugsCategory("Custom Drugs"));
        }

        [TestMethod]
        public void GetCategoryForDrugSource_FindsNestedGlandDrug()
        {
            using (Character objCharacter = new Character())
            {
                Cyberware objGland = new Cyberware(objCharacter);
                Drug objDrug = new Drug(objCharacter);
                XmlDocument xmlDoc = new XmlDocument { XmlResolver = null };
                xmlDoc.LoadXml(
                    "<drug><name>Jazz</name><category>Drugs</category><cost>150</cost><quantity>1</quantity><availability>2</availability></drug>");
                objDrug.Load(xmlDoc.DocumentElement);
                objGland.DrugChildren.Add(objDrug);
                objCharacter.Cyberware.Add(objGland);

                Assert.AreEqual("Drugs", Drug.GetCategoryForDrugSource(objCharacter, objDrug.InternalId));
                Assert.AreSame(objDrug, Drug.FindNestedDrug(objCharacter, objDrug.InternalId));
            }
        }

        [TestMethod]
        public void GetImprovementGroupName_NestedDrugDiffersFromInventoryDose()
        {
            using (Character objCharacter = new Character())
            {
                Drug objInventory = new Drug(objCharacter);
                XmlDocument xmlDoc = new XmlDocument { XmlResolver = null };
                xmlDoc.LoadXml(
                    "<drug><name>Bliss</name><category>Drugs</category><cost>15</cost><quantity>1</quantity><availability>2R</availability></drug>");
                objInventory.Load(xmlDoc.DocumentElement);

                Cyberware objGland = new Cyberware(objCharacter) { Name = "Chemical Gland" };
                Drug objGlandDrug = new Drug(objCharacter);
                objGlandDrug.Load(xmlDoc.DocumentElement);
                objGland.DrugChildren.Add(objGlandDrug);

                Assert.AreEqual("Bliss", objInventory.GetImprovementGroupName());
                Assert.AreEqual("Bliss (Chemical Gland)", objGlandDrug.GetImprovementGroupName());
            }
        }
    }
}
