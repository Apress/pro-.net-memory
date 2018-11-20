using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;

/*
	                 Method |    N |           Mean |  Gen 0 |  Gen 1 | Allocated |
--------------------------- |----- |---------------:|-------:|-------:|----------:|
 ConsumeNonFinalizableClass |    1 |       2.777 ns | 0.0076 |      - |      32 B |
    ConsumeFinalizableClass |    1 |     132.138 ns | 0.0074 | 0.0036 |      32 B |
 ConsumeNonFinalizableClass |   10 |      30.667 ns | 0.0762 |      - |     320 B |
    ConsumeFinalizableClass |   10 |   1,342.092 ns | 0.0744 | 0.0362 |     320 B |  
 ConsumeNonFinalizableClass |  100 |     316.633 ns | 0.7625 |      - |    3200 B |
    ConsumeFinalizableClass |  100 |  13,607.436 ns | 0.7477 | 0.3662 |    3200 B |
 ConsumeNonFinalizableClass | 1000 |   3,244.837 ns | 7.6256 |      - |   32000 B |
    ConsumeFinalizableClass | 1000 | 131,725.089 ns | 7.5684 | 3.6621 |   32000 B |
 */
namespace Benchmarks.Tests
{
    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-11
    [CoreJob]
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(printAsm: true, printSource: true, printIL: true)]
    public class Finalization
    {
        [Params(1, 10, 100, 1000)]
        public int N;

        public int Data = 360;

        [Benchmark]
        public void ConsumeNonFinalizableClass()
        {
            for (int i = 0; i < N; ++i)
            {
                var obj = new NonFinalizableClass();
                obj.Value1 = Data;
            }
        }

        [Benchmark]
        public void ConsumeFinalizableClass()
        {
            for (int i = 0; i < N; ++i)
            {
                var obj = new FinalizableClass();
                obj.Value1 = Data;
            }
        }

        public class NonFinalizableClass
        {
            public int Value1;
            public int Value2;
            public int Value3;
            public int Value4;
        }

        public class FinalizableClass
        {
            public int Value1;
            public int Value2;
            public int Value3;
            public int Value4;

            ~FinalizableClass()
            {

            }
        }
    }
}
