using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class SegmentSkel
	{
		#region Members

		/// <summary>The possible segment end states</summary>
		public enum SegState { Complete, Building, Retracting }

		/// <summary>The player that owns this segment.</summary>
		protected Player owner;

		/// <summary>The player that owns this segment.</summary>
		public virtual Player Owner { get { return owner; } set { owner = value; } }

		/// <summary>This segment's id.</summary>
		public string ID { get; set; }

		/// <summary>The location of each end.</summary>
		public VectorF[] EndLoc { get; set; }

		/// <summary>The current length of the segment.</summary>
		public FInt CurLength { get; set; }

		/// <summary>The current state that each end of the segment is in.</summary>
		public SegState[] State { get; set; }

		/// <summary>Whether or not this segment is visible.</summary>
		public bool Visible { get; set; }

		/// <summary>The time of the last check.</summary>
		public TimeSpan LastCheck { get; set; }

		/// <summary>The check number of the last check.</summary>
		public int LastCheckNum { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of SegmentBase.</summary>
		public SegmentSkel()
		{
			clear();
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values back to their defaults.</summary>
		public virtual void clear()
		{
			Owner = null;
			ID = null;
			EndLoc = new VectorF[2];
			CurLength = FInt.F0;
			State = new SegState[2];
			State[0] = SegState.Complete;
			State[0] = SegState.Complete;
			Visible = false;
			LastCheck = TimeSpan.MinValue;
			LastCheckNum = 0;
		}

		/// <summary>Gets a skeleton version of this segment.</summary>
		/// <returns>A skeleton version of this segment.</returns>
		public SegmentSkel getSkeleton()
		{
			SegmentSkel skeleton = new SegmentSkel();

			skeleton.owner = this.Owner;
			skeleton.ID = this.ID;
			skeleton.EndLoc = (VectorF[])this.EndLoc.Clone();
			skeleton.CurLength = this.CurLength;
			skeleton.State = (SegState[])this.State.Clone();

			return skeleton;
		}

		/// <summary>Gets whether or not this segment is retracting on either end.</summary>
		/// <returns>Whether or not this segment is retracting on either end.</returns>
		public bool IsRetracting()
		{
			return State[0] == SegState.Retracting || State[1] == SegState.Retracting;
		}

		#endregion Methods
	}
}
