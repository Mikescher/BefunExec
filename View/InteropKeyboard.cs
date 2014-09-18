using BefunGen.MathExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BefunExec.View
{
	public class InteropKeyboard
	{
		[Flags]
		private enum KeyStates
		{
			None = 0,
			Down = 1,
			Toggled = 2
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		private static extern short GetKeyState(int keyCode);

		private Dictionary<Keys, bool> last = new Dictionary<Keys, bool>();
		private Dictionary<Keys, bool> now = new Dictionary<Keys, bool>();

		public bool this[Keys k]
		{
			get
			{
				if (last.ContainsKey(k))
				{
					return now[k] && !last[k];
				}
				else
				{
					now.Add(k, false);
					last.Add(k, false);
					return false;
				}
			}
		}

		public bool isDown(Keys k)
		{
			if (now.ContainsKey(k))
			{
				return now[k];
			}
			else
			{
				now.Add(k, false);
				return false;
			}
		}

		public void update()
		{
			MathExt.Swap(ref last, ref now);
			now.Clear();

			foreach (KeyValuePair<Keys, bool> kvp in last)
			{
				now.Add(kvp.Key, IsKeyDown(kvp.Key));
			}
		}

		private KeyStates GetKeyState(Keys key)
		{
			KeyStates state = KeyStates.None;

			short retVal = GetKeyState((int)key);

			//If the high-order bit is 1, the key is down
			//otherwise, it is up.
			if ((retVal & 0x8000) == 0x8000)
				state |= KeyStates.Down;

			//If the low-order bit is 1, the key is toggled.
			if ((retVal & 1) == 1)
				state |= KeyStates.Toggled;

			return state;
		}

		private bool IsKeyDown(Keys key)
		{
			return KeyStates.Down == (GetKeyState(key) & KeyStates.Down);
		}

		public bool AnyKey()
		{
			return now.Any(p => p.Value);
		}
	}
}
