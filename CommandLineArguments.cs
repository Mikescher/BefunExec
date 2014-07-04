using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace BefunExec
{
	public class CommandLineArguments
	{
		private StringDictionary Parameters;

		public CommandLineArguments(string[] Args)
		{
			Parameters = new StringDictionary();
			Regex Spliter = new Regex(@"^-{1,2}|^/|=|:",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);

			Regex Remover = new Regex(@"^['""]?(.*?)['""]?$",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);

			string Parameter = null;
			string[] Parts;

			foreach (string Txt in Args)
			{
				Parts = Spliter.Split(Txt, 3);

				switch (Parts.Length)
				{
					case 1:
						if (Parameter != null)
						{
							if (!Parameters.ContainsKey(Parameter))
							{
								Parts[0] =
									Remover.Replace(Parts[0], "$1");

								Parameters.Add(Parameter, Parts[0]);
							}
							Parameter = null;
						}
						break;

					case 2:
						if (Parameter != null)
						{
							if (!Parameters.ContainsKey(Parameter))
								Parameters.Add(Parameter, "true");
						}
						Parameter = Parts[1];
						break;

					case 3:
						if (Parameter != null)
						{
							if (!Parameters.ContainsKey(Parameter))
								Parameters.Add(Parameter, "true");
						}

						Parameter = Parts[1];

						if (!Parameters.ContainsKey(Parameter))
						{
							Parts[2] = Remover.Replace(Parts[2], "$1");
							Parameters.Add(Parameter, Parts[2]);
						}

						Parameter = null;
						break;
				}
			}
			if (Parameter != null)
			{
				if (!Parameters.ContainsKey(Parameter))
					Parameters.Add(Parameter, "true");
			}
		}

		public bool Contains(string key)
		{
			return Parameters.ContainsKey(key);
		}

		public bool IsSet(string key)
		{
			return Parameters.ContainsKey(key) && Parameters[key] != null;
		}

		public string this[string Param]
		{
			get
			{
				return (Parameters[Param]);
			}
		}

		public bool IsInt(string p)
		{
			int a;
			return IsSet(p) && int.TryParse(Parameters[p], out a);
		}

		public bool isEmpty()
		{
			return Parameters.Count == 0;
		}
	}
}
