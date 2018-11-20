using System;
using System.Collections.Generic;
using System.Runtime;
using System.Threading;

namespace CoreCLR.LOHFragmentation
{
    /*
     * Workstation GC
     * 
     * #GC  Before      After       Time [ms]
     * 16   289.739     168.957     60.378
     * 17   344.114     222.627     85.147
     * 20   947.216     572.462     206.320
     */
    class Program
    {
        ///////////////////////////////////////////////////////////////////////
        // Listing 10-2
        static void Main(string[] args)
        {
            int ObjectSize = int.Parse(args[0]);
            const long SizeThreshold = 4L * 1024 * 1024 * 1024;
            Console.WriteLine($"Running with {ObjectSize:N0} object size...");

            var random = new Random();
            var reader = new Reader();
            var processor = new Processor();
            int counter = 0;
            List<byte[]> smallBlocks = new List<byte[]>();
            while (true)
            {
                var modifier = counter++; //random.Next(-1000, 1000);
                var size = 90_000 + modifier;
                var frame = reader.ReadBytes(size);
                var output = processor.Process(frame);
                var result = BitConverter.ToDouble(output.Data, 0);
                Console.WriteLine($"Processsed frame - input {frame.Lenght}B, output {output.Lenght}, result {result}");
                var smallBlock = new byte[ObjectSize];
                smallBlocks.Add(smallBlock);

                int count = smallBlocks.Count;
                if (smallBlocks.Count % 1000 == 0)
                {
                    Console.WriteLine($"Created {count} objects with total size {count * ObjectSize:N0}");
                    //Console.ReadLine();
                }

                if (GC.GetTotalMemory(false) > SizeThreshold)
                {
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect();
                    Console.WriteLine($"Created {count} objects with total size {count * ObjectSize:N0}");
                    Console.ReadLine();
                    break;
                }
            }
            GC.KeepAlive(smallBlocks);
        }
    }

    class Reader
    {
        Random rand = new Random();

        public DataFrame ReadBytes(int size)
        {
            var input = new byte[size];
            rand.NextBytes(input);
            var result = new DataFrame(input);
            return result;
        }
    }

    class Processor
    {
        public DataFrame Process(DataFrame input)
        {
            DataFrame result = new DataFrame(input.Lenght + sizeof(double));
            double avg = 0.0;
            for (int i = 0; i < input.Lenght; ++i)
                avg += input.Data[i];
            avg /= input.Lenght;
            byte[] avgByteArray = BitConverter.GetBytes(avg);
            Array.Copy(avgByteArray, result.Data, avgByteArray.Length);
            Array.Copy(input.Data, 0, result.Data, avgByteArray.Length, input.Lenght);
            Thread.Sleep(10);
            return result;
        }
    }

    class DataFrame
    {
        private byte[] buffer;

        public DataFrame(int size)
        {
            this.buffer = new byte[size];
        }

        public DataFrame(byte[] input)
        {
            this.buffer = input;
        }

        public byte[] Data => buffer;

        public int Lenght => buffer?.Length ?? 0;
    }
}
