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

        public void AddRange(IEnumerable<Tuple<T, T>> range)
        {
            foreach (Tuple<T, T> tuple in range)
            {
                Add(tuple.Item1, tuple.Item2);
            }
        }

        public T Read(T orginal, ref T translated)
        {
            //TODO: should probably make sure Language don't change before restart
            //I feel that stuff could break in other cases
            if (GlobalOptions.Instance.Language == "en-us")
            {
                return orginal;
            }
            else
            {
                if(translated != null) return translated;

                if (orginal != null && _translate.TryGetValue(orginal, out translated))
                {
                    return translated;
                }

                return orginal;
            }
        }

        public void Write(T value, ref T orginal, ref T translated)
        {
            if (GlobalOptions.Instance.Language == "en-us")
            {
                if (orginal != null && value != null)
                {
                    T objTmp;
                    if (_translate.TryGetValue(orginal, out objTmp) && objTmp == translated)
                    {
                        _translate.TryGetValue(value, out translated);
                    }
                }
                orginal = value;
            }
            else
            {
                if (value != null && !_back.TryGetValue(value, out orginal))
                    orginal = value;

                translated = value;
            }
        }

    }
}
