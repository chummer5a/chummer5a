/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using System;
using System.Collections.Generic;

namespace Chummer
{
    public sealed class TranslatedField<T> where T : class
    {
        private readonly Dictionary<T, T> _translate = new Dictionary<T, T>();
        private readonly Dictionary<T, T> _back = new Dictionary<T, T>();
        private readonly string _strLanguage;

        public TranslatedField(string strLanguage)
        {
            _strLanguage = strLanguage;
        }

        public string Language => _strLanguage;

        public void Add(T orginal, T translated)
        {
            _translate[orginal] = translated;
            _back[translated] = orginal;
        }

        public void AddRange(IEnumerable<Tuple<T, T>> range)
        {
            if (range == null)
                return;
            foreach ((T item1, T item2) in range)
            {
                Add(item1, item2);
            }
        }

        public T Read(T orginal, ref T translated)
        {
            //TODO: should probably make sure Language don't change before restart
            //I feel that stuff could break in other cases
            if (_strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                return orginal;
            }

            if(translated != null)
                return translated;

            if (orginal != null && _translate.TryGetValue(orginal, out translated))
            {
                return translated;
            }

            return orginal;
        }

        public void Write(T value, ref T orginal, ref T translated)
        {
            if (_strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                if (orginal != null && value != null && _translate.TryGetValue(orginal, out T objTmp) && objTmp == translated)
                {
                    _translate.TryGetValue(value, out translated);
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
