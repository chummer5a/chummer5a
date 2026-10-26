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
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class GearLocationReorderTests
    {
        [TestMethod]
        public void MoveGearRootAsync_SelectedGearHeader_ReordersOntoLaterLocation()
        {
            RunOnStaThread(() =>
            {
                using (Character objCharacter = new Character())
                using (TreeView treGear = new TreeView())
                {
                    Location objAlpha = new Location(objCharacter, objCharacter.GearLocations, "Alpha");
                    Location objBeta = new Location(objCharacter, objCharacter.GearLocations, "Beta");
                    objCharacter.GearLocations.Add(objAlpha);
                    objCharacter.GearLocations.Add(objBeta);

                    treGear.Nodes.Add("Selected Gear").Tag = "Node_SelectedGear";
                    TreeNode nodAlpha = treGear.Nodes.Add("Alpha");
                    nodAlpha.Tag = objAlpha;
                    TreeNode nodBeta = treGear.Nodes.Add("Beta");
                    nodBeta.Tag = objBeta;

                    // Beta's tree index is 2, which is past GearLocations.Count and used to throw.
                    objCharacter.MoveGearRootAsync(nodBeta.Index, nodBeta, nodAlpha).GetAwaiter().GetResult();

                    Assert.AreEqual(objBeta, objCharacter.GearLocations[0]);
                    Assert.AreEqual(objAlpha, objCharacter.GearLocations[1]);
                }
            });
        }

        [TestMethod]
        public void MoveGearRoot_SelectedGearHeader_ReordersOntoEarlierLocation()
        {
            RunOnStaThread(() =>
            {
                using (Character objCharacter = new Character())
                using (TreeView treGear = new TreeView())
                {
                    Location objAlpha = new Location(objCharacter, objCharacter.GearLocations, "Alpha");
                    Location objBeta = new Location(objCharacter, objCharacter.GearLocations, "Beta");
                    objCharacter.GearLocations.Add(objAlpha);
                    objCharacter.GearLocations.Add(objBeta);

                    treGear.Nodes.Add("Selected Gear").Tag = "Node_SelectedGear";
                    TreeNode nodAlpha = treGear.Nodes.Add("Alpha");
                    nodAlpha.Tag = objAlpha;
                    nodAlpha.Nodes.Add("Loose gear");
                    TreeNode nodBeta = treGear.Nodes.Add("Beta");
                    nodBeta.Tag = objBeta;

                    objCharacter.MoveGearRoot(nodAlpha.Nodes[0].Index, nodAlpha.Nodes[0], nodBeta);

                    Assert.AreEqual(objBeta, objCharacter.GearLocations[0]);
                    Assert.AreEqual(objAlpha, objCharacter.GearLocations[1]);
                }
            });
        }

        [TestMethod]
        public void MoveGearRoot_WithoutHeader_UsesLocationOrder()
        {
            RunOnStaThread(() =>
            {
                using (Character objCharacter = new Character())
                using (TreeView treGear = new TreeView())
                {
                    Location objAlpha = new Location(objCharacter, objCharacter.GearLocations, "Alpha");
                    Location objBeta = new Location(objCharacter, objCharacter.GearLocations, "Beta");
                    objCharacter.GearLocations.Add(objAlpha);
                    objCharacter.GearLocations.Add(objBeta);

                    TreeNode nodAlpha = treGear.Nodes.Add("Alpha");
                    nodAlpha.Tag = objAlpha;
                    TreeNode nodBeta = treGear.Nodes.Add("Beta");
                    nodBeta.Tag = objBeta;

                    objCharacter.MoveGearRoot(nodBeta.Index, nodBeta, nodAlpha);

                    Assert.AreEqual(objBeta, objCharacter.GearLocations[0]);
                    Assert.AreEqual(objAlpha, objCharacter.GearLocations[1]);
                }
            });
        }

        private static void RunOnStaThread(Action action)
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                action();
                return;
            }

            Exception objException = null;
            Thread objThread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    objException = ex;
                }
            });
            objThread.SetApartmentState(ApartmentState.STA);
            objThread.Start();
            if (!objThread.Join(TimeSpan.FromMinutes(3)))
                Assert.Fail("Gear location reorder test timed out.");
            if (objException != null)
                throw objException;
        }
    }
}
