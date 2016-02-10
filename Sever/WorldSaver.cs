using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class WorldSaver
	{
		#region Methods

		/// <summary>Saves the provided world to the specified path.</summary>
		/// <param name="world">The world to save.</param>
		/// <param name="path">The path to save it to.</param>
		/// <returns>Any error messages.</returns>
		public static string saveWorld(World world, string path)
		{
			string errorMessage = string.Empty;

			TextWriter writer = null;

			try
			{
				writer = new StreamWriter(path);
				writer.WriteLine("[World]");
				writer.WriteLine("Width=" + world.Width);
				writer.WriteLine("Height=" + world.Height);
				writer.WriteLine("PersonSpacing=" + world.PersonSpacing);
				writer.WriteLine("PersonSpeedLower=" + world.PersonSpeedLower);
				writer.WriteLine("PersonSpeedUpper=" + world.PersonSpeedUpper);
				writer.WriteLine("RetractSpeed=" + world.RetractSpeed);
				writer.WriteLine("BuildRate=" + world.BuildRate);
				if (!string.IsNullOrWhiteSpace(world.ScriptBeginUpdate))
					writer.WriteLine("ScriptBeginUpdate=" + world.ScriptBeginUpdate.Replace("\n", "[newline]"));
				writer.WriteLine();
				writer.WriteLine("[Camera]");
				writer.WriteLine("X=" + world.Cam.CenterX);
				writer.WriteLine("Y=" + world.Cam.CenterY);
				writer.WriteLine();
				writer.WriteLine("[Grid]");
				writer.WriteLine("Rows=" + world.Grid.NumRows);
				writer.WriteLine("Cols=" + world.Grid.NumCols);
				writer.WriteLine();
				writer.WriteLine("[FogOfWar]");
				writer.WriteLine("Rows=" + world.FogRows);
				writer.WriteLine("Cols=" + world.FogCols);
				writer.WriteLine();
				writer.WriteLine("[PathFinder]");
				writer.WriteLine("Rows=" + world.PathRows);
				writer.WriteLine("Cols=" + world.PathCols);
				writer.WriteLine("//");
				writer.WriteLine("// Hotspots");
				writer.WriteLine("//");
				foreach (Hotspot hs in world.Hotspots)
				{
					writer.WriteLine();
					writer.WriteLine("[Hotspot]");
					writer.WriteLine("ID=" + hs.ID);
					writer.WriteLine("X=" + hs.X);
					writer.WriteLine("Y=" + hs.Y);
					if (!string.IsNullOrWhiteSpace(hs.Script))
					writer.WriteLine("Script=" + hs.Script.Replace("\n", "[newline]"));
				}
				writer.WriteLine();
				writer.WriteLine("//");
				writer.WriteLine("// Node Types");
				writer.WriteLine("//");
				foreach (NodeType nt in world.NodeTypes)
				{
					writer.WriteLine();
					writer.WriteLine("[NodeType]");
					writer.WriteLine("ID=" + nt.ID);
					writer.WriteLine("Name=" + nt.Name);
					writer.WriteLine("IsParent=" + (nt.IsParent ? "true" : "false"));
					writer.WriteLine("NumSegments=" + nt.NumSegments);
					writer.WriteLine("Radius=" + nt.Radius);
					writer.WriteLine("Spacing=" + nt.Spacing);
					writer.WriteLine("GenSpacing=" + nt.GenSpacing);
					writer.WriteLine("SightDistance=" + nt.SightDistance);
					writer.WriteLine("BuildRangeMin=" + nt.BuildRangeMin);
				}
				writer.WriteLine();
				writer.WriteLine("//");
				writer.WriteLine("// Players");
				writer.WriteLine("//");
				foreach (Player p in world.Players)
				{
					writer.WriteLine();
					writer.WriteLine("[Player]");
					writer.WriteLine("Type=" + p.Type);
					writer.WriteLine("ID=" + p.ID);
					writer.WriteLine("Name=" + p.Name);
				}
				writer.WriteLine();
				writer.WriteLine("//");
				writer.WriteLine("// Nodes");
				writer.WriteLine("//");
				foreach (Node n in world.Nodes)
				{
					writer.WriteLine();
					writer.WriteLine("[Node]");
					writer.WriteLine("ID=" + n.ID);

					if (n.NType != null)
						writer.WriteLine("Type=" + n.NType.ID);

					for (int i = 0; i < n.Segments.Length; i++)
					{
						if (n.Segments[i] != null)
							writer.WriteLine("Seg[" + i + "]=" + n.Segments[i].ID);
					}
					
					writer.WriteLine("GenCountDown=" + n.GenCountDown);
					writer.WriteLine("X=" + n.X);
					writer.WriteLine("Y=" + n.Y);
					if (n.Owner != null)
						writer.WriteLine("Owner=" + n.Owner.ID);
					writer.WriteLine("Active=" + (n.Active ? "true" : "false"));

					if (n.NType == null || n.IsParent != n.NType.IsParent)
						writer.WriteLine("IsParent=" + (n.IsParent ? "true" : "false"));
					if (n.NType == null || n.Segments.Length != n.NType.NumSegments)
						writer.WriteLine("NumSegments=" + n.Segments.Length);
					if (n.NType == null || n.Radius != n.NType.Radius)
						writer.WriteLine("Radius=" + n.Radius);
					if (n.NType == null || n.Spacing != n.NType.Spacing)
						writer.WriteLine("Spacing=" + n.Spacing);
					if (n.NType == null || n.GenSpacing != n.NType.GenSpacing)
						writer.WriteLine("GenSpacing=" + n.GenSpacing);
					if (n.NType == null || n.SightDistance != n.NType.SightDistance)
						writer.WriteLine("SightDistance=" + n.SightDistance);

					if (n.OwnsHotspot != null)
						writer.WriteLine("OwnsHotspot=" + n.OwnsHotspot.ID);
				}
				writer.WriteLine();
				writer.WriteLine("//");
				writer.WriteLine("// Segments");
				writer.WriteLine("//");
				foreach (Segment s in world.Segments)
				{
					writer.WriteLine();
					writer.WriteLine("[Segment]");
					writer.WriteLine("ID=" + s.ID);
					for (int i = 0; i < 2; i++)
					{
						if (world.Nodes.Contains(s.Nodes[i]))
							writer.WriteLine("Node[" + i + "]=" + s.Nodes[i].ID);
						else
							writer.WriteLine("End[" + i + "]=" + s.Nodes[i].X + "," + s.Nodes[i].Y);
					}
					writer.WriteLine("EndLength[0]=" + s.EndLength[0]);
					writer.WriteLine("EndLength[1]=" + s.EndLength[1]);
					if (s.Owner != null)
						writer.WriteLine("Owner=" + s.Owner.ID);
					writer.WriteLine("State[0]=" + s.State[0]);
					writer.WriteLine("State[1]=" + s.State[1]);
					for (int i = 0; i < 2; i++)
					{
						writer.WriteLine("Person[" + i + "]=" + string.Join(",", s.People[i]));
					}
				}
				writer.WriteLine();
				writer.WriteLine("//");
				writer.WriteLine("// Geos");
				writer.WriteLine("//");
				foreach (Geo geo in world.Geos)
				{
					writer.WriteLine();
					writer.WriteLine("[Geo]");
					writer.WriteLine("ID=" + geo.ID);
					writer.WriteLine("Display=" + (geo.Display ? "true" : "false"));
					writer.WriteLine("CloseLoop=" + (geo.CloseLoop ? "true" : "false"));
					for (int i = 0; i < geo.Vertices.Length; i += 2)
					{
						writer.WriteLine("Vertex=" + geo.Vertices[i].X + "," + geo.Vertices[i].Y);
					}
					if (geo.Vertices.Length > 1)
						writer.WriteLine("Vertex=" + geo.Vertices[geo.Vertices.Length - 1].X + "," + geo.Vertices[geo.Vertices.Length - 1].Y);
				}
			}
			catch (Exception ex)
			{
				errorMessage += ex.Message;
			}
			finally { if (writer != null) writer.Close(); }

			return errorMessage;
		}

		#endregion Methods
	}
}
