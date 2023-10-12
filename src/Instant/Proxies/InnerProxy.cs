using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Radical.Uniques;

namespace Radical.Instant.Proxies;

using Rubrics;
using Origins;

[DataContract]
[StructLayout(LayoutKind.Sequential)]
public abstract class InnerProxy : Origin, IInnerProxy
{
    [JsonIgnore]
    [IgnoreDataMember]
    protected IProxy proxy;

    public virtual object this[string propertyName]
    {
        get { return proxy[propertyName]; }
        set { proxy[propertyName] = value; }
    }
    public virtual object this[int id]
    {
        get { return proxy[id]; }
        set { proxy[id] = value; }
    }

    [JsonIgnore]
    [IgnoreDataMember]
    object[] IInstant.ValueArray
    {
        get => proxy.ValueArray;
        set => proxy.ValueArray = value;
    }

    [JsonIgnore]
    [IgnoreDataMember]
    Uscn IInstant.Code
    {
        get => proxy.Code;
        set => proxy.Code = value;
    }

    [JsonIgnore]
    [IgnoreDataMember]
    IRubrics IInnerProxy.Rubrics => proxy.Rubrics;

    [JsonIgnore]
    [IgnoreDataMember]
    IProxy IInnerProxy.Proxy
    {
        get => proxy;
        set => proxy = value;
    }

    [JsonIgnore]
    [IgnoreDataMember]
    public virtual ulong Key
    {
        get => proxy.Key;
        set => proxy.Key = value;
    }

    [JsonIgnore]
    [IgnoreDataMember]
    public virtual ulong TypeKey
    {
        get => proxy.TypeKey;
        set => proxy.TypeKey = value;
    }

    protected virtual void CreateProxy(Action<IInnerProxy> compileAction)
    {
        compileAction.Invoke(this);
    }

    protected virtual void CreateProxy()
    {
        proxy = ProxyFactory.GetCreator(this.GetType()).Create(this);
    }

    public virtual bool Equals(IUnique? other)
    {
        return proxy.Equals(other);
    }

    public virtual int CompareTo(IUnique? other)
    {
        return proxy.CompareTo(other);
    }

    public virtual byte[] GetBytes()
    {
        return proxy.GetBytes();
    }

    public virtual byte[] GetKeyBytes()
    {
        return proxy.GetKeyBytes();
    }
}
