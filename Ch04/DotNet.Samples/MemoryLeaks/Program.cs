using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using MemoryLeaks.Leaks;

namespace MemoryLeaks
{
    class Program
    {
        static void Main(string[] args)
        {
            var mainAssembly = Assembly.GetExecutingAssembly();
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(mainAssembly)
                .AssignableTo<IMemoryLeakExample>()
                .AsSelf();

            var container = builder.Build();
            var leaks = container.ComponentRegistry
                                 .Registrations
                                 .Where(r => typeof(IMemoryLeakExample).IsAssignableFrom(r.Activator.LimitType))
                                 .Select(r => r.Activator.LimitType)
                                 .ToArray();

            Console.WriteLine("Please select memory leak to run...");
            for (int i = 0; i < leaks.Count(); ++i)
            {
                Console.WriteLine($"{i} - {leaks[i].Name}");
            }
            var key = Console.ReadKey();
            var leak = container.Resolve(leaks[int.Parse(key.KeyChar.ToString())]) as IMemoryLeakExample;
            leak.Run();
            Console.ReadKey();
        }
    }
}
