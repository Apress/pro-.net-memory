using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCLR.CollectScenarios.Scenarios
{
    /*
    Shows creating large gap before pinned object:

    000001d750da6558 00007ff8a35b7640      152 System.RuntimeType+ActivatorCacheEntry[]
    000001d750da65f0 00007ff8a35e34a8       48 System.RuntimeType+ActivatorCacheEntry
    000001d750da6620 00007ff8a35d7b40       74 "Creating 10 small arrays"
    000001d750da6670 00007ff8a35d7b40       94 "Just before blocking compacting GC"
    000001d750da66d0 00007ff8a35d7b40       48 "After GC..."
    000001d750da6700 00007ff8a35a2fa8     1024 System.Byte[]    
    000001d750da6b00 00007ff8a35c01a0       56 System.Byte[][]    
    000001d750da6b38 00007ff8a35a2fa8     1024 System.Byte[]    
    000001d750da6f38 00007ff8a35a2fa8     1024 System.Byte[]    
    000001d750da7338 00007ff8a35a2fa8     1024 System.Byte[]    
    000001d750da7738 00007ff8a35a2fa8     1024 System.Byte[]    
    000001d750da7b38 00007ff8a35c01a0       88 System.Byte[][] 
    000001d750da7b90 00007ff8a35a2fa8     1024 System.Byte[]    
    000001d750da7f90 00007ff8a35a2fa8     1024 System.Byte[]    
    000001d750da8390 00007ff8a35a2fa8     1024 System.Byte[] (pinned)    
    000001d750da8790 00007ff8a35a2fa8     1024 System.Byte[]    
    000001d750da8b90 00007ff8a35c01a0      152 System.Byte[][]
    000001d750da8c28 00007ff8a35a2fa8     1024 System.Byte[]  

   
    000001d750d8f578 00007ff8a35b7640      152 System.RuntimeType+ActivatorCacheEntry[]
    000001d750d8f610 00007ff8a35e34a8       48 System.RuntimeType+ActivatorCacheEntry
    000001d750d8f640 00007ff8a35d7b40       74 "Creating 10 small arrays"
    000001d750d8f690 00007ff8a35d7b40       94 "Just before blocking compacting GC"
    000001d750d8f6f0 00007ff8a35d7b40       48 "After GC..."
    000001d750d8f720 00007ff8a35c01a0      152 System.Byte[][]
    000001d750d8f7b8 00007ff8a35a2fa8     1024 System.Byte[] 
    000001d750d8fbb8 000001d74f26d250       24 Free
    000001d750d8fbd0 000001d74f26d250   100286 Free
    000001d750da8390 00007ff8a35a2fa8     1024 System.Byte[] (pinned)
    000001d750da8790 00007ff8a35a2fa8     1024 System.Byte[] 
    */
    public class SOHCompactionWithPinning : ICollectScenario
    {
        public unsafe int Run()
        {
            Console.WriteLine("Creating 12 small arrays");
            for (int i = 0; i < 12; ++i)
            {
                byte[] bigArray = new byte[1_000];
                list.Add(bigArray);
            }
            fixed (byte* array = list[7])
            {
                for (int i = 0; i < 7; ++i)
                {
                    list[i] = null;
                }
                Console.WriteLine("Just before blocking compacting GC");
                Console.ReadLine();
                GC.Collect(0, GCCollectionMode.Forced, blocking: true, compacting: true);
                Console.WriteLine("After GC...");
                Console.ReadLine();
            }
            
            return list.Count;
        }

        private List<byte[]> list = new List<byte[]>();
    }
}
