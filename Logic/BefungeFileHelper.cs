using System;
using System.IO;
using System.Linq;

namespace BefunExec.Logic
{
	public static class BefungeFileHelper
	{
		public static string LoadTextFile(string fn)
		{
			if (File.Exists(fn))
			{
				try
				{
					string[] code = File.ReadAllLines(fn);

					if (code.Length >= 3 && code[0].StartsWith("CodePiece") && code[1].StartsWith("{") && code.Any(p => p.Trim() == "}"))
					{
						int start = 2;
						int end = code.Select((p, i) => new { Item = p, Index = i }).Where(p => p.Item.Trim() == "}").Last().Index;

						code = code.Skip(start).Take(end - start).ToArray();
					}

					if (code.Length > 0 && code.Any(p => p.Length > 0))
					{
						return string.Join(Environment.NewLine, code);
					}
					else
					{
						return null;
					}

				}
				catch
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}
	}
}
