using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemoryLeaks.Leaks
{
    class LOHFragmentationLeak : IMemoryLeakExample
    {
        static byte[] block;

        public void Run()
        {
            int blockSize = 10_000_000;
            int blockIncrease = 1000;
            List<byte[]> smallBlocks = new List<byte[]>(100_000);
            while (true)
            {
                GC.Collect(2, GCCollectionMode.Forced, true);
                block = new byte[blockSize];
                Console.WriteLine(GC.GetGeneration(block));
                var smallBlock = new byte[85_000];
                smallBlocks.Add(smallBlock);
                Console.WriteLine(GC.GetGeneration(smallBlock));
                Thread.Sleep(100);
                blockSize += blockIncrease;
            }
        }
    }
}
