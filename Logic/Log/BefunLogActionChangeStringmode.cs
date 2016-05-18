
namespace BefunExec.Logic.Log
{
	public class BefunLogActionChangeStringmode : BefunLogAction
	{
		public override void Reverse(BefunProg prog)
		{
			prog.Stringmode = !prog.Stringmode;
		}
	}
}
