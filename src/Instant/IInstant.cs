namespace Radical.Instant
{
    using Uniques;

    public interface IInstant : IUnique
    {
        object this[string propertyName] { get; set; }

        object this[int fieldId] { get; set; }

        object[] ValueArray { get; set; }

        Uscn Code { get; set; }
    }
}
