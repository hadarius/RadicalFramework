using System.Reflection;

namespace Radical.Instant.Rubrics
{
    public interface IMemberRubric
    {
        bool Editable { get; set; }

        MemberInfo MemberInfo { get; }

        object[] RubricAttributes { get; set; }

        int RubricId { get; set; }

        string RubricName { get; set; }

        int RubricOffset { get; set; }

        int RubricSize { get; set; }

        Type RubricType { get; set; }

        bool Visible { get; set; }
    }
}
