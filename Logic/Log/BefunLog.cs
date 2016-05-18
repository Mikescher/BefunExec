
using System;
namespace BefunExec.Logic.Log
{
	public class BefunLog
	{
		private const int HISTORY_SIZE = 256;

		private readonly BefunLogAction[,] history = new BefunLogAction[6, HISTORY_SIZE];
		private int hposition = 0;

		private bool collecting = false;

		public bool Enabled = false;

		private bool internalEnabled = false;

		public int Size { get; private set; }

		public BefunLog()
		{
			Reset();
		}

		public void Reset()
		{
			Size = 0;
			hposition = 0;
			for (int i = 0; i < HISTORY_SIZE; i++)
			{
				for (int p = 0; p < 6; p++)
				{
					history[p, i] = null;
				}
			}
		}

		public void StartCollecting()
		{
			if (!internalEnabled)
				return;

			if (collecting)
				EndCollecting();

			history[0, hposition] = null;
			history[1, hposition] = null;
			history[2, hposition] = null;
			history[3, hposition] = null;
			history[4, hposition] = null;
			history[5, hposition] = null;

			collecting = true;
		}

		private void Collect(BefunLogAction a)
		{
			if (!internalEnabled)
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

		public void EndCollecting()
		{
			Update();

			if (!internalEnabled)
				return;

			hposition = (hposition + 1) % HISTORY_SIZE;
			Size = Math.Min(Size + 1, HISTORY_SIZE);

			collecting = false;
		}

		public void Update()
		{
			if (internalEnabled != Enabled)
			{
				Reset();
				internalEnabled = Enabled;
			}
		}

		public bool Reverse(BefunProg prog)
		{
			if (!internalEnabled)
				return false;

			hposition = ((hposition - 1) + HISTORY_SIZE) % HISTORY_SIZE;
			Size = Math.Max(Size - 1, 0);

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

		public void CollectChangeStringmode()
		{
			if (!internalEnabled)
				return;

			Collect(new BefunLogActionChangeStringmode());
		}

		public void CollectDeltaChange(int dx, int dy)
		{
			if (!internalEnabled)
				return;

			Collect(new BefunLogActionDeltaChange(dx, dy));
		}

		public void CollectGridChange(long x, long y, long v)
		{
			if (!internalEnabled)
				return;

			Collect(new BefunLogActionGridChange(x, y, v));
		}

		public void CollectPCMove(int x, int y)
		{
			if (!internalEnabled)
				return;

			Collect(new BefunLogActionPCMove(x, y));
		}

		public void CollectStackAdd()
		{
			if (!internalEnabled)
				return;

			Collect(new BefunLogActionStackAdd());
		}

		public void CollectStackRemove(long v)
		{
			if (!internalEnabled)
				return;

			Collect(new BefunLogActionStackRemove(v));
		}
	}
}
