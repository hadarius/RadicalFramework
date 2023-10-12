namespace Radical.Uniques
{
    public interface IUniqueObject : IUniqueCode
    {
        long Id { get; set; }

        int Ordinal { get; set; }

        string Label { get; set; }

        int OriginKey { get; set; }

        string OriginName { get; set; }

        long SourceId { get; set; }

        string SourceType { get; set; }

        long TargetId { get; set; }

        string TargetType { get; set; }

        DateTime Modified { get; set; }

        string Modifier { get; set; }

        DateTime Created { get; set; }

        string Creator { get; set; }

        string CodeNumber { get; set; }

        long AutoId();

    }
}
