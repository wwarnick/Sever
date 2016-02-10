using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
    public class World
	{
		#region Members

		/// <summary>The types of objects in the world.</summary>
		public enum WorldObject { Node, Segment, HotSpot, Geo }

		/// <summary>This world's script manager.</summary>
		public ScriptMan Script { get; private set; }

		/// <summary>Whether or not this world is in editor mode.</summary>
		public bool EditorMode { get; private set; }

		/// <summary>The active players.</summary>
		public List<Player> Players { get; private set; }

		/// <summary>The human player.</summary>
		public Player HumanPlayer { get; set; }

		/// <summary>The actions that players wish to take in the current update.</summary>
		public List<PlayerAction> PlayerActions { get; private set; }

		/// <summary>For each ai player, a list of world segment events that have taken place since the last sync.</summary>
		public List<Dictionary<string, WorldEvent.EventType>> SegEvents { get; private set; }

		/// <summary>For each ai player, a list of world node events that have taken place since the last sync.</summary>
		public List<Dictionary<string, WorldEvent.EventType>> NodeEvents { get; private set; }

		/// <summary>A list of events to send to an AI player.</summary>
		private List<WorldEvent> EventsToSend { get; set; }

		/// <summary>All nodes in play.</summary>
		public List<Node> Nodes { get; private set; }

		/// <summary>All segments in play.</summary>
		public List<Segment> Segments { get; private set; }

		/// <summary>All geometric obstacles in play.</summary>
		public List<Geo> Geos { get; private set; }

		/// <summary>All hotspots in play.</summary>
		public List<Hotspot> Hotspots { get; private set; }

		/// <summary>All node types in play.</summary>
		public List<NodeType> NodeTypes { get; private set; }

		/// <summary>A dictionary of the world's players by id.</summary>
		public Dictionary<string, Player> PlayerByID { get; private set; }

		/// <summary>A dictionary of the world's nodes by id.</summary>
		public Dictionary<string, Node> NodeByID { get; private set; }

		/// <summary>A dictionary of the world's segments by id.</summary>
		public Dictionary<string, Segment> SegByID { get; private set; }

		/// <summary>A dictionary of the world's geos by id.</summary>
		public Dictionary<string, Geo> GeoByID { get; private set; }

		/// <summary>A dictionary of the world's hotspots by id.</summary>
		public Dictionary<string, Hotspot> HotspotByID { get; private set; }

		/// <summary>A dictionary of the world's node types by id.</summary>
		public Dictionary<string, NodeType> NodeTypeByID { get; private set; }

		/// <summary>The next id to assign to a player.</summary>
		private long NextPlayerID { get; set; }

		/// <summary>The next id to assign to a node.</summary>
		private long NextNodeID { get; set; }

		/// <summary>The next id to assign to a segment.</summary>
		private long NextSegID { get; set; }

		/// <summary>The next id to assign to a geo.</summary>
		public long NextGeoID { get; private set; }

		/// <summary>The next id to assign to a hotspot.</summary>
		public long NextHotspotID { get; private set; }

		/// <summary>The next id to assign to a node type.</summary>
		public long NextNodeTypeID { get; private set; }

		/// <summary>The active camera.</summary>
		public Camera Cam { get; private set; }

		/// <summary>The active collision manager.</summary>
		public CollisionManager Collision { get; private set; }

		/// <summary>The active grid manager.</summary>
		private GridManager grid;

		/// <summary>The active grid manager.</summary>
		public GridManager Grid
		{
			get { return grid; }
			set
			{
				grid = value;
				Collision.Grid = value;
			}
		}

		/// <summary>The current game time.</summary>
		public GameTime CurTime { get; set; }

		/// <summary>The size of the world.</summary>
		private VectorF size;

		/// <summary>The size of the world.</summary>
		public VectorF Size { get { return size; } set { size = value; } }

		/// <summary>The width of the world.</summary>
		public FInt Width { get { return size.X; } set { size.X = value; } }

		/// <summary>The height of the world.</summary>
		public FInt Height { get { return size.Y; } set { size.Y = value; } }
		
		/// <summary>The minimum spacing between each person.</summary>
		public FInt PersonSpacing { get; set; }

		/// <summary>The speed (pixels per second) at which people move in a segment at lowest capacity.</summary>
		private FInt personSpeedLower;

		/// <summary>The speed (pixels per second) at which people move in a segment at lowest capacity.</summary>
		public FInt PersonSpeedLower
		{
			get { return personSpeedLower; }
			set
			{
				personSpeedLower = value;
				PersonSpeedRange = PersonSpeedUpper - personSpeedLower;
			}
		}

		/// <summary>The speed (pixels per second) at which people move in a segment at highest capacity.</summary>
		private FInt personSpeedUpper;

		/// <summary>The speed (pixels per second) at which people move in a segment at highest capacity.</summary>
		public FInt PersonSpeedUpper
		{
			get { return personSpeedUpper; }
			set
			{
				personSpeedUpper = value;
				PersonSpeedRange = personSpeedUpper - PersonSpeedLower;
			}
		}

		/// <summary>The range in speed (pixels per second) at which people move in a segment dependent on its capacity.</summary>
		public FInt PersonSpeedRange { get; private set; }

		/// <summary>The speed (pixels per second) at which segments retract.</summary>
		public FInt RetractSpeed { get; set; }

		/// <summary>The number of pixels built on each build step.</summary>
		public FInt BuildRate { get; set; }

		/// <summary>The number of rows in the fog of war grid.</summary>
		public int FogRows { get; set; }

		/// <summary>The number of columns in the fog of war grid.</summary>
		public int FogCols { get; set; }

		/// <summary>The number of rows in the path-finding grid.</summary>
		public int PathRows { get; set; }

		/// <summary>The number of columns in the path-finding grid.</summary>
		public int PathCols { get; set; }

		/// <summary>The script that runs at the beginning of an update cycle.</summary>
		public string ScriptBeginUpdate { get; set; }

		#region For FrontEnd

		/// <summary>A list of actions for the front-end to perform.</summary>
		public Queue<FrontEndAction> FrontEndActions { get; private set; }

		#endregion For FrontEnd

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of World.</summary>
		public World()
		{
			clear();
		}

		/// <summary>Creates a new instance of World.</summary>
		/// <param name="editorMode">Whether or not this world is in editor mode.</param>
		public World(bool editorMode)
			: this()
		{
			EditorMode = editorMode;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all members to their default values.</summary>
		public void clear()
		{
			Script = new ScriptMan(this);
			EditorMode = false;
			Players = new List<Player>();
			PlayerActions = new List<PlayerAction>();
			SegEvents = new List<Dictionary<string, WorldEvent.EventType>>();
			NodeEvents = new List<Dictionary<string, WorldEvent.EventType>>();
			EventsToSend = new List<WorldEvent>();
			Nodes = new List<Node>();
			Segments = new List<Segment>();
			Geos = new List<Geo>();
			Hotspots = new List<Hotspot>();
			NodeTypes = new List<NodeType>();
			PlayerByID = new Dictionary<string, Player>();
			NodeByID = new Dictionary<string, Node>();
			SegByID = new Dictionary<string, Segment>();
			GeoByID = new Dictionary<string, Geo>();
			HotspotByID = new Dictionary<string, Hotspot>();
			NodeTypeByID = new Dictionary<string, NodeType>();
			NextPlayerID = 0L;
			NextNodeID = 0L;
			NextSegID = 0L;
			NextGeoID = 0L;
			NextHotspotID = 0L;
			NextNodeTypeID = 0L;
			Cam = new Camera();
			Collision = new CollisionManager();
			Grid = null;
			CurTime = new GameTime();
			Size = new VectorF((FInt)1000);
			PersonSpacing = (FInt)20;
			PersonSpeedLower = (FInt)50;
			PersonSpeedUpper = (FInt)200;
			RetractSpeed = (FInt)90;
			BuildRate = (FInt)10;
			FogRows = 1;
			FogCols = 1;
			PathRows = 1;
			PathCols = 1;
			ScriptBeginUpdate = string.Empty;
			FrontEndActions = new Queue<FrontEndAction>();
		}

		/// <summary>Refreshes the parentage, number of segments, and density for all nodes at once.</summary>
		public void refreshNodeVars()
		{
			List<Node> parents = new List<Node>();

			foreach (Node node in Nodes)
			{
				// update number of segments
				node.updateNumSegments();

				// collect list of parents
				node.Parents.Clear();
				if (node.IsParent)
					parents.Add(node);
			}

			// update parentage
			foreach (Node node in parents)
			{
				node.spreadParentage();
			}

			// update densities TODO: this method will only update one system
			refreshDensities();
		}

		/// <summary>Refreshes the nodes, segments, hotspots, and geos in each grid square.</summary>
		public void refreshGrid()
		{
			foreach (GridSqr square in Grid.Squares)
			{
				square.Nodes.Clear();
				square.Segments.Clear();
				square.Hotspots.Clear();
				square.Geos.Clear();
			}

			foreach (Node node in Nodes)
			{
				Grid.Point(node.Pos, node, gridAddNode);
			}

			foreach (Segment segment in Segments)
			{
				Grid.Line(segment.Nodes[0].Pos, segment.Nodes[1].Pos, segment, gridAddSegment);
			}

			foreach (Hotspot hotspot in Hotspots)
			{
				Grid.Point(hotspot.Pos, hotspot, gridAddHotspot);
			}

			foreach (Geo geo in Geos)
			{
				Grid.Rect(geo.UpperLeft, geo.LowerRight, geo, gridAddGeo);
			}
		}

		/// <summary>Finds a node with only one segment attached to it.</summary>
		/// <returns>A node with only one segment attached to it.</returns>
		public void refreshDensities()
		{
			int lastNode = 0;

			while (true)
			{
				Node node = null;
				Segment fromSeg = null;
				Segment toSeg = null;

				// start with a node with at least one segment
				for (int i = lastNode; i < Nodes.Count; i++)
				{
					if (Nodes[i].NumSegments > 0 && Nodes[i].LastCheck != TimeSpan.MaxValue)
					{
						node = Nodes[i];
						lastNode = i + 1;
						break;
					}
				}

				// if no node found, break loop
				if (node == null)
					break;

				// find one with only one segment
				bool lonely = false;
				while (!lonely)
				{
					lonely = node.NumSegments == 1;

					if (!lonely)
					{
						foreach (Segment seg in node.Segments)
						{
							if (seg != null && seg != fromSeg)
							{
								toSeg = seg;
								break;
							}
						}

						node = toSeg.getOppNode(node);
						fromSeg = toSeg;
					}
				}

				node.updateDensity(true);
			}
		}

		/// <summary>Updates the world.</summary>
		/// <param name="elapsed">The time elapsed since the last update</param>
		public void update(GameTime gt)
		{
			FInt elapsed = (FInt)gt.ElapsedGameTime.TotalSeconds;

			// gather AI actions
			foreach (Player player in Players)
			{
				if (player.Type == Player.PlayerType.Computer)
				{
					lock (player.AIThreadBase.Actions)
					{
						if (player.AIThreadBase.Actions.Count > 0)
						{
							PlayerActions.AddRange(player.AIThreadBase.Actions);
							player.AIThreadBase.Actions.Clear();
						}
					}
				}
			}

			// run begin update script
			if (!string.IsNullOrEmpty(ScriptBeginUpdate))
				Script.runScript(ScriptBeginUpdate);

			VectorF tempV2 = new VectorF();
			Node tempNode0 = null;
			Node tempNode1 = null;

			// execute player actions
			foreach (PlayerAction action in PlayerActions)
			{
				switch (action.Action)
				{
					case PlayerAction.ActionType.BuildSeg:
						// don't execute if the node no longer exists or if it has no more open segment slots
						if (NodeByID.TryGetValue((string)action.Arguments[0], out tempNode0) && tempNode0.NumSegments < tempNode0.Segments.Length)
						{
							bool onHotspot = (bool)action.Arguments[1];
							if (onHotspot)
							{
								Hotspot hs = HotspotByID[(string)action.Arguments[2]];

								tempNode1 = new Node(this, getNodeType(tempNode0.Pos, hs.Pos));
								tempNode1.OwnsHotspot = hs;
								tempNode1.Owner = action.Actor;
								tempNode1.Pos = hs.Pos;
								addNode(tempNode1);
							}
							else
							{
								tempV2 = (VectorF)action.Arguments[2];
								tempNode1 = new Node(this, getNodeType(tempNode0.Pos, tempV2));
								tempNode1.Owner = action.Actor;
								tempNode1.Pos = tempV2;
								addNode(tempNode1);
							}

							tempNode0.createSegment(tempNode1);
						}
						break;
					case PlayerAction.ActionType.DestroyNode:
						if (NodeByID.TryGetValue((string)action.Arguments[0], out tempNode0))
							tempNode0.destroy();
						break;
					case PlayerAction.ActionType.SplitSeg:
						Segment seg = null;
						if (SegByID.TryGetValue((string)action.Arguments[0], out seg))
						{
							tempV2 = (VectorF)action.Arguments[1];
							seg.split(Calc.getAdj(VectorF.Distance(tempV2, seg.Nodes[0].Pos), Calc.LinePointDistance(tempV2, seg.EndLoc[0], seg.EndLoc[1])));
						}
						break;
					case PlayerAction.ActionType.ClaimNode:
						if (NodeByID.TryGetValue((string)action.Arguments[0], out tempNode0) && NodeByID.TryGetValue((string)action.Arguments[1], out tempNode1))
							tempNode0.createSegment(tempNode1);
						break;
					default:
						throw new Exception("Unrecognized player action: " + action.ToString());
				}
			}

			PlayerActions.Clear();

			// update segments
			for (int i = 0; i < Segments.Count; i++)
			{
#if DEBUG
				if (Segments[i].Destroyed)
					throw new Exception("Segment was marked as destroyed before it was updated.");
#endif
				Segments[i].update(elapsed);

				if (Segments[i].Destroyed)
				{
					removeSegment(i);
					i--;
				}
			}

			// update nodes
			for (int i = 0; i < Nodes.Count; i++)
			{
				if (Nodes[i].Destroyed)
				{
					removeNode(i);
					i--;
					continue;
				}

				Nodes[i].update(elapsed);

				if (Nodes[i].Destroyed)
				{
					removeNode(i);
					i--;
				}
			}

			// sync AI threads
			for (int i = 0; i < Players.Count; i++)
			{
				Player player = Players[i];

				// start thread if not started
				if (player.Type == Player.PlayerType.Computer && player.AIThreadBase.TheThread == null)
					player.AIThreadBase.startThread(gt);
				else if (player.Type != Player.PlayerType.Computer || !player.AIThreadBase.Waiting || player.AIThreadBase.SyncFinished)
					continue;

				lock (player.AIThreadBase.Actions)
				{
					PlayerActions.AddRange(player.AIThreadBase.Actions);
					player.AIThreadBase.Actions.Clear();
				}
				EventsToSend.Clear();

				// sync segments
				foreach (KeyValuePair<string, WorldEvent.EventType> e in SegEvents[i])
				{
					string id = e.Key;
					WorldEvent.EventType evnt = e.Value;
					Segment tempSeg = null;

					switch (evnt)
					{
						case WorldEvent.EventType.AddSeg:
							tempSeg = SegByID[id];

							if (tempSeg.Owner == player)
								EventsToSend.Add(new WorldEvent(evnt, true, new Segment(tempSeg)));
							else
								EventsToSend.Add(new WorldEvent(evnt, false, id, tempSeg.EndLoc[0], tempSeg.State[0], tempSeg.EndLoc[1], tempSeg.State[1]));
							break;
						case WorldEvent.EventType.RemSeg:
							EventsToSend.Add(new WorldEvent(evnt, id));
							break;
						case WorldEvent.EventType.SegChangeState:
							tempSeg = SegByID[id];
							if (tempSeg.Owner == player)
								EventsToSend.Add(new WorldEvent(evnt, id, tempSeg.EndLength[0], tempSeg.State[0], tempSeg.EndLength[1], tempSeg.State[1]));
							else
								EventsToSend.Add(new WorldEvent(evnt, id, tempSeg.EndLoc[0], tempSeg.State[0], tempSeg.EndLoc[1], tempSeg.State[1]));
							break;
						default:
							throw new Exception("Unrecognized segment event: " + evnt.ToString());
					}
				}
				SegEvents[i].Clear();

				// sync nodes
				foreach (KeyValuePair<string, WorldEvent.EventType> e in NodeEvents[i])
				{
					string id = e.Key;
					WorldEvent.EventType evnt = e.Value;
					Node tempNode = null;

					switch (evnt)
					{
						case WorldEvent.EventType.AddNode:
							tempNode = NodeByID[id];

							if (tempNode.Owner == player)
								EventsToSend.Add(new WorldEvent(evnt, true, new Node(tempNode)));
							else
								EventsToSend.Add(new WorldEvent(evnt, false, id, tempNode.IsParent, tempNode.Pos, tempNode.Radius));
							break;
						case WorldEvent.EventType.RemNode:
							EventsToSend.Add(new WorldEvent(evnt, id));
							break;
						case WorldEvent.EventType.NodeChangeState:
							tempNode = NodeByID[id];

							if (tempNode.Owner != player)
								EventsToSend.Add(new WorldEvent(evnt, id, tempNode.Active));
							else
								EventsToSend.Add(new WorldEvent(evnt, id, tempNode.Active, new List<Segment>(tempNode.Segments)));
							break;
						default:
							throw new Exception("Unrecognized node event: " + evnt.ToString());
					}
				}
				NodeEvents[i].Clear();

				lock (player.AIThreadBase.Events)
					player.AIThreadBase.Events.AddRange(EventsToSend);
				EventsToSend.Clear();

				// sync density for player's system
				foreach (Node node in player.AIThreadBase.Nodes)
				{
					Node wNode = null;
					if (NodeByID.TryGetValue(node.ID, out wNode))
					{
						for (int j = 0; j < node.Segments.Length; j++)
						{
							node.SegNumPeople[j] = wNode.SegNumPeople[j];
							node.SegCapacity[j] = wNode.SegCapacity[j];
						}
					}
				}

				player.AIThreadBase.SyncFinished = true;
			}
		}

		/// <summary>Ends the game and cleans things up.</summary>
		public void endGame()
		{
			foreach (Player player in Players)
			{
				if (player.Type == Player.PlayerType.Computer)
					player.AIThreadBase.stopThread();
			}
		}

		/// <summary>Adds an event to each computer player's list so that they can be synced later.</summary>
		/// <param name="id">The id of the object involved in the event.</param>
		/// <param name="evnt">The event that took place.</param>
		public void addEvent(string id, WorldEvent.EventType evnt)
		{
			for (int i = 0; i < Players.Count; i++)
			{
				if (Players[i].Type != Player.PlayerType.Computer)
					continue;

				WorldEvent.EventType ev;
				bool exists = false;

				switch (evnt)
				{
					case WorldEvent.EventType.AddSeg:
						// add segment should always be added before any other events
#if DEBUG
						if (SegEvents[i].ContainsKey(id))
							throw new Exception("An event for segment id " + id + " already exists.");
						if (!SegByID.ContainsKey(id))
							throw new Exception("Segment id " + id + " does not exist.");
#endif
						SegEvents[i].Add(id, evnt);
						break;
					case WorldEvent.EventType.RemSeg:
						// remove segment overrides all other segment events
						exists = SegEvents[i].TryGetValue(id, out ev);

						if (exists)
							SegEvents[i].Remove(id);

						if (!exists || ev != WorldEvent.EventType.AddSeg)
							SegEvents[i].Add(id, evnt);
						break;
					case WorldEvent.EventType.SegChangeState:
						// only add if there isn't already another event added
						if (!SegEvents[i].ContainsKey(id))
							SegEvents[i].Add(id, evnt);
						break;
					case WorldEvent.EventType.AddNode:
						// add node should always be added before any other events
#if DEBUG
						if (NodeEvents[i].ContainsKey(id))
							throw new Exception("An event for node id " + id + " already exists.");
						if (!NodeByID.ContainsKey(id))
							throw new Exception("Node id " + id + " does not exist.");
#endif
						NodeEvents[i].Add(id, evnt);
						break;
					case WorldEvent.EventType.RemNode:
						// remove node overrides all other segment events
						exists = NodeEvents[i].TryGetValue(id, out ev);

						if (exists)
							NodeEvents[i].Remove(id);

						if (!exists || ev != WorldEvent.EventType.AddNode)
							NodeEvents[i].Add(id, evnt);
						break;
					case WorldEvent.EventType.NodeChangeState:
						// only add if there isn't already another event added
						if (!NodeEvents[i].ContainsKey(id))
							NodeEvents[i].Add(id, evnt);
						break;
					default:
						throw new Exception("Unrecognized event: " + evnt.ToString());
				}
			}
		}

		/// <summary>Refreshes the next player, node, segment, geo, hotspot, and node type ids.  Should be called after this world is created.</summary>
		public void refreshNextGenIDs()
		{
			NextPlayerID = 0L;
			NextNodeID = 0L;
			NextSegID = 0L;
			NextGeoID = 0L;
			NextHotspotID = 0L;
			NextNodeTypeID = 0L;

			long tempLong;

			foreach (Player player in Players)
			{
				if (!long.TryParse(player.ID, out tempLong))
					continue;
				if (tempLong >= NextPlayerID)
					NextPlayerID = tempLong + 1L;
			}

			foreach (Node node in Nodes)
			{
				if (!long.TryParse(node.ID, out tempLong))
					continue;
				if (tempLong >= NextNodeID)
					NextNodeID = tempLong + 1L;
			}

			foreach (Segment seg in Segments)
			{
				if (!long.TryParse(seg.ID, out tempLong))
					continue;
				if (tempLong >= NextSegID)
					NextSegID = tempLong + 1L;
			}

			foreach (Geo geo in Geos)
			{
				if (!long.TryParse(geo.ID, out tempLong))
					continue;
				if (tempLong >= NextGeoID)
					NextGeoID = tempLong + 1L;
			}

			foreach (Hotspot hotspot in Hotspots)
			{
				if (!long.TryParse(hotspot.ID, out tempLong))
					continue;
				if (tempLong >= NextHotspotID)
					NextHotspotID = tempLong + 1L;
			}

			foreach (NodeType node in NodeTypes)
			{
				if (!long.TryParse(node.ID, out tempLong))
					continue;
				if (tempLong >= NextNodeTypeID)
					NextNodeTypeID = tempLong + 1L;
			}
		}

		/// <summary>Refreshes the visibility of all objects in the specified area.</summary>
		/// <param name="upperLeft">The upper left corner of the area to refresh.</param>
		/// <param name="lowerRight">The lower-right corner of the area to refresh.</param>
		public void refreshVisibility(VectorF upperLeft, VectorF lowerRight)
		{
			Grid.Rect(upperLeft, lowerRight, new VectorF[] { upperLeft, lowerRight }, gridRefreshVisibility);
		}

		/// <summary>Refreshes the visibility of all objects on the map.</summary>
		public void refreshVisibility()
		{
			foreach (NodeSkel node in Nodes)
			{
				node.Visible = node.Owner.Type == Player.PlayerType.Human || (node.Active && HumanPlayer.Fog.isVisible(node));
			}

			foreach (SegmentSkel seg in Segments)
			{
				seg.Visible = seg.Owner.Type == Player.PlayerType.Human || HumanPlayer.Fog.isVisible(seg);
			}
		}

		#region Players

		/// <summary>Adds a player to the world.</summary>
		/// <param name="player">The player to add.</param>
		public void addPlayer(Player player)
		{
			if (player.ID == null)
				player.ID = getNextPlayerID();
			PlayerByID.Add(player.ID, player);
			Players.Add(player);
		}

		/// <summary>Removes a player from the world.</summary>
		/// <param name="player">The player to remove.</param>
		public void removePlayer(Player player)
		{
			PlayerByID.Remove(player.ID);
			Players.Remove(player);
		}

		/// <summary>Gets the next player id to assign.</summary>
		/// <returns>The next player id to assign.</returns>
		public string getNextPlayerID()
		{
			string id = NextPlayerID.ToString();
			NextPlayerID++;
			return id;
		}

		#endregion Players
		#region Segments

		/// <summary>Adds the specified segment to the world.</summary>
		/// <param name="segment">The segment to add.</param>
		public void addSegment(Segment segment)
		{
			if (segment.ID == null)
				segment.ID = getNextSegID();
			SegByID.Add(segment.ID, segment);
			Segments.Add(segment);
			Grid.Line(segment.Nodes[0].Pos, segment.Nodes[1].Pos, segment, gridAddSegment);

			if (segment.Owner.Type == Player.PlayerType.Human)
				segment.Visible = segment.Owner.Fog.isVisible(segment);

			if (!EditorMode)
				addEvent(segment.ID, WorldEvent.EventType.AddSeg);
		}

		/// <summary>Gets the next segment id to assign.</summary>
		/// <returns>The next segment id to assign.</returns>
		public string getNextSegID()
		{
			string id = NextSegID.ToString();
			NextSegID++;
			return id;
		}

		/// <summary>Removes the specified segment from the world.</summary>
		/// <param name="segment">The segment to remove.</param>
		public void removeSegment(Segment segment)
		{
			removeSegment(Segments.IndexOf(segment));
		}

		/// <summary>Removes the specified segment from the world.</summary>
		/// <param name="segIndex">The index of the segment to remove.</param>
		public void removeSegment(int segIndex)
		{
			Segment seg = Segments[segIndex];

			// remove from player's list
			seg.Owner = null;

			// remove from grid
			Grid.Line(seg.Nodes[0].Pos, seg.Nodes[1].Pos, seg, gridRemoveSegment);

			// nullify references to allow the garbage collector to do its thing
			seg.InWorld = null;
			seg.Nodes[0] = null;
			seg.Nodes[1] = null;

			// remove from world
			SegByID.Remove(seg.ID);
			Segments.RemoveAt(segIndex);

			// add event
			if (!EditorMode)
				addEvent(seg.ID, WorldEvent.EventType.RemSeg);
		}

		/// <summary>Find a segment owned by the specified player that overlaps the specified point.</summary>
		/// <param name="point">The point to search for.</param>
		/// <param name="owner">The player that owns the segment.</param>
		/// <param name="checkEnds">If true, checks to see if the point is near one of the end nodes.  Otherwise, sees if the point overlaps the active portion of the segment.</param>
		public Segment segmentAtPoint(VectorF point, Player owner)
		{
			object[] data = new object[3];
			data[0] = point;
			data[1] = owner;

			Grid.PointExpand(point, data, gridSegmentAtPoint);

			return (Segment)data[2];
		}

		/// <summary>Finds a segment end that overlaps the specified point.</summary>
		/// <param name="point">The point to search.</param>
		/// <returns>A segment end that overlaps the specified point.</returns>
		public Node SegmentEndAtPoint(VectorF point)
		{
			object[] data = new object[2];
			data[0] = point;

			Grid.PointExpand(point, data, gridSegmentEndAtPoint);

			return (Node)data[1];
		}

		#endregion Segments
		#region Nodes

		/// <summary>Adds the specified node to the world.</summary>
		/// <param name="node">The node to add.</param>
		public void addNode(Node node)
		{
			if (node.ID == null)
				node.ID = getNextNodeID();
			NodeByID.Add(node.ID, node);
			Nodes.Add(node);
			Grid.Point(node.Pos, node, gridAddNode);

			node.Visible = node.Owner != null && (node.Owner.Type == Player.PlayerType.Human || (node.Active && HumanPlayer.Fog.isVisible(node)));

			if (!EditorMode)
				addEvent(node.ID, WorldEvent.EventType.AddNode);
		}

		/// <summary>Gets the next node id to assign.</summary>
		/// <returns>The next node id to assign.</returns>
		public string getNextNodeID()
		{
			string id = NextNodeID.ToString();
			NextNodeID++;
			return id;
		}

		/// <summary>Removes the specified node from the world.</summary>
		/// <param name="node">The node to remove.</param>
		public void removeNode(Node node)
		{
			removeNode(Nodes.IndexOf(node));
		}

		/// <summary>Removes the specified node from the world.</summary>
		/// <param name="nodeIndex">The index of the node to remove.</param>
		public void removeNode(int nodeIndex)
		{
			Node node = Nodes[nodeIndex];

			// prepare to invalidate fog of war
			Player invalOwner = null;
			VectorF upperLeft = VectorF.Zero;
			VectorF lowerRight = VectorF.Zero;

			if (node.Active && node.Owner != null && node.Owner.Type == Player.PlayerType.Human)
			{
				invalOwner = node.Owner;
				upperLeft = new VectorF(node.X - node.SightDistance, node.Y - node.SightDistance);
				lowerRight = new VectorF(node.X + node.SightDistance, node.Y + node.SightDistance);
			}

			// remove from player's list
			node.Owner = null;

			// remove from grid
			Grid.Point(node.Pos, node, gridRemoveNode);

			// nullify references so the garbage collector can do its thing
			node.InWorld = null;
			node.Parents.Clear();

			for (int i = 0; i < node.Segments.Length; i++)
			{
				node.Segments[i] = null;
			}

			// remove from world
			NodeByID.Remove(node.ID);
			Nodes.RemoveAt(nodeIndex);

			// invalidate
			if (invalOwner != null)
			{
				invalOwner.Fog.invalidate(upperLeft, lowerRight);
				refreshVisibility(upperLeft, lowerRight);
			}

			// add event
			if (!EditorMode)
				addEvent(node.ID, WorldEvent.EventType.RemNode);
		}

		/// <summary>Find a node that overlaps the specified point.</summary>
		/// <param name="point">The point to search.</param>
		/// <param name="activeOnly">Whether or not to test only active nodes.</param>
		/// <returns>A node that overlaps the specified point.</returns>
		public Node NodeAtPoint(VectorF point, bool activeOnly)
		{
			object[] data = new object[3];
			data[0] = point;
			data[1] = activeOnly;
			data[2] = null;

			Grid.PointExpand(point, data, gridNodeAtPoint);

			return (Node)data[2];
		}

		#endregion Nodes
		#region Geos

		/// <summary>Adds the provided geometric obstacle to the world.</summary>
		/// <param name="geo">The geometri obstacle to add to the world.</param>
		public void addGeo(Geo geo)
		{
			if (geo.ID == null)
				geo.ID = getNextGeoID();
			Geos.Add(geo);
			GeoByID.Add(geo.ID, geo);
			Grid.Rect(geo.UpperLeft, geo.LowerRight, geo, gridAddGeo);
		}

		/// <summary>Gets the next geo id to assign.</summary>
		public string getNextGeoID()
		{
			string id = NextGeoID.ToString();
			NextGeoID++;
			return id;
		}

		/// <summary>Removes the specified geometric obstacle from the world.</summary>
		/// <param name="geo">The geometric obstacle to remove.</param>
		public void removeGeo(Geo geo)
		{
			removeGeo(Geos.IndexOf(geo));
		}

		/// <summary>Removes the specified geometric obstacle from the world.</summary>
		/// <param name="geoIndex">The index of the geometric obstacle to remove.</param>
		public void removeGeo(int geoIndex)
		{
			Geo geo = Geos[geoIndex];
			Grid.Rect(geo.UpperLeft, geo.LowerRight, geo, gridRemoveGeo);
			GeoByID.Remove(geo.ID);
			Geos.RemoveAt(geoIndex);
		}

		/// <summary>Finds a geo that overlaps the provided point.</summary>
		/// <param name="point">The point to test.</param>
		/// <param name="vertex">The vertex or first vertex of the line that was overlapped.</param>
		/// <param name="verticesOnly">If true, only tests vertices.  Otherwise tests lines.</param>
		/// <returns>A geo that overlaps the provided point.</returns>
		public Geo geoAtPoint(VectorF point, out int vertex, bool verticesOnly)
		{
			object[] data = new object[3];
			data[0] = point;

			if (verticesOnly)
				Grid.PointExpand(point, data, gridGeoVertexAtPoint);
			else
				Grid.PointExpand(point, data, gridGeoLineAtPoint);

			if (data[1] != null)
			{
				vertex = (int)data[2];
				return (Geo)data[1];
			}

			vertex = -1;
			return null;
		}

		#endregion Geos
		#region Hotspots

		/// <summary>Adds the specified hotspot to the world.</summary>
		/// <param name="hotspot">The hotspot to add.</param>
		public void addHotspot(Hotspot hotspot)
		{
			if (hotspot.ID == null)
				hotspot.ID = getNextHotspotID();
			HotspotByID.Add(hotspot.ID, hotspot);
			Hotspots.Add(hotspot);
			Grid.Point(hotspot.Pos, hotspot, gridAddHotspot);
		}

		/// <summary>Gets the next hotspot id to assign.</summary>
		/// <returns>The next hotspot id to assign.</returns>
		public string getNextHotspotID()
		{
			string id = NextHotspotID.ToString();
			NextHotspotID++;
			return id;
		}

		/// <summary>Removes the specified hotspot from the world.</summary>
		/// <param name="hotspot">The hotspot to remove.</param>
		public void removeHotspot(Hotspot hotspot)
		{
			removeHotspot(Hotspots.IndexOf(hotspot));
		}

		/// <summary>Removes the specified hotspot from the world.</summary>
		/// <param name="hotspotIndex">The index of the hotspot to remove.</param>
		public void removeHotspot(int hotspotIndex)
		{
			Hotspot hotspot = Hotspots[hotspotIndex];

			// remove from grid
			Grid.Point(hotspot.Pos, hotspot, gridRemoveHotspot);

			// nullify references so the garbage collector can do its thing
			hotspot.InWorld = null;

			// remove from world
			HotspotByID.Remove(hotspot.ID);
			Hotspots.RemoveAt(hotspotIndex);
		}

		/// <summary>Find a hotspot that overlaps the specified point.</summary>
		/// <param name="point">The point to search.</param>
		/// <returns>A hotspot that overlaps the specified point.</returns>
		public Hotspot HotspotAtPoint(VectorF point)
		{
			object[] data = new object[2];
			data[0] = point;
			data[1] = null;

			Grid.PointExpand(point, data, gridHotspotAtPoint);

			return (Hotspot)data[1];
		}

		#endregion Hotspots
		#region Node Types

		/// <summary>Adds the provided node type to the world.</summary>
		/// <param name="nodeType">The node type to add.</param>
		public void addNodeType(NodeType nodeType)
		{
			if (nodeType.ID == null)
				nodeType.ID = getNextNodeTypeID();
			NodeTypes.Add(nodeType);
			NodeTypeByID.Add(nodeType.ID, nodeType);
		}

		/// <summary>Gets the next node type id to assign.</summary>
		/// <returns>The next node type id to assign.</returns>
		public string getNextNodeTypeID()
		{
			string id = NextNodeTypeID.ToString();
			NextNodeTypeID++;
			return id;
		}

		/// <summary>Sorts the node types by their BuildRangeMin values in ascending order.</summary>
		public void sortNodeTypes()
		{
			// bubble sort
			int iEnd = NodeTypes.Count - 1;
			for (int i = 0; i < iEnd; i++)
			{
				for (int j = 0, end = iEnd - i; j < end; j++)
				{
					int j1 = j + 1;
					if (NodeTypes[j].BuildRangeMin > NodeTypes[j1].BuildRangeMin)
					{
						// swap them
						NodeType tempType = NodeTypes[j];
						NodeTypes[j] = NodeTypes[j1];
						NodeTypes[j1] = tempType;
					}
				}
			}
		}

		/// <summary>Gets the node type that may be built at the determined range.</summary>
		/// <param name="srcPoint">The position of the source node.</param>
		/// <param name="destPoint">The position of the new node.</param>
		/// <returns>The node type that may be built at the determined range.</returns>
		public NodeType getNodeType(VectorF srcPoint, VectorF destPoint)
		{
			return getNodeType(VectorF.Distance(srcPoint, destPoint));
		}

		/// <summary>Gets the node type that may be built at the determined range.</summary>
		/// <param name="dist">The distance that the node will be built from the source node.</param>
		public NodeType getNodeType(FInt dist)
		{
			NodeType selType = null;
			foreach (NodeType type in NodeTypes)
			{
				if (type.BuildRangeMin <= dist)
					selType = type;
				else
					break;
			}

			return selType;
		}

		/// <summary>Removes the specified node type from the world.</summary>
		/// <param name="nodeType">The node type to remove.</param>
		private void removeNodeType(NodeType nodeType)
		{
			removeNodeType(NodeTypes.IndexOf(nodeType));
		}

		/// <summary>Removes the specified node type from the world.</summary>
		/// <param name="typeIndex">The index of the node type to remove.</param>
		private void removeNodeType(int typeIndex)
		{
			NodeTypeByID.Remove(NodeTypes[typeIndex].ID);
			NodeTypes.RemoveAt(typeIndex);
		}

		#endregion Node Types
		#region Grid Actions

		/// <summary>Adds the provided segment to the specified grid square.</summary>
		/// <param name="sqr">The grid square to add the segment to.</param>
		/// <param name="segment">The segment to add to the grid square.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		public void gridAddSegment(GridSqr sqr, object segment, TimeSpan time, int opID)
		{
			((Segment)segment).LastCheck = time;
			((Segment)segment).LastCheckNum = opID;
			sqr.Segments.Add((Segment)segment);
		}

		/// <summary>Removes the provided segment from the specified grid square.</summary>
		/// <param name="sqr">The grid square to remove the segment from.</param>
		/// <param name="segment">The segment to remove from the grid square.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		public void gridRemoveSegment(GridSqr sqr, object segment, TimeSpan time, int opID)
		{
			((Segment)segment).LastCheck = time;
			((Segment)segment).LastCheckNum = opID;
			sqr.Segments.Remove((Segment)segment);
		}

		/// <summary>Search the specified grid square for a segment owned by the specified player that overlaps the specified point.</summary>
		/// <param name="sqr">The grid square to search.</param>
		/// <param name="array">An array containing the following values: the point to search, the owner of the segment (null if any), and the segment, if found.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridSegmentAtPoint(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			VectorF point = (VectorF)((object[])array)[0];
			Player owner = (Player)((object[])array)[1];

			FInt top = new FInt();
			FInt left = new FInt();
			FInt right = new FInt();
			FInt bottom = new FInt();

			// TODO: specify threshold
			FInt thresh = (FInt)10;

			foreach (Segment seg in sqr.Segments)
			{
				if (seg.LastCheck != time || seg.LastCheckNum != opID)
				{
					seg.LastCheck = time;
					seg.LastCheckNum = opID;

					// get bounding box
					if (seg.EndLoc[0].X < seg.EndLoc[1].X)
					{
						left = seg.EndLoc[0].X;
						right = seg.EndLoc[1].X;
					}
					else
					{
						left = seg.EndLoc[1].X;
						right = seg.EndLoc[0].X;
					}

					if (seg.EndLoc[0].Y < seg.EndLoc[1].Y)
					{
						top = seg.EndLoc[0].Y;
						bottom = seg.EndLoc[1].Y;
					}
					else
					{
						top = seg.EndLoc[1].Y;
						bottom = seg.EndLoc[0].Y;
					}

					// test segment
					if ((seg.Owner == owner || owner == null)
						&& point.X >= left - thresh
						&& point.X <= right + thresh
						&& point.Y >= top - thresh
						&& point.Y <= bottom + thresh
						&& seg.overlapsPoint(point))
					{
						((object[])array)[2] = seg;
						Grid.CancelOperation = true;
						break;
					}
				}
			}
		}

		/// <summary>Search the specified grid square for a segment end that overlaps the specified point.</summary>
		/// <param name="sqr">The grid square to search.</param>
		/// <param name="array">An array containing the following values: the point to search, and the segment end, if found.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridSegmentEndAtPoint(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			VectorF point = (VectorF)((object[])array)[0];

			// TODO: specify threshold
			FInt thresh = (FInt)20;

			foreach (Segment seg in sqr.Segments)
			{
				if (seg.LastCheck != time || seg.LastCheckNum != opID)
				{
					seg.LastCheck = time;
					seg.LastCheckNum = opID;

					// test segment
					if (point.X >= seg.Nodes[0].X - thresh
						&& point.X <= seg.Nodes[0].X + thresh
						&& point.Y >= seg.Nodes[0].Y - thresh
						&& point.Y <= seg.Nodes[0].Y + thresh
						&& !Nodes.Contains(seg.Nodes[0])
						&& VectorF.Distance(point, seg.Nodes[0].Pos) <= thresh)
					{
						((object[])array)[1] = seg.Nodes[0];
					}
					else if (point.X >= seg.Nodes[1].X - thresh
						&& point.X <= seg.Nodes[1].X + thresh
						&& point.Y >= seg.Nodes[1].Y - thresh
						&& point.Y <= seg.Nodes[1].Y + thresh
						&& !Nodes.Contains(seg.Nodes[1])
						&& VectorF.Distance(point, seg.Nodes[1].Pos) <= thresh)
					{
						((object[])array)[1] = seg.Nodes[1];
					}
					else
					{
						continue;
					}

					break;
				}
			}

			if (((object[])array)[1] != null)
				Grid.CancelOperation = true;
		}

		/// <summary>Adds the provided node to the specified grid square.</summary>
		/// <param name="sqr">The grid square to add the node to.</param>
		/// <param name="node">The node to add to the grid square.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		public void gridAddNode(GridSqr sqr, object node, TimeSpan time, int opID)
		{
			((Node)node).LastCheck = time;
			((Node)node).LastCheckNum = opID;
			sqr.Nodes.Add((Node)node);
		}

		/// <summary>Removes the provided node from the specified grid square.</summary>
		/// <param name="sqr">The grid square to remove the node from.</param>
		/// <param name="node">The node to remove from the grid square.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		public void gridRemoveNode(GridSqr sqr, object node, TimeSpan time, int opID)
		{
			((Node)node).LastCheck = time;
			((Node)node).LastCheckNum = opID;
			sqr.Nodes.Remove((Node)node);
		}

		/// <summary>Search the specified grid square for a node that overlaps the specified point.</summary>
		/// <param name="sqr">The grid square to search.</param>
		/// <param name="array">An array containing the following values: the point to search, whether or not to test only active nodes, and the node, if found.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridNodeAtPoint(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			object[] data = (object[])array;
			VectorF point = (VectorF)data[0];
			bool activeOnly = (bool)data[1];
#if DEBUG
			if (data[2] != null)
				throw new Exception("The grid node was already found, but the operation was not cancelled.");
#endif
			foreach (Node node in sqr.Nodes)
			{
				// make sure we haven't already tested this node
				if ((node.LastCheck != time || node.LastCheckNum != opID) && (node.Active || !activeOnly))
				{
					VectorF pos = node.Pos;
					FInt xDiff = point.X - pos.X;
					FInt yDiff = point.Y - pos.Y;
					if (Calc.Sqrt((xDiff * xDiff) + (yDiff * yDiff)) <= node.Radius)
					{
						data[2] = node;
						Grid.CancelOperation = true;
					}

					node.LastCheck = time;
					node.LastCheckNum = opID;
				}
			}
		}

		/// <summary>Adds the provided geometric obstacle to the specified grid square.</summary>
		/// <param name="sqr">The grid square to add the geometric obstacle to.</param>
		/// <param name="array">The geometric obstacle to add to the grid square.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		public void gridAddGeo(GridSqr sqr, object geo, TimeSpan time, int opID)
		{
			((Geo)geo).LastCheck = time;
			((Geo)geo).LastCheckNum = opID;
			sqr.Geos.Add((Geo)geo);
		}

		/// <summary>Removes the provided geometric obstacle from the specified grid square.</summary>
		/// <param name="sqr">The grid square to remove the geometric obstacle from.</param>
		/// <param name="geo">The geometric obstacle to remove from the grid square.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		public void gridRemoveGeo(GridSqr sqr, object geo, TimeSpan time, int opID)
		{
			((Geo)geo).LastCheck = time;
			((Geo)geo).LastCheckNum = opID;
			sqr.Geos.Remove((Geo)geo);
		}

		/// <summary>Searches for specified grid square for the a geo vertex that overlaps the provided point.</summary>
		/// <param name="sqr">The grid square to search.</param>
		/// <param name="array">An array that contains the following values: the point to search, the geo (if found), and the vertex (if found).</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridGeoVertexAtPoint(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			VectorF point = (VectorF)((object[])array)[0];

			// TODO: specify threshold
			FInt thresh = (FInt)10;

			foreach (Geo geo in sqr.Geos)
			{
				if (geo.LastCheck == time && geo.LastCheckNum == opID)
					continue;
				
				geo.LastCheck = time;
				geo.LastCheckNum = opID;

				// test bounding box
				if (point.X < geo.Left - thresh
					|| point.X > geo.Right + thresh
					|| point.Y < geo.Top - thresh
					|| point.Y > geo.Bottom + thresh)
				{
					continue;
				}

				// test all vertices
				for (int i = 0; i < geo.Vertices.Length; i += 2)
				{
					if (point.X > geo.Vertices[i].X - thresh
						&& point.X < geo.Vertices[i].X + thresh
						&& point.Y > geo.Vertices[i].Y - thresh
						&& point.Y < geo.Vertices[i].Y + thresh)
					{
						((object[])array)[1] = geo;
						((object[])array)[2] = i;
						Grid.CancelOperation = true;
						return;
					}
				}

				if (geo.Vertices.Length > 1)
				{
					// test last vertex
					int i = geo.Vertices.Length - 1;
					if (point.X > geo.Vertices[i].X - thresh
						&& point.X < geo.Vertices[i].X + thresh
						&& point.Y > geo.Vertices[i].Y - thresh
						&& point.Y < geo.Vertices[i].Y + thresh)
					{
						((object[])array)[1] = geo;
						((object[])array)[2] = i;
						Grid.CancelOperation = true;
						return;
					}

					// test center
					if (geo.Vertices.Length > 2
						&& point.X > geo.Center.X - thresh
						&& point.X < geo.Center.X + thresh
						&& point.Y > geo.Center.Y - thresh
						&& point.Y < geo.Center.Y + thresh)
					{
						((object[])array)[1] = geo;
						((object[])array)[2] = -1;
						Grid.CancelOperation = true;
						return;
					}
				}
			}
		}

		/// <summary>Searches for specified grid square for the a geo line that overlaps the provided point.</summary>
		/// <param name="sqr">The grid square to search.</param>
		/// <param name="array">An array that contains the following values: the point to search, the geo (if found), and the first vertex of the line (if found).</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridGeoLineAtPoint(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			VectorF point = (VectorF)((object[])array)[0];

			// TODO: specify threshold
			FInt thresh = (FInt)10;

			foreach (Geo geo in sqr.Geos)
			{
				if (geo.LastCheck == time && geo.LastCheckNum == opID)
					continue;

				geo.LastCheck = time;
				geo.LastCheckNum = opID;

				// test bounding box
				if (geo.Vertices.Length < 2
					|| point.X < geo.Left - thresh
					|| point.X > geo.Right + thresh
					|| point.Y < geo.Top - thresh
					|| point.Y > geo.Bottom + thresh)
				{
					continue;
				}

				// test all lines
				for (int i = 0, max = geo.Vertices.Length; i < max; i += 2)
				{
					int bi = i / 2;

					if (point.X > geo.LineTopLeft[bi].X - thresh
						&& point.X < geo.LineLowerRight[bi].X + thresh
						&& point.Y > geo.LineTopLeft[bi].Y - thresh
						&& point.Y < geo.LineLowerRight[bi].Y + thresh
						&& Calc.LinePointDistance(point, geo.Vertices[i], geo.Vertices[i + 1]) < thresh)
					{
						((object[])array)[1] = geo;
						((object[])array)[2] = i;
						Grid.CancelOperation = true;
						return;
					}
				}

				if (geo.CloseLoop)
				{
					int i = geo.Vertices.Length - 1;
					int bi = geo.LineLowerRight.Length - 1;

					if (point.X > geo.LineTopLeft[bi].X - thresh
						&& point.X < geo.LineLowerRight[bi].X + thresh
						&& point.Y > geo.LineTopLeft[bi].Y - thresh
						&& point.Y < geo.LineLowerRight[bi].Y + thresh
						&& Calc.LinePointDistance(point, geo.Vertices[i], geo.Vertices[0]) < thresh)
					{
						((object[])array)[1] = geo;
						((object[])array)[2] = i;
						Grid.CancelOperation = true;
						return;
					}
				}
			}
		}

		/// <summary>Adds the provided hotspot to the specified grid square.</summary>
		/// <param name="sqr">The grid square to add the hotspot to.</param>
		/// <param name="hotspot">The hotspot to add to the grid square.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		public void gridAddHotspot(GridSqr sqr, object hotspot, TimeSpan time, int opID)
		{
			((Hotspot)hotspot).LastCheck = time;
			((Hotspot)hotspot).LastCheckNum = opID;
			sqr.Hotspots.Add((Hotspot)hotspot);
		}

		/// <summary>Removes the provided hotspot from the specified grid square.</summary>
		/// <param name="sqr">The grid square to remove the hotspot from.</param>
		/// <param name="hotspot">The hotspot to remove the grid square from.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		public void gridRemoveHotspot(GridSqr sqr, object hotspot, TimeSpan time, int opID)
		{
			((Hotspot)hotspot).LastCheck = time;
			((Hotspot)hotspot).LastCheckNum = opID;
			sqr.Hotspots.Remove((Hotspot)hotspot);
		}

		/// <summary>Search the specified grid square for a hotspot that overlaps the specified point.</summary>
		/// <param name="sqr">The grid square to search.</param>
		/// <param name="array">An array containing the following values: the point to search and the hotspot, if found.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridHotspotAtPoint(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			object[] data = (object[])array;
			VectorF point = (VectorF)data[0];
#if DEBUG
			if (data[1] != null)
				throw new Exception("The grid hotspot was already found, but the operation was not cancelled.");
#endif
			foreach (Hotspot hotspot in sqr.Hotspots)
			{
				// make sure we haven't already tested this hotspot
				if (hotspot.LastCheck != time || hotspot.LastCheckNum != opID)
				{
					VectorF pos = hotspot.Pos;
					FInt xDiff = point.X - pos.X;
					FInt yDiff = point.Y - pos.Y;
					if (Calc.Sqrt((xDiff * xDiff) + (yDiff * yDiff)) <= (FInt)50) // TODO: get rid of magic number
					{
						data[1] = hotspot;
						Grid.CancelOperation = true;
					}

					hotspot.LastCheck = time;
					hotspot.LastCheckNum = opID;
				}
			}
		}

		/// <summary>Refreshes the visibility of all objects in the specified grid square.</summary>
		/// <param name="sqr">The grid square to refresh.</param>
		/// <param name="array">An array that contains the upper-left corner and the lower-right corner of the area to search.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridRefreshVisibility(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			VectorF topLeft = ((VectorF[])array)[0];
			VectorF bottomRight = ((VectorF[])array)[1];

			foreach (NodeSkel node in sqr.Nodes)
			{
				if (node.LastCheck == time && node.LastCheckNum == opID)
					continue;

				node.LastCheck = time;
				node.LastCheckNum = opID;

				if (node.Owner.Type == Player.PlayerType.Human
					|| node.X - node.Radius > bottomRight.X
					|| node.X + node.Radius < topLeft.X
					|| node.Y - node.Radius > bottomRight.Y
					|| node.Y + node.Radius < topLeft.Y)
					continue;

				node.Visible = HumanPlayer.Fog.isVisible(node);
			}

			foreach (SegmentSkel seg in sqr.Segments)
			{
				if (seg.LastCheck == time && seg.LastCheckNum == opID)
					continue;

				seg.LastCheck = time;
				seg.LastCheckNum = opID;

				if (seg.Owner.Type == Player.PlayerType.Human
					|| Calc.Min(seg.EndLoc[0].X, seg.EndLoc[1].X) > bottomRight.X
					|| Calc.Max(seg.EndLoc[0].X, seg.EndLoc[1].X) < topLeft.X
					|| Calc.Min(seg.EndLoc[0].Y, seg.EndLoc[1].Y) > bottomRight.Y
					|| Calc.Max(seg.EndLoc[0].Y, seg.EndLoc[1].Y) < topLeft.Y)
					continue;

				seg.Visible = HumanPlayer.Fog.isVisible(seg);
			}
		}

		#endregion Grid Actions

		#endregion Methods
	}
}
