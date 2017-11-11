using System;
using System.Windows.Forms;
using Chummer.Backend.Extensions;

namespace Chummer.Backend.UI
{
    public class HoverHelper
    {
        public event EventHandler<MouseEventArgs> Hover;
        public event EventHandler<MouseEventArgs> StopHover;

        private bool _hovering = false;
        private readonly Timer _timer = new Timer()
        {
            Interval = 500
        };

        private MouseEventArgs _mousePos;

        public void MouseEventHandler(object sender, MouseEventArgs args)
        {
            //_mousePos is initialized to null, but can never be as _hovering 
            //is only set after time has run for 500ms which only happens after
            //_mousePos is sat
            if (_hovering && _mousePos.Location.ManhatanDistanceFrom(args.Location) != 0)
            {
                _hovering = false;
                StopHover?.Invoke(this, args);
            }

            _mousePos = args;
            _timer.Stop();
            _timer.Start();
        }

        public int HoverDelay
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        public HoverHelper()
        {
            _timer.Tick += HoverLimitReachedBeforeDisplayingToolTip;
        }

        private void HoverLimitReachedBeforeDisplayingToolTip(object sender, EventArgs eventArgs)
        {
            _hovering = true;
            Hover?.Invoke(this, _mousePos);
        }
    }
}