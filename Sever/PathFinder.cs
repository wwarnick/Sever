using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class PathFinder
	{
		#region Members

		/// <summary>The world that contains this path finder.</summary>
		public World InWorld { get; set; }

		/// <summary>The grid of path nodes.</summary>
		private PathNode[,] grid;

		/// <summary>The grid of path nodes.</summary>
		public PathNode[,] Grid {
			get { return grid; }
			private set
			{
				grid = value;

				if (grid != null)
				{
					NumRows = grid.GetLength(0);
					NumCols = grid.GetLength(1);
				}
				else
				{
					NumRows = 0;
					NumCols = 0;
				}
			}
		}

		/// <summary>An array of all of the nodes.</summary>
		public PathNode[] Nodes { get; private set; }

		/// <summary>The number of rows in the grid.</summary>
		public int NumRows { get; private set; }

		/// <summary>The number of columns in the grid.</summary>
		public int NumCols { get; private set; }

		/// <summary>The spacing between each adjacent node.</summary>
		private VectorD nodeSpacing;

		/// <summary>The spacing between each adjacent node.</summary>
		public VectorD NodeSpacing
		{
			get { return nodeSpacing; }
			private set
			{
				nodeSpacing = value;
				crossEdgeSlopeDown = (nodeSpacing.X == 0d) ? 0d : (nodeSpacing.Y / nodeSpacing.X);
				crossEdgeSlopeUp = -crossEdgeSlopeDown;
				crossEdgeLength = NodeSpacing.Length();
			}
		}

		/// <summary>The slope of the downward diagonal edges.</summary>
		private double crossEdgeSlopeDown;

		/// <summary>The slope of the upward diagonal edges.</summary>
		private double crossEdgeSlopeUp;

		/// <summary>The length of the diagonal lines.</summary>
		private double crossEdgeLength;

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of PathFinder.</summary>
		/// <param name="numRows">The number of rows in the grid.</param>
		/// <param name="numCols">The number of columns in the grid.</param>
		/// <param name="inWorld">The world that contains this path finder.</param>
		public PathFinder(int numRows, int numCols, World inWorld)
		{
			clear();

			InWorld = inWorld;
			Grid = new PathNode[numRows, numCols];
			Nodes = new PathNode[numRows * numCols];
			NodeSpacing = (VectorD)inWorld.Size / new VectorD((double)numCols, (double)numRows);

			// build grid
			PathNode node = null; // temporary variable

			// initialize each node in the grid
			for (int row = 0; row < numRows; row++)
			{
				for (int col = 0; col < numCols; col++)
				{
					node = new PathNode();
					node.Index = row * NumCols + col;
					node.Position = new VectorD((double)col * NodeSpacing.X, (double)row * NodeSpacing.Y);
					Grid[row, col] = node;
					Nodes[node.Index] = node;
				}
			}

			// connect nodes to each other, if not blocked
			for (int row = 0; row < numRows; row++)
			{
				for (int col = 0; col < numCols; col++)
				{
					// use collision detection to rule out connections (only geos can block)
					node = Grid[row, col];
					PathNode destNode = null;

					VectorF nodePos = (VectorF)node.Position;

					if (col < numCols - 1)
					{
						// upper-right
						if (row > 0)
						{
							destNode = Grid[row - 1, col + 1];

							if (!inWorld.Collision.segCollision(nodePos, (VectorF)destNode.Position, null, null, false, false, true))
							{
								node.addLast(new PathEdge(this, node.Index, destNode.Index));
								destNode.addLast(new PathEdge(this, destNode.Index, node.Index));
							}
						}

						// right
						destNode = Grid[row, col + 1];
						if (!inWorld.Collision.segCollision(nodePos, (VectorF)destNode.Position, null, null, false, false, true))
						{
							node.addLast(new PathEdge(this, node.Index, destNode.Index));
							destNode.addLast(new PathEdge(this, destNode.Index, node.Index));
						}

						// lower right
						if (row < numRows - 1)
						{
							destNode = Grid[row + 1, col + 1];
							if (!inWorld.Collision.segCollision(nodePos, (VectorF)destNode.Position, null, null, false, false, true))
							{
								node.addLast(new PathEdge(this, node.Index, destNode.Index));
								destNode.addLast(new PathEdge(this, destNode.Index, node.Index));
							}
						}
					}

					// bottom
					if (row < numRows - 1)
					{
						destNode = Grid[row + 1, col];
						if (!inWorld.Collision.segCollision(nodePos, (VectorF)destNode.Position, null, null, false, false, true))
						{
							node.addLast(new PathEdge(this, node.Index, destNode.Index));
							destNode.addLast(new PathEdge(this, destNode.Index, node.Index));
						}
					}
				}
			}
		}

		/// <summary>Creates a new instance of PathFinder.</summary>
		/// <param name="grid">The grid of path nodes.</param>
		/// <param name="inWorld">The world that contains this path finder.</param>
		public PathFinder(PathNode[,] grid, World inWorld)
		{
			clear();

			InWorld = InWorld;
			Grid = grid;
			Nodes = new PathNode[NumRows * NumCols];
			for (int row = 0; row < NumRows; row++)
			{
				for (int col = 0; col < NumCols; col++)
				{
					Nodes[row * NumCols + col] = Grid[row, col];
				}
			}
			NodeSpacing = (VectorD)inWorld.Size / new VectorD((double)NumCols, (double)NumRows);
		}

		#endregion Constructore

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public void clear()
		{
			InWorld = null;
			Grid = null;
			Nodes = null;
			NodeSpacing = VectorD.Zero;
		}

		/// <summary>Clears all intersections from all edges.</summary>
		public void clearIntersections()
		{
			foreach (PathNode node in Nodes)
			{
				for (PathEdge edge = node.FirstEdge; edge != null; edge = edge.Next)
				{
					edge.NumIntersections = 0;
				}
			}
		}

		/// <summary>Adds an intersection to all edges that intersect the specified circle.</summary>
		/// <param name="point">The center of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="toAdd">The number of intersections to add.</param>
		public void intersect(VectorF point, FInt radius, int toAdd)
		{
			VectorD pointD = (VectorD)point;
			double radiusD = (double)radius;

			int left = (int)((pointD.X - radiusD) / NodeSpacing.X);
			int top = (int)((pointD.Y - radiusD) / NodeSpacing.Y);
			int right = (int)Calc.RoundUp((pointD.X + radiusD) / NodeSpacing.X);
			int bottom = (int)Calc.RoundUp((pointD.Y + radiusD) / NodeSpacing.Y);

			for (int row = top; row <= bottom; row++)
			{
				double y = (double)row * NodeSpacing.Y;

				for (int col = left; col <= right; col++)
				{
					VectorD tl = new VectorD((double)col * NodeSpacing.X, y);
					double nx = tl.X + NodeSpacing.X;
					double ny = tl.Y + NodeSpacing.Y;

					if (Calc.LinePointDistance(pointD, tl, new VectorD(tl.X, ny), NodeSpacing.Y) <= radiusD)
						intersect(Grid[row, col], Grid[row + 1, col], toAdd);
					if (Calc.LinePointDistance(pointD, tl, new VectorD(nx, y), NodeSpacing.X) <= radiusD)
						intersect(Grid[row, col], Grid[row, col + 1], toAdd);
					if (Calc.LinePointDistance(pointD, tl, new VectorD(nx, ny), crossEdgeLength) <= radiusD)
						intersect(Grid[row, col], Grid[row + 1, col + 1], toAdd);
					if (Calc.LinePointDistance(pointD, new VectorD(tl.X, ny), new VectorD(nx, y), crossEdgeLength) <= radiusD)
						intersect(Grid[row + 1, col], Grid[row, col + 1], toAdd);
				}
			}
		}

		/// <summary>Adds an intersection to all edges that intersect the specified line.</summary>
		/// <param name="point1">The first point in the line.</param>
		/// <param name="point2">The second point in the line.</param>
		/// <param name="toAdd">The number of intersections to add.</param>
		public void intersect(VectorF point1, VectorF point2, int toAdd)
		{
			VectorD point1D = (VectorD)point1;
			VectorD point2D = (VectorD)point2;

			// find first square
			int col = (int)(point1D.X / NodeSpacing.X);
			int row = (int)(point1D.Y / NodeSpacing.Y);

			// find last square
			int fCol = (int)(point2D.X / NodeSpacing.X);
			int fRow = (int)(point2D.Y / NodeSpacing.Y);

			// if outside boundaries...
			if (Math.Min(row, Math.Min(col, Math.Min(fRow, fCol))) < 0 || Math.Max(row, Math.Max(col, Math.Max(fRow, fCol))) >= NumCols)
				return;

			// correct values
			/*col = Math.Min(Math.Max(col, 0), NumCols - 1);
			row = Math.Min(Math.Max(row, 0), NumRows - 1);
			fCol = Math.Min(Math.Max(fCol, 0), NumCols - 1);
			fRow = Math.Min(Math.Max(fRow, 0), NumRows - 1);*/

			{ // test top-most and left-most edges
				double e = (double)Math.Min(col, fCol) * NodeSpacing.X;
				if (point1D.X == e)
					intersect(Grid[row, col], Grid[row + 1, col], toAdd);
				else if (point2D.X == e)
					intersect(Grid[fRow, fCol], Grid[fRow + 1, fCol], toAdd);

				e = (double)Math.Min(row, fRow) * NodeSpacing.Y;
				if (point1D.Y == e)
					intersect(Grid[row, col], Grid[row, col + 1], toAdd);
				else if (point2D.Y == e)
					intersect(Grid[fRow, fCol], Grid[fRow, fCol + 1], toAdd);
			}

			// general direction
			bool right;
			bool down;
			// increment direction
			int cold;
			int rowd;
			// grid offsets; used when finding y & x intercepts
			int colo;
			int rowo;

			if (right = fCol > col)
			{
				cold =
				colo = 1;
			}
			else
			{
				cold = -1;
				colo = 0;
			}

			if (down = fRow > row)
			{
				rowd =
				rowo = 1;
			}
			else
			{
				rowd = -1;
				rowo = 0;
			}

			// get the slope
			double slope = (point2D.Y - point1D.Y) / ((point2D.X == point1D.X) ? 0d : (point2D.X - point1D.X));

			// run the line
			while (col != fCol || row != fRow)
			{
				intersect(point1D, point2D, row, col, toAdd);

				double hCost = Math.Abs(((col + colo) * NodeSpacing.X - point1D.X) * slope); // the cost of moving left or right
				double vCost = Math.Abs((row + rowo) * NodeSpacing.Y - point1D.Y); // the cost of moving up or down

				// move to next square

				if (hCost == vCost) // if it hits the corner dead-on
				{
					if (right)
					{
						if (down) // down-right
						{
							if (row < NumRows - 1 && col < NumCols - 1)
							{
								intersect(Grid[row, col + 1], Grid[row + 1, col + 1], toAdd); // up
								intersect(Grid[row + 1, col], Grid[row + 1, col + 1], toAdd); // left

								if (col < NumCols - 2)
								{
									intersect(Grid[row + 1, col + 1], Grid[row, col + 2], toAdd); // up-right
									intersect(Grid[row + 1, col + 1], Grid[row + 1, col + 2], toAdd); // right
								}
								if (row < NumRows - 2)
								{
									intersect(Grid[row + 1, col + 1], Grid[row + 2, col], toAdd); // down-left
									intersect(Grid[row + 1, col + 1], Grid[row + 2, col + 1], toAdd); // down
								}
							}
						}
						else // up-right
						{
							if (col < NumCols - 1)
							{
								intersect(Grid[row, col], Grid[row, col + 1], toAdd); // right

								if (row > 0)
								{
									intersect(Grid[row, col + 1], Grid[row - 1, col], toAdd); // up-left
									intersect(Grid[row, col + 1], Grid[row - 1, col + 1], toAdd); // up
								}

								if (col < NumCols - 2)
								{
									intersect(Grid[row, col + 1], Grid[row, col + 2], toAdd); // right

									if (row < NumRows - 1)
										intersect(Grid[row, col + 1], Grid[row + 1, col + 2], toAdd); // down-right
								}

								if (row < NumRows - 1)
									intersect(Grid[row, col + 1], Grid[row + 1, col + 1], toAdd); // down
							}
						}
					}
					else if (down) // down-left
					{
						if (row < NumRows - 1)
						{
							intersect(Grid[row, col], Grid[row + 1, col], toAdd); // up

							if (col > 0)
							{
								intersect(Grid[row + 1, col], Grid[row, col - 1], toAdd); // up-left
								intersect(Grid[row + 1, col], Grid[row + 1, col - 1], toAdd); // left
							}

							if (row < NumRows - 2)
							{
								intersect(Grid[row + 1, col], Grid[row + 2, col], toAdd); // down

								if (col < NumCols - 1)
									intersect(Grid[row + 1, col], Grid[row + 2, col + 1], toAdd); // down-right
							}

							if (col < NumCols - 1)
								intersect(Grid[row + 1, col], Grid[row + 1, col + 1], toAdd); // right
						}
					}
					else // up-left
					{
						if (row > 0)
						{
							intersect(Grid[row, col], Grid[row - 1, col], toAdd); // up

							if (col < NumCols - 1)
								intersect(Grid[row, col], Grid[row - 1, col + 1], toAdd); // up-right
						}

						if (row < NumRows - 1)
						{
							intersect(Grid[row, col], Grid[row + 1, col], toAdd); // down

							if (col > 0)
								intersect(Grid[row, col], Grid[row + 1, col - 1], toAdd); // down-left
						}

						if (col > 0)
							intersect(Grid[row, col], Grid[row, col - 1], toAdd); // left

						if (col < NumCols - 1)
							intersect(Grid[row, col], Grid[row, col + 1], toAdd); // right
					}

					if (col != fCol)
						col += cold;

					if (row != fRow)
						row += rowd;
				}
				else if ((hCost	< vCost	&& col != fCol) || row == fRow) // horizontal
				{
					if (right)
						intersect(Grid[row, col + 1], Grid[row + 1, col + 1], toAdd);
					else
						intersect(Grid[row, col], Grid[row + 1, col], toAdd);

					col += cold; // move left or right
				}
				else // vertical
				{
					if (down)
						intersect(Grid[row + 1, col], Grid[row + 1, col + 1], toAdd);
					else
						intersect(Grid[row, col], Grid[row, col + 1], toAdd);

					row += rowd; // move up or down
				}
			}

			// last grid square
			intersect(point1D, point2D, fRow, fCol, toAdd);
		}

		/// <summary>Adds the specified number of intersections to the intersecting diagonal edges in the specified square.</summary>
		/// <param name="point1">The first point in the line.</param>
		/// <param name="point2">The second point in the line.</param>
		/// <param name="row">The row to test.</param>
		/// <param name="col">The column to test.</param>
		/// <param name="toAdd">The number of intersections to add.</param>
		private void intersect(VectorD point1, VectorD point2, int row, int col, int toAdd)
		{
			// downward slope first
			VectorD ul = new VectorD((double)col * NodeSpacing.X, (double)row * nodeSpacing.Y);
			double diff1 = Calc.LinePointDifference(point1, ul, ul + NodeSpacing, crossEdgeSlopeDown);
			double diff2 = Calc.LinePointDifference(point2, ul, ul + NodeSpacing, crossEdgeSlopeDown);
			if ((diff1 < 0d && diff2 > 0d) || (diff1 > 0d && diff2 < 0d) || diff1 == 0d || diff2 == 0d)
				intersect(Grid[row, col], Grid[row + 1, col + 1], toAdd);

			// upward slope next
			diff1 = Calc.LinePointDifference(point1, new VectorD(ul.X, ul.Y + NodeSpacing.Y), new VectorD(ul.X + NodeSpacing.X, ul.Y), crossEdgeSlopeUp);
			diff2 = Calc.LinePointDifference(point2, new VectorD(ul.X, ul.Y + NodeSpacing.Y), new VectorD(ul.X + NodeSpacing.X, ul.Y), crossEdgeSlopeUp);
			if ((diff1 < 0d && diff2 > 0d) || (diff1 > 0d && diff2 < 0d) || diff1 == 0d || diff2 == 0d)
				intersect(Grid[row + 1, col], Grid[row, col + 1], toAdd);
		}

		/// <summary>Adds the specified number of intersections to the PathEdges between the two provided nodes.</summary>
		/// <param name="node1">The first node.</param>
		/// <param name="node2">The second node.</param>
		/// <param name="toAdd">The number of intersections to add.</param>
		private void intersect(PathNode node1, PathNode node2, int toAdd)
		{
			foreach (PathEdge edge in node1)
			{
				if (edge.NodeDest == node2.Index)
				{
					edge.NumIntersections += toAdd;
					break;
				}
			}

			foreach (PathEdge edge in node2)
			{
				if (edge.NodeDest == node1.Index)
				{
					edge.NumIntersections += toAdd;
					break;
				}
			}
		}



		public PathEdge[] search(PathNode src, PathNode dest)
		{
			int destIndex = (dest == null) ? -1 : dest.Index;

			double[] costs = new double[Nodes.Length];
			double[] heur = new double[Nodes.Length];
			MinPQCost queue = new MinPQCost(costs, heur);
			queue.Enqueue(src.Index);

			PathEdge[] frontier = new PathEdge[Nodes.Length];
			PathEdge[] shortest = new PathEdge[Nodes.Length];

			while (queue.Count > 0)
			{
				int nextClosest = queue.Dequeue();

				shortest[nextClosest] = frontier[nextClosest];

				if (nextClosest == destIndex)
					break;

				foreach (PathEdge edge in Nodes[nextClosest])
				{
					if (edge.NumIntersections != 0)
						continue;
#if DEBUG
					if (edge.NumIntersections < 0)
						throw new Exception("NumIntersections should not be less than 0.");
#endif
					double newCost = costs[nextClosest] + edge.Distance;

					if (frontier[edge.NodeDest] == null)
					{
						costs[edge.NodeDest] = newCost;
						heur[edge.NodeDest] = VectorD.Distance(Nodes[edge.NodeDest].Position, dest.Position);
						queue.Enqueue(edge.NodeDest);
						frontier[edge.NodeDest] = edge;
					}
					else if (newCost < costs[edge.NodeDest] && shortest[edge.NodeDest] == null)
					{
						costs[edge.NodeDest] = newCost;
						queue.costShrunk(edge.NodeDest); // update position in queue
						frontier[edge.NodeDest] = edge;
					}
				}
			}

			return shortest;
		}

		/// <summary>Starting with the src node, searches for the shortest path to all of the specified destinations.</summary>
		/// <param name="src">The node to start from.</param>
		/// <param name="dest">An array of nodes to search for.</param>
		/// <param name="index">The index of each of the provided nodes in the returned PathEdge array.</param>
		/// <returns>The shortest path to all of the specified destinations.</returns>
		public PathEdge[] search(PathNode src, int[] dest, out int[] index)
		{
			int numFound = 0;
			int stepsSinceLast = -1;

			index = new int[dest.Length];
			for (int i = 0; i < index.Length; i++)
			{
				index[i] = -1;
			}

			double[] costs = new double[Nodes.Length];
			MinPQCost queue = new MinPQCost(costs, null);
			queue.Enqueue(src.Index);

			PathEdge[] frontier = new PathEdge[Nodes.Length];
			PathEdge[] shortest = new PathEdge[Nodes.Length];

			// TODO: specify threshold
			while (queue.Count > 0 && (numFound == 0 || stepsSinceLast < 100))
			{
				int nextClosest = queue.Dequeue();

				shortest[nextClosest] = frontier[nextClosest];

				for (int i = 0; i < dest.Length; i++)
				{
					if (nextClosest == dest[i])
					{
						numFound++;
						stepsSinceLast = 0;
						index[i] = nextClosest;
						if (numFound == dest.Length)
							break;
					}
				}

				if (numFound < dest.Length)
				{
					foreach (PathEdge edge in Nodes[nextClosest])
					{
						if (edge.NumIntersections != 0)
							continue;
#if DEBUG
						if (edge.NumIntersections < 0)
							throw new Exception("NumIntersections should not be less than 0.");
#endif
						double newCost = costs[nextClosest] + edge.Distance;

						if (frontier[edge.NodeDest] == null)
						{
							costs[edge.NodeDest] = newCost;
							queue.Enqueue(edge.NodeDest);
							frontier[edge.NodeDest] = edge;
						}
						else if (newCost < costs[edge.NodeDest] && shortest[edge.NodeDest] == null)
						{
							costs[edge.NodeDest] = newCost;
							queue.costShrunk(edge.NodeDest); // update position in queue
							frontier[edge.NodeDest] = edge;
						}
					}
				}

				stepsSinceLast++;
			}

			return shortest;
		}

		/// <summary>Creates a shallow copy of the PathFinder.</summary>
		/// <returns>A shallow copy of the PathFinder.</returns>
		public PathFinder Clone()
		{
			PathNode[,] grid = new PathNode[NumRows, NumCols];

			for (int row = 0; row < NumRows; row++)
			{
				for (int col = 0; col < NumCols; col++)
				{
					grid[row, col] = Grid[row, col].Clone();
				}
			}

			PathFinder clone = new PathFinder(grid, InWorld);

			return clone;
		}


		public List<PathEdge> gridToList(PathEdge[] edgeGrid, PathNode srcNode, PathNode destNode)
		{
			List<PathEdge> edges = new List<PathEdge>();

			while (destNode != srcNode)
			{
				if (edgeGrid[destNode.Index] == null)
					return null;

				edges.Add(edgeGrid[destNode.Index]);
				destNode = Nodes[edgeGrid[destNode.Index].NodeSrc];
			}

			edges.Reverse();

			return edges;
		}

		#endregion Methods
	}
}
