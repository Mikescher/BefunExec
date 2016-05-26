using System;

namespace BefunExec.Logic
{
	public class WatchedField
	{
		public enum WatchType
		{
			Int  = 1,
			Int8 = 2,
			Char = 3,
			Hex  = 4,
			Hex8 = 5,
			Bits = 6,
		}

		public readonly int X;
		public readonly int Y;

		public readonly WatchType Type;
		public readonly string Name;

		public WatchedField(int x, int y, WatchType type = WatchType.Int, string name = null)
		{
			X = x;
			Y = y;
			Type = type;
			Name = name;
		}

		public Tuple<string, string> GetDisplayString(long value)
		{
			var disp = "??";
			switch (Type)
			{

				case WatchType.Int:
					disp = string.Format("{0}", value);
					break;
				case WatchType.Int8:
					disp = string.Format("{0:00000000}", value);
					break;
				case WatchType.Char:
					disp = (value > '~' || value < ' ') ? "OOB" : ((char)value).ToString();
					break;
				case WatchType.Hex:
					disp = value < 0 ? value.ToString() : string.Format("0x{0:X}", value);
					break;
				case WatchType.Hex8:
					disp = value < 0 ? value.ToString() : string.Format("0x{0:X8}", value);
					break;
				case WatchType.Bits:
					disp = value < 0 ? value.ToString() : string.Format("0b{0}", Convert.ToString(value, 2).PadLeft(24, '0'));
					break;
			}

			if (Name == null)
				return Tuple.Create(string.Format("[{0:00}, {1:00}]", X, Y), disp);
			else
				return Tuple.Create(Name, disp);
		}

		public WatchedField GetNext()
		{
			if (Type == WatchType.Bits) return null;

			return new WatchedField(X, Y, Type + 1, Name);
		}

		public static WatchType? ParseTypeFromString(string value)
		{
			value = value.Trim().ToLower();

			if (value == "1" || value == "int" || value == "integer")
				return WatchedField.WatchType.Int;

			if (value == "2" || value == "long" || value == "int8")
				return WatchedField.WatchType.Int8;

			if (value == "3" || value == "char" || value == "character")
				return WatchedField.WatchType.Char;

			if (value == "4" || value == "hex")
				return WatchedField.WatchType.Hex;

			if (value == "5" || value == "hex8" || value == "longhex")
				return WatchedField.WatchType.Hex8;

			if (value == "6" || value == "binary" || value == "bits" || value == "bit24")
				return WatchedField.WatchType.Bits;

			return null;
		}
	}
}
