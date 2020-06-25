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
		//public Function(XmlNode node)
		//{
		//	ID = node.Attributes?["id"]?.InnerText ?? string.Empty;
		//	if (ID == string.Empty)
		//	{

		//	}

		//	foreach (XmlNode component in node.ChildNodes)
		//	{
		//		switch (component.Name)
		//		{
		//			case "par":
		//				FuncDefs.Add(new Function(component.Attributes["id"].InnerText));
		//				break;

		//			case "fun":
		//				FuncDefs.Add(new Function(component));
		//				break;

		//			case "app":
		//				if (component.InnerXml != component.InnerText)
		//				{
		//					Body = new Application(component, this);
		//				}
		//				else
		//				{
		//					Body = (DValue)component.InnerText;
		//				}
		//				break;

		//			default:
		//				break;
		//		}
		//	}
		//}
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
						func.Environment.Add(new Function(component.Attributes["id"].InnerText));
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
		//public List<Function> FuncDefs = new List<Function>();
		//public IApplicable Body = null;
		public HashSet<Function> Environment = new HashSet<Function>();
		public Application Application;

		//public DValue GetValue(List<IApplicable> args)
		//{
		//	if (Body is Application application)
		//	{
		//		return application.Invoke(args);
		//	}
		//	else
		//	{
		//		return Body as DValue;
		//	}
		//}

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
				return func.Invoke();
			}
		}
	}


	//public class Application// : IApplicable
	//{
	//	private Application(DValue value)
	//	{
	//		Link = "valueof";
	//		Args.Add(value);
	//	}
	//	public Application(XmlNode node, Function parent)
	//	{
	//		Link = node?.Attributes?["id"]?.InnerText ?? string.Empty;
	//		_Parent = parent;

	//		if (node.InnerXml[0] != '<')
	//		{
	//			var value = node.InnerText;
	//			if (node.InnerText[0] == '\"')
	//			{
	//				Args.Add(DValue.GetString(value));
	//			}
	//			else
	//			{
	//				Args.Add((DValue)value);
	//			}
	//		}
	//		else
	//		{
	//			foreach (XmlNode app in node.ChildNodes)
	//			{
	//				if (app.InnerText == app.InnerXml)
	//				{
	//					Args.Add((DValue)app.InnerText);
	//				}
	//				else
	//				{
	//					//Args.Add(new Application(app, _Parent));
	//				}
	//			}
	//		}
	//	}

	//	internal string Link;
	//	public List<IApplicable> Args { get; } = new List<IApplicable>();
	//	private Function _Parent;

	//	//public DValue Invoke(List<IApplicable> args)
	//	//{
	//	//	for (int i = 0; i < args.Count; i++)
	//	//	{
	//	//		if (_Parent != null && _Parent.FuncDefs[i].Body == null)
	//	//		{
	//	//			_Parent.FuncDefs[i].Body = args[i];
	//	//		}
	//	//	}

	//	//	if (_Parent != null && _Parent.Body is Application app)
	//	//	{
	//	//		Function func;
	//	//		if (app.Link == _Parent.ID)
	//	//		{
	//	//			func = _Parent;
	//	//		}
	//	//		else
	//	//		{
	//	//			func = _Parent.FuncDefs.Find(f => f.ID == app.Link);
	//	//		}

	//	//		var values = Args.Select(a => a.Invoke(a.Args)).ToList();

	//	//		if (func == null)
	//	//		{
	//	//			return Stdlib.Call(app.Link, values);
	//	//		}
	//	//		else
	//	//		{
	//	//			return Invoke(app.Args);
	//	//		}
	//	//	}
	//	//	else
	//	//	{
	//	//		return Stdlib.Call("valueof", Args.Select(a => a.Invoke(a.Args)).ToList());
	//	//	}
	//	//}
	//}
}