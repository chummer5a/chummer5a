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
using System.Threading;
using System.Threading.Tasks;
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
using Timer = System.Timers.Timer;
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
                //swallow this
            }
            catch (System.Security.SecurityException)
            {
                //swallow this
            }

            s_TmrDarkModeCheckerTimer = new Timer { Interval = 5000 }; // Poll registry every 5 seconds
            s_TmrDarkModeCheckerTimer.Elapsed += TmrDarkModeCheckerTimerOnTick;
            switch (GlobalSettings.ColorModeSetting)
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

        private static async void TmrDarkModeCheckerTimerOnTick(object sender, EventArgs e)
        {
            await AutoApplyLightDarkModeAsync(CancellationToken.None).ConfigureAwait(false);
        }

        public static void AutoApplyLightDarkMode()
        {
            if (GlobalSettings.ColorModeSetting == ColorMode.Automatic)
            {
                IsLightMode = !DoesRegistrySayDarkMode();
                s_TmrDarkModeCheckerTimer.Enabled = true;
            }
        }

        public static async Task AutoApplyLightDarkModeAsync(CancellationToken token = default)
        {
            if (GlobalSettings.ColorModeSetting == ColorMode.Automatic)
            {
                await SetIsLightModeAsync(!DoesRegistrySayDarkMode(), token).ConfigureAwait(false);
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
            private set
            {
                if (_blnIsLightMode == value)
                    return;
                _blnIsLightMode = value;
                if (Program.MainForm == null)
                    return;
                using (CursorWait.New(Program.MainForm))
                    Program.MainForm.UpdateLightDarkMode(CancellationToken.None);
            }
        }

        public static Task SetIsLightModeAsync(bool blnNewValue, CancellationToken token = default)
        {
            if (_blnIsLightMode == blnNewValue)
                return Task.CompletedTask;
            _blnIsLightMode = blnNewValue;
            return Program.MainForm == null ? Task.CompletedTask : Inner();
            async Task Inner()
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(Program.MainForm, token: token).ConfigureAwait(false);
                try
                {
                    await Program.MainForm.UpdateLightDarkModeAsync(token).ConfigureAwait(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
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
            return s_DicDarkModeColors.GetOrAdd(objColor, GetDarkModeVersion);
        }

        /// <summary>
        /// Returns an inverted version of a color that has gone through GenerateDarkModeColor()
        /// </summary>
        /// <param name="objColor">Color whose Dark Mode conversions for lightness and saturation should be inverted.</param>
        /// <returns>New Color object identical to <paramref name="objColor"/>, but with its Dark Mode conversion inverted.</returns>
        public static Color GenerateInverseDarkModeColor(Color objColor)
        {
            return s_DicInverseDarkModeColors.GetOrAdd(objColor, InverseGetDarkModeVersion);
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
            return IsLightMode
                ? s_DicDimmedColors.GetOrAdd(objColor, GetDimmedVersion)
                : s_DicBrightenedColors.GetOrAdd(objColor, GetBrightenedVersion);
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
        private static Color WindowTextLight { get; } = SystemColors.WindowText;
        private static Color WindowTextDark { get; } = GenerateDarkModeColor(WindowTextLight);

        public static Color Window => IsLightMode ? WindowLight : WindowDark;
        private static Color WindowLight { get; } = SystemColors.Window;
        private static Color WindowDark { get; } = GenerateDarkModeColor(WindowLight);

        public static Color InfoText => IsLightMode ? InfoTextLight : InfoTextDark;
        private static Color InfoTextLight { get; } = SystemColors.InfoText;
        private static Color InfoTextDark { get; } = GenerateDarkModeColor(InfoTextLight);

        public static Color Info => IsLightMode ? InfoLight : InfoDark;
        private static Color InfoLight { get; } = SystemColors.Info;
        private static Color InfoDark { get; } = GenerateDarkModeColor(InfoLight);

        public static Color GrayText => IsLightMode ? GrayTextLight : GrayTextDark;
        private static Color GrayTextLight { get; } = SystemColors.GrayText;
        private static Color GrayTextDark { get; } = GenerateDarkModeColor(GrayTextLight);

        public static Color HighlightText => IsLightMode ? HighlightTextLight : HighlightTextDark;
        private static Color HighlightTextLight { get; } = SystemColors.HighlightText;
        private static Color HighlightTextDark { get; } = GenerateDarkModeColor(HighlightTextLight);

        public static Color Highlight => IsLightMode ? HighlightLight : HighlightDark;
        private static Color HighlightLight { get; } = SystemColors.Highlight;
        private static Color HighlightDark { get; } = GenerateDarkModeColor(HighlightLight);

        public static Color ControlText => IsLightMode ? ControlTextLight : ControlTextDark;
        public static Color ControlTextLight { get; } = SystemColors.ControlText;
        private static Color ControlTextDark { get; } = GenerateDarkModeColor(ControlTextLight);

        public static Color ControlDarkest => IsLightMode ? ControlDarkestLight : ControlDarkestDark;
        public static Color ControlDarkestLight { get; } = SystemColors.ControlDarkDark;
        private static Color ControlDarkestDark { get; } = GenerateDarkModeColor(ControlDarkestLight);

        public static Color ControlDarker => IsLightMode ? ControlDarkerLight : ControlDarkerDark;
        public static Color ControlDarkerLight { get; } = SystemColors.ControlDark;
        private static Color ControlDarkerDark { get; } = GenerateDarkModeColor(ControlDarkerLight);

        public static Color Control => IsLightMode ? ControlLight : ControlDark;
        public static Color ControlLight { get; } = SystemColors.Control;
        private static Color ControlDark { get; } = GenerateDarkModeColor(ControlLight);

        public static Color ControlLighter => IsLightMode ? ControlLighterLight : ControlLighterDark;
        private static Color ControlLighterLight { get; } = SystemColors.ControlLight;
        private static Color ControlLighterDark { get; } = GenerateDarkModeColor(ControlLight);

        public static Color ControlLightest => IsLightMode ? ControlLightestLight : ControlLightestDark;
        private static Color ControlLightestLight { get; } = SystemColors.ControlLightLight;
        private static Color ControlLightestDark { get; } = GenerateDarkModeColor(ControlLightestLight);

        public static Color ButtonFace => IsLightMode ? ButtonFaceLight : ButtonFaceDark;
        private static Color ButtonFaceLight { get; } = SystemColors.ButtonFace;
        private static Color ButtonFaceDark { get; } = GenerateDarkModeColor(ButtonFaceLight);

        public static Color ButtonShadow => IsLightMode ? ButtonShadowLight : ButtonShadowDark;
        private static Color ButtonShadowLight { get; } = SystemColors.ButtonShadow;
        private static Color ButtonShadowDark { get; } = GenerateDarkModeColor(ButtonShadowLight);

        public static Color AppWorkspace => IsLightMode ? AppWorkspaceLight : AppWorkspaceDark;
        private static Color AppWorkspaceLight { get; } = SystemColors.AppWorkspace;
        private static Color AppWorkspaceDark { get; } = GenerateDarkModeColor(AppWorkspaceLight);

        public static Color SplitterColor => IsLightMode ? SplitterColorLight : SplitterColorDark;
        private static Color SplitterColorLight { get; } = SystemColors.InactiveCaption;
        private static Color SplitterColorDark { get; } = GenerateDarkModeColor(SplitterColorLight);

        public static Color HasNotesColor => IsLightMode ? HasNotesColorLight : HasNotesColorDark;
        private static Color HasNotesColorLight { get; } = Color.Chocolate;
        private static Color HasNotesColorDark { get; } = GenerateDarkModeColor(HasNotesColorLight);

        public static Color GrayHasNotesColor => IsLightMode ? GrayHasNotesColorLight : GrayHasNotesColorDark;
        private static Color GrayHasNotesColorLight { get; } = Color.Tan;
        private static Color GrayHasNotesColorDark { get; } = GenerateDarkModeColor(GrayHasNotesColorLight);

        public static Color ErrorColor { get; } = Color.Red;

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
        private static Color DieHitBackgroundLight { get; } = Color.LightGreen;
        private static Color DieHitBackgroundDark => GenerateDarkModeColor(DieHitBackgroundLight);
        public static Color DieGlitchHitFore => IsLightMode ? DieGlitchHitForeLight : DieGlitchHitForeDark;
        private static Color DieGlitchHitForeLight => ControlTextLight;
        private static Color DieGlitchHitForeDark => GenerateDarkModeColor(DieHitForeLight);
        public static Color DieGlitchHitBackground => IsLightMode ? DieGlitchHitBackgroundLight : DieGlitchHitBackgroundDark;
        private static Color DieGlitchHitBackgroundLight { get; } = Color.DarkGreen;
        private static Color DieGlitchHitBackgroundDark => GenerateDarkModeColor(DieHitBackgroundLight);

        public static void UpdateLightDarkMode(this Control objControl, CancellationToken token = default)
        {
            ApplyColorsRecursively(objControl, IsLightMode, token);
        }

        public static void UpdateLightDarkMode(this Control objControl, bool blnLightMode, CancellationToken token = default)
        {
            ApplyColorsRecursively(objControl, blnLightMode, token);
        }

        public static void UpdateLightDarkMode(this ToolStripItem tssItem, CancellationToken token = default)
        {
            ApplyColorsRecursively(tssItem, IsLightMode, token);
        }

        public static void UpdateLightDarkMode(this ToolStripItem tssItem, bool blnLightMode, CancellationToken token = default)
        {
            ApplyColorsRecursively(tssItem, blnLightMode, token);
        }

        public static Task UpdateLightDarkModeAsync(this Control objControl, CancellationToken token = default)
        {
            return ApplyColorsRecursivelyAsync(objControl, IsLightMode, token);
        }

        public static Task UpdateLightDarkModeAsync(this Control objControl, bool blnLightMode, CancellationToken token = default)
        {
            return ApplyColorsRecursivelyAsync(objControl, blnLightMode, token);
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
            fltNewValue += 0.14f * (1.0f - Convert.ToSingle(Math.Sqrt(fltNewValue)));
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
                fltNewSaturationHsv = Math.Min((float)(5.0 - Math.Sqrt(25.0 - 10.0 * fltSaturationHsv)), 1.0f);
                Color objColorIntermediate = FromHsv(fltHue, fltNewSaturationHsv, fltValue);
                fltLightness = objColorIntermediate.GetBrightness(); // It's called Brightness, but it's actually Lightness
                fltValue = fltLightness + objColorIntermediate.GetSaturation() * Math.Min(fltLightness, 1 - fltLightness);
            }
            // y + 0.14(1 - sqrt(y)) = m is the regular transform where y is the Dark Mode Value pre-adjustment and m is the Dark Mode Value post-adjustment
            // To get it back, we need to solve for y knowing only m:
            // 1 - sqrt(y) + 50/7*y - 50/7*m = 0
            // 1 + 50/7*y - 50/7*m = sqrt(y)
            // 1 + 2500/49*y^2 + 2500/49*m^2 + 100/7*y - 100/7*m - 5000/49*my = y
            // 2500/49*y^2 + (93/7 - 5000/49*m)y + 2500/49*m^2 - 100/7*m + 1 = 0
            // 2500/7*y^2 + (93 - 5000m/7)y + 2500/7*m^2 - 100m + 7 = 0
            // y = (5000m/7 - 93 +/- sqrt((93 - 5000m/7)^2 - 10000/7*(2500/7*m^2 - 100m + 7)))/5000*7
            // y = (5000m - 651 +/- 7*sqrt(25000000m^2/49 - 930000m/7 + 8649 - 25000000m^2/49 + 1000000m/7 - 10000))/5000
            // y = (5000m - 651 +/- 7*sqrt(10000m - 1351))/5000
            // y = m - 0.1302 +/- 0.14*sqrt(m - 0.1351)
            // Because expression for y must be the same across all m, only positive result is valid, therefore: y = m - 0.1302 + 0.14*sqrt(m - 0.1351)
            float fltNewValue = Math.Min((float)(fltValue - 0.1302 + 0.14 * Math.Sqrt(fltValue - 0.1351)), 1.0f);
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

        private static void ApplyColorsRecursively(Control objControl, bool blnLightMode, CancellationToken token = default)
        {
            void ApplyButtonStyle()
            {
                // Buttons look weird if colored based on anything other than the default color scheme in dark mode
                objControl.DoThreadSafe((x, y) => x.ForeColor = SystemColors.ControlText, token);
            }
            switch (objControl)
            {
                case DataGridView objDataGridView:
                    objDataGridView.DoThreadSafe((x, y) =>
                    {
                        if (blnLightMode)
                        {
                            x.BackgroundColor = AppWorkspaceLight;
                            x.GridColor = ControlTextLight;
                            x.DefaultCellStyle.ForeColor = ControlTextLight;
                            x.DefaultCellStyle.BackColor = ControlLight;
                            x.ColumnHeadersDefaultCellStyle.ForeColor = ControlTextLight;
                            x.ColumnHeadersDefaultCellStyle.BackColor = ControlLight;
                            x.AlternatingRowsDefaultCellStyle.ForeColor = ControlTextLight;
                            x.AlternatingRowsDefaultCellStyle.BackColor = ControlLighterLight;
                            x.RowTemplate.DefaultCellStyle.ForeColor = ControlTextLight;
                            x.RowTemplate.DefaultCellStyle.BackColor = ControlLight;
                            foreach (DataGridViewTextBoxColumn objColumn in x.Columns)
                            {
                                y.ThrowIfCancellationRequested();
                                objColumn.DefaultCellStyle.ForeColor = ControlTextLight;
                                objColumn.DefaultCellStyle.BackColor = ControlLight;
                            }
                        }
                        else
                        {
                            x.BackgroundColor = AppWorkspaceDark;
                            x.GridColor = ControlTextDark;
                            x.DefaultCellStyle.ForeColor = ControlTextDark;
                            x.DefaultCellStyle.BackColor = ControlDark;
                            x.ColumnHeadersDefaultCellStyle.ForeColor = ControlTextDark;
                            x.ColumnHeadersDefaultCellStyle.BackColor = ControlDark;
                            x.AlternatingRowsDefaultCellStyle.ForeColor = ControlTextDark;
                            x.AlternatingRowsDefaultCellStyle.BackColor = ControlLighterDark;
                            x.RowTemplate.DefaultCellStyle.ForeColor = ControlTextDark;
                            x.RowTemplate.DefaultCellStyle.BackColor = ControlDark;
                            foreach (DataGridViewTextBoxColumn objColumn in x.Columns)
                            {
                                y.ThrowIfCancellationRequested();
                                objColumn.DefaultCellStyle.ForeColor = ControlTextDark;
                                objColumn.DefaultCellStyle.BackColor = ControlDark;
                            }
                        }
                    }, token);
                    break;

                case SplitContainer objSplitControl:
                    objSplitControl.DoThreadSafe((x, y) =>
                    {
                        if (blnLightMode)
                        {
                            x.ForeColor = SplitterColorLight;
                            x.BackColor = SplitterColorLight;
                        }
                        else
                        {
                            x.ForeColor = SplitterColorDark;
                            x.BackColor = SplitterColorDark;
                        }
                    }, token);
                    ApplyColorsRecursively(objSplitControl.DoThreadSafeFunc((x, y) => x.Panel1, token), blnLightMode, token);
                    ApplyColorsRecursively(objSplitControl.DoThreadSafeFunc((x, y) => x.Panel2, token), blnLightMode, token);
                    break;

                case TreeView treControl:
                    treControl.DoThreadSafe((x, y) =>
                    {
                        if (blnLightMode)
                        {
                            x.ForeColor = WindowTextLight;
                            x.BackColor = WindowLight;
                            x.LineColor = WindowTextLight;
                        }
                        else
                        {
                            x.ForeColor = WindowTextDark;
                            x.BackColor = WindowDark;
                            x.LineColor = WindowTextDark;
                        }
                    }, token);
                    foreach (TreeNode objNode in treControl.DoThreadSafeFunc(x => x.Nodes, token))
                        ApplyColorsRecursively(objNode, blnLightMode, token);
                    break;

                case TextBox txtControl:
                    txtControl.DoThreadSafe((x, y) =>
                    {
                        if (x.ForeColor != ErrorColor)
                        {
                            if (blnLightMode)
                            {
                                x.ForeColor = WindowTextLight;
                                if (x.ReadOnly && (x.BackColor == ControlLight || x.BackColor == ControlDark))
                                    x.BackColor = ControlLight;
                                else
                                    x.BackColor = WindowLight;
                            }
                            else
                            {
                                x.ForeColor = WindowTextDark;
                                if (x.ReadOnly && (x.BackColor == ControlLight || x.BackColor == ControlDark))
                                    x.BackColor = ControlDark;
                                else
                                    x.BackColor = WindowDark;
                            }
                        }
                        else if (blnLightMode)
                        {
                            if (x.ReadOnly && (x.BackColor == ControlLight || x.BackColor == ControlDark))
                                x.BackColor = ControlLight;
                            else
                                x.BackColor = WindowLight;
                        }
                        else if (x.ReadOnly && (x.BackColor == ControlLight || x.BackColor == ControlDark))
                            x.BackColor = ControlDark;
                        else
                            x.BackColor = WindowDark;
                    }, token);
                    break;

                case ListView objListView:
                    objListView.DoThreadSafe((x, y) =>
                    {
                        if (blnLightMode)
                        {
                            x.ForeColor = WindowTextLight;
                            x.BackColor = WindowLight;
                            foreach (DiceRollerListViewItem objItem in x.Items)
                            {
                                if (objItem.IsHit)
                                {
                                    if (objItem.IsGlitch)
                                    {
                                        objItem.ForeColor = DieGlitchHitForeLight;
                                        objItem.BackColor = DieGlitchHitBackgroundLight;
                                    }
                                    else
                                    {
                                        objItem.ForeColor = DieHitForeLight;
                                        objItem.BackColor = DieHitBackgroundLight;
                                    }
                                }
                                else if (objItem.IsGlitch)
                                {
                                    objItem.ForeColor = DieGlitchForeLight;
                                    objItem.BackColor = DieGlitchBackgroundLight;
                                }
                                else
                                {
                                    objItem.ForeColor = WindowTextLight;
                                    objItem.BackColor = WindowLight;
                                }
                            }
                        }
                        else
                        {
                            x.ForeColor = WindowTextDark;
                            x.BackColor = WindowDark;
                            foreach (DiceRollerListViewItem objItem in x.Items)
                            {
                                if (objItem.IsHit)
                                {
                                    if (objItem.IsGlitch)
                                    {
                                        objItem.ForeColor = DieGlitchHitForeDark;
                                        objItem.BackColor = DieGlitchHitBackgroundDark;
                                    }
                                    else
                                    {
                                        objItem.ForeColor = DieHitForeDark;
                                        objItem.BackColor = DieHitBackgroundDark;
                                    }
                                }
                                else if (objItem.IsGlitch)
                                {
                                    objItem.ForeColor = DieGlitchForeDark;
                                    objItem.BackColor = DieGlitchBackgroundDark;
                                }
                                else
                                {
                                    objItem.ForeColor = WindowTextDark;
                                    objItem.BackColor = WindowDark;
                                }
                            }
                        }
                    }, token);
                    break;

                case ListBox _:
                case ComboBox _:
                case TableCell _:
                    objControl.DoThreadSafe((x, y) =>
                    {
                        if (blnLightMode)
                        {
                            x.ForeColor = WindowTextLight;
                            x.BackColor = WindowLight;
                        }
                        else
                        {
                            x.ForeColor = WindowTextDark;
                            x.BackColor = WindowDark;
                        }
                    }, token);
                    break;

                case GroupBox _:
                    objControl.DoThreadSafe((x, y) =>
                    {
                        if (blnLightMode)
                        {
                            x.ForeColor = ControlTextLight;
                            x.BackColor = ControlLight;
                        }
                        else
                        {
                            x.ForeColor = ControlTextDark;
                            x.BackColor = ControlDark;
                        }
                    }, token);
                    break;

                case ContactControl _:
                case PetControl _:
                case SkillControl _:
                case KnowledgeSkillControl _:
                    // These controls have colors that are always data-bound
                    break;

                case RichTextBox _:
                    // Rtf TextBox is special because we don't want any color changes, otherwise it will mess up the saved Rtf text
                    return;

                case CheckBox chkControl:
                    if (chkControl.DoThreadSafeFunc((x, y) => x.Appearance == Appearance.Button, token) || chkControl is DpiFriendlyCheckBoxDisguisedAsButton)
                    {
                        ApplyButtonStyle();
                        break;
                    }

                    if (chkControl is ColorableCheckBox chkControlColored)
                    {
                        chkControlColored.DoThreadSafe((x, y) =>
                        {
                            x.DefaultColorScheme = blnLightMode;
                            if (blnLightMode) // Disabled case for Light mode already handled by the switch above
                                x.ForeColor = ControlTextLight;
                            else
                                x.ForeColor = x.Enabled ? ControlTextDark : GrayText;
                        }, token);
                        break;
                    }
                    goto default;

                case Button cmdControl:
                    if (cmdControl.DoThreadSafeFunc((x, y) => x.FlatStyle, token) == FlatStyle.Flat)
                        goto default;
                    ApplyButtonStyle();
                    break;

                case HeaderCell _:
                    // Header cells should use inverted colors
                    objControl.DoThreadSafe((x, y) =>
                    {
                        if (blnLightMode)
                        {
                            x.ForeColor = ControlLightestLight;
                            x.BackColor = ControlTextLight;
                        }
                        else
                        {
                            x.ForeColor = ControlLightestDark;
                            x.BackColor = ControlTextDark;
                        }
                    }, token);
                    return;

                case TableLayoutPanel tlpControl:
                    tlpControl.DoThreadSafe((x, y) =>
                    {
                        if (x.BorderStyle != BorderStyle.None)
                            x.BorderStyle = blnLightMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                    }, token);
                    goto default;
                case Form frmControl:
                    MenuStrip objMainMenuStrip = frmControl.DoThreadSafeFunc((x, y) => x.MainMenuStrip, token);
                    if (objMainMenuStrip != null)
                    {
                        foreach (ToolStripMenuItem tssItem in objMainMenuStrip.DoThreadSafeFunc((x, y) => x.Items, token))
                            ApplyColorsRecursively(tssItem, blnLightMode, token);
                    }
                    goto default;
                case TabControl objTabControl:
                    foreach (TabPage tabPage in objTabControl.DoThreadSafeFunc((x, y) => x.TabPages, token))
                        ApplyColorsRecursively(tabPage, blnLightMode, token);
                    goto default;
                case ToolStrip tssStrip:
                    foreach (ToolStripItem tssItem in tssStrip.DoThreadSafeFunc((x, y) => x.Items, token))
                        ApplyColorsRecursively(tssItem, blnLightMode, token);
                    goto default;
                default:
                    objControl.DoThreadSafe((x, y) =>
                    {
                        if (blnLightMode)
                        {
                            if (x.ForeColor == ControlDark)
                                x.ForeColor = ControlLight;
                            else if (x.ForeColor == ControlDarkerDark)
                                x.ForeColor = ControlDarkerLight;
                            else if (x.ForeColor == ControlDarkestDark)
                                x.ForeColor = ControlDarkestLight;
                            else if (x.ForeColor == WindowTextDark)
                                x.ForeColor = WindowTextLight;
                            else if (x.ForeColor == ControlTextLight || x.ForeColor == ControlTextDark)
                                x.ForeColor = ControlTextLight;
                        }
                        else if (x.ForeColor == ControlLight)
                            x.ForeColor = ControlDark;
                        else if (x.ForeColor == ControlDarkerLight)
                            x.ForeColor = ControlDarkerDark;
                        else if (x.ForeColor == ControlDarkestLight)
                            x.ForeColor = ControlDarkestDark;
                        else if (x.ForeColor == WindowTextLight)
                            x.ForeColor = WindowTextDark;
                        else if (x.ForeColor == ControlTextLight || x.ForeColor == ControlTextDark)
                            x.ForeColor = ControlTextDark;
                    }, token);
                    // These controls never have backgrounds set explicitly, so shouldn't have their backgrounds overwritten
                    if (!(objControl is Label || objControl is CheckBox || objControl is PictureBox
                          || objControl is Button || (objControl is Panel && !(objControl is SplitterPanel
                                                                               || objControl
                                                                                   .DoThreadSafeFunc(
                                                                                       (x, y) => x.BackColor, token).A
                                                                               == byte.MaxValue))))
                    {
                        objControl.DoThreadSafe((x, y) =>
                        {
                            if (blnLightMode)
                            {
                                if (x.BackColor == ControlLighterDark)
                                    x.BackColor = ControlLighterLight;
                                else if (x.BackColor == ControlLightestDark)
                                    x.BackColor = ControlLightestLight;
                                else if (x.BackColor == WindowDark)
                                    x.BackColor = WindowLight;
                                else if (x.BackColor == ControlLight || x.BackColor == ControlDark)
                                    x.BackColor = ControlLight;
                            }
                            else if (x.BackColor == ControlLighterLight)
                                x.BackColor = ControlLighterDark;
                            else if (x.BackColor == ControlLightestLight)
                                x.BackColor = ControlLightestDark;
                            else if (x.BackColor == WindowLight)
                                x.BackColor = WindowDark;
                            else if (x.BackColor == ControlLight || x.BackColor == ControlDark)
                                x.BackColor = ControlDark;
                        }, token);
                    }

                    break;
            }

            foreach (Control objChild in objControl.DoThreadSafeFunc((x, y) => x.Controls, token))
                ApplyColorsRecursively(objChild, blnLightMode, token);
        }

        private static void ApplyColorsRecursively(ToolStripItem tssItem, bool blnLightMode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            ToolStrip objParent = tssItem.GetCurrentParent();
            if (objParent != null)
                objParent.DoThreadSafe(DoColor, token: token);
            else
                DoColor();

            void DoColor()
            {
                if (blnLightMode)
                {
                    if (tssItem.ForeColor == ControlDark)
                        tssItem.ForeColor = ControlLight;
                    else if (tssItem.ForeColor == ControlDarkerDark)
                        tssItem.ForeColor = ControlDarkerLight;
                    else if (tssItem.ForeColor == ControlDarkestDark)
                        tssItem.ForeColor = ControlDarkestLight;
                    else
                        tssItem.ForeColor = ControlTextLight;
                    token.ThrowIfCancellationRequested();
                    if (tssItem.BackColor == ControlLighterDark)
                        tssItem.BackColor = ControlLighterLight;
                    else if (tssItem.BackColor == ControlLightestDark)
                        tssItem.BackColor = ControlLightestLight;
                    else
                        tssItem.BackColor = ControlLight;
                }
                else
                {
                    if (tssItem.ForeColor == ControlLight)
                        tssItem.ForeColor = ControlDark;
                    else if (tssItem.ForeColor == ControlDarkerLight)
                        tssItem.ForeColor = ControlDarkerDark;
                    else if (tssItem.ForeColor == ControlDarkestLight)
                        tssItem.ForeColor = ControlDarkestDark;
                    else
                        tssItem.ForeColor = ControlTextDark;
                    token.ThrowIfCancellationRequested();
                    if (tssItem.BackColor == ControlLighterLight)
                        tssItem.BackColor = ControlLighterDark;
                    else if (tssItem.BackColor == ControlLightestLight)
                        tssItem.BackColor = ControlLightestDark;
                    else
                        tssItem.BackColor = ControlDark;
                }
            }

            token.ThrowIfCancellationRequested();
            switch (tssItem)
            {
                case ToolStripDropDownItem tssDropDownItem:
                    foreach (ToolStripItem tssDropDownChild in tssDropDownItem.DropDownItems)
                        ApplyColorsRecursively(tssDropDownChild, blnLightMode, token);
                    break;
                case ColorableToolStripSeparator tssSeparator when objParent != null:
                    objParent.DoThreadSafe(() => tssSeparator.DefaultColorScheme = blnLightMode, token: token);
                    break;
                case ColorableToolStripSeparator tssSeparator:
                    tssSeparator.DefaultColorScheme = blnLightMode;
                    break;
            }
        }

        private static void ApplyColorsRecursively(TreeNode nodNode, bool blnLightMode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            TreeView treView = nodNode.TreeView;
            if (treView != null)
                treView.DoThreadSafe(DoColor, token: token);
            else
                DoColor();

            void DoColor()
            {
                if (blnLightMode)
                {
                    if (nodNode.ForeColor == HasNotesColorDark)
                        nodNode.ForeColor = HasNotesColorLight;
                    else if (nodNode.ForeColor == GrayHasNotesColorDark)
                        nodNode.ForeColor = GrayHasNotesColorLight;
                    else if (nodNode.ForeColor == WindowTextDark)
                        nodNode.ForeColor = WindowTextLight;
                    nodNode.BackColor = WindowLight;
                }
                else
                {
                    if (nodNode.ForeColor == HasNotesColorLight)
                        nodNode.ForeColor = HasNotesColorDark;
                    else if (nodNode.ForeColor == GrayHasNotesColorLight)
                        nodNode.ForeColor = GrayHasNotesColorDark;
                    else if (nodNode.ForeColor == WindowTextLight)
                        nodNode.ForeColor = WindowTextDark;
                    nodNode.BackColor = WindowDark;
                }
            }
            token.ThrowIfCancellationRequested();
            foreach (TreeNode nodNodeChild in nodNode.Nodes)
                ApplyColorsRecursively(nodNodeChild, blnLightMode, token);
        }

        private static async Task ApplyColorsRecursivelyAsync(Control objControl, bool blnLightMode, CancellationToken token = default)
        {
            Task ApplyButtonStyle()
            {
                // Buttons look weird if colored based on anything other than the default color scheme in dark mode
                return objControl.DoThreadSafeAsync(x => x.ForeColor = SystemColors.ControlText, token);
            }
            switch (objControl)
            {
                case DataGridView objDataGridView:
                    {
                        Color objBackgroundColor;
                        Color objForeColor;
                        Color objBackColor;
                        Color objAlternateBackColor;
                        if (blnLightMode)
                        {
                            objBackgroundColor = AppWorkspaceLight;
                            objForeColor = ControlTextLight;
                            objBackColor = ControlLight;
                            objAlternateBackColor = ControlLighterLight;
                        }
                        else
                        {
                            objBackgroundColor = AppWorkspaceDark;
                            objForeColor = ControlTextDark;
                            objBackColor = ControlDark;
                            objAlternateBackColor = ControlLighterDark;
                        }
                        await objDataGridView.DoThreadSafeAsync(x =>
                        {
                            x.BackgroundColor = objBackgroundColor;
                            x.GridColor = objForeColor;
                            x.DefaultCellStyle.ForeColor = objForeColor;
                            x.DefaultCellStyle.BackColor = objBackColor;
                            x.ColumnHeadersDefaultCellStyle.ForeColor = objForeColor;
                            x.ColumnHeadersDefaultCellStyle.BackColor = objBackColor;
                            x.AlternatingRowsDefaultCellStyle.ForeColor = objForeColor;
                            x.AlternatingRowsDefaultCellStyle.BackColor = objAlternateBackColor;
                            x.RowTemplate.DefaultCellStyle.ForeColor = objForeColor;
                            x.RowTemplate.DefaultCellStyle.BackColor = objBackColor;
                            foreach (DataGridViewTextBoxColumn objColumn in x.Columns)
                            {
                                objColumn.DefaultCellStyle.ForeColor = objForeColor;
                                objColumn.DefaultCellStyle.BackColor = objBackColor;
                            }
                        }, token).ConfigureAwait(false);
                        break;
                    }
                case SplitContainer objSplitControl:
                    {
                        Color objColor = blnLightMode
                            ? SplitterColorLight
                            : SplitterColorDark;
                        await objSplitControl.DoThreadSafeAsync(x =>
                        {
                            x.ForeColor = objColor;
                            x.BackColor = objColor;
                        }, token).ConfigureAwait(false);
                        await ApplyColorsRecursivelyAsync(
                            await objSplitControl.DoThreadSafeFuncAsync(x => x.Panel1, token: token).ConfigureAwait(false), blnLightMode, token).ConfigureAwait(false);
                        await ApplyColorsRecursivelyAsync(
                            await objSplitControl.DoThreadSafeFuncAsync(x => x.Panel2, token: token).ConfigureAwait(false), blnLightMode, token).ConfigureAwait(false);
                        break;
                    }
                case TreeView treControl:
                    {
                        Color objForeColor;
                        Color objBackColor;
                        if (blnLightMode)
                        {
                            objForeColor = WindowTextLight;
                            objBackColor = WindowLight;
                        }
                        else
                        {
                            objForeColor = WindowTextDark;
                            objBackColor = WindowDark;
                        }
                        await treControl.DoThreadSafeAsync(x =>
                        {
                            x.ForeColor = objForeColor;
                            x.BackColor = objBackColor;
                            x.LineColor = objForeColor;
                        }, token).ConfigureAwait(false);
                        foreach (TreeNode objNode in await treControl.DoThreadSafeFuncAsync(x => x.Nodes, token).ConfigureAwait(false))
                            await ApplyColorsRecursivelyAsync(objNode, blnLightMode, token).ConfigureAwait(false);
                        break;
                    }
                case TextBox txtControl:
                    {
                        await txtControl.DoThreadSafeAsync(x =>
                        {
                            if (x.ForeColor != ErrorColor)
                            {
                                if (blnLightMode)
                                {
                                    x.ForeColor = WindowTextLight;
                                    if (x.ReadOnly && (x.BackColor == ControlLight || x.BackColor == ControlDark))
                                        x.BackColor = ControlLight;
                                    else
                                        x.BackColor = WindowLight;
                                }
                                else
                                {
                                    x.ForeColor = WindowTextDark;
                                    if (x.ReadOnly && (x.BackColor == ControlLight || x.BackColor == ControlDark))
                                        x.BackColor = ControlDark;
                                    else
                                        x.BackColor = WindowDark;
                                }
                            }
                            else if (blnLightMode)
                            {
                                if (x.ReadOnly && (x.BackColor == ControlLight || x.BackColor == ControlDark))
                                    x.BackColor = ControlLight;
                                else
                                    x.BackColor = WindowLight;
                            }
                            else if (x.ReadOnly && (x.BackColor == ControlLight || x.BackColor == ControlDark))
                                x.BackColor = ControlDark;
                            else
                                x.BackColor = WindowDark;
                        }, token).ConfigureAwait(false);
                        break;
                    }
                case ListView objListView:
                    {
                        Color objForeColor;
                        Color objBackColor;
                        if (blnLightMode)
                        {
                            objForeColor = WindowTextLight;
                            objBackColor = WindowLight;
                        }
                        else
                        {
                            objForeColor = WindowTextDark;
                            objBackColor = WindowDark;
                        }

                        await objListView.DoThreadSafeAsync(x =>
                        {
                            x.ForeColor = objForeColor;
                            x.BackColor = objBackColor;
                            if (blnLightMode)
                            {
                                foreach (DiceRollerListViewItem objItem in x.Items)
                                {
                                    if (objItem.IsHit)
                                    {
                                        if (objItem.IsGlitch)
                                        {
                                            objItem.ForeColor = DieGlitchHitForeLight;
                                            objItem.BackColor = DieGlitchHitBackgroundLight;
                                        }
                                        else
                                        {
                                            objItem.ForeColor = DieHitForeLight;
                                            objItem.BackColor = DieHitBackgroundLight;
                                        }
                                    }
                                    else if (objItem.IsGlitch)
                                    {
                                        objItem.ForeColor = DieGlitchForeLight;
                                        objItem.BackColor = DieGlitchBackgroundLight;
                                    }
                                    else
                                    {
                                        objItem.ForeColor = objForeColor;
                                        objItem.BackColor = objBackColor;
                                    }
                                }
                            }
                            else
                            {
                                foreach (DiceRollerListViewItem objItem in x.Items)
                                {
                                    if (objItem.IsHit)
                                    {
                                        if (objItem.IsGlitch)
                                        {
                                            objItem.ForeColor = DieGlitchHitForeDark;
                                            objItem.BackColor = DieGlitchHitBackgroundDark;
                                        }
                                        else
                                        {
                                            objItem.ForeColor = DieHitForeDark;
                                            objItem.BackColor = DieHitBackgroundDark;
                                        }
                                    }
                                    else if (objItem.IsGlitch)
                                    {
                                        objItem.ForeColor = DieGlitchForeDark;
                                        objItem.BackColor = DieGlitchBackgroundDark;
                                    }
                                    else
                                    {
                                        objItem.ForeColor = objForeColor;
                                        objItem.BackColor = objBackColor;
                                    }
                                }
                            }
                        }, token).ConfigureAwait(false);
                        break;
                    }
                case ListBox _:
                case ComboBox _:
                case TableCell _:
                    {
                        Color objForeColor;
                        Color objBackColor;
                        if (blnLightMode)
                        {
                            objForeColor = WindowTextLight;
                            objBackColor = WindowLight;
                        }
                        else
                        {
                            objForeColor = WindowTextDark;
                            objBackColor = WindowDark;
                        }
                        await objControl.DoThreadSafeAsync(x =>
                        {
                            x.ForeColor = objForeColor;
                            x.BackColor = objBackColor;
                        }, token).ConfigureAwait(false);
                        break;
                    }
                case GroupBox _:
                    {
                        Color objForeColor;
                        Color objBackColor;
                        if (blnLightMode)
                        {
                            objForeColor = ControlTextLight;
                            objBackColor = ControlLight;
                        }
                        else
                        {
                            objForeColor = ControlTextDark;
                            objBackColor = ControlDark;
                        }
                        await objControl.DoThreadSafeAsync(x =>
                        {
                            x.ForeColor = objForeColor;
                            x.BackColor = objBackColor;
                        }, token).ConfigureAwait(false);
                        break;
                    }
                case ContactControl _:
                case PetControl _:
                case SkillControl _:
                case KnowledgeSkillControl _:
                    // These controls have colors that are always data-bound
                    break;

                case RichTextBox _:
                    // Rtf TextBox is special because we don't want any color changes, otherwise it will mess up the saved Rtf text
                    return;

                case CheckBox chkControl:
                    if (await chkControl.DoThreadSafeFuncAsync(x => x.Appearance == Appearance.Button, token).ConfigureAwait(false) || chkControl is DpiFriendlyCheckBoxDisguisedAsButton)
                    {
                        await ApplyButtonStyle().ConfigureAwait(false);
                        break;
                    }

                    if (chkControl is ColorableCheckBox chkControlColored)
                    {
                        Color objForeColor;
                        if (blnLightMode)
                        {
                            objForeColor = ControlTextLight; // Disabled case for Light mode already handled by the DefaultColorScheme = true property
                        }
                        else
                        {
                            objForeColor = await chkControlColored.DoThreadSafeFuncAsync(x => x.Enabled, token).ConfigureAwait(false)
                                ? ControlTextDark
                                : GrayTextDark;
                        }
                        await chkControlColored.DoThreadSafeAsync(x =>
                        {
                            x.DefaultColorScheme = blnLightMode;
                            x.ForeColor = objForeColor;
                        }, token).ConfigureAwait(false);
                        break;
                    }
                    goto default;

                case Button cmdControl:
                    if (await cmdControl.DoThreadSafeFuncAsync(x => x.FlatStyle, token).ConfigureAwait(false) == FlatStyle.Flat)
                        goto default;
                    await ApplyButtonStyle().ConfigureAwait(false);
                    break;

                case HeaderCell _:
                    // Header cells should use inverted colors
                    {
                        Color objForeColor;
                        Color objBackColor;
                        if (blnLightMode)
                        {
                            objForeColor = ControlLightestLight;
                            objBackColor = ControlDark;
                        }
                        else
                        {
                            objForeColor = ControlLightestDark;
                            objBackColor = ControlLight;
                        }

                        await objControl.DoThreadSafeAsync(x =>
                        {
                            x.ForeColor = objForeColor;
                            x.BackColor = objBackColor;
                        }, token).ConfigureAwait(false);
                        return;
                    }
                case TableLayoutPanel tlpControl:
                    await tlpControl.DoThreadSafeAsync(x =>
                    {
                        if (x.BorderStyle != BorderStyle.None)
                            x.BorderStyle = blnLightMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                    }, token).ConfigureAwait(false);
                    goto default;
                case Form frmControl:
                    MenuStrip objMainMenuStrip = await frmControl.DoThreadSafeFuncAsync(x => x.MainMenuStrip, token: token).ConfigureAwait(false);
                    if (objMainMenuStrip != null)
                    {
                        foreach (ToolStripMenuItem tssItem in await objMainMenuStrip.DoThreadSafeFuncAsync(x => x.Items, token).ConfigureAwait(false))
                            await ApplyColorsRecursivelyAsync(tssItem, blnLightMode, token).ConfigureAwait(false);
                    }
                    goto default;
                case TabControl objTabControl:
                    foreach (TabPage tabPage in await objTabControl.DoThreadSafeFuncAsync(x => x.TabPages, token).ConfigureAwait(false))
                        await ApplyColorsRecursivelyAsync(tabPage, blnLightMode, token).ConfigureAwait(false);
                    goto default;
                case ToolStrip tssStrip:
                    foreach (ToolStripItem tssItem in tssStrip.Items)
                        await ApplyColorsRecursivelyAsync(tssItem, blnLightMode, token).ConfigureAwait(false);
                    goto default;
                default:
                    {
                        Color objControlLightColor = ControlLight;
                        Color objControlDarkColor = ControlDark;
                        Color objControlDarkerLightColor = ControlDarkerLight;
                        Color objControlDarkestLightColor = ControlDarkestLight;
                        Color objControlDarkerDarkColor = ControlDarkerDark;
                        Color objControlDarkestDarkColor = ControlDarkestDark;
                        Color objControlTextLightColor = ControlTextLight;
                        Color objControlTextDarkColor = ControlTextDark;
                        await objControl.DoThreadSafeAsync(x =>
                        {
                            if (blnLightMode)
                            {
                                if (x.ForeColor == objControlDarkColor)
                                    x.ForeColor = objControlLightColor;
                                else if (x.ForeColor == objControlDarkerDarkColor)
                                    x.ForeColor = objControlDarkerLightColor;
                                else if (x.ForeColor == objControlDarkestDarkColor)
                                    x.ForeColor = objControlDarkestLightColor;
                                else if (x.ForeColor == objControlTextLightColor || x.ForeColor == objControlTextDarkColor)
                                    x.ForeColor = objControlTextLightColor;
                            }
                            else if (x.ForeColor == objControlLightColor)
                                x.ForeColor = objControlDarkColor;
                            else if (x.ForeColor == objControlDarkerLightColor)
                                x.ForeColor = objControlDarkerDarkColor;
                            else if (x.ForeColor == objControlDarkestLightColor)
                                x.ForeColor = objControlDarkestDarkColor;
                            else if (x.ForeColor == objControlTextLightColor || x.ForeColor == objControlTextDarkColor)
                                x.ForeColor = objControlTextDarkColor;
                        }, token).ConfigureAwait(false);
                        // These controls never have backgrounds set explicitly, so shouldn't have their backgrounds overwritten
                        if (!(objControl is Label || objControl is CheckBox || objControl is PictureBox
                              || objControl is Button || (objControl is Panel && !(objControl is SplitterPanel
                                                                                   || (await objControl
                                                                                       .DoThreadSafeFuncAsync(
                                                                                           x => x.BackColor, token).ConfigureAwait(false)).A
                                                                                   == byte.MaxValue))))
                        {
                            Color objControlLighterLightColor
                                = ControlLighterLight;
                            Color objControlLightestLightColor
                                = ControlLightestLight;
                            Color objControlLighterDarkColor
                                = ControlLighterDark;
                            Color objControlLightestDarkColor
                                = ControlLightestDark;
                            await objControl.DoThreadSafeAsync(x =>
                            {
                                if (blnLightMode)
                                {
                                    if (x.BackColor == objControlLighterDarkColor)
                                        x.BackColor = objControlLighterLightColor;
                                    else if (x.BackColor == objControlLightestDarkColor)
                                        x.BackColor = objControlLightestLightColor;
                                    else
                                        x.BackColor = objControlLightColor;
                                }
                                else if (x.BackColor == objControlLighterLightColor)
                                    x.BackColor = objControlLighterDarkColor;
                                else if (x.BackColor == objControlLightestLightColor)
                                    x.BackColor = objControlLightestDarkColor;
                                else
                                    x.BackColor = objControlDarkColor;
                            }, token).ConfigureAwait(false);
                        }

                        break;
                    }
            }

            foreach (Control objChild in await objControl.DoThreadSafeFuncAsync(x => x.Controls, token).ConfigureAwait(false))
                await ApplyColorsRecursivelyAsync(objChild, blnLightMode, token).ConfigureAwait(false);
        }

        private static async Task ApplyColorsRecursivelyAsync(ToolStripItem tssItem, bool blnLightMode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            Color objControlLightColor = ControlLight;
            Color objControlDarkColor = ControlDark;

            ToolStrip objParent = tssItem.GetCurrentParent();
            if (objParent != null)
                await objParent.DoThreadSafeAsync(DoColor, token).ConfigureAwait(false);
            else
                DoColor();

            void DoColor()
            {
                if (blnLightMode)
                {
                    if (tssItem.ForeColor == objControlDarkColor)
                        tssItem.ForeColor = objControlLightColor;
                    else if (tssItem.ForeColor == ControlDarkerDark)
                        tssItem.ForeColor = ControlDarkerLight;
                    else if (tssItem.ForeColor == ControlDarkestDark)
                        tssItem.ForeColor = ControlDarkestLight;
                    else
                        tssItem.ForeColor = ControlTextLight;
                    token.ThrowIfCancellationRequested();
                    if (tssItem.BackColor == ControlLighterDark)
                        tssItem.BackColor = ControlLighterLight;
                    else if (tssItem.BackColor == ControlLightestDark)
                        tssItem.BackColor = ControlLightestLight;
                    else
                        tssItem.BackColor = objControlLightColor;
                }
                else
                {
                    if (tssItem.ForeColor == objControlLightColor)
                        tssItem.ForeColor = objControlDarkColor;
                    else if (tssItem.ForeColor == ControlDarkerLight)
                        tssItem.ForeColor = ControlDarkerDark;
                    else if (tssItem.ForeColor == ControlDarkestLight)
                        tssItem.ForeColor = ControlDarkestDark;
                    else
                        tssItem.ForeColor = ControlTextDark;
                    token.ThrowIfCancellationRequested();
                    if (tssItem.BackColor == ControlLighterLight)
                        tssItem.BackColor = ControlLighterDark;
                    else if (tssItem.BackColor == ControlLightestLight)
                        tssItem.BackColor = ControlLightestDark;
                    else
                        tssItem.BackColor = objControlDarkColor;
                }
            }

            token.ThrowIfCancellationRequested();
            switch (tssItem)
            {
                case ToolStripDropDownItem tssDropDownItem:
                    foreach (ToolStripItem tssDropDownChild in tssDropDownItem.DropDownItems)
                        await ApplyColorsRecursivelyAsync(tssDropDownChild, blnLightMode, token).ConfigureAwait(false);
                    break;
                case ColorableToolStripSeparator tssSeparator when objParent != null:
                    await objParent.DoThreadSafeAsync(() => tssSeparator.DefaultColorScheme = blnLightMode, token).ConfigureAwait(false);
                    break;
                case ColorableToolStripSeparator tssSeparator:
                    tssSeparator.DefaultColorScheme = blnLightMode;
                    break;
            }
        }

        private static async Task ApplyColorsRecursivelyAsync(TreeNode nodNode, bool blnLightMode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            Color objBackColor = blnLightMode ? WindowLight : WindowDark;

            TreeView treView = nodNode.TreeView;
            if (treView != null)
                await treView.DoThreadSafeAsync(DoColor, token).ConfigureAwait(false);
            else
                DoColor();

            void DoColor()
            {
                if (blnLightMode)
                {
                    if (nodNode.ForeColor == HasNotesColorDark)
                        nodNode.ForeColor = HasNotesColorLight;
                    else if (nodNode.ForeColor == GrayHasNotesColorDark)
                        nodNode.ForeColor = GrayHasNotesColorLight;
                    else if (nodNode.ForeColor == WindowTextDark)
                        nodNode.ForeColor = WindowTextLight;
                }
                else
                {
                    if (nodNode.ForeColor == HasNotesColorLight)
                        nodNode.ForeColor = HasNotesColorDark;
                    else if (nodNode.ForeColor == GrayHasNotesColorLight)
                        nodNode.ForeColor = GrayHasNotesColorDark;
                    else if (nodNode.ForeColor == WindowTextLight)
                        nodNode.ForeColor = WindowTextDark;
                }
                nodNode.BackColor = objBackColor;
            }

            token.ThrowIfCancellationRequested();
            foreach (TreeNode nodNodeChild in nodNode.Nodes)
                await ApplyColorsRecursivelyAsync(nodNodeChild, blnLightMode, token).ConfigureAwait(false);
        }

        #endregion Color Inversion Methods

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
                Math.Max(Math.Min(Convert.ToInt32(Math.Round(dblRed * byte.MaxValue, MidpointRounding.AwayFromZero)), byte.MaxValue), byte.MinValue),
                Math.Max(Math.Min(Convert.ToInt32(Math.Round(dblGreen * byte.MaxValue, MidpointRounding.AwayFromZero)), byte.MaxValue), byte.MinValue),
                Math.Max(Math.Min(Convert.ToInt32(Math.Round(dblBlue * byte.MaxValue, MidpointRounding.AwayFromZero)), byte.MaxValue), byte.MinValue));
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
                Math.Max(Math.Min(Convert.ToInt32(Math.Round(dblRed * byte.MaxValue, MidpointRounding.AwayFromZero)), byte.MaxValue), byte.MinValue),
                Math.Max(Math.Min(Convert.ToInt32(Math.Round(dblGreen * byte.MaxValue, MidpointRounding.AwayFromZero)), byte.MaxValue), byte.MinValue),
                Math.Max(Math.Min(Convert.ToInt32(Math.Round(dblBlue * byte.MaxValue, MidpointRounding.AwayFromZero)), byte.MaxValue), byte.MinValue));
        }

        #endregion Color Utility Methods
    }
}
