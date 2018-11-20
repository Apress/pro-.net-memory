using System;
using System.Runtime.CompilerServices;

namespace CoreCLR.DependentHandles
{
    class Program
    {
        static void Main(string[] args)
        {
            //Leak();
            //SimpleUsage1();
            //SimpleUsage2();
            //SimpleUsage3();
            //MultipleUsage();
            FinalizationUsage();
        }

        public static void Leak()
        {
            int counter = 0;
            try
            {
                object key = new object();
                while (true)
                {
                    var table = new ConditionalWeakTable<object, Tuple<object, byte[]>>();
                    table.Add(key, new Tuple<object, byte[]>(table, new byte[10_000_000]));
                    GC.Collect();
                    //GC.WaitForPendingFinalizers();
                    //GC.Collect();
                    counter++;
                }
            }
            catch (OutOfMemoryException e)
            {
                Console.WriteLine(counter);
                throw;
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // Listings 13-1
        public static void SimpleConditionalWeakTableUsage()
        {
            // Dependent handles between SomeClass (primary) and SomeData (secondary)
            ConditionalWeakTable<SomeClass, SomeData> weakTable = new ConditionalWeakTable<SomeClass, SomeData>();

            var obj1 = new SomeClass();
            var data1 = new SomeData();

            var obj1weakRef = new WeakReference(obj1);
            var data1weakRef = new WeakReference(data1);
            weakTable.Add(obj1, data1); // Throws an exception if key already added
            weakTable.AddOrUpdate(obj1, data1);

            GC.Collect();
            Console.WriteLine($"{obj1weakRef.IsAlive} {data1weakRef.IsAlive}");   // Prints True True
            if (weakTable.TryGetValue(obj1, out var value))
            {
                Console.WriteLine(value.Data);
            }
            GC.KeepAlive(obj1);
            GC.Collect();
            Console.WriteLine($"{obj1weakRef.IsAlive} {data1weakRef.IsAlive}"); // Prints False False
        }


        public static void SimpleUsage1()
        {
            ConditionalWeakTable<SomeClass, SomeData> weakTable = new ConditionalWeakTable<SomeClass, SomeData>();

            var obj1 = new SomeClass();
            var data1 = new SomeData();
            
            weakTable.Add(obj1, data1); // Throws an exception if key already added
            weakTable.AddOrUpdate(obj1, data1);
            
            if (weakTable.TryGetValue(obj1, out var value))
            {
                Console.WriteLine(value.Data);
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 13-2
        public static void SimpleUsage2()
        {
            ConditionalWeakTable<object, object> weakTable = new ConditionalWeakTable<object, object>();

            var obj1 = new SomeClass();
            var data1 = new SomeData();
            weakTable.Add(obj1, data1);
            
        }

        public static void SimpleUsage3()
        {
            ConditionalWeakTable<SomeClass, SomeData> weakTable = new ConditionalWeakTable<SomeClass, SomeData>();

            var obj1 = new SomeClass();
            var data1 = new SomeData();

            var obj1weakRef = new WeakReference(obj1);
            var data1weakRef = new WeakReference(data1);
            weakTable.Add(obj1, data1);

            GC.Collect();
            Console.WriteLine($"{obj1weakRef.IsAlive} {data1weakRef.IsAlive}");
            GC.KeepAlive(obj1);
            GC.Collect();
            Console.WriteLine($"{obj1weakRef.IsAlive} {data1weakRef.IsAlive}");
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 13-3
        public static void MultipleUsage()
        {
            var obj1 = new SomeClass();
            var weakTable1 = new ConditionalWeakTable<object, object>();
            var weakTable2 = new ConditionalWeakTable<object, object>();
            var data1 = new SomeData();
            var data2 = new SomeData();
            weakTable1.Add(obj1, data1);
            weakTable2.Add(obj1, data2);
            Console.ReadLine();
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 13-4
        public static void FinalizationUsage()
        {
            ConditionalWeakTable<SomeClass, SomeData> weakTable = new ConditionalWeakTable<SomeClass, SomeData>();

            var obj1 = new FinalizableClass();
            var data1 = new SomeData();

            var obj1weakRef = new WeakReference(obj1, trackResurrection: true);
            var data1weakRef = new WeakReference(data1, trackResurrection: true);
            weakTable.Add(obj1, data1);

            GC.Collect();
            Console.WriteLine($"{obj1weakRef.IsAlive} {data1weakRef.IsAlive}");
            GC.KeepAlive(obj1);
            GC.Collect();
            //GC.WaitForPendingFinalizers();
            //GC.Collect();
            Console.WriteLine($"{obj1weakRef.IsAlive} {data1weakRef.IsAlive}");
        }
    }

    class SomeClass
    {
        public int Field;
    }

    class SomeData
    {
        public int Data;
    }

    class FinalizableClass : SomeClass
    {
        ~FinalizableClass()
        {
            Console.WriteLine("X");
        }
    }
}
