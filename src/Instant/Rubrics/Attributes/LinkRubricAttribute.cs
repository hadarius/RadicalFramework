namespace Radical.Instant.Rubrics.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class LinkRubricAttribute : RubricAttribute
    {
        public string LinkRubric;
        public string TargetName;
        public Type TargetType;

        public LinkRubricAttribute(string targetName, string linkRubric)
        {
            TargetName = targetName;
            LinkRubric = linkRubric;
        }

        public LinkRubricAttribute(Type targetType, string linkRubric)
        {
            TargetType = targetType;
            TargetName = targetType.Name;
            LinkRubric = linkRubric;
        }
    }
}
