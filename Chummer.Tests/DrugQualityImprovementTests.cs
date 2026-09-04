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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Chummer.Backend.Equipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class DrugQualityImprovementTests
    {
        private const string GrantedQualityName = "Toughness";
        private const string DrugName = "Test Stim";

        [TestMethod]
        public async Task GenerateImprovement_DoesNotAddQualityWhileDisabled()
        {
            using (Character objCharacter = new Character())
            {
                Drug objDrug = CreateTestDrug(objCharacter);
                try
                {
                    await objDrug.GenerateImprovement().ConfigureAwait(false);

                    Assert.IsFalse(objCharacter.Qualities.Any(x => x.Name == GrantedQualityName),
                        "Drug qualities should not appear until the custom group is enabled");
                    Assert.IsTrue(objCharacter.Improvements.Any(x =>
                        x.ImproveType == Improvement.ImprovementType.SpecificQuality
                        && x.SourceName == objDrug.InternalId
                        && !x.Enabled));
                }
                finally
                {
                    await objDrug.RemoveAsync(false).ConfigureAwait(false);
                }
            }
        }

        [TestMethod]
        public async Task EnableThenDisable_AddsAndRemovesGrantedQuality()
        {
            using (Character objCharacter = new Character())
            {
                Drug objDrug = CreateTestDrug(objCharacter);
                try
                {
                    await objDrug.GenerateImprovement().ConfigureAwait(false);
                    List<Improvement> lstDrugImprovements = await objCharacter.Improvements
                        .ToListAsync(x => x.SourceName == objDrug.InternalId).ConfigureAwait(false);

                    await ImprovementManager.EnableImprovementsAsync(objCharacter, lstDrugImprovements)
                        .ConfigureAwait(false);
                    Assert.IsTrue(objCharacter.Qualities.Any(x => x.Name == GrantedQualityName
                                                                  && x.OriginSource == QualitySource.Improvement),
                        "Enabling the drug should grant " + GrantedQualityName);

                    await ImprovementManager.DisableImprovementsAsync(objCharacter, lstDrugImprovements)
                        .ConfigureAwait(false);
                    Assert.IsFalse(objCharacter.Qualities.Any(x => x.Name == GrantedQualityName),
                        "Disabling the drug should remove the granted quality");
                }
                finally
                {
                    await objDrug.RemoveAsync(false).ConfigureAwait(false);
                }
            }
        }

        [TestMethod]
        public async Task RemoveDrug_DeletesGrantedQuality()
        {
            using (Character objCharacter = new Character())
            {
                Drug objDrug = CreateTestDrug(objCharacter);
                await objDrug.GenerateImprovement().ConfigureAwait(false);
                List<Improvement> lstDrugImprovements = await objCharacter.Improvements
                    .ToListAsync(x => x.SourceName == objDrug.InternalId).ConfigureAwait(false);
                await ImprovementManager.EnableImprovementsAsync(objCharacter, lstDrugImprovements)
                    .ConfigureAwait(false);
                Assert.IsTrue(objCharacter.Qualities.Any(x => x.Name == GrantedQualityName));

                Assert.IsTrue(await objDrug.RemoveAsync(false).ConfigureAwait(false));

                Assert.IsFalse(objCharacter.Qualities.Any(x => x.Name == GrantedQualityName),
                    "Deleting the drug should remove the granted quality");
                Assert.IsFalse(objCharacter.Improvements.Any(x =>
                    x.ImproveSource == Improvement.ImprovementSource.Drug
                    && x.CustomGroup == DrugName));
            }
        }

        [TestMethod]
        public async Task RatingAttribute_CreatesOneStubAndStackedLevels()
        {
            using (Character objCharacter = new Character())
            {
                Drug objDrug = CreateTestDrug(objCharacter, 3);
                try
                {
                    await objDrug.GenerateImprovement().ConfigureAwait(false);
                    List<Improvement> lstQualityImprovements = await objCharacter.Improvements
                        .ToListAsync(x => x.ImproveType == Improvement.ImprovementType.SpecificQuality
                                          && x.SourceName == objDrug.InternalId).ConfigureAwait(false);
                    Assert.AreEqual(1, lstQualityImprovements.Count,
                        "rating=N should be one SpecificQuality stub, not N stubs");
                    Assert.AreEqual(3, lstQualityImprovements[0].Rating);

                    await ImprovementManager.EnableImprovementsAsync(objCharacter,
                            await objCharacter.Improvements.ToListAsync(x => x.SourceName == objDrug.InternalId)
                                .ConfigureAwait(false))
                        .ConfigureAwait(false);
                    Assert.AreEqual(3,
                        objCharacter.Qualities.Count(x => x.Name == GrantedQualityName
                                                          && x.OriginSource == QualitySource.Improvement),
                        "Enabling should add Rating stacked quality instances");

                    await ImprovementManager.DisableImprovementsAsync(objCharacter,
                            await objCharacter.Improvements.ToListAsync(x => x.SourceName == objDrug.InternalId)
                                .ConfigureAwait(false))
                        .ConfigureAwait(false);
                    Assert.IsFalse(objCharacter.Qualities.Any(x => x.Name == GrantedQualityName));
                }
                finally
                {
                    await objDrug.RemoveAsync(false).ConfigureAwait(false);
                }
            }
        }

        private static Drug CreateTestDrug(Character objCharacter, int intQualityRating = 1)
        {
            Drug objDrug = new Drug(objCharacter) { Name = DrugName };
            DrugComponent objComponent = new DrugComponent(objCharacter);
            DrugEffect objEffect = new DrugEffect { Level = 0 };
            XmlDocument xmlQuality = new XmlDocument { XmlResolver = null };
            string strRatingAttr = intQualityRating > 1
                ? " rating=\"" + intQualityRating.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\""
                : string.Empty;
            xmlQuality.LoadXml("<quality forced=\"True\"" + strRatingAttr + ">" + GrantedQualityName + "</quality>");
            objEffect.Qualities.Add(xmlQuality.DocumentElement);
            objComponent.DrugEffects.Add(objEffect);
            objDrug.Components.Add(objComponent);
            objCharacter.Drugs.Add(objDrug);
            return objDrug;
        }
    }
}
