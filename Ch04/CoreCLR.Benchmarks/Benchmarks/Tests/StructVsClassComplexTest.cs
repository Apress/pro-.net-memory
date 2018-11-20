using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Diagnosers;

namespace Benchmarks.Tests
{
    /*
                                       Method | Amount |     Mean |    Error |   StdDev |   Gen 0 | Allocated |
    ----------------------------------------- |------- |---------:|---------:|---------:|--------:|----------:|
              ComplexProcessingBasedOnObjects |   1000 | 376.5 us | 1.842 us | 1.633 us | 15.1367 |  63.13 KB |
              ComplexProcessingBasedOnStructs |   1000 | 364.2 us | 3.192 us | 2.986 us |  9.2773 |  39.13 KB |
     ComplexProcessingBasedOnArrayPoolStructs |   1000 | 362.6 us | 2.014 us | 1.786 us |       - |   1.35 KB |
    */

    [CoreJob]
    [MemoryDiagnoser]
    //[DisassemblyDiagnoser(printAsm: true, printSource: true, printIL: true)]
    //[HardwareCounters(HardwareCounter.CacheMisses, HardwareCounter.LlcMisses)]
    public class StructVsClassComplexTest
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
        public List<string> ComplexProcessingBasedOnObjects()
        {
            int amount = Amount;
            LocationClass location = new LocationClass();
            DateTime now = DateTime.Now;
            List<InputDataClass> input = service.GetDataBatch(amount);
            var adults = input.Where(x => now.Subtract(x.BirthDate).TotalDays > 18 * 365)
                              .Select(x => new
                              {
                                  Name = string.Format("{0} {1}", x.Firstname, x.Lastname),
                                  Employee = service.GetEmployee(x.EmployeeId)
                              });
            var nearBy = adults.Where(x => locationService.DistanceWithClass(location, x.Employee.Address) < 10.0)
                               .Select(x => x.Name)
                               .ToList();
            return nearBy;
        }

        [Benchmark]
        public List<string> ComplexProcessingBasedOnStructs()
        {
            int amount = Amount;
            LocationStruct location = new LocationStruct();
            List<string> result = new List<string>();
            InputDataStruct[] input = service.GetDataBatchStructs(amount);
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
        public IEnumerable<string> ComplexProcessingBasedOnArrayPoolStructs()
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

        public class InputDataClass
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
            internal List<InputDataClass> GetDataBatch(int amount)
            {
                List<InputDataClass> result = new List<InputDataClass>(amount);
                for (int i = 0; i < amount; ++i)
                {
                    result.Add(new InputDataClass()
                    {
                        Firstname = "A",
                        Lastname = "B",
                        BirthDate = DateTime.Now.AddYears(20),
                        EmployeeId = Guid.NewGuid()
                    });
                }
                return result;
            }

            internal InputDataStruct[] GetDataBatchStructs(int amount)
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

            internal EmployeeClass GetEmployee(Guid employeeId)
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
