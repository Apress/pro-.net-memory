using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading;

namespace RowColumnsBenchmark
{
    class Container
    {
        public OneLineStruct OneLine;
        public TwoLineStruct TwoLine;
        public SizeBenchmark_Poco Poco;
        public SizeBenchmark_Hot Hot;
    }

    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG   
            Console.WriteLine("--- DEBUG MODE ---");
#endif            
            if (args.Length == 0)
            {
                Console.WriteLine("Required one argument!");
                //return;
                args = new string[] { "1lrr" };
            }

            Container c = new Container()
            {
                OneLine = new OneLineStruct()
                {
                    data1 = 1, data2 = 2, data3 = 3, data4 = 4, data5 = 5, data6 = 6, data7 = 7
                },
                TwoLine = new TwoLineStruct()
                {
                    data1 = 8, data2 = 9, data3 = 10, data4 = 11, data5 = 12, data6 = 13, data7 = 14, data8 = 15, data9 = 16
                },
                Poco = new SizeBenchmark_Poco()
                {
                    Index = 100, State = RecordState.Modified, SomeField1 = 1, SomeField2 = 2, SomeField3 = 3, SomeField4 = "1", SomeField5 = "1"
                },
                Hot = new SizeBenchmark_Hot()
                {
                    Index = 101, State = RecordState.Modified, Cold = new SizeBenchmark_Cold()
                }
            };

            int? argument = args.Length == 2 ? new Nullable<int>(int.Parse(args[1])) : null;
            Console.WriteLine($"Running {args[0]} with argument {argument}");
            switch (args[0])
            {
                case "a1" : Tester(() => AccessTest.IJAccessPattern(argument ?? 5000));            
                           break;
                case "a2" : Tester(() => AccessTest.JIAccessPattern(argument ?? 5000));            
                           break;
                case "f1" : Tester(() => FalseSharingTest.DoTest(argument ?? 100000000));            
                           break;
                case "f2" : Tester(() => FalseSharingTest.DoTestBetter(argument ?? 100000000));            
                           break;
                case "f3" : Tester(() => FalseSharingTest.DoTestBest(argument ?? 100000000));            
                           break;
                case "sr" : Tester(() => AccessTest.SequentialReadPattern(argument ?? 5000));            
                           break;
                case "sw" : Tester(() => AccessTest.SequentialWritePattern(argument ?? 5000));            
                           break;
                case "size-poco":
                {
                    var data = AccessTest.SequentialReadPatternPoco_Prepare(argument ?? 5000);
                    Tester(() => AccessTest.SequentialReadPatternPoco(data));
                    break;
                }
                case "size-pocobig":
                {
                    var data = AccessTest.SequentialReadPatternPocoBig_Prepare(argument ?? 5000);
                    Tester(() => AccessTest.SequentialReadPatternPocoBig(data));
                    break;
                }
                case "size-hotcold":
                {
                    var data = AccessTest.SequentialReadPatternHot_Prepare(argument ?? 5000);
                    Tester(() => AccessTest.SequentialWritePatternHotCold(data));
                    break;
                }
                case "1lsr":
                {
                    OneLineStruct[] tab = new OneLineStruct[argument ?? 5000];
                    Tester(() => AccessTest.OneLineStructSequentialReadPattern(tab));
                    break;
                }
                case "2lsr":
                {
                    TwoLineStruct[] tab = new TwoLineStruct[argument ?? 5000];
                    Tester(() => AccessTest.TwoLineStructSequentialReadPattern(tab));
                    break;
                }
                case "1lrr" : 
                {
                    OneLineStruct[] tab = new OneLineStruct[argument ?? 5000];
                    int[] indexes = Enumerable.Range(0, argument ?? 5000).OrderBy(n => Guid.NewGuid()).ToArray();
                    Tester(() => AccessTest.OneLineStructRandomReadPattern(tab, indexes));            
                    break;
                }
                case "2lrr" : 
                {
                    TwoLineStruct[] tab = new TwoLineStruct[argument ?? 5000];
                    int[] indexes = Enumerable.Range(0, argument ?? 5000).OrderBy(n => Guid.NewGuid()).ToArray();
                    Tester(() => AccessTest.TwoLineStructRandomReadPattern(tab, indexes));            
                    break;
                }
                default : Console.WriteLine("Unknown argument!");
                          break;
            }            
        }

        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long value);

        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long value);

        private static bool Initialize(out long qpf)
        {
            try
            {
                long counter;
                return
                    QueryPerformanceFrequency(out qpf) &&
                    QueryPerformanceCounter(out counter);
            }
            catch
            {
                qpf = default(long);
                return false;
            }
        }

        static long GetTimestamp()
        {
            long value;
            QueryPerformanceCounter(out value);
            return value;
        }

        static void Tester(Func<long> act)
        {
            long frequency;
            Initialize(out frequency);
            decimal multiplier = new Decimal(1.0e3);

            byte[] clearer = new byte[160 * 1024 * 1024];
            clearer[0] = 0;
            for (int i = 1; i < clearer.Length; ++i)
                clearer[i] = (byte) (clearer[i - 1] + (byte) 1);

            // Give the test as good a chance as possible
            // of avoiding garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Pre-JIT
            act();

            int runs = 100; //_000;
            double averageTicks = 0.0;
            double averageMilliseconds = 0.0;
            long minTicks = long.MaxValue;
            double minMillseconds = double.MaxValue;
            for (int i = 0; i < runs; ++i)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                act();

                Stopwatch sw = new Stopwatch();
                sw.Start();
                long s = GetTimestamp();
                long result = act();
                long e = GetTimestamp() - s;
                //Console.WriteLine($"Elapsed: {e} ticks");            
                averageTicks += e;
                sw.Stop();
                averageMilliseconds += sw.Elapsed.TotalMilliseconds;
                if (e < minTicks)
                    minTicks = e;
                //if (s.Elapsed.TotalMilliseconds < minMillseconds)
                //    minMillseconds = s.Elapsed.TotalMilliseconds;
                //Console.WriteLine(result);
            }
            averageTicks /= runs;
            averageMilliseconds /= runs;
            //Console.WriteLine($"Average elapsed: {averageTicks} ticks ({averageMilliseconds} ms).");
            Console.WriteLine($"Minimum elapsed: {minTicks} ticks ({minTicks * (double)multiplier / frequency} ms) ({averageMilliseconds})");
            int controlsum = 0;
            for (int i = 0; i < clearer.Length; ++i)
            {
                unchecked
                {
                    controlsum += clearer[i];
                }
            }
            //Console.WriteLine($"Control sum: {controlsum}");
        }
    }

    public class AccessTest
    {
        public static long IJAccessPattern(int size = 5000)
        {
            int n = size;
            int m = size;
            int[,] tab = new int[n, m];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < m; ++j)
                {
                    tab[i, j] = 1;
                }
            }
            return 0;
        }

        public static long JIAccessPattern(int size = 5000)
        {
            int n = size;
            int m = size;
            int[,] tab = new int[n, m];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < m; ++j)
                {
                    tab[j, i] = 1;
                }
            }
            return 0;
        }

        public static long SequentialReadPattern(int size = 5000)
        {
            long sum = 0;
            int n = size;
            int[] tab = new int[n*16];
            for (int i = 0; i < n; ++i)
            {
                unchecked { sum += tab[i*16]; }
            }
            return sum;
        }

        public static long SequentialWritePattern(int size = 5000)
        {
            int n = size;
            int[] tab = new int[n*16];
            for (int i = 0; i < n; ++i)
            {
                tab[i*16] = 1;
            }
            return 0;
        }

        public static long OneLineStructSequentialReadPattern(OneLineStruct[] tab)
        {
            long sum = 0;
            int n = tab.Length;
            for (int i = 0; i < n; ++i)
            {                
                unchecked { sum += tab[i].data1; }
            }
            return sum;
        }

        public static long TwoLineStructSequentialReadPattern(TwoLineStruct[] tab)
        {
            long sum = 0;
            int n = tab.Length;
            for (int i = 0; i < n; ++i)
            {
                unchecked { sum += tab[i].data1; }
            }
            return sum;
        }

        public static long OneLineStructRandomReadPattern(OneLineStruct[] tab, int[] indexes)
        {
            long sum = 0;
            int n = indexes.Length;
            for (int i = 0; i < n; ++i)
            {                
                unchecked { sum += tab[indexes[i]].data1; }
            }
            return sum;
        }

        public static long TwoLineStructRandomReadPattern(TwoLineStruct[] tab, int[] indexes)
        {
            long sum = 0;
            int n = indexes.Length;
            for (int i = 0; i < n; ++i)
            {                
                unchecked { sum += tab[indexes[i]].data1; }
            }
            return sum;
        }

        public static List<SizeBenchmark_Poco> SequentialReadPatternPoco_Prepare(int size)
        {
            List<SizeBenchmark_Poco> result = new List<SizeBenchmark_Poco>(size);
            for (int i = 0; i < size; ++i)
            {
                result.Add(new SizeBenchmark_Poco()
                {
                    Index = i,
                    //State = i % 4 == 0 ? RecordState.Modified : RecordState.NonModified,
                    State = RecordState.NonModified,
                    SomeField1 = (long)i
                });
            }
            return result;
        }

        public static long SequentialReadPatternPoco(List<SizeBenchmark_Poco> list)
        {
            long sum = 0;
            int n = list.Count;
            for (int i = 0; i < n; ++i)
            {
                if (list[i].State == RecordState.Modified)
                    sum++;
            }
            return sum;
        }

        public static List<SizeBenchmark_Hot> SequentialReadPatternHot_Prepare(int size)
        {
            List<SizeBenchmark_Cold> colds = new List<SizeBenchmark_Cold>(size);
            for (int i = 0; i < size; ++i)
            {
                colds.Add(new SizeBenchmark_Cold()
                { 
                  SomeField1 = i
                });
            }

            List<SizeBenchmark_Hot> result = new List<SizeBenchmark_Hot>(size);
            for (int i = 0; i < size; ++i)
            {
                result.Add(new SizeBenchmark_Hot()
                {
                    Index = i,
                    //State = i % 4 == 0 ? RecordState.Modified : RecordState.NonModified,
                    State = RecordState.NonModified,
                    Cold = colds[i]
                });
            }
            return result;
        }

        public static long SequentialWritePatternHotCold(List<SizeBenchmark_Hot> listHot)
        {
            long sum = 0;
            int n = listHot.Count;
            for (int i = 0; i < n; ++i)
            {
                if (listHot[i].State == RecordState.Modified)
                    sum++;
            }
            return sum;
        }

        public static List<SizeBenchmark_PocoBig> SequentialReadPatternPocoBig_Prepare(int size)
        {
            List<SizeBenchmark_PocoBig> result = new List<SizeBenchmark_PocoBig>(size);
            for (int i = 0; i < size; ++i)
            {
                result.Add(new SizeBenchmark_PocoBig()
                {
                    Index = i,
                    //State = i % 4 == 0 ? RecordState.Modified : RecordState.NonModified,
                    State = RecordState.NonModified,
                    SomeField1 = (long)i
                });
            }
            return result;
        }

        public static long SequentialReadPatternPocoBig(List<SizeBenchmark_PocoBig> list)
        {
            long sum = 0;
            int n = list.Count;
            for (int i = 0; i < n; ++i)
            {
                var elem = list[i];
                if (elem.State == RecordState.Modified)
                    unchecked
                    {
                        sum += 1; //elem.SomeField1; 
                    }
            }
            return sum;
        }

    }

    public struct OneLineStruct
    {
        public long data1;
        public long data2;
        public long data3;
        public long data4;
        public long data5;
        public long data6;
        public long data7;
        public long data8;
    }

    public struct TwoLineStruct
    {
        public long data1;
        public long data2;
        public long data3;
        public long data4;
        public long data5;
        public long data6;
        public long data7;
        public long data8;
        public long data9;
    }

        public enum RecordState : int
        {
            NonModified,
            Modified
        }

        public class SizeBenchmark_Poco
        {
            public int Index;
            public RecordState State;
            public long SomeField1;
            public long SomeField2;
            public long SomeField3;
            public string SomeField4;
            public string SomeField5;
        }

    public class SizeBenchmark_PocoBig
    {
        public int Index;
        public RecordState State;
        public long SomeField1;
        public long SomeField2;
        public long SomeField3;
        public string SomeField4;
        public string SomeField5;
        public long SomeLong1;
        public long SomeLong2;
        public long SomeLong3;
        public long SomeLong4;
        public long SomeLong5;
        public long SomeLong6;
        public long SomeLong7;
        public long SomeLong8;
        public long SomeLong9;
        public long SomeLong10;
        public long SomeLong11;
        public long SomeLong12;
        public long SomeLong13;
        public long SomeLong14;
        public long SomeLong15;
        public long SomeLong16;
    }

        public class SizeBenchmark_Hot
        {
            public int Index;
            public RecordState State;
            public SizeBenchmark_Cold Cold;
        }

        public class SizeBenchmark_Cold
        {
            public long SomeField1;
            public long SomeField2;
            public long SomeField3;
            public string SomeField4;
            public string SomeField5;
            public string SomeField6;
            public string SomeField7;
        }

    public class FalseSharingTest
    {
        public static int[] counters = new int[4];
        public static int[] counters_better = new int[4*16];
        public static int[] counters_best = new int[4*16 + 16];

        const int offset = 1;
        const int startGap = 0;
        public static int[] sharedData = new int[4 * offset + startGap * offset];
        public static long DoFalseSharingTest(int threadsCount, int size = 100_000_000)
        {
            Thread[] workers = new Thread[threadsCount];
            for (int i = 0; i < threadsCount; ++i)
            {
                workers[i] = new Thread(new ParameterizedThreadStart(idx =>
                {
                    int index = (int)idx + startGap;
                    for (int j = 0; j < size; ++j)
                    {
                        counters[index * offset] = counters[index * offset] + 1;
                    }
                }));
            }
            for (int i = 0; i < threadsCount; ++i)
                workers[i].Start(i);
            for (int i = 0; i < threadsCount; ++i)
                workers[i].Join();
            return 0;
        }

        public static long DoTest(int size = 100000000)
        {
            Thread[] workers = new Thread[4];
            for (int i = 0; i < 4; ++i)
            {
                workers[i] = new Thread(new ParameterizedThreadStart(idx =>
                {
                    int index = (int)idx;
                    for (int j = 0; j < 100000000; ++j)
                    {
                        counters[index] = counters[index] + 1;
                    }
                }));
            }
            for (int i = 0; i < 4; ++i)
                workers[i].Start(i);
            for (int i = 0; i < 4; ++i)
                workers[i].Join();
            return 0;
        }

        public static long DoTestBetter(int size = 100000000)
        {
            Thread[] workers = new Thread[4];
            for (int i = 0; i < 4; ++i)
            {
                workers[i] = new Thread(new ParameterizedThreadStart(idx =>
                {
                    int index = (int)idx;
                    for (int j = 0; j < 100000000; ++j)
                    {
                        counters_better[index*16] = counters_better[index*16] + 1;
                    }
                }));
            }
            for (int i = 0; i < 4; ++i)
                workers[i].Start(i);
            for (int i = 0; i < 4; ++i)
                workers[i].Join();
            return 0;
        }

        public static long DoTestBest(int size = 100000000)
        {
            Thread[] workers = new Thread[4];
            for (int i = 0; i < 4; ++i)
            {
                workers[i] = new Thread(new ParameterizedThreadStart(idx =>
                {
                    int index = (int)idx + 1;
                    for (int j = 0; j < 100000000; ++j)
                    {
                        counters_best[index*16] = counters_best[index*16] + 1;
                    }
                }));
            }
            for (int i = 0; i < 4; ++i)
                workers[i].Start(i);
            for (int i = 0; i < 4; ++i)
                workers[i].Join();
            return 0;
        }
    }
}