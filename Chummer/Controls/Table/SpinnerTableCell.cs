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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer.UI.Table
{
    public partial class SpinnerTableCell<T> : TableCell where T : class, INotifyPropertyChanged
    {
        private int _intUpdating;
        private readonly CancellationToken _objMyToken;
        private readonly DebuggableSemaphoreSlim _objUpdateSemaphore;
        private readonly TableView<T> _objMyTable;

        public SpinnerTableCell(TableView<T> table, CancellationToken objMyToken = default)
        {
            _objUpdateSemaphore = new DebuggableSemaphoreSlim();
            _objMyToken = objMyToken;
            _objMyTable = table;
            InitializeComponent();
            ContentField = nudSpinner;
            Leave += DoResumeSort;

            async void DoResumeSort(object a, EventArgs b)
            {
                try
                {
                    await _objMyTable.ResumeSortAsync(this, _objMyToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            _objMyTable.PauseSort(this);
            base.OnEnter(e);
        }

        private void OnLoad(object sender, EventArgs eventArgs)
        {
            MinimumSize = nudSpinner.Size;
        }

        protected internal override void UpdateValue(object newValue)
        {
            try
            {
                base.UpdateValue(newValue);
                T tValue = newValue as T;
                if (MinExtractor != null)
                {
                    if (MaxExtractor != null)
                    {
                        decimal decMin = Utils.SafelyRunSynchronously(() => MinExtractor(tValue, _objMyToken), _objMyToken);
                        decimal decMax = Utils.SafelyRunSynchronously(() => MaxExtractor(tValue, _objMyToken), _objMyToken);
                        nudSpinner.Minimum = Math.Min(decMin, decMax);
                        nudSpinner.Maximum = Math.Max(decMax, decMin);
                    }
                    else
                        nudSpinner.Minimum = Utils.SafelyRunSynchronously(() => MinExtractor(tValue, _objMyToken), _objMyToken);
                }
                else if (MaxExtractor != null)
                {
                    nudSpinner.Maximum = Utils.SafelyRunSynchronously(() => MaxExtractor(tValue, _objMyToken), _objMyToken);
                }
                if (EnabledExtractor != null)
                {
                    nudSpinner.Enabled = Utils.SafelyRunSynchronously(() => EnabledExtractor(tValue, _objMyToken), _objMyToken);
                }

                if (ValueUpdater == null)
                    return;
                if (Interlocked.CompareExchange(ref _intUpdating, 1, 0) != 0)
                    return;
                try
                {
                    nudSpinner.Value = Utils.SafelyRunSynchronously(() => ValueGetter(tValue, _objMyToken), _objMyToken);
                }
                finally
                {
                    Interlocked.CompareExchange(ref _intUpdating, 0, 1);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        protected internal override async Task UpdateValueAsync(object newValue, CancellationToken token = default)
        {
            await base.UpdateValueAsync(newValue, token).ConfigureAwait(false);

            T tValue = newValue as T;
            bool blnDoMin = MinExtractor != null;
            bool blnDoMax = MaxExtractor != null;
            bool blnDoEnabled = EnabledExtractor != null;
            if (ValueUpdater != null)
            {
                decimal decMin = blnDoMin ? await MinExtractor(tValue, token).ConfigureAwait(false) : decimal.MinValue;
                decimal decMax = blnDoMax ? await MaxExtractor(tValue, token).ConfigureAwait(false) : decimal.MaxValue;
                bool blnEnabled = blnDoEnabled && await EnabledExtractor(tValue, token).ConfigureAwait(false);
                if (Interlocked.CompareExchange(ref _intUpdating, 1, 0) != 0)
                {
                    if (blnDoMin || blnDoMax || blnDoEnabled)
                    {
                        await nudSpinner.DoThreadSafeAsync(x =>
                        {
                            if (blnDoMin)
                                x.Minimum = Math.Min(decMin, decMax);
                            if (blnDoMax)
                                x.Maximum = Math.Max(decMax, decMin);
                            if (blnDoEnabled)
                                x.Enabled = blnEnabled;
                        }, token: token).ConfigureAwait(false);
                    }
                    return;
                }

                try
                {
                    decimal decValue = await ValueGetter(tValue, token).ConfigureAwait(false);
                    await nudSpinner.DoThreadSafeAsync(x =>
                    {
                        if (blnDoMin)
                            x.Minimum = Math.Min(decMin, decMax);
                        if (blnDoMax)
                            x.Maximum = Math.Max(decMax, decMin);
                        if (blnDoEnabled)
                            x.Enabled = blnEnabled;
                        x.Value = decValue;
                    }, token: token).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.CompareExchange(ref _intUpdating, 0, 1);
                }
            }
            else if (blnDoEnabled)
            {
                bool blnEnabled = await EnabledExtractor(tValue, token).ConfigureAwait(false);
                if (blnDoMax)
                {
                    decimal decMax = await MaxExtractor(tValue, token).ConfigureAwait(false);
                    if (blnDoMin)
                    {
                        decimal decMin = await MinExtractor(tValue, token).ConfigureAwait(false);
                        await nudSpinner.DoThreadSafeAsync(x =>
                        {
                            x.Minimum = Math.Min(decMin, decMax);
                            x.Maximum = Math.Max(decMax, decMin);
                            x.Enabled = blnEnabled;
                        }, token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        await nudSpinner.DoThreadSafeAsync(x =>
                        {
                            x.Maximum = decMax;
                            x.Enabled = blnEnabled;
                        }, token: token).ConfigureAwait(false);
                    }
                }
                else if (blnDoMin)
                {
                    decimal decMin = await MinExtractor(tValue, token).ConfigureAwait(false);
                    await nudSpinner.DoThreadSafeAsync(x =>
                    {
                        x.Minimum = decMin;
                        x.Enabled = blnEnabled;
                    }, token: token).ConfigureAwait(false);
                }
                else
                    await nudSpinner.DoThreadSafeAsync(x => x.Enabled = blnEnabled, token: token).ConfigureAwait(false);
            }
            else if (blnDoMax)
            {
                decimal decMax = await MaxExtractor(tValue, token).ConfigureAwait(false);
                if (blnDoMin)
                {
                    decimal decMin = await MinExtractor(tValue, token).ConfigureAwait(false);
                    await nudSpinner.DoThreadSafeAsync(x =>
                    {
                        x.Minimum = Math.Min(decMin, decMax);
                        x.Maximum = Math.Max(decMax, decMin);
                    }, token: token).ConfigureAwait(false);
                }
                else
                    await nudSpinner.DoThreadSafeAsync(x => x.Maximum = decMax, token: token).ConfigureAwait(false);
            }
            else if (blnDoMin)
            {
                decimal decMin = await MinExtractor(tValue, token).ConfigureAwait(false);
                await nudSpinner.DoThreadSafeAsync(x => x.Minimum = decMin, token: token).ConfigureAwait(false);
            }
        }

        public Func<T, CancellationToken, Task<bool>> EnabledExtractor { get; set; }

        /// <summary>
        /// The extractor extracting the minimum value for the spinner
        /// form the value.
        /// </summary>
        public Func<T, CancellationToken, Task<decimal>> MinExtractor { get; set; }

        /// <summary>
        /// The extractor to extract the maximum value for the spinner
        /// from the value.
        /// </summary>
        public Func<T, CancellationToken, Task<decimal>> MaxExtractor { get; set; }

        /// <summary>
        /// The extractor for the property displayed in the spinner.
        /// </summary>
        public Func<T, CancellationToken, Task<decimal>> ValueGetter { get; set; }

        /// <summary>
        /// The extractor for the property displayed in the spinner.
        /// </summary>
        public Func<T, decimal, CancellationToken, Task> ValueUpdater { get; set; }

        /// <summary>
        /// spinner value change handler
        /// </summary>
        private async void nudSpinner_ValueChanged(object sender, EventArgs e)
        {
            if (ValueUpdater == null)
                return;
            try
            {
                await _objUpdateSemaphore.WaitAsync(_objMyToken).ConfigureAwait(false);
                try
                {
                    if (Interlocked.CompareExchange(ref _intUpdating, 1, 0) != 0)
                        return;
                    try
                    {
                        await ValueUpdater(Value as T,
                                await nudSpinner.DoThreadSafeFuncAsync(x => x.Value, _objMyToken).ConfigureAwait(false), _objMyToken)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.CompareExchange(ref _intUpdating, 0, 1);
                    }
                }
                finally
                {
                    _objUpdateSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }
    }
}
