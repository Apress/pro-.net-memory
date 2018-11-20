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
    public class LINQ
    {
        string[] strings =
        {
            "A penny saved is a penny earned.",
            "The early bird catches the worm.",
            "The pen is mightier than the sword."
        };

        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark]
        public IEnumerable<string> LINQQuerySyntax()
        {
            var linq =
                from sentence in strings
                let words = sentence.Split(' ')
                from word in words
                let w = word.ToLower()
                where w[0] == 'a' || w[0] == 'e'
                      || w[0] == 'i' || w[0] == 'o'
                      || w[0] == 'u'
                select word;
            return linq;
        }

        [Benchmark]
        public IEnumerable<string> LINQMethodSyntax()
        {
            var linq = strings.Select(sentence => new
                {
                    sentence = sentence,
                    words = sentence.Split()
                })
                .SelectMany(x => x.words, (x, word) => new {x = x, word = word})
                .Select(x => new {x = x, w = x.word.ToLower()})
                .Where(x => x.w[0] == 'a' || x.w[0] == 'e' || x.w[0] == 'i' || x.w[0] == 'o' || x.w[0] == 'u')
                .Select(x => x.x.word);
            return linq;
        }

        [Benchmark]
        public IEnumerable<string> LINQMethodSyntax2()
        {
            var linq = strings.Select(sentence => new
                {
                    sentence = sentence,
                    words = sentence.Split()
                })
                .SelectMany(x => x.words, (x, word) => new {x = x, word = word})
                .Select(x => new {x = x, w = x.word.ToLower()})
                .Where(x => x.w[0] == 'a' || x.w[0] == 'e' || x.w[0] == 'i' || x.w[0] == 'o' || x.w[0] == 'u')
                .Select(x => x.x.word);
            return linq;
        }
    }

    public class SomeOtherClass
    {
        public int X;
        public int Y;
    }
}
