using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Chummer.Backend.Datastructures
{
    class RTree<T>
    {
        private readonly int _nodesize;
        private RTreeNode<T> _root;

        public RTree(int nodesize = 3)
        {
            _nodesize = nodesize;
            _root = new RTreeNode<T>(this);
        }

        public void Insert(T item, Rectangle location)
        {
            RTreeNode<T> newleaf = new RTreeNode<T>(item, location, this);
            List<RTreeNode<T>> stack = new List<RTreeNode<T>> {_root};

            while (stack.Last().IsLeaf() == false)
            {
                stack.Add(stack.Last().BestChild(newleaf));
            }

            Add(stack, newleaf);
        }

        private void Remove(T item)
        {
            throw new NotImplementedException();
        }

        public T Find(Point point)
        {
            return _root.Under(point);
        }

        public Rectangle Size => _root.Location;

        private void Add(List<RTreeNode<T>> stack, RTreeNode<T> newleaf)
        {
            RTreeNode<T> leaf = stack.Last().Add(newleaf);

            if (leaf != null)
            {
                if (stack.Count > 1)
                {
                    stack.RemoveAt(stack.Count - 1);
                    Add(stack, leaf);
                }
                else
                {
                    RTreeNode<T> newroot = new RTreeNode<T>(this);
                    newroot.Add(_root);
                    newroot.Add(leaf);
                    _root = newroot;

                }
            }
        }

        private class RTreeNode<T>
        {
            public Rectangle Location { get; private set; }

            private readonly List<RTreeNode<T>> _children = new List<RTreeNode<T>>();
            private readonly T _content = default(T);
            private readonly RTree<T> _parrent;

            public RTreeNode(RTree <T>parrent)
            {
                _parrent = parrent;
            }

            public RTreeNode(T item, Rectangle location,  RTree<T> parrent) : this(parrent)
            {
                _content = item;
                Location = location;
            }

            private RTreeNode(List<RTreeNode<T>> children, RTree<T> parrent) : this(parrent)
            {
                _children = children;
                RecalculateBoundingBox();
            }

            public bool IsLeaf()
            {
                return _children.Count == 0 || _children.Any(x => x._children.Count > 0 || x._content != null);
            }

            private int InsertionCost(RTreeNode<T> newleaf)
            {
                int cost = 0;

                cost += Math.Min(0, Location.Top - newleaf.Location.Top);
                cost += Math.Min(0, Location.Left - newleaf.Location.Left);

                cost += Math.Min(0, newleaf.Location.Bottom - Location.Bottom);
                cost += Math.Min(0, newleaf.Location.Right - Location.Right);

                return cost;
            }

            public RTreeNode<T> Add(RTreeNode<T> newleaf)
            {
                if (_content != null) throw new InvalidOperationException("FIX LATER");
                _children.Add(newleaf);
                if (_children.Count <= _parrent._nodesize)
                {
                    RecalculateBoundingBox();
                    return null;
                }
                else
                {
                    List<RTreeNode<T>> newNode = _children.OrderByDescending(x => x.Location.Top).Take(_children.Count/2).ToList();
                    _children.RemoveAll(x => newNode.Contains(x));

                    RecalculateBoundingBox();
                    return new RTreeNode<T>(newNode, _parrent);
                }
            }

            private void RecalculateBoundingBox()
            {
                Location = Rectangle.FromLTRB(
                    _children.Max(x => x.Location.Left),
                    _children.Max(x => x.Location.Top),
                    _children.Max(x => x.Location.Right),
                    _children.Max(x => x.Location.Bottom)
                );
            }

            public RTreeNode<T> BestChild(RTreeNode<T> newleaf)
            {
                return _children.Where(x => x._content == null).OrderByDescending(x => x.InsertionCost(newleaf)).First();
            }

            public T Under(Point point)
            {
                if(_content != null && Location.Contains(point)) return _content;

                return _children.Select(rTreeNode => rTreeNode.Under(point)).FirstOrDefault(child => child != null);
            }
        }
    }

}