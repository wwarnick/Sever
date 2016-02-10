using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sever
{
	class NodeDestDistInt
	{
		/// <summary>The source Node.</summary>
		public Node Node { get; set; }

		/// <summary>The destination of the new node.</summary>
		public VectorF Dest { get; set; }

		/// <summary>The distance from the source node to the nearest intersection.</summary>
		public FInt Dist { get; set; }

		/// <summary>The number of intersections.</summary>
		public int Intersections { get; set; }

		/// <summary>Creates a new instance of NodeDestDistInt.</summary>
		/// <param name="node">The source Node.</param>
		/// <param name="dest">The destination of the new node.</param>
		/// <param name="dist">The distance from the source node to the nearest intersection.</param>
		/// <param name="intersections">The number of intersections.</param>
		public NodeDestDistInt(Node node, VectorF dest, FInt dist, int intersections)
		{
			this.Node = node;
			this.Dest = dest;
			this.Dist = dist;
			this.Intersections = intersections;
		}

		/// <summary>Creates a new instance of NodeDestDistInt.</summary>
		public NodeDestDistInt() : this(null, VectorF.Zero, FInt.F0, 0) { }
	}
}
