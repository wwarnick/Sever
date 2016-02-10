using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class GeoSkel
	{
		#region Members

		/// <summary>This geo's id.</summary>
		public string ID { get; set; }

		/// <summary>A line list of the edges of this geometric obstacle.</summary>
		public VectorF[] Vertices { get; set; }

		/// <summary>The upper-left corner of this geometric obstacle.</summary>
		private VectorF upperLeft;

		/// <summary>The lower-right corner of this obstacle.</summary>
		private VectorF lowerRight;

		/// <summary>The upper-left corner boundary of this geometric obstacle.</summary>
		public VectorF UpperLeft { get { return upperLeft; } set { upperLeft = value; } }

		/// <summary>The lower-right corner boundary of this obstacle.</summary>
		public VectorF LowerRight { get { return lowerRight; } set { lowerRight = value; } }

		/// <summary>The left boundary of this geometric obstacle.</summary>
		public FInt Left { get { return upperLeft.X; } set { upperLeft.X = value; } }

		/// <summary>The top boundary of this geometric obstacle.</summary>
		public FInt Top { get { return upperLeft.Y; } set { upperLeft.Y = value; } }

		/// <summary>The right boundary of this geometric obstacle.</summary>
		public FInt Right { get { return lowerRight.X; } set { lowerRight.X = value; } }

		/// <summary>The bottom boundary of this geometric obstacle.</summary>
		public FInt Bottom { get { return lowerRight.Y; } set { lowerRight.Y = value; } }

		/// <summary>The top-left corner boundary of each line.</summary>
		public VectorF[] LineTopLeft { get; protected set; }

		/// <summary>The lower-right corner boundary of each line.</summary>
		public VectorF[] LineLowerRight { get; protected set; }

		/// <summary>Whether or not to add a line from the last vertice to the first.</summary>
		public bool CloseLoop { get; set; }

		/// <summary>The visibility of each line.</summary>
		public bool[] LineVisible { get; protected set; }

		/// <summary>The time of the last check.</summary>
		public TimeSpan LastCheck { get; set; }

		/// <summary>The check number of the last check.</summary>
		public int LastCheckNum { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of GeoSkel.</summary>
		public GeoSkel()
		{
			clear();
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public virtual void clear()
		{
			ID = null;
			Vertices = null;
			UpperLeft = VectorF.Zero;
			LowerRight = VectorF.Zero;
			LineTopLeft = null;
			LineLowerRight = null;
			CloseLoop = true;
			LineVisible = null;
			LastCheck = TimeSpan.MinValue;
			LastCheckNum = 0;
		}

		/// <summary>Gets a skeleton version of this geo.</summary>
		/// <returns>A skeleton version of this geo.</returns>
		public GeoSkel getSkeleton()
		{
			GeoSkel skel = new GeoSkel();

			skel.ID = this.ID;
			skel.Vertices = (VectorF[])this.Vertices.Clone();
			skel.UpperLeft = this.UpperLeft;
			skel.LowerRight = this.LowerRight;
			skel.LineTopLeft = (VectorF[])this.LineTopLeft.Clone();
			skel.LineLowerRight = (VectorF[])this.LineLowerRight.Clone();
			skel.CloseLoop = this.CloseLoop;
			skel.LineVisible = (skel.LineVisible == null) ? null : (bool[])this.LineVisible.Clone();

			return skel;
		}

		/// <summary>Refreshes this geo's bounding box.</summary>
		public virtual void refreshMath()
		{
			// update bounding boxes
			LineTopLeft = new VectorF[Vertices.Length / 2 + (CloseLoop ? 1 : 0)];
			LineLowerRight = new VectorF[LineTopLeft.Length];
			Top = Vertices[0].Y;
			Bottom = Vertices[0].Y;
			Left = Vertices[0].X;
			Right = Vertices[0].X;

			for (int i = 1; i < Vertices.Length; i += 2)
			{
				// update main bounding box
				Top = Calc.Min(Top, Vertices[i].Y);
				Bottom = Calc.Max(Bottom, Vertices[i].Y);
				Left = Calc.Min(Left, Vertices[i].X);
				Right = Calc.Max(Right, Vertices[i].X);

				// update line bounding box
				int prevI = i - 1;
				int lbi = prevI / 2;
				LineTopLeft[lbi].X = Calc.Min(Vertices[prevI].X, Vertices[i].X);
				LineTopLeft[lbi].Y = Calc.Min(Vertices[prevI].Y, Vertices[i].Y);
				LineLowerRight[lbi].X = Calc.Max(Vertices[prevI].X, Vertices[i].X);
				LineLowerRight[lbi].Y = Calc.Max(Vertices[prevI].Y, Vertices[i].Y);
			}

			// get last line to close the loop
			if (CloseLoop)
			{
				int lastI = LineTopLeft.Length - 1;
				int lastV = Vertices.Length - 1;
				LineTopLeft[lastI].X = Calc.Min(Vertices[lastV].X, Vertices[0].X);
				LineTopLeft[lastI].Y = Calc.Min(Vertices[lastV].Y, Vertices[0].Y);
				LineLowerRight[lastI].X = Calc.Max(Vertices[lastV].X, Vertices[0].X);
				LineLowerRight[lastI].Y = Calc.Max(Vertices[lastV].Y, Vertices[0].Y);
			}
		}

		#endregion Methods
	}
}
