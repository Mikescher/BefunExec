namespace BefunExec.MathExtensions
{
	public static class MathExt
	{
		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			var temp = lhs;
			lhs = rhs;
			rhs = temp;
		}
	}
}
