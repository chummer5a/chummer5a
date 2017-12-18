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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Chummer
{
    /// <summary>
    /// Hosts a month-level calendar where user can select day-based dates
    /// </summary>
    [DefaultEvent("SelectionChanged")]
    public class MonthView
        : ContainerControl
    {
        #region Fields
        private int _forwardMonthIndex;
        private MonthViewDay _lastHitted;
        private bool _mouseDown;
        private Size _daySize;
        private DateTime _selectionStart;
        private DateTime _selectionEnd;
        private string _monthTitleFormat;
        private DayOfWeek _weekStart;
        private DayOfWeek _workWeekStart;
        private DayOfWeek _workWeekEnd;
        private MonthViewSelection _selectionMode;
        private string _dayNamesFormat;
        private bool _dayNamesVisible;
        private int _dayNamesLength;
        private DateTime _viewStart;
        private Size _monthSize;
        private MonthViewMonth[] _months;
        private Padding _itemPadding;
        private Color _monthTitleColor;
        private Color _monthTitleColorInactive;
        private Color _monthTitleTextColor;
        private Color _monthTitleTextColorInactive;
        private Color _dayBackgroundColor;
        private Color _daySelectedBackgroundColor;
        private Color _dayTextColor;
        private Color _daySelectedTextColor;
        private Color _arrowsColor;
        private Color _arrowsSelectedColor;
        private Color _dayGrayedText;
        private Color _todayBorderColor;
        private int _maxSelectionCount;
        private Rectangle _forwardButtonBounds;
        private bool _forwardButtonSelected;
        private Rectangle _backwardButtonBounds;
        private bool _backwardButtonSelected;
        #endregion

        #region Events

        /// <summary>
        /// Occurs when selection has changed.
        /// </summary>
        public event EventHandler SelectionChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the size of days rectangles
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public Size DaySize
        {
            get { return _daySize; }
        }

        /// <summary>
        /// Gets or sets the format of day names
        /// </summary>
        [DefaultValue( "ddd" )]
        public string DayNamesFormat
        {
            get { return _dayNamesFormat; }
            set { _dayNamesFormat = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if day names should be visible
        /// </summary>
        [DefaultValue( true )]
        public bool DayNamesVisible
        {
            get { return _dayNamesVisible; }
            set { _dayNamesVisible = value; }
        }

        /// <summary>
        /// Gets or sets how many characters of day names should be displayed
        /// </summary>
        [DefaultValue( 2 )]
        public int DayNamesLength
        {
            get { return _dayNamesLength; }
            set { _dayNamesLength = value; UpdateMonths(); }
        }

        /// <summary>
        /// Gets or sets what the first day of weeks should be
        /// </summary>
        [DefaultValue( DayOfWeek.Sunday )]
        public DayOfWeek FirstDayOfWeek
        {
            get { return _weekStart; }
            set { _weekStart = value; }
        }

        /// <summary>
        /// Gets a value indicating if the backward button is selected
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public bool BackwardButtonSelected
        {
            get { return _backwardButtonSelected; }
        }

        /// <summary>
        /// Gets the bounds of the backward button
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public Rectangle BackwardButtonBounds
        {
            get { return _backwardButtonBounds; }
        }

        /// <summary>
        /// Gets a value indicating if the forward button is selected
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public bool ForwardButtonSelected
        {
            get { return _forwardButtonSelected; }
        }

        /// <summary>
        /// Gets the bounds of the forward button
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public Rectangle ForwardButtonBounds
        {
            get { return _forwardButtonBounds; }
        }

        /// <summary>
        /// Gets or sets the Font of the Control
        /// </summary>
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;

                UpdateMonthSize();
                UpdateMonths();
            }
        }

        /// <summary>
        /// Gets or sets the internal padding of items (Days, day names, month names)
        /// </summary>
        public Padding ItemPadding
        {
            get { return _itemPadding; }
            set { _itemPadding = value; }
        }

        /// <summary>
        /// Gets or sets the maximum selection count of days
        /// </summary>
        [DefaultValue( 0 )]
        public int MaxSelectionCount
        {
            get { return _maxSelectionCount; }
            set { _maxSelectionCount = value; }
        }

        /// <summary>
        /// Gets the Months currently displayed on the calendar
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public MonthViewMonth[] Months
        {
            get { return _months; }
        }

        /// <summary>
        /// Gets the size of an entire month inside the <see cref="MonthView"/>
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public Size MonthSize
        {
            get { return _monthSize; }
        }

        /// <summary>
        /// Gets or sets the format of month titles
        /// </summary>
        [DefaultValue( "MMMM yyyy" )]
        public string MonthTitleFormat
        {
            get { return _monthTitleFormat; }
            set { _monthTitleFormat = value; UpdateMonths(); }
        }

        /// <summary>
        /// Gets or sets the start of selection
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public DateTime SelectionStart
        {
            get { return _selectionStart; }
            set
            {
                if( MaxSelectionCount > 0 )
                {
                    if( Math.Abs( value.Subtract( SelectionEnd ).TotalDays ) >= MaxSelectionCount )
                    {
                        return;
                    }
                }

                _selectionStart = value;
                Invalidate();
                OnSelectionChanged( EventArgs.Empty );
            }
        }

        /// <summary>
        /// Gets or sets the end of selection
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public DateTime SelectionEnd
        {
            get { return _selectionEnd; }
            set
            {
                if( MaxSelectionCount > 0 )
                {
                    if( Math.Abs( value.Subtract( SelectionStart ).TotalDays ) >= MaxSelectionCount )
                    {
                        return;
                    }
                }

                _selectionEnd = value.Date.Add( new TimeSpan( 23, 59, 59 ) );
                Invalidate();
                OnSelectionChanged( EventArgs.Empty );
            }
        }

        /// <summary>
        /// Gets or sets the selection mode of <see cref="MonthView"/>
        /// </summary>
        [DefaultValue( MonthViewSelection.Manual )]
        public MonthViewSelection SelectionMode
        {
            get { return _selectionMode; }
            set { _selectionMode = value; }
        }

        /// <summary>
        /// Gets or sets the date of the first displayed month
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public DateTime ViewStart
        {
            get { return _viewStart; }
            set { _viewStart = value; UpdateMonths(); Invalidate(); }
        }

        /// <summary>
        /// Gets the last day of the last month showed on the view.
        /// </summary>
        public DateTime ViewEnd
        {
            get
            {
                DateTime month = Months[Months.Length - 1].Date;
                return month.Date.AddDays( DateTime.DaysInMonth( month.Year, month.Month ) );
            }
        }

        /// <summary>
        /// Gets or sets the day that starts a work-week
        /// </summary>
        [DefaultValue( DayOfWeek.Monday )]
        public DayOfWeek WorkWeekStart
        {
            get { return _workWeekStart; }
            set { _workWeekStart = value; }
        }

        /// <summary>
        /// Gets or sets the day that ends a work-week
        /// </summary>
        [DefaultValue( DayOfWeek.Friday )]
        public DayOfWeek WorkWeekEnd
        {
            get { return _workWeekEnd; }
            set { _workWeekEnd = value; }
        }

        #endregion

        #region Color Properties

        /// <summary>
        /// Gets or sets the color of the arrows selected.
        /// </summary>
        /// <value>
        /// The color of the arrows selected.
        /// </value>
        public Color ArrowsSelectedColor
        {
            get { return _arrowsSelectedColor; }
            set { _arrowsSelectedColor = value; }
        }

        /// <summary>
        /// Gets or sets the color of the arrows.
        /// </summary>
        /// <value>
        /// The color of the arrows.
        /// </value>
        public Color ArrowsColor
        {
            get { return _arrowsColor; }
            set { _arrowsColor = value; }
        }

        /// <summary>
        /// Gets or sets the color of the day selected text.
        /// </summary>
        /// <value>
        /// The color of the day selected text.
        /// </value>
        public Color DaySelectedTextColor
        {
            get { return _daySelectedTextColor; }
            set { _daySelectedTextColor = value; }
        }

        /// <summary>
        /// Gets or sets the color of the day selected.
        /// </summary>
        /// <value>
        /// The color of the day selected.
        /// </value>
        public Color DaySelectedColor
        {
            get { return _dayTextColor; }
            set { _dayTextColor = value; }
        }

        /// <summary>
        /// Gets or sets the color of the day selected background.
        /// </summary>
        /// <value>
        /// The color of the day selected background.
        /// </value>
        public Color DaySelectedBackgroundColor
        {
            get { return _daySelectedBackgroundColor; }
            set { _daySelectedBackgroundColor = value; }
        }

        /// <summary>
        /// Gets or sets the color of the day background.
        /// </summary>
        /// <value>
        /// The color of the day background.
        /// </value>
        public Color DayBackgroundColor
        {
            get { return _dayBackgroundColor; }
            set { _dayBackgroundColor = value; }
        }

        /// <summary>
        /// Gets or sets the day grayed text.
        /// </summary>
        /// <value>
        /// The day grayed text.
        /// </value>
        public Color DayGrayedText
        {
            get { return _dayGrayedText; }
            set { _dayGrayedText = value; }
        }

        /// <summary>
        /// Gets or sets the color of the month title.
        /// </summary>
        /// <value>
        /// The color of the month title.
        /// </value>
        public Color MonthTitleColor
        {
            get { return _monthTitleColor; }
            set { _monthTitleColor = value; }
        }

        /// <summary>
        /// Gets or sets the month title text color inactive.
        /// </summary>
        /// <value>
        /// The month title text color inactive.
        /// </value>
        public Color MonthTitleTextColorInactive
        {
            get { return _monthTitleTextColorInactive; }
            set { _monthTitleTextColorInactive = value; }
        }

        /// <summary>
        /// Gets or sets the color of the month title text.
        /// </summary>
        /// <value>
        /// The color of the month title text.
        /// </value>
        public Color MonthTitleTextColor
        {
            get { return _monthTitleTextColor; }
            set { _monthTitleTextColor = value; }
        }

        /// <summary>
        /// Gets or sets the month title color inactive.
        /// </summary>
        /// <value>
        /// The month title color inactive.
        /// </value>
        public Color MonthTitleColorInactive
        {
            get { return _monthTitleColorInactive; }
            set { _monthTitleColorInactive = value; }
        }

        /// <summary>
        /// Gets or sets the color of the today day border color
        /// </summary>
        public Color TodayBorderColor
        {
            get { return _todayBorderColor; }
            set { _todayBorderColor = value; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthView"/> class.
        /// </summary>
        public MonthView()
        {
            SetStyle(ControlStyles.Opaque, true);
            DoubleBuffered = true;

            _dayNamesFormat = "ddd";
            _monthTitleFormat = "MMMM yyyy";
            _selectionMode = MonthViewSelection.Manual;
            _workWeekStart = DayOfWeek.Monday;
            _workWeekEnd = DayOfWeek.Friday;
            _weekStart = DayOfWeek.Sunday;
            _dayNamesVisible = true;
            _dayNamesLength = 2;
            _viewStart = DateTime.Now;
            _itemPadding = new Padding(2);
            _monthTitleColor = SystemColors.ActiveCaption;
            _monthTitleColorInactive = SystemColors.InactiveCaption;
            _monthTitleTextColor = SystemColors.ActiveCaptionText;
            _monthTitleTextColorInactive = SystemColors.InactiveCaptionText;
            _dayBackgroundColor = Color.Empty;
            _daySelectedBackgroundColor = SystemColors.Highlight;
            _dayTextColor = SystemColors.WindowText;
            _daySelectedTextColor = SystemColors.HighlightText;
            _arrowsColor = SystemColors.Window;
            _arrowsSelectedColor = Color.Gold;
            _dayGrayedText = SystemColors.GrayText;
            _todayBorderColor = Color.Maroon;

            UpdateMonthSize();
            UpdateMonths();
        }

        #region Public Methods

        /// <summary>
        /// Checks if a day is hitted on the specified point
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public MonthViewDay HitTest(Point p)
        {
            for (int i = 0; i < Months.Length; i++)
            {
                if (Months[i].Bounds.Contains(p))
                {
                    for (int j = 0; j < Months[i].Days.Length; j++)
                    {
                        if (/*Months[i].Days[j].Visible && */Months[i].Days[j].Bounds.Contains(p))
                        {
                            return Months[i].Days[j];
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Moves the view one month forward
        /// </summary>
        public void GoForward()
        {
            ViewStart = ViewStart.AddMonths(1);
        }

        /// <summary>
        /// Moves the view one month backward
        /// </summary>
        public void GoBackward()
        {
            ViewStart = ViewStart.AddMonths(-1);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the forward button bounds.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        private void SetForwardButtonBounds(Rectangle bounds)
        {
            _forwardButtonBounds = bounds;
        }

        /// <summary>
        /// Sets the backward button bounds.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        private void SetBackwardButtonBounds(Rectangle bounds)
        {
            _backwardButtonBounds = bounds;
        }

        /// <summary>
        /// Sets the forward button selected.
        /// </summary>
        /// <param name="selected">if set to <c>true</c> [selected].</param>
        private void SetForwardButtonSelected(bool selected)
        {
            _forwardButtonSelected = selected;
            Invalidate(ForwardButtonBounds);
        }

        /// <summary>
        /// Sets the backward button selected.
        /// </summary>
        /// <param name="selected">if set to <c>true</c> [selected].</param>
        private void SetBackwardButtonSelected(bool selected)
        {
            _backwardButtonSelected = selected;
            Invalidate(BackwardButtonBounds);
        }

        /// <summary>
        /// Selects the week where the hit is contained
        /// </summary>
        /// <param name="hit"></param>
        private void SelectWeek(DateTime hit)
        {
            int preDays = (new int[] { 0, 1, 2, 3, 4, 5, 6 })[(int)hit.DayOfWeek] - (int)FirstDayOfWeek;

            _selectionStart = hit.AddDays(-preDays);
            SelectionEnd = SelectionStart.AddDays(6);
        }

        /// <summary>
        /// Selecs the work-week where the hit is contanied
        /// </summary>
        /// <param name="hit"></param>
        private void SelectWorkWeek(DateTime hit)
        {
            int preDays = (new int[] { 0, 1, 2, 3, 4, 5, 6 })[(int)hit.DayOfWeek] - (int)WorkWeekStart;

            _selectionStart = hit.AddDays(-preDays);
            SelectionEnd = SelectionStart.AddDays(Math.Abs(WorkWeekStart - WorkWeekEnd));
        }

        /// <summary>
        /// Selecs the month where the hit is contanied
        /// </summary>
        /// <param name="hit"></param>
        private void SelectMonth(DateTime hit)
        {
            _selectionStart = new DateTime(hit.Year, hit.Month, 1);
            SelectionEnd = new DateTime(hit.Year, hit.Month, DateTime.DaysInMonth(hit.Year, hit.Month));
        }

        /// <summary>
        /// Draws a box of text
        /// </summary>
        /// <param name="e"></param>
        private void DrawBox(MonthViewBoxEventArgs e)
        {
            if (!e.BackgroundColor.IsEmpty)
            {
                using (SolidBrush b = new SolidBrush(e.BackgroundColor))
                {
                    e.Graphics.FillRectangle(b, e.Bounds);
                }
            }

            if (!e.TextColor.IsEmpty && !string.IsNullOrEmpty(e.Text))
            {
                TextRenderer.DrawText(e.Graphics, e.Text, e.Font != null ? e.Font : Font, e.Bounds, e.TextColor, e.TextFlags);
            }

            if (!e.BorderColor.IsEmpty)
            {
                using (Pen p = new Pen(e.BorderColor))
                {
                    Rectangle r = e.Bounds;
                    r.Width--; r.Height--;
                    e.Graphics.DrawRectangle(p, r);
                }
            }
        }

        /// <summary>
        /// Updates the size of the month.
        /// </summary>
        private void UpdateMonthSize()
        {
            //One row of day names plus 31 possible numbers
            string[] strs = new string[7 + 31];
            int maxWidth = 0;
            int maxHeight = 0;

            for (int i = 0; i < 7; i++)
            {
                strs[i] = ViewStart.AddDays(i).ToString(DayNamesFormat).Substring(0, DayNamesLength);
            }

            for (int i = 7; i < strs.Length; i++)
            {
                strs[i] = (i - 6).ToString();
            }

            Font f = new Font(Font, FontStyle.Bold);

            for (int i = 0; i < strs.Length; i++)
            {
                Size s = TextRenderer.MeasureText(strs[i], f);
                maxWidth = Math.Max(s.Width, maxWidth);
                maxHeight = Math.Max(s.Height, maxHeight);
            }

            maxWidth += ItemPadding.Horizontal;
            maxHeight += ItemPadding.Vertical;

            _daySize = new Size(maxWidth, maxHeight);
            _monthSize = new Size(maxWidth * 7, maxHeight * 7 + maxHeight * (DayNamesVisible ? 1 : 0));
        }

        /// <summary>
        /// Updates the months.
        /// </summary>
        private void UpdateMonths()
        {
            int gapping = 2;
            int calendarsX = Convert.ToInt32(Math.Max(Math.Floor((double)ClientSize.Width / (double)(MonthSize.Width + gapping)), 1.0));
            int calendarsY = Convert.ToInt32(Math.Max(Math.Floor((double)ClientSize.Height / (double)(MonthSize.Height + gapping)), 1.0));
            int calendars = calendarsX * calendarsY;
            int monthsWidth = (calendarsX * MonthSize.Width) + (calendarsX - 1) * gapping;
            int monthsHeight = (calendarsY * MonthSize.Height) + (calendarsY - 1) * gapping;
            int startX = (ClientSize.Width - monthsWidth) / 2;
            int startY = (ClientSize.Height - monthsHeight) / 2;
            int curX = startX;
            int curY = startY;
            _forwardMonthIndex = calendarsX - 1;

            _months = new MonthViewMonth[calendars];

            for (int i = 0; i < Months.Length; i++)
            {
                Months[i] = new MonthViewMonth(this, ViewStart.AddMonths(i));
                Months[i].SetLocation(new Point(curX, curY));

                curX += gapping + MonthSize.Width;

                if ((i + 1) % calendarsX == 0)
                {
                    curX = startX;
                    curY += gapping + MonthSize.Height;
                }
            }

            MonthViewMonth first = Months[0];
            MonthViewMonth last = Months[_forwardMonthIndex];

            SetBackwardButtonBounds(new Rectangle(first.Bounds.Left + ItemPadding.Left, first.Bounds.Top + ItemPadding.Top, DaySize.Height - ItemPadding.Horizontal, DaySize.Height - ItemPadding.Vertical));
            SetForwardButtonBounds(new Rectangle(first.Bounds.Right - ItemPadding.Right - BackwardButtonBounds.Width, first.Bounds.Top + ItemPadding.Top, BackwardButtonBounds.Width, BackwardButtonBounds.Height ));
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.GotFocus"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Focus();

            _mouseDown = true;

            MonthViewDay day = HitTest(e.Location);

            if (day != null)
            {
                switch (SelectionMode)
                {
                    case MonthViewSelection.Manual:
                    case MonthViewSelection.Day:
                        SelectionEnd = _selectionStart = day.Date;
                        break;
                    case MonthViewSelection.WorkWeek:
                        SelectWorkWeek(day.Date);
                        break;
                    case MonthViewSelection.Week:
                        SelectWeek(day.Date);
                        break;
                    case MonthViewSelection.Month:
                        SelectMonth(day.Date);
                        break;
                }
            }

            if (ForwardButtonSelected)
            {
                GoForward();
            }
            else if (BackwardButtonSelected)
            {
                GoBackward();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_mouseDown)
            {
                MonthViewDay day = HitTest(e.Location);

                if (day != null && day != _lastHitted)
                {
                    switch (SelectionMode)
                    {
                        case MonthViewSelection.Manual:
                            if (day.Date > SelectionStart)
                            {
                                SelectionEnd = day.Date;
                            }
                            else
                            {
                                SelectionStart = day.Date;
                            }
                            break;
                        case MonthViewSelection.Day:
                            SelectionEnd = _selectionStart = day.Date;
                            break;
                        case MonthViewSelection.WorkWeek:
                            SelectWorkWeek(day.Date);
                            break;
                        case MonthViewSelection.Week:
                            SelectWeek(day.Date);
                            break;
                        case MonthViewSelection.Month:
                            SelectMonth(day.Date);
                            break;
                    }

                    _lastHitted = day;
                }
            }

            if (ForwardButtonBounds.Contains(e.Location))
            {
                SetForwardButtonSelected(true);
            }
            else if (ForwardButtonSelected)
            {
                SetForwardButtonSelected(false);
            }

            if (BackwardButtonBounds.Contains(e.Location))
            {
                SetBackwardButtonSelected(true);
            }
            else if (BackwardButtonSelected)
            {
                SetBackwardButtonSelected(false);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _mouseDown = false;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta < 0)
            {
                GoForward();
            }
            else
            {
                GoBackward();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.LostFocus"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(SystemColors.Window);

            for (int i = 0; i < Months.Length; i++)
            {
                if (Months[i].Bounds.IntersectsWith(e.ClipRectangle))
                {
                    #region MonthTitle

                    string title = Months[i].Date.ToString(MonthTitleFormat);
                    MonthViewBoxEventArgs evtTitle = new MonthViewBoxEventArgs(e.Graphics, Months[i].MonthNameBounds,
                        title,
                        Focused ? MonthTitleTextColor : MonthTitleTextColorInactive,
                        Focused ? MonthTitleColor : MonthTitleColorInactive);

                    DrawBox(evtTitle);

                    #endregion

                    #region DayNames

                    for (int j = 0; j < Months[i].DayNamesBounds.Length; j++)
                    {
                        MonthViewBoxEventArgs evtDay = new MonthViewBoxEventArgs(e.Graphics, Months[i].DayNamesBounds[j], Months[i].DayHeaders[j],
                            StringAlignment.Far, ForeColor, DayBackgroundColor);

                        DrawBox(evtDay);
                    }

                    if (Months[i].DayNamesBounds != null && Months[i].DayNamesBounds.Length != 0)
                    {
                        using (Pen p = new Pen(MonthTitleColor))
                        {
                            int y = Months[i].DayNamesBounds[0].Bottom;
                            e.Graphics.DrawLine(p, new Point(Months[i].Bounds.X, y), new Point(Months[i].Bounds.Right, y));
                        }
                    }
                    #endregion

                    #region Days
                    foreach (MonthViewDay day in Months[i].Days)
                    {
                        if (!day.Visible) continue;

                        MonthViewBoxEventArgs evtDay = new MonthViewBoxEventArgs(e.Graphics, day.Bounds, day.Date.Day.ToString(),
                            StringAlignment.Far,
                            day.Grayed ? DayGrayedText : (day.Selected ? DaySelectedTextColor : ForeColor),
                            day.Selected ? DaySelectedBackgroundColor : DayBackgroundColor);

                        if (day.Date.Equals(DateTime.Now.Date))
                        {
                            evtDay.BorderColor = TodayBorderColor;
                        }

                        DrawBox(evtDay);
                    }
                    #endregion 

                    #region Arrows

                    if (i == 0)
                    {
                        Rectangle r = BackwardButtonBounds;
                        using (Brush b = new SolidBrush(BackwardButtonSelected ? ArrowsSelectedColor : ArrowsColor))
                        {
                            e.Graphics.FillPolygon(b, new Point[] { 
                                new Point(r.Right, r.Top),
                                new Point(r.Right, r.Bottom - 1),
                                new Point(r.Left + r.Width / 2, r.Top + r.Height / 2),
                            });
                        }
                    }

                    if (i == _forwardMonthIndex)
                    {
                        Rectangle r = ForwardButtonBounds;
                        using (Brush b = new SolidBrush(ForwardButtonSelected ? ArrowsSelectedColor : ArrowsColor))
                        {
                            e.Graphics.FillPolygon(b, new Point[] { 
                                new Point(r.X, r.Top),
                                new Point(r.X, r.Bottom - 1),
                                new Point(r.Left + r.Width / 2, r.Top + r.Height / 2),
                            });
                        }
                    }

                    #endregion
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Resize"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            UpdateMonths();
            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:SelectionChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void OnSelectionChanged(EventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, e);
            }
        }

        #endregion

    }
}

