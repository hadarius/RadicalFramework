using Radical.Extracting;
using Radical.Logging;
using Radical.Uniques;
using System.Runtime.InteropServices;

namespace Radical.Series;

using Base;
using Invoking;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public class CacheSeriesItem<V> : SeriesItemBase<V>
{
    private ulong _key;
    private TimeSpan duration;
    private DateTime expiration;
    private IInvoker callback;

    public void SetupExpiration(TimeSpan? lifetime, IInvoker callback = null)
    {
        duration = (lifetime != null) ? lifetime.Value : TimeSpan.FromMinutes(15);
        expiration = Log.Clock + duration;
        this.callback = callback;
    }

    private void setupExpiration()
    {
        expiration = Log.Clock + duration;
    }

    public CacheSeriesItem() : base()
    {
        SetupExpiration(TimeSpan.FromMinutes(15));
    }

    public CacheSeriesItem(ISeriesItem<V> value, TimeSpan? lifeTime = null, IInvoker deputy = null)
        : base(value)
    {
        SetupExpiration(lifeTime, deputy);
    }

    public CacheSeriesItem(object key, V value, TimeSpan? lifeTime = null, IInvoker deputy = null)
        : base(key, value)
    {
        SetupExpiration(lifeTime, deputy);
    }

    public CacheSeriesItem(ulong key, V value, TimeSpan? lifeTime = null, IInvoker deputy = null)
        : base(key, value)
    {
        SetupExpiration(lifeTime, deputy);
    }

    public CacheSeriesItem(V value, TimeSpan? lifeTime = null, IInvoker deputy = null) : base(value)
    {
        SetupExpiration(lifeTime, deputy);
    }

    public override ulong Key
    {
        get { return _key; }
        set { _key = value; }
    }

    public override V Value
    {
        get
        {
            if (Log.Clock > expiration)
            {
                Removed = true;
                if (callback != null)
                    _ = callback.InvokeAsync(value);
                return default(V);
            }
            return value;
        }
        set
        {
            setupExpiration();
            this.value = value;
        }
    }

    public override V UniqueObject
    {
        get => this.Value;
        set => this.Value = value;
    }

    public override int CompareTo(ISeriesItem<V> other)
    {
        return (int)(Key - other.Key);
    }

    public override int CompareTo(object other)
    {
        return (int)(Key - other.UniqueKey64(TypeKey));
    }

    public override int CompareTo(ulong key)
    {
        return (int)(Key - key);
    }

    public override bool Equals(object y)
    {
        return Key.Equals(y.UniqueKey64(TypeKey));
    }

    public override bool Equals(ulong key)
    {
        return Key == key;
    }

    public override byte[] GetBytes()
    {
        return this.value.GetBytes();
    }

    public override int GetHashCode()
    {
        return (int)Key.UniqueKey32();
    }

    public unsafe override byte[] GetKeyBytes()
    {
        byte[] b = new byte[8];
        fixed (byte* s = b)
            *(ulong*)s = _key;
        return b;
    }

    public override void Set(ISeriesItem<V> item)
    {
        Value = item.Value;
        _key = item.Key;
    }

    public override void Set(object key, V value)
    {
        this.Value = value;
        _key = key.UniqueKey64(TypeKey);
    }

    public override void Set(V value)
    {
        this.Value = value;
        if (this.value is IUnique<V>)
            _key = ((IUnique<V>)value).CompactKey();
    }
}
