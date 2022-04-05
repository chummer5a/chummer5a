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
            s_TmrDarkModeCheckerTimer.Tick += TmrDarkModeCheckerTimerOnTick;
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
            await AutoApplyLightDarkModeAsync();
        }

        public static void AutoApplyLightDarkMode()
        {
            if (GlobalSettings.ColorModeSetting == ColorMode.Automatic)
            {
                IsLightMode = !DoesRegistrySayDarkMode();
                s_TmrDarkModeCheckerTimer.Enabled = true;
            }
        }

        public static async Task AutoApplyLightDarkModeAsync()
        {
            if (GlobalSettings.ColorModeSetting == ColorMode.Automatic)
            {
                await SetIsLightModeAsync(!DoesRegistrySayDarkMode());
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
                    Program.MainForm.UpdateLightDarkMode();
            }
        }

        public static Task SetIsLightModeAsync(bool blnNewValue)
        {
            if (_blnIsLightMode == blnNewValue)
                return Task.CompletedTask;
            _blnIsLightMode = blnNewValue;
            return Program.MainForm == null ? Task.CompletedTask : Inner();
            async Task Inner()
            {
                CursorWait objCursorWait = await CursorWait.NewAsync(Program.MainForm);
                try
                {
                    await Program.MainForm.UpdateLightDarkModeAsync();
                }
                finally
                {
                    await objCursorWait.DisposeAsync();
                }
            }
        }

        private static readonly LockingDictionary<Color, Color> s_DicDarkModeColors = new LockingDictionary<Color, Color>();
        private static readonly LockingDictionary<Color, Color> s_DicInverseDarkModeColors = new LockingDictionary<Color, Color>();
        private static readonly LockingDictionary<Color, Color> s_DicDimmedColors = new LockingDictionary<Color, Color>();
        private static readonly LockingDictionary<Color, Color> s_DicBrightenedColors = new LockingDictionary<Color, Color>();

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
        /// Returns a version of a color that has its lightness almost inverted (slightly increased lightness from inversion, slight desaturation)
        /// </summary>
        /// <param name="objColor">Color whose lightness and saturation should be adjusted for Dark Mode.</param>
        /// <returns>New Color object identical to <paramref name="objColor"/>, but with lightness and saturation adjusted for Dark Mode.</returns>
        public static async Task<Color> GenerateDarkModeColorAsync(Color objColor)
        {
            (bool blnSuccess, Color objDarkModeColor) = await s_DicDarkModeColors.TryGetValueAsync(objColor);
            if (!blnSuccess)
            {
                objDarkModeColor = GetDarkModeVersion(objColor);
                await s_DicDarkModeColors.TryAddAsync(objColor, objDarkModeColor);
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
        /// Returns an inverted version of a color that has gone through GenerateDarkModeColor()
        /// </summary>
        /// <param name="objColor">Color whose Dark Mode conversions for lightness and saturation should be inverted.</param>
        /// <returns>New Color object identical to <paramref name="objColor"/>, but with its Dark Mode conversion inverted.</returns>
        public static async Task<Color> GenerateInverseDarkModeColorAsync(Color objColor)
        {
            (bool blnSuccess, Color objInverseDarkModeColor) = await s_DicInverseDarkModeColors.TryGetValueAsync(objColor);
            if (!blnSuccess)
            {
                objInverseDarkModeColor = InverseGetDarkModeVersion(objColor);
                await s_DicInverseDarkModeColors.TryAddAsync(objColor, objInverseDarkModeColor);
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
        /// Returns a version of a color that has is adapted to the current Color mode setting (same color in Light mode, changed one in Dark mode)
        /// </summary>
        /// <param name="objColor">Color as it would be in Light mode</param>
        /// <returns>New Color object identical to <paramref name="objColor"/>, but potentially adapted to dark mode.</returns>
        public static Task<Color> GenerateCurrentModeColorAsync(Color objColor)
        {
            return IsLightMode ? Task.FromResult(objColor) : GenerateDarkModeColorAsync(objColor);
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
        /// Returns a version of a color that is independent of the current Color mode and can savely be used for storing.
        /// </summary>
        /// <param name="objColor">Color as it is shown in current color mode</param>
        /// <returns>New Color object identical to <paramref name="objColor"/>, but potentially adapted to light mode.</returns>
        public static Task<Color> GenerateModeIndependentColorAsync(Color objColor)
        {
            return IsLightMode ? Task.FromResult(objColor) : GenerateInverseDarkModeColorAsync(objColor);
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
            else if (!s_DicBrightenedColors.TryGetValue(objColor, out objRetColor))
            {
                objRetColor = GetBrightenedVersion(objColor);
                s_DicBrightenedColors.TryAdd(objColor, objRetColor);
            }

            return objRetColor;
        }

        /// <summary>
        /// Returns a version of a color that has its lightness dimmed down in Light mode or brightened in Dark Mode
        /// </summary>
        /// <param name="objColor">Color whose lightness should be dimmed.</param>
        /// <returns>New Color object identical to <paramref name="objColor"/>, but with its lightness values dimmed.</returns>
        public static async Task<Color> GenerateCurrentModeDimmedColorAsync(Color objColor)
        {
            bool blnSuccess;
            Color objRetColor;
            if (IsLightMode)
            {
                (blnSuccess, objRetColor) = await s_DicDimmedColors.TryGetValueAsync(objColor);
                if (!blnSuccess)
                {
                    objRetColor = GetDimmedVersion(objColor);
                    await s_DicDimmedColors.TryAddAsync(objColor, objRetColor);
                }
            }
            else
            {
                (blnSuccess, objRetColor) = await s_DicBrightenedColors.TryGetValueAsync(objColor);
                if (!blnSuccess)
                {
                    objRetColor = GetBrightenedVersion(objColor);
                    await s_DicBrightenedColors.TryAddAsync(objColor, objRetColor);
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

        /// <summary>
        /// Because the transforms applied to convert a Light Mode color to Dark Mode cannot produce some ranges of lightness and saturation, not all colors are valid in Dark Mode.
        /// This function takes a color intended for Dark Mode and converts it to the closest possible color that is valid in Dark Mode.
        /// If the original color is valid in Dark Mode to begin with, the transforms should end up reproducing it.
        /// </summary>
        /// <param name="objColor">Color to adjust, originally specified within Dark Mode.</param>
        /// <returns>New Color very similar to <paramref name="objColor"/>, but with lightness and saturation values set to within the range allowable in Dark Mode.</returns>
        private static async Task<Color> TransformToDarkModeValidVersionAsync(Color objColor)
        {
            return await GenerateDarkModeColorAsync(await GenerateInverseDarkModeColorAsync(objColor));
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
        private static Color ControlTextLight { get; } = SystemColors.ControlText;
        private static Color ControlTextDark { get; } = GenerateDarkModeColor(ControlTextLight);

        public static Color ControlDarkest => IsLightMode ? ControlDarkestLight : ControlDarkestDark;
        private static Color ControlDarkestLight { get; } = SystemColors.ControlDarkDark;
        private static Color ControlDarkestDark { get; } = GenerateDarkModeColor(ControlDarkestLight);

        public static Color ControlDarker => IsLightMode ? ControlDarkerLight : ControlDarkerDark;
        private static Color ControlDarkerLight { get; } = SystemColors.ControlDark;
        private static Color ControlDarkerDark { get; } = GenerateDarkModeColor(ControlDarkerLight);

        public static Color Control => IsLightMode ? ControlLight : ControlDark;
        private static Color ControlLight { get; } = SystemColors.Control;
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

        public static Task<Color> WindowTextAsync => IsLightMode ? WindowTextLightAsync : WindowTextDarkAsync;
        private static Task<Color> WindowTextLightAsync => Task.FromResult(WindowTextLight);
        private static Task<Color> WindowTextDarkAsync => GenerateDarkModeColorAsync(WindowTextLight);

        public static Task<Color> WindowAsync => IsLightMode ? WindowLightAsync : WindowDarkAsync;
        private static Task<Color> WindowLightAsync => Task.FromResult(WindowLight);
        private static Task<Color> WindowDarkAsync => GenerateDarkModeColorAsync(WindowLight);

        public static Task<Color> InfoTextAsync => IsLightMode ? InfoTextLightAsync : InfoTextDarkAsync;
        private static Task<Color> InfoTextLightAsync => Task.FromResult(InfoTextLight);
        private static Task<Color> InfoTextDarkAsync => GenerateDarkModeColorAsync(InfoTextLight);

        public static Task<Color> InfoAsync => IsLightMode ? InfoLightAsync : InfoDarkAsync;
        private static Task<Color> InfoLightAsync => Task.FromResult(InfoLight);
        private static Task<Color> InfoDarkAsync => GenerateDarkModeColorAsync(InfoLight);

        public static Task<Color> GrayTextAsync => IsLightMode ? GrayTextLightAsync : GrayTextDarkAsync;
        private static Task<Color> GrayTextLightAsync => Task.FromResult(GrayTextLight);
        private static Task<Color> GrayTextDarkAsync => GenerateDarkModeColorAsync(GrayTextLight);

        public static Task<Color> HighlightTextAsync => IsLightMode ? HighlightTextLightAsync : HighlightTextDarkAsync;
        private static Task<Color> HighlightTextLightAsync => Task.FromResult(HighlightTextLight);
        private static Task<Color> HighlightTextDarkAsync => GenerateDarkModeColorAsync(HighlightTextLight);

        public static Task<Color> HighlightAsync => IsLightMode ? HighlightLightAsync : HighlightDarkAsync;
        private static Task<Color> HighlightLightAsync => Task.FromResult(HighlightLight);
        private static Task<Color> HighlightDarkAsync => GenerateDarkModeColorAsync(HighlightLight);

        public static Task<Color> ControlTextAsync => IsLightMode ? ControlTextLightAsync : ControlTextDarkAsync;
        private static Task<Color> ControlTextLightAsync => Task.FromResult(ControlTextLight);
        private static Task<Color> ControlTextDarkAsync => GenerateDarkModeColorAsync(ControlTextLight);

        public static Task<Color> ControlDarkestAsync => IsLightMode ? ControlDarkestLightAsync : ControlDarkestDarkAsync;
        private static Task<Color> ControlDarkestLightAsync => Task.FromResult(ControlDarkestLight);
        private static Task<Color> ControlDarkestDarkAsync => GenerateDarkModeColorAsync(ControlDarkestLight);

        public static Task<Color> ControlDarkerAsync => IsLightMode ? ControlDarkerLightAsync : ControlDarkerDarkAsync;
        private static Task<Color> ControlDarkerLightAsync => Task.FromResult(ControlDarkerLight);
        private static Task<Color> ControlDarkerDarkAsync => GenerateDarkModeColorAsync(ControlDarkerLight);

        public static Task<Color> ControlAsync => IsLightMode ? ControlLightAsync : ControlDarkAsync;
        private static Task<Color> ControlLightAsync => Task.FromResult(ControlLight);
        private static Task<Color> ControlDarkAsync => GenerateDarkModeColorAsync(ControlLight);

        public static Task<Color> ControlLighterAsync => IsLightMode ? ControlLighterLightAsync : ControlLighterDarkAsync;
        private static Task<Color> ControlLighterLightAsync => Task.FromResult(ControlLighterLight);
        private static Task<Color> ControlLighterDarkAsync => GenerateDarkModeColorAsync(ControlLight);

        public static Task<Color> ControlLightestAsync => IsLightMode ? ControlLightestLightAsync : ControlLightestDarkAsync;
        private static Task<Color> ControlLightestLightAsync => Task.FromResult(ControlLightestLight);
        private static Task<Color> ControlLightestDarkAsync => GenerateDarkModeColorAsync(ControlLightestLight);

        public static Task<Color> ButtonFaceAsync => IsLightMode ? ButtonFaceLightAsync : ButtonFaceDarkAsync;
        private static Task<Color> ButtonFaceLightAsync => Task.FromResult(ButtonFaceLight);
        private static Task<Color> ButtonFaceDarkAsync => GenerateDarkModeColorAsync(ButtonFaceLight);

        public static Task<Color> ButtonShadowAsync => IsLightMode ? ButtonShadowLightAsync : ButtonShadowDarkAsync;
        private static Task<Color> ButtonShadowLightAsync => Task.FromResult(ButtonShadowLight);
        private static Task<Color> ButtonShadowDarkAsync => GenerateDarkModeColorAsync(ButtonShadowLight);

        public static Task<Color> AppWorkspaceAsync => IsLightMode ? AppWorkspaceLightAsync : AppWorkspaceDarkAsync;
        private static Task<Color> AppWorkspaceLightAsync => Task.FromResult(AppWorkspaceLight);
        private static Task<Color> AppWorkspaceDarkAsync => GenerateDarkModeColorAsync(AppWorkspaceLight);

        public static Task<Color> SplitterColorAsync => IsLightMode ? SplitterColorLightAsync : SplitterColorDarkAsync;
        private static Task<Color> SplitterColorLightAsync => Task.FromResult(SplitterColorLight);
        private static Task<Color> SplitterColorDarkAsync => GenerateDarkModeColorAsync(SplitterColorLight);

        public static Task<Color> HasNotesColorAsync => IsLightMode ? HasNotesColorLightAsync : HasNotesColorDarkAsync;
        private static Task<Color> HasNotesColorLightAsync => Task.FromResult(HasNotesColorLight);
        private static Task<Color> HasNotesColorDarkAsync => GenerateDarkModeColorAsync(HasNotesColorLight);

        public static Task<Color> GrayHasNotesColorAsync => IsLightMode ? GrayHasNotesColorLightAsync : GrayHasNotesColorDarkAsync;
        private static Task<Color> GrayHasNotesColorLightAsync => Task.FromResult(GrayHasNotesColorLight);
        private static Task<Color> GrayHasNotesColorDarkAsync => GenerateDarkModeColorAsync(GrayHasNotesColorLight);

        public static Task<Color> DieGlitchForeAsync => IsLightMode ? DieGlitchForeLightAsync : DieGlitchForeDarkAsync;
        private static Task<Color> DieGlitchForeLightAsync => Task.FromResult(DieGlitchForeLight);
        private static Task<Color> DieGlitchForeDarkAsync => GenerateDarkModeColorAsync(DieGlitchForeLight);
        public static Task<Color> DieGlitchBackgroundAsync => IsLightMode ? DieGlitchBackgroundLightAsync : DieGlitchBackgroundDarkAsync;
        private static Task<Color> DieGlitchBackgroundLightAsync => Task.FromResult(DieGlitchBackgroundLight);
        private static Task<Color> DieGlitchBackgroundDarkAsync => GenerateDarkModeColorAsync(DieGlitchBackgroundLight);
        public static Task<Color> DieHitForeAsync => IsLightMode ? DieHitForeLightAsync : DieHitForeDarkAsync;
        private static Task<Color> DieHitForeLightAsync => Task.FromResult(DieHitForeLight);
        private static Task<Color> DieHitForeDarkAsync => GenerateDarkModeColorAsync(DieHitForeLight);
        public static Task<Color> DieHitBackgroundAsync => IsLightMode ? DieHitBackgroundLightAsync : DieHitBackgroundDarkAsync;
        private static Task<Color> DieHitBackgroundLightAsync => Task.FromResult(DieHitBackgroundLight);
        private static Task<Color> DieHitBackgroundDarkAsync => GenerateDarkModeColorAsync(DieHitBackgroundLight);
        public static Task<Color> DieGlitchHitForeAsync => IsLightMode ? DieGlitchHitForeLightAsync : DieGlitchHitForeDarkAsync;
        private static Task<Color> DieGlitchHitForeLightAsync => Task.FromResult(DieGlitchHitForeLight);
        private static Task<Color> DieGlitchHitForeDarkAsync => GenerateDarkModeColorAsync(DieHitForeLight);
        public static Task<Color> DieGlitchHitBackgroundAsync => IsLightMode ? DieGlitchHitBackgroundLightAsync : DieGlitchHitBackgroundDarkAsync;
        private static Task<Color> DieGlitchHitBackgroundLightAsync => Task.FromResult(DieGlitchHitBackgroundLight);
        private static Task<Color> DieGlitchHitBackgroundDarkAsync => GenerateDarkModeColorAsync(DieHitBackgroundLight);

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

        public static Task UpdateLightDarkModeAsync(this Control objControl)
        {
            return ApplyColorsRecursivelyAsync(objControl, IsLightMode);
        }

        public static Task UpdateLightDarkModeAsync(this Control objControl, bool blnLightMode)
        {
            return ApplyColorsRecursivelyAsync(objControl, blnLightMode);
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
            float fltNewValue = Math.Min((float)(fltValue - 0.1302 + 0.14*Math.Sqrt(fltValue - 0.1351)), 1.0f);
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
                objControl.DoThreadSafe(x => x.ForeColor = SystemColors.ControlText);
            }
            switch (objControl)
            {
                case DataGridView objDataGridView:
                    objDataGridView.DoThreadSafe(x =>
                    {
                        x.BackgroundColor = blnLightMode ? AppWorkspaceLight : AppWorkspaceDark;
                        x.GridColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        x.DefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        x.DefaultCellStyle.BackColor = blnLightMode ? ControlLight : ControlDark;
                        x.ColumnHeadersDefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        x.ColumnHeadersDefaultCellStyle.BackColor = blnLightMode ? ControlLight : ControlDark;
                        x.AlternatingRowsDefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        x.AlternatingRowsDefaultCellStyle.BackColor
                            = blnLightMode ? ControlLighterLight : ControlLighterDark;
                        x.RowTemplate.DefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        x.RowTemplate.DefaultCellStyle.BackColor = blnLightMode ? ControlLight : ControlDark;
                        foreach (DataGridViewTextBoxColumn objColumn in x.Columns)
                        {
                            objColumn.DefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                            objColumn.DefaultCellStyle.BackColor = blnLightMode ? ControlLight : ControlDark;
                        }
                    });
                    break;

                case SplitContainer objSplitControl:
                    objSplitControl.DoThreadSafe(x =>
                    {
                        x.ForeColor = blnLightMode
                            ? SplitterColorLight
                            : SplitterColorDark;
                        x.BackColor = blnLightMode ? SplitterColorLight : SplitterColorDark;
                    });
                    ApplyColorsRecursively(objSplitControl.DoThreadSafeFunc(x => x.Panel1), blnLightMode);
                    ApplyColorsRecursively(objSplitControl.DoThreadSafeFunc(x => x.Panel2), blnLightMode);
                    break;

                case TreeView treControl:
                    treControl.DoThreadSafe(x =>
                    {
                        x.ForeColor = blnLightMode ? WindowTextLight : WindowTextDark;
                        x.BackColor = blnLightMode ? WindowLight : WindowDark;
                        x.LineColor = blnLightMode ? WindowTextLight : WindowTextDark;
                        foreach (TreeNode objNode in x.Nodes)
                            ApplyColorsRecursively(objNode, blnLightMode);
                    });
                    break;

                case TextBox txtControl:
                    txtControl.DoThreadSafe(x =>
                    {
                        if (x.ForeColor == ErrorColor)
                            x.ForeColor = ErrorColor;
                        else if (blnLightMode)
                            x.ForeColor = WindowTextLight;
                        else
                            x.ForeColor = WindowTextDark;
                        if (x.ReadOnly)
                            x.BackColor = blnLightMode ? ControlLight : ControlDark;
                        else
                            x.BackColor = blnLightMode ? WindowLight : WindowDark;
                    });
                    break;

                case ListView objListView:
                    objListView.DoThreadSafe(x =>
                    {
                        x.ForeColor = blnLightMode ? WindowTextLight : WindowTextDark;
                        x.BackColor = blnLightMode ? WindowLight : WindowDark;
                        foreach (DiceRollerListViewItem objItem in x.Items)
                        {
                            if (objItem.IsHit)
                            {
                                if (objItem.IsGlitch)
                                {
                                    objItem.ForeColor = blnLightMode ? DieGlitchHitForeLight : DieGlitchHitForeDark;
                                    objItem.BackColor = blnLightMode
                                        ? DieGlitchHitBackgroundLight
                                        : DieGlitchHitBackgroundDark;
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
                    });
                    break;

                case ListBox _:
                case ComboBox _:
                case TableCell _:
                    objControl.DoThreadSafe(x =>
                    {
                        x.ForeColor = blnLightMode ? WindowTextLight : WindowTextDark;
                        x.BackColor = blnLightMode ? WindowLight : WindowDark;
                    });
                    break;

                case GroupBox _:
                    objControl.DoThreadSafe(x =>
                    {
                        x.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        x.BackColor = blnLightMode ? ControlLight : ControlDark;
                    });
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
                    if (chkControl.DoThreadSafeFunc(x => x.Appearance == Appearance.Button) || chkControl is DpiFriendlyCheckBoxDisguisedAsButton)
                    {
                        ApplyButtonStyle();
                        break;
                    }

                    if (chkControl is ColorableCheckBox chkControlColored)
                    {
                        chkControlColored.DoThreadSafe(x =>
                        {
                            x.DefaultColorScheme = blnLightMode;
                            if (blnLightMode) // Disabled case for Light mode already handled by the switch above
                                x.ForeColor = ControlTextLight;
                            else
                                x.ForeColor = x.Enabled ? ControlTextDark : GrayText;
                        });
                        break;
                    }
                    goto default;

                case Button cmdControl:
                    if (cmdControl.DoThreadSafeFunc(x => x.FlatStyle) == FlatStyle.Flat)
                        goto default;
                    ApplyButtonStyle();
                    break;

                case HeaderCell _:
                    // Header cells should use inverted colors
                    objControl.DoThreadSafe(x =>
                    {
                        x.ForeColor = blnLightMode ? ControlLightestLight : ControlLightestDark;
                        x.BackColor = blnLightMode ? ControlTextLight : ControlTextDark;
                    });
                    return;

                case TableLayoutPanel tlpControl:
                    tlpControl.DoThreadSafe(x =>
                    {
                        if (x.BorderStyle != BorderStyle.None)
                            x.BorderStyle = blnLightMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                    });
                    goto default;
                case Form frmControl:
                    frmControl.DoThreadSafe(x =>
                    {
                        if (x.MainMenuStrip != null)
                            foreach (ToolStripMenuItem tssItem in x.MainMenuStrip.Items)
                                ApplyColorsRecursively(tssItem, blnLightMode);
                    });
                    goto default;
                case TabControl objTabControl:
                    foreach (TabPage tabPage in objTabControl.DoThreadSafeFunc(x => x.TabPages))
                        ApplyColorsRecursively(tabPage, blnLightMode);
                    goto default;
                case ToolStrip tssStrip:
                    tssStrip.DoThreadSafe(x =>
                    {
                        foreach (ToolStripItem tssItem in x.Items)
                            ApplyColorsRecursively(tssItem, blnLightMode);
                    });
                    goto default;
                default:
                    objControl.DoThreadSafe(x =>
                    {
                        if (x.ForeColor == (blnLightMode ? ControlDark : ControlLight))
                            x.ForeColor = blnLightMode ? ControlLight : ControlDark;
                        else if (x.ForeColor == (blnLightMode ? ControlDarkerDark : ControlDarkerLight))
                            x.ForeColor = blnLightMode ? ControlDarkerLight : ControlDarkerDark;
                        else if (x.ForeColor == (blnLightMode ? ControlDarkestDark : ControlDarkestLight))
                            x.ForeColor = blnLightMode ? ControlDarkestLight : ControlDarkestDark;
                        else
                            x.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        // These controls never have backgrounds set explicitly, so shouldn't have their backgrounds overwritten
                        if (!(x is Label || x is CheckBox || x is PictureBox || x is Button
                              || (x is Panel && !(x is SplitterPanel || x is TabPage))))
                        {
                            if (x.BackColor == (blnLightMode ? ControlLighterDark : ControlLighterLight))
                                x.BackColor = blnLightMode ? ControlLighterLight : ControlLighterDark;
                            else if (x.BackColor == (blnLightMode ? ControlLightestDark : ControlLightestLight))
                                x.BackColor = blnLightMode ? ControlLightestLight : ControlLightestDark;
                            else
                                x.BackColor = blnLightMode ? ControlLight : ControlDark;
                        }
                    });

                    break;
            }

            foreach (Control objChild in objControl.DoThreadSafeFunc(x => x.Controls))
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

        private static async Task ApplyColorsRecursivelyAsync(Control objControl, bool blnLightMode)
        {
            Task ApplyButtonStyle()
            {
                // Buttons look weird if colored based on anything other than the default color scheme in dark mode
                return objControl.DoThreadSafeAsync(x => x.ForeColor = SystemColors.ControlText);
            }
            switch (objControl)
            {
                case DataGridView objDataGridView:
                    await objDataGridView.DoThreadSafeAsync(x =>
                    {
                        x.BackgroundColor = blnLightMode ? AppWorkspaceLight : AppWorkspaceDark;
                        x.GridColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        x.DefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        x.DefaultCellStyle.BackColor = blnLightMode ? ControlLight : ControlDark;
                        x.ColumnHeadersDefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        x.ColumnHeadersDefaultCellStyle.BackColor = blnLightMode ? ControlLight : ControlDark;
                        x.AlternatingRowsDefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        x.AlternatingRowsDefaultCellStyle.BackColor
                            = blnLightMode ? ControlLighterLight : ControlLighterDark;
                        x.RowTemplate.DefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        x.RowTemplate.DefaultCellStyle.BackColor = blnLightMode ? ControlLight : ControlDark;
                        foreach (DataGridViewTextBoxColumn objColumn in x.Columns)
                        {
                            objColumn.DefaultCellStyle.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                            objColumn.DefaultCellStyle.BackColor = blnLightMode ? ControlLight : ControlDark;
                        }
                    });
                    break;

                case SplitContainer objSplitControl:
                    await objSplitControl.DoThreadSafeAsync(x =>
                    {
                        x.ForeColor = blnLightMode
                            ? SplitterColorLight
                            : SplitterColorDark;
                        x.BackColor = blnLightMode ? SplitterColorLight : SplitterColorDark;
                    });
                    await ApplyColorsRecursivelyAsync(await objSplitControl.DoThreadSafeFuncAsync(x => x.Panel1), blnLightMode);
                    await ApplyColorsRecursivelyAsync(await objSplitControl.DoThreadSafeFuncAsync(x => x.Panel2), blnLightMode);
                    break;

                case TreeView treControl:
                    await treControl.DoThreadSafeAsync(x =>
                    {
                        x.ForeColor = blnLightMode ? WindowTextLight : WindowTextDark;
                        x.BackColor = blnLightMode ? WindowLight : WindowDark;
                        x.LineColor = blnLightMode ? WindowTextLight : WindowTextDark;
                        foreach (TreeNode objNode in x.Nodes)
                            ApplyColorsRecursively(objNode, blnLightMode);
                    });
                    break;

                case TextBox txtControl:
                    await txtControl.DoThreadSafeAsync(x =>
                    {
                        if (x.ForeColor == ErrorColor)
                            x.ForeColor = ErrorColor;
                        else if (blnLightMode)
                            x.ForeColor = WindowTextLight;
                        else
                            x.ForeColor = WindowTextDark;
                        if (x.ReadOnly)
                            x.BackColor = blnLightMode ? ControlLight : ControlDark;
                        else
                            x.BackColor = blnLightMode ? WindowLight : WindowDark;
                    });
                    break;

                case ListView objListView:
                    await objListView.DoThreadSafeAsync(x =>
                    {
                        x.ForeColor = blnLightMode ? WindowTextLight : WindowTextDark;
                        x.BackColor = blnLightMode ? WindowLight : WindowDark;
                        foreach (DiceRollerListViewItem objItem in x.Items)
                        {
                            if (objItem.IsHit)
                            {
                                if (objItem.IsGlitch)
                                {
                                    objItem.ForeColor = blnLightMode ? DieGlitchHitForeLight : DieGlitchHitForeDark;
                                    objItem.BackColor = blnLightMode
                                        ? DieGlitchHitBackgroundLight
                                        : DieGlitchHitBackgroundDark;
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
                    });
                    break;

                case ListBox _:
                case ComboBox _:
                case TableCell _:
                    await objControl.DoThreadSafeAsync(x =>
                    {
                        x.ForeColor = blnLightMode ? WindowTextLight : WindowTextDark;
                        x.BackColor = blnLightMode ? WindowLight : WindowDark;
                    });
                    break;

                case GroupBox _:
                    await objControl.DoThreadSafeAsync(x =>
                    {
                        x.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        x.BackColor = blnLightMode ? ControlLight : ControlDark;
                    });
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
                    if (await chkControl.DoThreadSafeFuncAsync(x => x.Appearance == Appearance.Button) || chkControl is DpiFriendlyCheckBoxDisguisedAsButton)
                    {
                        await ApplyButtonStyle();
                        break;
                    }

                    if (chkControl is ColorableCheckBox chkControlColored)
                    {
                        await chkControlColored.DoThreadSafeAsync(x =>
                        {
                            x.DefaultColorScheme = blnLightMode;
                            if (blnLightMode) // Disabled case for Light mode already handled by the switch above
                                x.ForeColor = ControlTextLight;
                            else
                                x.ForeColor = x.Enabled ? ControlTextDark : GrayText;
                        });
                        break;
                    }
                    goto default;

                case Button cmdControl:
                    if (await cmdControl.DoThreadSafeFuncAsync(x => x.FlatStyle) == FlatStyle.Flat)
                        goto default;
                    await ApplyButtonStyle();
                    break;

                case HeaderCell _:
                    // Header cells should use inverted colors
                    await objControl.DoThreadSafeAsync(x =>
                    {
                        x.ForeColor = blnLightMode ? ControlLightestLight : ControlLightestDark;
                        x.BackColor = blnLightMode ? ControlTextLight : ControlTextDark;
                    });
                    return;

                case TableLayoutPanel tlpControl:
                    await tlpControl.DoThreadSafeAsync(x =>
                    {
                        if (x.BorderStyle != BorderStyle.None)
                            x.BorderStyle = blnLightMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                    });
                    goto default;
                case Form frmControl:
                    await frmControl.DoThreadSafeAsync(x =>
                    {
                        if (x.MainMenuStrip != null)
                            foreach (ToolStripMenuItem tssItem in x.MainMenuStrip.Items)
                                ApplyColorsRecursively(tssItem, blnLightMode);
                    });
                    goto default;
                case TabControl objTabControl:
                    foreach (TabPage tabPage in await objTabControl.DoThreadSafeFuncAsync(x => x.TabPages))
                        await ApplyColorsRecursivelyAsync(tabPage, blnLightMode);
                    goto default;
                case ToolStrip tssStrip:
                    await tssStrip.DoThreadSafeAsync(x =>
                    {
                        foreach (ToolStripItem tssItem in x.Items)
                            ApplyColorsRecursively(tssItem, blnLightMode);
                    });
                    goto default;
                default:
                    await objControl.DoThreadSafeAsync(x =>
                    {
                        if (x.ForeColor == (blnLightMode ? ControlDark : ControlLight))
                            x.ForeColor = blnLightMode ? ControlLight : ControlDark;
                        else if (x.ForeColor == (blnLightMode ? ControlDarkerDark : ControlDarkerLight))
                            x.ForeColor = blnLightMode ? ControlDarkerLight : ControlDarkerDark;
                        else if (x.ForeColor == (blnLightMode ? ControlDarkestDark : ControlDarkestLight))
                            x.ForeColor = blnLightMode ? ControlDarkestLight : ControlDarkestDark;
                        else
                            x.ForeColor = blnLightMode ? ControlTextLight : ControlTextDark;
                        // These controls never have backgrounds set explicitly, so shouldn't have their backgrounds overwritten
                        if (!(x is Label || x is CheckBox || x is PictureBox || x is Button
                              || (x is Panel && !(x is SplitterPanel || x is TabPage))))
                        {
                            if (x.BackColor == (blnLightMode ? ControlLighterDark : ControlLighterLight))
                                x.BackColor = blnLightMode ? ControlLighterLight : ControlLighterDark;
                            else if (x.BackColor == (blnLightMode ? ControlLightestDark : ControlLightestLight))
                                x.BackColor = blnLightMode ? ControlLightestLight : ControlLightestDark;
                            else
                                x.BackColor = blnLightMode ? ControlLight : ControlDark;
                        }
                    });

                    break;
            }

            foreach (Control objChild in await objControl.DoThreadSafeFuncAsync(x => x.Controls))
                await ApplyColorsRecursivelyAsync(objChild, blnLightMode);
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
                Math.Max(Math.Min(Convert.ToInt32(Math.Round(dblRed * byte.MaxValue, MidpointRounding.AwayFromZero)) , byte.MaxValue), byte.MinValue),
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
