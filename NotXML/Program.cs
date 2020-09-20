using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace NotXML
{
	public interface IApplicable
	{
		DValue Invoke(List<IApplicable> args);
		List<IApplicable> Args { get; }
	}

	public interface IFunction 
	{
		IFunction Invoke();
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

			var doc = GetMain(@"C:\Users\yisha.000\source\repos\NotXML\NotXML\fib.notxml");
			var main = Function.Create(doc, new HashSet<Function>());
			(main as Function).Environment.First(f => f.ID == "argv").Application = new Application((DValue)2);
			main.Invoke();
		}

		private static XmlNode GetMain(string path)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(path);
			return doc.DocumentElement;
		}
	}

	public class Function : IFunction
	{
		private Function() { }
		public static IFunction Create(XmlNode node, HashSet<Function> env)
		{
			var func = new Function();
			func.Environment = new HashSet<Function>(env);
			func.Environment.Add(func);

			func.ID = node.Attributes?["id"]?.InnerText ?? string.Empty;
			// It is a literal or an alias
			if (func.ID == string.Empty)
			{
				// find the aliasing function
				var specifiedFunc = env.FirstOrDefault(f => f.ID == node.InnerText);
				if (specifiedFunc != null)
				{
					func.Application = new Application(specifiedFunc);
				}
				// generate the literal
				else if (node.InnerText[0] == '\"')
				{
					var strLit = DValue.GetString(node.InnerText);
					func.Application = new Application(strLit);
				}
				else
				{
					var valueLit = DValue.Create(node.InnerText);
					func.Application = new Application(valueLit);
				}
			}

			foreach (XmlNode component in node.ChildNodes)
			{
				switch (component.Name)
				{
					case "par":
						var param = new Function(component.Attributes["id"].InnerText);
						param.Environment = func.Environment;
						func.Parameters.Add(param);
						func.Environment.Add(param);
						break;

					case "fun":
						var newFunc = Function.Create(component, func.Environment);
						if (newFunc is Function castedFunc)
						{
							func.Environment.Add(castedFunc);
						}
						else
						{
							throw new InvalidCastException();
						}
						break;
					case "app":
						func.Application = new Application(component, func.Environment);
						break;

					default:
						break;
				}
			}

			return func;
		}

		private Function(string id) => ID = id;
		public string ID;
		public List<Function> Parameters = new List<Function>();
		public HashSet<Function> Environment = new HashSet<Function>();
		public Application Application;

		public IFunction Invoke()
		{
			return Application.Invoke();
		}
	}

	public class Application : IFunction
	{
		public Application(IFunction constFunc)
		{
			Link = "valueof";
			Args.Add(constFunc);
		}
		public Application(XmlNode node, HashSet<Function> env)
		{
			Environment = new HashSet<Function>(env);
			Link = node.Attributes["id"].InnerText;

			foreach (XmlNode arg in node.ChildNodes)
			{
				switch (arg.Name)
				{
					case "fun":
						Args.Add(Function.Create(arg, Environment));
						break;

					case "app":
						Args.Add(new Application(arg, env));
						break;

					default:
						break;
				}
			}
		}

		public string Link;
		public List<IFunction> Args = new List<IFunction>();
		public HashSet<Function> Environment = new HashSet<Function>();

		public IFunction Invoke()
		{
			var func = Environment.FirstOrDefault(f => f.ID == Link);
			if (func == null)
			{
				return Stdlib.Call(Link, Args);
			}
			else
			{
				//var i = 0;
				//foreach (var f in func.Environment)
				//{
				//	if (f.Application == null)
				//	{
				//		f.Application = new Application(Args[i]);
				//		i++;
				//	}
				//}
				int i = 0;
				var argList = Args.ToList();
				foreach (var para in func.Parameters)
				{
					para.Application = new Application(argList[i].Invoke());
					i++;
				}

				//Args = Args.Select(a => a.Invoke()).ToList();
				return func.Invoke();
			}
		}
	}
}