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

        private static readonly ConcurrentDictionary<Color, Color> s_DicDarkModeColors = new ConcurrentDictionary<Color, Color>();
        private static readonly ConcurrentDictionary<Color, Color> s_DicInverseDarkModeColors = new ConcurrentDictionary<Color, Color>();
        private static readonly ConcurrentDictionary<Color, Color> s_DicDimmedColors = new ConcurrentDictionary<Color, Color>();
        private static readonly ConcurrentDictionary<Color, Color> s_DicBrightenedColors = new ConcurrentDictionary<Color, Color>();

        /// <summary>
        /// Returns a version of a color that has its lightness almost inverted (slightly increased lightness from inversion, slight desaturation)
        /// </summary>
        /// <param name="objColor">Color whose lightness and saturation should be adjusted for Dark Mode.</param>
        /// <returns>New Color object identical to <paramref name="objColor"/>, but with lightness and saturation adjusted for Dark Mode.</returns>
        public static Color GenerateDarkModeColor(Color objColor)
        {
            if (!s_DicDarkModeColors.TryGetValue(objColor, out Color objDarkModeColor))
            {
                objDarkModeColor = GetDarkModeVersion(objColor);
                s_DicDarkModeColors.TryAdd(objColor, objDarkModeColor);
            }
            return objDarkModeColor;
        }

        /// <summary>
        /// Returns an inverted version of a color that has gone through GenerateDarkModeColor()
        /// </summary>
        /// <param name="objColor">Color whose Dark Mode conversions for lightness and saturation should be inverted.</param>
        /// <returns>New Color object identical to <paramref name="objColor"/>, but with its Dark Mode conversion inverted.</returns>
        public static Color GenerateInverseDarkModeColor(Color objColor)
        {
            if (!s_DicInverseDarkModeColors.TryGetValue(objColor, out Color objInverseDarkModeColor))
            {
                objInverseDarkModeColor = InverseGetDarkModeVersion(objColor);
                s_DicInverseDarkModeColors.TryAdd(objColor, objInverseDarkModeColor);
            }
            return objInverseDarkModeColor;
        }

        /// <summary>
        /// Returns a version of a color that has is adapted to the current Color mode setting (same color in Light mode, changed one in Dark mode)
        /// </summary>
        /// <param name="objColor">Color as it would be in Light mode</param>
        /// <returns>New Color object identical to <paramref name="objColor"/>, but potentially adapted to dark mode.</returns>
        public static Color GenerateCurrentModeColor(Color objColor)
        {
            return IsLightMode ? objColor : GenerateDarkModeColor(objColor);
        }

        /// <summary>
        /// Returns a version of a color that is independent of the current Color mode and can savely be used for storing.
        /// </summary>
        /// <param name="objColor">Color as it is shown in current color mode</param>
        /// <returns>New Color object identical to <paramref name="objColor"/>, but potentially adapted to light mode.</returns>
        public static Color GenerateModeIndependentColor(Color objColor)
        {
            return IsLightMode ? objColor : GenerateInverseDarkModeColor(objColor);
        }

        /// <summary>
        /// Returns a version of a color that has its lightness dimmed down in Light mode or brightened in Dark Mode
        /// </summary>
        /// <param name="objColor">Color whose lightness should be dimmed.</param>
        /// <returns>New Color object identical to <paramref name="objColor"/>, but with its lightness values dimmed.</returns>
        public static Color GenerateCurrentModeDimmedColor(Color objColor)
        {
            Color objRetColor;
            if (IsLightMode)
            {
                if (!s_DicDimmedColors.TryGetValue(objColor, out objRetColor))
                {
                    objRetColor = GetDimmedVersion(objColor);
                    s_DicDimmedColors.TryAdd(objColor, objRetColor);
                }
            }
            else
            {
                if (!s_DicBrightenedColors.TryGetValue(objColor, out objRetColor))
                {
                    objRetColor = GetBrightenedVersion(objColor);
                    s_DicBrightenedColors.TryAdd(objColor, objRetColor);
                }
            }

            return objRetColor;
        }

        /// <summary>
        /// Because the transforms applied to convert a Light Mode color to Dark Mode cannot produce some ranges of lightness and saturation, not all colors are valid in Dark Mode.
        /// This function takes a color intended for Dark Mode and converts it to the closest possible color that is valid in Dark Mode.
        /// If the original color is valid in Dark Mode to begin with, the transforms should end up reproducing it.
        /// </summary>
        /// <param name="objColor">Color to adjust, originally specified within Dark Mode.</param>
        /// <returns>New Color very similar to <paramref name="objColor"/>, but with lightness and saturation values set to within the range allowable in Dark Mode.</returns>
        private static Color TransformToDarkModeValidVersion(Color objColor)
        {
            return GenerateDarkModeColor(GenerateInverseDarkModeColor(objColor));
        }


        public static Color WindowText => IsLightMode ? WindowTextLight : WindowTextDark;
        private static Color WindowTextLight => SystemColors.WindowText;
        private static Color WindowTextDark => GenerateDarkModeColor(WindowTextLight);
        public static Color Window => IsLightMode ? WindowLight : WindowDark;
        private static Color WindowLight => SystemColors.Window;
        private static Color WindowDark => GenerateDarkModeColor(WindowLight);
        public static Color InfoText => IsLightMode ? InfoTextLight : InfoTextDark;
        private static Color InfoTextLight => SystemColors.InfoText;
        private static Color InfoTextDark => GenerateDarkModeColor(InfoTextLight);
        public static Color Info => IsLightMode ? InfoLight : InfoDark;
        private static Color InfoLight => SystemColors.Info;
        private static Color InfoDark => GenerateDarkModeColor(InfoLight);
        public static Color GrayText => IsLightMode ? GrayTextLight : GrayTextDark;
        private static Color GrayTextLight => SystemColors.GrayText;
        private static Color GrayTextDark => GenerateDarkModeColor(GrayTextLight);
        public static Color HighlightText => IsLightMode ? HighlightTextLight : HighlightTextDark;
        private static Color HighlightTextLight => SystemColors.HighlightText;
        private static Color HighlightTextDark => GenerateDarkModeColor(HighlightTextLight);
        public static Color Highlight => IsLightMode ? HighlightLight : HighlightDark;
        private static Color HighlightLight => SystemColors.Highlight;
        private static Color HighlightDark => GenerateDarkModeColor(HighlightLight);
        public static Color ControlText => IsLightMode ? ControlTextLight : ControlTextDark;
        private static Color ControlTextLight => SystemColors.ControlText;
        private static Color ControlTextDark => GenerateDarkModeColor(ControlTextLight);
        public static Color ControlDarkest => IsLightMode ? ControlDarkestLight : ControlDarkestDark;
        private static Color ControlDarkestLight => SystemColors.ControlDarkDark;
        private static Color ControlDarkestDark => GenerateDarkModeColor(ControlDarkestLight);
        public static Color ControlDarker => IsLightMode ? ControlDarkerLight : ControlDarkerDark;
        private static Color ControlDarkerLight => SystemColors.ControlDark;
        private static Color ControlDarkerDark => GenerateDarkModeColor(ControlDarkerLight);
        public static Color Control => IsLightMode ? ControlLight : ControlDark;
        private static Color ControlLight => SystemColors.Control;
        private static Color ControlDark => GenerateDarkModeColor(ControlLight);
        public static Color ControlLighter => IsLightMode ? ControlLighterLight : ControlLighterDark;
        private static Color ControlLighterLight => SystemColors.ControlLight;
        private static Color ControlLighterDark => GenerateDarkModeColor(ControlLight);
        public static Color ControlLightest => IsLightMode ? ControlLightestLight : ControlLightestDark;
        private static Color ControlLightestLight => SystemColors.ControlLightLight;
        private static Color ControlLightestDark => GenerateDarkModeColor(ControlLightestLight);
        public static Color ButtonFace => IsLightMode ? ButtonFaceLight : ButtonFaceDark;
        private static Color ButtonFaceLight => SystemColors.ButtonFace;
        private static Color ButtonFaceDark => GenerateDarkModeColor(ButtonFaceLight);
        public static Color ButtonShadow => IsLightMode ? ButtonShadowLight : ButtonShadowDark;
        private static Color ButtonShadowLight => SystemColors.ButtonShadow;
        private static Color ButtonShadowDark => GenerateDarkModeColor(ButtonShadowLight);
        public static Color AppWorkspace => IsLightMode ? AppWorkspaceLight : AppWorkspaceDark;
        private static Color AppWorkspaceLight => SystemColors.AppWorkspace;
        private static Color AppWorkspaceDark => GenerateDarkModeColor(AppWorkspaceLight);

        public static Color SplitterColor => IsLightMode ? SplitterColorLight : SplitterColorDark;
        private static Color SplitterColorLight => SystemColors.InactiveCaption;
        private static Color SplitterColorDark => GenerateDarkModeColor(SplitterColorLight);
        public static Color HasNotesColor => IsLightMode ? HasNotesColorLight : HasNotesColorDark;
        private static Color HasNotesColorLight => Color.Chocolate;
        private static Color HasNotesColorDark => Color.Chocolate;
        public static Color GrayHasNotesColor => IsLightMode ? GrayHasNotesColorLight : GrayHasNotesColorDark;
        private static Color GrayHasNotesColorLight => Color.Tan;
        private static Color GrayHasNotesColorDark => GenerateDarkModeColor(GrayHasNotesColorLight);
        public static Color ErrorColor => Color.Red;

        public static Color DieGlitchFore => IsLightMode ? DieGlitchForeLight : DieGlitchForeDark;
        private static Color DieGlitchForeLight => WindowTextDark;
        private static Color DieGlitchForeDark => GenerateDarkModeColor(DieGlitchForeLight);
        public static Color DieGlitchBackground => IsLightMode ? DieGlitchBackgroundLight : DieGlitchBackgroundDark;
        private static Color DieGlitchBackgroundLight => ControlDarkerLight;
        private static Color DieGlitchBackgroundDark => GenerateDarkModeColor(DieGlitchBackgroundLight);
        public static Color DieHitFore => IsLightMode ? DieHitForeLight : DieHitForeDark;
        private static Color DieHitForeLight => ControlTextLight;
        private static Color DieHitForeDark => GenerateDarkModeColor(DieHitForeLight);
        public static Color DieHitBackground => IsLightMode ? DieHitBackgroundLight : DieHitBackgroundDark;
        private static Color DieHitBackgroundLight => Color.LightGreen;
        private static Color DieHitBackgroundDark => GenerateDarkModeColor(DieHitBackgroundLight);
        public static Color DieGlitchHitFore => IsLightMode ? DieGlitchHitForeLight : DieGlitchHitForeDark;
        private static Color DieGlitchHitForeLight => ControlTextLight;
        private static Color DieGlitchHitForeDark => GenerateDarkModeColor(DieHitForeLight);
        public static Color DieGlitchHitBackground => IsLightMode ? DieGlitchHitBackgroundLight : DieGlitchHitBackgroundDark;
        private static Color DieGlitchHitBackgroundLight => Color.DarkGreen;
        private static Color DieGlitchHitBackgroundDark => GenerateDarkModeColor(DieHitBackgroundLight);

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
        /// <summary>
        /// Converts a color to the version it would have in Dark Mode.
        /// Lightness is inverted and then increased nonlinearly.
        /// Saturation is slightly decreased nonlinearly.
        /// </summary>
        /// <param name="objColor"></param>
        /// <returns></returns>
        private static Color GetDarkModeVersion(Color objColor)
        {
            float fltHue = objColor.GetHue() / 360.0f;
            float fltLightness = objColor.GetBrightness(); // It's called Brightness, but it's actually Lightness
            float fltNewLightness = 1.0f - fltLightness;
            float fltNewValue = fltNewLightness + objColor.GetSaturation() * Math.Min(fltNewLightness, 1 - fltNewLightness);
            float fltSaturationHsv = fltNewValue == 0 ? 0 : 2 * (1 - fltNewLightness / fltNewValue);
            // Lighten dark colors a little by increasing value so that we don't warp colors that are highly saturated to begin with.
            fltNewValue += 0.25f * fltNewValue * fltNewValue;
            fltNewValue = Math.Min(fltNewValue, 1.0f);
            Color objColorIntermediate = FromHsv(fltHue, fltSaturationHsv, fltNewValue);
            fltNewLightness = objColorIntermediate.GetBrightness();
            fltNewValue = fltNewLightness + objColorIntermediate.GetSaturation() * Math.Min(fltNewLightness, 1 - fltNewLightness);
            fltSaturationHsv = fltNewValue == 0 ? 0 : 2 * (1 - fltNewLightness / fltNewValue);
            // Desaturate high saturation colors a little
            float fltNewSaturationHsv = fltSaturationHsv - 0.1f * fltSaturationHsv * fltSaturationHsv;
            return FromHsva(fltHue, fltNewSaturationHsv, fltNewValue, objColor.A);
        }

        /// <summary>
        /// Inverse operation of GetDarkModeVersion(). If a color is fed through that function, and the result is then fed through this one, the final result should be the original color.
        /// Note that because GetDarkModeVersion() always does some amount of desaturation and lightening, not all colors are valid results of GetDarkModeVersion().
        /// This function should therefore *not* be used as a kind of GetLightModeVersion() of a dark mode color directly.
        /// </summary>
        /// <param name="objColor"></param>
        /// <returns></returns>
        private static Color InverseGetDarkModeVersion(Color objColor)
        {
            float fltHue = objColor.GetHue() / 360.0f;
            float fltLightness = objColor.GetBrightness(); // It's called Brightness, but it's actually Lightness
            float fltValue = fltLightness + objColor.GetSaturation() * Math.Min(fltLightness, 1 - fltLightness);
            float fltSaturationHsv = fltValue == 0 ? 0 : 2 * (1 - fltLightness / fltValue);
            float fltNewSaturationHsv = 0;
            if (fltSaturationHsv != 0)
            {
                // x - 0.1x^2 = n is the regular transform where x is the Light Mode saturation and n is the Dark Mode saturation
                // To get it back, we need to solve for x knowing only n:
                // x^2 - 10x + 10n = 0
                // x = (10 +/- sqrt(100 - 40n))/2 = 5 +/- sqrt(25 - 10n)
                // Because saturation cannot be greater than 1, positive result is unreal, therefore: x = 5 - sqrt(25 - 10n)
                fltNewSaturationHsv = Math.Min((float) (5.0 - Math.Sqrt(25.0 - 10.0 * fltSaturationHsv)), 1.0f);
                Color objColorIntermediate = FromHsv(fltHue, fltNewSaturationHsv, fltValue);
                fltLightness = objColorIntermediate.GetBrightness(); // It's called Brightness, but it's actually Lightness
                fltValue = fltLightness + objColorIntermediate.GetSaturation() * Math.Min(fltLightness, 1 - fltLightness);
            }
            // y + 0.25y^2 = m is the regular transform where y is the Dark Mode Value pre-adjustment and m is the Dark Mode Value post-adjustment
            // To get it back, we need to solve for y knowing only m:
            // y^2 + 4y - 4m = 0
            // y = (-4 +/- sqrt(16 + 16m))/2 = -2 +/- sqrt(4 + 4m) = -2 +/- 2*sqrt(1 + m)
            // Because value cannot be greater than 1, negative result is unreal, therefore: y = -2 + 2*sqrt(1 + m)
            float fltNewValue = Math.Min((float) (2 * Math.Sqrt(1 + fltValue) - 2), 1.0f);
            // Now convert to Lightness so we can flip it
            float fltNewLightness = fltNewValue * (1 - fltNewSaturationHsv / 2.0f);
            float fltNewSaturationHsl = fltNewLightness == 0
                ? 0
                : (fltNewValue - fltNewLightness) / Math.Min(fltNewLightness, 1 - fltNewLightness);
            fltNewLightness = 1 - fltNewLightness;
            return FromHsla(fltHue, fltNewSaturationHsl, fltNewLightness, objColor.A);
        }

        private static Color GetBrightenedVersion(Color objColor)
        {
            // Built-in functions are in HSV/HSB, so we need to convert to HSL to invert lightness.
            float fltHue = objColor.GetHue() / 360.0f;
            float fltBrightness = objColor.GetBrightness();
            float fltSaturation = objColor.GetSaturation();
            fltSaturation = Math.Min(fltSaturation * 1.15f, 1);
            return FromHsva(fltHue, fltBrightness, fltSaturation, objColor.A);
        }

        private static Color GetDimmedVersion(Color objColor)
        {
            // Built-in functions are in HSV/HSB, so we need to convert to HSL to invert lightness.
            float fltHue = objColor.GetHue() / 360.0f;
            float fltBrightness = objColor.GetBrightness();
            float fltSaturation = objColor.GetSaturation();
            fltSaturation = Math.Max(0, fltSaturation * 0.85f);
            return FromHsva(fltHue, fltBrightness, fltSaturation, objColor.A);
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
                    objDataGridView.BackgroundColor = blnLightMode ? AppWorkspaceLight : AppWorkspaceDark;
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
                case ListView objListView:
                    objControl.ForeColor = blnLightMode ? WindowTextLight : WindowTextDark;
                    objControl.BackColor = blnLightMode ? WindowLight : WindowDark;
                    foreach (DiceRollerListViewItem objItem in objListView.Items)
                    {
                        if (objItem.IsHit)
                        {
                            if (objItem.IsGlitch)
                            {
                                objItem.ForeColor = blnLightMode ? DieGlitchHitForeLight : DieGlitchHitForeDark;
                                objItem.BackColor = blnLightMode ? DieGlitchHitBackgroundLight : DieGlitchHitBackgroundDark;
                            }
                            else
                            {
                                objItem.ForeColor = blnLightMode ? DieHitForeLight : DieHitForeDark;
                                objItem.BackColor = blnLightMode ? DieHitBackgroundLight : DieHitBackgroundDark;
                            }
                        }
                        else if (objItem.IsGlitch)
                        {
                            objItem.ForeColor = blnLightMode ? DieGlitchForeLight : DieGlitchForeDark;
                            objItem.BackColor = blnLightMode ? DieGlitchBackgroundLight : DieGlitchBackgroundDark;
                        }
                        else
                        {
                            objItem.ForeColor = blnLightMode ? WindowTextLight : WindowTextDark;
                            objItem.BackColor = blnLightMode ? WindowLight : WindowDark;
                        }
                    }
                    break;
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
                            if (blnLightMode) // Disabled case for Light mode already handled by the switch above
                                chkControlColored.ForeColor = ControlTextLight;
                            else
                                chkControlColored.ForeColor = chkControlColored.Enabled ? ControlTextDark : GrayText;
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
        public static Color FromHsl(float fltHue, float fltSaturation, float fltLightness)
        {
            return FromHsla(fltHue, fltSaturation, fltLightness, byte.MaxValue);
        }

        /// <summary>
        /// Returns an RGBA Color from HSLA values.
        /// </summary>
        /// <param name="fltHue">Hue value, between 0.0 and 1.0.</param>
        /// <param name="fltSaturation">Saturation value, between 0.0 and 1.0.</param>
        /// <param name="fltLightness">Lightness value, between 0.0 and 1.0.</param>
        /// <param name="chrAlpha">Alpha value.</param>
        /// <returns>A Color with RGBA values corresponding to the HSLA inputs.</returns>
        public static Color FromHsla(float fltHue, float fltSaturation, float fltLightness, byte chrAlpha)
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
        public static Color FromHsv(float fltHue, float fltSaturation, float fltValue)
        {
            return FromHsva(fltHue, fltSaturation, fltValue, byte.MaxValue);
        }

        /// <summary>
        /// Returns an RGBA Color from HSVA (or HSBA) values.
        /// </summary>
        /// <param name="fltHue">Hue value, between 0.0 and 1.0.</param>
        /// <param name="fltSaturation">Saturation value, between 0.0 and 1.0.</param>
        /// <param name="fltValue">Value/Brightness value, between 0.0 and 1.0.</param>
        /// <param name="chrAlpha">Alpha value.</param>
        /// <returns>A Color with RGBA values corresponding to the HSVA inputs.</returns>
        public static Color FromHsva(float fltHue, float fltSaturation, float fltValue, byte chrAlpha)
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
