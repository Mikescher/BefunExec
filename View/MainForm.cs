using BefunExec.Logic;
using BefunExec.View.OpenGL.OGLMath;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BefunExec.View
{
	public sealed partial class MainForm : Form
	{
		#region Constants

		private const int FOLLOW_MODE_RADIUS = 15;

		#endregion

		#region Fields

		private bool Loaded => glStackView.Loaded && glProgramView.Loaded;

		private BefunProg prog;
		private FileInformation initCode;

		private readonly InteropKeyboard keyboard = new InteropKeyboard();

		private char? lastInput = null;

		private string currInput = "";

		private int currOutputHash = -1;

		#endregion

		#region Konstruktor

		public MainForm(BefunProg bp, FileInformation code)
		{
			InitializeComponent();
			if (RunOptions.FILEPATH != null)
				Text = RunOptions.FILEPATH + " - " + Program.TITLE;
			else
				Text = Program.TITLE;

			prog = bp;
			initCode = code;

			syntaxHighlighting_noneToolStripMenuItem.Checked = (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_NONE);
			syntaxHighlighting_simpleToolStripMenuItem.Checked = (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_SIMPLE);
			syntaxHighlighting_extendedBefunHighlightToolStripMenuItem.Checked = (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED);
			aSCIIStackToolStripMenuItem.Checked = RunOptions.ASCII_STACK;
			followCursorToolStripMenuItem.Checked = RunOptions.FOLLOW_MODE;
			skipNOPsToolStripMenuItem.Checked = RunOptions.SKIP_NOP;
			debugModeToolStripMenuItem.Checked = RunOptions.DEBUGRUN;
			showTrailToolStripMenuItem.Checked = RunOptions.SHOW_DECAY;
			showStackReversedToolStripMenuItem.Checked = RunOptions.SHOW_STACK_REVERSED;
			showStackReversedToolStripMenuItem.Checked = RunOptions.PREPROCESSOR;
			enableUndoToolStripMenuItem.Checked = RunOptions.ENABLEUNDO;
			undoToolStripMenuItem.Enabled = RunOptions.ENABLEUNDO;
			prog.UndoLog.Enabled = RunOptions.ENABLEUNDO;
			SetSpeed(RunOptions.RUN_FREQUENCY_IDX, true);

			Application.Idle += Application_Idle;
		}

		#endregion

		#region Display & FormEvents

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			prog.Running = false;
			Application.Exit();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			ActiveControl = glProgramView;
		}

		void Application_Idle(object sender, EventArgs e)
		{
			if (!Loaded) // Play nice
				return;

			if (glProgramView.IsIdle)
			{
				glProgramView.MakeCurrent();

				glProgramView.UpdateTimer.Start();
				{
					UpdateProgramView();
				}
				glProgramView.UpdateTimer.Stop();

				glProgramView.RenderTimer.Start();
				{
					try
					{
						glProgramView.DoRender(true, keyboard.IsDown(Keys.Tab), currInput);
					}
					catch (Exception re)
					{
						Console.Out.WriteLine("Error in DoRender:\r\n" + re.ToString());
					}
				}
				glProgramView.RenderTimer.Stop();

			}

			if (glStackView.IsIdle)
			{
				glStackView.MakeCurrent();

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

		private void glProgramView_Load(object sender, EventArgs e)
		{
			glProgramView.DoInit(prog);

			UpdateStatusbar();
		}

		private void glProgramView_Resize(object sender, EventArgs e)
		{
			glProgramView.MakeCurrent();

			GL.Viewport(0, 0, glProgramView.Width, glProgramView.Height);

			glProgramView.Invalidate();
		}

		private void glStackView_Resize(object sender, EventArgs e)
		{
			glStackView.MakeCurrent();

			GL.Viewport(0, 0, glStackView.Width, glStackView.Height);

			glStackView.Invalidate();
		}

		private void glProgramView_MouseDown(object sender, MouseEventArgs e)
		{
			glProgramView.DoMouseDown(e);

			UpdateStatusbar();
		}

		private void glProgramView_MouseMove(object sender, MouseEventArgs e)
		{
			glProgramView.DoMouseMove(e);

			UpdateStatusbar();
		}

		private void glProgramView_MouseUp(object sender, MouseEventArgs e)
		{
			glProgramView.DoMouseUp(e);

			UpdateStatusbar();
		}

		private void glProgramView_KeyPress(object sender, KeyPressEventArgs e)
		{
			lastInput = e.KeyChar;
		}

		private void glProgramView_MouseWheel(object sender, MouseEventArgs e)
		{
			glProgramView.DoMouseWheel(e);

			UpdateStatusbar();
		}

		#endregion

		#region Update

		private void UpdateProgramView()
		{
			if (glProgramView.ContainsFocus)
				keyboard.Update();

			bool isrun = (prog.Mode == BefunProg.MODE_RUN);

			#region Keys

			if (isrun && keyboard[Keys.Escape])
			{
				SetFollowMode(false);

				glProgramView.Zoom.Pop();
			}

			if (isrun && keyboard[Keys.Space] && prog.Mode == BefunProg.MODE_RUN)
			{
				prog.Paused = !prog.Paused;
			}

			if (keyboard[Keys.Back] && currInput.Length > 0)
				currInput = currInput.Substring(0, currInput.Length - 1);

			if (keyboard[Keys.Enter])
			{
				if (prog.Mode == BefunProg.MODE_IN_INT && currInput.Length > 0 && currInput != "-")
				{
					prog.Push(int.Parse(currInput));
					currInput = "";
					lastInput = null;
					prog.Mode = BefunProg.MODE_MOVEANDRUN;
				}
			}

			if (isrun && keyboard[Keys.Right])
				prog.DoSingleStep = true;

			if (isrun && keyboard[Keys.Left])
				prog.DoSingleUndo = true;

			if (isrun && keyboard[Keys.D1])
				SetSpeed(RunOptions.STANDARDFREQ_1, true);

			if (isrun && keyboard[Keys.D2])
				SetSpeed(RunOptions.STANDARDFREQ_2, true);

			if (isrun && keyboard[Keys.D3])
				SetSpeed(RunOptions.STANDARDFREQ_3, true);

			if (isrun && keyboard[Keys.D4])
				SetSpeed(RunOptions.STANDARDFREQ_4, true);

			if (isrun && keyboard[Keys.D5])
				SetSpeed(RunOptions.STANDARDFREQ_5, true);

			if (isrun && keyboard[Keys.R] & keyboard.IsDown(Keys.ControlKey)) // no shortcut eval on purpose
				Reload();

			if (isrun && keyboard[Keys.R] & !keyboard.IsDown(Keys.ControlKey)) // no shortcut eval on purpose
				Reset();

			if (isrun && keyboard[Keys.C])
			{
				ResetBPs();
				ResetWatched();
			}

			if (isrun && keyboard[Keys.PageUp])
				IncSpeed();

			if (isrun && keyboard[Keys.PageDown])
				DecSpeed();

			if (isrun && keyboard[Keys.F])
				SetFollowMode(!RunOptions.FOLLOW_MODE);

			if (isrun && keyboard[Keys.P] && RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED && glProgramView.ExtendedSHGraph.isEffectiveSizeCalculated())
				glProgramView.Zoom.Push(new Rect2I(0, 0, glProgramView.ExtendedSHGraph.EffectiveWidth, glProgramView.ExtendedSHGraph.EffectiveHeight));

			if (keyboard.AnyKey())
				UpdateStatusbar();

			#endregion

			#region Follow Mode

			if (isrun && RunOptions.FOLLOW_MODE)
			{
				Rect2I progRect = new Rect2I(0, 0, prog.Width, prog.Height);
				Vec2I p = new Vec2I(prog.PC);
				Rect2I z = new Rect2I(p.X - FOLLOW_MODE_RADIUS, p.Y - FOLLOW_MODE_RADIUS, FOLLOW_MODE_RADIUS * 2, FOLLOW_MODE_RADIUS * 2);

				z.ForceInside(progRect);
				z.setInsideRatio_Expanding((12.0 * glProgramView.Width) / (8.0 * glProgramView.Height), progRect);

				glProgramView.Zoom.Pop();
				glProgramView.Zoom.Push(z);
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

			if (prog.Mode != BefunProg.MODE_RUN)
			{
				if (lastInput != null)
				{
					if (prog.Mode == BefunProg.MODE_IN_INT && (char.IsDigit(lastInput.Value) || (currInput.Length == 0 && lastInput.Value == '-')))
					{
						currInput += lastInput;
					}
					if (prog.Mode == BefunProg.MODE_IN_CHAR && currInput.Length == 0)
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

			char[] chrQueque = prog.InputCharacters.ToArray();

			edInputQueque.Text = string.Join("", chrQueque);

			#endregion

			#region OUTPUT

			int progOHash = prog.SimpleOutputHash;

			if (progOHash != currOutputHash)
			{
				currOutputHash = progOHash;

				String s;
				lock (prog.Output)
				{
					s = prog.Output.ToString();
				}

				edOutput.Text = s;
			}

			#endregion

		}

		#endregion

		#region Helper

		private void ResetBPs()
		{
			for (int x = 0; x < prog.Width; x++)
			{
				for (int y = 0; y < prog.Height; y++)
				{
					prog.Breakpoints[x, y] = false;
				}
			}
			prog.Breakpointcount = 0;
		}

		private void ResetWatched()
		{
			prog.WatchedFields = new List<WatchedField>();

			for (int x = 0; x < prog.Width; x++)
			{
				for (int y = 0; y < prog.Height; y++)
				{
					prog.WatchData[x, y] = false;
				}
			}
		}

		private void Reset()
		{
			prog.ResetFreezeRequest = true;

			while (!prog.ResetFreezeAnswer)
				Thread.Sleep(0);

			prog.full_reset(initCode);

			SetFollowMode(false);

			Console.WriteLine();
			Console.WriteLine();

			Console.WriteLine("########## OUTPUT ##########");

			Console.WriteLine();
			Console.WriteLine();

			prog.ResetFreezeRequest = false;
		}

		private void Reload() // Not sure if threadsafe :-/
		{
			if (RunOptions.FILEPATH != null)
			{
				var code = BefungeFileHelper.LoadTextFile(RunOptions.FILEPATH, RunOptions.PREPROCESSOR);

				if (code == null)
				{
					Console.WriteLine();
					Console.WriteLine("Could not load program from Filepath: ");
					Console.WriteLine(RunOptions.FILEPATH);

					Reset();
				}
				else
				{
					glProgramView.Loaded = false;

					var oldInitRaster = initCode.GetRaster();
					initCode = code;
					var newInintRaster = initCode.GetRaster();

					prog.ResetFreezeRequest = true;
					while (!prog.ResetFreezeAnswer)
						Thread.Sleep(0);
					prog.Running = false;
					Thread.Sleep(250 + (int)prog.GetActualSleepTime());

					var oldProg = prog;

					prog = new BefunProg(initCode);

					var keepView = oldProg.Width == prog.Width && oldProg.Height == prog.Height;

					glProgramView.ResetProg(prog, null, keepView);
					prog.UndoLog.Enabled = RunOptions.ENABLEUNDO;

					if (keepView)
					{
						for (int x = 0; x < prog.Width; x++)
							for (int y = 0; y < prog.Height; y++)
								if (oldInitRaster[x, y] == newInintRaster[x, y])
									prog.Raster[x, y] = oldProg.Raster[x, y];
						

						for (int x = 0; x < prog.Width; x++)
							for (int y = 0; y < prog.Height; y++)
							{
								prog.WatchData[x, y] = oldProg.WatchData[x, y];
							}

						prog.WatchedFields = oldProg.WatchedFields;


						for (int x = 0; x < prog.Width; x++)
							for (int y = 0; y < prog.Height; y++)
							{
								prog.Breakpoints[x, y] = oldProg.Breakpoints[x, y];
							}

						prog.Breakpointcount = oldProg.Breakpointcount;


						prog.PC = oldProg.PC;

						foreach (var s in oldProg.Stack)
							prog.Stack.Push(s);

						prog.Stringmode = oldProg.Stringmode;
						prog.Output.Append(oldProg.Output);
						prog.SimpleOutputHash = oldProg.SimpleOutputHash;
					}


					glStackView.ReInit(prog);

					glProgramView.InitSyntaxHighlighting();

					new Thread(prog.Run).Start();

					glProgramView.Loaded = true;
				}
			}
			else
			{
				Reset();
			}
		}

		private void SetSpeed(int freqIdx, bool recheck)
		{
			RunOptions.RUN_FREQUENCY_IDX = freqIdx;
			prog.CurrSleeptimeFreq = RunOptions.FREQUENCY_SLIDER[freqIdx];

			if (recheck)
				speedFreqBar.Value = freqIdx;

			UpdateStatusbar();
		}

		private void SetSyntaxHighlighting(int sh, bool recheck)
		{
			if (sh == RunOptions.SH_NONE)
				syntaxHighlighting_noneToolStripMenuItem.Checked = true;
			if (sh == RunOptions.SH_SIMPLE)
				syntaxHighlighting_simpleToolStripMenuItem.Checked = true;
			if (sh == RunOptions.SH_EXTENDED)
				syntaxHighlighting_extendedBefunHighlightToolStripMenuItem.Checked = true;
		}

		private void IncSpeed()
		{
			SetSpeed(Math.Min(RunOptions.RUN_FREQUENCY_IDX + 1, 15), true);
		}

		private void DecSpeed()
		{
			SetSpeed(Math.Max(RunOptions.RUN_FREQUENCY_IDX - 1, 0), true);
		}

		private void SetFollowMode(bool v)
		{
			if (!(v ^ RunOptions.FOLLOW_MODE)) // v == FOLLOW_MODE
				return;

			RunOptions.FOLLOW_MODE = v;
			followCursorToolStripMenuItem.Checked = RunOptions.FOLLOW_MODE;

			if (RunOptions.FOLLOW_MODE)
			{
				glProgramView.Zoom.Push(glProgramView.Zoom.Peek());
			}
			else
			{
				glProgramView.Zoom.Pop();
			}
		}

		private void UpdateStatusbar()
		{
			if (!Loaded)
				return;

			int posx, posy;
			Point mp = glProgramView.PointToClient(Cursor.Position);
			glProgramView.GetPointInProgram(mp.X, mp.Y, out posx, out posy);
			bool inControl = posx >= 0 && posy >= 0;

			if (inControl)
			{
				toolStripLabelPosition.Text = string.Format("Position: ({0:000}|{1:000})", posx, posy);
				toolStripLabelValue.Text = string.Format("Value: {0:0000}", prog.Raster[posx, posy]);
			}
			else
			{
				toolStripLabelPosition.Text = string.Format("Position: ({0}|{1})", "???", "???");
				toolStripLabelValue.Text = string.Format("Value: {0}", "????");
			}
			toolStripLabelSize.Text = string.Format("Size: {0}x{1}", prog.Width, prog.Height);

			if (RunOptions.SYNTAX_HIGHLIGHTING == RunOptions.SH_EXTENDED && glProgramView.ExtendedSHGraph.isEffectiveSizeCalculated())
				toolStripLabelEffectiveSize.Text = string.Format("Effective Size: {0}x{1}", glProgramView.ExtendedSHGraph.EffectiveWidth, glProgramView.ExtendedSHGraph.EffectiveHeight);
			else
				toolStripLabelEffectiveSize.Text = string.Format("Effective Size: {0}x{1}", '?', '?');

			toolStripLabelZoom.Text = string.Format("Zoom: x{0:0.##}", glProgramView.Zoom.GetZoomFactor());
			toolStripLabelBreakpoints.Text = string.Format("Breakpoints: {0}", prog.GetBreakPointCount());
			toolStripLabelWatchedFields.Text = string.Format("Watched Fields: {0}", prog.WatchedFields.Count);
			toolStripLabelSpeed.Text = string.Format("Speed level: {0}", GLExtendedViewControl.GetFreqFormatted(RunOptions.GetRunFrequency()));
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
			Reset();
		}

		private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Reload();
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog fd = new OpenFileDialog
			{
				Filter = "Befunge-Program|*.b93;*.b98;*.tfd|Befunge-93|*.b93|Befunge-98|*.b98|TextFunge-Debug-File|*.tfd|All Files|*",
				FilterIndex = 1
			};
			
			if (fd.ShowDialog() == DialogResult.OK)
			{
				var c = BefungeFileHelper.LoadTextFile(fd.FileName, RunOptions.PREPROCESSOR);
				if (c != null)
				{
					glProgramView.Loaded = false;
					glStackView.Loaded = false;

					initCode = c;
					RunOptions.FILEPATH = fd.FileName;

					prog.Running = false;

					var arrprog = initCode.GetRaster();

					if (arrprog.GetLength(0) * arrprog.GetLength(1) > GLProgramViewControl.MAX_EXTENDEDSH_SIZE)
					{
						SetSyntaxHighlighting(RunOptions.SH_SIMPLE, true);
					}

					prog = new BefunProg(initCode);

					glProgramView.ResetProg(prog, null);
					prog.UndoLog.Enabled = RunOptions.ENABLEUNDO;
					glStackView.ReInit(prog);

					new Thread(prog.Run).Start();
					glProgramView.InitSyntaxHighlighting();

					Text = fd.FileName + " - " + Program.TITLE;

					glProgramView.Loaded = true;
					glStackView.Loaded = true;
				}
			}
		}

		private void aSCIIStackToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RunOptions.ASCII_STACK = aSCIIStackToolStripMenuItem.Checked;
		}

		private void followCursorToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			SetFollowMode(followCursorToolStripMenuItem.Checked);
		}

		private void zoomToInitialToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (RunOptions.FOLLOW_MODE) //NOT POSSIBLE WHILE FOLLOWING
				return;

			glProgramView.Zoom.PopToBase();

			glProgramView.Zoom.Push(RunOptions.INIT_ZOOM);
		}

		private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (RunOptions.FOLLOW_MODE) //NOT POSSIBLE WHILE FOLLOWING
				return;


			glProgramView.Zoom.Pop();
		}

		private void zoomCompleteOutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (RunOptions.FOLLOW_MODE) //NOT POSSIBLE WHILE FOLLOWING
				return;

			glProgramView.Zoom.PopToBase();
		}

		private void runToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (prog.Mode == BefunProg.MODE_RUN)
				prog.Paused = !prog.Paused;
		}

		private void stepToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (prog.Mode != BefunProg.MODE_RUN)
				prog.DoSingleStep = true;
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
			ResetBPs();
			ResetWatched();
		}

		private void showCompleteStackToolStripMenuItem_Click(object sender, EventArgs e)
		{
			StringBuilder s = new StringBuilder();

			lock (prog.Stack)
			{
				glStackView.CurrStack.AddRange(prog.Stack);
			}

			s.AppendLine("Stack<" + glStackView.CurrStack.Count + ">");

			s.AppendLine();
			s.AppendLine();

			foreach (long val in glStackView.CurrStack)
			{
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

			lock (prog.Output)
			{
				s = prog.Output.ToString();
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
					long chr = prog.Raster[x, y];

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
			if (!Loaded)
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

			glProgramView.InitSyntaxHighlighting();
		}

		private void createHDScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog {Filter = "PNG-Image|*.png"};

			if (sfd.ShowDialog() == DialogResult.OK)
			{
				string fn = sfd.FileName;

				if (!fn.ToLower().EndsWith(".png"))
					fn += ".png";

				Bitmap b = glProgramView.GrabFullResScreenshot();

				b.Save(fn, ImageFormat.Png);
			}
		}

		private void lowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetSpeed(RunOptions.STANDARDFREQ_1, true);
		}

		private void middleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetSpeed(RunOptions.STANDARDFREQ_2, true);
		}

		private void fastToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetSpeed(RunOptions.STANDARDFREQ_3, true);
		}

		private void veryFastToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetSpeed(RunOptions.STANDARDFREQ_4, true);
		}

		private void fullToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetSpeed(RunOptions.STANDARDFREQ_5, true);
		}

		private void speedFreqBar_ValueChanged(object sender, EventArgs e)
		{
			SetSpeed(speedFreqBar.Value, false);
		}

		#endregion

		#region Controls

		private void btnAddInput_Click(object sender, EventArgs e)
		{
			PerformBufferedInput();
		}

		private void edInput_KeyUp(object sender, KeyEventArgs e)
		{
			PerformBufferedInput();
		}

		private void PerformBufferedInput()
		{
			string s = edInput.Text;
			edInput.Text = "";

			foreach (char c in s)
			{
				prog.InputCharacters.Enqueue(c);
			}
		}

		#endregion

		private void showStackReversedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RunOptions.SHOW_STACK_REVERSED = showStackReversedToolStripMenuItem.Checked;
		}

		private void enableInputPreprocessorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RunOptions.PREPROCESSOR = enableInputPreprocessorToolStripMenuItem.Checked;
		}

		private void enableUndoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RunOptions.ENABLEUNDO = enableUndoToolStripMenuItem.Checked;

			prog.UndoLog.Enabled = RunOptions.ENABLEUNDO;
			undoToolStripMenuItem.Enabled = RunOptions.ENABLEUNDO;
		}

		private void undoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (prog.Mode == BefunProg.MODE_RUN)
				prog.DoSingleUndo = true;
		}

		private void copyExecutiondataToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(glProgramView.GetExecutionData());
		}
	}
}

//TODO Edit Code Dialog
//TODO Edit Stack Dialog (?)
//TODO Move PC (Change Direction) Dialog

//TODO Conditional Breakpoints