﻿using OpenTK;
using System;

namespace BefunExec.View.OpenGL.OGLMath
{
	public class Vec2D
	{
		public static Vec2D Zero { get { return new Vec2D(); } private set { } }

		public double X;
		public double Y;

		public Vec2D()
		{
			X = 0;
			Y = 0;
		}

		public Vec2D(double pX, double pY)
		{
			X = pX;
			Y = pY;
		}

		public Vec2D(Vec2D v)
		{
			X = v.X;
			Y = v.Y;
		}

		#region Operators

		public static implicit operator Vector2d(Vec2D instance)
		{
			return new Vector2d(instance.X, instance.Y);
		}

		public static explicit operator Vec2I(Vec2D instance)
		{
			return new Vec2I((int)instance.X, (int)instance.Y);
		}

		public static Vec2D operator +(Vec2D v1, Vec2D v2)
		{
			return new Vec2D(v1.X + v2.X, v1.Y + v2.Y);
		}

		public static Vec2D operator +(Vec2D v1, double v2)
		{
			return new Vec2D(v1.X + v2, v1.Y + v2);
		}

		public static Vec2D operator -(Vec2D v1, Vec2D v2)
		{
			return new Vec2D(v1.X - v2.X, v1.Y - v2.Y);
		}

		public static Vec2D operator -(Vec2D v1, double v2)
		{
			return new Vec2D(v1.X - v2, v1.Y - v2);
		}

		public static Vec2D operator *(Vec2D v1, Vec2D v2)
		{
			return new Vec2D(v1.X * v2.X, v1.Y * v2.Y);
		}

		public static Vec2D operator *(Vec2D v1, double v2)
		{
			return new Vec2D(v1.X * v2, v1.Y * v2);
		}

		public static Vec2D operator /(Vec2D v1, Vec2D v2)
		{
			return new Vec2D(v1.X / v2.X, v1.Y / v2.Y);
		}

		public static Vec2D operator /(Vec2D v1, double v2)
		{
			return new Vec2D(v1.X / v2, v1.Y / v2);
		}

		public static Vec2D operator -(Vec2D v)
		{
			return new Vec2D(-v.X, -v.Y);
		}

		public static bool operator ==(Vec2D a, Vec2D b)
		{
			if ((object)a == null && (object)b == null)
				return true;

			if ((object)a == null || (object)b == null)
				return false;

			return (a.X == b.X && a.Y == b.Y);
		}

		public static bool operator !=(Vec2D a, Vec2D b)
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
				X = X / w;
				Y = Y / w;
			}
		}

		public void SetLength(double len)
		{
			Normalize();

			X = X * len;
			Y = Y * len;
		}

		public void DoMaxLength(double max)
		{
			SetLength(Math.Min(max, GetLength()));
		}

		public void DoMinLength(double min)
		{
			SetLength(Math.Max(min, GetLength()));
		}

		public void Set(double px, double py)
		{
			X = px;
			Y = py;
		}

		public void rotateAround(Vec2D centerPoint, double rads)
		{
			double cosTheta = Math.Cos(rads);
			double sinTheta = Math.Sin(rads);

			double nX = (cosTheta * (this.X - centerPoint.X) - sinTheta * (this.Y - centerPoint.Y) + centerPoint.X);
			double nY = (sinTheta * (this.X - centerPoint.X) + cosTheta * (this.Y - centerPoint.Y) + centerPoint.Y);

			X = nX;
			Y = nY;
		}

		public override string ToString()
		{
			return String.Format("({0}|{1})", X, Y);
		}
	}
}