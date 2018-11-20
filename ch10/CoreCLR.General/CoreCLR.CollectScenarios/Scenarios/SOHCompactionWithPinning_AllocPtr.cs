using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCLR.CollectScenarios.Scenarios
{
    /*
    With initial cleanup
        
    Just before blocking compacting GC:

    0000025e5cdff718 00007ff8a0be7b40       72     "Creating objects layout"
    0000025e5cdff760 00007ff8a0be7b40       94     "Just before blocking compacting GC"
    0000025e5cdff7c0 00007ff8a0be7b40      102     "After GC... Just before new allocation"
    0000025e5cdff828 00007ff8a0be7b40       64     "After allocation..."
    0000025e5cdff868 0000025e5b28f020       24     Free				<- gen0 start
    0000025e5cdff880 00007ff841470ff0       24     A
    0000025e5cdff898 00007ff8414710a0       24     B (reachable)
    0000025e5cdff8b0 00007ff841471190       40     C
    0000025e5cdff8d8 00007ff841471270       32     D (reachable)
    0000025e5cdff8f8 00007ff841470f40       24     E (pinned)
    0000025e5cdff910 00007ff841471360       40     F
    0000025e5cdff938 00007ff841471420       24     G (reachable)

    After GC... Just before new allocation:
    
    0000025e5cdff718 00007ff8a0be7b40       72     "Creating objects layout"
    0000025e5cdff760 00007ff8a0be7b40       94     "Just before blocking compacting GC"
    0000025e5cdff7c0 00007ff8a0be7b40      102     "After GC... Just before new allocation"
    0000025e5cdff828 00007ff8a0be7b40       64     "After allocation..."
    0000025e5cdff868 0000025e5b28f020       24     Free
    0000025e5cdff880 00007ff8414710a0       24     B  
    0000025e5cdff898 00007ff841471270       32     D
    0000025e5cdff8b8 0000025e5b28f020       62     Free
    0000025e5cdff8f8 00007ff841470f40       24     E (pinned)
    0000025e5cdff910 0000025e5b28f020       24     Free             - to jest ciekawe, G nie zostało przysunięte do E tylko została dziura (żeby w przyszłości nie robić extended pinned?)
    0000025e5cdff928 00007ff841471420       24     G
    0000025e5cdff940 0000025e5b28f020       24     Free				<- gen0 start

    After allocation...
  
    0000025e5cdff718 00007ff8a0be7b40       72     "Creating objects layout"
    0000025e5cdff760 00007ff8a0be7b40       94     "Just before blocking compacting GC"
    0000025e5cdff7c0 00007ff8a0be7b40      102     "After GC... Just before new allocation"
    0000025e5cdff828 00007ff8a0be7b40       64     "After allocation..."
    0000025e5cdff868 0000025e5b28f020       24     Free
    0000025e5cdff880 00007ff8414710a0       24     B
    0000025e5cdff898 00007ff841471270       32     D
    0000025e5cdff8b8 0000025e5b28f020       62     Free
    0000025e5cdff8f8 00007ff841470f40       24     E (pinned)
    0000025e5cdff910 0000025e5b28f020       24     Free
    0000025e5cdff928 00007ff841471420       24     G  
    0000025e5cdff940 0000025e5b28f020       24     Free				<- gen0 start
    0000025e5cdff958 00007ff8a0be7b40       28     "3"              - wczytane z klawiatury
    0000025e5cdff978 00007ff8414714d0       24     H


    Without initial cleanup

    Just before blocking compacting GC:

    00000163942b1030 0000025e5b28f020       24     					<- gen0 start
    ...
    00000163942d6b90 00007ff8a0be7b40       72     "Creating objects layout"
    00000163942d6bd8 00007ff8a0be7b40       94     "Just before blocking compacting GC"
    00000163942d6c38 00007ff8a0be7b40      102     "After GC... Just before new allocation"
    00000163942d6ca0 00007ff8a0be7b40       64     "After allocation..."
    00000163942d6ce0 00007ff841470ff0       24     A
    00000163942d6cf8 00007ff8414710a0       24     B (reachable)
    00000163942d6d10 00007ff841471190       40     C
    00000163942d6d38 00007ff841471270       32     D (reachable)
    00000163942d6d58 00007ff841470f40       24     E (pinned)
    00000163942d6d70 00007ff841471360       40     F
    00000163942d6d98 00007ff841471420       24     G (reachable)

    After GC... Just before new allocation:

    00000163942bf718 00007ff8a0be7b40       72     "Creating objects layout"
    00000163942bf760 00007ff8a0be7b40       94     "Just before blocking compacting GC"
    00000163942bf7c0 00007ff8a0be7b40      102     "After GC... Just before new allocation"
    00000163942bf828 00007ff8a0be7b40       64     "After allocation..."
    00000163942bf868 00007ff8414710a0       24     B
    00000163942bf880 00007ff841471270       32     D
    00000163942bf8a0 00007ff841471420       24     G
    00000163942bf8b8 000001639297fa70       24 Free					<- gen0 start
    00000163942bf8d0 000001639297fa70    95366 Free
    00000163942d6d58 00007ff841470f40       24     E

    After allocation...

    00000163942bf718 00007ff8a0be7b40       72     "Creating objects layout"
    00000163942bf760 00007ff8a0be7b40       94     "Just before blocking compacting GC"
    00000163942bf7c0 00007ff8a0be7b40      102     "After GC... Just before new allocation"
    00000163942bf828 00007ff8a0be7b40       64     "After allocation..."
    00000163942bf868 00007ff8414710a0       24     B
    00000163942bf880 00007ff841471270       32     D
    00000163942bf8a0 00007ff841471420       24     G
    00000163942bf8b8 000001639297fa70       24 Free					<- gen0 start
    00000163942bf8d0 00007ff8a0be7b40       28     "3"
    00000163942bf8f0 00007ff8414714d0       24     H
    00000163942c18d0 000001639297fa70    87174 Free
    00000163942d6d58 00007ff841470f40       24     E
    */
    class SOHCompactionWithPinning_AllocPtr : ICollectScenario
    {
        public unsafe int Run()
        {
            // Cleanup everything
            if (true) GC.Collect(0, GCCollectionMode.Forced, blocking: true, compacting: true);

            Console.WriteLine("Creating objects layout");
            list.Add(new A());
            list.Add(new B());
            list.Add(new C());
            list.Add(new D());
            var e = new E();
            list.Add(e);
            list.Add(new F());
            list.Add(new G());
            fixed (int* p = &e.F1)
            {
                list[0] = null;
                list[2] = null;
                list[5] = null;
                Console.WriteLine("Just before blocking compacting GC");
                Console.ReadLine();
                GC.Collect(0, GCCollectionMode.Forced, blocking: true, compacting: true);
                Console.WriteLine("After GC... Just before new allocation");
                Console.ReadLine();
                list.Add(new H());
                Console.WriteLine("After allocation...");
                Console.ReadLine();
            }
            return list.Count;
        }

        private List<object> list = new List<object>(10);
    }

    public class A
    {
        public int F1 = 101;
    }

    public class B
    {
        public int F1 = 201;
    }

    public class C
    {
        public int F1 = 301;
        public int F2 = 302;
        public int F3 = 303;
        public int F4 = 304;
        public int F5 = 305;
    }

    public class D
    {
        public int F1 = 401;
        public int F2 = 402;
        public int F3 = 403;
        public int F4 = 404;
    }

    public class E /* pinned */
    {
        public int F1 = 501;
    }

    public class F
    {
        public int F1 = 601;
        public int F2 = 602;
        public int F3 = 603;
        public int F4 = 604;
        public int F5 = 605;
    }

    public class G
    {
        public int F1 = 701;
        public int F2 = 702;
    }

    public class H
    {
        public int F1 = 801;
    }
}
