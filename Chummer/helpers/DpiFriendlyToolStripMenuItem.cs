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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Chummer
{
    public partial class DpiFriendlyToolStripMenuItem : ToolStripMenuItem
    {
        public DpiFriendlyToolStripMenuItem()
        {
            InitializeComponent();
            RefreshImage();
        }

        public DpiFriendlyToolStripMenuItem(string strText) : base(strText)
        {
            InitializeComponent();
            RefreshImage();
        }

        public DpiFriendlyToolStripMenuItem(Image objImage) : base(objImage)
        {
            InitializeComponent();
            RefreshImage();
        }

        public DpiFriendlyToolStripMenuItem(string strText, Image objImage) : base(strText, objImage)
        {
            InitializeComponent();
            RefreshImage();
        }

        public DpiFriendlyToolStripMenuItem(string strText, Image objImage, EventHandler funcOnClick) : base(strText, objImage, funcOnClick)
        {
            InitializeComponent();
            RefreshImage();
        }

        public DpiFriendlyToolStripMenuItem(string strText, Image objImage, EventHandler funcOnClick, Keys objShortcutKeys) : base(strText, objImage, funcOnClick, objShortcutKeys)
        {
            InitializeComponent();
            RefreshImage();
        }

        public DpiFriendlyToolStripMenuItem(string strText, Image objImage, EventHandler funcOnClick, string strName) : base(strText, objImage, funcOnClick, strName)
        {
            InitializeComponent();
            RefreshImage();
        }

        public DpiFriendlyToolStripMenuItem(string strText, Image objImage, params ToolStripItem[] aobjDropDownItems) : base(strText, objImage, aobjDropDownItems)
        {
            InitializeComponent();
            RefreshImage();
        }

        public DpiFriendlyToolStripMenuItem(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            RefreshImage();
        }

        public void RefreshImage()
        {
            if (Utils.IsDesignerMode)
                return;
            int intHeight = Height;
            Image objBestImage = null;
            int intBestImageMetric = int.MaxValue;
            foreach (Image objLoopImage in Images)
            {
                int intLoopMetric = (intHeight - objLoopImage.Height).RaiseToPower(2);
                // Small biasing so that in case of a tie, the image that gets picked is the one that would be scaled down, not scaled up
                if (objLoopImage.Height >= intHeight)
                    intLoopMetric -= 1;
                if (objBestImage == null || intLoopMetric < intBestImageMetric)
                {
                    objBestImage = objLoopImage;
                    intBestImageMetric = intLoopMetric;
                }
            }
            Image = objBestImage;
        }

        public override Image Image
        {
            get => base.Image;
            set
            {
                if (Utils.IsDesignerMode || !Images.Any())
                    ImageDpi96 = value;
                base.Image = value;
            }
        }

        private IEnumerable<Image> Images
        {
            get
            {
                if (ImageDpi96 != null)
                    yield return ImageDpi96;
                if (ImageDpi120 != null)
                    yield return ImageDpi120;
                if (ImageDpi144 != null)
                    yield return ImageDpi144;
                if (ImageDpi192 != null)
                    yield return ImageDpi192;
                if (ImageDpi288 != null)
                    yield return ImageDpi288;
                if (ImageDpi384 != null)
                    yield return ImageDpi384;
            }
        }

        public Image ImageDpi96 { get; set; }

        public Image ImageDpi120 { get; set; }

        public Image ImageDpi144 { get; set; }

        public Image ImageDpi192 { get; set; }

        public Image ImageDpi288 { get; set; }

        public Image ImageDpi384 { get; set; }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            RefreshImage();
        }
    }
}
