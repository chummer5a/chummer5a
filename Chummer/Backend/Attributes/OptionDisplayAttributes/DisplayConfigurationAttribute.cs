namespace Chummer.Backend.Attributes.OptionDisplayAttributes
{
    internal sealed class DisplayConfigurationAttribute : System.Attribute
    {
        public DisplayConfigurationAttribute(string description, string tooltip = "")
        {
            DisplayName = LanguageManager.Instance.GetString(description);
            if (!string.IsNullOrWhiteSpace(tooltip))
            {
                Tooltip = LanguageManager.Instance.GetString(tooltip);
            }
        }

        public string DisplayName { get; set; }
        public string Tooltip { get; set; }
    }
}