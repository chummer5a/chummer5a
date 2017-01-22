using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Chummer.Properties;

namespace Chummer.Datastructures
{
	class TranslatedField<T> where T : class
	{
		private readonly Dictionary<T, T> _translate = new Dictionary<T, T>();
		private readonly Dictionary<T, T> _back = new Dictionary<T, T>();

		public void Add(T orginal, T translated)
		{
			_translate[orginal] = translated;
			_back[translated] = orginal;
		}

		public void AddRange(IEnumerable<KeyValuePair<T, T>> range)
		{
			foreach (KeyValuePair<T, T> tuple in range)
			{
				Add(tuple.Key, tuple.Value);
			}
		}

		public T Read(T original, ref T translated)
		{
			//TODO: should probably make sure Language don't change before restart
			//I feel that stuff could break in other cases
			if (GlobalOptions.Instance.Language == "en-us")
			{
				return original;
			}
			else
			{
				if (translated != null)
                    return translated;

			    T objTemp;
				if (original != null && _translate.TryGetValue(original, out objTemp))
				{
				    translated = objTemp;
                    return translated;
				}

				return original;
			}
		}

		public void Write(T value, ref T original, ref T translated)
		{
			if (GlobalOptions.Instance.Language == "en-us")
			{
                T objTemp;
                if (original != null && _translate.TryGetValue(original, out objTemp))
				{
				    if (objTemp == translated)
				    {
                        _translate.TryGetValue(value, out translated);
				    }
				}
                original = value;
			}
			else
			{
                if (!_back.TryGetValue(value, out original))
                    original = value;

				translated = value;
			}
		}

	}
}
