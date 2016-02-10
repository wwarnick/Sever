using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class WorldLoader
	{
		#region Temporary Variables

		private static GraphicsDeviceManager Graphics;

		#endregion

		public static World loadWorld(string path, GraphicsDeviceManager graphics)
		{
			Graphics = graphics;
			World map = new World();
			List<List<string[]>> commands = new List<List<string[]>>();
			
			// first read file into a list of commands
			TextReader reader = null;
			int debugI = -1;
			try
			{
				reader = new StreamReader(path);
				string line = null;
				while ((line = reader.ReadLine()) != null)
				{
					debugI++;
					if (debugI == 29)
					{ }
					// remove comments
					int commentIndex = line.IndexOf(@"//");
					if (commentIndex != -1)
						line = line.Substring(0, commentIndex);

					// remove whitespace outside of quotes
					{
						bool inQuotes = false;
						StringBuilder buf = new StringBuilder(line);
						for (int i = 0; i < buf.Length; i++)
						{
							if (buf[i] == '"')
								inQuotes = !inQuotes;
							else if (!inQuotes && (buf[i] == ' ' || buf[i] == '\t'))
							{
								buf.Remove(i, 1);
								i--;
							}
						}
					}

					if (string.IsNullOrEmpty(line))
						continue;

					List<string[]> section = null;

					switch (line)
					{
						case "[World]":
						case "[Player]":
						case "[Camera]":
						case "[Grid]":
						case "[FogOfWar]":
						case "[Scripts]":
						case "[Node]":
						case "[Segment]":
						case "[Geo]":
						case "[NodeType]":
						case "[PathFinder]":
						case "[Hotspot]":
							commands.Add(new List<string[]>());
							section = commands[commands.Count - 1];
							section.Add(new string[3]);
							section[0][0] = line;
							break;
						default:
							int split = line.IndexOf('=');
							string[] breakUp = new string[] { line.Substring(0, split), line.Substring(split + 1) };
							section = commands[commands.Count - 1];
							section.Add(new string[3]);
							section[section.Count - 1][2] = breakUp[1];
							if(breakUp[0].EndsWith("]"))
							{
								int openBrace = breakUp[0].IndexOf('[');

								if (openBrace != -1)
								{
									section[section.Count - 1][0] = breakUp[0].Substring(0, openBrace);
									section[section.Count - 1][1] = breakUp[0].Substring(openBrace + 1, breakUp[0].Length - openBrace - 2);
								}
								else
								{
									section[section.Count - 1][0] = breakUp[0];
								}
							}
							else
							{
								section[section.Count - 1][0] = breakUp[0];
							}
							break;
					}
				}
			}
			catch (Exception ex)
			{ }
			finally { if (reader != null) reader.Close(); }

			Dictionary<string, Player> players = new Dictionary<string, Player>();
			Dictionary<string, Node> nodes = new Dictionary<string,Node>();
			Dictionary<string, Segment> segments = new Dictionary<string,Segment>();
			Dictionary<string, Geo> geos = new Dictionary<string, Geo>();
			Dictionary<string, Hotspot> hotspots = new Dictionary<string, Hotspot>();
			Dictionary<string, NodeType> nodeTypes = new Dictionary<string, NodeType>();

			List<Node> nodeList = new List<Node>();
			List<Segment> segList = new List<Segment>();

			List<Node> nodeNoID = new List<Node>();
			List<Segment> segNoID = new List<Segment>();
			List<Geo> geoNoID = new List<Geo>();
			List<Hotspot> hotspotNoID = new List<Hotspot>();
			List<NodeType> nodeTypeNoID = new List<NodeType>();

			// process commands

			// world
			for (int i = 0; i < commands.Count; i++)
			{
				if (commands[i][0][0] == "[World]")
				{
					readWorldVars(commands[i], map);
					commands.RemoveAt(i);
					break;
				}
			}

			// camera
			map.Cam.Width = (FInt)Graphics.PreferredBackBufferWidth;
			map.Cam.Height = (FInt)Graphics.PreferredBackBufferHeight;
			for (int i = 0; i < commands.Count; i++)
			{
				if (commands[i][0][0] == "[Camera]")
				{
					readCameraVars(commands[i], map);
					commands.RemoveAt(i);
					break;
				}
			}

			// grid, fog of war, node types, and pathfinder
			bool gridFound = false;
			bool fogFound = false;
			for (int i = 0; i < commands.Count; i++)
			{
				if (!gridFound && commands[i][0][0] == "[Grid]")
				{
					gridFound = true;
					readGridVars(commands[i], map);
					commands.RemoveAt(i);
					i--;
				}
				else if (!fogFound && commands[i][0][0] == "[FogOfWar]")
				{
					fogFound = true;
					readFogOfWarVars(commands[i], map);
					commands.RemoveAt(i);
					i--;
				}
				else if (commands[i][0][0] == "[NodeType]")
				{
					readNodeTypeVars(commands[i], map, nodeTypes, nodeTypeNoID);
					commands.RemoveAt(i);
					i--;
				}
				else if (commands[i][0][0] == "[PathFinder]")
				{
					readPathFinderVars(commands[i], map);
					commands.RemoveAt(i);
					i--;
				}
			}

			// players, geos, and hotspots
			for (int i = 0; i < commands.Count; i++)
			{
				if (commands[i][0][0] == "[Player]")
				{
					readPlayerVars(commands[i], map, players);
					commands.RemoveAt(i);
					i--;
				}
				else if (commands[i][0][0] == "[Geo]")
				{
					readGeoVars(commands[i], map, geos, geoNoID);
					commands.RemoveAt(i);
					i--;
				}
				else if (commands[i][0][0] == "[Hotspot]")
				{
					readHotspotVars(commands[i], map, hotspots, hotspotNoID);
					commands.RemoveAt(i);
					i--;
				}
			}

			// nodes and segments: 1st round
			for (int i = 0; i < commands.Count; i++)
			{
				if (commands[i][0][0] == "[Node]")
				{
					readNodeVars1(commands[i], map, nodeList, nodes, players, nodeTypes, hotspots);
					readNodeVars2(commands[i], map, nodeList[nodeList.Count - 1], nodes, players);
				}
				else if (commands[i][0][0] == "[Segment]")
				{
					readSegmentVars1(commands[i], map, segList, segments, players);
				}
			}

			// nodes and segments: 2nd round
			int curNode = 0;
			int curSegment = 0;
			for (int i = 0; i < commands.Count; i++)
			{
				if (commands[i][0][0] == "[Node]")
				{
					readNodeVars3(commands[i], map, nodeList[curNode], segments, nodeNoID);
					curNode++;
					commands.RemoveAt(i);
					i--;
				}
				else if (commands[i][0][0] == "[Segment]")
				{
					readSegmentVars2(commands[i], map, segList[curSegment], nodes);
					curSegment++;
				}
			}

			// segments: 3rd round
			curSegment = 0;
			for (int i = 0; i < commands.Count; i++)
			{
				if (commands[i][0][0] == "[Segment]")
				{
					readSegmentVars3(commands[i], map, segList[curSegment], segNoID);
					curSegment++;
				}
				else
				{
					throw new Exception("Invalid section: " + commands[i][0][0]);
				}
			}

			map.refreshNodeVars();
			map.sortNodeTypes();
			map.refreshNextGenIDs();

			// add IDless objects to map
			foreach (Node node in nodeNoID)
			{
				map.addNode(node);
			}
			foreach (Segment seg in segNoID)
			{
				map.addSegment(seg);
			}
			foreach (Geo geo in geoNoID)
			{
				map.addGeo(geo);
			}
			foreach (Hotspot hotspot in hotspotNoID)
			{
				map.addHotspot(hotspot);
			}
			foreach (NodeType nt in nodeTypeNoID)
			{
				map.addNodeType(nt);
			}

			// prepare player vars
			for (int i = 0; i < map.Players.Count; i++)
			{
				map.Players[i].Fog.applyVisibility(map.Players[i].Nodes);

				if (map.Players[i].Type == Player.PlayerType.Computer)
				{
					map.Players[i].AIThreadBase = new AIThread(map.Players[i]);
					map.Players[i].AIThreadBase.Path = new PathFinder(map.PathRows, map.PathCols, map);
					Node[] plyrNodes = null;
					Segment[] plyrSegs = null;
					NodeSkel[] allNodes = null;
					SegmentSkel[] allSegs = null;
					map.Players[i].AIThreadBase.Grid = map.Grid.getSkeleton(map.Players[i], out plyrNodes, out plyrSegs, out allNodes, out allSegs);
					map.Players[i].AIThreadBase.Nodes.AddRange(plyrNodes);
					map.Players[i].AIThreadBase.Segments.AddRange(plyrSegs);
					map.Players[i].AIThreadBase.buildDictionaries(allNodes, allSegs);

					map.NodeEvents[i].Clear();
					map.SegEvents[i].Clear();

					map.Players[i].AIThreadBase.refreshVisibility();
					map.Players[i].AIThreadBase.refreshPathIntersections();
				}
			}

			map.refreshVisibility();

			// let go of the graphics device, since it's no longer needed
			Graphics = null;

			return map;
		}

		#region Read Vars

		private static void readSegmentVars1(List<string[]> commands, World map, List<Segment> segList, Dictionary<string, Segment> segments, Dictionary<string, Player> players)
		{
			double tempDbl = 0d;
			int tempInt = 0;
			Segment segment = new Segment(map);

			for (int j = 1; j < commands.Count; j++)
			{
				string value = translateValue(commands[j][2], "[Segment]", map, segment);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "ID":
						segment.ID = value;
						segments.Add(value, segment);
						break;
					case "Node":
					case "EndLength":
						// do nothing until next step
						break;
					case "Owner":
						segment.Owner = players[value];
						break;
					case "State":
						valueValid = int.TryParse(commands[j][1], out tempInt);
						switch (value)
						{
							case "Complete":
								segment.State[tempInt] = Segment.SegState.Complete;
								break;
							case "Retracting":
								segment.State[tempInt] = Segment.SegState.Retracting;
								break;
							case "Building":
								segment.State[tempInt] = Segment.SegState.Building;
								break;
							default:
								throw new Exception("Unrecognized segment lane state: " + value);
						}
						break;
					case "Person":
						valueValid = int.TryParse(commands[j][1], out tempInt);
						string[] people = value.Split(',');
						foreach (string p in people)
						{
							if (double.TryParse(p, out tempDbl))
								segment.People[tempInt].AddLast((FInt)tempDbl);
						}
						break;
					default:
						throw new Exception("Segment variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid for variable '" + commands[j][0] + "'.");
			}

			segList.Add(segment);
		}

		private static void readSegmentVars2(List<string[]> commands, World map, Segment segment, Dictionary<string, Node> nodes)
		{
			int tempInt = 0;
			double tempDbl = 0d;

			for (int j = 1; j < commands.Count; j++)
			{
				string value = translateValue(commands[j][2], "[Segment]", map, segment);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "ID":
						// do nothing
						break;
					case "Node":
						valueValid = int.TryParse(commands[j][1], out tempInt);
						segment.Nodes[tempInt] = nodes[value];
						break;
					case "End":
						valueValid = int.TryParse(commands[j][1], out tempInt);
						Node newNode = new Node(map);
						string[] coords = value.Split(',');
						if (valueValid && coords.Length == 2 && double.TryParse(coords[0], out tempDbl))
						{
							newNode.X = (FInt)tempDbl;

							if (double.TryParse(coords[1], out tempDbl))
								newNode.Y = (FInt)tempDbl;
							else
								valueValid = false;
						}
						else
							valueValid = false;
						if (valueValid)
							segment.Nodes[tempInt] = newNode;
						break;
					case "EndLength":
					case "Owner":
					case "State":
					case "Person":
						// do nothing
						break;
					default:
						throw new Exception("Segment variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid for variable '" + commands[j][0] + "'.");
			}

			segment.refreshMath();
		}

		private static void readSegmentVars3(List<string[]> commands, World map, Segment segment, List<Segment> segNoID)
		{
			double tempDbl = 0d;
			int tempInt = 0;

			for (int j = 1; j < commands.Count; j++)
			{
				string value = translateValue(commands[j][2], "[Segment]", map, segment);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "ID":
					case "Node":
						// do nothing
						break;
					case "EndLength":
						valueValid = int.TryParse(commands[j][1], out tempInt);
						valueValid &= double.TryParse(value, out tempDbl);
						segment.EndLength[tempInt] = (FInt)tempDbl;
						segment.refreshEndLoc(1 - tempInt);
						break;
					case "Owner":
					case "State":
					case "Person":
						// do nothing
						break;
					default:
						throw new Exception("Segment variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid for variable '" + commands[j][0] + "'.");
			}

			if (segment.ID != null)
				map.addSegment(segment);
			else
				segNoID.Add(segment);
		}

		private static void readNodeVars1(List<string[]> commands, World map, List<Node> nodeList, Dictionary<string, Node> nodes, Dictionary<string, Player> players, Dictionary<string, NodeType> nodeTypes, Dictionary<string, Hotspot> hotspots)
		{
			double tempDouble = 0d;
			Node node = new Node(map);

			for (int j = 1; j < commands.Count; j++)
			{
				string value = translateValue(commands[j][2], "[Node]", map, node);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "ID":
						node.ID = value;
						nodes.Add(value, node);
						break;
					case "SegCap":
					case "Seg":
					case "IsParent":
					case "GenSpacing":
					case "GenCountDown":
						// do nothing until next step
						break;
					case "X":
						valueValid = double.TryParse(value, out tempDouble);
						node.X = (FInt)tempDouble;
						break;
					case "Y":
						valueValid = double.TryParse(value, out tempDouble);
						node.Y = (FInt)tempDouble;
						break;
					case "Owner":
						node.Owner = players[value];
						break;
					case "Radius":
					case "SightDistance":
						// do nothing until next step
						break;
					case "Active":
						valueValid = value == "true" || value == "false";
						node.Active = value == "true";
						break;
					case "Spacing":
						// do nothing until next step
						break;
					case "Type":
						node.setNodeType(nodeTypes[value]);
						break;
					case "OwnsHotspot":
						node.OwnsHotspot = hotspots[value];
						break;
					default:
						throw new Exception("Node variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid for variable '" + commands[j][0] + "'.");
			}

			nodeList.Add(node);
		}

		private static void readNodeVars2(List<string[]> commands, World map, Node node, Dictionary<string, Node> nodes, Dictionary<string, Player> players)
		{
			double tempDbl = 0d;
			int tempInt = 0;

			for (int j = 1; j < commands.Count; j++)
			{
				string value = translateValue(commands[j][2], "[Node]", map, node);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "ID":
						// do nothing
						break;
					case "SegCap":
						valueValid = int.TryParse(value, out tempInt);
						node.initSegArrays(tempInt);
						break;
					case "Seg":
						// do nothing until next step
						break;
					case "IsParent":
						valueValid = value == "true" || value == "false";
						node.IsParent = value == "true";
						break;
					case "GenSpacing":
						valueValid = double.TryParse(value, out tempDbl);
						node.GenSpacing = (FInt)tempDbl;
						break;
					case "GenCountDown":
						valueValid = double.TryParse(value, out tempDbl);
						node.GenCountDown = (FInt)tempDbl;
						break;
					case "X":
					case "Y":
					case "Owner":
						// do nothing
						break;
					case "Radius":
						valueValid = double.TryParse(value, out tempDbl);
						node.Radius = (FInt)tempDbl;
						break;
					case "SightDistance":
						valueValid = double.TryParse(value, out tempDbl);
						node.SightDistance = (FInt)tempDbl;
						break;
					case "Active":
						// do nothing
						break;
					case "Spacing":
						valueValid = double.TryParse(value, out tempDbl);
						node.Spacing = (FInt)tempDbl;
						break;
					case "Type":
					case "OwnsHotspot":
						// do nothing
						break;
					default:
						throw new Exception("Node variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid for variable '" + commands[j][0] + "'.");
			}
		}

		private static void readNodeVars3(List<string[]> commands, World map, Node node, Dictionary<string, Segment> segments, List<Node> nodeNoID)
		{
			int tempInt = 0;

			for (int j = 1; j < commands.Count; j++)
			{
				string value = translateValue(commands[j][2], "[Node]", map, node);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "ID":
					case "SegCap":
						// do nothing
						break;
					case "Seg":
						valueValid = int.TryParse(commands[j][1], out tempInt);
						node.Segments[tempInt] = segments[value];
						break;
					case "IsParent":
					case "GenSpacing":
					case "GenCountDown":
					case "X":
					case "Y":
					case "Owner":
					case "Radius":
					case "SightDistance":
					case "Active":
					case "Spacing":
					case "Type":
					case "OwnsHotspot":
						// do nothing
						break;
					default:
						throw new Exception("Node variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid for variable '" + commands[j][0] + "'.");
			}

			node.updateNumSegments();

			if (node.ID != null)
				map.addNode(node);
			else
				nodeNoID.Add(node);
		}

		private static void readPlayerVars(List<string[]> commands, World map, Dictionary<string, Player> players)
		{
			Player player = new Player(map);
			player.Fog = new FogOfWar(map.FogRows, map.FogCols, map.Size, player);

			for (int j = 1; j < commands.Count; j++)
			{
				string value = translateValue(commands[j][2], "[Player]", map);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "Type":
						switch (value)
						{
							case "Human":
								player.Type = Player.PlayerType.Human;
								map.HumanPlayer = player;
								break;
							case "Network":
								player.Type = Player.PlayerType.Network;
								break;
							case "Computer":
								player.Type = Player.PlayerType.Computer;
								break;
							default:
								throw new Exception("Unrecognized player type: " + value);
						}
						break;
					case "Name":
						player.Name = value;
						break;
					case "ID":
						player.ID = value;
						players.Add(value, player);
						break;
					default:
						throw new Exception("Player variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid.");
			}

			map.addPlayer(player);
			if (player.Type == Player.PlayerType.Computer)
			{
				map.SegEvents.Add(new Dictionary<string, WorldEvent.EventType>());
				map.NodeEvents.Add(new Dictionary<string, WorldEvent.EventType>());
			}
			else
			{
				map.SegEvents.Add(null);
				map.NodeEvents.Add(null);
			}
		}

		private static void readGridVars(List<string[]> commands, World map)
		{
			int tempInt = 0;
			int numRows = 0;
			int numCols = 0;

			for (int j = 1; j < commands.Count; j++)
			{
				string value = translateValue(commands[j][2], "[Grid]", map);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "Rows":
						valueValid = int.TryParse(value, out tempInt);
						numRows = tempInt;
						break;
					case "Cols":
						valueValid = int.TryParse(value, out tempInt);
						numCols = tempInt;
						break;
					default:
						throw new Exception("Grid variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid.");
			}

			map.Grid = new GridManager(numCols, numRows, map);
		}

		private static void readFogOfWarVars(List<string[]> commands, World map)
		{
			int tempInt = 0;

			for (int j = 1; j < commands.Count; j++)
			{
				string value = translateValue(commands[j][2], "[FogOfWar]", map);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "Rows":
						valueValid = int.TryParse(value, out tempInt);
						map.FogRows = tempInt;
						break;
					case "Cols":
						valueValid = int.TryParse(value, out tempInt);
						map.FogCols = tempInt;
						break;
					default:
						throw new Exception("FogOfWar variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid.");
			}
		}

		private static void readPathFinderVars(List<string[]> commands, World map)
		{
			int tempInt = 0;

			for (int j = 1; j < commands.Count; j++)
			{
				string value = translateValue(commands[j][2], "[PathFinder]", map);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "Rows":
						valueValid = int.TryParse(value, out tempInt);
						map.PathRows = tempInt;
						break;
					case "Cols":
						valueValid = int.TryParse(value, out tempInt);
						map.PathCols = tempInt;
						break;
					default:
						throw new Exception("PathFinder variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid.");
			}
		}

		private static void readGeoVars(List<string[]> commands, World map, Dictionary<string, Geo> geos, List<Geo> geoNoID)
		{
			double tempDbl0 = 0f;
			double tempDbl1 = 0f;
			string id = null;
			bool display = true;
			bool closeLoop = true;
			List<VectorF> vertices = new List<VectorF>();

			for (int j = 1; j < commands.Count; j++)
			{
				string value = translateValue(commands[j][2], "[Grid]", map);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "Vertex":
						string[] tempStr = value.Split(',');

						for (int i = 0; i < tempStr.Length; i += 2)
						{
							valueValid = double.TryParse(tempStr[i], out tempDbl0);
							valueValid = double.TryParse(tempStr[i + 1], out tempDbl1);

							// add previous vertex again to start new line
							if (vertices.Count > 1)
								vertices.Add(vertices[vertices.Count - 1]);

							vertices.Add(new VectorF((FInt)tempDbl0, (FInt)tempDbl1));
						}
						break;
					case "ID":
						id = value;
						break;
					case "Display":
						valueValid = value == "true" || value == "false";
						display = value == "true";
						break;
					case "CloseLoop":
						valueValid = value == "true" || value == "false";
						closeLoop = value == "true";
						break;
					default:
						throw new Exception("Geo variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid.");
			}

			Geo geo = new Geo(map);
			if (id != null)
			{
				geo.ID = id;
				geos.Add(id, geo);
			}
			geo.Display = display;
			geo.CloseLoop = closeLoop;
			VectorF[] vArray = new VectorF[vertices.Count];
			vertices.CopyTo(vArray);
			geo.Vertices = vArray;
			geo.refreshMath(new Vector2(50f)); // TODO: find out texture size

			if (id != null)
				map.addGeo(geo);
			else
				geoNoID.Add(geo);
		}

		private static void readCameraVars(List<string[]> commands, World map)
		{
			double tempDbl = 0d;

			for (int j = 1; j < commands.Count; j++)
			{
				string value = translateValue(commands[j][2], "[Camera]", map);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "Width":
						valueValid = double.TryParse(value, out tempDbl);
						map.Cam.Width = (FInt)tempDbl;
						break;
					case "Height":
						valueValid = double.TryParse(value, out tempDbl);
						map.Cam.Height = (FInt)tempDbl;
						break;
					case "X":
						valueValid = double.TryParse(value, out tempDbl);
						map.Cam.CenterX = (FInt)tempDbl;
						break;
					case "Y":
						valueValid = double.TryParse(value, out tempDbl);
						map.Cam.CenterY = (FInt)tempDbl;
						break;
					default:
						throw new Exception("Camera variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid.");
			}

			map.Cam.refreshCorners();
		}

		private static void readWorldVars(List<string[]> commands, World map)
		{
			double tempDbl = 0d;

			for (int j = 1; j < commands.Count; j++)
			{
                string value = translateValue(commands[j][2], "[World]", map);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "Width":
						valueValid = double.TryParse(value, out tempDbl);
						map.Width = (FInt)tempDbl;
						break;
					case "Height":
						valueValid = double.TryParse(value, out tempDbl);
						map.Height = (FInt)tempDbl;
						break;
					case "PersonSpacing":
						valueValid = double.TryParse(value, out tempDbl);
						map.PersonSpacing = (FInt)tempDbl;
						break;
					case "PersonSpeedLower":
						valueValid = double.TryParse(value, out tempDbl);
						map.PersonSpeedLower = (FInt)tempDbl;
						break;
					case "PersonSpeedUpper":
						valueValid = double.TryParse(value, out tempDbl);
						map.PersonSpeedUpper = (FInt)tempDbl;
						break;
					case "RetractSpeed":
						valueValid = double.TryParse(value, out tempDbl);
						map.RetractSpeed = (FInt)tempDbl;
						break;
					case "BuildRate":
						valueValid = double.TryParse(value, out tempDbl);
						map.BuildRate = (FInt)tempDbl;
						break;
					case "ScriptBeginUpdate":
						map.ScriptBeginUpdate = value.Replace("[newline]", "\n");
						break;
                    default:
						throw new Exception("World variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid.");
			}
		}

		private static void readHotspotVars(List<string[]> commands, World map, Dictionary<string, Hotspot> hotspots, List<Hotspot> hotspotNoID)
		{
			double tempDouble = 0d;
			Hotspot hotspot = new Hotspot(map);

			for (int j = 1; j < commands.Count; j++)
			{
				string value = translateValue(commands[j][2], "[Hotspot]", map);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "ID":
						hotspot.ID = value;
						hotspots.Add(value, hotspot);
						break;
					case "X":
						valueValid = double.TryParse(value, out tempDouble);
						hotspot.X = (FInt)tempDouble;
						break;
					case "Y":
						valueValid = double.TryParse(value, out tempDouble);
						hotspot.Y = (FInt)tempDouble;
						break;
					case "Script":
						hotspot.Script = value.Replace("[newline]", "\n");
						break;
					default:
						throw new Exception("Hotspot variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid for variable '" + commands[j][0] + "'.");
			}

			if (hotspot.ID != null)
				map.addHotspot(hotspot);
		}

		private static void readNodeTypeVars(List<string[]> commands, World map, Dictionary<string, NodeType> nodeTypes, List<NodeType> nodeTypeNoID)
		{
			int tempInt = 0;
			double tempDbl = 0d;

			NodeType nodeType = new NodeType();

			for (int j = 1; j < commands.Count; j++)
			{
				string value = translateValue(commands[j][2], "[NodeType]", map);
				bool valueValid = true;
				switch (commands[j][0])
				{
					case "ID":
						nodeType.ID = value;
						nodeTypes.Add(value, nodeType);
						break;
					case "IsParent":
						valueValid = value == "true" || value == "false";
						nodeType.IsParent = value == "true";
						break;
					case "NumSegments":
						valueValid = int.TryParse(value, out tempInt);
						nodeType.NumSegments = tempInt;
						break;
					case "Radius":
						valueValid = double.TryParse(value, out tempDbl);
						nodeType.Radius = (FInt)tempDbl;
						break;
					case "Spacing":
						valueValid = double.TryParse(value, out tempDbl);
						nodeType.Spacing = (FInt)tempDbl;
						break;
					case "GenSpacing":
						valueValid = double.TryParse(value, out tempDbl);
						nodeType.GenSpacing = (FInt)tempDbl;
						break;
					case "SightDistance":
						valueValid = double.TryParse(value, out tempDbl);
						nodeType.SightDistance = (FInt)tempDbl;
						break;
					case "BuildRangeMin":
						valueValid = double.TryParse(value, out tempDbl);
						nodeType.BuildRangeMin = (FInt)tempDbl;
						break;
					case "Name":
						nodeType.Name = value;
						break;
					default:
						throw new Exception("NodeType variable not recognized: " + commands[j][0]);
				}

				if (!valueValid)
					throw new Exception("Value '" + commands[j][2] + "' not valid.");
			}

			if (nodeType.ID != null)
				map.addNodeType(nodeType);
			else
				nodeTypeNoID.Add(nodeType);
		}

		#endregion ReadVars

		#region Translate Values

		private static string translateValue(string val, string context, World map)
		{
			return translateValue(val, context, map, null, null);
		}

		private static string translateValue(string val, string context, World map, Node node)
		{
			return translateValue(val, context, map, node, null);	
		}

		private static string translateValue(string val, string context, World map, Segment segment)
		{
			return translateValue(val, context, map, null, segment);
		}

		private static string translateValue(string val, string context, World map, Node node, Segment segment)
		{
			string result = null;

			switch (context)
			{
				case "[World]":
					switch (val)
					{
						case "WindowWidth":
							result = Graphics.PreferredBackBufferWidth.ToString();
							break;
						case "WindowHeight":
							result = Graphics.PreferredBackBufferHeight.ToString();
							break;
						default:
							result = val;
							break;
					}
					break;
				case "[Camera]":
					switch (val)
					{
						case "CenterX":
							result = ((double)(map.Width / FInt.F2)).ToString();
							break;
						case "CenterY":
							result = ((double)(map.Height / FInt.F2)).ToString();
							break;
						case "WindowWidth":
							result = Graphics.PreferredBackBufferWidth.ToString();
							break;
						case "WindowHeight":
							result = Graphics.PreferredBackBufferHeight.ToString();
							break;
						default:
							result = val;
							break;
					}
					break;
				case "[Grid]":
				case "[FogOfWar]":
				case "[Geo]":
				case "[Player]":
				case "[NodeType]":
				case "[PathFinder]":
				case "[Hotspot]":
					result = val;
					break;
				case "[Node]":
					switch (val)
					{
						case "GenSpacing":
							result = node.GenSpacing.DblValue.ToString();
							break;
						case "GenCountDown":
							result = node.GenCountDown.DblValue.ToString();
							break;
						default:
							result = val;
							break;
					}
					break;
				case "[Segment]":
					switch (val)
					{
						case "Length":
							result = segment.Length.DblValue.ToString();
							break;
						default:
							result = val;
							break;
					}
					break;
				default:
					throw new Exception("Unrecognized context: " + context);
			}

			return result;
		}

		#endregion Translate Values
	}
}
