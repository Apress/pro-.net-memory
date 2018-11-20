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
 
            Console.WriteLine("Benchmark finished. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
