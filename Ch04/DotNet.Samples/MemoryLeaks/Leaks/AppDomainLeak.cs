using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemoryLeaks.Leaks
{
    /*
     * AppDomains creation without unloading.
     * VMMap: Managed Heap growth (domains), Private Data growth (including Execute/Read/Write segments for JITted code.
     */
    class AppDomainLeak : IMemoryLeakExample
    {
        public void Run()
        {
            while (true)
            {
                var appDomain = AppDomain.CreateDomain($"AppDomain{DateTime.Now.Ticks}");
                var str = appDomain.CreateInstanceAndUnwrap(typeof(Worker).Assembly.FullName, typeof(Worker).FullName) as Worker;
                str.PrintDomain();
                Thread.Sleep(1000);
            }
        }
    }

    public class Worker : MarshalByRefObject
    {
        public void PrintDomain()
        {
            Console.WriteLine($"Object is executing in AppDomain \"{AppDomain.CurrentDomain.FriendlyName}\"");
        }
    }
}
