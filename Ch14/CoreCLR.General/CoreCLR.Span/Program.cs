using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace CoreCLR.SpanOfT
{
    class Program
    {
        static void Main(string[] args)
        {
            string str = "Hello world!";
            Span<byte> span = new Span<byte>();
            Console.WriteLine("Hello World!");
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 14-1
        unsafe public static void UseSpan()
        {
            var array = new int[64];
            Span<int> span1 = new Span<int>(array);
            Span<int> span2 = new Span<int>(array, start: 8, length: 4);
            Span<int> span3 = span1.Slice(0, 4);

            Span<int> span4 = stackalloc[] { 1, 2, 3, 4, 5 };
            Span<int> span5 = span4.Slice(0, 2);

            IntPtr memory = Marshal.AllocHGlobal(64);
            void* ptr = memory.ToPointer();
            Span<byte> span6 = new Span<byte>(ptr, 64);
            Marshal.FreeHGlobal(memory);

            var span = span1; // or span2, span3, ...
            for (int i = 0; i < span.Length; i++)
                Console.WriteLine(span[i]);

            Marshal.FreeHGlobal(memory);
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 14-2
        public static void UseReadOnlySpan()
        {
            var array = new int[64];
            ReadOnlySpan<int> span1 = new ReadOnlySpan<int>(array);
            ReadOnlySpan<int> span2 = new Span<int>(array);

            string str = "Hello world";
            ReadOnlySpan<char> span3 = str.AsSpan();
            ReadOnlySpan<char> span4 = str.AsSpan(start: 6, length: 5);
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 14-5
        public Span<int> ReturnArrayAsSpan()
        {
            var array = new int[64];
            return new Span<int>(array);
        }

        public unsafe Span<int> ReturnStackallocAsSpan()
        {
            //Span<int> span = stackalloc[] { 1, 2, 3, 4, 5 }; // Compilation Error CS8352: Cannot use local 'span' in this context because it may expose referenced variables outside of their declaration scope
            //return span;
            return new Span<int>();
        }

        public unsafe Span<int> ReturnNativeAsSpan()
        {
            IntPtr memory = Marshal.AllocHGlobal(64);
            return new Span<int>(memory.ToPointer(), 8);
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 14-13
        private const int StackAllocSafeThreshold = 128;
        public void UseSpanNotWisely(int size)
        {
            //Span<int> span = size < StackAllocSafeThreshold ? stackalloc int[size] : ArrayPool<int>.Shared.Rent(size);
            //for (int i = 0; i < size; ++i)
            //    Console.WriteLine(span[i]);
            //ArrayPool<int>.Shared.Return(??);
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 14-14
        public unsafe void UseSpanWisely(int size)
        {
            int* ptr = default;
            int[] array = null;
            if (size < StackAllocSafeThreshold)
            {
                int* localPtr = stackalloc int[size];
                ptr = localPtr;
            }
            else
            {
                array = ArrayPool<int>.Shared.Rent(size);
            }
            Span<int> span = array ?? new Span<int>(ptr, size);
            for (int i = 0; i < size; ++i)
                Console.WriteLine(span[i]);
            if (array != null) ArrayPool<int>.Shared.Return(array);
        }

    }
}
