/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
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
using NLog;

namespace Chummer
{
    public partial class Chummy : Form
    {
        private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private const int EyeBallWidth = 20;
        private const int EyeBallHeight = 32;
        private const int DistanceBetweenEyes = 10;
        private readonly Point _eyeballCenter = new Point(95, 15);
        private readonly Point _mouthCenter = new Point(100, 50);
        private readonly Pen _thickPen = new Pen(Color.Black, 3);
        readonly XPathNavigator _objXmlDocument = XmlManager.Load("tips.xml").GetFastNavigator().SelectSingleNode("/chummer/tips");
        private readonly List<string> _usedTips = new List<string>();
        private Point _oldMousePos = new Point(-1, -1);
        private Character _characterObject;

        readonly ToolTip _myToolTip = new ToolTip
        {
            IsBalloon = true
        };

        public Chummy()
        {
            InitializeComponent();

            this.Paint += panel1_Paint;

            var tmrDraw = new Timer {Interval = 100};
            tmrDraw.Tick += tmr_DrawTick;
            tmrDraw.Start();

            var tmrTip = new Timer { Interval = 300000 };
            tmrTip.Tick += tmr_TipTick;
            tmrTip.Start();

            _myToolTip.Show(LanguageManager.GetString("Chummy_Intro", GlobalOptions.Language).WordWrap(100), this, _mouthCenter);
        }
        #region Event Handlers
        private void tmr_DrawTick(object sender, EventArgs e)
        {
            // See if the cursor has moved.
            Point newPos = Control.MousePosition;
            if (newPos.Equals(_oldMousePos)) return;
            _oldMousePos = newPos;

            // Redraw.
            this.Invalidate();
        }

        private void tmr_TipTick(object sender, EventArgs e)
        {
            ShowBalloonTip();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
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
            gr.DrawEllipse(_thickPen, x1, y1, wid, hgt);

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
            int x = (int)(px - (double)wid / 4);
            int y = (int)(py - (double)hgt / 4);
            int width = wid / 2;
            int height = hgt / 2;
            try
            {
                gr.FillEllipse(Brushes.Blue, x, y, width, height);
            }
            catch (Exception e)
            {
                string msg = String.Format("Got an " + e.GetType().ToString() + " with these variables in Chummy.cs-DrawEye(): x={0},y={1},width={2},height={3}", x,
                    y, width, height);
                Log.Warn(e, msg);
            }
            
        }
        #endregion
        #region Chat Bubble

        private string HelpfulAdvice()
        {
            if (_usedTips.Count == _objXmlDocument.Select("tip").Count)
            {
                _usedTips.Clear();
            }
            foreach (XPathNavigator objXmlTip in _objXmlDocument.Select("tip"))
            {
                var strId = objXmlTip.SelectSingleNode("id")?.Value;
                if (string.IsNullOrEmpty(strId) || _usedTips.Contains(strId)) continue;
                if (!objXmlTip.RequirementsMet(CharacterObject)) continue;
                _usedTips.Add(strId);
                return objXmlTip.SelectSingleNode("translate")?.Value ?? objXmlTip.SelectSingleNode("text")?.Value ?? string.Empty;
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
        #endregion
        #region Properties

        public Character CharacterObject
        {
            get => _characterObject;
            set
            {
                _usedTips.Clear();
                _characterObject = value;
            }
        }

        #endregion
    }
}
