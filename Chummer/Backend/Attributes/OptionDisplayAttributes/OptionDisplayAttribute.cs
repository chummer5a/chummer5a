using System;

namespace Chummer.Backend.Attributes.OptionDisplayAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class OptionAttributes : Attribute
    {
		/// <summary>
		/// Attributes that govern the display of this object in the Options menu.
		/// </summary>
		/// <param name="path">String that groups the object in with other objects in a tree. Defaults to Uncategorized if not specified.</param>
		public OptionAttributes(string path )
        {
            Path = path;
        }

        public string Path { get; set; }
	}
}
