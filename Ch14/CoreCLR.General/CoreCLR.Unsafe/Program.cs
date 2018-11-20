using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ObjectLayoutInspector;

namespace CoreCLR.UnsafeTests
{
    class C
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            //Span<int> x = stackalloc[] { 1, 2, 3 };
            //Console.WriteLine(x[0]);

            //SomeStruct someStruct = new SomeStruct();
            //SomeClass someClass = new SomeClass();


            //MovableFixedBuffers testFixedBuffers = new MovableFixedBuffers();
            //testFixedBuffers.Test();

            //PassingByref.ByRefReferenceType(ref someClass);
            //PassingByref.ByRefValueType(ref someStruct);
            //PassingByref.ByRefInterior(ref someClass.Field);
            //Console.WriteLine(someClass.Field);
            //Console.WriteLine(someStruct.Field);

            #region Unmanaged constraint
            /*
            ConstraintTests tests = new ConstraintTests();
            tests.Test();
            */
            #endregion

            #region Observing returned interior pointer
            /*
            ref int byRef = ref PassingByref.ObservableReturnByRefReferenceTypeInterior(2, out WeakReference wr);
            byRef = 4;
            for (int i = 0; i < 3; ++i)
            {
                GC.Collect();
                Console.WriteLine(byRef + " " + wr.IsAlive);
                //Console.ReadLine();
            }
            GC.Collect();
            Console.WriteLine(wr.IsAlive);
            */
            #endregion
            //Console.WriteLine(PassingByref.UsingRefLocal(30));
            #region Return by ref
            /*
            int[] array = {1, 2, 3};
            ref int arrElementRef = ref PassingByref.GetArrayElementByRef(array, 0);
            arrElementRef = 4;
            Console.WriteLine(string.Join("", array));

            int arrElementVal = PassingByref.GetArrayElementByRef(array, 0);
            arrElementVal = 5;
            Console.WriteLine(string.Join("", array));
            */
            #endregion

            #region Interior fun
            /*
            int v = 0;
            Console.WriteLine(new C().M<int>()(ref v));
            */
            #endregion

            #region Unmanaged and ref
            /*
            ref SomeStruct pStruct = ref Unsafe.AsRef<SomeStruct>(Marshal.AllocHGlobal(Unsafe.SizeOf<SomeStruct>()).ToPointer());
            */
            #endregion

            #region Readonly refs
            /*
            var collection = new BookCollection();
            ref readonly var book = ref collection.GetBookByTitle("Call of the Wild, The");
            //if (book != null)
            {
                //book = new Book();
                //book.Title = "Call of the Wild, The";
                //book.Author = "Ja";
                book.ModifyAuthor();
                Console.WriteLine(book.Author);
                Console.WriteLine(collection.GetBookByTitle("Call of the Wild, The").Author);
            }
            */
            #endregion

            #region In structs
            /*
            var book = new Book() { Author = "Konrad Kokosa", Title = "Pro .NET Memory Management" };
            var collection = new BookCollection();
            collection.CheckBook(in book);
            Console.WriteLine(book.Author);
            */
            #endregion

            #region Ref returning collections

            /*
            SomeStructRefList refList = new SomeStructRefList(10);
            for (var i = 0; i < 10; ++i)
                refList[i].Field = i; 
                //refList[i].ModifyMe();
            for (var i = 0; i < 10; ++i)
                Console.WriteLine(refList[i].Field);

            SomeStructReadOnlyRefList readOnlyRefList = new SomeStructReadOnlyRefList(10);
            for (var i = 0; i < 10; ++i)
                //readOnlyRefList[i].Field = i; // Error CS8332: Cannot assign to a member of property 'SomeStructRefList.this[int]' because it is a readonly variable
                readOnlyRefList[i].ModifyMe();
            for (var i = 0; i < 10; ++i)
                Console.WriteLine(readOnlyRefList[i].Field);
            */
            #endregion

            #region Readonly struct

            //ReadonlyBook book = new ReadonlyBook(Author = "Konrad Kokosa" };

            #endregion

            #region Fixed size buffers
            /*
            List<StructWithFixedArray> list = new List<StructWithFixedArray>();
            list.Add(new StructWithFixedArray());
            */
            #endregion

            #region Span
            //SpanExamples tests = new SpanExamples();
            //tests.UseSpanWisely(24);
            #endregion

            #region Memory
            /*
            MemoryExamples tests = new MemoryExamples();
            tests.Tests();
            */
            #endregion

            #region Unsafe 
            UnsafeTests tests = new UnsafeTests();
            tests.Test();
            #endregion

            //Console.ReadLine();
        }

        public delegate ref T Delegate<T>(ref T x);
        public Delegate<T> M<T>() => (ref T x) => ref (new T[1] { x })[0];
    }

    #region Passing by ref

    public static class PassingByref
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ByRefValueType(ref SomeStruct data)
        {
            data.Field = 11;
            //for (int i = 0; i < data.Field; ++i)
            //{
            //    Console.WriteLine(data.Field);
            //    data.KeepMeAlive();
            //}
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ByRefReferenceType(ref SomeClass data)
        {
            //data = new SomeClass();
            data.Field = 11;
            //for (int i = 0; i < data.Field; ++i)
            //{
            //    Console.WriteLine(data.Field);
            //    data.KeepMeAlive();
            //}
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ByRefInterior(ref int data)
        {
            // If GC happens here, local variable data is a managed (interior) pointer to an object
            // - GC will find corresponding object via "interior pointer dereference" using plug tree and plug scan
            data = 11;
            for (int i = 0; i < data; ++i)
            {
                if (i % 3 == 0)
                    Console.WriteLine(data);
            }
        }

        public static ref int ReturnByRefReferenceTypeInterior(int index)
        {
            int[] localArray = new[] { 1, 2, 3 };
            return ref localArray[index];
        }

        public static ref int ObservableReturnByRefReferenceTypeInterior(int index, out WeakReference wr)
        {
            ArrayWrapper wrapper = new ArrayWrapper() {Array = new[] {1, 2, 3}, Field = 0};
            wr = new WeakReference(wrapper);
            return ref wrapper.Array[index];
            //return ref wrapper.Field;
        }

        public static int UsingRefLocal(int data)
        {
            ref int refLocal = ref data;
            refLocal = 2;
            return data;
        }

        public static void UsingRefLocal(SomeClass data)
        {
            ref int refLocal = ref data.Field;
            refLocal = 2;
        }

        public static ref int GetArrayElementByRef(int[] array, int index)
        {
            return ref array[index];
        }

#if RETURNBYREF
        public static ref int ReturnByRefValueTypeInterior(int index)
        {
            int localInt = 7;
            return ref localInt; // Error CS8168  Cannot return local 'localInt' by reference because it is not a ref local 
        }
#endif
    }
    #endregion

    #region C# 7.2 - Ref structs
    public ref struct RefBook
    {
        public string Title;
        public string Author;
        public RefPublisher Publisher;
    }

    public ref struct RefPublisher
    {
        public string Name;
    }

    public ref struct DisposableRefStruct
    {
        public string Hello;

        public void Dispose()
        {
            Console.Write("Disposing...");
        }
    }

    public class RefBookTest
    {
        //private RefBook book;

        public void Test()
        {
            RefBook localBook = new RefBook();
            //object box = (object) localBook;
            //RefBook[] array = new RefBook[4];
            //using (var refStruct = new DisposableRefStruct())
            //{

            //}
        }
    }

    //public readonly ref struct StackallocWrapper
    //{
    //    private readonly Span<int> data;
    //    public StackallocWrapper(int size)
    //    {
    //        this.data = stackalloc[] { 1, 2, 3 };
    //    }
    //}
    #endregion

    #region C# 7.3 - System.Enum, System.Delegate and unmanaged constraint
    public struct ConstraintStruct : IDummy
    {
        //public int Field;        // Unmanaged. Sequential
        //public byte B;          // Unmanaged. Sequential
        //public double D;        // Unmanaged. Sequential
        //public int I;           // Unmanaged. Sequential
        //public SomeEnum E;      // Unmanaged. Sequential
        //public SomeStruct AD;   // Unmanaged. Sequential
        //public unsafe void* P;  // Unmanaged. Sequential
        //public object O;          // Fails. Auto
        public decimal DE;      // Unmanaged. Sequential (while non-blittable)
        public DateTime DT;     // Unmanaged. Triggers auto!  (and non-blittable)
        public Guid G;          // Unmanaged. Sequential (while non-blittable)
        public char C;          // Unmanaged. Sequential (while non-blittable) BTW: alignment 2 bytes
        public Boolean BL;      // Unmanaged. Sequential (while non-blittable) BTW: alignment 1 byte
        //public UnmanagedStruct* Pointer;    // Pointers and fixed size buffers may only be used in an unsafe context

    }

    public interface IDummy
    {
    }

    public struct UnmanagedStruct
    {
        public int Field;
    }

    public struct UnmanagedStructDummy 
    {
        public int Field;
    }

    public struct NonUnmanagedStruct
    {
        public int Field;
        public object O;
    }

    public unsafe struct UnmanagedGenericStruct<T> where T : unmanaged
    {
        public T Field;
        public T* Pointer;
    }

    class ConstraintTests
    {
        public void Test()
        {
            //var t = new UnmanagedGenericStruct<object>();
            ConstraintStruct someStruct = new ConstraintStruct();
            //someStruct.DE = 10.0M;
            Constraints test = new Constraints();
            Console.WriteLine(test.UseUnmanagedConstraint(someStruct));
            //Console.WriteLine(test.UseUnmanagedRefConstraint(ref someStruct));

            Constraints.UnamanagedContraint(new UnmanagedStruct());
            //Constraints.UnamanagedContraint(new NonUnmanagedStruct());

            using (UnmanagedArray<int> array = new UnmanagedArray<int>(20))
            {
                array[10] = 10;
                for (int i = 0; i < 20; i++)
                    Console.WriteLine(array[i]);
            }
        }
    }

    public static class ArrayExtensions
    {
        unsafe public static byte[] ToByteArray<T>(this T argument) where T : unmanaged
        {
            var size = sizeof(T);
            var result = new Byte[size];
            Byte* p = (byte*)&argument;
            for (var i = 0; i < size; i++)
                result[i] = *p++;
            return result;
        }
    }

    class Constraints
    {
        public static void UnamanagedContraint<T>(T arg) where T : unmanaged
        {
        }

        unsafe public int UseUnmanagedConstraint<T>(T obj) where T : unmanaged
        {
            T* p = &obj;    // Use T* pointer
            T* sa = stackalloc T[16]; // Use stackalloc
            //fixed (T* ptr = &obj) //You cannot use the fixed statement to take the address of an already fixed expression
            {

            }
            sa[2] = *p;
            //var handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
            Console.WriteLine((long)p);
            //Console.WriteLine(handle.AddrOfPinnedObject());
            return sizeof(T);
        }

        //unsafe public int UseUnmanagedConstraint2<T>(T obj) where T : struct
        //{
        //    T* p = &obj;    // Use T* pointer
        //    T* sa = stackalloc T[16]; // Use stackalloc
        //    //fixed (T* ptr = &obj) //You cannot use the fixed statement to take the address of an already fixed expression
        //    {

        //    }
        //    return sizeof(T);
        //}

        unsafe public int UseUnmanagedRefConstraint<T>(ref T obj) where T : unmanaged
        {
            fixed (T* p = &obj)
            {
                Console.WriteLine((long) p);
                return sizeof(T);
            }
        }

        Span<byte> Convert<T>(ref T value) where T : unmanaged
        {
            Span<T> span = stackalloc T[8];
            // ...
            return new Span<byte>();
        }

        public ref struct Testing<T> where T : unmanaged
        {
            private T field;
            unsafe public int Use()
            {
                //T* p = &field;    // CS0212: You can only take the address of an unfixed expression inside of a fixed statement initializer
                fixed (T* ptr = &field) 
                {

                }
                return sizeof(T);
            }
        }

        public unsafe void LogData<T>(T arg) where T : unmanaged
        {
            if (IsEnabled())
            {
                EventData* data = stackalloc EventData[1];
                data[0].DataPointer = (IntPtr)(&arg);
                data[0].Size = sizeof(T);
                WriteEventCore(data);
            }
        }

        private unsafe void WriteEventCore(EventData* data)
        {
            throw new NotImplementedException();
        }

        private bool IsEnabled() => true;

        private struct EventData
        {
            internal IntPtr DataPointer;
            internal int Size;
        }
    }

    public unsafe class UnmanagedArray<T> : IDisposable
        where T : unmanaged
    {
        private T* data;
        public UnmanagedArray(int length)
        {
            data = (T*)Marshal.AllocHGlobal(length * sizeof(T));
        }

        public ref T this[int index]
        {
            get { return ref data[index]; }
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)data);
        }
    }

    //public unsafe class PoorUnmanagedArray<T> : IDisposable
    //{
    //    private void* data;
    //    private int elementSize;
    //    public PoorUnmanagedArray(int length, int elementSize)
    //    {
    //        data = (void*)Marshal.AllocHGlobal(length * elementSize);
    //        this.elementSize = elementSize;
    //    }

    //    public void* this[int index]
    //    {
    //        get { return ((byte*) data) + index * elementSize; }
    //    }

    //    public void Dispose()
    //    {
    //        Marshal.FreeHGlobal((IntPtr)data);
    //    }
    //}
    #endregion

    #region C# 7.3 - Indexing movable fixed buffers

    public class MovableFixedBuffers
    {
        unsafe public void Test()
        {
            StructWithFixedArray s1 = new StructWithFixedArray();
            Console.WriteLine(s1.text[3]);

            // After C# 7.3
            StructWithFixedArrayWrapper w1 = new StructWithFixedArrayWrapper();
            Console.WriteLine(w1.Data.text[3]);

            // Before C# 7.3
            fixed (char* array = w1.Data.text)
            {
                Console.WriteLine(array[4]);
            }

//            Span<StructWithFixedArray> span0 = stackalloc StructWithArray[4];
            Span<StructWithFixedArray> span = stackalloc StructWithFixedArray[4];
            Console.WriteLine(span[2].text[3]);
        }
    }

    public struct StructWithArray
    {
        public char[] text;
    }

    public unsafe struct StructWithFixedArray
    {
        public fixed char text[128];
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct StructWithFixedArrayExplicit
    {
        [FieldOffset(0)]
        public long Long;
        [FieldOffset(0)]
        public fixed int Integers[2];
        [FieldOffset(0)]
        public fixed byte Bytes[8];
    }

    public class StructWithFixedArrayWrapper
    {
        public StructWithFixedArray Data = new StructWithFixedArray();
    }
#endregion

    #region C# 7.3 - Custom fixed pattern
    public class CustomPinnablePattern
    {
        unsafe public void Test(PinnableType obj)
        {
            fixed (int* ptr = obj)
            {
                Console.WriteLine((long)ptr);
            }
        }
    }

    public class PinnableType
    {
        private int field;
        public ref int GetPinnableReference()
        {
            return ref this.field;
        }
    }
    #endregion

    #region Ref returning collection

    public class SomeStructRefList
    {
        private SomeStruct[] items;

        public SomeStructRefList(int count)
        {
            this.items = new SomeStruct[count];
        }

        //public ref SomeStruct this[int index] => ref items[index];
        public ref SomeStruct this[int index] => ref items[index];

        //public SomeStruct this[int index]
        //{
        //    get { return items[index]; }
        //    set { items[index] = value; }
        //}
    }

    public class SomeStructReadOnlyRefList
    {
        private SomeStruct[] items;

        public SomeStructReadOnlyRefList(int count)
        {
            this.items = new SomeStruct[count];
        }

        //public ref SomeStruct this[int index] => ref items[index];
        public ref readonly SomeStruct this[int index] => ref items[index];
    }

    public class RefList<T>
    {
        private T[] items;

        public RefList(int count)
        {
            this.items = new T[count];
        }

        public ref T this[int index] => ref items[index];
    }
    #endregion

    #region Unsafe

    public class UnsafeTests
    {
        public class SomeClass
        {
            public int Field1;
            public int Field2; 
        }

        public class SomeOtherClass
        {
            public long Field;
        }

        public struct SomeStruct
        {
            public int Field1;
            public int Field2;
        }

        public struct UnmanagedStruct
        {
            public long Long1;
            public long Long2;
        }

        public struct ManagedStruct
        {
            public long Long1;
            public string String;
        }

        public struct SmallStruct
        {
            public byte B1;
            public byte B2;
            public byte B3;
            public byte B4;
            public byte B5;
            public byte B6;
            public byte B7;
            public byte B8;
        }

        public unsafe struct HugeType
        {
            public fixed byte Buffer[0x1_0000 + 128];
        }

        public void Test()
        {
            Console.ReadLine();
            for (int i = 0; i < 8224; ++i)
            {
                Console.WriteLine($"    public long Field{i};");
            }
            //object obj = new HugeType();
            //StrangeException();

            unsafe
            {
                //int read = *((int*)0x1_0000 + 1);
                //int read = *((int*)IntPtr.Zero);
            }
            
            TypeLayout.PrintLayout<SomeClass>();
            TypeLayout.PrintLayout<SomeStruct>();

            Console.WriteLine(Unsafe.SizeOf<SomeClass>());
            Console.WriteLine(Unsafe.SizeOf<SomeStruct>());

            var local = new SomeClass() {Field1 = 1, Field2 = 2};
            Dangerous(local);

            UnmanagedStruct s1;
            s1.Long1 = 1;
            s1.Long2 = 2;
            //VeryDangerous(ref s1);
            Reinterpretation(ref s1);

            UnsafeAndDangerous();
        }

        public unsafe void StrangeException(ref HugeType data)
        {
            Console.WriteLine(data.Buffer[0x1_0000 - 1]);
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 14-39
        public void Dangerous(SomeClass obj)
        {
            ref SomeOtherClass target = ref Unsafe.As<SomeClass, SomeOtherClass>(ref obj);
            Console.WriteLine(target.Field);
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 14-41
        public void VeryDangerous(ref UnmanagedStruct data)
        {
            Console.WriteLine(data.Long1);
            ref ManagedStruct target = ref Unsafe.As<UnmanagedStruct, ManagedStruct>(ref data);
            Console.WriteLine(target.String);
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 14-43
        public unsafe void Reinterpretation(ref UnmanagedStruct data)
        {
            var span = new Span<UnmanagedStruct>(Unsafe.AsPointer(ref data), 1);
            ref var part = ref MemoryMarshal.Cast<byte, SmallStruct>(
                                        MemoryMarshal.AsBytes(span)
                                                     .Slice(0, 8))[0];
            Console.WriteLine(part.B1);
        }

        public unsafe void UnsafeAndDangerous()
        {
            // OK
            // An unmanaged type can be used with pointers
            UnmanagedStruct us = new UnmanagedStruct { Long1 = 1, Long2 = 2 };
            var usp = (long*)Unsafe.AsPointer(ref us);
            usp[0] = 100;
            usp[1] = 200;
            Console.WriteLine(us); // (100, 200)
            ManagedStruct ms = new ManagedStruct { Long1 = 1, String = "Hello world" };
            var msp = (IntPtr*)Unsafe.AsPointer(ref ms);
            msp[0] = (IntPtr)0;
            msp[1] = (IntPtr)0;
            Console.WriteLine(ms); // (NULL, NULL)
            msp[0] = (IntPtr)1234567; // invalid pointer. GC could be crashed   

            int gen = GC.GetGeneration(Unsafe.As<UnmanagedStruct, object>(ref us));
            Console.WriteLine(gen);
        }
    }
    #endregion

    #region Memory
    ///////////////////////////////////////////////////////////////////////
    // Listing 14-27
    public class BufferedWriter : IDisposable
    {
        private const int WriteBufferSize = 32;
        private readonly byte[] writeBuffer = new byte[WriteBufferSize];
        private readonly Stream stream;
        private int writeOffset = 0;

        public BufferedWriter(Stream stream)
        {
            this.stream = stream;
        }

        public async Task WriteAsync(ReadOnlyMemory<byte> source)
        {
            Memory<int> v = new Memory<int>(new int[2]);
            int remaining = writeBuffer.Length - writeOffset;
            if (source.Length <= remaining)
            {
                // Fits in current write buffer. Just copy and return.
                WriteToBuffer(source.Span);
                return;
            }
            while (source.Length > 0)
            {
                // Fit what we can in the current write buffer and flush it.
                remaining = Math.Min(writeBuffer.Length - writeOffset, source.Length);
                WriteToBuffer(source.Slice(0, remaining).Span);
                source = source.Slice(remaining);
                await FlushAsync().ConfigureAwait(false);
            }
        }
        private void WriteToBuffer(ReadOnlySpan<byte> source)
        {
            source.CopyTo(new Span<byte>(writeBuffer, writeOffset, source.Length));
            writeOffset += source.Length;
        }

        private Task FlushAsync()
        {
            if (writeOffset > 0)
            {
                Task task = stream.WriteAsync(writeBuffer, 0, writeOffset);
                writeOffset = 0;
                return task;
            }
            return default;
        }

        public void Dispose()
        {
            stream?.Dispose();
        }
    }

    ///////////////////////////////////////////////////////////////////////
    // Listings 14-32, 14-33, 14-34
    public class FlexibleBufferedWriter : IDisposable
    {
        private const int MinimumWriteBufferSize = 32;
        private readonly IMemoryOwner<byte> memoryOwner;
        private readonly Stream stream;
        private int writeOffset = 0;

        public FlexibleBufferedWriter(Stream stream, IMemoryOwner<byte> memoryOwner)
        {
            //Debug.Assert(memoryOwner.Memory.Length > MinimumWriteBufferSize);
            this.stream = stream;
            this.memoryOwner = memoryOwner;
        }

        public async Task WriteAsync(ReadOnlyMemory<byte> source)
        {
            Memory<int> v = new Memory<int>(new int[2]);
            int remaining = memoryOwner.Memory.Length - writeOffset;
            if (source.Length <= remaining)
            {
                // Fits in current write buffer. Just copy and return.
                WriteToBuffer(source.Span);
                return;
            }
            while (source.Length > 0)
            {
                byte[] sharedBuffer = null;
                // Fit what we can in the current write buffer and flush it.
                remaining = Math.Min(memoryOwner.Memory.Length - writeOffset, source.Length);
                WriteToBuffer(source.Slice(0, remaining).Span);
                source = source.Slice(remaining);
                await FlushAsync(out sharedBuffer).ConfigureAwait(false);
                if (sharedBuffer != null)
                    ArrayPool<byte>.Shared.Return(sharedBuffer);
            }
        }
        private void WriteToBuffer(ReadOnlySpan<byte> source)
        {
            source.CopyTo(memoryOwner.Memory.Span.Slice(writeOffset, source.Length));
            writeOffset += source.Length;
        }

        private Task FlushAsync(out byte[] sharedBuffer)
        {
            sharedBuffer = null;
            if (writeOffset > 0)
            {
                Task result;
                if (MemoryMarshal.TryGetArray(memoryOwner.Memory, out ArraySegment<byte> array))
                {
                    result = stream.WriteAsync(array.Array, array.Offset, writeOffset);
                }
                else
                {
                    sharedBuffer = ArrayPool<byte>.Shared.Rent(writeOffset);
                    memoryOwner.Memory.Span.Slice(0, writeOffset).CopyTo(sharedBuffer);
                    result = stream.WriteAsync(sharedBuffer, 0, writeOffset);
                }
                //Task task = stream.WriteAsync(writeBuffer, 0, writeOffset);
                writeOffset = 0;
                return result;
            }
            return default;
        }

        public void Dispose()
        {
            stream?.Dispose();
            memoryOwner?.Dispose();
        }
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 14-31
    public class Worker : IDisposable
    {
        private readonly IMemoryOwner<byte> memoryOwner;

        public Worker(IMemoryOwner<byte> memoryOwner)
        {
            this.memoryOwner = memoryOwner;
        }

        public void UseMemory()
        {
            //ConsumeMemory(memoryOwner.Memory);
        }

        public void Dispose()
        {
            this.memoryOwner?.Dispose();
        }
    }

    public class MemoryExamples
    {
        public void Tests() // This method is Owner because it holds IMemoryOwner reference (and must Dispose it)
        {
            // Memory with implicit owner (GC root)
            byte[] array = new byte[] {1, 2, 3};
            Memory<byte> memory1 = new Memory<byte>(array);
            //Memory<int> mempory = new Memory<int>(array, start: 1, length: 2);

            ReadOnlyMemory<char> memory2 = "Hello world".AsMemory();
            //ReadOnlyMemory<char> memory2 = "Hello world".AsMemory(start: 2, length: 3);

            Memory<int> pooledMemory = new Memory<int>(ArrayPool<int>.Shared.Rent(100));
            //await Consume(pooledMemory);

            // Memory with explicit owner
            using (IMemoryOwner<int> owner = MemoryPool<int>.Shared.Rent(100))
            {
                Memory<int> memory3 = owner.Memory;
                Span<int> span = memory3.Span;
                
                MemoryHandle handle = memory3.Pin(); // Calls owner's Pin
                handle.Dispose();
            }

            using (var stream = new MemoryStream())
            {
                FlexibleBufferedWriter bw = new FlexibleBufferedWriter(stream, MemoryPool<byte>.Shared.Rent(16));
                bw.WriteAsync(Encoding.UTF8.GetBytes("Hello world from Poland here!").AsMemory()).Wait();
                stream.Position = 0;
                string result = new StreamReader(stream).ReadToEnd();
                Console.WriteLine(result);
            }
        }

        //public static async Task<string> GetDirectorySizeAsync(ReadOnlySpan<char> requestUrl) // Error CS4012  Parameters or locals of type 'ReadOnlySpan<char>' cannot be declared in async methods or lambda expressions.
        //{
        //    HttpClient client = new HttpClient();
        //    var task = client.GetStringAsync(requestUrl.ToString());
        //    return await task;
        //}

        public static async Task<string> GetDirectorySizeAsync(ReadOnlyMemory<char> requestUrl)
        {
            HttpClient client = new HttpClient();
            var task = client.GetStringAsync(requestUrl.Span.ToString());
            return await task;
        }
    }

    public class CustomOwnedMemory : MemoryManager<int>
    {
        protected override void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        public override Span<int> GetSpan()
        {
            throw new NotImplementedException();
        }

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override void Unpin()
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Span
    public ref struct RefStruct
    {

    }

    public unsafe struct FixedBufferStruct
    {
        public fixed char Buffer[16];
        public char[] Array;

        public void Method()
        {
            fixed (char* ch = Buffer)
            {
                Span<char> span = new Span<char>(ch, 16);
            }
            Span<char> span2 = new Span<char>(Array);
        }
    }

    public class Utility
    {
        public static int[] ReturnArray() => new int[32];
    }

    public class SpanExamples
    {
        //public static async Task<int> CheckAsync()
        //{
        //    Span<int> span = new Span<int>(new int[] {});
        //    await Task.Delay(1000);
        //    return await Task.FromResult<int>(0);
        //}

        unsafe public void UseSpan()
        {
            var array = new int[64];
            Span<int> span1 = new Span<int>(array);
            Span<int> span2 = new Span<int>(array, start: 8, length: 4);
            Span<int> span3 = span1.Slice(0, 4);

            Span<int> span4 = stackalloc[] { 1, 2, 3, 4, 5 };
            Span<int> span5 = span4.Slice(0, 2);

            IntPtr memory = Marshal.AllocHGlobal(64);
            void* ptr = memory.ToPointer();
            Span<int> span6 = new Span<int>(ptr, 8);

            var span = span1;
            for (int i = 0; i < span.Length; i++)
                Console.WriteLine(span[i]);

            Marshal.FreeHGlobal(memory);
        }

        unsafe public void UseReadOnlySpan()
        {
            var array = new int[64];
            ReadOnlySpan<int> span1 = new ReadOnlySpan<int>(array);
            ReadOnlySpan<int> span2 = new Span<int>(array);

            string str = "Hello world";
            ReadOnlySpan<char> span3 = str.AsSpan();
            ReadOnlySpan<char> span4 = str.AsSpan(start: 6, length: 5);
        }

        public Span<int> ReturnArrayAsSpan()
        {
            var array = new int[64];
            return new Span<int>(array);
        }

        //public unsafe Span<int> ReturnStackallocAsSpan()
        //{
        //    Span<int> span = stackalloc[] { 1, 2, 3, 4, 5 }; // Compilation Error CS8352: Cannot use local 'span' in this context because it may expose referenced variables outside of their declaration scope
        //    return span;
        //}

        public unsafe Span<int> ReturnNativeAsSpan()
        {
            IntPtr memory = Marshal.AllocHGlobal(64);
            return new Span<int>(memory.ToPointer(), 8);
        }

        private const int StackAllocSafeThreshold = 128;
        public void UseSpanNotWisely(int size)
        {
            Span<int> span = size < StackAllocSafeThreshold ? stackalloc int[size] : ArrayPool<int>.Shared.Rent(size);
            for (int i = 0; i < size; ++i)
                Console.WriteLine(span[i]);
            //ArrayPool<int>.Shared.Return(span.ToArray());
        }

        public unsafe void UseSpanWisely(int size)
        {
            int* ptr = default;
            int[] array = null;
            if (size < StackAllocSafeThreshold)
            {
                int* localPtr = stackalloc int[size];
                ptr = localPtr;
            }
            else
            {
                array = ArrayPool<int>.Shared.Rent(size);
            }
            Span<int> span = array ?? new Span<int>(ptr, size);
            for (int i = 0; i < size; ++i)
                Console.WriteLine(span[i]);
            if (array != null) ArrayPool<int>.Shared.Return(array);
        }
    }
    public interface ISome { }
    public ref struct StructWithArray2
    {
        public char[] Text;
    }

    #endregion
    
    #region General types
    public enum SomeEnum
    {
        True,
        False
    }

    public class SomeClass
    {
        public int Field;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void KeepMeAlive()
        {
            Console.WriteLine(Field);
        }
    }

    public struct SomeStruct
    {
        public int Field;

        public void KeepMeAlive()
        {
            Console.WriteLine(Field);
        }

        public void ModifyMe()
        {
            this.Field = 11;
        }
    }

    public class ArrayWrapper
    {
        public int[] Array;
        public int Field;
    }

    public struct Book
    {
        public string Title;
        public string Author;

        public void ModifyAuthor()
        {
            //this.Author = "XXX";
            Console.WriteLine(this.Author);
        }
    }

    public readonly struct ReadonlyBook
    {
        public readonly string Title;
        public readonly string Author;

        public ReadonlyBook(string title, string author)
        {
            this.Title = title;
            this.Author = author;
        }

        public void ModifyAuthor()
        {
            //this.Author = "XXX";
            Console.WriteLine(this.Author);
        }
    }

    public class BookCollection
    {
        private Book[] books = {
            new Book { Title = "Call of the Wild, The", Author = "Jack London" },
            new Book { Title = "Tale of Two Cities, A", Author = "Charles Dickens" }
        };
        private Book nobook = default;
        public ref readonly Book GetBookByTitle(string title)
        {
            for (int ctr = 0; ctr < books.Length; ctr++)
            {
                if (title == books[ctr].Title)
                    return ref books[ctr];
            }
            return ref nobook;
        }

        public void CheckBook(in Book book)
        {
            //book.Title = "XXX";
            book.ModifyAuthor();
        }
    }

    public class ReadOnlyBookCollection
    {
        private ReadonlyBook[] books = {
            new ReadonlyBook("Call of the Wild, The", "Jack London" ),
            new ReadonlyBook("Tale of Two Cities, A", "Charles Dickens")
        };
        private ReadonlyBook nobook = default;
        public ref readonly ReadonlyBook GetBookByTitle(string title)
        {
            for (int ctr = 0; ctr < books.Length; ctr++)
            {
                if (title == books[ctr].Title)
                    return ref books[ctr];
            }
            return ref nobook;
        }

        public void CheckBook(in ReadonlyBook book)
        {
            //book.Title = "XXX";
            book.ModifyAuthor();
        }
    }
    #endregion

}
