//#define STRUCTS
//#define STRUCTS_BY_REF
#define CLASS_BY_REF
using System;

namespace CoreCLR.Types
{
#if STRUCTS
    public struct SomeStruct
    {
        public int Value1;
        public int Value2;
        public int Value3;
        public int Value4;
    }

    public class ExampleClass
    {
        public int Main(int data)
        {
            SomeStruct sd = new SomeStruct();
            sd.Value1 = data;
            return Helper(sd);
        }

        private int Helper(SomeStruct arg)
        {
            return arg.Value1;
        }
    }

    public struct SomeData
    {
        public int Value1;
        public int Value2;
        public int Value3;
        public int Value4;

        public void Bizzarre()
        {
            this = new SomeData();
        }
    }
#elif STRUCTS_BY_REF
    public struct SomeStruct
    {
        public int Value1;
        public int Value2;
        public int Value3;
        public int Value4;
    }

    public class ExampleClass
    {
        public int Main(int data)
        {
            SomeStruct ss = new SomeStruct();
            ss.Value1 = 10;
            Helper(ss); 
            //Helper(ref ss);
            return ss.Value1;
        }

        private void Helper(SomeStruct data)
        {
            data.Value1 = 11;
        }
        private void Helper(ref SomeStruct data)
        {
            data.Value1 = 11;
        }
    }
#elif CLASS_BY_REF
    public class SomeClass
    {
        public int Value1;
        public int Value2;
        public int Value3;
        public int Value4;
    }

    public class ExampleClass
    {
        public int Main(int data)
        {
            SomeClass sc = new SomeClass();
            sc.Value1 = 10;
            Helper(ref sc);
            return sc.Value1;
        }
        private void Helper(ref SomeClass data)
        {
            data = new SomeClass();
            data.Value1 = 11;
        }

    }
#else
    public class SomeClass
    {
        public int Value1;
        public int Value2;
        public int Value3;
        public int Value4;
    }

    public class ExampleClass
    {
        public int Main(int data)
        {
            SomeClass sd = new SomeClass();
            sd.Value1 = data;
            return Helper(sd);
        }

        private int Helper(SomeClass arg)
        {
            return arg.Value1;
        }
    }
#endif
    class Program
    {
        static void Main(string[] args)
        {
            ExampleClass ec = new ExampleClass();
            ec.Main(10);
        }
    }
}
