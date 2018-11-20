using System;
using System.Runtime.InteropServices;

namespace CoreCLR.VirtualAlloc
{
    class Program
    {
        static void Main(string[] args)
        {
            LargePages();
            Console.WriteLine("Hello World!");
        }

        /*
         * https://msdn.microsoft.com/en-us/library/windows/desktop/aa366720%28v=vs.85%29.aspx
         */
        static void LargePages()
        {
            // The minimum large page size varies, but it is typically 2 MB or greater.
            // Will return 0 if large pages are not supported.
            uint largePageMinimum = DllImports.GetLargePageMinimum();

            // Environment.SystemPageSize missing in .NET Core?
            DllImports.SystemInfo si;
            DllImports.GetSystemInfo(out si);
            uint pageSize = si.PageSize;
            uint allocationGranulatity = si.AllocationGranularity;

            //while (true)
            {
                long baseAddress = long.Parse(Console.ReadLine());
                // Normal pages
                IntPtr block = DllImports.VirtualAlloc(new IntPtr(baseAddress), new IntPtr(pageSize + 1024),
                    DllImports.AllocationType.Reserve,
                    DllImports.MemoryProtection.ReadWrite);
            }
            // Large page
            long blockSize = largePageMinimum;
            IntPtr blockPtr = DllImports.VirtualAlloc(IntPtr.Zero, new IntPtr(blockSize),
                DllImports.AllocationType.Reserve | DllImports.AllocationType.Commit | DllImports.AllocationType.LargePages,
                DllImports.MemoryProtection.ReadWrite);
            uint error = DllImports.GetLastError();
            if (error == 1314)
            {
                // ERROR_PRIVILEGE_NOT_HELD
                // A required privilege is not held by the client.
            }
            if (error == 1450)
            {
                // ERROR_NO_SYSTEM_RESOURCES
                // Insufficient system resources exist to complete the requested service.
            }

        }
    }

    class DllImports
    {
        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern void GetSystemInfo(out SystemInfo Info);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern UInt32 GetLargePageMinimum();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress,
            IntPtr dwSize,
            AllocationType flAllocationType,
            MemoryProtection flProtect);

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        public enum ProcessorArchitecture
        {
            X86 = 0,
            X64 = 9,
            @Arm = -1,
            Itanium = 6,
            Unknown = 0xFFFF,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SystemInfo
        {
            public ProcessorArchitecture ProcessorArchitecture; // WORD
            public uint PageSize; // DWORD
            public IntPtr MinimumApplicationAddress; // (long)void*
            public IntPtr MaximumApplicationAddress; // (long)void*
            public IntPtr ActiveProcessorMask; // DWORD*
            public uint NumberOfProcessors; // DWORD (WTF)
            public uint ProcessorType; // DWORD
            public uint AllocationGranularity; // DWORD
            public ushort ProcessorLevel; // WORD
            public ushort ProcessorRevision; // WORD
        }
    }

}
