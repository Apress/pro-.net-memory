using System;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = "Presy any key to exit...";
            string s2 = string.IsInterned(s);
            Console.ReadLine();

            var domain = AppDomain.CreateDomain("Test");
            //var str = domain.CreateInstanceAndUnwrap((typeof(string).Assembly.FullName, typeof(string).FullName new object[] { '!', 3 });

            //Consumer c = new Consumer();
            //c.CallAll();
            //s2 = string.IsInterned(s2);
            Console.WriteLine(s2);
            Console.ReadLine();
        }
    }
}
