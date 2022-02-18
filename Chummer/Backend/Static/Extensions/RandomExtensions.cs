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
using System.Threading.Tasks;

namespace Chummer
{
    public static class RandomExtensions
    {
        /// <summary>
        /// Special version of NextModuloBiasRemoved(minValue, maxValue) built specifically for a 1D6 roll. The modulo bias to check is calculated at compile time, so the code should run faster.
        /// </summary>
        /// <param name="objRandom">Instance of Random to use.</param>
        public static int NextD6ModuloBiasRemoved(this Random objRandom)
        {
            if (objRandom == null)
                throw new ArgumentNullException(nameof(objRandom));
            const int intModuloCheck = int.MaxValue - 1;  // Faster Modulo bias removal for 1d6
            int intLoopResult;
            do
            {
                intLoopResult = objRandom.Next();
            }
            while (intLoopResult >= intModuloCheck);

            return 1 + intLoopResult % 6;
        }

        /// <summary>
        /// Wraps Random::Next(maxValue) around code that eliminates modulo bias (i.e. the fact that certain results will be more common based on the remainder when dividing int.MaxValue by them)
        /// </summary>
        /// <param name="objRandom">Instance of Random to use.</param>
        /// <param name="maxValue">Maximum value (exclusive) to generate.</param>
        public static int NextModuloBiasRemoved(this Random objRandom, int maxValue)
        {
            if (objRandom == null)
                throw new ArgumentNullException(nameof(objRandom));
            int intModuloCheck = int.MaxValue - int.MaxValue % maxValue;
            int intLoopResult;
            do
            {
                intLoopResult = objRandom.Next();
            }
            while (intLoopResult >= intModuloCheck);

            return intLoopResult % maxValue;
        }

        /// <summary>
        /// Special version of NextModuloBiasRemoved(minValue, maxValue) built specifically for a 1D6 roll. The modulo bias to check is calculated at compile time, so the code should run faster.
        /// </summary>
        /// <param name="objRandom">Instance of Random to use.</param>
        public static async ValueTask<int> NextD6ModuloBiasRemovedAsync(this Random objRandom)
        {
            if (objRandom == null)
                throw new ArgumentNullException(nameof(objRandom));
            const int intModuloCheck = int.MaxValue - 1;  // Faster Modulo bias removal for 1d6
            int intLoopResult = 0;
            await Task.Run(() =>
            {
                do
                {
                    intLoopResult = objRandom.Next();
                } while (intLoopResult >= intModuloCheck);
            });

            return 1 + intLoopResult % 6;
        }

        /// <summary>
        /// Wraps Random::Next(maxValue) around code that eliminates modulo bias (i.e. the fact that certain results will be more common based on the remainder when dividing int.MaxValue by them)
        /// </summary>
        /// <param name="objRandom">Instance of Random to use.</param>
        /// <param name="maxValue">Maximum value (exclusive) to generate.</param>
        public static async ValueTask<int> NextModuloBiasRemovedAsync(this Random objRandom, int maxValue)
        {
            if (objRandom == null)
                throw new ArgumentNullException(nameof(objRandom));
            int intModuloCheck = int.MaxValue - int.MaxValue % maxValue;
            int intLoopResult = 0;
            await Task.Run(() =>
            {
                do
                {
                    intLoopResult = objRandom.Next();
                } while (intLoopResult >= intModuloCheck);
            });

            return intLoopResult % maxValue;
        }

        /// <summary>
        /// Wraps Random::Next(minValue, maxValue) around code that eliminates modulo bias (i.e. the fact that certain results will be more common based on the remainder when dividing int.MaxValue by them)
        /// </summary>
        /// <param name="objRandom">Instance of Random to use.</param>
        /// <param name="minValue">Minimum value (inclusive) to generate.</param>
        /// <param name="maxValue">Maximum value (exclusive) to generate.</param>
        public static int NextModuloBiasRemoved(this Random objRandom, int minValue, int maxValue)
        {
            return objRandom.NextModuloBiasRemoved(maxValue - minValue) + minValue;
        }
    }
}
