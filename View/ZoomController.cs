using BefunExec.Logic;
using BefunExec.View.OpenGL.OGLMath;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BefunExec.View
{
	public class ZoomController
	{
		private Vec2i selectionStart = null;
		private Rect2i selection = null;
		public Stack<Rect2i> zoom = new Stack<Rect2i>();

		private readonly BefunProg prog;

		public ZoomController(BefunProg p)
		{
			this.prog = p;
			zoom.Push(new Rect2i(0, 0, prog.Width, prog.Height));
		}

		public void renderSelection(double offx, double offy, double w, double h)
		{
			if (selection != null)
			{
				Rect2d rect = new Rect2d(offx + ((selection.tl.X) - zoom.Peek().bl.X) * w, offy + ((zoom.Peek().Height - 1) - ((selection.tl.Y - 1) - zoom.Peek().bl.Y)) * h, selection.Width * w, selection.Height * h);

				GL.Disable(EnableCap.Texture2D);

				GL.Begin(BeginMode.LineLoop);
				GL.Translate(0, 0, -3);
				GL.Color4(Color.Black);
				GL.Vertex3(rect.tl.X, rect.tl.Y, 0);
				GL.Vertex3(rect.bl.X, rect.bl.Y, 0);
				GL.Vertex3(rect.br.X, rect.br.Y, 0);
				GL.Vertex3(rect.tr.X, rect.tr.Y, 0);
				GL.Color3(1.0, 1.0, 1.0);
				GL.Translate(0, 0, 3);
				GL.End();

				GL.Begin(BeginMode.Quads);
				GL.Translate(0, 0, -4);
				GL.Color4(0.0, 0.0, 0.0, 0.5);
				GL.Vertex3(rect.tl.X, rect.tl.Y, 0);
				GL.Vertex3(rect.bl.X, rect.bl.Y, 0);
				GL.Vertex3(rect.br.X, rect.br.Y, 0);
				GL.Vertex3(rect.tr.X, rect.tr.Y, 0);
				GL.Color3(1.0, 1.0, 1.0);
				GL.Translate(0, 0, 4);
				GL.End();

				GL.Enable(EnableCap.Texture2D);
			}
		}

		public void DoMouseDown(MouseEventArgs e, int selx, int sely)
		{
			if (RunOptions.FOLLOW_MODE) //NOT POSSIBLE WHILE FOLLOWING
				return;

			if (e.Button != MouseButtons.Left)
				return;


			if (selx != -1 && sely != -1)
			{
				selectionStart = new Vec2i(selx, sely);

				updateSelectionCalculation(selx, sely);
			}
			else
			{
				selectionStart = null;
			}
		}

		public void DoMouseMove(MouseEventArgs e, int selx, int sely)
		{
			if (e.Button != MouseButtons.Left)
				selection = null;


			if (selectionStart != null)
			{
				updateSelectionCalculation(selx, sely);
			}
		}

		public void DoMouseUp(MouseEventArgs e, int selx, int sely)
		{
			if (e.Button != MouseButtons.Left)
				selection = null;

			if (selectionStart != null)
			{
				updateSelectionCalculation(selx, sely);

				if (selection.Width == 1 && selection.Height == 1)
				{
					if (prog.Breakpoints[selection.bl.X, selection.bl.Y])
					{
						prog.Breakpoints[selection.bl.X, selection.bl.Y] = false;
						prog.Breakpointcount--;
					}
					else
					{
						prog.Breakpoints[selection.bl.X, selection.bl.Y] = true;
						prog.Breakpointcount++;
					}
				}
				else if (Math.Abs(selection.Width) > 0 && Math.Abs(selection.Height) > 0)
				{
					if (selection != zoom.Peek())
						zoom.Push(selection);
				}
			}

			selectionStart = null;
			selection = null;
		}

		public void DoMouseWheel(MouseEventArgs e)
		{
			if (RunOptions.FOLLOW_MODE) //NOT POSSIBLE WHILE FOLLOWING
				return;

			for (int i = 0; i < Math.Abs(e.Delta); i += 120)
			{
				if (e.Delta > 0)
					zoomIn();
				else if (e.Delta < 0)
					zoomOut();
			}
		}

		private void zoomIn()
		{
			if (RunOptions.FOLLOW_MODE) //NOT POSSIBLE WHILE FOLLOWING
				return;

			Rect2i z = new Rect2i(zoom.Peek());

			int dx = z.Width / 10;
			int dy = z.Height / 10;

			if (dx == 0)
				dx = 1;

			if (dy == 0)
				dy = 1;

			if (z.Width > 2 * dx)
			{
				z.TrimHorizontal(dx);
			}
			else
			{
				int trim1 = z.Width / 2;
				int trim2 = (z.Width - trim1) - 1;

				z.TrimEast(trim1);
				z.TrimWest(trim2);
			}

			if (z.Height > 2 * dy)
			{
				z.TrimVertical(dy);
			}
			else
			{
				int trim1 = z.Height / 2;
				int trim2 = (z.Height - trim1) - 1;

				z.TrimNorth(trim1);
				z.TrimSouth(trim2);
			}

			if (zoom.Count > 1)
				zoom.Pop();
			zoom.Push(z);
		}

		private void zoomOut()
		{
			if (RunOptions.FOLLOW_MODE) //NOT POSSIBLE WHILE FOLLOWING
				return;

			Rect2i z = new Rect2i(zoom.Peek());

			int dx = z.Width / 10;
			int dy = z.Height / 10;

			if (dx == 0)
				dx = 1;

			if (dy == 0)
				dy = 1;

			z.TrimHorizontal(-dx);
			z.TrimVertical(-dy);

			z.ForceInside(new Rect2i(0, 0, prog.Width, prog.Height));


			if (zoom.Count > 1)
				zoom.Pop();
			zoom.Push(z);
		}

		public double getZoomFactor()
		{
			return Math.Min(prog.Width * 1.0 / this.zoom.Peek().Width, prog.Height * 1.0 / this.zoom.Peek().Height);
		}

		private void updateSelectionCalculation(int mouseX, int mouseY)
		{
			if (mouseX != -1 && mouseY != -1)
			{
				int l = Math.Min(mouseX, selectionStart.X);
				int r = Math.Max(mouseX, selectionStart.X);
				int b = Math.Min(mouseY, selectionStart.Y);
				int t = Math.Max(mouseY, selectionStart.Y);

				selection = new Rect2i(new Vec2i(l, b), new Vec2i(r + 1, t + 1));
			}
		}

		public Rect2i Peek()
		{
			return zoom.Peek();
		}

		public Rect2i Pop()
		{
			if (Count() > 1)
				return zoom.Pop();
			else
				return null;
		}

		public void PopInvalid()
		{
			if (zoom.Peek() == null || zoom.Peek().bl.X < 0 || zoom.Peek().bl.Y < 0 || zoom.Peek().tr.X > prog.Width || zoom.Peek().tr.Y > prog.Height)
				zoom.Pop();
		}

		public void PopToBase()
		{
			while (Count() > 1)
				Pop();
		}

		public void Clear()
		{
			zoom.Clear();
		}

		public void Push(Rect2i rect)
		{
			if (rect == null || rect.bl.X < 0 || rect.bl.Y < 0 || rect.tr.X > prog.Width || rect.tr.Y > prog.Height)
				return;

			zoom.Push(rect);
		}

		public int Count()
		{
			return zoom.Count();
		}
	}
}
