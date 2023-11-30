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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer.UI.Table
{
    public partial class CheckBoxTableCell<T> : TableCell where T : class
    {
        private int _intUpdating;

        public CheckBoxTableCell(string text = "", string tag = null)
        {
            InitializeComponent();
            ContentField = _checkBox;
            _checkBox.Text = text;
            _checkBox.Tag = tag;
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            Size = _checkBox.Size;
        }

        protected internal override void UpdateValue(object newValue)
        {
            base.UpdateValue(newValue);
            T tValue = newValue as T;
            if (VisibleExtractor != null)
            {
                _checkBox.Visible = Utils.SafelyRunSynchronously(() => VisibleExtractor(tValue, CancellationToken.None));
            }

            if (EnabledExtractor != null)
            {
                _checkBox.Enabled = Utils.SafelyRunSynchronously(() => EnabledExtractor(Value as T, CancellationToken.None));
            }
            if (ValueUpdater == null)
                return;
            if (Interlocked.CompareExchange(ref _intUpdating, 1, 0) != 0)
                return;
            try
            {
                _checkBox.Checked = Utils.SafelyRunSynchronously(() => ValueGetter(tValue, CancellationToken.None));
            }
            finally
            {
                Interlocked.CompareExchange(ref _intUpdating, 0, 1);
            }
        }

        protected internal override async Task UpdateValueAsync(object newValue, CancellationToken token = default)
        {
            await base.UpdateValueAsync(newValue, token).ConfigureAwait(false);

            T tValue = newValue as T;
            bool blnDoVisible = VisibleExtractor != null;
            bool blnDoEnabled = EnabledExtractor != null;
            if (ValueUpdater != null)
            {
                bool blnVisible = blnDoVisible && await VisibleExtractor(tValue, token).ConfigureAwait(false);

                bool blnEnabled = blnDoEnabled && await EnabledExtractor(tValue, token).ConfigureAwait(false);

                if (Interlocked.CompareExchange(ref _intUpdating, 1, 0) != 0)
                {
                    if (blnDoVisible || blnDoEnabled)
                    {
                        await _checkBox.DoThreadSafeAsync(x =>
                        {
                            if (blnDoVisible)
                                x.Visible = blnVisible;
                            if (blnDoEnabled)
                                x.Enabled = blnEnabled;
                        }, token).ConfigureAwait(false);
                    }

                    return;
                }

                try
                {
                    bool blnChecked = await ValueGetter(tValue, token).ConfigureAwait(false);
                    await _checkBox.DoThreadSafeAsync(x =>
                    {
                        if (blnDoVisible)
                            x.Visible = blnVisible;
                        if (blnDoEnabled)
                            x.Enabled = blnEnabled;
                        x.Checked = blnChecked;
                    }, token).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.CompareExchange(ref _intUpdating, 0, 1);
                }
            }
            else if (blnDoEnabled)
            {
                bool blnEnabled = await EnabledExtractor(tValue, token).ConfigureAwait(false);
                if (blnDoVisible)
                {
                    bool blnVisible = await VisibleExtractor(tValue, token).ConfigureAwait(false);
                    await _checkBox.DoThreadSafeAsync(x =>
                    {
                        x.Visible = blnVisible;
                        x.Enabled = blnEnabled;
                    }, token).ConfigureAwait(false);
                }
                else
                    await _checkBox.DoThreadSafeAsync(x => x.Enabled = blnEnabled, token).ConfigureAwait(false);
            }
            else if (blnDoVisible)
            {
                bool blnVisible = await VisibleExtractor(tValue, token).ConfigureAwait(false);
                await _checkBox.DoThreadSafeAsync(x => x.Visible = blnVisible, token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The extractor for getting the enabled state from the
        /// value.
        /// </summary>
        public Func<T, CancellationToken, Task<bool>> EnabledExtractor { get; set; }

        public Func<T, CancellationToken, Task<bool>> EnabledGetter { get; set; }

        /// <summary>
        /// The extractor for getting the checked state from the
        /// value.
        /// </summary>
        public Func<T, CancellationToken, Task<bool>> VisibleExtractor { get; set; }

        public Func<T, CancellationToken, Task<bool>> ValueGetter { get; set; }

        /// <summary>
        /// Updater handling the change of the checked state
        /// of the checkbox.
        /// </summary>
        public Func<T, bool, Task> ValueUpdater { get; set; }

        private async void checked_changed(object sender, EventArgs e)
        {
            if (ValueUpdater == null)
                return;
            if (Interlocked.CompareExchange(ref _intUpdating, 1, 0) != 0)
                return;
            try
            {
                await ValueUpdater(Value as T, await _checkBox.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false)).ConfigureAwait(false);
            }
            finally
            {
                Interlocked.CompareExchange(ref _intUpdating, 0, 1);
            }
        }
    }
}
