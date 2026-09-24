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
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class VehicleModRequirementTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task CheckModRequirements_VehicleModRequiresInstalledModOnSameVehicle()
        {
            CancellationToken token = TestContext.CancellationToken;
            token.ThrowIfCancellationRequested();
            try
            {
                Character objCharacter = new Character();
                try
                {
                    Vehicle objVehicle = new Vehicle(objCharacter);
                    Vehicle objOtherVehicle = new Vehicle(objCharacter);
                    await objCharacter.Vehicles.AddAsync(objVehicle, token).ConfigureAwait(false);
                    await objCharacter.Vehicles.AddAsync(objOtherVehicle, token).ConfigureAwait(false);

                    VehicleMod objInstalled = new VehicleMod(objCharacter)
                    {
                        Name = "Rigger Interface"
                    };
                    await objVehicle.Mods.AddAsync(objInstalled, token).ConfigureAwait(false);

                    XPathNavigator objSameParent = LoadModRequirement(true);
                    XPathNavigator objAnyVehicle = LoadModRequirement(false);

                    Assert.IsTrue(await objVehicle.CheckModRequirementsAsync(objSameParent, token).ConfigureAwait(false));
                    Assert.IsFalse(await objOtherVehicle.CheckModRequirementsAsync(objSameParent, token).ConfigureAwait(false));
                    Assert.IsTrue(await objOtherVehicle.CheckModRequirementsAsync(objAnyVehicle, token).ConfigureAwait(false));

                    await objVehicle.Mods.RemoveAsync(objInstalled, token).ConfigureAwait(false);
                    Assert.IsFalse(await objVehicle.CheckModRequirementsAsync(objSameParent, token).ConfigureAwait(false));
                    Assert.IsFalse(await objOtherVehicle.CheckModRequirementsAsync(objAnyVehicle, token).ConfigureAwait(false));
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
#if MEMORYTESTING
            finally
            {
                TestContext.CancellationTokenSource.Dispose();
            }
#endif
        }

        private static XPathNavigator LoadModRequirement(bool blnSameParent)
        {
            string strSameParent = blnSameParent ? " sameparent=\"True\"" : string.Empty;
            XmlDocument xmlDocument = new XmlDocument { XmlResolver = null };
            xmlDocument.LoadXml("<mod><required><allof><vehiclemod" + strSameParent
                                + ">Rigger Interface</vehiclemod></allof></required></mod>");
            return xmlDocument.DocumentElement.CreateNavigator();
        }
    }
}
