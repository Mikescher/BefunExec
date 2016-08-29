using BefunExec.View.OpenGL.OGLMath;

namespace BefunExec.Logic
{
	public static class RunOptions
	{
		public const int SH_NONE = 0;
		public const int SH_SIMPLE = 1;
		public const int SH_EXTENDED = 2;

		public static bool INIT_PAUSED = true;
		public static Rect2I INIT_ZOOM = null;

		public static readonly double[] FREQUENCY_SLIDER = 
		{
			1,
			7.5,
			25,
			100,
			250,
			500,
			1 * 1000,
			5 * 1000,
			25 * 1000,
			50 * 1000,
			100 * 1000,
			250 * 1000,
			500 * 1000,
			1 * 1000 * 1000,
			3 * 1000 * 1000,
			float.PositiveInfinity,
		};

		public static int RUN_FREQUENCY_IDX = 6;

		public const int STANDARDFREQ_1 = 1;		// 7.5
		public const int STANDARDFREQ_2 = 2;		// 25
		public const int STANDARDFREQ_3 = 6;		// 1000
		public const int STANDARDFREQ_4 = 9;		// 50000;
		public const int STANDARDFREQ_5 = 15;		// +INF

		public static int DECAY_TIME = 500;			// Time until decay
		public static bool SHOW_DECAY = true;

		public static int SYNTAX_HIGHLIGHTING = SH_EXTENDED;
		public static bool ASCII_STACK = true;
		public static bool SHOW_STACK_REVERSED = false;
		public static bool PREPROCESSOR = false;
		public static bool FORCE_TEXTURE_RENDERING = false;
		public static bool FILL_VIEWPORT = false;

		public static bool FOLLOW_MODE = false;

		public static string FILEPATH = null;

		public static bool SKIP_NOP = true;

		public static bool ENABLEUNDO = false;
		public static bool DEBUGRUN = false;

		public static double GetRunFrequency()
		{
			return FREQUENCY_SLIDER[RUN_FREQUENCY_IDX];
		}
	}
}
