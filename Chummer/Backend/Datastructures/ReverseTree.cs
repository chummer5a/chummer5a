using System.Collections;
using System.Collections.Generic;
using System.Windows.Navigation;

namespace Chummer.Datastructures
{
	/// <summary>
	/// This class is for multiple objects that depend on one or more child objects
	/// When changing the child, this will allow easily fetching every impacted parrent
	/// </summary>
	internal class ReverseTree<T> : IEnumerable<T>
	{
		private T self;
		private ReverseTree<T> parent = null;
		private ReverseTree<T> root = null;
		private ReverseTree<T>[] children;
		private Dictionary<T, ReverseTree<T>> seachDictionary = new Dictionary<T, ReverseTree<T>>();
		public ReverseTree(T self, params ReverseTree<T>[] children)
		{
			this.self = self;
			seachDictionary.Add(self, this);

			foreach (ReverseTree<T> child in children)
			{
				child.parent = this;
				child.setRoot(this);
			}
		}



		private void setRoot(ReverseTree<T> root)
		{
			if (children != null)
			{
				foreach (ReverseTree<T> reverseTree in children)
				{
					reverseTree.setRoot(root);
				}
			}
			this.root = root;
			foreach (KeyValuePair<T, ReverseTree<T>> keyValuePair in seachDictionary)
			{
				root.seachDictionary.Add(keyValuePair.Key, keyValuePair.Value);
			}
			seachDictionary = root.seachDictionary;
		}

		public ReverseTree<T> Find(T key)
		{
			ReverseTree<T> ret;
			if (seachDictionary.TryGetValue(key, out ret))
			{
				return ret;
			}

			return new ReverseTree<T>(key); // single tree with only key
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			ReverseTree<T> current = this;

			yield return current.self;
			while (current.parent != null)
			{
				current = current.parent;
				yield return current.self;
			}

		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
