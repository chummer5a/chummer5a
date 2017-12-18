/*
	Copyright 2012 Justin LeCheminant

	This file is part of WindowsFormsCalendar.

    indowsFormsCalendar is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    indowsFormsCalendar is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with indowsFormsCalendar.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Chummer
{
    /// <summary>
    /// Holds data about a box of text to be rendered on the month view
    /// </summary>
    public class MonthViewBoxEventArgs
    {
        #region Fields
        private Graphics _graphics;
        private Color _textColor;
        private Color _backgroundColor;
        private string _text;
        private Color _borderColor;
        private Rectangle _bounds;
        private Font _font;
        private TextFormatFlags _TextFlags;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the bounds of the box
        /// </summary>
        public Rectangle Bounds
        {
            get { return _bounds; }
        }

        /// <summary>
        /// Gets or sets the font of the text. If null, default will be used.
        /// </summary>
        public Font Font
        {
            get { return _font; }
            set { _font = value; }
        }

        /// <summary>
        /// Gets or sets the Graphics object where to draw
        /// </summary>
        public Graphics Graphics
        {
            get { return _graphics; }
        }

        /// <summary>
        /// Gets or sets the border color of the box
        /// </summary>
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; }
        }

        /// <summary>
        /// Gets or sets the text of the box
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        /// <summary>
        /// Gets or sets the background color of the box
        /// </summary>
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        /// <summary>
        /// Gets or sets the text color of the box
        /// </summary>
        public Color TextColor
        {
            get { return _textColor; }
            set { _textColor = value; }
        }

        /// <summary>
        /// Gets or sets the flags of the text
        /// </summary>
        public TextFormatFlags TextFlags
        {
            get { return _TextFlags; }
            set { _TextFlags = value; }
        }


        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthViewBoxEventArgs"/> class.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="text">The text.</param>
        /// <param name="textAlign">The text align.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="backColor">Color of the back.</param>
        /// <param name="borderColor">Color of the border.</param>
        internal MonthViewBoxEventArgs( Graphics graphics, Rectangle bounds, string text, StringAlignment textAlign, Color textColor, Color backColor, Color borderColor )
        {
            _graphics = graphics;

            _graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            _graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            _graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            _graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            _bounds = bounds;
            Text = text;
            TextColor = textColor;
            BackgroundColor = backColor;
            BorderColor = borderColor;

            switch( textAlign )
            {
                case StringAlignment.Center:
                    TextFlags |= TextFormatFlags.HorizontalCenter;
                    break;
                
                case StringAlignment.Far:
                    TextFlags |= TextFormatFlags.Right;
                    break;
                
                case StringAlignment.Near:
                    TextFlags |= TextFormatFlags.Left;
                    break;
                
                default:
                    break;

            }

            TextFlags |= TextFormatFlags.VerticalCenter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthViewBoxEventArgs"/> class.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="text">The text.</param>
        /// <param name="textColor">Color of the text.</param>
        internal MonthViewBoxEventArgs( Graphics graphics, Rectangle bounds, string text, Color textColor )
            : this( graphics, bounds, text, StringAlignment.Center, textColor, Color.Empty, Color.Empty )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthViewBoxEventArgs"/> class.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="text">The text.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="backColor">Color of the back.</param>
        internal MonthViewBoxEventArgs( Graphics graphics, Rectangle bounds, string text, Color textColor, Color backColor )
            : this( graphics, bounds, text, StringAlignment.Center, textColor, backColor, Color.Empty )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthViewBoxEventArgs"/> class.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="text">The text.</param>
        /// <param name="textAlign">The text align.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="backColor">Color of the back.</param>
        internal MonthViewBoxEventArgs( Graphics graphics, Rectangle bounds, string text, StringAlignment textAlign, Color textColor, Color backColor )
            : this( graphics, bounds, text, textAlign, textColor, backColor, Color.Empty )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthViewBoxEventArgs"/> class.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="text">The text.</param>
        /// <param name="textAlign">The text align.</param>
        /// <param name="textColor">Color of the text.</param>
        internal MonthViewBoxEventArgs( Graphics graphics, Rectangle bounds, string text, StringAlignment textAlign, Color textColor )
            : this( graphics, bounds, text, textAlign, textColor, Color.Empty, Color.Empty )
        { }

    }
}