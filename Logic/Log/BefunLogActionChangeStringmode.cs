﻿
namespace BefunExec.Logic.Log
{
	public class BefunLogActionChangeStringmode : BefunLogAction
	{

		public BefunLogActionChangeStringmode()
		{

		}

		public override void Reverse(BefunProg prog)
		{
			prog.Stringmode = !prog.Stringmode;
		}
	}
}
