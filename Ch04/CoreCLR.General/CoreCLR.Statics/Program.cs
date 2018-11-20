using System;
using System.Runtime.CompilerServices;

namespace CoreCLR.Statics
{
    /*
    > !dumpheap -type CoreCLR
    ...
    Statistics:
                  MT    Count    TotalSize Class Name
    00007ff93c8a5680        1           24 CoreCLR.Statics.C
    00007ff93c8a6b90        2           48 CoreCLR.Statics.R
    00007ff93c8a5760        2           48 CoreCLR.Statics.S
Total 5 objects
     */
    class Program
    {
        /*
        00007ff9`3c9c14a0 56              push    rsi
        00007ff9`3c9c14a1 4883ec20        sub     rsp,20h
        00007ff9`3c9c14a5 48b980568a3cf97f0000 mov rcx,7FF93C8A5680h (MT: CoreCLR.Statics.C)
        00007ff9`3c9c14af e83cecab5f      call    coreclr!JIT_TrialAllocSFastMP_InlineGetThread (00007ff9`9c4800f0)
        00007ff9`3c9c14b4 488bf0          mov     rsi,rax

        00007ff9`3c9c14b7 488bce          mov     rcx,rsi
        00007ff9`3c9c14ba e8f1fbffff      call    00007ff9`3c9c10b0 (CoreCLR.Statics.C.Method1(), mdToken: 0000000006000003)

        00007ff9`3c9c14bf 488bce          mov     rcx,rsi
        00007ff9`3c9c14c2 e8f1fbffff      call    00007ff9`3c9c10b8 (CoreCLR.Statics.C.Method2(), mdToken: 0000000006000004)

        00007ff9`3c9c14c7 488bce          mov     rcx,rsi
        00007ff9`3c9c14ca e8f1fbffff      call    00007ff9`3c9c10c0 (CoreCLR.Statics.C.Method3(), mdToken: 0000000006000005)

        00007ff9`3c9c14cf 488bce          mov     rcx,rsi
        00007ff9`3c9c14d2 e8f1fbffff      call    00007ff9`3c9c10c8 (CoreCLR.Statics.C.Method4(), mdToken: 0000000006000006)

        00007ff9`3c9c14d7 488bce          mov     rcx,rsi
        00007ff9`3c9c14da e8f1fbffff      call    00007ff9`3c9c10d0 (CoreCLR.Statics.C.Method5(), mdToken: 0000000006000007)

        00007ff9`3c9c14df 488bce          mov     rcx,rsi
        00007ff9`3c9c14e2 e8f1fbffff      call    00007ff9`3c9c10d8 (CoreCLR.Statics.C.Method6(), mdToken: 0000000006000008)

        00007ff9`3c9c14e7 48b9a04b8a3cf97f0000 mov rcx,7FF93C8A4BA0h
        00007ff9`3c9c14f1 ba04000000      mov     edx,4
        00007ff9`3c9c14f6 e8f5efab5f      call    coreclr!JIT_GetSharedNonGCStaticBase_SingleAppDomain (00007ff9`9c4804f0)
        00007ff9`3c9c14fb 48ba4829001095010000 mov rdx,19510002948h
        00007ff9`3c9c1505 488b12          mov     rdx,qword ptr [rdx]
        00007ff9`3c9c1508 8b5208          mov     edx,dword ptr [rdx+8]
        00007ff9`3c9c150b 488bce          mov     rcx,rsi
        00007ff9`3c9c150e e8cdfbffff      call    00007ff9`3c9c10e0 (CoreCLR.Statics.C.Method7(CoreCLR.Statics.S), mdToken: 0000000006000009)

        00007ff9`3c9c1513 e820feffff      call    00007ff9`3c9c1338 (System.Console.ReadLine(), mdToken: 0000000006000075)

        00007ff9`3c9c1518 90              nop
        00007ff9`3c9c1519 4883c420        add     rsp,20h
        00007ff9`3c9c151d 5e              pop     rsi
        00007ff9`3c9c151e c3              ret
         */
        static void Main(string[] args)
        {
            C c = new C();
            c.Method1();
            c.Method2();
            c.Method3();
            c.Method4();
            c.Method5();
            c.Method6();
            c.Method7(ExampleStruct.StaticStruct);
            Console.ReadLine();
        }
    }

    public class C
    {
        /*
        00007ff9`3c9c1540 4883ec28        sub     rsp,28h
        00007ff9`3c9c1544 48b9a04b8a3cf97f0000 mov rcx,7FF93C8A4BA0h <- 7FF93C8A4BA0h in Managed Heap / Domain 1 High Frequency Heap
        00007ff9`3c9c154e ba03000000      mov     edx,3
        00007ff9`3c9c1553 e898efab5f      call    coreclr!JIT_GetSharedNonGCStaticBase_SingleAppDomain (00007ff9`9c4804f0)
        00007ff9`3c9c1558 8b0d7a36eeff    mov     ecx,dword ptr [00007ff9`3c8a4bd8] <- 00007ff9`3c8a4bd8 in Managed Heap / Domain 1 High Frequency Heap
        00007ff9`3c9c155e e81dfeffff      call    00007ff9`3c9c1380 (System.Console.WriteLine(Int32), mdToken: 000000000600007e)
        00007ff9`3c9c1563 90              nop
        00007ff9`3c9c1564 4883c428        add     rsp,28h
        00007ff9`3c9c1568 c3              ret  
         */
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Method1()
        {
            Console.WriteLine(ExampleClass.StaticPrimitive);
        }
        /*
        00007ff9`3c9c2b00 4883ec28        sub     rsp,28h
        00007ff9`3c9c2b04 48b93829001095010000 mov rcx,19510002938h <- in LOH
        00007ff9`3c9c2b0e 488b09          mov     rcx,qword ptr [rcx]
        00007ff9`3c9c2b11 8b4908          mov     ecx,dword ptr [rcx+8]
        00007ff9`3c9c2b14 e847000000      call    00007ff9`3c9c2b60 (System.Console.WriteLine(Int32), mdToken: 000000000600007e)
        00007ff9`3c9c2b19 90              nop
        00007ff9`3c9c2b1a 4883c428        add     rsp,28h
        00007ff9`3c9c2b1e c3              ret
         */
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Method2()
        {
            Console.WriteLine(ExampleClass.StaticStruct.Value);
        }
        /*
        00007ff9`3c9c2cf0 4883ec28        sub     rsp,28h
        00007ff9`3c9c2cf4 48b94029001095010000 mov rcx,19510002940h <- in LOH
        00007ff9`3c9c2cfe 488b09          mov     rcx,qword ptr [rcx]
        00007ff9`3c9c2d01 8b4908          mov     ecx,dword ptr [rcx+8]
        00007ff9`3c9c2d04 e857feffff      call    00007ff9`3c9c2b60 (System.Console.WriteLine(Int32), mdToken: 000000000600007e)
        00007ff9`3c9c2d09 90              nop
        00007ff9`3c9c2d0a 4883c428        add     rsp,28h
        00007ff9`3c9c2d0e c3              ret
         */
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Method3()
        {
            Console.WriteLine(ExampleClass.StaticObject.Value);
        }
        /*
        00007ff9`3c9c2d30 4883ec28        sub     rsp,28h
        00007ff9`3c9c2d34 48b9a04b8a3cf97f0000 mov rcx,7FF93C8A4BA0h <- in Managed Heap / Domain 1 High Frequency Heap
        00007ff9`3c9c2d3e ba04000000      mov     edx,4
        00007ff9`3c9c2d43 e8a8d7ab5f      call    coreclr!JIT_GetSharedNonGCStaticBase_SingleAppDomain (00007ff9`9c4804f0)
        00007ff9`3c9c2d48 48b9dc4b8a3cf97f0000 mov rcx,7FF93C8A4BDCh <- in Managed Heap / Domain 1 High Frequency Heap
        00007ff9`3c9c2d52 8b09            mov     ecx,dword ptr [rcx]
        00007ff9`3c9c2d54 e807feffff      call    00007ff9`3c9c2b60 (System.Console.WriteLine(Int32), mdToken: 000000000600007e)
        00007ff9`3c9c2d59 90              nop
        00007ff9`3c9c2d5a 4883c428        add     rsp,28h
        00007ff9`3c9c2d5e c3              ret
         */
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Method4()
        {
            Console.WriteLine(ExampleStruct.StaticPrimitive);
        }
        /*
        00007ff9`3c9c2de0 4883ec28        sub     rsp,28h
        00007ff9`3c9c2de4 48b94829001095010000 mov rcx,19510002948h <- in LOH
        00007ff9`3c9c2dee 488b09          mov     rcx,qword ptr [rcx]
        00007ff9`3c9c2df1 8b4908          mov     ecx,dword ptr [rcx+8]
        00007ff9`3c9c2df4 e867fdffff      call    00007ff9`3c9c2b60 (System.Console.WriteLine(Int32), mdToken: 000000000600007e)
        00007ff9`3c9c2df9 90              nop
        00007ff9`3c9c2dfa 4883c428        add     rsp,28h
        00007ff9`3c9c2dfe c3              ret
         */
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Method5()
        {
            Console.WriteLine(ExampleStruct.StaticStruct.Value);
        }
        /*
        00007ff9`3c9c2e20 4883ec28        sub     rsp,28h
        00007ff9`3c9c2e24 48b95029001095010000 mov rcx,19510002950h <- in LOH
        00007ff9`3c9c2e2e 488b09          mov     rcx,qword ptr [rcx]
        00007ff9`3c9c2e31 8b4908          mov     ecx,dword ptr [rcx+8]
        00007ff9`3c9c2e34 e827fdffff      call    00007ff9`3c9c2b60 (System.Console.WriteLine(Int32), mdToken: 000000000600007e)
        00007ff9`3c9c2e39 90              nop
        00007ff9`3c9c2e3a 4883c428        add     rsp,28h
        00007ff9`3c9c2e3e c3              ret
         */
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Method6()
        {
            Console.WriteLine(ExampleStruct.StaticObject.Value);
        }
        /*
        00007ff9`3c9c2e60 4883ec28        sub     rsp,28h
        00007ff9`3c9c2e64 8bca            mov     ecx,edx
        00007ff9`3c9c2e66 e8f5fcffff      call    00007ff9`3c9c2b60 (System.Console.WriteLine(Int32), mdToken: 000000000600007e)
        00007ff9`3c9c2e6b 90              nop
        00007ff9`3c9c2e6c 4883c428        add     rsp,28h
        00007ff9`3c9c2e70 c3              ret
         */
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Method7(S s)
        {
            Console.WriteLine(s.Value);
        }
    }

    public class ExampleClass
    {
        public static int StaticPrimitive;
        public static S StaticStruct;
        public static R StaticObject = new R();
    }

    public struct ExampleStruct
    {
        public static int StaticPrimitive;
        public static S StaticStruct;
        public static R StaticObject = new R();
    }

    public class R
    {
        public int Value;
    }

    public struct S
    {
        public int Value;
    }
}
    