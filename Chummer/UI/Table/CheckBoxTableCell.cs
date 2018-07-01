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

namespace Chummer.UI.Table
{
    public partial class CheckBoxTableCell<T> : TableCell where T : class
    {
        private bool updating = false;

        public CheckBoxTableCell(string text = "", string tag = null)
        {
            InitializeComponent();
            contentField = _checkBox;
            _checkBox.Text = text;
            _checkBox.Tag = tag;
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            Size = _checkBox.Size;
        }

        protected internal override void UpdateValue(object newValue)
        {
            base.UpdateValue(newValue);
            T tValue = newValue as T;
            if (VisibleExtractor != null) {
                _checkBox.Visible = VisibleExtractor(tValue);
            }
            if (!updating && ValueGetter != null)
            {
                updating = true;
                _checkBox.Checked = ValueGetter(tValue);
                updating = false;
            }
        }

        public Func<T, bool> VisibleExtractor { get; set; }

        /// <summary>
        /// The extractor for getting the checked state from the
        /// value.
        /// </summary>
        public Func<T, bool> ValueGetter { get; set; }

        /// <summary>
        /// Updater handling the change of the checked state
        /// of the checkbox.
        /// </summary>
        public Action<T, bool> ValueUpdater { get; set; }

        private void checked_changed(object sender, EventArgs e)
        {
            if (!updating && ValueUpdater != null)
            {
                updating = true;
                ValueUpdater(Value as T, _checkBox.Checked);
                updating = false;
            }
        }

    }
}
