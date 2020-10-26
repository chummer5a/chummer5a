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
using System.Collections.Concurrent;
using System.Drawing;
using System.Windows.Forms;
using Chummer.UI.Editors;
using Chummer.UI.Skills;
using Chummer.UI.Table;
using Microsoft.Win32;
using BorderStyle = System.Windows.Forms.BorderStyle;
using Button = System.Windows.Forms.Button;
using CheckBox = System.Windows.Forms.CheckBox;
using ListBox = System.Windows.Forms.ListBox;
using ListView = System.Windows.Forms.ListView;
using SystemColors = System.Drawing.SystemColors;
using TableCell = Chummer.UI.Table.TableCell;
using TextBox = System.Windows.Forms.TextBox;
using TreeNode = System.Windows.Forms.TreeNode;
using TreeView = System.Windows.Forms.TreeView;

namespace Chummer
{
    public static class ColorManager
    {
        // The setting for whether stuff uses dark mode or light mode is accessible purely through a specific registry key
        // So we save that key for accessing both at startup and should the setting be changed while Chummer is running
        private static readonly RegistryKey s_ObjPersonalizeKey;
        // While events that trigger on changes to a registry value are possible, they're a PITA in C#.
        // Checking for dark mode on a timer interval is less elegant, but also easier to set up, track, and debug.
        private static readonly Timer s_TmrDarkModeCheckerTimer;

        static ColorManager()
        {
            if (Utils.IsDesignerMode)
                return;

            try
            {
                s_ObjPersonalizeKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
            }
            catch (ObjectDisposedException)
            {
            }
            catch (System.Security.SecurityException)
            {
            }

            s_TmrDarkModeCheckerTimer = new Timer { Interval = 5000 }; // Poll registry every 5 seconds
            s_TmrDarkModeCheckerTimer.Tick += TmrDarkModeCheckerTimerOnTick;
            switch (GlobalOptions.ColorModeSetting)
            {
                case ColorMode.Automatic:
                    AutoApplyLightDarkMode();
                    break;
                case ColorMode.Light:
                    IsLightMode = true;
                    break;
                case ColorMode.Dark:
                    IsLightMode = false;
                    break;
            }
        }

        private static void TmrDarkModeCheckerTimerOnTick(object sender, EventArgs e)
        {
            AutoApplyLightDarkMode();
        }

        public static void AutoApplyLightDarkMode()
        {
            if (GlobalOptions.ColorModeSetting == ColorMode.Automatic)
            {
                IsLightMode = !DoesRegistrySayDarkMode();
                s_TmrDarkModeCheckerTimer.Enabled = true;
            }
        }

        public static bool DoesRegistrySayDarkMode()
        {
            object objLightModeResult = s_ObjPersonalizeKey?.GetValue("AppsUseLightTheme");
            if (objLightModeResult != null && int.TryParse(objLightModeResult.ToString(), out int intTemp))
                return intTemp == 0;
            return false;
        }

        public static void DisableAutoTimer()
        {
            s_TmrDarkModeCheckerTimer.Enabled = false;
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
                    Program.MainForm?.DoThreadSafe(() =>
                    {
                        using (new CursorWait(Program.MainForm))
                            Program.MainForm.UpdateLightDarkMode();
                    });
                }
            }
        }

        private static readonly ConcurrentDictionary<Color, Color> _dicLInvertedColors = new ConcurrentDictionary<Color, Color>();

        /// <summary>
        /// Returns a version of a color that has its lightness inverted. Suitable for getting dark & light mode versions of colors.
        /// </summary>
        /// <param name="objColor">Color whose lightness should be inverted.</param>
        /// <returns>New Color object identical to <paramref name="objColor"/>, but with its lightness values inverted.</returns>
        private static Color GetLInvertedColor(Color objColor)
        {
            if (!_dicLInvertedColors.TryGetValue(objColor, out Color objLInvertedColor))
            {
                objLInvertedColor = InvertLightness(objColor);
                _dicLInvertedColors.TryAdd(objColor, objLInvertedColor);
            }
            return objLInvertedColor;
        }

        public static Color WindowText => IsLightMode ? WindowTextLight : WindowTextDark;
        private static Color WindowTextLight => SystemColors.WindowText;
        private static Color WindowTextDark => GetLInvertedColor(WindowTextLight);
        public static Color Window => IsLightMode ? WindowLight : WindowDark;
        private static Color WindowLight => SystemColors.Window;
        private static Color WindowDark => GetLInvertedColor(WindowLight);
        public static Color InfoText => IsLightMode ? InfoTextLight : InfoTextDark;
        private static Color InfoTextLight => SystemColors.InfoText;
        private static Color InfoTextDark => GetLInvertedColor(InfoTextLight);
        public static Color Info => IsLightMode ? InfoLight : InfoDark;
        private static Color InfoLight => SystemColors.Info;
        private static Color InfoDark => GetLInvertedColor(InfoLight);
        public static Color GrayText => IsLightMode ? GrayTextLight : GrayTextDark;
        private static Color GrayTextLight => SystemColors.GrayText;
        private static Color GrayTextDark => GetLInvertedColor(GrayTextLight);
        public static Color HighlightText => IsLightMode ? HighlightTextLight : HighlightTextDark;
        private static Color HighlightTextLight => SystemColors.HighlightText;
        private static Color HighlightTextDark => GetLInvertedColor(HighlightTextLight);
        public static Color Highlight => IsLightMode ? HighlightLight : HighlightDark;
        private static Color HighlightLight => SystemColors.Highlight;
        private static Color HighlightDark => GetLInvertedColor(HighlightLight);
        public static Color ControlText => IsLightMode ? ControlTextLight : ControlTextDark;
        private static Color ControlTextLight => SystemColors.ControlText;
        private static Color ControlTextDark => GetLInvertedColor(ControlTextLight);
        public static Color ControlDarkest => IsLightMode ? ControlDarkestLight : ControlDarkestDark;
        private static Color ControlDarkestLight => SystemColors.ControlDarkDark;
        private static Color ControlDarkestDark => GetLInvertedColor(ControlDarkestLight);
        public static Color ControlDarker => IsLightMode ? ControlDarkerLight : ControlDarkerDark;
        private static Color ControlDarkerLight => SystemColors.ControlDark;
        private static Color ControlDarkerDark => GetLInvertedColor(ControlDarkerLight);
        public static Color Control => IsLightMode ? ControlLight : ControlDark;
        private static Color ControlLight => SystemColors.Control;
        private static Color ControlDark => GetLInvertedColor(ControlLight);
        public static Color ControlLighter => IsLightMode ? ControlLighterLight : ControlLighterDark;
        private static Color ControlLighterLight => SystemColors.ControlLight;
        private static Color ControlLighterDark => GetLInvertedColor(ControlLight);
        public static Color ControlLightest => IsLightMode ? ControlLightestLight : ControlLightestDark;
        private static Color ControlLightestLight => SystemColors.ControlLightLight;
        private static Color ControlLightestDark => GetLInvertedColor(ControlLightestLight);
        public static Color ButtonFace => IsLightMode ? ButtonFaceLight : ButtonFaceDark;
        private static Color ButtonFaceLight => SystemColors.ButtonFace;
        private static Color ButtonFaceDark => GetLInvertedColor(ButtonFaceLight);
        public static Color ButtonShadow => IsLightMode ? ButtonShadowLight : ButtonShadowDark;
        private static Color ButtonShadowLight => SystemColors.ButtonShadow;
        private static Color ButtonShadowDark => GetLInvertedColor(ButtonShadowLight);

        public static Color SplitterColor => IsLightMode ? SplitterColorLight : SplitterColorDark;
        private static Color SplitterColorLight => SystemColors.InactiveCaption;
        private static Color SplitterColorDark => GetLInvertedColor(SplitterColorLight);
        public static Color HasNotesColor => IsLightMode ? HasNotesColorLight : HasNotesColorDark;
        private static Color HasNotesColorLight => Color.Chocolate;
        private static Color HasNotesColorDark => Color.Chocolate;
        public static Color GrayHasNotesColor => IsLightMode ? GrayHasNotesColorLight : GrayHasNotesColorDark;
        private static Color GrayHasNotesColorLight => Color.Tan;
        private static Color GrayHasNotesColorDark => GetLInvertedColor(GrayHasNotesColorLight);
        public static Color ErrorColor => Color.Red;

        public static void UpdateLightDarkMode(this Control objControl)
        {
            ApplyColorsRecursively(objControl, IsLightMode);
        }

        public static void UpdateLightDarkMode(this Control objControl, bool blnLightMode)
        {
            ApplyColorsRecursively(objControl, blnLightMode);
        }

        public static void UpdateLightDarkMode(this ToolStripItem tssItem)
        {
            ApplyColorsRecursively(tssItem, IsLightMode);
        }

        public static void UpdateLightDarkMode(this ToolStripItem tssItem, bool blnLightMode)
        {
            ApplyColorsRecursively(tssItem, blnLightMode);
        }

        #region Color Inversion Methods

        private static Color InvertLightness(Color objColor)
        {
            // Built-in functions are in HSV/HSB, so we need to convert to HSL to invert lightness.
            float fltHue = objColor.GetHue() / 360.0f;
            float fltBrightness = objColor.GetBrightness();
            float fltLightness = fltBrightness * (1 - objColor.GetSaturation() / 2);
            float fltSaturationHSL = fltLightness > 0 && fltLightness < 1
                ? (fltBrightness - fltLightness) / Math.Min(fltLightness, 1 - fltLightness)
                : 0;
            return FromHSLA(fltHue, fltSaturationHSL, 1.0f - fltLightness, objColor.A);
        }

        private static void ApplyColorsRecursively(Control objControl, bool blnLightMode)
        {
            void ApplyButtonStyle()
            {
                // Buttons look weird if colored based on anything other than the default color scheme in dark mode
                objControl.ForeColor = SystemColors.ControlText;
            }
            switch (objControl)
            {
                case DataGridView objDataGridView:
                    objDataGridView.GridColor = blnLightMode ? ControlTextLight : ControlTextDark;
                    objDataGridView.DefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                    objDataGridView.DefaultCellStyle.BackColor = blnLightMode ? ControlLight : ControlDark;
                    objDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                    objDataGridView.ColumnHeadersDefaultCellStyle.BackColor = blnLightMode ? ControlLight : ControlDark;
                    objDataGridView.AlternatingRowsDefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                    objDataGridView.AlternatingRowsDefaultCellStyle.BackColor = blnLightMode ? ControlLighterLight : ControlLighterDark;
                    objDataGridView.RowTemplate.DefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                    objDataGridView.RowTemplate.DefaultCellStyle.BackColor = blnLightMode ? ControlLight : ControlDark;
                    foreach (DataGridViewTextBoxColumn objColumn in objDataGridView.Columns)
                    {
                        objColumn.DefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        objColumn.DefaultCellStyle.BackColor = blnLightMode ? ControlLight : ControlDark;
                    }
                    break;
                case SplitContainer objSplitControl:
                    objSplitControl.ForeColor = blnLightMode ? SplitterColorLight : SplitterColorDark;
                    objSplitControl.BackColor = blnLightMode ? SplitterColorLight : SplitterColorDark;
                    ApplyColorsRecursively(objSplitControl.Panel1, blnLightMode);
                    ApplyColorsRecursively(objSplitControl.Panel2, blnLightMode);
                    break;
                case TreeView treControl:
                    treControl.ForeColor = blnLightMode ? WindowTextLight : WindowTextDark;
                    treControl.BackColor = blnLightMode ? WindowLight : WindowDark;
                    treControl.LineColor = blnLightMode ? WindowTextLight : WindowTextDark;
                    foreach (TreeNode objNode in treControl.Nodes)
                        ApplyColorsRecursively(objNode, blnLightMode);
                    break;
                case TextBox txtControl:
                    txtControl.ForeColor = txtControl.ForeColor == ErrorColor
                        ? ErrorColor
                        : blnLightMode ? WindowTextLight : WindowTextDark;
                    txtControl.BackColor = txtControl.ReadOnly
                        ? blnLightMode ? ControlLight : ControlDark
                        : blnLightMode ? WindowLight : WindowDark;
                    break;
                case ListView _:
                case ListBox _:
                case ComboBox _:
                case TableCell _:
                    objControl.ForeColor = blnLightMode ? WindowTextLight : WindowTextDark;
                    objControl.BackColor = blnLightMode ? WindowLight : WindowDark;
                    break;
                case GroupBox _:
                    objControl.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                    objControl.BackColor = blnLightMode ? ControlLight : ControlDark;
                    break;
                case ContactControl _:
                case PetControl _:
                case SkillControl2 _:
                case KnowledgeSkillControl _:
                    // These controls have colors that are always data-bound
                    break;
                case RichTextBox _:
                    // Rtf TextBox is special because we don't want any color changes, otherwise it will mess up the saved Rtf text
                    return;
                case CheckBox chkControl:
                    if (chkControl.Appearance != Appearance.Button)
                    {
                        if (chkControl is ColorableCheckBox chkControlColored)
                        {
                            chkControlColored.DefaultColorScheme = blnLightMode;
                            chkControlColored.ForeColor = blnLightMode || chkControlColored.Enabled
                                ? blnLightMode ? ControlTextLight : ControlTextDark
                                : GrayText;
                            break;
                        }
                        goto default;
                    }
                    ApplyButtonStyle();
                    break;
                case Button cmdControl:
                    if (cmdControl.FlatStyle == FlatStyle.Flat)
                        goto default;
                    ApplyButtonStyle();
                    break;
                case HeaderCell _:
                    // Header cells should use inverted colors
                    objControl.ForeColor = blnLightMode ? ControlLightestLight : ControlLightestDark;
                    objControl.BackColor = blnLightMode ? ControlTextLight : ControlTextDark;
                    return;
                case TableLayoutPanel tlpControl:
                    if (tlpControl.BorderStyle != BorderStyle.None)
                        tlpControl.BorderStyle = blnLightMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                    goto default;
                case Form frmControl:
                    if (frmControl.MainMenuStrip != null)
                        foreach (ToolStripMenuItem tssItem in frmControl.MainMenuStrip.Items)
                            ApplyColorsRecursively(tssItem, blnLightMode);
                    goto default;
                case TabControl objTabControl:
                    foreach (TabPage tabPage in objTabControl.TabPages)
                        ApplyColorsRecursively(tabPage, blnLightMode);
                    goto default;
                case ToolStrip tssStrip:
                    foreach (ToolStripItem tssItem in tssStrip.Items)
                        ApplyColorsRecursively(tssItem, blnLightMode);
                    goto default;
                default:
                    if (objControl.ForeColor == (blnLightMode ? ControlDark : ControlLight))
                        objControl.ForeColor = blnLightMode ? ControlLight : ControlDark;
                    else if (objControl.ForeColor == (blnLightMode ? ControlDarkerDark : ControlDarkerLight))
                        objControl.ForeColor = blnLightMode ? ControlDarkerLight : ControlDarkerDark;
                    else if (objControl.ForeColor == (blnLightMode ? ControlDarkestDark : ControlDarkestLight))
                        objControl.ForeColor = blnLightMode ? ControlDarkestLight : ControlDarkestDark;
                    else
                        objControl.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                    // These controls never have backgrounds set explicitly, so shouldn't have their backgrounds overwritten
                    if (!(objControl is Label
                          || objControl is CheckBox
                          || objControl is PictureBox
                          || objControl is Button
                          || (objControl is Panel
                              && !(objControl is SplitterPanel
                                   || objControl is TabPage))))
                    {
                        if (objControl.BackColor == (blnLightMode ? ControlLighterDark : ControlLighterLight))
                            objControl.BackColor = blnLightMode ? ControlLighterLight : ControlLighterDark;
                        else if (objControl.BackColor == (blnLightMode ? ControlLightestDark : ControlLightestLight))
                            objControl.BackColor = blnLightMode ? ControlLightestLight : ControlLightestDark;
                        else
                            objControl.BackColor = blnLightMode ? ControlLight : ControlDark;
                    }

                    break;
            }

            foreach (Control objChild in objControl.Controls)
                ApplyColorsRecursively(objChild, blnLightMode);
        }

        private static void ApplyColorsRecursively(ToolStripItem tssItem, bool blnLightMode)
        {
            if (tssItem.ForeColor == (blnLightMode ? ControlDark : ControlLight))
                tssItem.ForeColor = blnLightMode ? ControlLight : ControlDark;
            else if (tssItem.ForeColor == (blnLightMode ? ControlDarkerDark : ControlDarkerLight))
                tssItem.ForeColor = blnLightMode ? ControlDarkerLight : ControlDarkerDark;
            else if (tssItem.ForeColor == (blnLightMode ? ControlDarkestDark : ControlDarkestLight))
                tssItem.ForeColor = blnLightMode ? ControlDarkestLight : ControlDarkestDark;
            else
                tssItem.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
            if (tssItem.BackColor == (blnLightMode ? ControlLighterDark : ControlLighterLight))
                tssItem.BackColor = blnLightMode ? ControlLighterLight : ControlLighterDark;
            else if (tssItem.BackColor == (blnLightMode ? ControlLightestDark : ControlLightestLight))
                tssItem.BackColor = blnLightMode ? ControlLightestLight : ControlLightestDark;
            else
                tssItem.BackColor = blnLightMode ? ControlLight : ControlDark;

            if (tssItem is ToolStripDropDownItem tssDropDownItem)
            {
                foreach (ToolStripItem tssDropDownChild in tssDropDownItem.DropDownItems)
                    ApplyColorsRecursively(tssDropDownChild, blnLightMode);
            }
        }

        private static void ApplyColorsRecursively(TreeNode nodNode, bool blnLightMode)
        {
            if (nodNode.ForeColor == (blnLightMode ? HasNotesColorDark : HasNotesColorLight))
                nodNode.ForeColor = blnLightMode ? HasNotesColorLight : HasNotesColorDark;
            else if (nodNode.ForeColor == (blnLightMode ? GrayHasNotesColorDark : GrayHasNotesColorLight))
                nodNode.ForeColor = blnLightMode ? GrayHasNotesColorLight : GrayHasNotesColorDark;
            else if (nodNode.ForeColor == (blnLightMode ? WindowTextDark : WindowTextLight))
                nodNode.ForeColor = blnLightMode ? WindowTextLight : WindowTextDark;
            nodNode.BackColor = blnLightMode ? WindowLight : WindowDark;

            foreach (TreeNode nodNodeChild in nodNode.Nodes)
                ApplyColorsRecursively(nodNodeChild, blnLightMode);
        }
        #endregion

        #region Color Utility Methods

        /// <summary>
        /// Returns an RGB Color from HSL values.
        /// </summary>
        /// <param name="fltHue">Hue value, between 0.0 and 1.0.</param>
        /// <param name="fltSaturation">Saturation value, between 0.0 and 1.0.</param>
        /// <param name="fltLightness">Lightness value, between 0.0 and 1.0.</param>
        /// <returns>A Color with RGB values corresponding to the HSL inputs.</returns>
        public static Color FromHSL(float fltHue, float fltSaturation, float fltLightness)
        {
            return FromHSLA(fltHue, fltSaturation, fltLightness, byte.MaxValue);
        }

        /// <summary>
        /// Returns an RGBA Color from HSLA values.
        /// </summary>
        /// <param name="fltHue">Hue value, between 0.0 and 1.0.</param>
        /// <param name="fltSaturation">Saturation value, between 0.0 and 1.0.</param>
        /// <param name="fltLightness">Lightness value, between 0.0 and 1.0.</param>
        /// <param name="chrAlpha">Alpha value.</param>
        /// <returns>A Color with RGBA values corresponding to the HSLA inputs.</returns>
        public static Color FromHSLA(float fltHue, float fltSaturation, float fltLightness, byte chrAlpha)
        {
            if (fltHue > 1.0f || fltHue < 0)
                throw new ArgumentOutOfRangeException(nameof(fltHue));
            if (fltSaturation > 1.0f || fltSaturation < 0)
                throw new ArgumentOutOfRangeException(nameof(fltSaturation));
            if (fltLightness > 1.0f || fltLightness < 0)
                throw new ArgumentOutOfRangeException(nameof(fltLightness));
            // Ramp up precision for intermediates
            double dblChroma = fltSaturation * (1.0 - Math.Abs(2.0 * fltLightness - 1.0));
            double dblCommon = fltLightness - dblChroma / 2.0;
            double dblCardinalHue = fltHue * 6.0;
            double dblCardinalHueMod2 = dblCardinalHue;
            while (dblCardinalHueMod2 > 2.0)
                dblCardinalHueMod2 -= 2.0;
            double dblMinorChroma = dblChroma * (1.0 - Math.Abs(dblCardinalHueMod2 - 1.0));

            double dblRed = dblCommon;
            double dblGreen = dblCommon;
            double dblBlue = dblCommon;
            switch ((int)Math.Floor(dblCardinalHue))
            {
                case 6:
                case 5:
                    dblRed += dblChroma;
                    dblBlue += dblMinorChroma;
                    break;
                case 4:
                    dblRed += dblMinorChroma;
                    dblBlue += dblChroma;
                    break;
                case 3:
                    dblGreen += dblMinorChroma;
                    dblBlue += dblChroma;
                    break;
                case 2:
                    dblGreen += dblChroma;
                    dblBlue += dblMinorChroma;
                    break;
                case 1:
                    dblRed += dblMinorChroma;
                    dblGreen += dblChroma;
                    break;
                default:
                    dblRed += dblChroma;
                    dblGreen += dblMinorChroma;
                    break;
            }

            return Color.FromArgb(chrAlpha,
                Convert.ToByte(dblRed * 255.0),
                Convert.ToByte(dblGreen * 255.0),
                Convert.ToByte(dblBlue * 255.0));
        }

        /// <summary>
        /// Returns an RGB Color from HSV (or HSB) values.
        /// </summary>
        /// <param name="fltHue">Hue value, between 0.0 and 1.0.</param>
        /// <param name="fltSaturation">Saturation value, between 0.0 and 1.0.</param>
        /// <param name="fltValue">Value/Brightness value, between 0.0 and 1.0.</param>
        /// <returns>A Color with RGB values corresponding to the HSV inputs.</returns>
        public static Color FromHSV(float fltHue, float fltSaturation, float fltValue)
        {
            return FromHSVA(fltHue, fltSaturation, fltValue, byte.MaxValue);
        }

        /// <summary>
        /// Returns an RGBA Color from HSVA (or HSBA) values.
        /// </summary>
        /// <param name="fltHue">Hue value, between 0.0 and 1.0.</param>
        /// <param name="fltSaturation">Saturation value, between 0.0 and 1.0.</param>
        /// <param name="fltValue">Value/Brightness value, between 0.0 and 1.0.</param>
        /// <param name="chrAlpha">Alpha value.</param>
        /// <returns>A Color with RGBA values corresponding to the HSVA inputs.</returns>
        public static Color FromHSVA(float fltHue, float fltSaturation, float fltValue, byte chrAlpha)
        {
            if (fltHue > 1.0f || fltHue < 0)
                throw new ArgumentOutOfRangeException(nameof(fltHue));
            if (fltSaturation > 1.0f || fltSaturation < 0)
                throw new ArgumentOutOfRangeException(nameof(fltSaturation));
            if (fltValue > 1.0f || fltValue < 0)
                throw new ArgumentOutOfRangeException(nameof(fltValue));
            // Ramp up precision for intermediates
            double dblChroma = fltSaturation * (double)fltValue;
            double dblCommon = fltValue - dblChroma;
            double dblCardinalHue = fltHue * 6.0;
            double dblCardinalHueMod2 = dblCardinalHue;
            while (dblCardinalHueMod2 > 2.0)
                dblCardinalHueMod2 -= 2.0;
            double dblMinorChroma = dblChroma * (1.0 - Math.Abs(dblCardinalHueMod2 - 1.0));

            double dblRed = dblCommon;
            double dblGreen = dblCommon;
            double dblBlue = dblCommon;
            switch ((int)Math.Floor(dblCardinalHue))
            {
                case 6:
                case 5:
                    dblRed += dblChroma;
                    dblBlue += dblMinorChroma;
                    break;
                case 4:
                    dblRed += dblMinorChroma;
                    dblBlue += dblChroma;
                    break;
                case 3:
                    dblGreen += dblMinorChroma;
                    dblBlue += dblChroma;
                    break;
                case 2:
                    dblGreen += dblChroma;
                    dblBlue += dblMinorChroma;
                    break;
                case 1:
                    dblRed += dblMinorChroma;
                    dblGreen += dblChroma;
                    break;
                default:
                    dblRed += dblChroma;
                    dblGreen += dblMinorChroma;
                    break;
            }

            return Color.FromArgb(chrAlpha,
                Convert.ToByte(dblRed * 255.0),
                Convert.ToByte(dblGreen * 255.0),
                Convert.ToByte(dblBlue * 255.0));
        }
        #endregion
    }
}
