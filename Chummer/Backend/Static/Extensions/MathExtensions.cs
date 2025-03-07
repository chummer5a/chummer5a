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

namespace Chummer
{
    public static class MathExtensions
    {
        public static int Min(params int[] aintValues)
        {
            switch (aintValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(aintValues));
                case 1:
                    return aintValues[0];
            }

            int intReturn = aintValues[0];
            for (int i = 1; i < aintValues.Length - 1; ++i)
            {
                int intLoop = aintValues[i];
                if (intReturn > intLoop)
                    intReturn = intLoop;
            }
            return intReturn;
        }

        public static float Min(params float[] afltValues)
        {
            switch (afltValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(afltValues));
                case 1:
                    return afltValues[0];
            }

            float fltReturn = afltValues[0];
            for (int i = 1; i < afltValues.Length - 1; ++i)
            {
                float fltLoop = afltValues[i];
                if (fltReturn > fltLoop)
                    fltReturn = fltLoop;
            }
            return fltReturn;
        }

        public static double Min(params double[] adblValues)
        {
            switch (adblValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(adblValues));
                case 1:
                    return adblValues[0];
            }

            double dblReturn = adblValues[0];
            for (int i = 1; i < adblValues.Length - 1; ++i)
            {
                double dblLoop = adblValues[i];
                if (dblReturn > dblLoop)
                    dblReturn = dblLoop;
            }
            return dblReturn;
        }

        public static decimal Min(params decimal[] adecValues)
        {
            switch (adecValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(adecValues));
                case 1:
                    return adecValues[0];
            }

            decimal decReturn = adecValues[0];
            for (int i = 1; i < adecValues.Length - 1; ++i)
            {
                decimal decLoop = adecValues[i];
                if (decReturn > decLoop)
                    decReturn = decLoop;
            }
            return decReturn;
        }

        public static int Max(params int[] aintValues)
        {
            switch (aintValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(aintValues));
                case 1:
                    return aintValues[0];
            }

            int intReturn = aintValues[0];
            for (int i = 1; i < aintValues.Length - 1; ++i)
            {
                int intLoop = aintValues[i];
                if (intReturn < intLoop)
                    intReturn = intLoop;
            }
            return intReturn;
        }

        public static float Max(params float[] afltValues)
        {
            switch (afltValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(afltValues));
                case 1:
                    return afltValues[0];
            }

            float fltReturn = afltValues[0];
            for (int i = 1; i < afltValues.Length - 1; ++i)
            {
                float fltLoop = afltValues[i];
                if (fltReturn < fltLoop)
                    fltReturn = fltLoop;
            }
            return fltReturn;
        }

        public static double Max(params double[] adblValues)
        {
            switch (adblValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(adblValues));
                case 1:
                    return adblValues[0];
            }

            double dblReturn = adblValues[0];
            for (int i = 1; i < adblValues.Length - 1; ++i)
            {
                double dblLoop = adblValues[i];
                if (dblReturn < dblLoop)
                    dblReturn = dblLoop;
            }
            return dblReturn;
        }

        public static decimal Max(params decimal[] adecValues)
        {
            switch (adecValues.Length)
            {
                case 0:
                    throw new InvalidOperationException(nameof(adecValues));
                case 1:
                    return adecValues[0];
            }

            decimal decReturn = adecValues[0];
            for (int i = 1; i < adecValues.Length - 1; ++i)
            {
                decimal decLoop = adecValues[i];
                if (decReturn < decLoop)
                    decReturn = decLoop;
            }
            return decReturn;
        }

        /// <inheritdoc cref="Math.DivRem(int, int, out int)"/>
        public static int DivRem(this int a, int b, out int result)
        {
            // This version is faster than .NET Framework 4.8's implementation, but still slower than if the register storing the division remainder could be fetched directly
            int div = a / b;
            result = a - (div * b);
            return div;
        }

        /// <inheritdoc cref="Math.DivRem(int, int, out int)"/>
        public static Tuple<int, int> DivRem(this int a, int b)
        {
            // This version is faster than .NET Framework 4.8's implementation, but still slower than if the register storing the division remainder could be fetched directly
            int div = a / b;
            int result = a - (div * b);
            return new Tuple<int, int>(div, result);
        }

        /// <inheritdoc cref="Math.DivRem(long, long, out long)"/>
        public static long DivRem(this long a, long b, out long result)
        {
            // This version is faster than .NET Framework 4.8's implementation, but still slower than if the register storing the division remainder could be fetched directly
            long div = a / b;
            result = a - (div * b);
            return div;
        }

        /// <inheritdoc cref="Math.DivRem(long, long, out long)"/>
        public static Tuple<long, long> DivRem(this long a, long b)
        {
            // This version is faster than .NET Framework 4.8's implementation, but still slower than if the register storing the division remainder could be fetched directly
            long div = a / b;
            long result = a - (div * b);
            return new Tuple<long, long>(div, result);
        }

        /// <summary>
        /// Quick way to get the floor of the base-2 logarithm of an integer
        /// </summary>
        public static int FloorLog2(this int n)
        {
            if (n <= 0)
                throw new ArgumentOutOfRangeException(nameof(n));
            int num = 0;
            for (; n >= 65536; n /= 65536)
                num += 16;
            for (; n >= 256; n /= 256)
                num += 8;
            for (; n >= 16; n /= 16)
                num += 4;
            for (; n >= 2; n /= 2)
                ++num;
            return num;
        }

        /// <summary>
        /// Quick way to get the ceiling of the base-2 logarithm of an integer
        /// </summary>
        public static int CeilingLog2(this int n)
        {
            if (n <= 0)
                throw new ArgumentOutOfRangeException(nameof(n));
            return FloorLog2(n) + (n % 2);
        }

        /// <summary>
        /// Quick way to get the floor of the base-10 logarithm of an integer
        /// </summary>
        public static int FloorLog10(this int n)
        {
            if (n <= 0)
                throw new ArgumentOutOfRangeException(nameof(n));
            int num = 0;
            for (; n >= 10000; n /= 10000)
                num += 4;
            for (; n >= 10; n /= 10)
                ++num;
            return num;
        }

        /// <summary>
        /// Quick way to get the ceiling of the base-2 logarithm of an integer
        /// </summary>
        public static int CeilingLog10(this int n)
        {
            if (n <= 0)
                throw new ArgumentOutOfRangeException(nameof(n));
            int num = FloorLog10(n);
            if (n % 10 != 0)
                ++num;
            return num;
        }

        /// <inheritdoc cref="Math.Log(double, double)"/>
        public static decimal Log(this decimal d, decimal decBase)
        {
            decimal decReturn = Log2(d);
            if (decBase != 2.0m)
                decReturn /= Log2(decBase);
            return decReturn;
        }

        /// <inheritdoc cref="Math.Log10(double)"/>
        public static decimal Log10(this decimal d)
        {
            return Log2(d) / 3.32192809488736234787031942948939017586483139302458061205475639581593477660m; // Log_2(10) for transforming the base
        }

        /// <summary>
        /// A high-precision version of calculating a base-2 logarithm, used for an arbitrary precision number
        /// </summary>
        /// <param name="d">Num</param>
        /// <returns>The base-2 logarithm of <paramref name="d"/>, calculated as accurately as possible within decimal precision.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="d"/> is less than or equal to 0, which is outside the domain for a logarithm.</exception>
        private static decimal Log2(this decimal d)
        {
            if (d <= 0)
                throw new ArgumentOutOfRangeException(nameof(d));
            if (d == 1)
                return 0;
            bool blnNegate = d < 1;
            if (blnNegate)
                d = 1m / d;
            decimal decReturn = 0;
            // First handle the integer part
            for (; d >= 18446744073709551615m; d /= 18446744073709551615m)
                decReturn += 64;
            for (; d >= 4294967296m; d /= 4294967296m)
                decReturn += 32;
            for (; d >= 65536m; d /= 65536m)
                decReturn += 16;
            for (; d >= 256m; d /= 256m)
                decReturn += 8;
            for (; d >= 16m; d /= 16m)
                decReturn += 4;
            for (; d >= 2m; d /= 2m)
                ++decReturn;
            // d is now between 1 and 2, so the remaining part should be a fraction.
            if (d == 1)
                return blnNegate ? -decReturn : decReturn;
            decimal decReturnFraction = 0;
            // Handle this through a Taylor series of ln(x)/ln(2)
            // Start with ln(x)
            const int intUpperLimit = 75; // Decimal types have ca. 30 digits of precision, and this is enough terms to get < 10^-30 for the last term.
            // To help with precision, center around 1 if we are less than sqrt(2), around 2 if greater or equal. x = sqrt(2) should be where ln(x) is halfway between ln(1) and ln(2).
            if (d >= 1.41421356237309504880168872420969807856967187537694807317667973799073247846m)
            {
                // Taylor series if centered around 2 is: ln(2) + sum_k (-1/2)^(k+1) (x-2)^k / k
                ++decReturn; // The ln(2) part, but adding it directly to the output as 1 because we will be dividing the fraction part by ln(2) at the end anyway 
                for (int i = 1; i <= intUpperLimit; ++i)
                {
                    if (i % 2 == 1)
                        decReturnFraction += (d / 4m - 0.5m).RaiseToPower(i) / i;
                    else
                        decReturnFraction -= (d / 4m - 0.5m).RaiseToPower(i) / i;
                }
            }
            else
            {
                // Taylor series if centered around 1 is: sum_k (-1)^(k+1) * (x-1)^k / k
                for (int i = 1; i <= intUpperLimit; ++i)
                {
                    if (i % 2 == 1)
                        decReturnFraction += (d - 1m).RaiseToPower(i) / i;
                    else
                        decReturnFraction -= (d - 1m).RaiseToPower(i) / i;
                }
            }

            // divide by ln(2) to transform base from e to 2 for fraction part
            decReturn += decReturnFraction /
                         0.69314718055994530941723212145817656807550013436025525412068000949339362196m;
            return blnNegate ? -decReturn : decReturn;
        }
    }
}
