using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ObjectLayoutInspector;

namespace CoreCLR.ObjectLayout
{
    unsafe class Program
    {
        ///////////////////////////////////////////////////////////////////////
        // Listings 13-79, 13-81
        static void Main(string[] args)
        {
            Console.WriteLine(sizeof(Test1));
            Console.WriteLine(sizeof(Test2));
            //Console.WriteLine(sizeof(Test1o));
            //Console.WriteLine(sizeof(Test2o));
            Console.WriteLine(Unsafe.SizeOf<Test3>());
            Console.WriteLine(Unsafe.SizeOf<Test4>());

            TypeLayout.PrintLayout<SomeClass>();
            TypeLayout layout = TypeLayout.GetLayout<AlignedDouble>();
            Console.WriteLine($"Total size {layout.FullSize}B with {layout.Paddings}B padding.");
            foreach (var fieldBase in layout.Fields)
            {
                switch (fieldBase)
                {
                    case FieldLayout field: Console.WriteLine($"{field.Offset} {field.Size} {field.FieldInfo.Name}"); break;
                    case Padding padding: Console.WriteLine($"{padding.Offset} {padding.Size} Padding"); break;
                }
            }
            //TypeLayout.PrintLayout<UnalignedDoubleExplicit>();
            //var o = new AlignedDouble();
            //Console.ReadLine();
            //GC.KeepAlive(o);
            Console.ReadLine();
        }
    }

    public class SomeClass
    {
        public long Field0;
        public long Field1;
        public long Field2;
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 13-71
    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AlignedDouble
    {
        public byte B;
        public double D;
        public int I;
    }

    ///////////////////////////////////////////////////////////////////////
    // Listings 13-74, 13-75
    public struct AlignedDoubleWithReference
    {
        public byte B;
        public double D;
        public int I;
        //public SomeEnum E;      // Still sequential
        //public SomeStruct AD;   // Still sequential
        //public unsafe void* P;  // Still sequential
        public object O;        // Triggers auto
        //public decimal DE;      // Still sequential (while non-blittable)
        //public DateTime DT;     // Triggers auto!
        //public Guid G;          // Still sequential (while non-blittable)
        //public char C;          // Still sequential (while non-blittable) BTW: alignment 2 bytes
        //public Boolean BL;      // Still sequential (while non-blittable) BTW: alignment 1 byte
    }

    public enum SomeEnum
    {
        Default,
        Max
    }

    [StructLayout(LayoutKind.Auto)]
    public struct AlignedDoubleAuto
    {
        public byte B;
        public double D;
        public int I;
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 13-76
    [StructLayout(LayoutKind.Explicit)]
    public struct UnalignedDoubleExplicit
    {
        [FieldOffset(0)]
        public byte B;
        [FieldOffset(1)]
        public double D;
        [FieldOffset(9)]
        public int I;
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 13-77
    [StructLayout(LayoutKind.Explicit)]
    public struct DiscriminatedUnion
    {
        [FieldOffset(0)]
        public bool Bool;
        [FieldOffset(0)]
        public byte Byte;
        [FieldOffset(0)]
        public int Integer;
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 13-78
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct UnsafeDiscriminatedUnion
    {
        [FieldOffset(0)]
        public bool Bool;
        [FieldOffset(0)]
        public byte Byte;
        [FieldOffset(0)]
        public int Integer;
        [FieldOffset(0)]
        public fixed byte Buffer[8];

    }

    public struct ManyDoubles
    {
        public byte B1;
        public double D1;
        public byte B2;
        public double D2;
        public byte B3;
        public double D3;
        public byte B4;
        public double D5;
    }

    [StructLayout(LayoutKind.Auto)]
    public struct ManyDoublesAuto
    {
        public byte B1;
        public double D1;
        public byte B2;
        public double D2;
        public byte B3;
        public double D3;
        public byte B4;
        public double D5;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UnalignedDouble
    {
        public byte Byte;
        public double Double;
        public int Int;
        public long Long;
        public byte Byte2;
        public byte Byte3;
        public byte Byte4;
    }

    //[StructLayout(LayoutKind.Auto)]
    struct Test1
    {
        byte b;
        double d;
        byte b2;
        double d2;
        byte b3;
    }

    //[StructLayout(LayoutKind.Auto)]
    //[StructLayout(LayoutKind.Sequential)] // default
    //[StructLayout(LayoutKind.Sequential, Pack = 1)] // Poorer performance - benchmark of misaligned double access?!
    //[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode, Size = 10)]
    struct Test2
    {
        double d;
        double d2;
        byte b;
        byte b2;
        byte b3;
    }

    struct Test1o
    {
        byte b;
        double d;
        byte b2;
        double d2;
        byte b3;
        object o;
    }

    //[StructLayout(LayoutKind.Auto)]
    //[StructLayout(LayoutKind.Sequential)] // default
    //[StructLayout(LayoutKind.Sequential, Pack = 1)] // Poorer performance - benchmark of misaligned double access?!
    //[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode, Size = 10)]
    struct Test2o
    {
        double d;
        double d2;
        byte b;
        byte b2;
        byte b3;
        object o;
    }

    class Test3
    {
        byte b;
        double d;
        byte b2;
        double d2;
        byte b3;
    }

    class Test4
    {
        double d;
        double d2;
        byte b;
        byte b2;
        byte b3;
    }

    struct StringId
    {
        public readonly int X;
    }

    class Wrapper1
    {
        public readonly StringId Id1, Id2;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Wrapper2
    {
        public readonly StringId Id1, Id2;
    }
}
