using BefunExec.View.OpenGL.OGLMath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BefunExec.Logic
{
	public static class BefungeFileHelper
	{
		private static readonly Regex RexBase = new Regex(@"^#\$(?<name>[A-Za-z]+)\s*(?<parameter>.*)\s*$", RegexOptions.Compiled);
		
		public static FileInformation LoadTextFile(string fn, bool preprocess)
		{
			if (File.Exists(fn))
			{
				try
				{
					string[] code = File.ReadAllLines(fn);

					if (code.Length >= 3 && code[0].StartsWith("CodePiece") && code[1].StartsWith("{") && code.Any(p => p.Trim() == "}"))
					{
						int start = 2;
						int end = code.Select((p, i) => new { Item = p, Index = i }).Last(p => p.Item.Trim() == "}").Index;

						code = code.Skip(start).Take(end - start).ToArray();
					}

					if (code.Length > 0 && code.Any(p => p.Length > 0))
					{
						if (preprocess)
						{
							return ExecutePreprocessor(code);
						}
						else
						{
							return new FileInformation
							{
								Code = string.Join(Environment.NewLine, code)
							};
						}
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

		/// <summary>
		/// Examples:
		/// 
		/// #$watch[0,2]:int
		/// #$replace C -> 1
		/// #break[3,17]
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private static FileInformation ExecutePreprocessor(string[] input)
		{
			var info = new FileInformation();

			List<Tuple<char, char>> replacements = new List<Tuple<char, char>>();

			for (int i = 0; i < input.Length; i++)
			{
				var line = input[i];

				if (!line.StartsWith("#$")) continue;

				var rex = RexBase.Match(line);
				if (! rex.Success) continue;

				var name = rex.Groups["name"].Value.Trim().ToLower();
				var parameter = rex.Groups["parameter"].Value.Trim();

				if (name == "replace")
				{
					var repl = ParseReplParam(parameter);

					if (repl == null)
					{
						Console.Out.WriteLine("[PREPROCESSOR] Line {0}: Cannot parse parameter of statement\r\n{1}", i + 1, line);
						continue;
					}

					replacements.Add(repl);

					Console.Out.WriteLine("[PREPROCESSOR] Replace '{0}' with '{1}'", repl.Item1, repl.Item2);
				}
				else if (name == "watch")
				{
					if (info.Watchpoints == null) info.Watchpoints = new List<Tuple<Vec2I, byte>>();
					var point = ParseWatchParam(parameter);

					if (point == null)
					{
						Console.Out.WriteLine("[PREPROCESSOR] Line {0}: Cannot parse parameter of statement\r\n{1}", i+1, line);
						continue;
					}

					if (!info.Watchpoints.Any(p => p.Item1.Equals(point.Item1)))
					{
						info.Watchpoints.Add(point);

						Console.Out.WriteLine("[PREPROCESSOR] Add Watchpoint [{0}|{1}] with type {2}", point.Item1.X, point.Item1.Y, point.Item2);
					}
				}
				else if (name == "break")
				{
					if (info.Breakpoints == null) info.Breakpoints = new List<Vec2I>();
					var point = ParseVecParam(parameter);

					if (point == null)
					{
						Console.Out.WriteLine("[PREPROCESSOR] Line {0}: Cannot parse parameter of statement\r\n{1}", i + 1, line);
						continue;
					}

					if (!info.Breakpoints.Any(p => p.Equals(point)))
					{
						info.Breakpoints.Add(point);

						Console.Out.WriteLine("[PREPROCESSOR] Add Breakpoint [{0}|{1}]", point.X, point.Y);
					}
				}

			}

			foreach (var rep in replacements)
			{
				for (int i = 0; i < input.Length; i++)
				{
					input[i] = input[i].Replace(rep.Item1, rep.Item2);
				}
			}

			info.Code = string.Join(Environment.NewLine, input);

			return info;
		}

		private static Tuple<char, char> ParseReplParam(string param)
		{
			param = param.Trim();
			var split = param.Split(new [] { "->" }, StringSplitOptions.None);
			if (split.Length != 2) return null;

			split[0] = split[0].Trim();
			split[1] = split[1].Trim();

			if (split[0].Length != 1) return null;
			if (split[1].Length != 1) return null;

			return Tuple.Create(split[0][0], split[1][0]);
		}

		private static Vec2I ParseVecParam(string param)
		{
			param = param.Trim();

			if (param.Length < 5) return null;
			if (param[0] != '[') return null;
			if (param.Last() != ']') return null;

			param = param.Substring(1, param.Length - 2);
			var split = param.Split(',');
			if (split.Length != 2) return null;

			split[0] = split[0].Trim();
			split[1] = split[1].Trim();

			int xx;
			int yy;

			if (int.TryParse(split[0], out xx) && int.TryParse(split[1], out yy))
			{
				return new Vec2I(xx, yy);
			}

			return null;
		}

		private static Tuple<Vec2I, byte> ParseWatchParam(string param)
		{
			param = param.Trim();
			var split = param.Split(':');
			if (split.Length != 2) return null;

			var spos = split[0].Trim();
			var stype = split[1].Trim().ToLower();

			Vec2I pos = ParseVecParam(spos);

			if (pos == null) return null;

			if (stype == "1" || stype == "int" || stype == "integer")
				return Tuple.Create<Vec2I, byte>(pos, 1);

			if (stype == "2" || stype == "long" || stype == "int8")
				return Tuple.Create<Vec2I, byte>(pos, 2);

			if (stype == "3" || stype == "char" || stype == "character")
				return Tuple.Create<Vec2I, byte>(pos, 3);

			if (stype == "4" || stype == "hex")
				return Tuple.Create<Vec2I, byte>(pos, 4);

			if (stype == "5" || stype == "hex8" || stype == "longhex")
				return Tuple.Create<Vec2I, byte>(pos, 5);

			if (stype == "6" || stype == "binary" || stype == "bits" || stype == "bit24")
				return Tuple.Create<Vec2I, byte>(pos, 6);

			return null;
		}
	}
}
