using System;
using System.Threading;

namespace CoreCLR.EagerRootCollection
{
    class Program
    {
        ///////////////////////////////////////////////////////////////////////
        // Listings 8-12, 8-16
        static void Main2(string[] args)
        {
            Timer t = new Timer((obj) => Console.WriteLine(DateTime.Now.ToString()), null, 0, 100);
            Console.WriteLine("Hello World!");
            GC.Collect();
            Console.ReadKey();
            //GC.KeepAlive(t);
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 8-13
        static void Main1(string[] args)
        {
            SomeClass sc = new SomeClass();
            sc.DoSomething("Hello world!");
            Console.ReadKey();
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 8-14
        static void Main(string[] args)
        {
            SomeClass sc = new SomeClass() {Field = new Random().Next()};
            sc.DoSomething("Number: ");
            Console.ReadKey();
        }
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 8-13
    class SomeClass
    {
        public int Field;

        public void DoSomething(string msg)
        {
            GC.Collect();
            Console.WriteLine(msg);
        }
        public void DoSomethingElse()
        {
            Console.WriteLine(this.Field.ToString());
        }

        ~SomeClass()
        {
            Console.WriteLine("Killing...");
        }
    }
}
