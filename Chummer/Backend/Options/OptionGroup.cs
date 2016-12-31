using System;
using System.Collections.Generic;
using System.Linq;

namespace Chummer.Backend.Options
{
    public class OptionGroup : OptionItem
    {
        public string TypeTag { get; }
        public IReadOnlyCollection<OptionItem> Children { get; }

        public OptionGroup(string displayString, string category, string typeTag, IEnumerable<OptionItem> children) : base(displayString, category)
        {
            TypeTag = typeTag;
            Children = children.ToList().AsReadOnly();
        }

        public override bool Save()
        {
            bool childchanged = false;
            foreach (OptionItem child in Children)
            {
                childchanged |= child.Save();
            }

            return childchanged;
        }

        public override void Reload()
        {
            foreach (OptionItem child in Children)
            {
                child.Reload();
            }
        }
    }
}