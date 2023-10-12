namespace Radical.Linking
{
    public interface ILink
    {
        long SourceId { get; set; }

        string SourceType { get; set; }

        long TargetId { get; set; }

        string TargetType { get; set; }
    }
}
