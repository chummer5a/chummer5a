using System;

namespace Chummer
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SettingsElementAttribute : Attribute
    {
        public SettingsElementAttribute(string elementName)
        {
            ElementName = elementName ?? throw new ArgumentNullException(nameof(elementName));
        }

        public string ElementName { get; }
        
        /// <summary>
        /// Gets the parent element name if this is a nested element (e.g., "karmacost" for "karmacost/karmaattribute")
        /// </summary>
        public string ParentElementName
        {
            get
            {
                int lastSlashIndex = ElementName.LastIndexOf('/');
                return lastSlashIndex > 0 ? ElementName.Substring(0, lastSlashIndex) : null;
            }
        }
        
        /// <summary>
        /// Gets the child element name if this is a nested element (e.g., "karmaattribute" for "karmacost/karmaattribute")
        /// </summary>
        public string ChildElementName
        {
            get
            {
                int lastSlashIndex = ElementName.LastIndexOf('/');
                return lastSlashIndex > 0 ? ElementName.Substring(lastSlashIndex + 1) : ElementName;
            }
        }
        
        /// <summary>
        /// Gets whether this element is nested (has a parent element)
        /// </summary>
        public bool IsNested => ElementName.Contains("/");
    }
}


