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

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class ImprovementConditionTests
    {
        [TestMethod]
        public void EvaluateCondition_CharacterCreated_FollowsCareerMode()
        {
            using (Character objCharacter = new Character())
            {
                Assert.IsFalse(ImprovementManager.EvaluateCondition("/character/created", objCharacter));
                Assert.IsTrue(ImprovementManager.EvaluateCondition("/character/created = false", objCharacter));
                Assert.IsTrue(ImprovementManager.EvaluateCondition("create", objCharacter));
                Assert.IsFalse(ImprovementManager.EvaluateCondition("career", objCharacter));

                objCharacter.SetCreated(true, false);

                Assert.IsTrue(ImprovementManager.EvaluateCondition("/character/created", objCharacter));
                Assert.IsFalse(ImprovementManager.EvaluateCondition("/character/created = false", objCharacter));
                Assert.IsFalse(ImprovementManager.EvaluateCondition("create", objCharacter));
                Assert.IsTrue(ImprovementManager.EvaluateCondition("career", objCharacter));
            }
        }

        [TestMethod]
        public void EvaluateCondition_SpellAlchemical_ExcludesPreparations()
        {
            using (Character objCharacter = new Character())
            {
                Spell objSpell = new Spell(objCharacter);
                try
                {
                    Assert.IsTrue(ImprovementManager.EvaluateCondition("not(/spell/alchemical)", objSpell));
                    Assert.IsFalse(ImprovementManager.EvaluateCondition("/spell/alchemical", objSpell));

                    objSpell.Alchemical = true;

                    Assert.IsFalse(ImprovementManager.EvaluateCondition("not(/spell/alchemical)", objSpell));
                    Assert.IsTrue(ImprovementManager.EvaluateCondition("/spell/alchemical", objSpell));
                }
                finally
                {
                    objSpell.Dispose();
                }
            }
        }

        [TestMethod]
        public void ValueCache_CharacterCreatedConditions_ApplyOnlyInMatchingMode()
        {
            using (Character objCreate = new Character())
            {
                Improvement objCareerOnly = AddAcademicKarmaCost(objCreate, "/character/created");
                Improvement objCreateOnly = AddAcademicKarmaMultiplier(objCreate, "/character/created = false");

                CollectionAssert.DoesNotContain(GetAcademicCosts(objCreate), objCareerOnly);
                CollectionAssert.Contains(GetAcademicMultipliers(objCreate), objCreateOnly);
            }

            using (Character objCareer = new Character())
            {
                objCareer.SetCreated(true, false);
                Improvement objCareerOnly = AddAcademicKarmaCost(objCareer, "/character/created");
                Improvement objCreateOnly = AddAcademicKarmaMultiplier(objCareer, "/character/created = false");

                CollectionAssert.Contains(GetAcademicCosts(objCareer), objCareerOnly);
                CollectionAssert.DoesNotContain(GetAcademicMultipliers(objCareer), objCreateOnly);
            }
        }

        private static Improvement AddAcademicKarmaCost(Character objCharacter, string strCondition)
        {
            return ImprovementManager.CreateImprovement(
                objCharacter, "Academic", Improvement.ImprovementSource.Quality, "test",
                Improvement.ImprovementType.SkillCategoryKarmaCost, string.Empty, -1, 1, 3,
                strCondition: strCondition);
        }

        private static Improvement AddAcademicKarmaMultiplier(Character objCharacter, string strCondition)
        {
            return ImprovementManager.CreateImprovement(
                objCharacter, "Academic", Improvement.ImprovementSource.Quality, "test",
                Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier, string.Empty, 50,
                strCondition: strCondition);
        }

        private static List<Improvement> GetAcademicCosts(Character objCharacter)
        {
            return ImprovementManager.GetCachedImprovementListForValueOf(
                objCharacter, Improvement.ImprovementType.SkillCategoryKarmaCost, "Academic");
        }

        private static List<Improvement> GetAcademicMultipliers(Character objCharacter)
        {
            return ImprovementManager.GetCachedImprovementListForValueOf(
                objCharacter, Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier, "Academic");
        }
    }
}
