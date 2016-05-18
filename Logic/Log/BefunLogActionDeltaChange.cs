namespace BefunExec.Logic.Log
{
	public class BefunLogActionDeltaChange : BefunLogAction
	{
		private readonly int dx;
		private readonly int dy;

		public BefunLogActionDeltaChange(int olddeltaX, int olddeltaY)
		{
			dx = olddeltaX;
			dy = olddeltaY;
		}

		public override void Reverse(BefunProg prog)
		{
			prog.Delta.Set(dx, dy);
		}
	}
}
