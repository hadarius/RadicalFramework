using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Radical.Uniques;

namespace Radical.Instant.Proxies;

public class Proxy<T> : Proxy
{
    protected new T target { get; set; }

    public Proxy(T target)
    {
        this.target = target;
    }

    public Proxy(T target, Action<IInnerProxy, T> compilationAction)
    {
        this.target = target;
        CreateProxy(compilationAction);
    }

    protected virtual void CreateProxy(Action<IInnerProxy, T> compilationAction)
    {
        compilationAction.Invoke(this, target);
    }

    protected override void CreateProxy()
    {
        proxy = ProxyFactory.GetCreator<T>().Create(target);
    }
}

public class Proxy : InnerProxy
{
    protected virtual object target { get; set; }

    public Proxy() { }

    public Proxy(object target) 
    {
        this.target = target;
        CreateProxy(); 
    }

    public Proxy(object target, Action<IInnerProxy> compilationAction)
    {
        this.target = target;
        CreateProxy(compilationAction);
    }

    protected override void CreateProxy(Action<IInnerProxy> compilationAction)
    {
        compilationAction.Invoke(this);
    }

    protected override void CreateProxy()
    {
        proxy = ProxyFactory.GetCreator(target.GetType()).Create(target);
    }
}
