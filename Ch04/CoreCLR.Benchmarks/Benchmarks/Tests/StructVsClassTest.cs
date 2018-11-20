using BenchmarkDotNet.Attributes.Jobs;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Benchmarks.Tests
{
    /*
 BenchmarkDotNet=v0.10.14, OS=Windows 10.0.16299.431 (1709/FallCreatorsUpdate/Redstone3)
Intel Core i7-4770K CPU 3.50GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
Frequency=3410078 Hz, Resolution=293.2484 ns, Timer=TSC
.NET Core SDK=2.1.300
  [Host] : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT
  Core   : .NET Core 2.1.0 (CoreCLR 4.6.26515.07, CoreFX 4.6.26515.06), 64bit RyuJIT

Job=Core  Runtime=Core

                                          Method | Amount |     Mean |     Error |    StdDev |   Gen 0 | Allocated |
------------------------------------------------ |------- |---------:|----------:|----------:|--------:|----------:|
           PeopleEmployeedWithinLocation_Classes |   1000 | 348.8 us | 2.9698 us | 2.7779 us | 15.1367 |   62.6 KB |
           PeopleEmployeedWithinLocation_Structs |   1000 | 344.7 us | 0.8458 us | 0.7912 us |  9.2773 |  39.13 KB |
  PeopleEmployeedWithinLocation_ArrayPoolStructs |   1000 | 343.4 us | 0.6593 us | 0.5506 us |       - |   1.35 KB |
     */

    [CoreJob]
    [MemoryDiagnoser]
    //[DisassemblyDiagnoser(printAsm: true, printSource: true, printIL: true)]
    //[HardwareCounters(HardwareCounter.CacheMisses, HardwareCounter.LlcMisses)]
    public class StructVsClassTest
    {
        [Params(1000)]
        public int Amount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            // Warm up pool
            var array = ArrayPool<InputDataStruct>.Shared.Rent(Amount);
            ArrayPool<InputDataStruct>.Shared.Return(array);
        }

        [Benchmark]
        public List<string> PeopleEmployeedWithinLocation_Classes()
        {
            int amount = Amount;
            LocationClass location = new LocationClass();
            List<string> result = new List<string>();
            List<PersonDataClass> input = service.GetPersonsInBatchClasses(amount);
            DateTime now = DateTime.Now;
            for (int i = 0; i < input.Count; ++i)
            {
                PersonDataClass item = input[i];
                if (now.Subtract(item.BirthDate).TotalDays > 18 * 365)
                {
                    var employee = service.GetEmployeeClass(item.EmployeeId);
                    if (locationService.DistanceWithClass(location, employee.Address) < 10.0)
                    {
                        string name = string.Format("{0} {1}", item.Firstname, item.Lastname);
                        result.Add(name);
                    }
                }
            }
            return result;
        }

        [Benchmark]
        public List<string> PeopleEmployeedWithinLocation_Structs()
        {
            int amount = Amount;
            LocationStruct location = new LocationStruct();
            List<string> result = new List<string>();
            InputDataStruct[] input = service.GetPersonsInBatchStructs(amount);
            DateTime now = DateTime.Now;
            for (int i = 0; i < input.Length; ++i)
            {
                ref InputDataStruct item = ref input[i];
                if (now.Subtract(item.BirthDate).TotalDays > 18 * 365)
                {
                    var employee = service.GetEmployeeStruct(item.EmployeeId);
                    if (locationService.DistanceWithStruct(ref location, employee.Address) < 10.0)
                    {
                        string name = string.Format("{0} {1}", item.Firstname, item.Lastname);
                        result.Add(name);
                    }
                }
            }
            return result;
        }

        [Benchmark]
        public IEnumerable<string> PeopleEmployeedWithinLocation_ArrayPoolStructs()
        {
            int amount = Amount;
            LocationStruct location = new LocationStruct();
            List<string> result = new List<string>();
            InputDataStruct[] input = service.GetDataArrayPoolStructs(amount);
            DateTime now = DateTime.Now;
            for (int i = 0; i < input.Length; ++i)
            {
                ref InputDataStruct item = ref input[i];
                if (now.Subtract(item.BirthDate).TotalDays > 18 * 365)
                {
                    var employee = service.GetEmployeeStruct(item.EmployeeId);
                    if (locationService.DistanceWithStruct(ref location, employee.Address) < 10.0)
                    {
                        string name = string.Format("{0} {1}", item.Firstname, item.Lastname);
                        result.Add(name);
                    }
                }
            }
            ArrayPool<InputDataStruct>.Shared.Return(input);
            return result;
        }

        public class PersonDataClass
        {
            public string Firstname;
            public string Lastname;
            public DateTime BirthDate;
            public Guid EmployeeId;
        }

        public struct InputDataStruct
        {
            public string Firstname;
            public string Lastname;
            public DateTime BirthDate;
            public Guid EmployeeId;
        }

        public class EmployeeClass
        {
            public string Name;
            public string Address;
        }
        public struct EmployeeStruct
        {
            public string Name;
            public string Address;
        }
        public class LocationClass
        {
            public double Lat;
            public double Long;
        }

        public struct LocationStruct
        {
            public double Lat;
            public double Long;
        }

        private LocationService locationService = new LocationService();
        private SomeService service = new SomeService();

        public class LocationService
        {
            internal double DistanceWithClass(LocationClass location, string address)
            {
                return 1.0;
            }

            internal double DistanceWithStruct(ref LocationStruct location, string address)
            {
                return 1.0;
            }
        }
        public class SomeService
        {
            internal List<PersonDataClass> GetPersonsInBatchClasses(int amount)
            {
                List<PersonDataClass> result = new List<PersonDataClass>(amount);
                for (int i = 0; i < amount; ++i)
                {
                    result.Add(new PersonDataClass()
                    {
                        Firstname = "A",
                        Lastname = "B",
                        BirthDate = DateTime.Now.AddYears(20),
                        EmployeeId = Guid.NewGuid()
                    });
                }
                return result;
            }

            internal InputDataStruct[] GetPersonsInBatchStructs(int amount)
            {
                InputDataStruct[] result = new InputDataStruct[amount];
                for (int i = 0; i < amount; ++i)
                {
                    result[i] = new InputDataStruct()
                    {
                        Firstname = "A",
                        Lastname = "B",
                        BirthDate = DateTime.Now.AddYears(20),
                        EmployeeId = Guid.NewGuid()
                    };
                }
                return result;
            }

            internal InputDataStruct[] GetDataArrayPoolStructs(int amount)
            {
                InputDataStruct[] result = ArrayPool<InputDataStruct>.Shared.Rent(amount);
                for (int i = 0; i < amount; ++i)
                {
                    result[i] = new InputDataStruct()
                    {
                        Firstname = "A",
                        Lastname = "B",
                        BirthDate = DateTime.Now.AddYears(20),
                        EmployeeId = Guid.NewGuid()
                    };
                }
                return result;
            }

            internal EmployeeClass GetEmployeeClass(Guid employeeId)
            {
                return new EmployeeClass() { Address = "X", Name = "Y" };
            }

            internal EmployeeStruct GetEmployeeStruct(Guid employeeId)
            {
                return new EmployeeStruct() { Address = "X", Name = "Y" };
            }
        }
    }
}
