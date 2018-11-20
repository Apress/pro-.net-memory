using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;

/*
BenchmarkDotNet=v0.10.14, OS=Windows 10.0.16299.431 (1709/FallCreatorsUpdate/Redstone3)
Intel Core i7-4770K CPU 3.50GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
Frequency=3410076 Hz, Resolution=293.2486 ns, Timer=TSC
.NET Core SDK=2.1.300
  [Host]        : .NET Core 2.0.7 (CoreCLR 4.6.26328.01, CoreFX 4.6.26403.03), 64bit RyuJIT
  .NET 4.7.1    : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2650.0
  .NET Core 2.1 : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT


      Method |           Job |     Toolchain |     Mean |     Error |    StdDev | Allocated |
------------ |-------------- |-------------- |---------:|----------:|----------:|----------:|
  SpanAccess |    .NET 4.7.1 |  CsProjnet471 | 90.35 ns | 0.1085 ns | 0.0847 ns |       0 B |
 ArrayAccess |    .NET 4.7.1 |  CsProjnet471 | 66.86 ns | 0.7334 ns | 0.6860 ns |       0 B |
  SpanAccess | .NET Core 2.1 | .NET Core 2.1 | 61.81 ns | 0.7035 ns | 0.6581 ns |       0 B |
 ArrayAccess | .NET Core 2.1 | .NET Core 2.1 | 66.18 ns | 0.0603 ns | 0.0564 ns |       0 B |
 */

namespace Benchmarks.Tests
{
    [Config(typeof(MultipleRuntimesConfig))]
    public class Span
    {
        private byte[] array;

        [GlobalSetup]
        public void Setup()
        {
            array = new byte[128];
            for (int i = 0; i < 128; ++i)
                array[i] = (byte)i;
        }

        [Benchmark]
        public int SpanAccess()
        {
            var span = new Span<byte>(this.array);
            int result = 0;
            for (int i = 0; i < 128; ++i)
            {
                result += span[i];
            }
            return result;
        }

        [Benchmark]
        public int ArrayAccess()
        {
            int result = 0;
            for (int i = 0; i < 128; ++i)
            {
                result += this.array[i];
            }
            return result;
        }
    }

    public class MultipleRuntimesConfig : ManualConfig
    {
        public MultipleRuntimesConfig()
        {
            Add(Job.Default
                .With(CsProjClassicNetToolchain.Net471) // Span NOT supported by Runtime
                .WithId(".NET 4.7.1"));

            //Add(Job.Default
            //    .With(CsProjCoreToolchain.NetCoreApp11) // Span NOT supported by Runtime
            //    .WithId(".NET Core 1.1"));

            /// !!! warning !!! NetCoreApp20 toolchain simply sets TargetFramework = netcoreapp2.0 in generated .csproj
            /// // so you need Visual Studio 2017 Preview 15.3 to be able to run it!
            //Add(Job.Default
            //    .With(CsProjCoreToolchain.NetCoreApp20) // Span SUPPORTED by Runtime
            //    .WithId(".NET Core 2.0"));

            Add(Job.Default
                .With(CsProjCoreToolchain.NetCoreApp21) // Span SUPPORTED by Runtime
                .WithId(".NET Core 2.1"));
            Add(DisassemblyDiagnoser.Create(new DisassemblyDiagnoserConfig(printAsm: true, printIL: true,
                printSource: true)));
            Add(MemoryDiagnoser.Default);

        }
    }
}
