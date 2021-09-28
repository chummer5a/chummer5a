using Chummer.Backend.Equipment;
using System.Collections.Generic;
using System.Xml.XPath;

namespace MatrixPlugin
{
    public class SoftwareModifier
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Action { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Attribute { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Value { get; set; }
    }
    public class Software
    {
        public string Name { get; set; }
        public string Description { get; set; }
        [System.Xml.Serialization.XmlArrayItemAttribute("modifier", IsNullable = false)]
        public SoftwareModifier[] Modifiers { get; set; }
        private Gear _gear;

    }
}
