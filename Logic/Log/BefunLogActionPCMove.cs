namespace BefunExec.Logic.Log
{
	class BefunLogActionPCMove : BefunLogAction
	{
		private readonly int px;
		private readonly int py;

		public BefunLogActionPCMove(int oldx, int oldy)
		{
			px = oldx;
			py = oldy;
		}

		public override void Reverse(BefunProg prog)
		{
			prog.PC.Set(px, py);
		}
	}
}
