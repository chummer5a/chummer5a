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
using System.ComponentModel;

namespace Chummer.UI.Table
{
    public partial class SpinnerTableCell<T> : TableCell where T : class, INotifyPropertyChanged
    {
        private bool _blnUpdating;

        public SpinnerTableCell(TableView<T> table)
        {
            InitializeComponent();
            contentField = _spinner;
            Enter += (a, b) => table.PauseSort(this);
            Leave += (a, b) => table.ResumeSort(this);
        }

        private void OnLoad(object sender, EventArgs eventArgs)
        {
            MinimumSize = _spinner.Size;
        }

        protected internal override void UpdateValue(object newValue)
        {
            base.UpdateValue(newValue);
            T tValue = newValue as T;
            if (MinExtractor != null)
            {
                _spinner.Minimum = MinExtractor(tValue);
            }
            if (MaxExtractor != null)
            {
                _spinner.Maximum = MaxExtractor(tValue);
            }
            if (EnabledExtractor != null)
            {
                _spinner.Enabled = EnabledExtractor(tValue);
            }

            if (!_blnUpdating && ValueGetter != null)
            {
                decimal value = Convert.ToDecimal(ValueGetter(tValue));

                _blnUpdating = true;
                _spinner.Value = value;
                _blnUpdating = false;
            }
        }

        public Func<T, bool> EnabledExtractor { get; set; }

        /// <summary>
        /// The extractor extracting the minimum value for the spinner
        /// form the value.
        /// </summary>
        public Func<T, decimal> MinExtractor { get; set; }

        /// <summary>
        /// The extractor to extract the maximum value for the spinner
        /// from the value.
        /// </summary>
        public Func<T, decimal> MaxExtractor { get; set; }

        /// <summary>
        /// The extractor for the property displayed in the spinner.
        /// </summary>
        public Func<T, decimal> ValueGetter { get; set; }

        /// <summary>
        /// The extractor for the property displayed in the spinner.
        /// </summary>
        public Action<T, decimal> ValueUpdater { get; set; }

        /// <summary>
        /// spinner value change handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void value_changed(object sender, EventArgs e)
        {
            if (_blnUpdating || ValueUpdater == null) return;
            _blnUpdating = true;
            try
            {
                ValueUpdater(Value as T, _spinner.Value);
            }
            finally
            {
                _blnUpdating = false;
            }

        }
    }
}
