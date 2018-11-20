using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CoreCLR.ManagedPointers
{
    class Program
    {
        public static void Main(string[] args)
        {
            var sc = new SomeClass();
            Console.WriteLine(sc.Process(10));
            var x = typeof(SomeClass).GetMethod("Process", BindingFlags.Instance | BindingFlags.Public);
            Console.WriteLine("{0:X16}", GetAddressOf(sc));
            Console.WriteLine("{0:X16}", AddressByPin(sc));
            Console.ReadKey();
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 13-21
        public static void UsingRefLocal(SomeClass data)
        {
            ref int refLocal = ref data.Field;
            refLocal = 2;
        }

        public static void Inc(TypedReference value)
        {
            __refvalue(value, int)++;
        }

        //This bypasses the restriction that you can't have a pointer to T,
        //letting you write very high-performance generic code.
        //It's dangerous if you don't know what you're doing, but very worth if you do.
        static T Read<T>(IntPtr address)
        {
            var obj = default(T);
            var tr = __makeref(obj);

            //This is equivalent to shooting yourself in the foot
            //but it's the only high-perf solution in some cases
            //it sets the first field of the TypedReference (which is a pointer)
            //to the address you give it, then it dereferences the value.
            //Better be 10000% sure that your type T is unmanaged/blittable...
            unsafe { *(IntPtr*)(&tr) = address; }

            return __refvalue(tr, T);
        }

        static unsafe ulong GetAddressOf<T>(T obj) where T : class
        {
            var tr = __makeref(obj);
            return (ulong) **(IntPtr**) (&tr);
        }

        public static ulong AddressByPin<T>(T obj)
        {
            GCHandle handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
            try
            {
                return (ulong)handle.AddrOfPinnedObject();
            }
            finally
            {
                handle.Free();
            }
        }
    }

    class SomeClass
    {
        public int Field;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public int Process(int x)
        {
            return x;
        }
    }
}
