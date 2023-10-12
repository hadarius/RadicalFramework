using System.Collections.Generic;
using Radical.Instant;
using System.Runtime.CompilerServices;
using System.Threading;
using Radical.Uniques;

namespace Radical.Series;

using Base;
using Invoking;
using Instant.Proxies;
using Instant.Rubrics;

public class TypedCache<V> : BaseTypedRegistry<V> where V : IUnique
{
    private readonly Catalog<Timer> timers = new Catalog<Timer>();

    private TimeSpan duration;
    private IInvoker callback;

    private void setupExpiration(TimeSpan? lifetime, IInvoker callback)
    {
        duration = (lifetime != null) ? lifetime.Value : TimeSpan.FromMinutes(15);
        if (callback != null)
            this.callback = callback;
    }

    public TypedCache(
        IEnumerable<IUnique<V>> collection,
        TimeSpan? lifeTime = null,
        IInvoker callback = null,
        int capacity = 17
    ) : base(collection, capacity)
    {
        setupExpiration(lifeTime, callback);
    }

    public TypedCache(
        IEnumerable<V> collection,
        TimeSpan? lifeTime = null,
        IInvoker callback = null,
        int capacity = 17
    ) : base(collection, capacity)
    {
        setupExpiration(lifeTime, callback);
    }

    public TypedCache(
        IList<IUnique<V>> collection,
        TimeSpan? lifeTime = null,
        IInvoker callback = null,
        int capacity = 17
    ) : base(collection, capacity)
    {
        setupExpiration(lifeTime, callback);
    }

    public TypedCache(
        IList<V> collection,
        TimeSpan? lifeTime = null,
        IInvoker callback = null,
        int capacity = 17
    ) : base(collection, capacity)
    {
        setupExpiration(lifeTime, callback);
    }

    public TypedCache(TimeSpan? lifeTime = null, IInvoker callback = null, int capacity = 17)
        : base(capacity)
    {
        setupExpiration(lifeTime, callback);
    }

    public override ISeriesItem<V> EmptyItem()
    {
        return new CacheSeriesItem<V>();
    }

    public override ISeriesItem<V>[] EmptyTable(int size)
    {
        return new CacheSeriesItem<V>[size];
    }

    public override ISeriesItem<V>[] EmptyVector(int size)
    {
        return new CacheSeriesItem<V>[size];
    }

    public override ISeriesItem<V> NewItem(ISeriesItem<V> item)
    {
        return new CacheSeriesItem<V>(item, duration, callback);
    }

    public override ISeriesItem<V> NewItem(object key, V value)
    {
        return new CacheSeriesItem<V>(key, value, duration, callback);
    }

    public override ISeriesItem<V> NewItem(ulong key, V value)
    {
        return new CacheSeriesItem<V>(key, value, duration, callback);
    }

    public override ISeriesItem<V> NewItem(V value)
    {
        return new CacheSeriesItem<V>(value, duration, callback);
    }

    protected virtual ITypedSeries<IUnique> cache { get; set; }

    protected virtual T InnerMemorize<T>(T item) where T : IUnique
    {
        uint group = GetValidTypeKey(typeof(T));
        if (!cache.TryGet(group, out IUnique catalog))
        {
            ProxyCreator sleeve = ProxyFactory.GetCreator(GetValidType(typeof(T)), group);
            sleeve.Create();

            IRubrics keyrubrics = sleeve.Rubrics.KeyRubrics;

            IProxy isleeve = item.ToProxy();

            catalog = new TypedRegistry<IUnique>();

            foreach (MemberRubric keyRubric in keyrubrics)
            {
                Registry<IUnique> subcatalog = new Registry<IUnique>();

                subcatalog.Add(item);

                ((ITypedSeries<IUnique>)catalog).Put(
                    isleeve[keyRubric.RubricId],
                    keyRubric.RubricName.UniqueKey32(),
                    subcatalog);
            }

            cache.Add(group, catalog);

            cache.Add(item);

            return item;
        }

        if (!cache.ContainsKey(item))
        {
            ITypedSeries<IUnique> _catalog = (ITypedSeries<IUnique>)catalog;

            IProxy isleeve = item.ToProxy();

            foreach (MemberRubric keyRubric in isleeve.Rubrics.KeyRubrics)
            {
                if (!_catalog.TryGet(
                    isleeve[keyRubric.RubricId],
                    keyRubric.RubricName.UniqueKey32(),
                    out IUnique outcatalog))
                {
                    outcatalog = new Registry<IUnique>();

                    ((ISeries<IUnique>)outcatalog).Put(item);

                    _catalog.Put(isleeve[keyRubric.RubricId], keyRubric.RubricName.UniqueKey32(), outcatalog);
                }
                else
                {
                    ((ISeries<IUnique>)outcatalog).Put(item);
                }
            }
            cache.Add(item);
        }

        return item;
    }

    protected virtual T InnerMemorize<T>(T item, params string[] names) where T : IUnique
    {
        Memorize(item);

        IProxy sleeve = item.ToProxy();

        MemberRubric[] keyrubrics = sleeve.Rubrics.Where(p => names.Contains(p.RubricName)).ToArray();

        ITypedSeries<IUnique> _catalog = (ITypedSeries<IUnique>)cache.Get(item.TypeKey);

        foreach (MemberRubric keyRubric in keyrubrics)
        {
            if (!_catalog.TryGet(sleeve[keyRubric.RubricId], keyRubric.RubricName.UniqueKey32(), out IUnique outcatalog))
            {
                outcatalog = new Registry<IUnique>();

                ((ISeries<IUnique>)outcatalog).Put(item);

                _catalog.Put(sleeve[keyRubric.RubricId], keyRubric.RubricName.UniqueKey32(), outcatalog);
            }
            else
            {
                ((ISeries<IUnique>)outcatalog).Put(item);
            }
        }

        return item;
    }      

    public virtual ITypedSeries<IUnique> CacheSet<T>() where T : IUnique
    {
        if (cache.TryGet(GetValidTypeKey(typeof(T)), out IUnique catalog))
            return (ITypedSeries<IUnique>)catalog;
        return null;
    }

    public virtual T Lookup<T>(object keys) where T : IUnique
    {
        if (cache.TryGet(keys, GetValidTypeKey(typeof(T)), out IUnique output))
            return (T)output;
        return default;
    }

    public virtual ISeries<IUnique> Lookup<T>(Tuple<string, object> valueNamePair) where T : IUnique
    { return Lookup<T>((m) => (ISeries<IUnique>)m.Get(valueNamePair.Item2, valueNamePair.Item2.UniqueKey32())); }

    public virtual ISeries<IUnique> Lookup<T>(Func<ITypedSeries<IUnique>, ISeries<IUnique>> selector) where T : IUnique
    { return selector(CacheSet<T>()); }

    public virtual T Lookup<T>(T item) where T : IUnique
    {
        IProxy shell = item.ToProxy();
        IRubrics mrs = shell.Rubrics.KeyRubrics;
        T[] result = new T[mrs.Count];
        int i = 0;
        if (cache.TryGet(GetValidTypeKey(typeof(T)), out IUnique catalog))
        {
            foreach (MemberRubric mr in mrs)
            {
                if (((ITypedSeries<IUnique>)catalog).TryGet(
                    shell[mr.RubricId],
                    mr.RubricName.UniqueKey32(),
                    out IUnique outcatalog))
                    if (((ISeries<IUnique>)outcatalog).TryGet(item, out IUnique output))
                        result[i++] = (T)output;
            }
        }

        if (result.Any(r => r == null))
            return default;
        return result[0];
    }

    public virtual T[] Lookup<T>(object key, params Tuple<string, object>[] valueNamePairs) where T : IUnique
    {
        return Lookup<T>(
            (k) => k[key],
            valueNamePairs.ForEach(
                (vnp) => new Func<ITypedSeries<IUnique>, ISeries<IUnique>>(
                    (m) => (ISeries<IUnique>)m
                                                                    .Get(vnp.Item2, vnp.Item1.UniqueKey32())))
                .ToArray());
    }

    public virtual T[] Lookup<T>(
        Func<ISeries<IUnique>, IUnique> key,
        params Func<ITypedSeries<IUnique>, ISeries<IUnique>>[] selectors)
        where T : IUnique
    {
        if (cache.TryGet(GetValidTypeKey(typeof(T)), out IUnique catalog))
        {
            T[] result = new T[selectors.Length];
            for (int i = 0; i < selectors.Length; i++)
            {
                result[i] = (T)key(selectors[i]((ITypedSeries<IUnique>)catalog));
            }
            return result;
        }

        return default;
    }

    public virtual ISeries<IUnique> Lookup<T>(object key, string propertyNames) where T : IUnique
    {
        if (CacheSet<T>().TryGet(key, propertyNames.UniqueKey32(), out IUnique outcatalog))
            return (ISeries<IUnique>)outcatalog;
        return default;
    }

    public virtual T Lookup<T>(T item, params string[] propertyNames) where T : IUnique
    {
        IProxy ilValuator = item.ToProxy();
        MemberRubric[] mrs = ilValuator.Rubrics.Where(p => propertyNames.Contains(p.RubricName)).ToArray();
        T[] result = new T[mrs.Length];

        if (cache.TryGet(GetValidTypeKey(typeof(T)), out IUnique catalog))
        {
            int i = 0;
            foreach (MemberRubric mr in mrs)
            {
                if (((ITypedSeries<IUnique>)catalog).TryGet(
                    ilValuator[mr.RubricId],
                    mr.RubricName.UniqueKey32(),
                    out IUnique outcatalog))
                    if (((ISeries<IUnique>)outcatalog).TryGet(item, out IUnique output))
                        result[i++] = (T)output;
            }
        }

        if (result.Any(r => r == null))
            return default;
        return result[0];
    }

    public virtual IEnumerable<T> Memorize<T>(IEnumerable<T> items) where T : IUnique
    { return items.ForEach(p => Memorize(p)); }

    public virtual T Memorize<T>(T item) where T : IUnique
    {
        return InnerMemorize(item);
    }

    public virtual T Memorize<T>(T item, params string[] names) where T : IUnique
    {
        if (InnerMemorize(item) != null)
            return InnerMemorize(item, names);
        return default(T);
    }

    public virtual async Task<T> MemorizeAsync<T>(T item) where T : IUnique
    { return await Task.Run(() => Memorize(item)); }
    public virtual async Task<T> MemorizeAsync<T>(T item, params string[] names) where T : IUnique
    { return await Task.Run(() => Memorize(item, names)); }

    public virtual ITypedSeries<IUnique> Catalog => cache;

    public virtual Type GetValidType(object obj)
    {
        return obj.GetType();
    }
    public virtual Type GetValidType(Type obj)
    {
        return obj;
    }
    public virtual uint GetValidTypeKey(object obj)
    {
        return obj.GetType().UniqueKey32();
    }

    public virtual uint GetValidTypeKey(Type obj)
    {
        return obj.UniqueKey32();
    }

}
