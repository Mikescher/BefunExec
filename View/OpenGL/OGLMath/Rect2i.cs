﻿using System;

namespace BefunExec.View.OpenGL.OGLMath
{
	public class Rect2I
	{
		private Vec2I position; // bottomLeft
		public int Width { get; set; }
		public int Height { get; set; }

		public int Area { get { return Width * Height; } }

		public Vec2I tl { get { return new Vec2I(position.X, position.Y + Height); } }

		public Vec2I bl { get { return new Vec2I(position.X, position.Y); } }

		public Vec2I br { get { return new Vec2I(position.X + Width, position.Y); } }

		public Vec2I tr { get { return new Vec2I(position.X + Width, position.Y + Height); } }

		public Rect2I(int bl_x, int bl_y, int pwidth, int pheight)
		{
			position = new Vec2I(bl_x, bl_y);
			Width = pwidth;
			Height = pheight;
		}

		public Rect2I(Vec2I bottomleft, int pwidth, int pheight)
		{
			position = new Vec2I(bottomleft);
			Width = pwidth;
			Height = pheight;
		}

		public Rect2I(Vec2I bottomleft, int psize)
		{
			position = new Vec2I(bottomleft);
			Width = psize;
			Height = psize;
		}

		public Rect2I(Vec2I bottomleft, Vec2I topRight)
		{
			position = new Vec2I(bottomleft);
			Width = topRight.X - bottomleft.X;
			Height = topRight.Y - bottomleft.Y;
		}

		public Rect2I(Rect2I r)
		{
			position = new Vec2I(r.position);
			Width = r.Width;
			Height = r.Height;
		}

		#region Operators

		public static implicit operator Rect2D(Rect2I instance)
		{
			return new Rect2D(instance.position, instance.Width, instance.Height);
		}

		public static Rect2I operator +(Rect2I v1, Vec2I v2)
		{
			return new Rect2I(v1.position + v2, v1.Width, v1.Height);
		}

		public static Rect2I operator +(Rect2I v1, int v2)
		{
			return new Rect2I(v1.position + v2, v1.Width, v1.Height);
		}

		public static Rect2I operator -(Rect2I v1, Vec2I v2)
		{
			return new Rect2I(v1.position - v2, v1.Width, v1.Height);
		}

		public static Rect2I operator -(Rect2I v1, int v2)
		{
			return new Rect2I(v1.position - v2, v1.Width, v1.Height);
		}

		public static Rect2I operator *(Rect2I v1, Vec2I v2)
		{
			return new Rect2I(v1.position * v2, v1.Width * v2.X, v1.Height * v2.Y);
		}

		public static Rect2I operator *(Rect2I v1, int v2)
		{
			return new Rect2I(v1.position * v2, v1.Width * v2, v1.Height * v2);
		}

		public static Rect2I operator /(Rect2I v1, Vec2I v2)
		{
			return new Rect2I(v1.position / v2, v1.Width / v2.X, v1.Height / v2.Y);
		}

		public static Rect2I operator /(Rect2I v1, int v2)
		{
			return new Rect2I(v1.position / v2, v1.Width / v2, v1.Height / v2);
		}

		public static bool operator ==(Rect2I a, Rect2I b)
		{
			if ((object)a == null && (object)b == null)
				return true;

			if ((object)a == null || (object)b == null)
				return false;

			return (a.position == b.position && a.Width == b.Width && a.Height == b.Height);
		}

		public static bool operator !=(Rect2I a, Rect2I b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj is Rect2I)
				return this == (Rect2I)obj;
			return false;
		}

		public override int GetHashCode()
		{
			return position.GetHashCode() + Width.GetHashCode() + Height.GetHashCode();
		}

		#endregion Operators

		public Vec2D GetMiddle()
		{
			return new Vec2D(position.X + Width / 2.0, position.Y + Height / 2.0);
		}

		public bool IsColldingWith(Rect2I rect)
		{
			return !(this.tl.X >= rect.br.X || this.br.X <= rect.tl.X || this.tl.Y <= rect.br.Y || this.br.Y >= rect.tl.Y);
		}

		public bool IsTouching(Rect2I rect)
		{
			return (this.tl.X == rect.br.X && this.tl.X > rect.tl.X) ||
				   (this.br.X == rect.tl.X && this.br.X < rect.br.X) ||
				   (this.tl.Y == rect.br.Y && this.tl.Y < rect.tl.Y) ||
				   (this.br.Y == rect.tl.Y && this.br.Y > rect.br.Y);
		}

		public Vec2I GetDistanceTo(Rect2I rect)
		{
			int vecX;
			int vecY;

			if (rect.br.X < this.tl.X)
				vecX = rect.br.X - this.tl.X;
			else if (rect.tl.X > this.br.X)
				vecX = rect.tl.X - this.br.X;
			else
				vecX = 0;

			if (rect.br.Y > this.tl.Y)
				vecY = rect.br.Y - this.tl.Y;
			else if (rect.tl.Y < this.br.Y)
				vecY = rect.tl.Y - this.br.Y;
			else
				vecY = 0;

			return new Vec2I(vecX, vecY);
		}

		public void TrimNorth(int len)
		{
			Height -= len;
		}

		public void TrimEast(int len)
		{
			Width -= len;
		}

		public void TrimSouth(int len)
		{
			position.Y += len;
			Height -= len;
		}

		public void TrimWest(int len)
		{
			position.X += len;
			Width -= len;
		}

		public void TrimHorizontal(int len)
		{
			TrimEast(len);
			TrimWest(len);
		}

		public void TrimVertical(int len)
		{
			TrimNorth(len);
			TrimSouth(len);
		}

		public void Trim(int len)
		{
			TrimHorizontal(len);
			TrimVertical(len);
		}

		public bool Includes(Vec2I vec)
		{
			return (vec.X > position.X && vec.Y > position.Y && vec.X < tl.X && vec.Y < tl.Y);
		}

		public Vec2I GetDistanceTo(Vec2I vec)
		{
			Vec2I result = Vec2I.Zero;

			if (vec.X < position.X)
			{
				result.X = vec.X - position.X;
			}
			else if (vec.X > tl.X)
			{
				result.X = vec.X - tl.X;
			}

			if (vec.Y < position.Y)
			{
				result.Y = vec.Y - position.Y;
			}
			else if (vec.Y > tl.Y)
			{
				result.Y = vec.Y - tl.Y;
			}

			return result;
		}

		public void Normalize()
		{
			if (Width < 0)
			{
				position.X += Width;
				Width *= -1;
			}

			if (Height < 0)
			{
				position.Y += Height;
				Height *= -1;
			}
		}

		/// <summary>
		/// Trims the rect in a way that its fully contained in the other rect
		/// </summary>
		public void ForceInside(Rect2I other)
		{
			if (this.bl.X < other.bl.X)
				this.TrimWest(other.bl.X - this.bl.X);

			if (this.bl.Y < other.bl.Y)
				this.TrimSouth(other.bl.Y - this.bl.Y);

			if (this.tr.X > other.tr.X)
				this.TrimEast(this.tr.X - other.tr.X);

			if (this.tr.Y > other.tr.Y)
				this.TrimNorth(this.tr.Y - other.tr.Y);
		}

		public void ForceTranslateInside(Rect2I other)
		{
			if (this.Width > other.Width)
			{
				this.position.X = other.position.X;
				this.Width = other.Width;
			}
			else
			{
				if (this.position.X < other.position.X)
					this.position.X = other.position.X;

				if (this.position.X + Width > other.position.X + other.Width)
					this.position.X = other.position.X + other.Width - this.Width;
			}

			if (this.Height > other.Height)
			{
				this.position.Y = other.position.Y;
				this.Height = other.Height;
			}
			else
			{
				if (this.position.Y < other.position.Y)
					this.position.Y = other.position.Y;

				if (this.position.Y + Height > other.position.Y + other.Height)
					this.position.Y = other.position.Y + other.Height - this.Height;
			}

		}

		public double GetRatio()
		{
			return Width / (Height * 1.0);
		}

		public void setRatio_Expanding(double ratio)
		{
			double real_r = GetRatio();

			if (real_r < ratio) // Expand Width
			{
				while (real_r < ratio)
				{
					TrimHorizontal(-1);
					real_r = GetRatio();
				}
				TrimHorizontal(1);
			}
			else // Expand Height
			{
				while (real_r > ratio)
				{
					TrimVertical(-1);
					real_r = GetRatio();
				}
				TrimVertical(1);
			}
		}

		public void setInsideRatio_Expanding(double ratio, Rect2I other)
		{
			double real_r = GetRatio();

			if (real_r < ratio) // Expand Width
			{
				double old = real_r;
				while (real_r < ratio)
				{
					TrimHorizontal(-1);
					ForceInside(other);
					real_r = GetRatio();

					if (old == real_r)
						return;
					old = real_r;
				}
			}
			else // Expand Height
			{
				double old = real_r;
				while (real_r > ratio)
				{
					TrimVertical(-1);
					ForceInside(other);
					real_r = GetRatio();

					if (old == real_r)
						return;
					old = real_r;
				}
			}
		}

		public void Move(int xx, int yy)
		{
			position.X += xx;
			position.Y += yy;
		}

		public override string ToString()
		{
			return String.Format(@"({0}-{1}|{2}-{3}) :: {4}x{5}", bl.X, tr.X, bl.Y, tr.Y, Width, Height);
		}
	}
}