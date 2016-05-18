using BefunExec.View.OpenGL.OGLMath;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace BefunExec.View.OpenGL
{
	public class OGLTextureSheet
	{
		private readonly int texID;

		protected readonly int Width; // texturecount X-Axis
		protected readonly int Height; // texturecount Y-Axis

		protected OGLTextureSheet(int id, int w, int h)
		{
			this.Width = w;
			this.Height = h;
			this.texID = id;
		}

		public virtual int GetID()
		{
			return texID;
		}

		public static OGLTextureSheet LoadTextureFromFile(string filename, int width, int height)
		{
			return new OGLTextureSheet(LoadResourceIntoUID(filename), width, height);
		}

		public static OGLTextureSheet LoadTextureFromBitmap(Bitmap bmp, int width, int height)
		{
			return new OGLTextureSheet(LoadResourceIntoUID(bmp, TextureMinFilter.Nearest), width, height);
		}

		public static OGLTextureSheet LoadTextureFromRessourceID(int id, int width, int height)
		{
			return new OGLTextureSheet(id, width, height);
		}

		public Rect2D GetCoordinates(long x, long y)
		{
			if (x >= Width || y >= Height || x < 0 || y < 0)
			{
				throw new ArgumentException(String.Format("X:{0}, Y:{1}, W:{2}, H:{3}", x, y, Width, Height));
			}
			double texWidth = 1.0 / Width;
			double texHeight = 1.0 / Height;

			Vec2D p = new Vec2D(texWidth * x, texHeight * y);

			return new Rect2D(p, texWidth, texHeight);
		}

		public Rect2D GetCoordinates(long pos)
		{
			return GetCoordinates(pos % Width, pos / Width);
		}

		public Vec2I GetPosition(long pos)
		{
			return new Vec2I((int)(pos % Width), (int)(pos / Width));
		}

		public void bind()
		{
			GL.BindTexture(TextureTarget.Texture2D, GetID());
		}

		public static int LoadResourceIntoUID(string filename)
		{
			if (string.IsNullOrEmpty(filename))
				throw new ArgumentException(filename);

			int id = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, id);

			Bitmap bmp = new Bitmap(filename);
			BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);

			bmp.UnlockBits(bmpData);

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

			return id;
		}

		public static int LoadResourceIntoUID(Bitmap bmp, TextureMinFilter filter)
		{
			if (bmp == null)
				throw new ArgumentException();

			int id = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, id);

			BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);

			bmp.UnlockBits(bmpData);

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)filter);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)filter);

			return id;
		}
	}
}
