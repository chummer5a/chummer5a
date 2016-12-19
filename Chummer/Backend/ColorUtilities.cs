using System;
using System.Windows.Forms;

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
            double[] argb1 = ToColorFractions(color1);
            double[] argb2 = ToColorFractions(color2);

            double[] xyz1 = RGBToXYZ(argb1);
            double[] xyz2 = RGBToXYZ(argb2);

            double[] lab1 = XYZToLAB(xyz1);
            double[] lab2 = XYZToLAB(xyz2);

            double newalpha = Math.Max(argb1[3],argb2[3]);

            double realscale = CalculateRealScale(argb1[3], argb2[3], scale);

            double[] lab3 = new double[3];
            for (int i = 0; i < 3; i++)
            {
                lab3[i] = lab1[i] * (1 - realscale) + lab2[i] * realscale;
            }

            double[] xyz = LABToXYZ(lab3);

            double[] rgb = XYZToRGB(xyz);

            for (int i = 0; i < 3; i++)
            {
                rgb[i] = Math.Max(0, Math.Min(1, rgb[i]));
            }

            int final = (int)(newalpha * 255);
            return (final << 24) | ((int)(rgb[0] * 255) << 16) | ((int)(rgb[1] * 255) << 8) | ((int)(rgb[2] * 255));

                unchecked
            {

                for (int i = 0; i < 3; i++)
                {
                    final <<= 8;
                    final |= (int)(rgb[2-i] * 255);
                }
                return final;
            }
        }

        private static void ScaleSanityCheck(float scale)
        {
            if(scale > 1) throw new ArgumentOutOfRangeException(nameof(scale));
            if(scale < 0) throw new ArgumentOutOfRangeException(nameof(scale));
        }

        public static int ARGBIntXYZInterpolate(int color1, int color2, float scale)
        {
            ScaleSanityCheck(scale);

            double[] argb1 = ToColorFractions(color1);
            double[] argb2 = ToColorFractions(color2);

            double[] xyz1 = RGBToXYZ(argb1);
            double[] xyz2 = RGBToXYZ(argb2);

            double newalpha = Math.Max(argb1[3],argb2[3]);

            double realscale = CalculateRealScale(argb1[3], argb2[3], scale);

            double[] xyz = new double[3];
            for (int i = 0; i < 3; i++)
            {
                xyz[i] = xyz1[i] * (1 - realscale) + xyz2[i] * realscale;
            }

            double[] rgb = XYZToRGB(xyz);

            for (int i = 0; i < 3; i++)
            {
                rgb[i] = Math.Max(0, Math.Min(1, rgb[i]));
            }

            int final = (int)(newalpha * 255);
            return (final << 24) | ((int)(rgb[0] * 255) << 16) | ((int)(rgb[1] * 255) << 8) | ((int)(rgb[2] * 255));
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
        private static readonly double[] XYZreference = RGBToXYZ(new[] {1.0, 1.0, 1.0});

        internal static double[] XYZToLAB(double[] XYZ)
        {

            double[] xyz = new double[3];
            double[] f= new double[3];
            double[] Lab = new double[3];

            for (int i = 0; i < 3; i++)
            {
                xyz[i] = XYZ[i] / XYZreference[i];
            }

            for (int i = 0; i < 3; i++)
            {
                if (xyz[i] > epsilon)
                {
                    //Cube root
                    f[i] = Math.Pow(xyz[i], 1.0 / 3.0);
                }
                else
                {
                    f[i] = (kappa * xyz[i] + 16.0) / 116;
                }
            }

            Lab[2] = 116.0 * f[1] - 16;
            Lab[1] = 500.0 * (f[2] - f[1]);
            Lab[0] = 200.0 * (f[1] - f[0]);
            return Lab;
        }

        internal static double[] LABToXYZ(double[] Lab)
        {
            double[] f = new double[3];
            double[] xyz = new double[3];
            double[] XYZ = new double[3];

            f[1] = (Lab[2] + 16) / 116;
            f[0] = f[1] - (Lab[0] / 200);
            f[2] = (Lab[1] / 500) + f[1];

            if (Math.Pow(f[2], 3) > epsilon)
            {
                xyz[2] = Math.Pow(f[2], 3);
            }
            else
            {
                xyz[2] = (116*f[2]-16) /kappa;
            }

            if (Lab[2] > kappa * epsilon)
            {
                xyz[1] = Math.Pow((Lab[2] + 16) / 116, 3);
            }
            else
            {
                xyz[1] = Lab[2] / kappa;
            }

            if (Math.Pow(f[0], 3) > epsilon)
            {
                xyz[0] = Math.Pow(f[0], 3);
            }
            else
            {
                xyz[0] = (116 * f[0] - 16) / kappa;
            }

            for (int i = 0; i < 3; i++)
            {
                XYZ[i] = xyz[i] * XYZreference[i];
            }

            return XYZ;
        }

        internal static double[] XYZToRGB(double[] input)
        {
            /*
1     3.2404548360214087	-1.537138850102575	-0.4985315468684809
2	-0.9692663898756537	1.876010928842491	0.04155608234667351
3	0.055643419604213644	-0.20402585426769815	1.0572251624579287
*/
            double[] v = new double[3];

            v[0] = 3.2404548360214087 * input[0] + -1.537138850102575 * input[1] + -0.4985315468684809 * input[2];
            v[1] = -0.9692663898756537 * input[0] + 1.876010928842491 * input[1] + 0.04155608234667351 * input[2];
            v[2] = 0.055643419604213644 * input[0] + -0.20402585426769815 * input[1] + 1.0572251624579287 * input[2];

            double[] V = new double[3];

            for (int i = 0; i < 3; i++)
            {
                if (v[i] <= 0.00313080)
                {
                    V[i] = 12.92 * v[i];
                }
                else
                {
                    V[i] = 1.055 * Math.Pow(v[i], 1 / 2.4) - 0.055;
                }


            }

            return V;
        }

        internal static double[] RGBToXYZ(double[] input)
        {
            double[] result = new double[3];
            double[] v = new double[3];
            int i = 0;
            for (; i < 3; i++)
            {
                double V = input[i];

                v[i] = V > 0.04045 ? Math.Pow((V + 0.055) / 1.055, 2.4) : V / 12.92;
            }

            /* [X;Y;Z] = M*v
            *
            *    0.4124564  0.3575761  0.1804375
            * M= 0.2126729  0.7151522  0.0721750
            *    0.0193339  0.1191920  0.9503041
            * [2;1;0]
            */

            //Do the matrix multiplication. As we don't have any matrix math package included, here it is raw

            result[0] = v[0] * 0.4124564 + v[1] * 0.3575761 + v[2] * 0.1804375;
            result[1] = v[0] * 0.2126729 + v[1] * 0.7151522 + v[2] * 0.0721750;
            result[2] = v[0] * 0.0193339 + v[1] * 0.1191920 + v[2] * 0.9503041;

            return result;
        }

        internal static double[] ToColorFractions(int color)
        {
            double[] result = new double[4];
            for (int i = 0; i < 4; i++)
            {
                result[i] = (color & 0xff) / 255.0;
                color >>= 8;
            }
            //Now we  have
            double temp = result[0];
            result[0] = result[2];
            result[2] = temp;

            return result;
        }


    }
}