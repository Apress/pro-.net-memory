using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace CoreCLR.CollectScenarios.Scenarios
{
    class EagerRootCollection : ICollectScenario
    {
        public int Run()
        {
            int total = 0;
            Console.WriteLine("Type number to process:");
            string str = Console.ReadLine();
            int input = int.Parse(str);
            Console.WriteLine("Call 0");
            Console.ReadLine();
            total += LexicalScopeExample(input);
            Console.WriteLine("Call 1");
            Console.ReadLine();
            total += RegisterMap(input);
            Console.WriteLine("Call 2");
            Console.ReadLine();
            total += RegisterMap2(input);
            Console.WriteLine("Call 3");
            Console.ReadLine();
            total += StackMap(input);
            Console.WriteLine("Call 4");
            Console.ReadLine();
            total += SimpleCase(input);
            Console.WriteLine("Call 5");
            Console.ReadLine();
            total += SimpleCase2(input);
            Console.WriteLine("Call 6");
            Console.ReadLine();
            total += PassReference(str).Length;

            Console.WriteLine("Finish.");
            Console.ReadLine();
            return total;
        }

        /* This sample shows that local's lifetime ends before Sleep, a lot earlier than its lexical scope.
         * This code is JITted into partialy-interruptible code - only safe points are on method calls
         * (please note each such safepoint resets previous tracked slots, so safepoint without any
         * following '+' means - "there are no live slots till now"
        > !u -gcinfo -o 00007fff43218528
        Normal JIT generated code
        CoreCLR.CollectScenarios.Scenarios.EagerRootCollection.SimpleCase(Int32)
        Begin 00007fff43333450, size 40
        0000 00007fff`43333450 57              push    rdi
        0001 00007fff`43333451 56              push    rsi
        0002 00007fff`43333452 4883ec28        sub     rsp,28h
        0006 00007fff`43333456 8bf2            mov     esi,edx
        0008 00007fff`43333458 48b9e0af3e43ff7f0000 mov rcx,7FFF433EAFE0h (MT: CoreCLR.CollectScenarios.Scenarios.EagerRootCollection+BigClass)
        0012 00007fff`43333462 e81969f65e      call    CoreCLR!JIT_New (00007fff`a2299d80)
        00000017 is a safepoint: 
        0017 00007fff`43333467 488bf8          mov     rdi,rax
        001a 00007fff`4333346a 488bcf          mov     rcx,rdi
        001d 00007fff`4333346d e87e5cd15c      call    System_Private_CoreLib+0xc890f0 (00007fff`a00490f0) (System.Object..ctor(), mdToken: 0000000006000117)
        00000022 is a safepoint: 
        00000021 +rdi
        0022 00007fff`43333472 897708          mov     dword ptr [rdi+8],esi
        0025 00007fff`43333475 8b4f08          mov     ecx,dword ptr [rdi+8]
        0028 00007fff`43333478 e813d6ffff      call    00007fff`43330a90 (System.Console.Write(Int32), mdToken: 0000000006000093)
        0000002d is a safepoint: 
        002d 00007fff`4333347d b9e8030000      mov     ecx,3E8h
        0032 00007fff`43333482 e8e97adf5c      call    System_Private_CoreLib+0xd6af70 (00007fff`a012af70) (Internal.Runtime.Augments.RuntimeThread.Sleep(Int32), mdToken: 00000000060000e7)
        00000037 is a safepoint: 
        0037 00007fff`43333487 33c0            xor     eax,eax
        0039 00007fff`43333489 4883c428        add     rsp,28h
        003d 00007fff`4333348d 5e              pop     rsi
        003e 00007fff`4333348e 5f              pop     rdi
        003f 00007fff`4333348f c3              ret
        */
        private int SimpleCase(int value)
        {
            BigClass local = new BigClass() {Field = value};
            local.DoSomething();
            Thread.Sleep(1000);
            return 0;
        }

        /* This sample shows that local's lifetime ends before loop, a lot earlier than its lexical scope.
         * This code is JITted into fully-interruptible code - safe points are on all most instructions
         * (please note, each line within an interruptible section is safepoint so only differences are
         * printed. For example, at offset 26h, rax content starts to hold live reference.
        > !u -gcinfo -o 00007fff43218560
        Normal JIT generated code
        CoreCLR.CollectScenarios.Scenarios.EagerRootCollection.SimpleCase2(Int32)
        Begin 00007fff433334b0, size 6c
        0000 00007fff`433334b0 57              push    rdi
        0001 00007fff`433334b1 56              push    rsi
        0002 00007fff`433334b2 4883ec38        sub     rsp,38h
        0006 00007fff`433334b6 c5f877          vzeroupper
        0009 00007fff`433334b9 c4e17829742420  vmovaps xmmword ptr [rsp+20h],xmm6
        0010 00007fff`433334c0 8bf2            mov     esi,edx
        00000012 interruptible
        0012 00007fff`433334c2 c4e14857f6      vxorps  xmm6,xmm6,xmm6
        0017 00007fff`433334c7 48b9e0af3e43ff7f0000 mov rcx,7FFF433EAFE0h (MT: CoreCLR.CollectScenarios.Scenarios.EagerRootCollection+BigClass)
        0021 00007fff`433334d1 e8aa68f65e      call    CoreCLR!JIT_New (00007fff`a2299d80)
        00000026 +rax
        0026 00007fff`433334d6 488bf8          mov     rdi,rax
        00000029 +rdi
        0029 00007fff`433334d9 488bcf          mov     rcx,rdi
        0000002c +rcx
        00007fff`433334dc e80f5cd15c      call    System_Private_CoreLib+0xc890f0 (00007fff`a00490f0) (System.Object..ctor(), mdToken: 0000000006000117)
        00000031 -rcx -rax
        0031 00007fff`433334e1 897708          mov     dword ptr [rdi+8],esi
        0034 00007fff`433334e4 8b4f08          mov     ecx,dword ptr [rdi+8]
        0037 00007fff`433334e7 e8b8f3ffff      call    00007fff`433328a4 (System.Console.Write(Int32), mdToken: 0000000006000093)
        0000003c -rdi
        003c 00007fff`433334ec 33ff            xor     edi,edi
        003e 00007fff`433334ee 85f6            test    esi,esi
        0040 00007fff`433334f0 7e1a            jle     00007fff`4333350c
        0042 00007fff`433334f2 c4e17857c0      vxorps  xmm0,xmm0,xmm0
        0047 00007fff`433334f7 c4e17b2ac7      vcvtsi2sd xmm0,xmm0,edi
        004c 00007fff`433334fc e8af7b835f      call    CoreCLR!COMDouble::Sin (00007fff`a2b6b0b0)
        0051 00007fff`43333501 c4e14b58f0      vaddsd  xmm6,xmm6,xmm0
        0056 00007fff`43333506 ffc7            inc     edi
        0058 00007fff`43333508 3bfe            cmp     edi,esi
        005a 00007fff`4333350a 7ce6            jl      00007fff`433334f2
        005c 00007fff`4333350c 33c0            xor     eax,eax
        0000005e not interruptible
        005e 00007fff`4333350e c4e17828742420  vmovaps xmm6,xmmword ptr [rsp+20h]
        0065 00007fff`43333515 4883c438        add     rsp,38h
        0069 00007fff`43333519 5e              pop     rsi
        006a 00007fff`4333351a 5f              pop     rdi
        006b 00007fff`4333351b c3              ret
        */
        private int SimpleCase2(int value)
        {
            double total = 0.0;
            BigClass local = new BigClass() { Field = value };
            local.DoSomething();
            for (int i = 0; i < value; ++i)
            {
                total += Math.Sin(i); // * Math.Pow(i, Math.PI);
            }
            return 0;
        }

        /* This sample illustrates lexical scope vs lifetime (thanks to JIT's eager root collection).
         * Both local and sc are collected much earlier than their lexical scope.
        !u -gcinfo 00007ff81c4c8598
        Normal JIT generated code
        CoreCLR.CollectScenarios.Scenarios.EagerRootCollection.LexicalScopeExample(Int32)
        Begin 00007ff81c5e3310, size 71
        00007ff8`1c5e3310 57              push    rdi
        00007ff8`1c5e3311 56              push    rsi
        00007ff8`1c5e3312 4883ec28        sub     rsp,28h
        00007ff8`1c5e3316 8bf2            mov     esi,edx
        00007ff8`1c5e3318 48b908ad691cf87f0000 mov rcx,7FF81C69AD08h (MT: CoreCLR.CollectScenarios.Scenarios.EagerRootCollection+BigClass)
        00007ff8`1c5e3322 e8596af65e      call    CoreCLR!JIT_New (00007ff8`7b549d80)
        00000017 is a safepoint: 
        00007ff8`1c5e3327 488bf8          mov     rdi,rax
        00007ff8`1c5e332a 488bcf          mov     rcx,rdi
        00007ff8`1c5e332d e8be5d0b5e      call    System_Private_CoreLib+0xc890f0 (00007ff8`7a6990f0) (System.Object..ctor(), mdToken: 0000000006000117)
        00000022 is a safepoint: 
        00000021 +rdi
        00007ff8`1c5e3332 897708          mov     dword ptr [rdi+8],esi
        00007ff8`1c5e3335 488bcf          mov     rcx,rdi
        00007ff8`1c5e3338 e87bf8ffff      call    00007ff8`1c5e2bb8 (CoreCLR.CollectScenarios.Scenarios.EagerRootCollection+BigClass.Check(), mdToken: 000000000600005a)
        0000002d is a safepoint: 
        00007ff8`1c5e333d 85c0            test    eax,eax
        00007ff8`1c5e333f 7437            je      00007ff8`1c5e3378
        00007ff8`1c5e3341 48b9e8af691cf87f0000 mov rcx,7FF81C69AFE8h (MT: CoreCLR.CollectScenarios.Scenarios.EagerRootCollection+SomeClass)
        00007ff8`1c5e334b e8b0e14b5f      call    CoreCLR!JIT_TrialAllocSFastMP_InlineGetThread (00007ff8`7baa1500)
        00000040 is a safepoint: 
        00007ff8`1c5e3350 488bf8          mov     rdi,rax
        00007ff8`1c5e3353 488bcf          mov     rcx,rdi
        00007ff8`1c5e3356 e8955d0b5e      call    System_Private_CoreLib+0xc890f0 (00007ff8`7a6990f0) (System.Object..ctor(), mdToken: 0000000006000117)
        0000004b is a safepoint: 
        0000004a +rdi
        00007ff8`1c5e335b 488bcf          mov     rcx,rdi
        00007ff8`1c5e335e 8bd6            mov     edx,esi
        00007ff8`1c5e3360 e87bf8ffff      call    00007ff8`1c5e2be0 (CoreCLR.CollectScenarios.Scenarios.EagerRootCollection+SomeClass.CalculateSomething(Int32), mdToken: 0000000006000054)
        00000055 is a safepoint: 
        00007ff8`1c5e3365 8bc8            mov     ecx,eax
        00007ff8`1c5e3367 e8047c195e      call    System_Private_CoreLib+0xd6af70 (00007ff8`7a77af70) (Internal.Runtime.Augments.RuntimeThread.Sleep(Int32), mdToken: 00000000060000e7)
        0000005c is a safepoint: 
        00007ff8`1c5e336c b801000000      mov     eax,1
        00007ff8`1c5e3371 4883c428        add     rsp,28h
        00007ff8`1c5e3375 5e              pop     rsi
        00007ff8`1c5e3376 5f              pop     rdi
        00007ff8`1c5e3377 c3              ret
        00007ff8`1c5e3378 33c0            xor     eax,eax
        00007ff8`1c5e337a 4883c428        add     rsp,28h
        00007ff8`1c5e337e 5e              pop     rsi
        00007ff8`1c5e337f 5f              pop     rdi
        00007ff8`1c5e3380 c3              ret        
         */

        ///////////////////////////////////////////////////////////////////////
        // Listing 8-5
        private int LexicalScopeExample(int value)
        {
            BigClass local = new BigClass() { Field = value };
            if (local.Check())
            {
                SomeClass sc = new SomeClass();
                int data = sc.CalculateSomething(value);
                Thread.Sleep(data); // or DoSomeLongRunningCall(data);
                return 1;
            }
            return 0;
        }

        /* 
        > !u -gcinfo 00007fff42c18518
        Normal JIT generated code
        CoreCLR.CollectScenarios.Scenarios.EagerRootCollection.RegisterMap(Int32)
        Begin 00007fff42d32f20, size 3d
        00007fff`42d32f20 57              push    rdi
        00007fff`42d32f21 56              push    rsi
        00007fff`42d32f22 4883ec28        sub     rsp,28h
        00007fff`42d32f26 8bf2            mov     esi,edx
        00000008 interruptible
        00007fff`42d32f28 33ff            xor     edi,edi
        00007fff`42d32f2a 48b9c8aade42ff7f0000 mov rcx,7FFF42DEAAC8h (MT: CoreCLR.CollectScenarios.Scenarios.EagerRootCollection+SomeClass)
        00007fff`42d32f34 e8c7e54b5f      call    CoreCLR!JIT_TrialAllocSFastMP_InlineGetThread (00007fff`a21f1500)
        00000019 +rax
        00007fff`42d32f39 488bc8          mov     rcx,rax
        0000001c +rcx
        00007fff`42d32f3c e8af610b5e      call    System_Private_CoreLib+0xc890f0 (00007fff`a0de90f0) (System.Object..ctor(), mdToken: 0000000006000117)
        00000021 -rcx -rax
        00007fff`42d32f41 33c0            xor     eax,eax
        00007fff`42d32f43 85f6            test    esi,esi
        00007fff`42d32f45 7e0d            jle     00007fff`42d32f54
        00007fff`42d32f47 8bd0            mov     edx,eax
        00007fff`42d32f49 0fafd0          imul    edx,eax
        00007fff`42d32f4c 03fa            add     edi,edx
        00007fff`42d32f4e ffc0            inc     eax
        00007fff`42d32f50 3bc6            cmp     eax,esi
        00007fff`42d32f52 7cf3            jl      00007fff`42d32f47
        00007fff`42d32f54 8bc7            mov     eax,edi
        00000036 not interruptible
        00007fff`42d32f56 4883c428        add     rsp,28h
        00007fff`42d32f5a 5e              pop     rsi
        00007fff`42d32f5b 5f              pop     rdi
        00007fff`42d32f5c c3              ret
        */

        ///////////////////////////////////////////////////////////////////////
        // Listing 8-21
        [MethodImpl(MethodImplOptions.NoInlining)]
        private int RegisterMap(int value)
        {
            int total = 0;
            SomeClass local = new SomeClass(); 
            // local is unreachable since this line - DoSomeStuff logic has been inlined so there is no need to keep it
            for (int i = 0; i < value; ++i)
            {
                total += local.DoSomeStuff(i);
            }
            return total;
        }

        /*
        > !u -gcinfo 00007fff433d8550
        Normal JIT generated code
        CoreCLR.CollectScenarios.Scenarios.EagerRootCollection.RegisterMap2(Int32)
        Begin 00007fff434f2f80, size 52
        00007fff`434f2f80 57              push    rdi
        00007fff`434f2f81 56              push    rsi
        00007fff`434f2f82 55              push    rbp
        00007fff`434f2f83 53              push    rbx
        00007fff`434f2f84 4883ec28        sub     rsp,28h
        00007fff`434f2f88 488bf9          mov     rdi,rcx
        00007fff`434f2f8b 8bf2            mov     esi,edx
        0000000d interruptible
        0000000d +rdi
        00007fff`434f2f8d 33db            xor     ebx,ebx
        00007fff`434f2f8f 48b988ab5a43ff7f0000 mov rcx,7FFF435AAB88h (MT: CoreCLR.CollectScenarios.Scenarios.EagerRootCollection+SomeClass)
        00007fff`434f2f99 e862e54a5f      call    CoreCLR!JIT_TrialAllocSFastMP_InlineGetThread (00007fff`a29a1500)
        0000001e +rax
        00007fff`434f2f9e 488be8          mov     rbp,rax
        00000021 +rbp
        00007fff`434f2fa1 488bcd          mov     rcx,rbp
        00000024 +rcx
        00007fff`434f2fa4 e847610a5e      call    System_Private_CoreLib+0xc890f0 (00007fff`a15990f0) (System.Object..ctor(), mdToken: 0000000006000117)
        00000029 -rcx -rax
        00007fff`434f2fa9 488bd5          mov     rdx,rbp
        0000002c +rdx
        00007fff`434f2fac 33c9            xor     ecx,ecx
        00007fff`434f2fae 85f6            test    esi,esi
        00007fff`434f2fb0 7e0d            jle     00007fff`434f2fbf
        00000032 -rbp
        00007fff`434f2fb2 8bc1            mov     eax,ecx
        00007fff`434f2fb4 0fafc1          imul    eax,ecx
        00007fff`434f2fb7 03d8            add     ebx,eax
        00007fff`434f2fb9 ffc1            inc     ecx
        00007fff`434f2fbb 3bce            cmp     ecx,esi
        00007fff`434f2fbd 7cf3            jl      00007fff`434f2fb2
        00007fff`434f2fbf 488bcf          mov     rcx,rdi
        00000042 +rcx
        00007fff`434f2fc2 e8c1d9ffff      call    00007fff`434f0988 (CoreCLR.CollectScenarios.Scenarios.EagerRootCollection.SomeHelper(SomeClass), mdToken: 000000000600000e)
        00000047 -rdi -rdx -rcx
        00007fff`434f2fc7 8bc3            mov     eax,ebx
        00000049 not interruptible
        00007fff`434f2fc9 4883c428        add     rsp,28h
        00007fff`434f2fcd 5b              pop     rbx
        00007fff`434f2fce 5d              pop     rbp
        00007fff`434f2fcf 5e              pop     rsi
        00007fff`434f2fd0 5f              pop     rdi
        00007fff`434f2fd1 c3              ret
        */
        [MethodImpl(MethodImplOptions.NoInlining)]
        private int RegisterMap2(int value)
        {
            int total = 0;
            SomeClass local = new SomeClass();
            for (int i = 0; i < value; ++i)
            {
                total += local.DoSomeStuff(i);
            }
            SomeHelper(local);
            // local is unreachable since this line - SomeHelper has inlining disabled
            return total;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private unsafe int StackMap(int value)
        {
            var sc = new SomeClass();
            int total = 0;
            total += sc.DoSomeStuff(value);
            for (int i = 0; i < value; ++i)
            {
                var ss = new SomeStruct();
                ss.F1 = value;
                total += (int)Math.Sin(ss.DoMagic());
            }
            return total;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private string PassReference(string input)
        {
            return input;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SomeHelper(SomeClass c)
        {
            
        }

        class SomeClass
        {
            public int F1;
            public int F2;

            [MethodImpl(MethodImplOptions.NoInlining)]
            public int Sum() => F1 + F2;

            public int DoSomeStuff(int value)
            {
                return value * value;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public int CalculateSomething(int value)
            {
                return value * value;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public int DoSomeStuffForced(SomeStruct ss)
            {
                return ss.RefField.F1 * ss.RefField.F2 + ss.F1 + ss.F2 + ss.F3 + ss.F4 + ss.F5 + ss.F6 + ss.F7 + ss.F8;
            }

            public void DoSomething()
            {
                Console.Write(F1);
            }
        }

        class BigClass
        {
            public int Field;

            public void DoSomething()
            {
                Console.Write(this.Field);
            }

            ~BigClass()
            {
                Console.WriteLine("Dying...");
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public bool Check()
            {
                Random rand = new Random();
                return rand.Next(10) < 5;
            }
        }

        struct SomeStruct
        {
            public SomeClass RefField;
            public int F1;
            public int F2;
            public int F3;
            public int F4;
            public int F5;
            public int F6;
            public int F7;
            public int F8;

            public int DoMagic()
            {
                Random rand = new Random();
                int next = rand.Next(8);
                switch (next)
                {
                    case 0: return F1; break;
                    case 1: return F2; break;
                    case 2: return F3; break;
                    case 3: return F4; break;
                    case 4: return F5; break;
                    case 5: return F6; break;
                    case 6: return F7; break;
                    case 7: return RefField.DoSomeStuff(next); break;
                }
                return 1;
            }
        }
    }
}

