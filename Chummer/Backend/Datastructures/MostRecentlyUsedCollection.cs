using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    public class MostRecentlyUsedCollection<T> : ObservableCollectionWithMaxSize<T>
    {
        public MostRecentlyUsedCollection(int intMaxSize) : base(intMaxSize)
        {
        }

        public MostRecentlyUsedCollection(List<T> list, int intMaxSize) : base(list, intMaxSize)
        {
        }

        public MostRecentlyUsedCollection(IEnumerable<T> collection, int intMaxSize) : base(collection, intMaxSize)
        {
        }

        public override void Add(T item)
        {
            int intExistingIndex = IndexOf(item);
            if (intExistingIndex == -1)
                base.Add(item);
            else
                MoveItem(intExistingIndex, Count);
        }

        protected override void InsertItem(int index, T item)
        {
            int intExistingIndex = IndexOf(item);
            if (intExistingIndex == -1)
                base.InsertItem(index, item);
            else
                MoveItem(intExistingIndex, index);
        }
    }
}
