using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MemoryLeaks.Helpers;

namespace MemoryLeaks.Leaks
{
    /*
     * Infinite allocation of pages with very big unusable portion.
     */
    class UnusableLeak : IMemoryLeakExample
    {
        public void Run()
        {
            DllImports.SystemInfo si;
            DllImports.GetSystemInfo(out si);
            uint pageSize = si.PageSize;
            uint allocationGranulatity = si.AllocationGranularity;

            while (true)
            {
                ulong block = (ulong)DllImports.VirtualAlloc(IntPtr.Zero, new IntPtr(pageSize),
                    DllImports.AllocationType.Commit,
                    DllImports.MemoryProtection.ReadWrite);
                Console.WriteLine($"Allocated ${pageSize:X16} at ${block:X16}");
                Thread.Sleep(10);
            }
        }
    }
}
