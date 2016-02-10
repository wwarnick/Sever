using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sever
{
	public struct VectorF : IEquatable<VectorF>
	{	
		#region Members

		public FInt X;

		public FInt Y;

		#region Static

		private static readonly VectorF one = new VectorF(FInt.F1);

		public static VectorF One { get { return one; } }

		private static readonly VectorF zero = new VectorF(FInt.F0);

		public static VectorF Zero { get { return zero; } }

		private static readonly VectorF unitX = new VectorF(FInt.F1, FInt.F0);

		public static VectorF UnitX { get { return unitX; } }

		private static readonly VectorF unitY = new VectorF(FInt.F0, FInt.F1);

		public static VectorF UnitY { get { return unitY; } }

		#endregion Static

		#endregion Members

		#region Constructors

		public VectorF(FInt value)
		{
			X = value;
			Y = value;
		}

		public VectorF(FInt x, FInt y)
		{
			X = x;
			Y = y;
		}

		#endregion Constructors

		#region Methods

		public FInt Length()
		{
			return Calc.Sqrt((X * X) + (Y * Y));
		}

		public FInt LengthSquared()
		{
			return (X * X) + (Y * Y);
		}

		public void Normalize()
		{
			FInt Len = Length();

			if (Len <= FInt.F0)
			{
				X = FInt.F0;
				Y = FInt.F0;
			}
			else
			{
				X /= Len;
				Y /= Len;
			}
		}

		bool IEquatable<VectorF>.Equals(VectorF other)
		{
			return this.X == other.X && this.Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			// TODO: optimize this

			try
			{
				VectorF p = (VectorF)obj;
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

		public static VectorF operator -(VectorF value)
		{
			return new VectorF(value.X.Inverse, value.Y.Inverse);
		}

		public static VectorF operator -(VectorF value1, VectorF value2)
		{
			return new VectorF(value1.X - value2.X, value1.Y - value2.Y);
		}

		public static bool operator !=(VectorF value1, VectorF value2)
		{
			return value1.X != value2.X || value1.Y != value2.Y;
		}

		public static VectorF operator *(FInt scaleFactor, VectorF value)
		{
			return new VectorF(value.X * scaleFactor, value.Y * scaleFactor);
		}

		public static VectorF operator *(VectorF value, FInt scaleFactor)
		{
			return new VectorF(value.X * scaleFactor, value.Y * scaleFactor);
		}

		public static VectorF operator *(VectorF value1, VectorF value2)
		{
			return new VectorF(value1.X * value2.X, value1.Y * value2.Y);
		}

		public static VectorF operator /(VectorF value1, FInt divider)
		{
			return new VectorF(value1.X / divider, value1.Y / divider);
		}

		public static VectorF operator /(VectorF value1, VectorF value2)
		{
			return new VectorF(value1.X / value2.X, value1.Y / value2.Y);
		}

		public static VectorF operator +(VectorF value1, VectorF value2)
		{
			return new VectorF(value1.X + value2.X, value1.Y + value2.Y);
		}

		public static bool operator ==(VectorF value1, VectorF value2)
		{
			return value1.X == value2.X && value1.Y == value2.Y;
		}

		public static explicit operator Vector2(VectorF value)
		{
			return new Vector2((float)value.X.ToDouble(), (float)value.Y.ToDouble());
		}

		public static explicit operator VectorF(Vector2 value)
		{
			return new VectorF(new FInt((double)value.X), new FInt((double)value.Y));
		}

		public static explicit operator VectorF(VectorD value)
		{
			return new VectorF((FInt)value.X, (FInt)value.Y);
		}

		public static explicit operator Vector3(VectorF value)
		{
			return new Vector3((float)value.X.ToDouble(), (float)value.Y.ToDouble(), 0f);
		}

		public static explicit operator VectorF(Vector3 value)
		{
			return new VectorF(new FInt((double)value.X), new FInt((double)value.Y));
		}

		#endregion Operators

		#region Static

		public static VectorF Add(VectorF value1, VectorF value2)
		{
			return new VectorF(value1.X + value2.X, value1.Y + value2.Y);
		}

		public static void Add(ref VectorF value1, ref VectorF value2, out VectorF result)
		{
			result = new VectorF(value1.X + value2.X, value1.Y + value2.Y);
		}

		public static FInt Distance(VectorF value1, VectorF value2)
		{
			FInt width = value2.X - value1.X;
			FInt height = value2.Y - value1.Y;

			return Calc.Sqrt((width * width) + (height * height));
		}

		public static void Distance(ref VectorF value1, ref VectorF value2, out FInt result)
		{
			FInt width = value2.X - value1.X;
			FInt height = value2.Y - value1.Y;

			result = Calc.Sqrt((width * width) + (height * height));
		}

		public static FInt DistanceSquared(VectorF value1, VectorF value2)
		{
			FInt width = value2.X - value1.X;
			FInt height = value2.Y - value1.Y;
			
			return (width * width) + (height * height);
		}

		public static void DistanceSquared(ref VectorF value1, ref VectorF value2, out FInt result)
		{
			FInt width = value2.X - value1.X;
			FInt height = value2.Y - value1.Y;

			result = (width * width) + (height * height);
		}

		public static FInt DistanceApprox(VectorF value1, VectorF value2)
		{
			FInt dx = Calc.Abs(value2.X - value1.X);
			FInt dy = Calc.Abs(value2.Y - value1.Y);
			FInt min, max, approx;

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

			approx = ( max * (FInt)1007 ) + ( min * (FInt)441 );
			if ( max < ( min << 4 ))
				approx -= ( max * (FInt)40 );

			// add 512 for proper rounding
			return (( approx + (FInt)512 ) >> 10 );
		}

		public static VectorF Divide(VectorF value1, FInt divider)
		{
			return new VectorF(value1.X / divider, value1.Y / divider);
		}

		public static VectorF Divide(VectorF value1, VectorF value2)
		{
			return new VectorF(value1.X / value2.X, value1.Y / value2.Y);
		}

		public static void Divide(ref VectorF value1, ref FInt divider, out VectorF result)
		{
			result = new VectorF(value1.X / divider, value1.Y / divider);
		}

		public static void Divide(ref VectorF value1, ref VectorF value2, out VectorF result)
		{
			result = new VectorF(value1.X / value2.X, value1.Y / value2.Y);
		}

		public static FInt Dot(VectorF value1, VectorF value2)
		{
			return (value1.X * value2.X) + (value1.Y * value2.Y);
		}

		public static void Dot(ref VectorF value1, ref VectorF value2, out FInt result)
		{
			result = (value1.X * value2.X) + (value1.Y * value2.Y);
		}

		public static VectorF Multiply(VectorF value1, FInt scaleFactor)
		{
			return new VectorF(value1.X * scaleFactor, value1.Y * scaleFactor);
		}

		public static VectorF Multiply(VectorF value1, VectorF value2)
		{
			return new VectorF(value1.X * value2.X, value1.Y * value2.Y);
		}

		public static void Multiply(ref VectorF value1, ref FInt scaleFactor, out VectorF result)
		{
			result = new VectorF(value1.X * scaleFactor, value1.Y * scaleFactor);
		}

		public static void Multiply(ref VectorF value1, ref VectorF value2, out VectorF result)
		{
			result = new VectorF(value1.X * value2.X, value1.Y * value2.Y);
		}

		public static VectorF Negate(VectorF value1)
		{
			return new VectorF(value1.X.Inverse, value1.Y.Inverse);
		}

		public static void Negate(ref VectorF value1, out VectorF result)
		{
			result = new VectorF(value1.X.Inverse, value1.Y.Inverse);
		}

		public static VectorF Normalize(VectorF value1)
		{
			value1.Normalize();
			return value1;
		}

		public static void Normalize(ref VectorF value1, out VectorF result)
		{
			FInt Len = value1.Length();
			result = new VectorF(value1.X / Len, value1.Y / Len);
		}

		public static VectorF Subtract(VectorF value1, VectorF value2)
		{
			return new VectorF(value1.X - value2.X, value1.Y - value2.Y);
		}

		public static void Subtract(ref VectorF value1, ref VectorF value2, out VectorF result)
		{
			result = new VectorF(value1.X - value2.X, value1.Y - value2.Y);
		}

		public static VectorF FromAngle(FInt angle)
		{
			return new VectorF(Calc.Cos(angle), -Calc.Sin(angle));
		}

		#endregion Static

		#endregion Methods
	}
}
