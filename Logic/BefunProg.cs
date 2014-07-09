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
		private static int[,] randDelta = { { 1, 0 }, { 0, -1 }, { -1, 0 }, { 0, 1 } };

		public bool running;
		public bool doSingleStep = false;
		public bool paused;
		public int mode = 0;

		public FrequencyCounter freq = new FrequencyCounter();

		public long[,] raster;
		public long[,] decay_raster;
		public bool[,] breakpoints;

		public ulong StepCount = 0; // MAX_ULONG = 18.446.744.073.709.551.615

		public int Width { get { return raster.GetLength(0); } }
		public int Height { get { return raster.GetLength(1); } }

		public long this[int x, int y] { get { return raster[x, y]; } }

		public Vec2i PC = new Vec2i(0, 0);
		public Vec2i delta = new Vec2i(1, 0);
		public bool stringmode = false;

		public Stack<long> Stack = new Stack<long>();

		private Vec2i dimension;

		Random rnd = new Random();

		public const int MODE_RUN = 0;
		public const int MODE_IN_INT = 1;
		public const int MODE_IN_CHAR = 2;
		public const int MODE_MOVEANDRUN = 3;

		public ConcurrentQueue<char> InputCharacters = new ConcurrentQueue<char>();
		public ConcurrentQueue<Tuple<long, long, long>> RasterChanges = new ConcurrentQueue<Tuple<long, long, long>>(); // <x, y, char>

		public int curr_lvl_sleeptime;

		public bool reset_freeze_request = false;
		public bool reset_freeze_answer = false;

		public string err = null;

		public StringBuilder output = new StringBuilder();
		public int simpleOutputHash = 0;

		public BefunProg(long[,] iras)
		{
			raster = iras;
			decay_raster = new long[Width, Height];
			breakpoints = new bool[Width, Height];

			for (int x = 0; x < Width; x++)
				for (int y = 0; y < Height; y++)
				{
					decay_raster[x, y] = 0;
					breakpoints[x, y] = false;
				}

			dimension = new Vec2i(Width, Height);

			paused = RunOptions.INIT_PAUSED;

			curr_lvl_sleeptime = RunOptions.GetSleep(RunOptions.INIT_SPEED);
		}

		public void run()
		{
			int skipcount;

			running = true;

			long start = Environment.TickCount;
			int sleeptime;

			bool paused_cached;

			while (running)
			{
				paused_cached = paused;

				if ((paused_cached && !doSingleStep) || mode != MODE_RUN)
				{
					if (mode == MODE_IN_CHAR && InputCharacters.Count > 0)
					{
						char deqv;
						if (InputCharacters.TryDequeue(out deqv))
						{
							push(deqv);
							mode = MODE_MOVEANDRUN;
						}
					}
					else if (mode == MODE_MOVEANDRUN)
					{
						move();
						mode = MODE_RUN;
					}
					else
					{
						Thread.Sleep(curr_lvl_sleeptime);
						decay();

						testForFreeze();

						start = Environment.TickCount;
					}
					continue;
				}
				freq.Inc();

				if (mode == MODE_RUN)
				{
					calc();
					debug();

					if (mode == MODE_RUN && (!paused_cached || doSingleStep))
					{
						skipcount = 0;
						do
						{
							move();
							decay();
							conditionalbreak();
							debug();

							skipcount++;
							if (skipcount > Width * 2)
							{
								err = "Program entered infinite NOP-Loop";
								debug();
								break; // Even when no debug - no infinite loop in this thread
							}
						}
						while (RunOptions.SKIP_NOP && raster[PC.X, PC.Y] == ' ' && !stringmode && (!paused_cached || doSingleStep));


					}
				}

				doSingleStep = false;

				sleeptime = (int)Math.Max(0, curr_lvl_sleeptime - (Environment.TickCount - start));
				if (curr_lvl_sleeptime != 0)
				{
					Thread.Sleep(sleeptime);
				}

				testForFreeze();

				start = Environment.TickCount;
			}
		}

		private void testForFreeze()
		{
			while (reset_freeze_request)
			{
				reset_freeze_answer = true;
				Thread.Sleep(0);
			}
			reset_freeze_answer = false;
		}

		private void conditionalbreak()
		{
			paused = paused || breakpoints[PC.X, PC.Y];
		}

		private void debug()
		{
			if (err != null && RunOptions.DEBUGRUN)
			{
				Console.WriteLine();
				Console.WriteLine("Debug Break: " + err);

				paused = true;
				err = null;
			}
		}

		private long pop()
		{
			lock (Stack)
			{
				if (Stack.Count == 0)
				{
					err = "Trying to pop an empty stack";
					return 0;
				}
				else
					return Stack.Pop();
			}
		}

		private long peek()
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

		private bool popBool()
		{
			lock (Stack)
			{
				if (Stack.Count == 0)
				{
					err = "Trying to pop an empty stack"; // Yes, pop, not peek - no peek OP in Befunge
					return false;
				}
				else
					return (Stack.Pop() != 0);
			}
		}

		public void push(long a)
		{
			lock (Stack)
			{
				Stack.Push(a);
			}
		}

		public void Out(string c)
		{
			c = c.Replace("\0", "");

			Console.Out.Write(c);
			lock (output)
			{
				output.Append(c);
			}
			simpleOutputHash++;
		}

		public void push(bool a)
		{
			lock (Stack)
			{
				Stack.Push(a ? 1 : 0);
			}
		}

		private void calc()
		{
			long curr = raster[PC.X, PC.Y];

			if (stringmode && curr != '"')
			{
				push(curr);
			}
			else
			{
				long tmp, tmp2, tmp3;

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
						push(curr - '0');
						break;
					case '+':
						push(pop() + pop());
						break;
					case '-':
						tmp = pop();
						push(pop() - tmp);
						break;
					case '*':
						push(pop() * pop());
						break;
					case '/':
						tmp = pop();
						push(tmp != 0 ? (pop() / tmp) : 0);
						break;
					case '%':
						tmp = pop();
						push(tmp != 0 ? (pop() % tmp) : 0);
						break;
					case '!':
						push(!popBool());
						break;
					case '`':
						tmp = pop();
						push(pop() > tmp);
						break;
					case '>':
						delta.Set(1, 0);
						break;
					case '<':
						delta.Set(-1, 0);
						break;
					case '^':
						delta.Set(0, -1);
						break;
					case 'v':
						delta.Set(0, 1);
						break;
					case '?':
						tmp = rnd.Next(4);
						delta.Set(randDelta[tmp, 0], randDelta[tmp, 1]);
						break;
					case '_':
						tmp = popBool() ? 2 : 0;
						delta.Set(randDelta[tmp, 0], randDelta[tmp, 1]);
						break;
					case '|':
						tmp = popBool() ? 1 : 3;
						delta.Set(randDelta[tmp, 0], randDelta[tmp, 1]);
						break;
					case '"':
						stringmode = !stringmode;
						break;
					case ':':
						push(peek());
						break;
					case '\\':
						tmp = pop();
						tmp2 = pop();
						push(tmp);
						push(tmp2);
						break;
					case '$':
						pop();
						break;
					case '.':
						Out(pop().ToString());
						break;
					case ',':
						Out(((char)pop()).ToString());
						break;
					case '#':
						move();
						break;
					case 'g':
						tmp = pop();
						tmp2 = pop();
						if (tmp >= 0 && tmp2 >= 0 && tmp2 < Width && tmp < Height)
							push(raster[tmp2, tmp]);
						else
							push(0);
						break;
					case 'p':
						tmp = pop();
						tmp2 = pop();
						if (tmp >= 0 && tmp2 >= 0 && tmp2 < Width && tmp < Height)
						{
							tmp3 = pop();
							raster[tmp2, tmp] = tmp3;
							RasterChanges.Enqueue(Tuple.Create(tmp2, tmp, tmp3));
						}
						else
							pop();
						break;
					case '&':
						mode = MODE_IN_INT;
						break;
					case '~':
						mode = MODE_IN_CHAR;
						break;
					case '@':
						delta.Set(0, 0);
						break;
					default:
						err = String.Format("Unknown Operation at {0}|{1}: {2}({3})", PC.X, PC.Y, curr, (char)curr);
						// NOP
						break;
				}
			}

			if (!delta.isZero()) //Don't count when finished
				StepCount++;
		}

		public void move()
		{
			int pcx = (PC.X + delta.X + dimension.X) % dimension.X;
			int pcy = (PC.Y + delta.Y + dimension.Y) % dimension.Y;

			int bx = PC.X + delta.X;
			int by = PC.Y + delta.Y;

			PC.Set(pcx, pcy);

			if (bx != PC.X || by != PC.Y)
				err = "PC wrapped around ledge (" + bx + "|" + by + ") - (" + PC.X + "|" + PC.Y + ")";
		}

		private void decay()
		{
			if (!RunOptions.SHOW_DECAY)
			{
				for (int x = 0; x < Width; x++)
				{
					for (int y = 0; y < Height; y++)
					{
						decay_raster[x, y] = (PC.X == x && PC.Y == y) ? Environment.TickCount : 0;
					}
				}
			}
			else
			{
				long now = Environment.TickCount;

				if (PC.X >= 0 && PC.Y >= 0)
					decay_raster[PC.X, PC.Y] = Environment.TickCount;
			}
		}

		public void full_reset(string code)
		{
			raster = GetProg(code);
			PC = new Vec2i(0, 0);
			paused = true;
			doSingleStep = false;

			for (int x = 0; x < Width; x++)
				for (int y = 0; y < Height; y++)
				{
					decay_raster[x, y] = 0;
				}
			Stack.Clear();
			stringmode = false;
			delta = new Vec2i(1, 0);
			mode = MODE_RUN;
			running = true;
			dimension = new Vec2i(Width, Height);
			StepCount = 0;

			InputCharacters = new ConcurrentQueue<char>();
			RasterChanges = new ConcurrentQueue<Tuple<long, long, long>>();
			RasterChanges.Enqueue(Tuple.Create(-1L, -1L, -1L));

			output.Clear();
			simpleOutputHash++;
		}

		public static int GetProgWidth(string pg)
		{
			return pg.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Max(s => s.Length);
		}

		public static int GetProgHeight(string pg)
		{
			return pg.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Length;
		}

		public static long[,] GetProg(string pg)
		{
			int w, h;

			long[,] prog = new long[w = GetProgWidth(pg), h = GetProgHeight(pg)];

			string[] split = pg.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					prog[x, y] = (x < split[y].Length) ? split[y][x] : (int)' ';
				}
			}

			return prog;
		}
	}
}
