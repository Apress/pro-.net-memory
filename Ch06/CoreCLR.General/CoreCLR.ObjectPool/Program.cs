using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CoreCLR.ObjectPool
{
    class Program
    {
        static void Main(string[] args)
        {
            var objPool1 = new ObjectPool_Trivial<SomeClass>(() => new SomeClass());
            SomeClass obj1 = objPool1.Rent();
            Console.WriteLine(obj1.ToString());
            objPool1.Return(obj1);

            var objPool2 = new ObjectPool_Naive<SomeClass>(() => new SomeClass(), 10);
            SomeClass obj2 = objPool1.Rent();
            Console.WriteLine(obj2.ToString());
            objPool2.Return(obj2);

            ObjectPool<SomeClass> objPool3 = new ObjectPool<SomeClass>(() => new SomeClass(), 10);
            SomeClass obj3 = objPool3.Rent();
            Console.WriteLine(obj3.ToString());
            objPool3.Return(obj3);

            Console.Read();
        }
    }


    // Based on:
    // * https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/how-to-create-an-object-pool
    // * http://source.roslyn.codeplex.com/#Microsoft.CodeAnalysis/ObjectPool%25601.cs,168da5c8839e7ef4
    // * https://www.infoworld.com/article/3221392/application-development/how-to-use-the-object-pool-design-pattern-in-c.html

    // Design decisions:
    // * should be thread-safe
    // * should manage maximum capacity - to not grow working-set without control

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

    ///////////////////////////////////////////////////////////////////////
    // Listing 6-30
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

    public class SomeClass
    {
        public string Text;
    }
}
