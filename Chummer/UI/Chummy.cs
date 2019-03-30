using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public partial class Chummy : Form
    {
        private const int EyeBallWidth = 20;
        private const int EyeBallHeight = 32;
        private const int DistanceBetweenEyes = 10;
        private readonly Point _eyeballCenter = new Point(95, 15);
        private readonly Point _mouthCenter = new Point(100, 50);
        private readonly Pen ThickPen = new Pen(Color.Black, 3);
        readonly XPathNavigator objXmlDocument = XmlManager.Load("tips.xml").GetFastNavigator().SelectSingleNode("/chummer/tips");
        private List<string> UsedTips = new List<string>();

        readonly ToolTip _myToolTip = new ToolTip
        {
            IsBalloon = true
        };

        // The previous mouse location.
        private Point _oldMousePos = new Point(-1, -1);
        private Character _characterObject;

        public Chummy()
        {
            InitializeComponent();

            this.Paint += panel1_Paint;

            var tmr = new Timer {Interval = 100};
            tmr.Tick += tmr_Tick;
            tmr.Start();
        }
        #region Event Handlers
        void tmr_Tick(object sender, EventArgs e)
        {
            // See if the cursor has moved.
            Point newPos = Control.MousePosition;
            if (newPos.Equals(_oldMousePos)) return;
            _oldMousePos = newPos;

            // Redraw.
            this.Invalidate();
        }

        void panel1_Paint(object sender, PaintEventArgs e)
        {
            DrawEyes(e.Graphics);
        }
        
        private void Chummy_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left when e.Clicks == 1:
                    // drag the form without the caption bar
                    // present on left mouse button
                    HideBalloonTip();
                    ReleaseCapture();
                    SendMessage(this.Handle, 0xa1, 0x2, 0);
                    break;
                case MouseButtons.Left:
                    ShowBalloonTip();
                    break;
            }
        }
        #endregion
        #region Form Dragging API Support
        //The SendMessage function sends a message to a window or windows.

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]

        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        //ReleaseCapture releases a mouse capture

        [DllImportAttribute("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]

        public static extern bool ReleaseCapture();

        #endregion
        #region Draw Eyes
        private void DrawEyes(Graphics gr)
        {
            // Convert the cursor position into form units.
            Point localPos = this.PointToClient(_oldMousePos);

            // Find the positions of the eyes.
            int x1 = _eyeballCenter.X - DistanceBetweenEyes;
            int x2 = _eyeballCenter.X + DistanceBetweenEyes;

            // Create a Bitmap on which to draw.
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            //gr.Clear(this.BackColor);

            // Draw the eyes.
            DrawEye(gr, localPos, x1, _eyeballCenter.Y, EyeBallWidth, EyeBallHeight);
            DrawEye(gr, localPos, x2, _eyeballCenter.Y, EyeBallWidth, EyeBallHeight);
        }

        private void DrawEye(Graphics gr, Point local_pos,
            int x1, int y1, int wid, int hgt)
        {
            // Draw the outside.
            gr.FillEllipse(Brushes.White, x1, y1, wid, hgt);
            gr.DrawEllipse(ThickPen, x1, y1, wid, hgt);

            // Find the center of the eye.
            int cx = x1 + wid / 2;
            int cy = y1 + hgt / 2;

            // Get the unit vector pointing towards the mouse position.
            double dx = local_pos.X - cx;
            double dy = local_pos.Y - cy;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            dx /= dist;
            dy /= dist;

            // This point is 1/4 of the way
            // from the center to the edge of the eye.
            double px = cx + dx * wid / 4;
            double py = cy + dy * hgt / 4;

            // Draw an ellipse 1/2 the size of the eye
            // centered at (px, py).
            gr.FillEllipse(Brushes.Blue, (int)(px - wid / 4),
                (int)(py - hgt / 4), wid / 2, hgt / 2);
        }
        #endregion
        #region Chat Bubble

        private string HelpfulAdvice()
        {
            if (UsedTips.Count == objXmlDocument.Select("tip").Count)
            {
                UsedTips.Clear();
            }
            foreach (XPathNavigator objXmlTip in objXmlDocument.Select("tip"))
            {
                var strId = objXmlTip.SelectSingleNode("id")?.Value;
                if (string.IsNullOrEmpty(strId) || UsedTips.Contains(strId)) continue;
                if (!objXmlTip.RequirementsMet(CharacterObject)) continue;
                UsedTips.Add(strId);
                return objXmlTip.SelectSingleNode("text")?.Value;
            }
            return string.Empty;
        }
        private void ShowBalloonTip()
        {
            _myToolTip.Show(HelpfulAdvice().WordWrap(100), this, _mouthCenter);
        }

        private void HideBalloonTip()
        {
            _myToolTip.Hide(this);
        }
        public static void DrawRoundedRectangle(Graphics g, Color color, Rectangle rec, int radius,
            RoundedCorners corners)
        {
            using (var b = new SolidBrush(color))
            {
                int x = rec.X;
                int y = rec.Y;
                int diameter = radius * 2;
                var horiz = new Rectangle(x, y + radius, rec.Width, rec.Height - diameter);
                var vert = new Rectangle(x + radius, y, rec.Width - diameter, rec.Height);

                g.FillRectangle(b, horiz);
                g.FillRectangle(b, vert);

                if ((corners & RoundedCorners.TopLeft) == RoundedCorners.TopLeft)
                    g.FillEllipse(b, x, y, diameter, diameter);
                else
                    g.FillRectangle(b, x, y, diameter, diameter);

                if ((corners & RoundedCorners.TopRight) == RoundedCorners.TopRight)
                    g.FillEllipse(b, x + rec.Width - (diameter + 1), y, diameter, diameter);
                else
                    g.FillRectangle(b, x + rec.Width - (diameter + 1), y, diameter, diameter);

                if ((corners & RoundedCorners.BottomLeft) == RoundedCorners.BottomLeft)
                    g.FillEllipse(b, x, y + rec.Height - (diameter + 1), diameter, diameter);
                else
                    g.FillRectangle(b, x, y + rec.Height - (diameter + 1), diameter, diameter);

                if ((corners & RoundedCorners.BottomRight) == RoundedCorners.BottomRight)
                    g.FillEllipse(b, x + rec.Width - (diameter + 1), y + rec.Height - (diameter + 1), diameter, diameter);
                else
                    g.FillRectangle(b, x + rec.Width - (diameter + 1), y + rec.Height - (diameter + 1), diameter,
                        diameter);
            }
        }

        public enum RoundedCorners
        {
            None = 0x00,
            TopLeft = 0x02,
            TopRight = 0x04,
            BottomLeft = 0x08,
            BottomRight = 0x10,
            All = 0x1F
        }
        #endregion
        #region Properties

        public Character CharacterObject
        {
            get => _characterObject;
            set
            {
                UsedTips.Clear();
                _characterObject = value;
            }
        }

        #endregion
    }
}
