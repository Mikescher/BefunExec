
namespace BefunExec.Logic.Log
{
	class BefunLogActionPCMove : BefunLogAction
	{
		private readonly int px;
		private readonly int py;
		private readonly long vold;

		public BefunLogActionPCMove(int oldx, int oldy)
		{
			this.px = oldx;
			this.py = oldy;
		}

		public override void Reverse(BefunProg prog)
		{
			prog.PC.Set(px, py);
		}
	}
}
