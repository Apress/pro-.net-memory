using Microsoft.IO;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CoreCLR.HiddenAllocations
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(DateTime.Now);
            int counter = 0;
            var p = new Program();
            p.Setup();

            Console.Write(p.ProcessWithLogging());
            Console.ReadLine();
            Console.Write(p.ProcessWithLogging());

            List<SomeClass> list = new List<SomeClass>() { new SomeClass() };

            // Structs
            var l1 = p.PeopleEmployeedWithinLocation_Classes(100, new LocationClass());
            var l2 = p.PeopleEmployeedWithinLocation_Structs(100, new LocationStruct());
            Console.WriteLine(l1.ToString() + l2.ToString());

            // Listing 6-12
            // Tuples
            var tuple1 = new Tuple<int, double>(0, 0.0);
            var tuple2 = Tuple.Create(0, 0.0);
            // tuple1.Item1 = 3; not possible
            var tuple3 = new {A = 1, B = 0.0};

            // Listing 6-13
            var tuple4 = (0, 0.0);
            tuple4.Item1 = 3; // possible
            var tuple5 = (A: 0, B: 0.0);
            tuple5.A = 3;
            p.FooBar(tuple1); p.FooBar(tuple4); // boxing dla tuple4!
            p.SomeBar(3);

            // Listing 6-14
            var result1 = p.ProcessDataReturningTupleWithObjects(list);
            var result2 = p.ProcessDataReturnigValueTupleWithStructs(list);
            (ResultDescStruct desc, _) = p.ProcessDataReturnigValueTupleWithStructs(list);

            // Listing 6-17
            int minLength = 10;
            var pool = ArrayPool<int>.Shared;
            int[] buffer = pool.Rent(minLength);
            try
            {
                Consume(buffer);
            }
            finally
            {
                pool.Return(buffer);
            }

            // RecyclableMemoryStream
            //while (true)
            {
                var data = new SomeDataClass() { X = 1, Y = 2 };
                var str = p.SerializeXmlWithMemoryStream(data);
                var str2 = p.SerializeXmlWithRecyclableMemoryStream(data);
                Console.WriteLine(str + Environment.NewLine + str2);
            }

            // Delegates
            p.Delegates(new SomeStruct());

            // Closures
            int value = 4;
            p.Closures(value);

            // Listings 6-53, 6-55, 6-57, 6-59
            // LINQ
            var range = Enumerable.Range(0, 100); // allocates System.Linq.Enumerable/'<RangeIterator>d__111' 
            var linq0b = list.Where(x => x.X > 0);
            var linq0a = list.FirstOrDefault(GreaterHelper);
            var linq1 = list.Select(x => new {S = x.X + x.Y });
            var linq2 = list.Select(x => ValueTuple.Create(x.X + x.Y));
            var linq3 = from x in list
                let s = x.X + x.Y
                select s;
            var linq4 = p.FilterStrings_WithLINQ();
            var linq5 = p.FilterStrings_WithoutLINQ();
            Console.WriteLine(linq4.Count() + linq5.Count());


            // Params
            p.MethodWithParams("Counter {0}", counter); // new object[] { counter }
            p.MethodWithParams("Hello!"); // uses static EmptyArray<object>.Value


            // Summary
            var arg1 = new int[] { 1, 4, 7, 9 };
            int[] intData = new[] { 123, 32, 4 };
            int[] results = p.SummaryOnArrayToArray(arg1, 2, 8, 2);
            var results2 = p.SummaryOnArrayToList(intData, 31).TrueForAll(x => x > 0);
            var results3 = p.SummaryOnArrayToListGeneric(intData, 31).TrueForAll(x => x > 0);
            var results4 = p.SummaryOnArrayToEnumerable(intData, 31).ToList().TrueForAll(x => x > 0);
            Console.WriteLine(results.ToString() + results2.ToString() + results3.ToString() + results4.ToString());

            // Async
            Console.WriteLine(p.ReadFileAsync("TextFile2.txt").Result);
            Console.WriteLine(p.ReadFileAsyncValue("TextFile2.txt").Result);
            Console.WriteLine(p.ConsumeValueTask_RarelyNeeded("TextFile2.txt").Result);
            Console.WriteLine(p.ConsumeValueTask_Good("TextFile2.txt").Result);
            // ArrayPool
            Console.WriteLine(p.UseArrayPool(100));
            var arrayPool = ArrayPool<int>.Shared.Rent(2 * 1024 * 1024);
            Console.WriteLine(arrayPool[0]);
            ArrayPool<int>.Shared.Return(arrayPool);

            Console.ReadLine();
        }

        private static void Consume(int[] buffer)
        {
        }

        string[] FilterStrings(string[] inputs, int min, int max, int charIndex)
        {
            var results = inputs.Where(x => x.Length >= min && x.Length <= max)
                .Select(x => x.ToLower());
            return results.ToArray();
        }

        string[] strings =
        {
            "A", "penny", "saved", "is", "a", "penny", "earned.", "The", "early", "bird", "catches", "the", "worm.",
            "The", "pen", "is", "mightier", "than", "the", "sword."
        };
        int min = 3;
        int max = 5;

        [Serializable]
        public struct TestStruct
        {
            public int Value1 { get; set; }
            public int Value2 { get; set; }
            public string Value3 { get; set; }
            public double Value4 { get; set; }
        }

        ///////////////////////////////////////////////////////////////////////
        /// Listing 6-10
        public List<string> PeopleEmployeedWithinLocation_Classes(int amount, LocationClass location)
        {
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
        internal List<PersonDataClass> GetPersonsInBatchClasses(int amount)
        {
            List<PersonDataClass> result = new List<PersonDataClass>(amount);
            // Populate list from external source
            return result;
        }

        ///////////////////////////////////////////////////////////////////////
        /// Listing 6-11
        public List<string> PeopleEmployeedWithinLocation_Structs(int amount, LocationStruct location)
        {
            List<string> result = new List<string>();
            PersonDataStruct[] input = service.GetPersonsInBatchStructs(amount);
            DateTime now = DateTime.Now;
            for (int i = 0; i < input.Length; ++i)
            {
                ref PersonDataStruct item = ref input[i];
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


        public class PersonDataClass
        {
            public string Firstname;
            public string Lastname;
            public DateTime BirthDate;
            public Guid EmployeeId;
        }

        public struct PersonDataStruct
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
                        EmployeeId =  Guid.NewGuid()
                    });
                }
                return result;
            }

            internal PersonDataStruct[] GetPersonsInBatchStructs(int amount)
            {
                PersonDataStruct[] result = new PersonDataStruct[amount];
                for (int i = 0; i < amount; ++i)
                {
                    result[i] = new PersonDataStruct()
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

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-60
        public IEnumerable<string> FilterStrings_WithLINQ()
        {
            var results = strings.Where(x => x.Length >= min && x.Length <= max)
                .Select(x => x.ToLower());
            return results;
        }

        public IEnumerable<string> FilterStrings_WithoutLINQ()
        {
            //IEnumerable<string> Helper()
            //{
            foreach (var word in strings)
            {
                if (word.Length >= min && word.Length <= max)
                    yield return word.ToLower();
            }
            // }
            //return Helper().ToArray();
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-23
        public async Task<string> ReadFileAsync(string filename)
        {
            // Allocates 
            /*
            [AsyncStateMachine(typeof(Program.<ReadFileAsync>d__4))]
            public Task<string> ReadFileAsync(string filename)
            {
	            Program.<ReadFileAsync>d__4 <ReadFileAsync>d__;
	            <ReadFileAsync>d__.filename = filename;
	            <ReadFileAsync>d__.<>t__builder = AsyncTaskMethodBuilder<string>.Create();
	            <ReadFileAsync>d__.<>1__state = -1;
	            AsyncTaskMethodBuilder<string> <>t__builder = <ReadFileAsync>d__.<>t__builder;
	            <>t__builder.Start<Program.<ReadFileAsync>d__4>(ref <ReadFileAsync>d__);
	            return <ReadFileAsync>d__.<>t__builder.Task;
            }
            */
            if (!File.Exists(filename))
                return string.Empty;
            return await File.ReadAllTextAsync(filename);
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-28
        public async ValueTask<string> ReadFileAsyncValue(string filename)
        {
            if (!File.Exists(filename))
                return string.Empty;
            return await File.ReadAllTextAsync(filename);
        }

        private async Task<string> ConsumeValueTask_Good(string filename)
        {
            return await ReadFileAsyncValue(filename);
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-29
        private async Task<string> ConsumeValueTask_RarelyNeeded(string filename)
        {
            var valueTask = ReadFileAsyncValue(filename);
            if (valueTask.IsCompleted)
                return valueTask.Result;
            else
                return await valueTask.AsTask();
        }

        public int UseArrayPool(int size)
        {
            var pool = ArrayPool<int>.Shared;
            //var pool2 = ArrayPool<SomeStruct>.Create(1000, 10);
            int[] buffer = pool.Rent(size);
            try
            {
                return ConsumeArray(buffer);
            }
            finally
            {
                pool.Return(buffer);
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-21
        public static readonly XmlWriterSettings XmlWriterSettings = new XmlWriterSettings();
        public string SerializeXmlWithMemoryStream(object obj)
        {
            using (var ms = new MemoryStream())
            {
                using (var xw = XmlWriter.Create(ms, XmlWriterSettings))
                {
                    var serializer = new DataContractSerializer(obj.GetType()); // could be cached!
                    serializer.WriteObject(xw, obj);
                    xw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    var reader = new StreamReader(ms);
                    return reader.ReadToEnd();
                }
            }
        }

        private static readonly Dictionary<Type, DataContractSerializer> serializers = new Dictionary<Type, DataContractSerializer>()
        {
            { typeof(SomeDataClass), new DataContractSerializer(typeof(SomeDataClass)) }
        };

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-22

        // Install-Package Microsoft.IO.RecyclableMemoryStream
        static RecyclableMemoryStreamManager manager = new RecyclableMemoryStreamManager(blockSize: 128 * 1024,
                                                                                         largeBufferMultiple: 1024 * 1024,
                                                                                         maximumBufferSize: 128 * 1024 * 1024);
        public string SerializeXmlWithRecyclableMemoryStream<T>(T obj)
        {
            using (var ms = manager.GetStream())
            {
                using (var xw = XmlWriter.Create(ms, XmlWriterSettings))
                {
                    var serializer = serializers[obj.GetType()]; 
                    serializer.WriteObject(xw, obj);
                    xw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    var reader = new StreamReader(ms);
                    return reader.ReadToEnd();
                }
            }
        }

        private int ConsumeArray(int[] buffer)
        {
            return buffer.Length;
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-39
        private IEnumerable<string> Closures(int value)
        {
            var filteredList = _list.Where(x => x > value);
            var result = filteredList.Select(x => x.ToString());
            return result;
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-42
        private IEnumerable<string> WithoutClosures2(int value)
        {
            List<string> result = new List<string>();
            foreach (int x in _list)
                if (x > value)
                    result.Add(x.ToString());
            return result;
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-43
        private IEnumerable<string> ClosuresWithLocalFunction(int value)
        {
            bool WhereCondition(int x) => x > value;
            string SelectAction(int x) => x.ToString();

            var filteredList = _list.Where(WhereCondition);
            var result = filteredList.Select(SelectAction);
            return result;
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-45
        private IEnumerable<string> WithoutClosures(int value)
        {
            foreach (int x in _list)
                if (x > value)
                    yield return x.ToString();
        }



        private IEnumerable<string> ClosuresComplex(int value)
        {
            var filteredList = _list.Where(x => x > value);
            var result = _list.Select(x => filteredList.Contains(x) ? x.ToString() : " ");
            return result;
        }

        public static bool GreaterHelper(SomeClass sc) => sc.X > 0;

        ///////////////////////////////////////////////////////////////////////
        /// Listing 6-14
        public Tuple<object, object> ProcessDataReturningTupleWithObjects(IEnumerable<SomeClass> data)
        {
            return new Tuple<object, object>(new ResultDesc(), new ResultData());
        }
        public (ResultDescStruct, ResultDataStruct) ProcessDataReturnigValueTupleWithStructs(IEnumerable<SomeClass> data)
        {
            // This will capture data
            void Tally(SomeClass rd)
            {
                Console.WriteLine($"Sum of {rd.X} in {this._list.Count} and {data.Count()}");
            }
            var resultData = new ResultDataStruct();
            Tally(data.FirstOrDefault());
            return (new ResultDescStruct(), resultData);
        }

        void SomeBar<T>(T obj)
        {
            Console.WriteLine(obj.ToString());
        }

        void FooBar(ITuple tuple)
        {
            Console.WriteLine($"# of elements: {tuple.Length}");
            Console.WriteLine($"Second to last element: {tuple[tuple.Length - 2]}");
        }

        void FooBar<T>(T tuple) where T : ITuple
        {
            Console.WriteLine($"# of elements: {tuple.Length}");
            Console.WriteLine($"Second to last element: {tuple[tuple.Length - 2]}");
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-31, 6-32, 6-41

        double Delegates(SomeStruct ss)
        {
            Func<double> action = ProcessWithLogging; // allocate delegate
            var r1 = action();

            Func<double> action2 = ss.SomeMethod; // allocate delegate but before box ss to get function pointer
            var r2 = action2();

            Func<double> action3 = () => InstanceMethodNotUsingThis(); // allocate delegate (there is no closure because nothing to be captured, this is not captured)
            var r3 = action3();

            Func<double> action4 = () => InstanceMethodUsingThis(); // allocate delegate (there is no closure because nothing to be captured, this is not captured)
            var r4 = action4();

            Func<double> action5 = () => StaticMethod(); // allocate delegate (there is nothing to be captured)
            var r5 = action5();

            Func<double> action6 = () => StaticMethodUsingLocalVariable(ss); // allocate delegate (there is nothing to be captured)
            var r6 = action6();

            Func<double> action7 = () => InstanceMethodUsingLocalVariable(ss); // allocate delegate, closure captures ss and this (to call this.<>4__this.ProcessSomeStruct(this.ss); inside)
                                                                // obecnosc tego argumentu zmienia postac rzeczy. Gdyby go nie bylo, nie bylby capturowany this 
            var r7 = action7();

            return r1 + r2 + r3 + r4 + r5 + r6 + r7;
        }

        static double StaticDelegates(Program p)
        {
            Func<double> action1 = () => p.InstanceMethodUsingThis();
            var r1 = action1();
            return r1;
        }

        private List<int> _list;
        const int _items = 1;
        private Logger _logger = new Logger();

        ///////////////////////////////////////////////////////////////////////
        // Listing 6-47
        public void MethodWithParams(string str, params object[] args)
        {
            Console.WriteLine(str, args);
        }

        public void Setup()
        {
            _list = new List<int>();
            for (int i = 0; i < _items; i++)
            {
                _list.Add(i);
                
            }
            // Warm up StringBuilderCache
            Console.WriteLine(string.Format("Warming: {0}", _items.ToString(CultureInfo.InvariantCulture)));
            _logger.Level = Logger.LoggingLevel.Info;
        }

        public double InstanceMethodNotUsingThis()
        {
            return 0.0;
        }

        public double InstanceMethodUsingThis()
        {
            return _list.Count;
        }

        public double ProcessWithLogging()
        {
            double sum = 0.0;
            sum += ProcessListCalculatingSum(_list);
            _logger.Debug(() => string.Format("Result: {0}", (sum / _items).ToString(CultureInfo.InvariantCulture)));
            return sum;
        }

        public static double StaticMethod()
        {
            return 0.0;
        }

        public static double StaticMethodUsingLocalVariable(SomeStruct data)
        {
            return data.SomeMethod();
        }

        public double InstanceMethodUsingLocalVariable(SomeStruct data)
        {
            return data.SomeMethod();
        }

        public int ProcessListCalculatingSum(List<int> list)
        {
            var sum = 0;
            foreach (var x in list)
            {
                sum += x;
            }
            return sum;
        }

        int[] SummaryOnArrayToArray(int[] inputs, int lo, int hi, int c)
        {
            // Allocates 264B:
            //- 2xFunc<> - 2x64B
            //- closure's DisplayClass - 32B (caontains lo, hi, c)
            //- WhereArrayIterator - 48B
            //- WhereSelectArrayIterator - 56B
            var results = inputs.Where(x => x >= lo && x <= hi)
                .Select(x => x * c);
            return results.ToArray();
        }

        public List<int> SummaryOnArrayToList(int[] intData, int min)
        {
            // Allocates 136B: 
            //- WhereArrayIterator - 48B
            //- Func<> - 64B
            //- closure's DisplayClass - 24B (contains min)
            //- co z ToList? olewany w zależności od użycia dla JIT wystarczy po prostu iterator (nice!)
            var results = intData.Where(i => i > min).ToList();
            return results;
        }

        public List<T> SummaryOnArrayToListGeneric<T>(T[] intData, T min) where T : IComparable<T>
        {
            // Allocates 136B: 
            //- WhereArrayIterator - 48B
            //- Func<> - 64B
            //- closure's DisplayClass<T> - 24B (contains min)
            //- co z ToList? olewany w zależności od użycia dla JIT wystarczy po prostu iterator (nice!)
            var results = intData.Where(i => i.CompareTo(min) > 0).ToList();
            return results;
        }

        public IEnumerable<T> SummaryOnArrayToEnumerable<T>(T[] intData, T min) where T : IComparable<T>
        {
            // Allocates 64B:
            //- state machine <TestingGenericNoAlloc>d__4<T>
            //      [CompilerGenerated]
            //      private sealed class <TestingGenericNoAlloc>d__4<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable where T : IComparable<T>
            //{
            //	[DebuggerHidden]
            //      public <TestingGenericNoAlloc>d__4(int <>1__state)
            //      {
            //          this.<> 1__state = <> 1__state;
            //          this.<> l__initialThreadId = Environment.CurrentManagedThreadId;
            //      }

            //      [DebuggerHidden]
            //      void IDisposable.Dispose()
            //      {
            //      }

            //      bool IEnumerator.MoveNext()
            //      {
            //          int num = this.<> 1__state;
            //          if (num == 0)
            //          {
            //              this.<> 1__state = -1;
            //              this.<> 7__wrap1 = this.intData;
            //              this.<> 7__wrap2 = 0;
            //              goto IL_79;
            //          }
            //          if (num != 1)
            //          {
            //              return false;
            //          }
            //          this.<> 1__state = -1;
            //          IL_6B:
            //          this.<> 7__wrap2++;
            //          IL_79:
            //          if (this.<> 7__wrap2 >= this.<> 7__wrap1.Length)
            //		{
            //              this.<> 7__wrap1 = null;
            //              return false;
            //          }
            //          T t = this.<> 7__wrap1[this.<> 7__wrap2];
            //          if (t.CompareTo(this.min) > 0)
            //          {
            //              this.<> 2__current = t;
            //              this.<> 1__state = 1;
            //              return true;
            //          }
            //          goto IL_6B;
            //      }

            //      T IEnumerator<T>.Current
            //      {
            //          [DebuggerHidden]
            //          get
            //          {
            //              return this.<> 2__current;
            //          }
            //      }

            //      [DebuggerHidden]
            //      void IEnumerator.Reset()
            //      {
            //          throw new NotSupportedException();
            //      }

            //      object IEnumerator.Current
            //      {
            //          [DebuggerHidden]
            //          get
            //          {
            //              return this.<> 2__current;
            //          }
            //      }

            //      [DebuggerHidden]
            //      IEnumerator<T> IEnumerable<T>.GetEnumerator()
            //      {
            //          Program.< TestingGenericNoAlloc > d__4 < T > < TestingGenericNoAlloc > d__;
            //          if (this.<> 1__state == -2 && this.<> l__initialThreadId == Environment.CurrentManagedThreadId)
            //		{
            //              this.<> 1__state = 0;

            //                  < TestingGenericNoAlloc > d__ = this;
            //          }
            //		else
            //		{

            //                  < TestingGenericNoAlloc > d__ = new Program.< TestingGenericNoAlloc > d__4<T>(0);
            //          }

            //              < TestingGenericNoAlloc > d__.intData = this.<> 3__intData;

            //              < TestingGenericNoAlloc > d__.min = this.<> 3__min;
            //          return < TestingGenericNoAlloc > d__;
            //      }

            //      [DebuggerHidden]
            //      IEnumerator IEnumerable.GetEnumerator()
            //      {
            //          return this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();
            //      }

            //      private int <>1__state;
            //	private T<>2__current;
            //	private int <>l__initialThreadId;
            //	private T[] intData;

            //      public T[] <>3__intData;

            //	private T min;

            //      public T<>3__min;

            //	private T[] <>7__wrap1;

            //	private int <>7__wrap2;
            //}
            foreach (var item in intData)
            {
                if (item.CompareTo(min) > 0)
                    yield return item;
            }
        }


    }

    public class Logger
    {
        public enum LoggingLevel
        {
            Debug,
            Info,
            Warning
        }

        public LoggingLevel Level { get; set; }

        private void Log(LoggingLevel level, Func<string> exceptionMessage)
        {
            if (level >= Level)
            {
                Console.WriteLine(exceptionMessage());
            }
        }

        public void Debug(Func<string> exceptionMessage) => Log(LoggingLevel.Debug, exceptionMessage);
        public void Info(Func<string> exceptionMessage) => Log(LoggingLevel.Info, exceptionMessage);
        public void Warning(Func<string> exceptionMessage) => Log(LoggingLevel.Warning, exceptionMessage);
    }

    public class SomeClass
    {
        public int X;
        public int Y;
    }

    public struct SomeStruct
    {
        public double SomeMethod()
        {
            return 0.0;
        }
    }

    public class ResultDesc : IResultDesc
    {
        public int Count;
    }

    public class ResultData : IResultData
    {
        public double Sum;
        public double Average;
    }

    public interface IResultDesc
    {
        
    }
    public interface IResultData
    {
        
    }

    public struct ResultDescStruct
    {
        public int Count;
    }

    public struct ResultDataStruct
    {
        public double Sum;
        public double Average;
    }

    public class SomeDataClass
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
