using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class NodeSkel : NodeBase
	{
		#region Members

		/// <summary>The player that owns this node.</summary>
		protected Player owner;

		/// <summary>The player that owns this node.</summary>
		public virtual Player Owner { get { return owner; } set { owner = value; } }

		/// <summary>This node's id.</summary>
		public string ID { get; set; }

		/// <summary>This node's type.</summary>
		public NodeType NType { get; set; }

		/// <summary>This node's position in the world.</summary>
		private VectorF pos;

		/// <summary>This node's position in the world.</summary>
		public VectorF Pos { get { return pos; } set { pos = value; } }

		/// <summary>This node's x-coordinate.</summary>
		public FInt X { get { return pos.X; } set { pos.X = value; } }

		/// <summary>This node's y-coordinate.</summary>
		public FInt Y { get { return pos.Y; } set { pos.Y = value; } }

		/// <summary>Whether or not this node is active.  Nodes, when first built, are inactive.  As soon as they are claimed by a segment, they become active.</summary>
		public bool Active { get; set; }

		/// <summary>Whether or not this node is visible.</summary>
		public bool Visible { get; set; }

		/// <summary>The time of the last check.</summary>
		public TimeSpan LastCheck { get; set; }

		/// <summary>The check number of the last check.</summary>
		public int LastCheckNum { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of NodeSkeleton.</summary>
		public NodeSkel()
			: base()
		{
			clear();
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public override void clear()
		{
			base.clear();

			Owner = null;
			ID = null;
			NType = null;
			Pos = VectorF.Zero;
			Active = false;
			Visible = false;
			LastCheck = TimeSpan.MinValue;
			LastCheckNum = 0;
		}

		/// <summary>Gets a skeleton version of this node.</summary>
		/// <returns>A skeleton version of this node.</returns>
		public NodeSkel getSkeleton()
		{
			NodeSkel skeleton = new NodeSkel();

			skeleton.Owner = this.Owner;
			skeleton.ID = this.ID;
			skeleton.NType = this.NType;
			skeleton.pos = this.pos;
			skeleton.Active = this.Active;
			skeleton.Radius = this.Radius;
			skeleton.GenSpacing = this.GenSpacing;
			skeleton.IsParent = this.IsParent;
			skeleton.SightDistance = this.SightDistance;
			skeleton.Spacing = this.Spacing;

			return skeleton;
		}

		#endregion Methods
	}
}
