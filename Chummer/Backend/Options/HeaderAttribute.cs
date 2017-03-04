using System;

namespace Chummer.Backend.Options
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HeaderAttribute : Attribute
    {
        public string Title { get; }

        public HeaderAttribute(string title)
        {
            Title = title;
        }
    }
}