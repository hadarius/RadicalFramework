using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Radical.Invoking.Work
{
    using Uniques;
    using Series;

    public static class WorkSpot
    {
        private static Registry<IntPtr> spots;

        static WorkSpot()
        {
            spots = new Registry<IntPtr>(true, UniquePrimes.Get(7));
        }

        public static IEnumerable<T> GetRange<T>(long id)
        {
            if (spots.TryGet(id, out ISeriesItem<IntPtr> ptrs))
                return ptrs.ForEach(p => Target<T>(p));
            return default;
        }

        public static IEnumerable<T> GetRange<T>()
        {
            if (spots.TryGet(ThreadId, out ISeriesItem<IntPtr> ptrs))
                return ptrs.ForEach(p => Target<T>(p));
            return default;
        }

        public static IEnumerable<object> GetRange()
        {
            if (spots.TryGet(ThreadId, out ISeriesItem<IntPtr> ptrs))
                return ptrs.ForEach(p => Target(p));
            return default;
        }

        public static IEnumerable<T> GetTypedRange<T>(long id)
        {
            if (spots.TryGet(typeof(T).FullName.UniqueKey64((uint)id), out ISeriesItem<IntPtr> ptrs))
                return ptrs.ForEach(p => Target<T>(p));
            return default;
        }

        public static IEnumerable<T> GetTypedRange<T>()
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            var key = typeof(T).FullName.UniqueKey64((uint)id);
            if (spots.TryGet(key, out ISeriesItem<IntPtr> ptrs))
                return ptrs.ForEach(p => Target<T>(p));
            return default;
        }

        public static IEnumerable<object> GetTypedRange(Type type)
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            var key = type.FullName.UniqueKey64((uint)id);
            if (spots.TryGet(key, out ISeriesItem<IntPtr> ptrs))
                return ptrs.ForEach(p => Target(p));
            return default;
        }

        public static T Get<T>(long id)
        {
            if (spots.TryGet(id, out IntPtr ptr))
                return Target<T>(ptr);
            return default;
        }

        public static T Get<T>()
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            if (spots.TryGet(id, out IntPtr ptr))
                return Target<T>(ptr);
            return default;
        }

        public static object Get()
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            if (spots.TryGet(id, out IntPtr ptr))
                return Target(ptr);
            return default;
        }

        public static T GetTyped<T>()
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            var key = typeof(T).FullName.UniqueKey64((uint)id);
            if (spots.TryGet(key, out IntPtr ptr))
                return Target<T>(ptr);
            return default;
        }

        public static object GetTyped(Type type)
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            var key = type.FullName.UniqueKey64((uint)id);
            if (spots.TryGet(key, out IntPtr ptr))
                return Target(ptr);
            return default;
        }

        public static T GetTyped<T>(long id)
        {
            if (spots.TryGet(typeof(T).FullName.UniqueKey64((uint)id), out IntPtr ptr))
                return Target<T>(ptr);
            return default;
        }

        public static T RemoveTyped<T>()
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            var key = typeof(T).FullName.UniqueKey64((uint)id);
            return Target<T>(spots.Remove(key));
        }

        public static object RemoveTyped(Type type)
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            var key = type.FullName.UniqueKey64((uint)id);
            return Target(spots.Remove(key));
        }

        public static T Remove<T>()
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            var key = typeof(T).FullName.UniqueKey64((uint)id);
            return Target<T>(spots.Remove(key));
        }

        public static T Remove<T>(long key)
        {
            return Target<T>(spots.Remove(key));
        }

        public static void RemoveRange<T>(IEnumerable<long> keys)
        {
            keys.ForEach(
                (k) =>
                {
                    spots.Remove(k);
                }
            );
        }

        public static long SetTyped<T>(T item)
        {
            var key = (long)typeof(T).FullName.UniqueKey64((uint)ThreadId);
            spots.Put(key, Address(item));
            return key;
        }

        public static long Set<T>(T item)
        {
            var id = ThreadId;
            spots.Put(id, Address(item));
            return id;
        }

        private static void ChangeKey<T>(long key, long newkey)
        {
            spots.Put(newkey, spots.Remove(key));
        }

        public static long Add<T>(T item)
        {
            var id = ThreadId;
            spots.Add(id, Address(item));
            return id;
        }

        public static long Add(object item)
        {
            var id = ThreadId;
            spots.Add(id, Address(item));
            return id;
        }

        public static long Add<T>(T item, long key)
        {
            if (key == 0)
                key = (long)item.UniqueKey();

            spots.Add(key, Address(item));

            return key;
        }

        public static IEnumerable<long> AddRange<T>(IEnumerable<T> items)
        {
            return items.ForEach(p => Add(p));
        }

        public static IEnumerable<long> AddRange(params object[] items)
        {
            return items.ForEach(p => Add(p));
        }

        public static long AddTyped<T>(object item)
        {
            var key = (long)typeof(T).FullName.UniqueKey64((uint)ThreadId);
            spots.Put(key, Address(item));
            return key;
        }

        public static long AddTyped(Type type, object item)
        {
            var key = (long)type.FullName.UniqueKey64((uint)ThreadId);
            spots.Put(key, Address(item));
            return key;
        }

        public static Task<long> AddTypedAsync<T>(T item)
        {
            return Task.Run(() => SetTyped(item));
        }

        public static Task<long> AddAsync<T>(T item)
        {
            return Task.Run(() => Add(item));
        }

        public static Task<long> AddAsync<T>(T item, long id)
        {
            return Task.Run(() => Add(item, id));
        }

        private static IntPtr Address<T>(T item)
        {
            return GCHandle.ToIntPtr(GCHandle.Alloc(item, GCHandleType.Normal));
        }

        private static T Target<T>(IntPtr ptr)
        {
            return (T)GCHandle.FromIntPtr(ptr).Target;
        }

        private static object Target(IntPtr ptr)
        {
            return GCHandle.FromIntPtr(ptr).Target;
        }

        private static int ThreadId => Thread.CurrentThread.ManagedThreadId;
    }
}
