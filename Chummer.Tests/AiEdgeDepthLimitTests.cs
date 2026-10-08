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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Chummer.Backend.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class AiEdgeDepthLimitTests
    {
        private const int EdgeMetatypeMinimum = 1;
        private const int EdgeMetatypeMaximum = 6;
        private const int DepthMetatypeMinimum = 1;
        private const int DepthRoomAboveMetatypeMaximum = 12;

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task AiEdgeLimits_MatchDepthTotal()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                Character objCharacter = await CreateAiCharacterAsync(token).ConfigureAwait(false);
                try
                {
                    CharacterAttrib objDepth = objCharacter.DEP;
                    CharacterAttrib objEdge = objCharacter.EDG;

                    Assert.AreEqual(DepthMetatypeMinimum, await objDepth.GetTotalValueAsync(token).ConfigureAwait(false));
                    Assert.AreEqual(DepthMetatypeMinimum, await objEdge.GetMetatypeMaximumAsync(token).ConfigureAwait(false));
                    Assert.AreEqual(DepthMetatypeMinimum, await objEdge.GetMetatypeAugmentedMaximumAsync(token).ConfigureAwait(false));
                    Assert.AreEqual(DepthMetatypeMinimum, await objEdge.GetTotalAugmentedMaximumAsync(token).ConfigureAwait(false));

                    await objDepth.SetKarmaAsync(EdgeMetatypeMaximum, token).ConfigureAwait(false);

                    int intDepth = await objDepth.GetTotalValueAsync(token).ConfigureAwait(false);
                    Assert.AreEqual(DepthMetatypeMinimum + EdgeMetatypeMaximum, intDepth);
                    Assert.AreEqual(intDepth, await objEdge.GetMetatypeMaximumAsync(token).ConfigureAwait(false));
                    Assert.AreEqual(intDepth, await objEdge.GetMetatypeAugmentedMaximumAsync(token).ConfigureAwait(false));
                    Assert.AreEqual(intDepth, objEdge.MetatypeMaximum);
                    Assert.AreEqual(intDepth, objEdge.MetatypeAugmentedMaximum);

                    await objEdge.SetKarmaAsync(EdgeMetatypeMaximum, token).ConfigureAwait(false);
                    Assert.AreEqual(intDepth, await objEdge.GetValueAsync(token).ConfigureAwait(false));
                    Assert.AreEqual(intDepth, await objEdge.GetTotalValueAsync(token).ConfigureAwait(false));
                }
                finally
                {
                    await objCharacter.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task AiEdgeValue_RisesWhenDepthRises()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                Character objCharacter = await CreateAiCharacterAsync(token).ConfigureAwait(false);
                try
                {
                    CharacterAttrib objDepth = objCharacter.DEP;
                    CharacterAttrib objEdge = objCharacter.EDG;
                    await objEdge.SetKarmaAsync(EdgeMetatypeMaximum, token).ConfigureAwait(false);

                    Assert.AreEqual(DepthMetatypeMinimum, await objEdge.GetValueAsync(token).ConfigureAwait(false));

                    await objDepth.SetKarmaAsync(EdgeMetatypeMaximum, token).ConfigureAwait(false);

                    int intDepth = await objDepth.GetTotalValueAsync(token).ConfigureAwait(false);
                    Assert.AreEqual(intDepth, await objEdge.GetValueAsync(token).ConfigureAwait(false));
                    Assert.AreEqual(intDepth, await objEdge.GetTotalValueAsync(token).ConfigureAwait(false));

                    await objCharacter.SetKarmaAsync(100, token).ConfigureAwait(false);
                    Assert.IsFalse(await objEdge.GetCanUpgradeCareerAsync(token).ConfigureAwait(false));
                }
                finally
                {
                    await objCharacter.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task NonAiEdgeLimits_StayAtMetatypeMaximum()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                Character objCharacter = new Character();
                try
                {
                    await objCharacter.EDG.AssignLimitsAsync(EdgeMetatypeMinimum, EdgeMetatypeMaximum, EdgeMetatypeMaximum, token)
                        .ConfigureAwait(false);
                    await objCharacter.DEP.AssignLimitsAsync(DepthMetatypeMinimum, DepthRoomAboveMetatypeMaximum, DepthRoomAboveMetatypeMaximum, token)
                        .ConfigureAwait(false);
                    await objCharacter.DEP.SetKarmaAsync(EdgeMetatypeMaximum, token).ConfigureAwait(false);

                    Assert.IsFalse(await objCharacter.GetIsAIAsync(token).ConfigureAwait(false));
                    Assert.AreEqual(EdgeMetatypeMaximum, await objCharacter.EDG.GetMetatypeMaximumAsync(token).ConfigureAwait(false));
                    Assert.AreEqual(EdgeMetatypeMaximum, await objCharacter.EDG.GetMetatypeAugmentedMaximumAsync(token).ConfigureAwait(false));
                }
                finally
                {
                    await objCharacter.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
                throw;
            }
        }

        private static async Task<Character> CreateAiCharacterAsync(CancellationToken token)
        {
            Character objCharacter = new Character();
            try
            {
                await objCharacter.BOD.AssignLimitsAsync(0, 0, 0, token).ConfigureAwait(false);
                await objCharacter.EDG.AssignLimitsAsync(EdgeMetatypeMinimum, EdgeMetatypeMaximum, EdgeMetatypeMaximum, token)
                    .ConfigureAwait(false);
                await objCharacter.DEP.AssignLimitsAsync(DepthMetatypeMinimum, DepthRoomAboveMetatypeMaximum, DepthRoomAboveMetatypeMaximum, token)
                    .ConfigureAwait(false);
                await objCharacter.SetDEPEnabledAsync(true, token).ConfigureAwait(false);
                Assert.IsTrue(await objCharacter.GetIsAIAsync(token).ConfigureAwait(false));
                return objCharacter;
            }
            catch
            {
                await objCharacter.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }
    }
}
