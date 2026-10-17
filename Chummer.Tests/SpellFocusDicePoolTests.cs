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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class SpellFocusDicePoolTests
    {
        private const string SpellCategory = "UnmappedSpellCategory";
        private const string FocusCondition = "not(/spell/alchemical)";

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task SpellDicePool_DuplicateHealthFocus_CountsHighestOnce()
        {
            CancellationToken token = TestContext.CancellationToken;
            try
            {
                using (Character objCharacter = new Character())
                {
                    AddHealthBonuses(objCharacter, token);
                    using (Spell objSpell = new Spell(objCharacter) { Category = SpellCategory })
                    {
                        string strFocusValue = "(" + 6m.ToString(GlobalSettings.CultureInfo) + ")";
                        Assert.AreEqual(8, objSpell.DicePool);
                        Assert.AreEqual(1, CountOccurrences(objSpell.DicePoolTooltip, strFocusValue));
                        Assert.AreEqual(8, await objSpell.GetDicePoolAsync(token).ConfigureAwait(false));
                        Assert.AreEqual(1, CountOccurrences(await objSpell.GetDicePoolTooltipAsync(token).ConfigureAwait(false), strFocusValue));
                    }
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
                throw ex;
            }
#if MEMORYTESTING
            finally
            {
                TestContext.CancellationTokenSource.Dispose();
            }
#endif
        }

        [TestMethod]
        public async Task SpellDicePool_AlchemicalSpell_SkipsSpellcastingFocus()
        {
            CancellationToken token = TestContext.CancellationToken;
            try
            {
                using (Character objCharacter = new Character())
                {
                    AddHealthBonuses(objCharacter, token);
                    using (Spell objSpell = new Spell(objCharacter)
                           {
                               Category = SpellCategory,
                               Alchemical = true
                           })
                    {
                        string strFocusValue = "(" + 6m.ToString(GlobalSettings.CultureInfo) + ")";
                        Assert.AreEqual(2, objSpell.DicePool);
                        Assert.AreEqual(0, CountOccurrences(objSpell.DicePoolTooltip, strFocusValue));
                        Assert.AreEqual(2, await objSpell.GetDicePoolAsync(token).ConfigureAwait(false));
                        Assert.AreEqual(0, CountOccurrences(await objSpell.GetDicePoolTooltipAsync(token).ConfigureAwait(false), strFocusValue));
                    }
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
                throw ex;
            }
#if MEMORYTESTING
            finally
            {
                TestContext.CancellationTokenSource.Dispose();
            }
#endif
        }

        private static void AddHealthBonuses(Character objCharacter, CancellationToken token)
        {
            ImprovementManager.CreateImprovement(
                objCharacter, SpellCategory, Improvement.ImprovementSource.MentorSpirit, "mentor",
                Improvement.ImprovementType.SpellCategory, string.Empty, 2, token: token);
            ImprovementManager.CreateImprovement(
                objCharacter, SpellCategory, Improvement.ImprovementSource.Gear, "focus-a",
                Improvement.ImprovementType.SpellCategory, "healthspellfocus", 6,
                strCondition: FocusCondition, token: token);
            ImprovementManager.CreateImprovement(
                objCharacter, SpellCategory, Improvement.ImprovementSource.Gear, "focus-b",
                Improvement.ImprovementType.SpellCategory, "healthspellfocus", 6,
                strCondition: FocusCondition, token: token);
        }

        private static int CountOccurrences(string strText, string strValue)
        {
            int intCount = 0;
            int intIndex = 0;
            while ((intIndex = strText.IndexOf(strValue, intIndex, StringComparison.Ordinal)) >= 0)
            {
                ++intCount;
                intIndex += strValue.Length;
            }

            return intCount;
        }
    }
}
