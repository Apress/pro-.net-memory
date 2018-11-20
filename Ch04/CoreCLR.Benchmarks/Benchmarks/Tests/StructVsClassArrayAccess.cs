using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Diagnosers;

//BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 1 [1607, Anniversary Update] (10.0.14393.1884)
//Processor=Intel Core i7-4770K CPU 3.50GHz(Haswell), ProcessorCount=8
//Frequency=3410073 Hz, Resolution=293.2489 ns, Timer=TSC
//.NET Core SDK=2.0.0
//  [Host] : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT
//  Clr    : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
//  Core   : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT

//            Method |  Job | Runtime |       Mean |     Error |    StdDev | Allocated |
//------------------ |----- |-------- |-----------:|----------:|----------:|----------:|
// StructArrayAccess |  Clr |     Clr |   605.1 us |  3.398 us |  2.838 us |       0 B |
//  ClassArrayAccess |  Clr |     Clr | 1,754.2 us |  8.436 us |  7.478 us |       0 B |
// StructArrayAccess | Core |    Core |   618.7 us | 12.349 us | 18.484 us |       0 B |
//  ClassArrayAccess | Core |    Core | 1,816.5 us | 36.041 us | 44.261 us |       0 B |
namespace Benchmarks.Tests
{
    [CoreJob, ClrJob]
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(printAsm: true, printSource: true, printIL: true)]
    [HardwareCounters(HardwareCounter.CacheMisses, HardwareCounter.LlcMisses)]
    public class StructVsClassArrayAccess
    {
        private SmallClass[] classes;
        private SmallStruct[] structs;
        const int items = 1000_000;

        [GlobalSetup]
        public void Setup()
        {
            structs = new SmallStruct[items];
            classes = new SmallClass[items];
            for (int i = 0; i < items; i++)
            {
                structs[i] = new SmallStruct { Value1 = i };
                classes[i] = new SmallClass { Value1 = i };
            }
        }


        [Benchmark]
        public int StructArrayAccess()
        {
            int result = 0;
            for (int i = 0; i < items; i++)
                result += Helper1(structs, i);
            return result;
        }

        [Benchmark]
        public int ClassArrayAccess()
        {
            int result = 0;
            for (int i = 0; i < items; i++)
                result += Helper2(classes, i);
            return result;
        }

        private int Helper1(SmallStruct[] data, int index)
        {
            return data[index].Value1;
        }

        private int Helper2(SmallClass[] data, int index)
        {
            return data[index].Value1;
        }

        public struct SmallStruct
        {
            public int Value1;
            public int Value2;
        }

        public class SmallClass
        {
            public int Value1;
            public int Value2;
        }
    }


}
