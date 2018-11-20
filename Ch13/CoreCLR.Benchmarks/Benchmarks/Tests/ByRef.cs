using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;

/*
              Method |     Mean |     Error |    StdDev | Allocated |
-------------------- |---------:|----------:|----------:|----------:|
      Struct32Access | 1.560 ns | 0.0071 ns | 0.0059 ns |       0 B |
     Struct112Access | 5.229 ns | 0.0085 ns | 0.0075 ns |       0 B |
     Struct192Access | 7.457 ns | 0.0244 ns | 0.0229 ns |       0 B |
 ByRefStruc32tAccess | 1.332 ns | 0.0154 ns | 0.0145 ns |       0 B |
ByRefStruct112Access | 1.343 ns | 0.0032 ns | 0.0030 ns |       0 B |
ByRefStruct192Access | 1.329 ns | 0.0071 ns | 0.0066 ns |       0 B |
         ClassAccess | 1.098 ns | 0.0013 ns | 0.0010 ns |       0 B |

 */

namespace Benchmarks.Tests
{
    [CoreJob]
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(printAsm: true, printSource: true, printIL: true)]
    public unsafe class ByRef
    {
        private BigClass bigClass;
        private BigStruct bigStruct;
        private BigStruct20 bigStruct20;
        private BigStruct40 bigStruct40;
        const int items = 1000_000;

        [GlobalSetup]
        public void Setup()
        {
            this.bigStruct = new BigStruct();
            this.bigStruct20 = new BigStruct20();
            this.bigStruct40 = new BigStruct40();
            this.bigClass = new BigClass();
        }

        [Benchmark]
        public int StructAccess()
        {
            int result = 0;
            result = Helper1(bigStruct, 0);
            return result;
        }

        [Benchmark]
        public int Struct20Access()
        {
            int result = 0;
            result = Helper1(bigStruct20, 0);
            return result;
        }

        [Benchmark]
        public int Struct40Access()
        {
            int result = 0;
            result = Helper1(bigStruct40, 0);
            return result;
        }

        [Benchmark]
        public int ByRefStructAccess()
        {
            int result = 0;
            result = Helper1(ref bigStruct, 0);
            return result;
        }

        [Benchmark]
        public int ByRefStruct20Access()
        {
            int result = 0;
            result = Helper1(ref bigStruct20, 0);
            return result;
        }

        [Benchmark]
        public int ByRefStruct40Access()
        {
            int result = 0;
            result = Helper1(ref bigStruct40, 0);
            return result;
        }

        [Benchmark]
        public int ClassAccess()
        {
            int result = 0;
            result = Helper2(bigClass, 0);
            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int Helper1(BigStruct data, int index)
        {
            return data.Value1;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int Helper1(BigStruct20 data, int index)
        {
            return data.Value1;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int Helper1(BigStruct40 data, int index)
        {
            return data.Value1;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int Helper1(ref BigStruct data, int index)
        {
            return data.Value1;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int Helper1(ref BigStruct20 data, int index)
        {
            return data.Value1;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int Helper1(ref BigStruct40 data, int index)
        {
            return data.Value1;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int Helper2(BigClass data, int index)
        {
            return data.Value1;
        }

        public struct BigStruct
        {
            public int Value1;
            public int Value2;
            public int Value3;
            public int Value4;
            public int Value5;
            public int Value6;
            public int Value7;
            public int Value8;
        }

        public struct BigStruct20
        {
            public int Value1;
            public int Value2;
            public int Value3;
            public int Value4;
            public int Value5;
            public int Value6;
            public int Value7;
            public int Value8;
            public fixed int Array[20];
        }

        public struct BigStruct40
        {
            public int Value1;
            public int Value2;
            public int Value3;
            public int Value4;
            public int Value5;
            public int Value6;
            public int Value7;
            public int Value8;
            public fixed int Array[40];
        }

        public class BigClass
        {
            public int Value1;
            public int Value2;
            public int Value3;
            public int Value4;
            public int Value5;
            public int Value6;
            public int Value7;
            public int Value8;
        }
    }
}
