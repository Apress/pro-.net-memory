using BenchmarkDotNet.Attributes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes.Jobs;
using Newtonsoft.Json;

namespace Benchmarks.Tests
{
    /*
            Method |     Mean |     Error |    StdDev |  Gen 0 | Allocated |
------------------ |---------:|----------:|----------:|-------:|----------:|
         ReadPlain | 14.58 us | 0.2937 us | 0.6923 us | 1.4648 |    6.1 KB |
 ReadWithArrayPool | 13.37 us | 0.2593 us | 0.3462 us | 1.0681 |   4.42 KB |
 */

    [CoreJob]
    [MemoryDiagnoser]
    public class JsonArrayPoolTest
    {
        private string Input;

        [GlobalSetup]
        public void Setup()
        {
            // Warm up pool
            var array = ArrayPool<char>.Shared.Rent(4000);
            var array2 = ArrayPool<char>.Shared.Rent(4 * 1024 * 1024);
            ArrayPool< char>.Shared.Return(array);

            var numbers = string.Join(",", Enumerable.Range(1, 100).Select(i => i.ToString()));
            Input = string.Format(@"[{0}]", numbers);
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-19
        [Benchmark]
        public IList<int> ReadPlain()
        {
            IList<int> value;

            JsonSerializer serializer = new JsonSerializer();
            using (JsonTextReader reader = new JsonTextReader(new StringReader(Input)))
            {
                value = serializer.Deserialize<IList<int>>(reader);
                return value;
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-20
        [Benchmark]
        public int[] ReadWithArrayPool()
        {
            JsonSerializer serializer = new JsonSerializer();
            using (JsonTextReader reader = new JsonTextReader(new StringReader(Input)))
            {
                // reader will get buffer from array pool
                reader.ArrayPool = JsonArrayPool.Instance;

                var value = serializer.Deserialize<int[]>(reader);
                return value;
            }
        }

        public class JsonArrayPool : IArrayPool<char>
        {
            public static readonly JsonArrayPool Instance = new JsonArrayPool();

            public char[] Rent(int minimumLength)
            {
                // get char array from System.Buffers shared pool
                return ArrayPool<char>.Shared.Rent(minimumLength);
            }

            public void Return(char[] array)
            {
                // return char array to System.Buffers shared pool
                ArrayPool<char>.Shared.Return(array);
            }
        }
    }
}
