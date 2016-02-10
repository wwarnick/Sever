using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class FogOfWar
	{
		#region Members

		/// <summary>The various statuses that each cell could be.</summary>
		public enum VisOption { Visible, Explored, Unexplored }

		/// <summary>The player that owns this fog of war.</summary>
		public Player Owner { get; set; }

		/// <summary>The visibility grid.</summary>
		private VisOption[,] grid;

		/// <summary>The visibility grid.</summary>
		public VisOption[,] Grid
		{
			get { return grid; }
			private set
			{
				grid = value;

				if (grid != null)
				{
					NumRows = value.GetLength(0);
					NumCols = value.GetLength(1);
				}
				else
				{
					NumRows = 0;
					NumCols = 0;
				}
			}
		}

		/// <summary>Specifies which tiles have been drawn.</summary>
		private bool[,] drawn;

		/// <summary>The number of rows in the grid.</summary>
		public int NumRows { get; private set; }

		/// <summary>The number of columns in the grid.</summary>
		public int NumCols { get; private set; }

		/// <summary>The size of each grid square.</summary>
		private VectorF sqrSize;

		/// <summary>The size of each grid square.</summary>
		public VectorF SqrSize { get { return sqrSize; } set { sqrSize = value; } }

		/// <summary>The width of each grid square.</summary>
		public FInt SqrWidth { get { return sqrSize.X; } set { sqrSize.X = value; } }

		/// <summary>The height of each grid square.</summary>
		public FInt SqrHeight { get { return sqrSize.Y; } set { sqrSize.Y = value; } }

		/// <summary>Half of the square size.</summary>
		private VectorF SqrSizeHalf { get; set; }

		/// <summary>The origin of the sprite.</summary>
		private readonly Vector2 spriteOrigin = new Vector2(.5f, .5f);

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of FogOfWar.</summary>
		/// <param name="rows">The number of rows.</param>
		/// <param name="cols">The number of columns.</param>
		/// <param name="worldSize">The size of the world.</param>
		/// <param name="owner">The player that owns this fog of war.</param>
		public FogOfWar(int rows, int cols, VectorF worldSize, Player owner)
		{
			clear();

			Owner = owner;
			Grid = new VisOption[rows, cols];

			// set grid square size
			SqrSize = new VectorF(worldSize.X / (FInt)cols, worldSize.Y / (FInt)rows);
			SqrSizeHalf = SqrSize / FInt.F2;

			// begin unexplored
			resetTo(VisOption.Unexplored);

			// only use drawn if human
			drawn = (owner.Type == Player.PlayerType.Human) ? new bool[rows, cols] : null;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public void clear()
		{
			Owner = null;
			Grid = null;
			SqrSize = VectorF.Zero;
			SqrSizeHalf = VectorF.Zero;
		}

		/// <summary>Resets all values in the grid to the specified visibliity option.</summary>
		/// <param name="visibility">The visibility option to set.</param>
		public void resetTo(VisOption visibility)
		{
			for (int row = 0; row < NumRows; row++)
			{
				for (int col = 0; col < NumCols; col++)
				{
					Grid[row, col] = VisOption.Unexplored;
				}
			}
		}

		/// <summary>Applies the visibility for a node.</summary>
		/// <param name="node">The node to apply.</param>
		public void applyVisibility(Node node)
		{
			if (!node.Active || node.Owner != Owner)
				return;

			int leftCol = Math.Max(0, (int)((node.X - node.SightDistance) / SqrWidth));
			int topRow = Math.Max(0, (int)((node.Y - node.SightDistance) / SqrHeight));
			int rightCol = Math.Min(NumCols - 1, (int)Calc.RoundUp((node.X + node.SightDistance) / SqrWidth));
			int bottomRow = Math.Min(NumRows - 1, (int)Calc.RoundUp((node.Y + node.SightDistance) / SqrHeight));

			for (int col = leftCol; col <= rightCol; col++)
			{
				for (int row = topRow; row <= bottomRow; row++)
				{
					// don't do the math again if it's already visible
					if (Grid[row, col] == VisOption.Visible)
						continue;

					// find closest point in the square
					VectorF testPoint = VectorF.Zero;
					VectorF topLeft = new VectorF(SqrWidth * (FInt)col, SqrHeight * (FInt)row);
					VectorF bottomRight = topLeft + SqrSize;

					if (topLeft.X >= node.X)
						testPoint.X = topLeft.X;
					else if (bottomRight.X <= node.X)
						testPoint.X = bottomRight.X;
					else
						testPoint.X = node.X;

					if (topLeft.Y >= node.Y)
						testPoint.Y = topLeft.Y;
					else if (bottomRight.Y <= node.Y)
						testPoint.Y = bottomRight.Y;
					else
						testPoint.Y = node.Y;

					// test the point
					if (VectorF.Distance(testPoint, node.Pos) <= node.SightDistance)
						Grid[row, col] = VisOption.Visible;
				}
			}
		}

		/// <summary>Applies the visibility for a list of nodes.</summary>
		/// <param name="nodes">The list of nodes.</param>
		public void applyVisibility(IEnumerable<Node> nodes)
		{
			foreach(Node node in nodes)
			{
				applyVisibility(node);
			}
		}

		//// <summary>Determines whether or not the provided node is visible.</summary>
		/// <param name="node">The node to test.</param>
		/// <returns>Whether or not the provided node is visible.</returns>
		public bool isVisible(Node node)
		{
			if (node.Owner == Owner)
				return true;

			return isVisible((NodeSkel)node);
		}

		/// <summary>Determines whether or not the provided node is visible.</summary>
		/// <param name="node">The node to test.</param>
		/// <returns>Whether or not the provided node is visible.</returns>
		public bool isVisible(NodeSkel node)
		{
			if (node.Owner == Owner)
				return true;

			int topRow = Math.Max(0, (int)((node.Y - node.Radius) / SqrHeight));
			int leftCol = Math.Max(0, (int)((node.X - node.Radius) / SqrWidth));
			int bottomRow = Math.Min(NumRows - 1, (int)Calc.RoundUp((node.Y + node.Radius) / SqrHeight));
			int rightCol = Math.Min(NumCols - 1, (int)Calc.RoundUp((node.X + node.Radius) / SqrWidth));

			for (int row = topRow; row <= bottomRow; row++)
			{
				for (int col = leftCol; col <= rightCol; col++)
				{
					if (Grid[row, col] != VisOption.Visible)
						continue;

					FInt left = (FInt)col * SqrWidth;
					FInt right = left + SqrWidth;
					FInt top = (FInt)row * SqrHeight;
					FInt bottom = top + SqrHeight;

					FInt closestY = (node.Y >= top && node.Y <= bottom) ? node.Y : (node.Y > bottom) ? bottom : top;
					FInt closestX = (node.X >= left && node.X <= right) ? node.X : (node.X > right) ? right : left;

					if (VectorF.Distance(node.Pos, new VectorF(closestX, closestY)) <= node.Radius)
						return true;
				}
			}

			return false;
		}

		/// <summary>Determines whether or not the provided segment is visible.</summary>
		/// <param name="seg">The segment to test.</param>
		/// <returns>Whether or not the provided segment is visible.</returns>
		public bool isVisible(Segment seg)
		{
			if (seg.Owner == Owner)
				return true;

			return isVisible((SegmentSkel)seg);
		}

		/// <summary>Determines whether or not the provided segment is visible.</summary>
		/// <param name="seg">The segment to test.</param>
		/// <returns>Whether or not the provided segment is visible.</returns>
		public bool isVisible(SegmentSkel seg)
		{
			return isVisible(seg.EndLoc[0], seg.EndLoc[1]);
		}

		/// <summary>Determines whether or not the provided line is visible.</summary>
		/// <param name="point1">The first point in the line.</param>
		/// <param name="point2">The second point in the line.</param>
		/// <returns>Whether or not the provided line is visible.</returns>
		private bool isVisible(VectorF point1, VectorF point2)
		{
			// find first square
			int col = (int)(point1.X / SqrSize.X);
			int row = (int)(point1.Y / SqrSize.Y);

			// find last square
			int fCol = (int)(point2.X / SqrSize.X);
			int fRow = (int)(point2.Y / SqrSize.Y);

			// if outside boundaries...
			if (fCol < 0 || fRow < 0 || col >= NumCols || row >= NumRows)
				return false;

			// correct values
			col = Math.Min(Math.Max(col, 0), NumCols - 1);
			row = Math.Min(Math.Max(row, 0), NumRows - 1);
			fCol = Math.Min(Math.Max(fCol, 0), NumCols - 1);
			fRow = Math.Min(Math.Max(fRow, 0), NumRows - 1);

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
			FInt slope = (point2.X == point1.X) ? FInt.F0 : ((point2.Y - point1.Y) / (point2.X - point1.X));

			// run the line
			while (col != fCol || row != fRow)
			{
				if (Grid[row, col] == VisOption.Visible)
					return true;

				FInt hCost = Calc.Abs(((col + colo) * SqrSize.X - point1.X) * slope); // the cost of moving left or right
				FInt vCost = Calc.Abs((row + rowo) * SqrSize.Y - point1.Y); // the cost of moving up or down

				if (hCost == vCost)
				{
					if (col != fCol)
						col += cold;
					if (row != fRow)
						row += rowd;
				}
				else if (col != fCol && (hCost < vCost || row == fRow))
				{
					col += cold; // move left or right
				}
				else
				{
					row += rowd; // move up or down
				}
			}

			// test last grid square
			return Grid[fRow, fCol] == VisOption.Visible;
		}

		/// <summary>Sets the visibility for each line in the geo.</summary>
		/// <param name="geo">The geo to test.</param>
		public void setLineVisibility(Geo geo)
		{
			if (geo.Vertices.Length > 1)
			{
				for (int i = 0; i < geo.Vertices.Length; i += 2)
				{
					geo.LineVisible[i / 2] = isVisible(geo.Vertices[i], geo.Vertices[i + 1]);
				}

				if (geo.CloseLoop && geo.Vertices.Length > 2)
					geo.LineVisible[geo.LineVisible.Length - 1] = isVisible(geo.Vertices[geo.Vertices.Length - 1], geo.Vertices[0]);
			}
		}

		/// <summary>Refreshes the visibility for a certain region of the map.</summary>
		/// <param name="upperLeft">The upper-left corner of the region to invalidate.</param>
		/// <param name="lowerRight">The lower-right corner of the region to invalidate.</param>
		public void invalidate(VectorF upperLeft, VectorF lowerRight)
		{
			// change all visible cells to explored
			int leftCol = Math.Max(0, (int)(upperLeft.X / SqrWidth));
			int topRow = Math.Max(0, (int)(upperLeft.Y / SqrHeight));
			int rightCol = Math.Min(NumCols - 1, (int)Calc.RoundUp(lowerRight.X / SqrWidth));
			int bottomRow = Math.Min(NumRows - 1, (int)Calc.RoundUp(lowerRight.Y / SqrHeight));

			for (int row = topRow; row <= bottomRow; row++)
			{
				for (int col = leftCol; col <= rightCol; col++)
				{
					if (Grid[row, col] == VisOption.Visible)
						Grid[row, col] = VisOption.Explored;
				}
			}

			// apply visibility of nearby nodes
			Owner.InWorld.Grid.RectExpand(upperLeft, lowerRight, null, gridInvalidate);
		}

		/// <summary>Applies the visibility of all nodes in the provided grid square.</summary>
		/// <param name="sqr">The square to apply.</param>
		/// <param name="array"></param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridInvalidate(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			foreach (Node node in sqr.Nodes)
			{
				if (node.LastCheck != time || node.LastCheckNum != opID)
				{
					applyVisibility(node);

					node.LastCheck = time;
					node.LastCheckNum = opID;
				}
			}
		}

		/// <summary>Draws the fog of war.</summary>
		/// <param name="batch">The sprite batch to use.</param>
		/// <param name="tex">The texture to use.</param>
		public void draw(SpriteBatch batch, Texture2D tex)
		{
			Camera Cam = Owner.InWorld.Cam;

			// get the bounds of the grid to draw
			int left = Math.Min(Math.Max(0, (int)(Cam.Left / SqrWidth)), NumCols - 1);
			int top = Math.Min(Math.Max(0, (int)(Cam.Top / SqrHeight)), NumRows - 1);
			int right = Math.Min(Math.Max(0, (int)Calc.RoundUp(Cam.Right / SqrWidth)), NumCols - 1);
			int bottom = Math.Min(Math.Max(0, (int)Calc.RoundUp(Cam.Bottom / SqrHeight)), NumRows - 1);

			// initialize drawn to false
			for (int row = top; row <= bottom; row++)
			{
				for (int col = left; col <= right; col++)
				{
					drawn[row, col] = false;
				}
			}

			for (int row = top; row <= bottom; row++)
			{
				for (int col = left; col <= right; col++)
				{
					if (drawn[row, col] || Grid[row, col] == VisOption.Visible)
						continue;

					Color color = new Color(.075f, .075f, .075f, (Grid[row, col] == FogOfWar.VisOption.Unexplored) ? .75f : .5f);//.75f : .5f);

					int width = 1;
					int height = 1;

					VisOption curVis = Grid[row, col];

					// expand square as far as possible
					for (int i = 1; row + i <= bottom && col + i <= right; i++)
					{
						if (drawn[row + i, col + i] || Grid[row + i, col + i] != curVis)
							break;

						bool toBreak = false;
						for (int j = 0; j < i; j++)
						{
							if (drawn[row + i, col + j] || drawn[row + j, col + i] || Grid[row + i, col + j] != curVis || Grid[row + j, col + i] != curVis)
							{
								toBreak = true;
								break;
							}
						}

						if (toBreak)
							break;

						width++;
						height++;
					}

					// expand height
					for (int iRow = row + height; iRow <= bottom; iRow++)
					{
						bool toBreak = false;
						for (int iCol = col, iColMax = col + width; iCol < iColMax; iCol++)
						{
							if (drawn[iRow, iCol] || Grid[iRow, iCol] != curVis)
							{
								toBreak = true;
								break;
							}
						}
						if (toBreak)
							break;
						height++;
					}

					// expand width
					for (int iCol = col + width; iCol <= right; iCol++)
					{
						bool toBreak = false;
						for (int iRow = row, iRowMax = row + height; iRow < iRowMax; iRow++)
						{
							if (drawn[iRow, iCol] || Grid[iRow, iCol] != curVis)
							{
								toBreak = true;
								break;
							}
						}
						if (toBreak)
							break;
						width++;
					}

					// mark as drawn
					for (int iRow = row, iRowMax = row + height; iRow < iRowMax; iRow++)
					{
						for (int iCol = col, iColMax = col + width; iCol < iColMax; iCol++)
						{
							drawn[iRow, iCol] = true;
						}
					}

					batch.Draw(tex, Cam.worldToScreenDraw(new VectorF((FInt)col * SqrWidth, (FInt)row * SqrHeight) + new VectorF(SqrSizeHalf.X * (FInt)width, SqrSizeHalf.Y * (FInt)height)), null, color, 0f, spriteOrigin, new Vector2((float)SqrSize.X * (float)width, (float)SqrSize.Y * (float)height), SpriteEffects.None, 0f);

					col += width - 1;
				}
			}
		}

		#endregion Methods
	}
}
