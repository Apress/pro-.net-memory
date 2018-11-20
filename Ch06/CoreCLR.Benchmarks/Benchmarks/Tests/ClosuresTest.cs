using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;

namespace Benchmarks.Tests
{

    [CoreJob]
    [MemoryDiagnoser]
    public class ClosuresTest
    {
        private List<int> _list;
        int value = 50;

        [GlobalSetup]
        public void Setup()
        {
            _list = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                _list.Add(i);
            }
        }

        [Benchmark]
        public IEnumerable<string> Closures()
        {
            var filteredList = _list.Where(x => x > value);
            var result = filteredList.Select(x => x.ToString());
            return result.ToList();
        }

        [Benchmark]
        public IEnumerable<string> WithoutClosures()
        {
            List<string> result = new List<string>();
            foreach (int x in _list)
                if (x > value)
                    result.Add(x.ToString());
            return result;
        }
    }
}
