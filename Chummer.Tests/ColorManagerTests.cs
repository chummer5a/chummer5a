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
using System.Diagnostics;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class ColorManagerTests
    {
        [TestMethod]
        public void DarkModeIsDark()
        {
            try
            {
                Color objColorLightGrayInDarkMode = ColorManager.GenerateDarkModeColor(Color.LightGray);
                float fltLightGrayLightness = Color.LightGray.GetBrightness();
                float fltLightGrayDarkModeLightness = objColorLightGrayInDarkMode.GetBrightness();
                Assert.IsGreaterThan(fltLightGrayDarkModeLightness, fltLightGrayLightness);
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
                throw ex;
            }
        }

        [TestMethod]
        public void DarkModeColorGenerationIsConsistent()
        {
            try
            {
                Color objColorBlackInvert = ColorManager.GenerateInverseDarkModeColor(Color.Black);
                Color objColorBlackInvertDark = ColorManager.GenerateDarkModeColor(objColorBlackInvert);
                float fltBlackHue = Color.Black.GetHue();
                float fltBlackInvertDarkHue = objColorBlackInvertDark.GetHue();
                Assert.AreEqual(fltBlackInvertDarkHue, fltBlackHue, 0.1f / 360.0f); // Only care if we're off by more than 0.1 degrees
                Color objColorBlackInvertDarkInvert = ColorManager.GenerateInverseDarkModeColor(objColorBlackInvertDark);
                Color objColorBlackInvertDarkInvertDark = ColorManager.GenerateDarkModeColor(objColorBlackInvertDarkInvert);
                Assert.IsTrue(objColorBlackInvertDark == objColorBlackInvertDarkInvertDark);

                Color objColorWhiteInvert = ColorManager.GenerateInverseDarkModeColor(Color.White);
                Color objColorWhiteInvertDark = ColorManager.GenerateDarkModeColor(objColorWhiteInvert);
                float fltWhiteHue = Color.White.GetHue();
                float fltWhiteInvertDarkHue = objColorWhiteInvertDark.GetHue();
                Assert.AreEqual(fltWhiteInvertDarkHue, fltWhiteHue, 0.1f / 360.0f); // Only care if we're off by more than 0.1 degrees
                Color objColorWhiteInvertDarkInvert = ColorManager.GenerateInverseDarkModeColor(objColorWhiteInvertDark);
                Color objColorWhiteInvertDarkInvertDark = ColorManager.GenerateDarkModeColor(objColorWhiteInvertDarkInvert);
                Assert.IsTrue(objColorWhiteInvertDark == objColorWhiteInvertDarkInvertDark);

                Color objColorRedInvert = ColorManager.GenerateInverseDarkModeColor(Color.Red);
                Color objColorRedInvertDark = ColorManager.GenerateDarkModeColor(objColorRedInvert);
                float fltRedHue = Color.Red.GetHue();
                float fltRedInvertDarkHue = objColorRedInvertDark.GetHue();
                Assert.AreEqual(fltRedInvertDarkHue, fltRedHue, 0.1f / 360.0f); // Only care if we're off by more than 0.1 degrees
                Color objColorRedInvertDarkInvert = ColorManager.GenerateInverseDarkModeColor(objColorRedInvertDark);
                Color objColorRedInvertDarkInvertDark = ColorManager.GenerateDarkModeColor(objColorRedInvertDarkInvert);
                Assert.IsTrue(objColorRedInvertDark == objColorRedInvertDarkInvertDark);

                Color objColorChocolateInvert = ColorManager.GenerateInverseDarkModeColor(Color.Chocolate);
                Color objColorChocolateInvertDark = ColorManager.GenerateDarkModeColor(objColorChocolateInvert);
                float fltChocolateHue = Color.Chocolate.GetHue();
                float fltChocolateInvertDarkHue = objColorChocolateInvertDark.GetHue();
                Assert.AreEqual(fltChocolateInvertDarkHue, fltChocolateHue, 0.1f / 360.0f); // Only care if we're off by more than 0.1 degrees
                Color objColorChocolateInvertDarkInvert = ColorManager.GenerateInverseDarkModeColor(objColorChocolateInvertDark);
                Color objColorChocolateInvertDarkInvertDark = ColorManager.GenerateDarkModeColor(objColorChocolateInvertDarkInvert);
                Assert.IsTrue(objColorChocolateInvertDark == objColorChocolateInvertDarkInvertDark);
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
                throw ex;
            }
        }
    }
}
