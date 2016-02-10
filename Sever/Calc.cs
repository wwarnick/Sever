using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public static class Calc
	{
		/// <summary>Rounds the decimal (if there is one) up</summary>
		/// <param name="num">The number to round up</param>
		/// <returns>The supplied number rounded up</returns>
		public static FInt RoundUp(FInt num)
		{
			FInt roundDown = (FInt)(int)num;
			return (roundDown != num)
				? roundDown + FInt.F1
				: num;
		}

		/// <summary>Rounds the decimal (if there is one) up</summary>
		/// <param name="num">The number to round up</param>
		/// <returns>The supplied number rounded up</returns>
		public static double RoundUp(double num)
		{
			double roundDown = (double)(int)num;
			return (roundDown != num)
				? roundDown + 1d
				: num;
		}

		/// <summary>Rounds the number to the nearest integer.</summary>
		/// <param name="num">The number to round.</param>
		/// <returns>The number rounded to the nearest integer.</returns>
		public static FInt Round(FInt num)
		{
			return (FInt)(int)(num + (FInt).5);
		}

		/// <summary>Rounds the number to the nearest integer.</summary>
		/// <param name="num">The number to round.</param>
		/// <returns>The number rounded to the nearest integer.</returns>
		public static float Round(float num)
		{
			return (float)(int)(num + .5f);
		}

		/// <summary>Determines whether the two specified lines intersect; requires less processing than LineIntersect</summary>
		/// <param name="a1">The first point of the first line</param>
		/// <param name="a2">The second point of the first line</param>
		/// <param name="b1">The first point of the second line</param>
		/// <param name="b2">The second point of the second line</param>
		/// <returns>Whether or not the two lines intersect</returns>
		public static bool DoLinesIntersect(VectorF a1, VectorF a2, VectorF b1, VectorF b2)
		{
			FInt xb2b1 = b2.X - b1.X;
			FInt ya1b1 = a1.Y - b1.Y;
			FInt yb2b1 = b2.Y - b1.Y;
			FInt xa1b1 = a1.X - b1.X;
			FInt xa2a1 = a2.X - a1.X;
			FInt ya2a1 = a2.Y - a1.Y;

			FInt divisor = (yb2b1 * xa2a1 - xb2b1 * ya2a1);

			if (divisor == 0)
				return true;

			FInt ua = (xb2b1 * ya1b1 - yb2b1 * xa1b1) /
				divisor;

			FInt ub = (xa2a1 * ya1b1 - ya2a1 * xa1b1) /
				divisor;

			return !(ua < FInt.F0 || ua > FInt.F1 || ub < FInt.F0 || ub > FInt.F1);
		}

		/// <summary>Determines whether the two specified lines intersect; requires less processing than LineIntersect</summary>
		/// <param name="a1">The first point of the first line</param>
		/// <param name="a2">The second point of the first line</param>
		/// <param name="b1">The first point of the second line</param>
		/// <param name="b2">The second point of the second line</param>
		/// <returns>Whether or not the two lines intersect</returns>
		public static bool DoLinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
		{
			float xb2b1 = b2.X - b1.X;
			float ya1b1 = a1.Y - b1.Y;
			float yb2b1 = b2.Y - b1.Y;
			float xa1b1 = a1.X - b1.X;
			float xa2a1 = a2.X - a1.X;
			float ya2a1 = a2.Y - a1.Y;

			float divisor = (yb2b1 * xa2a1 - xb2b1 * ya2a1);

			if (divisor == 0)
				return true;

			float ua = (xb2b1 * ya1b1 - yb2b1 * xa1b1) /
				divisor;

			float ub = (xa2a1 * ya1b1 - ya2a1 * xa1b1) /
				divisor;

			return !(ua < 0f || ua > 1f || ub < 0f || ub > 1f);
		}

		/// <summary>Finds where the two specified lines intersect; null if none</summary>
		/// <param name="a1">The first point of the first line</param>
		/// <param name="a2">The second point of the first line</param>
		/// <param name="b1">The first point of the second line</param>
		/// <param name="b2">The second point of the second line</param>
		/// <returns>The point of intersection; null if none</returns>
		public static VectorF? LineIntersectIfExists(VectorF a1, VectorF a2, VectorF b1, VectorF b2)
		{
			FInt xb2b1 = b2.X - b1.X;
			FInt ya1b1 = a1.Y - b1.Y;
			FInt yb2b1 = b2.Y - b1.Y;
			FInt xa1b1 = a1.X - b1.X;
			FInt xa2a1 = a2.X - a1.X;
			FInt ya2a1 = a2.Y - a1.Y;

			FInt divisor = (yb2b1 * xa2a1 - xb2b1 * ya2a1);

			// lines are on top of each other, find mid-point of overlap
			if (divisor == 0)
			{
				VectorF tempV = VectorF.Zero;
				// sort points by x ascending (if equal, then y)
				for (int i = 0; i < 3; i++)
				{
					if (a2.X < a1.X || (a2.X == a1.X && a2.Y < a1.Y))
					{
						tempV = a1;
						a1 = a2;
						a2 = tempV;
					}

					if (b1.X < a2.X || (b1.X == a2.X && b1.Y < a2.Y))
					{
						tempV = a2;
						a1 = b1;
						b1 = tempV;
					}

					if (b2.X < b1.X || (b2.X == b1.X && b2.Y < b1.Y))
					{
						tempV = b1;
						b1 = b2;
						b2 = tempV;
					}
				}

				return (a2 + b1) / FInt.F2;
			}

			// find intersection
			FInt ua = (xb2b1 * ya1b1 - yb2b1 * xa1b1) /
				divisor;

			FInt ub = (xa2a1 * ya1b1 - ya2a1 * xa1b1) /
				divisor;

			if (ua < FInt.F0 || ua > FInt.F1 || ub < FInt.F0 || ub > FInt.F1)
				return null;

			return new VectorF(a1.X + ua * (a2.X - a1.X), a1.Y + ua * (a2.Y - a1.Y));
		}

		/// <summary>Finds where the two specified lines intersect; assumes the fact that they intersect.</summary>
		/// <param name="a1">The first point of the first line</param>
		/// <param name="a2">The second point of the first line</param>
		/// <param name="b1">The first point of the second line</param>
		/// <param name="b2">The second point of the second line</param>
		/// <returns>The point of intersection.</returns>
		public static VectorF LineIntersect(VectorF a1, VectorF a2, VectorF b1, VectorF b2)
		{
			FInt divisor = ((b2.Y - b1.Y) * (a2.X - a1.X) - (b2.X - b1.X) * (a2.Y - a1.Y));

			// lines are on top of each other, find mid-point of overlap
			if (divisor == 0)
			{
				VectorF tempV = VectorF.Zero;
				// sort points by x ascending (if equal, then y)
				for (int i = 0; i < 3; i++)
				{
					if (a2.X < a1.X || (a2.X == a1.X && a2.Y < a1.Y))
					{
						tempV = a1;
						a1 = a2;
						a2 = tempV;
					}

					if (b1.X < a2.X || (b1.X == a2.X && b1.Y < a2.Y))
					{
						tempV = a2;
						a1 = b1;
						b1 = tempV;
					}

					if (b2.X < b1.X || (b2.X == b1.X && b2.Y < b1.Y))
					{
						tempV = b1;
						b1 = b2;
						b2 = tempV;
					}
				}

				return (a2 + b1) / FInt.F2;
			}

			// find intersection
			FInt ua = ((b2.X - b1.X) * (a1.Y - b1.Y) - (b2.Y - b1.Y) * (a1.X - b1.X)) /
				divisor;

			return new VectorF(a1.X + ua * (a2.X - a1.X), a1.Y + ua * (a2.Y - a1.Y));
		}

		/// <summary>Determines the distance between a line and a point at their closest point.</summary>
		/// <param name="point">The point</param>
		/// <param name="lp1">The first point in the line</param>
		/// <param name="lp2">The second point in the line</param>
		/// <returns>The distance between a line and a point at their closest point</returns>
		public static FInt LinePointDistance(VectorF point, VectorF lp1, VectorF lp2)
		{
			return LinePointDistance(point, lp1, lp2, VectorF.Distance(lp1, lp2));
		}

		/// <summary>Determines the distance between a line and a point at their closest point</summary>
		/// <param name="point">The point</param>
		/// <param name="lp1">The first point in the line</param>
		/// <param name="lp2">The second point in the line</param>
		/// <param name="length">The length of the line.  Provide so that the method doesn't have to calculate it again.</param>
		/// <returns>The distance between a line and a point at their closest point</returns>
		public static FInt LinePointDistance(VectorF point, VectorF lp1, VectorF lp2, FInt length)
		{
			return length == FInt.F0 ? VectorF.Distance(point, lp1) : VectorF.Distance(lp1 + ((length - VectorF.Distance(lp2, point) + VectorF.Distance(lp1, point)) / FInt.F2 / length) * (lp2 - lp1), point);
		}

		/// <summary>Determines the distance between a line and a point at their closest point.</summary>
		/// <param name="point">The point</param>
		/// <param name="lp1">The first point in the line</param>
		/// <param name="lp2">The second point in the line</param>
		/// <returns>The distance between a line and a point at their closest point</returns>
		public static double LinePointDistance(VectorD point, VectorD lp1, VectorD lp2)
		{
			return LinePointDistance(point, lp1, lp2, VectorD.Distance(lp1, lp2));
		}

		/// <summary>Determines the distance between a line and a point at their closest point</summary>
		/// <param name="point">The point</param>
		/// <param name="lp1">The first point in the line</param>
		/// <param name="lp2">The second point in the line</param>
		/// <param name="length">The length of the line.  Provide so that the method doesn't have to calculate it again.</param>
		/// <returns>The distance between a line and a point at their closest point</returns>
		public static double LinePointDistance(VectorD point, VectorD lp1, VectorD lp2, double length)
		{
			return length == 0d ? VectorD.Distance(point, lp1) : VectorD.Distance(lp1 + ((length - VectorD.Distance(lp2, point) + VectorD.Distance(lp1, point)) / 2d / length) * (lp2 - lp1), point);
		}

		/// <summary>Determines the approximate distance between a line and a point at their closest point.</summary>
		/// <param name="point">The point</param>
		/// <param name="lp1">The first point in the line</param>
		/// <param name="lp2">The second point in the line</param>
		/// <returns>The approximate distance between a line and a point at their closest point</returns>
		public static FInt LinePointDistApprox(VectorF point, VectorF lp1, VectorF lp2)
		{
			return LinePointDistApprox(point, lp1, lp2, VectorF.DistanceApprox(lp1, lp2));
		}

		/// <summary>Determines the approximate distance between a line and a point at their closest point</summary>
		/// <param name="point">The point</param>
		/// <param name="lp1">The first point in the line</param>
		/// <param name="lp2">The second point in the line</param>
		/// <param name="length">The length of the line.  Provide so that the method doesn't have to calculate it again.</param>
		/// <returns>The approximate distance between a line and a point at their closest point</returns>
		public static FInt LinePointDistApprox(VectorF point, VectorF lp1, VectorF lp2, FInt length)
		{
			return length == FInt.F0 ? VectorF.DistanceApprox(point, lp1) : VectorF.DistanceApprox(lp1 + ((length - VectorF.DistanceApprox(lp2, point) + VectorF.DistanceApprox(lp1, point)) / FInt.F2 / length) * (lp2 - lp1), point);
		}

		/// <summary>Determines how much higher or lower a point is than a line</summary>
		/// <param name="point">The point</param>
		/// <param name="lp1">The first point in the line</param>
		/// <param name="lp2">The second point in the line</param>
		/// <returns>How much higher or lower a point is than a line</returns>
		public static FInt LinePointDifference(VectorF point, VectorF lp1, VectorF lp2)
		{
			return LinePointDifference(point, lp1, lp2, (lp2.Y - lp1.Y) / (lp2.X - lp1.X));
		}

		/// <summary>Determines how much higher or lower a point is than a line</summary>
		/// <param name="point">The point</param>
		/// <param name="lp1">The first point in the line</param>
		/// <param name="lp2">The second point in the line</param>
		/// <param name="slope">The slope of the line</param>
		/// <returns>How much higher or lower a point is than a line</returns>
		public static FInt LinePointDifference(VectorF point, VectorF lp1, VectorF lp2, FInt slope)
		{
			FInt a = point.Y - (lp1.Y + (point.X - lp1.X) * slope);
			return point.Y - (lp1.Y + (point.X - lp1.X) * slope);
		}

		/// <summary>Determines how much higher or lower a point is than a line</summary>
		/// <param name="point">The point</param>
		/// <param name="lp1">The first point in the line</param>
		/// <param name="lp2">The second point in the line</param>
		/// <returns>How much higher or lower a point is than a line</returns>
		public static float LinePointDifference(Vector2 point, Vector2 lp1, Vector2 lp2)
		{
			return LinePointDifference(point, lp1, lp2, (lp2.Y - lp1.Y) / (lp2.X - lp1.X));
		}

		/// <summary>Determines how much higher or lower a point is than a line</summary>
		/// <param name="point">The point</param>
		/// <param name="lp1">The first point in the line</param>
		/// <param name="lp2">The second point in the line</param>
		/// <param name="slope">The slope of the line</param>
		/// <returns>How much higher or lower a point is than a line</returns>
		public static float LinePointDifference(Vector2 point, Vector2 lp1, Vector2 lp2, float slope)
		{
			if (lp1.X < lp2.X)
				return point.Y - (lp1.Y + (point.X - lp1.X) * slope);

			return point.Y - (lp2.Y + (point.X - lp2.X) * slope);
		}

		/// <summary>Determines how much higher or lower a point is than a line</summary>
		/// <param name="point">The point</param>
		/// <param name="lp1">The first point in the line</param>
		/// <param name="lp2">The second point in the line</param>
		/// <returns>How much higher or lower a point is than a line</returns>
		public static double LinePointDifference(VectorD point, VectorD lp1, VectorD lp2)
		{
			return LinePointDifference(point, lp1, lp2, (lp2.Y - lp1.Y) / (lp2.X - lp1.X));
		}

		/// <summary>Determines how much higher or lower a point is than a line</summary>
		/// <param name="point">The point</param>
		/// <param name="lp1">The first point in the line</param>
		/// <param name="lp2">The second point in the line</param>
		/// <param name="slope">The slope of the line</param>
		/// <returns>How much higher or lower a point is than a line</returns>
		public static double LinePointDifference(VectorD point, VectorD lp1, VectorD lp2, double slope)
		{
			if (lp1.X < lp2.X)
				return point.Y - (lp1.Y + (point.X - lp1.X) * slope);

			return point.Y - (lp2.Y + (point.X - lp2.X) * slope);
		}

		/// <summary>Finds the angle of the line connecting the two points.</summary>
		/// <param name="point1">The first point in the line.</param>
		/// <param name="point2">The second point in the line.</param>
		/// <returns>The angle of the line connecting the two points.</returns>
		public static FInt FindAngle(VectorF point1, VectorF point2)
		{
			FInt denom = (point2.X - point1.X);
			if (denom == FInt.F0)
				return FInt.F0;

			return (PI / FInt.F2) + Atan((point2.Y - point1.Y) / denom);
		}

		/// <summary>Finds the angle of the line connecting the two points.</summary>
		/// <param name="point1">The first point in the line.</param>
		/// <param name="point2">The second point in the line.</param>
		/// <returns>The angle of the line connecting the two points.</returns>
		public static float FindAngle(Vector2 point1, Vector2 point2)
		{
			double denom = ((double)point2.X - (double)point1.X);
			if (denom == 0d)
				return 0f;

			return (float)((Math.PI / 2d) + Math.Atan(((double)point2.Y - (double)point1.Y) / denom));
		}

		/// <summary>Finds the vector's angle with respect to VectorF.Zero</summary>
		/// <param name="vector">The vector to measure</param>
		/// <returns>he vector's angle with respect to VectorF.Zero</returns>
		public static FInt FindAngle(VectorF vector)
		{
			FInt dist = vector.Length();

			if (dist == FInt.F0)
				return FInt.F0;

			FInt angle = Calc.Acos(vector.X / dist);

			if (vector.Y < FInt.F0)
				return TwoPIF - angle;

			return angle;
		}

		/// <summary>Find's the angle between the two lines with respect to the specified origin</summary>
		/// <param name="p1">The first line</param>
		/// <param name="mp">The origin</param>
		/// <param name="p2">The second line</param>
		/// <returns>The angle between the two lines with respect to the specified origin</returns>
		public static FInt FindAngle(VectorF p1, VectorF mp, VectorF p2)
		{
			// this algorithm may not be working correctly
			p1 -= mp;
			p2 -= mp;

			FInt angle = (FindAngle(p1) - FindAngle(p2));
			if (angle > Calc.PI)
				return TwoPIF - angle;

			return angle;
		}

		/// <summary>Finds the length of the adjacent edge in a right triangle.</summary>
		/// <param name="hyp">The length of the hypotenuse of the triangle.</param>
		/// <param name="opp">The length of the opposite edge of the triangle.</param>
		/// <returns>The length of the adjacent edge in a right triangle.</returns>
		public static FInt getAdj(FInt hyp, FInt opp)
		{
			return Calc.Sqrt((hyp * hyp) - (opp * opp));
		}

		/// <summary>Splits the provided line strip into a triangle list.</summary>
		/// <param name="vertex">A line list of the vertices in the line.</param>
		/// <returns>A triangle list.</returns>
		public static Vector3[] getTriangles(VectorF[] vertex)
		{
			try
			{
				List<Vector2> triangle = new List<Vector2>();
				List<List<Vector2>> shape = new List<List<Vector2>>();

				// convert from line list to line strip
				shape.Add(new List<Vector2>());
				shape[0].Add((Vector2)vertex[0]);
				for (int i = 1; i < vertex.Length; i += 2)
				{
					shape[0].Add((Vector2)vertex[i]);
				}

				// break it down into triangles
				while (shape.Count > 0)
				{
					int index = shape.Count - 1;
					List<Vector2> shp = shape[index];

					if (shp.Count == 3)
					{
						triangle.AddRange(shp);
						shape.RemoveAt(index);
						continue;
					}

					// get topmost vertex
					int vx0 = 0;
					for (int i = 1; i < shp.Count; i++)
					{
						if (shp[i].Y < shp[vx0].Y || (shp[i].Y == shp[vx0].Y && shp[i].X < shp[vx0].X))
							vx0 = i;
					}

					// get second vertex in pair
					int vx1 = (vx0 == 0) ? (shp.Count - 1) : (vx0 - 1);

					// get neighbors
					int vx0Nbr = (vx0 == shp.Count - 1) ? 0 : (vx0 + 1);
					int vx1Nbr = (vx1 == 0) ? (shp.Count - 1) : (vx1 - 1);

					// get slopes
					float denom = shp[vx1].X - shp[vx1Nbr].X;
					float slLeft = (denom == 0f) ? 0f : ((shp[vx1].Y - shp[vx1Nbr].Y) / denom);

					denom = shp[vx0].X - shp[vx1].X;
					float slMid = (denom == 0f) ? 0f : ((shp[vx0].Y - shp[vx1].Y) / denom);

					denom = shp[vx0Nbr].X - shp[vx0].X;
					float slRight = (denom == 0f) ? 0f : ((shp[vx0Nbr].Y - shp[vx0].Y) / denom);

					// find out if neighbor is acute or obtuse
					bool leftLkAbove = (shp[vx1Nbr].X <= shp[vx1].X);
					bool midLkAbove = (shp[vx1].X <= shp[vx0].X);
					bool rightLkAbove = (shp[vx0Nbr].X >= shp[vx0].X);

					// if counterclockwise, reverse them
					if ((shp[vx0Nbr].X <= shp[vx0].X && Calc.LinePointDifference(shp[vx1], shp[vx0], shp[vx0Nbr], slRight) > 0)
						|| (shp[vx0Nbr].X > shp[vx0].X && Calc.LinePointDifference(shp[vx1], shp[vx0], shp[vx0Nbr], slRight) < 0))
					{
						leftLkAbove = !leftLkAbove;
						midLkAbove = !midLkAbove;
						rightLkAbove = !rightLkAbove;
					}

					// search vertices for one that doesn't intersect and falls in valid zones
					bool invalid = true;
					int vx2;
					bool useVx1NbrScope = (midLkAbove ? 1f : -1f) * Calc.LinePointDifference(shp[vx1Nbr], shp[vx1], shp[vx0], slMid) >= 0f && (vx1Nbr == vx0Nbr || (rightLkAbove ? 1f : -1f) * Calc.LinePointDifference(shp[vx1Nbr], shp[vx0], shp[vx0Nbr], slRight) >= 0f);
					for (vx2 = vx0Nbr; vx2 != vx1; vx2 = (vx2 + 1) % shp.Count)
					{
						invalid = false;

						// test point to see if it's in the valid zones
						if ((midLkAbove ? 1f : -1f) * Calc.LinePointDifference(shp[vx2], shp[vx1], shp[vx0], slMid) >= 0f
							&& (vx2 == vx0Nbr || (rightLkAbove ? 1f : -1f) * Calc.LinePointDifference(shp[vx2], shp[vx0], shp[vx0Nbr], slRight) >= 0f))
						{
							if (!useVx1NbrScope || vx2 == vx1Nbr || ((leftLkAbove ? 1f : -1f) * Calc.LinePointDifference(shp[vx2], shp[vx1Nbr], shp[vx1], slLeft)) >= 0f)
							{
								// get bounding boxes
								float vx0Left = Math.Min(shp[vx2].X, shp[vx0].X);
								float vx0Top = Math.Min(shp[vx2].Y, shp[vx0].Y);
								float vx0Right = Math.Max(shp[vx2].X, shp[vx0].X);
								float vx0Bottom = Math.Max(shp[vx2].Y, shp[vx0].Y);

								float vx1Left = Math.Min(shp[vx2].X, shp[vx1].X);
								float vx1Top = Math.Min(shp[vx2].Y, shp[vx1].Y);
								float vx1Right = Math.Max(shp[vx2].X, shp[vx1].X);
								float vx1Bottom = Math.Max(shp[vx2].Y, shp[vx1].Y);

								// test to make sure it doesn't intersect any other lines
								for (int vxt0 = vx0, vxt1 = 0; vxt0 != vx1Nbr && !invalid; vxt0 = vxt1)
								{
									vxt1 = vxt0 + 1;
									if (vxt1 == shp.Count)
										vxt1 = 0;

									if (vxt0 != vx2 && vxt1 != vx2)
									{
										// find bounding box
										float vLeft = Math.Min(shp[vxt0].X, shp[vxt1].X);
										float vTop = Math.Min(shp[vxt0].Y, shp[vxt1].Y);
										float vRight = Math.Max(shp[vxt0].X, shp[vxt1].X);
										float vBottom = Math.Max(shp[vxt0].Y, shp[vxt1].Y);

										if (vxt0 != vx0
											&& vLeft <= vx0Right
											&& vTop <= vx0Bottom
											&& vRight >= vx0Left
											&& vBottom >= vx0Top)
										{
											invalid = Calc.DoLinesIntersect(shp[vx0], shp[vx2], shp[vxt0], shp[vxt1]);
										}

										if (!invalid && vxt1 != vx1
											&& vLeft <= vx1Right
											&& vTop <= vx1Bottom
											&& vRight >= vx1Left
											&& vBottom >= vx1Top)
										{
											invalid = Calc.DoLinesIntersect(shp[vx1], shp[vx2], shp[vxt0], shp[vxt1]);
										}
									}
								}
							}
							else
							{
								invalid = true;
							}
						}
						else
						{
							invalid = true;
						}

						if (!invalid)
							break;
					}
#if DEBUG
					if (vx2 == vx1)
						throw new Exception("No valid third vertex found.");
#endif
					triangle.AddRange(new Vector2[] { shp[vx0], shp[vx1], shp[vx2] });

					// if all three points were adjacent, remove the one in the middle
					if (vx2 == vx0Nbr)
					{
						shp.RemoveAt(vx0);
					}
					else if (vx2 == vx1Nbr)
					{
						shp.RemoveAt(vx1);
					}
					else // if the shape is split in 2
					{
						List<Vector2> newShp = new List<Vector2>();

						for (int i = vx0; i != vx2; i++)
						{
							if (i == shp.Count)
							{
								shp.RemoveRange(vx0, shp.Count - vx0);
								i = 0;
							}

							newShp.Add(shp[i]);
						}

						newShp.Add(shp[vx2]);

						if (vx2 < vx0)
							shp.RemoveRange(0, vx2);
						else
							shp.RemoveRange(vx0, vx2 - vx0);

						shape.Add(newShp);
					}
				}

				Vector3[] result = new Vector3[triangle.Count];
				for (int i = 0; i < triangle.Count; i++)
				{
					result[i] = new Vector3(triangle[i], 0f);
				}

				return result;
			}
			catch (Exception ex)
			{ }

			return null;
		}

		#region FInt

		#region PI, DoublePI, and other constants
		public static readonly FInt PI = new FInt(12868, false); //PI x 2^12
		public static readonly FInt TwoPIF = PI * FInt.F2; //radian equivalent of 260 degrees
		public static readonly FInt PIOver180F = PI / (FInt)180; //PI / 180

		private static readonly FInt F25736 = new FInt(25736, false);
		private static readonly FInt F10 = new FInt(10, false);
		private static readonly FInt F714 = new FInt(714, false);
		private static readonly FInt F6434 = new FInt(6434, false);
		private static readonly FInt F19302 = new FInt(19302, false);
		private static readonly FInt F100 = new FInt(100, false);
		private static readonly FInt F90 = new FInt(90, false);
		private static readonly FInt F180 = new FInt(180, false);
		private static readonly FInt F270 = new FInt(270, false);
		private static readonly FInt F360 = new FInt(360, false);
		private static readonly FInt F6435 = new FInt(6435, false);
		#endregion

		#region Sqrt
		public static FInt Sqrt(FInt f, int NumberOfIterations)
		{
			if (f.RawValue < 0) //NaN in Math.Sqrt
				throw new ArithmeticException("Input Error");
			if (f.RawValue == 0)
				return (FInt)0;
			FInt k = f + FInt.OneF >> 1;
			for (int i = 0; i < NumberOfIterations; i++)
				k = (k + (f / k)) >> 1;

			if (k.RawValue < 0)
				throw new ArithmeticException("Overflow");
			else
				return k;
		}

		public static FInt Sqrt(FInt f)
		{
			byte numberOfIterations = 8;
			if (f.RawValue > 0x64000)
				numberOfIterations = 12;
			if (f.RawValue > 0x3e8000)
				numberOfIterations = 16;
			return Sqrt(f, numberOfIterations);
		}
		#endregion

		#region Sin
		public static FInt Sin(FInt i)
		{
			FInt j = (FInt)0;

			for (; i < 0; i += F25736) ;
			if (i > F25736)
				i %= F25736;
			FInt k = (i * F10) / F714;
			if (i != 0 && i != F6434 && i != PI &&
				i != F19302 && i != F25736)
				j = (i * F100) / F714 - k * F10;
			if (k <= F90)
				return sin_lookup(k, j);
			if (k <= F180)
				return sin_lookup(F180 - k, j);
			if (k <= F270)
				return sin_lookup(k - F180, j).Inverse;
			else
				return sin_lookup(F360 - k, j).Inverse;
		}

		private static FInt sin_lookup(FInt i, FInt j)
		{
			FInt rawValue = new FInt(SIN_TABLE[i.RawValue], false);

			if (j > 0 && j < F10 && i < new FInt(90, false))
				return rawValue +
					((new FInt(SIN_TABLE[i.RawValue + 1], false) - rawValue) /
					F10) * j;
			else
				return rawValue;
		}

		private static int[] SIN_TABLE = {
        0, 71, 142, 214, 285, 357, 428, 499, 570, 641, 
        711, 781, 851, 921, 990, 1060, 1128, 1197, 1265, 1333, 
        1400, 1468, 1534, 1600, 1665, 1730, 1795, 1859, 1922, 1985, 
        2048, 2109, 2170, 2230, 2290, 2349, 2407, 2464, 2521, 2577, 
        2632, 2686, 2740, 2793, 2845, 2896, 2946, 2995, 3043, 3091, 
        3137, 3183, 3227, 3271, 3313, 3355, 3395, 3434, 3473, 3510, 
        3547, 3582, 3616, 3649, 3681, 3712, 3741, 3770, 3797, 3823, 
        3849, 3872, 3895, 3917, 3937, 3956, 3974, 3991, 4006, 4020, 
        4033, 4045, 4056, 4065, 4073, 4080, 4086, 4090, 4093, 4095, 
        4096
    };
		#endregion

		public static FInt Min(FInt F1, FInt F2)
		{
			if (F1.RawValue < F2.RawValue)
				return F1;
			return F2;
		}

		public static FInt Max(FInt F1, FInt F2)
		{
			if (F1.RawValue > F2.RawValue)
				return F1;
			return F2;
		}

		private static FInt mul(FInt F1, FInt F2)
		{
			return F1 * F2;
		}

		#region Cos, Tan, Asin, Acos
		public static FInt Cos(FInt i)
		{
			return Sin(i + F6435);
		}

		public static FInt Tan(FInt i)
		{
			return Sin(i) / Cos(i);
		}

		public static FInt Asin(FInt F)
		{
			bool isNegative = F < 0;
			F = Abs(F);
#if DEBUG
			if (F > FInt.OneF)
				throw new ArithmeticException("Bad Asin Input:" + F.ToDouble());
#endif

			FInt f1 = ((((new FInt(145103 >> FInt.SHIFT_AMOUNT, false) * F) -
				new FInt(599880 >> FInt.SHIFT_AMOUNT, false) * F) +
				new FInt(1420468 >> FInt.SHIFT_AMOUNT, false) * F) -
				new FInt(3592413 >> FInt.SHIFT_AMOUNT, false) * F) +
				new FInt(26353447 >> FInt.SHIFT_AMOUNT, false);
			FInt f2 = PI / new FInt(2, true) - (Sqrt(FInt.OneF - F) * f1);

			return isNegative ? f2.Inverse : f2;
		}

		public static FInt Acos(FInt F)
		{
			return Asin(F + F6435);
		}
		#endregion

		#region ATan, ATan2
		public static FInt Atan(FInt F)
		{
			return Asin(F / Sqrt(FInt.OneF + (F * F)));
		}

		public static FInt Atan2(FInt F1, FInt F2)
		{
			if (F2.RawValue == 0 && F1.RawValue == 0)
				return (FInt)0;

			FInt result = (FInt)0;
			if (F2 > 0)
				result = Atan(F1 / F2);
			else if (F2 < 0)
			{
				if (F1 >= 0)
					result = (PI - Atan(Abs(F1 / F2)));
				else
					result = (PI - Atan(Abs(F1 / F2))).Inverse;
			}
			else
				result = (F1 >= 0 ? PI : PI.Inverse) / new FInt(2, true);

			return result;
		}
		#endregion

		#region Abs
		public static FInt Abs(FInt F)
		{
			if (F < FInt.F0)
				return F.Inverse;
			else
				return F;
		}
		#endregion

		#endregion FInt
	}
}
