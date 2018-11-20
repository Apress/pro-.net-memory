using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CoreCLR.Finalization
{
    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-13
    public class LeakyApplication
    {
        public void Run()
        {
            //List<WeakReference> observer = new List<WeakReference>();
            while (true)
            {
                Thread.Sleep(100);
                var obj = new EvilFinalizableClass(10, 10000);
                //observer.Add(new WeakReference(obj));
                GC.KeepAlive(obj);
                long counter = 0;
                //foreach (var weakReference in observer)
                //{
                //    if (weakReference.IsAlive)
                //        counter++;
                //}
                //if (counter > 10)
                GC.Collect();
                //Console.WriteLine("Hello");
            }
        }
    }

    public class EvilFinalizableClass
    {
        private readonly int finalizationDelay;

        public EvilFinalizableClass(int allocationDelay, int finalizationDelay)
        {
            this.finalizationDelay = finalizationDelay;
            Thread.Sleep(allocationDelay);
        }

        ~EvilFinalizableClass()
        {
            Thread.Sleep(finalizationDelay);
        }
    }
}
