using BefunExec.View.OpenGL.OGLMath;

namespace BefunExec.Logic
{
	public static class RunOptions
	{
		public const int SH_NONE = 0;
		public const int SH_SIMPLE = 1;
		public const int SH_EXTENDED = 2;



		public static bool INIT_PAUSED = true;
		public static Rect2i INIT_ZOOM = null;

		public static int INIT_SPEED = 3;

		public static int SLEEP_TIME_5 = 0;			// Time (ms) per Cycle
		public static int SLEEP_TIME_4 = 1;			// Time (ms) per Cycle
		public static int SLEEP_TIME_3 = 50;		// Time (ms) per Cycle
		public static int SLEEP_TIME_2 = 150;		// Time (ms) per Cycle
		public static int SLEEP_TIME_1 = 500;		// Time (ms) per Cycle

		public static int DECAY_TIME = 500;			// Time until decay
		public static bool SHOW_DECAY = true;

		public static int SYNTAX_HIGHLIGHTING = SH_SIMPLE;
		public static bool ASCII_STACK = true;
		public static bool FOLLOW_MODE = false;

		public static string FILEPATH = null;

		public static bool SKIP_NOP = true;
		public static bool DEBUGRUN = false;

		public static int GetSleep(int idx)
		{
			switch (idx)
			{
				case 1:
					return SLEEP_TIME_1;
				case 2:
					return SLEEP_TIME_2;
				case 3:
					return SLEEP_TIME_3;
				case 4:
					return SLEEP_TIME_4;
				case 5:
					return SLEEP_TIME_5;
				default:
					return SLEEP_TIME_3;
			}
		}
	}
}
