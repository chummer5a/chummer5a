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
        }

        public DpiFriendlyToolStripMenuItem(string strText) : base(strText)
        {
            InitializeComponent();
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
        }

        public void RefreshImage()
        {
            if (Utils.IsDesignerMode)
                return;
            List<Image> lstImages = new List<Image>(Images);
            if (lstImages.Count == 0)
            {
                Image = null;
                return;
            }
            if (lstImages.Count == 1)
            {
                Image = lstImages[0];
                return;
            }
            // Toolstrip items contain both images and text, so we take the smallest of the two dimensions for the image and then assume that the image should be square-shaped
            int intWidth = Math.Min(Width, Height);
            int intHeight = Math.Min(Width, Height);
            Image objBestImage = null;
            int intBestImageMetric = int.MaxValue;
            foreach (Image objLoopImage in lstImages)
            {
                int intLoopMetric = (intHeight - objLoopImage.Height).RaiseToPower(2) + (intWidth - objLoopImage.Width).RaiseToPower(2);
                // Small biasing so that in case of a tie, the image that gets picked is the one that would be scaled down, not scaled up
                if (objLoopImage.Height >= intHeight)
                    intLoopMetric -= 1;
                if (objLoopImage.Width >= intWidth)
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

        private Image _objImageDpi96;

        private Image _objImageDpi120;

        private Image _objImageDpi144;

        private Image _objImageDpi192;

        private Image _objImageDpi288;

        private Image _objImageDpi384;

        public Image ImageDpi96
        {
            get => _objImageDpi96;
            set
            {
                if (_objImageDpi96 == value)
                    return;
                _objImageDpi96 = value;
                UpdateImageIfBetterMatch(value);
            }
        }

        public Image ImageDpi120
        {
            get => _objImageDpi120;
            set
            {
                if (_objImageDpi120 == value)
                    return;
                _objImageDpi120 = value;
                UpdateImageIfBetterMatch(value);
            }
        }

        public Image ImageDpi144
        {
            get => _objImageDpi144;
            set
            {
                if (_objImageDpi144 == value)
                    return;
                _objImageDpi144 = value;
                UpdateImageIfBetterMatch(value);
            }
        }

        public Image ImageDpi192
        {
            get => _objImageDpi192;
            set
            {
                if (_objImageDpi192 == value)
                    return;
                _objImageDpi192 = value;
                UpdateImageIfBetterMatch(value);
            }
        }

        public Image ImageDpi288
        {
            get => _objImageDpi288;
            set
            {
                if (_objImageDpi288 == value)
                    return;
                _objImageDpi288 = value;
                UpdateImageIfBetterMatch(value);
            }
        }

        public Image ImageDpi384
        {
            get => _objImageDpi384;
            set
            {
                if (_objImageDpi384 == value)
                    return;
                _objImageDpi384 = value;
                UpdateImageIfBetterMatch(value);
            }
        }

        /// <summary>
        /// Checks a newly set image against the existing image of the button to see if it's a better fit than the current image.
        /// Only use this with images that are one of the ones set for this button!
        /// </summary>
        /// <param name="objNewImage"></param>
        private void UpdateImageIfBetterMatch(Image objNewImage)
        {
            if (Utils.IsDesignerMode)
                return;
            if (objNewImage == null)
                return;
            if (Image == null)
            {
                Image = objNewImage;
                return;
            }
            // Toolstrip items contain both images and text, so we take the smallest of the two dimensions for the image and then assume that the image should be square-shaped
            int intWidth = Math.Min(Width, Height);
            int intHeight = Math.Min(Width, Height);
            int intCurrentMetric = (intHeight - Image.Height).RaiseToPower(2) +
                                   (intWidth - Image.Width).RaiseToPower(2);
            int intNewMetric = (intHeight - objNewImage.Height).RaiseToPower(2) +
                               (intWidth - objNewImage.Width).RaiseToPower(2);
            if (intNewMetric > intCurrentMetric)
                Image = objNewImage;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            RefreshImage();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            RefreshImage();
        }
    }
}
