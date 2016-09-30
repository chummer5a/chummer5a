using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chummer.Backend.Attributes.OptionDisplayAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class OptionPathAttribute : Attribute
    {
        public OptionPathAttribute(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }

	internal class OptionDisplayNameAttribute : Attribute
	{
		public OptionDisplayNameAttribute(string description)
		{
			//Could/should/can this be cached? If DisplayName == null, DisplayName = getstring?
			DisplayName = LanguageManager.Instance.GetString(description);
		}
		public string DisplayName { get; }
	}

	internal class OptionTooltipAttribute : Attribute
	{
		public OptionTooltipAttribute(string tooltip)
		{
			//Could/should/can this be cached? If Tooltip == null, Tooltip = getstring?
			Tooltip = LanguageManager.Instance.GetString(tooltip);
		}
		public string Tooltip { get; }
	}
}
