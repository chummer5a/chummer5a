namespace Chummer.Backend.Attributes.OptionAttributes
{
    class DropDownAttribute : System.Attribute
    {
        public string[] RealValues { get; set; }
        public string[] DirectDisplay { get; set; }
        public string[] TranslatedDisplay { get; set; }
        public DropDownAttribute(string[] realValues)
        {
            RealValues = realValues;
        }
    }
}
