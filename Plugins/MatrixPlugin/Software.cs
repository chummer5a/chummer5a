using Chummer.Backend.Equipment;
using System.Xml.Serialization;

namespace MatrixPlugin
{
    public class SoftwareModifier
    {
        [XmlAttribute()]
        public string Action { get; set; }
        [XmlAttribute()]
        public string Attribute { get; set; }
        [XmlAttribute()]
        public int Value { get; set; }

        public SoftwareModifier()
        {
        }
    }

    [XmlRoot(ElementName = "gear")]
    public class Software
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }
        [XmlArrayItem("modifiers", IsNullable = false)]
        public SoftwareModifier[] Modifiers { get; set; }
        
        public Software()
        {
        }
    }
}
