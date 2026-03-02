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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public static class RandomExtensions
    {
        /// <summary>
        /// Special version of <see cref="NextModuloBiasRemoved(Random, int, int)"/> built specifically for a 1D6 roll. The modulo bias to check is calculated at compile time, so the code should run faster.
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
        /// Wraps <see cref="Random.Next(int)"/> around code that eliminates modulo bias (i.e. the fact that certain results will be more common based on the remainder when dividing <see cref="int.MaxValue"/> by them)
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
        /// Wraps <see cref="Random.Next(int, int)"/> around code that eliminates modulo bias (i.e. the fact that certain results will be more common based on the remainder when dividing <see cref="int.MaxValue"/> by them)
        /// </summary>
        /// <param name="objRandom">Instance of Random to use.</param>
        /// <param name="minValue">Minimum value (inclusive) to generate.</param>
        /// <param name="maxValue">Maximum value (exclusive) to generate.</param>
        public static int NextModuloBiasRemoved(this Random objRandom, int minValue, int maxValue)
        {
            return objRandom.NextModuloBiasRemoved(maxValue - minValue) + minValue;
        }

        /// <summary>
        /// Special version of <see cref="NextModuloBiasRemovedAsync(Random, int, CancellationToken)"/> built specifically for a 1D6 roll. The modulo bias to check is calculated at compile time, so the code should run faster.
        /// </summary>
        /// <param name="objRandom">Instance of Random to use.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task<int> NextD6ModuloBiasRemovedAsync(this Random objRandom, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<int>(token);
            if (objRandom == null)
                return Task.FromException<int>(new ArgumentNullException(nameof(objRandom)));
            const int intModuloCheck = int.MaxValue - 1;  // Faster Modulo bias removal for 1d6
            return DoLoop();
            async Task<int> DoLoop()
            {
                int intReturn;
                if (objRandom is ThreadSafeRandom objThreadSafeRandom)
                {
                    do
                    {
                        intReturn = await objThreadSafeRandom.NextAsync(token).ConfigureAwait(false);
                    } while (intReturn >= intModuloCheck);
                }
                else
                {
                    do
                    {
                        intReturn = objRandom.Next();
                    } while (intReturn >= intModuloCheck);
                }

                return 1 + intReturn % 6;
            }
        }

        /// <summary>
        /// Async version of <see cref="Random.Next(int)"/> with a wrapper that eliminates modulo bias (i.e. the fact that certain results will be more common based on the remainder when dividing <see cref="int.MaxValue"/> by them)
        /// </summary>
        /// <param name="objRandom">Instance of Random to use.</param>
        /// <param name="maxValue">Maximum value (exclusive) to generate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task<int> NextModuloBiasRemovedAsync(this Random objRandom, int maxValue, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<int>(token);
            if (objRandom == null)
                return Task.FromException<int>(new ArgumentNullException(nameof(objRandom)));
            int intModuloCheck = int.MaxValue - int.MaxValue % maxValue;
            return DoLoop();
            async Task<int> DoLoop()
            {
                int intReturn;
                if (objRandom is ThreadSafeRandom objThreadSafeRandom)
                {
                    do
                    {
                        intReturn = await objThreadSafeRandom.NextAsync(token).ConfigureAwait(false);
                    } while (intReturn >= intModuloCheck);
                }
                else
                {
                    do
                    {
                        intReturn = objRandom.Next();
                    } while (intReturn >= intModuloCheck);
                }

                return intReturn % maxValue;
            }
        }

        /// <summary>
        /// Async version of <see cref="Random.Next(int, int)"/> with a wrapper that eliminates modulo bias (i.e. the fact that certain results will be more common based on the remainder when dividing <see cref="int.MaxValue"/> by them)
        /// </summary>
        /// <param name="objRandom">Instance of Random to use.</param>
        /// <param name="minValue">Minimum value (inclusive) to generate.</param>
        /// <param name="maxValue">Maximum value (exclusive) to generate.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<int> NextModuloBiasRemovedAsync(this Random objRandom, int minValue, int maxValue, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await NextModuloBiasRemovedAsync(objRandom, maxValue - minValue, token).ConfigureAwait(false) + minValue;
        }
    }
}
