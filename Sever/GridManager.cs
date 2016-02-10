using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class GridManager
	{
		#region Members

		/// <summary>Describes a method to perform an operation on a grid square</summary>
		/// <param name="square">The grid square to operate on.</param>
		/// <param name="data">Any data associated with the operation</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		public delegate void gridOperation(GridSqr square, object data, TimeSpan time, int opID);

		/// <summary>The world that contains this grid.</summary>
		public World InWorld { get; set; }

		/// <summary>The grid squares to use.</summary>
		private GridSqr[,] squares;

		/// <summary>The grid squares to use.</summary>
		public GridSqr[,] Squares
		{
			get { return squares; }
			private set
			{
				squares = value;

				if (squares != null)
				{
					NumRows = squares.GetLength(0);
					NumCols = squares.GetLength(1);
				}
				else
				{
					NumRows = 0;
					NumCols = 0;
				}
			}
		}

		/// <summary>The number of columns in the grid.</summary>
		public int NumCols { get; private set; }

		/// <summary>The number of rows in the grid.</summary>
		public int NumRows { get; private set; }

		/// <summary>The size of each grid square.</summary>
		private VectorF sqrSize;

		/// <summary>The size of each grid square.</summary>
		public VectorF SqrSize { get { return sqrSize; } set { sqrSize = value; } }

		/// <summary>The width of each grid square.</summary>
		public FInt SqrWidth { get { return sqrSize.X; } set { sqrSize.X = value; } }

		/// <summary>The height of each grid square.</summary>
		public FInt SqrHeight { get { return sqrSize.Y; } set { sqrSize.Y = value; } }

		/// <summary>Whether or not to cancel the current operation</summary>
		public bool CancelOperation { get; set; }

		/// <summary>The current game time.</summary>
		public TimeSpan CurTime { get; set; }

		/// <summary>The current operation ID.</summary>
		public int OperationID { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of GridMananger.</summary>
		/// <param name="numCols">The number of rows in the grid.</param>
		/// <param name="numRows">The number of columns in the grid.</param>
		/// <param name="inWorld">The world that contains this grid.</param>
		/// <param name="initSquares">Whether or not to initialize the grid squares.</param>
		private GridManager(int numCols, int numRows, World inWorld, bool initSquares)
		{
			clear();

			InWorld = inWorld;
			Squares = new GridSqr[numRows, numCols];
			SqrWidth = inWorld.Width / (FInt)numCols;
			SqrHeight = inWorld.Height / (FInt)numRows;

			if (initSquares)
			{
				for (int row = 0; row < NumRows; row++)
				{
					for (int col = 0; col < NumCols; col++)
					{
						Squares[row, col] = new GridSqr();
					}
				}
			}
		}

		/// <summary>Creates a new instance of GridMananger.</summary>
		/// <param name="numCols">The number of rows in the grid.</param>
		/// <param name="numRows">The number of columns in the grid.</param>
		/// <param name="inWorld">The world that contains this grid.</param>
		public GridManager(int numCols, int numRows, World inWorld)
			: this(numCols, numRows, inWorld, true) { }

		#endregion Constructors

		#region Methods

		/// <summary>Resets all members to their defaults.</summary>
		public void clear()
		{
			InWorld = null;
			Squares = null;
			SqrSize = VectorF.Zero;
			CancelOperation = false;
			CurTime = TimeSpan.MinValue;
			OperationID = 0;
		}

		/// <summary>Gets a skeleton version of this grid manager.</summary>
		/// <param name="forPlayer">The player that will use it.  His nodes and segments are full instead of skeletons.</param>
		/// <param name="playerNodes">A list of all of the player's nodes.</param>
		/// <param name="playerSegs">A list of all of the player's segments.</param>
		/// <param name="allSegs">A list of all segments.</param>
		/// <param name="allNodes">A list of all nodes.</param>
		/// <returns>A skeleton version of this grid manager.</returns>
		public GridManager getSkeleton(Player forPlayer, out Node[] playerNodes, out Segment[] playerSegs, out NodeSkel[] allNodes, out SegmentSkel[] allSegs)
		{
			playerNodes = new Node[forPlayer.Nodes.Count];
			playerSegs = new Segment[forPlayer.Segments.Count];
			allNodes = new NodeSkel[InWorld.Nodes.Count];
			allSegs = new SegmentSkel[InWorld.Segments.Count];
			int nextPlyrNode = 0;
			int nextPlyrSeg = 0;
			int nextAllNode = 0;
			int nextAllSeg = 0;

			GridManager skeleton = new GridManager(NumCols, NumRows, InWorld, false);
			Dictionary<string, NodeSkel> nodes = new Dictionary<string, NodeSkel>(InWorld.Nodes.Count);
			Dictionary<string, SegmentSkel> segments = new Dictionary<string, SegmentSkel>(InWorld.Segments.Count);
			Dictionary<string, GeoSkel> geos = new Dictionary<string, GeoSkel>(InWorld.Geos.Count);

			// add nodes, segments, and geos to dictionary

			foreach (Node node in InWorld.Nodes)
			{
				if (node.Owner == forPlayer)
				{
					Node clone = new Node(node);
					nodes.Add(clone.ID, clone);
					playerNodes[nextPlyrNode] = clone;
					nextPlyrNode++;
					allNodes[nextAllNode] = clone;
				}
				else if (!node.Destroyed)
				{
					NodeSkel skel = node.getSkeleton();
					nodes.Add(node.ID, skel);
					allNodes[nextAllNode] = skel;
				}

				nextAllNode++;
			}

			foreach (Segment seg in InWorld.Segments)
			{
				if (seg.Owner == forPlayer)
				{
					Segment clone = new Segment(seg);
					segments.Add(clone.ID, clone);
					playerSegs[nextPlyrSeg] = clone;
					nextPlyrSeg++;
					allSegs[nextAllSeg] = clone;
					nextAllSeg++;
				}
				else if (!seg.Destroyed)
				{
					SegmentSkel skel = seg.getSkeleton();
					segments.Add(seg.ID, skel);
					allSegs[nextAllSeg] = skel;
					nextAllSeg++;
				}
			}

			foreach (Geo geo in InWorld.Geos)
			{
				geos.Add(geo.ID, geo.getSkeleton());
			}

			// connect cloned nodes and segments to each other

			foreach (Node node in playerNodes)
			{
				for (int i = 0; i < node.Segments.Length; i++)
				{
					Segment seg = node.Segments[i];
					if (seg != null)
						node.Segments[i] = (Segment)segments[seg.ID];
				}

				for (int i = 0; i < node.Parents.Count; i++)
				{
					Node n = node.Parents[i];
					node.Parents[i] = (Node)nodes[n.ID];
				}
			}

			foreach (Segment seg in playerSegs)
			{
				for (int i = 0; i < seg.Nodes.Length; i++)
				{
					Node node = seg.Nodes[i];

					if (!node.Destroyed)
						seg.Nodes[i] = (Node)nodes[node.ID];
					else
						seg.Nodes[i] = new Node(node);
				}
			}

			// build grid

			skeleton.SqrSize = this.SqrSize;

			for (int row = 0; row < NumRows; row++)
			{
				for (int col = 0; col < NumRows; col++)
				{
					GridSqr sqr = Squares[row, col];
					skeleton.Squares[row, col] = new GridSqr(new List<NodeSkel>(sqr.Nodes.Count), new List<SegmentSkel>(sqr.Segments.Count), new List<GeoSkel>(sqr.Geos.Count), new List<Hotspot>(sqr.Hotspots.Count), false);

					foreach (NodeSkel node in sqr.Nodes)
					{
						if (((Node)node).Active && !((Node)node).Destroyed)
							skeleton.Squares[row, col].Nodes.Add(nodes[node.ID]);
					}

					foreach (SegmentSkel seg in sqr.Segments)
					{
						if (!((Segment)seg).Destroyed)
							skeleton.Squares[row, col].Segments.Add(segments[seg.ID]);
					}

					foreach (GeoSkel geo in sqr.Geos)
					{
						skeleton.Squares[row, col].Geos.Add(geos[geo.ID]);
					}
				}
			}

			return skeleton;
		}

		/// <summary>Starts a new update cycle.  Sets the current time and resets the operation id.</summary>
		/// <param name="curTime">The current game time.</param>
		public void startNewUpdate(GameTime curTime)
		{
			CurTime = curTime.TotalGameTime;
			OperationID = 0;
		}

		/// <summary>Runs an operation on a single grid square.</summary>
		/// <param name="point1">The point in the grid.</param>
		/// <param name="data">Any data associated with the operation</param>
		/// <param name="action">The operation to run</param>
		public void Point(VectorF point, object data, gridOperation action)
		{
			int col = (int)(point.X / SqrSize.X);
			int row = (int)(point.Y / SqrSize.Y);

			if (col >= 0 && row >= 0 && col < NumCols && row < NumRows)
			{
				CancelOperation = false;
				action(Squares[row, col], data, CurTime, OperationID);
				OperationID++;
			}
		}

		/// <summary>Runs an operation on each grid square in a line</summary>
		/// <param name="point1">The first point of the line</param>
		/// <param name="point2">The second point of the line</param>
		/// <param name="data">Any data associated with the operation</param>
		/// <param name="action">The operation to run</param>
		public void Line(VectorF point1, VectorF point2, object data, gridOperation action)
		{	
			// find first square
			int col = (int)(point1.X / SqrSize.X);
			int row = (int)(point1.Y / SqrSize.Y);

			// find last square
			int fCol = (int)(point2.X / SqrSize.X);
			int fRow = (int)(point2.Y / SqrSize.Y);

			// if outside boundaries...
			if (fCol < 0 || fRow < 0 || col >= NumCols || row >= NumRows)
				return;

			// correct values
			col = Math.Max(col, 0);
			row = Math.Max(row, 0);
			col = Math.Min(col, NumCols - 1);
			row = Math.Min(row, NumRows - 1);

			fCol = Math.Max(fCol, 0);
			fRow = Math.Max(fRow, 0);
			fCol = Math.Min(fCol, NumCols - 1);
			fRow = Math.Min(fRow, NumRows - 1);

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

			// if both points are in the same column
			if (col == fCol)
			{
				if (down)
					Rect(col, row, col, fRow, data, action);
				else
					Rect(col, fRow, col, row, data, action);
				return;
			}

			// if both points are in the same row
			if (row == fRow)
			{
				if (right)
					Rect(col, row, fCol, row, data, action);
				else
					Rect(fCol, row, col, row, data, action);
				return;
			}

			// get the slope
			FInt slope = (point2.Y - point1.Y) / ((point2.X == point1.X) ? FInt.F0 : (point2.X - point1.X));

			// run the line
			CancelOperation = false;
			while (!CancelOperation && (col != fCol || row != fRow))
			{
				action(Squares[row, col], data, CurTime, OperationID);

				FInt hCost = Calc.Abs(((col + colo) * SqrSize.X - point1.X) * slope); // the cost of moving left or right
				FInt vCost = Calc.Abs((row + rowo) * SqrSize.Y - point1.Y); // the cost of moving up or down

				if (hCost == vCost)
				{
					if (col != fCol)
						col += cold;
					if (row != rowd)
						row += rowd;
				}
				else if (hCost < vCost)
				{
					col += cold; // move left or right
				}
				else
				{
					row += rowd; // move up or down
				}
			}

			// run operation on last grid square
			if (!CancelOperation)
				action(Squares[fRow, fCol], data, CurTime, OperationID);

			OperationID++;
		}

		/// <summary>Performs an operation on a rectangle of grid squares.</summary>
		/// <param name="col1">The left-most column</param>
		/// <param name="row1">The top-most row</param>
		/// <param name="col2">The right-most column</param>
		/// <param name="row2">The bottom row</param>
		/// <param name="data">Any data associated with the operation</param>
		/// <param name="action">The operation to perform on the grid</param>
		public void Rect(int col1, int row1, int col2, int row2, object data, gridOperation action)
		{
			// check range
			col1 = Math.Max(col1, 0);
			row1 = Math.Max(row1, 0);
			col2 = Math.Min(col2, NumCols - 1);
			row2 = Math.Min(row2, NumRows - 1);

			// run tests
			CancelOperation = false;
			for (int col; row1 <= row2 && !CancelOperation; row1++)
			{
				for (col = col1; col <= col2 && !CancelOperation; col++)
				{
					action(Squares[row1, col], data, CurTime, OperationID);
				}
			}

			OperationID++;
		}

		/// <summary>Performs an operation on a rectangle of grid squares.</summary>
		/// <param name="upperLeft">The upper-left corner of the rectangle in world-coordinates.</param>
		/// <param name="lowerRight">The lower-right corner of the rectangle in world coordinates.</param>
		/// <param name="data">Any data associated with the operation</param>
		/// <param name="action">The operation to perform on the grid</param>
		public void Rect(VectorF upperLeft, VectorF lowerRight, object data, gridOperation action)
		{
			int col1 = (int)(upperLeft.X / SqrWidth);
			int row1 = (int)(upperLeft.Y / SqrHeight);
			int col2 = (int)(lowerRight.X / SqrWidth);
			int row2 = (int)(lowerRight.Y / SqrHeight);

			Rect(col1, row1, col2, row2, data, action);
		}

		/// <summary>Performs an operation on a rectangle of grid squares and the squares areound them.</summary>
		/// <param name="upperLeft">The upper-left corner of the rectangle in world-coordinates.</param>
		/// <param name="lowerRight">The lower-right corner of the rectangle in world coordinates.</param>
		/// <param name="data">Any data associated with the operation</param>
		/// <param name="action">The operation to perform on the grid</param>
		public void RectExpand(VectorF upperLeft, VectorF lowerRight, object data, gridOperation action)
		{
			int col1 = (int)(upperLeft.X / SqrWidth) - 1;
			int row1 = (int)(upperLeft.Y / SqrHeight) - 1;
			int col2 = (int)(lowerRight.X / SqrWidth) + 1;
			int row2 = (int)(lowerRight.Y / SqrHeight) + 1;

			Rect(col1, row1, col2, row2, data, action);
		}

		/// <summary>Runs an operation on each gridsquare in a line and the gridsquares around them</summary>
		/// <param name="point1">The first point of the line</param>
		/// <param name="point2">The second point of the line</param>
		/// <param name="data">Any data associated with the operation</param>
		/// <param name="action">The operation to run</param>
		public void LineExpand(VectorF point1, VectorF point2, object data, gridOperation action)
		{
			// draw expanded line
			Line(point1, point2, null, ActivatePoint);

			// find corners
			int col1, row1, col2, row2;
			if (point1.X < point2.X)
			{
				col1 = (int)(point1.X / SqrSize.X);
				col2 = (int)(point2.X / SqrSize.X);
			}
			else
			{
				col2 = (int)(point1.X / SqrSize.X);
				col1 = (int)(point2.X / SqrSize.X);
			}
			if (point1.Y < point2.Y)
			{
				row1 = (int)(point1.Y / SqrSize.Y);
				row2 = (int)(point2.Y / SqrSize.Y);
			}
			else
			{
				row2 = (int)(point1.Y / SqrSize.Y);
				row1 = (int)(point2.Y / SqrSize.Y);
			}
			col1 -= (col1 != 0) ? 1 : 0;
			row1 -= (row1 != 0) ? 1 : 0;
			col2 += (col2 != NumCols - 1) ? 1 : 0;
			row2 += (row2 != NumRows - 1) ? 1 : 0;

			// run operation
			for (int col = col1, row; col <= col2; col++)
			{
				for (row = row1; row <= row2; row++)
				{
					if (Squares[row, col].Active
						|| (col > 0 && (Squares[row, col - 1].Active || (row > 0 && Squares[row - 1, col - 1].Active)))
						|| (row > 0 && Squares[row - 1, col].Active)
						|| (col < NumCols - 1 && (Squares[row, col + 1].Active || (row < NumRows - 1 && Squares[row + 1, col + 1].Active)))
						|| (row < NumRows - 1 && Squares[row + 1, col].Active))
					{
						if (!CancelOperation) action(Squares[row, col], data, CurTime, OperationID);
						Squares[row, col].Active = false;
					}
				}
			}

			OperationID++;
		}

		/// <summary>Performs an operation on a point's grid square and those surrounding it</summary>
		/// <param name="point">The point</param>
		/// <param name="data">Any data associated with the operation</param>
		/// <param name="action">The operation to run</param>
		public void PointExpand(VectorF point, object data, gridOperation action)
		{
			int col = (int)(point.X / SqrSize.X);
			int row = (int)(point.Y / SqrSize.Y);

			if (col < 0 || row < 0 || col >= NumCols || row >= NumRows)
				return;

			int endCol = (col < NumCols - 1) ? col + 2 : NumCols;
			int endRow = (row < NumRows - 1) ? row + 2 : NumRows;
			int begRow = (row != 0) ? (row - 1) : row;

			CancelOperation = false;
			for (col = (col != 0) ? (col - 1) : col; col < endCol && !CancelOperation; col++)
			{
				for (row = begRow; row < endRow && !CancelOperation; row++)
				{
					action(Squares[row, col], data, CurTime, OperationID);
				}
			}

			OperationID++;
		}

		/// <summary>Activates a single square on the grid</summary>
		/// <param name="sqr">The square to activate.</param>
		/// <param name="data">null</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void ActivatePoint(GridSqr sqr, object data, TimeSpan time, int opID)
		{
			sqr.Active = true;
		}

		#endregion Methods
	}
}
