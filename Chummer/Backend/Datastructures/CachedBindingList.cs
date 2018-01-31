using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    public class CachedBindingList<T> : BindingList<T>
    {
        public delegate void BeforeRemoveEventHandler(object sender, RemovingOldEventArgs e);
        public event BeforeRemoveEventHandler BeforeRemove;

        protected override void RemoveItem(int intIndex)
        {
            BeforeRemove?.Invoke(this, new RemovingOldEventArgs(Items[intIndex], intIndex));

            base.RemoveItem(intIndex);
        }

        protected override void ClearItems()
        {
            if (BeforeRemove != null)
                for (int i = 0; i < Items.Count; ++i)
                    BeforeRemove.Invoke(this, new RemovingOldEventArgs(Items[i], i));
            base.ClearItems();
        }

        protected override void SetItem(int index, T item)
        {
            if (BeforeRemove != null)
            {
                T objOldItem = Items[index];
                if (!objOldItem.Equals(item))
                {
                    BeforeRemove.Invoke(this, new RemovingOldEventArgs(objOldItem, index));
                }
            }
            base.SetItem(index, item);
        }
    }

    public class RemovingOldEventArgs : EventArgs
    {
        public RemovingOldEventArgs()
        {
            OldObject = null;
            OldIndex = 0;
        }

        public RemovingOldEventArgs(object objOldObject, int intOldIndex)
        {
            OldObject = objOldObject;
            OldIndex = intOldIndex;
        }
        
        public object OldObject { get; set; }
        public int OldIndex { get; set; }
    }
}
