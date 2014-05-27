using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BefunGen.MathExtensions
{
	public class MathExt
	{
		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T temp;
			temp = lhs;
			lhs = rhs;
			rhs = temp;
		}
	}
}
