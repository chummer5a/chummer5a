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

using System.IO;
using System.Xml.XPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class ShapeshifterFormRestrictionTests
    {
        [TestMethod]
        public void GetShapeshifterFormCondition_MapsLegacyExtras()
        {
            // Old saves / migration path still parse Extra form tags
            Assert.IsNull(Character.GetShapeshifterFormCondition(null));
            Assert.IsNull(Character.GetShapeshifterFormCondition("Colored Fur"));

            Assert.AreEqual(Character.MetahumanFormCondition,
                Character.GetShapeshifterFormCondition(Character.MetahumanFormOnlyExtra));
            Assert.AreEqual(Character.AnimalFormCondition,
                Character.GetShapeshifterFormCondition(Character.ShapeshifterFormOnlyExtra));
            Assert.AreEqual(Character.MetahumanFormCondition,
                Character.GetShapeshifterFormCondition("Body; " + Character.MetahumanFormOnlyExtra));
        }

        [TestMethod]
        public void StripShapeshifterFormExtra_RemovesFormTagsOnly()
        {
            Assert.AreEqual(string.Empty,
                Character.StripShapeshifterFormExtra(Character.MetahumanFormOnlyExtra));
            Assert.AreEqual("Colored Fur",
                Character.StripShapeshifterFormExtra("Colored Fur; " + Character.MetahumanFormOnlyExtra));
            Assert.AreEqual("Body",
                Character.StripShapeshifterFormExtra(Character.MetahumanFormOnlyExtra + "; Body"));
        }

        [TestMethod]
        public void MetatypesXml_GnomeShapeshifter_NeotenyUsesMetahumanFormCondition()
        {
            string strPath = Path.Combine(Utils.GetDataFolderPath, "metatypes.xml");
            Assert.IsTrue(File.Exists(strPath), "metatypes.xml not found at " + strPath);

            XPathDocument objDoc = new XPathDocument(strPath);
            XPathNavigator objRoot = objDoc.CreateNavigator();

            XPathNavigator objNeoteny = objRoot.SelectSingleNode(
                "/chummer/metatypes/metatype/metavariants/metavariant[name = 'Gnome']/qualities/negative/quality[. = 'Neoteny' and @condition = '/character/metahumanform']");
            Assert.IsNotNull(objNeoteny, "Shapeshifter Gnome Neoteny grant with metahumanform condition not found");
            Assert.AreEqual(string.Empty, objNeoteny.GetAttribute("select", string.Empty));
            Assert.IsNull(
                objRoot.SelectSingleNode(
                    "/chummer/metatypes/metatype/metavariants/metavariant/qualities/*/quality[contains(@select, 'Form Only')]"),
                "Form Only should no longer appear in select attributes");
        }
    }
}
