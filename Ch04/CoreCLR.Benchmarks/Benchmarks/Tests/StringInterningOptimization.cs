using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Diagnosers;

//BenchmarkDotNet is unable to show the difference - test are too short lived to make benefit of string interning
//
//BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 1 [1607, Anniversary Update] (10.0.14393.1884)
//Processor=Intel Core i7-4770K CPU 3.50GHz(Haswell), ProcessorCount=8
//Frequency=3410073 Hz, Resolution=293.2489 ns, Timer=TSC
//.NET Core SDK=2.0.0
//  [Host] : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT
//  Clr    : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
//  Core   : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT


//                 Method |  Job | Runtime |     Mean |     Error |    StdDev |     Gen 0 | Allocated |
//----------------------- |----- |-------- |---------:|----------:|----------:|----------:|----------:|
//         GroupByCommand |  Clr |     Clr | 194.2 ms | 1.1849 ms | 1.1084 ms | 9000.0000 |  36.21 MB |
// GroupByCommandInterned |  Clr |     Clr | 295.2 ms | 0.6957 ms | 0.5432 ms | 9000.0000 |  36.21 MB |
//         GroupByCommand | Core |    Core | 178.9 ms | 0.6926 ms | 0.6479 ms | 9000.0000 |  36.21 MB |
// GroupByCommandInterned | Core |    Core | 238.9 ms | 2.2130 ms | 2.0700 ms | 9000.0000 |  36.21 MB |

namespace Benchmarks.Tests
{
    [CoreJob, ClrJob]
    [MemoryDiagnoser]
    public class StringInterningOptimization
    {
        [Benchmark]
        public int GroupByCommand()
        {
            int result = 0;
            string file = "StringInterningFileTemplate.txt";
            Dictionary<string, int> counter = new Dictionary<string, int>();
            foreach (var line in File.ReadLines(file))
            {
                bool counted = false;
                foreach (var key in counter.Keys)
                {
                    if (key == line)
                    {
                        counter[key]++;
                        counted = true;
                        break;
                    }
                }
                if (!counted)
                {
                    counter.Add(line, 0);
                }
            }
            foreach (var pair in counter)
            {
                result += pair.Value;
            }
            return result;
        }

        [Benchmark]
        public int GroupByCommandInterned()
        {
            int result = 0;
            string file = "StringInterningFileTemplate.txt";
            Dictionary<string, int> counter = new Dictionary<string, int>();
            foreach (var line in File.ReadLines(file))
            {
                var line2 = string.Intern(line);    // line lifetime most probably ends here
                bool counted = false;
                foreach (var key in counter.Keys)
                {
                    if (key == line2) // should use ReferenceEquals
                    {
                        counter[key]++;
                        counted = true;
                        break;
                    }
                }
                if (!counted)
                {
                    counter.Add(line2, 0); // adding interned string
                }
            }
            foreach (var pair in counter)
            {
                result += pair.Value;
            }
            return result;
        }
    }
}
