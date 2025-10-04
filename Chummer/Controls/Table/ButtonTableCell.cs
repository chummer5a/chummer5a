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
using System.Windows.Forms;

namespace Chummer.UI.Table
{
    public partial class ButtonTableCell<T> : TableCell where T : class
    {
        private readonly ButtonBase _button;
        private readonly CancellationToken _objMyToken;

        public ButtonTableCell(ButtonBase button, CancellationToken objMyToken = default) : base(button)
        {
            _objMyToken = objMyToken;
            InitializeComponent();
            _button = button ?? throw new ArgumentNullException(nameof(button));
            ContentField = _button;
            button.Click += OnButtonClick;
            SuspendLayout();
            try
            {
                Controls.Add(button);
                this.UpdateLightDarkMode(objMyToken);
                this.TranslateWinForm(token: objMyToken);

                button.PerformLayout();
            }
            finally
            {
                ResumeLayout(false);
            }
        }

        private readonly DebuggableSemaphoreSlim _objUpdateSemaphore = new DebuggableSemaphoreSlim();

        private async void OnButtonClick(object sender, EventArgs e)
        {
            if (ClickHandler != null)
            {
                try
                {
                    await _objUpdateSemaphore.WaitAsync(_objMyToken).ConfigureAwait(false);
                    try
                    {
                        await ClickHandler.Invoke(Value as T, _objMyToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        _objUpdateSemaphore.Release();
                    }
                }
                catch (ObjectDisposedException)
                {
                    //swallow this
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }
        }

        private void OnLoad(object sender, EventArgs eventArgs)
        {
            MinimumSize = _button.Size;
        }

        protected internal override void UpdateValue(object newValue)
        {
            try
            {
                base.UpdateValue(newValue);

                if (EnabledExtractor != null)
                {
                    _button.Enabled = Utils.SafelyRunSynchronously(() => EnabledExtractor(Value as T, _objMyToken), _objMyToken);
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

            if (EnabledExtractor != null)
            {
                bool blnEnabled = await EnabledExtractor(Value as T, token).ConfigureAwait(false);
                await _button.DoThreadSafeAsync(x => x.Enabled = blnEnabled, token: token).ConfigureAwait(false);
            }
        }

        public Func<T, CancellationToken, Task> ClickHandler { get; set; }

        public Func<T, CancellationToken, Task<bool>> EnabledExtractor { get; set; }
    }
}
