using System;

namespace Chummer.Backend.Attributes.SaveAttributes
{
    internal sealed class SavePropertyAsAttribute : Attribute
    {
        public string Name { get; set; }

        public SavePropertyAsAttribute(string name)
        {
            Name = name;
        }

    }
}