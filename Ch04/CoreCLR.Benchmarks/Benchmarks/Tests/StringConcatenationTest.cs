using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Diagnosers;

// dotnet run -c Release -f netcoreapp2.0

//BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 1 [1607, Anniversary Update] (10.0.14393.1884)
//Processor=Intel Core i7-4770K CPU 3.50GHz(Haswell), ProcessorCount=8
//Frequency=3410081 Hz, Resolution=293.2482 ns, Timer=TSC
//.NET Core SDK=2.0.0
//  [Host] : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT
//  Clr    : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
//  Core   : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT


//              Method |  Job | Runtime |      Mean |     Error |    StdDev |  Gen 0 | Allocated |
//-------------------- |----- |-------- |----------:|----------:|----------:|-------:|----------:|
// StringConcatenation |  Clr |     Clr | 14.639 us | 0.1192 us | 0.1115 us | 7.8125 |  33.26 KB |
//       StringBuilder |  Clr |     Clr |  9.293 us | 0.1069 us | 0.0948 us | 3.4180 |  14.15 KB |
// StringBuilderCached |  Clr |     Clr |  9.163 us | 0.0405 us | 0.0359 us | 3.1738 |  13.08 KB |
// StringConcatenation | Core |    Core | 12.420 us | 0.0920 us | 0.0860 us | 6.3477 |  26.75 KB |
//       StringBuilder | Core |    Core |  7.708 us | 0.1394 us | 0.1164 us | 1.7090 |   7.64 KB |
// StringBuilderCached | Core |    Core |  7.630 us | 0.0449 us | 0.0420 us | 1.4648 |   6.57 KB |

namespace Benchmarks.Tests
{
    [CoreJob, ClrJob]
    //[LegacyJitX64Job, RyuJitX64Job]
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(printAsm: true, printSource: true, printIL: true)]
    [HardwareCounters(HardwareCounter.CacheMisses, HardwareCounter.LlcMisses)]
    [MarkdownExporter, AsciiDocExporter, HtmlExporter, CsvExporter, RPlotExporter]
    public class StringConcatenationTest
    {
        [Benchmark]
        public static string StringConcatenation()
        {
            string result = string.Empty;
            foreach (var num in Enumerable.Range(0, 64))
                result += string.Format("{0:D4}", num);
            return result;
        }

        [Benchmark]
        public static string StringBuilderDefault()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var num in Enumerable.Range(0, 64))
                sb.AppendFormat("{0:D4}", num);
            return sb.ToString();
        }

        [Benchmark]
        public static string StringBuilderWithCapacity()
        {
            StringBuilder sb = new StringBuilder(2 * 4 * 64);
            foreach (var num in Enumerable.Range(0, 64))
                sb.AppendFormat("{0:D4}", num);
            return sb.ToString();
        }

        [Benchmark]
        public static string StringBuilderCached()
        {
            StringBuilder sb = StringBuilderCache.Acquire(2 * 4 * 64);
            foreach (var num in Enumerable.Range(0, 64))
                sb.AppendFormat("{0:D4}", num);
            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }

    internal static class StringBuilderCache
    {
        // The value 360 was chosen in discussion with performance experts as a compromise between using
        // as litle memory (per thread) as possible and still covering a large part of short-lived
        // StringBuilder creations on the startup path of VS designers.
        private const int MAX_BUILDER_SIZE = 1024; // 360

        [ThreadStatic]
        private static StringBuilder CachedInstance;

        public static StringBuilder Acquire(int capacity = 16)
        {
            if (capacity <= MAX_BUILDER_SIZE)
            {
                StringBuilder sb = StringBuilderCache.CachedInstance;
                if (sb != null)
                {
                    // Avoid stringbuilder block fragmentation by getting a new StringBuilder
                    // when the requested size is larger than the current capacity
                    if (capacity <= sb.Capacity)
                    {
                        StringBuilderCache.CachedInstance = null;
                        sb.Clear();
                        return sb;
                    }
                }
            }
            return new StringBuilder(capacity);
        }

        public static void Release(StringBuilder sb)
        {
            if (sb.Capacity <= MAX_BUILDER_SIZE)
            {
                StringBuilderCache.CachedInstance = sb;
            }
        }

        public static string GetStringAndRelease(StringBuilder sb)
        {
            string result = sb.ToString();
            Release(sb);
            return result;
        }
    }
}
