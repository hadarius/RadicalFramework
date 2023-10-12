namespace Radical.Instant
{
    using Radical.Instant.Attributes;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class RubricAttribute : InstantAttribute
    {
        public RubricAttribute()
        {
        }
    }
}
