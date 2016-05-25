using BefunExec.View.OpenGL.OGLMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BefunExec.Logic
{
	public class FileInformation
	{
		public string Code = "";

		public List<Vec2I> Breakpoints = null;
		public List<Tuple<Vec2I, byte>> Watchpoints = null;

		public int GetProgWidth()
		{
			return Code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Max(s => s.Length);
		}

		public int GetProgHeight()
		{
			return Code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Length;
		}

		public long[,] GetRaster()
		{
			int w, h;

			long[,] prog = new long[w = GetProgWidth(), h = GetProgHeight()];

			string[] split = Code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					prog[x, y] = (x < split[y].Length) ? split[y][x] : ' ';
				}
			}

			return prog;
		}

		public void ApplyMetadata(BefunProg prog, bool forceOverride)
		{
			if (Breakpoints != null)
			{
				prog.Breakpointcount = Breakpoints.Count;
				foreach (var point in Breakpoints)
				{
					prog.Breakpoints[point.X, point.Y] = false;
					prog.Breakpointcount++;
				}
			}
			else if (forceOverride)
			{
				prog.Breakpointcount = 0;
				for (int x = 0; x < prog.Width; x++)
				{
					for (int y = 0; y < prog.Height; y++)
					{
						prog.Breakpoints[x, y] = false;
					}
				}
			}

			if (Watchpoints != null)
			{
				prog.WatchedFields = Watchpoints.Select(p => p.Item1).ToList();
				foreach (var point in Watchpoints)
				{
					prog.WatchData[point.Item1.X, point.Item1.Y] = point.Item2;
				}
			} 
			else if (forceOverride)
			{
				prog.WatchedFields.Clear();
				for (int x = 0; x < prog.Width; x++)
				{
					for (int y = 0; y < prog.Height; y++)
					{
						prog.WatchData[x, y] = 0;
					}
				}
			}
		}
	}
}
