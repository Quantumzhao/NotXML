using System;
using System.Collections.Generic;

namespace NotXML
{
	public class DValue : IApplicable, IFunction
	{
		public static DValue GetString(string quotedString) => quotedString[1..^1];
		private DValue(decimal input) => _Data = input;
		private DValue(string input) => _Data = input;
		public static implicit operator DValue(decimal input) => new DValue(input);
		public static implicit operator DValue(int input) => new DValue(input);
		public static implicit operator DValue(double input) => new DValue((decimal)input);
		public static implicit operator DValue(bool input) => new DValue(input ? 1 : 0);
		public static implicit operator DValue(string input) => new DValue(input);

		public static explicit operator bool(DValue input)
		{
			if (input.TryCast(out decimal d) && d != 0)
			{
				return true;
			}
			else if (input.TryCast(out string s) && s != string.Empty)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static DValue Create(string content)
		{
			if (decimal.TryParse(content, out decimal d))
			{
				return d;
			}
			else if (bool.TryParse(content, out bool b))
			{
				return b;
			}
			else
			{
				return content;
			}
		}

		private object _Data;

		public List<IApplicable> Args => new List<IApplicable>();

		public bool TryCast(out decimal output)
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
		public bool TryCast(out string output)
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

		public IFunction Invoke() => this;

		public override string ToString() => _Data.ToString();
	}
}