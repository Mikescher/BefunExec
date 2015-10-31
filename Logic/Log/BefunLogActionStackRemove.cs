
namespace BefunExec.Logic.Log
{
	public class BefunLogActionStackRemove : BefunLogAction
	{
		private readonly long stackval;

		public BefunLogActionStackRemove(long val)
		{
			this.stackval = val;
		}

		public override void Reverse(BefunProg prog)
		{
			prog.Push(stackval, false);
		}
	}
}
