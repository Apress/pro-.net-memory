using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCLR.CollectScenarios.Scenarios
{
    class Demotion : ICollectScenario
    {
        /*
         * Create following layout of Pad objects:
         * |--- FirstPlugSize ---|--- GapSize ---| Pinned object |--- SecondPlugSize ---|
         * All they live in gen1 because one GC.Collect was called
         * 
         * For constants:
         * const int FirstPlugSize = 70;
         * const int SecondPlugSize = 20;
         * 
         * Pinned plug will be:
         * - promoted to gen 2 for GapSize equal or less 20 (second plug size) - there is no room for second plug so pinned plug just gets promoted (plugs are not splitted)
         * - not promoted (stays in gen1) for GapSize 21 - gap left after moving second plug to the first plug is big enough to put gen 1 start there so pinned plug lands in gen1
         * - demoted for GapSize bigger or equal to 22 - gap left after moving second plug to the first plug is big enough to put gen 1 and 0 there
         */
        const int FirstPlugSize = 70;
        const int GapSize = 22;
        const int SecondPlugSize = 20;

        public unsafe int Run()
        {
            // Cleanup everything
            if (true) GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);

            Console.WriteLine("Creating objects layout");
            for (int i = 0; i < FirstPlugSize + GapSize; ++i)
            {
                list.Add(new Pad());
            }
            var pinned = new Pad();
            list.Add(pinned);
            fixed (long* p = &pinned.F1) // Pin ASAP just for sure
            {
                for (int i = 0; i < SecondPlugSize; ++i)
                {
                    list.Add(new Pad());
                }
                // Cleanup everything
                if (true) GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
                // Create gap
                for (int i = 0; i < GapSize; ++i)
                {
                    list[FirstPlugSize + i] = null;
                }
                Console.WriteLine("Just before blocking compacting GC");
                //PrintGenerations();
                Console.ReadLine();
                GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
                Console.WriteLine("After GC... Just before new allocation");
                //PrintGenerations();
                Console.WriteLine(GC.GetGeneration(pinned));
                Console.ReadLine();

                list.Add(new Pad());
                Console.WriteLine("After allocation");
                //PrintGenerations();
                Console.ReadLine();
            }
            return list.Count;
        }

        private List<object> list = new List<object>(FirstPlugSize + GapSize + SecondPlugSize + 2);

        private unsafe UInt64 AddressOf(object o)
        {
            TypedReference tr = __makeref(o);
            IntPtr ptr = **(IntPtr**)(&tr);
            return (UInt64)ptr;
        }

        public class Pad
        {
            public long F1 = 301;
            //public long F2 = 302;
        }
    }
}
