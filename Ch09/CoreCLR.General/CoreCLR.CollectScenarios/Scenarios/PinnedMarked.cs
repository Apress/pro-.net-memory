using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreCLR.CollectScenarios.Scenarios
{
    class PinnedMarked : ICollectScenario
    {
        /*
         * Create following layout of Pad objects:
         * | Pinned | Marked | Pinned | Marked | ...
         * All they will be groped into a single pinned plug
         */
        const int NumberOfPairs = 10;

        public unsafe int Run()
        {
            // Cleanup everything
            if (true) GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);

            Console.WriteLine("Creating objects layout");
            for (int i = 0; i < NumberOfPairs; ++i)
            {
                var pinned = new Pinned();
                GCHandle handle = GCHandle.Alloc(pinned, GCHandleType.Pinned);
                list.Add(pinned);
                list.Add(new Marked());
                handles.Add(handle);
            }
            Console.WriteLine("Just before blocking compacting GC");
            Console.ReadLine();
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            Console.WriteLine("After GC...");
            Console.WriteLine(GC.GetGeneration(list[0]));
            Console.WriteLine(GC.GetGeneration(list[list.Count - 1]));
            Console.ReadLine();

            return list.Count;
        }

        private List<object> list = new List<object>(2 * NumberOfPairs);
        private List<GCHandle> handles = new List<GCHandle>(NumberOfPairs);

        [StructLayout(LayoutKind.Sequential)]
        public class Pinned
        {
            public long F1 = 301;
        }

        public class Marked
        {
            public long F1 = 401;
        }
    }
}
