using System;

namespace CoreCLR.Lifetime
{
    internal class GcIsWeird
    {
        ~GcIsWeird()
        {
            Console.WriteLine("Finalizing instance.");
        }

        public int data = 42;

        public void DoSomething()
        {
            Console.WriteLine("Doing something. The answer is ... " + data);
            CheckReachability(this);
            Console.WriteLine("Finished doing something.");
        }

        static void CheckReachability(object d)
        {
            var weakRef = new WeakReference(d);
            Console.WriteLine("Calling GC.Collect...");

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            string message = weakRef.IsAlive ? "alive" : "dead";
            Console.WriteLine("Object is " + message);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            new GcIsWeird().DoSomething();
        }
    }
}
