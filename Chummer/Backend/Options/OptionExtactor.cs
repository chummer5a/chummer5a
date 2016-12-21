using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Chummer.Datastructures;
using Chummer.Backend.Attributes.OptionDisplayAttributes;
using Chummer.Classes;

namespace Chummer.Backend.Options
{
    public class OptionExtactor
    {
        private readonly List<Predicate<PropertyInfo>> _supported;

        public OptionExtactor(List<Predicate<PropertyInfo>> supported)
        {
            //Make copy. I don't event want to think about what happens if somebody changes it while running.
            _supported = new List<Predicate<PropertyInfo>>(supported);
        }

        public SimpleTree<OptionEntryProxy> Extract(object target)
        {
            SimpleTree<OptionEntryProxy> root = new SimpleTree<OptionEntryProxy> {Tag = "root"};

            DictionaryList<string, PropertyInfo> properties = new DictionaryList<string, PropertyInfo>();

            string currentName = "";
            SimpleTree<OptionEntryProxy> parentTree;
            string[] npath;

            //BAD JOHANNES: what did we say about logic in forms?
            //to be fair, rest of code is winform specific too

            //Collect all properties in groups based on their option path
            foreach (PropertyInfo info in target.GetType().GetProperties())
            {
                if (info.GetCustomAttribute<DisplayIgnoreAttribute>() != null) continue;
                if (info.GetCustomAttribute<OptionAttributes>() != null)
                {
                    currentName = info.GetCustomAttribute<OptionAttributes>().Path;
                }

                properties.Add(currentName, info);
            }


            var temp = properties
                .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                .OrderBy(x => x.Key);

            foreach (KeyValuePair<string, List<PropertyInfo>> group in temp)
            {
                string[] path = group.Key.Split('/');
                SimpleTree<OptionEntryProxy> parrent = root;

                //find path in option tree, skip last as thats new
                //Breaks if trying to "jump" a path element
                for (int i = 0; i < path.Length - 1; i++)
                {
                    parrent = parrent.Children.First(x => (string) x.Tag == path[i]);
                }

                SimpleTree<OptionEntryProxy> newChild = new SimpleTree<OptionEntryProxy> {Tag = path.Last()};
                newChild.Leafs.AddRange(
                    group.Value
                        .Where(p => _supported.Any(x => x(p)))
                        .Select(p => CreateOptionEntry(target, p))
                        .Where(x => x != null));
                parrent.Children.Add(newChild);

            }

            return root;
        }

        private OptionEntryProxy CreateOptionEntry(object target, PropertyInfo arg)
        {
            try
            {
                DisplayConfigurationAttribute disp = arg.GetCustomAttribute<DisplayConfigurationAttribute>();
                string displayString;
                string toolTip = null;
                if (disp != null)
                {
                    displayString = LanguageManager.Instance.GetString(disp.DisplayName);
                    if (!string.IsNullOrWhiteSpace(disp.Tooltip))
                    {
                        toolTip = LanguageManager.Instance.GetString(disp.Tooltip);
                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (char c in arg.Name)
                    {
                        if (c == '_')
                            sb.Append(' ');
                        else if (char.IsUpper(c))
                        {
                            sb.Append(' ');
                            sb.Append(c);
                        }
                        else sb.Append(c);
                    }
                    displayString = sb.ToString();
                }

                return new OptionEntryProxy(target, arg, displayString, toolTip);
            }
            catch (Exception ex)
            {
                Log.Error(new object[]{ex, arg});

                if (Debugger.IsAttached)
                    throw;
                else return null;
            }
        }
    }
}