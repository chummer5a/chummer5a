using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Chummer.Backend.Options;
using Chummer.UI.Options;
using Chummer.UI.Options.ControlGenerators;

namespace Chummer.Classes
{
    abstract class AbstractOptionTree
    {
        public string Name { get; }
        public List<AbstractOptionTree> Children { get; }= new List<AbstractOptionTree>();

        protected AbstractOptionTree(string name)
        {
            Name = name;
        }

        public abstract Control ControlLazy();

    }

    class SimpleOptionTree : AbstractOptionTree
    {
        public List<OptionRenderItem> Items { get; }
        private readonly List<IOptionWinFromControlFactory> _factories;
        private Lazy<Control> _lazyItem;

        public SimpleOptionTree(string name, List<OptionRenderItem> items, List<IOptionWinFromControlFactory> factories) : base(name)
        {
            Items = items;
            _factories = factories;
            if(string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name) +" needs to be a visible string");

            if(items == null) throw new ArgumentNullException(nameof(items));

            _lazyItem = new Lazy<Control>(GenerateItem);
        }

        public override Control ControlLazy() => _lazyItem.Value;

        private OptionRender GenerateItem()
        {
            OptionRender item = new OptionRender();
            item.Factories = _factories;
            item.SetContents(Items);
            return item;
        }
    }

    class DummyOptionTree : AbstractOptionTree
    {
        private Control p;
        
        public DummyOptionTree(string name) : base(name)
        {
        }
        
        
        public override Control ControlLazy()
        {
            return p ?? (p = new FlowLayoutPanel());
        }


    }

    class BookNode : AbstractOptionTree
    {
        //TODO: Recheck here
        /*
        * I think this needs a rewrite someday once i have a better idea on how this is supposed to work, but this saving methods probably won't end here.
        *
        */
        private readonly OptionCollectionCache _enabledBooks;
        private readonly Lazy<BookControl> _bookControl;
        public BookNode(OptionCollectionCache enabledBooks) : base(LanguageManager.GetString("String_Books"))
        {
            _enabledBooks = enabledBooks;
            _bookControl = new Lazy<BookControl>(() => new BookControl(_enabledBooks));
        }
        
        public override Control ControlLazy() => _bookControl.Value;
    }
}
