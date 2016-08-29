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
		private enum ZoomSelectionMode { None, Select, Drag, SetBreakPoint, SetWatchPoint }

		private ZoomSelectionMode selectionMode = ZoomSelectionMode.None;
		private Rect2I selectionStartZoom = null;
		private Vec2I selectionStart = null;
		private Rect2I selection = null;

		private readonly Rect2I fullzoom;
		private readonly Stack<Rect2I> zoom = new Stack<Rect2I>();

		private readonly InteropKeyboard keyboard;
		private readonly BefunProg prog;

		public ZoomController(BefunProg p, InteropKeyboard kb)
		{
			prog = p;
			keyboard = kb;

			zoom.Push(fullzoom = new Rect2I(0, 0, prog.Width, prog.Height));
		}

		public ZoomController(BefunProg p, ZoomController copy)
		{
			prog = p;
			keyboard = copy.keyboard;

			fullzoom = copy.fullzoom;
			zoom = new Stack<Rect2I>(copy.zoom.Reverse());
		}

		public void RenderSelection(double offx, double offy, double w, double h, Rect2I currZoom)
		{
			if (selection != null)
			{
				Rect2D rect = new Rect2D(offx + ((selection.tl.X) - currZoom.bl.X) * w, offy + ((currZoom.Height - 1) - ((selection.tl.Y - 1) - currZoom.bl.Y)) * h, selection.Width * w, selection.Height * h);

				GL.Disable(EnableCap.Texture2D);

				GL.Begin(PrimitiveType.LineLoop);
				GL.Translate(0, 0, -3);
				GL.Color4(Color.Black);
				GL.Vertex3(rect.tl.X, rect.tl.Y, 0);
				GL.Vertex3(rect.bl.X, rect.bl.Y, 0);
				GL.Vertex3(rect.br.X, rect.br.Y, 0);
				GL.Vertex3(rect.tr.X, rect.tr.Y, 0);
				GL.Color3(1.0, 1.0, 1.0);
				GL.Translate(0, 0, 3);
				GL.End();

				GL.Begin(PrimitiveType.Quads);
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

			if (e.Button == MouseButtons.Middle)
			{
				if (selx >= 0 && sely >= 0 && selx < prog.Width && sely < prog.Height)
				{
					selectionMode = ZoomSelectionMode.SetWatchPoint;

					var prev = prog.WatchedFields.FirstOrDefault(p => p.X == selx && p.Y == sely);

					if (prev == null)
					{
						prog.WatchData[selx, sely] = true;
						prog.WatchedFields.Add(new WatchedField(selx, sely));
					}
					else
					{
						prog.WatchedFields.Remove(prev);

						var next = prev.GetNext();
						if (next == null)
						{
							prog.WatchData[selx, sely] = false;
						}
						else
						{
							prog.WatchedFields.Add(next);
						}
					}

				}
			}
			else if (e.Button == MouseButtons.Left)
			{
				if (keyboard.IsDown(Keys.ShiftKey))
				{
					if (selx != -1 && sely != -1)
					{
						selectionMode = ZoomSelectionMode.Drag;

						selectionStartZoom = new Rect2I(Peek());
						selectionStart = new Vec2I(selx, sely);
					}
					else
					{
						selectionStart = null;
					}
				}
				else
				{
					if (selx != -1 && sely != -1)
					{
						selectionMode = ZoomSelectionMode.Select;

						selectionStart = new Vec2I(selx, sely);

						UpdateSelectionCalculation(selx, sely);
					}
					else
					{
						selectionStart = null;
					}
				}
			}
		}

		public void DoMouseMove(MouseEventArgs e, int selx, int sely)
		{
			if (e.Button != MouseButtons.Left)
				selection = null;


			if (selectionStart != null)
			{
				if (selectionMode == ZoomSelectionMode.Select)
				{
					UpdateSelectionCalculation(selx, sely);
				}
				else if (selectionMode == ZoomSelectionMode.Drag)
				{
					UpdateZoomDragging(selx, sely);
					
					if (!keyboard.IsDown(Keys.ShiftKey)) selectionMode = ZoomSelectionMode.None;
				}
			}
		}

		public void DoMouseUp(MouseEventArgs e, int selx, int sely)
		{
			if (e.Button != MouseButtons.Left)
				selection = null;

			if (selectionStart != null)
			{
				if (selectionMode == ZoomSelectionMode.Select)
				{
					UpdateSelectionCalculation(selx, sely);

					if (selection == null) return;

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
				else if (selectionMode == ZoomSelectionMode.Drag)
				{
					UpdateZoomDragging(selx, sely);
				}
			}

			selectionMode = ZoomSelectionMode.None;
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
					ZoomIn();
				else if (e.Delta < 0)
					ZoomOut();
			}
		}

		private void ZoomIn()
		{
			if (RunOptions.FOLLOW_MODE) //NOT POSSIBLE WHILE FOLLOWING
				return;

			Rect2I z = new Rect2I(zoom.Peek());

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

		private void ZoomOut()
		{
			if (RunOptions.FOLLOW_MODE) //NOT POSSIBLE WHILE FOLLOWING
				return;

			Rect2I z = new Rect2I(zoom.Peek());

			int dx = z.Width / 10;
			int dy = z.Height / 10;

			if (dx == 0)
				dx = 1;

			if (dy == 0)
				dy = 1;

			z.TrimHorizontal(-dx);
			z.TrimVertical(-dy);

			z.ForceInside(new Rect2I(0, 0, prog.Width, prog.Height));


			if (zoom.Count > 1)
				zoom.Pop();
			zoom.Push(z);
		}

		public double GetZoomFactor()
		{
			return Math.Min(prog.Width * 1.0 / zoom.Peek().Width, prog.Height * 1.0 / zoom.Peek().Height);
		}

		private void UpdateSelectionCalculation(int mouseX, int mouseY)
		{
			if (mouseX != -1 && mouseY != -1)
			{
				int l = Math.Min(mouseX, selectionStart.X);
				int r = Math.Max(mouseX, selectionStart.X);
				int b = Math.Min(mouseY, selectionStart.Y);
				int t = Math.Max(mouseY, selectionStart.Y);

				selection = new Rect2I(new Vec2I(l, b), new Vec2I(r + 1, t + 1));
			}
		}

		private void UpdateZoomDragging(int mouseX, int mouseY)
		{
			if (mouseX != -1 && mouseY != -1)
			{
				Rect2I r = new Rect2I(selectionStartZoom);

				r.Move(selectionStart.X - mouseX, selectionStart.Y - mouseY);

				r.ForceTranslateInside(fullzoom);

				Replace(r);

				selectionStartZoom = r;
			}
		}

		public Rect2I Peek()
		{
			return zoom.Peek();
		}

		public Rect2I Pop()
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

		public void Push(Rect2I rect)
		{
			if (rect == null || rect.bl.X < 0 || rect.bl.Y < 0 || rect.tr.X > prog.Width || rect.tr.Y > prog.Height)
				return;

			zoom.Push(rect);
		}

		public int Count()
		{
			return zoom.Count();
		}

		public InteropKeyboard GetKeyboard()
		{
			return keyboard;
		}

		public void Replace(Rect2I zr)
		{
			if (Count() > 1)
				Pop();

			Push(zr);
		}
	}
}
