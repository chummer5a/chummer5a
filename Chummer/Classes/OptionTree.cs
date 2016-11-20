using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Chummer.UI.Options;

namespace Chummer.Classes
{
    abstract class AbstractOptionTree
    {
        public abstract bool Created { get; }
        public string Name { get; }
        public List<AbstractOptionTree> Children = new List<AbstractOptionTree>();

        protected AbstractOptionTree(string name)
        {
            Name = name;
        }

        public abstract Control ControlLazy();
        public abstract void Save();
    }

    class SimpleOptionTree : AbstractOptionTree
    {
        private readonly object _target;
        private readonly List<PropertyInfo> _properies;
        private OptionItem item = null;

        public SimpleOptionTree(string name, object target, List<PropertyInfo> properies) : base(name)
        {
            if(string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name) +" needs to be a visible string");

            if(target == null) throw new ArgumentNullException(nameof(target));
            if(properies == null || properies.Count == 0) throw new ArgumentException(nameof(properies));

            _target = target;
            _properies = properies.ToList();
        }

        public override bool Created => item != null;

        public override Control ControlLazy()
        {
            return item ?? (item = GenerateItem());
        }

        private OptionItem GenerateItem()
        {
            OptionItem item = new OptionItem();
            item.SetManyToSingleOptions(_properies,_target);
            return item;
        }

        public override void Save()
        {
            item?.WriteBack();
        }
    }

    class DummyOptionTree : AbstractOptionTree
    {
        private Control p;
        
        public DummyOptionTree(string name) : base(name)
        {
        }

        public override bool Created => p != null;
        
        public override Control ControlLazy()
        {
            return p ?? (p = new FlowLayoutPanel());
        }

        public override void Save()
        {
            
        }
    }

    class BookNode : AbstractOptionTree
    {
        private readonly HashSet<string> _enabledBooks;
        private readonly Lazy<BookControl> _bookControl;
        public BookNode(HashSet<string> enabledBooks) : base(LanguageManager.Instance.GetString("String_Books"))
        {
            _enabledBooks = enabledBooks;
            _bookControl = new Lazy<BookControl>(() => new BookControl(_enabledBooks));
        }

        public override bool Created => _bookControl.IsValueCreated;
        public override Control ControlLazy() => _bookControl.Value;

        public override void Save()
        {
            _bookControl.Value.Save();
        }
    }
}
