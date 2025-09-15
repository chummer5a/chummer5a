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
using System.Windows.Forms;

namespace Chummer.UI.Table
{
    public partial class HeaderCell : UserControl, ITranslatable
    {
        private int _intArrowSize = 8;
        private int _intArrowPadding = 3;
        private SortOrder _eSortType = SortOrder.None;
        private const int _intLabelPadding = 3;

        public HeaderCell(CancellationToken token = default)
        {
            InitializeComponent();
            this.UpdateLightDarkMode(token: token);
            Sortable = false;
        }

        public override string Text
        {
            get => _lblCellText.DoThreadSafeFunc(x => x.Text);
            set
            {
                _lblCellText.DoThreadSafe(x => x.Text = value);
                ResizeControl();
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            ResizeControl();
            base.OnLayout(e);
        }

        private void ResizeControl()
        {
            this.DoThreadSafe(x => x.SuspendLayout());
            try
            {
                using (Graphics g = this.DoThreadSafeFunc(x => x.CreateGraphics()))
                {
                    if (Sortable)
                    {
                        int intArrowSize = 2 * ArrowPadding + ArrowSize;
                        int intMinWidth = (int)((intArrowSize + _intLabelPadding) * g.DpiX / 96.0f) +
                                          _lblCellText.DoThreadSafeFunc(x => x.Width);
                        int intMinHeight =
                            Math.Max(_lblCellText.DoThreadSafeFunc(x => x.Height) + (int)(2 * _intLabelPadding * g.DpiY / 96.0f),
                                (int)(intArrowSize * g.DpiY / 96.0f));
                        this.DoThreadSafe(x => x.MinimumSize = new Size(intMinWidth, intMinHeight));
                    }
                    else
                    {
                        int intMinWidth = _lblCellText.DoThreadSafeFunc(x => x.Width) + (int)(_intLabelPadding * g.DpiX / 96.0f);
                        int intMinHeight = _lblCellText.DoThreadSafeFunc(x => x.Height) + (int)(2 * _intLabelPadding * g.DpiY / 96.0f);
                        this.DoThreadSafe(x => x.MinimumSize = new Size(intMinWidth, intMinHeight));
                    }

                    // ReSharper disable once AccessToDisposedClosure
                    _lblCellText.DoThreadSafe(x => x.Location = new Point((int)(_intLabelPadding * g.DpiX / 96.0f),
                                                                          (MinimumSize.Height - _lblCellText.Height) / 2));
                }
            }
            finally
            {
                this.DoThreadSafe(x =>
                {
                    x.ResumeLayout(false);
                    x.Invalidate();
                });
            }
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

                if (Interlocked.Exchange(ref _intArrowSize, value) != value)
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

                if (Interlocked.Exchange(ref _intArrowPadding, value) != value)
                    ResizeControl();
            }
        }

        public SortOrder SortType
        {
            get => _eSortType;
            set
            {
                if (InterlockedExtensions.Exchange(ref _eSortType, value) != value)
                    this.DoThreadSafe(x => x.Invalidate());
            }
        }

        public object TextTag
        {
            get => _lblCellText.DoThreadSafeFunc(x => x.Tag);
            set => _lblCellText.DoThreadSafe(x => x.Tag = value);
        }

        internal bool Sortable { get; set; }

        public virtual event EventHandler HeaderClick
        {
            add
            {
                this.DoThreadSafe(x => x.Click += value);
                _lblCellText.DoThreadSafe(x => x.Click += value);
            }
            remove
            {
                this.DoThreadSafe(x => x.Click -= value);
                _lblCellText.DoThreadSafe(x => x.Click -= value);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Sortable && SortType != SortOrder.None)
            {
                // draw arrow
                int intTipY = ArrowPadding + ArrowSize / 6;
                int intBottomY = ArrowPadding + ArrowSize - ArrowSize / 6;
                int intRight = Width - ArrowPadding;
                int intLeft = intRight - ArrowSize;
                int intTipX = intLeft + ArrowSize / 2;

                if (SortType == SortOrder.Descending)
                {
                    // swap top & bottom
                    (intTipY, intBottomY) = (intBottomY, intTipY);
                }
                using (SolidBrush objBrush = new SolidBrush(ColorManager.ControlLightest))
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

        public void Translate(CancellationToken token = default)
        {
            this.DoThreadSafe((x, y) => x.TranslateWinForm(token: y), token);
        }
    }
}
