using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public abstract class NodeBase
	{
		#region Members

		/// <summary>Whether or not this node is a parent.</summary>
		public bool IsParent { get; set; }

		/// <summary>This node's radius.</summary>
		public FInt Radius { get; set; }

		/// <summary>The amount of space required between this node and any other.</summary>
		public FInt Spacing { get; set; }

		/// <summary>How much time to elapse between generating new people.</summary>
		public FInt GenSpacing { get; set; }

		/// <summary>The distance that this node can see.</summary>
		public FInt SightDistance { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of NodeBase.</summary>
		public NodeBase()
		{
			clear();
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public virtual void clear()
		{
			IsParent = false;
			Radius = FInt.F0;
			Spacing = FInt.F0;
			GenSpacing = FInt.F0;
			SightDistance = FInt.F0;
		}

		#endregion Methods
	}
}
