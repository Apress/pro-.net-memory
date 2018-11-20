using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Diagnostics.Runtime;

namespace CoreCLR.ClrMD
{
    class Program
    {
        static void Main(string[] args)
        {
            int pid = 25668; //Process.GetCurrentProcess().Id;

            // Listing 15-26
            // DataTarget target = DataTarget.LoadCrashDump(@"c:\work\crash.dmp")
            using (DataTarget target = DataTarget.AttachToProcess(pid, 5000, AttachFlag.Invasive))
            {
                foreach (ClrInfo clrInfo in target.ClrVersions)
                {
                    Console.WriteLine("Found CLR Version:" + clrInfo.Version.ToString());

                    // This is the data needed to request the dac from the symbol server:
                    ModuleInfo dacInfo = clrInfo.DacInfo;
                    Console.WriteLine($"Filesize:  {dacInfo.FileSize:X}");
                    Console.WriteLine($"Timestamp: {dacInfo.TimeStamp:X}");
                    Console.WriteLine($"Dac File:  {dacInfo.FileName}");

                    ClrRuntime runtime = clrInfo.CreateRuntime();

                    // Listing 15-27
                    foreach (ClrThread thread in runtime.Threads)
                    {
                        if (!thread.IsAlive)
                            continue;
                        Console.WriteLine("Thread {0:X}:", thread.OSThreadId);
                        foreach (ClrStackFrame frame in thread.StackTrace)
                            Console.WriteLine("{0,12:X} {1,12:X} {2}", frame.StackPointer, frame.InstructionPointer,
                                frame.ToString());
                        Console.WriteLine();
                    }

                    // Listing 15-28
                    foreach (var domain in runtime.AppDomains)
                    {
                        Console.WriteLine($"AppDomain {domain.Name} ({domain.Address:X})");
                        foreach (var module in domain.Modules)
                        {
                            Console.WriteLine($"   Module {module.Name} ({(module.IsFile ? module.FileName : "")})");
                            foreach (var type in module.EnumerateTypes())
                            {
                                Console.WriteLine($"{type.Name} Fields: {type.Fields.Count}");
                            }
                        }
                    }

                    // Listing 15-29
                    foreach (var region in runtime.EnumerateMemoryRegions().OrderBy(r => r.Address))
                    {
                        Console.WriteLine($"{region.Address:X} (size: {region.Size:N0}) - {region.Type} " +
                                          $"{(region.Type == ClrMemoryRegionType.GCSegment ? "(" + region.GCSegmentType.ToString() + ")" : "")}");
                    }

                    // Listing 15-30
                    ClrHeap heap = runtime.Heap;
                    foreach (var clrObject in heap.EnumerateObjects())
                    {
                        if (clrObject.Type.Name.EndsWith("SampleClass"))
                            ShowObject(heap, clrObject, string.Empty);
                    }

                    // Listing 15-33
                    foreach (var segment in heap.Segments)
                    {
                        Console.WriteLine(
                            $"{segment.Start:X16} - {segment.End:X16} ({segment.CommittedEnd:X16}) CPU#: {segment.ProcessorAffinity}");
                        if (segment.IsEphemeral)
                        {
                            Console.WriteLine($"   Gen0: {segment.Gen0Start:X16} ({segment.Gen0Length})");
                            Console.WriteLine($"   Gen1: {segment.Gen1Start:X16} ({segment.Gen1Length})");
                            if (segment.Gen2Start >= segment.Start &&
                                segment.Gen2Start < segment.CommittedEnd)
                            {
                                Console.WriteLine($"   Gen2: {segment.Gen2Start:X16} ({segment.Gen2Length})");
                            }
                        }
                        else if (segment.IsLarge)
                        {
                            Console.WriteLine($"   LOH: {segment.Start} ({segment.Length})");
                        }
                        else
                        {
                            Console.WriteLine($"   Gen2: {segment.Gen2Start:X16} ({segment.Gen2Length})");
                        }

                        foreach (var address in segment.EnumerateObjectAddresses())
                        {
                            var type = heap.GetObjectType(address);
                            if (type == heap.Free)
                            {
                                Console.WriteLine($"{type.GetSize(address)}");
                            }
                        }
                    }

                    foreach (ClrThread thread in runtime.Threads)
                    {
                        var mi = runtime.GetType()
                            .GetMethod("GetThread", BindingFlags.Instance | BindingFlags.NonPublic);
                        var threadData = mi.Invoke(runtime, new object[] {thread.Address});
                        var pi = threadData.GetType()
                            .GetProperty("AllocPtr", BindingFlags.Instance | BindingFlags.Public);
                        ulong allocPtr = (ulong) pi.GetValue(threadData);
                    }
                }
            }
        }

        private static void ShowObject(ClrHeap heap, ClrObject clrObject, string indent)
        {
            Console.WriteLine($"{indent} {clrObject.Type.Name} ({clrObject.HexAddress}) - gen{heap.GetGeneration(clrObject.Address)}");
            foreach (var reference in clrObject.EnumerateObjectReferences())
            {
                ShowObject(heap, reference, "  ");
            }
        }
    }
}
