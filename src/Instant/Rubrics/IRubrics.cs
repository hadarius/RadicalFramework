namespace Radical.Instant.Rubrics
{
    using Radical.Series;
    using Series;
    using Uniques;

    public interface IRubrics : IUnique, ISeries<MemberRubric>
    {
        int BinarySize { get; }

        int[] BinarySizes { get; }

        IInstantSeries Figures { get; set; }

        IRubrics KeyRubrics { get; set; }

        RubricSqlMappings Mappings { get; set; }

        int[] Ordinals { get; }

        byte[] GetBytes(IInstant figure);

        byte[] GetUniqueBytes(IInstant figure, uint seed = 0);

        ulong GetUniqueKey(IInstant figure, uint seed = 0);

        void SetUniqueKey(IInstant figure, uint seed = 0);

        void Update();
    }
}
