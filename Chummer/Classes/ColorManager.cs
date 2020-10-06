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

            if (s_ObjPersonalizeKey != null)
            {
                object objLightModeResult = s_ObjPersonalizeKey.GetValue("AppsUseLightTheme");
                if (objLightModeResult != null && int.TryParse(objLightModeResult.ToString(), out int intTemp))
                    IsLightMode = intTemp != 0;
                s_TmrDarkModeCheckerTimer = new Timer {Interval = 5000}; // Poll registry every 5 seconds
                s_TmrDarkModeCheckerTimer.Tick += CheckAndRefreshLightDarkMode;
                s_TmrDarkModeCheckerTimer.Enabled = true;
            }
        }

        private static void CheckAndRefreshLightDarkMode(object sender, EventArgs e)
        {
            if (s_ObjPersonalizeKey != null && s_TmrDarkModeCheckerTimer != null)
            {
                object objLightModeResult = s_ObjPersonalizeKey.GetValue("AppsUseLightTheme");
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
        private static Color ButtonFaceLight => SystemColors.ButtonShadow;
        private static Color ButtonFaceDark => GetLInvertedColor(ButtonShadowLight);
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
            ApplyColorsRecursively(objControl);
        }

        public static void UpdateLightDarkMode(this ToolStripItem tssItem)
        {
            ApplyColorsRecursively(tssItem);
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

        private static void ApplyColorsRecursively(Control objControl)
        {
            void ApplyButtonStyle()
            {
                // Buttons look weird if colored based on anything other than the default color scheme in dark mode
                objControl.ForeColor = SystemColors.ControlText;
            }
            switch (objControl)
            {
                case DataGridView objDataGridView:
                    objDataGridView.GridColor = ControlText;
                    objDataGridView.DefaultCellStyle.ForeColor = ControlText;
                    objDataGridView.DefaultCellStyle.BackColor = Control;
                    objDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = ControlText;
                    objDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Control;
                    objDataGridView.AlternatingRowsDefaultCellStyle.ForeColor = ControlText;
                    objDataGridView.AlternatingRowsDefaultCellStyle.BackColor = ControlLighter;
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
                    ApplyColorsRecursively(objSplitControl.Panel1);
                    ApplyColorsRecursively(objSplitControl.Panel2);
                    break;
                case TreeView treControl:
                    treControl.ForeColor = WindowText;
                    treControl.BackColor = Window;
                    treControl.LineColor = WindowText;
                    foreach (TreeNode objNode in treControl.Nodes)
                        ApplyColorsRecursively(objNode);
                    break;
                case TextBox txtControl:
                    txtControl.ForeColor = txtControl.ForeColor == ErrorColor ? ErrorColor : WindowText;
                    txtControl.BackColor = txtControl.ReadOnly ? Control : Window;
                    break;
                case ListView _:
                case ListBox _:
                case ComboBox _:
                case TableCell _:
                    objControl.ForeColor = WindowText;
                    objControl.BackColor = Window;
                    break;
                case GroupBox _:
                    objControl.ForeColor = ControlText;
                    objControl.BackColor = Control;
                    break;
                case ContactControl _:
                case PetControl _:
                case SkillControl2 _:
                case KnowledgeSkillControl _:
                    // These controls have colors that are always data-bound
                    break;
                case RichTextBox rtbControl:
                    rtbControl.ForeColor = rtbControl.ForeColor == ErrorColor ? ErrorColor : WindowText;
                    rtbControl.BackColor = rtbControl.ReadOnly ? Control : Window;
                    break;
                case RtfEditor _:
                    // Rtf Editor is special because we don't want any color changes for the controls inside of it, otherwise it will mess up the saved Rtf text
                    return;
                case CheckBox chkControl:
                    if (chkControl.Appearance != Appearance.Button)
                    {
                        if (chkControl is ColorableCheckBox chkControlColored)
                        {
                            if (chkControlColored.Enabled)
                            {
                                chkControlColored.ForeColor = ControlText;
                                chkControlColored.FlatAppearance.MouseDownBackColor = ControlDarkest;
                                chkControlColored.FlatAppearance.MouseOverBackColor = ControlDarker;
                            }
                            else
                            {
                                chkControlColored.ForeColor = GrayText;
                                chkControlColored.FlatAppearance.MouseDownBackColor = chkControlColored.BackColor;
                                chkControlColored.FlatAppearance.MouseOverBackColor = chkControlColored.BackColor;
                            }
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
                    objControl.ForeColor = ControlLightest;
                    objControl.BackColor = ControlText;
                    return;
                case TableLayoutPanel tlpControl:
                    if (tlpControl.BorderStyle != BorderStyle.None)
                        tlpControl.BorderStyle = IsLightMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                    goto default;
                case Form frmControl:
                    if (frmControl.MainMenuStrip != null)
                        foreach (ToolStripMenuItem tssItem in frmControl.MainMenuStrip.Items)
                            ApplyColorsRecursively(tssItem);
                    goto default;
                case TabControl objTabControl:
                    foreach (TabPage tabPage in objTabControl.TabPages)
                        ApplyColorsRecursively(tabPage);
                    goto default;
                case ToolStrip tssStrip:
                    foreach (ToolStripItem tssItem in tssStrip.Items)
                        ApplyColorsRecursively(tssItem);
                    goto default;
                default:
                    if (objControl.ForeColor == (IsLightMode ? ControlDark : ControlLight))
                        objControl.ForeColor = Control;
                    else if (objControl.ForeColor == (IsLightMode ? ControlDarkerDark : ControlDarkerLight))
                        objControl.ForeColor = ControlDarker;
                    else if (objControl.ForeColor == (IsLightMode ? ControlDarkestDark : ControlDarkestLight))
                        objControl.ForeColor = ControlDarkest;
                    else
                        objControl.ForeColor = ControlText;
                    // These controls never have backgrounds set explicitly, so shouldn't have their backgrounds overwritten
                    if (!(objControl is Label
                          || objControl is CheckBox
                          || objControl is PictureBox
                          || objControl is Button
                          || (objControl is Panel
                              && !(objControl is SplitterPanel
                                   || objControl is TabPage))))
                    {
                        if (objControl.BackColor == (IsLightMode ? ControlLighterDark : ControlLighterLight))
                            objControl.BackColor = ControlLighter;
                        else if (objControl.BackColor == (IsLightMode ? ControlLightestDark : ControlLightestLight))
                            objControl.BackColor = ControlLightest;
                        else
                            objControl.BackColor = Control;
                    }

                    break;
            }

            foreach (Control objChild in objControl.Controls)
                ApplyColorsRecursively(objChild);
        }

        private static void ApplyColorsRecursively(ToolStripItem tssItem)
        {
            if (tssItem.ForeColor == (IsLightMode ? ControlDark : ControlLight))
                tssItem.ForeColor = Control;
            else if (tssItem.ForeColor == (IsLightMode ? ControlDarkerDark : ControlDarkerLight))
                tssItem.ForeColor = ControlDarker;
            else if (tssItem.ForeColor == (IsLightMode ? ControlDarkestDark : ControlDarkestLight))
                tssItem.ForeColor = ControlDarkest;
            else
                tssItem.ForeColor = ControlText;
            if (tssItem.BackColor == (IsLightMode ? ControlLighterDark : ControlLighterLight))
                tssItem.BackColor = ControlLighter;
            else if (tssItem.BackColor == (IsLightMode ? ControlLightestDark : ControlLightestLight))
                tssItem.BackColor = ControlLightest;
            else
                tssItem.BackColor = Control;

            if (tssItem is ToolStripDropDownItem tssDropDownItem)
            {
                foreach (ToolStripItem tssDropDownChild in tssDropDownItem.DropDownItems)
                    ApplyColorsRecursively(tssDropDownChild);
            }
        }

        private static void ApplyColorsRecursively(TreeNode nodNode)
        {
            if (nodNode.ForeColor == (IsLightMode ? HasNotesColorDark : HasNotesColorLight))
                nodNode.ForeColor = HasNotesColor;
            else if (nodNode.ForeColor == (IsLightMode ? WindowTextDark : WindowTextLight))
                nodNode.ForeColor = WindowText;
            nodNode.BackColor = Window;

            foreach (TreeNode nodNodeChild in nodNode.Nodes)
                ApplyColorsRecursively(nodNodeChild);
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
