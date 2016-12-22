using System;

namespace Chummer.Backend.Attributes.OptionAttributes
{
    internal sealed class DisplayConfigurationAttribute : System.Attribute
    {
        public DisplayConfigurationAttribute(string description, string tooltip = "")
        {
            DisplayName = description;
            Tooltip = tooltip;

        }

        public string DisplayName { get; set; }
        public string Tooltip { get; set; }
    }
}