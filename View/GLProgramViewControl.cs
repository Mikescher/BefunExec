using BefunExec.Logic;
using BefunExec.View.OpenGL;
using BefunExec.View.OpenGL.OGLMath;
using BefunHighlight;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using QuickFont;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BefunExec.View
{
	public class GLProgramViewControl : GLExtendedViewControl
	{
		public const int MAX_HQ_CELLCOUNT = 16 * 1000;
		public const int MAX_MQ_CELLCOUNT = 400 * 1000;
		public const int LQ_CELLCOUNT = 50 * 1000;

		public DebugTimer updateTimer = new DebugTimer();
		public DebugTimer renderTimer = new DebugTimer();

		public ZoomController zoom;

		private FontRasterSheet font;
		private FontRasterSheet nop_font;
		private FontRasterSheet stringfont;
		private FontRasterSheet bwfont;

		private QFont DebugFont;
		private QFont BoxFont;

		public BeGraph ExtendedSHGraph = null;
		private BefunProg prog;

		public GLProgramViewControl()
		{
			loaded = false;
		}

		public void DoInit(BefunProg p)
		{
			this.prog = p;
			this.zoom = new ZoomController(prog);

			this.MakeCurrent();

			GL.Enable(EnableCap.Texture2D);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			GL.Disable(EnableCap.CullFace);
			GL.Disable(EnableCap.DepthTest);

			QFontBuilderConfiguration builderConfig = new QFontBuilderConfiguration(true);
			builderConfig.ShadowConfig.blurRadius = 1; //reduce blur radius because font is very small
			builderConfig.TextGenerationRenderHint = TextGenerationRenderHint.ClearTypeGridFit; //best render hint for this font

			DebugFont = new QFont(new Font("Arial", 8));
			DebugFont.Options.DropShadowActive = true;
			DebugFont.Options.Colour = Color4.Black;

			BoxFont = new QFont(new Font("Arial", 16));
			BoxFont.Options.DropShadowActive = true;
			BoxFont.Options.Colour = Color4.Black;

			initSyntaxHighlighting();

			stringfont = FontRasterSheet.create(false, Color.DarkGreen, Color.FromArgb(212, 255, 212));
			nop_font = FontRasterSheet.create(false, Color.Black, Color.FromArgb(244, 244, 244));
			bwfont = FontRasterSheet.create(false, Color.Black, Color.White);

			zoom.Push(RunOptions.INIT_ZOOM);

			loaded = true;
		}

		public void DoRender(bool do_swap /* = true */, bool renderDebug, string currInput)
		{
			#region INIT

			fps.Inc();

			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.ClearColor(Color.FromArgb(244, 244, 244));

			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(0.0, this.Width, 0.0, this.Height, 0.0, 4.0);

			GL.Color3(1.0, 1.0, 1.0);

			#endregion

			#region SIZE

			double offx;
			double offy;
			double w; // width & height of an single cell
			double h;
			calcProgPos(out offx, out offy, out w, out h);

			int cellcount = zoom.Peek().Width * zoom.Peek().Height;

			#endregion

			#region RENDER

			int quality = -1;

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

			zoom.renderSelection(offx, offy, w, h);

			#endregion

			#region INPUT

			if (prog.mode == BefunProg.MODE_IN_INT /*|| showInputASCIIMessageToolStripMenuItem.Checked*/)
			{
				int bw = 512;
				int bh = 128;

				int box = (this.Width - bw) / 2;
				int boy = (this.Height - bh) / 2;

				Rect2d rect = new Rect2d(box, boy, bw, bh);

				GL.Disable(EnableCap.Texture2D);
				GL.Begin(BeginMode.Quads);
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

				RenderFont(this.Height, new Vec2d(box, boy), "Please enter a " + ((prog.mode == BefunProg.MODE_IN_INT) ? "number" : "character"), -1, BoxFont, true);

				RenderFont(this.Height, new Vec2d(box, boy + 64), currInput, -1, BoxFont, true);
			}

			#endregion

			#region DEBUG

			if (renderDebug)
			{
				RenderFont(this.Height, new Vec2d(0f, 00f), String.Format("FPS: {0} (U := {1}ms | R := {2}ms | L := {3}ms)", (int)fps.Frequency, (int)updateTimer.Time, (int)renderTimer.Time, (int)prog.logicTimer.Time), -1, DebugFont, true);
				RenderFont(this.Height, new Vec2d(0f, 20f), String.Format("SPEED: {0}", getFreqFormatted(prog.freq.Frequency)), -1, DebugFont, true);
				RenderFont(this.Height, new Vec2d(0f, 40f), String.Format("STEPS: {0:n0}", prog.StepCount), -1, DebugFont, true);
				RenderFont(this.Height, new Vec2d(0f, 60f), String.Format("Time: {0:n0} ms", prog.getExecutedTime()), -1, DebugFont, true);
				RenderFont(this.Height, new Vec2d(0f, 80f), String.Format("UndoLog: {0}", prog.undoLog.enabled ? prog.undoLog.size.ToString() : "disabled"), -1, DebugFont, true);
				RenderFont(this.Height, new Vec2d(0f, 100f), String.Format("Rendermode: [{0}] {1} (= {2:#,0} sprites)", quality, (new[] { "High Quality", "Low Quality", "Spritemap" })[quality], cellcount), -1, DebugFont, true);
				RenderFont(this.Height, new Vec2d(0f, 120f), getCodeTypeString(), -1, DebugFont, true);
			}

			#endregion

			#region FINISH

			if (do_swap)
				this.SwapBuffers();

			#endregion
		}

		public void DoMouseDown(MouseEventArgs e)
		{
			int selx, sely;
			getPointInProgram(e.X, e.Y, out selx, out sely);

			zoom.DoMouseDown(e, selx, sely);
		}

		public void DoMouseMove(MouseEventArgs e)
		{
			int selx, sely;
			getPointInProgram(e.X, e.Y, out selx, out sely);

			zoom.DoMouseMove(e, selx, sely);
		}

		public void DoMouseUp(MouseEventArgs e)
		{
			int selx, sely;
			getPointInProgram(e.X, e.Y, out selx, out sely);

			zoom.DoMouseUp(e, selx, sely);
		}

		public void DoMouseWheel(MouseEventArgs e)
		{
			zoom.DoMouseWheel(e);
		}

		private void Render_LQ(double offx, double offy, double w, double h)
		{
			long now = Environment.TickCount;

			double oldW = w;
			double oldH = h;

			double targetW = zoom.Peek().Width * w;
			double targetH = zoom.Peek().Height * h;

			double ratio = (zoom.Peek().Width * 1.0) / zoom.Peek().Height;
			double frenderH = Math.Sqrt(LQ_CELLCOUNT / ratio);
			double frenderW = LQ_CELLCOUNT / frenderH;

			int renderW = (int)Math.Ceiling(frenderW);
			int renderH = (int)Math.Ceiling(frenderH);

			int ox = zoom.Peek().bl.X;
			int oy = zoom.Peek().bl.Y;

			renderW = Math.Min(prog.Width, ox + renderW) - ox;
			renderH = Math.Min(prog.Height, oy + renderH) - oy;

			renderW = Math.Min(zoom.Peek().Width, renderW);
			renderH = Math.Min(zoom.Peek().Height, renderH);

			w = targetW / renderW;
			h = targetH / renderH;

			double scaleX = w / oldW;
			double scaleY = h / oldH;

			GL.Disable(EnableCap.Texture2D);

			GL.Begin(BeginMode.Quads);

			long last = 0;

			for (int sx = 0; sx < renderW; sx++)
			{
				for (int sy = 0; sy < renderH; sy++)
				{
					int x = ox + (int)(sx * scaleX);
					int y = oy + (int)(sy * scaleY);

					bool docol = last != prog.raster[x, y];

					font.RenderLQ(docol, new Rect2d(offx + sx * w, offy - sy * h + targetH - oldH, w, h), -4, prog[x, y]);

					last = prog.raster[x, y];
				}
			}

			GL.End();

			GL.Enable(EnableCap.Texture2D);
		}

		private void Render_MQ(double offx, double offy, double w, double h)
		{
			long now = Environment.TickCount;

			GL.Disable(EnableCap.Texture2D);

			GL.Begin(BeginMode.Quads);

			long last = 0;

			for (int x = zoom.Peek().bl.X; x < zoom.Peek().tr.X; x++)
			{
				for (int y = zoom.Peek().bl.Y; y < zoom.Peek().tr.Y; y++)
				{
					double decay_perc = (now - prog.decay_raster[x, y] * 1d) / RunOptions.DECAY_TIME;

					if (!prog.breakpoints[x, y] && prog.raster[x, y] == ' ' && decay_perc >= 1)
						continue;

					bool docol = true;
					if (prog.breakpoints[x, y])
					{
						GL.Color3(0.0, 0.0, 1.0);

						docol = false;
					}
					else if (decay_perc < 0.66)
					{
						GL.Color3(1.0, 0.0, 0.0);

						docol = false;
					}
					else if (last == prog.raster[x, y])
					{
						docol = false;
					}

					font.RenderLQ(docol, new Rect2d(offx + (x - zoom.Peek().bl.X) * w, offy + ((zoom.Peek().Height - 1) - (y - zoom.Peek().bl.Y)) * h, w, h), -4, prog[x, y]);

					last = (decay_perc < 0.66 || prog.breakpoints[x, y]) ? int.MinValue : prog.raster[x, y];
				}
			}

			GL.End();

			GL.Enable(EnableCap.Texture2D);
		}

		private void Render_MQ_sh(double offx, double offy, double w, double h)
		{
			long now = Environment.TickCount;

			GL.Disable(EnableCap.Texture2D);

			GL.Begin(BeginMode.Quads);

			long last = 0;

			for (int x = zoom.Peek().bl.X; x < zoom.Peek().tr.X; x++)
			{
				for (int y = zoom.Peek().bl.Y; y < zoom.Peek().tr.Y; y++)
				{
					double decay_perc = (now - prog.decay_raster[x, y] * 1d) / RunOptions.DECAY_TIME;

					if (!prog.breakpoints[x, y] && prog.raster[x, y] == ' ' && decay_perc >= 1)
						continue;

					bool docol = true;
					if (prog.breakpoints[x, y])
					{
						GL.Color3(0.0, 0.0, 1.0);

						docol = false;
					}
					else if (decay_perc < 0.66)
					{
						GL.Color3(1.0, 0.0, 0.0);

						docol = false;
					}
					else if (last == prog.raster[x, y])
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

					font.RenderLQ(docol, new Rect2d(offx + (x - zoom.Peek().bl.X) * w, offy + ((zoom.Peek().Height - 1) - (y - zoom.Peek().bl.Y)) * h, w, h), -4, prog[x, y]);

					last = (decay_perc < 0.66 || prog.breakpoints[x, y]) ? int.MinValue : prog.raster[x, y];
				}
			}

			GL.End();

			GL.Enable(EnableCap.Texture2D);
		}

		private void Render_HQ(double offx, double offy, double w, double h)
		{
			long now = Environment.TickCount;

			int f_binded = -1;

			double p_r = -1;
			double p_g = -1;
			double p_b = -1;

			for (int x = zoom.Peek().bl.X; x < zoom.Peek().tr.X; x++)
			{
				for (int y = zoom.Peek().bl.Y; y < zoom.Peek().tr.Y; y++)
				{
					double decay_perc = (RunOptions.DECAY_TIME != 0) ? (1 - (now - prog.decay_raster[x, y] * 1d) / RunOptions.DECAY_TIME) : (prog.decay_raster[x, y]);
					decay_perc = Math.Min(1, decay_perc);
					if (x == prog.PC.X && y == prog.PC.Y)
						decay_perc = 1;

					double r = prog.breakpoints[x, y] ? decay_perc : 1;
					double g = prog.breakpoints[x, y] ? 0 : (1 - decay_perc);
					double b = prog.breakpoints[x, y] ? (1 - decay_perc) : (1 - decay_perc);

					if (p_r != r || p_g != g || p_b != b)
						GL.Color3(p_r = r, p_g = g, p_b = b);

					Rect2d renderRect = new Rect2d(offx + (x - zoom.Peek().bl.X) * w, offy + ((zoom.Peek().Height - 1) - (y - zoom.Peek().bl.Y)) * h, w, h);

					if (prog.breakpoints[x, y] || decay_perc > 0.25)
					{
						if (f_binded != 1)
							bwfont.bind();
						bwfont.Render(renderRect, -4, prog[x, y]);
						f_binded = 1;
					}
					else
					{
						if (f_binded != 2)
							font.bind();
						font.Render(renderRect, -4, prog[x, y]);
						f_binded = 2;
					}
				}
			}
		}

		private void Render_HQ_sh(double offx, double offy, double w, double h, bool renderDebug)
		{
			if (ExtendedSHGraph == null)
			{
				initSyntaxHighlighting(); // re-init
				return;
			}

			long now = Environment.TickCount;

			int f_binded = -1;

			double p_r = -1;
			double p_g = -1;
			double p_b = -1;

			for (int x = zoom.Peek().bl.X; x < zoom.Peek().tr.X; x++)
			{
				for (int y = zoom.Peek().bl.Y; y < zoom.Peek().tr.Y; y++)
				{
					double decay_perc = (RunOptions.DECAY_TIME != 0) ? (1 - (now - prog.decay_raster[x, y] * 1d) / RunOptions.DECAY_TIME) : (prog.decay_raster[x, y]);
					decay_perc = Math.Min(1, decay_perc);
					if (x == prog.PC.X && y == prog.PC.Y)
						decay_perc = 1;

					double r = prog.breakpoints[x, y] ? decay_perc : 1;
					double g = prog.breakpoints[x, y] ? 0 : (1 - decay_perc);
					double b = prog.breakpoints[x, y] ? (1 - decay_perc) : (1 - decay_perc);

					if (p_r != r || p_g != g || p_b != b)
						GL.Color3(p_r = r, p_g = g, p_b = b);

					Rect2d renderRect = new Rect2d(offx + (x - zoom.Peek().bl.X) * w, offy + ((zoom.Peek().Height - 1) - (y - zoom.Peek().bl.Y)) * h, w, h);

					if (prog.breakpoints[x, y] || decay_perc > 0.25)
					{
						if (f_binded != 1)
							bwfont.bind();
						bwfont.Render(renderRect, -4, prog[x, y]);
						f_binded = 1;
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
							if (f_binded != 4)
								nop_font.bind();
							nop_font.Render(renderRect, -4, prog[x, y]);
							f_binded = 4;
						}
						else if (type == HighlightType.Command || type == HighlightType.String_and_Command)
						{
							if (f_binded != 2)
								font.bind();
							font.Render(renderRect, -4, prog[x, y]);
							f_binded = 2;
						}
						else if (type == HighlightType.String)
						{
							if (f_binded != 3)
								stringfont.bind();
							stringfont.Render(renderRect, -4, prog[x, y]);
							f_binded = 3;
						}

					}

					if (renderDebug)
						renderBeGraphDebug(w, h, x, y, renderRect);

				}
			}
		}

		private void calcProgPos(out double offx, out double offy, out double w, out double h)
		{
			int th = zoom.Peek().Height - 1;

			offx = 0;
			offy = 0;

			w = ((this.Width) * 1.0) / zoom.Peek().Width;
			h = (this.Height * 1.0) / zoom.Peek().Height;

			if ((w / h) < (8.0 / 12.0))
			{
				offy = h * zoom.Peek().Height;
				h = (12.0 * w) / (8.0);
				offy -= h * zoom.Peek().Height;
				offy /= 2;
				offx = 0;
			}
			else if ((w / h) > (8.0 / 12.0))
			{
				offx = w * zoom.Peek().Width;
				w = (8.0 * h) / (12.0);
				offx -= w * zoom.Peek().Width;
				offx /= 2;
			}
			else
			{
				offx = 0;
			}
		}

		private void renderBeGraphDebug(double w, double h, int x, int y, Rect2d renderRect)
		{
			HighlightField sh_field;

			try
			{
				sh_field = ExtendedSHGraph.fields[x, y];
				if (sh_field == null)
					return;
			}
			catch (Exception)
			{
				return; // safety quit
			}

			if (sh_field.incoming_information && !sh_field.outgoing_information)
			{
				if (sh_field.incoming_information_left)
					renderPipeHorz(renderRect, 0, w / 2, w / 3);
				if (sh_field.incoming_information_right)
					renderPipeHorz(renderRect, w / 2, 0, w / 3);
				if (sh_field.incoming_information_top)
					renderPipeVert(renderRect, 0, h / 2, w / 3);
				if (sh_field.incoming_information_bottom)
					renderPipeVert(renderRect, h / 2, 0, w / 3);

				renderInsetEllipse(renderRect, w / 9, w / 9, true);
			}
			else
			{
				bool vert = sh_field.information[(int)BeGraphDirection.TopBottom].outgoing_direction_bottom
					|| sh_field.information[(int)BeGraphDirection.TopBottom_sm].outgoing_direction_bottom
					|| sh_field.information[(int)BeGraphDirection.BottomTop].outgoing_direction_top
					|| sh_field.information[(int)BeGraphDirection.BottomTop_sm].outgoing_direction_top;

				bool horz = sh_field.information[(int)BeGraphDirection.LeftRight].outgoing_direction_right
					|| sh_field.information[(int)BeGraphDirection.LeftRight_sm].outgoing_direction_right
					|| sh_field.information[(int)BeGraphDirection.RightLeft].outgoing_direction_left
					|| sh_field.information[(int)BeGraphDirection.RightLeft_sm].outgoing_direction_left;

				bool curve_tl = sh_field.information[(int)BeGraphDirection.TopBottom].outgoing_direction_left
					|| sh_field.information[(int)BeGraphDirection.TopBottom_sm].outgoing_direction_left
					|| sh_field.information[(int)BeGraphDirection.LeftRight].outgoing_direction_top
					|| sh_field.information[(int)BeGraphDirection.LeftRight_sm].outgoing_direction_top;

				bool curve_tr = sh_field.information[(int)BeGraphDirection.TopBottom].outgoing_direction_right
					|| sh_field.information[(int)BeGraphDirection.TopBottom_sm].outgoing_direction_right
					|| sh_field.information[(int)BeGraphDirection.RightLeft].outgoing_direction_top
					|| sh_field.information[(int)BeGraphDirection.RightLeft_sm].outgoing_direction_top;

				bool curve_br = sh_field.information[(int)BeGraphDirection.BottomTop].outgoing_direction_right
					|| sh_field.information[(int)BeGraphDirection.BottomTop_sm].outgoing_direction_right
					|| sh_field.information[(int)BeGraphDirection.RightLeft].outgoing_direction_bottom
					|| sh_field.information[(int)BeGraphDirection.RightLeft_sm].outgoing_direction_bottom;

				bool curve_bl = sh_field.information[(int)BeGraphDirection.BottomTop].outgoing_direction_left
					|| sh_field.information[(int)BeGraphDirection.BottomTop_sm].outgoing_direction_left
					|| sh_field.information[(int)BeGraphDirection.LeftRight].outgoing_direction_bottom
					|| sh_field.information[(int)BeGraphDirection.LeftRight_sm].outgoing_direction_bottom;

				bool left = sh_field.information[(int)BeGraphDirection.LeftRight].outgoing_direction_left
					|| sh_field.information[(int)BeGraphDirection.LeftRight_sm].outgoing_direction_left;

				bool right = sh_field.information[(int)BeGraphDirection.RightLeft].outgoing_direction_right
					|| sh_field.information[(int)BeGraphDirection.RightLeft_sm].outgoing_direction_right;

				bool top = sh_field.information[(int)BeGraphDirection.TopBottom].outgoing_direction_top
					|| sh_field.information[(int)BeGraphDirection.TopBottom_sm].outgoing_direction_top;

				bool bottom = sh_field.information[(int)BeGraphDirection.BottomTop].outgoing_direction_bottom
					|| sh_field.information[(int)BeGraphDirection.BottomTop_sm].outgoing_direction_bottom;

				bool jump_horz = sh_field.information[(int)BeGraphDirection.LeftRight].hl_jumpover
					|| sh_field.information[(int)BeGraphDirection.LeftRight_sm].hl_jumpover
					|| sh_field.information[(int)BeGraphDirection.RightLeft].hl_jumpover
					|| sh_field.information[(int)BeGraphDirection.RightLeft_sm].hl_jumpover;

				bool jump_vert = sh_field.information[(int)BeGraphDirection.TopBottom].hl_jumpover
					|| sh_field.information[(int)BeGraphDirection.TopBottom_sm].hl_jumpover
					|| sh_field.information[(int)BeGraphDirection.BottomTop].hl_jumpover
					|| sh_field.information[(int)BeGraphDirection.BottomTop_sm].hl_jumpover;

				if (horz)
					renderPipeHorz(renderRect, 0, 0, w / 3);
				if (vert)
					renderPipeVert(renderRect, 0, 0, w / 3);

				if (curve_bl)
					renderCurve(renderRect.bl, w / 2, h / 2, w / 3, 0, 4, 16);

				if (curve_tl)
					renderCurve(renderRect.tl, w / 2, h / 2, w / 3, 4, 8, 16);

				if (curve_tr)
					renderCurve(renderRect.tr, w / 2, h / 2, w / 3, 8, 12, 16);

				if (curve_br)
					renderCurve(renderRect.br, w / 2, h / 2, w / 3, 12, 16, 16);

				if (left)
					renderPipeHorz(renderRect, 0, w / 2, w / 3);
				if (right)
					renderPipeHorz(renderRect, w / 2, 0, w / 3);
				if (top)
					renderPipeVert(renderRect, 0, h / 2, w / 3);
				if (bottom)
					renderPipeVert(renderRect, h / 2, 0, w / 3);

				if (jump_horz)
					renderPipeDottedHorz(renderRect, w / 3, w / 8);
				if (jump_vert)
					renderPipeDottedVert(renderRect, w / 8, w / 3);

			}
		}

		public void getPointInProgram(int px, int py, out int selx, out int sely)
		{
			double offx;
			double offy;
			double w;
			double h;
			calcProgPos(out offx, out offy, out w, out h);

			selx = -1;
			sely = -1;

			for (int x = 0; x < prog.Width; x++)
			{
				for (int y = 0; y < prog.Height; y++)
				{
					if (new Rect2d(offx + (x - zoom.Peek().bl.X) * w, offy + (y - zoom.Peek().bl.Y) * h, w, h).Includes(new Vec2d(px, py)))
					{
						selx = x;
						sely = y;
						return;
					}
				}
			}
		}

		public void initSyntaxHighlighting()
		{
			font = FontRasterSheet.create(RunOptions.SYNTAX_HIGHLIGHTING != RunOptions.SH_NONE, Color.Black, Color.White);

			ExtendedSHGraph = null;
			if (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED)
			{
				ExtendedSHGraph = new BeGraph(prog.Width, prog.Height);

				ExtendedSHGraph.Calculate(BeGraphHelper.parse(prog.raster));
			}
		}

		public string getCodeTypeString()
		{
			return prog.isBefunge93() ?
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
			double r_offx;
			double r_offy;
			double r_w;
			double r_h;
			calcProgPos(out r_offx, out r_offy, out r_w, out r_h);
			r_w *= prog.Width;
			r_h *= prog.Height;

			int w = this.Width;
			int h = this.Height;

			this.MakeCurrent();

			DoRender(false, false, "");

			if (GraphicsContext.CurrentContext == null)
				throw new GraphicsContextMissingException();

			Bitmap bmp = new Bitmap(w, h);
			System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			GL.ReadPixels(0, 0, w, h, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
			bmp.UnlockBits(data);

			bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

			return bmp.Clone(new Rectangle((int)r_offx, (int)r_offy, (int)r_w, (int)r_h), bmp.PixelFormat);
		}

		public Bitmap GrabFullResScreenshot()
		{
			int pixel_w = 8 * prog.Width;
			int pixel_h = 12 * prog.Height;

			double r_offx;
			double r_offy;
			double r_w;
			double r_h;
			calcProgPos(out r_offx, out r_offy, out r_w, out r_h);

			int w = pixel_w;
			int h = pixel_h;

			r_offx = 0;
			r_offy = 0;

			r_w = 8;
			r_h = 12;

			GL.Viewport(0, 0, pixel_w, pixel_h);

			this.MakeCurrent();

			//##################################################
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.ClearColor(Color.FromArgb(244, 244, 244));

			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(0.0, pixel_w, 0.0, pixel_h, 0.0, 4.0);

			GL.Color3(1.0, 1.0, 1.0);

			if (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED)
				Render_HQ_sh(r_offx, r_offy, r_w, r_h, false);
			else
				Render_HQ(r_offx, r_offy, r_w, r_h);
			//##################################################

			if (GraphicsContext.CurrentContext == null)
				throw new GraphicsContextMissingException();

			Bitmap bmp = new Bitmap(w, h);
			System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			GL.ReadPixels(0, 0, w, h, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
			bmp.UnlockBits(data);

			bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

			GL.Viewport(0, 0, this.Width, this.Height);

			return bmp;
		}

		public bool ProcessProgramChanges()
		{
			Tuple<long, long, long> sh_change; // <x, y, char>

			if (prog.RasterChanges.TryDequeue(out sh_change))
			{
				if (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED)
				{
					if (sh_change.Item1 == -1 && sh_change.Item2 == -1 && sh_change.Item3 == -1)
						ExtendedSHGraph.Calculate(BeGraphHelper.parse(prog.raster)); // recalc
					else
						ExtendedSHGraph.Update(sh_change.Item1, sh_change.Item2, BeGraphCommand.getCommand(sh_change.Item3), prog.PC.X, prog.PC.Y, prog.delta.X, prog.delta.Y);

					if (prog.RasterChanges.Count > 4)
					{
						while (prog.RasterChanges.TryDequeue(out sh_change))
						{
							if (sh_change.Item1 == -1 && sh_change.Item2 == -1 && sh_change.Item3 == -1)
								ExtendedSHGraph.Calculate(BeGraphHelper.parse(prog.raster)); // recalc
							else
							{
								bool upd_result = ExtendedSHGraph.Update(sh_change.Item1, sh_change.Item2, BeGraphCommand.getCommand(sh_change.Item3), prog.PC.X, prog.PC.Y, prog.delta.X, prog.delta.Y);

								if (upd_result)
								{
									return false;
								}
							}
						}
					}
				}
				else
				{
					while (prog.RasterChanges.TryDequeue(out sh_change))
						;
				}
			}

			return true;
		}

		public void resetProg(BefunProg p, BeGraph g)
		{
			this.ExtendedSHGraph = g;
			this.prog = p;
			this.zoom = new ZoomController(prog);
		}
	}
}
