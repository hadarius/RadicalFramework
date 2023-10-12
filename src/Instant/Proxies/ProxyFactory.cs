namespace Radical.Instant.Proxies;

using Radical.Series;
using Uniques;

public static class ProxyFactory
{
    public static ISeries<ProxyCreator> Cache = new Registry<ProxyCreator>();

    public static ProxyCreator GetCreator<T>()
    {
        return GetCreator(typeof(T));
    }

    public static ProxyCreator GetCreator(Type type)
    {
        return GetCreator(type, type.UniqueKey32());
    }

    public static ProxyCreator GetCreator(Type type, uint key)
    {
        if (!Cache.TryGet(key, out ProxyCreator sleeve))
        {
            Cache.Add(key, sleeve = new ProxyCreator(type));
        }
        return sleeve;
    }

    public static ProxyCreator GetCompiledCreator<T>()
    {
        var sleeve = GetCreator<T>();
        sleeve.Create();
        return sleeve;
    }

    public static ProxyCreator GetCompiledCreator(Type type)
    {
        var sleeve = GetCreator(type);
        sleeve.Create();
        return sleeve;
    }

    public static ProxyCreator GetCompiledCreator(object item)
    {
        var sleeve = item.GetProxyCreator();
        sleeve.Create(item);
        return sleeve;
    }

    public static ProxyCreator GetCompiledCreator<T>(T item)
    {
        var sleeve = item.GetProxyCreator<T>();
        sleeve.Create(item);
        return sleeve;
    }

    public static IProxy Create(object item)
    {
        var t = item.GetType();
        if (t.IsAssignableTo(typeof(IProxy)))
            return (IProxy)item;
        else if (t.IsAssignableTo(typeof(IInnerProxy)))
            return ((IInnerProxy)item).Proxy;

        var key = t.UniqueKey32();
        if (!Cache.TryGet(key, out ProxyCreator sleeve))
            Cache.Add(key, sleeve = new ProxyCreator(t));

        return sleeve.Create(item);
    }

    public static IProxy Create<T>(T item)
    {
        var t = typeof(T);
        if (t.IsAssignableTo(typeof(IProxy)))
            return (IProxy)item;
        else if (t.IsAssignableTo(typeof(IInnerProxy)))
            return ((IInnerProxy)item).Proxy;

        var key = t.UniqueKey32();
        if (!Cache.TryGet(key, out ProxyCreator sleeve))
            Cache.Add(key, sleeve = new ProxyCreator<T>());

        return sleeve.Create(item);
    }
}  
