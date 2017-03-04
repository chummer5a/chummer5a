using System;
using System.Windows.Forms;

namespace Chummer.Backend.UI
{
    public class HowerHelper
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
            if (_hovering)
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

        public HowerHelper()
        {
            _timer.Tick += HoverLimitReached;
        }

        private void HoverLimitReached(object sender, EventArgs eventArgs)
        {
            Hover?.Invoke(this, _mousePos);
        }
    }
}