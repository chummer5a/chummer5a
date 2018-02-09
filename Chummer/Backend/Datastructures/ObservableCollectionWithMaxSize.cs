using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    public class ObservableCollectionWithMaxSize<T> : ObservableCollection<T>
    {
        private readonly int _intMaxSize;

        public ObservableCollectionWithMaxSize(int intMaxSize)
        {
            _intMaxSize = intMaxSize;
        }

        public ObservableCollectionWithMaxSize(List<T> list, int intMaxSize) : base(list)
        {
            _intMaxSize = intMaxSize;
            while (Count > _intMaxSize)
                RemoveAt(Count - 1);
        }

        public ObservableCollectionWithMaxSize(IEnumerable<T> collection, int intMaxSize) : base(collection)
        {
            _intMaxSize = intMaxSize;
            while (Count > _intMaxSize)
                RemoveAt(Count - 1);
        }

        public new virtual void Add(T item)
        {
            if (Count <= _intMaxSize)
                base.Add(item);
        }

        protected override void InsertItem(int index, T item)
        {
            if (index < _intMaxSize)
            {
                base.InsertItem(index, item);
                while (Count > _intMaxSize)
                    RemoveAt(Count - 1);
            }
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (newIndex >= _intMaxSize)
                newIndex = _intMaxSize;
            base.MoveItem(oldIndex, newIndex);
        }
    }
}
