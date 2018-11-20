using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreCLR.CollectScenarios.Scenarios
{
    class VariousPinning : ICollectScenario
    {
        public unsafe int Run()
        {
            byte[] array = new byte[] {1, 2, 3};
            PinnedValueType[] vtArray = new[] {new PinnedValueType() {F1 = 304}};
            var pinnedByHandle = new PinnedByHandle();
            var pinnedByField = new PinnedByField();
            GCHandle handle = GCHandle.Alloc(pinnedByHandle, GCHandleType.Pinned);

            fixed (byte* b1 = &array[0])
            {
                Console.WriteLine(b1[1]);
            }

            fixed (byte* b2 = array)
            {
                Console.WriteLine(b2[1]);
            }

            fixed (long* l = &pinnedByField.F1)
            {
                Console.WriteLine(*l);
            }

            fixed (PinnedValueType* ptr = vtArray)
            {
                Console.WriteLine(ptr->F1);
            }
            
            Console.ReadLine();

            GC.KeepAlive(array);
            GC.KeepAlive(pinnedByHandle);
            GC.KeepAlive(handle);

            return 0;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class PinnedByHandle
    {
        public long F1 = 301;
    }

    public class PinnedByLocal
    {
        public long F1 = 302;
    }

    public class PinnedByField
    {
        public long F1 = 303;
    }

    public struct PinnedValueType
    {
        public long F1;

    }
}
