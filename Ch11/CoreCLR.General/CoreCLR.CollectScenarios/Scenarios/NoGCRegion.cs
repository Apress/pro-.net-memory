using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;

namespace CoreCLR.CollectScenarios.Scenarios
{
    class NoGCRegion : ICollectScenario
    {
        public int Run()
        {
            long mbs = long.Parse(Console.ReadLine());
            //bool success = GC.TryStartNoGCRegion(3916L * 1024 * 1024);
            //Console.WriteLine(success);
            while (true)
            {
                try
                {
                    if (GC.TryStartNoGCRegion(mbs * 1024 * 1024, 0, false))
                        break;
                }
                catch (ArgumentOutOfRangeException )
                {
                    mbs -= 1;
                }
            }
            Console.Write(mbs);
            GC.EndNoGCRegion();
            Console.ReadLine();
            return 0;
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 11-7
        public void ExampleNoGCRegionCreation()
        {
            // in case of previous finally block not executed
            if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
                GC.EndNoGCRegion();
            if (GC.TryStartNoGCRegion(1024, true))
            {
                try
                {
                    // Do some work. 
                }
                finally
                {
                    try
                    {
                        GC.EndNoGCRegion();
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Log message
                    }
                }
            }

        }
    }
}
