using BefunExec.Logic.Log;
using BefunExec.View;
using BefunExec.View.OpenGL.OGLMath;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BefunExec.Logic
{
	public class BefunProg
	{
		private static readonly int[,] RandDelta = { { 1, 0 }, { 0, -1 }, { -1, 0 }, { 0, 1 } };
		
		public const int MODE_RUN = 0;
		public const int MODE_IN_INT = 1;
		public const int MODE_IN_CHAR = 2;
		public const int MODE_MOVEANDRUN = 3;

		public bool Running;
		public bool DoSingleStep = false;
		public bool DoSingleUndo = false;
		public bool Paused;
		public int Mode = 0;

		public readonly FrequencyCounter Freq = new FrequencyCounter();
		public readonly DebugTimer LogicTimer = new DebugTimer();
		private readonly Random rnd = new Random();

		public long[,] Raster;
		private readonly Vec2i decayRasterLast = new Vec2i(-1, -1);
		public readonly long[,] DecayRaster;
		public readonly bool[,] Breakpoints;
		public int Breakpointcount = 0;

		public ConcurrentQueue<Vec2i> WatchDataChanges = new ConcurrentQueue<Vec2i>();
		public readonly byte[,] WatchData;
		public List<Vec2i> WatchedFields = new List<Vec2i>(); 

		public ulong StepCount = 0; // MAX_ULONG = 18.446.744.073.709.551.615

		private long startTime = -1;
		private long endTime = -1;

		public int Width => Raster.GetLength(0);
		public int Height => Raster.GetLength(1);

		public long this[int x, int y] => Raster[x, y];

		public Vec2i PC = new Vec2i(0, 0);
		public Vec2i Delta = new Vec2i(1, 0);
		public bool Stringmode = false;

		public readonly BefunLog UndoLog = new BefunLog();
		public readonly Stack<long> Stack = new Stack<long>();

		private Vec2i dimension;
		
		public ConcurrentQueue<char> InputCharacters = new ConcurrentQueue<char>();
		public ConcurrentQueue<Tuple<long, long, long>> RasterChanges = new ConcurrentQueue<Tuple<long, long, long>>(); // <x, y, char>

		private double _currSleeptimeFreq;
		public double CurrSleeptimeFreq
		{
			get { return _currSleeptimeFreq; }
			set { _currSleeptimeFreq = value; actualCurrentSleepTime = (value == float.PositiveInfinity) ? (0.0) : (1000.0 / value); }
		}

		private double sleepTimeAccu = 0;
		private double actualCurrentSleepTime = 0;

		public bool ResetFreezeRequest = false;
		public bool ResetFreezeAnswer = false;

		private string err = null;

		public readonly StringBuilder Output = new StringBuilder();
		public int SimpleOutputHash = 0;

		public BefunProg(long[,] iras)
		{
			Raster = iras;
			DecayRaster = new long[Width, Height];
			Breakpoints = new bool[Width, Height];
			WatchData = new byte[Width, Height];

			for (int x = 0; x < Width; x++)
				for (int y = 0; y < Height; y++)
				{
					DecayRaster[x, y] = 0;
					Breakpoints[x, y] = false;
				}
			Breakpointcount = 0;

			dimension = new Vec2i(Width, Height);

			Paused = RunOptions.INIT_PAUSED;

			CurrSleeptimeFreq = RunOptions.getRunFrequency();
		}

		public void Run()
		{
			Running = true;

			while (Running)
			{
				LogicTimer.Reset();
				LogicTimer.Start();

				var pausedCached = Paused;

				UndoLog.update();

				if (Paused && DoSingleUndo && Mode == MODE_RUN)
				{
					UndoLog.Reverse(this);
					DoSingleUndo = false;
				}

				if (WatchDataChanges.Count > 0)
				{
					Vec2i wdc;
					if (WatchDataChanges.TryDequeue(out wdc))
					{
						if (wdc.X >= 0 && wdc.X < Width && wdc.Y >= 0 && wdc.Y < Height)
						{
							byte newValue = (byte) ((WatchData[wdc.X, wdc.Y] + 1)%7);
							WatchData[wdc.X, wdc.Y] = newValue;
							if (newValue == 1)
							{
								WatchedFields.Add(new Vec2i(wdc));
								WatchedFields = new List<Vec2i>(WatchedFields);
							}
							if (newValue == 0)
							{
								WatchedFields.RemoveAll(p => p == wdc);
								WatchedFields = new List<Vec2i>(WatchedFields);
							}
						}
					}
				}

				if ((pausedCached && !DoSingleStep) || Mode != MODE_RUN)
				{
					if (Mode == MODE_IN_CHAR && InputCharacters.Count > 0)
					{
						char deqv;
						if (InputCharacters.TryDequeue(out deqv))
						{
							Push(deqv);
							Mode = MODE_MOVEANDRUN;
						}
					}
					else if (Mode == MODE_MOVEANDRUN)
					{
						UndoLog.startCollecting();
						Move(true);
						UndoLog.endCollecting();
						Mode = MODE_RUN;
					}
					else
					{
						Sleep();
						Decay();

						TestForFreeze();
					}
					continue;
				}
				Freq.Inc();

				if (Mode == MODE_RUN)
				{
					if (startTime < 0)
						startTime = Environment.TickCount;

					UndoLog.startCollecting();
					Calc();
					Debug();

					if (Mode == MODE_RUN && (!pausedCached || DoSingleStep))
					{
						Move(true);
						Decay();
						ConditionalBreak();
						Debug();
					}

					UndoLog.endCollecting();

					if (Mode == MODE_RUN && (!pausedCached || DoSingleStep))
					{
						var skipcount = 0;
						while (RunOptions.SKIP_NOP && Raster[PC.X, PC.Y] == ' ' && !Stringmode && (!pausedCached || DoSingleStep))
						{
							Move(false);
							Decay();
							ConditionalBreak();
							Debug();

							skipcount++;
							if (skipcount > Math.Max(Width, Height) * 2)
							{
								err = "Program entered infinite NOP-Loop";
								Debug();
								break; // Even when no debug - no infinite loop in this thread
							}
						}
					}
				}

				DoSingleStep = false;

				LogicTimer.Stop();

				Sleep();

				TestForFreeze();
			}
		}

		private void Sleep()
		{
			if (actualCurrentSleepTime != 0)
			{
				sleepTimeAccu += actualCurrentSleepTime;

				if (sleepTimeAccu >= 1)
				{
					Thread.Sleep((int)sleepTimeAccu);
					sleepTimeAccu -= (int)sleepTimeAccu;
				}
			}
		}

		private void TestForFreeze()
		{
			while (ResetFreezeRequest)
			{
				ResetFreezeAnswer = true;
				Thread.Sleep(0);
			}
			ResetFreezeAnswer = false;
		}

		private void ConditionalBreak()
		{
			Paused = Paused || Breakpoints[PC.X, PC.Y];
		}

		private void Debug()
		{
			if (err != null && RunOptions.DEBUGRUN)
			{
				Console.WriteLine();
				Console.WriteLine("Debug Break: " + err);

				Paused = true;
				err = null;
			}
		}

		public long Pop(bool log = true)
		{
			lock (Stack)
			{
				if (Stack.Count == 0)
				{
					err = "Trying to pop an empty stack";
					return 0;
				}
				else
				{
					if (log)
						UndoLog.collectStackRemove(Stack.Peek());
					return Stack.Pop();
				}
			}
		}

		private long Peek()
		{
			lock (Stack)
			{
				if (Stack.Count == 0)
				{
					err = "Trying to pop an empty stack"; // Yes, pop, not peek - no peek OP in Befunge

					Stack.Push(0);
				}

				return Stack.Peek();
			}
		}

		private bool PopBool(bool log = true)
		{
			lock (Stack)
			{
				if (Stack.Count == 0)
				{
					err = "Trying to pop an empty stack"; // Yes, pop, not peek - no peek OP in Befunge
					return false;
				}
				else
				{
					if (log)
						UndoLog.collectStackRemove(Stack.Peek());
					return (Stack.Pop() != 0);
				}
			}


		}

		public void Push(long a, bool log = true)
		{
			lock (Stack)
			{
				Stack.Push(a);
			}

			if (log)
				UndoLog.collectStackAdd();
		}

		private void Out(string c)
		{
			c = c.Replace("\0", "");

			Console.Out.Write(c);
			lock (Output)
			{
				Output.Append(c);
			}
			SimpleOutputHash++;
		}

		private void Push(bool a, bool log = true)
		{
			lock (Stack)
			{
				Stack.Push(a ? 1 : 0);
			}

			if (log)
				UndoLog.collectStackAdd();
		}

		public long GetExecutedTime()
		{
			return (startTime < 0) ? (0) : ((endTime < 0) ? (Environment.TickCount - startTime) : (endTime - startTime));
		}

		private void Calc()
		{
			long curr = Raster[PC.X, PC.Y];

			if (Stringmode && curr != '"')
			{
				Push(curr);
			}
			else
			{
				long tmp, tmp2;

				switch (curr)
				{
					case ' ':
						break; // NOP
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						Push(curr - '0');
						break;
					case '+':
						Push(Pop() + Pop());
						break;
					case '-':
						tmp = Pop();
						Push(Pop() - tmp);
						break;
					case '*':
						Push(Pop() * Pop());
						break;
					case '/':
						tmp = Pop();
						tmp2 = Pop();
						Push(tmp != 0 ? (tmp2 / tmp) : 0);
						break;
					case '%':
						tmp = Pop();
						tmp2 = Pop();
						Push(tmp != 0 ? (tmp2 % tmp) : 0);
						break;
					case '!':
						Push(!PopBool());
						break;
					case '`':
						tmp = Pop();
						Push(Pop() > tmp);
						break;
					case '>':
						ChangeDelta(1, 0);
						break;
					case '<':
						ChangeDelta(-1, 0);
						break;
					case '^':
						ChangeDelta(0, -1);
						break;
					case 'v':
						ChangeDelta(0, 1);
						break;
					case '?':
						tmp = rnd.Next(4);
						ChangeDelta(RandDelta[tmp, 0], RandDelta[tmp, 1]);
						break;
					case '_':
						tmp = PopBool() ? 2 : 0;
						ChangeDelta(RandDelta[tmp, 0], RandDelta[tmp, 1]);
						break;
					case '|':
						tmp = PopBool() ? 1 : 3;
						ChangeDelta(RandDelta[tmp, 0], RandDelta[tmp, 1]);
						break;
					case '"':
						Stringmode = !Stringmode;
						UndoLog.collectChangeStringmode();
						break;
					case ':':
						Push(Peek());
						break;
					case '\\':
						tmp = Pop();
						tmp2 = Pop();
						Push(tmp);
						Push(tmp2);
						break;
					case '$':
						Pop();
						break;
					case '.':
						Out(Pop().ToString());
						break;
					case ',':
						Out(((char)Pop()).ToString());
						break;
					case '#':
						Move(true);
						break;
					case 'g':
						tmp = Pop();
						tmp2 = Pop();
						if (tmp >= 0 && tmp2 >= 0 && tmp2 < Width && tmp < Height)
							Push(Raster[tmp2, tmp]);
						else
							Push(0);
						break;
					case 'p':
						tmp = Pop();
						tmp2 = Pop();
						if (tmp >= 0 && tmp2 >= 0 && tmp2 < Width && tmp < Height)
						{
							var tmp3 = Pop();
							ChangeRaster(tmp2, tmp, tmp3);
						}
						else
							Pop();
						break;
					case '&':
						Mode = MODE_IN_INT;
						break;
					case '~':
						Mode = MODE_IN_CHAR;
						break;
					case '@':
						Delta.Set(0, 0);
						if (endTime < 0)
							endTime = Environment.TickCount;
						break;
					default:
						err = String.Format("Unknown Operation at {0}|{1}: {2}({3})", PC.X, PC.Y, curr, (char)curr);
						// NOP
						break;
				}
			}

			if (!Delta.isZero()) //Don't count when finished
				StepCount++;
		}

		private void ChangeDelta(int dx, int dy)
		{
			UndoLog.collectDeltaChange(Delta.X, Delta.Y);

			Delta.Set(dx, dy);
		}

		public void ChangeRaster(long posX, long posY, long v, bool log = true)
		{
			long old = Raster[posX, posY];
			Raster[posX, posY] = v;
			RasterChanges.Enqueue(Tuple.Create(posX, posY, v));

			if (log)
				UndoLog.collectGridChange(posX, posY, old);
		}

		private void Move(bool collect)
		{
			if (collect) UndoLog.collectPCMove(PC.X, PC.Y);

			int pcx = (PC.X + Delta.X + dimension.X) % dimension.X;
			int pcy = (PC.Y + Delta.Y + dimension.Y) % dimension.Y;

			int bx = PC.X + Delta.X;
			int by = PC.Y + Delta.Y;

			PC.Set(pcx, pcy);

			if (bx != PC.X || by != PC.Y)
				err = "PC wrapped around ledge (" + bx + "|" + by + ") - (" + PC.X + "|" + PC.Y + ")";
		}

		private void Decay()
		{
			if (!RunOptions.SHOW_DECAY)
			{
				if (decayRasterLast.X >= 0 && decayRasterLast.Y >= 0)
					DecayRaster[decayRasterLast.X, decayRasterLast.Y] = 0;

				if (PC.X >= 0 && PC.Y >= 0)
				{
					DecayRaster[PC.X, PC.Y] = Environment.TickCount;
					decayRasterLast.Set(PC.X, PC.Y);
				}
			}
			else
			{
				long now = Environment.TickCount;

				if (PC.X >= 0 && PC.Y >= 0)
					DecayRaster[PC.X, PC.Y] = now;
			}
		}

		public void full_reset(string code)
		{
			Raster = GetProg(code);
			PC = new Vec2i(0, 0);
			Paused = true;
			DoSingleStep = false;

			for (int x = 0; x < Width; x++)
				for (int y = 0; y < Height; y++)
				{
					DecayRaster[x, y] = 0;
				}

			UndoLog.reset();
			Stack.Clear();
			Stringmode = false;
			Delta = new Vec2i(1, 0);
			Mode = MODE_RUN;
			Running = true;
			dimension = new Vec2i(Width, Height);
			StepCount = 0;
			startTime = -1;
			endTime = -1;

			InputCharacters = new ConcurrentQueue<char>();
			RasterChanges = new ConcurrentQueue<Tuple<long, long, long>>();
			RasterChanges.Enqueue(Tuple.Create(-1L, -1L, -1L));

			Output.Clear();
			SimpleOutputHash++;
		}

		public static int GetProgWidth(string pg)
		{
			return pg.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Max(s => s.Length);
		}

		public static int GetProgHeight(string pg)
		{
			return pg.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Length;
		}

		public static long[,] GetProg(string pg)
		{
			int w, h;

			long[,] prog = new long[w = GetProgWidth(pg), h = GetProgHeight(pg)];

			string[] split = pg.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					prog[x, y] = (x < split[y].Length) ? split[y][x] : ' ';
				}
			}

			return prog;
		}

		public bool IsBefunge93()
		{
			return Width <= 80 && Height <= 25;
		}

		public int GetBreakPointCount()
		{
			return Breakpointcount;
		}

		public double GetActualSleepTime()
		{
			return actualCurrentSleepTime;
		}
	}
}
