using System;
using System.Globalization;
using NUnit.Framework;
using Chummer.Backend;
using Chummer.Backend.Datastructures;

namespace Chummer.Test
{
    [TestFixture]
    public class ColorUtilitiesTest
    {
        private const string RGB = "RGB";

        private static void XYZBackTest(Vector3 rgb)
        {
            Vector3 rgb2 = ColorUtilities.XYZToRGB(ColorUtilities.RGBToXYZ(rgb));

            TestContext.Write("R");
            Assert.AreEqual(rgb.X, rgb2.X, 0.000001);

            TestContext.Write("G");
            Assert.AreEqual(rgb.Y, rgb2.Y, 0.000001);

            TestContext.Write("B");
            Assert.AreEqual(rgb.Z, rgb2.Z, 0.000001);

        }

        private static void RBGLabBackTest(Vector3 rgb)
        {

            var rgb2 =
                ColorUtilities.XYZToRGB(ColorUtilities.LABToXYZ(ColorUtilities.XYZToLAB(ColorUtilities.RGBToXYZ(rgb))));

            TestUtils.AreMostlyEqual(rgb, rgb2, 0.01);
        }

        private static Vector3 FromHex(string hex)
        {
            return new Vector3(
                int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber) / 255.0,
                int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber) / 255.0,
                int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber) / 255.0
            );
        }

        [Test]
        public void RBGXYZBackZero()
        {
            XYZBackTest(new Vector3(0,0,0));
        }

        [Test]
        public void RBGXYZBackQuarter()
        {
            XYZBackTest(new Vector3(0.25, 0.25, 0.25));
        }

        [Test]
        public void RBGXYZBackHalf()
        {
            XYZBackTest(new Vector3(0.5, 0.5, 0.5));
        }


        [Test]
        public void RBGXYZBackOne()
        {
            XYZBackTest(new Vector3(1, 1, 1));
        }

        [Test]
        [TestCase("8B636C")]
        [TestCase("4B0082")]
        [TestCase("0000FF")]
        [TestCase("4682B4")]
        [TestCase("00C5CD")]
        [TestCase("F5FFFA")]
        [TestCase("00C957")]
        [TestCase("00FFFF")]
        [TestCase("00FF00")]
        public void RGBXYZColors(string hex)
        {
            XYZBackTest(FromHex(hex));
        }

        [Test]
        [TestCase("8B636C")]
        [TestCase("4B0082")]
        [TestCase("0000FF")]
        [TestCase("4682B4")]
        [TestCase("00C5CD")]
        [TestCase("F5FFFA")]
        [TestCase("00C957")]
        [TestCase("00FFFF")]
        [TestCase("00FF00")]
        public void RGBLabColors(string hex)
        {
            RBGLabBackTest(FromHex(hex));
        }

        [Test]
        public void RGBToXYZGreen()
        {
            var xyz = ColorUtilities.RGBToXYZ(new Vector3(0, 1, 0));
            TestUtils.AreMostlyEqual(new Vector3(.3576, .7152, .1192), xyz, 0.01);
        }
        [Test]
        public void RGBToXYZPink()
        {
            var xyz = ColorUtilities.RGBToXYZ(new Vector3(1, .75294, .79608));
            TestUtils.AreMostlyEqual(new Vector3(.70869, .63271, .64977), xyz, 0.01);
        }
        [Test]
        public void XYZToRGBGreen()
        {
            var rgb = ColorUtilities.XYZToRGB(new Vector3(.3576, .7152, .1192));
            TestUtils.AreMostlyEqual(new Vector3(0, 1, 0), rgb, 0.01);
        }

        [Test]
        public void XYZToRGBPink()
        {
            var rgb = ColorUtilities.XYZToRGB(new Vector3(.70869, .63271, .64977));
            TestUtils.AreMostlyEqual(new Vector3(1, .75294, .79608), rgb, 0.01);

        }

        [Test]
        public void OneColorFraction()
        {
            TestUtils.AreMostlyEqual(new Vector4(1,1,1,1), ColorUtilities.ToColorFractions(-1), 0.001);
        }

        [Test]
        public void FullAlphaHalfColorFraction()
        {
            uint a = 0xff808080;
            TestUtils.AreMostlyEqual(new Vector4(0.5,0.5,0.5,1), ColorUtilities.ToColorFractions(unchecked((int)a)), 0.01);
        }
        [Test]
        public void HalfColorFraction()
        {
            TestUtils.AreMostlyEqual(new Vector4(0.5,0.5,0.5,0.5), ColorUtilities.ToColorFractions(int.MinValue + 8421504), 0.01);
        }

    }
}
