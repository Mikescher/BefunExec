using BefunExec.View.OpenGL.OGLMath;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using BefunExec.View.OpenGL;

namespace BefunExec.View
{
	public class GLExtendedViewControl : GLControl
	{
		public bool loaded = false;

		protected FrequencyCounter fps = new FrequencyCounter();
        
        protected float RenderFont(int CompHeight, Vec2d pos, string text, int distance, StringFontRasterSheet fnt, bool backg)
        {
            float h = fnt.Size;

            if (backg)
            {
                float w = fnt.MeasureWidth(text);
                Rect2d rect = new Rect2d(pos.X, CompHeight - pos.Y - h, w, h);

                GL.Disable(EnableCap.Texture2D);
                GL.Begin(BeginMode.Quads);
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

            fnt.Print(text, pos.X, CompHeight - pos.Y - h);

            GL.PopMatrix();

            GL.Color3(1.0, 1.0, 1.0);

            return h;
        }

        protected void renderPipeHorz(Rect2d renderRect, double insetX_l, double insetX_r, double height)
		{
			GL.Disable(EnableCap.Texture2D);
			GL.Begin(BeginMode.Quads);
			GL.Translate(0, 0, -3);
			GL.Color4(Color.FromArgb(192, 255, 000, 000));
			double cy = (renderRect.tl.Y + renderRect.br.Y) / 2;

			GL.Vertex3(renderRect.tl.X + insetX_l, cy - height / 2, 0);
			GL.Vertex3(renderRect.bl.X + insetX_l, cy + height / 2, 0);
			GL.Vertex3(renderRect.br.X - insetX_r, cy + height / 2, 0);
			GL.Vertex3(renderRect.tr.X - insetX_r, cy - height / 2, 0);

			GL.Color3(1.0, 1.0, 1.0);
			GL.Translate(0, 0, 3);
			GL.End();
			GL.Enable(EnableCap.Texture2D);
		}

		protected void renderPipeVert(Rect2d renderRect, double insetY_t, double insetY_b, double width)
		{
			GL.Disable(EnableCap.Texture2D);
			GL.Begin(BeginMode.Quads);
			GL.Translate(0, 0, -3);
			GL.Color4(Color.FromArgb(192, 255, 000, 000));
			double cx = (renderRect.tl.X + renderRect.br.X) / 2;

			GL.Vertex3(cx + width / 2, renderRect.tl.Y - insetY_t, 0);
			GL.Vertex3(cx + width / 2, renderRect.bl.Y + insetY_b, 0);
			GL.Vertex3(cx - width / 2, renderRect.br.Y + insetY_b, 0);
			GL.Vertex3(cx - width / 2, renderRect.tr.Y - insetY_t, 0);

			GL.Color3(1.0, 1.0, 1.0);
			GL.Translate(0, 0, 3);
			GL.End();
			GL.Enable(EnableCap.Texture2D);
		}

		protected void renderPipeDottedHorz(Rect2d renderRect, double height, double width)
		{
			GL.Disable(EnableCap.Texture2D);
			GL.Begin(BeginMode.Quads);
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

		protected void renderPipeDottedVert(Rect2d renderRect, double height, double width)
		{
			GL.Disable(EnableCap.Texture2D);
			GL.Begin(BeginMode.Quads);
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

		protected void renderInsetEllipse(Rect2d renderRect, double insetX, double insetY, bool circle)
		{
			GL.Disable(EnableCap.Texture2D);
			GL.Begin(BeginMode.TriangleFan);
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

			const int VCOUNT = 23;
			for (int i = 0; i <= VCOUNT; i++)
			{
				GL.Vertex3(cx + Math.Sin((i * 2 * Math.PI) / VCOUNT) * w / 2, cy + Math.Cos((i * 2 * Math.PI) / VCOUNT) * h / 2, 0);
			}

			GL.Color3(1.0, 1.0, 1.0);
			GL.Translate(0, 0, 3);
			GL.End();
			GL.Enable(EnableCap.Texture2D);
		}

		protected void renderCurve(Vec2d center, double rad_x, double rad_y, double thickness, int beginCurveSeg, int endCurveSeg, int curveSegCount)
		{
			GL.Disable(EnableCap.Texture2D);
			GL.Begin(BeginMode.TriangleStrip);
			GL.Translate(0, 0, -3);
			GL.Color4(Color.FromArgb(192, 255, 000, 000));

			for (int i = beginCurveSeg; i <= endCurveSeg; i++)
			{
				GL.Vertex3(center.X + Math.Sin((i * 2 * Math.PI) / curveSegCount) * (rad_x - thickness / 2), center.Y + Math.Cos((i * 2 * Math.PI) / curveSegCount) * (rad_y - thickness / 2), 0);
				GL.Vertex3(center.X + Math.Sin((i * 2 * Math.PI) / curveSegCount) * (rad_x + thickness / 2), center.Y + Math.Cos((i * 2 * Math.PI) / curveSegCount) * (rad_y + thickness / 2), 0);
			}

			GL.Color3(1.0, 1.0, 1.0);
			GL.Translate(0, 0, 3);
			GL.End();
			GL.Enable(EnableCap.Texture2D);
		}

		public static String getFreqFormatted(double freq, string nan = "NaN", string pi = "MAX", string ni = "MIN")
		{
			if (freq == float.PositiveInfinity)
				return pi;
			if (freq == float.NegativeInfinity)
				return ni;
			if (freq == float.NaN)
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
