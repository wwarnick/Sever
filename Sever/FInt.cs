using Microsoft.Xna.Framework;
using System;

namespace Sever
{
	public struct FInt
	{
		public long RawValue;
		public const int SHIFT_AMOUNT = 12; //12 is 4096

		public const long One = 1 << SHIFT_AMOUNT;
		public const int OneI = 1 << SHIFT_AMOUNT;
		public static readonly FInt OneF = new FInt(1L, true);
		public static readonly FInt F0 = (FInt)0;
		public static readonly FInt F1 = (FInt)1;
		public static readonly FInt F2 = (FInt)2;
		public static readonly FInt F3 = (FInt)3;
		public static readonly FInt F4 = (FInt)4;
		public static readonly FInt F5 = (FInt)5;
		public static readonly FInt FN1 = (FInt)(-1);
		public static readonly FInt MaxValue = new FInt(long.MaxValue, false);
		public static readonly FInt MinValue = new FInt(long.MinValue, false);

		#region Constructors

		public FInt(long StartingRawValue, bool UseMultiple)
		{
			RawValue = StartingRawValue;
			if (UseMultiple)
				RawValue = RawValue << SHIFT_AMOUNT;
		}

		public FInt(double DoubleValue)
		{
			DoubleValue *= (double)One;
			RawValue = (int)Math.Round(DoubleValue);
		}

		#endregion

		public int IntValue
		{
			get { return (int)(this.RawValue >> SHIFT_AMOUNT); }
		}

		public int ToInt()
		{
			return (int)(this.RawValue >> SHIFT_AMOUNT);
		}

		public double ToDouble()
		{
			return (double)this.RawValue / (double)One;
		}

		public double DblValue
		{
			get { return (double)this.RawValue / (double)One; }
		}

		public FInt Inverse
		{
			get { return new FInt(-this.RawValue, false); }
		}

		#region FromParts
		/// <summary>
		/// Create a fixed-int number from parts.  For example, to create 1.5 pass in 1 and 500.
		/// </summary>
		/// <param name="PreDecimal">The number above the decimal.  For 1.5, this would be 1.</param>
		/// <param name="PostDecimal">The number below the decimal, to three digits.  
		/// For 1.5, this would be 500. For 1.005, this would be 5.</param>
		/// <returns>A fixed-int representation of the number parts</returns>
		public static FInt FromParts(int PreDecimal, int PostDecimal)
		{
			FInt f = new FInt(PreDecimal, true);
			if (PostDecimal != 0)
				f.RawValue += (new FInt(PostDecimal) / 1000).RawValue;

			return f;
		}
		#endregion

		#region *
		public static FInt operator *(FInt one, FInt other)
		{
			FInt fInt;
			fInt.RawValue = (one.RawValue * other.RawValue) >> SHIFT_AMOUNT;
			return fInt;
		}

		public static FInt operator *(FInt one, int multi)
		{
			return one * (FInt)multi;
		}

		public static FInt operator *(int multi, FInt one)
		{
			return one * (FInt)multi;
		}

		public static FInt operator -(FInt value)
		{
			return new FInt(-value.RawValue, false);
		}
		#endregion

		#region /
		public static FInt operator /(FInt one, FInt other)
		{
			FInt fInt;
			fInt.RawValue = (one.RawValue << SHIFT_AMOUNT) / (other.RawValue);
			return fInt;
		}

		public static FInt operator /(FInt one, int divisor)
		{
			return one / (FInt)divisor;
		}

		public static FInt operator /(int divisor, FInt one)
		{
			return (FInt)divisor / one;
		}
		#endregion

		#region %
		public static FInt operator %(FInt one, FInt other)
		{
			FInt fInt;
			fInt.RawValue = (one.RawValue) % (other.RawValue);
			return fInt;
		}

		public static FInt operator %(FInt one, int divisor)
		{
			return one % (FInt)divisor;
		}

		public static FInt operator %(int divisor, FInt one)
		{
			return (FInt)divisor % one;
		}
		#endregion

		#region +
		public static FInt operator +(FInt one, FInt other)
		{
			FInt fInt;
			fInt.RawValue = one.RawValue + other.RawValue;
			return fInt;
		}

		public static FInt operator +(FInt one, int other)
		{
			return one + (FInt)other;
		}

		public static FInt operator +(int other, FInt one)
		{
			return one + (FInt)other;
		}
		#endregion

		#region -
		public static FInt operator -(FInt one, FInt other)
		{
			FInt fInt;
			fInt.RawValue = one.RawValue - other.RawValue;
			return fInt;
		}

		public static FInt operator -(FInt one, int other)
		{
			return one - (FInt)other;
		}

		public static FInt operator -(int other, FInt one)
		{
			return (FInt)other - one;
		}
		#endregion

		#region ==
		public static bool operator ==(FInt one, FInt other)
		{
			return one.RawValue == other.RawValue;
		}

		public static bool operator ==(FInt one, int other)
		{
			return one == (FInt)other;
		}

		public static bool operator ==(int other, FInt one)
		{
			return (FInt)other == one;
		}
		#endregion

		#region !=
		public static bool operator !=(FInt one, FInt other)
		{
			return one.RawValue != other.RawValue;
		}

		public static bool operator !=(FInt one, int other)
		{
			return one != (FInt)other;
		}

		public static bool operator !=(int other, FInt one)
		{
			return (FInt)other != one;
		}
		#endregion

		#region >=
		public static bool operator >=(FInt one, FInt other)
		{
			return one.RawValue >= other.RawValue;
		}

		public static bool operator >=(FInt one, int other)
		{
			return one >= (FInt)other;
		}

		public static bool operator >=(int other, FInt one)
		{
			return (FInt)other >= one;
		}
		#endregion

		#region <=
		public static bool operator <=(FInt one, FInt other)
		{
			return one.RawValue <= other.RawValue;
		}

		public static bool operator <=(FInt one, int other)
		{
			return one <= (FInt)other;
		}

		public static bool operator <=(int other, FInt one)
		{
			return (FInt)other <= one;
		}
		#endregion

		#region >
		public static bool operator >(FInt one, FInt other)
		{
			return one.RawValue > other.RawValue;
		}

		public static bool operator >(FInt one, int other)
		{
			return one > (FInt)other;
		}

		public static bool operator >(int other, FInt one)
		{
			return (FInt)other > one;
		}
		#endregion

		#region <
		public static bool operator <(FInt one, FInt other)
		{
			return one.RawValue < other.RawValue;
		}

		public static bool operator <(FInt one, int other)
		{
			return one < (FInt)other;
		}

		public static bool operator <(int other, FInt one)
		{
			return (FInt)other < one;
		}
		#endregion

		public static explicit operator int(FInt src)
		{
			return (int)(src.RawValue >> SHIFT_AMOUNT);
		}

		public static explicit operator FInt(int src)
		{
			return new FInt(src, true);
		}

		public static explicit operator FInt(long src)
		{
			return new FInt(src, true);
		}

		public static explicit operator FInt(ulong src)
		{
			return new FInt((long)src, true);
		}

		public static explicit operator double(FInt src)
		{
			return (double)src.RawValue / (double)One;
		}

		public static explicit operator FInt(double src)
		{
			return new FInt(src);
		}

		public static explicit operator float(FInt src)
		{
			return (float)((double)src.RawValue / (double)One);
		}

		public static explicit operator FInt(float src)
		{
			return new FInt((double)src);
		}

		public static FInt operator <<(FInt one, int Amount)
		{
			return new FInt(one.RawValue << Amount, false);
		}

		public static FInt operator >>(FInt one, int Amount)
		{
			return new FInt(one.RawValue >> Amount, false);
		}

		public override bool Equals(object obj)
		{
			if (obj is FInt)
				return ((FInt)obj).RawValue == this.RawValue;
			else
				return false;
		}

		public override int GetHashCode()
		{
			return RawValue.GetHashCode();
		}

		public override string ToString()
		{
			return this.DblValue.ToString();
		}
	}

	public struct FPoint
	{
		public FInt X;
		public FInt Y;

		public static FPoint Create(FInt X, FInt Y)
		{
			FPoint fp;
			fp.X = X;
			fp.Y = Y;
			return fp;
		}

		public static FPoint FromPoint(Point p)
		{
			FPoint f;
			f.X = (FInt)p.X;
			f.Y = (FInt)p.Y;
			return f;
		}

		public static Point ToPoint(FPoint f)
		{
			return new Point(f.X.IntValue, f.Y.IntValue);
		}

		#region Vector Operations
		public static FPoint VectorAdd(FPoint F1, FPoint F2)
		{
			FPoint result;
			result.X = F1.X + F2.X;
			result.Y = F1.Y + F2.Y;
			return result;
		}

		public static FPoint VectorSubtract(FPoint F1, FPoint F2)
		{
			FPoint result;
			result.X = F1.X - F2.X;
			result.Y = F1.Y - F2.Y;
			return result;
		}

		public static FPoint VectorDivide(FPoint F1, int Divisor)
		{
			FPoint result;
			result.X = F1.X / Divisor;
			result.Y = F1.Y / Divisor;
			return result;
		}
		#endregion
	}
}
