using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace NotXML
{
	public class StdFunc : IFunction
	{
		public StdFunc(Func<List<IFunction>, DValue> callback, List<IFunction> args)
		{
			_Args = args;
			_Invoke = callback;
		}
		private readonly List<IFunction> _Args;
		private readonly Func<List<IFunction>, DValue> _Invoke;
		public IFunction Invoke()
		{
			return _Invoke(_Args);
		}
	}
	public static class Stdlib
	{
		public static IFunction Call(string name, List<IFunction> values)
		{
			switch (name)
			{
				case "valueof":
					return Valueof(values[0]);

				case "vector":
					return Vector(values);

				case "if":
					return If(values[0], values[1], values[2]);

				default:
					return Stdio.Call(name, values);
			}
		}

		public static DValue Vector(List<IFunction> values)
		{
			throw new NotImplementedException();
		}

		public static IFunction Valueof(IFunction value)
		{
			var iter = value;
			IFunction prev;
			do
			{
				prev = iter;
				iter = iter.Invoke();
			} while (prev != iter);
			return iter;
		}

		public static IFunction If(IFunction cond, IFunction then, IFunction el)
		{
			if ((bool)(DValue)cond.Invoke())
			{
				return then.Invoke();
			}
			else
			{
				return el.Invoke();
			}
		}

		public static DValue Fold(Function accumulator, DValue init, DValue vector)
		{
			throw new NotImplementedException();
		}
	}

	public static class Stdio
	{
		public static DValue Call(string name, List<IFunction> values)
		{
			switch (name)
			{
				case "print":
					return Print((DValue)values[0].Invoke());

				case "scan":
					return Scan();

				default:
					return StdMath.Call(name, values);
			}
		}

		public static DValue Print(DValue value)
		{
			Console.WriteLine(value);
			return true;
		}

		public static DValue Scan()
		{
			var s = Console.ReadLine();
			if (decimal.TryParse(s, out decimal d))
			{
				return d;
			}
			else if (bool.TryParse(s, out bool b))
			{
				return b;
			}
			else
			{
				return s;
			}
		}
	}

	public static class StdMath
	{
		public static DValue Call(string name, List<IFunction> values)
		{
			switch (name)
			{
				case "add":
					return Add(values[0], values[1]);

				case "sub":
					return Subtract(values[0], values[1]);

				case "mult":
					return Multiply(values[0], values[1]);

				case "div":
					return Divide(values[0], values[1]);

				case "rdiv":
					return Quotient(values[0], values[1]);

				case "mod":
					return Remainder(values[0], values[1]);

				case "ls":
					return Less(values[0], values[1]);

				case "gt":
					return Greater(values[0], values[1]);

				case "leq":
					return LessEqual(values[0], values[1]);

				case "geq":
					return GreaterEqual(values[0], values[1]);

				case "eq":
					return Equal(values[0], values[1]);

				case "neq":
					return NotEqual(values[0], values[1]);

				case "and":
					return And(values[0], values[1]);

				case "or":
					return Or(values[0], values[1]);

				case "not":
					return Not(values[0]);

				default:
					return StdString.Call(name, values);
			}
		}

		public static DValue Add         (IFunction v1, IFunction v2) => Binop(v1, v2, (n1, n2) => n1 + n2);
		public static DValue Subtract    (IFunction v1, IFunction v2) => Binop(v1, v2, (n1, n2) => n1 - n2);
		public static DValue Multiply    (IFunction v1, IFunction v2) => Binop(v1, v2, (n1, n2) => n1 * n2);
		public static DValue Divide      (IFunction v1, IFunction v2) => Binop(v1, v2, (n1, n2) => n1 / n2);
		public static DValue Quotient    (IFunction v1, IFunction v2) => Binop(v1, v2, (n1, n2) => (int)n1 / (int)n2);
		public static DValue Remainder   (IFunction v1, IFunction v2) => Binop(v1, v2, (n1, n2) => (int)n1 % (int)n2);
		public static DValue Equal       (IFunction v1, IFunction v2) => Binop(v1, v2, (n1, n2) => n1 == n2);
		public static DValue NotEqual    (IFunction v1, IFunction v2) => Binop(v1, v2, (n1, n2) => n1 != n2);
		public static DValue Greater     (IFunction v1, IFunction v2) => Binop(v1, v2, (n1, n2) => n1 > n2);
		public static DValue Less        (IFunction v1, IFunction v2) => Binop(v1, v2, (n1, n2) => n1 < n2);
		public static DValue GreaterEqual(IFunction v1, IFunction v2) => Binop(v1, v2, (n1, n2) => n1 >= n2);
		public static DValue LessEqual   (IFunction v1, IFunction v2) => Binop(v1, v2, (n1, n2) => n1 <= n2);
		public static DValue And         (IFunction v1, IFunction v2) => (bool)(DValue)v1.Invoke() && (bool)(DValue)v2.Invoke();
		public static DValue Or          (IFunction v1, IFunction v2) => ((DValue)v1.Invoke()) == 0 || (bool)(DValue)v2.Invoke();
		public static DValue Not         (IFunction v)                => !(bool)(DValue)v.Invoke();

		private static DValue Binop<T>(IFunction f1, IFunction f2, Func<decimal, decimal, T> op)
		{
			if (f1.Invoke() is DValue v1 && f2.Invoke() is DValue v2)
			{
				if (v1.TryCast(out decimal d1) && v2.TryCast(out decimal d2))
				{
					var res = op(d1, d2);
					if (res is decimal d)
					{
						return d;
					}
					else if (res is bool b)
					{
						return b;
					}
					else
					{
						throw new InvalidCastException();
					}
				}
				else
				{
					throw new InvalidCastException();
				}
			}
			else
			{
				throw new InvalidCastException();
			}
		}
	}

	public static class StdString
	{
		public static DValue Call(string name, List<IFunction> values)
		{
			switch (name)
			{
				case "concat":
					return Concatnate((DValue)values[0].Invoke(), (DValue)values[1].Invoke());

				default:
					throw new ArgumentException();
			}
		}

		public static DValue Concatnate(DValue v1, DValue v2)
		{
			if (v1.TryCast(out string s1) && v2.TryCast(out string s2))
			{
				return s1 + s2;
			}
			else
			{
				throw new InvalidCastException();
			}
		}
	}
}