using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class LList<T> : IEnumerable<T>
	{
		#region Members

		/// <summary>The number of elements in this linked list.</summary>
		public int Count { get; private set; }

		/// <summary>The first element in the linked list.</summary>
		public LListNode<T> First { get; private set; }

		/// <summary>The last element in the linked list.</summary>
		public LListNode<T> Last { get; private set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of LList.</summary>
		public LList()
		{
			Clear();
		}

		#endregion Constructors

		#region Methods

		/// <summary>Empty this linked list.</summary>
		public void Clear()
		{
			First = null;
			Last = null;
			Count = 0;
		}

		/// <summary>Creates a shallow copy of the LList.</summary>
		/// <returns>A shallow copy of the LList.</returns>
		public LList<T> Clone()
		{
			LList<T> newList = new LList<T>();

			foreach (T node in this)
			{
				newList.AddLast(node);
			}

			return newList;
		}

		/// <summary>Add the provided item to the beginning of the linked list.</summary>
		/// <param name="item">The item to add.</param>
		public void AddFirst(T item)
		{
			LListNode<T> node = new LListNode<T>(item);
			node.List = this;
			node.Next = First;

			if (First != null)
				First.Previous = node;
			else
				Last = node;
			
			First = node;
			Count++;
		}

		/// <summary>Add the provided item to the beginning of the linked list.</summary>
		/// <param name="item">The item to add.</param>
		public void AddFirst(LListNode<T> item)
		{
			item.List = this;
			item.Previous = null;
			item.Next = First;

			if (First != null)
				First.Previous = item;
			else
				Last = item;

			First = item;
			Count++;
		}

		/// <summary>Add the provided item to the end of the list.</summary>
		/// <param name="item">The item to add.</param>
		public void AddLast(T item)
		{
			LListNode<T> node = new LListNode<T>(item);
			node.List = this;
			node.Previous = Last;

			if (Last != null)
				Last.Next = node;
			else
				First = node;

			Last = node;
			Count++;
		}

		/// <summary>Add the provided item to the end of the linked list.</summary>
		/// <param name="item">The item to add.</param>
		public void AddLast(LListNode<T> item)
		{
			item.List = this;
			item.Next = null;
			item.Previous = Last;

			if (Last != null)
				Last.Next = item;
			else
				First = item;

			Last = item;
			Count++;
		}

		/// <summary>Remove the first item in the list.</summary>
		public void RemoveFirst()
		{
			First = First.Next;

			if (First != null)
				First.Previous = null;
			else
				Last = null;

			Count--;
		}

		/// <summary>Remove the last item in the list.</summary>
		public void RemoveLast()
		{
			Last = Last.Previous;

			if (Last != null)
				Last.Next = null;
			else
				First = null;

			Count--;
		}

		#region IEnumerable<T> Members

		/// <summary>Returns an enumerator that allows iteration through this LList</summary>
		/// <returns>An enumerator that allows iteration through this LList</returns>
		public IEnumerator<T> GetEnumerator()
		{
			for (LListNode<T> node = First; node != null; node = node.Next)
			{
				yield return node.Value;
			}
		}

		#endregion

		#region IEnumerable Members

		/// <summary>This will never be called; it is required by the IEnumerable interface</summary>
		/// <exception cref="Exception">Thrown when this function is called; this function should never be called</exception>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new Exception("System.Collections.IEnumerable.GetEnumerator() should never be called.");
		}

		#endregion

		#endregion Methods
	}
}
