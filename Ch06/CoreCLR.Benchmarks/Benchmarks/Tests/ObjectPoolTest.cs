using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;

namespace Benchmarks.Tests
{
    [CoreJob]
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(printAsm: true, printSource: true, printIL: true)]
    //[Config(typeof(MonitoringConfig))]
    public class ObjectPoolTest
    {
        private ObjectPool_Trivial<SampleClass> objectPoolTrivial = new ObjectPool_Trivial<SampleClass>(() => new SampleClass());
        private ObjectPool_Naive<SampleClass> objectPoolNaive = new ObjectPool_Naive<SampleClass>(() => new SampleClass(), 10);
        private ObjectPool<SampleClass> objectPool = new ObjectPool<SampleClass>(() => new SampleClass(), 10);
        private string Text = "Text";

        [Params(100_000_000)]
        public int N;

        [Benchmark(OperationsPerInvoke = 16)]
        public void WithoutObjectPool()
        {
            var obj = new SampleClass();
            Consume(obj);
            objectPoolTrivial.Return(obj);
        }

        [Benchmark(OperationsPerInvoke = 16)]
        public void ObjectPoolTrivial_Singlethreaded()
        {
            var obj = objectPoolTrivial.Rent();
            Consume(obj);
            objectPoolTrivial.Return(obj);
        }

        [Benchmark(OperationsPerInvoke = 16)]
        public void ObjectPoolNaive_Singlethreaded()
        {
            var obj = objectPoolNaive.Rent();
            Consume(obj);
            objectPoolNaive.Return(obj);
        }

        [Benchmark(OperationsPerInvoke = 16)]
        public void ObjectPool_Singlethreaded()
        {
            var obj = objectPool.Rent();
            Consume(obj);
            objectPool.Return(obj);
        }

        private void Consume(SampleClass obj)
        {
            GC.KeepAlive(obj);
        }
    }

    public class MonitoringConfig : ManualConfig
    {
        public MonitoringConfig()
        {
            //Add(Job.Default.With(RunStrategy.Monitoring).With(Runtime.Clr).With(Jit.RyuJit).With(Platform.X64).WithId("NET4.7_RyuJIT-x64"));
            Add(Job.Default.With(RunStrategy.Monitoring).With(Runtime.Core).With(CsProjCoreToolchain.NetCoreApp20).WithId("Core2.0-x64"));
            //Add(RPlotExporter.Default);
            Add(MemoryDiagnoser.Default);
            KeepBenchmarkFiles = true;
        }
    }

    public class ObjectPool_Trivial<T>
    {
        private ConcurrentBag<T> items;
        private Func<T> generator;

        public ObjectPool_Trivial(Func<T> generator)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            this.items = new ConcurrentBag<T>();
            this.generator = generator;
        }

        public T Rent()
        {
            T item;
            if (items.TryTake(out item)) return item;
            return generator();
        }

        public void Return(T item)
        {
            items.Add(item);
        }
    }

    public class ObjectPool_Naive<T>
    {
        private readonly ConcurrentBag<T> items;
        private readonly Func<T> generator;
        private int counter = 0;
        private int size;

        public ObjectPool_Naive(Func<T> generator, int size)
        {
            if (generator == null) throw new ArgumentNullException("generator");
            this.items = new ConcurrentBag<T>();
            this.generator = generator;
            this.size = size;
        }

        public T Rent()
        {
            T item;
            if (items.TryTake(out item))
            {
                counter--;
                return item;
            }
            else
            {
                T obj = generator();
                items.Add(obj);
                counter++;
                return obj;
            }
        }

        public void Return(T item)
        {
            if (counter < size)
            {
                items.Add(item);
                counter++;
            }
        }
    }

    public class ObjectPool<T> where T : class
    {
        private T firstItem;
        private readonly T[] items;
        private readonly Func<T> generator;

        public ObjectPool(Func<T> generator, int size)
        {
            this.generator = generator ?? throw new ArgumentNullException("generator");
            this.items = new T[size - 1];
        }

        public T Rent()
        {
            // PERF: Examine the first element. If that fails, RentSlow will look at the remaining elements.
            // Note that the initial read is optimistically not synchronized. That is intentional. 
            // We will interlock only when we have a candidate. in a worst case we may miss some
            // recently returned objects. Not a big deal.
            T inst = firstItem;
            if (inst == null || inst != Interlocked.CompareExchange(ref firstItem, null, inst))
            {
                inst = RentSlow();
            }
            return inst;
        }

        public void Return(T item)
        {
            if (firstItem == null)
            {
                // Intentionally not using interlocked here. 
                // In a worst case scenario two objects may be stored into same slot.
                // It is very unlikely to happen and will only mean that one of the objects will get collected.
                firstItem = item;
            }
            else
            {
                ReturnSlow(item);
            }
        }

        private T RentSlow()
        {
            for (int i = 0; i < items.Length; i++)
            {
                // Note that the initial read is optimistically not synchronized. That is intentional. 
                // We will interlock only when we have a candidate. in a worst case we may miss some
                // recently returned objects. Not a big deal.
                T inst = items[i];
                if (inst != null)
                {
                    if (inst == Interlocked.CompareExchange(ref items[i], null, inst))
                    {
                        return inst;
                    }
                }
            }

            return generator();
        }

        private void ReturnSlow(T obj)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    // Intentionally not using interlocked here. 
                    // In a worst case scenario two objects may be stored into same slot.
                    // It is very unlikely to happen and will only mean that one of the objects will get collected.
                    items[i] = obj;
                    break;
                }
            }
        }
    }

    public class SampleClass
    {
        public SampleClass()
        {
            Text = "Text";
        }

         public string Text;
    }
}
