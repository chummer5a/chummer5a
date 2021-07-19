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

using System.Windows.Forms;

namespace Chummer
{
    /// <summary>
    /// Helper class that allows inverting the state of a property for databinding.
    /// </summary>
    internal class NegatableBinding
    {
        private string PropertyName { get; }
        public object DataSource { get; }
        public string DataMember { get; }
        public bool Negate { get; }
        public bool OneWay { get; }

        public NegatableBinding(string propertyName, object dataSource, string dataMember, bool negate = false, bool oneway = false)
        {
            PropertyName = propertyName;
            DataSource = dataSource;
            DataMember = dataMember;
            Negate = negate;
            OneWay = oneway;
        }

        public static implicit operator Binding(NegatableBinding eb)
        {
            var binding = new Binding(eb.PropertyName, eb.DataSource, eb.DataMember, false,
                eb.OneWay ? DataSourceUpdateMode.Never : DataSourceUpdateMode.OnPropertyChanged);
            if (!eb.Negate) return binding;
            binding.Parse += NegateValue;
            binding.Format += NegateValue;

            return binding;
        }

        private static void NegateValue(object sender, ConvertEventArgs e)
        {
            e.Value = !(bool)e.Value;
        }
    }
}
