using BefungExec.Logic;
using BefungExec.View;
using BefungExec.View.OpenGL.OGLMath;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace BefungExec
{
	class Program
	{
		private static string demo = Properties.Resources.demoProg;

		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();

			//args.ToList().ForEach(Console.WriteLine);

			string code;

			parseParams(args, out code);

			Console.WriteLine();
			Console.WriteLine();

			Console.WriteLine("########## KEYS ##########");
			Console.WriteLine();
			Console.WriteLine("Space:         Pause | Resume");
			Console.WriteLine("Right:         Step Forward");

			Console.WriteLine("Mouse:         Zoom in | Breakpoint");
			Console.WriteLine("MouseWheel:    Zoom In/Out");
			Console.WriteLine("Esc:           Zoom out | Exit");

			Console.WriteLine("C:             Remove all breakpoints");
			Console.WriteLine("R:             Reset");
			Console.WriteLine("F:             Follow Cursor Mode");

			Console.WriteLine("1:             Debug speed");
			Console.WriteLine("2:             Normal speed");
			Console.WriteLine("3:             High speed");
			Console.WriteLine("4:             Very High speed");
			Console.WriteLine("5:             Full speed");


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

			BefunProg bp = new BefunProg(BefunProg.GetProg(code));
			new Thread(new ThreadStart(bp.run)).Start();

			//MainView mv = new MainView(bp, code);
			Application.Run(new MainForm(bp, code));
		}

		private static CommandLineArguments parseParams(string[] args, out string code)
		{
			if (args.Length > 0 && File.Exists(args[0]))
			{
				args[0] = "-file=" + args[0];
			}

			CommandLineArguments cmda = new CommandLineArguments(args);

			if (cmda.isEmpty())
			{
				Console.WriteLine("########## Parameter ##########");
				Console.WriteLine();
				Console.WriteLine("pause | no_pause           : Start Interpreter paused");
				Console.WriteLine("asciistack | no_asciistack : Enable char display in stack");
				Console.WriteLine("skipnop | no_skipnop       : Skip NOP's");
				Console.WriteLine("debug | no_debug           : Activates additional debug-messages");
				Console.WriteLine("follow | no_follow         : Activates the follow-cursor mode");

				Console.WriteLine("no-highlight               : Set Syntax-Highlighting to [none]");
				Console.WriteLine("highlight | s-highlight    : Set Syntax-Highlighting to [simple]");
				Console.WriteLine("e-highlight                : Set Syntax-Highlighting to [extended]");

				Console.WriteLine("speed=?                    : Set the initial speed (index)");

				Console.WriteLine("speed_5=?                  : Set the speed (ms) for speed-5");
				Console.WriteLine("speed_4=?                  : Set the speed (ms) for speed-4");
				Console.WriteLine("speed_3=?                  : Set the speed (ms) for speed-3");
				Console.WriteLine("speed_2=?                  : Set the speed (ms) for speed-2");
				Console.WriteLine("speed_1=?                  : Set the speed (ms) for speed-1");

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

			if (cmda.IsSet("no_highlight"))
				RunOptions.SYNTAX_HIGHLIGHTING = RunOptions.SH_NONE;
			if (cmda.IsSet("highlight") || cmda.IsSet("s-highlight"))
				RunOptions.SYNTAX_HIGHLIGHTING = RunOptions.SH_SIMPLE;
			if (cmda.IsSet("e-highlight"))
				RunOptions.SYNTAX_HIGHLIGHTING = RunOptions.SH_EXTENDED;

			//##############

			if (cmda.IsSet("asciistack"))
				RunOptions.ASCII_STACK = true;
			if (cmda.IsSet("no_asciistack"))
				RunOptions.ASCII_STACK = false;

			//##############

			if (cmda.IsSet("no_skipnop") || cmda.IsSet("executenop"))
				RunOptions.SKIP_NOP = false;
			if (cmda.IsSet("skipnop"))
				RunOptions.SKIP_NOP = true;

			//##############

			if (cmda.IsSet("no_debug") || cmda.IsSet("no_debugrun"))
				RunOptions.DEBUGRUN = false;
			if (cmda.IsSet("debug") || cmda.IsSet("debugrun"))
				RunOptions.DEBUGRUN = true;

			//##############

			if (cmda.IsSet("no_followcursor") || cmda.IsSet("no_followcursor"))
				RunOptions.FOLLOW_MODE = false;
			if (cmda.IsSet("follow") || cmda.IsSet("followcursor"))
				RunOptions.FOLLOW_MODE = true;

			//##############

			if (cmda.IsSet("no_decay"))
				RunOptions.SHOW_DECAY = false;
			if (cmda.IsSet("dodecay"))
				RunOptions.SHOW_DECAY = true;

			//##############

			if (cmda.IsInt("speed"))
				RunOptions.INIT_SPEED = int.Parse(cmda["speed"]);

			//##############

			if (cmda.IsInt("speed_1"))
				RunOptions.SLEEP_TIME_1 = int.Parse(cmda["speed_1"]);
			if (cmda.IsInt("speed_2"))
				RunOptions.SLEEP_TIME_2 = int.Parse(cmda["speed_2"]);
			if (cmda.IsInt("speed_3"))
				RunOptions.SLEEP_TIME_3 = int.Parse(cmda["speed_3"]);
			if (cmda.IsInt("speed_4"))
				RunOptions.SLEEP_TIME_4 = int.Parse(cmda["speed_4"]);
			if (cmda.IsInt("speed_5"))
				RunOptions.SLEEP_TIME_5 = int.Parse(cmda["speed_5"]);

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
					RunOptions.INIT_ZOOM = new Rect2i(
						int.Parse(zooms[0]),
						int.Parse(zooms[1]),
						int.Parse(zooms[2]) - int.Parse(zooms[0]),
						int.Parse(zooms[3]) - int.Parse(zooms[1]));
				}
			}

			//##############

			if (!cmda.IsSet("file") || (code = BefungeFileHelper.LoadTextFile(cmda["file"].Trim('"'))) == null)
			{
				Console.WriteLine("########## FILE NOT FOUND ##########");

				Console.WriteLine("Please pass a BefungeFile with the parameter '-file'");

				Console.WriteLine("Using Demo ...");

				code = demo;
				//RunOptions.DEBUGRUN = false; // Please do not debug demo :/
			}

			//Console.WriteLine();
			//Console.WriteLine("Actual arguments:");
			//Array.ForEach(args, p => Console.WriteLine(p));
			Console.WriteLine();
			return cmda;
		}
	}
}

//TODO Compile to BefunExec.exe ( no _g_ )
//TODO Add reload from file key (strg+r)
//TODO Use long internal
//TODO autom use SH_EXTENDED when programm_size <= 80x80 (and no Param)
