using BefunExec.Logic;
using BefunExec.View;
using BefunExec.View.OpenGL.OGLMath;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace BefunExec
{
	class Program
	{
		public static string TITLE = "BefunExec";
		private static string demo = Properties.Resources.demoProg;

		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();

			//args.ToList().ForEach(Console.WriteLine);

			FileInformation code;

			ParseParams(args, out code);

			Console.WriteLine();
			Console.WriteLine();

			//                 00000000011111111112222222222333333333344444444445555555555666666666677777777778
			//                 12345678901234567890123456789012345678901234567890123456789012345678901234567890

			Console.WriteLine("########## KEYS ##########");
			Console.WriteLine();
			Console.WriteLine("Space:          Pause | Resume");
			Console.WriteLine("Right:          Step Forward");
			Console.WriteLine("Left:           Undo last Step (needs undo enabled)");

			Console.WriteLine("Mouse (Click):  Breakpoint");
			Console.WriteLine("Mouse (Drag):   Zoom in");
			Console.WriteLine("MouseWheel:     Zoom In/Out");
			Console.WriteLine("Mouse (Middle): Add field to watch list");
			Console.WriteLine("Mouse (Middle): Toggle watch field type");
			Console.WriteLine("Esc:            Zoom out");

			Console.WriteLine("C:              Remove all breakpoints");
			Console.WriteLine("R:              Reset program");
			Console.WriteLine("Strg+R:         Reload file");
			Console.WriteLine("F:              Follow Cursor Mode");
			Console.WriteLine("P:              Zoom in on program code");
			Console.WriteLine("V:              Toggle 'Fill viewport' option");

			Console.WriteLine("1:              Debug speed");
			Console.WriteLine("2:              Normal speed");
			Console.WriteLine("3:              High speed");
			Console.WriteLine("4:              Very High speed");
			Console.WriteLine("5:              Full speed");
			Console.WriteLine("Pg-Up:          Increase Speed");
			Console.WriteLine("Pg-Down:        Decrease Speed");
			Console.WriteLine();			   
			Console.WriteLine("TAB:            Debug View  (TRY IT OUT !)");

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();

			Console.WriteLine("########## OUTPUT ##########");

			//###########

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();

			BefunProg bp = new BefunProg(code);
			new Thread(bp.Run).Start();

			//MainView mv = new MainView(bp, code);
			Application.Run(new MainForm(bp, code));
		}

		private static CommandLineArguments ParseParams(string[] args, out FileInformation code)
		{
			if (args.Length > 0 && File.Exists(args[0]))
			{
				args[0] = "-file=" + args[0];
			}

			CommandLineArguments cmda = new CommandLineArguments(args);

			if (cmda.isEmpty())
			{
				//                 00000000011111111112222222222333333333344444444445555555555666666666677777777778
				//                 12345678901234567890123456789012345678901234567890123456789012345678901234567890
				Console.WriteLine("########## Parameter ##########");
				Console.WriteLine();
				Console.WriteLine("pause | no_pause           : Start Interpreter paused");
				Console.WriteLine("asciistack | no_asciistack : Enable char display in stack");
				Console.WriteLine("skipnop | no_skipnop       : Skip NOP's");
				Console.WriteLine("debug | no_debug           : Activates additional debug-messages");
				Console.WriteLine("                             and the input-preprocessor");
				Console.WriteLine("preprocess | no_preprocess : Explicitly activate/deactivate the proprocessor");
				Console.WriteLine("follow | no_follow         : Activates the follow-cursor mode");
				Console.WriteLine("full_vp | limited_vp       : Fill the whole viewport in zoomed mode");

				Console.WriteLine("p_highlight | no_highlight : Set Syntax-Highlighting to [none]");
				Console.WriteLine("s_highlight | highlight    : Set Syntax-Highlighting to [simple]");
				Console.WriteLine("e_highlight                : Set Syntax-Highlighting to [extended]");

				Console.WriteLine("undo | no_undo             : Enables the undo log");
				Console.WriteLine("revstack | normstack       : Show the stack reversed");
				Console.WriteLine("forcetexturerenderer       : Always render the full textures");
				Console.WriteLine("                             (warning will (!) result in slow performance)");

				Console.WriteLine("speed=?                    : Set the initial speed (index 0..15)");

				Console.WriteLine("decay=?                    : Time (ms) for the decay effect");
				Console.WriteLine("dodecay | no_decay         : Show decay trail");
				Console.WriteLine("zoom=?;?;?;?               : Initial zoom position (x1, y1, x2, y2)");
				Console.WriteLine("file=?                     : The file to execute");
				Console.WriteLine();
			}

			//############################

			if (cmda.IsSet("no_pause"))
				RunOptions.INIT_PAUSED = false;
			if (cmda.IsSet("pause"))
				RunOptions.INIT_PAUSED = true;

			//##############

			bool customHighlight = cmda.IsSet("p_highlight") || cmda.IsSet("no_highlight") || cmda.IsSet("highlight") || cmda.IsSet("s_highlight") || cmda.IsSet("e_highlight");

			if (cmda.IsSet("p_highlight") | cmda.IsSet("no_highlight"))
				RunOptions.SYNTAX_HIGHLIGHTING = RunOptions.SH_NONE;
			if (cmda.IsSet("s_highlight") | cmda.IsSet("highlight"))
				RunOptions.SYNTAX_HIGHLIGHTING = RunOptions.SH_SIMPLE;
			if (cmda.IsSet("e_highlight"))
				RunOptions.SYNTAX_HIGHLIGHTING = RunOptions.SH_EXTENDED;

			//##############

			if (cmda.IsSet("asciistack"))
				RunOptions.ASCII_STACK = true;
			if (cmda.IsSet("no_asciistack"))
				RunOptions.ASCII_STACK = false;

			//##############

			if (cmda.IsSet("undo"))
				RunOptions.ENABLEUNDO = true;
			if (cmda.IsSet("no_undo"))
				RunOptions.ENABLEUNDO = false;

			if (cmda.IsSet("revstack"))
				RunOptions.SHOW_STACK_REVERSED = true;
			if (cmda.IsSet("normstack"))
				RunOptions.SHOW_STACK_REVERSED = false;

			if (cmda.IsSet("forcetexturerenderer"))
				RunOptions.FORCE_TEXTURE_RENDERING = true;
			if (cmda.IsSet("forceperfrenderer"))
				RunOptions.FORCE_TEXTURE_RENDERING = false;

			//##############

			if (cmda.IsSet("no_skipnop") || cmda.IsSet("executenop"))
				RunOptions.SKIP_NOP = false;
			if (cmda.IsSet("skipnop"))
				RunOptions.SKIP_NOP = true;

			//##############

			if (cmda.IsSet("no_debug") || cmda.IsSet("no_debugrun"))
				RunOptions.DEBUGRUN = false;
			if (cmda.IsSet("debug") || cmda.IsSet("debugrun"))
			{
				RunOptions.PREPROCESSOR = true;
				RunOptions.DEBUGRUN = true;
			}

			//##############

			if (cmda.IsSet("no_follow") || cmda.IsSet("no_followcursor"))
				RunOptions.FOLLOW_MODE = false;
			if (cmda.IsSet("follow") || cmda.IsSet("followcursor"))
				RunOptions.FOLLOW_MODE = true;

			//##############

			if (cmda.IsSet("no_preprocess") || cmda.IsSet("no_preprocessor"))
				RunOptions.PREPROCESSOR = false;
			if (cmda.IsSet("preprocess") || cmda.IsSet("preprocessor"))
				RunOptions.PREPROCESSOR = true;

			//##############

			if (cmda.IsSet("full_vp") || cmda.IsSet("fullvp") || cmda.IsSet("fill_vp") || cmda.IsSet("fillvp"))
				RunOptions.FILL_VIEWPORT = true;
			if (cmda.IsSet("limited_vp") || cmda.IsSet("limitedvp"))
				RunOptions.FILL_VIEWPORT = false;

			//##############

			if (cmda.IsSet("no_decay"))
				RunOptions.SHOW_DECAY = false;
			if (cmda.IsSet("dodecay"))
				RunOptions.SHOW_DECAY = true;

			//##############

			if (cmda.IsUInt("speed"))
				RunOptions.RUN_FREQUENCY_IDX = (int)(uint.Parse(cmda["speed"]) % 16);

			//##############

			if (cmda.IsInt("decay"))
				RunOptions.DECAY_TIME = int.Parse(cmda["decay"]);

			//##############

			if (cmda.IsSet("zoom"))
			{
				int tmp;
				string[] zooms = cmda["zoom"].Split(','); // zoom=X1,Y1,X2,Y2
				if (zooms.Length == 4 && zooms.All(p => int.TryParse(p, out tmp)))
				{
					RunOptions.INIT_ZOOM = new Rect2I(
						int.Parse(zooms[0]),
						int.Parse(zooms[1]),
						int.Parse(zooms[2]) - int.Parse(zooms[0]),
						int.Parse(zooms[3]) - int.Parse(zooms[1]));
				}
			}

			//##############
			
			if (!cmda.IsSet("file") || (code = BefungeFileHelper.LoadTextFile(cmda["file"].Trim('"'), RunOptions.PREPROCESSOR)) == null)
			{
				Console.WriteLine("########## FILE NOT FOUND ##########");

				Console.WriteLine("Please pass a BefungeFile with the parameter '-file'");

				Console.WriteLine("Using Demo ...");

				code = new FileInformation{ Code = demo };
			}
			else // succ loaded
			{
				RunOptions.FILEPATH = Path.GetFullPath(cmda["file"].Trim('"'));
			}

			if (!customHighlight && code.GetProgWidth() * code.GetProgHeight() > GLProgramViewControl.MAX_EXTENDEDSH_SIZE)
			{
				RunOptions.SYNTAX_HIGHLIGHTING = RunOptions.SH_SIMPLE;
			}

			//Console.WriteLine();
			//Console.WriteLine("Actual arguments:");
			//Array.ForEach(args, p => Console.WriteLine(p));
			Console.WriteLine();
			return cmda;
		}
	}
}