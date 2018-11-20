using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;

/*
          Method |     Mean |     Error |    StdDev | Allocated |
---------------- |---------:|----------:|----------:|----------:|
 FindValueReturn | 813.2 ns |  9.060 ns |  8.031 ns |       0 B |
 FindByRefReturn | 767.4 ns | 13.460 ns | 12.591 ns |       0 B | 
 
 */

namespace Benchmarks.Tests
{
    [CoreJob]
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(printAsm: true, printSource: true, printIL: true)]
    public class MatrixFind
    {
        private int[,] matrix;

        [GlobalSetup]
        public void Setup()
        {
            this.matrix = new int[10,10];
            for (int i = 0; i < 10; ++i)
            for (int j = 0; j < 10; ++j)
                this.matrix[i, j] = i * j;
        }

        [Benchmark]
        public (int, int) FindValueReturn()
        {
            var result = Find(this.matrix, x => x == 49);
            this.matrix[result.i, result.j] = 49;
            return result;
        }

        [Benchmark]
        public ref int FindByRefReturn()
        {
            ref var result = ref FindByRefReturn(this.matrix, x => x == 49);
            result = 49;
            return ref result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static (int i, int j) Find(int[,] matrix, Func<int, bool> predicate)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            for (int j = 0; j < matrix.GetLength(1); j++)
                if (predicate(matrix[i, j]))
                    return (i, j);
            return (-1, -1); // Not found
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ref int FindByRefReturn(int[,] matrix, Func<int, bool> predicate)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            for (int j = 0; j < matrix.GetLength(1); j++)
                if (predicate(matrix[i, j]))
                    return ref matrix[i, j];
            throw new InvalidOperationException("Not found");
        }
    }
}
