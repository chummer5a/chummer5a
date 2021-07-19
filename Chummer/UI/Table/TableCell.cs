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

using System.Drawing;
using System.Windows.Forms;

namespace Chummer.UI.Table
{
    public partial class TableCell : UserControl
    {
        protected Control contentField;

        public TableCell(Control content = null)
        {
            contentField = content;
            InitializeComponent();
            Alignment = Alignment.Left;
        }

        /// <summary>
        /// Alignment of the content
        /// </summary>
        public Alignment Alignment { get; set; }

        public object Value { get; private set; }

        /// <summary>
        /// called when a item is updated
        /// </summary>
        /// <param name="newValue">the extracted value, if there is a extractor in the column,
        /// the associated item otherwise</param>
        protected internal virtual void UpdateValue(object newValue)
        {
            Value = newValue;
        }

        /// <summary>
        /// Apply layout depending on alignment.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected internal virtual void UpdateAvailableSize(int width, int height)
        {
            if (contentField == null) return;
            Size size = contentField.Size;
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
            contentField.Location = new Point(x, y);
            Size = new Size(x + size.Width, y + size.Height);
            ResumeLayout(false);
            //Invalidate();
        }

        internal Control Content => contentField;
    }
}
