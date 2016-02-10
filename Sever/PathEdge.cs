using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class PathEdge
	{
		#region Members

		/// <summary>The path finder that contains this edge.</summary>
		public PathFinder Path { get; set; }

		/// <summary>The source path node.</summary>
		public int NodeSrc { get; set; }

		/// <summary>The destination path node.</summary>
		public int NodeDest { get; set; }

		/// <summary>The distance between the two nodes.</summary>
		public double Distance { get; private set; }

		/// <summary>The number of objects in the map that intersect this PathEdge.</summary>
		public int NumIntersections { get; set; }

		/// <summary>The previous edge in the list.</summary>
		public PathEdge Previous { get; set; }

		/// <summary>The next edge in the list.</summary>
		public PathEdge Next { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of PathEdge.</summary>
		/// <param name="path">The path finder that contains this edge.</param>
		public PathEdge(PathFinder path)
		{
			clear();

			Path = path;
		}

		/// <summary>Creates a new instance of PathEdge.</summary>
		/// <param name="path">The path finder that contains this edge.</param>
		/// <param name="nodeSrc">The source path node.</param>
		/// <param name="nodeDest">The destination path node.</param>
		public PathEdge(PathFinder path, int nodeSrc, int nodeDest)
			: this(path)
		{
			NodeSrc = nodeSrc;
			NodeDest = nodeDest;
			updateMath();
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public void clear()
		{
			NodeSrc = -1;
			NodeDest = -1;
			Distance = 0d;
			NumIntersections = 0;
			Previous = null;
			Next = null;
		}

		/// <summary>Updates the math.</summary>
		public void updateMath()
		{
			Distance = VectorD.Distance(Path.Nodes[NodeSrc].Position, Path.Nodes[NodeDest].Position);
		}

		/// <summary>Creates a shallow copy of the PathEdge.</summary>
		/// <returns>A shallow copy of the PathEdge.</returns>
		public PathEdge Clone()
		{
			PathEdge clone = new PathEdge(Path, this.NodeSrc, this.NodeDest);
			clone.Distance = this.Distance;

			return clone;
		}

		#endregion Methods
	}
}
