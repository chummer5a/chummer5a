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
    internal static class MathExtensions
    {
        public static int Min(params int[] aintValues)
        {
            if (aintValues.Length == 0)
                throw new ArgumentException(nameof(aintValues));
            if (aintValues.Length == 1)
                return aintValues[0];
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
            if (afltValues.Length == 0)
                throw new ArgumentException(nameof(afltValues));
            if (afltValues.Length == 1)
                return afltValues[0];
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
            if (adblValues.Length == 0)
                throw new ArgumentException(nameof(adblValues));
            if (adblValues.Length == 1)
                return adblValues[0];
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
            if (adecValues.Length == 0)
                throw new ArgumentException(nameof(adecValues));
            if (adecValues.Length == 1)
                return adecValues[0];
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
            if (aintValues.Length == 0)
                throw new ArgumentException(nameof(aintValues));
            if (aintValues.Length == 1)
                return aintValues[0];
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
            if (afltValues.Length == 0)
                throw new ArgumentException(nameof(afltValues));
            if (afltValues.Length == 1)
                return afltValues[0];
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
            if (adblValues.Length == 0)
                throw new ArgumentException(nameof(adblValues));
            if (adblValues.Length == 1)
                return adblValues[0];
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
            if (adecValues.Length == 0)
                throw new ArgumentException(nameof(adecValues));
            if (adecValues.Length == 1)
                return adecValues[0];
            decimal decReturn = adecValues[0];
            for (int i = 1; i < adecValues.Length - 1; ++i)
            {
                decimal decLoop = adecValues[i];
                if (decReturn < decLoop)
                    decReturn = decLoop;
            }
            return decReturn;
        }
    }
}
