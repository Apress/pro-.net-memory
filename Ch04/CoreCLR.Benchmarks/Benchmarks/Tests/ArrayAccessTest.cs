using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Diagnosers;

// dotnet run -c Release -f net47

//BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 1 [1607, Anniversary Update] (10.0.14393.1884)
//Processor=Intel Core i7-4770K CPU 3.50GHz(Haswell), ProcessorCount=8
//Frequency=3410078 Hz, Resolution=293.2484 ns, Timer=TSC
//[Host] : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
//Clr    : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
//
//          Method |  Job | Runtime |      Mean |     Error |    StdDev |    Gen 0 |    Gen 1 |    Gen 2 |   Allocated | CacheMisses/Op | LlcMisses/Op |
//---------------- |----- |-------- |----------:|----------:|----------:|---------:|---------:|---------:|------------:|---------------:|-------------:|
// IJAccessPattern |  Clr |     Clr |  47.60 ms | 0.9017 ms | 0.8434 ms | 937.5000 | 937.5000 | 937.5000 | 100000040 B |         555441 |       555441 |
// JIAccessPattern |  Clr |     Clr | 245.26 ms | 1.5075 ms | 1.4101 ms | 937.5000 | 937.5000 | 937.5000 | 100000040 B |        2353572 |      2353554 |
// IJAccessPattern | Core |    Core |        NA |        NA |        NA |      N/A |      N/A |      N/A |         N/A |              - |            - |
// JIAccessPattern | Core |    Core |        NA |        NA |        NA |      N/A |      N/A |      N/A |         N/A |              - |            - |

namespace Benchmarks.Tests
{
    [ClrJob, CoreJob]
    //[LegacyJitX64Job, RyuJitX64Job]
    [MemoryDiagnoser]
    [HardwareCounters(HardwareCounter.CacheMisses, HardwareCounter.LlcMisses)]
    public class ArrayAccessTest
    {
        [Benchmark]
        public static int IJAccessPattern()
        {
            int sum = 0;
            int n = 5000;
            int m = 5000;
            int[,] tab = new int[n, m];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < m; ++j)
                {
                    tab[i, j] = 1;
                }
            }
            for (int i = 0; i < n; ++i)
            {
                sum += tab[i, i];
            }
            return sum;
        }

        [Benchmark]
        public static int JIAccessPattern()
        {
            int sum = 0;
            int n = 5000;
            int m = 5000;
            int[,] tab = new int[n, m];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < m; ++j)
                {
                    tab[j, i] = 1;
                }
            }
            for (int i = 0; i < n; ++i)
            {
                sum += tab[i, i];
            }
            return sum;
        }
    }
}
