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
using System.Drawing;
using System.Windows.Forms;

namespace Chummer.UI.Editors
{
    public partial class RtfEditor : UserControl
    {
        private bool _blnAllowFormatting = true;

        public RtfEditor()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            if (!Utils.IsDesignerMode)
                tsControls.Visible = false;
        }

        public KeyEventHandler ContentKeyDown;

        public void FocusContent()
        {
            rtbContent.Focus();
        }

        private void UpdateFont(object sender, EventArgs e)
        {
            FontStyle eNewFontStyle = FontStyle.Regular;

            if (tsbBold.Checked)
            {
                eNewFontStyle |= FontStyle.Bold;
            }
            if (tsbItalic.Checked)
            {
                eNewFontStyle |= FontStyle.Italic;
            }
            if (tsbUnderline.Checked)
            {
                eNewFontStyle |= FontStyle.Underline;
            }
            if (tsbStrikeout.Checked)
            {
                eNewFontStyle |= FontStyle.Strikeout;
            }
            if (tsbAlignRight.Checked)
            {
                if (rtbContent.SelectionAlignment != HorizontalAlignment.Right)
                {
                    int intTemp = rtbContent.SelectionIndent;
                    rtbContent.SelectionIndent = rtbContent.SelectionRightIndent;
                    rtbContent.SelectionRightIndent = intTemp;
                    rtbContent.SelectionAlignment = HorizontalAlignment.Right;
                }
            }
            else if (tsbAlignCenter.Checked)
            {
                if (rtbContent.SelectionAlignment != HorizontalAlignment.Center)
                {
                    rtbContent.SelectionIndent = 0;
                    rtbContent.SelectionRightIndent = 0;
                    rtbContent.SelectionAlignment = HorizontalAlignment.Center;
                }
            }
            else
            {
                if (rtbContent.SelectionAlignment != HorizontalAlignment.Left)
                {
                    int intTemp = rtbContent.SelectionIndent;
                    rtbContent.SelectionIndent = rtbContent.SelectionRightIndent;
                    rtbContent.SelectionRightIndent = intTemp;
                    rtbContent.SelectionAlignment = HorizontalAlignment.Left;
                }
                if (!tsbAlignLeft.Checked)
                    UpdateButtons(sender, e);
            }
            try
            {
                Font objCurrentFont = rtbContent.SelectionFont ?? DefaultFont;
                rtbContent.SelectionFont = new Font(objCurrentFont.FontFamily, objCurrentFont.Size, eNewFontStyle);
            }
            catch (ArgumentException)
            {
                UpdateButtons(sender, e);
            }

            rtbContent.Focus();
        }

        private void UpdateButtons(object sender, EventArgs e)
        {
            if (rtbContent.SelectionFont != null)
            {
                tsbBold.Checked = rtbContent.SelectionFont.Bold;
                tsbItalic.Checked = rtbContent.SelectionFont.Italic;
                tsbUnderline.Checked = rtbContent.SelectionFont.Underline;
                tsbStrikeout.Checked = rtbContent.SelectionFont.Strikeout;
            }
            else // Backup for weird cases where selection has no font, use the default font of the RichTextBox
            {
                tsbBold.Checked = rtbContent.Font.Bold;
                tsbItalic.Checked = rtbContent.Font.Italic;
                tsbUnderline.Checked = rtbContent.Font.Underline;
                tsbStrikeout.Checked = rtbContent.Font.Strikeout;
            }
            tsbAlignLeft.Checked = IsJustifyLeft;
            tsbAlignCenter.Checked = IsJustifyCenter;
            tsbAlignRight.Checked = IsJustifyRight;
            tsbUnorderedList.Checked = rtbContent.SelectionBullet;
            tsbIncreaseIndent.Enabled = !IsJustifyCenter;
            tsbDecreaseIndent.Enabled = !IsJustifyCenter;
            rtbContent.Cursor = rtbContent.SelectionType == RichTextBoxSelectionTypes.Object
                ? Cursors.SizeAll
                : Cursors.IBeam;
        }

        #region Properties

        public string Rtf
        {
            get => rtbContent.Rtf;
            set
            {
                if (!AllowFormatting)
                    rtbContent.Text = value.RtfToPlainText();
                else if (value.IsRtf())
                    rtbContent.Rtf = value;
                else
                    rtbContent.Text = value;
            }
        }

        public override string Text
        {
            get => rtbContent.Text;
            set => rtbContent.Text = value.RtfToPlainText();
        }

        public bool AllowFormatting
        {
            get => _blnAllowFormatting;
            set
            {
                if (_blnAllowFormatting != value)
                {
                    _blnAllowFormatting = value;
                    rtbContent.DetectUrls = value;
                    if (!Utils.IsDesignerMode)
                        tsControls.Visible = value && rtbContent.Focused;
                }
            }
        }

        private bool IsJustifyLeft => rtbContent.SelectionAlignment == HorizontalAlignment.Left;

        private bool IsJustifyCenter => rtbContent.SelectionAlignment == HorizontalAlignment.Center;

        private bool IsJustifyRight => rtbContent.SelectionAlignment == HorizontalAlignment.Right;

        #endregion

        #region Control Methods

        private void tsbFont_Click(object sender, EventArgs e)
        {
            using (FontDialog dlgNewFont = new FontDialog
            {
                Font = rtbContent.SelectionFont,
                FontMustExist = true
            })
            {
                if (dlgNewFont.ShowDialog() != DialogResult.OK)
                    return;
                rtbContent.SelectionFont = dlgNewFont.Font;
                UpdateButtons(sender, e);
            }
        }

        private void tsbForeColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlgNewColor = new ColorDialog { Color = rtbContent.SelectionColor })
            {
                if (dlgNewColor.ShowDialog() != DialogResult.OK)
                    return;
                rtbContent.SelectionColor = dlgNewColor.Color;
            }
        }

        private void tsbBackColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlgNewColor = new ColorDialog { Color = rtbContent.SelectionBackColor })
            {
                if (dlgNewColor.ShowDialog() != DialogResult.OK)
                    return;
                rtbContent.SelectionBackColor = dlgNewColor.Color;
            }
        }

        private void tsbUnorderedList_Click(object sender, EventArgs e)
        {
            rtbContent.SelectionBullet = tsbUnorderedList.Checked;
        }

        private void tsbAlignLeft_Click(object sender, EventArgs e)
        {
            tsbAlignCenter.Checked = false;
            tsbAlignRight.Checked = false;
            UpdateFont(sender, e);
        }

        private void tsbAlignCenter_Click(object sender, EventArgs e)
        {
            tsbAlignLeft.Checked = false;
            tsbAlignRight.Checked = false;
            UpdateFont(sender, e);
        }

        private void tsbAlignRight_Click(object sender, EventArgs e)
        {
            tsbAlignLeft.Checked = false;
            tsbAlignCenter.Checked = false;
            UpdateFont(sender, e);
        }

        private void tsbAlignLeft_CheckedChanged(object sender, EventArgs e)
        {
            tsbAlignLeft.CheckOnClick = !tsbAlignLeft.Checked;
        }

        private void tsbAlignCenter_CheckedChanged(object sender, EventArgs e)
        {
            tsbAlignCenter.CheckOnClick = !tsbAlignCenter.Checked;
        }

        private void tsbAlignRight_CheckedChanged(object sender, EventArgs e)
        {
            tsbAlignRight.CheckOnClick = !tsbAlignRight.Checked;
        }

        private void tsbIncreaseIndent_Click(object sender, EventArgs e)
        {
            if (IsJustifyLeft)
                rtbContent.SelectionIndent += rtbContent.BulletIndent;
            else if (IsJustifyRight)
                rtbContent.SelectionRightIndent += rtbContent.BulletIndent;
        }

        private void tsbDecreaseIndent_Click(object sender, EventArgs e)
        {
            if (IsJustifyLeft)
                rtbContent.SelectionIndent = Math.Max(0, rtbContent.SelectionIndent - rtbContent.BulletIndent);
            else if (IsJustifyRight)
                rtbContent.SelectionRightIndent = Math.Max(0, rtbContent.SelectionRightIndent - rtbContent.BulletIndent);
        }

        private void rtbContent_Enter(object sender, EventArgs e)
        {
            if (AllowFormatting)
                tsControls.Visible = true;
        }

        private void rtbContent_Leave(object sender, EventArgs e)
        {
            tsControls.Visible = false;
        }

        private void rtbContent_KeyDown(object sender, KeyEventArgs e)
        {
            ContentKeyDown?.Invoke(sender, e);
        }

        #endregion
    }
}
