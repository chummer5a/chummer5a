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
using System.Buffers;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Chummer.Benchmarks
{
    /// <summary>
    /// This is a dummy program meant for if/when a dev wants to quickly benchmark something
    /// </summary>
    public static class Program
    {
        private static void Main()
        {
            // Benchmarks should not be run in a Debug configuration
            Utils.BreakIfDebug();
            BenchmarkRunner.Run<ForeachSplitComparison>();
        }
    }

    /// <summary>
    /// Replace with a more suitable name in practice. You can also benchmark as many methods as you like.
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60, baseline: true)]
    public class ForeachSplitComparison
    {
        private readonly string _strLongWord;

        [Params(10, 100, 1000, 10000)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnassignedField.Global
        public int N;
        private const string StringSeparator = "rem ";
        private const char CharSeparator = ' ';
        private static readonly string[] separator = { StringSeparator };
        private static readonly char[] separator2 = { CharSeparator };

        public ForeachSplitComparison()
        {
            Random objRandom = new Random(42);
            string[] astrWords =
            {
                "lorem", "ipsum", "dolor", "sit", "amet"
            };
            StringBuilder sbdLongWord = new StringBuilder(600000);
            foreach (string strWord in astrWords)
                sbdLongWord.Append(strWord).Append(CharSeparator);
            for (int iI = astrWords.Length; iI < 100000; ++iI)
            {
                sbdLongWord.Append(astrWords[objRandom.Next(0, astrWords.Length)]).Append(CharSeparator);
            }
            --sbdLongWord.Length;
            _strLongWord = sbdLongWord.ToString();
        }

        /// <summary>
        /// Replace with a more suitable name in practice.
        /// </summary>
        /// <returns>Doesn't matter for benchmarks, but makes sure that key code isn't optimized away.</returns>
        [Benchmark(Baseline = true)]
        public bool ForeachSplit()
        {
            string strBaseWord = _strLongWord.Substring((_strLongWord.Length - N) / 2, N);
            bool blnDummy = false;
            foreach (string strWord in strBaseWord.Split(separator2, StringSplitOptions.RemoveEmptyEntries))
            {
                blnDummy = strWord.Length > 3;
            }
            return blnDummy;
        }

        /// <summary>
        /// Replace with a more suitable name in practice.
        /// </summary>
        /// <returns>Doesn't matter for benchmarks, but makes sure that key code isn't optimized away.</returns>
        [Benchmark]
        public bool ForeachSplitNoAlloc()
        {
            string strBaseWord = _strLongWord.Substring((_strLongWord.Length - N) / 2, N);
            bool blnDummy = false;
            foreach (string strWord in strBaseWord.SplitNoAlloc(CharSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                blnDummy = strWord.Length > 3;
            }
            return blnDummy;
        }

        /// <summary>
        /// Replace with a more suitable name in practice.
        /// </summary>
        /// <returns>Doesn't matter for benchmarks, but makes sure that key code isn't optimized away.</returns>
        [Benchmark]
        public bool ForeachSplitToPooledArray()
        {
            string strBaseWord = _strLongWord.Substring((_strLongWord.Length - N) / 2, N);
            bool blnDummy = false;
            string[] astrSplit = strBaseWord.SplitToPooledArray(out int intLength, CharSeparator, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                for (int i = 0; i < intLength; ++i)
                {
                    blnDummy = astrSplit[i].Length > 3;
                }
                return blnDummy;
            }
            finally
            {
                ArrayPool<string>.Shared.Return(astrSplit);
            }
        }

        /// <summary>
        /// Replace with a more suitable name in practice.
        /// </summary>
        /// <returns>Doesn't matter for benchmarks, but makes sure that key code isn't optimized away.</returns>
        [Benchmark]
        public bool ForeachSplitString()
        {
            string strBaseWord = _strLongWord.Substring((_strLongWord.Length - N) / 2, N);
            bool blnDummy = false;
            foreach (string strWord in strBaseWord.Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                blnDummy = strWord.Length > 3;
            }
            return blnDummy;
        }

        /// <summary>
        /// Replace with a more suitable name in practice.
        /// </summary>
        /// <returns>Doesn't matter for benchmarks, but makes sure that key code isn't optimized away.</returns>
        [Benchmark]
        public bool ForeachSplitNoAllocString()
        {
            string strBaseWord = _strLongWord.Substring((_strLongWord.Length - N) / 2, N);
            bool blnDummy = false;
            foreach (string strWord in strBaseWord.SplitNoAlloc(StringSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                blnDummy = strWord.Length > 3;
            }
            return blnDummy;
        }

        /// <summary>
        /// Replace with a more suitable name in practice.
        /// </summary>
        /// <returns>Doesn't matter for benchmarks, but makes sure that key code isn't optimized away.</returns>
        [Benchmark]
        public bool ForeachSplitToPooledArrayString()
        {
            string strBaseWord = _strLongWord.Substring((_strLongWord.Length - N) / 2, N);
            bool blnDummy = false;
            string[] astrSplit = strBaseWord.SplitToPooledArray(out int intLength, StringSeparator, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                for (int i = 0; i < intLength; ++i)
                {
                    blnDummy = astrSplit[i].Length > 3;
                }
                return blnDummy;
            }
            finally
            {
                ArrayPool<string>.Shared.Return(astrSplit);
            }
        }
    }
}
