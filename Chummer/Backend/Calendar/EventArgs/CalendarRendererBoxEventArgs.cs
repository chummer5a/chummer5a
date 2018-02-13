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
    /// Contains information about something's bounds and text to draw on the calendar
    /// </summary>
    public class CalendarRendererBoxEventArgs
        : CalendarRendererEventArgs
    {
        #region Fields
        private Color _backgroundColor;
        private Rectangle _bounds;
        private Font _font;
        private TextFormatFlags _format;
        private string _text;
        private Color _textColor;
        private Size _textSize;
        #endregion

        /// <summary>
        /// Initializes some fields
        /// </summary>
        private CalendarRendererBoxEventArgs()
        {
            this.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            this.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarRendererBoxEventArgs"/> class.
        /// </summary>
        /// <param name="original">The <see cref="WindowsFormsCalendar.CalendarRendererEventArgs"/> instance containing the event data.</param>
        public CalendarRendererBoxEventArgs(CalendarRendererEventArgs original)
            : base(original)
        {
            Font = original.Calendar.Font;
            Format |= TextFormatFlags.Default | TextFormatFlags.WordBreak | TextFormatFlags.PreserveGraphicsClipping;// | TextFormatFlags.WordEllipsis;
            TextColor = SystemColors.ControlText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarRendererBoxEventArgs"/> class.
        /// </summary>
        /// <param name="original">The <see cref="WindowsFormsCalendar.CalendarRendererEventArgs"/> instance containing the event data.</param>
        /// <param name="bounds">The bounds.</param>
        public CalendarRendererBoxEventArgs(CalendarRendererEventArgs original, Rectangle bounds)
            : this(original)
        {
            Bounds = bounds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarRendererBoxEventArgs"/> class.
        /// </summary>
        /// <param name="original">The <see cref="WindowsFormsCalendar.CalendarRendererEventArgs"/> instance containing the event data.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="text">The text.</param>
        public CalendarRendererBoxEventArgs(CalendarRendererEventArgs original, Rectangle bounds, string text)
            : this(original)
        {
            Bounds = bounds;
            Text = text;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarRendererBoxEventArgs"/> class.
        /// </summary>
        /// <param name="original">The <see cref="WindowsFormsCalendar.CalendarRendererEventArgs"/> instance containing the event data.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="text">The text.</param>
        /// <param name="flags">The flags.</param>
        public CalendarRendererBoxEventArgs(CalendarRendererEventArgs original, Rectangle bounds, string text, TextFormatFlags flags)
            : this(original)
        {
            Bounds = bounds;
            Text = text;
            Format |= flags;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarRendererBoxEventArgs"/> class.
        /// </summary>
        /// <param name="original">The <see cref="WindowsFormsCalendar.CalendarRendererEventArgs"/> instance containing the event data.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="text">The text.</param>
        /// <param name="textColor">Color of the text.</param>
        public CalendarRendererBoxEventArgs(CalendarRendererEventArgs original, Rectangle bounds, string text, Color textColor)
            : this(original)
        {
            Bounds = bounds;
            Text = text;
            TextColor = textColor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarRendererBoxEventArgs"/> class.
        /// </summary>
        /// <param name="original">The <see cref="WindowsFormsCalendar.CalendarRendererEventArgs"/> instance containing the event data.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="text">The text.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="flags">The flags.</param>
        public CalendarRendererBoxEventArgs(CalendarRendererEventArgs original, Rectangle bounds, string text, Color textColor, TextFormatFlags flags)
            : this(original)
        {
            Bounds = bounds;
            Text = text;
            TextColor = TextColor;
            Format |= flags;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarRendererBoxEventArgs"/> class.
        /// </summary>
        /// <param name="original">The <see cref="WindowsFormsCalendar.CalendarRendererEventArgs"/> instance containing the event data.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="text">The text.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        public CalendarRendererBoxEventArgs(CalendarRendererEventArgs original, Rectangle bounds, string text, Color textColor, Color backgroundColor)
            : this(original)
        {
            Bounds = bounds;
            Text = text;
            TextColor = TextColor;
            BackgroundColor = backgroundColor;
        }

        #region Props

        /// <summary>
        /// Gets or sets the background color of the text
        /// </summary>
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        /// <summary>
        /// Gets or sets the bounds to draw the text
        /// </summary>
        public Rectangle Bounds
        {
            get { return _bounds; }
            set { _bounds = value; }
        }

        /// <summary>
        /// Gets or sets the font of the text to be rendered
        /// </summary>
        public Font Font
        {
            get { return _font; }
            set { _font = value; _textSize = Size.Empty; }
        }

        /// <summary>
        /// Gets or sets the format to draw the text
        /// </summary>
        public TextFormatFlags Format
        {
            get { return _format; }
            set { _format = value; _textSize = Size.Empty; }
        }

        /// <summary>
        /// Gets or sets the text to draw
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; _textSize = Size.Empty; }
        }

        /// <summary>
        /// Gets the result of measuring the text
        /// </summary>
        public Size TextSize
        {
            get 
            {
                if (_textSize.IsEmpty)
                {
                    _textSize = TextRenderer.MeasureText(Text, Font);
                }
                return _textSize; 
            }
        }


        /// <summary>
        /// Gets or sets the color to draw the text
        /// </summary>
        public Color TextColor
        {
            get { return _textColor; }
            set { _textColor = value; }
        }

        

        #endregion

        #region Methods

        #endregion
    }
}
