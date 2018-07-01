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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Chummer.UI.Table
{
    public partial class HeaderCell : UserControl, ITranslatable
    {
        private int _arrowSize = 8;
        private int _arrowPadding = 3;
        private SortOrder _sortType = SortOrder.None;

        private static readonly SolidBrush _arrowBrush = new SolidBrush(Color.Black);
        private static readonly int _labelPadding = 3;

        public HeaderCell()
        {
            InitializeComponent();
            Sortable = false;
            Layout += (sender, evt) => ResizeControl();
        }

        public override string Text
        {
            get => _label.Text;
            set
            {
                _label.Text = value;
                ResizeControl();
            }
        }

        private void ResizeControl()
        {
            SuspendLayout();
            if (Sortable)
            {
                int arrowSize = 2 * ArrowPadding + ArrowSize;
                int minWidth = arrowSize + _label.Width + _labelPadding;
                int minHeight = Math.Max(_label.Height + 2 * _labelPadding, arrowSize);
                MinimumSize = new Size(minWidth, minHeight);
            }
            else
            {
                MinimumSize = new Size(_label.Width + _labelPadding, _label.Height + 2 * _labelPadding);
            }

            _label.Location = new Point(_labelPadding, (MinimumSize.Height - _label.Height) / 2);

            ResumeLayout(false);
            Invalidate();
        }

        private int ArrowSize
        {
            get => _arrowSize;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _arrowSize = value;
                ResizeControl();
            }
        }

        private int ArrowPadding
        {
            get => _arrowPadding;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _arrowPadding = value;
                ResizeControl();
            }
        }

        public SortOrder SortType
        {
            get => _sortType;
            set {
                _sortType = value;
                Invalidate();
            }
        }

        public object TextTag {
            get => _label.Tag;
            set => _label.Tag = value;
        }

        internal bool Sortable { get; set; }

        public virtual event EventHandler HeaderClick {
            add
            {
                Click += value;
                _label.Click += value;
            }
            remove
            {
                Click -= value;
                _label.Click -= value;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // add gradient to background
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            e.Graphics.FillRectangle(new LinearGradientBrush(rect, SystemColors.ControlLight, SystemColors.ControlDark, LinearGradientMode.Vertical), rect);

            if (Sortable && SortType != SortOrder.None) {
                // draw arrow
                int tipY = ArrowPadding + ArrowSize / 6;
                int bottomY = (ArrowPadding + ArrowSize) - ArrowSize / 6;
                int right = Width - ArrowPadding;
                int left = right - ArrowSize;
                int tipX = left + ArrowSize / 2;

                if (SortType == SortOrder.Descending)
                {
                    // swap top & bottom
                    int temp = tipY;
                    tipY = bottomY;
                    bottomY = temp;
                }
                e.Graphics.FillPolygon(_arrowBrush, new Point[] { new Point(left, bottomY), new Point(tipX, tipY), new Point(right, bottomY) });
            }
            base.OnPaint(e);
        }

        public void Translate()
        {
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
        }
    }
}
