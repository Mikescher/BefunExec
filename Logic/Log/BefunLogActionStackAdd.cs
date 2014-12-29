
namespace BefunExec.Logic.Log
{
	public class BefunLogActionStackAdd : BefunLogAction
	{
		public BefunLogActionStackAdd()
		{

		}

		public override void Reverse(BefunProg prog)
		{
			prog.pop(false);
		}
	}
}
