using BefunExec.Logic;
using BefunExec.View.OpenGL.OGLMath;
using OpenTK.Graphics.OpenGL;
using QuickFont;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BefunExec.View
{
	public partial class MainForm : Form
	{
		#region Constants

		private const int FOLLOW_MODE_RADIUS = 15;

		#endregion

		#region Fields

		private bool loaded { get { return glStackView.loaded && glProgramView.loaded; } }

		private BefunProg prog;
		private string init_code;
		private int currentSpeedLevel = RunOptions.INIT_SPEED;

		private InteropKeyboard kb = new InteropKeyboard();

		private char? lastInput = null;

		private string currInput = "";

		private int currOutputHash = -1;

		private int QFontViewportState = -1; // 0=>glProgram  // 1=>glStack

		#endregion

		#region Konstruktor

		public MainForm(BefunProg bp, string code)
		{
			InitializeComponent();
			if (RunOptions.FILEPATH != null)
				this.Text = RunOptions.FILEPATH + " - " + Program.TITLE;
			else
				this.Text = Program.TITLE;

			prog = bp;
			init_code = code;

			syntaxHighlighting_noneToolStripMenuItem.Checked = (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_NONE);
			syntaxHighlighting_simpleToolStripMenuItem.Checked = (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_SIMPLE);
			syntaxHighlighting_extendedBefunHighlightToolStripMenuItem.Checked = (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED);
			aSCIIStackToolStripMenuItem.Checked = RunOptions.ASCII_STACK;
			followCursorToolStripMenuItem.Checked = RunOptions.FOLLOW_MODE;
			skipNOPsToolStripMenuItem.Checked = RunOptions.SKIP_NOP;
			debugModeToolStripMenuItem.Checked = RunOptions.DEBUGRUN;
			showTrailToolStripMenuItem.Checked = RunOptions.SHOW_DECAY;
			setSpeed(RunOptions.INIT_SPEED, true);

			Application.Idle += Application_Idle;
		}

		#endregion

		#region Display & FormEvents

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			prog.running = false;
			Application.Exit();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			this.ActiveControl = glProgramView;
		}

		void Application_Idle(object sender, EventArgs e)
		{
			if (!loaded) // Play nice
				return;

			if (glProgramView.IsIdle)
			{
				glProgramView.MakeCurrent();

				if (QFontViewportState != 0 && (prog.mode != BefunProg.MODE_RUN || kb.isDown(Keys.Tab))) // Only update when Font is rendered
				{
					QFont.InvalidateViewport();
					QFontViewportState = 0;
				}

				updateProgramView();

				glProgramView.DoRender(true, kb.isDown(Keys.Tab), currInput);

			}

			if (glStackView.IsIdle)
			{
				glStackView.MakeCurrent();

				if (QFontViewportState != 1)
				{
					QFont.InvalidateViewport();
					QFontViewportState = 1;
				}

				glStackView.DoRender();

			}

			glProgramView.Invalidate();
			glStackView.Invalidate();
		}

		#endregion

		#region Events

		private void glStackView_Load(object sender, EventArgs e)
		{
			glStackView.DoInit(prog);
		}

		private void glProgramView_Load(object sender, System.EventArgs e)
		{
			glProgramView.DoInit(prog);

			updateStatusbar();
		}

		private void glProgramView_Resize(object sender, EventArgs e)
		{
			glProgramView.MakeCurrent();

			GL.Viewport(0, 0, glProgramView.Width, glProgramView.Height);
			QFont.InvalidateViewport();

			glProgramView.Invalidate();
		}

		private void glStackView_Resize(object sender, EventArgs e)
		{
			glStackView.MakeCurrent();

			GL.Viewport(0, 0, glStackView.Width, glStackView.Height);
			QFont.InvalidateViewport();

			glStackView.Invalidate();
		}

		private void glProgramView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			glProgramView.DoMouseDown(e);

			updateStatusbar();
		}

		private void glProgramView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			glProgramView.DoMouseMove(e);

			updateStatusbar();
		}

		private void glProgramView_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			glProgramView.DoMouseUp(e);

			updateStatusbar();
		}

		private void glProgramView_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			lastInput = e.KeyChar;
		}

		private void glProgramView_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			glProgramView.DoMouseWheel(e);

			updateStatusbar();
		}

		#endregion

		#region Render & Update

		private void updateProgramView()
		{
			if (glProgramView.ContainsFocus)
				kb.update();

			Rect2i prog_rect = new Rect2i(0, 0, prog.Width, prog.Height);
			bool isrun = (prog.mode == BefunProg.MODE_RUN);

			#region Keys

			if (isrun && kb[Keys.Escape])
			{
				setFollowMode(false);

				if (glProgramView.zoom.Count > 1)
				{
					glProgramView.zoom.Pop();
				}
				else
				{
					//Exit();
				}
			}

			if (isrun && kb[Keys.Space] && prog.mode == BefunProg.MODE_RUN)
			{
				prog.paused = !prog.paused;
			}

			if (kb[Keys.Back] && currInput.Length > 0)
				currInput = currInput.Substring(0, currInput.Length - 1);

			if (kb[Keys.Enter])
			{
				if (prog.mode == BefunProg.MODE_IN_INT && currInput.Length > 0 && currInput != "-")
				{
					prog.push(int.Parse(currInput));
					currInput = "";
					lastInput = null;
					prog.mode = BefunProg.MODE_MOVEANDRUN;
				}
			}

			if (isrun && kb[Keys.Right])
				prog.doSingleStep = true;

			if (isrun && kb[Keys.D1])
				setSpeed(1, true);

			if (isrun && kb[Keys.D2])
				setSpeed(2, true);

			if (isrun && kb[Keys.D3])
				setSpeed(3, true);

			if (isrun && kb[Keys.D4])
				setSpeed(4, true);

			if (isrun && kb[Keys.D5])
				setSpeed(5, true);

			if (isrun && kb[Keys.R] & kb.isDown(Keys.ControlKey)) // no shortcut eval on purpose
				reload();

			if (isrun && kb[Keys.R] & !kb.isDown(Keys.ControlKey)) // no shortcut eval on purpose
				reset();

			if (isrun && kb[Keys.C])
				resetBPs();

			if (isrun && kb[Keys.F])
				setFollowMode(!RunOptions.FOLLOW_MODE);

			if (isrun && kb[Keys.P] && RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED && glProgramView.ExtendedSHGraph.isEffectiveSizeCalculated())
				glProgramView.zoom.Push(new Rect2i(0, 0, glProgramView.ExtendedSHGraph.EffectiveWidth, glProgramView.ExtendedSHGraph.EffectiveHeight));

			if (kb.AnyKey())
				updateStatusbar();

			#endregion

			#region Follow Mode

			if (isrun && RunOptions.FOLLOW_MODE)
			{
				Vec2i p = new Vec2i(prog.PC);

				Rect2i z = new Rect2i(p.X - FOLLOW_MODE_RADIUS, p.Y - FOLLOW_MODE_RADIUS, FOLLOW_MODE_RADIUS * 2, FOLLOW_MODE_RADIUS * 2);

				z.ForceInside(prog_rect);
				z.setInsideRatio_Expanding((12.0 * glProgramView.Width) / (8.0 * glProgramView.Height), prog_rect);

				if (glProgramView.zoom.Count > 1)
					glProgramView.zoom.Pop();
				glProgramView.zoom.Push(z);
			}

			#endregion

			#region SyntaxHighlighting

			if (!glProgramView.ProcessProgramChanges())
			{
				syntaxHighlighting_simpleToolStripMenuItem.Checked = true; // EMERGENCY EXIT

				Console.WriteLine();
				Console.WriteLine("!> Too much updates - stopping ext. Highlighting ...");
			}

			#endregion


			#region INPUT

			if (prog.mode != BefunProg.MODE_RUN)
			{
				if (lastInput != null)
				{
					if (prog.mode == BefunProg.MODE_IN_INT && (char.IsDigit(lastInput.Value) || (currInput.Length == 0 && lastInput.Value == '-')))
					{
						currInput += lastInput;
					}
					if (prog.mode == BefunProg.MODE_IN_CHAR && currInput.Length == 0)
					{
						// WAIT
					}
				}
			}
			else
			{
				currInput = "";
			}
			lastInput = null;

			// SHOW QUEQUE

			char[] chr_queque = prog.InputCharacters.ToArray();

			edInputQueque.Text = string.Join("", chr_queque);

			#endregion

			#region OUTPUT

			int progOHash = prog.simpleOutputHash;

			if (progOHash != currOutputHash)
			{
				currOutputHash = progOHash;

				String s;
				lock (prog.output)
				{
					s = prog.output.ToString();
				}

				edOutput.Text = s;
			}

			#endregion

		}

		#endregion

		#region Helper

		private void resetBPs()
		{
			for (int x = 0; x < prog.Width; x++)
			{
				for (int y = 0; y < prog.Height; y++)
				{
					prog.breakpoints[x, y] = false;
				}
			}
		}

		private void reset()
		{
			prog.reset_freeze_request = true;

			while (!prog.reset_freeze_answer)
				Thread.Sleep(0);

			prog.full_reset(init_code);

			setFollowMode(false);

			Console.WriteLine();
			Console.WriteLine();

			Console.WriteLine("########## OUTPUT ##########");

			Console.WriteLine();
			Console.WriteLine();

			prog.reset_freeze_request = false;
		}

		private void reload() // Not sure if threadsafe :-/
		{
			if (RunOptions.FILEPATH != null)
			{
				string code = BefungeFileHelper.LoadTextFile(RunOptions.FILEPATH);

				if (code == null)
				{
					Console.WriteLine();
					Console.WriteLine("Could not load program from Filepath: ");
					Console.WriteLine(RunOptions.FILEPATH);

					reset();
				}
				else
				{
					glProgramView.loaded = false;

					init_code = code;

					prog.reset_freeze_request = true;
					while (!prog.reset_freeze_answer)
						Thread.Sleep(0);
					prog.running = false;
					Thread.Sleep(250 + prog.curr_lvl_sleeptime);

					prog = new BefunProg(BefunProg.GetProg(init_code));
					glProgramView.resetProg(prog, null);
					glProgramView.zoom.Clear();
					glProgramView.zoom.Push(new Rect2i(0, 0, prog.Width, prog.Height));
					glProgramView.initSyntaxHighlighting();

					new Thread(new ThreadStart(prog.run)).Start();

					glProgramView.loaded = true;
				}
			}
			else
			{
				reset();
			}
		}

		private void setSpeed(int i, bool recheck)
		{
			currentSpeedLevel = i;

			switch (i)
			{
				case 1:
					if (prog.curr_lvl_sleeptime != RunOptions.SLEEP_TIME_1)
						prog.curr_lvl_sleeptime = RunOptions.SLEEP_TIME_1;
					if (recheck)
						lowToolStripMenuItem.Checked = true;
					break;
				case 2:
					if (prog.curr_lvl_sleeptime != RunOptions.SLEEP_TIME_2)
						prog.curr_lvl_sleeptime = RunOptions.SLEEP_TIME_2;
					if (recheck)
						middleToolStripMenuItem.Checked = true;
					break;
				case 3:
					if (prog.curr_lvl_sleeptime != RunOptions.SLEEP_TIME_3)
						prog.curr_lvl_sleeptime = RunOptions.SLEEP_TIME_3;
					if (recheck)
						fastToolStripMenuItem.Checked = true;
					break;
				case 4:
					if (prog.curr_lvl_sleeptime != RunOptions.SLEEP_TIME_4)
						prog.curr_lvl_sleeptime = RunOptions.SLEEP_TIME_4;
					if (recheck)
						veryFastToolStripMenuItem.Checked = true;
					break;
				case 5:
					if (prog.curr_lvl_sleeptime != RunOptions.SLEEP_TIME_5)
						prog.curr_lvl_sleeptime = RunOptions.SLEEP_TIME_5;

					if (recheck)
						fullToolStripMenuItem.Checked = true;
					break;
			}
		}

		private void setFollowMode(bool v)
		{
			if (!(v ^ RunOptions.FOLLOW_MODE)) // v == FOLLOW_MODE
				return;

			RunOptions.FOLLOW_MODE = v;
			followCursorToolStripMenuItem.Checked = RunOptions.FOLLOW_MODE;

			if (RunOptions.FOLLOW_MODE)
			{
				glProgramView.zoom.Push(glProgramView.zoom.Peek());
			}
			else
			{
				if (glProgramView.zoom.Count > 1)
					glProgramView.zoom.Pop();
			}
		}

		private void updateStatusbar()
		{
			int posx, posy;
			Point mp = glProgramView.PointToClient(Cursor.Position);
			glProgramView.getPointInProgram(mp.X, mp.Y, out posx, out posy);
			bool inControl = posx >= 0 && posy >= 0;

			if (inControl)
			{
				toolStripLabelPosition.Text = String.Format("Position: ({0:000}|{1:000})", posx, posy);
				toolStripLabelValue.Text = String.Format("Value: {0:0000}", prog.raster[posx, posy]);
			}
			else
			{
				toolStripLabelPosition.Text = String.Format("Position: ({0}|{1})", "???", "???");
				toolStripLabelValue.Text = String.Format("Value: {0}", "????");
			}
			toolStripLabelSize.Text = String.Format("Size: {0}x{1}", prog.Width, prog.Height);

			if (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED && glProgramView.ExtendedSHGraph.isEffectiveSizeCalculated())
				toolStripLabelEffectiveSize.Text = String.Format("Effective size: {0}x{1}", glProgramView.ExtendedSHGraph.EffectiveWidth, glProgramView.ExtendedSHGraph.EffectiveHeight);
			else
				toolStripLabelEffectiveSize.Text = String.Format("Effective size: {0}x{1}", '?', '?');

			toolStripLabelZoom.Text = String.Format("Zoom: x{0:0.##}", glProgramView.getZoomFactor());
			toolStripLabelBreakpoints.Text = String.Format("Breakpoints: {0}", prog.getBreakPointCount());
			toolStripLabelSpeed.Text = String.Format("Speed level: {0:}", currentSpeedLevel);
		}

		public Bitmap GrabScreenshot()
		{
			return glProgramView.GrabCurrentResScreenshot();
		}

		#endregion

		#region Menubar

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void resetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			reset();
		}

		private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			reload();
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog fd = new OpenFileDialog();

			fd.Filter = "Befunge-Program|*.b93;*.b98;*.tfd|Befunge-93|*.b93|Befunge-98|*.b98|TextFunge-Debug-File|*.tfd|All Files|*";
			fd.FilterIndex = 1;

			if (fd.ShowDialog() == DialogResult.OK)
			{
				string c = BefungeFileHelper.LoadTextFile(fd.FileName);
				if (c != null)
				{
					glProgramView.loaded = false;
					glStackView.loaded = false;

					glProgramView.zoom.Clear();

					init_code = c;
					RunOptions.FILEPATH = fd.FileName;

					prog.running = false;

					prog = new BefunProg(BefunProg.GetProg(init_code));

					glProgramView.resetProg(prog, null);

					new Thread(new ThreadStart(prog.run)).Start();
					glProgramView.initSyntaxHighlighting();

					this.Text = fd.FileName + " - " + Program.TITLE;

					glProgramView.zoom.Push(new Rect2i(0, 0, prog.Width, prog.Height));

					glProgramView.loaded = true;
					glStackView.loaded = true;
				}
			}
		}

		private void aSCIIStackToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RunOptions.ASCII_STACK = aSCIIStackToolStripMenuItem.Checked;
		}

		private void followCursorToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			setFollowMode(followCursorToolStripMenuItem.Checked);
		}

		private void speedToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			if (lowToolStripMenuItem.Checked)
				setSpeed(1, false);
			else if (middleToolStripMenuItem.Checked)
				setSpeed(2, false);
			else if (fastToolStripMenuItem.Checked)
				setSpeed(3, false);
			else if (veryFastToolStripMenuItem.Checked)
				setSpeed(4, false);
			else if (fullToolStripMenuItem.Checked)
				setSpeed(5, false);
		}

		private void zoomToInitialToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (RunOptions.FOLLOW_MODE) //NOT POSSIBLE WHILE FOLLOWING
				return;

			while (glProgramView.zoom.Count > 1)
			{
				glProgramView.zoom.Pop();
			}

			glProgramView.zoom.Push(RunOptions.INIT_ZOOM);
			if (glProgramView.zoom.Peek() == null || glProgramView.zoom.Peek().bl.X < 0 || glProgramView.zoom.Peek().bl.Y < 0 || glProgramView.zoom.Peek().tr.X > prog.Width || glProgramView.zoom.Peek().tr.Y > prog.Height)
				glProgramView.zoom.Pop();
		}

		private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (RunOptions.FOLLOW_MODE) //NOT POSSIBLE WHILE FOLLOWING
				return;

			if (glProgramView.zoom.Count > 1)
			{
				glProgramView.zoom.Pop();
			}
		}

		private void zoomCompleteOutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (RunOptions.FOLLOW_MODE) //NOT POSSIBLE WHILE FOLLOWING
				return;

			while (glProgramView.zoom.Count > 1)
			{
				glProgramView.zoom.Pop();
			}
		}

		private void runToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (prog.mode == BefunProg.MODE_RUN)
				prog.paused = !prog.paused;
		}

		private void stepToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (prog.mode != BefunProg.MODE_RUN)
				prog.doSingleStep = true;
		}

		private void skipNOPsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RunOptions.SKIP_NOP = skipNOPsToolStripMenuItem.Checked;
		}

		private void debugModeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RunOptions.DEBUGRUN = debugModeToolStripMenuItem.Checked;
		}

		private void showTrailToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RunOptions.SHOW_DECAY = showTrailToolStripMenuItem.Checked;
		}

		private void removeAllBreakpointsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			resetBPs();
		}

		private void showCompleteStackToolStripMenuItem_Click(object sender, EventArgs e)
		{
			StringBuilder s = new StringBuilder();

			lock (prog.Stack)
			{
				glStackView.currStack.AddRange(prog.Stack);
			}

			s.AppendLine("Stack<" + glStackView.currStack.Count + ">");

			s.AppendLine();
			s.AppendLine();

			for (int i = 0; i < glStackView.currStack.Count; i++)
			{
				long val = glStackView.currStack[i];

				if (RunOptions.ASCII_STACK && val >= 32 && val <= 126)
					s.AppendLine(string.Format("{0:0000} <{1}>", val, (char)val));
				else
					s.AppendLine(string.Format("{0:0000}", val));
			}

			new TextDisplayForm("Output", s.ToString()).ShowDialog();
		}

		private void showCompleteOutputToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string s;

			lock (prog.output)
			{
				s = prog.output.ToString();
			}

			new TextDisplayForm("Output", s).ShowDialog();
		}

		private void showCurrentStateToolStripMenuItem_Click(object sender, EventArgs e)
		{
			StringBuilder s = new StringBuilder();

			for (int y = 0; y < prog.Height; y++)
			{
				for (int x = 0; x < prog.Width; x++)
				{
					long chr = prog.raster[x, y];

					if (chr < ' ' || chr > '~')
						chr = ' ';

					s.Append((char)chr);
				}
				s.Append(Environment.NewLine);
			}

			new TextDisplayForm("Program", s.ToString()).ShowDialog();
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			(new AboutForm()).ShowDialog();
		}

		private void captureGIFToolStripMenuItem_Click(object sender, EventArgs e)
		{
			(new CaptureForm(this, prog)).ShowDialog();
		}

		private void syntaxhighlightingToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			if (!loaded)
				return;

			if (syntaxHighlighting_noneToolStripMenuItem.Checked)
			{
				RunOptions.SYNTAX_HIGHLIGHTING = RunOptions.SH_NONE;
			}
			else if (syntaxHighlighting_simpleToolStripMenuItem.Checked)
			{
				RunOptions.SYNTAX_HIGHLIGHTING = RunOptions.SH_SIMPLE;
			}
			else if (syntaxHighlighting_extendedBefunHighlightToolStripMenuItem.Checked)
			{
				RunOptions.SYNTAX_HIGHLIGHTING = RunOptions.SH_EXTENDED;
			}

			glProgramView.initSyntaxHighlighting();
		}

		private void createHDScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "PNG-Image|*.png";

			if (sfd.ShowDialog() == DialogResult.OK)
			{
				string fn = sfd.FileName;

				if (!fn.ToLower().EndsWith(".png"))
					fn += ".png";

				Bitmap b = glProgramView.GrabFullResScreenshot();

				b.Save(fn, ImageFormat.Png);
			}
		}

		#endregion

		#region Controls

		private void btnAddInput_Click(object sender, EventArgs e)
		{
			performBufferedInput();
		}

		private void edInput_KeyUp(object sender, KeyEventArgs e)
		{
			performBufferedInput();
		}

		private void performBufferedInput()
		{
			string s = edInput.Text;
			edInput.Text = "";

			foreach (char c in s)
			{
				prog.InputCharacters.Enqueue(c);
			}
		}

		#endregion
	}
}

//TODO Edit Code Dialog
//TODO Edit Stack Dialog (?)
//TODO Move PC (Change Direction) Dialog