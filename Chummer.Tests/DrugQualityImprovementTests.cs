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
using System.Xml;
using Chummer.Backend.Equipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class DrugQualityImprovementTests
    {
        public TestContext TestContext { get; set; }
        private const string GrantedQualityName = "Toughness";
        private const string DrugName = "Test Stim";

        [TestMethod]
        public async Task GenerateImprovement_DoesNotAddQualityWhileDisabled()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                using (Character objCharacter = new Character())
                {
                    Drug objDrug = await CreateTestDrugAsync(objCharacter, token: token).ConfigureAwait(false);
                    try
                    {
                        await objDrug.GenerateImprovement(token).ConfigureAwait(false);

                        Assert.IsFalse(await objCharacter.Qualities.AnyAsync(x => x.Name == GrantedQualityName, token).ConfigureAwait(false),
                            "Drug qualities should not appear until the custom group is enabled");
                        Assert.IsTrue(await objCharacter.Improvements.AnyAsync(x =>
                            x.ImproveType == Improvement.ImprovementType.SpecificQuality
                            && x.SourceName == objDrug.InternalId
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
        public async Task EnableThenDisable_AddsAndRemovesGrantedQuality()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                using (Character objCharacter = new Character())
                {
                    Drug objDrug = await CreateTestDrugAsync(objCharacter, token: token).ConfigureAwait(false);
                    try
                    {
                        await objDrug.GenerateImprovement(token).ConfigureAwait(false);
                        List<Improvement> lstDrugImprovements = await objCharacter.Improvements
                            .ToListAsync(x => x.SourceName == objDrug.InternalId, token).ConfigureAwait(false);

                        await ImprovementManager.EnableImprovementsAsync(objCharacter, lstDrugImprovements, token)
                            .ConfigureAwait(false);
                        Assert.IsTrue(await objCharacter.Qualities.AnyAsync(x => x.Name == GrantedQualityName
                                                                      && x.OriginSource == QualitySource.Improvement, token).ConfigureAwait(false),
                            "Enabling the drug should grant " + GrantedQualityName);

                        await ImprovementManager.DisableImprovementsAsync(objCharacter, lstDrugImprovements, token)
                            .ConfigureAwait(false);
                        Assert.IsFalse(await objCharacter.Qualities.AnyAsync(x => x.Name == GrantedQualityName, token).ConfigureAwait(false),
                            "Disabling the drug should remove the granted quality");
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
        public async Task RemoveDrug_DeletesGrantedQuality()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                using (Character objCharacter = new Character())
                {
                    Drug objDrug = await CreateTestDrugAsync(objCharacter, token: token).ConfigureAwait(false);
                    await objDrug.GenerateImprovement(token).ConfigureAwait(false);
                    List<Improvement> lstDrugImprovements = await objCharacter.Improvements
                        .ToListAsync(x => x.SourceName == objDrug.InternalId, token).ConfigureAwait(false);
                    await ImprovementManager.EnableImprovementsAsync(objCharacter, lstDrugImprovements, token)
                        .ConfigureAwait(false);
                    Assert.IsTrue(await objCharacter.Qualities.AnyAsync(x => x.Name == GrantedQualityName, token).ConfigureAwait(false));

                    Assert.IsTrue(await objDrug.RemoveAsync(false, token).ConfigureAwait(false));

                    Assert.IsFalse(await objCharacter.Qualities.AnyAsync(x => x.Name == GrantedQualityName, token).ConfigureAwait(false),
                        "Deleting the drug should remove the granted quality");
                    Assert.IsFalse(await objCharacter.Improvements.AnyAsync(x =>
                        x.ImproveSource == Improvement.ImprovementSource.Drug
                        && x.CustomGroup == DrugName, token).ConfigureAwait(false));
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
        public async Task RatingAttribute_CreatesOneStubAndStackedLevels()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                using (Character objCharacter = new Character())
                {
                    Drug objDrug = await CreateTestDrugAsync(objCharacter, 3, token).ConfigureAwait(false);
                    try
                    {
                        await objDrug.GenerateImprovement(token).ConfigureAwait(false);
                        List<Improvement> lstQualityImprovements = await objCharacter.Improvements
                            .ToListAsync(x => x.ImproveType == Improvement.ImprovementType.SpecificQuality
                                              && x.SourceName == objDrug.InternalId, token).ConfigureAwait(false);
                        Assert.HasCount(1, lstQualityImprovements,
                            "rating=N should be one SpecificQuality stub, not N stubs");
                        Assert.AreEqual(3, lstQualityImprovements[0].Rating);

                        await ImprovementManager.EnableImprovementsAsync(objCharacter,
                                await objCharacter.Improvements.ToListAsync(x => x.SourceName == objDrug.InternalId, token)
                                    .ConfigureAwait(false), token)
                            .ConfigureAwait(false);
                        Assert.AreEqual(3,
                            await objCharacter.Qualities.CountAsync(x => x.Name == GrantedQualityName
                                                              && x.OriginSource == QualitySource.Improvement, token).ConfigureAwait(false),
                            "Enabling should add Rating stacked quality instances");

                        await ImprovementManager.DisableImprovementsAsync(objCharacter,
                                await objCharacter.Improvements.ToListAsync(x => x.SourceName == objDrug.InternalId, token)
                                    .ConfigureAwait(false), token)
                            .ConfigureAwait(false);
                        Assert.IsFalse(await objCharacter.Qualities.AnyAsync(x => x.Name == GrantedQualityName, token).ConfigureAwait(false));
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

        private static async Task<Drug> CreateTestDrugAsync(Character objCharacter, int intQualityRating = 1, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Drug objDrug = new Drug(objCharacter) { Name = DrugName };
            token.ThrowIfCancellationRequested();
            DrugComponent objComponent = new DrugComponent(objCharacter);
            token.ThrowIfCancellationRequested();
            DrugEffect objEffect = new DrugEffect { Level = 0 };
            token.ThrowIfCancellationRequested();
            XmlDocument xmlQuality = new XmlDocument { XmlResolver = null };
            token.ThrowIfCancellationRequested();
            string strRatingAttr = intQualityRating > 1
                ? " rating=\"" + intQualityRating.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\""
                : string.Empty;
            token.ThrowIfCancellationRequested();
            xmlQuality.LoadXml("<quality forced=\"True\"" + strRatingAttr + ">" + GrantedQualityName + "</quality>");
            token.ThrowIfCancellationRequested();
            objEffect.Qualities.Add(xmlQuality.DocumentElement);
            token.ThrowIfCancellationRequested();
            objComponent.DrugEffects.Add(objEffect);
            await objDrug.Components.AddAsync(objComponent, token).ConfigureAwait(false);
            await objCharacter.Drugs.AddAsync(objDrug, token).ConfigureAwait(false);
            return objDrug;
        }
    }
}
