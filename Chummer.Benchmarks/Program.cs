using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Chummer.Benchmarks
{
    /// <summary>
    /// This is a dummy program meant for if/when a dev wants to quickly benchmark something
    /// </summary>
    public class Program
    {
        private static void Main(string[] args)
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
    [SimpleJob(RuntimeMoniker.Net472, baseline: true)]
    public class ForeachSplitComparison
    {
        private readonly string[] astrWords = {
            "lorem", "ipsum", "dolor", "sit", "amet"
        };

        private readonly string strLongWord;

        [Params(100, 1000, 10000)]
        // ReSharper disable once MemberCanBePrivate.Global
        public int N;

        public ForeachSplitComparison()
        {
            Random objRandom = new Random(42);
            StringBuilder sbdLongWord = new StringBuilder(600000);
            foreach (string strWord in astrWords)
                sbdLongWord.Append(strWord).Append(' ');
            for (int iI = astrWords.Length; iI < 100000; ++iI)
            {
                sbdLongWord.Append(astrWords[objRandom.Next(0, astrWords.Length)]).Append(' ');
            }
            sbdLongWord.Length -= 1;
            strLongWord = sbdLongWord.ToString();
        }

        /// <summary>
        /// Replace with a more suitable name in practice.
        /// </summary>
        /// <returns>Doesn't matter for benchmarks, but makes sure that key code isn't optimized away.</returns>
        [Benchmark(Baseline = true)]
        public bool ForeachSplit()
        {
            string strBaseWord = strLongWord.Substring((strLongWord.Length - N) / 2, N);
            bool blnDummy = false;
            foreach (string strWord in strBaseWord.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
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
            string strBaseWord = strLongWord.Substring((strLongWord.Length - N) / 2, N);
            bool blnDummy = false;
            foreach (string strWord in strBaseWord.SplitNoAlloc(' ', StringSplitOptions.RemoveEmptyEntries))
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
        public bool ForeachSplitString()
        {
            string strBaseWord = strLongWord.Substring((strLongWord.Length - N) / 2, N);
            bool blnDummy = false;
            foreach (string strWord in strBaseWord.Split(new[] { "rem " }, StringSplitOptions.RemoveEmptyEntries))
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
            string strBaseWord = strLongWord.Substring((strLongWord.Length - N) / 2, N);
            bool blnDummy = false;
            foreach (string strWord in strBaseWord.SplitNoAlloc("rem ", StringSplitOptions.RemoveEmptyEntries))
            {
                blnDummy = strWord.Length > 3;
            }
            return blnDummy;
        }
    }
}
