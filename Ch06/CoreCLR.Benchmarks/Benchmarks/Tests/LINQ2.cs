using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;

namespace Benchmarks.Tests
{
    [CoreJob]
    [MemoryDiagnoser]
    public class LINQ2
    {
        private string[] strings =
        {
            "A", "penny", "saved", "is", "a", "penny", "earned.", "The", "early", "bird", "catches", "the", "worm.",
            "The", "pen", "is", "mightier", "than", "the", "sword."
        };

        private int min = 3;
        private int max = 5;

        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark]
        public IEnumerable<string> FilterStrings_WithLINQ()
        {
            var results = strings.Where(x => x.Length >= min && x.Length <= max)
                .Select(x => x.ToLower());
            return results;
        }

        [Benchmark]
        public IEnumerable<string> FilterStrings_WithoutLINQ()
        {
            //IEnumerable<string> Helper()
            //{
                foreach (var word in strings)
                {
                    if (word.Length >= min && word.Length <= max)
                        yield return word.ToLower();
                }
           // }
            //return Helper().ToArray();
        }
    }
}
