using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCLR.CollectScenarios.Scenarios
{
    /*
    Shows padding between LOH objects for LOH compacting:

    Before GC:
    0:006> !dumpheap 000001a1f5991000  000001a1f5a898f0
             Address               MT     Size
    000001a1f5991000 000001a1e3cdd1b0       24 Free
    000001a1f5991018 000001a1e3cdd1b0       30 Free
    000001a1f5991038 00007ff8a35a2c00     8184     
    000001a1f5993030 000001a1e3cdd1b0       30 Free
    000001a1f5993050 00007ff8a35a2c00     1048     
    000001a1f5993468 000001a1e3cdd1b0       30 Free
    000001a1f5993488 00007ff8a35a2c00     8184     
    000001a1f5995480 000001a1e3cdd1b0       30 Free
    000001a1f59954a0 00007ff8a35a2fa8   100024     
    000001a1f59adb58 000001a1e3cdd1b0       30 Free
    000001a1f59adb78 00007ff8a35a2fa8   100024     
    000001a1f59c6230 000001a1e3cdd1b0       30 Free
    000001a1f59c6250 00007ff8a35a2fa8   100024     
    000001a1f59de908 000001a1e3cdd1b0       30 Free
    000001a1f59de928 00007ff8a35a2fa8   100024     
    000001a1f59f6fe0 000001a1e3cdd1b0       30 Free
    000001a1f59f7000 00007ff8a35a2fa8   100024     
    000001a1f5a0f6b8 000001a1e3cdd1b0       30 Free
    000001a1f5a0f6d8 00007ff8a35a2fa8   100024     
    000001a1f5a27d90 000001a1e3cdd1b0       30 Free
    000001a1f5a27db0 00007ff8a35a2fa8   100024     
    000001a1f5a40468 000001a1e3cdd1b0       30 Free
    000001a1f5a40488 00007ff8a35a2fa8   100024     
    000001a1f5a58b40 000001a1e3cdd1b0       30 Free
    000001a1f5a58b60 00007ff8a35a2fa8   100024     
    000001a1f5a71218 000001a1e3cdd1b0       30 Free
    000001a1f5a71238 00007ff8a35a2fa8   100024 

    After GC:
    0:006> !dumpheap 000001a1f5991000  000001a1f5a898f0
             Address               MT     Size
    000001a1f5991000 000001a1e3cdd1b0       24 Free
    000001a1f5991018 000001a1e3cdd1b0       30 Free
    000001a1f5991038 00007ff8a35a2c00     8184     
    000001a1f5993030 000001a1e3cdd1b0       30 Free
    000001a1f5993050 00007ff8a35a2c00     1048     
    000001a1f5993468 000001a1e3cdd1b0       30 Free
    000001a1f5993488 00007ff8a35a2c00     8184     
    000001a1f5995480 000001a1e3cdd1b0       30 Free
    000001a1f59954a0 00007ff8a35a2fa8   100024     
    000001a1f59adb58 000001a1e3cdd1b0       30 Free
    000001a1f59adb78 00007ff8a35a2fa8   100024     
    000001a1f59c6230 000001a1e3cdd1b0       30 Free
    000001a1f59c6250 00007ff8a35a2fa8   100024     
    000001a1f59de908 000001a1e3cdd1b0       30 Free
    000001a1f59de928 00007ff8a35a2fa8   100024     
    000001a1f59f6fe0 000001a1e3cdd1b0       30 Free
    000001a1f59f7000 00007ff8a35a2fa8   100024     
    000001a1f5a0f6b8 000001a1e3cdd1b0   200142 Free
    000001a1f5a40488 00007ff8a35a2fa8   100024     
    000001a1f5a58b40 000001a1e3cdd1b0   100086 Free
    000001a1f5a71238 00007ff8a35a2fa8   100024   
    */
    public class LOHPadding : ICollectScenario
    {
        public int Run()
        {
            Console.WriteLine("Creating 10 big arrays in LOH.");
            for (int i = 0; i < 10; ++i)
            {
                byte[] bigArray = new byte[100_000];
                list.Add(bigArray);
            }
            Console.WriteLine("Just before clearing some items and blocking compacting GC");
            Console.ReadLine();
            list[5] = null;
            list[6] = null;
            list[8] = null;
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            Console.WriteLine("After GC...");
            Console.ReadLine();

            return list.Count;
        }

        private List<byte[]> list = new List<byte[]>();
    }
}
