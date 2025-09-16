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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    public class CheckBoxWithToolTip : CheckBox, IControlWithToolTip
    {
        private readonly int _intToolTipWrap;

        private ToolTip _objToolTip;

        public ToolTip ToolTipObject => _objToolTip;

        private string _strToolTipText = string.Empty;

        public string ToolTipText
        {
            get => _strToolTipText;
            set
            {
                value = _intToolTipWrap > 0 ? value.WordWrap(_intToolTipWrap) : value.WordWrap();
                if (Interlocked.Exchange(ref _strToolTipText, value) == value)
                    return;
                this.DoThreadSafe(x => _objToolTip?.SetToolTip(x, value.CleanForHtml()));
            }
        }

        public Task SetToolTipTextAsync(string value, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            value = _intToolTipWrap > 0 ? value.WordWrap(_intToolTipWrap) : value.WordWrap();
            return Interlocked.Exchange(ref _strToolTipText, value) != value
                ? this.DoThreadSafeAsync(x => _objToolTip?.SetToolTip(x, value.CleanForHtml()), token: token)
                : Task.CompletedTask;
        }

        public CheckBoxWithToolTip(int intToolTipWrap = -1) : base()
        {
            _intToolTipWrap = intToolTipWrap;
            _frmParent = FindForm();
            _objToolTip = ToolTipFactory.GetToolTipForForm(_frmParent);
        }

        private Form _frmParent;

        public void UpdateToolTipParent()
        {
            if (Interlocked.Exchange(ref _frmParent, FindForm()) != _frmParent)
            {
                ToolTip objOldToolTip = Interlocked.Exchange(ref _objToolTip, ToolTipFactory.GetToolTipForForm(_frmParent));
                if (objOldToolTip != null || !string.IsNullOrEmpty(_strToolTipText))
                    _objToolTip?.SetToolTip(this, _strToolTipText.CleanForHtml());
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            // Note: because we cannot unsubscribe old parents from events if/when we change parents, we do not want to have this automatically update
            // based on a subscription to our parent's ParentChanged (which we would need to be able to automatically update our parent form for nested controls)
            // We therefore need to use the hacky workaround of calling UpdateParentForToolTipControls() for parent forms/controls as appropriate
            UpdateToolTipParent();
        }
    }
}
