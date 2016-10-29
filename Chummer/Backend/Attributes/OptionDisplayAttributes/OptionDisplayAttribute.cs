using System;

namespace Chummer.Backend.Attributes.OptionDisplayAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class OptionAttributes : Attribute
    {
		/// <summary>
		/// Attributes that govern the display of this object in the Options menu.
		/// </summary>
		/// <param name="description">Localised description string to look for. REQUIRED (Set to defaultable for debugging).</param>
		/// <param name="objDefault">Default value to use for the option. REQUIRED (Set to null for debugging).</param>
		/// <param name="path">String that groups the object in with other objects in a tree. Defaults to Uncategorized if not specified.</param>
		/// <param name="tooltip">Tooltip to describe the option in further detail. Optional.</param>
		public OptionAttributes(string path )
        {
            Path = path;
        }

        public string Path { get; set; }
	}
}
