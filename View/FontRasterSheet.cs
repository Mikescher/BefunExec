using BefunExec.Properties;
using BefunExec.View.OpenGL;
using BefunExec.View.OpenGL.OGLMath;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace BefunExec.View
{
	public class FontRasterSheet : OGLTextureSheet
	{
		private Color[,] lqTable;

		protected FontRasterSheet(int id, int w, int h, Bitmap b)
			: base(id, w, h)
		{
			lqTable = new Color[w, h];

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					Rect2I r = new Rect2I(b.Width / w * x, b.Height / h * y, b.Width / w, b.Height / h);

					for (int tx = r.bl.X; tx < r.tr.X; tx++)
					{
						for (int ty = r.bl.Y; ty < r.tr.Y; ty++)
						{
							Color c = b.GetPixel(tx, ty);
							if (c.R + c.G + c.B != (255 * 3))
							{
								lqTable[x, y] = c;

								goto FinInnerLoop;
							}
						}
					}
					lqTable[x, y] = Color.White;

				FinInnerLoop:
					NOP();
				}
			}
		}

		private void NOP() { }

		public static FontRasterSheet Create(bool color, Color repl, Color background)
		{
			Bitmap b = new Bitmap(Resources.raster);
			b = b.Clone(new Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			if (!color || background != Color.White)
			{
				for (int x = 0; x < b.Width; x++)
				{
					for (int y = 0; y < b.Height; y++)
					{
						Color c = b.GetPixel(x, y);
						if (c.R + c.G + c.B != (255 * 3)) // Foreground
						{
							b.SetPixel(x, y, repl);
						}
						else // Background
						{
							b.SetPixel(x, y, background);
						}
					}
				}
			}

			return new FontRasterSheet(LoadResourceIntoUID(b, TextureMinFilter.Nearest), 80, 2, b);
		}

		public Rect2D GetCharCoords(long c)
		{
			if (!(c >= 0 && 126 >= c))
			{
				if (c == 164)
					c = 158;
				else
					c = 159;
			}

			return GetCoordinates(c);
		}

		public Vec2I GetCharPos(long c)
		{
			if (!(c >= 0 && 126 >= c))
			{
				if (c == 164)
					c = 158;
				else
					c = 159;
			}

			return GetPosition(c);
		}

		public void Render(Rect2D rect, double distance, long chr)
		{
			Rect2D coords = GetCharCoords(chr);

			//##########
			GL.Begin(PrimitiveType.Quads);
			//##########

			GL.TexCoord2(coords.bl);
			GL.Vertex3(rect.tl.X, rect.tl.Y, distance);

			GL.TexCoord2(coords.tl);
			GL.Vertex3(rect.bl.X, rect.bl.Y, distance);

			GL.TexCoord2(coords.tr);
			GL.Vertex3(rect.br.X, rect.br.Y, distance);

			GL.TexCoord2(coords.br);
			GL.Vertex3(rect.tr.X, rect.tr.Y, distance);

			//##########
			GL.End();
			//##########
		}

		public void RenderLQ(bool col, Rect2D rect, double distance, long chr)
		{
			Vec2I pos = GetCharPos(chr);

			//##########
			//GL.Begin(PrimitiveType.Quads);
			//##########

			if (col)
				GL.Color3(lqTable[pos.X, pos.Y]);

			GL.Vertex3(rect.tl.X, rect.tl.Y, distance);

			GL.Vertex3(rect.bl.X, rect.bl.Y, distance);

			GL.Vertex3(rect.br.X, rect.br.Y, distance);

			GL.Vertex3(rect.tr.X, rect.tr.Y, distance);

			//##########
			//GL.End();
			//##########
		}
	}
}
