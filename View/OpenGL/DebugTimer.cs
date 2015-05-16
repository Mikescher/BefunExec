using System;

namespace BefunExec.View
{
	public class DebugTimer
	{
		private const int MAX_COUNTER = 20;

		public double Time { get; private set; }

		private long timeSum;
		private long startTime;
		private int count;

		public DebugTimer()
		{
			Time = 1;
			count = 0;
			timeSum = 0;
		}

		public void Start()
		{
			count++;
			startTime = Environment.TickCount;
		}

		public void Stop()
		{
			count++;
			timeSum += Environment.TickCount - startTime;

			if (count > MAX_COUNTER)
				calc();
		}

		private void calc()
		{
			Time = (timeSum * 1.0) / count;

			count = 0;
			timeSum = 0;
		}
	}
}