using BefunExec.Logic;
using BefunExec.View.OpenGL;
using BefunExec.View.OpenGL.OGLMath;
using BefunHighlight;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BefunExec.View
{
	public class GLProgramViewControl : GLExtendedViewControl
	{
		private const int MAX_HQ_CELLCOUNT = 16 * 1000;
		private const int MAX_MQ_CELLCOUNT = 400 * 1000;

		private const int LQ_CELLCOUNT = 50 * 1000;

		public const int MAX_EXTENDEDSH_SIZE = 3 * 1000 * 1000;

		public readonly DebugTimer UpdateTimer = new DebugTimer();
		public readonly DebugTimer RenderTimer = new DebugTimer();

		public ZoomController Zoom;

		private FontRasterSheet font;
		private FontRasterSheet nopFont;
		private FontRasterSheet stringfont;
		private FontRasterSheet bwfont;

		private StringFontRasterSheet debugFont;
		private StringFontRasterSheet watchFont;
		private StringFontRasterSheet boxFont;

		public BeGraph ExtendedSHGraph = null;
		private BefunProg prog;

		public GLProgramViewControl()
		{
			Loaded = false;
		}

		public void DoInit(BefunProg p)
		{
			prog = p;
			Zoom = new ZoomController(prog);

			MakeCurrent();

			GL.Enable(EnableCap.Texture2D);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			GL.Disable(EnableCap.CullFace);
			GL.Disable(EnableCap.DepthTest);
			
			debugFont = StringFontRasterSheet.Create(Properties.Resources.font_bold, 16, Color.Black);
			watchFont = StringFontRasterSheet.Create(Properties.Resources.font_vera, 18, Color.Black);
			boxFont = StringFontRasterSheet.Create(Properties.Resources.font, 32, Color.Black);

			InitSyntaxHighlighting();

			stringfont = FontRasterSheet.Create(false, Color.DarkGreen, Color.FromArgb(212, 255, 212));
			nopFont = FontRasterSheet.Create(false, Color.Black, Color.FromArgb(244, 244, 244)); //TODO CHR(0) stay gray
			bwfont = FontRasterSheet.Create(false, Color.Black, Color.White);

			Zoom.Push(RunOptions.INIT_ZOOM);

			Loaded = true;
		}

		public void DoRender(bool doSwap /* = true */, bool renderDebug, string currInput)
		{
			#region INIT

			FPS.Inc();

			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.ClearColor(Color.FromArgb(244, 244, 244));

			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(0.0, Width, 0.0, Height, 0.0, 4.0);

			GL.Color3(1.0, 1.0, 1.0);

			#endregion

			#region SIZE

			double offx;
			double offy;
			double w; // Width & Height of an single cell
			double h;
			CalcProgPos(out offx, out offy, out w, out h);

			int cellcount = Zoom.Peek().Width * Zoom.Peek().Height;

			#endregion

			#region RENDER

			int quality;

			if (cellcount < MAX_HQ_CELLCOUNT)
			{
				quality = 0;

				if (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED)
				{
					Render_HQ_sh(offx, offy, w, h, renderDebug);
				}
				else
				{
					Render_HQ(offx, offy, w, h);
				}
			}
			else if (cellcount < MAX_MQ_CELLCOUNT)
			{
				quality = 1;

				if (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED)
				{
					Render_MQ_sh(offx, offy, w, h);
				}
				else
				{
					Render_MQ(offx, offy, w, h);
				}
			}
			else
			{
				quality = 2;

				Render_LQ(offx, offy, w, h);
			}

			#endregion

			#region SELECTION

			Zoom.RenderSelection(offx, offy, w, h);

			#endregion

			#region INPUT

			if (prog.Mode == BefunProg.MODE_IN_INT /*|| showInputASCIIMessageToolStripMenuItem.Checked*/)
			{
				int bw = 512;
				int bh = 128;

				int box = (Width - bw) / 2;
				int boy = (Height - bh) / 2;

				Rect2D rect = new Rect2D(box, boy, bw, bh);

				GL.Disable(EnableCap.Texture2D);
				GL.Begin(PrimitiveType.Quads);
				GL.Translate(0, 0, -3);
				GL.Color4(Color.FromArgb(255, 128, 128, 128));
				GL.Vertex3(rect.tl.X, rect.tl.Y, 0);
				GL.Vertex3(rect.bl.X, rect.bl.Y, 0);
				GL.Vertex3(rect.br.X, rect.br.Y, 0);
				GL.Vertex3(rect.tr.X, rect.tr.Y, 0);
				GL.Color3(1.0, 1.0, 1.0);
				GL.Translate(0, 0, 3);
				GL.End();
				GL.Enable(EnableCap.Texture2D);


				boxFont.bind();

				RenderFont(Height, new Vec2D(box, boy), "Please enter a " + ((prog.Mode == BefunProg.MODE_IN_INT) ? "number" : "character"), -1, boxFont, true);

				RenderFont(Height, new Vec2D(box, boy + 64), currInput, -1, boxFont, true);
			}

			#endregion

			#region Watch List
			
			if (prog.WatchedFields.Any())
			{
				watchFont.bind();

				int ty = 0;
				foreach (var watch in prog.WatchedFields.AsEnumerable().Reverse())
				{
					RenderFontBottomUpSplitted(
						new Vec2D(10f, 10f + ty++ * 20f), 
						watch.GetDisplayString(prog.Raster[watch.X, watch.Y]),
						100,
						" := ",
						-0.9f, 
						watchFont, 
						true);
				}
			}


			#endregion

			#region DEBUG

			if (renderDebug)
			{
				debugFont.bind();

				RenderFont(Height, new Vec2D(0f, 00f),  string.Format("FPS: {0} (U := {1}ms | R := {2}ms | L := {3}ms)", (int)FPS.Frequency, (int)UpdateTimer.Time, (int)RenderTimer.Time, (int)prog.LogicTimer.Time), -1, debugFont, true);
				RenderFont(Height, new Vec2D(0f, 20f),  string.Format("SPEED: {0}", GetFreqFormatted(prog.Freq.Frequency)), -1, debugFont, true);
				RenderFont(Height, new Vec2D(0f, 40f),  string.Format("STEPS: {0:n0} {1} (Average: {2})", prog.StepCount, prog.Delta.isZero() ? "(stopped)" : "", GetFreqFormatted(((double)prog.StepCount / (prog.GetExecutedTime() + 1))*1000)), -1, debugFont, true);
				RenderFont(Height, new Vec2D(0f, 60f),  string.Format("SPEED (avg): {0}", GetFreqFormatted((double)prog.StepCount / (prog.GetExecutedTime() + 1))), -1, debugFont, true);
				RenderFont(Height, new Vec2D(0f, 80f),  string.Format("Time: {0:n0} ms", prog.GetExecutedTime()), -1, debugFont, true);
				RenderFont(Height, new Vec2D(0f, 100f), string.Format("UndoLog: {0}", prog.UndoLog.Enabled ? prog.UndoLog.Size.ToString() : "disabled"), -1, debugFont, true);
				RenderFont(Height, new Vec2D(0f, 120f), string.Format("Rendermode: [{0}] {1} (= {2:#,0} sprites)", quality, (new[] { "High Quality", "Low Quality", "Spritemap" })[quality], cellcount), -1, debugFont, true);
				RenderFont(Height, new Vec2D(0f, 140f), GetCodeTypeString(), -1, debugFont, true);
			}

			#endregion

			#region FINISH

			if (doSwap) SwapBuffers();

			#endregion
		}

		public void DoMouseDown(MouseEventArgs e)
		{
			int selx, sely;
			GetPointInProgram(e.X, e.Y, out selx, out sely);

			Zoom.DoMouseDown(e, selx, sely);
		}

		public void DoMouseMove(MouseEventArgs e)
		{
			int selx, sely;
			GetPointInProgram(e.X, e.Y, out selx, out sely);

			Zoom.DoMouseMove(e, selx, sely);
		}

		public void DoMouseUp(MouseEventArgs e)
		{
			int selx, sely;
			GetPointInProgram(e.X, e.Y, out selx, out sely);

			Zoom.DoMouseUp(e, selx, sely);
		}

		public void DoMouseWheel(MouseEventArgs e)
		{
			Zoom.DoMouseWheel(e);
		}

		private void Render_LQ(double offx, double offy, double w, double h)
		{
			double oldW = w;
			double oldH = h;

			double targetW = Zoom.Peek().Width * w;
			double targetH = Zoom.Peek().Height * h;

			double ratio = (Zoom.Peek().Width * 1.0) / Zoom.Peek().Height;
			double frenderH = Math.Sqrt(LQ_CELLCOUNT / ratio);
			double frenderW = LQ_CELLCOUNT / frenderH;

			int renderW = (int)Math.Ceiling(frenderW);
			int renderH = (int)Math.Ceiling(frenderH);

			int ox = Zoom.Peek().bl.X;
			int oy = Zoom.Peek().bl.Y;

			renderW = Math.Min(prog.Width, ox + renderW) - ox;
			renderH = Math.Min(prog.Height, oy + renderH) - oy;

			renderW = Math.Min(Zoom.Peek().Width, renderW);
			renderH = Math.Min(Zoom.Peek().Height, renderH);

			w = targetW / renderW;
			h = targetH / renderH;

			double scaleX = w / oldW;
			double scaleY = h / oldH;

			GL.Disable(EnableCap.Texture2D);

			GL.Begin(PrimitiveType.Quads);

			long last = 0;

			for (int sx = 0; sx < renderW; sx++)
			{
				for (int sy = 0; sy < renderH; sy++)
				{
					int x = ox + (int)(sx * scaleX);
					int y = oy + (int)(sy * scaleY);

					bool docol = last != prog.Raster[x, y];

					font.RenderLQ(docol, new Rect2D(offx + sx * w, offy - sy * h + targetH - oldH, w, h), -4, prog[x, y]);

					last = prog.Raster[x, y];
				}
			}

			GL.End();

			GL.Enable(EnableCap.Texture2D);
		}

		private void Render_MQ(double offx, double offy, double w, double h)
		{
			long now = Environment.TickCount;

			GL.Disable(EnableCap.Texture2D);

			GL.Begin(PrimitiveType.Quads);

			long last = 0;

			for (int x = Zoom.Peek().bl.X; x < Zoom.Peek().tr.X; x++)
			{
				for (int y = Zoom.Peek().bl.Y; y < Zoom.Peek().tr.Y; y++)
				{
					double decayPerc = (now - prog.DecayRaster[x, y] * 1d) / RunOptions.DECAY_TIME;

					if (!prog.WatchData[x, y] && !prog.Breakpoints[x, y] && prog.Raster[x, y] == ' ' && decayPerc >= 1)
						continue;

					bool docol = true;
					if (prog.Breakpoints[x, y])
					{
						GL.Color3(0.0, 0.0, 1.0);

						docol = false;
					}
					else if (prog.WatchData[x, y])
					{
						GL.Color3(1.0, 0.9, 0.5);

						docol = false;
					}
					else if (decayPerc < 0.66)
					{
						GL.Color3(1.0, 0.0, 0.0);

						docol = false;
					}
					else if (last == prog.Raster[x, y])
					{
						docol = false;
					}

					font.RenderLQ(docol, new Rect2D(offx + (x - Zoom.Peek().bl.X) * w, offy + ((Zoom.Peek().Height - 1) - (y - Zoom.Peek().bl.Y)) * h, w, h), -4, prog[x, y]);

					last = (decayPerc < 0.66 || prog.Breakpoints[x, y] || prog.WatchData[x, y]) ? int.MinValue : prog.Raster[x, y];
				}
			}

			GL.End();

			GL.Enable(EnableCap.Texture2D);
		}

		private void Render_MQ_sh(double offx, double offy, double w, double h)
		{
			long now = Environment.TickCount;

			GL.Disable(EnableCap.Texture2D);

			GL.Begin(PrimitiveType.Quads);

			long last = 0;

			for (int x = Zoom.Peek().bl.X; x < Zoom.Peek().tr.X; x++)
			{
				for (int y = Zoom.Peek().bl.Y; y < Zoom.Peek().tr.Y; y++)
				{
					double decayPerc = (now - prog.DecayRaster[x, y] * 1d) / RunOptions.DECAY_TIME;

					if (!prog.WatchData[x, y] && !prog.Breakpoints[x, y] && prog.Raster[x, y] == ' ' && decayPerc >= 1)
						continue;

					bool docol = true;
					if (prog.Breakpoints[x, y])
					{
						GL.Color3(0.0, 0.0, 1.0);

						docol = false;
					}
					else if (prog.WatchData[x, y])
					{
						GL.Color3(1.0, 0.9, 0.5);

						docol = false;
					}
					else if (decayPerc < 0.66)
					{
						GL.Color3(1.0, 0.0, 0.0);

						docol = false;
					}
					else if (last == prog.Raster[x, y])
					{
						docol = false;
					}
					else
					{
						HighlightType type;
						try
						{
							type = ExtendedSHGraph.fields[x, y].getType();
						}
						catch (Exception)
						{
							return; // safety quit
						}

						if (type == HighlightType.NOP)
						{
							if (prog[x, y] == ' ')
								GL.Color3(0.95, 0.95, 0.95);
							else
								GL.Color3(Color.Black);

							docol = false;
						}
						else if (type == HighlightType.String)
						{
							GL.Color3(Color.DarkGreen);

							docol = false;
						}
					}

					font.RenderLQ(docol, new Rect2D(offx + (x - Zoom.Peek().bl.X) * w, offy + ((Zoom.Peek().Height - 1) - (y - Zoom.Peek().bl.Y)) * h, w, h), -4, prog[x, y]);

					last = (decayPerc < 0.66 || prog.Breakpoints[x, y] || prog.WatchData[x, y]) ? int.MinValue : prog.Raster[x, y];
				}
			}

			GL.End();

			GL.Enable(EnableCap.Texture2D);
		}

		private void Render_HQ(double offx, double offy, double w, double h)
		{
			long now = Environment.TickCount;

			int fontBinded = -1;

			double prevR = -1;
			double prevG = -1;
			double prevB = -1;

			for (int x = Zoom.Peek().bl.X; x < Zoom.Peek().tr.X; x++)
			{
				for (int y = Zoom.Peek().bl.Y; y < Zoom.Peek().tr.Y; y++)
				{
					double decayPerc = (RunOptions.DECAY_TIME != 0) ? (1 - (now - prog.DecayRaster[x, y] * 1d) / RunOptions.DECAY_TIME) : (prog.DecayRaster[x, y]);
					decayPerc = Math.Min(1, decayPerc);
					decayPerc = Math.Max(0, decayPerc);
					if (x == prog.PC.X && y == prog.PC.Y)
						decayPerc = 1;

					double r;
					double g;
					double b;

					if (prog.Breakpoints[x, y])
					{
						r = decayPerc;
						g = 0;
						b = (1 - decayPerc);
					}
					else if (prog.WatchData[x, y])
					{
						r = 1;
						g = (1 - decayPerc) * 0.9;
						b = (1 - decayPerc) * 0.5;
					}
					else
					{
						r = 1;
						g = 1 - decayPerc;
						b = 1 - decayPerc;
					}
					
					// ReSharper disable CompareOfFloatsByEqualityOperator
					if (prevR != r || prevG != g || prevB != b)
						GL.Color3(prevR = r, prevG = g, prevB = b);
					// ReSharper restore CompareOfFloatsByEqualityOperator

					Rect2D renderRect = new Rect2D(offx + (x - Zoom.Peek().bl.X) * w, offy + ((Zoom.Peek().Height - 1) - (y - Zoom.Peek().bl.Y)) * h, w, h);

					if (prog.WatchData[x, y] || prog.Breakpoints[x, y] || decayPerc > 0.25)
					{
						if (fontBinded != 1)
							bwfont.bind();
						bwfont.Render(renderRect, -4, prog[x, y]);
						fontBinded = 1;
					}
					else
					{
						if (fontBinded != 2)
							font.bind();
						font.Render(renderRect, -4, prog[x, y]);
						fontBinded = 2;
					}
				}
			}
		}

		private void Render_HQ_sh(double offx, double offy, double w, double h, bool renderDebug)
		{
			if (ExtendedSHGraph == null)
			{
				InitSyntaxHighlighting(); // re-init
				return;
			}

			long now = Environment.TickCount;

			int fontBinded = -1;

			double prevR = -1;
			double prevG = -1;
			double prevB = -1;

			for (int x = Zoom.Peek().bl.X; x < Zoom.Peek().tr.X; x++)
			{
				for (int y = Zoom.Peek().bl.Y; y < Zoom.Peek().tr.Y; y++)
				{
					double decayPerc = (RunOptions.DECAY_TIME != 0) ? (1 - (now - prog.DecayRaster[x, y] * 1d) / RunOptions.DECAY_TIME) : (prog.DecayRaster[x, y]);
					decayPerc = Math.Min(1, decayPerc);
					decayPerc = Math.Max(0, decayPerc);
					if (x == prog.PC.X && y == prog.PC.Y)
						decayPerc = 1;

					double r;
					double g;
					double b;

					if (prog.Breakpoints[x, y])
					{
						r = decayPerc;
						g = 0;
						b = (1 - decayPerc);
					}
					else if (prog.WatchData[x, y])
					{
						r = 1;
						g = (1 - decayPerc) * 0.9;
						b = (1 - decayPerc) * 0.5;
					}
					else
					{
						r = 1;
						g = 1 - decayPerc;
						b = 1 - decayPerc;
					}

					// ReSharper disable CompareOfFloatsByEqualityOperator
					if (prevR != r || prevG != g || prevB != b)
						GL.Color3(prevR = r, prevG = g, prevB = b);
					// ReSharper restore CompareOfFloatsByEqualityOperator

					Rect2D renderRect = new Rect2D(offx + (x - Zoom.Peek().bl.X) * w, offy + ((Zoom.Peek().Height - 1) - (y - Zoom.Peek().bl.Y)) * h, w, h);

					if (prog.WatchData[x, y] || prog.Breakpoints[x, y] || decayPerc > 0.25)
					{
						if (fontBinded != 1)
							bwfont.bind();
						bwfont.Render(renderRect, -4, prog[x, y]);
						fontBinded = 1;
					}
					else
					{
						HighlightType type;
						try
						{
							type = ExtendedSHGraph.fields[x, y].getType();
						}
						catch (Exception)
						{
							return; // safety quit
						}

						if (type == HighlightType.NOP)
						{
							if (fontBinded != 4)
								nopFont.bind();
							nopFont.Render(renderRect, -4, prog[x, y]);
							fontBinded = 4;
						}
						else if (type == HighlightType.Command || type == HighlightType.String_and_Command)
						{
							if (fontBinded != 2)
								font.bind();
							font.Render(renderRect, -4, prog[x, y]);
							fontBinded = 2;
						}
						else if (type == HighlightType.String)
						{
							if (fontBinded != 3)
								stringfont.bind();
							stringfont.Render(renderRect, -4, prog[x, y]);
							fontBinded = 3;
						}

					}

					if (renderDebug)
						RenderBeGraphDebug(w, h, x, y, renderRect);

				}
			}
		}

		private void CalcProgPos(out double offx, out double offy, out double w, out double h)
		{
			w = (Width  * 1.0) / Zoom.Peek().Width;
			h = (Height * 1.0) / Zoom.Peek().Height;

			if ((w / h) < (8.0 / 12.0))
			{
				offy = h * Zoom.Peek().Height;
				h = (12.0 * w) / (8.0);
				offy -= h * Zoom.Peek().Height;
				offy /= 2;
				offx = 0;
			}
			else if ((w / h) > (8.0 / 12.0))
			{
				offx = w * Zoom.Peek().Width;
				w = (8.0 * h) / (12.0);
				offx -= w * Zoom.Peek().Width;
				offx /= 2;
				offy = 0;
			}
			else
			{
				offx = 0;
				offy = 0;
			}
		}

		private void RenderBeGraphDebug(double w, double h, int x, int y, Rect2D renderRect)
		{
			HighlightField shField;

			try
			{
				shField = ExtendedSHGraph.fields[x, y];
				if (shField == null)
					return;
			}
			catch (Exception)
			{
				return; // safety quit
			}

			if (shField.incoming_information && !shField.outgoing_information)
			{
				if (shField.incoming_information_left)
					RenderPipeHorz(renderRect, 0, w / 2, w / 3);
				if (shField.incoming_information_right)
					RenderPipeHorz(renderRect, w / 2, 0, w / 3);
				if (shField.incoming_information_top)
					RenderPipeVert(renderRect, 0, h / 2, w / 3);
				if (shField.incoming_information_bottom)
					RenderPipeVert(renderRect, h / 2, 0, w / 3);

				RenderInsetEllipse(renderRect, w / 9, w / 9, true);
			}
			else
			{
				bool vert = shField.information[(int)BeGraphDirection.TopBottom].outgoing_direction_bottom
					|| shField.information[(int)BeGraphDirection.TopBottom_sm].outgoing_direction_bottom
					|| shField.information[(int)BeGraphDirection.BottomTop].outgoing_direction_top
					|| shField.information[(int)BeGraphDirection.BottomTop_sm].outgoing_direction_top;

				bool horz = shField.information[(int)BeGraphDirection.LeftRight].outgoing_direction_right
					|| shField.information[(int)BeGraphDirection.LeftRight_sm].outgoing_direction_right
					|| shField.information[(int)BeGraphDirection.RightLeft].outgoing_direction_left
					|| shField.information[(int)BeGraphDirection.RightLeft_sm].outgoing_direction_left;

				bool curveTL = shField.information[(int)BeGraphDirection.TopBottom].outgoing_direction_left
					|| shField.information[(int)BeGraphDirection.TopBottom_sm].outgoing_direction_left
					|| shField.information[(int)BeGraphDirection.LeftRight].outgoing_direction_top
					|| shField.information[(int)BeGraphDirection.LeftRight_sm].outgoing_direction_top;

				bool curveTR = shField.information[(int)BeGraphDirection.TopBottom].outgoing_direction_right
					|| shField.information[(int)BeGraphDirection.TopBottom_sm].outgoing_direction_right
					|| shField.information[(int)BeGraphDirection.RightLeft].outgoing_direction_top
					|| shField.information[(int)BeGraphDirection.RightLeft_sm].outgoing_direction_top;

				bool curveBR = shField.information[(int)BeGraphDirection.BottomTop].outgoing_direction_right
					|| shField.information[(int)BeGraphDirection.BottomTop_sm].outgoing_direction_right
					|| shField.information[(int)BeGraphDirection.RightLeft].outgoing_direction_bottom
					|| shField.information[(int)BeGraphDirection.RightLeft_sm].outgoing_direction_bottom;

				bool curveBL = shField.information[(int)BeGraphDirection.BottomTop].outgoing_direction_left
					|| shField.information[(int)BeGraphDirection.BottomTop_sm].outgoing_direction_left
					|| shField.information[(int)BeGraphDirection.LeftRight].outgoing_direction_bottom
					|| shField.information[(int)BeGraphDirection.LeftRight_sm].outgoing_direction_bottom;

				bool left = shField.information[(int)BeGraphDirection.LeftRight].outgoing_direction_left
					|| shField.information[(int)BeGraphDirection.LeftRight_sm].outgoing_direction_left;

				bool right = shField.information[(int)BeGraphDirection.RightLeft].outgoing_direction_right
					|| shField.information[(int)BeGraphDirection.RightLeft_sm].outgoing_direction_right;

				bool top = shField.information[(int)BeGraphDirection.TopBottom].outgoing_direction_top
					|| shField.information[(int)BeGraphDirection.TopBottom_sm].outgoing_direction_top;

				bool bottom = shField.information[(int)BeGraphDirection.BottomTop].outgoing_direction_bottom
					|| shField.information[(int)BeGraphDirection.BottomTop_sm].outgoing_direction_bottom;

				bool jumpHorz = shField.information[(int)BeGraphDirection.LeftRight].hl_jumpover
					|| shField.information[(int)BeGraphDirection.LeftRight_sm].hl_jumpover
					|| shField.information[(int)BeGraphDirection.RightLeft].hl_jumpover
					|| shField.information[(int)BeGraphDirection.RightLeft_sm].hl_jumpover;

				bool jumpVert = shField.information[(int)BeGraphDirection.TopBottom].hl_jumpover
					|| shField.information[(int)BeGraphDirection.TopBottom_sm].hl_jumpover
					|| shField.information[(int)BeGraphDirection.BottomTop].hl_jumpover
					|| shField.information[(int)BeGraphDirection.BottomTop_sm].hl_jumpover;

				if (horz)
					RenderPipeHorz(renderRect, 0, 0, w / 3);
				if (vert)
					RenderPipeVert(renderRect, 0, 0, w / 3);

				if (curveBL)
					RenderCurve(renderRect.bl, w / 2, h / 2, w / 3, 0, 4, 16);

				if (curveTL)
					RenderCurve(renderRect.tl, w / 2, h / 2, w / 3, 4, 8, 16);

				if (curveTR)
					RenderCurve(renderRect.tr, w / 2, h / 2, w / 3, 8, 12, 16);

				if (curveBR)
					RenderCurve(renderRect.br, w / 2, h / 2, w / 3, 12, 16, 16);

				if (left)
					RenderPipeHorz(renderRect, 0, w / 2, w / 3);
				if (right)
					RenderPipeHorz(renderRect, w / 2, 0, w / 3);
				if (top)
					RenderPipeVert(renderRect, 0, h / 2, w / 3);
				if (bottom)
					RenderPipeVert(renderRect, h / 2, 0, w / 3);

				if (jumpHorz)
					RenderPipeDottedHorz(renderRect, w / 3, w / 8);
				if (jumpVert)
					RenderPipeDottedVert(renderRect, w / 8, w / 3);

			}
		}

		public void GetPointInProgram(int px, int py, out int selx, out int sely)
		{
			double offx;
			double offy;
			double w;
			double h;
			CalcProgPos(out offx, out offy, out w, out h);

			int iselx = (int)((px - offx + Zoom.Peek().bl.X * w) / w);
			int isely = (int)((py - offy + Zoom.Peek().bl.Y * h) / h);

			if (iselx >= 0 && isely >= 0 && iselx < prog.Width && isely < prog.Height)
			{
				selx = iselx;
				sely = isely;
			}
			else
			{
				selx = -1;
				sely = -1;
			}
		}

		public void InitSyntaxHighlighting()
		{
			font = FontRasterSheet.Create(RunOptions.SYNTAX_HIGHLIGHTING != RunOptions.SH_NONE, Color.Black, Color.White);

			ExtendedSHGraph = null;
			if (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED)
			{
				if (prog.Width * prog.Height > MAX_EXTENDEDSH_SIZE)
				{
					RunOptions.SYNTAX_HIGHLIGHTING = RunOptions.SH_SIMPLE;
				}
				else
				{
					ExtendedSHGraph = new BeGraph(prog.Width, prog.Height);
					ExtendedSHGraph.Calculate(BeGraphHelper.parse(prog.Raster));
				}


			}
		}

		private string GetCodeTypeString()
		{
			return prog.IsBefunge93() ?
				"Befunge-93" :
				("Befunge-98" + (
					(RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED &&
					ExtendedSHGraph.EffectiveWidth <= 80 &&
					ExtendedSHGraph.EffectiveHeight <= 25) ?
						" (effective Befunge-93)" :
						""
					)
				);
		}

		public Bitmap GrabCurrentResScreenshot()
		{
			double rOffX;
			double rOffY;
			double rWidth;
			double rHeight;
			CalcProgPos(out rOffX, out rOffY, out rWidth, out rHeight);
			rWidth *= prog.Width;
			rHeight *= prog.Height;

			int w = Width;
			int h = Height;

			MakeCurrent();

			DoRender(false, false, "");

			if (GraphicsContext.CurrentContext == null)
				throw new GraphicsContextMissingException();

			Bitmap bmp = new Bitmap(w, h);
			System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			GL.ReadPixels(0, 0, w, h, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
			bmp.UnlockBits(data);

			bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

			return bmp.Clone(new Rectangle((int)rOffX, (int)rOffY, (int)rWidth, (int)rHeight), bmp.PixelFormat);
		}

		public Bitmap GrabFullResScreenshot()
		{
			int pixelW = 8 * prog.Width;
			int pixelH = 12 * prog.Height;

			double rOffX;
			double rOffY;
			double rWidth;
			double rHeight;
			CalcProgPos(out rOffX, out rOffY, out rWidth, out rHeight);

			int w = pixelW;
			int h = pixelH;

			rOffX = 0;
			rOffY = 0;

			rWidth = 8;
			rHeight = 12;

			GL.Viewport(0, 0, pixelW, pixelH);

			MakeCurrent();

			//##################################################
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.ClearColor(Color.FromArgb(244, 244, 244));

			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(0.0, pixelW, 0.0, pixelH, 0.0, 4.0);

			GL.Color3(1.0, 1.0, 1.0);

			if (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED)
				Render_HQ_sh(rOffX, rOffY, rWidth, rHeight, false);
			else
				Render_HQ(rOffX, rOffY, rWidth, rHeight);
			//##################################################

			if (GraphicsContext.CurrentContext == null)
				throw new GraphicsContextMissingException();

			Bitmap bmp = new Bitmap(w, h);
			System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			GL.ReadPixels(0, 0, w, h, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
			bmp.UnlockBits(data);

			bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

			GL.Viewport(0, 0, Width, Height);

			return bmp;
		}

		public bool ProcessProgramChanges()
		{
			Tuple<long, long, long> shChange; // <x, y, char>

			if (prog.RasterChanges.TryDequeue(out shChange))
			{
				if (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED)
				{
					if (shChange.Item1 == -1 && shChange.Item2 == -1 && shChange.Item3 == -1)
						ExtendedSHGraph.Calculate(BeGraphHelper.parse(prog.Raster)); // recalc
					else
						ExtendedSHGraph.Update(shChange.Item1, shChange.Item2, BeGraphCommand.getCommand(shChange.Item3), prog.PC.X, prog.PC.Y, prog.Delta.X, prog.Delta.Y);

					if (prog.RasterChanges.Count > 4)
					{
						while (prog.RasterChanges.TryDequeue(out shChange))
						{
							if (shChange.Item1 == -1 && shChange.Item2 == -1 && shChange.Item3 == -1)
								ExtendedSHGraph.Calculate(BeGraphHelper.parse(prog.Raster)); // recalc
							else
							{
								bool updateResult = ExtendedSHGraph.Update(shChange.Item1, shChange.Item2, BeGraphCommand.getCommand(shChange.Item3), prog.PC.X, prog.PC.Y, prog.Delta.X, prog.Delta.Y);

								if (updateResult)
								{
									return false;
								}
							}
						}
					}
				}
				else
				{
					while (prog.RasterChanges.TryDequeue(out shChange)) { /**/ }
				}
			}

			return true;
		}

		public void ResetProg(BefunProg p, BeGraph g, bool keepView = false)
		{
			ExtendedSHGraph = g;
			prog = p;

			if (keepView)
				Zoom = new ZoomController(prog, Zoom);
			else
				Zoom = new ZoomController(prog);
		}

		public string GetExecutionData()
		{
			StringBuilder buildr = new StringBuilder();

			buildr.AppendLine(String.Format("FPS: {0} (U := {1}ms | R := {2}ms | L := {3}ms)", (int)FPS.Frequency, (int)UpdateTimer.Time, (int)RenderTimer.Time, (int)prog.LogicTimer.Time));
			buildr.AppendLine(String.Format("SPEED: {0}", GetFreqFormatted(prog.Freq.Frequency)));
			buildr.AppendLine(String.Format("STEPS: {0:n0} {1} (Average: {2})", prog.StepCount, prog.Delta.isZero() ? "(stopped)" : "", GetFreqFormatted(((double)prog.StepCount / (prog.GetExecutedTime() + 1)) * 1000)));
			buildr.AppendLine(String.Format("SPEED (avg): {0}", GetFreqFormatted((double)prog.StepCount / (prog.GetExecutedTime() + 1))));
			buildr.AppendLine(String.Format("Time: {0:n0} ms", prog.GetExecutedTime()));
			buildr.AppendLine(String.Format("UndoLog: {0}", prog.UndoLog.Enabled ? prog.UndoLog.Size.ToString() : "disabled"));
			buildr.AppendLine(GetCodeTypeString());

			return buildr.ToString();
		}
	}
}
