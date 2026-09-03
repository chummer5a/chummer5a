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
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class ImprovementConditionTests
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            Utils.IsUnitTest = true;
            Utils.IsUnitTestForUI = false;
            Utils.CreateSynchronizationContext();
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void EvaluateCondition_CharacterCreated_FollowsCareerMode()
        {
            try
            {
                using (Character objCharacter = new Character())
                {
                    Assert.IsFalse(ImprovementManager.EvaluateCondition("/character/created", objCharacter, token: TestContext.CancellationToken));
                    Assert.IsTrue(ImprovementManager.EvaluateCondition("/character/created = false", objCharacter, token: TestContext.CancellationToken));
                    Assert.IsTrue(ImprovementManager.EvaluateCondition("create", objCharacter, token: TestContext.CancellationToken));
                    Assert.IsFalse(ImprovementManager.EvaluateCondition("career", objCharacter, token: TestContext.CancellationToken));

                    objCharacter.SetCreated(true, false);

                    Assert.IsTrue(ImprovementManager.EvaluateCondition("/character/created", objCharacter, token: TestContext.CancellationToken));
                    Assert.IsFalse(ImprovementManager.EvaluateCondition("/character/created = false", objCharacter, token: TestContext.CancellationToken));
                    Assert.IsFalse(ImprovementManager.EvaluateCondition("create", objCharacter, token: TestContext.CancellationToken));
                    Assert.IsTrue(ImprovementManager.EvaluateCondition("career", objCharacter, token: TestContext.CancellationToken));
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
        public void EvaluateCondition_SpellAlchemical_ExcludesPreparations()
        {
            try
            {
                using (Character objCharacter = new Character())
                {
                    using (Spell objSpell = new Spell(objCharacter))
                    {
                        Assert.IsTrue(ImprovementManager.EvaluateCondition("not(/spell/alchemical)", objSpell, token: TestContext.CancellationToken));
                        Assert.IsFalse(ImprovementManager.EvaluateCondition("/spell/alchemical", objSpell, token: TestContext.CancellationToken));

                        objSpell.Alchemical = true;

                        Assert.IsFalse(ImprovementManager.EvaluateCondition("not(/spell/alchemical)", objSpell, token: TestContext.CancellationToken));
                        Assert.IsTrue(ImprovementManager.EvaluateCondition("/spell/alchemical", objSpell, token: TestContext.CancellationToken));
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
        public void ValueCache_CharacterCreatedConditions_ApplyOnlyInMatchingMode()
        {
            try
            {
                using (Character objCreate = new Character())
                {
                    Improvement objCareerOnly = AddAcademicKarmaCost(objCreate, "/character/created", token: TestContext.CancellationToken);
                    Improvement objCreateOnly = AddAcademicKarmaMultiplier(objCreate, "/character/created = false", token: TestContext.CancellationToken);

                    Assert.DoesNotContain(objCareerOnly, GetAcademicCosts(objCreate, token: TestContext.CancellationToken));
                    Assert.Contains(objCreateOnly, GetAcademicMultipliers(objCreate, token: TestContext.CancellationToken));
                }

                using (Character objCareer = new Character())
                {
                    objCareer.SetCreated(true, false);
                    Improvement objCareerOnly = AddAcademicKarmaCost(objCareer, "/character/created", token: TestContext.CancellationToken);
                    Improvement objCreateOnly = AddAcademicKarmaMultiplier(objCareer, "/character/created = false", token: TestContext.CancellationToken);

                    Assert.Contains(objCareerOnly, GetAcademicCosts(objCareer, token: TestContext.CancellationToken));
                    Assert.DoesNotContain(objCreateOnly, GetAcademicMultipliers(objCareer, token: TestContext.CancellationToken));
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
        public void ValueCache_DrugPositiveAttributeModifier_AddsOnePerPositiveDrugAttribute()
        {
            try
            {
                using (Character objCharacter = new Character())
                {
                    ImprovementManager.CreateImprovement(
                        objCharacter, "STR", Improvement.ImprovementSource.Drug, "jazz",
                        Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, 2, token: TestContext.CancellationToken);
                    ImprovementManager.CreateImprovement(
                        objCharacter, "LOG", Improvement.ImprovementSource.Drug, "jazz",
                        Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -1, token: TestContext.CancellationToken);
                    ImprovementManager.CreateImprovement(
                        objCharacter, "AGI", Improvement.ImprovementSource.Cyberware, "muscle toner",
                        Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, 2, token: TestContext.CancellationToken);
                    ImprovementManager.CreateImprovement(
                        objCharacter, string.Empty, Improvement.ImprovementSource.Bioware, "narco",
                        Improvement.ImprovementType.DrugPositiveAttributeModifier, string.Empty, 1, token: TestContext.CancellationToken);

                    Assert.AreEqual(3, ImprovementManager.AugmentedValueOf(
                        objCharacter, Improvement.ImprovementType.Attribute, strImprovedName: "STR", token: TestContext.CancellationToken));
                    Assert.AreEqual(-1, ImprovementManager.AugmentedValueOf(
                        objCharacter, Improvement.ImprovementType.Attribute, strImprovedName: "LOG", token: TestContext.CancellationToken));
                    Assert.AreEqual(2, ImprovementManager.AugmentedValueOf(
                        objCharacter, Improvement.ImprovementType.Attribute, strImprovedName: "AGI", token: TestContext.CancellationToken));
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
        public void ValueCache_DrugPositiveAttributeModifier_StacksPerDrug()
        {
            try
            {
                using (Character objCharacter = new Character())
                {
                    ImprovementManager.CreateImprovement(
                        objCharacter, "STR", Improvement.ImprovementSource.Drug, "kamikaze",
                        Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, 2, token: TestContext.CancellationToken);
                    ImprovementManager.CreateImprovement(
                        objCharacter, "STR", Improvement.ImprovementSource.Drug, "cram",
                        Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, 1, token: TestContext.CancellationToken);
                    ImprovementManager.CreateImprovement(
                        objCharacter, string.Empty, Improvement.ImprovementSource.Bioware, "narco",
                        Improvement.ImprovementType.DrugPositiveAttributeModifier, string.Empty, 1, token: TestContext.CancellationToken);

                    Assert.AreEqual(5, ImprovementManager.AugmentedValueOf(
                        objCharacter, Improvement.ImprovementType.Attribute, strImprovedName: "STR", token: TestContext.CancellationToken));
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

        private static Improvement AddAcademicKarmaCost(Character objCharacter, string strCondition, CancellationToken token = default)
        {
            return ImprovementManager.CreateImprovement(
                objCharacter, "Academic", Improvement.ImprovementSource.Quality, "test",
                Improvement.ImprovementType.SkillCategoryKarmaCost, string.Empty, -1, 1, 3,
                strCondition: strCondition, token: token);
        }

        private static Improvement AddAcademicKarmaMultiplier(Character objCharacter, string strCondition, CancellationToken token = default)
        {
            return ImprovementManager.CreateImprovement(
                objCharacter, "Academic", Improvement.ImprovementSource.Quality, "test",
                Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier, string.Empty, 50,
                strCondition: strCondition, token: token);
        }

        private static List<Improvement> GetAcademicCosts(Character objCharacter, CancellationToken token = default)
        {
            return ImprovementManager.GetCachedImprovementListForValueOf(
                objCharacter, Improvement.ImprovementType.SkillCategoryKarmaCost, "Academic", token: token);
        }

        private static List<Improvement> GetAcademicMultipliers(Character objCharacter, CancellationToken token = default)
        {
            return ImprovementManager.GetCachedImprovementListForValueOf(
                objCharacter, Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier, "Academic", token: token);
        }
    }
}
