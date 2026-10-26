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
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class GearLocationReorderTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void MoveGearRootAsync_SelectedGearHeader_ReordersOntoLaterLocation()
        {
            try
            {
                using (Character objCharacter = new Character())
                {
                    Location objAlpha = new Location(objCharacter, objCharacter.GearLocations, "Alpha");
                    Location objBeta = new Location(objCharacter, objCharacter.GearLocations, "Beta");
                    objCharacter.GearLocations.Add(objAlpha);
                    objCharacter.GearLocations.Add(objBeta);
                    Utils.RunOnMainThread(t =>
                    {
                        if (t.IsCancellationRequested)
                            return Task.FromCanceled(t);
                        using (TreeView treGear = new TreeView())
                        {
                            if (t.IsCancellationRequested)
                                return Task.FromCanceled(t);
                            treGear.Nodes.Add("Selected Gear").Tag = "Node_SelectedGear";
                            TreeNode nodAlpha = treGear.Nodes.Add("Alpha");
                            nodAlpha.Tag = objAlpha;
                            TreeNode nodBeta = treGear.Nodes.Add("Beta");
                            nodBeta.Tag = objBeta;

                            // Beta's tree index is 2, which is past GearLocations.Count and used to throw.
                            return objCharacter.MoveGearRootAsync(nodBeta.Index, nodBeta, nodAlpha, t);
                        }
                    }, token: TestContext.CancellationToken);
                    Assert.AreEqual(objBeta, objCharacter.GearLocations[0]);
                    Assert.AreEqual(objAlpha, objCharacter.GearLocations[1]);
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
        public void MoveGearRoot_SelectedGearHeader_ReordersOntoEarlierLocation()
        {
            try
            {
                using (Character objCharacter = new Character())
                {
                    Location objAlpha = new Location(objCharacter, objCharacter.GearLocations, "Alpha");
                    Location objBeta = new Location(objCharacter, objCharacter.GearLocations, "Beta");
                    objCharacter.GearLocations.Add(objAlpha);
                    objCharacter.GearLocations.Add(objBeta);

                    Utils.RunOnMainThread(t =>
                    {
                        t.ThrowIfCancellationRequested();
                        using (TreeView treGear = new TreeView())
                        {
                            t.ThrowIfCancellationRequested();
                            treGear.Nodes.Add("Selected Gear").Tag = "Node_SelectedGear";
                            TreeNode nodAlpha = treGear.Nodes.Add("Alpha");
                            nodAlpha.Tag = objAlpha;
                            nodAlpha.Nodes.Add("Loose gear");
                            TreeNode nodBeta = treGear.Nodes.Add("Beta");
                            nodBeta.Tag = objBeta;

                            objCharacter.MoveGearRoot(nodAlpha.Nodes[0].Index, nodAlpha.Nodes[0], nodBeta, t);
                        }
                    }, token: TestContext.CancellationToken);

                    Assert.AreEqual(objBeta, objCharacter.GearLocations[0]);
                    Assert.AreEqual(objAlpha, objCharacter.GearLocations[1]);
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
        public void MoveGearRoot_WithoutHeader_UsesLocationOrder()
        {
            try
            {
                using (Character objCharacter = new Character())
                {
                    Location objAlpha = new Location(objCharacter, objCharacter.GearLocations, "Alpha");
                    Location objBeta = new Location(objCharacter, objCharacter.GearLocations, "Beta");
                    objCharacter.GearLocations.Add(objAlpha);
                    objCharacter.GearLocations.Add(objBeta);
                    Utils.RunOnMainThread(t =>
                    {
                        t.ThrowIfCancellationRequested();
                        using (TreeView treGear = new TreeView())
                        {
                            t.ThrowIfCancellationRequested();
                            TreeNode nodAlpha = treGear.Nodes.Add("Alpha");
                            nodAlpha.Tag = objAlpha;
                            TreeNode nodBeta = treGear.Nodes.Add("Beta");
                            nodBeta.Tag = objBeta;

                            objCharacter.MoveGearRoot(nodBeta.Index, nodBeta, nodAlpha, t);
                        }
                    }, token: TestContext.CancellationToken);
                    Assert.AreEqual(objBeta, objCharacter.GearLocations[0]);
                    Assert.AreEqual(objAlpha, objCharacter.GearLocations[1]);
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
    }
}
