using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class Node : NodeSkel
	{
		#region Members

		/// <summary>The player that owns this node.</summary>
		public override Player Owner
		{
			get { return owner; }
			set
			{
				if (owner != value)
				{
					if (owner != null)
						owner.Nodes.Remove(this);

					owner = value;

					if (owner != null)
						owner.Nodes.Add(this);
				}
			}
		}

		/// <summary>The world that contains this node.</summary>
		public World InWorld { get; set; }

		/// <summary>This node's parents.</summary>
		public List<Node> Parents { get; private set; }

		/// <summary>Which segment each parent comes from.</summary>
		public List<int> ParentFromSeg { get; private set; }

		/// <summary>The segments attached to this node.</summary>
		public Segment[] Segments { get; private set; }

		/// <summary>The number of segments currently attached to this node.</summary>
		public int NumSegments { get; set; }

		/// <summary>The number of people in each segment and its descendants.</summary>
		public int[] SegNumPeople { get; private set; }

		/// <summary>The sum capacity of each segment and its descendants.</summary>
		public FInt[] SegCapacity { get; private set; }

		/// <summary>People that need to be divvied up into segments.</summary>
		public int PeopleToSort { get; set; }

		/// <summary>How much time is left before the next person generation.</summary>
		public FInt GenCountDown { get; set; }

		/// <summary>Whether or not this node has been destroyed.</summary>
		public bool Destroyed { get; set; }

		/// <summary>The hotspot that this node owns.</summary>
		public Hotspot OwnsHotspot { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of Node.</summary>
		/// <param name="inWorld">The world that contains this node.</param>
		public Node(World inWorld)
		{
			clear();
			InWorld = inWorld;
		}

		/// <summary>Creates a new instance of Node.</summary>
		/// <param name="inWorld">The world that contains this node.</param>
		/// <param name="nodeType">The type of node.</param>
		public Node(World inWorld, NodeType nodeType)
			: this(inWorld)
		{
			setNodeType(nodeType);
		}

		/// <summary>Clones the provided node.  All references to other nodes or segments are deep.</summary>
		/// <param name="toClone">The node to clone.</param>
		public Node(Node toClone)
			: this(toClone.InWorld)
		{
			this.IsParent = toClone.IsParent;
			this.Radius = toClone.Radius;
			this.Spacing = toClone.Spacing;
			this.GenSpacing = toClone.GenSpacing;
			this.SightDistance = toClone.SightDistance;
			this.ID = toClone.ID;
			this.NType = toClone.NType;
			this.Pos = toClone.Pos;
			this.owner = toClone.Owner;
			this.Parents = new List<Node>(toClone.Parents);
			this.ParentFromSeg = new List<int>(toClone.ParentFromSeg);
			this.Segments = (Segment[])toClone.Segments.Clone();
			this.NumSegments = toClone.NumSegments;
			this.SegNumPeople = (int[])toClone.SegNumPeople.Clone();
			this.SegCapacity = (FInt[])toClone.SegCapacity.Clone();
			this.PeopleToSort = toClone.PeopleToSort;
			this.GenCountDown = toClone.GenCountDown;
			this.Destroyed = toClone.Destroyed;
			this.Active = toClone.Active;
			this.OwnsHotspot = toClone.OwnsHotspot;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Changes the settings of this node to match the provided node type.</summary>
		/// <param name="type">The type of node.</param>
		public void setNodeType(NodeType type)
		{
			NType = type;
			IsParent = type.IsParent;
			initSegArrays(type.NumSegments);
			GenSpacing = type.GenSpacing;
			Radius = type.Radius;
			SightDistance = type.SightDistance;
			Spacing = type.Spacing;
		}

		/// <summary>Initializes the segment arrays with the specified number of elements.</summary>
		/// <param name="NumSegments">The number of segments that may be attached to this node.</param>
		public void initSegArrays(int NumSegments)
		{
			Segments = new Segment[NumSegments];
			SegNumPeople = new int[NumSegments];
			SegCapacity = new FInt[NumSegments];
		}

		/// <summary>Resets all members to their default values.</summary>
		public override void clear()
		{
			base.clear();

			InWorld = null;
			Parents = new List<Node>();
			ParentFromSeg = new List<int>();
			Segments = null;
			NumSegments = 0;
			SegNumPeople = null;
			SegCapacity = null;
			PeopleToSort = 0;
			GenCountDown = FInt.F0;
			Destroyed = false;
			OwnsHotspot = null;
			
		}

		/// <summary>Adds a person to the node from the specified segment.</summary>
		/// <param name="fromSeg">The segment that the person came from.</param>
		public void addPerson(Segment fromSeg)
		{
			PeopleToSort++;
			SegNumPeople[getSegIndex(fromSeg)]--;
		}

		/// <summary>Updates the node.</summary>
		public void update(FInt elapsed)
		{
			// person generation
			if (GenSpacing > 0)
			{
				GenCountDown -= elapsed;

				if (GenCountDown <= 0)
				{
					GenCountDown += GenSpacing;
					genPerson();
				}
			}

			// sort people
			if (PeopleToSort > 0)
			{
				FInt[] density = new FInt[Segments.Length];

				for (int i = 0; i < density.Length; i++)
				{
					density[i] = (Segments[i] == null || SegCapacity[i] == FInt.F0) ? FInt.MaxValue : ((FInt)SegNumPeople[i] / SegCapacity[i]);
				}

				for (; PeopleToSort > 0; PeopleToSort--)
				{
					bool allFull = true;

					for (int j = 0; j < Segments.Length && allFull; j++)
					{
						if (Segments[j] != null && SegCapacity[j] > FInt.F0 && Segments[j].People[Segments[j].getNodeIndex(this)].Count < Segments[j].CurLaneCapacity)
							allFull = false;
					}

					int selSeg = -1;
					FInt selDensity = FInt.F0;

					for (int j = 0; j < Segments.Length; j++)
					{
						// if they're all full send it in the branch with the least
						// total density; otherwise, block segments that are full
						if (Segments[j] != null // if the segment exists
							&& (selSeg == -1 || density[j] < selDensity) && // if lower density
							(allFull || Segments[j].CurLength < InWorld.PersonSpacing // if new segment to build
							|| Segments[j].People[Segments[j].getNodeIndex(this)].Count < Segments[j].CurLaneCapacity)) // if outgoing lane isn't full
						{
							selSeg = j;
							selDensity = density[j];
						}
					}

					Segments[selSeg].addPerson(this);
					SegNumPeople[selSeg]++;
					density[selSeg] = SegCapacity[selSeg] == FInt.F0 ? FInt.MaxValue : (FInt)SegNumPeople[selSeg] / SegCapacity[selSeg];
				}
			}
		}

		/// <summary>Gets the index of the specified segment within this node.</summary>
		/// <param name="segment">The segment to search for.</param>
		/// <returns>The index of the specified segment within this node.</returns>
		public int getSegIndex(Segment segment)
		{
			for (int i = 0; i < Segments.Length; i++)
			{
				if (Segments[i] == segment)
					return i;
			}

			return -1;
		}

		/// <summary>Attempt to claim this node for the specified segment.</summary>
		/// <param name="segment">The segment attempting to claim the node.</param>
		/// <returns>Whether or not the claim was successful.</returns>
		public bool claim(Segment segment)
		{
			bool claimed = false;
			int relSegIndex = -1;
			int lane = segment.getNodeIndex(this);
			int oppLane = 1 - lane;
			Node oppNode = segment.Nodes[oppLane];

			// Determine whether or not the claim is successful
			if (Destroyed || Owner != segment.Owner)
			{
				// can't claim destroyed nodes or nodes owned by someone else
			}
			else if (!Active) // if the node is inactive...
			{
				if (InWorld.Collision.nodeCollision(Pos, Radius, new List<SegmentSkel>(1) { segment }, new List<NodeSkel>(1) { this })
					|| InWorld.Collision.nodeCollNodeSpacing(Pos, Radius, Spacing, Owner, new List<NodeSkel>(1) { this }, false))
					destroy();
				else
					claimed = true;
			}
			else if ((relSegIndex = relatedSeg(oppNode)) != -1) // if the node is related
			{
				claimed = true;

				// make sure the connection is more than one segment away
				foreach (Segment seg in Segments)
				{
					if (seg != null && seg.getOppNode(this) == oppNode)
					{
						claimed = false;
						break;
					}
				}

				if (claimed)
					severSegment(relSegIndex);
			}
			else if (NumSegments < Segments.Length) // if same owner, but unrelated
			{
				claimed = true;
			}

			if (!claimed)
			{
				segment.State[oppLane] = Segment.SegState.Retracting;
			}
			else
			{
				// complete the segment
				segment.State[oppLane] = Segment.SegState.Complete;

				// prepare to invalidate visibility for old owner
				// and apply new visibility to new owner
				Player invalOwner = (Active && Owner != segment.Owner && Owner.Type == Player.PlayerType.Human) ? Owner : null;
				bool applyVisibility = (!Active || Owner != segment.Owner) && segment.Owner.Type == Player.PlayerType.Human;

				Active = true;
				addSegment(segment, true);
				Owner = segment.Owner;

				// exchange parentage and density
				if (segment.State[lane] == Segment.SegState.Complete)
				{
					// exchange density
					int peopleToSend = 0;
					FInt capacityToSend = FInt.F0;
					for (int i = 0; i < Segments.Length; i++)
					{
						if (Segments[i] != null)
						{
							peopleToSend += SegNumPeople[i];
							capacityToSend += SegCapacity[i];
						}
					}

					int peopleToReceive = oppNode.PeopleToSort;
					FInt capacityToReceive = FInt.F0;
					for (int i = 0; i < oppNode.Segments.Length; i++)
					{
						if (oppNode.Segments[i] != null)
						{
							peopleToReceive += oppNode.SegNumPeople[i];
							capacityToReceive += oppNode.SegCapacity[i];
						}
					}

					alterDensity(peopleToReceive, capacityToReceive, segment);
					oppNode.alterDensity(peopleToSend, capacityToSend, segment);

					// exchange parentage
					Node[] parentsToSend = null;

					if (Parents.Count > 0 || IsParent)
					{
						parentsToSend = new Node[Parents.Count + (IsParent ? 1 : 0)];
						Parents.CopyTo(parentsToSend);
						if (IsParent)
							parentsToSend[parentsToSend.Length - 1] = this;
					}

					if (oppNode.Parents.Count > 0 || oppNode.IsParent)
					{
						if (!oppNode.IsParent)
						{
							addParents(segment, oppNode.Parents);
						}
						else
						{
							List<Node> parentsToReceive = new List<Node>(oppNode.Parents.Count + 1);
							parentsToReceive.AddRange(oppNode.Parents);
							parentsToReceive.Add(oppNode);
							addParents(segment, parentsToReceive);
						}
					}

					if (parentsToSend != null)
						oppNode.addParents(segment, parentsToSend);
				}
				else // get segment's density
				{
					int peopleToReceive = segment.NumPeople;
					FInt capacityToReceive = (segment.State[lane] != Segment.SegState.Retracting) ? segment.Capacity : FInt.F0;

					alterDensity(peopleToReceive, capacityToReceive, segment);
				}

				// update visibility
				if (invalOwner != null)
				{
					invalOwner.Fog.invalidate(new VectorF(X - SightDistance, Y - SightDistance), new VectorF(X + SightDistance, Y + SightDistance));
					InWorld.refreshVisibility(new VectorF(X - SightDistance, Y - SightDistance), new VectorF(X + Radius, Y + Radius));
				}

				if (applyVisibility)
				{
					Owner.Fog.applyVisibility(this);
					InWorld.refreshVisibility(new VectorF(X - SightDistance, Y - SightDistance), new VectorF(X + SightDistance, Y + SightDistance));
				}

				Visible = Owner.Type == Player.PlayerType.Human || (Active && InWorld.HumanPlayer.Fog.isVisible(this));

				// add event
				InWorld.addEvent(ID, WorldEvent.EventType.NodeChangeState);

				// run script if on hotspot
				if (OwnsHotspot != null && !string.IsNullOrWhiteSpace(OwnsHotspot.Script))
					InWorld.Script.runScript(OwnsHotspot.Script);
			}

			// add event
			if (!Destroyed)
				InWorld.addEvent(segment.ID, WorldEvent.EventType.SegChangeState);

			return claimed;
		}

		/// <summary>Gets the index of the parent that's shared with the provided node.  If non-existent, returns -1.</summary>
		/// <param name="relative">The potentially related node.</param>
		/// <returns>The index of the parent that's shared with the provided node.  If non-existent, returns -1.</returns>
		public int relatedParentIndex(Node relative)
		{
			for (int i = 0; i < Parents.Count; i++)
			{
				foreach (Node parent in relative.Parents)
				{
					if (Parents[i] == parent)
						return i;
				}
			}

			return -1;
		}

		/// <summary>Finds the segment that relates to the specified node.</summary>
		/// <param name="relative">The node to search for.</param>
		/// <returns>The segment that relates to the specified node.</returns>
		public int relatedSeg(Node relative)
		{
			Stack<Node> nodes = new Stack<Node>();
			Stack<Segment> fromSegs = new Stack<Segment>();
			for (int i = 0; i < Segments.Length; i++)
			{
				int lane = -1;

				if (Segments[i] == null || Segments[i].State[lane = Segments[i].getNodeIndex(this)] != SegmentSkel.SegState.Complete)
					continue;

				Node firstNode = Segments[i].Nodes[1 - lane];

				nodes.Clear();
				fromSegs.Clear();

				foreach (Segment seg in firstNode.Segments)
				{
					if (seg != null && seg != Segments[i] && seg.State[lane = seg.getNodeIndex(firstNode)] == SegmentSkel.SegState.Complete)
					{
						nodes.Push(seg.Nodes[1 - lane]);
						fromSegs.Push(seg);
					}
				}

				while (nodes.Count != 0)
				{
					Node node = nodes.Pop();
					Segment fromSeg = fromSegs.Pop();

					if (node == relative)
						return i;

					foreach (Segment seg in node.Segments)
					{
						if (seg != null && seg != fromSeg && seg.State[lane = seg.getNodeIndex(node)] == SegmentSkel.SegState.Complete)
						{
							nodes.Push(seg.Nodes[1 - lane]);
							fromSegs.Push(seg);
						}
					}
				}
			}

			return -1;
		}

		/// <summary>Adds the provided segment to this node.</summary>
		/// <param name="segment">The segment to add.</param>
		/// <returns>The index of the added segment.</returns>
		/// <param name="addEvent">Whether or not to add an event.</param>
		public int addSegment(Segment segment, bool addEvent)
		{
			for (int i = 0; i < Segments.Length; i++)
			{
				if (Segments[i] == null)
				{
					Segments[i] = segment;
					SegNumPeople[i] = 0;
					SegCapacity[i] = FInt.F0;
					NumSegments++;

					if (addEvent)
						InWorld.addEvent(ID, WorldEvent.EventType.NodeChangeState);
					return i;
				}
			}

			throw new Exception("Cannot add segment because node is already filled to capacity.");
		}

		/// <summary>Removes the specified segment from this node.</summary>
		/// <param name="segIndex">The index of the segment to remove.</param>
		/// <param name="disassociate">Whether or not to disassociate densities and parentage as well.</param>
		/// <param name="addEvent">Whether or not to add an event.</param>
		public void removeSegment(int segIndex, bool disassociate, bool addEvent)
		{
#if DEBUG
			if (Segments[segIndex] == null)
				throw new Exception("Cannot remove null segment.");
#endif
			if (disassociate)
			{
				// update density
				disassociateDensity(segIndex);

				int lane = Segments[segIndex].getNodeIndex(this);
				if (Segments[segIndex].State[lane] == Segment.SegState.Complete)
					disassociateParentage(segIndex); // update parentage
			}

			Segments[segIndex] = null;
			SegNumPeople[segIndex] = 0;
			SegCapacity[segIndex] = FInt.F0;
			NumSegments--;

			if (addEvent)
				InWorld.addEvent(ID, WorldEvent.EventType.NodeChangeState);
		}

		/// <summary>Removes the specified segment from this node.</summary>
		/// <param name="segment">The segment to remove.</param>
		/// <param name="disassociate">Whether or not to disassociate densities and parentage as well.</param>
		/// <param name="addEvent">Whether or not to add an event.</param>
		public void removeSegment(Segment segment, bool disassociate, bool addEvent)
		{
			int segIndex = 0;
			while (Segments[segIndex] != segment) { segIndex++; }
			removeSegment(segIndex, disassociate, addEvent);
		}

		/// <summary>Creates a segment to connect to the specified node.</summary>
		/// <param name="node">The node to connect to.</param>
		public void createSegment(Node node)
		{
			Segment segment = new Segment(InWorld);

			segment.Owner = Owner;
			segment.Nodes[0] = this;
			segment.Nodes[1] = node;
			segment.refreshMath();
			segment.EndLength[1] = segment.Length;
			segment.refreshEndLoc(0);
			segment.State[0] = Segment.SegState.Building;
			segment.State[1] = Segment.SegState.Complete;
			
			InWorld.addSegment(segment);
			addSegment(segment, true);
			alterDensity(0, segment.Capacity, segment);
		}

		/// <summary>Generates a new person.</summary>
		public void genPerson()
		{
			int totPeople = PeopleToSort;
			FInt totCapacity = FInt.F0;

			// first make sure the system isn't overflowing
			for (int i = 0; i < Segments.Length; i++)
			{
				if (Segments[i] != null)
				{
					totPeople += SegNumPeople[i];
					totCapacity += SegCapacity[i];
				}
			}

			// add the person
			if ((FInt)totPeople + FInt.F1 < totCapacity)
			{
				PeopleToSort++;
				alterDensity(1, FInt.F0, null);
			}
		}

		/// <summary>Destroys this node and severs all segments attached to it.</summary>
		public void destroy()
		{
			// used later
			List<Node> parents = (Parents.Count == 0 && !IsParent) ? null : new List<Node>(Parents.Count + (IsParent ? 1 : 0));

			Destroyed = true;

			// sever connected segments
			for (int i = 0; i < Segments.Length; i++)
			{
				if (Segments[i] != null)
				{
					int lane = Segments[i].getNodeIndex(this);
					int oppLane = 1 - lane;
					Segments[i].State[oppLane] = Segment.SegState.Retracting;

					// if connected to next node update node's density and parentage
					if (Segments[i].State[lane] == Segment.SegState.Complete)
					{
						int oppSegIndex = 0;
						while (Segments[i].Nodes[oppLane].Segments[oppSegIndex] != Segments[i]) { oppSegIndex++; }

						// update density
						Segments[i].Nodes[oppLane].alterDensity(Segments[i].NumPeople - Segments[i].Nodes[oppLane].SegNumPeople[oppSegIndex], -Segments[i].Nodes[oppLane].SegCapacity[oppSegIndex], Segments[i]);

						// update parentage
						if (parents != null)
						{
							// compile parents to remove
							parents.Clear();

							if (IsParent)
								parents.Add(this);

							for (int j = 0; j < Parents.Count; j++)
							{
								if (ParentFromSeg[j] != i)
									parents.Add(Parents[j]);
							}

							// remove the parents
							if (parents.Count > 0)
								Segments[i].Nodes[oppLane].removeParents(Segments[i], parents);
						}
					}

					// add event
					InWorld.addEvent(Segments[i].ID, WorldEvent.EventType.SegChangeState);

					Segments[i] = null; // remove reference so the garbage collector can do its thing
				}
			}
		}

		/// <summary>Sever a segment from this node.</summary>
		/// <param name="segIndex">The index of the segment to sever.</param>
		public void severSegment(int segIndex)
		{
			int lane = Segments[segIndex].getNodeIndex(this);
			Segments[segIndex].State[1 - lane] = Segment.SegState.Retracting;
			InWorld.addEvent(Segments[segIndex].ID, WorldEvent.EventType.SegChangeState); // add event

			removeSegment(segIndex, true, true);
		}

		/// <summary>Removes the parents of the specified node from this node and this node's parents from the node connected by the specified segment.</summary>
		/// <param name="segIndex">The index of the segment connecting the two nodes.</param>
		private void disassociateParentage(int segIndex)
		{
			Segment segment = Segments[segIndex];
			int lane = segment.getNodeIndex(this);
#if DEBUG
			if (segment.State[lane] != Segment.SegState.Complete)
				throw new Exception("Cannot disassociate parentage across an incomplete segment.");
#endif
			Node oppNode = segment.getOppNode(this);

			// get the index of the segment in the opposite node
			int oppSegIndex = 0;
			while (oppNode.Segments[oppSegIndex] != segment) { oppSegIndex++; }

			// prepare to compile the parents
			List<Node> parentsToSend = new List<Node>(Parents.Count + (IsParent ? 1 : 0));
			List<Node> parentsToReceive = new List<Node>(oppNode.Parents.Count + (oppNode.IsParent ? 1 : 0));

			// if these nodes are parents themselves...
			if (IsParent)
				parentsToSend.Add(this);

			if (oppNode.IsParent)
				parentsToReceive.Add(oppNode);

			// compile the parents
			for (int i = 0; i < Parents.Count; i++)
			{
				if (ParentFromSeg[i] != segIndex)
					parentsToSend.Add(Parents[i]);
			}

			for (int i = 0; i < oppNode.Parents.Count; i++)
			{
				if (oppNode.ParentFromSeg[i] != oppSegIndex)
					parentsToReceive.Add(oppNode.Parents[i]);
			}

			// remove the parents
			removeParents(segment, parentsToReceive);
			oppNode.removeParents(segment, parentsToSend);
		}

		/// <summary>Removes the specified segment's density from this node.  If segment is connected, also removes this node's density from the connected node.</summary>
		/// <param name="segIndex">The index of the segment to disassociate.</param>
		private void disassociateDensity(int segIndex)
		{
			Segment segment = Segments[segIndex];
			int lane = segment.getNodeIndex(this);
			alterDensity(-SegNumPeople[segIndex], -SegCapacity[segIndex], segment);

			if (segment.State[lane] == Segment.SegState.Complete)
			{
				// if the segment is retracting remove its density
				int segPeople = Segments[segIndex].NumPeople;
				FInt segCapacity = Segments[segIndex].IsRetracting() ? FInt.F0 : Segments[segIndex].Capacity;

				Node node = segment.getOppNode(this);
				int oppSegIndex = 0;
				while (node.Segments[oppSegIndex] != segment) { oppSegIndex++; }
				node.alterDensity(segPeople - node.SegNumPeople[oppSegIndex], segCapacity - node.SegCapacity[oppSegIndex], segment);
			}
		}

		/// <summary>Updates the number of segments in this node.</summary>
		public void updateNumSegments()
		{
			NumSegments = 0;
			foreach (Segment seg in Segments)
			{
				if (seg != null)
					NumSegments++;
			}
		}

		#region Recurring Methods

		/// <summary>Alters the densities in this node and all branches.</summary>
		/// <param name="people">The number of people to add.</param>
		/// <param name="capacity">The capacity to add.</param>
		/// <param name="fromSeg">The segment that called this method.  null is valid.</param>
		public void alterDensity(int people, FInt capacity, Segment fromSeg)
		{
#if DEBUG
			bool found = fromSeg == null;
#endif
			for (int i = 0; i < Segments.Length; i++)
			{
				if (Segments[i] != null)
				{
					if (Segments[i] == fromSeg)
					{
						SegNumPeople[i] += people;
						SegCapacity[i] += capacity;
#if DEBUG
						found = true;
#endif
					}
					else
					{
						int lane = Segments[i].getNodeIndex(this);

						if (Segments[i].State[lane] == Segment.SegState.Complete)
							Segments[i].Nodes[1 - lane].alterDensity(people, capacity, Segments[i]);
					}
				}
			}
#if DEBUG
			if (!found)
				throw new Exception("The specified segment is not connected to this node.");
#endif
		}

		/// <summary>Recurring method that runs through all nodes and updates densities.  Must be called only on a node with only one segment attached.</summary>
		/// <param name="setLastCheckToMax">Whether or not to set LastCheck to the maximum value.</param>
		public void updateDensity(bool setLastCheckToMax)
		{
#if DEBUG
			// make sure this was called on the correct node
			if (NumSegments > 1)
				throw new Exception("updateDensity() was called on a node with more than one segment.");
#endif
			updateDensity1stStep(null, setLastCheckToMax);
			updateDensity2ndStep(null, 0, FInt.F0);
		}

		/// <summary>1st step recurring method that runs through all nodes and updates densities.  Must first be called on a node with only one segment attached.</summary>
		/// <param name="fromSeg">The segment that called this method.</param>
		/// <param name="setLastCheckToMax">Whether or not to set LastCheck to the maximum value.</param>
		private void updateDensity1stStep(Segment fromSeg, bool setLastCheckToMax)
		{
			// the first step sets densities of all branches leading back to the first node

			for (int i = 0; i < Segments.Length; i++)
			{
				if (Segments[i] != null && Segments[i] != fromSeg)
				{
					int lane = Segments[i].getNodeIndex(this);

					// don't include density of retracting segment
					SegNumPeople[i] = 0;
					SegCapacity[i] = FInt.F0;
					if (!Segments[i].IsRetracting())
					{
						SegNumPeople[i] = Segments[i].NumPeople;
						SegCapacity[i] = Segments[i].Capacity;
					}

					// if connected to the next node...
					if (Segments[i].State[lane] == Segment.SegState.Complete)
					{
						Node oppNode = Segments[i].Nodes[1 - lane];
						oppNode.updateDensity1stStep(Segments[i], setLastCheckToMax);

						SegNumPeople[i] += oppNode.PeopleToSort;

						for (int j = 0; j < oppNode.Segments.Length; j++)
						{
							if (oppNode.Segments[j] != null && oppNode.Segments[j] != Segments[i])
							{
								SegNumPeople[i] += oppNode.SegNumPeople[j];
								SegCapacity[i] += oppNode.SegCapacity[j];
							}
						}
					}
				}
			}

			if (setLastCheckToMax)
				LastCheck = TimeSpan.MaxValue;
		}

		/// <summary>2ndst step recurring method that runs through all nodes and updates densities.  Must be called right after the 1st step on the same beginning node.</summary>
		/// <param name="fromSeg">The segment that called this method.</param>
		/// <param name="people">The total number of people in the branch that called this method.</param>
		/// <param name="capacity">The total capacity of the branch that called this method.</param>
		private void updateDensity2ndStep(Segment fromSeg, int people, FInt capacity)
		{
			// the second step updates densities of all branches lead away from the first node

			int totPeople = PeopleToSort;
			FInt totCapacity = FInt.F0;

			// get sum of all branches
			for (int i = 0; i < Segments.Length; i++)
			{
				if (Segments[i] != null)
				{
					// update from segment vars
					if (Segments[i] == fromSeg)
					{
						SegNumPeople[i] = people;
						SegCapacity[i] = capacity;
					}

					totPeople += SegNumPeople[i];
					totCapacity += SegCapacity[i];
				}
			}

			for (int i = 0; i < Segments.Length; i++)
			{
				if (Segments[i] != null && Segments[i] != fromSeg)
				{
					int lane = Segments[i].getNodeIndex(this);

					// if connected to the next node...
					if (Segments[i].State[i] == Segment.SegState.Complete)
						Segments[i].Nodes[1 - lane].updateDensity2ndStep(Segments[i], totPeople - SegNumPeople[i], totCapacity - SegCapacity[i] + Segments[i].Capacity);
				}
			}
		}

		/// <summary>Adds this node as parent to all of its child nodes.  Must only be called on parent nodes.</summary>
		public void spreadParentage()
		{
#if DEBUG
			if (!IsParent)
				throw new Exception("updateParentage() must only be called on parent nodes.");
#endif
			foreach (Segment seg in Segments)
			{
				if (seg != null)
				{
					int lane = seg.getNodeIndex(this);
					Node oppNode = seg.Nodes[1 - lane];

					if (seg.State[lane] == Segment.SegState.Complete)
						oppNode.addParents(seg, new Node[] { this });
				}
			}
		}

		/// <summary>Adds the specified parents to this node and all branches.</summary>
		/// <param name="fromSeg">The segment that called this method.</param>
		/// <param name="parents">The parents to add.</param>
		public void addParents(Segment fromSeg, IEnumerable<Node> parents)
		{
			Parents.AddRange(parents);

			for (int i = 0; i < Segments.Length; i++)
			{
				if (Segments[i] != null)
				{
					if (Segments[i] == fromSeg)
					{
						foreach (Node parent in parents)
						{
							ParentFromSeg.Add(i);
						}
					}
					else
					{
						int lane = Segments[i].getNodeIndex(this);
						Node oppNode = Segments[i].Nodes[1 - lane];

						if (Segments[i].State[lane] == Segment.SegState.Complete)
							oppNode.addParents(Segments[i], parents);
					}
				}
			}
		}

		/// <summary>Removes the specified parents from this node and all branches.</summary>
		/// <param name="fromSeg">The segment that called this method.</param>
		/// <param name="parents">The parents to add.</param>
		public void removeParents(Segment fromSeg, IEnumerable<Node> parents)
		{
#if	DEBUG
			// get fromSeg's index in Segments
			int fromSegIndex = 0;
			while (Segments[fromSegIndex] != fromSeg && fromSegIndex < Segments.Length) { fromSegIndex++; }
			if (fromSegIndex == Segments.Length)
				throw new Exception("fromSeg is not connected to this node.");
#endif
			// remove the parents from this node
			bool spreadFurther = false; // if false, then 'parents' is empty and shouldn't be passed on to connected nodes
			foreach (Node parent in parents)
			{
				int index = Parents.IndexOf(parent);
#if DEBUG
				if (index == -1)
					throw new Exception("This node does not contain the specified parent.");
				else if (ParentFromSeg[index] != fromSegIndex)
					throw new Exception("The parent to remove comes from a different segment than the one that called this method.");
#endif
				Parents.RemoveAt(index);
				ParentFromSeg.RemoveAt(index);

				spreadFurther = true;
			}

			if (spreadFurther)
			{
				foreach (Segment seg in Segments)
				{
					if (seg != null && seg != fromSeg)
					{
						int lane = seg.getNodeIndex(this);
						Node oppNode = seg.Nodes[1 - lane];

						if (seg.State[lane] == Segment.SegState.Complete)
							oppNode.removeParents(seg, parents);
					}
				}
			}
		}

		#endregion Recurring Methods

		#endregion Methods
	}
}
