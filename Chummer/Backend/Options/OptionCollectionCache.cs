using System.Collections.Generic;
using System.Linq;
using Chummer.Datastructures;
using Chummer.UI.Options.ControlGenerators;

namespace Chummer.Backend.Options
{
    public class OptionCollectionCache
    {
        public SimpleTree<OptionItem> Tree { get; }
        public List<OptionItem> BookOptions { get; }
        public List<OptionItem> NotBookOptions { get; }
        public List<IOptionWinFromControlFactory> ControlFactories { get; }
        public List<OptionItem> SearchList { get; }
        public Dictionary<string, OptionDictionaryEntryProxy<string, bool>> BookEnabled { get; }
        public Dictionary<string, OptionGroup> Books { get; }

        public OptionCollectionCache(SimpleTree<OptionItem> tree, List<OptionItem> bookOptions, List<IOptionWinFromControlFactory> controlFactories)
        {
            Tree = tree;
            BookOptions = bookOptions;
            ControlFactories = controlFactories;
            NotBookOptions = tree.DepthFirstEnumerator().ToList();

            SearchList = BookOptions.Concat(NotBookOptions).ToList();
            BookEnabled = bookOptions.OfType<OptionGroup>()
                .Select(x => x.Children.OfType<OptionDictionaryEntryProxy<string, bool>>().First())
                .ToDictionary(x => x.Key);

            Books = BookOptions.OfType<OptionGroup>()
                .ToDictionary(x => x.Children.OfType<OptionDictionaryEntryProxy<string, bool>>().First().Key);

        }
    }
}