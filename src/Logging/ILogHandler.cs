namespace Radical.Logging
{
    public interface ILogHandler
    {
        bool Clean(DateTime olderThen);

        void Write(Starlog log);
    }
}
