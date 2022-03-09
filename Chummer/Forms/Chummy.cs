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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml.XPath;
using NLog;

namespace Chummer
{
    public partial class Chummy : Form
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private const int EyeBallWidth = 20;
        private const int EyeBallHeight = 32;
        private const int DistanceBetweenEyes = 10;
        private readonly Point _eyeballCenter;
        private readonly Point _mouthCenter;
        private readonly Pen _thickPen;
        private readonly XPathNavigator _objXmlDocument;
        private readonly List<string> _lstUsedTips = new List<string>();
        private Point _oldMousePos = new Point(-1, -1);
        private Character _characterObject;

        private readonly ToolTip _myToolTip = new ToolTip
        {
            IsBalloon = true
        };

        public Chummy(Character objCharacter)
        {
            InitializeComponent();

            using (Graphics g = CreateGraphics())
            {
                _eyeballCenter = new Point((int)(95 * g.DpiX / 96.0f), (int)(15 * g.DpiY / 96.0f));
                _mouthCenter = new Point((int)(100 * g.DpiX / 96.0f), (int)(50 * g.DpiY / 96.0f));
                _thickPen = new Pen(Color.Black, (int)(3 * g.DpiY / 96.0f));
            }

            Paint += panel1_Paint;

            using (Timer tmrDraw = new Timer { Interval = 100 })
            {
                tmrDraw.Tick += tmr_DrawTick;
                tmrDraw.Start();
            }

            using (Timer tmrTip = new Timer { Interval = 300000 })
            {
                tmrTip.Tick += tmr_TipTick;
                tmrTip.Start();
            }

            _myToolTip.Show(LanguageManager.GetString("Chummy_Intro").WordWrap(), this, _mouthCenter);
            _objXmlDocument = (objCharacter?.LoadDataXPath("tips.xml") ?? XmlManager.LoadXPath("tips.xml")).SelectSingleNodeAndCacheExpression("/chummer/tips");
        }

        #region Event Handlers

        private void tmr_DrawTick(object sender, EventArgs e)
        {
            // See if the cursor has moved.
            Point newPos = MousePosition;
            if (newPos.Equals(_oldMousePos)) return;
            _oldMousePos = newPos;

            // Redraw.
            Invalidate();
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
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(Handle, 0xa1, 0x2, IntPtr.Zero);
                    break;

                case MouseButtons.Left:
                    ShowBalloonTip();
                    break;
            }
        }

        #endregion Event Handlers

        #region Draw Eyes

        private void DrawEyes(Graphics gr)
        {
            // Convert the cursor position into form units.
            Point localPos = PointToClient(_oldMousePos);

            // Find the positions of the eyes.
            int x1 = _eyeballCenter.X - (int)(DistanceBetweenEyes * gr.DpiX / 96.0f);
            int x2 = _eyeballCenter.X + (int)(DistanceBetweenEyes * gr.DpiX / 96.0f);

            // Create a Bitmap on which to draw.
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            //gr.Clear(this.BackColor);

            // Draw the eyes.
            DrawEye(gr, localPos, x1, _eyeballCenter.Y, (int)(EyeBallWidth * gr.DpiX / 96.0f), (int)(EyeBallHeight * gr.DpiY / 96.0f));
            DrawEye(gr, localPos, x2, _eyeballCenter.Y, (int)(EyeBallWidth * gr.DpiX / 96.0f), (int)(EyeBallHeight * gr.DpiY / 96.0f));
        }

        private void DrawEye(Graphics gr, Point localPos,
            int x1, int y1, int wid, int hgt)
        {
            // Draw the outside.
            gr.FillEllipse(Brushes.White, x1, y1, wid, hgt);
            gr.DrawEllipse(_thickPen, x1, y1, wid, hgt);

            // Find the center of the eye.
            int cx = x1 + wid / 2;
            int cy = y1 + hgt / 2;

            // Get the unit vector pointing towards the mouse position.
            double dx = localPos.X - cx;
            double dy = localPos.Y - cy;
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
                string msg = string.Format(GlobalSettings.InvariantCultureInfo, "Got an " + e.GetType() + " with these variables in Chummy.cs-DrawEye(): x={0},y={1},width={2},height={3}",
                    x, y, width, height);
                Log.Warn(e, msg);
            }
        }

        #endregion Draw Eyes

        #region Chat Bubble

        private string HelpfulAdvice()
        {
            if (_lstUsedTips.Count == _objXmlDocument.SelectAndCacheExpression("tip").Count)
            {
                _lstUsedTips.Clear();
            }
            foreach (XPathNavigator objXmlTip in _objXmlDocument.SelectAndCacheExpression("tip"))
            {
                string strId = objXmlTip.SelectSingleNodeAndCacheExpression("id")?.Value;
                if (string.IsNullOrEmpty(strId) || _lstUsedTips.Contains(strId))
                    continue;
                if (!objXmlTip.RequirementsMet(CharacterObject))
                    continue;
                _lstUsedTips.Add(strId);
                return objXmlTip.SelectSingleNodeAndCacheExpression("translate")?.Value
                       ?? objXmlTip.SelectSingleNodeAndCacheExpression("text")?.Value ?? string.Empty;
            }
            return string.Empty;
        }

        private void ShowBalloonTip()
        {
            _myToolTip.Show(HelpfulAdvice().WordWrap(), this, _mouthCenter);
        }

        private void HideBalloonTip()
        {
            _myToolTip.Hide(this);
        }

        #endregion Chat Bubble

        #region Properties

        public Character CharacterObject
        {
            get => _characterObject;
            set
            {
                _lstUsedTips.Clear();
                _characterObject = value;
            }
        }

        #endregion Properties
    }
}
