using System.Drawing;
using Xunit.Sdk;

namespace Chummer.Tests;

public class ColorManagerTests
{
    [Fact]
    public void Should_Generate_Darker_Color_For_Dark_Mode()
    {
        var lightGray = Color.LightGray;
        var darkerVersionInDarkMode = ColorManager.GenerateDarkModeColor(lightGray);

        var originalBrightness = lightGray.GetBrightness();
        var darkerBrightness = darkerVersionInDarkMode.GetBrightness();

        Assert.True(darkerBrightness < originalBrightness);
    }


    [Theory]
    [InlineData( nameof(Color.Red))]
    [InlineData( nameof(Color.Chocolate))]
    [InlineData(nameof(Color.Gray))]
    public void Validate_DarkMode_Color_Generation_And_Inversion(string colorName)
    {
        var color = (Color)(typeof(Color).GetProperty(colorName)?.GetValue(null) ?? throw new XunitException("Color could not be fetched from the name"));

        var colorInvert = ColorManager.GenerateInverseDarkModeColor(color);
        var colorInvertDark = ColorManager.GenerateDarkModeColor(colorInvert);
        var colorHue = color.GetHue();
        var colorInvertDarkHue = colorInvertDark.GetHue();
        Assert.True(Math.Abs(colorInvertDarkHue - colorHue) < 0.1f / 360.0f);

        var colorInvertDarkInvert = ColorManager.GenerateInverseDarkModeColor(colorInvertDark);
        var colorInvertDarkInvertDark = ColorManager.GenerateDarkModeColor(colorInvertDarkInvert);
        Assert.True(colorInvertDark == colorInvertDarkInvertDark);
    }
}
