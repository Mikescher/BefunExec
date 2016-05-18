using BefunExec.View.OpenGL;
using BefunExec.View.OpenGL.OGLMath;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Linq;

namespace BefunExec.View
{
	public class StringFontRasterSheet : FontRasterSheet
    {
        public readonly int Size;
	    private float[] charWidth;
	    private float[] charStart;

	    private readonly Color fontColor;

	    private StringFontRasterSheet(int id, int w, int h, Bitmap b, int size, Color color, bool mono)
            : base(id, w, h, b)
        {
            Size = size;
            fontColor = color;

            InitCharSizes(w, h, b);

	        if (mono)
	        {
		        float mw = charWidth.Max();
		        charWidth = Enumerable.Repeat(mw, charWidth.Length).ToArray();
	        }
        }

        private void InitCharSizes(int w, int h, Bitmap b)
        {
            charWidth = new float[w*h];
            charStart = new float[w*h];
            int rw = b.Width/w;
            int rh = b.Width/h;
            for (int i = 0; i < w*h; i++)
            {
                int start = -1;
                int end = -1;

                for (int x = 0; x < rw; x++)
                {
                    if (Enumerable.Range(0, rh).Select(y => b.GetPixel((i%w)*rw + x, (i/w)*rh + y)).Distinct().Count() > 1)
                    {
                        if (start == -1) start = x;
                        end = x;
                    }
                }


                if (start < 0 && end < 0)
                {
                    charStart[i] = 0;
                    charWidth[i] = 1f/w;

                    continue;
                }

                start -= 1;
                end += 1;

                start = Math.Max(0, Math.Min(rw - 1, start));
                end = Math.Max(0, Math.Min(rw - 1, end));

                charStart[i] = (start*1f/rw)/w;
                charWidth[i] = ((end - start)*1f/rw)/w;
            }
		}

		public static StringFontRasterSheet Create(Bitmap font, int size, Color color)
		{
			Bitmap b = new Bitmap(font);
			b = b.Clone(new Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			return new StringFontRasterSheet(LoadResourceIntoUID(b, TextureMinFilter.Linear), 16, 16, b, size, color, false);
		}

		public static StringFontRasterSheet CreateMonoSpace(Bitmap font, int size, Color color)
		{
			Bitmap b = new Bitmap(font);
			b = b.Clone(new Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			return new StringFontRasterSheet(LoadResourceIntoUID(b, TextureMinFilter.Linear), 16, 16, b, size, color, true);
		}

		public float MeasureWidth(string text)
        {
            return text.ToCharArray().Select(c => charWidth[c] * Width * Size).Sum();
        }

        public void Print(string text, double posX, double posY)
        {
            float distance = 0;
            float pw = Size;
            float ph = Size;
            float px = (float)posX;
            float py = (float)posY;

            GL.Color4(fontColor.R, fontColor.G, fontColor.B, fontColor.A);

            foreach (char chr in text)
            {
                Rect2D coords = GetCoordinates(chr);

                float cstart = (float)coords.bl.X + charStart[chr];
                float cend   = cstart + charWidth[chr];

                pw = charWidth[chr] * Width * Size;

                //##########
                GL.Begin(PrimitiveType.Quads);
                //##########

                GL.TexCoord2(cstart, coords.bl.Y);
                GL.Vertex3(px, py + ph, distance);

                GL.TexCoord2(cstart, coords.tl.Y);
                GL.Vertex3(px, py, distance);

                GL.TexCoord2(cend, coords.tr.Y);
                GL.Vertex3(px + pw, py, distance);

                GL.TexCoord2(cend, coords.br.Y);
                GL.Vertex3(px+ pw, py + ph, distance);

                //##########
                GL.End();
                //##########

                px += pw;
            }
        }
    }
}
