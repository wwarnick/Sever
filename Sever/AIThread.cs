using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sever
{
	public class AIThread
	{
		#region Members

		/// <summary>The player that owns this thread.</summary>
		public Player Owner { get; set; }

		/// <summary>The actual thread.</summary>
		public Thread TheThread { get; set; }

		/// <summary>Whether or not this thread is waiting to be synced in between cycles.  Actually a bool type.  Stored as an object so that it can be locked.</summary>
		private object waiting;

		/// <summary>Whether or not this thread is waiting to be synced in between cycles.</summary>
		public bool Waiting
		{
			get
			{
				lock (waiting)
					return (bool)waiting;
			}
			private set
			{
				lock (waiting)
					waiting = value;
			}
		}

		/// <summary>Whether or not the AI is done syncing with the main thread.  Actually a bool type.  Stored as an object so that it can be locked.</summary>
		private object syncFinished;

		/// <summary>Whether or not the AI is done syncing with the main thread.</summary>
		public bool SyncFinished
		{
			get
			{
				lock (syncFinished)
					return (bool)syncFinished;
			}
			set
			{
				lock (syncFinished)
					syncFinished = value;
			}
		}

		/// <summary>Whether or not this thread should stop running.  Actually a bool type.  Stored as an object so that it can be locked.</summary>
		private object stop;

		/// <summary>Whether or not this thread should stop running.</summary>
		private bool Stop
		{
			get
			{
				lock (stop)
					return (bool)stop;
			}
			set
			{
				lock (stop)
					stop = value;
			}
		}

		/// <summary>The AI's skeleton grid manager.</summary>
		private GridManager grid;

		/// <summary>The AI's skeleton grid manager.</summary>
		public GridManager Grid
		{
			get { return grid; }
			set
			{
				grid = value;
				if (Collision == null)
					Collision = new CollisionManager();
				Collision.Grid = grid;
			}
		}

		/// <summary>The collision manager to use.</summary>
		private CollisionManager Collision { get; set; }

		/// <summary>The AI's path finder.</summary>
		public PathFinder Path { get; set; }

		/// <summary>The AI player's list of nodes.</summary>
		public List<Node> Nodes { get; set; }

		/// <summary>The AI player's list of segments.</summary>
		public List<Segment> Segments { get; set; }

		/// <summary>A dictionary of all nodes by id.</summary>
		public Dictionary<string, NodeSkel> NodeByID { get; private set; }

		/// <summary>A dictionary of all segments by id.</summary>
		public Dictionary<string, SegmentSkel> SegByID { get; private set; }

		/// <summary>The list of actions that this player wishes to perform.</summary>
		public List<PlayerAction> Actions { get; private set; }

		/// <summary>A list of events that occurred in the world since the last sync.</summary>
		public List<WorldEvent> Events { get; private set; }

		/// <summary>Whether or not the AI's destination is due for an update.</summary>
		public bool destUpdateDue { get; set; }

		/// <summary>The current game time.</summary>
		public Microsoft.Xna.Framework.GameTime CurTime { get; private set; }

		#endregion Members

		#region Constructors and Destructors

		/// <summary>Creates a new instance of AIThread.</summary>
		public AIThread()
		{
			clear();
		}

		/// <summary>Creates a new instance of AIThread.</summary>
		/// <param name="owner">The player that owns this thread.</param>
		public AIThread(Player owner)
			: this()
		{
			Owner = owner;
		}

		~AIThread()
		{
			stopThread();
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public void clear()
		{
			Owner = null;
			TheThread = null;
			waiting = false;
			syncFinished = false;
			stop = false;
			Grid = null;
			Path = null;
			Nodes = new List<Node>();
			Segments = new List<Segment>();
			NodeByID = new Dictionary<string, NodeSkel>();
			SegByID = new Dictionary<string, SegmentSkel>();
			Actions = new List<PlayerAction>();
			Events = new List<WorldEvent>();
			destUpdateDue = true;
			CurTime = null;
		}

		/// <summary>Builds the dictionaries of all nodes and segments.</summary>
		public void buildDictionaries(NodeSkel[] nodes, SegmentSkel[] segs)
		{
			foreach (NodeSkel node in nodes)
			{
				NodeByID.Add(node.ID, node);
			}

			foreach (SegmentSkel seg in segs)
			{
				SegByID.Add(seg.ID, seg);
			}
		}

		/// <summary>Starts the thread.</summary>
		public void startThread(Microsoft.Xna.Framework.GameTime curTime)
		{
			Waiting = false;
			Stop = false;
			CurTime = curTime;
			TheThread = new System.Threading.Thread(run);
			TheThread.Start();
		}

		/// <summary>Tells the thread to stop at the end of its current cycle.</summary>
		public void stopThread()
		{
			Stop = true;
		}

		/// <summary>Runs the thread.</summary>
		private void run()
		{
			List<WorldEvent> evnts = new List<WorldEvent>();
			while (true)
			{
				evnts.Clear();
				Waiting = true;
				while (Waiting)
				{
					if (Stop)
						break;

					if (SyncFinished)
					{
						lock (Events)
						{
							evnts.AddRange(Events);
							Events.Clear();
						}

						lock (waiting)
						{
							lock (syncFinished)
							{
								waiting = false;
								syncFinished = false;
							}
						}
					}
				}

				if (Stop)
					break;

				Grid.startNewUpdate(CurTime);

				#region Sync
				{
					List<Node> newNodes = new List<Node>();
					List<Segment> newSegs = new List<Segment>();
					List<Node> updateNodes = new List<Node>();
					List<List<Segment>> updateNodeSegs = new List<List<Segment>>();
					SegmentSkel tempSeg = null;
					NodeSkel tempNode = null;
					foreach (WorldEvent evnt in evnts)
					{
						switch (evnt.WEvent)
						{
							// add segment
							case WorldEvent.EventType.AddSeg:
								if ((bool)evnt.Arguments[0]) // if owned by this player
								{
									tempSeg = (Segment)evnt.Arguments[1];
									Segments.Add((Segment)tempSeg);
									newSegs.Add((Segment)tempSeg);
									Path.intersect(tempSeg.EndLoc[0], tempSeg.EndLoc[1], 1);
									tempSeg.Visible = true;
								}
								else
								{
									tempSeg = new SegmentSkel();
									tempSeg.ID = (string)evnt.Arguments[1];
									tempSeg.EndLoc[0] = (VectorF)evnt.Arguments[2];
									tempSeg.State[0] = (SegmentSkel.SegState)evnt.Arguments[3];
									tempSeg.EndLoc[1] = (VectorF)evnt.Arguments[4];
									tempSeg.State[1] = (SegmentSkel.SegState)evnt.Arguments[5];
									lock (Owner.Fog) { tempSeg.Visible = Owner.Fog.isVisible(tempSeg); }
								}

								SegByID.Add(tempSeg.ID, tempSeg);
								Grid.Line(tempSeg.EndLoc[0], tempSeg.EndLoc[1], tempSeg, gridAddSegment);
								break;

							// remove segment
							case WorldEvent.EventType.RemSeg:
								tempSeg = SegByID[(string)evnt.Arguments[0]];
								if (tempSeg.Owner == Owner)
								{
									Segments.Remove((Segment)tempSeg);
									Path.intersect(tempSeg.EndLoc[0], tempSeg.EndLoc[1], -1);
								}
								SegByID.Remove(tempSeg.ID);
								Grid.Line(tempSeg.EndLoc[0], tempSeg.EndLoc[1], tempSeg, gridRemoveSegment);
								break;

							// change segment state
							case WorldEvent.EventType.SegChangeState:
								tempSeg = SegByID[(string)evnt.Arguments[0]];
								Grid.Line(tempSeg.EndLoc[0], tempSeg.EndLoc[1], tempSeg, gridRemoveSegment);

								if (tempSeg.Owner == Owner)
								{
									Segment s = (Segment)tempSeg;
									FInt l0 = (FInt)evnt.Arguments[1];
									FInt l1 = (FInt)evnt.Arguments[3];

									bool reAdd = false;
									if (s.EndLength[0] != l0 || s.EndLength[1] != l1)
									{
										Path.intersect(s.EndLoc[0], s.EndLoc[1], -1);
										reAdd = true;
									}

									s.EndLength[0] = l0;
									s.State[0] = (SegmentSkel.SegState)evnt.Arguments[2];
									s.EndLength[1] = l1;
									s.State[1] = (SegmentSkel.SegState)evnt.Arguments[4];
									s.refreshEndLocs();

									if (reAdd)
										Path.intersect(s.EndLoc[0], s.EndLoc[1], 1);
								}
								else
								{
									tempSeg.EndLoc[0] = (VectorF)evnt.Arguments[1];
									tempSeg.State[0] = (SegmentSkel.SegState)evnt.Arguments[2];
									tempSeg.EndLoc[1] = (VectorF)evnt.Arguments[3];
									tempSeg.State[1] = (SegmentSkel.SegState)evnt.Arguments[4];
									lock (Owner.Fog) { tempSeg.Visible = Owner.Fog.isVisible(tempSeg); }
								}
								Grid.Line(tempSeg.EndLoc[0], tempSeg.EndLoc[1], tempSeg, gridAddSegment);
								break;

							// add node
							case WorldEvent.EventType.AddNode:
								if ((bool)evnt.Arguments[0]) // if owned by this player
								{
									tempNode = (Node)evnt.Arguments[1];
									tempNode.Visible = true;
									Nodes.Add((Node)tempNode);
									newNodes.Add((Node)tempNode);
								}
								else
								{
									tempNode = new NodeSkel();
									tempNode.ID = (string)evnt.Arguments[1];
									tempNode.IsParent = (bool)evnt.Arguments[2];
									tempNode.Pos = (VectorF)evnt.Arguments[3];
									tempNode.Radius = (FInt)evnt.Arguments[4];
									lock (Owner.Fog) { tempNode.Visible = Owner.Fog.isVisible(tempNode); }
								}

								NodeByID.Add(tempNode.ID, tempNode);
								Grid.Point(tempNode.Pos, tempNode, gridAddNode);

								//if ((bool)evnt.Arguments[0] || tempNode.Active)
								//	Path.intersect(tempNode.Pos, tempNode.Radius, 1);

								if ((bool)evnt.Arguments[0] && tempNode.Active) // if owned by this player
									refreshVisibility(new VectorF(tempNode.X - tempNode.SightDistance, tempNode.Y - tempNode.SightDistance), new VectorF(tempNode.X + tempNode.SightDistance, tempNode.Y + tempNode.SightDistance));

								break;

							// change node state
							case WorldEvent.EventType.NodeChangeState:
								tempNode = NodeByID[(string)evnt.Arguments[0]];

								if (tempNode.Owner == Owner) // if owned by this player
								{
									Node realNode = (Node)tempNode;
									bool applyVisibility = !realNode.Active && (bool)evnt.Arguments[1];
									realNode.Active = (bool)evnt.Arguments[1];
									List<Segment> segs = (List<Segment>)evnt.Arguments[2];

									updateNodes.Add(realNode);
									updateNodeSegs.Add(segs);

									if (applyVisibility)
									{
										lock (Owner.Fog) { Owner.Fog.applyVisibility(realNode); }
										refreshVisibility(new VectorF(tempNode.X - tempNode.SightDistance, tempNode.Y - tempNode.SightDistance), new VectorF(tempNode.X + tempNode.SightDistance, tempNode.Y + tempNode.SightDistance));
									}
								}
								else
								{
									/*if ((bool)evnt.Arguments[1] && !tempNode.Active)
										Path.intersect(tempNode.Pos, tempNode.Radius, 1);
									else if (!(bool)evnt.Arguments[1] && tempNode.Active)
										Path.intersect(tempNode.Pos, tempNode.Radius, -1);*/

									tempNode.Active = (bool)evnt.Arguments[1];
								}
								break;

							// remove node
							case WorldEvent.EventType.RemNode:
								tempNode = NodeByID[(string)evnt.Arguments[0]];

								// prepare to invalidate fog of war
								VectorF upperLeft = VectorF.Zero;
								VectorF lowerRight = VectorF.Zero;

								if (tempNode.Active && tempNode.Owner == Owner)
								{
									upperLeft = new VectorF(tempNode.X - tempNode.SightDistance, tempNode.Y - tempNode.SightDistance);
									lowerRight = new VectorF(tempNode.X + tempNode.SightDistance, tempNode.Y + tempNode.SightDistance);
								}

								if (tempNode.Owner == Owner)
									Nodes.Remove((Node)tempNode);
								NodeByID.Remove(tempNode.ID);
								Grid.Point(tempNode.Pos, tempNode, gridRemoveNode);

								//if (tempNode.Owner == Owner || tempNode.Active)
								//	Path.intersect(tempNode.Pos, tempNode.Radius, -1);

								// invalidate fog of war
								if (upperLeft != lowerRight)
								{
									lock (Owner.Fog) { Owner.Fog.invalidate(upperLeft, lowerRight); }
									refreshVisibility(upperLeft, lowerRight);
								}
								break;
							default:
								throw new Exception("Unrecognized event: " + evnt.ToString());
						}
					}
					evnts.Clear();

					// link new nodes to their connected segments and parents
					foreach (Node newNode in newNodes)
					{
						// segments
						for (int i = 0; i < newNode.Segments.Length; i++)
						{
							if (newNode.Segments[i] != null)
							{
								SegmentSkel reassignSeg = null;
								if (SegByID.TryGetValue(newNode.Segments[i].ID, out reassignSeg))
									newNode.Segments[i] = (Segment)reassignSeg;
#if DEBUG
								else
									throw new Exception("Segment id " + newNode.Segments[i].ID + " not found.");
#endif
							}
						}

						// parents
						for (int i = 0; i < newNode.Parents.Count; i++)
						{
							NodeSkel reassignNode = null;
							if (NodeByID.TryGetValue(newNode.Parents[i].ID, out reassignNode))
								newNode.Parents[i] = (Node)reassignNode;
						}
					}

					// link new segments to their connected nodes
					foreach (Segment newSeg in newSegs)
					{
						for (int i = 0; i < 2; i++)
						{
							NodeSkel reassignNode = null;
							if (NodeByID.TryGetValue(newSeg.Nodes[i].ID, out reassignNode))
								newSeg.Nodes[i] = (Node)reassignNode;
							else
								newSeg.Nodes[i] = new Node(newSeg.Nodes[i]);
						}
					}

					// link updated nodes to their connected segments
					for (int i = 0; i < updateNodes.Count; i++)
					{
						Node node = updateNodes[i];
						List<Segment> segs = updateNodeSegs[i];

						updateNodes[i].NumSegments = 0;

						for (int j = 0; j < segs.Count; j++)
						{
							if (segs[j] != null)
								updateNodes[i].NumSegments++;

							if (segs[j] == null && node.Segments[j] != null) // if removing a segment
							{
								node.Segments[j] = null;
							}
							else if (segs[j] != null && node.Segments[j] == null) // if adding a segment
							{
								node.Segments[j] = (Segment)SegByID[segs[j].ID];
							}
							else if (segs[j] != null && node.Segments[j] != null && segs[j].ID != node.Segments[j].ID) // if switching out a segment with another
							{
								node.Segments[j] = (Segment)SegByID[segs[j].ID];
							}
						}
					}
				}
				#endregion Sync

				#region Update Destination

				if (destUpdateDue)
				{
					// TODO: make this work


					destUpdateDue = false;
				}

				#endregion Update Destination

				#region Decision-Making

				/*{ // Pattern AI
				Node top = null;
				Node bottom = null;
				Node right = null;
				Node left = null;

				foreach (Node n in Nodes)
				{
					if (top == null || n.Pos.Y < top.Y)
						top = n;

					if (bottom == null || n.Pos.Y > bottom.Y)
						bottom = n;

					if (right == null || n.Pos.X > right.X)
						right = n;

					if (left == null || n.Pos.X < left.X)
						left = n;
				}

				foreach (Node fromNode in Nodes)
				{
					if (!fromNode.Active || fromNode.NumSegments > 1)
						continue;

					VectorF dest = VectorF.Zero;

					if (fromNode.IsParent)
					{
						FInt x = (fromNode.Pos.X + fromNode.Segments[0].getOppNode(fromNode).Pos.X) / FInt.F2;
						if (left == fromNode)
							dest = new VectorF(x, fromNode.Pos.Y + (FInt)200);
						else
							dest = new VectorF(x, fromNode.Pos.Y - (FInt)200);
					}
					else
					{
						if (bottom == fromNode)
							dest = new VectorF(right.Pos.X + (FInt)200, right.Pos.Y);
						else if (top == fromNode)
							dest = new VectorF(left.Pos.X - (FInt)200, left.Pos.Y);
						else if (right == fromNode)
							dest = new VectorF(top.Pos.X, top.Pos.Y - (FInt)200);
						else // left
							dest = new VectorF(bottom.Pos.X, bottom.Pos.Y + (FInt)200);
					}

					Actions.Add(new PlayerAction(Owner, PlayerAction.ActionType.BuildSeg, fromNode.ID, false, dest));
				}

				if (Actions.Count == 0)
					for (int i = 0; i < 9999999; i++) { } // give the main loop a break
			}*/

				#endregion Decision-Making

				#region Decision-Making
				/*{ // Real thinking AI
					Node bestNode = null;
					PathNode bestPathNode = null;
					FInt bestWorth = FInt.F0;
					Node worstNode = null;
					PathNode worstPathNode = null;
					FInt worstWorth = FInt.F0;
					bool stillBuilding = false;
					FInt maxDist = (FInt)400;
					foreach (Node fromNode in Nodes)
					{
						if (!fromNode.Active)
							stillBuilding = true;

						if (!fromNode.Active || fromNode.NumSegments == fromNode.Segments.Length)
							continue;

						VectorF fromPos = fromNode.Pos;

						VectorF nSpacing = (VectorF)Path.NodeSpacing;
						VectorF nde = new VectorF(fromPos.X - maxDist, fromPos.Y - maxDist) / nSpacing;
						nde.X = (FInt)Math.Round((double)nde.X);
						nde.Y = (FInt)Math.Round((double)nde.Y);
						int left = Math.Max(0, (int)nde.X);
						int top = Math.Max(0, (int)nde.Y);
						nde = new VectorF(fromPos.X + maxDist, fromPos.Y + maxDist) / nSpacing;
						nde.X = (FInt)Math.Round((double)nde.X);
						nde.Y = (FInt)Math.Round((double)nde.Y);
						int right = Math.Min(Path.NumCols - 1, (int)nde.X);
						int bottom = Math.Min(Path.NumRows - 1, (int)nde.Y);
						FInt[,] worth = new FInt[bottom - top + 1, right - left + 1];

						nde = (Grid.Squares[0, 0].Nodes[0].Pos + Grid.Squares[0, 0].Nodes[1].Pos) / FInt.F2 / nSpacing;
						nde.X = (FInt)Math.Round((double)nde.X);
						nde.Y = (FInt)Math.Round((double)nde.Y);
						VectorF toPos = (VectorF)Path.Grid[(int)nde.Y, (int)nde.X].Position;
						FInt totDist = VectorF.Distance(fromPos, toPos);

						for (int row = top; row <= bottom; row++)
						{
							for (int col = left; col <= right; col++)
							{
								PathNode pn = Path.Grid[row, col];
								VectorF pnPos = (VectorF)pn.Position;

								// distance from source node
								FInt srcDist = VectorF.Distance(fromPos, pnPos);
								FInt srcDistWorth = FInt.F0;

								if (srcDist > maxDist || srcDist < (FInt)100)
									srcDistWorth = FInt.F1 / FInt.F2;
								else
									srcDistWorth = FInt.F1;

								// distance from destination node
								FInt destDist = VectorF.Distance(toPos, pnPos);
								FInt destDistWorth = FInt.F1 - (destDist / (maxDist * FInt.F2));

								// collisions
								List<NodeSkel> ignoreNode = new List<NodeSkel>(1) { fromNode };
								List<SegmentSkel> ignoreSeg = new List<SegmentSkel>(fromNode.NumSegments);
								foreach (Segment seg in fromNode.Segments)
								{
									if (seg != null)
										ignoreSeg.Add(seg);
								}

								FInt collWorth = FInt.F0;
								List<SegmentSkel> collSeg;
								List<NodeSkel> collNode;
								List<GeoSkel> collGeo;
								if (Collision.segCollision(fromPos, pnPos, ignoreSeg, ignoreNode, out collSeg, out collNode, out collGeo, true, Owner))
								{
									if (collGeo.Count > 0)
									{
										collWorth = FInt.F0;
									}
									else if (collNode.Count > 0)
									{
										collWorth = FInt.F0;
									}
									else
									{
										SegmentSkel closestSeg = null;
										VectorF closestInt = VectorF.Zero;
										FInt closestDist = FInt.F0;

										for (int i = 0; i < collSeg.Count; i++)
										{
											VectorF nextInt = Calc.LineIntersect(fromPos, pnPos, collSeg[i].EndLoc[0], collSeg[i].EndLoc[1]);

											if (collSeg[i].Owner == Owner)
											{
												VectorF end0 = (collSeg[i].State[1] == SegmentSkel.SegState.Retracting)
													? collSeg[i].EndLoc[0]
													: ((Segment)collSeg[i]).Nodes[0].Pos;

												VectorF end1 = (collSeg[i].State[0] == SegmentSkel.SegState.Retracting)
													? collSeg[i].EndLoc[1]
													: ((Segment)collSeg[i]).Nodes[1].Pos;

												nextInt = Calc.LineIntersect(fromPos, pnPos, end0, end1);
											}
											else
											{
												nextInt = Calc.LineIntersect(fromPos, pnPos, collSeg[i].EndLoc[0], collSeg[i].EndLoc[1]);
											}

											FInt nextDist = VectorF.Distance(fromPos, nextInt);

											if (closestSeg == null || nextDist < closestDist)
											{
												closestSeg = collSeg[i];
												closestInt = nextInt;
												closestDist = nextDist;
											}
										}

										if (closestSeg.Owner == Owner)
											collWorth = FInt.F0;
										else
											collWorth = FInt.F1;
									}
								}
								else
								{
									NodeType nType = Owner.InWorld.getNodeType(srcDist);

									if (pnPos.X - nType.Radius < 0
										|| pnPos.Y - nType.Radius < 0
										|| pnPos.X + nType.Radius >= Owner.InWorld.Width
										|| pnPos.Y + nType.Radius >= Owner.InWorld.Height
										|| Collision.nodeCollision(pnPos, nType.Radius, new List<SegmentSkel>(0), new List<NodeSkel>(0), true, Owner)
										|| Collision.nodeCollNodeSpacing(pnPos, nType.Radius, nType.Spacing, Owner, new List<NodeSkel>(0), true))
									{
										collWorth = FInt.F0;
									}
									else
										collWorth = FInt.F1 / FInt.F2;
								}

								// calculate worth
								worth[row - top, col - left] = (collWorth * FInt.F5) + (srcDistWorth * FInt.F2) + destDistWorth;

								if (worth[row - top, col - left] > bestWorth || (worth[row - top, col - left] == bestWorth && new Random().Next(2) == 1))
								{
									bestNode = fromNode;
									bestPathNode = pn;
									bestWorth = worth[row - top, col - left];
								}
								else if (fromNode.NumSegments == 1 && worth[row - top, col - left] < worstWorth || (worth[row - top, col - left] == worstWorth && new Random().Next(2) == 1))
								{
									worstNode = fromNode;
									worstPathNode = pn;
									worstWorth = worth[row - top, col - left];
								}
							}
						}
					}

					PlayerAction action = null;

					if (bestPathNode != null && bestNode != null && (!stillBuilding || bestWorth > (FInt)3.5)) // if worth isn't high enough, don't do anything
						action = new PlayerAction(Owner, PlayerAction.ActionType.BuildSeg, bestNode.ID, false, (VectorF)bestPathNode.Position);
					else if (worstPathNode != null && worstNode != null)
						action = new PlayerAction(Owner, PlayerAction.ActionType.DestroyNode, worstNode.ID);

					if (action != null)
					{
						lock (Actions)
							Actions.Add(action);
					}
					
				}*/
				#endregion Decision-Making

				#region Decision-Making
				if (false)
				{
					VectorF nSpacing = (VectorF)Path.NodeSpacing;

					VectorF dest = (Grid.Squares[0, 0].Nodes[0].Pos + Grid.Squares[0, 0].Nodes[1].Pos) / FInt.F2 / nSpacing;
					PathNode destNode = Path.Grid[(int)dest.Y, (int)dest.X];
					dest = (VectorF)destNode.Position;
					Node uselessNode = null;
					List<NodeDistPath> openNodes = new List<NodeDistPath>();

					// collect list of nodes with an open branch
					foreach (Node n in Nodes)
					{
						if (n.Active && n.NumSegments < n.Segments.Length)
							openNodes.Add(new NodeDistPath(n, VectorF.Distance(n.Pos, dest), null));
					}


					//
					// FIND CLOSEST NODE TO DESTINATION
					//

					// sort nodes by distance from destination
					NodeDistPath swap;
					for (int i = 1, j; i < openNodes.Count; i++)
					{
						for (j = i; j > 0 && openNodes[j] < openNodes[j - 1]; j--)
						{
							swap = openNodes[j];
							openNodes[j] = openNodes[j - 1];
							openNodes[j - 1] = swap;
						}
					}

					// find the node with the shortest path to the destination
					while (openNodes.Count > 0 && openNodes[0].Path == null)
					{
						// get a starting path node
						PathNode srcNode = Path.Grid[(int)((double)openNodes[0].Node.Pos.Y / Path.NodeSpacing.Y), (int)((double)openNodes[0].Node.Pos.X / Path.NodeSpacing.X)];
						while (srcNode.isOrphaned())
						{
							srcNode = Path.Nodes[srcNode.Index - Path.NumCols];
						}

						// get shortest path
						openNodes[0].Path = Path.gridToList(Path.search(srcNode, destNode), srcNode, destNode);

						// if path doesn't exist
						if (openNodes[0].Path == null)
						{
							openNodes.RemoveAt(0);
							continue;
						}

						double dist = 0d;
						foreach (PathEdge edge in openNodes[0].Path)
						{
							dist += edge.Distance;
						}
						openNodes[0].Dist = (FInt)dist;

						// update order in list
						for (int i = 0; i < openNodes.Count - 1 && openNodes[i] > openNodes[i + 1]; i++)
						{
							swap = openNodes[i];
							openNodes[i] = openNodes[i + 1];
							openNodes[i + 1] = swap;
						}
					}

					if (openNodes.Count > 0)
					{

						// find furthest reachable path node
						FInt maxDist = (FInt)400; // TODO: get rid of magic number (400)
						int magnetEdge = -1;
						for (int i = openNodes[0].Path.Count - 1; i >= 0; i--)
						{
							if (VectorF.Distance(openNodes[0].Node.Pos, (VectorF)Path.Nodes[openNodes[0].Path[i].NodeDest].Position) < maxDist)
							{
								magnetEdge = i;
								break;
							}
						}

						// find furthest node with no collisions
						List<NodeSkel> ignoreNode = new List<NodeSkel>(1) { openNodes[0].Node };
						List<SegmentSkel> ignoreSeg = new List<SegmentSkel>(openNodes[0].Node.NumSegments);
						foreach (Segment seg in openNodes[0].Node.Segments)
						{
							if (seg != null)
								ignoreSeg.Add(seg);
						}
						List<SegmentSkel> collSeg;
						List<NodeSkel> collNode;
						List<GeoSkel> collGeo;
						VectorF magnetPoint = VectorF.Zero;

						for (int i = magnetEdge; i >= 0; i--)
						{
							bool collision = false;
							if (Collision.segCollision(openNodes[0].Node.Pos, (VectorF)Path.Nodes[openNodes[0].Path[i].NodeDest].Position, ignoreSeg, ignoreNode, out collSeg, out collNode, out collGeo, true, Owner))
							{
								if (collNode.Count > 0 || collGeo.Count > 0)
								{
									collision = true;
								}
								else
								{
									foreach (SegmentSkel s in collSeg)
									{
										if (s.Owner == Owner)
										{
											collision = true;
											break;
										}
									}
								}
							}

							if (!collision)
							{
								magnetPoint = (VectorF)Path.Nodes[openNodes[0].Path[i].NodeDest].Position;
								break;
							}
						}

						// find best path node towards destination

						VectorF nde = new VectorF(openNodes[0].Node.X - maxDist, openNodes[0].Node.Y - maxDist) / nSpacing;
						nde.X = (FInt)Math.Round((double)nde.X);
						nde.Y = (FInt)Math.Round((double)nde.Y);
						int left = Math.Max(0, (int)nde.X);
						int top = Math.Max(0, (int)nde.Y);
						nde = new VectorF(openNodes[0].Node.X + maxDist, openNodes[0].Node.Y + maxDist) / nSpacing;
						nde.X = (FInt)Math.Round((double)nde.X);
						nde.Y = (FInt)Math.Round((double)nde.Y);
						int right = Math.Min(Path.NumCols - 1, (int)nde.X);
						int bottom = Math.Min(Path.NumRows - 1, (int)nde.Y);

						Node bestNode = null;
						PathNode bestPNode = null;
						FInt bestWorth = FInt.F0;
						for (int row = top; row <= bottom; row++)
						{
							for (int col = left; col <= right; col++)
							{
								PathNode pn = Path.Grid[row, col];
								VectorF pnPos = (VectorF)pn.Position;

								FInt collWorth = FInt.F0;

								if (Collision.segCollision(openNodes[0].Node.Pos, pnPos, ignoreSeg, ignoreNode, out collSeg, out collNode, out collGeo, true, Owner))
								{
									if (collGeo.Count > 0 || collNode.Count > 0)
									{
										collWorth = FInt.FN1;
									}
									else
									{
										SegmentSkel closestSeg = null;
										VectorF closestInt = VectorF.Zero;
										FInt closestDist = FInt.F0;

										for (int i = 0; i < collSeg.Count; i++)
										{
											VectorF nextInt = Calc.LineIntersect(openNodes[0].Node.Pos, pnPos, collSeg[i].EndLoc[0], collSeg[i].EndLoc[1]);

											if (collSeg[i].Owner == Owner)
											{
												VectorF end0 = (collSeg[i].State[1] == SegmentSkel.SegState.Retracting)
													? collSeg[i].EndLoc[0]
													: ((Segment)collSeg[i]).Nodes[0].Pos;

												VectorF end1 = (collSeg[i].State[0] == SegmentSkel.SegState.Retracting)
													? collSeg[i].EndLoc[1]
													: ((Segment)collSeg[i]).Nodes[1].Pos;

												nextInt = Calc.LineIntersect(openNodes[0].Node.Pos, pnPos, end0, end1);
											}
											else
											{
												nextInt = Calc.LineIntersect(openNodes[0].Node.Pos, pnPos, collSeg[i].EndLoc[0], collSeg[i].EndLoc[1]);
											}

											FInt nextDist = VectorF.Distance(openNodes[0].Node.Pos, nextInt);

											if (closestSeg == null || nextDist < closestDist)
											{
												closestSeg = collSeg[i];
												closestInt = nextInt;
												closestDist = nextDist;
											}
										}

										if (closestSeg.Owner == Owner)
											collWorth = FInt.FN1;
										else
											collWorth = FInt.F1;
									}
								}
								else
								{
									NodeType nType = Owner.InWorld.getNodeType(VectorF.Distance(openNodes[0].Node.Pos, pnPos));

									if (pnPos.X - nType.Radius < 0
										|| pnPos.Y - nType.Radius < 0
										|| pnPos.X + nType.Radius >= Owner.InWorld.Width
										|| pnPos.Y + nType.Radius >= Owner.InWorld.Height
										|| Collision.nodeCollision(pnPos, nType.Radius, new List<SegmentSkel>(0), new List<NodeSkel>(0), true, Owner)
										|| Collision.nodeCollNodeSpacing(pnPos, nType.Radius, nType.Spacing, Owner, new List<NodeSkel>(0), true))
									{
										collWorth = FInt.FN1;
									}
								}

								if (collWorth != FInt.FN1)
								{
									// distance from destination node
									FInt destDist = VectorF.Distance(magnetPoint, pnPos);
									FInt worth = maxDist - destDist + collWorth;

									if (bestPNode == null || worth > bestWorth || (worth == bestWorth && new Random().Next(2) == 1))
									{
										bestPNode = pn;
										bestWorth = worth;
									}
								}
							}
						}

						if (bestPNode != null)
							bestNode = openNodes[0].Node;

						//
						// FIND MOST USELESS NODE
						//

						for (int i = openNodes.Count - 1; i >= 0; i--)
						{
							if (!openNodes[i].Node.IsParent && openNodes[i].Node.NumSegments == 1)
							{
								uselessNode = openNodes[i].Node;
								break;
							}
						}

						//
						// FIND INTERSECTIONS
						//

						foreach (NodeDistPath fromNode in openNodes)
						{
							VectorF fromPos = fromNode.Node.Pos;

							nde = new VectorF(fromPos.X - maxDist, fromPos.Y - maxDist) / nSpacing;
							nde.X = (FInt)Math.Round((double)nde.X);
							nde.Y = (FInt)Math.Round((double)nde.Y);
							left = Math.Max(0, (int)nde.X);
							top = Math.Max(0, (int)nde.Y);
							nde = new VectorF(fromPos.X + maxDist, fromPos.Y + maxDist) / nSpacing;
							nde.X = (FInt)Math.Round((double)nde.X);
							nde.Y = (FInt)Math.Round((double)nde.Y);
							right = Math.Min(Path.NumCols - 1, (int)nde.X);
							bottom = Math.Min(Path.NumRows - 1, (int)nde.Y);

							nde = (Grid.Squares[0, 0].Nodes[0].Pos + Grid.Squares[0, 0].Nodes[1].Pos) / FInt.F2 / nSpacing;
							nde.X = (FInt)Math.Round((double)nde.X);
							nde.Y = (FInt)Math.Round((double)nde.Y);

							for (int row = top; row <= bottom; row++)
							{
								for (int col = left; col <= right; col++)
								{
									PathNode pn = Path.Grid[row, col];
									VectorF pnPos = (VectorF)pn.Position;

									// distance from source node
									FInt srcDist = VectorF.Distance(fromPos, pnPos);

									// collisions
									ignoreNode = new List<NodeSkel>(1) { fromNode.Node };
									ignoreSeg = new List<SegmentSkel>(fromNode.Node.NumSegments);
									foreach (Segment seg in fromNode.Node.Segments)
									{
										if (seg != null)
											ignoreSeg.Add(seg);
									}

									FInt collWorth = FInt.F0;
									if (Collision.segCollision(fromPos, pnPos, ignoreSeg, ignoreNode, out collSeg, out collNode, out collGeo, true, Owner) && collGeo.Count == 0 && collNode.Count == 0)
									{
										SegmentSkel closestSeg = null;
										VectorF closestInt = VectorF.Zero;
										FInt closestDist = FInt.F0;

										for (int i = 0; i < collSeg.Count; i++)
										{
											VectorF nextInt = Calc.LineIntersect(fromPos, pnPos, collSeg[i].EndLoc[0], collSeg[i].EndLoc[1]);

											if (collSeg[i].Owner == Owner)
											{
												VectorF end0 = (collSeg[i].State[1] == SegmentSkel.SegState.Retracting)
													? collSeg[i].EndLoc[0]
													: ((Segment)collSeg[i]).Nodes[0].Pos;

												VectorF end1 = (collSeg[i].State[0] == SegmentSkel.SegState.Retracting)
													? collSeg[i].EndLoc[1]
													: ((Segment)collSeg[i]).Nodes[1].Pos;

												nextInt = Calc.LineIntersect(fromPos, pnPos, end0, end1);
											}
											else
											{
												nextInt = Calc.LineIntersect(fromPos, pnPos, collSeg[i].EndLoc[0], collSeg[i].EndLoc[1]);
											}

											FInt nextDist = VectorF.Distance(fromPos, nextInt);

											if (closestSeg == null || nextDist < closestDist)
											{
												closestSeg = collSeg[i];
												closestInt = nextInt;
												closestDist = nextDist;
											}
										}

										if (closestSeg.Owner == Owner)
											collWorth = FInt.F0;
										else
											collWorth = FInt.F1;

										// calculate worth

										FInt worth = collWorth * (maxDist - closestDist) * 2;

										if (bestPNode == null || worth > bestWorth || (worth == bestWorth && new Random().Next(2) == 1))
										{
											bestNode = fromNode.Node;
											bestPNode = pn;
											bestWorth = worth;
										}
									}
								}
							}
						}

						//
						// MAKE DECISION
						//

						PlayerAction action = null;

						if (bestPNode != null && bestNode != null && (bestWorth > (FInt)3.5)) // if worth isn't high enough, don't do anything
							action = new PlayerAction(Owner, PlayerAction.ActionType.BuildSeg, bestNode.ID, false, (VectorF)bestPNode.Position);
						else if (uselessNode != null)
							action = new PlayerAction(Owner, PlayerAction.ActionType.DestroyNode, uselessNode.ID);

						if (action != null)
						{
							lock (Actions)
								Actions.Add(action);
						}
					}
				}

				#endregion Decision-Making

				#region Decision-Making

				if (false)
				{	
					// Real thinking AI
					Node bestNode = null;
					PathNode bestPathNode = null;
					FInt bestWorth = FInt.F0;
					Node worstNode = null;
					PathNode worstPathNode = null;
					FInt worstWorth = FInt.F0;
					bool stillBuilding = false;
					FInt maxDist = (FInt)400;
					foreach (Node fromNode in Nodes)
					{
						if (!fromNode.Active)
							stillBuilding = true;

						if (!fromNode.Active || fromNode.NumSegments == fromNode.Segments.Length)
							continue;

						VectorF fromPos = fromNode.Pos;

						VectorF nSpacing = (VectorF)Path.NodeSpacing;
						VectorF nde = new VectorF(fromPos.X - maxDist, fromPos.Y - maxDist) / nSpacing;
						nde.X = (FInt)Math.Round((double)nde.X);
						nde.Y = (FInt)Math.Round((double)nde.Y);
						int left = Math.Max(0, (int)nde.X);
						int top = Math.Max(0, (int)nde.Y);
						nde = new VectorF(fromPos.X + maxDist, fromPos.Y + maxDist) / nSpacing;
						nde.X = (FInt)Math.Round((double)nde.X);
						nde.Y = (FInt)Math.Round((double)nde.Y);
						int right = Math.Min(Path.NumCols - 1, (int)nde.X);
						int bottom = Math.Min(Path.NumRows - 1, (int)nde.Y);
						FInt[,] worth = new FInt[bottom - top + 1, right - left + 1];

						nde = (Grid.Squares[0, 0].Nodes[0].Pos + Grid.Squares[0, 0].Nodes[1].Pos) / FInt.F2 / nSpacing;
						nde.X = (FInt)Math.Round((double)nde.X);
						nde.Y = (FInt)Math.Round((double)nde.Y);
						VectorF toPos = (VectorF)Path.Grid[(int)nde.Y, (int)nde.X].Position;
						FInt totDist = VectorF.Distance(fromPos, toPos);

						for (int row = top; row <= bottom; row++)
						{
							for (int col = left; col <= right; col++)
							{
								PathNode pn = Path.Grid[row, col];
								VectorF pnPos = (VectorF)pn.Position;

								// distance from source node
								FInt srcDist = VectorF.Distance(fromPos, pnPos);
								FInt srcDistWorth = FInt.F0;

								if (srcDist > maxDist || srcDist < (FInt)100)
									srcDistWorth = FInt.F1 / FInt.F2;
								else
									srcDistWorth = FInt.F1;

								// distance from destination node
								FInt destDist = VectorF.Distance(toPos, pnPos);
								FInt destDistWorth = FInt.F1 - (destDist / (maxDist * FInt.F2));

								// collisions
								List<NodeSkel> ignoreNode = new List<NodeSkel>(1) { fromNode };
								List<SegmentSkel> ignoreSeg = new List<SegmentSkel>(fromNode.NumSegments);
								foreach (Segment seg in fromNode.Segments)
								{
									if (seg != null)
										ignoreSeg.Add(seg);
								}

								FInt collWorth = FInt.F0;
								List<SegmentSkel> collSeg;
								List<NodeSkel> collNode;
								List<GeoSkel> collGeo;
								if (Collision.segCollision(fromPos, pnPos, ignoreSeg, ignoreNode, out collSeg, out collNode, out collGeo, true, Owner))
								{
									if (collGeo.Count > 0 || collNode.Count > 0)
									{
										collWorth = FInt.F0;
									}
									else
									{
										SegmentSkel closestSeg = null;
										VectorF closestInt = VectorF.Zero;
										FInt closestDist = FInt.F0;

										for (int i = 0; i < collSeg.Count; i++)
										{
											VectorF nextInt = Calc.LineIntersect(fromPos, pnPos, collSeg[i].EndLoc[0], collSeg[i].EndLoc[1]);

											if (collSeg[i].Owner == Owner)
											{
												VectorF end0 = (collSeg[i].State[1] == SegmentSkel.SegState.Retracting)
													? collSeg[i].EndLoc[0]
													: ((Segment)collSeg[i]).Nodes[0].Pos;

												VectorF end1 = (collSeg[i].State[0] == SegmentSkel.SegState.Retracting)
													? collSeg[i].EndLoc[1]
													: ((Segment)collSeg[i]).Nodes[1].Pos;

												nextInt = Calc.LineIntersect(fromPos, pnPos, end0, end1);
											}
											else
											{
												nextInt = Calc.LineIntersect(fromPos, pnPos, collSeg[i].EndLoc[0], collSeg[i].EndLoc[1]);
											}

											FInt nextDist = VectorF.Distance(fromPos, nextInt);

											if (closestSeg == null || nextDist < closestDist)
											{
												closestSeg = collSeg[i];
												closestInt = nextInt;
												closestDist = nextDist;
											}
										}

										if (closestSeg.Owner == Owner)
											collWorth = FInt.F0;
										else
											collWorth = FInt.F1;
									}
								}
								else
								{
									NodeType nType = Owner.InWorld.getNodeType(srcDist);

									if (pnPos.X - nType.Radius < 0
										|| pnPos.Y - nType.Radius < 0
										|| pnPos.X + nType.Radius >= Owner.InWorld.Width
										|| pnPos.Y + nType.Radius >= Owner.InWorld.Height
										|| Collision.nodeCollision(pnPos, nType.Radius, new List<SegmentSkel>(0), new List<NodeSkel>(0), true, Owner)
										|| Collision.nodeCollNodeSpacing(pnPos, nType.Radius, nType.Spacing, Owner, new List<NodeSkel>(0), true))
									{
										collWorth = FInt.F0;
									}
									else
										collWorth = FInt.F1 / FInt.F2;
								}

								// calculate worth
								worth[row - top, col - left] = (collWorth * FInt.F5) + (srcDistWorth * FInt.F2) + destDistWorth;

								if (worth[row - top, col - left] > bestWorth || (worth[row - top, col - left] == bestWorth && new Random().Next(2) == 1))
								{
									bestNode = fromNode;
									bestPathNode = pn;
									bestWorth = worth[row - top, col - left];
								}
								else if (fromNode.NumSegments == 1 && worth[row - top, col - left] < worstWorth || (worth[row - top, col - left] == worstWorth && new Random().Next(2) == 1))
								{
									worstNode = fromNode;
									worstPathNode = pn;
									worstWorth = worth[row - top, col - left];
								}
							}
						}
					}

					PlayerAction action = null;

					if (bestPathNode != null && bestNode != null && (!stillBuilding || bestWorth > (FInt)3.5)) // if worth isn't high enough, don't do anything
						action = new PlayerAction(Owner, PlayerAction.ActionType.BuildSeg, bestNode.ID, false, (VectorF)bestPathNode.Position);
					else if (worstPathNode != null && worstNode != null)
						action = new PlayerAction(Owner, PlayerAction.ActionType.DestroyNode, worstNode.ID);

					if (action != null)
					{
						lock (Actions)
							Actions.Add(action);
					}

				}
				#endregion Decision-Making
			}

			Waiting = false;
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
			foreach (KeyValuePair<string, NodeSkel> node in NodeByID)
			{
				node.Value.Visible = node.Value.Owner == Owner || (node.Value.Active && Owner.Fog.isVisible(node.Value));
			}

			foreach (KeyValuePair<string, SegmentSkel> seg in SegByID)
			{
				seg.Value.Visible = seg.Value.Owner == Owner || Owner.Fog.isVisible(seg.Value);
			}
		}

		/// <summary>Refreshes all path intersections.</summary>
		public void refreshPathIntersections()
		{
			Path.clearIntersections();

			// only the computer's segments
			foreach (Segment seg in Segments)
			{
				Path.intersect(seg.EndLoc[0], seg.EndLoc[1], 1);
			}

			// all nodes
			/*foreach (KeyValuePair<string, NodeSkel> node in NodeByID)
			{
				if (node.Value.Owner == Owner || node.Value.Active)
					Path.intersect(node.Value.Pos, node.Value.Radius, 1);
			}*/
		}

		public bool getNodeVisibility(string ID)
		{
			NodeSkel node = null;

			lock (NodeByID)
			{
				if (NodeByID.TryGetValue(ID, out node))
					return node.Visible;
			}

			return false;
		}

		public bool getSegVisibility(string ID)
		{
			SegmentSkel seg = null;

			lock (SegByID)
			{
				if (SegByID.TryGetValue(ID, out seg))
					return seg.Visible;
			}

			return false;
		}

		/// <summary>Adds the provided segment to the specified square.</summary>
		/// <param name="sqr">The grid square to add the segment to.</param>
		/// <param name="seg">The segment to add.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		public void gridAddSegment(GridSqr sqr, object seg, TimeSpan time, int opID)
		{
			((SegmentSkel)seg).LastCheck = time;
			((SegmentSkel)seg).LastCheckNum = opID;
			sqr.Segments.Add((SegmentSkel)seg);
		}

		/// <summary>Adds the provided node to the specified square.</summary>
		/// <param name="sqr">The grid square to add the node to.</param>
		/// <param name="node">The node to add.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridAddNode(GridSqr sqr, object node, TimeSpan time, int opID)
		{
			((NodeSkel)node).LastCheck = time;
			((NodeSkel)node).LastCheckNum = opID;
			sqr.Nodes.Add((NodeSkel)node);
		}

		/// <summary>Removes the specified segment from the specified square.</summary>
		/// <param name="sqr">The square to remove the segment from.</param>
		/// <param name="segment">The segment to remove.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridRemoveSegment(GridSqr sqr, object segment, TimeSpan time, int opID)
		{
			((SegmentSkel)segment).LastCheck = time;
			((SegmentSkel)segment).LastCheckNum = opID;
			sqr.Segments.Remove((SegmentSkel)segment);
		}

		/// <summary>Removes the provided node from the specified grid square.</summary>
		/// <param name="sqr">The grid square to remove the node from.</param>
		/// <param name="node">The node to remove from the grid square.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridRemoveNode(GridSqr sqr, object node, TimeSpan time, int opID)
		{
			((NodeSkel)node).LastCheck = time;
			((NodeSkel)node).LastCheckNum = opID;
			sqr.Nodes.Remove((NodeSkel)node);
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

				if (node.Owner == Owner
					|| node.X - node.Radius > bottomRight.X
					|| node.X + node.Radius < topLeft.X
					|| node.Y - node.Radius > bottomRight.Y
					|| node.Y + node.Radius < topLeft.Y)
					continue;

				node.Visible = Owner.Fog.isVisible(node);
			}

			foreach (SegmentSkel seg in sqr.Segments)
			{
				if (seg.LastCheck == time && seg.LastCheckNum == opID)
					continue;

				seg.LastCheck = time;
				seg.LastCheckNum = opID;

				if (seg.Owner == Owner
					|| Calc.Min(seg.EndLoc[0].X, seg.EndLoc[1].X) > bottomRight.X
					|| Calc.Max(seg.EndLoc[0].X, seg.EndLoc[1].X) < topLeft.X
					|| Calc.Min(seg.EndLoc[0].Y, seg.EndLoc[1].Y) > bottomRight.Y
					|| Calc.Max(seg.EndLoc[0].Y, seg.EndLoc[1].Y) < topLeft.Y)
					continue;

				seg.Visible = Owner.Fog.isVisible(seg);
			}
		}

		#endregion Methods
	}
}
