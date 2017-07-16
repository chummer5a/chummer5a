// ============================================================================ '
// NumericUpDownEx - v.1.6                                                      '
// ============================================================================ '
// Author:   Claudio Nicora                                                     '
// WebSite:  http://coolsoft.altervista.org/numericupdownex                     '
// CodeProject: http://www.codeproject.com/KB/edit/NumericUpDownEx.aspx         '
// License:  CodeProject Open License                                           '
//           http://www.codeproject.com/info/cpol10.aspx                        '
// Feel free to contribute here: http://coolsoft.altervista.org                 '
// ============================================================================ '

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace Chummer.helpers
{
    public class NumericUpDownEx : NumericUpDown
    {
        // reference to the underlying TextBox control
        private TextBox _textbox;

        // reference to the underlying UpDownButtons control
        private Control _upDownButtons;

        // default value that will be used when incrementing via mousewheel
        private int _intMouseIncrement = 1;

        /// <summary>
        /// object creator
        /// </summary>
        public NumericUpDownEx()
            : base()
        {
            // get a reference to the underlying UpDownButtons field
            // Underlying private type is System.Windows.Forms.UpDownBase+UpDownButtons
            _upDownButtons = base.Controls[0];
            if (_upDownButtons == null || _upDownButtons.GetType().FullName != "System.Windows.Forms.UpDownBase+UpDownButtons")
            {
                throw new ArgumentNullException(this.GetType().FullName + ": Can't find internal UpDown buttons field.");
            }
            // Get a reference to the underlying TextBox field.
            // Underlying private type is System.Windows.Forms.UpDownBase+UpDownButtons
            _textbox = base.Controls[1] as TextBox;
            if (_textbox == null || _textbox.GetType().FullName != "System.Windows.Forms.UpDownBase+UpDownEdit")
            {
                throw new ArgumentNullException(this.GetType().FullName + ": Can't find internal TextBox field.");
            }
            // add handlers (MouseEnter and MouseLeave events of NumericUpDown
            // are not working properly)
            _textbox.MouseEnter += _mouseEnterLeave;
            _textbox.MouseLeave += _mouseEnterLeave;
            _textbox.KeyDown += txt_KeyDown;
            _upDownButtons.MouseEnter += _mouseEnterLeave;
            _upDownButtons.MouseLeave += _mouseEnterLeave;
            base.MouseEnter += _mouseEnterLeave;
            base.MouseLeave += _mouseEnterLeave;
        }


        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            if (_upDownButtons.Visible == false)
            {
                e.Graphics.Clear(this.BackColor);
            }
            base.OnPaint(e);
        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (!Parent.Parent.Focus())
                {
                    Utils.BreakIfDebug();
                }
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        /// <summary>
        /// WndProc override to kill WN_MOUSEWHEEL message
        /// </summary>
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            const int WM_MOUSEWHEEL = 0x20a;

            if (m.Msg == WM_MOUSEWHEEL)
            {

                switch (_interceptMouseWheel)
                {

                    case InterceptMouseWheelMode.Always:
                        // standard message
                        base.WndProc(ref m);
                        break;
                    case InterceptMouseWheelMode.WhenMouseOver:
                        if (_mouseOver)
                        {
                            // standard message
                            base.WndProc(ref m);
                        }
                        break;
                    case InterceptMouseWheelMode.WhenFocus:
                        if (_haveFocus)
                        {
                            base.WndProc(ref m);
                        }
                        break;
                    case InterceptMouseWheelMode.Never:
                        // kill the message
                        return;
                }

            }
            else
            {
                base.WndProc(ref m);
            }

        }


        #region New properties

        [DefaultValue(false)]
        [Category("Behavior")]
        [Description("Automatically select control text when it receives focus.")]
        public bool AutoSelect { get; set; }


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart
        {
            get { return _textbox.SelectionStart; }
            set { _textbox.SelectionStart = value; }
        }


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionLength
        {
            get { return _textbox.SelectionLength; }
            set { _textbox.SelectionLength = value; }
        }


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get { return _textbox.SelectedText; }
            set { _textbox.SelectedText = value; }
        }


        [DefaultValue(typeof(InterceptMouseWheelMode), "Always")]
        [Category("Behavior")]
        [Description("Enables MouseWheel only under certain conditions.")]
        public InterceptMouseWheelMode InterceptMouseWheel
        {
            get { return _interceptMouseWheel; }
            set { _interceptMouseWheel = value; }
        }

        private InterceptMouseWheelMode _interceptMouseWheel = InterceptMouseWheelMode.Always;

        public enum InterceptMouseWheelMode
        {
            /// <summary>MouseWheel always works (defauld behavior)</summary>
            Always,
            /// <summary>MouseWheel works only when mouse is over the (focused) control</summary>
            WhenMouseOver,
            /// <summary>UpDownButtons are visible only when control has focus</summary>
            WhenFocus,
            /// <summary>MouseWheel never works</summary>
            Never
        }


        [Category("Behavior")]
        [DefaultValue(typeof(ShowUpDownButtonsMode), "Always")]
        [Description("Set UpDownButtons visibility mode.")]
        public ShowUpDownButtonsMode ShowUpDownButtons
        {
            get { return _showUpDownButtons; }
            set
            {
                _showUpDownButtons = value;
                // update UpDownButtons visibility
                UpdateUpDownButtonsVisibility();
            }
        }

        private ShowUpDownButtonsMode _showUpDownButtons = ShowUpDownButtonsMode.Always;

        public enum ShowUpDownButtonsMode
        {
            /// <summary>UpDownButtons are always visible (defauld behavior)</summary>
            Always,
            /// <summary>UpDownButtons are visible only when mouse is over the control</summary>
            WhenMouseOver,
            /// <summary>UpDownButtons are visible only when control has focus</summary>
            WhenFocus,
            /// <summary>UpDownButtons are visible when control has focus or mouse is over it</summary>
            WhenFocusOrMouseOver,
            /// <summary>UpDownButtons are never visible</summary>
            Never,
        }

        /// <summary>
        /// If set, incrementing value will cause it to restart from Minimum 
        /// when Maximum is reached (and viceversa).
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(1)]
        [Description("Indicates the amount to increment or decrement with the use of the mouse wheel.")]
        public int MouseIncrement
        {
            get { return _intMouseIncrement; }
            set { _intMouseIncrement = value; }
        }

        /// <summary>
        /// If set, incrementing value will cause it to restart from Minimum 
        /// when Maximum is reached (and viceversa).
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("If set, incrementing value will cause it to restart from Minimum when Maximum is reached (and viceversa).")]
        public bool WrapValue
        {
            get { return _wrapValue; }
            set { _wrapValue = value; }
        }
        private bool _wrapValue = false;

        #endregion


        #region Text selection

        // select all the text on focus enter
        protected override void OnGotFocus(System.EventArgs e)
        {
            _haveFocus = true;
            if (AutoSelect)
            {
                _textbox.SelectAll();
            }
            // Update UpDownButtons visibility
            if (_showUpDownButtons == ShowUpDownButtonsMode.WhenFocus | _showUpDownButtons == ShowUpDownButtonsMode.WhenFocusOrMouseOver)
            {
                UpdateUpDownButtonsVisibility();
            }
            base.OnGotFocus(e);
        }


        // indicate that we have lost the focus
        protected override void OnLostFocus(EventArgs e)
        {
            _haveFocus = false;
            // Update UpDownButtons visibility
            if (_showUpDownButtons == ShowUpDownButtonsMode.WhenFocus | _showUpDownButtons == ShowUpDownButtonsMode.WhenFocusOrMouseOver)
            {
                UpdateUpDownButtonsVisibility();
            }
            base.OnLostFocus(e);
        }


        // MouseUp will kill the SelectAll made on GotFocus.
        // Will restore it, but only if user have not made a partial text selection.
        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs mevent)
        {
            if (AutoSelect && _textbox.SelectionLength == 0)
            {
                _textbox.SelectAll();
            }
            base.OnMouseUp(mevent);
        }

        #endregion


        #region Additional events

        // these events will be raised correctly, when mouse enters on the textbox
        public new event EventHandler<EventArgs> MouseEnter;
        public new event EventHandler<EventArgs> MouseLeave;

        // Events raised BEFORE value decrement(increment
        public event CancelEventHandler BeforeValueDecrement;
        public event CancelEventHandler BeforeValueIncrement;

        // flag to track mouse position
        private bool _mouseOver = false;

        // flag to track focus
        private bool _haveFocus = false;

        // this handler is called at each mouse Enter/Leave movement
        private void _mouseEnterLeave(object sender, System.EventArgs e)
        {
            Rectangle cr = RectangleToScreen(ClientRectangle);
            Point mp = MousePosition;

            // actual state
            bool isOver = cr.Contains(mp);

            // test if status changed
            if (_mouseOver ^ isOver)
            {
                // update state
                _mouseOver = isOver;
                if (_mouseOver)
                {
                    MouseEnter?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MouseLeave?.Invoke(this, EventArgs.Empty);
                }
            }

            // update UpDownButtons visibility
            if (_showUpDownButtons != ShowUpDownButtonsMode.Always)
            {
                UpdateUpDownButtonsVisibility();
            }

        }

        #endregion


        #region Value increment/decrement management

        // raises the two new events
        public override void DownButton()
        {
            if (base.ReadOnly) return;
            CancelEventArgs e = new CancelEventArgs();
            BeforeValueDecrement?.Invoke(this, e);
            if (e.Cancel) return;

            if (_wrapValue && Value - Increment < Minimum)
            {
                Value = Maximum;
            }
            else
            {
                base.DownButton();
            }
        }
        public override void UpButton()
        {
            if (base.ReadOnly) return;
            CancelEventArgs e = new CancelEventArgs();
            BeforeValueIncrement?.Invoke(this, e);
            if (e.Cancel) return;

            if (_wrapValue && Value + Increment > Maximum)
            {
                Value = Minimum;
            }
            else
            {
                base.UpButton();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            HandledMouseEventArgs hme = e as HandledMouseEventArgs;
            if (hme != null)
                hme.Handled = true;

            if (e.Delta > 0)
                this.Value = Math.Min(this.Value + this.MouseIncrement, this.Maximum);
            else if (e.Delta < 0)
                this.Value = Math.Max(this.Value - this.MouseIncrement, this.Minimum);
        }
        #endregion


        #region UpDownButtons visibility management

        /// <summary>
        /// Show or hide the UpDownButtons, according to ShowUpDownButtons property value
        /// </summary>
        public void UpdateUpDownButtonsVisibility()
        {
            // test new state
            bool newVisible = false;
            switch (_showUpDownButtons)
            {
                case ShowUpDownButtonsMode.WhenMouseOver:
                    newVisible = _mouseOver;
                    break;
                case ShowUpDownButtonsMode.WhenFocus:
                    newVisible = _haveFocus;
                    break;
                case ShowUpDownButtonsMode.WhenFocusOrMouseOver:
                    newVisible = _mouseOver | _haveFocus;
                    break;
                case ShowUpDownButtonsMode.Never:
                    newVisible = false;
                    break;
                default:
                    newVisible = true;
                    break;
            }

            // assign only if needed
            if (_upDownButtons.Visible != newVisible)
            {
                if (newVisible)
                {
                    _textbox.Width = this.ClientRectangle.Width - _upDownButtons.Width;
                }
                else
                {
                    _textbox.Width = this.ClientRectangle.Width;
                }
                _upDownButtons.Visible = newVisible;
                OnTextBoxResize(_textbox, EventArgs.Empty);
                this.Invalidate();
            }

        }


        /// <summary>
        /// Custom textbox size management
        /// </summary>
        protected override void OnTextBoxResize(object source, System.EventArgs e)
        {
            if (_textbox == null)
                return;
            if (_showUpDownButtons == ShowUpDownButtonsMode.Always)
            {
                // standard management
                base.OnTextBoxResize(source, e);
            }
            else
            {
                // custom management

                // change position if RTL
                bool fixPos = this.RightToLeft == RightToLeft.Yes ^ this.UpDownAlign == LeftRightAlignment.Left;

                if (_mouseOver)
                {
                    _textbox.Width = this.ClientSize.Width - _textbox.Left - _upDownButtons.Width - 2;
                    if (fixPos)
                        _textbox.Location = new Point(16, _textbox.Location.Y);
                }
                else
                {
                    if (fixPos)
                        _textbox.Location = new Point(2, _textbox.Location.Y);
                    _textbox.Width = this.ClientSize.Width - _textbox.Left - 2;
                }

            }

        }

        #endregion

    }
}