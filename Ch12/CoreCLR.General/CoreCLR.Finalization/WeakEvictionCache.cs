using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCLR.Finalization
{
    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-41
    public class WeakEvictionCache<TKey, TValue> where TValue : class
    {
        private readonly TimeSpan weakEvictionThreshold;
        private Dictionary<TKey, StrongToWeakReference<TValue>> items;

        WeakEvictionCache(TimeSpan weakEvictionThreshold)
        {
            this.weakEvictionThreshold = weakEvictionThreshold;
            this.items = new Dictionary<TKey, StrongToWeakReference<TValue>>();
        }

        public void Add(TKey key, TValue value)
        {
            items.Add(key, new StrongToWeakReference<TValue>(value));
        }

        public bool TryGet(TKey key, out TValue result)
        {
            result = null;
            if (items.TryGetValue(key, out var value))
            {
                result = value.Target;
                if (result != null)
                {
                    // Item is used, make it strong again
                    value.MakeStrong();
                    return true;
                }
            }
            return false;
        }

        public void DoWeakEviction()
        {
            List<TKey> toRemove = new List<TKey>();
            foreach (var strongToWeakReference in items)
            {
                var reference = strongToWeakReference.Value;
                var target = reference.Target;
                if (target != null)
                {
                    if (DateTime.Now.Subtract(reference.StrongTime) 
                        >= weakEvictionThreshold)
                    {
                        reference.MakeWeak();
                    }
                }
                else
                {
                    // Remove already zeroed weak references
                    toRemove.Add(strongToWeakReference.Key);
                }
            }

            foreach (var key in toRemove)
            {
                items.Remove(key);
            }
        }

        ///////////////////////////////////////////////////////////////////////////
        // Listing 12-40
        sealed class StrongToWeakReference<T> : WeakReference where T : class
        {
            private DateTime _strongTime;
            private T _strongRef;

            public StrongToWeakReference(T obj) : base(obj)
            {
                _strongTime = DateTime.Now;
                _strongRef = obj;
            }

            public void MakeWeak() => _strongRef = null;

            public void MakeStrong()
            {
                _strongTime = DateTime.Now;
                _strongRef = WeakTarget;
            }

            public DateTime StrongTime => _strongTime;
            public new T Target => _strongRef ?? WeakTarget;
            private T WeakTarget => base.Target as T;
        }
    }
}
