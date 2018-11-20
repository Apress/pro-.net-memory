using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MemoryLeaksLibrary;

namespace MemoryLeaks.Leaks
{
    /*
     * Pure unmanaged leak - some unmanaged component does not free used memory.
     */
    class UnmanagedLeak : IMemoryLeakExample
    {
        public void Run()
        {
            GC.Collect();
            SomeService worker = new SomeService();
            while (true)
            {
                worker.DoSomeProcessing();
                Thread.Sleep(100);
            }
        }
    }

    class SomeService
    {
        readonly MemoryLeaksLibrary.Class1 library = new Class1();

        public void DoSomeProcessing()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var result = library.DoSomething(10_000);
            sw.Stop();
            long elapsedMilliseconds = sw.ElapsedMilliseconds;
            Console.WriteLine($"Work done in {elapsedMilliseconds}");
        }
    }
}
