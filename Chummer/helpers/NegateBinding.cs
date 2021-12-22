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
using System.Windows.Forms;

namespace Chummer
{
    /// <summary>
    /// Helper class that allows inverting the state of a property for databinding.
    /// </summary>
    internal sealed class NegatableBinding : Binding
    {
        /// <inheritdoc />
        public NegatableBinding(string propertyName, object dataSource, string dataMember) : base(propertyName, dataSource, dataMember)
        {
            ConstructorCommon();
        }

        /// <inheritdoc />
        public NegatableBinding(string propertyName, object dataSource, string dataMember, bool formattingEnabled) : base(propertyName, dataSource, dataMember, formattingEnabled)
        {
            ConstructorCommon();
        }

        /// <inheritdoc />
        public NegatableBinding(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode) : base(propertyName, dataSource, dataMember, formattingEnabled, dataSourceUpdateMode)
        {
            ConstructorCommon();
        }

        /// <inheritdoc />
        public NegatableBinding(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode, object nullValue) : base(propertyName, dataSource, dataMember, formattingEnabled, dataSourceUpdateMode, nullValue)
        {
            ConstructorCommon();
        }

        /// <inheritdoc />
        public NegatableBinding(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode, object nullValue, string formatString) : base(propertyName, dataSource, dataMember, formattingEnabled, dataSourceUpdateMode, nullValue, formatString)
        {
            ConstructorCommon();
        }

        /// <inheritdoc />
        public NegatableBinding(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode, object nullValue, string formatString, IFormatProvider formatInfo) : base(propertyName, dataSource, dataMember, formattingEnabled, dataSourceUpdateMode, nullValue, formatString, formatInfo)
        {
            ConstructorCommon();
        }

        private void ConstructorCommon()
        {
            Parse += NegateValue;
            Format += NegateValue;

            void NegateValue(object sender, ConvertEventArgs e)
            {
                e.Value = !(bool)e.Value;
            }
        }
    }
}
