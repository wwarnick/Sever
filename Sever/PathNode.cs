using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class PathNode : IEnumerable<PathEdge>
	{
		#region Members

		/// <summary>This node's index.</summary>
		public int Index { get; set; }

		/// <summary>The position of this path node on the map.</summary>
		public VectorD Position;

		/// <summary>The first edge in the list of edges.</summary>
		public PathEdge FirstEdge { get; private set; }

		/// <summary>The last edge in the list of edges.</summary>
		public PathEdge LastEdge { get; private set; }

		/// <summary>The number of edges leading from this node.</summary>
		public int NumEdges { get; private set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of PathNode.</summary>
		public PathNode()
		{
			clear();
		}

		/// <summary>Creates a new instance of PathNode.</summary>
		/// <param name="index">This node's index.</param>
		public PathNode(int index)
			: this()
		{
			Index = index;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public void clear()
		{
			Index = -1;
			Position = VectorD.Zero;
			FirstEdge = null;
			LastEdge = null;
			NumEdges = 0;
		}

		/// <summary>Adds the provided edge to the beginning of the list.</summary>
		/// <param name="edge">The edge to add.</param>
		public void addFirst(PathEdge edge)
		{
			edge.Previous = null;
			edge.Next = FirstEdge;

			if (FirstEdge != null)
				FirstEdge.Previous = edge;
			else
				LastEdge = edge;

			FirstEdge = edge;

			NumEdges++;
		}

		/// <summary>Adds the provided edge to the end of the list.</summary>
		/// <param name="edge">The edge to add.</param>
		public void addLast(PathEdge edge)
		{
			edge.Next = null;
			edge.Previous = LastEdge;

			if (LastEdge != null)
				LastEdge.Next = edge;
			else
				FirstEdge = edge;

			LastEdge = edge;

			NumEdges++;
		}

		/// <summary>Removes the first edge from the list.</summary>
		public void removeFirst()
		{
			if (NumEdges == 1)
			{
				FirstEdge = null;
				LastEdge = null;
				NumEdges = 0;
			}
			else if (NumEdges > 0)
			{
				FirstEdge = FirstEdge.Next;
				FirstEdge.Previous = null;
				NumEdges--;
			}
			// else if (NumEdges == 0) do nothing
		}

		/// <summary>Removes the last edge from the list.</summary>
		public void removeLast()
		{
			if (NumEdges == 1)
			{
				FirstEdge = null;
				LastEdge = null;
				NumEdges = 0;
			}
			else if (NumEdges > 0)
			{
				LastEdge = LastEdge.Previous;
				LastEdge.Next = null;
				NumEdges--;
			}
			// else if (NumEdges == 0) do nothing
		}

		/// <summary>Creates a shallow copy of the PathNode.</summary>
		/// <returns>A shallow copy of the PathNode.</returns>
		public PathNode Clone()
		{
			PathNode clone = new PathNode(this.Index);

			clone.Position = this.Position;
			clone.NumEdges = this.NumEdges;

			if (this.FirstEdge != null)
			{
				clone.FirstEdge = this.FirstEdge.Clone();
				clone.LastEdge = clone.FirstEdge;
				PathEdge prev = clone.FirstEdge;

				for (PathEdge edge = this.FirstEdge.Next; edge != null; edge = edge.Next)
				{
					clone.LastEdge = edge.Clone();

					clone.LastEdge.Previous = prev;
					prev.Next = clone.LastEdge;
					prev = clone.LastEdge;
				}
			}

			return clone;
		}

		/// <summary>Returns true if this PathNode has no edges or all of its edges are intersected.</summary>
		public bool isOrphaned()
		{
			if (FirstEdge == null)
				return true;

			for (PathEdge edge = FirstEdge; edge != null; edge = edge.Next)
			{
				if (edge.NumIntersections == 0)
					return false;
			}

			return true;
		}

		#region IEnumerable<T> Members

		/// <summary>Returns an enumerator that allows iteration through this PathNode</summary>
		/// <returns>An enumerator that allows iteration through this PathNode</returns>
		public IEnumerator<PathEdge> GetEnumerator()
		{
			for (PathEdge node = FirstEdge; node != null; node = node.Next)
			{
				yield return node;
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
