using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using CoreCLR.CIL;
using Microsoft.Win32.SafeHandles;

namespace CoreCLR.Finalization
{
    class Program
    {
        public static FinalizableObject GlobalFinalizableObject;

        static void Main(string[] args)
        {
            #region Simple experiments
            // Listing 12-1
            //FileWrapper f = new FileWrapper(@"C:\Notes.md");
            //Console.WriteLine("Doing GC!");            
            //GC.Collect();

            //LoggedObject obj = new LoggedObject(new Logger());

            //var pf = new ProblematicObject();
            //Console.WriteLine(pf.UseMe());

            //var pfw = new ProblematicFileWrapper(@"C:\temp.txt");

            //CustomFileSafeHandle f2 = new CustomFileSafeHandle(@"C:\Notes.md");

            //Console.WriteLine("Doing GC2!");            
            //GC.Collect();

            //Timer t = new Timer();
            //Console.WriteLine("Doing GC2!");
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //GC.Collect();
            //Console.WriteLine(pf.UseMe());
            //Console.WriteLine(pfw.UseMe());
            #endregion

            #region Get total memory usage
            //long last;
            //var current = GC.GetTotalMemory(forceFullCollection: false);
            //do
            //{
            //    last = current;
            //    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            //    GC.Collect();
            //    GC.WaitForPendingFinalizers();
            //    current = GC.GetTotalMemory(forceFullCollection: true);
            //} while (current != last);
            #endregion

            #region Scenario - Re-registeting for finalization
            /*
            LoggedObject obj = new LoggedObject(new Logger());
            WeakReference weakRef = new WeakReference(obj, true);
            // Promote obj to gen2
            GC.Collect();
            GC.Collect();
            Console.WriteLine($"Gen: {GC.GetGeneration(obj)}");
            GC.KeepAlive(obj);

            // Obj become unreachable, it will be collected & finalized (but ressurected into LoggedObject.Global)
            Console.WriteLine($"Gen2s: {GC.CollectionCount(2)}");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine($"Gen2s: {GC.CollectionCount(2)}");
            Console.WriteLine($"IsAlive: {weakRef.IsAlive}");
            Console.WriteLine($"Gen: {GC.GetGeneration(weakRef.Target)}");
            
            // ... yet make it unreachable again
            LoggedObject.Global = null;
            // Then try to clean it
            GC.Collect(2, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(2, GCCollectionMode.Forced, true);
            Console.WriteLine($"Gen2s: {GC.CollectionCount(2)}");
            Console.WriteLine($"IsAlive: {weakRef.IsAlive}");
            if (weakRef.IsAlive)
                Console.WriteLine($"Gen: {GC.GetGeneration(weakRef.Target)}");
            */
            #endregion

            #region Generic reference counter
            /*ReferenceCountedObject ctr = new ReferenceCountedObject();
            using (ctr.Use())
            {
                ctr.RegularMethod();
            }*/
            #endregion

            #region Weak references experiments
            /*
            {
                // Listing 12-35
                var obj = new LargeClass(ressurect: true);
                GCHandle weakHandle = GCHandle.Alloc(obj, GCHandleType.Weak);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Console.WriteLine(weakHandle.Target ?? "<null>"); // prints <null>
            }

            {
                // Listing 12-36
                var obj = new LargeClass(ressurect: true);
                GCHandle weakHandle = GCHandle.Alloc(obj, GCHandleType.WeakTrackResurrection);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Console.WriteLine(weakHandle.Target ?? "<null>"); // prints CoreCLR.Finalization.LargeClass
            }

            {
                var obj = new LargeClass(ressurect: true);
                WeakReference weakReference = new WeakReference(obj, trackResurrection: false);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Console.WriteLine(weakReference.Target ?? "<null>"); // prints <null>
            }

            {
                // Listing 12-38
                var obj = new LargeClass(ressurect: true);
                WeakReference weakReference = new WeakReference(obj, trackResurrection: false);
                if (weakReference.IsAlive)
                    Console.WriteLine(weakReference.Target ?? "<null>");
            }

            {
                // Listing 12-39
                var obj = new LargeClass(ressurect: true);
                WeakReference<LargeClass> weakReference = new WeakReference<LargeClass>(obj, trackResurrection: false);
                if (weakReference.TryGetTarget(out var target))
                    Console.WriteLine(target);
            }*/
            #endregion

            #region Weak event pattern
            /*
            Application app = new Application();
            app.Run();
            */
            #endregion

            #region Struct finalizer
            /*
            CoreCLR.CIL.FinalizableClass fc = new FinalizableClass();
            fc.M();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            object boxedfs = new FinalizableStruct();
            GC.KeepAlive(boxedfs);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();*/
            #endregion

            #region Finalization starving
            LeakyApplication app = new LeakyApplication();
            app.Run();
            #endregion

            Console.ReadLine();
        }
    }

    public static class Unmanaged
    {
        [DllImport("kernel32.dll", BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern IntPtr OpenFile([MarshalAs(UnmanagedType.LPStr)]string lpFileName, 
            out OFSTRUCT lpReOpenBuff,
            long uStyle);
        [DllImport("kernel32.dll", BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern CustomFileSafeHandle OpenFile2([MarshalAs(UnmanagedType.LPStr)]string lpFileName,
            out OFSTRUCT lpReOpenBuff,
            long uStyle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(HandleRef hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(CustomFileSafeHandle hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
        public struct OFSTRUCT
        {
            public byte cBytes;
            public byte fFixedDisc;
            public UInt16 nErrCode;
            public UInt16 Reserved1;
            public UInt16 Reserved2;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPathName;
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-1
    public class FileWrapper
    {
        private IntPtr handle;
        public FileWrapper(string filename)
        {
            Unmanaged.OFSTRUCT s;
            handle = Unmanaged.OpenFile(filename, out s, 0x00000000);
        }
#if !DESTRUCTOR
        ~FileWrapper()
        {
            Console.WriteLine("Finalizing FileWrapper");
            if (handle != IntPtr.Zero)
                Unmanaged.CloseHandle(handle);
        }
#else
        // Do not compile with error: Error	CS0249	Do not override object.Finalize. Instead, provide a destructor.
        protected override void Finalize()
        {
        }
#endif
    }

    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-4
    class LoggedObject
    {
        public static volatile LoggedObject Global = null;
        private static bool alreadyResurrected = false;
        private ILogger logger;
        public LoggedObject(ILogger logger)
        {
            this.logger = logger;
            // ...
            this.logger.Log("Object created.");

        }

        // Destructor
        ~LoggedObject()
        {
            if (!alreadyResurrected)
            {
                Global = this;
                GC.ReRegisterForFinalize(this);
                alreadyResurrected = true;
                this.logger.Log("Object finalized but ressurected.");
            }
            else
            {
                this.logger.Log("Object finalized.");
            }
        }
    }

    interface ILogger
    {
        void Log(string format);
    }

    class Logger : ILogger
    {
        public void Log(string format)
        {
            Console.WriteLine(format);
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-5
    class ProblematicObject
    {
        Stream stream;

        public ProblematicObject() => stream = File.OpenRead(@"C:\Temp.txt");

        ~ProblematicObject()
        {
            Console.WriteLine("Finalizing ProblematicObject");
            stream.Close();
        }

        public int UseMeOriginal()
        {
            return stream.ReadByte();
        }

        public int UseMe()
        {
            var localStream = this.stream;
            // Normal code, complex enough to make this method not inlineable and partialy or fully-interrptible
            // ...
            // GC happens here and finalizers had enough time to execute. 
            // You can simulate that by the following calls:
            // GC.Collect();
            // GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return localStream.ReadByte();
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-6
    public class ProblematicFileWrapper
    {
        private IntPtr handle;
        public ProblematicFileWrapper(string filename)
        {
            Unmanaged.OFSTRUCT s;
            handle = Unmanaged.OpenFile(filename, out s, 0x00000000);
        }
        ~ProblematicFileWrapper()
        {
            Console.WriteLine("Finalizing ProblematicFileWrapper");
            if (handle != IntPtr.Zero)
                Unmanaged.CloseHandle(handle);
        }

        public int UseMeOriginal()
        {
            byte[] buffer = new byte[1];
            //if (Unmanaged.ReadFile(new HandleRef(this, handle), buffer, 1, out uint read, IntPtr.Zero))    // to solve exception
            if (Unmanaged.ReadFile(handle, buffer, 1, out uint read, IntPtr.Zero))
            {
                return buffer[0];
            }
            // GC.KeepAlive(handle); to solve exception
            return -1;
        }

        public int UseMe()
        {
            var hnd = this.handle;
            // Normal code

            // GC happens here and finalizers had enough time to execute. 
            // You can simulate that by the following calls:
            GC.Collect();
            GC.WaitForPendingFinalizers();

            byte[] buffer = new byte[1];
            if (Unmanaged.ReadFile(new HandleRef(this, hnd), buffer, 1, out uint read, IntPtr.Zero))    // to solve exception
                //if (Unmanaged.ReadFile(hnd, buffer, 1, out uint read, IntPtr.Zero))
            {
                return buffer[0];
            }
            //GC.KeepAlive(this); // to solve exception
            return -1;
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-18
    class FinalizableObject
    {
        // Destructor
        ~FinalizableObject()
        {
            Program.GlobalFinalizableObject = this;
            //GC.ReRegisterForFinalize(this);
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-20
    public class Pool<T> where T : class
    {
        static private List<T> items = new List<T>();
        static public void ReturnToPool(T obj) { }

        static public T GetFromPool()
        {
            return null;
        }
    }

    public class SomeClass
    {
        ~SomeClass()
        {
            Pool<SomeClass>.ReturnToPool(this);
            GC.ReRegisterForFinalize(this);
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-21
    public class Timer : IDisposable
    {
        private TimerHolder m_timerHolder;

        public Timer()
        {
            m_timerHolder = new TimerHolder(this);
        }

        private sealed class TimerHolder
        {
            internal Timer m_timer;

            public TimerHolder(Timer timer) => m_timer = timer;

            ~TimerHolder() => m_timer?.Close();

            public void Close()
            {
                m_timer.Close();
                GC.SuppressFinalize(this);
            }
        }

        public void Close()
        {
            Console.WriteLine("Finalizing Timer!");
        }

        public void Dispose()
        {
            m_timerHolder.Close();
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-25
    public class FileWrapper2 : IDisposable
    {
        private bool disposed = false;
        private CustomFileSafeHandle handle;
        public FileWrapper2(string filename)
        {
            Unmanaged.OFSTRUCT s;
            handle = Unmanaged.OpenFile2(filename, out s, 0x00000000);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                handle?.Dispose();
                disposed = true;
            }
        }

        public int UseMe()
        {
            byte[] buffer = new byte[1];
            if (Unmanaged.ReadFile(handle, buffer, 1, out uint read, IntPtr.Zero))
            {
                return buffer[0];
            }
            return -1;
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-29
    class FileWrapper3 : IDisposable
    {
        private bool disposed = false;

        private IntPtr handle;
        public FileWrapper3(string filename)
        {
            Unmanaged.OFSTRUCT s;
            handle = Unmanaged.OpenFile(filename, out s, 0x00000000);
        }

        // Cleanup
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Put here code required only in case of explicit Dispose call
                }

                // Common cleanup - including unmanaged resources
                if (handle != IntPtr.Zero)
                    Unmanaged.CloseHandle(handle);
                disposed = true;
            }
        }

        ~FileWrapper3()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int UseMe()
        {
            if (this.disposed) throw new ObjectDisposedException("...");

            byte[] buffer = new byte[1];
            if (Unmanaged.ReadFile(this.handle, buffer, 1, out uint read, IntPtr.Zero))
            {
                return buffer[0];
            }
            return -1;
        }
    }

    public struct StructFileWrapper
    {
        Unmanaged.OFSTRUCT s;
        private IntPtr handle;
        public StructFileWrapper(string filename)
        {
            handle = Unmanaged.OpenFile(filename, out s, 0x00000000);
        }

        // Not possible
        //~StructFileWrapper()
        //{
        //    Console.WriteLine("Finalizing StructFileWrapper");
        //    if (handle != IntPtr.Zero)
        //        Unmanaged.CloseHandle(handle);
        //}
    }


    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-31
    public class CustomFileSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Called by P/Invoke when returning SafeHandles. Valid handle value will be set afterwards.
        private CustomFileSafeHandle() : base(true)
        {
        }

        // If and only if you need to support user-supplied handles
        internal CustomFileSafeHandle(IntPtr preexistingHandle, bool ownsHandle) : base(ownsHandle)
        {
            SetHandle(preexistingHandle);
        }

        internal CustomFileSafeHandle(string filename) : base(true)
        {
            Unmanaged.OFSTRUCT s;
            IntPtr handle = Unmanaged.OpenFile(filename, out s, 0x00000000); ;
            SetHandle(handle);
        }

        override protected bool ReleaseHandle()
        {
            return Unmanaged.CloseHandle(handle);
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-32
    public class FileWrapper4 : IDisposable
    {
        private bool disposed = false;
        private CustomFileSafeHandle handle;
        public FileWrapper4(string filename)
        {
            Unmanaged.OFSTRUCT s;
            handle = Unmanaged.OpenFile2(filename, out s, 0x00000000);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                handle?.Dispose();
                disposed = true;
            }
        }

        public int UseMe()
        {
            byte[] buffer = new byte[1];
            if (Unmanaged.ReadFile(handle, buffer, 1, out uint read, IntPtr.Zero))
            {
                return buffer[0];
            }
            return -1;
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-34
    public class LargeClass
    {
        private readonly bool ressurect;
        public LargeClass(bool ressurect) => this.ressurect = ressurect;
        ~LargeClass()
        {
            if (ressurect)
            {
                GC.ReRegisterForFinalize(this);
            }
        }
    }


    abstract class ReferenceCounter<T>
    {
        protected int counter;
        private object lockObj = new object();

        public IDisposable Use() => new ReferenceCounterHelper(this);

        public abstract void Cleanup();

        private void InternalCleanup()
        {
            lock (lockObj)
            {
                Cleanup();
            }
        }

        sealed class ReferenceCounterHelper : IDisposable
        {
            readonly ReferenceCounter<T> target;

            public ReferenceCounterHelper(ReferenceCounter<T> target)
            {
                this.target = target;
                Interlocked.Increment(ref this.target.counter);
            }

            public void Dispose()
            {
                int value = Interlocked.Decrement(ref this.target.counter);
                if (value == 0)
                    this.target.InternalCleanup();
            }
        }
    }

    class ReferenceCountedObject : ReferenceCounter<ReferenceCountedObject>
    {
        public void RegularMethod()
        {
            Console.WriteLine("Hello world!");
        }

        public override void Cleanup()
        {
            Console.WriteLine("I am cleaning my unmanaged resources here!");
        }
    }

    sealed class StrongToWeakReference<T> : WeakReference where T : class
    {
        private T _strongRef;

        public StrongToWeakReference(T obj) : base(obj)
        {
            _strongRef = obj;
        }

        public void MakeWeak() => _strongRef = null;

        public void MakeStrong()
        {
            _strongRef = WeakTarget;
        }

        public new T Target => _strongRef ?? WeakTarget;
        private T WeakTarget => base.Target as T;
    }
}
