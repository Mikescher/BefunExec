using BefunExec.View.OpenGL.OGLMath;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace BefunExec.View
{
	public class OGLTextureSheet
	{
		private int texID;

		protected int width; // texturecount X-Axis
		protected int height; // texturecount Y-Axis

		protected OGLTextureSheet(int id, int w, int h)
		{
			this.width = w;
			this.height = h;
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

		public Rect2d GetCoordinates(int x, int y)
		{
			if (x >= width || y >= height || x < 0 || y < 0)
			{
				throw new ArgumentException(String.Format("X:{0}, Y:{1}, W:{2}, H:{3}", x, y, width, height));
			}
			double texWidth = 1.0 / width;
			double texHeight = 1.0 / height;

			Vec2d p = new Vec2d(texWidth * x, texHeight * y);

			return new Rect2d(p, texWidth, texHeight);
		}

		public Rect2d GetCoordinates(int pos)
		{
			return GetCoordinates(pos % width, pos / width);
		}

		public Vec2i GetPosition(int pos)
		{
			return new Vec2i(pos % width, pos / width);
		}

		public void bind()
		{
			GL.BindTexture(TextureTarget.Texture2D, GetID());
		}

		public static int LoadResourceIntoUID(string filename)
		{
			if (String.IsNullOrEmpty(filename))
				throw new ArgumentException(filename);

			int id = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, id);

			Bitmap bmp = new Bitmap(filename);
			BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

			bmp.UnlockBits(bmp_data);

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

			BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

			bmp.UnlockBits(bmp_data);

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)filter);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)filter);

			return id;
		}
	}
}
