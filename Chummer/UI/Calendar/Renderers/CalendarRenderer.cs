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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Chummer
{
    /// <summary>
    /// Base class that renders visual elements of Calendar control
    /// </summary>
    public class CalendarRenderer
    {
        #region Static

        /// <summary>
        /// Comparison delegate to sort items
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns></returns>
        private static int CompareItems(CalendarItem item1, CalendarItem item2)
        {
            return item1.StartDate.CompareTo(item2.StartDate) * -1;
        }

        /// <summary>
        /// Comparison delegate to sort units
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns></returns>
        private static int CompareUnits(CalendarTimeScaleUnit item1, CalendarTimeScaleUnit item2)
        {
            return item1.Date.CompareTo(item2.Date);
        }

        /// <summary>
        /// Compares both <see cref="CalendarDayTop"/> items by Date
        /// </summary>
        /// <param name="top1"></param>
        /// <param name="top2"></param>
        /// <returns></returns>
        private static int CompareTops(CalendarDayTop top1, CalendarDayTop top2)
        {
            return top1.Date.CompareTo(top2.Date);
        }

        /// <summary>
        /// Creates a rectangle with rounded corners
        /// </summary>
        /// <param name="r"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static GraphicsPath RoundRectangle(Rectangle r, int radius)
        {
            return RoundRectangle(r, radius, Corners.All);
        }

        /// <summary>
        /// Creates a rectangle with the specified corners rounded
        /// </summary>
        /// <param name="r"></param>
        /// <param name="radius"></param>
        /// <param name="corners"></param>
        /// <returns></returns>
        public static GraphicsPath RoundRectangle(Rectangle r, int radius, Corners corners)
        {
            GraphicsPath path = new GraphicsPath(); if (r.Width <= 0 || r.Height <= 0) return path;
            int d = radius * 2;

            int nw = (corners & Corners.NorthWest) == Corners.NorthWest ? d : 0;
            int ne = (corners & Corners.NorthEast) == Corners.NorthEast ? d : 0;
            int se = (corners & Corners.SouthEast) == Corners.SouthEast ? d : 0;
            int sw = (corners & Corners.SouthWest) == Corners.SouthWest ? d : 0;

            path.AddLine(r.Left + nw, r.Top, r.Right - ne, r.Top);

            if (ne > 0)
            {
                path.AddArc(Rectangle.FromLTRB(r.Right - ne, r.Top, r.Right, r.Top + ne),
                    -90, 90);
            }

            path.AddLine(r.Right, r.Top + ne, r.Right, r.Bottom - se);

            if (se > 0)
            {
                path.AddArc(Rectangle.FromLTRB(r.Right - se, r.Bottom - se, r.Right, r.Bottom),
                    0, 90);
            }

            path.AddLine(r.Right - se, r.Bottom, r.Left + sw, r.Bottom);

            if (sw > 0)
            {
                path.AddArc(Rectangle.FromLTRB(r.Left, r.Bottom - sw, r.Left + sw, r.Bottom),
                    90, 90);
            }

            path.AddLine(r.Left, r.Bottom - sw, r.Left, r.Top + nw);

            if (nw > 0)
            {
                path.AddArc(Rectangle.FromLTRB(r.Left, r.Top, r.Left + nw, r.Top + nw),
                    180, 90);
            }

            path.CloseFigure();

            return path;
        }


        #endregion

        #region Events

        #endregion
        
        #region Fields

        private int _allDayItemsPadding;
        private int _standardItemHeight;
        private int _dayTopHeight;
        private int _dayTopMinHeight;
        private Calendar _calendar;
        private Rectangle[] _dayNameHeaderColumns;
        private int _dayHeaderHeight;
        private int _dayNameHeadersHeight;
        private int _itemInvalidateMargin;
        private int _itemsPadding;
        private Padding _itemTextMargin;
        private int _itemShadowPadding;
        private int _itemRoundness;
        private Rectangle _timeScaleBounds;
        private int _timeScaleUnitHeight;
        private int _timeScaleWidth;
        private int _weekHeaderWidth;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the padding of the items that goes on the top part of the days,
        /// when in <see cref="CalendarDaysMode.Expanded"/>
        /// </summary>
        /// <value>
        /// The day top items padding.
        /// </value>
        public int DayTopItemsPadding
        {
            get { return _allDayItemsPadding; }
            set { _allDayItemsPadding = value; }
        }

        /// <summary>
        /// Gets the current height of the all day items area
        /// </summary>
        /// <value>
        /// The height of the day top.
        /// </value>
        public virtual int DayTopHeight
        {
            get
            {
                if( _dayTopHeight == 0 )
                {
                    _dayTopHeight = DayTopMinHeight;
                }
                return _dayTopHeight;
            }
            set
            {
                _dayTopHeight = value;
            }
        }

        /// <summary>
        /// Gets the height of items on day tops
        /// </summary>
        /// <value>
        /// The height of the standard item.
        /// </value>
        public virtual int StandardItemHeight
        {
            get
            {
                if( _standardItemHeight == 0 )
                {
                    _standardItemHeight = TextRenderer.MeasureText( "Ag", Calendar.Font ).Height;
                }

                return _standardItemHeight + ItemTextMargin.Vertical;
            }
        }

        /// <summary>
        /// Gets the minimum height for day tops
        /// </summary>
        /// <value>
        /// The height of the day top min.
        /// </value>
        public virtual int DayTopMinHeight
        {
            get
            {
                if( _dayTopMinHeight == 0 )
                {
                    _dayTopMinHeight = TextRenderer.MeasureText( "Ag", Calendar.Font ).Height + 16;
                }

                return _dayTopMinHeight;
            }
        }

        /// <summary>
        /// Gets the <see cref="Calendar"/> this renderer belongs to
        /// </summary>
        public Calendar Calendar
        {
            get { return _calendar; }
        }

        /// <summary>
        /// Gets the bounds for day name headers
        /// </summary>
        public Rectangle[] DayNameHeaderColumns
        {
            get { return _dayNameHeaderColumns; }
        }

        /// <summary>
        /// Gets the height of the header of days
        /// </summary>
        /// <value>
        /// The height of the day header.
        /// </value>
        public virtual int DayHeaderHeight
        {
            get
            {
                if( _dayHeaderHeight == 0 )
                {
                    _dayHeaderHeight = TextRenderer.MeasureText( "Ag", Calendar.Font ).Height + 4;
                }

                return _dayHeaderHeight;
            }
        }

        /// <summary>
        /// Gets a value indicating if the day names headers are visible (e.g. Monday, Tuesday, Wednesday ...)
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [day name headers visible]; otherwise, <c>false</c>.
        /// </value>
        public bool DayNameHeadersVisible
        {
            get { return Calendar.DaysMode == CalendarDaysMode.Short; }
        }

        /// <summary>
        /// Gets the height of the day name headers
        /// </summary>
        /// <value>
        /// The height of the day name headers.
        /// </value>
        public virtual int DayNameHeadersHeight
        {
            get
            {
                if( _dayNameHeadersHeight == 0 )
                {
                    _dayNameHeadersHeight = DayHeaderHeight;
                }

                return _dayNameHeadersHeight;
            }
        }

        /// <summary>
        /// Gets the margin of the text in the items
        /// </summary>
        /// <value>
        /// The item text margin.
        /// </value>
        public virtual Padding ItemTextMargin
        {
            get { return _itemTextMargin; }
            set
            {
                _itemTextMargin = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of pixels that the item's shadow is dropped
        /// </summary>
        /// <value>
        /// The item shadow padding.
        /// </value>
        public virtual int ItemShadowPadding
        {
            get { return _itemShadowPadding; }
            set { _itemShadowPadding = value; }
        }

        /// <summary>
        /// Gets or sets the extra margin for invalidating and redrawing items
        /// </summary>
        /// <value>
        /// The item invalidate margin.
        /// </value>
        public int ItemInvalidateMargin
        {
            get { return _itemInvalidateMargin; }
            set { _itemInvalidateMargin = value; }
        }

        /// <summary>
        /// Gets or sets the padding of items on expanded mode
        /// </summary>
        /// <value>
        /// The items padding.
        /// </value>
        public int ItemsPadding
        {
            get { return _itemsPadding; }
            set { _itemsPadding = value; }
        }

        /// <summary>
        /// Gets or sets the roundness of the item
        /// </summary>
        /// <value>
        /// The item roundness.
        /// </value>
        public int ItemRoundness
        {
            get { return _itemRoundness; }
            set { _itemRoundness = value; }
        }

        /// <summary>
        /// Gets or sets the bounds of the timescale
        /// </summary>
        /// <value>
        /// The time scale bounds.
        /// </value>
        public Rectangle TimeScaleBounds
        {
            get { return _timeScaleBounds; }
            set { _timeScaleBounds = value; }
        }

        /// <summary>
        /// Gets the height of the time scale unit.
        /// </summary>
        /// <value>
        /// The height of the time scale unit.
        /// </value>
        public virtual int TimeScaleUnitHeight
        {
            get
            {
                if( _timeScaleUnitHeight == 0 )
                {
                    _timeScaleUnitHeight = TextRenderer.MeasureText( "Ag", Calendar.Font ).Height + 10;
                }

                return _timeScaleUnitHeight;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [time scale visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [time scale visible]; otherwise, <c>false</c>.
        /// </value>
        public bool TimeScaleVisible
        {
            get { return Calendar.DaysMode == CalendarDaysMode.Expanded; }
        }

        /// <summary>
        /// Gets the width of the timescale
        /// </summary>
        /// <value>
        /// The width of the time scale.
        /// </value>
        public virtual int TimeScaleWidth
        {
            get
            {
                if( _timeScaleWidth == 0 )
                {
                    _timeScaleWidth = 60;
                }
                return _timeScaleWidth;
            }
        }

        /// <summary>
        /// Gets the width of the week header.
        /// </summary>
        /// <value>
        /// The width of the week header.
        /// </value>
        public virtual int WeekHeaderWidth
        {
            get
            {
                if( _weekHeaderWidth == 0 )
                {
                    _weekHeaderWidth = TextRenderer.MeasureText( "Ag", Calendar.Font ).Height + 4;
                }

                return _weekHeaderWidth;
            }
        }

        #endregion
        
        /// <summary>
        /// Creates a new renderer for the specified calendar
        /// </summary>
        /// <param name="calendar"></param>
        public CalendarRenderer(Calendar calendar)
        {
            if (calendar == null)
            {
                throw new ArgumentNullException("calendar");
            }

            _calendar = calendar;
            _allDayItemsPadding = 5;
            _itemsPadding = 5;
            _itemTextMargin = new Padding(3);
            _itemShadowPadding = 4;
            _itemInvalidateMargin = 0;
        }

        #region Public Methods

        /// <summary>
        /// Gets the exact Y coordinate that corresponds to the specified time.
        /// This only works when in <c>Expanded</c> mode.
        /// </summary>
        /// <param name="time">Time of day to get Y coordinate</param>
        /// <returns>
        /// Y coordinate corresponding to the specified <para>time</para>
        /// </returns>
        /// <exception cref="InvalidOperationException">When calendar is not in <c>Expaned</c> mode.</exception>
        public int GetTimeY(TimeSpan time)
        {
            if (Calendar.DaysMode != CalendarDaysMode.Expanded) 
                throw new InvalidOperationException("Can't measure Time's Y when calendar isn't in Expanded mode");

            //If no days, no Y
            if (Calendar.Days == null || Calendar.Days.Length == 0) 
                return 0;
            
            CalendarDay fisrtDay = Calendar.Days[0];
            CalendarTimeScaleUnit firstUnit = fisrtDay.TimeUnits[0];
            double duration = Convert.ToDouble(firstUnit.Duration.TotalMinutes);
            double totalmins = time.TotalMinutes;
            int unitIndex = Convert.ToInt32(Math.Floor(totalmins / duration));
            double module = Convert.ToInt32(Math.Floor(totalmins % duration));
            
            CalendarTimeScaleUnit unit = Calendar.Days[0].TimeUnits[unitIndex];

            int minuteHeight = Convert.ToInt32(Convert.ToDouble(unit.Bounds.Height) / duration);

            return unit.Bounds.Top + minuteHeight * Convert.ToInt32(module);
            
        }

        /// <summary>
        /// Creates a rectangle with item roundess
        /// </summary>
        /// <param name="evtData">The instance containing the event data.</param>
        /// <param name="bounds">The bounds.</param>
        /// <returns></returns>
        public GraphicsPath ItemRectangle(CalendarRendererItemBoundsEventArgs evtData, Rectangle bounds)
        {
            int pointerPadding = 5;
            

            if ((evtData.Item.Bounds.Top != evtData.Item.MinuteStartTop ||
                evtData.Item.Bounds.Bottom != evtData.Item.MinuteEndTop) &&
                (evtData.Item.MinuteEndTop != 0 && evtData.Item.MinuteStartTop != 0) &&
                ! evtData.Item.IsOnDayTop && evtData.Calendar.DaysMode == CalendarDaysMode.Expanded)
            {
                /*
                 * Trace pointed item
                 * 
                 *     C--------------------D
                 *     |                    |
                 * A---B                    |
                 * |                        |
                 * H---G                    |
                 *     |                    |
                 *     F--------------------E
                */

                int sq = ItemRoundness * 2;
                Point a = new Point(bounds.Left, evtData.Item.MinuteStartTop);
                Point b = new Point(a.X + pointerPadding, a.Y);
                Point c = new Point(b.X, bounds.Top);
                Point d = new Point(bounds.Right, c.Y);
                Point e = new Point(d.X, bounds.Bottom);
                Point f = new Point(b.X, e.Y);
                Point g = new Point(b.X, evtData.Item.MinuteEndTop);
                Point h = new Point(a.X, g.Y);
                

                GraphicsPath path = new GraphicsPath();

                path.AddLine(a, b);
                path.AddLine(b, c);
                path.AddLine(c, new Point(d.X - sq, d.Y));
                path.AddArc(new Rectangle(d.X - sq, d.Y, sq, sq), -90, 90);
                path.AddLine(new Point(d.X, d.Y + sq), new Point(d.X, e.Y - sq));
                path.AddArc(new Rectangle(e.X - sq, e.Y - sq, sq, sq), 0, 90);
                path.AddLine(new Point(e.X - sq, e.Y), f);
                path.AddLine(f, g);
                path.AddLine(g, h);
                path.AddLine(h, a);

                path.CloseFigure();

                return path;

            }
            else
            {
                Corners crns = Corners.None;

                if (evtData.IsFirst)
                {
                    crns |= Corners.West;
                }

                if (evtData.IsLast)
                {
                    crns |= Corners.East;
                }

                return RoundRectangle(bounds, ItemRoundness, crns);
            }
        }

        /// <summary>
        /// Fills the specified rectangle with item border roundness
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarRendererItemBoundsEventArgs"/> instance containing the event data.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="north">The north.</param>
        /// <param name="south">The south.</param>
        public void ItemFill(CalendarRendererItemBoundsEventArgs e, Rectangle bounds, Color north, Color south)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using (GraphicsPath r = ItemRectangle(e, bounds))
            {
                using (LinearGradientBrush b = new LinearGradientBrush(bounds, north, south, 90))
                {
                    e.Graphics.FillPath(b, r);
                }
            }
        }

        /// <summary>
        /// Items the pattern.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarRendererItemBoundsEventArgs"/> instance containing the event data.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="patternColor">Color of the pattern.</param>
        public void ItemPattern(CalendarRendererItemBoundsEventArgs e, Rectangle bounds, Color patternColor)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using (GraphicsPath r = ItemRectangle(e, bounds))
            {
                using (Brush b = new HatchBrush(e.Item.Pattern, patternColor, Color.Transparent))
                {
                    e.Graphics.FillPath(b, r);
                }
            }
        }

        /// <summary>
        /// Draws the specified rectangle with item border roundnesss
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarRendererItemBoundsEventArgs"/> instance containing the event data.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="color">The color.</param>
        /// <param name="width">The width.</param>
        public void ItemBorder(CalendarRendererItemBoundsEventArgs e, Rectangle bounds, Color color, float width)
        {
            using (GraphicsPath r = ItemRectangle(e, bounds))
            {
                using (Pen p = new Pen(color, width))
                {
                    e.Graphics.DrawPath(p, r);
                }
            }
        }

        /// <summary>
        /// Peform layout of elements and items of the calendar
        /// </summary>
        public void PerformLayout()
        {
            PerformLayout(true);
        }

        /// <summary>
        /// Updates the bounds of graphical elements.
        /// Optionally calls <see cref="PerformItemsLayout"/> to update bounds of items.
        /// </summary>
        /// <remarks>
        /// This method is called every time the <see cref="Calendar"/> control is resized.
        /// </remarks>
        public void PerformLayout(bool performItemsLayout)
        {
            if (Calendar.Days == null) return;

            int leftStart = 0;
            int curLeft = 0;
            int curTop = 0;
            int dayWidth = 0;
            int dayHeight = 0;
            int scrollBarWidth = 20;

            TimeScaleBounds = Rectangle.Empty;

            if (Calendar.DaysMode == CalendarDaysMode.Expanded)
            {
                #region Expanded days
                TimeScaleBounds = new Rectangle(0, 0, TimeScaleWidth, Calendar.ClientRectangle.Height);
                curLeft = TimeScaleBounds.Right;
                dayHeight = Calendar.ClientSize.Height - 1;
                dayWidth = (Calendar.ClientSize.Width - TimeScaleBounds.Width - scrollBarWidth) / Calendar.Days.Length;

                for (int i = 0; i < Calendar.Days.Length; i++)
                {
                    CalendarDay day = Calendar.Days[i];
                    day.SetBounds(new Rectangle(curLeft, curTop, dayWidth, dayHeight));
                    day.DayTop.SetBounds(new Rectangle(curLeft, day.HeaderBounds.Bottom, dayWidth, DayTopHeight));
                    curLeft += dayWidth + 1;
                    //int k = 0;
                    int utop = day.BodyBounds.Top + Calendar.TimeUnitsOffset * TimeScaleUnitHeight;

                    for (int j = 0; j < day.TimeUnits.Length; j++)
                    {
                        CalendarTimeScaleUnit unit = day.TimeUnits[j];

                        if (Calendar.TimeUnitsOffset * -1 >= (j + 1))
                        {
                            unit.SetVisible(false);
                        }
                        else
                        {
                            unit.SetVisible(true);
                            //unit.SetBounds(new Rectangle(day.Bounds.Left, day.BodyBounds.Top + k++ * TimeScaleUnitHeight, day.Bounds.Width, TimeScaleUnitHeight));
                        }
                        unit.SetBounds(new Rectangle(day.Bounds.Left, utop, day.Bounds.Width, TimeScaleUnitHeight));
                        utop += TimeScaleUnitHeight;
                    }
                } 
                #endregion
            }
            else
            {
                #region Short days (Calendar View)
                leftStart = WeekHeaderWidth;
                curLeft = leftStart;
                curTop = DayNameHeadersHeight;
                dayWidth = (Calendar.ClientSize.Width - leftStart - scrollBarWidth) / 7;
                dayHeight = (Calendar.ClientSize.Height - curTop) / (Calendar.Days.Length / 7) - 1;
                _dayNameHeaderColumns = new Rectangle[7];
                int j = 0;

                for (int i = 0; i < Calendar.Days.Length; i++)
                {
                    Calendar.Days[i].SetBounds(new Rectangle(curLeft, curTop, dayWidth, dayHeight));
                    curLeft += dayWidth + 1;

                    if ((i + 1) % 7 == 0)
                    {
                        curTop += dayHeight + 1;
                        curLeft = leftStart;
                    }

                    if (i < _dayNameHeaderColumns.Length)
                    {
                        _dayNameHeaderColumns[i] = new Rectangle(curLeft, 0, dayWidth, DayNameHeadersHeight);
                    }

                    if (Calendar.Days[i].Date.DayOfWeek == Calendar.FirstDayOfWeek)
                    {
                        Calendar.Weeks[j++].SetBounds(new Rectangle(0, curTop, Calendar.ClientSize.Width, dayHeight));
                    }
                } 
                #endregion
            }

            if(performItemsLayout)
                PerformItemsLayout();
        }

        /// <summary>
        /// Updates the bounds of CalendarItems
        /// </summary>
        public void PerformItemsLayout()
        {
            if (Calendar.Days == null || Calendar.Items.Count == 0) return;
            bool alldaychanged = false;
            int offset = Math.Abs(Calendar.TimeUnitsOffset);
            List<CalendarItem> itemsOnScene = new List<CalendarItem>();

            foreach (CalendarDay day in Calendar.Days)
                day.ContainedItems.Clear();

            if (Calendar.DaysMode == CalendarDaysMode.Expanded)
            {
                #region Expanded mode algorithm

                #region Assign units and initial coords

                int maxItemsOnDayTop = 0;

                foreach (CalendarItem item in Calendar.Items)
                {
                    item.ClearBounds();
                    item.ClearPassings();

                    if (item.IsOnDayTop)
                    {
                        #region Among day tops

                        CalendarDay dayStart = item.DayStart;
                        CalendarDay dayEnd = item.DayEnd;
                            
                        if (dayStart == null) dayStart = Calendar.Days[0];
                        if (dayEnd == null) dayEnd = Calendar.Days[Calendar.Days.Length - 1];

                        for (int i = dayStart.Index; i <= dayEnd.Index; i++)
                        {
                            item.AddTopPassing(Calendar.Days[i].DayTop);
                            Calendar.Days[i].DayTop.AddPassingItem(item);
                        }

                        item.SetBounds(Rectangle.FromLTRB(dayStart.DayTop.Bounds.Left, 0, dayEnd.DayTop.Bounds.Right, 1));

                        #endregion
                    }
                    else
                    {
                        #region Among time units
                        
                        CalendarDay day = item.DayStart; if (day == null) continue;
                        double unitDurationMinutes = Convert.ToDouble((int)Calendar.TimeScale);
                        DateTime date1 = item.StartDate;
                        DateTime date2 = item.EndDate;

                        int indexStart = Convert.ToInt32(Math.Floor(date1.TimeOfDay.TotalMinutes / unitDurationMinutes));
                        int indexEnd = Convert.ToInt32(Math.Ceiling(date2.TimeOfDay.TotalMinutes / unitDurationMinutes));

                        for (int i = 0; i < day.TimeUnits.Length; i++)
                        {
                            if (i >= indexStart && i < indexEnd)
                            {
                                day.TimeUnits[i].AddPassingItem(item);
                                item.AddUnitPassing(day.TimeUnits[i]);
                            }
                        }
                        
                        item.SetBounds(Rectangle.Empty);
                        itemsOnScene.Add(item);
                        
                        #endregion
                    }
                }

                //Calendar.Items.Sort(CompareItems);
                #endregion

                #region Items on DayTops
                foreach (CalendarDay day in Calendar.Days)
                {
                    maxItemsOnDayTop = Math.Max(maxItemsOnDayTop, day.DayTop.PassingItems.Count);
                }

                int[,] tmatix = new int[Calendar.Days.Length, maxItemsOnDayTop];

                if (tmatix.GetLength(1) > 0)
                {
                    foreach (CalendarItem item in Calendar.Items)
                    {
                        if (!item.IsOnDayTop) continue;

                        item.TopsPassing.Sort(CompareTops);

                        int topStart = item.TopsPassing[0].Day.Index;
                        int topEnd = item.TopsPassing[item.TopsPassing.Count - 1].Day.Index;

                        PlaceInMatrix(ref tmatix, Calendar.Items.IndexOf(item) + 1, topStart, topEnd);
                    }

                    int dayTopsHeight = tmatix.GetLength(1) * StandardItemHeight + DayTopMinHeight;

                    if (DayTopHeight != dayTopsHeight)
                    {
                        DayTopHeight = dayTopsHeight;
                        alldaychanged = true;
                    }

                    int itemHeight = StandardItemHeight;//Convert.ToInt32(Math.Floor(Convert.ToSingle(DayTopHeight) / Convert.ToSingle(tmatix.GetLength(1))));

                    foreach (CalendarItem item in Calendar.Items)
                    {
                        if (!item.IsOnDayTop) continue;

                        int index = Calendar.Items.IndexOf(item);

                        int top, left;
                        FindInMatrix(tmatix, index + 1, out left, out top);

                        Rectangle r = item.Bounds;
                        r.Y = Calendar.Days[0].DayTop.Bounds.Top + top * itemHeight;
                        r.Height = itemHeight;
                        item.SetBounds(r);
                        item.FirstAndLastRectangleGapping();
                    }
                }
                if (alldaychanged)
                    PerformLayout(false);
                #endregion

                foreach (CalendarDay day in Calendar.Days)
                {
                    #region Create groups
                    
                    maxItemsOnDayTop = Math.Max(maxItemsOnDayTop, day.DayTop.PassingItems.Count);

                    List<List<CalendarItem>> groups = new List<List<CalendarItem>>();
                    List<CalendarItem> items = new List<CalendarItem>(day.ContainedItems);

                    while (items.Count > 0)
                    {
                        List<CalendarItem> group = new List<CalendarItem>();

                        CollectIntersectingGroup(items[0], items, group);

                        groups.Add(group);

                        foreach (CalendarItem item in group)
                            items.Remove(item);
                    }

                    #endregion

                    foreach (List<CalendarItem> group in groups)
                    {
                        #region Create group matrix

                        int maxConcurrent = 0;
                        int startIndex, endIndex;
                        GetGroupBoundUnits(group, out startIndex, out endIndex);

                        //Get the maximum concurrent items
                        for (int i = startIndex; i <= endIndex; i++) maxConcurrent = Math.Max(day.TimeUnits[i].PassingItems.Count, maxConcurrent);

                        int[,] matix = new int[maxConcurrent, endIndex - startIndex + 1];

                        foreach (CalendarItem item in group)
                        {
                            int x = 0;
                            item.UnitsPassing.Sort(CompareUnits);
                            int unitStart = item.UnitsPassing[0].Index - startIndex;
                            int unitEnd = unitStart + item.UnitsPassing.Count - 1;
                            bool xFound = false;

                            //if (startIndex + unitEnd < offset)
                            //{
                            //    item.SetIsOnView(false);
                            //    continue;
                            //}
                            //else
                            //{
                            //    item.SetIsOnView(true);
                            //}

                            while (!xFound)
                            {
                                xFound = true;

                                for (int i = unitStart; i <= unitEnd; i++)
                                {
                                    if (matix[x, i] != 0)
                                    {
                                        xFound = false;
                                        break;
                                    }
                                }
                                if (!xFound) x++;
                            }

                            for (int i = unitStart; i <= unitEnd; i++)
                            {
                                matix[x, i] = group.IndexOf(item) + 1;
                            }
                        }
                        #endregion

                        #region Expand Items
                        foreach (CalendarItem item in group)
                        {
                            int index = group.IndexOf(item);
                            int left, top;
                            int height = item.UnitsPassing.Count;
                            int width = 1;
                            FindInMatrix(matix, index + 1, out left, out top);


                            bool canExpand = left >= 0 && top >= 0;
                            while (canExpand)
                            {
                                for (int i = top; i < top + height; i++)
                                {
                                    if (matix.GetLength(0) <= left + width || matix[left + width, i] != 0)
                                    {
                                        canExpand = false;
                                        break;
                                    }
                                }

                                if (canExpand)
                                {
                                    for (int i = top; i < top + height; i++)
                                    {
                                        matix[left + width, i] = index + 1;
                                    }
                                    width++;
                                }
                            }
                        }
                        #endregion

                        #region Matrix to rectangles

                        int itemWidth = Convert.ToInt32(Math.Floor(Convert.ToSingle(day.Bounds.Width - ItemsPadding) / Convert.ToSingle(matix.GetLength(0))));

                        foreach (CalendarItem item in group)
                        {
                            int index = group.IndexOf(item);
                            int top, left;
                            int width = 1;
                            FindInMatrix(matix, index + 1, out left, out top);

                            if (left >= 0 && top >= 0)
                            {
                                for (int i = left + 1; i < matix.GetLength(0); i++)
                                {
                                    if (matix[i, top] == index + 1)
                                    {
                                        width++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                } 
                            }

                            int rtop = day.TimeUnits[item.UnitsPassing[0].Index].Bounds.Top;
                            int bottom = day.TimeUnits[item.UnitsPassing[item.UnitsPassing.Count - 1].Index].Bounds.Bottom;
                            int rleft = day.Bounds.Left + left * itemWidth;
                            int right = rleft + itemWidth * width;
                            item.SetBounds(Rectangle.FromLTRB(rleft, rtop, right, bottom));
                            item.SetMinuteStartTop(GetTimeY(item.StartDate.TimeOfDay));
                            item.SetMinuteEndTop(GetTimeY(item.EndDate.TimeOfDay));
                            
                        }

                        #endregion
                    }
                }
                #endregion
            }
            else if (Calendar.DaysMode == CalendarDaysMode.Short)
            {
                #region Short mode algorithm

                Calendar.Items.Reverse();

                for (int i = 0; i < Calendar.Days.Length; i++)
                {
                    Calendar.Days[i].ContainedItems.Clear();
                    Calendar.Days[i].SetOverflowEnd(false);
                    Calendar.Days[i].SetOverflowStart(false);
                }

                int maxItems = 0;

                foreach (CalendarItem item in Calendar.Items)
                {
                    CalendarDay dayStart = item.DayStart;
                    CalendarDay dayEnd = item.DayEnd;
                    item.ClearBounds();

                    for (int i = dayStart.Index; i <= dayEnd.Index; i++)
                    {
                        Calendar.Days[i].AddContainedItem(item);
                        maxItems = Math.Max(maxItems, Calendar.Days[i].ContainedItems.Count);
                    }
                }

                int[,] matix = new int[Calendar.Days.Length, maxItems];
                int curIndex = 0;
                foreach (CalendarItem item in Calendar.Items)
                {
                    CalendarDay dayStart = item.DayStart;
                    CalendarDay dayEnd = item.DayEnd;

                    PlaceInMatrix(ref matix, curIndex + 1, dayStart.Index, dayEnd.Index);
                    curIndex++;
                }


                for (int week = 0; week < Calendar.Weeks.Length; week++)
                {
                    int xStart = week * 7;
                    int xEnd = xStart + 6;
                    int index = 0;
                    int[,] wmatix = new int[7, matix.GetLength(1)];
                    CalendarDay sunday = Calendar.FindDay(Calendar.Weeks[week].StartDate);

                    #region Fill week matrix

                    for (int i = 0; i < wmatix.GetLength(1); i++)
                        for (int j = 0; j < wmatix.GetLength(0); j++)
                            wmatix[j, i] = matix[j + xStart, i];

                    #endregion

                    foreach (CalendarItem item in Calendar.Items)
                    {
                        int left, top, width = 0;

                        FindInMatrix(wmatix, ++index, out left, out top);

                        if (left < 0 || top < 0) continue;

                        for (int i = left; i < wmatix.GetLength(0); i++)
                            if (wmatix[i, top] == index) 
                                width++; 
                            else 
                                break;
                        

                        CalendarDay dayStart = Calendar.Days[xStart + left];
                        CalendarDay dayEnd = Calendar.Days[xStart + left + width - 1];
                        Rectangle rStart = dayStart.Bounds;
                        Rectangle rEnd = dayEnd.Bounds;
                        int rtop = rStart.Top + DayHeaderHeight + top * StandardItemHeight;
                        Rectangle r = Rectangle.FromLTRB(rStart.Left, rtop, rEnd.Right, rtop + StandardItemHeight);

                        if (r.Bottom <= sunday.Bounds.Bottom)
                            item.AddBounds(r);
                        else
                            for (int i = dayStart.Index; i <= dayEnd.Index; i++)
                                Calendar.Days[i].SetOverflowEnd(true);
                        
                    }
                }

                foreach (CalendarItem item in Calendar.Items)
                    item.FirstAndLastRectangleGapping();

                Calendar.Items.Reverse();

                #endregion
            }

            Calendar.RaiseItemsPositioned();
        }

        /// <summary>
        /// Places the specified item in the matrix for the layout engine purposes
        /// </summary>
        /// <param name="m"></param>
        /// <param name="index"></param>
        /// <param name="startX"></param>
        /// <param name="endX"></param>
        private void PlaceInMatrix(ref int[,] m, int index, int startX, int endX)
        {
            int y = 0;
            bool yFound = false;

            while (!yFound && y < m.GetLength(1)) //HACK: && y < m.GetLength(1) //This is Because of some bug, possible item not showing
            {
                yFound = true;

                for (int i = startX; i <= endX; i++)
                {
                    if (i >= 0 && i < m.GetLength(0) &&
                        m[i, y] != 0)
                    {
                        yFound = false;
                        break;
                    }
                }

                if (!yFound) y++;
            }


            if (y < m.GetLength(1)) //HACK: This if is because of same bug
            {
                for (int i = startX; i <= endX; i++)
                {
                    m[i, y] = index;
                } 
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the amout of units that can be displayed on the calendar viewport
        /// </summary>
        internal int GetVisibleTimeUnits()
        {
            if (Calendar.DaysMode == CalendarDaysMode.Short)
            {
                return 0;
            }
            else if (Calendar.Days != null && Calendar.Days.Length > 0)
            {
                return Convert.ToInt32(Math.Floor(
                    Convert.ToSingle(Calendar.Days[0].BodyBounds.Height) / Convert.ToSingle(TimeScaleUnitHeight)
                    ));
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Sets the value of the <see cref="DayTopHeight"/> property
        /// </summary>
        /// <param name="height">Height of all <see cref="CalendarDayTop"/> elements</param>
        protected void SetDayTopHeight(int height)
        {
            _dayTopHeight = height;
        }

        /// <summary>
        /// Sets the value of the <see cref="DayHeaderHeight"/> property
        /// </summary>
        /// <param name="height">Height of the day header</param>
        protected void SetDayHeaderHeight(int height)
        {
            _dayHeaderHeight = height;
        }

        /// <summary>
        /// Sets the value of the <see cref="DayNameHeadersHeight"/> property
        /// </summary>
        /// <param name="height">Height of the day name headers</param>
        protected void SetDayNameHeadersHeight(int height)
        {
            _dayNameHeadersHeight = height;
        }

        /// <summary>
        /// Sets the value of the <see cref="TimeScaleUnitHeight"/> property
        /// </summary>
        /// <param name="height">Height of the time scale unit</param>
        protected void SetTimeScaleUnitHeight(int height)
        {
            _timeScaleUnitHeight = height;
        }

        /// <summary>
        /// Sets the value of the <see cref="TimeScaleWidth"/> property
        /// </summary>
        /// <param name="width">New width for the time scale</param>
        protected void SetTimeScaleWidth(int width)
        {
            _timeScaleWidth = width;
        }

        /// <summary>
        /// Draws text using the information of the <see cref="CalendarRendererBoxEventArgs"/>
        /// </summary>
        /// <param name="e"></param>
        protected virtual void DrawStandarBoxText(CalendarRendererBoxEventArgs e)
        {
            TextRenderer.DrawText(e.Graphics, e.Text, e.Font, e.Bounds, e.TextColor, e.Format);
        }

        /// <summary>
        /// Outs the location of the specified number in the matrix
        /// </summary>
        /// <param name="m">Matrix to search in</param>
        /// <param name="number">Number to find</param>
        /// <param name="left">Result left</param>
        /// <param name="top">Result top</param>
        private void FindInMatrix(int[,] m, int number, out int left, out int top)
        {
            for (int i = 0; i < m.GetLength(1); i++)
            {
                for (int j = 0; j < m.GetLength(0); j++)
                {
                    if (m[j, i] == number)
                    {
                        left = j;
                        top = i;
                        return;
                    }
                }
            }

            left = top = -1;
        }

        /// <summary>
        /// Outs the startIndex and the endIndex of units in the group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        private void GetGroupBoundUnits(List<CalendarItem> group, out int startIndex, out int endIndex)
        {
            startIndex = int.MaxValue;
            endIndex = int.MinValue;

            foreach (CalendarItem item in group)
            {
                foreach (CalendarTimeScaleUnit unit in item.UnitsPassing)
                {
                    startIndex = Math.Min(startIndex, unit.Index);
                    endIndex = Math.Max(endIndex, unit.Index);
                }
            }
        }

        /// <summary>
        /// Recursive method that collects items intersecting on time, to graphically represent-them on the layout
        /// </summary>
        /// <param name="calendarItem"></param>
        /// <param name="items"></param>
        /// <param name="grouped"></param>
        private void CollectIntersectingGroup(CalendarItem calendarItem, List<CalendarItem> items, List<CalendarItem> grouped)
        {
            if (!grouped.Contains(calendarItem))
                grouped.Add(calendarItem);

            foreach (CalendarItem item in items)
            {
                if (!grouped.Contains(item) &&
                    calendarItem.IntersectsWith(item.StartDate.TimeOfDay, item.EndDate.TimeOfDay))
                {
                    grouped.Add(item);

                    CollectIntersectingGroup(item, items, grouped);
                }
            }
        }

        /// <summary>
        /// Prints the specified matrix on debug
        /// </summary>
        /// <param name="m"></param>
        private void PrintMatrix(int[,] m)
        {
            //return;
            Console.WriteLine("--------------------------------");
            for (int i = 0; i < m.GetLength(1); i++)
            {
                for (int j = 0; j < m.GetLength(0); j++)
                {
                    Console.Write(string.Format(" {0}", m[j, i]));
                }
                Console.WriteLine(" ");
            }
            Console.WriteLine("--------------------------------");
        }

        #endregion

        #region Render Methods

        /// <summary>
        /// Initializes the Calendar
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnInitialize(CalendarRendererEventArgs e)
        {

        }

        /// <summary>
        /// Paints the background of the calendar
        /// </summary>
        /// <param name="e">Paint info</param>
        public virtual void OnDrawBackground(CalendarRendererEventArgs e)
        {
        }

        /// <summary>
        /// Paints the timescale of the calendar
        /// </summary>
        /// <param name="e">Paint info</param>
        public virtual void OnDrawTimeScale(CalendarRendererEventArgs e)
        {
            if (e.Calendar.DaysMode == CalendarDaysMode.Short
                || e.Calendar.Days == null
                || e.Calendar.Days.Length == 0
                || e.Calendar.Days[0].TimeUnits == null
                ) return;

            Font hourFont = new Font(e.Calendar.Font.FontFamily, e.Calendar.Font.Size * (e.Calendar.TimeScale == CalendarTimeScale.SixtyMinutes ? 1f : 1.5f));
            Font minuteFont = e.Calendar.Font;
            int hourLeft = TimeScaleBounds.Left;
            int hourWidth = TimeScaleBounds.Left + TimeScaleBounds.Width / 2;
            int minuteLeft = hourLeft + hourWidth;
            int minuteWidth = hourWidth;
            int k = 0;

            for (int i = 0; i < e.Calendar.Days[0].TimeUnits.Length; i++)
            {
                CalendarTimeScaleUnit unit = e.Calendar.Days[0].TimeUnits[i];

                if (!unit.Visible) continue;

                string hours = unit.Hours.ToString("00");
                string minutes = unit.Minutes == 0 ? "00" : string.Empty;

                if (!string.IsNullOrEmpty(minutes))
                {
                    switch( _calendar.CalendarTimeFormat )
                    {
                        case CalendarTimeFormat.TwelveHour:
                            if( Convert.ToInt32( hours ) > 12 ) hours = ( Convert.ToInt32( hours ) - 12 ).ToString();
                            break;

                        case CalendarTimeFormat.TwentyFourHour:
                            if( hours == "00" ) hours = "12";
                            break;
                    }

                    CalendarRendererBoxEventArgs hevt = new CalendarRendererBoxEventArgs(e, new Rectangle(hourLeft, unit.Bounds.Top, hourWidth, unit.Bounds.Height), hours, TextFormatFlags.Right);

                    hevt.Font = hourFont;

                    OnDrawTimeScaleHour(hevt);

                    if (k++ == 0 || unit.Hours == 0 || unit.Hours == 12 ) minutes = unit.Date.ToString("tt");

                    CalendarRendererBoxEventArgs mevt = new CalendarRendererBoxEventArgs(e, new Rectangle(minuteLeft, unit.Bounds.Top, minuteWidth, unit.Bounds.Height), minutes, TextFormatFlags.Top | TextFormatFlags.Left);

                    mevt.Font = minuteFont;

                    OnDrawTimeScaleMinutes(mevt);
                }
            }
        }

        /// <summary>
        /// Paints an hour of a timescale unit
        /// </summary>
        /// <param name="e">Paint Info</param>
        public virtual void OnDrawTimeScaleHour(CalendarRendererBoxEventArgs e)
        {
            DrawStandarBoxText(e);
        }

        /// <summary>
        /// Paints minutes of a timescale unit
        /// </summary>
        /// <param name="e">Paint Info</param>
        public virtual void OnDrawTimeScaleMinutes(CalendarRendererBoxEventArgs e)
        {
            DrawStandarBoxText(e);
        }

        /// <summary>
        /// Paints the days on the current calendar view
        /// </summary>
        /// <param name="e">Paint Info</param>
        public virtual void OnDrawDays(CalendarRendererEventArgs e)
        {
            for (int i = 0; i < e.Calendar.Days.Length; i++)
            {
                CalendarDay day = e.Calendar.Days[i];

                e.Tag = day;

                OnDrawDay(new CalendarRendererDayEventArgs(e, 
                    day));
            }
        }

        /// <summary>
        /// Paints the specified day on the calendar
        /// </summary>
        /// <param name="e">Paint info</param>
        public virtual void OnDrawDay(CalendarRendererDayEventArgs e)
        {
            CalendarDay day = e.Day;

            CalendarRendererBoxEventArgs hevt = new CalendarRendererBoxEventArgs(e,
                    day.HeaderBounds,
                    day.Date.Day.ToString(),
                    TextFormatFlags.VerticalCenter);
            hevt.Font = new Font(Calendar.Font, FontStyle.Bold);

            CalendarRendererBoxEventArgs devt = new CalendarRendererBoxEventArgs(e,
                    day.HeaderBounds,
                    day.Date.ToString("dddd"),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            OnDrawDayHeaderBackground(e);

            if (Calendar.DaysMode == CalendarDaysMode.Short && (day.Index == 0 || day.Date.Day == 1))
            {
                hevt.Text = day.Date.ToString("dd MMM");
            }

            OnDrawDayHeaderText(hevt);

            if (devt.TextSize.Width < day.HeaderBounds.Width - hevt.TextSize.Width * 2
                && e.Calendar.DaysMode == CalendarDaysMode.Expanded)
            {
                OnDrawDayHeaderText(devt);
            }

            

            OnDrawDayTimeUnits(e);
            OnDrawDayTop(e);
            OnDrawDayBorder(e);
        }

        /// <summary>
        /// Paints the border of the specified day
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnDrawDayBorder(CalendarRendererDayEventArgs e)
        {

        }

        /// <summary>
        /// Draws the all day items area
        /// </summary>
        /// <param name="e">Paint Info</param>
        public virtual void OnDrawDayTop(CalendarRendererDayEventArgs e)
        {
        }

        /// <summary>
        /// Paints the background of the specified day's header
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnDrawDayHeaderBackground(CalendarRendererDayEventArgs e)
        {

        }

        /// <summary>
        /// Paints the header of the specified day
        /// </summary>
        /// <param name="e">Paint info</param>
        public virtual void OnDrawDayHeaderText(CalendarRendererBoxEventArgs e)
        {
            DrawStandarBoxText(e);
        }

        /// <summary>
        /// Raises the <see cref="E:DrawDayTimeUnits"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarRendererDayEventArgs"/> instance containing the event data.</param>
        public virtual void OnDrawDayTimeUnits(CalendarRendererDayEventArgs e)
        {
            for (int i = 0; i < e.Day.TimeUnits.Length; i++)
            {
                CalendarTimeScaleUnit unit = e.Day.TimeUnits[i];

                if(unit.Visible)
                    OnDrawDayTimeUnit(new CalendarRendererTimeUnitEventArgs(e, unit));
            }
        }

        /// <summary>
        /// Draws a time unit of a day
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnDrawDayTimeUnit(CalendarRendererTimeUnitEventArgs e)
        {

        }

        /// <summary>
        /// Raises the <see cref="E:DrawDayNameHeaders"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarRendererEventArgs"/> instance containing the event data.</param>
        public virtual void OnDrawDayNameHeaders(CalendarRendererEventArgs e)
        {
            DateTime startDate = DateTime.Now.AddDays(-((int)DateTime.Now.DayOfWeek % 7) + 1 + (int)Calendar.FirstDayOfWeek);

            for (int i = 0; i < DayNameHeaderColumns.Length; i++)
            {
                OnDrawDayNameHeader(new CalendarRendererBoxEventArgs(e,
                    DayNameHeaderColumns[i], 
                    startDate.AddDays(i).ToString("dddd"),
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter));

            }
        }

        /// <summary>
        /// Raises the <see cref="E:DrawDayNameHeader"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarRendererBoxEventArgs"/> instance containing the event data.</param>
        public virtual void OnDrawDayNameHeader(CalendarRendererBoxEventArgs e)
        {
            DrawStandarBoxText(e);
        }

        /// <summary>
        /// Draws the items of the calendar
        /// </summary>
        /// <param name="e">Event info</param>
        public virtual void OnDrawItems(CalendarRendererEventArgs e)
        {
            Rectangle days = e.Calendar.DaysBodyRectangle; days.Inflate(-1, -1);
            Region oldclip = e.Graphics.Clip;
            bool doClip = e.Calendar.DaysMode == CalendarDaysMode.Expanded;
            bool clipped = false;

            #region Shadows
            foreach (CalendarItem item in e.Calendar.Items)
            {
                clipped = false;

                if (doClip && !item.IsOnDayTop && item.Bounds.Top < days.Top)
                {
                    e.Graphics.SetClip(days, CombineMode.Intersect);
                    clipped = true;
                }

                List<Rectangle> rects = new List<Rectangle>(item.GetAllBounds());

                for (int i = 0; i < rects.Count; i++)
                {
                    CalendarRendererItemBoundsEventArgs evt = new CalendarRendererItemBoundsEventArgs(
                        new CalendarRendererItemEventArgs(e, item),
                        rects[i],
                        i == 0 && !item.IsOpenStart,
                        (i == rects.Count - 1) && !item.IsOpenEnd);
                    OnDrawItemShadow(evt);
                }

                if (clipped)
                    e.Graphics.SetClip(oldclip, CombineMode.Replace);
            } 
            #endregion

            #region Items
            foreach (CalendarItem item in e.Calendar.Items)
            {
                clipped = false;

                if( doClip && !item.IsOnDayTop && item.Bounds.Top < days.Top )
                {
                    e.Graphics.SetClip( days, CombineMode.Intersect );
                    clipped = true;
                }

                OnDrawItem(new CalendarRendererItemEventArgs(e, item));

                if( clipped )
                {
                    e.Graphics.SetClip( oldclip, CombineMode.Replace );
                }
            } 
            #endregion

            #region Borders of selected items
            foreach (CalendarItem item in e.Calendar.Items)
            {
                if (!item.Selected) continue;

                List<Rectangle> rects = new List<Rectangle>(item.GetAllBounds());

                for (int i = 0; i < rects.Count; i++)
                {
                    CalendarRendererItemBoundsEventArgs evt = new CalendarRendererItemBoundsEventArgs(
                        new CalendarRendererItemEventArgs(e, item),
                        rects[i],
                        i == 0 && !item.IsOpenStart,
                        (i == rects.Count - 1) && !item.IsOpenEnd);

                    SmoothingMode smbuff = e.Graphics.SmoothingMode;
                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                    OnDrawItemBorder(evt);

                    e.Graphics.SmoothingMode = smbuff;
                }

            } 
            #endregion
        }

        /// <summary>
        /// Draws an item of the calendar
        /// </summary>
        /// <param name="e">Event Info</param>
        public virtual void OnDrawItem( CalendarRendererItemEventArgs e )
        {
            List<Rectangle> rects = new List<Rectangle>( e.Item.GetAllBounds() );

            for( int i = 0; i < rects.Count; i++ )
            {
                CalendarRendererItemBoundsEventArgs evt = new CalendarRendererItemBoundsEventArgs(
                    e,
                    rects[i],
                    i == 0 && !e.Item.IsOpenStart,
                    ( i == rects.Count - 1 ) && !e.Item.IsOpenEnd );

                OnDrawItemBackground( evt );

                if( !evt.Item.PatternColor.IsEmpty )
                {
                    OnDrawItemPattern( evt );
                }

                if( !e.Item.IsEditing )
                {
                    OnDrawItemContent( evt );
                }

                OnDrawItemBorder( evt );

            }
        }

        /// <summary>
        /// Draws the background of the specified item
        /// </summary>
        /// <param name="e">Event Info</param>
        public virtual void OnDrawItemBackground(CalendarRendererItemBoundsEventArgs e)
        {

        }

        /// <summary>
        /// Raises the <see cref="E:DrawItemPattern"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarRendererItemBoundsEventArgs"/> instance containing the event data.</param>
        public virtual void OnDrawItemPattern(CalendarRendererItemBoundsEventArgs e)
        {
            foreach (Rectangle bounds in e.Item.GetAllBounds())
            {
                ItemPattern(e, bounds, e.Item.PatternColor);
            }
        }

        /// <summary>
        /// Draws the strings of an item. Strings inlude StartTime, EndTime and Text
        /// </summary>
        /// <param name="e">Event Info</param>
        public virtual void OnDrawItemContent(CalendarRendererItemBoundsEventArgs e)
        {
            if (e.Item == e.Calendar.EditModeItem) return;

            List<Rectangle> rectangles = new List<Rectangle>(e.Item.GetAllBounds());

            for (int i = 0; i < rectangles.Count; i++)
            {
                Rectangle bounds = rectangles[i];
                Rectangle imageBounds = Rectangle.Empty;
                Rectangle rStartTime = new Rectangle();
                Rectangle rEndTime = new Rectangle();
                string endTime = string.Empty;
                string startTime = string.Empty;
                Color secondaryForecolor = e.Item.ForeColor;

                if (e.Item.ShowEndTime && i == rectangles.Count - 1)
                {
                    endTime = e.Item.EndDateText;
                    rEndTime = new Rectangle(Point.Empty, TextRenderer.MeasureText(endTime, e.Calendar.Font));
                    rEndTime = Rectangle.FromLTRB(bounds.Right - rEndTime.Width - ItemTextMargin.Right,
                        bounds.Top + ItemTextMargin.Top,
                        bounds.Right - ItemTextMargin.Right,
                        bounds.Bottom - ItemTextMargin.Bottom);
                    OnDrawItemEndTime(new CalendarRendererBoxEventArgs(e, rEndTime, endTime, secondaryForecolor));
                }

                if (e.Item.ShowStartTime && i == 0)
                {
                    startTime = e.Item.StartDateText;
                    rStartTime = new Rectangle(Point.Empty, TextRenderer.MeasureText(startTime, e.Calendar.Font));
                    rStartTime.X = bounds.Left + ItemTextMargin.Left;
                    rStartTime.Y = bounds.Top + ItemTextMargin.Top;
                    rStartTime.Height = bounds.Height - ItemTextMargin.Vertical;
                    OnDrawItemStartTime(new CalendarRendererBoxEventArgs(e, rStartTime, startTime, secondaryForecolor));
                }

                Rectangle r = Rectangle.FromLTRB(
                    bounds.Left + ItemTextMargin.Left + rStartTime.Width,
                    bounds.Top + ItemTextMargin.Top,
                    bounds.Right - ItemTextMargin.Right - rEndTime.Width,
                    bounds.Bottom - ItemTextMargin.Bottom);

                CalendarRendererBoxEventArgs evt = new CalendarRendererBoxEventArgs(e, r, e.Item.Text, TextFormatFlags.Left | TextFormatFlags.Top);

                evt.Font = e.Item.Font;

                if( e.Item.ShowStartTime || e.Item.ShowEndTime )
                {
                    evt.Font = new Font( evt.Font, FontStyle.Bold );
                }

                if( e.Item.IsOnDayTop || Calendar.DaysMode == CalendarDaysMode.Short )
                {
                    evt.Format |= TextFormatFlags.HorizontalCenter;
                }

                if (!e.Item.ForeColor.IsEmpty)
                {
                    evt.TextColor = e.Item.ForeColor;
                }

                evt.Tag = e.Item;

                #region Image

                if (e.Item.Image != null)
                {
                    Rectangle tBounds = e.Item.Bounds;
                    imageBounds.Size = e.Item.Image.Size;

                    switch (e.Item.ImageAlign)
                    {
                        case CalendarItemImageAlign.North:
                            tBounds.Height -= imageBounds.Height;
                            tBounds.Y += imageBounds.Height;
                            imageBounds.Y = tBounds.Y - imageBounds.Height;
                            break;
                        case CalendarItemImageAlign.South:
                            tBounds.Height -= imageBounds.Height;
                            imageBounds.Y = tBounds.Bottom;
                            break;
                        case CalendarItemImageAlign.East:
                            tBounds.Width -= imageBounds.Width;
                            imageBounds.X = tBounds.Right;
                            break;
                        case CalendarItemImageAlign.West:
                            tBounds.Width -= imageBounds.Width;
                            tBounds.X += imageBounds.Width;
                            imageBounds.X = tBounds.Left - imageBounds.Width;
                            break;
                    }

                    switch (e.Item.ImageAlign)
                    {
                        case CalendarItemImageAlign.North:
                        case CalendarItemImageAlign.South:
                            imageBounds.X = e.Item.Bounds.X + ( ( e.Item.Bounds.Width - imageBounds.Width ) / 2);
                            break;
                        case CalendarItemImageAlign.East:
                        case CalendarItemImageAlign.West:
                            imageBounds.Y = e.Item.Bounds.Y + ((e.Item.Bounds.Height - imageBounds.Height) / 2);
                            break;
                    }

                    evt.Bounds = tBounds;
                    OnDrawItemImage(new CalendarRendererItemBoundsEventArgs(e, imageBounds, false, false));
                }

                #endregion

                OnDrawItemText(evt);
            }
        }

        /// <summary>
        /// Draws the text of an item
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnDrawItemText(CalendarRendererBoxEventArgs e)
        {
            DrawStandarBoxText(e); 
        }

        /// <summary>
        /// Draws the image of an item
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnDrawItemImage(CalendarRendererItemBoundsEventArgs e)
        {
            if (e.Item.Image != null)
            {
                e.Graphics.DrawImage(e.Item.Image, e.Bounds);
            }
        }

        /// <summary>
        /// Draws the starttime of the item if applicable
        /// </summary>
        /// <param name="e">Event data</param>
        public virtual void OnDrawItemStartTime(CalendarRendererBoxEventArgs e)
        {
            DrawStandarBoxText(e);
        }

        /// <summary>
        /// Draws the end time of the item if applicable
        /// </summary>
        /// <param name="e">Event data</param>
        public virtual void OnDrawItemEndTime(CalendarRendererBoxEventArgs e)
        {
            DrawStandarBoxText(e);
        }

        /// <summary>
        /// Draws the border of the specified item
        /// </summary>
        /// <param name="e">Event Info</param>
        public virtual void OnDrawItemBorder(CalendarRendererItemBoundsEventArgs e)
        {

        }

        /// <summary>
        /// Draws the shadow of the specified item
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnDrawItemShadow(CalendarRendererItemBoundsEventArgs e)
        {

        }

        /// <summary>
        /// Draws the overflows of days
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnDrawOverflows(CalendarRendererEventArgs e)
        {
            for (int i = 0; i < e.Calendar.Days.Length; i++)
            {
                CalendarDay day = e.Calendar.Days[i];

                if (day.OverflowStart)
                {
                    OnDrawDayOverflowStart(new CalendarRendererDayEventArgs(e, day));
                }

                if(day.OverflowEnd)
                {
                    OnDrawDayOverflowEnd(new CalendarRendererDayEventArgs(e, day));
                }
            }
        }

        /// <summary>
        /// Draws the overflow to start of specified day
        /// </summary>
        /// <param name="e">Event data</param>
        public virtual void OnDrawDayOverflowStart(CalendarRendererDayEventArgs e)
        {

        }

        /// <summary>
        /// Draws the overflow to end of specified day
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnDrawDayOverflowEnd(CalendarRendererDayEventArgs e)
        {
            //e.Graphics.FillRectangle(Brushes.Red, e.Day.OverflowEndBounds);
        }

        /// <summary>
        /// Raises the <see cref="E:DrawWeekHeaders"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarRendererEventArgs"/> instance containing the event data.</param>
        public virtual void OnDrawWeekHeaders(CalendarRendererEventArgs e)
        {
            if (Calendar.Weeks == null) return;

            for (int i = 0; i < Calendar.Weeks.Length; i++)
            {
                string str = Calendar.Weeks[i].ToStringLarge();
                SizeF sz = e.Graphics.MeasureString(str, e.Calendar.Font);

                if (sz.Width > Calendar.Weeks[i].HeaderBounds.Height)
                {
                    str = Calendar.Weeks[i].ToStringShort();
                }

                OnDrawWeekHeader(new CalendarRendererBoxEventArgs(e,
                    Calendar.Weeks[i].HeaderBounds, str,  TextFormatFlags.Default));
            }
        }

        /// <summary>
        /// Raises the <see cref="E:DrawWeekHeader"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarRendererBoxEventArgs"/> instance containing the event data.</param>
        public virtual void OnDrawWeekHeader(CalendarRendererBoxEventArgs e)
        {
            StringFormat sf = new StringFormat();
            sf.FormatFlags = StringFormatFlags.DirectionVertical | StringFormatFlags.DirectionRightToLeft | StringFormatFlags.NoWrap;
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;

            using (SolidBrush b = new SolidBrush(e.TextColor))
            {
                e.Graphics.DrawString(e.Text, e.Font, b, e.Bounds, sf);
            }

            e.Graphics.ResetTransform();

            sf.Dispose();
        }



        #endregion

    }
}
