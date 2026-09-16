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
using System.Threading;
using System.Xml;
using Chummer.Backend.Equipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class CyberwareDrugChildrenTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ProcessCostExpression_GearCostIncludesNestedDrugTotalCost()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                using (Character objCharacter = new Character())
                {
                    Cyberware objGland = new Cyberware(objCharacter)
                    {
                        Cost = "20000 + (99 * Gear Cost)"
                    };

                    try
                    {
                        Drug objDrug = new Drug(objCharacter);
                        try
                        {
                            XmlDocument xmlDoc = new XmlDocument { XmlResolver = null };
                            xmlDoc.LoadXml(
                                "<drug><name>Jazz</name><category>Drugs</category><cost>150</cost><quantity>1</quantity><availability>2</availability></drug>");
                            objDrug.Load(xmlDoc.DocumentElement, token);
                            objGland.DrugChildren.Add(objDrug);

                            string strCost = objGland.ProcessCostExpression(objGland.Cost, () => 1, () => objGland.Grade);
                            Assert.AreEqual("34850", strCost);
                        }
                        finally
                        {
                            objDrug.Remove(false);
                        }
                    }
                    finally
                    {
                        objGland.Remove(false);
                    }
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
                throw;
            }
#if MEMORYTESTING
            finally
            {
                TestContext.CancellationTokenSource.Dispose();
            }
#endif
        }

        [TestMethod]
        public void ProcessCostExpression_ParentGearCostIncludesParentNestedDrugs()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                using (Character objCharacter = new Character())
                {
                    Cyberware objParent = new Cyberware(objCharacter)
                    {
                        Cost = "0"
                    };
                    try
                    {
                        Cyberware objExpanded = new Cyberware(objCharacter)
                        {
                            Cost = "2000 + (4 * Parent Gear Cost)",
                            Parent = objParent
                        };
                        try
                        {
                            Drug objDrug = new Drug(objCharacter);
                            try
                            {
                                XmlDocument xmlDoc = new XmlDocument { XmlResolver = null };
                                xmlDoc.LoadXml(
                                    "<drug><name>Jazz</name><category>Drugs</category><cost>100</cost><quantity>1</quantity><availability>2</availability></drug>");
                                objDrug.Load(xmlDoc.DocumentElement, token);
                                objParent.DrugChildren.Add(objDrug);

                                string strCost = objExpanded.ProcessCostExpression(objExpanded.Cost, () => 1, () => objExpanded.Grade);
                                Assert.AreEqual("2400", strCost);
                            }
                            finally
                            {
                                objDrug.Remove(false);
                            }
                        }
                        finally
                        {
                            objExpanded.Remove(false);
                        }
                    }
                    finally
                    {
                        objParent.Remove(false);
                    }
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
                throw;
            }
#if MEMORYTESTING
            finally
            {
                TestContext.CancellationTokenSource.Dispose();
            }
#endif
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
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                using (Character objCharacter = new Character())
                {
                    Cyberware objGland = new Cyberware(objCharacter);
                    try
                    {
                        Drug objDrug = new Drug(objCharacter);
                        try
                        {
                            XmlDocument xmlDoc = new XmlDocument { XmlResolver = null };
                            xmlDoc.LoadXml(
                                "<drug><name>Jazz</name><category>Drugs</category><cost>150</cost><quantity>1</quantity><availability>2</availability></drug>");
                            objDrug.Load(xmlDoc.DocumentElement, token);
                            objGland.DrugChildren.Add(objDrug);
                            objCharacter.Cyberware.Add(objGland);

                            Assert.AreEqual("Drugs", Drug.GetCategoryForDrugSource(objCharacter, objDrug.InternalId));
                            Assert.AreSame(objDrug, Drug.FindNestedDrug(objCharacter, objDrug.InternalId));
                        }
                        finally
                        {
                            objDrug.Remove(false);
                        }
                    }
                    finally
                    {
                        objGland.Remove(false);
                    }
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
                throw;
            }
#if MEMORYTESTING
            finally
            {
                TestContext.CancellationTokenSource.Dispose();
            }
#endif
        }

        [TestMethod]
        public void GetImprovementGroupName_NestedDrugDiffersFromInventoryDose()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                using (Character objCharacter = new Character())
                {
                    Drug objInventory = new Drug(objCharacter);
                    try
                    {
                        XmlDocument xmlDoc = new XmlDocument { XmlResolver = null };
                        xmlDoc.LoadXml(
                            "<drug><name>Bliss</name><category>Drugs</category><cost>15</cost><quantity>1</quantity><availability>2R</availability></drug>");
                        objInventory.Load(xmlDoc.DocumentElement, token);

                        Cyberware objGland = new Cyberware(objCharacter) { Name = "Chemical Gland" };
                        try
                        {
                            Drug objGlandDrug = new Drug(objCharacter);
                            try
                            {
                                objGlandDrug.Load(xmlDoc.DocumentElement, token);
                                objGland.DrugChildren.Add(objGlandDrug);

                                Assert.AreEqual("Bliss", objInventory.GetImprovementGroupName());
                                Assert.AreEqual("Bliss (Chemical Gland)", objGlandDrug.GetImprovementGroupName());
                            }
                            finally
                            {
                                objGlandDrug.Remove(false);
                            }
                        }
                        finally
                        {
                            objGland.Remove(false);
                        }
                    }
                    finally
                    {
                        objInventory.Remove(false);
                    }
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
                throw;
            }
#if MEMORYTESTING
            finally
            {
                TestContext.CancellationTokenSource.Dispose();
            }
#endif
        }
    }
}
