using System;
using System.Collections.Generic;
using CoreCLR.CollectScenarios.Scenarios;

namespace CoreCLR.CollectScenarios
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Type> scenarios = new List<Type>()
            {
                typeof(NoGCRegion)
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
