using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Radical.Uniques;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace Radical.Uniques;

using Uniques;
using Instant.Proxies;
using Instant.Attributes;
using Instant.Rubrics;
using Instant.Rubrics.Attributes;
using Radical.Extracting;

[DataContract]
[StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Ansi)]
public class UniqueIdentifiable : InnerProxy, IUniqueIdentifiable
{
    private Uscn code;
    private int[] keyOrdinals;

    public UniqueIdentifiable() : this(true) { }

    public UniqueIdentifiable(bool autoId)
    {
        if (!autoId)
            return;

        code.Key = Unique.NewKey;
        IsNewKey = true;
    }

    public UniqueIdentifiable(object id) : this(id.UniqueKey64()) { }

    public UniqueIdentifiable(ulong id) : this()
    {
        code.Key = id;
    }

    [NotMapped]
    [JsonIgnore]
    [IgnoreDataMember]
    [InstantAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public Uscn Code
    {
        get => code;
        set => code = value;
    }

    [KeyRubric]
    [DataMember(Order = 1)]
    [Column(Order = 1)]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public virtual long Id
    {
        get => (long)Key;
        set => Key = (ulong)value;
    }

    [DataMember(Order = 2)]
    [Column(Order = 2)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual int Ordinal { get; set; }

    [Column(Order = 3)]
    [StringLength(32)]
    [DataMember(Order = 3)]
    [InstantAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public virtual string Label { get; set; }

    [Column(Order = 4)]
    [DataMember(Order = 4)]
    public virtual long SourceId { get; set; }

    [Column(Order = 5)]
    [StringLength(128)]
    [DataMember(Order = 5)]
    [InstantAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public virtual string SourceType { get; set; }

    [Column(Order = 6)]
    [DataMember(Order = 6)]
    public virtual long TargetId { get; set; }

    [Column(Order = 7)]
    [StringLength(128)]
    [DataMember(Order = 7)]
    [InstantAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public virtual string TargetType { get; set; }

    [Required]
    [IdentityRubric]
    [StringLength(32)]
    [ConcurrencyCheck]
    [DataMember(Order = 199)]
    [Column(Order = 199)]
    [InstantAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public virtual string CodeNumber
    {
        get => code;
        set => code.FromTetrahex(value.ToCharArray());
    }


    [NotMapped]
    [JsonIgnore]
    [IgnoreDataMember]
    private bool IsNewKey { get; set; }

    [NotMapped]
    [JsonIgnore]
    [IgnoreDataMember]
    public virtual byte Flags
    {
        get => (byte)code.GetFlags();
        set => code.SetFlagBits(new BitVector32(value));
    }

    [NotMapped]
    [JsonIgnore]
    [IgnoreDataMember]
    public virtual bool Inactive
    {
        get => GetFlag(1);
        set => SetFlag(value, 1);
    }

    [NotMapped]
    [JsonIgnore]
    [IgnoreDataMember]
    public virtual bool Locked
    {
        get => GetFlag(0);
        set => SetFlag(value, 0);
    }

    [NotMapped]
    [JsonIgnore]
    [IgnoreDataMember]
    public override int OriginKey
    {
        get { return (int)code.OriginKey; }
        set { code.OriginKey = (uint)value; }
    }

    [NotMapped]
    [JsonIgnore]
    [IgnoreDataMember]
    public virtual bool Obsolete
    {
        get => GetFlag(2);
        set => SetFlag(value, 2);
    }

    [NotMapped]
    [JsonIgnore]
    [IgnoreDataMember]
    public virtual byte Priority
    {
        get => GetPriority();
        set => SetPriority(value);
    }

    [NotMapped]
    [JsonIgnore]
    [IgnoreDataMember]
    public virtual DateTime Time
    {
        get => DateTime.FromBinary(code.Time);
        set => code.Time = value.ToBinary();
    }

    [NotMapped]
    [JsonIgnore]
    [IgnoreDataMember]
    public override ulong Key
    {
        get => code.Key;
        set
        {
            if (value != 0 && !code.Equals(value) && IsNewKey)
            {
                code.Key = value;
                IsNewKey = false;
            }
        }
    }

    [NotMapped]
    [JsonIgnore]
    [IgnoreDataMember]
    public override ulong TypeKey
    {
        get =>
            code.TypeKey == 0
                ? (code.TypeKey = this.GetType().UniqueKey32())
                : code.TypeKey;
        set
        {
            if (value != 0 && value != code.TypeKey)
                code.TypeKey = this.GetType().UniqueKey32();
        }
    }

    public long AutoId()
    {
        ulong key = code.Key;
        if (key != 0)
            return (long)key;

        ulong id = Unique.NewKey;
        code.Key = id;
        return (long)id;
    }

    public void ClearFlag(ushort position)
    {
        code.ClearFlagBit(position);
    }

    public virtual ulong CompactKey()
    {
        return UniqueValues().UniqueKey64();
    }

    public byte ComparePriority(IUniqueIdentifiable entity)
    {
        return code.ComparePriority(entity.GetPriority());
    }

    public int CompareTo(IUniqueIdentifiable other)
    {
        return code.CompareTo(other);
    }

    public override int CompareTo(IUnique other)
    {
        return code.CompareTo(other);
    }

    public bool Equals(BitVector32 other)
    {
        return ((IEquatable<BitVector32>)code).Equals(other);
    }

    public bool Equals(DateTime other)
    {
        return ((IEquatable<DateTime>)code).Equals(other);
    }

    public bool Equals(IUniqueIdentifiable other)
    {
        return code.Equals(other);
    }

    public bool Equals(IUniqueStructure other)
    {
        return ((IEquatable<IUniqueStructure>)code).Equals(other);
    }

    public override bool Equals(IUnique other)
    {
        return code.Equals(other);
    }

    public override byte[] GetBytes()
    {
        return this.GetStructureBytes();
    }

    public bool GetFlag(ushort position)
    {
        return code.GetFlagBit(position);
    }

    public byte GetPriority()
    {
        return code.GetPriority();
    }

    public override byte[] GetKeyBytes()
    {
        return code.GetKeyBytes();
    }

    public void SetFlag(ushort position)
    {
        code.SetFlagBit(position);
    }

    public void SetFlag(bool flag, ushort position)
    {
        code.SetFlag(flag, position);
    }

    public long SetId(object id)
    {
        if (id == null)
            return AutoId();
        else if (id.GetType().IsPrimitive)
            return SetId((long)id);
        else
            return SetId((long)id.UniqueKey64());
    }

    public long SetId(long id)
    {
        ulong ulongid = (ulong)id;
        ulong key = code.Key;
        if (ulongid != 0 && key != ulongid)
            return (long)(Key = ulongid);
        return AutoId();
    }

    public byte SetPriority(byte priority)
    {
        return code.SetPriority(priority);
    }

    public IUniqueIdentifiable Sign()
    {
        return Sign(this);
    }

    public TEntity Sign<TEntity>() where TEntity : class, IUniqueIdentifiable
    {
        return Sign((TEntity)(object)this);
    }

    public TEntity Sign<TEntity>(TEntity entity) where TEntity : class, IUniqueIdentifiable
    {
        entity.AutoId();
        Stamp(entity);
        Created = Time;
        return entity;
    }

    public IUniqueIdentifiable Sign(object id)
    {
        return Sign(this, id);
    }

    public TEntity Sign<TEntity>(object id) where TEntity : class, IUniqueIdentifiable
    {
        return Sign((TEntity)(object)this);
    }

    public TEntity Sign<TEntity>(TEntity entity, object id) where TEntity : class, IUniqueIdentifiable
    {
        entity.SetId(id);
        Stamp(entity);
        Created = Time;
        return entity;
    }

    public IUniqueIdentifiable Stamp()
    {
        return Stamp(this);
    }

    public TEntity Stamp<TEntity>() where TEntity : class, IUniqueIdentifiable
    {
        return Stamp((TEntity)(object)this);
    }

    public TEntity Stamp<TEntity>(TEntity entity) where TEntity : class, IUniqueIdentifiable
    {
        //if (!entity.IsGUID)
        entity.Time = DateTime.Now;
        return entity;
    }

    public virtual int[] UniqueOrdinals()
    {
        if (keyOrdinals == null)
        {
            IRubrics pks = ((IInnerProxy)this).Proxy.Rubrics.KeyRubrics;
            if (pks.Any())
            {
                keyOrdinals = pks.Select(p => p.RubricId).ToArray();
            }
        }
        return keyOrdinals;
    }

    public virtual object[] UniqueValues()
    {
        int[] ids = keyOrdinals;
        if (ids == null)
            ids = UniqueOrdinals();
        return ids.Select(k => this[k]).ToArray();
    }


}
