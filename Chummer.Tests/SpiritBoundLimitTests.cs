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
using System.IO;
using System.Xml.XPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class SpiritBoundLimitTests
    {
        [TestMethod]
        public void TraditionsXml_WatcherAndHomunculus_IgnoreBoundSpiritLimit()
        {
            string strPath = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Chummer", "data", "traditions.xml"));
            Assert.IsTrue(File.Exists(strPath), "traditions.xml not found at " + strPath);

            XPathDocument objDoc = new XPathDocument(strPath);
            XPathNavigator objRoot = objDoc.CreateNavigator();

            Assert.AreEqual("True",
                objRoot.SelectSingleNode("/chummer/spirits/spirit[name = 'Watcher']/ignoreboundspiritlimit")?.Value);

            Assert.AreEqual(0,
                objRoot.Select(
                    "/chummer/spirits/spirit[starts-with(name, 'Homunculus') and not(ignoreboundspiritlimit = 'True')]").Count,
                "Every Homunculus spirit should set ignoreboundspiritlimit");

            Assert.IsNull(
                objRoot.SelectSingleNode("/chummer/spirits/spirit[name = 'Spirit of Fire']/ignoreboundspiritlimit"),
                "Normal spirits should not set ignoreboundspiritlimit");
        }
    }
}
