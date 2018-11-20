using System;
using System.Collections.Generic;
using CoreCLR.CollectScenarios.Scenarios;

namespace CoreCLR.CollectScenarios
{
    class Program
    {
        // To WinDbg CoreCLR:
        // F:\GithubProjects\coreclr\bin\Product\Windows_NT.x64.Debug\CoreRun.exe
        // f:\GithubProjects\NetMemoryBook\Projects\CoreCLR.General\CoreCLR.CollectScenarios\bin\x64\Release\netcoreapp2.0\CoreCLR.CollectScenarios.dll
        // f:\GithubProjects\NetMemoryBook\Projects\CoreCLR.General\CoreCLR.CollectScenarios\

        static void Main(string[] args)
        {
            List<Type> scenarios = new List<Type>()
            {
                typeof(PinneableObjectCacheUsage),
                typeof(EagerRootCollection),
                typeof(VariousRoots),
                typeof(VariousPinning),
            };

            Console.WriteLine("Select scenario: ");
            for (int i = 0; i < scenarios.Count; ++i)
                Console.WriteLine("{0} - {1}", i, scenarios[i].Name);
            int selection = int.Parse(Console.ReadLine());

            ICollectScenario instance = (ICollectScenario) Activator.CreateInstance(scenarios[selection]);
            instance.Run();
        }
    }
}
