﻿using BefunExec.View.OpenGL.OGLMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BefunExec.Logic
{
	public class FileInformation
	{
		public string Code = "";

		public List<Vec2I> Breakpoints = null;
		public List<WatchedField> Watchpoints = null;

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
				prog.WatchedFields = Watchpoints.Where(p => p.X >= 0 && p.Y >= 0 && p.X < prog.Width && p.Y < prog.Height).ToList();
				foreach (var point in Watchpoints)
				{
					prog.WatchData[point.X, point.Y] = true;
				}
			}
			else if (forceOverride)
			{
				prog.WatchedFields = new List<WatchedField>();
				for (int x = 0; x < prog.Width; x++)
				{
					for (int y = 0; y < prog.Height; y++)
					{
						prog.WatchData[x, y] = false;
					}
				}
			}
		}
	}
}
