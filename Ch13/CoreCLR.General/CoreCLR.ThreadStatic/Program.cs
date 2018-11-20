using System;
using System.Threading;

namespace CoreCLR.ThreadStatic
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            SomeClass.UseNamedSlots();
            SomeClass.UseUnnamedSlot();
            //ThreadLocal<string>
            Console.ReadLine();

            /*
            SomeClass runner = new SomeClass();
            Thread t1 = new Thread(new ParameterizedThreadStart(runner.Run));
            t1.Start(1);
            Thread t2 = new Thread(new ParameterizedThreadStart(runner.Run));
            t2.Start(2);
            */

            SomeOtherClass runner = new SomeOtherClass();
            Thread t1 = new Thread(runner.Run);
            t1.Start();
            //Console.ReadLine();
            Thread t2 = new Thread(runner.Run);
            t2.Start();

        }
    }

    class GenericClass<T>
    {
        [ThreadStatic]
        private static int threadData;
    }

    ///////////////////////////////////////////////////////////////////////
    // Listings 13-9, 13-10
    class SomeOtherClass
    {
        [ThreadStatic] private static int threadStaticValueData = 44;
        private ThreadLocal<int> threadValueLocal = new ThreadLocal<int>(() => 44, trackAllValues: true);

        [ThreadStatic]
        private static int? threadStaticData;
        public static int ThreadStaticData
        {
            get
            {
                if (threadStaticData == null)
                    threadStaticData = 44;
                return threadStaticData.Value;
            }
        }

        public void Run()
        {
            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine($"Worker {threadStaticValueData}:{threadValueLocal.Value}:{ThreadStaticData}.");
                Console.WriteLine(threadValueLocal.Values.Count);
                threadValueLocal.Value = threadValueLocal.Value + 1;
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 13-8
    class SomeData
    {
        public int Field;
    }

    class SomeClass
    {
        [ThreadStatic]
        private static int threadStaticValueData;
        [ThreadStatic]
        private static SomeData threadStaticReferenceData;
        [ThreadStatic] private static SomeData Data2;

        private ThreadLocal<int> threadValueLocal = new ThreadLocal<int>(true);

        public void Run(object param)
        {
            int arg = int.Parse(param.ToString());
            threadStaticValueData = arg;
            threadStaticReferenceData = new SomeData() { Field = arg };
            threadValueLocal.Value = arg;
            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine($"Worker {threadStaticValueData}:{threadStaticReferenceData.Field}:{threadValueLocal.Value}.");
                Console.WriteLine(threadValueLocal.Values.Count);
            }
        }

        public static void UseNamedSlots()
        {
            Thread.AllocateNamedDataSlot("SlotName");
            Thread.SetData(Thread.GetNamedDataSlot("SlotName"), new SomeData());

            object data = Thread.GetData(Thread.GetNamedDataSlot("SlotName"));
            Console.WriteLine(data);
            Thread.FreeNamedDataSlot("SlotName");
        }

        public static void UseUnnamedSlot()
        {
            LocalDataStoreSlot slot = Thread.AllocateDataSlot();
            Thread.SetData(slot, new SomeData());

            object data = Thread.GetData(slot);
            Console.WriteLine(data);
        }
    }

    struct SomeStruct
    {
        [ThreadStatic] public static int Data;
    }
}
