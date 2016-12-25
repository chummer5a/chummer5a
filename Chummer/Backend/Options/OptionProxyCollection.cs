using System.Collections.Generic;

namespace Chummer.Backend.Options
{
    public class OptionProxyCollection : OptionItem
    {
        public IReadOnlyCollection<OptionItem> Children { get; }

        OptionProxyCollection(params OptionItem[] children) : this("", "", children)
        {

        }

        OptionProxyCollection(string displayName, string category, params OptionItem[] children) : base(displayName, category)
        {
            Children = children;
        }
    }
}