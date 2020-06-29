using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NotXML
{
	public class DVector : IEnumerable<IFunction>, IFunction
	{
		public DVector(List<IFunction> entries) => _Entries = entries;

		private List<IFunction> _Entries;

		public IFunction this[int index]
		{
			get
			{
				if (index >= _Entries.Count)
				{
					return (DValue)0;
				}
				else
				{
					return _Entries[index];
				}
			}
			set
			{
				if (index >= _Entries.Count)
				{
					for (int i = 0; i < index - _Entries.Count; i++)
					{
						_Entries.Add((DValue)0);
					}
					_Entries.Add(value);
				}
				else
				{
					_Entries[index] = value;
				}
			}
		}

		public DValue Count => _Entries.Count;
		public DValue Contains(IFunction function) => _Entries.Contains(function);

		public IEnumerator<IFunction> GetEnumerator() => _Entries.GetEnumerator();

		public IFunction Invoke() => this;

		IEnumerator IEnumerable.GetEnumerator() => _Entries.GetEnumerator();
	}
}
