using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chummer.Backend.Attributes.OptionDisplayAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    class OptionPathAttribute : Attribute
    {
        public OptionPathAttribute(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
