using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;

namespace Benchmarks.Tests
{
    /*
 
                  Method |       Mean |     Error |    StdDev | Allocated |
------------------------ |-----------:|----------:|----------:|----------:|
          PrimitiveField |  0.0120 ns | 0.0032 ns | 0.0027 ns |       0 B |
          ReferenceField |  0.0213 ns | 0.0078 ns | 0.0069 ns |       0 B |
   PrimitiveThreadStatic |  4.0294 ns | 0.0263 ns | 0.0233 ns |       0 B |
   ReferenceThreadStatic |  4.9294 ns | 0.0072 ns | 0.0057 ns |       0 B |
 PrimitiveThreadDataSlot | 47.3747 ns | 0.0420 ns | 0.0350 ns |       0 B |
 ReferenceThreadDataSlot | 46.6289 ns | 0.2949 ns | 0.2614 ns |       0 B |
    PrimitiveThreadLocal |  7.8239 ns | 0.0140 ns | 0.0124 ns |       0 B |
 PrimitiveReferenceLocal | 11.7464 ns | 0.0190 ns | 0.0168 ns |       0 B |
     */
    [CoreJob]
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(printAsm: true, printSource: true, printIL: true)]
    public class ThreadStatics
    {
        private int valueLocal;
        private SomeData referenceLocal;
        private ThreadLocal<int> threadValueLocal;
        private ThreadLocal<SomeData> threadReferenceLocal;

        [GlobalSetup]
        public void Setup()
        {
           
            var slot1 = Thread.AllocateNamedDataSlot("SlotPrimitive");
            Thread.SetData(slot1, 0);
            var slot2 = Thread.AllocateNamedDataSlot("SlotManaged");
            Thread.SetData(slot2, new SomeData());
            threadValueLocal = new ThreadLocal<int>(() => 0);
            threadReferenceLocal = new ThreadLocal<SomeData>(() => new SomeData());
            referenceLocal = new SomeData();
        }

        [Benchmark]
        public int PrimitiveField()
        {
            return valueLocal;
        }

        [Benchmark]
        public int ReferenceField()
        {
            return referenceLocal.Field;
        }

        [Benchmark]
        public int PrimitiveThreadStatic()
        {
            return ClassWithStatics.threadStaticValueData;
        }

        [Benchmark]
        public int ReferenceThreadStatic()
        {
            return ClassWithStatics.threadStaticReferenceData.Field;
        }

        [Benchmark]
        public int PrimitiveThreadDataSlot()
        {
            return (int)Thread.GetData(Thread.GetNamedDataSlot("SlotPrimitive"));
        }

        [Benchmark]
        public int ReferenceThreadDataSlot()
        {
            SomeData data = (SomeData)Thread.GetData(Thread.GetNamedDataSlot("SlotManaged"));
            return data.Field;
        }

        [Benchmark]
        public int PrimitiveThreadLocal()
        {
            return threadValueLocal.Value;
        }

        [Benchmark]
        public int PrimitiveReferenceLocal()
        {
            return threadReferenceLocal.Value.Field;
        }
    }

    public class ClassWithStatics
    {
        [ThreadStatic]
        public static int threadStaticValueData;
        [ThreadStatic]
        public static SomeData threadStaticReferenceData = new SomeData();
    }

    public class SomeData
    {
        public int Field;
    }
}
