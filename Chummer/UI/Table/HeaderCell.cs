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
            this.UpdateLightDarkMode();
            Sortable = false;
            Layout += ResizeControl;
        }

        public override string Text
        {
            get => _lblCellText.Text;
            set
            {
                _lblCellText.Text = value;
                ResizeControl(this, null);
            }
        }

        private void ResizeControl(object sender, LayoutEventArgs e)
        {
            SuspendLayout();
            using (Graphics g = CreateGraphics())
            {
                if (Sortable)
                {
                    int intArrowSize = 2 * ArrowPadding + ArrowSize;
                    int intMinWidth = (int)((intArrowSize + _intLabelPadding) * g.DpiX / 96.0f) +
                                      _lblCellText.Width;
                    int intMinHeight =
                        Math.Max(_lblCellText.Height + (int)(2 * _intLabelPadding * g.DpiY / 96.0f),
                            (int)(intArrowSize * g.DpiY / 96.0f));
                    MinimumSize = new Size(intMinWidth, intMinHeight);
                }
                else
                {
                    int intMinWidth = _lblCellText.Width + (int)(_intLabelPadding * g.DpiX / 96.0f);
                    int intMinHeight = _lblCellText.Height + (int)(2 * _intLabelPadding * g.DpiY / 96.0f);
                    MinimumSize = new Size(intMinWidth, intMinHeight);
                }

                _lblCellText.Location = new Point((int)(_intLabelPadding * g.DpiX / 96.0f),
                    (MinimumSize.Height - _lblCellText.Height) / 2);
            }
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
                ResizeControl(this, null);
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
                ResizeControl(this, null);
            }
        }

        public SortOrder SortType
        {
            get => _eSortType;
            set
            {
                _eSortType = value;
                Invalidate();
            }
        }

        public object TextTag
        {
            get => _lblCellText.Tag;
            set => _lblCellText.Tag = value;
        }

        internal bool Sortable { get; set; }

        public virtual event EventHandler HeaderClick
        {
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
                    (intTipY, intBottomY) = (intBottomY, intTipY);
                }
                using (Brush objBrush = new SolidBrush(ColorManager.ControlLightest))
                    e.Graphics.FillPolygon(objBrush,
                        new[]
                        {
                            new Point(intLeft, intBottomY),
                            new Point(intTipX, intTipY),
                            new Point(intRight, intBottomY)
                        });
            }
            base.OnPaint(e);
        }

        public void Translate()
        {
            this.TranslateWinForm();
        }
    }
}
