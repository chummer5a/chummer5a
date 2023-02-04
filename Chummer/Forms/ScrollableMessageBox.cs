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

namespace Chummer.Forms
{
    public sealed partial class ScrollableMessageBox : Form
    {
        private readonly MessageBoxButtons _eBoxButtons;

        public static DialogResult Show(string text, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK,
                                        MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defButton = MessageBoxDefaultButton.Button1,
                                        MessageBoxOptions options = 0)
        {
            using (ThreadSafeForm<ScrollableMessageBox> frmMessageBox
                   = ThreadSafeForm<ScrollableMessageBox>.Get(() => new ScrollableMessageBox(text, caption, buttons, icon, defButton, options)))
                return frmMessageBox.ShowDialogSafe();
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK,
                                        MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defButton = MessageBoxDefaultButton.Button1,
                                        MessageBoxOptions options = 0)
        {
            using (ThreadSafeForm<ScrollableMessageBox> frmMessageBox
                   = ThreadSafeForm<ScrollableMessageBox>.Get(
                       () => new ScrollableMessageBox(text, caption, buttons, icon, defButton, options)))
                return frmMessageBox.ShowDialogSafe(owner);
        }

        private ScrollableMessageBox(string text, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defButton = MessageBoxDefaultButton.Button1, MessageBoxOptions options = 0)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            InitializeComponent();
            this.UpdateLightDarkMode();

            // This whole bunch of math makes sure that our message box can expand to be big if need be, shrink to its smallest size otherwise, and still use scrollbars appropriately
            int intMaxHeight = txtText.Height * 3;
            txtText.Dock = DockStyle.None;
            txtText.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtText.Text = text;
            (int intNumDisplayedLines, int intMaxLineHeight) = txtText.MeasureLineHeights();
            int intIdealHeight = intNumDisplayedLines * (intMaxLineHeight + 2);
            if (intIdealHeight > intMaxHeight)
                txtText.Height = intMaxHeight;
            else
            {
                txtText.Height = intIdealHeight;
                tlpTop.Padding = new Padding(tlpTop.Padding.Left, tlpTop.Padding.Top, tlpTop.Padding.Right, tlpTop.Padding.Top);
            }
            txtText.AutoSetScrollbars();

            Text = string.IsNullOrWhiteSpace(caption) ? string.Empty : caption;
            _eBoxButtons = buttons;
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    cmdButton1.Text = NativeMethods.GetSystemString(NativeMethods.SystemString.OK);
                    tlpButtons.Controls.Remove(cmdButton2);
                    tlpButtons.Controls.Remove(cmdButton3);
                    cmdButton2.Dispose();
                    cmdButton2 = null;
                    cmdButton3.Dispose();
                    cmdButton3 = null;
                    break;
                case MessageBoxButtons.OKCancel:
                    cmdButton1.Text = NativeMethods.GetSystemString(NativeMethods.SystemString.Cancel);
                    cmdButton2.Text = NativeMethods.GetSystemString(NativeMethods.SystemString.OK);
                    tlpButtons.Controls.Remove(cmdButton3);
                    cmdButton3.Dispose();
                    cmdButton3 = null;
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    cmdButton1.Text = NativeMethods.GetSystemString(NativeMethods.SystemString.Ignore);
                    cmdButton2.Text = NativeMethods.GetSystemString(NativeMethods.SystemString.Retry);
                    cmdButton3.Text = NativeMethods.GetSystemString(NativeMethods.SystemString.Abort);
                    break;
                case MessageBoxButtons.YesNoCancel:
                    cmdButton1.Text = NativeMethods.GetSystemString(NativeMethods.SystemString.Cancel);
                    cmdButton2.Text = NativeMethods.GetSystemString(NativeMethods.SystemString.No);
                    cmdButton3.Text = NativeMethods.GetSystemString(NativeMethods.SystemString.Yes);
                    break;
                case MessageBoxButtons.YesNo:
                    cmdButton1.Text = NativeMethods.GetSystemString(NativeMethods.SystemString.No);
                    cmdButton2.Text = NativeMethods.GetSystemString(NativeMethods.SystemString.Yes);
                    tlpButtons.Controls.Remove(cmdButton3);
                    cmdButton3.Dispose();
                    cmdButton3 = null;
                    break;
                case MessageBoxButtons.RetryCancel:
                    cmdButton1.Text = NativeMethods.GetSystemString(NativeMethods.SystemString.Cancel);
                    cmdButton2.Text = NativeMethods.GetSystemString(NativeMethods.SystemString.Retry);
                    tlpButtons.Controls.Remove(cmdButton3);
                    cmdButton3.Dispose();
                    cmdButton3 = null;
                    break;
            }

            switch (icon)
            {
                case MessageBoxIcon.None:
                    imgIcon.Visible = false;
                    break;
                case MessageBoxIcon.Error:
                    imgIcon.Image = Utils.GetStockIconBitmapsForSystemIcon(SystemIcons.Error);
                    break;
                case MessageBoxIcon.Question:
                    imgIcon.Image = Utils.GetStockIconBitmapsForSystemIcon(SystemIcons.Question);
                    break;
                case MessageBoxIcon.Warning:
                    imgIcon.Image = Utils.GetStockIconBitmapsForSystemIcon(SystemIcons.Warning);
                    break;
                case MessageBoxIcon.Information:
                    imgIcon.Image = Utils.GetStockIconBitmapsForSystemIcon(SystemIcons.Information);
                    break;
            }

            if (options.HasFlag(MessageBoxOptions.ServiceNotification) || options.HasFlag(MessageBoxOptions.DefaultDesktopOnly))
            {
                StartPosition = FormStartPosition.CenterScreen;
            }
            if (options.HasFlag(MessageBoxOptions.RightAlign))
            {
                txtText.TextAlign = HorizontalAlignment.Right;
            }
            if (options.HasFlag(MessageBoxOptions.RtlReading))
            {
                txtText.RightToLeft = RightToLeft.Yes;
            }

            switch (defButton)
            {
                case MessageBoxDefaultButton.Button2:
                    if (cmdButton2 != null)
                        cmdButton2.Focus();
                    else
                        cmdButton1.Focus();
                    break;
                case MessageBoxDefaultButton.Button3:
                    cmdButton1.Focus();
                    break;
                default:
                    if (cmdButton3 != null)
                        cmdButton3.Focus();
                    else if (cmdButton2 != null)
                        cmdButton2.Focus();
                    else
                        cmdButton1.Focus();
                    break;
            }
        }

        [Obsolete("This constructor is for use by form designers only.", true)]
        public ScrollableMessageBox()
        {
            InitializeComponent();
        }

        private void cmdButton1_Click(object sender, EventArgs e)
        {
            switch (_eBoxButtons)
            {
                case MessageBoxButtons.OK:
                    DialogResult = DialogResult.OK;
                    break;
                case MessageBoxButtons.OKCancel:
                case MessageBoxButtons.YesNoCancel:
                case MessageBoxButtons.RetryCancel:
                    DialogResult = DialogResult.Cancel;
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    DialogResult = DialogResult.Ignore;
                    break;
                case MessageBoxButtons.YesNo:
                    DialogResult = DialogResult.No;
                    break;
                default:
                    return;
            }
            Close();
        }

        private void cmdButton2_Click(object sender, EventArgs e)
        {
            switch (_eBoxButtons)
            {
                case MessageBoxButtons.OKCancel:
                    DialogResult = DialogResult.OK;
                    break;
                case MessageBoxButtons.YesNoCancel:
                    DialogResult = DialogResult.No;
                    break;
                case MessageBoxButtons.RetryCancel:
                case MessageBoxButtons.AbortRetryIgnore:
                    DialogResult = DialogResult.Retry;
                    break;
                case MessageBoxButtons.YesNo:
                    DialogResult = DialogResult.Yes;
                    break;
                default:
                    return;
            }
            Close();
        }

        private void cmdButton3_Click(object sender, EventArgs e)
        {
            switch (_eBoxButtons)
            {
                case MessageBoxButtons.YesNoCancel:
                    DialogResult = DialogResult.Yes;
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    DialogResult = DialogResult.Abort;
                    break;
                default:
                    return;
            }
            Close();
        }
    }
}
