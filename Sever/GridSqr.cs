using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class GridSqr
	{
		#region Members

		/// <summary>The nodes that overlap this grid square.</summary>
		public List<NodeSkel> Nodes { get; private set; }

		/// <summary>The segments that overlap this grid square.</summary>
		public List<SegmentSkel> Segments { get; private set; }

		/// <summary>The geometric obstacles that overlap this grid square.</summary>
		public List<GeoSkel> Geos { get; private set; }

		/// <summary>The hotspots that overlap this grid square.</summary>
		public List<Hotspot> Hotspots { get; private set; }

		/// <summary>Determines whether an action should be run on this square or not in certain functions.  False by default.</summary>
		public bool Active { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of GridSqr.</summary>
		public GridSqr()
		{
			clear();
		}

		/// <summary>Creates a new instance of GridSqr.</summary>
		/// <param name="nodes">The nodes that overlap this grid square.</param>
		/// <param name="segments">The segments that overlap this grid square.</param>
		/// <param name="geos">The geometric obstacles that overlap this grid square.</param>
		/// <param name="hotspots">The hotspots that overlap this grid square.</param>
		/// <param name="active">Determines whether an action should be run on this square or not in certain functions.  False by default.</param>
		public GridSqr(List<NodeSkel> nodes, List<SegmentSkel> segments, List<GeoSkel> geos, List<Hotspot> hotspots, bool active)
		{
			Nodes = nodes;
			Segments = segments;
			Geos = geos;
			Active = active;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all members to their default values.</summary>
		public void clear()
		{
			Nodes = new List<NodeSkel>();
			Segments = new List<SegmentSkel>();
			Geos = new List<GeoSkel>();
			Hotspots = new List<Hotspot>();
			Active = false;
		}

		#endregion Methods
	}
}
