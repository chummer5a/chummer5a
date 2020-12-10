using System;
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
        static void Main(string[] args)
        {
            // Benchmarks should not be run in a Debug configuration
            Utils.BreakIfDebug();
            BenchmarkRunner.Run<Method1VsMethod2>();
        }
    }

    /// <summary>
    /// Replace with a more suitable name in practice. You can also benchmark as many methods as you like.
    /// </summary>
    [SimpleJob(RuntimeMoniker.Net472, baseline: true)]
    [RPlotExporter]
    public class Method1VsMethod2
    {
        private const string strFirst = "lorem";
        private const string strLast = "ipsum dolor sit amet";

        [Params(10000, 100000)]
        public int N;

        /// <summary>
        /// Replace with a more suitable name in practice.
        /// </summary>
        /// <returns>Doesn't matter for benchmarks, but makes sure that key code isn't optimized away.</returns>
        [Benchmark]
        public string Method1()
        {
            string strLoop = string.Empty;
            for (int iI = 0; iI < N; ++iI)
            {
                strLoop = strFirst + strLast.Substring(0, 5);
            }
            return strLoop;
        }

        /// <summary>
        /// Replace with a more suitable name in practice.
        /// </summary>
        /// <returns>Doesn't matter for benchmarks, but makes sure that key code isn't optimized away.</returns>
        [Benchmark]
        public string Method2()
        {
            string strLoop = string.Empty;
            for (int iI = 0; iI < N; ++iI)
            {
                char[] achrLoop = new char[strFirst.Length + 5];
                Span<char> achrLoopSpan = achrLoop.AsSpan();
                strFirst.AsSpan().CopyTo(achrLoop);
                strLast.AsSpan(0, 5).CopyTo(achrLoopSpan.Slice(strFirst.Length));
                strLoop = new string(achrLoop);
            }
            return strLoop;
        }
    }
}
