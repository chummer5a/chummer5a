using System;

namespace Chummer.Backend.Attributes.OptionDisplayAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class OptionAttributes : Attribute
    {
		/// <summary>
		/// Attributes that govern the display of this object in the Options menu.
		/// </summary>
		/// <param name="description">Localised description string to look for. REQUIRED (Set to defaultable for debugging).</param>
		/// <param name="objDefault">Default value to use for the option. REQUIRED (Set to null for debugging).</param>
		/// <param name="path">String that groups the object in with other objects in a tree. Defaults to Uncategorized if not specified.</param>
		/// <param name="tooltip">Tooltip to describe the option in further detail. Optional.</param>
		public OptionAttributes(string description = "Label_Options_BPAttribute", object objDefault = null, string path = "Uncategorized", string tooltip = "")
        {
            Path = path;
			Default = objDefault;
			DisplayName = LanguageManager.Instance.GetString(description);
			if (!string.IsNullOrWhiteSpace(tooltip))
			{
				Tooltip = LanguageManager.Instance.GetString(tooltip);
			}
        }

        public string Path { get; set; }
		public string DisplayName { get; set; }
		public string Tooltip { get; set; }
		public object Default { get; set; }
	}
}
