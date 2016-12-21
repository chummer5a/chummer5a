using System.Collections.Generic;
using System.Linq;

namespace Chummer.Datastructures
{
    public class SimpleTree<T>
    {
        public List<SimpleTree<T>> Children { get; } = new List<SimpleTree<T>>();
        public List<T> Leafs { get; } = new List<T>();
        public object Tag { get; set; }

        public IEnumerable<T> DepthFirstEnumerator()
        {
            foreach (T child in Children.SelectMany(x => x.DepthFirstEnumerator()))
            {
                yield return child;
            }

            foreach (T child in Leafs)
                yield return child;
        }

    }
}