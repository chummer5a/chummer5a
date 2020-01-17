using System;
using System.Drawing;
using System.Windows.Forms;
using Chummer.Backend.Datastructures;

namespace Chummer.Backend
{
    public static class ColorUtilities
    {
        //Class written based on http://www.brucelindbloom.com/index.html
        /// <summary>
        /// Correctly interpolates between 2 colors
        /// </summary>
        /// <param name="color1">A 32bit int containing 8bit alpha, 8 bit red, 8 bit green and 8 bit blue, in that order</param>
        /// <param name="color2">A 32bit int containing 8bit alpha, 8 bit red, 8 bit green and 8 bit blue, in that order</param>
        /// <param name="scale">A value between 0.0 and 1.0 (inclusive) detailing how much of color1 and color2 should be in the final result</param>
        /// <returns>A correctly interpolated color encoded as a 32bit int containing 8bit alpha, 8 bit red, 8 bit green and 8 bit blue, in that order</returns>
        /// <exception cref="NotImplementedException"></exception>
        public static int ARGBIntLABInterpolate(int color1, int color2, float scale)
        {
            ScaleSanityCheck(scale);
            Vector4 argb1 = ToColorFractions(color1);
            Vector4 argb2 = ToColorFractions(color2);

            Vector3 xyz1 = RGBToXYZ(argb1.AsVector3);
            Vector3 xyz2 = RGBToXYZ(argb2.AsVector3);

            Vector3 lab1 = XYZToLAB(xyz1);
            Vector3 lab2 = XYZToLAB(xyz2);

            double newalpha = Math.Max(argb1.W,argb2.W);

            double realscale = CalculateRealScale(argb1.W, argb2.W, scale);

            Vector3 lab3 = new Vector3(
                lab1.X * (1 - realscale) + lab2.X * realscale,
                lab1.Y * (1 - realscale) + lab2.Y * realscale,
                lab1.Z * (1 - realscale) + lab2.Z * realscale
            );


            Vector3 xyz = LABToXYZ(lab3);

            Vector3 rgb = XYZToRGB(xyz);

            rgb = new Vector3(
                Math.Max(0, Math.Min(1, rgb.X)),
                Math.Max(0, Math.Min(1, rgb.Y)),
                Math.Max(0, Math.Min(1, rgb.Z))
            );

            int final = (int)(newalpha * 255);
            return (final << 24) | ((int)(rgb.X * 255) << 16) | ((int)(rgb.Y * 255) << 8) | ((int)(rgb.Z * 255));


        }

        private static void ScaleSanityCheck(float scale)
        {
            if(scale > 1) throw new ArgumentOutOfRangeException(nameof(scale));
            if(scale < 0) throw new ArgumentOutOfRangeException(nameof(scale));
        }

        public static int ARGBIntXYZInterpolate(int color1, int color2, float scale)
        {
            ScaleSanityCheck(scale);

            Vector4 argb1 = ToColorFractions(color1);
            Vector4 argb2 = ToColorFractions(color2);

            Vector3 xyz1 = RGBToXYZ(argb1.AsVector3);
            Vector3 xyz2 = RGBToXYZ(argb2.AsVector3);

            double newalpha = Math.Max(argb1.W,argb2.W);

            double realscale = CalculateRealScale(argb1.W, argb2.W, scale);

            Vector3 xyz = new Vector3(
                xyz1.X * (1 - realscale) + xyz2.X * realscale,
                xyz1.Y * (1 - realscale) + xyz2.Y * realscale, 
                xyz1.Z * (1 - realscale) + xyz2.Z * realscale
            );
            
            Vector3 rgb = XYZToRGB(xyz);

            rgb = new Vector3(
                Math.Max(0, Math.Min(1, rgb.X)),
                Math.Max(0, Math.Min(1, rgb.Y)),
                Math.Max(0, Math.Min(1, rgb.Z))
            );

            int final = (int)(newalpha * 255);
            return (final << 24) | ((int)(rgb.X * 255) << 16) | ((int)(rgb.Y * 255) << 8) | ((int)(rgb.Z * 255));
        }

        /// <summary>
        /// Performs alpha blending between 2 colors
        /// </summary>
        /// <param name="overlay">The color to be drawn above back</param>
        /// <param name="back">The background color</param>
        /// <returns></returns>
        public static int ARGBIntXYZAlphaBlend(int overlay, int back)
        {
            Vector4 overlayargb = ToColorFractions(overlay);
            Vector4 backargb = ToColorFractions(back);

            

            Vector3 overlayxyz = RGBToXYZ(overlayargb.AsVector3);
            Vector3 baclxyz = RGBToXYZ(backargb.AsVector3);

            //Calculate the new alpha value. Calculation is done with a multiplication of the alpha values, but instead of multiplying the alpha values
            //the "reverse" alpha is multiplied to find the new "reverse" alpha. Reverse alpha is the opposite of alpha 1 - a = r a
            double newalpha = 1 - ((1 - overlayargb.W) * (1 - backargb.W));

            double overlayfraction = overlayargb.W / newalpha;

            Vector3 xyz = new Vector3(
                (overlayxyz.X * overlayfraction) + (baclxyz.X * (1 - overlayfraction)),
                (overlayxyz.Y * overlayfraction) + (baclxyz.Y * (1 - overlayfraction)),
                (overlayxyz.Z * overlayfraction) + (baclxyz.Z * (1 - overlayfraction))
            );

            Vector3 rgb = XYZToRGB(xyz);

            rgb = new Vector3(
                Math.Max(0, Math.Min(1, rgb.X)),
                Math.Max(0, Math.Min(1, rgb.Y)),
                Math.Max(0, Math.Min(1, rgb.Z))
            );

            
            int final = (int)(newalpha * 255);
            return (final << 24) | ((int)(rgb.X * 255) << 16) | ((int)(rgb.Y * 255) << 8) | ((int)(rgb.Z * 255));
        }

        private static double CalculateRealScale(double alpha1, double alpha2, float scale)
        {
            double mod1 = alpha1 * (1-scale);
            double mod2 = alpha2 * scale;
            double realscale = mod1 / (mod1 + mod2);

            return realscale;
        }

        private const double epsilon = 216.0 / 24389.0;
        private const double kappa = 24389.0 / 27.0;
        private static readonly Vector3 XYZreference = RGBToXYZ(new Vector3(1,1,1));

        internal static Vector3 XYZToLAB(Vector3 XYZ)
        {

            Vector3 xyz = new Vector3(
                XYZ.X / XYZreference.X,
                XYZ.Y / XYZreference.Y, 
                XYZ.Z / XYZreference.Z);
            
            Vector3 f = new Vector3(
                xyz.X > epsilon ? Math.Pow(xyz.X, 1.0 / 3.0) : (kappa * xyz.X + 16.0) / 116,
                xyz.Y > epsilon ? Math.Pow(xyz.Y, 1.0 / 3.0) : (kappa * xyz.Y + 16.0) / 116,
                xyz.Z > epsilon ? Math.Pow(xyz.Z, 1.0 / 3.0) : (kappa * xyz.Z + 16.0) / 116
                );

            return new Vector3(
                200.0 * (f.Y - f.X), 
                500.0 * (f.Z - f.Y), 
                116.0 * f.Y - 16);
            
        }

        internal static Vector3 LABToXYZ(Vector3 Lab)
        {
            Vector3 xyz = new Vector3();

            double f_1 = (Lab.Z + 16) / 116;
            double f_0 = f_1 - (Lab.X / 200);
            double f_2 = (Lab.Y / 500) + f_1;

            if (Math.Pow(f_2, 3) > epsilon)
            {
                xyz.Z = Math.Pow(f_2, 3);
            }
            else
            {
                xyz.Z = (116*f_2-16) /kappa;
            }

            if (Lab.Z > kappa * epsilon)
            {
                xyz.Y = Math.Pow((Lab.Z + 16) / 116, 3);
            }
            else
            {
                xyz.Y = Lab.Z / kappa;
            }

            if (Math.Pow(f_0, 3) > epsilon)
            {
                xyz.X = Math.Pow(f_0, 3);
            }
            else
            {
                xyz.X = (116 * f_0 - 16) / kappa;
            }

            return new Vector3(
                xyz.X * XYZreference.X,
                xyz.Y * XYZreference.Y,
                xyz.Z * XYZreference.Z);
        }

        internal static Vector3 XYZToRGB(Vector3 input)
        {
            /*
1     3.2404548360214087	-1.537138850102575	-0.4985315468684809
2	-0.9692663898756537	1.876010928842491	0.04155608234667351
3	0.055643419604213644	-0.20402585426769815	1.0572251624579287
*/
            Vector3 v = new Vector3(
                3.2404548360214087 * input.X + -1.537138850102575 * input.Y + -0.4985315468684809 * input.Z,
                -0.9692663898756537 * input.X + 1.876010928842491 * input.Y + 0.04155608234667351 * input.Z,
                0.055643419604213644 * input.X + -0.20402585426769815 * input.Y + 1.0572251624579287 * input.Z
            );
            

            Vector3 V = new Vector3(
                v.X <= 0.00313080 ? 12.92 * v.X : 1.055 * Math.Pow(v.X, 1 / 2.4) - 0.055,
                v.Y <= 0.00313080 ? 12.92 * v.Y : 1.055 * Math.Pow(v.Y, 1 / 2.4) - 0.055,
                v.Z <= 0.00313080 ? 12.92 * v.Z : 1.055 * Math.Pow(v.Z, 1 / 2.4) - 0.055);

            return V;
        }

        internal static Vector3 RGBToXYZ(Vector3 input)
        {
            Vector3 v = new Vector3(
                input.X > 0.04045 ? Math.Pow((input.X + 0.055) / 1.055, 2.4) : input.X / 12.92,
                input.Y > 0.04045 ? Math.Pow((input.Y + 0.055) / 1.055, 2.4) : input.Y / 12.92,
                input.Z > 0.04045 ? Math.Pow((input.Z + 0.055) / 1.055, 2.4) : input.Z / 12.92);

            /* [X;Y;Z] = M*v
            *
            *    0.4124564  0.3575761  0.1804375
            * M= 0.2126729  0.7151522  0.0721750
            *    0.0193339  0.1191920  0.9503041
            * [2;1;0]
            */

            //Do the matrix multiplication. As we don't have any matrix math package included, here it is raw

            double x = v.X * 0.4124564 + v.Y * 0.3575761 + v.Z * 0.1804375;
            double y = v.X * 0.2126729 + v.Y * 0.7151522 + v.Z * 0.0721750;
            double z = v.X * 0.0193339 + v.Y * 0.1191920 + v.Z * 0.9503041;

            return new Vector3(x,y,z);
        }

        internal static Vector4 ToColorFractions(int color)
        {
            Vector4 result = new Vector4();
            
            result.Z = (color & 0xff) / 255.0;
            color >>= 8;

            result.Y = (color & 0xff) / 255.0;
            color >>= 8;

            result.X = (color & 0xff) / 255.0;
            color >>= 8;

            result.W = (color & 0xff) / 255.0;

            return result;
        }

        private static Random random = new Random();
        public static Color RandomColor()
        {
            return System.Drawing.Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
        }

        public static Color RandomLightColor()
        {
            return System.Drawing.Color.FromArgb(random.Next(127,255), random.Next(127,255), random.Next(127,255));
        }
    }
}