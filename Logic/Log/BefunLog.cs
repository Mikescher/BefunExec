
using System;
namespace BefunExec.Logic.Log
{
	public class BefunLog
	{
		public const int HISTORY_SIZE = 256;

		private BefunLogAction[,] history = new BefunLogAction[6, HISTORY_SIZE];
		private int hposition = 0;

		private bool collecting = false;

		public bool enabled = false;

		private bool internal_enabled = false;

		public int size { get; private set; }

		public BefunLog()
		{
			reset();
		}

		public void reset()
		{
			size = 0;
			hposition = 0;
			for (int i = 0; i < HISTORY_SIZE; i++)
			{
				for (int p = 0; p < 6; p++)
				{
					history[p, i] = null;
				}
			}
		}

		public void startCollecting()
		{
			if (!internal_enabled)
				return;

			if (collecting)
				endCollecting();

			history[0, hposition] = null;
			history[1, hposition] = null;
			history[2, hposition] = null;
			history[3, hposition] = null;
			history[4, hposition] = null;
			history[5, hposition] = null;

			collecting = true;
		}

		private void collect(BefunLogAction a)
		{
			if (!internal_enabled)
				return;

			if (!collecting)
				throw new Exception("Trying to log while not collecting - u wot m8");

			for (int i = 0; ; i++)
			{
				if (history[i, hposition] == null)
				{
					history[i, hposition] = a;
					return;
				}
			}
		}

		public void endCollecting()
		{
			update();

			if (!internal_enabled)
				return;

			hposition = (hposition + 1) % HISTORY_SIZE;
			size = Math.Min(size + 1, HISTORY_SIZE);

			collecting = false;
		}

		public void update()
		{
			if (internal_enabled != enabled)
			{
				reset();
				internal_enabled = enabled;
			}
		}

		public bool Reverse(BefunProg prog)
		{
			if (!internal_enabled)
				return false;

			hposition = ((hposition - 1) + HISTORY_SIZE) % HISTORY_SIZE;
			size = Math.Max(size - 1, 0);

			if (history[5, hposition] != null)
			{
				history[5, hposition].Reverse(prog);
				history[4, hposition].Reverse(prog);
				history[3, hposition].Reverse(prog);
				history[2, hposition].Reverse(prog);
				history[1, hposition].Reverse(prog);
				history[0, hposition].Reverse(prog);
			}
			else if (history[4, hposition] != null)
			{
				history[4, hposition].Reverse(prog);
				history[3, hposition].Reverse(prog);
				history[2, hposition].Reverse(prog);
				history[1, hposition].Reverse(prog);
				history[0, hposition].Reverse(prog);
			}
			else if (history[3, hposition] != null)
			{
				history[3, hposition].Reverse(prog);
				history[2, hposition].Reverse(prog);
				history[1, hposition].Reverse(prog);
				history[0, hposition].Reverse(prog);
			}
			else if (history[2, hposition] != null)
			{
				history[2, hposition].Reverse(prog);
				history[1, hposition].Reverse(prog);
				history[0, hposition].Reverse(prog);
			}
			else if (history[1, hposition] != null)
			{
				history[1, hposition].Reverse(prog);
				history[0, hposition].Reverse(prog);
			}
			else if (history[0, hposition] != null)
			{
				history[0, hposition].Reverse(prog);
			}
			else
			{
				return false;
			}

			history[0, hposition] = null;
			history[1, hposition] = null;
			history[2, hposition] = null;
			history[3, hposition] = null;
			history[4, hposition] = null;
			history[5, hposition] = null;

			return true;
		}

		public void collectChangeStringmode()
		{
			if (!internal_enabled)
				return;

			collect(new BefunLogActionChangeStringmode());
		}

		public void collectDeltaChange(int dx, int dy)
		{
			if (!internal_enabled)
				return;

			collect(new BefunLogActionDeltaChange(dx, dy));
		}

		public void collectGridChange(long x, long y, long v)
		{
			if (!internal_enabled)
				return;

			collect(new BefunLogActionGridChange(x, y, v));
		}

		public void collectPCMove(int x, int y)
		{
			if (!internal_enabled)
				return;

			collect(new BefunLogActionPCMove(x, y));
		}

		public void collectStackAdd()
		{
			if (!internal_enabled)
				return;

			collect(new BefunLogActionStackAdd());
		}

		public void collectStackRemove(long v)
		{
			if (!internal_enabled)
				return;

			collect(new BefunLogActionStackRemove(v));
		}
	}
}
