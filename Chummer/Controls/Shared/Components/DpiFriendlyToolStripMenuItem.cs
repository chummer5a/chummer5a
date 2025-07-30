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
using System.Threading;
using System.Windows.Forms;

namespace Chummer
{
    public class DpiFriendlyToolStripMenuItem : ToolStripMenuItem
    {
        public DpiFriendlyToolStripMenuItem()
        {
        }

        public DpiFriendlyToolStripMenuItem(string strText) : base(strText)
        {
        }

        public DpiFriendlyToolStripMenuItem(Image objImage) : base(objImage)
        {
            RefreshImage();
        }

        public DpiFriendlyToolStripMenuItem(string strText, Image objImage) : base(strText, objImage)
        {
            RefreshImage();
        }

        public DpiFriendlyToolStripMenuItem(string strText, Image objImage, EventHandler funcOnClick) : base(strText, objImage, funcOnClick)
        {
            RefreshImage();
        }

        public DpiFriendlyToolStripMenuItem(string strText, Image objImage, EventHandler funcOnClick, Keys objShortcutKeys) : base(strText, objImage, funcOnClick, objShortcutKeys)
        {
            RefreshImage();
        }

        public DpiFriendlyToolStripMenuItem(string strText, Image objImage, EventHandler funcOnClick, string strName) : base(strText, objImage, funcOnClick, strName)
        {
            RefreshImage();
        }

        public DpiFriendlyToolStripMenuItem(string strText, Image objImage, params ToolStripItem[] aobjDropDownItems) : base(strText, objImage, aobjDropDownItems)
        {
            RefreshImage();
        }

        public void RefreshImage()
        {
            if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                return;
            List<Image> lstImages = new List<Image>(6);
            lstImages.AddRange(Images);
            switch (lstImages.Count)
            {
                case 0:
                    Image = null;
                    return;

                case 1:
                    Image = lstImages[0];
                    return;
            }

            // Toolstrip items contain both images and text, so we take the smallest of the two dimensions for the image and then assume that the image should be square-shaped
            int intWidth;
            if (Parent != null)
            {
                Size ePreferredImageSize = Parent.ImageScalingSize;
                // No Padding incorporated into preferred image size because it's also ignored on the toolstrip side
                intWidth = Math.Min(ePreferredImageSize.Width, ePreferredImageSize.Height);
            }
            else
                intWidth = Math.Min(Width - (Padding.Left + Padding.Right), Height - (Padding.Top + Padding.Bottom));
            int intHeight = intWidth;
            Image objBestImage = null;
            int intBestImageMetric = int.MaxValue;
            foreach (Image objLoopImage in lstImages)
            {
                int intLoopMetric = (intHeight - objLoopImage.Height).Pow(2) + (intWidth - objLoopImage.Width).Pow(2);
                // Small biasing so that in case of a tie, the image that gets picked is the one that would be scaled down, not up
                if (objLoopImage.Height >= intHeight)
                    --intLoopMetric;
                if (objLoopImage.Width >= intWidth)
                    --intLoopMetric;
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
                if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
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
                Image objOldImage = Interlocked.Exchange(ref _objImageDpi96, value);
                if (objOldImage == value)
                    return;
                if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                    base.Image = value;
                else
                    UpdateImageIfBetterMatch(value, objOldImage);
            }
        }

        public Image ImageDpi120
        {
            get => _objImageDpi120;
            set
            {
                Image objOldImage = Interlocked.Exchange(ref _objImageDpi120, value);
                if (objOldImage == value)
                    return;
                UpdateImageIfBetterMatch(value, objOldImage);
            }
        }

        public Image ImageDpi144
        {
            get => _objImageDpi144;
            set
            {
                Image objOldImage = Interlocked.Exchange(ref _objImageDpi144, value);
                if (objOldImage == value)
                    return;
                UpdateImageIfBetterMatch(value, objOldImage);
            }
        }

        public Image ImageDpi192
        {
            get => _objImageDpi192;
            set
            {
                Image objOldImage = Interlocked.Exchange(ref _objImageDpi192, value);
                if (objOldImage == value)
                    return;
                UpdateImageIfBetterMatch(value, objOldImage);
            }
        }

        public Image ImageDpi288
        {
            get => _objImageDpi288;
            set
            {
                Image objOldImage = Interlocked.Exchange(ref _objImageDpi288, value);
                if (objOldImage == value)
                    return;
                UpdateImageIfBetterMatch(value, objOldImage);
            }
        }

        public Image ImageDpi384
        {
            get => _objImageDpi384;
            set
            {
                Image objOldImage = Interlocked.Exchange(ref _objImageDpi384, value);
                if (objOldImage == value)
                    return;
                UpdateImageIfBetterMatch(value, objOldImage);
            }
        }

        public void BatchSetImages(Image imgDpi96, Image imgDpi120, Image imgDpi144, Image imgDpi192, Image imgDpi288,
            Image imgDpi384)
        {
            _objImageDpi96 = imgDpi96;
            _objImageDpi120 = imgDpi120;
            _objImageDpi144 = imgDpi144;
            _objImageDpi192 = imgDpi192;
            _objImageDpi288 = imgDpi288;
            _objImageDpi384 = imgDpi384;

            if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                base.Image = imgDpi96;
            else
                RefreshImage();
        }

        /// <summary>
        /// Checks a newly set image against the existing image of the button to see if it's a better fit than the current image.
        /// Only use this with images that are one of the ones set for this button!
        /// </summary>
        /// <param name="objNewImage"></param>
        /// <param name="objImageToReplace"></param>
        private void UpdateImageIfBetterMatch(Image objNewImage, Image objImageToReplace)
        {
            if (Utils.IsDesignerMode || Utils.IsRunningInVisualStudio)
                return;
            if (Image == objImageToReplace)
            {
                Image = objNewImage;
                return;
            }
            if (objNewImage == null)
                return;
            if (Image == null)
            {
                Image = objNewImage;
                return;
            }

            // Toolstrip items contain both images and text, so we take the smallest of the two dimensions for the image and then assume that the image should be square-shaped
            int intWidth;
            if (Parent != null)
            {
                Size ePreferredImageSize = Parent.ImageScalingSize;
                // No Padding incorporated into preferred image size because it's also ignored on the toolstrip side
                intWidth = Math.Min(ePreferredImageSize.Width, ePreferredImageSize.Height);
            }
            else
                intWidth = Math.Min(Width - (Padding.Left + Padding.Right), Height - (Padding.Top + Padding.Bottom));
            int intHeight = intWidth;
            int intCurrentMetric = (intHeight - Image.Height).Pow(2) +
                                   (intWidth - Image.Width).Pow(2);
            int intNewMetric = (intHeight - objNewImage.Height).Pow(2) +
                               (intWidth - objNewImage.Width).Pow(2);
            // Small biasing so that in case of a tie, the image that gets picked is the one that would be scaled down, not up
            if (Image.Height >= intHeight)
                --intCurrentMetric;
            if (Image.Width >= intWidth)
                --intCurrentMetric;
            if (objNewImage.Height >= intHeight)
                --intNewMetric;
            if (objNewImage.Width >= intWidth)
                --intNewMetric;
            if (intNewMetric < intCurrentMetric)
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

        protected override void OnBoundsChanged()
        {
            base.OnBoundsChanged();
            RefreshImage();
        }

        protected override void OnParentChanged(ToolStrip oldParent, ToolStrip newParent)
        {
            base.OnParentChanged(oldParent, newParent);
            if (oldParent != null)
            {
                oldParent.DpiChangedAfterParent -= RefreshImage;
                oldParent.SizeChanged -= RefreshImage;
            }
            if (newParent != null)
            {
                newParent.DpiChangedAfterParent += RefreshImage;
                newParent.SizeChanged += RefreshImage;
            }
            RefreshImage();
        }

        private void RefreshImage(object sender, EventArgs e)
        {
            RefreshImage();
        }
    }
}
