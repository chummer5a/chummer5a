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
using System.Globalization;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// Represents an item of the calendar with a date and timespan
    /// </summary>
    /// <remarks>
    /// <para>CalendarItem provides a graphical representation of tasks within a date range.</para>
    /// </remarks>
    public sealed class CalendarItem
        : CalendarSelectableElement, IHasInternalId
    {
        #region Static

        /// <summary>
        /// Compares the bounds.
        /// </summary>
        /// <param name="r1">The r1.</param>
        /// <param name="r2">The r2.</param>
        /// <returns></returns>
        private static int CompareBounds(Rectangle r1, Rectangle r2)
        {
            return r1.Top.CompareTo(r2.Top);
        }

        #endregion

        #region Events

        #endregion

        #region Fields

        private DateTime _startDate;
        private DateTime _endDate;

        private TimeSpan _duration;

        private Guid _guiID;
        private bool _inCharacter;
        private string _notes;
        private HatchStyle _pattern;
        private Color _patternColor;
        private Color _backgroundColor;
        private Color _backgroundColorLighter;
        private Color _borderColor;
        private Color _foreColor;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets an array of rectangles containing bounds additional to Bounds property.
        /// </summary>
        /// <remarks>
        /// Items may contain additional bounds because of several graphical occourences, mostly when <see cref="Calendar"/> in 
        /// <see cref="CalendarDaysMode.Short"/> mode, due to the duration of the item; e.g. when an all day item lasts several weeks, 
        /// one rectangle for week must be drawn to indicate the presence of the item.
        /// </remarks>
        public Rectangle[] AditionalBounds { get; set; }

        public string InternalId => _guiID.ToString();

        /// <summary>
        /// Gets or sets the a background color for the object. If Color.Empty, renderer default's will be used.
        /// </summary>
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        /// <summary>
        /// Gets or sets the lighter background color of the item
        /// </summary>
        public Color BackgroundColorLighter
        {
            get => _backgroundColorLighter;
            set => _backgroundColorLighter = value;
        }

        /// <summary>
        /// Gets or sets the bordercolor of the item. If Color.Empty, renderer default's will be used.
        /// </summary>
        public Color BorderColor
        {
            get => _borderColor;
            set => _borderColor = value;
        }

        /// <summary>
        /// Gets the StartDate of the item. Implemented
        /// </summary>
        public override DateTime Date => StartDate;

        /// <summary>
        /// Gets the day on the <see cref="Calendar"/> where this item ends
        /// </summary>
        /// <remarks>
        /// This day is not necesarily the day corresponding to the day on <see cref="EndDate"/>, 
        /// since this date can be out of the range of the current view.
        /// <para>If Item is not on view date range this property will return null.</para>
        /// </remarks>
        public CalendarDay DayEnd
        {
            get
            {
                if (!IsOnViewDateRange)
                {
                    return null;
                }

                if (IsOpenEnd)
                {
                    return Calendar.Days[Calendar.Days.Length - 1];
                }

                return Calendar.FindDay(EndDate);
            }
        }

        // ReSharper disable once ConvertToAutoProperty
        public bool InCharacter
        {
            get => _inCharacter;
            set => _inCharacter = value;
        }

        /// <summary>
        /// Gets the day on the <see cref="Calendar"/> where this item starts
        /// </summary>
        /// <remarks>
        /// This day is not necesarily the day corresponding to the day on <see cref="StartDate"/>, 
        /// since start date can be out of the range of the current view.
        /// <para>If Item is not on view date range this property will return null.</para>
        /// </remarks>
        public CalendarDay DayStart
        {
            get
            {
                if (!IsOnViewDateRange)
                {
                    return null;
                }

                if (IsOpenStart)
                {
                    return Calendar.Days[0];
                }

                return Calendar.FindDay(StartDate);
            }
        }

        /// <summary>
        /// Gets the duration of the item
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                if (_duration.TotalMinutes == 0)
                {
                    _duration = EndDate.Subtract(StartDate);
                }
                return _duration;
            }
        }

        /// <summary>
        /// Gets or sets the end time of the item
        /// </summary>
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                _duration = new TimeSpan(0, 0, 0);
                ClearPassings();
            }
        }

        /// <summary>
        /// Gets the text of the end date
        /// </summary>
        public string EndDateText
        {
            get
            {
                string date = string.Empty;
                string time = string.Empty;

                if (IsOpenEnd)
                {
                    date = EndDate.ToString(Calendar.ItemsDateFormat);
                }

                if (ShowEndTime && !EndDate.TimeOfDay.Equals(new TimeSpan(23, 59, 59)))
                {
                    time = EndDate.ToString(Calendar.ItemsTimeFormat);
                }

                return $"{date} {time}".Trim();
            }
        }

        /// <summary>
        /// Gets or sets the forecolor of the item. If Color.Empty, renderer default's will be used.
        /// </summary>
        public Color ForeColor
        {
            get => _foreColor;
            set => _foreColor = value;
        }

        /// <summary>
        /// Gets or sets an image for the item
        /// </summary>
        public Image Image { get; set; }

        /// <summary>
        /// Gets or sets the alignment of the image relative to the text
        /// </summary>
        public CalendarItemImageAlign ImageAlign { get; set; }

        /// <summary>
        /// Gets a value indicating if the item is being dragged
        /// </summary>
        public bool IsDragging { get; private set; }

        /// <summary>
        /// Gets a value indicating if the item is currently being edited by the user
        /// </summary>
        public bool IsEditing { get; private set; }

        /// <summary>
        /// Gets a value indicating if the item goes on the DayTop area of the <see cref="CalendarDay"/>
        /// </summary>
        public bool IsOnDayTop => StartDate.Day != EndDate.AddSeconds(1).Day;

        /// <summary>
        /// Gets a value indicating if the item is currently on view.
        /// </summary>
        /// <remarks>
        /// The item may not be on view because of scrolling
        /// </remarks>
        public bool IsOnView { get; private set; }

        /// <summary>
        /// Gets a value indicating if the item is on the range specified by <see cref="Calendar.ViewStart"/> and <see cref="Calendar.ViewEnd"/>
        /// </summary>
        public bool IsOnViewDateRange
        {
            get
            {
                //Checks for an intersection of item's dates against calendar dates
                DateTime fd = Calendar.Days[0].Date;
                DateTime ld = Calendar.Days[Calendar.Days.Length - 1].Date.Add(new TimeSpan(23, 59, 59));
                DateTime sd = StartDate;
                DateTime ed = EndDate;
                return sd < ld && fd < ed;
            }
        }

        /// <summary>
        /// Gets a value indicating if the item's <see cref="StartDate"/> is before the <see cref="Calendar.ViewStart"/> date.
        /// </summary>
        public bool IsOpenStart => StartDate.CompareTo(Calendar.Days[0].Date) < 0;

        /// <summary>
        /// Gets a value indicating if the item's <see cref="EndDate"/> is aftter the <see cref="Calendar.ViewEnd"/> date.
        /// </summary>
        public bool IsOpenEnd => EndDate.CompareTo(Calendar.Days[Calendar.Days.Length - 1].Date.Add(new TimeSpan(23, 59, 59))) > 0;

        /// <summary>
        /// Gets a value indicating if item is being resized by the <see cref="StartDate"/>
        /// </summary>
        public bool IsResizingStartDate { get; private set; }

        /// <summary>
        /// Gets a value indicating if item is being resized by the <see cref="EndDate"/>
        /// </summary>
        public bool IsResizingEndDate { get; private set; }

        /// <summary>
        /// Gets a value indicating if this item is locked.
        /// </summary>
        /// <remarks>
        /// When an item is locked, the user can't drag it or change it's text
        /// </remarks>
        public bool Locked { get; set; }

        /// <summary>
        /// Gets the top correspoinding to the ending minute
        /// </summary>
        public int MinuteEndTop { get; private set; }

        /// <summary>
        /// Gets the top corresponding to the starting minute
        /// </summary>
        public int MinuteStartTop { get; private set; }

        /// <summary>
        /// Gets or sets the units that this item passes by
        /// </summary>
        internal List<CalendarTimeScaleUnit> UnitsPassing { get; set; }

        /// <summary>
        /// Gets or sets the pattern style to use in the background of item.
        /// </summary>
        public HatchStyle Pattern
        {
            get => _pattern;
            set => _pattern = value;
        }

        /// <summary>
        /// Gets or sets the pattern's color
        /// </summary>
        public Color PatternColor
        {
            get => _patternColor;
            set => _patternColor = value;
        }

        /// <summary>
        /// Gets the list of DayTops that this item passes thru
        /// </summary>
        internal List<CalendarDayTop> TopsPassing { get; }

        /// <summary>
        /// Gets a value indicating if the item should show the time of the <see cref="StartDate"/>
        /// </summary>
        public bool ShowStartTime => IsOpenStart || ((this.IsOnDayTop || Calendar.DaysMode == CalendarDaysMode.Short) && !StartDate.TimeOfDay.Equals(new TimeSpan(0, 0, 0)));

        /// <summary>
        /// Gets a value indicating if the item should show the time of the <see cref="EndDate"/>
        /// </summary>
        public bool ShowEndTime => (IsOpenEnd ||
                                            ((this.IsOnDayTop || Calendar.DaysMode == CalendarDaysMode.Short) && !EndDate.TimeOfDay.Equals(new TimeSpan(23, 59, 59)))) &&
                                           !(Calendar.DaysMode == CalendarDaysMode.Short && StartDate.Date == EndDate.Date);

        /// <summary>
        /// Gets the text of the start date
        /// </summary>
        public string StartDateText
        {
            get
            {
                string date = string.Empty;
                string time = string.Empty;

                if (IsOpenStart)
                {
                    date = StartDate.ToString(Calendar.ItemsDateFormat);
                }

                if (ShowStartTime && !StartDate.TimeOfDay.Equals(new TimeSpan(0, 0, 0)))
                {
                    time = StartDate.ToString(Calendar.ItemsTimeFormat);
                }

                return $"{date} {time}".Trim();
            }
        }

        /// <summary>
        /// Gets or sets the start time of the item
        /// </summary>
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                _duration = new TimeSpan(0, 0, 0);
                ClearPassings();
            }
        }

        /// <summary>
        /// Gets or sets the text of the item
        /// </summary>
        public string Text
        {
            get => _notes;
            set => _notes = value;
        }

        /// <summary>
        /// Gets or sets the font.
        /// </summary>
        /// <value>
        /// The font.
        /// </value>
        public Font Font { get; set; }

        public Calendar Calendar { get; set; }

        #endregion

        #region Constructor, Save, Load, Print Methods 


        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("week");
            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("datestart", StartDate.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("dateend", EndDate.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("incharacter", InCharacter.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("notes", Text);
            objWriter.WriteElementString("forecolor", ForeColor.Name);
            objWriter.WriteElementString("pattern", Pattern.ToString());
            objWriter.WriteElementString("patterncolor", PatternColor.Name);
            objWriter.WriteElementString("backgroundcolor", BackgroundColor.Name);
            objWriter.WriteElementString("backgroundcolorlighter", BackgroundColorLighter.Name);
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Calendar Week from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            _guiID = Guid.Parse(objNode["guid"].InnerText);
            if (objNode["datestart"] != null)
            {
                _startDate = DateTime.ParseExact(objNode["datestart"].InnerText, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                _endDate   = DateTime.ParseExact(objNode["dateend"].InnerText, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
            else if (objNode["week"] != null)
            {
                int i = 0;
                switch (Convert.ToInt32(objNode["week"].InnerText))
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        i = 1;
                        break;
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                        i = 2;
                        break;
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        i = 3;
                        break;
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        i = 4;
                        break;
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                        i = 5;
                        break;
                    case 22:
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                        i = 6;
                        break;
                    case 27:
                    case 28:
                    case 29:
                    case 30:
                        i = 7;
                        break;
                    case 31:
                    case 32:
                    case 33:
                    case 34:
                        i = 8;
                        break;
                    case 35:
                    case 36:
                    case 37:
                    case 38:
                    case 39:
                        i = 9;
                        break;
                    case 40:
                    case 41:
                    case 42:
                    case 43:
                        i = 10;
                        break;
                    case 44:
                    case 45:
                    case 46:
                    case 47:
                        i = 11;
                        break;
                    default:
                        i = 12;
                        break;
                }
                StartDate = DateTime.Parse(string.Format($"1/{i}/{objNode["year"].InnerText}"));
                EndDate = DateTime.Parse(string.Format($"1/{i}/{objNode["year"].InnerText}"));
            }

            objNode.TryGetBoolFieldQuickly("incharacter", ref _inCharacter);
            objNode.TryGetStringFieldQuickly("notes", ref _notes);
            _foreColor = Color.FromName(objNode["forecolor"]?.InnerText ?? "Empty");
            _backgroundColor = Color.FromName(objNode["backgroundcolor"]?.InnerText ?? "Empty");
            _backgroundColorLighter = Color.FromName(objNode["backgroundcolorlighter"]?.InnerText ?? "Empty");
            _patternColor = Color.FromName(objNode["patterncolor"]?.InnerText ?? "Empty");
            Enum.TryParse(objNode["pattern"]?.InnerText, out _pattern);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, bool blnPrintNotes = true)
        {
            objWriter.WriteStartElement("entry");
            objWriter.WriteElementString("startdate", StartDate.ToString(objCulture));
            objWriter.WriteElementString("enddate", EndDate.ToString(objCulture));
            if (blnPrintNotes)
                objWriter.WriteElementString("notes", Text);
            objWriter.WriteEndElement();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarItem"/> class.
        /// </summary>
        /// <param name="calendar"></param>
        public CalendarItem(Calendar calendar = null)
            : base(calendar)
        {
            UnitsPassing = new List<CalendarTimeScaleUnit>();
            TopsPassing = new List<CalendarDayTop>();
            BackgroundColor = Color.Empty;
            BorderColor = Color.Empty;
            ForeColor = Color.Empty;
            BackgroundColorLighter = Color.Empty;
            ImageAlign = CalendarItemImageAlign.West;
            _guiID = Guid.NewGuid();
            if (calendar == null) return;
            Font = calendar.ItemsFont;
            BackgroundColor = calendar.ItemsBackgroundColor;
            ForeColor = calendar.ItemsForeColor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarItem"/> class.
        /// </summary>
        /// <param name="calendar">The calendar.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="text">The text.</param>
        public CalendarItem(Calendar calendar, DateTime startDate, DateTime endDate, string text)
            : this(calendar)
        {
            StartDate = startDate;
            EndDate = endDate;
            Text = text;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarItem"/> class.
        /// </summary>
        /// <param name="calendar">The calendar.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="text">The text.</param>
        public CalendarItem(Calendar calendar, DateTime startDate, TimeSpan duration, string text)
            : this(calendar, startDate, startDate.Add(duration), text)
        { }

        public CalendarItem() : this(null)
        {
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Applies color to background, border, and forecolor, from the specified color.
        /// </summary>
        /// <param name="color">The color.</param>
        public void ApplyColor(Color color)
        {
            BackgroundColor = color;
            BackgroundColorLighter = Color.FromArgb(
                color.R + (255 - color.R) / 2 + (255 - color.R) / 3,
                color.G + (255 - color.G) / 2 + (255 - color.G) / 3,
                color.B + (255 - color.B) / 2 + (255 - color.B) / 3);

            BorderColor = Color.FromArgb(
                Convert.ToInt32(Convert.ToSingle(color.R) * .8f),
                Convert.ToInt32(Convert.ToSingle(color.G) * .8f),
                Convert.ToInt32(Convert.ToSingle(color.B) * .8f));

            int avg = (color.R + color.G + color.B) / 3;

            ForeColor = avg > 255 / 2 ? Color.Black : Color.White;
        }

        /// <summary>
        /// Gets all the bounds related to the item.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Items that are broken on two or more weeks may have more than one rectangle bounds.
        /// </remarks>
        public IEnumerable<Rectangle> GetAllBounds()
        {
            List<Rectangle> r = new List<Rectangle>(AditionalBounds ?? new Rectangle[] { }) {Bounds};

            r.Sort(CompareBounds);

            return r;
        }

        /// <summary>
        /// Removes all specific coloring for the item.
        /// </summary>
        public void RemoveColors()
        {
            BackgroundColor = Color.Empty;
            ForeColor = Color.Empty;
            BorderColor = Color.Empty;
        }

        /// <summary>
        /// Gets a value indicating if the specified point is in a resize zone of <see cref="StartDate"/>
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public bool ResizeStartDateZone(Point point)
        {
            int margin = 4;

            List<Rectangle> rects = new List<Rectangle>(GetAllBounds());
            Rectangle first = rects[0];
            Rectangle last = rects[rects.Count - 1];

            if (IsOnDayTop || Calendar.DaysMode == CalendarDaysMode.Short)
            {
                return Rectangle.FromLTRB(first.Left, first.Top, first.Left + margin, first.Bottom).Contains(point);
            }

            return Rectangle.FromLTRB(first.Left, first.Top, first.Right, first.Top + margin).Contains(point);
        }

        /// <summary>
        /// Gets a value indicating if the specified point is in a resize zone of <see cref="EndDate"/>
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public bool ResizeEndDateZone(Point point)
        {
            int margin = 4;

            List<Rectangle> rects = new List<Rectangle>(GetAllBounds());
            Rectangle first = rects[0];
            Rectangle last = rects[rects.Count - 1];

            if (IsOnDayTop || Calendar.DaysMode == CalendarDaysMode.Short)
            {
                return Rectangle.FromLTRB(last.Right - margin, last.Top, last.Right, last.Bottom).Contains(point);
            }

            return Rectangle.FromLTRB(last.Left, last.Bottom - margin, last.Right, last.Bottom).Contains(point);
        }

        /// <summary>
        /// Sets the bounds of the item
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        public new void SetBounds(Rectangle rectangle)
        {
            base.SetBounds(rectangle);
        }

        /// <summary>
        /// Indicates if the time of the item intersects with the provided time
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns></returns>
        public bool IntersectsWith(TimeSpan startTime, TimeSpan endTime)
        {
            Rectangle r1 = Rectangle.FromLTRB(0, Convert.ToInt32(StartDate.TimeOfDay.TotalMinutes), 5, Convert.ToInt32(EndDate.TimeOfDay.TotalMinutes));
            Rectangle r2 = Rectangle.FromLTRB(0, Convert.ToInt32(startTime.TotalMinutes), 5, Convert.ToInt32(endTime.TotalMinutes - 1));
            return r1.IntersectsWith(r2);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{StartDate.ToShortTimeString()} - {EndDate.ToShortTimeString()}";
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds bounds for the item
        /// </summary>
        /// <param name="r"></param>
        internal void AddBounds(Rectangle r)
        {
            if (r.IsEmpty) throw new ArgumentException("r can't be empty");

            if (Bounds.IsEmpty)
            {
                SetBounds(r);
            }
            else
            {
                List<Rectangle> rs = new List<Rectangle>(AditionalBounds ?? new Rectangle[] { });
                rs.Add(r);
                AditionalBounds = rs.ToArray();
            }
        }

        /// <summary>
        /// Adds the specified unit as a passing unit
        /// </summary>
        /// <param name="calendarTimeScaleUnit"></param>
        internal void AddUnitPassing(CalendarTimeScaleUnit calendarTimeScaleUnit)
        {
            if (!UnitsPassing.Contains(calendarTimeScaleUnit))
            {
                UnitsPassing.Add(calendarTimeScaleUnit);
            }
        }

        /// <summary>
        /// Adds the specified <see cref="CalendarDayTop"/> as a passing one
        /// </summary>
        /// <param name="top"></param>
        internal void AddTopPassing(CalendarDayTop top)
        {
            if (!TopsPassing.Contains(top))
            {
                TopsPassing.Add(top);
            }
        }

        /// <summary>
        /// Clears the item's existance off passing units and tops
        /// </summary>
        internal void ClearPassings()
        {
            foreach (CalendarTimeScaleUnit unit in UnitsPassing)
            {
                unit.ClearItemExistance(this);
            }

            UnitsPassing.Clear();
            TopsPassing.Clear();
        }

        /// <summary>
        /// Clears all bounds of the item
        /// </summary>
        internal void ClearBounds()
        {
            SetBounds(Rectangle.Empty);
            AditionalBounds = new Rectangle[] { };
            SetMinuteStartTop(0);
            SetMinuteEndTop(0);
        }

        /// <summary>
        /// It pushes the left and the right to the center of the item
        /// to visually indicate start and end time
        /// </summary>
        internal void FirstAndLastRectangleGapping()
        {
            if (!IsOpenStart)
                SetBounds(Rectangle.FromLTRB(Bounds.Left + Calendar.Renderer.ItemsPadding,
                    Bounds.Top, Bounds.Right, Bounds.Bottom));

            if (!IsOpenEnd)
            {
                if (AditionalBounds != null && AditionalBounds.Length > 0)
                {
                    Rectangle r = AditionalBounds[AditionalBounds.Length - 1];
                    AditionalBounds[AditionalBounds.Length - 1] = Rectangle.FromLTRB(
                        r.Left, r.Top, r.Right - Calendar.Renderer.ItemsPadding, r.Bottom);
                }
                else
                {
                    Rectangle r = Bounds;
                    SetBounds(Rectangle.FromLTRB(
                        r.Left, r.Top, r.Right - Calendar.Renderer.ItemsPadding, r.Bottom));
                }
            }
        }

        /// <summary>
        /// Sets the value of the IsDragging property
        /// </summary>
        /// <param name="dragging">Value indicating if the item is currently being dragged</param>
        internal void SetIsDragging(bool dragging)
        {
            IsDragging = dragging;
        }

        /// <summary>
        /// Sets the value of the <see cref="IsEditing"/> property
        /// </summary>
        /// <param name="editing">Value indicating if user is currently being editing</param>
        internal void SetIsEditing(bool editing)
        {
            IsEditing = editing;
        }

        /// <summary>
        /// Sets the value of the <see cref="IsOnView"/> property
        /// </summary>
        /// <param name="onView">Indicates if the item is currently on view</param>
        internal void SetIsOnView(bool onView)
        {
            IsOnView = onView;
        }

        /// <summary>
        /// Sets the value of the <see cref="IsResizingStartDate"/> property
        /// </summary>
        /// <param name="resizing"></param>
        internal void SetIsResizingStartDate(bool resizing)
        {
            IsResizingStartDate = resizing;
        }

        /// <summary>
        /// Sets the value of the <see cref="IsResizingEndDate"/> property
        /// </summary>
        /// <param name="resizing"></param>
        internal void SetIsResizingEndDate(bool resizing)
        {
            IsResizingEndDate = resizing;
        }

        /// <summary>
        /// Sets the value of the <see cref="MinuteStartTop"/> property
        /// </summary>
        /// <param name="top"></param>
        internal void SetMinuteStartTop(int top)
        {
            MinuteStartTop = top;
        }

        /// <summary>
        /// Sets the value of the <see cref="MinuteEndTop"/> property
        /// </summary>
        /// <param name="top"></param>
        internal void SetMinuteEndTop(int top)
        {
            MinuteEndTop = top;
        }

        internal string DisplayName(string language)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
