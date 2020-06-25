using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace NotXML
{
	public interface IApplicable 
	{
		DValue Invoke(List<IApplicable> args);
		List<IApplicable> Args { get; }
	}

	public class Program
	{
		static void Main(string[] args)
		{
			//if (args.Length == 0)
			//{
			//	Console.WriteLine("Please provide a .notxml file path");
			//	return;
			//}

			var doc = GetMain(@"C:\Users\yisha.000\source\repos\NotXML\NotXML\test.notxml");
			var main = new Function(doc);
			main.Body.Invoke(new List<IApplicable>());
		}

		private static XmlNode GetMain(string path)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(path);
			return doc.DocumentElement;
		}
	}

	public class Function
	{
		private Function(string id) => ID = id;
		public Function(XmlNode node)
		{
			ID = node.Attributes?["id"]?.InnerText ?? string.Empty;

			foreach (XmlNode component in node.ChildNodes)
			{
				switch (component.Name)
				{
					case "par":
						FuncDefs.Add(new Function(component.Attributes["id"].InnerText));
						break;

					case "fun":
						FuncDefs.Add(new Function(component));
						break;

					case "app":
						if (component.InnerXml != component.InnerText)
						{
							Body = new Application(component, this);
						}
						else
						{
							Body = (DValue)component.InnerText;
						}
						break;

					default:
						break;
				}
			}
		}

		public readonly string ID;
		public readonly List<Function> FuncDefs = new List<Function>();
		public IApplicable Body = null;

		public DValue GetValue(List<IApplicable> args)
		{
			if (Body is Application application)
			{
				return application.Invoke(args);
			}
			else
			{
				return Body as DValue;
			}
		}
	}

	public class Application : IApplicable
	{
		private Application(DValue value)
		{
			Link = "valueof";
			Args.Add(value);
		}
		public Application(XmlNode node, Function parent)
		{
			Link = node?.Attributes?["id"]?.InnerText ?? string.Empty;
			_Parent = parent;

			if (node.InnerXml[0] != '<')
			{
				var value = node.InnerText;
				if (node.InnerText[0] == '\"')
				{
					Args.Add(DValue.GetString(value));
				}
				else
				{
					Args.Add((DValue)value);
				}
			}
			else
			{
				foreach (XmlNode app in node.ChildNodes)
				{
					if (app.InnerText == app.InnerXml)
					{
						Args.Add((DValue)app.InnerText);
					}
					else
					{
						Args.Add(new Application(app, _Parent));
					}
				}
			}
		}

		internal string Link;
		public List<IApplicable> Args { get; } = new List<IApplicable>();
		private Function _Parent;

		public DValue Invoke(List<IApplicable> args)
		{
			for (int i = 0; i < args.Count; i++)
			{
				if (_Parent != null && _Parent.FuncDefs[i].Body == null)
				{
					_Parent.FuncDefs[i].Body = args[i];
				}
			}

			if (_Parent != null && _Parent.Body is Application app)
			{
				Function func;
				if (app.Link == _Parent.ID)
				{
					func = _Parent;
				}
				else
				{
					func = _Parent.FuncDefs.Find(f => f.ID == app.Link);
				}

				var values = Args.Select(a => a.Invoke(a.Args)).ToList();

				if (func == null)
				{
					return Stdlib.Call(app.Link, values);
				}
				else
				{
					return Invoke(app.Args);
				}
			}
			else
			{
				return Stdlib.Call("valueof", Args.Select(a => a.Invoke(a.Args)).ToList());
			}
		}
	}

	public class DValue : IApplicable
	{
		public static DValue GetString(string quotedString) => quotedString[1..^1];
		private DValue(decimal input) => _Data = input;
		private DValue(string input) => _Data = input;
		public static implicit operator DValue(decimal input) => new DValue(input);
		public static implicit operator DValue(int input) => new DValue(input);
		public static implicit operator DValue(double input) => new DValue((decimal)input);
		public static implicit operator DValue(bool input) => new DValue(input ? 1 : 0);
		public static implicit operator DValue(string input) => new DValue(input);

		private object _Data;

		public List<IApplicable> Args => new List<IApplicable>();

		public bool TryGet(out decimal output)
		{
			if (_Data is decimal)
			{
				output = (decimal)_Data;
				return true;
			}
			else
			{
				var ret = decimal.TryParse((string)_Data, out output);
				return ret;
			}
		}
		public bool TryGet(out string output)
		{
			if (_Data is string)
			{
				output = (string)_Data;
				return true;
			}
			else
			{
				output = _Data.ToString();
				return true;
			}
		}

		public DValue Invoke(List<IApplicable> args) => this;

		public override string ToString() => _Data.ToString();
	}

	public static class Stdlib
	{
		public static DValue Call(string name, List<DValue> values)
		{
			switch (name)
			{
				case "valueof":
					return values[0];

				case "print":
					return Print(values[0]);

				default:
					return StdMath.Call(name, values);
			}
		}

		public static DValue Print(DValue value)
		{
			Console.WriteLine(value);

			return true;
		}
	}

	public static class StdMath
	{
		public static DValue Call(string name, List<DValue> values)
		{
			switch (name)
			{
				case "add":
					return Add(values[0], values[1]);

				default:
					throw new ArgumentException();
			}
		}

		public static DValue Add(DValue v1, DValue v2)
		{
			if (v1.TryGet(out decimal d1) && v2.TryGet(out decimal d2))
			{
				return d1 + d2;
			}
			else
			{
				throw new InvalidCastException();
			}
		}
	}
}
