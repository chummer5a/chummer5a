using System;

namespace Chummer.Backend.Attributes.OptionDisplayAttributes
{
    internal sealed class DisplayConfigurationAttribute : System.Attribute
    {
        public DisplayConfigurationAttribute(string description, string tooltip = "")
        {
            try
            {
                DisplayName = LanguageManager.Instance.GetString(description);
                if (!string.IsNullOrWhiteSpace(tooltip))
                {

                    Tooltip = LanguageManager.Instance.GetString(tooltip);
                }
            }
            catch (Exception)
            {
                DisplayName = $"[{description}]";
                Tooltip = $"[{tooltip}]";
                Log.Error($"Failure to find string key either {description} {tooltip}");
            }

        }

        public string DisplayName { get; set; }
        public string Tooltip { get; set; }
    }
}