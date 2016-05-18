
namespace BefunExec.Logic.Log
{
	public class BefunLogActionStackAdd : BefunLogAction
	{
		public override void Reverse(BefunProg prog)
		{
			prog.Pop(false);
		}
	}
}
