using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Diagnosers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Formatting;

/*
 * - IEnumerable<int> boxes enumerator when passed via argument
 * - string
 * - wspomnieć o https://github.com/MikePopoloski/StringFormatter oraz https://github.com/Abc-Arbitrage/ZeroLog
 */

/* dla items = 100:

            Method |       Mean |     Error |    StdDev |  Gen 0 | Allocated |
 ----------------- |-----------:|----------:|----------:|-------:|----------:|
 ProcessEnumerable | 2,208.6 ns | 34.878 ns | 29.125 ns | 0.7782 |    3272 B |
 ProcessStackalloc |   542.9 ns |  5.385 ns |  5.037 ns |      - |       0 B |
 */

namespace Benchmarks.Tests
{
    //[ClrJob, CoreJob]
    [CoreJob]
    //[LegacyJitX64Job, RyuJitX64Job]
    [MemoryDiagnoser]
    public class HiddenAllocationsTest
    {
        private Logger _logger;
        private List<BigData> _list;
        int _items = 100;

        [GlobalSetup]
        public void Setup()
        {
            _list = new List<BigData>();
            for (int i = 0; i < _items; i++)
            {
                _list.Add(new BigData() { Age = i, Description = "SOME_FEM_DATA"});
            }
            // Warm up StringBuilderCache
            Console.WriteLine(string.Format("Warming: {0}", _items.ToString(CultureInfo.InvariantCulture)));
            _logger = new Logger
            {
                Level = Logger.LoggingLevel.Info
            };
            Span<int> var = ArrayPool<int>.Shared.Rent(100);
        }



        private double ProcessData(Span<DataStruct> readOnlySpan)
        {
            return 0.0;
        }

        //[Benchmark]
        public double SimpleVersion1()
        {
            double avg = CalculateSumSimple1(_list);

            //_logger.Debug("Result: " + (avg / _items)); // daje 160 B więc używamy StringBuildera ale on daje 240:
            StringBuilder sb = new StringBuilder();
            sb.Append("Result: ");
            sb.Append(avg / _items);
            _logger.Debug(sb.ToString());
            return avg;
        }

        //[Benchmark]
        public double SimpleVersion2()
        {

            double avg = CalculateSumSimple2(_list);
            // double temp = (avg / _items); // will remove to 88 B because now captures both avg and this (to access _items)
            _logger.Debug(() => string.Format("Result: {0}", (avg / _items).ToString(CultureInfo.InvariantCulture)));
            return avg;
        }

        //[Benchmark]
        public double SimpleVersion3()
        {
            double avg = CalculateSumSimple2(_list);
            _logger.Debug("Result: {0}", avg / _items);
            return avg;
        }

        //[Benchmark]
        public double Version0()
        {
            double avg = ProcessData1(_list.Select(x => new DataClass() { Age = x.Age, Sex = Sex.Female }));
            _logger.Debug("Result: {0}", avg / _items);
            return avg;
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-15
        [Benchmark]
        public double ProcessEnumerable()
        {
            double avg = ProcessData1(_list.Select(x => new DataClass()
                {
                    Age = x.Age,
                    Sex = Helper(x.Description) ? Sex.Female : Sex.Male //x.Description.Substring(5, 3) == "FEM" ? Sex.Female : Sex.Male
                }));
            _logger.Debug("Result: {0}", avg / _items);
            return avg;
        }

        //[Benchmark]
        public double Version1b()
        {
            double avg = ProcessData1(_list.Select(x => new DataClass() { Age = x.Age, Sex = ((x.Description.AsSpan().Slice(5, 3) == "FEM".AsSpan()) ? Sex.Female : Sex.Male) })); // this is comparing addresses!
            _logger.Debug("Result: {0}", avg / _items);
            return avg;
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-16
        [Benchmark]
        public unsafe double ProcessStackalloc()
        {
            //DataStruct[] data = _list.Select(x => new DataStruct() {Age = x.Age, Sex = x.Sex}).ToArray(); // to nadal na heapie tyle że mniejsze
            DataStruct* data = stackalloc DataStruct[_list.Count];
            for (int i = 0; i < _list.Count; ++i)
            {
                data[i].Age = _list[i].Age;
                data[i].Sex = Helper(_list[i].Description) ? Sex.Female : Sex.Male;
            }
            double avg = ProcessData2(new ReadOnlySpan<DataStruct>(data, _list.Count));
            _logger.Debug("Result: {0}", avg / _items);
            return avg;
        }

        private static bool Helper(string str)
        {
            return str[5] == 'F' && str[6] == 'E' && str[7] == 'M';
        }

        public double ProcessData1(IEnumerable<DataClass> list)
        {
            double sum = 0;
            foreach (var x in list)
            {
                sum += x.Age;
            }
            return sum;
        }

        public double ProcessData2(ReadOnlySpan<DataStruct> list)
        {
            double sum = 0;
            for (int i = 0; i < list.Length; ++i)
            {
                sum += list[i].Age;
            }
            return sum;
        }

        public double CalculateSumSimple1(IEnumerable<BigData> list)
        {
            double sum = 0;
            foreach (var x in list)
            {
                sum += x.Age;
            }
            return sum;
        }

        public double CalculateSumSimple2(List<BigData> list)
        {
            double sum = 0;
            foreach (var x in list)
            {
                sum += x.Age;
            }
            return sum;
        }
    }

    //public struct Constants
    //{
    //    public static ReadOnlySpan<char> FemaleSpan = "FEM".AsSpan();
    //}

    public class BigData
    {
        public string Description;
        public double Age;
    }

    public class DataClass
    {
        public Sex Sex;
        public double Age;
    }

    public struct DataStruct
    {
        public Sex Sex;
        public double Age;
    }

    public enum Sex
    {
        Male,
        Female
    }

    public class Logger
    {
        public enum LoggingLevel
        {
            Debug,
            Info,
            Warning
        }

        public LoggingLevel Level { get; set; }

        private void Log(LoggingLevel level, string message)
        {
            if (level >= Level)
            {
                Console.WriteLine(message);
            }
        }
        private void Log(LoggingLevel level, Func<string> exceptionMessage)
        {
            if (level >= Level)
            {
                Console.WriteLine(exceptionMessage());
            }
        }
        private void Log<T>(LoggingLevel level, string format, T arg)
        {
            if (level >= Level)
            {
                Console.WriteLine(string.Format(format, arg));
            }
        }

        public void Debug(string message) => Log(LoggingLevel.Debug, message);
        public void Info(string message) => Log(LoggingLevel.Info, message);
        public void Warning(string message) => Log(LoggingLevel.Warning, message);
        public void Debug(Func<string> exceptionMessage) => Log(LoggingLevel.Debug, exceptionMessage);
        public void Info(Func<string> exceptionMessage) => Log(LoggingLevel.Info, exceptionMessage);
        public void Warning(Func<string> exceptionMessage) => Log(LoggingLevel.Warning, exceptionMessage);
        public void Debug<T>(string format, T arg) => Log(LoggingLevel.Debug, format, arg);
        public void Info<T>(string format, T arg) => Log(LoggingLevel.Info, format, arg);
        public void Warning<T>(string format, T arg) => Log(LoggingLevel.Warning, format, arg);
    }
}

