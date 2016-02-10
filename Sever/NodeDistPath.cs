using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sever
{
	class NodeDistPath
	{
		/// <summary>A Node object.</summary>
		public Node Node { get; set; }

		/// <summary>The distance from the node to the destination.</summary>
		public FInt Dist { get; set; }

		/// <summary>The shortest path from the Node to the destination.</summary>
		public List<PathEdge> Path { get; set; }

		/// <summary>Creates a new instance of NodeDistPath.</summary>
		/// <param name="node">A Node object.</param>
		/// <param name="dist">The distance from the node to the destination.</param>
		/// <param name="path">The shortest path from the Node to the destination.</param>
		public NodeDistPath(Node node, FInt dist, List<PathEdge> path)
		{
			this.Node = node;
			this.Dist = dist;
			this.Path = path;
		}

		/// <summary>Creates a new instance of NodeDistPath.</summary>
		public NodeDistPath() : this(null, FInt.F0, null) { }

		public static bool operator <(NodeDistPath one, NodeDistPath other)
		{
			return one.Dist < other.Dist;
		}

		public static bool operator >(NodeDistPath one, NodeDistPath other)
		{
			return one.Dist > other.Dist;
		}
	}
}
