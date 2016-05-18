using OpenTK;
using System;

namespace BefunExec.View.OpenGL.OGLMath
{
	public class Vec2I
	{
		public static Vec2I Zero { get { return new Vec2I(); } private set { } }

		public int X;
		public int Y;

		public Vec2I()
		{
			X = 0;
			Y = 0;
		}

		public Vec2I(int pX, int pY)
		{
			X = pX;
			Y = pY;
		}

		public Vec2I(Vec2I v)
		{
			X = v.X;
			Y = v.Y;
		}

		#region Operators

		public static implicit operator Vector2d(Vec2I instance)
		{
			return new Vector2d(instance.X, instance.Y);
		}

		public static implicit operator Vec2D(Vec2I instance)
		{
			return new Vec2D(instance.X, instance.Y);
		}

		public static Vec2I operator +(Vec2I v1, Vec2I v2)
		{
			return new Vec2I(v1.X + v2.X, v1.Y + v2.Y);
		}

		public static Vec2I operator +(Vec2I v1, int v2)
		{
			return new Vec2I(v1.X + v2, v1.Y + v2);
		}

		public static Vec2I operator -(Vec2I v1, Vec2I v2)
		{
			return new Vec2I(v1.X - v2.X, v1.Y - v2.Y);
		}

		public static Vec2I operator -(Vec2I v1, int v2)
		{
			return new Vec2I(v1.X - v2, v1.Y - v2);
		}

		public static Vec2I operator *(Vec2I v1, Vec2I v2)
		{
			return new Vec2I(v1.X * v2.X, v1.Y * v2.Y);
		}

		public static Vec2I operator *(Vec2I v1, int v2)
		{
			return new Vec2I(v1.X * v2, v1.Y * v2);
		}

		public static Vec2I operator /(Vec2I v1, Vec2I v2)
		{
			return new Vec2I(v1.X / v2.X, v1.Y / v2.Y);
		}

		public static Vec2I operator /(Vec2I v1, int v2)
		{
			return new Vec2I(v1.X / v2, v1.Y / v2);
		}

		public static Vec2I operator -(Vec2I v)
		{
			return new Vec2I(-v.X, -v.Y);
		}

		public static Vec2I operator %(Vec2I v1, Vec2I v2)
		{
			return new Vec2I(v1.X % v2.X, v1.Y % v2.Y);
		}

		public static Vec2I operator %(Vec2I v1, int v2)
		{
			return new Vec2I(v1.X % v2, v1.Y % v2);
		}

		public static bool operator ==(Vec2I a, Vec2I b)
		{
			if ((object)a == null && (object)b == null)
				return true;

			if ((object)a == null || (object)b == null)
				return false;

			return (a.X == b.X && a.Y == b.Y);
		}

		public static bool operator !=(Vec2I a, Vec2I b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj is Vec2D)
				return this == (Vec2D)obj;
			return false;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() + Y.GetHashCode();
		}

		#endregion Operators

		public double GetLength()
		{
			return Math.Sqrt(X * X + Y * Y);
		}

		public bool isZero()
		{
			return X == 0 && Y == 0;
		}

		public void Normalize()
		{
			if (!isZero())
			{
				double w = GetLength();
				X = (int)(X / w);
				Y = (int)(Y / w);
			}
		}

		public void SetLength(double len)
		{
			if (!isZero())
			{
				double w = GetLength();
				X = (int)((X / w) * len);
				Y = (int)((Y / w) * len);
			}
		}

		public void DoMaxLength(double max)
		{
			SetLength(Math.Min(max, GetLength()));
		}

		public void DoMinLength(double min)
		{
			SetLength(Math.Max(min, GetLength()));
		}

		public void Set(int px, int py)
		{
			X = px;
			Y = py;
		}

		public void rotateAround(Vec2I centerPoint, double rads)
		{
			double cosTheta = Math.Cos(rads);
			double sinTheta = Math.Sin(rads);

			double nX = (cosTheta * (this.X - centerPoint.X) - sinTheta * (this.Y - centerPoint.Y) + centerPoint.X);
			double nY = (sinTheta * (this.X - centerPoint.X) + cosTheta * (this.Y - centerPoint.Y) + centerPoint.Y);

			X = (int)nX;
			Y = (int)nY;
		}

		public override string ToString()
		{
			return String.Format("({0}|{1})", X, Y);
		}
	}
}