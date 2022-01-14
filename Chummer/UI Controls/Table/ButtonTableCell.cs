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

namespace Chummer.UI.Table
{
    public partial class ButtonTableCell<T> : TableCell where T : class
    {
        private readonly Control _button;

        public ButtonTableCell(Control button) : base(button)
        {
            InitializeComponent();
            _button = button ?? throw new ArgumentNullException(nameof(button));
            button.Click += ((sender, evt) => ClickHandler?.Invoke(Value as T));
            SuspendLayout();
            Controls.Add(button);
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            button.PerformLayout();
            ResumeLayout(false);
        }

        private void OnLoad(object sender, EventArgs eventArgs)
        {
            MinimumSize = _button.Size;
        }

        protected internal override void UpdateValue(object newValue)
        {
            base.UpdateValue(newValue);

            if (EnabledExtractor != null)
            {
                _button.Enabled = EnabledExtractor(Value as T);
            }
        }

        public Action<T> ClickHandler { get; set; }

        public Func<T, bool> EnabledExtractor { get; set; }
    }
}
