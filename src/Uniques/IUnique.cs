namespace Radical.Uniques
{
    public interface IUnique : IEquatable<IUnique>, IComparable<IUnique>
    {
        ulong Key { get; set; }

        ulong TypeKey { get; set; }

        byte[] GetBytes();

        byte[] GetKeyBytes();
    }
}
