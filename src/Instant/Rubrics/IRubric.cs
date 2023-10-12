namespace Radical.Instant.Rubrics
{
    using Uniques;

    public interface IRubric : IMemberRubric, IUnique
    {
        short IdentityOrder { get; set; }

        bool IsAutoincrement { get; set; }

        bool IsDBNull { get; set; }

        bool IsExpandable { get; set; }

        bool IsIdentity { get; set; }

        bool IsKey { get; set; }

        bool IsUnique { get; set; }

        bool Required { get; set; }

        AggregationOperand AggregationOperand { get; set; }

        int AggregationOrdinal { get; set; }

        IRubric AggregateRubric { get; set; }
    }

    public enum AggregationOperand
    {
        None,
        Sum,
        Avg,
        Min,
        Max,
        Bis
    }
}
