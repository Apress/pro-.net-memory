using System;
using System.Diagnostics;
using System.Threading;
using BenchmarkDotNet.Running;
using Benchmarks.Tests;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var benchmark = BenchmarkRunner.Run<StructVsClassTest>();
            //var test = new StringInterningOptimization();
            //while (true)
            //{
            //    int result = test.GroupByCommand();
            //    Console.WriteLine(result);
            //    Thread.Sleep(100);
            //}
            #region Finalization benchmark
            /*
            int value = int.Parse(Console.ReadLine());
            Stopwatch st = new Stopwatch();
            st.Start();
            for (int i = 0; i < 1_000_000; ++i)
            {
                Finalization.NonFinalizableClass obj = new Finalization.NonFinalizableClass();
                obj.Value1 = value;
                GC.KeepAlive(obj);
            }
            Console.WriteLine(st.ElapsedTicks);
            
            st.Restart();
            for (int i = 0; i < 1_000_000; ++i)
            {
                Finalization.FinalizableClass obj = new Finalization.FinalizableClass();
                obj.Value1 = value;
                GC.KeepAlive(obj);
            }
            Console.WriteLine(st.ElapsedTicks);
            */
            #endregion

            Console.WriteLine("Benchmark finished. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
