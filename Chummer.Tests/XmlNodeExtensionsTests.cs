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
using System.Xml;
using Chummer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class XmlNodeExtensionsTests
    {
        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_NullNode()
        {
            XmlNode nullNode = null;
            Assert.IsTrue(nullNode.IsNullOrInnerTextIsEmpty(), "Null node should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_EmptyElement()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement emptyElement = doc.CreateElement("test");
            Assert.IsTrue(emptyElement.IsNullOrInnerTextIsEmpty(), "Element with no children should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ElementWithEmptyText()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement element = doc.CreateElement("test");
            element.InnerText = string.Empty;
            Assert.IsTrue(element.IsNullOrInnerTextIsEmpty(), "Element with empty InnerText should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ElementWithWhitespaceOnly()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement element = doc.CreateElement("test");
            element.InnerText = "   \t\n  ";
            // Whitespace-only nodes should be considered empty
            Assert.IsTrue(element.IsNullOrInnerTextIsEmpty(), "Element with only whitespace should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ElementWithContent()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement element = doc.CreateElement("test");
            element.InnerText = "some content";
            Assert.IsFalse(element.IsNullOrInnerTextIsEmpty(), "Element with content should not be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ElementWithChildElement()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement parent = doc.CreateElement("parent");
            XmlElement child = doc.CreateElement("child");
            parent.AppendChild(child);
            // Element with empty child element should be considered empty
            Assert.IsTrue(parent.IsNullOrInnerTextIsEmpty(), "Element with empty child element should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ElementWithChildElementWithContent()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement parent = doc.CreateElement("parent");
            XmlElement child = doc.CreateElement("child");
            child.InnerText = "content";
            parent.AppendChild(child);
            Assert.IsFalse(parent.IsNullOrInnerTextIsEmpty(), "Element with child element containing content should not be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ElementWithMultipleEmptyChildren()
        {
            // This test specifically targets the bug that was fixed where CheckChildren
            // was checking firstChild.NodeType instead of xmlNodeInner.NodeType
            XmlDocument doc = new XmlDocument();
            XmlElement parent = doc.CreateElement("parent");
            XmlElement child1 = doc.CreateElement("child1");
            XmlElement child2 = doc.CreateElement("child2");
            XmlElement child3 = doc.CreateElement("child3");
            parent.AppendChild(child1);
            parent.AppendChild(child2);
            parent.AppendChild(child3);
            // Element with multiple empty child elements should be considered empty
            Assert.IsTrue(parent.IsNullOrInnerTextIsEmpty(), "Element with multiple empty child elements should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ElementWithMultipleChildrenOneWithContent()
        {
            // This test specifically targets the bug that was fixed
            XmlDocument doc = new XmlDocument();
            XmlElement parent = doc.CreateElement("parent");
            XmlElement child1 = doc.CreateElement("child1");
            XmlElement child2 = doc.CreateElement("child2");
            child2.InnerText = "content";
            XmlElement child3 = doc.CreateElement("child3");
            parent.AppendChild(child1);
            parent.AppendChild(child2);
            parent.AppendChild(child3);
            Assert.IsFalse(parent.IsNullOrInnerTextIsEmpty(), "Element with multiple children where one has content should not be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ElementWithCDATA()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement element = doc.CreateElement("test");
            XmlCDataSection cdata = doc.CreateCDataSection("content");
            element.AppendChild(cdata);
            Assert.IsFalse(element.IsNullOrInnerTextIsEmpty(), "Element with CDATA containing content should not be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ElementWithEmptyCDATA()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement element = doc.CreateElement("test");
            XmlCDataSection cdata = doc.CreateCDataSection(string.Empty);
            element.AppendChild(cdata);
            Assert.IsTrue(element.IsNullOrInnerTextIsEmpty(), "Element with empty CDATA should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ElementWithNestedEmptyChildren()
        {
            // Test nested structure with all empty children
            XmlDocument doc = new XmlDocument();
            XmlElement parent = doc.CreateElement("parent");
            XmlElement child1 = doc.CreateElement("child1");
            XmlElement child2 = doc.CreateElement("child2");
            XmlElement grandchild = doc.CreateElement("grandchild");
            child1.AppendChild(grandchild);
            parent.AppendChild(child1);
            parent.AppendChild(child2);
            Assert.IsTrue(parent.IsNullOrInnerTextIsEmpty(), "Element with nested empty children should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ElementWithNestedChildrenWithContent()
        {
            // Test nested structure with content in nested child
            XmlDocument doc = new XmlDocument();
            XmlElement parent = doc.CreateElement("parent");
            XmlElement child1 = doc.CreateElement("child1");
            XmlElement child2 = doc.CreateElement("child2");
            XmlElement grandchild = doc.CreateElement("grandchild");
            grandchild.InnerText = "content";
            child1.AppendChild(grandchild);
            parent.AppendChild(child1);
            parent.AppendChild(child2);
            Assert.IsFalse(parent.IsNullOrInnerTextIsEmpty(), "Element with nested children containing content should not be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ElementWithMixedContent()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement element = doc.CreateElement("test");
            element.InnerXml = "<child1></child1>text<child2></child2>";
            Assert.IsFalse(element.IsNullOrInnerTextIsEmpty(), "Element with mixed content (text and elements) should not be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ElementWithOnlyEmptyChildElements()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement element = doc.CreateElement("test");
            element.InnerXml = "<child1></child1><child2></child2><child3></child3>";
            Assert.IsTrue(element.IsNullOrInnerTextIsEmpty(), "Element with only empty child elements should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_WeaponBonusScenario_EmptyElement()
        {
            // Simulate the weaponbonus scenario where an empty element is saved
            XmlDocument doc = new XmlDocument();
            XmlElement weaponBonus = doc.CreateElement("weaponbonus");
            // This simulates <weaponbonus></weaponbonus>
            Assert.IsTrue(weaponBonus.IsNullOrInnerTextIsEmpty(), "Empty weaponbonus element should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_WeaponBonusScenario_WithContent()
        {
            // Simulate the weaponbonus scenario with actual content
            XmlDocument doc = new XmlDocument();
            XmlElement weaponBonus = doc.CreateElement("weaponbonus");
            XmlElement damage = doc.CreateElement("damage");
            damage.InnerText = "+1";
            weaponBonus.AppendChild(damage);
            Assert.IsFalse(weaponBonus.IsNullOrInnerTextIsEmpty(), "Weaponbonus element with content should not be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_WeaponBonusScenario_WithMultipleEmptyChildren()
        {
            // This is the specific scenario that would have failed before the bug fix
            XmlDocument doc = new XmlDocument();
            XmlElement weaponBonus = doc.CreateElement("weaponbonus");
            XmlElement child1 = doc.CreateElement("damage");
            XmlElement child2 = doc.CreateElement("ap");
            XmlElement child3 = doc.CreateElement("acc");
            weaponBonus.AppendChild(child1);
            weaponBonus.AppendChild(child2);
            weaponBonus.AppendChild(child3);
            // Before the fix, this would incorrectly return false because it was checking firstChild.NodeType for all children
            Assert.IsTrue(weaponBonus.IsNullOrInnerTextIsEmpty(), "Weaponbonus with multiple empty child elements should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_WeaponBonusScenario_WithMultipleChildrenOneWithContent()
        {
            // This is the specific scenario that would have failed before the bug fix
            XmlDocument doc = new XmlDocument();
            XmlElement weaponBonus = doc.CreateElement("weaponbonus");
            XmlElement child1 = doc.CreateElement("damage");
            XmlElement child2 = doc.CreateElement("ap");
            child2.InnerText = "-1";
            XmlElement child3 = doc.CreateElement("acc");
            weaponBonus.AppendChild(child1);
            weaponBonus.AppendChild(child2);
            weaponBonus.AppendChild(child3);
            // Before the fix, this might have incorrectly evaluated due to checking firstChild instead of xmlNodeInner
            Assert.IsFalse(weaponBonus.IsNullOrInnerTextIsEmpty(), "Weaponbonus with multiple children where one has content should not be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_LoadedFromXmlString_EmptyElement()
        {
            // Test loading an empty element from XML string (simulates save/load scenario)
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<root><weaponbonus></weaponbonus></root>");
            XmlElement weaponBonus = (XmlElement)doc.SelectSingleNode("//weaponbonus");
            Assert.IsTrue(weaponBonus.IsNullOrInnerTextIsEmpty(), "Empty weaponbonus element loaded from XML string should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_LoadedFromXmlString_WithWhitespace()
        {
            // Test loading an element with whitespace from XML string
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml("<root><weaponbonus>   </weaponbonus></root>");
            XmlElement weaponBonus = (XmlElement)doc.SelectSingleNode("//weaponbonus");
            Assert.IsTrue(weaponBonus.IsNullOrInnerTextIsEmpty(), "Weaponbonus element with only whitespace should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_LoadedFromXmlString_WithNewlines()
        {
            // Test loading an element with newlines/formatting from XML string
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml("<root><weaponbonus>\n\t\n</weaponbonus></root>");
            XmlElement weaponBonus = (XmlElement)doc.SelectSingleNode("//weaponbonus");
            Assert.IsTrue(weaponBonus.IsNullOrInnerTextIsEmpty(), "Weaponbonus element with only whitespace/newlines should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_LoadedFromXmlString_WithContent()
        {
            // Test loading an element with actual content from XML string
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<root><weaponbonus><damage>+1</damage></weaponbonus></root>");
            XmlElement weaponBonus = (XmlElement)doc.SelectSingleNode("//weaponbonus");
            Assert.IsFalse(weaponBonus.IsNullOrInnerTextIsEmpty(), "Weaponbonus element with content loaded from XML string should not be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_LoadedFromXmlString_MultipleEmptyChildren()
        {
            // Test the specific bug scenario with XML loaded from string
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<root><weaponbonus><damage></damage><ap></ap><acc></acc></weaponbonus></root>");
            XmlElement weaponBonus = (XmlElement)doc.SelectSingleNode("//weaponbonus");
            // This is the exact scenario that would have failed before the bug fix
            Assert.IsTrue(weaponBonus.IsNullOrInnerTextIsEmpty(), "Weaponbonus with multiple empty children loaded from XML should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_TextNodeWithWhitespace()
        {
            // Test element with explicit whitespace text node
            XmlDocument doc = new XmlDocument();
            XmlElement element = doc.CreateElement("test");
            XmlText whitespaceNode = doc.CreateTextNode("   \t  ");
            element.AppendChild(whitespaceNode);
            Assert.IsTrue(element.IsNullOrInnerTextIsEmpty(), "Element with whitespace-only text node should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_SignificantWhitespaceNode()
        {
            // Test element with significant whitespace node
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            XmlElement element = doc.CreateElement("test");
            XmlSignificantWhitespace whitespaceNode = doc.CreateSignificantWhitespace("   ");
            element.AppendChild(whitespaceNode);
            Assert.IsTrue(element.IsNullOrInnerTextIsEmpty(), "Element with significant whitespace-only node should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ComplexNestedStructure_AllEmpty()
        {
            // Test complex nested structure with all empty elements
            XmlDocument doc = new XmlDocument();
            XmlElement parent = doc.CreateElement("weaponbonus");
            XmlElement level1a = doc.CreateElement("damage");
            XmlElement level1b = doc.CreateElement("ap");
            XmlElement level2a = doc.CreateElement("modifier");
            XmlElement level2b = doc.CreateElement("type");
            level1a.AppendChild(level2a);
            level1a.AppendChild(level2b);
            parent.AppendChild(level1a);
            parent.AppendChild(level1b);
            Assert.IsTrue(parent.IsNullOrInnerTextIsEmpty(), "Complex nested structure with all empty elements should be considered empty");
        }

        [TestMethod]
        public void TestIsNullOrInnerTextIsEmpty_ComplexNestedStructure_WithContent()
        {
            // Test complex nested structure with content in one nested element
            XmlDocument doc = new XmlDocument();
            XmlElement parent = doc.CreateElement("weaponbonus");
            XmlElement level1a = doc.CreateElement("damage");
            XmlElement level1b = doc.CreateElement("ap");
            XmlElement level2a = doc.CreateElement("modifier");
            level2a.InnerText = "+2";
            level1a.AppendChild(level2a);
            parent.AppendChild(level1a);
            parent.AppendChild(level1b);
            Assert.IsFalse(parent.IsNullOrInnerTextIsEmpty(), "Complex nested structure with content should not be considered empty");
        }
    }
}

