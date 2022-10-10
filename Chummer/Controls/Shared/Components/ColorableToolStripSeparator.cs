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

namespace Chummer
{
    public class ColorableToolStripSeparator : ToolStripSeparator
    {
        public ColorableToolStripSeparator()
        {
            Disposed += OnDisposed;
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            _objBackColorBrush?.Dispose();
            _objForeColorPen?.Dispose();
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (DefaultColorScheme || !Visible)
                return;
            // Get the separator's width and height.
            int intWidth = Width;
            if (intWidth <= 4)
                return;
            int intHeight = Height;
            if (intHeight <= 0)
                return;
            // Fill the background.
            if (_objBackColorBrush == null)
                _objBackColorBrush = new SolidBrush(BackColor);
            e.Graphics.FillRectangle(_objBackColorBrush, 0, 0, intWidth, intHeight);
            // Draw the line.
            if (_objForeColorPen == null)
                _objForeColorPen = new Pen(ForeColor);
            int intMargin = (4 * e.Graphics.DpiX / 96.0f).StandardRound();
            e.Graphics.DrawLine(_objForeColorPen, intMargin, intHeight / 2, intWidth - intMargin, intHeight / 2);
        }

        private bool _blnDefaultColorScheme = ColorManager.IsLightMode;
        private Color _objBackColor;
        private Color _objForeColor;
        private SolidBrush _objBackColorBrush;
        private Pen _objForeColorPen;

        public bool DefaultColorScheme
        {
            get => _blnDefaultColorScheme;
            set
            {
                if (_blnDefaultColorScheme == value)
                    return;
                _blnDefaultColorScheme = value;
                Invalidate();
            }
        }

        /// <inheritdoc />
        public override Color BackColor
        {
            get => _objBackColor;
            set
            {
                if (_objBackColor == value)
                    return;
                _objBackColor = value;
                _objBackColorBrush?.Dispose();
                _objBackColorBrush = new SolidBrush(value);
            }
        }

        /// <inheritdoc />
        public override Color ForeColor
        {
            get => _objForeColor;
            set
            {
                if (_objForeColor == value)
                    return;
                _objForeColor = value;
                _objForeColorPen?.Dispose();
                _objForeColorPen = new Pen(value);
            }
        }
    }
}
