using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreCLR.CollectScenarios.Scenarios
{
    class VariousRoots : ICollectScenario
    {
        ///////////////////////////////////////////////////////////////////////
        // Listing 8-26
        public int Run()
        {
            // Cleanup everything
            if (true) GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);

            Normal normal = new Normal();

            Pinned onlyPinned = new Pinned();
            GCHandle handle = GCHandle.Alloc(onlyPinned, GCHandleType.Pinned);

            ObjectWithStatic obj = new ObjectWithStatic();
            Console.WriteLine(ObjectWithStatic.StaticField);

            Marked strong = new Marked();
            GCHandle strongHandle = GCHandle.Alloc(strong, GCHandleType.Normal);

            string literal = "Hello world!";
            GCHandle literalHandle = GCHandle.Alloc(literal, GCHandleType.Normal);

            Console.ReadLine();

            GC.KeepAlive(obj);
            handle.Free();
            literalHandle.Free();
            strongHandle.Free();
            return 0;
        }

        public class Normal
        {
            public long F1 = 101;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Pinned
        {
            public long F1 = 301;
        }

        public class Marked
        {
            public long F1 = 401;
        }

        public class ObjectWithStatic
        {
            public static Static StaticField = new Static();
        }

        public class Static
        {
            public long F1 = 501;
        }
    }
}
