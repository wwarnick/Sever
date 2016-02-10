using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class NodeType : NodeBase
	{
		#region Members

		/// <summary>This node type's id.</summary>
		public string ID { get; set; }

		/// <summary>This node type's name.</summary>
		public string Name { get; set; }

		/// <summary>The number of segments that may be attached to this node.</summary>
		public int NumSegments { get; set; }

		/// <summary>The The start of this node type's build range.</summary>
		public FInt BuildRangeMin { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of NodeType.</summary>
		public NodeType()
		{
			clear();
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public override void clear()
		{
			base.clear();

			ID = null;
			Name = null;
			NumSegments = 0;
			BuildRangeMin = FInt.F0;
		}

		#endregion Methods
	}
}
