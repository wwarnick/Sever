using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	/// <summary>
	/// The <tt>PQCollection</tt> class represents a priority queue of generic keys.
	/// It supports the usual <em>insert</em> and <em>de-queue</em>
	/// operations, along with methods for peeking at the next key to de-queue,
	/// testing if the priority queue is empty, and iterating through
	/// the keys.
	/// <para/>
	/// This implementation uses a binary heap.
	/// The <em>insert</em> and <em>de-queue</em> operations take
	/// logarithmic amortized time.
	/// The <em>peek</em>, <em>size</em>, and <em>is-empty</em> operations take constant time.
	/// Construction takes time proportional to the number of items used to initialize the data structure.
	/// <para/>
	/// For additional documentation, see <a href="http://algs4.cs.princeton.edu/24pq">Section 2.4</a> of
	/// <i>Algorithms, fourth Edition</i> by Robert Sedgewick and Kevin Wayne.
	/// </summary>
	/// <typeparam name="T">The type of objects to be inserted in the queue</typeparam>
	/// <remarks>Implements ICollection so that <code>Linq</code> extension methods and aggregates can be used.</remarks>
	public abstract class PQCollection<T> : ICollection<T>
	{
		/// <summary>
		/// Store items at indices 1 to itemCount.
		/// </summary>
		private List<T> pq;

		/// <summary>
		/// Comparator used for determining order and equality.
		/// </summary>
		private IComparer<T> comparator;

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="PQCollection{T}"/> class with the given initial capacity.
		/// </summary>
		/// <param name="initialCapacity">The initial capacity of the priority queue.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the specified initial capacity is <code>int.MaxValue</code></exception>
		protected PQCollection(int initialCapacity)
		{
#if DEBUG
			if (int.MaxValue == initialCapacity || 0 > initialCapacity)
			{
				throw new ArgumentOutOfRangeException("initialCapacity", "Capacity must be in the [0, int.MaxValue) range.");
			}

			// TODO: Prefer compile-time error to run-time error.
			// Split methods so that they either take an IComparable or are generic and use an IComparer.
			if (!typeof(IComparable).IsAssignableFrom(typeof(T)))
			{
				throw new InvalidOperationException(
				   "The items in the queue do not have a default comparer. A Priority Queue cannot be constructed.");
			}
#endif

			this.comparator = Comparer<T>.Default;

#if DEBUG
			if (null == this.comparator)
			{
				throw new InvalidOperationException("A valid comparator is not available.");
			}
#endif

			this.pq = new List<T>(initialCapacity + 1);
			this.pq.Add(default(T)); // 0th item
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PQCollection{T}"/> class with the given initial capacity
		/// and a given comparator.
		/// </summary>
		/// <param name="initialCapacity">The initial capacity of the priority queue.</param>
		/// <param name="comparator">The order in which to compare the keys.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the specified initial capacity is <code>int.MaxValue</code></exception>
		protected PQCollection(int initialCapacity, IComparer<T> comparator)
		{
#if DEBUG
			if (int.MaxValue == initialCapacity || 0 > initialCapacity)
			{
				throw new ArgumentOutOfRangeException("initialCapacity", "Capacity must be in the [0, int.MaxValue) range.");
			}
#endif

			this.comparator = comparator;

#if DEBUG
			if (null == this.comparator)
			{
				throw new InvalidOperationException("A valid comparator is not available.");
			}
#endif

			this.pq = new List<T>(initialCapacity + 1);
			this.pq.Add(default(T)); // 0th item
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PQCollection{T}"/> class from an array of keys.
		/// Takes time proportional to the number of keys, using sink-based heap construction.
		/// </summary>
		/// <param name="keys">The array of keys.</param>
		protected PQCollection(T[] keys)
		{
#if DEBUG
			// TODO: Prefer compile-time error to run-time error.
			// Split methods so that they either take an IComparable or are generic and use an IComparer.
			if (!typeof(IComparable).IsAssignableFrom(typeof(T)))
			{
				throw new InvalidOperationException(
				   "The items in the queue do not have a default comparer. A Priority Queue cannot be constructed.");
			}
#endif

			this.comparator = Comparer<T>.Default;

#if DEBUG
			if (null == this.comparator)
			{
				throw new InvalidOperationException("A valid comparator is not available.");
			}
#endif

			this.pq = new List<T>(keys.Length + 1);
			this.pq.Add(default(T));  // 0th item
			this.pq.AddRange(keys);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets an enumerator for fast traversal of the heap items as a non-ordered set.
		/// </summary>
		/// <returns>An enumerator for traversal of the heap items.</returns>
		/// <remarks>
		/// The enumeration is non-destructive, it will not cause any item to be removed.
		/// The enumeration is non-ordered, the results may be unsorted.
		/// </remarks>
		public IEnumerator<T> Keys
		{
			get
			{
				for (int i = 1; i <= this.Count; i++)
				{
					yield return this.pq[i];
				}
			}
		}

		/// <summary>
		/// Gets the number of items in the <see cref="PQCollection"/>.
		/// </summary>
		public int Count
		{
			get
			{
				return this.pq.Count - 1;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the collection is read-only.
		/// </summary>
		/// <remarks>This is always false.</remarks>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets the comparator used to determine ordering
		/// </summary>
		protected IComparer<T> Comparator
		{
			get { return this.comparator; }
		}
		#endregion

		#region IQueue implementation (methods)
		#region ICollection<T> implementation (methods)
		/// <summary>
		/// Add an item to the priority queue.
		/// </summary>
		/// <param name="item">New item to be inserted into the queue.</param>
		/// <remarks>
		/// This is required for the <see cref="ICollection{T}"/> interface however, 
		/// using Enqueue is preferred.
		/// </remarks>
		[Obsolete("Preferred: Use the Enqueue method instead.")]
		public void Add(T item)
		{
			this.Enqueue(item);
		}

		/// <summary>
		/// Remove all items from the priority queue.
		/// </summary>
		public void Clear()
		{
			this.pq.RemoveRange(1, this.Count);
			this.pq.TrimExcess();
		}

		/// <summary>
		/// Find if an item is present in the priority queue.
		/// </summary>
		/// <param name="item">The item to search in the queue.</param>
		/// <returns>True if the item is found, false otherwise.</returns>
		public bool Contains(T item)
		{
			if (null == (object)item)
			{
				for (int i = 1; this.Count >= i; i++)
				{
					if (null == (object)this.pq[i])
					{
						return true;
					}
				}
			}
			else
			{
				for (int i = 1; this.Count >= i; i++)
				{
					if (0 == this.comparator.Compare(this.pq[i], item))
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Copy all the items in the Priority Queue to an array, starting at a specified position.
		/// </summary>
		/// <param name="array">The destination array.</param>
		/// <param name="arrayIndex">
		/// The index of the first element in the destination that will be overwritten.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the index is negative.</exception>
		/// <exception cref="ArgumentException">Thrown if the destination has insufficient capacity.</exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
#if DEBUG
			if (0 > arrayIndex)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}

			if (array.Length - arrayIndex < this.Count)
			{
				throw new ArgumentException("The destination array has insufficient capacity.");
			}
#endif

			System.Threading.Tasks.Parallel.For(0, this.Count, i => array[arrayIndex + i] = this.pq[i + 1]);
		}

		/// <summary>
		/// Removes the item at the top of the heap if it matches the specified item.
		/// </summary>
		/// <param name="item">The item to match.</param>
		/// <returns>True if there was a match, false otherwise.</returns>
		/// <remarks>
		/// Removal of items that are not at the top of the heap is not supported.
		/// Even if the specified item is present in the queue, if it is not at the top 
		/// the removal will fail and this method will return false.
		/// </remarks>
		public bool Remove(T item)
		{
			if (0 == this.comparator.Compare(this.Peek(), item))
			{
				this.Dequeue();
				return true;
			}
			else
			{
				return false;
			}
		}

		#region IEnumerable<T> implementation
		/// <summary>
		/// Get an enumerator for ordered traversal of the heap items.
		/// </summary>
		/// <returns>An enumerator for traversal of the heap items.</returns>
		/// <remarks>
		/// The enumeration is non-destructive, it will not cause any item to be removed.
		/// The enumeration is ordered, the queue items will be sorted.
		/// </remarks>
		public abstract IEnumerator<T> GetEnumerator();
		#endregion

		#region IEnumerable implementation
		/// <summary>
		/// Get an enumerator for ordered traversal of the heap items.
		/// </summary>
		/// <returns>An enumerator for traversal of the heap items.</returns>
		/// <remarks>
		/// The enumeration is non-destructive, it will not cause any item to be removed.
		/// The enumeration is ordered, the queue items will be sorted.
		/// </remarks>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		#endregion
		#endregion

		/// <summary>
		/// Remove the topmost key on the priority queue heap.
		/// </summary>
		/// <returns>The topmost key on the priority queue.</returns>
		/// <exception cref="NoSuchElementException">Thrown if the priority queue is empty.</exception>
		public virtual T Dequeue()
		{
#if DEBUG
			if (0 == this.Count)
				throw new Exception("Priority queue underflow.");
#endif

			T top = this.pq[1];
			this.pq[1] = this.pq[this.Count];
			this.pq.RemoveAt(this.Count);
			this.Sink(1);
#if DEBUG
			Debug.Assert(this.IsOrderedHeap(), "The heap is not properly ordered.");
#endif
			return top;
		}

		/// <summary>
		/// Add a key to the priority queue.
		/// </summary>
		/// <param name="item">The item that will be added to the priority queue.</param>
		public virtual void Enqueue(T item)
		{
			// add x, and percolate it up to maintain heap invariant
			this.pq.Add(item);
			this.Swim(this.Count);
#if DEBUG
			if (!this.IsOrderedHeap())
				throw new Exception("The heap is not properly ordered.");
#endif
		}

		/// <summary>
		/// Get the topmost key on the priority queue heap.
		/// </summary>
		/// <returns>The topmost key on the priority queue.</returns>
		/// <exception cref="NoSuchElementException">Thrown if the priority queue is empty.</exception>
		public T Peek()
		{
#if DEBUG
			if (0 == this.Count)
				throw new Exception("Priority queue underflow.");
#endif

			return this.pq[1];
		}
		#endregion

		#region Original API (obsolete, there are preferred replacement methods in the IQueue<T> interface)
		/*// <summary>
		/// Get the number of keys on the priority queue.
		/// </summary>
		/// <returns>The number of keys on the priority queue.</returns>
		[Obsolete("Use the Count property instead.")]
		public int Size()
		{
			return this.Count;
		}

		/// <summary>
		/// Adds a new key to the priority queue.
		/// </summary>
		/// <param name="newKey">The new key to add to the priority queue.</param>
		[Obsolete("Use the Enqueue(newKey) method instead.")]
		public void Insert(T newKey)
		{
			this.Enqueue(newKey);
		}

		/// <summary>
		/// Is the priority queue empty?
		/// </summary>
		/// <returns>True if the priority queue is empty, false otherwise.</returns>
		[Obsolete("Compare the Count property to zero instead.")]
		public bool IsEmpty()
		{
			return 0 == this.Count;
		}

		/// <summary>
		/// Returns an iterator that iterates over the keys on the priority queue
		/// in descending order.
		/// The iterator doesn't implement <tt>remove()</tt> since it's optional.
		/// </summary>
		/// <returns>An iterator that iterates over the keys in order.</returns>
		[Obsolete("Use the GetEnumerator() method instead.")]
		public IEnumerator Iterator()
		{
			return this.GetEnumerator();
		}*/
		#endregion

		#region Methods for restoring the heap invariant after a temporary break.
		/// <summary>
		/// Bottom-up restoration of the heap invariant.
		/// </summary>
		/// <param name="nodeIndex">Index of the node that may have broken the heap invariant.</param>
		public void Swim(int nodeIndex)
		{
			while (nodeIndex > 1 && this.PerformCompare(nodeIndex / 2, nodeIndex))
			{
				this.Exch(nodeIndex, nodeIndex / 2);
				nodeIndex = nodeIndex / 2;
			}
		}

		/// <summary>
		/// Top-down restoration of the heap invariant.
		/// </summary>
		/// <param name="nodeIndex">Index of the node that may have broken the heap invariant.</param>
		public void Sink(int nodeIndex)
		{
#if DEBUG
			if (int.MaxValue / 2 <= nodeIndex)
				throw new ArgumentException("Node index overflow", "nodeIndex");
#endif

			while (2 * nodeIndex <= this.Count)
			{
				int j = 2 * nodeIndex;
				if (j < this.Count && this.PerformCompare(j, j + 1))
					j++;

				if (!this.PerformCompare(nodeIndex, j))
					break;

				this.Exch(nodeIndex, j);
				nodeIndex = j;
			}
		}
		#endregion

		#region Methods for compares and swaps.
		/// <summary>
		/// Access an item from the internal queue.
		/// </summary>
		/// <param name="index">The index of the item to retrieve.</param>
		/// <returns>The item at the specified index.</returns>
		protected T GetPQItem(int index)
		{
#if DEBUG
			if (1 > index)
				throw new ArgumentOutOfRangeException("index");
#endif
			return this.pq[index];
		}

		/// <summary>
		/// Compare elements at the two specified indices.
		/// </summary>
		/// <param name="firstIndex">Index of the first item to compare.</param>
		/// <param name="secondIndex">Index of the second item to compare.</param>
		/// <returns>True if the first item is smaller than the second one, false otherwise.</returns>
		protected abstract bool PerformCompare(int firstIndex, int secondIndex);

		/// <summary>
		/// Swap items at the two specified indices
		/// </summary>
		/// <param name="firstIndex">Index of the first item to compare.</param>
		/// <param name="secondIndex">Index of the second item to compare.</param>
		protected virtual void Exch(int firstIndex, int secondIndex)
		{
			T swap = this.pq[firstIndex];
			this.pq[firstIndex] = this.pq[secondIndex];
			this.pq[secondIndex] = swap;
		}
		#endregion

		#region Validation methods for debugging (for using in assertions)
		/// <summary>
		/// Determine if the heap invariants are satisfied.
		/// </summary>
		/// <returns>True if the current state is a correct heap, false otherwise.</returns>
		private bool IsOrderedHeap()
		{
			return this.IsOrderedHeap(1);
		}

		/// <summary>
		/// Determine if the heap invariants are satisfied for the sub-tree rooted at a specified index.
		/// </summary>
		/// <param name="rootIndex">The index for the root of the search.</param>
		/// <returns>True if the current state is a correct heap, false otherwise.</returns>
		private bool IsOrderedHeap(int rootIndex)
		{
			if (rootIndex > this.Count)
			{
				return true;
			}

			int left = 2 * rootIndex;
			int right = left + 1;

			if (left <= this.Count && this.PerformCompare(rootIndex, left))
			{
				return false;
			}

			if (right <= this.Count && this.PerformCompare(rootIndex, right))
			{
				return false;
			}

			return this.IsOrderedHeap(left) && this.IsOrderedHeap(right);
		}
		#endregion
	}
}
