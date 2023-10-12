namespace Radical.Instant.Rubrics.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RubricAggregateAttribute : RubricAttribute
    {
        public AggregationOperand SummaryOperand = AggregationOperand.None;

        public RubricAggregateAttribute() { }
    }
}
