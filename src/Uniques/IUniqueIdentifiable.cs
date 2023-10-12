using System.Collections.Specialized;

namespace Radical.Uniques
{
    using Origins;
    using Instant;
    using Radical.Instant.Proxies;

    public interface IUniqueIdentifiable : IUniqueObject, IEquatable<IUniqueIdentifiable>, IComparable<IUniqueIdentifiable>, IEquatable<BitVector32>,
                                     IEquatable<DateTime>, IEquatable<IUniqueStructure>, IOrigin, IInnerProxy
    {
        bool Obsolete { get; set; }
        byte Priority { get; set; }
        bool Inactive { get; set; }
        bool Locked { get; set; }

        byte Flags { get; set; }

        DateTime Time { get; set; }

        void SetFlag(ushort position);
        void ClearFlag(ushort position);
        void SetFlag(bool flag, ushort position);
        bool GetFlag(ushort position);

        long SetId(long id);
        long SetId(object id);

        IUniqueIdentifiable Sign(object id);
        IUniqueIdentifiable Sign();
        IUniqueIdentifiable Stamp();

        TEntity Sign<TEntity>(object id) where TEntity : class, IUniqueIdentifiable;
        TEntity Sign<TEntity>() where TEntity : class, IUniqueIdentifiable;
        TEntity Stamp<TEntity>() where TEntity : class, IUniqueIdentifiable;

        TEntity Sign<TEntity>(TEntity entity, object id) where TEntity : class, IUniqueIdentifiable;
        TEntity Sign<TEntity>(TEntity entity) where TEntity : class, IUniqueIdentifiable;
        TEntity Stamp<TEntity>(TEntity entity) where TEntity : class, IUniqueIdentifiable;

        byte GetPriority();
        byte SetPriority(byte priority);
        byte ComparePriority(IUniqueIdentifiable entity);
    }
}