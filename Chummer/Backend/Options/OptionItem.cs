using System.Collections.Generic;
using System.Text;

namespace Chummer.Backend.Options
{
    public abstract class OptionItem : OptionRenderItem
    {
        protected OptionItem(string displayString, string category)
        {
            DisplayString = displayString ?? "";
            Category = category ?? "";
        }

        public string DisplayString { get; }

        public string Category { get; }

        public List<string> Tags { get; } = new List<string>();

        public virtual IEnumerable<string> SearchStrings()
        {
            //TODO: Cache
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < DisplayString.Length; i++)
            {
                if (DisplayString[i] != '\\')
                {
                    sb.Append(DisplayString[i]);
                }
                else
                {
                    i++;
                }
            }
            yield return sb.ToString();

            yield return Category;

            foreach (string tag in Tags)
            {
                yield return tag;
            }
        }

        public abstract bool Save();
        public abstract void Reload();
    }
}