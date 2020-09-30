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
using System.Globalization;
using System.Windows.Forms;
using Chummer.UI.Editors;
using Microsoft.Win32;
using SystemColors = System.Drawing.SystemColors;

namespace Chummer
{
    public static class ColorManager
    {
        private static readonly RegistryKey _objPersonalizeKey;
        // While events that trigger on changes to a registry value are possible, they're a PITA in C#.
        // Checking for dark mode on a timer interval is less elegant, but also easier to set up, track, and debug.
        private static readonly Timer _tmrDarkModeCheckerTimer;

        static ColorManager()
        {
            if (Utils.IsDesignerMode)
                return;

            try
            {
                _objPersonalizeKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
            }
            catch (ObjectDisposedException)
            {
            }
            catch (System.Security.SecurityException)
            {
            }

            if (_objPersonalizeKey != null)
            {
                object objLightModeResult = _objPersonalizeKey.GetValue("AppsUseLightTheme");
                if (objLightModeResult != null && int.TryParse(objLightModeResult.ToString(), out int intTemp))
                    IsLightMode = intTemp != 0;
                _tmrDarkModeCheckerTimer = new Timer {Interval = 5000}; // Poll registry every 5 seconds
                _tmrDarkModeCheckerTimer.Tick += CheckAndRefreshLightDarkMode;
                _tmrDarkModeCheckerTimer.Enabled = true;
            }
        }

        private static void CheckAndRefreshLightDarkMode(object sender, EventArgs e)
        {
            if (_objPersonalizeKey != null && _tmrDarkModeCheckerTimer != null)
            {
                object objLightModeResult = _objPersonalizeKey.GetValue("AppsUseLightTheme");
                if (objLightModeResult != null && int.TryParse(objLightModeResult.ToString(), out int intTemp))
                    IsLightMode = intTemp != 0;
            }
        }

        private static bool _blnIsLightMode = true;
        public static bool IsLightMode
        {
            get => _blnIsLightMode;
            set
            {
                if (_blnIsLightMode != value)
                {
                    _blnIsLightMode = value;
                    Program.MainForm.DoThreadSafe(() =>
                    {
                        using (new CursorWait(Program.MainForm))
                            Program.MainForm.UpdateLightDarkMode();
                    });
                }
            }
        }

        public static Color WindowText => IsLightMode ? SystemColors.WindowText : SystemColors.Window;
        public static Color Window => IsLightMode ? SystemColors.Window : SystemColors.WindowText;
        public static Color InfoText => IsLightMode ? SystemColors.InfoText : SystemColors.Info;
        public static Color Info => IsLightMode ? SystemColors.Info : SystemColors.InfoText;
        public static Color GrayText => SystemColors.GrayText;
        public static Color Highlight => SystemColors.Highlight;
        public static Color HighlightText => SystemColors.HighlightText;
        public static Color ControlText => IsLightMode ? SystemColors.ControlText : SystemColors.ControlLightLight;
        public static Color ControlDarkDark => IsLightMode ? SystemColors.ControlDarkDark : SystemColors.Control;
        public static Color ControlDark => IsLightMode ? SystemColors.ControlDark : SystemColors.ControlLight;
        public static Color Control => IsLightMode ? SystemColors.Control : SystemColors.ControlDarkDark;
        public static Color ControlLight => IsLightMode ? SystemColors.ControlLight : SystemColors.ControlDark;
        public static Color ControlLightLight => IsLightMode ? SystemColors.ControlLightLight : SystemColors.ControlText;
        public static Color ButtonFace => IsLightMode ? SystemColors.ButtonFace : SystemColors.ButtonShadow;
        public static Color ButtonShadow => IsLightMode ? SystemColors.ButtonShadow : SystemColors.ButtonFace;

        public static Color SplitterColor => IsLightMode ? SplitterColorLight : SplitterColorDark;
        private static Color SplitterColorLight => Color.LightBlue;
        private static Color SplitterColorDark => Color.DarkBlue;
        public static Color HasNotesColor => IsLightMode ? HasNotesColorLight : HasNotesColorDark;
        private static Color HasNotesColorLight => Color.SaddleBrown;
        private static Color HasNotesColorDark => Color.Chocolate;
        public static Color GrayHasNotesColor => Color.Tan;
        public static Color ErrorColor => Color.Red;

        public static void UpdateLightDarkMode(this Control objControl)
        {
            ApplyColorsRecusively(objControl);
        }

        public static void UpdateLightDarkMode(this ToolStripItem tssItem)
        {
            ApplyColorsRecusively(tssItem);
        }


        #region Color Inversion Methods

        private static void ApplyColorsRecusively(Control objControl)
        {
            switch (objControl)
            {
                case DataGridView objDataGridView:
                    objDataGridView.GridColor = WindowText;
                    objDataGridView.DefaultCellStyle.ForeColor = ControlText;
                    objDataGridView.DefaultCellStyle.BackColor = Control;
                    objDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = ControlText;
                    objDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Control;
                    objDataGridView.AlternatingRowsDefaultCellStyle.ForeColor = ControlText;
                    objDataGridView.AlternatingRowsDefaultCellStyle.BackColor = ControlLight;
                    objDataGridView.RowTemplate.DefaultCellStyle.ForeColor = ControlText;
                    objDataGridView.RowTemplate.DefaultCellStyle.BackColor = Control;
                    foreach (DataGridViewTextBoxColumn objColumn in objDataGridView.Columns)
                    {
                        objColumn.DefaultCellStyle.ForeColor = ControlText;
                        objColumn.DefaultCellStyle.BackColor = Control;
                    }
                    break;
                case SplitContainer objSplitControl:
                    objSplitControl.ForeColor = SplitterColor;
                    objSplitControl.BackColor = SplitterColor;
                    ApplyColorsRecusively(objSplitControl.Panel1);
                    ApplyColorsRecusively(objSplitControl.Panel2);
                    break;
                case TreeView treControl:
                    treControl.ForeColor = WindowText;
                    treControl.BackColor = Window;
                    treControl.LineColor = WindowText;
                    foreach (TreeNode objNode in treControl.Nodes)
                        ApplyColorsRecusively(objNode);
                    break;
                case TextBox txtControl:
                    txtControl.ForeColor = WindowText;
                    txtControl.BackColor = Window;
                    break;
                case ListView lstControl:
                    lstControl.ForeColor = WindowText;
                    lstControl.BackColor = Window;
                    break;
                case ListBox lsbControl:
                    lsbControl.ForeColor = WindowText;
                    lsbControl.BackColor = Window;
                    break;
                case GroupBox gpbControl:
                    gpbControl.ForeColor = ControlText;
                    gpbControl.BackColor = Control;
                    break;
                case RichTextBox _:
                case RtfEditor _:
                    // Rtf Editor is special because we don't want any color changes for the controls inside of it, otherwise it will mess up the saved Rtf text
                    return;
                case Button cmdControl:
                    if (cmdControl.FlatStyle == FlatStyle.Flat)
                        goto default;
                    // Buttons look weird if colored based on anything other than the default color scheme in dark mode
                    cmdControl.ForeColor = SystemColors.ControlText;
                    cmdControl.BackColor = Color.Transparent;
                    break;
                case TableLayoutPanel tlpControl:
                    if (tlpControl.BorderStyle != BorderStyle.None)
                        tlpControl.BorderStyle = IsLightMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                    goto default;
                case Form frmControl:
                    if (frmControl.MainMenuStrip != null)
                        foreach (ToolStripMenuItem tssItem in frmControl.MainMenuStrip.Items)
                            ApplyColorsRecusively(tssItem);
                    goto default;
                case TabControl objTabControl:
                    foreach (TabPage tabPage in objTabControl.TabPages)
                        ApplyColorsRecusively(tabPage);
                    goto default;
                case ToolStrip tssStrip:
                    foreach (ToolStripItem tssItem in tssStrip.Items)
                        ApplyColorsRecusively(tssItem);
                    goto default;
                case CheckBox chkControl:
                    if (chkControl.Appearance == Appearance.Button)
                    {
                        // This checkbox is a condition monitor box, so special treatment is required
                        chkControl.ForeColor = SystemColors.ControlText;
                        // Do not set backgrounds because they will be set based on overflow anyway
                        break;
                    }
                    goto default;
                default:
                    if (objControl.ForeColor == Control)
                        objControl.ForeColor = ControlDarkDark;
                    else if (objControl.ForeColor == ControlLight)
                        objControl.ForeColor = ControlDark;
                    else
                        objControl.ForeColor = ControlText;
                    if (objControl.BackColor == ControlText)
                        objControl.BackColor = ControlLightLight;
                    else if (objControl.BackColor == ControlDark)
                        objControl.BackColor = ControlLight;
                    else
                        objControl.BackColor = Control;
                    break;
            }

            foreach (Control objChild in objControl.Controls)
                ApplyColorsRecusively(objChild);
        }

        private static void ApplyColorsRecusively(ToolStripItem tssItem)
        {
            if (tssItem.ForeColor == Control)
                tssItem.ForeColor = ControlDarkDark;
            else if (tssItem.ForeColor == ControlLight)
                tssItem.ForeColor = ControlDark;
            else
                tssItem.ForeColor = ControlText;
            if (tssItem.BackColor == ControlText)
                tssItem.BackColor = ControlLightLight;
            else if (tssItem.BackColor == ControlDark)
                tssItem.BackColor = ControlLight;
            else
                tssItem.BackColor = Control;

            if (tssItem is ToolStripDropDownItem tssDropDownItem)
                foreach (ToolStripItem tssDropDownChild in tssDropDownItem.DropDownItems)
                    ApplyColorsRecusively(tssDropDownChild);
        }

        private static void ApplyColorsRecusively(TreeNode nodNode)
        {
            if (IsLightMode)
            {
                if (nodNode.ForeColor == HasNotesColorDark)
                    nodNode.ForeColor = HasNotesColor;
                else if (nodNode.ForeColor == Window)
                    nodNode.ForeColor = WindowText;
            }
            else if (nodNode.ForeColor == HasNotesColorLight)
                nodNode.ForeColor = HasNotesColor;
            else if (nodNode.ForeColor == Window)
                nodNode.ForeColor = WindowText;
            nodNode.BackColor = Window;

            foreach (TreeNode nodNodeChild in nodNode.Nodes)
                ApplyColorsRecusively(nodNodeChild);
        }
        #endregion
    }
}
