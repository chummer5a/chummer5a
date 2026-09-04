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
 *  along with Chummer5a.  If not, see <https://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chummer.Backend.Equipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class DrugCatalogImprovementTests
    {
        public TestContext TestContext { get; set; }
        private const string BlissCatalogId = "62bffcd7-4a43-45cd-9cf7-3067ba9688f6";
        private const string DownerBtlCatalogId = "08712808-0ef9-405f-a515-a84c27b7c4d4";
        private const string InfiltratorBtlCatalogId = "c153d7ec-af89-4c6f-a31d-fc3a6a490de4";

        [TestMethod]
        public async Task CatalogDrug_Bliss_CreatesDisabledReaImprovement()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                using (Character objCharacter = new Character())
                {
                    Drug objDrug = await Drug.CreateFromCatalogAsync(objCharacter, BlissCatalogId, null, token: token)
                        .ConfigureAwait(false);
                    Assert.IsNotNull(objDrug);
                    await objCharacter.Drugs.AddAsync(objDrug, token).ConfigureAwait(false);
                    try
                    {
                        await objDrug.GenerateImprovement(token).ConfigureAwait(false);

                        Improvement objRea = objCharacter.Improvements.FirstOrDefault(x =>
                            x.SourceName == objDrug.InternalId
                            && x.ImproveType == Improvement.ImprovementType.Attribute
                            && x.ImprovedName == "REA");
                        Assert.IsNotNull(objRea, "Bliss should create a REA attribute improvement");
                        Assert.IsFalse(objRea.Enabled, "Drug improvements should start disabled");
                        Assert.AreEqual(1m, objRea.Augmented);
                        Assert.IsTrue(objRea.Custom);
                        Assert.AreEqual("Bliss", objRea.CustomGroup);
                        Assert.IsFalse(string.IsNullOrEmpty(objRea.CustomName),
                            "Attribute improvements need a CustomName for the Improvements tab");
                        Assert.IsTrue(await objCharacter.ImprovementGroups.ContainsAsync("Bliss", token).ConfigureAwait(false));
                        Assert.IsTrue(
                            objCharacter.Improvements.Where(x => x.SourceName == objDrug.InternalId)
                                .All(x => x.CustomGroup == "Bliss" && !string.IsNullOrEmpty(x.CustomName)));
                    }
                    finally
                    {
                        await objDrug.RemoveAsync(false, token).ConfigureAwait(false);
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
        public async Task CatalogDrug_Bliss_QualityStubDisabledUntilEnable()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                using (Character objCharacter = new Character())
                {
                    Drug objDrug = await Drug.CreateFromCatalogAsync(objCharacter, BlissCatalogId, null, token: token)
                        .ConfigureAwait(false);
                    await objCharacter.Drugs.AddAsync(objDrug, token).ConfigureAwait(false);
                    try
                    {
                        await objDrug.GenerateImprovement(token).ConfigureAwait(false);

                        Assert.IsFalse(await objCharacter.Qualities.AnyAsync(x => x.Name == "High Pain Tolerance", token).ConfigureAwait(false));
                        Assert.IsTrue(await objCharacter.Improvements.AnyAsync(x =>
                            x.ImproveType == Improvement.ImprovementType.SpecificQuality
                            && x.SourceName == objDrug.InternalId
                            && x.ImprovedName == "High Pain Tolerance"
                            && !x.Enabled, token).ConfigureAwait(false));
                    }
                    finally
                    {
                        await objDrug.RemoveAsync(false, token).ConfigureAwait(false);
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
        public async Task CatalogDrug_Bliss_UsesCatalogCostWithoutComponents()
        {
            try
            {
                using (Character objCharacter = new Character())
                {
                    Drug objDrug = await Drug.CreateFromCatalogAsync(objCharacter, BlissCatalogId, null, token: TestContext.CancellationToken)
                        .ConfigureAwait(false);
                    Assert.IsNotNull(objDrug);
                    Assert.AreEqual(0, objDrug.Components.Count);
                    Assert.AreEqual(15m, objDrug.Cost);
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
        public async Task CatalogDrug_BlackMarketDiscount_ReducesCostWithoutMarkingStolen()
        {
            try
            {
                using (Character objCharacter = new Character())
                {
                    Drug objDrug = await Drug.CreateFromCatalogAsync(objCharacter, BlissCatalogId, null, token: TestContext.CancellationToken)
                        .ConfigureAwait(false);
                    Assert.IsNotNull(objDrug);
                    Assert.IsFalse(objDrug.Stolen);
                    Assert.AreEqual(15m, objDrug.Cost);

                    objDrug.DiscountCost = true;

                    Assert.AreEqual(13.5m, objDrug.Cost);
                    Assert.IsFalse(objDrug.Stolen);
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
        public async Task CatalogDrug_BtlDowner_UsesCatalogBonus()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                using (Character objCharacter = new Character())
                {
                    Drug objDrug = await Drug.CreateFromCatalogAsync(objCharacter, DownerBtlCatalogId, null, token: token)
                        .ConfigureAwait(false);
                    Assert.IsNotNull(objDrug);
                    await objCharacter.Drugs.AddAsync(objDrug, token).ConfigureAwait(false);
                    try
                    {
                        await objDrug.GenerateImprovement(token).ConfigureAwait(false);

                        Improvement objInt = objCharacter.Improvements.FirstOrDefault(x =>
                            x.SourceName == objDrug.InternalId
                            && x.ImproveType == Improvement.ImprovementType.Attribute
                            && x.ImprovedName == "INT");
                        Assert.IsNotNull(objInt);
                        Assert.AreEqual(1m, objInt.Augmented);
                        Assert.IsFalse(objInt.Enabled);
                        Assert.AreEqual(0, objDrug.Components.Count, "BTL effects come from catalog bonus, not components");
                    }
                    finally
                    {
                        await objDrug.RemoveAsync(false, token).ConfigureAwait(false);
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
        public async Task CatalogDrug_Infiltrator_CreatesSpecificSkillImprovements()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                using (Character objCharacter = new Character())
                {
                    Drug objDrug = await Drug.CreateFromCatalogAsync(objCharacter, InfiltratorBtlCatalogId, null, token: token)
                        .ConfigureAwait(false);
                    Assert.IsNotNull(objDrug);
                    await objCharacter.Drugs.AddAsync(objDrug, token).ConfigureAwait(false);
                    try
                    {
                        await objDrug.GenerateImprovement(token).ConfigureAwait(false);

                        Improvement objGymnastics = objCharacter.Improvements.FirstOrDefault(x =>
                            x.SourceName == objDrug.InternalId
                            && x.ImproveType == Improvement.ImprovementType.Skill
                            && x.ImprovedName == "Gymnastics");
                        Assert.IsNotNull(objGymnastics, "Infiltrator should grant Gymnastics");
                        Assert.AreEqual(2m, objGymnastics.Value);
                        Assert.IsFalse(objGymnastics.Enabled);
                    }
                    finally
                    {
                        await objDrug.RemoveAsync(false, token).ConfigureAwait(false);
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
        public async Task EnableCatalogDrug_Cram_InitiativeDiceCustomUniqueValueOf()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                using (Character objCharacter = new Character())
                {
                    const string CramCatalogId = "8dc829b9-8b94-4510-ab1a-2305cbad69c9";
                    Drug objDrug = await Drug.CreateFromCatalogAsync(objCharacter, CramCatalogId, null, token: token)
                        .ConfigureAwait(false);
                    Assert.IsNotNull(objDrug);
                    await objCharacter.Drugs.AddAsync(objDrug, token).ConfigureAwait(false);
                    try
                    {
                        await objDrug.GenerateImprovement(token).ConfigureAwait(false);
                        List<Improvement> lstDrugImprovements = await objCharacter.Improvements
                            .ToListAsync(x => x.SourceName == objDrug.InternalId, token).ConfigureAwait(false);
                        Assert.Contains(x => x.ImproveType == Improvement.ImprovementType.InitiativeDice && x.Custom, lstDrugImprovements);

                        await ImprovementManager.EnableImprovementsAsync(objCharacter, lstDrugImprovements, token)
                            .ConfigureAwait(false);

                        // Custom + UniqueName InitiativeDice used to cache a non-zero value with an empty
                        // used-improvements list (MetaValueOf looked up the wrong dictionary).
                        decimal decDice = ImprovementManager.ValueOf(
                            objCharacter, Improvement.ImprovementType.InitiativeDice, out List<Improvement> lstUsed, token: token);
                        Assert.AreEqual(1m, decDice);
                        Assert.IsGreaterThan(0, lstUsed.Count);
                    }
                    finally
                    {
                        await objDrug.RemoveAsync(false, token).ConfigureAwait(false);
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
        public async Task EnableCatalogDrug_AppliesAttributeBonus()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                using (Character objCharacter = new Character())
                {
                    token.ThrowIfCancellationRequested();
                    Drug objDrug = await Drug.CreateFromCatalogAsync(objCharacter, BlissCatalogId, null, token: token)
                        .ConfigureAwait(false);
                    await objCharacter.Drugs.AddAsync(objDrug, token).ConfigureAwait(false);
                    try
                    {
                        await objDrug.GenerateImprovement(token).ConfigureAwait(false);
                        List<Improvement> lstDrugImprovements = await objCharacter.Improvements
                            .ToListAsync(x => x.SourceName == objDrug.InternalId, token).ConfigureAwait(false);

                        decimal decReaBefore = await objCharacter.REA.GetTotalValueAsync(token).ConfigureAwait(false);
                        await ImprovementManager.EnableImprovementsAsync(objCharacter, lstDrugImprovements, token)
                            .ConfigureAwait(false);
                        Assert.AreEqual(decReaBefore + 1m, await objCharacter.REA.GetTotalValueAsync(token).ConfigureAwait(false));

                        await ImprovementManager.DisableImprovementsAsync(objCharacter, lstDrugImprovements, token)
                            .ConfigureAwait(false);
                        Assert.AreEqual(decReaBefore, await objCharacter.REA.GetTotalValueAsync(token).ConfigureAwait(false));
                    }
                    finally
                    {
                        await objDrug.RemoveAsync(false, token).ConfigureAwait(false);
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
