using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class Camera
	{
		#region Members

		/// <summary>The size of this camera.</summary>
		private VectorF size;

		/// <summary>The location of the center of the camera in the world.</summary>
		private VectorF center;

		/// <summary>the upper-left corner of the camera on the world map.</summary>
		private VectorF upperLeft;

		/// <summary>The lower-right corner of the camera on the world map.</summary>
		private VectorF lowerRight;

		/// <summary>The size of this camera.</summary>
		public VectorF Size { get { return size; } set { size = value; } }

		/// <summary>The width of this camera.</summary>
		public FInt Width { get { return size.X; } set { size.X = value; } }

		/// <summary>The height of this camera.</summary>
		public FInt Height { get { return size.Y; } set { size.Y = value; } }

		/// <summary>The location of the center of the camera in the world.</summary>
		public VectorF Center { get { return center; } set { center = value; } }

		/// <summary>The x-coordinate of the location of the center of the camera in the world.</summary>
		public FInt CenterX { get { return center.X; } set { center.X = value; } }

		/// <summary>The y-coordinate of the location of the center of the camera in the world.</summary>
		public FInt CenterY { get { return center.Y; } set { center.Y = value; } }

		/// <summary>the upper-left corner of the camera on the world map.</summary>
		public VectorF UpperLeft { get { return upperLeft; } set { upperLeft = value; } }

		/// <summary>The lower-right corner of the camera on the world map.</summary>
		public VectorF LowerRight { get { return lowerRight; } set { lowerRight = value; } }

		/// <summary>The x-coordinate of the left edge of the camera on the world map.</summary>
		public FInt Left { get { return upperLeft.X; } set { upperLeft.X = value; } }

		/// <summary>The y-coordinate of the top edge of the camera on the world map.</summary>
		public FInt Top { get { return upperLeft.Y; } set { upperLeft.Y = value; } }

		/// <summary>The x-coordinate of the right edge of the camera on the world map.</summary>
		public FInt Right { get { return lowerRight.X; } set { lowerRight.X = value; } }

		/// <summary>The y-coordinate of the bottom edge of the camera on the world map.</summary>
		public FInt Bottom { get { return lowerRight.Y; } set { lowerRight.Y = value; } }

		/// <summary>This camera's level of zoom.  1f is 100%.</summary>
		public FInt Zoom { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of Camera.</summary>
		public Camera()
		{
			clear();
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public void clear()
		{
			Size = new VectorF((FInt)1024, (FInt)768);
			Center = Size / FInt.F2;
			UpperLeft = VectorF.Zero;
			LowerRight = VectorF.Zero;
			Zoom = FInt.F1;
			refreshCorners();
		}

		/// <summary>Refreshes the corners of the camera on the map.</summary>
		public void refreshCorners()
		{
			FInt mod = FInt.F2 * Zoom;
			FInt halfWidth = size.X / mod;
			FInt halfHeight = size.Y / mod;
			upperLeft.X = center.X - halfWidth;
			upperLeft.Y = center.Y - halfHeight;
			lowerRight.X = center.X + halfWidth;
			lowerRight.Y = center.Y + halfHeight;
		}

		/// <summary>Converts a coordinate from the world to the screen.</summary>
		/// <param name="point">The point to convert.</param>
		/// <returns>A screen coordinate.</returns>
		public Vector2 worldToScreen(VectorF point)
		{
			return (Vector2)((point - upperLeft) * Zoom);
		}

		/// <summary>Converts a coordinate from the world to the screen for drawing (ignores the zoom).</summary>
		/// <param name="point">The point to convert.</param>
		/// <returns>A screen coordinate.</returns>
		public Vector2 worldToScreenDraw(VectorF point)
		{
			return (Vector2)(point - upperLeft);
		}

		/// <summary>Converts a coordinate from the world to the screen for drawing (ignores the zoom).</summary>
		/// <param name="point">The point to convert.</param>
		/// <returns>A screen coordinate.</returns>
		public Vector3 worldToScreenDraw(Vector3 point)
		{
			return new Vector3(point.X - (float)upperLeft.X, point.Y - (float)upperLeft.Y, 0f);
		}

		/// <summary>Converts a coordinate from the screen to the world.</summary>
		/// <param name="point">The point to convert.</param>
		/// <returns>A world coordinate.</returns>
		public VectorF screenToWorld(Vector2 point)
		{
			return ((VectorF)point / Zoom) + UpperLeft;
		}

		/// <summary>Determines whether or not the provided point is in view of this camera.</summary>
		/// <param name="point">The point to test.</param>
		/// <returns>Whether or not the provided point is in view of this camera.</returns>
		public bool inView(VectorF point)
		{
			return point.X >= Left
				&& point.X <= Right
				&& point.Y >= Top
				&& point.Y <= Bottom;
		}

		/// <summary>Determines whether or not the provided point is in view of this camera.</summary>
		/// <param name="point">The point to test.</param>
		/// <returns>Whether or not the provided point is in view of this camera.</returns>
		public bool inView(Vector3 point)
		{
			return (FInt)point.X >= Left
				&& (FInt)point.X <= Right
				&& (FInt)point.Y >= Top
				&& (FInt)point.Y <= Bottom;
		}

		#endregion Methods
	}
}
