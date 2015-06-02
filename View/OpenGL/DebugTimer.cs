using System;

namespace BefunExec.View
{
	public class DebugTimer
	{
		private const int MAX_COUNTER = 10;

		public double Time { get; private set; }

		private long timeSum;
		private long startTime;
		private int count;
		private bool running = false;

		public DebugTimer()
		{
			Time = 1;
			count = 0;
			timeSum = 0;
		}

		public void Start()
		{
			if (!running)
			{
				startTime = Environment.TickCount;

				running = true;
			}
		}

		public void Stop()
		{
			if (running)
			{
				count++;
				timeSum += Environment.TickCount - startTime;

				if (count > MAX_COUNTER)
					calc();

				running = false;
			}
		}

		public void Reset()
		{
			running = false;
		}

		private void calc()
		{
			Time = (timeSum * 1.0) / count;

			count = 0;
			timeSum = 0;
		}
	}
}