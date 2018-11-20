//#define TRACELOG

using System;
using System.IO;
using Microsoft.Diagnostics.Symbols;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;

namespace CoreCLR.TraceEvent
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var session = new TraceEventSession("MonitorKernelAndClrEventsSession"))
            {
                Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs cancelArgs) =>
                {
                    session.Dispose();
                    cancelArgs.Cancel = true;
                };

                //session.EnableKernelProvider(KernelTraceEventParser.Keywords.ImageLoad | KernelTraceEventParser.Keywords.Process | KernelTraceEventParser.Keywords.Thread);
                
                var optionsWithStacks = new TraceEventProviderOptions() { StacksEnabled = true };
                //session.EnableProvider(ClrTraceEventParser.ProviderGuid, TraceEventLevel.Verbose, (ulong)ClrTraceEventParser.Keywords.Default);
                //session.EnableProvider(ClrTraceEventParser.ProviderGuid, TraceEventLevel.Verbose, (ulong)ClrTraceEventParser.Keywords.Exception, optionsWithStacks);

                //session.EnableProvider(ClrRundownTraceEventParser.ProviderGuid, TraceEventLevel.Verbose, (ulong)ClrRundownTraceEventParser.Keywords.Default);

                session.EnableProvider("Microsoft-Windows-TCPIP", TraceEventLevel.Verbose);
#if TRACELOG
                using (TraceLogEventSource traceLogSource = TraceLog.CreateFromTraceEventSession(session))
                {
                    traceLogSource.Clr.ExceptionStart += ClrOnExceptionStart;
                    traceLogSource.Process();
                }
#else
                session.Source.Clr.GCStart += ClrOnGcStart;
                session.Source.Clr.GCStop += ClrOnGcStop;
                session.Source.Clr.GCHeapStats += ClrOnGcHeapStats;
                
                session.Source.Dynamic.All += delegate (Microsoft.Diagnostics.Tracing.TraceEvent data)
                {
                    // ETW buffers events and only delivers them after buffering up for some amount of time.  Thus 
                    // there is a small delay of about 2-4 seconds between the timestamp on the event (which is very 
                    // accurate), and the time we actually get the event.  We measure that delay here.     
                    var delay = (DateTime.Now - data.TimeStamp).TotalSeconds;
                    //Console.WriteLine("GOT Event Delay={0:f1}sec: {1}/{2} ", delay, data.ProviderName, data.EventName);
                };
                session.Source.Dynamic.AddCallbackForProviderEvent("Microsoft-Windows-TCPIP", "TcpSendTransmitted", 
                    data => Console.WriteLine($"PID {data.ProcessID} sent {data.PayloadByName("NumBytes")} B"));

                //session.Source.Kernel.ProcessStart += KernelOnProcessStart;
                //session.Source.Kernel.ProcessStop += KernelOnProcessStop;
                session.Source.Process();
#endif
            }
        }

        private static void ClrOnExceptionStart(ExceptionTraceData data)
        {
            var symbolReader = new SymbolReader(Console.Out, SymbolPath.MicrosoftSymbolServerPath);
            Console.WriteLine($"[{data.ProcessName}] Exception {data.ExceptionType} at {data.ExceptionEIP}");
            PrintStack(data.CallStack(), symbolReader);
        }

        private static void ClrOnGcHeapStats(GCHeapStatsTraceData data)
        {
            Console.WriteLine($"[{data.ProcessName}]     Heapstats - {data.GenerationSize0:N0}|{data.GenerationSize1:N0}|{data.GenerationSize2:N0}|{data.GenerationSize3}");
        }

        private static void KernelOnProcessStop(ProcessTraceData data)
        {
            Console.WriteLine($"[{data.ProcessName}] Ending PID {data.ProcessID}");

        }

        private static void KernelOnProcessStart(ProcessTraceData data)
        {
            Console.WriteLine($"[{data.ProcessName}] Starting {data.CommandLine} as PID {data.ProcessID}");
        }

        private static void ClrOnGcStart(GCStartTraceData data)
        {
            Console.WriteLine($"[{data.ProcessName}] GC gen{data.Depth} because {data.Reason} started {data.Type}.");
        }

        private static void ClrOnGcStop(GCEndTraceData data)
        {
            Console.WriteLine($"[{data.ProcessName}] GC ended.");
        }

        private static void PrintStack(TraceCallStack callStack, SymbolReader symbolReader)
        {
            while (callStack != null)
            {
                var method = callStack.CodeAddress.Method;
                var module = callStack.CodeAddress.ModuleFile;
                if (method != null)
                {
                    // see if we can get line number information
                    var lineInfo = "";
                    var sourceLocation = callStack.CodeAddress.GetSourceLine(symbolReader);
                    if (sourceLocation != null)
                        lineInfo = string.Format("  AT: {0}({1})", Path.GetFileName(sourceLocation.SourceFile.BuildTimeFilePath), sourceLocation.LineNumber);

                    Console.WriteLine("    Method: {0}!{1}{2}", module.Name, method.FullMethodName, lineInfo);
                }
                else if (module != null)
                    Console.WriteLine("    Module: {0}!0x{1:x}", module.Name, callStack.CodeAddress.Address);
                else
                    Console.WriteLine("    ?!0x{0:x}", callStack.CodeAddress.Address);

                callStack = callStack.Caller;
            }
        }
    }
}
