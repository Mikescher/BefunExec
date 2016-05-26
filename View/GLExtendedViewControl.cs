using BefunExec.View.OpenGL;
using BefunExec.View.OpenGL.OGLMath;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace BefunExec.View
{
	public class GLExtendedViewControl : GLControl
	{
		public bool Loaded = false;

		protected FrequencyCounter FPS = new FrequencyCounter();
		
		protected float RenderFont(int compHeight, Vec2D pos, string text, float distance, StringFontRasterSheet fnt, bool backg)
		{
			float h = fnt.Size;

			if (backg)
			{
				float w = fnt.MeasureWidth(text);
				Rect2D rect = new Rect2D(pos.X, compHeight - pos.Y - h, w, h);

				GL.Disable(EnableCap.Texture2D);
				GL.Begin(PrimitiveType.Quads);
				GL.Translate(0, 0, -4);
				GL.Color4(Color.FromArgb(245, 255, 255, 255));
				GL.Vertex3(rect.tl.X, rect.tl.Y, 0);
				GL.Vertex3(rect.bl.X, rect.bl.Y, 0);
				GL.Vertex3(rect.br.X, rect.br.Y, 0);
				GL.Vertex3(rect.tr.X, rect.tr.Y, 0);
				GL.Color3(1.0, 1.0, 1.0);
				GL.Translate(0, 0, 4);
				GL.End();
				GL.Enable(EnableCap.Texture2D);
			}
			
			GL.PushMatrix();

			GL.Translate(0, 0, distance);

			fnt.Print(text, pos.X, compHeight - pos.Y - h);

			GL.PopMatrix();

			GL.Color3(1.0, 1.0, 1.0);

			return h;
		}

		protected float RenderFontBottomUp(Vec2D pos, string text, float distance, StringFontRasterSheet fnt, bool backg)
		{
			float h = fnt.Size;

			if (backg)
			{
				float w = fnt.MeasureWidth(text);
				Rect2D rect = new Rect2D(pos.X, pos.Y, w, h);

				GL.Disable(EnableCap.Texture2D);
				GL.Begin(PrimitiveType.Quads);
				GL.Translate(0, 0, -4);
				GL.Color4(Color.FromArgb(245, 255, 225, 128));
				GL.Vertex3(rect.tl.X, rect.tl.Y, 0);
				GL.Vertex3(rect.bl.X, rect.bl.Y, 0);
				GL.Vertex3(rect.br.X, rect.br.Y, 0);
				GL.Vertex3(rect.tr.X, rect.tr.Y, 0);
				GL.Color3(1.0, 1.0, 1.0);
				GL.Translate(0, 0, 4);
				GL.End();
				GL.Enable(EnableCap.Texture2D);
			}

			GL.PushMatrix();

			GL.Translate(0, 0, distance);

			fnt.Print(text, pos.X, pos.Y);

			GL.PopMatrix();

			GL.Color3(1.0, 1.0, 1.0);

			return h;
		}

		protected float RenderFontBottomUpSplitted(Vec2D pos, Tuple<string, string> row, int colWidth, string sep, float distance, StringFontRasterSheet fnt, bool backg)
		{
			string text1 = row.Item1;
			string text2 = row.Item2;

			float h = fnt.Size;

			float w1 = Math.Max(fnt.MeasureWidth(text1), colWidth);
			float ws = fnt.MeasureWidth(sep);

			if (backg)
			{
				float w2 = fnt.MeasureWidth(text2);
				Rect2D rect = new Rect2D(pos.X, pos.Y, w1 + ws + w2, h);

				GL.Disable(EnableCap.Texture2D);
				GL.Begin(PrimitiveType.Quads);
				GL.Translate(0, 0, -4);
				GL.Color4(Color.FromArgb(245, 255, 225, 128));
				GL.Vertex3(rect.tl.X, rect.tl.Y, 0);
				GL.Vertex3(rect.bl.X, rect.bl.Y, 0);
				GL.Vertex3(rect.br.X, rect.br.Y, 0);
				GL.Vertex3(rect.tr.X, rect.tr.Y, 0);
				GL.Color3(1.0, 1.0, 1.0);
				GL.Translate(0, 0, 4);
				GL.End();
				GL.Enable(EnableCap.Texture2D);
			}

			GL.PushMatrix();

			GL.Translate(0, 0, distance);

			fnt.Print(text1, pos.X,           pos.Y);
			fnt.Print(sep,   pos.X + w1,      pos.Y);
			fnt.Print(text2, pos.X + w1 + ws, pos.Y);

			GL.PopMatrix();

			GL.Color3(1.0, 1.0, 1.0);

			return h;
		}

		protected void RenderPipeHorz(Rect2D renderRect, double insetXLeft, double insetXRight, double height)
		{
			GL.Disable(EnableCap.Texture2D);
			GL.Begin(PrimitiveType.Quads);
			GL.Translate(0, 0, -3);
			GL.Color4(Color.FromArgb(192, 255, 000, 000));
			double cy = (renderRect.tl.Y + renderRect.br.Y) / 2;

			GL.Vertex3(renderRect.tl.X + insetXLeft, cy - height / 2, 0);
			GL.Vertex3(renderRect.bl.X + insetXLeft, cy + height / 2, 0);
			GL.Vertex3(renderRect.br.X - insetXRight, cy + height / 2, 0);
			GL.Vertex3(renderRect.tr.X - insetXRight, cy - height / 2, 0);

			GL.Color3(1.0, 1.0, 1.0);
			GL.Translate(0, 0, 3);
			GL.End();
			GL.Enable(EnableCap.Texture2D);
		}

		protected void RenderPipeVert(Rect2D renderRect, double insetYTop, double insetYBottom, double width)
		{
			GL.Disable(EnableCap.Texture2D);
			GL.Begin(PrimitiveType.Quads);
			GL.Translate(0, 0, -3);
			GL.Color4(Color.FromArgb(192, 255, 000, 000));
			double cx = (renderRect.tl.X + renderRect.br.X) / 2;

			GL.Vertex3(cx + width / 2, renderRect.tl.Y - insetYTop, 0);
			GL.Vertex3(cx + width / 2, renderRect.bl.Y + insetYBottom, 0);
			GL.Vertex3(cx - width / 2, renderRect.br.Y + insetYBottom, 0);
			GL.Vertex3(cx - width / 2, renderRect.tr.Y - insetYTop, 0);

			GL.Color3(1.0, 1.0, 1.0);
			GL.Translate(0, 0, 3);
			GL.End();
			GL.Enable(EnableCap.Texture2D);
		}

		protected void RenderPipeDottedHorz(Rect2D renderRect, double height, double width)
		{
			GL.Disable(EnableCap.Texture2D);
			GL.Begin(PrimitiveType.Quads);
			GL.Translate(0, 0, -3);
			GL.Color4(Color.FromArgb(192, 255, 000, 000));

			double cy = (renderRect.tl.Y + renderRect.br.Y) / 2;

			double x = renderRect.bl.X;

			while ((x + width) < renderRect.tr.X)
			{
				GL.Vertex3(x + width, cy - height / 2, 0);
				GL.Vertex3(x + width, cy + height / 2, 0);
				GL.Vertex3(x, cy + height / 2, 0);
				GL.Vertex3(x, cy - height / 2, 0);

				x += 2 * width;
			}

			GL.Color3(1.0, 1.0, 1.0);
			GL.Translate(0, 0, 3);
			GL.End();
			GL.Enable(EnableCap.Texture2D);
		}

		protected void RenderPipeDottedVert(Rect2D renderRect, double height, double width)
		{
			GL.Disable(EnableCap.Texture2D);
			GL.Begin(PrimitiveType.Quads);
			GL.Translate(0, 0, -3);
			GL.Color4(Color.FromArgb(192, 255, 000, 000));
			double cx = (renderRect.tl.X + renderRect.br.X) / 2;

			double y = renderRect.bl.Y;

			while ((y + height) < renderRect.tr.Y)
			{
				GL.Vertex3(cx + width / 2, y, 0);
				GL.Vertex3(cx + width / 2, y + height, 0);
				GL.Vertex3(cx - width / 2, y + height, 0);
				GL.Vertex3(cx - width / 2, y, 0);

				y += 2 * height;
			}

			GL.Color3(1.0, 1.0, 1.0);
			GL.Translate(0, 0, 3);
			GL.End();
			GL.Enable(EnableCap.Texture2D);
		}

		protected void RenderInsetEllipse(Rect2D renderRect, double insetX, double insetY, bool circle)
		{
			GL.Disable(EnableCap.Texture2D);
			GL.Begin(PrimitiveType.TriangleFan);
			GL.Translate(0, 0, -3);
			GL.Color4(Color.FromArgb(192, 255, 000, 000));

			double cx = (renderRect.tl.X + renderRect.tr.X) / 2;
			double cy = (renderRect.tl.Y + renderRect.br.Y) / 2;

			double w = (renderRect.tr.X - renderRect.tl.X) - insetX * 2;
			double h = (renderRect.tl.Y - renderRect.bl.Y) - insetY * 2;

			if (circle)
			{
				w = Math.Min(w, h);
				h = w;
			}

			GL.Vertex3(cx, cy, 0);

			const int vcount = 23;
			for (int i = 0; i <= vcount; i++)
			{
				GL.Vertex3(cx + Math.Sin((i * 2 * Math.PI) / vcount) * w / 2, cy + Math.Cos((i * 2 * Math.PI) / vcount) * h / 2, 0);
			}

			GL.Color3(1.0, 1.0, 1.0);
			GL.Translate(0, 0, 3);
			GL.End();
			GL.Enable(EnableCap.Texture2D);
		}

		protected void RenderCurve(Vec2D center, double radX, double radY, double thickness, int beginCurveSeg, int endCurveSeg, int curveSegCount)
		{
			GL.Disable(EnableCap.Texture2D);
			GL.Begin(PrimitiveType.TriangleStrip);
			GL.Translate(0, 0, -3);
			GL.Color4(Color.FromArgb(192, 255, 000, 000));

			for (int i = beginCurveSeg; i <= endCurveSeg; i++)
			{
				GL.Vertex3(center.X + Math.Sin((i * 2 * Math.PI) / curveSegCount) * (radX - thickness / 2), center.Y + Math.Cos((i * 2 * Math.PI) / curveSegCount) * (radY - thickness / 2), 0);
				GL.Vertex3(center.X + Math.Sin((i * 2 * Math.PI) / curveSegCount) * (radX + thickness / 2), center.Y + Math.Cos((i * 2 * Math.PI) / curveSegCount) * (radY + thickness / 2), 0);
			}

			GL.Color3(1.0, 1.0, 1.0);
			GL.Translate(0, 0, 3);
			GL.End();
			GL.Enable(EnableCap.Texture2D);
		}

		public static String GetFreqFormatted(double freq, string nan = "NaN", string pi = "MAX", string ni = "MIN")
		{
			if (double.IsPositiveInfinity(freq))
				return pi;
			if (double.IsNegativeInfinity(freq))
				return ni;
			if (double.IsNaN(freq))
				return nan;

			string pref = "";

			if (freq > 1000)
			{
				freq /= 1000;
				pref = "k";

				if (freq > 1000)
				{
					freq /= 1000;
					pref = "M";

					if (freq > 1000)
					{
						freq /= 1000;
						pref = "G";
					}
				}
			}

			return String.Format(@"{0:0.00} {1}Hz", freq, pref);
		}

	}
}
