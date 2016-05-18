namespace BefunExec.View.OpenGL.OGLMath
{
	public class Rect2D
	{
		private Vec2D position; // bottomLeft
		public double Width { get; private set; }
		public double Height { get; private set; }

		public Vec2D tl { get { return new Vec2D(position.X, position.Y + Height); } }

		public Vec2D bl { get { return new Vec2D(position.X, position.Y); } }

		public Vec2D br { get { return new Vec2D(position.X + Width, position.Y); } }

		public Vec2D tr { get { return new Vec2D(position.X + Width, position.Y + Height); } }

		public Rect2D(double bl_x, double bl_y, double pwidth, double pheight)
		{
			position = new Vec2D(bl_x, bl_y);
			Width = pwidth;
			Height = pheight;
		}

		public Rect2D(Vec2D bottomleft, double pwidth, double pheight)
		{
			position = new Vec2D(bottomleft);
			Width = pwidth;
			Height = pheight;
		}

		public Rect2D(Vec2D bottomleft, double psize)
		{
			position = new Vec2D(bottomleft);
			Width = psize;
			Height = psize;
		}

		public Rect2D(Vec2D bottomleft, Vec2D topRight)
		{
			position = new Vec2D(bottomleft);
			Width = topRight.X - bottomleft.X;
			Height = topRight.Y - bottomleft.Y;
		}

		public Rect2D(Rect2D r)
		{
			position = new Vec2D(r.position);
			Width = r.Width;
			Height = r.Height;
		}

		#region Operators

		public static explicit operator Rect2I(Rect2D instance)
		{
			return new Rect2I((Vec2I)instance.position, (int)instance.Width, (int)instance.Height);
		}

		public static Rect2D operator +(Rect2D v1, Vec2D v2)
		{
			return new Rect2D(v1.position + v2, v1.Width, v1.Height);
		}

		public static Rect2D operator +(Rect2D v1, double v2)
		{
			return new Rect2D(v1.position + v2, v1.Width, v1.Height);
		}

		public static Rect2D operator -(Rect2D v1, Vec2D v2)
		{
			return new Rect2D(v1.position - v2, v1.Width, v1.Height);
		}

		public static Rect2D operator -(Rect2D v1, double v2)
		{
			return new Rect2D(v1.position - v2, v1.Width, v1.Height);
		}

		public static Rect2D operator *(Rect2D v1, Vec2D v2)
		{
			return new Rect2D(v1.position * v2, v1.Width * v2.X, v1.Height * v2.Y);
		}

		public static Rect2D operator *(Rect2D v1, double v2)
		{
			return new Rect2D(v1.position * v2, v1.Width * v2, v1.Height * v2);
		}

		public static Rect2D operator /(Rect2D v1, Vec2D v2)
		{
			return new Rect2D(v1.position / v2, v1.Width / v2.X, v1.Height / v2.Y);
		}

		public static Rect2D operator /(Rect2D v1, double v2)
		{
			return new Rect2D(v1.position / v2, v1.Width / v2, v1.Height / v2);
		}

		public static bool operator ==(Rect2D a, Rect2D b)
		{
			if ((object)a == null && (object)b == null)
				return true;

			if ((object)a == null || (object)b == null)
				return false;

			return (a.position == b.position && a.Width == b.Width && a.Height == b.Height);
		}

		public static bool operator !=(Rect2D a, Rect2D b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj is Rect2D)
				return this == (Rect2D)obj;
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

		public bool IsColldingWith(Rect2D rect)
		{
			return !(this.tl.X >= rect.br.X || this.br.X <= rect.tl.X || this.tl.Y <= rect.br.Y || this.br.Y >= rect.tl.Y);
		}

		public bool IsTouching(Rect2D rect)
		{
			return (this.tl.X == rect.br.X && this.tl.X > rect.tl.X) ||
				   (this.br.X == rect.tl.X && this.br.X < rect.br.X) ||
				   (this.tl.Y == rect.br.Y && this.tl.Y < rect.tl.Y) ||
				   (this.br.Y == rect.tl.Y && this.br.Y > rect.br.Y);
		}

		public Vec2D GetDistanceTo(Rect2D rect)
		{
			double vecX;
			double vecY;

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

			return new Vec2D(vecX, vecY);
		}

		public void TrimNorth(double len)
		{
			Height -= len;
		}

		public void TrimEast(double len)
		{
			Width -= len;
		}

		public void TrimSouth(double len)
		{
			position.Y += len;
			Height -= len;
		}

		public void TrimWest(double len)
		{
			position.X += len;
			Width -= len;
		}

		public void TrimHorizontal(double len)
		{
			TrimEast(len);
			TrimWest(len);
		}

		public void TrimVertical(double len)
		{
			TrimNorth(len);
			TrimSouth(len);
		}

		public void Trim(double len)
		{
			TrimHorizontal(len);
			TrimVertical(len);
		}

		public bool Includes(Vec2D vec)
		{
			return (vec.X > position.X && vec.Y > position.Y && vec.X < tr.X && vec.Y < tr.Y);
		}

		public Vec2D GetDistanceTo(Vec2D vec)
		{
			Vec2D result = Vec2D.Zero;

			if (vec.X < position.X)
			{
				result.X = vec.X - position.X;
			}
			else if (vec.X > tr.X)
			{
				result.X = vec.X - tr.X;
			}

			if (vec.Y < position.Y)
			{
				result.Y = vec.Y - position.Y;
			}
			else if (vec.Y > tr.Y)
			{
				result.Y = vec.Y - tr.Y;
			}

			return result;
		}

		public void FlipXAxis()
		{
			position.X += Width;
			Width *= -1;
		}

		public void FlipYAxis()
		{
			position.Y += Height;
			Height *= -1;
		}
	}
}