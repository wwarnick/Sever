using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class Segment : SegmentSkel
	{
		#region Members

		/// <summary>The player that owns this segment.</summary>
		public override Player Owner
		{
			get { return owner; }
			set
			{
				if (owner != value)
				{
					if (owner != null)
						owner.Segments.Remove(this);

					owner = value;

					if (owner != null)
						owner.Segments.Add(this);
				}
			}
		}

		/// <summary>The world that contains this segment.</summary>
		public World InWorld { get; set; }

		/// <summary>The distance between the two connecting nodes.</summary>
		public FInt Length { get; set; }

		/// <summary>The length of each end of the segment from its opposite node.</summary>
		public FInt[] EndLength { get; set; }

		/// <summary>The two nodes at each end of the segment.</summary>
		public Node[] Nodes { get; set; }

		/// <summary>The list of people in the segment.</summary>
		public LList<FInt>[] People { get; private set; }

		/// <summary>The number of people in this segment.</summary>
		public int NumPeople { get; private set; }

		/// <summary>The capacity of this segment.</summary>
		public FInt Capacity { get; private set; }

		/// <summary>The current capacity of this segment.</summary>
		public FInt CurCapacity { get; private set; }

		/// <summary>The current capacity for each lane in this segment.</summary>
		private FInt curLaneCapacity;

		/// <summary>The current capacity for each lane in this segment.</summary>
		public FInt CurLaneCapacity
		{
			get { return curLaneCapacity; }
			private set
			{
				curLaneCapacity = value;
				CurCapacity = value * FInt.F2;
			}
		}

		/// <summary>The direction that this segment is running.</summary>
		public VectorF Direction { get; set; }

		/// <summary>The direction perpendicular to this segment's direction.</summary>
		public VectorF DirectionPerp { get; set; }

		/// <summary>The angle that this segment is running.</summary>
		public FInt Angle { get; set; }

		/// <summary>Whether or not this segment has been destroyed.</summary>
		public bool Destroyed { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of Segment.</summary>
		/// <param name="inWorld">The world that contains this segment.</param>
		public Segment(World inWorld)
		{
			clear();

			InWorld = inWorld;
		}

		/// <summary>Clones the provided segment.  All references to other nodes or segments are deep.</summary>
		/// <param name="toClone">The segment to clone.</param>
		public Segment(Segment toClone)
			: this(toClone.InWorld)
		{
			this.ID = toClone.ID;
			this.EndLoc = (VectorF[])toClone.EndLoc.Clone();
			this.CurLength = toClone.CurLength;
			this.State = (SegState[])toClone.State.Clone();
			this.owner = toClone.Owner;
			this.Length = toClone.Length;
			this.EndLength = (FInt[])toClone.EndLength.Clone();
			this.Nodes = (Node[])toClone.Nodes.Clone();
			this.People[0] = toClone.People[0].Clone();
			this.People[1] = toClone.People[1].Clone();
			this.NumPeople = toClone.NumPeople;
			this.Capacity = toClone.Capacity;
			this.CurCapacity = toClone.CurCapacity;
			this.curLaneCapacity = toClone.CurLaneCapacity;
			this.Direction = toClone.Direction;
			this.DirectionPerp = toClone.DirectionPerp;
			this.Angle = toClone.Angle;
			this.Destroyed = toClone.Destroyed;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public override void clear()
		{
			base.clear();

			InWorld = null;
			Length = FInt.F0;
			EndLength = new FInt[2];
			Nodes = new Node[2];
			People = new LList<FInt>[2];
			People[0] = new LList<FInt>();
			People[1] = new LList<FInt>();
			NumPeople = 0;
			Capacity = FInt.F0;
			CurLaneCapacity = FInt.F0;
			Direction = VectorF.Zero;
			Direction = VectorF.Zero;
			Angle = FInt.F0;
			Destroyed = false;
		}

		/// <summary>Gets the node opposite the one provided.</summary>
		/// <param name="node">The node to search for.</param>
		/// <returns>The node opposite the one provided.</returns>
		public Node getOppNode(Node node)
		{
			if (Nodes[0] == node)
				return Nodes[1];
#if DEBUG
			else if (Nodes[1] == node)
				return Nodes[0];
			else
				throw new Exception("Provided node is not connected to this segment.");
#else
			return Nodes[0];
#endif
		}

		/// <summary>Gets the index of the provided node within the segment.</summary>
		/// <param name="node">The node to search for.</param>
		/// <returns>The index of the provided node within the segment.</returns>
		public int getNodeIndex(Node node)
		{
			if (Nodes[0] == node)
				return 0;
#if DEBUG
			else if (Nodes[1] == node)
				return 1;
			else
				throw new Exception("Provided node is not connected to this segment.");
#else
			return 1;
#endif
		}

		/// <summary>Updates this segment.</summary>
		/// <param name="elapsed">The time elapsed since the last update.</param>
		public void update(FInt elapsed)
		{
			bool updateVisibility = false;

			// update segment activity
			for (int i = 0; i < 2; i++)
			{
				switch (State[i])
				{
					case SegState.Complete:
					case SegState.Building:
						// do nothing
						break;
					case SegState.Retracting:
						EndLength[i] -= InWorld.RetractSpeed * elapsed;
						refreshEndLoc(1 - i);
						updateVisibility = true;
						InWorld.addEvent(ID, WorldEvent.EventType.SegChangeState);
						break;
					default:
						throw new Exception("Unrecognized segment state: " + State[i].ToString());
				}
			}

			// update person activity
			FInt speed = (CurCapacity == 0) ? InWorld.PersonSpeedLower : (Calc.Min(InWorld.PersonSpeedUpper, ((People[0].Count + People[1].Count) / CurCapacity * InWorld.PersonSpeedRange + InWorld.PersonSpeedLower)) * elapsed);
			for (int i = 0; i < 2; i++)
			{
				bool removeLast = false;

				for (LListNode<FInt> person = People[i].Last; person != null; person = person.Previous)
				{
					if (removeLast)
						People[i].RemoveLast();

					removeLast = false;

					person.Value += speed;

					if (person.Value > EndLength[i])
					{
						switch (State[i])
						{
							case SegState.Complete:
								Nodes[1 - i].addPerson(this);
								break;
							case SegState.Retracting:
								addPerson(1 - i);
								break;
							case SegState.Building:
								addPerson(1 - i);
								buildLane(i);
								break;
							default:
								throw new Exception("Unrecognized segment state: " + State[i].ToString());
						}

						removeLast = true;
					}
				}

				if (removeLast)
					People[i].RemoveLast();
			}

			NumPeople = People[0].Count + People[1].Count;

			// if retracting and length reaches zero...
			FInt netLength = EndLength[0] + EndLength[1];
			if (netLength < Length)
			{
				for (int i = 0; i < 2; i++)
				{
					if (State[i] == SegState.Retracting)
					{
						switch (State[1 - i])
						{
							case SegState.Complete:
								if (Nodes[i].Parents.Count > 0)
									Nodes[i].removeSegment(this, true, true); // if node, is okay, disconnect from it
								else
									Nodes[i].destroy(); // if no parents, destroy node
								break;
							case SegState.Building:
								if (!Nodes[i].Active) // if the segment was building towards an inactive node, destroy it
									Nodes[i].destroy();
								break;
							case SegState.Retracting:
								// do nothing
								break;
						}

						updateVisibility = false;
						destroy();
						break;
					}
				}
			}

			// update the visibility
			if (updateVisibility && Owner.Type != Player.PlayerType.Human && Visible)
				Visible = InWorld.HumanPlayer.Fog.isVisible(this);


			// get rid of people overflow (once per segment per update)
			if (!Destroyed && NumPeople > CurCapacity)
			{
				// see if there is an eligible person to remove
				int rLane = -1;

				for (rLane = 0; rLane < 2; rLane++)
				{
					LListNode<FInt> first = People[rLane].First;

					if (first != null &&
						((first.Value < Length - EndLength[1 - rLane]) ||
						(State[0] != SegState.Complete && State[1] != SegState.Complete && NumPeople == 1)))
						break;
				}

				if (rLane < 2)
				{
					// find a connected node
					Node testNode = null;
					for (int i = 0; i < 2; i++)
					{
						if (State[i] == SegState.Complete)
						{
							testNode = Nodes[1 - i];
							break;
						}
					}

					// find out if the entire system is overflowing
					int totPeople = 0;
					FInt totCapacity = FInt.F0;
					if (testNode != null)
					{
						totPeople += testNode.PeopleToSort;
						for (int i = 0; i < testNode.Segments.Length; i++)
						{
							if (testNode.Segments[i] != null)
							{
								totPeople += testNode.SegNumPeople[i];
								totCapacity += testNode.SegCapacity[i];
							}
						}
					}
					else
					{
						totPeople = NumPeople;
						totCapacity = CurCapacity;
					}

					// remove the person
					if (totPeople > totCapacity)
					{
						People[rLane].RemoveFirst();
						NumPeople--;

						if (State[0] == SegState.Complete)
							Nodes[1].alterDensity(-1, FInt.F0, this);

						if (State[1] == SegState.Complete)
							Nodes[0].alterDensity(-1, FInt.F0, this);
					}
				}
			}
		}

		/// <summary>Destroys this segment.</summary>
		public void destroy()
		{
			for (int i = 0; i < 2; i++)
			{
				int oppLane = 1 - i;
				if (State[i] == SegState.Building && !Nodes[oppLane].Active && !Nodes[oppLane].Destroyed)
					Nodes[oppLane].destroy();
			}

			// mark to destroy
			Destroyed = true;
		}

		/// <summary>Splits this segment into two pieces, and retracts them away from the split point.</summary>
		/// <param name="splitPoint">The distance from Node[0] to perform the split.</param>
		public void split(FInt splitPoint)
		{
			// prepare to update densities
			int[] nodePpl = new int[2];
			FInt[] nodeCap = new FInt[2];
			for (int i = 0; i < 2; i++)
			{
				if (State[i] == SegState.Complete)
				{
					int oppLane = 1 - i;
					int index = 0;
					while (Nodes[oppLane].Segments[index] != this) { index++; }

					// prepare densities
					nodePpl[oppLane] = Nodes[oppLane].SegNumPeople[index];
					nodeCap[oppLane] = Nodes[oppLane].SegCapacity[index];
				}
			}

			// prepare to update parentage
			List<Node>[] nodePar = new List<Node>[2];
			if (State[0] == SegState.Complete && State[1] == SegState.Complete)
			{
				nodePar[0] = new List<Node>(Nodes[1].Parents.Count + ((Nodes[1].IsParent) ? 1 : 0));
				nodePar[1] = new List<Node>(Nodes[0].Parents.Count + ((Nodes[0].IsParent) ? 1 : 0));

				if (Nodes[1].IsParent)
					nodePar[0].Add(Nodes[1]);

				int segIndex = 0;
				while (Nodes[1].Segments[segIndex] != this) { segIndex++; }

				for (int i = 0; i < Nodes[1].Parents.Count; i++)
				{
					if (Nodes[1].ParentFromSeg[i] == segIndex)
						nodePar[1].Add(Nodes[1].Parents[i]);
					else
						nodePar[0].Add(Nodes[1].Parents[i]);
				}
			}

			// create new segment
			Segment newSeg = new Segment(InWorld);
			newSeg.Angle = Angle;
			newSeg.Capacity = Capacity;
			newSeg.Destroyed = Destroyed;
			newSeg.Direction = Direction;
			newSeg.DirectionPerp = DirectionPerp;
			newSeg.Length = Length;
			newSeg.Nodes[0] = Nodes[0];
			newSeg.Nodes[1] = Nodes[1];
			newSeg.Owner = Owner;
			newSeg.State[0] = State[0];
			newSeg.State[1] = SegState.Retracting;
			newSeg.EndLength[0] = EndLength[0];
			newSeg.EndLength[1] = Length - splitPoint;
			newSeg.EndLoc[1] = EndLoc[1];
			newSeg.refreshEndLoc(0);

			// divvy up the people

			// first lane
			for (LListNode<FInt> p = People[0].Last; p != null && p.Value >= splitPoint; p = People[0].Last)
			{
				People[0].RemoveLast();
				newSeg.People[0].AddFirst(p);
			}

			// second lane
			for (LListNode<FInt> p = People[1].First; p != null && p.Value <= newSeg.EndLength[1]; p = People[1].First)
			{
				People[1].RemoveFirst();
				newSeg.People[1].AddLast(p);
			}

			NumPeople = People[0].Count + People[1].Count;
			newSeg.NumPeople = newSeg.People[0].Count + newSeg.People[1].Count;

			nodePpl[0] -= NumPeople;
			nodePpl[1] -= newSeg.NumPeople;

			// update densities and parentage
			for (int i = 0; i < 2; i++)
			{
				if (State[i] == SegState.Complete)
				{
					int oppLane = 1 - i;
					Nodes[oppLane].alterDensity(-nodePpl[oppLane], -nodeCap[oppLane], this);

					// only update parentage if both sides are complete
					if (State[1 - i] == SegState.Complete)
						Nodes[i].removeParents(this, nodePar[i]);
				}
			}

			// update the node connections
			if (State[0] == SegState.Complete)
			{
				for (int i = 0; i < Nodes[1].Segments.Length; i++)
				{
					if (Nodes[1].Segments[i] == this)
					{
						Nodes[1].Segments[i] = newSeg;
						InWorld.addEvent(Nodes[1].ID, WorldEvent.EventType.NodeChangeState);
						break;
					}
				}
			}

			// update this segment's values
			State[0] = SegState.Retracting;
			EndLength[0] = splitPoint;
			refreshEndLoc(1);

			// add segments to the player and world
			InWorld.addSegment(newSeg);

			// add event
			InWorld.addEvent(ID, WorldEvent.EventType.SegChangeState);
		}

		/// <summary>Adds a person to the specified lane.</summary>
		/// <param name="lane">The lane to add the person to.</param>
		public void addPerson(int lane)
		{
			FInt first = (People[lane].First == null) ? FInt.MaxValue : (People[lane].First.Value - InWorld.PersonSpacing);
			People[lane].AddFirst(Calc.Min(Length - EndLength[1 - lane], first));
			NumPeople++;
		}

		/// <summary>Adds a person to the lane leading from the specified node.</summary>
		/// <param name="fromNode">The node from which the person should come.</param>
		public void addPerson(Node fromNode)
		{
			addPerson(getNodeIndex(fromNode));
		}

		/// <summary>Builds the specified lane by one increment.</summary>
		/// <param name="lane">The lane to build.</param>
		public void buildLane(int lane)
		{
			int oppLane = 1 - lane;

			EndLength[lane] += InWorld.BuildRate;

			// test for collision
			VectorF end0 = EndLoc[1 - lane];
			VectorF end1 = end0 + ((((lane == 0) ? FInt.F1 : FInt.FN1) * InWorld.BuildRate) * Direction);
			SegmentSkel segColl = null;
			VectorF? intersection = null;

#if DEBUG
			// make sure it's not going off the map
			bool collided =
				(Calc.Min(end0.X, end1.X) < FInt.F0 ||
				Calc.Min(end0.Y, end1.Y) < FInt.F0 ||
				Calc.Max(end0.X, end1.X) >= InWorld.Width ||
				Calc.Max(end0.Y, end1.Y) >= InWorld.Height);
#else
			bool collided;
#endif

			// check for collisions
#if DEBUG
			if (!collided)
			{
#endif
				NodeSkel nodeColl = null;
				GeoSkel geoColl = null;
				List<NodeSkel> ignoreNode = new List<NodeSkel>(2) { Nodes[0], Nodes[1] };
				List<SegmentSkel> ignoreSeg = new List<SegmentSkel>(Nodes[0].NumSegments + Nodes[1].NumSegments + 1);
				ignoreSeg.Add(this);
				for (int i = 0; i < 2; i++)
				{
					foreach (Segment seg in Nodes[i].Segments)
					{
						if (seg != null && seg != this)
							ignoreSeg.Add(seg);
					}
				}

				collided = InWorld.Collision.segCollisionIntPoint(end0, end1, ignoreSeg, ignoreNode, out segColl, out nodeColl, out geoColl, out intersection);
#if DEBUG
			}
#endif

			// if it collided with something...
			if (collided)
			{
				if (segColl != null)
				{
					// if owned by someone else, split the segment at the intersection point
					if (segColl.Owner != Owner)
						((Segment)segColl).split(VectorF.Distance(((Segment)segColl).Nodes[0].Pos, intersection.Value));
				}

				// if the node was a new, inactive node, get rid of it
				if (!Nodes[oppLane].Active)
					Nodes[oppLane].destroy();

				State[lane] = SegState.Retracting;

				if (State[oppLane] == SegState.Complete)
					Nodes[lane].alterDensity(0, -Capacity, this);
			}
			else if (EndLength[lane] >= Length) // if building is complete...
			{
				EndLength[lane] = Length;
				Nodes[oppLane].claim(this);
			}

			refreshEndLoc(oppLane);

			// update visibility
			if (Owner.Type != Player.PlayerType.Human && !Visible)
				Visible = InWorld.HumanPlayer.Fog.isVisible(this);

			// add event
			InWorld.addEvent(ID, WorldEvent.EventType.SegChangeState);
		}

		/// <summary>Refreshes the time-consuming math of the segment.</summary>
		public void refreshMath()
		{
			Length = VectorF.Distance(Nodes[0].Pos, Nodes[1].Pos);
			Direction = VectorF.Normalize(Nodes[1].Pos - Nodes[0].Pos);
			Angle = Calc.FindAngle(Nodes[0].Pos, Nodes[1].Pos);
			NumPeople = People[0].Count + People[1].Count;
			Capacity = Length * FInt.F2 / InWorld.PersonSpacing;
			refreshEndLocs();

			VectorF perp = VectorF.Normalize(Nodes[1].Pos - Nodes[0].Pos);
			FInt temp = perp.X;
			perp.X = -perp.Y;
			perp.Y = temp;
			DirectionPerp = perp;
		}

		/// <summary>Refresh both ends' locations and the segment's current length and capacity.</summary>
		public void refreshEndLocs()
		{
			EndLoc[0] = Nodes[1].Pos + -Direction * EndLength[1];
			EndLoc[1] = Nodes[0].Pos + Direction * EndLength[0];

			refreshCurLengthAndCap();
		}

		/// <summary>Refresh the specified end's location and the segment's current length and capacity.</summary>
		/// <param name="end">The end to refresh.</param>
		public void refreshEndLoc(int end)
		{
			int oppEnd = 1 - end;
			EndLoc[end] = Nodes[oppEnd].Pos + ((end == 0) ? FInt.FN1 : FInt.F1) * Direction * EndLength[oppEnd];

			refreshCurLengthAndCap();
		}

		/// <summary>Refreshes the current length and capacity of the segment.</summary>
		public void refreshCurLengthAndCap()
		{
			CurLength = VectorF.Distance(EndLoc[0], EndLoc[1]);
			CurLaneCapacity = CurLength / InWorld.PersonSpacing;
		}

		/// <summary>Determines whether or not the segment overlaps the specified point.</summary>
		/// <param name="point"></param>
		/// <returns>Whether or not the segment overlaps the specified point.</returns>
		public bool overlapsPoint(VectorF point)
		{
			// TODO: specify threshold
			return Calc.LinePointDistApprox(point, EndLoc[0], EndLoc[1], CurLength) < (FInt)40 && // threshold * 4 for the approximation
				Calc.LinePointDistance(point, EndLoc[0], EndLoc[1], CurLength) < (FInt)10;
		}

		#endregion Methods
	}
}
