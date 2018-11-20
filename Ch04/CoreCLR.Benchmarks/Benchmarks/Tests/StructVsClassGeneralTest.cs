using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Diagnosers;

// C:\Program Files\dotnet\  dotnet run -c Release -f netcoreapp2.0

//BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 1 [1607, Anniversary Update] (10.0.14393.1884)
//Processor=Intel Core i7-4770K CPU 3.50GHz(Haswell), ProcessorCount=8
//Frequency=3410073 Hz, Resolution=293.2489 ns, Timer=TSC
//.NET Core SDK=2.0.0
//  [Host] : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT
//  Clr    : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
//  Core   : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT


//        Method |  Job | Runtime |      Mean |     Error |    StdDev |  Gen 0 | Allocated |
//-------------- |----- |-------- |----------:|----------:|----------:|-------:|----------:|
// ConsumeStruct |  Clr |     Clr | 0.6864 ns | 0.0128 ns | 0.0107 ns |      - |       0 B |
//  ConsumeClass |  Clr |     Clr | 3.3206 ns | 0.0565 ns | 0.0529 ns | 0.0076 |      32 B |
// ConsumeStruct | Core |    Core | 2.0636 ns | 0.0185 ns | 0.0173 ns |      - |       0 B |
//  ConsumeClass | Core |    Core | 4.4261 ns | 0.0638 ns | 0.0597 ns | 0.0076 |      32 B |

namespace Benchmarks.Tests
{
    [CoreJob, ClrJob]
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(printAsm: true, printSource: true, printIL: true)]
    [HardwareCounters(HardwareCounter.CacheMisses, HardwareCounter.LlcMisses)]
    public class StructVsClassGeneralTest
    {
        public static int Data = 360;

        [Benchmark]
        public static int ConsumeStruct()
        {
            SomeStruct sd = new SomeStruct();
            sd.Value1 = Data;
            return StructHelper(sd);
        }

        [Benchmark]
        public static int ConsumeClass()
        {
            SomeClass sd = new SomeClass();
            sd.Value1 = Data;
            return ClassHelper(sd);
        }

        private static int StructHelper(SomeStruct arg)
        {
            return arg.Value1;
        }

        private static int ClassHelper(SomeClass arg)
        {
            return arg.Value1;
        }
    }

    public struct SomeStruct
    {
        public int Value1;
        public int Value2;
        public int Value3;
        public int Value4;
    }

    public class SomeClass
    {
        public int Value1;
        public int Value2;
        public int Value3;
        public int Value4;
    }
}
