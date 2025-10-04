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
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer.UI.Table
{
    public partial class TableCell : UserControl
    {
        protected Control ContentField;

        public TableCell(Control content = null)
        {
            ContentField = content;
            InitializeComponent();
            Alignment = Alignment.Left;
        }

        /// <summary>
        /// Alignment of the content
        /// </summary>
        public Alignment Alignment { get; set; }

        public object Value { get; private set; }

        /// <summary>
        /// Called when a item is updated
        /// </summary>
        /// <param name="newValue">the extracted value, if there is a extractor in the column, the associated item otherwise</param>
        protected internal virtual void UpdateValue(object newValue)
        {
            Value = newValue;
        }

        /// <summary>
        /// Called when a item is updated
        /// </summary>
        /// <param name="newValue">the extracted value, if there is a extractor in the column, the associated item otherwise</param>
        /// <param name="token">Cancellation token to listen to.</param>
        protected internal virtual Task UpdateValueAsync(object newValue, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            Value = newValue;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Apply layout depending on alignment.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected internal virtual void UpdateAvailableSize(int width, int height)
        {
            if (ContentField == null)
                return;
            Size size = ContentField.Size;
            int x;
            if ((Alignment & Alignment.Left) != 0)
            {
                x = 0;
            }
            else
            {
                x = width - size.Width;
                if ((Alignment & Alignment.Right) == 0)
                {
                    x /= 2;
                }
            }

            int y;
            if ((Alignment & Alignment.Top) != 0)
            {
                y = 0;
            }
            else
            {
                y = height - size.Height;
                if ((Alignment & Alignment.Bottom) == 0)
                {
                    y /= 2;
                }
            }

            SuspendLayout();
            try
            {
                ContentField.Location = new Point(x, y);
                Size = new Size(x + size.Width, y + size.Height);
            }
            finally
            {
                ResumeLayout(false);
            }
            //Invalidate();
        }

        internal Control Content => ContentField;

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            // Note: because we cannot unsubscribe old parents from events if/when we change parents, we do not want to have this automatically update
            // based on a subscription to our parent's ParentChanged (which we would need to be able to automatically update our parent form for nested controls)
            // We therefore need to use the hacky workaround of calling UpdateParentForToolTipControls() for parent forms/controls as appropriate
            this.UpdateParentForToolTipControls();
        }
    }
}
