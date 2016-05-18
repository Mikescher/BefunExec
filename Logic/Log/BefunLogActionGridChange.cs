namespace BefunExec.Logic.Log
{
	public class BefunLogActionGridChange : BefunLogAction
	{
		private readonly long px;
		private readonly long py;
		private readonly long vold;

		public BefunLogActionGridChange(long x, long y, long oldvalue)
		{
			px = x;
			py = y;
			vold = oldvalue;
		}

		public override void Reverse(BefunProg prog)
		{
			prog.ChangeRaster(px, py, vold, false);
		}
	}
}
