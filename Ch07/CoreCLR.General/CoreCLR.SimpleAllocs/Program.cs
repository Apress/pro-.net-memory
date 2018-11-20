using System;

namespace CoreCLR.SimpleAllocs
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
            Console.WriteLine("Hello, Windows");
            Console.WriteLine("Love from CoreCLR.");

            GC.Collect(); // Here GC will only sweep, we can alloc 29720 bytes to see what happen

            Console.ReadLine();

            const int LEN = 1_000_000;
            byte[][] list = new byte[LEN][];
            for (int i = 0; i < LEN; ++i)
            {
                list[i] = new byte[25000];
                if (i % 100 == 0)
                {
                    Console.WriteLine("Allocated 100 arrays");
                    //Console.ReadKey();
                    //Thread.Sleep(100);
                }
            }

            int[] array = new int[3] { 100, 101, 102 };
            Console.WriteLine(array[2]);
        }
    }
}
