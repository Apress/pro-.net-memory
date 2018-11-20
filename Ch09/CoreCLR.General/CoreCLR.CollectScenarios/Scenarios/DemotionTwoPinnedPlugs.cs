using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreCLR.CollectScenarios.Scenarios
{
    class DemotionTwoPinnedPlugs : ICollectScenario
    {
        /*
         * Create following layout of Pad objects:
         * |--- FirstPlugSize ---|--- FirstGapSize ---| Pinned object |--- SecondPlugSize ---|--- SecondGapSize ---| Pinned object |--- ThirdPlugSize ---|
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
        const int FirstPlugSize = 20;
        const int FirstGapSize = 20;
        const int SecondPlugSize = 15;
        const int SecondGapSize = 5;
        const int ThirdPlugSize = 10;

        public unsafe int Run()
        {
            int totalSize = FirstPlugSize + FirstGapSize + SecondPlugSize + SecondGapSize + ThirdPlugSize + 2;
            list = new List<object>(totalSize);

            // Cleanup everything
            if (true) GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);

            Console.WriteLine("Creating objects layout");
            for (int i = 0; i < totalSize; ++i)
            {
                list.Add(new Pad());
            }
            var firstPinned = list[FirstPlugSize + FirstGapSize];
            GCHandle firstPinnedHandle = GCHandle.Alloc(firstPinned, GCHandleType.Pinned);
            var secondPinned = list[FirstPlugSize + FirstGapSize + 1 + SecondPlugSize + SecondGapSize];
            GCHandle secondPinnedHandle = GCHandle.Alloc(secondPinned, GCHandleType.Pinned);

            // Cleanup everything (everything lands in gen 1)
            if (true) GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);

            // Create first gap
            for (int i = 0; i < FirstGapSize; ++i)
            {
                list[FirstPlugSize + i] = null;
            }
            // Create second gap
            for (int i = 0; i < SecondGapSize; ++i)
            {
                list[FirstPlugSize + FirstGapSize + 1 + SecondPlugSize + i] = null;
            }
            Console.WriteLine("Just before blocking compacting GC");
            Console.ReadLine();
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            Console.WriteLine("After GC... Just before new allocation");
            Console.WriteLine(GC.GetGeneration(firstPinned));
            Console.WriteLine(GC.GetGeneration(secondPinned));
            Console.ReadLine();

            list.Add(new Pad());
            Console.WriteLine("After allocation");
            Console.ReadLine();

            firstPinnedHandle.Free();
            secondPinnedHandle.Free();
            return list.Count;
        }

        private List<object> list;

        private unsafe UInt64 AddressOf(object o)
        {
            TypedReference tr = __makeref(o);
            IntPtr ptr = **(IntPtr**)(&tr);
            return (UInt64)ptr;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Pad
        {
            public long F1 = 301;
            //public long F2 = 302;
        }
    }
}
