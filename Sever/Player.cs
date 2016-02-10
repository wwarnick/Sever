using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sever
{
	public class Player
	{
		#region Members

		/// <summary>The possible player types.</summary>
		public enum PlayerType { Human, Network, Computer }

		/// <summary>The world that this player is in.</summary>
		public World InWorld { get; set; }

		/// <summary>This player's id.</summary>
		public string ID { get; set; }

		/// <summary>The type of player.</summary>
		public PlayerType Type { get; set; }

		/// <summary>The player's name.</summary>
		public string Name { get; set; }

		/// <summary>All nodes owned by this player.</summary>
		public List<Node> Nodes { get; private set; }

		/// <summary>All segments owned by this player.</summary>
		public List<Segment> Segments { get; private set; }

		/// <summary>The active fog of war manager.</summary>
		public FogOfWar Fog { get; set; }

		/// <summary>The base for this player's AI thread.  Only used by Computer players.</summary>
		public AIThread AIThreadBase { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of Player.</summary>
		/// <param name="inWorld">The world that this player is in.</param>
		public Player(World inWorld)
		{
			clear();
			InWorld = inWorld;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public void clear()
		{
			InWorld = null;
			ID = null;
			Type = PlayerType.Human;
			Name = string.Empty;
			Nodes = new List<Node>();
			Segments = new List<Segment>();
			Fog = null;
			AIThreadBase = null;
		}

		#endregion Methods
	}
}
