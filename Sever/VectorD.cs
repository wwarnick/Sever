using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public struct VectorD : IEquatable<VectorD>
	{	
		#region Members

		public double X;

		public double Y;

		#region Static

		private static readonly VectorD one = new VectorD(1d);

		public static VectorD One { get { return one; } }

		private static readonly VectorD zero = new VectorD(0d);

		public static VectorD Zero { get { return zero; } }

		private static readonly VectorD unitX = new VectorD(1d, 0d);

		public static VectorD UnitX { get { return unitX; } }

		private static readonly VectorD unitY = new VectorD(0d, 1d);

		public static VectorD UnitY { get { return unitY; } }

		#endregion Static

		#endregion Members

		#region Constructors

		public VectorD(double value)
		{
			X = value;
			Y = value;
		}

		public VectorD(double x, double y)
		{
			X = x;
			Y = y;
		}

		#endregion Constructors

		#region Methods

		public double Length()
		{
			return Math.Sqrt((X * X) + (Y * Y));
		}

		public double LengthSquared()
		{
			return (X * X) + (Y * Y);
		}

		public void Normalize()
		{
			double Len = Length();

			if (Len <= 0d)
			{
				X = 0d;
				Y = 0d;
			}
			else
			{
				X /= Len;
				Y /= Len;
			}
		}

		bool IEquatable<VectorD>.Equals(VectorD other)
		{
			return this.X == other.X && this.Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			// TODO: optimize this

			try
			{
				VectorD p = (VectorD)obj;
				return X == p.X && Y == p.Y;
			}
			catch { }

			return false;
		}

		public override string ToString()
		{
			return X.ToString() + ", " + Y.ToString();
		}

		#region Operators

		public static VectorD operator -(VectorD value)
		{
			return new VectorD(-value.X, -value.Y);
		}

		public static VectorD operator -(VectorD value1, VectorD value2)
		{
			return new VectorD(value1.X - value2.X, value1.Y - value2.Y);
		}

		public static bool operator !=(VectorD value1, VectorD value2)
		{
			return value1.X != value2.X || value1.Y != value2.Y;
		}

		public static VectorD operator *(double scaleFactor, VectorD value)
		{
			return new VectorD(value.X * scaleFactor, value.Y * scaleFactor);
		}

		public static VectorD operator *(VectorD value, double scaleFactor)
		{
			return new VectorD(value.X * scaleFactor, value.Y * scaleFactor);
		}

		public static VectorD operator *(VectorD value1, VectorD value2)
		{
			return new VectorD(value1.X * value2.X, value1.Y * value2.Y);
		}

		public static VectorD operator /(VectorD value1, double divider)
		{
			return new VectorD(value1.X / divider, value1.Y / divider);
		}

		public static VectorD operator /(VectorD value1, VectorD value2)
		{
			return new VectorD(value1.X / value2.X, value1.Y / value2.Y);
		}

		public static VectorD operator +(VectorD value1, VectorD value2)
		{
			return new VectorD(value1.X + value2.X, value1.Y + value2.Y);
		}

		public static bool operator ==(VectorD value1, VectorD value2)
		{
			return value1.X == value2.X && value1.Y == value2.Y;
		}

		public static explicit operator Vector2(VectorD value)
		{
			return new Vector2((float)value.X, (float)value.Y);
		}

		public static explicit operator VectorD(Vector2 value)
		{
			return new VectorD((double)value.X, (double)value.Y);
		}

		public static explicit operator VectorD(VectorF value)
		{
			return new VectorD((double)value.X, (double)value.Y);
		}

		public static explicit operator Vector3(VectorD value)
		{
			return new Vector3((float)value.X, (float)value.Y, 0f);
		}

		public static explicit operator VectorD(Vector3 value)
		{
			return new VectorD((double)value.X, (double)value.Y);
		}

		#endregion Operators

		#region Static

		public static VectorD Add(VectorD value1, VectorD value2)
		{
			return new VectorD(value1.X + value2.X, value1.Y + value2.Y);
		}

		public static void Add(ref VectorD value1, ref VectorD value2, out VectorD result)
		{
			result = new VectorD(value1.X + value2.X, value1.Y + value2.Y);
		}

		public static double Distance(VectorD value1, VectorD value2)
		{
			double width = value2.X - value1.X;
			double height = value2.Y - value1.Y;

			return Math.Sqrt((width * width) + (height * height));
		}

		public static void Distance(ref VectorD value1, ref VectorD value2, out double result)
		{
			double width = value2.X - value1.X;
			double height = value2.Y - value1.Y;

			result = Math.Sqrt((width * width) + (height * height));
		}

		public static double DistanceSquared(VectorD value1, VectorD value2)
		{
			double width = value2.X - value1.X;
			double height = value2.Y - value1.Y;
			
			return (width * width) + (height * height);
		}

		public static void DistanceSquared(ref VectorD value1, ref VectorD value2, out double result)
		{
			double width = value2.X - value1.X;
			double height = value2.Y - value1.Y;

			result = (width * width) + (height * height);
		}

		public static double DistanceApprox(VectorD value1, VectorD value2)
		{
			double dx = Math.Abs(value2.X - value1.X);
			double dy = Math.Abs(value2.Y - value1.Y);
			double min, max, approx;

			if ( dx < dy )
			{
				min = dx;
				max = dy;
			}
			else
			{
				min = dy;
				max = dx;
			}

			approx = (max * 1007d) + (min * 441d);
			if (max < (min / 16d))
				approx -= (max * 40d);

			// add 512 for proper rounding
			return (approx + 512d) *1024d;
		}

		public static VectorD Divide(VectorD value1, double divider)
		{
			return new VectorD(value1.X / divider, value1.Y / divider);
		}

		public static VectorD Divide(VectorD value1, VectorD value2)
		{
			return new VectorD(value1.X / value2.X, value1.Y / value2.Y);
		}

		public static void Divide(ref VectorD value1, ref double divider, out VectorD result)
		{
			result = new VectorD(value1.X / divider, value1.Y / divider);
		}

		public static void Divide(ref VectorD value1, ref VectorD value2, out VectorD result)
		{
			result = new VectorD(value1.X / value2.X, value1.Y / value2.Y);
		}

		public static double Dot(VectorD value1, VectorD value2)
		{
			return (value1.X * value2.X) + (value1.Y * value2.Y);
		}

		public static void Dot(ref VectorD value1, ref VectorD value2, out double result)
		{
			result = (value1.X * value2.X) + (value1.Y * value2.Y);
		}

		public static VectorD Multiply(VectorD value1, double scaleFactor)
		{
			return new VectorD(value1.X * scaleFactor, value1.Y * scaleFactor);
		}

		public static VectorD Multiply(VectorD value1, VectorD value2)
		{
			return new VectorD(value1.X * value2.X, value1.Y * value2.Y);
		}

		public static void Multiply(ref VectorD value1, ref double scaleFactor, out VectorD result)
		{
			result = new VectorD(value1.X * scaleFactor, value1.Y * scaleFactor);
		}

		public static void Multiply(ref VectorD value1, ref VectorD value2, out VectorD result)
		{
			result = new VectorD(value1.X * value2.X, value1.Y * value2.Y);
		}

		public static VectorD Negate(VectorD value1)
		{
			return new VectorD(-value1.X, -value1.Y);
		}

		public static void Negate(ref VectorD value1, out VectorD result)
		{
			result = new VectorD(-value1.X, -value1.Y);
		}

		public static VectorD Normalize(VectorD value1)
		{
			value1.Normalize();
			return value1;
		}

		public static void Normalize(ref VectorD value1, out VectorD result)
		{
			double Len = value1.Length();
			result = new VectorD(value1.X / Len, value1.Y / Len);
		}

		public static VectorD Subtract(VectorD value1, VectorD value2)
		{
			return new VectorD(value1.X - value2.X, value1.Y - value2.Y);
		}

		public static void Subtract(ref VectorD value1, ref VectorD value2, out VectorD result)
		{
			result = new VectorD(value1.X - value2.X, value1.Y - value2.Y);
		}

		public static VectorD FromAngle(double angle)
		{
			return new VectorD(Math.Cos(angle), -Math.Sin(angle));
		}

		#endregion Static

		#endregion Methods
	}
}
