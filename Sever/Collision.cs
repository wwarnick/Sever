using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class CollisionManager
	{
		#region Members

		/// <summary>The various types of collision searches.</summary>
		private enum CollSrchType { GetIntersection, GetAllCollisions, DoesItCollide }

		/// <summary>The grid manager to use.</summary>
		public GridManager Grid { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of CollisionManager.</summary>
		public CollisionManager()
		{
			clear();
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public void clear()
		{
			Grid = null;
		}

		/// <summary>Searches for a collision between the provided line and any node, segment, or geo.  Ignores collisions with the specified segments and nodes.</summary>
		/// <param name="point1">The first point in the line.</param>
		/// <param name="point2">The second point in the line.</param>
		/// <param name="ignoreSeg">The segments to ignore.</param>
		/// <param name="ignoreNode">The nodes to ignore.</param>
		/// <param name="testSegs">Whether or not to test segments.</param>
		/// <param name="testNodes">Whether or not to test nodes.</param>
		/// <param name="testGeos">Whether or not to test geos.</param>
		/// <returns>Whether or not there was a collision.</returns>
		public bool segCollision(VectorF point1, VectorF point2, List<SegmentSkel> ignoreSeg, List<NodeSkel> ignoreNode, bool testSegs, bool testNodes, bool testGeos)
		{
			return segCollision(point1, point2, ignoreSeg, ignoreNode, testSegs, testNodes, testGeos, false, null);
		}

		/// <summary>Searches for a collision between the provided line and any node, segment, or geo.  Ignores collisions with the specified segments and nodes.</summary>
		/// <param name="point1">The first point in the line.</param>
		/// <param name="point2">The second point in the line.</param>
		/// <param name="ignoreSeg">The segments to ignore.</param>
		/// <param name="ignoreNode">The nodes to ignore.</param>
		/// <param name="testSegs">Whether or not to test segments.</param>
		/// <param name="testNodes">Whether or not to test nodes.</param>
		/// <param name="testGeos">Whether or not to test geos.</param>
		/// <param name="includeUnbuilt">Whether or not to test against unfinished nodes and segments owned by a certain player.</param>
		/// <param name="owner">The owner of the unbuilt nodes and segments to test against.</param>
		/// <returns>Whether or not there was a collision.</returns>
		public bool segCollision(VectorF point1, VectorF point2, List<SegmentSkel> ignoreSeg, List<NodeSkel> ignoreNode, bool testSegs, bool testNodes, bool testGeos, bool includeUnbuilt, Player owner)
		{
			List<SegmentSkel> segColls = new List<SegmentSkel>(1);
			List<NodeSkel> nodeColls = new List<NodeSkel>(1);
			List<GeoSkel> geoColls = new List<GeoSkel>(1);

			object[] array = new object[7];
			array[0] = point1;
			array[1] = point2;
			array[4] = CollSrchType.DoesItCollide;
			array[5] = includeUnbuilt;
			array[6] = owner;

			// first look for collisions with nodes
			if (testNodes)
			{
				array[2] = ignoreNode;
				array[3] = nodeColls;
				Grid.LineExpand(point1, point2, array, gridSegCollNode);
			}

			if (nodeColls.Count == 0)
			{
				// next look for collisions with segments
				if (testSegs)
				{
					array[2] = ignoreSeg;
					array[3] = segColls;
					Grid.Line(point1, point2, array, gridSegCollSeg);
				}

				if (segColls.Count == 0 && testGeos)
				{
					// next look for collisions with geos
					array[2] = null;
					array[3] = geoColls;
					Grid.Line(point1, point2, array, gridSegCollGeo);
				}
			}

			return nodeColls.Count > 0 || segColls.Count > 0 || geoColls.Count > 0;
		}

		/// <summary>Searches for a collision between the provided line and any node, segment, or geo.  Ignores collisions with the specified segments and nodes.</summary>
		/// <param name="point1">The first point in the line.</param>
		/// <param name="point2">The second point in the line.</param>
		/// <param name="ignoreSeg">The segments to ignore.</param>
		/// <param name="ignoreNode">The nodes to ignore.</param>
		/// <returns>Whether or not there was a collision.</returns>
		public bool segCollision(VectorF point1, VectorF point2, List<SegmentSkel> ignoreSeg, List<NodeSkel> ignoreNode)
		{
			return segCollision(point1, point2, ignoreSeg, ignoreNode, true, true, true, false, null);
		}

		/// <summary>Searches for a collision between the provided line and any node or segment.  Ignores collisions with the specified segments and nodes.</summary>
		/// <param name="point1">The first point in the line.</param>
		/// <param name="point2">The second point in the line.</param>
		/// <param name="ignoreSeg">The segments to ignore.</param>
		/// <param name="ignoreNode">The nodes to ignore.</param>
		/// <param name="segColls">The list of segment collisions.</param>
		/// <param name="nodeColls">The list of node collisions.</param>
		/// <param name="geoColls">The list of geo collisions.</param>
		/// <returns>Whether or not there was a collision.</returns>
		public bool segCollision(VectorF point1, VectorF point2, List<SegmentSkel> ignoreSeg, List<NodeSkel> ignoreNode, out List<SegmentSkel> segColls, out List<NodeSkel> nodeColls, out List<GeoSkel> geoColls)
		{
			return segCollision(point1, point2, ignoreSeg, ignoreNode, out segColls, out nodeColls, out geoColls, false, null);
		}

		/// <summary>Searches for a collision between the provided line and any node or segment.  Ignores collisions with the specified segments and nodes.</summary>
		/// <param name="point1">The first point in the line.</param>
		/// <param name="point2">The second point in the line.</param>
		/// <param name="ignoreSeg">The segments to ignore.</param>
		/// <param name="ignoreNode">The nodes to ignore.</param>
		/// <param name="segColls">The list of segment collisions.</param>
		/// <param name="nodeColls">The list of node collisions.</param>
		/// <param name="geoColls">The list of geo collisions.</param>
		/// <param name="includeUnbuilt">Whether or not to test against unfinished nodes and segments owned by a certain player.</param>
		/// <param name="owner">The owner of the unbuilt nodes and segments to test against.</param>
		/// <returns>Whether or not there was a collision.</returns>
		public bool segCollision(VectorF point1, VectorF point2, List<SegmentSkel> ignoreSeg, List<NodeSkel> ignoreNode, out List<SegmentSkel> segColls, out List<NodeSkel> nodeColls, out List<GeoSkel> geoColls, bool includeUnbuilt, Player owner)
		{
			segColls = new List<SegmentSkel>();
			nodeColls = new List<NodeSkel>();
			geoColls = new List<GeoSkel>();

			object[] array = new object[7];
			array[0] = point1;
			array[1] = point2;
			array[4] = CollSrchType.GetAllCollisions;
			array[5] = includeUnbuilt;
			array[6] = owner;

			// first look for collisions with nodes
			array[2] = ignoreNode;
			array[3] = nodeColls;
			Grid.LineExpand(point1, point2, array, gridSegCollNode);

			// next look for collisions with segments
			array[2] = ignoreSeg;
			array[3] = segColls;
			Grid.Line(point1, point2, array, gridSegCollSeg);

			// next look for collisions with geos
			array[2] = null;
			array[3] = geoColls;
			Grid.Line(point1, point2, array, gridSegCollGeo);

			return nodeColls.Count > 0 || segColls.Count > 0 || geoColls.Count > 0;
		}

		/// <summary>Searches for a collision between the provided line and any node or segment.  Also finds the exact intersection point if segment collision.</summary>
		/// <param name="point1">The first point in the line.</param>
		/// <param name="point2">The second point in the line.</param>
		/// <param name="ignoreSeg">The segments to ignore.</param>
		/// <param name="ignoreNode">The nodes to ignore.</param>
		/// <param name="nodeColl">The node that collided with the provided line.</param>
		/// <param name="segColl">The segment that collided with the provided line.</param>
		/// <param name="geoColl">The geometric object that collided with the provided line.</param>
		/// <param name="intersection">The intersection point if intersecting a segment.</param>
		/// <returns>Whether or not there was a collision between the line and any node or segment.</returns>
		public bool segCollisionIntPoint(VectorF point1, VectorF point2, List<SegmentSkel> ignoreSeg, List<NodeSkel> ignoreNode, out SegmentSkel segColl, out NodeSkel nodeColl, out GeoSkel geoColl, out VectorF? intersection)
		{
			intersection = null;
			segColl = null;
			nodeColl = null;
			geoColl = null;
			object[] array = new object[8];
			array[0] = point1;
			array[1] = point2;
			array[5] = false;
			array[6] = null;

			// look for collisions with segments
			List<SegmentSkel> segColls = new List<SegmentSkel>(1);
			array[2] = ignoreSeg;
			array[3] = segColls;
			array[4] = CollSrchType.GetIntersection;
			Grid.Line(point1, point2, array, gridSegCollSeg);

			if (segColls.Count > 0)
			{
				segColl = segColls[0];
				intersection = (VectorF)array[7];
#if DEBUG
				if (array[7] == null)
					throw new Exception(segColls.Count.ToString() + " segment(s) was returned, but no intersections.");
#endif
			}
			else // look for collisions with nodes and geos
			{
				// look for nodes
				List<NodeSkel> nodeColls = new List<NodeSkel>(1);
				array[2] = ignoreNode;
				array[3] = nodeColls;
				array[4] = CollSrchType.DoesItCollide;
				Grid.LineExpand(point1, point2, array, gridSegCollNode);

				if (nodeColls.Count > 0)
				{
					nodeColl = nodeColls[0];
				}
				else // look for geos
				{
					List<GeoSkel> geoColls = new List<GeoSkel>(1);
					array[2] = null;
					array[3] = geoColls;
					array[4] = CollSrchType.DoesItCollide;
					Grid.LineExpand(point1, point2, array, gridSegCollGeo);

					if (geoColls.Count > 0)
						geoColl = geoColls[0];
				}

			}

			return nodeColl != null || segColl != null || geoColl != null;
		}

		/// <summary>Find all nodes in the specified grid square that collide with the specified line.</summary>
		/// <param name="sqr">The grid square to search.</param>
		/// <param name="array">An object array that contains the following fields: line point1, line point 2, nodes to ignore, a list to collect all colliding nodes, and whether to stop after first collision.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridSegCollNode(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			VectorF point1 = (VectorF)((object[])array)[0];
			VectorF point2 = (VectorF)((object[])array)[1];
			List<NodeSkel> ignoreNode = (List<NodeSkel>)((object[])array)[2];
			List<NodeSkel> nodeColls = (List<NodeSkel>)((object[])array)[3];
			CollSrchType srchType = (CollSrchType)((object[])array)[4];
			bool includeUnbuilt = (bool)((object[])array)[5];
			Player owner = includeUnbuilt ? (Player)((object[])array)[6] : null;

			// find line bounds
			FInt lineLeft;
			FInt lineRight;
			FInt lineTop;
			FInt lineBottom;

			if (point1.X < point2.X)
			{
				lineLeft = point1.X;
				lineRight = point2.X;
			}
			else
			{
				lineLeft = point2.X;
				lineRight = point1.X;
			}

			if (point1.Y < point2.Y)
			{
				lineTop = point1.Y;
				lineBottom = point2.Y;
			}
			else
			{
				lineTop = point2.Y;
				lineBottom = point1.Y;
			}

			foreach (NodeSkel node in sqr.Nodes)
			{
				if (node.LastCheck != time || node.LastCheckNum != opID)
				{
					node.LastCheck = time;
					node.LastCheckNum = opID;

					// first compare against bounds and then find intersection
					if ((node.Active || (includeUnbuilt && node.Owner == owner)) &&
						!ignoreNode.Contains(node) &&
						lineLeft <= node.Pos.X + node.Radius &&
						lineRight >= node.Pos.X - node.Radius &&
						lineTop <= node.Pos.Y + node.Radius &&
						lineBottom >= node.Pos.Y - node.Radius &&
						Calc.LinePointDistance(node.Pos, point1, point2) <= node.Radius)
					{
						nodeColls.Add(node);

						if (srchType == CollSrchType.DoesItCollide || srchType == CollSrchType.GetIntersection)
						{
							Grid.CancelOperation = true;
							break;
						}
					}
				}
			}
		}

		/// <summary>Find all segments in the specified grid square that collide with the specified line.</summary>
		/// <param name="sqr">The grid square to search.</param>
		/// <param name="array">An object array that contains the following fields: line point1, line point 2, segments to ignore, a list to collect all colliding segments, and whether to stop after first collision.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridSegCollSeg(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			VectorF point1 = (VectorF)((object[])array)[0];
			VectorF point2 = (VectorF)((object[])array)[1];
			List<SegmentSkel> ignoreSeg = (List<SegmentSkel>)((object[])array)[2];
			List<SegmentSkel> segColls = (List<SegmentSkel>)((object[])array)[3];
			CollSrchType srchType = (CollSrchType)((object[])array)[4];
			bool includeUnbuilt = (bool)((object[])array)[5];
			Player owner = includeUnbuilt ? (Player)((object[])array)[6] : null;

			SegmentSkel closestSeg = null;
			VectorF closestInt = VectorF.Zero;
			VectorF closestDist = VectorF.Zero;

			// find line bounds
			FInt lineLeft;
			FInt lineRight;
			FInt lineTop;
			FInt lineBottom;

			if (point1.X < point2.X)
			{
				lineLeft = point1.X;
				lineRight = point2.X;
			}
			else
			{
				lineLeft = point2.X;
				lineRight = point1.X;
			}

			if (point1.Y < point2.Y)
			{
				lineTop = point1.Y;
				lineBottom = point2.Y;
			}
			else
			{
				lineTop = point2.Y;
				lineBottom = point1.Y;
			}

			foreach (SegmentSkel seg in sqr.Segments)
			{
				if ((seg.LastCheck != time || seg.LastCheckNum != opID) && !ignoreSeg.Contains(seg))
				{
					seg.LastCheck = time;
					seg.LastCheckNum = opID;

					// find endpoints
					VectorF end0;
					VectorF end1;
					if (includeUnbuilt && seg.Owner == owner)
					{
						if (seg.State[1] == SegmentSkel.SegState.Retracting)
							end0 = seg.EndLoc[0];
						else
							end0 = ((Segment)seg).Nodes[0].Pos;

						if (seg.State[0] == SegmentSkel.SegState.Retracting)
							end1 = seg.EndLoc[1];
						else
							end1 = ((Segment)seg).Nodes[1].Pos;
					}
					else
					{
						end0 = seg.EndLoc[0];
						end1 = seg.EndLoc[1];
					}

					// find segment bounds
					FInt segLeft;
					FInt segRight;
					FInt segTop;
					FInt segBottom;

					if (end1.X < end0.X)
					{
						segLeft = end1.X;
						segRight = end0.X;
					}
					else
					{
						segLeft = end0.X;
						segRight = end1.X;
					}

					if (end1.Y < end0.Y)
					{
						segTop = end1.Y;
						segBottom = end0.Y;
					}
					else
					{
						segTop = end0.Y;
						segBottom = end1.Y;
					}

					// first compare against bounds and then find intersection
					if (lineLeft <= segRight &&
						lineRight >= segLeft &&
						lineTop <= segBottom &&
						lineBottom >= segTop)
					{
						if ((srchType == CollSrchType.GetAllCollisions || srchType == CollSrchType.DoesItCollide) && Calc.DoLinesIntersect(point1, point2, end1, end0))
						{
							segColls.Add(seg);

							if (srchType == CollSrchType.DoesItCollide)
							{
								Grid.CancelOperation = true;
								break;
							}
						}
						else if (srchType == CollSrchType.GetIntersection)
						{
							VectorF? intersection = Calc.LineIntersectIfExists(point1, point2, end1, end0);

							if (intersection != null)
							{
								VectorF val = intersection.Value;
								if (closestSeg == null || Calc.Abs(val.Y - point1.Y) < closestDist.Y || Calc.Abs(val.X - point1.X) < closestDist.X)
								{
									closestSeg = seg;
									closestInt = val;
									closestDist = new VectorF(Calc.Abs(val.X - point1.X), Calc.Abs(val.Y - point1.Y));
								}
							}
						}
					}
				}
			}

			if (srchType == CollSrchType.GetIntersection && closestSeg != null)
			{
				segColls.Add(closestSeg);
				((object[])array)[7] = closestInt;
			}
		}

		/// <summary>Find all geos in the specified grid square that collide with the specified line.</summary>
		/// <param name="sqr">The grid square to search.</param>
		/// <param name="array">An object array that contains the following fields: line point1, line point 2, null, a list to collect all colliding geos, and whether to stop after first collision.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridSegCollGeo(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			VectorF point1 = (VectorF)((object[])array)[0];
			VectorF point2 = (VectorF)((object[])array)[1];
			// array[2] is null
			List<GeoSkel> geoColls = (List<GeoSkel>)((object[])array)[3];
			CollSrchType srchType = (CollSrchType)((object[])array)[4];

			// find line bounds
			FInt lineLeft;
			FInt lineRight;
			FInt lineTop;
			FInt lineBottom;

			if (point1.X < point2.X)
			{
				lineLeft = point1.X;
				lineRight = point2.X;
			}
			else
			{
				lineLeft = point2.X;
				lineRight = point1.X;
			}

			if (point1.Y < point2.Y)
			{
				lineTop = point1.Y;
				lineBottom = point2.Y;
			}
			else
			{
				lineTop = point2.Y;
				lineBottom = point1.Y;
			}

			// test each geo
			foreach (GeoSkel geo in sqr.Geos)
			{
				if (geo.LastCheck == time && geo.LastCheckNum == opID)
					continue;

				// if bounding boxes overlap
				if (lineLeft <= geo.Right
					&& lineTop <= geo.Bottom
					&& lineRight >= geo.Left
					&& lineBottom >= geo.Top)
				{
					for (int i = 0; i < geo.LineTopLeft.Length; i++)
					{
						// if line's bounding box overlaps
						if (geo.LineTopLeft[i].X <= lineRight
							&& geo.LineTopLeft[i].Y <= lineBottom
							&& geo.LineLowerRight[i].X >= lineLeft
							&& geo.LineLowerRight[i].Y >= lineTop)
						{
							int vi;
							int vi1;

							if (i == geo.LineTopLeft.Length - 1 && geo.CloseLoop)
							{
								vi = geo.Vertices.Length - 1;
								vi1 = 0;
							}
							else
							{
								vi = i * 2;
								vi1 = vi + 1;
							}

							if (Calc.DoLinesIntersect(point1, point2, geo.Vertices[vi], geo.Vertices[vi1]))
							{
								geoColls.Add(geo);
								break;
							}
						}
					}
				}

				geo.LastCheck = time;
				geo.LastCheckNum = opID;

				// if only one collision was needed, exit out
				if (geoColls.Count != 0 && (srchType == CollSrchType.DoesItCollide || srchType == CollSrchType.GetIntersection))
				{
					Grid.CancelOperation = true;
					break;
				}
			}
		}

		/// <summary>Searches for a collision between the provided node and any node, segment, or geo.  Ignores collisions with the specified segments and nodes.</summary>
		/// <param name="center">The center of the node.</param>
		/// <param name="radius">The radius of the node.</param>
		/// <param name="ignoreSeg">The segments to ignore.</param>
		/// <param name="ignoreNode">The nodes to ignore.</param>
		/// <returns>Whether or not there was a collision.</returns>
		public bool nodeCollision(VectorF center, FInt radius, List<SegmentSkel> ignoreSeg, List<NodeSkel> ignoreNode)
		{
			return nodeCollision(center, radius, ignoreSeg, ignoreNode, false, null);
		}

		/// <summary>Searches for a collision between the provided node and any node, segment, or geo.  Ignores collisions with the specified segments and nodes.</summary>
		/// <param name="center">The center of the node.</param>
		/// <param name="radius">The radius of the node.</param>
		/// <param name="ignoreSeg">The segments to ignore.</param>
		/// <param name="ignoreNode">The nodes to ignore.</param>
		/// <param name="includeUnbuilt">Whether or not to test against unfinished nodes or segments owned by a certain player.</param>
		/// <param name="owner">The owner of the unbuilt nodes and segments to test against.</param>
		/// <returns>Whether or not there was a collision.</returns>
		public bool nodeCollision(VectorF center, FInt radius, List<SegmentSkel> ignoreSeg, List<NodeSkel> ignoreNode, bool includeUnbuilt, Player owner)
		{
			List<SegmentSkel> segColls = new List<SegmentSkel>(1);
			List<NodeSkel> nodeColls = new List<NodeSkel>(1);
			List<GeoSkel> geoColls = new List<GeoSkel>(1);

			object[] array = new object[7];
			array[0] = center;
			array[1] = radius;
			array[4] = CollSrchType.DoesItCollide;
			array[5] = includeUnbuilt;
			array[6] = owner;

			// first look for collisions with nodes
			array[2] = ignoreNode;
			array[3] = nodeColls;
			Grid.PointExpand(center, array, gridNodeCollNode);

			if (nodeColls.Count == 0)
			{
				// next look for collisions with segments
				array[2] = ignoreSeg;
				array[3] = segColls;
				Grid.PointExpand(center, array, gridNodeCollSeg);

				if (segColls.Count == 0)
				{
					// next look for collisions with geos
					array[2] = null;
					array[3] = geoColls;
					Grid.PointExpand(center, array, gridNodeCollGeo);
				}
			}

			return nodeColls.Count > 0 || segColls.Count > 0 || geoColls.Count > 0;
		}

		/// <summary>Find all nodes in the specified grid square that collide with the specified circle.</summary>
		/// <param name="sqr">The grid square to search.</param>
		/// <param name="array">An object array that contains the following fields: circle center, circle radius, nodes to ignore, a list to collect all colliding nodes, and whether to stop after first collision.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridNodeCollNode(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			VectorF point = (VectorF)((object[])array)[0];
			FInt radius = (FInt)((object[])array)[1];
			List<NodeSkel> ignoreNode = (List<NodeSkel>)((object[])array)[2];
			List<NodeSkel> nodeColls = (List<NodeSkel>)((object[])array)[3];
			CollSrchType srchType = (CollSrchType)((object[])array)[4];
			bool includeUnbuilt = (bool)((object[])array)[5];
			Player owner = includeUnbuilt ? (Player)((object[])array)[6] : null;

			// find circle bounding box
			FInt circLeft = point.X - radius;
			FInt circRight = point.X + radius;
			FInt circTop = point.Y - radius;
			FInt circBottom = point.Y + radius;

			foreach (NodeSkel node in sqr.Nodes)
			{
				if (node.LastCheck != time || node.LastCheckNum != opID)
				{
					node.LastCheck = time;
					node.LastCheckNum = opID;

					FInt minDist = radius + node.Radius;

					// first compare against bounds and then find intersection
					if ((node.Active || (includeUnbuilt && node.Owner == owner)) &&
						!ignoreNode.Contains(node) &&
						circLeft <= node.Pos.X + node.Radius &&
						circRight >= node.Pos.X - node.Radius &&
						circTop <= node.Pos.Y + node.Radius &&
						circBottom >= node.Pos.Y - node.Radius &&
						VectorF.DistanceApprox(node.Pos, point) < (minDist * FInt.F3) &&
						VectorF.Distance(node.Pos, point) < minDist)
					{
						nodeColls.Add(node);

						if (srchType == CollSrchType.DoesItCollide || srchType == CollSrchType.GetIntersection)
						{
							Grid.CancelOperation = true;
							break;
						}
					}
				}
			}
		}

		/// <summary>Find all segments in the specified grid square that collide with the specified circle.</summary>
		/// <param name="sqr">The grid square to search.</param>
		/// <param name="array">An object array that contains the following fields: node center, circle radius, null, segments to ignore, a list to collect all colliding segments, and whether to stop after first collision.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridNodeCollSeg(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			VectorF point = (VectorF)((object[])array)[0];
			FInt radius = (FInt)((object[])array)[1];
			List<SegmentSkel> ignoreSeg = (List<SegmentSkel>)((object[])array)[2];
			List<SegmentSkel> segColls = (List<SegmentSkel>)((object[])array)[3];
			CollSrchType srchType = (CollSrchType)((object[])array)[4];
			bool includeUnbuilt = (bool)((object[])array)[5];
			Player owner = includeUnbuilt ? (Player)((object[])array)[6] : null;

			// find circle bounding box
			FInt circLeft = point.X - radius;
			FInt circRight = point.X + radius;
			FInt circTop = point.Y - radius;
			FInt circBottom = point.Y + radius;

			foreach (SegmentSkel seg in sqr.Segments)
			{
				if ((seg.LastCheck != time || seg.LastCheckNum != opID) && !ignoreSeg.Contains(seg))
				{
					seg.LastCheck = time;
					seg.LastCheckNum = opID;

					// find endpoints
					VectorF end0;
					VectorF end1;
					if (includeUnbuilt && seg.Owner == owner)
					{
						if (seg.State[1] == SegmentSkel.SegState.Retracting)
							end0 = seg.EndLoc[0];
						else
							end0 = ((Segment)seg).Nodes[0].Pos;

						if (seg.State[0] == SegmentSkel.SegState.Retracting)
							end1 = seg.EndLoc[1];
						else
							end1 = ((Segment)seg).Nodes[1].Pos;
					}
					else
					{
						end0 = seg.EndLoc[0];
						end1 = seg.EndLoc[1];
					}

					// find segment bounds
					FInt segLeft;
					FInt segRight;
					FInt segTop;
					FInt segBottom;

					if (end1.X < end0.X)
					{
						segLeft = end1.X;
						segRight = end0.X;
					}
					else
					{
						segLeft = end0.X;
						segRight = end1.X;
					}

					if (end1.Y < end0.Y)
					{
						segTop = end1.Y;
						segBottom = end0.Y;
					}
					else
					{
						segTop = end0.Y;
						segBottom = end1.Y;
					}

					// first compare against bounds and then test for collision
					if (circLeft <= segRight &&
						circRight >= segLeft &&
						circTop <= segBottom &&
						circBottom >= segTop &&
						Calc.LinePointDistance(point, end1, end0) <= radius)
					{
						segColls.Add(seg);

						if (srchType == CollSrchType.GetIntersection || srchType == CollSrchType.DoesItCollide)
						{
							Grid.CancelOperation = true;
							break;
						}
					}
				}
			}
		}

		/// <summary>Find all geos in the specified grid square that collide with the specified circle.</summary>
		/// <param name="sqr">The grid square to search.</param>
		/// <param name="array">An object array that contains the following fields: node center, circle radius, null, null, a list to collect all colliding geos, and whether to stop after first collision.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridNodeCollGeo(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			VectorF point = (VectorF)((object[])array)[0];
			FInt radius = (FInt)((object[])array)[1];
			// array[2] is null
			List<GeoSkel> geoColls = (List<GeoSkel>)((object[])array)[3];
			CollSrchType srchType = (CollSrchType)((object[])array)[4];

			// find circle bounding box
			FInt circLeft = point.X - radius;
			FInt circRight = point.X + radius;
			FInt circTop = point.Y - radius;
			FInt circBottom = point.Y + radius;

			// test each geo
			foreach (GeoSkel geo in sqr.Geos)
			{
				if (geo.LastCheck == time && geo.LastCheckNum == opID)
					continue;

				// if bounding boxes overlap
				if (circLeft <= geo.Right
					&& circTop <= geo.Bottom
					&& circRight >= geo.Left
					&& circBottom >= geo.Top)
				{
					for (int i = 0; i < geo.LineTopLeft.Length; i++)
					{
						// if line's bounding box overlaps
						if (geo.LineTopLeft[i].X <= circRight
							&& geo.LineTopLeft[i].Y <= circBottom
							&& geo.LineLowerRight[i].X >= circLeft
							&& geo.LineLowerRight[i].Y >= circTop)
						{
							int vi;
							int vi1;

							if (i == geo.LineTopLeft.Length - 1 && geo.CloseLoop)
							{
								vi = geo.Vertices.Length - 1;
								vi1 = 0;
							}
							else
							{
								vi = i * 2;
								vi1 = vi + 1;
							}

							if (Calc.LinePointDistance(point, geo.Vertices[vi], geo.Vertices[vi1]) <= radius)
							{
								geoColls.Add(geo);
								break;
							}
						}
					}
				}

				geo.LastCheck = time;
				geo.LastCheckNum = opID;

				// if only one collision was needed, exit out
				if (geoColls.Count != 0 && (srchType == CollSrchType.DoesItCollide || srchType == CollSrchType.GetIntersection))
				{
					Grid.CancelOperation = true;
					break;
				}
			}
		}

		/// <summary>Searches for a collision between the provided node and any other node based on their spacing.  Ignores collisions with the specified nodes.</summary>
		/// <param name="center">The center of the node.</param>
		/// <param name="radius">The radius of the node.</param>
		/// <param name="spacing">The spacing of the node.</param>
		/// <param name="owner">The owner of the circle.</param>
		/// <param name="ignoreNode">The nodes to ignore.</param>
		/// <param name="includeUnbuilt">Whether or not to include inactive nodes.</param>
		/// <returns>Whether or not there was a collision.</returns>
		public bool nodeCollNodeSpacing(VectorF center, FInt radius, FInt spacing, Player owner, List<NodeSkel> ignoreNode, bool includeUnbuilt)
		{
			List<NodeSkel> nodeColls = new List<NodeSkel>(1);

			object[] array = new object[8];
			array[0] = center;
			array[1] = radius;
			array[2] = spacing;
			array[3] = owner;
			array[4] = ignoreNode;
			array[5] = nodeColls;
			array[6] = CollSrchType.DoesItCollide;
			array[7] = includeUnbuilt;
			Grid.PointExpand(center, array, gridNodeCollNodeSpacing);

			return nodeColls.Count > 0;
		}

		/// <summary>Find all nodes in the specified grid square that collide with the specified circle based on spacing.</summary>
		/// <param name="sqr">The grid square to search.</param>
		/// <param name="array">An object array that contains the following fields: circle center, circle radius, circle spacing, whether to test against nodes' spacing as opposed to radius, nodes to ignore, a list to collect all colliding nodes, and whether to stop after first collision.</param>
		/// <param name="time">The current game time.</param>
		/// <param name="opID">The current operation id.</param>
		private void gridNodeCollNodeSpacing(GridSqr sqr, object array, TimeSpan time, int opID)
		{
			VectorF point = (VectorF)((object[])array)[0];
			FInt radius = (FInt)((object[])array)[1];
			FInt spacing = (FInt)((object[])array)[2];
			Player owner = (Player)((object[])array)[3];
			List<NodeSkel> ignoreNode = (List<NodeSkel>)((object[])array)[4];
			List<NodeSkel> nodeColls = (List<NodeSkel>)((object[])array)[5];
			CollSrchType srchType = (CollSrchType)((object[])array)[6];
			bool includeUnbuilt = (bool)((object[])array)[7];

			// find circle bounding box
			FInt circLeft = point.X - radius - spacing;
			FInt circRight = point.X + radius + spacing;
			FInt circTop = point.Y - radius - spacing;
			FInt circBottom = point.Y + radius + spacing;

			foreach (NodeSkel node in sqr.Nodes)
			{
				if (node.LastCheck != time || node.LastCheckNum != opID)
				{
					node.LastCheck = time;
					node.LastCheckNum = opID;

					if (ignoreNode.Contains(node) || node.Owner != owner || (!includeUnbuilt && !node.Active))
						continue;

					FInt minDist = radius + node.Radius + Calc.Max(spacing, node.Spacing);

					// first compare against bounds and then find intersection
					if (circLeft <= node.Pos.X + node.Radius + node.Spacing &&
						circRight >= node.Pos.X - node.Radius - node.Spacing &&
						circTop <= node.Pos.Y + node.Radius + node.Spacing &&
						circBottom >= node.Pos.Y - node.Radius - node.Spacing &&
						VectorF.DistanceApprox(node.Pos, point) < minDist * FInt.F3 &&
						VectorF.Distance(node.Pos, point) < minDist)
					{
						nodeColls.Add(node);

						if (srchType == CollSrchType.DoesItCollide || srchType == CollSrchType.GetIntersection)
						{
							Grid.CancelOperation = true;
							break;
						}
					}
				}
			}
		}

		#endregion Methods
	}
}
