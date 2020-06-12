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
        private int _intArrowSize = 8;
        private int _intArrowPadding = 3;
        private SortOrder _eSortType = SortOrder.None;
        private const int _intLabelPadding = 3;

        public HeaderCell()
        {
            InitializeComponent();
            Sortable = false;
            Layout += (sender, evt) => ResizeControl();
        }

        public override string Text
        {
            get => _lblCellText.Text;
            set
            {
                _lblCellText.Text = value;
                ResizeControl();
            }
        }

        private void ResizeControl()
        {
            SuspendLayout();
            if (Sortable)
            {
                int intArrowSize = 2 * ArrowPadding + ArrowSize;
                int intMinWidth = intArrowSize + _lblCellText.Width + _intLabelPadding;
                int intMinHeight = Math.Max(_lblCellText.Height + 2 * _intLabelPadding, intArrowSize);
                MinimumSize = new Size(intMinWidth, intMinHeight);
            }
            else
            {
                MinimumSize = new Size(_lblCellText.Width + _intLabelPadding, _lblCellText.Height + 2 * _intLabelPadding);
            }

            _lblCellText.Location = new Point(_intLabelPadding, (MinimumSize.Height - _lblCellText.Height) / 2);

            ResumeLayout(false);
            Invalidate();
        }

        private int ArrowSize
        {
            get => _intArrowSize;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(ArrowSize));
                }
                _intArrowSize = value;
                ResizeControl();
            }
        }

        private int ArrowPadding
        {
            get => _intArrowPadding;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(ArrowPadding));
                }
                _intArrowPadding = value;
                ResizeControl();
            }
        }

        public SortOrder SortType
        {
            get => _eSortType;
            set {
                _eSortType = value;
                Invalidate();
            }
        }

        public object TextTag {
            get => _lblCellText.Tag;
            set => _lblCellText.Tag = value;
        }

        internal bool Sortable { get; set; }

        public virtual event EventHandler HeaderClick {
            add
            {
                Click += value;
                _lblCellText.Click += value;
            }
            remove
            {
                Click -= value;
                _lblCellText.Click -= value;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // add gradient to background
            using (LinearGradientBrush objBrush = new LinearGradientBrush(ClientRectangle, SystemColors.ControlLight, SystemColors.ControlDark, LinearGradientMode.Vertical))
                e.Graphics.FillRectangle(objBrush, ClientRectangle);

            if (Sortable && SortType != SortOrder.None)
            {
                // draw arrow
                int intTipY = ArrowPadding + ArrowSize / 6;
                int intBottomY = (ArrowPadding + ArrowSize) - ArrowSize / 6;
                int intRight = Width - ArrowPadding;
                int intLeft = intRight - ArrowSize;
                int intTipX = intLeft + ArrowSize / 2;

                if (SortType == SortOrder.Descending)
                {
                    // swap top & bottom
                    int intTemp = intTipY;
                    intTipY = intBottomY;
                    intBottomY = intTemp;
                }
                e.Graphics.FillPolygon(Brushes.Black, new [] { new Point(intLeft, intBottomY), new Point(intTipX, intTipY), new Point(intRight, intBottomY) });
            }
            base.OnPaint(e);
        }

        public void Translate()
        {
            this.TranslateWinForm();
        }
    }
}
