using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;

namespace CoreCLR.GCApi
{
    class Program
    {
        static void Main(string[] arguments)
        {
            /*
            bool isServerGC = GCSettings.IsServerGC;
            int pointerSize = IntPtr.Size;
            int numberProcs = Environment.ProcessorCount;
            Console.WriteLine("{0} on {1}-bit with {2} procs",
                (isServerGC ? "Server" : "Workstation"),
                ((pointerSize == 8) ? 64 : 32),
                numberProcs);

            Console.WriteLine(GC.MaxGeneration);

            // Listing 15-1
            GC.Collect(0);
            Console.WriteLine($"{GC.CollectionCount(0)} {GC.CollectionCount(1)} {GC.CollectionCount(2)}");
            GC.Collect(1);
            Console.WriteLine($"{GC.CollectionCount(0)} {GC.CollectionCount(1)} {GC.CollectionCount(2)}");
            GC.Collect(2);
            Console.WriteLine($"{GC.CollectionCount(0)} {GC.CollectionCount(1)} {GC.CollectionCount(2)}");*/

            // Listing 15-6
            Console.WriteLine("Hello world!");
            var process = Process.GetCurrentProcess();
            Console.WriteLine($"{process.PrivateMemorySize64:N0}");
            Console.WriteLine($"{process.WorkingSet64:N0}");
            Console.WriteLine($"{process.VirtualMemorySize64:N0}");
            Console.WriteLine($"{GC.GetTotalMemory(true):N0}");
            //Console.WriteLine(process.PagedMemorySize64 / 1024);
            //Console.WriteLine(process.PagedSystemMemorySize64 / 1024);
            //Console.WriteLine(process.NonpagedSystemMemorySize64 / 1024);
            Console.ReadLine();
            //GC.Collect();
            //Console.ReadLine();
            /*
            {
                var args = new object[5];
                var mi = typeof(GC).GetMethod("GetMemoryInfo", BindingFlags.Static | BindingFlags.NonPublic);
                mi.Invoke(null, args);
                uint highMemLoadThreshold = (uint) args[0];
                ulong totalPhysicalMem = (ulong) args[1];
                uint lastRecordedMemLoad = (uint) args[2];
                UIntPtr lastRecordedHeapSize = (UIntPtr) args[3];
                UIntPtr lastRecordedFragmentation = (UIntPtr) args[4];
            }

            {
                var args = new object[2];
                var mi = typeof(MemoryFailPoint).GetMethod("GetMemorySettings", BindingFlags.Static | BindingFlags.NonPublic);
                mi.Invoke(null, args);
            }

            // Listing 15-14
            try
            {
                using (MemoryFailPoint failPoint = new MemoryFailPoint(sizeInMegabytes: 1024))
                {
                    // Do calculations
                }
            }
            catch (InsufficientMemoryException e)
            {
                Console.WriteLine(e);
                throw;
            }
            */
            {
                // Region 15-13
                GC.RegisterForFullGCNotification(10, 10);
                Thread startpolling = new Thread(() =>
                {
                    while (true)
                    {
                        GCNotificationStatus s = GC.WaitForFullGCApproach(1000);
                        if (s == GCNotificationStatus.Succeeded)
                        {
                            Console.WriteLine("GC is about to begin");
                        }
                        else if (s == GCNotificationStatus.Timeout)
                            continue;
                        
                        // ...
                        // react to full GC, for example call code disabling current server from load balancer
                        // ...
                        s = GC.WaitForFullGCComplete(10_000);
                        if (s == GCNotificationStatus.Succeeded)
                        {
                            Console.WriteLine("GC has ended");
                        }
                        else if (s == GCNotificationStatus.Timeout)
                            Console.WriteLine("GC took alarming amount of time");
                    }
                });
                startpolling.Start();
                GC.CancelFullGCNotification();

            }
        }
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 15-4
    public class PinnableObjectPool<T> where T : class
    {
        private readonly Func<T> factory;
        private ConcurrentStack<T> agedObjects = new ConcurrentStack<T>();
        private ConcurrentStack<T> notAgedObjects = new ConcurrentStack<T>();

        public PinnableObjectPool(Func<T> factory)
        {
            this.factory = factory;
            Gen2GcCallback.Register(Gen2GcCallbackFunc, this);
        }

        public T Rent()
        {
            if (!agedObjects.TryPop(out T result))
                RentYoungObject(out result);
            return result;
        }

        public void Return(T obj)
        {
            if (GC.GetGeneration(obj) < GC.MaxGeneration)
                notAgedObjects.Push(obj);
            else
                agedObjects.Push(obj);
        }

        private void RentYoungObject(out T result)
        {
            if (!notAgedObjects.TryPop(out result))
            {
                result = factory();
            }
        }

        private static bool Gen2GcCallbackFunc(object targetObj)
        {
            ((PinnableObjectPool<T>)(targetObj)).AgeObjects();
            return true;
        }

        private void AgeObjects()
        {
            List<T> notAgedList = new List<T>();
            foreach (var candidateObject in notAgedObjects)
            {
                if (GC.GetGeneration(candidateObject) == GC.MaxGeneration)
                {
                    agedObjects.Push(candidateObject);
                }
                else
                {
                    notAgedList.Add(candidateObject);
                }
            }
            notAgedObjects.Clear();
            foreach (var notAgedObject in notAgedList)
            {
                notAgedObjects.Push(notAgedObject);
            }
        }
    }

    internal sealed class Gen2GcCallback : CriticalFinalizerObject
    {
        private Gen2GcCallback()
        {
        }

        public static void Register(Func<object, bool> callback, object targetObj)
        {
            // Create a unreachable object that remembers the callback function and target object.
            Gen2GcCallback gcCallback = new Gen2GcCallback();
            gcCallback.Setup(callback, targetObj);
        }

        private Func<object, bool> _callback;
        private GCHandle _weakTargetObj;

        private void Setup(Func<object, bool> callback, object targetObj)
        {
            _callback = callback;
            _weakTargetObj = GCHandle.Alloc(targetObj, GCHandleType.Weak);
        }

        ~Gen2GcCallback()
        {
            // Check to see if the target object is still alive.
            object targetObj = _weakTargetObj.Target;
            if (targetObj == null)
            {
                // The target object is dead, so this callback object is no longer needed.
                _weakTargetObj.Free();
                return;
            }

            // Execute the callback method.
            try
            {
                if (!_callback(targetObj))
                {
                    // If the callback returns false, this callback object is no longer needed.
                    return;
                }
            }
            catch
            {
                // Ensure that we still get a chance to resurrect this object, even if the callback throws an exception.
            }

            // Resurrect ourselves by re-registering for finalization.
            if (!Environment.HasShutdownStarted)
            {
                GC.ReRegisterForFinalize(this);
            }
        }
    }

}
