using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JitLeak
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
            Consumer c = new Consumer();
            c.CallAll();
            Console.ReadLine();
        }
    }
}
