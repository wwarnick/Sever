using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sever
{
	public class Hotspot
	{
		/// <summary>This Hotspot's id.</summary>
		public string ID { get; set; }

		/// <summary>The world that contains this node.</summary>
		public World InWorld { get; set; }
		
		/// <summary>This Hotspot's position in the world.</summary>
		private VectorF pos;

		/// <summary>This Hotspot's position in the world.</summary>
		public VectorF Pos { get { return pos; } set { pos = value; } }

		/// <summary>This Hotspot's x-coordinate.</summary>
		public FInt X { get { return pos.X; } set { pos.X = value; } }

		/// <summary>This Hotspot's y-coordinate.</summary>
		public FInt Y { get { return pos.Y; } set { pos.Y = value; } }

		/// <summary>A script to run when a player builds on top of it.</summary>
		public string Script { get; set; }

		/// <summary>The time of the last check.</summary>
		public TimeSpan LastCheck { get; set; }

		/// <summary>The check number of the last check.</summary>
		public int LastCheckNum { get; set; }

		/// <summary>Creates a new instance of Hotspot.</summary>
		public Hotspot(World inWorld)
		{
			ID = null;
			InWorld = inWorld;
			Pos = VectorF.Zero;
			Script = null;
			LastCheck = TimeSpan.MinValue;
			LastCheckNum = 0;
		}

		/// <summary>Creates a shallow copy of this Hotspot.</summary>
		/// <returns>A shallow copyof this Hotspot.</returns>
		public Hotspot Clone()
		{
			Hotspot hotspot = new Hotspot(this.InWorld);
			hotspot.ID = this.ID;
			hotspot.Pos = this.Pos;
			hotspot.Script = this.Script;
			hotspot.LastCheck = this.LastCheck;
			hotspot.LastCheckNum = this.LastCheckNum;

			return hotspot;
		}
	}
}
