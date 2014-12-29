
namespace BefunExec.Logic.Log
{
	public class BefunLogActionDeltaChange : BefunLogAction
	{
		private readonly int dx;
		private readonly int dy;

		public BefunLogActionDeltaChange(int olddeltaX, int olddeltaY)
		{
			this.dx = olddeltaX;
			this.dy = olddeltaY;
		}

		public override void Reverse(BefunProg prog)
		{
			prog.delta.Set(dx, dy);
		}
	}
}
