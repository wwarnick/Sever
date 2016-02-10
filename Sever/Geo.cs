using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public class Geo : GeoSkel
	{
		#region Members

		/// <summary>The world that this geo is in.</summary>
		public World InWorld { get; set; }

		/// <summary>A triangle list of the triangles that make up this geometric object.</summary>
		public Vector3[] Triangles { get; private set; }

		/// <summary>Companion to Triangles.  Specifies the texture coordinates.</summary>
		public Vector2[] TexCoords { get; private set; }

		/// <summary>The center of this geo.</summary>
		public VectorF Center { get; private set; }

		/// <summary>Whether or not this Geo should be displayed as a geometric shape.</summary>
		public bool Display { get; set; }

		#endregion Members

		#region Constructors

		/// <summary>Creates a new instance of Geo.</summary>
		public Geo(World inWorld)
		{
			clear();
			InWorld = inWorld;
			Display = true;
		}

		#endregion Constructors

		#region Methods

		/// <summary>Resets all values to their defaults.</summary>
		public override void clear()
		{
			base.clear();

			Triangles = null;
			TexCoords = null;
		}

		/// <summary>Refreshes this geo's bounding box.</summary>
		public override void refreshMath()
		{
			base.refreshMath();

			// find center

			double totalX = 0d;
			double totalY = 0d;
			for (int i = 0; i < Vertices.Length; i += 2)
			{
				totalX += (double)Vertices[i].X;
				totalY += (double)Vertices[i].Y;
			}

			double quantity = Vertices.Length / 2;
			if (Vertices.Length > 1)
			{
				int last = Vertices.Length - 1;
				totalX += (double)Vertices[last].X;
				totalY += (double)Vertices[last].Y;

				quantity++;
			}
			VectorF center = new VectorF((FInt)(totalX / quantity), (FInt)(totalY / quantity));
			Center = center;
		}

		/// <summary>Refreshes the triangles that make up this geometric obstacle and its bounding box.</summary>
		/// <param name="textureSize">The size of the texture to tile across this geometric obstacle.</param>
		public void refreshMath(Vector2 textureSize)
		{
			// refresh triangles
			if (Display)
				Triangles = Calc.getTriangles(Vertices);

			if (Display && Triangles != null)
			{
				TexCoords = new Vector2[Triangles.Length];
				for (int i = 0; i < Triangles.Length; i++)
				{
					TexCoords[i] = new Vector2(Triangles[i].X / textureSize.X, Triangles[i].Y / textureSize.Y);
				}
			}
			else
			{
				Triangles = null;
				TexCoords = null;
			}

			refreshMath();
		}

		/// <summary>Gets a list of all triangles in view.</summary>
		/// <param name="camera">The camera to test against.</param>
		/// <returns>A list of all triangles in view.</returns>
		public List<VertexPositionTexture> getTriangles(Camera camera)
		{
			if (Triangles == null)
				return new List<VertexPositionTexture>(0);

			List<VertexPositionTexture> tris = new List<VertexPositionTexture>(Triangles.Length);

			Vector2[] tri = new Vector2[3];

			for (int i = 0; i < Triangles.Length; i += 3)
			{
				int i1 = i + 1;
				int i2 = i + 2;

				Vector3 tri0 = Triangles[i];
				Vector3 tri1 = Triangles[i1];
				Vector3 tri2 = Triangles[i2];

				// if triangle is in view of camera, add it
				/*if (camera.inView(tri0)
					|| camera.inView(tri1)
					|| camera.inView(tri2))*/
				{
					tris.AddRange(new VertexPositionTexture[]
					{
						new VertexPositionTexture(camera.worldToScreenDraw(tri0), TexCoords[i]),
						new VertexPositionTexture(camera.worldToScreenDraw(tri1), TexCoords[i1]),
						new VertexPositionTexture(camera.worldToScreenDraw(tri2), TexCoords[i2])
					});
				}
			}

			return tris;
		}

		#endregion Methods
	}
}
