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
    public class SkillSpecializationTranslationTests
    {
        /// <summary>
        /// Documents the German Wahrnehmung collision from #4739 and the parent-skill-scoped
        /// XPath that SkillSpecialization uses to resolve Detection instead of Perception.
        /// </summary>
        [TestMethod]
        public void GermanWahrnehmung_OnSpellcasting_ResolvesToDetectionNotPerception()
        {
            string strLangPath = Path.Combine(Utils.GetLanguageFolderPath, "de-de_data.xml");
            Assert.IsTrue(File.Exists(strLangPath), "de-de_data.xml not found at " + strLangPath);

            XPathNavigator objLang = new XPathDocument(strLangPath).CreateNavigator();
            XPathNavigator objSkills = objLang.SelectSingleNode("/chummer/chummer[@file = 'skills.xml']");
            Assert.IsNotNull(objSkills);

            Assert.AreEqual("Wahrnehmung",
                objSkills.SelectSingleNode("skills/skill[name = 'Perception']/translate")?.Value,
                "Perception skill must translate to Wahrnehmung (collision source)");

            // Parent-skill-scoped lookup used by SkillSpecialization.ReverseTranslateNameAsync
            XPathNavigator objSpellcasting = objSkills.SelectSingleNode("skills/skill[name = 'Spellcasting']");
            Assert.IsNotNull(objSpellcasting);
            Assert.AreEqual("Detection",
                objSpellcasting.SelectSingleNode("specs/spec[@translate = 'Wahrnehmung']")?.Value,
                "Spellcasting specialization Wahrnehmung must reverse-translate to Detection");
            Assert.IsNull(
                objSpellcasting.SelectSingleNode("specs/spec[. = 'Perception']"),
                "Spellcasting must not list Perception as a specialization");
        }
    }
}
