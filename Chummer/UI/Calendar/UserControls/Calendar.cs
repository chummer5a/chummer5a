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
    /// Hosts a calendar view where user can manage calendar items.
    /// </summary>
    [DefaultEvent( "LoadItems" )]
    public class Calendar
        : ScrollableControl
    {
        #region Static

        /// <summary>
        /// Returns a value indicating if two date ranges intersect.
        /// </summary>
        /// <param name="startA">The start A.</param>
        /// <param name="endA">The end A.</param>
        /// <param name="startB">The start B.</param>
        /// <param name="endB">The end B.</param>
        /// <returns></returns>
        public static bool DateIntersects( DateTime startA, DateTime endA, DateTime startB, DateTime endB )
        {
            return startB < endA && startA < endB;
        }

        #endregion

        #region Events

        /// <summary>
        /// Delegate that supports <see cref="LoadItems"/> event
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event Data</param>
        public delegate void CalendarLoadEventHandler( object sender, CalendarLoadEventArgs e );

        /// <summary>
        /// Delegate that supports item-related events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CalendarItemEventHandler( object sender, CalendarItemEventArgs e );

        /// <summary>
        /// Delegate that supports cancelable item-related events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CalendarItemCancelEventHandler( object sender, CalendarItemCancelEventArgs e );

        /// <summary>
        /// Delegate that supports <see cref="CalendarDay"/>-related events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CalendarDayEventHandler( object sender, CalendarDayEventArgs e );

        /// <summary>
        /// Occurs when items are load into view
        /// </summary>
        [Description( "Occurs when items are load into view" )]
        public event CalendarLoadEventHandler LoadItems;

        /// <summary>
        /// Occurs when a day header is clicked
        /// </summary>
        [Description( "Occurs when a day header is clicked" )]
        public event CalendarDayEventHandler DayHeaderClick;

        /// <summary>
        /// Occurs when an item is about to be created.
        /// </summary>
        /// <remarks>
        /// Event can be cancelled
        /// </remarks>
        [Description( "Occurs when an item is about to be created." )]
        public event CalendarItemCancelEventHandler ItemCreating;

        /// <summary>
        /// Occurs when an item has been created.
        /// </summary>
        [Description( "Occurs when an item has been created." )]
        public event CalendarItemCancelEventHandler ItemCreated;

        /// <summary>
        /// Occurs before an item is deleted
        /// </summary>
        [Description( "Occurs before an item is deleted" )]
        public event CalendarItemCancelEventHandler ItemDeleting;

        /// <summary>
        /// Occurs when an item has been deleted
        /// </summary>
        [Description( "Occurs when an item has been deleted" )]
        public event CalendarItemEventHandler ItemDeleted;

        /// <summary>
        /// Occurs when an item text is about to be edited
        /// </summary>
        [Description( "Occurs when an item text is about to be edited" )]
        public event CalendarItemCancelEventHandler ItemTextEditing;

        /// <summary>
        /// Occurs when an item text is edited
        /// </summary>
        [Description( "Occurs when an item text is edited" )]
        public event CalendarItemCancelEventHandler ItemTextEdited;

        /// <summary>
        /// Occurs when an item time range has changed
        /// </summary>
        [Description( "Occurs when an item time range has changed" )]
        public event CalendarItemEventHandler ItemDatesChanged;

        /// <summary>
        /// Occurs when an item is clicked
        /// </summary>
        [Description( "Occurs when an item is clicked" )]
        public event CalendarItemEventHandler ItemClick;

        /// <summary>
        /// Occurs when an item is double-clicked
        /// </summary>
        [Description( "Occurs when an item is double-clicked" )]
        public event CalendarItemEventHandler ItemDoubleClick;

        /// <summary>
        /// Occurs when an item is selected
        /// </summary>
        [Description( "Occurs when an item is selected" )]
        public event CalendarItemEventHandler ItemSelected;

        /// <summary>
        /// Occurs after the items are positioned
        /// </summary>
        /// <remarks>
        /// Items bounds can be altered using the <see cref="CalendarItem.SetBounds"/> method.
        /// </remarks>
        [Description( "Occurs after the items are positioned" )]
        public event EventHandler ItemsPositioned;

        /// <summary>
        /// Occurs when the mouse is moved over an item
        /// </summary>
        [Description( "Occurs when the mouse is moved over an item" )]
        public event CalendarItemEventHandler ItemMouseHover;

        #endregion

        #region Fields

        private CalendarTextBox _textBox;
       
        private bool _allowNew;
        private bool _allowItemEdit;
        private bool _allowItemResize;
        private bool _creatingItem;

        private CalendarDay[] _days;
        
        private CalendarDaysMode _daysMode;
        
        private CalendarItem _editModeItem;

        private bool _finalizingEdition;

        private DayOfWeek _firstDayOfWeek;

        private CalendarHighlightRange[] _highlightRanges;
        private CalendarItemCollection _items;

        private string _itemsDateFormat;
        private string _itemsTimeFormat;

        private int _maximumFullDays;
        private int _maximumViewDays;

        private CalendarRenderer _renderer;

        private DateTime _selEnd;
        private DateTime _selStart;

        private CalendarState _state;
        
        private CalendarTimeScale _timeScale;

        private int _timeUnitsOffset;

        private DateTime _viewEnd;
        private DateTime _viewStart;

        private CalendarWeek[] _weeks;

        private List<CalendarSelectableElement> _selectedElements;

        private ICalendarSelectableElement _selectedElementEnd;
        private ICalendarSelectableElement _selectedElementStart;

        private Rectangle _selectedElementSquare;

        private CalendarItem itemOnState;

        private bool itemOnStateChanged;

        private CalendarTimeFormat _timeFormat;

        private CalendarScrollBars _scrollbars;

        private Font _itemsmFont;

        private DateTime _itemsStartViewTime;
        private DateTime _itemEndViewTime;

        private Color _itemsForeColor = Color.Black;
        private Color _itemsBackgroundColor = Color.RoyalBlue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the color of the items fore.
        /// </summary>
        /// <value>
        /// The color of the items fore.
        /// </value>
        [Description( "The default foreground color of the calendar items." )]
        public Color ItemsForeColor
        {
            get { return _itemsForeColor; }
            set { _itemsForeColor = value; }
        }

        /// <summary>
        /// Gets or sets the color of the items background.
        /// </summary>
        /// <value>
        /// The color of the items background.
        /// </value>
        [Description( "The default background color of the calendar items." )]
        public Color ItemsBackgroundColor
        {
            get { return _itemsBackgroundColor; }
            set { _itemsBackgroundColor = value; }
        }

        /// <summary>
        /// Gets or sets the calendar item font.
        /// </summary>
        /// <value>
        /// The calendar item font.
        /// </value>
        [Description("The default font values for all calendar items.  This can be overriden in code.")]
        public Font ItemsFont
        {
            get { return _itemsmFont; }
            set { _itemsmFont = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the control let's the user create new items.
        /// </summary>
        [DefaultValue( true )]
        [Description( "Allows the user to create new items on the view" )]
        public bool AllowNew
        {
            get { return _allowNew; }
            set { _allowNew = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the user can edit the item using the mouse or keyboard
        /// </summary>
        [DefaultValue( true )]
        [Description( "Allows or denies the user the edition of items text or date ranges." )]
        public bool AllowItemEdit
        {
            get { return _allowItemEdit; }
            set { _allowItemEdit = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if calendar allows user to resize the calendar.
        /// </summary>
        [DefaultValue( true )]
        [Description( "Allows or denies the user to resize items on the calendar" )]
        public bool AllowItemResize
        {
            get { return _allowItemResize; }
            set { _allowItemResize = value; }
        }

        /// <summary>
        /// Gets the days visible on the ccurrent view
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public CalendarDay[] Days
        {
            get { return _days; }
        }

        /// <summary>
        /// Gets the mode in which days are drawn.
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public CalendarDaysMode DaysMode
        {
            get { return _daysMode; }
        }

        /// <summary>
        /// Gets the union of day body rectangles
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public Rectangle DaysBodyRectangle
        {
            get
            {
                Rectangle first = Days[0].BodyBounds;
                Rectangle last = Days[Days.Length - 1].BodyBounds;

                return Rectangle.Union( first, last );
            }
        }

        /// <summary>
        /// Gets if the calendar is currently in edit mode of some item
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public bool EditMode
        {
            get { return TextBox != null; }
        }

        /// <summary>
        /// Gets the item being edited (if any)
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public CalendarItem EditModeItem
        {
            get
            {
                return _editModeItem;
            }
        }

        /// <summary>
        /// Gets or sets the first day of weeks
        /// </summary>
        [Description( "Starting day of weeks" )]
        [DefaultValue( DayOfWeek.Sunday )]
        public DayOfWeek FirstDayOfWeek
        {
            set { _firstDayOfWeek = value; }
            get { return _firstDayOfWeek; }
        }

        /// <summary>
        /// Gets or sets the time ranges that should be highlighted as work-time.
        /// This ranges are week based.
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public CalendarHighlightRange[] HighlightRanges
        {
            get { return _highlightRanges; }
            set { _highlightRanges = value; UpdateHighlights(); }
        }

        /// <summary>
        /// Gets the collection of items currently on the view.
        /// </summary>
        /// <remarks>
        /// This collection changes every time the view is changed
        /// </remarks>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public CalendarItemCollection Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Gets or sets the format in which time is shown in the items, when applicable
        /// </summary>
        [DefaultValue( "dd/MMM" )]
        public string ItemsDateFormat
        {
            get { return _itemsDateFormat; }
            set { _itemsDateFormat = value; }
        }

        /// <summary>
        /// Gets or sets the format in which time is shown in the items, when applicable
        /// </summary>
        [DefaultValue( "hh:mm tt" )]
        public string ItemsTimeFormat
        {
            get { return _itemsTimeFormat; }
            set { _itemsTimeFormat = value; }
        }

        /// <summary>
        /// Gets or sets the maximum full days shown on the view. 
        /// After this amount of days, they will be shown as short days.
        /// </summary>
        [DefaultValue( 8 )]
        public int MaximumFullDays
        {
            get { return _maximumFullDays; }
            set { _maximumFullDays = value; }
        }

        /// <summary>
        /// Gets or sets the maximum amount of days supported by the view.
        /// Value must be multiple of 7
        /// </summary>
        [DefaultValue( 35 )]
        public int MaximumViewDays
        {
            get { return _maximumViewDays; }
            set
            {
                if( value % 7 != 0 )
                {
                    throw new Exception( "MaximumViewDays must be multiple of 7" );
                }
                _maximumViewDays = value;
            }
        }

        /// <summary>
        /// Gets or sets the time format.
        /// </summary>
        /// <value>
        /// The time format.
        /// </value>
        [Description( "The format of the calendar time (12 or 24 hour)." )]
        [DefaultValue( CalendarTimeFormat.TwelveHour )]
        public CalendarTimeFormat CalendarTimeFormat
        {
            get { return _timeFormat; }
            set { _timeFormat = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="CalendarRenderer"/> of the <see cref="Calendar"/>
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public CalendarRenderer Renderer
        {
            get { return _renderer; }
            set
            {
                _renderer = value;

                if( value != null && Created )
                {
                    value.OnInitialize( new CalendarRendererEventArgs( null, null, Rectangle.Empty ) );
                }
            }
        }

        /// <summary>
        /// Gets the last selected element
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public ICalendarSelectableElement SelectedElementEnd
        {
            get { return _selectedElementEnd; }
            set
            {
                _selectedElementEnd = value;

                UpdateSelectionElements();
            }
        }

        /// <summary>
        /// Gets the first selected element
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public ICalendarSelectableElement SelectedElementStart
        {
            get { return _selectedElementStart; }
            set
            {
                _selectedElementStart = value;

                UpdateSelectionElements();
            }
        }

        /// <summary>
        /// Gets or sets the end date-time of the view's selection.
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public DateTime SelectionEnd
        {
            get { return _selEnd; }
            set { _selEnd = value; }
        }

        /// <summary>
        /// Gets or sets the start date-time of the view's selection.
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public DateTime SelectionStart
        {
            get { return _selStart; }
            set { _selStart = value; }
        }

        /// <summary>
        /// Gets or sets the state of the calendar
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public CalendarState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Gets or sets the scrollbars.
        /// </summary>
        /// <value>
        /// The scrollbars.
        /// </value>
        [Description( "Does the calendar show scrollbars." )]
        [DefaultValue( CalendarScrollBars.None )]
        public CalendarScrollBars Scrollbars
        {
            get { return _scrollbars; }
            set { _scrollbars = value; }
        }

        /// <summary>
        /// Gets the TextBox of the edit mode
        /// </summary>
        internal CalendarTextBox TextBox
        {
            get { return _textBox; }
            set { _textBox = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="CalendarTimeScale"/> for visualization.
        /// </summary>
        [DefaultValue( CalendarTimeScale.ThirtyMinutes )]
        public CalendarTimeScale TimeScale
        {
            get { return _timeScale; }
            set
            {
                _timeScale = value;

                if( Days != null )
                {
                    for( int i = 0; i < Days.Length; i++ )
                    {
                        Days[i].UpdateUnits();
                    }
                }

                Renderer.PerformLayout();
                Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the offset of scrolled units
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public int TimeUnitsOffset
        {
            get { return _timeUnitsOffset; }
            set
            {
                _timeUnitsOffset = value;
                Renderer.PerformLayout();
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the end date-time of the current view.
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public DateTime ViewEnd
        {
            get { return _viewEnd; }
            set
            {
                _viewEnd = value.Date.Add( new TimeSpan( 23, 59, 59 ) );
                ClearItems();
                UpdateDaysAndWeeks();
                Renderer.PerformLayout();
                Invalidate();
                ReloadItems();
            }
        }

        /// <summary>
        /// Gets or sets the start date-time of the current view.
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public DateTime ViewStart
        {
            get { return _viewStart; }
            set
            {
                _viewStart = value.Date;
                ClearItems();
                UpdateDaysAndWeeks();
                Renderer.PerformLayout();
                Invalidate();
                ReloadItems();
            }
        }

        /// <summary>
        /// Gets the weeks currently visible on the calendar, if <see cref="DaysMode"/> is <see cref="CalendarDaysMode.Short"/>
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
        public CalendarWeek[] Weeks
        {
            get { return _weeks; }
        }

        #endregion

        /// <summary>
        /// Creates a new <see cref="Calendar"/> control
        /// </summary>
        public Calendar()
        {
            SetStyle( ControlStyles.ResizeRedraw, true );
            SetStyle( ControlStyles.Selectable, true );

            DoubleBuffered = true;

            _selectedElements = new List<CalendarSelectableElement>();
            _items = new CalendarItemCollection( this );
            _renderer = new CalendarProfessionalRenderer( this );
            _maximumFullDays = 8;
            _maximumViewDays = 35;

            HighlightRanges = new CalendarHighlightRange[] { 
                new CalendarHighlightRange( DayOfWeek.Monday, new TimeSpan(8,0,0), new TimeSpan(17,0,0)),
                new CalendarHighlightRange( DayOfWeek.Tuesday, new TimeSpan(8,0,0), new TimeSpan(17,0,0)),
                new CalendarHighlightRange( DayOfWeek.Wednesday, new TimeSpan(8,0,0), new TimeSpan(17,0,0)),
                new CalendarHighlightRange( DayOfWeek.Thursday, new TimeSpan(8,0,0), new TimeSpan(17,0,0)),
                new CalendarHighlightRange( DayOfWeek.Friday, new TimeSpan(8,0,0), new TimeSpan(17,0,0)),
            };

            _timeScale = CalendarTimeScale.ThirtyMinutes;
            SetViewRange( DateTime.Now, DateTime.Now.AddDays( 2 ) );


            _itemsDateFormat = "dd/MMM";
            _itemsTimeFormat = "hh:mm tt";
            _allowItemEdit = true;
            _allowNew = true;
            _allowItemResize = true;
        }

        #region Public Methods

        /// <summary>
        /// Activates the edit mode on the first selected item
        /// </summary>
        public void ActivateEditMode()
        {
            foreach( CalendarItem item in Items )
            {
                if( item.Selected )
                {
                    ActivateEditMode( item );
                    return;
                }
            }
        }

        /// <summary>
        /// Activates the edit mode on the specified item
        /// </summary>
        /// <param name="item">The item.</param>
        public void ActivateEditMode( CalendarItem item )
        {
            CalendarItemCancelEventArgs evt = new CalendarItemCancelEventArgs( item );

            if( !_creatingItem )
            {
                OnItemEditing( evt );
            }

            if( evt.Cancel )
            {
                return;
            }

            _editModeItem = item;
            TextBox = new CalendarTextBox( this );
            TextBox.KeyDown += new KeyEventHandler( TextBox_KeyDown );
            TextBox.LostFocus += new EventHandler( TextBox_LostFocus );
            Rectangle r = item.Bounds;
            r.Inflate( -2, -2 );
            TextBox.Bounds = r;
            TextBox.BorderStyle = BorderStyle.None;
            TextBox.Text = item.Text;
            TextBox.Multiline = true;

            Controls.Add( TextBox );
            TextBox.Visible = true;
            TextBox.Focus();
            TextBox.SelectionStart = TextBox.Text.Length;

            SetState( CalendarState.EditingItemText );
        }

        /// <summary>
        /// Creates a new item on the current selection. 
        /// If there's no selection, this will be ignored.
        /// </summary>
        /// <param name="itemText">Text of the item</param>
        /// <param name="editMode">If <c>true</c> activates the edit mode so user can edit the text of the item.</param>
        public void CreateItemOnSelection( string itemText, bool editMode )
        {
            if( SelectedElementEnd == null || SelectedElementStart == null ) return;

            CalendarTimeScaleUnit unitEnd = SelectedElementEnd as CalendarTimeScaleUnit;
            CalendarDayTop dayTop = SelectedElementEnd as CalendarDayTop;
            CalendarDay day = SelectedElementEnd as CalendarDay;
            TimeSpan duration = unitEnd != null ? unitEnd.Duration : new TimeSpan( 23, 59, 59 );
            CalendarItem item = new CalendarItem( this );

            DateTime dstart = SelectedElementStart.Date;
            DateTime dend = SelectedElementEnd.Date;

            if( dend.CompareTo( dstart ) < 0 )
            {
                DateTime dtmp = dend;
                dend = dstart;
                dstart = dtmp;
            }

            item.StartDate = dstart;
            item.EndDate = dend.Add( duration );
            item.Text = itemText;

            CalendarItemCancelEventArgs evtA = new CalendarItemCancelEventArgs( item );

            OnItemCreating( evtA );

            if( !evtA.Cancel )
            {
                // set the default font, developers can change this anytime via override
                Items.Add( item );

                if( editMode )
                {
                    _creatingItem = true;
                    ActivateEditMode( item );
                }
            }


        }

        /// <summary>
        /// Ensures the scrolling shows the specified time unit. It doesn't affect View date ranges.
        /// </summary>
        /// <param name="unit">Unit to ensure visibility</param>
        public void EnsureVisible( CalendarTimeScaleUnit unit )
        {
            if( Days == null || Days.Length == 0 ) return;

            Rectangle view = Days[0].BodyBounds;

            if( unit.Bounds.Bottom > view.Bottom )
            {
                TimeUnitsOffset = -Convert.ToInt32( Math.Ceiling( unit.Date.TimeOfDay.TotalMinutes / (double)TimeScale ) )
                     + Renderer.GetVisibleTimeUnits();
            }
            else if( unit.Bounds.Top < view.Top )
            {
                TimeUnitsOffset = -Convert.ToInt32( Math.Ceiling( unit.Date.TimeOfDay.TotalMinutes / (double)TimeScale ) );
            }
        }

        /// <summary>
        /// Finalizes editing the <see cref="EditModeItem"/>.
        /// </summary>
        /// <param name="cancel">Value indicating if edition of item should be canceled.</param>
        public void FinalizeEditMode( bool cancel )
        {
            if( !EditMode || EditModeItem == null || _finalizingEdition ) return;

            _finalizingEdition = true;

            string cancelText = _editModeItem.Text;
            CalendarItem itemBuffer = _editModeItem;
            _editModeItem = null;
            CalendarItemCancelEventArgs evt = new CalendarItemCancelEventArgs( itemBuffer );

            if( !cancel )
                itemBuffer.Text = TextBox.Text.Trim();

            if( TextBox != null )
            {
                TextBox.Visible = false;
                Controls.Remove( TextBox );
                TextBox.Dispose();
            }

            if( _editModeItem != null )
                Invalidate( itemBuffer );

            _textBox = null;

            if( _creatingItem )
            {
                OnItemCreated( evt );
            }
            else
            {
                OnItemEdited( evt );
            }

            if( evt.Cancel )
            {
                itemBuffer.Text = cancelText;
            }


            _creatingItem = false;
            _finalizingEdition = false;

            if( State == CalendarState.EditingItemText )
            {
                SetState( CalendarState.Idle );
            }
        }

        /// <summary>
        /// Finds the <see cref="CalendarDay"/> for the specified date, if in the view.
        /// </summary>
        /// <param name="d">Date to find day</param>
        /// <returns><see cref="CalendarDay"/> object that matches the date, <c>null</c> if day was not found.</returns>
        public CalendarDay FindDay( DateTime d )
        {
            if( Days == null ) return null;

            for( int i = 0; i < Days.Length; i++ )
            {
                if( Days[i].Date.Date.Equals( d.Date.Date ) )
                {
                    return Days[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the items that are currently selected
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CalendarItem> GetSelectedItems()
        {
            List<CalendarItem> items = new List<CalendarItem>();

            foreach( CalendarItem item in Items )
            {
                if( item.Selected )
                {
                    items.Add( item );
                }
            }

            return items;
        }

        /// <summary>
        /// Gets the time unit that starts with the specified date
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>
        /// Matching time unit. <c>null</c> If out of range.
        /// </returns>
        public CalendarTimeScaleUnit GetTimeUnit( DateTime d )
        {
            if( Days != null )
            {
                foreach( CalendarDay day in Days )
                {
                    if( day.Date.Equals( d.Date ) )
                    {
                        double duration = Convert.ToDouble( (int)TimeScale );
                        int index =
                            Convert.ToInt32(
                                Math.Floor(
                                    d.TimeOfDay.TotalMinutes / duration
                                )
                            );

                        return day.TimeUnits[index];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Hits the test.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        public ICalendarSelectableElement HitTest( Point p )
        {
            return HitTest( p, false );
        }

        /// <summary>
        /// Searches for the first hitted <see cref="ICalendarSelectableElement"/>
        /// </summary>
        /// <param name="p">Point to check for hit test</param>
        /// <param name="ignoreItems">if set to <c>true</c> [ignore items].</param>
        /// <returns></returns>
        public ICalendarSelectableElement HitTest( Point p, bool ignoreItems )
        {
            if( !ignoreItems )
                foreach( CalendarItem item in Items )
                {
                    foreach( Rectangle r in item.GetAllBounds() )
                    {
                        if( r.Contains( p ) )
                        {
                            return item;
                        }
                    }
                }

            for( int i = 0; i < Days.Length; i++ )
            {
                if( Days[i].Bounds.Contains( p ) )
                {
                    if( DaysMode == CalendarDaysMode.Expanded )
                    {
                        if( Days[i].DayTop.Bounds.Contains( p ) )
                        {
                            return Days[i].DayTop;
                        }
                        else
                        {
                            for( int j = 0; j < Days[i].TimeUnits.Length; j++ )
                            {
                                if( Days[i].TimeUnits[j].Visible &&
                                    Days[i].TimeUnits[j].Bounds.Contains( p ) )
                                {
                                    return Days[i].TimeUnits[j];
                                }
                            }
                        }

                        return Days[i];
                    }
                    else if( DaysMode == CalendarDaysMode.Short )
                    {
                        return Days[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the item hitted at the specified location. Null if no item hitted.
        /// </summary>
        /// <param name="p">Location to serach for items</param>
        /// <returns>Hitted item at the location. Null if no item hitted.</returns>
        public CalendarItem ItemAt( Point p )
        {
            return HitTest( p ) as CalendarItem;
        }

        /// <summary>
        /// Invalidates the bounds of the specified day
        /// </summary>
        /// <param name="day">The day.</param>
        public void Invalidate( CalendarDay day )
        {
            Invalidate( day.Bounds );
        }

        /// <summary>
        /// Ivalidates the bounds of the specified unit
        /// </summary>
        /// <param name="unit">The unit.</param>
        public void Invalidate( CalendarTimeScaleUnit unit )
        {
            Invalidate( unit.Bounds );
        }

        /// <summary>
        /// Invalidates the area of the specified item
        /// </summary>
        /// <param name="item">The item.</param>
        public void Invalidate( CalendarItem item )
        {
            Rectangle r = item.Bounds;

            foreach( Rectangle bounds in item.GetAllBounds() )
            {
                r = Rectangle.Union( r, bounds );
            }

            r.Inflate( Renderer.ItemShadowPadding + Renderer.ItemInvalidateMargin, Renderer.ItemShadowPadding + Renderer.ItemInvalidateMargin );
            Invalidate( r );
        }

        /// <summary>
        /// Establishes the selection range with only one graphical update.
        /// </summary>
        /// <param name="selectionStart">Fisrt selected element</param>
        /// <param name="selectionEnd">Last selection element</param>
        public void SetSelectionRange( ICalendarSelectableElement selectionStart, ICalendarSelectableElement selectionEnd )
        {
            _selectedElementStart = selectionStart;
            SelectedElementEnd = selectionEnd;
        }

        /// <summary>
        /// Sets the value of <see cref="ViewStart"/> and <see cref="ViewEnd"/> properties
        /// triggering only one repaint process
        /// </summary>
        /// <param name="dateStart">Start date of view</param>
        /// <param name="dateEnd">End date of view</param>
        public void SetViewRange( DateTime dateStart, DateTime dateEnd )
        {
            _viewStart = dateStart.Date;
            ViewEnd = dateEnd;
        }

        /// <summary>
        /// Returns a value indicating if the view range intersects the specified date range.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public bool ViewIntersects( DateTime startDate, DateTime endDate )
        {
            return DateIntersects( ViewStart, ViewEnd, startDate, endDate );
        }

        /// <summary>
        /// Returns a value indicating if the view range intersect the date range of the specified item
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool ViewIntersects( CalendarItem item )
        {
            return ViewIntersects( item.StartDate, item.EndDate );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines whether the specified key is a regular input key or a special key that requires preprocessing.
        /// </summary>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"/> values.</param>
        /// <returns>
        /// true if the specified key is a regular input key; otherwise, false.
        /// </returns>
        protected override bool IsInputKey( Keys keyData )
        {
            if(
                keyData == Keys.Down ||
                keyData == Keys.Up ||
                keyData == Keys.Right ||
                keyData == Keys.Left )
            {
                return true;
            }
            else
            {

                return base.IsInputKey( keyData );
            }
        }

        /// <summary>
        /// Removes all the items currently on the calendar
        /// </summary>
        private void ClearItems()
        {
            Items.Clear();
            Renderer.DayTopHeight = Renderer.DayTopMinHeight;
        }

        /// <summary>
        /// Unselects the selected items
        /// </summary>
        private void ClearSelectedItems()
        {
            Rectangle r = Rectangle.Empty;

            foreach( CalendarItem item in Items )
            {
                if( item.Selected )
                {
                    if( r.IsEmpty )
                    {
                        r = item.Bounds;
                    }
                    else
                    {
                        r = Rectangle.Union( r, item.Bounds );
                    }
                }

                item.SetSelected( false );
            }

            Invalidate( r );
        }

        /// <summary>
        /// Deletes the currently selected item
        /// </summary>
        private void DeleteSelectedItems()
        {
            Stack<CalendarItem> toDelete = new Stack<CalendarItem>();

            foreach( CalendarItem item in Items )
            {
                if( item.Selected )
                {
                    CalendarItemCancelEventArgs evt = new CalendarItemCancelEventArgs( item );

                    OnItemDeleting( evt );

                    if( !evt.Cancel )
                    {
                        toDelete.Push( item );
                    }
                }
            }

            if( toDelete.Count > 0 )
            {
                while( toDelete.Count > 0 )
                {
                    CalendarItem item = toDelete.Pop();

                    Items.Remove( item );

                    OnItemDeleted( new CalendarItemEventArgs( item ) );
                }

                Renderer.PerformItemsLayout();
            }
        }

        /// <summary>
        /// Clears current items and reloads for specified view
        /// </summary>
        private void ReloadItems()
        {
            OnLoadItems( new CalendarLoadEventArgs( this, ViewStart, ViewEnd ) );
        }

        /// <summary>
        /// Grows the rectangle to repaint currently selected elements
        /// </summary>
        /// <param name="rect">The rect.</param>
        private void GrowSquare( Rectangle rect )
        {
            if( _selectedElementSquare.IsEmpty )
            {
                _selectedElementSquare = rect;
            }
            else
            {
                _selectedElementSquare = Rectangle.Union( _selectedElementSquare, rect );
            }
        }

        /// <summary>
        /// Clears selection of currently selected components (As quick as possible)
        /// </summary>
        private void ClearSelectedComponents()
        {
            foreach( CalendarSelectableElement element in _selectedElements )
            {
                element.SetSelected( false );
            }

            _selectedElements.Clear();

            Invalidate( _selectedElementSquare );
            _selectedElementSquare = Rectangle.Empty;

        }

        /// <summary>
        /// Scrolls the calendar using the specified delta
        /// </summary>
        /// <param name="delta">The delta.</param>
        private void ScrollCalendar( int delta )
        {
            if( delta < 0 )
            {
                SetViewRange( ViewStart.AddDays( 7 ), ViewEnd.AddDays( 7 ) );
            }
            else
            {
                SetViewRange( ViewStart.AddDays( -7 ), ViewEnd.AddDays( -7 ) );
            }
        }

        /// <summary>
        /// Raises the <see cref="ItemsPositioned"/> event
        /// </summary>
        internal void RaiseItemsPositioned()
        {
            OnItemsPositioned( EventArgs.Empty );
        }

        /// <summary>
        /// Scrolls the time units using the specified delta
        /// </summary>
        /// <param name="delta">The delta.</param>
        private void ScrollTimeUnits( int delta )
        {
            int possible = TimeUnitsOffset;
            int visible = Renderer.GetVisibleTimeUnits();

            if( delta < 0 )
            {
                possible--;
            }
            else
            {
                possible++;
            }

            if( possible > 0 )
            {
                possible = 0;
            }
            else if(
                Days != null
                && Days.Length > 0
                && Days[0].TimeUnits != null
                && possible * -1 >= Days[0].TimeUnits.Length )
            {
                possible = Days[0].TimeUnits.Length - 1;
                possible *= -1;
            }
            else if( Days != null
               && Days.Length > 0
               && Days[0].TimeUnits != null )
            {
                int max = Days[0].TimeUnits.Length - visible;
                max *= -1;
                if( possible < max ) possible = max;
            }

            if( possible != TimeUnitsOffset )
            {
                TimeUnitsOffset = possible;
            }
        }

        /// <summary>
        /// Sets the value of the <see cref="DaysMode"/> property.
        /// </summary>
        /// <param name="mode">Mode in which days will be rendered</param>
        private void SetDaysMode( CalendarDaysMode mode )
        {
            _daysMode = mode;
        }

        /// <summary>
        /// Sets the state.
        /// </summary>
        /// <param name="state">The state.</param>
        private void SetState( CalendarState state )
        {
            _state = state;
        }

        /// <summary>
        /// Handles the LostFocus event of the TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void TextBox_LostFocus( object sender, EventArgs e )
        {
            FinalizeEditMode( false );
        }

        /// <summary>
        /// Handles the KeyDown event of the TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        private void TextBox_KeyDown( object sender, KeyEventArgs e )
        {
            if( e.KeyCode == Keys.Escape )
            {
                FinalizeEditMode( true );
            }
            else if( e.KeyCode == Keys.Enter )
            {
                FinalizeEditMode( false );
            }
        }

        /// <summary>
        /// Updates the days and weeks.
        /// </summary>
        private void UpdateDaysAndWeeks()
        {
            TimeSpan span = ( new DateTime( ViewEnd.Year, ViewEnd.Month, ViewEnd.Day, 23, 59, 59 ) ).Subtract( ViewStart.Date );
            int preDays = 0;
            span = span.Add( new TimeSpan( 0, 0, 0, 1, 0 ) );

            if( span.Days < 1 || span.Days > MaximumViewDays )
            {
                throw new Exception( "Days between ViewStart and ViewEnd should be between 1 and MaximumViewDays" );
            }

            if( span.Days > MaximumFullDays )
            {
                SetDaysMode( CalendarDaysMode.Short );
                preDays = ( new int[] { 0, 1, 2, 3, 4, 5, 6 } )[(int)ViewStart.DayOfWeek] - (int)FirstDayOfWeek;
                span = span.Add( new TimeSpan( preDays, 0, 0, 0 ) );

                while( span.Days % 7 != 0 )
                    span = span.Add( new TimeSpan( 1, 0, 0, 0 ) );
            }
            else
            {
                SetDaysMode( CalendarDaysMode.Expanded );
            }

            _days = new CalendarDay[span.Days];

            for( int i = 0; i < Days.Length; i++ )
                Days[i] = new CalendarDay( this, ViewStart.AddDays( -preDays + i ), i );


            //Weeks
            if( DaysMode == CalendarDaysMode.Short )
            {
                List<CalendarWeek> weeks = new List<CalendarWeek>();

                for( int i = 0; i < Days.Length; i++ )
                {
                    if( Days[i].Date.DayOfWeek == FirstDayOfWeek )
                    {
                        weeks.Add( new CalendarWeek( this, Days[i].Date ) );
                    }
                }

                _weeks = weeks.ToArray();
            }
            else
            {
                _weeks = new CalendarWeek[] { };
            }

            UpdateHighlights();

        }

        /// <summary>
        /// Updates the value of the <see cref="CalendarTimeScaleUnit.Highlighted"/> property on the time units of days.
        /// </summary>
        internal void UpdateHighlights()
        {
            if( Days == null ) return;

            for( int i = 0; i < Days.Length; i++ )
            {
                Days[i].UpdateHighlights();
            }
        }

        /// <summary>
        /// Informs elements who's selected and who's not, and repaints <see cref="_selectedElementSquare"/>
        /// </summary>
        private void UpdateSelectionElements()
        {
            CalendarTimeScaleUnit unitStart = _selectedElementStart as CalendarTimeScaleUnit;
            CalendarDayTop topStart = _selectedElementStart as CalendarDayTop;
            CalendarDay dayStart = _selectedElementStart as CalendarDay;
            CalendarTimeScaleUnit unitEnd = _selectedElementEnd as CalendarTimeScaleUnit;
            CalendarDayTop topEnd = _selectedElementEnd as CalendarDayTop;
            CalendarDay dayEnd = _selectedElementEnd as CalendarDay;

            ClearSelectedComponents();

            if( _selectedElementEnd == null || _selectedElementStart == null ) return;

            if( _selectedElementEnd.CompareTo( SelectedElementStart ) < 0 )
            {
                //swap
                unitStart = _selectedElementEnd as CalendarTimeScaleUnit;
                topStart = _selectedElementEnd as CalendarDayTop;
                dayStart = _selectedElementEnd as CalendarDay;
                unitEnd = SelectedElementStart as CalendarTimeScaleUnit;
                topEnd = SelectedElementStart as CalendarDayTop;
                dayEnd = _selectedElementStart as CalendarDay;
            }

            if( unitStart != null && unitEnd != null )
            {
                bool reached = false;
                for( int i = unitStart.Day.Index; !reached; i++ )
                {
                    for( int j = ( i == unitStart.Day.Index ? unitStart.Index : 0 ); i < Days.Length && j < Days[i].TimeUnits.Length; j++ )
                    {
                        CalendarTimeScaleUnit unit = Days[i].TimeUnits[j];
                        unit.SetSelected( true );
                        GrowSquare( unit.Bounds );
                        _selectedElements.Add( unit );

                        if( unit.Equals( unitEnd ) )
                        {
                            reached = true;
                            break;
                        }
                    }
                }
            }
            else if( topStart != null && topEnd != null )
            {
                for( int i = topStart.Day.Index; i <= topEnd.Day.Index; i++ )
                {
                    CalendarDayTop top = Days[i].DayTop;

                    top.SetSelected( true );
                    GrowSquare( top.Bounds );
                    _selectedElements.Add( top );
                }
            }
            else if( dayStart != null && dayEnd != null )
            {
                for( int i = dayStart.Index; i <= dayEnd.Index; i++ )
                {
                    CalendarDay day = Days[i];

                    day.SetSelected( true );
                    GrowSquare( day.Bounds );
                    _selectedElements.Add( day );
                }
            }

            Invalidate( _selectedElementSquare );
        }

        #endregion

        #region Overrided Events and Raisers

        /// <summary>
        /// Raises the <see cref="M:System.Windows.Forms.Control.CreateControl"/> method.
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            Renderer.OnInitialize( new CalendarRendererEventArgs( new CalendarRendererEventArgs( this, null, Rectangle.Empty ) ) );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Click"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnClick( EventArgs e )
        {
            base.OnClick( e );

            Select();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.DoubleClick"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnDoubleClick( EventArgs e )
        {
            base.OnDoubleClick( e );

            CreateItemOnSelection( string.Empty, true );
        }

        /// <summary>
        /// Raises the <see cref="E:DayHeaderClick"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarDayEventArgs"/> instance containing the event data.</param>
        protected virtual void OnDayHeaderClick( CalendarDayEventArgs e )
        {
            DayHeaderClick?.Invoke( this, e );
        }

        /// <summary>
        /// Raises the <see cref="E:ItemClick"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarItemEventArgs"/> instance containing the event data.</param>
        protected virtual void OnItemClick( CalendarItemEventArgs e )
        {
            ItemClick?.Invoke( this, e );
        }

        /// <summary>
        /// Raises the <see cref="E:ItemCreating"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarItemCancelEventArgs"/> instance containing the event data.</param>
        protected virtual void OnItemCreating( CalendarItemCancelEventArgs e )
        {
            ItemCreating?.Invoke( this, e );
        }

        /// <summary>
        /// Raises the <see cref="E:ItemCreated"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarItemCancelEventArgs"/> instance containing the event data.</param>
        protected virtual void OnItemCreated( CalendarItemCancelEventArgs e )
        {
            ItemCreated?.Invoke( this, e );
        }

        /// <summary>
        /// Raises the <see cref="E:ItemDeleting"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarItemCancelEventArgs"/> instance containing the event data.</param>
        protected virtual void OnItemDeleting( CalendarItemCancelEventArgs e )
        {
            ItemDeleting?.Invoke( this, e );
        }

        /// <summary>
        /// Raises the <see cref="E:ItemDeleted"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarItemEventArgs"/> instance containing the event data.</param>
        protected virtual void OnItemDeleted( CalendarItemEventArgs e )
        {
            ItemDeleted?.Invoke( this, e );
        }

        /// <summary>
        /// Raises the <see cref="E:ItemDoubleClick"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarItemEventArgs"/> instance containing the event data.</param>
        protected virtual void OnItemDoubleClick( CalendarItemEventArgs e )
        {
            ItemDoubleClick?.Invoke( this, e );
        }

        /// <summary>
        /// Raises the <see cref="E:ItemEditing"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarItemCancelEventArgs"/> instance containing the event data.</param>
        protected virtual void OnItemEditing( CalendarItemCancelEventArgs e )
        {
            ItemTextEditing?.Invoke( this, e );
        }

        /// <summary>
        /// Raises the <see cref="E:ItemEdited"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarItemCancelEventArgs"/> instance containing the event data.</param>
        protected virtual void OnItemEdited( CalendarItemCancelEventArgs e )
        {
            ItemTextEdited?.Invoke( this, e );
        }

        /// <summary>
        /// Raises the <see cref="E:ItemSelected"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarItemEventArgs"/> instance containing the event data.</param>
        protected virtual void OnItemSelected( CalendarItemEventArgs e )
        {
            ItemSelected?.Invoke( this, e );
        }

        /// <summary>
        /// Raises the <see cref="E:ItemsPositioned"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnItemsPositioned( EventArgs e )
        {
            ItemsPositioned?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:ItemDatesChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarItemEventArgs"/> instance containing the event data.</param>
        protected virtual void OnItemDatesChanged( CalendarItemEventArgs e )
        {
            ItemDatesChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:ItemMouseHover"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarItemEventArgs"/> instance containing the event data.</param>
        protected virtual void OnItemMouseHover( CalendarItemEventArgs e )
        {
            ItemMouseHover?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyDown( KeyEventArgs e )
        {
            base.OnKeyDown( e );

            bool shiftPressed = ( ModifierKeys & Keys.Shift ) == Keys.Shift;
            int jump = (int)TimeScale;
            ICalendarSelectableElement sStart = null;
            ICalendarSelectableElement sEnd = null;

            if( e.KeyCode == Keys.F2 )
            {
                ActivateEditMode();
            }
            else if( e.KeyCode == Keys.Delete )
            {
                DeleteSelectedItems();
            }
            else if( e.KeyCode == Keys.Insert )
            {
                if( AllowNew )
                    CreateItemOnSelection( string.Empty, true );
            }
            else if( e.KeyCode == Keys.Down )
            {
                if( e.Shift )
                    sStart = SelectedElementStart;

                sEnd = GetTimeUnit( SelectedElementEnd.Date.Add( new TimeSpan( 0, (int)TimeScale, 0 ) ) );
            }
            else if( e.KeyCode == Keys.Up )
            {
                if( e.Shift )
                    sStart = SelectedElementStart;

                sEnd = GetTimeUnit( SelectedElementEnd.Date.Add( new TimeSpan( 0, -(int)TimeScale, 0 ) ) );
            }
            else if( e.KeyCode == Keys.Right )
            {
                sEnd = GetTimeUnit( SelectedElementEnd.Date.Add( new TimeSpan( 24, 0, 0 ) ) );
            }
            else if( e.KeyCode == Keys.Left )
            {
                sEnd = GetTimeUnit( SelectedElementEnd.Date.Add( new TimeSpan( -24, 0, 0 ) ) );
            }
            else if( e.KeyCode == Keys.PageDown )
            {

            }
            else if( e.KeyCode == Keys.PageUp )
            {

            }


            if( sStart != null )
            {
                SetSelectionRange( sStart, sEnd );
            }
            else if( sEnd != null )
            {
                SetSelectionRange( sEnd, sEnd );

                if( sEnd is CalendarTimeScaleUnit )
                    EnsureVisible( sEnd as CalendarTimeScaleUnit );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyPress"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyPressEventArgs"/> that contains the event data.</param>
        protected override void OnKeyPress( KeyPressEventArgs e )
        {
            base.OnKeyPress( e );

            if( AllowNew )
                CreateItemOnSelection( e.KeyChar.ToString(), true );
        }

        /// <summary>
        /// Raises the <see cref="E:LoadItems"/> event.
        /// </summary>
        /// <param name="e">The <see cref="WindowsFormsCalendar.CalendarLoadEventArgs"/> instance containing the event data.</param>
        protected virtual void OnLoadItems( CalendarLoadEventArgs e )
        {
            LoadItems?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDoubleClick"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseDoubleClick( MouseEventArgs e )
        {
            base.OnMouseDoubleClick( e );

            CalendarItem item = ItemAt( e.Location );

            if( item != null )
            {
                OnItemDoubleClick( new CalendarItemEventArgs( item ) );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseDown( MouseEventArgs e )
        {
            base.OnMouseDown( e );

            ICalendarSelectableElement hitted = HitTest( e.Location );
            CalendarItem hittedItem = hitted as CalendarItem;
            bool shiftPressed = ( ModifierKeys & Keys.Shift ) == Keys.Shift;

            if( !Focused )
            {
                Focus();
            }

            switch( State )
            {
                case CalendarState.Idle:
                    if( hittedItem != null )
                    {
                        if( !shiftPressed )
                            ClearSelectedItems();

                        hittedItem.SetSelected( true );
                        Invalidate( hittedItem );
                        OnItemSelected( new CalendarItemEventArgs( hittedItem ) );

                        itemOnState = hittedItem;
                        itemOnStateChanged = false;

                        if( AllowItemEdit )
                        {
                            if( itemOnState.ResizeStartDateZone( e.Location ) && AllowItemResize )
                            {
                                SetState( CalendarState.ResizingItem );
                                itemOnState.SetIsResizingStartDate( true );
                            }
                            else if( itemOnState.ResizeEndDateZone( e.Location ) && AllowItemResize )
                            {
                                SetState( CalendarState.ResizingItem );
                                itemOnState.SetIsResizingEndDate( true );
                            }
                            else
                            {
                                SetState( CalendarState.DraggingItem );
                            }
                        }

                        SetSelectionRange( null, null );
                    }
                    else
                    {
                        ClearSelectedItems();

                        if( shiftPressed )
                        {
                            if( hitted != null && SelectedElementEnd == null && !SelectedElementEnd.Equals( hitted ) )
                                SelectedElementEnd = hitted;
                        }
                        else
                        {
                            if( SelectedElementStart == null || ( hitted != null && !SelectedElementStart.Equals( hitted ) ) )
                            {
                                SetSelectionRange( hitted, hitted );
                            }
                        }

                        SetState( CalendarState.DraggingTimeSelection );
                    }
                    break;
                case CalendarState.DraggingTimeSelection:
                    break;
                case CalendarState.DraggingItem:
                    break;
                case CalendarState.ResizingItem:
                    break;
                case CalendarState.EditingItemText:
                    break;

            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseMove( MouseEventArgs e )
        {
            base.OnMouseMove( e );

            ICalendarSelectableElement hitted = HitTest( e.Location, State != CalendarState.Idle );
            CalendarItem hittedItem = hitted as CalendarItem;
            CalendarDayTop hittedTop = hitted as CalendarDayTop;
            bool shiftPressed = ( ModifierKeys & Keys.Shift ) == Keys.Shift;

            if( hitted != null )
            {
                switch( State )
                {
                    case CalendarState.Idle:
                        Cursor should = Cursors.Default;

                        if( hittedItem != null )
                        {
                            if( ( hittedItem.ResizeEndDateZone( e.Location ) || hittedItem.ResizeStartDateZone( e.Location ) ) && AllowItemResize )
                            {
                                should = hittedItem.IsOnDayTop || DaysMode == CalendarDaysMode.Short ? Cursors.SizeWE : Cursors.SizeNS;
                            }

                            OnItemMouseHover( new CalendarItemEventArgs( hittedItem ) );

                        }
                        if( !Cursor.Equals( should ) ) Cursor = should;
                        break;
                    case CalendarState.DraggingTimeSelection:
                        if( SelectedElementStart != null && !SelectedElementEnd.Equals( hitted ) )
                            SelectedElementEnd = hitted;
                        break;
                    case CalendarState.DraggingItem:
                        TimeSpan duration = itemOnState.Duration;
                        itemOnState.SetIsDragging( true );
                        itemOnState.StartDate = hitted.Date;
                        itemOnState.EndDate = itemOnState.StartDate.Add( duration );
                        Renderer.PerformItemsLayout();
                        Invalidate();
                        itemOnStateChanged = true;
                        break;
                    case CalendarState.ResizingItem:
                        if( itemOnState.IsResizingEndDate && hitted.Date.CompareTo( itemOnState.StartDate ) >= 0 )
                        {
                            itemOnState.EndDate = hitted.Date.Add( hittedTop != null || DaysMode == CalendarDaysMode.Short ? new TimeSpan( 23, 59, 59 ) : Days[0].TimeUnits[0].Duration );
                        }
                        else if( itemOnState.IsResizingStartDate && hitted.Date.CompareTo( itemOnState.EndDate ) <= 0 )
                        {
                            itemOnState.StartDate = hitted.Date;
                        }
                        Renderer.PerformItemsLayout();
                        Invalidate();
                        itemOnStateChanged = true;
                        break;
                    case CalendarState.EditingItemText:
                        break;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseUp( MouseEventArgs e )
        {
            base.OnMouseUp( e );

            ICalendarSelectableElement hitted = HitTest( e.Location, State == CalendarState.DraggingTimeSelection );
            CalendarItem hittedItem = hitted as CalendarItem;
            CalendarDay hittedDay = hitted as CalendarDay;
            bool shiftPressed = ( ModifierKeys & Keys.Shift ) == Keys.Shift;

            switch( State )
            {
                case CalendarState.Idle:

                    break;
                case CalendarState.DraggingTimeSelection:
                    if( SelectedElementStart == null || ( hitted != null && !SelectedElementEnd.Equals( hitted ) ) )
                    {
                        SelectedElementEnd = hitted;
                    }
                    if( hittedDay != null )
                    {
                        if( hittedDay.HeaderBounds.Contains( e.Location ) )
                        {
                            OnDayHeaderClick( new CalendarDayEventArgs( hittedDay ) );
                        }
                    }
                    break;
                case CalendarState.DraggingItem:
                    if( itemOnStateChanged )
                        OnItemDatesChanged( new CalendarItemEventArgs( itemOnState ) );
                    break;
                case CalendarState.ResizingItem:
                    if( itemOnStateChanged )
                        OnItemDatesChanged( new CalendarItemEventArgs( itemOnState ) );
                    break;
                case CalendarState.EditingItemText:
                    break;
            }

            if( itemOnState != null )
            {
                itemOnState.SetIsDragging( false );
                itemOnState.SetIsResizingEndDate( false );
                itemOnState.SetIsResizingStartDate( false );
                Invalidate( itemOnState );
                OnItemClick( new CalendarItemEventArgs( itemOnState ) );
                itemOnState = null;
            }
            SetState( CalendarState.Idle );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseWheel( MouseEventArgs e )
        {
            base.OnMouseWheel( e );

            if( DaysMode == CalendarDaysMode.Expanded )
            {
                ScrollTimeUnits( e.Delta );
            }
            else if( DaysMode == CalendarDaysMode.Short )
            {
                ScrollCalendar( e.Delta );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint( PaintEventArgs e )
        {
            base.OnPaint( e );

            CalendarRendererEventArgs evt = new CalendarRendererEventArgs( this, e.Graphics, e.ClipRectangle );

            //Calendar background
            Renderer.OnDrawBackground( evt );

            // Headers / Timescale
            switch( DaysMode )
            {
                case CalendarDaysMode.Short:
                    Renderer.OnDrawDayNameHeaders( evt );
                    Renderer.OnDrawWeekHeaders( evt );
                    break;
                case CalendarDaysMode.Expanded:
                    Renderer.OnDrawTimeScale( evt );
                    break;
                default:
                    throw new NotImplementedException( "Current DaysMode not implemented" );
            }

            //Days on view
            Renderer.OnDrawDays( evt );

            //Items
            Renderer.OnDrawItems( evt );

            //Overflow marks
            Renderer.OnDrawOverflows( evt );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Resize"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResize( EventArgs e )
        {
            base.OnResize( e );

            TimeUnitsOffset = TimeUnitsOffset;
            Renderer.PerformLayout();
        }

        #endregion

    }
}
