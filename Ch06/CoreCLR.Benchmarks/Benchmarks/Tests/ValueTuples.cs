using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;

namespace Benchmarks.Tests
{
    /*
       Method |      Mean |     Error |    StdDev |  Gen 0 | Allocated |
------------- |----------:|----------:|----------:|-------:|----------:|
 ProcessData1 | 11.326 ns | 0.2964 ns | 0.3957 ns | 0.0210 |      88 B |
 ProcessData2 |  5.207 ns | 0.0198 ns | 0.0166 ns |      - |       0 B |
    */

    [CoreJob]
    [MemoryDiagnoser]
    public class ValueTuples
    {
        [Benchmark]
        public Tuple<ResultDesc, ResultData> ProcessData1()
        {
            return new Tuple<ResultDesc, ResultData>(new ResultDesc() { Count = 10 }, new ResultData() { Average = 0.0, Sum = 10.0 });
            //return Tuple.Create(new ResultDesc() { Count = 10 }, new ResultData() { Average = 0.0, Sum = 10.0 });
        }

        [Benchmark]
        public (ResultDescStruct, ResultDataStruct) ProcessData2()
        {
            return (new ResultDescStruct() { Count = 10 }, new ResultDataStruct() { Average = 0.0, Sum = 10.0 });
        }
    }

    public class ResultDesc : IResultDesc
    {
        public int Count;
    }

    public class ResultData : IResultData
    {
        public double Sum;
        public double Average;
    }

    public interface IResultDesc
    {

    }
    public interface IResultData
    {

    }

    public struct ResultDescStruct
    {
        public int Count;
    }

    public struct ResultDataStruct
    {
        public double Sum;
        public double Average;
    }
}
