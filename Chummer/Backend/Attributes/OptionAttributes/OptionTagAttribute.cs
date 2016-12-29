using System.Collections.Generic;

namespace Chummer.Backend.Attributes.OptionAttributes
{
    public sealed class OptionTagAttribute : System.Attribute
    {
        public string[] Tags;
        public string[] TranslatedTags;
    }
}